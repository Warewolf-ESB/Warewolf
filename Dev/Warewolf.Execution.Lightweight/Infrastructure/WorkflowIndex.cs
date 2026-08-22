/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using Dev2.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Warewolf.Execution.Lightweight
{
    /// <summary>
    /// Build-time pre-computed index of workflow files.
    ///
    /// The companion script <c>Scripts/Generate-WorkflowIndex.ps1</c> runs as a
    /// post-build step and writes <c>workflow-index.json</c> into the Resources
    /// directory.  At startup <see cref="WarmUp"/> reads that file into a
    /// <see cref="FrozenDictionary{TKey,TValue}"/> so that every subsequent call
    /// to <see cref="Resolve"/> is an O(1), allocation-free dictionary hit with no
    /// disk I/O.
    ///
    /// Index file format (JSON):
    /// <code>
    /// {
    ///   "tools/hello world":   "tools/Hello World.bite",
    ///   "examples/data merge": "Examples/Data - Data Merge.bite"
    /// }
    /// </code>
    /// Key   — lowercase relative path without extension, forward-slash separated.<br/>
    /// Value — relative path with extension, original casing, forward-slash separated.
    /// </summary>
    internal sealed class WorkflowIndex
    {
        private const string ExecutionIdForInfrastructure = "WorkflowIndex-Infrastructure";

        // ── Singleton ─────────────────────────────────────────────────────────

        static readonly Lazy<WorkflowIndex> _instance =
            new(() => new WorkflowIndex(), LazyThreadSafetyMode.ExecutionAndPublication);

        internal static WorkflowIndex Instance => _instance.Value;

        private WorkflowIndex() 
        {
            Dev2Logger.Info("WorkflowIndex singleton initialized", ExecutionIdForInfrastructure);
        }

        // ── Constants ─────────────────────────────────────────────────────────

        internal const string IndexFileName = "workflow-index.json";

        static readonly string[] _workflowExtensions = [".bite"];

        // ── Per-directory cache ───────────────────────────────────────────────

        // Key = normalised absolute directory path
        readonly ConcurrentDictionary<string, Lazy<FrozenDictionary<string, string>>> _cache =
            new(StringComparer.OrdinalIgnoreCase);

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Pre-loads the index for <paramref name="workflowsDirectory"/> so that the
        /// first HTTP request pays no file-system cost.  Safe to call multiple times;
        /// subsequent calls are no-ops once the index is loaded.
        /// </summary>
        internal void WarmUp(string workflowsDirectory)
        {
            Dev2Logger.Info($"WorkflowIndex WarmUp started for directory: {workflowsDirectory}", ExecutionIdForInfrastructure);
            try
            {
                var index = GetIndex(workflowsDirectory);
                Dev2Logger.Info($"WorkflowIndex WarmUp completed. Indexed {index.Count} workflows from directory: {workflowsDirectory}", ExecutionIdForInfrastructure);
            }
            catch (Exception ex)
            {
                Dev2Logger.Error($"WorkflowIndex WarmUp failed for directory: {workflowsDirectory}", ex, ExecutionIdForInfrastructure);
                throw;
            }
        }

        /// <summary>
        /// Resolves a workflow name (relative path without extension, any casing) to its
        /// absolute file path on disk.  Returns <c>null</c> when the index file is absent
        /// or the name has no matching entry.
        /// </summary>
        /// <param name="workflowsDirectory">Base directory that hosts workflow files.</param>
        /// <param name="nameWithoutExtension">
        /// Relative path without extension, e.g. <c>tools/hello World</c> or
        /// <c>tools\hello World</c>.  Path separators and casing are normalised
        /// internally before the lookup.
        /// </param>
        internal string? Resolve(string workflowsDirectory, string nameWithoutExtension)
        {
            if (string.IsNullOrWhiteSpace(workflowsDirectory) ||
                string.IsNullOrWhiteSpace(nameWithoutExtension))
            {
                Dev2Logger.Warn($"WorkflowIndex Resolve called with invalid parameters. WorkflowsDirectory: '{workflowsDirectory}', Name: '{nameWithoutExtension}'", ExecutionIdForInfrastructure);
                return null;
            }

            try
            {
                var index = GetIndex(workflowsDirectory);
                if (index.Count == 0)
                {
                    Dev2Logger.Warn($"WorkflowIndex is empty for directory: {workflowsDirectory}", ExecutionIdForInfrastructure);
                    return null;
                }

                // Normalise to forward slashes + lowercase, strip any leading separator.
                var key = nameWithoutExtension
                    .Replace('\\', '/')
                    .TrimStart('/')
                    .ToLowerInvariant();

                if (index.TryGetValue(key, out var relPath))
                {
                    var resolvedPath = Path.Combine(workflowsDirectory, relPath.Replace('/', Path.DirectorySeparatorChar));
                    Dev2Logger.Debug($"WorkflowIndex Resolve successful. Key: '{key}' -> Path: '{resolvedPath}'", ExecutionIdForInfrastructure);
                    return resolvedPath;
                }

                Dev2Logger.Warn($"WorkflowIndex Resolve failed. No match found for key: '{key}' in directory: {workflowsDirectory}", ExecutionIdForInfrastructure);
                return null;
            }
            catch (Exception ex)
            {
                Dev2Logger.Error($"WorkflowIndex Resolve error for name: '{nameWithoutExtension}' in directory: {workflowsDirectory}", ex, ExecutionIdForInfrastructure);
                return null;
            }
        }

        /// <summary>
        /// Adds or updates a single entry in the in-memory index for
        /// <paramref name="workflowsDirectory"/>, so a workflow just written to disk by
        /// <c>create_workflow</c>/<c>edit_workflow</c> resolves via the fast <see cref="Resolve"/>
        /// path on the very next call — without waiting for a fresh process start (which would
        /// re-run <see cref="WarmUp"/>) or relying solely on <see cref="WorkflowNameResolver"/>'s
        /// on-disk fallback.
        ///
        /// <para>
        /// Only the in-memory <see cref="FrozenDictionary{TKey,TValue}"/> cache is updated — the
        /// persisted <c>workflow-index.json</c> file (a build-time artefact written by
        /// <c>Scripts/Generate-WorkflowIndex.ps1</c>) is intentionally left untouched here; the
        /// next build reconciles it from disk. Safe to call even before the index for this
        /// directory has ever been loaded (loads it first via <see cref="GetIndex"/>).
        /// </para>
        /// </summary>
        /// <param name="workflowsDirectory">Base directory that hosts workflow files.</param>
        /// <param name="nameWithoutExtension">Relative path without extension, any casing/separator.</param>
        /// <param name="relativePathWithExtension">Relative path with extension, original casing, forward-slash separated.</param>
        internal void AddOrUpdate(string workflowsDirectory, string nameWithoutExtension, string relativePathWithExtension)
        {
            if (string.IsNullOrWhiteSpace(workflowsDirectory) ||
                string.IsNullOrWhiteSpace(nameWithoutExtension) ||
                string.IsNullOrWhiteSpace(relativePathWithExtension))
            {
                return;
            }

            try
            {
                var cacheKey = Path.GetFullPath(workflowsDirectory);
                var normalizedName = nameWithoutExtension.Replace('\\', '/').TrimStart('/').ToLowerInvariant();
                var normalizedPath = relativePathWithExtension.Replace('\\', '/');

                // Snapshot the current (possibly not-yet-loaded) index, layer the new entry on
                // top, and re-freeze. Not on any hot path, so the read-copy-replace race with a
                // concurrent Resolve() (which would just see the old, still-valid snapshot) is
                // an acceptable trade-off for keeping this simple.
                var current = GetIndex(workflowsDirectory);
                var updated = new Dictionary<string, string>(current, StringComparer.OrdinalIgnoreCase)
                {
                    [normalizedName] = normalizedPath
                };

                _cache[cacheKey] = new Lazy<FrozenDictionary<string, string>>(
                    () => updated.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase),
                    LazyThreadSafetyMode.ExecutionAndPublication);

                Dev2Logger.Info($"WorkflowIndex AddOrUpdate: '{normalizedName}' -> '{normalizedPath}' in directory: {workflowsDirectory}", ExecutionIdForInfrastructure);
            }
            catch (Exception ex)
            {
                Dev2Logger.Error($"WorkflowIndex AddOrUpdate failed for: '{nameWithoutExtension}' in directory: {workflowsDirectory}", ex, ExecutionIdForInfrastructure);
            }
        }

        /// <summary>
        /// Drops a single entry from the in-memory index for <paramref name="workflowsDirectory"/>,
        /// so a workflow just deleted from disk by <c>delete_workflow</c> stops resolving via the
        /// fast <see cref="Resolve"/> path immediately — without waiting for a process restart.
        /// The exact inverse of <see cref="AddOrUpdate"/>, and it shares that method's contract:
        /// only the in-memory <see cref="FrozenDictionary{TKey,TValue}"/> cache is touched, the
        /// persisted <c>workflow-index.json</c> build artefact is left for the next build to
        /// reconcile.
        ///
        /// <para>
        /// Removing a key that was never present is a no-op, so this is safe to call
        /// unconditionally after a delete, and safe before the index for this directory has ever
        /// been loaded (loads it first via <see cref="GetIndex"/>).
        /// </para>
        /// </summary>
        /// <param name="workflowsDirectory">Base directory that hosts workflow files.</param>
        /// <param name="nameWithoutExtension">Relative path without extension, any casing/separator.</param>
        internal void Remove(string workflowsDirectory, string nameWithoutExtension)
        {
            if (string.IsNullOrWhiteSpace(workflowsDirectory) ||
                string.IsNullOrWhiteSpace(nameWithoutExtension))
            {
                return;
            }

            try
            {
                var cacheKey = Path.GetFullPath(workflowsDirectory);
                var normalizedName = nameWithoutExtension.Replace('\\', '/').TrimStart('/').ToLowerInvariant();

                // Same read-copy-replace as AddOrUpdate: snapshot, drop the key, re-freeze. A
                // concurrent Resolve() racing this sees the old snapshot and resolves a path whose
                // file has already been deleted — which its caller must handle regardless, since
                // any file can vanish between Resolve and open.
                var current = GetIndex(workflowsDirectory);
                if (!current.ContainsKey(normalizedName))
                {
                    return;
                }

                var updated = new Dictionary<string, string>(current, StringComparer.OrdinalIgnoreCase);
                updated.Remove(normalizedName);

                _cache[cacheKey] = new Lazy<FrozenDictionary<string, string>>(
                    () => updated.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase),
                    LazyThreadSafetyMode.ExecutionAndPublication);

                Dev2Logger.Info($"WorkflowIndex Remove: '{normalizedName}' in directory: {workflowsDirectory}", ExecutionIdForInfrastructure);
            }
            catch (Exception ex)
            {
                Dev2Logger.Error($"WorkflowIndex Remove failed for: '{nameWithoutExtension}' in directory: {workflowsDirectory}", ex, ExecutionIdForInfrastructure);
            }
        }

        // ── Internal helpers ──────────────────────────────────────────────────

        FrozenDictionary<string, string> GetIndex(string workflowsDirectory)
        {
            var key = Path.GetFullPath(workflowsDirectory);

            // GetOrAdd with Lazy ensures only one file read per directory even under
            // concurrent first-call pressure.
            var lazy = _cache.GetOrAdd(key,
                k => new Lazy<FrozenDictionary<string, string>>(
                    () => LoadIndex(k),
                    LazyThreadSafetyMode.ExecutionAndPublication));

            return lazy.Value;
        }

        /// <summary>
        /// Returns a <see cref="FrozenDictionary{TKey,TValue}"/> for <paramref name="workflowsDirectory"/>.
        /// When <c>workflow-index.json</c> is present it is deserialised directly (minimal cold-start cost).
        /// When absent the directory is scanned once, the result is persisted for future cold-starts,
        /// and the in-memory index is returned.
        /// </summary>
        static FrozenDictionary<string, string> LoadIndex(string workflowsDirectory)
        {
            var indexPath = Path.Combine(workflowsDirectory, IndexFileName);
            Dev2Logger.Debug($"WorkflowIndex LoadIndex attempting to load from: {indexPath}", "WorkflowIndex-Infrastructure");

            if (File.Exists(indexPath))
            {
                Dev2Logger.Info($"WorkflowIndex found existing index file: {indexPath}", "WorkflowIndex-Infrastructure");
                var result = TryDeserializeIndex(indexPath);
                Dev2Logger.Info($"WorkflowIndex deserialized {result.Count} entries from: {indexPath}", "WorkflowIndex-Infrastructure");
                return result;
            }

            // Index absent — build from disk, persist for future cold-starts, then return.
            Dev2Logger.Warn($"WorkflowIndex file not found at: {indexPath}. Building from disk scan...", "WorkflowIndex-Infrastructure");
            try
            {
                var dict = BuildIndexFromDisk(workflowsDirectory);
                Dev2Logger.Info($"WorkflowIndex built {dict.Count} entries from disk for directory: {workflowsDirectory}", "WorkflowIndex-Infrastructure");

                TryPersistIndex(indexPath, dict);

                return dict.Count > 0
                    ? dict.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase)
                    : FrozenDictionary<string, string>.Empty;
            }
            catch (Exception ex)
            {
                Dev2Logger.Error($"WorkflowIndex LoadIndex failed for directory: {workflowsDirectory}", ex, "WorkflowIndex-Infrastructure");
                return FrozenDictionary<string, string>.Empty;
            }
        }

        /// <summary>
        /// Deserialises <c>workflow-index.json</c> from <paramref name="indexPath"/>.
        /// Returns <see cref="FrozenDictionary{TKey,TValue}.Empty"/> when the file is
        /// malformed or inaccessible.
        /// </summary>
        static FrozenDictionary<string, string> TryDeserializeIndex(string indexPath)
        {
            try
            {
                var json = File.ReadAllText(indexPath);
                var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                if (dict is null or { Count: 0 })
                {
                    Dev2Logger.Warn($"WorkflowIndex deserialization resulted in empty dictionary from: {indexPath}", "WorkflowIndex-Infrastructure");
                    return FrozenDictionary<string, string>.Empty;
                }

                return dict.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
            }
            catch (Exception ex)
            {
                Dev2Logger.Error($"WorkflowIndex deserialization failed for file: {indexPath}", ex, "WorkflowIndex-Infrastructure");
                return FrozenDictionary<string, string>.Empty;
            }
        }

        /// <summary>
        /// Recursively enumerates <c>.bite</c> then <c>.xml</c> files under
        /// <paramref name="workflowsDirectory"/> and builds a case-insensitive map of
        /// lowercase extension-free relative key → original relative path with extension.
        /// <c>.bite</c> takes priority: the first writer per key is never overwritten.
        /// </summary>
        static Dictionary<string, string> BuildIndexFromDisk(string workflowsDirectory)
        {
            if (!Directory.Exists(workflowsDirectory))
            {
                Dev2Logger.Error($"WorkflowIndex BuildIndexFromDisk failed: Directory does not exist: {workflowsDirectory}", "WorkflowIndex-Infrastructure");
                return new Dictionary<string, string>(0, StringComparer.OrdinalIgnoreCase);
            }

            Dev2Logger.Info($"WorkflowIndex BuildIndexFromDisk starting scan of directory: {workflowsDirectory}", "WorkflowIndex-Infrastructure");

            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var basePrefix = workflowsDirectory.TrimEnd(
                Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                + Path.DirectorySeparatorChar;

            foreach (var ext in _workflowExtensions)
            {
                try
                {
                    var files = Directory.EnumerateFiles(workflowsDirectory, $"*{ext}", SearchOption.AllDirectories).ToList();
                    Dev2Logger.Debug($"WorkflowIndex found {files.Count} {ext} files in: {workflowsDirectory}", "WorkflowIndex-Infrastructure");

                    foreach (var file in files)
                    {
                        try
                        {
                            var relPath = file[basePrefix.Length..];
                            var noExt   = relPath[..^ext.Length];
                            var key     = noExt.Replace('\\', '/').TrimStart('/').ToLowerInvariant();
                            var value   = relPath.Replace('\\', '/');

                            if (dict.TryAdd(key, value))
                            {
                                Dev2Logger.Debug($"WorkflowIndex indexed: '{key}' -> '{value}'", "WorkflowIndex-Infrastructure");
                            }
                            else
                            {
                                Dev2Logger.Warn($"WorkflowIndex skipped duplicate key: '{key}' for file: {file}", "WorkflowIndex-Infrastructure");
                            }
                        }
                        catch (Exception ex)
                        {
                            Dev2Logger.Error($"WorkflowIndex error processing file: {file}", ex, "WorkflowIndex-Infrastructure");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Dev2Logger.Error($"WorkflowIndex error enumerating {ext} files in: {workflowsDirectory}", ex, "WorkflowIndex-Infrastructure");
                }
            }

            Dev2Logger.Info($"WorkflowIndex BuildIndexFromDisk completed. Total entries: {dict.Count}", "WorkflowIndex-Infrastructure");
            return dict;
        }

        /// <summary>
        /// Serialises <paramref name="dict"/> to <paramref name="indexPath"/> as sorted,
        /// indented JSON.  Write failures are silently swallowed — the in-memory index remains valid.
        /// </summary>
        static void TryPersistIndex(string indexPath, Dictionary<string, string> dict)
        {
            try
            {
                Dev2Logger.Info($"WorkflowIndex TryPersistIndex attempting to write {dict.Count} entries to: {indexPath}", "WorkflowIndex-Infrastructure");

                var sorted = dict
                    .OrderBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(kv => kv.Key, kv => kv.Value);

                var json = JsonConvert.SerializeObject(sorted, Formatting.Indented);
                File.WriteAllText(indexPath, json);

                Dev2Logger.Info($"WorkflowIndex successfully persisted index to: {indexPath}", "WorkflowIndex-Infrastructure");
            }
            catch (Exception ex)
            {
                Dev2Logger.Warn($"WorkflowIndex persist failed for: {indexPath}. In-memory index remains valid.", ex, "WorkflowIndex-Infrastructure");
            }
        }
    }
}
