namespace Warewolf.Execution.ServiceBusWorker.E2EHarness;

/// <summary>
/// Placeholder expansion for <c>--workflow-inputs-json</c>.
/// <para>
/// A load run publishes N messages that differ only by correlationId; without expansion every
/// message carries a byte-identical <c>inputs</c> map. That is not merely a weaker test — it
/// actively breaks workflows that key off the message body. <c>RabbitProcess.bite</c> calls
/// <c>dbo.usp_jobs1_LogStart</c>, which hashes the message content and takes an EXCLUSIVE
/// <c>sp_getapplock</c> on that hash for the life of its transaction (15s timeout). Identical
/// bodies therefore hash identically, so all N executions serialise behind a single lock and
/// are recorded as N retry *attempts of one job* rather than N distinct jobs.
/// </para>
/// <para>
/// Expanding <c>{correlationId}</c> into each message's own inputs gives every message a
/// distinct body, hence a distinct hash, hence genuine concurrency.
/// </para>
/// </summary>
internal static class WorkflowInputTemplate
{
    /// <summary>The only supported placeholder. Case-sensitive, matching the JSON field name.</summary>
    internal const string CorrelationIdPlaceholder = "{correlationId}";

    /// <summary>
    /// Replaces every <see cref="CorrelationIdPlaceholder"/> occurrence in <paramref name="workflowInputsJson"/>
    /// with <paramref name="correlationId"/>. Returns the input unchanged when it is null, empty,
    /// whitespace, or contains no placeholder — so callers that pass a literal inputs map keep
    /// their existing behaviour exactly.
    /// </summary>
    /// <remarks>
    /// Substitution happens on the raw JSON text *before* deserialisation, so a placeholder may
    /// appear in a value, inside a larger string, or multiple times. correlationIds are generated
    /// by the harness as GUID-derived hex plus an index suffix, so they contain no JSON
    /// metacharacters and cannot break the surrounding document.
    /// </remarks>
    internal static string Expand(string workflowInputsJson, string correlationId)
    {
        if (string.IsNullOrWhiteSpace(workflowInputsJson))
        {
            return workflowInputsJson;
        }
        return workflowInputsJson.Replace(CorrelationIdPlaceholder, correlationId ?? string.Empty);
    }

    /// <summary>
    /// True when <paramref name="workflowInputsJson"/> contains the correlationId placeholder —
    /// i.e. each published message will carry a distinct inputs map.
    /// </summary>
    internal static bool HasCorrelationIdPlaceholder(string workflowInputsJson) =>
        !string.IsNullOrWhiteSpace(workflowInputsJson)
        && workflowInputsJson.Contains(CorrelationIdPlaceholder, StringComparison.Ordinal);
}
