/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Unit tests for EditWorkflowTool: required-parameter/validation delegation to
 *  ValidateWorkflowTool, not-found rejection for a name that doesn't already
 *  exist, Contribute permission gating (mirrors CreateWorkflowTool), the
 *  success path (in-place overwrite preserving Service ID, incrementing
 *  VersionNumber), and the httpEndpoints field added alongside
 *  CreateWorkflowTool's (see WorkflowHttpEndpointResolver).
 */

using Dev2.Common.X6;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModelContextProtocol;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using Warewolf.Execution.Lightweight.Auth;
using Warewolf.Execution.Lightweight.Auth.Models;
using Warewolf.Execution.Lightweight.Infrastructure;
using Warewolf.Execution.Lightweight.Mcp.ToolHandlers;

namespace Warewolf.Execution.Lightweight.Tests.Mcp.ToolHandlers
{
    [TestClass]
    public class EditWorkflowToolTests
    {
        private string _root = string.Empty;

        [TestInitialize]
        public void Setup() => _root = Directory.CreateDirectory(
            Path.Combine(Path.GetTempPath(), "edit-wf-tests-" + Guid.NewGuid().ToString("N"))).FullName;

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

        // ── Fixture builders (mirrors CreateWorkflowToolTests') ──────────

        static Cell MakeStartNode(string id = "start") =>
            new() { id = id, shape = "rect", data = new Dictionary<string, object> { ["type"] = Constants.START } };

        static Cell MakeAssign(string id, string displayName, JArray fields) =>
            new()
            {
                id = id,
                shape = "rect",
                data = new Dictionary<string, object>
                {
                    ["type"] = "dsfdotnetmultiassignactivity",
                    ["displayName"] = displayName,
                    ["fields"] = fields,
                }
            };

        static Cell MakeEdge(string id, string sourceId, string targetId) =>
            new() { id = id, shape = "edge", data = new Dictionary<string, object>(), Source = new Connector(sourceId), Target = new Connector(targetId) };

        static JsonElement BodyOf(string resourceName, params Cell[] cells)
        {
            var graph = new X6WorkflowSaveModel { ResourceName = resourceName, Cells = new List<Cell>(cells) };
            return JsonDocument.Parse(JsonConvert.SerializeObject(graph)).RootElement;
        }

        static JsonElement EnvelopeOf(object envelopeObj) => System.Text.Json.JsonSerializer.SerializeToElement(envelopeObj);

        static readonly JsonElement EmptyEnvelope = EnvelopeOf(new { inputs = Array.Empty<object>(), outputs = Array.Empty<object>() });

        static JsonElement ValidBody(string resourceName = "ExistingWorkflow")
        {
            var fields = new JArray(new JObject
            {
                ["FieldName"] = "[[Result]]",
                ["FieldValue"] = "hello",
                ["IndexNumber"] = 1
            });
            var assign = MakeAssign("assign1", "Assign", fields);
            return BodyOf(resourceName, MakeStartNode(), assign, MakeEdge("e1", "start", "assign1"));
        }

        static JsonElement ValidEnvelope(string name = "ExistingWorkflow") => EnvelopeOf(new
        {
            name,
            description = "An edited workflow",
            inputs = Array.Empty<object>(),
            outputs = new[] { new { name = "Result", kind = "scalar", fields = Array.Empty<string>() } },
        });

        private static EditWorkflowResult Handle(
            HostEnvironmentConfig hostConfig,
            IWorkflowAuthPolicyLoader authPolicyLoader,
            ClaimsPrincipal? user,
            string name,
            JsonElement envelope,
            JsonElement body) =>
            EditWorkflowTool.Handle(hostConfig, authPolicyLoader, user, name, envelope, body);

        /// <summary>Seeds an existing .bite file the way create_workflow's own writer would.</summary>
        private static CreateWorkflowResult SeedExisting(HostEnvironmentConfig hostConfig, string name) =>
            CreateWorkflowTool.Handle(hostConfig, new StubAuthPolicyLoader { IsConfigEffective = false }, null,
                name, ValidEnvelope(name), ValidBody(name));

        // ── Tests: input validation ────────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public void Handle_BlankName_Throws()
        {
            Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false }, null, "   ", ValidEnvelope(), ValidBody());
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public void Handle_InvalidBody_NoStartNode_ThrowsWithoutWriting()
        {
            var hostConfig = HostConfig();
            SeedExisting(hostConfig, "Broken");
            var body = BodyOf("Broken", MakeAssign("assign1", "Assign", new JArray()));

            Handle(hostConfig, new StubAuthPolicyLoader { IsConfigEffective = false }, null, "Broken", EmptyEnvelope, body);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public void Handle_NameDoesNotExist_Throws()
        {
            Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false }, null, "NeverCreated", ValidEnvelope(), ValidBody());
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public void Handle_SecureConfigEffective_NoContributePermission_ThrowsPermissionDenied()
        {
            var hostConfig = HostConfig();
            SeedExisting(hostConfig, "NoPermission");

            var loader = new StubAuthPolicyLoader
            {
                IsConfigEffective = true,
                EffectivePermissions = (_, _) => WorkflowPermission.View,
            };

            Handle(hostConfig, loader, Principal("Developers"), "NoPermission", ValidEnvelope(), ValidBody());
        }

