using System;
using System.Text;
using Dev2.Core.Tests.Utils;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Network;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

// ReSharper disable InconsistentNaming
namespace Dev2.Core.Tests.UtilsTests
{
    [TestClass]
    public class WebServerTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("Webserver_Send")]
        public void Webserver_Send_ValidParameters_ShouldMakeCallToExecuteCommand()
        {
            //------------Setup for test--------------------------
            var mockResourceModel = new Mock<IContextualResourceModel>();
            var mockEnvironment = new Mock<IEnvironmentModel>();
            var mockConnection = new Mock<IEnvironmentConnection>();
            mockConnection.Setup(connection => connection.WorkspaceID).Returns(Guid.NewGuid());
            mockConnection.Setup(connection => connection.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Verifiable();
            mockConnection.Setup(connection => connection.IsConnected).Returns(true);
            mockEnvironment.Setup(model => model.Connection).Returns(mockConnection.Object);
            mockEnvironment.Setup(model => model.IsConnected).Returns(true);
            mockResourceModel.Setup(model => model.Environment).Returns(mockEnvironment.Object);
            //------------Execute Test---------------------------
            WebServer.Send(WebServerMethod.POST, mockResourceModel.Object, "DataPayLoad", AsyncWorkerTests.CreateSynchronousAsyncWorker().Object);
            //------------Assert Results-------------------------
            mockConnection.Verify(connection => connection.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once());
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("Webserver_Send")]
        public void Webserver_Send_NullResource_ShouldNotMakeCallToExecuteCommand()
        {
            //------------Setup for test--------------------------
            var mockResourceModel = new Mock<IContextualResourceModel>();
            var mockEnvironment = new Mock<IEnvironmentModel>();
            var mockConnection = new Mock<IEnvironmentConnection>();
            mockConnection.Setup(connection => connection.WorkspaceID).Returns(Guid.NewGuid());
            mockConnection.Setup(connection => connection.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Verifiable();
            mockConnection.Setup(connection => connection.IsConnected).Returns(true);
            mockEnvironment.Setup(model => model.Connection).Returns(mockConnection.Object);
            mockEnvironment.Setup(model => model.IsConnected).Returns(true);
            mockResourceModel.Setup(model => model.Environment).Returns(mockEnvironment.Object);
            //------------Execute Test---------------------------
            WebServer.Send(WebServerMethod.POST, null, "DataPayLoad", AsyncWorkerTests.CreateSynchronousAsyncWorker().Object);
            //------------Assert Results-------------------------
            mockConnection.Verify(connection => connection.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never());
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("Webserver_Send")]
        public void Webserver_Send_NullEnvironment_ShouldNotMakeCallToExecuteCommand()
        {
            //------------Setup for test--------------------------
            var mockEnvironment = new Mock<IEnvironmentModel>();
            var mockConnection = new Mock<IEnvironmentConnection>();
            mockConnection.Setup(connection => connection.WorkspaceID).Returns(Guid.NewGuid());
            mockConnection.Setup(connection => connection.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Verifiable();
            mockConnection.Setup(connection => connection.IsConnected).Returns(true);
            mockEnvironment.Setup(model => model.Connection).Returns(mockConnection.Object);
            mockEnvironment.Setup(model => model.IsConnected).Returns(true);
            //------------Execute Test---------------------------
            WebServer.Send(WebServerMethod.POST, null, "DataPayLoad", AsyncWorkerTests.CreateSynchronousAsyncWorker().Object);
            //------------Assert Results-------------------------
            mockConnection.Verify(connection => connection.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never());
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("Webserver_Send")]
        public void Webserver_Send_NullEnvironmentNotConnected_ShouldNotMakeCallToExecuteCommand()
        {
            //------------Setup for test--------------------------
            var mockResourceModel = new Mock<IContextualResourceModel>();
            var mockEnvironment = new Mock<IEnvironmentModel>();
            var mockConnection = new Mock<IEnvironmentConnection>();
            mockConnection.Setup(connection => connection.WorkspaceID).Returns(Guid.NewGuid());
            mockConnection.Setup(connection => connection.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Verifiable();
            mockConnection.Setup(connection => connection.IsConnected).Returns(true);
            mockEnvironment.Setup(model => model.Connection).Returns(mockConnection.Object);
            mockEnvironment.Setup(model => model.IsConnected).Returns(false);
            mockResourceModel.Setup(model => model.Environment).Returns(mockEnvironment.Object);
            //------------Execute Test---------------------------
            WebServer.Send(WebServerMethod.POST, null, "DataPayLoad", AsyncWorkerTests.CreateSynchronousAsyncWorker().Object);
            //------------Assert Results-------------------------
            mockConnection.Verify(connection => connection.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never());
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("Webserver_Send")]
        public void Webserver_Send_NullEnvironmentConnectionNull_ShouldNotMakeCallToExecuteCommand()
        {
            //------------Setup for test--------------------------
            var mockResourceModel = new Mock<IContextualResourceModel>();
            var mockEnvironment = new Mock<IEnvironmentModel>();
            var mockConnection = new Mock<IEnvironmentConnection>();
            mockConnection.Setup(connection => connection.WorkspaceID).Returns(Guid.NewGuid());
            mockConnection.Setup(connection => connection.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Verifiable();
            mockConnection.Setup(connection => connection.IsConnected).Returns(true);
            mockEnvironment.Setup(model => model.IsConnected).Returns(false);
            mockResourceModel.Setup(model => model.Environment).Returns(mockEnvironment.Object);
            //------------Execute Test---------------------------
            WebServer.Send(WebServerMethod.POST, null, "DataPayLoad", AsyncWorkerTests.CreateSynchronousAsyncWorker().Object);
            //------------Assert Results-------------------------
            mockConnection.Verify(connection => connection.ExecuteCommand(It.IsAny<StringBuilder>(), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never());
        }
    }
}
