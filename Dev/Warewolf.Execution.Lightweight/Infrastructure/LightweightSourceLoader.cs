/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using Dev2.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.Data.ServiceModel;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;
using System.Xml.Linq;

namespace Warewolf.Execution.Lightweight
{
    /// <summary>
    /// Scans a resource directory for <see cref="DbSource"/>, <see cref="WebSource"/>,
    /// <see cref="RedisSource"/>, <see cref="RabbitMQSource"/>, <see cref="EmailSource"/>,
    /// <see cref="ExchangeSource"/>, <see cref="DropBoxSource"/>, <see cref="SharepointSource"/>,
    /// and <see cref="ElasticsearchSource"/> bite files and provides on-demand access to
    /// individual sources via <see cref="IOnDemandSourceLoader"/>.
    ///
    /// Design (minimum memory)
    /// ───────────────────────
    /// • <see cref="EnsureIndexed"/> reads ONLY the root-element attributes (ResourceID + Type)
    ///   of each .bite file via <see cref="XmlReader"/> — no <see cref="XElement"/> is loaded
    ///   and no source object is constructed during the scan.
    /// • <see cref="IOnDemandSourceLoader.EnsureSourceLoaded"/> loads and registers exactly
    ///   ONE source when first requested, caching it for subsequent lookups in the same
    ///   function instance.
    /// • Thread-safe — singleton + ConcurrentDictionary{Lazy} pattern.
    /// </summary>
    internal sealed class LightweightSourceLoader : IOnDemandSourceLoader
    {
        private static readonly Lazy<LightweightSourceLoader> _instance =
            new(() => new LightweightSourceLoader(),
                LazyThreadSafetyMode.ExecutionAndPublication);

        internal static LightweightSourceLoader Instance => _instance.Value;

        private LightweightSourceLoader() { }

        // Key = normalised directory path.
        // Value = Lazy index: ResourceID → (absolute file path, source type string e.g. "DbSource" / "WebSource" / "RedisSource" / "RabbitMQSource").
        // Built from XmlReader root-element peeks only — no XElement bodies loaded.
        private readonly ConcurrentDictionary<string, Lazy<IReadOnlyDictionary<Guid, (string Path, string Type)>>> _directoryIndices =
            new(StringComparer.OrdinalIgnoreCase);

        // Key = ResourceID; Value = Lazy<bool> — true once the source has been loaded from disk
        // and registered into ResourceCatalog. Stores only a bool, not the DbSource object itself,
        // so the single authoritative strong reference lives exclusively in ResourceCatalog.
        // ExecutionAndPublication ensures the file is read and registered at most once per ID
        // even under concurrent requests targeting the same source.
        private readonly ConcurrentDictionary<Guid, Lazy<bool>> _registeredIds = new();

        // Accumulates load-error messages from LoadSourceFile for inclusion in diagnostics.
        private readonly System.Collections.Concurrent.ConcurrentBag<string> _loadErrors = new();

        // ── Public API ────────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Removes the cached registration flag for <paramref name="sourceId"/> and
        /// discards the corresponding object from <see cref="ResourceCatalog"/> so
        /// that the next call to <see cref="IOnDemandSourceLoader.EnsureSourceLoaded"/>
        /// re-reads the source from disk.
        ///
        /// Called by <c>DropboxOAuthFunction</c> after it writes new OAuth tokens
        /// back to a <c>.bite</c> file so the next workflow execution picks up the
        /// refreshed tokens without requiring a server restart.
        /// </summary>
        /// <summary>
        /// Returns the absolute file path recorded in the directory index for
        /// <paramref name="sourceId"/>, or <c>null</c> when the source has not yet
        /// been indexed (index not yet built, or ID not present in any indexed directory).
        ///
        /// Called by <c>DropboxOAuthFunction</c> so it can write new OAuth tokens to the
        /// exact file the source loader will re-read from after cache invalidation,
        /// regardless of what <c>WorkflowsDirectory</c> is set to.
        /// </summary>
        internal string? GetIndexedFilePath(Guid sourceId)
        {
            foreach (var (_, indexLazy) in _directoryIndices)
            {
                try
                {
                    if (indexLazy.IsValueCreated &&
                        indexLazy.Value.TryGetValue(sourceId, out var entry))
                        return entry.Path;
                }
                catch { /* index build error — skip */ }
            }
            return null;
        }

