/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Unit tests for ServiceBusWorkflowTriggerFunction — the in-process Service Bus
 *  workflow trigger that trusts the TRANSPORT boundary only. There is no per-message
 *  caller token, no jti replay check, and no IWorkflowPolicyMatcher authorization call —
 *  every well-formed message on the configured queue executes unconditionally. See the
 *  class's own XML doc ("Security posture") for why.
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
 *   - IWorkflowExecutor gets a small hand-written fake.
 *   - ServiceBusReplayAndResultStore is the REAL production type, backed by
 *     Hangfire.MemoryStorage via its internal JobStorage test seam (mirrors
 *     ResumptionExecutorTests' convention for the suspend/resume store).
 *
 *  Coverage split, matching Run(...)'s early-exit branches vs. the extracted
 *  ProcessMessageAsync(...): the pre-claim branches (malformed JSON, null payload,
 *  duplicate correlation id, in-flight claim race) are exercised through
 *  Run(...) itself, including one test proving a message executes successfully with
 *  no Authorization property at all — the direct behavioural proof of the new trust
 *  model. The execution/settlement/timeout/concurrency branches are exercised by
 *  calling ProcessMessageAsync(...) directly, mirroring Run(...)'s own dispatch.
 */

using System.Linq;
using Azure.Messaging.ServiceBus;
using Hangfire.MemoryStorage;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Execution.Lightweight.Auth;
using Warewolf.Execution.Lightweight.Auth.Models;
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

    /// <summary>
    /// WOLF-8512: wraps a real <see cref="IServiceBusReplayAndResultStore"/> and records
    /// every <see cref="RecordClaimStage"/> call, in order, so tests can assert the
    /// silent-death breadcrumb sequence directly. The production store only ever keeps
    /// the LATEST stage per correlation id (and <see cref="ReleaseClaim"/> wipes it
    /// entirely), so a post-hoc read back from the real store cannot prove ordering or
    /// survive the timeout/exception cleanup paths - recording the calls as they happen
    /// is the only way to assert them.
    /// </summary>
    private sealed class RecordingReplayAndResultStore : IServiceBusReplayAndResultStore
    {
        private readonly IServiceBusReplayAndResultStore _inner;
        private readonly List<(string CorrelationId, string Stage)> _recordedStages = new();

        public RecordingReplayAndResultStore(IServiceBusReplayAndResultStore inner) => _inner = inner;

        public IReadOnlyList<(string CorrelationId, string Stage)> RecordedStages => _recordedStages;

        public bool TryRegisterJti(string jti) => _inner.TryRegisterJti(jti);
        public bool TryGetResult(string correlationId, out ServiceBusTriggerResult? result) => _inner.TryGetResult(correlationId, out result);
        public void SaveResult(ServiceBusTriggerResult result) => _inner.SaveResult(result);
        public bool TryClaim(string correlationId) => _inner.TryClaim(correlationId);
        public void ReleaseClaim(string correlationId) => _inner.ReleaseClaim(correlationId);

        public void RecordClaimStage(string correlationId, string stage)
        {
            _recordedStages.Add((correlationId, stage));
            _inner.RecordClaimStage(correlationId, stage);
        }

        public void RecordInstanceHeartbeat(string instanceId, int inFlightExecutions) =>
            _inner.RecordInstanceHeartbeat(instanceId, inFlightExecutions);
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
        string? messageId = null)
    {
        return ServiceBusModelFactory.ServiceBusReceivedMessage(
            body: BinaryData.FromString(body),
            messageId: messageId ?? Guid.NewGuid().ToString(),
            correlationId: correlationId);
    }

    private static readonly ServiceBusTriggerDefinition DefaultTriggerDefinition = new()
    {
        QueueName = "wwexecution-secure-trigger-queue",
        WorkflowName = "Hello World",
    };

    private static ServiceBusWorkflowTriggerFunction NewSut(
        IWorkflowExecutor? executor = null,
        IServiceBusReplayAndResultStore? store = null,
        ServiceBusTriggerOptions? triggerOptions = null,
        ServiceBusTriggerDefinition? triggerDefinition = null,
        SemaphoreSlim? executionConcurrencyLimiter = null)
    {
        var effectiveTriggerOptions = triggerOptions ?? new ServiceBusTriggerOptions();
        return new(
            executor ?? new FakeWorkflowExecutor(_ => new WorkflowExecutionResult { IsSuccess = true, Payload = "{}" }),
            store ?? new ServiceBusReplayAndResultStore(new MemoryStorage()),
            new AuditLogger(NullLogger<AuditLogger>.Instance),
            HostEnvironmentConfig.Load(),
            effectiveTriggerOptions,
            triggerDefinition ?? DefaultTriggerDefinition,
            executionConcurrencyLimiter ?? new SemaphoreSlim(effectiveTriggerOptions.MaxConcurrentExecutions, effectiveTriggerOptions.MaxConcurrentExecutions),
            NullLogger<ServiceBusWorkflowTriggerFunction>.Instance);
    }

    // ── Run(...) — pre-claim branches ───────────────────────────────────────────

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
    public async Task Run_NullPayload_DeadLettersAsMalformed()
    {
        // WOLF-8517: there is no "workflow" field on the message any more (it always comes
        // from the injected ServiceBusTriggerDefinition), so the only remaining malformed
        // shape is a body that is valid JSON but deserializes to null (e.g. the literal
        // "null").
        var executor = new FakeWorkflowExecutor(_ => throw new InvalidOperationException("must not execute"));
        var sut = NewSut(executor: executor);
        var actions = new FakeServiceBusMessageActions();
        var message = NewMessage("null");

        await sut.Run(message, actions, CancellationToken.None);

        Assert.AreEqual(0, executor.CallCount);
        Assert.AreEqual(1, actions.DeadLetterCalls);
        Assert.AreEqual("Malformed:NullPayload", actions.LastDeadLetterReason);
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
        var message = NewMessage("{\"correlationId\":\"dup-corr\"}", correlationId: "dup-corr");

        await sut.Run(message, actions, CancellationToken.None);

        Assert.AreEqual(0, executor.CallCount, "A duplicate delivery must not re-execute the workflow.");
        Assert.AreEqual(1, actions.CompleteCalls);
        Assert.AreEqual(0, actions.DeadLetterCalls);
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task Run_ClaimAlreadyHeldByInFlightDelivery_LeavesUnsettledForRedelivery()
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
        var message = NewMessage("{\"correlationId\":\"corr-in-flight\"}", correlationId: "corr-in-flight");

        await sut.Run(message, actions, CancellationToken.None);

        Assert.AreEqual(0, executor.CallCount, "A delivery racing an in-flight claim must not execute the workflow.");
        Assert.AreEqual(0, actions.CompleteCalls);
        Assert.AreEqual(0, actions.DeadLetterCalls);
        Assert.AreEqual(0, actions.AbandonCalls,
            "Must settle NEITHER way - an explicit Abandon releases the lock immediately, which (if the first " +
            "attempt is still genuinely executing) can retry-storm the message past MaxDeliveryAttempts before " +
            "the first attempt ever finishes, permanently losing it to Service Bus's own silent dead-letter. " +
            "Leaving the message unsettled lets its lock expire on Service Bus's own timing instead. " +
            "WOLF-8512 (build 30658): that is only true while AutoCompleteMessages is false ON THE " +
            "[ServiceBusTrigger] ATTRIBUTE - NOT, as this comment previously claimed, because of host.json's " +
            "extensions.serviceBus.autoCompleteMessages:false, which governs the global options only and is " +
            "overridden per-function by the dotnet-isolated host. While that override was active this exact " +
            "return was auto-completed and the message destroyed, and this test still passed - it asserts what " +
            "the code does, and the code was right. Trigger_OptsOutOfAutoCompleteMessages is what actually " +
            "guards the premise.");
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task Run_MessageWithNoAuthorizationProperty_ExecutesSuccessfully()
    {
        // The direct behavioural proof of the new trust model: a message carries no
        // caller identity at all (no 'Authorization' application property - the field
        // this trigger used to require), yet it still executes, because trust is now
        // the transport boundary (arrival on the configured queue) rather than anything
        // carried in the message.
        var executor = new FakeWorkflowExecutor(_ => new WorkflowExecutionResult { IsSuccess = true, Payload = "{}" });
        var store = new ServiceBusReplayAndResultStore(new MemoryStorage());
        var sut = NewSut(executor: executor, store: store);
        var actions = new FakeServiceBusMessageActions();
        var message = NewMessage("{}", correlationId: "corr-no-auth");

        await sut.Run(message, actions, CancellationToken.None);

        Assert.AreEqual(1, executor.CallCount);
        Assert.AreEqual(1, actions.CompleteCalls);
        Assert.AreEqual(0, actions.DeadLetterCalls);
        store.TryGetResult("corr-no-auth", out var result);
        Assert.AreEqual(ServiceBusTriggerStatus.Succeeded, result!.Status);
        Assert.AreEqual(ServiceBusWorkflowTriggerFunction.TrustedQueueCaller, result.Caller);
    }

    // ── ProcessMessageAsync(...) — execution/settlement/timeout/concurrency ────

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task ProcessMessage_ExecutionSucceeds_CompletesAndSavesOutputs()
    {
        var executor = new FakeWorkflowExecutor(_ => new WorkflowExecutionResult { IsSuccess = true, Payload = "{\"Greeting\":\"Hi\"}" });
        var store = new ServiceBusReplayAndResultStore(new MemoryStorage());
        var sut = NewSut(executor: executor, store: store);
        var actions = new FakeServiceBusMessageActions();
        var payload = new ServiceBusWorkflowMessage();
        var message = NewMessage("{}");

        await sut.ProcessMessageAsync(payload, "corr-success", message, actions, DateTimeOffset.UtcNow, CancellationToken.None);

        Assert.AreEqual(1, executor.CallCount);
        Assert.AreEqual(1, actions.CompleteCalls);
        Assert.AreEqual(0, actions.DeadLetterCalls);
        store.TryGetResult("corr-success", out var result);
        Assert.AreEqual(ServiceBusTriggerStatus.Succeeded, result!.Status);
        Assert.AreEqual(ServiceBusWorkflowTriggerFunction.TrustedQueueCaller, result.Caller);
        Assert.AreEqual("{\"Greeting\":\"Hi\"}", result.Outputs);
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task ProcessMessage_ExecutionSucceeds_StampsAnonymousExecutingPrincipalOnRequest()
    {
        var executor = new FakeWorkflowExecutor(_ => new WorkflowExecutionResult { IsSuccess = true, Payload = "{}" });
        var sut = NewSut(executor: executor);
        var actions = new FakeServiceBusMessageActions();
        var payload = new ServiceBusWorkflowMessage();
        var message = NewMessage("{}");

        await sut.ProcessMessageAsync(payload, "corr-principal", message, actions, DateTimeOffset.UtcNow, CancellationToken.None);

        var stampedPrincipal = executor.LastRequest?.ExecutingPrincipal as WorkflowClaimsPrincipal;
        Assert.IsNotNull(stampedPrincipal, "ExecutingPrincipal must still be populated (WorkflowExecutor.ResolveExecutingUser depends on it), even with no per-caller identity.");
        Assert.IsFalse(stampedPrincipal!.Identity?.IsAuthenticated, "No token is validated any more, so the stamped principal must be unauthenticated (WorkflowClaimsPrincipal.Anonymous()).");
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task ProcessMessage_SettlementTimesOut_ResultStillPersisted_NoThrow()
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
        var payload = new ServiceBusWorkflowMessage();
        var message = NewMessage("{}");

        // Must complete normally (no exception) despite CompleteMessageAsync never actually
        // finishing within SettlementTimeout.
        await sut.ProcessMessageAsync(payload, "corr-settlement-timeout", message, actions, DateTimeOffset.UtcNow, CancellationToken.None);

        Assert.AreEqual(0, actions.CompleteCalls, "CompleteMessageAsync must have been abandoned by the settlement timeout, never actually finishing.");
        store.TryGetResult("corr-settlement-timeout", out var result);
        Assert.AreEqual(ServiceBusTriggerStatus.Succeeded, result!.Status,
            "The result must already be persisted before settlement is attempted, so a settlement timeout never loses it.");
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task ProcessMessage_ExecutionBusinessFailure_DeadLettersAsFailed_NotThrown()
    {
        var executor = new FakeWorkflowExecutor(_ => WorkflowExecutionResult.Failure("Activity 'Divide' failed: divide by zero."));
        var store = new ServiceBusReplayAndResultStore(new MemoryStorage());
        var sut = NewSut(executor: executor, store: store);
        var actions = new FakeServiceBusMessageActions();
        var payload = new ServiceBusWorkflowMessage();
        var message = NewMessage("{}");

        await sut.ProcessMessageAsync(payload, "corr-failure", message, actions, DateTimeOffset.UtcNow, CancellationToken.None);

        Assert.AreEqual(0, actions.CompleteCalls);
        Assert.AreEqual(1, actions.DeadLetterCalls);
        Assert.AreEqual("Failed:ExecutionFailed", actions.LastDeadLetterReason);
        store.TryGetResult("corr-failure", out var result);
        Assert.AreEqual(ServiceBusTriggerStatus.Failed, result!.Status);
        StringAssert.Contains(result.Error, "divide by zero");
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task ProcessMessage_ExecutionTransientFailure_ThrowsInsteadOfDeadLettering_NoResultPersisted()
    {
        // OutOfMemoryException-under-cold-start signature, surfaced by WorkflowExecutor as
        // IsTransientFailure = true — must retry (throw), NOT dead-letter, and must NOT persist a
        // terminal result (a persisted "Failed" result would satisfy the dedupe check on the
        // Service-Bus-driven redelivery and complete it without ever re-executing).
        var executor = new FakeWorkflowExecutor(_ => WorkflowExecutionResult.TransientFailure("Insufficient memory to continue the execution of the program."));
        var store = new ServiceBusReplayAndResultStore(new MemoryStorage());
        var sut = NewSut(executor: executor, store: store);
        var actions = new FakeServiceBusMessageActions();
        var payload = new ServiceBusWorkflowMessage();
        var message = NewMessage("{}");

        await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            () => sut.ProcessMessageAsync(payload, "corr-transient", message, actions, DateTimeOffset.UtcNow, CancellationToken.None));

        Assert.AreEqual(0, actions.CompleteCalls);
        Assert.AreEqual(0, actions.DeadLetterCalls);
        Assert.IsFalse(store.TryGetResult("corr-transient", out _));
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task ProcessMessage_ExecutionTransientFailure_ReleasesClaim_AllowingRetryToReExecute()
    {
        // The claim must not survive a transient failure - otherwise the very redelivery
        // this throw exists to trigger would be permanently blocked by its own failed
        // attempt's claim, silently losing the message instead of retrying it.
        var executor = new FakeWorkflowExecutor(_ => WorkflowExecutionResult.TransientFailure("OOM"));
        var store = new ServiceBusReplayAndResultStore(new MemoryStorage());
        var sut = NewSut(executor: executor, store: store);
        var actions = new FakeServiceBusMessageActions();
        var payload = new ServiceBusWorkflowMessage();
        var message = NewMessage("{}");
        Assert.IsTrue(store.TryClaim("corr-transient-release"), "Simulates Run(...)'s claim before dispatching to this method.");

        await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            () => sut.ProcessMessageAsync(payload, "corr-transient-release", message, actions, DateTimeOffset.UtcNow, CancellationToken.None));

        Assert.IsTrue(store.TryClaim("corr-transient-release"),
            "The claim must be released on a transient failure so a genuine Service Bus redelivery can re-execute.");
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task ProcessMessage_ExecutorThrowsUnexpectedException_RethrowsWithoutSwallowing()
    {
        var executor = new FakeWorkflowExecutor(_ => throw new InvalidOperationException("unexpected boom"));
        var store = new ServiceBusReplayAndResultStore(new MemoryStorage());
        var sut = NewSut(executor: executor, store: store);
        var actions = new FakeServiceBusMessageActions();
        var payload = new ServiceBusWorkflowMessage();
        var message = NewMessage("{}");

        await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            () => sut.ProcessMessageAsync(payload, "corr-exception", message, actions, DateTimeOffset.UtcNow, CancellationToken.None));

        // Left for the Service Bus extension's own retry/backoff — no settlement, no
        // recorded result for this correlation id.
        Assert.AreEqual(0, actions.CompleteCalls);
        Assert.AreEqual(0, actions.DeadLetterCalls);
        Assert.IsFalse(store.TryGetResult("corr-exception", out _));
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task ProcessMessage_ExecutorThrowsUnexpectedException_ReleasesClaim_AllowingRetryToReExecute()
    {
        var executor = new FakeWorkflowExecutor(_ => throw new InvalidOperationException("unexpected boom"));
        var store = new ServiceBusReplayAndResultStore(new MemoryStorage());
        var sut = NewSut(executor: executor, store: store);
        var actions = new FakeServiceBusMessageActions();
        var payload = new ServiceBusWorkflowMessage();
        var message = NewMessage("{}");
        Assert.IsTrue(store.TryClaim("corr-exception-release"), "Simulates Run(...)'s claim before dispatching to this method.");

        await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            () => sut.ProcessMessageAsync(payload, "corr-exception-release", message, actions, DateTimeOffset.UtcNow, CancellationToken.None));

        Assert.IsTrue(store.TryClaim("corr-exception-release"),
            "The claim must be released on an unexpected exception so a genuine Service Bus redelivery can re-execute.");
    }

    // ── Silent-death diagnostic breadcrumb (IServiceBusReplayAndResultStore.RecordClaimStage) ──
    // WOLF-8512: re-wired in after build 30651 (2026-09-12) reproduced the same
    // claimed/vanished-without-a-trace signature as prior incidents - see
    // docs/ServiceBusSecureTrigger-Architecture.md's "Silent-death diagnostic breadcrumb"
    // section for the App Insights evidence.

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task Run_SuccessfulExecution_RecordsClaimStageCheckpointsInOrder()
    {
        var executor = new FakeWorkflowExecutor(_ => new WorkflowExecutionResult { IsSuccess = true, Payload = "{}" });
        var recordingStore = new RecordingReplayAndResultStore(new ServiceBusReplayAndResultStore(new MemoryStorage()));
        var sut = NewSut(executor: executor, store: recordingStore);
        var actions = new FakeServiceBusMessageActions();
        var message = NewMessage("{}", correlationId: "corr-stages-success");

        await sut.Run(message, actions, CancellationToken.None);

        Assert.AreEqual(3, recordingStore.RecordedStages.Count,
            "Expected exactly ClaimAcquired, AboutToExecute and ExecutionReturned for a successful run.");
        Assert.AreEqual(("corr-stages-success", "ClaimAcquired"), recordingStore.RecordedStages[0]);
        Assert.AreEqual("corr-stages-success", recordingStore.RecordedStages[1].CorrelationId);
        StringAssert.StartsWith(recordingStore.RecordedStages[1].Stage, "AboutToExecute");
        Assert.AreEqual(("corr-stages-success", "ExecutionReturned (IsSuccess=True)"), recordingStore.RecordedStages[2]);
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task ProcessMessage_ExecutionExceedsTimeout_RecordsAboutToExecute_ButNeverExecutionReturned()
    {
        var gate = new ManualResetEventSlim(false);
        var executor = new FakeWorkflowExecutor(_ =>
        {
            gate.Wait();
            return new WorkflowExecutionResult { IsSuccess = true, Payload = "{}" };
        });
        var recordingStore = new RecordingReplayAndResultStore(new ServiceBusReplayAndResultStore(new MemoryStorage()));
        var triggerOptions = new ServiceBusTriggerOptions { ExecutionTimeout = TimeSpan.FromMilliseconds(50) };
        var sut = NewSut(executor: executor, store: recordingStore, triggerOptions: triggerOptions);
        var payload = new ServiceBusWorkflowMessage();
        var message = NewMessage("{}");
        Assert.IsTrue(recordingStore.TryClaim("corr-stages-timeout"), "Simulates Run(...)'s claim before dispatching to this method.");

        await Assert.ThrowsExceptionAsync<TimeoutException>(
            () => sut.ProcessMessageAsync(payload, "corr-stages-timeout", message, new FakeServiceBusMessageActions(), DateTimeOffset.UtcNow, CancellationToken.None));

        Assert.AreEqual(1, recordingStore.RecordedStages.Count,
            "A timed-out execution never returns, so ExecutionReturned must never be recorded - only the checkpoint immediately before the no-cancellation-seam call.");
        Assert.AreEqual("corr-stages-timeout", recordingStore.RecordedStages[0].CorrelationId);
        StringAssert.StartsWith(recordingStore.RecordedStages[0].Stage, "AboutToExecute");

        gate.Set(); // let the orphaned background execution finish so it doesn't leak past the test
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task ProcessMessage_ExecutorThrowsUnexpectedException_RecordsAboutToExecute_ButNeverExecutionReturned()
    {
        var executor = new FakeWorkflowExecutor(_ => throw new InvalidOperationException("unexpected boom"));
        var recordingStore = new RecordingReplayAndResultStore(new ServiceBusReplayAndResultStore(new MemoryStorage()));
        var sut = NewSut(executor: executor, store: recordingStore);
        var actions = new FakeServiceBusMessageActions();
        var payload = new ServiceBusWorkflowMessage();
        var message = NewMessage("{}");
        Assert.IsTrue(recordingStore.TryClaim("corr-stages-exception"), "Simulates Run(...)'s claim before dispatching to this method.");

        await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            () => sut.ProcessMessageAsync(payload, "corr-stages-exception", message, actions, DateTimeOffset.UtcNow, CancellationToken.None));

        Assert.AreEqual(1, recordingStore.RecordedStages.Count,
            "An unhandled executor exception never reaches the 'Execution task returned' checkpoint, so ExecutionReturned must never be recorded.");
        Assert.AreEqual("corr-stages-exception", recordingStore.RecordedStages[0].CorrelationId);
        StringAssert.StartsWith(recordingStore.RecordedStages[0].Stage, "AboutToExecute");
    }

    // ── Execution heartbeat (build 30653 addendum, ServiceBusTriggerOptions.HeartbeatInterval) ──
    // Closes the gap the three bracketing checkpoints above leave open: AboutToExecute and
    // ExecutionReturned only bracket execution, so an execution that dies partway through a
    // LONG run leaves a "before" snapshot that can be stale by however long it had already
    // been running. RunExecutionHeartbeatAsync refreshes the breadcrumb periodically while
    // execution is in flight instead.

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task ProcessMessage_SlowExecution_RecordsStillExecutingHeartbeatsBetweenAboutToExecuteAndExecutionReturned()
    {
        var gate = new ManualResetEventSlim(false);
        var executor = new FakeWorkflowExecutor(_ =>
        {
            Assert.IsTrue(gate.Wait(TimeSpan.FromSeconds(5)), "Test executor did not get released in time.");
            return new WorkflowExecutionResult { IsSuccess = true, Payload = "{}" };
        });
        var recordingStore = new RecordingReplayAndResultStore(new ServiceBusReplayAndResultStore(new MemoryStorage()));
        var triggerOptions = new ServiceBusTriggerOptions { HeartbeatInterval = TimeSpan.FromMilliseconds(20) };
        var sut = NewSut(executor: executor, store: recordingStore, triggerOptions: triggerOptions);
        var actions = new FakeServiceBusMessageActions();
        var payload = new ServiceBusWorkflowMessage();
        var message = NewMessage("{}");
        Assert.IsTrue(recordingStore.TryClaim("corr-heartbeat-slow"));

        var processTask = sut.ProcessMessageAsync(payload, "corr-heartbeat-slow", message, actions, DateTimeOffset.UtcNow, CancellationToken.None);
        // Let several heartbeat ticks land while the executor is still blocked on the gate.
        await Task.Delay(TimeSpan.FromMilliseconds(120));
        gate.Set();
        await processTask;

        // ProcessMessageAsync is called directly here (bypassing Run's own ClaimAcquired
        // stage, exactly like the other ProcessMessage_* tests above), so the sequence is
        // AboutToExecute, one-or-more StillExecuting heartbeats, then ExecutionReturned.
        var stages = recordingStore.RecordedStages;
        Assert.IsTrue(stages.Count >= 3,
            $"Expected AboutToExecute, at least one StillExecuting tick, and ExecutionReturned; got {stages.Count}: [{string.Join(", ", stages.Select(s => s.Stage))}]");
        StringAssert.StartsWith(stages[0].Stage, "AboutToExecute");
        var middleStages = stages.Skip(1).Take(stages.Count - 2).ToList();
        Assert.IsTrue(middleStages.Count > 0, "Expected at least one StillExecuting heartbeat between AboutToExecute and ExecutionReturned.");
        foreach (var (correlationId, stage) in middleStages)
        {
            Assert.AreEqual("corr-heartbeat-slow", correlationId);
            StringAssert.StartsWith(stage, "StillExecuting");
        }
        Assert.AreEqual(("corr-heartbeat-slow", "ExecutionReturned (IsSuccess=True)"), stages[^1]);
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task ProcessMessage_FastExecution_NoHeartbeatTicksFire()
    {
        // Mirrors ProcessMessage_ExecutionSucceeds_CompletesAndSavesOutputs's shape (called
        // directly, bypassing Run's own ClaimAcquired stage): a synchronous, immediate
        // executor return must not race a heartbeat tick even at a 5s interval, so only
        // AboutToExecute + ExecutionReturned are ever recorded.
        var executor = new FakeWorkflowExecutor(_ => new WorkflowExecutionResult { IsSuccess = true, Payload = "{}" });
        var recordingStore = new RecordingReplayAndResultStore(new ServiceBusReplayAndResultStore(new MemoryStorage()));
        var triggerOptions = new ServiceBusTriggerOptions { HeartbeatInterval = TimeSpan.FromSeconds(5) };
        var sut = NewSut(executor: executor, store: recordingStore, triggerOptions: triggerOptions);
        var actions = new FakeServiceBusMessageActions();
        var payload = new ServiceBusWorkflowMessage();
        var message = NewMessage("{}");
        Assert.IsTrue(recordingStore.TryClaim("corr-heartbeat-fast"));

        await sut.ProcessMessageAsync(payload, "corr-heartbeat-fast", message, actions, DateTimeOffset.UtcNow, CancellationToken.None);

        Assert.AreEqual(2, recordingStore.RecordedStages.Count,
            "A fast execution must finish (and cancel its heartbeat) well before the 5s interval's first tick.");
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task ProcessMessage_ExecutionExceedsTimeout_HeartbeatStillTicksForTheAbandonedBackgroundExecution()
    {
        // The abandoned execution keeps running in the background after ExecutionTimeout
        // fires (IWorkflowExecutor.Execute has no cancellation seam) - its heartbeat must
        // keep ticking for as long as that background execution runs, not stop merely
        // because the trigger itself gave up waiting and threw.
        var gate = new ManualResetEventSlim(false);
        var executor = new FakeWorkflowExecutor(_ =>
        {
            gate.Wait();
            return new WorkflowExecutionResult { IsSuccess = true, Payload = "{}" };
        });
        var recordingStore = new RecordingReplayAndResultStore(new ServiceBusReplayAndResultStore(new MemoryStorage()));
        var triggerOptions = new ServiceBusTriggerOptions
        {
            ExecutionTimeout = TimeSpan.FromMilliseconds(50),
            HeartbeatInterval = TimeSpan.FromMilliseconds(20),
        };
        var sut = NewSut(executor: executor, store: recordingStore, triggerOptions: triggerOptions);
        var payload = new ServiceBusWorkflowMessage();
        var message = NewMessage("{}");
        Assert.IsTrue(recordingStore.TryClaim("corr-heartbeat-timeout"));

        await Assert.ThrowsExceptionAsync<TimeoutException>(
            () => sut.ProcessMessageAsync(payload, "corr-heartbeat-timeout", message, new FakeServiceBusMessageActions(), DateTimeOffset.UtcNow, CancellationToken.None));

        // The trigger has already given up and thrown, but the background execution (and
        // therefore its heartbeat) is still running - give it a couple more intervals.
        await Task.Delay(TimeSpan.FromMilliseconds(80));
        gate.Set(); // let the orphaned background execution finish so it doesn't leak past the test

        Assert.IsTrue(recordingStore.RecordedStages.Count > 1,
            "Expected at least one StillExecuting heartbeat beyond the initial AboutToExecute, recorded WHILE the trigger method had already returned from the timeout.");
        foreach (var (correlationId, stage) in recordingStore.RecordedStages.Skip(1))
        {
            Assert.AreEqual("corr-heartbeat-timeout", correlationId);
            StringAssert.StartsWith(stage, "StillExecuting");
        }
    }

    // ── Bounded execution time (ServiceBusTriggerOptions.ExecutionTimeout) ──────

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task ProcessMessage_ExecutionExceedsTimeout_ThrowsTimeoutException_ReleasesClaim_NoResultPersisted()
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
        var payload = new ServiceBusWorkflowMessage();
        var message = NewMessage("{}");
        Assert.IsTrue(store.TryClaim("corr-timeout"), "Simulates Run(...)'s claim before dispatching to this method.");

        await Assert.ThrowsExceptionAsync<TimeoutException>(
            () => sut.ProcessMessageAsync(payload, "corr-timeout", message, new FakeServiceBusMessageActions(), DateTimeOffset.UtcNow, CancellationToken.None));

        Assert.IsTrue(store.TryClaim("corr-timeout"),
            "The claim must be released on a timeout so a genuine Service Bus redelivery can re-execute.");
        Assert.IsFalse(store.TryGetResult("corr-timeout", out _),
            "No result should be persisted for an execution that timed out.");

        gate.Set(); // let the orphaned background execution finish so it doesn't leak past the test
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task ProcessMessage_ExecutionFasterThanTimeout_CompletesNormally()
    {
        var executor = new FakeWorkflowExecutor(_ => new WorkflowExecutionResult { IsSuccess = true, Payload = "{}" });
        var store = new ServiceBusReplayAndResultStore(new MemoryStorage());
        var triggerOptions = new ServiceBusTriggerOptions { ExecutionTimeout = TimeSpan.FromMilliseconds(50) };
        var sut = NewSut(executor: executor, store: store, triggerOptions: triggerOptions);
        var actions = new FakeServiceBusMessageActions();
        var payload = new ServiceBusWorkflowMessage();
        var message = NewMessage("{}");

        await sut.ProcessMessageAsync(payload, "corr-fast", message, actions, DateTimeOffset.UtcNow, CancellationToken.None);

        Assert.AreEqual(1, actions.CompleteCalls);
        store.TryGetResult("corr-fast", out var result);
        Assert.AreEqual(ServiceBusTriggerStatus.Succeeded, result!.Status);
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task ProcessMessage_TimedOutExecution_ReleasesConcurrencySlot_OnlyWhenBackgroundExecutionActuallyFinishes()
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
        var payload = new ServiceBusWorkflowMessage();
        var message = NewMessage("{}");
        Assert.IsTrue(store.TryClaim("corr-slot-honesty"), "Simulates Run(...)'s claim before dispatching to this method.");

        await Assert.ThrowsExceptionAsync<TimeoutException>(
            () => sut.ProcessMessageAsync(payload, "corr-slot-honesty", message, new FakeServiceBusMessageActions(), DateTimeOffset.UtcNow, CancellationToken.None));

        Assert.AreEqual(0, limiter.CurrentCount,
            "The slot must stay charged against the cap while the abandoned execution is still actually running in the background.");

        gate.Set();
        var released = SpinWait.SpinUntil(() => limiter.CurrentCount == 1, TimeSpan.FromSeconds(2));
        Assert.IsTrue(released, "The slot must be released once the abandoned execution actually finishes.");
    }

    // ── Bounded concurrency (ServiceBusTriggerOptions.MaxConcurrentExecutions) ──

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task ProcessMessage_ConcurrencyLimiterAtCapacity_NextDeliveryWaitsForAFreeSlot()
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
        var payloadA = new ServiceBusWorkflowMessage { Inputs = new Dictionary<string, string> { ["marker"] = "A" } };
        var payloadB = new ServiceBusWorkflowMessage { Inputs = new Dictionary<string, string> { ["marker"] = "B" } };
        var messageA = NewMessage("{}");
        var messageB = NewMessage("{}");

        var taskA = sut.ProcessMessageAsync(payloadA, "corr-cap-a", messageA, actionsA, DateTimeOffset.UtcNow, CancellationToken.None);
        Assert.IsTrue(startedA.Wait(TimeSpan.FromSeconds(2)), "First delivery must acquire the only slot and start executing.");

        var taskB = sut.ProcessMessageAsync(payloadB, "corr-cap-b", messageB, actionsB, DateTimeOffset.UtcNow, CancellationToken.None);
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
    public async Task ProcessMessage_SlotWaitExceedsTimeout_ThrowsTimeoutException_ReleasesClaim_NeverAcquiresSlot()
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
        var payloadA = new ServiceBusWorkflowMessage { Inputs = new Dictionary<string, string> { ["marker"] = "A" } };
        var payloadB = new ServiceBusWorkflowMessage { Inputs = new Dictionary<string, string> { ["marker"] = "B" } };
        var messageA = NewMessage("{}");
        var messageB = NewMessage("{}");
        Assert.IsTrue(store.TryClaim("corr-slot-wait-b"), "Simulates Run(...)'s claim before dispatching to this method.");

        var taskA = sut.ProcessMessageAsync(payloadA, "corr-slot-wait-a", messageA, new FakeServiceBusMessageActions(), DateTimeOffset.UtcNow, CancellationToken.None);
        Assert.IsTrue(startedA.Wait(TimeSpan.FromSeconds(2)), "First delivery must acquire the only slot and start executing.");

        await Assert.ThrowsExceptionAsync<TimeoutException>(
            () => sut.ProcessMessageAsync(payloadB, "corr-slot-wait-b", messageB, new FakeServiceBusMessageActions(), DateTimeOffset.UtcNow, CancellationToken.None));

        Assert.IsTrue(store.TryClaim("corr-slot-wait-b"),
            "The claim must be released when the wait for a free slot itself times out, so a redelivery can retry.");
        Assert.AreEqual(0, limiter.CurrentCount,
            "The second delivery never acquired a slot at all, so it must not have changed the limiter's count.");

        gateA.Set(); // let the first delivery's execution finish so it doesn't leak past the test
        await taskA;
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task ProcessMessage_DifferentCorrelationIds_WithinCapacity_BothProceedWithoutWaitingOnEachOther()
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
        var payloadA = new ServiceBusWorkflowMessage { Inputs = new Dictionary<string, string> { ["marker"] = "A" } };
        var payloadB = new ServiceBusWorkflowMessage { Inputs = new Dictionary<string, string> { ["marker"] = "B" } };
        var messageA = NewMessage("{}");
        var messageB = NewMessage("{}");

        var taskA = sut.ProcessMessageAsync(payloadA, "corr-within-a", messageA, new FakeServiceBusMessageActions(), DateTimeOffset.UtcNow, CancellationToken.None);
        var taskB = sut.ProcessMessageAsync(payloadB, "corr-within-b", messageB, new FakeServiceBusMessageActions(), DateTimeOffset.UtcNow, CancellationToken.None);

        Assert.IsTrue(startedA.Wait(TimeSpan.FromSeconds(2)) && startedB.Wait(TimeSpan.FromSeconds(2)),
            "Both deliveries must be able to start concurrently when the cap is not exceeded.");

        gateA.Set();
        gateB.Set();
        await Task.WhenAll(taskA, taskB);
    }

    // ── WOLF-8512 (build 30658) — the settlement contract ────────────────────────
    //
    // Root cause of the message loss in builds 30649-30658. host.json's
    // extensions.serviceBus.autoCompleteMessages:false configures the GLOBAL ServiceBusOptions
    // only; under the dotnet-isolated worker the host applies a PER-FUNCTION override that
    // defaults to true and wins. Build 30658 therefore ran with auto-complete ON - every host
    // start logged "The 'AutoCompleteMessages' option has been overriden to 'True' value for
    // 'Functions.ServiceBusWorkflowTrigger' function" - so every deliberate unsettled return
    // below was auto-completed instead, destroying the message with no retry, no dead-letter,
    // no result and nothing in the DLQ.
    //
    // Nothing else in this file can catch that regression: every other test asserts what the
    // CODE does, and the code was correct. Only the attribute tells the host how to treat a
    // return, so only the attribute can be asserted.

    [TestMethod]
    [TestCategory("UnitTest")]
    public void Trigger_OptsOutOfAutoCompleteMessages()
    {
        var runMethod = typeof(ServiceBusWorkflowTriggerFunction).GetMethod(nameof(ServiceBusWorkflowTriggerFunction.Run));
        Assert.IsNotNull(runMethod, "ServiceBusWorkflowTriggerFunction.Run must exist for the trigger to bind at all.");

        var messageParameter = runMethod!.GetParameters()
            .SingleOrDefault(p => p.ParameterType == typeof(ServiceBusReceivedMessage));
        Assert.IsNotNull(messageParameter, "Run must take the ServiceBusReceivedMessage that carries the [ServiceBusTrigger] attribute.");

        // Deliberately CustomAttributeData, not GetCustomAttributes: the attribute's CLR
        // property defaults to false, so an instantiated attribute reads false whether the
        // argument was written or omitted — a test asserting on the instance passes either way
        // and guards nothing (verified: removing the argument left such a test green).
        // NamedArguments carries only what is actually written in source, which is exactly the
        // distinction that matters, because it is the PRESENCE of the argument that suppresses
        // the host's per-function override.
        var triggerData = messageParameter!.GetCustomAttributesData()
            .SingleOrDefault(a => a.AttributeType == typeof(ServiceBusTriggerAttribute));
        Assert.IsNotNull(triggerData, "The message parameter must carry exactly one [ServiceBusTrigger].");

        var autoComplete = triggerData!.NamedArguments
            .Where(a => a.MemberName == nameof(ServiceBusTriggerAttribute.AutoCompleteMessages))
            .Select(a => (bool?)a.TypedValue.Value)
            .SingleOrDefault();

        Assert.IsNotNull(
            autoComplete,
            "AutoCompleteMessages must be stated EXPLICITLY on the [ServiceBusTrigger] attribute. Omitting it does not mean false: the dotnet-isolated host applies a per-function override that defaults to TRUE and takes precedence over host.json's global extensions.serviceBus.autoCompleteMessages. " +
            "With it true, every deliberate unsettled return in Run/ProcessMessageAsync is auto-completed and the message is DESTROYED: no retry, no dead-letter, no recorded result, and no DLQ entry to show for it. " +
            "That is exactly how correlation ids ...-000235/-000238/-000687 were lost in build 30658, each getting precisely two deliveries and then silence despite maxDeliveryCount=10 and lockDuration=PT5M.");

        Assert.IsFalse(
            autoComplete!.Value,
            "AutoCompleteMessages must be explicitly FALSE — this trigger settles every message itself and relies on unsettled returns meaning 'redeliver me'.");
    }

    // ── WOLF-8512 (build 30658) — drain guard ────────────────────────────────────
    //
    // Correlation ids ...-000687 and ...-000688 were claimed at 23:29:34.880 on instance
    // 5ae10c76, 1.1 SECONDS after that instance had already logged "Stopping JobHost". Its
    // worker was gone by 23:29:43, so both claims leaked with their executions never started -
    // and a leaked claim blocks every later redelivery until the staleness window expires.

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task Run_HostShuttingDown_DoesNotClaim_AndLeavesMessageUnsettled()
    {
        var executor = new FakeWorkflowExecutor(_ => throw new InvalidOperationException("must not execute while draining"));
        var store = new ServiceBusReplayAndResultStore(new MemoryStorage());
        var sut = NewSut(executor: executor, store: store);
        var actions = new FakeServiceBusMessageActions();
        var message = NewMessage("{\"correlationId\":\"corr-draining\"}", correlationId: "corr-draining");

        using var drained = new CancellationTokenSource();
        drained.Cancel();

        await sut.Run(message, actions, drained.Token);

        Assert.AreEqual(0, executor.CallCount, "A draining host must not start new work.");
        Assert.AreEqual(0, actions.CompleteCalls);
        Assert.AreEqual(0, actions.DeadLetterCalls);
        Assert.AreEqual(0, actions.AbandonCalls,
            "Leave the delivery unsettled so its lock expires and it redelivers to a healthy instance (safe because AutoCompleteMessages is false - see Trigger_OptsOutOfAutoCompleteMessages).");
        Assert.IsTrue(store.TryClaim("corr-draining"),
            "No claim may have been taken: a later delivery must find the correlation id completely free, not blocked by a claim this drained instance could never honour.");
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task Run_HostShutsDownImmediatelyAfterClaim_ReleasesClaim()
    {
        // Drain can begin in the window between the guard above and TryClaim returning. This
        // store cancels the host token at the exact moment the claim is taken, which is the
        // only way to land inside that window deterministically.
        using var drainOnClaim = new CancellationTokenSource();
        var inner = new ServiceBusReplayAndResultStore(new MemoryStorage());
        var store = new DrainOnClaimStore(inner, drainOnClaim);

        var executor = new FakeWorkflowExecutor(_ => throw new InvalidOperationException("must not execute while draining"));
        var sut = NewSut(executor: executor, store: store);
        var actions = new FakeServiceBusMessageActions();
        var message = NewMessage("{\"correlationId\":\"corr-drain-race\"}", correlationId: "corr-drain-race");

        await sut.Run(message, actions, drainOnClaim.Token);

        Assert.AreEqual(0, executor.CallCount, "A draining host must not start new work.");
        Assert.AreEqual(1, store.ReleaseClaimCalls,
            "The claim was taken and the host then began draining — it must be handed straight back, or it blocks every redelivery until the staleness window expires.");
        Assert.AreEqual(0, actions.CompleteCalls);
        Assert.AreEqual(0, actions.DeadLetterCalls);
        Assert.AreEqual(0, actions.AbandonCalls);
        Assert.IsTrue(inner.TryClaim("corr-drain-race"),
            "After the release, a later delivery must find the correlation id free.");
    }

    /// <summary>
    /// Decorates the real store, cancelling <paramref name="drain"/> the instant a claim is
    /// taken — standing in for the host entering drain in the narrow window between the
    /// pre-claim guard and the claim landing.
    /// </summary>
    private sealed class DrainOnClaimStore : IServiceBusReplayAndResultStore
    {
        private readonly IServiceBusReplayAndResultStore _inner;
        private readonly CancellationTokenSource _drain;

        public DrainOnClaimStore(IServiceBusReplayAndResultStore inner, CancellationTokenSource drain)
        {
            _inner = inner;
            _drain = drain;
        }

        public int ReleaseClaimCalls { get; private set; }

        public bool TryClaim(string correlationId)
        {
            var claimed = _inner.TryClaim(correlationId);
            if (claimed)
            {
                _drain.Cancel();
            }

            return claimed;
        }

        public void ReleaseClaim(string correlationId)
        {
            ReleaseClaimCalls++;
            _inner.ReleaseClaim(correlationId);
        }

        public bool TryRegisterJti(string jti) => _inner.TryRegisterJti(jti);
        public bool TryGetResult(string correlationId, out ServiceBusTriggerResult? result) => _inner.TryGetResult(correlationId, out result);
        public void SaveResult(ServiceBusTriggerResult result) => _inner.SaveResult(result);
        public void RecordClaimStage(string correlationId, string stage) => _inner.RecordClaimStage(correlationId, stage);
        public void RecordInstanceHeartbeat(string instanceId, int inFlightExecutions) => _inner.RecordInstanceHeartbeat(instanceId, inFlightExecutions);
    }

    // ── WOLF-8512 (build 30658) — "Workflow not found." on a not-ready instance ──
    //
    // Correlation id ...-000688 was dead-lettered with "Workflow not found." while 999 sibling
    // messages ran the very same workflow successfully; build 30649 failed all 1000 this way.
    // WorkflowExecutor now returns a TRANSIENT failure when the miss is explained by an
    // unavailable workflows directory (see WorkflowExecutorTransientFailureTests). This test
    // pins the trigger's half of that contract: a transient not-found must be retried, never
    // dead-lettered, and must persist NO result — a persisted result would satisfy the
    // idempotency dedupe check on redelivery and permanently prevent re-execution.

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task ProcessMessage_TransientWorkflowNotFound_ThrowsInsteadOfDeadLettering_NoResultPersisted()
    {
        var executor = new FakeWorkflowExecutor(_ => WorkflowExecutionResult.TransientFailure(
            "Workflow not found: the workflows directory is unavailable or empty on this instance (not yet ready)."));
        var store = new ServiceBusReplayAndResultStore(new MemoryStorage());
        var sut = NewSut(executor: executor, store: store);
        var actions = new FakeServiceBusMessageActions();
        var message = NewMessage("{}");
        var payload = new ServiceBusWorkflowMessage { Inputs = new Dictionary<string, string>() };

        await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            () => sut.ProcessMessageAsync(payload, "corr-transient-notfound", message, actions, DateTimeOffset.UtcNow, CancellationToken.None),
            "A transient not-found must bubble so the Service Bus extension applies its normal retry/backoff.");

        Assert.AreEqual(0, actions.DeadLetterCalls,
            "Dead-lettering this is what destroyed correlation id ...-000688 while the same workflow ran fine on 999 other messages.");
        Assert.AreEqual(0, actions.CompleteCalls);
        Assert.IsFalse(store.TryGetResult("corr-transient-notfound", out _),
            "No result may be persisted: it would satisfy the dedupe check on redelivery and permanently prevent re-execution.");
    }

    // ── Non-envelope body binding (2026-09-14) ──────────────────────────────────
    //
    // The trigger used to bind inputs ONLY from the {"inputs":{...}} envelope. A flat business
    // payload is still valid JSON, so it deserialized cleanly into Inputs == null and the
    // workflow ran against an EMPTY DATALIST - executed, not rejected - failing deep inside
    // itself on its own missing variables. A 5000-message profiler load test dead-lettered
    // 4998 that way ("Scalar value { MandateDRI } is NULL ... Invalid Calculate"), while the
    // identical payload over HTTP succeeded, because the HTTP path already had this
    // raw-body fallback (Http.WorkflowFunctionHelper.ExtractFromBody). It also defeated the
    // premise of the Shovel bridge, whose whole point is that existing RabbitMQ producers -
    // which publish flat payloads, never a Warewolf envelope - trigger the engine unmodified.

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task Run_FlatBodyWithoutInputsEnvelope_BindsBodyVerbatimAsRawInputPayload()
    {
        const string flatBody = "{\"ForPaymentCycle\":\"202402\",\"MandateDRI\":\"22\",\"ResultPaymentCycle\":\"202401\"}";
        var executor = new FakeWorkflowExecutor(_ => new WorkflowExecutionResult { IsSuccess = true, Payload = "{}" });
        var sut = NewSut(executor: executor);
        var actions = new FakeServiceBusMessageActions();

        await sut.Run(NewMessage(flatBody, correlationId: "corr-flat"), actions, CancellationToken.None);

        Assert.AreEqual(1, executor.CallCount);
        Assert.AreEqual(flatBody, executor.LastRequest!.RawInputPayload,
            "A flat payload must reach the executor verbatim, exactly as the HTTP routes deliver it.");
        Assert.AreEqual(0, executor.LastRequest.InputParameters.Count,
            "There is no envelope, so nothing binds through InputParameters - the raw payload is the whole input.");
        Assert.AreEqual(1, actions.CompleteCalls);
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task Run_InputsEnvelope_BindsThroughInputParameters_AndSetsNoRawPayload()
    {
        var executor = new FakeWorkflowExecutor(_ => new WorkflowExecutionResult { IsSuccess = true, Payload = "{}" });
        var sut = NewSut(executor: executor);
        var actions = new FakeServiceBusMessageActions();

        await sut.Run(
            NewMessage("{\"inputs\":{\"Name\":\"FromServiceBus\"},\"correlationId\":\"corr-env\"}"),
            actions,
            CancellationToken.None);

        Assert.AreEqual(1, executor.CallCount);
        Assert.IsNull(executor.LastRequest!.RawInputPayload,
            "The documented envelope must keep binding through InputParameters alone - handing the envelope " +
            "itself to the executor as a raw payload would try to bind a variable literally named 'inputs'.");
        Assert.AreEqual("FromServiceBus", executor.LastRequest.InputParameters["Name"]);
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task Run_NestedNonEnvelopeBody_ReachesExecutorVerbatim()
    {
        // The reason the raw body is passed through rather than re-synthesised from a
        // Dictionary<string,string>: a dictionary cannot carry a nested object, so recordset
        // inputs could never bind (see WorkflowExecutionRequest.RawInputPayload).
        const string nestedBody = "{\"Mandate\":{\"DRI\":\"22\"},\"Rows\":[{\"Id\":\"1\"}]}";
        var executor = new FakeWorkflowExecutor(_ => new WorkflowExecutionResult { IsSuccess = true, Payload = "{}" });
        var sut = NewSut(executor: executor);

        await sut.Run(NewMessage(nestedBody, correlationId: "corr-nested"), new FakeServiceBusMessageActions(), CancellationToken.None);

        Assert.AreEqual(1, executor.CallCount);
        Assert.AreEqual(nestedBody, executor.LastRequest!.RawInputPayload);
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public async Task Run_XmlBody_StillDeadLettersAsMalformed()
    {
        // Pins the boundary of this change. The HTTP routes accept an XML payload; this trigger
        // deserializes the body into ServiceBusWorkflowMessage FIRST, so a non-JSON body never
        // reaches the raw-payload fallback. Deliberate, and unchanged - asserted so a future
        // reader does not infer full HTTP parity from the flat-JSON case above.
        var executor = new FakeWorkflowExecutor(_ => throw new InvalidOperationException("must not execute"));
        var sut = NewSut(executor: executor);
        var actions = new FakeServiceBusMessageActions();

        await sut.Run(NewMessage("<DataList><MandateDRI>22</MandateDRI></DataList>"), actions, CancellationToken.None);

        Assert.AreEqual(0, executor.CallCount);
        Assert.AreEqual("Malformed:BodyNotJson", actions.LastDeadLetterReason);
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void ResolveRawInputPayload_EmptyBody_ReturnsNull()
    {
        Assert.IsNull(ServiceBusWorkflowTriggerFunction.ResolveRawInputPayload(""));
        Assert.IsNull(ServiceBusWorkflowTriggerFunction.ResolveRawInputPayload("   "));
    }
}
