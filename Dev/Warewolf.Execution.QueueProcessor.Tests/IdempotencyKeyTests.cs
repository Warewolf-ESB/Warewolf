/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using System.Net;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Data;
using Warewolf.Execution.QueueProcessor.Configuration;
using Warewolf.Execution.QueueProcessor.Consumers;
using Warewolf.Execution.QueueProcessor.Engine;
using Warewolf.Execution.QueueProcessor.Messaging;

namespace Warewolf.Execution.QueueProcessor.Tests
{
    /// <summary>
    /// Pins the de-duplication contract the engine depends on.
    ///
    /// <para>The bug these guard against: <c>Warewolf-Execution-Id</c> is minted with
    /// <c>Guid.NewGuid()</c> on every <c>Consume</c> call, so a redelivered message reaches the
    /// engine with a brand-new id and looks like a first execution. Raising MaxConcurrency and
    /// pinning maxReplicas to 1 makes redelivery MORE likely (a drain timeout can strand every
    /// in-flight message at once), so the engine needs a key that is a property of the MESSAGE
    /// rather than of the attempt.</para>
    /// </summary>
    [TestClass]
    public class IdempotencyKeyTests
    {
        static string SettingsDir => Path.Combine(AppContext.BaseDirectory, "TestResources");

        static string ResourcePath(string name) => Path.Combine(SettingsDir, "sources", name);
        static string TriggerPath(string name) => Path.Combine(SettingsDir, "triggers", name);

        Mock<IEngineWorkflowClient> _engine = null!;
        Mock<IDeadLetterPublisher> _deadLetter = null!;
        ResolvedQueueConfiguration _config = null!;
        List<IReadOnlyDictionary<string, string>> _captured = null!;

        [TestInitialize]
        public void Setup()
        {
            _engine = new Mock<IEngineWorkflowClient>();
            _deadLetter = new Mock<IDeadLetterPublisher>();
            _captured = new List<IReadOnlyDictionary<string, string>>();

            var trigger = new TriggerBiteReader().Read(TriggerPath("triggers-mandate.bite"));
            var source = RabbitMqSourceOptions.FromBiteFile(
                ResourcePath("0b142714-8f6d-41b7-9832-2aefa8c731ec.bite"));

            _config = new ResolvedQueueConfiguration
            {
                Trigger = trigger, Source = source, DeadLetterSource = source,
            };

            _engine.Setup(e => e.PostSecureAsync(
                       It.IsAny<string>(), It.IsAny<string>(), It.IsAny<byte[]?>(),
                       It.IsAny<string?>(), It.IsAny<IReadOnlyDictionary<string, string>>(),
                       It.IsAny<CancellationToken>()))
                   .Callback<string, string, byte[]?, string?, IReadOnlyDictionary<string, string>, CancellationToken>(
                       (_, _, _, _, headers, _) => _captured.Add(headers))
                   .ReturnsAsync(new EngineCallResult(
                       EngineCallOutcome.Success, HttpStatusCode.OK, null, null));
        }

        EngineForwarder BuildForwarder() =>
            new(_config, _engine.Object, _deadLetter.Object,
                new OptionsWrapper<QueueProcessorOptions>(new QueueProcessorOptions
                {
                    BaseUrl = "https://engine",
                    ResourceAppId = "app",
                    EngineTimeoutSeconds = 45,
                }));

        static byte[] Body(string json = "{\"amount\":10}") => Encoding.UTF8.GetBytes(json);

        static Headers WithCorrelation(string correlationId)
        {
            var headers = new Headers();
            headers["Warewolf-Custom-Transaction-Id"] = new[] { correlationId };
            return headers;
        }

