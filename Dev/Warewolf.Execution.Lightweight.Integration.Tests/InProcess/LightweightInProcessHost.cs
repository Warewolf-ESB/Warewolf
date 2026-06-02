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
 *  engine on http://localhost:7071.  Because secure.config and the request
 *  principal live inside *that* process, a test could not control permissions or
 *  identity per-test — every /public/ call shared whatever static secure.config
 *  the engine happened to boot with (the cause of the earlier 40 HTTP-500 auth
 *  failures).
 *
 *  This harness instead exercises the SAME production units the worker pipeline
 *  composes — WorkflowAuthPolicyLoader → WorkflowPolicyMatcher → WorkflowExecutor,
 *  fronted by the real WorkflowHttpFunction — entirely in-process.  Each test:
 *
 *    1. Seeds a real (encrypted) secure.config via SecureConfigBuilder, so
 *       permissions are read through the genuine loader/decrypt path.
 *    2. Runs the REAL WorkflowExecutor against the deployed .bite resources, so
 *       the workflow actually executes and produces real output.
 *
 *  Permission model (mirrors the server)
 *  ─────────────────────────────────────
 *    • /public/ routes  — the built-in "Public" group must hold View+Execute for
 *      the workflow (resolved by the matcher with an anonymous principal).
 *    • /secure/ routes  — the caller principal's roles (≡ WindowsGroups) must hold
 *      View+Execute, granted either server-wide (IsServer=true) or per-resource.
 *
 *  Lifecycle: construct inside a `using` (or via TestInitialize/TestCleanup) so the
 *  WAREWOLF_SECURE_CONFIG env var and the SecureConfigLoader singleton are restored
 *  after the test.  Tests using this harness MUST be [DoNotParallelize] because the
 *  loader is a process-wide singleton.
 */

using System.Net;
using Dev2.Services.Security;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Warewolf.Execution.Lightweight.Auth;
using Warewolf.Execution.Lightweight.Logging;
using Warewolf.Execution.Lightweight.Security;
using Warewolf.Execution.Lightweight.Tests.Auth;
using Warewolf.Execution.Lightweight.Tests.Security;

namespace Warewolf.Execution.Lightweight.Integration.Tests.InProcess
{
    /// <summary>
    /// Builds the real lightweight authorization + execution stack around an
    /// in-memory secure.config and invokes <see cref="WorkflowHttpFunction"/>
    /// in-process. Reusable across integration-test classes.
    /// </summary>
    internal sealed class LightweightInProcessHost : IDisposable
    {
        private const string ConfigPathEnvVar = SecureConfigLoader.ConfigPathEnvVar;
        private const string LicenseCheckEnvVar = "WAREWOLF_LICENSE_CHECK_ENABLED";

        private readonly string? _originalConfigEnv;
        private readonly string? _originalLicenseEnv;
        private readonly string _tempConfigPath;
        private readonly WorkflowHttpFunction _function;

        /// <summary>
        /// Seeds <paramref name="settings"/> as the active secure.config and wires the
        /// real loader, matcher, executor, apis.json generator, and HTTP function.
        /// </summary>
        internal LightweightInProcessHost(SecuritySettingsTO settings)
        {
            // 1. Persist + activate the secure.config the in-process singletons will read.
            _originalConfigEnv = Environment.GetEnvironmentVariable(ConfigPathEnvVar);
            _tempConfigPath = SecureConfigBuilder.WriteTempConfig(settings);
            Environment.SetEnvironmentVariable(ConfigPathEnvVar, _tempConfigPath);
            SecureConfigLoader.Reload();

            // 1a. Disable the license/subscription gate for the duration of the test,
            //     mirroring the engine's local.settings.json (WAREWOLF_LICENSE_CHECK_ENABLED=false).
            //     Without this the real WorkflowExecutor blocks execution before the workflow runs.
            _originalLicenseEnv = Environment.GetEnvironmentVariable(LicenseCheckEnvVar);
            Environment.SetEnvironmentVariable(LicenseCheckEnvVar, "false");

            // 2. Build the genuine authorization + execution stack.
            //    WorkflowAuthPolicyLoader reads SecureConfigLoader.Config in its ctor,
            //    so the Reload() above must already have run.
            var policyLoader = new WorkflowAuthPolicyLoader(NullLogger<WorkflowAuthPolicyLoader>.Instance);
            var policyMatcher = new WorkflowPolicyMatcher(policyLoader);
            var executor = new WorkflowExecutor(new NullExecutionLogger());

            // Real workflow resources are copied next to the test assembly under Resources/.
            var resourcesDir = Path.Combine(AppContext.BaseDirectory, "Resources");
            var apisJsonGenerator = new ApisJsonGenerator(resourcesDir);

            _function = new WorkflowHttpFunction(executor, apisJsonGenerator, policyMatcher);
        }

        /// <summary>
        /// Convenience factory: build settings where the built-in Public group has
        /// server-wide View+Execute (so every workflow is reachable on /public/),
        /// then construct the host. Mirrors the server's "Public can run everything"
        /// posture and is the common arrangement for public-route tests.
        /// </summary>
        internal static LightweightInProcessHost WithPublicExecuteAll()
        {
            var settings = SecureConfigBuilder.Build(
                SecureConfigBuilder.NewSecretKey(),
                SecureConfigBuilder.Admin(View: true),
                SecureConfigBuilder.ServerPerm(SecureConfigBuilder.PublicGroup, View: true, Execute: true));
            return new LightweightInProcessHost(settings);
        }

        /// <summary>
        /// Invokes the anonymous <c>/Public/{*name}</c> route in-process and returns the
        /// response status and body. <paramref name="routeName"/> is the catch-all route
        /// value exactly as the Functions host would supply it (already URL-decoded),
        /// e.g. <c>"tools/system info/TestGettingComputerName.json"</c>.
        /// </summary>
        internal async Task<(HttpStatusCode Status, string Body)> ExecutePublicAsync(
            string routeName, string method = "GET")
        {
            var url = new Uri($"http://localhost:7071/public/{routeName}");
            var request = new FakeHttpRequestData(new TestFunctionContext(), url, method);

            var response = await _function.ExecutePublicWorkflow(request, routeName);
            return (response.StatusCode, await ReadBodyAsync(response));
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

            // Restore the loader singleton to whatever the environment now dictates so
            // the next test class is not affected by this test's config.
            SecureConfigLoader.Reload();
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
