using Dev2;
using Dev2.Activities;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Data.TO;
using Dev2.Diagnostics;
using Dev2.Diagnostics.Debug;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Interfaces;
using Dev2.Runtime.Interfaces;
using Dev2.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Activities;
using System.Activities.XamlIntegration;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using Warewolf.Execution.Lightweight.Logging;
using Warewolf.Execution.Lightweight.Models;

namespace Warewolf.Execution.Lightweight
{
    /// <summary>
    /// Lightweight workflow executor that reads a workflow XML file directly,
    /// parses the XAML into a DynamicActivity, and walks the IDev2Activity chain.
    ///
    /// Execution pipeline (no ESB/ResourceCatalog/ServiceInvoker overhead):
    ///   1. Read workflow XML file from disk
    ///   2. Extract XamlDefinition + DataList from the XML
    ///   3. Load XAML ? DynamicActivity via ActivityXamlServices
    ///   4. Parse DynamicActivity ? IDev2Activity chain via ActivityParser
    ///   5. Build DsfDataObject with input parameters mapped into the environment
    ///   6. Walk IDev2Activity.Execute() in a loop
    ///   7. Extract output JSON from the execution environment
    /// </summary>
    public class WorkflowExecutor : IWorkflowExecutor
    {
        readonly IExecutionLogger _executionLogger;

        // Compiled DynamicActivity instances are expensive: ActivityXamlServices.Load parses
        // and compiles potentially hundreds of KB of XAML on every call.  Workflow files are
        // immutable within a single function deployment, so caching by normalised file path is
        // safe.  DynamicActivity is the compiled definition (not an execution instance) � all
        // runtime state flows through DsfDataObject � so sharing across concurrent requests is
        // thread-safe.  ActivityParser.Parse() is called fresh each time to get a new IDev2Activity
        // chain; only the expensive XAML compilation step is avoided on cache hits.
        private static readonly ConcurrentDictionary<string, DynamicActivity> _dynamicActivityCache =
            new(StringComparer.OrdinalIgnoreCase);

        public WorkflowExecutor(IExecutionLogger executionLogger)
        {
            _executionLogger = executionLogger ?? throw new ArgumentNullException(nameof(executionLogger));
        }

        /// <summary>
        /// Executes a workflow from a file path with the provided input parameters.
        /// </summary>
        public WorkflowExecutionResult Execute(string workflowFilePath, Dictionary<string, string> inputs = null)
        {
            return Execute(new WorkflowExecutionRequest
            {
                WorkflowFilePath = workflowFilePath,
                InputParameters = inputs ?? new Dictionary<string, string>()
            });
        }

