
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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Communication;
using Dev2.Data.Settings;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Services.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

// ReSharper disable InconsistentNaming
namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class SettingsWriteTests
    {

        #region ClassInitialize

        [ClassInitialize]
        public static void MyClassInitialize(TestContext context)
        {
        }

        #endregion

        ExecuteMessage ToMsg(StringBuilder sb)
        {
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            return serializer.Deserialize<ExecuteMessage>(sb);
        }

        #region Execute

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SettingsWrite_Execute")]
        [ExpectedException(typeof(InvalidDataException))]
        public void SettingsWrite_Execute_NoSettingsValuePassed_ExceptionThrown()
        {
            //------------Setup for test--------------------------
            var settingsWrite = new SettingsWrite();
            //------------Execute Test---------------------------
            settingsWrite.Execute(new Dictionary<string, StringBuilder> { { "NoSettings", new StringBuilder("Something") } }, null);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SettingsWrite_Execute")]
        [ExpectedException(typeof(InvalidDataException))]
        public void SettingsWrite_Execute_NoValuesPassed_ExceptionThrown()
        {
            //------------Setup for test--------------------------
            var settingsWrite = new SettingsWrite();
            //------------Execute Test---------------------------
            settingsWrite.Execute(null, null);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SettingsWrite_Execute")]
        public void SettingsWrite_Execute_SettingsValuePassedNotValidJSON_ExceptionThrown()
        {
            //------------Setup for test--------------------------
            var settingsWrite = new SettingsWrite();
            //------------Execute Test---------------------------
            var execute = settingsWrite.Execute(new Dictionary<string, StringBuilder> { { "Settings", new StringBuilder("Something") } }, null);
            //------------Assert Results-------------------------
            StringAssert.Contains(execute.ToString(), "Error writing settings.");

        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SettingsWrite_Execute")]
        public void SettingsWrite_Execute_SettingsWriteValuePassedValidJSON_ShouldDoSecurityWrite()
        {
            //------------Setup for test--------------------------
            var permission = new WindowsGroupPermission { Administrator = true, IsServer = true, WindowsGroup = Environment.UserName };
            var windowsGroupPermissions = new List<WindowsGroupPermission> { permission };
            var settings = new Settings { Security = new SecuritySettingsTO(windowsGroupPermissions) };
            var serializeObject = JsonConvert.SerializeObject(settings);
            var settingsWrite = new SettingsWrite();
            //------------Execute Test---------------------------
            StringBuilder execute = settingsWrite.Execute(new Dictionary<string, StringBuilder> { { "Settings", new StringBuilder(serializeObject) } }, null);
            //------------Assert Results-------------------------
            Assert.IsTrue(File.Exists("secure.config"));
            File.Delete("secure.config");

            var msg = ToMsg(execute);

            StringAssert.Contains(msg.Message.ToString(), "Success");
        }

        #endregion Exeute

        #region HandlesType

        [TestMethod]
        public void SettingsWrite_HandlesType_ReturnsSettingsWriteService()
        {
            var esb = new SettingsWrite();
            var result = esb.HandlesType();
            Assert.AreEqual("SettingsWriteService", result);
        }

        #endregion

        #region CreateServiceEntry

        [TestMethod]
        public void SettingsWrite_CreateServiceEntry_ReturnsDynamicService()
        {
            var esb = new SettingsWrite();
            var result = esb.CreateServiceEntry();
            Assert.AreEqual(esb.HandlesType(), result.Name);
            Assert.AreEqual("<DataList><Settings ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>", result.DataListSpecification.ToString());
            Assert.AreEqual(1, result.Actions.Count);

            var serviceAction = result.Actions[0];
            Assert.AreEqual(esb.HandlesType(), serviceAction.Name);
            Assert.AreEqual(enActionType.InvokeManagementDynamicService, serviceAction.ActionType);
            Assert.AreEqual(esb.HandlesType(), serviceAction.SourceMethod);
        }

        #endregion

    }
}
