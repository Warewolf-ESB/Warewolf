/*
 * Round-trip fidelity gate for Dev2.Activities.WF.WorkflowToX6Converter /
 * X6ToWorkflowConverter — the blocker flagged by warewolf-lee-mcp-v3-addendum-a.md
 * before the v3 spec's `bodyEditable` gating rule can be trusted.
 *
 * Question this answers, per activity type in the v3 spec's "Toolbox subset (v3)" table:
 *   Does XAML --ConvertToX6Json--> X6 JSON --X6JsonToWorkflow--> XAML' produce a workflow
 *   that executes IDENTICALLY (same success/failure, same JSON payload) to the original?
 *
 * This is an *informational discovery* pass, not a hard pass/fail gate: the goal is to
 * produce the empirical `fidelity-allowlist.json` artifact the MCP `bodyEditable` logic
 * will consume, not to block CI on activity types that are not yet supported. Individual
 * per-type mismatches are recorded and reported, never asserted on directly. The test
 * only fails if the harness itself is broken (e.g. it finds zero corpus at all when the
 * corpus tree is known to be deployed), which would indicate a wiring bug rather than a
 * genuine converter fidelity gap.
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
        enum FidelityStatus
        {
            Pass,
            PassBothFailedIdentically,
            NoCorpusSample,
            TranslationFailed,
            ExecutionMismatch,
            ExecutionAsymmetric,
            NonDeterministic,
        }

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

            WriteReport(devRoot, results);

            // Harness-integrity assertions only — never assert on individual fidelity outcomes here.
            Assert.IsTrue(results.Count == RoundTripFidelityCorpus.ToolboxSubset.Count,
                "Expected one result row per toolbox entry.");
            var withSamples = results.Count(r => r.Status != FidelityStatus.NoCorpusSample.ToString());
            Assert.IsTrue(withSamples > 0,
                "Expected at least one toolbox entry to have a real corpus sample — " +
                "zero matches suggests the classifier or corpus roots are broken, not that the corpus is empty.");
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

        static void WriteReport(string devRoot, List<FidelityResult> results)
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
                var json = JsonConvert.SerializeObject(new
                {
                    generatedAtUtc = DateTime.UtcNow,
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
