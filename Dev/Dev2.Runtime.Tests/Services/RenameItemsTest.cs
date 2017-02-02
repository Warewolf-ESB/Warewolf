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
    public class RenameItemsTest
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("GetResourceID")]
        public void GetResourceID_ShouldReturnEmptyGuid()
        {
            //------------Setup for test--------------------------
            var renameItemService = new RenameItemService();

            //------------Execute Test---------------------------
            var resId = renameItemService.GetResourceID(new Dictionary<string, StringBuilder>());
            //------------Assert Results-------------------------
            Assert.AreEqual(Guid.Empty, resId);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("GetResourceID")]
        public void GetAuthorizationContextForService_ShouldReturnContext()
        {
            //------------Setup for test--------------------------
            var renameItemService = new RenameItemService();

            //------------Execute Test---------------------------
            var resId = renameItemService.GetAuthorizationContextForService();
            //------------Assert Results-------------------------
            Assert.AreEqual(AuthorizationContext.Contribute, resId);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("RenameItem_HandlesType")]
        public void RenameItem_HandlesType_ExpectName()
        {
            //------------Setup for test--------------------------
            var renameItemService = new RenameItemService();


            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.AreEqual("RenameItemService", renameItemService.HandlesType());
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("RenameItemService_Execute")]
        public void RenameItem_Execute_NullValues_ErrorResult()
        {
            //------------Setup for test--------------------------
            var renameItemService = new RenameItemService();
            var serializer = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            StringBuilder jsonResult = renameItemService.Execute(null, null);
            IExplorerRepositoryResult result = serializer.Deserialize<IExplorerRepositoryResult>(jsonResult);
            //------------Assert Results-------------------------
            Assert.AreEqual(ExecStatus.Fail, result.Status);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("RenameItemService_Execute")]
        public void RenameItem_Execute_ItemToRenameNotInValues_ErrorResult()
        {
            //------------Setup for test--------------------------
            var values = new Dictionary<string, StringBuilder> { { "item", new StringBuilder() } };
            var renameItemService = new RenameItemService();
            var serializer = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            StringBuilder jsonResult = renameItemService.Execute(values, null);
            IExplorerRepositoryResult result = serializer.Deserialize<IExplorerRepositoryResult>(jsonResult);
            //------------Assert Results-------------------------
            Assert.AreEqual(ExecStatus.Fail, result.Status);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("RenameItemService_Execute")]
        public void RenameItem_Execute_NewNameNotInDictionary_ErrorResult()
        {
            //------------Setup for test--------------------------
            var values = new Dictionary<string, StringBuilder> { { "itemToRename", new StringBuilder("This is not deserializable to ServerExplorerItem") } };
            var renameItemService = new RenameItemService();
            var serializer = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            StringBuilder jsonResult = renameItemService.Execute(values, null);
            IExplorerRepositoryResult result = serializer.Deserialize<IExplorerRepositoryResult>(jsonResult);
            //------------Assert Results-------------------------
            Assert.AreEqual(ExecStatus.Fail, result.Status);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("RenameItemervice_Execute")]
        public void RenameItem_Execute_ItemToRenameNotServerExplorerItem_ErrorResult()
        {
            //------------Setup for test--------------------------
            var values = new Dictionary<string, StringBuilder> { { "itemToRename", new StringBuilder("This is not deserializable to ServerExplorerItem") }, { "newName", new StringBuilder("new name") } };
            var renameItemService = new RenameItemService();
            var serializer = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            StringBuilder jsonResult = renameItemService.Execute(values, null);
            IExplorerRepositoryResult result = serializer.Deserialize<IExplorerRepositoryResult>(jsonResult);
            //------------Assert Results-------------------------
            Assert.AreEqual(ExecStatus.Fail, result.Status);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("RenameItem_Execute")]
        public void RenameItem_Execute_ExpectRename()
        {
            //------------Setup for test--------------------------
            var renameItemService = new RenameItemService();

            ServerExplorerItem item = new ServerExplorerItem("a", Guid.NewGuid(), "Folder", null, Permissions.DeployFrom, "");
            var repo = new Mock<IExplorerServerResourceRepository>();
            var ws = new Mock<IWorkspace>();
            repo.Setup(a => a.RenameItem(It.IsAny<IExplorerItem>(), It.IsAny<string>(), It.IsAny<Guid>())).Returns(new ExplorerRepositoryResult(ExecStatus.Success, "")).Verifiable();

            var inputs = new Dictionary<string, StringBuilder>
                {
                    {
                        "itemToRename", new StringBuilder(item.ResourceId.ToString())
                    },
                    {
                        "newName", new StringBuilder("bob")
                    }
                };
            ws.Setup(a => a.ID).Returns(Guid.Empty);
            repo.Setup(repository => repository.Find(It.IsAny<Guid>())).Returns(item);
            renameItemService.ServerExplorerRepo = repo.Object;
            //------------Execute Test---------------------------
            renameItemService.Execute(inputs, ws.Object);
            //------------Assert Results-------------------------
            repo.Verify(a => a.RenameItem(It.IsAny<IExplorerItem>(), It.IsAny<string>(), It.IsAny<Guid>()));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("RenameItem_HandlesType")]
        public void RenameItem_CreateServiceEntry_ExpectProperlyFormedDynamicService()
        {
            //------------Setup for test--------------------------
            var renameItem = new RenameItemService();


            //------------Execute Test---------------------------
            var a = renameItem.CreateServiceEntry();
            //------------Assert Results-------------------------
            var b = a.DataListSpecification.ToString();
            Assert.AreEqual("<DataList><itemToRename ColumnIODirection=\"Input\"/><newName ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>", b);
        }
    }
}
