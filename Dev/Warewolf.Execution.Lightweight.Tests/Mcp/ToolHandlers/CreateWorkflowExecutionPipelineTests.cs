/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Regression coverage for the "create_workflow/edit_workflow produces a workflow that
 *  crashes at execution" defect (consolidated MCP testing findings, headline finding):
 *
 *  Root cause: X6ToWorkflowConverter.BuildWorkflow always compiles the X6 "start" cell to a
 *  placeholder WriteLine activity, then discards it in favour of whatever the start cell's
 *  outgoing edge points to (`flowchart.StartNode = startFlowNode.Next ?? startFlowNode`). If
 *  the start cell has no outgoing edge, that placeholder — which is not an IDev2Activity —
 *  becomes the Flowchart's literal StartNode. ActivityParser.Parse then called
 *  `.FirstOrDefault()` on the null IEnumerable<IDev2Activity> that ParseFlowStep returns for a
 *  non-IDev2Activity start node, throwing an unhandled `ArgumentNullException("source")` at
 *  execution time — reproduced end-to-end below before the fix.
 *
 *  Two independent layers close this gap:
 *    1. ValidateWorkflowTool now rejects a start node with no outgoing edge — reused by
 *       create_workflow/edit_workflow, so a broken workflow of this shape is never persisted.
 *    2. ActivityParser.Parse (shared Dev2.Activities code, also used by the on-prem Server) is
 *       defensive against a null tool chain regardless of cause — a hand-crafted/legacy
 *       resource that bypasses (1) still fails cleanly via WorkflowExecutor's existing
 *       "no start node found" handling instead of throwing.
 *    3. WorkflowExecutor.Execute's catch(Exception) no longer appends ex.StackTrace to the
 *       caller-facing Errors list (it leaked internal file paths — see the same findings doc,
 *       "Supporting bugs").
 */

using Dev2.Activities.WF;
using Dev2.Common.X6;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModelContextProtocol;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Warewolf.Execution.Lightweight.Auth;
using Warewolf.Execution.Lightweight.Auth.Models;
using Warewolf.Execution.Lightweight.Infrastructure;
using Warewolf.Execution.Lightweight.Logging;
using Warewolf.Execution.Lightweight.Mcp;
using Warewolf.Execution.Lightweight.Mcp.ToolHandlers;
using Warewolf.Execution.Lightweight.Tests.TestSupport;

namespace Warewolf.Execution.Lightweight.Tests.Mcp.ToolHandlers
{
    [TestClass]
    public class CreateWorkflowExecutionPipelineTests
    {
        private string _root = string.Empty;

        [TestInitialize]
        public void Setup() => _root = Directory.CreateDirectory(
            Path.Combine(Path.GetTempPath(), "cwep-tests-" + Guid.NewGuid().ToString("N"))).FullName;

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
            public Func<string, IEnumerable<string>, WorkflowPermission> EffectivePermissions { get; set; } = (_, _) => WorkflowPermission.None;
            public WorkflowPermission GetEffectivePermissions(string workflowName, IEnumerable<string> callerRoles) => EffectivePermissions(workflowName, callerRoles);
            public void Reload() { }
        }

        private sealed class NullExecutionLogger : IExecutionLogger
        {
            public void LogTrace(string message, Guid executionId) { }
            public void LogTrace(string message, Exception exception, Guid executionId) { }
            public void LogDebug(string message, Guid executionId) { }
            public void LogDebug(string message, Exception exception, Guid executionId) { }
            public void LogError(Exception ex, string log) { }
            public void LogInfo(string message, Guid executionId) { }
            public void LogInfo(string message, Exception exception, Guid executionId) { }
            public void LogInfo(string message) { }
            public void LogWarning(string message, Guid executionId) { }
            public void LogWarning(string message, Exception exception, Guid executionId) { }
            public void LogError(string message, Guid executionId) { }
            public void LogError(string activityName, Exception ex, Guid executionId) { }
            public void LogFatal(string message, Guid executionId) { }
            public void LogFatal(string message, Exception exception, Guid executionId) { }
        }

        private HostEnvironmentConfig HostConfig() => McpToolTestHostConfig.ForWorkflowsDirectory(_root);

        static JsonElement EmptyEnvelope() =>
            System.Text.Json.JsonSerializer.SerializeToElement(new { inputs = Array.Empty<object>(), outputs = Array.Empty<object>() });

        static JsonElement StartOnlyBody(string resourceName) => JsonDocument.Parse(JsonConvert.SerializeObject(new X6WorkflowSaveModel
        {
            ResourceName = resourceName,
            Cells = new List<Cell>
            {
                new() { id = "start", shape = "rect", data = new Dictionary<string, object> { ["type"] = Constants.START } }
            }
        })).RootElement;

        static JsonElement StartPlusAssignBody(string resourceName)
        {
            var fields = new JArray(new JObject { ["FieldName"] = "[[Result]]", ["FieldValue"] = "hello", ["IndexNumber"] = 1 });
            var graph = new X6WorkflowSaveModel
            {
                ResourceName = resourceName,
                Cells = new List<Cell>
                {
                    new() { id = "start", shape = "rect", data = new Dictionary<string, object> { ["type"] = Constants.START } },
                    new()
                    {
                        id = "assign1", shape = "rect",
                        data = new Dictionary<string, object>
                        {
                            ["type"] = "dsfdotnetmultiassignactivity",
                            ["displayName"] = "Assign",
                            ["fields"] = fields,
                        }
                    },
                    new() { id = "e1", shape = "edge", data = new Dictionary<string, object>(), Source = new Connector("start"), Target = new Connector("assign1") }
                }
            };
            return JsonDocument.Parse(JsonConvert.SerializeObject(graph)).RootElement;
        }

