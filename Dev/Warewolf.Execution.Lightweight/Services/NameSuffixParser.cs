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
    ///
    /// All public methods accept raw route segments that may contain query strings,
    /// percent-encoded characters, and backslash path separators.  Normalization is
    /// applied internally before any comparison.
    /// </summary>
    internal static class NameSuffixParser
    {
        /// <summary>
        /// Normalises a raw route segment for reliable suffix and path comparisons:
        /// <list type="number">
        ///   <item>Strips the query string — drops everything from the first literal
        ///         <c>?</c> or its percent-encoded form <c>%3F</c>.</item>
        ///   <item>URL-decodes the remainder so that <c>%2E</c>, <c>%5C</c>, etc.
        ///         are resolved to their actual characters.</item>
        ///   <item>Normalises backslashes to forward-slashes so that folder paths
        ///         entered with <c>\</c> are treated identically to <c>/</c>.</item>
        /// </list>
        /// </summary>
        internal static string Normalize(string name)
        {
            if (string.IsNullOrEmpty(name))
                return name ?? string.Empty;

            // Strip encoded '?' (%3F) before decoding, otherwise the decoder turns
            // %3F into '?' and the subsequent Split('?') would catch it — but at the
            // cost of a double-pass.  Doing it first is simpler and correct.
            var withoutEncodedQuery = name.Split(new[] { "%3F", "%3f" },
                StringSplitOptions.None)[0];

            // Now strip the literal query string separator.
            var pathOnly = withoutEncodedQuery.Split('?')[0];

            // Decode remaining percent-encoded characters (spaces, dots, etc.).
            var decoded = Uri.UnescapeDataString(pathOnly);

            // Normalise backslash → forward-slash.
            return decoded.Replace('\\', '/');
        }

        /// <summary>
        /// Strips the outermost recognised suffix from <paramref name="name"/> and
        /// returns flags indicating which suffix was found.
        /// </summary>
        internal static (string name, bool isDebug, bool isXml, bool isApi) Parse(string name)
        {
            var n = Normalize(name);

            if (n.EndsWith(".api", StringComparison.OrdinalIgnoreCase))
                return (n[..^4], false, false, true);
            if (n.EndsWith(".debug", StringComparison.OrdinalIgnoreCase))
                return (n[..^6], true, false, false);
            if (n.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                return (n[..^4], false, true, false);
            if (n.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                return (n[..^5], false, false, false);
            return (n, false, false, false);
        }

        /// <summary>
        /// Returns <c>true</c> when <paramref name="name"/> is, or ends with,
        /// <c>apis.json</c> — indicating the caller wants a workflow-discovery listing
        /// rather than a workflow execution.
        /// </summary>
        internal static bool IsApisJsonRequest(string name) =>
            Normalize(name).EndsWith("apis.json", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Returns <c>true</c> when <paramref name="name"/> ends with the <c>.api</c>
        /// suffix (after normalization) — indicating the caller wants an OpenAPI spec
        /// for the named workflow rather than a workflow execution.
        ///
        /// Distinct from <see cref="IsApisJsonRequest"/>:
        /// <list type="bullet">
        ///   <item><c>workflow.api</c>  → single-workflow OpenAPI spec  (<c>IsApiRequest</c>).</item>
        ///   <item><c>apis.json</c>     → folder/root discovery listing (<see cref="IsApisJsonRequest"/>).</item>
        /// </list>
        /// </summary>
        internal static bool IsApiRequest(string name) =>
            Normalize(name).EndsWith(".api", StringComparison.OrdinalIgnoreCase);

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
            var n = Normalize(name);
            var parts = n.Split(new[] { "/apis.json" }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0 || parts[0].Equals("apis.json", StringComparison.OrdinalIgnoreCase))
                return null;
            return parts[0];
        }
    }
}
