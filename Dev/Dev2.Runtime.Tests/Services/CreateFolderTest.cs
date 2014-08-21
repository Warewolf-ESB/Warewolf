using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Security;
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
    public class CreateFolderTest
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("CreateFolder_HandlesType")]
        public void CreateFolder_HandlesType_ExpectName()
        {
            //------------Setup for test--------------------------
            var addFolder = new AddFolderService();


            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.AreEqual("AddFolderService", addFolder.HandlesType());
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("CreateFolder_Execute")]
        public void CreateFolder_Execute_ExpectCreateCalled()
        {
            //------------Setup for test--------------------------
            var createFolderService = new AddFolderService();

            ServerExplorerItem item = new ServerExplorerItem("a", Guid.NewGuid(), ResourceType.Folder, null, Permissions.DeployFrom, "");
            var repo = new Mock<IExplorerServerResourceRepository>();
            var ws = new Mock<IWorkspace>();
            repo.Setup(a => a.AddItem(item, It.IsAny<Guid>())).Returns(new ExplorerRepositoryResult(ExecStatus.Fail, "noddy"));
            var serializer = new Dev2JsonSerializer();
            var inputs = new Dictionary<string, StringBuilder>();
            inputs.Add("itemToAdd", serializer.SerializeToBuilder(item));

            ws.Setup(a => a.ID).Returns(Guid.Empty);
            createFolderService.ServerExplorerRepo = repo.Object;
            //------------Execute Test---------------------------
            createFolderService.Execute(inputs, ws.Object);
            //------------Assert Results-------------------------
            repo.Verify(a => a.AddItem(It.IsAny<IExplorerItem>(), It.IsAny<Guid>()));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("CreateFolder_HandlesType")]
        public void CreateFolder_CreateServiceEntry_ExpectProperlyFormedDynamicService()
        {
            //------------Setup for test--------------------------
            var createFolder = new AddFolderService();


            //------------Execute Test---------------------------
            var a = createFolder.CreateServiceEntry();
            //------------Assert Results-------------------------
            var b = a.DataListSpecification;
            Assert.AreEqual("<DataList><itemToAdd ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>", b);
        }
    }
}