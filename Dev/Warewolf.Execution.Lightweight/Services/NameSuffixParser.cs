using System;

namespace Warewolf.Execution.Lightweight
{
    /// <summary>
    /// Parses well-known suffixes from a Warewolf workflow route segment,
    /// mirroring the suffix-detection logic in <c>WebServerController.ExecuteWorkflow</c>.
    ///
    /// Suffix precedence (matches the full server):
    ///   .api   → OpenAPI spec request
    ///   .debug → debug-mode execution
    ///   .xml   → XML output
    ///   .json  → JSON output (same as no suffix; strips extension only)
    /// </summary>
    internal static class NameSuffixParser
    {
        /// <summary>
        /// Strips the outermost recognised suffix from <paramref name="name"/> and
        /// returns flags indicating which suffix was found.
        /// </summary>
        internal static (string name, bool isDebug, bool isXml, bool isApi) Parse(string name)
        {
            if (name.EndsWith(".api", StringComparison.OrdinalIgnoreCase))
                return (name[..^4], false, false, true);
            if (name.EndsWith(".debug", StringComparison.OrdinalIgnoreCase))
                return (name[..^6], true, false, false);
            if (name.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                return (name[..^4], false, true, false);
            if (name.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                return (name[..^5], false, false, false);
            return (name, false, false, false);
        }

        /// <summary>
        /// Returns <c>true</c> when <paramref name="name"/> is, or ends with,
        /// <c>apis.json</c> — indicating the caller wants a workflow-discovery listing
        /// rather than a workflow execution.
        /// </summary>
        internal static bool IsApisJsonRequest(string name) =>
            name.EndsWith("apis.json", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Extracts the folder path prefix from an apis.json route segment.
        ///
        /// Examples:
        ///   "apis.json"               → <c>null</c>  (root listing)
        ///   "MyFolder/apis.json"      → "MyFolder"
        ///   "A/B/apis.json"           → "A/B"
        ///
        /// Mirrors the split logic in <c>WebServerController.ExecuteWorkflow</c>:
        /// <code>
        ///   var path = __name__.Split(new[] { "/apis.json" }, StringSplitOptions.RemoveEmptyEntries);
        ///   if (path.Any() &amp;&amp; path[0].Equals("apis.json", ...)) path[0] = null;
        /// </code>
        /// </summary>
        internal static string? ExtractApisJsonPath(string name)
        {
            var parts = name.Split(new[] { "/apis.json" }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0 || parts[0].Equals("apis.json", StringComparison.OrdinalIgnoreCase))
                return null;
            return parts[0];
        }
    }
}