        /// <summary>
        /// Executes a workflow based on a <see cref="WorkflowExecutionRequest"/>.
        /// </summary>
        public WorkflowExecutionResult Execute(WorkflowExecutionRequest request)
        {
            if (request == null)
            {
                return WorkflowExecutionResult.Failure("Execution request cannot be null.");
            }

            if (!request.IsValid)
            {
                return WorkflowExecutionResult.Failure("WorkflowFilePath must be provided.");
            }

            // OPENAPI: generate the spec before the file-exists guard so that
            // workflows whose top-level .xml is absent (e.g. only test-case sub-files
            // exist) still return a valid 200 spec.  ReadDataList handles missing
            // files with an empty <DataList />, producing a minimal but valid spec.
            if (request.ReturnType == EmitionTypes.OPENAPI)
            {
                var openApiName = request.WorkflowName
                    ?? Path.GetFileNameWithoutExtension(request.WorkflowFilePath);
                var spec = WorkflowOpenApiGenerator.Generate(
                    request.WorkflowFilePath,
                    openApiName,
                    request.WebServerUri ?? new Uri("https://localhost"));
                return new WorkflowExecutionResult
                {
                    IsSuccess     = true,
                    ExecutionId   = Guid.NewGuid(),
                    StartTime     = DateTime.UtcNow,
                    EndTime       = DateTime.UtcNow,
                    Duration      = TimeSpan.Zero,
                    ContentType   = "application/json",
                    PayloadWriter = (stream, ct) => WriteStringToStreamAsync(stream, spec, ct)
                };
            }

            if (!File.Exists(request.WorkflowFilePath))
            {
                return WorkflowExecutionResult.Failure($"Workflow file not found: {request.WorkflowFilePath}");
            }

            var stopwatch = Stopwatch.StartNew();
            var startTime = DateTime.UtcNow;
            var executionId = Guid.NewGuid();

            try
            {
                // Step 1: Read the workflow XML file
                var fileContents = ReadWorkflowFile(request.WorkflowFilePath);

                // Step 2: Extract XamlDefinition and DataList from the XML
                var (xamlDefinition, dataList, workflowName) = ExtractWorkflowParts(fileContents);

                if (xamlDefinition == null || xamlDefinition.Length == 0)
                {
                    return WorkflowExecutionResult.Failure("No XamlDefinition found in the workflow file.");
                }

                var resolvedName = request.WorkflowName
                    ?? workflowName
                    ?? Path.GetFileNameWithoutExtension(request.WorkflowFilePath);

                // (OPENAPI is handled before execution starts — see short-circuit above.)
                // Step 3: Load XAML into a DynamicActivity (cached per normalised file path �
                // ActivityXamlServices.Load compiles XAML only once per unique workflow file).
                var dynamicActivity = GetOrLoadDynamicActivity(request.WorkflowFilePath, xamlDefinition);

                if (dynamicActivity == null)
                {
                    return WorkflowExecutionResult.Failure("Failed to load DynamicActivity from XAML.");
                }

                // Step 4: Parse DynamicActivity into IDev2Activity chain
                var activityParser = new ActivityParser();
                var startActivity = activityParser.Parse(dynamicActivity);

                if (startActivity == null)
                {
                    return WorkflowExecutionResult.Failure(GlobalConstants.NoStartNodeError);
                }

                // Step 5: Build DsfDataObject with inputs
                var dataObject = BuildDataObject(request, executionId, resolvedName, dataList);

                // Index DbSource bite files in the resources directory so they can be loaded
                // on demand by ServiceExecutionAbstract.GetSource(Guid) without pre-loading them all.
                var resourcesDir = request.WorkflowsDirectory ?? Path.GetDirectoryName(request.WorkflowFilePath) ?? string.Empty;
                LightweightSourceLoader.Instance.EnsureIndexed(resourcesDir);
                _executionLogger.LogInfo($"[SourceLoader] EnsureIndexed dir='{resourcesDir}' | {AmbientSourceLoader.Current?.GetDiagnostics() ?? "AmbientSourceLoader.Current=null"}", executionId);

                // Step 6: Execute the activity chain; route debug writes to a per-request
                // capturer so no global singleton (DebugMessageRepo) is touched.
                PerRequestDebugCapturer debugCapturer = null;
                if (request.IsDebug)
                    debugCapturer = new PerRequestDebugCapturer();

                using (debugCapturer != null ? DebugDispatcher.UseContextDispatcher(debugCapturer) : null)
                {
                    // Emit workflow Start state before activities run � mirrors the Start marker
                    // the full Warewolf server emits from WfExecutionContainer.
                    if (debugCapturer != null)
                        EmitWorkflowStartState(resolvedName, request, startTime);

                    ExecuteActivityChain(dataObject, startActivity);
                    _executionLogger.LogInfo($"[SourceLoader] post-execution | {AmbientSourceLoader.Current?.GetDiagnostics() ?? "AmbientSourceLoader.Current=null"}", executionId);

                    // Emit workflow End state after activities finish � mirrors the End marker
                    // the full Warewolf server emits, including the final output variable values.
                    if (debugCapturer != null)
                        EmitWorkflowEndState(dataObject, resolvedName, dataList, startTime);
                }

                // Step 7: Extract outputs
                stopwatch.Stop();
                var result = new WorkflowExecutionResult
                {
                    ExecutionId = executionId,
                    StartTime = startTime,
                    EndTime = DateTime.UtcNow,
                    Duration = stopwatch.Elapsed
                };

                CollectErrors(dataObject, result, executionId);
                ExtractPayload(dataObject, dataList, request, result);
                if (debugCapturer != null)
                {
                    // Mirror Executor.DebugFromWebExecutionResponse: build a parent?child tree
                    // from the raw IDebugState objects (using ParentID links), then map to the
                    // serialisable DebugStepResult model.  Duration-only states are filtered out,
                    // matching the full server's Where(state => state.StateType != StateType.Duration).
                    var rawStates = debugCapturer.States
                        .Where(s => s.StateType != StateType.Duration)
                        .ToArray();
                    var tree = DebugStateTreeBuilder.BuildTree(rawStates);
                    result.DebugStates = tree.Select(MapDebugState).ToList();

                    // Debug mode: the response IS the debug tree, NOT the normal workflow output �
                    // mirrors Executor.DebugFromWebExecutionResponse on the full Warewolf server.
                    result.ContentType = "application/json";
                    result.PayloadWriter = (stream, ct) =>
                        WriteStringToStreamAsync(stream, JsonConvert.SerializeObject(new
                        {
                            hasErrors  = result.Errors.Count > 0,
                            errors     = result.Errors,
                            debugStates = result.DebugStates
                        }, Formatting.Indented), ct);
                }

                result.IsSuccess = result.Errors.Count == 0;

                return result;
            }
            catch (InvalidWorkflowException iwe)
            {
                stopwatch.Stop();
                _executionLogger.LogError(nameof(Execute), iwe, executionId);
                var msg = iwe.Message;
                var start = msg.IndexOf("Flowchart ", StringComparison.Ordinal);
                var errorMessage = start > 0 ? GlobalConstants.NoStartNodeError : iwe.Message;
                return new WorkflowExecutionResult
                {
                    IsSuccess = false,
                    ExecutionId = executionId,
                    Errors = new List<string> { errorMessage },
                    StartTime = startTime,
                    EndTime = DateTime.UtcNow,
                    Duration = stopwatch.Elapsed
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _executionLogger.LogError(nameof(Execute), ex, executionId);
                return new WorkflowExecutionResult
                {
                    IsSuccess = false,
                    ExecutionId = executionId,
                    Errors = new List<string> { $"{ex.Message}{Environment.NewLine}{ex.StackTrace}" },
                    StartTime = startTime,
                    EndTime = DateTime.UtcNow,
                    Duration = stopwatch.Elapsed
                };
            }
        }

        /// <summary>
        /// Step 1: Read the workflow resource XML file from disk.
        /// One allocation via <see cref="File.ReadAllText"/> instead of a per-line
        /// <see cref="StringBuilder"/> growth loop; XML/XAML parsers are insensitive
        /// to blank lines so the earlier filtering pass is not needed.
        /// </summary>
        internal static StringBuilder ReadWorkflowFile(string filePath)
            => new StringBuilder(File.ReadAllText(filePath));

        /// <summary>
        /// Step 2: Parse the resource XML to extract the XAML definition and DataList.
        /// Warewolf resource files have structure:
        ///   &lt;Service Name="..."&gt;
        ///     &lt;Actions&gt;
        ///       &lt;Action Type="Workflow"&gt;
        ///         &lt;XamlDefinition&gt;...XAML...&lt;/XamlDefinition&gt;
        ///       &lt;/Action&gt;
        ///     &lt;/Actions&gt;
        ///     &lt;DataList&gt;...&lt;/DataList&gt;
        ///   &lt;/Service&gt;
        /// </summary>
        internal static (StringBuilder xamlDefinition, string dataList, string workflowName) ExtractWorkflowParts(StringBuilder fileContents)
        {
            var xe = fileContents.ToXElement();

            var workflowName = Dev2.Common.Common.ExtensionMethods.AttributeSafe(xe, "Name");

            var actions = xe.Element("Actions");
            var action = actions != null ? actions.Element("Action") : xe.Element("Action");

            StringBuilder xamlDefinition = null;
            if (action != null)
            {
                var xElement = action.Element("XamlDefinition");
                if (xElement != null)
                {
                    var def = xElement.ToStringBuilder();
                    def = def.Replace("<XamlDefinition>", "").Replace("</XamlDefinition>", "");
                    xamlDefinition = def.Unescape();
                }
            }

            string dataList = null;
            var dataListElement = xe.Element("DataList");
            if (dataListElement != null)
            {
                dataList = dataListElement.ToString();
            }

            return (xamlDefinition, dataList, workflowName);
        }

        /// <summary>
        /// Step 3: Load XAML definition into a DynamicActivity using ActivityXamlServices.
        /// Applies namespace cleaning for cross-platform compatibility.
        /// </summary>
        internal static DynamicActivity LoadDynamicActivity(StringBuilder xamlDefinition)
        {
            if (GlobalConstants.RuntimeNamespaceClean)
            {
                xamlDefinition = new Dev2XamlCleaner().CleanServiceDef(xamlDefinition);
            }

            Dev2XamlLoader.RemoveWindowsElements(ref xamlDefinition);

            using var xamlStream = xamlDefinition.EncodeForXmlDocument();
            var activity = ActivityXamlServices.Load(xamlStream);
            return activity as DynamicActivity;
        }

        /// <summary>
        /// Returns the <see cref="DynamicActivity"/> for <paramref name="filePath"/> from the
        /// process-level cache, compiling it from <paramref name="xamlDefinition"/> on first access.
        /// Subsequent calls for the same path skip <see cref="ActivityXamlServices.Load"/> entirely.
        /// </summary>
        internal static DynamicActivity GetOrLoadDynamicActivity(string filePath, StringBuilder xamlDefinition)
            => _dynamicActivityCache.GetOrAdd(
                Path.GetFullPath(filePath),
                _ => LoadDynamicActivity(xamlDefinition));

        /// <summary>
        /// Step 5: Build DsfDataObject and map input parameters into the execution environment.
        /// </summary>
        static IDSFDataObject BuildDataObject(
            WorkflowExecutionRequest request,
            Guid executionId,
            string workflowName,
            string dataList)
        {
            var rawPayload = BuildJsonPayload(request.InputParameters);
            var workflowDir = Path.GetDirectoryName(request.WorkflowFilePath) ?? string.Empty;

            var dataObject = new DsfDataObject(string.Empty, Guid.NewGuid(), rawPayload)
            {
                IsDebug = request.IsDebug,
                IsDebugFromWeb = request.IsDebug,
                ReturnType = request.ReturnType,
                ServiceName = workflowName,
                ExecutionID = executionId,
                ExecutionToken = new LightweightExecutionToken(),
                EsbChannel = new LightweightEsbChannel(request.WorkflowsDirectory ?? workflowDir)
            };

            if (!string.IsNullOrEmpty(dataList)
                && request.InputParameters != null
                && request.InputParameters.Count > 0)
            {
                ExecutionEnvironmentUtils.UpdateEnvironmentFromInputPayload(
                    dataObject,
                    dataObject.RawPayload,
                    dataList);
            }

            return dataObject;
        }

        /// <summary>
        /// Serialize input parameters to a JSON payload string.
        /// </summary>
        static string BuildJsonPayload(Dictionary<string, string> inputs)
        {
            if (inputs == null || inputs.Count == 0)
            {
                return string.Empty;
            }

            var jObject = new JObject();
            foreach (var kvp in inputs)
            {
                jObject[kvp.Key] = kvp.Value;
            }
            return jObject.ToString(Formatting.None);
        }

        /// <summary>
        /// Step 6: Walk the IDev2Activity linked list, calling Execute() on each node.
        /// This is the stripped-down version of WfExecutionContainer.ExecuteNode �
        /// no ExecutionManager, no SubscriptionProvider, no StateNotifier overhead.
        /// </summary>
        internal static void ExecuteActivityChain(IDSFDataObject dataObject, IDev2Activity startActivity)
        {
            var next = startActivity;
            var environment = dataObject.Environment;

            while (next != null)
            {
                var current = next;
                next = current.Execute(dataObject, 0);
                environment.AllErrors.UnionWith(environment.Errors);

                if (dataObject.StopExecution)
                {
                    var fetchedErrors = environment.FetchErrors();
                    if (!string.IsNullOrEmpty(fetchedErrors))
                    {
                        dataObject.ExecutionException = new Exception(fetchedErrors);
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// Emits a workflow-level <c>StateType.Start</c> debug state to the active
        /// <see cref="PerRequestDebugCapturer"/> via <see cref="DebugDispatcher"/>.
        /// Mirrors the Start marker the full Warewolf server emits from WfExecutionContainer,
        /// including one <see cref="DebugItem"/> per input parameter so callers can see
        /// what values were passed into the workflow.
        /// Must be called inside a <c>DebugDispatcher.UseContextDispatcher</c> scope.
        /// </summary>
        static void EmitWorkflowStartState(string workflowName, WorkflowExecutionRequest request, DateTime startTime)
        {
            var state = new DebugState
            {
                StateType = StateType.Start,
                ActivityType = ActivityType.Workflow,
                DisplayName = workflowName,
                Name = "Start",
                StartTime = startTime,
                EndTime = startTime,
                IsDurationVisible = false,
                ExecutionOrigin = ExecutionOrigin.External
            };

            if (request.InputParameters != null)
            {
                foreach (var kv in request.InputParameters)
                {
                    var item = new DebugItem();
                    item.Add(new DebugItemResult
                    {
                        Type = DebugItemResultType.Variable,
                        Variable = $"[[{kv.Key}]]",
                        Operator = "=",
                        Value = kv.Value ?? string.Empty,
                        TruncatedValue = kv.Value ?? string.Empty
                    });
                    state.Inputs.Add(item);
                }
            }

            DebugDispatcher.Instance.Write(new WriteArgs { debugState = state, isDebugFromWeb = true });
        }

        /// <summary>
        /// Emits a workflow-level <c>StateType.End</c> debug state to the active
        /// <see cref="PerRequestDebugCapturer"/> via <see cref="DebugDispatcher"/>.
        /// Mirrors the End marker the full Warewolf server emits, including the final
        /// output variable values extracted from the execution environment via the DataList.
        /// Must be called inside a <c>DebugDispatcher.UseContextDispatcher</c> scope.
        /// </summary>
        static void EmitWorkflowEndState(IDSFDataObject dataObject, string workflowName, string dataList, DateTime startTime)
        {
            var endTime = DateTime.UtcNow;
            var state = new DebugState
            {
                StateType = StateType.End,
                ActivityType = ActivityType.Workflow,
                DisplayName = workflowName,
                Name = "End",
                StartTime = startTime,
                EndTime = endTime,
                IsDurationVisible = true
            };

            // Extract final output variables from the environment using the DataList schema,
            // matching how the full server populates the End state Outputs list.
            if (!string.IsNullOrEmpty(dataList))
            {
                try
                {
                    var json = ExecutionEnvironmentUtils.GetJsonOutputFromEnvironment(dataObject, dataList, 0);
                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        var outputs = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                        if (outputs != null)
                        {
                            foreach (var kv in outputs)
                            {
                                var value = kv.Value?.ToString() ?? string.Empty;
                                var item = new DebugItem();
                                item.Add(new DebugItemResult
                                {
                                    Type = DebugItemResultType.Variable,
                                    Variable = $"[[{kv.Key}]]",
                                    Operator = "=",
                                    Value = value,
                                    TruncatedValue = value
                                });
                                state.Outputs.Add(item);
                            }
                        }
                    }
                }
                catch { /* best-effort � payload extraction must not fail the debug response */ }
            }

            DebugDispatcher.Instance.Write(new WriteArgs { debugState = state, isDebugFromWeb = true });
        }

        /// <summary>
        /// Collect errors from the execution environment into the result.
        /// Errors and the ExecutionException (if any) are also emitted via <see cref="IExecutionLogger"/>
        /// so they appear in Application Insights / Azure Monitor with the full stack trace.
        /// </summary>
        void CollectErrors(IDSFDataObject dataObject, WorkflowExecutionResult result, Guid executionId)
        {
            var errors = new ErrorResultTO();
            foreach (var err in dataObject.Environment.Errors.ToList())
            {
                errors.AddError(err, true);
            }
            foreach (var err in dataObject.Environment.AllErrors.ToList())
            {
                errors.AddError(err, true);
            }

            if (errors.HasErrors())
            {
                result.Errors = errors.FetchErrors().ToList();
                foreach (var err in result.Errors)
                {
                    _executionLogger.LogWarning(err, executionId);
                }
            }

            if (dataObject.ExecutionException != null && result.Errors.Count == 0)
            {
                _executionLogger.LogError("ExecuteActivityChain", dataObject.ExecutionException, executionId);
                result.Errors.Add($"{dataObject.ExecutionException.Message}{Environment.NewLine}{dataObject.ExecutionException.StackTrace}");
            }
        }

        /// <summary>
        /// Mirrors ExecutionDtoExtensions.GetExecutePayload
        /// based on ReturnType and populates result.Payload + result.ContentType.
        ///   XML  ? ExecutionEnvironmentUtils.GetXmlOutputFromEnvironment  (DataList-shaped XML)
        ///   JSON ? ExecutionEnvironmentUtils.GetJsonOutputFromEnvironment  (DataList-shaped JSON)
        ///          Falls back to environment.ToJson() when no DataList is available.
        ///   OPENAPI is handled before execution ever starts (see short-circuit in Execute).
        ///
        /// Instead of storing the output as a string on the result, a <see cref="WorkflowExecutionResult.PayloadWriter"/>
        /// delegate is set. The delegate computes the string on demand when the HTTP response is being
        /// written and streams it via <see cref="WriteStringToStreamAsync"/> � a <see cref="StreamWriter"/>
        /// encodes chars in 4 KB chunks directly to the response body, avoiding the full
        /// <c>byte[]</c> allocation that <c>HttpResponseData.WriteStringAsync</c> would create.
        /// </summary>
        static void ExtractPayload(IDSFDataObject dataObject, string dataList, WorkflowExecutionRequest request, WorkflowExecutionResult result)
        {
            try
            {
                switch (request.ReturnType)
                {
                    case EmitionTypes.XML:
                        result.ContentType = "text/xml";
                        result.PayloadWriter = (stream, ct) =>
                        {
                            var xml = !string.IsNullOrEmpty(dataList)
                                ? ExecutionEnvironmentUtils.GetXmlOutputFromEnvironment(dataObject, dataList, 0)
                                : "<DataList />";

                            // Inject <Errors> into the XML so the browser can see them
                            if (result.Errors.Count > 0)
                            {
                                var errXml = new StringBuilder();
                                foreach (var err in result.Errors)
                                    errXml.Append($"<Error><![CDATA[{err}]]></Error>");

                                var closeIdx = xml.LastIndexOf("</", StringComparison.Ordinal);
                                xml = closeIdx > 0
                                    ? xml.Insert(closeIdx, $"<Errors>{errXml}</Errors>")
                                    : xml + $"<Errors>{errXml}</Errors>";
                            }

                            return WriteStringToStreamAsync(stream, xml, ct);
                        };
                        break;

                    default: // JSON
                        result.ContentType = "application/json";
                        result.PayloadWriter = async (stream, ct) =>
                        {
                            var json = !string.IsNullOrEmpty(dataList)
                                ? ExecutionEnvironmentUtils.GetJsonOutputFromEnvironment(dataObject, dataList, 0)
                                : dataObject.Environment.ToJson();
                            TryPopulateOutputsDictionary(result, json);

                            // Wrap in an error envelope so errors are visible in the browser
                            if (result.Errors.Count > 0)
                            {
                                JToken output;
                                try { output = JToken.Parse(json); }
                                catch { output = new JValue(json); }

                                var envelope = new JObject
                                {
                                    ["hasErrors"] = true,
                                    ["errors"] = new JArray(result.Errors),
                                    ["output"] = output
                                };
                                await WriteStringToStreamAsync(stream, envelope.ToString(Formatting.Indented), ct);
                            }
                            else
                            {
                                await WriteStringToStreamAsync(stream, json, ct);
                            }
                        };
                        break;
                }
            }
            catch
            {
                // Payload extraction is best-effort; errors are captured in result.Errors
            }
        }

        /// <summary>
        /// Deserialises <paramref name="json"/> into the convenience <see cref="WorkflowExecutionResult.Outputs"/> dictionary.
        /// </summary>
        static void TryPopulateOutputsDictionary(WorkflowExecutionResult result, string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return;
            try
            {
                var parsed = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                if (parsed != null)
                    result.Outputs = parsed;
            }
            catch { /* best-effort */ }
        }

        static DebugStepResult MapDebugState(IDebugState state) => new()
        {
            // Identity
            DisconnectedID       = state.DisconnectedID,
            ID                   = state.ID,
            SourceResourceID     = state.SourceResourceID,
            OriginatingResourceID = state.OriginatingResourceID,
            OriginalInstanceID   = state.OriginalInstanceID,
            // Session / routing
            SessionID            = state.SessionID,
            WorkspaceID          = state.WorkspaceID,
            ServerID             = state.ServerID,
            EnvironmentID        = state.EnvironmentID,
            ClientID             = state.ClientID,
            // Activity metadata
            DisplayName          = state.DisplayName,
            ActivityType         = (int)state.ActivityType,
            ActualType           = state.ActualType,
            StateType            = state.StateType.ToString(),
            // Status
            HasError             = state.HasError,
            ErrorMessage         = state.ErrorMessage,
            // Execution origin
            Origin               = state.Origin,
            ExecutionOrigin      = (int)state.ExecutionOrigin,
            WorkSurfaceMappingId = state.WorkSurfaceMappingId,
            // Timing
            IsDurationVisible    = state.IsDurationVisible,
            Duration             = state.Duration,
            StartTime            = state.StartTime,
            EndTime              = state.EndTime,
            // Debug items
            Inputs               = MapDebugItems(state.Inputs),
            Outputs              = MapDebugItems(state.Outputs),
            AssertResultList     = MapDebugItems(state.AssertResultList),
            // Tree
            Children             = state.Children?.Select(MapDebugState).ToList() ?? new List<DebugStepResult>()
        };

        static List<List<DebugLineItem>> MapDebugItems(List<IDebugItem> items) =>
            items?.Select(item => item.FetchResultsList()
                .Select(r => new DebugLineItem
                {
                    Type = (int)r.Type,
                    Label = r.Label,
                    Variable = r.Variable,
                    Operator = r.Operator,
                    Value = r.Value,
                    TruncatedValue = r.TruncatedValue,
                    GroupName = r.GroupName,
                    GroupIndex = r.GroupIndex,
                    MoreLink = r.MoreLink,
                    HasError = r.HasError
                })
                .ToList())
                .ToList() ?? new List<List<DebugLineItem>>();

        // UTF-8 without BOM � matches Azure Functions WriteStringAsync encoding behaviour.
        static readonly Encoding _utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

        /// <summary>
        /// Encodes <paramref name="content"/> to <paramref name="stream"/> via a
        /// <see cref="StreamWriter"/> (4 KB char buffer), avoiding the full
        /// <c>byte[]</c> heap allocation that <c>HttpResponseData.WriteStringAsync</c>
        /// creates internally through <c>Encoding.UTF8.GetBytes(string)</c>.
        /// <paramref name="stream"/> is left open so the caller can finalise the response.
        /// </summary>
        static async Task WriteStringToStreamAsync(Stream stream, string content, CancellationToken ct)
        {
            await using var writer = new StreamWriter(stream, _utf8NoBom, bufferSize: 4096, leaveOpen: true);
            await writer.WriteAsync(content.AsMemory(), ct);
        }

        }

    /// <summary>
    /// Minimal IExecutionToken implementation � no server-side dependencies.
    /// </summary>
    internal class LightweightExecutionToken : IExecutionToken
    {
        public bool IsUserCanceled { get; set; }
    }
}
