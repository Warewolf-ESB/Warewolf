using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Warewolf.Execution.Lightweight.Models;

namespace Warewolf.Execution.Lightweight
{
    /// <summary>
    /// Builds <see cref="HttpResponseData"/> instances from <see cref="WorkflowExecutionResult"/>
    /// or plain string payloads, centralising all Content-Type and error-envelope logic.
    ///
    /// Extracted from <c>WorkflowHttpFunction</c> so that every Function endpoint
    /// remains a thin, testable wrapper with no direct HTTP plumbing.
    /// </summary>
    internal static class ResponseBuilder
    {
        internal const string JsonContentType = "application/json";
        internal const string XmlContentType  = "text/xml";

        /// <summary>
        /// Creates an <see cref="HttpResponseData"/> from a <see cref="WorkflowExecutionResult"/>.
        /// <list type="bullet">
        ///   <item>Uses <see cref="WorkflowExecutionResult.PayloadWriter"/> when present — streams
        ///         the payload directly to the response body without a full <c>byte[]</c> allocation.</item>
        ///   <item>Falls back to <see cref="WorkflowExecutionResult.Payload"/> for small payloads.</item>
        ///   <item>Writes a structured error body (XML or JSON) when only
        ///         <see cref="WorkflowExecutionResult.Errors"/> is populated.</item>
        /// </list>
        /// </summary>
        internal static async Task<HttpResponseData> BuildAsync(
            HttpRequestData req,
            WorkflowExecutionResult result,
            string requestedContentType = JsonContentType)
        {
            var statusCode  = result.IsSuccess ? HttpStatusCode.OK
                                               : HttpStatusCode.InternalServerError;
            var response    = req.CreateResponse(statusCode);
            var contentType = result.ContentType ?? requestedContentType;
            response.Headers.Add("Content-Type", contentType);

            if (result.PayloadWriter != null)
            {
                await result.PayloadWriter(response.Body, CancellationToken.None);
            }
            else if (!string.IsNullOrEmpty(result.Payload))
            {
                await response.WriteStringAsync(result.Payload);
            }
            else if (result.Errors.Count > 0)
            {
                // Pure failure (file not found, invalid XAML, etc.) — write a structured
                // error body so the caller sees something meaningful rather than 500 + empty.
                response.Headers.Remove("Content-Type");
                if (requestedContentType == XmlContentType)
                {
                    response.Headers.Add("Content-Type", XmlContentType);
                    var sb = new StringBuilder("<DataList><Errors>");
                    foreach (var err in result.Errors)
                        sb.Append($"<Error><![CDATA[{err}]]></Error>");
                    sb.Append("</Errors></DataList>");
                    await response.WriteStringAsync(sb.ToString());
                }
                else
                {
                    response.Headers.Add("Content-Type", JsonContentType);
                    await response.WriteStringAsync(JsonConvert.SerializeObject(new
                    {
                        hasErrors = true,
                        errors    = result.Errors
                    }, Formatting.Indented));
                }
            }

            return response;
        }

        /// <summary>
        /// Creates an <see cref="HttpResponseData"/> with a plain string body.
        /// Used for apis.json, OpenAPI specs, and validation error responses.
        /// </summary>
        internal static async Task<HttpResponseData> BuildStringAsync(
            HttpRequestData req,
            string content,
            string contentType = JsonContentType,
            HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            var response = req.CreateResponse(statusCode);
            response.Headers.Add("Content-Type", contentType);
            await response.WriteStringAsync(content);
            return response;
        }
    }
}
