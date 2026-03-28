/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using Dev2.Common;
using Dev2.Common.Interfaces.Data;
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
    /// Scans a resource directory for <see cref="DbSource"/> bite files and provides
    /// on-demand access to individual sources via <see cref="IOnDemandSourceLoader"/>.
    ///
    /// Design (minimum memory)
    /// ───────────────────────
    /// • <see cref="EnsureIndexed"/> reads ONLY the root-element attributes (ResourceID + Type)
    ///   of each .bite file via <see cref="XmlReader"/> — no <see cref="XElement"/> is loaded
    ///   and no <see cref="DbSource"/> is constructed during the scan.
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

        // Key = normalised directory path; Value = Lazy index: ResourceID → absolute file path.
        // Built from XmlReader root-element peeks only — no XElement bodies loaded.
        private readonly ConcurrentDictionary<string, Lazy<IReadOnlyDictionary<Guid, string>>> _directoryIndices =
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
                return;

            var key = Path.GetFullPath(baseDirectory);
            _directoryIndices.GetOrAdd(key,
                k => new Lazy<IReadOnlyDictionary<Guid, string>>(
                    () => BuildFileIndex(k),
                    LazyThreadSafetyMode.ExecutionAndPublication));

            AmbientSourceLoader.Register(this);
        }

        /// <summary>
        /// Loads the single <see cref="DbSource"/> for <paramref name="sourceId"/> from disk
        /// (if not already registered) and adds it to <see cref="ResourceCatalog.Instance"/>.
        /// The <see cref="DbSource"/> object itself is not retained here — only a registration
        /// flag is cached, so <see cref="ResourceCatalog"/> holds the sole strong reference.
        /// Subsequent calls for the same ID are no-ops (flag already set).
        /// </summary>
        bool IOnDemandSourceLoader.EnsureSourceLoaded(Guid sourceId)
        {
            var lazy = _registeredIds.GetOrAdd(sourceId, id =>
                new Lazy<bool>(() =>
                {
                    var source = ResolveFromIndex(id);
                    if (source == null)
                        return false;
                    RegisterSingle(source);
                    return true;
                }, LazyThreadSafetyMode.ExecutionAndPublication));

            return lazy.Value;
        }

        // ── Private helpers ───────────────────────────────────────────────────────────────────────

        private DbSource? ResolveFromIndex(Guid sourceId)
        {
            foreach (var (_, indexLazy) in _directoryIndices)
            {
                if (indexLazy.Value.TryGetValue(sourceId, out var filePath))
                    return LoadSourceFile(filePath);
            }
            return null;
        }

        /// <summary>
        /// Scans <paramref name="directory"/> with <see cref="XmlReader"/> to build a
        /// ResourceID → filePath mapping without loading any XElement bodies.
        /// </summary>
        private static IReadOnlyDictionary<Guid, string> BuildFileIndex(string directory)
        {
            var index = new Dictionary<Guid, string>();

            if (!Directory.Exists(directory))
                return index;

            foreach (var file in Directory.EnumerateFiles(directory, "*.bite", SearchOption.AllDirectories))
            {
                if (TryPeekDbSourceId(file, out var id))
                    index[id] = file;
            }

            return index;
        }

        /// <summary>
        /// Opens <paramref name="filePath"/> with <see cref="XmlReader"/>, reads only the root
        /// element, and returns the ResourceID when <c>Type="DbSource"</c>.
        /// The remainder of the XML is never read.
        /// </summary>
        private static bool TryPeekDbSourceId(string filePath, out Guid id)
        {
            id = Guid.Empty;
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

                    if (!string.Equals(reader.GetAttribute("Type"), "DbSource",
                            StringComparison.OrdinalIgnoreCase))
                        return false; // root element is not a DbSource — stop reading

                    var idStr = reader.GetAttribute("ResourceID") ?? reader.GetAttribute("ID");
                    return idStr != null && Guid.TryParse(idStr, out id) && id != Guid.Empty;
                }
            }
            catch
            {
                // inaccessible or malformed file — skip silently
            }

            return false;
        }

        private static DbSource? LoadSourceFile(string filePath)
        {
            try
            {
                var xe = XElement.Load(filePath);
                var source = new DbSource(xe);
                return source.ResourceID != Guid.Empty ? source : null;
            }
            catch
            {
                return null; // malformed file — skip silently
            }
        }

        private static void RegisterSingle(DbSource source)
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
