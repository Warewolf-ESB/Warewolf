/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Unit tests for FidelityAllowList: the fidelity-allowlist.json reader that
 *  gates get_workflow_definition/list_workflows' bodyEditable flag. Exercises
 *  LoadFromFile directly (a fixture path, not AppContext.BaseDirectory) so
 *  Pass/non-Pass/missing-file/malformed-file behaviour is independently
 *  verifiable from the shared process-wide Resources copy.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Frozen;
using System.IO;
using Warewolf.Execution.Lightweight.Mcp;

namespace Warewolf.Execution.Lightweight.Tests.Mcp
{
    [TestClass]
    public class FidelityAllowListTests
    {
        private string _root = string.Empty;

        [TestInitialize]
        public void Setup() => _root = Directory.CreateDirectory(
            Path.Combine(Path.GetTempPath(), "fal-tests-" + Guid.NewGuid().ToString("N"))).FullName;

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(_root))
            {
                Directory.Delete(_root, recursive: true);
            }
        }

        private string WriteAllowList(string json)
        {
            var path = Path.Combine(_root, "fidelity-allowlist.json");
            File.WriteAllText(path, json);
            return path;
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void LoadFromFile_PassStatus_IsEditable()
        {
            var path = WriteAllowList(@"{ ""results"": [
                { ""StudioName"": ""Assign Object"", ""Status"": ""Pass"" }
            ] }");

            var map = FidelityAllowList.LoadFromFile(path);

            Assert.IsTrue(map.TryGetValue("Assign Object", out var status));
            Assert.AreEqual("Pass", status);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [DataRow("PassBothFailedIdentically")]
        [DataRow("NoCorpusSample")]
        [DataRow("TranslationFailed")]
        public void LoadFromFile_NonPassStatuses_AreNotEditableViaIsEditable(string status)
        {
            var path = WriteAllowList($@"{{ ""results"": [
                {{ ""StudioName"": ""Whatever"", ""Status"": ""{status}"" }}
            ] }}");

            var map = FidelityAllowList.LoadFromFile(path);

            Assert.IsTrue(map.ContainsKey("Whatever"));
            Assert.AreNotEqual(FidelityAllowList.PassStatus, map["Whatever"]);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void LoadFromFile_MissingFile_ReturnsEmptyMap()
        {
            var missingPath = Path.Combine(_root, "does-not-exist.json");

            var map = FidelityAllowList.LoadFromFile(missingPath);

            Assert.AreEqual(0, map.Count);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void LoadFromFile_MalformedJson_ReturnsEmptyMap()
        {
            var path = WriteAllowList("{ this is not valid json ");

            var map = FidelityAllowList.LoadFromFile(path);

            Assert.AreEqual(0, map.Count);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void LoadFromFile_EmptyResultsArray_ReturnsEmptyMap()
        {
            var path = WriteAllowList(@"{ ""results"": [] }");

            var map = FidelityAllowList.LoadFromFile(path);

            Assert.AreEqual(0, map.Count);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void LoadFromFile_LookupIsCaseInsensitive()
        {
            var path = WriteAllowList(@"{ ""results"": [
                { ""StudioName"": ""Assign Object"", ""Status"": ""Pass"" }
            ] }");

            var map = FidelityAllowList.LoadFromFile(path);

            Assert.IsTrue(map.TryGetValue("assign object", out var status));
            Assert.AreEqual("Pass", status);
        }

        // ── IsEditable / StatusFor, driven from a real allowlist fixture ──────
        //
        // These call the singleton-backed public API. To keep them independent
        // from AppContext.BaseDirectory's process-wide fidelity-allowlist.json,
        // they only assert against the small, self-contained default-location
        // failure mode (file absent) and otherwise validate through LoadFromFile
        // above, which every IsEditable/StatusFor call ultimately delegates to.

        [TestMethod]
        [TestCategory("UnitTest")]
        public void IsEditable_BlankName_ReturnsFalse()
        {
            Assert.IsFalse(FidelityAllowList.IsEditable(""));
            Assert.IsFalse(FidelityAllowList.IsEditable("   "));
            Assert.IsFalse(FidelityAllowList.IsEditable(null!));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void StatusFor_BlankName_ReturnsNull()
        {
            Assert.IsNull(FidelityAllowList.StatusFor(""));
            Assert.IsNull(FidelityAllowList.StatusFor(null!));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void IsEditable_UsesRealProcessAllowList_ForKnownPassEntry()
        {
            // "Assign Object" is committed in Resources/fidelity-allowlist.json with Status
            // "Pass" (see class remarks in FidelityAllowList) — validates the real,
            // process-wide default-location file the production code actually reads.
            Assert.IsTrue(FidelityAllowList.IsEditable("Assign Object"));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void IsEditable_UsesRealProcessAllowList_ForKnownNonPassEntry()
        {
            // "Assign" is committed with Status "PassBothFailedIdentically", which must not
            // count as editable (see FidelityAllowList class remarks).
            Assert.IsFalse(FidelityAllowList.IsEditable("Assign"));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void IsEditable_UnknownName_ReturnsFalse()
        {
            Assert.IsFalse(FidelityAllowList.IsEditable("Not A Real Studio Name At All"));
        }
        // ── Shipping the allow-list with the engine ──────────────────────────
        //
        // Regression: fidelity-allowlist.json was swept up in the csproj's Resources glob, which is
        // conditioned on '$(Configuration)' == 'Debug' because deployers supply their own workflow
        // resources. But this file is ENGINE METADATA, not a workflow - and FidelityAllowList fails
        // CLOSED when it is absent, so every Release deployment reported bodyEditable:false for
        // EVERY workflow. That silently disabled add_step outright and made get_workflow_definition
        // always return a null body. Confirmed on warewolfserver-mcp 2026-08-21:
        //   "FidelityAllowList file not found at: C:\home\site\wwwroot\Resources\
        //    fidelity-allowlist.json. All workflows will report bodyEditable:false."
        // It is now included unconditionally, so it must be present in the build output.

        [TestMethod]
        [TestCategory("UnitTest")]
        public void AllowList_ShipsInTheBuildOutput_WhereFidelityAllowListLooksForIt()
        {
            var deployedPath = Path.Combine(AppContext.BaseDirectory, "Resources", "fidelity-allowlist.json");

            Assert.IsTrue(File.Exists(deployedPath),
                $"fidelity-allowlist.json must ship with the engine, but was not found at '{deployedPath}'. " +
                "Without it FidelityAllowList fails closed and every workflow reports bodyEditable:false, " +
                "which disables add_step and makes get_workflow_definition return a null body.");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void AllowList_AsShipped_MarksKnownPassingActivitiesEditable()
        {
            // Proves the shipped file is not merely present but parses and answers correctly, via
            // the real AppContext.BaseDirectory lookup the engine itself uses. "Sequence" is a
            // stable Pass entry; if the corpus is ever regenerated and this fails, the allow-list
            // regressed rather than the test.
            Assert.IsTrue(FidelityAllowList.IsEditable("Sequence"),
                "the shipped allow-list should mark 'Sequence' editable");
            Assert.AreEqual(FidelityAllowList.PassStatus, FidelityAllowList.StatusFor("Sequence"));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void AllowList_AsShipped_TreatsPassBothFailedIdenticallyAsNotEditable()
        {
            // "Assign" currently sits at PassBothFailedIdentically, NOT Pass: its corpus sample
            // ("Hello World.bite") needs an input the fidelity harness does not supply, so original
            // and round-tripped both failed identically and the activity's own logic was never
            // actually proven to round-trip. Documented here because it is the single most common
            // toolbox activity, so any workflow using an Assign stays bodyEditable:false and cannot
            // be driven through add_step - a corpus gap, not an allow-list bug.
            Assert.AreEqual("PassBothFailedIdentically", FidelityAllowList.StatusFor("Assign"));
            Assert.IsFalse(FidelityAllowList.IsEditable("Assign"),
                "only Status == Pass may count as editable");
        }
    }
}
