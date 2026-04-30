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

namespace Warewolf.Execution.Lightweight.Integration.Tests
{
    /// <summary>
    /// Integration tests for <see cref="LightweightSourceLoader"/> verifying real disk I/O and
    /// <see cref="ResourceCatalog"/> registration for all five new source types.
    ///
    /// These tests do NOT require any external services — they write real-format .bite XML files
    /// to temp directories, invoke the loader, and assert that each source type ends up correctly
    /// indexed and registered in <see cref="ResourceCatalog.Instance"/>.
    ///
    /// If a test requires a live external service (e.g. a real SMTP server, SharePoint site) it
    /// should be marked <c>[TestCategory("RequiresExternalService")]</c> and call
    /// <c>Assert.Inconclusive()</c> when the endpoint is unreachable.
    /// </summary>
    [TestClass]
    public class SourceLoaderIntegrationTests
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
            AmbientSourceLoader.Clear();
        }

        // ── Helpers ───────────────────────────────────────────────────────────────────────────────

        private string CreateTempDir()
        {
            var dir = Path.Combine(Path.GetTempPath(), $"lwsl-int-{Guid.NewGuid():N}");
            Directory.CreateDirectory(dir);
            _tempDirs.Add(dir);
            return dir;
        }

        private static Guid WriteBite(string dir, string typeAttr, string connectionString = "")
        {
            var id = Guid.NewGuid();
            var xml = $"""
                <Source Type="{typeAttr}" ResourceID="{id}" ID="{id}" Name="Int-{typeAttr}" ResourceType="{typeAttr}" IsValid="false" ConnectionString="{connectionString}" />
                """;
            File.WriteAllText(Path.Combine(dir, $"{id:N}.bite"), xml);
            return id;
        }

        private static IOnDemandSourceLoader IndexDir(string dir)
        {
            var loader = LightweightSourceLoader.Instance;
            loader.EnsureIndexed(dir);
            return loader;
        }

        private static bool IsInCatalog(Guid id)
        {
            if (!ResourceCatalog.Instance.WorkspaceResources
                    .TryGetValue(GlobalConstants.ServerWorkspaceID, out var ws))
                return false;
            lock (ws)
                return ws.Any(r => r.ResourceID == id);
        }

        // ── TC1: all five source types found in a single directory ────────────────────────────────

        [TestMethod]
        [TestCategory("Integration")]
        public void TC_AllFiveSourceTypes_AreFoundInTempDirectory()
        {
            var dir = CreateTempDir();
            var emailId       = WriteBite(dir, "EmailSource",       "Host=localhost;Port=25;EnableSsl=false;Timeout=100000;UserName=;Password=");
            var exchangeId    = WriteBite(dir, "ExchangeSource",    "AutoDiscoverUrl=https://autodiscover.example.com/;UserName=user;Password=pass;WindowsAuthentication=false");
            var dropboxId     = WriteBite(dir, "DropBoxSource",     "AccessToken=sometoken;AppKey=somekey");
            var sharepointId  = WriteBite(dir, "SharepointSource",  "Server=http://sharepoint.local;AuthenticationType=Windows;UserName=;Password=");
            var elasticId     = WriteBite(dir, "ElasticsearchSource","HostName=localhost;Port=9200;AuthenticationType=Anonymous");

            var loader = IndexDir(dir);

            Assert.IsTrue(loader.EnsureSourceLoaded(emailId),      "EmailSource should be loadable");
            Assert.IsTrue(loader.EnsureSourceLoaded(exchangeId),   "ExchangeSource should be loadable");
            Assert.IsTrue(loader.EnsureSourceLoaded(dropboxId),    "DropBoxSource should be loadable");
            Assert.IsTrue(loader.EnsureSourceLoaded(sharepointId), "SharepointSource should be loadable");
            Assert.IsTrue(loader.EnsureSourceLoaded(elasticId),    "ElasticsearchSource should be loadable");
        }

        // ── TC2-6: catalog population per source type ─────────────────────────────────────────────

        [TestMethod]
        [TestCategory("Integration")]
        public void TC_EnsureSourceLoaded_PopulatesCatalog_Email()
        {
            var dir = CreateTempDir();
            var id = WriteBite(dir, "EmailSource", "Host=localhost;Port=25;EnableSsl=false;Timeout=100000;UserName=;Password=");
            var loader = IndexDir(dir);

            loader.EnsureSourceLoaded(id);

            Assert.IsTrue(IsInCatalog(id), "EmailSource should be present in ResourceCatalog after load");
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void TC_EnsureSourceLoaded_PopulatesCatalog_Exchange()
        {
            var dir = CreateTempDir();
            var id = WriteBite(dir, "ExchangeSource", "AutoDiscoverUrl=https://autodiscover.example.com/;UserName=user;Password=pass;WindowsAuthentication=false");
            var loader = IndexDir(dir);

            loader.EnsureSourceLoaded(id);

            Assert.IsTrue(IsInCatalog(id), "ExchangeSource should be present in ResourceCatalog after load");
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void TC_EnsureSourceLoaded_PopulatesCatalog_DropBox()
        {
            var dir = CreateTempDir();
            var id = WriteBite(dir, "DropBoxSource", "AccessToken=sometoken;AppKey=somekey");
            var loader = IndexDir(dir);

            loader.EnsureSourceLoaded(id);

            Assert.IsTrue(IsInCatalog(id), "DropBoxSource should be present in ResourceCatalog after load");
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void TC_EnsureSourceLoaded_PopulatesCatalog_Sharepoint()
        {
            var dir = CreateTempDir();
            var id = WriteBite(dir, "SharepointSource", "Server=http://sharepoint.local;AuthenticationType=Windows;UserName=;Password=");
            var loader = IndexDir(dir);

            loader.EnsureSourceLoaded(id);

            Assert.IsTrue(IsInCatalog(id), "SharepointSource should be present in ResourceCatalog after load");
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void TC_EnsureSourceLoaded_PopulatesCatalog_Elasticsearch()
        {
            var dir = CreateTempDir();
            var id = WriteBite(dir, "ElasticsearchSource", "HostName=localhost;Port=9200;AuthenticationType=Anonymous");
            var loader = IndexDir(dir);

            loader.EnsureSourceLoaded(id);

            Assert.IsTrue(IsInCatalog(id), "ElasticsearchSource should be present in ResourceCatalog after load");
        }

        // ── TC7: second call for same ID is a no-op ────────────────────────────────────────────────

        [TestMethod]
        [TestCategory("Integration")]
        public void TC_SecondCallSameId_IsNoop()
        {
            var dir = CreateTempDir();
            var id = WriteBite(dir, "EmailSource", "Host=localhost;Port=25;EnableSsl=false;Timeout=100000;UserName=;Password=");
            var loader = IndexDir(dir);

            loader.EnsureSourceLoaded(id);
            loader.EnsureSourceLoaded(id); // second call

            // Catalog must still contain exactly one entry for this ID.
            var count = 0;
            if (ResourceCatalog.Instance.WorkspaceResources
                    .TryGetValue(GlobalConstants.ServerWorkspaceID, out var ws))
            {
                lock (ws)
                    count = ws.Count(r => r.ResourceID == id);
            }
            Assert.AreEqual(1, count, "A second EnsureSourceLoaded call should not duplicate the catalog entry");
        }
    }
}
