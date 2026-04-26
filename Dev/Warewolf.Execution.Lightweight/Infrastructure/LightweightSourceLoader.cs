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
        /// Returns a one-line diagnostic snapshot: indexed directories, their sizes, and any
        /// source-load errors captured since this instance was created.  Designed to be embedded
        /// directly in exception messages so the information surfaces in structured log sinks
        /// that capture exception text (e.g., Azure Functions ILogger).
        /// </summary>
        public string GetDiagnostics()
        {
            var sb = new System.Text.StringBuilder();
            sb.Append($"AmbientSourceLoader.Current={(AmbientSourceLoader.Current == null ? "null" : "registered")}; ");
            sb.Append($"IndexedDirs=[");
            foreach (var (dir, indexLazy) in _directoryIndices)
            {
                try
                {
                    if (indexLazy.IsValueCreated)
                        sb.Append($"{dir}({indexLazy.Value.Count} entries), ");
                    else
                        sb.Append($"{dir}(index not yet built), ");
                }
                catch (Exception ex)
                {
                    sb.Append($"{dir}(INDEX BUILD ERROR: {ex.GetType().Name}: {ex.Message}), ");
                }
            }
            sb.Append("]; ");
            if (_loadErrors.Count > 0)
                sb.Append($"LoadErrors=[{string.Join("; ", _loadErrors)}]");
            return sb.ToString();
        }

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
            Dev2Logger.Warn($"[LightweightSourceLoader] EnsureIndexed: registering directory '{key}' (exists={Directory.Exists(key)}).", GlobalConstants.WarewolfInfo);
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
                    // Log which directories are indexed so we can diagnose path issues.
                    var indexedDirs = string.Join(", ", _directoryIndices.Keys);
                    Dev2Logger.Warn(
                        $"[LightweightSourceLoader] EnsureSourceLoaded({id}): indexed directories=[{indexedDirs}]", GlobalConstants.WarewolfInfo);

                    // Check whether the ID exists in any index before attempting to load.
                    // This lets us give a precise "found but failed" vs "not found" diagnostic.
                    bool foundInIndex = false;
                    foreach (var (dir, indexLazy) in _directoryIndices)
                    {
                        try
                        {
                            var keys = string.Join(", ", indexLazy.Value.Keys.Take(20));
                            var indexMsg = $"EnsureSourceLoaded({id}): directory '{dir}' index ({indexLazy.Value.Count} entries): [{keys}]";
                            _loadErrors.Add(indexMsg);
                            Dev2Logger.Warn($"[LightweightSourceLoader] {indexMsg}", GlobalConstants.WarewolfInfo);

                            if (indexLazy.Value.ContainsKey(id))
                                foundInIndex = true;
                        }
                        catch (Exception ex)
                        {
                            var buildFailMsg = $"EnsureSourceLoaded({id}): directory '{dir}' index build failed: {ex.GetType().Name}: {ex.Message}";
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
                if (source?.ResourceID == Guid.Empty)
                {
                    var msg = $"LoadSourceFile: '{Path.GetFileName(filePath)}' loaded but ResourceID is Guid.Empty — skipping.";
                    _loadErrors.Add(msg);
                    Dev2Logger.Warn($"[LightweightSourceLoader] {msg}", GlobalConstants.WarewolfInfo);
                    return null;
                }
                if (source is DropBoxSource dropboxSource)
                {
                    var tokenPreview = dropboxSource.AccessToken?.Length > 8
                        ? dropboxSource.AccessToken.Substring(0, 8) + "..."
                        : "(empty/null)";
                    var dropboxMsg = $"LoadSourceFile: DropBoxSource '{dropboxSource.ResourceName}' (ID={dropboxSource.ResourceID}) loaded from '{Path.GetFileName(filePath)}'. " +
                        $"AccessToken={tokenPreview}(len={dropboxSource.AccessToken?.Length}) " +
                        $"RefreshToken={(string.IsNullOrEmpty(dropboxSource.RefreshToken) ? "MISSING" : "present")} " +
                        $"AppKey={(string.IsNullOrEmpty(dropboxSource.AppKey) ? "MISSING" : "present")}";
                    _loadErrors.Add(dropboxMsg);
                    Dev2Logger.Warn($"[LightweightSourceLoader] {dropboxMsg}", GlobalConstants.WarewolfInfo);
                }
                return source;
            }
            catch (Exception ex)
            {
                var chain = new System.Text.StringBuilder();
                chain.Append($"{ex.GetType().Name}: {ex.Message}");
                var inner = ex.InnerException;
                while (inner != null)
                {
                    chain.Append($" ---> {inner.GetType().Name}: {inner.Message}");
                    inner = inner.InnerException;
                }
                var msg = $"LoadSourceFile: exception loading '{Path.GetFileName(filePath)}' (Type={sourceType}): {chain}";
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
