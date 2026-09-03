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

    private static ServiceBusReplayAndResultStore NewHangfireBackedStore(Func<DateTimeOffset> clock) =>
        new(new MemoryStorage(), clock);

    private static ServiceBusReplayAndResultStore NewHangfireBackedStore(Func<DateTimeOffset> clock, TimeSpan claimStaleAfter) =>
        new(new MemoryStorage(), clock, claimStaleAfter);

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

    // ── Claim staleness / stale-claim takeover (Hangfire-backed) ────────────────

    [TestMethod]
    [TestCategory("UnitTest")]
    public void TryClaim_ExistingClaimYoungerThanStaleThreshold_StillReturnsFalse()
    {
        var now = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var clock = now;
        var store = NewHangfireBackedStore(() => clock);
        Assert.IsTrue(store.TryClaim("claim-fresh"));

        // Advance right up to (but not past) the staleness threshold.
        clock = now + ServiceBusReplayAndResultStore.DefaultClaimStaleAfter;

        Assert.IsFalse(store.TryClaim("claim-fresh"),
            "A claim exactly at the staleness threshold has not yet been exceeded and must still block a second claim.");
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void TryClaim_ExistingClaimOlderThanStaleThreshold_IsTakenOverByNewDelivery()
    {
        var now = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var clock = now;
        var store = NewHangfireBackedStore(() => clock);
        Assert.IsTrue(store.TryClaim("claim-stale"));

        // Advance past the staleness threshold - the original attempt is presumed dead/hung.
        clock = now + ServiceBusReplayAndResultStore.DefaultClaimStaleAfter + TimeSpan.FromSeconds(1);

        Assert.IsTrue(store.TryClaim("claim-stale"),
            "A claim older than the staleness threshold must be treated as abandoned and taken over by a new delivery.");
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void TryClaim_StaleClaimTakenOver_ResetsTheClaimTimestampForTheNewAttempt()
    {
        var now = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var clock = now;
        var store = NewHangfireBackedStore(() => clock);
        Assert.IsTrue(store.TryClaim("claim-reset"));

        clock = now + ServiceBusReplayAndResultStore.DefaultClaimStaleAfter + TimeSpan.FromSeconds(1);
        Assert.IsTrue(store.TryClaim("claim-reset"), "The stale claim must be taken over.");

        // A third attempt immediately afterwards must see the NEW claim as fresh, not stale.
        clock += TimeSpan.FromSeconds(1);
        Assert.IsFalse(store.TryClaim("claim-reset"),
            "After a stale claim is taken over, the new claim's timestamp must reset - an immediate third attempt must be rejected as in-flight, not treated as stale again.");
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void TryClaim_NonDefaultClaimStaleAfter_StalenessIsMeasuredAgainstTheInjectedValue_NotTheDefaultConstant()
    {
        // WOLF-8512: ClaimStaleAfter is now resolved from ServiceBusTriggerOptions (see its
        // ResolveHostFunctionTimeout five-tier strategy) rather than fixed at 20 minutes -
        // confirm the constructor's optional override actually drives TryClaim's staleness
        // check, using a window well below DefaultClaimStaleAfter so a regression back to the
        // static constant would make this test fail (staleness would never trigger within it).
        var customClaimStaleAfter = TimeSpan.FromMinutes(2);
        Assert.IsTrue(customClaimStaleAfter < ServiceBusReplayAndResultStore.DefaultClaimStaleAfter,
            "Test setup invariant: the injected window must be shorter than the default so this test can't pass for the wrong reason.");

        var now = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var clock = now;
        var store = NewHangfireBackedStore(() => clock, customClaimStaleAfter);
        Assert.IsTrue(store.TryClaim("claim-custom-window"));

        // Still within the injected 2-minute window - must remain blocked.
        clock = now + TimeSpan.FromMinutes(1);
        Assert.IsFalse(store.TryClaim("claim-custom-window"),
            "A claim younger than the injected ClaimStaleAfter must still block a second claim.");

        // Past the injected window (but nowhere near the 20-minute default) - must be taken over.
        clock = now + customClaimStaleAfter + TimeSpan.FromSeconds(1);
        Assert.IsTrue(store.TryClaim("claim-custom-window"),
            "A claim older than the INJECTED ClaimStaleAfter must be taken over, even though it is nowhere near DefaultClaimStaleAfter.");
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void TryClaim_ResultAlreadySaved_StillReturnsFalse_RegardlessOfClaimAge()
    {
        // A stale claim is only ever taken over when NO terminal result exists - if the
        // original attempt actually finished and saved a result, that must still win over
        // any staleness reasoning about the claim.
        var now = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var clock = now;
        var store = NewHangfireBackedStore(() => clock);
        Assert.IsTrue(store.TryClaim("claim-then-completed"));
        store.SaveResult(SampleResult("claim-then-completed"));

        clock = now + ServiceBusReplayAndResultStore.DefaultClaimStaleAfter + TimeSpan.FromDays(1);

        Assert.IsFalse(store.TryClaim("claim-then-completed"),
            "A saved terminal result must block re-claiming regardless of how old the original claim is.");
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

    [TestMethod]
    [TestCategory("UnitTest")]
    [DoNotParallelize] // swaps process-global Config.Persistence
    public void PersistenceDisabled_TryClaim_ExistingClaimYoungerThanStaleThreshold_StillReturnsFalse()
    {
        using var _ = ResumeTestSupport.SwapPersistence(enable: false);
        var now = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var clock = now;
        var store = new ServiceBusReplayAndResultStore(() => clock);
        Assert.IsTrue(store.TryClaim("fallback-claim-fresh"));

        clock = now + ServiceBusReplayAndResultStore.DefaultClaimStaleAfter;

        Assert.IsFalse(store.TryClaim("fallback-claim-fresh"),
            "A claim exactly at the staleness threshold has not yet been exceeded and must still block a second claim in the fallback path too.");
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    [DoNotParallelize] // swaps process-global Config.Persistence
    public void PersistenceDisabled_TryClaim_ExistingClaimOlderThanStaleThreshold_IsTakenOverByNewDelivery()
    {
        using var _ = ResumeTestSupport.SwapPersistence(enable: false);
        var now = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var clock = now;
        var store = new ServiceBusReplayAndResultStore(() => clock);
        Assert.IsTrue(store.TryClaim("fallback-claim-stale"));

        clock = now + ServiceBusReplayAndResultStore.DefaultClaimStaleAfter + TimeSpan.FromSeconds(1);

        Assert.IsTrue(store.TryClaim("fallback-claim-stale"),
            "A claim older than the staleness threshold must be taken over by a new delivery in the fallback path too - this is the exact mechanism that recovers correlation ids stuck by a hung execution (see the 2026-08-24 ShovelBridge load test incident).");
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    [DoNotParallelize] // swaps process-global Config.Persistence
    public void PersistenceDisabled_TryClaim_ConcurrentStaleClaimAttempts_ExactlyOneSucceeds()
    {
        using var _ = ResumeTestSupport.SwapPersistence(enable: false);
        var now = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var clock = now;
        var store = new ServiceBusReplayAndResultStore(() => clock);
        Assert.IsTrue(store.TryClaim("fallback-claim-concurrent-stale"));
        clock = now + ServiceBusReplayAndResultStore.DefaultClaimStaleAfter + TimeSpan.FromSeconds(1);

        const int attempts = 8;
        var barrier = new Barrier(attempts);
        var results = new bool[attempts];

        Parallel.For(0, attempts, i =>
        {
            barrier.SignalAndWait();
            results[i] = store.TryClaim("fallback-claim-concurrent-stale");
        });

        Assert.AreEqual(1, results.Count(r => r),
            "Exactly one of several concurrent attempts to take over the same stale claim must succeed.");
    }
}
