/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Runtime.ESB.Management;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

// ReSharper disable InconsistentNaming
namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class DeleteItemsTest
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DeleteItem_HandlesType")]
        public void DeleteItem_HandlesType_ExpectName()
        {
            //------------Setup for test--------------------------
            var deleteItem = new DeleteItemService();


            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.AreEqual("DeleteItemService", deleteItem.HandlesType());
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DeleteItemService_Execute")]
        public void DeleteItem_Execute_NullValues_ErrorResult()
        {
            //------------Setup for test--------------------------
            var deleteItem = new DeleteItemService();
            var serializer = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            StringBuilder jsonResult = deleteItem.Execute(null, null);
            IExplorerRepositoryResult result = serializer.Deserialize<IExplorerRepositoryResult>(jsonResult);
            //------------Assert Results-------------------------
            Assert.AreEqual(ExecStatus.Fail, result.Status);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DeleteItemService_Execute")]
        public void DeleteItem_Execute_ItemToDeleteNotInValues_ErrorResult()
        {
            //------------Setup for test--------------------------
            var values = new Dictionary<string, StringBuilder> { { "item", new StringBuilder() } };
            var deleteItem = new DeleteItemService();
            var serializer = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            StringBuilder jsonResult = deleteItem.Execute(values, null);
            IExplorerRepositoryResult result = serializer.Deserialize<IExplorerRepositoryResult>(jsonResult);
            //------------Assert Results-------------------------
            Assert.AreEqual(ExecStatus.Fail, result.Status);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DeleteItemService_Execute")]
        public void DeleteItem_Execute_ItemToDeleteNotServerExplorerItem_ErrorResult()
        {
            //------------Setup for test--------------------------
            var values = new Dictionary<string, StringBuilder> { { "itemToDelete", new StringBuilder("This is not deserializable to ServerExplorerItem") } };
            var deleteItem = new DeleteItemService();
            var serializer = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            StringBuilder jsonResult = deleteItem.Execute(values, null);
            IExplorerRepositoryResult result = serializer.Deserialize<IExplorerRepositoryResult>(jsonResult);
            //------------Assert Results-------------------------
            Assert.AreEqual(ExecStatus.Fail, result.Status);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("deleteItem_Execute")]
        public void DeleteItem_Execute_ExpectName()
        {
            //------------Setup for test--------------------------
            var mock = new Mock<IAuthorizer>();
            mock.Setup(authorizer => authorizer.RunPermissions(It.IsAny<Guid>()));
            var deleteItem = new DeleteItemService(mock.Object);

            ServerExplorerItem item = new ServerExplorerItem("a", Guid.NewGuid(), "Folder", null, Permissions.DeployFrom, "", "", "");
            var repo = new Mock<IExplorerServerResourceRepository>();
            var ws = new Mock<IWorkspace>();
            repo.Setup(a => a.DeleteItem(It.IsAny<IExplorerItem>(), It.IsAny<Guid>())).Returns(new ExplorerRepositoryResult(ExecStatus.Success, "")).Verifiable();

            var serializer = new Dev2JsonSerializer();
            var inputs = new Dictionary<string, StringBuilder> { { "itemToDelete", serializer.SerializeToBuilder(item) } };
            ws.Setup(a => a.ID).Returns(Guid.Empty);
            deleteItem.ServerExplorerRepo = repo.Object;
            //------------Execute Test---------------------------
            deleteItem.Execute(inputs, ws.Object);
            //------------Assert Results-------------------------
            repo.Verify(a => a.DeleteItem(It.IsAny<IExplorerItem>(), It.IsAny<Guid>()));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("deleteItem_Execute")]
        public void DeleteItem_ExecuteNotPermited_ExpectError()
        {
            //------------Setup for test--------------------------
            var mock = new Mock<IAuthorizer>();
             
            mock.Setup(authorizer => authorizer.RunPermissions(It.IsAny<Guid>()))
                .Throws(new ServiceNotAuthorizedException(Warewolf.Resource.Errors.ErrorResource.NotAuthorizedToContributeException));
            var deleteItem = new DeleteItemService(mock.Object);

            ServerExplorerItem item = new ServerExplorerItem("a", Guid.NewGuid(), "Folder", null, Permissions.DeployFrom, "", "", "");
            var repo = new Mock<IExplorerServerResourceRepository>();
            var ws = new Mock<IWorkspace>();
            repo.Setup(a => a.DeleteItem(It.IsAny<IExplorerItem>(), It.IsAny<Guid>())).Returns(new ExplorerRepositoryResult(ExecStatus.Success, "")).Verifiable();
            repo.Setup(a => a.Find(It.IsAny<Func<IExplorerItem, bool>>())).Returns(item).Verifiable();

            var serializer = new Dev2JsonSerializer();
            var inputs = new Dictionary<string, StringBuilder> { { "itemToDelete", serializer.SerializeToBuilder(item) } };
            ws.Setup(a => a.ID).Returns(Guid.Empty);
            deleteItem.ServerExplorerRepo = repo.Object;
            //------------Execute Test---------------------------
            var stringBuilder = deleteItem.Execute(inputs, ws.Object);
            //------------Assert Results-------------------------
            repo.Verify(a => a.DeleteItem(It.IsAny<IExplorerItem>(), It.IsAny<Guid>()), Times.Never);
            var explorerRepositoryResult = serializer.Deserialize<ExplorerRepositoryResult>(stringBuilder);
            Assert.AreEqual(Warewolf.Resource.Errors.ErrorResource.NotAuthorizedToContributeException,explorerRepositoryResult.Message);
            Assert.IsTrue(explorerRepositoryResult.Status == ExecStatus.Fail);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("deleteItem_CreateEntry")]
        public void DeleteItem_CreateServiceEntry_ExpectProperlyFormedDynamicService()
        {
            //------------Setup for test--------------------------
            var deleteItem = new DeleteItemService();


            //------------Execute Test---------------------------
            var a = deleteItem.CreateServiceEntry();
            //------------Assert Results-------------------------
            var b = a.DataListSpecification.ToString();
            Assert.AreEqual("<DataList><itemToAdd ColumnIODirection=\"itemToDelete\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>", b);
        }
    }
}
