/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

namespace Warewolf.Execution.Lightweight.Models;

/// <summary>Terminal status of a secure Service Bus-triggered workflow execution.</summary>
public enum ServiceBusTriggerStatus
{
    /// <summary>The workflow executed successfully.</summary>
    Succeeded,

    /// <summary>The workflow executed but returned failure (activity error).</summary>
    Failed,

    /// <summary>The message's token failed authorization (policy denial) — dead-lettered.</summary>
    Denied,

    /// <summary>The message's token was missing, expired, or otherwise failed validation — dead-lettered.</summary>
    InvalidToken,

    /// <summary>The message body/properties were malformed — dead-lettered.</summary>
    Malformed,
}

/// <summary>
/// Persisted outcome of one secure Service Bus-triggered workflow execution, keyed by
/// <see cref="CorrelationId"/>. Written by
/// <see cref="Functions.ServiceBusWorkflowTriggerFunction"/> and read back by:
/// <list type="bullet">
///   <item>the same function, before re-executing, for business-idempotency dedupe;</item>
///   <item><see cref="Functions.ServiceBusResultFunction"/>, the
///         <c>GET /secure/servicebus-result/{correlationId}</c> polling endpoint.</item>
/// </list>
/// </summary>
public sealed class ServiceBusTriggerResult
{
    /// <summary>Correlation id this result is keyed by.</summary>
    public string CorrelationId { get; set; } = string.Empty;

    /// <summary>Terminal status of the execution attempt.</summary>
    public ServiceBusTriggerStatus Status { get; set; }

    /// <summary>Workflow name that was (or would have been) executed.</summary>
    public string? Workflow { get; set; }

    /// <summary>Caller identity resolved from the validated token (never the raw token itself).</summary>
    public string? Caller { get; set; }

    /// <summary>JSON-shaped outputs on success; <c>null</c> otherwise.</summary>
    public string? Outputs { get; set; }

    /// <summary>Error / denial-reason message when <see cref="Status"/> is not <see cref="ServiceBusTriggerStatus.Succeeded"/>.</summary>
    public string? Error { get; set; }

    /// <summary>UTC time the message was received by the trigger.</summary>
    public DateTimeOffset ReceivedAtUtc { get; set; }

    /// <summary>UTC time the outcome was recorded.</summary>
    public DateTimeOffset CompletedAtUtc { get; set; }
}
