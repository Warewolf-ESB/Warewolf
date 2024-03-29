/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Common.Interfaces.Enums;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Runtime.Interfaces;
using Dev2.Services.Security;
using Dev2.Tests.Runtime.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Dev2.Infrastructure.Tests.Services.Security;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    [DoNotParallelize]
    [TestCategory("CannotParallelize")]
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

            var expected = SecuritySettings.DefaultPermissions[0];
            var actual = securitySettings.WindowsGroupPermissions[0];

            var result = SecurityServiceBaseTests.WindowsGroupPermissionEquals(expected, actual);
            Assert.IsTrue(result);

            expected = SecuritySettings.DefaultPermissions[1];
            actual = securitySettings.WindowsGroupPermissions[1];
            result = SecurityServiceBaseTests.WindowsGroupPermissionEquals(expected, actual);
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

            var expected = SecuritySettings.DefaultPermissions[0];
            var actual = securitySettings.WindowsGroupPermissions[0];

            var result = SecurityServiceBaseTests.WindowsGroupPermissionEquals(expected, actual);
            Assert.IsTrue(result);

            expected = SecuritySettings.DefaultPermissions[1];
            actual = securitySettings.WindowsGroupPermissions[1];
            result = SecurityServiceBaseTests.WindowsGroupPermissionEquals(expected, actual);
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

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("SecurityRead_Execute")]
        public void SecurityRead_Execute_Given_Config_data()
        {
            //------------Setup for test--------------------------
            var serverSecuritySettingsFile = EnvironmentVariables.ServerSecuritySettingsFile;
            File.Delete(serverSecuritySettingsFile);

            var myConfig = "dYeKSekQgxWpheA6MFm8SRmXbNlDDz2Y0QoUSb+TLWJ41gfV2v1oY34y302xtLFX5kk+kgh9ZmI7C+La+yJltFtAN/pNx6q4SvUUeGVkY3GOYjjnZlbOm6ZmaQVG6/mFXXDavz4Lz7c1Ro16DDYApMX7neD2k34aSxNYLII20uRUx/LHDJJuP4fIuo3LwML/LxZD7hWqgMe0ySYKa2GJFRt0Nkt+IA6HXA2FcqDM14SD3w36UKQuB3IutLmHi4F0+RilU6oyWsgfKL2qSqm/qyfdOzTnTgMO7C39flS5DE7ORe7Fl+TtOKM3CuHUFN0/AIxhU6IkR7weYr1LL0TQdE4vgB877YUELD18VUWQfwUav3ggOEZoU8OqDUJq5hqJA8Gyspo8SSxPvkcelRwnSQThCtnYd979ejk9OCrUnPE+9zzeHyiVhWpqM6gyYmhgUNkZcJLrB3O0+QAr5pwB7iMOWoNVHBT46XykXbL9Tc0cXZECqR+N+E2c5GWv5QhxMBygnzo4U1/Yk7WUNRdru35WWbLjebaA/s2aotx2wrPcvP5Qd8c1VqKE3ZH3DpiWze0dWF7/udFTIYNO+9CMCoe6wEOg6vHtn+FzlRBcNy/bLPdgJaVKOV2aJxBNhk0RrtR8BkTtVqu2Hap4aGXejOGcxcPtb/Qe7mSBmHmaBUgETx26Gck8R5eEJ93hE6LitfgEA0oTuN/jTDnhS68RYNqlsjoRd+ezXsPuHXf5ZzHsuuj3C8Tjawzkep9wIY9XirM57gEiPpOF6n0Cxu9CwulcL9ZnOOa8Pl5tH+jAaYRF+AHtwuRFogxHhmeY02v0BLQUdONqVi4vUtcZJSbleHGkv3iMVXxybFp0Bn4GynZRNsRh9FV+sLOeLXaUdLSUzBLeatzOuC61bAN6oiHfwr2qlgsBWfB3S2qXcheoWY1JhiCbImRDJoyfSuEwmYTlFdcj/hZ+yeKBQOvtf10wzfNQwiJWQ05iEdeOARrkeACb8ZLtRYk6lYyd4J3qviBnZsXToz5mo936Cf4sg0SeqYX+1Ze3tP5J4v/bsnlhf040DcAxnoHJ0bf4FfHi+GfkvoaAIsmHnCXlfp7OrXvu9cUGTU6VbyK4YBdxrIp7Bz7tPk9yGxYRz5pIkXPIsj5L9cBZ6ohHqcpCsipICst8jGdZTVy3sc3UJgtuwV0ygmXZhKeqKcFwJ165SQL9BTLA7wVaDipwZwqfHeQ8q3FYWiyUQepISSpxN4xIsFYMSMSw4vRXOtDp24Jw69bXKflV0FB5OVQvNFtOJtpJNhieEaqBXC9Yd+UapyeD9gzP22uDhfe1Nai8ZY0vq5LFyc30PEdiXMdqX6ncjav8Y/aq46N7BBmql0kFdF8hrWSkJkJa/zgk2Q5l1q4It02Z24Wno25Wocy02qgRzRiOjFvGDutkv+oxspGAY6cJmxK9W/uIRHOIW44P3CKi4VwySUHZuBRSIamLlfLZrT2x9BRQyWCdR/AxX3+jWs9F8T3rTnpbv8D5MZsjXvxwv0CGt7plPJEfXw8DMsbgJTFHyyIjq0FnyDNlhSPwEU9Eny5Ex1QoUFtcEx9AKCSay1oWmCwm";
            File.WriteAllText(serverSecuritySettingsFile, myConfig);

            //------------Assert Preconditions-------------------------
            Assert.IsTrue(File.Exists(serverSecuritySettingsFile));
            // ensure the contents are what we expect ;)

            var mockCatalog = new Mock<IResourceCatalog>();
            mockCatalog.Setup(a => a.GetResourcePath(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns("");
            //------------Execute Test---------------------------
            var securityRead = new SecurityRead();
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
        }
    }
}
