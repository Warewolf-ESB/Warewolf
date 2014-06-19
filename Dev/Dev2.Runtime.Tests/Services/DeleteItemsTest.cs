using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Communication;
using Dev2.Data.ServiceModel;
using Dev2.Explorer;
using Dev2.Interfaces;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Runtime.Hosting;
using Dev2.Services.Security;
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
            var deleteItem = new DeleteItemService();

            ServerExplorerItem item = new ServerExplorerItem("a", Guid.NewGuid(), ResourceType.Folder, null, Permissions.DeployFrom, "");
            var repo = new Mock<IExplorerServerResourceRepository>();
            var ws = new Mock<IWorkspace>();
            repo.Setup(a => a.DeleteItem(It.IsAny<IExplorerItem>(), It.IsAny<Guid>())).Returns(new ExplorerRepositoryResult(ExecStatus.Success, "")).Verifiable();

            var serializer = new Dev2JsonSerializer();
            var inputs = new Dictionary<string, StringBuilder>();
            inputs.Add("itemToDelete", serializer.SerializeToBuilder(item));
            ws.Setup(a => a.ID).Returns(Guid.Empty);
            deleteItem.ServerExplorerRepo = repo.Object;
            //------------Execute Test---------------------------
            deleteItem.Execute(inputs, ws.Object);
            //------------Assert Results-------------------------
            repo.Verify(a => a.DeleteItem(It.IsAny<IExplorerItem>(), It.IsAny<Guid>()));
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
            var b = a.DataListSpecification;
            Assert.AreEqual("<DataList><itemToAdd ColumnIODirection=\"itemToDelete\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>", b);
        }
    }
}