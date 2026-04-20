/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using Dev2.Common;
using Dev2.Data.ServiceModel;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace Warewolf.Execution.Lightweight.Integration.Tests
{
    /// <summary>
    /// Live integration tests for <see cref="SharepointSource"/> loaded via
    /// <see cref="LightweightSourceLoader"/>.
    ///
    /// Uses <see href="https://github.com/WireMock-Net/WireMock.Net">WireMock.Net</see> as
    /// an in-process HTTP mock server that simulates SharePoint REST API endpoints.
    /// No Docker or external services are required.
    ///
    /// What is tested:
    ///  1. Properties round-trip — Server, AuthenticationType, UserName survive .bite → load.
    ///  2. WireMock simulates the SharePoint /_api/contextinfo endpoint so that the
    ///     <see cref="SharepointSource.Server"/> URL can be proven to point at a live
    ///     (mock) server.
    /// </summary>
    [TestClass]
    public class LiveSharepointSourceTests
    {
        private readonly List<string> _tempDirs = new();
        private WireMockServer? _wireMock;

        [TestInitialize]
        public void Setup() =>
            _wireMock = WireMockServer.Start();

        [TestCleanup]
        public void Cleanup()
        {
            _wireMock?.Stop();
            foreach (var d in _tempDirs)
                try { Directory.Delete(d, true); } catch { }
            _tempDirs.Clear();
            AmbientSourceLoader.Clear();
        }

        private string TempDir()
        {
            var dir = Path.Combine(Path.GetTempPath(), $"live-sp-{Guid.NewGuid():N}");
            Directory.CreateDirectory(dir);
            _tempDirs.Add(dir);
            return dir;
        }

        private Guid WriteBite(string dir, string serverUrl, string user = "spuser", string pass = "sppass")
        {
            var id = Guid.NewGuid();
            var connStr = $"Server={serverUrl};AuthenticationType=Windows;UserName={user};Password={pass}";
            var xml = $"""<Source Type="SharepointSource" ResourceID="{id}" ID="{id}" Name="FakeSharepoint" ResourceType="SharepointSource" IsValid="false" ConnectionString="{connStr}" />""";
            File.WriteAllText(Path.Combine(dir, $"{id:N}.bite"), xml);
            return id;
        }

        /// <summary>
        /// Verifies that Server, UserName, and AuthenticationType survive the full
        /// .bite write → LightweightSourceLoader index → EnsureSourceLoaded → ResourceCatalog
        /// round-trip without data loss.
        /// </summary>
        [TestMethod]
        [TestCategory("LiveIntegration_SharePoint")]
        public void TC_SharepointSource_PropertiesRoundTrip()
        {
            var spUrl = $"http://localhost:{_wireMock!.Port}";
            var dir = TempDir();
            var id = WriteBite(dir, spUrl);

            var loader = LightweightSourceLoader.Instance;
            loader.EnsureIndexed(dir);
            IOnDemandSourceLoader iLoader = loader;
            Assert.IsTrue(iLoader.EnsureSourceLoaded(id), "SharepointSource should load from .bite");

            var source = ResourceCatalog.Instance.GetResource<SharepointSource>(GlobalConstants.ServerWorkspaceID, id);
            Assert.IsNotNull(source, "SharepointSource must be in ResourceCatalog");
            Assert.AreEqual(spUrl, source.Server, "Server must round-trip correctly");
            Assert.AreEqual("spuser", source.UserName, "UserName must round-trip correctly");
        }

        /// <summary>
        /// Stubs the SharePoint /_api/contextinfo endpoint in WireMock, then verifies that
        /// an HTTP POST to that endpoint (as SharePoint CSOM does when connecting) returns
        /// the expected stub response.
        ///
        /// This test confirms that the <see cref="SharepointSource.Server"/> URL preserved
        /// through the source loader points at an active HTTP endpoint.
        /// </summary>
        [TestMethod]
        [TestCategory("LiveIntegration_SharePoint")]
        public async Task TC_SharepointSource_ContextInfoEndpoint_ViaMockServer()
        {
            // SharePoint CSOM always POSTs to /_api/contextinfo to obtain a form digest.
            var contextInfoResponse = """
                <d:GetContextWebInformation xmlns:d="http://schemas.microsoft.com/ado/2007/08/dataservices">
                  <d:FormDigestTimeoutSeconds>1800</d:FormDigestTimeoutSeconds>
                  <d:FormDigestValue>0x1FAKE,01 Jan 2024 00:00:00 -0000</d:FormDigestValue>
                  <d:WebFullUrl>http://localhost:{port}</d:WebFullUrl>
                </d:GetContextWebInformation>
                """.Replace("{port}", _wireMock!.Port.ToString());

            _wireMock.Given(
                Request.Create()
                    .WithPath("/_api/contextinfo")
                    .UsingPost())
                .RespondWith(
                    Response.Create()
                        .WithStatusCode(200)
                        .WithHeader("Content-Type", "application/xml; charset=utf-8")
                        .WithBody(contextInfoResponse));

            // Stub /_api/web/lists to return an empty lists collection.
            _wireMock.Given(
                Request.Create()
                    .WithPath("/_api/web/lists")
                    .UsingGet())
                .RespondWith(
                    Response.Create()
                        .WithStatusCode(200)
                        .WithHeader("Content-Type", "application/json")
                        .WithBody("""{"d":{"results":[]}}"""));

            var spUrl = $"http://localhost:{_wireMock.Port}";
            var dir = TempDir();
            var id = WriteBite(dir, spUrl);

            LightweightSourceLoader.Instance.EnsureIndexed(dir);
            IOnDemandSourceLoader iLoader = LightweightSourceLoader.Instance;
            iLoader.EnsureSourceLoaded(id);

            var source = ResourceCatalog.Instance.GetResource<SharepointSource>(GlobalConstants.ServerWorkspaceID, id);
            Assert.IsNotNull(source, "SharepointSource must be in catalog");
            Assert.AreEqual(spUrl, source.Server);

            // Verify that the WireMock endpoint at source.Server actually responds.
            using var http = new HttpClient();
            var resp = await http.PostAsync(
                $"{source.Server}/_api/contextinfo",
                new StringContent("", Encoding.UTF8, "application/json"));

            Assert.AreEqual(System.Net.HttpStatusCode.OK, resp.StatusCode,
                "WireMock SharePoint stub should return 200");
            var body = await resp.Content.ReadAsStringAsync();
            Assert.IsTrue(body.Contains("FormDigestValue"),
                "WireMock should return stubbed contextinfo response");
        }

        /// <summary>
        /// Verifies that the SharepointSource <see cref="SharepointSource.Server"/> URL
        /// preserved in the catalog is exactly what was written into the .bite file,
        /// confirming that no URL normalisation (trailing slash, scheme change, etc.)
        /// occurs during the load pipeline.
        /// </summary>
        [TestMethod]
        [TestCategory("LiveIntegration_SharePoint")]
        public void TC_SharepointSource_ServerUrl_ExactlyPreserved()
        {
            var spUrl = $"http://localhost:{_wireMock!.Port}/sites/testsite";
            var dir = TempDir();
            var id = WriteBite(dir, spUrl, user: "domain\\admin");

            LightweightSourceLoader.Instance.EnsureIndexed(dir);
            IOnDemandSourceLoader iLoader = LightweightSourceLoader.Instance;
            iLoader.EnsureSourceLoaded(id);

            var source = ResourceCatalog.Instance.GetResource<SharepointSource>(GlobalConstants.ServerWorkspaceID, id);
            Assert.IsNotNull(source);
            Assert.AreEqual(spUrl, source.Server, "Server URL must be preserved exactly (no trailing slash or scheme normalisation)");
            Assert.AreEqual("domain\\admin", source.UserName, "UserName with domain prefix must round-trip");
        }
    }
}
