/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

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
///         bound to the DEDICATED <see cref="ServiceBusEntraAuthOptions"/> audience (never the general HTTP audience — confused-deputy prevention).</item>
///   <item>Replay protection: register the token's <c>jti</c> (mirrored application property preferred, falls back to the token claim) — a repeat is dead-lettered, never retried.</item>
///   <item>Authorize via <see cref="IWorkflowPolicyMatcher.Evaluate"/> — denial is dead-lettered, never retried.</item>
///   <item>Execute in-process via <see cref="IWorkflowExecutor"/>. A business/activity failure is dead-lettered (not transient — retrying will not help).
///         An unexpected exception is rethrown so the Service Bus extension applies its normal retry/backoff and eventual max-delivery-count dead-letter.</item>
///   <item>Persist the terminal outcome (success, failure, or denial) via <see cref="IServiceBusReplayAndResultStore"/> for
///         <see cref="ServiceBusResultFunction"/>'s polling endpoint, and emit a structured audit event via <see cref="AuditLogger.LogServiceBusOutcome"/>.</item>
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
    private readonly ILogger<ServiceBusWorkflowTriggerFunction> _logger;

    public ServiceBusWorkflowTriggerFunction(
        ServiceBusEntraAuthOptions serviceBusAuthOptions,
        IWorkflowPolicyMatcher policyMatcher,
        IWorkflowExecutor executor,
        IServiceBusReplayAndResultStore store,
        AuditLogger audit,
        HostEnvironmentConfig config,
        ILogger<ServiceBusWorkflowTriggerFunction> logger)
    {
        _tokenValidator = new EntraBearerTokenValidator(serviceBusAuthOptions);
        _policyMatcher  = policyMatcher;
        _executor       = executor;
        _store          = store;
        _audit          = audit;
        _config         = config;
        _logger         = logger;
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
                receivedAt, cancellationToken);
            return;
        }

        if (payload is null || string.IsNullOrWhiteSpace(payload.Workflow))
        {
            await RecordTerminalOutcomeAsync(
                messageActions, message, correlationId, workflow: payload?.Workflow ?? string.Empty, caller: "(unknown)",
                status: ServiceBusTriggerStatus.Malformed, reason: "Message is missing the required 'workflow' field.",
                receivedAt, cancellationToken);
            return;
        }

        if (!string.IsNullOrWhiteSpace(payload.CorrelationId))
        {
            correlationId = payload.CorrelationId;
        }

        // ── Business-idempotency dedupe ────────────────────────────────────────
        if (_store.TryGetResult(correlationId, out var existing) && existing != null)
        {
            _logger.LogInformation(
                "ServiceBusWorkflowTrigger | CorrelationId={CorrelationId} | Duplicate message — result already recorded (Status={Status}); completing without re-execution.",
                correlationId, existing.Status);
            await messageActions.CompleteMessageAsync(message, cancellationToken).ConfigureAwait(false);
            return;
        }

        // ── Token extraction ────────────────────────────────────────────────────
        if (!message.ApplicationProperties.TryGetValue("Authorization", out var authObj)
            || authObj is not string authHeader
            || string.IsNullOrWhiteSpace(authHeader))
        {
            await RecordTerminalOutcomeAsync(
                messageActions, message, correlationId, payload.Workflow, caller: "(unknown)",
                status: ServiceBusTriggerStatus.InvalidToken, reason: "Missing 'Authorization' message application property.",
                receivedAt, cancellationToken);
            return;
        }

        if (!_tokenValidator.IsEnabled)
        {
            await RecordTerminalOutcomeAsync(
                messageActions, message, correlationId, payload.Workflow, caller: "(unknown)",
                status: ServiceBusTriggerStatus.InvalidToken,
                reason: "Service Bus token validation is not configured on this engine (missing WAREWOLF_ENTRA_TENANT_ID / WAREWOLF_ENTRA_SERVICEBUS_AUDIENCE).",
                receivedAt, cancellationToken);
            return;
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
        catch (Exception ex)
        {
            await RecordTerminalOutcomeAsync(
                messageActions, message, correlationId, payload.Workflow, caller: "(unknown)",
                status: ServiceBusTriggerStatus.InvalidToken, reason: $"Token validation failed: {ex.Message}",
                receivedAt, cancellationToken);
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
                receivedAt, cancellationToken);
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
                receivedAt, cancellationToken);
            return;
        }

        // ── Execute in-process (no HTTP hop) ────────────────────────────────────
        var executionRequest = WorkflowFunctionHelper.CreateRequestByName(
            payload.Workflow!, _config.WorkflowsDirectory, payload.Inputs ?? new Dictionary<string, string>());
        executionRequest.ExecutingPrincipal = principal;

        WorkflowExecutionResult result;
        try
        {
            result = _executor.Execute(executionRequest);
        }
        catch (Exception ex)
        {
            // Unexpected — NOT a policy/token/malformed-message failure. Let it bubble so
            // the Service Bus extension applies its standard retry/backoff and eventual
            // max-delivery-count dead-letter, per the spec's "no retry on non-transient
            // failures ONLY" requirement.
            _logger.LogError(
                ex,
                "ServiceBusWorkflowTrigger | CorrelationId={CorrelationId} | Workflow={Workflow} | Unexpected execution exception — leaving message for standard Service Bus retry.",
                correlationId, payload.Workflow);
            throw;
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
            await messageActions.CompleteMessageAsync(message, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            _audit.LogServiceBusOutcome("Failed", principal.CallerIdentity, payload.Workflow ?? string.Empty, outcome.Error ?? "Workflow execution failed.", correlationId);
            // A workflow/activity failure is a business outcome, not a transient transport
            // failure — dead-letter rather than let Service Bus retry an execution that will
            // fail identically every time.
            await messageActions.DeadLetterMessageAsync(
                message,
                deadLetterReason: "execution_failed",
                deadLetterErrorDescription: outcome.Error ?? "Workflow execution failed.",
                cancellationToken: cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Persists a non-success terminal outcome, emits the structured audit event, and
    /// dead-letters the message (no retry) — the shared tail for every deny/invalid/malformed
    /// path above.
    /// </summary>
    private async Task RecordTerminalOutcomeAsync(
        ServiceBusMessageActions messageActions,
        ServiceBusReceivedMessage message,
        string correlationId,
        string workflow,
        string caller,
        ServiceBusTriggerStatus status,
        string reason,
        DateTimeOffset receivedAt,
        CancellationToken cancellationToken)
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

        await messageActions.DeadLetterMessageAsync(
            message,
            deadLetterReason: status.ToString(),
            deadLetterErrorDescription: reason,
            cancellationToken: cancellationToken).ConfigureAwait(false);
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
