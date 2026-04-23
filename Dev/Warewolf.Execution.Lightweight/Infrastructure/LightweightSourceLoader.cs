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

        // ── Public API ────────────────────────────────────────────────────────────────────────────

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

                    var source = ResolveFromIndex(id);
                    if (source == null)
                    {
                        // Log the IDs in each directory index so we can see if the source was indexed.
                        foreach (var (dir, indexLazy) in _directoryIndices)
                        {
                            try
                            {
                                var keys = string.Join(", ", indexLazy.Value.Keys.Take(20));
                                Dev2Logger.Warn(
                                    $"[LightweightSourceLoader] Directory '{dir}' index ({indexLazy.Value.Count} entries): [{keys}]", GlobalConstants.WarewolfInfo);
                            }
                            catch (Exception ex)
                            {
                                Dev2Logger.Warn(
                                    $"[LightweightSourceLoader] Directory '{dir}' index build failed: {ex.GetType().Name}: {ex.Message}", GlobalConstants.WarewolfInfo);
                            }
                        }
                        Dev2Logger.Warn(
                            $"[LightweightSourceLoader] EnsureSourceLoaded({id}): source NOT found in any index.", GlobalConstants.WarewolfInfo);
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

        private static IResource? LoadSourceFile(string filePath, string sourceType)
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
                    Dev2Logger.Warn(
                        $"[LightweightSourceLoader] LoadSourceFile: source loaded from '{filePath}' but ResourceID is Guid.Empty — skipping.", GlobalConstants.WarewolfInfo);
                    return null;
                }
                return source;
            }
            catch (Exception ex)
            {
                var sb = new System.Text.StringBuilder();
                sb.Append($"[LightweightSourceLoader] LoadSourceFile: exception loading '{filePath}' (Type={sourceType}): {ex.GetType().Name}: {ex.Message}");
                var inner = ex.InnerException;
                while (inner != null)
                {
                    sb.Append($" ---> {inner.GetType().Name}: {inner.Message}");
                    inner = inner.InnerException;
                }
                Dev2Logger.Warn(sb.ToString(), GlobalConstants.WarewolfInfo);
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
