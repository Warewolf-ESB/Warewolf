/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Phase-5 unit tests for ResumptionExecutor — the engine-side replacement for the
 *  Server's WorkflowResume endpoint + ResumableExecutionContainer. These pin the parts
 *  that do NOT require executing a real workflow (which needs a deployed .bite + SQL
 *  storage — covered by the deferred end-to-end integration test):
 *
 *    - TryClaim         : atomic CAS Scheduled→Processing (the single duplicate-prevention
 *                         point) — NotFound / Claimed / Conflict, against an in-memory store.
 *    - ExecuteClaimed   : FAIL-ONLY terminal recording — a continuation that throws is
 *                         marked Failed via CAS on Processing and never re-scheduled.
 *    - Execute(values)  : the manual no-override seam never throws — failures come back as
 *                         an ExecuteMessage with HasError=true (parity with WorkflowResume).
 *    - ResolveWorkflowFile : stamped-path fast-path → resource-ID scan → fail-fast.
 *    - ExecuteOverrideContinuation : no-op when StartActivityId is empty.
 */

using Hangfire;
using Hangfire.MemoryStorage;
using Hangfire.States;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Dev2.Communication;
using Dev2.DynamicServices;
using Warewolf.Driver.Persistence;
using Dev2JsonSerializer = Dev2.Common.Serializers.Dev2JsonSerializer;

namespace Warewolf.Execution.Lightweight.Tests.Execution
{
    [TestClass]
    public class ResumptionExecutorTests
    {
        static ResumptionExecutor NewExecutor(MemoryStorage storage, string workflowsDirectory = null!) =>
            new(new NoOpExecutionLogger(),
                storage,
                storage is null ? null! : new BackgroundJobClient(storage),
                workflowsDirectory ?? Path.Combine(Path.GetTempPath(), "wwresume-none"));

