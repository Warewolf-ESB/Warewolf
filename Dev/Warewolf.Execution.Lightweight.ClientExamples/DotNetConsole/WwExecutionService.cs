using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace WwExecutionClient;

/// <summary>
/// Typed HTTP client for the Warewolf Execution Engine. Builds correctly URI-encoded route paths and
/// performs the calls; authentication is handled transparently upstream by
/// <see cref="Auth.BearerTokenHandler"/>, so this service never deals with tokens.
/// </summary>
/// <remarks>
/// Routes (all return JSON via the <c>.json</c> suffix):
/// <list type="bullet">
///   <item><c>GET  /public/{workflow}.json</c>   - anonymous.</item>
///   <item><c>GET|POST /secure/{workflow}.json</c>  - requires a bearer token.</item>
///   <item><c>GET|POST /services/{workflow}.json</c> - requires bearer token + <c>x-functions-key</c>.</item>
///   <item><c>GET  /apis.json</c> - discovery.</item>
/// </list>
/// </remarks>
public sealed class WwExecutionService
{
    private readonly HttpClient _http;
    private readonly ILogger<WwExecutionService> _logger;

    public WwExecutionService(HttpClient http, ILogger<WwExecutionService> logger)
    {
        _http = http;
        _logger = logger;
    }

    /// <summary>Calls the anonymous <c>GET /public/{workflow}.json</c> route.</summary>
    public Task<string> GetPublicAsync(
        string workflow,
        IReadOnlyDictionary<string, string>? query = null,
        CancellationToken ct = default)
        => SendAsync(HttpMethod.Get, BuildPath("public", workflow, query), body: null, ct);

    /// <summary>Calls the secured <c>GET /secure/{workflow}.json</c> route (bearer token auto-injected).</summary>
    public Task<string> GetSecureAsync(
        string workflow,
        IReadOnlyDictionary<string, string>? query = null,
        CancellationToken ct = default)
        => SendAsync(HttpMethod.Get, BuildPath("secure", workflow, query), body: null, ct);

    /// <summary>Calls the secured <c>POST /secure/{workflow}.json</c> route with a JSON body.</summary>
    public Task<string> PostSecureAsync(
        string workflow,
        object body,
        IReadOnlyDictionary<string, string>? query = null,
        CancellationToken ct = default)
        => SendAsync(HttpMethod.Post, BuildPath("secure", workflow, query), body, ct);

    /// <summary>Calls <c>GET /services/{workflow}.json</c> (bearer + <c>x-functions-key</c> auto-injected).</summary>
    public Task<string> GetServiceAsync(
        string workflow,
        IReadOnlyDictionary<string, string>? query = null,
        CancellationToken ct = default)
        => SendAsync(HttpMethod.Get, BuildPath("services", workflow, query), body: null, ct);

    /// <summary>Calls <c>POST /services/{workflow}.json</c> with a JSON body (bearer + <c>x-functions-key</c>).</summary>
    public Task<string> PostServiceAsync(
        string workflow,
        object body,
        IReadOnlyDictionary<string, string>? query = null,
        CancellationToken ct = default)
        => SendAsync(HttpMethod.Post, BuildPath("services", workflow, query), body, ct);

    /// <summary>Calls the discovery endpoint <c>GET /apis.json</c>.</summary>
    public Task<string> GetApisAsync(CancellationToken ct = default)
        => SendAsync(HttpMethod.Get, "apis.json", body: null, ct);

    // ---------------------------------------------------------------------

    private async Task<string> SendAsync(HttpMethod method, string relativePath, object? body, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(method, relativePath);

        if (body is not null)
        {
            var json = JsonSerializer.Serialize(body);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        _logger.LogInformation("{Method} {Path}", method, relativePath);

        using var response = await _http.SendAsync(request, ct).ConfigureAwait(false);
        var payload = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            // The engine wraps authorization denials as HTTP 500 (nested Error{...}); 401/400/503 use a
            // flat {error,message,path,correlationId} body. Surface both status and payload for diagnosis.
            _logger.LogWarning("Request to {Path} failed: {Status}. Body: {Body}",
                relativePath, (int)response.StatusCode, payload);
            throw new HttpRequestException(
                $"Request to '{relativePath}' returned {(int)response.StatusCode} {response.StatusCode}. Body: {payload}");
        }

        return payload;
    }

    /// <summary>
    /// Builds a route-relative path of the form <c>{prefix}/{uri-encoded-workflow}.json{?query}</c>.
    /// The workflow name is URI-encoded so names with spaces (e.g. <c>Hello World</c>) work correctly.
    /// </summary>
    private static string BuildPath(string prefix, string workflow, IReadOnlyDictionary<string, string>? query)
    {
        var encodedWorkflow = Uri.EscapeDataString(workflow);
        var path = $"{prefix}/{encodedWorkflow}.json";

        if (query is { Count: > 0 })
        {
            var queryString = string.Join("&", query.Select(kvp =>
                $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
            path = $"{path}?{queryString}";
        }

        return path;
    }
}
