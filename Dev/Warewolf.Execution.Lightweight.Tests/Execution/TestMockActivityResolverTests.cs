/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Unit tests for TestMockActivityResolver against a REAL compiled activity chain (via
 *  WorkflowExecutor.LoadDynamicActivity + ActivityParser, the same path WorkflowExecutor itself
 *  uses) — not a hand-built fake — so the string-equality match against IDev2Activity.UniqueID
 *  (stamped from the X6 node id by X6ToWorkflowConverter) is exercised exactly as it runs in
 *  production. Covers: no matching step returns the original activity unchanged; a Mock step on a
 *  regular (non-container) activity returns a TestMockStep wrapper carrying its configured
 *  outputs; and the two recursive container cases — a Sequence child gets replaced while its
 *  sibling is left untouched, and a ForEach's DataFunc.Handler gets swapped — mirroring
 *  Evaluator.RecursivelyMockRecursiveActivities' exact mutation targets.
 */

using Dev2;
using Dev2.Activities;
using Dev2.Common.Interfaces;
using Dev2.Common.X6;
using Dev2.Data;
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Execution.Lightweight.Auth;
using Warewolf.Execution.Lightweight.Auth.Models;
using Warewolf.Execution.Lightweight.Infrastructure;
using Warewolf.Execution.Lightweight.Mcp.ToolHandlers;
using Warewolf.Execution.Lightweight.Tests.TestSupport;

namespace Warewolf.Execution.Lightweight.Tests.Execution
{
    [TestClass]
    public class TestMockActivityResolverTests
    {
        private string _root = string.Empty;

        [TestInitialize]
        public void Setup() => _root = Directory.CreateDirectory(
            Path.Combine(Path.GetTempPath(), "mock-resolver-tests-" + Guid.NewGuid().ToString("N"))).FullName;

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
            public WorkflowPermission GetEffectivePermissions(string workflowName, IEnumerable<string> callerRoles) => WorkflowPermission.None;
            public void Reload() { }
        }

        static readonly StubAuthPolicyLoader OpenPolicy = new() { IsConfigEffective = false };

        HostEnvironmentConfig HostConfig() => McpToolTestHostConfig.ForWorkflowsDirectory(_root);

        /// <summary>Compiles and parses a real { start -> Assign } workflow the same way
        /// WorkflowExecutor.Execute/ExecuteTest do, returning the start activity of the chain
        /// and the Assign node's real (GUID) activity id.</summary>
        (IDev2Activity StartActivity, string AssignActivityId) BuildRealActivityChain()
        {
            var assignActivityId = Guid.NewGuid().ToString();
            var fields = new JArray(new JObject { ["FieldName"] = "[[Result]]", ["FieldValue"] = "hello", ["IndexNumber"] = 1 });
            var start = new Cell { id = "start", shape = "rect", data = new Dictionary<string, object> { ["type"] = Constants.START } };
            var assign = new Cell
            {
                id = assignActivityId,
                shape = "rect",
                data = new Dictionary<string, object>
                {
                    ["type"] = "dsfdotnetmultiassignactivity",
                    ["displayName"] = "Assign",
                    ["fields"] = fields,
                }
            };
            var edge = new Cell { id = "e1", shape = "edge", data = new Dictionary<string, object>(), Source = new Connector("start"), Target = new Connector(assignActivityId) };

            var graph = new X6WorkflowSaveModel { ResourceName = "RealChainWf", Cells = new List<Cell> { start, assign, edge } };
            var body = System.Text.Json.JsonDocument.Parse(JsonConvert.SerializeObject(graph)).RootElement;
            var envelope = System.Text.Json.JsonSerializer.SerializeToElement(new
            {
                name = "RealChainWf",
                description = "",
                inputs = Array.Empty<object>(),
                outputs = new[] { new { name = "Result", kind = "scalar", fields = Array.Empty<string>() } },
            });

            CreateWorkflowTool.Handle(HostConfig(), OpenPolicy, null, "RealChainWf", envelope, body);

            var filePath = Path.Combine(_root, "RealChainWf.bite");
            var fileContents = WorkflowExecutor.ReadWorkflowFile(filePath);
            var (xaml, _, _) = WorkflowExecutor.ExtractWorkflowParts(fileContents);
            var activity = WorkflowExecutor.LoadDynamicActivity(xaml);
            var startActivity = new ActivityParser().Parse(activity);

            return (startActivity, assignActivityId);
        }

