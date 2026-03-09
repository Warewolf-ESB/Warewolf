using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Warewolf.Execution.AzureFunction.Lightweight
{
    /// <summary>
    /// Azure Function implementations mirroring the Warewolf WebServerController routes.
    ///
    /// Supported routes:
    ///   GET/POST  /Services/{name}          - Execute workflow (function-key auth)
    ///   GET/POST  /Services/{name}.debug     - Execute in debug mode
    ///   GET/POST  /Services/{name}.xml       - Execute and return XML output
    ///   GET/POST  /Services/{name}.api       - Return OpenAPI 3.0 spec for the workflow
    ///   GET/POST  /Secure/{name}             - Execute workflow (function-key auth)
    ///   GET/POST  /Secure/{name}.debug       - Execute in debug mode
    ///   GET/POST  /Secure/{name}.xml         - Execute and return XML output
    ///   GET/POST  /Secure/{name}.api         - Return OpenAPI 3.0 spec for the workflow
    ///   GET/POST  /Public/{name}             - Execute workflow (anonymous)
    ///   GET/POST  /Public/{name}.debug       - Execute in debug mode
    ///   GET/POST  /Public/{name}.xml         - Execute and return XML output
    ///   GET/POST  /Public/{name}.api         - Return OpenAPI 3.0 spec for the workflow
    ///   GET/POST  /workflow/{workflowName}   - Execute by name; supports .debug/.xml/.api suffixes
    ///   GET/POST  /workflow                  - Execute via query string or body
    ///
    /// Not supported in lightweight mode (require full Warewolf server):
    ///   apis.json, *.tests, *.tests.trx, *.coverage*, login, getlogfile
    ///
    /// Input parameters (any route):
    ///   Query string:  ?Name=John&amp;Age=30
    ///   JSON body:     { "inputParameters": { "Name": "John", "Age": "30" } }
    /// </summary>
    public sealed class WorkflowHttpFunction
    {
        const string JsonContentType = "application/json";
        readonly IWorkflowExecutor _workflowExecutor;
        readonly string _workflowsDirectory;

        public WorkflowHttpFunction(IWorkflowExecutor workflowExecutor)
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

            executionRequest.WebServerUri = req.Url;
            executionRequest.ReturnType   = isXml ? Dev2.Web.EmitionTypes.XML
                                          : isApi ? Dev2.Web.EmitionTypes.OPENAPI
                                                  : Dev2.Web.EmitionTypes.JSON;
            if (isDebug)
                executionRequest.IsDebug = true;

            var result = _workflowExecutor.Execute(executionRequest);
            return await CreateFormattedResponse(req, result);
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
            return await CreateFormattedResponse(req, result);
        }

        /// <summary>
        /// Shared handler: strips .api/.debug/.xml suffix, stamps the request with
        /// ReturnType + WebServerUri, then runs the workflow (or short-circuits for .api).
        /// </summary>
        async Task<HttpResponseData> ExecuteNamedWorkflow(HttpRequestData req, string name)
        {
            var (workflowName, isDebug, isXml, isApi) = ParseNameSuffixes(name);

            var executionRequest = await WorkflowFunctionHelper.ParseRequestAsync(req, _workflowsDirectory, workflowName);

            // Override what ParseRequestAsync inferred from the URL — the already-decoded
            // suffix flags are authoritative and avoid any ambiguity in path parsing.
            executionRequest.WebServerUri = req.Url;
            executionRequest.ReturnType   = isXml  ? Dev2.Web.EmitionTypes.XML
                                          : isApi  ? Dev2.Web.EmitionTypes.OPENAPI
                                                   : Dev2.Web.EmitionTypes.JSON;
            if (isDebug)
                executionRequest.IsDebug = true;

            var result = _workflowExecutor.Execute(executionRequest);
            return await CreateFormattedResponse(req, result);
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

        /// <summary>
        /// Single response builder — reads result.ContentType so the caller never needs to know
        /// which format was requested. Uses <see cref="Models.WorkflowExecutionResult.PayloadWriter"/>
        /// to stream the payload directly to <c>response.Body</c> via a <see cref="StreamWriter"/>,
        /// avoiding the full <c>byte[]</c> allocation that <c>WriteStringAsync</c> produces
        /// internally through <c>Encoding.UTF8.GetBytes</c>. Falls back to <c>WriteStringAsync</c>
        /// only for small error payloads stored in <see cref="Models.WorkflowExecutionResult.Payload"/>.
        /// </summary>
        static async Task<HttpResponseData> CreateFormattedResponse(HttpRequestData req, Models.WorkflowExecutionResult result)
        {
            var statusCode  = result.IsSuccess ? HttpStatusCode.OK : HttpStatusCode.InternalServerError;
            var response    = req.CreateResponse(statusCode);
            var contentType = result.ContentType ?? JsonContentType;
            response.Headers.Add("Content-Type", contentType);

            if (result.PayloadWriter != null)
                await result.PayloadWriter(response.Body, CancellationToken.None);
            else if (!string.IsNullOrEmpty(result.Payload))
                await response.WriteStringAsync(result.Payload);

            return response;
        }
    }
}
