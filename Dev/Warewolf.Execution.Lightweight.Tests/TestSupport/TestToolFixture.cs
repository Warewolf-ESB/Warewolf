/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using Dev2.Common.X6;
using ModelContextProtocol;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Warewolf.Execution.Lightweight.Auth;
using Warewolf.Execution.Lightweight.Auth.Models;
using Warewolf.Execution.Lightweight.Infrastructure;
using Warewolf.Execution.Lightweight.Mcp.Secrets;
using Warewolf.Execution.Lightweight.Mcp.ToolHandlers;

namespace Warewolf.Execution.Lightweight.Tests.TestSupport;

/// <summary>
/// A throwaway workflows directory holding one Start → Assign workflow, for the list_tests/get_test/
/// delete_test handler tests: the same fixture <c>EditTestToolTests</c> builds inline, shared here so
/// the three test classes don't each carry a copy. Dispose deletes the directory.
/// </summary>
internal sealed class TestToolFixture : IDisposable
{
    internal static readonly StubAuthPolicyLoader OpenPolicy = new() { IsConfigEffective = false };

    internal string Root { get; } = Directory.CreateDirectory(
        Path.Combine(Path.GetTempPath(), "test-tool-tests-" + Guid.NewGuid().ToString("N"))).FullName;

    internal string AssignActivityId { get; } = Guid.NewGuid().ToString();

    internal HostEnvironmentConfig HostConfig() => McpToolTestHostConfig.ForWorkflowsDirectory(Root);

    internal string CreateWorkflow(string name = "TargetWorkflow")
    {
        var fields = new JArray(new JObject { ["FieldName"] = "[[Result]]", ["FieldValue"] = "hello", ["IndexNumber"] = 1 });
        var start = new Cell { id = "start", shape = "rect", data = new Dictionary<string, object> { ["type"] = Constants.START } };
        var assign = new Cell
        {
            id = AssignActivityId,
            shape = "rect",
            data = new Dictionary<string, object> { ["type"] = "dsfdotnetmultiassignactivity", ["displayName"] = "Assign", ["fields"] = fields },
        };
        var edge = new Cell { id = "e1", shape = "edge", data = new Dictionary<string, object>(), Source = new Connector("start"), Target = new Connector(AssignActivityId) };

        var graph = new X6WorkflowSaveModel { ResourceName = name, Cells = new List<Cell> { start, assign, edge } };
        var body = JsonDocument.Parse(JsonConvert.SerializeObject(graph)).RootElement;
        var envelope = System.Text.Json.JsonSerializer.SerializeToElement(new
        {
            name,
            description = "",
            inputs = Array.Empty<object>(),
            outputs = new[] { new { name = "Result", kind = "scalar", fields = Array.Empty<string>() } },
        });

        CreateWorkflowTool.Handle(HostConfig(), OpenPolicy, null, name, envelope, body);
        return name;
    }

    /// <summary>Saves <paramref name="test"/> through create_test, so it's validated like a real one.</summary>
    internal Task CreateTest(string workflowName, object test) =>
        CreateTestTool.Handle(HostConfig(), OpenPolicy, NoSecrets.Instance, null, workflowName, System.Text.Json.JsonSerializer.SerializeToElement(test));

    internal static WorkflowClaimsPrincipal Principal(params string[] roles) =>
        new(new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.Name, "alice") }.Concat(roles.Select(r => new Claim(ClaimTypes.Role, r))),
            "Bearer", ClaimTypes.Name, ClaimTypes.Role));

    /// <summary>A secure.config in force that grants every caller exactly <paramref name="permission"/>.</summary>
    internal static StubAuthPolicyLoader Granting(WorkflowPermission permission) =>
        new() { IsConfigEffective = true, EffectivePermissions = (_, _) => permission };

    public void Dispose()
    {
        if (Directory.Exists(Root))
        {
            Directory.Delete(Root, recursive: true);
        }
    }

    internal sealed class StubAuthPolicyLoader : IWorkflowAuthPolicyLoader
    {
        public bool IsConfigEffective { get; set; }
        public int PolicyCount => 0;
        public PolicyLookupResult GetPolicy(string workflowName) => PolicyLookupResult.ConfigMissing();

        public Func<string, IEnumerable<string>, WorkflowPermission> EffectivePermissions { get; set; } = (_, _) => WorkflowPermission.None;

        public WorkflowPermission GetEffectivePermissions(string workflowName, IEnumerable<string> callerRoles) =>
            EffectivePermissions(workflowName, callerRoles);

        public void Reload() { }
    }

    sealed class NoSecrets : IMcpSecretResolver
    {
        internal static readonly NoSecrets Instance = new();

        public Task<string> ResolveAsync(string name, CancellationToken cancellationToken) =>
            throw new McpException($"Secret reference '${{{name}}}' could not be resolved: no secret named '{name}' is staged.");
    }
}
