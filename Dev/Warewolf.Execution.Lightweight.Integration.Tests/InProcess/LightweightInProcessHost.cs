/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  In-process functional-test harness for the lightweight execution engine.
 *
 *  Why this exists
 *  ───────────────
 *  The original integration tests issued real HTTP calls to a separately-running
 *  engine on http://localhost:7071.  Because secure.config and the request principal
 *  live inside *that* process, a test could not control permissions or identity
 *  per-test.  This harness instead exercises the SAME production units the worker
 *  pipeline composes — entirely in-process — so each test seeds its own secure.config
 *  and identity and runs the real WorkflowExecutor against the deployed .bite resources.
 *
 *  Two entry points
 *  ────────────────
 *    • ExecutePublicAsync(route)            — invokes WorkflowHttpFunction directly for
 *                                             anonymous /public execution (no middleware).
 *    • SendThroughPipelineAsync(method,path) — runs the FULL middleware pipeline
 *                                             (EasyAuthRedirect → ClaimsPrincipalBuilder →
 *                                             WorkflowAuthorization → function dispatch),
 *                                             so /secure, /services, apis.json, and the
 *                                             auth 401/403/302/correlation-id behaviours
 *                                             are reproduced exactly as in the engine.
 *
 *  Parser chain: EasyAuthPrincipalParser + WarewolfHmacJwtPrincipalParser.  The HMAC
 *  parser validates the Warewolf HMAC-SHA256 JWTs the tests mint with this host's
 *  SecretKey.  (Production DI currently registers EasyAuth + Entra-Bearer; the Entra
 *  parser never validates the HMAC test tokens, so it is omitted here — an absent
 *  parser yields an Anonymous principal, which is the correct outcome for the
 *  negative/no-token cases.)
 *
 *  Lifecycle: construct inside a `using` (or TestInitialize/TestCleanup); Dispose
 *  restores the WAREWOLF_SECURE_CONFIG / licence env vars and reloads the loader.
 *  Tests using this harness MUST be [DoNotParallelize] (SecureConfigLoader is a
 *  process-wide singleton).
 */

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using Dev2.Services.Security;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Warewolf.Execution.Lightweight.Auth;
using Warewolf.Execution.Lightweight.Auth.Middleware;
using Warewolf.Execution.Lightweight.Auth.Models;
using Warewolf.Execution.Lightweight.Auth.Parsers;
using Warewolf.Execution.Lightweight.Functions;
using Warewolf.Execution.Lightweight.Infrastructure;
using Warewolf.Execution.Lightweight.Logging;
using Warewolf.Execution.Lightweight.Security;
using Warewolf.Execution.Lightweight.Tests.Auth;
using Warewolf.Execution.Lightweight.Tests.Security;

namespace Warewolf.Execution.Lightweight.Integration.Tests.InProcess
{
    /// <summary>Status + body + headers captured from an in-process pipeline invocation.</summary>
    internal sealed record PipelineResponse(
        HttpStatusCode Status,
        string Body,
        IReadOnlyDictionary<string, string> Headers);

    /// <summary>
    /// Builds the real lightweight authorization + execution stack around an in-memory
    /// secure.config and invokes the engine in-process. Reusable across test classes.
    /// </summary>
    internal sealed class LightweightInProcessHost : IDisposable
    {
        private const string ConfigPathEnvVar = SecureConfigLoader.ConfigPathEnvVar;
        private const string LicenseCheckEnvVar = "WAREWOLF_LICENSE_CHECK_ENABLED";

        private readonly string? _originalConfigEnv;
        private readonly string? _originalLicenseEnv;
        private readonly string _tempConfigPath;
        private readonly string _resourcesDir;

        private readonly WorkflowHttpFunction _function;
        private readonly LicensingHttpFunction _licensingFunction;
        private readonly LoginFunction _loginFunction;
        private readonly DropboxOAuthFunction _dropboxFunction;
        private readonly IWorkflowPolicyMatcher _policyMatcher;
        private readonly IRouteAuthorizationRegistry _routeRegistry;
        private readonly AuditLogger _auditLogger;
        private readonly IReadOnlyList<IPrincipalParser> _parsers;

