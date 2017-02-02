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
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Common.Interfaces.Security;
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
    public class FetchResourceDuplicatesTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("GetResourceID")]
        public void GetResourceID_ShouldReturnEmptyGuid()
        {
            //------------Setup for test--------------------------
            var fetchResourceDuplicates = new FetchResourceDuplicates();

            //------------Execute Test---------------------------
            var resId = fetchResourceDuplicates.GetResourceID(new Dictionary<string, StringBuilder>());
            //------------Assert Results-------------------------
            Assert.AreEqual(Guid.Empty, resId);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("GetResourceID")]
        public void GetAuthorizationContextForService_ShouldReturnContext()
        {
            //------------Setup for test--------------------------
            var fetchResourceDuplicates = new FetchResourceDuplicates();

            //------------Execute Test---------------------------
            var resId = fetchResourceDuplicates.GetAuthorizationContextForService();
            //------------Assert Results-------------------------
            Assert.AreEqual(AuthorizationContext.Any, resId);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        [TestCategory("FetchResourceDuplicates_HandlesType")]
        public void FetchResourceDuplicates_HandlesType_ExpectName()
        {
            //------------Setup for test--------------------------
            var FetchResourceDuplicates = new FetchResourceDuplicates();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.AreEqual("FetchResourceDuplicates", FetchResourceDuplicates.HandlesType());
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        [TestCategory("FetchResourceDuplicates_HandlesType")]
        public void FetchResourceDuplicates_Execute_ExpectName()
        {
            //------------Setup for test--------------------------
            var FetchResourceDuplicates = new FetchResourceDuplicates();

            var serverExplorerItem = new ServerExplorerItem("a", Guid.NewGuid(), "Folder", null, Permissions.DeployFrom, "");
            Assert.IsNotNull(serverExplorerItem);
            var repo = new Mock<IExplorerServerResourceRepository>();
            var ws = new Mock<IWorkspace>();
            repo.Setup(a => a.LoadDuplicate());
            ws.Setup(a => a.ID).Returns(Guid.Empty);
            FetchResourceDuplicates.ServerExplorerRepo = repo.Object;
            //------------Execute Test---------------------------
            FetchResourceDuplicates.Execute(new Dictionary<string, StringBuilder>(), ws.Object);
            //------------Assert Results-------------------------
            repo.Verify(a => a.LoadDuplicate());
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        [TestCategory("FetchResourceDuplicates_HandlesType")]
        public void FetchResourceDuplicates_CreateServiceEntry_ExpectProperlyFormedDynamicService()
        {
            //------------Setup for test--------------------------
            var FetchResourceDuplicates = new FetchResourceDuplicates();
            //------------Execute Test---------------------------
            var a = FetchResourceDuplicates.CreateServiceEntry();
            //------------Assert Results-------------------------
            var b = a.DataListSpecification.ToString();
            Assert.AreEqual("<DataList><ResourceType ColumnIODirection=\"Input\"/><Roles ColumnIODirection=\"Input\"/><ResourceName ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>", b);
        }
    }
  
}
