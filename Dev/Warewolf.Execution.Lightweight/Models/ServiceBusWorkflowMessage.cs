/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System.Text.Json.Serialization;

namespace Warewolf.Execution.Lightweight.Models;

/// <summary>
/// Deserialized body of a message on the secure Service Bus workflow-trigger queue
/// (Model A — <c>Spec-Secure-ServiceBus-Triggered-Execution.md</c>).
///
/// <para>
/// The caller's Entra token is carried as a Service Bus message
/// <b>application property</b> named <c>Authorization</c> (e.g.
/// <c>Bearer &lt;jwt&gt;</c>), NOT in this JSON body — the body only describes what to
/// execute; the message's transport-level properties carry who is asking. An optional
/// <c>jti</c> application property may mirror the token's own <c>jti</c> claim for a
/// cheaper replay-cache lookup (falls back to the claim inside the token when absent).
/// </para>
///
/// <para>
/// Uses a queue distinct from the existing <c>Warewolf.Execution.ServiceBusWorker</c>
/// (Model B) worker's <c>wwexecution-queue</c> — the two message contracts are not
/// interchangeable (this one requires a per-caller token; Model B's does not).
/// </para>
/// </summary>
public sealed class ServiceBusWorkflowMessage
{
    /// <summary>Workflow name to execute (resolved the same way as the HTTP routes).</summary>
    [JsonPropertyName("workflow")]
    public string? Workflow { get; init; }

    /// <summary>
    /// Input parameters for the workflow, keyed by DataList variable name (no
    /// <c>[[ ]]</c> notation) — mirrors the HTTP routes' JSON body / query-string inputs.
    /// </summary>
    [JsonPropertyName("inputs")]
    public Dictionary<string, string>? Inputs { get; init; }

    /// <summary>
    /// Caller-supplied correlation id used for idempotency dedupe and for the
    /// <c>GET /secure/servicebus-result/{correlationId}</c> polling endpoint. When
    /// omitted, the message's own <c>MessageId</c> is used instead (still stable across
    /// broker redelivery of the same physical message, but NOT across a caller
    /// re-publishing a logically-duplicate message with a new <c>MessageId</c> — callers
    /// that need dedupe across re-publish should always supply their own value).
    /// </summary>
    [JsonPropertyName("correlationId")]
    public string? CorrelationId { get; init; }
}
