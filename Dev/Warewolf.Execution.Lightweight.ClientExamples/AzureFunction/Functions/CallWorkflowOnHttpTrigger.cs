using System.Net;
using System.Text.Json;
using Azure.Core;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace WwExecutionCaller.Functions;

/// <summary>
/// HTTP-trigger that proxies incoming requests to the Warewolf Execution Engine.
///
///   GET  http://localhost:7071/api/run/Hello%20World?Name=CallerTest
///        → engine GET /secure/Hello%20World.json?Name=CallerTest
///
///   GET  http://localhost:7071/api/runpublic/Hello%20World?Name=CallerTest
///        → engine GET /public/Hello%20World.json?Name=CallerTest
///
/// The catch-all <c>{*workflow}</c> route segment captures the (possibly slash-containing)
/// workflow name; the inbound query string is forwarded verbatim. The downstream Bearer token
/// is attached automatically by <see cref="Auth.WwExecutionTokenHandler"/>.
/// </summary>
public sealed class CallWorkflowOnHttpTrigger
{
    private readonly IWwExecutionDownstreamService _engine;
    private readonly ILogger<CallWorkflowOnHttpTrigger> _logger;
    private readonly TokenCredential _credential;
    private readonly WwExecutionCallerOptions _options;

    public CallWorkflowOnHttpTrigger(
        IWwExecutionDownstreamService engine,
        ILogger<CallWorkflowOnHttpTrigger> logger,
        TokenCredential credential,
        IOptions<WwExecutionCallerOptions> options)
    {
        _engine = engine;
        _logger = logger;
        _credential = credential;
        _options = options.Value;
    }

    [Function(nameof(CallWorkflowOnHttpTrigger))]
    public async Task<HttpResponseData> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "run/{*workflow}")]
        HttpRequestData request,
        string workflow,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(workflow))
        {
            var bad = request.CreateResponse(HttpStatusCode.BadRequest);
            await bad.WriteStringAsync(
                "Workflow name is required. Example: /api/run/Hello%20World?Name=CallerTest",
                cancellationToken);
            return bad;
        }

        // The {*workflow} segment arrives URL-decoded; forward the raw inbound query string.
        var queryString = request.Url.Query; // includes leading '?', or empty
        _logger.LogInformation(
            "Proxying workflow '{Workflow}' (query '{Query}') to /secure.", workflow, queryString);

        var result = await _engine
            .ExecuteSecureAsync(workflow, queryString, cancellationToken)
            .ConfigureAwait(false);

        var response = request.CreateResponse((HttpStatusCode)result.StatusCode);
        response.Headers.Add("Content-Type", $"{result.ContentType}; charset=utf-8");
        await response.WriteStringAsync(result.Body, cancellationToken);
        return response;
    }

    /// <summary>
    /// Proxies an incoming request to the Warewolf Execution Engine's anonymous
    /// <c>/public/{workflow}.json</c> route. The engine does not require a Bearer token for this
    /// route, though the typed <c>HttpClient</c> still attaches one automatically via
    /// <see cref="Auth.WwExecutionTokenHandler"/>.
    /// </summary>
    [Function(nameof(RunPublic))]
    public async Task<HttpResponseData> RunPublic(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "runpublic/{*workflow}")]
        HttpRequestData request,
        string workflow,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(workflow))
        {
            var bad = request.CreateResponse(HttpStatusCode.BadRequest);
            await bad.WriteStringAsync(
                "Workflow name is required. Example: /api/runpublic/Hello%20World?Name=CallerTest",
                cancellationToken);
            return bad;
        }

        // The {*workflow} segment arrives URL-decoded; forward the raw inbound query string.
        var queryString = request.Url.Query; // includes leading '?', or empty
        _logger.LogInformation(
            "Proxying workflow '{Workflow}' (query '{Query}') to /public.", workflow, queryString);

        var result = await _engine
            .ExecutePublicAsync(workflow, queryString, cancellationToken)
            .ConfigureAwait(false);

        var response = request.CreateResponse((HttpStatusCode)result.StatusCode);
        response.Headers.Add("Content-Type", $"{result.ContentType}; charset=utf-8");
        await response.WriteStringAsync(result.Body, cancellationToken);
        return response;
    }

    /// <summary>
    /// Acquires the downstream engine's app-only Bearer token and returns it verbatim alongside
    /// its decoded header/claims (for inspection only — no signature validation is performed).
    /// Any failure (token acquisition or decoding) is handled centrally by
    /// <see cref="Middleware.ExceptionHandlingMiddleware"/>.
    /// </summary>
    [Function(nameof(GetInfo))]
    public async Task<HttpResponseData> GetInfo(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "info")]
        HttpRequestData request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("GetInfo: attempting token acquisition via DefaultAzureCredential.");

        var context = new TokenRequestContext([_options.EffectiveScope], tenantId: _options.TenantId);
        var token = await _credential.GetTokenAsync(context, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "GetInfo: token acquired successfully (expires {Expiry:u}).", token.ExpiresOn);

        var segments = token.Token.Split('.');
        var responseBody = new
        {
            rawToken = token.Token,
            tokenType = "Bearer",
            expiresOn = token.ExpiresOn,
            header = DecodeJwtSegment(segments[0]),
            claims = DecodeJwtSegment(segments[1])
        };

        var ok = request.CreateResponse(HttpStatusCode.OK);
        ok.Headers.Add("Content-Type", "application/json; charset=utf-8");
        await ok.WriteStringAsync(
            JsonSerializer.Serialize(responseBody, new JsonSerializerOptions { WriteIndented = true }),
            cancellationToken);
        return ok;
    }

    /// <summary>
    /// Decodes a single base64url-encoded JWT segment (header or payload) into a
    /// <see cref="JsonElement"/> for display. This performs no signature validation — it is for
    /// surfacing the contents of a token the caller already legitimately holds, never for
    /// authorization decisions.
    /// </summary>
    private static JsonElement DecodeJwtSegment(string base64UrlSegment)
    {
        var base64 = base64UrlSegment.Replace('-', '+').Replace('_', '/');
        var padding = (4 - base64.Length % 4) % 4;
        base64 = base64.PadRight(base64.Length + padding, '=');

        var bytes = Convert.FromBase64String(base64);
        using var document = JsonDocument.Parse(bytes);
        return document.RootElement.Clone();
    }
}
