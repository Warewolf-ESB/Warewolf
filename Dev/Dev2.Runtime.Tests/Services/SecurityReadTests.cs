using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.DynamicServices;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Services.Security;
using Dev2.Tests.Runtime.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Linq;

// ReSharper disable InconsistentNaming
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
        [Owner("Travis Frisinger")]
        [TestCategory("SecurityRead_Execute")]
        public void SecurityRead_Execute_WhenOldSecureConfigExist_MigratesAdministratorsToWarewolfAdministrators_ExpectSuccessfulMigration()
        {
            //------------Setup for test--------------------------
            if(File.Exists("secure.config"))
            {
                File.Delete("secure.config");
            }

            var myConfig = SecurityConfigFetcher.Fetch("secure.config");
            File.WriteAllText("secure.config", myConfig);

            //------------Assert Preconditions-------------------------
            Assert.IsTrue(File.Exists("secure.config"));
            // ensure the contents are what we expect ;)
            StringAssert.Contains(myConfig, "RQ4pIorv9CrH9j7QSj+h7SpkLO1bEXCnuzh7hcQOh1vP2LunCgC7jJNqyTQMLA6YNcl5mqdHnF9JFFU7STCYTneLCWNx2qb7GOE3Ne0Ilo8DQfcRT/3O9rutV/AXez0CgHmrDYJwB2GoC1pbgIMRrJoqbYp+rcB5Ee2sYo8ZkbaKAs3oEbUSj/Xr3GO0Y6AncMJKjW0bsfWP6Ga+dpov2KDILfJPLbSAaW8XKaOJw6U+ZuyeAos3BY93EzGHJvHCX8ZYt6x/pOr4z2crl6+FeeEjrn6QDML/Uv8A0HkNv62EikJCBuPlpytsMmaxE2Al7jrptHblWylEwCKdSNpW7IOwFuQyXQAAa2eBO4/+fmyMRV0MDkdOTRarXr4GYYD9fojqZ0v1zU3J2+zJy2grhpzjchGtVfH6il0B5Bqx4SXR1GxzsoyIVK/EEd2VKV0xve7wIcDqNgtWwq5ytWJVuR413h0+azNk7H3gT4YwNC5NuX4CqyrvdKaHVMQfDbnQqtvNYGPPdGcBAHg8Pvh3+xb8erJiPsNUXQ89qBcrdRU7VT+9WdIeS0jaFwdpCybfwa0SKCaNwPrPHXEg9MjVCUCEiruJwCRyvbnYqUOH50VVLtfaVwJaa707sQHjWIZ3vRPF1yoT3NUBiPQoxuRKn1vchuP4y+D31cm+mxrk+GhjFd+bfLX10ywhkiNLCl4hTOFhIUH3JnnVcOQ0IVaV1wA5EUW+t3lk4n4o1uZzwxtrdFd/5H6zkgQMh62I3mWzWZYJoIwZ9QT/zNpnb1Raiz7nTEFqwSozKOADbuxocmdabJLOWXZTk119gf5131i2ChEN7pLvGx3m3wQx+ngTOhkrFwzO5cPBAPhTn8GityV3iB1YF/hst5uiEgf119q7gs74LNE0k0uc+3ushPAztEpp2YKDChU6n9Y4s43KASKCkzjMYWMJeOsnatcrDbkA8Dj1akaopJpFkvySUJXXXjGg3XBXfD4hGsbjKva86isMgw94DmAV7W6pZqKo7SsUiLpmIgphqis+1wmzZYXqbPaE+y3xiA3csdver7ijCeH3q+W20UpyE2be+shBvBSbt34YNss3fP8cLNZsKLaNt1GgA9E28LkW5su5zjCNAS86YDO1Hy6/DG+aAE3HRyeNQMl06Y6sVA3lENfUc5hNFXYvCFaFABeVZNkef/LWGOMjTEqYooyPY3f4tGilRoZwLR96IkfxJZh7gSbA2TGQpsRBsjyliSgozEkSR+YYdUfo47idNg6Tu3SZ5zgqJOXzfTV+5e/29K79jSVSiWxh3AVVmrItqXrVcA1gY63+yFuOkQBtufYqWNNfAAdoXdoj5EEEHQw1InTMJIcj1LnbAopgiKW5fj0t7myvoQUkXdT4a9fc5rBsoI98cWp4cFAn+6xFIloH12APNadpttkFTEIVqWShKZXqmY1JZSDMJJcZ9cB2vOnBsBCFQraiuzyl");

            //------------Execute Test---------------------------
            var securityRead = new SecurityRead();
            var jsonPermissions = securityRead.Execute(null, null);

            File.Delete("secure.config");
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
            if(File.Exists("secure.config"))
            {
                File.Delete("secure.config");
            }
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
            Assert.IsTrue(File.Exists("secure.config"));
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
            File.WriteAllText("secure.config", @"Invalid content.");
            var securityRead = new SecurityRead();

            //------------Execute Test---------------------------
            var jsonPermissions = securityRead.Execute(null, null);
            var securitySettings = JsonConvert.DeserializeObject<SecuritySettingsTO>(jsonPermissions.ToString());

            File.Delete("secure.config");

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