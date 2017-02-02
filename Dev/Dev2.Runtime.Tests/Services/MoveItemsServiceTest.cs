/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Hosting;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Common.Interfaces.Security;
using Dev2.Communication;
using Dev2.Explorer;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Services.Security;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    // ReSharper disable InconsistentNaming
    public class MoveItemsServiceTest
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("GetResourceID")]
        public void GetResourceID_ShouldReturnEmptyGuid()
        {
            //------------Setup for test--------------------------
            var itemService = new MoveItemService();

            //------------Execute Test---------------------------
            var resId = itemService.GetResourceID(new Dictionary<string, StringBuilder>());
            //------------Assert Results-------------------------
            Assert.AreEqual(Guid.Empty, resId);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("GetResourceID")]
        public void GetAuthorizationContextForService_ShouldReturnContext()
        {
            //------------Setup for test--------------------------
            var itemService = new MoveItemService();

            //------------Execute Test---------------------------
            var resId = itemService.GetAuthorizationContextForService();
            //------------Assert Results-------------------------
            Assert.AreEqual(AuthorizationContext.Contribute, resId);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("MoveItem_HandlesType")]
        public void MoveItem_HandlesType_ExpectName()
        {
            //------------Setup for test--------------------------
            var MoveItemService = new MoveItemService();


            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.AreEqual("MoveItemService", MoveItemService.HandlesType());
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("MoveItemService_Execute")]
        public void MoveItem_Execute_NullValues_ErrorResult()
        {
            //------------Setup for test--------------------------
            var MoveItemService = new MoveItemService();
            var serializer = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            StringBuilder jsonResult = MoveItemService.Execute(null, null);
            IExplorerRepositoryResult result = serializer.Deserialize<IExplorerRepositoryResult>(jsonResult);
            //------------Assert Results-------------------------
            Assert.AreEqual(ExecStatus.Fail, result.Status);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("MoveItemService_Execute")]
        public void MoveItem_Execute_ItemToRenameNotInValues_ErrorResult()
        {
            //------------Setup for test--------------------------
            var values = new Dictionary<string, StringBuilder> { { "item", new StringBuilder() } };
            var MoveItemService = new MoveItemService();
            var serializer = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            StringBuilder jsonResult = MoveItemService.Execute(values, null);
            IExplorerRepositoryResult result = serializer.Deserialize<IExplorerRepositoryResult>(jsonResult);
            //------------Assert Results-------------------------
            Assert.AreEqual(ExecStatus.Fail, result.Status);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("MoveItemService_Execute")]
        public void MoveItem_Execute_NewNameNotInDictionary_ErrorResult()
        {
            //------------Setup for test--------------------------
            var values = new Dictionary<string, StringBuilder> { { "itemToRename", new StringBuilder("This is not deserializable to ServerExplorerItem") } };
            var MoveItemService = new MoveItemService();
            var serializer = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            StringBuilder jsonResult = MoveItemService.Execute(values, null);
            IExplorerRepositoryResult result = serializer.Deserialize<IExplorerRepositoryResult>(jsonResult);
            //------------Assert Results-------------------------
            Assert.AreEqual(ExecStatus.Fail, result.Status);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("MoveItemervice_Execute")]
        public void MoveItem_Execute_ItemToRenameNotServerExplorerItem_ErrorResult()
        {
            //------------Setup for test--------------------------
            var values = new Dictionary<string, StringBuilder> { { "itemToRename", new StringBuilder("This is not deserializable to ServerExplorerItem") }, { "newName", new StringBuilder("new name") } };
            var MoveItemService = new MoveItemService();
            var serializer = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            StringBuilder jsonResult = MoveItemService.Execute(values, null);
            IExplorerRepositoryResult result = serializer.Deserialize<IExplorerRepositoryResult>(jsonResult);
            //------------Assert Results-------------------------
            Assert.AreEqual(ExecStatus.Fail, result.Status);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("MoveItem_Execute")]
        public void MoveItem_Execute_ExpectRename()
        {
            //------------Setup for test--------------------------
            var moveItemService = new MoveItemService();

            ServerExplorerItem item = new ServerExplorerItem("a", Guid.NewGuid(), "Folder", null, Permissions.DeployFrom, "");
            var repo = new Mock<IExplorerServerResourceRepository>();
            var ws = new Mock<IWorkspace>();
            repo.Setup(a => a.MoveItem(It.IsAny<IExplorerItem>(), It.IsAny<string>(), It.IsAny<Guid>())).Returns(new ExplorerRepositoryResult(ExecStatus.Success, "")).Verifiable();

            var inputs = new Dictionary<string, StringBuilder>
                {
                    {
                        "itemToMove", new StringBuilder(item.ResourceId.ToString())
                    },
                    {
                        "newPath", new StringBuilder("bob")
                    },
                    {
                        "itemToBeRenamedPath",new StringBuilder(item.ResourcePath)
                    }
                };
            ws.Setup(a => a.ID).Returns(Guid.Empty);
            repo.Setup(repository => repository.Find(It.IsAny<Guid>())).Returns(item);
            moveItemService.ServerExplorerRepo = repo.Object;
            //------------Execute Test---------------------------
            moveItemService.Execute(inputs, ws.Object);
            //------------Assert Results-------------------------
            repo.Verify(a => a.MoveItem(It.IsAny<IExplorerItem>(), It.IsAny<string>(), It.IsAny<Guid>()));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("MoveItem_HandlesType")]
        public void MoveItem_CreateServiceEntry_ExpectProperlyFormedDynamicService()
        {
            //------------Setup for test--------------------------
            var MoveItem = new MoveItemService();


            //------------Execute Test---------------------------
            var a = MoveItem.CreateServiceEntry();
            //------------Assert Results-------------------------
            var b = a.DataListSpecification.ToString();
            Assert.AreEqual("<DataList><itemToMove ColumnIODirection=\"Input\"/><newPath ColumnIODirection=\"Input\"/><itemToBeRenamedPath ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>", b);
        }
    }
}
