/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Core;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace Warewolf.Execution.Lightweight.Integration.Tests
{
    /// <summary>
    /// End-to-end WireMock-driven tests for <see cref="WarewolfWebClient"/>.
    ///
    /// The wrapper around <see cref="WebClient"/> has historically been hard to
    /// unit-test because its non-empty-address branches issue real HTTP calls.
    /// These tests stand up an in-process WireMock server so that the success
    /// path of <see cref="WarewolfWebClient.DownloadString(string)"/> and
    /// <see cref="WarewolfWebClient.DownloadStringAsync(string)"/> can be
    /// exercised without external network dependencies.
    /// </summary>
    [TestClass]
    public class WarewolfWebClientWireMockTests
    {
        private WireMockServer? _wireMock;
        private string Url(string path) => $"http://localhost:{_wireMock!.Port}{path}";

        [TestInitialize]
        public void Setup() => _wireMock = WireMockServer.Start();

        [TestCleanup]
        public void Cleanup() => _wireMock?.Stop();

        [TestMethod]
        [TestCategory("LiveIntegration_WebClient")]
        public void TC_WarewolfWebClient_DownloadString_NonEmptyAddress_ReturnsStubbedBody()
        {
            _wireMock!.Given(Request.Create().WithPath("/payload").UsingGet())
                .RespondWith(Response.Create().WithStatusCode(200)
                    .WithHeader("Content-Type", "text/plain")
                    .WithBody("hello-from-wiremock"));

            using var inner = new WebClient();
            using var client = new WarewolfWebClient(inner);

            var result = client.DownloadString(Url("/payload"));

            Assert.AreEqual("hello-from-wiremock", result);
        }

        [TestMethod]
        [TestCategory("LiveIntegration_WebClient")]
        public async Task TC_WarewolfWebClient_DownloadStringAsync_NonEmptyAddress_ReturnsStubbedBody()
        {
            _wireMock!.Given(Request.Create().WithPath("/async-payload").UsingGet())
                .RespondWith(Response.Create().WithStatusCode(200)
                    .WithHeader("Content-Type", "text/plain")
                    .WithBody("async-body"));

            using var inner = new WebClient();
            using var client = new WarewolfWebClient(inner);

            var result = await client.DownloadStringAsync(Url("/async-payload"));

            Assert.AreEqual("async-body", result);
        }

        [TestMethod]
        [TestCategory("LiveIntegration_WebClient")]
        public void TC_WarewolfWebClient_DownloadProgressChanged_FiresWhenInnerEventRaises()
        {
            using var inner = new WebClient();
            using var client = new WarewolfWebClient(inner);

            var fired = false;
            System.Net.DownloadProgressChangedEventHandler handler = (_, __) => fired = true;
            client.DownloadProgressChanged += handler;

            // Use reflection to raise the protected OnDownloadProgressChanged event on the
            // inner WebClient to prove that subscribing on the wrapper actually forwards
            // to the inner client's event source.
            var raise = typeof(WebClient).GetMethod(
                "OnDownloadProgressChanged",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Assert.IsNotNull(raise, "OnDownloadProgressChanged must exist on WebClient");
            var args = (System.Net.DownloadProgressChangedEventArgs)
                System.Runtime.Serialization.FormatterServices
                    .GetUninitializedObject(typeof(System.Net.DownloadProgressChangedEventArgs));
            raise!.Invoke(inner, new object[] { args });

            Assert.IsTrue(fired, "Subscribing on the wrapper must forward to the inner WebClient event");
            client.DownloadProgressChanged -= handler;
        }
    }
}
