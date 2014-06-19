using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Data.ServiceModel;
using Dev2.Interfaces;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Runtime.Hosting;
using Dev2.Services.Security;
using Dev2.Shared_Models.Explorer;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class DeleteFolderTest
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DeleteFolder_HandlesType")]
        public void deleteItem_HandlesType_ExpectName()
        {
            //------------Setup for test--------------------------
            var deleteItem = new DeleteItemService();


            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.AreEqual("DeleteFolderService", deleteItem.HandlesType());
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("deleteItem_HandlesType")]
        public void deleteItem_Execute_ExpectName()
        {
            //------------Setup for test--------------------------
            var deleteFolder = new DeleteFolderService();

            ServerExplorerItem item = new ServerExplorerItem("a", Guid.NewGuid(), ResourceType.Folder, null, Permissions.DeployFrom, "");
            var repo = new Mock<IExplorerResourceRepository>();
            var ws = new Mock<IWorkspace>();
            repo.Setup(a => a.DeleteItem(item, It.IsAny<Guid>())).Returns(new ExplorerRepositoryResult(ExecStatus.Success, "")).Verifiable();

    
            var inputs = new Dictionary<string, StringBuilder>();
            inputs.Add("folderToDelete", new StringBuilder( "folderToDelete"));
            inputs.Add("deleteContents", new StringBuilder(true.ToString()));
            ws.Setup(a => a.ID).Returns(Guid.Empty);
            deleteFolder.ServerExplorerRepository = repo.Object;
            //------------Execute Test---------------------------
            var ax = deleteFolder.Execute(inputs, ws.Object);
            //------------Assert Results-------------------------
            repo.Verify(a => a.DeleteItem(item, It.IsAny<Guid>()));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("deleteItem_HandlesType")]
        public void deleteItem_CreateServiceEntry_ExpectProperlyFormedDynamicService()
        {
            //------------Setup for test--------------------------
            var deleteItem = new DeleteItemService();


            //------------Execute Test---------------------------
            var a = deleteItem.CreateServiceEntry();
            //------------Assert Results-------------------------
            var b = a.DataListSpecification;
            Assert.AreEqual("<DataList><ResourceType ColumnIODirection=\"Input\"/><Roles ColumnIODirection=\"Input\"/><ResourceName ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>", b);
        }
    }
}