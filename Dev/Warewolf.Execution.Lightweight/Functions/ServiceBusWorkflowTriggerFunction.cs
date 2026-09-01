/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System.Diagnostics.CodeAnalysis;
using System.Runtime.ExceptionServices;
using System.Security.Claims;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Warewolf.Execution.Lightweight.Auth;
using Warewolf.Execution.Lightweight.Auth.Models;
using Warewolf.Execution.Lightweight.Auth.Parsers;
using Warewolf.Execution.Lightweight.Infrastructure;
using Warewolf.Execution.Lightweight.Models;
using Warewolf.Execution.Lightweight.Security;

namespace Warewolf.Execution.Lightweight.Functions;

/// <summary>
/// Secure, in-process Service Bus workflow trigger — Model A of
/// <c>Spec-Secure-ServiceBus-Triggered-Execution.md</c>.
///
/// <para>
/// Triggered by a message on the dedicated <c>%WAREWOLF_SERVICEBUS_TRIGGER_QUEUE%</c>
/// queue (default <c>wwexecution-secure-trigger-queue</c> — distinct from the existing
/// <c>Warewolf.Execution.ServiceBusWorker</c> (Model B) worker's <c>wwexecution-queue</c>,
/// whose message contract has no per-caller token). Executes the workflow IN-PROCESS via
/// <see cref="IWorkflowExecutor"/> (no HTTP hop) after authenticating and authorizing the
/// caller through the SAME <see cref="IWorkflowPolicyMatcher"/> singleton the HTTP
/// <c>/secure</c> and <c>/services</c> routes use — there is exactly one authorization
/// evaluator in this engine, called identically from every transport (spec §5).
/// </para>
///
/// <para><b>Flow.</b></para>
/// <list type="number">
///   <item>Parse the JSON body into <see cref="ServiceBusWorkflowMessage"/>; malformed → dead-letter, no retry.</item>
///   <item>Idempotency: a correlation id with an existing recorded result is a duplicate delivery/republish — complete without re-executing.</item>
///   <item>Validate the caller's Entra token from the <c>Authorization</c> message application property via <see cref="EntraBearerTokenValidator"/>
///         bound to the DEDICATED <see cref="ServiceBusEntraAuthOptions"/> audience (never the general HTTP audience — confused-deputy prevention).
///         WOLF-8512: a failure here splits three ways — <see cref="IsOwnInvocationCancellation"/> (the trigger's own invocation was cancelled
///         mid-validation) and <see cref="Auth.Parsers.TokenValidationFailureClassifier.IsTransient"/> (every other shape of "the token could not
///         be evaluated at all" — a metadata/JWKS fetch failure, an unresolved signing key, or an IdentityModel metadata-retrieval error) are both
///         transient: <see cref="FailTransient"/> releases the claim and rethrows so Service Bus's own retry/backoff applies, with NO result
///         persisted. Everything else is a positively-decided bad token (bad signature against a resolved key, wrong audience/issuer, expired,
///         malformed) — terminal: dead-lettered with a triage sub-reason (see <c>docs/ServiceBusSecureTrigger-Architecture.md</c>'s "Failure
///         classification" section). An engine misconfiguration (<see cref="EntraBearerTokenValidator.IsEnabled"/> false) is also transient, for
///         the same reason — a config fix followed by redelivery should succeed.</item>
///   <item>Replay protection: register the token's <c>jti</c> (mirrored application property preferred, falls back to the token claim) — a repeat is dead-lettered, never retried.</item>
///   <item>Authorize via <see cref="IWorkflowPolicyMatcher.Evaluate"/> — denial is dead-lettered, never retried.</item>
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
/// EasyAuth/claims/policy middleware chain no-ops for non-HTTP triggers by design, so all
/// validation and authorization here is done directly and explicitly.
/// </para>
/// </summary>
public sealed class ServiceBusWorkflowTriggerFunction
{
    private readonly EntraBearerTokenValidator _tokenValidator;
    private readonly IWorkflowPolicyMatcher _policyMatcher;
    private readonly IWorkflowExecutor _executor;
    private readonly IServiceBusReplayAndResultStore _store;
    private readonly AuditLogger _audit;
    private readonly HostEnvironmentConfig _config;
    private readonly ServiceBusTriggerOptions _triggerOptions;
    private readonly SemaphoreSlim _executionConcurrencyLimiter;
    private readonly ILogger<ServiceBusWorkflowTriggerFunction> _logger;

