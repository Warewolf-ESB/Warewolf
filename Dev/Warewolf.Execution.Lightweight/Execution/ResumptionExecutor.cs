/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using Dev2;
using Dev2.Common;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Interfaces;
using Hangfire;
using Hangfire.States;
using System.Diagnostics;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Xml.Linq;
using Warewolf.Driver.Persistence;
using Warewolf.Execution.Lightweight.Logging;
using Warewolf.Storage;
using Dev2JsonSerializer = Dev2.Common.Serializers.Dev2JsonSerializer;

namespace Warewolf.Execution.Lightweight
{
    /// <summary>Claim outcome of <see cref="ResumptionExecutor.TryClaim"/>.</summary>
    internal enum ResumeClaimOutcome
    {
        /// <summary>No job with that id exists in the store.</summary>
        NotFound,

        /// <summary>The job was not in <c>Scheduled</c> state — another dispatch won, it already ran, or it was manually resumed.</summary>
        Conflict,

        /// <summary>This caller atomically transitioned the job Scheduled → Processing and owns its execution.</summary>
        Claimed,
    }

    internal sealed record ResumeClaimResult(ResumeClaimOutcome Outcome, string? CurrentState);

    internal sealed record ResumeExecutionResult(bool Success, string? Error, string? Outputs, long DurationMilliseconds);

    /// <summary>
    /// Executes a suspended workflow's continuation on the engine's lightweight pipeline —
    /// the engine-side replacement for the Server's <c>WorkflowResume</c> management
    /// endpoint + <c>ResumableExecutionContainer</c>.
    ///
    /// Serves all three resume paths through ONE code path (architecture §4):
    /// <list type="bullet">
    ///   <item><b>Scheduled</b> — <c>WorkflowResumeFunction</c> (invoked by the
    ///         ExecutionEngineJobProcessor or a role-gated human caller):
    ///         <see cref="TryClaim"/> (atomic CAS Scheduled→Processing — the system's
    ///         single duplicate-prevention point) then <see cref="ExecuteClaimed"/>
    ///         (execute + record <c>Succeeded</c>/<c>Failed</c>).</item>
    ///   <item><b>Manual, no-override</b> — <see cref="Execute"/> via the
    ///         <see cref="IResumptionExecutor"/> seam in <c>HangfireScheduler.ResumeJob</c>;
    ///         synchronous, returns a serialized <c>ExecuteMessage</c> exactly like
    ///         <c>WorkflowResume.Execute</c>.</item>
    ///   <item><b>Manual, override</b> — <see cref="ExecuteOverrideContinuation"/> via the
    ///         seam in <c>HangfireScheduler.ManualResumeWithOverrideJob</c>; runs against
    ///         the caller's merged environment from <c>StartActivityId</c>.</item>
    /// </list>
    ///
    /// Continuation semantics mirror <c>ResumableExecutionContainer.EvalInner/FindActivity</c>:
    /// parse the .bite, flatten the activity graph, SKIP to the node whose
    /// <c>UniqueID == startActivityId</c>, and walk from there with the restored environment.
    /// </summary>
    public sealed class ResumptionExecutor : IResumptionExecutor
    {
        readonly IExecutionLogger _executionLogger;
        readonly Lazy<JobStorage> _jobStorage;
        readonly Lazy<IBackgroundJobClient> _client;
        readonly string _workflowsDirectory;

        public ResumptionExecutor(IExecutionLogger executionLogger)
        {
            _executionLogger = executionLogger ?? throw new ArgumentNullException(nameof(executionLogger));
            _jobStorage = new Lazy<JobStorage>(HangfireStorageFactory.BuildFromPersistenceConfig, LazyThreadSafetyMode.ExecutionAndPublication);
            _client = new Lazy<IBackgroundJobClient>(() => new BackgroundJobClient(_jobStorage.Value), LazyThreadSafetyMode.ExecutionAndPublication);
            _workflowsDirectory = Environment.GetEnvironmentVariable("WorkflowsDirectory")
                ?? Path.Combine(AppContext.BaseDirectory, "Resources");
        }

        /// <summary>Test seam.</summary>
        internal ResumptionExecutor(IExecutionLogger executionLogger, JobStorage jobStorage, IBackgroundJobClient client, string workflowsDirectory)
        {
            _executionLogger = executionLogger;
            _jobStorage = new Lazy<JobStorage>(() => jobStorage);
            _client = new Lazy<IBackgroundJobClient>(() => client);
            _workflowsDirectory = workflowsDirectory;
        }

