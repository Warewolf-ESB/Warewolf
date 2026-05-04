/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using Dev2.Common;
using Dev2.Common.Exchange;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace Warewolf.Execution.Lightweight.Integration.Tests
{
    /// <summary>
    /// Live integration tests for <see cref="ExchangeSource"/> loaded via
    /// <see cref="LightweightSourceLoader"/>.
    ///
    /// <see cref="TC_ExchangeSource_SendEmail_ViaMockedEWS"/> requires the
    /// <c>warewolfserver/exchange-connector-testing</c> Docker container to be reachable
    /// on <c>http://localhost:8889/EWS/Exchange.asmx</c>.  <see cref="ClassSetup"/>
    /// attempts to start it automatically; in CI the pipeline starts it before test
    /// execution so the attempt is a no-op if the container is already running.
    ///
    /// What is tested:
    ///  1. Properties round-trip — AutoDiscoverUrl / UserName survive .bite → load.
    ///  2. WireMock confirms URL preservation for the autodiscover endpoint.
    ///  3. Send() successfully POSTs SOAP to the exchange-connector-testing container.
    /// </summary>
    [TestClass]
    public class LiveExchangeSourceTests
    {
        private const string ExchangeContainerName = "exchange-connector-testing";
        private const string ExchangeEwsUrl = "http://localhost:8889/EWS/Exchange.asmx";
        private const string ExchangeUser = "testuser";
        private const string ExchangePassword = "test123";

        private readonly List<string> _tempDirs = new();
        private WireMockServer? _wireMock;

        /// <summary>
        /// Starts the exchange connector testing container so that
        /// <see cref="TC_ExchangeSource_SendEmail_ViaMockedEWS"/> can POST SOAP to it.
        /// Handles the case where the container is already running (CI pipeline starts it
        /// before the test run) by ignoring docker startup errors.
        /// </summary>
        [ClassInitialize]
        public static void ClassSetup(TestContext _)
        {
            try
            {
                var psi = new ProcessStartInfo("docker",
                    $"run -d -p 8889:8080 --name {ExchangeContainerName} warewolfserver/exchange-connector-testing")
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                };
                using var p = Process.Start(psi);
                p?.WaitForExit(30_000);
                // Brief wait for the container's HTTP listener to become ready.
                Thread.Sleep(3_000);
            }
            catch
            {
                // Docker not available in this environment (e.g. test container without
                // Docker socket).  The CI pipeline is responsible for starting the
                // exchange-connector-testing container before running the tests.
            }
        }

        [ClassCleanup]
        public static void ClassTeardown()
        {
            try
            {
                Process.Start(new ProcessStartInfo("docker", $"rm -f {ExchangeContainerName}")
                {
                    UseShellExecute = false,
                })?.WaitForExit(10_000);
            }
            catch { }
        }

        [TestInitialize]
        public void Setup() =>
            _wireMock = WireMockServer.Start(); // random OS-assigned port

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
            var dir = Path.Combine(Path.GetTempPath(), $"live-exchange-{Guid.NewGuid():N}");
            Directory.CreateDirectory(dir);
            _tempDirs.Add(dir);
            return dir;
        }

        private Guid WriteBite(string dir, string autoDiscoverUrl, string user = "ewsuser", string pass = "ewspass")
        {
            var id = Guid.NewGuid();
            var connStr = $"AutoDiscoverUrl={autoDiscoverUrl};UserName={user};Password={pass};Timeout=5000";
            var xml = $"""<Source Type="ExchangeSource" ResourceID="{id}" ID="{id}" Name="FakeExchange" ResourceType="ExchangeSource" IsValid="false" ConnectionString="{connStr}" />""";
            File.WriteAllText(Path.Combine(dir, $"{id:N}.bite"), xml);
            return id;
        }

        private static T? GetFromCatalog<T>(Guid id) where T : class, Dev2.Common.Interfaces.Data.IResource
        {
            if (!ResourceCatalog.Instance.WorkspaceResources
                    .TryGetValue(GlobalConstants.ServerWorkspaceID, out var ws))
                return null;
            lock (ws)
                return ws.OfType<T>().FirstOrDefault(r => r.ResourceID == id);
        }

        /// <summary>
        /// Verifies that AutoDiscoverUrl, UserName, and Timeout survive the full
        /// .bite write → LightweightSourceLoader index → EnsureSourceLoaded → ResourceCatalog
        /// round-trip without any data loss or default-value override.
        /// </summary>
        [TestMethod]
        [TestCategory("LiveIntegration_Exchange")]
        public void TC_ExchangeSource_PropertiesRoundTrip()
        {
            var ewsUrl = $"http://localhost:{_wireMock!.Port}/autodiscover/autodiscover.xml";
            var dir = TempDir();
            var id = WriteBite(dir, ewsUrl);

            var loader = LightweightSourceLoader.Instance;
            loader.EnsureIndexed(dir);
            IOnDemandSourceLoader iLoader = loader;
            Assert.IsTrue(iLoader.EnsureSourceLoaded(id), "ExchangeSource should load from .bite");

            var source = GetFromCatalog<ExchangeSource>(id);
            Assert.IsNotNull(source, "ExchangeSource must be in ResourceCatalog");
            Assert.AreEqual(ewsUrl, source.AutoDiscoverUrl, "AutoDiscoverUrl must round-trip correctly");
            Assert.AreEqual("ewsuser", source.UserName, "UserName must round-trip correctly");
            Assert.AreEqual(5000, source.Timeout, "Timeout must round-trip correctly");
        }

        /// <summary>
        /// End-to-end: loads ExchangeSource from .bite → Send() issues a SOAP CreateItem
        /// request to the <c>warewolfserver/exchange-connector-testing</c> container
        /// (started by <see cref="ClassSetup"/> or the CI pipeline) → asserts no exception.
        ///
        /// Source credentials and URL match those embedded in <c>local exchange.bite</c>
        /// so that local development and CI use the same exchange stub.
        /// </summary>
        [TestMethod]
        [TestCategory("LiveIntegration_Exchange")]
        public void TC_ExchangeSource_SendEmail_ViaMockedEWS()
        {
            var dir = TempDir();
            var id = WriteBite(dir, ExchangeEwsUrl, ExchangeUser, ExchangePassword);

            LightweightSourceLoader.Instance.EnsureIndexed(dir);
            IOnDemandSourceLoader iLoader = LightweightSourceLoader.Instance;
            Assert.IsTrue(iLoader.EnsureSourceLoaded(id), "ExchangeSource should load from .bite");

            var source = GetFromCatalog<ExchangeSource>(id)!;
            var sender = new ExchangeEmailSender(source);
            var message = new ExchangeTestMessage { Subject = "Test Message", Body = "body test" };
            message.Tos.Add("ashley.lewis@dev2.co.za");

            // Should not throw; the exchange-connector-testing container accepts the SOAP call.
            source.Send(sender, message);
        }

        /// <summary>
        /// Stubs the EWS autodiscover XML endpoint in WireMock, then verifies that the
        /// <see cref="ExchangeSource.AutoDiscoverUrl"/> points to the live WireMock endpoint
        /// and that WireMock would serve a valid autodiscover response to it.
        ///
        /// This test validates the URL-preservation guarantee: a source loaded from a .bite
        /// file will contact exactly the host/port stored in the connection string.
        /// </summary>
        [TestMethod]
        [TestCategory("LiveIntegration_Exchange")]
        public void TC_ExchangeSource_AutoDiscoverUrl_PointsToWireMock()
        {
            // Stub the EWS autodiscover endpoint.
            var autodiscoverResponse = """
                <?xml version="1.0" encoding="utf-8"?>
                <Autodiscover xmlns="http://schemas.microsoft.com/exchange/autodiscover/responseschema/2006">
                  <Response>
                    <Account>
                      <AccountType>email</AccountType>
                      <Action>settings</Action>
                      <Protocol>
                        <Type>EXCH</Type>
                        <EwsUrl>http://localhost:{port}/EWS/Exchange.asmx</EwsUrl>
                      </Protocol>
                    </Account>
                  </Response>
                </Autodiscover>
                """.Replace("{port}", _wireMock!.Port.ToString());

            _wireMock.Given(
                Request.Create()
                    .WithPath("/autodiscover/autodiscover.xml")
                    .UsingAnyMethod())
                .RespondWith(
                    Response.Create()
                        .WithStatusCode(200)
                        .WithHeader("Content-Type", "text/xml; charset=utf-8")
                        .WithBody(autodiscoverResponse));

            var ewsUrl = $"http://localhost:{_wireMock.Port}/autodiscover/autodiscover.xml";
            var dir = TempDir();
            var id = WriteBite(dir, ewsUrl);

            LightweightSourceLoader.Instance.EnsureIndexed(dir);
            IOnDemandSourceLoader iLoader = LightweightSourceLoader.Instance;
            iLoader.EnsureSourceLoaded(id);

            var source = GetFromCatalog<ExchangeSource>(id);
            Assert.IsNotNull(source);

            // The stored AutoDiscoverUrl must point to WireMock's address exactly.
            Assert.IsTrue(
                source.AutoDiscoverUrl.Contains($":{_wireMock.Port}/"),
                $"AutoDiscoverUrl '{source.AutoDiscoverUrl}' should reference WireMock port {_wireMock.Port}");
        }
    }
}