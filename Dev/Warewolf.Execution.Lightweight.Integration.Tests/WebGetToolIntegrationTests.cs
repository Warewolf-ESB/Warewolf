using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Warewolf.Execution.Lightweight.Integration.Tests
{
    /// <summary>
    /// Integration tests for the HTTP GET Web Method tool executed via the Azure Function.
    /// Requires the Azure Function to be running at <see cref="BaseUrl"/> before running these tests.
    /// Each test triggers a named workflow (.bite file) and validates the JSON response.
    /// </summary>
    [TestClass]
    public class WebGetToolIntegrationTests
    {
        private const string BaseUrl = TestConstants.HttpbinGetBaseUrl;
        private static readonly HttpClient _client = new();

        /// <summary>TC-001: GET /get?tag=hdr with X-Custom-Header → Maps headers.X-Custom-Header → [[headersX-Custom-Header]]. Expected: headersX-Custom-Header == MyValue.</summary>
        [TestMethod, TestCategory("WebGetTool_Integration")]
        public async Task TC001_Get_CustomHeader_Echoed()
        {
            var response = await _client.GetAsync($"{BaseUrl}/TC013_Get_CustomHeader_Echoed.json");
            var json = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(response.IsSuccessStatusCode, $"Expected HTTP 200 but got {(int)response.StatusCode}: {json}");
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            Assert.IsTrue(root.TryGetProperty("headersX-Custom-Header", out var prop), $"Expected 'headersX-Custom-Header' in response: {json}");
            Assert.AreEqual("MyValue", prop.GetString(), $"Expected headersX-Custom-Header == 'MyValue'. Full response: {json}");
        }

        /// <summary>TC-002: GET /get → Maps headers.Accept and headers.Content-Type. Expected: headersAccept == application/json, headersContent-Type == application/json.</summary>
        [TestMethod, TestCategory("WebGetTool_Integration")]
        public async Task TC002_Get_MultipleHeaders_Mapped()
        {
            var response = await _client.GetAsync($"{BaseUrl}/TC014_Get_MultipleHeaders_Mapped.json");
            var json = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(response.IsSuccessStatusCode, $"Expected HTTP 200 but got {(int)response.StatusCode}: {json}");
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            Assert.IsTrue(root.TryGetProperty("headersAccept", out var accept), $"Expected 'headersAccept' in response: {json}");
            Assert.AreEqual("application/json", accept.GetString(), $"Expected headersAccept == 'application/json'. Full response: {json}");
            Assert.IsTrue(root.TryGetProperty("headersContent-Type", out var contentType), $"Expected 'headersContent-Type' in response: {json}");
            Assert.AreEqual("application/json", contentType.GetString(), $"Expected headersContent-Type == 'application/json'. Full response: {json}");
        }

        /// <summary>TC-003: GET /get → Maps origin → [[origin]]. Expected: origin is a non-empty IP address string.</summary>
        [TestMethod, TestCategory("WebGetTool_Integration")]
        public async Task TC003_Get_MapsOrigin_ReturnsIP()
        {
            var response = await _client.GetAsync($"{BaseUrl}/TC015_Get_MapsOrigin_ReturnsIP.json");
            var json = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(response.IsSuccessStatusCode, $"Expected HTTP 200 but got {(int)response.StatusCode}: {json}");
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            Assert.IsTrue(root.TryGetProperty("origin", out var prop), $"Expected 'origin' in response: {json}");
            Assert.IsFalse(string.IsNullOrEmpty(prop.GetString()), $"Expected non-empty origin. Full response: {json}");
        }

        /// <summary>TC-004: GET /get?a=1&amp;b=2&amp;c=3 → Maps args.a, args.b, args.c → [[argsa]], [[argsb]], [[argsc]]. Expected: argsa == 1, argsb == 2, argsc == 3.</summary>
        [TestMethod, TestCategory("WebGetTool_Integration")]
        public async Task TC004_Get_ThreeQueryParams_Mapped()
        {
            var response = await _client.GetAsync($"{BaseUrl}/TC016_Get_ThreeQueryParams_Mapped.json");
            var json = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(response.IsSuccessStatusCode, $"Expected HTTP 200 but got {(int)response.StatusCode}: {json}");
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            Assert.IsTrue(root.TryGetProperty("argsa", out var a), $"Expected 'argsa' in response: {json}");
            Assert.AreEqual(1, a.GetInt32(), $"Expected argsa == 1. Full response: {json}");
            Assert.IsTrue(root.TryGetProperty("argsb", out var b), $"Expected 'argsb' in response: {json}");
            Assert.AreEqual(2, b.GetInt32(), $"Expected argsb == 2. Full response: {json}");
            Assert.IsTrue(root.TryGetProperty("argsc", out var c), $"Expected 'argsc' in response: {json}");
            Assert.AreEqual(3, c.GetInt32(), $"Expected argsc == 3. Full response: {json}");
        }

        /// <summary>TC-005: GET /get?name=John%20Doe → Maps args.name → [[argsname]]. Expected: argsname == John Doe.</summary>
        [TestMethod, TestCategory("WebGetTool_Integration")]
        public async Task TC005_Get_UrlEncodedValue_MapsDecoded()
        {
            var response = await _client.GetAsync($"{BaseUrl}/TC017_Get_UrlEncodedValue_MapsDecoded.json");
            var json = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(response.IsSuccessStatusCode, $"Expected HTTP 200 but got {(int)response.StatusCode}: {json}");
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            Assert.IsTrue(root.TryGetProperty("argsname", out var prop), $"Expected 'argsname' in response: {json}");
            Assert.AreEqual("John Doe", prop.GetString(), $"Expected argsname == 'John Doe'. Full response: {json}");
        }

        /// <summary>TC-006: GET /anything?tag=test → Maps method and url → [[method]], [[url]]. Expected: method == GET, url == https://httpbin.org/anything?tag=test.</summary>
        [TestMethod, TestCategory("WebGetTool_Integration")]
        public async Task TC006_Get_Anything_MapsMethodAndUrl()
        {
            var response = await _client.GetAsync($"{BaseUrl}/TC018_Get_Anything_MapsMethodAndUrl.json");
            var json = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(response.IsSuccessStatusCode, $"Expected HTTP 200 but got {(int)response.StatusCode}: {json}");
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            Assert.IsTrue(root.TryGetProperty("method", out var method), $"Expected 'method' in response: {json}");
            Assert.AreEqual("GET", method.GetString(), $"Expected method == 'GET'. Full response: {json}");
            Assert.IsTrue(root.TryGetProperty("url", out var url), $"Expected 'url' in response: {json}");
            Assert.AreEqual(TestConstants.HttpbinAnythingUrl + "?tag=test", url.GetString(), $"Expected url == '{TestConstants.HttpbinAnythingUrl}?tag=test'. Full response: {json}");
        }

        /// <summary>TC-007: GET /get?score=99 → Maps args.score → [[argsscore]]. Expected: argsscore == 99.</summary>
        [TestMethod, TestCategory("WebGetTool_Integration")]
        public async Task TC007_Get_ScalarOutput_MapsScore()
        {
            var response = await _client.GetAsync($"{BaseUrl}/TC019_Get_ScalarOutput_MapsScore.json");
            var json = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(response.IsSuccessStatusCode, $"Expected HTTP 200 but got {(int)response.StatusCode}: {json}");
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            Assert.IsTrue(root.TryGetProperty("argsscore", out var prop), $"Expected 'argsscore' in response: {json}");
            Assert.AreEqual(99, prop.GetInt32(), $"Expected argsscore == 99. Full response: {json}");
        }

        /// <summary>TC-008: GET /get → Maps headers.User-Agent → [[headersUser-Agent]]. Expected: headersUser-Agent is non-empty.</summary>
        [TestMethod, TestCategory("WebGetTool_Integration")]
        public async Task TC008_Get_MapsUserAgent_NonEmpty()
        {
            var response = await _client.GetAsync($"{BaseUrl}/TC020_Get_MapsUserAgent_NonEmpty.json");
            var json = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(response.IsSuccessStatusCode, $"Expected HTTP 200 but got {(int)response.StatusCode}: {json}");
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            Assert.IsTrue(root.TryGetProperty("headersUser-Agent", out var prop), $"Expected 'headersUser-Agent' in response: {json}");
            Assert.IsFalse(string.IsNullOrEmpty(prop.GetString()), $"Expected non-empty headersUser-Agent. Full response: {json}");
        }

        /// <summary>TC-009: GET /get?id=1&amp;name=Sachin → Maps full response as nested object under data. Expected: data.args.id == "1", data.args.name == "Sachin", data.headers.Host == httpbin.org, data.url == https://httpbin.org/get?id=1&amp;name=Sachin.</summary>
        [TestMethod, TestCategory("WebGetTool_Integration")]
        public async Task TC009_Response_As_Object()
        {
            var response = await _client.GetAsync($"{BaseUrl}/TC020_Response_As_Object.json");
            var json = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(response.IsSuccessStatusCode, $"Expected HTTP 200 but got {(int)response.StatusCode}: {json}");
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            Assert.IsTrue(root.TryGetProperty("data", out var data), $"Expected 'data' in response: {json}");
            Assert.IsTrue(data.TryGetProperty("args", out var args), $"Expected 'data.args' in response: {json}");
            Assert.IsTrue(args.TryGetProperty("id", out var id), $"Expected 'data.args.id' in response: {json}");
            Assert.AreEqual("1", id.GetString(), $"Expected data.args.id == '1'. Full response: {json}");
            Assert.IsTrue(args.TryGetProperty("name", out var name), $"Expected 'data.args.name' in response: {json}");
            Assert.AreEqual("Sachin", name.GetString(), $"Expected data.args.name == 'Sachin'. Full response: {json}");
            Assert.IsTrue(data.TryGetProperty("headers", out var headers), $"Expected 'data.headers' in response: {json}");
            Assert.IsTrue(headers.TryGetProperty("Host", out var host), $"Expected 'data.headers.Host' in response: {json}");
            Assert.AreEqual(TestConstants.HttpbinHost, host.GetString(), $"Expected data.headers.Host == '{TestConstants.HttpbinHost}'. Full response: {json}");
            Assert.IsTrue(data.TryGetProperty("url", out var url), $"Expected 'data.url' in response: {json}");
            Assert.AreEqual(TestConstants.HttpbinGetUrl + "?id=1&name=Sachin", url.GetString(), $"Expected data.url == '{TestConstants.HttpbinGetUrl}?id=1&name=Sachin'. Full response: {json}");
        }

        /// <summary>TC-010: GET /get?verify=url → Maps url → [[url]]. Expected: url == https://httpbin.org/get?verify=url.</summary>
        [TestMethod, TestCategory("WebGetTool_Integration")]
        public async Task TC010_Get_MapsEchoedUrl_ContainsQuery()
        {
            var response = await _client.GetAsync($"{BaseUrl}/TC022_Get_MapsEchoedUrl_ContainsQuery.json");
            var json = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(response.IsSuccessStatusCode, $"Expected HTTP 200 but got {(int)response.StatusCode}: {json}");
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            Assert.IsTrue(root.TryGetProperty("url", out var prop), $"Expected 'url' in response: {json}");
            Assert.AreEqual(TestConstants.HttpbinGetUrl + "?verify=url", prop.GetString(), $"Expected url == '{TestConstants.HttpbinGetUrl}?verify=url'. Full response: {json}");
        }
    }
}
