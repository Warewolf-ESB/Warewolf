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

        /// <summary>
        /// WOLF-8512: when set, CompleteMessageAsync awaits a delay honouring the passed
        /// CancellationToken before completing - used to simulate a settlement call that
        /// outlives ServiceBusTriggerOptions.SettlementTimeout so SettleAsync's own
        /// CancellationTokenSource cancels it (Task.Delay throws TaskCanceledException, an
        /// OperationCanceledException, exactly like a real slow settlement call would).
        /// </summary>
        public TimeSpan? CompleteMessageDelay { get; set; }

        public override async Task CompleteMessageAsync(ServiceBusReceivedMessage message, CancellationToken cancellationToken = default)
        {
            if (CompleteMessageDelay is { } delay)
            {
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
            }
            CompleteCalls++;
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

        public TestExecutionResult ExecuteTest(TestExecutionRequest request) =>
            throw new NotSupportedException("ServiceBusWorkflowTriggerFunctionTests does not exercise execute_test.");
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
        ServiceBusEntraAuthOptions? authOptions = null,
        ServiceBusTriggerOptions? triggerOptions = null,
        SemaphoreSlim? executionConcurrencyLimiter = null)
    {
        var effectiveTriggerOptions = triggerOptions ?? new ServiceBusTriggerOptions();
        return new(
            new EntraBearerTokenValidator(authOptions ?? new ServiceBusEntraAuthOptions()),
            policyMatcher ?? new FakePolicyMatcher(PolicyMatchResult.Allow()),
            executor ?? new FakeWorkflowExecutor(_ => new WorkflowExecutionResult { IsSuccess = true, Payload = "{}" }),
            store ?? new ServiceBusReplayAndResultStore(new MemoryStorage()),
            new AuditLogger(NullLogger<AuditLogger>.Instance),
            HostEnvironmentConfig.Load(),
            effectiveTriggerOptions,
            executionConcurrencyLimiter ?? new SemaphoreSlim(effectiveTriggerOptions.MaxConcurrentExecutions, effectiveTriggerOptions.MaxConcurrentExecutions),
            NullLogger<ServiceBusWorkflowTriggerFunction>.Instance);
    }

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
        Assert.AreEqual("Malformed:BodyNotJson", actions.LastDeadLetterReason);
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
        Assert.AreEqual("Malformed:MissingWorkflowField", actions.LastDeadLetterReason);
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
        Assert.AreEqual(0, actions.AbandonCalls,
            "Must settle NEITHER way - an explicit Abandon releases the lock immediately, which (if the first " +
            "attempt is still genuinely executing) can retry-storm the message past MaxDeliveryAttempts before " +
            "the first attempt ever finishes, permanently losing it to Service Bus's own silent dead-letter. " +
            "Leaving the message unsettled lets its lock expire on Service Bus's own timing instead - " +
            "host.json's extensions.serviceBus.autoCompleteMessages:false means an unsettled return is never " +
            "auto-completed either.");
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
        Assert.AreEqual("InvalidToken:MissingAuthorizationProperty", actions.LastDeadLetterReason);
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task Run_ServiceBusAuthNotConfigured_TreatedAsTransient_NoResultSaved_WithoutNetworkCall()
    {
        // WOLF-8512: engine misconfiguration (missing WAREWOLF_ENTRA_TENANT_ID /
        // WAREWOLF_ENTRA_SERVICEBUS_AUDIENCE) must NOT permanently poison the correlation id -
        // a config fix followed by redelivery should succeed, so this is now transient rather
        // than the terminal InvalidToken it used to be (see ServiceBusWorkflowTriggerFunction's
        // "!_tokenValidator.IsEnabled" branch). Default ServiceBusEntraAuthOptions() has no
        // TenantId/Audience → IsEnabled == false, so this must still fail BEFORE any live OIDC
        // metadata call is attempted - only the classification of the failure has changed.
        var store = new ServiceBusReplayAndResultStore(new MemoryStorage());
        var sut = NewSut(authOptions: new ServiceBusEntraAuthOptions(), store: store);
        var actions = new FakeServiceBusMessageActions();
        var message = NewMessage("{\"workflow\":\"Hello World\"}");

        await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            () => sut.Run(message, actions, CancellationToken.None));

        Assert.AreEqual(0, actions.DeadLetterCalls,
            "Engine misconfiguration must not dead-letter on first delivery - it must retry via standard Service Bus redelivery instead.");
        Assert.AreEqual(0, actions.CompleteCalls);
        Assert.IsFalse(store.TryGetResult(message.MessageId, out _),
            "A transient failure must never persist a result - a saved result would satisfy the idempotency dedupe check and permanently prevent re-execution once the engine is fixed.");
        Assert.IsTrue(store.TryClaim(message.MessageId),
            "The claim taken by TryClaim must be released so a redelivery (after an operator fixes the configuration) is not blocked by this failed attempt's own claim.");
    }

    // ── IsOwnInvocationCancellation(...) — pure classification helper (WOLF-8512) ──
    //    No I/O, no need to fake the sealed EntraBearerTokenValidator - same reason
    //    WorkflowExecutorTransientFailureTests tests BuildTransientFailureResult directly
    //    rather than driving the whole Execute(...) call for the analogous OOM fix.

    [TestMethod]
    [TestCategory("UnitTest")]
    public void IsOwnInvocationCancellation_OwnTokenCancelled_ReturnsTrue()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var ex = new TaskCanceledException();

        Assert.IsTrue(ServiceBusWorkflowTriggerFunction.IsOwnInvocationCancellation(ex, cts.Token));
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void IsOwnInvocationCancellation_OperationCanceledException_OwnTokenCancelled_ReturnsTrue()
    {
        // TaskCanceledException is the concrete type actually observed live ("A task was
        // canceled."), but the classification must hold for the base type too - anything
        // awaited that respects the token can surface either.
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var ex = new OperationCanceledException();

        Assert.IsTrue(ServiceBusWorkflowTriggerFunction.IsOwnInvocationCancellation(ex, cts.Token));
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void IsOwnInvocationCancellation_TokenNotCancelled_ReturnsFalse()
    {
        // A cancellation-shaped exception surfacing while OUR token is still live cannot be
        // our own invocation being cancelled - do not misclassify it as transient.
        using var cts = new CancellationTokenSource();
        var ex = new TaskCanceledException();

        Assert.IsFalse(ServiceBusWorkflowTriggerFunction.IsOwnInvocationCancellation(ex, cts.Token));
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void IsOwnInvocationCancellation_GenuineValidationFailure_ReturnsFalse()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel(); // even with our own token already cancelled for an unrelated reason...
        var ex = new Microsoft.IdentityModel.Tokens.SecurityTokenExpiredException("token expired");

        // ...a genuinely bad token must never be reclassified as transient just because the
        // invocation also happened to be cancelling around the same time.
        Assert.IsFalse(ServiceBusWorkflowTriggerFunction.IsOwnInvocationCancellation(ex, cts.Token));
    }

    // ── FailTransient(...) — shared transient-tail helper (WOLF-8512, step 5a) ──
    //    Both the shipped IsOwnInvocationCancellation catch clause and the newer
    //    TokenValidationFailureClassifier.IsTransient clause (and the !IsEnabled branch)
    //    now route through this one helper - tested directly here so its release/no-save/
    //    rethrow contract is verified independently of which caller reaches it.

    [TestMethod]
    [TestCategory("UnitTest")]
    public void FailTransient_ReleasesClaim_SavesNoResult_Rethrows()
    {
        var store = new ServiceBusReplayAndResultStore(new MemoryStorage());
        Assert.IsTrue(store.TryClaim("corr-fail-transient"), "Pre-claim to simulate an in-flight attempt.");
        var sut = NewSut(store: store);
        var originalException = new InvalidOperationException("boom");

        var thrown = Assert.ThrowsException<InvalidOperationException>(
            () => sut.FailTransient("corr-fail-transient", originalException, "template {CorrelationId}", "corr-fail-transient"));

        Assert.AreSame(originalException, thrown,
            "The original exception instance must be rethrown via ExceptionDispatchInfo, not wrapped or replaced.");
        Assert.IsFalse(store.TryGetResult("corr-fail-transient", out _),
            "FailTransient must never call SaveResult - a persisted result would satisfy the idempotency dedupe check and permanently prevent re-execution.");
        Assert.IsTrue(store.TryClaim("corr-fail-transient"),
            "The claim must be released so a new delivery is not permanently blocked by this failed attempt's own claim.");
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
        Assert.AreEqual("InvalidToken:JtiReplay", actions.LastDeadLetterReason);
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
        Assert.AreEqual("Denied:PolicyForbidden", actions.LastDeadLetterReason);
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
        Assert.AreEqual("Denied:ConfigMissingDeny", actions.LastDeadLetterReason);
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
    public async Task ProcessAuthenticated_SettlementTimesOut_ResultStillPersisted_NoThrow()
    {
        // WOLF-8512 (§2.2's safety argument): a settlement call that never completes within
        // SettlementTimeout must not throw or lose the outcome - the result is always saved
        // BEFORE settlement is attempted, so a timed-out settlement just leaves the message
        // unsettled; the lock expires, it redelivers, and the idempotency dedupe check finds
        // the already-saved result and completes it then.
        var executor = new FakeWorkflowExecutor(_ => new WorkflowExecutionResult { IsSuccess = true, Payload = "{}" });
        var store = new ServiceBusReplayAndResultStore(new MemoryStorage());
        var triggerOptions = new ServiceBusTriggerOptions { SettlementTimeout = TimeSpan.FromMilliseconds(50) };
        var sut = NewSut(executor: executor, store: store, triggerOptions: triggerOptions);
        var actions = new FakeServiceBusMessageActions { CompleteMessageDelay = TimeSpan.FromSeconds(5) };
        var payload = new ServiceBusWorkflowMessage { Workflow = "Hello World" };
        var message = NewMessage("{\"workflow\":\"Hello World\"}");
        var identity = NewUserIdentity(jti: "jti-settlement-timeout");

        // Must complete normally (no exception) despite CompleteMessageAsync never actually
        // finishing within SettlementTimeout.
        await sut.ProcessAuthenticatedMessageAsync(identity, payload, "corr-settlement-timeout", message, actions, DateTimeOffset.UtcNow, CancellationToken.None);

        Assert.AreEqual(0, actions.CompleteCalls, "CompleteMessageAsync must have been abandoned by the settlement timeout, never actually finishing.");
        store.TryGetResult("corr-settlement-timeout", out var result);
        Assert.AreEqual(ServiceBusTriggerStatus.Succeeded, result!.Status,
            "The result must already be persisted before settlement is attempted, so a settlement timeout never loses it.");
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
        Assert.AreEqual("Failed:ExecutionFailed", actions.LastDeadLetterReason);
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

    // ── Bounded execution time (ServiceBusTriggerOptions.ExecutionTimeout) ──────

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task ProcessAuthenticated_ExecutionExceedsTimeout_ThrowsTimeoutException_ReleasesClaim_NoResultPersisted()
    {
        // gate is never set during the assertion window below - simulates an execution that
        // hangs (e.g. thread-pool starvation under load), not one that merely errors.
        var gate = new ManualResetEventSlim(false);
        var executor = new FakeWorkflowExecutor(_ =>
        {
            gate.Wait();
            return new WorkflowExecutionResult { IsSuccess = true, Payload = "{}" };
        });
        var store = new ServiceBusReplayAndResultStore(new MemoryStorage());
        var triggerOptions = new ServiceBusTriggerOptions { ExecutionTimeout = TimeSpan.FromMilliseconds(50) };
        var sut = NewSut(executor: executor, store: store, triggerOptions: triggerOptions);
        var payload = new ServiceBusWorkflowMessage { Workflow = "Hello World" };
        var message = NewMessage("{\"workflow\":\"Hello World\"}");
        var identity = NewUserIdentity(jti: "jti-timeout");
        Assert.IsTrue(store.TryClaim("corr-timeout"), "Simulates Run(...)'s claim before dispatching to this method.");

        await Assert.ThrowsExceptionAsync<TimeoutException>(
            () => sut.ProcessAuthenticatedMessageAsync(identity, payload, "corr-timeout", message, new FakeServiceBusMessageActions(), DateTimeOffset.UtcNow, CancellationToken.None));

        Assert.IsTrue(store.TryClaim("corr-timeout"),
            "The claim must be released on a timeout so a genuine Service Bus redelivery can re-execute.");
        Assert.IsFalse(store.TryGetResult("corr-timeout", out _),
            "No result should be persisted for an execution that timed out.");

        gate.Set(); // let the orphaned background execution finish so it doesn't leak past the test
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task ProcessAuthenticated_ExecutionFasterThanTimeout_CompletesNormally()
    {
        var executor = new FakeWorkflowExecutor(_ => new WorkflowExecutionResult { IsSuccess = true, Payload = "{}" });
        var store = new ServiceBusReplayAndResultStore(new MemoryStorage());
        var triggerOptions = new ServiceBusTriggerOptions { ExecutionTimeout = TimeSpan.FromMilliseconds(50) };
        var sut = NewSut(executor: executor, store: store, triggerOptions: triggerOptions);
        var actions = new FakeServiceBusMessageActions();
        var payload = new ServiceBusWorkflowMessage { Workflow = "Hello World" };
        var message = NewMessage("{\"workflow\":\"Hello World\"}");
        var identity = NewUserIdentity(jti: "jti-fast");

        await sut.ProcessAuthenticatedMessageAsync(identity, payload, "corr-fast", message, actions, DateTimeOffset.UtcNow, CancellationToken.None);

        Assert.AreEqual(1, actions.CompleteCalls);
        store.TryGetResult("corr-fast", out var result);
        Assert.AreEqual(ServiceBusTriggerStatus.Succeeded, result!.Status);
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task ProcessAuthenticated_TimedOutExecution_ReleasesConcurrencySlot_OnlyWhenBackgroundExecutionActuallyFinishes()
    {
        // Pins the deliberate design decision documented on ServiceBusTriggerOptions.
        // MaxConcurrentExecutions: a timed-out (abandoned, not cancelled) execution keeps
        // consuming a real resource until it actually finishes, so its concurrency slot
        // must stay charged against the cap for that whole time - freeing it the moment we
        // merely stop waiting would let timed-out background work become invisible extra
        // load beyond the configured cap.
        var gate = new ManualResetEventSlim(false);
        var executor = new FakeWorkflowExecutor(_ =>
        {
            gate.Wait();
            return new WorkflowExecutionResult { IsSuccess = true, Payload = "{}" };
        });
        var store = new ServiceBusReplayAndResultStore(new MemoryStorage());
        var limiter = new SemaphoreSlim(1, 1);
        var triggerOptions = new ServiceBusTriggerOptions { ExecutionTimeout = TimeSpan.FromMilliseconds(50), MaxConcurrentExecutions = 1 };
        var sut = NewSut(executor: executor, store: store, triggerOptions: triggerOptions, executionConcurrencyLimiter: limiter);
        var payload = new ServiceBusWorkflowMessage { Workflow = "Hello World" };
        var message = NewMessage("{\"workflow\":\"Hello World\"}");
        var identity = NewUserIdentity(jti: "jti-slot-honesty");
        Assert.IsTrue(store.TryClaim("corr-slot-honesty"), "Simulates Run(...)'s claim before dispatching to this method.");

        await Assert.ThrowsExceptionAsync<TimeoutException>(
            () => sut.ProcessAuthenticatedMessageAsync(identity, payload, "corr-slot-honesty", message, new FakeServiceBusMessageActions(), DateTimeOffset.UtcNow, CancellationToken.None));

        Assert.AreEqual(0, limiter.CurrentCount,
            "The slot must stay charged against the cap while the abandoned execution is still actually running in the background.");

        gate.Set();
        var released = SpinWait.SpinUntil(() => limiter.CurrentCount == 1, TimeSpan.FromSeconds(2));
        Assert.IsTrue(released, "The slot must be released once the abandoned execution actually finishes.");
    }

    // ── Bounded concurrency (ServiceBusTriggerOptions.MaxConcurrentExecutions) ──

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task ProcessAuthenticated_ConcurrencyLimiterAtCapacity_NextDeliveryWaitsForAFreeSlot()
    {
        var gateA = new ManualResetEventSlim(false);
        var startedA = new ManualResetEventSlim(false);
        var startedB = new ManualResetEventSlim(false);
        var executor = new FakeWorkflowExecutor(request =>
        {
            if (request.InputParameters["marker"] == "A")
            {
                startedA.Set();
                gateA.Wait();
            }
            else
            {
                startedB.Set();
            }
            return new WorkflowExecutionResult { IsSuccess = true, Payload = "{}" };
        });
        var limiter = new SemaphoreSlim(1, 1);
        var triggerOptions = new ServiceBusTriggerOptions { MaxConcurrentExecutions = 1 };
        var sut = NewSut(executor: executor, triggerOptions: triggerOptions, executionConcurrencyLimiter: limiter);
        var actionsA = new FakeServiceBusMessageActions();
        var actionsB = new FakeServiceBusMessageActions();
        var payloadA = new ServiceBusWorkflowMessage { Workflow = "Hello World", Inputs = new Dictionary<string, string> { ["marker"] = "A" } };
        var payloadB = new ServiceBusWorkflowMessage { Workflow = "Hello World", Inputs = new Dictionary<string, string> { ["marker"] = "B" } };
        var messageA = NewMessage("{\"workflow\":\"Hello World\"}");
        var messageB = NewMessage("{\"workflow\":\"Hello World\"}");

        var taskA = sut.ProcessAuthenticatedMessageAsync(
            NewUserIdentity(jti: "jti-cap-a"), payloadA, "corr-cap-a", messageA, actionsA, DateTimeOffset.UtcNow, CancellationToken.None);
        Assert.IsTrue(startedA.Wait(TimeSpan.FromSeconds(2)), "First delivery must acquire the only slot and start executing.");

        var taskB = sut.ProcessAuthenticatedMessageAsync(
            NewUserIdentity(jti: "jti-cap-b"), payloadB, "corr-cap-b", messageB, actionsB, DateTimeOffset.UtcNow, CancellationToken.None);
        Assert.IsFalse(startedB.Wait(TimeSpan.FromMilliseconds(200)),
            "Second delivery must wait for a free execution slot instead of running immediately while the cap is exhausted.");

        gateA.Set();
        await taskA;
        Assert.IsTrue(startedB.Wait(TimeSpan.FromSeconds(2)), "Second delivery must proceed once the first releases its slot.");
        await taskB;

        Assert.AreEqual(1, actionsA.CompleteCalls);
        Assert.AreEqual(1, actionsB.CompleteCalls);
    }

    // ── Bounded slot wait (ServiceBusTriggerOptions.SlotWaitTimeout) ────────────

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task ProcessAuthenticated_SlotWaitExceedsTimeout_ThrowsTimeoutException_ReleasesClaim_NeverAcquiresSlot()
    {
        // Covers the gap ExecutionTimeout alone cannot close: a slot held by a genuinely
        // deadlocked execution is never released (see ExecutionTimeout's "Known
        // limitation"), so a delivery queued behind it must have its own bound on the WAIT
        // itself - otherwise it waits forever with no result, no error, and nothing ever
        // reaching the DLQ (observed directly in the 1000-message ShovelBridge load test
        // incident of 2026-08-25, even with ExecutionTimeout and MaxConcurrentExecutions
        // both already in place).
        var gateA = new ManualResetEventSlim(false); // holder never releases within this test
        var startedA = new ManualResetEventSlim(false);
        var executor = new FakeWorkflowExecutor(request =>
        {
            if (request.InputParameters["marker"] == "A")
            {
                startedA.Set();
                gateA.Wait();
            }
            return new WorkflowExecutionResult { IsSuccess = true, Payload = "{}" };
        });
        var store = new ServiceBusReplayAndResultStore(new MemoryStorage());
        var limiter = new SemaphoreSlim(1, 1);
        var triggerOptions = new ServiceBusTriggerOptions { MaxConcurrentExecutions = 1, SlotWaitTimeout = TimeSpan.FromMilliseconds(50) };
        var sut = NewSut(executor: executor, store: store, triggerOptions: triggerOptions, executionConcurrencyLimiter: limiter);
        var payloadA = new ServiceBusWorkflowMessage { Workflow = "Hello World", Inputs = new Dictionary<string, string> { ["marker"] = "A" } };
        var payloadB = new ServiceBusWorkflowMessage { Workflow = "Hello World", Inputs = new Dictionary<string, string> { ["marker"] = "B" } };
        var messageA = NewMessage("{\"workflow\":\"Hello World\"}");
        var messageB = NewMessage("{\"workflow\":\"Hello World\"}");
        Assert.IsTrue(store.TryClaim("corr-slot-wait-b"), "Simulates Run(...)'s claim before dispatching to this method.");

        var taskA = sut.ProcessAuthenticatedMessageAsync(
            NewUserIdentity(jti: "jti-slot-wait-a"), payloadA, "corr-slot-wait-a", messageA, new FakeServiceBusMessageActions(), DateTimeOffset.UtcNow, CancellationToken.None);
        Assert.IsTrue(startedA.Wait(TimeSpan.FromSeconds(2)), "First delivery must acquire the only slot and start executing.");

        await Assert.ThrowsExceptionAsync<TimeoutException>(
            () => sut.ProcessAuthenticatedMessageAsync(
                NewUserIdentity(jti: "jti-slot-wait-b"), payloadB, "corr-slot-wait-b", messageB, new FakeServiceBusMessageActions(), DateTimeOffset.UtcNow, CancellationToken.None));

        Assert.IsTrue(store.TryClaim("corr-slot-wait-b"),
            "The claim must be released when the wait for a free slot itself times out, so a redelivery can retry.");
        Assert.AreEqual(0, limiter.CurrentCount,
            "The second delivery never acquired a slot at all, so it must not have changed the limiter's count.");

        gateA.Set(); // let the first delivery's execution finish so it doesn't leak past the test
        await taskA;
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task ProcessAuthenticated_DifferentCorrelationIds_WithinCapacity_BothProceedWithoutWaitingOnEachOther()
    {
        var gateA = new ManualResetEventSlim(false);
        var gateB = new ManualResetEventSlim(false);
        var startedA = new ManualResetEventSlim(false);
        var startedB = new ManualResetEventSlim(false);
        var executor = new FakeWorkflowExecutor(request =>
        {
            if (request.InputParameters["marker"] == "A")
            {
                startedA.Set();
                gateA.Wait();
            }
            else
            {
                startedB.Set();
                gateB.Wait();
            }
            return new WorkflowExecutionResult { IsSuccess = true, Payload = "{}" };
        });
        var limiter = new SemaphoreSlim(2, 2);
        var triggerOptions = new ServiceBusTriggerOptions { MaxConcurrentExecutions = 2 };
        var sut = NewSut(executor: executor, triggerOptions: triggerOptions, executionConcurrencyLimiter: limiter);
        var payloadA = new ServiceBusWorkflowMessage { Workflow = "Hello World", Inputs = new Dictionary<string, string> { ["marker"] = "A" } };
        var payloadB = new ServiceBusWorkflowMessage { Workflow = "Hello World", Inputs = new Dictionary<string, string> { ["marker"] = "B" } };
        var messageA = NewMessage("{\"workflow\":\"Hello World\"}");
        var messageB = NewMessage("{\"workflow\":\"Hello World\"}");

        var taskA = sut.ProcessAuthenticatedMessageAsync(
            NewUserIdentity(jti: "jti-within-a"), payloadA, "corr-within-a", messageA, new FakeServiceBusMessageActions(), DateTimeOffset.UtcNow, CancellationToken.None);
        var taskB = sut.ProcessAuthenticatedMessageAsync(
            NewUserIdentity(jti: "jti-within-b"), payloadB, "corr-within-b", messageB, new FakeServiceBusMessageActions(), DateTimeOffset.UtcNow, CancellationToken.None);

        Assert.IsTrue(startedA.Wait(TimeSpan.FromSeconds(2)) && startedB.Wait(TimeSpan.FromSeconds(2)),
            "Both deliveries must be able to start concurrently when the cap is not exceeded.");

        gateA.Set();
        gateB.Set();
        await Task.WhenAll(taskA, taskB);
    }
}
