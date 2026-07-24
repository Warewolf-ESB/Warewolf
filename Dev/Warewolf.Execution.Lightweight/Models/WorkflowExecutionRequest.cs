using Dev2.Web;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Principal;

namespace Warewolf.Execution.Lightweight.Models
{
    /// <summary>
    /// Request to execute a Warewolf workflow directly from a file.
    /// </summary>
    public class WorkflowExecutionRequest
    {
        /// <summary>
        /// Root directory that was used to resolve <see cref="WorkflowFilePath"/> from a name.
        /// Carried through to <see cref="LightweightEsbChannel"/> so the resource cache is
        /// always seeded from the full resource tree, not just the directory of the
        /// requested workflow file.
        /// </summary>
        public string WorkflowsDirectory { get; set; }

        /// <summary>
        /// Full path to the workflow resource XML file on disk.
        /// </summary>
        public string WorkflowFilePath { get; set; }

        /// <summary>
        /// Optional display name for the workflow (used in logging).
        /// If not provided, the file name is used.
        /// </summary>
        public string WorkflowName { get; set; }

        /// <summary>
        /// Input parameters for the workflow as key-value pairs.
        /// Keys should match the workflow DataList variable names (without [[ ]] notation).
        /// </summary>
        public Dictionary<string, string> InputParameters { get; set; } = new();

        /// <summary>
        /// Whether to execute the workflow in debug mode.
        /// </summary>
        public bool IsDebug { get; set; }

        /// <summary>
        /// The desired output format, inferred from the URL extension:
        ///   .xml  → EmitionTypes.XML   (text/xml)
        ///   .api  → EmitionTypes.OPENAPI (application/json spec)
        ///   (none / .json) → EmitionTypes.JSON (application/json, default)
        /// Mirrors the EmitionTypes used by ExecutionDtoExtensions.GetExecutePayload.
        /// </summary>
        public EmitionTypes ReturnType { get; set; } = EmitionTypes.JSON;

        /// <summary>
        /// Full request URI — required when ReturnType is OPENAPI so the
        /// generated spec can embed the correct server URL and path.
        /// </summary>
        public Uri WebServerUri { get; set; }

        /// <summary>
        /// Caller-supplied execution ID propagated from the <c>Warewolf-Execution-Id</c>
        /// request header.  When set, this value is used instead of generating a new GUID,
        /// enabling distributed tracing correlation across services.
        /// Mirrors <c>DataObjectExtensions.SetHeaders()</c> on the full server.
        /// </summary>
        public Guid? ExecutionId { get; set; }

        /// <summary>
        /// Caller-supplied custom transaction ID propagated from the
        /// <c>Warewolf-Custom-Transaction-Id</c> request header.
        /// Mirrors <c>DataObjectExtensions.SetHeaders()</c> on the full server.
        /// </summary>
        public string CustomTransactionId { get; set; }

        /// <summary>
        /// The authenticated principal of the caller, taken from the auth middleware
        /// (<c>FunctionContext.Items[AuthConstants.PrincipalContextKey]</c>) — never from
        /// the request payload. Flows into <c>DsfDataObject.ExecutingUser</c> so activities
        /// that capture the executing user (e.g. <c>SuspendExecutionActivity</c> persisting
        /// <c>currentuserprincipal</c>) work inside the engine.
        /// <see cref="JsonIgnoreAttribute"/> guards against a caller injecting a principal
        /// through the JSON request body.
        /// </summary>
        [JsonIgnore]
        public IPrincipal ExecutingPrincipal { get; set; }

        /// <summary>
        /// Validates that the request has the minimum required information.
        /// </summary>
        public bool IsValid => !string.IsNullOrWhiteSpace(WorkflowFilePath);
    }
}
