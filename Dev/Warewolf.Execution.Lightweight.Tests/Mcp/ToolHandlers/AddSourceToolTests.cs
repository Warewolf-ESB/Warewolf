/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Unit tests for AddSourceTool (add_source): required-parameter/sourceType
 *  validation, name-already-exists rejection, Contribute permission gating
 *  (reusing ListWorkflowsTool's generalised rule), unrecognised/missing config
 *  field errors, "${NAME}" secret-reference substitution via IMcpSecretResolver
 *  (success + unresolved error), and the success path for every supported
 *  sourceType — a written .bite file with an encrypted connection string and
 *  no plaintext secret on disk, whose response never echoes the secret back.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModelContextProtocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Warewolf.Execution.Lightweight.Auth;
using Warewolf.Execution.Lightweight.Auth.Models;
using Warewolf.Execution.Lightweight.Infrastructure;
using Warewolf.Execution.Lightweight.Mcp.Secrets;
using Warewolf.Execution.Lightweight.Mcp.ToolHandlers;

namespace Warewolf.Execution.Lightweight.Tests.Mcp.ToolHandlers
{
    [TestClass]
    public class AddSourceToolTests
    {
        private string _root = string.Empty;

        [TestInitialize]
        public void Setup() => _root = Directory.CreateDirectory(
            Path.Combine(Path.GetTempPath(), "add-source-tests-" + Guid.NewGuid().ToString("N"))).FullName;

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(_root))
            {
                Directory.Delete(_root, recursive: true);
            }
        }

        // ── Test doubles (matches CreateWorkflowToolTests' conventions) ──

        private sealed class StubAuthPolicyLoader : IWorkflowAuthPolicyLoader
        {
            public bool IsConfigEffective { get; set; }
            public int PolicyCount => 0;
            public PolicyLookupResult GetPolicy(string workflowName) => PolicyLookupResult.ConfigMissing();

            public Func<string, IEnumerable<string>, WorkflowPermission> EffectivePermissions { get; set; } =
                (_, _) => WorkflowPermission.None;

            public WorkflowPermission GetEffectivePermissions(string workflowName, IEnumerable<string> callerRoles) =>
                EffectivePermissions(workflowName, callerRoles);

            public void Reload() { }
        }

        /// <summary>
        /// In-memory stand-in for a production secret store (Key Vault) or the local-dev
        /// environment-variable fallback — lets tests exercise "${NAME}" resolution without
        /// touching real environment variables or requiring live Azure access.
        /// </summary>
        private sealed class FakeSecretResolver : IMcpSecretResolver
        {
            private readonly Dictionary<string, string> _secrets = new(StringComparer.Ordinal);

            public FakeSecretResolver With(string name, string value)
            {
                _secrets[name] = value;
                return this;
            }

            public Task<string> ResolveAsync(string name, CancellationToken cancellationToken) =>
                _secrets.TryGetValue(name, out var value)
                    ? Task.FromResult(value)
                    : throw new McpException($"Secret reference '${{{name}}}' could not be resolved: no secret named '{name}' is staged.");
        }

        private static WorkflowClaimsPrincipal Principal(params string[] roles) =>
            new(new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.Name, "alice") }
                    .Concat(roles.Select(r => new Claim(ClaimTypes.Role, r))),
                "Bearer", ClaimTypes.Name, ClaimTypes.Role));

        private HostEnvironmentConfig HostConfig()
        {
            Environment.SetEnvironmentVariable("WorkflowsDirectory", _root);
            try
            {
                return HostEnvironmentConfig.Load();
            }
            finally
            {
                Environment.SetEnvironmentVariable("WorkflowsDirectory", null);
            }
        }

        static JsonElement ConfigOf(object obj) => JsonSerializer.SerializeToElement(obj);

        static readonly StubAuthPolicyLoader OpenPolicy = new() { IsConfigEffective = false };
        static readonly FakeSecretResolver NoSecrets = new();

        private static Task<AddSourceResult> Handle(
            HostEnvironmentConfig hostConfig,
            IWorkflowAuthPolicyLoader authPolicyLoader,
            ClaimsPrincipal? user,
            string name,
            string sourceType,
            JsonElement config,
            IMcpSecretResolver? secretResolver = null) =>
            AddSourceTool.Handle(hostConfig, authPolicyLoader, secretResolver ?? NoSecrets, user, name, sourceType, config);

        // ── Tests: input validation ────────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public async Task Handle_BlankName_Throws()
        {
            await Handle(HostConfig(), OpenPolicy, null, "   ", "Redis", ConfigOf(new { HostName = "localhost" }));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public async Task Handle_BlankSourceType_Throws()
        {
            await Handle(HostConfig(), OpenPolicy, null, "NewSource", "   ", ConfigOf(new { HostName = "localhost" }));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public async Task Handle_UnsupportedSourceType_Throws()
        {
            await Handle(HostConfig(), OpenPolicy, null, "NewSource", "MongoDb", ConfigOf(new { HostName = "localhost" }));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public async Task Handle_ConfigNotAnObject_Throws()
        {
            await Handle(HostConfig(), OpenPolicy, null, "NewSource", "Redis", JsonSerializer.SerializeToElement("not-an-object"));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public async Task Handle_UnrecognisedConfigField_Throws()
        {
            await Handle(HostConfig(), OpenPolicy, null, "NewSource", "Redis", ConfigOf(new { HostName = "localhost", NotAField = "x" }));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public async Task Handle_MissingRequiredField_Throws()
        {
            // Redis requires HostName.
            await Handle(HostConfig(), OpenPolicy, null, "NewSource", "Redis", ConfigOf(new { Port = 6379 }));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public async Task Handle_SqlDatabase_UserAuthWithoutPassword_Throws()
        {
            await Handle(HostConfig(), OpenPolicy, null, "NewSource", "SqlDatabase",
                ConfigOf(new { Server = "localhost", DatabaseName = "db", AuthenticationType = "User", UserID = "sa" }));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public async Task Handle_NameAlreadyExists_Throws()
        {
            File.WriteAllText(Path.Combine(_root, "Existing.bite"),
                "<Source Name=\"Existing\" ResourceType=\"RedisSource\"></Source>");

            await Handle(HostConfig(), OpenPolicy, null, "Existing", "Redis", ConfigOf(new { HostName = "localhost" }));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public async Task Handle_SecureConfigEffective_NoContributePermission_ThrowsPermissionDenied()
        {
            var loader = new StubAuthPolicyLoader
            {
                IsConfigEffective = true,
                EffectivePermissions = (_, _) => WorkflowPermission.View,
            };

            await Handle(HostConfig(), loader, Principal("Developers"), "NoPermission", "Redis", ConfigOf(new { HostName = "localhost" }));
        }

        // ── Tests: "${NAME}" secret-reference substitution ─────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public async Task Handle_UnresolvedSecretPlaceholder_Throws()
        {
            await Handle(HostConfig(), OpenPolicy, null, "NewSource", "Redis",
                ConfigOf(new { HostName = "localhost", AuthenticationType = "Password", Password = "${does-not-exist}" }));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_SecretPlaceholder_ResolvesViaResolver_AndIsReportedByName()
        {
            var resolver = new FakeSecretResolver().With("redis-pwd", "s3cr3t-value");

            var result = await Handle(HostConfig(), OpenPolicy, null, "SecretSource", "Redis",
                ConfigOf(new { HostName = "localhost", AuthenticationType = "Password", Password = "${redis-pwd}" }),
                resolver);

            CollectionAssert.Contains(result.ResolvedSecretFields.ToList(), "Password");

            var contents = File.ReadAllText(Path.Combine(_root, "SecretSource.bite"));
            StringAssert.DoesNotMatch(contents, new System.Text.RegularExpressions.Regex("s3cr3t-value"));
            StringAssert.DoesNotMatch(contents, new System.Text.RegularExpressions.Regex(@"\$\{redis-pwd\}"));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_SecretResolver_IsNeverCalledWithLiteralConfigValue()
        {
            // The resolver only ever receives the placeholder NAME, never the literal value a
            // caller might mistakenly put in `config` — guards against a resolver accidentally
            // being handed something that looks like a secret.
            var seenNames = new List<string>();
            var resolver = new RecordingResolver(seenNames, "resolved-value");

            await Handle(HostConfig(), OpenPolicy, null, "RecordedSource", "Redis",
                ConfigOf(new { HostName = "localhost", AuthenticationType = "Password", Password = "${my-secret-name}" }),
                resolver);

            CollectionAssert.AreEqual(new[] { "my-secret-name" }, seenNames);
        }

        private sealed class RecordingResolver : IMcpSecretResolver
        {
            private readonly List<string> _seenNames;
            private readonly string _value;

            public RecordingResolver(List<string> seenNames, string value)
            {
                _seenNames = seenNames;
                _value = value;
            }

            public Task<string> ResolveAsync(string name, CancellationToken cancellationToken)
            {
                _seenNames.Add(name);
                return Task.FromResult(_value);
            }
        }

        // ── Tests: success path / written-file shape ───────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_ValidConfig_WritesFile_ReturnsCreatedTrue()
        {
            var result = await Handle(HostConfig(), OpenPolicy, null, "NewRedisSource", "Redis", ConfigOf(new { HostName = "localhost" }));

            Assert.AreEqual("NewRedisSource", result.Name);
            Assert.AreEqual("Redis", result.SourceType);
            Assert.IsTrue(result.Created);
            Assert.IsTrue(File.Exists(Path.Combine(_root, "NewRedisSource.bite")));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_SecureConfigEffective_WithContributePermission_Succeeds()
        {
            var loader = new StubAuthPolicyLoader
            {
                IsConfigEffective = true,
                EffectivePermissions = (path, _) =>
                    path.Equals("Allowed", StringComparison.OrdinalIgnoreCase)
                        ? WorkflowPermission.Contribute
                        : WorkflowPermission.None,
            };

            var result = await Handle(HostConfig(), loader, Principal("Developers"), "Allowed", "Redis", ConfigOf(new { HostName = "localhost" }));

            Assert.IsTrue(result.Created);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_WrittenFile_ContainsExpectedSourceShape_AndEncryptedConnectionString()
        {
            await Handle(HostConfig(), OpenPolicy, null, "ShapedSource", "SqlDatabase",
                ConfigOf(new { Server = "myserver", DatabaseName = "mydb" }));

            var contents = File.ReadAllText(Path.Combine(_root, "ShapedSource.bite"));
            var doc = XDocument.Parse(contents);
            var source = doc.Root!;

            Assert.AreEqual("Source", source.Name.LocalName);
            Assert.AreEqual("SqlDatabase", source.Attribute("ResourceType")!.Value);
            Assert.AreEqual("SqlDatabase", source.Attribute("ServerType")!.Value);
            Assert.AreEqual("DbSource", source.Attribute("Type")!.Value);
            Assert.AreEqual("false", source.Attribute("IsValid")!.Value);
            Assert.IsFalse(string.IsNullOrWhiteSpace(source.Attribute("ID")!.Value));
            Assert.AreEqual("ShapedSource", source.Attribute("Name")!.Value);
            Assert.AreEqual("ShapedSource", source.Element("DisplayName")!.Value);
            Assert.AreEqual("Save", source.Element("VersionInfo")!.Attribute("Reason")!.Value);
            Assert.AreEqual("1", source.Element("VersionInfo")!.Attribute("VersionNumber")!.Value);

            var connectionString = source.Attribute("ConnectionString")!.Value;
            Assert.IsFalse(string.IsNullOrWhiteSpace(connectionString));
            StringAssert.DoesNotMatch(connectionString, new System.Text.RegularExpressions.Regex("Data Source=myserver"));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_Response_NeverContainsConnectionStringOrSecret()
        {
            var result = await Handle(HostConfig(), OpenPolicy, null, "SecretiveSource", "Email",
                ConfigOf(new { Host = "smtp.example.com", UserName = "bot", Password = "hunter2literal" }));

            var serialized = JsonSerializer.Serialize(result);
            StringAssert.DoesNotMatch(serialized, new System.Text.RegularExpressions.Regex("hunter2literal"));
            StringAssert.DoesNotMatch(serialized, new System.Text.RegularExpressions.Regex("smtp.example.com"));
        }

        // ── Per-sourceType success coverage ─────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_MySqlDatabase_Succeeds()
        {
            var result = await Handle(HostConfig(), OpenPolicy, null, "MySqlSource", "MySqlDatabase",
                ConfigOf(new { Server = "db.local", DatabaseName = "app", UserID = "root", Password = "pw" }));

            Assert.IsTrue(result.Created);
            Assert.IsTrue(File.Exists(Path.Combine(_root, "MySqlSource.bite")));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_PostgreSQL_Succeeds()
        {
            var result = await Handle(HostConfig(), OpenPolicy, null, "PgSource", "PostgreSQL",
                ConfigOf(new { Server = "db.local", DatabaseName = "app", UserID = "postgres", Password = "pw" }));

            Assert.IsTrue(result.Created);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_Oracle_Succeeds()
        {
            var result = await Handle(HostConfig(), OpenPolicy, null, "OracleSource", "Oracle",
                ConfigOf(new { Server = "db.local", UserID = "sys", Password = "pw" }));

            Assert.IsTrue(result.Created);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_ODBC_Succeeds()
        {
            var result = await Handle(HostConfig(), OpenPolicy, null, "OdbcSource", "ODBC",
                ConfigOf(new { DatabaseName = "MyDsn" }));

            Assert.IsTrue(result.Created);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_RabbitMQ_Succeeds()
        {
            var result = await Handle(HostConfig(), OpenPolicy, null, "RabbitSource", "RabbitMQ",
                ConfigOf(new { HostName = "broker.local", UserName = "guest", Password = "guest" }));

            Assert.IsTrue(result.Created);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_Email_Succeeds()
        {
            var result = await Handle(HostConfig(), OpenPolicy, null, "EmailSource", "Email",
                ConfigOf(new { Host = "smtp.local", UserName = "bot", Password = "pw" }));

            Assert.IsTrue(result.Created);
        }
    }
}

