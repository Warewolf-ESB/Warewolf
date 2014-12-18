using System;
using System.Net;
using Dev2.Common;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Common.Interfaces.Studio.Core;
using Dev2.Common.Interfaces.Studio.Core.Controller;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Studio.ServerProxyLayer;

namespace ServerProxyLayerTests
{
    [TestClass]
    public class LogManagerTests
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("LogManager_SetLogMaxSize")]
        public void LogManager_SetLogMaxSize_CorrectServerCalls_ExpectSuccess()
        {
            //------------Setup for test--------------------------
            var factory = new Mock<ICommunicationControllerFactory>();
            var connection = new Mock<IEnvironmentConnection>();
            var controller = new Mock<ICommunicationController>();
            var manager = new LogManagerProxy(new NetworkCredential(), factory.Object, connection.Object,new Uri("http://bob"));

            // ReSharper disable MaximumChainedReferences
            controller.Setup(a => a.ExecuteCommand<IExplorerRepositoryResult>(connection.Object, GlobalConstants.ServerWorkspaceID));

            // ReSharper restore MaximumChainedReferences
            factory.Setup(a => a.CreateController("SetLogMaxSizeService")).Returns(controller.Object);
            manager.SetMaxLogSize(25);
            controller.Verify(a => a.ExecuteCommand<string>(connection.Object, GlobalConstants.ServerWorkspaceID), Times.Once());
            controller.Verify(a => a.AddPayloadArgument("sizeInMb", "25"), Times.Once());
            controller.Verify(a => a.ExecuteCommand<string>(connection.Object, GlobalConstants.ServerWorkspaceID));

                       
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("LogManager_Ctor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void LogManager_Ctor_NullUri()
        {
            //------------Setup for test--------------------------
            new LogManagerProxy(new NetworkCredential("a","b"),new Mock<ICommunicationControllerFactory>().Object,new Mock<IEnvironmentConnection>().Object,null );

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("LogManager_Ctor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void LogManager_Ctor_NullCredential()
        {
            //------------Setup for test--------------------------
            new LogManagerProxy(null, new Mock<ICommunicationControllerFactory>().Object, new Mock<IEnvironmentConnection>().Object, new Uri("http://bob"));

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }
    }
}