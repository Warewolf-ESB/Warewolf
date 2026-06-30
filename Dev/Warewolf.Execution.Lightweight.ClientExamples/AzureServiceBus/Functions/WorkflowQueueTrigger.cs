using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Warewolf.Execution.ClientExamples.AzureServiceBus.Functions;

/// <summary>
/// Triggered by a message on the <c>wwexecution-queue</c> Service Bus queue. Each message names
/// a Warewolf workflow plus its inputs; this worker authenticates to Entra (Managed Identity) and
/// calls the engine's <c>/secure</c> route on the message's behalf.
///
/// Why an SB-triggered worker (and not "Service Bus calls the engine")? Service Bus is a message
/// broker — it cannot hold an OAuth token or make an outbound HTTP call. The realistic pattern is
/// a compute trigger (this Function) that reads the message, acquires a token and calls the engine.
/// </summary>
public sealed class WorkflowQueueTrigger
{
    private readonly IWwExecutionClient _client;
    private readonly ILogger<WorkflowQueueTrigger> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
    };

    public WorkflowQueueTrigger(IWwExecutionClient client, ILogger<WorkflowQueueTrigger> logger)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Message contract: <c>{ "workflow": "Hello World", "inputs": { "Name": "FromServiceBus" } }</c>
    /// </summary>
    [Function(nameof(WorkflowQueueTrigger))]
    public async Task RunAsync(
        [ServiceBusTrigger("wwexecution-queue", Connection = "ServiceBusConnection")]
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

        if (request is null || string.IsNullOrWhiteSpace(request.Workflow))
        {
            throw new InvalidOperationException(
                $"Message is missing the required 'workflow' field. Body: {messageBody}");
        }

        var query = request.Inputs ?? new Dictionary<string, string?>();

        _logger.LogInformation(
            "Executing workflow '{Workflow}' with {InputCount} input(s) from Service Bus message.",
            request.Workflow, query.Count);

        var result = await _client
            .ExecuteSecureAsync(request.Workflow, query, cancellationToken)
            .ConfigureAwait(false);

        _logger.LogInformation(
            "Workflow '{Workflow}' completed. Response: {Response}", request.Workflow, result);
    }

    /// <summary>Deserialized Service Bus message payload.</summary>
    private sealed class WorkflowExecutionRequest
    {
        [JsonPropertyName("workflow")]
        public string? Workflow { get; init; }

        [JsonPropertyName("inputs")]
        public Dictionary<string, string?>? Inputs { get; init; }
    }
}
