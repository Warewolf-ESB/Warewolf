using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Warewolf.Execution.Lightweight.Integration.Tests.InProcess;

namespace Warewolf.Execution.Lightweight.Integration.Tests
{
    /// <summary>
    /// In-process tests for the Profiler mock journey-email API workflows
    /// (<c>Profiler/Mock/email/validate.bite</c> and <c>send.bite</c>) that stand in for the
    /// real email host behind <c>WarewolfCollectionsNewJourneyEmailSourceHost</c>.
    /// </summary>
    [TestClass]
    [DoNotParallelize]
    public class MockEmailApiIntegrationTests
    {
        public TestContext TestContext { get; set; } = null!;

        [TestMethod, TestCategory("MockEmailApi_Integration")]
        [DataRow("Mock/email/validate.json")]
        [DataRow("Mock/email/send.json")]
        public async Task ValidPayload_ReturnsSuccess(string endpoint)
        {
            var (status, body) = await Call(endpoint, "ToEmail=user%40example.com&TemplateName=MissingGrantDates");

            Assert.AreEqual(HttpStatusCode.OK, status, body);
            Assert.AreEqual("SUCCESS", Prop(body, "Status"), body);
            Assert.AreEqual(string.Empty, Prop(body, "Error"), body);
        }

        [TestMethod, TestCategory("MockEmailApi_Integration")]
        [DataRow("Mock/email/validate.json", "ToEmail=not-an-email&TemplateName=T1")]
        [DataRow("Mock/email/validate.json", "ToEmail=user%40example.com")]
        [DataRow("Mock/email/validate.json", "TemplateName=T1")]
        [DataRow("Mock/email/validate.json", "")]
        [DataRow("Mock/email/send.json", "ToEmail=not-an-email&TemplateName=T1")]
        [DataRow("Mock/email/send.json", "ToEmail=user%40example.com")]
        [DataRow("Mock/email/send.json", "")]
        public async Task InvalidPayload_ReturnsFailedWithError(string endpoint, string query)
        {
            var (status, body) = await Call(endpoint, query);

            Assert.AreEqual(HttpStatusCode.OK, status, body);
            Assert.AreEqual("FAILED", Prop(body, "Status"), body);
            Assert.IsFalse(string.IsNullOrWhiteSpace(Prop(body, "Error")), body);
        }

        private static async Task<(HttpStatusCode, string)> Call(string endpoint, string query)
        {
            using var host = LightweightInProcessHost.WithPublicExecuteAll();
            return await host.ExecutePublicAsync(query.Length == 0 ? endpoint : $"{endpoint}?{query}");
        }

        private static string Prop(string json, string name)
        {
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.TryGetProperty(name, out var v) ? v.GetString() ?? string.Empty : "<missing>";
        }
    }
}
