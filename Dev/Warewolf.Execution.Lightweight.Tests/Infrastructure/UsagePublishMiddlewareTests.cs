/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Tests for UsagePublishMiddleware — the first-registered Functions worker
 *  middleware (8501) that publishes per-invocation usage/uptime telemetry.
 *  Scope: the middleware's own contract (timing spans `next()`, publish only
 *  when FunctionContext.Items carries a UsagePublishContext under
 *  UsagePublishContext.ItemsKey, publish still happens on error paths, the
 *  Items entry never leaks across invocations). This mirrors what
 *  WorkflowHttpFunction does in production: it calls WorkflowExecutor.Execute
 *  synchronously and immediately stashes UsagePublishContext.TakeCurrent()
 *  into context.Items — these tests simulate that same hand-off directly via
 *  context.Items rather than via the AsyncLocal, since context.Items (not the
 *  AsyncLocal) is what the middleware actually reads. WorkflowExecutor's own
 *  behaviour is covered separately in WorkflowExecutorUsagePublishTests.
 */

using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Execution.Lightweight.Infrastructure;
using Warewolf.Execution.Lightweight.Logging;
using Warewolf.Execution.Lightweight.Tests.Auth;

namespace Warewolf.Execution.Lightweight.Tests.Infrastructure;

[TestClass]
public class UsagePublishMiddlewareTests
{
    [TestCleanup]
    public void Cleanup()
    {
        // Defence in depth: never let a failed assertion mid-test leave the
        // AsyncLocal slot set for a later test running on a reused thread.
        UsagePublishContext.Current = null;
    }

    // ── Constructor ───────────────────────────────────────────────────────────

    [TestMethod]
    public void Constructor_NullEmitter_Throws()
    {
        Assert.ThrowsException<ArgumentNullException>(() => new UsagePublishMiddleware(null!));
    }

    // ── No context recorded downstream ─────────────────────────────────────────

    [TestMethod]
    public async Task Invoke_NoUsagePublishContextSet_CallsNext_NoEventPublished()
    {
        var emitter = new FakeUsageEventEmitter();
        var middleware = new UsagePublishMiddleware(emitter);
        var context = new TestFunctionContext();
        var nextCalled = false;

        await middleware.Invoke(context, _ => { nextCalled = true; return Task.CompletedTask; });

        Assert.IsTrue(nextCalled, "next() must always be called.");
        Assert.AreEqual(0, emitter.Calls.Count,
            "Requests that never reach WorkflowExecutor.Execute (health/login/licensing/auth-denials) must not publish a usage event.");
    }

    // ── Context recorded downstream (happy path) ────────────────────────────────

    [TestMethod]
    public async Task Invoke_UsagePublishContextSetDuringNext_PublishesEventWithFullDuration_AfterNextReturns()
    {
        var emitter = new FakeUsageEventEmitter();
        var middleware = new UsagePublishMiddleware(emitter);
        var context = new TestFunctionContext();
        var executionId = Guid.NewGuid();

        await middleware.Invoke(context, async _ =>
        {
            // Simulate WorkflowHttpFunction: it calls WorkflowExecutor.Execute()
            // (which may itself be reached after some awaited downstream work),
            // then immediately stashes the recorded facts into context.Items —
            // exactly what FlushUsagePublishContext does in production.
            await Task.Delay(20);
            context.Items[UsagePublishContext.ItemsKey] = new UsagePublishContext
            {
                WorkflowName = "Hello World",
                ExecutionId = executionId,
                IsSuccess = true,
                ErrorCount = 0
            };
        });

        Assert.AreEqual(1, emitter.Calls.Count, "Exactly one usage event must be published.");
        var evt = emitter.Calls[0];
        Assert.AreEqual("Hello World", evt.WorkflowName);
        Assert.AreEqual(executionId, evt.ExecutionId);
        Assert.IsTrue(evt.IsSuccess);
        Assert.AreEqual(0, evt.ErrorCount);
        Assert.IsTrue(evt.Duration >= TimeSpan.FromMilliseconds(20),
            $"Duration must span the full next() call (including the simulated 20ms of downstream work), was {evt.Duration.TotalMilliseconds}ms.");
    }

