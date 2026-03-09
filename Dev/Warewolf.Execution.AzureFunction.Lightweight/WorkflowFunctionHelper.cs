using Dev2.Web;
using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Warewolf.Execution.AzureFunction.Lightweight.Models;

namespace Warewolf.Execution.AzureFunction.Lightweight
{
    /// <summary>
    /// Helpers for creating <see cref="WorkflowExecutionRequest"/> instances
    /// from Azure Function HTTP triggers, explicit parameters, or JSON.
    ///
    /// Supports extracting workflow file path and input parameters from:
    ///   - Route parameters (e.g., /api/workflow/{workflowName})
    ///   - Query string parameters (e.g., ?workflowFilePath=...&amp;param1=value1)
    ///   - JSON request body
    ///   - A combination (body takes precedence for inputs)
    /// </summary>
    public static class WorkflowFunctionHelper
    {
        // Created once; reused across all requests to avoid per-request HashSet allocations.
        static readonly HashSet<string> _reservedQueryKeys = new(StringComparer.OrdinalIgnoreCase)
        {
            "workflowName", "workflowFilePath", "isDebug"
        };
        /// <summary>
        /// Creates a <see cref="WorkflowExecutionRequest"/> from an HTTP request.
        /// </summary>
        /// <param name="request">The Azure Function HTTP request.</param>
        /// <param name="workflowsDirectory">
        /// Base directory where workflow files are stored.
        /// When a workflowName is provided (rather than a full path),
        /// the file path is resolved as {workflowsDirectory}/{workflowName}.xml
        /// </param>
        /// <param name="workflowNameFromRoute">Optional workflow name from a route parameter.</param>
        public static async Task<WorkflowExecutionRequest> ParseRequestAsync(
            HttpRequestData request,
            string workflowsDirectory,
            string workflowNameFromRoute = null)
        {
            var executionRequest = new WorkflowExecutionRequest();

            ParseQueryString(request, executionRequest);
            await ParseBodyAsync(request, executionRequest);

            if (!string.IsNullOrWhiteSpace(workflowNameFromRoute))
            {
                executionRequest.WorkflowName = workflowNameFromRoute;
            }

            ResolveFilePath(executionRequest, workflowsDirectory);

            if (!string.IsNullOrWhiteSpace(workflowsDirectory))
                executionRequest.WorkflowsDirectory = workflowsDirectory;

            return executionRequest;
        }

        /// <summary>
        /// Creates a request from explicit parameters.
        /// </summary>
        public static WorkflowExecutionRequest CreateRequest(
            string workflowFilePath,
            Dictionary<string, string> inputs = null) => new()
        {
            WorkflowFilePath = workflowFilePath,
            InputParameters = inputs ?? new Dictionary<string, string>()
        };

        /// <summary>
        /// Creates a request by resolving a workflow name to a file in the given directory.
        /// </summary>
        public static WorkflowExecutionRequest CreateRequestByName(
            string workflowName,
            string workflowsDirectory,
            Dictionary<string, string> inputs = null)
        {
            var req = new WorkflowExecutionRequest
            {
                WorkflowName = workflowName,
                WorkflowsDirectory = workflowsDirectory,
                InputParameters = inputs ?? new Dictionary<string, string>()
            };
            ResolveFilePath(req, workflowsDirectory);
            return req;
        }

        /// <summary>
        /// Parses a JSON string into a <see cref="WorkflowExecutionRequest"/>.
        /// </summary>
        public static WorkflowExecutionRequest ParseFromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return new WorkflowExecutionRequest();
            }

