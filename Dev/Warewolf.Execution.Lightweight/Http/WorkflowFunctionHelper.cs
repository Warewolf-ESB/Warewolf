using Dev2.Web;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Frozen;
using System.IO;
using System.Text;
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
        //
        // 'wid' is the workspace id. It is a TRANSPORT concern, not a workflow input: the full
        // server explicitly skips it in both SubmittedData.ExtractKeyValuePairForGetMethod and
        // ExtractKeyValuePairs ("Don't add the Workspace ID to DataList"). Without it here, a
        // caller that passes ?wid=... - which every Studio-issued URL does - gets a spurious
        // 'wid' scalar in the execution environment that the workflow never declared.
        static readonly FrozenSet<string> _reservedQueryKeys =
            new[] { "workflowName", "workflowFilePath", "isDebug", "wid" }
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

                // SECURITY: the ROUTE is authoritative once it names a workflow.
                //
                // Authorization is evaluated on the route-derived name (WorkflowHttpFunction:326)
                // BEFORE this request is parsed (:365). ResolveFilePath then returns early whenever
                // WorkflowFilePath is already populated - so a body- or query-supplied
                // 'workflowFilePath' used to win over the route and execute a DIFFERENT workflow
                // than the one that was authorized. On /Public that means reaching a workflow which
                // is not public at all.
                //
                // Clearing it here keeps the two decisions on the same subject. The generic
                // /workflow route passes no route name and is unaffected, so callers that legitimately
                // select a workflow by path keep working.
                executionRequest.WorkflowFilePath = null;
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

            // ── The ENTIRE query string is a raw XML or JSON payload ────────────────────────
            // Parity with SubmittedData.GetPostData:56-70, which slices everything after '?' and,
            // when it IsXml()/IsJSON(), returns it AS the payload without any key=value parsing.
            // Verified against the live server: GET /secure/Hello World.json?<DataList><Name>Sachin
            // </Name></DataList> and ?{"Name":"Sachin"} both bind and return "Hello Sachin.".
            //
            // This MUST be handled before ParseQueryString's key loop, because HttpUtility
            // .ParseQueryString gives a segment with no '=' a NULL key, and the loop below skips
            // null keys - so the whole payload was silently discarded and the workflow ran with
            // nothing bound. Like the server, this short-circuits: a payload query carries no
            // workflowName/wid/isDebug pairs to extract.
            if (TryGetRawQueryPayload(request.Url.Query, out var queryPayload))
            {
                executionRequest.RawInputPayload = queryPayload;
                return;
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

        /// <summary>
        /// Binds a <c>multipart/form-data</c> body, one input parameter per part.
        /// </summary>
        /// <remarks>
        /// Mirrors <c>SubmittedData.ExtractMultipartFormDataArgumentsFromDataList</c>, including
        /// its one non-obvious rule: a part that declares its OWN <c>Content-Type</c> is bound as
        /// <b>Base64</b>, and only an untyped part is bound as text. Verified against the live
        /// server — posting <c>Name=Sachin</c> as a typed part returns <c>"Hello U2FjaGlu."</c>.
        /// That is what makes file/binary uploads reachable from a workflow.
        ///
        /// <para>Parsing uses <see cref="MultipartReader"/> from the ASP.NET Core shared framework
        /// rather than a hand-rolled boundary scanner: quoted boundaries, CRLF handling and
        /// epilogues all have edge cases whose failure mode here would be a silently unbound
        /// input, which is the exact class of bug this work exists to remove.</para>
        ///
        /// <para>A malformed body binds nothing instead of throwing. The request then behaves like
        /// one with no inputs and fails on the workflow's first required variable, which is the
        /// same outcome the server produces and is preferable to a 500 from the parser.</para>
        /// </remarks>
        static async Task ParseMultipartAsync(
            HttpRequestData request, WorkflowExecutionRequest executionRequest, string contentType)
        {
            if (!MediaTypeHeaderValue.TryParse(contentType, out var mediaType))
            {
                return;
            }

            var boundary = HeaderUtilities.RemoveQuotes(mediaType.Boundary).Value;
            if (string.IsNullOrWhiteSpace(boundary))
            {
                return;
            }

            try
            {
                var reader = new MultipartReader(boundary, request.Body);

                MultipartSection section;
                while ((section = await reader.ReadNextSectionAsync()) != null)
                {
                    if (!ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var disposition))
                    {
                        continue;
                    }

                    var name = HeaderUtilities.RemoveQuotes(disposition.Name).Value;
                    if (string.IsNullOrEmpty(name) || _reservedQueryKeys.Contains(name))
                    {
                        continue;
                    }

                    using var buffer = new MemoryStream();
                    await section.Body.CopyToAsync(buffer);
                    var bytes = buffer.ToArray();

                    executionRequest.InputParameters[name] = string.IsNullOrEmpty(section.ContentType)
                        ? Encoding.UTF8.GetString(bytes)
                        : Convert.ToBase64String(bytes);
                }
            }
            catch (IOException)
            {
                // Truncated or malformed multipart stream — keep whatever parts were read.
            }
            catch (InvalidDataException)
            {
                // Boundary did not match the body — same treatment.
            }
        }

        /// <summary>
        /// True when the whole query string is itself an XML or JSON document rather than
        /// key=value pairs, in which case it IS the input payload.
        /// </summary>
        /// <remarks>
        /// Deliberately PARSES rather than sniffing the first character. The server's IsXml()/
        /// IsJSON() do real validation, and a cheap '&lt;' or '{' test would capture query strings
        /// that merely start with one - e.g. ?a=&lt;b - turning ordinary (if odd) parameters into an
        /// unparseable payload and losing them. Failing the parse falls through to normal
        /// key=value handling, which is the safe direction.
        /// </remarks>
        internal static bool TryGetRawQueryPayload(string query, out string payload)
        {
            payload = null;

            if (string.IsNullOrWhiteSpace(query))
            {
                return false;
            }

            var decoded = HttpUtility.UrlDecode(query.TrimStart('?'))?.Trim();
            if (string.IsNullOrWhiteSpace(decoded))
            {
                return false;
            }

            if (decoded.StartsWith("{", StringComparison.Ordinal))
            {
                try
                {
                    JObject.Parse(decoded);
                    payload = decoded;
                    return true;
                }
                catch (JsonException)
                {
                    return false;
                }
            }

            if (decoded.StartsWith("<", StringComparison.Ordinal))
            {
                try
                {
                    System.Xml.Linq.XDocument.Parse(decoded);
                    payload = decoded;
                    return true;
                }
                catch (System.Xml.XmlException)
                {
                    return false;
                }
            }

            return false;
        }

        static async Task ParseBodyAsync(HttpRequestData request, WorkflowExecutionRequest executionRequest)
        {
            if (request.Body == null || !request.Body.CanRead)
            {
                return;
            }

            var contentType = TryGetHeaderValue(request, "Content-Type") ?? string.Empty;

            // ── multipart/form-data ──────────────────────────────────────────────────────────
            // Handled FIRST and straight off the stream, because a part may be binary and reading
            // the body as a UTF-8 string would corrupt it. Parity with the full server's
            // SubmittedData.ExtractMultipartFormDataArgumentsFromDataList.
            if (contentType.Contains("multipart/form-data", StringComparison.OrdinalIgnoreCase))
            {
                await ParseMultipartAsync(request, executionRequest, contentType);
                return;
            }

            string body;
            using (var reader = new StreamReader(request.Body))
            {
                body = await reader.ReadToEndAsync();
            }

            if (string.IsNullOrWhiteSpace(body))
            {
                return;
            }

            // ── application/x-www-form-urlencoded ────────────────────────────────────────────
            // Parity with the full server, whose ExtractKeyValuePairForPostMethod falls through to
            // ExtractArgumentsFromDataListOrQueryString for a non-XML, non-JSON body - i.e. it treats
            // the body as a query string. Handled as INPUT PARAMETERS rather than as a raw payload,
            // because ExecutionEnvironmentUtils only understands JSON and XML: passing 'a=1&b=2' to it
            // would bind nothing at all.
            if (contentType.Contains("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase))
            {
                var form = HttpUtility.ParseQueryString(body);
                foreach (var key in form.AllKeys)
                {
                    if (key != null && !_reservedQueryKeys.Contains(key))
                    {
                        executionRequest.InputParameters[key] = form[key];
                    }
                }
                return;
            }

            // Only attempt the DTO/envelope parse when the body actually looks like a JSON object.
            // Previously an XML body reached JsonConvert.DeserializeObject<WorkflowExecutionRequest>,
            // threw, and was swallowed by the catch - so XML inputs were silently discarded even
            // though ExecutionEnvironmentUtils converts XML payloads perfectly well.
            var trimmed = body.TrimStart();
            var looksLikeJsonObject = trimmed.StartsWith("{", StringComparison.Ordinal);

            var hasEnvelopeInputs = false;
            if (looksLikeJsonObject)
            {
                try
                {
                    hasEnvelopeInputs = JObject.Parse(body)["inputParameters"] != null;
                }
                catch
                {
                    // Malformed JSON: fall through and treat the body as an opaque payload.
                }
            }

            // Anything that is NOT the documented envelope is preserved VERBATIM and handed to
            // ExecutionEnvironmentUtils, exactly as Dev2.Runtime.WebServer does with
            // WebRequestTO.RawRequestPayload. That is what makes a flat body, an XML body and
            // nested/recordset inputs bind here the same way they do on the full server.
            if (!hasEnvelopeInputs)
            {
                executionRequest.RawInputPayload = body;
            }

            if (!looksLikeJsonObject)
            {
                return;
            }

            try
            {
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
            fileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase) ? fileName[..^4] :
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
