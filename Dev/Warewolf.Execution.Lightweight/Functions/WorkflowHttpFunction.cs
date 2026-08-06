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
using Warewolf.Execution.Lightweight.Auth;
using Warewolf.Execution.Lightweight.Auth.Middleware;
using Warewolf.Execution.Lightweight.Auth.Models;
using Warewolf.Execution.Lightweight.Http;
using Warewolf.Execution.Lightweight.Logging;
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
        readonly IWorkflowExecutor _workflowExecutor;
        readonly IApisJsonGenerator _apisJsonGenerator;
        readonly IWorkflowPolicyMatcher _policyMatcher;
        readonly string _workflowsDirectory;

        // The Azure Functions host's HTTP route table matches by function-registration
        // order (observed alphabetical-by-function-name), NOT by route specificity — a
        // catch-all route like "Secure/{*name}" therefore SHADOWS more specific sibling
        // routes registered under the same "Secure/" prefix (e.g. ServiceBusResultFunction's
        // "Secure/servicebus-result/{correlationId}" and WorkflowResumeFunction's
        // "Secure/resume/{suspensionId}"), since "ExecuteSecureWorkflow" sorts before both
        // alphabetically. Confirmed: requests to those routes were being silently executed
        // here instead, failing with "Workflow file not found" for the reserved sub-path.
        // This constraint excludes every literal Secure/ sibling route so the catch-all only
        // claims names that aren't already owned elsewhere. Keep this list in sync whenever a
        // new literal route is added under "Secure/" (currently: ServiceBusResultFunction,
        // WorkflowResumeFunction, LicensingHttpFunction.SaveSubscriptionData).
        const string NotReservedSecureSubPathPattern = "^(?!(?i:servicebus-result/|resume/|Subscriptions$)).*$";

        public WorkflowHttpFunction(
            IWorkflowExecutor workflowExecutor,
            IApisJsonGenerator apisJsonGenerator,
            IWorkflowPolicyMatcher policyMatcher)
        {
            _workflowExecutor = workflowExecutor;
            _apisJsonGenerator = apisJsonGenerator;
            _policyMatcher = policyMatcher;
            _workflowsDirectory = Environment.GetEnvironmentVariable("WorkflowsDirectory")
                ?? Path.Combine(AppContext.BaseDirectory, "Resources");
        }

        // ── Authenticated workflow routes ─────────────────────────────────────

        /// <summary>
        /// Mirrors Services/{*name} — function-key authenticated execution.
        /// <para>
        /// <c>FunctionContext</c> is accepted so that the principal already built by
        /// <see cref="ClaimsPrincipalBuilderMiddleware"/> can be reused for the
        /// <c>/services/apis.json</c> discovery route — exactly as
        /// <see cref="ExecuteSecureWorkflow"/> does for <c>/secure/apis.json</c>.
        /// Without it, <see cref="GetSecureFilter"/> falls back to a raw JWT header
        /// check which fails when App Service EasyAuth has already consumed the token,
        /// resulting in an empty discovery list.
        /// </para>
        /// </summary>
        [Function("ExecuteService")]
        [RequireWorkflowPermission(WorkflowPermission.View | WorkflowPermission.Execute)]
        public async Task<HttpResponseData> ExecuteService(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "Services/{*name}")] HttpRequestData req,
            string name,
            FunctionContext context)
            => await ExecuteNamedWorkflow(req, name, isPublic: false, context);

        /// <summary>
        /// Mirrors Secure/{*name} — JWT-authenticated execution.
        /// The Azure Function authorization level is Anonymous so the function host does
        /// not reject the request before we can validate the JWT ourselves.
        /// Auth is handled by the EasyAuth + WorkflowAuthorization middleware pipeline;
        /// FunctionContext is passed so the principal built by middleware can be reused.
        /// <para>
        /// (RTE-05) The <c>/secure/apis.json</c> discovery route flows through this same
        /// function; the View|Execute requirement makes the View intent explicit and
        /// keeps a single declarative source of truth.
        /// (RTE-06) For an admin-only route, declare:
        /// <code>[RequireWorkflowPermission(WorkflowPermission.Contribute)]</code>
        /// or <see cref="WorkflowPermission.Administrator"/>.
        /// </para>
        /// </summary>
        [Function("ExecuteSecureWorkflow")]
        [RequireWorkflowPermission(WorkflowPermission.View | WorkflowPermission.Execute)]
        public async Task<HttpResponseData> ExecuteSecureWorkflow(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "Secure/{*name:regex(" + NotReservedSecureSubPathPattern + ")}")] HttpRequestData req,
            string name,
            FunctionContext context)
            => await ExecuteNamedWorkflow(req, name, isPublic: false, context);

        // ── Anonymous / public route ──────────────────────────────────────────

        /// <summary>Mirrors Public/{*name} — anonymous (unauthenticated) execution.</summary>
        [Function("ExecutePublicWorkflow")]
        public async Task<HttpResponseData> ExecutePublicWorkflow(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "Public/{*name}")] HttpRequestData req,
            string name,
            FunctionContext context)
            => await ExecuteNamedWorkflow(req, name, isPublic: true, context);

        // ── apis.json discovery routes ────────────────────────────────────────

        /// <summary>
        /// Root-level apis.json — lists workflows visible to the caller.
        ///
        /// <para>
        /// Mirrors <c>WebServerController.ExecuteGetRootLevelApisJson</c> /
        /// <c>GetApisJsonServiceHandler.ProcessRequest</c> together with
        /// <c>ServerAuthorizationService.IsAuthorizedImpl</c> which routes both
        /// <c>WebExecuteGetRootLevelApisJson</c> and <c>WebExecuteGetApisJsonForFolder</c>
        /// through <c>IsAuthorizedToConnect(request.User)</c> — a global "any permission"
        /// gate — before the per-resource discovery loop.
        /// </para>
        ///
        /// <para>
        /// <b>Connect gate (Q3 = same as server)</b>: because <c>/apis.json</c> is a
        /// route and does not map to any workflow resource, the gate is the same
        /// <c>AuthorizationContext.Any</c> check the server uses: at least one in-role
        /// permission entry must exist (public or authenticated, depending on
        /// <c>?isPublic</c>).
        /// </para>
        ///
        /// <para>
        /// <b>isPublic (Q2 = B)</b>: reads <c>?isPublic=true</c> from the query string.
        /// When <c>true</c>, falls through without requiring a token and applies the
        /// public discovery filter.  When absent or <c>false</c>, the secure filter is
        /// used; if no valid credential is present <see cref="GetSecureFilter"/> returns
        /// a predicate that hides every workflow.
        /// </para>
        /// </summary>
        [Function("ExecuteRootApisJson")]
        public async Task<HttpResponseData> ExecuteRootApisJson(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "apis.json")] HttpRequestData req, FunctionContext context)
        {
            var config = SecureConfigLoader.Config;
            var isPublic = IsPublicQueryParam(req);

            if (isPublic)
            {
                // Connect gate — public path: any Public-group entry must grant ≥1 permission.
                if (config.IsLoaded && !PermissionChecker.HasPublicConnectPermission(config))
                    return await BuildForbiddenResponse(req, "Public access to apis.json is not permitted.");

                return await CreateApisJsonResponse(req, pathFilter: null, isPublic: true, GetPublicFilter());
            }

            // Connect gate — authenticated path: caller must have ≥1 permission anywhere.
            var (isAuthenticated, userGroups) = ResolveCallerGroups(req, context);
            if (config.IsLoaded && !PermissionChecker.HasConnectPermission(config, userGroups))
                return await BuildForbiddenResponse(req);

            return await CreateApisJsonResponse(req, pathFilter: null, isPublic: false, GetSecureFilter(req, context));
        }

        // ── Named-workflow routes ─────────────────────────────────────────────

        /// <summary>
        /// Execute a workflow by name from the route.
        /// Mirrors <c>WebServerController.ExecuteService</c> + suffix handling.
        /// </summary>
        [Function("ExecuteWorkflowByName")]
        [RequireWorkflowPermission(WorkflowPermission.View | WorkflowPermission.Execute)]
        public async Task<HttpResponseData> ExecuteByName(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "workflow/{workflowName}")] HttpRequestData req,
            string workflowName,
            FunctionContext context)
        {
            var (resolvedName, isDebug, isXml, isApi) = NameSuffixParser.Parse(workflowName);
            var executionRequest = await WorkflowFunctionHelper.ParseRequestAsync(req, _workflowsDirectory, resolvedName);

            executionRequest.WebServerUri = req.Url;
            executionRequest.ReturnType = isXml ? EmitionTypes.XML
                                          : isApi ? EmitionTypes.OPENAPI
                                                  : EmitionTypes.JSON;
            if (isDebug)
                executionRequest.IsDebug = true;

            var result = _workflowExecutor.Execute(executionRequest);
            FlushUsagePublishContext(context);
            return await ResponseBuilder.BuildAsync(req, result,
                isXml ? ResponseBuilder.XmlContentType : ResponseBuilder.JsonContentType);
        }

        /// <summary>
        /// Execute a workflow identified via query string or request body.
        /// Mirrors <c>WebServerController.ExecuteService</c> (generic path).
        /// </summary>
        [Function("ExecuteWorkflow")]
        [RequireWorkflowPermission(WorkflowPermission.View | WorkflowPermission.Execute)]
        public async Task<HttpResponseData> Execute(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "workflow")] HttpRequestData req,
            FunctionContext context)
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
            FlushUsagePublishContext(context);
            return await ResponseBuilder.BuildAsync(req, result);
        }

        // ── Shared private helpers ────────────────────────────────────────────

        /// <summary>
        /// Core dispatch method shared by all named-workflow routes.
        ///
        /// <list type="bullet">
        ///   <item>apis.json routes → permission-filtered discovery, never returns 401.</item>
        ///   <item>Secure/Services execution routes → JWT validation + policy check
        ///         (View|Execute); returns 401 when the token is absent or invalid,
        ///         403 when the policy denies access.</item>
        ///   <item>Public execution routes → policy check with an anonymous principal
        ///         so only workflows whose <em>Public</em> group has Execute permission
        ///         are reachable (mirrors server's <c>AuthorizationContext.Execute</c>
        ///         check for <c>WebExecutePublicWorkflow</c>).</item>
        /// </list>
        /// </summary>
        async Task<HttpResponseData> ExecuteNamedWorkflow(
            HttpRequestData request,
            string name,
            bool isPublic,
            FunctionContext? context = null)
        {

            // (MWA-07 / OBS-05) Per-request correlation id — accept caller-supplied
            // value or generate a short one when missing.  Same value is included
            // in audit logs and 401/403 response headers + body.
            var correlationId = HttpResponseHelper.ResolveCorrelationId(request);

            // ── Route dispatch ────────────────────────────────────────────────────
            // Cases are evaluated in priority order:
            //   1. apis.json  — discovery listing (public or connect-gated).
            //   2. isPublic   — anonymous Execute permission check via policy matcher.
            //   3. !isPublic  — JWT/principal authentication check (secure routes).
            var routeCase = NameSuffixParser.IsApisJsonRequest(name) ? RouteCase.ApisJson

                          : isPublic ? RouteCase.PublicExecution
                                                                      : RouteCase.SecureExecution;

            switch (routeCase)
            {
                // ── Case 1: apis.json discovery listing ───────────────────────────
                // Always accessible; filtered by permissions.  Public endpoint returns
                // workflows visible to the anonymous/Public group.  Authenticated endpoint
                // mirrors ServerAuthorizationService.IsAuthorizedToConnect before listing.
                case RouteCase.ApisJson:
                    {
                        var pathFilter = NameSuffixParser.ExtractApisJsonPath(name);

                        if (isPublic)
                            return await CreateApisJsonResponse(request, pathFilter, isPublic, GetPublicFilter());

                        // Connect gate (authenticated path) — global "any permission" check.
                        var config = SecureConfigLoader.Config;
                        var (_, callerGroups) = ResolveCallerGroups(request, context);
                        if (config.IsLoaded && !PermissionChecker.HasConnectPermission(config, callerGroups))
                            return await BuildForbiddenResponse(request);

                        return await CreateApisJsonResponse(request, pathFilter, isPublic, GetSecureFilter(request, context));
                    }

                // ── Case 2: public execution ──────────────────────────────────────
                // Enforces Execute permission via the unified policy matcher with an
                // anonymous principal.  Mirrors AuthorizationContext.Execute for
                // WebExecutePublicWorkflow — Public group's Execute flag is the sole gate.
                case RouteCase.PublicExecution:
                    {
                        bool isApiRequest = NameSuffixParser.IsApiRequest(name);
                        var publicWfKey = BuildWorkflowKey(name);
                        var matchResult = _policyMatcher.Evaluate(
                            publicWfKey,
                            WorkflowClaimsPrincipal.Anonymous(),
                            isApiRequest ? WorkflowPermission.Execute : WorkflowPermission.View | WorkflowPermission.Execute);

                        if (matchResult.Outcome == PolicyMatchOutcome.Forbidden ||
                            matchResult.Outcome == PolicyMatchOutcome.ConfigMissingDeny)
                        {
                            return await HttpResponseHelper.WriteWrappedErrorAsync(request, context, HttpStatusCode.InternalServerError,
                                (int)HttpStatusCode.InternalServerError, "internal_server_error",
                                "Invalid Authentication Token or invalid permissions to Execute resource",
                                matchResult.DenialReason ?? "Insufficient permissions.", correlationId);
                        }
                        break;
                    }

                // ── Case 3: secure execution ──────────────────────────────────────
                // Prefer the principal already built by the middleware pipeline (Secure/* routes).
                // Fall back to direct JWT validation for Services/* routes that bypass middleware.
                case RouteCase.SecureExecution:
                    {
                        var principalAuthenticated =
                            context is not null &&
                            context.Items.TryGetValue(Auth.Models.AuthConstants.PrincipalContextKey, out var p) &&
                            p is Auth.WorkflowClaimsPrincipal wcp &&
                            wcp.Identity?.IsAuthenticated == true;

                        if (!principalAuthenticated)
                        {
                            var authResult = ValidateJwt(request);
                            if (!authResult.IsValid)
                                return await BuildForbiddenResponse(request);
                        }
                        break;
                    }
            }

            // ── Execute the workflow ──────────────────────────────────────────────
            var (workflowName, isDebug, isXml, isApi) = NameSuffixParser.Parse(name);
            var executionRequest = await WorkflowFunctionHelper.ParseRequestAsync(request, _workflowsDirectory, workflowName);

            // Suffix flags are authoritative — override any format inferred from the URL path.
            executionRequest.WebServerUri = request.Url;
            executionRequest.ReturnType = isXml ? EmitionTypes.XML
                                          : isApi ? EmitionTypes.OPENAPI
                                                  : EmitionTypes.JSON;
            if (isDebug)
                executionRequest.IsDebug = true;

            var result = _workflowExecutor.Execute(executionRequest);
            FlushUsagePublishContext(context);
            return await ResponseBuilder.BuildAsync(request, result,
                isXml ? ResponseBuilder.XmlContentType : ResponseBuilder.JsonContentType);
        }

        /// <summary>
        /// Transfers the ambient <see cref="UsagePublishContext"/> (if any) that
        /// <c>WorkflowExecutor.Execute</c> just populated into
        /// <c>FunctionContext.Items</c>, where <see cref="Infrastructure.UsagePublishMiddleware"/>
        /// can reliably read it after the rest of the pipeline unwinds.
        ///
        /// <para>
        /// Must be called immediately after <c>_workflowExecutor.Execute(...)</c>,
        /// on the same synchronous call stack — see the AsyncLocal caveat
        /// documented on <see cref="UsagePublishContext"/>. When <paramref name="context"/>
        /// is <c>null</c> (a Function overload that does not receive
        /// <see cref="FunctionContext"/>), the ambient value is simply discarded —
        /// no usage event will be published for that invocation. Every current
        /// entry point that calls <c>_workflowExecutor.Execute</c> passes its
        /// <see cref="FunctionContext"/> here.
        /// </para>
        /// </summary>
        internal static void FlushUsagePublishContext(FunctionContext? context)
        {
            var pending = UsagePublishContext.TakeCurrent();
            if (context is not null && pending is not null)
            {
                context.Items[UsagePublishContext.ItemsKey] = pending;
            }
        }

        /// <summary>
        /// Generates and returns an apis.json discovery document for the given path,
        /// with an optional per-workflow permission predicate applied.
        /// </summary>
        async Task<HttpResponseData> CreateApisJsonResponse(
            HttpRequestData req,
            string? pathFilter,
            bool isPublic,
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

            return name => PermissionChecker.HasPublicDiscoveryPermission(name, config);
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
                return name => PermissionChecker.HasUserDiscoveryPermission(name, config, middlewareGroups);
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

            return name => PermissionChecker.HasUserDiscoveryPermission(name, config, groups);
        }

        // ── Response helpers ──────────────────────────────────────────────────────

        static async Task<HttpResponseData> BuildForbiddenResponse(HttpRequestData req)
        {
            var response = req.CreateResponse(HttpStatusCode.Forbidden);
            response.Headers.Add("WWW-Authenticate", "Bearer");
            await response.WriteStringAsync(JsonConvert.SerializeObject(new
            {
                Error = new
                {
                    Status = 403,
                    Title = "user_forbidden",
                    Message = "Authorization has been denied for this request."
                }
            }));
            return response;
        }

        static async Task<HttpResponseData> BuildForbiddenResponse(HttpRequestData req, string message)
        {
            var response = req.CreateResponse(HttpStatusCode.Forbidden);
            await response.WriteStringAsync(JsonConvert.SerializeObject(new { error = message }));
            return response;
        }

        /// <summary>
        /// Reads the <c>?isPublic</c> query-string parameter.
        /// Returns <c>true</c> only when the value is explicitly <c>true</c>
        /// (case-insensitive), mirroring how the server binds the route variable.
        /// </summary>
        static bool IsPublicQueryParam(HttpRequestData req)
        {
            var qs = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
            var val = qs["isPublic"];
            return string.Equals(val, "true", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Resolves the caller's group list from the middleware principal context,
        /// Warewolf HMAC-SHA256 JWT, or Entra token — in that order.
        /// Returns <c>(true, groups)</c> when at least one valid source was found.
        /// </summary>
        (bool IsAuthenticated, IReadOnlyList<string>? Groups) ResolveCallerGroups(
            HttpRequestData req, FunctionContext? context)
        {
            // ── 1. Middleware-built principal (preferred) ─────────────────────────
            if (context is not null &&
                context.Items.TryGetValue(Auth.Models.AuthConstants.PrincipalContextKey, out var p) &&
                p is Auth.WorkflowClaimsPrincipal wcp &&
                wcp.Identity?.IsAuthenticated == true)
            {
                return (true, (IReadOnlyList<string>)wcp.Groups);
            }

            var config = SecureConfigLoader.Config;
            var authHeader = TryGetAuthHeader(req);

            // ── 2. Warewolf HMAC-SHA256 JWT ───────────────────────────────────────
            var groups = JwtValidator.GetUserGroups(authHeader, config.SecretKey);
            if (groups is not null)
                return (true, groups);

            // ── 3. Microsoft Entra OAuth token ────────────────────────────────────
            var easyAuthHeader = TryGetEasyAuthPrincipalHeader(req);
            var entraRoles = EntraTokenValidator.GetRoles(
                authHeader, easyAuthHeader, config.EntraTenantId, config.EntraAudience);
            return (entraRoles is not null, entraRoles);
        }

        static string? TryGetAuthHeader(HttpRequestData req) =>
            req.Headers.TryGetValues("Authorization", out var vals)
                ? vals.FirstOrDefault()
                : null;

        static string? TryGetEasyAuthPrincipalHeader(HttpRequestData req) =>
            req.Headers.TryGetValues(EntraTokenValidator.EasyAuthPrincipalHeader, out var vals)
                ? vals.FirstOrDefault()
                : null;

        // ── Resource key helpers ──────────────────────────────────────────────────

        /// <summary>
        /// Builds a lowercase, folder-preserving resource key from a raw route
        /// segment (e.g. <c>"Folder/MyWorkflow.json"</c> → <c>"folder/myworkflow"</c>).
        ///
        /// Delegates normalization (query-string stripping, URL-decoding, backslash
        /// normalization) to <see cref="NameSuffixParser.Normalize"/> so the produced
        /// key is always consistent with <see cref="WorkflowAuthorizationMiddleware.ExtractWorkflowName"/>.
        /// </summary>
        static string BuildWorkflowKey(string routeSegment)
        {
            var normalized = NameSuffixParser.Normalize(routeSegment);
            var segments = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length == 0)
                return string.Empty;

            var lastStripped = Path.GetFileNameWithoutExtension(segments[^1]);

            if (segments.Length == 1)
                return lastStripped.ToLowerInvariant();

            var folderParts = segments[..^1].Select(s => s.ToLowerInvariant());
            return string.Join("/", folderParts.Append(lastStripped.ToLowerInvariant()));
        }
    }

    /// <summary>
    /// Identifies the dispatch branch chosen by <c>ExecuteNamedWorkflow</c>.
    /// </summary>
    internal enum RouteCase
    {
        /// <summary>apis.json discovery listing (public or connect-gated).</summary>
        ApisJson,
        /// <summary>Single-workflow OpenAPI spec request ({workflow}.api).</summary>
        ApiSpec,
        /// <summary>Public workflow execution — anonymous Execute permission check.</summary>
        PublicExecution,
        /// <summary>Secure workflow execution — JWT / middleware principal required.</summary>
        SecureExecution,
    }
}
