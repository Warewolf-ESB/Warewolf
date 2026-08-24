/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Unit tests for ServiceBusWorkflowTriggerFunction — the secure, in-process Service
 *  Bus workflow trigger (Model A of Spec-Secure-ServiceBus-Triggered-Execution.md).
 *
 *  Test-double strategy (no mocking library — matches the rest of this test project):
 *   - ServiceBusMessageActions has a protected parameterless constructor and every
 *     settlement method is virtual/non-final, so FakeServiceBusMessageActions below
 *     subclasses it directly and records Complete/DeadLetter calls without a real
 *     SettlementClient.
 *   - ServiceBusReceivedMessage instances are built via
 *     Azure.Messaging.ServiceBus.ServiceBusModelFactory.ServiceBusReceivedMessage(...),
 *     whose optional-parameters overload lets every field but the ones under test be
 *     left at its default.
 *   - IWorkflowPolicyMatcher / IWorkflowExecutor get small hand-written fakes.
 *   - ServiceBusReplayAndResultStore is the REAL production type, backed by
 *     Hangfire.MemoryStorage via its internal JobStorage test seam (mirrors
 *     ResumptionExecutorTests' convention for the suspend/resume store).
 *
 *  Coverage split, matching Run(...)'s early-exit branches vs. the extracted
 *  ProcessAuthenticatedMessageAsync(...) (see that method's XML doc for why it was
 *  pulled out): the pre-authentication branches (malformed JSON, missing 'workflow',
 *  duplicate correlation id, missing Authorization property, validator not configured)
 *  are exercised through Run(...) itself — none of them reach the network-dependent
 *  token validator. The post-authentication branches (jti replay, policy
 *  denied/allowed, execution success/failure/exception) are exercised by calling
 *  ProcessAuthenticatedMessageAsync(...) directly with a hand-built ClaimsIdentity,
 *  bypassing EntraBearerTokenValidator.ValidateAsync's live OIDC metadata call —
 *  the same network-avoidance convention BearerTokenPrincipalParserTests already
 *  established for the HTTP path.
 */

using System.Security.Claims;
using Azure.Messaging.ServiceBus;
using Hangfire.MemoryStorage;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Execution.Lightweight.Auth;
using Warewolf.Execution.Lightweight.Auth.Models;
using Warewolf.Execution.Lightweight.Auth.Parsers;
using Warewolf.Execution.Lightweight.Functions;
using Warewolf.Execution.Lightweight.Infrastructure;
using Warewolf.Execution.Lightweight.Models;
using Warewolf.Execution.Lightweight.Security;

namespace Warewolf.Execution.Lightweight.Tests.Functions;

[TestClass]
public class ServiceBusWorkflowTriggerFunctionTests
{
    // ── Test doubles ─────────────────────────────────────────────────────────────

    private sealed class FakeServiceBusMessageActions : ServiceBusMessageActions
    {
        public int CompleteCalls { get; private set; }
        public int DeadLetterCalls { get; private set; }
        public int AbandonCalls { get; private set; }
        public string? LastDeadLetterReason { get; private set; }
        public string? LastDeadLetterDescription { get; private set; }

        public override Task CompleteMessageAsync(ServiceBusReceivedMessage message, CancellationToken cancellationToken = default)
        {
            CompleteCalls++;
            return Task.CompletedTask;
        }

        public override Task AbandonMessageAsync(
            ServiceBusReceivedMessage message,
            IDictionary<string, object>? propertiesToModify = null,
            CancellationToken cancellationToken = default)
        {
            AbandonCalls++;
            return Task.CompletedTask;
        }

        public override Task DeadLetterMessageAsync(
            ServiceBusReceivedMessage message,
            Dictionary<string, object>? propertiesToModify = null,
            string? deadLetterReason = null,
            string? deadLetterErrorDescription = null,
            CancellationToken cancellationToken = default)
        {
            DeadLetterCalls++;
            LastDeadLetterReason = deadLetterReason;
            LastDeadLetterDescription = deadLetterErrorDescription;
            return Task.CompletedTask;
        }
    }

    private sealed class FakePolicyMatcher : IWorkflowPolicyMatcher
    {
        private readonly PolicyMatchResult _result;
        public FakePolicyMatcher(PolicyMatchResult result) => _result = result;

        public PolicyMatchResult Evaluate(
            string workflowName,
            WorkflowClaimsPrincipal principal,
            WorkflowPermission requiredPermissions = WorkflowPermission.View | WorkflowPermission.Execute) => _result;
    }

    private sealed class FakeWorkflowExecutor : IWorkflowExecutor
    {
        private readonly Func<WorkflowExecutionRequest, WorkflowExecutionResult> _impl;
        public FakeWorkflowExecutor(Func<WorkflowExecutionRequest, WorkflowExecutionResult> impl) => _impl = impl;
        public int CallCount { get; private set; }
        public WorkflowExecutionRequest? LastRequest { get; private set; }

        public WorkflowExecutionResult Execute(string workflowFilePath, Dictionary<string, string>? inputs = null) =>
            throw new NotSupportedException("Trigger function only calls the WorkflowExecutionRequest overload.");

        public WorkflowExecutionResult Execute(WorkflowExecutionRequest request)
        {
            CallCount++;
            LastRequest = request;
            return _impl(request);
        }
    }

    // ── Fixture helpers ──────────────────────────────────────────────────────────

    private static ServiceBusReceivedMessage NewMessage(
        string body,
        string? correlationId = null,
        string? messageId = null,
        string? authorization = "Bearer test-token",
        string? jtiProperty = null)
    {
        var properties = new Dictionary<string, object>();
        if (authorization is not null)
        {
            properties["Authorization"] = authorization;
        }
        if (jtiProperty is not null)
        {
            properties["jti"] = jtiProperty;
        }

        return ServiceBusModelFactory.ServiceBusReceivedMessage(
            body: BinaryData.FromString(body),
            messageId: messageId ?? Guid.NewGuid().ToString(),
            correlationId: correlationId,
            properties: properties);
    }

    private static ClaimsIdentity NewUserIdentity(string userName = "alice@example.com", string jti = "jti-001") =>
        new(new[]
        {
            new Claim(ClaimTypes.Name, userName),
            new Claim(AuthConstants.Scope, "user_impersonation"),
            new Claim("jti", jti),
        }, "Bearer");

    private static ServiceBusWorkflowTriggerFunction NewSut(
        IWorkflowPolicyMatcher? policyMatcher = null,
        IWorkflowExecutor? executor = null,
        IServiceBusReplayAndResultStore? store = null,
        ServiceBusEntraAuthOptions? authOptions = null) =>
        new(
            new EntraBearerTokenValidator(authOptions ?? new ServiceBusEntraAuthOptions()),
            policyMatcher ?? new FakePolicyMatcher(PolicyMatchResult.Allow()),
            executor ?? new FakeWorkflowExecutor(_ => new WorkflowExecutionResult { IsSuccess = true, Payload = "{}" }),
            store ?? new ServiceBusReplayAndResultStore(new MemoryStorage()),
            new AuditLogger(NullLogger<AuditLogger>.Instance),
            HostEnvironmentConfig.Load(),
            NullLogger<ServiceBusWorkflowTriggerFunction>.Instance);

    // ── Run(...) — pre-authentication branches (no network dependency) ──────────

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task Run_MalformedJson_DeadLettersAsMalformed_NoExecution()
    {
        var executor = new FakeWorkflowExecutor(_ => throw new InvalidOperationException("must not execute"));
        var store = new ServiceBusReplayAndResultStore(new MemoryStorage());
        var sut = NewSut(executor: executor, store: store);
        var actions = new FakeServiceBusMessageActions();
        var message = NewMessage("{ not valid json");

        await sut.Run(message, actions, CancellationToken.None);

        Assert.AreEqual(0, executor.CallCount);
        Assert.AreEqual(1, actions.DeadLetterCalls);
        Assert.AreEqual(0, actions.CompleteCalls);
        Assert.AreEqual("Malformed", actions.LastDeadLetterReason);
        store.TryGetResult(message.MessageId, out var result);
        Assert.AreEqual(ServiceBusTriggerStatus.Malformed, result!.Status);
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task Run_MissingWorkflowField_DeadLettersAsMalformed()
    {
        var sut = NewSut();
        var actions = new FakeServiceBusMessageActions();
        var message = NewMessage("{\"inputs\":{}}");

        await sut.Run(message, actions, CancellationToken.None);

        Assert.AreEqual(1, actions.DeadLetterCalls);
        Assert.AreEqual("Malformed", actions.LastDeadLetterReason);
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task Run_DuplicateCorrelationId_CompletesWithoutReExecuting()
    {
        var executor = new FakeWorkflowExecutor(_ => new WorkflowExecutionResult { IsSuccess = true, Payload = "{}" });
        var store = new ServiceBusReplayAndResultStore(new MemoryStorage());
        store.SaveResult(new ServiceBusTriggerResult
        {
            CorrelationId = "dup-corr",
            Status = ServiceBusTriggerStatus.Succeeded,
            Workflow = "Hello World",
        });
        var sut = NewSut(executor: executor, store: store);
        var actions = new FakeServiceBusMessageActions();
        var message = NewMessage("{\"workflow\":\"Hello World\",\"correlationId\":\"dup-corr\"}", correlationId: "dup-corr");

        await sut.Run(message, actions, CancellationToken.None);

        Assert.AreEqual(0, executor.CallCount, "A duplicate delivery must not re-execute the workflow.");
        Assert.AreEqual(1, actions.CompleteCalls);
        Assert.AreEqual(0, actions.DeadLetterCalls);
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task Run_ClaimAlreadyHeldByInFlightDelivery_AbandonsWithoutExecutingOrSettling()
    {
        // Simulates a redelivery landing while a first delivery is still mid-execution
        // (claimed, no result saved yet) - the exact race the plain TryGetResult check used
        // to miss. Must not execute a second time, and must not complete/dead-letter either
        // (that's for Service Bus's own lock-expiry/redelivery timing to resolve).
        var executor = new FakeWorkflowExecutor(_ => throw new InvalidOperationException("must not execute"));
        var store = new ServiceBusReplayAndResultStore(new MemoryStorage());
        Assert.IsTrue(store.TryClaim("corr-in-flight"), "Pre-claim to simulate the first, still-executing delivery.");
        var sut = NewSut(executor: executor, store: store);
        var actions = new FakeServiceBusMessageActions();
        var message = NewMessage("{\"workflow\":\"Hello World\",\"correlationId\":\"corr-in-flight\"}", correlationId: "corr-in-flight");

        await sut.Run(message, actions, CancellationToken.None);

        Assert.AreEqual(0, executor.CallCount, "A delivery racing an in-flight claim must not execute the workflow.");
        Assert.AreEqual(0, actions.CompleteCalls);
        Assert.AreEqual(0, actions.DeadLetterCalls);
        Assert.AreEqual(1, actions.AbandonCalls, "Must abandon rather than settle, leaving redelivery timing to Service Bus.");
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task Run_ConcurrentDeliveriesOfSameCorrelationId_ExecutesExactlyOnce()
    {
        // Real concurrency, not a pre-seeded claim: two deliveries of the same message race
        // through Run(...) at once. The first blocks mid-execution (simulating a slow
        // workflow) until the second delivery has already been dispatched and observed the
        // claim - proving TryClaim, not just TryGetResult, is what prevents the second
        // execution.
        var firstCallStarted = new TaskCompletionSource();
        var releaseFirstCall = new TaskCompletionSource();
        var executor = new FakeWorkflowExecutor(_ =>
        {
            firstCallStarted.TrySetResult();
            releaseFirstCall.Task.GetAwaiter().GetResult();
            return new WorkflowExecutionResult { IsSuccess = true, Payload = "{}" };
        });
        var store = new ServiceBusReplayAndResultStore(new MemoryStorage());
        var sut = NewSut(executor: executor, store: store);
        var actions1 = new FakeServiceBusMessageActions();
        var actions2 = new FakeServiceBusMessageActions();
        var message1 = NewMessage("{\"workflow\":\"Hello World\",\"correlationId\":\"corr-concurrent\"}", correlationId: "corr-concurrent");
        var message2 = NewMessage("{\"workflow\":\"Hello World\",\"correlationId\":\"corr-concurrent\"}", correlationId: "corr-concurrent");

        var firstRun = sut.Run(message1, actions1, CancellationToken.None);
        await firstCallStarted.Task; // first delivery is now mid-execution, claim held
        var secondRun = sut.Run(message2, actions2, CancellationToken.None);
        await secondRun; // the racing delivery must resolve (abandon) without waiting on the first

        Assert.AreEqual(1, executor.CallCount, "Only the first delivery may execute the workflow.");
        Assert.AreEqual(0, actions2.CompleteCalls);
        Assert.AreEqual(0, actions2.DeadLetterCalls);
        Assert.AreEqual(1, actions2.AbandonCalls, "The racing delivery must be abandoned, not settled.");

        releaseFirstCall.SetResult();
        await firstRun;
        Assert.AreEqual(1, actions1.CompleteCalls, "The first delivery completes normally once its execution finishes.");
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task Run_MissingAuthorizationProperty_DeadLettersAsInvalidToken()
    {
        var sut = NewSut();
        var actions = new FakeServiceBusMessageActions();
        var message = NewMessage("{\"workflow\":\"Hello World\"}", authorization: null);

        await sut.Run(message, actions, CancellationToken.None);

        Assert.AreEqual(1, actions.DeadLetterCalls);
        Assert.AreEqual("InvalidToken", actions.LastDeadLetterReason);
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task Run_ServiceBusAuthNotConfigured_DeadLettersAsInvalidToken_WithoutNetworkCall()
    {
        // Default ServiceBusEntraAuthOptions() has no TenantId/Audience → IsEnabled == false,
        // so this must fail closed BEFORE any live OIDC metadata call is attempted.
        var sut = NewSut(authOptions: new ServiceBusEntraAuthOptions());
        var actions = new FakeServiceBusMessageActions();
        var message = NewMessage("{\"workflow\":\"Hello World\"}");

        await sut.Run(message, actions, CancellationToken.None);

        Assert.AreEqual(1, actions.DeadLetterCalls);
        Assert.AreEqual("InvalidToken", actions.LastDeadLetterReason);
        StringAssert.Contains(actions.LastDeadLetterDescription, "not configured");
    }

    // ── ProcessAuthenticatedMessageAsync(...) — post-authentication branches ────

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task ProcessAuthenticated_JtiReplay_DeadLettersAsInvalidToken_NoExecution()
    {
        var executor = new FakeWorkflowExecutor(_ => new WorkflowExecutionResult { IsSuccess = true, Payload = "{}" });
        var store = new ServiceBusReplayAndResultStore(new MemoryStorage());
        store.TryRegisterJti("jti-repeat"); // pre-seed as already used
        var sut = NewSut(executor: executor, store: store);
        var actions = new FakeServiceBusMessageActions();
        var payload = new ServiceBusWorkflowMessage { Workflow = "Hello World" };
        var message = NewMessage("{\"workflow\":\"Hello World\"}", jtiProperty: "jti-repeat");
        var identity = NewUserIdentity(jti: "jti-repeat");

        await sut.ProcessAuthenticatedMessageAsync(identity, payload, "corr-replay", message, actions, DateTimeOffset.UtcNow, CancellationToken.None);

        Assert.AreEqual(0, executor.CallCount);
        Assert.AreEqual(1, actions.DeadLetterCalls);
        Assert.AreEqual("InvalidToken", actions.LastDeadLetterReason);
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task ProcessAuthenticated_PolicyForbidden_DeadLettersAsDenied_NoExecution()
    {
        var executor = new FakeWorkflowExecutor(_ => throw new InvalidOperationException("must not execute"));
        var policy = new FakePolicyMatcher(PolicyMatchResult.DenyGroup("Caller is not a member of any allowed group."));
        var store = new ServiceBusReplayAndResultStore(new MemoryStorage());
        var sut = NewSut(policyMatcher: policy, executor: executor, store: store);
        var actions = new FakeServiceBusMessageActions();
        var payload = new ServiceBusWorkflowMessage { Workflow = "Admin Only" };
        var message = NewMessage("{\"workflow\":\"Admin Only\"}");
        var identity = NewUserIdentity(jti: "jti-denied");

        await sut.ProcessAuthenticatedMessageAsync(identity, payload, "corr-denied", message, actions, DateTimeOffset.UtcNow, CancellationToken.None);

        Assert.AreEqual(0, executor.CallCount);
        Assert.AreEqual(1, actions.DeadLetterCalls);
        Assert.AreEqual("Denied", actions.LastDeadLetterReason);
        store.TryGetResult("corr-denied", out var result);
        Assert.AreEqual(ServiceBusTriggerStatus.Denied, result!.Status);
        Assert.AreEqual("Caller is not a member of any allowed group.", result.Error);
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task ProcessAuthenticated_ConfigMissingDeny_DeadLettersAsDenied()
    {
        var policy = new FakePolicyMatcher(PolicyMatchResult.DenyConfigMissing("secure.config is absent."));
        var sut = NewSut(policyMatcher: policy);
        var actions = new FakeServiceBusMessageActions();
        var payload = new ServiceBusWorkflowMessage { Workflow = "Hello World" };
        var message = NewMessage("{\"workflow\":\"Hello World\"}");
        var identity = NewUserIdentity(jti: "jti-config-missing");

        await sut.ProcessAuthenticatedMessageAsync(identity, payload, "corr-config-missing", message, actions, DateTimeOffset.UtcNow, CancellationToken.None);

        Assert.AreEqual(1, actions.DeadLetterCalls);
        Assert.AreEqual("Denied", actions.LastDeadLetterReason);
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task ProcessAuthenticated_ExecutionSucceeds_CompletesAndSavesOutputs()
    {
        var executor = new FakeWorkflowExecutor(_ => new WorkflowExecutionResult { IsSuccess = true, Payload = "{\"Greeting\":\"Hi\"}" });
        var store = new ServiceBusReplayAndResultStore(new MemoryStorage());
        var sut = NewSut(executor: executor, store: store);
        var actions = new FakeServiceBusMessageActions();
        var payload = new ServiceBusWorkflowMessage { Workflow = "Hello World" };
        var message = NewMessage("{\"workflow\":\"Hello World\"}");
        var identity = NewUserIdentity(userName: "alice@example.com", jti: "jti-success");

        await sut.ProcessAuthenticatedMessageAsync(identity, payload, "corr-success", message, actions, DateTimeOffset.UtcNow, CancellationToken.None);

        Assert.AreEqual(1, executor.CallCount);
        Assert.AreEqual(1, actions.CompleteCalls);
        Assert.AreEqual(0, actions.DeadLetterCalls);
        store.TryGetResult("corr-success", out var result);
        Assert.AreEqual(ServiceBusTriggerStatus.Succeeded, result!.Status);
        Assert.AreEqual("alice@example.com", result.Caller);
        Assert.AreEqual("{\"Greeting\":\"Hi\"}", result.Outputs);
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task ProcessAuthenticated_ExecutionSucceeds_StampsExecutingPrincipalOnRequest()
    {
        var executor = new FakeWorkflowExecutor(_ => new WorkflowExecutionResult { IsSuccess = true, Payload = "{}" });
        var sut = NewSut(executor: executor);
        var actions = new FakeServiceBusMessageActions();
        var payload = new ServiceBusWorkflowMessage { Workflow = "Hello World" };
        var message = NewMessage("{\"workflow\":\"Hello World\"}");
        var identity = NewUserIdentity(userName: "alice@example.com", jti: "jti-principal");

        await sut.ProcessAuthenticatedMessageAsync(identity, payload, "corr-principal", message, actions, DateTimeOffset.UtcNow, CancellationToken.None);

        var stampedPrincipal = executor.LastRequest?.ExecutingPrincipal as WorkflowClaimsPrincipal;
        Assert.IsNotNull(stampedPrincipal);
        Assert.AreEqual("alice@example.com", stampedPrincipal!.CallerIdentity);
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task ProcessAuthenticated_ExecutionBusinessFailure_DeadLettersAsFailed_NotThrown()
    {
        var executor = new FakeWorkflowExecutor(_ => WorkflowExecutionResult.Failure("Activity 'Divide' failed: divide by zero."));
        var store = new ServiceBusReplayAndResultStore(new MemoryStorage());
        var sut = NewSut(executor: executor, store: store);
        var actions = new FakeServiceBusMessageActions();
        var payload = new ServiceBusWorkflowMessage { Workflow = "Hello World" };
        var message = NewMessage("{\"workflow\":\"Hello World\"}");
        var identity = NewUserIdentity(jti: "jti-failure");

        await sut.ProcessAuthenticatedMessageAsync(identity, payload, "corr-failure", message, actions, DateTimeOffset.UtcNow, CancellationToken.None);

        Assert.AreEqual(0, actions.CompleteCalls);
        Assert.AreEqual(1, actions.DeadLetterCalls);
        Assert.AreEqual("execution_failed", actions.LastDeadLetterReason);
        store.TryGetResult("corr-failure", out var result);
        Assert.AreEqual(ServiceBusTriggerStatus.Failed, result!.Status);
        StringAssert.Contains(result.Error, "divide by zero");
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task ProcessAuthenticated_ExecutionTransientFailure_ThrowsInsteadOfDeadLettering_NoResultPersisted()
    {
        // OutOfMemoryException-under-cold-start signature, surfaced by WorkflowExecutor as
        // IsTransientFailure = true — must retry (throw), NOT dead-letter, and must NOT persist a
        // terminal result (a persisted "Failed" result would satisfy the dedupe check on the
        // Service-Bus-driven redelivery and complete it without ever re-executing).
        var executor = new FakeWorkflowExecutor(_ => WorkflowExecutionResult.TransientFailure("Insufficient memory to continue the execution of the program."));
        var store = new ServiceBusReplayAndResultStore(new MemoryStorage());
        var sut = NewSut(executor: executor, store: store);
        var actions = new FakeServiceBusMessageActions();
        var payload = new ServiceBusWorkflowMessage { Workflow = "Hello World" };
        var message = NewMessage("{\"workflow\":\"Hello World\"}");
        var identity = NewUserIdentity(jti: "jti-transient");

        await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            () => sut.ProcessAuthenticatedMessageAsync(identity, payload, "corr-transient", message, actions, DateTimeOffset.UtcNow, CancellationToken.None));

        Assert.AreEqual(0, actions.CompleteCalls);
        Assert.AreEqual(0, actions.DeadLetterCalls);
        Assert.IsFalse(store.TryGetResult("corr-transient", out _));
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task ProcessAuthenticated_ExecutionTransientFailure_ReleasesClaim_AllowingRetryToReExecute()
    {
        // The claim must not survive a transient failure - otherwise the very redelivery
        // this throw exists to trigger would be permanently blocked by its own failed
        // attempt's claim, silently losing the message instead of retrying it.
        var executor = new FakeWorkflowExecutor(_ => WorkflowExecutionResult.TransientFailure("OOM"));
        var store = new ServiceBusReplayAndResultStore(new MemoryStorage());
        var sut = NewSut(executor: executor, store: store);
        var actions = new FakeServiceBusMessageActions();
        var payload = new ServiceBusWorkflowMessage { Workflow = "Hello World" };
        var message = NewMessage("{\"workflow\":\"Hello World\"}");
        var identity = NewUserIdentity(jti: "jti-transient-release");
        Assert.IsTrue(store.TryClaim("corr-transient-release"), "Simulates Run(...)'s claim before dispatching to this method.");

        await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            () => sut.ProcessAuthenticatedMessageAsync(identity, payload, "corr-transient-release", message, actions, DateTimeOffset.UtcNow, CancellationToken.None));

        Assert.IsTrue(store.TryClaim("corr-transient-release"),
            "The claim must be released on a transient failure so a genuine Service Bus redelivery can re-execute.");
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task ProcessAuthenticated_ExecutorThrowsUnexpectedException_RethrowsWithoutSwallowing()
    {
        var executor = new FakeWorkflowExecutor(_ => throw new InvalidOperationException("unexpected boom"));
        var store = new ServiceBusReplayAndResultStore(new MemoryStorage());
        var sut = NewSut(executor: executor, store: store);
        var actions = new FakeServiceBusMessageActions();
        var payload = new ServiceBusWorkflowMessage { Workflow = "Hello World" };
        var message = NewMessage("{\"workflow\":\"Hello World\"}");
        var identity = NewUserIdentity(jti: "jti-exception");

        await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            () => sut.ProcessAuthenticatedMessageAsync(identity, payload, "corr-exception", message, actions, DateTimeOffset.UtcNow, CancellationToken.None));

        // Left for the Service Bus extension's own retry/backoff — no settlement, no
        // recorded result for this correlation id.
        Assert.AreEqual(0, actions.CompleteCalls);
        Assert.AreEqual(0, actions.DeadLetterCalls);
        Assert.IsFalse(store.TryGetResult("corr-exception", out _));
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task ProcessAuthenticated_ExecutorThrowsUnexpectedException_ReleasesClaim_AllowingRetryToReExecute()
    {
        var executor = new FakeWorkflowExecutor(_ => throw new InvalidOperationException("unexpected boom"));
        var store = new ServiceBusReplayAndResultStore(new MemoryStorage());
        var sut = NewSut(executor: executor, store: store);
        var actions = new FakeServiceBusMessageActions();
        var payload = new ServiceBusWorkflowMessage { Workflow = "Hello World" };
        var message = NewMessage("{\"workflow\":\"Hello World\"}");
        var identity = NewUserIdentity(jti: "jti-exception-release");
        Assert.IsTrue(store.TryClaim("corr-exception-release"), "Simulates Run(...)'s claim before dispatching to this method.");

        await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            () => sut.ProcessAuthenticatedMessageAsync(identity, payload, "corr-exception-release", message, actions, DateTimeOffset.UtcNow, CancellationToken.None));

        Assert.IsTrue(store.TryClaim("corr-exception-release"),
            "The claim must be released on an unexpected exception so a genuine Service Bus redelivery can re-execute.");
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task ProcessAuthenticated_AppOnlyToken_CallerIdentityIsAppPrefixed()
    {
        var executor = new FakeWorkflowExecutor(_ => new WorkflowExecutionResult { IsSuccess = true, Payload = "{}" });
        var store = new ServiceBusReplayAndResultStore(new MemoryStorage());
        var sut = NewSut(executor: executor, store: store);
        var actions = new FakeServiceBusMessageActions();
        var payload = new ServiceBusWorkflowMessage { Workflow = "Hello World" };
        var message = NewMessage("{\"workflow\":\"Hello World\"}");
        var appOnlyIdentity = new ClaimsIdentity(new[]
        {
            new Claim(AuthConstants.ObjectIdentifier, "11111111-2222-3333-4444-555555555555"),
            new Claim("jti", "jti-app-only"),
        }, "Bearer");

        await sut.ProcessAuthenticatedMessageAsync(appOnlyIdentity, payload, "corr-app-only", message, actions, DateTimeOffset.UtcNow, CancellationToken.None);

        store.TryGetResult("corr-app-only", out var result);
        Assert.AreEqual("app:11111111-2222-3333-4444-555555555555", result!.Caller);
    }
}
