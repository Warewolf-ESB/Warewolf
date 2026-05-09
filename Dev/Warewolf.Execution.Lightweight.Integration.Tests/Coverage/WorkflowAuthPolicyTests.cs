/*
 * In-process unit tests for classes that had 0% coverage.
 * No Azure Functions host required — these exercise the classes directly.
 *
 * Covers:
 *   WorkflowAuthPolicy / WorkflowGroupEntry
 *   DebugStepResult / ExecutionErrorDetail
 *   PerRequestDebugCapturer (internal, visible via InternalsVisibleTo)
 *   JwtGenerator             (internal, visible via InternalsVisibleTo)
 *   WorkflowOpenApiGenerator (internal, visible via InternalsVisibleTo)
 *   CompositeExecutionLogger
 *   ElasticsearchLogDocument
 *   ElasticsearchLoggingOptions  (FromEnvironment + FromBiteFile)
 */

using Dev2.Common.Interfaces.Diagnostics.Debug;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Warewolf.Execution.Lightweight.Auth.Models;
using Warewolf.Execution.Lightweight.Logging;
using Warewolf.Execution.Lightweight.Models;
using Warewolf.Execution.Lightweight.Security;

namespace Warewolf.Execution.Lightweight.Integration.Tests.Coverage
{
    // ═══════════════════════════════════════════════════════════════════════════
    // WorkflowAuthPolicy + WorkflowGroupEntry
    // ═══════════════════════════════════════════════════════════════════════════

    [TestClass]
    [TestCategory("Unit_Coverage")]
    public class WorkflowAuthPolicyTests
    {
        [TestMethod]
        public void WorkflowGroupEntry_StoresGroupNameAndPermissions()
        {
            var entry = new WorkflowGroupEntry("TeamA", WorkflowPermission.View);
            Assert.AreEqual("TeamA", entry.GroupName);
            Assert.AreEqual(WorkflowPermission.View, entry.Permissions);
        }

        [TestMethod]
        public void WorkflowGroupEntry_RecordEquality_SameValues_AreEqual()
        {
            var a = new WorkflowGroupEntry("G", WorkflowPermission.Execute);
            var b = new WorkflowGroupEntry("G", WorkflowPermission.Execute);
            Assert.AreEqual(a, b);
        }

        [TestMethod]
        public void WorkflowAuthPolicy_Create_BuildsDistinctGroupsCaseInsensitive()
        {
            var entries = new[]
            {
                new WorkflowGroupEntry("TeamA",  WorkflowPermission.View),
                new WorkflowGroupEntry("teama",  WorkflowPermission.Execute),  // same group, different case
                new WorkflowGroupEntry("TeamB",  WorkflowPermission.View),
            };

            var policy = WorkflowAuthPolicy.Create("MyWorkflow", entries, WorkflowPermission.View);

            Assert.AreEqual("MyWorkflow", policy.WorkflowName);
            Assert.AreEqual(WorkflowPermission.View, policy.RequiredPermissions);
            Assert.AreEqual(2, policy.AllowedGroups.Count, "Case-insensitive dedup should yield 2 unique groups");
            Assert.AreEqual(3, policy.GroupEntries.Count, "All three entries should be preserved");
        }

        [TestMethod]
        public void WorkflowAuthPolicy_Create_EmptyEntries_YieldsEmptyPolicy()
        {
            var policy = WorkflowAuthPolicy.Create("WF", Array.Empty<WorkflowGroupEntry>(), WorkflowPermission.View);
            Assert.AreEqual(0, policy.AllowedGroups.Count);
            Assert.AreEqual(0, policy.GroupEntries.Count);
        }

