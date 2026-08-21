/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using System.Net;
using System.Text;
using Dev2.Common;

namespace Warewolf.Execution.QueueProcessor.Engine
{
    /// <summary>Outcome classes the worker must distinguish — see plan §2.6.</summary>
    public enum EngineCallOutcome
    {
        /// <summary>2xx — ack the message.</summary>
        Success,

        /// <summary>
        /// Non-2xx from the engine, including a WOLF-8418 authorization denial (which surfaces
        /// as HTTP 500, not 403). Permanent: dead-letter the body, then ack.
        /// </summary>
        BusinessFailure,

        /// <summary>
        /// Unreachable, TLS/DNS failure, token failure, or timeout. Transient: do NOT ack, let
        /// the broker redeliver.
        /// </summary>
        TransportFailure,
    }

    public sealed record EngineCallResult(
        EngineCallOutcome Outcome,
        HttpStatusCode? StatusCode,
        string? ResponseBody,
        Exception? Exception);

    public interface IEngineWorkflowClient
    {
        Task<EngineCallResult> PostSecureAsync(
            string workflowPath,
            string jsonBody,
            byte[]? rawBody,
            string? formFieldName,
            IReadOnlyDictionary<string, string> headers,
            CancellationToken cancellationToken);
    }

    /// <summary>
    /// Typed client over the engine's <c>/Secure/{*name}</c> route, which accepts GET and POST
    /// (<c>WorkflowHttpFunction.cs:136-141</c>). Auth is handled entirely by
    /// <see cref="WwExecutionTokenHandler"/> in the pipeline, so this class only builds routes
    /// and classifies responses.
    /// </summary>
    public sealed class EngineWorkflowClient : IEngineWorkflowClient
    {
        const string ExecutionId = "QueueProcessor-Engine";

        readonly HttpClient _httpClient;
        readonly bool _retryEngineInternalErrors;

        public EngineWorkflowClient(HttpClient httpClient, bool retryEngineInternalErrors = false)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _retryEngineInternalErrors = retryEngineInternalErrors;
        }

        /// <summary>
        /// Decides whether a non-2xx response means "this message is unacceptable" (dead-letter) or
        /// "the platform could not serve it, try again" (requeue).
        ///
        /// <para>Every status listed here shares one property: <b>the workflow provably never
        /// ran</b>. The request was rejected at the front door, so the message itself is untouched
        /// and a retry is meaningful.</para>
        /// <list type="bullet">
        ///   <item><b>408</b> Request Timeout — the server gave up reading the request;</item>
        ///   <item><b>429</b> Too Many Requests — explicit throttling, retry is the intended response;</item>
        ///   <item><b>502</b> Bad Gateway — the Functions host was unavailable;</item>
        ///   <item><b>503</b> Service Unavailable — the app was shedding load while scaling out;</item>
        ///   <item><b>504</b> Gateway Timeout — the front end gave up waiting for the host.</item>
        /// </list>
        ///
        /// <para>WHY THIS EXISTS: every non-2xx used to be a BusinessFailure, which the forwarder
        /// dead-letters AND acks — permanently. Measured 2026-08-12 on a Consumption plan, a burst of
        /// 100 messages across 10 replicas produced 17x502, 8x503 and 6x504, and all 31 were
        /// discarded as though the messages were malformed. They were not: 30 of them never reached
        /// the workflow at all, which the database proved by having no row for them.</para>
        ///
        /// <para><b>500 is deliberately excluded unless opted in.</b> It is overloaded — a genuine
        /// workflow error, a WOLF-8418 authorization denial, and host memory exhaustion all surface
        /// as 500, and only the last is worth retrying. See
        /// <see cref="Configuration.QueueProcessorOptions.RetryEngineInternalErrors"/>.</para>
        /// </summary>
        internal static bool IsRetryableStatus(HttpStatusCode status, bool retryInternalServerError)
            => status switch
            {
                HttpStatusCode.RequestTimeout      => true,   // 408
                HttpStatusCode.TooManyRequests     => true,   // 429
                HttpStatusCode.BadGateway          => true,   // 502
                HttpStatusCode.ServiceUnavailable  => true,   // 503
                HttpStatusCode.GatewayTimeout      => true,   // 504
                HttpStatusCode.InternalServerError => retryInternalServerError,  // 500 - opt-in only
                _                                  => false,
            };

