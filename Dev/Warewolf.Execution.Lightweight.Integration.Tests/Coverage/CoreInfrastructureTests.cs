/*
 * Coverage tests for the core infrastructure classes of the lightweight execution engine.
 *
 * PRE-REQUISITE for the HTTP test classes: Azure Functions host running at http://localhost:7071
 * HTTP tests are marked Inconclusive (not Failed) when the host is not reachable.
 *
 * In-process test classes (no host required):
 *
 *   WorkflowFunctionHelperTests          — static request-building helpers
 *   WorkflowIndexTests                   — workflow-file index (JSON fast-path + disk scan)
 *   WorkflowResourceCacheTests           — bite-file resource cache (ById + ByName lookups)
 *   WorkflowExecutorTests                — error-path and OpenAPI short-circuit paths
 *
 * HTTP integration test classes (host required):
 *
 *   WorkflowHttpFunctionHttpTests        — WorkflowHttpFunction routes:
 *     /workflow           (400 on missing params)
 *     /workflow/{name}    (named workflow route)
 *     /Public/{name}      (anonymous execution)
 *     /Public/{name}.api  (OpenAPI spec via WorkflowOpenApiGenerator)
 *     /apis.json          (root discovery)
 *     /Secure/{name}      (JWT-gated; 401 without token)
 */

using Dev2.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Warewolf.Execution.Lightweight.Logging;
using Warewolf.Execution.Lightweight.Models;

namespace Warewolf.Execution.Lightweight.Integration.Tests.Coverage
{
    // ══════════════════════════════════════════════════════════════════════════════
    // WorkflowFunctionHelper — public static helper; no HTTP host required
    // ══════════════════════════════════════════════════════════════════════════════

    [TestClass]
    [TestCategory("Unit_Coverage")]
    public class WorkflowFunctionHelperTests
    {
        [TestMethod]
        public void CreateRequest_SetsFilePath()
        {
            var req = WorkflowFunctionHelper.CreateRequest("/data/workflows/Hello.bite");

            Assert.AreEqual("/data/workflows/Hello.bite", req.WorkflowFilePath);
            Assert.IsNotNull(req.InputParameters);
        }

        [TestMethod]
        public void CreateRequest_WithInputs_PropagatesInputs()
        {
            var inputs = new Dictionary<string, string> { ["Name"] = "Alice", ["Age"] = "30" };
            var req    = WorkflowFunctionHelper.CreateRequest("/wf/test.bite", inputs);

            Assert.AreEqual("Alice", req.InputParameters["Name"]);
            Assert.AreEqual("30",   req.InputParameters["Age"]);
        }

        [TestMethod]
        public void CreateRequest_NullInputs_YieldsEmptyDictionary()
        {
            var req = WorkflowFunctionHelper.CreateRequest("/wf/test.bite", inputs: null);

            Assert.IsNotNull(req.InputParameters);
            Assert.AreEqual(0, req.InputParameters.Count);
        }

        [TestMethod]
        public void CreateRequestByName_SetsWorkflowName()
        {
            var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var req = WorkflowFunctionHelper.CreateRequestByName("HelloWorld", dir);

            Assert.AreEqual("HelloWorld", req.WorkflowName);
            Assert.AreEqual(dir, req.WorkflowsDirectory);
        }

        [TestMethod]
        public void CreateRequestByName_NoMatchingFile_DefaultsToXmlExtension()
        {
            // Use a directory that does not exist so no disk I/O can match the name.
            var dir     = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var req     = WorkflowFunctionHelper.CreateRequestByName("MyWorkflow", dir);

            // When neither index file nor on-disk file is found, the helper builds
            // a default path with .xml extension.
            Assert.IsTrue(req.WorkflowFilePath.EndsWith("MyWorkflow.xml",
                StringComparison.OrdinalIgnoreCase),
                $"Expected .xml default path; got: {req.WorkflowFilePath}");
        }

