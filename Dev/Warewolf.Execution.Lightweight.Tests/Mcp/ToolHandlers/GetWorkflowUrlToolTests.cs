/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Unit tests for GetWorkflowUrlTool: name resolution/not-found, View
 *  permission gating (reusing ListWorkflowsTool's rule), the secure.config-
 *  not-effective failure path, and that the response never carries a Public
 *  URL — only /Secure/{name}.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using Warewolf.Execution.Lightweight.Auth;
using Warewolf.Execution.Lightweight.Auth.Models;
using Warewolf.Execution.Lightweight.Infrastructure;
using Warewolf.Execution.Lightweight.Mcp.ToolHandlers;

namespace Warewolf.Execution.Lightweight.Tests.Mcp.ToolHandlers
{
    [TestClass]
    public class GetWorkflowUrlToolTests
    {
        private string _root = string.Empty;

        [TestInitialize]
        public void Setup() => _root = Directory.CreateDirectory(
            Path.Combine(Path.GetTempPath(), "gwu-tests-" + Guid.NewGuid().ToString("N"))).FullName;

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(_root))
            {
                Directory.Delete(_root, recursive: true);
            }
        }

        // ── Test doubles (matches GetWorkflowDefinitionToolTests' conventions) ────

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

        // ── Helpers ────────────────────────────────────────────────────────────

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

        private void WriteWorkflow(string relativePath, string name)
        {
            var fullPath = Path.Combine(_root, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
            File.WriteAllText(fullPath,
                $"<Service Name=\"{name}\" ResourceType=\"WorkflowService\">" +
                "<Comment></Comment><DataList /><Action Name=\"InvokeWorkflow\" Type=\"Workflow\">" +
                "<XamlDefinition></XamlDefinition></Action></Service>");
        }

        private static GetWorkflowUrlResult Handle(
            HostEnvironmentConfig hostConfig,
            IWorkflowAuthPolicyLoader authPolicyLoader,
            ClaimsPrincipal? user,
            string name) =>
            GetWorkflowUrlTool.Handle(hostConfig, authPolicyLoader, user, name);

        // ── Tests: input validation / not-found / permissions ─────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(ModelContextProtocol.McpException))]
        public void Handle_BlankName_Throws()
        {
            Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = true }, null, "   ");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(ModelContextProtocol.McpException))]
        public void Handle_UnknownName_Throws()
        {
            Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = true }, null, "DoesNotExist");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(ModelContextProtocol.McpException))]
        public void Handle_NonWorkflowResourceType_ThrowsNotFound()
        {
            var fullPath = Path.Combine(_root, "NotAWorkflow.bite");
            File.WriteAllText(fullPath, "<Service Name=\"NotAWorkflow\" ResourceType=\"Source\"></Service>");

            Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = true }, null, "NotAWorkflow");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(ModelContextProtocol.McpException))]
        public void Handle_SecureConfigEffective_NoViewPermission_ThrowsPermissionDenied()
        {
            WriteWorkflow("Secret.bite", "Secret");
            var loader = new StubAuthPolicyLoader
            {
                IsConfigEffective = true,
                EffectivePermissions = (_, _) => WorkflowPermission.None,
            };

            Handle(HostConfig(), loader, null, "Secret");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(ModelContextProtocol.McpException))]
        public void Handle_SecureConfigNotEffective_ThrowsBecauseSecureRouteIsUnreachable()
        {
            // /Secure/* always 401s with no secure.config to validate a JWT against, regardless of
            // BYPASS_SECURE_CONFIG — this tool must refuse to hand back a URL that cannot be called,
            // rather than silently falling back to /Public/.
            WriteWorkflow("OpenAccess.bite", "OpenAccess");

            Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false }, null, "OpenAccess");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_SecureConfigEffective_WithViewPermission_ReturnsSecureUrlOnly()
        {
            WriteWorkflow("Visible.bite", "Visible");
            var loader = new StubAuthPolicyLoader
            {
                IsConfigEffective = true,
                EffectivePermissions = (path, _) =>
                    path.Equals("Visible", StringComparison.OrdinalIgnoreCase)
                        ? WorkflowPermission.View
                        : WorkflowPermission.None,
            };

            var result = Handle(HostConfig(), loader, Principal("Developers"), "Visible");

            Assert.AreEqual("Visible", result.Name);
            Assert.AreEqual("/Secure/Visible", result.Url);
            StringAssert.DoesNotMatch(result.Url, new System.Text.RegularExpressions.Regex("^/Public/"));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_NameCanBeResolvedViaNestedFolderPath()
        {
            WriteWorkflow(Path.Combine("Sub", "Nested.bite"), "Nested");
            var loader = new StubAuthPolicyLoader { IsConfigEffective = true, EffectivePermissions = (_, _) => WorkflowPermission.View };

            var result = Handle(HostConfig(), loader, null, "Sub/Nested");

            Assert.AreEqual("Nested", result.Name);
            Assert.AreEqual("/Secure/Sub/Nested", result.Url);
        }
    }
}
