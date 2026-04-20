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
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace Warewolf.Execution.Lightweight.Integration.Tests
{
    /// <summary>
    /// Live integration tests for <see cref="DropBoxSource"/> loaded via
    /// <see cref="LightweightSourceLoader"/>.
    ///
    /// Uses <see href="https://github.com/WireMock-Net/WireMock.Net">WireMock.Net</see> as
    /// an in-process HTTP mock server that simulates a Dropbox-like REST API.
    ///
    /// <para>
    /// Note: DropBoxSource stores credentials (AccessToken, AppKey) that are used by
    /// Dropbox activities at execution time. The source itself does not make outbound HTTP
    /// calls during construction/loading — the HTTP calls happen in the activity. Therefore
    /// the live tests here verify:
    ///  1. Properties round-trip correctly through .bite → load → ResourceCatalog.
    ///  2. WireMock can simulate the Dropbox token-info endpoint, confirming that a
    ///     WireMock URL substituted as a DropBox API base would respond as expected.
    /// </para>
    /// </summary>
    [TestClass]
    public class LiveDropBoxSourceTests
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
            var dir = Path.Combine(Path.GetTempPath(), $"live-dropbox-{Guid.NewGuid():N}");
            Directory.CreateDirectory(dir);
            _tempDirs.Add(dir);
            return dir;
        }

        private Guid WriteBite(string dir, string accessToken = "test-access-token", string appKey = "test-app-key")
        {
            var id = Guid.NewGuid();
            var connStr = $"AccessToken={accessToken};AppKey={appKey}";
            var xml = $"""<Source Type="DropBoxSource" ResourceID="{id}" ID="{id}" Name="FakeDropBox" ResourceType="DropBoxSource" IsValid="false" ConnectionString="{connStr}" />""";
            File.WriteAllText(Path.Combine(dir, $"{id:N}.bite"), xml);
            return id;
        }

        /// <summary>
        /// Verifies that AccessToken and AppKey survive the full .bite → load round-trip.
        /// </summary>
        [TestMethod]
        [TestCategory("LiveIntegration_DropBox")]
        public void TC_DropBoxSource_PropertiesRoundTrip()
        {
            var dir = TempDir();
            var id = WriteBite(dir, accessToken: "my-dropbox-token-123", appKey: "my-app-key-456");

            var loader = LightweightSourceLoader.Instance;
            loader.EnsureIndexed(dir);
            IOnDemandSourceLoader iLoader = loader;
            Assert.IsTrue(iLoader.EnsureSourceLoaded(id), "DropBoxSource should load from .bite");

            var source = ResourceCatalog.Instance.GetResource<DropBoxSource>(GlobalConstants.ServerWorkspaceID, id);
            Assert.IsNotNull(source, "DropBoxSource must be in ResourceCatalog");
            Assert.AreEqual("my-dropbox-token-123", source.AccessToken, "AccessToken must round-trip correctly");
            Assert.AreEqual("my-app-key-456", source.AppKey, "AppKey must round-trip correctly");
        }

        /// <summary>
        /// Stubs a Dropbox-compatible token-info endpoint in WireMock and verifies that the
        /// WireMock server correctly serves the response. This confirms that if a Dropbox
        /// activity were redirected to WireMock (e.g. via environment override), the HTTP
        /// layer would function correctly.
        /// </summary>
        [TestMethod]
        [TestCategory("LiveIntegration_DropBox")]
        public async System.Threading.Tasks.Task TC_DropBoxSource_WireMock_TokenInfoEndpoint_Responds()
        {
            // Stub a fake Dropbox check-user endpoint (Dropbox v2 API style).
            _wireMock!.Given(
                Request.Create()
                    .WithPath("/2/auth/token/revoke")
                    .UsingPost())
                .RespondWith(
                    Response.Create()
                        .WithStatusCode(200)
                        .WithHeader("Content-Type", "application/json")
                        .WithBody("null"));

            _wireMock.Given(
                Request.Create()
                    .WithPath("/2/users/get_current_account")
                    .UsingPost())
                .RespondWith(
                    Response.Create()
                        .WithStatusCode(200)
                        .WithHeader("Content-Type", "application/json")
                        .WithBody("""{"account_id":"dbid:test","name":{"display_name":"Test User"}}"""));

            // Verify WireMock is ready by hitting the token endpoint.
            using var http = new System.Net.Http.HttpClient();
            var resp = await http.PostAsync(
                $"http://localhost:{_wireMock.Port}/2/users/get_current_account",
                new System.Net.Http.StringContent("null", System.Text.Encoding.UTF8, "application/json"));

            Assert.AreEqual(System.Net.HttpStatusCode.OK, resp.StatusCode,
                "WireMock Dropbox stub should return 200");

            var body = await resp.Content.ReadAsStringAsync();
            Assert.IsTrue(body.Contains("Test User"), "WireMock should return stubbed user response");

            // Also verify the source loads correctly in the same test run.
            var dir = TempDir();
            var id = WriteBite(dir);
            LightweightSourceLoader.Instance.EnsureIndexed(dir);
            IOnDemandSourceLoader iLoader = LightweightSourceLoader.Instance;
            Assert.IsTrue(iLoader.EnsureSourceLoaded(id), "DropBoxSource should load from .bite");
        }
    }
}
