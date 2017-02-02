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

// ReSharper disable InconsistentNaming
namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class FetchExplorerItemsTest
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("GetResourceID")]
        public void GetResourceID_ShouldReturnEmptyGuid()
        {
            //------------Setup for test--------------------------
            var fetchExplorerItems = new FetchExplorerItems();

            //------------Execute Test---------------------------
            var resId = fetchExplorerItems.GetResourceID(new Dictionary<string, StringBuilder>());
            //------------Assert Results-------------------------
            Assert.AreEqual(Guid.Empty, resId);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("GetResourceID")]
        public void GetAuthorizationContextForService_ShouldReturnContext()
        {
            //------------Setup for test--------------------------
            var fetchExplorerItems = new FetchExplorerItems();

            //------------Execute Test---------------------------
            var resId = fetchExplorerItems.GetAuthorizationContextForService();
            //------------Assert Results-------------------------
            Assert.AreEqual(AuthorizationContext.Any, resId);
        }

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

            var item = new ServerExplorerItem("a", Guid.NewGuid(), "Folder", null, Permissions.DeployFrom, "");
            Assert.IsNotNull(item);
            var repo = new Mock<IExplorerServerResourceRepository>();
            var ws = new Mock<IWorkspace>();
            repo.Setup(a => a.Load(GlobalConstants.ServerWorkspaceID, false))
                .Returns(item).Verifiable();
            var serializer = new Dev2JsonSerializer();
            ws.Setup(a => a.ID).Returns(Guid.Empty);
            fetchExplorerItems.ServerExplorerRepo = repo.Object;
            //------------Execute Test---------------------------
            var execute = fetchExplorerItems.Execute(new Dictionary<string, StringBuilder>(), ws.Object);
            //------------Assert Results-------------------------
            repo.Verify(a => a.Load(GlobalConstants.ServerWorkspaceID, false));
            var message = serializer.Deserialize<CompressedExecuteMessage>(execute);
            Assert.AreEqual(serializer.Deserialize<IExplorerItem>(message.GetDecompressedMessage()).ResourceId, item.ResourceId);
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
            var b = a.DataListSpecification.ToString();
            Assert.AreEqual("<DataList><ResourceType ColumnIODirection=\"Input\"/><Roles ColumnIODirection=\"Input\"/><ResourceName ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>", b);
        }
    }

}
