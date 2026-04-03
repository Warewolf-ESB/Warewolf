using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Warewolf.Execution.Lightweight.Tests
{
    /// <summary>
    /// Integration tests for the named workflow bite files:
    ///   - multipart-formdata (IsFormDataChecked, record set "rs")
    ///   - www-form-urlencoded (IsUrlEncodedChecked, record set "data")
    /// Requires the Azure Function to be running at <see cref="BaseUrl"/> before running these tests.
    /// The bite files are in Resources/tools/http post/ and target localhost:4000/post.
    /// </summary>
    [TestClass]
    public class WebPostNamedWorkflowIntegrationTests
    {
        private const string BaseUrl = TestConstants.AzureFunctionBaseUrl;
        private static readonly HttpClient _client = new();

        // ---------------------------------------------------------------
        // multipart-formdata workflow (record set "rs")
        // Conditions: numbers=10,20,30 (text) + data=test.txt file attachment
        // Settings: IsFormDataChecked=True
        // ---------------------------------------------------------------

        /// <summary>multipart-formdata: POST multipart/form-data → rs[0].url == http://localhost:4000/post.</summary>
        [TestMethod, TestCategory("WebPostTool_Integration")]
        public async Task MultipartFormdata_MapsUrl_Returns_HttpbinPost()
        {
            var response = await _client.GetAsync($"{BaseUrl}/multipart-formdata.json");
            var json = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(response.IsSuccessStatusCode, $"Expected HTTP 200 but got {(int)response.StatusCode}: {json}");
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            Assert.IsTrue(root.TryGetProperty("rs", out var rs), $"Expected record set 'rs' in response: {json}");
            Assert.IsTrue(rs.ValueKind == JsonValueKind.Array && rs.GetArrayLength() > 0, $"Expected 'rs' to be a non-empty array: {json}");
            Assert.IsTrue(rs[0].TryGetProperty("url", out var url), $"Expected 'url' field in rs[0]: {json}");
            Assert.AreEqual(TestConstants.HttpbinPostUrl, url.GetString(), $"Expected url == '{TestConstants.HttpbinPostUrl}'. Full response: {json}");
        }

        /// <summary>multipart-formdata: POST multipart/form-data → rs[0].headersContent-Type starts with multipart/form-data.</summary>
        [TestMethod, TestCategory("WebPostTool_Integration")]
        public async Task MultipartFormdata_MapsContentType_Contains_MultipartFormdata()
        {
            var response = await _client.GetAsync($"{BaseUrl}/multipart-formdata.json");
            var json = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(response.IsSuccessStatusCode, $"Expected HTTP 200 but got {(int)response.StatusCode}: {json}");
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            Assert.IsTrue(root.TryGetProperty("rs", out var rs), $"Expected record set 'rs' in response: {json}");
            Assert.IsTrue(rs.ValueKind == JsonValueKind.Array && rs.GetArrayLength() > 0, $"Expected 'rs' to be a non-empty array: {json}");
            Assert.IsTrue(rs[0].TryGetProperty("headersContent-Type", out var ct), $"Expected 'headersContent-Type' field in rs[0]: {json}");
            var ctValue = ct.GetString() ?? string.Empty;
            Assert.IsTrue(ctValue.StartsWith("multipart/form-data"), $"Expected headersContent-Type to start with 'multipart/form-data' but was '{ctValue}'. Full response: {json}");
        }

        /// <summary>multipart-formdata: POST multipart/form-data → rs[0].headersHost == localhost:4000.</summary>
        [TestMethod, TestCategory("WebPostTool_Integration")]
        public async Task MultipartFormdata_MapsHost_Returns_HttpbinOrg()
        {
            var response = await _client.GetAsync($"{BaseUrl}/multipart-formdata.json");
            var json = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(response.IsSuccessStatusCode, $"Expected HTTP 200 but got {(int)response.StatusCode}: {json}");
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            Assert.IsTrue(root.TryGetProperty("rs", out var rs), $"Expected record set 'rs' in response: {json}");
            Assert.IsTrue(rs.ValueKind == JsonValueKind.Array && rs.GetArrayLength() > 0, $"Expected 'rs' to be a non-empty array: {json}");
            Assert.IsTrue(rs[0].TryGetProperty("headersHost", out var host), $"Expected 'headersHost' field in rs[0]: {json}");
            Assert.AreEqual(TestConstants.HttpbinHost, host.GetString(), $"Expected headersHost == '{TestConstants.HttpbinHost}'. Full response: {json}");
        }

        // ---------------------------------------------------------------
        // www-form-urlencoded workflow (record set "data")
        // Conditions: id=100 (text), name=sachin (text)
        // Settings: IsUrlEncodedChecked=True
        // ---------------------------------------------------------------

        /// <summary>www-form-urlencoded: POST application/x-www-form-urlencoded → data[0].url == http://localhost:4000/post.</summary>
        [TestMethod, TestCategory("WebPostTool_Integration")]
        public async Task WwwFormUrlencoded_MapsUrl_Returns_HttpbinPost()
        {
            var response = await _client.GetAsync($"{BaseUrl}/www-form-urlencoded.json");
            var json = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(response.IsSuccessStatusCode, $"Expected HTTP 200 but got {(int)response.StatusCode}: {json}");
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            Assert.IsTrue(root.TryGetProperty("data", out var data), $"Expected record set 'data' in response: {json}");
            Assert.IsTrue(data.ValueKind == JsonValueKind.Array && data.GetArrayLength() > 0, $"Expected 'data' to be a non-empty array: {json}");
            Assert.IsTrue(data[0].TryGetProperty("url", out var url), $"Expected 'url' field in data[0]: {json}");
            Assert.AreEqual(TestConstants.HttpbinPostUrl, url.GetString(), $"Expected url == '{TestConstants.HttpbinPostUrl}'. Full response: {json}");
        }

        /// <summary>www-form-urlencoded: POST application/x-www-form-urlencoded → data[0].headersContent-Type == application/x-www-form-urlencoded.</summary>
        [TestMethod, TestCategory("WebPostTool_Integration")]
        public async Task WwwFormUrlencoded_MapsContentType_Returns_UrlEncoded()
        {
            var response = await _client.GetAsync($"{BaseUrl}/www-form-urlencoded.json");
            var json = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(response.IsSuccessStatusCode, $"Expected HTTP 200 but got {(int)response.StatusCode}: {json}");
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            Assert.IsTrue(root.TryGetProperty("data", out var data), $"Expected record set 'data' in response: {json}");
            Assert.IsTrue(data.ValueKind == JsonValueKind.Array && data.GetArrayLength() > 0, $"Expected 'data' to be a non-empty array: {json}");
            Assert.IsTrue(data[0].TryGetProperty("headersContent-Type", out var ct), $"Expected 'headersContent-Type' field in data[0]: {json}");
            Assert.AreEqual("application/x-www-form-urlencoded", ct.GetString(), $"Expected headersContent-Type == 'application/x-www-form-urlencoded'. Full response: {json}");
        }

        /// <summary>www-form-urlencoded: POST application/x-www-form-urlencoded → data[0].headersHost == localhost:4000.</summary>
        [TestMethod, TestCategory("WebPostTool_Integration")]
        public async Task WwwFormUrlencoded_MapsHost_Returns_HttpbinOrg()
        {
            var response = await _client.GetAsync($"{BaseUrl}/www-form-urlencoded.json");
            var json = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(response.IsSuccessStatusCode, $"Expected HTTP 200 but got {(int)response.StatusCode}: {json}");
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            Assert.IsTrue(root.TryGetProperty("data", out var data), $"Expected record set 'data' in response: {json}");
            Assert.IsTrue(data.ValueKind == JsonValueKind.Array && data.GetArrayLength() > 0, $"Expected 'data' to be a non-empty array: {json}");
            Assert.IsTrue(data[0].TryGetProperty("headersHost", out var host), $"Expected 'headersHost' field in data[0]: {json}");
            Assert.AreEqual(TestConstants.HttpbinHost, host.GetString(), $"Expected headersHost == '{TestConstants.HttpbinHost}'. Full response: {json}");
        }
    }
}
