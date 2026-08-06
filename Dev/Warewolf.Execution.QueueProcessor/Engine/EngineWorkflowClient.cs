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

        public EngineWorkflowClient(HttpClient httpClient)
            => _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

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

                Dev2Logger.Error(
                    $"Engine returned {(int)response.StatusCode} for '{relativeUrl}'. " +
                    "Note: the engine wraps authorization denials as HTTP 500 (WOLF-8418), so a 500 " +
                    "here often means this identity has no Execute permission on the workflow in " +
                    $"secure.config. Body: {Truncate(body)}", ExecutionId);

                return new EngineCallResult(
                    EngineCallOutcome.BusinessFailure, response.StatusCode, body, null);
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
