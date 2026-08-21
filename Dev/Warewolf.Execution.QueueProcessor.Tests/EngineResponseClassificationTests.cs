/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Text;
using Warewolf.Execution.QueueProcessor.Engine;

namespace Warewolf.Execution.QueueProcessor.Tests
{
    /// <summary>
    /// How a non-2xx engine response is classified, which decides whether the message is retried or
    /// discarded.
    ///
    /// <para><b>The defect these cover.</b> Every non-2xx used to become a
    /// <see cref="EngineCallOutcome.BusinessFailure"/>, which <c>EngineForwarder</c> dead-letters
    /// AND acks — permanently. That conflates two entirely different situations: "the workflow ran
    /// and rejected this message" versus "the platform never ran the workflow at all".</para>
    ///
    /// <para>Measured live on 2026-08-12: 100 messages across 10 replicas against an Azure Functions
    /// Consumption plan produced 17x502, 8x503 and 6x504. All 31 were discarded as though the
    /// messages were malformed. The database proved otherwise — 30 of them had no <c>jobs1</c> row
    /// at all, so the workflow provably never executed. They were retryable, and were thrown away.</para>
    ///
    /// <para>The status codes below are exactly those where the request is rejected at the front
    /// door and the message itself is untouched.</para>
    /// </summary>
    [TestClass]
    public class EngineResponseClassificationTests
    {
        // ── Always retryable: the workflow provably never ran ────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        [DataRow(HttpStatusCode.RequestTimeout,     408)]
        [DataRow(HttpStatusCode.TooManyRequests,    429)]
        [DataRow(HttpStatusCode.BadGateway,         502)]
        [DataRow(HttpStatusCode.ServiceUnavailable, 503)]
        [DataRow(HttpStatusCode.GatewayTimeout,     504)]
        public void PlatformRejections_AreRetryable_RegardlessOfTheOptIn(HttpStatusCode status, int code)
        {
            // Independent of the 500 opt-in: these five never depend on it.
            Assert.IsTrue(EngineWorkflowClient.IsRetryableStatus(status, retryInternalServerError: false),
                $"{code} must be retryable with the opt-in OFF - the workflow never ran");
            Assert.IsTrue(EngineWorkflowClient.IsRetryableStatus(status, retryInternalServerError: true),
                $"{code} must remain retryable with the opt-in ON");
        }

        // ── Never retryable: the request itself is unacceptable ──────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        [DataRow(HttpStatusCode.BadRequest,    400)]
        [DataRow(HttpStatusCode.Unauthorized,  401)]
        [DataRow(HttpStatusCode.Forbidden,     403)]
        [DataRow(HttpStatusCode.NotFound,      404)]
        [DataRow(HttpStatusCode.Conflict,      409)]
        [DataRow(HttpStatusCode.NotImplemented, 501)]
        public void ClientAndPermanentErrors_AreNeverRetryable(HttpStatusCode status, int code)
        {
            Assert.IsFalse(EngineWorkflowClient.IsRetryableStatus(status, retryInternalServerError: false),
                $"{code} is permanent - retrying only delays the dead-letter");
            Assert.IsFalse(EngineWorkflowClient.IsRetryableStatus(status, retryInternalServerError: true),
                $"{code} must stay permanent even with the 500 opt-in ON - the opt-in covers 500 ONLY");
        }