        /// <summary>The HMAC secret key seeded into secure.config — mint test JWTs with this.</summary>
        internal string SecretKey { get; }

        internal LightweightInProcessHost(SecuritySettingsTO settings)
        {
            SecretKey = settings.SecretKey;

            // 1. Persist + activate the secure.config the in-process singletons will read.
            _originalConfigEnv = Environment.GetEnvironmentVariable(ConfigPathEnvVar);
            _tempConfigPath = SecureConfigBuilder.WriteTempConfig(settings);
            Environment.SetEnvironmentVariable(ConfigPathEnvVar, _tempConfigPath);
            SecureConfigLoader.Reload();

            // 1a. Disable the licence/subscription gate so the real WorkflowExecutor runs.
            _originalLicenseEnv = Environment.GetEnvironmentVariable(LicenseCheckEnvVar);
            Environment.SetEnvironmentVariable(LicenseCheckEnvVar, "false");

            // 2. Build the genuine authorization + execution stack.
            var policyLoader = new WorkflowAuthPolicyLoader(NullLogger<WorkflowAuthPolicyLoader>.Instance);
            _policyMatcher = new WorkflowPolicyMatcher(policyLoader);
            var executor = new WorkflowExecutor(new NullExecutionLogger());

            _resourcesDir = Path.Combine(AppContext.BaseDirectory, "Resources");
            var apisJsonGenerator = new ApisJsonGenerator(_resourcesDir);

            _function = new WorkflowHttpFunction(executor, apisJsonGenerator, _policyMatcher);

            // 2a. Special-function classes (Phase 3): Licensing / Login / Dropbox OAuth.
            //     - LicensingHttpFunction never dereferences its IWarewolfLicense dependency
            //       (IsLicensed reads SubscriptionProvider.Instance), so null is safe here.
            //     - DropboxOAuthFunction resolves only OPTIONAL services (FileDecryptionHelper,
            //       KeyVaultSecretManager) via GetService, so an empty provider is fine — the
            //       error/redirect paths under test never touch Key Vault.
            _licensingFunction = new LicensingHttpFunction(null!, NullLogger<LicensingHttpFunction>.Instance);
            _loginFunction = new LoginFunction(executor);
            _dropboxFunction = new DropboxOAuthFunction(
                new ServiceCollection().BuildServiceProvider(),
                NullLogger<DropboxOAuthFunction>.Instance);

            // 3. Middleware-pipeline collaborators.
            _routeRegistry = RouteAuthorizationRegistry.BuildFrom(typeof(WorkflowHttpFunction));
            _auditLogger = new AuditLogger(NullLogger<AuditLogger>.Instance);
            _parsers = new IPrincipalParser[]
            {
                new EasyAuthPrincipalParser(NullLogger<EasyAuthPrincipalParser>.Instance),
                new WarewolfHmacJwtPrincipalParser(NullLogger<WarewolfHmacJwtPrincipalParser>.Instance),
            };
        }

        // ── Config-variant factories ────────────────────────────────────────────

        /// <summary>Public group has server-wide View+Execute — every workflow reachable on /public.</summary>
        internal static LightweightInProcessHost WithPublicExecuteAll() =>
            new(SecureConfigBuilder.Build(
                SecureConfigBuilder.NewSecretKey(),
                SecureConfigBuilder.Admin(View: true),
                SecureConfigBuilder.ServerPerm(SecureConfigBuilder.PublicGroup, View: true, Execute: true)));

        /// <summary>Full secure.config with the given extra permission entries (Admin always included).</summary>
        internal static LightweightInProcessHost WithSettings(SecuritySettingsTO settings) => new(settings);

        // ── Anonymous /public execution (direct, no middleware) ───────────────────

        internal async Task<(HttpStatusCode Status, string Body)> ExecutePublicAsync(
            string routeName, string method = "GET")
        {
            var url = new Uri($"http://localhost:7071/public/{routeName}");
            var ctx = new HttpFunctionContext();
            var request = new FakeHttpRequestData(ctx, url, method);

            var response = await _function.ExecutePublicWorkflow(request, routeName, ctx);
            return (response.StatusCode, await ReadBodyAsync(response));
        }

        // ── Full middleware pipeline (any route) ──────────────────────────────────