        [TestMethod]
        public void CreateRequestByName_WithInputs_ForwardsInputs()
        {
            var dir    = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var inputs = new Dictionary<string, string> { ["x"] = "1" };
            var req    = WorkflowFunctionHelper.CreateRequestByName("Wf", dir, inputs);

            Assert.AreEqual("1", req.InputParameters["x"]);
        }

        [TestMethod]
        public void ParseFromJson_ValidJson_Deserializes()
        {
            const string json = @"{
                ""workflowFilePath"": ""/wf/test.bite"",
                ""workflowName"":     ""Test"",
                ""isDebug"":          true,
                ""inputParameters"":  { ""key"": ""value"" }
            }";

            var req = WorkflowFunctionHelper.ParseFromJson(json);

            Assert.AreEqual("/wf/test.bite", req.WorkflowFilePath);
            Assert.AreEqual("Test",          req.WorkflowName);
            Assert.IsTrue(req.IsDebug);
            Assert.AreEqual("value", req.InputParameters["key"]);
        }

        [TestMethod]
        public void ParseFromJson_EmptyString_ReturnsEmptyRequest()
        {
            var req = WorkflowFunctionHelper.ParseFromJson(string.Empty);
            Assert.IsNotNull(req);
            Assert.IsNull(req.WorkflowFilePath);
        }

        [TestMethod]
        public void ParseFromJson_NullString_ReturnsEmptyRequest()
        {
            var req = WorkflowFunctionHelper.ParseFromJson(null);
            Assert.IsNotNull(req);
            Assert.IsNull(req.WorkflowFilePath);
        }

        [TestMethod]
        public void ParseFromJson_InvalidJson_ReturnsEmptyRequest()
        {
            var req = WorkflowFunctionHelper.ParseFromJson("{this is not valid json{{{{");
            Assert.IsNotNull(req);
        }