        [TestMethod]
        public void WorkflowAuthPolicy_RecordEquality_SameReference_IsEqual()
        {
            var e = new WorkflowGroupEntry("G", WorkflowPermission.View);
            var a = WorkflowAuthPolicy.Create("WF", new[] { e }, WorkflowPermission.View);
            // Records use structural equality — same lists (same references) → equal
            Assert.AreEqual(a, a);
            Assert.AreEqual("WF", a.WorkflowName);
            Assert.AreEqual(WorkflowPermission.View, a.RequiredPermissions);
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // DebugStepResult + ExecutionErrorDetail
    // ═══════════════════════════════════════════════════════════════════════════

    [TestClass]
    [TestCategory("Unit_Coverage")]
    public class ModelTests
    {
        [TestMethod]
        public void DebugStepResult_PropertiesRoundTrip()
        {
            var id  = Guid.NewGuid();
            var sid = Guid.NewGuid();
            var dsr = new DebugStepResult
            {
                ID                   = id,
                SessionID            = sid,
                DisconnectedID       = Guid.NewGuid(),
                SourceResourceID     = Guid.NewGuid(),
                OriginatingResourceID = Guid.NewGuid(),
                OriginalInstanceID   = id,
                ClientID             = Guid.Empty,
                DisplayName          = "Hello World",
                ActualType           = "DsfActivity",
                HasError             = false,
                StartTime            = DateTime.UtcNow,
                EndTime              = DateTime.UtcNow,
            };

            Assert.AreEqual(id,  dsr.ID);
            Assert.AreEqual(sid, dsr.SessionID);
            Assert.AreEqual("Hello World", dsr.DisplayName);
        }

        [TestMethod]
        public void DebugStepResult_CanSerializeToJson()
        {
            var dsr = new DebugStepResult { ID = Guid.NewGuid(), DisplayName = "Ping" };
            var json = JsonSerializer.Serialize(dsr);
            Assert.IsTrue(json.Contains("Ping"), $"Serialised JSON should contain DisplayName. Got: {json}");
        }

        [TestMethod]
        public void ExecutionErrorDetail_ToLogScope_ContainsAllKeys()
        {
            var id  = Guid.NewGuid();
            var ts  = DateTime.UtcNow;
            var eed = new ExecutionErrorDetail
            {
                ExecutionId  = id,
                ActivityName = "MssqlActivity",
                Message      = "Connection refused",
                StackTrace   = "at line 42",
                Timestamp    = ts,
            };

            var scope = eed.ToLogScope();

            Assert.AreEqual(id,                scope["ExecutionId"]);
            Assert.AreEqual("MssqlActivity",   scope["ActivityName"]);
            Assert.AreEqual("Connection refused", scope["Message"]);
            Assert.AreEqual("at line 42",      scope["StackTrace"]);
            Assert.AreEqual(ts,                scope["Timestamp"]);
        }

        [TestMethod]
        public void ExecutionErrorDetail_ToLogScope_NullStrings_ReturnEmpty()
        {
            var eed = new ExecutionErrorDetail { ExecutionId = Guid.Empty };
            var scope = eed.ToLogScope();

            Assert.AreEqual(string.Empty, scope["ActivityName"]);
            Assert.AreEqual(string.Empty, scope["Message"]);
            Assert.AreEqual(string.Empty, scope["StackTrace"]);
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // PerRequestDebugCapturer  (internal)
    // ═══════════════════════════════════════════════════════════════════════════

    [TestClass]
    [TestCategory("Unit_Coverage")]
    public class PerRequestDebugCapturerTests
    {
        sealed class StubDebugState : IDebugState
        {
            public string DisplayName { get; set; } = "stub";
            public Guid WorkspaceID { get; set; }
            public Guid ID { get; set; }
            public Guid DisconnectedID { get; set; }
            public Guid? ParentID { get; set; }
            public Guid SourceResourceID { get; set; }
            public StateType StateType { get; set; }
            public string Name { get; set; } = "";
            public ActivityType ActivityType { get; set; }
            public string Version { get; set; } = "";
            public bool IsSimulation { get; set; }
            public bool IsAdded { get; set; }
            public bool HasError { get; set; }
            public string ErrorMessage { get; set; } = "";
            public string Server { get; set; } = "";
            public Guid ServerID { get; set; }
            public Guid EnvironmentID { get; set; }
            public Guid ClientID { get; set; }
            public Guid OriginatingResourceID { get; set; }
            public List<IDebugItem> Inputs { get; } = new();
            public List<IDebugItem> Outputs { get; } = new();
            public List<IDebugItem> AssertResultList { get; } = new();
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
            public TimeSpan Duration => EndTime - StartTime;
            public string Message { get; set; } = "";
            public Guid OriginalInstanceID { get; set; }
            public int NumberOfSteps { get; set; }
            public ExecutionOrigin ExecutionOrigin { get; set; }
            public string ExecutionOriginDescription { get; set; } = "";
            public string ExecutingUser { get; set; } = "";
            public string Origin => "";
            public Guid SessionID { get; set; }
            public Guid WorkSurfaceMappingId { get; set; }
            public bool IsDurationVisible { get; set; }
            public string ActualType { get; set; } = "";
            public List<IDebugState> Children { get; set; } = new();
            public bool IsFinalStep() => false;
            public bool IsFirstStep() => false;
            public bool Equals(IDebugState? other) => ReferenceEquals(this, other);
        }

        [TestMethod]
        public void Write_WithNonNullDebugState_AddsToStates()
        {
            var capturer = new PerRequestDebugCapturer();
            var state    = new StubDebugState { DisplayName = "TestActivity" };

            capturer.Write(new WriteArgs { debugState = state });

            Assert.AreEqual(1, capturer.States.Count);
            Assert.AreEqual("TestActivity", ((StubDebugState)capturer.States[0]).DisplayName);
        }

        [TestMethod]
        public void Write_WithNullDebugState_DoesNotAddToStates()
        {
            var capturer = new PerRequestDebugCapturer();
            capturer.Write(new WriteArgs { debugState = null });
            Assert.AreEqual(0, capturer.States.Count);
        }

        [TestMethod]
        public void Write_MultipleStates_AllCaptured()
        {
            var capturer = new PerRequestDebugCapturer();
            capturer.Write(new WriteArgs { debugState = new StubDebugState { DisplayName = "A" } });
            capturer.Write(new WriteArgs { debugState = new StubDebugState { DisplayName = "B" } });
            capturer.Write(new WriteArgs { debugState = new StubDebugState { DisplayName = "C" } });

            Assert.AreEqual(3, capturer.States.Count);
        }

        [TestMethod]
        public void NoOpMembers_DoNotThrow()
        {
            var capturer = new PerRequestDebugCapturer();

            Assert.AreEqual(0, capturer.Count);
            capturer.Add(Guid.NewGuid(), null);
            Assert.IsNull(capturer.Get(Guid.NewGuid()));
            capturer.Remove(Guid.NewGuid());
            capturer.Shutdown();
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // JwtGenerator  (internal)
    // ═══════════════════════════════════════════════════════════════════════════

    [TestClass]
    [TestCategory("Unit_Coverage")]
    public class JwtGeneratorTests
    {
        static string _key = null!;

        [ClassInitialize]
        public static void Init(TestContext _)
        {
            using var hmac = new System.Security.Cryptography.HMACSHA256();
            _key = Convert.ToBase64String(hmac.Key);
        }

        [TestMethod]
        public void GenerateToken_ReturnsThreePartJwt()
        {
            var token = JwtGenerator.GenerateToken(new[] { "Admins" }, _key);
            var parts = token.Split('.');
            Assert.AreEqual(3, parts.Length, $"JWT must have header.payload.signature. Got: {token}");
        }

        [TestMethod]
        public void GenerateToken_PayloadContainsUserGroups()
        {
            var groups = new[] { "TeamA", "TeamB" };
            var token  = JwtGenerator.GenerateToken(groups, _key);

            var payloadJson = System.Text.Encoding.UTF8.GetString(
                Convert.FromBase64String(PadBase64(token.Split('.')[1])));

            Assert.IsTrue(payloadJson.Contains("TeamA"), $"Payload should contain 'TeamA'. Got: {payloadJson}");
            Assert.IsTrue(payloadJson.Contains("TeamB"), $"Payload should contain 'TeamB'. Got: {payloadJson}");
        }

        [TestMethod]
        public void GenerateToken_ValidatesWithJwtValidator()
        {
            var groups = new[] { "Warewolf Administrators" };
            var token  = "Bearer " + JwtGenerator.GenerateToken(groups, _key);

            var result = JwtValidator.GetUserGroups(token, _key);

            Assert.IsNotNull(result, "JwtValidator should accept a token generated by JwtGenerator");
            CollectionAssert.Contains((System.Collections.ICollection)result, "Warewolf Administrators");
        }

        [TestMethod]
        public void GenerateToken_DefaultLifetime_IsNotExpired()
        {
            var token  = JwtGenerator.GenerateToken(new[] { "G" }, _key);
            var groups = JwtValidator.GetUserGroups("Bearer " + token, _key);
            Assert.IsNotNull(groups, "Token with default 20-minute lifetime should not be expired immediately");
        }

        static string PadBase64(string base64Url)
        {
            var s = base64Url.Replace('-', '+').Replace('_', '/');
            return (s.Length % 4) switch
            {
                2 => s + "==",
                3 => s + "=",
                _ => s,
            };
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // WorkflowOpenApiGenerator  (internal)
    // ═══════════════════════════════════════════════════════════════════════════

    [TestClass]
    [TestCategory("Unit_Coverage")]
    public class WorkflowOpenApiGeneratorTests
    {
        static string _workflowFile = null!;

        [ClassInitialize]
        public static void Init(TestContext _)
        {
            // Minimal workflow XML with scalars, a recordset and a JSON object input/output.
            const string xml = """
                <Service Name="TestWorkflow">
                  <DataList>
                    <username Description="" IsEditable="true" ColumnIODirection="Input" />
                    <result   Description="" IsEditable="true" ColumnIODirection="Output" />
                    <orders   Description="" IsEditable="true" ColumnIODirection="Both">
                      <item    Description="" IsEditable="true" ColumnIODirection="Both" />
                      <amount  Description="" IsEditable="true" ColumnIODirection="Both" />
                    </orders>
                    <meta     Description="" IsEditable="true" IsJson="true" ColumnIODirection="Input" />
                  </DataList>
                </Service>
                """;
            _workflowFile = Path.GetTempFileName();
            File.WriteAllText(_workflowFile, xml);
        }

        [ClassCleanup]
        public static void Cleanup() => File.Delete(_workflowFile);

        [TestMethod]
        public void Generate_ReturnsValidJson()
        {
            var spec = WorkflowOpenApiGenerator.Generate(
                _workflowFile,
                "TestWorkflow",
                new Uri("https://example.com/api/TestWorkflow"));

            Assert.IsNotNull(spec);
            using var doc = JsonDocument.Parse(spec);
            Assert.AreEqual("3.0.1", doc.RootElement.GetProperty("openapi").GetString());
        }

        [TestMethod]
        public void Generate_ContainsWorkflowPathAndScalarParam()
        {
            var spec = WorkflowOpenApiGenerator.Generate(
                _workflowFile,
                "TestWorkflow",
                new Uri("https://example.com/api/TestWorkflow"));

            Assert.IsTrue(spec.Contains("username"), "Should include scalar input param 'username'");
        }

        [TestMethod]
        public void Generate_RecordsetInput_IsTypedAsObject()
        {
            var spec = WorkflowOpenApiGenerator.Generate(
                _workflowFile,
                "TestWorkflow",
                new Uri("https://example.com/api/TestWorkflow"));

            using var doc  = JsonDocument.Parse(spec);
            var paths      = doc.RootElement.GetProperty("paths");
            var firstPath  = paths.EnumerateObject().GetEnumerator();
            firstPath.MoveNext();
            var parameters = firstPath.Current.Value.GetProperty("get").GetProperty("parameters");
            bool hasOrdersObject = false;
            foreach (var p in parameters.EnumerateArray())
            {
                if (p.GetProperty("name").GetString() == "orders" &&
                    p.GetProperty("schema").GetProperty("type").GetString() == "object")
                {
                    hasOrdersObject = true;
                    break;
                }
            }
            Assert.IsTrue(hasOrdersObject, "Recordset 'orders' should be typed as object in parameters");
        }

        [TestMethod]
        public void Generate_MissingWorkflowFile_ReturnsEmptyDataListSpec()
        {
            // A missing file should not throw; it falls back to an empty DataList.
            var spec = WorkflowOpenApiGenerator.Generate(
                Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".bite"),
                "NonExistent",
                new Uri("https://example.com/api/NonExistent"));

            Assert.IsNotNull(spec);
            using var doc = JsonDocument.Parse(spec);
            Assert.AreEqual("3.0.1", doc.RootElement.GetProperty("openapi").GetString());
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CompositeExecutionLogger
    // ═══════════════════════════════════════════════════════════════════════════

    [TestClass]
    [TestCategory("Unit_Coverage")]
    public class CompositeExecutionLoggerTests
    {
        sealed class TrackingLogger : IExecutionLogger
        {
            public readonly List<string> Calls = new();

            public void LogDebug(string message, Guid executionId)              => Calls.Add($"Debug:{message}");
            public void LogDebug(string message, Exception ex, Guid executionId)=> Calls.Add($"Debug+Ex:{message}");
            public void LogInfo(string message, Guid executionId)               => Calls.Add($"Info:{message}");
            public void LogInfo(string message, Exception ex, Guid executionId) => Calls.Add($"Info+Ex:{message}");
            public void LogInfo(string message)                                  => Calls.Add($"Info0:{message}");
            public void LogWarning(string message, Guid executionId)            => Calls.Add($"Warn:{message}");
            public void LogWarning(string message, Exception ex, Guid executionId) => Calls.Add($"Warn+Ex:{message}");
            public void LogError(string message, Guid executionId)              => Calls.Add($"Error:{message}");
            public void LogError(string name, Exception ex, Guid executionId)   => Calls.Add($"Error+Ex:{name}");
            public void LogError(Exception ex, string log)                       => Calls.Add($"Error+Log:{log}");
            public void LogFatal(string message, Guid executionId)              => Calls.Add($"Fatal:{message}");
            public void LogFatal(string message, Exception ex, Guid executionId)=> Calls.Add($"Fatal+Ex:{message}");
        }

        static readonly Guid _id = Guid.NewGuid();

        [TestMethod]
        public void AllLogMethods_ForwardToAllInnerLoggers()
        {
            var a = new TrackingLogger();
            var b = new TrackingLogger();
            var composite = new CompositeExecutionLogger(new[] { a, b });
            var ex = new Exception("boom");

            composite.LogDebug("d1", _id);
            composite.LogDebug("d2", ex, _id);
            composite.LogInfo("i1", _id);
            composite.LogInfo("i2", ex, _id);
            composite.LogInfo("i3");
            composite.LogWarning("w1", _id);
            composite.LogWarning("w2", ex, _id);
            composite.LogError("e1", _id);
            composite.LogError("act", ex, _id);
            composite.LogError(ex, "log");
            composite.LogFatal("f1", _id);
            composite.LogFatal("f2", ex, _id);

            Assert.AreEqual(12, a.Calls.Count, "All 12 log calls should reach logger A");
            Assert.AreEqual(12, b.Calls.Count, "All 12 log calls should reach logger B");
        }

        [TestMethod]
        public void Constructor_NullLoggers_Throws()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => new CompositeExecutionLogger(null!));
        }

        [TestMethod]
        public void EmptyLoggers_DoesNotThrow()
        {
            var composite = new CompositeExecutionLogger(Array.Empty<IExecutionLogger>());
            composite.LogInfo("msg", Guid.NewGuid()); // no-op, no throw
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ElasticsearchLogDocument
    // ═══════════════════════════════════════════════════════════════════════════

    [TestClass]
    [TestCategory("Unit_Coverage")]
    public class ElasticsearchLogDocumentTests
    {
        [TestMethod]
        public void DefaultDocument_HasInfoLevel()
        {
            var doc = new ElasticsearchLogDocument();
            Assert.AreEqual("info", doc.Level);
            Assert.AreEqual(string.Empty, doc.Message);
        }

        [TestMethod]
        public void Document_AllPropertiesAssignable()
        {
            var id  = Guid.NewGuid();
            var doc = new ElasticsearchLogDocument
            {
                Level        = "error",
                Message      = "Something went wrong",
                ExecutionId  = id,
                ActivityName = "MySqlActivity",
                ErrorMessage = "Timeout",
                StackTrace   = "at line 1",
            };

            Assert.AreEqual("error",                doc.Level);
            Assert.AreEqual("Something went wrong", doc.Message);
            Assert.AreEqual(id,                     doc.ExecutionId);
            Assert.AreEqual("MySqlActivity",         doc.ActivityName);
            Assert.AreEqual("Timeout",              doc.ErrorMessage);
            Assert.AreEqual("at line 1",            doc.StackTrace);
        }

        [TestMethod]
        public void Timestamp_IsCloseToUtcNow()
        {
            var before = DateTimeOffset.UtcNow.AddSeconds(-1);
            var doc    = new ElasticsearchLogDocument();
            var after  = DateTimeOffset.UtcNow.AddSeconds(1);

            Assert.IsTrue(doc.Timestamp >= before && doc.Timestamp <= after,
                $"Timestamp {doc.Timestamp} should be close to UTC now");
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ElasticsearchLoggingOptions
    // ═══════════════════════════════════════════════════════════════════════════

    [TestClass]
    [TestCategory("Unit_Coverage")]
    public class ElasticsearchLoggingOptionsTests
    {
        [TestMethod]
        public void IsConfigured_WhenUriSet_ReturnsTrue()
        {
            var opts = new ElasticsearchLoggingOptions { Uri = "http://localhost:9200" };
            Assert.IsTrue(opts.IsConfigured);
        }

        [TestMethod]
        public void IsConfigured_WhenUriNull_ReturnsFalse()
        {
            var opts = new ElasticsearchLoggingOptions();
            Assert.IsFalse(opts.IsConfigured);
        }

        [TestMethod]
        public void FromEnvironment_ReadsElasticsearchVariables()
        {
            Environment.SetEnvironmentVariable("Elasticsearch__Uri",       "http://es.local:9200");
            Environment.SetEnvironmentVariable("Elasticsearch__IndexName",  "my-index");
            Environment.SetEnvironmentVariable("Elasticsearch__Username",   "admin");
            Environment.SetEnvironmentVariable("Elasticsearch__Password",   "secret");
            Environment.SetEnvironmentVariable("Elasticsearch__ApiKey",     "key123");
            try
            {
                var opts = ElasticsearchLoggingOptions.FromEnvironment();
                Assert.AreEqual("http://es.local:9200", opts.Uri);
                Assert.AreEqual("my-index",            opts.IndexName);
                Assert.AreEqual("admin",               opts.Username);
                Assert.AreEqual("secret",              opts.Password);
                Assert.AreEqual("key123",              opts.ApiKey);
            }
            finally
            {
                Environment.SetEnvironmentVariable("Elasticsearch__Uri",      null);
                Environment.SetEnvironmentVariable("Elasticsearch__IndexName", null);
                Environment.SetEnvironmentVariable("Elasticsearch__Username",  null);
                Environment.SetEnvironmentVariable("Elasticsearch__Password",  null);
                Environment.SetEnvironmentVariable("Elasticsearch__ApiKey",    null);
            }
        }

        [TestMethod]
        public void FromEnvironment_MissingIndexName_DefaultsToWarewolfLogs()
        {
            Environment.SetEnvironmentVariable("Elasticsearch__IndexName", null);
            var opts = ElasticsearchLoggingOptions.FromEnvironment();
            Assert.AreEqual("warewolf-execution-logs", opts.IndexName);
        }

        [TestMethod]
        public void FromBiteFile_Password_ParsesUsernameAndPassword()
        {
            var xml = """
                <Source ConnectionString="HostName=http://myes;Port=9200;SearchIndex=test-logs;AuthenticationType=Password;Username=testuser;Password=test123" />
                """;
            var tmpFile = Path.GetTempFileName();
            try
            {
                File.WriteAllText(tmpFile, xml);
                var opts = ElasticsearchLoggingOptions.FromBiteFile(tmpFile);

                Assert.AreEqual("http://myes:9200", opts.Uri);
                Assert.AreEqual("test-logs",         opts.IndexName);
                Assert.AreEqual("testuser",          opts.Username);
                Assert.AreEqual("test123",           opts.Password);
                Assert.IsNull(opts.ApiKey);
                Assert.IsTrue(opts.IsConfigured);
            }
            finally
            {
                File.Delete(tmpFile);
            }
        }

        [TestMethod]
        public void FromBiteFile_ApiKey_PopulatesApiKey()
        {
            var xml = """
                <Source ConnectionString="HostName=http://myes;Port=9200;SearchIndex=api-logs;AuthenticationType=API_Key;Password=myBase64Key==" />
                """;
            var tmpFile = Path.GetTempFileName();
            try
            {
                File.WriteAllText(tmpFile, xml);
                var opts = ElasticsearchLoggingOptions.FromBiteFile(tmpFile);

                Assert.AreEqual("myBase64Key==", opts.ApiKey);
                Assert.IsNull(opts.Username);
                Assert.IsNull(opts.Password);
            }
            finally
            {
                File.Delete(tmpFile);
            }
        }

        [TestMethod]
        public void FromBiteFile_Anonymous_NoCredentials()
        {
            var xml = """
                <Source ConnectionString="HostName=http://myes;Port=9200;SearchIndex=anon-logs;AuthenticationType=Anonymous" />
                """;
            var tmpFile = Path.GetTempFileName();
            try
            {
                File.WriteAllText(tmpFile, xml);
                var opts = ElasticsearchLoggingOptions.FromBiteFile(tmpFile);

                Assert.IsNull(opts.Username);
                Assert.IsNull(opts.Password);
                Assert.IsNull(opts.ApiKey);
            }
            finally
            {
                File.Delete(tmpFile);
            }
        }
    }
}
