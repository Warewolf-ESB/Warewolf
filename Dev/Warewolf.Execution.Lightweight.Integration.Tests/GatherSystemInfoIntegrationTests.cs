using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Warewolf.Execution.Lightweight.Integration.Tests.InProcess;

namespace Warewolf.Execution.Lightweight.Integration.Tests
{
    /// <summary>
    /// In-process functional tests for the Gather System Information activity executed
    /// via <see cref="WorkflowHttpFunction"/>.
    ///
    /// These tests no longer require a separately-running engine on port 7071. The
    /// <see cref="LightweightInProcessHost"/> seeds a real (encrypted) secure.config so
    /// permissions are resolved through the genuine loader/matcher, and runs the REAL
    /// <c>WorkflowExecutor</c> against the deployed workflow resource
    /// (<c>Resources/tools/system info/TestGettingComputerName.bite</c>) so the workflow
    /// actually executes and produces real output.
    ///
    /// Marked <see cref="DoNotParallelizeAttribute"/> because the host mutates the
    /// process-wide <c>SecureConfigLoader</c> singleton + <c>WAREWOLF_SECURE_CONFIG</c>
    /// env var for the duration of each test.
    /// </summary>
    [TestClass]
    [DoNotParallelize]
    public class GatherSystemInfoIntegrationTests
    {
        // Catch-all route value exactly as the Functions host supplies it (URL-decoded).
        private const string ComputerNameRoute = "tools/system info/TestGettingComputerName.json";

        /// <summary>
        /// TC001: Executes the TestGettingComputerName workflow on the anonymous /public/
        /// route with the Public group granted View+Execute, and asserts a non-empty
        /// ComputerName scalar is returned.
        /// </summary>
        [TestMethod, TestCategory("GatherSystemInfo_Integration")]
        public async Task TC001_GetComputerName_ReturnsNonEmpty()
        {
            // Arrange — Public group has server-wide View+Execute, so the anonymous
            // caller is authorised to execute the workflow on the /public/ route.
            using var host = LightweightInProcessHost.WithPublicExecuteAll();

            // Act
            var (status, json) = await host.ExecutePublicAsync(ComputerNameRoute);

            TestContext.WriteLine($"HTTP status : {(int)status} {status}");
            TestContext.WriteLine($"Raw response: {json}");

            // Assert — authorisation succeeded and the workflow executed.
            Assert.AreEqual(HttpStatusCode.OK, status,
                $"Expected HTTP 200 but got {(int)status}: {json}");

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
