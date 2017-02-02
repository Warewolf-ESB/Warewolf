/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Runtime.Interfaces;
using Dev2.Services.Security;
using Dev2.Tests.Runtime.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;

// ReSharper disable InconsistentNaming
namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
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
        [Owner("Travis Frisinger")]
        [TestCategory("SecurityRead_Execute")]
        public void SecurityRead_Execute_WhenOldSecureConfigExist_MigratesAdministratorsToWarewolfAdministrators_ExpectSuccessfulMigration()
        {
            //------------Setup for test--------------------------
            var serverSecuritySettingsFile = EnvironmentVariables.ServerSecuritySettingsFile;
            File.Delete(serverSecuritySettingsFile);

            var myConfig = SecurityConfigFetcher.Fetch(serverSecuritySettingsFile);
            File.WriteAllText(serverSecuritySettingsFile, myConfig);

            //------------Assert Preconditions-------------------------
            Assert.IsTrue(File.Exists(serverSecuritySettingsFile));
            // ensure the contents are what we expect ;)

            var mockCatalog = new Mock<IResourceCatalog>();
            mockCatalog.Setup(a => a.GetResourcePath(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns("");
            //------------Execute Test---------------------------
            var securityRead = new SecurityRead
            {
                Catalog = mockCatalog.Object
            };
            var jsonPermissions = securityRead.Execute(null, null);

            File.Delete(serverSecuritySettingsFile);
            var readSecuritySettings = JsonConvert.DeserializeObject<SecuritySettingsTO>(jsonPermissions.ToString());
            //------------Assert Results-------------------------
            Assert.AreEqual(2, readSecuritySettings.WindowsGroupPermissions.Count);

            Assert.AreEqual(WindowsGroupPermission.BuiltInAdministratorsText, readSecuritySettings.WindowsGroupPermissions[0].WindowsGroup);
            Assert.AreEqual(true, readSecuritySettings.WindowsGroupPermissions[0].IsServer);
            Assert.AreEqual(true, readSecuritySettings.WindowsGroupPermissions[0].View);
            Assert.AreEqual(true, readSecuritySettings.WindowsGroupPermissions[0].Execute);
            Assert.AreEqual(true, readSecuritySettings.WindowsGroupPermissions[0].Contribute);
            Assert.AreEqual(true, readSecuritySettings.WindowsGroupPermissions[0].DeployTo);
            Assert.AreEqual(true, readSecuritySettings.WindowsGroupPermissions[0].DeployFrom);
            Assert.AreEqual(true, readSecuritySettings.WindowsGroupPermissions[0].Administrator);

            Assert.AreEqual(WindowsGroupPermission.BuiltInGuestsText, readSecuritySettings.WindowsGroupPermissions[1].WindowsGroup);
            Assert.AreEqual(true, readSecuritySettings.WindowsGroupPermissions[0].IsServer);
            Assert.AreEqual(true, readSecuritySettings.WindowsGroupPermissions[0].View);
            Assert.AreEqual(true, readSecuritySettings.WindowsGroupPermissions[0].Execute);
            Assert.AreEqual(true, readSecuritySettings.WindowsGroupPermissions[0].Contribute);
            Assert.AreEqual(true, readSecuritySettings.WindowsGroupPermissions[0].DeployTo);
            Assert.AreEqual(true, readSecuritySettings.WindowsGroupPermissions[0].DeployFrom);
            Assert.AreEqual(true, readSecuritySettings.WindowsGroupPermissions[0].Administrator);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SecurityRead_Execute")]
        public void SecurityRead_Execute_WhenSecureConfigDoesExistWithNoGuestPermission_ShouldHaveExistingPermissionsAndGuest()
        {
            //------------Setup for test--------------------------
            var serverSecuritySettingsFile = EnvironmentVariables.ServerSecuritySettingsFile;
            File.Delete(serverSecuritySettingsFile);
            var permission = new WindowsGroupPermission { Administrator = true, IsServer = true, WindowsGroup = Environment.UserName };
            var permission2 = new WindowsGroupPermission { Administrator = false, DeployFrom = false, IsServer = true, WindowsGroup = "NETWORK SERVICE" };
            var windowsGroupPermissions = new List<WindowsGroupPermission> { permission, permission2 };
            var securitySettings = new SecuritySettingsTO(windowsGroupPermissions) { CacheTimeout = new TimeSpan(0, 10, 0) };
            var serializeObject = JsonConvert.SerializeObject(securitySettings);
            var securityWrite = new SecurityWrite();
            var securityRead = new SecurityRead();
            securityWrite.Execute(new Dictionary<string, StringBuilder> { { "SecuritySettings", new StringBuilder(serializeObject) } }, null);
            //------------Assert Preconditions-------------------------
            Assert.IsTrue(File.Exists(serverSecuritySettingsFile));
            //------------Execute Test---------------------------
            var jsonPermissions = securityRead.Execute(null, null);
            File.Delete(serverSecuritySettingsFile);
            var readSecuritySettings = JsonConvert.DeserializeObject<SecuritySettingsTO>(jsonPermissions.ToString());
            //------------Assert Results-------------------------
            Assert.AreEqual(4, readSecuritySettings.WindowsGroupPermissions.Count);
            var guestPermission = readSecuritySettings.WindowsGroupPermissions.FirstOrDefault(p => p.WindowsGroup == WindowsGroupPermission.BuiltInGuestsText);
            Assert.IsNotNull(guestPermission);
            Assert.AreEqual(true, guestPermission.IsServer);
            Assert.AreEqual(false, guestPermission.View);
            Assert.AreEqual(false, guestPermission.Execute);
            Assert.AreEqual(false, guestPermission.Contribute);
            Assert.AreEqual(false, guestPermission.DeployTo);
            Assert.AreEqual(false, guestPermission.DeployFrom);
            Assert.AreEqual(false, guestPermission.Administrator);

            var networkServicePermission = readSecuritySettings.WindowsGroupPermissions.FirstOrDefault(p => p.WindowsGroup == "NETWORK SERVICE");
            Assert.IsNotNull(networkServicePermission);
            Assert.AreEqual(true, networkServicePermission.IsServer);
            Assert.AreEqual(false, networkServicePermission.View);
            Assert.AreEqual(false, networkServicePermission.Execute);
            Assert.AreEqual(false, networkServicePermission.Contribute);
            Assert.AreEqual(false, networkServicePermission.DeployTo);
            Assert.AreEqual(false, networkServicePermission.DeployFrom);
            Assert.AreEqual(false, networkServicePermission.Administrator);

            var userPermission = readSecuritySettings.WindowsGroupPermissions.FirstOrDefault(p => p.WindowsGroup == Environment.UserName);
            Assert.IsNotNull(userPermission);
            Assert.AreEqual(true, userPermission.IsServer);
            Assert.AreEqual(false, userPermission.View);
            Assert.AreEqual(false, userPermission.Execute);
            Assert.AreEqual(false, userPermission.Contribute);
            Assert.AreEqual(false, userPermission.DeployTo);
            Assert.AreEqual(false, userPermission.DeployFrom);
            Assert.AreEqual(true, userPermission.Administrator);

            Assert.AreEqual(new TimeSpan(0, 10, 0), readSecuritySettings.CacheTimeout);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SecurityRead_Execute")]
        public void SecurityRead_Execute_WhenSecureConfigDoesExistWithGuestPermission_ShouldHaveExistingPermissions()
        {
            //------------Setup for test--------------------------
            var serverSecuritySettingsFile = EnvironmentVariables.ServerSecuritySettingsFile;
            File.Delete(serverSecuritySettingsFile);
            var permission = new WindowsGroupPermission { Administrator = true, IsServer = true, WindowsGroup = Environment.UserName };
            var permission2 = new WindowsGroupPermission { Administrator = false, DeployFrom = false, IsServer = true, WindowsGroup = "NETWORK SERVICE" };
            var guests = WindowsGroupPermission.CreateGuests();
            guests.View = true;
            var permission3 = guests;
            var windowsGroupPermissions = new List<WindowsGroupPermission> { permission, permission2, permission3 };
            var securitySettings = new SecuritySettingsTO(windowsGroupPermissions) { CacheTimeout = new TimeSpan(0, 10, 0) };
            var serializeObject = JsonConvert.SerializeObject(securitySettings);
            var securityWrite = new SecurityWrite();
            var securityRead = new SecurityRead();
            securityWrite.Execute(new Dictionary<string, StringBuilder> { { "SecuritySettings", new StringBuilder(serializeObject) } }, null);
            //------------Assert Preconditions-------------------------
            Assert.IsTrue(File.Exists(serverSecuritySettingsFile));
            //------------Execute Test---------------------------
            var jsonPermissions = securityRead.Execute(null, null);
            File.Delete("secure.config");
            var readSecuritySettings = JsonConvert.DeserializeObject<SecuritySettingsTO>(jsonPermissions.ToString());
            //------------Assert Results-------------------------
            Assert.AreEqual(4, readSecuritySettings.WindowsGroupPermissions.Count);

            var adminPermission = readSecuritySettings.WindowsGroupPermissions.FirstOrDefault(p => p.IsBuiltInAdministrators);
            Assert.IsNotNull(adminPermission);
            Assert.AreEqual(true, adminPermission.IsServer);
            Assert.AreEqual(true, adminPermission.View);
            Assert.AreEqual(true, adminPermission.Execute);
            Assert.AreEqual(true, adminPermission.Contribute);
            Assert.AreEqual(true, adminPermission.DeployTo);
            Assert.AreEqual(true, adminPermission.DeployFrom);
            Assert.AreEqual(true, adminPermission.Administrator);

            var userPermission = readSecuritySettings.WindowsGroupPermissions.FirstOrDefault(p => p.WindowsGroup == Environment.UserName);
            Assert.IsNotNull(userPermission);
            Assert.AreEqual(true, userPermission.IsServer);
            Assert.AreEqual(false, userPermission.View);
            Assert.AreEqual(false, userPermission.Execute);
            Assert.AreEqual(false, userPermission.Contribute);
            Assert.AreEqual(false, userPermission.DeployTo);
            Assert.AreEqual(false, userPermission.DeployFrom);
            Assert.AreEqual(true, userPermission.Administrator);

            var networkServicePermission = readSecuritySettings.WindowsGroupPermissions.FirstOrDefault(p => p.WindowsGroup == "NETWORK SERVICE");
            Assert.IsNotNull(networkServicePermission);
            Assert.AreEqual(true, networkServicePermission.IsServer);
            Assert.AreEqual(false, networkServicePermission.View);
            Assert.AreEqual(false, networkServicePermission.Execute);
            Assert.AreEqual(false, networkServicePermission.Contribute);
            Assert.AreEqual(false, networkServicePermission.DeployTo);
            Assert.AreEqual(false, networkServicePermission.DeployFrom);
            Assert.AreEqual(false, networkServicePermission.Administrator);

            var guestPermission = readSecuritySettings.WindowsGroupPermissions.FirstOrDefault(p => p.WindowsGroup == WindowsGroupPermission.BuiltInGuestsText);
            Assert.IsNotNull(guestPermission);
            Assert.AreEqual(true, guestPermission.IsServer);
            Assert.AreEqual(true, guestPermission.View);
            Assert.AreEqual(false, guestPermission.Execute);
            Assert.AreEqual(false, guestPermission.Contribute);
            Assert.AreEqual(false, guestPermission.DeployTo);
            Assert.AreEqual(false, guestPermission.DeployFrom);
            Assert.AreEqual(false, guestPermission.Administrator);
            Assert.AreEqual(new TimeSpan(0, 10, 0), readSecuritySettings.CacheTimeout);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SecurityRead_Execute")]
        public void SecurityRead_Execute_WhenSecureConfigDoesNotExist_ReturnsDefaultPermissions()
        {
            //------------Setup for test--------------------------
            var serverSecuritySettingsFile = EnvironmentVariables.ServerSecuritySettingsFile;
            File.Delete(serverSecuritySettingsFile);
            var securityRead = new SecurityRead();

            //------------Execute Test---------------------------
            var jsonPermissions = securityRead.Execute(null, null);
            var securitySettings = JsonConvert.DeserializeObject<SecuritySettingsTO>(jsonPermissions.ToString());

            //------------Assert Results-------------------------
            Assert.IsTrue(securitySettings.WindowsGroupPermissions.Count == 2);

            var expected = SecurityRead.DefaultPermissions[0];
            var actual = securitySettings.WindowsGroupPermissions[0];

            var result = new WindowsGroupPermissionEqualityComparer().Equals(expected, actual);
            Assert.IsTrue(result);

            expected = SecurityRead.DefaultPermissions[1];
            actual = securitySettings.WindowsGroupPermissions[1];
            result = new WindowsGroupPermissionEqualityComparer().Equals(expected, actual);
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SecurityRead_Execute")]
        public void SecurityRead_Execute_DecryptThrowsException_ReturnsDefaultPermissions()
        {
            //------------Setup for test--------------------------
            var serverSecuritySettingsFile = EnvironmentVariables.ServerSecuritySettingsFile;
            File.Delete(serverSecuritySettingsFile);
            File.WriteAllText(serverSecuritySettingsFile, @"Invalid content.");
            var securityRead = new SecurityRead();

            //------------Execute Test---------------------------
            var jsonPermissions = securityRead.Execute(null, null);
            var securitySettings = JsonConvert.DeserializeObject<SecuritySettingsTO>(jsonPermissions.ToString());

            File.Delete(serverSecuritySettingsFile);

            //------------Assert Results-------------------------
            Assert.IsTrue(securitySettings.WindowsGroupPermissions.Count == 2);

            var expected = SecurityRead.DefaultPermissions[0];
            var actual = securitySettings.WindowsGroupPermissions[0];

            var result = new WindowsGroupPermissionEqualityComparer().Equals(expected, actual);
            Assert.IsTrue(result);

            expected = SecurityRead.DefaultPermissions[1];
            actual = securitySettings.WindowsGroupPermissions[1];
            result = new WindowsGroupPermissionEqualityComparer().Equals(expected, actual);
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
            Assert.AreEqual("<DataList><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>", result.DataListSpecification.ToString());
            Assert.AreEqual(1, result.Actions.Count);

            var serviceAction = result.Actions[0];
            Assert.AreEqual(esb.HandlesType(), serviceAction.Name);
            Assert.AreEqual(enActionType.InvokeManagementDynamicService, serviceAction.ActionType);
            Assert.AreEqual(esb.HandlesType(), serviceAction.SourceMethod);
        }

        #endregion

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("GetResourceID")]
        public void GetResourceID_ShouldReturnEmptyGuid()
        {
            //------------Setup for test--------------------------
            var securityRead = new SecurityRead();

            //------------Execute Test---------------------------
            var resId = securityRead.GetResourceID(new Dictionary<string, StringBuilder>());
            //------------Assert Results-------------------------
            Assert.AreEqual(Guid.Empty, resId);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("GetResourceID")]
        public void GetAuthorizationContextForService_ShouldReturnContext()
        {
            //------------Setup for test--------------------------
            var securityRead = new SecurityRead();

            //------------Execute Test---------------------------
            var resId = securityRead.GetAuthorizationContextForService();
            //------------Assert Results-------------------------
            Assert.AreEqual(AuthorizationContext.Any, resId);
        }

    }
}
