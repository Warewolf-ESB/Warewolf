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
    /// The staged RabbitMQ sources, read <b>once at startup</b> and cached by
    /// <c>ID</c> — the container's read-only equivalent of the Server's resource catalogue.
    ///
    /// <para><b>Why a cache and not a lookup per call.</b> A trigger references a source twice
    /// (<c>QueueSourceId</c> for the work queue, <c>QueueSinkId</c> for the dead-letter queue), and
    /// resolving each one by scanning the folder meant re-reading and re-parsing every
    /// <c>.bite</c> per reference. Worse, it made the source files a <i>live</i> dependency: a
    /// reconnect or a dead-letter publish could re-read the disk long after startup, so a
    /// half-written or removed file would surface as a failure mid-message rather than at cold
    /// start. Reading everything up front means the replica either starts with a complete, valid
    /// broker configuration or fails immediately with a clear reason — the same fail-fast
    /// discipline as <c>ValidateOnStart</c>.</para>
    ///
    /// <para>Files are read from <see cref="QueueProcessorOptions.SourcesPath"/>
    /// (<c>Settings/sources/</c>) and then from the <see cref="QueueProcessorOptions.SettingsPath"/>
    /// root, which is the legacy flat layout kept for compatibility. Both naming conventions
    /// resolve: <c>{sourceId}.bite</c> as written by <c>Deploy-WwQueueProcessor.ps1</c>, and an
    /// operator-named Studio file such as <c>Warewolf DevOps RabbitMQ Source.bite</c>, because the
    /// index is keyed on the <c>ID</c> attribute inside the file rather than on its name.</para>
    ///
    /// Thread-safe: the index is built once in the constructor and never mutated, so the pump,
    /// the dead-letter publisher and any reconnect all read the same immutable snapshot.
    /// </summary>
    public sealed class RabbitMqSourceCatalog
    {
        const string ExecutionId = "QueueProcessor-SourceCatalog";

        readonly Dictionary<Guid, RabbitMqSourceOptions> _byId = new();
        readonly List<string> _searchedFolders = new();

        public RabbitMqSourceCatalog(IOptions<QueueProcessorOptions> options)
        {
            var opts = (options ?? throw new ArgumentNullException(nameof(options))).Value;
            Load(opts);
        }

        /// <summary>Number of RabbitMQ sources cached. Zero is legal only until a trigger asks for one.</summary>
        public int Count => _byId.Count;

        /// <summary>Folders actually scanned, in probe order — used in the not-found message.</summary>
        public IReadOnlyList<string> SearchedFolders => _searchedFolders;

        /// <summary>Cached source ids, for diagnostics.</summary>
        public IReadOnlyCollection<Guid> SourceIds => _byId.Keys;

        /// <summary>
        /// Returns the cached source for <paramref name="sourceId"/>, or throws naming every folder
        /// searched and every id that IS present — a bare "not found" sends operators hunting
        /// through an image layer.
        /// </summary>
        public RabbitMqSourceOptions Get(Guid sourceId, string role)
        {
            if (sourceId == Guid.Empty)
            {
                throw new TriggerConfigurationException(
                    $"The trigger has no {role} source id.");
            }

            if (_byId.TryGetValue(sourceId, out var source))
            {
                return source;
            }

            var available = _byId.Count == 0
                ? "none"
                : string.Join(", ", _byId.Keys.Select(k => k.ToString()));

            throw new TriggerConfigurationException(
                $"The {role} source '{sourceId}' is not staged. Searched: " +
                $"{string.Join(", ", _searchedFolders.Select(f => $"'{f}'"))}. " +
                $"Sources found: {available}. Stage it as '{sourceId}.bite' under the sources " +
                "folder - Deploy-WwQueueProcessor.ps1 does this automatically for every source a " +
                "staged trigger references (QueueSourceId AND QueueSinkId).");
        }

        public bool TryGet(Guid sourceId, out RabbitMqSourceOptions? source) =>
            _byId.TryGetValue(sourceId, out source);

        void Load(QueueProcessorOptions opts)
        {
            foreach (var folder in ProbeFolders(opts))
            {
                _searchedFolders.Add(folder);

                foreach (var file in Directory.EnumerateFiles(folder, "*.bite").OrderBy(f => f, StringComparer.Ordinal))
                {
                    RabbitMqSourceOptions parsed;
                    try
                    {
                        parsed = RabbitMqSourceOptions.FromBiteFile(file, opts.UseSsl);
                    }
                    catch (TriggerConfigurationException)
                    {
                        // Not a RabbitMQ source - the Elasticsearch source and trigger files both
                        // legitimately live in this tree. Skipped, not fatal.
                        continue;
                    }

                    if (parsed.SourceId == Guid.Empty)
                    {
                        Dev2Logger.Warn(
                            $"Source file '{Path.GetFileName(file)}' has no usable ID attribute and " +
                            "cannot be matched to a trigger's QueueSourceId; ignoring it.",
                            ExecutionId);
                        continue;
                    }

                    if (_byId.ContainsKey(parsed.SourceId))
                    {
                        // First wins, and the probe order makes that deterministic: the dedicated
                        // sources folder beats the legacy root, so an upgraded deployment that
                        // still has a copy in the root keeps using the new one.
                        Dev2Logger.Warn(
                            $"Source '{parsed.SourceId}' is staged more than once; keeping the first " +
                            $"and ignoring '{file}'. Remove the duplicate so the active broker is " +
                            "unambiguous.",
                            ExecutionId);
                        continue;
                    }

                    _byId[parsed.SourceId] = parsed;
                }
            }

            Dev2Logger.Info(
                $"Source catalog cached {_byId.Count} RabbitMQ source(s) at startup from " +
                $"{string.Join(", ", _searchedFolders.Select(f => $"'{f}'"))}: " +
                $"{string.Join("; ", _byId.Select(kv => $"{kv.Key} -> {kv.Value.Describe()}"))}",
                ExecutionId);
        }

        /// <summary>
        /// Source folders in probe order, skipping any that do not exist so a deployment staging
        /// only one of the two still starts.
        /// </summary>
        static IEnumerable<string> ProbeFolders(QueueProcessorOptions opts)
        {
            var sources = opts.SourcesPath;
            if (Directory.Exists(sources))
            {
                yield return sources;
            }

            var root = opts.SettingsPath;
            if (Directory.Exists(root) && !SamePath(root, sources))
            {
                yield return root;
            }
        }

        static bool SamePath(string a, string b) =>
            string.Equals(
                Path.GetFullPath(a).TrimEnd(Path.DirectorySeparatorChar),
                Path.GetFullPath(b).TrimEnd(Path.DirectorySeparatorChar),
                StringComparison.OrdinalIgnoreCase);
    }
}
