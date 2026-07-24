/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using Hangfire;
using Hangfire.Storage;
using Hangfire.Storage.Monitoring;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Driver.Persistence;
using Warewolf.Execution.EngineJobProcessor.Functions;
using Warewolf.Execution.EngineJobProcessor.Services;
using Hangfire.MemoryStorage;
using Hangfire.States;

namespace Warewolf.Execution.EngineJobProcessor.Tests;

/// <summary>
/// Pins due-job selection (the single UTC due-comparison): only Scheduled jobs with
/// <c>EnqueueAt &lt;= UtcNow</c> are dispatched; future jobs and disabled persistence are
/// skipped; one failed dispatch never aborts the tick.
/// </summary>
[TestClass]
[DoNotParallelize] // swaps process-global Config.Persistence
public class JobPollFunctionTests
{
    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task Run_DispatchesOnlyDueScheduledJobs()
    {
        using var _ = TestSupport.SwapPersistence(enable: true);

        var monitoring = new Mock<IMonitoringApi>();
        monitoring.Setup(m => m.ScheduledJobs(0, It.IsAny<int>())).Returns(new JobList<ScheduledJobDto>(new[]
        {
            Scheduled("due-past",     DateTime.UtcNow.AddMinutes(-5)),
            Scheduled("due-boundary", DateTime.UtcNow.AddSeconds(-1)),
            Scheduled("not-due",      DateTime.UtcNow.AddHours(1)),
        }));

        var dispatched = new List<string>();
        var resumeClient = new RecordingResumeClient(dispatched, ResumeDispatchOutcome.Dispatched);

        await NewFunction(monitoring, resumeClient).Run(new TimerInfo(), CancellationToken.None);

        CollectionAssert.AreEqual(new[] { "due-past", "due-boundary" }, dispatched,
            "Exactly the due jobs, in storage order — the future job must never be dispatched.");
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task Run_FailedDispatch_DoesNotAbortTheTick()
    {
        using var _ = TestSupport.SwapPersistence(enable: true);

        var monitoring = new Mock<IMonitoringApi>();
        monitoring.Setup(m => m.ScheduledJobs(0, It.IsAny<int>())).Returns(new JobList<ScheduledJobDto>(new[]
        {
            Scheduled("first-fails", DateTime.UtcNow.AddMinutes(-5)),
            Scheduled("second-runs", DateTime.UtcNow.AddMinutes(-5)),
        }));

        var dispatched = new List<string>();
        var resumeClient = new RecordingResumeClient(dispatched, ResumeDispatchOutcome.Failed);

        await NewFunction(monitoring, resumeClient).Run(new TimerInfo(), CancellationToken.None);

        CollectionAssert.AreEqual(new[] { "first-fails", "second-runs" }, dispatched,
            "A Failed outcome for one job must not stop the remaining due jobs from dispatching.");
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task Run_PersistenceDisabled_NeverTouchesStorage()
    {
        using var _ = TestSupport.SwapPersistence(enable: false);

        var monitoring = new Mock<IMonitoringApi>(MockBehavior.Strict); // any call would throw
        var dispatched = new List<string>();

        await NewFunction(monitoring, new RecordingResumeClient(dispatched, ResumeDispatchOutcome.Dispatched))
            .Run(new TimerInfo(), CancellationToken.None);

        Assert.AreEqual(0, dispatched.Count);
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task Run_ReapedFailedAndProcessingJobs_NeverReSelected()
    {
        // FAIL-ONLY end-to-end: a job the reaper marked Failed — and a job currently
        // Processing (claimed) — must be INVISIBLE to the poller, which selects only
        // Scheduled jobs. Uses real MemoryStorage so the actual ScheduledJobs filtering
        // is exercised (not a mock), proving there is no re-invocation of terminal jobs.
        using var _ = TestSupport.SwapPersistence(enable: true);

        var storage = new MemoryStorage();
        var client = new BackgroundJobClient(storage);

        var due = TestSupport.SeedScheduled(storage, DateTime.UtcNow.AddMinutes(-5));

        var reaped = TestSupport.SeedScheduled(storage, DateTime.UtcNow.AddMinutes(-5));
        client.ChangeState(reaped, new FailedState(new Exception("reaped")) { Reason = "Exceeded execution budget" });

        var processing = TestSupport.SeedScheduled(storage, DateTime.UtcNow.AddMinutes(-5));
        client.ChangeState(processing, new ExternalProcessingState("srv", "wkr")); // Hangfire's ProcessingState ctor is internal

        var dispatched = new List<string>();
        var fn = new JobPollFunction(
            new JobStorageProvider(storage, client),
            new RecordingResumeClient(dispatched, ResumeDispatchOutcome.Dispatched),
            NullLogger<JobPollFunction>.Instance);

        await fn.Run(new TimerInfo(), CancellationToken.None);

        CollectionAssert.AreEqual(new[] { due }, dispatched,
            "Only the still-Scheduled due job is dispatched — a reaped Failed job and an in-flight Processing job are never re-selected by the poller.");
    }

    static JobPollFunction NewFunction(Mock<IMonitoringApi> monitoring, IEngineResumeClient resumeClient)
    {
        var storage = new Mock<JobStorage>();
        storage.Setup(s => s.GetMonitoringApi()).Returns(monitoring.Object);

        return new JobPollFunction(
            new JobStorageProvider(storage.Object, new Mock<IBackgroundJobClient>().Object),
            resumeClient,
            NullLogger<JobPollFunction>.Instance);
    }

    static KeyValuePair<string, ScheduledJobDto> Scheduled(string jobId, DateTime enqueueAtUtc) =>
        new(jobId, new ScheduledJobDto { EnqueueAt = enqueueAtUtc, InScheduledState = true });

    sealed class RecordingResumeClient : IEngineResumeClient
    {
        readonly List<string> _dispatched;
        readonly ResumeDispatchOutcome _outcome;

        public RecordingResumeClient(List<string> dispatched, ResumeDispatchOutcome outcome)
        {
            _dispatched = dispatched;
            _outcome = outcome;
        }

        public Task<ResumeDispatchOutcome> TryResumeAsync(string jobId, CancellationToken cancellationToken)
        {
            _dispatched.Add(jobId);
            return Task.FromResult(_outcome);
        }
    }
}
