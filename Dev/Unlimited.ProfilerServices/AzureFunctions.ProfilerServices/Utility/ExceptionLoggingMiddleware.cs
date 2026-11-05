using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace AzureFunctions.ProfilerServices.Utility
{
    public class ExceptionLoggingMiddleware : IFunctionsWorkerMiddleware
    {
        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                var logger = context.GetLogger(context.FunctionDefinition.Name);
                logger.LogError("Unexpected Error in {0}: {1}", context.FunctionDefinition.Name, ex.Message);
            }
        }
    }
}