    [TestMethod]
    public async Task Invoke_ContextSet_ClearsItemsEntryAfterPublish()
    {
        var emitter = new FakeUsageEventEmitter();
        var middleware = new UsagePublishMiddleware(emitter);
        var context = new TestFunctionContext();

        await middleware.Invoke(context, _ =>
        {
            context.Items[UsagePublishContext.ItemsKey] = new UsagePublishContext { WorkflowName = "wf", ExecutionId = Guid.NewGuid(), IsSuccess = true };
            return Task.CompletedTask;
        });

        Assert.IsFalse(context.Items.ContainsKey(UsagePublishContext.ItemsKey),
            "The Items entry must be cleared after publishing so it cannot leak into an unrelated later invocation on a reused FunctionContext.");
    }

    [TestMethod]
    public async Task Invoke_SecondInvocation_DoesNotInheritFirstInvocationsContext()
    {
        var emitter = new FakeUsageEventEmitter();
        var middleware = new UsagePublishMiddleware(emitter);

        // First invocation records a usage event.
        var firstContext = new TestFunctionContext();
        await middleware.Invoke(firstContext, _ =>
        {
            firstContext.Items[UsagePublishContext.ItemsKey] = new UsagePublishContext { WorkflowName = "first", ExecutionId = Guid.NewGuid(), IsSuccess = true };
            return Task.CompletedTask;
        });

        // Second invocation uses a fresh FunctionContext (as a real request
        // would) whose next() never touches WorkflowExecutor (e.g. a
        // health-check route) — it must NOT see, or re-publish, "first"'s event.
        await middleware.Invoke(new TestFunctionContext(), _ => Task.CompletedTask);

        Assert.AreEqual(1, emitter.Calls.Count,
            "Only the first invocation should have published; the second must not inherit stale ambient state.");
    }

    // ── Error paths ──────────────────────────────────────────────────────────

    [TestMethod]
    public async Task Invoke_NextThrows_ContextWasSetBeforeThrow_StillPublishesEvent_AndRethrows()
    {
        var emitter = new FakeUsageEventEmitter();
        var middleware = new UsagePublishMiddleware(emitter);
        var context = new TestFunctionContext();
        var executionId = Guid.NewGuid();

        var ex = await Assert.ThrowsExceptionAsync<InvalidOperationException>(() =>
            middleware.Invoke(context, _ =>
            {
                context.Items[UsagePublishContext.ItemsKey] = new UsagePublishContext
                {
                    WorkflowName = "wf",
                    ExecutionId = executionId,
                    IsSuccess = false,
                    ErrorCount = 1
                };
                throw new InvalidOperationException("downstream blew up after recording usage facts");
            }));

        Assert.AreEqual("downstream blew up after recording usage facts", ex.Message,
            "The original exception must propagate unmodified — publishing must not swallow it.");
        Assert.AreEqual(1, emitter.Calls.Count,
            "Usage must still be published on the error path (finally block), matching the pre-move behaviour of always tracking attempted executions.");
        Assert.IsFalse(emitter.Calls[0].IsSuccess);
        Assert.AreEqual(1, emitter.Calls[0].ErrorCount);
    }

    [TestMethod]
    public async Task Invoke_NextThrows_ContextNeverSet_NoEventEmitted_ExceptionRethrown()
    {
        var emitter = new FakeUsageEventEmitter();
        var middleware = new UsagePublishMiddleware(emitter);
        var context = new TestFunctionContext();

        await Assert.ThrowsExceptionAsync<InvalidOperationException>(() =>
            middleware.Invoke(context, _ => throw new InvalidOperationException("blew up before reaching WorkflowExecutor")));

        Assert.AreEqual(0, emitter.Calls.Count,
            "No usage event should be published when the failure happens before any workflow execution facts were recorded (e.g. auth middleware threw).");
    }

    // ── Test double ───────────────────────────────────────────────────────────

    private sealed class FakeUsageEventEmitter : IUsageEventEmitter
    {
        public System.Collections.Generic.List<WorkflowUsageEvent> Calls { get; } = new();

        public void TrackWorkflowExecution(WorkflowUsageEvent evt) => Calls.Add(evt);
    }
}
