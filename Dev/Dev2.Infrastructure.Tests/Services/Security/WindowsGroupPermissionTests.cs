
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Common.Interfaces.Security;
using Dev2.Services.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

// ReSharper disable InconsistentNaming
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
        [Owner("Hagashen Naidu")]
        [TestCategory("WindowsGroupPermission_IsBuiltInGuests")]
        public void WindowsGroupPermission_IsBuiltInGuests_IsNotServer_False()
        {
            //------------Setup for test--------------------------            

            //------------Execute Test---------------------------
            var p = new WindowsGroupPermission { IsServer = false, WindowsGroup = WindowsGroupPermission.BuiltInGuestsText };

            //------------Assert Results-------------------------
            Assert.IsFalse(p.IsBuiltInGuests);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("WindowsGroupPermission_IsBuiltInGuests")]
        public void WindowsGroupPermission_IsBuiltInGuests_IsServerAndWindowsGroupIsNotBuiltInAdministrators_False()
        {
            //------------Setup for test--------------------------            

            //------------Execute Test---------------------------
            var p = new WindowsGroupPermission { IsServer = true, WindowsGroup = "xxxx" };

            //------------Assert Results-------------------------
            Assert.IsFalse(p.IsBuiltInGuests);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("WindowsGroupPermission_IsBuiltInGuests")]
        public void WindowsGroupPermission_IsBuiltInGuests_IsServerAndWindowsGroupIsBuiltInAdministrators_True()
        {
            //------------Setup for test--------------------------            

            //------------Execute Test---------------------------
            var p = new WindowsGroupPermission { IsServer = true, WindowsGroup = WindowsGroupPermission.BuiltInGuestsText };

            //------------Assert Results-------------------------
            Assert.IsTrue(p.IsBuiltInGuests);
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
        [TestCategory("WindowsGroupPermission_CreateAdministrators")]
        public void WindowsGroupPermission_CreateAdministrators_IsNotNull()
        {
            //------------Setup for test--------------------------            

            //------------Execute Test---------------------------
            var p = WindowsGroupPermission.CreateAdministrators();

            //------------Assert Results-------------------------
            Assert.IsTrue(p.IsServer);
            Assert.IsTrue(p.View);
            Assert.IsTrue(p.Execute);
            Assert.IsTrue(p.Contribute);
            Assert.IsTrue(p.DeployTo);
            Assert.IsTrue(p.DeployFrom);
            Assert.IsTrue(p.Administrator);
            Assert.AreEqual(WindowsGroupPermission.BuiltInAdministratorsText, p.WindowsGroup);
            Assert.AreEqual(Guid.Empty, p.ResourceID);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("WindowsGroupPermission_CreateGuestGroup")]
        public void WindowsGroupPermission_CreateGuestGroup_IsNotNull_NoAccess()
        {
            //------------Setup for test--------------------------


            //------------Execute Test---------------------------
            var guestPermissions = WindowsGroupPermission.CreateGuests();
            //------------Assert Results-------------------------
            Assert.IsNotNull(guestPermissions);
            Assert.IsTrue(guestPermissions.IsServer);
            Assert.IsFalse(guestPermissions.View);
            Assert.IsFalse(guestPermissions.Execute);
            Assert.IsFalse(guestPermissions.Contribute);
            Assert.IsFalse(guestPermissions.DeployTo);
            Assert.IsFalse(guestPermissions.DeployFrom);
            Assert.IsFalse(guestPermissions.Administrator);
            Assert.AreEqual(WindowsGroupPermission.BuiltInGuestsText, guestPermissions.WindowsGroup);
            Assert.AreEqual(Guid.Empty, guestPermissions.ResourceID);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("WindowsGroupPermission_CreateEveryone")]
        public void WindowsGroupPermission_CreateEveryone_IsNotNull()
        {
            //------------Setup for test--------------------------            

            //------------Execute Test---------------------------
            var p = WindowsGroupPermission.CreateEveryone();

            //------------Assert Results-------------------------
            Assert.IsTrue(p.IsServer);
            Assert.IsTrue(p.View);
            Assert.IsTrue(p.Execute);
            Assert.IsTrue(p.Contribute);
            Assert.IsTrue(p.DeployTo);
            Assert.IsTrue(p.DeployFrom);
            Assert.IsTrue(p.Administrator);
            Assert.AreEqual("Everyone", p.WindowsGroup);
            Assert.AreEqual(Guid.Empty, p.ResourceID);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("WindowsGroupPermissions_CanRemove")]
        public void WindowsGroupPermissions_CanRemove_IsPublic_False()
        {
            //------------Setup for test--------------------------
            var guestPermissions = WindowsGroupPermission.CreateGuests();
            //------------Execute Test---------------------------
            var canRemove = guestPermissions.CanRemove;
            //------------Assert Results-------------------------
            Assert.IsFalse(canRemove);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("WindowsGroupPermissions_CanRemove")]
        public void WindowsGroupPermissions_CanRemove_NoWindowsGroupName_False()
        {
            //------------Setup for test--------------------------
            var guestPermissions = WindowsGroupPermission.CreateGuests();
            guestPermissions.WindowsGroup = "";
            //------------Execute Test---------------------------
            var canRemove = guestPermissions.CanRemove;
            //------------Assert Results-------------------------
            Assert.IsFalse(canRemove);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("WindowsGroupPermissions_CanRemove")]
        public void WindowsGroupPermissions_CanRemove_Server_True()
        {
            //------------Setup for test--------------------------
            var p = new WindowsGroupPermission { IsServer = true, WindowsGroup = "xxx" };
            //------------Execute Test---------------------------
            var canRemove = p.CanRemove;
            //------------Assert Results-------------------------
            Assert.IsTrue(canRemove);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("WindowsGroupPermissions_CanRemove")]
        public void WindowsGroupPermissions_CanRemove_Resource_True()
        {
            //------------Setup for test--------------------------
            var p = new WindowsGroupPermission { IsServer = false, ResourceID = Guid.NewGuid(), WindowsGroup = "xxx" };
            //------------Execute Test---------------------------
            var canRemove = p.CanRemove;
            //------------Assert Results-------------------------
            Assert.IsTrue(canRemove);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("WindowsGroup_MethodName")]
        public void WindowsGroup_RemoveRow_DeleteFalse_DeleteTrueEnableCellEditingFalse()
        {
            //------------Setup for test--------------------------
            var p = new WindowsGroupPermission { IsDeleted = false };
            //------------Execute Test---------------------------
            p.RemoveRow.Execute(null);
            //------------Assert Results-------------------------
            Assert.IsTrue(p.IsDeleted);
            Assert.IsFalse(p.EnableCellEditing);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("WindowsGroupPermissions_CanRemoveRow")]
        public void WindowsGroupPermissions_CanRemoveRow_IsServerChanges_CanExecuteChangedFires()
        {
            //------------Setup for test--------------------------
            var p = new WindowsGroupPermission { IsDeleted = false };
            var hitCount=0;
            p.RemoveRow.CanExecuteChanged += (sender, args) =>
            {
                hitCount++;
            };
            //------------Execute Test---------------------------
            p.IsServer = true;
            //------------Assert Results-------------------------
            Assert.AreEqual(1,hitCount);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("WindowsGroupPermissions_CanRemoveRow")]
        public void WindowsGroupPermissions_CanRemoveRow_WindowsGroupChanges_CanExecuteChangedFires()
        {
            //------------Setup for test--------------------------
            var p = new WindowsGroupPermission { IsDeleted = false };
            var hitCount=0;
            p.RemoveRow.CanExecuteChanged += (sender, args) =>
            {
                hitCount++;
            };
            //------------Execute Test---------------------------
            p.WindowsGroup = "TestGroup";
            //------------Assert Results-------------------------
            Assert.AreEqual(1,hitCount);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("WindowsGroup_MethodName")]
        public void WindowsGroup_RemoveRow_DeleteTrue_DeletefalseEnableCellEditingTrue()
        {
            //------------Setup for test--------------------------
            var p = new WindowsGroupPermission { IsDeleted = true };
            //------------Execute Test---------------------------
            p.RemoveRow.Execute(null);
            //------------Assert Results-------------------------
            Assert.IsFalse(p.IsDeleted);
            Assert.IsTrue(p.EnableCellEditing);
        }
    }
}
