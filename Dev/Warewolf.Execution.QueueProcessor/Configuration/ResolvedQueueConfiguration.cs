/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using Dev2.Common;
using Microsoft.Extensions.Options;

namespace Warewolf.Execution.QueueProcessor.Configuration
{
    /// <summary>
    /// The single trigger this replica serves, with its broker source resolved — the
    /// container's equivalent of <c>QueueWorker</c>'s <c>WorkerContext</c>
    /// (<c>WorkerContext.cs:78-139</c>), but built from staged files instead of a SignalR
    /// call to the Warewolf Server.
    /// </summary>
    public sealed class ResolvedQueueConfiguration
    {
        public required TriggerDefinition Trigger { get; init; }
        public required RabbitMqSourceOptions Source { get; init; }

        /// <summary>Dead-letter broker source. Usually the same source as <see cref="Source"/>.</summary>
        public required RabbitMqSourceOptions DeadLetterSource { get; init; }

        public string QueueName => Trigger.QueueName!;
        public string? DeadLetterQueueName => Trigger.DeadLetterQueue;
        public ushort Prefetch => Trigger.ResolvedPrefetch;

        /// <summary>
        /// Workflow path for the engine's <c>/Secure/{*name}</c> route. The Server stores
        /// folder-qualified names with <b>backslashes</b>
        /// (e.g. <c>ProfilerWrapper\Queue\MandateCollectionSuccessConsume</c>); on-prem this
        /// worked only because <c>System.Uri</c> normalises them. Per-segment escaping splits
        /// on '/', so the separators must be normalised here or each backslash would be
        /// percent-encoded to <c>%5C</c> and the route would not resolve (plan §2.8.8).
        /// </summary>
        public string WorkflowPath => (Trigger.WorkflowName ?? string.Empty)
            .Replace('\\', '/')
            .Trim('/');

        /// <summary>True when the queue must be declared durable (from the trigger's Options).</summary>
        public bool Durable => Trigger.OptionBool("Durable");
        public bool Exclusive => Trigger.OptionBool("Exclusive");
        public bool AutoDelete => Trigger.OptionBool("AutoDelete");

        public bool DeadLetterDurable => Trigger.DeadLetterOptionBool("Durable");

        public bool HasDeadLetter => !string.IsNullOrWhiteSpace(DeadLetterQueueName);
    }

    /// <summary>
    /// Cold-start loader: discovers the trigger file, decrypts + parses it, resolves the
    /// broker source(s), and logs a secret-free summary. Read-only — nothing is ever written
    /// back, so the container image/volume can stay immutable (the same discipline as the
    /// engine's persistence settings, Hangfire decisions #10/#11).
    /// </summary>
    public sealed class QueueConfigurationLoader
    {
        const string ExecutionId = "QueueProcessor-ConfigLoader";

        readonly QueueProcessorOptions _options;
        readonly TriggerBiteReader _reader;
        readonly RabbitMqSourceCatalog _sources;

        public QueueConfigurationLoader(
            IOptions<QueueProcessorOptions> options,
            TriggerBiteReader reader,
            RabbitMqSourceCatalog sources)
        {
            _options = (options ?? throw new ArgumentNullException(nameof(options))).Value;
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
            _sources = sources ?? throw new ArgumentNullException(nameof(sources));
        }

        /// <summary>
        /// Convenience overload for callers that do not hold a catalog — it builds one, which
        /// performs the startup scan. Kept so a test or a diagnostic tool can load a configuration
        /// from a folder in one call; the host always injects the shared singleton instead, so the
        /// scan happens exactly once per process.
        /// </summary>
        public QueueConfigurationLoader(IOptions<QueueProcessorOptions> options, TriggerBiteReader reader)
            : this(options, reader, new RabbitMqSourceCatalog(options))
        {
        }

