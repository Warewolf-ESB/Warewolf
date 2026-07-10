using System.Web;
using Microsoft.Identity.Abstractions;

namespace WwExecutionWebMvc.Services;

/// <summary>
/// Default <see cref="IWwExecutionService"/> implementation.
///
/// Secure and services calls go through <see cref="IDownstreamApi"/>, which:
///   1. reads the named "WwExecution" options (BaseUrl + Scopes),
///   2. silently acquires a delegated access token for the configured scope
///      (<c>api://{resourceAppId}/user_impersonation</c>) on the signed-in
///      user's behalf using the server-side token cache, and
///   3. injects it as <c>Authorization: Bearer &lt;token&gt;</c> automatically.
///
/// Public calls and /apis.json need no credentials, so they use a plain
/// <see cref="HttpClient"/> built from the configured BaseUrl.
/// </summary>
public sealed class WwExecutionService : IWwExecutionService
{
    /// <summary>The downstream API name registered in <c>Program.cs</c> and bound to the "WwExecution" config section.</summary>
    public const string DownstreamApiName = "WwExecution";

    private readonly IDownstreamApi _downstreamApi;
    private readonly HttpClient _anonymousClient;
    private readonly string _baseUrl;
    private readonly string? _functionKey;

    public WwExecutionService(
        IDownstreamApi downstreamApi,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _downstreamApi = downstreamApi;

        _baseUrl = configuration["WwExecution:BaseUrl"]?.TrimEnd('/')
                   ?? throw new InvalidOperationException("WwExecution:BaseUrl is not configured.");
        _functionKey = configuration["WwExecution:FunctionKey"];

        _anonymousClient = httpClientFactory.CreateClient();
        _anonymousClient.BaseAddress = new Uri(_baseUrl + "/");
    }

    public async Task<string> ExecutePublicAsync(string workflow, IDictionary<string, string?>? query = null, CancellationToken ct = default)
    {
        var relative = BuildRelativePath("public", workflow, query);
        using var response = await _anonymousClient.GetAsync(relative, ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(ct);
    }

    public Task<string> ExecuteSecureAsync(string workflow, IDictionary<string, string?>? query = null, CancellationToken ct = default)
        => CallWithTokenAsync("secure", workflow, query, addFunctionKey: false, ct);

    public Task<string> ExecuteServiceAsync(string workflow, IDictionary<string, string?>? query = null, CancellationToken ct = default)
        => CallWithTokenAsync("services", workflow, query, addFunctionKey: true, ct);

    public async Task<string> GetApisAsync(CancellationToken ct = default)
    {
        using var response = await _anonymousClient.GetAsync("apis.json", ct);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(ct);
    }

    /// <summary>
    /// Performs a delegated call via <see cref="IDownstreamApi"/>. The Bearer
    /// token for the engine is acquired and attached by Microsoft.Identity.Web.
    /// If interactive consent / re-auth is needed,
    /// <see cref="Microsoft.Identity.Web.MicrosoftIdentityWebChallengeUserException"/>
    /// bubbles up so the controller can issue a challenge.
    /// </summary>
    private async Task<string> CallWithTokenAsync(
        string routePrefix,
        string workflow,
        IDictionary<string, string?>? query,
        bool addFunctionKey,
        CancellationToken ct)
    {
        var relative = BuildRelativePath(routePrefix, workflow, query);

        using var response = await _downstreamApi.CallApiForUserAsync(
            DownstreamApiName,
            options =>
            {
                options.HttpMethod = HttpMethod.Get.Method;
                options.RelativePath = relative;
                if (addFunctionKey && !string.IsNullOrWhiteSpace(_functionKey))
                {
                    // /services/* requires the host function key in addition to the Bearer token.
                    options.CustomizeHttpRequestMessage =
                        request => request.Headers.TryAddWithoutValidation("x-functions-key", _functionKey);
                }
            },
            cancellationToken: ct);

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(ct);
    }

    /// <summary>
    /// Builds a relative path like <c>secure/Hello%20World.json?Name=Alice</c>.
    /// The workflow segment is URI-encoded so names containing spaces or other
    /// reserved characters round-trip correctly.
    /// </summary>
    private static string BuildRelativePath(string routePrefix, string workflow, IDictionary<string, string?>? query)
    {
        var encodedWorkflow = Uri.EscapeDataString(workflow);
        var path = $"{routePrefix}/{encodedWorkflow}.json";

        if (query is { Count: > 0 })
        {
            var pairs = query
                .Where(kv => kv.Value is not null)
                .Select(kv => $"{HttpUtility.UrlEncode(kv.Key)}={HttpUtility.UrlEncode(kv.Value)}");
            var queryString = string.Join("&", pairs);
            if (queryString.Length > 0)
            {
                path += "?" + queryString;
            }
        }

        return path;
    }
}