        // ── Derivation ───────────────────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void BuildIdempotencyKey_CorrelationIdPresent_PrefersItOverTheBodyHash()
        {
            var key = EngineForwarder.BuildIdempotencyKey(Body(), "order-4711");

            Assert.AreEqual("cid:order-4711", key,
                "a publisher-assigned CorrelationId survives requeue AND republish, so it must win");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void BuildIdempotencyKey_NoCorrelationId_FallsBackToADeterministicBodyHash()
        {
            var first = EngineForwarder.BuildIdempotencyKey(Body(), string.Empty);
            var second = EngineForwarder.BuildIdempotencyKey(Body(), string.Empty);

            StringAssert.StartsWith(first, "sha256:",
                "the prefix tells the engine which derivation was used so it can weaken its policy");
            Assert.AreEqual(first, second, "the same bytes must always yield the same key");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void BuildIdempotencyKey_DifferentBodies_ProduceDifferentKeys()
        {
            var a = EngineForwarder.BuildIdempotencyKey(Body("{\"amount\":10}"), string.Empty);
            var b = EngineForwarder.BuildIdempotencyKey(Body("{\"amount\":11}"), string.Empty);

            Assert.AreNotEqual(a, b, "distinct payloads must never collide into one dedup entry");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void BuildIdempotencyKey_WhitespaceCorrelationId_IsTreatedAsAbsent()
        {
            var key = EngineForwarder.BuildIdempotencyKey(Body(), "   ");

            StringAssert.StartsWith(key, "sha256:",
                "a blank CorrelationId carries no identity and must not become the key 'cid:   '");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void BuildIdempotencyKey_EmptyBodyAndNoCorrelationId_DoesNotThrow()
        {
            var key = EngineForwarder.BuildIdempotencyKey(Array.Empty<byte>(), string.Empty);

            StringAssert.StartsWith(key, "sha256:");
        }

        // ── Stability across redelivery ──────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Consume_SameMessageDeliveredTwice_SendsTheSameIdempotencyKey()
        {
            var forwarder = BuildForwarder();

            // Two independent Consume calls == the redelivery path: the pump builds fresh Headers
            // for every delivery, so nothing is carried over in memory between attempts.
            await forwarder.Consume(Body(), WithCorrelation("order-4711"));
            await forwarder.Consume(Body(), WithCorrelation("order-4711"));

            Assert.AreEqual(2, _captured.Count);
            Assert.AreEqual(_captured[0]["Warewolf-Idempotency-Key"],
                            _captured[1]["Warewolf-Idempotency-Key"],
                            "a redelivery the engine cannot recognise is a duplicate execution");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Consume_SameMessageDeliveredTwice_StillSendsDifferentExecutionIds()
        {
            var forwarder = BuildForwarder();

            await forwarder.Consume(Body(), WithCorrelation("order-4711"));
            await forwarder.Consume(Body(), WithCorrelation("order-4711"));

            Assert.AreNotEqual(_captured[0]["Warewolf-Execution-Id"],
                               _captured[1]["Warewolf-Execution-Id"],
                               "Execution-Id stays per-ATTEMPT; the new key is additive, not a rename");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Consume_NoCorrelationId_IdempotencyKeyIsStillStableAcrossDeliveries()
        {
            var forwarder = BuildForwarder();

            await forwarder.Consume(Body(), new Headers());
            await forwarder.Consume(Body(), new Headers());

            Assert.AreEqual(_captured[0]["Warewolf-Idempotency-Key"],
                            _captured[1]["Warewolf-Idempotency-Key"],
                            "the hash fallback must hold the dedup contract when publishers set no id");
        }

        // ── Delivery-attempt passthrough ─────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Consume_NoDeliveryAttemptHeader_DefaultsToFirstAttempt()
        {
            await BuildForwarder().Consume(Body(), new Headers());

            Assert.AreEqual("1", _captured[0]["Warewolf-Delivery-Attempt"]);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Consume_RedeliveredMessage_ForwardsTheSecondAttemptMarkerToTheEngine()
        {
            var headers = WithCorrelation("order-4711");
            headers["Warewolf-Delivery-Attempt"] = new[] { "2" };

            await BuildForwarder().Consume(Body(), headers);

            Assert.AreEqual("2", _captured[0]["Warewolf-Delivery-Attempt"],
                "this is what lets the engine skip the dedup lookup on the common first attempt");
        }
    }
}
