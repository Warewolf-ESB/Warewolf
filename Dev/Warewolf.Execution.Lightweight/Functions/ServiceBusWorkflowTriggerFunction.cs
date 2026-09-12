/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Warewolf.Execution.Lightweight.Auth;
using Warewolf.Execution.Lightweight.Auth.Models;
using Warewolf.Execution.Lightweight.Infrastructure;
using Warewolf.Execution.Lightweight.Models;
using Warewolf.Execution.Lightweight.Security;

namespace Warewolf.Execution.Lightweight.Functions;

/// <summary>
/// In-process Service Bus workflow trigger, trusting the TRANSPORT boundary only.
///
/// <para>
/// Triggered by a message on the dedicated <c>%WAREWOLF_SERVICEBUS_TRIGGER_QUEUE%</c>
/// queue (default <c>wwexecution-secure-trigger-queue</c>). A message is authorized to
/// execute simply because it arrived on this queue — Azure RBAC ("Data Sender" on
/// producers, "Data Receiver" on this Function App's own listen identity via its
/// identity-based Service Bus connection) is the ENTIRE trust boundary. There is no
/// per-message caller token, no jti replay check, and no
/// <see cref="IWorkflowPolicyMatcher"/> authorization call — every well-formed message on
/// the configured queue executes unconditionally, in-process, via
/// <see cref="IWorkflowExecutor"/> (no HTTP hop).
/// </para>
///
/// <para>
/// <b>Security posture (deliberate).</b> This is more permissive than even the anonymous
/// HTTP <c>/Public/*</c> route, which still gates on the workflow's "Public" group in
/// <c>secure.config</c> — there is no equivalent gate here. Anyone who can send to the
/// configured queue can run any workflow this engine can name. The only mitigation is
/// controlling who holds "Azure Service Bus Data Sender" on that queue/namespace; scope it
/// as tightly as the deployment allows. See <c>docs/ServiceBusSecureTrigger-Architecture.md</c>'s
/// "Threat model coverage" section.
/// </para>
///
/// <para><b>Flow.</b></para>
/// <list type="number">
///   <item>Parse the JSON body into <see cref="ServiceBusWorkflowMessage"/>; malformed → dead-letter, no retry.</item>
///   <item>Idempotency: a correlation id with an existing recorded result is a duplicate delivery/republish — complete without re-executing.</item>
///   <item>Wait for a free execution slot (<see cref="Auth.Models.ServiceBusTriggerOptions.MaxConcurrentExecutions"/>), itself bounded by
///         <see cref="Auth.Models.ServiceBusTriggerOptions.SlotWaitTimeout"/> so a slot held by a deadlocked execution cannot block a queued
///         delivery forever either, then execute in-process via <see cref="IWorkflowExecutor"/>, bounded by
///         <see cref="Auth.Models.ServiceBusTriggerOptions.ExecutionTimeout"/> — either bound expiring is treated as failed the same way an
///         unexpected exception is (see below), instead of leaving the delivery waiting forever with no result ever recorded. A genuine
///         business/activity failure is dead-lettered (not transient — retrying will not help).
///         A <see cref="WorkflowExecutionResult.IsTransientFailure"/> result (e.g. an <see cref="OutOfMemoryException"/> under Consumption-plan
///         cold-start memory pressure) is neither persisted nor dead-lettered — it is thrown instead, same as an unexpected exception below.
///         An unexpected exception (thrown, not returned) is rethrown so the Service Bus extension applies its normal retry/backoff and eventual max-delivery-count dead-letter.</item>
///   <item>Persist the terminal outcome (success, failure, or denial) via <see cref="IServiceBusReplayAndResultStore"/> for
///         <see cref="ServiceBusResultFunction"/>'s polling endpoint, and emit a structured audit event via <see cref="AuditLogger.LogServiceBusOutcome"/>.
///         WOLF-8512: every settlement call (complete or dead-letter) goes through <see cref="SettleAsync"/>, which uses an independent,
///         timeout-bounded token over <see cref="CancellationToken.None"/> rather than the host invocation's own — see that method's doc comment
///         for why a settlement timeout is safe (the result is always persisted first).</item>
/// </list>
///
/// <para>
/// No Azure Functions HTTP middleware pipeline applies to this trigger — the
/// EasyAuth/claims/policy middleware chain no-ops for non-HTTP triggers by design, and this
/// trigger does not opt into any authorization pipeline of its own.
/// </para>
/// </summary>
public sealed class ServiceBusWorkflowTriggerFunction
{
    /// <summary>
    /// Fixed caller label used everywhere this trigger would otherwise need a per-caller
    /// identity (audit events, <see cref="ServiceBusTriggerResult.Caller"/>, log messages) —
    /// there is no real caller identity any more, only "a message arrived on the trusted,
    /// configured queue".
    /// </summary>
    internal const string TrustedQueueCaller = "ServiceBus(trusted-queue)";