    public ServiceBusWorkflowTriggerFunction(
        EntraBearerTokenValidator tokenValidator,
        IWorkflowPolicyMatcher policyMatcher,
        IWorkflowExecutor executor,
        IServiceBusReplayAndResultStore store,
        AuditLogger audit,
        HostEnvironmentConfig config,
        ServiceBusTriggerOptions triggerOptions,
        SemaphoreSlim executionConcurrencyLimiter,
        ILogger<ServiceBusWorkflowTriggerFunction> logger)
    {
        // tokenValidator MUST be DI-injected as a singleton (see ServiceCollectionExtensions
        // AUTH-09/SB), never `new`'d here: EntraBearerTokenValidator caches Entra's OIDC
        // metadata/JWKS internally, and this Function class is NOT explicitly registered in
        // DI, so the Functions isolated-worker host resolves it (and therefore would
        // resolve a `new`-here validator) PER INVOCATION. Under a burst of many concurrent
        // Service Bus messages that would mean one cold OIDC-metadata fetch per message —
        // hundreds of simultaneous outbound calls to login.microsoftonline.com — which
        // exhausts outbound connections/SNAT ports on a Consumption-plan Function App and
        // manifests as widespread IDX20803/IDX20804 + InvalidToken failures under load
        // (reproduced by the 1000-message ShovelBridge load test). Injecting the singleton
        // gives this trigger the same one-cache-for-app-lifetime behaviour that
        // BearerTokenPrincipalParser already has for the HTTP path.
        _tokenValidator              = tokenValidator;
        _policyMatcher               = policyMatcher;
        _executor                    = executor;
        _store                       = store;
        _audit                       = audit;
        _config                      = config;
        _triggerOptions              = triggerOptions;
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
                messageActions, message, correlationId, workflow: string.Empty, caller: "(unknown)",
                status: ServiceBusTriggerStatus.Malformed, reason: $"Message body is not valid JSON: {ex.Message}",
                receivedAt, subReason: "BodyNotJson");
            return;
        }

        if (payload is null || string.IsNullOrWhiteSpace(payload.Workflow))
        {
            await RecordTerminalOutcomeAsync(
                messageActions, message, correlationId, workflow: payload?.Workflow ?? string.Empty, caller: "(unknown)",
                status: ServiceBusTriggerStatus.Malformed, reason: "Message is missing the required 'workflow' field.",
                receivedAt, subReason: "MissingWorkflowField");
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
            "ServiceBusWorkflowTrigger | CorrelationId={CorrelationId} | Workflow={Workflow} | Claim acquired — proceeding to token validation. ElapsedSinceReceivedMs={ElapsedMs}",
            correlationId, payload.Workflow, (DateTimeOffset.UtcNow - receivedAt).TotalMilliseconds);

        // ── Token extraction ────────────────────────────────────────────────────
        if (!message.ApplicationProperties.TryGetValue("Authorization", out var authObj)
            || authObj is not string authHeader
            || string.IsNullOrWhiteSpace(authHeader))
        {
            await RecordTerminalOutcomeAsync(
                messageActions, message, correlationId, payload.Workflow, caller: "(unknown)",
                status: ServiceBusTriggerStatus.InvalidToken, reason: "Missing 'Authorization' message application property.",
                receivedAt, subReason: "MissingAuthorizationProperty");
            return;
        }

        if (!_tokenValidator.IsEnabled)
        {
            // WOLF-8512: engine misconfiguration (missing WAREWOLF_ENTRA_TENANT_ID /
            // WAREWOLF_ENTRA_SERVICEBUS_AUDIENCE), NOT a bad message — a config fix followed by
            // redelivery would succeed, so treat this as transient rather than the terminal
            // InvalidToken it used to be. A saved terminal result here would permanently poison
            // every correlation id received while the engine happened to be misconfigured, even
            // after an operator fixes the configuration and redelivers. Note the interaction with
            // EntraBearerTokenValidator.ValidateAsync (Auth/Parsers/EntraBearerTokenValidator.cs):
            // it independently throws InvalidOperationException when IsEnabled is false, which
            // TokenValidationFailureClassifier.IsTransient also classifies as transient (fail-open
            // default) - this branch and that one agree without needing special-case handling of
            // InvalidOperationException.
            FailTransient(
                correlationId,
                new InvalidOperationException(
                    "Service Bus token validation is not configured on this engine (missing WAREWOLF_ENTRA_TENANT_ID / WAREWOLF_ENTRA_SERVICEBUS_AUDIENCE)."),
                "ServiceBusWorkflowTrigger | CorrelationId={CorrelationId} | Service Bus token validation is not configured on this engine — engine misconfiguration, not a bad message; leaving message for standard Service Bus retry instead of permanently poisoning the correlation id.",
                correlationId);
        }

        const string bearerScheme = "Bearer ";
        var rawToken = authHeader.StartsWith(bearerScheme, StringComparison.OrdinalIgnoreCase)
            ? authHeader[bearerScheme.Length..].Trim()
            : authHeader.Trim();

        ClaimsIdentity identity;
        try
        {
            identity = await _tokenValidator.ValidateAsync(rawToken, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (IsOwnInvocationCancellation(ex, cancellationToken))
        {
            // WOLF-8512: transient, NOT an invalid token. EntraBearerTokenValidator.ValidateAsync
            // awaits _configManager.GetConfigurationAsync(cancellationToken) - a cold OIDC-metadata
            // fetch the FIRST time any invocation needs it after this app starts (the validator is
            // a DI singleton so this only happens once per app lifetime, not once per message - see
            // this class's own constructor comment - but that one cold fetch still has to complete
            // somewhere, and under host-level burst/cold-start pressure the invocation's own
            // cancellationToken can fire before it does). The pre-fix behaviour below treated this
            // identically to a genuinely bad token: a permanent InvalidToken result saved AND the
            // message dead-lettered on the very first attempt, with zero retry - exactly the same
            // misclassification bug already fixed for WorkflowExecutor.Execute's OutOfMemoryException
            // via WorkflowExecutionResult.IsTransientFailure (see that fix's own doc comment). Release
            // the claim and rethrow instead, so Service Bus's own retry/backoff applies - a retry
            // moments later almost always succeeds, since the cache is then warm regardless of which
            // attempt populated it.
            FailTransient(
                correlationId,
                ex,
                "ServiceBusWorkflowTrigger | CorrelationId={CorrelationId} | Token validation was cancelled (the trigger's own invocation cancellation fired mid-validation - most likely a cold OIDC-metadata fetch under host-level load, not an invalid token) — leaving message for standard Service Bus retry instead of a permanent InvalidToken dead-letter.",
                correlationId);
            return; // unreachable at runtime (FailTransient always throws) - satisfies definite-assignment analysis for 'identity' below.
        }
        catch (Exception ex) when (TokenValidationFailureClassifier.IsTransient(ex))
        {
            // WOLF-8512: broadens the transient set beyond "our own invocation was cancelled"
            // (the clause above) to every other shape of "the token could not be evaluated at
            // all" — HttpRequestException/IOException/SocketException from the OIDC metadata/
            // JWKS fetch itself failing (e.g. an internal HttpClient timeout that is NOT tied to
            // this invocation's own cancellationToken, so IsOwnInvocationCancellation above does
            // not catch it), SecurityTokenSignatureKeyNotFoundException (JWKS cache doesn't yet
            // have the signing key - a redelivery after the next refresh can succeed), and
            // IDX20803/IDX20804 (IdentityModel's own "could not retrieve/reload metadata"
            // codes). See TokenValidationFailureClassifier's doc comment for the full
            // transient/terminal split.
            FailTransient(
                correlationId,
                ex,
                "ServiceBusWorkflowTrigger | CorrelationId={CorrelationId} | Token validation failed with a transient cause ({ErrorMessage}) — leaving message for standard Service Bus retry instead of a permanent InvalidToken dead-letter.",
                correlationId, ex.Message);
            return; // unreachable at runtime (FailTransient always throws) - satisfies definite-assignment analysis for 'identity' below.
        }
        catch (Exception ex)
        {
            // Everything else is a positively-decided bad token (bad signature against a
            // resolved key, wrong audience/issuer, expired, malformed) — terminal, per
            // TokenValidationFailureClassifier.IsTransient having already returned false.
            await RecordTerminalOutcomeAsync(
                messageActions, message, correlationId, payload.Workflow, caller: "(unknown)",
                status: ServiceBusTriggerStatus.InvalidToken, reason: $"Token validation failed: {ex.Message}",
                receivedAt, subReason: TokenValidationFailureClassifier.ResolveSubReason(ex));
            return;
        }

        await ProcessAuthenticatedMessageAsync(
            identity, payload, correlationId, message, messageActions, receivedAt, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Everything that happens once the caller's token has already been validated:
    /// build the principal, check jti replay, authorize via the shared
    /// <see cref="IWorkflowPolicyMatcher"/>, execute, and record/settle the outcome.
    ///
    /// <para>
    /// Extracted as its own internal (test-visible) entry point specifically so unit
    /// tests can exercise the jti-replay / policy-denied / success / business-failure /
    /// unexpected-exception branches deterministically with a hand-built
    /// <see cref="ClaimsIdentity"/> — without requiring a live network round-trip to
    /// Entra's OIDC metadata endpoint (which <see cref="_tokenValidator"/> needs and
    /// which, per the existing <c>BearerTokenPrincipalParserTests</c> convention in this
    /// codebase, is deliberately left to the integration test suite). The token
    /// EXTRACTION and VALIDATION logic above this call is unchanged and untouched by
    /// this refactor.
    /// </para>
    /// </summary>
    internal async Task ProcessAuthenticatedMessageAsync(
        ClaimsIdentity identity,
        ServiceBusWorkflowMessage payload,
        string correlationId,
        ServiceBusReceivedMessage message,
        ServiceBusMessageActions messageActions,
        DateTimeOffset receivedAt,
        CancellationToken cancellationToken)
    {
        var principal = new WorkflowClaimsPrincipal(identity);

        // ── Replay protection ───────────────────────────────────────────────────
        var jti = ResolveJti(message, identity);
        if (!_store.TryRegisterJti(jti ?? string.Empty))
        {
            await RecordTerminalOutcomeAsync(
                messageActions, message, correlationId, payload.Workflow!, principal.CallerIdentity,
                status: ServiceBusTriggerStatus.InvalidToken, reason: "Token 'jti' has already been used — replay rejected.",
                receivedAt, subReason: "JtiReplay");
            return;
        }

        // ── Authorization — the ONE shared policy evaluator, same as the HTTP routes ──
        var policyResult = _policyMatcher.Evaluate(payload.Workflow!, principal);
        if (policyResult.Outcome is PolicyMatchOutcome.Forbidden or PolicyMatchOutcome.ConfigMissingDeny)
        {
            await RecordTerminalOutcomeAsync(
                messageActions, message, correlationId, payload.Workflow!, principal.CallerIdentity,
                status: ServiceBusTriggerStatus.Denied,
                reason: policyResult.DenialReason ?? "Denied by workflow authorization policy.",
                receivedAt,
                subReason: policyResult.Outcome == PolicyMatchOutcome.ConfigMissingDeny ? "ConfigMissingDeny" : "PolicyForbidden");
            return;
        }

        // ── Execute in-process (no HTTP hop) ────────────────────────────────────
        var executionRequest = WorkflowFunctionHelper.CreateRequestByName(
            payload.Workflow!, _config.WorkflowsDirectory, payload.Inputs ?? new Dictionary<string, string>());
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
                    correlationId, payload.Workflow, _triggerOptions.SlotWaitTimeout, _triggerOptions.MaxConcurrentExecutions);
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
                correlationId, payload.Workflow);
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
            correlationId, payload.Workflow, (DateTimeOffset.UtcNow - receivedAt).TotalMilliseconds, GC.GetTotalMemory(false), Environment.WorkingSet);
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
                correlationId, payload.Workflow, _triggerOptions.ExecutionTimeout);
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
                correlationId, payload.Workflow);
            throw;
        }

        _logger.LogInformation(
            "ServiceBusWorkflowTrigger | CorrelationId={CorrelationId} | Workflow={Workflow} | Execution task returned. IsSuccess={IsSuccess} ElapsedSinceReceivedMs={ElapsedMs}",
            correlationId, payload.Workflow, result.IsSuccess, (DateTimeOffset.UtcNow - receivedAt).TotalMilliseconds);

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
                correlationId, payload.Workflow, transientError);
            throw new InvalidOperationException(
                $"Transient workflow execution failure for correlationId '{correlationId}': {transientError}");
        }

        var outcome = new ServiceBusTriggerResult
        {
            CorrelationId  = correlationId,
            Status         = result.IsSuccess ? ServiceBusTriggerStatus.Succeeded : ServiceBusTriggerStatus.Failed,
            Workflow       = payload.Workflow,
            Caller         = principal.CallerIdentity,
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
                correlationId, payload.Workflow, principal.CallerIdentity, result.Duration.TotalMilliseconds);
            await SettleAsync(ct => messageActions.CompleteMessageAsync(message, ct), correlationId, "CompleteMessageAsync (success)").ConfigureAwait(false);
        }
        else
        {
            _audit.LogServiceBusOutcome("Failed", principal.CallerIdentity, payload.Workflow ?? string.Empty, outcome.Error ?? "Workflow execution failed.", correlationId);
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
    /// WOLF-8512: optional dead-letter triage sub-reason (see the class doc's "Failure
    /// classification" section / <c>docs/ServiceBusSecureTrigger-Architecture.md</c>).
    /// When supplied, <c>deadLetterReason</c> becomes <c>"{status}:{subReason}"</c> (e.g.
    /// <c>InvalidToken:JtiReplay</c>) instead of the bare status, so DLQ entries sharing a
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
    /// WOLF-8512: the shared "transient — retry via Service Bus redelivery" tail (§2.1's
    /// "Retry" row): release the claim taken by <see cref="IServiceBusReplayAndResultStore.TryClaim"/>
    /// (so a redelivery is not permanently blocked by this failed attempt's own claim — see
    /// <c>TryClaim</c>'s doc comment), log a warning, then rethrow <paramref name="ex"/> so the
    /// Service Bus extension applies its standard retry/backoff and eventual
    /// <c>maxDeliveryCount</c> dead-letter.
    ///
    /// <para>
    /// Deliberately does NOT call <c>SaveResult</c> and does NOT settle the message — the two
    /// non-negotiable invariants for a transient outcome (a persisted result would satisfy the
    /// idempotency dedupe check on redelivery and permanently prevent re-execution; see this
    /// class's own flow doc and <c>RecordTerminalOutcomeAsync</c>'s terminal-path counterpart).
    /// </para>
    ///
    /// <para>
    /// Rethrows via <see cref="ExceptionDispatchInfo"/> rather than <c>throw ex;</c> so the
    /// original stack trace is preserved even when called from a location other than the
    /// original <c>catch</c> block (e.g. the <c>!_tokenValidator.IsEnabled</c> branch, which
    /// constructs a fresh exception rather than re-throwing a caught one).
    /// </para>
    /// </summary>
    [DoesNotReturn]
    internal void FailTransient(string correlationId, Exception ex, string logMessageTemplate, params object?[] logArgs)
    {
        _store.ReleaseClaim(correlationId);
        _logger.LogWarning(logMessageTemplate, logArgs);
        ExceptionDispatchInfo.Capture(ex).Throw();
    }

    /// <summary>
    /// WOLF-8512: distinguishes "the trigger's own invocation was cancelled while
    /// <c>EntraBearerTokenValidator.ValidateAsync</c> was awaiting something" (transient — should be
    /// retried) from every other validation failure (genuinely bad token — should not be retried).
    /// Pure classification, no I/O, so it is directly unit-testable without needing a live OIDC call
    /// or a fake for the sealed <see cref="EntraBearerTokenValidator"/> — same reason
    /// <c>WorkflowExecutor.BuildTransientFailureResult</c> was extracted as its own testable helper
    /// for the analogous <see cref="OutOfMemoryException"/> fix.
    ///
    /// <para>
    /// Checking <paramref name="cancellationToken"/>.<see
    /// cref="CancellationToken.IsCancellationRequested"/> (rather than only the exception's own type)
    /// guards against misclassifying an <see cref="OperationCanceledException"/> thrown for an
    /// unrelated reason as this invocation's own cancellation.
    /// </para>
    /// </summary>
    internal static bool IsOwnInvocationCancellation(Exception ex, CancellationToken cancellationToken) =>
        ex is OperationCanceledException && cancellationToken.IsCancellationRequested;

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

    /// <summary>
    /// Resolves the token's <c>jti</c> for replay-cache lookup: prefers the cheaper mirrored
    /// <c>jti</c> message application property, falling back to the claim inside the
    /// validated token when the property was not supplied.
    /// </summary>
    private static string? ResolveJti(ServiceBusReceivedMessage message, ClaimsIdentity identity)
    {
        if (message.ApplicationProperties.TryGetValue("jti", out var jtiObj)
            && jtiObj is string jtiProperty
            && !string.IsNullOrWhiteSpace(jtiProperty))
        {
            return jtiProperty;
        }

        return identity.FindFirst("jti")?.Value;
    }
}
