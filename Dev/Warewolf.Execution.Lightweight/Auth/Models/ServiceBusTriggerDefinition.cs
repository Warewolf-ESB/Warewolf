/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System.Text.Json;

namespace Warewolf.Execution.Lightweight.Auth.Models;

/// <summary>
/// Which Service Bus queue the secure in-process trigger
/// (<c>Functions.ServiceBusWorkflowTriggerFunction</c>) listens on, and which workflow every
/// message on that queue executes.
///
/// <para>
/// Read once from <c>Settings/servicebustrigger.bite</c> at cold start, BEFORE the host is
/// built — <see cref="QueueName"/> is injected into
/// <see cref="Microsoft.Extensions.Configuration.IConfiguration"/> under the
/// <c>WAREWOLF_SERVICEBUS_TRIGGER_QUEUE</c> key so the existing
/// <c>[ServiceBusTrigger("%WAREWOLF_SERVICEBUS_TRIGGER_QUEUE%")]</c> attribute indirection
/// resolves unchanged (see <c>Infrastructure.HostBuilderExtensions.ConfigureWarewolf</c>).
/// <see cref="WorkflowName"/> is registered as a DI singleton and consumed directly by
/// <c>ServiceBusWorkflowTriggerFunction</c> — the message body no longer names the workflow
/// (see <c>Models.ServiceBusWorkflowMessage</c>).
/// </para>
///
/// <para>
/// Contains no secret — Service Bus authentication is Managed-Identity-only — so, unlike
/// <c>Settings/persistencesettingsdbsource.bite</c>, this file is plain JSON and is never
/// WFAES-encrypted.
/// </para>
/// </summary>
public sealed class ServiceBusTriggerDefinition
{
    public const string FileName = "servicebustrigger.bite";

    const string DisabledAppSetting = "AzureWebJobs.ServiceBusWorkflowTrigger.Disabled";

    public Guid TriggerId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string QueueName { get; init; } = string.Empty;
    public string WorkflowName { get; init; } = string.Empty;
    public Guid ResourceId { get; init; }

    /// <summary>
    /// Optional. The workflow input that receives the WHOLE message body as a string — the
    /// in-process equivalent of a queue trigger's <c>MapEntireMessage = true</c>, which on-prem
    /// binds the raw message to <c>Inputs[0]</c> only (<c>MessageToInputsMapper.cs:31-33</c>).
    /// Blank or absent keeps the default binding: the <c>{"inputs":{…}}</c> envelope, else the
    /// body verbatim (see <c>ServiceBusWorkflowTriggerFunction.ResolveRawInputPayload</c>).
    ///
    /// <para>
    /// Without it, a workflow that declares a single whole-document input (the profiler's
    /// <c>RequestForProfilingConsume</c>, input <c>ProfileRequest</c>) receives the body's
    /// top-level fields instead, and fails on its first step with
    /// "Scalar value { ProfileRequest } is NULL".
    /// </para>
    /// </summary>
    public string? MapEntireMessageTo
    {
        get => _mapEntireMessageTo;
        init => _mapEntireMessageTo = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    readonly string? _mapEntireMessageTo;

    /// <summary>
    /// Loads and validates <c>Settings/servicebustrigger.bite</c>. Returns <c>null</c> without
    /// touching disk when the trigger is disabled entirely via
    /// <c>AzureWebJobs.ServiceBusWorkflowTrigger.Disabled</c> (the existing, unchanged
    /// zero-code-change opt-out). Otherwise a missing file, malformed JSON, or a missing
    /// <see cref="QueueName"/>/<see cref="WorkflowName"/> is fatal — refusing to start an
    /// enabled trigger that cannot be configured is safer than silently defaulting to a queue
    /// nobody chose or a workflow nobody named.
    /// </summary>
    /// <exception cref="ServiceBusTriggerConfigurationException">
    /// The trigger is enabled but the file is missing, malformed, or incomplete.
    /// </exception>
    public static ServiceBusTriggerDefinition? Load(string? settingsDirectory = null)
    {
        if (IsDisabled())
        {
            return null;
        }

        settingsDirectory ??= Path.Combine(AppContext.BaseDirectory, "Settings");
        var path = Path.Combine(settingsDirectory, FileName);

        if (!File.Exists(path))
        {
            throw new ServiceBusTriggerConfigurationException(
                $"Service Bus trigger config '{path}' was not found. The trigger is enabled " +
                $"(no '{DisabledAppSetting}=true' app setting) but has no queue/workflow to run — " +
                "either supply the file or disable the trigger explicitly.");
        }

        ServiceBusTriggerDefinition? definition;
        try
        {
            var json = File.ReadAllText(path);
            var options = new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
                PropertyNameCaseInsensitive = true,
            };
            definition = JsonSerializer.Deserialize<ServiceBusTriggerDefinition>(json, options);
        }
        catch (JsonException ex)
        {
            throw new ServiceBusTriggerConfigurationException(
                $"Service Bus trigger config '{path}' could not be parsed: {ex.Message}", ex);
        }

        if (definition is null)
        {
            throw new ServiceBusTriggerConfigurationException(
                $"Service Bus trigger config '{path}' deserialized to null.");
        }

        var missing = new List<string>();
        if (string.IsNullOrWhiteSpace(definition.QueueName)) missing.Add(nameof(QueueName));
        if (string.IsNullOrWhiteSpace(definition.WorkflowName)) missing.Add(nameof(WorkflowName));
        if (missing.Count > 0)
        {
            throw new ServiceBusTriggerConfigurationException(
                $"Service Bus trigger config '{path}' is missing required value(s): {string.Join(", ", missing)}.");
        }

        if (definition.MapEntireMessageTo is { } mapTo && mapTo.StartsWith('@'))
        {
            // On-prem posts an '@'-prefixed whole-message input as multipart/form-data
            // (WarewolfWebRequestForwarder.cs:100-108); this trigger executes in-process and has
            // no such binding, so refuse rather than guess at a JSON object input.
            throw new ServiceBusTriggerConfigurationException(
                $"Service Bus trigger config '{path}' has {nameof(MapEntireMessageTo)}='{mapTo}'. An '@'-prefixed " +
                "(JSON object) input is not supported by the in-process trigger - name a scalar input instead.");
        }

        return definition;
    }

    static bool IsDisabled()
    {
        var value = Environment.GetEnvironmentVariable(DisabledAppSetting);
        return string.Equals(value, "true", StringComparison.OrdinalIgnoreCase) || value == "1";
    }
}

/// <summary>Thrown by <see cref="ServiceBusTriggerDefinition.Load"/> on a fatal configuration problem.</summary>
public sealed class ServiceBusTriggerConfigurationException : Exception
{
    public ServiceBusTriggerConfigurationException(string message) : base(message) { }
    public ServiceBusTriggerConfigurationException(string message, Exception inner) : base(message, inner) { }
}
