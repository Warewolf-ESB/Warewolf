/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using Warewolf.Bite;

namespace Warewolf.Execution.Lightweight.Tests.Execution
{
    /// <summary>
    /// WF removal T1.7 shadow mode in the Lightweight engine: every freshly built workflow is also read by the
    /// WF-free reader and compared, and nothing about that may change the workflow that is executed.
    /// <see cref="WorkflowExecutor.ShadowSink"/> is process-wide, so these do not run in parallel.
    /// </summary>
    [TestClass]
    [DoNotParallelize]
    public class WorkflowExecutorShadowTests
    {
        static string WorkflowPath =>
            Path.Combine(AppContext.BaseDirectory, "TestResources", "hangfiredemo", "Hello World.bite");

        static System.Text.StringBuilder Xaml()
        {
            var (xaml, _, _) = WorkflowExecutor.ExtractWorkflowParts(WorkflowExecutor.ReadWorkflowFile(WorkflowPath));
            Assert.IsNotNull(xaml, "test resource must contain a XamlDefinition");
            return xaml!;
        }

        [TestInitialize]
        public void Setup() => WorkflowExecutor.ClearWorkflowPool();

        [TestCleanup]
        public void Cleanup()
        {
            WorkflowExecutor.ShadowSink = null;
            WorkflowExecutor.ClearWorkflowPool();
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [Owner("Ashley Lewis")]
        public void Shadow_FreshBuild_ReportsOnceAndMatches()
        {
            var results = new List<BiteShadowResult>();
            WorkflowExecutor.ShadowSink = results.Add;

            var prepared = WorkflowExecutor.RentPreparedWorkflow(WorkflowPath, Xaml());

            Assert.IsNotNull(prepared?.StartActivity);
            Assert.AreEqual(1, results.Count);
            Assert.IsTrue(results[0].Matched, results[0].ToString());
            Assert.AreEqual(WorkflowPath, results[0].ResourceId);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [Owner("Ashley Lewis")]
        public void Shadow_PooledReuse_IsNotShadowedAgain()
        {
            var results = new List<BiteShadowResult>();
            WorkflowExecutor.ShadowSink = results.Add;

            var first = WorkflowExecutor.RentPreparedWorkflow(WorkflowPath, Xaml());
            WorkflowExecutor.ReturnPreparedWorkflow(WorkflowPath, first);
            var reused = WorkflowExecutor.RentPreparedWorkflow(WorkflowPath, Xaml());

            Assert.AreSame(first, reused, "precondition: the second rent is served from the pool");
            Assert.AreEqual(1, results.Count, "Only a fresh build is compared; pool hits cost nothing.");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [Owner("Ashley Lewis")]
        public void Shadow_SinkThrows_WorkflowIsStillBuiltAndUnchanged()
        {
            var baseline = WorkflowExecutor.BuildExclusivePreparedWorkflow(Xaml());
            WorkflowExecutor.ShadowSink = _ => throw new InvalidOperationException("telemetry down");

            var prepared = WorkflowExecutor.BuildExclusivePreparedWorkflow(Xaml(), WorkflowPath);

            Assert.IsNotNull(prepared?.StartActivity);
            Assert.AreEqual(0, ActivityGraphDiff.Compare(baseline.StartActivity, prepared.StartActivity).Count,
                "Shadow mode must not change the graph that is executed.");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [Owner("Ashley Lewis")]
        public void Shadow_NoSink_DoesNotRun()
        {
            WorkflowExecutor.ShadowSink = null;

            var prepared = WorkflowExecutor.RentPreparedWorkflow(WorkflowPath, Xaml());

            Assert.IsNotNull(prepared?.StartActivity, "With shadow mode off, building is unaffected.");
        }
    }
}
