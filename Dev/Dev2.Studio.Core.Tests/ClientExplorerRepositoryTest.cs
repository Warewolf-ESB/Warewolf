using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Security;
using Dev2.Controller;
using Dev2.Explorer;
using Dev2.Interfaces;
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


        //[TestMethod]
        //[Owner("Leon Rajindrapersadh")]
        //[TestCategory("ClientExplorerRepository_CreateStudioExplorerItems")]
        //public void ClientExplorerRepository_CreateStudioExplorerItems_ConvertToClient_ExpectSuccessfulConversion()
        //{
        //    //------------Setup for test--------------------------
        //    var serverItem = new ServerExplorerItem("a", Guid.NewGuid(), ResourceType.DbSource, CreateChildren(3, 5),
        //                                        Permissions.Administrator, "");

        //    //------------Execute Test---------------------------
        //    var converted = ServerExplorerClientProxy.MapData(serverItem);
        //    //------------Assert Results-------------------------

        //    Assert.IsTrue(AssertTreeEquality(serverItem, converted));
        //}


        //        private bool AssertTreeEquality(IExplorerItem serverItem, ExplorerItemModel converted)
        //        {
        //            if(serverItem.DisplayName == converted.DisplayName && converted.Permissions == serverItem.Permissions
        //                && converted.ResourceId == serverItem.ResourceId && converted.ResourceType == serverItem.ResourceType
        //                && converted.ResourcePath == serverItem.ResourcePath)
        //            {
        //                if(serverItem.Children == null) return converted.Children == null;
        //                bool childrenEq = true;
        //                for(int i = 0; i < serverItem.Children.Count; i++)
        //                {
        //                    childrenEq &= AssertTreeEquality(serverItem.Children[i], converted.Children[i]);
        //                }
        //                return childrenEq;
        //            }
        //            return false;
        //        }

        //        private IList<IExplorerItem> CreateChildren(int depth, int width)
        //        {
        //           return Infinite()
        //                .Take(width)
        //                .Select(
        //                    a =>
        //// ReSharper disable SpecifyACultureInStringConversionExplicitly
        //                    new ServerExplorerItem(string.Format("{0}{1}", depth.ToString(), width.ToString()), Guid.NewGuid(),
        //// ReSharper restore SpecifyACultureInStringConversionExplicitly
        //                                            depth % 2 == 0 ? ResourceType.DbSource : ResourceType.PluginService, depth < 0 ? null :
        //                                           CreateChildren(depth - 1, width).ToList(), Permissions.DeployFrom,
        //                                           "bob" + depth + width) as IExplorerItem).ToList();
        //        }

        //        private static IEnumerable<int> Infinite()
        //        {
        //            int i = 0;
        //            while(true)
        //            {
        //                i++;
        //                yield return i;
        //            }
        //// ReSharper disable FunctionNeverReturns
        //        }
        //// ReSharper restore FunctionNeverReturns
    }
}
