/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using Warewolf.Execution.Lightweight.Models;

namespace Warewolf.Execution.Lightweight.Integration.Tests.Coverage
{
    /// <summary>
    /// Debugging aid for round-trip fidelity gaps.
    ///
    /// <para>
    /// <see cref="RoundTripFidelityTests"/> answers "does this activity survive a round trip?" — a
    /// yes/no per activity. When the answer is no, that tells you nothing about <em>what</em> was
    /// lost, and the XAML involved is far too large to eyeball. This class answers the follow-up
    /// question: it round-trips a sample and reports the attribute-level difference between the
    /// original and round-tripped XAML, so a missing property points straight at the
    /// <c>ToX6Json</c>/<c>FromX6Json</c> pair that dropped it.
    /// </para>
    ///
    /// <para>
    /// Deliberately asserts nothing about converter behaviour — a gap here is a finding to act on,
    /// not a build break, exactly as in <see cref="RoundTripFidelityTests"/>. The only assertion is
    /// harness integrity, so this tool cannot silently rot into a no-op. Point
    /// <see cref="ActivitiesUnderDiagnosis"/> at whatever is currently failing.
    /// </para>
    /// </summary>
    [TestClass]
    [TestCategory("ConverterGapDiagnostics")]
    public class ConverterGapDiagnostics
    {
        /// <summary>
        /// The activities to diagnose. Kept as an explicit list rather than "everything not passing"
        /// so a run stays fast and its output stays readable.
        /// </summary>
        static readonly string[] ActivitiesUnderDiagnosis =
        {
            "Date and Time",
            "Date and Time Difference",
            "GET Web Method",
            "Send Email (SMTP)",
        };

        [TestMethod]
        public void Diagnose_RoundTripGaps_ReportsAttributeLevelDifferences()
        {
            var devRoot = RoundTripFidelityCorpus.FindRepoDevRoot();
            if (devRoot == null)
            {
                Assert.Inconclusive("Could not locate the Dev/ corpus root from " + AppContext.BaseDirectory);
                return;
            }

            // Prefer the exact sample RoundTripFidelityTests recorded for this activity. Re-picking
            // one from the corpus would silently diagnose a DIFFERENT workflow than the one that
            // actually failed — which is precisely the trap this tool exists to avoid.
            var recorded = RecordedSamplePaths(devRoot);
            var classified = RoundTripFidelityCorpus.ClassifyCorpus(
                RoundTripFidelityCorpus.DiscoverBiteFiles(devRoot));

            var reported = 0;
            foreach (var studioName in ActivitiesUnderDiagnosis)
            {
                Console.WriteLine();
                Console.WriteLine("################ " + studioName + " ################");

                string sample = null;
                if (recorded.TryGetValue(studioName, out var recordedPath) && File.Exists(recordedPath))
                {
                    sample = recordedPath;
                    Console.WriteLine("  (sample taken from fidelity-allowlist.json)");
                }
                else if (classified.TryGetValue(studioName, out var samples) && samples.Count > 0)
                {
                    sample = samples[0];
                    Console.WriteLine("  (no recorded sample; falling back to first corpus match)");
                }

                if (sample == null)
                {
                    Console.WriteLine("  no corpus sample");
                    reported++;
                    continue;
                }

                DiagnoseSample(sample);
                reported++;
            }

            Assert.AreEqual(ActivitiesUnderDiagnosis.Length, reported,
                "Expected one diagnosis block per configured activity.");
        }


        /// <summary>
        /// Reads the <c>SamplePath</c> recorded per activity in the generated
        /// <c>fidelity-allowlist.json</c>. Returns an empty map when the artifact is absent or
        /// unreadable, so the diagnostic degrades to corpus discovery rather than failing.
        /// </summary>
        static Dictionary<string, string> RecordedSamplePaths(string devRoot)
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                var path = Path.Combine(devRoot, "Warewolf.Execution.Lightweight", "Resources", "fidelity-allowlist.json");
                if (!File.Exists(path))
                {
                    return map;
                }

                var document = JObject.Parse(File.ReadAllText(path));
                foreach (var row in document["results"] ?? new JArray())
                {
                    var name = (string)row["StudioName"];
                    var sample = (string)row["SamplePath"];
                    if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(sample))
                    {
                        map[name] = sample;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("  (could not read fidelity-allowlist.json: " + ex.Message + ")");
            }
            return map;
        }

