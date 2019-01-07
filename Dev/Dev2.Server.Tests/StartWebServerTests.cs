using System;
using System.Collections.Generic;
using System.Net;
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
            mockPauseHelper.Verify(o => o.Pause(), Times.Once);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(StartWebServer))]
        public void StartWebServer_VerifyDispose_ExpectIsCalledOne()
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

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(StartWebServer))]
        public void StartWebServer_DisposeCatch_DoesNotClushTheSystem_ExpectNomarlFlowAfter()
        {
            //-------------------Arrange---------------------
            var listEndPoints = new List<Dev2Endpoint>();
            var mockWriter = new Mock<IWriter>();
            var mockWebServerConfiguration = new Mock<IWebServerConfiguration>();
            var mockPauseHelper = new Mock<IPauseHelper>();
            mockWebServerConfiguration.Setup(o => o.IsWebServerEnabled).Returns(true);
            mockWebServerConfiguration.Setup(o => o.IsWebServerSslEnabled).Returns(true);

            mockWebServerConfiguration.Setup(o => o.EndPoints).Returns(new Dev2Endpoint[] { new Dev2Endpoint(new IPEndPoint(0x40E9BB63, 8080), "Url", "path") });

            mockWriter.Setup(o => o.Fail(It.IsAny<string>(), It.IsAny<Exception>()));
            //-------------------Act-------------------------
            using (var start = new StartWebServer(mockWriter.Object, StartAction1))
            {
                start.Execute(mockWebServerConfiguration.Object, mockPauseHelper.Object);
            }

            //-------------------Assert----------------------
            mockWriter.Verify(a => a.WriteLine("Web server listening at Url"), Times.Once);
        }

        IDisposable StartAction1(Dev2Endpoint[] endPoints = null)
        {
            Mock<IDisposable> mockServer1 = new Mock<IDisposable>();
            mockServer1.Setup(o => o.Dispose()).Callback(() => throw new Exception());

            return mockServer1.Object;
        }


        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(StartWebServer))]
        public void StartWebServer_WebServerConfigurationEndPoints_NotNull_ExpectListOfEndpoints()
        {
            //-------------------Arrange---------------------
            var listEndPoints = new List<Dev2Endpoint>();
            var mockWriter = new Mock<IWriter>();
            var mockWebServerConfiguration = new Mock<IWebServerConfiguration>();
            var mockPauseHelper = new Mock<IPauseHelper>();
            mockWebServerConfiguration.Setup(o => o.IsWebServerEnabled).Returns(true);
            mockWebServerConfiguration.Setup(o => o.IsWebServerSslEnabled).Returns(true);
            
            mockWebServerConfiguration.Setup(o => o.EndPoints).Returns(new Dev2Endpoint[] { new Dev2Endpoint(new IPEndPoint(0x40E9BB63, 8080),"Url", "path") });

            mockWriter.Setup(o => o.Fail(It.IsAny<string>(), It.IsAny<Exception>()));
            //-------------------Act-------------------------
            using (var start = new StartWebServer(mockWriter.Object, StartAction))
            {
                start.Execute(mockWebServerConfiguration.Object, mockPauseHelper.Object);
            }

            //-------------------Assert----------------------
            mockWriter.Verify(a => a.WriteLine("Web server listening at Url"), Times.Once);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(StartWebServer))]
        public void StartWebServer_IsWebServerConfig_False_ExpectException()
        {
            //-------------------Arrange---------------------
            var mockWriter = new Mock<IWriter>();
            var mockWebServerConfiguration = new Mock<IWebServerConfiguration>();
            var mockPauseHelper = new Mock<IPauseHelper>();
            mockWebServerConfiguration.Setup(o => o.IsWebServerEnabled).Returns(false);
            mockWebServerConfiguration.Setup(o => o.IsWebServerSslEnabled).Returns(true);

            mockWriter.Setup(o => o.Fail(It.IsAny<string>(), It.IsAny<Exception>()));
            //-------------------Act-------------------------
            new StartWebServer(mockWriter.Object, WebServerStartup.Start).Execute(mockWebServerConfiguration.Object, mockPauseHelper.Object);
            //-------------------Assert----------------------
            mockPauseHelper.Verify(o => o.Pause(), Times.Once);
        }

    }
}
