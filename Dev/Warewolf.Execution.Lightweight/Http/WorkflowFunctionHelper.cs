using Dev2.Web;
using Microsoft.Azure.Functions.Worker.Http;
using Newtonsoft.Json;
using System.Collections.Frozen;
using System.Web;
using Warewolf.Execution.Lightweight.Models;

namespace Warewolf.Execution.Lightweight
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
        // Frozen once at startup — lookup is allocation-free and JIT-friendly.
        static readonly FrozenSet<string> _reservedQueryKeys =
            new[] { "workflowName", "workflowFilePath", "isDebug" }
                .ToFrozenSet(StringComparer.OrdinalIgnoreCase);
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

            // Attach the authenticated principal AFTER body parsing so a request payload
            // can never supply or override it. The auth middleware stores a
            // WorkflowClaimsPrincipal for every request (Anonymous() on /public routes).
            if (request.FunctionContext?.Items != null
                && request.FunctionContext.Items.TryGetValue(
                       Auth.Models.AuthConstants.PrincipalContextKey, out var principalObj)
                && principalObj is System.Security.Principal.IPrincipal principal)
            {
                executionRequest.ExecutingPrincipal = principal;
            }

            // Propagate Warewolf tracing headers — mirrors DataObjectExtensions.SetHeaders().
            var executionIdHeader = TryGetHeaderValue(request, "Warewolf-Execution-Id");
            if (!string.IsNullOrEmpty(executionIdHeader) && Guid.TryParse(executionIdHeader, out var parsedExecId))
                executionRequest.ExecutionId = parsedExecId;

            var customTxId = TryGetHeaderValue(request, "Warewolf-Custom-Transaction-Id");
            if (!string.IsNullOrEmpty(customTxId))
                executionRequest.CustomTransactionId = customTxId;

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
            else
            {
                // No URL suffix — honour Content-Type / Accept headers as a fallback.
                // Mirrors DataObjectExtensions.SetContentType() on the full server.
                var contentType = TryGetHeaderValue(request, "Content-Type")
                               ?? TryGetHeaderValue(request, "Accept");
                if (!string.IsNullOrEmpty(contentType))
                {
                    if (contentType.Contains("xml", StringComparison.OrdinalIgnoreCase))
                        executionRequest.ReturnType = EmitionTypes.XML;
                    else if (contentType.Contains("json", StringComparison.OrdinalIgnoreCase))
                        executionRequest.ReturnType = EmitionTypes.JSON;
                }
            }

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
                request.WorkflowFilePath = NormalizeSeparators(request.WorkflowFilePath);
                return;
            }

            if (string.IsNullOrWhiteSpace(request.WorkflowName) || string.IsNullOrWhiteSpace(workflowsDirectory))
            {
                return;
            }

            var fileName = NormalizeSeparators(request.WorkflowName);

            // Fast path: O(1) index lookup built at compile time — no disk I/O per request.
            var indexPath = WorkflowIndex.Instance.Resolve(workflowsDirectory, StripKnownExtension(fileName));
            if (indexPath != null)
            {
                request.WorkflowFilePath = indexPath;
                return;
            }

            var fileDirectory = Path.GetDirectoryName(Path.Combine(workflowsDirectory, fileName))
                                ?? workflowsDirectory;
            var baseName = Path.GetFileName(fileName);

            if (!fileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase)
                && !fileName.EndsWith(".bite", StringComparison.OrdinalIgnoreCase))
            {
                if (!Directory.Exists(fileDirectory))
                {
                    // Workflows directory absent — construct the default path without a disk hit.
                    request.WorkflowFilePath = Path.Combine(workflowsDirectory, fileName + ".xml");
                    return;
                }

                // On Linux the FS is case-sensitive: "hello World" must resolve to
                // "Hello World.bite". Prefer .bite; fall back to .xml.
                var resolved = FindFileCaseInsensitive(fileDirectory, baseName + ".bite")
                            ?? FindFileCaseInsensitive(fileDirectory, baseName + ".xml");
                if (resolved != null)
                {
                    request.WorkflowFilePath = resolved;
                    return;
                }

                // File not found on disk — default to .xml (preserves original behaviour).
                request.WorkflowFilePath = Path.Combine(workflowsDirectory, fileName + ".xml");
                return;
            }

            // Extension already present — resolve the actual on-disk casing if possible.
            request.WorkflowFilePath = FindFileCaseInsensitive(fileDirectory, baseName)
                                    ?? Path.Combine(workflowsDirectory, fileName);
        }

        /// <summary>
        /// Strips a trailing <c>.xml</c> or <c>.bite</c> extension so the result
        /// can be used as an index key that matches regardless of which file format
        /// is on disk.
        /// </summary>
        static string StripKnownExtension(string fileName) =>
            fileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase)  ? fileName[..^4] :
            fileName.EndsWith(".bite", StringComparison.OrdinalIgnoreCase) ? fileName[..^5] :
            fileName;

        // Reused per call — avoids allocating a new EnumerationOptions on every lookup.
        static readonly EnumerationOptions _caseInsensitiveOptions = new()
        {
            MatchCasing = MatchCasing.CaseInsensitive,
            RecurseSubdirectories = false,
        };

        /// <summary>
        /// Normalises both Windows (<c>\</c>) and Unix (<c>/</c>) directory separators to
        /// <see cref="Path.DirectorySeparatorChar"/>.
        /// On Linux this converts any Windows-style backslashes sent by clients on other
        /// platforms; on Windows it converts forward slashes to backslashes.
        /// A bare <c>.Replace('/', sep)</c> is insufficient on Linux because a backslash
        /// is a valid filename character there and would not be treated as a separator.
        /// </summary>
        static string NormalizeSeparators(string path) =>
            path.Replace('\\', Path.DirectorySeparatorChar)
                .Replace('/', Path.DirectorySeparatorChar);

        /// <summary>
        /// Returns the full path of the first file in <paramref name="directory"/> whose name
        /// matches <paramref name="fileName"/> using a case-insensitive comparison, or
        /// <c>null</c> when no match is found or the directory does not exist.
        /// <para>
        /// Uses <see cref="EnumerationOptions.MatchCasing"/> so the exact filename is passed as
        /// the search pattern — the runtime stops as soon as the first match is yielded rather
        /// than enumerating every file in the directory.
        /// Required on Linux where the file system is case-sensitive and the workflow name in
        /// the request URL may differ in casing from the file on disk.
        /// </para>
        /// </summary>
        static string? FindFileCaseInsensitive(string directory, string fileName)
        {
            if (!Directory.Exists(directory))
                return null;

            return Directory.EnumerateFiles(directory, fileName, _caseInsensitiveOptions)
                            .FirstOrDefault();
        }

        /// <summary>
        /// Reads the first value of <paramref name="headerName"/> from the request headers.
        /// Returns <c>null</c> when the header is absent or empty.
        /// </summary>
        static string? TryGetHeaderValue(HttpRequestData request, string headerName)
        {
            if (request.Headers.TryGetValues(headerName, out var values))
            {
                var v = values.FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(v))
                    return v;
            }
            return null;
        }
    }
}
