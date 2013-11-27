using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Dev2.Data.Settings.Security;
using Dev2.DynamicServices;
using Dev2.Runtime.ESB.Management.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Dev2.Tests.Runtime.Services
{
    // ReSharper disable InconsistentNaming

    [TestClass][ExcludeFromCodeCoverage]
    public class SecurityWriteTests
    {
        static string _testDir;

        #region ClassInitialize

        [ClassInitialize]
        public static void MyClassInitialize(TestContext context)
        {
        }

        #endregion

        #region Execute

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SecurityWrite_Execute")]
        [ExpectedException(typeof(InvalidDataException))]
        public void SecurityWrite_Execute_NoPermissionsValuePassed_ExceptionThrown()
        {
            //------------Setup for test--------------------------
            var securityWrite = new SecurityWrite();
            //------------Execute Test---------------------------
            securityWrite.Execute(new Dictionary<string, string> { { "NoPermisisons", "Something" } }, null);
            //------------Assert Results-------------------------
        }  
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SecurityWrite_Execute")]
        [ExpectedException(typeof(InvalidDataException))]
        public void SecurityWrite_Execute_NoValuesPassed_ExceptionThrown()
        {
            //------------Setup for test--------------------------
            var securityWrite = new SecurityWrite();
            //------------Execute Test---------------------------
            securityWrite.Execute(null, null);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SecurityWrite_Execute")]
        [ExpectedException(typeof(InvalidDataException))]
        public void SecurityWrite_Execute_PermissionsValuePassedNotValidJSON_ExceptionThrown()
        {
            //------------Setup for test--------------------------
            var securityWrite = new SecurityWrite();
            //------------Execute Test---------------------------
            securityWrite.Execute(new Dictionary<string, string> { { "Permissions", "Something" } }, null);
            //------------Assert Results-------------------------
        } 
 

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SecurityWrite_Execute")]
        public void SecurityWrite_Execute_PermissionsValuePassedValidJSON_ShouldWriteFile()
        {
            //------------Setup for test--------------------------
            var permission = new WindowsGroupPermission { Administrator = true, IsServer = true, WindowsGroup = Environment.UserName };
            var windowsGroupPermissions = new List<WindowsGroupPermission> { permission };
            var serializeObject = JsonConvert.SerializeObject(windowsGroupPermissions);
            var securityWrite = new SecurityWrite();
            //------------Execute Test---------------------------
            securityWrite.Execute(new Dictionary<string, string> { { "Permissions", serializeObject } }, null);
            //------------Assert Results-------------------------
            Assert.IsTrue(File.Exists("secure.config"));
            var fileData = File.ReadAllText("secure.config");
            Assert.IsFalse(fileData.StartsWith("{"));
            Assert.IsFalse(fileData.EndsWith("}"));
            Assert.IsFalse(fileData.Contains("IsServer"));
            File.Delete("secure.config");
        }  

        #endregion Exeute

        #region HandlesType

        [TestMethod]
        public void SecurityWriteHandlesTypeExpectedReturnsSecurityWriteService()
        {
            var esb = new SecurityWrite();
            var result = esb.HandlesType();
            Assert.AreEqual("SecurityWriteService", result);
        }

        #endregion

        #region CreateServiceEntry

        [TestMethod]
        public void SecurityWriteCreateServiceEntryExpectedReturnsDynamicService()
        {
            var esb = new SecurityWrite();
            var result = esb.CreateServiceEntry();
            Assert.AreEqual(esb.HandlesType(), result.Name);
            Assert.AreEqual("<DataList><Permissions/><Result/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>", result.DataListSpecification);
            Assert.AreEqual(1, result.Actions.Count);

            var serviceAction = result.Actions[0];
            Assert.AreEqual(esb.HandlesType(), serviceAction.Name);
            Assert.AreEqual(enActionType.InvokeManagementDynamicService, serviceAction.ActionType);
            Assert.AreEqual(esb.HandlesType(), serviceAction.SourceMethod);
        }

        #endregion

    }
}