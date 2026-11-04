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
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Bite;
using Warewolf.Bite.Json;
using Warewolf.Bite.Xaml;

namespace Dev2.Tests.Activities.Parity
{
    /// <summary>
    /// WF removal T1.1: loads every corpus workflow through two <see cref="IBiteReader"/>s and diffs the
    /// canonical graphs property by property. Until the WF-free reader exists both sides are today's WF
    /// path, which proves the canonical form is deterministic and the corpus loads; the new reader then
    /// replaces <see cref="Candidate"/>.
    /// </summary>
    [TestClass]
    public class WfParityHarnessTests
    {
        const string ExtraCorpusVariable = "WAREWOLF_WF_PARITY_EXTRA_DIRS";

        // Workflows today's engine already cannot load. The new reader must load these correctly or fail
        // them too; either way a change here is deliberate. Paths are relative to the repo root.
        static readonly HashSet<string> KnownOldPathFailures = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Dev/Server Tests Setup/Resources/Examples/Recordset - Unique Records.bite",
        };

        static readonly IBiteReader Reference = new WfBiteReader();
        static readonly IBiteReader Candidate = new BiteReader();

        [TestMethod]
        [Timeout(300000)]
        [Owner("Ashley Lewis")]
        [TestCategory("WfParity")]
        public void WfParity_GeneratedCorpus_ReaderOutputsMatch() => AssertParity(GeneratedCorpus(), new HashSet<string>());

        [TestMethod]
        [Timeout(1200000)]
        [Owner("Ashley Lewis")]
        [TestCategory("WfParity")]
        public void WfParity_RepoWorkflows_ReaderOutputsMatch() => AssertParity(RepoWorkflows(), KnownOldPathFailures);

        [TestMethod]
        [Timeout(1200000)]
        [Owner("Ashley Lewis")]
        [TestCategory("WfParity")]
        public void WfParity_ExternalCorpora_ReaderOutputsMatch() => AssertParity(ExternalCorpora(), new HashSet<string>());

        // ---------------------------------------------------------------- JSON body round trip (T1.5)

        [TestMethod]
        [Timeout(300000)]
        [Owner("Ashley Lewis")]
        [TestCategory("WfParity")]
        public void WfParity_JsonRoundTrip_GeneratedCorpus() => AssertJsonRoundTrip(GeneratedCorpus(), new HashSet<string>());

        [TestMethod]
        [Timeout(1200000)]
        [Owner("Ashley Lewis")]
        [TestCategory("WfParity")]
        public void WfParity_JsonRoundTrip_RepoWorkflows() => AssertJsonRoundTrip(RepoWorkflows(), KnownOldPathFailures);

        [TestMethod]
        [Timeout(1200000)]
        [Owner("Ashley Lewis")]
        [TestCategory("WfParity")]
        public void WfParity_JsonRoundTrip_ExternalCorpora() => AssertJsonRoundTrip(ExternalCorpora(), new HashSet<string>());

        // XAML -> BiteReader -> G1 -> JSON body -> BiteReader -> G2. G2 must equal both G1 and the WF path's graph.
        static void AssertJsonRoundTrip(IEnumerable<(string Name, string Bite)> workflows, HashSet<string> knownFailures)
        {
            var problems = new List<string>();
            var compared = 0;
            long xamlChars = 0, jsonChars = 0;
            var bite = new BiteReader();

            foreach (var (name, original) in workflows)
            {
                if (knownFailures.Contains(name))
                {
                    continue; // nothing to round-trip: no reader can load it
                }
                try
                {
                    var converted = BiteJsonWriter.ConvertResource(original);
                    xamlChars += XElement.Parse(original).Descendants("XamlDefinition").First().Value.Length;
                    jsonChars += XElement.Parse(converted).Descendants("BiteDefinition").First().Value.Length;

                    var fromJson = bite.Read(converted);
                    var againstXaml = ActivityGraphDiff.Compare(bite.Read(original), fromJson)
                        .Where(d => !MintedIds.IsMintedIdDifference(d, original) && !ReencryptedSecrets.IsReencryptionDifference(d, original)).ToList();
                    var againstWf = ActivityGraphDiff.Compare(Reference.Read(original), fromJson)
                        .Where(d => !MintedIds.IsMintedIdDifference(d, original) && !ReencryptedSecrets.IsReencryptionDifference(d, original)).ToList();
                    compared++;
                    problems.AddRange(againstXaml.Take(10).Select(d => $"{name}: vs XAML read: {d}"));
                    problems.AddRange(againstWf.Take(10).Select(d => $"{name}: vs WF path: {d}"));
                }
                catch (Exception e)
                {
                    problems.Add($"{name}: {e.GetType().Name}: {e.Message}");
                }
            }

            Console.WriteLine($"JSON round trip: {compared} workflow(s); XamlDefinition {xamlChars:N0} chars, BiteDefinition {jsonChars:N0} chars ({(xamlChars == 0 ? 0 : 100.0 * jsonChars / xamlChars):F0}%)");
            Assert.IsTrue(compared > 0, "No workflows were round-tripped.");
            Assert.AreEqual(0, problems.Count, $"{problems.Count} round-trip problem(s) across {compared} workflow(s):{Environment.NewLine}{string.Join(Environment.NewLine, problems.Take(200))}");
        }

