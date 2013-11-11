using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Dev2.Data.Settings;
using Dev2.Data.Settings.Security;
using Dev2.DynamicServices;
using Dev2.Runtime.ESB.Management.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

// ReSharper disable InconsistentNaming
namespace Dev2.Tests.Runtime.Services
{
    [TestClass][ExcludeFromCodeCoverage]
    public class SettingsWriteTests
    {

        #region ClassInitialize

        [ClassInitialize]
        public static void MyClassInitialize(TestContext context)
        {
        }

        #endregion

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
            settingsWrite.Execute(new Dictionary<string, string> { { "NoSettings", "Something" } }, null);
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
            var execute = settingsWrite.Execute(new Dictionary<string, string> { { "Settings", "Something" } }, null);
            //------------Assert Results-------------------------
            StringAssert.Contains(execute, "Error writing settings configuration.");

        } 
 

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SettingsWrite_Execute")]
        public void SettingsWrite_Execute_SettingsWriteValuePassedValidJSON_ShouldDoSecurityWrite()
        {
            //------------Setup for test--------------------------
            var permission = new WindowsGroupPermission { Administrator = true, IsServer = true, WindowsGroup = Environment.UserName };
            var windowsGroupPermissions = new List<WindowsGroupPermission> { permission };
            var settings = new Settings { Security = windowsGroupPermissions };
            var serializeObject = JsonConvert.SerializeObject(settings);
            var settingsWrite = new SettingsWrite();
            //------------Execute Test---------------------------
            var execute = settingsWrite.Execute(new Dictionary<string, string> { { "Settings", serializeObject } }, null);
            //------------Assert Results-------------------------
            Assert.IsTrue(File.Exists("secure.config"));
            File.Delete("secure.config");
            Assert.AreEqual("Success", execute);
        }  

        #endregion Exeute

        #region HandlesType

        [TestMethod]
        public void SettingsWriteHandlesTypeExpectedReturnsSecurityWriteService()
        {
            var esb = new SettingsWrite();
            var result = esb.HandlesType();
            Assert.AreEqual("SettingsWriteService", result);
        }

        #endregion

        #region CreateServiceEntry

        [TestMethod]
        public void SettingsWriteCreateServiceEntryExpectedReturnsDynamicService()
        {
            var esb = new SettingsWrite();
            var result = esb.CreateServiceEntry();
            Assert.AreEqual(esb.HandlesType(), result.Name);
            Assert.AreEqual("<DataList><Settings/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>", result.DataListSpecification);
            Assert.AreEqual(1, result.Actions.Count);

            var serviceAction = result.Actions[0];
            Assert.AreEqual(esb.HandlesType(), serviceAction.Name);
            Assert.AreEqual(enActionType.InvokeManagementDynamicService, serviceAction.ActionType);
            Assert.AreEqual(esb.HandlesType(), serviceAction.SourceMethod);
        }

        #endregion

    }
}