        internal void Invalidate(Guid sourceId)
        {
            // Remove the "already loaded" flag so EnsureSourceLoaded will re-run.
            _registeredIds.TryRemove(sourceId, out _);

            // Remove the stale source object from ResourceCatalog so the loader
            // can register a fresh copy on the next EnsureSourceLoaded call.
            if (ResourceCatalog.Instance.WorkspaceResources
                    .TryGetValue(GlobalConstants.ServerWorkspaceID, out var resources))
            {
                lock (resources)
                {
                    resources.RemoveAll(r => r.ResourceID == sourceId);
                }
            }

            Dev2Logger.Warn(
                $"[LightweightSourceLoader] Invalidate({sourceId}): cache entry removed. " +
                "Source will be reloaded from disk on next access.",
                GlobalConstants.WarewolfInfo);
        }

        /// <summary>
        /// Returns a one-line diagnostic snapshot: indexed directories, their sizes, and any
        /// source-load errors captured since this instance was created.  Designed to be embedded
        /// directly in exception messages so the information surfaces in structured log sinks
        /// that capture exception text (e.g., Azure Functions ILogger).
        /// </summary>
        public string GetDiagnostics()
        {
            // Leaf folder names only — SharepointReadListActivity embeds this string in a thrown
            // InvalidOperationException, which reaches the caller via the workflow's error list,
            // so it must not disclose absolute paths.
            var dirs   = string.Join(", ", _directoryIndices.Select(kv => $"{DirName(kv.Key)}({Describe(kv.Value)})"));
            var errors = _loadErrors.IsEmpty ? string.Empty : $" LoadErrors=[{string.Join("; ", _loadErrors)}]";

            return $"AmbientSourceLoader.Current={(AmbientSourceLoader.Current is null ? "null" : "registered")}; " +
                   $"IndexedDirs=[{dirs}];{errors}";

            // A Lazy whose factory threw reports IsValueCreated == false and caches the
            // exception, so "index not built" deliberately covers both never-attempted and
            // failed. We must not touch .Value to tell them apart: for a never-attempted
            // index that would trigger the whole directory scan from a diagnostic call.
            static string Describe(Lazy<IReadOnlyDictionary<Guid, (string Path, string Type)>> index)
            {
                try   { return index.IsValueCreated ? $"{index.Value.Count} entries" : "index not built"; }
                catch (Exception ex) { return $"INDEX BUILD ERROR: {ex.Message}"; }
            }
        }

