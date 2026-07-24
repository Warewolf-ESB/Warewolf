/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Phase-8 end-to-end RESUME tests against the REAL demo suspend workflow
 *  (hangfiredemo\Suspend Execution Example.bite + its Hello World sub-workflow), staged
 *  as committed fixtures under TestResources\. Both the scheduled-resume path
 *  (WorkflowResumeFunction -> ResumptionExecutor.TryClaim + ExecuteClaimed) and the manual
 *  no-override path (IResumptionExecutor.Execute, the driver seam) are exercised, executing
 *  the workflow's real continuation (Assign -> Hello World sub-workflow -> Write File) to
 *  Succeeded on real Hangfire storage (Hangfire.MemoryStorage).
 *
 *  Hermetic: the continuation's Write File activity targets D:\jobdata.txt in the demo; the
 *  staged copy redirects it to a temp file so the test writes no side effects outside its
 *  own temp dir and needs no external SQL / D: drive. The SUSPEND side (creating the job) is
 *  covered by Dev2.Activities.Tests\SuspendExecutionActivityTests + the driver ScheduleJob
 *  tests — here the job is seeded with exactly the values SuspendExecutionActivity persists
 *  (SuspendExecutionActivity.cs:164-171): resourceID / environment / startActivityId /
 *  versionNumber / currentuserprincipal, with startActivityId = NextNodes.First (the Assign).
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
using Dev2JsonSerializer = Dev2.Common.Serializers.Dev2JsonSerializer;

namespace Warewolf.Execution.Lightweight.Tests.Execution
{
    [TestClass]
    [DoNotParallelize] // executes real workflows through the process-global LightweightSourceLoader + activity cache
    public class ResumeEndToEndTests
    {
        // From "Suspend Execution Example.bite": Service (resource) id, workflow version, and
        // the resume node = NextNodes.First after the SuspendExecutionActivity (the "Assign (1)").
        const string SuspendResourceId  = "ad45daa3-7676-4db3-b347-4aeb4d51335b";
        const string ResumeNodeUniqueId = "ad3b2218-77d6-4409-8123-f42b1f9515c9";
        const string WorkflowVersion    = "8";

        [TestMethod]
        [TestCategory("UnitTest")]
        public void ScheduledResume_RealSuspendWorkflow_ExecutesContinuation_ToSucceeded()
        {
            using var fixture = StageRealSuspendWorkflow();
            if (fixture is null) { Assert.Inconclusive("Demo workflow fixtures not found under TestResources."); return; }

            var (storage, client, jobId) = SeedRealSuspendJob(fixture);
            var executor = new ResumptionExecutor(new NoOpExecutionLogger(), storage, client, fixture.WorkflowsDir);

            // Scheduled path: claim (CAS Scheduled->Processing) then execute the continuation.
            var claim = executor.TryClaim(jobId);
            Assert.AreEqual(ResumeClaimOutcome.Claimed, claim.Outcome);

            var result = executor.ExecuteClaimed(jobId);

            Assert.IsTrue(result.Success,
                $"Resuming the real demo workflow must run the continuation to success. Error: {result.Error}");
            Assert.AreEqual(SucceededState.StateName, ResumeTestSupport.LastStateName(storage, jobId),
                "A successful resume records Succeeded via CAS on Processing.");
            Assert.IsTrue(File.Exists(fixture.JobDataFile),
                "The continuation's Write File activity must have written the (redirected) job-data file — proving the real continuation executed.");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void ManualResume_RealSuspendWorkflow_ViaSeam_ExecutesContinuation_NoError()
        {
            using var fixture = StageRealSuspendWorkflow();
            if (fixture is null) { Assert.Inconclusive("Demo workflow fixtures not found under TestResources."); return; }

            var (storage, client, _) = SeedRealSuspendJob(fixture);
            var executor = new ResumptionExecutor(new NoOpExecutionLogger(), storage, client, fixture.WorkflowsDir);

            // Manual no-override path: HangfireScheduler.ResumeJob invokes IResumptionExecutor.Execute
            // (values already decrypted). Mirrors "Manual Resumption Tool Example" -> the engine seam.
            var values = RealSuspendValues(fixture);
            var serialized = executor.Execute(values);

            var message = new Dev2JsonSerializer().Deserialize<ExecuteMessage>(serialized);
            Assert.IsFalse(message.HasError,
                $"Manual resumption via the seam must execute the continuation without error. Message: {message.Message}");
            Assert.IsTrue(File.Exists(fixture.JobDataFile),
                "The manually-resumed continuation must have executed the real workflow (redirected Write File).");
        }

        // ── fixture helpers ─────────────────────────────────────────────────────────

        sealed class WorkflowFixture : IDisposable
        {
            public string WorkflowsDir = "";
            public string SuspendBitePath = "";
            public string JobDataFile = "";
            public void Dispose() { try { Directory.Delete(WorkflowsDir, recursive: true); } catch (IOException) { } }
        }

        /// <summary>
        /// Copies the committed demo fixtures into a temp workflows dir, redirecting the
        /// continuation's D:\jobdata.txt Write File target to a temp file (hermetic).
        /// Returns null when the fixtures are not present.
        /// </summary>
        static WorkflowFixture? StageRealSuspendWorkflow()
        {
            var srcDir = Path.Combine(AppContext.BaseDirectory, "TestResources", "hangfiredemo");
            var suspendSrc = Path.Combine(srcDir, "Suspend Execution Example.bite");
            var helloSrc = Path.Combine(srcDir, "Hello World.bite");
            if (!File.Exists(suspendSrc) || !File.Exists(helloSrc)) return null;

            var dir = Directory.CreateTempSubdirectory("wwresume-e2e-").FullName;
            var jobDataFile = Path.Combine(dir, "jobdata.txt");

            var suspendDest = Path.Combine(dir, "Suspend Execution Example.bite");
            File.WriteAllText(suspendDest, File.ReadAllText(suspendSrc).Replace(@"D:\jobdata.txt", jobDataFile));
            File.Copy(helloSrc, Path.Combine(dir, "Hello World.bite"), overwrite: true);

            return new WorkflowFixture { WorkflowsDir = dir, SuspendBitePath = suspendDest, JobDataFile = jobDataFile };
        }

        static Dictionary<string, StringBuilder> RealSuspendValues(WorkflowFixture fixture)
        {
            var values = ResumeTestSupport.LegacyValues(
                resourceId: SuspendResourceId,
                startActivityId: ResumeNodeUniqueId,
                environment: "{}",
                versionNumber: WorkflowVersion,
                principal: "alice");
            values[LightweightJobValuesEnricher.WorkflowFilePathKey] = new StringBuilder(fixture.SuspendBitePath);
            return values;
        }

        static (MemoryStorage storage, BackgroundJobClient client, string jobId) SeedRealSuspendJob(WorkflowFixture fixture)
        {
            // No Config.Persistence swap needed: ResumptionExecutor works on the injected
            // MemoryStorage directly (the Enable gate lives in the route/poller, not here), and
            // the now-lazy SuspendExecutionActivity ctor only reads Config.Persistence.Enable
            // (any value) without building storage when the workflow is parsed.
            var storage = new MemoryStorage();
            var client = new BackgroundJobClient(storage);
            var jobId = ResumeTestSupport.SeedScheduledJob(storage, RealSuspendValues(fixture));
            return (storage, client, jobId);
        }
    }
}
