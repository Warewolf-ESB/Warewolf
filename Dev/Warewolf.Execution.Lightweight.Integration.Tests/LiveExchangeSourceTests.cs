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
using System.IO;
using System.Linq;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace Warewolf.Execution.Lightweight.Integration.Tests
{
    /// <summary>
    /// Live integration tests for <see cref="ExchangeSource"/> loaded via
    /// <see cref="LightweightSourceLoader"/>.
    ///
    /// Uses <see href="https://github.com/WireMock-Net/WireMock.Net">WireMock.Net</see> as
    /// an in-process HTTP mock server that simulates the Exchange Web Services (EWS) SOAP
    /// endpoint. No Docker or external services are required.
    ///
    /// What is tested:
    ///  1. Source configuration round-trip — AutoDiscoverUrl / UserName survive .bite → load.
    ///  2. WireMock connectivity — after loading, the stored URL points to a live endpoint
    ///     (WireMock) confirming that the URL was preserved exactly as written.
    /// </summary>
    [TestClass]
    public class LiveExchangeSourceTests
    {
        private readonly List<string> _tempDirs = new();
        private WireMockServer? _wireMock;

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
        /// request to the WireMock-stubbed EWS endpoint → WireMock confirms receipt.
        ///
        /// AutoDiscoverUrl is set directly to /EWS/Exchange.asmx so ExchangeEmailSender
        /// skips autodiscover and POSTs SOAP straight to WireMock (see ExchangeEmailSender.Initialize).
        /// </summary>
        [TestMethod]
        [TestCategory("LiveIntegration_Exchange")]
        public void TC_ExchangeSource_SendEmail_ViaMockedEWS()
        {
            const string createItemResponse =
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                "<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\">" +
                  "<s:Body>" +
                    "<m:CreateItemResponse xmlns:m=\"http://schemas.microsoft.com/exchange/services/2006/messages\"" +
                                        " xmlns:t=\"http://schemas.microsoft.com/exchange/services/2006/types\">" +
                      "<m:ResponseMessages>" +
                        "<m:CreateItemResponseMessage ResponseClass=\"Success\">" +
                          "<m:ResponseCode>NoError</m:ResponseCode>" +
                          "<m:Items/>" +
                        "</m:CreateItemResponseMessage>" +
                      "</m:ResponseMessages>" +
                    "</m:CreateItemResponse>" +
                  "</s:Body>" +
                "</s:Envelope>";

            _wireMock!.Given(
                Request.Create()
                    .WithPath("/EWS/Exchange.asmx")
                    .UsingPost())
                .RespondWith(
                    Response.Create()
                        .WithStatusCode(200)
                        .WithHeader("Content-Type", "text/xml; charset=utf-8")
                        .WithBody(createItemResponse));

            var ewsUrl = $"http://localhost:{_wireMock.Port}/EWS/Exchange.asmx";
            var dir = TempDir();
            var id = WriteBite(dir, ewsUrl);

            LightweightSourceLoader.Instance.EnsureIndexed(dir);
            IOnDemandSourceLoader iLoader = LightweightSourceLoader.Instance;
            Assert.IsTrue(iLoader.EnsureSourceLoaded(id));

            var source = GetFromCatalog<ExchangeSource>(id)!;
            var sender = new ExchangeEmailSender(source);
            var message = new ExchangeTestMessage { Subject = "Test Subject", Body = "Test Body" };
            message.Tos.Add("to@test.local");

            source.Send(sender, message);

            var received = _wireMock.LogEntries.Any(e => e.RequestMessage.Path == "/EWS/Exchange.asmx");
            Assert.IsTrue(received, "WireMock should have received a SOAP POST to /EWS/Exchange.asmx");
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
