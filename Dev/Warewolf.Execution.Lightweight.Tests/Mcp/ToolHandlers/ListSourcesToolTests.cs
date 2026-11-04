/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Unit tests for ListSourcesTool: only <Source> roots are listed, each with its resource ID,
 *  the connection string never leaves the engine, and View permission filters the rows.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
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
using Warewolf.Execution.Lightweight.Tests.TestSupport;

namespace Warewolf.Execution.Lightweight.Tests.Mcp.ToolHandlers
{
    [TestClass]
    public class ListSourcesToolTests
    {
        private string _root = string.Empty;

        [TestInitialize]
        public void Setup() => _root = Directory.CreateDirectory(
            Path.Combine(Path.GetTempPath(), "lst-tests-" + Guid.NewGuid().ToString("N"))).FullName;

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(_root))
            {
                Directory.Delete(_root, recursive: true);
            }
        }

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

        private HostEnvironmentConfig HostConfig() => McpToolTestHostConfig.ForWorkflowsDirectory(_root);

        private void Write(string relativePath, string xml)
        {
            var fullPath = Path.Combine(_root, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
            File.WriteAllText(fullPath, xml);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_ListsSourcesWithTheirIds_AndSkipsWorkflows()
        {
            var webId = Guid.NewGuid();
            var dbId = Guid.NewGuid();
            Write("ImagineApi.bite", $"<Source ID=\"{webId}\" Name=\"ImagineApi\" ResourceType=\"WebSource\" Type=\"WebSource\" ConnectionString=\"AQAAANCMnd8secret\"></Source>");
            Write("Db/Orders.bite", $"<Source ID=\"{dbId}\" Name=\"Orders\" ResourceType=\"SqlDatabase\" Type=\"DbSource\" ConnectionString=\"WFAES::x\"></Source>");
            Write("imagine.bite", $"<Service ID=\"{Guid.NewGuid()}\" Name=\"imagine\" ResourceType=\"WorkflowService\"><DataList /></Service>");

            var result = ListSourcesTool.Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false });

            CollectionAssert.AreEqual(new[] { "Db/Orders", "ImagineApi" }, result.Sources.Select(s => s.Path).ToArray());
            var web = result.Sources.Single(s => s.Name == "ImagineApi");
            Assert.AreEqual(webId.ToString(), web.Id);
            Assert.AreEqual("WebSource", web.ResourceType);
            var db = result.Sources.Single(s => s.Name == "Orders");
            Assert.AreEqual(dbId.ToString(), db.Id);
            Assert.AreEqual("DbSource", db.Type);
            Assert.IsNull(result.NextCursor);

            var json = JsonSerializer.Serialize(result);
            Assert.IsFalse(json.Contains("AQAAANCMnd8secret") || json.Contains("WFAES::"), "the connection string must never be returned");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_SecureConfigEffective_ReturnsOnlySourcesTheCallerCanView()
        {
            Write("Visible.bite", $"<Source ID=\"{Guid.NewGuid()}\" Name=\"Visible\" ResourceType=\"WebSource\"></Source>");
            Write("Hidden.bite", $"<Source ID=\"{Guid.NewGuid()}\" Name=\"Hidden\" ResourceType=\"WebSource\"></Source>");
            var loader = new StubAuthPolicyLoader
            {
                IsConfigEffective = true,
                EffectivePermissions = (path, _) => path == "Visible" ? WorkflowPermission.View : WorkflowPermission.None,
            };

            var result = ListSourcesTool.Handle(HostConfig(), loader, Principal("Developers"));

            CollectionAssert.AreEqual(new[] { "Visible" }, result.Sources.Select(s => s.Name).ToArray());
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_PagesWithACursor()
        {
            for (var i = 0; i < 3; i++)
            {
                Write($"S{i}.bite", $"<Source ID=\"{Guid.NewGuid()}\" Name=\"S{i}\" ResourceType=\"WebSource\"></Source>");
            }

            var loader = new StubAuthPolicyLoader { IsConfigEffective = false };
            var first = ListSourcesTool.Handle(HostConfig(), loader, pageSize: 2);
            var second = ListSourcesTool.Handle(HostConfig(), loader, cursor: first.NextCursor, pageSize: 2);

            CollectionAssert.AreEqual(new[] { "S0", "S1" }, first.Sources.Select(s => s.Name).ToArray());
            CollectionAssert.AreEqual(new[] { "S2" }, second.Sources.Select(s => s.Name).ToArray());
            Assert.IsNull(second.NextCursor);
        }
    }
}
