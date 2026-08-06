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
    /// The failure contract — the parity item that matters most in this migration.
    ///
    /// On-prem, a business failure (engine non-2xx) is dead-lettered by
    /// <c>WarewolfWebRequestForwarder</c> and then reported as <b>Success</b> by
    /// <c>LoggingConsumerWrapper</c> (<c>LoggingConsumerWrapper.cs:64-79</c>), so the original
    /// message is <b>acked</b> and never redelivered. Only an exception leaves a message unacked.
    /// These tests pin that behaviour on the container implementation, because getting it wrong
    /// silently changes redelivery for every existing trigger.
    /// </summary>
    [TestClass]
    public class EngineForwarderTests
    {
        // Staged layout: trigger definitions in Settings\triggers\, sources in Settings\sources\.
        static string SettingsDir => Path.Combine(AppContext.BaseDirectory, "TestResources");

        static string ResourcePath(string name) => Path.Combine(SettingsDir, "sources", name);
        static string TriggerPath(string name) => Path.Combine(SettingsDir, "triggers", name);

        Mock<IEngineWorkflowClient> _engine = null!;
        Mock<IDeadLetterPublisher> _deadLetter = null!;
        ResolvedQueueConfiguration _config = null!;

        [TestInitialize]
        public void Setup()
        {
            _engine = new Mock<IEngineWorkflowClient>();
            _deadLetter = new Mock<IDeadLetterPublisher>();

            var trigger = new TriggerBiteReader().Read(TriggerPath("triggers-mandate.bite"));
            var source = RabbitMqSourceOptions.FromBiteFile(
                ResourcePath("0b142714-8f6d-41b7-9832-2aefa8c731ec.bite"));

            _config = new ResolvedQueueConfiguration
            {
                Trigger = trigger, Source = source, DeadLetterSource = source,
            };
        }

        EngineForwarder BuildForwarder(int timeoutSeconds = 45) =>
            new(_config, _engine.Object, _deadLetter.Object,
                new OptionsWrapper<QueueProcessorOptions>(new QueueProcessorOptions
                {
                    BaseUrl = "https://engine",
                    ResourceAppId = "app",
                    EngineTimeoutSeconds = timeoutSeconds,
                }));

        void SetupEngine(EngineCallOutcome outcome, HttpStatusCode? status = null) =>
            _engine.Setup(e => e.PostSecureAsync(
                       It.IsAny<string>(), It.IsAny<string>(), It.IsAny<byte[]?>(),
                       It.IsAny<string?>(), It.IsAny<IReadOnlyDictionary<string, string>>(),
                       It.IsAny<CancellationToken>()))
                   .ReturnsAsync(new EngineCallResult(outcome, status, null, null));

        static byte[] Body(string json = "{\"amount\":10}") => Encoding.UTF8.GetBytes(json);

        // ── Outcome mapping ──────────────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Consume_Engine2xx_ReturnsSuccessAndDoesNotDeadLetter()
        {
            SetupEngine(EngineCallOutcome.Success, HttpStatusCode.OK);

            var result = await BuildForwarder().Consume(Body(), new Headers());

            Assert.AreEqual(ConsumerResult.Success, result);
            _deadLetter.Verify(d => d.PublishAsync(
                It.IsAny<byte[]>(), It.IsAny<IReadOnlyDictionary<string, object?>>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Consume_EngineNon2xx_DeadLettersAndStillReturnsSuccessSoTheOriginalIsAcked()
        {
            SetupEngine(EngineCallOutcome.BusinessFailure, HttpStatusCode.BadRequest);

            var result = await BuildForwarder().Consume(Body(), new Headers());

            _deadLetter.Verify(d => d.PublishAsync(
                It.IsAny<byte[]>(), It.IsAny<IReadOnlyDictionary<string, object?>>(),
                It.IsAny<CancellationToken>()), Times.Once);

            Assert.AreEqual(ConsumerResult.Success, result,
                "on-prem parity: a business failure is dead-lettered AND acked, never redelivered");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Consume_Wolf8418DenialAs500_IsTreatedAsPermanentAndDeadLettered()
        {
            // The engine wraps authorization denials as HTTP 500, not 403 (WOLF-8418). A denial
            // will not fix itself, so redelivering forever is wrong - dead-letter it.
            SetupEngine(EngineCallOutcome.BusinessFailure, HttpStatusCode.InternalServerError);

            var result = await BuildForwarder().Consume(Body(), new Headers());

            Assert.AreEqual(ConsumerResult.Success, result);
            _deadLetter.Verify(d => d.PublishAsync(
                It.IsAny<byte[]>(), It.IsAny<IReadOnlyDictionary<string, object?>>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Consume_TransportFailure_ReturnsFailedSoTheBrokerRedelivers()
        {
            SetupEngine(EngineCallOutcome.TransportFailure);

            var result = await BuildForwarder().Consume(Body(), new Headers());

            Assert.AreEqual(ConsumerResult.Failed, result);
            _deadLetter.Verify(d => d.PublishAsync(
                It.IsAny<byte[]>(), It.IsAny<IReadOnlyDictionary<string, object?>>(),
                It.IsAny<CancellationToken>()), Times.Never,
                "a transient failure must not consume the dead-letter queue");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Consume_DeadLetterPublishFails_ThrowsRatherThanAckingAndLosingTheMessage()
        {
            SetupEngine(EngineCallOutcome.BusinessFailure, HttpStatusCode.BadRequest);
            _deadLetter.Setup(d => d.PublishAsync(
                          It.IsAny<byte[]>(), It.IsAny<IReadOnlyDictionary<string, object?>>(),
                          It.IsAny<CancellationToken>()))
                       .ThrowsAsync(new InvalidOperationException("broker down"));

            // Acking here would destroy the only copy of the message.
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => BuildForwarder().Consume(Body(), new Headers()));
        }

        // ── Correlation ──────────────────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Consume_GeneratesExecutionIdWhenAbsentAndForwardsBothCorrelationHeaders()
        {
            IReadOnlyDictionary<string, string>? captured = null;
            _engine.Setup(e => e.PostSecureAsync(
                       It.IsAny<string>(), It.IsAny<string>(), It.IsAny<byte[]?>(),
                       It.IsAny<string?>(), It.IsAny<IReadOnlyDictionary<string, string>>(),
                       It.IsAny<CancellationToken>()))
                   .Callback<string, string, byte[]?, string?, IReadOnlyDictionary<string, string>, CancellationToken>(
                       (_, _, _, _, h, _) => captured = h)
                   .ReturnsAsync(new EngineCallResult(EngineCallOutcome.Success, HttpStatusCode.OK, null, null));

            var headers = new Headers();
            headers["Warewolf-Custom-Transaction-Id"] = new[] { "txn-9" };

            await BuildForwarder().Consume(Body(), headers);

            Assert.IsNotNull(captured);
            Assert.IsTrue(captured!.ContainsKey("Warewolf-Execution-Id"));
            Assert.IsFalse(string.IsNullOrWhiteSpace(captured["Warewolf-Execution-Id"]));
            Assert.AreEqual("txn-9", captured["Warewolf-Custom-Transaction-Id"]);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task Consume_PreservesAnExistingExecutionId()
        {
            IReadOnlyDictionary<string, string>? captured = null;
            _engine.Setup(e => e.PostSecureAsync(
                       It.IsAny<string>(), It.IsAny<string>(), It.IsAny<byte[]?>(),
                       It.IsAny<string?>(), It.IsAny<IReadOnlyDictionary<string, string>>(),
                       It.IsAny<CancellationToken>()))
                   .Callback<string, string, byte[]?, string?, IReadOnlyDictionary<string, string>, CancellationToken>(
                       (_, _, _, _, h, _) => captured = h)
                   .ReturnsAsync(new EngineCallResult(EngineCallOutcome.Success, HttpStatusCode.OK, null, null));

            var headers = new Headers();
            headers["Warewolf-Execution-Id"] = new[] { "11111111-1111-1111-1111-111111111111" };

            await BuildForwarder().Consume(Body(), headers);

            Assert.AreEqual("11111111-1111-1111-1111-111111111111", captured!["Warewolf-Execution-Id"],
                "a redelivery must carry the same execution id so the engine can dedupe");
        }

        // ── Mapping (MessageToInputsMapper reuse) ────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void BuildPostBody_MapEntireMessageWithNonAtInput_ProducesJsonBodyNotMultipart()
        {
            // The real trigger has MapEntireMessage=true and a single input 'PayloadRequest'
            // (no '@' prefix), so the multipart path must NOT engage.
            var forwarder = BuildForwarder();
            const string message = "{\"amount\":10}";

            var body = forwarder.BuildPostBody(Encoding.UTF8.GetBytes(message), out var formField);

            Assert.IsNull(formField, "multipart requires an '@'-prefixed input name");

            // MapEntireMessage puts the WHOLE raw message in as the input's value, and the inputs are
            // nested under 'inputParameters' - the JSON body contract the Lightweight engine binds
            // (WorkflowHttpFunction.cs:77). Parse rather than substring-match, so the assertion
            // survives escaping.
            var parsed = Newtonsoft.Json.Linq.JObject.Parse(body);
            Assert.IsNotNull(parsed["inputParameters"],
                "a FLAT body is silently ignored by the engine - every message would dead-letter");
            Assert.AreEqual(message, (string?)parsed["inputParameters"]!["PayloadRequest"],
                "the engine must receive the untouched message as the mapped input value");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void WrapForEngine_NestsInputsUnderInputParameters()
        {
            // Verified live against the deployed engine before writing this:
            //   POST {"message":"x"}                     -> 500 "Scalar value { message } is NULL"
            //   POST {"inputParameters":{"message":"x"}} -> 200 {"output":"x"}
            // The on-prem WarewolfWebRequestForwarder posts the FLAT form and Dev2.Server accepts it,
            // so this is an on-prem -> Lightweight contract gap, not a mapper bug.
            var wrapped = EngineForwarder.WrapForEngine("{\"message\":\"x\"}");

            var parsed = Newtonsoft.Json.Linq.JObject.Parse(wrapped);
            Assert.AreEqual("x", (string?)parsed["inputParameters"]!["message"]);
            Assert.IsNull(parsed["message"], "the flat key must not remain at the top level");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void WrapForEngine_EmptyMapperOutput_StillProducesTheEnvelope()
        {
            // A trigger with no inputs maps to nothing; the engine must still get a well-formed body
            // rather than an empty string it would reject outright.
            foreach (var empty in new[] { "", "   ", "{}" })
            {
                var parsed = Newtonsoft.Json.Linq.JObject.Parse(EngineForwarder.WrapForEngine(empty));
                Assert.IsNotNull(parsed["inputParameters"], $"input '{empty}' must still be wrapped");
            }
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void BuildPostBody_MapEntireMessageWithAtPrefixedInput_SelectsMultipartField()
        {
            _config.Trigger.Inputs = new List<TriggerInput> { new() { Name = "@object", Value = "" } };
            var forwarder = BuildForwarder();

            var body = forwarder.BuildPostBody(Encoding.UTF8.GetBytes("<x/>"), out var formField);

            Assert.AreEqual("@object", formField,
                "parity with WarewolfWebRequestForwarder.cs:100-108");
            Assert.IsNotNull(body);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void BuildPostBody_XmlMessage_IsDetectedAsXml()
        {
            _config.Trigger.MapEntireMessage = false;
            _config.Trigger.Inputs = new List<TriggerInput>
            {
                new() { Name = "amount", Value = "amount" },
            };
            var forwarder = BuildForwarder();

            var body = forwarder.BuildPostBody(
                Encoding.UTF8.GetBytes("<root><amount>10</amount></root>"), out _);

            var parsed = Newtonsoft.Json.Linq.JObject.Parse(body);
            Assert.AreEqual("10", (string?)parsed["inputParameters"]!["amount"],
                "XML is mapped per-input and must still arrive inside the engine's envelope");
        }

        // ── Engine route building ────────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void BuildRelativeUrl_FolderQualifiedWorkflow_EscapesPerSegmentAndKeepsSlashes()
        {
            var url = EngineWorkflowClient.BuildRelativeUrl(
                "secure", "ProfilerWrapper/Queue/MandateCollectionSuccessConsume");

            Assert.AreEqual("secure/ProfilerWrapper/Queue/MandateCollectionSuccessConsume.json", url);
            Assert.IsFalse(url.Contains("%5C"), "backslashes must never reach the route");
            Assert.IsFalse(url.Contains("%2F"), "'/' separators must be preserved, not encoded");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void BuildRelativeUrl_WorkflowNameWithSpaces_EncodesThem()
        {
            var url = EngineWorkflowClient.BuildRelativeUrl("secure", "Hello World");

            Assert.AreEqual("secure/Hello%20World.json", url);
        }
    }
}
