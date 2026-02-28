using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Warewolf.Execution.AzureFunction.Lightweight
{
    /// <summary>
    /// Example Azure Function implementations showing how to use the lightweight
    /// <see cref="WorkflowExecutor"/> to run Warewolf workflows from workflow files.
    ///
    /// Usage examples:
    ///
    /// 1. Execute by workflow name via route (file resolved from configured directory):
    ///    POST /api/workflow/MyWorkflow
    ///    Body: { "inputParameters": { "Name": "John", "Age": "30" } }
    ///
    /// 2. Execute by workflow name via query string:
    ///    GET /api/workflow?workflowName=MyWorkflow&amp;Name=John&amp;Age=30
    ///
    /// 3. Execute with full file path via request body:
    ///    POST /api/workflow
    ///    Body: {
    ///      "workflowFilePath": "/workflows/MyWorkflow.xml",
    ///      "inputParameters": { "Name": "John", "Age": "30" }
    ///    }
    /// </summary>
    public class SampleWorkflowFunction
    {
        readonly IWorkflowExecutor _workflowExecutor;
        readonly string _workflowsDirectory;

        public SampleWorkflowFunction(IWorkflowExecutor workflowExecutor)
        {
            _workflowExecutor = workflowExecutor;
            _workflowsDirectory = Environment.GetEnvironmentVariable("WorkflowsDirectory") ?? "/workflows";
        }

        /// <summary>
        /// Execute a workflow by name from the route. The workflow file is resolved
        /// from the configured WorkflowsDirectory environment variable.
        /// </summary>
        [Function("ExecuteWorkflowByName")]
        public async Task<HttpResponseData> ExecuteByName(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "workflow/{workflowName}")] HttpRequestData req,
            string workflowName)
        {
            var executionRequest = await WorkflowFunctionHelper.ParseRequestAsync(req, _workflowsDirectory, workflowName);
            var result = _workflowExecutor.Execute(executionRequest);
            return await CreateResponse(req, result);
        }

        /// <summary>
        /// Execute a workflow identified via query string or request body.
        /// </summary>
        [Function("ExecuteWorkflow")]
        public async Task<HttpResponseData> Execute(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "workflow")] HttpRequestData req)
        {
            var executionRequest = await WorkflowFunctionHelper.ParseRequestAsync(req, _workflowsDirectory);

            if (!executionRequest.IsValid)
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync(JsonConvert.SerializeObject(new
                {
                    error = "WorkflowFilePath or WorkflowName must be provided via query string or request body."
                }));
                return badRequest;
            }

            var result = _workflowExecutor.Execute(executionRequest);
            return await CreateResponse(req, result);
        }

        static async Task<HttpResponseData> CreateResponse(HttpRequestData req, Models.WorkflowExecutionResult result)
        {
            var statusCode = result.IsSuccess ? HttpStatusCode.OK : HttpStatusCode.InternalServerError;
            var response = req.CreateResponse(statusCode);
            response.Headers.Add("Content-Type", "application/json");
            await response.WriteStringAsync(JsonConvert.SerializeObject(result, Formatting.Indented));
            return response;
        }
    }
}
