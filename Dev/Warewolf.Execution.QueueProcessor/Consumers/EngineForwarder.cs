/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using System.Text;
using Dev2.Common;
using Dev2.Common.ExtMethods;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Warewolf.Data;
using Warewolf.Execution.QueueProcessor.Configuration;
using Warewolf.Execution.QueueProcessor.Engine;
using Warewolf.Execution.QueueProcessor.Messaging;
using Warewolf.Streams;

namespace Warewolf.Execution.QueueProcessor.Consumers
{
    /// <summary>
    /// Maps a queue message to workflow inputs and invokes the engine — the container
    /// equivalent of <c>WarewolfWebRequestForwarder</c>
    /// (<c>Warewolf.Common.Framework48/WarewolfWebRequestForwarder.cs</c>).
    ///
    /// <para>Mapping is the <b>existing</b> <see cref="MessageToInputsMapper"/>, referenced
    /// unchanged, so JSON / XML / whole-message behaviour cannot drift. The forwarder itself is
    /// reimplemented rather than referenced because its project's closure includes
    /// <c>Dev2.Studio.Core</c>, and because the auth model differs (managed identity, not
    /// <c>NetworkCredential</c>).</para>
    ///
    /// <para><b>Failure contract (parity — plan §1.2/§2.6):</b>
    /// engine 2xx → <see cref="ConsumerResult.Success"/> (ack);
    /// engine non-2xx → dead-letter the mapped body, then STILL return
    /// <see cref="ConsumerResult.Success"/> so the original is acked and not redelivered — this
    /// is what <c>LoggingConsumerWrapper.cs:64-79</c> does today;
    /// transport/token failure → <see cref="ConsumerResult.Failed"/> (no ack → redelivery).</para>
    /// </summary>
    public sealed class EngineForwarder : IConsumer
    {
        const string ExecutionId = "QueueProcessor-Forwarder";

        readonly ResolvedQueueConfiguration _config;
        readonly IEngineWorkflowClient _engine;
        readonly IDeadLetterPublisher _deadLetter;
        readonly QueueProcessorOptions _options;
        readonly MessageToInputsMapper _mapper = new();

        public EngineForwarder(
            ResolvedQueueConfiguration config,
            IEngineWorkflowClient engine,
            IDeadLetterPublisher deadLetter,
            IOptions<QueueProcessorOptions> options)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _engine = engine ?? throw new ArgumentNullException(nameof(engine));
            _deadLetter = deadLetter ?? throw new ArgumentNullException(nameof(deadLetter));
            _options = (options ?? throw new ArgumentNullException(nameof(options))).Value;
        }

        public async Task<ConsumerResult> Consume(byte[] body, object parameters)
        {
            var headers = parameters as Headers ?? new Headers();

            // Same correlation contract as LoggingConsumerWrapper.cs:42-49.
            if (!headers.KeyExists("Warewolf-Execution-Id"))
            {
                headers["Warewolf-Execution-Id"] = new[] { Guid.NewGuid().ToString() };
            }

            var executionId = headers["Warewolf-Execution-Id", Array.Empty<string>()]?.FirstOrDefault()
                              ?? Guid.NewGuid().ToString();
            var customTransactionId =
                headers["Warewolf-Custom-Transaction-Id", Array.Empty<string>()]?.FirstOrDefault()
                ?? string.Empty;

            var postBody = BuildPostBody(body, out var formFieldName);

            using var cts = new CancellationTokenSource(
                TimeSpan.FromSeconds(_options.EngineTimeoutSeconds));

            var outboundHeaders = new Dictionary<string, string>
            {
                ["Warewolf-Execution-Id"] = executionId,
                ["Warewolf-Custom-Transaction-Id"] = customTransactionId,
            };

            var result = await _engine.PostSecureAsync(
                _config.WorkflowPath,
                postBody,
                formFieldName is null ? null : body,
                formFieldName,
                outboundHeaders,
                cts.Token).ConfigureAwait(false);

            switch (result.Outcome)
            {
                case EngineCallOutcome.Success:
                    return ConsumerResult.Success;

                case EngineCallOutcome.BusinessFailure:
                    await DeadLetterAsync(postBody, executionId, customTransactionId, result)
                        .ConfigureAwait(false);
                    // Deliberately Success: parity with on-prem, where a business failure is
                    // dead-lettered AND acked so it is never redelivered.
                    return ConsumerResult.Success;

                default:
                    return ConsumerResult.Failed;
            }
        }

