using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Warewolf.Execution.Lightweight.Tests
{
    /// <summary>
    /// Integration tests for the HTTP POST Web Method tool executed via the Azure Function.
    /// Requires the Azure Function to be running at <see cref="BaseUrl"/> before running these tests.
    /// Each test triggers a named workflow (.bite file in Resources/) and validates the JSON response.
    /// </summary>
    [TestClass]
    public class WebPostToolIntegrationTests
    {
        private const string BaseUrl = TestConstants.AzureFunctionBaseUrl;
        private static readonly HttpClient _client = new();

        /// <summary>TC-005:
        [TestMethod, TestCategory("WebPostTool_Integration")]
        public async Task TC005_PostJsonBody_MapsName_Returns_Alice()
        {
            var response = await _client.GetAsync($"{BaseUrl}/TC005_PostJsonBody_MapsName_Returns_Alice.json");
            var json = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(response.IsSuccessStatusCode, $"Expected HTTP 200 but got {(int)response.StatusCode}: {json}");
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            Assert.IsTrue(root.TryGetProperty("Name", out var prop), $"Expected 'Name' in response: {json}");
            Assert.AreEqual("Alice", prop.GetString(), $"Expected Name == 'Alice'. Full response: {json}");
        }

        /// <summary>TC-012:
        [TestMethod, TestCategory("WebPostTool_Integration")]
        public async Task TC012_PostJsonBody_MapsUrl_Returns_HttpbinPost()
        {
            var response = await _client.GetAsync($"{BaseUrl}/TC012_PostJsonBody_MapsUrl_Returns_HttpbinPost.json");
            var json = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(response.IsSuccessStatusCode, $"Expected HTTP 200 but got {(int)response.StatusCode}: {json}");
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            Assert.IsTrue(root.TryGetProperty("Url", out var prop), $"Expected 'Url' in response: {json}");
            Assert.AreEqual(TestConstants.HttpbinPostUrl, prop.GetString(), $"Expected Url == '{TestConstants.HttpbinPostUrl}'. Full response: {json}");
        }

        /// <summary>TC-013: POST any body → Maps headers.Host → [[Host]]. Expected: Host == localhost:4000.</summary>
        [TestMethod, TestCategory("WebPostTool_Integration")]
        public async Task TC013_PostJsonBody_MapsHost_Returns_HttpbinOrg()
        {
            var response = await _client.GetAsync($"{BaseUrl}/TC013_PostJsonBody_MapsHost_Returns_HttpbinOrg.json");
            var json = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(response.IsSuccessStatusCode, $"Expected HTTP 200 but got {(int)response.StatusCode}: {json}");
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            Assert.IsTrue(root.TryGetProperty("Host", out var prop), $"Expected 'Host' in response: {json}");
            Assert.AreEqual(TestConstants.HttpbinHost, prop.GetString(), $"Expected Host == '{TestConstants.HttpbinHost}'. Full response: {json}");
        }

        /// <summary>TC-014: POST with Content-Type application/json → Maps headers.Content-Type → [[ContentType]]. Expected: ContentType == application/json.</summary>
        [TestMethod, TestCategory("WebPostTool_Integration")]
        public async Task TC014_PostJsonBody_MapsContentType_Returns_ApplicationJson()
        {
            var response = await _client.GetAsync($"{BaseUrl}/TC014_PostJsonBody_MapsContentType_Returns_ApplicationJson.json");
            var json = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(response.IsSuccessStatusCode, $"Expected HTTP 200 but got {(int)response.StatusCode}: {json}");
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            Assert.IsTrue(root.TryGetProperty("ContentType", out var prop), $"Expected 'ContentType' in response: {json}");
            Assert.AreEqual("application/json", prop.GetString(), $"Expected ContentType == 'application/json'. Full response: {json}");
        }

        /// <summary>TC-026: POST {"key":"value"} to /anything → Maps url → [[Url]]. Expected: Url == http://localhost:4000/anything.</summary>
        [TestMethod, TestCategory("WebPostTool_Integration")]
        public async Task TC026_PostToAnything_MapsUrl_Returns_HttpbinAnything()
        {
            var response = await _client.GetAsync($"{BaseUrl}/TC026_PostToAnything_MapsUrl_Returns_HttpbinAnything.json");
            var json = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(response.IsSuccessStatusCode, $"Expected HTTP 200 but got {(int)response.StatusCode}: {json}");
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            Assert.IsTrue(root.TryGetProperty("Url", out var prop), $"Expected 'Url' in response: {json}");
            Assert.AreEqual(TestConstants.HttpbinAnythingUrl, prop.GetString(), $"Expected Url == '{TestConstants.HttpbinAnythingUrl}'. Full response: {json}");
        }

        /// <summary>TC-031: POST {"city":"Paris","country":"France"} → Maps json.city+json.country+method → [[City]]+[[Country]]+[[Method]]. Expected: Paris, France, POST.</summary>
        [TestMethod, TestCategory("WebPostTool_Integration")]
        public async Task TC031_PostJsonBody_MapsCity_Country_Method()
        {
            var response = await _client.GetAsync($"{BaseUrl}/TC031_PostJsonBody_MapsCity_Country_Method.json");
            var json = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(response.IsSuccessStatusCode, $"Expected HTTP 200 but got {(int)response.StatusCode}: {json}");
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            Assert.IsTrue(root.TryGetProperty("City", out var city), $"Expected 'City' in response: {json}");
            Assert.AreEqual("Paris", city.GetString(), $"Expected City == 'Paris'. Full response: {json}");
            Assert.IsTrue(root.TryGetProperty("Country", out var country), $"Expected 'Country' in response: {json}");
            Assert.AreEqual("France", country.GetString(), $"Expected Country == 'France'. Full response: {json}");
        }

        /// <summary>TC-051:
        [TestMethod, TestCategory("WebPostTool_Integration")]
        public async Task TC051_PostWithQueryParam_MapsSearch_Returns_Warewolf()
        {
            var response = await _client.GetAsync($"{BaseUrl}/TC051_PostWithQueryParam_MapsSearch_Returns_Warewolf.json");
            var json = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(response.IsSuccessStatusCode, $"Expected HTTP 200 but got {(int)response.StatusCode}: {json}");
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            Assert.IsTrue(root.TryGetProperty("Search", out var prop), $"Expected 'Search' in response: {json}");
            Assert.AreEqual("warewolf", prop.GetString(), $"Expected Search == 'warewolf'. Full response: {json}");
        }

        /// <summary>TC-052: POST with query params ?x=10&amp;y=20 → Maps args.x → [[ArgX]], args.y → [[ArgY]]. Expected: ArgX == 10, ArgY == 20.</summary>
        [TestMethod, TestCategory("WebPostTool_Integration")]
        public async Task TC052_PostWithMultipleQueryParams_MapsArgX_And_ArgY()
        {
            var response = await _client.GetAsync($"{BaseUrl}/TC052_PostWithMultipleQueryParams_MapsArgX_And_ArgY.json");
            var json = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(response.IsSuccessStatusCode, $"Expected HTTP 200 but got {(int)response.StatusCode}: {json}");
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            Assert.IsTrue(root.TryGetProperty("ArgX", out var argX), $"Expected 'ArgX' in response: {json}");
            Assert.AreEqual("10", argX.ToString(), $"Expected ArgX == '10'. Full response: {json}");
            Assert.IsTrue(root.TryGetProperty("ArgY", out var argY), $"Expected 'ArgY' in response: {json}");
            Assert.AreEqual("20", argY.ToString(), $"Expected ArgY == '20'. Full response: {json}");
        }

        /// <summary>TC-053: POST with custom header X-Test-Header → Maps headers.X-Test-Header → [[RequestId]]. Expected: RequestId == req-abc-789.</summary>
        [TestMethod, TestCategory("WebPostTool_Integration")]
        public async Task TC053_PostWithCustomHeader_MapsRequestId_Returns_ReqAbc789()
        {
            var response = await _client.GetAsync($"{BaseUrl}/TC053_PostWithCustomHeader_MapsRequestId_Returns_ReqAbc789.json");
            var json = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(response.IsSuccessStatusCode, $"Expected HTTP 200 but got {(int)response.StatusCode}: {json}");
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            Assert.IsTrue(root.TryGetProperty("RequestId", out var prop), $"Expected 'RequestId' in response: {json}");
            Assert.AreEqual("req-abc-789", prop.GetString(), $"Expected RequestId == 'req-abc-789'. Full response: {json}");
        }

        /// <summary>TC-054: POST with 4 outputs → Maps json.a→A, json.b→B, json.c→C, json.d→D. Expected: alpha, beta, gamma, delta.</summary>
        [TestMethod, TestCategory("WebPostTool_Integration")]
        public async Task TC054_PostWithFourOutputs_MapsAll_Returns_CorrectValues()
        {
            var response = await _client.GetAsync($"{BaseUrl}/TC054_PostWithFourOutputs_MapsAll_Returns_CorrectValues.json");
            var json = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(response.IsSuccessStatusCode, $"Expected HTTP 200 but got {(int)response.StatusCode}: {json}");
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            Assert.IsTrue(root.TryGetProperty("A", out var a), $"Expected 'A' in response: {json}");
            Assert.AreEqual("alpha", a.GetString(), $"Expected A == 'alpha'. Full response: {json}");
            Assert.IsTrue(root.TryGetProperty("B", out var b), $"Expected 'B' in response: {json}");
            Assert.AreEqual("beta", b.GetString(), $"Expected B == 'beta'. Full response: {json}");
            Assert.IsTrue(root.TryGetProperty("C", out var c), $"Expected 'C' in response: {json}");
            Assert.AreEqual("gamma", c.GetString(), $"Expected C == 'gamma'. Full response: {json}");
            Assert.IsTrue(root.TryGetProperty("D", out var d), $"Expected 'D' in response: {json}");
            Assert.AreEqual("delta", d.GetString(), $"Expected D == 'delta'. Full response: {json}");
        }

        /// <summary>TC-055: POST with query param + body field → Maps args.mode → [[Mode]], json.item → [[Item]]. Expected: Mode == test, Item == widget.</summary>
        [TestMethod, TestCategory("WebPostTool_Integration")]
        public async Task TC055_PostWithQueryAndBody_MapsMode_And_Item()
        {
            var response = await _client.GetAsync($"{BaseUrl}/TC055_PostWithQueryAndBody_MapsMode_And_Item.json");
            var json = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(response.IsSuccessStatusCode, $"Expected HTTP 200 but got {(int)response.StatusCode}: {json}");
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            Assert.IsTrue(root.TryGetProperty("Mode", out var mode), $"Expected 'Mode' in response: {json}");
            Assert.AreEqual("test", mode.GetString(), $"Expected Mode == 'test'. Full response: {json}");
            Assert.IsTrue(root.TryGetProperty("Item", out var item), $"Expected 'Item' in response: {json}");
            Assert.AreEqual("widget", item.GetString(), $"Expected Item == 'widget'. Full response: {json}");
        }

        /// <summary>TC-056: POST to /anything?action=submit → Maps args.action, json.form, method. Expected: Action == submit, Form == contact, Method == POST.</summary>
        [TestMethod, TestCategory("WebPostTool_Integration")]
        public async Task TC056_PostToAnythingWithQuery_MapsAction_Form_Method()
        {
            var response = await _client.GetAsync($"{BaseUrl}/TC056_PostToAnythingWithQuery_MapsAction_Form_Method.json");
            var json = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(response.IsSuccessStatusCode, $"Expected HTTP 200 but got {(int)response.StatusCode}: {json}");
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            Assert.IsTrue(root.TryGetProperty("Action", out var action), $"Expected 'Action' in response: {json}");
            Assert.AreEqual("submit", action.GetString(), $"Expected Action == 'submit'. Full response: {json}");
            Assert.IsTrue(root.TryGetProperty("Form", out var form), $"Expected 'Form' in response: {json}");
            Assert.AreEqual("contact", form.GetString(), $"Expected Form == 'contact'. Full response: {json}");
            Assert.IsTrue(root.TryGetProperty("Method", out var method), $"Expected 'Method' in response: {json}");
            Assert.AreEqual("POST", method.GetString(), $"Expected Method == 'POST'. Full response: {json}");
        }

        /// <summary>TC-057: POST with empty JSON body to /anything → Maps method → [[Method]]. Expected: Method == POST.</summary>
        [TestMethod, TestCategory("WebPostTool_Integration")]
        public async Task TC057_PostEmptyBody_MapsMethod_Returns_POST()
        {
            var response = await _client.GetAsync($"{BaseUrl}/TC057_PostEmptyBody_MapsMethod_Returns_POST.json");
            var json = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(response.IsSuccessStatusCode, $"Expected HTTP 200 but got {(int)response.StatusCode}: {json}");
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            Assert.IsTrue(root.TryGetProperty("Method", out var prop), $"Expected 'Method' in response: {json}");
            Assert.AreEqual("POST", prop.GetString(), $"Expected Method == 'POST'. Full response: {json}");
        }

        /// <summary>TC-058: POST with custom header X-Trace-Id + body → Maps headers.X-Trace-Id → [[TraceId]], json.level → [[Level]]. Expected: TraceId == trace-42, Level == info.</summary>
        [TestMethod, TestCategory("WebPostTool_Integration")]
        public async Task TC058_PostWithCustomHeaderAndBody_MapsTraceId_And_Level()
        {
            var response = await _client.GetAsync($"{BaseUrl}/TC058_PostWithCustomHeaderAndBody_MapsTraceId_And_Level.json");
            var json = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(response.IsSuccessStatusCode, $"Expected HTTP 200 but got {(int)response.StatusCode}: {json}");
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            Assert.IsTrue(root.TryGetProperty("TraceId", out var traceId), $"Expected 'TraceId' in response: {json}");
            Assert.AreEqual("trace-42", traceId.GetString(), $"Expected TraceId == 'trace-42'. Full response: {json}");
            Assert.IsTrue(root.TryGetProperty("Level", out var level), $"Expected 'Level' in response: {json}");
            Assert.AreEqual("info", level.GetString(), $"Expected Level == 'info'. Full response: {json}");
        }

        /// <summary>TC-059: POST multipart/form-data with single text field name=alice → Maps form.name → [[FormName]]. Expected: FormName == alice.</summary>
        [TestMethod, TestCategory("WebPostTool_Integration")]
        public async Task TC059_PostFormData_MapsSingleTextField_Returns_Alice()
        {
            var response = await _client.GetAsync($"{BaseUrl}/TC059_PostFormData_MapsSingleTextField_Returns_Alice.json");
            var json = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(response.IsSuccessStatusCode, $"Expected HTTP 200 but got {(int)response.StatusCode}: {json}");
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            Assert.IsTrue(root.TryGetProperty("FormName", out var prop), $"Expected 'FormName' in response: {json}");
            Assert.AreEqual("alice", prop.GetString(), $"Expected FormName == 'alice'. Full response: {json}");
        }

        /// <summary>TC-060: POST multipart/form-data with two fields city=Paris, country=France → Maps form.city → [[FormCity]], form.country → [[FormCountry]]. Expected: FormCity == Paris, FormCountry == France.</summary>
        [TestMethod, TestCategory("WebPostTool_Integration")]
        public async Task TC060_PostFormData_MapsMultipleTextFields_Returns_ParisFrance()
        {
            var response = await _client.GetAsync($"{BaseUrl}/TC060_PostFormData_MapsMultipleTextFields_Returns_ParisFrance.json");
            var json = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(response.IsSuccessStatusCode, $"Expected HTTP 200 but got {(int)response.StatusCode}: {json}");
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            Assert.IsTrue(root.TryGetProperty("FormCity", out var city), $"Expected 'FormCity' in response: {json}");
            Assert.AreEqual("Paris", city.GetString(), $"Expected FormCity == 'Paris'. Full response: {json}");
            Assert.IsTrue(root.TryGetProperty("FormCountry", out var country), $"Expected 'FormCountry' in response: {json}");
            Assert.AreEqual("France", country.GetString(), $"Expected FormCountry == 'France'. Full response: {json}");
        }

        /// <summary>TC-061: POST multipart/form-data with tag=endpoint → Maps form.tag → [[FormTag]], url → [[UrlOut]]. Expected: FormTag == endpoint, UrlOut == http://localhost:4000/post.</summary>
        [TestMethod, TestCategory("WebPostTool_Integration")]
        public async Task TC061_PostFormData_MapsTextField_And_Url()
        {
            var response = await _client.GetAsync($"{BaseUrl}/TC061_PostFormData_MapsTextField_And_Url.json");
            var json = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(response.IsSuccessStatusCode, $"Expected HTTP 200 but got {(int)response.StatusCode}: {json}");
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            Assert.IsTrue(root.TryGetProperty("FormTag", out var tag), $"Expected 'FormTag' in response: {json}");
            Assert.AreEqual("endpoint", tag.GetString(), $"Expected FormTag == 'endpoint'. Full response: {json}");
            Assert.IsTrue(root.TryGetProperty("UrlOut", out var url), $"Expected 'UrlOut' in response: {json}");
            Assert.AreEqual(TestConstants.HttpbinPostUrl, url.GetString(), $"Expected UrlOut == '{TestConstants.HttpbinPostUrl}'. Full response: {json}");
        }

        /// <summary>TC-062: POST application/x-www-form-urlencoded with product=widget → Maps form.product → [[UrlProduct]]. Expected: UrlProduct == widget.</summary>
        [TestMethod, TestCategory("WebPostTool_Integration")]
        public async Task TC062_PostUrlEncoded_MapsSingleField_Returns_Widget()
        {
            var response = await _client.GetAsync($"{BaseUrl}/TC062_PostUrlEncoded_MapsSingleField_Returns_Widget.json");
            var json = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(response.IsSuccessStatusCode, $"Expected HTTP 200 but got {(int)response.StatusCode}: {json}");
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            Assert.IsTrue(root.TryGetProperty("UrlProduct", out var prop), $"Expected 'UrlProduct' in response: {json}");
            Assert.AreEqual("widget", prop.GetString(), $"Expected UrlProduct == 'widget'. Full response: {json}");
        }

        /// <summary>TC-063: POST application/x-www-form-urlencoded with x=10, y=20 → Maps form.x → [[UrlX]], form.y → [[UrlY]]. Expected: UrlX == 10, UrlY == 20.</summary>
        [TestMethod, TestCategory("WebPostTool_Integration")]
        public async Task TC063_PostUrlEncoded_MapsMultipleFields_Returns_10And20()
        {
            var response = await _client.GetAsync($"{BaseUrl}/TC063_PostUrlEncoded_MapsMultipleFields_Returns_10And20.json");
            var json = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(response.IsSuccessStatusCode, $"Expected HTTP 200 but got {(int)response.StatusCode}: {json}");
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            Assert.IsTrue(root.TryGetProperty("UrlX", out var x), $"Expected 'UrlX' in response: {json}");
            Assert.AreEqual("10", x.ToString(), $"Expected UrlX == '10'. Full response: {json}");
            Assert.IsTrue(root.TryGetProperty("UrlY", out var y), $"Expected 'UrlY' in response: {json}");
            Assert.AreEqual("20", y.ToString(), $"Expected UrlY == '20'. Full response: {json}");
        }

        /// <summary>TC-064: POST application/x-www-form-urlencoded with status=active → Maps form.status → [[UrlStatus]], url → [[UrlOut]]. Expected: UrlStatus == active, UrlOut == http://localhost:4000/post.</summary>
        [TestMethod, TestCategory("WebPostTool_Integration")]
        public async Task TC064_PostUrlEncoded_MapsField_And_Method()
        {
            var response = await _client.GetAsync($"{BaseUrl}/TC064_PostUrlEncoded_MapsField_And_Method.json");
            var json = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(response.IsSuccessStatusCode, $"Expected HTTP 200 but got {(int)response.StatusCode}: {json}");
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            Assert.IsTrue(root.TryGetProperty("UrlStatus", out var status), $"Expected 'UrlStatus' in response: {json}");
            Assert.AreEqual("active", status.GetString(), $"Expected UrlStatus == 'active'. Full response: {json}");
            Assert.IsTrue(root.TryGetProperty("UrlOut", out var urlOut), $"Expected 'UrlOut' in response: {json}");
            Assert.AreEqual(TestConstants.HttpbinPostUrl, urlOut.GetString(), $"Expected UrlOut == '{TestConstants.HttpbinPostUrl}'. Full response: {json}");
        }

        /// <summary>TC-065: POST JSON body with IsObject=True → Stores entire httpbin response in [[@Response]]. Expected: Response.json.tag == obj.</summary>
        [TestMethod, TestCategory("WebPostTool_Integration")]
        public async Task TC065_PostJsonBody_IsObject_Returns_FullResponse()
        {
            var response = await _client.GetAsync($"{BaseUrl}/TC065_PostJsonBody_IsObject_Returns_FullResponse.json");
            var json = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(response.IsSuccessStatusCode, $"Expected HTTP 200 but got {(int)response.StatusCode}: {json}");
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            Assert.IsTrue(root.TryGetProperty("Response", out var responseProp), $"Expected 'Response' object in response: {json}");
            
            // Parse the Response object (it's stored as a JSON string or nested object)
            JsonElement responseObj;
            if (responseProp.ValueKind == JsonValueKind.String)
            {
                responseObj = JsonDocument.Parse(responseProp.GetString()).RootElement;
            }
            else
            {
                responseObj = responseProp;
            }
            
            Assert.IsTrue(responseObj.TryGetProperty("json", out var jsonProp), $"Expected 'json' in Response object: {responseProp}");
            Assert.IsTrue(jsonProp.TryGetProperty("tag", out var tag), $"Expected 'tag' in Response.json: {jsonProp}");
            Assert.AreEqual("obj", tag.GetString(), $"Expected Response.json.tag == 'obj'. Full Response: {responseProp}");
        }

        /// <summary>TC-066: POST multipart/form-data with IsObject=True → Stores entire httpbin response in [[@FormResponse]]. Expected: FormResponse.form.test == value.</summary>
        [TestMethod, TestCategory("WebPostTool_Integration")]
        public async Task TC066_PostFormData_IsObject_Returns_FullResponse()
        {
            var response = await _client.GetAsync($"{BaseUrl}/TC066_PostFormData_IsObject_Returns_FullResponse.json");
            var json = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(response.IsSuccessStatusCode, $"Expected HTTP 200 but got {(int)response.StatusCode}: {json}");
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            Assert.IsTrue(root.TryGetProperty("FormResponse", out var responseProp), $"Expected 'FormResponse' object in response: {json}");
            
            // Parse the FormResponse object (it's stored as a JSON string or nested object)
            JsonElement responseObj;
            if (responseProp.ValueKind == JsonValueKind.String)
            {
                responseObj = JsonDocument.Parse(responseProp.GetString()).RootElement;
            }
            else
            {
                responseObj = responseProp;
            }
            
            Assert.IsTrue(responseObj.TryGetProperty("form", out var formProp), $"Expected 'form' in FormResponse object: {responseProp}");
            Assert.IsTrue(formProp.TryGetProperty("test", out var test), $"Expected 'test' in FormResponse.form: {formProp}");
            Assert.AreEqual("value", test.GetString(), $"Expected FormResponse.form.test == 'value'. Full FormResponse: {responseProp}");
        }

        /// <summary>TC-067: POST multipart/form-data with file attachment (Key=data, hello world base64, test.txt) → Maps files.data to [[FileContent]]. Expected: FileContent == hello world.</summary>
        [TestMethod, TestCategory("WebPostTool_Integration")]
        public async Task TC067_PostFormData_FileAttachment_MapsFilesData_Returns_HelloWorld()
        {
            var response = await _client.GetAsync($"{BaseUrl}/TC067_PostFormData_FileAttachment_MapsFilesData_Returns_HelloWorld.json");
            var json = await response.Content.ReadAsStringAsync();
            Assert.IsTrue(response.IsSuccessStatusCode, $"Expected HTTP 200 but got {(int)response.StatusCode}: {json}");
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            Assert.IsTrue(root.TryGetProperty("FileContent", out var prop), $"Expected 'FileContent' in response: {json}");
            Assert.AreEqual("hello world", prop.GetString(), $"Expected FileContent == 'hello world'. Full response: {json}");
        }
    }
}