        static IServiceTestStep MockStep(string activityId, string activityType, params IServiceTestOutput[] outputs) =>
            new ServiceTestStepTO
            {
                ActivityID = Guid.Parse(activityId),
                UniqueID = Guid.Parse(activityId),
                ActivityType = activityType,
                Type = StepType.Mock,
                StepOutputs = new ObservableCollection<IServiceTestOutput>(outputs),
            };

        /// <summary>Compiles and parses a real { start -> Sequence[AssignOne, AssignTwo] }
        /// workflow. Nested children are separate top-level cells carrying the generic
        /// isNested/parentId/index markers CellOrganizer.BuildHierarchy groups by — shared by both
        /// the Sequence and ForEach X6 converter helpers.</summary>
        (IDev2Activity StartActivity, string SequenceId, string Child1Id, string Child2Id) BuildSequenceActivityChain()
        {
            var sequenceId = Guid.NewGuid().ToString();
            var child1Id = Guid.NewGuid().ToString();
            var child2Id = Guid.NewGuid().ToString();

            var start = new Cell { id = "start", shape = "rect", data = new Dictionary<string, object> { ["type"] = Constants.START } };
            var sequence = new Cell
            {
                id = sequenceId,
                shape = "rect",
                data = new Dictionary<string, object> { ["type"] = "DsfSequenceActivity", ["displayName"] = "Sequence" },
            };
            var child1 = new Cell
            {
                id = child1Id,
                shape = "rect",
                data = new Dictionary<string, object>
                {
                    ["type"] = "dsfdotnetmultiassignactivity",
                    ["displayName"] = "AssignOne",
                    ["fields"] = new JArray(new JObject { ["FieldName"] = "[[A]]", ["FieldValue"] = "one", ["IndexNumber"] = 1 }),
                    ["isNested"] = true,
                    ["parentId"] = sequenceId,
                    ["index"] = 0,
                }
            };
            var child2 = new Cell
            {
                id = child2Id,
                shape = "rect",
                data = new Dictionary<string, object>
                {
                    ["type"] = "dsfdotnetmultiassignactivity",
                    ["displayName"] = "AssignTwo",
                    ["fields"] = new JArray(new JObject { ["FieldName"] = "[[B]]", ["FieldValue"] = "two", ["IndexNumber"] = 1 }),
                    ["isNested"] = true,
                    ["parentId"] = sequenceId,
                    ["index"] = 1,
                }
            };
            var edge = new Cell { id = "e1", shape = "edge", data = new Dictionary<string, object>(), Source = new Connector("start"), Target = new Connector(sequenceId) };

            var graph = new X6WorkflowSaveModel { ResourceName = "SequenceWf", Cells = new List<Cell> { start, sequence, child1, child2, edge } };
            var body = System.Text.Json.JsonDocument.Parse(JsonConvert.SerializeObject(graph)).RootElement;
            var envelope = System.Text.Json.JsonSerializer.SerializeToElement(new
            {
                name = "SequenceWf",
                description = "",
                inputs = Array.Empty<object>(),
                outputs = new[]
                {
                    new { name = "A", kind = "scalar", fields = Array.Empty<string>() },
                    new { name = "B", kind = "scalar", fields = Array.Empty<string>() },
                },
            });

            CreateWorkflowTool.Handle(HostConfig(), OpenPolicy, null, "SequenceWf", envelope, body);

            var startActivity = ParseCompiledStartActivity("SequenceWf.bite");
            return (startActivity, sequenceId, child1Id, child2Id);
        }

