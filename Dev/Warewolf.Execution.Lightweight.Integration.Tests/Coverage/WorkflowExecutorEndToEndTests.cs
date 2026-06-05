/*
 * End-to-end coverage tests for Warewolf.Execution.Lightweight.WorkflowExecutor.
 *
 * Why this file exists
 * --------------------
 * The merged Cobertura snapshot at coverage/merged/all_merged.cobertura.xml shows
 * WorkflowExecutor sitting at ~0.8% line coverage (479 uncovered of 483).  The
 * existing WorkflowExecutorTests in CoreInfrastructureTests.cs only drive the
 * argument-validation guards (null request, missing file, etc.).  Nothing in the
 * unit suite actually loads a real .bite workflow file through
 *
 *     Read → ExtractWorkflowParts → LoadDynamicActivity → ActivityParser.Parse
 *           → BuildDataObject → ExecuteActivityChain → ExtractPayload
 *
 * That single success path also drives big chunks of ActivityParser,
 * DsfNativeActivity, DsfMultiAssignActivity, DsfDecision, WarewolfDataEvaluation
 * and the lightweight environment plumbing, which are all in the top-30
 * "uncovered LOC" hotspot list from LocalCoverage.md.
 *
 * Strategy
 * --------
 * Locate the canonical "Hello World.bite" workflow shipped under
 *   <repo>/Dev/Resources - Release/Resources/Hello World.bite
 * by walking up from the test assembly's BaseDirectory.  Each test copies the
 * workflow into a fresh temp directory (so the DynamicActivity cache key — the
 * absolute file path — is unique per test and we don't share cached compiled
 * activities across cases that need different runtime configuration).
 *
 * Tests are marked Inconclusive (not Failed) when the bite file cannot be found
 * — this mirrors the pattern used by SecurityHttpTests.cs so the suite still
 * runs cleanly in environments where the Resources tree is not deployed
 * (e.g. Bin/ServerTests-only CI legs).
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dev2.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using Warewolf.Execution.Lightweight.Logging;
using Warewolf.Execution.Lightweight.Models;

namespace Warewolf.Execution.Lightweight.Integration.Tests.Coverage
{
    [TestClass]
    [TestCategory("Unit_Coverage")]
    public class WorkflowExecutorEndToEndTests
    {
        // ─────────────────────────────────────────────────────────────────
        // Test fixtures
        // ─────────────────────────────────────────────────────────────────

        sealed class NoOpLogger : IExecutionLogger
        {
            public void LogDebug(string message, Guid executionId) { }
            public void LogDebug(string message, Exception exception, Guid executionId) { }
            public void LogInfo(string message, Guid executionId) { }
            public void LogInfo(string message, Exception exception, Guid executionId) { }
            public void LogInfo(string message) { }
            public void LogWarning(string message, Guid executionId) { }
            public void LogWarning(string message, Exception exception, Guid executionId) { }
            public void LogError(string message, Guid executionId) { }
            public void LogError(string activityName, Exception ex, Guid executionId) { }
            public void LogError(Exception ex, string log) { }
            public void LogFatal(string message, Guid executionId) { }
            public void LogFatal(string message, Exception exception, Guid executionId) { }
            public void LogTrace(string message, Guid executionId) { }
            public void LogTrace(string message, Exception exception, Guid executionId) { }
        }

        static IWorkflowExecutor CreateExecutor() => new WorkflowExecutor(new NoOpLogger());

        /// <summary>
        /// Walks parents of the test assembly's base directory looking for the
        /// "Hello World.bite" file under "Resources - Release\Resources\".
        /// Returns null when nothing is found.
        /// </summary>
        static string? FindHelloWorldBite()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            for (var hops = 0; dir != null && hops < 10; hops++, dir = dir.Parent)
            {
                var candidate = Path.Combine(
                    dir.FullName,
                    "Resources - Release", "Resources", "Hello World.bite");
                if (File.Exists(candidate)) return candidate;

                // Also check under a "Dev" child (running from repo root layout)
                var devCandidate = Path.Combine(
                    dir.FullName,
                    "Dev", "Resources - Release", "Resources", "Hello World.bite");
                if (File.Exists(devCandidate)) return devCandidate;
            }
            return null;
        }

        /// <summary>
        /// Copies the canonical Hello World bite file into a freshly-created
        /// temporary directory and returns the new file path.  The unique
        /// directory ensures each test gets its own DynamicActivity cache slot
        /// (cache key is the normalised file path) and avoids cross-test bleed
        /// of any source-loader indexing state.
        /// </summary>
        static string CopyHelloWorldToTemp()
        {
            var source = FindHelloWorldBite();
            if (source == null)
            {
                Assert.Inconclusive(
                    "Hello World.bite not found in 'Resources - Release/Resources/' " +
                    "under any ancestor of " + AppContext.BaseDirectory +
                    ". Deploy the example workflow tree or run from a checkout to enable this test.");
            }

            var dir = Path.Combine(Path.GetTempPath(), "wf-exec-e2e-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            var dest = Path.Combine(dir, "Hello World.bite");
            File.Copy(source!, dest);
            return dest;
        }

        static async Task<string> ReadPayloadAsync(WorkflowExecutionResult result)
        {
            Assert.IsNotNull(result.PayloadWriter, "Successful executions must produce a PayloadWriter");
            using var ms = new MemoryStream();
            await result.PayloadWriter(ms, CancellationToken.None);
            return Encoding.UTF8.GetString(ms.ToArray());
        }

        // ─────────────────────────────────────────────────────────────────
        // 1. Happy path: JSON emission with no inputs.
        //    Drives the full pipeline end-to-end.
        // ─────────────────────────────────────────────────────────────────

        [TestMethod]
        public async Task Execute_HelloWorld_JsonReturnType_ProducesJsonPayload()
        {
            var path = CopyHelloWorldToTemp();
            try
            {
                var executor = CreateExecutor();
                var request  = new WorkflowExecutionRequest
                {
                    WorkflowFilePath = path,
                    ReturnType       = EmitionTypes.JSON,
                    // Hello World references [[Name]] from its DataList; supply a value so the
                    // FlowDecision + DotNetMultiAssign path can resolve the variable.
                    InputParameters  = new Dictionary<string, string> { ["Name"] = "Tester" }
                };

                var result = executor.Execute(request);

                Assert.IsTrue(result.IsSuccess,
                    "Expected Hello World to succeed. Errors: " + string.Join("; ", result.Errors));
                Assert.AreNotEqual(Guid.Empty, result.ExecutionId, "ExecutionId must be assigned");
                Assert.IsTrue(result.Duration > TimeSpan.Zero, "Duration must be measured");
                Assert.IsTrue(result.StartTime <= result.EndTime, "StartTime must precede EndTime");

                var payload = await ReadPayloadAsync(result);
                Assert.IsFalse(string.IsNullOrWhiteSpace(payload),
                    "Payload must be non-empty for a successful workflow execution");
            }
            finally
            {
                TryDeleteParent(path);
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // 2. Input parameters round-trip through the execution environment.
        // ─────────────────────────────────────────────────────────────────

        [TestMethod]
        public async Task Execute_HelloWorld_WithNameInput_IncludesNameInOutput()
        {
            var path = CopyHelloWorldToTemp();
            try
            {
                var executor = CreateExecutor();
                var request  = new WorkflowExecutionRequest
                {
                    WorkflowFilePath = path,
                    ReturnType       = EmitionTypes.JSON,
                    InputParameters  = new Dictionary<string, string> { ["Name"] = "Warewolf" }
                };

                var result = executor.Execute(request);

                Assert.IsTrue(result.IsSuccess,
                    "Expected Hello World with input to succeed. Errors: " + string.Join("; ", result.Errors));

                var payload = await ReadPayloadAsync(result);
                Assert.IsFalse(string.IsNullOrWhiteSpace(payload), "Payload must be non-empty");

                // Hello World concatenates "Hello " + [[Name]] into [[Message]].  The exact JSON
                // shape varies with the DataList output mapping but the Name value should appear
                // in the output payload somewhere.
                StringAssert.Contains(payload, "Warewolf",
                    "Expected the input 'Name' value to round-trip into the JSON output. Payload: " + payload);
            }
            finally
            {
                TryDeleteParent(path);
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // 3. OPENAPI emission short-circuits before activity execution and
        //    produces a valid OpenAPI 3.0.1 document.
        // ─────────────────────────────────────────────────────────────────

        [TestMethod]
        public async Task Execute_HelloWorld_OpenApiReturnType_ProducesValidSpec()
        {
            var path = CopyHelloWorldToTemp();
            try
            {
                var executor = CreateExecutor();
                var request  = new WorkflowExecutionRequest
                {
                    WorkflowFilePath = path,
                    WorkflowName     = "Hello World",
                    ReturnType       = EmitionTypes.OPENAPI,
                    WebServerUri     = new Uri("https://localhost:7071/Public/Hello%20World.api")
                };

                var result = executor.Execute(request);

                Assert.IsTrue(result.IsSuccess,
                    "OPENAPI emission must succeed. Errors: " + string.Join("; ", result.Errors));
                Assert.AreEqual("application/json", result.ContentType,
                    "OPENAPI emission must declare application/json content type");

                var spec = await ReadPayloadAsync(result);
                var parsed = JObject.Parse(spec);
                Assert.IsNotNull(parsed["openapi"], "OpenAPI document must contain an 'openapi' version field");
                StringAssert.StartsWith(parsed["openapi"]!.Value<string>(), "3.",
                    "Expected an OpenAPI 3.x document. Got: " + parsed["openapi"]);
            }
            finally
            {
                TryDeleteParent(path);
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // 4. Debug mode populates the DebugStates tree and reshapes the
        //    payload into { hasErrors, errors, debugStates }.
        // ─────────────────────────────────────────────────────────────────

        [TestMethod]
        public async Task Execute_HelloWorld_DebugMode_PopulatesDebugStatesTree()
        {
            var path = CopyHelloWorldToTemp();
            try
            {
                var executor = CreateExecutor();
                var request  = new WorkflowExecutionRequest
                {
                    WorkflowFilePath = path,
                    ReturnType       = EmitionTypes.JSON,
                    IsDebug          = true,
                    InputParameters  = new Dictionary<string, string> { ["Name"] = "DebugTester" }
                };

                var result = executor.Execute(request);

                Assert.IsTrue(result.IsSuccess,
                    "Debug-mode execution must succeed. Errors: " + string.Join("; ", result.Errors));
                Assert.IsNotNull(result.DebugStates, "DebugStates list must be initialised in debug mode");
                Assert.IsTrue(result.DebugStates.Count > 0,
                    "Debug mode should emit at least one DebugState (Start/End markers + activity states)");

                var payload = await ReadPayloadAsync(result);
                var parsed  = JObject.Parse(payload);

                Assert.IsNotNull(parsed["debugStates"], "Debug payload must contain a 'debugStates' field");
                Assert.IsNotNull(parsed["hasErrors"], "Debug payload must contain a 'hasErrors' field");
                Assert.IsNotNull(parsed["errors"], "Debug payload must contain an 'errors' field");
                Assert.IsFalse(parsed["hasErrors"]!.Value<bool>(),
                    "Hello World debug execution should not report errors. Payload: " + payload);
            }
            finally
            {
                TryDeleteParent(path);
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // 5. Empty XamlDefinition path: build a malformed .bite that has a
        //    Service shell but no <XamlDefinition>, ensure the executor
        //    reports a graceful "No XamlDefinition" failure rather than
        //    throwing.  Drives the ExtractWorkflowParts → null XAML branch.
        // ─────────────────────────────────────────────────────────────────

        [TestMethod]
        public void Execute_BiteWithoutXamlDefinition_ReturnsXamlFailure()
        {
            var dir = Path.Combine(Path.GetTempPath(), "wf-exec-e2e-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            var path = Path.Combine(dir, "NoXaml.bite");
            File.WriteAllText(path,
                "<Service ID=\"acb75027-ddeb-47d7-814e-a54c37247ec1\" Name=\"NoXaml\">" +
                "  <DataList />" +
                "  <Action Name=\"InvokeWorkflow\" Type=\"Workflow\" />" +
                "</Service>");
            try
            {
                var executor = CreateExecutor();
                var result   = executor.Execute(new WorkflowExecutionRequest
                {
                    WorkflowFilePath = path,
                    ReturnType       = EmitionTypes.JSON
                });

                Assert.IsFalse(result.IsSuccess, "Workflow without XamlDefinition must not succeed");
                Assert.IsTrue(result.Errors.Count > 0, "Must report at least one error");
                Assert.IsTrue(result.Errors.Any(e =>
                        e.IndexOf("XamlDefinition", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        e.IndexOf("xaml", StringComparison.OrdinalIgnoreCase) >= 0),
                    "Error should mention the missing XAML. Got: " + string.Join("; ", result.Errors));
            }
            finally
            {
                TryDeleteParent(path);
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // 6. Malformed bite XML: feed garbage that fails ToXElement and
        //    confirm the catch-all path returns a failure result with an
        //    error message instead of propagating the exception.
        // ─────────────────────────────────────────────────────────────────

        [TestMethod]
        public void Execute_MalformedBiteXml_ReturnsFailureWithErrorMessage()
        {
            var dir = Path.Combine(Path.GetTempPath(), "wf-exec-e2e-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            var path = Path.Combine(dir, "Malformed.bite");
            File.WriteAllText(path, "<<not valid xml at all>>");
            try
            {
                var executor = CreateExecutor();
                var result   = executor.Execute(new WorkflowExecutionRequest
                {
                    WorkflowFilePath = path,
                    ReturnType       = EmitionTypes.JSON
                });

                Assert.IsFalse(result.IsSuccess, "Malformed XML must not succeed");
                Assert.IsTrue(result.Errors.Count > 0, "Malformed XML must report an error");
                Assert.IsTrue(result.Duration >= TimeSpan.Zero, "Duration must be set even on failure");
            }
            finally
            {
                TryDeleteParent(path);
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // 7. String-overload of Execute(...) with a real workflow must
        //    delegate to the request overload and succeed.  Also exercises
        //    the executionId-logged branch of the simple-path overload.
        // ─────────────────────────────────────────────────────────────────

        [TestMethod]
        public async Task Execute_StringOverload_WithRealWorkflow_Succeeds()
        {
            var path = CopyHelloWorldToTemp();
            try
            {
                var executor = CreateExecutor();

                var result = executor.Execute(
                    path,
                    new Dictionary<string, string> { ["Name"] = "Overload" });

                Assert.IsTrue(result.IsSuccess,
                    "String overload should succeed for Hello World. Errors: " + string.Join("; ", result.Errors));

                var payload = await ReadPayloadAsync(result);
                StringAssert.Contains(payload, "Overload",
                    "String-overload execution should round-trip the Name input. Payload: " + payload);
            }
            finally
            {
                TryDeleteParent(path);
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // 8. Second execution of the same workflow file hits the
        //    DynamicActivity cache (GetOrLoadDynamicActivity).  We assert
        //    correctness — measuring the cache hit directly would require
        //    reflection, but the second call must still produce identical
        //    successful output.  This drives the cache-hit branch.
        // ─────────────────────────────────────────────────────────────────

        [TestMethod]
        public async Task Execute_HelloWorld_CalledTwice_BothSucceed()
        {
            var path = CopyHelloWorldToTemp();
            try
            {
                var executor = CreateExecutor();
                var request  = new WorkflowExecutionRequest
                {
                    WorkflowFilePath = path,
                    ReturnType       = EmitionTypes.JSON,
                    InputParameters  = new Dictionary<string, string> { ["Name"] = "Repeat" }
                };

                var first  = executor.Execute(request);
                var second = executor.Execute(request);

                Assert.IsTrue(first.IsSuccess,  "First execution must succeed");
                Assert.IsTrue(second.IsSuccess, "Second (cached-XAML) execution must succeed");

                var firstPayload  = await ReadPayloadAsync(first);
                var secondPayload = await ReadPayloadAsync(second);

                StringAssert.Contains(firstPayload,  "Repeat");
                StringAssert.Contains(secondPayload, "Repeat");
                Assert.AreNotEqual(first.ExecutionId, second.ExecutionId,
                    "Each Execute call must mint a fresh ExecutionId");
            }
            finally
            {
                TryDeleteParent(path);
            }
        }

        // ─────────────────────────────────────────────────────────────────
        // Cleanup helper — best-effort temp-directory removal.
        // ─────────────────────────────────────────────────────────────────

        static void TryDeleteParent(string filePath)
        {
            try
            {
                var dir = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
                    Directory.Delete(dir, recursive: true);
            }
            catch { /* best-effort */ }
        }
    }
}
