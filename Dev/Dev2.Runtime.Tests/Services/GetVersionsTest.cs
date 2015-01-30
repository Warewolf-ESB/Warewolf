
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/



    using Dev2.Common.Interfaces.Data;
    using Dev2.Common.Interfaces.Explorer;
    using Dev2.Common.Interfaces.Hosting;
    using Dev2.Common.Interfaces.Infrastructure;
    using Dev2.Common.Interfaces.Security;
    using Dev2.Common.Interfaces.Versioning;
    using Dev2.Communication;
    using Dev2.Explorer;
    using Dev2.Runtime.ESB.Management.Services;
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
        public class GetVersionsTest
        {
            [TestMethod]
            [Owner("Leon Rajindrapersadh")]
            [TestCategory("GetVersions_HandlesType")]
            public void GetVersions_HandlesType_ExpectName()
            {
                //------------Setup for test--------------------------
                var getVersions = new GetVersions();


                //------------Execute Test---------------------------

                //------------Assert Results-------------------------
                Assert.AreEqual("GetVersions", getVersions.HandlesType());
            }

            [TestMethod]
            [Owner("Leon Rajindrapersadh")]
            [TestCategory("GetVersions_Execute")]
            public void GetVersions_Execute_NullValuesParameter_ErrorResult()
            {
                //------------Setup for test--------------------------
                var getVersions = new GetVersions();
                var serializer = new Dev2JsonSerializer();
                //------------Execute Test---------------------------
                StringBuilder jsonResult = getVersions.Execute(null, null);
                IExplorerRepositoryResult result = serializer.Deserialize<IExplorerRepositoryResult>(jsonResult);
                //------------Assert Results-------------------------
                Assert.AreEqual(ExecStatus.Fail, result.Status);
            }

            [TestMethod]
            [Owner("Leon Rajindrapersadh")]
            [TestCategory("GetVersions_HandlesType")]
            public void GetVersions_Execute_ExpectName()
            {
                //------------Setup for test--------------------------
                var getVersions = new GetVersions();
                var resourceId = Guid.NewGuid();
                ServerExplorerItem item = new ServerExplorerItem("a", Guid.NewGuid(), ResourceType.Folder, null, Permissions.DeployFrom, "", "", "");
                var repo = new Mock<IServerVersionRepository>();
                var ws = new Mock<IWorkspace>();
                repo.Setup(a => a.GetVersions(resourceId)).Returns(new List<IExplorerItem> {item});
                var serializer = new Dev2JsonSerializer();
                ws.Setup(a => a.ID).Returns(Guid.Empty);
                getVersions.ServerVersionRepo = repo.Object;
                //------------Execute Test---------------------------
                var ax = getVersions.Execute(new Dictionary<string, StringBuilder> {{"resourceId",new StringBuilder( resourceId.ToString())}}, ws.Object);
                //------------Assert Results-------------------------
                repo.Verify(a => a.GetVersions(It.IsAny<Guid>()));
                Assert.AreEqual(serializer.Deserialize<IList<IExplorerItem>>(ax.ToString())[0].ResourceId, item.ResourceId);
            }

            [TestMethod]
            [Owner("Leon Rajindrapersadh")]
            [TestCategory("GetVersions_HandlesType")]
            public void GetVersions_CreateServiceEntry_ExpectProperlyFormedDynamicService()
            {
                //------------Setup for test--------------------------
                var getVersions = new GetVersions();


                //------------Execute Test---------------------------
                var a = getVersions.CreateServiceEntry();
                //------------Assert Results-------------------------
                var b = a.DataListSpecification.ToString();
                Assert.AreEqual(@"<DataList><ResourceType ColumnIODirection=""Input""/><Roles ColumnIODirection=""Input""/><ResourceId ColumnIODirection=""Input""/><Dev2System.ManagmentServicePayload ColumnIODirection=""Both""></Dev2System.ManagmentServicePayload></DataList>", b);
            }
        }
    }


