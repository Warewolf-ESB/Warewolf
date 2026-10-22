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
using Dev2.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.WfCorpus
{
    /// <summary>
    /// Guards the WF-removal activity corpus (WfCorpus/*.bite, embedded). See <see cref="WfCorpusGenerator"/>.
    /// Regenerate after adding or changing an activity class:
    /// <c>WAREWOLF_REGENERATE_WF_CORPUS=1</c>, then run <see cref="WfCorpus_Regenerate"/>.
    /// </summary>
    [TestClass]
    public class WfCorpusTests
    {
        const string RegenerateVariable = "WAREWOLF_REGENERATE_WF_CORPUS";
        const string ResourceMarker = ".WfCorpus.";

        // ActivityParser turns the WF flow-node wrappers into their engine counterparts.
        static readonly Dictionary<string, string> ParsedStartType = new Dictionary<string, string>
        {
            [nameof(DsfFlowDecisionActivity)] = nameof(DsfDecision),
            [nameof(DsfFlowSwitchActivity)] = nameof(DsfSwitch),
        };

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        [TestCategory("WfCorpus")]
        public void WfCorpus_EveryConcreteActivityType_HasCorpusEntry()
        {
            var entries = CorpusEntries().Keys.ToHashSet(StringComparer.Ordinal);

            var missing = WfCorpusGenerator.ActivityTypes().Select(t => t.Name).Where(n => !entries.Contains(n)).ToList();

            Assert.AreEqual(0, missing.Count,
                $"Activity classes with no WfCorpus entry: {string.Join(", ", missing)}. " +
                $"Set {RegenerateVariable}=1 and run {nameof(WfCorpus_Regenerate)}.");
        }

        [TestMethod]
        [Timeout(300000)]
        [Owner("Ashley Lewis")]
        [TestCategory("WfCorpus")]
        public void WfCorpus_EveryEntry_LoadsAndParsesToExpectedStartType()
        {
            var failures = new List<string>();

            foreach (var entry in CorpusEntries())
            {
                try
                {
                    var start = WfCorpusGenerator.LoadAndParse(entry.Value);
                    var expected = ParsedStartType.TryGetValue(entry.Key, out var mapped) ? mapped : entry.Key;
                    if (start?.GetType().Name != expected)
                    {
                        failures.Add($"{entry.Key}: start node parsed as {start?.GetType().Name ?? "null"}, expected {expected}");
                    }
                }
                catch (Exception e)
                {
                    failures.Add($"{entry.Key}: {e.GetType().Name}: {e.Message}");
                }
            }

            Assert.AreEqual(0, failures.Count, string.Join(Environment.NewLine, failures));
        }

        [TestMethod]
        [Timeout(300000)]
        [Owner("Ashley Lewis")]
        [TestCategory("WfCorpus")]
        public void WfCorpus_Regenerate()
        {
            if (Environment.GetEnvironmentVariable(RegenerateVariable) != "1")
            {
                Assert.Inconclusive($"Writes the checked-in corpus; set {RegenerateVariable}=1 to run.");
            }

            var directory = Path.GetDirectoryName(SourceFile());
            foreach (var stale in Directory.GetFiles(directory, "*.bite"))
            {
                File.Delete(stale);
            }
            WfCorpusGenerator.Generate(directory);

            Assert.AreEqual(WfCorpusGenerator.ActivityTypes().Count, Directory.GetFiles(directory, "*.bite").Length);
        }

        static Dictionary<string, string> CorpusEntries()
        {
            var assembly = typeof(WfCorpusTests).Assembly;
            var entries = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var name in assembly.GetManifestResourceNames().Where(n => n.Contains(ResourceMarker) && n.EndsWith(".bite", StringComparison.Ordinal)))
            {
                var typeName = name.Substring(name.IndexOf(ResourceMarker, StringComparison.Ordinal) + ResourceMarker.Length);
                typeName = typeName.Substring(0, typeName.Length - ".bite".Length);
                using (var reader = new StreamReader(assembly.GetManifestResourceStream(name)))
                {
                    entries[typeName] = reader.ReadToEnd();
                }
            }
            return entries;
        }

        static string SourceFile([CallerFilePath] string path = "") => path;
    }
}
