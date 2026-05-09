#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, CC0022, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001, IDE0019, CC0105, RECS008, CA2202, IDE0016
/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Integration-level step definitions for EasyAuthMiddleware.feature.
 *  Tests exercise the EasyAuth redirect middleware via real HTTP requests
 *  to the lightweight Warewolf server running at http://localhost:7071,
 *  providing overlapping coverage with the unit tests in
 *  Warewolf.Execution.Lightweight.Tests.Auth.EasyAuthRedirectMiddlewareTests.
 */

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;

namespace Warewolf.Security.Specs
{
    [Binding]
    public class EasyAuthMiddlewareSteps
    {
        private const string BaseUrl        = "http://localhost:7071";
        private const string ResponseKey    = "EasyAuthResponse";
        private const string ResponseBodyKey = "EasyAuthResponseBody";

        // Fake Bearer token recognised by the integration-test server configuration.
        private const string BearerToken = "ASfas123@!fda_LONG_TOKEN_GENERATED_FROM_ENTRA";

        private readonly ScenarioContext _scenarioContext;

        public EasyAuthMiddlewareSteps(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext ?? throw new ArgumentNullException(nameof(scenarioContext));
        }

        // ── Background ────────────────────────────────────────────────────────────

        [Given(@"the lightweight Warewolf server is running")]
        public void GivenTheLightweightWarewolfServerIsRunning()
        {
            using var probe = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false });
            HttpResponseMessage response = null;
            try
            {
                response = probe.GetAsync($"{BaseUrl}/public/apis.json").Result;
                // A 200 or 401 both confirm the server is up and routing requests.
                if (response.StatusCode != HttpStatusCode.OK &&
                    response.StatusCode != HttpStatusCode.Unauthorized)
                {
                    Assert.Fail(
                        $"Lightweight Warewolf server at {BaseUrl} returned unexpected status " +
                        $"{(int)response.StatusCode} during probe. Ensure the server is running.");
                }
            }
            catch (Exception ex) when (!(ex is AssertFailedException))
            {
                Assert.Fail(
                    $"Cannot reach lightweight Warewolf server at {BaseUrl}. " +
                    $"Ensure the server is running before executing EasyAuthMiddleware specs. {ex.Message}");
            }
            finally
            {
                response?.Dispose();
            }
        }

        // ── When — request types ──────────────────────────────────────────────────

        [When(@"an API client requests ""(.*)"" with no authentication")]
        public void WhenAnAPIClientRequestsWithNoAuthentication(string path)
        {
            var handler = new HttpClientHandler { AllowAutoRedirect = false };
            using var client = new HttpClient(handler);
            // No Authorization header, no Easy Auth headers, no browser Accept header.
            var response = client.GetAsync($"{BaseUrl}{path}").Result;
            CaptureResponse(response);
        }

        [When(@"a browser navigates to ""(.*)"" without authentication")]
        public void WhenABrowserNavigatesToWithoutAuthentication(string path)
        {
            var handler = new HttpClientHandler { AllowAutoRedirect = false };
            using var client = new HttpClient(handler);
            // Simulate a browser: Accept text/html, no Sec-Fetch-Mode: cors.
            client.DefaultRequestHeaders.Accept.ParseAdd("text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            var response = client.GetAsync($"{BaseUrl}{path}").Result;
            CaptureResponse(response);
        }

        [When(@"an API client requests ""(.*)"" with a Bearer token")]
        public void WhenAnAPIClientRequestsWithABearerToken(string path)
        {
            var handler = new HttpClientHandler { AllowAutoRedirect = false };
            using var client = new HttpClient(handler);
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", BearerToken);
            var response = client.GetAsync($"{BaseUrl}{path}").Result;
            CaptureResponse(response);
        }

        [When(@"an API client requests ""(.*)"" with an Easy Auth principal header")]
        public void WhenAnAPIClientRequestsWithAnEasyAuthPrincipalHeader(string path)
        {
            var handler = new HttpClientHandler { AllowAutoRedirect = false };
            using var client = new HttpClient(handler);
            // Build a minimal Easy Auth principal payload (base64-encoded JSON).
            var principalJson = "{\"auth_typ\":\"aad\",\"claims\":[{\"typ\":\"name\",\"val\":\"Integration Test User\"}],\"name_typ\":\"name\",\"role_typ\":\"roles\"}";
            var principalBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(principalJson));
            client.DefaultRequestHeaders.Add("X-MS-CLIENT-PRINCIPAL", principalBase64);
            var response = client.GetAsync($"{BaseUrl}{path}").Result;
            CaptureResponse(response);
        }

        // ── Then — assertions ─────────────────────────────────────────────────────

        [Then(@"the HTTP response status code should be (\d+)")]
        public void ThenTheHTTPResponseStatusCodeShouldBe(int expectedStatus)
        {
            var response = GetCapturedResponse();
            Assert.AreEqual(
                expectedStatus,
                (int)response.StatusCode,
                $"Expected HTTP {expectedStatus} but got {(int)response.StatusCode} ({response.StatusCode}).");
        }

        [Then(@"the HTTP response status code should not be (\d+)")]
        public void ThenTheHTTPResponseStatusCodeShouldNotBe(int unexpectedStatus)
        {
            var response = GetCapturedResponse();
            Assert.AreNotEqual(
                unexpectedStatus,
                (int)response.StatusCode,
                $"Expected response NOT to be HTTP {unexpectedStatus}, but it was. " +
                "The EasyAuth middleware should have passed the authenticated request to the next middleware.");
        }

        [Then(@"the response body should contain ""(.*)""")]
        public void ThenTheResponseBodyShouldContain(string expected)
        {
            var body = GetCapturedBody();
            Assert.IsTrue(
                body.Contains(expected, StringComparison.OrdinalIgnoreCase),
                $"Expected response body to contain \"{expected}\" but body was: {body}");
        }

        [Then(@"the response should include a ""(.*)"" header")]
        public void ThenTheResponseShouldIncludeAHeader(string headerName)
        {
            var response = GetCapturedResponse();
            Assert.IsTrue(
                response.Headers.Contains(headerName),
                $"Expected response to include a \"{headerName}\" header but it was absent.");
        }

        [Then(@"the ""(.*)"" header should contain ""(.*)""")]
        public void ThenTheHeaderShouldContain(string headerName, string expectedValue)
        {
            var response = GetCapturedResponse();
            Assert.IsTrue(
                response.Headers.TryGetValues(headerName, out var values),
                $"Expected response to include a \"{headerName}\" header but it was absent.");
            var joined = string.Join(", ", values);
            Assert.IsTrue(
                joined.Contains(expectedValue, StringComparison.OrdinalIgnoreCase),
                $"Expected \"{headerName}\" header to contain \"{expectedValue}\" but was: {joined}");
        }

        // ── AfterScenario cleanup ─────────────────────────────────────────────────

        [AfterScenario("EasyAuthMiddleware")]
        public void CleanUpAfterScenario()
        {
            if (_scenarioContext.TryGetValue(ResponseKey, out HttpResponseMessage response))
                response?.Dispose();
        }

        // ── Private helpers ───────────────────────────────────────────────────────

        private void CaptureResponse(HttpResponseMessage response)
        {
            // Read and cache the body now while the connection is still open.
            var body = string.Empty;
            try { body = response.Content.ReadAsStringAsync().Result; } catch { /* best-effort */ }
            _scenarioContext[ResponseKey]     = response;
            _scenarioContext[ResponseBodyKey] = body;
        }

        private HttpResponseMessage GetCapturedResponse()
        {
            if (!_scenarioContext.TryGetValue(ResponseKey, out HttpResponseMessage response) || response is null)
                Assert.Fail("No HTTP response was captured. Ensure a When step ran before this Then step.");
            return response;
        }

        private string GetCapturedBody()
        {
            if (!_scenarioContext.TryGetValue(ResponseBodyKey, out string body))
                return string.Empty;
            return body ?? string.Empty;
        }
    }
}
