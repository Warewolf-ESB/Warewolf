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
    }
}
