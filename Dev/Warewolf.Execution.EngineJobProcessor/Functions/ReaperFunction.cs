/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using Dev2.Common;
using Hangfire.States;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Warewolf.Execution.EngineJobProcessor.Services;

namespace Warewolf.Execution.EngineJobProcessor.Functions;

/// <summary>
/// FAIL-ONLY reaper (resolved plan decision #1) for suspend/resume jobs stuck in
/// <c>Processing</c> — typically because the Execution Engine's functionTimeout
/// (10 minutes on the Consumption plan) killed the resume execution mid-flight.
///
/// Every <c>%JOB_REAPER_SCHEDULE%</c> tick, jobs Processing for longer than
/// <c>%JOB_STALE_MINUTES%</c> are transitioned <c>Processing → Failed</c> with an
/// explicit budget-exceeded reason via an atomic compare-and-swap (the transition
/// silently loses if the engine finished in the meantime). A <c>Failed</c> job is
/// terminal: it is never re-invoked automatically — re-scheduling and hosting-tier
/// upgrades are deliberate human decisions (resolved plan decision #2), because a
/// partially-executed continuation may have side effects
/// ("re-queue with caution" — HangfireScheduler.cs).
/// </summary>
public sealed class ReaperFunction
{
    const int PageSize = 100;

    internal const string BudgetExceededReason =
        "Exceeded execution budget (engine functionTimeout or crash) — reaped by ExecutionEngineJobProcessor. " +
        "Fail-only policy: re-schedule manually only after verifying partial side effects.";

    readonly JobStorageProvider _storageProvider;
    readonly ProcessorSettings _settings;
    readonly ILogger<ReaperFunction> _logger;

    public ReaperFunction(
        JobStorageProvider storageProvider,
        ProcessorSettings settings,
        ILogger<ReaperFunction> logger)
    {
        _storageProvider = storageProvider;
        _settings = settings;
        _logger = logger;
    }

    [Function("JobReaper")]
    public Task Run([TimerTrigger("%JOB_REAPER_SCHEDULE%")] TimerInfo timerInfo)
    {
        if (!Config.Persistence.Enable)
        {
            _logger.LogDebug("JobReaper | Persistence disabled — nothing to reap.");
            return Task.CompletedTask;
        }

        var cutoffUtc = DateTime.UtcNow.AddMinutes(-_settings.StaleMinutes);
        var monitoring = _storageProvider.Storage.GetMonitoringApi();

        var scanned = 0;
        var reaped = 0;

        for (var from = 0; ; from += PageSize)
        {
            var page = monitoring.ProcessingJobs(from, PageSize);
            if (page.Count == 0)
            {
                break;
            }

            foreach (var (jobId, job) in page)
            {
                scanned++;

                if (job?.StartedAt is not { } startedAt || startedAt > cutoffUtc)
                {
                    continue;
                }

                // Atomic CAS: only applies while the job is still Processing. If the
                // engine recorded Succeeded/Failed in the meantime, this is a no-op.
                var applied = _storageProvider.Client.ChangeState(
                    jobId,
                    new FailedState(new Exception(BudgetExceededReason)) { Reason = "Exceeded execution budget" },
                    ProcessingState.StateName);

                if (applied)
                {
                    reaped++;
                    _logger.LogError(
                        "JobReaper | JobId={JobId} | Processing since {StartedAt:O} exceeded {StaleMinutes} min — marked Failed (fail-only; manual re-schedule/tier decision required).",
                        jobId, startedAt, _settings.StaleMinutes);
                }
            }

            if (page.Count < PageSize)
            {
                break;
            }
        }

        if (scanned > 0 || reaped > 0)
        {
            _logger.LogInformation("JobReaper | Completed | Scanned={Scanned} Reaped={Reaped} | CutoffUtc={CutoffUtc:O}", scanned, reaped, cutoffUtc);
        }

        return Task.CompletedTask;
    }
}
