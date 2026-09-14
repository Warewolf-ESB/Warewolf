/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Tests for WorkflowExecutor's OutOfMemoryException handling — the fix for the
 *  ShovelBridge load-test finding documented in docs/ShovelBridge-Architecture.md:
 *  an Azure Functions Consumption-plan cold start under a concurrent burst can throw
 *  OutOfMemoryException during workflow compile/execute; that must be surfaced as a
 *  TRANSIENT failure (IsTransientFailure = true) rather than an ordinary business
 *  failure, so ServiceBusWorkflowTriggerFunction retries instead of dead-lettering
 *  on the first hit.
 *
 *  Execute(...) itself is not exercised here: deliberately exhausting process memory
 *  to force a real OutOfMemoryException would be slow, environment-dependent, and
 *  could destabilise the shared test host. Instead, WorkflowExecutor.BuildTransientFailureResult
 *  — the small internal (InternalsVisibleTo this test project) helper the catch(OutOfMemoryException)
 *  clause delegates to — is tested directly with a manually constructed exception, which
 *  deterministically verifies the exact same result-shaping logic production code runs.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using Warewolf.Execution.Lightweight.Models;

namespace Warewolf.Execution.Lightweight.Tests.Execution
{
    [TestClass]
    public class WorkflowExecutorTransientFailureTests
    {
        [TestMethod]
        [TestCategory("UnitTest")]
        public void BuildTransientFailureResult_OutOfMemoryException_MarksResultAsTransientFailure()
        {
            var oom = new OutOfMemoryException("Insufficient memory to continue the execution of the program.");
            var executionId = Guid.NewGuid();
            var startTime = DateTime.UtcNow.AddSeconds(-1);
            var elapsed = TimeSpan.FromMilliseconds(250);

            var result = WorkflowExecutor.BuildTransientFailureResult(oom, executionId, startTime, elapsed);

            Assert.IsFalse(result.IsSuccess, "An OutOfMemoryException is never a success.");
            Assert.IsTrue(result.IsTransientFailure, "OutOfMemoryException must be flagged transient so callers that can retry (e.g. the Service Bus trigger) do so instead of treating it as a terminal business failure.");
            Assert.AreEqual(executionId, result.ExecutionId);
            Assert.AreEqual(startTime, result.StartTime);
            Assert.AreEqual(elapsed, result.Duration);
            Assert.AreEqual(1, result.Errors.Count);
            StringAssert.Contains(result.Errors[0], "Insufficient memory to continue the execution of the program.");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void TransientFailure_FactoryMethod_SetsIsSuccessFalseAndIsTransientFailureTrue()
        {
            var result = WorkflowExecutionResult.TransientFailure("Insufficient memory to continue the execution of the program.");

            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.IsTransientFailure);
            CollectionAssert.Contains(result.Errors, "Insufficient memory to continue the execution of the program.");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Failure_FactoryMethod_DoesNotSetIsTransientFailure()
        {
            // Regression guard: ordinary business failures (e.g. Failure(...), used throughout
            // the codebase for validation/activity errors) must default IsTransientFailure to
            // false so they keep being dead-lettered immediately, not retried.
            var result = WorkflowExecutionResult.Failure("Activity 'Divide' failed: divide by zero.");

            Assert.IsFalse(result.IsSuccess);
            Assert.IsFalse(result.IsTransientFailure);
        }

        // ── WOLF-8512 (build 30658): "Workflow not found." is not always terminal ──────
        //
        // Correlation id ...-000688 was dead-lettered with "Workflow not found." while 999
        // sibling messages executed the very same workflow successfully; build 30649 failed
        // all 1000 the same way. The File.Exists miss is now classified by whether the
        // workflows directory itself is usable, which is what IsWorkflowsDirectoryUnavailable
        // answers. Both directions are asserted: over-correcting into "always transient" would
        // make a genuinely-absent workflow burn its whole delivery budget before dead-lettering
        // anyway.

        [TestMethod]
        [TestCategory("UnitTest")]
        public void IsWorkflowsDirectoryUnavailable_DirectoryMissing_ClassifiesAsTransient()
        {
            var request = new WorkflowExecutionRequest
            {
                WorkflowName = "profiler/ProfilerMain",
                WorkflowsDirectory = Path.Combine(Path.GetTempPath(), "ww-does-not-exist-" + Guid.NewGuid().ToString("N")),
            };

            Assert.IsTrue(
                WorkflowExecutor.IsWorkflowsDirectoryUnavailable(request),
                "A workflows directory that does not exist means this INSTANCE is not ready (cold start / resources not staged), not that the workflow is absent. Must be transient so the broker retries elsewhere instead of destroying the message.");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void IsWorkflowsDirectoryUnavailable_DirectoryPresentButEmpty_ClassifiesAsTransient()
        {
            var dir = CreateTempDirectory();
            try
            {
                Assert.IsTrue(
                    WorkflowExecutor.IsWorkflowsDirectoryUnavailable(new WorkflowExecutionRequest
                    {
                        WorkflowName = "profiler/ProfilerMain",
                        WorkflowsDirectory = dir,
                    }),
                    "A present-but-empty workflows directory is incomplete staging — the exact shape build 30649 hit for all 1000 messages.");
            }
            finally
            {
                Directory.Delete(dir, recursive: true);
            }
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void IsWorkflowsDirectoryUnavailable_DirectoryPopulatedButWorkflowAbsent_StaysTerminal()
        {
            // The symmetry guard for the two tests above: a healthy instance that simply does
            // not have the requested workflow must NOT be retried.
            var dir = CreateTempDirectory();
            try
            {
                Directory.CreateDirectory(Path.Combine(dir, "profiler"));
                File.WriteAllText(Path.Combine(dir, "profiler", "SomeOtherWorkflow.bite"), "<x/>");

                Assert.IsFalse(
                    WorkflowExecutor.IsWorkflowsDirectoryUnavailable(new WorkflowExecutionRequest
                    {
                        WorkflowName = "profiler/ProfilerMain",
                        WorkflowsDirectory = dir,
                    }),
                    "The directory is staged and contains workflows — this workflow genuinely does not exist, which is terminal. Retrying it would burn the whole delivery budget and dead-letter anyway.");
            }
            finally
            {
                Directory.Delete(dir, recursive: true);
            }
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void IsWorkflowsDirectoryUnavailable_NoDirectoryConfigured_StaysTerminal()
        {
            Assert.IsFalse(
                WorkflowExecutor.IsWorkflowsDirectoryUnavailable(new WorkflowExecutionRequest { WorkflowName = "x" }),
                "Nothing configured to look in is a malformed request, not an unhealthy instance. Retrying cannot help.");
        }

        // ── WOLF-8512 (build 30658): OOM raised inside the activity chain ─────────────
        //
        // An OutOfMemoryException does not always reach Execute's dedicated catch clause —
        // raised inside the activity chain it is captured on DsfDataObject.ExecutionException
        // and surfaces as the generic "Workflow execution failed due to an unexpected error.",
        // which was dead-lettered as a business failure. Correlation id ...-000343 was lost
        // this way in the same second its neighbour ...-000342 hit a plainly-reported OOM on
        // the same instance. Classification must follow the exception chain, not the call site.

        [TestMethod]
        [TestCategory("UnitTest")]
        public void IsTransientExecutionException_BareOutOfMemory_IsTransient()
        {
            Assert.IsTrue(WorkflowExecutor.IsTransientExecutionException(
                new OutOfMemoryException("Insufficient memory to continue the execution of the program.")));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void IsTransientExecutionException_WrappedOutOfMemory_IsTransient()
        {
            // The shape that actually reaches ExecutionException: the activity chain wraps
            // freely, and a wrapped OOM must be classified identically to a bare one.
            var wrapped = new InvalidOperationException(
                "Activity 'Calculate TrackingDays' failed.",
                new OutOfMemoryException("Insufficient memory to continue the execution of the program."));

            Assert.IsTrue(WorkflowExecutor.IsTransientExecutionException(wrapped));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void IsTransientExecutionException_OutOfMemoryInsideAggregate_IsTransient()
        {
            var aggregate = new AggregateException(
                new InvalidOperationException("unrelated branch"),
                new OutOfMemoryException("Insufficient memory to continue the execution of the program."));

            Assert.IsTrue(WorkflowExecutor.IsTransientExecutionException(aggregate));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void IsTransientExecutionException_OrdinaryWorkflowException_StaysTerminal()
        {
            // Symmetry guard: the classifier must stay narrow. Anything matched here is retried
            // by the broker, so a wrongly-included type turns a permanent failure into repeated
            // load that still ends in the DLQ once the delivery count is exhausted.
            var ordinary = new InvalidOperationException(
                "Activity 'Divide' failed: divide by zero.",
                new ArgumentOutOfRangeException("denominator"));

            Assert.IsFalse(WorkflowExecutor.IsTransientExecutionException(ordinary));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void IsTransientExecutionException_Null_StaysTerminal()
        {
            // A failed execution with no ExecutionException at all (errors collected from the
            // environment instead) is ordinary business output — never retried.
            Assert.IsFalse(WorkflowExecutor.IsTransientExecutionException(null));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void IsTransientExecutionException_SelfReferencingChain_TerminatesAndStaysTerminal()
        {
            // Defensive: the walk is depth-bounded so a pathological chain cannot hang the
            // executor. Building a genuine cycle is not possible via InnerException, so nest
            // deeply enough to exceed the bound instead.
            Exception deep = new InvalidOperationException("innermost");
            for (var i = 0; i < 50; i++)
            {
                deep = new InvalidOperationException($"layer {i}", deep);
            }

            Assert.IsFalse(WorkflowExecutor.IsTransientExecutionException(deep));
        }

        private static string CreateTempDirectory()
        {
            var dir = Path.Combine(Path.GetTempPath(), "ww-wf-dir-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            return dir;
        }
    }
}