        async Task DeadLetterAsync(
            string postBody, string executionId, string customTransactionId, EngineCallResult result)
        {
            try
            {
                var diagnostics = new Dictionary<string, object?>
                {
                    ["x-warewolf-execution-id"] = executionId,
                    ["x-warewolf-custom-transaction-id"] = customTransactionId,
                    ["x-warewolf-queue"] = _config.QueueName,
                    ["x-warewolf-workflow"] = _config.WorkflowPath,
                    ["x-warewolf-engine-status"] = result.StatusCode.HasValue
                        ? ((int)result.StatusCode.Value).ToString()
                        : "none",
                    ["x-warewolf-dead-lettered-utc"] = DateTime.UtcNow.ToString("O"),
                };

                await _deadLetter
                    .PublishAsync(Encoding.UTF8.GetBytes(postBody), diagnostics, CancellationToken.None)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // If dead-lettering itself fails we must NOT ack, or the message is lost: escalate
                // by throwing so the pump treats it as a failure and leaves it unacked.
                Dev2Logger.Fatal(
                    $"Dead-letter publish failed for execution {executionId}; refusing to ack so the " +
                    "message is redelivered rather than lost.", ex, ExecutionId);
                throw;
            }
        }

        /// <summary>
        /// Builds the request body with the existing mapper, preserving
        /// <c>WarewolfWebRequestForwarder.BuildPostBody</c> semantics
        /// (<c>WarewolfWebRequestForwarder.cs:85-92</c>).
        /// </summary>
        /// <param name="formFieldName">
        /// Set when the single input is <c>@</c>-prefixed and <c>MapEntireMessage</c> is on, in
        /// which case the raw bytes are posted as <c>multipart/form-data</c> under this name.
        /// </param>
        internal string BuildPostBody(byte[] body, out string? formFieldName)
        {
            formFieldName = null;

            var message = Encoding.UTF8.GetString(body);
            var inputs = (_config.Trigger.Inputs ?? new List<TriggerInput>())
                .Select(i => (variableName: i.Name ?? string.Empty, messageValue: i.Value ?? string.Empty))
                .ToList();

            var first = _config.Trigger.Inputs?.FirstOrDefault();
            if (_config.Trigger.MapEntireMessage
                && first?.Name is { Length: > 0 } name
                && name.StartsWith('@'))
            {
                formFieldName = name;
            }

            var mapped = _mapper.Map(
                message,
                inputs,
                message.IsJSON(),
                IsXml(message),
                _config.Trigger.MapEntireMessage);

            return WrapForEngine(mapped);
        }

        /// <summary>
        /// Nests the mapped inputs under <c>inputParameters</c>, which is the JSON body contract the
        /// Lightweight engine actually binds (WorkflowHttpFunction.cs:77):
        /// <code>{ "inputParameters": { "Name": "John", "Age": "30" } }</code>
        /// </summary>
        /// <remarks>
        /// MessageToInputsMapper returns a FLAT object - {"message":"..."} - and the on-prem
        /// WarewolfWebRequestForwarder posts exactly that (line 110). Dev2.Server accepts it; the
        /// Lightweight engine does NOT, and the failure is quiet and total: the workflow runs but
        /// every input is unbound, so it fails on its first variable and the worker dead-letters
        /// (and acks) every single message. Verified live against the deployed engine:
        ///   POST {"message":"x"}                       -> 500 "Scalar value { message } is NULL"
        ///   POST {"inputParameters":{"message":"x"}}   -> 200 {"output":"x"}
        /// The wrapping belongs here rather than in the shared mapper, which the on-prem forwarder
        /// also uses, and rather than loosening the engine, where treating any top-level property as
        /// an input would collide with reserved keys.
        /// </remarks>
        internal static string WrapForEngine(string mappedJson)
        {
            // Re-parse rather than string-concatenate so malformed mapper output fails here instead
            // of producing a body the engine silently ignores.
            var inner = string.IsNullOrWhiteSpace(mappedJson)
                ? new JObject()
                : JObject.Parse(mappedJson);

            return new JObject { ["inputParameters"] = inner }.ToString(Formatting.None);
        }

        /// <summary>
        /// Cheap XML probe. The on-prem forwarder used <c>DataListUtilBase.IsXml</c>; that type
        /// lives behind a heavier closure, and for the mapper's purposes the leading-angle-bracket
        /// test is equivalent (the mapper only needs to choose a parse strategy, and
        /// <c>XDocument.Parse</c> inside it enforces real well-formedness).
        /// </summary>
        static bool IsXml(string message)
        {
            var trimmed = message?.TrimStart();
            return !string.IsNullOrEmpty(trimmed) && trimmed!.StartsWith('<');
        }
    }
}
