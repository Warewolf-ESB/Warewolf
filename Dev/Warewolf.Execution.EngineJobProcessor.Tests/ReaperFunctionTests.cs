/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using Hangfire;
using Hangfire.States;
using Hangfire.Storage;
using Hangfire.Storage.Monitoring;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Execution.EngineJobProcessor.Functions;
using Warewolf.Execution.EngineJobProcessor.Services;

namespace Warewolf.Execution.EngineJobProcessor.Tests;

/// <summary>
/// Pins the FAIL-ONLY reaper contract (resolved plan decision #1): only Processing jobs
/// older than the stale threshold are transitioned — via an atomic CAS expecting the
/// <c>Processing</c> state — to <c>Failed</c>; fresh jobs are untouched; nothing is ever
/// re-scheduled or re-enqueued; disabled persistence is a no-op.
/// </summary>
[TestClass]
[DoNotParallelize] // swaps process-global Config.Persistence
public class ReaperFunctionTests
{
    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task Run_StaleProcessingJob_MarkedFailed_WithProcessingCas()
    {
        using var _ = TestSupport.SwapPersistence(enable: true);
        var settings = new ProcessorSettings { StaleMinutes = 15 };

        var monitoring = new Mock<IMonitoringApi>();
        monitoring.Setup(m => m.ProcessingJobs(0, It.IsAny<int>())).Returns(new JobList<ProcessingJobDto>(new[]
        {
            Processing("stale", DateTime.UtcNow.AddMinutes(-60)),
            Processing("fresh", DateTime.UtcNow.AddMinutes(-1)),
            Processing("no-started-at", startedAtUtc: null),
        }));

        var client = new Mock<IBackgroundJobClient>();
        client.Setup(c => c.ChangeState("stale", It.IsAny<FailedState>(), ProcessingState.StateName)).Returns(true);

        await NewFunction(monitoring, client, settings).Run(new TimerInfo());

        client.Verify(c => c.ChangeState("stale", It.IsAny<FailedState>(), ProcessingState.StateName), Times.Once,
            "The stale job must be failed via CAS expecting the Processing state.");
        client.Verify(c => c.ChangeState(It.IsAny<string>(), It.IsAny<IState>(), It.IsAny<string>()), Times.Once,
            "FAIL-ONLY: no other transition (re-schedule, re-enqueue) may ever be issued.");
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task Run_CasLost_EngineFinishedMeanwhile_IsBenign()
    {
        using var _ = TestSupport.SwapPersistence(enable: true);
        var settings = new ProcessorSettings { StaleMinutes = 15 };

        var monitoring = new Mock<IMonitoringApi>();
        monitoring.Setup(m => m.ProcessingJobs(0, It.IsAny<int>())).Returns(new JobList<ProcessingJobDto>(new[]
        {
            Processing("finished-just-now", DateTime.UtcNow.AddMinutes(-60)),
        }));

        var client = new Mock<IBackgroundJobClient>();
        client.Setup(c => c.ChangeState(It.IsAny<string>(), It.IsAny<IState>(), It.IsAny<string>()))
              .Returns(false); // CAS lost — the engine recorded a final state in the meantime

        await NewFunction(monitoring, client, settings).Run(new TimerInfo()); // must not throw
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task Run_PersistenceDisabled_NeverTouchesStorage()
    {
        using var _ = TestSupport.SwapPersistence(enable: false);

        var monitoring = new Mock<IMonitoringApi>(MockBehavior.Strict);
        var client = new Mock<IBackgroundJobClient>(MockBehavior.Strict);

        await NewFunction(monitoring, client, new ProcessorSettings()).Run(new TimerInfo());
    }

    static ReaperFunction NewFunction(Mock<IMonitoringApi> monitoring, Mock<IBackgroundJobClient> client, ProcessorSettings settings)
    {
        var storage = new Mock<JobStorage>();
        storage.Setup(s => s.GetMonitoringApi()).Returns(monitoring.Object);

        return new ReaperFunction(
            new JobStorageProvider(storage.Object, client.Object),
            settings,
            NullLogger<ReaperFunction>.Instance);
    }

    static KeyValuePair<string, ProcessingJobDto> Processing(string jobId, DateTime? startedAtUtc) =>
        new(jobId, new ProcessingJobDto { StartedAt = startedAtUtc, InProcessingState = true });
}
