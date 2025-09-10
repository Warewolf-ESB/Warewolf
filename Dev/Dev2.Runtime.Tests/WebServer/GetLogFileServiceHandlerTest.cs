/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using Dev2.Runtime.WebServer;
using Dev2.Runtime.WebServer.Handlers;
using Dev2.Runtime.WebServer.Responses;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Runtime.WebServer
{
    /// <summary>
    /// Summary description for WebsiteResourceHandlerTest
    /// </summary>
    [TestClass]
    [TestCategory("Runtime WebServer")]
    public class GetLogFileServiceHandlerTest
    {
        const string TestLogContent = @"2023-01-01 10:00:01,123 INFO  - First log line
2023-01-01 10:00:02,456 DEBUG - Second log line
2023-01-01 10:00:03,789 WARN  - Third log line
2023-01-01 10:00:04,012 ERROR - Fourth log line
2023-01-01 10:00:05,345 INFO  - Fifth log line";

        NameValueCollection LocalQueryString => new NameValueCollection
        {
            { "Name", "the_name" },
            { "numLines", "5" },
            { "wid", "workflowid" },
            { "rid", "resourceid" }
        };

        NameValueCollection EmptyQueryString => new NameValueCollection();

        NameValueCollection QueryStringWithInvalidNumLines => new NameValueCollection
        {
            { "numLines", "invalid" }
        };

        NameValueCollection QueryStringWithZeroNumLines => new NameValueCollection
        {
            { "numLines", "0" }
        };

        NameValueCollection QueryStringWithNegativeNumLines => new NameValueCollection
        {
            { "numLines", "-5" }
        };

        [TestInitialize]
        public void Setup()
        {
            CreateTestLogFile();
        }

        [TestCleanup]
        public void Cleanup()
        {
            CleanupTestLogFile();
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void ProcessRequest_GiveQueryStrignHasNoKeys()
        {
            var communicationContext = new Mock<ICommunicationContext>();
            var request = new Mock<ICommunicationRequest>();
            request.Setup(communicationRequest => communicationRequest.QueryString).Returns(new NameValueCollection());
            communicationContext.Setup(context => context.Request).Returns(request.Object);
            //------------Setup for test-------------------------
            var handler = new GetLogFileServiceHandler();
            //------------Execute Test---------------------------
            handler.ProcessRequest(communicationContext.Object);
            //------------Assert Results-------------------------
            communicationContext.Verify(ctx => ctx.Send(It.IsAny<IResponseWriter>()), Times.Once);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void ProcessRequest_GiveQueryStrignHasKeys()
        {
            var communicationContext = new Mock<ICommunicationContext>();
            var request = new Mock<ICommunicationRequest>();
            request.Setup(communicationRequest => communicationRequest.QueryString).Returns(LocalQueryString);
            communicationContext.Setup(context => context.Request).Returns(request.Object);
            //------------Setup for test-------------------------
            var handler = new GetLogFileServiceHandler();
            //------------Execute Test---------------------------
            handler.ProcessRequest(communicationContext.Object);
            //------------Assert Results-------------------------
            communicationContext.Verify(ctx => ctx.Send(It.IsAny<IResponseWriter>()), Times.Once);
        }

        [TestMethod]
        [Owner("GitHub Copilot")]
        [TestCategory("GetLogFileServiceHandler_NumLines")]
        public void ProcessRequest_WithValidNumLines_CallsSendOnce()
        {
            //------------Setup for test--------------------------
            var communicationContext = new Mock<ICommunicationContext>();
            var request = new Mock<ICommunicationRequest>();
            var queryString = new NameValueCollection { { "numLines", "3" } };
            request.Setup(communicationRequest => communicationRequest.QueryString).Returns(queryString);
            communicationContext.Setup(context => context.Request).Returns(request.Object);

            var handler = new GetLogFileServiceHandler();

            //------------Execute Test---------------------------
            handler.ProcessRequest(communicationContext.Object);

            //------------Assert Results-------------------------
            communicationContext.Verify(ctx => ctx.Send(It.IsAny<IResponseWriter>()), Times.Once);
        }

        [TestMethod]
        [Owner("GitHub Copilot")]
        [TestCategory("GetLogFileServiceHandler_NumLines")]
        public void ProcessRequest_WithoutNumLines_ReturnsFileResponseWriter()
        {
            //------------Setup for test--------------------------
            var communicationContext = new Mock<ICommunicationContext>();
            var request = new Mock<ICommunicationRequest>();
            request.Setup(communicationRequest => communicationRequest.QueryString).Returns(EmptyQueryString);
            communicationContext.Setup(context => context.Request).Returns(request.Object);

            FileResponseWriter capturedWriter = null;
            communicationContext.Setup(ctx => ctx.Send(It.IsAny<FileResponseWriter>()))
                .Callback<IResponseWriter>(writer => capturedWriter = writer as FileResponseWriter);

            var handler = new GetLogFileServiceHandler();

            //------------Execute Test---------------------------
            handler.ProcessRequest(communicationContext.Object);

            //------------Assert Results-------------------------
            communicationContext.Verify(ctx => ctx.Send(It.IsAny<FileResponseWriter>()), Times.Once);
            Assert.IsNotNull(capturedWriter, "Expected FileResponseWriter to be sent");
        }

        [TestMethod]
        [Owner("GitHub Copilot")]
        [TestCategory("GetLogFileServiceHandler_NumLines")]
        public void ProcessRequest_WithInvalidNumLines_CallsSendOnce()
        {
            //------------Setup for test--------------------------
            var communicationContext = new Mock<ICommunicationContext>();
            var request = new Mock<ICommunicationRequest>();
            var queryString = new NameValueCollection { { "numLines", "invalid" } };
            request.Setup(communicationRequest => communicationRequest.QueryString).Returns(queryString);
            communicationContext.Setup(context => context.Request).Returns(request.Object);

            var handler = new GetLogFileServiceHandler();

            //------------Execute Test---------------------------
            handler.ProcessRequest(communicationContext.Object);

            //------------Assert Results-------------------------
            communicationContext.Verify(ctx => ctx.Send(It.IsAny<IResponseWriter>()), Times.Once);
        }

        [TestMethod]
        [Owner("GitHub Copilot")]
        [TestCategory("GetLogFileServiceHandler_NumLines")]
        public void ProcessRequest_WithZeroNumLines_CallsSendOnce()
        {
            //------------Setup for test--------------------------
            var communicationContext = new Mock<ICommunicationContext>();
            var request = new Mock<ICommunicationRequest>();
            var queryString = new NameValueCollection { { "numLines", "0" } };
            request.Setup(communicationRequest => communicationRequest.QueryString).Returns(queryString);
            communicationContext.Setup(context => context.Request).Returns(request.Object);

            var handler = new GetLogFileServiceHandler();

            //------------Execute Test---------------------------
            handler.ProcessRequest(communicationContext.Object);

            //------------Assert Results-------------------------
            communicationContext.Verify(ctx => ctx.Send(It.IsAny<IResponseWriter>()), Times.Once);
        }

        [TestMethod]
        [Owner("GitHub Copilot")]
        [TestCategory("GetLogFileServiceHandler_NumLines")]
        public void ProcessRequest_WithNegativeNumLines_CallsSendOnce()
        {
            //------------Setup for test--------------------------
            var communicationContext = new Mock<ICommunicationContext>();
            var request = new Mock<ICommunicationRequest>();
            var queryString = new NameValueCollection { { "numLines", "-5" } };
            request.Setup(communicationRequest => communicationRequest.QueryString).Returns(queryString);
            communicationContext.Setup(context => context.Request).Returns(request.Object);

            var handler = new GetLogFileServiceHandler();

            //------------Execute Test---------------------------
            handler.ProcessRequest(communicationContext.Object);

            //------------Assert Results-------------------------
            communicationContext.Verify(ctx => ctx.Send(It.IsAny<IResponseWriter>()), Times.Once);
        }

        [TestMethod]
        [Owner("GitHub Copilot")]
        [TestCategory("GetLogFileServiceHandler_ReadLastLines")]
        public void ReadLastLines_WithNonExistentFile_ReturnsEmptyArray()
        {
            //------------Setup for test--------------------------
            var readLastLinesMethod = GetPrivateMethod("ReadLastLines");

            //------------Execute Test---------------------------
            var result = (string[])readLastLinesMethod.Invoke(null, new object[] { "non_existent_file.log", 5 });

            //------------Assert Results-------------------------
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Length);
        }

        [TestMethod]
        [Owner("GitHub Copilot")]
        [TestCategory("GetLogFileServiceHandler_ReadLastLines")]
        public void ReadLastLines_WithValidFile_ReturnsCorrectNumberOfLines()
        {
            //------------Setup for test--------------------------
            var readLastLinesMethod = GetPrivateMethod("ReadLastLines");
            var testFilePath = GetTestLogFilePath();

            //------------Execute Test---------------------------
            var result = (string[])readLastLinesMethod.Invoke(null, new object[] { testFilePath, 3 });

            //------------Assert Results-------------------------
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Length);
            Assert.IsTrue(result[0].Contains("Third log line"));
            Assert.IsTrue(result[1].Contains("Fourth log line"));
            Assert.IsTrue(result[2].Contains("Fifth log line"));
        }

        [TestMethod]
        [Owner("GitHub Copilot")]
        [TestCategory("GetLogFileServiceHandler_ReadLastLines")]
        public void ReadLastLines_WithLargeNumLines_ReturnsAllAvailableLines()
        {
            //------------Setup for test--------------------------
            var readLastLinesMethod = GetPrivateMethod("ReadLastLines");
            var testFilePath = GetTestLogFilePath();

            //------------Execute Test---------------------------
            var result = (string[])readLastLinesMethod.Invoke(null, new object[] { testFilePath, 100 });

            //------------Assert Results-------------------------
            Assert.IsNotNull(result);
            Assert.AreEqual(5, result.Length); // Only 5 lines exist in test file
            Assert.IsTrue(result[0].Contains("First log line"));
            Assert.IsTrue(result[4].Contains("Fifth log line"));
        }

        [TestMethod]
        [Owner("GitHub Copilot")]
        [TestCategory("GetLogFileServiceHandler_ReadLastLines")]
        public void ReadLastLines_WithSmallFile_UsesSimpleAlgorithm()
        {
            //------------Setup for test--------------------------
            var readLastLinesMethod = GetPrivateMethod("ReadLastLines");
            var testFilePath = GetTestLogFilePath();

            //------------Execute Test---------------------------
            var result = (string[])readLastLinesMethod.Invoke(null, new object[] { testFilePath, 2 });

            //------------Assert Results-------------------------
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Length);
            Assert.IsTrue(result[0].Contains("Fourth log line"));
            Assert.IsTrue(result[1].Contains("Fifth log line"));
        }

        [TestMethod]
        [Owner("GitHub Copilot")]
        [TestCategory("GetLogFileServiceHandler_ReadLastLinesEfficient")]
        public void ReadLastLinesEfficient_WithValidFileStream_ReturnsCorrectLines()
        {
            //------------Setup for test--------------------------
            var readLastLinesEfficientMethod = GetPrivateMethod("ReadLastLinesEfficient");
            var testFilePath = GetTestLogFilePath();

            using (var fileStream = File.Open(testFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                //------------Execute Test---------------------------
                var result = (string[])readLastLinesEfficientMethod.Invoke(null, new object[] { fileStream, 2 });

                //------------Assert Results-------------------------
                Assert.IsNotNull(result);
                Assert.AreEqual(2, result.Length);
                Assert.IsTrue(result[0].Contains("Fourth log line"));
                Assert.IsTrue(result[1].Contains("Fifth log line"));
            }
        }

        [TestMethod]
        [Owner("GitHub Copilot")]
        [TestCategory("GetLogFileServiceHandler_ResponseType")]
        public void ProcessRequest_WithNumLines_SendsStringResponseWriter()
        {
            //------------Setup for test--------------------------
            var communicationContext = new Mock<ICommunicationContext>();
            var request = new Mock<ICommunicationRequest>();
            var queryString = new NameValueCollection { { "numLines", "3" } };
            request.Setup(communicationRequest => communicationRequest.QueryString).Returns(queryString);
            communicationContext.Setup(context => context.Request).Returns(request.Object);

            StringResponseWriter capturedWriter = null;
            communicationContext.Setup(ctx => ctx.Send(It.IsAny<StringResponseWriter>()))
                .Callback<IResponseWriter>(writer => capturedWriter = writer as StringResponseWriter);

            var handler = new GetLogFileServiceHandler();

            //------------Execute Test---------------------------
            handler.ProcessRequest(communicationContext.Object);

            //------------Assert Results-------------------------
            communicationContext.Verify(ctx => ctx.Send(It.IsAny<StringResponseWriter>()), Times.Once);
            Assert.IsNotNull(capturedWriter, "Expected StringResponseWriter to be sent");
        }

        [TestMethod]
        [Owner("GitHub Copilot")]
        [TestCategory("GetLogFileServiceHandler_ResponseType")]
        public void ProcessRequest_WithoutNumLines_SendsFileResponseWriter()
        {
            //------------Setup for test--------------------------
            var communicationContext = new Mock<ICommunicationContext>();
            var request = new Mock<ICommunicationRequest>();
            request.Setup(communicationRequest => communicationRequest.QueryString).Returns(EmptyQueryString);
            communicationContext.Setup(context => context.Request).Returns(request.Object);

            FileResponseWriter capturedWriter = null;
            communicationContext.Setup(ctx => ctx.Send(It.IsAny<FileResponseWriter>()))
                .Callback<IResponseWriter>(writer => capturedWriter = writer as FileResponseWriter);

            var handler = new GetLogFileServiceHandler();

            //------------Execute Test---------------------------
            handler.ProcessRequest(communicationContext.Object);

            //------------Assert Results-------------------------
            communicationContext.Verify(ctx => ctx.Send(It.IsAny<FileResponseWriter>()), Times.Once);
            Assert.IsNotNull(capturedWriter, "Expected FileResponseWriter to be sent");
        }

        [TestMethod]
        [Owner("GitHub Copilot")]
        [TestCategory("GetLogFileServiceHandler_EdgeCases")]
        public void ReadLastLines_WithEmptyFile_ReturnsEmptyArray()
        {
            //------------Setup for test--------------------------
            var readLastLinesMethod = GetPrivateMethod("ReadLastLines");
            var emptyFilePath = Path.Combine(Path.GetTempPath(), "empty_test.log");
            File.WriteAllText(emptyFilePath, string.Empty);

            try
            {
                //------------Execute Test---------------------------
                var result = (string[])readLastLinesMethod.Invoke(null, new object[] { emptyFilePath, 5 });

                //------------Assert Results-------------------------
                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Length);
            }
            finally
            {
                if (File.Exists(emptyFilePath))
                {
                    File.Delete(emptyFilePath);
                }
            }
        }

        [TestMethod]
        [Owner("GitHub Copilot")]
        [TestCategory("GetLogFileServiceHandler_EdgeCases")]
        public void ReadLastLines_WithSingleLineFile_ReturnsCorrectly()
        {
            //------------Setup for test--------------------------
            var readLastLinesMethod = GetPrivateMethod("ReadLastLines");
            var singleLineFilePath = Path.Combine(Path.GetTempPath(), "single_line_test.log");
            const string singleLineContent = "2023-01-01 10:00:01,123 INFO  - Only log line";
            File.WriteAllText(singleLineFilePath, singleLineContent);

            try
            {
                //------------Execute Test---------------------------
                var result = (string[])readLastLinesMethod.Invoke(null, new object[] { singleLineFilePath, 5 });

                //------------Assert Results-------------------------
                Assert.IsNotNull(result);
                Assert.AreEqual(1, result.Length);
                Assert.AreEqual(singleLineContent, result[0]);
            }
            finally
            {
                if (File.Exists(singleLineFilePath))
                {
                    File.Delete(singleLineFilePath);
                }
            }
        }

        #region Helper Methods

        private static MethodInfo GetPrivateMethod(string methodName)
        {
            var method = typeof(GetLogFileServiceHandler).GetMethod(methodName, 
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(method, $"Could not find private method: {methodName}");
            return method;
        }

        private static string GetTestLogFilePath()
        {
            return Path.Combine(Path.GetTempPath(), "test_warewolf_server.log");
        }

        private static void CreateTestLogFile()
        {
            var testFilePath = GetTestLogFilePath();
            File.WriteAllText(testFilePath, TestLogContent);
        }

        private static void CleanupTestLogFile()
        {
            var testFilePath = GetTestLogFilePath();
            if (File.Exists(testFilePath))
            {
                try
                {
                    File.Delete(testFilePath);
                }
                catch
                {
                    // Ignore cleanup errors in tests
                }
            }
        }

        #endregion
    }
}
