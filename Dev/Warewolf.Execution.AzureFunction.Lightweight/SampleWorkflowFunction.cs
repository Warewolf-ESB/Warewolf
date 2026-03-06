using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Warewolf.Execution.AzureFunction.Lightweight
{
    /// <summary>
    /// Azure Function implementations mirroring the Warewolf WebServerController routes.
    ///
    /// Supported routes:
    ///   GET/POST  /api/Services/{name}          - Execute workflow (function-key auth)
    ///   GET/POST  /api/Services/{name}.debug     - Execute in debug mode
    ///   GET/POST  /api/Services/{name}.xml       - Execute and return XML output
    ///   GET/POST  /api/Services/{name}.api       - Return OpenAPI 3.0 spec for the workflow
    ///   GET/POST  /api/Secure/{name}             - Execute workflow (function-key auth)
    ///   GET/POST  /api/Secure/{name}.debug       - Execute in debug mode
    ///   GET/POST  /api/Secure/{name}.xml         - Execute and return XML output
    ///   GET/POST  /api/Secure/{name}.api         - Return OpenAPI 3.0 spec for the workflow
    ///   GET/POST  /api/Public/{name}             - Execute workflow (anonymous)
    ///   GET/POST  /api/Public/{name}.debug       - Execute in debug mode
    ///   GET/POST  /api/Public/{name}.xml         - Execute and return XML output
    ///   GET/POST  /api/Public/{name}.api         - Return OpenAPI 3.0 spec for the workflow
    ///   GET/POST  /api/workflow/{workflowName}   - Execute by name; supports .debug/.xml/.api suffixes
    ///   GET/POST  /api/workflow                  - Execute via query string or body
    ///
    /// Not supported in lightweight mode (require full Warewolf server):
    ///   apis.json, *.tests, *.tests.trx, *.coverage*, login, getlogfile
    ///
    /// Input parameters (any route):
    ///   Query string:  ?Name=John&amp;Age=30
    ///   JSON body:     { "inputParameters": { "Name": "John", "Age": "30" } }
    /// </summary>
    public sealed class SampleWorkflowFunction
    {
        const string JsonContentType = "application/json";
        const string XmlContentType  = "text/xml";
        readonly IWorkflowExecutor _workflowExecutor;
        readonly string _workflowsDirectory;

        public SampleWorkflowFunction(IWorkflowExecutor workflowExecutor)
        {
            _workflowExecutor = workflowExecutor;
            _workflowsDirectory = Environment.GetEnvironmentVariable("WorkflowsDirectory")
                ?? Path.Combine(AppContext.BaseDirectory, "Resources");
        }

        /// <summary>
        /// Mirrors Services/{*name} — authenticated workflow execution.
        /// Append .debug to the workflow name to enable debug mode.
        /// </summary>
        [Function("ExecuteService")]
        public async Task<HttpResponseData> ExecuteService(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "Services/{*name}")] HttpRequestData req,
            string name)
            => await ExecuteNamedWorkflow(req, name);

        /// <summary>
        /// Mirrors Secure/{*name} — authenticated workflow execution.
        /// Append .debug to the workflow name to enable debug mode.
        /// </summary>
        [Function("ExecuteSecureWorkflow")]
        public async Task<HttpResponseData> ExecuteSecureWorkflow(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "Secure/{*name}")] HttpRequestData req,
            string name)
            => await ExecuteNamedWorkflow(req, name);

        /// <summary>
        /// Mirrors Public/{*name} — anonymous (unauthenticated) workflow execution.
        /// Append .debug to the workflow name to enable debug mode.
        /// </summary>
        [Function("ExecutePublicWorkflow")]
        public async Task<HttpResponseData> ExecutePublicWorkflow(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "Public/{*name}")] HttpRequestData req,
            string name)
            => await ExecuteNamedWorkflow(req, name);

        /// <summary>
        /// Execute a workflow by name from the route. The workflow file is resolved
        /// from the configured WorkflowsDirectory environment variable.
        /// Supports .debug and .xml suffixes on the workflow name.
        /// </summary>
        [Function("ExecuteWorkflowByName")]
        public async Task<HttpResponseData> ExecuteByName(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "workflow/{workflowName}")] HttpRequestData req,
            string workflowName)
        {
            var (resolvedName, isDebug, isXml, isApi) = ParseNameSuffixes(workflowName);
            var executionRequest = await WorkflowFunctionHelper.ParseRequestAsync(req, _workflowsDirectory, resolvedName);
            if (isApi)
            {
                return await CreateApiSpecResponse(req, resolvedName, executionRequest.WorkflowFilePath);
            }
            if (isDebug)
            {
                executionRequest.IsDebug = true;
            }
            var result = _workflowExecutor.Execute(executionRequest);
            return isXml ? await CreateXmlResponse(req, result) : await CreateResponse(req, result);
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

        /// <summary>
        /// Shared handler: strips .api/.debug/.xml suffix, builds the execution request, and runs the workflow.
        /// </summary>
        async Task<HttpResponseData> ExecuteNamedWorkflow(HttpRequestData req, string name)
        {
            var (workflowName, isDebug, isXml, isApi) = ParseNameSuffixes(name);

            var executionRequest = await WorkflowFunctionHelper.ParseRequestAsync(req, _workflowsDirectory, workflowName);
            if (isApi)
            {
                return await CreateApiSpecResponse(req, workflowName, executionRequest.WorkflowFilePath);
            }
            if (isDebug)
            {
                executionRequest.IsDebug = true;
            }

            var result = _workflowExecutor.Execute(executionRequest);
            return isXml ? await CreateXmlResponse(req, result) : await CreateResponse(req, result);
        }

        /// <summary>
        /// Strips known suffixes (.api, .debug, .xml) from a workflow name and returns the resolved parts.
        /// Suffix precedence matches WebServerController: .api checked first, then .debug, then .xml.
        /// </summary>
        static (string name, bool isDebug, bool isXml, bool isApi) ParseNameSuffixes(string name)
        {
            if (name.EndsWith(".api", StringComparison.OrdinalIgnoreCase))
                return (name[..^".api".Length], false, false, true);
            if (name.EndsWith(".debug", StringComparison.OrdinalIgnoreCase))
                return (name[..^".debug".Length], true, false, false);
            if (name.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                return (name[..^".xml".Length], false, true, false);
            if (name.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                return (name[..^".json".Length], false, false, false);
            return (name, false, false, false);
        }

        static async Task<HttpResponseData> CreateApiSpecResponse(HttpRequestData req, string workflowName, string workflowFilePath)
        {
            if (string.IsNullOrWhiteSpace(workflowFilePath) || !File.Exists(workflowFilePath))
            {
                var notFound = req.CreateResponse(HttpStatusCode.NotFound);
                    notFound.Headers.Add("Content-Type", JsonContentType);
                    await notFound.WriteStringAsync(JsonConvert.SerializeObject(new { error = $"Workflow not found: {workflowName}" }));
                    return notFound;
                }

                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", JsonContentType);
            await response.WriteStringAsync(WorkflowOpenApiGenerator.Generate(workflowFilePath, workflowName, req.Url));
            return response;
        }

        static async Task<HttpResponseData> CreateResponse(HttpRequestData req, Models.WorkflowExecutionResult result)
        {
            var statusCode = result.IsSuccess ? HttpStatusCode.OK : HttpStatusCode.InternalServerError;
            var response = req.CreateResponse(statusCode);
            response.Headers.Add("Content-Type", JsonContentType);
            await response.WriteStringAsync(JsonConvert.SerializeObject(result, Formatting.Indented));
            return response;
        }

        static async Task<HttpResponseData> CreateXmlResponse(HttpRequestData req, Models.WorkflowExecutionResult result)
        {
            var statusCode = result.IsSuccess ? HttpStatusCode.OK : HttpStatusCode.InternalServerError;
            var response = req.CreateResponse(statusCode);
            response.Headers.Add("Content-Type", XmlContentType);
            // result.OutputXml is populated by WorkflowExecutor.TryExtractXmlOutput via
            // ExecutionEnvironmentUtils.GetXmlOutputFromEnvironment — the same path the full
            // Warewolf server uses. Falling back to an empty DataList if unavailable.
            await response.WriteStringAsync(result.OutputXml ?? "<DataList />");
            return response;
        }
    }
}
