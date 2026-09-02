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
using Warewolf.Execution.Lightweight.Infrastructure;
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
        // and compiles potentially hundreds of KB of XAML on every call, so they must be reused.
        //
        // THEY MUST NOT BE SHARED BY CONCURRENT EXECUTIONS. The previous design cached ONE
        // DynamicActivity per path and relied on "all runtime state flows through DsfDataObject".
        // That is false: ActivityParser.Parse() does not clone anything - it walks the cached
        // Flowchart via WorkflowInspectionServices.GetActivities() and hands back references to
        // the SAME Dsf*Activity objects (ActivityParser.cs:192-202). Those objects carry
        // per-execution state in instance fields; every database activity assigns
        //     ServiceExecution = new DatabaseServiceExecution(dataObject)
        // in BeforeExecutionStart and reads it back in ExecutionImpl. Two concurrent executions
        // therefore overwrite each other's ServiceExecution, and the loser executes against the
        // winner's DsfDataObject - so its output variable is never written.
        //
        // Measured live 2026-08-11 against rabbit\RabbitProcess (a SQL workflow):
        //     sequential x10      -> 10/10 HTTP 200
        //     concurrency 3       ->  3/3  HTTP 200
        //     concurrency 4/6/10  ->  3/4, 4/6, 8/10; the rest HTTP 500
        //                            "Object reference not set to an instance of an object."
        //                            "Error with variables in input. [[JobLogId]]"
        //     a SQL-FREE workflow -> 20/20 at concurrency 20 (an Assign has no such instance state)
        // In the queue path this is worse than a plain error: the worker dead-letters AND acks a
        // business failure, so the queue drains to zero and the deployment looks healthy while
        // valid messages are silently diverted (16 of 30 in the 2026-08-11 burst).
        //
        // The fix is exclusive ownership: each execution RENTS a prepared workflow and RETURNS it
        // in a finally. The pool grows to the peak concurrency seen for that workflow and no
        // further, so the XAML compile and the parse are both still amortised. Sequential reuse of
        // a returned instance is exactly what the old cache already did on every call, and is
        // proven by the 10/10 sequential result above.
        //
        // Fixed in Warewolf.Execution.Lightweight ONLY, deliberately: the offending instance field
        // lives in shared Dev2.Activities code used by the on-prem server and Studio, and pooling
        // here fixes every activity carrying that pattern - not just the six database activities -
        // without changing behaviour for those hosts. The shared-code defect remains latent there.
        private static readonly ConcurrentDictionary<string, ConcurrentBag<PreparedWorkflow>> _workflowPool =
            new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// A compiled workflow plus its parsed activity chain, owned EXCLUSIVELY by one execution
        /// between <see cref="RentPreparedWorkflow"/> and <see cref="ReturnPreparedWorkflow"/>.
        /// The chain is kept with the activity it was parsed from because the two are the same
        /// object graph - parsing a rented activity again would only re-walk the identical tree.
        /// </summary>
        internal sealed class PreparedWorkflow
        {
            internal DynamicActivity Activity { get; init; }
            internal IDev2Activity StartActivity { get; init; }

            /// <summary>
            /// The pool key this instance was compiled under, stamped at rent time. Returning uses
            /// THIS rather than recomputing from the file, so an instance compiled from the old
            /// definition can never be filed under the key of a definition that changed while it
            /// was executing.
            /// </summary>
            internal string PoolKey { get; set; }
        }

        public WorkflowExecutor(IExecutionLogger executionLogger)
            : this(executionLogger, null)
        {
        }

        /// <summary>
        /// WOLF-8516: <paramref name="config"/> is optional (defaulted by DI, omittable by the
        /// many existing single-arg test call sites) rather than required, so the pool cap can
        /// be sourced from <see cref="HostEnvironmentConfig.WorkflowPoolMax"/> (deploy-bundled
        /// settings file only — no env var) in production without a breaking constructor-signature
        /// change across every test that constructs a bare <c>new WorkflowExecutor(logger)</c>.
        /// </summary>
        public WorkflowExecutor(IExecutionLogger executionLogger, HostEnvironmentConfig config)
        {
            _executionLogger = executionLogger ?? throw new ArgumentNullException(nameof(executionLogger));
            if (config is not null)
            {
                MaxPooledPerWorkflow = config.WorkflowPoolMax;
            }
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
               // Dev2Logger.Error($"WorkflowExecutor Execute: Workflow not found: {WorkflowIdentifier(request)}", "WorkflowExecutor-Validation");
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

           // Dev2Logger.Info($"WorkflowExecutor Execute starting. Workflow: {WorkflowIdentifier(request)}, ReturnType: {request.ReturnType}, IsDebug: {request.IsDebug}", executionId.ToString());

            // Declared outside the try so the finally can release it however this method exits -
            // including the early returns for a missing start node and the two catch blocks.
            PreparedWorkflow prepared = null;

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

                // Steps 3+4: take EXCLUSIVE ownership of a compiled+parsed workflow. Rented rather
                // than shared because the activity instances carry per-execution state - see the
                // _workflowPool comment. Released in the finally at the end of this method.
                Dev2Logger.Debug("WorkflowExecutor Step 3: Renting prepared workflow (compile+parse)", executionId.ToString());
                prepared = RentPreparedWorkflow(request.WorkflowFilePath, xamlDefinition);
                Dev2Logger.Debug("WorkflowExecutor Step 3 completed: Prepared workflow rented", executionId.ToString());

                if (prepared?.Activity == null)
                {
                    Dev2Logger.Error("WorkflowExecutor Execute: Failed to load DynamicActivity from XAML", executionId.ToString());
                    return WorkflowExecutionResult.Failure("Failed to load DynamicActivity from XAML.");
                }

                var startActivity = prepared.StartActivity;
                Dev2Logger.Debug("WorkflowExecutor Step 4 completed: IDev2Activity chain available", executionId.ToString());

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

                // Per-execution usage telemetry (8438 / 8501) — record the execution facts
                // AFTER the result is built so building the response is never affected.
                // The actual publish (and its full-request timing) happens in
                // UsagePublishMiddleware, registered first in the pipeline; this only
                // hands off the workflow-specific payload via the ambient context.
                UsagePublishContext.Current = new UsagePublishContext
                {
                    WorkflowName = resolvedName,
                    ExecutionId  = executionId,
                    IsSuccess    = result.IsSuccess,
                    ErrorCount   = result.Errors.Count
                };

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
                var errorMessage = start > 0 ? GlobalConstants.NoStartNodeError : iwe.Message;
                UsagePublishContext.Current = new UsagePublishContext
                {
                    WorkflowName = Path.GetFileNameWithoutExtension(request.WorkflowFilePath) ?? string.Empty,
                    ExecutionId  = executionId,
                    IsSuccess    = false,
                    ErrorCount   = 1
                };
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
            catch (OutOfMemoryException oom)
            {
                // TRANSIENT, not a workflow/business failure: this is the documented
                // Consumption-plan cold-start memory-pressure signature (compile-time
                // allocation failure, e.g. inside Roslyn/PEReader) — see the _workflowPool
                // comment above and docs/ShovelBridge-Architecture.md. Flagging
                // IsTransientFailure lets a broker-driven caller (ServiceBusWorkflowTriggerFunction)
                // retry instead of treating this as terminal and dead-lettering immediately;
                // retrying will very likely succeed once the instance has warmed up or scaled
                // out. HTTP callers ignore the flag and see the same failure response as before.
                stopwatch.Stop();
                Dev2Logger.Error($"WorkflowExecutor Execute: OutOfMemoryException (transient) for workflow: {request.WorkflowFilePath}", oom, executionId.ToString());
                _executionLogger.LogError(nameof(Execute), oom, executionId);
                UsagePublishContext.Current = new UsagePublishContext
                {
                    WorkflowName = Path.GetFileNameWithoutExtension(request.WorkflowFilePath) ?? string.Empty,
                    ExecutionId  = executionId,
                    IsSuccess    = false,
                    ErrorCount   = 1
                };
                return BuildTransientFailureResult(oom, executionId, startTime, stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                // Full exception (message + stack trace) goes to the server-side logs only -
                // Dev2Logger/_executionLogger both receive the whole `ex` object below. The
                // CALLER-facing Errors list must never include ex.StackTrace: it leaks internal
                // implementation detail (local file paths, type/member names) to whoever is on
                // the other end of create_workflow/execute_workflow/invoke_workflow. See
                // consolidated MCP testing findings, "Supporting bugs" - an unhandled-looking
                // ArgumentNullException("source") surfaced a raw stack trace including a local
                // dev machine path.
                Dev2Logger.Error($"WorkflowExecutor Execute: Unexpected exception for workflow: {request.WorkflowFilePath}", ex, executionId.ToString());
                _executionLogger.LogError(nameof(Execute), ex, executionId);
                UsagePublishContext.Current = new UsagePublishContext
                {
                    WorkflowName = Path.GetFileNameWithoutExtension(request.WorkflowFilePath) ?? string.Empty,
                    ExecutionId  = executionId,
                    IsSuccess    = false,
                    ErrorCount   = 1
                };
                return new WorkflowExecutionResult
                {
                    IsSuccess = false,
                    ExecutionId = executionId,
                    Errors = new List<string> { $"Workflow execution failed: {ex.Message}" },
                    StartTime = startTime,
                    EndTime = DateTime.UtcNow,
                    Duration = stopwatch.Elapsed
                };
            }
            finally
            {
                // Released here rather than after ExecuteActivityChain so that a workflow which
                // throws mid-chain still returns its instance to the pool. Returning an instance
                // that failed is safe: the activity state it carries is overwritten by the next
                // execution's BeforeExecutionStart, which is exactly what the previous shared-cache
                // design relied on for every sequential execution.
                ReturnPreparedWorkflow(request.WorkflowFilePath, prepared);
            }
        }

        /// <summary>
        /// Builds the <see cref="WorkflowExecutionResult"/> for the <see cref="OutOfMemoryException"/>
        /// catch clause in <see cref="Execute(WorkflowExecutionRequest)"/>. Extracted as a small,
        /// pure, internal (<c>InternalsVisibleTo</c> the test project) helper so unit tests can verify
        /// the transient-failure shape (<see cref="WorkflowExecutionResult.IsTransientFailure"/> = true,
        /// error message preserved) deterministically, without needing to force a real
        /// <see cref="OutOfMemoryException"/> by exhausting process memory.
        /// </summary>
        internal static WorkflowExecutionResult BuildTransientFailureResult(OutOfMemoryException oom, Guid executionId, DateTime startTime, TimeSpan elapsed) =>
            new()
            {
                IsSuccess = false,
                IsTransientFailure = true,
                ExecutionId = executionId,
                Errors = new List<string> { oom.Message },
                StartTime = startTime,
                EndTime = DateTime.UtcNow,
                Duration = elapsed
            };

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
        /// Takes exclusive ownership of a compiled+parsed workflow for <paramref name="filePath"/>,
        /// reusing a previously returned one when available and compiling a new one otherwise.
        ///
        /// <para>The caller MUST pass the result to <see cref="ReturnPreparedWorkflow"/> in a
        /// <c>finally</c>. Failing to return one is not a correctness bug - the next execution
        /// simply compiles another - but it forfeits the reuse that makes this cheap.</para>
        /// </summary>
        /// <returns>
        /// <c>null</c> when the XAML does not yield a <see cref="DynamicActivity"/>, matching the
        /// previous contract so the caller's existing null handling is unchanged.
        /// </returns>
        /// <summary>
        /// Builds the pool key for <paramref name="filePath"/>: its full path PLUS a content
        /// discriminator (last-write timestamp + length).
        ///
        /// <para>
        /// The key used to be the path alone, which meant a pooled instance was reused for a file
        /// that had since changed on disk - <c>RentPreparedWorkflow</c> returns the pooled instance
        /// before it ever looks at the freshly-read <c>xamlDefinition</c>. So after
        /// <c>edit_workflow</c> rewrote a .bite, every later execution silently replayed the
        /// PRE-EDIT compilation until the process restarted: the tool reported success, the file
        /// genuinely changed, and behaviour did not. Reproduced on warewolfserver-mcp 2026-08-21 -
        /// get_workflow_definition showed the edited description while execute_workflow kept
        /// returning the old output.
        /// </para>
        ///
        /// <para>
        /// Including the file's identity in the key means a changed definition simply lands in a
        /// different bucket, so a stale instance can never be handed out - no cache-invalidation
        /// call is required at the write site, and out-of-band edits are covered too. MCP writes
        /// additionally call <see cref="EvictWorkflow"/>, which closes the one gap this cannot see:
        /// two edits of identical length landing within the filesystem's timestamp granularity.
        /// </para>
        /// </summary>
        static string BuildPoolKey(string filePath)
        {
            var fullPath = Path.GetFullPath(filePath);
            try
            {
                var info = new FileInfo(fullPath);
                if (info.Exists)
                {
                    return $"{fullPath}|{info.LastWriteTimeUtc.Ticks}|{info.Length}";
                }
            }
            catch (Exception ex)
            {
                // A stat failure must never fail an execution - fall back to the path-only key,
                // which is exactly the pre-fix behaviour rather than a new failure mode.
                Dev2Logger.Warn($"WorkflowExecutor BuildPoolKey could not stat '{fullPath}': {ex.Message}", "WorkflowExecutor-Pool");
            }

            return fullPath;
        }

        /// <summary>
        /// Drops every pooled instance for <paramref name="filePath"/>, whatever version they were
        /// compiled from. Called by the MCP write tools after a successful save so the very next
        /// execution recompiles, even when the rewrite is indistinguishable by timestamp+length.
        /// Safe to call for a path that was never pooled.
        /// </summary>
        internal static void EvictWorkflow(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            string fullPath;
            try
            {
                fullPath = Path.GetFullPath(filePath);
            }
            catch
            {
                return;
            }

            var prefix = fullPath + "|";
            foreach (var key in _workflowPool.Keys)
            {
                if (string.Equals(key, fullPath, StringComparison.OrdinalIgnoreCase) ||
                    key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    _workflowPool.TryRemove(key, out _);
                }
            }
        }

        internal static PreparedWorkflow RentPreparedWorkflow(string filePath, StringBuilder xamlDefinition)
        {
            var key = BuildPoolKey(filePath);

            if (_workflowPool.TryGetValue(key, out var available) && available.TryTake(out var reused))
            {
                reused.PoolKey = key;
                return reused;
            }

            var activity = LoadDynamicActivity(xamlDefinition);
            if (activity == null)
            {
                return null;
            }

            // Parsed once per compiled instance rather than once per execution: Parse() only walks
            // the activity's own object graph, so the chain it returns belongs to this instance and
            // is as exclusively owned as the instance itself.
            return new PreparedWorkflow
            {
                Activity      = activity,
                StartActivity = new ActivityParser().Parse(activity),
                PoolKey       = key
            };
        }

        /// <summary>
        /// Releases a rented workflow back for reuse. Null-tolerant so callers can return
        /// unconditionally from a <c>finally</c> without first testing whether the rent succeeded.
        /// </summary>
        internal static void ReturnPreparedWorkflow(string filePath, PreparedWorkflow? prepared)
        {
            if (prepared?.Activity == null || string.IsNullOrEmpty(filePath))
            {
                return;
            }

            // Prefer the key stamped at rent time: recomputing from the file here would file an
            // instance compiled from the OLD definition under the NEW definition's key whenever the
            // .bite changed mid-execution.
            var key = string.IsNullOrEmpty(prepared.PoolKey) ? BuildPoolKey(filePath) : prepared.PoolKey;
            var bag = _workflowPool.GetOrAdd(key, _ => new ConcurrentBag<PreparedWorkflow>());

            // BOUNDED. Beyond the cap the instance is simply not retained - it becomes garbage and
            // the next rent compiles a fresh one. Renting is deliberately NOT throttled, so the
            // exclusivity guarantee is untouched; only how much is KEPT is capped.
            //
            // Why this matters: a compiled workflow tree measured ~32 MB (engine working set rose
            // 396 MB -> 712 MB across 10 concurrent executions, 2026-08-12). An unbounded pool grows
            // to peak concurrency and never shrinks, so a single burst permanently raises the
            // floor. On an Azure Functions Consumption instance (~1.5 GB) that reached exhaustion:
            //   "Insufficient memory to continue the execution of the program."
            //     at System.Reflection.PortableExecutable.PEReader..ctor(...)
            // - i.e. the XAML compile itself failed for lack of memory, returning HTTP 500.
            //
            // Set the cap at or above expected peak concurrency and the reuse rate is unchanged;
            // set it below and the excess simply recompiles, trading CPU for a hard memory ceiling.
            if (bag.Count >= MaxPooledPerWorkflow)
            {
                return;
            }

            bag.Add(prepared);
        }

        /// <summary>
        /// Maximum prepared workflows RETAINED per workflow path. Production (DI-constructed)
        /// instances source this from <see cref="HostEnvironmentConfig.WorkflowPoolMax"/> (deploy
        /// file only — no env var, WOLF-8516) via the constructor; without a config (e.g. the many
        /// <c>new WorkflowExecutor(logger)</c> test call sites) the hardcoded default below is used
        /// instead. Values below 1 are ignored in favour of the default either way.
        /// </summary>
        /// <remarks>
        /// 8 by default: comfortably above the per-replica concurrency the queue path generates
        /// (<c>WORKER__MAXCONCURRENCY</c> is 1, so one replica issues one request at a time), while
        /// bounding retained memory to roughly 8 x the compiled tree size per distinct workflow.
        /// </remarks>
        internal static int MaxPooledPerWorkflow { get; private set; } = DefaultPoolCap;

        const int DefaultPoolCap = 8;

        /// <summary>
        /// Discards every pooled workflow. Test hook only - lets a test observe compilation
        /// behaviour from a known-empty state without depending on execution order.
        /// </summary>
        internal static void ClearWorkflowPool() => _workflowPool.Clear();

        /// <summary>Pooled (idle) instance count for <paramref name="filePath"/>. Test hook only.</summary>
        internal static int PooledWorkflowCount(string filePath)
        {
            // Sums every key belonging to this workflow, not just one. The pool key gained a
            // content discriminator (see BuildPoolKey), so a single .bite can legitimately own
            // several buckets - one per file version still holding instances. Callers ask "how
            // many compilations of THIS workflow are pooled", which is the total.
            var fullPath = Path.GetFullPath(filePath);
            var prefix = fullPath + "|";
            var total = 0;
            foreach (var pair in _workflowPool)
            {
                if (string.Equals(pair.Key, fullPath, StringComparison.OrdinalIgnoreCase) ||
                    pair.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    total += pair.Value.Count;
                }
            }

            return total;
        }

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
            var rawPayload = ResolveInputPayload(request);
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

            // Gate on the PAYLOAD, not on InputParameters.Count: a raw body (flat JSON or XML)
            // carries inputs without ever populating InputParameters, and gating on the dictionary
            // meant such a body was never handed to the environment at all.
            if (!string.IsNullOrEmpty(dataList) && !string.IsNullOrWhiteSpace(rawPayload))
            {
                ExecutionEnvironmentUtils.UpdateEnvironmentFromInputPayload(
                    dataObject,
                    dataObject.RawPayload,
                    dataList);
            }

            return dataObject;
        }

        /// <summary>
        /// Chooses the payload handed to <c>UpdateEnvironmentFromInputPayload</c>, preferring the
        /// caller's ORIGINAL body over one re-synthesised from <see cref="WorkflowExecutionRequest.InputParameters"/>.
        /// </summary>
        /// <remarks>
        /// Parity with Dev2.Runtime.WebServer, which passes <c>WebRequestTO.RawRequestPayload</c>
        /// straight through. Re-synthesising from a Dictionary&lt;string,string&gt; silently dropped
        /// flat bodies, XML bodies and nested/recordset inputs - see
        /// <see cref="WorkflowExecutionRequest.RawInputPayload"/>.
        ///
        /// Query-string inputs are merged in when the raw body is a JSON object, and the BODY WINS
        /// on a name clash - the same precedence as before, where the body was parsed after the
        /// query string and overwrote it. An XML body is used as-is, because merging query values
        /// into arbitrary XML would require guessing its shape.
        /// </remarks>
        internal static string ResolveInputPayload(WorkflowExecutionRequest request)
        {
            var raw = request.RawInputPayload;
            var hasQueryInputs = request.InputParameters is { Count: > 0 };

            if (string.IsNullOrWhiteSpace(raw))
            {
                return BuildJsonPayload(request.InputParameters);
            }

            if (!hasQueryInputs)
            {
                return raw;
            }

            if (raw.TrimStart().StartsWith("{", StringComparison.Ordinal))
            {
                try
                {
                    var merged = JObject.Parse(raw);
                    foreach (var kv in request.InputParameters)
                    {
                        if (merged[kv.Key] == null)
                        {
                            merged[kv.Key] = kv.Value;
                        }
                    }
                    return merged.ToString(Formatting.None);
                }
                catch
                {
                    // Not parseable after all - fall through and use the body untouched.
                }
            }

            return raw;
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

                // Nested sub-workflow invocation nodes (e.g. "Hello World" called from a
                // continuation) default to the Server's legacy Windows-groups authorization
                // (ServerAuthorizationService), which the Lightweight engine has no secure.config
                // to satisfy. The caller was already authorized at the HTTP/claims layer before
                // execution began, so hand nested invocations a permissive service instead.
                if (current is Unlimited.Applications.BusinessDesignStudio.Activities.DsfActivity dsfActivity)
                {
                    dsfActivity.AuthorizationService = Security.LightweightAuthorizationService.Instance;
                }

                // A sub-workflow invoke may also be nested inside a composite/container
                // activity (DsfSequenceActivity, DsfForEachActivity, GateActivity, etc.) whose
                // own Execute() iterates its children internally, never surfacing them through
                // this flat `next`-chain loop. Walk GetChildrenNodes() recursively so every
                // nested DsfActivity gets the same permissive patch before `current` executes.
                PatchNestedAuthorizationServices(current, new HashSet<string>());

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
        /// Recursively walks <paramref name="node"/>'s <see cref="IDev2Activity.GetChildrenNodes"/>
        /// tree, patching every nested <c>DsfActivity</c> (sub-workflow invoke) onto the permissive
        /// <see cref="Security.LightweightAuthorizationService"/> — mirroring the flat top-level
        /// patch in <see cref="ExecuteActivityChain"/> for nodes reachable only through a
        /// composite/container activity's own internal iteration (Sequence, ForEach, Gate,
        /// ManualResumption, RedisCache, SelectAndApply, etc.), which never surface through the
        /// outer `next`-chain loop. <paramref name="visited"/> is keyed on <see cref="IDev2Activity.UniqueID"/>
        /// to guard against re-processing a node twice and against any cyclic/self-referencing
        /// GetChildrenNodes() implementation causing unbounded recursion.
        /// </summary>
        static void PatchNestedAuthorizationServices(IDev2Activity node, HashSet<string> visited)
        {
            if (node == null || !visited.Add(node.UniqueID))
            {
                return;
            }

            var children = node.GetChildrenNodes();
            if (children == null)
            {
                return;
            }

            foreach (var child in children)
            {
                if (child == null)
                {
                    continue;
                }

                if (child is Unlimited.Applications.BusinessDesignStudio.Activities.DsfActivity childDsfActivity)
                {
                    childDsfActivity.AuthorizationService = Security.LightweightAuthorizationService.Instance;
                }

                PatchNestedAuthorizationServices(child, visited);
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
                // The message is NOT logged: ExecutionException is constructed from
                // environment.FetchErrors() (see ExecuteActivityChain), so it IS the workflow's
                // evaluated variable values. executionId correlates this to the completion line,
                // which already carries the error count.
                _executionLogger.LogError("ExecuteActivityChain failed.", executionId);
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
