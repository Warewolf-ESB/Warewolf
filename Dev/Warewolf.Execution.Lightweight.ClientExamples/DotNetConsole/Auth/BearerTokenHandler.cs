using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace WwExecutionClient.Auth;

/// <summary>
/// The centerpiece of the example: a <see cref="DelegatingHandler"/> that transparently attaches the
/// <c>Authorization: Bearer &lt;token&gt;</c> header to every outgoing request, acquiring the token from
/// <see cref="TokenAcquirer"/> (cache/silent-first). It also adds the <c>x-functions-key</c> header for
/// <c>/services/*</c> routes and never adds auth to anonymous <c>/public/*</c> routes.
/// </summary>
/// <remarks>
/// Because the typed <see cref="WwExecutionService"/> uses an <see cref="HttpClient"/> wired with this
/// handler, callers never touch tokens directly - the handler resolves the right credential per request.
/// </remarks>
public sealed class BearerTokenHandler : DelegatingHandler
{
    private readonly TokenAcquirer _tokenAcquirer;
    private readonly WwExecutionClientOptions _options;
    private readonly ILogger<BearerTokenHandler> _logger;

    /// <summary>
    /// The auth flow used to obtain tokens. Set once at startup (see <c>Program.cs</c>). Mutable so the
    /// demo menu can switch flows at runtime without rebuilding the host.
    /// </summary>
    public AuthFlow Flow { get; set; } = AuthFlow.Interactive;

    public BearerTokenHandler(
        TokenAcquirer tokenAcquirer,
        IOptions<WwExecutionClientOptions> options,
        ILogger<BearerTokenHandler> logger)
    {
        _tokenAcquirer = tokenAcquirer;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var path = request.RequestUri?.AbsolutePath ?? string.Empty;

        // /public/* is anonymous - do not attach a token (sending one is harmless but unnecessary).
        if (IsPublicRoute(path))
        {
            _logger.LogDebug("Public route {Path}: no bearer token attached.", path);
            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

        // /secure/* and /services/* require a bearer token.
        var auth = await _tokenAcquirer.AcquireAsync(Flow, cancellationToken).ConfigureAwait(false);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);

        // /services/* additionally requires the Azure Functions key.
        if (IsServicesRoute(path))
        {
            if (!_options.HasFunctionKey)
            {
                throw new InvalidOperationException(
                    $"Route '{path}' requires a function key, but WwExecution:FunctionKey is not configured.");
            }

            // Only set if a caller hasn't already supplied one.
            if (!request.Headers.Contains("x-functions-key"))
            {
                request.Headers.Add("x-functions-key", _options.FunctionKey);
            }
            _logger.LogDebug("Services route {Path}: bearer + x-functions-key attached.", path);
        }
        else
        {
            _logger.LogDebug("Secure route {Path}: bearer token attached.", path);
        }

        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }

    private static bool IsPublicRoute(string path) =>
        path.StartsWith("/public/", StringComparison.OrdinalIgnoreCase);

    private static bool IsServicesRoute(string path) =>
        path.StartsWith("/services/", StringComparison.OrdinalIgnoreCase);
}