        /// <summary>Compiles and parses a real { start -> ForEach[InnerAssign] } workflow — a
        /// single nested child, so X6ToWorkflowConverter_ForEachActivityHelper wires it directly
        /// onto DataFunc.Handler rather than wrapping it in a synthetic Sequence.</summary>
        (IDev2Activity StartActivity, string ForEachId, string ChildId) BuildForEachActivityChain()
        {
            var forEachId = Guid.NewGuid().ToString();
            var childId = Guid.NewGuid().ToString();

            var start = new Cell { id = "start", shape = "rect", data = new Dictionary<string, object> { ["type"] = Constants.START } };
            var forEach = new Cell
            {
                id = forEachId,
                shape = "rect",
                data = new Dictionary<string, object>
                {
                    ["type"] = "DsfForEachActivity",
                    ["displayName"] = "For Each",
                    ["foreachtype"] = "NumOfExecution",
                    ["numofexecutions"] = "1",
                }
            };
            var child = new Cell
            {
                id = childId,
                shape = "rect",
                data = new Dictionary<string, object>
                {
                    ["type"] = "dsfdotnetmultiassignactivity",
                    ["displayName"] = "InnerAssign",
                    ["fields"] = new JArray(new JObject { ["FieldName"] = "[[Result]]", ["FieldValue"] = "hello", ["IndexNumber"] = 1 }),
                    ["isNested"] = true,
                    ["parentId"] = forEachId,
                    ["index"] = 0,
                }
            };
            var edge = new Cell { id = "e1", shape = "edge", data = new Dictionary<string, object>(), Source = new Connector("start"), Target = new Connector(forEachId) };

            var graph = new X6WorkflowSaveModel { ResourceName = "ForEachWf", Cells = new List<Cell> { start, forEach, child, edge } };
            var body = System.Text.Json.JsonDocument.Parse(JsonConvert.SerializeObject(graph)).RootElement;
            var envelope = System.Text.Json.JsonSerializer.SerializeToElement(new
            {
                name = "ForEachWf",
                description = "",
                inputs = Array.Empty<object>(),
                outputs = new[] { new { name = "Result", kind = "scalar", fields = Array.Empty<string>() } },
            });

            CreateWorkflowTool.Handle(HostConfig(), OpenPolicy, null, "ForEachWf", envelope, body);

            var startActivity = ParseCompiledStartActivity("ForEachWf.bite");
            return (startActivity, forEachId, childId);
        }