        /// <summary>
        /// Returns the leaf folder name of <paramref name="directory"/> — e.g. <c>Resources</c>
        /// for <c>D:\home\site\wwwroot\Resources</c>.  Used so production Info/Error/Warning
        /// entries and <see cref="GetDiagnostics"/> can identify which directory is involved
        /// without disclosing its absolute path.
        /// </summary>
        private static string DirName(string directory) =>
            Path.GetFileName(directory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
                is { Length: > 0 } name
                ? name
                : "(root)";

        /// <summary>
        /// Registers <paramref name="baseDirectory"/> for on-demand source resolution and
        /// sets this instance as the ambient <see cref="IOnDemandSourceLoader"/>.
        /// Only reads root-element attributes (XmlReader peek) — no XElement is loaded.
        /// Safe to call multiple times; the index for a given directory is built at most once.
        /// </summary>
        internal void EnsureIndexed(string baseDirectory)
        {
            if (string.IsNullOrEmpty(baseDirectory))
            {
                Dev2Logger.Warn("[LightweightSourceLoader] EnsureIndexed called with null/empty directory — source indexing skipped.", GlobalConstants.WarewolfInfo);
                return;
            }

            var key = Path.GetFullPath(baseDirectory);

            // Leaf folder name only; the absolute path is never logged. Emitted once.
            Dev2Logger.Warn($"[LightweightSourceLoader] EnsureIndexed: registering directory '{DirName(key)}' (exists={Directory.Exists(key)}).", GlobalConstants.WarewolfInfo);
            _directoryIndices.GetOrAdd(key,
                k => new Lazy<IReadOnlyDictionary<Guid, (string Path, string Type)>>(
                    () => BuildFileIndex(k),
                    LazyThreadSafetyMode.ExecutionAndPublication));

            AmbientSourceLoader.Register(this);
        }

        /// <summary>
        /// Loads the single source (<see cref="DbSource"/>, <see cref="WebSource"/>,
        /// <see cref="RedisSource"/>, <see cref="RabbitMQSource"/>, <see cref="EmailSource"/>,
        /// <see cref="ExchangeSource"/>, <see cref="DropBoxSource"/>, <see cref="SharepointSource"/>,
        /// or <see cref="ElasticsearchSource"/>) for <paramref name="sourceId"/> from disk
        /// (if not already registered) and adds it to <see cref="ResourceCatalog.Instance"/>.
        /// The source object itself is not retained here — only a registration flag is cached,
        /// so <see cref="ResourceCatalog"/> holds the sole strong reference.
        /// Subsequent calls for the same ID are no-ops (flag already set).
        /// </summary>
        bool IOnDemandSourceLoader.EnsureSourceLoaded(Guid sourceId)
        {
            var lazy = _registeredIds.GetOrAdd(sourceId, id =>
                new Lazy<bool>(() =>
                {
                    // Directory COUNT at Warning so path issues are still visible in production;
                    // the absolute paths themselves are never logged. Emitted once.
                    Dev2Logger.Warn(
                        $"[LightweightSourceLoader] EnsureSourceLoaded({id}): {_directoryIndices.Count} indexed directory(ies).", GlobalConstants.WarewolfInfo);

                    // Check whether the ID exists in any index before attempting to load.
                    // This lets us give a precise "found but failed" vs "not found" diagnostic.
                    bool foundInIndex = false;
                    foreach (var (dir, indexLazy) in _directoryIndices)
                    {
                        try
                        {
                            // Entry COUNT only at Warning — index keys are resource GUIDs and the
                            // directory is an absolute path, so neither belongs in production logs.
                            // Not accumulated into _loadErrors either: this is a success-path
                            // diagnostic, and _loadErrors is replayed verbatim by GetDiagnostics().
                            Dev2Logger.Warn(
                                $"[LightweightSourceLoader] EnsureSourceLoaded({id}): directory '{DirName(dir)}' index has {indexLazy.Value.Count} entries.", GlobalConstants.WarewolfInfo);

                            if (indexLazy.Value.ContainsKey(id))
                                foundInIndex = true;
                        }
                        catch (Exception ex)
                        {
                            var buildFailMsg = $"EnsureSourceLoaded({id}): index build failed for directory '{DirName(dir)}': {ex.Message}";
                            _loadErrors.Add(buildFailMsg);
                            Dev2Logger.Warn($"[LightweightSourceLoader] {buildFailMsg}", GlobalConstants.WarewolfInfo);
                        }
                    }

                    var source = ResolveFromIndex(id);
                    if (source == null)
                    {
                        // Distinguish "found in index but failed to load" (e.g. AES key mismatch)
                        // from "genuinely not present in any index" so the diagnostic is actionable.
                        var summaryMsg = foundInIndex
                            ? $"EnsureSourceLoaded({id}): source found in index but failed to load — check LoadSourceFile error above (e.g. AES key mismatch or corrupt .bite file)."
                            : $"EnsureSourceLoaded({id}): source NOT found in any indexed directory.";
                        _loadErrors.Add(summaryMsg);
                        Dev2Logger.Warn($"[LightweightSourceLoader] {summaryMsg}", GlobalConstants.WarewolfInfo);
                        return false;
                    }
                    RegisterSingle(source);
                    Dev2Logger.Warn(
                        $"[LightweightSourceLoader] EnsureSourceLoaded({id}): source registered OK (Type={source.GetType().Name}, ResourceID={source.ResourceID}).", GlobalConstants.WarewolfInfo);
                    return true;
                }, LazyThreadSafetyMode.ExecutionAndPublication));

            return lazy.Value;
        }

        // ── Private helpers ───────────────────────────────────────────────────────────────────────

        private IResource? ResolveFromIndex(Guid sourceId)
        {
            foreach (var (_, indexLazy) in _directoryIndices)
            {
                if (indexLazy.Value.TryGetValue(sourceId, out var entry))
                    return LoadSourceFile(entry.Path, entry.Type);
            }
            return null;
        }

        /// <summary>
        /// Scans <paramref name="directory"/> with <see cref="XmlReader"/> to build a
        /// ResourceID → (filePath, sourceType) mapping without loading any XElement bodies.
        /// Accepts <c>DbSource</c>, <c>WebSource</c>, <c>RedisSource</c>, <c>RabbitMQSource</c>,
        /// <c>EmailSource</c>, <c>ExchangeSource</c>, <c>DropBoxSource</c>,
        /// <c>SharepointSource</c>, and <c>ElasticsearchSource</c> type attributes.
        /// </summary>
        private static IReadOnlyDictionary<Guid, (string Path, string Type)> BuildFileIndex(string directory)
        {
            var index = new Dictionary<Guid, (string Path, string Type)>();

            if (!Directory.Exists(directory))
                return index;

            foreach (var file in Directory.EnumerateFiles(directory, "*.bite", SearchOption.AllDirectories))
            {
                if (TryPeekSourceId(file, out var id, out var sourceType))
                    index[id] = (file, sourceType);
            }

            return index;
        }

        /// <summary>
        /// Opens <paramref name="filePath"/> with <see cref="XmlReader"/>, reads only the root
        /// element, and returns the ResourceID when <c>Type</c> is <c>DbSource</c>,
        /// <c>WebSource</c>, <c>RedisSource</c>, <c>RabbitMQSource</c>, <c>EmailSource</c>,
        /// <c>ExchangeSource</c>, <c>DropBoxSource</c>, <c>SharepointSource</c>, or
        /// <c>ElasticsearchSource</c>. The remainder of the XML is never read.
        /// </summary>
        private static bool TryPeekSourceId(string filePath, out Guid id, out string sourceType)
        {
            id = Guid.Empty;
            sourceType = string.Empty;
            try
            {
                var settings = new XmlReaderSettings
                {
                    DtdProcessing = DtdProcessing.Ignore,
                    XmlResolver = null,
                    IgnoreWhitespace = true,
                    IgnoreComments = true,
                    IgnoreProcessingInstructions = true
                };

                using var reader = XmlReader.Create(filePath, settings);
                while (reader.Read())
                {
                    if (reader.NodeType != XmlNodeType.Element)
                        continue;

                    var typeAttr = reader.GetAttribute("Type") ?? string.Empty;
                    if (typeAttr.ToLowerInvariant() is not ("dbsource" or "websource" or "redissource" or "rabbitmqsource"
                            or "emailsource" or "exchangesource" or "dropboxsource"
                            or "sharepointsource" or "elasticsearchsource"))
                        return false; // root element is not a supported source type — stop reading

                    var idStr = reader.GetAttribute("ResourceID") ?? reader.GetAttribute("ID");
                    if (idStr != null && Guid.TryParse(idStr, out id) && id != Guid.Empty)
                    {
                        sourceType = typeAttr;
                        return true;
                    }
                    return false;
                }
            }
            catch
            {
                // inaccessible or malformed file — skip silently
            }

            return false;
        }

        private IResource? LoadSourceFile(string filePath, string sourceType)
        {
            try
            {
                var xe = XElement.Load(filePath);
                IResource source = sourceType.ToLowerInvariant() switch
                {
                    "websource"           => new WebSource(xe),
                    "dbsource"            => new DbSource(xe),
                    "redissource"         => new RedisSource(xe),
                    "rabbitmqsource"      => new RabbitMQSource(xe),
                    "emailsource"         => new EmailSource(xe),
                    "exchangesource"      => new ExchangeSource(xe),
                    "dropboxsource"       => new DropBoxSource(xe),
                    "sharepointsource"    => new SharepointSource(xe),
                    "elasticsearchsource" => new ElasticsearchSource(xe),
                    _ => null
                };
                if (source != null)
                {
                    source.FilePath = filePath;
                }
                if (source?.ResourceID == Guid.Empty)
                {
                    var msg = $"LoadSourceFile: '{Path.GetFileName(filePath)}' loaded but ResourceID is Guid.Empty — skipping.";
                    _loadErrors.Add(msg);
                    Dev2Logger.Warn($"[LightweightSourceLoader] {msg}", GlobalConstants.WarewolfInfo);
                    return null;
                }
                if (source is DropBoxSource)
                {
                    // Never log or store a token prefix, length, or presence flag: a prefix
                    // identifies the credential and a length narrows a brute-force search.
                    // Nothing is added to _loadErrors — GetDiagnostics() replays it verbatim
                    // into exception messages that can reach a response body.
                    Dev2Logger.Warn(
                        "[LightweightSourceLoader] Dropbox source loaded successfully.",
                        GlobalConstants.WarewolfInfo);
                }
                return source;
            }
            catch (Exception ex)
            {
                // NOTE: this string is also accumulated into _loadErrors, which GetDiagnostics()
                // replays into exception messages that can surface in an HTTP response body.
                var msg = $"LoadSourceFile: exception loading '{Path.GetFileName(filePath)}' (Type={sourceType}): {ex.Message}";
                _loadErrors.Add(msg);
                Dev2Logger.Warn($"[LightweightSourceLoader] {msg}", GlobalConstants.WarewolfInfo);
                return null;
            }
        }

        private static void RegisterSingle(IResource source)
        {
            var resources = ResourceCatalog.Instance.WorkspaceResources
                .GetOrAdd(GlobalConstants.ServerWorkspaceID, _ => new List<IResource>());

            lock (resources)
            {
                if (!resources.Any(r => r.ResourceID == source.ResourceID))
                    resources.Add(source);
            }
        }
    }
}
