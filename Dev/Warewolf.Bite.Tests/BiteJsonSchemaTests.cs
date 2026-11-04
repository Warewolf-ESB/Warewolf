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
using System.Text.Json.Nodes;
using System.Xml.Linq;
using Json.Schema;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Bite.Json;

namespace Warewolf.Bite.Tests
{
    /// <summary>The generated JSON Schema for the version 1 JSON body - WF removal T3.1.</summary>
    [TestClass]
    public class BiteJsonSchemaTests
    {
        const string RegenerateVariable = "WAREWOLF_REGENERATE_BITE_SCHEMA";
        const string ExtraCorpusVariable = "WAREWOLF_WF_PARITY_EXTRA_DIRS";
        const string Comment = "Unlimited.Applications.BusinessDesignStudio.Activities.DsfCommentActivity";

        // Today's engine cannot load this one either (see WfParityHarnessTests.KnownOldPathFailures).
        static readonly HashSet<string> KnownUnreadable = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Dev/Server Tests Setup/Resources/Examples/Recordset - Unique Records.bite",
        };

        static readonly Lazy<string> Generated = new Lazy<string>(BiteJsonSchema.Generate);
        static readonly Lazy<JsonSchema> Schema = new Lazy<JsonSchema>(() => JsonSchema.FromText(Generated.Value));

        static EvaluationResults Evaluate(string json) =>
            Schema.Value.Evaluate(JsonNode.Parse(json), new EvaluationOptions { OutputFormat = OutputFormat.List });

        static string Body(string activity) =>
            $"{{\"start\": \"n0\", \"nodes\": {{\"n0\": {{\"kind\": \"step\", \"activity\": {activity}, \"next\": null}}}}}}";

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("BiteJsonSchema")]
        public void BiteJsonSchema_CheckedInCopy_IsCurrent()
        {
            var path = Path.Combine(RepoRoot(), "Dev", "Warewolf.Bite", "docs", "BiteDefinition-v1.schema.json");
            if (Environment.GetEnvironmentVariable(RegenerateVariable) == "1")
            {
                File.WriteAllText(path, Generated.Value);
                Assert.Inconclusive($"Regenerated {path}; review and commit it.");
            }

            Assert.IsTrue(File.Exists(path), $"{path} is missing; run this test with {RegenerateVariable}=1 to write it.");
            Assert.AreEqual(Generated.Value, File.ReadAllText(path).Replace("\r\n", "\n"),
                $"The checked-in schema is out of date with the reader/writer rules; run this test with {RegenerateVariable}=1 and commit the result.");
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("BiteJsonSchema")]
        public void BiteJsonSchema_Generate_IsDeterministic()
        {
            Assert.AreEqual(Generated.Value, BiteJsonSchema.Generate());
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("BiteJsonSchema")]
        public void BiteJsonSchema_HandWrittenBody_ValidatesAndAMistakeDoesNot()
        {
            var valid = Body($"{{\"$type\": \"{Comment}\", \"DisplayName\": \"c\", \"Text\": \"hi\", \"ExplicitDataList\": {{\"$expr\": \"x\"}}}}");

            Assert.IsTrue(Evaluate(valid).IsValid, "a hand-written comment step");
            Assert.IsFalse(Evaluate(valid.Replace("\"Text\"", "\"Txet\"")).IsValid, "an unknown member");
            Assert.IsFalse(Evaluate(valid.Replace(Comment, "System.Diagnostics.Process")).IsValid, "a type outside the allowlist");
            Assert.IsFalse(Evaluate(valid.Replace("\"step\"", "\"loop\"")).IsValid, "an unknown node kind");
            Assert.IsFalse(Evaluate(valid.Replace("\"Text\": \"hi\"", "\"Text\": 1")).IsValid, "a member of the wrong type");
            Assert.IsFalse(Evaluate(valid.Replace("{\"$expr\": \"x\"}", "{\"$expr\": \"x\", \"y\": 1}")).IsValid, "an argument with extra members");
        }

        [TestMethod]
        [Timeout(1200000)]
        [Owner("Ashley Lewis")]
        [TestCategory("BiteJsonSchema")]
        public void BiteJsonSchema_EveryRepoWorkflowsJsonBody_Validates()
        {
            var root = RepoRoot();
            AssertCorpusValidates(Directory.EnumerateFiles(Path.Combine(root, "Dev"), "*.bite", SearchOption.AllDirectories)
                .Select(f => Path.GetRelativePath(root, f).Replace('\\', '/'))
                .Where(f => !f.Contains("/bin/") && !f.Contains("/obj/") && !KnownUnreadable.Contains(f))
                .Select(f => (f, Path.Combine(root, f))));
        }

        [TestMethod]
        [Timeout(1200000)]
        [Owner("Ashley Lewis")]
        [TestCategory("BiteJsonSchema")]
        public void BiteJsonSchema_EveryExternalWorkflowsJsonBody_Validates()
        {
            var directories = Environment.GetEnvironmentVariable(ExtraCorpusVariable);
            if (string.IsNullOrWhiteSpace(directories))
            {
                Assert.Inconclusive($"Customer/real-world corpora are not checked in; set {ExtraCorpusVariable} to a '{Path.PathSeparator}'-separated list of folders to include them.");
            }
            AssertCorpusValidates(directories.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .SelectMany(d => Directory.EnumerateFiles(d, "*.bite", SearchOption.AllDirectories))
                .Select(f => (f, f)));
        }

        // Each workflow's XAML is written as a JSON body (the writer's output is what the schema describes) and checked.
        static void AssertCorpusValidates(IEnumerable<(string Name, string Path)> files)
        {
            var problems = new List<string>();
            var validated = 0;
            foreach (var (name, path) in files.OrderBy(f => f.Name, StringComparer.Ordinal))
            {
                XElement service;
                try
                {
                    service = XElement.Parse(File.ReadAllText(path));
                }
                catch (System.Xml.XmlException)
                {
                    continue; // not a resource (e.g. an encrypted one), as WfParityHarnessTests.IsWorkflow skips it
                }
                var xaml = (service.Element("Actions")?.Element("Action") ?? service.Element("Action"))?.Element("XamlDefinition")?.Value;
                if (string.IsNullOrWhiteSpace(xaml))
                {
                    continue; // not a workflow
                }
                try
                {
                    var results = Evaluate(BiteJsonWriter.FromXaml(xaml, (string?)service.Attribute("ID") ?? name));
                    validated++;
                    if (!results.IsValid)
                    {
                        problems.Add($"{name}: {Describe(results)}");
                    }
                }
                catch (Exception e) when (e is BiteFormatException or NotSupportedException)
                {
                    problems.Add($"{name}: {e.GetType().Name}: {e.Message}");
                }
            }

            Assert.IsTrue(validated > 0, "No workflows were validated.");
            Assert.AreEqual(0, problems.Count, $"{problems.Count} of {validated} JSON bodies do not match the schema:{Environment.NewLine}{string.Join(Environment.NewLine, problems.Take(50))}");
        }

        // The deepest failing locations are the useful ones; the anyOf chains above them only say "no choice matched".
        static string Describe(EvaluationResults results) =>
            string.Join("; ", (results.Details ?? Array.Empty<EvaluationResults>())
                .Where(d => !d.IsValid && d.Errors != null)
                .SelectMany(d => d.Errors!.Select(e => $"{d.InstanceLocation} {e.Key}: {e.Value}"))
                .Distinct()
                .Take(5));

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
            Assert.Inconclusive("Repo source tree not found: set BUILD_SOURCESDIRECTORY to the checkout root.");
            return "";
        }
    }
}
