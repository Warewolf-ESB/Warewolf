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
        readonly IUsageEventEmitter _usageEventEmitter;

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
            : this(executionLogger, usageEventEmitter: null)
        {
        }

        /// <summary>
        /// DI-friendly constructor.  <paramref name="usageEventEmitter"/> is optional —
        /// when null, a no-op emitter is used so existing call sites and tests that
        /// pass only the logger keep working unchanged.
        /// </summary>
        public WorkflowExecutor(IExecutionLogger executionLogger, IUsageEventEmitter usageEventEmitter)
        {
            _executionLogger = executionLogger ?? throw new ArgumentNullException(nameof(executionLogger));
            _usageEventEmitter = usageEventEmitter ?? NoOpUsageEventEmitter.Instance;
        }

        /// <summary>
        /// Executes a workflow from a file path with the provided input parameters.
        /// </summary>
        public WorkflowExecutionResult Execute(string workflowFilePath, Dictionary<string, string> inputs = null)
        {
            // No logging here: the request overload below logs start and completion with
            // richer data (workflow, return type, duration, error count).
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
                Dev2Logger.Error("WorkflowExecutor Execute called with null request", "WorkflowExecutor-Validation");
                return WorkflowExecutionResult.Failure("Execution request cannot be null.");
            }

            if (!request.IsValid)
            {
                Dev2Logger.Error("WorkflowExecutor Execute called with invalid request (missing WorkflowFilePath)", "WorkflowExecutor-Validation");
                return WorkflowExecutionResult.Failure("WorkflowFilePath must be provided.");
            }

            // OPENAPI short-circuit — generate the spec even when the workflow file is missing.
            // WorkflowOpenApiGenerator.ReadDataList handles missing/unreadable files gracefully
            // by returning an empty DataList, matching server GetOpenAPIServiceHandler behaviour.
            if (request.ReturnType == EmitionTypes.OPENAPI)
            {
                var openapiId   = Guid.NewGuid();
                var openapiStart = DateTime.UtcNow;
                Dev2Logger.Info($"WorkflowExecutor generating OpenAPI spec for: {request.WorkflowName ?? Path.GetFileNameWithoutExtension(request.WorkflowFilePath)}", openapiId.ToString());
                var resolvedNameForSpec = request.WorkflowName
                    ?? Path.GetFileNameWithoutExtension(request.WorkflowFilePath);
                var spec = WorkflowOpenApiGenerator.Generate(
                    request.WorkflowFilePath,
                    resolvedNameForSpec,
                    request.WebServerUri ?? new Uri("https://localhost"));
                return new WorkflowExecutionResult
                {
                    IsSuccess     = true,
                    ExecutionId   = openapiId,
                    StartTime     = openapiStart,
                    EndTime       = DateTime.UtcNow,
                    Duration      = DateTime.UtcNow - openapiStart,
                    ContentType   = "application/json",
                    PayloadWriter = (stream, ct) => WriteStringToStreamAsync(stream, spec, ct)
                };
            }

            if (!File.Exists(request.WorkflowFilePath))
            {
                Dev2Logger.Error($"WorkflowExecutor Execute: Workflow not found: {WorkflowIdentifier(request)}", "WorkflowExecutor-Validation");
                return WorkflowExecutionResult.Failure("Workflow not found.");
            }

            // License/subscription gate — mirrors ExecutorBase.TryExecute subscription check.
            // Controlled via WAREWOLF_LICENSE_CHECK_ENABLED env var (default: enabled).
            if (IsLicenseCheckEnabled())
            {
                try
                {
                    var subscription = Dev2.Runtime.Subscription.SubscriptionProvider.Instance.GetSubscriptionData();
                    if (subscription == null || !subscription.IsLicensed)
                    {
                        Dev2Logger.Warn("WorkflowExecutor Execute: License/subscription validation failed — execution blocked.", "WorkflowExecutor-License");
                        return WorkflowExecutionResult.Failure("Execution blocked: a valid Warewolf license/subscription is required.");
                    }
                }
                catch (Exception licEx)
                {
                    // Log only the exception type — a licensing/subscription failure can surface
                    // provider detail (endpoints, tokens) in its message. Logged once.
                    Dev2Logger.Error($"WorkflowExecutor Execute: License check threw an exception: {licEx.Message}", "WorkflowExecutor-License");
                    return WorkflowExecutionResult.Failure("Execution blocked: unable to validate license/subscription.");
                }
            }

            var stopwatch = Stopwatch.StartNew();
            var startTime = DateTime.UtcNow;
            var executionId = Guid.NewGuid();

            Dev2Logger.Info($"WorkflowExecutor Execute starting. Workflow: {WorkflowIdentifier(request)}, ReturnType: {request.ReturnType}, IsDebug: {request.IsDebug}", executionId.ToString());

            try
            {
                // Step 1: Read the workflow XML file
                var fileContents = ReadWorkflowFile(request.WorkflowFilePath);

                // Step 2: Extract XamlDefinition and DataList from the XML
                var (xamlDefinition, dataList, workflowName) = ExtractWorkflowParts(fileContents);

                if (xamlDefinition == null || xamlDefinition.Length == 0)
                {
                    Dev2Logger.Error("WorkflowExecutor Execute: No XamlDefinition found in workflow file", executionId.ToString());
                    return WorkflowExecutionResult.Failure("No XamlDefinition found in the workflow file.");
                }

                var resolvedName = request.WorkflowName
                    ?? workflowName
                    ?? Path.GetFileNameWithoutExtension(request.WorkflowFilePath);


                // OPENAPI � generate the spec from the DataList only; no XAML load or execution needed.
                if (request.ReturnType == EmitionTypes.OPENAPI)
                {
                    var spec = WorkflowOpenApiGenerator.Generate(
                        request.WorkflowFilePath,
                        resolvedName,
                        request.WebServerUri ?? new Uri("https://localhost"));
                    stopwatch.Stop();
                    Dev2Logger.Info($"WorkflowExecutor OpenAPI spec generated successfully. Duration: {stopwatch.Elapsed.TotalMilliseconds}ms", executionId.ToString());
                    return new WorkflowExecutionResult
                    {
                        IsSuccess   = true,
                        ExecutionId = executionId,
                        StartTime   = startTime,
                        EndTime     = DateTime.UtcNow,
                        Duration    = stopwatch.Elapsed,
                        ContentType = "application/json",
                        PayloadWriter = (stream, ct) => WriteStringToStreamAsync(stream, spec, ct)
                    };
                }

                // Step 3: Load XAML into a DynamicActivity (cached per normalised file path �
                // ActivityXamlServices.Load compiles XAML only once per unique workflow file).
                var dynamicActivity = GetOrLoadDynamicActivity(request.WorkflowFilePath, xamlDefinition);

                if (dynamicActivity == null)
                {
                    Dev2Logger.Error("WorkflowExecutor Execute: Failed to load DynamicActivity from XAML", executionId.ToString());
                    return WorkflowExecutionResult.Failure("Failed to load DynamicActivity from XAML.");
                }

                // Step 4: Parse DynamicActivity into IDev2Activity chain
                var activityParser = new ActivityParser();
                var startActivity = activityParser.Parse(dynamicActivity);

                if (startActivity == null)
                {
                    Dev2Logger.Error("WorkflowExecutor Execute: No start node found in workflow", executionId.ToString());
                    return WorkflowExecutionResult.Failure(GlobalConstants.NoStartNodeError);
                }

                // Step 5: Build DsfDataObject with inputs
                var (resourceId, versionNumber) = ExtractResourceIdentity(fileContents);
                var dataObject = BuildDataObject(request, executionId, resolvedName, dataList, resourceId, versionNumber);

                // Index DbSource bite files in the resources directory so they can be loaded
                // on demand by ServiceExecutionAbstract.GetSource(Guid) without pre-loading them all.
                var resourcesDir = request.WorkflowsDirectory ?? Path.GetDirectoryName(request.WorkflowFilePath) ?? string.Empty;
                LightweightSourceLoader.Instance.EnsureIndexed(resourcesDir);

                // Step 6: Execute the activity chain; route debug writes to a per-request
                // capturer so no global singleton (DebugMessageRepo) is touched.
                PerRequestDebugCapturer debugCapturer = null;
                if (request.IsDebug)
                {
                    debugCapturer = new PerRequestDebugCapturer();
                }

                using (debugCapturer != null ? DebugDispatcher.UseContextDispatcher(debugCapturer) : null)
                // Ambient suspend-snapshot context: when a SuspendExecutionActivity inside this
                // chain schedules a persistence job, LightweightJobValuesEnricher reads this
                // scope to stamp engine-specific keys (workflow name/path, execution id) into
                // the persisted job values — without touching the activity or the 5 legacy keys.
                using (SuspendSnapshotContext.BeginScope(resolvedName, request.WorkflowFilePath, dataObject.ExecutionID ?? executionId))
                {
                    // Emit workflow Start state before activities run � mirrors the Start marker
                    // the full Warewolf server emits from WfExecutionContainer.
                    if (debugCapturer != null)
                        EmitWorkflowStartState(resolvedName, request, startTime);

                    ExecuteActivityChain(dataObject, startActivity);

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

                Dev2Logger.Info($"WorkflowExecutor Execute completed. IsSuccess: {result.IsSuccess}, ErrorCount: {result.Errors.Count}, Duration: {stopwatch.Elapsed.TotalMilliseconds}ms", executionId.ToString());

                // Per-execution usage telemetry (8438) — emit AFTER the result is built so the
                // emit never affects response latency or content.  The emitter is non-throwing.
                _usageEventEmitter.TrackWorkflowExecution(new WorkflowUsageEvent(
                    workflowName: resolvedName,
                    executionId:  executionId,
                    duration:     stopwatch.Elapsed,
                    isSuccess:    result.IsSuccess,
                    errorCount:   result.Errors.Count,
                    startedAtUtc: startTime));

                return result;
            }
            catch (InvalidWorkflowException iwe)
            {
                stopwatch.Stop();
                // Exception is not passed to the Error sinks: the composite sinks persist
                // exception.ToString() (message + stack) and workflow exception text can embed
                // evaluated variable values. Logged once — Dev2Logger.Error already routes into
                // the IExecutionLogger sinks via Dev2LoggerSinkAdapter.
                Dev2Logger.Error($"WorkflowExecutor Execute: InvalidWorkflowException: {iwe.Message}", executionId.ToString());
                var msg = iwe.Message;
                var start = msg.IndexOf("Flowchart ", StringComparison.Ordinal);
                // The fallback must not return iwe.Message: a XAML/workflow-definition exception
                // can embed evaluated variable values and absolute paths. The NoStartNodeError
                // branch is unchanged. Full detail remains at Debug above.
                var errorMessage = start > 0
                    ? GlobalConstants.NoStartNodeError
                    : "Workflow execution failed because the workflow definition is invalid.";
                _usageEventEmitter.TrackWorkflowExecution(new WorkflowUsageEvent(
                    workflowName: Path.GetFileNameWithoutExtension(request.WorkflowFilePath) ?? string.Empty,
                    executionId:  executionId,
                    duration:     stopwatch.Elapsed,
                    isSuccess:    false,
                    errorCount:   1,
                    startedAtUtc: startTime));
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
                // See the InvalidWorkflowException branch: logged once, exception object withheld.
                Dev2Logger.Error($"WorkflowExecutor Execute: Unexpected exception for workflow: {WorkflowIdentifier(request)}: {ex.Message}", executionId.ToString());
                _usageEventEmitter.TrackWorkflowExecution(new WorkflowUsageEvent(
                    workflowName: Path.GetFileNameWithoutExtension(request.WorkflowFilePath) ?? string.Empty,
                    executionId:  executionId,
                    duration:     stopwatch.Elapsed,
                    isSuccess:    false,
                    errorCount:   1,
                    startedAtUtc: startTime));
                return new WorkflowExecutionResult
                {
                    IsSuccess = false,
                    ExecutionId = executionId,
                    // Catch-all handler: the exception can originate in any activity, so its
                    // message and stack can carry connector credentials, connection strings or
                    // evaluated variable values. ExecutionId below remains the caller's correlator.
                    Errors = new List<string> { "Workflow execution failed due to an unexpected error." },
                    StartTime = startTime,
                    EndTime = DateTime.UtcNow,
                    Duration = stopwatch.Elapsed
                };
            }
        }

        /// <summary>
        /// The workflow's route-relative identifier, for logs and caller-facing messages:
        /// the path that follows <c>/Public/</c> or <c>/Secure/</c>, already stripped of its
        /// <c>.json</c>/<c>.xml</c>/<c>.debug</c>/<c>.api</c> suffix by
        /// <see cref="NameSuffixParser.Parse"/> — e.g. <c>folder/myworkflow</c>.
        ///
        /// <para>Preferred over the file path: it is what the caller actually asked for,
        /// preserves the folder structure, and never exposes the server's directory layout.
        /// Falls back to the extension-free file name for the by-path entry points
        /// (<c>/workflow</c> and the <c>Execute(string)</c> overload) which carry no route name.</para>
        /// </summary>
        static string WorkflowIdentifier(WorkflowExecutionRequest request) =>
            !string.IsNullOrWhiteSpace(request.WorkflowName)
                ? request.WorkflowName
                : Path.GetFileNameWithoutExtension(request.WorkflowFilePath) ?? string.Empty;

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
        /// Extracts the resource identity from the workflow XML: the root <c>ID</c>
        /// attribute and the <c>VersionInfo/@VersionNumber</c> (default 1).
        /// Suspend/resume persistence keys these values —
        /// <c>SuspendExecutionActivity</c> persists <c>ResourceID</c> and
        /// <c>VersionNumber</c> from the data object, and resumption resolves the
        /// workflow by that ID — so they must be populated before execution.
        /// </summary>
        internal static (Guid resourceId, int versionNumber) ExtractResourceIdentity(StringBuilder fileContents)
        {
            try
            {
                var xe = fileContents.ToXElement();

                Guid.TryParse(
                    Dev2.Common.Common.ExtensionMethods.AttributeSafe(xe, "ID"),
                    out var resourceId);

                var versionNumber = 1;
                var versionInfo = xe.Element("VersionInfo");
                if (versionInfo != null
                    && int.TryParse(versionInfo.Attribute("VersionNumber")?.Value, out var parsed)
                    && parsed > 0)
                {
                    versionNumber = parsed;
                }

                return (resourceId, versionNumber);
            }
            catch
            {
                // Malformed XML is reported by the main parse path; identity stays default.
                return (Guid.Empty, 1);
            }
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
            string dataList,
            Guid resourceId,
            int versionNumber)
        {
            var rawPayload = BuildJsonPayload(request.InputParameters);
            var workflowDir = Path.GetDirectoryName(request.WorkflowFilePath) ?? string.Empty;

            var dataObject = new DsfDataObject(string.Empty, Guid.NewGuid(), rawPayload)
            {
                IsDebug = request.IsDebug,
                IsDebugFromWeb = request.IsDebug,
                ReturnType = request.ReturnType,
                ServiceName = workflowName,
                ExecutionID = request.ExecutionId ?? executionId,
                CustomTransactionID = request.CustomTransactionId ?? string.Empty,
                ExecutionToken = new LightweightExecutionToken(),
                EsbChannel = new LightweightEsbChannel(request.WorkflowsDirectory ?? workflowDir),
                // Suspend/resume requirements: SuspendExecutionActivity dereferences
                // ExecutingUser.Identity.Name and persists ResourceID + VersionNumber,
                // so all three must be populated on every execution.
                ResourceID = resourceId,
                VersionNumber = versionNumber,
                ExecutingUser = ResolveExecutingUser(request.ExecutingPrincipal)
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
        /// Returns the authenticated principal when it carries a usable identity name;
        /// otherwise substitutes a named <c>GenericPrincipal("Public")</c>. A principal
        /// with a null <c>Identity.Name</c> (e.g. <c>WorkflowClaimsPrincipal.Anonymous()</c>
        /// on <c>/public</c> routes) would persist an empty <c>currentuserprincipal</c> at
        /// suspend time and fail principal reconstruction on resume.
        /// </summary>
        internal static System.Security.Principal.IPrincipal ResolveExecutingUser(System.Security.Principal.IPrincipal principal)
        {
            if (!string.IsNullOrWhiteSpace(principal?.Identity?.Name))
            {
                return principal;
            }

            return new System.Security.Principal.GenericPrincipal(
                new System.Security.Principal.GenericIdentity("Public"),
                Array.Empty<string>());
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
                // Not logged, matching the server: Executor.DefaultExecutionResponse folds the
                // same Environment.Errors / AllErrors into the response and logs nothing —
                // a workflow reporting errors is business output, not a server fault. The count
                // is already on the "Execute completed" Info line, and the errors themselves
                // reach the caller via result.Errors.
                result.Errors = errors.FetchErrors().ToList();
            }

            if (dataObject.ExecutionException != null && result.Errors.Count == 0)
            {
                // Activity exceptions frequently carry evaluated variable values and connector
                // detail (connection strings, endpoints) in their message/stack, so only the
                // exception type reaches Error. Logged once.
                _executionLogger.LogError($"ExecuteActivityChain failed: {dataObject.ExecutionException.Message}", executionId);
                // Response body carries a generic message only — the activity exception's
                // message and stack stay out of it (full detail is at Debug above).
                result.Errors.Add("Workflow execution failed due to an unexpected error.");
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

        /// <summary>
        /// Returns <c>true</c> when the license/subscription gate should be enforced.
        /// Controlled by the <c>WAREWOLF_LICENSE_CHECK_ENABLED</c> environment variable.
        /// Defaults to <c>true</c> (enabled) when the variable is absent or not explicitly "false"/"0".
        /// </summary>
        static bool IsLicenseCheckEnabled()
        {
            var value = Environment.GetEnvironmentVariable("WAREWOLF_LICENSE_CHECK_ENABLED");
            if (string.IsNullOrWhiteSpace(value))
                return true;
            return !value.Equals("false", StringComparison.OrdinalIgnoreCase)
                && !value.Equals("0", StringComparison.Ordinal);
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
