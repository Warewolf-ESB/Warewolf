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
using Dev2.Common.Interfaces.Hosting;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Common.Interfaces.Security;
using Dev2.Communication;
using Dev2.Explorer;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

// ReSharper disable InconsistentNaming
namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class FetchExplorerIDuplicatesTests
    {
        [TestMethod]
        [Owner("Sanele Mthembu")]
        [TestCategory("FetchExplorerIDuplicates_HandlesType")]
        public void FetchExplorerIDuplicates_HandlesType_ExpectName()
        {
            //------------Setup for test--------------------------
            var FetchExplorerIDuplicates = new FetchExplorerIDuplicates();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.AreEqual("FetchExplorerIDuplicates", FetchExplorerIDuplicates.HandlesType());
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        [TestCategory("FetchExplorerIDuplicates_Execute")]
        public void FetchExplorerIDuplicates_Execute_NullValuesParameter_ErrorResult()
        {
            //------------Setup for test--------------------------
            var FetchExplorerIDuplicates = new FetchExplorerIDuplicates();
            var serializer = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            StringBuilder jsonResult = FetchExplorerIDuplicates.Execute(null, null);
            IExplorerRepositoryResult result = serializer.Deserialize<IExplorerRepositoryResult>(jsonResult);
            //------------Assert Results-------------------------
            Assert.AreEqual(ExecStatus.Fail, result.Status);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        [TestCategory("FetchExplorerIDuplicates_HandlesType")]
        public void FetchExplorerIDuplicates_Execute_ExpectName()
        {
            //------------Setup for test--------------------------
            var fetchExplorerIDuplicates = new FetchExplorerIDuplicates();

            var serverExplorerItem = new ServerExplorerItem("a", Guid.NewGuid(), "Folder", null, Permissions.DeployFrom, "", "", "");
            Assert.IsNotNull(serverExplorerItem);
            var repo = new Mock<IExplorerServerResourceRepository>();
            var ws = new Mock<IWorkspace>();
            repo.Setup(a => a.LoadDuplicate());
            ws.Setup(a => a.ID).Returns(Guid.Empty);
            fetchExplorerIDuplicates.ServerExplorerRepo = repo.Object;
            //------------Execute Test---------------------------
            fetchExplorerIDuplicates.Execute(new Dictionary<string, StringBuilder>(), ws.Object);
            //------------Assert Results-------------------------
            repo.Verify(a => a.LoadDuplicate());
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        [TestCategory("FetchExplorerIDuplicates_HandlesType")]
        public void FetchExplorerIDuplicates_CreateServiceEntry_ExpectProperlyFormedDynamicService()
        {
            //------------Setup for test--------------------------
            var FetchExplorerIDuplicates = new FetchExplorerIDuplicates();
            //------------Execute Test---------------------------
            var a = FetchExplorerIDuplicates.CreateServiceEntry();
            //------------Assert Results-------------------------
            var b = a.DataListSpecification.ToString();
            Assert.AreEqual("<DataList><ResourceType ColumnIODirection=\"Input\"/><Roles ColumnIODirection=\"Input\"/><ResourceName ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>", b);
        }
    }
  
}
