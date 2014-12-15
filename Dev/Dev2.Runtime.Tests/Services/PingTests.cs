
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
using System.Text;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Communication;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class PingTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("Ping_Constructor")]
        public void Ping_Constructor_InitializesProperties()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var ping = new Ping();

            //------------Assert Results-------------------------
            Assert.IsNotNull(ping.Now);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [Description("HandlesType() MUST return Ping otherwise Server connection tests will fail with a Serviec Not Found exception.")]
        [TestCategory("Ping_HandlesType")]
        public void Ping_HandlesType_Returns_Ping()
        {
            //------------Setup for test--------------------------
            var ping = new Ping();

            //------------Execute Test---------------------------
            var handlesType = ping.HandlesType();

            //------------Assert Results-------------------------
            Assert.AreEqual("Ping", handlesType);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("Ping_Execute")]
        public void Ping_Execute_Returns_Pong()
        {
            //------------Setup for test--------------------------
            var now = DateTime.Now;
            var ping = new Ping { Now = () => now };
            var expected = "Pong @ " + now.ToString("yyyy-MM-dd hh:mm:ss.fff");

            //------------Execute Test---------------------------
            var result = ping.Execute(It.IsAny<Dictionary<string, StringBuilder>>(), It.IsAny<IWorkspace>());

            var ser = new Dev2JsonSerializer();
            var msg = ser.Deserialize<ExecuteMessage>(result);

            //------------Assert Results-------------------------
            Assert.AreEqual(expected, msg.Message.ToString());
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("Ping_CreateServiceEntry")]
        public void Ping_CreateServiceEntry_Correct()
        {
            //------------Setup for test--------------------------
            var ping = new Ping();
            const string ExpectedName = "Ping";

            //------------Execute Test---------------------------
            var result = ping.CreateServiceEntry();

            //------------Assert Results-------------------------
            Assert.AreEqual(ExpectedName, result.Name);

            Assert.AreEqual(1, result.Actions.Count);
            Assert.AreEqual(ExpectedName, result.Actions[0].Name);
            Assert.AreEqual(ExpectedName, result.Actions[0].SourceMethod);
            Assert.AreEqual(enActionType.InvokeManagementDynamicService, result.Actions[0].ActionType);
            Assert.AreEqual("<DataList><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>", result.Actions[0].DataListSpecification.ToString());
        }
    }
}
