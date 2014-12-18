using System;
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
    public class ExplorerUpdateManagerTests
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerUpdateProxy_Ctor")]
        // ReSharper disable InconsistentNaming
        public void ExplorerUpdateProxy_Ctor_Valid_ExpectCreatedItem()

        {
            //------------Setup for test--------------------------
            var factory = new Mock<ICommunicationControllerFactory>();
            var connection = new Mock<IEnvironmentConnection>();

            // ReSharper disable ObjectCreationAsStatement
            new ExplorerUpdateManagerProxy(factory.Object, connection.Object);
            // ReSharper restore ObjectCreationAsStatement

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerUpdateProxy_AddFolder")]
        public void ExplorerUpdateProxy_AddFolder_ExpectCorrectCallsToServer()
        {
            //------------Setup for test--------------------------
            var factory = new Mock<ICommunicationControllerFactory>();
            var connection = new Mock<IEnvironmentConnection>();
            var controller = new Mock<ICommunicationController>();
            var manager = new ExplorerUpdateManagerProxy(factory.Object, connection.Object);

            // ReSharper disable MaximumChainedReferences
            controller.Setup(a => a.ExecuteCommand<IExplorerRepositoryResult>(connection.Object, GlobalConstants.ServerWorkspaceID)).Returns(new Mock<IExplorerRepositoryResult>().Object);

            // ReSharper restore MaximumChainedReferences
            factory.Setup(a => a.CreateController("AddFolderService")).Returns(controller.Object);
            manager.AddFolder("bob\\moo");
            controller.Verify(a => a.ExecuteCommand<IExplorerRepositoryResult>(connection.Object, GlobalConstants.ServerWorkspaceID), Times.Once());
            controller.Verify(a => a.AddPayloadArgument("path", "bob\\moo"), Times.Once());
            controller.Verify(a => a.ExecuteCommand<IExplorerRepositoryResult>(connection.Object, GlobalConstants.ServerWorkspaceID));

        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerUpdateProxy_DeleteFolder")]
        public void ExplorerUpdateProxy_DeleteFolder_ExpectCorrectCallsToServer()
        {
            //------------Setup for test--------------------------
            var factory = new Mock<ICommunicationControllerFactory>();
            var connection = new Mock<IEnvironmentConnection>();
            var controller = new Mock<ICommunicationController>();
            var manager = new ExplorerUpdateManagerProxy(factory.Object, connection.Object);

            // ReSharper disable MaximumChainedReferences
            controller.Setup(a => a.ExecuteCommand<IExplorerRepositoryResult>(connection.Object, GlobalConstants.ServerWorkspaceID)).Returns(new Mock<IExplorerRepositoryResult>().Object);

            // ReSharper restore MaximumChainedReferences
            factory.Setup(a => a.CreateController("DeleteFolderService")).Returns(controller.Object);
            manager.DeleteFolder("bob\\moo");
            controller.Verify(a => a.ExecuteCommand<IExplorerRepositoryResult>(connection.Object, GlobalConstants.ServerWorkspaceID), Times.Once());
            controller.Verify(a => a.AddPayloadArgument("path", "bob\\moo"), Times.Once());
            controller.Verify(a => a.ExecuteCommand<IExplorerRepositoryResult>(connection.Object, GlobalConstants.ServerWorkspaceID));

        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerUpdateProxy_DeleteResource")]
        public void ExplorerUpdateProxy_DeleteResource_ExpectCorrectCallsToServer()
        {
            //------------Setup for test--------------------------
            var factory = new Mock<ICommunicationControllerFactory>();
            var connection = new Mock<IEnvironmentConnection>();
            var controller = new Mock<ICommunicationController>();
            var manager = new ExplorerUpdateManagerProxy(factory.Object, connection.Object);
            var id = Guid.NewGuid();
            // ReSharper disable MaximumChainedReferences
            controller.Setup(a => a.ExecuteCommand<IExplorerRepositoryResult>(connection.Object, GlobalConstants.ServerWorkspaceID)).Returns(new Mock<IExplorerRepositoryResult>().Object);

            // ReSharper restore MaximumChainedReferences
            factory.Setup(a => a.CreateController("DeleteItemService")).Returns(controller.Object);
            manager.DeleteResource(id);
            controller.Verify(a => a.ExecuteCommand<IExplorerRepositoryResult>(connection.Object, GlobalConstants.ServerWorkspaceID), Times.Once());
            controller.Verify(a => a.AddPayloadArgument("itemToDelete", id.ToString()), Times.Once());
            controller.Verify(a => a.ExecuteCommand<IExplorerRepositoryResult>(connection.Object, GlobalConstants.ServerWorkspaceID));

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerUpdateProxy_RenameResource")]
        public void ExplorerUpdateProxy_RenameResource_ExpectCorrectCallsToServer()
        {
            //------------Setup for test--------------------------
            var factory = new Mock<ICommunicationControllerFactory>();
            var connection = new Mock<IEnvironmentConnection>();
            var controller = new Mock<ICommunicationController>();
            var manager = new ExplorerUpdateManagerProxy(factory.Object, connection.Object);
            var id = Guid.NewGuid();
            // ReSharper disable MaximumChainedReferences
            controller.Setup(a => a.ExecuteCommand<IExplorerRepositoryResult>(connection.Object, GlobalConstants.ServerWorkspaceID)).Returns(new Mock<IExplorerRepositoryResult>().Object);

            // ReSharper restore MaximumChainedReferences
            factory.Setup(a => a.CreateController("RenameItemService")).Returns(controller.Object);
            manager.Rename(id,"bob");
            controller.Verify(a => a.ExecuteCommand<IExplorerRepositoryResult>(connection.Object, GlobalConstants.ServerWorkspaceID), Times.Once());
            controller.Verify(a => a.AddPayloadArgument("itemToRename", id.ToString()), Times.Once());
            controller.Verify(a => a.AddPayloadArgument("newName", "bob"), Times.Once());
            controller.Verify(a => a.ExecuteCommand<IExplorerRepositoryResult>(connection.Object, GlobalConstants.ServerWorkspaceID));

        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ExplorerUpdateProxy_MoveItem")]
        public void ExplorerUpdateProxy_MoveItem_ExpectCorrectCallsToServer()
        {
            //------------Setup for test--------------------------
            var factory = new Mock<ICommunicationControllerFactory>();
            var connection = new Mock<IEnvironmentConnection>();
            var controller = new Mock<ICommunicationController>();
            var manager = new ExplorerUpdateManagerProxy(factory.Object, connection.Object);
            var id = Guid.NewGuid();
            // ReSharper disable MaximumChainedReferences
            controller.Setup(a => a.ExecuteCommand<IExplorerRepositoryResult>(connection.Object, GlobalConstants.ServerWorkspaceID)).Returns(new Mock<IExplorerRepositoryResult>().Object);

            // ReSharper restore MaximumChainedReferences
            factory.Setup(a => a.CreateController("MoveItemService")).Returns(controller.Object);
            manager.MoveItem(id, "bob");
            controller.Verify(a => a.ExecuteCommand<IExplorerRepositoryResult>(connection.Object, GlobalConstants.ServerWorkspaceID), Times.Once());
            controller.Verify(a => a.AddPayloadArgument("itemToMove", id.ToString()), Times.Once());
            controller.Verify(a => a.AddPayloadArgument("newPath", "bob"), Times.Once());
            controller.Verify(a => a.ExecuteCommand<IExplorerRepositoryResult>(connection.Object, GlobalConstants.ServerWorkspaceID));

        }

    }            // ReSharper restore InconsistentNaming
}
