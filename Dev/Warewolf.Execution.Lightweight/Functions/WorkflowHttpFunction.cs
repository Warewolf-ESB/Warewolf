using Dev2.Web;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Warewolf.Execution.Lightweight.Security;

namespace Warewolf.Execution.Lightweight
{
    /// <summary>
    /// Azure Function entry points mirroring the Warewolf WebServerController routes.
    /// Each public method is a thin wrapper: it delegates suffix parsing to
    /// <see cref="NameSuffixParser"/>, request building to <see cref="WorkflowFunctionHelper"/>,
    /// execution to <see cref="IWorkflowExecutor"/>, apis.json listing to
    /// <see cref="IApisJsonGenerator"/>, and response assembly to <see cref="ResponseBuilder"/>.
    ///
    /// ## Authentication &amp; authorisation
    ///
    /// Security is driven by the presence of a <c>secure.config</c> file in the server
    /// bin directory (resolved at startup by <see cref="SecureConfigLoader"/>):
    ///
    /// <list type="bullet">
    ///   <item>
    ///     <b>No secure.config</b> — open-access mode.  All workflows are reachable
    ///     via the <c>/Public/</c> endpoint.  <c>/Secure/</c> routes return <c>401</c>
    ///     because there is no secret key with which to validate a JWT.
    ///   </item>
    ///   <item>
    ///     <b>secure.config present</b> — JWT mode.  Callers of <c>/Secure/</c> routes
    ///     must supply a valid <c>Authorization: Bearer &lt;token&gt;</c> header.  The
    ///     token is validated with HMAC-SHA256 using the secret key from the config file
    ///     by <see cref="JwtValidator"/>.
    ///   </item>
    /// </list>
    ///
    /// ## apis.json visibility
    ///
    /// Both <c>/Public/apis.json</c> and <c>/Secure/apis.json</c> (and the root
    /// <c>/apis.json</c>) are always reachable without a 401 — they only differ in what
    /// they return:
    ///
    /// <list type="bullet">
    ///   <item><c>/Public/…/apis.json</c> / root <c>/apis.json</c> — workflows where the
    ///         built-in <em>Public</em> group has View permission (all when no config).</item>
    ///   <item><c>/Secure/…/apis.json</c> — workflows the JWT user can view (empty list
    ///         when the token is absent or invalid).</item>
    /// </list>
    ///
    /// Supported routes:
    ///   GET/POST  /Services/{name}           - Execute workflow (function-key auth)
    ///   GET/POST  /Services/{name}.debug      - Execute in debug mode
    ///   GET/POST  /Services/{name}.xml        - Execute and return XML output
    ///   GET/POST  /Services/{name}.api        - Return OpenAPI 3.0 spec for the workflow
    ///   GET/POST  /Services/{folder}/apis.json - List workflows under folder (authenticated)
    ///   GET/POST  /Secure/{name}              - Execute workflow (JWT auth)
    ///   GET/POST  /Secure/{folder}/apis.json  - List workflows the JWT user can access
    ///   GET/POST  /Public/{name}              - Execute workflow (anonymous)
    ///   GET/POST  /Public/{folder}/apis.json  - List publicly visible workflows
    ///   GET       /apis.json                  - List all publicly visible workflows (root discovery)
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

