using Dev2.Common;
using Dev2.Common.Interfaces.Studio.Core;
using Dev2.Common.Interfaces.Studio.Core.Controller;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

using Warewolf.Studio.ServerProxyLayer;

namespace ServerProxyLayerTests
{
    [TestClass]
    public class AdminProxyTests
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("AdminProxy_Ctor")]
        // ReSharper disable InconsistentNaming
        public void AdminProxy_Ctor_CallsBase_NoError()

        {
            //------------Setup for test--------------------------
            var factory = new Mock<ICommunicationControllerFactory>();
            var connection = new Mock<IEnvironmentConnection>();
            // ReSharper disable once ObjectCreationAsStatement
            new AdminManagerProxy(factory.Object,connection.Object);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("AdminProxy_QueueDepth")]
        public void AdminProxy_GetQueueDepth()
        {
            var output = CallProxyMethodWithNoParams("GetCurrentQueueDepthService").GetCurrentQueueDepth();
            Assert.AreEqual(-1,output);
        }



        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("AdminProxy_QueueDepth")]
        public void AdminProxy_GetMaxQueueDepth()
        {
            var output = CallProxyMethodWithNoParams("GetMaxQueueDepthService").GetMaxQueueDepth();
            Assert.AreEqual(-1, output);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("AdminProxy_ThreadCount")]
        public void AdminProxy_GetThreadCount()
        {
            var output = CallProxyMethodWithNoParams("GetMaxThreadCountService").GetMaxThreadCount();
            Assert.AreEqual(-1, output);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("AdminProxy_SetThreadCount")]
        public void AdminProxy_SetThreadCount()
        {
            //------------Setup for test--------------------------
            var factory = new Mock<ICommunicationControllerFactory>();
            var connection = new Mock<IEnvironmentConnection>();
            var controller = new Mock<ICommunicationController>();
            var manager = new AdminManagerProxy(factory.Object, connection.Object);
            // ReSharper disable MaximumChainedReferences
            controller.Setup(a => a.ExecuteCommand<string>(connection.Object, GlobalConstants.ServerWorkspaceID)).Returns("-1");
            controller.Setup(a => a.AddPayloadArgument("maxThreadCount", "-1"));
            // ReSharper restore MaximumChainedReferences
            factory.Setup(a => a.CreateController("SetMaxThreadCountService")).Returns(controller.Object);
            manager.SetMaxThreadCount(-1);
            controller.Verify(a => a.ExecuteCommand<string>(connection.Object, GlobalConstants.ServerWorkspaceID),Times.Once());
            controller.Verify(a => a.AddPayloadArgument("maxThreadCount", "-1"), Times.Once());

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("AdminProxy_SetMaxQueueDepth")]
        public void AdminProxy_SetMaxQueueDepth()
        {
            //------------Setup for test--------------------------
            var factory = new Mock<ICommunicationControllerFactory>();
            var connection = new Mock<IEnvironmentConnection>();
            var controller = new Mock<ICommunicationController>();
            var manager = new AdminManagerProxy(factory.Object, connection.Object);
            // ReSharper disable MaximumChainedReferences
            controller.Setup(a => a.ExecuteCommand<string>(connection.Object, GlobalConstants.ServerWorkspaceID)).Returns("-1");
            controller.Setup(a => a.AddPayloadArgument("maxDepth", "-1"));
            // ReSharper restore MaximumChainedReferences
            factory.Setup(a => a.CreateController("SetMaxQueueDepthService")).Returns(controller.Object);
            manager.SetMaxQueueDepth(-1);
            controller.Verify(a => a.ExecuteCommand<string>(connection.Object, GlobalConstants.ServerWorkspaceID), Times.Once());
            controller.Verify(a => a.AddPayloadArgument("maxDepth", "-1"), Times.Once());

        }


        #region helpers
        static AdminManagerProxy CallProxyMethodWithNoParams(string service)
        {
            //------------Setup for test--------------------------
            var factory = new Mock<ICommunicationControllerFactory>();
            var connection = new Mock<IEnvironmentConnection>();
            var controller = new Mock<ICommunicationController>();
            var manager = new AdminManagerProxy(factory.Object, connection.Object);
            // ReSharper disable MaximumChainedReferences
            controller.Setup(a => a.ExecuteCommand<int>(connection.Object, GlobalConstants.ServerWorkspaceID)).Returns(-1);
            // ReSharper restore MaximumChainedReferences
            factory.Setup(a => a.CreateController(service)).Returns(controller.Object);
            return manager;
        }


        #endregion

    }
    // ReSharper restore InconsistentNaming
}
