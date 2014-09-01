using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Hosting;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Common.Interfaces.Security;
using Dev2.Communication;
using Dev2.Explorer;
using Dev2.Interfaces;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

// ReSharper disable InconsistentNaming
namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class FetchExplorerItemsTest
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("FetchExplorerItems_HandlesType")]
        public void FetchExplorerItems_HandlesType_ExpectName()
        {
            //------------Setup for test--------------------------
            var fetchExplorerItems = new FetchExplorerItems();


            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.AreEqual("FetchExplorerItemsService", fetchExplorerItems.HandlesType());
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("FetchExplorerItems_Execute")]
        public void FetchExplorerItems_Execute_NullValuesParameter_ErrorResult()
        {
            //------------Setup for test--------------------------
            var fetchExplorerItems = new FetchExplorerItems();
            var serializer = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            StringBuilder jsonResult = fetchExplorerItems.Execute(null, null);
            IExplorerRepositoryResult result = serializer.Deserialize<IExplorerRepositoryResult>(jsonResult);
            //------------Assert Results-------------------------
            Assert.AreEqual(ExecStatus.Fail, result.Status);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("FetchExplorerItems_HandlesType")]
        public void FetchExplorerItems_Execute_ExpectName()
        {
            //------------Setup for test--------------------------
            var fetchExplorerItems = new FetchExplorerItems();

            ServerExplorerItem item = new ServerExplorerItem("a", Guid.NewGuid(), ResourceType.Folder, null, Permissions.DeployFrom, "");
            var repo = new Mock<IExplorerServerResourceRepository>();
            var ws = new Mock<IWorkspace>();
            repo.Setup(a => a.Load(It.IsAny<Guid>())).Returns(item).Verifiable();
            var serializer = new Dev2JsonSerializer();
            ws.Setup(a => a.ID).Returns(Guid.Empty);
            fetchExplorerItems.ServerExplorerRepo = repo.Object;
            //------------Execute Test---------------------------
            var ax = fetchExplorerItems.Execute(new Dictionary<string, StringBuilder>(), ws.Object);
            //------------Assert Results-------------------------
            repo.Verify(a => a.Load(It.IsAny<Guid>()));
            Assert.AreEqual(serializer.Deserialize<IExplorerItem>(ax.ToString()).ResourceId, item.ResourceId);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("FetchExplorerItems_HandlesType")]
        public void FetchExplorerItems_CreateServiceEntry_ExpectProperlyFormedDynamicService()
        {
            //------------Setup for test--------------------------
            var fetchExplorerItems = new FetchExplorerItems();


            //------------Execute Test---------------------------
            var a = fetchExplorerItems.CreateServiceEntry();
            //------------Assert Results-------------------------
            var b = a.DataListSpecification;
            Assert.AreEqual("<DataList><ResourceType ColumnIODirection=\"Input\"/><Roles ColumnIODirection=\"Input\"/><ResourceName ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>", b);
        }
    }
  
}
