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
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Warewolf.Execution.Lightweight.Auth;
using Warewolf.Execution.Lightweight.Auth.Models;
using Warewolf.Execution.Lightweight.Infrastructure;
using Warewolf.Execution.Lightweight.Mcp;
using Warewolf.Execution.Lightweight.Mcp.Secrets;
using Warewolf.Execution.Lightweight.Mcp.ToolHandlers;
using Warewolf.Execution.Lightweight.Tests.TestSupport;

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

        private HostEnvironmentConfig HostConfig() => McpToolTestHostConfig.ForWorkflowsDirectory(_root);

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

        // ── F2: Web source type ─────────────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SourceCatalog_Web_ResolvesCaseInsensitively_AndIsListed()
        {
            Assert.IsNotNull(SourceCatalog.Resolve("web"));
            Assert.IsNotNull(SourceCatalog.Resolve("WEB"));
            StringAssert.Contains(SourceCatalog.SupportedTypesList, "Web");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_Web_Succeeds()
        {
            var result = await Handle(HostConfig(), OpenPolicy, null, "WebSource1", "Web",
                ConfigOf(new { Address = "https://api.example.com" }));

            Assert.AreEqual("Web", result.SourceType);
            Assert.IsTrue(result.Created);
            Assert.IsTrue(File.Exists(Path.Combine(_root, "WebSource1.bite")));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public async Task Handle_Web_MissingAddress_Throws()
        {
            await Handle(HostConfig(), OpenPolicy, null, "NewSource", "Web", ConfigOf(new { DefaultQuery = "search" }));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public async Task Handle_Web_UserAuthWithoutPassword_Throws()
        {
            await Handle(HostConfig(), OpenPolicy, null, "NewSource", "Web",
                ConfigOf(new { Address = "https://api.example.com", AuthenticationType = "User", UserName = "bob" }));
        }

        /// <summary>
        /// F2 round-trip (the strongest guard): the XML add_source writes must be readable by
        /// WebSource's own XML constructor, preserving Address/DefaultQuery. Also proves the
        /// Windows-default-trap fix: WebSource.ToXml()'s XML constructor falls back to
        /// AuthenticationType.Windows when the connection string's AuthenticationType segment is
        /// missing/unparseable, so an omitted AuthenticationType must still round-trip to
        /// Anonymous (SourceCatalog's Field.Default), never silently becoming Windows-auth.
        /// </summary>
        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_Web_AnonymousAuth_RoundTripsThroughWebSource_DefaultsToAnonymous()
        {
            await Handle(HostConfig(), OpenPolicy, null, "WebRoundTrip", "Web",
                ConfigOf(new { Address = "https://api.example.com/base", DefaultQuery = "?x=1" }));

            var xml = XDocument.Parse(File.ReadAllText(Path.Combine(_root, "WebRoundTrip.bite"))).Root!;
            var webSource = new WebSource(xml);

            Assert.AreEqual("https://api.example.com/base", webSource.Address);
            Assert.AreEqual(AuthenticationType.Anonymous, webSource.AuthenticationType);
            Assert.AreEqual("?x=1", webSource.DefaultQuery);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_Web_UserAuth_RoundTripsThroughWebSource_PreservesCredentials()
        {
            await Handle(HostConfig(), OpenPolicy, null, "WebUserAuth", "Web",
                ConfigOf(new { Address = "https://api.example.com", AuthenticationType = "User", UserName = "bob", Password = "s3cret" }));

            var xml = XDocument.Parse(File.ReadAllText(Path.Combine(_root, "WebUserAuth.bite"))).Root!;
            var webSource = new WebSource(xml);

            Assert.AreEqual("https://api.example.com", webSource.Address);
            Assert.AreEqual(AuthenticationType.User, webSource.AuthenticationType);
            Assert.AreEqual("bob", webSource.UserName);
            Assert.AreEqual("s3cret", webSource.Password);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_Web_WrittenFile_HasWebSourceResourceTypeAndNoServerType()
        {
            await Handle(HostConfig(), OpenPolicy, null, "WebShape", "Web", ConfigOf(new { Address = "https://api.example.com" }));

            var source = XDocument.Parse(File.ReadAllText(Path.Combine(_root, "WebShape.bite"))).Root!;

            Assert.AreEqual("WebSource", source.Attribute("ResourceType")!.Value);
            Assert.AreEqual("WebSource", source.Attribute("Type")!.Value);
            Assert.IsNull(source.Attribute("ServerType"), "Web is not a DbSource entry; it must not get the DB-only ServerType attribute.");
        }

        // ── Fix for: a source created on an instance whose directory index was already built ──
        // (LightweightSourceLoader._directoryIndices is a Lazy per directory, built at most once)
        // was invisible to EnsureSourceLoaded forever — Handle must invalidate that cache.

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_SourceIsImmediatelyResolvableByLightweightSourceLoader_EvenIfDirectoryWasAlreadyIndexed()
        {
            // Simulate a warm instance that already resolved some other source in _root before
            // this call — e.g. an earlier workflow execution — materializing the directory's index.
            LightweightSourceLoader.Instance.EnsureIndexed(_root);
            ((IOnDemandSourceLoader)LightweightSourceLoader.Instance).EnsureSourceLoaded(Guid.NewGuid());

            await Handle(HostConfig(), OpenPolicy, null, "WebImmediate", "Web", ConfigOf(new { Address = "https://api.example.com" }));

            var resourceId = Guid.Parse(XDocument.Parse(File.ReadAllText(Path.Combine(_root, "WebImmediate.bite"))).Root!.Attribute("ID")!.Value);

            Assert.IsTrue(((IOnDemandSourceLoader)LightweightSourceLoader.Instance).EnsureSourceLoaded(resourceId),
                "The newly-created source must be resolvable on its very next EnsureSourceLoaded call, without a process restart.");
        }
    }
}

