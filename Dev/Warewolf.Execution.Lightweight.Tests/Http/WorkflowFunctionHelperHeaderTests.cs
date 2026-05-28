using Dev2.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Warewolf.Execution.Lightweight.Tests.Auth;

namespace Warewolf.Execution.Lightweight.Tests.Http
{
    /// <summary>
    /// Tests for <see cref="WorkflowFunctionHelper.ParseRequestAsync"/> covering:
    /// - Content-Type/Accept header fallback for return type when no URL suffix
    /// - Warewolf-Execution-Id header extraction
    /// - Warewolf-Custom-Transaction-Id header extraction
    /// </summary>
    [TestClass]
    public class WorkflowFunctionHelperHeaderTests
    {
        static FakeHttpRequestData CreateRequest(string url, string method = "GET")
        {
            var ctx = new TestFunctionContext();
            return new FakeHttpRequestData(ctx, new Uri(url), method);
        }

        // ══════════════════════════════════════════════════════════════════════════
        // Content-Type / Accept header → ReturnType fallback
        // ══════════════════════════════════════════════════════════════════════════

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task ParseRequestAsync_NoSuffix_ContentTypeXml_SetsXmlReturnType()
        {
            var req = CreateRequest("https://localhost/api/Services/HelloWorld");
            req.AddHeader("Content-Type", "application/xml");

            var result = await WorkflowFunctionHelper.ParseRequestAsync(req, "", "HelloWorld");

            Assert.AreEqual(EmitionTypes.XML, result.ReturnType);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task ParseRequestAsync_NoSuffix_AcceptJson_SetsJsonReturnType()
        {
            var req = CreateRequest("https://localhost/api/Services/HelloWorld");
            req.AddHeader("Accept", "application/json");

            var result = await WorkflowFunctionHelper.ParseRequestAsync(req, "", "HelloWorld");

            Assert.AreEqual(EmitionTypes.JSON, result.ReturnType);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task ParseRequestAsync_NoSuffix_ContentTypeTakesPrecedenceOverAccept()
        {
            var req = CreateRequest("https://localhost/api/Services/HelloWorld");
            req.AddHeader("Content-Type", "text/xml");
            req.AddHeader("Accept", "application/json");

            var result = await WorkflowFunctionHelper.ParseRequestAsync(req, "", "HelloWorld");

            Assert.AreEqual(EmitionTypes.XML, result.ReturnType);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task ParseRequestAsync_XmlSuffix_IgnoresHeaders()
        {
            var req = CreateRequest("https://localhost/api/Services/HelloWorld.xml");
            req.AddHeader("Accept", "application/json");

            var result = await WorkflowFunctionHelper.ParseRequestAsync(req, "", "HelloWorld");

            Assert.AreEqual(EmitionTypes.XML, result.ReturnType);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task ParseRequestAsync_ApiSuffix_SetsOpenApi()
        {
            var req = CreateRequest("https://localhost/api/Services/HelloWorld.api");

            var result = await WorkflowFunctionHelper.ParseRequestAsync(req, "", "HelloWorld");

            Assert.AreEqual(EmitionTypes.OPENAPI, result.ReturnType);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task ParseRequestAsync_NoSuffix_NoHeaders_DefaultsToJson()
        {
            var req = CreateRequest("https://localhost/api/Services/HelloWorld");

            var result = await WorkflowFunctionHelper.ParseRequestAsync(req, "", "HelloWorld");

            Assert.AreEqual(EmitionTypes.JSON, result.ReturnType);
        }

        // ══════════════════════════════════════════════════════════════════════════
        // Warewolf tracing headers
        // ══════════════════════════════════════════════════════════════════════════

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task ParseRequestAsync_WarewolfExecutionId_Parsed()
        {
            var expectedId = Guid.NewGuid();
            var req = CreateRequest("https://localhost/api/Services/HelloWorld");
            req.AddHeader("Warewolf-Execution-Id", expectedId.ToString());

            var result = await WorkflowFunctionHelper.ParseRequestAsync(req, "", "HelloWorld");

            Assert.AreEqual(expectedId, result.ExecutionId);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task ParseRequestAsync_WarewolfCustomTransactionId_Parsed()
        {
            var req = CreateRequest("https://localhost/api/Services/HelloWorld");
            req.AddHeader("Warewolf-Custom-Transaction-Id", "TX-12345");

            var result = await WorkflowFunctionHelper.ParseRequestAsync(req, "", "HelloWorld");

            Assert.AreEqual("TX-12345", result.CustomTransactionId);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task ParseRequestAsync_InvalidExecutionIdGuid_IgnoredGracefully()
        {
            var req = CreateRequest("https://localhost/api/Services/HelloWorld");
            req.AddHeader("Warewolf-Execution-Id", "not-a-guid");

            var result = await WorkflowFunctionHelper.ParseRequestAsync(req, "", "HelloWorld");

            Assert.IsNull(result.ExecutionId);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task ParseRequestAsync_NoTracingHeaders_PropertiesRemainNull()
        {
            var req = CreateRequest("https://localhost/api/Services/HelloWorld");

            var result = await WorkflowFunctionHelper.ParseRequestAsync(req, "", "HelloWorld");

            Assert.IsNull(result.ExecutionId);
            Assert.IsNull(result.CustomTransactionId);
        }
    }
}