        // ---------------------------------------------------------------- WF-free XAML writer (T3.2)

        [TestMethod]
        [Timeout(300000)]
        [Owner("Ashley Lewis")]
        [TestCategory("WfParity")]
        public void WfParity_XamlRewrite_GeneratedCorpus() => AssertXamlRewrite(GeneratedCorpus(), new HashSet<string>());

        [TestMethod]
        [Timeout(1200000)]
        [Owner("Ashley Lewis")]
        [TestCategory("WfParity")]
        public void WfParity_XamlRewrite_RepoWorkflows() => AssertXamlRewrite(RepoWorkflows(), KnownOldPathFailures);

        [TestMethod]
        [Timeout(1200000)]
        [Owner("Ashley Lewis")]
        [TestCategory("WfParity")]
        public void WfParity_XamlRewrite_ExternalCorpora() => AssertXamlRewrite(ExternalCorpora(), new HashSet<string>());

        // XAML -> model -> BiteXamlWriter -> XAML'. XAML' must load on the WF path (what engines still running WF
        // do) to the same graph as the original, and read the same through BiteReader.
        static void AssertXamlRewrite(IEnumerable<(string Name, string Bite)> workflows, HashSet<string> knownFailures)
        {
            var problems = new List<string>();
            var compared = 0;
            var bite = new BiteReader();

            foreach (var (name, original) in workflows)
            {
                if (knownFailures.Contains(name))
                {
                    continue;
                }
                try
                {
                    var rewritten = RewriteXaml(original, bite);
                    var againstWf = ActivityGraphDiff.Compare(Reference.Read(original), Reference.Read(rewritten))
                        .Where(d => !MintedIds.IsMintedIdDifference(d, original) && !ReencryptedSecrets.IsReencryptionDifference(d, original)).ToList();
                    var againstBite = ActivityGraphDiff.Compare(bite.Read(original), bite.Read(rewritten))
                        .Where(d => !MintedIds.IsMintedIdDifference(d, original) && !ReencryptedSecrets.IsReencryptionDifference(d, original)).ToList();
                    compared++;
                    problems.AddRange(againstWf.Take(10).Select(d => $"{name}: WF path: {d}"));
                    problems.AddRange(againstBite.Take(10).Select(d => $"{name}: BiteReader: {d}"));
                }
                catch (Exception e)
                {
                    problems.Add($"{name}: {e.GetType().Name}: {(e.InnerException ?? e).Message}");
                }
            }

            Assert.IsTrue(compared > 0, "No workflows were rewritten.");
            Assert.AreEqual(0, problems.Count, $"{problems.Count} rewrite problem(s) across {compared} workflow(s):{Environment.NewLine}{string.Join(Environment.NewLine, problems.Take(200))}");
        }

        static string RewriteXaml(string bite, BiteReader reader)
        {
            var service = XElement.Parse(bite, LoadOptions.PreserveWhitespace);
            var xaml = service.Descendants("XamlDefinition").First();
            var model = reader.ReadModel(xaml.Value, (string)service.Attribute("ID") ?? "")
                        ?? throw new InvalidOperationException("No Flowchart to rewrite");
            xaml.Value = BiteXamlWriter.Write(model);
            return service.ToString(SaveOptions.DisableFormatting);
        }

        // ---------------------------------------------------------------- corpora

        static IEnumerable<(string Name, string Bite)> GeneratedCorpus()
        {
            var assembly = typeof(WfParityHarnessTests).Assembly;
            return assembly.GetManifestResourceNames()
                .Where(n => n.Contains(".WfCorpus.") && n.EndsWith(".bite", StringComparison.Ordinal))
                .OrderBy(n => n, StringComparer.Ordinal)
                .Select(n =>
                {
                    using (var reader = new StreamReader(assembly.GetManifestResourceStream(n)))
                    {
                        return (Name: n, Bite: reader.ReadToEnd());
                    }
                })
                .ToList();
        }

        static IEnumerable<(string Name, string Bite)> RepoWorkflows()
        {
            var root = RepoRoot();
            if (root == null)
            {
                Assert.Inconclusive("Repo source tree not found: set BUILD_SOURCESDIRECTORY to the checkout root.");
            }

            return Directory.EnumerateFiles(Path.Combine(root, "Dev"), "*.bite", SearchOption.AllDirectories)
                .Select(f => Path.GetRelativePath(root, f).Replace('\\', '/'))
                .Where(f => !f.Contains("/bin/") && !f.Contains("/obj/") && !f.Contains("/WfCorpus/"))
                .OrderBy(f => f, StringComparer.Ordinal)
                .Select(f => (Name: f, Bite: File.ReadAllText(Path.Combine(root, f))))
                .Where(w => IsWorkflow(w.Bite))
                .ToList();
        }

