using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace WwExecutionCaller.Functions;

/// <summary>
/// Timer-trigger that invokes a configured secure workflow on a schedule.
/// Default CRON <c>0 0 * * * *</c> fires once every hour, on the hour. The downstream Bearer
/// token is acquired/cached/injected automatically by <see cref="Auth.WwExecutionTokenHandler"/>.
///
/// The schedule and the target workflow (<c>WwExecution:ScheduledWorkflow</c>) are configurable;
/// for environment-driven schedules, replace the literal with a <c>%AppSetting%</c> token.
/// </summary>
public sealed class CallWorkflowOnTimer
{
    private readonly IWwExecutionDownstreamService _engine;
    private readonly WwExecutionCallerOptions _options;
    private readonly ILogger<CallWorkflowOnTimer> _logger;

    public CallWorkflowOnTimer(
        IWwExecutionDownstreamService engine,
        IOptions<WwExecutionCallerOptions> options,
        ILogger<CallWorkflowOnTimer> logger)
    {
        _engine = engine;
        _options = options.Value;
        _logger = logger;
    }

    [Function(nameof(CallWorkflowOnTimer))]
    public async Task RunAsync(
        [TimerTrigger("0 0 * * * *")] TimerInfo timer,
        CancellationToken cancellationToken)
    {
        var workflow = _options.ScheduledWorkflow;
        _logger.LogInformation(
            "Timer fired at {Now:u}; invoking scheduled workflow '{Workflow}'. Next run: {Next:u}.",
            DateTimeOffset.UtcNow, workflow, timer.ScheduleStatus?.Next);

        try
        {
            var result = await _engine
                .ExecuteSecureAsync(workflow, queryString: "Name=Scheduler", cancellationToken)
                .ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation(
                    "Scheduled workflow '{Workflow}' succeeded ({StatusCode}).",
                    workflow, result.StatusCode);
            }
            else
            {
                _logger.LogError(
                    "Scheduled workflow '{Workflow}' failed ({StatusCode}): {Body}",
                    workflow, result.StatusCode, result.Body);
            }
        }
        catch (Exception ex)
        {
            // Swallow-and-log so a transient downstream failure doesn't crash the host;
            // the next timer tick will retry.
            _logger.LogError(ex, "Scheduled invocation of '{Workflow}' threw.", workflow);
        }
    }
}
