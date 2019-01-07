using System;
using Dev2.Common.Wrappers;
using Dev2.Runtime.WebServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Server.Tests
{
    [TestClass]
    public class StartWebServerTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(StartWebServer))]
        public void StartWebServer_WebServerConfigurationEndPoints_Null_ExpectExeption()
        {
            //-------------------Arrange---------------------
            var mockWriter = new Mock<IWriter>();
            var mockWebServerConfiguration = new Mock<IWebServerConfiguration>();
            var mockPauseHelper = new Mock<IPauseHelper>();
            mockWebServerConfiguration.Setup(o => o.IsWebServerEnabled).Returns(true);
            mockWebServerConfiguration.Setup(o => o.IsWebServerSslEnabled).Returns(true);

            mockWriter.Setup(o => o.Fail(It.IsAny<string>(), It.IsAny<Exception>()));
            //-------------------Act-------------------------
            new StartWebServer(mockWriter.Object, WebServerStartup.Start).Execute(mockWebServerConfiguration.Object, mockPauseHelper.Object);
            //-------------------Assert----------------------
            mockWriter.Verify(o => o.Fail("Webserver failed to start", It.IsAny<MissingMemberException>()), Times.Once);
            mockPauseHelper.Verify(o => o.Pause(), Times.Once);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(StartWebServer))]
        public void StartWebServer_WebServerConfigurationEndPoints_Null_ExpectExeption4()
        {
            //-------------------Arrange---------------------
            var mockWriter = new Mock<IWriter>();
            var mockWebServerConfiguration = new Mock<IWebServerConfiguration>();
            var mockPauseHelper = new Mock<IPauseHelper>();
            mockWebServerConfiguration.Setup(o => o.IsWebServerEnabled).Returns(true);
            mockWebServerConfiguration.Setup(o => o.IsWebServerSslEnabled).Returns(true);
            mockWebServerConfiguration.SetupGet(o => o.EndPoints).Returns(new Dev2Endpoint[] {  });

            mockWriter.Setup(o => o.Fail(It.IsAny<string>(), It.IsAny<Exception>()));
            //-------------------Act-------------------------
            using (var start = new StartWebServer(mockWriter.Object, StartAction))
            {
                start.Execute(mockWebServerConfiguration.Object, mockPauseHelper.Object);
            }


            //-------------------Assert----------------------
            mockServer.Verify(o => o.Dispose(), Times.Once);
        }

        Mock<IDisposable> mockServer = new Mock<IDisposable>();
        IDisposable StartAction(Dev2Endpoint[] endPoints) {
            Assert.IsNotNull(endPoints);
            return mockServer.Object;
        }
    }
}
