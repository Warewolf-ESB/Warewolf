/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Xml;
using Warewolf.Execution.Lightweight.Models;

namespace Warewolf.Execution.Lightweight
{
    /// <summary>
    /// Global lazy-loaded, thread-safe index of Warewolf resource XML files.
    ///
    /// Design goals
    /// ─────────────
    /// • Zero startup cost — scanning is deferred to the first call that needs a
    ///   given base directory (lazy per-directory initialisation).
    /// • O(1) lookups — two pre-built <see cref="System.Collections.Frozen.FrozenDictionary{TKey,TValue}"/>
    ///   instances per directory (zero virtual dispatch, JIT-friendly, lower memory):
    ///     - by ResourceId  (Guid, exact)
    ///     - by Name        (string, OrdinalIgnoreCase, for fallback)
    /// • Minimal I/O — each file is read only up to its first XML element; the
    ///   XmlReader stops immediately after the opening &lt;Service&gt; tag attributes
    ///   are consumed, so even large XAML bodies are never touched.
    /// • Thread-safe — <see cref="Lazy{T}"/> (singleton) +
    ///   <see cref="ConcurrentDictionary{TKey,TValue}"/> (per-directory cache).
    /// </summary>
    internal sealed class WorkflowResourceCache
    {
        // ── Singleton ─────────────────────────────────────────────────────────
        private static readonly Lazy<WorkflowResourceCache> _instance =
            new(() => new WorkflowResourceCache(),
                LazyThreadSafetyMode.ExecutionAndPublication);

        internal static WorkflowResourceCache Instance => _instance.Value;

        private WorkflowResourceCache() { }

        // ── Per-directory index ───────────────────────────────────────────────
        /// <summary>
        /// Holds both lookup dictionaries for a single scanned directory.
        /// Built once; never mutated after construction.
        /// </summary>
        private sealed class DirectoryIndex
        {
            internal readonly FrozenDictionary<Guid,   WorkflowResourceEntry> ById;
            internal readonly FrozenDictionary<string, WorkflowResourceEntry> ByName;

            internal DirectoryIndex(
                FrozenDictionary<Guid,   WorkflowResourceEntry> byId,
                FrozenDictionary<string, WorkflowResourceEntry> byName)
            {
                ById   = byId;
                ByName = byName;
            }
        }

        // Key = normalised absolute directory path
        private readonly System.Collections.Concurrent.ConcurrentDictionary<string, Lazy<DirectoryIndex>> _cache =
            new(StringComparer.OrdinalIgnoreCase);

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Pre-warms the index for <paramref name="baseDirectory"/> so the first
        /// sub-workflow lookup does not pay the full scan cost.  Safe to call multiple
        /// times; subsequent calls are no-ops once the index is built.
        /// </summary>
        internal void WarmUp(string baseDirectory) => GetIndex(baseDirectory);

        /// <summary>
        /// Resolve a workflow file path from a ResourceId (primary) or service
        /// name (fallback).  Returns <c>null</c> when neither key matches.
        /// </summary>
        /// <param name="baseDirectory">Root directory that contains *.xml resource files.</param>
        /// <param name="resourceId">GUID from <c>IDSFDataObject.ResourceID</c>.</param>
        /// <param name="serviceName">Name from <c>IDSFDataObject.ServiceName</c> used as fallback.</param>
        internal string? Resolve(string baseDirectory, Guid resourceId, string? serviceName)
        {
            var index = GetIndex(baseDirectory);

            // 1 – exact ResourceId match (fastest)
            if (resourceId != Guid.Empty && index.ById.TryGetValue(resourceId, out var byId))
                return byId.FilePath;

            // 2 – case-insensitive name fallback
            if (!string.IsNullOrEmpty(serviceName) && index.ByName.TryGetValue(serviceName, out var byName))
                return byName.FilePath;

            return null;
        }

        /// <summary>
        /// Returns the full <see cref="WorkflowResourceEntry"/> for a ResourceId, or
        /// <c>null</c> if not found.  Useful for callers that need metadata beyond
        /// the file path.
        /// </summary>
        internal WorkflowResourceEntry? GetEntry(string baseDirectory, Guid resourceId)
        {
            var index = GetIndex(baseDirectory);
            return index.ById.TryGetValue(resourceId, out var entry) ? entry : null;
        }

        // ── Internal helpers ──────────────────────────────────────────────────

        private DirectoryIndex GetIndex(string baseDirectory)
        {
            var key = Path.GetFullPath(baseDirectory);
            // GetOrAdd with Lazy ensures only one scan per directory even under
            // concurrent first-call pressure.
            var lazy = _cache.GetOrAdd(key,
                k => new Lazy<DirectoryIndex>(
                    () => BuildIndex(k),
                    LazyThreadSafetyMode.ExecutionAndPublication));
            return lazy.Value;
        }

        /// <summary>
        /// Scans <paramref name="directory"/> recursively for *.xml files and
        /// reads only the root element attributes from each — stopping the
        /// XmlReader after the first element so no XAML content is parsed.
        /// </summary>
        private static DirectoryIndex BuildIndex(string directory)
        {
            var byId   = new Dictionary<Guid,   WorkflowResourceEntry>();
            var byName = new Dictionary<string, WorkflowResourceEntry>(StringComparer.OrdinalIgnoreCase);

            if (!Directory.Exists(directory))
                return new DirectoryIndex(
                    FrozenDictionary<Guid,   WorkflowResourceEntry>.Empty,
                    FrozenDictionary<string, WorkflowResourceEntry>.Empty);

            foreach (var file in Directory.EnumerateFiles(directory, "*.bite", SearchOption.AllDirectories))
            {
                var entry = TryReadEntry(file);
                if (entry is null)
                    continue;

                // ResourceId is the authoritative key; last write wins on collision
                byId[entry.ResourceId] = entry;

                // Name index: only register if not already present (first file wins)
                byName.TryAdd(entry.Name, entry);
            }

            return new DirectoryIndex(
                byId.ToFrozenDictionary(),
                byName.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Opens <paramref name="filePath"/> with an <see cref="XmlReader"/> and
        /// reads <em>only</em> the root element's attributes.
        /// The reader is disposed after the opening tag — the XAML body
        /// (which can be hundreds of KB) is never loaded into memory.
        /// </summary>
        private static WorkflowResourceEntry? TryReadEntry(string filePath)
        {
            try
            {
                var settings = new XmlReaderSettings
                {
                    DtdProcessing   = DtdProcessing.Ignore,
                    XmlResolver     = null,
                    IgnoreWhitespace = true,
                    IgnoreComments  = true,
                    IgnoreProcessingInstructions = true
                };

                using var reader = XmlReader.Create(filePath, settings);

                // Advance to the first element — that is the <Service …> root.
                while (reader.Read())
                {
                    if (reader.NodeType != XmlNodeType.Element)
                        continue;

                    var id         = reader.GetAttribute("ID");
                    var serverId   = reader.GetAttribute("ServerID");
                    var name       = reader.GetAttribute("Name");
                    var typeStr    = reader.GetAttribute("ResourceType");

                    // Only index resources with a parseable GUID
                    if (!Guid.TryParse(id, out var resourceId))
                        return null;

                    return new WorkflowResourceEntry(
                        ResourceId:   resourceId,
                        ServerId:     Guid.TryParse(serverId, out var sid) ? sid : Guid.Empty,
                        Name:         name ?? string.Empty,
                        ResourceType: WorkflowResourceEntry.ParseResourceType(typeStr),
                        FilePath:     filePath);
                }
            }
            catch
            {
                // Malformed or inaccessible file — skip silently
            }

            return null;
        }
    }
}
