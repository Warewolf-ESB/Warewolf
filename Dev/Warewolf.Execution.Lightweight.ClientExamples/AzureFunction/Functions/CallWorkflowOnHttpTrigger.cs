using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace WwExecutionCaller.Functions;

/// <summary>
/// HTTP-trigger that proxies an incoming request to a secure Warewolf Execution Engine workflow.
///
///   GET  http://localhost:7071/api/run/Hello%20World?Name=CallerTest
///        → engine GET /secure/Hello%20World.json?Name=CallerTest
///
/// The catch-all <c>{*workflow}</c> route segment captures the (possibly slash-containing)
/// workflow name; the inbound query string is forwarded verbatim. The downstream Bearer token
/// is attached automatically by <see cref="Auth.WwExecutionTokenHandler"/>.
/// </summary>
public sealed class CallWorkflowOnHttpTrigger
{
    private readonly IWwExecutionDownstreamService _engine;
    private readonly ILogger<CallWorkflowOnHttpTrigger> _logger;

    public CallWorkflowOnHttpTrigger(
        IWwExecutionDownstreamService engine,
        ILogger<CallWorkflowOnHttpTrigger> logger)
    {
        _engine = engine;
        _logger = logger;
    }

    [Function(nameof(CallWorkflowOnHttpTrigger))]
    public async Task<HttpResponseData> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "run/{*workflow}")]
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
}