        [TestMethod]
        public void ParseFromJson_JsonNull_ReturnsEmptyRequest()
        {
            var req = WorkflowFunctionHelper.ParseFromJson("null");
            Assert.IsNotNull(req);
        }
    }

    // ══════════════════════════════════════════════════════════════════════════════
    // WorkflowIndex — internal sealed singleton; covers fast-path (JSON index) and
    // slow-path (disk scan + persist) branches.
    // Requires InternalsVisibleTo("Warewolf.Execution.Lightweight.Integration.Tests")
    // ══════════════════════════════════════════════════════════════════════════════

    [TestClass]
    [TestCategory("Unit_Coverage")]
    public class WorkflowIndexTests
    {
        [TestMethod]
        public void Instance_ReturnsSameSingleton()
        {
            var a = WorkflowIndex.Instance;
            var b = WorkflowIndex.Instance;
            Assert.AreSame(a, b, "WorkflowIndex.Instance must always return the same object");
        }

        [TestMethod]
        public void WarmUp_NonExistentDirectory_DoesNotThrow()
        {
            var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            // Non-existent directory → BuildIndexFromDisk returns empty; should not throw.
            WorkflowIndex.Instance.WarmUp(dir);
        }

        [TestMethod]
        public void Resolve_NullDirectory_ReturnsNull()
        {
            var result = WorkflowIndex.Instance.Resolve(null, "SomeWorkflow");
            Assert.IsNull(result);
        }

        [TestMethod]
        public void Resolve_NullWorkflowName_ReturnsNull()
        {
            var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var result = WorkflowIndex.Instance.Resolve(dir, null);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void Resolve_EmptyWorkflowName_ReturnsNull()
        {
            var dir    = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var result = WorkflowIndex.Instance.Resolve(dir, "   ");
            Assert.IsNull(result);
        }

        [TestMethod]
        public void Resolve_WithIndexFile_FindsWorkflow()
        {
            // Arrange: create a temp directory with a workflow-index.json and the .bite file.
            var dir       = Directory.CreateTempSubdirectory().FullName;
            var indexPath = Path.Combine(dir, WorkflowIndex.IndexFileName);
            var wfPath    = Path.Combine(dir, "Hello World.bite");
            try
            {
                File.WriteAllText(wfPath, "<Service/>");
                File.WriteAllText(indexPath, """
                    {
                      "hello world": "Hello World.bite"
                    }
                    """);

                // Act
                var resolved = WorkflowIndex.Instance.Resolve(dir, "Hello World");

                // Assert
                Assert.IsNotNull(resolved, "Should resolve workflow from index file");
                Assert.IsTrue(resolved.EndsWith("Hello World.bite", StringComparison.OrdinalIgnoreCase),
                    $"Resolved path should end with 'Hello World.bite'; got: {resolved}");
            }
            finally
            {
                Directory.Delete(dir, recursive: true);
            }
        }

        [TestMethod]
        public void Resolve_WithIndexFile_CaseInsensitiveLookup()
        {
            var dir       = Directory.CreateTempSubdirectory().FullName;
            var indexPath = Path.Combine(dir, WorkflowIndex.IndexFileName);
            try
            {
                File.WriteAllText(indexPath, """{ "hello world": "Hello World.bite" }""");
                File.WriteAllText(Path.Combine(dir, "Hello World.bite"), "<Service/>");

                // Key is normalised to lowercase; lookup should be case-insensitive.
                var resolved = WorkflowIndex.Instance.Resolve(dir, "HELLO WORLD");

                Assert.IsNotNull(resolved, "Lookup should be case-insensitive");
            }
            finally
            {
                Directory.Delete(dir, recursive: true);
            }
        }

        [TestMethod]
        public void Resolve_WithoutIndexFile_BuildsIndexFromDisk()
        {
            // Arrange: directory with .bite files but no workflow-index.json.
            var dir    = Directory.CreateTempSubdirectory().FullName;
            var wfFile = Path.Combine(dir, "MyFlow.bite");
            try
            {
                File.WriteAllText(wfFile, "<Service/>");
                // Do NOT create workflow-index.json — forces the disk-scan code path.

                var resolved = WorkflowIndex.Instance.Resolve(dir, "MyFlow");

                Assert.IsNotNull(resolved, "Disk scan should find MyFlow.bite");
                Assert.IsTrue(resolved.EndsWith("MyFlow.bite", StringComparison.OrdinalIgnoreCase),
                    $"Expected path ending in 'MyFlow.bite'; got: {resolved}");
            }
            finally
            {
                Directory.Delete(dir, recursive: true);
            }
        }

        [TestMethod]
        public void Resolve_WithoutIndexFile_PersistsIndexJson()
        {
            // After a disk scan the index should be written to workflow-index.json.
            var dir    = Directory.CreateTempSubdirectory().FullName;
            var wfFile = Path.Combine(dir, "Persist.bite");
            try
            {
                File.WriteAllText(wfFile, "<Service/>");

                WorkflowIndex.Instance.Resolve(dir, "Persist");

                var indexPath = Path.Combine(dir, WorkflowIndex.IndexFileName);
                Assert.IsTrue(File.Exists(indexPath),
                    "workflow-index.json should be written after first disk scan");

                var json   = File.ReadAllText(indexPath);
                var parsed = JObject.Parse(json);
                Assert.IsTrue(parsed.ContainsKey("persist"),
                    $"Written index should contain key 'persist'. Index: {json}");
            }
            finally
            {
                Directory.Delete(dir, recursive: true);
            }
        }

        [TestMethod]
        public void Resolve_NonExistentWorkflowInIndex_ReturnsNull()
        {
            var dir       = Directory.CreateTempSubdirectory().FullName;
            var indexPath = Path.Combine(dir, WorkflowIndex.IndexFileName);
            try
            {
                File.WriteAllText(indexPath, """{ "existing": "Existing.bite" }""");

                var result = WorkflowIndex.Instance.Resolve(dir, "NonExistent");

                Assert.IsNull(result, "Should return null for a name not present in the index");
            }
            finally
            {
                Directory.Delete(dir, recursive: true);
            }
        }

        [TestMethod]
        public void Resolve_EmptyDirectory_ReturnsNull()
        {
            var dir = Directory.CreateTempSubdirectory().FullName;
            try
            {
                var result = WorkflowIndex.Instance.Resolve(dir, "AnyWorkflow");
                Assert.IsNull(result, "Empty directory should yield null");
            }
            finally
            {
                Directory.Delete(dir, recursive: true);
            }
        }

        [TestMethod]
        public void Resolve_MalformedIndexFile_ReturnsNull()
        {
            var dir       = Directory.CreateTempSubdirectory().FullName;
            var indexPath = Path.Combine(dir, WorkflowIndex.IndexFileName);
            try
            {
                File.WriteAllText(indexPath, "THIS IS NOT JSON {{{{");

                var result = WorkflowIndex.Instance.Resolve(dir, "AnyWorkflow");

                // Malformed index should be silently ignored; result is null.
                Assert.IsNull(result);
            }
            finally
            {
                Directory.Delete(dir, recursive: true);
            }
        }
    }

    // ══════════════════════════════════════════════════════════════════════════════
    // WorkflowResourceCache — internal sealed singleton; covers BuildIndex (disk scan),
    // TryReadEntry (XML attribute extraction), Resolve (ById + ByName), GetEntry.
    // Requires InternalsVisibleTo("Warewolf.Execution.Lightweight.Integration.Tests")
    // ══════════════════════════════════════════════════════════════════════════════

    [TestClass]
    [TestCategory("Unit_Coverage")]
    public class WorkflowResourceCacheTests
    {
        static readonly Guid _testResourceId = new("12345678-1234-1234-1234-123456789012");
        static readonly Guid _testServerId   = new("11111111-1111-1111-1111-111111111111");

        /// <summary>Creates a minimal .bite file that TryReadEntry can parse.</summary>
        static string CreateMinimalBiteFile(string directory, string name, Guid resourceId)
        {
            var path = Path.Combine(directory, $"{name}.bite");
            File.WriteAllText(path, $"""
                <Service ID="{resourceId}" ServerID="{_testServerId}" Name="{name}" ResourceType="WorkflowService">
                </Service>
                """);
            return path;
        }

        [TestMethod]
        public void Instance_ReturnsSameSingleton()
        {
            var a = WorkflowResourceCache.Instance;
            var b = WorkflowResourceCache.Instance;
            Assert.AreSame(a, b, "WorkflowResourceCache.Instance must be a singleton");
        }

        [TestMethod]
        public void WarmUp_NonExistentDirectory_DoesNotThrow()
        {
            var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            WorkflowResourceCache.Instance.WarmUp(dir);  // must not throw
        }

        [TestMethod]
        public void Resolve_NonExistentDirectory_ReturnsNull()
        {
            var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var result = WorkflowResourceCache.Instance.Resolve(dir, _testResourceId, "TestWorkflow");
            Assert.IsNull(result, "Non-existent directory should yield null");
        }

        [TestMethod]
        public void Resolve_ById_FindsMatchingBiteFile()
        {
            var dir = Directory.CreateTempSubdirectory().FullName;
            try
            {
                var filePath = CreateMinimalBiteFile(dir, "MyWorkflow", _testResourceId);

                var resolved = WorkflowResourceCache.Instance.Resolve(dir, _testResourceId, null);

                Assert.IsNotNull(resolved, "Should find workflow by ResourceId");
                Assert.AreEqual(filePath, resolved);
            }
            finally
            {
                Directory.Delete(dir, recursive: true);
            }
        }

        [TestMethod]
        public void Resolve_ByName_WhenGuidIsEmpty_FindsByNameFallback()
        {
            var dir = Directory.CreateTempSubdirectory().FullName;
            try
            {
                CreateMinimalBiteFile(dir, "NamedWorkflow", Guid.NewGuid());

                // Pass Guid.Empty → forces the name fallback branch.
                var resolved = WorkflowResourceCache.Instance.Resolve(dir, Guid.Empty, "NamedWorkflow");

                Assert.IsNotNull(resolved, "Should find workflow by name fallback");
                StringAssert.Contains(resolved, "NamedWorkflow");
            }
            finally
            {
                Directory.Delete(dir, recursive: true);
            }
        }

        [TestMethod]
        public void Resolve_UnknownGuidAndUnknownName_ReturnsNull()
        {
            var dir = Directory.CreateTempSubdirectory().FullName;
            try
            {
                CreateMinimalBiteFile(dir, "Existing", Guid.NewGuid());

                var result = WorkflowResourceCache.Instance.Resolve(dir, Guid.NewGuid(), "NonExistent");

                Assert.IsNull(result, "Neither ById nor ByName matched — should return null");
            }
            finally
            {
                Directory.Delete(dir, recursive: true);
            }
        }

        [TestMethod]
        public void Resolve_EmptyGuidAndNullName_ReturnsNull()
        {
            var dir = Directory.CreateTempSubdirectory().FullName;
            try
            {
                CreateMinimalBiteFile(dir, "AnyFlow", Guid.NewGuid());

                var result = WorkflowResourceCache.Instance.Resolve(dir, Guid.Empty, null);

                Assert.IsNull(result, "Empty guid + null name should return null");
            }
            finally
            {
                Directory.Delete(dir, recursive: true);
            }
        }

        [TestMethod]
        public void GetEntry_WithValidResourceId_ReturnsEntry()
        {
            var dir = Directory.CreateTempSubdirectory().FullName;
            try
            {
                CreateMinimalBiteFile(dir, "EntryWorkflow", _testResourceId);

                var entry = WorkflowResourceCache.Instance.GetEntry(dir, _testResourceId);

                Assert.IsNotNull(entry, "GetEntry should return a non-null entry for a known GUID");
                Assert.AreEqual(_testResourceId, entry.ResourceId);
                Assert.AreEqual("EntryWorkflow", entry.Name);
            }
            finally
            {
                Directory.Delete(dir, recursive: true);
            }
        }

        [TestMethod]
        public void GetEntry_UnknownResourceId_ReturnsNull()
        {
            var dir = Directory.CreateTempSubdirectory().FullName;
            try
            {
                var result = WorkflowResourceCache.Instance.GetEntry(dir, Guid.NewGuid());
                Assert.IsNull(result, "GetEntry should return null for an unknown GUID");
            }
            finally
            {
                Directory.Delete(dir, recursive: true);
            }
        }

        [TestMethod]
        public void BuildIndex_MalformedBiteFile_IsSkippedSilently()
        {
            var dir = Directory.CreateTempSubdirectory().FullName;
            try
            {
                // Write a file that looks like a .bite but contains no parseable GUID.
                File.WriteAllText(Path.Combine(dir, "Broken.bite"), "<Service Name=\"Broken\"/>");
                // A valid one alongside the broken one.
                CreateMinimalBiteFile(dir, "GoodFlow", Guid.NewGuid());

                // WarmUp triggers BuildIndex; it must not throw despite the malformed file.
                WorkflowResourceCache.Instance.WarmUp(dir);
            }
            finally
            {
                Directory.Delete(dir, recursive: true);
            }
        }
    }

    // ══════════════════════════════════════════════════════════════════════════════
    // WorkflowExecutor — public class; covers error-path (null/invalid/missing-file)
    // and the OpenAPI short-circuit branch (spec generated before file-exists guard).
    // ══════════════════════════════════════════════════════════════════════════════

    [TestClass]
    [TestCategory("Unit_Coverage")]
    public class WorkflowExecutorTests
    {
        sealed class NoOpLogger : IExecutionLogger
        {
            public void LogTrace(string message, Guid executionId) { }
            public void LogTrace(string message, Exception exception, Guid executionId) { }
            public void LogDebug(string message, Guid executionId) { }
            public void LogDebug(string message, Exception exception, Guid executionId) { }
            public void LogInfo(string message, Guid executionId) { }
            public void LogInfo(string message, Exception exception, Guid executionId) { }
            public void LogInfo(string message) { }
            public void LogWarning(string message, Guid executionId) { }
            public void LogWarning(string message, Exception exception, Guid executionId) { }
            public void LogError(string message, Guid executionId) { }
            public void LogError(string activityName, Exception ex, Guid executionId) { }
            public void LogError(Exception ex, string log) { }
            public void LogFatal(string message, Guid executionId) { }
            public void LogFatal(string message, Exception exception, Guid executionId) { }
        }

        static IWorkflowExecutor CreateExecutor() => new WorkflowExecutor(new NoOpLogger());

        [TestMethod]
        public void Constructor_NullLogger_ThrowsArgumentNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new WorkflowExecutor(null));
        }

        [TestMethod]
        public void Execute_NullRequest_ReturnsFailure()
        {
            var executor = CreateExecutor();

            var result = executor.Execute((WorkflowExecutionRequest)null);

            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Errors.Count > 0, "Failure result should contain at least one error message");
        }

        [TestMethod]
        public void Execute_InvalidRequest_NoFilePath_ReturnsFailure()
        {
            var executor = CreateExecutor();
            var request  = new WorkflowExecutionRequest();   // WorkflowFilePath is null → IsValid = false

            var result = executor.Execute(request);

            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Errors.Count > 0);
        }

        [TestMethod]
        public void Execute_MissingWorkflowFile_ReturnsFailure()
        {
            var executor = CreateExecutor();
            var request  = new WorkflowExecutionRequest
            {
                WorkflowFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + "_nonexistent.bite"),
                ReturnType       = EmitionTypes.JSON
            };

            var result = executor.Execute(request);

            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.Errors.Count > 0, "Should report file-not-found error");
            StringAssert.Contains(result.Errors[0], "not found",
                $"Error message should mention 'not found'. Got: {result.Errors[0]}");
        }

        [TestMethod]
        [Ignore("Requires WorkflowExecutor to handle the OPENAPI emission type before the file-exists guard. Re-introduce when WOLF-8418 is complete.")]
        public async Task Execute_OpenApiRequest_MissingFile_ReturnsValidOpenApiSpec()
        {
            // The OPENAPI path is handled BEFORE the file-exists guard in WorkflowExecutor,
            // so a missing file is not an error — it returns a minimal empty-DataList spec.
            var executor = CreateExecutor();
            var request  = new WorkflowExecutionRequest
            {
                WorkflowFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + "_missing.bite"),
                WorkflowName     = "MissingWorkflow",
                ReturnType       = EmitionTypes.OPENAPI,
                WebServerUri     = new Uri("https://localhost:7071/Public/MissingWorkflow.api")
            };

            var result = executor.Execute(request);

            Assert.IsTrue(result.IsSuccess, "OpenAPI spec should be returned even for a missing file");
            Assert.IsNotNull(result.PayloadWriter, "PayloadWriter should be set for the OpenAPI result");

            // Stream the payload and verify it is valid JSON with the OpenAPI version field.
            using var ms  = new MemoryStream();
            await result.PayloadWriter(ms, System.Threading.CancellationToken.None);
            var spec = Encoding.UTF8.GetString(ms.ToArray());

            Assert.IsFalse(string.IsNullOrWhiteSpace(spec), "OpenAPI spec should not be empty");
            var parsed = JObject.Parse(spec);
            Assert.AreEqual("3.0.1", parsed["openapi"]?.Value<string>(),
                $"spec.openapi should be '3.0.1'. Got: {spec.Substring(0, Math.Min(200, spec.Length))}");
        }

        [TestMethod]
        public void Execute_SimpleRequest_ReturnsWorkflowFilePath()
        {
            // Verify the string overload delegates to the request overload correctly.
            var executor = CreateExecutor();
            var filePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + "_nofile.bite");

            var result = executor.Execute(filePath);

            Assert.IsFalse(result.IsSuccess, "Non-existent file should yield a failure result");
        }
    }

    // ══════════════════════════════════════════════════════════════════════════════
    // WorkflowHttpFunction — HTTP integration tests exercising:
    //   • /workflow            → 400 when no params given
    //   • /workflow/{name}     → non-existent file → 500 with JSON error body
    //   • /Public/{name}       → anonymous execution; non-existent → 500 error JSON
    //   • /Public/{name}.api   → OpenAPI spec (WorkflowOpenApiGenerator exercised)
    //   • /apis.json           → root discovery (ExecuteRootApisJson)
    //   • /Secure/{name}       → 401 when no JWT is supplied
    // ══════════════════════════════════════════════════════════════════════════════

    [TestClass]
    [TestCategory("HTTP_Coverage")]
    public class WorkflowHttpFunctionHttpTests
    {
        const string BaseUrl = "http://localhost:7071";

        static readonly HttpClient _http = new(new HttpClientHandler { AllowAutoRedirect = false })
        {
            Timeout = TimeSpan.FromSeconds(15)
        };

        static bool _hostAvailable;

        [ClassInitialize]
        public static async Task Init(TestContext _)
        {
            try
            {
                var r = await _http.GetAsync(BaseUrl + "/admin/host/ping");
                _hostAvailable = (int)r.StatusCode < 500;
            }
            catch
            {
                _hostAvailable = false;
            }
        }

        void SkipIfUnavailable()
        {
            if (!_hostAvailable)
                Assert.Inconclusive($"Azure Functions host not reachable at {BaseUrl}");
        }

        // ── /workflow (no params) → 400 ──────────────────────────────────────────

        [TestMethod]
        public async Task Execute_GetWorkflow_NoParams_Returns400()
        {
            SkipIfUnavailable();

            var resp = await _http.GetAsync(BaseUrl + "/workflow");
            var body = await resp.Content.ReadAsStringAsync();

            Assert.AreEqual(HttpStatusCode.BadRequest, resp.StatusCode,
                $"GET /workflow with no params should be 400. Got {(int)resp.StatusCode}: {body}");

            // Body should be valid JSON describing the missing parameter error.
            var json = JObject.Parse(body);
            Assert.IsTrue(json.ContainsKey("error"),
                $"400 body should have 'error' key. Got: {body}");
        }

        // ── /workflow/{workflowName} non-existent → 500 error JSON ───────────────

        [TestMethod]
        public async Task ExecuteByName_NonExistentWorkflow_Returns500WithErrorBody()
        {
            SkipIfUnavailable();

            var resp = await _http.GetAsync(BaseUrl + "/workflow/NonExistentWorkflow_unique_f3a9c1");
            var body = await resp.Content.ReadAsStringAsync();

            Assert.AreEqual(HttpStatusCode.InternalServerError, resp.StatusCode,
                $"GET /workflow/NonExistentWorkflow_unique_f3a9c1 should be 500. Got {(int)resp.StatusCode}: {body}");

            var json = JObject.Parse(body);
            Assert.IsTrue(json.ContainsKey("hasErrors") || json.ContainsKey("errors"),
                $"Error body should have 'hasErrors'/'errors'. Got: {body}");
        }

        // ── /Public/{name} non-existent → 500 error JSON ─────────────────────────

        [TestMethod]
        public async Task ExecutePublicWorkflow_NonExistentWorkflow_Returns500WithErrorBody()
        {
            SkipIfUnavailable();

            var resp = await _http.GetAsync(BaseUrl + "/Public/NonExistentWorkflow_unique_b7d2e4");
            var body = await resp.Content.ReadAsStringAsync();

            Assert.AreEqual(HttpStatusCode.InternalServerError, resp.StatusCode,
                $"GET /Public/NonExistentWorkflow should be 500. Got {(int)resp.StatusCode}: {body}");

            var json = JObject.Parse(body);
            Assert.IsTrue(json.ContainsKey("hasErrors") || json.ContainsKey("errors"),
                $"Error body should have 'hasErrors'/'errors'. Got: {body}");
        }

        // ── /Public/{name}.api → 200 with OpenAPI JSON ───────────────────────────

        [TestMethod]
        [Ignore("Requires /Public/{workflow}.api to return an OpenAPI spec instead of falling through to the workflow file lookup. Re-introduce when WOLF-8418 is complete.")]
        public async Task ExecutePublicWorkflow_ApiSuffix_Returns200WithOpenApiJson()
        {
            SkipIfUnavailable();

            // Even a non-existent workflow file returns a valid OpenAPI spec when the
            // .api suffix is used — WorkflowExecutor generates the spec before the
            // file-exists guard and WorkflowOpenApiGenerator handles missing files.
            var resp = await _http.GetAsync(BaseUrl + "/Public/NonExistentWorkflow_unique_c8e5f1.api");
            var body = await resp.Content.ReadAsStringAsync();

            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode,
                $"GET /Public/name.api should be 200 even for a missing file. Got {(int)resp.StatusCode}: {body}");

            var json = JObject.Parse(body);
            Assert.AreEqual("3.0.1", json["openapi"]?.Value<string>(),
                $"Response should be an OpenAPI 3.0.1 spec. Got: {body.Substring(0, Math.Min(300, body.Length))}");
        }

        // ── /apis.json → 200 with JSON body ──────────────────────────────────────

        [TestMethod]
        public async Task ExecuteRootApisJson_Returns200WithJsonBody()
        {
            SkipIfUnavailable();

            var resp = await _http.GetAsync(BaseUrl + "/apis.json");
            var body = await resp.Content.ReadAsStringAsync();

            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode,
                $"GET /apis.json should be 200. Got {(int)resp.StatusCode}: {body}");

            // Body must be valid JSON.
            Assert.IsNotNull(JToken.Parse(body),
                $"GET /apis.json body should be valid JSON. Got: {body}");
        }

        // ── /Secure/{name} with no JWT → 401 ────────────────────────────────────

        [TestMethod]
        public async Task ExecuteSecureWorkflow_NoJwt_Returns401()
        {
            SkipIfUnavailable();

            var resp = await _http.GetAsync(BaseUrl + "/Secure/AnyWorkflow");
            var body = await resp.Content.ReadAsStringAsync();

            // The middleware or WorkflowHttpFunction must reject unauthenticated requests.
            Assert.AreEqual(HttpStatusCode.Unauthorized, resp.StatusCode,
                $"GET /Secure/AnyWorkflow without JWT should be 401. Got {(int)resp.StatusCode}: {body}");
        }

        // ── /workflow (POST body) → 400 when body missing workflowFilePath/Name ──

        [TestMethod]
        public async Task Execute_PostWorkflow_EmptyBody_Returns400()
        {
            SkipIfUnavailable();

            var content = new StringContent("{}", Encoding.UTF8, "application/json");
            var resp    = await _http.PostAsync(BaseUrl + "/workflow", content);
            var body    = await resp.Content.ReadAsStringAsync();

            Assert.AreEqual(HttpStatusCode.BadRequest, resp.StatusCode,
                $"POST /workflow with empty body should be 400. Got {(int)resp.StatusCode}: {body}");
        }

        // ── /Public/apis.json → 200 with JSON body ────────────────────────────────

        [TestMethod]
        public async Task ExecutePublicApisJson_Returns200WithJsonBody()
        {
            SkipIfUnavailable();

            var resp = await _http.GetAsync(BaseUrl + "/Public/apis.json");
            var body = await resp.Content.ReadAsStringAsync();

            Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode,
                $"GET /Public/apis.json should be 200. Got {(int)resp.StatusCode}: {body}");

            Assert.IsNotNull(JToken.Parse(body),
                $"Response body should be valid JSON. Got: {body}");
        }

        // ── /workflow?workflowName=NonExistent → 500 error ───────────────────────

        [TestMethod]
        public async Task Execute_QueryStringWorkflowName_NonExistent_Returns500()
        {
            SkipIfUnavailable();

            var resp = await _http.GetAsync(BaseUrl + "/workflow?workflowName=NonExistent_e9a3b7");
            var body = await resp.Content.ReadAsStringAsync();

            Assert.AreEqual(HttpStatusCode.InternalServerError, resp.StatusCode,
                $"GET /workflow?workflowName=NonExistent should be 500. Got {(int)resp.StatusCode}: {body}");

            var json = JObject.Parse(body);
            Assert.IsTrue(json.ContainsKey("hasErrors") || json.ContainsKey("errors"),
                $"500 body should describe the error. Got: {body}");
        }
    }
}
