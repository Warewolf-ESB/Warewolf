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
    [TestClass][ExcludeFromCodeCoverage]
    public class SecurityReadTests
    {
        static string _testDir;

        #region ClassInitialize

        [ClassInitialize]
        public static void MyClassInitialize(TestContext context)
        {
            _testDir = Path.Combine(context.DeploymentDirectory, "SecurityConfig");
            Directory.CreateDirectory(_testDir);
        }

        #endregion

        #region Execute

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SecurityRead_Execute")]
        public void SecurityRead_Execute_WhenSecureConfigDoesNotExist_DefaultPermissionsSet()
        {
            //------------Setup for test--------------------------
            var securityRead = new SecurityRead();
            
            //------------Execute Test---------------------------
            var jsonPermissions = securityRead.Execute(null, null);
            var windowsGroupPermissions = JsonConvert.DeserializeObject<List<WindowsGroupPermission>>(jsonPermissions);
            //------------Assert Results-------------------------
            Assert.AreEqual(1,windowsGroupPermissions.Count);
            Assert.AreEqual("BuiltIn\\Administrators", windowsGroupPermissions[0].WindowsGroup);
            Assert.AreEqual(true,windowsGroupPermissions[0].IsServer);
            Assert.AreEqual(false,windowsGroupPermissions[0].View);
            Assert.AreEqual(false,windowsGroupPermissions[0].Execute);
            Assert.AreEqual(true,windowsGroupPermissions[0].Contribute);
            Assert.AreEqual(true,windowsGroupPermissions[0].DeployTo);
            Assert.AreEqual(true,windowsGroupPermissions[0].DeployFrom);
            Assert.AreEqual(true,windowsGroupPermissions[0].Administrator);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SecurityRead_Execute")]
        public void SecurityRead_Execute_WhenSecureConfigDoesExist_PermissionsSetFromSecureConfig()
        {
            //------------Setup for test--------------------------
            var permission = new WindowsGroupPermission { Administrator = true, IsServer = true, WindowsGroup = Environment.UserName };
            var permission2 = new WindowsGroupPermission { Administrator = false,DeployFrom = false, IsServer = true, WindowsGroup = "NETWORK SERVICE" };
            var windowsGroupPermissions = new List<WindowsGroupPermission> { permission,permission2 };
            var serializeObject = JsonConvert.SerializeObject(windowsGroupPermissions);
            var securityWrite = new SecurityWrite();
            var securityRead = new SecurityRead();
            securityWrite.Execute(new Dictionary<string, string> { { "Permissions", serializeObject } }, null);
            //------------Assert Preconditions-------------------------
            Assert.IsTrue(File.Exists("secure.config"));
            //------------Execute Test---------------------------
            var jsonPermissions = securityRead.Execute(null, null);
            File.Delete("secure.config");
            windowsGroupPermissions = JsonConvert.DeserializeObject<List<WindowsGroupPermission>>(jsonPermissions);
            //------------Assert Results-------------------------
            Assert.AreEqual(2,windowsGroupPermissions.Count);
            Assert.AreEqual(Environment.UserName, windowsGroupPermissions[0].WindowsGroup);
            Assert.AreEqual(true,windowsGroupPermissions[0].IsServer);
            Assert.AreEqual(false,windowsGroupPermissions[0].View);
            Assert.AreEqual(false,windowsGroupPermissions[0].Execute);
            Assert.AreEqual(false,windowsGroupPermissions[0].Contribute);
            Assert.AreEqual(false,windowsGroupPermissions[0].DeployTo);
            Assert.AreEqual(false,windowsGroupPermissions[0].DeployFrom);
            Assert.AreEqual(true,windowsGroupPermissions[0].Administrator);
            Assert.AreEqual("NETWORK SERVICE", windowsGroupPermissions[1].WindowsGroup);
            Assert.AreEqual(true, windowsGroupPermissions[1].IsServer);
            Assert.AreEqual(false, windowsGroupPermissions[1].View);
            Assert.AreEqual(false, windowsGroupPermissions[1].Execute);
            Assert.AreEqual(false, windowsGroupPermissions[1].Contribute);
            Assert.AreEqual(false, windowsGroupPermissions[1].DeployTo);
            Assert.AreEqual(false, windowsGroupPermissions[1].DeployFrom);
            Assert.AreEqual(false, windowsGroupPermissions[1].Administrator);
            
        }

        #endregion Exeute

        #region HandlesType

        [TestMethod]
        public void SecurityReadHandlesTypeExpectedReturnsSecurityWriteService()
        {
            var esb = new SecurityRead();
            var result = esb.HandlesType();
            Assert.AreEqual("SecurityReadService", result);
        }

        #endregion

        #region CreateServiceEntry

        [TestMethod]
        public void SecurityReadCreateServiceEntryExpectedReturnsDynamicService()
        {
            var esb = new SecurityRead();
            var result = esb.CreateServiceEntry();
            Assert.AreEqual(esb.HandlesType(), result.Name);
            Assert.AreEqual("<DataList><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>", result.DataListSpecification);
            Assert.AreEqual(1, result.Actions.Count);

            var serviceAction = result.Actions[0];
            Assert.AreEqual(esb.HandlesType(), serviceAction.Name);
            Assert.AreEqual(enActionType.InvokeManagementDynamicService, serviceAction.ActionType);
            Assert.AreEqual(esb.HandlesType(), serviceAction.SourceMethod);
        }

        #endregion

    }
}