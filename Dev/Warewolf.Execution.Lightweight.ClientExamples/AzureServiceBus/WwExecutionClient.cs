using Microsoft.Extensions.Logging;
using System.Net;

namespace Warewolf.Execution.ClientExamples.AzureServiceBus;

/// <summary>
/// Default <see cref="IWwExecutionClient"/>. A thin wrapper over a typed <see cref="HttpClient"/>
/// whose pipeline includes <c>WwExecutionTokenHandler</c> — so this class focuses purely on
/// building routes and returning bodies, never on auth.
/// </summary>
public sealed class WwExecutionClient : IWwExecutionClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WwExecutionClient> _logger;

    public WwExecutionClient(HttpClient httpClient, ILogger<WwExecutionClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<string> ExecuteSecureAsync(
        string workflow, IDictionary<string, string?>? query, CancellationToken cancellationToken = default)
        => SendAsync("secure", workflow, query, cancellationToken);

    public Task<string> ExecuteServiceAsync(
        string workflow, IDictionary<string, string?>? query, CancellationToken cancellationToken = default)
        => SendAsync("services", workflow, query, cancellationToken);

    public Task<string> ExecutePublicAsync(
        string workflow, IDictionary<string, string?>? query, CancellationToken cancellationToken = default)
        => SendAsync("public", workflow, query, cancellationToken);

    private async Task<string> SendAsync(
        string area, string workflow, IDictionary<string, string?>? query, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(workflow))
        {
            throw new ArgumentException("Workflow name must be supplied.", nameof(workflow));
        }

        var relativeUrl = BuildRelativeUrl(area, workflow, query);
        _logger.LogInformation("Calling Warewolf Execution Engine: GET {RelativeUrl}", relativeUrl);

        using var request = new HttpRequestMessage(HttpMethod.Get, relativeUrl);
        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);

        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            // The engine wraps authorization denials as HTTP 500 (nested Error{…}) rather than 403
            // (WOLF-8418). 401 indicates a token problem; a 500 denial usually means the caller's
            // identity has no app role on the engine's resource service principal.
            _logger.LogError(
                "Engine returned {StatusCode} for {RelativeUrl}. Body: {Body}",
                (int)response.StatusCode, relativeUrl, body);

            throw new WwExecutionException(response.StatusCode, relativeUrl, body);
        }

        return body;
    }

    private static string BuildRelativeUrl(string area, string workflow, IDictionary<string, string?>? query)
    {
        // Encode each '/'-separated segment independently (e.g. "Hello World" -> "Hello%20World")
        // while preserving the '/' separators — so folder-qualified names such as "data/sales"
        // resolve to "{area}/data/sales.json" instead of collapsing into a single "data%2Fsales" segment.
        var encodedSegments = workflow
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Select(Uri.EscapeDataString);
        var path = $"{area}/{string.Join('/', encodedSegments)}.json";

        if (query is null || query.Count == 0)
        {
            return path;
        }

        var parts = query
            .Where(kvp => !string.IsNullOrEmpty(kvp.Key))
            .Select(kvp =>
                $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value ?? string.Empty)}");

        return $"{path}?{string.Join('&', parts)}";
    }
}

/// <summary>Raised when the engine returns a non-success status code.</summary>
public sealed class WwExecutionException(HttpStatusCode statusCode, string relativeUrl, string responseBody)
    : Exception($"Warewolf Execution Engine returned {(int)statusCode} for '{relativeUrl}'.")
{
    public HttpStatusCode StatusCode { get; } = statusCode;
    public string RelativeUrl { get; } = relativeUrl;
    public string ResponseBody { get; } = responseBody;
}