        static void DiagnoseSample(string samplePath)
        {
            Console.WriteLine("  sample: " + samplePath);

            var fileContents = new StringBuilder(File.ReadAllText(samplePath, Encoding.UTF8));
            var (xamlDefinition, _, _) = WorkflowExecutor.ExtractWorkflowParts(fileContents);
            if (xamlDefinition == null || xamlDefinition.Length == 0)
            {
                Console.WriteLine("  sample has no XamlDefinition");
                return;
            }

            string roundTripped;
            try
            {
                roundTripped = X6RoundTripBridge.RoundTripXaml(xamlDefinition);
            }
            catch (Exception ex)
            {
                Console.WriteLine("  CONVERSION THREW " + ex.GetType().Name + ": " + ex.Message);
                Console.WriteLine(ex.StackTrace);
                return;
            }

            Report(Flatten(xamlDefinition.ToString()), Flatten(roundTripped));
        }

        static void Report(Dictionary<string, string> before, Dictionary<string, string> after)
        {
            // A property whose original value was {x:Null} and which is simply not re-emitted carries
            // no meaning — it is null before and null after. Counting those as losses drowns the real
            // ones, so they are tallied separately rather than listed.
            bool IsNullDrop(string key) => before[key] == "{x:Null}" && !after.ContainsKey(key);

            var allMissing = before.Keys.Where(k => !after.ContainsKey(k)).ToList();
            var nullDrops = allMissing.Count(IsNullDrop);
            var lost = allMissing.Where(k => !IsNullDrop(k)).OrderBy(k => k).ToList();
            var added = after.Keys.Where(k => !before.ContainsKey(k)).OrderBy(k => k).ToList();
            var changed = before.Keys.Where(k => after.ContainsKey(k) && after[k] != before[k])
                                     .OrderBy(k => k).ToList();

            Console.WriteLine($"  lost={lost.Count} added={added.Count} changed={changed.Count} (plus {nullDrops} benign {{x:Null}} drops)");

            foreach (var k in lost.Take(40))
            {
                Console.WriteLine($"    LOST    {k} = {Clip(before[k])}");
            }
            foreach (var k in changed.Take(40))
            {
                Console.WriteLine($"    CHANGED {k}: {Clip(before[k])}  ->  {Clip(after[k])}");
            }
            foreach (var k in added.Take(40))
            {
                Console.WriteLine($"    ADDED   {k} = {Clip(after[k])}");
            }
        }

        static string Clip(string s, int max = 120) =>
            string.IsNullOrEmpty(s) ? "(empty)" : s.Length <= max ? s : s.Substring(0, max) + "…";

        /// <summary>
        /// Flattens a XAML document to a path → value map covering both elements and attributes, so a
        /// dropped property shows up whether it was serialized as an attribute or as a child element.
        /// Sibling elements of the same name are indexed to keep paths stable and comparable.
        /// </summary>
        static Dictionary<string, string> Flatten(string xaml)
        {
            var map = new Dictionary<string, string>();
            var root = XDocument.Parse(xaml).Root;
            if (root == null)
            {
                return map;
            }

            void Walk(XElement element, string path)
            {
                // Key an activity by its UniqueID rather than its position in the tree. The converter
                // legitimately re-emits the flow in a different document order and renumbers XAML
                // __ReferenceIDs, which under positional keys reports the entire workflow as
                // lost-and-re-added (observed: lost=344 added=325) and buries the real difference.
                // Identity keys make the comparison order-insensitive, so what remains is signal.
                var uniqueId = (string)element.Attribute("UniqueID");
                if (!string.IsNullOrWhiteSpace(uniqueId))
                {
                    path = element.Name.LocalName + "#" + uniqueId;
                }

                map[path] = element.HasElements ? "(element)" : element.Value;
                foreach (var attribute in element.Attributes())
                {
                    map[path + "/@" + attribute.Name.LocalName] = attribute.Value;
                }

                var seen = new Dictionary<string, int>();
                foreach (var child in element.Elements())
                {
                    var name = child.Name.LocalName;
                    seen.TryGetValue(name, out var index);
                    seen[name] = index + 1;
                    Walk(child, $"{path}/{name}[{index}]");
                }
            }

            Walk(root, root.Name.LocalName);
            return map;
        }
    }
}
