using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using Dev2.DynamicServices;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Services.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    [ExcludeFromCodeCoverage]
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
        public void SecurityRead_Execute_WhenSecureConfigDoesExist_PermissionsSetFromSecureConfig()
        {
            //------------Setup for test--------------------------
            if(File.Exists("secure.config"))
            {
                File.Delete("secure.config");
            }
            var permission = new WindowsGroupPermission { Administrator = true, IsServer = true, WindowsGroup = Environment.UserName };
            var permission2 = new WindowsGroupPermission { Administrator = false, DeployFrom = false, IsServer = true, WindowsGroup = "NETWORK SERVICE" };
            var windowsGroupPermissions = new List<WindowsGroupPermission> { permission, permission2 };
            var securitySettings = new SecuritySettingsTO(windowsGroupPermissions) { CacheTimeout = new TimeSpan(0, 10, 0) };
            var serializeObject = JsonConvert.SerializeObject(securitySettings);
            var securityWrite = new SecurityWrite();
            var securityRead = new SecurityRead();
            securityWrite.Execute(new Dictionary<string, StringBuilder> { { "SecuritySettings", new StringBuilder(serializeObject) } }, null);
            //------------Assert Preconditions-------------------------
            Assert.IsTrue(File.Exists("secure.config"));
            //------------Execute Test---------------------------
            var jsonPermissions = securityRead.Execute(null, null);
            File.Delete("secure.config");
            var readSecuritySettings = JsonConvert.DeserializeObject<SecuritySettingsTO>(jsonPermissions.ToString());
            //------------Assert Results-------------------------
            Assert.AreEqual(2, readSecuritySettings.WindowsGroupPermissions.Count);
            Assert.AreEqual(Environment.UserName, readSecuritySettings.WindowsGroupPermissions[0].WindowsGroup);
            Assert.AreEqual(true, readSecuritySettings.WindowsGroupPermissions[0].IsServer);
            Assert.AreEqual(false, readSecuritySettings.WindowsGroupPermissions[0].View);
            Assert.AreEqual(false, readSecuritySettings.WindowsGroupPermissions[0].Execute);
            Assert.AreEqual(false, readSecuritySettings.WindowsGroupPermissions[0].Contribute);
            Assert.AreEqual(false, readSecuritySettings.WindowsGroupPermissions[0].DeployTo);
            Assert.AreEqual(false, readSecuritySettings.WindowsGroupPermissions[0].DeployFrom);
            Assert.AreEqual(true, readSecuritySettings.WindowsGroupPermissions[0].Administrator);
            Assert.AreEqual("NETWORK SERVICE", readSecuritySettings.WindowsGroupPermissions[1].WindowsGroup);
            Assert.AreEqual(true, readSecuritySettings.WindowsGroupPermissions[1].IsServer);
            Assert.AreEqual(false, readSecuritySettings.WindowsGroupPermissions[1].View);
            Assert.AreEqual(false, readSecuritySettings.WindowsGroupPermissions[1].Execute);
            Assert.AreEqual(false, readSecuritySettings.WindowsGroupPermissions[1].Contribute);
            Assert.AreEqual(false, readSecuritySettings.WindowsGroupPermissions[1].DeployTo);
            Assert.AreEqual(false, readSecuritySettings.WindowsGroupPermissions[1].DeployFrom);
            Assert.AreEqual(false, readSecuritySettings.WindowsGroupPermissions[1].Administrator);
            Assert.AreEqual(new TimeSpan(0, 10, 0), readSecuritySettings.CacheTimeout);

        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SecurityRead_Execute")]
        public void SecurityRead_Execute_WhenSecureConfigDoesNotExist_ReturnsDefaultPermissions()
        {
            //------------Setup for test--------------------------
            var securityRead = new SecurityRead();

            //------------Execute Test---------------------------
            var jsonPermissions = securityRead.Execute(null, null);
            var securitySettings = JsonConvert.DeserializeObject<SecuritySettingsTO>(jsonPermissions.ToString());

            //------------Assert Results-------------------------
            Assert.IsTrue(securitySettings.WindowsGroupPermissions.Count == 1);

            var expected = SecurityRead.DefaultPermissions[0];
            var actual = securitySettings.WindowsGroupPermissions[0];

            var result = new WindowsGroupPermissionEqualityComparer().Equals(expected, actual);
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SecurityRead_Execute")]
        public void SecurityRead_Execute_DecryptThrowsException_ReturnsDefaultPermissions()
        {
            //------------Setup for test--------------------------
            File.WriteAllText("secure.config", "Invalid content.");
            var securityRead = new SecurityRead();

            //------------Execute Test---------------------------
            var jsonPermissions = securityRead.Execute(null, null);
            var securitySettings = JsonConvert.DeserializeObject<SecuritySettingsTO>(jsonPermissions.ToString());

            File.Delete("secure.config");

            //------------Assert Results-------------------------
            Assert.IsTrue(securitySettings.WindowsGroupPermissions.Count == 1);

            var expected = SecurityRead.DefaultPermissions[0];
            var actual = securitySettings.WindowsGroupPermissions[0];

            var result = new WindowsGroupPermissionEqualityComparer().Equals(expected, actual);
            Assert.IsTrue(result);
        }

        #endregion Exeute

        #region HandlesType

        [TestMethod]
        public void SecurityRead_HandlesType_ReturnsSecurityReadService()
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