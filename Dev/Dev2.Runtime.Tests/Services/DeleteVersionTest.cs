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
using System.Linq;
using System.Text;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Versioning;
using Dev2.Communication;
using Dev2.Runtime.ESB.Management;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Runtime.Exceptions;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class DeleteVersionTest
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DeleteVersion_Name")]
// ReSharper disable InconsistentNaming
        public void DeleteVersion_Name_GetName()

        {
            //------------Setup for test--------------------------
            var DeleteVersion = new DeleteVersion();

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.AreEqual("DeleteVersion", DeleteVersion.HandlesType());
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DeleteVersion_CreateServiceEntry")]
        public void DeleteVersion_CreateServiceEntry_ExpectCorrectDL()
        {
            //------------Setup for test--------------------------
            var DeleteVersion = new DeleteVersion();

            //------------Execute Test---------------------------
            var res = DeleteVersion.CreateServiceEntry();
            //------------Assert Results-------------------------
            Assert.AreEqual("<DataList><Roles ColumnIODirection=\"Input\"/><ResourceXml ColumnIODirection=\"Input\"/><WorkspaceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>", res.DataListSpecification.ToString());
            Assert.AreEqual(enActionType.InvokeManagementDynamicService, res.Actions.First().ActionType);
            Assert.AreEqual(res.Actions.First().SourceMethod, DeleteVersion.HandlesType());
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DeleteVersion_Execute")]
        public void DeleteVersion_Execute_InvalidParams()
        {
            //------------Setup for test--------------------------
            var DeleteVersion = new DeleteVersion();
            var serializer = new Dev2JsonSerializer();
            var ws = new Mock<IWorkspace>();
            //------------Execute Test---------------------------
            var ax = DeleteVersion.Execute(new Dictionary<string, StringBuilder>(), ws.Object);

            //------------Assert Results-------------------------
            var des = serializer.Deserialize<ExecuteMessage>(ax);
            Assert.AreEqual(des.HasError, true);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DeleteVersion_Execute")]
        public void DeleteVersion_Execute_InvalidParams_NoVersion()
        {
            //------------Setup for test--------------------------
            var DeleteVersion = new DeleteVersion();
            var serializer = new Dev2JsonSerializer();
            var ws = new Mock<IWorkspace>();
            //------------Execute Test---------------------------
            var ax = DeleteVersion.Execute(new Dictionary<string, StringBuilder> { { "resourceId", new StringBuilder(Guid.NewGuid().ToString()) } }, ws.Object);

            //------------Assert Results-------------------------
            var des = serializer.Deserialize<ExecuteMessage>(ax);
            Assert.AreEqual(des.HasError, true);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("DeleteVersion_Execute")]
        public void DeleteVersion_ExecuteNotPermited_Valid_ExpectFailure()
        {
            //------------Setup for test--------------------------
            var mock = new Mock<IAuthorizer>();
            mock.Setup(authorizer => authorizer.RunPermissions(It.IsAny<Guid>())).Throws(new ServiceNotAuthorizedException(Warewolf.Resource.Errors.ErrorResource.NotAuthorizedToContributeException));
            var deleteVersion = new DeleteVersion(mock.Object);
            var serializer = new Dev2JsonSerializer();
            var ws = new Mock<IWorkspace>();
            var server = new Mock<IServerVersionRepository>();
            var res = Guid.NewGuid();
            //------------Execute Test---------------------------
            deleteVersion.ServerVersionRepo = server.Object;
            var ax = deleteVersion.Execute(new Dictionary<string, StringBuilder> { { "resourceId", new StringBuilder(res.ToString()) }, { "versionNumber", new StringBuilder("1") } }, ws.Object);

            //------------Assert Results-------------------------
            var executeMessage = serializer.Deserialize<ExecuteMessage>(ax);
            server.Verify(a => a.DeleteVersion(It.IsAny<Guid>(), "1", ""), Times.Never);
            Assert.AreEqual(executeMessage.Message.ToString(), Warewolf.Resource.Errors.ErrorResource.NotAuthorizedToContributeException);
            Assert.IsTrue(executeMessage.HasError);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DeleteVersion_Execute")]
        public void DeleteVersion_Execute_Valid_ExpectServerCalled()
        {
            //------------Setup for test--------------------------

            var mock = new Mock<IAuthorizer>();
            mock.Setup(authorizer => authorizer.RunPermissions(It.IsAny<Guid>()));
            var deleteVersion = new DeleteVersion(mock.Object);
            var serializer = new Dev2JsonSerializer();
            var ws = new Mock<IWorkspace>();
            var server = new Mock<IServerVersionRepository>();
            var res = Guid.NewGuid();
            //------------Execute Test---------------------------
            deleteVersion.ServerVersionRepo = server.Object;
            var ax = deleteVersion.Execute(new Dictionary<string, StringBuilder> { { "resourceId", new StringBuilder(res.ToString()) }, { "versionNumber", new StringBuilder("1") } }, ws.Object);

            //------------Assert Results-------------------------
            serializer.Deserialize<ExecuteMessage>(ax);
            server.Verify(a => a.DeleteVersion(It.IsAny<Guid>(), "1", ""));
        }
    }
    // ReSharper restore InconsistentNaming
}
