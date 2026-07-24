/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using Dev2.Common;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Net;
using Warewolf.Execution.Lightweight.Auth;
using Warewolf.Execution.Lightweight.Auth.Models;

namespace Warewolf.Execution.Lightweight.Functions;

/// <summary>
/// POST <c>/secure/resume/{suspensionId}</c> — the Execution Engine's resume route,
/// dispatch target of the ExecutionEngineJobProcessor and open to role-gated
/// human/manual callers (resolved plan decision #3).
///
/// <para><b>Auth.</b> Lives under the <c>/secure</c> prefix so the full middleware
/// pipeline applies (EasyAuth → claims → policy). The policy middleware evaluates the
/// path's pseudo workflow name (<c>resume/&lt;jobId&gt;</c>); no per-workflow resource
/// entry exists for it, so the GLOBAL role map decides — grant the caller's role
/// (e.g. <c>Warewolf_JobProcessor</c>) a global (<c>IsServer=true</c>) <c>Execute</c>
/// row in <c>secure.config</c>. Denials are wrapped as HTTP 500 (WOLF-8418).</para>
///
/// <para><b>Flow.</b> Claim (atomic CAS Scheduled→Processing; the single
/// duplicate-prevention point — losers get 409) → execute the continuation
/// SYNCHRONOUSLY → record <c>Succeeded</c>/<c>Failed</c> → respond. The response is
/// returned on completion (isolated-worker HTTP cannot emit an early 202 and keep
/// executing; a caller disconnect does NOT abort execution — the invocation runs to
/// functionTimeout, and the reaper fails anything the host kills). The processor
/// treats its short ack timeout as "reconcile next tick", not as failure.</para>
///
/// <para>Responses: 200 <c>{suspensionId,state:"Succeeded",outputs}</c> ·
/// 404 unknown job · 409 <c>{state}</c> not claimable · 500 execution failed
/// (job marked <c>Failed</c>, fail-only) · 503 persistence disabled.</para>
/// </summary>
public sealed class WorkflowResumeFunction
{
    readonly ResumptionExecutor _resumption;
    readonly ILogger<WorkflowResumeFunction> _logger;

    public WorkflowResumeFunction(ResumptionExecutor resumption, ILogger<WorkflowResumeFunction> logger)
    {
        _resumption = resumption;
        _logger = logger;
    }

    [Function("WorkflowResume")]
    [RequireWorkflowPermission(WorkflowPermission.Execute)]
    public async Task<HttpResponseData> Resume(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Secure/resume/{suspensionId}")] HttpRequestData req,
        string suspensionId,
        FunctionContext context)
    {
        if (!Config.Persistence.Enable)
        {
            _logger.LogWarning("Resume | JobId={JobId} | Rejected — persistence is disabled on this engine.", suspensionId);
            return await JsonResponse(req, HttpStatusCode.ServiceUnavailable, new JObject
            {
                ["error"] = "persistence_disabled",
                ["message"] = "Suspend/resume persistence is not enabled on this Execution Engine.",
            });
        }

        var claim = _resumption.TryClaim(suspensionId);
        switch (claim.Outcome)
        {
            case ResumeClaimOutcome.NotFound:
                _logger.LogWarning("Resume | JobId={JobId} | Not found.", suspensionId);
                return await JsonResponse(req, HttpStatusCode.NotFound, new JObject
                {
                    ["suspensionId"] = suspensionId,
                    ["error"] = "not_found",
                    ["message"] = "No suspension job with this id exists in the persistence store.",
                });

            case ResumeClaimOutcome.Conflict:
                _logger.LogInformation(
                    "Resume | JobId={JobId} | Claim lost — current state {State}. Benign for duplicate dispatches.",
                    suspensionId, claim.CurrentState ?? "(unknown)");
                return await JsonResponse(req, HttpStatusCode.Conflict, new JObject
                {
                    ["suspensionId"] = suspensionId,
                    ["error"] = "not_claimable",
                    ["state"] = claim.CurrentState,
                    ["message"] = "The job is not in Scheduled state — it is already running, finished, failed, or was manually resumed.",
                });
        }

        var result = _resumption.ExecuteClaimed(suspensionId);
        if (result.Success)
        {
            var body = new JObject
            {
                ["suspensionId"] = suspensionId,
                ["state"] = "Succeeded",
                ["durationMs"] = result.DurationMilliseconds,
            };
            if (!string.IsNullOrEmpty(result.Outputs))
            {
                body["outputs"] = TryParse(result.Outputs);
            }

            return await JsonResponse(req, HttpStatusCode.OK, body);
        }

        return await JsonResponse(req, HttpStatusCode.InternalServerError, new JObject
        {
            ["suspensionId"] = suspensionId,
            ["state"] = "Failed",
            ["error"] = "resume_execution_failed",
            ["message"] = result.Error,
            ["durationMs"] = result.DurationMilliseconds,
        });
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