        // ── Layer 1: validate_workflow/create_workflow reject the broken shape up front ──────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void ValidateWorkflow_StartNodeWithNoOutgoingEdge_ReportsHardError()
        {
            var result = ValidateWorkflowTool.Handle(EmptyEnvelope(), StartOnlyBody("StartOnly"));

            Assert.IsFalse(result.Valid);
            Assert.IsTrue(result.Errors.Any(e => e.Severity == "error" &&
                e.Message.Contains("no outgoing connection", StringComparison.OrdinalIgnoreCase)));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public void CreateWorkflow_StartNodeWithNoOutgoingEdge_ThrowsWithoutWriting()
        {
            var hostConfig = HostConfig();
            try
            {
                CreateWorkflowTool.Handle(hostConfig, new StubAuthPolicyLoader { IsConfigEffective = false }, null,
                    "StartOnly", EmptyEnvelope(), StartOnlyBody("StartOnly"));
            }
            finally
            {
                Assert.IsFalse(File.Exists(Path.Combine(_root, "StartOnly.bite")),
                    "A start node with no outgoing edge must be rejected before any file is written " +
                    "(previously this was persisted and crashed WorkflowExecutor at execution time).");
            }
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void CreateWorkflow_MissingEnvelopeAndBody_ReportsBothInOneMessage()
        {
            var ex = Assert.ThrowsException<McpException>(() =>
                ValidateWorkflowTool.Handle(default, default));

            StringAssert.Contains(ex.Message, "envelope");
            StringAssert.Contains(ex.Message, "body");
        }

        // ── Success path: a real, connected workflow still creates AND executes cleanly ───────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void CreateWorkflow_ThenExecute_WithConnectedAssignStep_ExecutesSuccessfully()
        {
            var hostConfig = HostConfig();
            var createResult = CreateWorkflowTool.Handle(hostConfig, new StubAuthPolicyLoader { IsConfigEffective = false }, null,
                "ConnectedAssign",
                System.Text.Json.JsonSerializer.SerializeToElement(new
                {
                    inputs = Array.Empty<object>(),
                    outputs = new[] { new { name = "Result", kind = "scalar", fields = Array.Empty<string>() } },
                }),
                StartPlusAssignBody("ConnectedAssign"));

            Assert.IsTrue(createResult.Created);

            var filePath = Path.Combine(_root, "ConnectedAssign.bite");
            Environment.SetEnvironmentVariable("WAREWOLF_LICENSE_CHECK_ENABLED", "false");
            var result = new WorkflowExecutor(new NullExecutionLogger()).Execute(filePath, new Dictionary<string, string>());

            Assert.IsTrue(result.IsSuccess, "errors: " + string.Join("; ", result.Errors ?? new List<string>()));
        }

        // ── Layer 2: WorkflowExecutor/ActivityParser stay defensive for a hand-crafted/legacy
        //    resource that bypasses validate_workflow entirely (e.g. authored by another tool,
        //    or a pre-fix resource still on disk from before this change). ─────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Execute_HandCraftedFileWithUnconnectedStartNode_FailsCleanly_NoStackTraceLeaked()
        {
            // Bypasses ValidateWorkflowTool entirely - drives X6ToWorkflowConverter directly,
            // exactly like CreateWorkflowTool's own compile step, to reproduce the same
            // WriteLine-only-StartNode XAML shape a hand-crafted or legacy .bite file could have.
            var xaml = new Dev2.Activities.WF.X6ToWorkflowConverter().X6JsonToWorkflow(StartOnlyBody("Legacy").GetRawText());
            xaml = Dev2.Activities.WF.X6ToWorkflowConverter.AddReplaceNameSpace(xaml);

            var biteContents = EnvelopeBiteWriter.BuildBiteFileContents(
                Guid.NewGuid().ToString(), "Legacy", "", EmptyEnvelope(), xaml.ToString(),
                versionNumber: 1, timestampUtc: DateTimeOffset.UtcNow, user: "test");

            var filePath = Path.Combine(_root, "Legacy.bite");
            File.WriteAllText(filePath, biteContents);

            Environment.SetEnvironmentVariable("WAREWOLF_LICENSE_CHECK_ENABLED", "false");
            var result = new WorkflowExecutor(new NullExecutionLogger()).Execute(filePath, new Dictionary<string, string>());

            Assert.IsFalse(result.IsSuccess);
            Assert.IsNotNull(result.Errors);
            Assert.IsFalse(result.Errors.Any(e => e.Contains("ArgumentNullException") || e.Contains(" at System.")),
                "must not throw/leak a raw stack trace for this shape - " + string.Join("; ", result.Errors));
            Assert.IsFalse(result.Errors.Any(e => e.Contains(":\\")),
                "must not leak a local file path in the error message - " + string.Join("; ", result.Errors));
        }
    }
}