            try
            {
                return JsonConvert.DeserializeObject<WorkflowExecutionRequest>(json)
                    ?? new WorkflowExecutionRequest();
            }
            catch
            {
                return new WorkflowExecutionRequest();
            }
        }

        static void ParseQueryString(HttpRequestData request, WorkflowExecutionRequest executionRequest)
        {
            if (request.Url == null)
            {
                return;
            }

            // Capture the full URI so WorkflowExecutor can embed it in OpenAPI specs.
            executionRequest.WebServerUri = request.Url;

            // Infer the desired response format from the URL path extension.
            // This mirrors how WebServerController sets EmissionType from the request path:
            //   .xml  → XML,  .api → OPENAPI,  (none / .json) → JSON (default).
            var urlPath = request.Url.AbsolutePath;
            if (urlPath.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                executionRequest.ReturnType = EmitionTypes.XML;
            else if (urlPath.EndsWith(".api", StringComparison.OrdinalIgnoreCase))
                executionRequest.ReturnType = EmitionTypes.OPENAPI;
            // else: stays EmitionTypes.JSON (the default set in WorkflowExecutionRequest)

            var queryParams = HttpUtility.ParseQueryString(request.Url.Query);

            var workflowName = queryParams["workflowName"];
            if (!string.IsNullOrWhiteSpace(workflowName))
            {
                executionRequest.WorkflowName = workflowName;
            }

            var filePath = queryParams["workflowFilePath"];
            if (!string.IsNullOrWhiteSpace(filePath))
            {
                executionRequest.WorkflowFilePath = filePath;
            }

            var isDebug = queryParams["isDebug"];
            if (bool.TryParse(isDebug, out var debug))
            {
                executionRequest.IsDebug = debug;
            }

            foreach (var key in queryParams.AllKeys)
            {
                if (key != null && !_reservedQueryKeys.Contains(key))
                    executionRequest.InputParameters[key] = queryParams[key];
            }
        }

        static async Task ParseBodyAsync(HttpRequestData request, WorkflowExecutionRequest executionRequest)
        {
            if (request.Body == null || !request.Body.CanRead)
            {
                return;
            }

            try
            {
                using var reader = new StreamReader(request.Body);
                var body = await reader.ReadToEndAsync();

                if (string.IsNullOrWhiteSpace(body))
                {
                    return;
                }

                var bodyRequest = JsonConvert.DeserializeObject<WorkflowExecutionRequest>(body);
                if (bodyRequest == null)
                {
                    return;
                }

                if (!string.IsNullOrWhiteSpace(bodyRequest.WorkflowName))
                {
                    executionRequest.WorkflowName = bodyRequest.WorkflowName;
                }

                if (!string.IsNullOrWhiteSpace(bodyRequest.WorkflowFilePath))
                {
                    executionRequest.WorkflowFilePath = bodyRequest.WorkflowFilePath;
                }

                if (bodyRequest.IsDebug)
                {
                    executionRequest.IsDebug = true;
                }

                if (bodyRequest.InputParameters != null)
                {
                    foreach (var kvp in bodyRequest.InputParameters)
                    {
                        executionRequest.InputParameters[kvp.Key] = kvp.Value;
                    }
                }
            }
            catch
            {
                // Continue with what we have from query string
            }
        }

        /// <summary>
        /// If WorkflowFilePath is not set but WorkflowName is,
        /// resolve the file path from the workflows directory.
        /// </summary>
        static void ResolveFilePath(WorkflowExecutionRequest request, string workflowsDirectory)
        {
            if (!string.IsNullOrWhiteSpace(request.WorkflowFilePath))
            {
                request.WorkflowFilePath = request.WorkflowFilePath.Replace('/', Path.DirectorySeparatorChar);
                return;
            }

            if (string.IsNullOrWhiteSpace(request.WorkflowName) || string.IsNullOrWhiteSpace(workflowsDirectory))
            {
                return;
            }

            var fileName = request.WorkflowName.Replace('/', Path.DirectorySeparatorChar);
            if (!fileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase)
                && !fileName.EndsWith(".bite", StringComparison.OrdinalIgnoreCase))
            {
                var bitePath = Path.Combine(workflowsDirectory, fileName + ".bite");
                fileName += File.Exists(bitePath) ? ".bite" : ".xml";
            }

            request.WorkflowFilePath = Path.Combine(workflowsDirectory, fileName);
        }
    }
}
