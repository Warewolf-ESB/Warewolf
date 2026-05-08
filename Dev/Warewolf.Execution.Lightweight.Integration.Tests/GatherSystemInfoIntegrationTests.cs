using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Warewolf.Execution.Lightweight.Integration.Tests
{
    /// <summary>
    /// Integration tests for the Gather System Information activity executed via the Azure Function.
    /// Requires the Azure Function to be running at <see cref="BaseUrl"/> before running these tests.
    /// The workflow file is Resources/tools/system info/TestGettingComputerName.bite.
    /// </summary>
    [TestClass]
    public class GatherSystemInfoIntegrationTests
    {
        private const string BaseUrl = TestConstants.SystemInfoBaseUrl;
        private static readonly HttpClient _client = new();

        /// <summary>
        /// TC001: Executes TestGettingComputerName workflow → expects a non-empty ComputerName scalar in the JSON response.
        /// First-pass version: accepts any name and logs what is returned so subsequent versions can assert the exact value.
        /// </summary>
        [TestMethod, TestCategory("GatherSystemInfo_Integration")]
        public async Task TC001_GetComputerName_ReturnsNonEmpty()
        {
            var response = await _client.GetAsync($"{BaseUrl}/TestGettingComputerName.json");
            var json = await response.Content.ReadAsStringAsync();

            TestContext.WriteLine($"HTTP status : {(int)response.StatusCode} {response.StatusCode}");
            TestContext.WriteLine($"Raw response: {json}");

            Assert.IsTrue(response.IsSuccessStatusCode,
                $"Expected HTTP 200 but got {(int)response.StatusCode}: {json}");

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            TestContext.WriteLine($"JSON keys   : {string.Join(", ", EnumerateKeys(root))}");

            Assert.IsTrue(root.TryGetProperty("ComputerName", out var computerName),
                $"Expected 'ComputerName' property in response. Full response: {json}");

            var value = computerName.GetString();
            TestContext.WriteLine($"ComputerName: {value}");

            Assert.IsFalse(string.IsNullOrWhiteSpace(value),
                $"Expected non-empty ComputerName. Full response: {json}");
        }

        // Injected by the MSTest framework.
        public TestContext TestContext { get; set; } = null!;

        static System.Collections.Generic.IEnumerable<string> EnumerateKeys(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Object)
                foreach (var prop in element.EnumerateObject())
                    yield return prop.Name;
        }
    }
}
