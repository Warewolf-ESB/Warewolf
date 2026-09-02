/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Unit tests for ExecuteWorkflowTool: name resolution/not-found, Execute permission gating
 *  (reusing ListWorkflowsTool's rule), inputs → RawInputPayload binding,
 *  and outputs/status/error/executionId shape for both a successful and a
 *  failing execution.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Warewolf.Execution.Lightweight.Auth;
using Warewolf.Execution.Lightweight.Auth.Models;
using Warewolf.Execution.Lightweight.Infrastructure;
using Warewolf.Execution.Lightweight.Mcp.ToolHandlers;
using Warewolf.Execution.Lightweight.Models;
using Warewolf.Execution.Lightweight.Tests.TestSupport;

namespace Warewolf.Execution.Lightweight.Tests.Mcp.ToolHandlers
{
    [TestClass]
    public class ExecuteWorkflowToolTests
    {
        private string _root = string.Empty;

        [TestInitialize]
        public void Setup() => _root = Directory.CreateDirectory(
            Path.Combine(Path.GetTempPath(), "ewf-tests-" + Guid.NewGuid().ToString("N"))).FullName;

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(_root))
            {
                Directory.Delete(_root, recursive: true);
            }
        }

        // ── Test doubles (matches GetWorkflowDefinitionToolTests' conventions) ─

        private sealed class StubAuthPolicyLoader : IWorkflowAuthPolicyLoader
        {
            public bool IsConfigEffective { get; set; }
            public int PolicyCount => 0;
            public PolicyLookupResult GetPolicy(string workflowName) => PolicyLookupResult.ConfigMissing();

            public Func<string, IEnumerable<string>, WorkflowPermission> EffectivePermissions { get; set; } =
                (_, _) => WorkflowPermission.None;

            public WorkflowPermission GetEffectivePermissions(string workflowName, IEnumerable<string> callerRoles) =>
                EffectivePermissions(workflowName, callerRoles);

            public void Reload() { }
        }

        /// <summary>
        /// Hand-written fake mirroring <c>ServiceBusWorkflowTriggerFunctionTests.FakeWorkflowExecutor</c>'s
        /// conventions. <see cref="Execute(WorkflowExecutionRequest)"/> records the request it was
        /// called with (so tests can assert <c>RawInputPayload</c>/<c>ExecutingPrincipal</c> binding)
        /// and returns whatever result the test configured.
        /// </summary>
        private sealed class FakeWorkflowExecutor : IWorkflowExecutor
        {
            private readonly Func<WorkflowExecutionRequest, WorkflowExecutionResult> _impl;
            public FakeWorkflowExecutor(Func<WorkflowExecutionRequest, WorkflowExecutionResult> impl) => _impl = impl;

            public int CallCount { get; private set; }
            public WorkflowExecutionRequest? LastRequest { get; private set; }

            public WorkflowExecutionResult Execute(string workflowFilePath, Dictionary<string, string>? inputs = null) =>
                throw new NotSupportedException("ExecuteWorkflowTool only calls the WorkflowExecutionRequest overload.");

            public WorkflowExecutionResult Execute(WorkflowExecutionRequest request)
            {
                CallCount++;
                LastRequest = request;
                return _impl(request);
            }
        }

        private static WorkflowClaimsPrincipal Principal(params string[] roles) =>
            new(new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.Name, "alice") }
                    .Concat(roles.Select(r => new Claim(ClaimTypes.Role, r))),
                "Bearer", ClaimTypes.Name, ClaimTypes.Role));

        /// <summary>
        /// Builds a <see cref="WorkflowExecutionResult"/> whose <see cref="WorkflowExecutionResult.PayloadWriter"/>
        /// mirrors <c>WorkflowExecutor.ExtractPayload</c>'s real side effect: populating
        /// <see cref="WorkflowExecutionResult.Outputs"/> the first time the writer runs (i.e. the first
        /// time something calls <see cref="WorkflowExecutionResult.ReadPayloadAsync"/>), exactly as
        /// <see cref="ExecuteWorkflowTool.Handle"/> relies on.
        /// </summary>
        private static WorkflowExecutionResult SuccessResult(Dictionary<string, object> outputs, string rawJson)
        {
            var result = new WorkflowExecutionResult
            {
                IsSuccess = true,
                ExecutionId = Guid.NewGuid(),
            };
            result.PayloadWriter = async (stream, ct) =>
            {
                result.Outputs = outputs;
                var bytes = Encoding.UTF8.GetBytes(rawJson);
                await stream.WriteAsync(bytes, 0, bytes.Length, ct);
            };
            return result;
        }

        private static WorkflowExecutionResult FailureResult(params string[] errors)
        {
            var result = new WorkflowExecutionResult
            {
                IsSuccess = false,
                ExecutionId = Guid.NewGuid(),
                Errors = errors.ToList(),
            };
            result.PayloadWriter = (stream, ct) => Task.CompletedTask;
            return result;
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private HostEnvironmentConfig HostConfig() => McpToolTestHostConfig.ForWorkflowsDirectory(_root);

        private void WriteWorkflow(string relativePath, string name, string comment = "", string dataListXml = "<DataList />")
        {
            var fullPath = Path.Combine(_root, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
            File.WriteAllText(fullPath,
                $"<Service Name=\"{name}\" ResourceType=\"WorkflowService\">" +
                $"<Comment>{comment}</Comment>{dataListXml}<Action Name=\"InvokeWorkflow\" Type=\"Workflow\">" +
                "<XamlDefinition></XamlDefinition></Action></Service>");
        }

        private static Task<ExecuteWorkflowResult> Handle(
            HostEnvironmentConfig hostConfig,
            IWorkflowAuthPolicyLoader authPolicyLoader,
            IWorkflowExecutor executor,
            ClaimsPrincipal? user,
            string name,
            JsonElement? inputs = null) =>
            ExecuteWorkflowTool.Handle(hostConfig, authPolicyLoader, executor, user, name, inputs, CancellationToken.None);

        private static FakeWorkflowExecutor NeverCalledExecutor() =>
            new(_ => throw new InvalidOperationException("Executor should not have been invoked."));

        // ── Tests: input validation / not-found / permissions ──────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_BlankName_Throws()
        {
            await Assert.ThrowsExceptionAsync<ModelContextProtocol.McpException>(() =>
                Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false }, NeverCalledExecutor(), null, "   "));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_UnknownName_Throws()
        {
            await Assert.ThrowsExceptionAsync<ModelContextProtocol.McpException>(() =>
                Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false }, NeverCalledExecutor(), null, "DoesNotExist"));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_NonWorkflowResourceType_ThrowsNotFound()
        {
            File.WriteAllText(Path.Combine(_root, "NotAWorkflow.bite"),
                "<Service Name=\"NotAWorkflow\" ResourceType=\"Source\"></Service>");

            await Assert.ThrowsExceptionAsync<ModelContextProtocol.McpException>(() =>
                Handle(HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false }, NeverCalledExecutor(), null, "NotAWorkflow"));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_SecureConfigEffective_NoExecutePermission_ThrowsPermissionDenied()
        {
            WriteWorkflow("Secret.bite", "Secret");
            var loader = new StubAuthPolicyLoader
            {
                IsConfigEffective = true,
                EffectivePermissions = (_, _) => WorkflowPermission.View, // View, not Execute
            };

            await Assert.ThrowsExceptionAsync<ModelContextProtocol.McpException>(() =>
                Handle(HostConfig(), loader, NeverCalledExecutor(), null, "Secret"));
        }

        // ── Tests: successful execution ─────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_SecureConfigEffective_WithExecutePermission_Succeeds()
        {
            WriteWorkflow("Visible.bite", "Visible");
            var loader = new StubAuthPolicyLoader
            {
                IsConfigEffective = true,
                EffectivePermissions = (path, _) =>
                    path.Equals("Visible", StringComparison.OrdinalIgnoreCase)
                        ? WorkflowPermission.Execute
                        : WorkflowPermission.None,
            };
            var executor = new FakeWorkflowExecutor(_ =>
                SuccessResult(new Dictionary<string, object> { ["Message"] = "Hello" }, "{\"Message\":\"Hello\"}"));

            var result = await Handle(HostConfig(), loader, executor, Principal("Developers"), "Visible");

            Assert.AreEqual("success", result.Status);
            Assert.IsNull(result.Error);
            Assert.AreEqual("Hello", result.Outputs.GetProperty("Message").GetString());
            Assert.AreEqual(1, executor.CallCount);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_OpenAccessMode_NoPermissionCheckRequired_Succeeds()
        {
            WriteWorkflow("Open.bite", "Open");
            var executor = new FakeWorkflowExecutor(_ =>
                SuccessResult(new Dictionary<string, object>(), "{}"));

            var result = await Handle(
                HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false }, executor, null, "Open");

            Assert.AreEqual("success", result.Status);
            Assert.AreEqual(1, executor.CallCount);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_ExecutionId_ReturnedAsGuidString()
        {
            WriteWorkflow("WithId.bite", "WithId");
            var expectedId = Guid.NewGuid();
            var executor = new FakeWorkflowExecutor(_ =>
            {
                var result = SuccessResult(new Dictionary<string, object>(), "{}");
                result.ExecutionId = expectedId;
                return result;
            });

            var result = await Handle(
                HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false }, executor, null, "WithId");

            Assert.AreEqual(expectedId.ToString(), result.ExecutionId);
        }

        // ── Tests: failing execution ─────────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_ExecutorFailure_ReturnsErrorStatusWithJoinedErrors()
        {
            WriteWorkflow("Failing.bite", "Failing");
            var executor = new FakeWorkflowExecutor(_ => FailureResult("boom", "second error"));

            var result = await Handle(
                HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false }, executor, null, "Failing");

            Assert.AreEqual("error", result.Status);
            Assert.AreEqual("boom; second error", result.Error);
        }

        // ── Tests: inputs → RawInputPayload binding ───────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_InputsProvided_FlowsToRawInputPayloadVerbatim()
        {
            WriteWorkflow("Inputs.bite", "Inputs");
            var executor = new FakeWorkflowExecutor(_ => SuccessResult(new Dictionary<string, object>(), "{}"));
            using var inputsDoc = JsonDocument.Parse("{\"Name\":\"John\",\"Age\":30}");

            await Handle(
                HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false }, executor, null, "Inputs",
                inputsDoc.RootElement.Clone());

            Assert.IsNotNull(executor.LastRequest);
            Assert.AreEqual("{\"Name\":\"John\",\"Age\":30}", executor.LastRequest!.RawInputPayload);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_NoInputsProvided_RawInputPayloadNotSet()
        {
            WriteWorkflow("NoInputs.bite", "NoInputs");
            var executor = new FakeWorkflowExecutor(_ => SuccessResult(new Dictionary<string, object>(), "{}"));

            await Handle(
                HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false }, executor, null, "NoInputs");

            Assert.IsNotNull(executor.LastRequest);
            Assert.IsNull(executor.LastRequest!.RawInputPayload);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Handle_ExecutingPrincipal_FlowsFromUserToRequest()
        {
            WriteWorkflow("WithPrincipal.bite", "WithPrincipal");
            var executor = new FakeWorkflowExecutor(_ => SuccessResult(new Dictionary<string, object>(), "{}"));
            var principal = Principal("Developers");

            await Handle(
                HostConfig(), new StubAuthPolicyLoader { IsConfigEffective = false }, executor, principal, "WithPrincipal");

            Assert.AreSame(principal, executor.LastRequest!.ExecutingPrincipal);
        }
    }
}