    private readonly IWorkflowExecutor _executor;
    private readonly IServiceBusReplayAndResultStore _store;
    private readonly AuditLogger _audit;
    private readonly HostEnvironmentConfig _config;
    private readonly ServiceBusTriggerOptions _triggerOptions;
    private readonly ServiceBusTriggerDefinition _triggerDefinition;
    private readonly SemaphoreSlim _executionConcurrencyLimiter;
    private readonly ILogger<ServiceBusWorkflowTriggerFunction> _logger;

    public ServiceBusWorkflowTriggerFunction(
        IWorkflowExecutor executor,
        IServiceBusReplayAndResultStore store,
        AuditLogger audit,
        HostEnvironmentConfig config,
        ServiceBusTriggerOptions triggerOptions,
        ServiceBusTriggerDefinition triggerDefinition,
        SemaphoreSlim executionConcurrencyLimiter,
        ILogger<ServiceBusWorkflowTriggerFunction> logger)
    {
        _executor                    = executor;
        _store                       = store;
        _audit                       = audit;
        _config                      = config;
        _triggerOptions              = triggerOptions;
        _triggerDefinition           = triggerDefinition;
        _executionConcurrencyLimiter = executionConcurrencyLimiter;
        _logger                      = logger;
    }

    [Function("ServiceBusWorkflowTrigger")]
    public async Task Run(
        [ServiceBusTrigger("%WAREWOLF_SERVICEBUS_TRIGGER_QUEUE%", Connection = "ServiceBusConnection")]
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions,
        CancellationToken cancellationToken)
    {
        var receivedAt = DateTimeOffset.UtcNow;
        var correlationId = !string.IsNullOrWhiteSpace(message.CorrelationId) ? message.CorrelationId : message.MessageId;

        ServiceBusWorkflowMessage? payload;
        try
        {
            payload = JsonConvert.DeserializeObject<ServiceBusWorkflowMessage>(message.Body.ToString());
        }
        catch (JsonException ex)
        {
            await RecordTerminalOutcomeAsync(
                messageActions, message, correlationId, workflow: _triggerDefinition.WorkflowName, caller: "(unknown)",
                status: ServiceBusTriggerStatus.Malformed, reason: $"Message body is not valid JSON: {ex.Message}",
                receivedAt, subReason: "BodyNotJson");
            return;
        }

        if (payload is null)
        {
            await RecordTerminalOutcomeAsync(
                messageActions, message, correlationId, workflow: _triggerDefinition.WorkflowName, caller: "(unknown)",
                status: ServiceBusTriggerStatus.Malformed, reason: "Message body deserialized to null.",
                receivedAt, subReason: "NullPayload");
            return;
        }

        if (!string.IsNullOrWhiteSpace(payload.CorrelationId))
        {
            correlationId = payload.CorrelationId;
        }

        // ── Business-idempotency dedupe ────────────────────────────────────────
        // TryClaim atomically reserves this correlationId for processing so a redelivery
        // landing while the FIRST attempt is still executing (i.e. before it has reached
        // SaveResult below) is caught here too - a plain TryGetResult-then-SaveResult check
        // leaves that whole execution window open to a duplicate run (see
        // IServiceBusReplayAndResultStore.TryClaim's doc comment).
        if (!_store.TryClaim(correlationId))
        {
            if (_store.TryGetResult(correlationId, out var existing) && existing != null)
            {
                _logger.LogInformation(
                    "ServiceBusWorkflowTrigger | CorrelationId={CorrelationId} | Duplicate message — result already recorded (Status={Status}); completing without re-execution.",
                    correlationId, existing.Status);
                await SettleAsync(ct => messageActions.CompleteMessageAsync(message, ct), correlationId, "CompleteMessageAsync (duplicate)").ConfigureAwait(false);
                return;
            }

            // No terminal result yet, but another delivery already holds the claim - it is
            // still executing (or failed without releasing it, which is itself a bug).
            // Do NOT execute a second time, and do NOT settle this delivery either way.
            // Deliberately NOT calling AbandonMessageAsync here: Abandon releases the lock
            // IMMEDIATELY, so if the first attempt is still genuinely executing, an
            // immediate redelivery would hit the same still-held claim and abandon again -
            // a tight retry storm that can exhaust MaxDeliveryAttempts (as low as 2) in
            // milliseconds, long before the first attempt ever finishes and calls
            // SaveResult. Service Bus would then dead-letter the message itself, silently,
            // outside this code entirely - permanent message loss with no recorded result.
            // Returning without any settlement leaves the lock to expire on its own natural
            // timing instead (host.json's extensions.serviceBus.autoCompleteMessages:false
            // means an unsettled return is never auto-completed either), giving the first
            // attempt its full lock duration before a genuine redelivery is even possible.
            // If the first attempt is not merely slow but actually dead or permanently hung
            // (no cancellation path reaches WorkflowExecutor.Execute — see the 1000-message
            // ShovelBridge load test incident of 2026-08-24), this claim would otherwise
            // block every future redelivery forever with no result ever recorded; TryClaim's
            // staleness check (IServiceBusReplayAndResultStore) is what eventually lets a
            // later redelivery take the claim over instead.
            _logger.LogInformation(
                "ServiceBusWorkflowTrigger | CorrelationId={CorrelationId} | Another delivery is already in flight for this correlation id — leaving this delivery unsettled instead of racing a duplicate execution.",
                correlationId);
            return;
        }

        // 1000-message ShovelBridge load test, 2026-09-01: 2 claimed correlation ids left
        // ZERO telemetry (no jobs1 row, no exception, no timeout warning) — the process
        // died before reaching any of this function's existing failure-path logging. This
        // marker (and the two below, bracketing the call into IWorkflowExecutor.Execute)
        // exist purely so a future silent death leaves a last-known-state trail.
        _logger.LogInformation(
            "ServiceBusWorkflowTrigger | CorrelationId={CorrelationId} | Workflow={Workflow} | Claim acquired — proceeding to execution (transport-trusted, no per-message token). ElapsedSinceReceivedMs={ElapsedMs}",
            correlationId, _triggerDefinition.WorkflowName, (DateTimeOffset.UtcNow - receivedAt).TotalMilliseconds);
        // WOLF-8512: mirrored durably (see IServiceBusReplayAndResultStore.RecordClaimStage) —
        // re-wired in after build 30651 (2026-09-12) reproduced the same claimed/vanished
        // signature as prior incidents; see docs/ServiceBusSecureTrigger-Architecture.md's
        // "Silent-death diagnostic breadcrumb" section.
        _store.RecordClaimStage(correlationId, "ClaimAcquired");

        await ProcessMessageAsync(payload, correlationId, message, messageActions, receivedAt, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Everything that happens once a message has been claimed for processing: execute
    /// the workflow unconditionally (no token, no jti, no <see cref="IWorkflowPolicyMatcher"/>
    /// check — see the class doc's "Security posture" note) and record/settle the outcome.
    ///
    /// <para>
    /// Extracted as its own internal (test-visible) entry point so unit tests can exercise
    /// the success / business-failure / unexpected-exception / timeout / concurrency
    /// branches deterministically without needing to also drive <see cref="Run"/>'s
    /// idempotency-claim logic each time.
    /// </para>
    /// </summary>
    internal async Task ProcessMessageAsync(
        ServiceBusWorkflowMessage payload,
        string correlationId,
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions,
        DateTimeOffset receivedAt,
        CancellationToken cancellationToken)
    {
        var principal = WorkflowClaimsPrincipal.Anonymous();

        // ── Execute in-process (no HTTP hop) ────────────────────────────────────
        var executionRequest = WorkflowFunctionHelper.CreateRequestByName(
            _triggerDefinition.WorkflowName, _config.WorkflowsDirectory, payload.Inputs ?? new Dictionary<string, string>());
        executionRequest.ExecutingPrincipal = principal;

        WorkflowExecutionResult result;

        // ── Bounded concurrency ─────────────────────────────────────────────────
        // Wait for a free execution slot rather than starting immediately - caps how many
        // workflow executions run at once on this instance (ServiceBusTriggerOptions.
        // MaxConcurrentExecutions) so a large burst is processed at a sustainable rate
        // instead of overwhelming a single instance's thread pool all at once. Service
        // Bus's own durable queue absorbs the rest while they wait; nothing is lost.
        //
        // The wait itself is bounded (ServiceBusTriggerOptions.SlotWaitTimeout), not just
        // the execution that follows it: a slot held by a genuinely deadlocked execution is
        // never released (ExecutionTimeout cannot reclaim it either - see that property's
        // "Known limitation"), so without a bound here a delivery queued behind it would
        // wait forever with no result, no error, and nothing ever reaching the DLQ (observed
        // directly in the 1000-message ShovelBridge load test incident of 2026-08-25, even
        // with ExecutionTimeout and MaxConcurrentExecutions both already in place).
        try
        {
            var acquiredSlot = await _executionConcurrencyLimiter
                .WaitAsync(_triggerOptions.SlotWaitTimeout, cancellationToken)
                .ConfigureAwait(false);
            if (!acquiredSlot)
            {
                _store.ReleaseClaim(correlationId);
                _logger.LogWarning(
                    "ServiceBusWorkflowTrigger | CorrelationId={CorrelationId} | Workflow={Workflow} | Timed out after {SlotWaitTimeout} waiting for a free execution slot ({MaxConcurrent} max concurrent) — leaving message for standard Service Bus retry instead of waiting indefinitely.",
                    correlationId, _triggerDefinition.WorkflowName, _triggerOptions.SlotWaitTimeout, _triggerOptions.MaxConcurrentExecutions);
                throw new TimeoutException(
                    $"Timed out waiting for a free execution slot for correlationId '{correlationId}' after {_triggerOptions.SlotWaitTimeout}.");
            }
        }
        catch (Exception ex) when (ex is not TimeoutException)
        {
            // Failed to even finish waiting (e.g. the trigger's own cancellation fired while
            // waiting). Release the claim so a redelivery can retry once capacity frees up.
            _store.ReleaseClaim(correlationId);
            _logger.LogError(
                ex,
                "ServiceBusWorkflowTrigger | CorrelationId={CorrelationId} | Workflow={Workflow} | Failed while waiting for a free execution slot — leaving message for standard Service Bus retry.",
                correlationId, _triggerDefinition.WorkflowName);
            throw;
        }

        // ── Bounded execution time ──────────────────────────────────────────────
        // IWorkflowExecutor.Execute is synchronous with no cancellation seam, so it cannot
        // be interrupted directly. Run it on the thread pool and race it against a timeout
        // instead: if it has not finished within ServiceBusTriggerOptions.ExecutionTimeout,
        // treat the delivery as failed exactly like the unexpected-exception path below
        // (release the claim, throw so Service Bus's own retry/backoff applies, eventually
        // dead-lettering once maxDeliveryCount is exhausted) rather than waiting forever
        // with no result ever recorded (see the 1000-message ShovelBridge load test
        // incidents of 2026-08-24/25).
        _logger.LogInformation(
            "ServiceBusWorkflowTrigger | CorrelationId={CorrelationId} | Workflow={Workflow} | About to call IWorkflowExecutor.Execute — last log point before the no-cancellation-seam call. ElapsedSinceReceivedMs={ElapsedMs} GcTotalMemoryBytes={GcTotalMemoryBytes} ProcessWorkingSetBytes={ProcessWorkingSetBytes}",
            correlationId, _triggerDefinition.WorkflowName, (DateTimeOffset.UtcNow - receivedAt).TotalMilliseconds, GC.GetTotalMemory(false), Environment.WorkingSet);
        // WOLF-8512: durable mirror, same reasoning as the ClaimAcquired stage above — this is
        // the last checkpoint before the no-cancellation-seam call, so it is the most likely
        // stage a future silent death is last recorded at.
        _store.RecordClaimStage(correlationId, $"AboutToExecute (GcTotalMemoryBytes={GC.GetTotalMemory(false)}, ProcessWorkingSetBytes={Environment.WorkingSet})");
        var executionTask = Task.Run(() => _executor.Execute(executionRequest));

        // The concurrency slot is released only once execution actually finishes (success,
        // failure, or - if we time out below - whenever the abandoned task eventually
        // completes on its own), not merely once we stop waiting on it: a timed-out
        // execution keeps running and keeps consuming real resources, so the slot it
        // occupies must stay charged against the cap until it genuinely frees up.
        _ = executionTask.ContinueWith(
            _ => _executionConcurrencyLimiter.Release(),
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default);

        var winner = await Task.WhenAny(executionTask, Task.Delay(_triggerOptions.ExecutionTimeout, cancellationToken))
            .ConfigureAwait(false);
        if (winner != executionTask)
        {
            _store.ReleaseClaim(correlationId);
            _logger.LogWarning(
                "ServiceBusWorkflowTrigger | CorrelationId={CorrelationId} | Workflow={Workflow} | Execution exceeded the configured timeout ({Timeout}) — leaving message for standard Service Bus retry instead of waiting indefinitely. The abandoned execution keeps running in the background (IWorkflowExecutor.Execute has no cancellation seam) and will release its concurrency slot whenever it eventually finishes.",
                correlationId, _triggerDefinition.WorkflowName, _triggerOptions.ExecutionTimeout);
            throw new TimeoutException(
                $"Workflow execution for correlationId '{correlationId}' exceeded the configured timeout ({_triggerOptions.ExecutionTimeout}).");
        }

        try
        {
            result = await executionTask.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            // Unexpected — NOT a policy/token/malformed-message failure. Let it bubble so
            // the Service Bus extension applies its standard retry/backoff and eventual
            // max-delivery-count dead-letter, per the spec's "no retry on non-transient
            // failures ONLY" requirement. Release the claim taken above so the eventual
            // redelivery this retry produces isn't permanently blocked by this failed
            // attempt's own claim (see TryClaim's doc comment).
            _store.ReleaseClaim(correlationId);
            _logger.LogError(
                ex,
                "ServiceBusWorkflowTrigger | CorrelationId={CorrelationId} | Workflow={Workflow} | Unexpected execution exception — leaving message for standard Service Bus retry.",
                correlationId, _triggerDefinition.WorkflowName);
            throw;
        }

        _logger.LogInformation(
            "ServiceBusWorkflowTrigger | CorrelationId={CorrelationId} | Workflow={Workflow} | Execution task returned. IsSuccess={IsSuccess} ElapsedSinceReceivedMs={ElapsedMs}",
            correlationId, _triggerDefinition.WorkflowName, result.IsSuccess, (DateTimeOffset.UtcNow - receivedAt).TotalMilliseconds);
        // WOLF-8512: durable mirror, same reasoning as the checkpoints above.
        _store.RecordClaimStage(correlationId, $"ExecutionReturned (IsSuccess={result.IsSuccess})");

        if (!result.IsSuccess && result.IsTransientFailure)
        {
            // Transient (e.g. an OutOfMemoryException during compile/execute — the documented
            // Consumption-plan cold-start memory-pressure signature, see WorkflowExecutor.Execute's
            // dedicated catch clause), NOT a terminal business outcome. Deliberately do NOT persist
            // a result here: a persisted Failed result would satisfy the idempotency dedupe check
            // above on redelivery, completing the retried message without ever re-executing it. Do
            // NOT dead-letter either — throw so the Service Bus extension applies its standard
            // retry/backoff, exactly like the unexpected-exception path above, and only dead-letters
            // once maxDeliveryCount is exhausted, instead of failing the whole run on the first hit.
            var transientError = string.Join("; ", result.Errors);
            _store.ReleaseClaim(correlationId);
            _logger.LogWarning(
                "ServiceBusWorkflowTrigger | CorrelationId={CorrelationId} | Workflow={Workflow} | Transient execution failure ({Error}) — leaving message for standard Service Bus retry instead of dead-lettering.",
                correlationId, _triggerDefinition.WorkflowName, transientError);
            throw new InvalidOperationException(
                $"Transient workflow execution failure for correlationId '{correlationId}': {transientError}");
        }

        var outcome = new ServiceBusTriggerResult
        {
            CorrelationId  = correlationId,
            Status         = result.IsSuccess ? ServiceBusTriggerStatus.Succeeded : ServiceBusTriggerStatus.Failed,
            Workflow       = _triggerDefinition.WorkflowName,
            Caller         = TrustedQueueCaller,
            Outputs        = result.IsSuccess ? await result.ReadPayloadAsync(cancellationToken).ConfigureAwait(false) : null,
            Error          = result.IsSuccess ? null : string.Join("; ", result.Errors),
            ReceivedAtUtc  = receivedAt,
            CompletedAtUtc = DateTimeOffset.UtcNow,
        };
        _store.SaveResult(outcome);

        if (result.IsSuccess)
        {
            _logger.LogInformation(
                "ServiceBusWorkflowTrigger | CorrelationId={CorrelationId} | Workflow={Workflow} | Caller={Caller} | Succeeded | DurationMs={DurationMs}",
                correlationId, _triggerDefinition.WorkflowName, TrustedQueueCaller, result.Duration.TotalMilliseconds);
            await SettleAsync(ct => messageActions.CompleteMessageAsync(message, ct), correlationId, "CompleteMessageAsync (success)").ConfigureAwait(false);
        }
        else
        {
            _audit.LogServiceBusOutcome("Failed", TrustedQueueCaller, _triggerDefinition.WorkflowName, outcome.Error ?? "Workflow execution failed.", correlationId);
            // A workflow/activity failure is a business outcome, not a transient transport
            // failure — dead-letter rather than let Service Bus retry an execution that will
            // fail identically every time.
            await SettleAsync(
                ct => messageActions.DeadLetterMessageAsync(
                    message,
                    // WOLF-8512: "Failed:ExecutionFailed" — Status:SubReason, consistent with
                    // RecordTerminalOutcomeAsync's terminal paths (see its subReason parameter).
                    // This path builds its own ServiceBusTriggerResult/DeadLetterMessageAsync call
                    // rather than going through RecordTerminalOutcomeAsync because it always has
                    // exactly one sub-reason - there is no classification to share.
                    deadLetterReason: "Failed:ExecutionFailed",
                    deadLetterErrorDescription: TruncateDeadLetterDescription(outcome.Error ?? "Workflow execution failed."),
                    cancellationToken: ct),
                correlationId,
                "DeadLetterMessageAsync (execution failed)").ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Persists a non-success terminal outcome, emits the structured audit event, and
    /// dead-letters the message (no retry) — the shared tail for every deny/invalid/malformed
    /// path above.
    /// </summary>
    /// <param name="subReason">
    /// WOLF-8512: optional dead-letter triage sub-reason (see
    /// <c>docs/ServiceBusSecureTrigger-Architecture.md</c>'s "Failure classification" section).
    /// When supplied, <c>deadLetterReason</c> becomes <c>"{status}:{subReason}"</c> (e.g.
    /// <c>Malformed:BodyNotJson</c>) instead of the bare status, so DLQ entries sharing a
    /// status are distinguishable without parsing <c>deadLetterErrorDescription</c>. Existing
    /// filters matching on the bare status still match, since it remains the string's prefix.
    /// </param>
    private async Task RecordTerminalOutcomeAsync(
        ServiceBusMessageActions messageActions,
        ServiceBusReceivedMessage message,
        string correlationId,
        string workflow,
        string caller,
        ServiceBusTriggerStatus status,
        string reason,
        DateTimeOffset receivedAt,
        string? subReason = null)
    {
        _store.SaveResult(new ServiceBusTriggerResult
        {
            CorrelationId  = correlationId,
            Status         = status,
            Workflow       = workflow,
            Caller         = caller,
            Error          = reason,
            ReceivedAtUtc  = receivedAt,
            CompletedAtUtc = DateTimeOffset.UtcNow,
        });

        _audit.LogServiceBusOutcome(status.ToString(), caller, workflow, reason, correlationId);

        var deadLetterReason = subReason is null ? status.ToString() : $"{status}:{subReason}";
        await SettleAsync(
            ct => messageActions.DeadLetterMessageAsync(
                message,
                deadLetterReason: deadLetterReason,
                deadLetterErrorDescription: TruncateDeadLetterDescription(reason),
                cancellationToken: ct),
            correlationId,
            $"DeadLetterMessageAsync ({deadLetterReason})").ConfigureAwait(false);
    }

    /// <summary>
    /// WOLF-8512: settles a message (complete or dead-letter) with a short, independent
    /// timeout over <see cref="CancellationToken.None"/> — never the host invocation's own
    /// <c>cancellationToken</c> (§1.3's root cause: during shutdown/drain that token is
    /// already cancelled, so a settlement call using it throws immediately and leaves the
    /// message unsettled even though its outcome was already decided).
    ///
    /// <para>
    /// <b>Why a timeout here is safe.</b> Every caller of this method has already called
    /// <c>SaveResult</c> (or is settling a duplicate whose result was already found) before
    /// settling. If settlement itself times out, the message is simply left unsettled: the
    /// Service Bus lock expires, the message redelivers, and the idempotency dedupe check
    /// (see the top of <c>Run</c>) finds the already-saved result and completes it then —
    /// self-healing at the cost of one extra delivery, never a lost or duplicated outcome.
    /// </para>
    /// </summary>
    private async Task SettleAsync(Func<CancellationToken, Task> settle, string correlationId, string operation)
    {
        using var settlementCts = new CancellationTokenSource(_triggerOptions.SettlementTimeout);
        try
        {
            await settle(settlementCts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (settlementCts.IsCancellationRequested)
        {
            _logger.LogWarning(
                "ServiceBusWorkflowTrigger | CorrelationId={CorrelationId} | Settlement operation '{Operation}' did not complete within {SettlementTimeout} — leaving the message unsettled. The already-persisted result will be found by the idempotency dedupe check on the next redelivery once the lock expires (self-healing at the cost of one extra delivery).",
                correlationId, operation, _triggerOptions.SettlementTimeout);
        }
    }

    /// <summary>
    /// Azure Service Bus rejects a <c>deadLetterErrorDescription</c> longer than 4096 characters
    /// with <c>ArgumentOutOfRangeException</c>. That surfaces as an <c>RpcException</c> from the
    /// settlement service and leaves the message <b>unsettled</b>, so it is redelivered and
    /// re-executed instead of being dead-lettered — turning one poison message into repeated
    /// load. Warewolf SQL failures routinely exceed the limit (full exception text plus stack
    /// trace), so the description is truncated here rather than at each call site.
    /// </summary>
    internal const int MaxDeadLetterDescriptionLength = 4096;

    internal static string TruncateDeadLetterDescription(string description)
    {
        if (string.IsNullOrEmpty(description) || description.Length <= MaxDeadLetterDescriptionLength)
        {
            return description;
        }

        const string suffix = "... [truncated]";
        return description.Substring(0, MaxDeadLetterDescriptionLength - suffix.Length) + suffix;
    }
}
