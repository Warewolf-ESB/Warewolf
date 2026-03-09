using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Warewolf.Execution.AzureFunction.Lightweight.Models
{
    /// <summary>
    /// Result of a Warewolf workflow execution.
    /// </summary>
    public class WorkflowExecutionResult
    {
        /// <summary>
        /// Ready-to-return response body in the format requested (JSON, XML, or OpenAPI spec).
        /// Populated by WorkflowExecutor.ExtractPayload — mirrors ExecutionDtoExtensions.GetExecutePayload:
        ///   XML     → ExecutionEnvironmentUtils.GetXmlOutputFromEnvironment  (DataList-shaped XML)
        ///   JSON    → ExecutionEnvironmentUtils.GetJsonOutputFromEnvironment  (DataList-shaped JSON)
        ///   OPENAPI → WorkflowOpenApiGenerator.Generate                       (OpenAPI 3.0 JSON spec)
        /// </summary>
        public string Payload { get; set; }

        /// <summary>
        /// MIME content type that matches Payload:
        ///   "application/json" for JSON and OPENAPI,
        ///   "text/xml"         for XML.
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Writes the response payload directly to a <see cref="Stream"/>, encoding chars
        /// in chunks via <see cref="StreamWriter"/> instead of materialising a full
        /// <c>byte[]</c> (as <c>HttpResponseData.WriteStringAsync</c> does).
        /// Set by <c>WorkflowExecutor</c> for every successful execution; <c>null</c> for
        /// failure results where <see cref="Payload"/> holds a small error body.
        /// </summary>
        public Func<Stream, CancellationToken, Task> PayloadWriter { get; internal set; }

        /// <summary>
        /// Whether the workflow executed successfully without errors.
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// The execution ID assigned to this workflow run.
        /// </summary>
        public Guid ExecutionId { get; set; }

        /// <summary>
        /// Output data from the workflow environment as key-value pairs.
        /// </summary>
        public Dictionary<string, object> Outputs { get; set; } = new();

        /// <summary>
        /// Errors that occurred during execution.
        /// </summary>
        public List<string> Errors { get; set; } = new();

        /// <summary>
        /// Per-activity debug states captured when the request used the .debug extension.
        /// Each entry mirrors the <c>IDebugState</c> the full Warewolf server sends to the Studio,
        /// containing the activity name, inputs, outputs, timing, and error information.
        /// Populated only when <c>IsDebug = true</c>; empty list otherwise.
        /// </summary>
        public List<DebugStepResult> DebugStates { get; set; } = new();

        /// <summary>
        /// Duration of the workflow execution.
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// Time the workflow execution started.
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Time the workflow execution completed.
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Materialises the payload to a string — intended for tests and diagnostics only.
        /// For HTTP responses prefer <see cref="PayloadWriter"/> which streams directly to
        /// the response body without allocating a full <c>byte[]</c>.
        /// </summary>
        public async Task<string> ReadPayloadAsync(CancellationToken ct = default)
        {
            if (PayloadWriter == null)
                return Payload ?? string.Empty;
            using var ms = new MemoryStream();
            await PayloadWriter(ms, ct);
            ms.Position = 0;
            return new StreamReader(ms, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true).ReadToEnd();
        }

        /// <summary>
        /// Creates a failure result with the specified error message.
        /// </summary>
        public static WorkflowExecutionResult Failure(string errorMessage) => new()
        {
            IsSuccess = false,
            Errors = new List<string> { errorMessage },
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow
        };
    }
}
