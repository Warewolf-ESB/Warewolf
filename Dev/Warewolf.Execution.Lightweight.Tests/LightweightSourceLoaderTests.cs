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
    }
}
