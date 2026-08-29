/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using Dev2.Common;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Warewolf.Execution.Lightweight.Tests
{
    /// <summary>
    /// Unit tests for <see cref="LightweightSourceLoader"/> covering the five new source types
    /// (Email, Exchange, DropBox, Sharepoint, Elasticsearch) as well as regression guards for
    /// existing types (DbSource, WebSource) and edge-case / concurrency scenarios.
    ///
    /// All tests write minimal synthetic .bite XML files to isolated temp directories and
    /// clean them up in TestCleanup so they do not affect other tests or the file system.
    ///
    /// Note: <see cref="LightweightSourceLoader"/> is a singleton — tests use distinct GUIDs
    /// and distinct temp directories so they do not interfere with each other even though they
    /// share the same instance.
    /// </summary>
    [TestClass]
    public class LightweightSourceLoaderTests
    {
        private readonly List<string> _tempDirs = new();

        [TestCleanup]
        public void Cleanup()
        {
            foreach (var dir in _tempDirs)
            {
                try { Directory.Delete(dir, recursive: true); }
                catch { /* best-effort */ }
            }
            _tempDirs.Clear();

            // Deregister the ambient loader so tests that follow start clean.
            AmbientSourceLoader.Clear();
        }

        // ── Helpers ───────────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Creates a temp directory, writes a single .bite file for the given source type and
        /// returns the GUID that was embedded in the file.
        /// </summary>
        private (string dir, Guid id) CreateBiteDir(string sourceType, string connectionString = "")
        {
            var dir = Path.Combine(Path.GetTempPath(), $"lwsl-tests-{Guid.NewGuid():N}");
            Directory.CreateDirectory(dir);
            _tempDirs.Add(dir);

            var id = Guid.NewGuid();
            var xml = BuildBiteXml(sourceType, id, connectionString);
            File.WriteAllText(Path.Combine(dir, $"{id:N}.bite"), xml);
            return (dir, id);
        }

        private static string BuildBiteXml(string typeAttr, Guid id, string connectionString = "")
            => $"""
                <Source Type="{typeAttr}" ResourceID="{id}" ID="{id}" Name="Test-{typeAttr}" ResourceType="{typeAttr}" IsValid="false" ConnectionString="{connectionString}" />
                """;

        private static IOnDemandSourceLoader GetLoader(string dir)
        {
            var loader = LightweightSourceLoader.Instance;
            loader.EnsureIndexed(dir);
            return loader;
        }

        // ── New source types ──────────────────────────────────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void TC_EmailSource_IsIndexed()
        {
            var (dir, id) = CreateBiteDir("EmailSource");
            var loader = GetLoader(dir);

            var result = loader.EnsureSourceLoaded(id);

            Assert.IsTrue(result, "EnsureSourceLoaded should return true for EmailSource");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void TC_ExchangeSource_IsIndexed()
        {
            var (dir, id) = CreateBiteDir("ExchangeSource");
            var loader = GetLoader(dir);

            var result = loader.EnsureSourceLoaded(id);

            Assert.IsTrue(result, "EnsureSourceLoaded should return true for ExchangeSource");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void TC_DropBoxSource_IsIndexed()
        {
            var (dir, id) = CreateBiteDir("DropBoxSource", "AccessToken=test;AppKey=testkey");
            var loader = GetLoader(dir);

            var result = loader.EnsureSourceLoaded(id);

            Assert.IsTrue(result, "EnsureSourceLoaded should return true for DropBoxSource");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void TC_SharepointSource_IsIndexed()
        {
            var (dir, id) = CreateBiteDir("SharepointSource", "Server=http://localhost;AuthenticationType=Windows;UserName=;Password=");
            var loader = GetLoader(dir);

            var result = loader.EnsureSourceLoaded(id);

            Assert.IsTrue(result, "EnsureSourceLoaded should return true for SharepointSource");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void TC_ElasticsearchSource_IsIndexed()
        {
            var (dir, id) = CreateBiteDir("ElasticsearchSource", "HostName=localhost;Port=9200;AuthenticationType=Anonymous");
            var loader = GetLoader(dir);

            var result = loader.EnsureSourceLoaded(id);

            Assert.IsTrue(result, "EnsureSourceLoaded should return true for ElasticsearchSource");
        }

        // ── Regression guards ─────────────────────────────────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void TC_ExistingDbSource_StillWorks()
        {
            var (dir, id) = CreateBiteDir("DbSource", "Data Source=localhost;Initial Catalog=test");
            var loader = GetLoader(dir);

            var result = loader.EnsureSourceLoaded(id);

            Assert.IsTrue(result, "EnsureSourceLoaded should still return true for DbSource (regression)");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void TC_ExistingWebSource_StillWorks()
        {
            var (dir, id) = CreateBiteDir("WebSource", "Address=http://localhost/api");
            var loader = GetLoader(dir);

            var result = loader.EnsureSourceLoaded(id);

            Assert.IsTrue(result, "EnsureSourceLoaded should still return true for WebSource (regression)");
        }

        // ── Edge cases ────────────────────────────────────────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void TC_UnknownSourceType_IsSkipped()
        {
            var (dir, id) = CreateBiteDir("UnknownFooSource");
            var loader = GetLoader(dir);

            var result = loader.EnsureSourceLoaded(id);

            Assert.IsFalse(result, "EnsureSourceLoaded should return false for an unknown source type");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void TC_EmailSource_RegisteredInCatalog()
        {
            var (dir, id) = CreateBiteDir("EmailSource", "Host=localhost;Port=25;EnableSsl=false;Timeout=100000;UserName=;Password=");
            var loader = GetLoader(dir);

            loader.EnsureSourceLoaded(id);

            var found = false;
            if (ResourceCatalog.Instance.WorkspaceResources
                    .TryGetValue(GlobalConstants.ServerWorkspaceID, out var ws))
            {
                lock (ws)
                    found = ws.Any(r => r.ResourceID == id);
            }
            Assert.IsTrue(found, "EmailSource should be present in ResourceCatalog after EnsureSourceLoaded");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void TC_LoadFailure_DiagnosticsContainFoundButFailedMessage()
        {
            // Write a .bite file whose root element is valid XML (so TryPeekSourceId indexes it)
            // but whose content will throw during source construction (malformed inner XML).
            var dir = Path.Combine(Path.GetTempPath(), $"lwsl-tests-{Guid.NewGuid():N}");
            Directory.CreateDirectory(dir);
            _tempDirs.Add(dir);

            var id = Guid.NewGuid();
            // Valid root attrs so indexing succeeds, but broken child XML so XElement.Load fails.
            var brokenXml = $"<Source Type=\"SharepointSource\" ResourceID=\"{id}\" ID=\"{id}\" Name=\"Broken\"><<<<NOTXML";
            File.WriteAllText(Path.Combine(dir, $"{id:N}.bite"), brokenXml);

            var loader = LightweightSourceLoader.Instance;
            loader.EnsureIndexed(dir);

            var result = ((IOnDemandSourceLoader)loader).EnsureSourceLoaded(id);

            Assert.IsFalse(result, "EnsureSourceLoaded should return false when the file cannot be loaded");

            var diag = ((IOnDemandSourceLoader)loader).GetDiagnostics();
            StringAssert.Contains(diag, "found in index but failed to load",
                "Diagnostics should say 'found in index but failed to load', not the misleading 'NOT found' message, " +
                "when the source IS indexed but LoadSourceFile throws.");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void TC_ConcurrentLoads_SameId_OnlyOneRegistered()
        {
            var (dir, id) = CreateBiteDir("EmailSource", "Host=localhost;Port=25;EnableSsl=false;Timeout=100000;UserName=;Password=");
            var loader = GetLoader(dir);

            // Fire 20 concurrent loads of the same ID.
            var tasks = Enumerable.Range(0, 20)
                .Select(_ => Task.Run(() => loader.EnsureSourceLoaded(id)))
                .ToArray();
            Task.WaitAll(tasks);

            // All should have returned true.
            Assert.IsTrue(tasks.All(t => t.Result), "All concurrent loads should succeed");

            // Exactly one entry in the catalog for this ID.
            if (ResourceCatalog.Instance.WorkspaceResources
                    .TryGetValue(GlobalConstants.ServerWorkspaceID, out var ws))
            {
                int count;
                lock (ws)
                    count = ws.Count(r => r.ResourceID == id);
                Assert.AreEqual(1, count, "ResourceCatalog should contain exactly one entry for the source ID");
            }
        }

        // ── InvalidateDirectory / Invalidate (add_source / edit_source staleness) ───────────────────
        // Fix for: add_source writes a new .bite file, but _directoryIndices is a Lazy built at
        // most once per directory with no staleness check — a source created AFTER an instance
        // already indexed its directory was invisible to EnsureSourceLoaded forever (reproduced
        // live against warewolfserver-mcp: a real, correctly-configured sourceId still resolved to
        // a null WebSource). InvalidateDirectory forces the next EnsureSourceLoaded to rebuild the
        // index from disk.

        /// <summary>
        /// Materializes <paramref name="dir"/>'s index (a Lazy that otherwise wouldn't build until
        /// first accessed) without permanently caching a negative result for any specific real ID —
        /// EnsureSourceLoaded's own per-ID cache (<c>_registeredIds</c>) commits its result (found
        /// or not) forever for whatever ID it's called with, so looking up the ID under test BEFORE
        /// it exists would poison that exact test, not reproduce the real bug: in production, an
        /// instance's index is materialized by resolving a DIFFERENT, already-existing source
        /// (e.g. an earlier workflow execution), never by a premature lookup of the new source's own
        /// not-yet-minted ID.
        /// </summary>
        private static void MaterializeIndex(LightweightSourceLoader loader) =>
            ((IOnDemandSourceLoader)loader).EnsureSourceLoaded(Guid.NewGuid());

        [TestMethod]
        [TestCategory("UnitTest")]
        public void TC_SourceAddedAfterIndexing_NotFound_WithoutInvalidateDirectory()
        {
            var dir = Path.Combine(Path.GetTempPath(), $"lwsl-tests-{Guid.NewGuid():N}");
            Directory.CreateDirectory(dir);
            _tempDirs.Add(dir);

            var loader = LightweightSourceLoader.Instance;
            loader.EnsureIndexed(dir);
            MaterializeIndex(loader);

            // Simulate add_source writing a new file AFTER the directory was already indexed.
            var newId = Guid.NewGuid();
            File.WriteAllText(Path.Combine(dir, $"{newId:N}.bite"), BuildBiteXml("WebSource", newId, "Address=http://localhost/api"));

            Assert.IsFalse(((IOnDemandSourceLoader)loader).EnsureSourceLoaded(newId),
                "Reproduces the bug: a source file written after the directory was indexed is invisible to EnsureSourceLoaded.");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void TC_SourceAddedAfterIndexing_FoundAfterInvalidateDirectory()
        {
            var dir = Path.Combine(Path.GetTempPath(), $"lwsl-tests-{Guid.NewGuid():N}");
            Directory.CreateDirectory(dir);
            _tempDirs.Add(dir);

            var loader = LightweightSourceLoader.Instance;
            loader.EnsureIndexed(dir);
            MaterializeIndex(loader);

            var newId = Guid.NewGuid();
            File.WriteAllText(Path.Combine(dir, $"{newId:N}.bite"), BuildBiteXml("WebSource", newId, "Address=http://localhost/api"));

            loader.InvalidateDirectory(dir);

            Assert.IsTrue(((IOnDemandSourceLoader)loader).EnsureSourceLoaded(newId),
                "After InvalidateDirectory, the newly-written source should be found on its first EnsureSourceLoaded call.");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void TC_InvalidateDirectory_OnNeverIndexedDirectory_DoesNotThrow()
        {
            var dir = Path.Combine(Path.GetTempPath(), $"lwsl-tests-{Guid.NewGuid():N}");
            _tempDirs.Add(dir); // deliberately not created — matches add_source's fresh-directory case

            Assert.IsFalse(string.IsNullOrEmpty(dir));
            LightweightSourceLoader.Instance.InvalidateDirectory(dir);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void TC_EditedSource_StaleUntilInvalidate_ThenReflectsNewContent()
        {
            var (dir, id) = CreateBiteDir("WebSource", "Address=http://old-host/api");
            var loader = GetLoader(dir);

            Assert.IsTrue(((IOnDemandSourceLoader)loader).EnsureSourceLoaded(id));
            var beforeEdit = ResolveWebSourceAddress(id);
            Assert.AreEqual("http://old-host/api", beforeEdit);

            // Simulate edit_source overwriting the SAME file path/ID with new content.
            File.WriteAllText(Path.Combine(dir, $"{id:N}.bite"), BuildBiteXml("WebSource", id, "Address=http://new-host/api"));

            Assert.IsTrue(((IOnDemandSourceLoader)loader).EnsureSourceLoaded(id),
                "Already-registered id short-circuits to true without re-reading the file.");
            Assert.AreEqual("http://old-host/api", ResolveWebSourceAddress(id),
                "Reproduces the bug: the edit is invisible until Invalidate(id) is called.");

            LightweightSourceLoader.Instance.Invalidate(id);

            Assert.IsTrue(((IOnDemandSourceLoader)loader).EnsureSourceLoaded(id));
            Assert.AreEqual("http://new-host/api", ResolveWebSourceAddress(id),
                "After Invalidate, the edited source's new content should be loaded.");
        }

        /// <summary>Reads the Address a registered WebSource currently holds in ResourceCatalog.</summary>
        private static string ResolveWebSourceAddress(Guid id)
        {
            if (ResourceCatalog.Instance.WorkspaceResources
                    .TryGetValue(GlobalConstants.ServerWorkspaceID, out var ws))
            {
                lock (ws)
                {
                    var source = ws.FirstOrDefault(r => r.ResourceID == id) as Dev2.Runtime.ServiceModel.Data.WebSource;
                    return source?.Address ?? string.Empty;
                }
            }
            return string.Empty;
        }
    }
}
