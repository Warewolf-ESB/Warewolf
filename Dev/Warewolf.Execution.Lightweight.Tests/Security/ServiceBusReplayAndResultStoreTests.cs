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

    // ── Liveness-aware takeover (WOLF-8512, build 30658) ────────────────────────
    //
    // ClaimStaleAfter is a worst-case backstop (11 minutes by default), long enough that it can
    // never cut short a genuinely-running execution - but it cannot tell "dead" from "slow". In
    // build 30658 the claims on ...-000235/-000238 were orphaned by a worker crash at 23:28:22
    // and the first redelivery at 23:33:22 was still refused, so the messages were never
    // re-executed. TryClaim now also consults the owning instance's heartbeat row.
    //
    // Two stores over ONE shared storage stand in for two instances/worker processes - the only
    // way to drive this in a single test process.

    private static ServiceBusReplayAndResultStore NewStoreForInstance(
        Hangfire.JobStorage storage,
        Func<DateTimeOffset> clock,
        string instanceId,
        string processId,
        TimeSpan? instanceLivenessTimeout = null) =>
        new(storage,
            clock,
            claimStaleAfter: TimeSpan.FromMinutes(11),
            instanceLivenessTimeout: instanceLivenessTimeout ?? TimeSpan.FromSeconds(30),
            instanceId: instanceId,
            processId: processId);

    [TestMethod]
    [TestCategory("UnitTest")]
    public void TryClaim_OwningWorkerProcessReplaced_TakesOverImmediately()
    {
        // The decisive signal, and the one that mattered in build 30658: on instances 813073bd
        // and 5ae10c76 the Functions HOST survived (the role instance kept logging) while the
        // isolated WORKER crashed and restarted underneath it. An instance-level liveness check
        // alone would have missed that entirely; a heartbeat arriving from a different process
        // id proves the claiming process is gone, with no timing judgement at all.
        var now = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var clock = now;
        var storage = new MemoryStorage();

        var deadWorker = NewStoreForInstance(storage, () => clock, "instance-A", "pid-100");
        Assert.IsTrue(deadWorker.TryClaim("corr-worker-crash"));
        deadWorker.RecordInstanceHeartbeat("instance-A", 1);

        // The worker process dies and is replaced; the new one heartbeats under the SAME
        // instance id with a new process id, seconds later - nowhere near ClaimStaleAfter.
        clock = now + TimeSpan.FromSeconds(5);
        var restartedWorker = NewStoreForInstance(storage, () => clock, "instance-A", "pid-200");
        restartedWorker.RecordInstanceHeartbeat("instance-A", 0);

        var redelivery = NewStoreForInstance(storage, () => clock, "instance-B", "pid-300");
        Assert.IsTrue(redelivery.TryClaim("corr-worker-crash"),
            "The claim's owning PROCESS has been replaced, so its in-flight execution is gone. The claim must be taken over on the very next redelivery rather than blocking it for the full 11-minute staleness window.");
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void TryClaim_OwningInstanceHeartbeatStale_TakesOverImmediately()
    {
        var now = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var clock = now;
        var storage = new MemoryStorage();

        var dyingInstance = NewStoreForInstance(storage, () => clock, "instance-A", "pid-100");
        Assert.IsTrue(dyingInstance.TryClaim("corr-instance-gone"));
        dyingInstance.RecordInstanceHeartbeat("instance-A", 1);

        // The whole instance goes away (scale-in or outright death) - its heartbeat simply
        // stops. Advance past the liveness timeout but stay far short of ClaimStaleAfter.
        clock = now + TimeSpan.FromSeconds(31);

        var redelivery = NewStoreForInstance(storage, () => clock, "instance-B", "pid-300");
        Assert.IsTrue(redelivery.TryClaim("corr-instance-gone"),
            "The owning instance stopped heartbeating well inside the staleness window — the claim is orphaned and must be taken over now.");
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void TryClaim_OwnerStillHeartbeatingHealthily_StillReturnsFalse()
    {
        // Symmetry guard, and the one that matters most: a false takeover means TWO concurrent
        // executions of the same message. For the ProfilerMain load test that is a duplicate
        // Profiler.ProfilerRecommendation row, which the test explicitly asserts against.
        var now = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var clock = now;
        var storage = new MemoryStorage();

        var busyInstance = NewStoreForInstance(storage, () => clock, "instance-A", "pid-100");
        Assert.IsTrue(busyInstance.TryClaim("corr-slow-but-alive"));
        busyInstance.RecordInstanceHeartbeat("instance-A", 1);

        // A slow execution: time passes, but the owner keeps heartbeating from the same process.
        clock = now + TimeSpan.FromMinutes(3);
        busyInstance.RecordInstanceHeartbeat("instance-A", 1);

        var redelivery = NewStoreForInstance(storage, () => clock, "instance-B", "pid-300");
        Assert.IsFalse(redelivery.TryClaim("corr-slow-but-alive"),
            "The owner is alive and running the same process — the execution is merely slow. Stealing the claim here would duplicate the execution.");
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void TryClaim_OwnerNeverHeartbeated_DoesNotTakeOver()
    {
        // Ambiguity must always resolve to "still held": with no heartbeat row at all we cannot
        // distinguish "died before its first tick" from "heartbeat service not running", so the
        // ClaimStaleAfter backstop stays the only arbiter.
        var now = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var clock = now;
        var storage = new MemoryStorage();

        var silentInstance = NewStoreForInstance(storage, () => clock, "instance-A", "pid-100");
        Assert.IsTrue(silentInstance.TryClaim("corr-no-heartbeat"));

        clock = now + TimeSpan.FromMinutes(5);

        var redelivery = NewStoreForInstance(storage, () => clock, "instance-B", "pid-300");
        Assert.IsFalse(redelivery.TryClaim("corr-no-heartbeat"),
            "No heartbeat row means 'unknown', not 'dead'. Falling back to ClaimStaleAfter is the safe answer.");
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void ReleaseClaim_LeavesADiagnosticTombstone_ButStillAllowsReClaiming()
    {
        // WOLF-8512 (build 30658): ReleaseClaim used to RemoveHash the claim row outright, which
        // also destroyed the RecordClaimStage breadcrumb trail — so a correlation id that failed
        // transiently and was released left nothing to diagnose. The tombstone must not resurrect
        // the claim itself.
        var storage = new MemoryStorage();
        var store = NewStoreForInstance(storage, () => DateTimeOffset.UtcNow, "instance-A", "pid-100");

        Assert.IsTrue(store.TryClaim("corr-tombstone"));
        store.RecordClaimStage("corr-tombstone", "AboutToExecute");
        store.ReleaseClaim("corr-tombstone");

        var (stage, atUtc, instanceId) = store.GetReleaseTombstoneForTests("corr-tombstone");
        Assert.AreEqual("AboutToExecute", stage,
            "The stage the claim had reached must survive its release — that breadcrumb is the whole diagnostic value, and removing the hash outright used to erase it.");
        Assert.IsNotNull(atUtc, "The release must be timestamped.");
        Assert.AreEqual("instance-A", instanceId, "The tombstone must record which instance released the claim.");

        Assert.IsTrue(store.TryClaim("corr-tombstone"),
            "The release tombstone carries no claim timestamp, so a redelivery must still be able to claim the correlation id normally.");
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

    // ── Durable claim-stage breadcrumb (RecordClaimStage) — Hangfire-backed ─────

    [TestMethod]
    [TestCategory("UnitTest")]
    public void RecordClaimStage_AfterClaim_IsReadableViaTheTestSeam()
    {
        var store = NewHangfireBackedStore();
        Assert.IsTrue(store.TryClaim("stage-001"));

        store.RecordClaimStage("stage-001", "ClaimAcquired");

        var (stage, atUtc) = store.GetClaimStageForTests("stage-001");
        Assert.AreEqual("ClaimAcquired", stage);
        Assert.IsNotNull(atUtc);
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void RecordClaimStage_CalledMultipleTimes_LatestStageWins_WithoutClobberingClaimedAtUtc()
    {
        var store = NewHangfireBackedStore();
        Assert.IsTrue(store.TryClaim("stage-002"));

        store.RecordClaimStage("stage-002", "ClaimAcquired");
        store.RecordClaimStage("stage-002", "AboutToExecute");

        var (stage, _) = store.GetClaimStageForTests("stage-002");
        Assert.AreEqual("AboutToExecute", stage, "The most recently recorded stage must win.");
        // The claim itself (written by TryClaim, read by a fresh TryClaim call below) must
        // still be intact — RecordClaimStage merges into the same hash and must not disturb
        // the claimedAtUtc field TryClaim's own staleness check depends on.
        Assert.IsFalse(store.TryClaim("stage-002"), "The original claim must still be held after RecordClaimStage merges extra fields into its hash.");
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void RecordClaimStage_NoPriorClaim_DoesNotThrow()
    {
        var store = NewHangfireBackedStore();

        store.RecordClaimStage("stage-never-claimed", "ClaimAcquired"); // must not throw

        var (stage, _) = store.GetClaimStageForTests("stage-never-claimed");
        Assert.AreEqual("ClaimAcquired", stage, "Recording a stage with no prior claim still creates a readable breadcrumb row.");
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void RecordClaimStage_EmptyOrWhitespaceCorrelationId_IsANoOp()
    {
        var store = NewHangfireBackedStore();

        store.RecordClaimStage(string.Empty, "ClaimAcquired"); // must not throw
        store.RecordClaimStage("   ", "ClaimAcquired"); // must not throw

        Assert.AreEqual((null, (DateTimeOffset?)null), store.GetClaimStageForTests(string.Empty));
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void GetClaimStageForTests_NoStageEverRecorded_ReturnsNull()
    {
        var store = NewHangfireBackedStore();
        Assert.IsTrue(store.TryClaim("stage-untouched"));

        var (stage, atUtc) = store.GetClaimStageForTests("stage-untouched");

        Assert.IsNull(stage);
        Assert.IsNull(atUtc);
    }

    // ── Claim-stage instance/process identity (build 30653 addendum) — Hangfire-backed ──

    [TestMethod]
    [TestCategory("UnitTest")]
    public void RecordClaimStage_HangfireBacked_RecordsInstanceAndProcessIdAlongsideStage()
    {
        var store = NewHangfireBackedStore();
        Assert.IsTrue(store.TryClaim("stage-instance-001"));

        store.RecordClaimStage("stage-instance-001", "ClaimAcquired");

        var (instanceId, processId) = store.GetClaimInstanceForTests("stage-instance-001");
        Assert.IsFalse(string.IsNullOrWhiteSpace(instanceId), "A stage write must always record SOME instance identifier (falls back to Environment.MachineName off-Azure).");
        Assert.IsFalse(string.IsNullOrWhiteSpace(processId));
        Assert.AreEqual(Environment.ProcessId.ToString(), processId);
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void GetClaimInstanceForTests_NoStageEverRecorded_ReturnsNull()
    {
        var store = NewHangfireBackedStore();
        Assert.IsTrue(store.TryClaim("stage-instance-untouched"));

        var (instanceId, processId) = store.GetClaimInstanceForTests("stage-instance-untouched");

        Assert.IsNull(instanceId);
        Assert.IsNull(processId);
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    [DoNotParallelize] // swaps process-global Config.Persistence
    public void PersistenceDisabled_GetClaimInstanceForTests_AlwaysReturnsNull_InstanceIdentityIsPersistenceOnly()
    {
        using var _ = ResumeTestSupport.SwapPersistence(enable: false);
        var store = new ServiceBusReplayAndResultStore();
        store.RecordClaimStage("fallback-stage-instance", "ClaimAcquired");

        var (instanceId, processId) = store.GetClaimInstanceForTests("fallback-stage-instance");

        Assert.IsNull(instanceId);
        Assert.IsNull(processId);
    }

    // ── Instance liveness heartbeat (RecordInstanceHeartbeat) — Hangfire-backed ─

    [TestMethod]
    [TestCategory("UnitTest")]
    public void RecordInstanceHeartbeat_IsReadableViaTheTestSeam()
    {
        var store = NewHangfireBackedStore();

        store.RecordInstanceHeartbeat("instance-A", inFlightExecutions: 3);

        var (inFlight, atUtc) = store.GetInstanceHeartbeatForTests("instance-A");
        Assert.AreEqual(3, inFlight);
        Assert.IsNotNull(atUtc);
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void RecordInstanceHeartbeat_CalledMultipleTimes_LatestOverwritesPrevious()
    {
        var store = NewHangfireBackedStore();

        store.RecordInstanceHeartbeat("instance-B", inFlightExecutions: 1);
        store.RecordInstanceHeartbeat("instance-B", inFlightExecutions: 5);

        var (inFlight, _) = store.GetInstanceHeartbeatForTests("instance-B");
        Assert.AreEqual(5, inFlight, "The most recently recorded heartbeat must win.");
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void RecordInstanceHeartbeat_TwoDifferentInstances_TrackedIndependently()
    {
        var store = NewHangfireBackedStore();

        store.RecordInstanceHeartbeat("instance-C1", inFlightExecutions: 2);
        store.RecordInstanceHeartbeat("instance-C2", inFlightExecutions: 7);

        Assert.AreEqual(2, store.GetInstanceHeartbeatForTests("instance-C1").InFlightExecutions);
        Assert.AreEqual(7, store.GetInstanceHeartbeatForTests("instance-C2").InFlightExecutions);
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void RecordInstanceHeartbeat_EmptyOrWhitespaceInstanceId_IsANoOp()
    {
        var store = NewHangfireBackedStore();

        store.RecordInstanceHeartbeat(string.Empty, 1); // must not throw
        store.RecordInstanceHeartbeat("   ", 1); // must not throw

        Assert.AreEqual((null, (DateTimeOffset?)null), store.GetInstanceHeartbeatForTests(string.Empty));
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    public void GetInstanceHeartbeatForTests_NeverRecorded_ReturnsNull()
    {
        var store = NewHangfireBackedStore();

        var (inFlight, atUtc) = store.GetInstanceHeartbeatForTests("instance-never-seen");

        Assert.IsNull(inFlight);
        Assert.IsNull(atUtc);
    }

    // ── Instance liveness heartbeat (RecordInstanceHeartbeat) — in-memory fallback ─

    [TestMethod]
    [TestCategory("UnitTest")]
    [DoNotParallelize] // swaps process-global Config.Persistence
    public void PersistenceDisabled_RecordInstanceHeartbeat_IsReadableViaTheTestSeam()
    {
        using var _ = ResumeTestSupport.SwapPersistence(enable: false);
        var store = new ServiceBusReplayAndResultStore();

        store.RecordInstanceHeartbeat("fallback-instance", inFlightExecutions: 4);

        var (inFlight, atUtc) = store.GetInstanceHeartbeatForTests("fallback-instance");
        Assert.AreEqual(4, inFlight);
        Assert.IsNotNull(atUtc);
    }

    // ── Durable claim-stage breadcrumb (RecordClaimStage) — in-memory fallback ──

    [TestMethod]
    [TestCategory("UnitTest")]
    [DoNotParallelize] // swaps process-global Config.Persistence
    public void PersistenceDisabled_RecordClaimStage_IsReadableViaTheTestSeam()
    {
        using var _ = ResumeTestSupport.SwapPersistence(enable: false);
        var store = new ServiceBusReplayAndResultStore();
        Assert.IsTrue(store.TryClaim("fallback-stage-001"));

        store.RecordClaimStage("fallback-stage-001", "AboutToExecute");

        var (stage, atUtc) = store.GetClaimStageForTests("fallback-stage-001");
        Assert.AreEqual("AboutToExecute", stage);
        Assert.IsNotNull(atUtc);
    }

    [TestMethod]
    [TestCategory("UnitTest")]
    [DoNotParallelize] // swaps process-global Config.Persistence
    public void PersistenceDisabled_RecordClaimStage_EmptyCorrelationId_IsANoOp()
    {
        using var _ = ResumeTestSupport.SwapPersistence(enable: false);
        var store = new ServiceBusReplayAndResultStore();

        store.RecordClaimStage(string.Empty, "ClaimAcquired"); // must not throw

        Assert.AreEqual((null, (DateTimeOffset?)null), store.GetClaimStageForTests(string.Empty));
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
