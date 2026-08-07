/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json.Linq;
using System.Net;
using Warewolf.Execution.Lightweight.Auth;
using Warewolf.Execution.Lightweight.Auth.Models;
using Warewolf.Execution.Lightweight.Models;
using Warewolf.Execution.Lightweight.Security;

namespace Warewolf.Execution.Lightweight.Functions;

/// <summary>
/// GET <c>/secure/servicebus-result/{correlationId}</c> — the polling endpoint for the
/// secure Service Bus workflow trigger's async response path (resolved plan decision #1:
/// polling only, no push/webhook delivery in this pass).
///
/// <para><b>Auth.</b> Lives under the <c>/secure</c> prefix so the full HTTP middleware
/// pipeline applies (EasyAuth → claims → policy) — the SAME
/// <see cref="Auth.IWorkflowPolicyMatcher"/> singleton the Service Bus trigger itself uses
/// for message-level authorization. No secure.config resource entry exists for this
/// pseudo route, so (mirroring <c>WorkflowResumeFunction</c>) the GLOBAL role map decides:
/// grant the polling caller's role a global (<c>IsServer=true</c>) <see cref="WorkflowPermission.View"/>
/// row in <c>secure.config</c>. Denials are wrapped as HTTP 500 (WOLF-8418, unchanged by
/// this work).</para>
///
/// <para>
/// Results are only available when <see cref="IServiceBusReplayAndResultStore"/> has
/// recorded a terminal outcome for the requested correlation id (see
/// <see cref="ServiceBusWorkflowTriggerFunction"/>) — 404 before that, or always when
/// persistence is disabled and a different engine instance processed the message (see the
/// in-memory fallback caveat documented on <see cref="ServiceBusReplayAndResultStore"/>).
/// </para>
/// </summary>
public sealed class ServiceBusResultFunction
{
    private readonly IServiceBusReplayAndResultStore _store;

    public ServiceBusResultFunction(IServiceBusReplayAndResultStore store)
        => _store = store;

    [Function("ServiceBusResult")]
    [RequireWorkflowPermission(WorkflowPermission.View)]
    public async Task<HttpResponseData> GetResult(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Secure/servicebus-result/{correlationId}")] HttpRequestData req,
        string correlationId,
        FunctionContext context)
    {
        if (!_store.TryGetResult(correlationId, out var result) || result is null)
        {
            return await JsonResponse(req, HttpStatusCode.NotFound, new JObject
            {
                ["correlationId"] = correlationId,
                ["error"] = "not_found",
                ["message"] = "No result recorded for this correlation id — the message may not have been " +
                              "processed yet, the id may be wrong, or (with persistence disabled) it was " +
                              "processed by a different engine instance.",
            });
        }

        var body = new JObject
        {
            ["correlationId"]  = result.CorrelationId,
            ["status"]         = result.Status.ToString(),
            ["workflow"]       = result.Workflow,
            ["caller"]         = result.Caller,
            ["receivedAtUtc"]  = result.ReceivedAtUtc,
            ["completedAtUtc"] = result.CompletedAtUtc,
        };

        if (result.Status == ServiceBusTriggerStatus.Succeeded && !string.IsNullOrEmpty(result.Outputs))
        {
            body["outputs"] = TryParse(result.Outputs);
        }
        else if (!string.IsNullOrEmpty(result.Error))
        {
            body["error"] = result.Error;
        }

        return await JsonResponse(req, HttpStatusCode.OK, body);
    }

    static JToken TryParse(string outputs)
    {
        try { return JToken.Parse(outputs); }
        catch { return outputs; }
    }

    static async Task<HttpResponseData> JsonResponse(HttpRequestData req, HttpStatusCode status, JObject body)
    {
        var response = req.CreateResponse(status);
        response.Headers.Add("Content-Type", "application/json; charset=utf-8");
        await response.WriteStringAsync(body.ToString());
        return response;
    }
}
