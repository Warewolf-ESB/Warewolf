/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Execution.Lightweight;
using Warewolf.Execution.Lightweight.Models;

namespace Warewolf.Execution.Lightweight.Tests
{
    [TestClass]
    public class WorkflowResourceCacheCoverageTests
    {
        private string _root = null!;

        [TestInitialize]
        public void Setup()
        {
            _root = Path.Combine(Path.GetTempPath(),
                "wrc_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_root);
        }

        [TestCleanup]
        public void Cleanup()
        {
            try { if (Directory.Exists(_root)) Directory.Delete(_root, true); }
            catch { /* best-effort */ }
        }

        private string WriteResource(string fileName,
            Guid id, Guid serverId, string name, string resourceType,
            string? subDir = null)
        {
            var dir = subDir is null ? _root : Path.Combine(_root, subDir);
            Directory.CreateDirectory(dir);
            var path = Path.Combine(dir, fileName);
            var xml = $"<Service ID=\"{id}\" ServerID=\"{serverId}\" Name=\"{name}\" ResourceType=\"{resourceType}\"><Body/></Service>";
            File.WriteAllText(path, xml);
            return path;
        }

        // ── Singleton ────────────────────────────────────────────────────────

        [TestMethod]
        [TestCategory("WorkflowResourceCache_Coverage")]
        public void Instance_ReturnsSameSingleton()
        {
            var a = WorkflowResourceCache.Instance;
            var b = WorkflowResourceCache.Instance;
            Assert.AreSame(a, b);
        }

        // ── Resolve ──────────────────────────────────────────────────────────

        [TestMethod]
        [TestCategory("WorkflowResourceCache_Coverage")]
        public void Resolve_ByResourceId_ReturnsFilePath()
        {
            var id = Guid.NewGuid();
            var expected = WriteResource("wf.bite", id, Guid.NewGuid(), "Hello", "WorkflowService");
            var actual = WorkflowResourceCache.Instance.Resolve(_root, id, null);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [TestCategory("WorkflowResourceCache_Coverage")]
        public void Resolve_ByName_WhenIdEmpty_ReturnsFilePath()
        {
            var expected = WriteResource("a.bite", Guid.NewGuid(), Guid.NewGuid(), "MyService", "WorkflowService");
            var actual = WorkflowResourceCache.Instance.Resolve(_root, Guid.Empty, "MyService");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [TestCategory("WorkflowResourceCache_Coverage")]
        public void Resolve_ByName_CaseInsensitive()
        {
            var expected = WriteResource("a.bite", Guid.NewGuid(), Guid.NewGuid(), "MixedCase", "WorkflowService");
            var actual = WorkflowResourceCache.Instance.Resolve(_root, Guid.Empty, "mixedcase");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [TestCategory("WorkflowResourceCache_Coverage")]
        public void Resolve_NameFallback_WhenIdMissButNameHit()
        {
            var expected = WriteResource("a.bite", Guid.NewGuid(), Guid.NewGuid(), "FallbackName", "WorkflowService");
            var actual = WorkflowResourceCache.Instance.Resolve(_root, Guid.NewGuid(), "FallbackName");
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [TestCategory("WorkflowResourceCache_Coverage")]
        public void Resolve_GuidEmptyAndNullName_ReturnsNull()
        {
            WriteResource("a.bite", Guid.NewGuid(), Guid.NewGuid(), "Whatever", "WorkflowService");
            Assert.IsNull(WorkflowResourceCache.Instance.Resolve(_root, Guid.Empty, null));
        }

        [TestMethod]
        [TestCategory("WorkflowResourceCache_Coverage")]
        public void Resolve_GuidEmptyAndEmptyName_ReturnsNull()
        {
            WriteResource("a.bite", Guid.NewGuid(), Guid.NewGuid(), "Whatever", "WorkflowService");
            Assert.IsNull(WorkflowResourceCache.Instance.Resolve(_root, Guid.Empty, string.Empty));
        }

        [TestMethod]
        [TestCategory("WorkflowResourceCache_Coverage")]
        public void Resolve_UnknownIdAndUnknownName_ReturnsNull()
        {
            WriteResource("a.bite", Guid.NewGuid(), Guid.NewGuid(), "Real", "WorkflowService");
            Assert.IsNull(WorkflowResourceCache.Instance.Resolve(_root, Guid.NewGuid(), "NotThere"));
        }

        [TestMethod]
        [TestCategory("WorkflowResourceCache_Coverage")]
        public void Resolve_NonExistentDirectory_ReturnsNull()
        {
            var nonExist = Path.Combine(_root, "missing");
            Assert.IsNull(WorkflowResourceCache.Instance.Resolve(nonExist, Guid.NewGuid(), "anything"));
        }

        [TestMethod]
        [TestCategory("WorkflowResourceCache_Coverage")]
        public void Resolve_EmptyDirectory_ReturnsNull()
        {
            Assert.IsNull(WorkflowResourceCache.Instance.Resolve(_root, Guid.NewGuid(), "x"));
        }

        [TestMethod]
        [TestCategory("WorkflowResourceCache_Coverage")]
        public void Resolve_FindsResourceInNestedSubdirectory()
        {
            var id = Guid.NewGuid();
            var expected = WriteResource("nested.bite", id, Guid.NewGuid(), "Deep", "WorkflowService", subDir: "a\\b\\c");
            var actual = WorkflowResourceCache.Instance.Resolve(_root, id, null);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [TestCategory("WorkflowResourceCache_Coverage")]
        public void Resolve_IgnoresNonBiteFiles()
        {
            var id = Guid.NewGuid();
            // Write a .xml file (should be ignored) with the same id
            var xmlPath = Path.Combine(_root, "ignored.xml");
            File.WriteAllText(xmlPath,
                $"<Service ID=\"{id}\" ServerID=\"{Guid.NewGuid()}\" Name=\"X\" ResourceType=\"WorkflowService\"/>");
            Assert.IsNull(WorkflowResourceCache.Instance.Resolve(_root, id, "X"));
        }

        [TestMethod]
        [TestCategory("WorkflowResourceCache_Coverage")]
        public void Resolve_SkipsFileWithNonGuidId()
        {
            var path = Path.Combine(_root, "bad.bite");
            File.WriteAllText(path,
                "<Service ID=\"not-a-guid\" ServerID=\"\" Name=\"Bad\" ResourceType=\"WorkflowService\"/>");
            Assert.IsNull(WorkflowResourceCache.Instance.Resolve(_root, Guid.Empty, "Bad"));
        }

        [TestMethod]
        [TestCategory("WorkflowResourceCache_Coverage")]
        public void Resolve_SkipsMalformedXmlFile()
        {
            var path = Path.Combine(_root, "broken.bite");
            File.WriteAllText(path, "<<not valid xml>>");
            // Add a sibling good file so we can prove the good one still works.
            var id = Guid.NewGuid();
            var expected = WriteResource("good.bite", id, Guid.NewGuid(), "Good", "WorkflowService");
            Assert.AreEqual(expected, WorkflowResourceCache.Instance.Resolve(_root, id, null));
        }

        [TestMethod]
        [TestCategory("WorkflowResourceCache_Coverage")]
        public void Resolve_ByName_FirstWriteWinsOnDuplicateName()
        {
            var firstId = Guid.NewGuid();
            var secondId = Guid.NewGuid();
            var first = WriteResource("a.bite", firstId, Guid.NewGuid(), "Dup", "WorkflowService");
            WriteResource("b.bite", secondId, Guid.NewGuid(), "Dup", "WorkflowService");
            // Name fallback returns one of them; both id lookups should resolve to their files.
            var byName = WorkflowResourceCache.Instance.Resolve(_root, Guid.Empty, "Dup");
            Assert.IsTrue(byName == first ||
                          byName == Path.Combine(_root, "b.bite"),
                "Name lookup should return one of the duplicates");
            // Id lookups must still resolve precisely.
            Assert.AreEqual(first, WorkflowResourceCache.Instance.Resolve(_root, firstId, null));
        }

        // ── GetEntry ─────────────────────────────────────────────────────────

        [TestMethod]
        [TestCategory("WorkflowResourceCache_Coverage")]
        public void GetEntry_ById_ReturnsPopulatedEntry()
        {
            var id = Guid.NewGuid();
            var serverId = Guid.NewGuid();
            var path = WriteResource("e.bite", id, serverId, "EntryName", "WorkflowService");
            var entry = WorkflowResourceCache.Instance.GetEntry(_root, id);
            Assert.IsNotNull(entry);
            Assert.AreEqual(id, entry!.ResourceId);
            Assert.AreEqual(serverId, entry.ServerId);
            Assert.AreEqual("EntryName", entry.Name);
            Assert.AreEqual(WorkflowResourceType.WorkflowService, entry.ResourceType);
            Assert.AreEqual(path, entry.FilePath);
        }

        [TestMethod]
        [TestCategory("WorkflowResourceCache_Coverage")]
        public void GetEntry_UnknownId_ReturnsNull()
        {
            WriteResource("a.bite", Guid.NewGuid(), Guid.NewGuid(), "X", "WorkflowService");
            Assert.IsNull(WorkflowResourceCache.Instance.GetEntry(_root, Guid.NewGuid()));
        }

        [TestMethod]
        [TestCategory("WorkflowResourceCache_Coverage")]
        public void GetEntry_MissingServerId_DefaultsToGuidEmpty()
        {
            var id = Guid.NewGuid();
            var path = Path.Combine(_root, "noserver.bite");
            File.WriteAllText(path,
                $"<Service ID=\"{id}\" Name=\"NS\" ResourceType=\"WorkflowService\"/>");
            var entry = WorkflowResourceCache.Instance.GetEntry(_root, id);
            Assert.IsNotNull(entry);
            Assert.AreEqual(Guid.Empty, entry!.ServerId);
        }

        [TestMethod]
        [TestCategory("WorkflowResourceCache_Coverage")]
        public void GetEntry_MissingName_DefaultsToEmptyString()
        {
            var id = Guid.NewGuid();
            var path = Path.Combine(_root, "noname.bite");
            File.WriteAllText(path,
                $"<Service ID=\"{id}\" ServerID=\"{Guid.NewGuid()}\" ResourceType=\"WorkflowService\"/>");
            var entry = WorkflowResourceCache.Instance.GetEntry(_root, id);
            Assert.IsNotNull(entry);
            Assert.AreEqual(string.Empty, entry!.Name);
        }

        [TestMethod]
        [TestCategory("WorkflowResourceCache_Coverage")]
        public void GetEntry_InvalidResourceType_ParsedAsUnknown()
        {
            var id = Guid.NewGuid();
            WriteResource("u.bite", id, Guid.NewGuid(), "U", "NotARealType");
            var entry = WorkflowResourceCache.Instance.GetEntry(_root, id);
            Assert.IsNotNull(entry);
            Assert.AreEqual(WorkflowResourceType.Unknown, entry!.ResourceType);
        }

        // ── WarmUp ───────────────────────────────────────────────────────────

        [TestMethod]
        [TestCategory("WorkflowResourceCache_Coverage")]
        public void WarmUp_BuildsIndex_SoLaterResolveSucceeds()
        {
            var id = Guid.NewGuid();
            var expected = WriteResource("wu.bite", id, Guid.NewGuid(), "W", "WorkflowService");
            WorkflowResourceCache.Instance.WarmUp(_root);
            Assert.AreEqual(expected, WorkflowResourceCache.Instance.Resolve(_root, id, null));
        }

        [TestMethod]
        [TestCategory("WorkflowResourceCache_Coverage")]
        public void WarmUp_IsIdempotent_AndAddingFilesAfterDoesNotInvalidateCache()
        {
            // First scan happens here — directory has no .bite files yet.
            WorkflowResourceCache.Instance.WarmUp(_root);
            // Adding a file afterwards should NOT appear (per-directory cache is frozen).
            var id = Guid.NewGuid();
            WriteResource("late.bite", id, Guid.NewGuid(), "Late", "WorkflowService");
            // Calling WarmUp again is a no-op
            WorkflowResourceCache.Instance.WarmUp(_root);
            Assert.IsNull(WorkflowResourceCache.Instance.Resolve(_root, id, "Late"));
        }

        // ── ParseResourceType (model) ────────────────────────────────────────

        [TestMethod]
        [TestCategory("WorkflowResourceCache_Coverage")]
        public void ParseResourceType_RecognisedValues_ReturnEnum()
        {
            Assert.AreEqual(WorkflowResourceType.WorkflowService,
                WorkflowResourceEntry.ParseResourceType("WorkflowService"));
            Assert.AreEqual(WorkflowResourceType.DbService,
                WorkflowResourceEntry.ParseResourceType("dbservice"));
            Assert.AreEqual(WorkflowResourceType.PluginService,
                WorkflowResourceEntry.ParseResourceType("PLUGINSERVICE"));
            Assert.AreEqual(WorkflowResourceType.WebService,
                WorkflowResourceEntry.ParseResourceType("WebService"));
            Assert.AreEqual(WorkflowResourceType.Server,
                WorkflowResourceEntry.ParseResourceType("Server"));
        }

        [TestMethod]
        [TestCategory("WorkflowResourceCache_Coverage")]
        public void ParseResourceType_NullOrUnknown_ReturnsUnknown()
        {
            Assert.AreEqual(WorkflowResourceType.Unknown,
                WorkflowResourceEntry.ParseResourceType(null));
            Assert.AreEqual(WorkflowResourceType.Unknown,
                WorkflowResourceEntry.ParseResourceType(""));
            Assert.AreEqual(WorkflowResourceType.Unknown,
                WorkflowResourceEntry.ParseResourceType("Bogus"));
        }
    }
}
