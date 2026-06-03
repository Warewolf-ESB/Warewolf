using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Warewolf.Execution.Lightweight.Integration.Tests.InProcess;

namespace Warewolf.Execution.Lightweight.Integration.Tests
{
    /// <summary>
    /// In-process functional tests for the named record-set Web POST workflows:
    ///   - multipart-formdata (IsFormDataChecked, record set "rs")
    ///   - www-form-urlencoded (IsUrlEncodedChecked, record set "data")
    ///
    /// No longer requires a running engine on port 7071: <see cref="LightweightInProcessHost"/>
    /// executes the real workflow in-process, and the workflow's outbound POST hits the
    /// in-process <see cref="HttpbinEmulator"/> (port 4000) instead of the live httpbin.org.
    /// The .bite files target tools/http post/* and resolve the repointed httpbin WebSource.
    ///
    /// Marked <see cref="DoNotParallelizeAttribute"/> — the host mutates process-wide
    /// SecureConfigLoader singleton + environment-variable state per test.
    /// </summary>
    [TestClass]
    [DoNotParallelize]
    public class WebPostNamedWorkflowIntegrationTests
    {
        private const string RoutePrefix = "tools/http post";

        private LightweightInProcessHost _host = null!;

        [TestInitialize]
        public void Init() => _host = LightweightInProcessHost.WithPublicExecuteAll();

        [TestCleanup]
        public void Cleanup() => _host?.Dispose();

        // ---------------------------------------------------------------
        // multipart-formdata workflow (record set "rs")
        // ---------------------------------------------------------------

        /// <summary>multipart-formdata: POST multipart/form-data → rs[0].url == httpbin /post.</summary>
        [TestMethod, TestCategory("WebPostTool_Integration")]
        public async Task MultipartFormdata_MapsUrl_Returns_HttpbinPost()
        {
            var (status, json) = await _host.ExecutePublicAsync($"{RoutePrefix}/multipart-formdata.json");
            Assert.AreEqual(HttpStatusCode.OK, status, $"Expected HTTP 200 but got {(int)status}: {json}");
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
            var (status, json) = await _host.ExecutePublicAsync($"{RoutePrefix}/multipart-formdata.json");
            Assert.AreEqual(HttpStatusCode.OK, status, $"Expected HTTP 200 but got {(int)status}: {json}");
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            Assert.IsTrue(root.TryGetProperty("rs", out var rs), $"Expected record set 'rs' in response: {json}");
            Assert.IsTrue(rs.ValueKind == JsonValueKind.Array && rs.GetArrayLength() > 0, $"Expected 'rs' to be a non-empty array: {json}");
            Assert.IsTrue(rs[0].TryGetProperty("headersContent-Type", out var ct), $"Expected 'headersContent-Type' field in rs[0]: {json}");
            var ctValue = ct.GetString() ?? string.Empty;
            Assert.IsTrue(ctValue.StartsWith("multipart/form-data"), $"Expected headersContent-Type to start with 'multipart/form-data' but was '{ctValue}'. Full response: {json}");
        }

        /// <summary>multipart-formdata: POST multipart/form-data → rs[0].headersHost == httpbin.org.</summary>
        [TestMethod, TestCategory("WebPostTool_Integration")]
        public async Task MultipartFormdata_MapsHost_Returns_HttpbinOrg()
        {
            var (status, json) = await _host.ExecutePublicAsync($"{RoutePrefix}/multipart-formdata.json");
            Assert.AreEqual(HttpStatusCode.OK, status, $"Expected HTTP 200 but got {(int)status}: {json}");
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            Assert.IsTrue(root.TryGetProperty("rs", out var rs), $"Expected record set 'rs' in response: {json}");
            Assert.IsTrue(rs.ValueKind == JsonValueKind.Array && rs.GetArrayLength() > 0, $"Expected 'rs' to be a non-empty array: {json}");
            Assert.IsTrue(rs[0].TryGetProperty("headersHost", out var host), $"Expected 'headersHost' field in rs[0]: {json}");
            Assert.AreEqual(TestConstants.HttpbinHost, host.GetString(), $"Expected headersHost == '{TestConstants.HttpbinHost}'. Full response: {json}");
        }

        // ---------------------------------------------------------------
        // www-form-urlencoded workflow (record set "data")
        // ---------------------------------------------------------------

        /// <summary>www-form-urlencoded: POST application/x-www-form-urlencoded → data[0].url == httpbin /post.</summary>
        [TestMethod, TestCategory("WebPostTool_Integration")]
        public async Task WwwFormUrlencoded_MapsUrl_Returns_HttpbinPost()
        {
            var (status, json) = await _host.ExecutePublicAsync($"{RoutePrefix}/www-form-urlencoded.json");
            Assert.AreEqual(HttpStatusCode.OK, status, $"Expected HTTP 200 but got {(int)status}: {json}");
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
            var (status, json) = await _host.ExecutePublicAsync($"{RoutePrefix}/www-form-urlencoded.json");
            Assert.AreEqual(HttpStatusCode.OK, status, $"Expected HTTP 200 but got {(int)status}: {json}");
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            Assert.IsTrue(root.TryGetProperty("data", out var data), $"Expected record set 'data' in response: {json}");
            Assert.IsTrue(data.ValueKind == JsonValueKind.Array && data.GetArrayLength() > 0, $"Expected 'data' to be a non-empty array: {json}");
            Assert.IsTrue(data[0].TryGetProperty("headersContent-Type", out var ct), $"Expected 'headersContent-Type' field in data[0]: {json}");
            Assert.AreEqual("application/x-www-form-urlencoded", ct.GetString(), $"Expected headersContent-Type == 'application/x-www-form-urlencoded'. Full response: {json}");
        }

        /// <summary>www-form-urlencoded: POST application/x-www-form-urlencoded → data[0].headersHost == httpbin.org.</summary>
        [TestMethod, TestCategory("WebPostTool_Integration")]
        public async Task WwwFormUrlencoded_MapsHost_Returns_HttpbinOrg()
        {
            var (status, json) = await _host.ExecutePublicAsync($"{RoutePrefix}/www-form-urlencoded.json");
            Assert.AreEqual(HttpStatusCode.OK, status, $"Expected HTTP 200 but got {(int)status}: {json}");
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            Assert.IsTrue(root.TryGetProperty("data", out var data), $"Expected record set 'data' in response: {json}");
            Assert.IsTrue(data.ValueKind == JsonValueKind.Array && data.GetArrayLength() > 0, $"Expected 'data' to be a non-empty array: {json}");
            Assert.IsTrue(data[0].TryGetProperty("headersHost", out var host), $"Expected 'headersHost' field in data[0]: {json}");
            Assert.AreEqual(TestConstants.HttpbinHost, host.GetString(), $"Expected headersHost == '{TestConstants.HttpbinHost}'. Full response: {json}");
        }
    }
}
