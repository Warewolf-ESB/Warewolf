using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace Warewolf.Execution.Lightweight
{
    /// <summary>
    /// File-system–based implementation of <see cref="IApisJsonGenerator"/>.
    ///
    /// Scans <c>WorkflowsDirectory</c> (and an optional sub-folder) for workflow
    /// resource files (*.xml / *.bite), reads only the root XML element attributes
    /// (ResourceType + Name — no XAML body loaded), and produces a minimal
    /// apis.json discovery document.
    ///
    /// The output mirrors <c>ApisJsonBuilder.BuildForPath</c> in the full Warewolf
    /// server; each workflow produces one <c>SingleApi</c> entry with a <c>BaseUrl</c>
    /// pointing to the JSON execution endpoint and an <c>OpenAPI</c> property pointing
    /// to the <c>.api</c> endpoint.
    /// </summary>
    internal sealed class ApisJsonGenerator : IApisJsonGenerator
    {
        readonly string _workflowsDirectory;

        internal ApisJsonGenerator(string workflowsDirectory)
        {
            _workflowsDirectory = workflowsDirectory ?? string.Empty;
        }

        /// <inheritdoc/>
        public string Generate(string pathFilter, Uri requestUri, bool isPublic = false)
        {
            var baseUrl    = BuildBaseUrl(requestUri);
            var accessPath = isPublic ? "Public" : "Services";
            var workflows  = EnumerateWorkflows(pathFilter);
            return BuildApisJson(baseUrl, pathFilter, accessPath, workflows);
        }

        // ── File scanning ─────────────────────────────────────────────────────

        IEnumerable<(string name, string relativePath)> EnumerateWorkflows(string pathFilter)
        {
            if (!Directory.Exists(_workflowsDirectory))
                yield break;

            var searchDir = string.IsNullOrWhiteSpace(pathFilter)
                ? _workflowsDirectory
                : Path.Combine(_workflowsDirectory, pathFilter.Replace('/', Path.DirectorySeparatorChar));

            if (!Directory.Exists(searchDir))
                yield break;

            // Primary format (.bite) first, legacy XML fallback second.
            // Use a HashSet to avoid emitting the same workflow name twice when both
            // a .bite and a .xml file exist in the same directory.
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var pattern in new[] { "*.bite", "*.xml" })
            {
                foreach (var file in Directory.EnumerateFiles(searchDir, pattern, SearchOption.AllDirectories))
                {
                    var (name, isWorkflow) = TryReadWorkflowInfo(file);
                    if (!isWorkflow || name is null)
                        continue;

                    if (!seen.Add(name))
                        continue;

                    var relative   = Path.GetRelativePath(_workflowsDirectory, file);
                    var ext        = Path.GetExtension(relative);
                    var withoutExt = relative[..^ext.Length];
                    yield return (name, withoutExt.Replace(Path.DirectorySeparatorChar, '/'));
                }
            }
        }

        // ── XML header-only reader ────────────────────────────────────────────

        static readonly XmlReaderSettings _xmlSettings = new()
        {
            DtdProcessing               = DtdProcessing.Ignore,
            XmlResolver                 = null,
            IgnoreWhitespace            = true,
            IgnoreComments              = true,
            IgnoreProcessingInstructions = true
        };

        /// <summary>
        /// Opens <paramref name="filePath"/> and reads only the root element attributes.
        /// Returns immediately after the first element — the XAML body is never touched.
        /// </summary>
        static (string? name, bool isWorkflow) TryReadWorkflowInfo(string filePath)
        {
            try
            {
                using var reader = XmlReader.Create(filePath, _xmlSettings);
                while (reader.Read())
                {
                    if (reader.NodeType != XmlNodeType.Element)
                        continue;

                    var resourceType = reader.GetAttribute("ResourceType");
                    if (!string.Equals(resourceType, "WorkflowService", StringComparison.OrdinalIgnoreCase))
                        return (null, false);

                    return (reader.GetAttribute("Name"), true);
                }
            }
            catch { /* skip unreadable files */ }
            return (null, false);
        }

        // ── JSON assembly ─────────────────────────────────────────────────────

        static string BuildApisJson(
            string baseUrl,
            string? pathFilter,
            string accessPath,
            IEnumerable<(string name, string relativePath)> workflows)
        {
            var urlSuffix = string.IsNullOrWhiteSpace(pathFilter)
                ? "apis.json"
                : $"{pathFilter.TrimEnd('/')}/apis.json";

            var apis = new JArray();
            foreach (var (name, relativePath) in workflows)
            {
                apis.Add(new JObject
                {
                    ["Name"]       = name,
                    ["baseUrl"]    = $"{baseUrl}/{accessPath}/{relativePath}.json",
                    ["properties"] = new JArray(new JObject
                    {
                        ["type"]  = "OpenAPI",
                        ["value"] = $"{baseUrl}/{accessPath}/{relativePath}.api"
                    })
                });
            }

            var spec = new JObject
            {
                ["Name"]                 = baseUrl,
                ["Description"]          = string.Empty,
                ["Image"]                = string.Empty,
                ["Url"]                  = $"{baseUrl}/{urlSuffix}",
                ["Created"]              = DateTime.Today.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                ["Modified"]             = DateTime.Today.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                ["SpecificationVersion"] = "0.15",
                ["Apis"]                 = apis,
                ["Include"]              = new JArray(),
                ["Maintainers"]          = new JArray()
            };

            return spec.ToString(Newtonsoft.Json.Formatting.Indented);
        }

        // ── URL helpers ───────────────────────────────────────────────────────

        static string BuildBaseUrl(Uri requestUri) =>
            requestUri.IsDefaultPort
                ? $"{requestUri.Scheme}://{requestUri.Host}"
                : $"{requestUri.Scheme}://{requestUri.Host}:{requestUri.Port}";
    }
}
