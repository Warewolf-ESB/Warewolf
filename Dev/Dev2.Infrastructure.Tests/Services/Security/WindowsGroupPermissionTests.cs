using System;
using Dev2.Services.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Infrastructure.Tests.Services.Security
{
    [TestClass]
    public class WindowsGroupPermissionTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WindowsGroupPermission_Permissions")]
        public void WindowsGroupPermission_Permissions_Get_CorrectPermissionsReturned()
        {
            //------------Setup for test--------------------------            
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.AreEqual(Permissions.None, new WindowsGroupPermission().Permissions);
            Assert.AreEqual(Permissions.View, new WindowsGroupPermission { View = true }.Permissions);
            Assert.AreEqual(Permissions.Execute, new WindowsGroupPermission { Execute = true }.Permissions);
            Assert.AreEqual(Permissions.Contribute, new WindowsGroupPermission { Contribute = true }.Permissions);
            Assert.AreEqual(Permissions.DeployTo, new WindowsGroupPermission { DeployTo = true }.Permissions);
            Assert.AreEqual(Permissions.DeployFrom, new WindowsGroupPermission { DeployFrom = true }.Permissions);
            Assert.AreEqual(Permissions.Administrator, new WindowsGroupPermission { Administrator = true }.Permissions);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WindowsGroupPermission_Permissions")]
        public void WindowsGroupPermission_Permissions_SetNone_CorrectlyApplied()
        {
            //------------Setup for test--------------------------            
            
            //------------Execute Test---------------------------
            var p = new WindowsGroupPermission { Permissions = Permissions.None };

            //------------Assert Results-------------------------
            Assert.IsFalse(p.View);
            Assert.IsFalse(p.Execute);
            Assert.IsFalse(p.Contribute);
            Assert.IsFalse(p.DeployTo);
            Assert.IsFalse(p.DeployFrom);
            Assert.IsFalse(p.Administrator);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WindowsGroupPermission_Permissions")]
        public void WindowsGroupPermission_Permissions_SetView_CorrectlyApplied()
        {
            //------------Setup for test--------------------------            
            
            //------------Execute Test---------------------------
            var p = new WindowsGroupPermission { Permissions = Permissions.View };

            //------------Assert Results-------------------------
            Assert.IsTrue(p.View);
            Assert.IsFalse(p.Execute);
            Assert.IsFalse(p.Contribute);
            Assert.IsFalse(p.DeployTo);
            Assert.IsFalse(p.DeployFrom);
            Assert.IsFalse(p.Administrator);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WindowsGroupPermission_Permissions")]
        public void WindowsGroupPermission_Permissions_SetExecute_CorrectlyApplied()
        {
            //------------Setup for test--------------------------            

            //------------Execute Test---------------------------
            var p = new WindowsGroupPermission { Permissions = Permissions.Execute };

            //------------Assert Results-------------------------
            Assert.IsFalse(p.View);
            Assert.IsTrue(p.Execute);
            Assert.IsFalse(p.Contribute);
            Assert.IsFalse(p.DeployTo);
            Assert.IsFalse(p.DeployFrom);
            Assert.IsFalse(p.Administrator);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WindowsGroupPermission_Permissions")]
        public void WindowsGroupPermission_Permissions_SetContribute_CorrectlyApplied()
        {
            //------------Setup for test--------------------------            

            //------------Execute Test---------------------------
            var p = new WindowsGroupPermission { Permissions = Permissions.Contribute };

            //------------Assert Results-------------------------
            Assert.IsFalse(p.View);
            Assert.IsFalse(p.Execute);
            Assert.IsTrue(p.Contribute);
            Assert.IsFalse(p.DeployTo);
            Assert.IsFalse(p.DeployFrom);
            Assert.IsFalse(p.Administrator);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WindowsGroupPermission_Permissions")]
        public void WindowsGroupPermission_Permissions_SetDeployTo_CorrectlyApplied()
        {
            //------------Setup for test--------------------------            

            //------------Execute Test---------------------------
            var p = new WindowsGroupPermission { Permissions = Permissions.DeployTo };

            //------------Assert Results-------------------------
            Assert.IsFalse(p.View);
            Assert.IsFalse(p.Execute);
            Assert.IsFalse(p.Contribute);
            Assert.IsTrue(p.DeployTo);
            Assert.IsFalse(p.DeployFrom);
            Assert.IsFalse(p.Administrator);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WindowsGroupPermission_Permissions")]
        public void WindowsGroupPermission_Permissions_SetDeployFrom_CorrectlyApplied()
        {
            //------------Setup for test--------------------------            

            //------------Execute Test---------------------------
            var p = new WindowsGroupPermission { Permissions = Permissions.DeployFrom };

            //------------Assert Results-------------------------
            Assert.IsFalse(p.View);
            Assert.IsFalse(p.Execute);
            Assert.IsFalse(p.Contribute);
            Assert.IsFalse(p.DeployTo);
            Assert.IsTrue(p.DeployFrom);
            Assert.IsFalse(p.Administrator);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WindowsGroupPermission_Permissions")]
        public void WindowsGroupPermission_Permissions_SetAdministrator_CorrectlyApplied()
        {
            //------------Setup for test--------------------------            

            //------------Execute Test---------------------------
            var p = new WindowsGroupPermission { Permissions = Permissions.Administrator };

            //------------Assert Results-------------------------
            Assert.IsFalse(p.View);
            Assert.IsFalse(p.Execute);
            Assert.IsFalse(p.Contribute);
            Assert.IsFalse(p.DeployTo);
            Assert.IsFalse(p.DeployFrom);
            Assert.IsTrue(p.Administrator);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WindowsGroupPermission_IsBuiltInAdministrators")]
        public void WindowsGroupPermission_IsBuiltInAdministrators_IsNotServer_False()
        {
            //------------Setup for test--------------------------            

            //------------Execute Test---------------------------
            var p = new WindowsGroupPermission { IsServer = false, WindowsGroup = WindowsGroupPermission.BuiltInAdministratorsText };

            //------------Assert Results-------------------------
            Assert.IsFalse(p.IsBuiltInAdministrators);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WindowsGroupPermission_IsBuiltInAdministrators")]
        public void WindowsGroupPermission_IsBuiltInAdministrators_IsServerAndWindowsGroupIsNotBuiltInAdministrators_False()
        {
            //------------Setup for test--------------------------            

            //------------Execute Test---------------------------
            var p = new WindowsGroupPermission { IsServer = true, WindowsGroup = "xxxx" };

            //------------Assert Results-------------------------
            Assert.IsFalse(p.IsBuiltInAdministrators);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WindowsGroupPermission_IsBuiltInAdministrators")]
        public void WindowsGroupPermission_IsBuiltInAdministrators_IsServerAndWindowsGroupIsBuiltInAdministrators_True()
        {
            //------------Setup for test--------------------------            

            //------------Execute Test---------------------------
            var p = new WindowsGroupPermission { IsServer = true, WindowsGroup = WindowsGroupPermission.BuiltInAdministratorsText };

            //------------Assert Results-------------------------
            Assert.IsTrue(p.IsBuiltInAdministrators);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WindowsGroupPermission_IsValid")]
        public void WindowsGroupPermission_IsValid_IsServerAndWindowsGroupIsEmpty_False()
        {
            //------------Setup for test--------------------------            

            //------------Execute Test---------------------------
            var p = new WindowsGroupPermission { IsServer = true, WindowsGroup = string.Empty };

            //------------Assert Results-------------------------
            Assert.IsFalse(p.IsValid);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WindowsGroupPermission_IsValid")]
        public void WindowsGroupPermission_IsValid_IsServerAndWindowsGroupIsNotEmpty_True()
        {
            //------------Setup for test--------------------------            

            //------------Execute Test---------------------------
            var p = new WindowsGroupPermission { IsServer = true, WindowsGroup = "xxx" };

            //------------Assert Results-------------------------
            Assert.IsTrue(p.IsValid);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WindowsGroupPermission_IsValid")]
        public void WindowsGroupPermission_IsValid_IsNotServerAndWindowsGroupIsEmpty_False()
        {
            //------------Setup for test--------------------------            

            //------------Execute Test---------------------------
            var p = new WindowsGroupPermission { IsServer = false, WindowsGroup = string.Empty, ResourceName = "xxx" };

            //------------Assert Results-------------------------
            Assert.IsFalse(p.IsValid);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WindowsGroupPermission_IsValid")]
        public void WindowsGroupPermission_IsValid_IsNotServerAndResourceNameIsEmpty_False()
        {
            //------------Setup for test--------------------------            

            //------------Execute Test---------------------------
            var p = new WindowsGroupPermission { IsServer = false, WindowsGroup = "xxx", ResourceName = string.Empty };

            //------------Assert Results-------------------------
            Assert.IsFalse(p.IsValid);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WindowsGroupPermission_IsValid")]
        public void WindowsGroupPermission_IsValid_IsNotServerAndWindowsGroupAndResourceNameAreNotEmpty_True()
        {
            //------------Setup for test--------------------------            

            //------------Execute Test---------------------------
            var p = new WindowsGroupPermission { IsServer = false, WindowsGroup = "xxx", ResourceName = "xxx" };

            //------------Assert Results-------------------------
            Assert.IsTrue(p.IsValid);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WindowsGroupPermission_CreateDefault")]
        public void WindowsGroupPermission_CreateDefault_IsNotNull()
        {
            //------------Setup for test--------------------------            

            //------------Execute Test---------------------------
            var p = WindowsGroupPermission.CreateDefault();

            //------------Assert Results-------------------------
            Assert.IsTrue(p.IsServer);
            Assert.IsFalse(p.View);
            Assert.IsFalse(p.Execute);
            Assert.IsTrue(p.Contribute);
            Assert.IsTrue(p.DeployTo);
            Assert.IsTrue(p.DeployFrom);
            Assert.IsTrue(p.Administrator);
            Assert.AreEqual(WindowsGroupPermission.BuiltInAdministratorsText, p.WindowsGroup);
            Assert.AreEqual(Guid.Empty, p.ResourceID);
        }
    }
}
