using Dev2.Web;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Warewolf.Execution.Lightweight
{
    /// <summary>
    /// Azure Function entry points mirroring the Warewolf WebServerController routes.
    /// Each public method is a thin wrapper: it delegates suffix parsing to
    /// <see cref="NameSuffixParser"/>, request building to <see cref="WorkflowFunctionHelper"/>,
    /// execution to <see cref="IWorkflowExecutor"/>, apis.json listing to
    /// <see cref="IApisJsonGenerator"/>, and response assembly to <see cref="ResponseBuilder"/>.
    ///
    /// Supported routes:
    ///   GET/POST  /Services/{name}           - Execute workflow (function-key auth)
    ///   GET/POST  /Services/{name}.debug      - Execute in debug mode
    ///   GET/POST  /Services/{name}.xml        - Execute and return XML output
    ///   GET/POST  /Services/{name}.api        - Return OpenAPI 3.0 spec for the workflow
    ///   GET/POST  /Services/{folder}/apis.json - List workflows under folder (authenticated)
    ///   GET/POST  /Secure/{name}              - Execute workflow (function-key auth)
    ///   GET/POST  /Secure/{folder}/apis.json  - List workflows under folder (authenticated)
    ///   GET/POST  /Public/{name}              - Execute workflow (anonymous)
    ///   GET/POST  /Public/{folder}/apis.json  - List workflows under folder (anonymous)
    ///   GET       /apis.json                  - List all workflows (anonymous, root discovery)
    ///   GET/POST  /workflow/{workflowName}    - Execute by name; supports .debug/.xml/.api suffixes
    ///   GET/POST  /workflow                   - Execute via query string or body
    ///
    /// Not supported in lightweight mode (require full Warewolf server):
    ///   *.tests, *.tests.trx, *.coverage*, login, getlogfile
    ///
    /// Input parameters (any route):
    ///   Query string:  ?Name=John&amp;Age=30
    ///   JSON body:     { "inputParameters": { "Name": "John", "Age": "30" } }
    /// </summary>
    public sealed class WorkflowHttpFunction
    {
        readonly IWorkflowExecutor   _workflowExecutor;
        readonly IApisJsonGenerator  _apisJsonGenerator;
        readonly string              _workflowsDirectory;

        public WorkflowHttpFunction(IWorkflowExecutor workflowExecutor, IApisJsonGenerator apisJsonGenerator)
        {
            _workflowExecutor   = workflowExecutor;
            _apisJsonGenerator  = apisJsonGenerator;
            _workflowsDirectory = Environment.GetEnvironmentVariable("WorkflowsDirectory")
                ?? Path.Combine(AppContext.BaseDirectory, "Resources");
        }

        // ── Authenticated workflow routes ─────────────────────────────────────

        /// <summary>Mirrors Services/{*name} — function-key authenticated execution.</summary>
        [Function("ExecuteService")]
        public async Task<HttpResponseData> ExecuteService(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "Services/{*name}")] HttpRequestData req,
            string name)
            => await ExecuteNamedWorkflow(req, name, isPublic: false);

        /// <summary>Mirrors Secure/{*name} — function-key authenticated execution.</summary>
        [Function("ExecuteSecureWorkflow")]
        public async Task<HttpResponseData> ExecuteSecureWorkflow(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "Secure/{*name}")] HttpRequestData req,
            string name)
            => await ExecuteNamedWorkflow(req, name, isPublic: false);

        // ── Anonymous / public route ──────────────────────────────────────────

        /// <summary>Mirrors Public/{*name} — anonymous (unauthenticated) execution.</summary>
        [Function("ExecutePublicWorkflow")]
        public async Task<HttpResponseData> ExecutePublicWorkflow(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "Public/{*name}")] HttpRequestData req,
            string name)
            => await ExecuteNamedWorkflow(req, name, isPublic: true);

        // ── apis.json discovery routes ────────────────────────────────────────

        /// <summary>
        /// Root-level apis.json — lists all available workflows.
        /// Mirrors <c>WebServerController.ExecuteGetRootLevelApisJson</c>.
        /// Anonymous so that API discovery tools (Postman, etc.) can reach it without a key.
        /// </summary>
        [Function("ExecuteRootApisJson")]
        public async Task<HttpResponseData> ExecuteRootApisJson(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "apis.json")] HttpRequestData req)
            => await CreateApisJsonResponse(req, pathFilter: null, isPublic: true);

        // ── Named-workflow routes ─────────────────────────────────────────────

        /// <summary>
        /// Execute a workflow by name from the route.
        /// Mirrors <c>WebServerController.ExecuteService</c> + suffix handling.
        /// </summary>
        [Function("ExecuteWorkflowByName")]
        public async Task<HttpResponseData> ExecuteByName(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "workflow/{workflowName}")] HttpRequestData req,
            string workflowName)
        {
            var (resolvedName, isDebug, isXml, isApi) = NameSuffixParser.Parse(workflowName);
            var executionRequest = await WorkflowFunctionHelper.ParseRequestAsync(req, _workflowsDirectory, resolvedName);

            executionRequest.WebServerUri = req.Url;
            executionRequest.ReturnType   = isXml ? EmitionTypes.XML
                                          : isApi ? EmitionTypes.OPENAPI
                                                  : EmitionTypes.JSON;
            if (isDebug)
                executionRequest.IsDebug = true;

            var result = _workflowExecutor.Execute(executionRequest);
            return await ResponseBuilder.BuildAsync(req, result,
                isXml ? ResponseBuilder.XmlContentType : ResponseBuilder.JsonContentType);
        }

        /// <summary>
        /// Execute a workflow identified via query string or request body.
        /// Mirrors <c>WebServerController.ExecuteService</c> (generic path).
        /// </summary>
        [Function("ExecuteWorkflow")]
        public async Task<HttpResponseData> Execute(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "workflow")] HttpRequestData req)
        {
            var executionRequest = await WorkflowFunctionHelper.ParseRequestAsync(req, _workflowsDirectory);

            if (!executionRequest.IsValid)
            {
                return await ResponseBuilder.BuildStringAsync(req,
                    JsonConvert.SerializeObject(new
                    {
                        error = "WorkflowFilePath or WorkflowName must be provided via query string or request body."
                    }),
                    statusCode: HttpStatusCode.BadRequest);
            }

            var result = _workflowExecutor.Execute(executionRequest);
            return await ResponseBuilder.BuildAsync(req, result);
        }

        // ── Shared private helpers ────────────────────────────────────────────

        /// <summary>
        /// Handles any named route: short-circuits to apis.json listing when the
        /// route ends with <c>apis.json</c>; otherwise parses suffix flags, builds
        /// the execution request, runs the workflow, and returns the formatted response.
        /// </summary>
        async Task<HttpResponseData> ExecuteNamedWorkflow(HttpRequestData req, string name, bool isPublic)
        {
            if (NameSuffixParser.IsApisJsonRequest(name))
                return await CreateApisJsonResponse(req, NameSuffixParser.ExtractApisJsonPath(name), isPublic);

            var (workflowName, isDebug, isXml, isApi) = NameSuffixParser.Parse(name);
            var executionRequest = await WorkflowFunctionHelper.ParseRequestAsync(req, _workflowsDirectory, workflowName);

            // Suffix flags are authoritative — override any format inferred from the URL path.
            executionRequest.WebServerUri = req.Url;
            executionRequest.ReturnType   = isXml ? EmitionTypes.XML
                                          : isApi ? EmitionTypes.OPENAPI
                                                  : EmitionTypes.JSON;
            if (isDebug)
                executionRequest.IsDebug = true;

            var result = _workflowExecutor.Execute(executionRequest);
            return await ResponseBuilder.BuildAsync(req, result,
                isXml ? ResponseBuilder.XmlContentType : ResponseBuilder.JsonContentType);
        }

        /// <summary>
        /// Generates and returns an apis.json discovery document for the given path.
        /// </summary>
        async Task<HttpResponseData> CreateApisJsonResponse(HttpRequestData req, string? pathFilter, bool isPublic)
        {
            var json = _apisJsonGenerator.Generate(pathFilter, req.Url, isPublic);
            return await ResponseBuilder.BuildStringAsync(req, json, ResponseBuilder.JsonContentType);
        }
    }
}