        static IEnumerable<(string Name, string Bite)> ExternalCorpora()
        {
            var directories = Environment.GetEnvironmentVariable(ExtraCorpusVariable);
            if (string.IsNullOrWhiteSpace(directories))
            {
                Assert.Inconclusive($"Customer/real-world corpora are not checked in; set {ExtraCorpusVariable} to a '{Path.PathSeparator}'-separated list of folders to include them.");
            }

            return directories.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .SelectMany(d => Directory.EnumerateFiles(d, "*.bite", SearchOption.AllDirectories))
                .OrderBy(f => f, StringComparer.Ordinal)
                .Select(f => (Name: f, Bite: File.ReadAllText(f)))
                .Where(w => IsWorkflow(w.Bite))
                .ToList();
        }

        static void AssertParity(IEnumerable<(string Name, string Bite)> workflows, HashSet<string> knownFailures)
        {
            var problems = new List<string>();
            var compared = 0;
            var stillFailing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var (name, bite) in workflows)
            {
                IDev2Activity expected;
                try
                {
                    expected = Reference.Read(bite);
                }
                catch (Exception e)
                {
                    if (knownFailures.Contains(name))
                    {
                        stillFailing.Add(name);
                        CandidateMustAlsoReject(name, bite, problems);
                    }
                    else
                    {
                        problems.Add($"{name}: reference reader failed: {e.GetType().Name}: {e.Message}");
                    }
                    continue;
                }

                List<GraphDifference> differences;
                try
                {
                    differences = ActivityGraphDiff.Compare(expected, Candidate.Read(bite))
                        .Where(d => !MintedIds.IsMintedIdDifference(d, bite) && !ReencryptedSecrets.IsReencryptionDifference(d, bite))
                        .ToList();
                }
                catch (Exception e)
                {
                    problems.Add($"{name}: candidate reader failed: {e.GetType().Name}: {e.Message}");
                    continue;
                }
                compared++;
                problems.AddRange(differences.Take(20).Select(d => $"{name}: {d}"));
                if (differences.Count > 20)
                {
                    problems.Add($"{name}: ... {differences.Count - 20} more difference(s)");
                }
            }

            foreach (var fixedFailure in knownFailures.Where(f => !stillFailing.Contains(f)))
            {
                problems.Add($"{fixedFailure}: listed as a known reference-path failure but loaded or was not found; update {nameof(KnownOldPathFailures)}.");
            }

            Assert.IsTrue(compared > 0, "No workflows were compared.");
            Assert.AreEqual(0, problems.Count, $"{problems.Count} parity problem(s) across {compared} compared workflow(s):{Environment.NewLine}{string.Join(Environment.NewLine, problems.Take(200))}");
        }

        // A workflow the reference path cannot load has nothing to compare against, so the candidate must reject it
        // too, loudly. If it loads, someone must check the result by hand and take the file off the known list.
        static void CandidateMustAlsoReject(string name, string bite, List<string> problems)
        {
            if (Candidate is WfBiteReader)
            {
                return; // old path against itself: it fails the same way by definition
            }
            try
            {
                Candidate.Read(bite);
                problems.Add($"{name}: candidate loaded a workflow the reference path rejects; verify by hand and update {nameof(KnownOldPathFailures)}.");
            }
            catch (BiteFormatException)
            {
                // expected: rejected with the element and line
            }
            catch (Exception e)
            {
                problems.Add($"{name}: candidate rejected it, but with {e.GetType().Name} instead of a {nameof(BiteFormatException)}: {e.Message}");
            }
        }

        static bool IsWorkflow(string bite)
        {
            try
            {
                var service = XElement.Parse(bite);
                var action = service.Element("Actions")?.Element("Action") ?? service.Element("Action");
                return !string.IsNullOrWhiteSpace(action?.Element("XamlDefinition")?.Value);
            }
            catch (System.Xml.XmlException)
            {
                return false; // trigger/config JSON that shares the .bite extension
            }
        }

        // CI test jobs check the repo out but run cross-compiled binaries, so a compile-time path is useless there.
        static string RepoRoot([CallerFilePath] string sourceFile = "")
        {
            foreach (var candidate in new[] { Environment.GetEnvironmentVariable("BUILD_SOURCESDIRECTORY"), Path.GetDirectoryName(sourceFile) })
            {
                for (var directory = candidate; !string.IsNullOrEmpty(directory); directory = Path.GetDirectoryName(directory))
                {
                    if (Directory.Exists(Path.Combine(directory, "Dev")) && File.Exists(Path.Combine(directory, "Compile.ps1")))
                    {
                        return directory;
                    }
                }
            }
            return null;
        }
    }
}