        // ── Scheduled path (resume route) ─────────────────────────────────────────

        /// <summary>
        /// Atomic claim: CAS the job Scheduled → Processing. Exactly one caller wins;
        /// everyone else gets <see cref="ResumeClaimOutcome.Conflict"/> (409 upstream).
        /// </summary>
        internal ResumeClaimResult TryClaim(string suspensionId)
        {
            var jobDetails = _jobStorage.Value.GetMonitoringApi().JobDetails(suspensionId);
            if (jobDetails is null)
            {
                return new ResumeClaimResult(ResumeClaimOutcome.NotFound, null);
            }

            var claimed = _client.Value.ChangeState(
                suspensionId,
                new ExternalProcessingState(
                    serverId: $"execution-engine:{Environment.MachineName}",
                    workerId: Guid.NewGuid().ToString("N")),
                ScheduledState.StateName);

            if (claimed)
            {
                _executionLogger.LogInfo($"Resume | JobId={suspensionId} | Claimed (Scheduled→Processing).", Guid.Empty);
                return new ResumeClaimResult(ResumeClaimOutcome.Claimed, ProcessingState.StateName);
            }

            var currentState = _jobStorage.Value.GetMonitoringApi().JobDetails(suspensionId)?
                .History?.OrderBy(s => s.CreatedAt).LastOrDefault()?.StateName;
            return new ResumeClaimResult(ResumeClaimOutcome.Conflict, currentState);
        }

        /// <summary>
        /// Executes a job this caller has claimed and records the terminal state:
        /// <c>Succeeded</c> on success, <c>Failed</c> (fail-only, never auto-retried)
        /// on any error — both via CAS expecting <c>Processing</c>.
        /// </summary>
        internal ResumeExecutionResult ExecuteClaimed(string suspensionId)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var values = ReadJobValues(suspensionId);
                var outputs = RunContinuationFromValues(values, decryptValues: true);

                stopwatch.Stop();
                _client.Value.ChangeState(
                    suspensionId,
                    new ExternalSucceededState(latencyMilliseconds: 0, performanceDurationMilliseconds: stopwatch.ElapsedMilliseconds),
                    ProcessingState.StateName);

                _executionLogger.LogInfo(
                    $"Resume | JobId={suspensionId} | Succeeded | DurationMs={stopwatch.ElapsedMilliseconds}", Guid.Empty);
                return new ResumeExecutionResult(true, null, outputs, stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _client.Value.ChangeState(
                    suspensionId,
                    new FailedState(ex) { Reason = "Resume execution failed on the Execution Engine" },
                    ProcessingState.StateName);

                // ex.Message here is workflow data by construction: RunContinuation throws with
                // string.Join(NewLine, errors) built from Environment.Errors / AllErrors — i.e.
                // evaluated variable values — and other throw sites embed absolute paths.
                // It therefore reaches neither the log nor the response: only the exception type
                // is logged (the exception object is never passed, since the sinks persist
                // ex.ToString()), and the returned Error string is generic. The failure is
                // logged exactly once; suspensionId is the correlator. Hangfire retains the
                // full exception on the job's FailedState above for diagnostics.
                _executionLogger.LogError($"Resume operation failed. JobId={suspensionId}: {ex.Message}", Guid.Empty);
                return new ResumeExecutionResult(false, "Resume execution failed due to an unexpected error.", null, stopwatch.ElapsedMilliseconds);
            }
        }

        // ── IResumptionExecutor (manual paths, invoked through the driver seam) ──

        /// <inheritdoc/>
        public StringBuilder Execute(Dictionary<string, StringBuilder> values)
        {
            var serializer = new Dev2JsonSerializer();
            try
            {
                // ResumeJob decrypted "environment" / "currentuserprincipal" before the seam.
                _ = RunContinuationFromValues(values, decryptValues: false);
                return serializer.SerializeToBuilder(new ExecuteMessage
                {
                    HasError = false,
                    Message = new StringBuilder("Execution Completed."),
                });
            }
            catch (Exception ex)
            {
                return serializer.SerializeToBuilder(new ExecuteMessage
                {
                    HasError = true,
                    Message = new StringBuilder(ex.Message),
                });
            }
        }

