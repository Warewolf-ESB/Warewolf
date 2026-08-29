/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Unit tests for EditSourceTool (edit_source): required-parameter/sourceType
 *  validation, source-not-found rejection, Contribute permission gating
 *  (reusing ListWorkflowsTool's generalised rule), unrecognised/missing config
 *  field errors, "${NAME}" secret-reference substitution via IMcpSecretResolver
 *  (success + unresolved error), Source ID / VersionNumber preservation across
 *  the edit, and the success path for every supported sourceType — a rewritten
 *  .bite file with an encrypted connection string and no plaintext secret on
 *  disk, whose response never echoes the secret back.
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
using Dev2.Common;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Warewolf.Execution.Lightweight.Auth;
using Warewolf.Execution.Lightweight.Auth.Models;
using Warewolf.Execution.Lightweight.Infrastructure;
using Warewolf.Execution.Lightweight.Mcp.Secrets;
using Warewolf.Execution.Lightweight.Mcp.ToolHandlers;

namespace Warewolf.Execution.Lightweight.Tests.Mcp.ToolHandlers
{
    [TestClass]
    public class EditSourceToolTests
    {
        private string _root = string.Empty;

        [TestInitialize]
        public void Setup() => _root = Directory.CreateDirectory(
            Path.Combine(Path.GetTempPath(), "edit-source-tests-" + Guid.NewGuid().ToString("N"))).FullName;

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(_root))
            {
                Directory.Delete(_root, recursive: true);
            }
        }

        // ── Test doubles (matches AddSourceToolTests' conventions) ──

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

        private static Task<EditSourceResult> Handle(
            HostEnvironmentConfig hostConfig,
            IWorkflowAuthPolicyLoader authPolicyLoader,
            ClaimsPrincipal? user,
            string name,
            string sourceType,
            JsonElement config,
            IMcpSecretResolver? secretResolver = null) =>
            EditSourceTool.Handle(hostConfig, authPolicyLoader, secretResolver ?? NoSecrets, user, name, sourceType, config);

        /// <summary>Seeds an existing source .bite file at <paramref name="relativeName"/>, as add_source would have written it.</summary>
        private async Task<string> SeedExistingSourceAsync(string relativeName = "ExistingSource", string sourceType = "Redis", object? config = null)
        {
            var result = await AddSourceTool.Handle(HostConfig(), OpenPolicy, NoSecrets, null, relativeName, sourceType,
                ConfigOf(config ?? new { HostName = "localhost" }));
            Assert.IsTrue(result.Created);
            return Path.Combine(_root, relativeName + ".bite");
        }

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
            await SeedExistingSourceAsync();
            await Handle(HostConfig(), OpenPolicy, null, "ExistingSource", "   ", ConfigOf(new { HostName = "localhost" }));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public async Task Handle_UnsupportedSourceType_Throws()
        {
            await SeedExistingSourceAsync();
            await Handle(HostConfig(), OpenPolicy, null, "ExistingSource", "MongoDb", ConfigOf(new { HostName = "localhost" }));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public async Task Handle_SourceNotFound_Throws()
        {
            await Handle(HostConfig(), OpenPolicy, null, "DoesNotExist", "Redis", ConfigOf(new { HostName = "localhost" }));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public async Task Handle_ConfigNotAnObject_Throws()
        {
            await SeedExistingSourceAsync();
            await Handle(HostConfig(), OpenPolicy, null, "ExistingSource", "Redis", JsonSerializer.SerializeToElement("not-an-object"));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public async Task Handle_UnrecognisedConfigField_Throws()
        {
            await SeedExistingSourceAsync();
            await Handle(HostConfig(), OpenPolicy, null, "ExistingSource", "Redis", ConfigOf(new { HostName = "localhost", NotAField = "x" }));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public async Task Handle_MissingRequiredField_Throws()
        {
            await SeedExistingSourceAsync();
            // Redis requires HostName.
            await Handle(HostConfig(), OpenPolicy, null, "ExistingSource", "Redis", ConfigOf(new { Port = 6379 }));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public async Task Handle_SqlDatabase_UserAuthWithoutPassword_Throws()
        {
            await SeedExistingSourceAsync();
            await Handle(HostConfig(), OpenPolicy, null, "ExistingSource", "SqlDatabase",
                ConfigOf(new { Server = "localhost", DatabaseName = "db", AuthenticationType = "User", UserID = "sa" }));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public async Task Handle_SecureConfigEffective_NoContributePermission_ThrowsPermissionDenied()
        {
            await SeedExistingSourceAsync("NoPermission");

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
            await SeedExistingSourceAsync();
            await Handle(HostConfig(), OpenPolicy, null, "ExistingSource", "Redis",
                ConfigOf(new { HostName = "localhost", AuthenticationType = "Password", Password = "${does-not-exist}" }));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_SecretPlaceholder_ResolvesViaResolver_AndIsReportedByName()
        {
            await SeedExistingSourceAsync("SecretSource");
            var resolver = new FakeSecretResolver().With("redis-pwd", "s3cr3t-value");

            var result = await Handle(HostConfig(), OpenPolicy, null, "SecretSource", "Redis",
                ConfigOf(new { HostName = "localhost", AuthenticationType = "Password", Password = "${redis-pwd}" }),
                resolver);

            CollectionAssert.Contains(result.ResolvedSecretFields.ToList(), "Password");

            var contents = File.ReadAllText(Path.Combine(_root, "SecretSource.bite"));
            StringAssert.DoesNotMatch(contents, new System.Text.RegularExpressions.Regex("s3cr3t-value"));
            StringAssert.DoesNotMatch(contents, new System.Text.RegularExpressions.Regex(@"\$\{redis-pwd\}"));
        }

        // ── Tests: success path / written-file shape ───────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_ValidConfig_OverwritesFile_ReturnsUpdatedTrue()
        {
            await SeedExistingSourceAsync("ExistingRedisSource");

            var result = await Handle(HostConfig(), OpenPolicy, null, "ExistingRedisSource", "Redis", ConfigOf(new { HostName = "otherhost" }));

            Assert.AreEqual("ExistingRedisSource", result.Name);
            Assert.AreEqual("Redis", result.SourceType);
            Assert.IsTrue(result.Updated);
            Assert.IsTrue(File.Exists(Path.Combine(_root, "ExistingRedisSource.bite")));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_SecureConfigEffective_WithContributePermission_Succeeds()
        {
            await SeedExistingSourceAsync("Allowed");

            var loader = new StubAuthPolicyLoader
            {
                IsConfigEffective = true,
                EffectivePermissions = (path, _) =>
                    path.Equals("Allowed", StringComparison.OrdinalIgnoreCase)
                        ? WorkflowPermission.Contribute
                        : WorkflowPermission.None,
            };

            var result = await Handle(HostConfig(), loader, Principal("Developers"), "Allowed", "Redis", ConfigOf(new { HostName = "localhost" }));

            Assert.IsTrue(result.Updated);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_PreservesSourceId_AndIncrementsVersionNumber()
        {
            var path = await SeedExistingSourceAsync("VersionedSource");
            var originalId = XElement.Load(path).Attribute("ID")!.Value;

            await Handle(HostConfig(), OpenPolicy, null, "VersionedSource", "Redis", ConfigOf(new { HostName = "otherhost" }));

            var updated = XElement.Load(path);
            Assert.AreEqual(originalId, updated.Attribute("ID")!.Value);
            Assert.AreEqual("2", updated.Element("VersionInfo")!.Attribute("VersionNumber")!.Value);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_CanChangeSourceType_AcrossEdit()
        {
            await SeedExistingSourceAsync("RetypedSource", "Redis", new { HostName = "localhost" });

            var result = await Handle(HostConfig(), OpenPolicy, null, "RetypedSource", "SqlDatabase",
                ConfigOf(new { Server = "myserver", DatabaseName = "mydb" }));

            Assert.AreEqual("SqlDatabase", result.SourceType);

            var contents = File.ReadAllText(Path.Combine(_root, "RetypedSource.bite"));
            var source = XDocument.Parse(contents).Root!;
            Assert.AreEqual("SqlDatabase", source.Attribute("ResourceType")!.Value);
            Assert.AreEqual("DbSource", source.Attribute("Type")!.Value);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_WrittenFile_ContainsExpectedSourceShape_AndEncryptedConnectionString()
        {
            await SeedExistingSourceAsync("ShapedSource", "SqlDatabase", new { Server = "myserver", DatabaseName = "mydb" });

            await Handle(HostConfig(), OpenPolicy, null, "ShapedSource", "SqlDatabase",
                ConfigOf(new { Server = "myserver2", DatabaseName = "mydb2" }));

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

            var connectionString = source.Attribute("ConnectionString")!.Value;
            Assert.IsFalse(string.IsNullOrWhiteSpace(connectionString));
            StringAssert.DoesNotMatch(connectionString, new System.Text.RegularExpressions.Regex("Data Source=myserver2"));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_Response_NeverContainsConnectionStringOrSecret()
        {
            await SeedExistingSourceAsync("SecretiveSource", "Email", new { Host = "smtp.example.com", UserName = "bot", Password = "oldliteral" });

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
            await SeedExistingSourceAsync("MySqlSource", "MySqlDatabase", new { Server = "db.local", DatabaseName = "app", UserID = "root", Password = "pw" });

            var result = await Handle(HostConfig(), OpenPolicy, null, "MySqlSource", "MySqlDatabase",
                ConfigOf(new { Server = "db.local2", DatabaseName = "app", UserID = "root", Password = "pw" }));

            Assert.IsTrue(result.Updated);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_PostgreSQL_Succeeds()
        {
            await SeedExistingSourceAsync("PgSource", "PostgreSQL", new { Server = "db.local", DatabaseName = "app", UserID = "postgres", Password = "pw" });

            var result = await Handle(HostConfig(), OpenPolicy, null, "PgSource", "PostgreSQL",
                ConfigOf(new { Server = "db.local2", DatabaseName = "app", UserID = "postgres", Password = "pw" }));

            Assert.IsTrue(result.Updated);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_Oracle_Succeeds()
        {
            await SeedExistingSourceAsync("OracleSource", "Oracle", new { Server = "db.local", UserID = "sys", Password = "pw" });

            var result = await Handle(HostConfig(), OpenPolicy, null, "OracleSource", "Oracle",
                ConfigOf(new { Server = "db.local2", UserID = "sys", Password = "pw" }));

            Assert.IsTrue(result.Updated);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_ODBC_Succeeds()
        {
            await SeedExistingSourceAsync("OdbcSource", "ODBC", new { DatabaseName = "MyDsn" });

            var result = await Handle(HostConfig(), OpenPolicy, null, "OdbcSource", "ODBC", ConfigOf(new { DatabaseName = "MyDsn2" }));

            Assert.IsTrue(result.Updated);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_RabbitMQ_Succeeds()
        {
            await SeedExistingSourceAsync("RabbitSource", "RabbitMQ", new { HostName = "broker.local", UserName = "guest", Password = "guest" });

            var result = await Handle(HostConfig(), OpenPolicy, null, "RabbitSource", "RabbitMQ",
                ConfigOf(new { HostName = "broker.local2", UserName = "guest", Password = "guest" }));

            Assert.IsTrue(result.Updated);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_Email_Succeeds()
        {
            await SeedExistingSourceAsync("EmailSource", "Email", new { Host = "smtp.local", UserName = "bot", Password = "pw" });

            var result = await Handle(HostConfig(), OpenPolicy, null, "EmailSource", "Email",
                ConfigOf(new { Host = "smtp.local2", UserName = "bot", Password = "pw" }));

            Assert.IsTrue(result.Updated);
        }

        // ── Fix for: an edited source kept serving its OLD connection details indefinitely on any
        // instance that had already loaded it once — EnsureSourceLoaded's per-ID "already
        // registered" flag short-circuits without re-reading the file, and Handle never cleared it.

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_EditedSource_IsImmediatelyReflectedByLightweightSourceLoader_EvenIfAlreadyLoadedOnce()
        {
            var path = await SeedExistingSourceAsync("WebToEdit", "Web", new { Address = "https://old-host.example.com" });
            var resourceId = Guid.Parse(XDocument.Parse(File.ReadAllText(path)).Root!.Attribute("ID")!.Value);

            // Simulate a warm instance that already resolved this source once before the edit.
            LightweightSourceLoader.Instance.EnsureIndexed(_root);
            Assert.IsTrue(((IOnDemandSourceLoader)LightweightSourceLoader.Instance).EnsureSourceLoaded(resourceId));

            await Handle(HostConfig(), OpenPolicy, null, "WebToEdit", "Web", ConfigOf(new { Address = "https://new-host.example.com" }));

            Assert.IsTrue(((IOnDemandSourceLoader)LightweightSourceLoader.Instance).EnsureSourceLoaded(resourceId),
                "The edited source must still resolve (it was not deleted, only reloaded).");

            var reloaded = ResourceCatalog.Instance.WorkspaceResources.TryGetValue(GlobalConstants.ServerWorkspaceID, out var ws)
                ? ws.OfType<WebSource>().FirstOrDefault(r => r.ResourceID == resourceId)
                : null;
            Assert.IsNotNull(reloaded, "The edited source should still be registered in ResourceCatalog.");
            Assert.AreEqual("https://new-host.example.com", reloaded!.Address,
                "Reflects the fix: without it, this would still read the pre-edit 'https://old-host.example.com'.");
        }
    }
}