        public async Task<EngineCallResult> PostSecureAsync(
            string workflowPath,
            string jsonBody,
            byte[]? rawBody,
            string? formFieldName,
            IReadOnlyDictionary<string, string> headers,
            CancellationToken cancellationToken)
        {
            var relativeUrl = BuildRelativeUrl("secure", workflowPath);

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, relativeUrl);

                // Parity with WarewolfWebRequestForwarder.SendEventToWarewolf
                // (WarewolfWebRequestForwarder.cs:100-108): a single '@'-prefixed input with
                // MapEntireMessage is sent as multipart/form-data; everything else is a JSON body.
                if (rawBody is not null && !string.IsNullOrEmpty(formFieldName))
                {
                    var multipart = new MultipartFormDataContent();
                    multipart.Add(new ByteArrayContent(rawBody), formFieldName!);
                    request.Content = multipart;
                }
                else
                {
                    request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                }

                foreach (var header in headers)
                {
                    if (!string.IsNullOrEmpty(header.Value))
                    {
                        request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }

                using var response = await _httpClient.SendAsync(request, cancellationToken)
                                                      .ConfigureAwait(false);
                var body = await response.Content.ReadAsStringAsync(cancellationToken)
                                                 .ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    return new EngineCallResult(EngineCallOutcome.Success, response.StatusCode, body, null);
                }

                var retryable = IsRetryableStatus(response.StatusCode, _retryEngineInternalErrors);

                Dev2Logger.Error(
                    $"Engine returned {(int)response.StatusCode} for '{relativeUrl}' " +
                    $"(classified {(retryable ? "TRANSPORT - will be retried" : "BUSINESS - will be dead-lettered")}). " +
                    "Note: the engine wraps authorization denials as HTTP 500 (WOLF-8418), so a 500 " +
                    "here often means this identity has no Execute permission on the workflow in " +
                    $"secure.config. Body: {Truncate(body)}", ExecutionId);

                return new EngineCallResult(
                    retryable ? EngineCallOutcome.TransportFailure : EngineCallOutcome.BusinessFailure,
                    response.StatusCode, body, null);
            }
            catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
            {
                // HttpClient.Timeout elapsed - transient by classification, so the message is
                // redelivered rather than dead-lettered.
                Dev2Logger.Error(
                    $"Engine call to '{relativeUrl}' timed out after {_httpClient.Timeout.TotalSeconds:F0}s.",
                    ex, ExecutionId);
                return new EngineCallResult(EngineCallOutcome.TransportFailure, null, null, ex);
            }
            catch (Exception ex)
            {
                Dev2Logger.Error($"Engine call to '{relativeUrl}' failed.", ex, ExecutionId);
                return new EngineCallResult(EngineCallOutcome.TransportFailure, null, null, ex);
            }
        }

        /// <summary>
        /// Escapes each '/'-separated segment independently so folder-qualified workflow names
        /// survive (e.g. <c>ProfilerWrapper/Queue/MandateCollectionSuccessConsume</c> →
        /// <c>secure/ProfilerWrapper/Queue/MandateCollectionSuccessConsume.json</c>) while
        /// spaces become <c>%20</c>. Backslash separators are normalised upstream in
        /// <c>ResolvedQueueConfiguration.WorkflowPath</c>.
        /// </summary>
        internal static string BuildRelativeUrl(string area, string workflowPath)
        {
            var encoded = workflowPath
                .Split('/', StringSplitOptions.RemoveEmptyEntries)
                .Select(Uri.EscapeDataString);

            return $"{area}/{string.Join('/', encoded)}.json";
        }

        static string Truncate(string? value, int max = 512)
            => string.IsNullOrEmpty(value) ? string.Empty
               : value.Length <= max ? value : value[..max] + "...";
    }
}
