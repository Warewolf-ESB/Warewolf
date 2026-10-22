/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Dev2;

namespace Warewolf.Bite
{
    /// <summary>
    /// The outcome of reading one workflow with <see cref="BiteReader"/> alongside the WF path. Holds difference
    /// <em>paths</em> and short hashes of the two values, never the values: activity properties can hold connection
    /// strings and passwords, and this goes to logs.
    /// </summary>
    public sealed class BiteShadowResult
    {
        public string ResourceId { get; init; } = "";
        public bool Matched => CandidateError == null && DifferenceCount == 0;
        public int DifferenceCount { get; init; }

        /// <summary>The first <see cref="BiteShadow.MaxReportedDifferences"/> differences as <c>path [hash != hash]</c>.</summary>
        public IReadOnlyList<string> Differences { get; init; } = Array.Empty<string>();

        /// <summary>Set when the WF-free reader (or the comparison) failed; the type and message only.</summary>
        public string? CandidateError { get; init; }

        public double CandidateMilliseconds { get; init; }
        public double CompareMilliseconds { get; init; }

        public override string ToString() => Matched
            ? $"BiteShadow match {ResourceId} (read {CandidateMilliseconds:F1} ms, compare {CompareMilliseconds:F1} ms)"
            : CandidateError != null
                ? $"BiteShadow candidate error {ResourceId}: {CandidateError}"
                : $"BiteShadow MISMATCH {ResourceId}: {DifferenceCount} difference(s): {string.Join("; ", Differences)}";
    }

    /// <summary>
    /// Shadow mode (WF removal T1.7): after the WF path has produced a workflow's graph, read the same XAML with
    /// <see cref="BiteReader"/> and compare. The WF graph is always the one executed. Never throws: any failure of
    /// the new reader or the comparison becomes <see cref="BiteShadowResult.CandidateError"/>.
    /// </summary>
    public static class BiteShadow
    {
        public const string EnabledVariable = "WAREWOLF_BITE_SHADOW";
        public const int MaxReportedDifferences = 20;

        static readonly BiteReader Reader = new BiteReader();

        /// <summary>True when <c>WAREWOLF_BITE_SHADOW</c> is <c>true</c> or <c>1</c>. Off by default.</summary>
        public static bool IsEnabled() => IsEnabled(Environment.GetEnvironmentVariable(EnabledVariable));

        public static bool IsEnabled(string? value) =>
            string.Equals(value?.Trim(), "true", StringComparison.OrdinalIgnoreCase) || value?.Trim() == "1";

        public static BiteShadowResult Compare(IDev2Activity? reference, string xamlDefinition, string resourceId)
        {
            var watch = Stopwatch.StartNew();
            IDev2Activity? candidate;
            try
            {
                candidate = Reader.ReadXaml(xamlDefinition, resourceId);
            }
            catch (Exception e)
            {
                return new BiteShadowResult { ResourceId = resourceId, CandidateError = Describe(e), CandidateMilliseconds = watch.Elapsed.TotalMilliseconds };
            }
            var readMs = watch.Elapsed.TotalMilliseconds;

            try
            {
                watch.Restart();
                var differences = ActivityGraphDiff.Compare(reference, candidate)
                    .Where(d => !MintedIds.IsMintedIdDifference(d, xamlDefinition))
                    .ToList();
                return new BiteShadowResult
                {
                    ResourceId = resourceId,
                    DifferenceCount = differences.Count,
                    Differences = differences.Take(MaxReportedDifferences).Select(Redact).ToList(),
                    CandidateMilliseconds = readMs,
                    CompareMilliseconds = watch.Elapsed.TotalMilliseconds,
                };
            }
            catch (Exception e)
            {
                return new BiteShadowResult { ResourceId = resourceId, CandidateError = "compare: " + Describe(e), CandidateMilliseconds = readMs };
            }
        }

        static string Describe(Exception e) => $"{e.GetType().Name}: {e.Message}";

        static string Redact(GraphDifference difference) => $"{difference.Path} [{Hash(difference.Expected)} != {Hash(difference.Actual)}]";

        static string Hash(string? value)
        {
            if (value == null)
            {
                return "absent";
            }
            using (var sha = SHA256.Create())
            {
                return Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(value))).Substring(0, 8);
            }
        }
    }

    /// <summary>The server-side hook: registered in place of the no-op when shadow mode is enabled.</summary>
    public sealed class BiteShadowParseObserver : IWorkflowParseObserver
    {
        readonly Action<BiteShadowResult> _report;

        public BiteShadowParseObserver(Action<BiteShadowResult> report)
        {
            _report = report ?? throw new ArgumentNullException(nameof(report));
        }

        public void Parsed(Guid resourceId, StringBuilder xamlDefinition, IDev2Activity result)
        {
            try
            {
                _report(BiteShadow.Compare(result, xamlDefinition.ToString(), resourceId.ToString()));
            }
            catch
            {
                // Shadow mode must never affect execution, including through a failing sink.
            }
        }
    }
}