        /// <inheritdoc/>
        public void ExecuteOverrideContinuation(IDSFDataObject dsfDataObject, Dictionary<string, StringBuilder> values)
        {
            var startActivityId = dsfDataObject.StartActivityId;
            if (startActivityId == Guid.Empty)
            {
                return;
            }

            values.TryGetValue("resourceID", out var resourceIdValue);
            Guid.TryParse(resourceIdValue?.ToString(), out var resourceId);

            var filePath = ResolveWorkflowFile(values, resourceId);

            // Mirror ResumableExecutionContainer.EvalInner: reset StartActivityId before
            // executing so nested logic never re-triggers a resumption.
            dsfDataObject.StartActivityId = Guid.Empty;

            RunContinuation(filePath, startActivityId, dsfDataObject, throwOnEnvironmentErrors: true);
        }

        // ── Continuation core ─────────────────────────────────────────────────────

        Dictionary<string, StringBuilder> ReadJobValues(string suspensionId)
        {
            var jobDetails = _jobStorage.Value.GetMonitoringApi().JobDetails(suspensionId)
                             ?? throw new InvalidOperationException($"Job '{suspensionId}' no longer exists in the persistence store.");

            return jobDetails.Job.Args[0] as Dictionary<string, StringBuilder>
                   ?? throw new InvalidOperationException($"Job '{suspensionId}' does not carry Warewolf suspension values.");
        }

        /// <summary>
        /// Builds a fresh execution context from the persisted job values and runs the
        /// continuation. Value handling mirrors the Server (<c>WorkflowResume</c>):
        /// DPAPI/AES decrypt (when <paramref name="decryptValues"/>), URL-decode the
        /// environment, rebuild a <see cref="GenericPrincipal"/> from the persisted name.
        /// </summary>
        string RunContinuationFromValues(Dictionary<string, StringBuilder> values, bool decryptValues)
        {
            values.TryGetValue("resourceID", out var resourceIdValue);
            values.TryGetValue("environment", out var environmentValue);
            values.TryGetValue("startActivityId", out var startActivityIdValue);
            values.TryGetValue("versionNumber", out var versionNumberValue);
            values.TryGetValue("currentuserprincipal", out var principalValue);

            if (!Guid.TryParse(startActivityIdValue?.ToString(), out var startActivityId) || startActivityId == Guid.Empty)
            {
                throw new InvalidOperationException("Persisted job has no valid startActivityId.");
            }

            Guid.TryParse(resourceIdValue?.ToString(), out var resourceId);
            int.TryParse(versionNumberValue?.ToString(), out var versionNumber);

            var environmentJson = environmentValue?.ToString() ?? string.Empty;
            var principalName = principalValue?.ToString() ?? string.Empty;
            if (decryptValues)
            {
                environmentJson = Warewolf.Security.Encryption.DpapiWrapper.DecryptIfEncrypted(environmentJson);
                principalName = Warewolf.Security.Encryption.DpapiWrapper.DecryptIfEncrypted(principalName);
            }

            var filePath = ResolveWorkflowFile(values, resourceId);
            var (fileResourceId, fileVersion) = WorkflowExecutor.ExtractResourceIdentity(WorkflowExecutor.ReadWorkflowFile(filePath));
            if (versionNumber > 0 && fileVersion != versionNumber)
            {
                Dev2Logger.Warn(
                    $"Resume version mismatch: job was suspended at workflow version {versionNumber} but the deployed " +
                    $"'{Path.GetFileNameWithoutExtension(filePath)}' is version {fileVersion}. Resuming against the deployed version " +
                    "(the engine has no version catalog).", "ResumptionExecutor");
            }

            // Server parity (WorkflowResume): environment is URL-decoded before FromJson.
            var environment = new ExecutionEnvironment();
            if (!string.IsNullOrWhiteSpace(environmentJson))
            {
                environment.FromJson(HttpUtility.UrlDecode(environmentJson));
            }

            var dataObject = new DsfDataObject(string.Empty, Guid.NewGuid())
            {
                ResourceID = resourceId != Guid.Empty ? resourceId : fileResourceId,
                Environment = environment,
                VersionNumber = versionNumber > 0 ? versionNumber : fileVersion,
                ExecutingUser = BuildPrincipal(principalName),
                ExecutionID = Guid.NewGuid(),
                ExecutionToken = new LightweightExecutionToken(),
                EsbChannel = new LightweightEsbChannel(_workflowsDirectory),
            };

            return RunContinuation(filePath, startActivityId, dataObject, throwOnEnvironmentErrors: true);
        }

