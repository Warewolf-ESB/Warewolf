using Microsoft.Extensions.Logging;

namespace WwExecutionCaller;

/// <summary>
/// Typed HttpClient implementation that calls the Warewolf Execution Engine.
/// The injected <see cref="HttpClient"/> is configured in <c>Program.cs</c> with the engine
/// base address and the <see cref="Auth.WwExecutionTokenHandler"/> message handler, so this
/// class never deals with tokens or headers directly — it only shapes URLs and reads responses.
/// </summary>
public sealed class WwExecutionDownstreamService : IWwExecutionDownstreamService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<WwExecutionDownstreamService> _logger;

    public WwExecutionDownstreamService(
        HttpClient httpClient,
        ILogger<WwExecutionDownstreamService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<WwExecutionResult> ExecutePublicAsync(
        string workflow, string? queryString = null, CancellationToken cancellationToken = default)
        => SendAsync("public", workflow, queryString, cancellationToken);

    public Task<WwExecutionResult> ExecuteSecureAsync(
        string workflow, string? queryString = null, CancellationToken cancellationToken = default)
        => SendAsync("secure", workflow, queryString, cancellationToken);

    public Task<WwExecutionResult> ExecuteServicesAsync(
        string workflow, string? queryString = null, CancellationToken cancellationToken = default)
        => SendAsync("services", workflow, queryString, cancellationToken);

    private async Task<WwExecutionResult> SendAsync(
        string routePrefix, string workflow, string? queryString, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(workflow))
        {
            throw new ArgumentException("Workflow name is required.", nameof(workflow));
        }

        var relativeUrl = BuildRelativeUrl(routePrefix, workflow, queryString);
        _logger.LogInformation("Calling Warewolf Execution Engine: GET {RelativeUrl}", relativeUrl);

        using var response = await _httpClient
            .GetAsync(relativeUrl, cancellationToken)
            .ConfigureAwait(false);

        var body = await response.Content
            .ReadAsStringAsync(cancellationToken)
            .ConfigureAwait(false);

        var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/json";
        var statusCode = (int)response.StatusCode;

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "Engine returned {StatusCode} for {RelativeUrl}: {Body}",
                statusCode, relativeUrl, Truncate(body));
        }

        return new WwExecutionResult(statusCode, contentType, body);
    }

    /// <summary>
    /// Builds the route-relative URL: <c>{prefix}/{UrlEncodedWorkflow}.json{?query}</c>.
    /// Each path segment is URL-encoded independently (so names with spaces, e.g. "Hello World",
    /// work) while the '/' separators themselves are preserved — this lets folder-qualified
    /// workflow names such as <c>data/sales</c> resolve to <c>{prefix}/data/sales.json</c> instead
    /// of being collapsed into a single, non-existent "data%2Fsales" segment.
    /// </summary>
    private static string BuildRelativeUrl(string routePrefix, string workflow, string? queryString)
    {
        var encodedSegments = workflow
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Select(Uri.EscapeDataString);
        var path = $"{routePrefix}/{string.Join('/', encodedSegments)}.json";

        if (string.IsNullOrWhiteSpace(queryString))
        {
            return path;
        }

        var query = queryString.StartsWith('?') ? queryString[1..] : queryString;
        return string.IsNullOrEmpty(query) ? path : $"{path}?{query}";
    }

    private static string Truncate(string value, int max = 500) =>
        value.Length <= max ? value : value[..max] + "…";
}
