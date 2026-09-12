using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;
using Warewolf.Execution.ServiceBusWorker.Configuration;

namespace Warewolf.Execution.ServiceBusWorker.Functions;

/// <summary>
/// Triggered by a message on the Service Bus queue named by
/// <c>Settings/servicebustrigger.bite</c>'s <c>QueueName</c> (see
/// <see cref="ServiceBusTriggerDefinition"/> — default <c>wwexecution-queue</c>, matching
/// <c>Deploy-WwExecutionServiceBusWorker.ps1</c>'s <c>-ServiceBusQueueName</c> param in Azure).
/// Every message on that queue runs the SAME workflow, named by that file's
/// <c>WorkflowName</c> — the message itself only carries an optional target route and its
/// inputs; this worker authenticates to Entra (Managed Identity) and calls the engine's
/// <c>/secure</c> or <c>/public</c> route on the message's behalf (mirroring the AzureFunction
/// sample's <c>run</c> / <c>runpublic</c> proxies).
///
/// Why an SB-triggered worker (and not "Service Bus calls the engine")? Service Bus is a message
/// broker — it cannot hold an OAuth token or make an outbound HTTP call. The realistic pattern is
/// a compute trigger (this Function) that reads the message, acquires a token and calls the engine.
/// </summary>
public sealed class WorkflowQueueTrigger
{
    private readonly IWwExecutionClient _client;
    private readonly ServiceBusTriggerDefinition _triggerDefinition;
    private readonly ILogger<WorkflowQueueTrigger> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
    };

    public WorkflowQueueTrigger(
        IWwExecutionClient client,
        ServiceBusTriggerDefinition triggerDefinition,
        ILogger<WorkflowQueueTrigger> logger)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _triggerDefinition = triggerDefinition ?? throw new ArgumentNullException(nameof(triggerDefinition));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Message contract:
    /// <c>{ "route": "secure", "inputs": { "Name": "FromServiceBus" } }</c>
    /// <para><c>route</c> is optional: omitted/blank defaults to <c>secure</c>; <c>public</c> targets
    /// the engine's anonymous route. Any other value dead-letters the message. There is no
    /// <c>workflow</c> field any more — every message on this queue runs
    /// <see cref="ServiceBusTriggerDefinition.WorkflowName"/>.</para>
    /// </summary>
    [Function(nameof(WorkflowQueueTrigger))]
    public async Task RunAsync(
        [ServiceBusTrigger("%WAREWOLF_SERVICEBUS_TRIGGER_QUEUE%", Connection = "ServiceBusConnection")]
        string messageBody,
        FunctionContext context,
        CancellationToken cancellationToken)
    {
        // NOTE: exceptions are intentionally NOT swallowed. Letting them bubble up means the
        // Functions runtime abandons the message; after the configured max delivery count the
        // broker dead-letters it — exactly what we want for a poison message.

        WorkflowExecutionRequest? request;
        try
        {
            request = JsonSerializer.Deserialize<WorkflowExecutionRequest>(messageBody, JsonOptions);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Message body is not valid JSON; dead-lettering. Body: {Body}", messageBody);
            throw;
        }

        if (request is null)
        {
            throw new InvalidOperationException(
                $"Message body deserialized to null. Body: {messageBody}");
        }

        var workflow = _triggerDefinition.WorkflowName;
        var route = ResolveRoute(request.Route);
        var query = request.Inputs ?? new Dictionary<string, string?>();

        _logger.LogInformation(
            "Executing workflow '{Workflow}' on the '{Route}' route with {InputCount} input(s) from Service Bus message.",
            workflow, route, query.Count);

        var result = route == "public"
            ? await _client.ExecutePublicAsync(workflow, query, cancellationToken).ConfigureAwait(false)
            : await _client.ExecuteSecureAsync(workflow, query, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "Workflow '{Workflow}' completed on '{Route}'. Response: {Response}",
            workflow, route, result);
    }

    /// <summary>
    /// Normalizes the optional message <c>route</c> to a supported engine route. A blank/absent value
    /// defaults to <c>secure</c> (back-compatible); <c>secure</c> and <c>public</c> are accepted
    /// case-insensitively. Any other value throws so the runtime dead-letters the poison message.
    /// </summary>
    private static string ResolveRoute(string? route)
    {
        if (string.IsNullOrWhiteSpace(route))
        {
            return "secure";
        }

        return route.Trim().ToLowerInvariant() switch
        {
            "secure" => "secure",
            "public" => "public",
            _ => throw new InvalidOperationException(
                $"Unsupported route '{route}'. Supported values: 'secure' (default) or 'public'.")
        };
    }

    /// <summary>Deserialized Service Bus message payload.</summary>
    private sealed class WorkflowExecutionRequest
    {
        [JsonPropertyName("route")]
        public string? Route { get; init; }

        [JsonPropertyName("inputs")]
        public Dictionary<string, string?>? Inputs { get; init; }
    }
}