        /// <summary>
        /// Mirrors Secure/{*name} — JWT-authenticated execution.
        /// The Azure Function authorization level is Anonymous so the function host does
        /// not reject the request before we can validate the JWT ourselves.
        /// Auth is handled by the EasyAuth + WorkflowAuthorization middleware pipeline;
        /// FunctionContext is passed so the principal built by middleware can be reused.
        /// </summary>
        [Function("ExecuteSecureWorkflow")]
        public async Task<HttpResponseData> ExecuteSecureWorkflow(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "Secure/{*name}")] HttpRequestData req,
            string name,
            FunctionContext context)
            => await ExecuteNamedWorkflow(req, name, isPublic: false, context);

        // ── Anonymous / public route ──────────────────────────────────────────

        /// <summary>Mirrors Public/{*name} — anonymous (unauthenticated) execution.</summary>
        [Function("ExecutePublicWorkflow")]
        public async Task<HttpResponseData> ExecutePublicWorkflow(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "Public/{*name}")] HttpRequestData req,
            string name)
            => await ExecuteNamedWorkflow(req, name, isPublic: true);

        // ── apis.json discovery routes ────────────────────────────────────────

        /// <summary>
        /// Root-level apis.json — lists all publicly visible workflows.
        /// Mirrors <c>WebServerController.ExecuteGetRootLevelApisJson</c>.
        /// Always accessible; filtered by public-view permissions when a
        /// <c>secure.config</c> is present.
        /// </summary>
        [Function("ExecuteRootApisJson")]
        public async Task<HttpResponseData> ExecuteRootApisJson(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "apis.json")] HttpRequestData req)
            => await CreateApisJsonResponse(req, pathFilter: null, isPublic: true, GetPublicFilter());

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
        /// Core dispatch method shared by all named-workflow routes.
        ///
        /// <list type="bullet">
        ///   <item>apis.json routes → permission-filtered discovery, never returns 401.</item>
        ///   <item>Secure execution routes → JWT validation; returns 401 when the token is
        ///         absent, invalid, or no <c>secure.config</c> exists.</item>
        ///   <item>Public execution routes → no auth check; executes unconditionally.</item>
        /// </list>
        /// </summary>
        async Task<HttpResponseData> ExecuteNamedWorkflow(
            HttpRequestData req,
            string name,
            bool isPublic,
            FunctionContext? context = null)
        {
            // ── apis.json: always accessible, filtered by permissions ─────────────
            if (NameSuffixParser.IsApisJsonRequest(name))
            {
                var pathFilter   = NameSuffixParser.ExtractApisJsonPath(name);
                var permFilter   = isPublic ? GetPublicFilter() : GetSecureFilter(req, context);
                return await CreateApisJsonResponse(req, pathFilter, isPublic, permFilter);
            }

            // ── Secure (non-public) execution ─────────────────────────────────────
            if (!isPublic)
            {
                // Prefer the principal already built by the middleware pipeline (Secure/* routes).
                // Fall back to direct JWT validation for Services/* routes that bypass middleware.
                var principalAuthenticated =
                    context is not null &&
                    context.Items.TryGetValue(Auth.Models.AuthConstants.PrincipalContextKey, out var p) &&
                    p is Auth.WorkflowClaimsPrincipal wcp &&
                    wcp.Identity?.IsAuthenticated == true;

                if (!principalAuthenticated)
                {
                    var authResult = ValidateJwt(req);
                    if (!authResult.IsValid)
                        return await BuildUnauthorizedResponse(req);
                }
            }

            // ── Execute the workflow ──────────────────────────────────────────────
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
        /// Generates and returns an apis.json discovery document for the given path,
        /// with an optional per-workflow permission predicate applied.
        /// </summary>
        async Task<HttpResponseData> CreateApisJsonResponse(
            HttpRequestData    req,
            string?            pathFilter,
            bool               isPublic,
            Func<string, bool>? workflowFilter)
        {
            var json = _apisJsonGenerator.Generate(pathFilter, req.Url, isPublic, workflowFilter);
            return await ResponseBuilder.BuildStringAsync(req, json, ResponseBuilder.JsonContentType);
        }

        // ── JWT helpers ───────────────────────────────────────────────────────────

        /// <summary>
        /// Validates the JWT in the <c>Authorization</c> header.
        /// Tries the Warewolf HMAC-SHA256 token first; falls back to a Microsoft Entra
        /// Bearer token when the Warewolf validation fails.
        /// Returns <c>(false, null)</c> when no <c>secure.config</c> is loaded (no secret
        /// key exists) or when neither token variant is valid.
        /// </summary>
        (bool IsValid, IReadOnlyList<string>? Groups) ValidateJwt(HttpRequestData req)
        {
            var config = SecureConfigLoader.Config;
            if (!config.IsLoaded)
                return (false, null);   // No config → no secret key → cannot validate any JWT.

            var authHeader = TryGetAuthHeader(req);

            // ── 1. Warewolf HMAC-SHA256 JWT ───────────────────────────────────────
            var groups = JwtValidator.GetUserGroups(authHeader, config.SecretKey);
            if (groups is not null)
                return (true, groups);

            // ── 2. Microsoft Entra OAuth token ────────────────────────────────────
            var easyAuthHeader = TryGetEasyAuthPrincipalHeader(req);
            var entraRoles = EntraTokenValidator.GetRoles(
                authHeader, easyAuthHeader, config.EntraTenantId, config.EntraAudience);
            return (entraRoles is not null, entraRoles);
        }

        // ── Permission filter factories ───────────────────────────────────────────

        /// <summary>
        /// Returns a predicate that passes workflows visible on the public endpoint.
        /// When no <c>secure.config</c> is loaded, <c>null</c> is returned so that
        /// <see cref="IApisJsonGenerator.Generate"/> emits all workflows.
        /// </summary>
        Func<string, bool>? GetPublicFilter()
        {
            var config = SecureConfigLoader.Config;
            if (!config.IsLoaded)
                return null;    // Open-access mode — show everything.

            return name => PermissionChecker.HasPublicViewPermission(name, config);
        }

        /// <summary>
        /// Returns a predicate for the secure apis.json endpoint.
        /// Tries the Warewolf HMAC-SHA256 JWT first; falls back to a Microsoft Entra
        /// token.  When neither is valid, returns a predicate that always returns
        /// <c>false</c> so that no workflows are revealed.
        /// </summary>
        Func<string, bool>? GetSecureFilter(HttpRequestData req, FunctionContext? context = null)
        {
            var config = SecureConfigLoader.Config;
            if (!config.IsLoaded)
                return _ => false;  // No config → nothing accessible via secure discovery.

            // ── 1. Use principal from middleware context when available ────────────
            if (context is not null &&
                context.Items.TryGetValue(Auth.Models.AuthConstants.PrincipalContextKey, out var p) &&
                p is Auth.WorkflowClaimsPrincipal wcp &&
                wcp.Identity?.IsAuthenticated == true)
            {
                var middlewareGroups = (IReadOnlyList<string>)wcp.Groups;
                return name => PermissionChecker.HasUserViewPermission(name, config, middlewareGroups);
            }

            // ── 2. Warewolf HMAC-SHA256 JWT (fallback for non-middleware routes) ───
            var authHeader = TryGetAuthHeader(req);
            var groups = JwtValidator.GetUserGroups(authHeader, config.SecretKey);

            // ── 3. Microsoft Entra OAuth token (fallback) ─────────────────────────
            if (groups is null)
            {
                var easyAuthHeader = TryGetEasyAuthPrincipalHeader(req);
                groups = EntraTokenValidator.GetRoles(
                    authHeader, easyAuthHeader, config.EntraTenantId, config.EntraAudience);
            }

            if (groups is null)
                return _ => false;  // Invalid / absent token → empty list.

            return name => PermissionChecker.HasUserViewPermission(name, config, groups);
        }

        // ── Response helpers ──────────────────────────────────────────────────────

        static async Task<HttpResponseData> BuildUnauthorizedResponse(HttpRequestData req)
        {
            var response = req.CreateResponse(HttpStatusCode.Unauthorized);
            response.Headers.Add("WWW-Authenticate", "Bearer");
            await response.WriteStringAsync(JsonConvert.SerializeObject(new
            {
                error = "Authentication required. Provide a valid JWT Bearer token in the Authorization header."
            }));
            return response;
        }

        static string? TryGetAuthHeader(HttpRequestData req) =>
            req.Headers.TryGetValues("Authorization", out var vals)
                ? vals.FirstOrDefault()
                : null;

        static string? TryGetEasyAuthPrincipalHeader(HttpRequestData req) =>
            req.Headers.TryGetValues(EntraTokenValidator.EasyAuthPrincipalHeader, out var vals)
                ? vals.FirstOrDefault()
                : null;
    }
}
