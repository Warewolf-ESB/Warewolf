/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Phase-5 tests for the /secure/resume/{suspensionId} route (WorkflowResumeFunction),
 *  driving the response mapping directly against a real ResumptionExecutor over an
 *  in-memory job store:
 *
 *    503 persistence disabled · 404 unknown job · 409 not claimable (already out of
 *    Scheduled) · 500 execution failed (job marked Failed, fail-only).
 *
 *  The 200/Succeeded mapping requires executing a real deployed workflow (.bite) against
 *  SQL-backed Hangfire storage and is covered by the end-to-end integration test.
 */

using Hangfire;
using Hangfire.MemoryStorage;
using Hangfire.States;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Warewolf.Driver.Persistence;
using Warewolf.Execution.Lightweight.Functions;
using Warewolf.Execution.Lightweight.Tests.Auth;
using Warewolf.Execution.Lightweight.Tests.Execution;

namespace Warewolf.Execution.Lightweight.Tests.Functions
{
    [TestClass]
    [DoNotParallelize] // swaps process-global Config.Persistence
    public class WorkflowResumeFunctionTests
    {
        static ResumptionExecutor NewExecutor(MemoryStorage storage) =>
            new(new NoOpExecutionLogger(), storage, new BackgroundJobClient(storage),
                Path.Combine(Path.GetTempPath(), "wwresume-route-none"));

        static async Task<(HttpStatusCode Status, JObject Body)> Invoke(ResumptionExecutor executor, string jobId)
        {
            var ctx = new HttpFunctionContext();
            var req = new FakeHttpRequestData(ctx, new Uri($"https://engine.test/secure/resume/{jobId}"), "POST");
            var function = new WorkflowResumeFunction(executor, NullLogger<WorkflowResumeFunction>.Instance);

            var response = await function.Resume(req, jobId, ctx);

            response.Body.Position = 0;
            using var reader = new StreamReader(response.Body);
            var text = await reader.ReadToEndAsync();
            return (response.StatusCode, JObject.Parse(text));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Resume_PersistenceDisabled_503()
        {
            using var _ = ResumeTestSupport.SwapPersistence(enable: false);

            var (status, body) = await Invoke(NewExecutor(new MemoryStorage()), "any");

            Assert.AreEqual(HttpStatusCode.ServiceUnavailable, status);
            Assert.AreEqual("persistence_disabled", body["error"]?.ToString());
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Resume_UnknownJob_404()
        {
            using var _ = ResumeTestSupport.SwapPersistence(enable: true);

            var (status, body) = await Invoke(NewExecutor(new MemoryStorage()), "no-such-job");

            Assert.AreEqual(HttpStatusCode.NotFound, status);
            Assert.AreEqual("not_found", body["error"]?.ToString());
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Resume_JobNotInScheduledState_409_WithCurrentState()
        {
            using var _ = ResumeTestSupport.SwapPersistence(enable: true);
            var storage = new MemoryStorage();
            var jobId = ResumeTestSupport.SeedScheduledJob(storage, ResumeTestSupport.LegacyValues());

            // Move it out of Scheduled so the route's claim CAS loses (duplicate-dispatch case).
            new BackgroundJobClient(storage).ChangeState(
                jobId, new ExternalProcessingState("test", "worker"), ScheduledState.StateName);

            var (status, body) = await Invoke(NewExecutor(storage), jobId);

            Assert.AreEqual(HttpStatusCode.Conflict, status);
            Assert.AreEqual("not_claimable", body["error"]?.ToString());
            Assert.AreEqual(ProcessingState.StateName, body["state"]?.ToString());
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Resume_ContinuationFails_500_JobMarkedFailed()
        {
            using var _ = ResumeTestSupport.SwapPersistence(enable: true);
            var storage = new MemoryStorage();
            // No startActivityId → the claimed continuation throws → recorded Failed.
            var jobId = ResumeTestSupport.SeedScheduledJob(storage, ResumeTestSupport.LegacyValues(startActivityId: ""));

            var (status, body) = await Invoke(NewExecutor(storage), jobId);

            Assert.AreEqual(HttpStatusCode.InternalServerError, status);
            Assert.AreEqual("resume_execution_failed", body["error"]?.ToString());
            Assert.AreEqual("Failed", body["state"]?.ToString());
            Assert.AreEqual(FailedState.StateName, ResumeTestSupport.LastStateName(storage, jobId));
        }
    }
}
