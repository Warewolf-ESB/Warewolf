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

    // ── Claim reservation / race-window closure (Hangfire-backed) ───────────────

    [TestMethod]
    [TestCategory("UnitTest")]
    public void TryClaim_FirstTime_ReturnsTrue()
    {
        var store = NewHangfireBackedStore();

        Assert.IsTrue(store.TryClaim("claim-001"));
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void TryClaim_SecondTimeWhileStillClaimed_ReturnsFalse()
    {
        var store = NewHangfireBackedStore();
        Assert.IsTrue(store.TryClaim("claim-002"));

        var second = store.TryClaim("claim-002");

        Assert.IsFalse(second, "A correlation id already claimed (no result yet) must reject a second concurrent claim.");
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void TryClaim_DifferentCorrelationIds_BothSucceed()
    {
        var store = NewHangfireBackedStore();

        Assert.IsTrue(store.TryClaim("claim-a"));
        Assert.IsTrue(store.TryClaim("claim-b"));
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void TryClaim_ResultAlreadySaved_ReturnsFalse_EvenWithoutAPriorClaim()
    {
        // Mirrors a genuine duplicate delivery arriving after the first attempt already
        // completed and saved its result - TryClaim must reject it just as reliably as the
        // in-flight-claim case above, without ever needing an explicit claim to have been
        // taken first.
        var store = NewHangfireBackedStore();
        store.SaveResult(SampleResult("claim-with-result"));

        Assert.IsFalse(store.TryClaim("claim-with-result"));
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void ReleaseClaim_ThenTryClaim_SucceedsAgain()
    {
        var store = NewHangfireBackedStore();
        Assert.IsTrue(store.TryClaim("claim-release"));

        store.ReleaseClaim("claim-release");

        Assert.IsTrue(store.TryClaim("claim-release"),
            "Releasing a claim must allow a subsequent (e.g. redelivered) attempt to claim it again.");
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void ReleaseClaim_UnknownCorrelationId_IsANoOp()
    {
        var store = NewHangfireBackedStore();

        store.ReleaseClaim("never-claimed"); // must not throw

        Assert.IsTrue(store.TryClaim("never-claimed"));
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void ReleaseClaim_AfterResultSaved_DoesNotAllowReClaimingAResolvedCorrelationId()
    {
        // ReleaseClaim is only ever called on the throw paths, which never save a result -
        // but defensively, releasing after a result exists must not reopen a genuinely
        // completed correlation id to re-execution.
        var store = NewHangfireBackedStore();
        Assert.IsTrue(store.TryClaim("claim-then-result"));
        store.SaveResult(SampleResult("claim-then-result"));

        store.ReleaseClaim("claim-then-result");

        Assert.IsFalse(store.TryClaim("claim-then-result"),
            "A saved result must still block re-claiming even after a (harmless, no-op) release.");
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void TryClaim_ConcurrentClaimsForSameCorrelationId_ExactlyOneSucceeds()
    {
        var store = NewHangfireBackedStore();
        const int attempts = 8;
        var barrier = new Barrier(attempts);
        var results = new bool[attempts];

        Parallel.For(0, attempts, i =>
        {
            barrier.SignalAndWait();
            results[i] = store.TryClaim("claim-concurrent");
        });

        Assert.AreEqual(1, results.Count(r => r), "Exactly one concurrent claim attempt for the same correlation id must succeed.");
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

    [TestMethod]
    [TestCategory("UnitTest")]
    [DoNotParallelize] // swaps process-global Config.Persistence
    public void PersistenceDisabled_TryClaim_StillClosesTheSameRaceWindowForOneInstance()
    {
        using var _ = ResumeTestSupport.SwapPersistence(enable: false);
        var store = new ServiceBusReplayAndResultStore();

        Assert.IsTrue(store.TryClaim("fallback-claim"));
        Assert.IsFalse(store.TryClaim("fallback-claim"), "A second claim while the first is still in flight must be rejected even in the single-instance fallback.");

        store.ReleaseClaim("fallback-claim");
        Assert.IsTrue(store.TryClaim("fallback-claim"), "Releasing must allow re-claiming in the fallback path too.");

        store.SaveResult(SampleResult("fallback-claim"));
        Assert.IsFalse(store.TryClaim("fallback-claim"), "A saved result must block re-claiming in the fallback path too.");
    }
}
