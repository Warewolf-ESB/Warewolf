using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Azure.Messaging.ServiceBus;

namespace Warewolf.Execution.ServiceBusWorker.E2EHarness;

/// <summary>
/// Two harness modes, selected via <c>--mode</c> (default <c>arrival</c>):
///
/// <list type="bullet">
///   <item><b><c>arrival</c></b> (original/default — unchanged behaviour). Publishes a
///   uniquely-marked test message to a RabbitMQ source queue (via the RabbitMQ Management
///   HTTP API's "publish" endpoint — no AMQP 0.9.1 client dependency needed for that side)
///   and then polls a Service Bus queue (via the official SDK — real AMQP 1.0) for the same
///   marker to arrive, proving a RabbitMQ Shovel actually bridged the message across
///   protocols. Does NOT prove a workflow was executed — see <c>workflow-execution</c>.</item>
///   <item><b><c>workflow-execution</c></b> (additive). Publishes the real
///   <c>ServiceBusWorkflowMessage</c> JSON contract (<c>{"workflow","inputs","correlationId"}</c>)
///   to RabbitMQ, carrying the caller's bearer token as an AMQP 0.9.1 message header
///   (<c>Authorization</c>, optionally <c>jti</c>) — the Shovel bridges these to AMQP 1.0
///   application properties, which is exactly where
///   <c>Warewolf.Execution.Lightweight.Functions.ServiceBusWorkflowTriggerFunction</c>
///   (the in-process "Model A" secure Service Bus trigger, see
///   docs/ServiceBusSecureTrigger-Architecture.md) reads them from. It then polls that same
///   Lightweight engine's own <c>GET /secure/servicebus-result/{correlationId}</c> endpoint
///   for a terminal status — this is the only way to observe the outcome, since the trigger
///   is in-process and there is no separate worker to watch. A Service Bus SDK receiver is
///   deliberately NOT used to "check arrival" in this mode: it would compete with the
///   engine's own Managed-Identity subscription on the SAME queue and could steal the
///   message before the engine processes it.</item>
/// </list>
///
/// Exit code 0 + "PASS: ..." on success, exit code 1 + "FAIL: ..." otherwise.
/// Intended to be invoked by Scripts/Tests/Integration/Test-ShovelBridgeE2E.ps1,
/// not run standalone in normal development.
/// </summary>
internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
        Dictionary<string, string> opts;
        try
        {
            opts = ParseArgs(args);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"FAIL: {ex.Message}");
            PrintUsage();
            return 1;
        }

        string Required(string name)
        {
            if (!opts.TryGetValue(name, out var value) || string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException($"Missing required argument --{name}");
            }
            return value;
        }

        var mode = opts.GetValueOrDefault("mode", "arrival");

        try
        {
            return mode.ToLowerInvariant() switch
            {
                "arrival" => await RunArrivalModeAsync(opts, Required),
                "workflow-execution" => await RunWorkflowExecutionModeAsync(opts, Required),
                _ => throw new ArgumentException($"Unsupported --mode '{mode}'. Supported: 'arrival' (default), 'workflow-execution'."),
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"FAIL: {ex.GetType().Name}: {ex.Message}");
            return 1;
        }
    }

    /// <summary>
    /// Original bridge-delivery proof: publish one or more uniquely-marked messages and poll
    /// Service Bus until all of them arrive (or the timeout elapses). <c>--message-count</c>
    /// (default 1, unchanged single-message behaviour) lets a caller drive a bulk/load run —
    /// e.g. the ShovelBridge load test job publishes 1000 messages here to prove the shovel
    /// bridge itself can sustain that volume, without also exercising workflow execution (see
    /// <c>workflow-execution</c> mode for that separate, heavier concern).
    /// </summary>
    private static async Task<int> RunArrivalModeAsync(Dictionary<string, string> opts, Func<string, string> required)
    {
        var rabbitMqManagementUri = required("rabbitmq-management-uri").TrimEnd('/');
        var rabbitMqUsername = required("rabbitmq-username");
        var rabbitMqPassword = required("rabbitmq-password");
        var rabbitMqVHost = opts.GetValueOrDefault("rabbitmq-vhost", "/");
        var sourceQueue = required("source-queue");
        var serviceBusConnectionString = required("servicebus-connection-string");
        var destinationQueue = required("destination-queue");
        var timeoutSeconds = int.Parse(opts.GetValueOrDefault("timeout-seconds", "60"));
        var messageCount = int.Parse(opts.GetValueOrDefault("message-count", "1"));
        if (messageCount < 1)
        {
            throw new ArgumentException("--message-count must be at least 1.");
        }
        var markerPrefix = opts.GetValueOrDefault("marker", Guid.NewGuid().ToString("N"));

        // Fixed-width numeric suffixes (D6) guarantee no marker is ever a substring of another
        // marker sharing the same prefix, so exact-match lookup by value (see
        // WaitForMessagesOnServiceBusAsync) is unambiguous even at 1000+ messages.
        var markers = messageCount == 1
            ? new[] { markerPrefix }
            : Enumerable.Range(0, messageCount).Select(i => $"{markerPrefix}-{i:D6}").ToArray();

        Console.WriteLine(messageCount == 1
            ? $"Marker: {markers[0]}"
            : $"Marker prefix: {markerPrefix} ({messageCount} messages, '{markers[0]}' .. '{markers[^1]}')");
        Console.WriteLine($"Publishing {messageCount} message(s) to RabbitMQ queue '{sourceQueue}' (vhost '{rabbitMqVHost}') ...");

        var publishStopwatch = Stopwatch.StartNew();
        await PublishManyToRabbitMqAsync(rabbitMqManagementUri, rabbitMqUsername, rabbitMqPassword, rabbitMqVHost, sourceQueue, markers);
        publishStopwatch.Stop();
        Console.WriteLine($"Published {messageCount} message(s) in {publishStopwatch.Elapsed.TotalSeconds:F1}s. Waiting for the Shovel to bridge them to Service Bus ...");

        var waitStopwatch = Stopwatch.StartNew();
        var arrivedCount = await WaitForMessagesOnServiceBusAsync(serviceBusConnectionString, destinationQueue, markers, TimeSpan.FromSeconds(timeoutSeconds));
        waitStopwatch.Stop();

        if (arrivedCount == messageCount)
        {
            var rate = messageCount / Math.Max(waitStopwatch.Elapsed.TotalSeconds, 0.001);
            var suffix = messageCount == 1 ? $"message with marker '{markers[0]}'" : $"all {messageCount} messages (marker prefix '{markerPrefix}')";
            Console.WriteLine($"PASS: {suffix} arrived on Service Bus queue '{destinationQueue}' via the RabbitMQ Shovel bridge in {waitStopwatch.Elapsed.TotalSeconds:F1}s ({rate:F1} msg/s).");
            return 0;
        }

        Console.WriteLine($"FAIL: only {arrivedCount}/{messageCount} message(s) (marker prefix '{markerPrefix}') arrived on Service Bus queue '{destinationQueue}' within {timeoutSeconds}s.");
        return 1;
    }

    /// <summary>
    /// Full-pipeline proof: publish the real workflow-trigger message contract (with a
    /// caller bearer token as a message header) and poll the Lightweight engine's own
    /// result endpoint for a terminal, successful execution outcome.
    /// </summary>
    private static async Task<int> RunWorkflowExecutionModeAsync(Dictionary<string, string> opts, Func<string, string> required)
    {
        var rabbitMqManagementUri = required("rabbitmq-management-uri").TrimEnd('/');
        var rabbitMqUsername = required("rabbitmq-username");
        var rabbitMqPassword = required("rabbitmq-password");
        var rabbitMqVHost = opts.GetValueOrDefault("rabbitmq-vhost", "/");
        var sourceQueue = required("source-queue");
        var workflow = required("workflow");
        var engineBaseUrl = required("engine-base-url").TrimEnd('/');
        var messageAuthToken = required("message-auth-token");
        var resultPollAuthToken = opts.GetValueOrDefault("result-poll-auth-token", messageAuthToken);
        var correlationId = opts.GetValueOrDefault("correlation-id", Guid.NewGuid().ToString("N"));
        var jti = opts.GetValueOrDefault("jti", string.Empty);
        var workflowInputsJson = opts.GetValueOrDefault("workflow-inputs-json", string.Empty);
        var timeoutSeconds = int.Parse(opts.GetValueOrDefault("result-timeout-seconds", "90"));

        Console.WriteLine($"CorrelationId: {correlationId}");
        Console.WriteLine($"Publishing '{workflow}' workflow-trigger message to RabbitMQ queue '{sourceQueue}' (vhost '{rabbitMqVHost}') ...");

        await PublishWorkflowTriggerMessageAsync(
            rabbitMqManagementUri, rabbitMqUsername, rabbitMqPassword, rabbitMqVHost, sourceQueue,
            workflow, workflowInputsJson, correlationId, messageAuthToken, jti);
        Console.WriteLine("Published. Waiting for the Shovel to bridge it to Service Bus and the Lightweight engine's in-process trigger to process it ...");

        var result = await WaitForServiceBusResultAsync(engineBaseUrl, resultPollAuthToken, correlationId, TimeSpan.FromSeconds(timeoutSeconds));

        if (result is null)
        {
            Console.WriteLine($"FAIL: no result was recorded for correlationId '{correlationId}' at {engineBaseUrl}/secure/servicebus-result/{correlationId} within {timeoutSeconds}s.");
            return 1;
        }

        var status = result.Value.GetProperty("status").GetString();
        if (string.Equals(status, "Succeeded", StringComparison.OrdinalIgnoreCase))
        {
            var outputs = result.Value.TryGetProperty("outputs", out var outputsEl) ? outputsEl.ToString() : "(none)";
            Console.WriteLine($"PASS: workflow '{workflow}' (correlationId '{correlationId}') executed successfully via RabbitMQ -> Shovel -> Service Bus -> Lightweight engine. Outputs: {outputs}");
            return 0;
        }

        var error = result.Value.TryGetProperty("error", out var errorEl) ? errorEl.GetString() : "(no error detail)";
        Console.WriteLine($"FAIL: workflow '{workflow}' (correlationId '{correlationId}') did not succeed. Status='{status}'. Error='{error}'.");
        return 1;
    }

    private static Dictionary<string, string> ParseArgs(string[] args)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            if (!arg.StartsWith("--", StringComparison.Ordinal))
            {
                throw new ArgumentException($"Unexpected argument '{arg}' (expected --name value pairs).");
            }
            var name = arg[2..];
            if (i + 1 >= args.Length)
            {
                throw new ArgumentException($"Argument --{name} is missing its value.");
            }
            result[name] = args[++i];
        }
        return result;
    }

    private static void PrintUsage()
    {
        Console.WriteLine("""
            Usage: ShovelBridgeE2EHarness --mode arrival [options]
                --rabbitmq-management-uri <uri>        e.g. http://localhost:15672
                --rabbitmq-username <user>
                --rabbitmq-password <password>
                [--rabbitmq-vhost <vhost>]             default: /
                --source-queue <name>
                --servicebus-connection-string <conn>  emulator or real Azure Service Bus
                --destination-queue <name>
                [--timeout-seconds <n>]                default: 60
                [--marker <guid>]                      default: a freshly generated GUID
                [--message-count <n>]                  default: 1; publishes N uniquely-marked
                                                        messages (marker-000000 .. marker-{n-1})
                                                        and waits for all N to arrive - used for
                                                        shovel-bridge load/throughput testing

            Usage: ShovelBridgeE2EHarness --mode workflow-execution [options]
                --rabbitmq-management-uri <uri>
                --rabbitmq-username <user>
                --rabbitmq-password <password>
                [--rabbitmq-vhost <vhost>]             default: /
                --source-queue <name>
                --workflow <name>                      workflow to execute, e.g. "Hello World"
                [--workflow-inputs-json <json>]        e.g. {"Name":"FromRabbitMq"}
                [--correlation-id <id>]                default: a freshly generated GUID
                --message-auth-token <bearer-token>    embedded as the message's Authorization
                                                        property (validated by the engine's
                                                        secure Service Bus trigger)
                [--jti <jti>]                          mirrors the token's own jti claim
                --engine-base-url <url>                e.g. https://warewolfserver-uat.azurewebsites.net
                [--result-poll-auth-token <token>]     default: same as --message-auth-token;
                                                        used to call GET /secure/servicebus-result
                [--result-timeout-seconds <n>]         default: 90
            """);
    }

    /// <summary>
    /// Publishes via the RabbitMQ Management HTTP API's default-exchange publish
    /// endpoint (POST /api/exchanges/{vhost}/amq.default/publish) — deliberately
    /// avoids taking on an AMQP 0.9.1 client dependency just for this one publish.
    /// </summary>
    private static async Task PublishToRabbitMqAsync(string managementUri, string username, string password, string vhost, string queue, string marker)
    {
        using var http = new HttpClient();
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}")));

        await PublishToRabbitMqAsync(http, managementUri, vhost, queue, marker);
    }

    /// <summary>
    /// Publishes <paramref name="markers"/> (one message each) to the same RabbitMQ queue,
    /// reusing a single authenticated <see cref="HttpClient"/> across all of them — publishing
    /// a fresh client per message (as the single-message overload above does) is wasteful once
    /// the count reaches load-test volumes (e.g. 1000 messages).
    /// </summary>
    private static async Task PublishManyToRabbitMqAsync(string managementUri, string username, string password, string vhost, string queue, IReadOnlyList<string> markers)
    {
        using var http = new HttpClient();
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}")));

        foreach (var marker in markers)
        {
            await PublishToRabbitMqAsync(http, managementUri, vhost, queue, marker);
        }
    }

    private static async Task PublishToRabbitMqAsync(HttpClient http, string managementUri, string vhost, string queue, string marker)
    {
        var vhostSegment = vhost == "/" ? "%2f" : Uri.EscapeDataString(vhost);
        var payload = JsonSerializer.Serialize(new { marker, sentAtUtc = DateTime.UtcNow.ToString("o") });

        var body = new
        {
            properties = new { },
            routing_key = queue,
            payload,
            payload_encoding = "string"
        };

        var response = await http.PostAsync(
            $"{managementUri}/api/exchanges/{vhostSegment}/amq.default/publish",
            new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"));

        var responseBody = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"RabbitMQ publish failed ({(int)response.StatusCode}): {responseBody}");
        }

        using var doc = JsonDocument.Parse(responseBody);
        if (!doc.RootElement.TryGetProperty("routed", out var routed) || !routed.GetBoolean())
        {
            throw new InvalidOperationException($"RabbitMQ reported the message was not routed (queue '{queue}' may not exist): {responseBody}");
        }
    }

    /// <summary>
    /// Publishes the real <c>ServiceBusWorkflowMessage</c> JSON body
    /// (<c>{"workflow","inputs","correlationId"}</c>) via the same RabbitMQ Management HTTP
    /// API publish endpoint used by <see cref="PublishToRabbitMqAsync"/>, but additionally
    /// sets AMQP 0.9.1 basic-properties <c>headers</c> for <c>Authorization</c> (and,
    /// optionally, <c>jti</c>) — RabbitMQ's Shovel amqp10 destination bridges these headers
    /// to AMQP 1.0 application properties, which is exactly where
    /// <c>ServiceBusWorkflowTriggerFunction</c> reads the caller's bearer token from (see
    /// docs/ServiceBusSecureTrigger-Architecture.md). Also sets the AMQP 0.9.1
    /// <c>correlation_id</c> basic property as a belt-and-braces fallback — the trigger
    /// prefers the JSON body's own <c>correlationId</c> field, but falls back to the
    /// message's native <c>CorrelationId</c> if that field is omitted.
    /// </summary>
    private static async Task PublishWorkflowTriggerMessageAsync(
        string managementUri, string username, string password, string vhost, string queue,
        string workflow, string workflowInputsJson, string correlationId, string authToken, string jti)
    {
        using var http = new HttpClient();
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}")));

        var vhostSegment = vhost == "/" ? "%2f" : Uri.EscapeDataString(vhost);

        Dictionary<string, string>? inputs = null;
        if (!string.IsNullOrWhiteSpace(workflowInputsJson))
        {
            inputs = JsonSerializer.Deserialize<Dictionary<string, string>>(workflowInputsJson)
                ?? throw new InvalidOperationException($"--workflow-inputs-json did not deserialize to a string map: {workflowInputsJson}");
        }

        var payload = JsonSerializer.Serialize(new { workflow, inputs, correlationId });

        var bearerValue = authToken.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? authToken
            : $"Bearer {authToken}";

        var headers = new Dictionary<string, object> { ["Authorization"] = bearerValue };
        if (!string.IsNullOrWhiteSpace(jti))
        {
            headers["jti"] = jti;
        }

        var body = new
        {
            properties = new { headers, correlation_id = correlationId },
            routing_key = queue,
            payload,
            payload_encoding = "string"
        };

        var response = await http.PostAsync(
            $"{managementUri}/api/exchanges/{vhostSegment}/amq.default/publish",
            new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"));

        var responseBody = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"RabbitMQ publish failed ({(int)response.StatusCode}): {responseBody}");
        }

        using var doc = JsonDocument.Parse(responseBody);
        if (!doc.RootElement.TryGetProperty("routed", out var routed) || !routed.GetBoolean())
        {
            throw new InvalidOperationException($"RabbitMQ reported the message was not routed (queue '{queue}' may not exist): {responseBody}");
        }
    }

    /// <summary>
    /// Polls the Lightweight engine's own <c>GET /secure/servicebus-result/{correlationId}</c>
    /// endpoint (<c>ServiceBusResultFunction</c>) for a terminal outcome. A 404 means the
    /// message hasn't been processed yet (still in flight through the Shovel, or the engine
    /// hasn't dequeued it) and is not an error — polling continues until a 200 (terminal
    /// result recorded) or the timeout elapses. Returns <c>null</c> on timeout.
    /// </summary>
    private static async Task<JsonElement?> WaitForServiceBusResultAsync(
        string engineBaseUrl, string authToken, string correlationId, TimeSpan timeout)
    {
        using var http = new HttpClient();
        var bearerValue = authToken.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? authToken["Bearer ".Length..].Trim()
            : authToken;
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerValue);

        var resultUrl = $"{engineBaseUrl}/secure/servicebus-result/{Uri.EscapeDataString(correlationId)}";
        var deadline = DateTime.UtcNow + timeout;

        while (DateTime.UtcNow < deadline)
        {
            HttpResponseMessage response;
            try
            {
                response = await http.GetAsync(resultUrl);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"  (transient error polling {resultUrl}: {ex.Message} — retrying)");
                await Task.Delay(TimeSpan.FromSeconds(3));
                continue;
            }

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                await Task.Delay(TimeSpan.FromSeconds(3));
                continue;
            }

            var body = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"GET {resultUrl} returned {(int)response.StatusCode}: {body}");
            }

            using var doc = JsonDocument.Parse(body);
            return doc.RootElement.Clone();
        }

        return null;
    }

    /// <summary>
    /// Polls the destination Service Bus queue for messages carrying any of the given
    /// <paramref name="markers"/>, completing each on an exact match (parsed from the
    /// message's own JSON <c>marker</c> property — see <see cref="PublishToRabbitMqAsync(HttpClient, string, string, string, string)"/>)
    /// and abandoning any unrelated message found along the way (the queue is expected to be
    /// dedicated to this test, so this is a safety net, not the expected path). Returns once
    /// every marker has arrived or the timeout elapses, whichever comes first — a single
    /// marker is just the N=1 case of this same loop, so this also serves the original
    /// one-message arrival check.
    /// </summary>
    private static async Task<int> WaitForMessagesOnServiceBusAsync(string connectionString, string queueName, IReadOnlyList<string> markers, TimeSpan timeout)
    {
        await using var client = new ServiceBusClient(connectionString);
        await using var receiver = client.CreateReceiver(queueName);

        var remaining = new HashSet<string>(markers, StringComparer.Ordinal);
        var totalCount = markers.Count;
        var deadline = DateTime.UtcNow + timeout;
        var lastProgressReport = DateTime.UtcNow;

        while (remaining.Count > 0 && DateTime.UtcNow < deadline)
        {
            var remainingTime = deadline - DateTime.UtcNow;
            var waitTime = remainingTime > TimeSpan.FromSeconds(5) ? TimeSpan.FromSeconds(5) : remainingTime;
            if (waitTime <= TimeSpan.Zero) break;

            var messages = await receiver.ReceiveMessagesAsync(maxMessages: 100, maxWaitTime: waitTime);
            foreach (var message in messages)
            {
                var body = message.Body.ToString();
                string? marker = null;
                try
                {
                    using var doc = JsonDocument.Parse(body);
                    if (doc.RootElement.TryGetProperty("marker", out var markerEl))
                    {
                        marker = markerEl.GetString();
                    }
                }
                catch (JsonException)
                {
                    // Not our JSON shape - treated as unrelated below.
                }

                if (marker is not null && remaining.Remove(marker))
                {
                    await receiver.CompleteMessageAsync(message);
                }
                else
                {
                    Console.WriteLine($"  (ignoring unrelated message on the queue: {body})");
                    await receiver.AbandonMessageAsync(message);
                }
            }

            if (totalCount > 1 && DateTime.UtcNow - lastProgressReport >= TimeSpan.FromSeconds(10))
            {
                Console.WriteLine($"  ... {totalCount - remaining.Count}/{totalCount} arrived so far");
                lastProgressReport = DateTime.UtcNow;
            }
        }

        return totalCount - remaining.Count;
    }
}