        // ── 500: opt-in, because the code is overloaded ──────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void InternalServerError_IsNotRetryableByDefault()
        {
            // 500 means at least three different things in this engine: a genuine workflow error, a
            // WOLF-8418 authorization denial (which surfaces as 500 rather than 403), and host
            // memory exhaustion. Only the last is worth retrying, and the status cannot tell them
            // apart - so the safe default is to dead-letter and let an operator decide.
            Assert.IsFalse(
                EngineWorkflowClient.IsRetryableStatus(HttpStatusCode.InternalServerError, retryInternalServerError: false),
                "500 must default to permanent so bad messages and auth denials are not retried");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void InternalServerError_IsRetryable_WhenExplicitlyEnabled()
        {
            // Enabled by WORKER__RETRYENGINEINTERNALERRORS, for hosts where 500 is dominated by
            // exhaustion rather than by workflow errors - e.g. the Consumption-plan OOM measured at
            // concurrency 10: "Insufficient memory to continue the execution of the program."
            Assert.IsTrue(
                EngineWorkflowClient.IsRetryableStatus(HttpStatusCode.InternalServerError, retryInternalServerError: true),
                "the opt-in must actually take effect");
        }

        // ── The option itself ────────────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void RetryEngineInternalErrors_DefaultsToDisabled()
        {
            Assert.IsFalse(new Configuration.QueueProcessorOptions().RetryEngineInternalErrors,
                "opting in to retrying 500 must be a deliberate act, never the default");
        }

        // ── End to end through the real client, over a stubbed transport ─────

        [TestMethod]
        [TestCategory("UnitTest")]
        [DataRow(503, EngineCallOutcome.TransportFailure)]
        [DataRow(502, EngineCallOutcome.TransportFailure)]
        [DataRow(504, EngineCallOutcome.TransportFailure)]
        [DataRow(400, EngineCallOutcome.BusinessFailure)]
        [DataRow(500, EngineCallOutcome.BusinessFailure)]
        [DataRow(200, EngineCallOutcome.Success)]
        public async Task PostSecureAsync_ClassifiesTheResponse(int status, EngineCallOutcome expected)
        {
            var client = NewClient((HttpStatusCode)status, "{\"body\":\"x\"}", retryInternalServerError: false);

            var result = await client.PostSecureAsync(
                "folder\\Wf", "{}", null, null, new Dictionary<string, string>(), CancellationToken.None);

            Assert.AreEqual(expected, result.Outcome, $"HTTP {status} was misclassified");
            Assert.AreEqual((HttpStatusCode)status, result.StatusCode, "the status must be preserved for diagnostics");
            Assert.IsNotNull(result.ResponseBody, "the body must be preserved so it can be dead-lettered or logged");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task PostSecureAsync_Honours500OptIn_EndToEnd()
        {
            var offClient = NewClient(HttpStatusCode.InternalServerError, "boom", retryInternalServerError: false);
            var onClient  = NewClient(HttpStatusCode.InternalServerError, "boom", retryInternalServerError: true);
            var args = new object?[] { "folder\\Wf", "{}", null, null };

            var off = await offClient.PostSecureAsync((string)args[0]!, (string)args[1]!, null, null,
                                                      new Dictionary<string, string>(), CancellationToken.None);
            var on  = await onClient.PostSecureAsync((string)args[0]!, (string)args[1]!, null, null,
                                                    new Dictionary<string, string>(), CancellationToken.None);

            Assert.AreEqual(EngineCallOutcome.BusinessFailure, off.Outcome, "default must dead-letter a 500");
            Assert.AreEqual(EngineCallOutcome.TransportFailure, on.Outcome, "opt-in must retry a 500");
        }

        static EngineWorkflowClient NewClient(HttpStatusCode status, string body, bool retryInternalServerError)
        {
            var http = new HttpClient(new StubHandler(status, body))
            {
                BaseAddress = new Uri("https://engine.example/"),
            };
            return new EngineWorkflowClient(http, retryInternalServerError);
        }

        /// <summary>Returns one canned response, so the test exercises classification and nothing else.</summary>
        sealed class StubHandler : HttpMessageHandler
        {
            readonly HttpStatusCode _status;
            readonly string _body;

            public StubHandler(HttpStatusCode status, string body) { _status = status; _body = body; }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
                => Task.FromResult(new HttpResponseMessage(_status)
                {
                    Content = new StringContent(_body, Encoding.UTF8, "application/json"),
                });
        }
    }
}
