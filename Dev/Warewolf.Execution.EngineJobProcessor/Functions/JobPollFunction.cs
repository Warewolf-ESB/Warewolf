/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using Dev2.Common;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Warewolf.Execution.EngineJobProcessor.Services;

namespace Warewolf.Execution.EngineJobProcessor.Functions;

/// <summary>
/// Timer-driven replacement for hangfireserver.exe's worker loop.
///
/// Every <c>%JOB_POLL_SCHEDULE%</c> tick (default 1 minute; TimerTriggers are singleton
/// per app via the host's blob-lease lock, so overlapping/parallel polls cannot occur
/// across scaled-out instances):
///
/// <list type="number">
///   <item>Pages Hangfire storage for <b>Scheduled</b> jobs whose <c>EnqueueAt</c> (UTC)
///         has passed — jobs in any other state are invisible to the poller, which is
///         what makes a timed-out <c>Processing</c> job non-re-invocable (fail-only,
///         see the reaper).</item>
///   <item>Fire-and-forget dispatches each due job to the Execution Engine's secured
///         resume route and awaits only the engine's short ack window (the engine
///         responds 200 on completion — it executes synchronously; a long continuation
///         elapses the window and reconciles next tick). Duplicate dispatches are
///         harmless — the engine's atomic Scheduled→Processing claim admits exactly
///         one winner (losers get 409).</item>
/// </list>
///
/// A failed dispatch leaves the job Scheduled; it is retried on the next tick.
/// </summary>
public sealed class JobPollFunction
{
    const int PageSize = 100;

    readonly JobStorageProvider _storageProvider;
    readonly IEngineResumeClient _resumeClient;
    readonly ILogger<JobPollFunction> _logger;

    public JobPollFunction(
        JobStorageProvider storageProvider,
        IEngineResumeClient resumeClient,
        ILogger<JobPollFunction> logger)
    {
        _storageProvider = storageProvider;
        _resumeClient = resumeClient;
        _logger = logger;
    }

    [Function("JobPoll")]
    public async Task Run([TimerTrigger("%JOB_POLL_SCHEDULE%")] TimerInfo timerInfo, CancellationToken cancellationToken)
    {
        if (!Config.Persistence.Enable)
        {
            _logger.LogDebug("JobPoll | Persistence disabled — nothing to poll.");
            return;
        }

        var nowUtc = DateTime.UtcNow;
        var monitoring = _storageProvider.Storage.GetMonitoringApi();

        var scanned = 0;
        var dispatched = 0;
        var alreadyClaimed = 0;
        var ackTimeouts = 0;
        var failed = 0;

        for (var from = 0; ; from += PageSize)
        {
            var page = monitoring.ScheduledJobs(from, PageSize);
            if (page.Count == 0)
            {
                break;
            }

            foreach (var (jobId, job) in page)
            {
                scanned++;

                // ScheduledJobDto.EnqueueAt is UTC — the single due-comparison in the system.
                if (job is null || job.EnqueueAt > nowUtc)
                {
                    continue;
                }

                var outcome = await _resumeClient.TryResumeAsync(jobId, cancellationToken);
                switch (outcome)
                {
                    case ResumeDispatchOutcome.Dispatched:     dispatched++;     break;
                    case ResumeDispatchOutcome.AlreadyClaimed: alreadyClaimed++; break;
                    case ResumeDispatchOutcome.AckTimeout:     ackTimeouts++;    break;
                    default:                                   failed++;         break;
                }
            }

            if (page.Count < PageSize)
            {
                break;
            }
        }

        _logger.LogInformation(
            "JobPoll | Completed | Scanned={Scanned} Dispatched={Dispatched} AlreadyClaimed={AlreadyClaimed} AckTimeouts={AckTimeouts} Failed={Failed} | NowUtc={NowUtc:O}",
            scanned, dispatched, alreadyClaimed, ackTimeouts, failed, nowUtc);
    }
}