        public ResolvedQueueConfiguration Load()
        {
            var triggersPath = _options.TriggersPath;
            Dev2Logger.Info(
                $"QueueConfigurationLoader loading triggers from '{triggersPath}' (filter " +
                $"'{_options.TriggerFilter}'); sources from '{_options.SourcesPath}'",
                ExecutionId);

            var files = TriggerBiteReader.Discover(triggersPath, _options.TriggerFilter);
            var trigger = SelectTrigger(files);

            if (trigger.Concurrency == 0)
            {
                // Parity with WorkerMonitor.cs:55-58, where Concurrency == 0 means the trigger
                // is not run at all. In ACA this state is normally expressed as min=max=0
                // replicas, so reaching it inside a running replica is a deployment mismatch.
                Dev2Logger.Warn(
                    $"Trigger '{trigger.Name}' has Concurrency = 0, which on-prem means DISABLED. " +
                    "This replica should not have been scheduled - check the app's scale settings.",
                    ExecutionId);
            }

            // NOTE - '@'-prefixed MapEntireMessage triggers.
            //
            // Such a trigger makes the forwarder post multipart/form-data (parity with
            // WarewolfWebRequestForwarder.cs:100-108). This used to be a hard startup failure,
            // because the Lightweight engine bound only the query string, a JSON body and an XML
            // body - so every message would have been dead-lettered while appearing to process
            // normally. WorkflowFunctionHelper.ParseMultipartAsync now binds multipart, matching
            // the full server including its Base64-for-typed-parts rule, so the shape is supported
            // and the guard has been removed.
            //
            // DEPLOYMENT DEPENDENCY: that support lives in the ENGINE, not here. A worker pointed
            // at an engine built before it will still dead-letter every message from such a
            // trigger. Verify the target engine binds multipart before deploying one.

            // Both come from the catalog cached at startup - no disk access per reference, and a
            // missing/invalid source has already failed the process before any message is taken.
            var source = _sources.Get(trigger.QueueSourceId, "queue");
            var deadLetterSource = trigger.QueueSinkId == trigger.QueueSourceId
                ? source
                : _sources.Get(trigger.QueueSinkId, "dead-letter");

            var resolved = new ResolvedQueueConfiguration
            {
                Trigger = trigger,
                Source = source,
                DeadLetterSource = deadLetterSource,
            };

            Dev2Logger.Info(
                "QueueConfigurationLoader resolved trigger " +
                $"'{trigger.Name}' ({trigger.TriggerId}): queue='{resolved.QueueName}', " +
                $"workflow='{resolved.WorkflowPath}', prefetch={resolved.Prefetch}, " +
                $"concurrency={trigger.Concurrency}, durable={resolved.Durable}, " +
                $"deadLetter='{resolved.DeadLetterQueueName}', mapEntireMessage={trigger.MapEntireMessage}, " +
                $"source={source.Describe()}, tls={source.UseSsl}",
                ExecutionId);

            // Optimum shape for this worker: Prefetch == MaxConcurrency. Dispatch is serial per
            // channel (measured in Phase 0 - ~2.2s apart, no overlap, at MaxConcurrency 1), so any
            // prefetch above the in-flight cap only PARKS messages in this replica's buffer where
            // no other replica can take them. That costs twice: it starves scale-out (KEDA sees the
            // messages as unavailable) and it widens the redelivery window, because a drain nacks
            // every buffered message back to the queue.
            if (resolved.Prefetch > _options.MaxConcurrency)
            {
                Dev2Logger.Warn(
                    $"Trigger '{trigger.Name}' sets Prefetch={resolved.Prefetch} but this replica's " +
                    $"in-flight cap is MaxConcurrency={_options.MaxConcurrency}, so " +
                    $"{resolved.Prefetch - _options.MaxConcurrency} message(s) will sit buffered in " +
                    "this replica instead of being available to other replicas - slowing scale-out " +
                    "and enlarging the redelivery window on scale-in. The optimum for a " +
                    "queue-per-app worker is Prefetch == MaxConcurrency (both 1 unless a workflow " +
                    "is measured safe to run concurrently).",
                    ExecutionId);
            }

            if (!source.UseSsl)
            {
                // Deliberate parity default (decision #25) - but never silently.
                Dev2Logger.Warn(
                    $"Broker connection for queue '{resolved.QueueName}' is NOT using TLS " +
                    $"({source.Describe()}). This matches PublishRabbitMQActivity's behaviour, but " +
                    "credentials and message bodies cross the network unencrypted. Production requires " +
                    "AMQPS - see the go-live gate in the migration plan.",
                    ExecutionId);
            }

            return resolved;
        }

        TriggerDefinition SelectTrigger(IReadOnlyList<string> files)
        {
            if (!string.IsNullOrWhiteSpace(_options.TriggerId))
            {
                if (!Guid.TryParse(_options.TriggerId, out var wanted))
                {
                    throw new TriggerConfigurationException(
                        $"Queue:TriggerId '{_options.TriggerId}' is not a GUID.");
                }

                foreach (var file in files)
                {
                    var candidate = _reader.Read(file);
                    if (candidate.TriggerId == wanted)
                    {
                        return candidate;
                    }
                }

                throw new TriggerConfigurationException(
                    $"No staged trigger file matched Queue:TriggerId '{wanted}'. " +
                    $"Searched {files.Count} file(s) under '{_options.TriggersPath}'.");
            }

            if (files.Count > 1)
            {
                throw new TriggerConfigurationException(
                    $"{files.Count} trigger files are staged in '{_options.TriggersPath}' but " +
                    "Queue:TriggerId is not set, so the active trigger is ambiguous. This app serves " +
                    "exactly one queue (decision #10): set Queue:TriggerId, or stage only that trigger's file. " +
                    $"Found: {string.Join(", ", files.Select(Path.GetFileName))}");
            }

            return _reader.Read(files[0]);
        }
    }
}
