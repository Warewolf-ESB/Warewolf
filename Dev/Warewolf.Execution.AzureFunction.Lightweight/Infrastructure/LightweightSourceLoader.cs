/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using Dev2.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;
using System.Xml.Linq;

namespace Warewolf.Execution.AzureFunction.Lightweight
{
    /// <summary>
    /// Scans a resource directory for <see cref="DbSource"/> bite files and pre-populates
    /// <see cref="ResourceCatalog.Instance"/> so that SQL Server and other database activities
    /// can resolve their source connection strings in the lightweight Azure Function executor.
    ///
    /// Design
    /// ──────
    /// • Zero cost on repeated calls — per-directory scan is performed at most once (Lazy).
    /// • Workflow bite files (potentially hundreds of KB of XAML) are skipped after reading
    ///   only the root element attributes via XmlReader.
    /// • Thread-safe — singleton + ConcurrentDictionary{Lazy} pattern matching WorkflowResourceCache.
    /// </summary>
    internal sealed class LightweightSourceLoader
    {
        private static readonly Lazy<LightweightSourceLoader> _instance =
            new(() => new LightweightSourceLoader(),
                LazyThreadSafetyMode.ExecutionAndPublication);

        internal static LightweightSourceLoader Instance => _instance.Value;

        private LightweightSourceLoader() { }

        // Key = normalised absolute directory path
        private readonly ConcurrentDictionary<string, Lazy<IReadOnlyList<DbSource>>> _cache =
            new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Ensures database sources in <paramref name="baseDirectory"/> are loaded into
        /// <see cref="ResourceCatalog.Instance"/>.  Safe to call multiple times — subsequent
        /// calls for the same directory are no-ops once the scan is complete.
        /// </summary>
        internal void EnsureLoaded(string baseDirectory)
        {
            if (string.IsNullOrEmpty(baseDirectory))
                return;

            var key = Path.GetFullPath(baseDirectory);
            var lazy = _cache.GetOrAdd(key,
                k => new Lazy<IReadOnlyList<DbSource>>(
                    () => ScanAndRegister(k),
                    LazyThreadSafetyMode.ExecutionAndPublication));

            _ = lazy.Value; // force evaluation on first call
        }

        private static IReadOnlyList<DbSource> ScanAndRegister(string directory)
        {
            var sources = ScanDirectory(directory);
            if (sources.Count > 0)
                RegisterInCatalog(sources);
            return sources;
        }

        private static List<DbSource> ScanDirectory(string directory)
        {
            var sources = new List<DbSource>();

            if (!Directory.Exists(directory))
                return sources;

            foreach (var file in Directory.EnumerateFiles(directory, "*.bite", SearchOption.AllDirectories))
            {
                if (!IsDbSourceFile(file))
                    continue;

                try
                {
                    var xe = XElement.Load(file);
                    var source = new DbSource(xe);
                    if (source.ResourceID != Guid.Empty)
                        sources.Add(source);
                }
                catch
                {
                    // malformed or inaccessible file — skip silently
                }
            }

            return sources;
        }

        /// <summary>
        /// Uses an XmlReader to peek at only the root element, avoiding loading large XAML bodies
        /// from workflow files.  Returns true only for files whose root element is
        /// &lt;Source Type="DbSource" ...&gt;.
        /// </summary>
        private static bool IsDbSourceFile(string filePath)
        {
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

                    return reader.LocalName == "Source" &&
                           string.Equals(reader.GetAttribute("Type"), "DbSource", StringComparison.OrdinalIgnoreCase);
                }
            }
            catch
            {
                // inaccessible or malformed file — skip silently
            }

            return false;
        }

        private static void RegisterInCatalog(IReadOnlyList<DbSource> sources)
        {
            var resources = ResourceCatalog.Instance.WorkspaceResources
                .GetOrAdd(GlobalConstants.ServerWorkspaceID, _ => new List<IResource>());

            lock (resources)
            {
                foreach (var source in sources)
                {
                    if (!resources.Any(r => r.ResourceID == source.ResourceID))
                        resources.Add(source);
                }
            }
        }
    }
}
