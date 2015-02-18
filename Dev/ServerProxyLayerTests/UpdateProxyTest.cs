using System;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Infrastructure.Communication;
using Dev2.Common.Interfaces.Studio.Core;
using Dev2.Common.Interfaces.Studio.Core.Controller;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Studio.ServerProxyLayer;

namespace ServerProxyLayerTests
{
    [TestClass]
    public class UpdateProxyTest
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("UpdateProxy_SaveResource")]
        // ReSharper disable InconsistentNaming
        public void UpdateProxyProxy_SaveResourceService()

        {
            //------------Setup for test--------------------------
            var factory = new Mock<ICommunicationControllerFactory>();
            var connection = new Mock<IEnvironmentConnection>();
            var controller = new Mock<ICommunicationController>();
            var manager = new UpdateProxy(factory.Object, connection.Object);
            StringBuilder res = new StringBuilder();
            // ReSharper disable MaximumChainedReferences
            controller.Setup(a => a.ExecuteCommand<IExecuteMessage>(connection.Object, GlobalConstants.ServerWorkspaceID)).Returns(new Mock<IExecuteMessage>().Object);
            controller.Setup(a => a.AddPayloadArgument("ResourceXml", res));
            // ReSharper restore MaximumChainedReferences
            factory.Setup(a => a.CreateController("SaveResourceService")).Returns(controller.Object);
            manager.SaveResource(res,Guid.Empty);
            controller.Verify(a => a.ExecuteCommand<IExecuteMessage>(connection.Object, GlobalConstants.ServerWorkspaceID), Times.Once());
            controller.Verify(a => a.AddPayloadArgument("ResourceXml", res), Times.Once());

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("UpdateProxy_SaveResource")]
        public void UpdateProxy_DeployItem()
        {
            //------------Setup for test--------------------------
            var factory = new Mock<ICommunicationControllerFactory>();
            var connection = new Mock<IEnvironmentConnection>();
            var controller = new Mock<ICommunicationController>();
            var manager = new UpdateProxy(factory.Object, connection.Object);
            StringBuilder res = new StringBuilder();
            // ReSharper disable MaximumChainedReferences
            controller.Setup(a => a.ExecuteCommand<IExecuteMessage>(connection.Object, GlobalConstants.ServerWorkspaceID)).Returns(new Mock<IExecuteMessage>().Object);
            controller.Setup(a => a.AddPayloadArgument("ResourceXml", res));
            // ReSharper restore MaximumChainedReferences
            factory.Setup(a => a.CreateController("SaveResourceService")).Returns(controller.Object);
            manager.SaveResource(res, Guid.Empty);
            controller.Verify(a => a.ExecuteCommand<IExecuteMessage>(connection.Object, GlobalConstants.ServerWorkspaceID), Times.Once());
            controller.Verify(a => a.AddPayloadArgument("ResourceXml", res), Times.Once());

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("UpdateProxy_TestDbConnection")]
        public void UpdateProxy_TestDbConnection()
        {
            //------------Setup for test--------------------------
            var factory = new Mock<ICommunicationControllerFactory>();
            var connection = new Mock<IEnvironmentConnection>();
            var controller = new Mock<ICommunicationController>();
            var manager = new UpdateProxy(factory.Object, connection.Object);
            StringBuilder res = new StringBuilder();
            // ReSharper disable MaximumChainedReferences
            controller.Setup(a => a.ExecuteCommand<IExecuteMessage>(connection.Object, GlobalConstants.ServerWorkspaceID)).Returns(new Mock<IExecuteMessage>().Object);
            controller.Setup(a => a.AddPayloadArgument("DbSource", res));
            // ReSharper restore MaximumChainedReferences
            factory.Setup(a => a.CreateController("TestDbSourceService")).Returns(controller.Object);
            manager.TestDbConnection(new DbSourceDefinition());
            controller.Verify(a => a.ExecuteCommand<IExecuteMessage>(connection.Object, GlobalConstants.ServerWorkspaceID), Times.Once());
            controller.Verify(a => a.AddPayloadArgument("DbSource", It.IsAny<StringBuilder>()), Times.Once());

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("UpdateProxy_TestDbConnection")]
        [ExpectedException(typeof(WarewolfTestException))]
        public void UpdateProxy_TestDbConnection_nullReturned()
        {
            //------------Setup for test--------------------------
            var factory = new Mock<ICommunicationControllerFactory>();
            var connection = new Mock<IEnvironmentConnection>();
            var controller = new Mock<ICommunicationController>();
            var manager = new UpdateProxy(factory.Object, connection.Object);
            StringBuilder res = new StringBuilder();
            // ReSharper disable MaximumChainedReferences
            controller.Setup(a => a.ExecuteCommand<IExecuteMessage>(connection.Object, GlobalConstants.ServerWorkspaceID)).Returns((IExecuteMessage)null);
            controller.Setup(a => a.AddPayloadArgument("DbSource", res));
            // ReSharper restore MaximumChainedReferences
            factory.Setup(a => a.CreateController("TestDbSourceService")).Returns(controller.Object);
            manager.TestDbConnection(new DbSourceDefinition());
            controller.Verify(a => a.ExecuteCommand<IExecuteMessage>(connection.Object, GlobalConstants.ServerWorkspaceID), Times.Once());
            controller.Verify(a => a.AddPayloadArgument("DbSource", It.IsAny<StringBuilder>()), Times.Once());

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("UpdateProxy_TestDbConnection")]
        [ExpectedException(typeof(WarewolfTestException))]
        public void UpdateProxy_TestDbConnection_HasError()
        {
            //------------Setup for test--------------------------
            var factory = new Mock<ICommunicationControllerFactory>();
            var connection = new Mock<IEnvironmentConnection>();
            var controller = new Mock<ICommunicationController>();
            var manager = new UpdateProxy(factory.Object, connection.Object);
            var msg = new Mock<IExecuteMessage>();
            msg.Setup(a => a.Message).Returns(new StringBuilder("Test failes"));
            msg.Setup(a => a.HasError).Returns(true);
            StringBuilder res = new StringBuilder();
            // ReSharper disable MaximumChainedReferences
            controller.Setup(a => a.ExecuteCommand<IExecuteMessage>(connection.Object, GlobalConstants.ServerWorkspaceID)).Returns(msg.Object);
            controller.Setup(a => a.AddPayloadArgument("DbSource", res));
            // ReSharper restore MaximumChainedReferences
            factory.Setup(a => a.CreateController("TestDbSourceService")).Returns(controller.Object);
            manager.TestDbConnection(new DbSourceDefinition());
            controller.Verify(a => a.ExecuteCommand<IExecuteMessage>(connection.Object, GlobalConstants.ServerWorkspaceID), Times.Once());
            controller.Verify(a => a.AddPayloadArgument("DbSource", It.IsAny<StringBuilder>()), Times.Once());

        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("UpdateProxy_TestDbConnection")]
        public void UpdateProxy_SaveDbConnection()
        {
            //------------Setup for test--------------------------
            var factory = new Mock<ICommunicationControllerFactory>();
            var connection = new Mock<IEnvironmentConnection>();
            var controller = new Mock<ICommunicationController>();
            var manager = new UpdateProxy(factory.Object, connection.Object);
            StringBuilder res = new StringBuilder();
            // ReSharper disable MaximumChainedReferences
            controller.Setup(a => a.ExecuteCommand<IExecuteMessage>(connection.Object, GlobalConstants.ServerWorkspaceID)).Returns(new Mock<IExecuteMessage>().Object);
            controller.Setup(a => a.AddPayloadArgument("DbSource", res));
            // ReSharper restore MaximumChainedReferences
            factory.Setup(a => a.CreateController("SaveDbSourceService")).Returns(controller.Object);
            manager.SaveDbSource(new DbSourceDefinition(),Guid.Empty);
            controller.Verify(a => a.ExecuteCommand<IExecuteMessage>(connection.Object, GlobalConstants.ServerWorkspaceID), Times.Once());
            controller.Verify(a => a.AddPayloadArgument("DbSource", It.IsAny<StringBuilder>()), Times.Once());

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("UpdateProxy_TestDbConnection")]
        [ExpectedException(typeof(WarewolfSaveException))]
        public void UpdateProxy_SaveDbConnection_hasError()
        {
            //------------Setup for test--------------------------
            var factory = new Mock<ICommunicationControllerFactory>();
            var connection = new Mock<IEnvironmentConnection>();
            var controller = new Mock<ICommunicationController>();
            var manager = new UpdateProxy(factory.Object, connection.Object);
            var msg = new Mock<IExecuteMessage>();
            msg.Setup(a => a.Message).Returns(new StringBuilder("Test failes"));
            msg.Setup(a => a.HasError).Returns(true);
            StringBuilder res = new StringBuilder();
            // ReSharper disable MaximumChainedReferences
            controller.Setup(a => a.ExecuteCommand<IExecuteMessage>(connection.Object, GlobalConstants.ServerWorkspaceID)).Returns(msg.Object);
            controller.Setup(a => a.AddPayloadArgument("DbSource", res));
            // ReSharper restore MaximumChainedReferences
            factory.Setup(a => a.CreateController("SaveDbSourceService")).Returns(controller.Object);
            manager.SaveDbSource(new DbSourceDefinition(), Guid.Empty);
            controller.Verify(a => a.ExecuteCommand<IExecuteMessage>(connection.Object, GlobalConstants.ServerWorkspaceID), Times.Once());
            controller.Verify(a => a.AddPayloadArgument("DbSource", It.IsAny<StringBuilder>()), Times.Once());

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("UpdateProxy_TestDbConnection")]
        public void UpdateProxy_SaveServerConnection()
        {
            //------------Setup for test--------------------------
            var factory = new Mock<ICommunicationControllerFactory>();
            var connection = new Mock<IEnvironmentConnection>();
            var controller = new Mock<ICommunicationController>();
            var manager = new UpdateProxy(factory.Object, connection.Object);
            StringBuilder res = new StringBuilder();
            // ReSharper disable MaximumChainedReferences
            controller.Setup(a => a.ExecuteCommand<IExecuteMessage>(connection.Object, GlobalConstants.ServerWorkspaceID)).Returns(new Mock<IExecuteMessage>().Object);
            controller.Setup(a => a.AddPayloadArgument("ServerSource", res));
            // ReSharper restore MaximumChainedReferences
            factory.Setup(a => a.CreateController("SaveServerSourceService")).Returns(controller.Object);
            manager.SaveServerSource(new ServerSource(), Guid.Empty);
            controller.Verify(a => a.ExecuteCommand<IExecuteMessage>(connection.Object, GlobalConstants.ServerWorkspaceID), Times.Once());
            controller.Verify(a => a.AddPayloadArgument("ServerSource", It.IsAny<StringBuilder>()), Times.Once());

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("UpdateProxy_TestDbConnection")]
        public void UpdateProxy_SaveServerConnectionTest()
        {
            //------------Setup for test--------------------------
            var factory = new Mock<ICommunicationControllerFactory>();
            var connection = new Mock<IEnvironmentConnection>();
            var controller = new Mock<ICommunicationController>();
            var manager = new UpdateProxy(factory.Object, connection.Object);
            StringBuilder res = new StringBuilder();
            var msg = new Mock<IExecuteMessage>();
            msg.Setup(a => a.Message).Returns( new StringBuilder( "Test Passed"));
            // ReSharper disable MaximumChainedReferences
            controller.Setup(a => a.ExecuteCommand<IExecuteMessage>(connection.Object, GlobalConstants.ServerWorkspaceID)).Returns(msg.Object);
            controller.Setup(a => a.AddPayloadArgument("ServerSource", res));
            // ReSharper restore MaximumChainedReferences
            factory.Setup(a => a.CreateController("TestConnectionService")).Returns(controller.Object);
            manager.TestConnection(new ServerSource());
            controller.Verify(a => a.ExecuteCommand<IExecuteMessage>(connection.Object, GlobalConstants.ServerWorkspaceID), Times.Once());
            controller.Verify(a => a.AddPayloadArgument("ServerSource", It.IsAny<StringBuilder>()), Times.Once());

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("UpdateProxy_TestDbConnection")]
        public void UpdateProxy_SaveServerConnectionTestFailure()
        {
            //------------Setup for test--------------------------
            var factory = new Mock<ICommunicationControllerFactory>();
            var connection = new Mock<IEnvironmentConnection>();
            var controller = new Mock<ICommunicationController>();
            var manager = new UpdateProxy(factory.Object, connection.Object);
            StringBuilder res = new StringBuilder();
            var msg = new Mock<IExecuteMessage>();
            msg.Setup(a => a.Message).Returns(new StringBuilder("Test failes"));
            msg.Setup(a => a.HasError).Returns(true);
            // ReSharper disable MaximumChainedReferences
            controller.Setup(a => a.ExecuteCommand<IExecuteMessage>(connection.Object, GlobalConstants.ServerWorkspaceID)).Returns(msg.Object);
            controller.Setup(a => a.AddPayloadArgument("ServerSource", res));
            // ReSharper restore MaximumChainedReferences
            factory.Setup(a => a.CreateController("TestConnectionService")).Returns(controller.Object);
            var output = manager.TestConnection(new ServerSource());
            controller.Verify(a => a.ExecuteCommand<IExecuteMessage>(connection.Object, GlobalConstants.ServerWorkspaceID), Times.Once());
            controller.Verify(a => a.AddPayloadArgument("ServerSource", It.IsAny<StringBuilder>()), Times.Once());
            Assert.AreEqual(output, "Test failes");

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("UpdateProxy_TestDbConnection")]
        public void UpdateProxy_SaveServerConnectionTestFailureNullReturned()
        {
            //------------Setup for test--------------------------
            var factory = new Mock<ICommunicationControllerFactory>();
            var connection = new Mock<IEnvironmentConnection>();
            var controller = new Mock<ICommunicationController>();
            var manager = new UpdateProxy(factory.Object, connection.Object);
            StringBuilder res = new StringBuilder();
            var msg = new Mock<IExecuteMessage>();
            msg.Setup(a => a.Message).Returns(new StringBuilder("Test failes"));
            msg.Setup(a => a.HasError).Returns(true);
            // ReSharper disable MaximumChainedReferences
            controller.Setup(a => a.ExecuteCommand<IExecuteMessage>(connection.Object, GlobalConstants.ServerWorkspaceID)).Returns((IExecuteMessage)null);
            controller.Setup(a => a.AddPayloadArgument("ServerSource", res));
            // ReSharper restore MaximumChainedReferences
            factory.Setup(a => a.CreateController("TestConnectionService")).Returns(controller.Object);
            var output = manager.TestConnection(new ServerSource());
            controller.Verify(a => a.ExecuteCommand<IExecuteMessage>(connection.Object, GlobalConstants.ServerWorkspaceID), Times.Once());
            controller.Verify(a => a.AddPayloadArgument("ServerSource", It.IsAny<StringBuilder>()), Times.Once());
            Assert.AreEqual(output, "Unable to contact server");

        }
        // ReSharper restore InconsistentNaming

    }
}