        IDev2Activity ParseCompiledStartActivity(string biteFileName)
        {
            var filePath = Path.Combine(_root, biteFileName);
            var fileContents = WorkflowExecutor.ReadWorkflowFile(filePath);
            var (xaml, _, _) = WorkflowExecutor.ExtractWorkflowParts(fileContents);
            var activity = WorkflowExecutor.LoadDynamicActivity(xaml);
            return new ActivityParser().Parse(activity);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void MockActivityIfNecessary_NoMatchingStep_ReturnsOriginalActivity()
        {
            var (startActivity, _) = BuildRealActivityChain();
            var testSteps = new List<IServiceTestStep>();

            var resolved = TestMockActivityResolver.MockActivityIfNecessary(startActivity, testSteps);

            Assert.AreSame(startActivity, resolved);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void MockActivityIfNecessary_NullTestSteps_ReturnsOriginalActivity()
        {
            var (startActivity, _) = BuildRealActivityChain();

            var resolved = TestMockActivityResolver.MockActivityIfNecessary(startActivity, null);

            Assert.AreSame(startActivity, resolved);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void MockActivityIfNecessary_MockStepOnRegularActivity_ReturnsTestMockStepWrapper()
        {
            var (startActivity, assignActivityId) = BuildRealActivityChain();

            // Walk to the Assign node (the second node in the { start -> assign } chain).
            var assignActivity = startActivity.UniqueID == assignActivityId ? startActivity : FindByUniqueId(startActivity, assignActivityId);
            Assert.IsNotNull(assignActivity, "Fixture must contain the Assign node.");

            var output = new ServiceTestOutputTO { Variable = "[[Result]]", Value = "mocked-value", AssertOp = "=" };
            var testSteps = new List<IServiceTestStep> { MockStep(assignActivityId, "DsfDotNetMultiAssignActivity", output) };

            var resolved = TestMockActivityResolver.MockActivityIfNecessary(assignActivity!, testSteps);

            Assert.IsInstanceOfType(resolved, typeof(Dev2.TestMockStep));
            var mockStep = (Dev2.TestMockStep)resolved;
            Assert.AreEqual(1, mockStep.Outputs.Count);
            Assert.AreEqual("mocked-value", mockStep.Outputs[0].Value);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void MockActivityIfNecessary_SequenceWithMockedFirstChild_ReplacesOnlyThatChild()
        {
            var (startActivity, sequenceId, child1Id, _) = BuildSequenceActivityChain();
            var sequenceActivity = startActivity.UniqueID == sequenceId ? startActivity : FindByUniqueId(startActivity, sequenceId);
            Assert.IsNotNull(sequenceActivity, "Fixture must contain the Sequence node.");
            var sequence = (DsfSequenceActivity)sequenceActivity!;
            var originalSecondChild = sequence.Activities[1];

            var output = new ServiceTestOutputTO { Variable = "[[A]]", Value = "mocked-A", AssertOp = "=" };
            var testSteps = new List<IServiceTestStep>
            {
                new ServiceTestStepTO
                {
                    ActivityID = Guid.Parse(sequenceId),
                    UniqueID = Guid.Parse(sequenceId),
                    ActivityType = "DsfSequenceActivity",
                    Type = StepType.Mock,
                    Children = new ObservableCollection<IServiceTestStep> { MockStep(child1Id, "DsfDotNetMultiAssignActivity", output) },
                },
            };

            var resolved = TestMockActivityResolver.MockActivityIfNecessary(sequence, testSteps);

            Assert.AreSame(sequence, resolved, "A Sequence is mutated in place (its children swapped internally), not wholesale-replaced like a regular activity.");
            Assert.IsInstanceOfType(sequence.Activities[0], typeof(Dev2.TestMockStep), "The first child (matched by a Mock test step) must be substituted.");
            Assert.AreSame(originalSecondChild, sequence.Activities[1], "The second child (no matching test step) must be left untouched — it still needs to execute for real.");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void MockActivityIfNecessary_ForEachWithMockedChild_ReplacesDataFuncHandler()
        {
            var (startActivity, forEachId, childId) = BuildForEachActivityChain();
            var forEachActivity = startActivity.UniqueID == forEachId ? startActivity : FindByUniqueId(startActivity, forEachId);
            Assert.IsNotNull(forEachActivity, "Fixture must contain the ForEach node.");
            var forEach = (DsfForEachActivity)forEachActivity!;
            Assert.IsNotInstanceOfType(forEach.DataFunc.Handler, typeof(Dev2.TestMockStep), "Sanity check: the fixture's inner activity must start out real.");

            var output = new ServiceTestOutputTO { Variable = "[[Result]]", Value = "mocked-value", AssertOp = "=" };
            var testSteps = new List<IServiceTestStep>
            {
                new ServiceTestStepTO
                {
                    ActivityID = Guid.Parse(forEachId),
                    UniqueID = Guid.Parse(forEachId),
                    ActivityType = "DsfForEachActivity",
                    Type = StepType.Mock,
                    Children = new ObservableCollection<IServiceTestStep> { MockStep(childId, "DsfDotNetMultiAssignActivity", output) },
                },
            };

            var resolved = TestMockActivityResolver.MockActivityIfNecessary(forEach, testSteps);

            Assert.AreSame(forEach, resolved, "A ForEach is mutated in place (DataFunc.Handler swapped internally), not wholesale-replaced like a regular activity.");
            Assert.IsInstanceOfType(forEach.DataFunc.Handler, typeof(Dev2.TestMockStep),
                "DataFunc.Handler must be swapped to the mock wrapper — this is the one mutation that is only safe because execute_test never pools its PreparedWorkflow (see TestMockActivityResolver's remarks).");
        }

        static IDev2Activity FindByUniqueId(IDev2Activity node, string uniqueId)
        {
            var visited = new HashSet<string>();
            var queue = new Queue<IDev2Activity>();
            queue.Enqueue(node);
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (current is null || !visited.Add(current.UniqueID))
                {
                    continue;
                }

                if (current.UniqueID == uniqueId)
                {
                    return current;
                }

                var next = current.NextNodes;
                if (next != null)
                {
                    foreach (var n in next)
                    {
                        queue.Enqueue(n);
                    }
                }
            }

            return null;
        }
    }
}
