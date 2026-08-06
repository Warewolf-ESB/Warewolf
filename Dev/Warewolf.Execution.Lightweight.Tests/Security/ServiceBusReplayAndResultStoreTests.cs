/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Unit tests for ServiceBusReplayAndResultStore — the backing store for the secure
 *  Service Bus trigger's jti replay protection and correlation-id idempotency/result
 *  lookup (Model A — Spec-Secure-ServiceBus-Triggered-Execution.md §6/§7).
 *
 *  The Hangfire-backed path is exercised against Hangfire.MemoryStorage via the
 *  store's internal test-seam constructor (mirrors the pattern already used by
 *  ResumptionExecutorTests / WorkflowResumeFunctionTests for the suspend/resume
 *  Hangfire integration). The in-memory-fallback path (Config.Persistence disabled)
 *  is exercised via the public constructor with ResumeTestSupport.SwapPersistence.
 */

using Hangfire.MemoryStorage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Execution.Lightweight.Models;
using Warewolf.Execution.Lightweight.Security;
using Warewolf.Execution.Lightweight.Tests.Execution;

namespace Warewolf.Execution.Lightweight.Tests.Security;

[TestClass]
public class ServiceBusReplayAndResultStoreTests
{
    private static ServiceBusReplayAndResultStore NewHangfireBackedStore() =>
        new(new MemoryStorage());

    private static ServiceBusTriggerResult SampleResult(string correlationId, ServiceBusTriggerStatus status = ServiceBusTriggerStatus.Succeeded) =>
        new()
        {
            CorrelationId = correlationId,
            Status = status,
            Workflow = "Hello World",
            Caller = "alice@example.com",
            Outputs = status == ServiceBusTriggerStatus.Succeeded ? "{\"Greeting\":\"Hi\"}" : null,
            Error = status == ServiceBusTriggerStatus.Succeeded ? null : "boom",
            ReceivedAtUtc = DateTimeOffset.UtcNow,
            CompletedAtUtc = DateTimeOffset.UtcNow,
        };

    // ── jti replay protection (Hangfire-backed) ─────────────────────────────────

    [TestMethod]
    [TestCategory("UnitTest")]
    public void TryRegisterJti_FirstTime_ReturnsTrue()
    {
        var store = NewHangfireBackedStore();

        Assert.IsTrue(store.TryRegisterJti("jti-001"));
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void TryRegisterJti_SecondTimeForSameJti_ReturnsFalse_ReplayRejected()
    {
        var store = NewHangfireBackedStore();
        Assert.IsTrue(store.TryRegisterJti("jti-002"));

        var second = store.TryRegisterJti("jti-002");

        Assert.IsFalse(second, "A jti already registered must be rejected as a replay on every subsequent call.");
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void TryRegisterJti_DifferentJtis_BothSucceed()
    {
        var store = NewHangfireBackedStore();

        Assert.IsTrue(store.TryRegisterJti("jti-a"));
        Assert.IsTrue(store.TryRegisterJti("jti-b"));
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void TryRegisterJti_EmptyOrWhitespace_AlwaysReturnsTrue_NoDedupeKey()
    {
        var store = NewHangfireBackedStore();

        Assert.IsTrue(store.TryRegisterJti(string.Empty));
        Assert.IsTrue(store.TryRegisterJti(string.Empty), "No jti to dedupe on — never treated as a replay.");
        Assert.IsTrue(store.TryRegisterJti("   "));
    }

    // ── Correlation-id idempotency / result round-trip (Hangfire-backed) ────────

    [TestMethod]
    [TestCategory("UnitTest")]
    public void TryGetResult_UnknownCorrelationId_ReturnsFalse()
    {
        var store = NewHangfireBackedStore();

        var found = store.TryGetResult("no-such-correlation-id", out var result);

        Assert.IsFalse(found);
        Assert.IsNull(result);
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void SaveResult_ThenTryGetResult_RoundTripsAllFields()
    {
        var store = NewHangfireBackedStore();
        var saved = SampleResult("corr-001");

        store.SaveResult(saved);
        var found = store.TryGetResult("corr-001", out var loaded);

        Assert.IsTrue(found);
        Assert.IsNotNull(loaded);
        Assert.AreEqual(saved.CorrelationId, loaded!.CorrelationId);
        Assert.AreEqual(saved.Status, loaded.Status);
        Assert.AreEqual(saved.Workflow, loaded.Workflow);
        Assert.AreEqual(saved.Caller, loaded.Caller);
        Assert.AreEqual(saved.Outputs, loaded.Outputs);
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void SaveResult_OverwritesPreviousResultForSameCorrelationId()
    {
        var store = NewHangfireBackedStore();
        store.SaveResult(SampleResult("corr-002", ServiceBusTriggerStatus.Denied));

        store.SaveResult(SampleResult("corr-002", ServiceBusTriggerStatus.Succeeded));

        store.TryGetResult("corr-002", out var loaded);
        Assert.AreEqual(ServiceBusTriggerStatus.Succeeded, loaded!.Status);
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void SaveResult_NullResult_Throws()
    {
        var store = NewHangfireBackedStore();

        Assert.ThrowsException<ArgumentNullException>(() => store.SaveResult(null!));
    }

    // ── In-memory fallback (Config.Persistence disabled) ────────────────────────

    [TestMethod]
    [TestCategory("UnitTest")]
    [DoNotParallelize] // swaps process-global Config.Persistence
    public void PersistenceDisabled_InMemoryFallback_StillDedupesJtiAndRoundTripsResults()
    {
        using var _ = ResumeTestSupport.SwapPersistence(enable: false);
        var store = new ServiceBusReplayAndResultStore();

        Assert.IsTrue(store.TryRegisterJti("fallback-jti"));
        Assert.IsFalse(store.TryRegisterJti("fallback-jti"),
            "The in-memory fallback must still dedupe jtis correctly for a single instance.");

        store.SaveResult(SampleResult("fallback-corr"));
        var found = store.TryGetResult("fallback-corr", out var result);

        Assert.IsTrue(found);
        Assert.AreEqual("fallback-corr", result!.CorrelationId);
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    [DoNotParallelize] // swaps process-global Config.Persistence
    public void PersistenceDisabled_UnknownCorrelationId_ReturnsFalse()
    {
        using var _ = ResumeTestSupport.SwapPersistence(enable: false);
        var store = new ServiceBusReplayAndResultStore();

        Assert.IsFalse(store.TryGetResult("does-not-exist", out var result));
        Assert.IsNull(result);
    }
}
