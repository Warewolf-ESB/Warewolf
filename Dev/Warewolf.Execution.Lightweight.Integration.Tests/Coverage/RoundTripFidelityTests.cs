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

            var classified = RoundTripFidelityCorpus.ClassifyCorpus(biteFiles);
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

                var sample = samples[0];
                var result = await EvaluateSampleAsync(entry, sample);
                results.Add(result);
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

        async Task<FidelityResult> EvaluateSampleAsync(ToolboxEntry entry, string samplePath)
        {
            var result = new FidelityResult
            {
                StudioName = entry.StudioName,
                Category = entry.Category,
                RequiresSource = entry.RequiresSource,
                SamplePath = samplePath,
            };

            string originalFileText;
            try
            {
                originalFileText = File.ReadAllText(samplePath, Encoding.UTF8);
            }
            catch (IOException ex)
            {
                result.Status = FidelityStatus.TranslationFailed.ToString();
                result.Detail = "Could not read sample file: " + ex.Message;
                return result;
            }

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
                var activityBuilder = XamlActivityHelper.GetXamlActivityBuilderAsDataActivities(xamlDefinition);
                var loadJson = new WorkflowToX6Converter().ConvertToX6Json(activityBuilder, xamlDefinition.ToString());
                // ConvertToX6Json emits an X6WorkflowLoadModel ("nodes"+"edges" arrays) — the shape
                // the AntV/X6 JS graph library is loaded from. X6JsonToWorkflow instead expects an
                // X6WorkflowSaveModel ("cells" — a single merged array where edges are the AntV/X6
                // library's own `shape: "edge"` convention). In production the X6 web client performs
                // this merge when the user saves the graph; there is no C# bridge for it since the
                // web-studio frontend lives outside this repo. Reproduce that merge here so the
                // underlying converter round-trip can be exercised in isolation.
                var saveJson = BridgeLoadModelToSaveModel(loadJson);
                roundTrippedXaml = new X6ToWorkflowConverter().X6JsonToWorkflow(saveJson).ToString();
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
                    InputParameters = new Dictionary<string, string>()
                });
                var roundTrippedExec = executor.Execute(new WorkflowExecutionRequest
                {
                    WorkflowFilePath = roundTrippedPath,
                    ReturnType = EmitionTypes.JSON,
                    InputParameters = new Dictionary<string, string>()
                });

                var originalPayload = await originalExec.ReadPayloadAsync();
                var roundTrippedPayload = await roundTrippedExec.ReadPayloadAsync();

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
                                            : "(likely a missing required input under the empty InputParameters used by this harness). ") +
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
                TryDeleteParent(originalPath);
                TryDeleteParent(roundTrippedPath);
            }
        }

        /// <summary>
        /// Merges an X6WorkflowLoadModel JSON string (separate "nodes"/"edges" arrays, as emitted
        /// by WorkflowToX6Converter.ConvertToX6Json) into an X6WorkflowSaveModel JSON string (a
        /// single "cells" array, as expected by X6ToWorkflowConverter.X6JsonToWorkflow), tagging
        /// every edge with the AntV/X6 library's own <c>shape: "edge"</c> convention so
        /// X6JsonToWorkflow's <c>c.shape != "edge"</c> node/edge split resolves correctly.
        /// This mirrors what the X6 web client does when it saves an edited graph — there is no
        /// equivalent bridge in the .NET codebase because the web-studio frontend that owns the X6
        /// graph instance lives outside this repo.
        /// </summary>
        static string BridgeLoadModelToSaveModel(string loadModelJson)
        {
            var load = JsonConvert.DeserializeObject<X6WorkflowLoadModel>(loadModelJson);
            foreach (var edge in load.Edges)
            {
                edge.shape = "edge";
            }
            var save = new X6WorkflowSaveModel
            {
                WorkflowXml = load.WorkflowXml,
                Cells = load.Nodes.Concat(load.Edges).ToList()
            };
            return JsonConvert.SerializeObject(save);
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