        /// <summary>
        /// Runs the request through the real worker middleware pipeline and dispatches to
        /// the matching function, returning the response a remote HTTP caller would see.
        /// <paramref name="path"/> is the full path (e.g. "/Secure/HelloWorld.json").
        /// </summary>
        internal async Task<PipelineResponse> SendThroughPipelineAsync(
            string method,
            string path,
            IDictionary<string, string>? headers = null,
            bool isDevelopment = false)
        {
            var ctx = new HttpFunctionContext();
            var req = new FakeHttpRequestData(ctx, new Uri($"http://localhost:7071{path}"), method);
            if (headers != null)
                foreach (var h in headers)
                    req.AddHeader(h.Key, h.Value);
            ctx.SetHttpRequest(req);
            ctx.SetFunctionName(ResolveFunctionName(path));

            var easyAuth = new EasyAuthRedirectMiddleware(BuildHostConfig(isDevelopment));
            var claims = new ClaimsPrincipalBuilderMiddleware(
                _parsers, NullLogger<ClaimsPrincipalBuilderMiddleware>.Instance);
            var authz = new WorkflowAuthorizationMiddleware(
                _policyMatcher, _routeRegistry,
                new StubHostEnvironment(isDevelopment ? Environments.Development : Environments.Production),
                _auditLogger, NullLogger<WorkflowAuthorizationMiddleware>.Instance);

            HttpResponseData? terminal = null;
            FunctionExecutionDelegate dispatch = async _ => terminal = await DispatchAsync(req, ctx, path);
            FunctionExecutionDelegate afterAuthz = c => authz.Invoke(c, dispatch);
            FunctionExecutionDelegate afterClaims = c => claims.Invoke(c, afterAuthz);

            await easyAuth.Invoke(ctx, afterClaims);

            var response = terminal ?? ctx.CapturedInvocationResult as HttpResponseData;
            if (response is null)
                return new PipelineResponse(HttpStatusCode.OK, string.Empty, new Dictionary<string, string>());

            return new PipelineResponse(
                response.StatusCode,
                await ReadBodyAsync(response),
                HeadersToDictionary(response.Headers));
        }

        private async Task<HttpResponseData> DispatchAsync(
            FakeHttpRequestData req, FunctionContext ctx, string path)
        {
            var p = path.Split('?')[0];

            // ── Special-function routes (Phase 3) — matched before the generic
            //    /secure/ and workflow routes so they reach the right function. ──
            if (p.Equals("/IsLicensed", StringComparison.OrdinalIgnoreCase))
                return await _licensingFunction.IsLicensed(req);
            if (p.Equals("/Subscriptions", StringComparison.OrdinalIgnoreCase))
                return await _licensingFunction.GetSubscription(req);
            if (p.Equals("/secure/Subscriptions", StringComparison.OrdinalIgnoreCase))
                return await _licensingFunction.SaveSubscription(req, ctx);
            if (p.Equals("/login", StringComparison.OrdinalIgnoreCase))
                return await _loginFunction.Login(req);
            if (p.Equals("/oauth/dropbox/start", StringComparison.OrdinalIgnoreCase))
                return await _dropboxFunction.Start(req);
            if (p.Equals("/oauth/dropbox/callback", StringComparison.OrdinalIgnoreCase))
                return await _dropboxFunction.Callback(req);

            if (p.Equals("/apis.json", StringComparison.OrdinalIgnoreCase))
                return await _function.ExecuteRootApisJson(req, ctx);
            if (p.StartsWith("/public/", StringComparison.OrdinalIgnoreCase))
                return await _function.ExecutePublicWorkflow(req, p.Substring("/public/".Length), ctx);
            if (p.StartsWith("/secure/", StringComparison.OrdinalIgnoreCase))
                return await _function.ExecuteSecureWorkflow(req, p.Substring("/secure/".Length), ctx);
            if (p.StartsWith("/services/", StringComparison.OrdinalIgnoreCase))
                return await _function.ExecuteService(req, p.Substring("/services/".Length), ctx);
            if (p.StartsWith("/workflow/", StringComparison.OrdinalIgnoreCase))
                return await _function.ExecuteByName(req, p.Substring("/workflow/".Length), ctx);
            if (p.Equals("/workflow", StringComparison.OrdinalIgnoreCase))
                return await _function.Execute(req, ctx);

            return await _function.ExecutePublicWorkflow(req, p.TrimStart('/'), ctx);
        }