        // ── TryClaim ───────────────────────────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void TryClaim_UnknownJob_NotFound()
        {
            var result = NewExecutor(new MemoryStorage()).TryClaim("does-not-exist");

            Assert.AreEqual(ResumeClaimOutcome.NotFound, result.Outcome);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void TryClaim_ScheduledJob_Claimed_AndNowProcessing()
        {
            var storage = new MemoryStorage();
            var jobId = ResumeTestSupport.SeedScheduledJob(storage, ResumeTestSupport.LegacyValues());

            var result = NewExecutor(storage).TryClaim(jobId);

            Assert.AreEqual(ResumeClaimOutcome.Claimed, result.Outcome);
            Assert.AreEqual(ProcessingState.StateName, result.CurrentState);
            Assert.AreEqual(ProcessingState.StateName, ResumeTestSupport.LastStateName(storage, jobId),
                "A claimed job must be in Processing — visible to the reaper exactly like a worker-claimed job.");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void TryClaim_SecondClaimAfterFirst_Conflict()
        {
            var storage = new MemoryStorage();
            var jobId = ResumeTestSupport.SeedScheduledJob(storage, ResumeTestSupport.LegacyValues());
            var executor = NewExecutor(storage);

            Assert.AreEqual(ResumeClaimOutcome.Claimed, executor.TryClaim(jobId).Outcome);

            var second = executor.TryClaim(jobId);

            Assert.AreEqual(ResumeClaimOutcome.Conflict, second.Outcome,
                "Only one caller may win the Scheduled→Processing CAS — the loser is a benign duplicate dispatch.");
            Assert.AreEqual(ProcessingState.StateName, second.CurrentState);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void TryClaim_FailedJob_Conflict_NotClaimable()
        {
            var storage = new MemoryStorage();
            var jobId = ResumeTestSupport.SeedJobInState(storage, ResumeTestSupport.LegacyValues(),
                new FailedState(new Exception("reaped")) { Reason = "Exceeded execution budget" });

            var result = NewExecutor(storage).TryClaim(jobId);

            Assert.AreEqual(ResumeClaimOutcome.Conflict, result.Outcome,
                "A Failed job is terminal — the reaper's fail-only outcome must never be re-claimed and re-run.");
            Assert.AreEqual(FailedState.StateName, result.CurrentState);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void TryClaim_SucceededJob_Conflict_NotClaimable()
        {
            var storage = new MemoryStorage();
            var jobId = ResumeTestSupport.SeedJobInState(storage, ResumeTestSupport.LegacyValues(),
                new ExternalSucceededState(latencyMilliseconds: 0, performanceDurationMilliseconds: 0));

            var result = NewExecutor(storage).TryClaim(jobId);

            Assert.AreEqual(ResumeClaimOutcome.Conflict, result.Outcome,
                "An already-Succeeded job cannot be resumed again — parity with the driver's 'already resumed' guard.");
            Assert.AreEqual(SucceededState.StateName, result.CurrentState);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void TryClaim_ManuallyResumedJob_Conflict_NotClaimable()
        {
            var storage = new MemoryStorage();
            var jobId = ResumeTestSupport.SeedJobInState(storage, ResumeTestSupport.LegacyValues(),
                new ManuallyResumedState("{}"));

            var result = NewExecutor(storage).TryClaim(jobId);

            Assert.AreEqual(ResumeClaimOutcome.Conflict, result.Outcome,
                "A ManuallyResumed job is final — the scheduled route must not double-run a manually-resumed workflow.");
            Assert.AreEqual(ManuallyResumedState.StateName, result.CurrentState);
        }

        // ── ExecuteClaimed (FAIL-ONLY recording) ─────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void ExecuteClaimed_ContinuationThrows_RecordsFailed_NeverReschedules()
        {
            var storage = new MemoryStorage();
            // No startActivityId → the continuation throws before any workflow/file access.
            var values = ResumeTestSupport.LegacyValues(startActivityId: "");
            var jobId = ResumeTestSupport.SeedScheduledJob(storage, values);
            var executor = NewExecutor(storage);
            Assert.AreEqual(ResumeClaimOutcome.Claimed, executor.TryClaim(jobId).Outcome);

            var result = executor.ExecuteClaimed(jobId);

            Assert.IsFalse(result.Success);
            Assert.IsFalse(string.IsNullOrEmpty(result.Error));
            Assert.AreEqual(FailedState.StateName, ResumeTestSupport.LastStateName(storage, jobId),
                "FAIL-ONLY: a failed continuation is marked Failed (via CAS on Processing) and never re-scheduled.");
        }

        // ── Execute (manual no-override seam) never throws ───────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Execute_BadValues_ReturnsExecuteMessageWithError_DoesNotThrow()
        {
            // No storage needed — Execute operates purely on the supplied values.
            var executor = NewExecutor(storage: null);
            var values = ResumeTestSupport.LegacyValues(startActivityId: "");

            var serialized = executor.Execute(values);

            var message = new Dev2JsonSerializer().Deserialize<ExecuteMessage>(serialized);
            Assert.IsTrue(message.HasError,
                "Execute mirrors WorkflowResume.Execute: it never throws — failure is carried on the ExecuteMessage.");
            StringAssert.Contains(message.Message.ToString(), "startActivityId");
        }

        // ── ResolveWorkflowFile precedence ───────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void ResolveWorkflowFile_StampedPathExists_UsedAsFastPath()
        {
            var dir = Directory.CreateTempSubdirectory("wwresolve-").FullName;
            try
            {
                var stamped = Path.Combine(dir, "Suspend Me.bite");
                File.WriteAllText(stamped, "<Service ID=\"" + Guid.NewGuid() + "\" />");
                var values = ResumeTestSupport.LegacyValues();
                values[LightweightJobValuesEnricher.WorkflowFilePathKey] = new StringBuilder(stamped);

                var resolved = NewExecutor(storage: null, workflowsDirectory: dir)
                    .ResolveWorkflowFile(values, Guid.Empty);

                Assert.AreEqual(stamped, resolved);
            }
            finally { Directory.Delete(dir, recursive: true); }
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void ResolveWorkflowFile_NoStampedPath_ScansByResourceId()
        {
            var dir = Directory.CreateTempSubdirectory("wwresolve-").FullName;
            try
            {
                var resourceId = Guid.NewGuid();
                var expected = Path.Combine(dir, "Target.bite");
                File.WriteAllText(expected, $"<Service ID=\"{resourceId}\" Name=\"Target\" />");
                File.WriteAllText(Path.Combine(dir, "Other.bite"), $"<Service ID=\"{Guid.NewGuid()}\" Name=\"Other\" />");

                var resolved = NewExecutor(storage: null, workflowsDirectory: dir)
                    .ResolveWorkflowFile(ResumeTestSupport.LegacyValues(), resourceId);

                Assert.AreEqual(expected, resolved,
                    "With no stamped path (e.g. a Server-created job) the .bite is located by its resource ID.");
            }
            finally { Directory.Delete(dir, recursive: true); }
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void ResolveWorkflowFile_Unresolvable_FailsFast()
        {
            var dir = Directory.CreateTempSubdirectory("wwresolve-").FullName;
            try
            {
                var ex = Assert.ThrowsException<InvalidOperationException>(() =>
                    NewExecutor(storage: null, workflowsDirectory: dir)
                        .ResolveWorkflowFile(ResumeTestSupport.LegacyValues(), Guid.Empty));
                StringAssert.Contains(ex.Message, "Could not resolve");
            }
            finally { Directory.Delete(dir, recursive: true); }
        }

        // ── ExecuteOverrideContinuation no-op ────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void ExecuteOverrideContinuation_EmptyStartActivityId_IsNoOp()
        {
            var dataObject = new DsfDataObject(string.Empty, Guid.NewGuid()); // StartActivityId defaults to Guid.Empty

            // Must return without touching the file system, storage, or throwing.
            NewExecutor(storage: null).ExecuteOverrideContinuation(dataObject, ResumeTestSupport.LegacyValues());

            Assert.AreEqual(Guid.Empty, dataObject.StartActivityId);
        }
    }
}
