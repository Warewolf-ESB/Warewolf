using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Azure.Messaging.ServiceBus;

namespace Warewolf.Execution.ServiceBusWorker.E2EHarness;

/// <summary>
/// Publishes a uniquely-marked test message to a RabbitMQ source queue (via the
/// RabbitMQ Management HTTP API's "publish" endpoint — no AMQP 0.9.1 client
/// dependency needed for that side) and then polls a Service Bus queue (via the
/// official SDK — real AMQP 1.0) for the same marker to arrive, proving a
/// RabbitMQ Shovel actually bridged the message across protocols.
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

        try
        {
            var rabbitMqManagementUri = Required("rabbitmq-management-uri").TrimEnd('/');
            var rabbitMqUsername = Required("rabbitmq-username");
            var rabbitMqPassword = Required("rabbitmq-password");
            var rabbitMqVHost = opts.GetValueOrDefault("rabbitmq-vhost", "/");
            var sourceQueue = Required("source-queue");
            var serviceBusConnectionString = Required("servicebus-connection-string");
            var destinationQueue = Required("destination-queue");
            var timeoutSeconds = int.Parse(opts.GetValueOrDefault("timeout-seconds", "60"));
            var marker = opts.GetValueOrDefault("marker", Guid.NewGuid().ToString("N"));

            Console.WriteLine($"Marker: {marker}");
            Console.WriteLine($"Publishing test message to RabbitMQ queue '{sourceQueue}' (vhost '{rabbitMqVHost}') ...");

            await PublishToRabbitMqAsync(rabbitMqManagementUri, rabbitMqUsername, rabbitMqPassword, rabbitMqVHost, sourceQueue, marker);
            Console.WriteLine("Published. Waiting for the Shovel to bridge it to Service Bus ...");

            var found = await WaitForMessageOnServiceBusAsync(serviceBusConnectionString, destinationQueue, marker, TimeSpan.FromSeconds(timeoutSeconds));

            if (found)
            {
                Console.WriteLine($"PASS: message with marker '{marker}' arrived on Service Bus queue '{destinationQueue}' via the RabbitMQ Shovel bridge.");
                return 0;
            }

            Console.WriteLine($"FAIL: message with marker '{marker}' did NOT arrive on Service Bus queue '{destinationQueue}' within {timeoutSeconds}s.");
            return 1;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"FAIL: {ex.GetType().Name}: {ex.Message}");
            return 1;
        }
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
            Usage: ShovelBridgeE2EHarness
                --rabbitmq-management-uri <uri>        e.g. http://localhost:15672
                --rabbitmq-username <user>
                --rabbitmq-password <password>
                [--rabbitmq-vhost <vhost>]             default: /
                --source-queue <name>
                --servicebus-connection-string <conn>  emulator or real Azure Service Bus
                --destination-queue <name>
                [--timeout-seconds <n>]                default: 60
                [--marker <guid>]                      default: a freshly generated GUID
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
    /// Polls the destination Service Bus queue for a message carrying the given
    /// marker, completing it on match and abandoning any unrelated messages
    /// found along the way (the queue is expected to be dedicated to this test,
    /// so this is a safety net, not the expected path).
    /// </summary>
    private static async Task<bool> WaitForMessageOnServiceBusAsync(string connectionString, string queueName, string marker, TimeSpan timeout)
    {
        await using var client = new ServiceBusClient(connectionString);
        await using var receiver = client.CreateReceiver(queueName);

        var deadline = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < deadline)
        {
            var remaining = deadline - DateTime.UtcNow;
            var waitTime = remaining > TimeSpan.FromSeconds(5) ? TimeSpan.FromSeconds(5) : remaining;
            if (waitTime <= TimeSpan.Zero) break;

            var messages = await receiver.ReceiveMessagesAsync(maxMessages: 10, maxWaitTime: waitTime);
            foreach (var message in messages)
            {
                var body = message.Body.ToString();
                if (body.Contains(marker, StringComparison.Ordinal))
                {
                    await receiver.CompleteMessageAsync(message);
                    return true;
                }

                Console.WriteLine($"  (ignoring unrelated message on the queue: {body})");
                await receiver.AbandonMessageAsync(message);
            }
        }

        return false;
    }
}