        /// <summary>Maps a route to its [Function] name so the route registry resolves required permissions.</summary>
        private static string ResolveFunctionName(string path)
        {
            var p = path.Split('?')[0];
            if (p.StartsWith("/secure/", StringComparison.OrdinalIgnoreCase)) return "ExecuteSecureWorkflow";
            if (p.StartsWith("/services/", StringComparison.OrdinalIgnoreCase)) return "ExecuteService";
            if (p.StartsWith("/workflow/", StringComparison.OrdinalIgnoreCase)) return "ExecuteWorkflowByName";
            if (p.Equals("/workflow", StringComparison.OrdinalIgnoreCase)) return "ExecuteWorkflow";
            return "ExecutePublicWorkflow";
        }

        /// <summary>
        /// Constructs a <see cref="HostEnvironmentConfig"/> with the requested IsDevelopment flag
        /// via its private constructor (no public factory exists for tests).
        /// EasyAuthRedirectMiddleware only reads IsDevelopment, so the other args are inert.
        /// </summary>
        private HostEnvironmentConfig BuildHostConfig(bool isDevelopment)
        {
            var ctor = typeof(HostEnvironmentConfig)
                .GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)
                .Single();

            return (HostEnvironmentConfig)ctor.Invoke(new object?[]
            {
                _resourcesDir, // workflowsDirectory
                null,          // vaultName
                "secret",      // secretName
                "test",        // instanceId
                false,         // skipFailureToRetrieveSecret
                null,          // tenantId
                null,          // managedIdentityClientId
                null,          // debugKeyVaultSecret
                isDevelopment, // isDevelopment
                null,          // debugPrincipalToken
            });
        }

        private static IReadOnlyDictionary<string, string> HeadersToDictionary(HttpHeadersCollection headers)
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var h in headers)
                dict[h.Key] = string.Join(",", h.Value);
            return dict;
        }

        private static async Task<string> ReadBodyAsync(HttpResponseData response)
        {
            response.Body.Position = 0;
            using var reader = new StreamReader(response.Body, leaveOpen: true);
            return await reader.ReadToEndAsync();
        }

        public void Dispose()
        {
            Environment.SetEnvironmentVariable(ConfigPathEnvVar, _originalConfigEnv);
            Environment.SetEnvironmentVariable(LicenseCheckEnvVar, _originalLicenseEnv);
            try
            {
                if (File.Exists(_tempConfigPath))
                    File.Delete(_tempConfigPath);
            }
            catch { /* best effort — temp file */ }

            SecureConfigLoader.Reload();
        }

        /// <summary>Minimal <see cref="IHostEnvironment"/> with a configurable environment name.</summary>
        private sealed class StubHostEnvironment : IHostEnvironment
        {
            public StubHostEnvironment(string environmentName) => EnvironmentName = environmentName;
            public string EnvironmentName { get; set; }
            public string ApplicationName { get; set; } = "Warewolf.Execution.Lightweight.Tests";
            public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
            public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        }

        /// <summary>No-op <see cref="IExecutionLogger"/> — keeps execution silent in tests.</summary>
        private sealed class NullExecutionLogger : IExecutionLogger
        {
            public void LogTrace(string message, Guid executionId) { }
            public void LogTrace(string message, Exception exception, Guid executionId) { }
            public void LogDebug(string message, Guid executionId) { }
            public void LogDebug(string message, Exception exception, Guid executionId) { }
            public void LogError(Exception ex, string log) { }
            public void LogInfo(string message, Guid executionId) { }
            public void LogInfo(string message, Exception exception, Guid executionId) { }
            public void LogInfo(string message) { }
            public void LogWarning(string message, Guid executionId) { }
            public void LogWarning(string message, Exception exception, Guid executionId) { }
            public void LogError(string message, Guid executionId) { }
            public void LogError(string activityName, Exception ex, Guid executionId) { }
            public void LogFatal(string message, Guid executionId) { }
            public void LogFatal(string message, Exception exception, Guid executionId) { }
        }
    }
}
