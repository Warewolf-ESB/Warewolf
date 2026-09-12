/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System.Text.Json;

namespace Warewolf.Execution.ServiceBusWorker.Configuration;

/// <summary>
/// Which Service Bus queue this worker's <c>WorkflowQueueTrigger</c> listens on, and which
/// workflow every message on that queue executes.
///
/// <para>
/// Read once from <c>Settings/servicebustrigger.bite</c> at cold start, BEFORE the host is
/// built — <see cref="QueueName"/> is injected into
/// <see cref="Microsoft.Extensions.Configuration.IConfiguration"/> under the
/// <c>WAREWOLF_SERVICEBUS_TRIGGER_QUEUE</c> key so the existing
/// <c>[ServiceBusTrigger("%WAREWOLF_SERVICEBUS_TRIGGER_QUEUE%")]</c> attribute indirection
/// resolves unchanged (see <c>Program.cs</c>). <see cref="WorkflowName"/> is registered as a
/// DI singleton and consumed directly by <c>WorkflowQueueTrigger</c> — the message body no
/// longer names the workflow.
/// </para>
///
/// <para>
/// Deliberately a standalone copy, not shared with
/// <c>Warewolf.Execution.Lightweight.Auth.Models.ServiceBusTriggerDefinition</c>: the two
/// projects are separate deployables (isolated-worker <c>Exe</c> projects cannot reference
/// each other) and this one has no other <c>Dev2.*</c>/<c>Warewolf.*</c> project reference —
/// mirrors the same worker-local-DTO precedent
/// <c>Warewolf.Execution.QueueProcessor.Configuration.TriggerDefinition</c> already
/// established for the RabbitMQ trigger path.
/// </para>
///
/// <para>
/// Contains no secret — Service Bus authentication is Managed-Identity-only — so this file
/// is plain JSON and is never WFAES-encrypted.
/// </para>
/// </summary>
public sealed class ServiceBusTriggerDefinition
{
    public const string FileName = "servicebustrigger.bite";

    const string DisabledAppSetting = "AzureWebJobs.WorkflowQueueTrigger.Disabled";

    public Guid TriggerId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string QueueName { get; init; } = string.Empty;
    public string WorkflowName { get; init; } = string.Empty;
    public Guid ResourceId { get; init; }

    /// <summary>
    /// Loads and validates <c>Settings/servicebustrigger.bite</c>. Returns <c>null</c> without
    /// touching disk when the trigger is disabled entirely via
    /// <c>AzureWebJobs.WorkflowQueueTrigger.Disabled</c> — the standard Azure Functions
    /// zero-code-change opt-out (the Functions attribute name here is
    /// <c>WorkflowQueueTrigger</c>, not <c>ServiceBusWorkflowTrigger</c> — see
    /// <c>Functions.WorkflowQueueTrigger</c>'s <c>[Function(nameof(WorkflowQueueTrigger))]</c>).
    /// Otherwise a missing file, malformed JSON, or a missing
    /// <see cref="QueueName"/>/<see cref="WorkflowName"/> is fatal.
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