        // ── Tests: success path ────────────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_ValidEnvelopeAndBody_OverwritesFile_ReturnsUpdatedTrue()
        {
            var hostConfig = HostConfig();
            SeedExisting(hostConfig, "ExistingWorkflow");

            var result = Handle(hostConfig, new StubAuthPolicyLoader { IsConfigEffective = false }, null,
                "ExistingWorkflow", ValidEnvelope(), ValidBody());

            Assert.AreEqual("ExistingWorkflow", result.Name);
            Assert.IsTrue(result.Updated);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_Edit_PreservesServiceId_AndIncrementsVersionNumber()
        {
            var hostConfig = HostConfig();
            SeedExisting(hostConfig, "Versioned");
            var filePath = Path.Combine(hostConfig.WorkflowsDirectory, "Versioned.bite");
            var originalId = System.Xml.Linq.XDocument.Parse(File.ReadAllText(filePath)).Root!.Attribute("ID")!.Value;

            Handle(hostConfig, new StubAuthPolicyLoader { IsConfigEffective = false }, null, "Versioned", ValidEnvelope(), ValidBody());

            var updated = System.Xml.Linq.XDocument.Parse(File.ReadAllText(filePath)).Root!;
            Assert.AreEqual(originalId, updated.Attribute("ID")!.Value, "edit_workflow must preserve the existing Service ID.");
            Assert.AreEqual("2", updated.Element("VersionInfo")!.Attribute("VersionNumber")!.Value);
        }

        // ── Tests: httpEndpoints (mirrors CreateWorkflowToolTests') ────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_ConfigMissing_NoBypassFlag_ReturnsNoUrls()
        {
            var hostConfig = HostConfig();
            SeedExisting(hostConfig, "NoConfigWf");

            var previous = Environment.GetEnvironmentVariable("BYPASS_SECURE_CONFIG");
            Environment.SetEnvironmentVariable("BYPASS_SECURE_CONFIG", null);
            try
            {
                var result = Handle(hostConfig, new StubAuthPolicyLoader { IsConfigEffective = false }, null, "NoConfigWf", ValidEnvelope(), ValidBody());

                Assert.IsNull(result.HttpEndpoints.PublicUrl,
                    "secure.config missing and BYPASS_SECURE_CONFIG not set: /Public/* is denied with a 500, so publicUrl must be omitted.");
                Assert.IsNull(result.HttpEndpoints.SecureUrl);
            }
            finally
            {
                Environment.SetEnvironmentVariable("BYPASS_SECURE_CONFIG", previous);
            }
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_ConfigMissing_BypassFlagSet_ReturnsPublicUrlOnly()
        {
            var hostConfig = HostConfig();
            SeedExisting(hostConfig, "BypassWf");

            var previous = Environment.GetEnvironmentVariable("BYPASS_SECURE_CONFIG");
            Environment.SetEnvironmentVariable("BYPASS_SECURE_CONFIG", "true");
            try
            {
                var result = Handle(hostConfig, new StubAuthPolicyLoader { IsConfigEffective = false }, null, "BypassWf", ValidEnvelope(), ValidBody());

                Assert.AreEqual("/Public/BypassWf", result.HttpEndpoints.PublicUrl);
                Assert.IsNull(result.HttpEndpoints.SecureUrl);
            }
            finally
            {
                Environment.SetEnvironmentVariable("BYPASS_SECURE_CONFIG", previous);
            }
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_SecureConfigEffective_PublicGroupNotGranted_ReturnsSecureUrlOnly()
        {
            var hostConfig = HostConfig();
            SeedExisting(hostConfig, "SecureOnlyWf");

            var loader = new StubAuthPolicyLoader
            {
                IsConfigEffective = true,
                EffectivePermissions = (_, roles) =>
                    roles.Any() ? WorkflowPermission.Contribute : WorkflowPermission.None,
            };

            var result = Handle(hostConfig, loader, Principal("Developers"), "SecureOnlyWf", ValidEnvelope(), ValidBody());

            Assert.AreEqual("/Secure/SecureOnlyWf", result.HttpEndpoints.SecureUrl);
            Assert.IsNull(result.HttpEndpoints.PublicUrl);
        }
    }
}
