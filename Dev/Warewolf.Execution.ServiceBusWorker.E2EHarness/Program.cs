using System.Collections.Concurrent;
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
        var publishConcurrency = int.Parse(opts.GetValueOrDefault("publish-concurrency", "1"));
        if (publishConcurrency < 1)
        {
            throw new ArgumentException("--publish-concurrency must be at least 1.");
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
        await PublishManyToRabbitMqAsync(rabbitMqManagementUri, rabbitMqUsername, rabbitMqPassword, rabbitMqVHost, sourceQueue, markers, publishConcurrency);
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
    /// result endpoint for a terminal, successful execution outcome. <c>--message-count</c>
    /// (default 1, unchanged single-message behaviour) drives the same bulk/load use case as
    /// arrival mode's own <c>--message-count</c> — e.g. the ShovelBridge load test job
    /// publishes 1000 distinct workflow-trigger messages here and requires ALL 1000 to
    /// actually execute successfully on the target engine, proving the full
    /// RabbitMQ -> Shovel -> Service Bus -> workflow-execution pipeline end-to-end at that
    /// volume, not just bridge connectivity.
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
        var correlationIdPrefix = opts.GetValueOrDefault("correlation-id", Guid.NewGuid().ToString("N"));
        var jti = opts.GetValueOrDefault("jti", string.Empty);
        var workflowInputsJson = opts.GetValueOrDefault("workflow-inputs-json", string.Empty);
        var timeoutSeconds = int.Parse(opts.GetValueOrDefault("result-timeout-seconds", "90"));
        var messageCount = int.Parse(opts.GetValueOrDefault("message-count", "1"));
        if (messageCount < 1)
        {
            throw new ArgumentException("--message-count must be at least 1.");
        }
        var publishConcurrency = int.Parse(opts.GetValueOrDefault("publish-concurrency", "1"));
        if (publishConcurrency < 1)
        {
            throw new ArgumentException("--publish-concurrency must be at least 1.");
        }

        // Fixed-width numeric suffixes (D6), same convention as arrival mode's markers — each
        // correlationId is independently tracked through publish + result-polling below.
        var correlationIds = messageCount == 1
            ? new[] { correlationIdPrefix }
            : Enumerable.Range(0, messageCount).Select(i => $"{correlationIdPrefix}-{i:D6}").ToArray();

        Console.WriteLine(messageCount == 1
            ? $"CorrelationId: {correlationIds[0]}"
            : $"CorrelationId prefix: {correlationIdPrefix} ({messageCount} messages, '{correlationIds[0]}' .. '{correlationIds[^1]}')");
        Console.WriteLine($"Publishing {messageCount} '{workflow}' workflow-trigger message(s) to RabbitMQ queue '{sourceQueue}' (vhost '{rabbitMqVHost}') ...");
        if (messageCount > 1 && !string.IsNullOrWhiteSpace(workflowInputsJson) && !WorkflowInputTemplate.HasCorrelationIdPlaceholder(workflowInputsJson))
        {
            Console.WriteLine($"  WARNING: --workflow-inputs-json has no '{WorkflowInputTemplate.CorrelationIdPlaceholder}' placeholder, so all {messageCount} messages carry an IDENTICAL inputs map.");
            Console.WriteLine("           A workflow that keys off the message body (e.g. RabbitProcess -> usp_jobs1_LogStart, which takes an exclusive");
            Console.WriteLine("           applock on a hash of the content) will serialise every execution behind one lock instead of running concurrently.");
        }

        var publishStopwatch = Stopwatch.StartNew();
        await PublishManyWorkflowTriggerMessagesAsync(
            rabbitMqManagementUri, rabbitMqUsername, rabbitMqPassword, rabbitMqVHost, sourceQueue,
            workflow, workflowInputsJson, correlationIds, messageAuthToken, jti, publishConcurrency);
        publishStopwatch.Stop();
        Console.WriteLine($"Published {messageCount} message(s) in {publishStopwatch.Elapsed.TotalSeconds:F1}s. Waiting for the Shovel to bridge them to Service Bus and the Lightweight engine's in-process trigger to process them ...");

        var waitStopwatch = Stopwatch.StartNew();
        var (results, lastTransientErrors) = await WaitForServiceBusResultsAsync(engineBaseUrl, resultPollAuthToken, correlationIds, TimeSpan.FromSeconds(timeoutSeconds));
        waitStopwatch.Stop();

        var succeededIds = correlationIds.Where(id => results.TryGetValue(id, out var r) && string.Equals(r.Status, "Succeeded", StringComparison.OrdinalIgnoreCase)).ToArray();
        var failedResults = correlationIds.Where(id => results.ContainsKey(id) && !succeededIds.Contains(id)).Select(id => (Id: id, Result: results[id])).ToArray();
        var neverResolvedIds = correlationIds.Where(id => !results.ContainsKey(id)).ToArray();

        if (messageCount == 1)
        {
            // Preserve the original single-message wording exactly.
            if (neverResolvedIds.Length > 0)
            {
                var lastErrorSuffix = lastTransientErrors.TryGetValue(correlationIds[0], out var lastError) ? $" Last transient error: {lastError}" : string.Empty;
                Console.WriteLine($"FAIL: no result was recorded for correlationId '{correlationIds[0]}' at {engineBaseUrl}/secure/servicebus-result/{correlationIds[0]} within {timeoutSeconds}s.{lastErrorSuffix}");
                return 1;
            }
            if (succeededIds.Length == 1)
            {
                var outputs = results[correlationIds[0]].OutputsJson ?? "(none)";
                Console.WriteLine($"PASS: workflow '{workflow}' (correlationId '{correlationIds[0]}') executed successfully via RabbitMQ -> Shovel -> Service Bus -> Lightweight engine. Outputs: {outputs}");
                return 0;
            }
            var (failedId, failedResult) = failedResults[0];
            Console.WriteLine($"FAIL: workflow '{workflow}' (correlationId '{failedId}') did not succeed. Status='{failedResult.Status}'. Error='{failedResult.Error ?? "(no error detail)"}'.");
            return 1;
        }

        if (succeededIds.Length == messageCount)
        {
            var rate = messageCount / Math.Max(waitStopwatch.Elapsed.TotalSeconds, 0.001);
            Console.WriteLine($"PASS: all {messageCount} '{workflow}' executions (correlationId prefix '{correlationIdPrefix}') succeeded via RabbitMQ -> Shovel -> Service Bus -> Lightweight engine in {waitStopwatch.Elapsed.TotalSeconds:F1}s ({rate:F1} exec/s).");
            return 0;
        }

        Console.WriteLine($"FAIL: only {succeededIds.Length}/{messageCount} '{workflow}' execution(s) (correlationId prefix '{correlationIdPrefix}') succeeded within {timeoutSeconds}s ({failedResults.Length} failed, {neverResolvedIds.Length} never got a result).");
        foreach (var (id, result) in failedResults.Take(10))
        {
            Console.WriteLine($"  FAILED correlationId '{id}': Status='{result.Status}', Error='{result.Error ?? "(no error detail)"}'");
        }
        if (failedResults.Length > 10)
        {
            Console.WriteLine($"  ... and {failedResults.Length - 10} more failures (see /secure/servicebus-result/{{correlationId}} on {engineBaseUrl} for full detail).");
        }
        foreach (var id in neverResolvedIds.Take(10))
        {
            var lastErrorSuffix = lastTransientErrors.TryGetValue(id, out var lastError) ? $"; last transient error: {lastError}" : string.Empty;
            Console.WriteLine($"  NO RESULT for correlationId '{id}' (never appeared at /secure/servicebus-result/{{correlationId}}{lastErrorSuffix}).");
        }
        if (neverResolvedIds.Length > 10)
        {
            Console.WriteLine($"  ... and {neverResolvedIds.Length - 10} more correlationIds with no result.");
        }
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
                [--publish-concurrency <n>]            default: 1 (sequential, unchanged
                                                        behaviour); bounds how many of the N
                                                        publishes above are in flight at once -
                                                        raise this (e.g. 20) at load-test volumes,
                                                        since sequential HTTP-per-message publish
                                                        is the dominant cost, not shovel throughput

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
                [--message-count <n>]                  default: 1; publishes N workflow-trigger
                                                        messages with distinct correlationIds
                                                        (correlation-id-000000 .. -{n-1}) and
                                                        requires ALL N to report a Succeeded
                                                        result - used for full end-to-end
                                                        (bridge + workflow execution) load testing
                [--publish-concurrency <n>]            default: 1 (sequential, unchanged
                                                        behaviour); bounds how many of the N
                                                        publishes above are in flight at once -
                                                        raise this (e.g. 20) at load-test volumes,
                                                        since sequential HTTP-per-message publish
                                                        is the dominant cost, not shovel throughput
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
    /// the count reaches load-test volumes (e.g. 1000 messages). <paramref name="maxConcurrency"/>
    /// (default 1, unchanged sequential behaviour) bounds how many publishes are in flight at
    /// once — see <see cref="PublishManyBoundedAsync"/> for why this is the harness's actual
    /// throughput knob at load-test volumes.
    /// </summary>
    private static async Task PublishManyToRabbitMqAsync(string managementUri, string username, string password, string vhost, string queue, IReadOnlyList<string> markers, int maxConcurrency = 1)
    {
        using var http = new HttpClient();
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}")));

        await PublishManyBoundedAsync(markers, marker => PublishToRabbitMqAsync(http, managementUri, vhost, queue, marker), maxConcurrency);
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
    private static async Task PublishManyWorkflowTriggerMessagesAsync(
        string managementUri, string username, string password, string vhost, string queue,
        string workflow, string workflowInputsJson, IReadOnlyList<string> correlationIds, string authToken, string jti,
        int maxConcurrency = 1)
    {
        using var http = new HttpClient();
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}")));

        await PublishManyBoundedAsync(
            correlationIds,
            id => PublishWorkflowTriggerMessageAsync(http, managementUri, vhost, queue, workflow, workflowInputsJson, id, authToken, jti),
            maxConcurrency);
    }

    /// <summary>
    /// Fans <paramref name="publishOne"/> out over <paramref name="items"/> with an
    /// in-flight cap of <paramref name="maxConcurrency"/> concurrent publishes (a
    /// <see cref="SemaphoreSlim"/>, same bounded-concurrency shape as
    /// <see cref="WaitForServiceBusResultsAsync"/>'s result polling below) — publishing one
    /// message at a time (the original, still-default behaviour at
    /// <paramref name="maxConcurrency"/> == 1) is the dominant cost at load-test volumes
    /// (e.g. 1000 messages), since each publish is a separate blocking HTTP round-trip to the
    /// RabbitMQ Management API. Raising <paramref name="maxConcurrency"/> pipelines multiple
    /// round-trips at once, which is the actual throughput knob for the harness's own publish
    /// step (the Shovel's own <c>src-prefetch-count</c> is a separate, later-stage knob — see
    /// docs/ShovelBridge-Architecture.md).
    /// </summary>
    private static async Task PublishManyBoundedAsync(IReadOnlyList<string> items, Func<string, Task> publishOne, int maxConcurrency)
    {
        using var throttle = new SemaphoreSlim(Math.Max(1, maxConcurrency));
        var completed = 0;
        var progressStopwatch = Stopwatch.StartNew();
        var progressLock = new object();

        var tasks = items.Select(async item =>
        {
            await throttle.WaitAsync();
            try
            {
                await publishOne(item);
            }
            finally
            {
                throttle.Release();
            }

            var done = Interlocked.Increment(ref completed);
            if (items.Count > 1 && progressStopwatch.Elapsed.TotalSeconds >= 10)
            {
                lock (progressLock)
                {
                    if (progressStopwatch.Elapsed.TotalSeconds >= 10)
                    {
                        Console.WriteLine($"  ... published {done}/{items.Count}");
                        progressStopwatch.Restart();
                    }
                }
            }
        });

        await Task.WhenAll(tasks);
    }

    private static async Task PublishWorkflowTriggerMessageAsync(
        HttpClient http, string managementUri, string vhost, string queue,
        string workflow, string workflowInputsJson, string correlationId, string authToken, string jti)
    {
        var vhostSegment = vhost == "/" ? "%2f" : Uri.EscapeDataString(vhost);

        // Expand {correlationId} so each message in a bulk run carries a distinct inputs map -
        // see WorkflowInputTemplate for why identical bodies serialise a load run.
        var expandedInputsJson = WorkflowInputTemplate.Expand(workflowInputsJson, correlationId);

        Dictionary<string, string>? inputs = null;
        if (!string.IsNullOrWhiteSpace(expandedInputsJson))
        {
            inputs = JsonSerializer.Deserialize<Dictionary<string, string>>(expandedInputsJson)
                ?? throw new InvalidOperationException($"--workflow-inputs-json did not deserialize to a string map: {expandedInputsJson}");
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

    private readonly record struct ServiceBusResult(string Status, string? Error, string? OutputsJson);

    /// <summary>
    /// Concurrently polls the Lightweight engine's own
    /// GET /secure/servicebus-result/{correlationId} endpoint (ServiceBusResultFunction) for
    /// every correlationId in correlationIds until each has a terminal result or the shared
    /// timeout elapses. There is no bulk/batch results endpoint, so this issues one GET per
    /// pending correlationId per sweep, bounded by maxConcurrency concurrent requests,
    /// removing each id from the pending set as soon as it resolves. A 404 means the message
    /// hasn't been processed yet (still in flight through the Shovel, or the engine hasn't
    /// dequeued it) and is not an error. The single-correlationId case (N=1) is just the
    /// degenerate case of this same sweep loop.
    /// </summary>
    private static async Task<(ConcurrentDictionary<string, ServiceBusResult> Results, ConcurrentDictionary<string, string> LastTransientErrors)> WaitForServiceBusResultsAsync(
        string engineBaseUrl, string authToken, IReadOnlyList<string> correlationIds, TimeSpan timeout, int maxConcurrency = 20)
    {
        using var http = new HttpClient();
        var bearerValue = authToken.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? authToken["Bearer ".Length..].Trim()
            : authToken;
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerValue);

        var results = new ConcurrentDictionary<string, ServiceBusResult>();
        // Remembers the most recent transient (retried) error per still-pending id purely for
        // end-of-run diagnostics - see the "NO RESULT" reporting below - it never marks an id
        // resolved.
        var lastTransientErrors = new ConcurrentDictionary<string, string>();
        var pending = new ConcurrentDictionary<string, byte>(correlationIds.Select(id => new KeyValuePair<string, byte>(id, 0)));
        var deadline = DateTime.UtcNow + timeout;
        using var throttle = new SemaphoreSlim(maxConcurrency);
        var progressStopwatch = Stopwatch.StartNew();

        while (pending.Count > 0 && DateTime.UtcNow < deadline)
        {
            var sweepIds = pending.Keys.ToArray();
            var sweepTasks = sweepIds.Select(async id =>
            {
                await throttle.WaitAsync();
                try
                {
                    var (result, transientError) = await PollOneServiceBusResultAsync(http, engineBaseUrl, id);
                    if (result is { } resolved)
                    {
                        results[id] = resolved;
                        pending.TryRemove(id, out _);
                        lastTransientErrors.TryRemove(id, out _);
                    }
                    else if (transientError is { } error)
                    {
                        lastTransientErrors[id] = error;
                    }
                }
                finally
                {
                    throttle.Release();
                }
            });
            await Task.WhenAll(sweepTasks);

            if (correlationIds.Count > 1 && pending.Count > 0 && progressStopwatch.Elapsed.TotalSeconds >= 10)
            {
                Console.WriteLine($"  ... {results.Count}/{correlationIds.Count} resolved, {pending.Count} still pending");
                progressStopwatch.Restart();
            }

            if (pending.Count > 0 && DateTime.UtcNow < deadline)
            {
                await Task.Delay(TimeSpan.FromSeconds(3));
            }
        }

        return (results, lastTransientErrors);
    }

    /// <summary>
    /// One GET against the result endpoint for a single correlationId. Returns a null Result if
    /// the result isn't recorded yet (404 - still in flight, not an error), on a transient
    /// network error, on a client-side request timeout (HttpClient.Timeout, default 100s,
    /// elapsing for this one poll under load - surfaces as a TaskCanceledException, NOT an
    /// HttpRequestException, since no CancellationToken is ever passed to GetAsync here so
    /// there is no other source of cancellation to confuse it with), or on a transient
    /// server-side status code (503/502/504/429/408 - the engine overloaded, cold-starting, or
    /// mid-restart under load-test volumes) - in all of these cases the caller's sweep loop
    /// retries the id on the next sweep instead of giving up on it permanently. Returns a
    /// terminal ServiceBusResult for a recorded success/failure outcome, or an "HttpError"
    /// result for any other unexpected non-2xx/non-404 response (e.g. a real 500 auth denial
    /// per WOLF-8418, which is NOT transient) so it doesn't abort polling for the rest of a
    /// bulk run.
    /// </summary>
    private static async Task<(ServiceBusResult? Result, string? TransientError)> PollOneServiceBusResultAsync(HttpClient http, string engineBaseUrl, string correlationId)
    {
        var resultUrl = $"{engineBaseUrl}/secure/servicebus-result/{Uri.EscapeDataString(correlationId)}";

        HttpResponseMessage response;
        try
        {
            response = await http.GetAsync(resultUrl);
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"  (transient error polling {resultUrl}: {ex.Message} - retrying)");
            return (null, ex.Message);
        }
        catch (OperationCanceledException ex)
        {
            // HttpClient.Timeout elapsing throws TaskCanceledException (a subclass of
            // OperationCanceledException), not HttpRequestException - previously uncaught here,
            // this propagated out of WaitForServiceBusResultsAsync's Task.WhenAll and aborted
            // the ENTIRE bulk run (all still-pending correlationIds, not just this one poll)
            // even with a large --result-timeout-seconds budget remaining. Treat it exactly
            // like the other transient cases above: retry this one id on the next sweep.
            var detail = $"HttpClient.Timeout ({http.Timeout.TotalSeconds:F0}s) elapsed polling {resultUrl}: {ex.Message}";
            Console.WriteLine($"  (transient timeout polling {resultUrl} - retrying)");
            return (null, detail);
        }

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return (null, null);
        }

        var body = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            var detail = $"GET {resultUrl} returned {(int)response.StatusCode}: {body}";
            if (IsTransientStatusCode(response.StatusCode))
            {
                return (null, detail);
            }
            return (new ServiceBusResult("HttpError", detail, null), null);
        }

        using var doc = JsonDocument.Parse(body);
        var status = doc.RootElement.GetProperty("status").GetString() ?? "Unknown";
        var error = doc.RootElement.TryGetProperty("error", out var errorEl) ? errorEl.GetString() : null;
        var outputsJson = doc.RootElement.TryGetProperty("outputs", out var outputsEl) ? outputsEl.ToString() : null;
        return (new ServiceBusResult(status, error, outputsJson), null);
    }

    /// <summary>
    /// Status codes worth retrying rather than failing permanently: transport/scale hiccups on
    /// the engine side (overloaded, cold-starting, or mid-restart under load-test volumes), not
    /// genuine application-level denials. 500 is deliberately excluded - per WOLF-8418, the
    /// Lightweight engine's auth middleware wraps policy denials as HTTP 500, which is a real,
    /// terminal failure, not a transient one.
    /// </summary>
    private static bool IsTransientStatusCode(System.Net.HttpStatusCode statusCode) =>
        statusCode is System.Net.HttpStatusCode.ServiceUnavailable
            or System.Net.HttpStatusCode.BadGateway
            or System.Net.HttpStatusCode.GatewayTimeout
            or System.Net.HttpStatusCode.RequestTimeout
            or System.Net.HttpStatusCode.TooManyRequests;


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
