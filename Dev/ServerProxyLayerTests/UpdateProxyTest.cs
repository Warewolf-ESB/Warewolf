using System;
using System.Text;
using Dev2.Common;
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
        // ReSharper restore InconsistentNaming
    }
}
