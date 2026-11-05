using AzureFunctions.ProfilerServices.Domain.Interfaces;
using AzureFunctions.ProfilerServices.Utility;
using Microsoft.Azure.Functions.Worker;
using System.Threading.Tasks;

namespace AzureFunctions.ProfilerServices
{
    public class ExecuteRequestForMissingGrantDates
    {
        const string _cronExpression = "0 0 8 * * MON";
        private readonly IProfilerHttpService _profilerHttpService;

        internal const string _requestURL = "secure/Profiler/V2/Functions/ProfilerAlerts/GrantDates.json";
        // The VM Warewolf server serves the same routes under 'Profiler/'.
        private const string _vmPrefix = "Profiler/";

        // Also used by SmokeTestGrantDates, so a smoke test reaches an engine route exactly as the timer does.
        internal static string ToServerURL(string requestUrl) =>
            GlobalVariables.usesEngineIdentity ? requestUrl : _vmPrefix + requestUrl;

        public ExecuteRequestForMissingGrantDates(IProfilerHttpService profilerHttpService)
        {
            _profilerHttpService = profilerHttpService;
        }

        [Function("ExecuteRequestForMissingGrantDates")]
        public async Task Run([TimerTrigger(_cronExpression)] TimerInfo myTimer, FunctionContext context)
        {
            // Awaited, so the call completes inside the invocation and an engine failure surfaces as an error.
            using var response = await _profilerHttpService.GetAsync(ToServerURL(_requestURL));
            response.EnsureSuccessStatusCode();
        }
    }
}