        /// <summary>
        /// The shared chain-runner: parse the .bite, flatten the graph, skip to the node
        /// with <paramref name="startActivityId"/>, execute with the supplied data object,
        /// and return the DataList-shaped JSON outputs.
        /// </summary>
        string RunContinuation(string filePath, Guid startActivityId, IDSFDataObject dataObject, bool throwOnEnvironmentErrors)
        {
            var fileContents = WorkflowExecutor.ReadWorkflowFile(filePath);
            var (xamlDefinition, dataList, workflowName) = WorkflowExecutor.ExtractWorkflowParts(fileContents);
            if (xamlDefinition == null || xamlDefinition.Length == 0)
            {
                throw new InvalidOperationException($"No XamlDefinition found in '{filePath}'.");
            }

            var dynamicActivity = WorkflowExecutor.GetOrLoadDynamicActivity(filePath, xamlDefinition)
                                  ?? throw new InvalidOperationException($"Failed to load DynamicActivity from '{filePath}'.");

            var parser = new Dev2.Activities.ActivityParser();
            var startActivity = parser.Parse(dynamicActivity)
                                ?? throw new InvalidOperationException(GlobalConstants.NoStartNodeError);

            // Mirror ResumableExecutionContainer.FindActivity: flatten and locate by UniqueID.
            var resumeNode = parser.ParseToLinkedFlatList(startActivity)
                .FirstOrDefault(a => a.UniqueID == startActivityId.ToString());
            if (resumeNode is null)
            {
                throw new InvalidOperationException($"Resume Node not found. UniqueID:{startActivityId}");
            }

            if (string.IsNullOrEmpty(dataObject.ServiceName))
            {
                dataObject.ServiceName = workflowName ?? Path.GetFileNameWithoutExtension(filePath);
            }

            LightweightSourceLoader.Instance.EnsureIndexed(_workflowsDirectory);

            WorkflowExecutor.ExecuteActivityChain(dataObject, resumeNode);

            var errors = dataObject.Environment.Errors
                .Concat(dataObject.Environment.AllErrors)
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .Distinct()
                .ToList();
            if (dataObject.ExecutionException != null && errors.Count == 0)
            {
                errors.Add(dataObject.ExecutionException.Message);
            }

            if (errors.Count > 0 && throwOnEnvironmentErrors)
            {
                throw new InvalidOperationException(string.Join(Environment.NewLine, errors));
            }

            return string.IsNullOrEmpty(dataList)
                ? dataObject.Environment.ToJson()
                : ExecutionEnvironmentUtils.GetJsonOutputFromEnvironment(dataObject, dataList, 0);
        }

        /// <summary>
        /// Resolves the suspended workflow's .bite: the engine-stamped
        /// <c>engineWorkflowFilePath</c> when it still exists (fast path), otherwise a
        /// resource-ID scan of the workflows directory (covers Server-created jobs and
        /// re-deployed packages where the absolute path changed).
        /// </summary>
        internal string ResolveWorkflowFile(Dictionary<string, StringBuilder> values, Guid resourceId)
        {
            if (values.TryGetValue(LightweightJobValuesEnricher.WorkflowFilePathKey, out var stampedPath))
            {
                var candidate = stampedPath.ToString();
                if (!string.IsNullOrWhiteSpace(candidate) && File.Exists(candidate))
                {
                    return candidate;
                }
            }

            if (resourceId != Guid.Empty && Directory.Exists(_workflowsDirectory))
            {
                var match = Directory
                    .EnumerateFiles(_workflowsDirectory, "*.bite", SearchOption.AllDirectories)
                    .FirstOrDefault(f => HasResourceId(f, resourceId));
                if (match != null)
                {
                    return match;
                }
            }

            throw new InvalidOperationException(
                $"Could not resolve the suspended workflow (.bite) for resource '{resourceId}' " +
                $"under '{_workflowsDirectory}'. The workflow may have been removed from the deployment.");
        }

        static bool HasResourceId(string filePath, Guid resourceId)
        {
            try
            {
                return string.Equals(
                    XElement.Load(filePath).Attribute("ID")?.Value,
                    resourceId.ToString(),
                    StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        // Parity with HangfireScheduler.BuildClaimsPrincipal / WorkflowResume.
        static IPrincipal BuildPrincipal(string persistedName)
        {
            var name = string.IsNullOrWhiteSpace(persistedName) ? "Public" : persistedName;
            if (name.Contains('\\'))
            {
                name = name.Split('\\').Last().Trim();
            }

            return new GenericPrincipal(new GenericIdentity(name.Trim()), Array.Empty<string>());
        }
    }
}
