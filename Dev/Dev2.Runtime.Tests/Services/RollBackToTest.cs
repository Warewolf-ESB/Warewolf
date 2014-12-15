
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class RollBackToTest
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("RolbackTo_Name")]
// ReSharper disable InconsistentNaming
        public void RolbackTo_Name_GetName()

        {
            //------------Setup for test--------------------------
            var rolbackTo = new RollbackTo();
            
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.AreEqual("RollbackTo",rolbackTo.HandlesType());
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("RolbackTo_CreateServiceEntry")]
        public void RolbackTo_CreateServiceEntry_ExpectCorrectDL()
        {
            //------------Setup for test--------------------------
            var rolbackTo = new RollbackTo();

            //------------Execute Test---------------------------
            var res = rolbackTo.CreateServiceEntry();
            //------------Assert Results-------------------------
            Assert.AreEqual("<DataList><Roles ColumnIODirection=\"Input\"/><ResourceXml ColumnIODirection=\"Input\"/><WorkspaceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>", res.DataListSpecification.ToString());
            Assert.AreEqual(enActionType.InvokeManagementDynamicService, res.Actions.First().ActionType);
            Assert.AreEqual(res.Actions.First().SourceMethod,rolbackTo.HandlesType());
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("RolbackTo_Execute")]
        public void RolbackTo_Execute_InvalidParams()
        {
            //------------Setup for test--------------------------
            var rolbackTo = new RollbackTo();
            var serializer = new Dev2JsonSerializer();
            var ws = new Mock<IWorkspace>();
            //------------Execute Test---------------------------
            var ax = rolbackTo.Execute(new Dictionary<string, StringBuilder>(), ws.Object);

            //------------Assert Results-------------------------
            var des =serializer.Deserialize<ExecuteMessage>(ax);
            Assert.AreEqual(des.HasError,true);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("RolbackTo_Execute")]
        public void RolbackTo_Execute_InvalidParams_NoVersion()
        {
            //------------Setup for test--------------------------
            var rolbackTo = new RollbackTo();
            var serializer = new Dev2JsonSerializer();
            var ws = new Mock<IWorkspace>();
            //------------Execute Test---------------------------
            var ax = rolbackTo.Execute(new Dictionary<string, StringBuilder> { { "resourceId", new StringBuilder(Guid.NewGuid().ToString()) } }, ws.Object);

            //------------Assert Results-------------------------
            var des = serializer.Deserialize<ExecuteMessage>(ax);
            Assert.AreEqual(des.HasError, true);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("RolbackTo_Execute")]
        public void RolbackTo_Execute_Valid_ExpectServerCalled()
        {
            //------------Setup for test--------------------------
            var rolbackTo = new RollbackTo();
            var serializer = new Dev2JsonSerializer();
            var ws = new Mock<IWorkspace>();
            var server = new Mock<IServerVersionRepository>();
            var res = Guid.NewGuid();
            //------------Execute Test---------------------------
            rolbackTo.ServerVersionRepo = server.Object;
            var ax = rolbackTo.Execute(new Dictionary<string, StringBuilder> { { "resourceId", new StringBuilder(res.ToString()) }, { "versionNumber", new StringBuilder("1") } }, ws.Object);
            
            //------------Assert Results-------------------------
            serializer.Deserialize<ExecuteMessage>(ax);
            server.Verify(a=>a.RollbackTo(res,"1"));
        }
    }
    // ReSharper restore InconsistentNaming
}
