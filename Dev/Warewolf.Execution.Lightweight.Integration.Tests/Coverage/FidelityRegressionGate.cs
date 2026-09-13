/*
 * Baseline-drift gate for the round-trip fidelity sweep.
 *
 * Split out of RoundTripFidelityTests so the comparison rules are unit-testable without
 * paying for the ~60 workflow executions the sweep itself costs, and so the "what counts
 * as a regression" decision lives in one reviewable place.
 *
 * Two things are checked, and they fail for different reasons:
 *
 *   1. Provenance — was the committed baseline produced by THIS harness? A baseline captured
 *      under different execution semantics is not a baseline, it is noise, and comparing
 *      against it produces confident nonsense.
 *   2. Regression — did an activity type that was proven to round-trip stop doing so?
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Warewolf.Execution.Lightweight.Integration.Tests.Coverage
{
    /// <summary>
    /// Verdict for one activity type's round-trip. Namespace-level (rather than nested in the
    /// test class) so the gate and its tests can name these values without duplicating strings.
    /// </summary>
    public enum FidelityStatus
    {
        /// <summary>Original and round-tripped executions both succeeded and agree.</summary>
        Pass,

        /// <summary>
        /// Both executions failed with the SAME error. The round-trip preserved behaviour — it
        /// just wasn't a happy path, usually because the activity needs a live source the sandbox
        /// does not have. Behaviour-preserving, so never a regression, but it does not prove the
        /// workflow body survived either, so it is not <see cref="Pass"/>.
        /// </summary>
        PassBothFailedIdentically,

        /// <summary>No corpus sample contains this activity type. No information either way.</summary>
        NoCorpusSample,

        /// <summary>The converter refused or threw — the round-trip could not be performed.</summary>
        TranslationFailed,

        /// <summary>Both executions ran but disagreed on payload or errors. A genuine fidelity gap.</summary>
        ExecutionMismatch,

        /// <summary>One execution succeeded and the other failed. A genuine fidelity gap.</summary>
        ExecutionAsymmetric,

        /// <summary>
        /// The original does not produce the same result twice, so any difference after
        /// round-tripping proves nothing. No information either way.
        /// </summary>
        NonDeterministic,
    }

    /// <summary>Read-side projection of one report row — the fields the gate compares.</summary>
    public sealed record FidelityRow(string StudioName, string Status, string? SamplePath, string? Detail);

    /// <summary>
    /// The execution configuration a report was produced under. Recorded in the generated
    /// allow-list so a later run can tell whether the committed baseline is comparable to what
    /// it just measured, instead of assuming it is.
    /// </summary>
    public sealed class HarnessProvenance
    {
        [JsonProperty("harnessVersion")]
        public int HarnessVersion { get; set; }

        [JsonProperty("maxSamplesPerType")]
        public int MaxSamplesPerType { get; set; }

        /// <summary>Corpus roots that actually existed when the report was generated.</summary>
        [JsonProperty("corpusRoots")]
        public List<string> CorpusRoots { get; set; } = new();
    }

    /// <summary>Minimal read-side shape of the generated <c>fidelity-allowlist.json</c>.</summary>
    public sealed class BaselineDocument
    {
        [JsonProperty("generatedAtUtc")]
        public DateTime? GeneratedAtUtc { get; set; }

        [JsonProperty("harness")]
        public HarnessProvenance? Harness { get; set; }

        [JsonProperty("results")]
        public List<BaselineEntry>? Results { get; set; }
    }

    public sealed class BaselineEntry
    {
        [JsonProperty("StudioName")]
        public string? StudioName { get; set; }

        [JsonProperty("Status")]
        public string? Status { get; set; }

        [JsonProperty("SamplePath")]
        public string? SamplePath { get; set; }
    }

    /// <summary>Outcome of comparing a run against the committed baseline.</summary>
    public sealed class GateVerdict
    {
        /// <summary>Types that were proven to round-trip and demonstrably no longer do.</summary>
        public List<string> Regressions { get; } = new();

        /// <summary>Types that newly pass. Reported, never failed on.</summary>
        public List<string> Improvements { get; } = new();

        /// <summary>
        /// Types excluded from comparison because the baseline measured them against a corpus
        /// root this run cannot see, so any difference says more about the agent than the converter.
        /// </summary>
        public List<string> Skipped { get; } = new();
    }

    public static class FidelityRegressionGate
    {
        /// <summary>
        /// Bump whenever a change to the sweep alters what a status MEANS for a given activity —
        /// then regenerate and commit the allow-list in the same change.
        ///
        /// <para>
        /// Version 1: source resolution via <c>LightweightSourceLoader.EnsureIndexed</c> on the
        /// sample's real corpus directory, with sub-workflow resolution left scoped to the lone
        /// temp copy (i.e. <c>request.WorkflowsDirectory</c> deliberately NOT set). The two are
        /// not interchangeable: setting <c>WorkflowsDirectory</c> also repoints
        /// <c>LightweightEsbChannel</c> and warms the process-wide <c>WorkflowResourceCache</c>,
        /// which lets sub-workflows resolve but makes the round-tripped copy return an empty
        /// payload. A baseline captured under one and compared under the other reports half a
        /// dozen phantom regressions and improvements, which is exactly what this version stamp
        /// exists to catch.
        /// </para>
        /// </summary>
        public const int CurrentHarnessVersion = 1;

        /// <summary>
        /// The statuses that mean the round-trip demonstrably changed or destroyed the workflow.
        ///
        /// <para>
        /// Deliberately narrower than "anything other than Pass". <c>PassBothFailedIdentically</c>
        /// is behaviour-preserving by definition — both sides failed the same way — and whether a
        /// source-backed activity lands there or on <c>Pass</c> is decided by whether the agent
        /// has a broker or database reachable, not by the converter. <c>NoCorpusSample</c> and
        /// <c>NonDeterministic</c> carry no information at all. Gating on those made the build a
        /// referendum on the agent's environment; gating on these three keeps it a statement
        /// about the converter. The FlowDecision regression this gate was built for was an
        /// <c>ExecutionMismatch</c>, and is still caught.
        /// </para>
        /// </summary>
        static readonly HashSet<string> RegressionStatuses = new(StringComparer.OrdinalIgnoreCase)
        {
            nameof(FidelityStatus.TranslationFailed),
            nameof(FidelityStatus.ExecutionMismatch),
            nameof(FidelityStatus.ExecutionAsymmetric),
        };

        static readonly string PassStatus = nameof(FidelityStatus.Pass);

        /// <summary>
        /// True when <paramref name="currentStatus"/> is a proven loss of fidelity against a
        /// baseline that recorded <see cref="FidelityStatus.Pass"/>.
        /// </summary>
        public static bool IsRegression(string? baselineStatus, string? currentStatus) =>
            string.Equals(baselineStatus, PassStatus, StringComparison.OrdinalIgnoreCase)
            && currentStatus != null
            && RegressionStatuses.Contains(currentStatus);

        /// <summary>True when a type that did not pass now passes.</summary>
        public static bool IsImprovement(string? baselineStatus, string? currentStatus) =>
            !string.Equals(baselineStatus, PassStatus, StringComparison.OrdinalIgnoreCase)
            && string.Equals(currentStatus, PassStatus, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Identifies which corpus root a recorded sample path came from. The path is absolute and
        /// was written on whichever machine generated the baseline, so only the root segment is
        /// portable — everything left of it (a dev checkout, an agent's artifact directory) is not.
        /// Returns the longest matching root, so a multi-segment root is never shadowed by a
        /// shorter one.
        /// </summary>
        /// <param name="knownRoots">
        /// Roots to recognise. Callers pass more than <see cref="RoundTripFidelityCorpus.CorpusRoots"/>
        /// on purpose — see <see cref="Compare"/>: a root that has since been retired from the list is
        /// still named in the paths of any baseline generated before the retirement, and failing to
        /// recognise it there is the difference between skipping those rows and mis-scoring them.
        /// </param>
        public static bool TryGetCorpusRoot(string? samplePath, IEnumerable<string> knownRoots, out string root)
        {
            root = string.Empty;
            if (string.IsNullOrWhiteSpace(samplePath))
            {
                return false;
            }

            var normalised = Normalise(samplePath);
            foreach (var candidate in knownRoots.Distinct(StringComparer.OrdinalIgnoreCase)
                         .OrderByDescending(r => r.Length))
            {
                var needle = Normalise(candidate);
                if (normalised.StartsWith(needle + "\\", StringComparison.OrdinalIgnoreCase) ||
                    normalised.IndexOf("\\" + needle + "\\", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    root = candidate;
                    return true;
                }
            }
            return false;
        }

        /// <summary>Convenience overload recognising only the roots the sweep scans today.</summary>
        public static bool TryGetCorpusRoot(string? samplePath, out string root) =>
            TryGetCorpusRoot(samplePath, RoundTripFidelityCorpus.CorpusRoots, out root);

        static string Normalise(string path) =>
            path.Replace(Path.AltDirectorySeparatorChar, '\\').Replace('/', '\\').TrimEnd('\\');

        /// <summary>
        /// Explains why <paramref name="baseline"/> is not comparable to a run of this harness,
        /// or <c>null</c> when it is. Missing or stale provenance is an authoring mistake — the
        /// baseline and the code that produces it drifted apart — and is fixed by regenerating,
        /// so it is worth failing on rather than quietly comparing anyway.
        /// </summary>
        public static string? DescribeProvenanceMismatch(BaselineDocument? baseline)
        {
            var harness = baseline?.Harness;
            if (harness == null)
            {
                return "The committed fidelity baseline records no 'harness' provenance block, so it " +
                       "cannot be shown to have been generated by this version of the sweep. Re-run " +
                       "this test and commit the regenerated Resources/fidelity-allowlist.json.";
            }

            if (harness.HarnessVersion != CurrentHarnessVersion)
            {
                return "The committed fidelity baseline was generated by harness version " +
                       harness.HarnessVersion + ", but this is version " + CurrentHarnessVersion +
                       ". A status recorded under different execution semantics is not comparable — " +
                       "re-run this test and commit the regenerated Resources/fidelity-allowlist.json.";
            }

            return null;
        }

        /// <summary>
        /// Compares one run against the committed baseline.
        /// </summary>
        /// <param name="baseline">The committed allow-list.</param>
        /// <param name="current">This run's results.</param>
        /// <param name="corpusRootsPresent">
        /// Corpus roots this run could actually see. A baseline row whose sample came from a root
        /// that is now absent is skipped rather than scored: the two runs did not measure the same
        /// workflow, so neither "regressed" nor "improved" is a true statement about it.
        /// </param>
        public static GateVerdict Compare(
            BaselineDocument? baseline,
            IReadOnlyList<FidelityRow> current,
            IReadOnlyCollection<string> corpusRootsPresent)
        {
            var verdict = new GateVerdict();
            if (baseline?.Results == null)
            {
                return verdict;
            }

            var byName = current
                .GroupBy(r => r.StudioName, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
            var rootsPresent = new HashSet<string>(corpusRootsPresent, StringComparer.OrdinalIgnoreCase);

            // Recognise the roots the baseline says it was generated against as well as the ones
            // scanned today. A root dropped from CorpusRoots still appears in the sample paths of
            // every baseline written before the drop; without this it would go unrecognised, and an
            // unrecognised root is treated as "comparable" — which is exactly backwards, since a
            // retired root guarantees the two runs measured different samples.
            var knownRoots = RoundTripFidelityCorpus.CorpusRoots
                .Concat(baseline.Harness?.CorpusRoots ?? Enumerable.Empty<string>())
                .ToList();

            foreach (var previous in baseline.Results)
            {
                if (string.IsNullOrWhiteSpace(previous.StudioName) ||
                    !byName.TryGetValue(previous.StudioName!, out var now))
                {
                    // Retired or renamed toolbox entry — the subset table is the source of truth.
                    continue;
                }

                if (TryGetCorpusRoot(previous.SamplePath, knownRoots, out var baselineRoot) &&
                    !rootsPresent.Contains(baselineRoot))
                {
                    verdict.Skipped.Add(
                        "  " + previous.StudioName + ": baseline measured '" + baselineRoot +
                        "', which this run cannot see (baseline " + previous.Status +
                        ", now " + now.Status + ").");
                    continue;
                }

                if (IsRegression(previous.Status, now.Status))
                {
                    verdict.Regressions.Add(
                        "  " + previous.StudioName + ": Pass -> " + now.Status + ". " +
                        Truncate(now.Detail ?? string.Empty));
                }
                else if (IsImprovement(previous.Status, now.Status))
                {
                    verdict.Improvements.Add("  " + previous.StudioName + ": " + previous.Status + " -> Pass");
                }
            }

            return verdict;
        }

        /// <summary>The assertion message for a verdict carrying regressions.</summary>
        public static string BuildFailureMessage(GateVerdict verdict) =>
            "Round-trip fidelity REGRESSED for " + verdict.Regressions.Count + " activity type(s) that the " +
            "committed baseline records as Pass:" + Environment.NewLine +
            string.Join(Environment.NewLine, verdict.Regressions) + Environment.NewLine +
            "A workflow using these can no longer be edited losslessly, so bodyEditable would start " +
            "reporting false for them. Fix the converter, or — if the change is intended — re-run this " +
            "test and commit the regenerated Resources/fidelity-allowlist.json to re-baseline.";

        static string Truncate(string s, int max = 400) =>
            string.IsNullOrEmpty(s) ? "(empty)" : s.Length <= max ? s : s.Substring(0, max) + "…";
    }
}
