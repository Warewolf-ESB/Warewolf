
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Common.Interfaces.Security;
using Dev2.Controller;
using Dev2.Explorer;
using Dev2.Models;
using Dev2.Studio.Core.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

// ReSharper disable InconsistentNaming
namespace Dev2.Core.Tests
{
    [TestClass]
    public class ClientExplorerRepositoryTest
    {



        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ClientExplorerRepository_Ctor")]
        public void ClientExplorerRepository_ClientExplorerRepository_Ctor_ExpectConnectionIsSet()
        {
            //------------Setup for test--------------------------


            //------------Execute Test---------------------------
            var env = new Mock<IEnvironmentConnection>();
            var comFactory = new Mock<ICommunicationControllerFactory>();
            var rep = new ServerExplorerClientProxy(env.Object, comFactory.Object);
            //------------Assert Results-------------------------

            Assert.AreEqual(rep.Connection, env.Object);
            Assert.AreEqual(rep.CommunicationControllerFactory, comFactory.Object);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ClientExplorerRepository_CreateStudioExplorerItems")]
        public void ClientExplorerRepository_ClientExplorerRepository_ExpectLoadCalled_ExpectExecCalled()
        {
            //------------Setup for test--------------------------
            var env = new Mock<IEnvironmentConnection>();
            var comFactory = new Mock<ICommunicationControllerFactory>();
            var rep = new ServerExplorerClientProxy(env.Object, comFactory.Object);
            var com = new Mock<ICommunicationController>();
            var item = new Mock<IExplorerItem>();
            // ReSharper disable MaximumChainedReferences
            comFactory.Setup(a => a.CreateController("FetchExplorerItemsService")).Returns(com.Object).Verifiable();
     
            com.Setup(a => a.ExecuteCommand<IExplorerItem>(env.Object, Guid.Empty)).Returns(item.Object).Verifiable();

            //------------Execute Test---------------------------
            rep.Load(Guid.Empty);
            //------------Assert Results-------------------------

            comFactory.Verify(a => a.CreateController("FetchExplorerItemsService"));
            com.Verify(a => a.ExecuteCommand<IExplorerItem>(env.Object, Guid.Empty));
        }




        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ClientExplorerRepository_CreateFolder")]
        public void ClientExplorerRepository_CreateFolder_ExpectCreateServiceCalled()
        {
            //------------Setup for test--------------------------
            var env = new Mock<IEnvironmentConnection>();
            var comFactory = new Mock<ICommunicationControllerFactory>();
            var rep = new ServerExplorerClientProxy(env.Object, comFactory.Object);
            var com = new Mock<ICommunicationController>();
            var item = new ServerExplorerItem("", Guid.Empty, ResourceType.DbService, null, Permissions.Contribute, "f");
            comFactory.Setup(a => a.CreateController("AddFolderService")).Returns(com.Object).Verifiable();
            com.Setup(a => a.ExecuteCommand<IExplorerItem>(env.Object, Guid.Empty)).Returns(item).Verifiable();

            //------------Execute Test---------------------------
            rep.AddItem(item, Guid.Empty);
            //------------Assert Results-------------------------

            comFactory.Verify(a => a.CreateController("AddFolderService"));
            com.Verify(a => a.ExecuteCommand<IExplorerRepositoryResult>(env.Object, Guid.Empty));
            com.Verify(a => a.AddPayloadArgument("itemToAdd", It.IsAny<string>()));
            com.Verify(a => a.ExecuteCommand<IExplorerRepositoryResult>(env.Object, Guid.Empty));

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ClientExplorerRepository_RenameItem")]
        public void ClientExplorerRepository_RenameItem_ExpectRenameServiceCalled()
        {
            //------------Setup for test--------------------------
            var env = new Mock<IEnvironmentConnection>();
            var comFactory = new Mock<ICommunicationControllerFactory>();
            var rep = new ServerExplorerClientProxy(env.Object, comFactory.Object);
            var com = new Mock<ICommunicationController>();
            var item = new ServerExplorerItem("", Guid.Empty, ResourceType.DbService, null, Permissions.Contribute, "f");
            comFactory.Setup(a => a.CreateController("RenameItemService")).Returns(com.Object).Verifiable();
            com.Setup(a => a.ExecuteCommand<IExplorerItem>(env.Object, Guid.Empty)).Returns(item).Verifiable();

            //------------Execute Test---------------------------
            rep.RenameItem(item, "bob", Guid.Empty);
            //------------Assert Results-------------------------

            comFactory.Verify(a => a.CreateController("RenameItemService"));
            com.Verify(a => a.ExecuteCommand<IExplorerRepositoryResult>(env.Object, Guid.Empty));
            com.Verify(a => a.AddPayloadArgument("itemToRename", It.IsAny<string>()));
            com.Verify(a => a.AddPayloadArgument("newName", "bob"));
            com.Verify(a => a.ExecuteCommand<IExplorerRepositoryResult>(env.Object, Guid.Empty));

        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ClientExplorerRepository_MoveItem")]
        public void ClientExplorerRepository_MoveItem_ExpectMoveServiceCalled()
        {
            //------------Setup for test--------------------------
            var env = new Mock<IEnvironmentConnection>();
            var comFactory = new Mock<ICommunicationControllerFactory>();
            var rep = new ServerExplorerClientProxy(env.Object, comFactory.Object);
            var com = new Mock<ICommunicationController>();
            var item = new ServerExplorerItem("", Guid.Empty, ResourceType.DbService, null, Permissions.Contribute, "f");
            comFactory.Setup(a => a.CreateController("MoveItemService")).Returns(com.Object).Verifiable();
            com.Setup(a => a.ExecuteCommand<IExplorerItem>(env.Object, Guid.Empty)).Returns(item).Verifiable();

            //------------Execute Test---------------------------
            rep.MoveItem(item, "bob", Guid.Empty);
            //------------Assert Results-------------------------

            comFactory.Verify(a => a.CreateController("MoveItemService"));
            com.Verify(a => a.ExecuteCommand<IExplorerRepositoryResult>(env.Object, Guid.Empty));
            com.Verify(a => a.AddPayloadArgument("itemToMove", It.IsAny<string>()));
            com.Verify(a => a.AddPayloadArgument("newPath", "bob"));
            com.Verify(a => a.ExecuteCommand<IExplorerRepositoryResult>(env.Object, Guid.Empty));

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ClientExplorerRepository_MoveItem")]
        public void ClientExplorerRepository_GetServerVersion_ExpectCorrectServiceCalled()
        {
            //------------Setup for test--------------------------
            var env = new Mock<IEnvironmentConnection>();
            var comFactory = new Mock<ICommunicationControllerFactory>();
            var rep = new ServerExplorerClientProxy(env.Object, comFactory.Object);
            var com = new Mock<ICommunicationController>();
           comFactory.Setup(a => a.CreateController("GetServerVersion")).Returns(com.Object).Verifiable();
            com.Setup(a => a.ExecuteCommand<string>(env.Object, Guid.Empty)).Returns("1,2,3,4").Verifiable();

            //------------Execute Test---------------------------
            Assert.AreEqual("1,2,3,4",rep.GetServerVersion());
            //------------Assert Results-------------------------

            comFactory.Verify(a => a.CreateController("GetServerVersion"));
            com.Verify(a => a.ExecuteCommand<string>(env.Object, Guid.Empty));
            com.Verify(a => a.ExecuteCommand<string>(env.Object, Guid.Empty));

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ClientExplorerRepository_MoveItem")]
        public void ClientExplorerRepository_GetServerVersion_ExpectCorrectServiceCalled_defaultIfEmpty()
        {
            //------------Setup for test--------------------------
            var env = new Mock<IEnvironmentConnection>();
            var comFactory = new Mock<ICommunicationControllerFactory>();
            var rep = new ServerExplorerClientProxy(env.Object, comFactory.Object);
            var com = new Mock<ICommunicationController>();
            comFactory.Setup(a => a.CreateController("GetServerVersion")).Returns(com.Object).Verifiable();
            com.Setup(a => a.ExecuteCommand<string>(env.Object, Guid.Empty)).Returns("").Verifiable();

            //------------Execute Test---------------------------
            Assert.AreEqual("less than 0.4.19.1", rep.GetServerVersion());
            //------------Assert Results-------------------------

            comFactory.Verify(a => a.CreateController("GetServerVersion"));
            com.Verify(a => a.ExecuteCommand<string>(env.Object, Guid.Empty));
            com.Verify(a => a.ExecuteCommand<string>(env.Object, Guid.Empty));

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ClientExplorerRepository_DeleteItemItem")]
        public void ClientExplorerRepository_DeleteItemItem_ExpectDeleteServiceCalled()
        {
            //------------Setup for test--------------------------
            var env = new Mock<IEnvironmentConnection>();
            var comFactory = new Mock<ICommunicationControllerFactory>();
            var rep = new ServerExplorerClientProxy(env.Object, comFactory.Object);
            var com = new Mock<ICommunicationController>();
            var item = new ServerExplorerItem("", Guid.Empty, ResourceType.DbService, null, Permissions.Contribute, "f");
            comFactory.Setup(a => a.CreateController("DeleteItemService")).Returns(com.Object).Verifiable();
            com.Setup(a => a.ExecuteCommand<IExplorerItem>(env.Object, Guid.Empty)).Returns(item).Verifiable();

            //------------Execute Test---------------------------
            rep.DeleteItem(item, Guid.Empty);
            //------------Assert Results-------------------------

            comFactory.Verify(a => a.CreateController("DeleteItemService"));
            com.Verify(a => a.ExecuteCommand<IExplorerRepositoryResult>(env.Object, Guid.Empty));
            com.Verify(a => a.AddPayloadArgument("itemToDelete", It.IsAny<string>()));
            com.Verify(a => a.ExecuteCommand<IExplorerRepositoryResult>(env.Object, Guid.Empty));

        }

        // ReSharper restore MaximumChainedReferences
        
    }
}
