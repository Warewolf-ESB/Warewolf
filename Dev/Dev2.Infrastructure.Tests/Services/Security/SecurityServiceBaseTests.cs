using System.Collections.Generic;
using Dev2.Services.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Infrastructure.Tests.Services.Security
{
    [TestClass]
    public class SecurityServiceBaseTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SecurityServiceBase_Constructor")]
        public void SecurityServiceBase_Constructor_PropertiesInitialized()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var securityServiceBase = new TestSecurityServiceBase();

            //------------Assert Results-------------------------
            Assert.IsNotNull(securityServiceBase.Permissions);
            Assert.AreEqual(0, securityServiceBase.Permissions.Count);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SecurityServiceBase_Read")]
        public void SecurityServiceBase_Read_ReadPermissionsResultIsNotNull_PermissionsUpdated()
        {
            //------------Setup for test--------------------------
            var perms1 = new List<WindowsGroupPermission>
            {
                new WindowsGroupPermission(),
                new WindowsGroupPermission()
            };
            var perms2 = new List<WindowsGroupPermission>
            {
                new WindowsGroupPermission { ResourceName = "Permission1" },
                new WindowsGroupPermission { ResourceName = "Permission2" },
                new WindowsGroupPermission { ResourceName = "Permission3" },
            };
            var securityServiceBase = new TestSecurityServiceBase { ReadPermissionsResult = perms1 };
            securityServiceBase.Read();
            Assert.AreEqual(perms1.Count, securityServiceBase.Permissions.Count);

            securityServiceBase.ReadPermissionsResult = perms2;

            //------------Execute Test---------------------------
            securityServiceBase.Read();

            //------------Assert Results-------------------------
            Assert.AreEqual(perms2.Count, securityServiceBase.Permissions.Count);
            for(int i = 0; i < perms2.Count; i++)
            {
                Assert.AreEqual(perms2[i].ResourceName, securityServiceBase.Permissions[i].ResourceName);
            }
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SecurityServiceBase_Read")]
        public void SecurityServiceBase_Read_ReadPermissionsResultIsNull_PermissionsCleared()
        {
            //------------Setup for test--------------------------
            var perms1 = new List<WindowsGroupPermission>
            {
                new WindowsGroupPermission(),
                new WindowsGroupPermission()
            };
            var securityServiceBase = new TestSecurityServiceBase { ReadPermissionsResult = perms1 };
            securityServiceBase.Read();
            Assert.AreEqual(perms1.Count, securityServiceBase.Permissions.Count);

            securityServiceBase.ReadPermissionsResult = null;

            //------------Execute Test---------------------------
            securityServiceBase.Read();

            //------------Assert Results-------------------------
            Assert.AreEqual(0, securityServiceBase.Permissions.Count);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SecurityServiceBase_Read")]
        public void SecurityServiceBase_Read_ChangeEvent_Fired()
        {
            //------------Setup for test--------------------------
            bool changedEventWasFired = false;
            var securityServiceBase = new TestSecurityServiceBase();
            securityServiceBase.Changed += (sender, args) => changedEventWasFired = true;

            //------------Execute Test---------------------------
            securityServiceBase.Read();

            //------------Assert Results-------------------------
            Assert.IsTrue(changedEventWasFired);
        }
    }
}
