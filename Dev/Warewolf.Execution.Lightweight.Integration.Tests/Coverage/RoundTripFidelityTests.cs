/*
 * Round-trip fidelity gate for Dev2.Activities.WF.WorkflowToX6Converter /
 * X6ToWorkflowConverter — the gate that must pass before the `bodyEditable`
 * rule can be trusted.
 *
 * Question this answers, per activity type in the toolbox subset table:
 *   Does XAML --ConvertToX6Json--> X6 JSON --X6JsonToWorkflow--> XAML' produce a workflow
 *   that executes IDENTICALLY (same success/failure, same JSON payload) to the original?
 *
 * This is primarily an *informational discovery* pass: the goal is to produce the empirical
 * `fidelity-allowlist.json` artifact the MCP `bodyEditable` logic will consume, not to block
 * CI on activity types that are not yet supported. A per-type mismatch is recorded and
 * reported, never asserted on for its own sake — an activity that has never round-tripped
 * cannot fail this build.
 *
 * There are exactly two ways it does fail. The harness being broken (e.g. it finds zero corpus
 * at all when the corpus tree is known to be deployed), and an activity type the committed
 * baseline proved to round-trip demonstrably losing that property — see
 * AssertNoRegressionAgainstCommittedBaseline and FidelityRegressionGate, which decide what
 * "demonstrably" means and refuse to compare against a baseline that is not comparable.
 *
 * Real corpus samples are classified by RoundTripFidelityCorpus (see that file). Every
 * result is written to Console (captured in test output/CI logs) and to
 *   <repo>/Dev/Warewolf.Execution.Lightweight/Resources/fidelity-allowlist.json
 * so it can be reviewed and committed once the run looks representative.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Dev2.Activities.WF;
using Dev2.Common.X6;
using Dev2.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Warewolf.Execution.Lightweight.Logging;
using Warewolf.Execution.Lightweight.Models;

namespace Warewolf.Execution.Lightweight.Integration.Tests.Coverage
{
    [TestClass]
    [TestCategory("RoundTripFidelity")]
    public class RoundTripFidelityTests
    {
        sealed class FidelityResult
        {
            public string StudioName { get; set; }
            public string Category { get; set; }
            public bool RequiresSource { get; set; }
            public string SamplePath { get; set; }
            public string Status { get; set; }
            public string Detail { get; set; }
        }

        sealed class NoOpLogger : IExecutionLogger
        {
            public void LogDebug(string message, Guid executionId) { }
            public void LogDebug(string message, Exception exception, Guid executionId) { }
            public void LogInfo(string message, Guid executionId) { }
            public void LogInfo(string message, Exception exception, Guid executionId) { }
            public void LogInfo(string message) { }
            public void LogWarning(string message, Guid executionId) { }
            public void LogWarning(string message, Exception exception, Guid executionId) { }
            public void LogError(string message, Guid executionId) { }
            public void LogError(string activityName, Exception ex, Guid executionId) { }
            public void LogError(Exception ex, string log) { }
            public void LogFatal(string message, Guid executionId) { }
            public void LogFatal(string message, Exception exception, Guid executionId) { }
            public void LogTrace(string message, Guid executionId) { }
            public void LogTrace(string message, Exception exception, Guid executionId) { }
        }

        static IWorkflowExecutor CreateExecutor() => new WorkflowExecutor(new NoOpLogger());

        /// <summary>
        /// How many corpus samples to evaluate per toolbox entry. Every sample costs two workflow
        /// executions, so this is a deliberate bound on runtime rather than an exhaustive sweep —
        /// the corpus holds 139 samples for Assign alone.
        /// </summary>
        const int MaxSamplesPerType = 8;

        /// <summary>
        /// Bound to every declared input. "1" is chosen because it is simultaneously a valid number,
        /// a valid non-empty string and a valid index, so it satisfies far more activities than an
        /// empty or purely alphabetic value would.
        /// </summary>
        const string SyntheticInputValue = "1";

        /// <summary>
        /// Matches a Warewolf *scalar* variable reference. Recordset and object expressions
        /// (<c>[[rs().field]]</c>, <c>[[@obj.member]]</c>) are deliberately excluded: binding those
        /// needs shape information the DataList alone does not carry.
        /// </summary>
        static readonly Regex ScalarReferenceRegex =
            new(@"\[\[([A-Za-z_][A-Za-z0-9_]*)\]\]", RegexOptions.Compiled);

        /// <summary>Matches the whole &lt;DataList&gt; block, including the self-closing empty form.</summary>
        static readonly Regex DataListBlockRegex =
            new(@"<DataList\s*/>|<DataList[^>]*>.*?</DataList>",
                RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);

        static readonly Dictionary<string, string> NoInputs = new();

        /// <summary>
        /// Preference order used only to break ties between samples that all failed to prove a pass.
        /// Genuine behavioural gaps are NOT ranked here — see <see cref="SelectRepresentative"/>.
        /// </summary>
        static int Rank(string status) =>
            status == FidelityStatus.Pass.ToString() ? 0
            : status == FidelityStatus.PassBothFailedIdentically.ToString() ? 1
            : status == FidelityStatus.NonDeterministic.ToString() ? 2
            : status == FidelityStatus.TranslationFailed.ToString() ? 3
            : 4;

        /// <summary>
        /// Reduces the per-sample verdicts for one activity to the single result reported for it.
        ///
        /// <para>
        /// A genuine behavioural gap in <em>any</em> sample wins outright. That asymmetry is
        /// deliberate: <c>ExecutionMismatch</c>/<c>ExecutionAsymmetric</c> mean the round-trip
        /// demonstrably changed what the workflow does, and letting a passing sample mask that would
        /// turn this report into exactly the false reassurance the fidelity gate exists to prevent.
        /// Absent such a gap, one sample proving a clean round-trip is enough.
        /// </para>
        /// </summary>
        static FidelityResult SelectRepresentative(List<FidelityResult> attempts)
        {
            var genuineGap = attempts.FirstOrDefault(a =>
                a.Status == FidelityStatus.ExecutionMismatch.ToString() ||
                a.Status == FidelityStatus.ExecutionAsymmetric.ToString());
            if (genuineGap != null)
            {
                return genuineGap;
            }

            var passed = attempts.FirstOrDefault(a => a.Status == FidelityStatus.Pass.ToString());
            if (passed != null)
            {
                return passed;
            }

            var best = attempts.OrderBy(a => Rank(a.Status)).First();
            if (attempts.Count > 1)
            {
                best.Detail = $"Best of {attempts.Count} corpus samples. " + best.Detail;
            }
            return best;
        }

        /// <summary>
        /// Returns the .bite text with every scalar the workflow references but does not declare
        /// added to its DataList as an Input, together with the input values to bind at execution.
        /// Declaring the variable is a design-time change to the workflow definition; binding a value
        /// is the execution-time counterpart, and both are applied identically to the original and
        /// the round-tripped copy.
        /// </summary>
        static (string BiteText, Dictionary<string, string> Inputs) PrepareInputs(string biteText)
        {
            var inputs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var dataListMatch = DataListBlockRegex.Match(biteText);
            if (!dataListMatch.Success)
            {
                return (biteText, inputs);
            }

            var dataList = XElement.Parse(dataListMatch.Value, LoadOptions.PreserveWhitespace);

            var declared = new HashSet<string>(
                dataList.Elements().Select(e => e.Name.LocalName), StringComparer.OrdinalIgnoreCase);

            foreach (Match match in ScalarReferenceRegex.Matches(biteText))
            {
                var name = match.Groups[1].Value;
                if (!declared.Add(name))
                {
                    continue;
                }
                var declaration = new XElement(name);
                declaration.SetAttributeValue("Description", "");
                declaration.SetAttributeValue("IsEditable", "True");
                declaration.SetAttributeValue("ColumnIODirection", "Input");
                dataList.Add(declaration);
            }

            foreach (var element in dataList.Elements())
            {
                // A recordset declares its fields as child elements; only scalars are bound here.
                if (element.HasElements)
                {
                    continue;
                }
                var direction = (string)element.Attribute("ColumnIODirection") ?? "None";
                if (direction.Equals("Input", StringComparison.OrdinalIgnoreCase) ||
                    direction.Equals("Both", StringComparison.OrdinalIgnoreCase))
                {
                    inputs[element.Name.LocalName] = SyntheticInputValue;
                }
            }

            // Splice the amended DataList back in textually. Re-serialising the whole .bite via
            // XElement would also rewrite the escaped XamlDefinition, and that silently corrupted
            // samples whose XAML then failed to convert at all (observed: "An item with the same key
            // has already been added" out of the X6 converter). Only the DataList may change here.
            var amended = biteText.Substring(0, dataListMatch.Index)
                          + dataList.ToString(SaveOptions.DisableFormatting)
                          + biteText.Substring(dataListMatch.Index + dataListMatch.Length);
            return (amended, inputs);
        }

        [TestMethod]
        public async Task RoundTripFidelity_AcrossToolboxSubset_GeneratesAllowListReport()
        {
            var devRoot = RoundTripFidelityCorpus.FindRepoDevRoot();
            if (devRoot == null)
            {
                Assert.Inconclusive(
                    "Could not locate the Dev/ corpus root (Resources - Release, Server Tests Setup, etc.) " +
                    "from " + AppContext.BaseDirectory + ". Run from a full checkout to enable this test.");
                return;
            }

            // Captured once, before the report directory is created: WriteReport creates
            // <devRoot>/Warewolf.Execution.Lightweight/Resources, which is itself a corpus root,
            // so computing this later would claim a root that contributed no samples to this run.
            var presentRoots = RoundTripFidelityCorpus.PresentCorpusRoots(devRoot);
            var biteFiles = RoundTripFidelityCorpus.DiscoverBiteFiles(devRoot);
            if (biteFiles.Count == 0)
            {
                Assert.Inconclusive("No .bite files found under known corpus roots at " + devRoot);
                return;
            }

            var classified = RoundTripFidelityCorpus.ClassifyCorpus(biteFiles, MaxSamplesPerType);
            var results = new List<FidelityResult>();

            foreach (var entry in RoundTripFidelityCorpus.ToolboxSubset)
            {
                var samples = classified[entry.StudioName];
                if (samples.Count == 0)
                {
                    results.Add(new FidelityResult
                    {
                        StudioName = entry.StudioName,
                        Category = entry.Category,
                        RequiresSource = entry.RequiresSource,
                        SamplePath = null,
                        Status = FidelityStatus.NoCorpusSample.ToString(),
                        Detail = "No real .bite sample found containing " + string.Join("/", entry.SearchTokens) +
                                 ". Needs a synthetic fixture to cover this activity type."
                    });
                    continue;
                }

                // Evaluate every candidate sample, not just the first. A single sample is a poor
                // proxy for "can this activity round-trip": the corpus sample that happens to sort
                // first often fails for reasons that have nothing to do with the activity under test
                // (most commonly it also contains some *other*, legacy activity the X6 converter does
                // not support, which aborts the whole conversion). See SelectRepresentative for how
                // the per-sample verdicts are reduced to one result.
                //
                // Suspend Execution's own logic (SuspendExecutionActivity.Execute) needs a real,
                // reachable Hangfire SqlServerStorage connection to get past scheduling and actually
                // prove its round-trip fidelity — without one, both the original and round-tripped
                // executions fail identically before the activity's own logic ever runs. Scoped to
                // just this entry so no other toolbox type's execution semantics change; see
                // SuspendExecutionPersistenceSupport's remarks for why this is safe without a live
                // Hangfire worker.
                using var persistence = entry.StudioName == "Suspend Execution"
                    ? SuspendExecutionPersistenceSupport.SwapToRealHangfireSqlServer()
                    : null;

                var attempts = new List<FidelityResult>();
                foreach (var sample in samples)
                {
                    attempts.AddRange(await EvaluateSampleAsync(entry, sample));
                    if (attempts.Any(a => a.Status == FidelityStatus.Pass.ToString()))
                    {
                        // One clean round-trip is all this gate asks for; the remaining samples would
                        // cost two workflow executions each to re-prove the same thing.
                        break;
                    }
                }

                results.Add(SelectRepresentative(attempts));
            }

            WriteReport(devRoot, results, presentRoots);

            // Harness-integrity assertions only — never assert on individual fidelity outcomes here.
            Assert.IsTrue(results.Count == RoundTripFidelityCorpus.ToolboxSubset.Count,
                "Expected one result row per toolbox entry.");

            AssertCorpusCoversEveryTool(results, presentRoots);

            AssertNoRegressionAgainstCommittedBaseline(results, presentRoots);
        }

        /// <summary>
        /// Coverage gate: every toolbox type must be measured against at least one corpus sample,
        /// and every declared corpus root must actually be on disk.
        ///
        /// <para>
        /// This is a harness-integrity assertion, not a fidelity one — it says nothing about whether
        /// a type round-trips, only that the sweep looked. It exists because the two ways this
        /// harness silently under-measures are indistinguishable from a healthy run in the report:
        /// a corpus root that was never staged, and a <see cref="ToolboxEntry.SearchTokens"/> entry
        /// naming a class that does not exist. Both surface as <c>NoCorpusSample</c>, which the old
        /// "at least one type has a sample" check happily tolerated for the other 65.
        /// </para>
        ///
        /// <para>
        /// Both failures were real. On the 2026-08-23 CI allow-list, 10 of 66 types reported
        /// <c>NoCorpusSample</c>: <c>Warewolf.Execution.Lightweight\Resources</c> was declared in
        /// <see cref="RoundTripFidelityCorpus.CorpusRoots"/> but never staged into the TestBinaries
        /// artifact (so every purpose-built fixture was invisible to CI, and Suspend Execution
        /// reported no sample despite having a committed one); <c>*.xml</c> resources were not
        /// discovered at all (hiding <c>All Tools.xml</c>, which alone covers 51 types); and three
        /// rows named classes that do not exist — <c>DsfManualResumptionActivity</c>,
        /// <c>FileWriteWithBase64</c>, and a Calculate row missing its <c>DsfCalculateActivity</c>
        /// alias.
        /// </para>
        /// </summary>
        static void AssertCorpusCoversEveryTool(List<FidelityResult> results, List<string> presentRoots)
        {
            var missingRoots = RoundTripFidelityCorpus.CorpusRoots
                .Where(root => !presentRoots.Contains(root))
                .ToList();
            Assert.AreEqual(0, missingRoots.Count,
                "Declared corpus root(s) absent from this run: " + string.Join(", ", missingRoots) + "." +
                Environment.NewLine +
                "The sweep measured a smaller corpus than it claims to. If this is CI, the root was not " +
                "staged into the TestBinaries artifact — see the corpus-staging block in Compile.ps1. " +
                "A run against a partial corpus must not produce a baseline.");

            var uncovered = results
                .Where(r => r.Status == FidelityStatus.NoCorpusSample.ToString())
                .Select(r => r.StudioName)
                .ToList();
            Assert.AreEqual(0, uncovered.Count,
                "No corpus sample found for " + uncovered.Count + " of " + results.Count +
                " toolbox type(s): " + string.Join(", ", uncovered) + "." + Environment.NewLine +
                "Every toolbox type must be exercised by at least one workflow resource under a corpus " +
                "root. Either the type's SearchTokens name a class that does not exist (check the real " +
                "class name under Dev2.Activities\\Activities), or the type genuinely has no sample and " +
                "needs a purpose-built fixture committed under " +
                "Warewolf.Execution.Lightweight\\Resources\\tools\\<tool>\\.");
        }

        /// <summary>
        /// Drift gate: fails when an activity type that the committed
        /// <c>fidelity-allowlist.json</c> records as <c>Pass</c> demonstrably stops round-tripping.
        ///
        /// <para>
        /// This is the one place the suite does assert on fidelity outcomes, and it is deliberately
        /// one-directional and narrow. The file header's "informational discovery pass" rule still
        /// holds for *discovering* what a type's status is — a type that has never passed is never
        /// asserted on, so unsupported activities cannot block CI. What is asserted is that a type
        /// which HAS been proven to round-trip does not silently stop doing so, which is exactly the
        /// regression that went unnoticed: FlowDecision was flattened into a non-branching FlowStep,
        /// and the only signal was a row quietly changing status in a generated file nobody diffed.
        /// </para>
        ///
        /// <para>
        /// Which transitions count as a regression, and why a row can be skipped from comparison
        /// entirely, live in <see cref="FidelityRegressionGate"/> so they can be unit-tested without
        /// paying for this sweep's ~60 workflow executions.
        /// </para>
        ///
        /// <para>
        /// The baseline is read from the copy shipped beside the test binaries
        /// (<see cref="AppContext.BaseDirectory"/><c>/Resources/fidelity-allowlist.json</c>) rather
        /// than from <paramref name="results"/>' own output path, because the run has already
        /// overwritten that path by this point. That copy comes from the committed file via the
        /// build, and is present both in a local checkout and in the CI test-binaries artifact.
        /// </para>
        ///
        /// <para>
        /// Improvements are never failures: a type moving to <c>Pass</c> is reported and welcomed.
        /// When a regression is genuine and accepted, re-run this test to regenerate the allow-list
        /// and commit it — that re-baselines the gate deliberately, in a reviewable diff.
        /// </para>
        /// </summary>
        static void AssertNoRegressionAgainstCommittedBaseline(
            List<FidelityResult> results, IReadOnlyCollection<string> presentRoots)
        {
            var baselinePath = Path.Combine(AppContext.BaseDirectory, "Resources", "fidelity-allowlist.json");
            if (!File.Exists(baselinePath))
            {
                Assert.Inconclusive(
                    "No committed fidelity baseline found at " + baselinePath +
                    " — cannot check for regressions. Commit Resources/fidelity-allowlist.json to enable this gate.");
                return;
            }

            BaselineDocument? baseline;
            try
            {
                baseline = JsonConvert.DeserializeObject<BaselineDocument>(File.ReadAllText(baselinePath));
            }
            catch (Exception ex)
            {
                Assert.Inconclusive($"Committed fidelity baseline at {baselinePath} could not be parsed: {ex.Message}");
                return;
            }

            if (baseline?.Results == null || baseline.Results.Count == 0)
            {
                Assert.Inconclusive($"Committed fidelity baseline at {baselinePath} contains no results.");
                return;
            }

            // Checked before any comparison: a baseline captured under different execution semantics
            // produces confident nonsense in both directions, so there is nothing to learn from
            // comparing against it. This is an authoring mistake (the allow-list and the code that
            // generates it drifted apart), not an environment problem, so it fails rather than skips.
            var provenanceMismatch = FidelityRegressionGate.DescribeProvenanceMismatch(baseline);
            Assert.IsNull(provenanceMismatch, provenanceMismatch + " Baseline: " + baselinePath);

            var verdict = FidelityRegressionGate.Compare(
                baseline,
                results
                    .Select(r => new FidelityRow(r.StudioName, r.Status, r.SamplePath, r.Detail))
                    .ToList(),
                presentRoots);

            if (verdict.Skipped.Count > 0)
            {
                Console.WriteLine(
                    "Not compared — " + verdict.Skipped.Count + " activity type(s) were baselined against a " +
                    "corpus root this run cannot see:");
                verdict.Skipped.ForEach(Console.WriteLine);
            }

            if (verdict.Improvements.Count > 0)
            {
                Console.WriteLine("Round-trip fidelity IMPROVED for " + verdict.Improvements.Count + " activity type(s):");
                verdict.Improvements.ForEach(Console.WriteLine);
                Console.WriteLine("Commit the regenerated fidelity-allowlist.json to lock these in.");
            }

            Assert.AreEqual(0, verdict.Regressions.Count, FidelityRegressionGate.BuildFailureMessage(verdict));
        }

        /// <summary>
        /// Evaluates one corpus sample and returns every attempt made against it.
        ///
        /// <para>
        /// Two attempts are made, in order. First the workflow exactly as authored, with nothing
        /// bound — this is the ground truth, and if it already proves a clean round-trip there is
        /// nothing more to learn. Only if that does not pass are inputs declared and bound, because
        /// binding is not free of side effects: a synthetic value is a poor stand-in for a path or a
        /// connection string, and forcing one can push a workflow that previously ran into failing
        /// (observed: Delete Records, whose sample passed unbound and failed once "1" was bound to
        /// its path variable). Returning both attempts lets <see cref="SelectRepresentative"/> keep
        /// the better outcome, so binding can only ever add evidence, never destroy it.
        /// </para>
        /// </summary>
        async Task<List<FidelityResult>> EvaluateSampleAsync(ToolboxEntry entry, string samplePath)
        {
            var attempts = new List<FidelityResult>();

            string rawFileText;
            try
            {
                rawFileText = File.ReadAllText(samplePath, Encoding.UTF8);
            }
            catch (IOException ex)
            {
                attempts.Add(new FidelityResult
                {
                    StudioName = entry.StudioName,
                    Category = entry.Category,
                    RequiresSource = entry.RequiresSource,
                    SamplePath = samplePath,
                    Status = FidelityStatus.TranslationFailed.ToString(),
                    Detail = "Could not read sample file: " + ex.Message,
                });
                return attempts;
            }

            var unbound = await EvaluateOnceAsync(entry, samplePath, rawFileText, NoInputs, "as authored");
            attempts.Add(unbound);
            if (unbound.Status == FidelityStatus.Pass.ToString())
            {
                return attempts;
            }

            string boundFileText;
            Dictionary<string, string> inputParameters;
            try
            {
                (boundFileText, inputParameters) = PrepareInputs(rawFileText);
            }
            catch (Exception ex)
            {
                attempts.Add(new FidelityResult
                {
                    StudioName = entry.StudioName,
                    Category = entry.Category,
                    RequiresSource = entry.RequiresSource,
                    SamplePath = samplePath,
                    Status = FidelityStatus.TranslationFailed.ToString(),
                    Detail = "Could not prepare DataList inputs: " + ex.GetType().Name + ": " + ex.Message,
                });
                return attempts;
            }

            if (inputParameters.Count > 0)
            {
                attempts.Add(await EvaluateOnceAsync(
                    entry, samplePath, boundFileText, inputParameters, "with inputs bound"));
            }

            return attempts;
        }

        async Task<FidelityResult> EvaluateOnceAsync(
            ToolboxEntry entry,
            string samplePath,
            string originalFileText,
            Dictionary<string, string> inputParameters,
            string mode)
        {
            var result = new FidelityResult
            {
                StudioName = entry.StudioName,
                Category = entry.Category,
                RequiresSource = entry.RequiresSource,
                SamplePath = samplePath,
            };

            var fileContents = new StringBuilder(originalFileText);
            var (xamlDefinition, _, _) = WorkflowExecutor.ExtractWorkflowParts(fileContents);
            if (xamlDefinition == null || xamlDefinition.Length == 0)
            {
                result.Status = FidelityStatus.TranslationFailed.ToString();
                result.Detail = "Sample has no XamlDefinition to convert.";
                return result;
            }

            string roundTrippedXaml;
            try
            {
                roundTrippedXaml = X6RoundTripBridge.RoundTripXaml(xamlDefinition);
            }
            catch (Exception ex)
            {
                result.Status = FidelityStatus.TranslationFailed.ToString();
                result.Detail = ex.GetType().Name + ": " + ex.Message;
                return result;
            }

            string roundTrippedBiteText;
            try
            {
                roundTrippedBiteText = BuildBiteWithReplacedXaml(originalFileText, roundTrippedXaml);
            }
            catch (Exception ex)
            {
                result.Status = FidelityStatus.TranslationFailed.ToString();
                result.Detail = "Failed to splice round-tripped XAML back into a .bite file: " + ex.Message;
                return result;
            }

            var originalPath = CopyToTemp(originalFileText, Path.GetFileName(samplePath));
            var roundTrippedPath = CopyToTemp(roundTrippedBiteText, Path.GetFileName(samplePath));

            // Sub-workflow resolution, and ONLY sub-workflow resolution, needs a second resource
            // reachable from the workflow under test. LightweightEsbChannel.ExecuteSubRequest looks
            // the callee up through WorkflowResourceCache.Resolve(_workflowBaseDirectory, ...), and
            // _workflowBaseDirectory falls back to the directory of the file being executed - which
            // is a fresh temp directory holding exactly one file, so the callee is never there and
            // the row could only ever report PassBothFailedIdentically on "Sub-workflow ... not
            // found".
            //
            // Staging the callee INTO each temp directory is deliberately not the same thing as
            // setting request.WorkflowsDirectory, which the comment above rejects: that property is
            // also handed to LightweightEsbChannel's constructor, whose WarmUp would give the
            // original and round-tripped copies a SHARED cache scope over the real corpus folder.
            // Here each copy keeps its own private directory and its own private callee, and
            // WorkflowResourceCache indexes per full directory path, so the two remain as isolated
            // as they were before.
            //
            // Scoped twice over. To the one row that needs it, following the same per-entry pattern
            // SuspendExecutionPersistenceSupport uses above; and to generated fixtures, whose folder
            // holds only the fixture and its companions. The real corpus sample for this row sits in
            // 'Resources - Release\Resources\Examples' beside several hundred unrelated workflows,
            // and copying those into a temp directory per execution would be both slow and exactly
            // the broad shared cache scope this is avoiding.
            if (entry.StudioName == "Service (sub-workflow)" &&
                RoundTripFidelityCorpus.IsGeneratedFixture(samplePath))
            {
                CopyCompanionResources(samplePath, originalPath);
                CopyCompanionResources(samplePath, roundTrippedPath);
            }

            // Each temp copy sits alone in a fresh directory, so WorkflowExecutor's
            // `request.WorkflowsDirectory ?? Path.GetDirectoryName(request.WorkflowFilePath)`
            // fallback hands LightweightSourceLoader.EnsureIndexed an empty folder and no SourceId
            // can ever resolve. Every source-backed activity then failed on its missing source
            // ("The web source has an incomplete web address", "An invalid request URI was
            // provided", ...) instead of exercising the activity, which says nothing about
            // round-trip fidelity.
            //
            // Index the sample's real corpus directory directly rather than by setting
            // request.WorkflowsDirectory. That property is NOT source-indexing-only: it is also
            // handed to `new LightweightEsbChannel(...)` (WorkflowExecutor.cs), whose constructor
            // warms the process-wide, name-keyed WorkflowResourceCache over the entire directory —
            // and would give the original and round-tripped copies a shared cache scope they do
            // not have today. Doing so regressed Assign/Decision/Comment/Count Records from Pass
            // to ExecutionMismatch (the round-tripped copy returned an empty payload) while fixing
            // the web tools, so the two effects are deliberately separated here: take the source
            // index, leave sub-workflow resolution scoped to the temp copy exactly as before.
            //
            // The index is registered on a singleton and keyed by directory, so one call covers
            // both executions below and the IsSelfConsistentAsync re-run.
            var sourcesDirectory = Path.GetDirectoryName(samplePath) ?? string.Empty;
            if (!string.IsNullOrEmpty(sourcesDirectory))
            {
                LightweightSourceLoader.Instance.EnsureIndexed(sourcesDirectory);
            }
            try
            {
                var executor = CreateExecutor();
                var originalExec = executor.Execute(new WorkflowExecutionRequest
                {
                    WorkflowFilePath = originalPath,
                    ReturnType = EmitionTypes.JSON,
                    InputParameters = inputParameters
                });
                var roundTrippedExec = executor.Execute(new WorkflowExecutionRequest
                {
                    WorkflowFilePath = roundTrippedPath,
                    ReturnType = EmitionTypes.JSON,
                    InputParameters = inputParameters
                });

                var originalPayload = await originalExec.ReadPayloadAsync();
                var roundTrippedPayload = await roundTrippedExec.ReadPayloadAsync();

                // Before blaming the converter for any difference, prove the workflow is stable
                // against itself. Anything built on Random, the current date/time, or a live external
                // service returns something different on every run, so comparing one execution to
                // another cannot say anything about round-trip fidelity — Dice Roll.bite rolled a 3
                // then a 4 and was duly reported as an ExecutionMismatch. Re-running the original is
                // only paid for when the two executions actually disagreed, which is rare.
                var disagrees = originalExec.IsSuccess != roundTrippedExec.IsSuccess
                                || originalPayload != roundTrippedPayload
                                || !originalExec.Errors.SequenceEqual(roundTrippedExec.Errors);
                if (disagrees && !await IsSelfConsistentAsync(
                        originalPath, originalExec.IsSuccess, originalPayload, inputParameters))
                {
                    result.Status = FidelityStatus.NonDeterministic.ToString();
                    result.Detail = "The original workflow does not produce the same result twice, so a " +
                                    "difference after round-tripping proves nothing about the converter. " +
                                    "Proving this activity needs a deterministic fixture (seeded/frozen " +
                                    "inputs, or a stubbed source). Original payload: " + Truncate(originalPayload);
                    return result;
                }

                if (!originalExec.IsSuccess && !roundTrippedExec.IsSuccess)
                {
                    // Fidelity means "behaves the same", not "succeeds". Both sides failing with the
                    // exact same error (e.g. missing external source, missing required input under
                    // the empty InputParameters used here) is a PASS for fidelity purposes — the
                    // round-trip preserved behaviour, it just wasn't a happy-path execution.
                    // Errors are normalised to strip the per-run temp directory (unique per copy,
                    // per WorkflowExecutor's DynamicActivity cache key) before comparing, since that
                    // path is a harness artifact, not a behavioural difference.
                    var originalDir = Path.GetDirectoryName(originalPath);
                    var roundTrippedDir = Path.GetDirectoryName(roundTrippedPath);
                    var normalizedOriginalErrors = originalExec.Errors
                        .Select(e => e.Replace(originalDir, "<TEMP>").Replace(roundTrippedDir, "<TEMP>"))
                        .ToList();
                    var normalizedRoundTrippedErrors = roundTrippedExec.Errors
                        .Select(e => e.Replace(originalDir, "<TEMP>").Replace(roundTrippedDir, "<TEMP>"))
                        .ToList();
                    var sameErrors = normalizedOriginalErrors.SequenceEqual(normalizedRoundTrippedErrors);
                    if (sameErrors)
                    {
                        result.Status = FidelityStatus.PassBothFailedIdentically.ToString();
                        result.Detail = "Both original and round-tripped executions failed identically " +
                                        (entry.RequiresSource
                                            ? "(expected — this activity needs a live source not present in this sandbox). "
                                            : "(the activity's own preconditions are not met by this sample even with its declared inputs bound). ") +
                                        "Error: " + string.Join("; ", originalExec.Errors);
                    }
                    else
                    {
                        result.Status = FidelityStatus.ExecutionMismatch.ToString();
                        result.Detail = "Both executions failed but with DIFFERENT errors — a genuine fidelity gap. " +
                                        "Original: " + string.Join("; ", originalExec.Errors) +
                                        " | RoundTripped: " + string.Join("; ", roundTrippedExec.Errors);
                    }
                    return result;
                }

                if (originalExec.IsSuccess != roundTrippedExec.IsSuccess)
                {
                    result.Status = FidelityStatus.ExecutionAsymmetric.ToString();
                    result.Detail = $"Original.IsSuccess={originalExec.IsSuccess}, RoundTripped.IsSuccess={roundTrippedExec.IsSuccess}. " +
                                     "Original errors: " + string.Join("; ", originalExec.Errors) +
                                     " | RoundTripped errors: " + string.Join("; ", roundTrippedExec.Errors);
                    return result;
                }

                if (originalPayload != roundTrippedPayload)
                {
                    result.Status = FidelityStatus.ExecutionMismatch.ToString();
                    result.Detail = "Payload differs after round-trip.\nOriginal:      " + Truncate(originalPayload) +
                                     "\nRound-tripped: " + Truncate(roundTrippedPayload);
                    return result;
                }

                result.Status = FidelityStatus.Pass.ToString();
                result.Detail = "Original and round-tripped executions match (both " +
                                (originalExec.IsSuccess ? "succeeded" : "failed identically") + ").";
                return result;
            }
            finally
            {
                result.Detail = $"[{mode}] " + result.Detail;
                TryDeleteParent(originalPath);
                TryDeleteParent(roundTrippedPath);
            }
        }

        /// <summary>
        /// Re-runs the original (un-round-tripped) workflow and reports whether it produced the same
        /// outcome twice. Used to tell "the converter changed the behaviour" apart from "this workflow
        /// never produces the same answer twice in the first place".
        /// </summary>
        static async Task<bool> IsSelfConsistentAsync(
            string originalPath, bool firstIsSuccess, string firstPayload, Dictionary<string, string> inputParameters)
        {
            var executor = CreateExecutor();
            var repeat = executor.Execute(new WorkflowExecutionRequest
            {
                WorkflowFilePath = originalPath,
                ReturnType = EmitionTypes.JSON,
                InputParameters = inputParameters
            });
            var repeatPayload = await repeat.ReadPayloadAsync();
            return repeat.IsSuccess == firstIsSuccess && repeatPayload == firstPayload;
        }

        static string Truncate(string s, int max = 400) =>
            string.IsNullOrEmpty(s) ? "(empty)" : s.Length <= max ? s : s.Substring(0, max) + "…";

        /// <summary>
        /// Replaces the &lt;Action&gt;/&lt;XamlDefinition&gt; text content in a raw .bite XML
        /// document with a new XAML string, preserving everything else (DataList, resource
        /// metadata, etc.) so execution behaves the same modulo the workflow body itself.
        /// </summary>
        static string BuildBiteWithReplacedXaml(string originalFileText, string newXaml)
        {
            var doc = XElement.Parse(originalFileText, LoadOptions.PreserveWhitespace);
            var actions = doc.Element("Actions");
            var action = actions != null ? actions.Element("Action") : doc.Element("Action");
            var xamlElement = action?.Element("XamlDefinition");
            if (xamlElement == null)
            {
                throw new InvalidOperationException("No <Action>/<XamlDefinition> element found in sample.");
            }
            xamlElement.Value = newXaml;
            return doc.ToString(SaveOptions.DisableFormatting);
        }

        /// <summary>
        /// Copies every OTHER <c>.bite</c> in <paramref name="samplePath"/>'s directory next to
        /// <paramref name="tempCopyPath"/>, so a workflow that calls another resource can find it.
        /// The sample itself is skipped - the temp copy already holds it, and it is the copy under
        /// test (round-tripped, in one of the two calls), so overwriting it with the pristine
        /// original would quietly measure nothing at all.
        /// </summary>
        static void CopyCompanionResources(string samplePath, string tempCopyPath)
        {
            var sourceDir = Path.GetDirectoryName(samplePath);
            var destinationDir = Path.GetDirectoryName(tempCopyPath);
            if (sourceDir == null || destinationDir == null)
            {
                return;
            }

            var sampleFileName = Path.GetFileName(samplePath);
            foreach (var companion in Directory.EnumerateFiles(sourceDir, "*.bite"))
            {
                var companionFileName = Path.GetFileName(companion);
                if (string.Equals(companionFileName, sampleFileName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                File.Copy(companion, Path.Combine(destinationDir, companionFileName), overwrite: true);
            }
        }

        static string CopyToTemp(string fileText, string fileName)
        {
            var dir = Path.Combine(Path.GetTempPath(), "wf-fidelity-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(dir);
            var dest = Path.Combine(dir, fileName);
            File.WriteAllText(dest, fileText, Encoding.UTF8);
            return dest;
        }

        static void TryDeleteParent(string filePath)
        {
            try
            {
                var dir = Path.GetDirectoryName(filePath);
                if (dir != null && Directory.Exists(dir))
                {
                    Directory.Delete(dir, recursive: true);
                }
            }
            catch (IOException)
            {
                // Best-effort cleanup; leftover temp dirs don't affect correctness.
            }
        }

        static void WriteReport(
            string devRoot, List<FidelityResult> results, List<string> presentRoots)
        {
            Console.WriteLine("=== Round-Trip Fidelity Report ===");
            foreach (var r in results.OrderBy(r => r.Category).ThenBy(r => r.StudioName))
            {
                Console.WriteLine($"[{r.Status,-32}] {r.StudioName,-28} ({r.Category}) sample={r.SamplePath}");
                if (!string.IsNullOrEmpty(r.Detail))
                {
                    Console.WriteLine("    " + r.Detail.Replace("\n", "\n    "));
                }
            }

            var summary = results.GroupBy(r => r.Status).ToDictionary(g => g.Key, g => g.Count());
            Console.WriteLine("=== Summary ===");
            foreach (var kvp in summary.OrderByDescending(k => k.Value))
            {
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
            }

            try
            {
                var reportDir = Path.Combine(devRoot, "Warewolf.Execution.Lightweight", "Resources");
                Directory.CreateDirectory(reportDir);
                var reportPath = Path.Combine(reportDir, "fidelity-allowlist.json");
                // The provenance block is what lets a later run decide whether this file is
                // comparable to what it just measured. Without it, a baseline captured under
                // different execution semantics or against a different corpus is indistinguishable
                // from a genuine converter change — which is precisely how a set of phantom
                // regressions once reached CI.
                var json = JsonConvert.SerializeObject(new
                {
                    generatedAtUtc = DateTime.UtcNow,
                    harness = new HarnessProvenance
                    {
                        HarnessVersion = FidelityRegressionGate.CurrentHarnessVersion,
                        MaxSamplesPerType = MaxSamplesPerType,
                        CorpusRoots = presentRoots,
                    },
                    results
                }, Formatting.Indented);
                File.WriteAllText(reportPath, json, Encoding.UTF8);
                Console.WriteLine("Report written to " + reportPath);
            }
            catch (IOException ex)
            {
                Console.WriteLine("Could not write fidelity-allowlist.json artifact: " + ex.Message);
            }
        }
    }
}
