using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Infrastructure.SharedModels;
using Dev2.Common.Interfaces.Studio.Core;
using Dev2.Common.Interfaces.Studio.Core.Controller;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Studio.ServerProxyLayer;

namespace ServerProxyLayerTests
{
    [TestClass]
    public class QueryManagerProxyTests
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("QueryManagerProxy_Ctor")]
        // ReSharper disable InconsistentNaming
        public void QueryManagerProxy_Ctor_ValidArgs_Constructs()

        {
            //------------Setup for test--------------------------

            var factory = new Mock<ICommunicationControllerFactory>();
            var connection = new Mock<IEnvironmentConnection>();

            //------------Setup for test--------------------------
            // ReSharper disable ObjectCreationAsStatement
            new QueryManagerProxy(factory.Object, connection.Object);
            // ReSharper restore ObjectCreationAsStatement

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("QueryManagerProxy_FetchDependants")]
        public void QueryManagerProxy_FetchDependants_AssertValidCalls()
        {
            //------------Setup for test--------------------------

            var factory = new Mock<ICommunicationControllerFactory>();
            var connection = new Mock<IEnvironmentConnection>();
            var controller = new Mock<ICommunicationController>();
            var items = new List<IExplorerItem>();
            //------------Setup for test--------------------------
            var queryProxy = new QueryManagerProxy(factory.Object, connection.Object);
            // ReSharper disable MaximumChainedReferences
            controller.Setup(a => a.ExecuteCommand<IList<IExplorerItem>>(connection.Object, GlobalConstants.ServerWorkspaceID)).Returns(items);
            // ReSharper restore MaximumChainedReferences
            factory.Setup(a => a.CreateController("FindDependencyService")).Returns(controller.Object);
            var resourceId = Guid.NewGuid();
            //------------Execute Test---------------------------
            var res = queryProxy.FetchDependants(resourceId);
            //------------Assert Results-------------------------
            controller.Verify(a => a.AddPayloadArgument("ResourceId", resourceId.ToString()), Times.Once());
            factory.Verify(a => a.CreateController("FindDependencyService"), Times.Once());
            controller.Verify(a => a.ExecuteCommand<IList<IExplorerItem>>(connection.Object, GlobalConstants.ServerWorkspaceID));
            Assert.AreEqual(items, res);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("QueryManagerProxy_FetchTables")]
        public void QueryManagerProxy_FetchTables_AssertValidCalls()
        {
            //------------Setup for test--------------------------

            var factory = new Mock<ICommunicationControllerFactory>();
            var connection = new Mock<IEnvironmentConnection>();
            var controller = new Mock<ICommunicationController>();
            var items = new List<IDbTable>();
            //------------Setup for test--------------------------
            var queryProxy = new QueryManagerProxy(factory.Object, connection.Object);
            // ReSharper disable MaximumChainedReferences
            controller.Setup(a => a.ExecuteCommand<IList<IDbTable>>(connection.Object, GlobalConstants.ServerWorkspaceID)).Returns(items);
            // ReSharper restore MaximumChainedReferences
            factory.Setup(a => a.CreateController("FetchTablesService")).Returns(controller.Object);
            var resourceId = Guid.NewGuid();
            //------------Execute Test---------------------------
            var res = queryProxy.FetchTables(resourceId);
            //------------Assert Results-------------------------
            controller.Verify(a => a.AddPayloadArgument("ResourceId", resourceId.ToString()), Times.Once());
            factory.Verify(a => a.CreateController("FetchTablesService"), Times.Once());
            controller.Verify(a => a.ExecuteCommand<IList<IDbTable>>(connection.Object, GlobalConstants.ServerWorkspaceID));
            Assert.AreEqual(items, res);
        }
        
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("QueryManagerProxy_FetchResource")]
        public void QueryManagerProxy_FetchResource_AssertValidCalls()
        {
            //------------Setup for test--------------------------

            var factory = new Mock<ICommunicationControllerFactory>();
            var connection = new Mock<IEnvironmentConnection>();
            var controller = new Mock<ICommunicationController>();
            var item = new StringBuilder();
            //------------Setup for test--------------------------
            var queryProxy = new QueryManagerProxy(factory.Object, connection.Object);
            // ReSharper disable MaximumChainedReferences
            controller.Setup(a => a.ExecuteCommand<StringBuilder>(connection.Object, GlobalConstants.ServerWorkspaceID)).Returns(item);
            // ReSharper restore MaximumChainedReferences
            factory.Setup(a => a.CreateController("FetchResourceDefinitionService")).Returns(controller.Object);
            var resourceId = Guid.NewGuid();
            //------------Execute Test---------------------------
            var res = queryProxy.FetchResourceXaml(resourceId);
            //------------Assert Results-------------------------
            controller.Verify(a => a.AddPayloadArgument("ResourceId", resourceId.ToString()), Times.Once());
            factory.Verify(a => a.CreateController("FetchResourceDefinitionService"), Times.Once());
            controller.Verify(a => a.ExecuteCommand<StringBuilder>(connection.Object, GlobalConstants.ServerWorkspaceID));
            Assert.AreEqual(item, res);
        }

        // ReSharper restore InconsistentNaming
    }
}