/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Linq;
using Hangfire;
using Hangfire.MemoryStorage;
using Hangfire.States;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Driver.Persistence;

namespace Warewolf.Driver.Drivers.HangfireScheduler.Tests
{
    /// <summary>Public, Hangfire-serialisable seed method for the external-state tests.</summary>
    public static class ExternalStateSeed
    {
        public static void Job() { }
    }

    /// <summary>
    /// Pins the driver-owned <see cref="ExternalProcessingState"/> / <see cref="ExternalSucceededState"/>.
    /// Hangfire's own <c>ProcessingState</c>/<c>SucceededState</c> have internal constructors (only a
    /// <c>BackgroundJobServer</c> worker can create them — which the Azure topology never runs), so the
    /// Execution Engine applies these equivalents. They MUST carry the canonical Hangfire state names and
    /// serialized-data keys, otherwise the monitoring API — and therefore the JobProcessor's reaper and the
    /// dashboard — would not recognise engine-executed jobs.
    /// </summary>
    [TestClass]
    [DoNotParallelize]
    [TestCategory("CannotParallelize")]
    public class ExternalJobStatesTests
    {
        [TestMethod]
        [Owner("Warewolf")]
        [TestCategory(nameof(HangfireScheduler))]
        public void ExternalProcessingState_MatchesHangfireProcessingContract()
        {
            var state = new ExternalProcessingState(serverId: "execution-engine:host", workerId: "worker-1");

            Assert.AreEqual(ProcessingState.StateName, state.Name);
            Assert.IsFalse(state.IsFinal);
            Assert.IsFalse(state.IgnoreJobLoadException);

            var data = state.SerializeData();
            CollectionAssert.AreEquivalent(new[] { "StartedAt", "ServerId", "WorkerId" }, data.Keys.ToArray(),
                "Monitoring reads ProcessingJobs by exactly these keys.");
            Assert.AreEqual("execution-engine:host", data["ServerId"]);
            Assert.AreEqual("worker-1", data["WorkerId"]);
        }

        [TestMethod]
        [Owner("Warewolf")]
        [TestCategory(nameof(HangfireScheduler))]
        public void ExternalSucceededState_MatchesHangfireSucceededContract()
        {
            var state = new ExternalSucceededState(latencyMilliseconds: 5, performanceDurationMilliseconds: 42);

            Assert.AreEqual(SucceededState.StateName, state.Name);
            Assert.IsTrue(state.IsFinal, "Succeeded is terminal — the state machine applies normal job expiration.");

            var data = state.SerializeData();
            CollectionAssert.AreEquivalent(new[] { "SucceededAt", "PerformanceDuration", "Latency" }, data.Keys.ToArray());
            Assert.AreEqual("42", data["PerformanceDuration"]);
            Assert.AreEqual("5", data["Latency"]);
        }

        [TestMethod]
        [Owner("Warewolf")]
        [TestCategory(nameof(HangfireScheduler))]
        public void ExternalStates_AppliedViaCas_AreVisibleToMonitoringApi_LikeWorkerStates()
        {
            var storage = new MemoryStorage();
            var client = new BackgroundJobClient(storage);
            var monitoring = storage.GetMonitoringApi();

            var jobId = client.Create(() => ExternalStateSeed.Job(), new ScheduledState(DateTime.UtcNow.AddDays(1)));

            // Scheduled → Processing (the engine's atomic claim).
            Assert.IsTrue(client.ChangeState(jobId, new ExternalProcessingState("engine", "w1"), ScheduledState.StateName));
            Assert.IsTrue(monitoring.ProcessingJobs(0, 50).Any(j => j.Key == jobId),
                "An engine-claimed job must appear in ProcessingJobs — that is what the reaper enumerates.");

            // Processing → Succeeded (terminal recording after execution).
            Assert.IsTrue(client.ChangeState(jobId, new ExternalSucceededState(0, 10), ProcessingState.StateName));
            Assert.IsTrue(monitoring.SucceededJobs(0, 50).Any(j => j.Key == jobId),
                "An engine-succeeded job must appear in SucceededJobs, indistinguishable from a worker-succeeded one.");
            Assert.AreEqual(SucceededState.StateName,
                monitoring.JobDetails(jobId)?.History?.OrderBy(s => s.CreatedAt).LastOrDefault()?.StateName);
        }
    }
}
