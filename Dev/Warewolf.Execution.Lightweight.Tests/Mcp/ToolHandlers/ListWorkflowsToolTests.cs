/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Unit tests for ListWorkflowsTool (warewolf-lee-mcp-v3-spec.md, "Tools" §
 *  list_workflows): enumeration, folder scoping, pagination, per-item View
 *  permission filtering (open-access vs. secure-config-effective modes), and
 *  DataList inputs/outputs/description extraction.
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
    public class ListWorkflowsToolTests
    {
        private string _root = string.Empty;

        [TestInitialize]
        public void Setup() => _root = Directory.CreateDirectory(
            Path.Combine(Path.GetTempPath(), "lwt-tests-" + Guid.NewGuid().ToString("N"))).FullName;

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(_root))
            {
                Directory.Delete(_root, recursive: true);
            }
        }

        // ── Test doubles ──────────────────────────────────────────────────────

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

        // ── Helpers ───────────────────────────────────────────────────────────

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

        private void WriteWorkflow(string relativePath, string name, string comment = "", string dataListXml = "<DataList />")
        {
            var fullPath = Path.Combine(_root, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
            File.WriteAllText(fullPath,
                $"<Service Name=\"{name}\" ResourceType=\"WorkflowService\">" +
                $"<Comment>{comment}</Comment>{dataListXml}</Service>");
        }

        private static ListWorkflowsResult Handle(
            HostEnvironmentConfig hostConfig,
            IWorkflowAuthPolicyLoader authPolicyLoader,
            ClaimsPrincipal? user = null,
            string? folder = null,
            string? cursor = null,
            int? pageSize = null) =>
            ListWorkflowsTool.Handle(hostConfig, authPolicyLoader, user, folder, cursor, pageSize);

        // ── Tests ─────────────────────────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_EmptyDirectory_ReturnsNoWorkflows()
        {
            var result = Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false });

            Assert.AreEqual(0, result.Workflows.Count);
            Assert.IsNull(result.NextCursor);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_OpenAccessMode_ReturnsAllWorkflowsSortedByPath()
        {
            WriteWorkflow("Zebra.bite", "Zebra");
            WriteWorkflow("Alpha.bite", "Alpha");

            var result = Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false });

            Assert.AreEqual(2, result.Workflows.Count);
            Assert.AreEqual("Alpha", result.Workflows[0].Name);
            Assert.AreEqual("Zebra", result.Workflows[1].Name);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_ExtractsDescriptionAndInputsOutputsFromDataList()
        {
            WriteWorkflow("Hello.bite", "Hello", comment: "Says hello",
                dataListXml: "<DataList><Name Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Input\" />" +
                             "<Message Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Output\" /></DataList>");

            var result = Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false });

            var wf = result.Workflows.Single();
            Assert.AreEqual("Says hello", wf.Description);
            CollectionAssert.Contains(wf.Inputs.ToList(), "Name");
            CollectionAssert.Contains(wf.Outputs.ToList(), "Message");
            Assert.IsFalse(wf.BodyEditable, "bodyEditable must default false until fidelity-gate consumption lands.");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_FolderFilter_ScopesToSubfolderOnly()
        {
            WriteWorkflow("Root.bite", "Root");
            WriteWorkflow(Path.Combine("Sub", "Nested.bite"), "Nested");

            var result = Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false }, folder: "Sub");

            var wf = result.Workflows.Single();
            Assert.AreEqual("Nested", wf.Name);
            Assert.AreEqual("Sub/Nested", wf.Path);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_Pagination_ReturnsNextCursorAndHonorsItOnNextCall()
        {
            for (var i = 0; i < 5; i++)
            {
                WriteWorkflow($"Wf{i}.bite", $"Wf{i}");
            }

            var loader = new StubAuthPolicyLoader { IsConfigEffective = false };
            var page1 = Handle(HostConfig(), loader, pageSize: 2);

            Assert.AreEqual(2, page1.Workflows.Count);
            Assert.IsNotNull(page1.NextCursor);
            Assert.AreEqual("Wf0", page1.Workflows[0].Name);
            Assert.AreEqual("Wf1", page1.Workflows[1].Name);

            var page2 = Handle(HostConfig(), loader, cursor: page1.NextCursor, pageSize: 2);

            Assert.AreEqual(2, page2.Workflows.Count);
            Assert.AreEqual("Wf2", page2.Workflows[0].Name);
            Assert.AreEqual("Wf3", page2.Workflows[1].Name);
            Assert.IsNotNull(page2.NextCursor);

            var page3 = Handle(HostConfig(), loader, cursor: page2.NextCursor, pageSize: 2);

            Assert.AreEqual(1, page3.Workflows.Count);
            Assert.AreEqual("Wf4", page3.Workflows[0].Name);
            Assert.IsNull(page3.NextCursor, "Last page must not advertise a further cursor.");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_PageSize_ClampedToMax200()
        {
            WriteWorkflow("Only.bite", "Only");

            // Passing an oversized pageSize must not throw and must still return results
            // (Math.Clamp caps it at MaxPageSize rather than failing the request).
            var result = Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false }, pageSize: 10_000);

            Assert.AreEqual(1, result.Workflows.Count);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_SecureConfigEffective_NoPrincipal_FiltersOutEverything()
        {
            WriteWorkflow("Secret.bite", "Secret");
            var loader = new StubAuthPolicyLoader
            {
                IsConfigEffective = true,
                EffectivePermissions = (_, _) => WorkflowPermission.None,
            };

            var result = Handle(HostConfig(), loader, user: null);

            Assert.AreEqual(0, result.Workflows.Count);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_SecureConfigEffective_PrincipalWithViewPermission_IncludesWorkflow()
        {
            WriteWorkflow("Visible.bite", "Visible");
            WriteWorkflow("Hidden.bite", "Hidden");

            var loader = new StubAuthPolicyLoader
            {
                IsConfigEffective = true,
                EffectivePermissions = (path, _) =>
                    path.Equals("Visible", StringComparison.OrdinalIgnoreCase)
                        ? WorkflowPermission.View
                        : WorkflowPermission.None,
            };

            var result = Handle(HostConfig(), loader, user: Principal("Developers"));

            var wf = result.Workflows.Single();
            Assert.AreEqual("Visible", wf.Name);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_MalformedWorkflowBody_StillListedWithoutDetail()
        {
            var fullPath = Path.Combine(_root, "Malformed.bite");
            File.WriteAllText(fullPath, "<Service Name=\"Malformed\" ResourceType=\"WorkflowService\"><Comment>Oops");

            var result = Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false });

            var wf = result.Workflows.Single();
            Assert.AreEqual("Malformed", wf.Name);
            Assert.AreEqual(string.Empty, wf.Description);
            Assert.AreEqual(0, wf.Inputs.Count);
            Assert.AreEqual(0, wf.Outputs.Count);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_NonWorkflowResourceType_IsExcluded()
        {
            var fullPath = Path.Combine(_root, "NotAWorkflow.bite");
            File.WriteAllText(fullPath, "<Service Name=\"NotAWorkflow\" ResourceType=\"Source\"></Service>");

            var result = Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false });

            Assert.AreEqual(0, result.Workflows.Count);
        }
    }
}
