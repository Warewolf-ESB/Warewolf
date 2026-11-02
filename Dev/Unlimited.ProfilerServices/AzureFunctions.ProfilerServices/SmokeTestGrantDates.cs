using AzureFunctions.ProfilerServices.Domain.Interfaces;
using AzureFunctions.ProfilerServices.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using System;
using System.Threading.Tasks;

namespace AzureFunctions.ProfilerServices
{
    // CD smoke test (SolutionItems/Scripts/Test-ProfilerEngineCaller.ps1): executes an engine workflow on demand and
    // returns the engine's answer, proving this app's identity can execute workflows on the engine.
    //   GET /api/smoke/grantdates                      the timer's own call (GrantDates)
    //   GET /api/smoke/grantdates?requestUrl=<path>    a workflow on the engine's secure/ or public/ route, in the
    //                                                  same form as the timer's _requestURL
    // Anonymous, so anyone who can reach this app can execute engine workflows with its identity: restrict access to
    // the app if that matters. It has no [OpenApiOperation], so it stays out of Swagger.json and APIM. GrantDates
    // sends the missing grant dates email when dates are missing, exactly as the Monday run does.
    public class SmokeTestGrantDates
    {
        private readonly IProfilerHttpService _profilerHttpService;

        public SmokeTestGrantDates(IProfilerHttpService profilerHttpService)
        {
            _profilerHttpService = profilerHttpService;
        }

        [Function("SmokeTestGrantDates")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "smoke/grantdates")] HttpRequest req)
        {
            string requestUrl = req.Query["requestUrl"];
            if (string.IsNullOrWhiteSpace(requestUrl))
                requestUrl = ExecuteRequestForMissingGrantDates._requestURL;

            if (!IsEngineRoute(requestUrl))
                return new BadRequestObjectResult(new { requestUrl, error = "requestUrl must be a workflow path starting with secure/ or public/, e.g. secure/Profiler/V2/Functions/ProfilerAlerts/GrantDates.json." });

            // The VM Warewolf server's 'Profiler/' prefix is added here, as the timer adds it.
            requestUrl = ExecuteRequestForMissingGrantDates.ToServerURL(requestUrl);

            // Caught here: ExceptionLoggingMiddleware swallows exceptions, which would turn a failure into an empty 200.
            try
            {
                using var response = await _profilerHttpService.GetAsync(requestUrl);
                var body = await response.Content.ReadAsStringAsync();
                return new ObjectResult(new { requestUrl, engineStatus = (int)response.StatusCode, engineBody = body })
                {
                    StatusCode = response.IsSuccessStatusCode ? StatusCodes.Status200OK : StatusCodes.Status502BadGateway
                };
            }
            catch (Exception ex)
            {
                return new ObjectResult(new { requestUrl, engineStatus = 0, error = $"{ex.GetType().Name}: {ex.Message}" })
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }

        // Only the engine's two workflow routes. ProfilerHttpService appends the path to GenericBaseURL and sends the
        // app's token with it, so the result must also stay on the engine's host (GenericBaseURL without its trailing
        // '/' would otherwise turn the path into part of the host name).
        private static bool IsEngineRoute(string requestUrl)
        {
            if (!(requestUrl.StartsWith("secure/", StringComparison.OrdinalIgnoreCase) || requestUrl.StartsWith("public/", StringComparison.OrdinalIgnoreCase))
                || requestUrl.Contains("..") || requestUrl.Contains("//") || requestUrl.Contains('\\'))
                return false;
            var serverUrl = GlobalVariables.genericBaseURL + ExecuteRequestForMissingGrantDates.ToServerURL(requestUrl);
            if (!Uri.TryCreate(GlobalVariables.genericBaseURL, UriKind.Absolute, out var baseUri)
                || !Uri.TryCreate(serverUrl, UriKind.Absolute, out var target))
                return false;
            return Uri.Compare(baseUri, target, UriComponents.SchemeAndServer, UriFormat.Unescaped, StringComparison.OrdinalIgnoreCase) == 0;
        }
    }
}
