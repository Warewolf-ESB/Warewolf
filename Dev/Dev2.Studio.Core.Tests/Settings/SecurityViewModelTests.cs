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
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using CubicOrange.Windows.Forms.ActiveDirectory;
using Dev2.Common;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Dialogs;
using Dev2.Services.Security;
using Dev2.Settings.Security;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

// ReSharper disable InconsistentNaming
namespace Dev2.Core.Tests.Settings
{
    [TestClass]
    public class SecurityViewModelTests
    {
        [TestInitialize]
        public void setup()
        {

            AppSettings.LocalHost = "http://localhost:3142";
            EnvironmentRepository.Instance.ActiveEnvironment = new Mock<IEnvironmentModel>().Object;
        }



        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SecurityViewModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SecurityViewModel_Constructor_DirectoryObjectPickerDialogIsNull_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            // ReSharper disable ObjectCreationAsStatement
            new SecurityViewModel(new SecuritySettingsTO(), null, new Mock<IWin32Window>().Object, new Mock<IEnvironmentModel>().Object, () => new Mock<IResourcePickerDialog>().Object);
            // ReSharper restore ObjectCreationAsStatement

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SecurityViewModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SecurityViewModel_Constructor_ParentWindowIsNull_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            // ReSharper disable ObjectCreationAsStatement
            new SecurityViewModel(new SecuritySettingsTO(), new Mock<DirectoryObjectPickerDialog>().Object, null, new Mock<IEnvironmentModel>().Object, () => new Mock<IResourcePickerDialog>().Object);
            // ReSharper restore ObjectCreationAsStatement

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SecurityViewModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SecurityViewModel_Constructor_EnvironmentIsNull_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            // ReSharper disable ObjectCreationAsStatement
            new SecurityViewModel(new SecuritySettingsTO(), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, null, () => new Mock<IResourcePickerDialog>().Object);
            // ReSharper restore ObjectCreationAsStatement

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SecurityViewModel_Constructor")]
        public void SecurityViewModel_Constructor_SecuritySettingsIsNull_PropertiesInitialized()
        {
            Verify_Constructor_InitializesProperties(null);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SecurityViewModel_Constructor")]
        public void SecurityViewModel_Constructor_AllParametersValid_PropertiesInitialized()
        {
            //------------Setup for test--------------------------
            var permissions = new[]
            {
                new WindowsGroupPermission
                {
                    IsServer = true, WindowsGroup = "Deploy Admins",
                    View = false, Execute = false, Contribute = false, DeployTo = true, DeployFrom = true, Administrator = false
                },

                new WindowsGroupPermission
                {
                    ResourceID = Guid.NewGuid(), ResourceName = "Category1\\Workflow1",
                    WindowsGroup = "Windows Group 1", View = false, Execute = true, Contribute = false
                }
            };
            var securitySettingsTO = new SecuritySettingsTO(permissions);

            Verify_Constructor_InitializesProperties(securitySettingsTO);
        }

        static void Verify_Constructor_InitializesProperties(SecuritySettingsTO securitySettingsTO)
        {
            //------------Execute Test---------------------------
            var viewModel = new SecurityViewModel(securitySettingsTO, new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IEnvironmentModel>().Object, () => new Mock<IResourcePickerDialog>().Object);

            //------------Assert Results-------------------------
            Assert.IsTrue(viewModel.CloseHelpCommand.CanExecute(null));
            Assert.IsNotNull(viewModel.PickResourceCommand);
            Assert.IsTrue(viewModel.PickResourceCommand.CanExecute(null));
            Assert.IsNotNull(viewModel.PickWindowsGroupCommand);
            Assert.IsTrue(viewModel.PickWindowsGroupCommand.CanExecute(null));
            Assert.IsNotNull(viewModel.ServerPermissions);
            Assert.IsNotNull(viewModel.ResourcePermissions);

            var serverPerms = securitySettingsTO?.WindowsGroupPermissions.Where(p => p.IsServer).ToList() ?? new List<WindowsGroupPermission>();
            var resourcePerms = securitySettingsTO?.WindowsGroupPermissions.Where(p => !p.IsServer).ToList() ?? new List<WindowsGroupPermission>();

            // constructor adds an extra "new"  permission
            Assert.AreEqual(serverPerms.Count + 1, viewModel.ServerPermissions.Count);
            Assert.AreEqual(resourcePerms.Count + 1, viewModel.ResourcePermissions.Count);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SecurityViewModel_OnPermissionPropertyChanged")]
        public void SecurityViewModel_OnPermissionPropertyChanged_PermissionChanged_IsDirtyIsTrue()
        {
            //------------Setup for test--------------------------
            var permission = new WindowsGroupPermission
            {
                IsServer = true,
                WindowsGroup = "Deploy Admins",
                View = false,
                Execute = false,
                Contribute = false,
                DeployTo = true,
                DeployFrom = true,
                Administrator = false
            };
            var viewModel = new SecurityViewModel(new SecuritySettingsTO(new[] { permission }), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IEnvironmentModel>().Object, () => new Mock<IResourcePickerDialog>().Object);

            Assert.IsFalse(viewModel.IsDirty);

            //------------Execute Test---------------------------
            permission.Administrator = true;

            //------------Assert Results-------------------------
            Assert.IsTrue(viewModel.IsDirty);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SecurityViewModel_OnPermissionPropertyChanged")]
        public void SecurityViewModel_OnPermissionPropertyChanged_MakeContributeTrue_MakesViewExecuteTrue()
        {
            //------------Setup for test--------------------------
            var permission = new WindowsGroupPermission
            {
                IsServer = false,
                WindowsGroup = "Deploy Admins",
                View = false,
                Execute = false,
                Contribute = false,
            };
            var viewModel = new SecurityViewModel(new SecuritySettingsTO(new[] { permission }), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IEnvironmentModel>().Object, () => new Mock<IResourcePickerDialog>().Object);

            Assert.IsFalse(viewModel.IsDirty);

            //------------Execute Test---------------------------
            permission.Contribute = true;

            //------------Assert Results-------------------------
            Assert.IsTrue(permission.Contribute);
            Assert.IsTrue(permission.View);
            Assert.IsTrue(permission.Execute);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SecurityViewModel_OnPermissionPropertyChanged")]
        public void SecurityViewModel_OnPermissionPropertyChanged_WithContributeTrue_MakeViewFalse_MakesContributeFalse()
        {
            //------------Setup for test--------------------------
            var permission = new WindowsGroupPermission
            {
                IsServer = false,
                WindowsGroup = "Deploy Admins",
                View = true,
                Execute = true,
                Contribute = true,
            };
            var viewModel = new SecurityViewModel(new SecuritySettingsTO(new[] { permission }), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IEnvironmentModel>().Object, () => new Mock<IResourcePickerDialog>().Object);

            Assert.IsFalse(viewModel.IsDirty);

            //------------Execute Test---------------------------
            permission.View = false;

            //------------Assert Results-------------------------
            Assert.IsFalse(permission.Contribute);
            Assert.IsFalse(permission.View);
            Assert.IsTrue(permission.Execute);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SecurityViewModel_OnPermissionPropertyChanged")]
        public void SecurityViewModel_OnPermissionPropertyChanged_WithContributeTrue_MakeExecuteFalse_MakesContributeFalse()
        {
            //------------Setup for test--------------------------
            var permission = new WindowsGroupPermission
            {
                IsServer = false,
                WindowsGroup = "Deploy Admins",
                View = true,
                Execute = true,
                Contribute = true,
            };
            var viewModel = new SecurityViewModel(new SecuritySettingsTO(new[] { permission }), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IEnvironmentModel>().Object, () => new Mock<IResourcePickerDialog>().Object);

            Assert.IsFalse(viewModel.IsDirty);

            //------------Execute Test---------------------------
            permission.Execute = false;

            //------------Assert Results-------------------------
            Assert.IsFalse(permission.Contribute);
            Assert.IsTrue(permission.View);
            Assert.IsFalse(permission.Execute);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SecurityViewModel_OnPermissionPropertyChanged")]
        public void SecurityViewModel_OnPermissionPropertyChanged_MakeAdministratorTrue_MakesAllOtherPermissionsTrue()
        {
            //------------Setup for test--------------------------
            var permission = new WindowsGroupPermission
            {
                IsServer = true,
                WindowsGroup = "Deploy Admins",
                DeployFrom = false,
                DeployTo = false,
                Administrator = false,
                Contribute = false,
                View = false,
                Execute = false
            };
            var viewModel = new SecurityViewModel(new SecuritySettingsTO(new[] { permission }), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IEnvironmentModel>().Object, () => new Mock<IResourcePickerDialog>().Object);

            Assert.IsFalse(viewModel.IsDirty);

            //------------Execute Test---------------------------
            permission.Administrator = true;

            //------------Assert Results-------------------------
            Assert.IsTrue(permission.Contribute);
            Assert.IsTrue(permission.View);
            Assert.IsTrue(permission.Execute);
            Assert.IsTrue(permission.Administrator);
            Assert.IsTrue(permission.DeployFrom);
            Assert.IsTrue(permission.DeployTo);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SecurityViewModel_OnPermissionPropertyChanged")]
        public void SecurityViewModel_OnPermissionPropertyChanged_MakeDeployFromFalse_MakesAdministratorPermissionFalse()
        {
            //------------Setup for test--------------------------
            var permission = new WindowsGroupPermission
            {
                IsServer = true,
                WindowsGroup = "Deploy Admins",
                DeployFrom = true,
                DeployTo = true,
                Administrator = true,
                Contribute = true,
                View = true,
                Execute = true
            };
            var viewModel = new SecurityViewModel(new SecuritySettingsTO(new[] { permission }), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IEnvironmentModel>().Object, () => new Mock<IResourcePickerDialog>().Object);

            Assert.IsFalse(viewModel.IsDirty);

            //------------Execute Test---------------------------
            permission.DeployFrom = false;

            //------------Assert Results-------------------------
            Assert.IsTrue(permission.Contribute);
            Assert.IsTrue(permission.View);
            Assert.IsTrue(permission.Execute);
            Assert.IsFalse(permission.Administrator);
            Assert.IsFalse(permission.DeployFrom);
            Assert.IsTrue(permission.DeployTo);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SecurityViewModel_OnPermissionPropertyChanged")]
        public void SecurityViewModel_OnPermissionPropertyChanged_MakeDeployToFalse_MakesAdministratorPermissionFalse()
        {
            //------------Setup for test--------------------------
            var permission = new WindowsGroupPermission
            {
                IsServer = true,
                WindowsGroup = "Deploy Admins",
                DeployFrom = true,
                DeployTo = true,
                Administrator = true,
                Contribute = true,
                View = true,
                Execute = true
            };
            var viewModel = new SecurityViewModel(new SecuritySettingsTO(new[] { permission }), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IEnvironmentModel>().Object, () => new Mock<IResourcePickerDialog>().Object);

            Assert.IsFalse(viewModel.IsDirty);

            //------------Execute Test---------------------------
            permission.DeployTo = false;

            //------------Assert Results-------------------------
            Assert.IsTrue(permission.Contribute);
            Assert.IsTrue(permission.View);
            Assert.IsTrue(permission.Execute);
            Assert.IsFalse(permission.Administrator);
            Assert.IsTrue(permission.DeployFrom);
            Assert.IsFalse(permission.DeployTo);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SecurityViewModel_OnPermissionPropertyChanged")]
        public void SecurityViewModel_OnPermissionPropertyChanged_MakeContributeFalse_MakesAdministratorPermissionFalse()
        {
            //------------Setup for test--------------------------
            var permission = new WindowsGroupPermission
            {
                IsServer = true,
                WindowsGroup = "Deploy Admins",
                DeployFrom = true,
                DeployTo = true,
                Administrator = true,
                Contribute = true,
                View = true,
                Execute = true
            };
            var viewModel = new SecurityViewModel(new SecuritySettingsTO(new[] { permission }), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IEnvironmentModel>().Object, () => new Mock<IResourcePickerDialog>().Object);

            Assert.IsFalse(viewModel.IsDirty);

            //------------Execute Test---------------------------
            permission.Contribute = false;

            //------------Assert Results-------------------------
            Assert.IsFalse(permission.Contribute);
            Assert.IsTrue(permission.View);
            Assert.IsTrue(permission.Execute);
            Assert.IsFalse(permission.Administrator);
            Assert.IsTrue(permission.DeployFrom);
            Assert.IsTrue(permission.DeployTo);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SecurityViewModel_OnPermissionPropertyChanged")]
        public void SecurityViewModel_OnPermissionPropertyChanged_MakeViewFalse_MakesAdministratorPermissionFalse()
        {
            //------------Setup for test--------------------------
            var permission = new WindowsGroupPermission
            {
                IsServer = true,
                WindowsGroup = "Deploy Admins",
                DeployFrom = true,
                DeployTo = true,
                Administrator = true,
                Contribute = true,
                View = true,
                Execute = true
            };
            var viewModel = new SecurityViewModel(new SecuritySettingsTO(new[] { permission }), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IEnvironmentModel>().Object, () => new Mock<IResourcePickerDialog>().Object);

            Assert.IsFalse(viewModel.IsDirty);

            //------------Execute Test---------------------------
            permission.View = false;

            //------------Assert Results-------------------------
            Assert.IsFalse(permission.Contribute);
            Assert.IsFalse(permission.View);
            Assert.IsTrue(permission.Execute);
            Assert.IsFalse(permission.Administrator);
            Assert.IsTrue(permission.DeployFrom);
            Assert.IsTrue(permission.DeployTo);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SecurityViewModel_OnPermissionPropertyChanged")]
        public void SecurityViewModel_OnPermissionPropertyChanged_MakeExecuteFalse_MakesAdministratorPermissionFalse()
        {
            //------------Setup for test--------------------------
            var permission = new WindowsGroupPermission
            {
                IsServer = true,
                WindowsGroup = "Deploy Admins",
                DeployFrom = true,
                DeployTo = true,
                Administrator = true,
                Contribute = true,
                View = true,
                Execute = true
            };
            var viewModel = new SecurityViewModel(new SecuritySettingsTO(new[] { permission }), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IEnvironmentModel>().Object, () => new Mock<IResourcePickerDialog>().Object);

            Assert.IsFalse(viewModel.IsDirty);

            //------------Execute Test---------------------------
            permission.Execute = false;

            //------------Assert Results-------------------------
            Assert.IsFalse(permission.Contribute);
            Assert.IsTrue(permission.View);
            Assert.IsFalse(permission.Execute);
            Assert.IsFalse(permission.Administrator);
            Assert.IsTrue(permission.DeployFrom);
            Assert.IsTrue(permission.DeployTo);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SecurityViewModel_OnPermissionPropertyChanged")]
        public void SecurityViewModel_OnPermissionPropertyChanged_ServerPermissionWindowsGroupChangedToNonEmptyAndIsNew_NewServerPermissionIsAdded()
        {
            //------------Setup for test--------------------------
            var viewModel = new SecurityViewModel(new SecuritySettingsTO(), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IEnvironmentModel>().Object, () => new Mock<IResourcePickerDialog>().Object);

            Assert.AreEqual(1, viewModel.ServerPermissions.Count);
            Assert.IsTrue(viewModel.ServerPermissions[0].IsNew);

            //------------Execute Test---------------------------
            viewModel.ServerPermissions[0].WindowsGroup = "xxx";

            //------------Assert Results-------------------------
            Assert.IsFalse(viewModel.ServerPermissions[0].IsNew);

            Assert.AreEqual(2, viewModel.ServerPermissions.Count);
            Assert.IsTrue(viewModel.ServerPermissions[1].IsNew);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SecurityViewModel_OnPermissionPropertyChanged")]
        public void SecurityViewModel_OnPermissionPropertyChanged_ServerPermissionWindowsGroupChangedToEmptyAndIsNotNew_ServerPermissionIsRemoved()
        {
            //------------Setup for test--------------------------
            var permission = new WindowsGroupPermission
            {
                IsServer = true,
                WindowsGroup = "Deploy Admins",
                View = false,
                Execute = false,
                Contribute = false,
                DeployTo = true,
                DeployFrom = true,
                Administrator = false
            };
            var viewModel = new SecurityViewModel(new SecuritySettingsTO(new[] { permission }), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IEnvironmentModel>().Object, () => new Mock<IResourcePickerDialog>().Object);

            Assert.AreEqual(2, viewModel.ServerPermissions.Count);
            Assert.IsFalse(viewModel.ServerPermissions[0].IsNew);
            Assert.IsTrue(viewModel.ServerPermissions[1].IsNew);
            Assert.AreSame(permission, viewModel.ServerPermissions[0]);

            //------------Execute Test---------------------------
            viewModel.ServerPermissions[0].WindowsGroup = "";

            //------------Assert Results-------------------------
            Assert.AreEqual(1, viewModel.ServerPermissions.Count);

            Assert.IsTrue(viewModel.ServerPermissions[0].IsNew);
            Assert.AreNotSame(permission, viewModel.ServerPermissions[0]);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SecurityViewModel_OnPermissionPropertyChanged")]
        public void SecurityViewModel_OnPermissionPropertyChanged_ResourcePermissionWindowsGroupAndResourceNameChangedToNonEmptyAndIsNew_NewResourcePermissionIsAdded()
        {
            //------------Setup for test--------------------------
            var viewModel = new SecurityViewModel(new SecuritySettingsTO(), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IEnvironmentModel>().Object, () => new Mock<IResourcePickerDialog>().Object);

            Assert.AreEqual(1, viewModel.ResourcePermissions.Count);
            Assert.IsTrue(viewModel.ResourcePermissions[0].IsNew);

            //------------Execute Test---------------------------
            viewModel.ResourcePermissions[0].ResourceName = "xxx";
            viewModel.ResourcePermissions[0].WindowsGroup = "xxx";

            //------------Assert Results-------------------------
            Assert.IsFalse(viewModel.ResourcePermissions[0].IsNew);

            Assert.AreEqual(2, viewModel.ResourcePermissions.Count);
            Assert.IsTrue(viewModel.ResourcePermissions[1].IsNew);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SecurityViewModel_OnPermissionPropertyChanged")]
        public void SecurityViewModel_OnPermissionPropertyChanged_ResourcePermissionWindowsGroupChangedToEmptyAndIsNotNew_ResourcePermissionIsRemoved()
        {
            //------------Setup for test--------------------------
            var permission = new WindowsGroupPermission
            {
                IsServer = false,
                WindowsGroup = "Deploy Admins",
                View = false,
                Execute = false,
                Contribute = false,
                DeployTo = true,
                DeployFrom = true,
                Administrator = false
            };
            var viewModel = new SecurityViewModel(new SecuritySettingsTO(new[] { permission }), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IEnvironmentModel>().Object, () => new Mock<IResourcePickerDialog>().Object);

            Assert.AreEqual(2, viewModel.ResourcePermissions.Count);
            Assert.IsFalse(viewModel.ResourcePermissions[0].IsNew);
            Assert.IsTrue(viewModel.ResourcePermissions[1].IsNew);
            Assert.AreSame(permission, viewModel.ResourcePermissions[0]);

            //------------Execute Test---------------------------
            viewModel.ResourcePermissions[0].WindowsGroup = "";

            //------------Assert Results-------------------------
            Assert.AreEqual(1, viewModel.ResourcePermissions.Count);

            Assert.IsTrue(viewModel.ResourcePermissions[0].IsNew);
            Assert.AreNotSame(permission, viewModel.ResourcePermissions[0]);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SecurityViewModel_PickWindowsGroupCommand")]
        public void SecurityViewModel_PickWindowsGroupCommand_DialogResultIsNotOK_DoesNothing()
        {
            //------------Setup for test--------------------------
            var resourceID = Guid.NewGuid();
            const string ResourceName = "Cat\\Resource";
            var permission = new WindowsGroupPermission
            {
                IsServer = false,
                WindowsGroup = "Deploy Admins",
                View = false,
                Execute = false,
                Contribute = false,
                DeployTo = true,
                DeployFrom = true,
                Administrator = false,
                ResourceID = resourceID,
                ResourceName = ResourceName
            };

            var viewModel = new TestSecurityViewModel(new SecuritySettingsTO(new[] { permission }), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IEnvironmentModel>().Object) { Result = DialogResult.Cancel, SelectedObjects = new[] { (DirectoryObject)null } };
            //------------Execute Test---------------------------
            viewModel.PickWindowsGroupCommand.Execute(permission);

            //------------Assert Results-------------------------
            Assert.AreEqual("Deploy Admins", viewModel.ResourcePermissions[0].WindowsGroup);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SecurityViewModel_PickWindowsGroupCommand")]
        public void SecurityViewModel_PickWindowsGroupCommand_DialogResultIsOKAndNothingSelected_DoesNothing()
        {
            //------------Setup for test--------------------------
            var resourceID = Guid.NewGuid();
            const string ResourceName = "Cat\\Resource";
            var permission = new WindowsGroupPermission
            {
                IsServer = false,
                WindowsGroup = "Deploy Admins",
                View = false,
                Execute = false,
                Contribute = false,
                DeployTo = true,
                DeployFrom = true,
                Administrator = false,
                ResourceID = resourceID,
                ResourceName = ResourceName
            };

            var viewModel = new TestSecurityViewModel(new SecuritySettingsTO(new[] { permission }), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IEnvironmentModel>().Object) { Result = DialogResult.OK, SelectedObjects = new[] { (DirectoryObject)null } };
            //------------Execute Test---------------------------
            viewModel.PickWindowsGroupCommand.Execute(permission);

            //------------Assert Results-------------------------
            Assert.AreEqual("Deploy Admins", viewModel.ResourcePermissions[0].WindowsGroup);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SecurityViewModel_PickWindowsGroupCommand")]
        public void SecurityViewModel_PickWindowsGroupCommand_PermissionIsNull_DoesNothing()
        {
            //------------Setup for test--------------------------
            var resourceID = Guid.NewGuid();
            const string ResourceName = "Cat\\Resource";
            var permission = new WindowsGroupPermission
            {
                IsServer = false,
                WindowsGroup = "Deploy Admins",
                View = false,
                Execute = false,
                Contribute = false,
                DeployTo = true,
                DeployFrom = true,
                Administrator = false,
                ResourceID = resourceID,
                ResourceName = ResourceName
            };
            var viewModel = new SecurityViewModel(new SecuritySettingsTO(new[] { permission }), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IEnvironmentModel>().Object, () => new Mock<IResourcePickerDialog>().Object);

            //------------Execute Test---------------------------
            viewModel.PickWindowsGroupCommand.Execute(null);

            //------------Assert Results-------------------------
            Assert.AreEqual("Deploy Admins", viewModel.ResourcePermissions[0].WindowsGroup);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SecurityViewModel_PickWindowsGroupCommand")]
        public void SecurityViewModel_PickWindowsGroupCommand_ResultIsNull_PermissionWindowsGroupIsNotUpdated()
        {
            //------------Setup for test--------------------------
            var resourceID = Guid.NewGuid();
            const string ResourceName = "Cat\\Resource";
            var permission = new WindowsGroupPermission
            {
                IsServer = false,
                WindowsGroup = "Deploy Admins",
                View = false,
                Execute = false,
                Contribute = false,
                DeployTo = true,
                DeployFrom = true,
                Administrator = false,
                ResourceID = resourceID,
                ResourceName = ResourceName
            };
            var picker = new Mock<DirectoryObjectPickerDialog>();

            var viewModel = new TestSecurityViewModel(new SecuritySettingsTO(new[] { permission }), new DirectoryObjectPickerDialog(), new Mock<IWin32Window>().Object, new Mock<IEnvironmentModel>().Object) { Result = DialogResult.OK, SelectedObjects = new[] { (DirectoryObject)null } };
            //------------Execute Test---------------------------
            viewModel.PickWindowsGroupCommand.Execute(viewModel.ResourcePermissions[0]);

            //------------Assert Results-------------------------
            Assert.AreEqual("Deploy Admins", viewModel.ResourcePermissions[0].WindowsGroup);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SecurityViewModel_PickWindowsGroupCommand")]
        public void SecurityViewModel_PickWindowsGroupCommand_ResultIsNotNull_PermissionWindowsGroupIsUpdated()
        {
            //------------Setup for test--------------------------
            var resourceID = Guid.NewGuid();
            const string ResourceName = "Cat\\Resource";
            var permission = new WindowsGroupPermission
            {
                IsServer = false,
                WindowsGroup = "Deploy Admins",
                View = false,
                Execute = false,
                Contribute = false,
                DeployTo = true,
                DeployFrom = true,
                Administrator = false,
                ResourceID = resourceID,
                ResourceName = ResourceName
            };

            var directoryObj = new DirectoryObject("Administrators", "WinNT://dev2/MyPC/Administrators", "Group", "", new object[1]);

            var picker = new Mock<DirectoryObjectPickerDialog>();

            var viewModel = new TestSecurityViewModel(new SecuritySettingsTO(new[] { permission }), new DirectoryObjectPickerDialog(), new Mock<IWin32Window>().Object, new Mock<IEnvironmentModel>().Object) { Result = DialogResult.OK, SelectedObjects = new[] { directoryObj } };
            //------------Execute Test---------------------------
            viewModel.PickWindowsGroupCommand.Execute(viewModel.ResourcePermissions[0]);

            //------------Assert Results-------------------------
            Assert.AreEqual(directoryObj.Name, viewModel.ResourcePermissions[0].WindowsGroup);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SecurityViewModel_PickWindowsGroupCommand")]
        public void SecurityViewModel_PickWindowsGroupCommand_ResultIsEmptyArray_DoesNothing()
        {
            //------------Setup for test--------------------------
            var resourceID = Guid.NewGuid();
            const string ResourceName = "Cat\\Resource";
            var permission = new WindowsGroupPermission
            {
                IsServer = false,
                WindowsGroup = "Deploy Admins",
                View = false,
                Execute = false,
                Contribute = false,
                DeployTo = true,
                DeployFrom = true,
                Administrator = false,
                ResourceID = resourceID,
                ResourceName = ResourceName
            };

            var viewModel = new TestSecurityViewModel(new SecuritySettingsTO(new[] { permission }), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IEnvironmentModel>().Object) { Result = DialogResult.Cancel, SelectedObjects = new DirectoryObject[0] };
            //------------Execute Test---------------------------
            viewModel.PickWindowsGroupCommand.Execute(permission);

            //------------Assert Results-------------------------
            Assert.AreEqual("Deploy Admins", viewModel.ResourcePermissions[0].WindowsGroup);
        }



        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SecurityViewModel_PickResourceCommand")]
        public void SecurityViewModel_PickResourceCommand_PermissionIsNull_DoesNothing()
        {
            //------------Setup for test--------------------------
            var resourceID = Guid.NewGuid();
            const string ResourceName = "Cat\\Resource";
            var permission = new WindowsGroupPermission
            {
                IsServer = false,
                WindowsGroup = "Deploy Admins",
                View = false,
                Execute = false,
                Contribute = false,
                DeployTo = true,
                DeployFrom = true,
                Administrator = false,
                ResourceID = resourceID,
                ResourceName = ResourceName
            };

            var picker = new Mock<IResourcePickerDialog>();

            var viewModel = new SecurityViewModel(new SecuritySettingsTO(new[] { permission }), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IEnvironmentModel>().Object, () => new Mock<IResourcePickerDialog>().Object);

            //------------Execute Test---------------------------
            viewModel.PickResourceCommand.Execute(null);

            //------------Assert Results-------------------------
            Assert.AreEqual(resourceID, viewModel.ResourcePermissions[0].ResourceID);
            Assert.AreEqual(ResourceName, viewModel.ResourcePermissions[0].ResourceName);
        }



        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SecurityViewModel_HelpText")]
        public void SecurityViewModel_HelpText_IsResourceHelpVisibleIsTrue_ContainsResourceHelpText()
        {
            //------------Setup for test--------------------------          
            var viewModel = new SecurityViewModel(new SecuritySettingsTO(new WindowsGroupPermission[0]), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IEnvironmentModel>().Object, () => new Mock<IResourcePickerDialog>().Object) { IsResourceHelpVisible = true };

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.AreEqual(Warewolf.Studio.Resources.Languages.HelpText.SettingsSecurityResourceHelpResource, viewModel.HelpText);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SecurityViewModel_IsServerHelpVisible")]
        public void SecurityViewModel_IsServerHelpVisible_ChangedToTrueAndIsResourceHelpVisibleIsTrue_IsResourceHelpVisibleIsFalse()
        {
            //------------Setup for test--------------------------          
            var viewModel = new SecurityViewModel(new SecuritySettingsTO(new WindowsGroupPermission[0]), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IEnvironmentModel>().Object, () => new Mock<IResourcePickerDialog>().Object) { IsResourceHelpVisible = true, IsServerHelpVisible = true };

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.IsTrue(viewModel.IsServerHelpVisible);
            Assert.IsFalse(viewModel.IsResourceHelpVisible);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SecurityViewModel_IsResourceHelpVisible")]
        public void SecurityViewModel_IsResourceHelpVisible_ChangedToTrueAndIsServerHelpVisibleIsTrue_IsServerHelpVisibleIsFalse()
        {
            //------------Setup for test--------------------------          
            var viewModel = new SecurityViewModel(new SecuritySettingsTO(new WindowsGroupPermission[0]), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IEnvironmentModel>().Object, () => new Mock<IResourcePickerDialog>().Object) { IsServerHelpVisible = true, IsResourceHelpVisible = true };

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.IsTrue(viewModel.IsResourceHelpVisible);
            Assert.IsFalse(viewModel.IsServerHelpVisible);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SecurityViewModel_CloseHelpCommand")]
        public void SecurityViewModel_CloseHelpCommand_IsServerHelpVisibleIsTrue_IsServerHelpVisibleIsFalse()
        {
            //------------Setup for test--------------------------          
            var viewModel = new SecurityViewModel(new SecuritySettingsTO(new WindowsGroupPermission[0]), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IEnvironmentModel>().Object, () => new Mock<IResourcePickerDialog>().Object) { IsResourceHelpVisible = true };

            //------------Execute Test---------------------------
            viewModel.CloseHelpCommand.Execute(null);

            //------------Assert Results-------------------------
            Assert.IsFalse(viewModel.IsResourceHelpVisible);
            Assert.IsFalse(viewModel.IsServerHelpVisible);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SecurityViewModel_CloseHelpCommand")]
        public void SecurityViewModel_CloseHelpCommand_IsResourceHelpVisibleIsTrue_IsResourceHelpVisibleIsFalse()
        {
            //------------Setup for test--------------------------          
            var viewModel = new SecurityViewModel(new SecuritySettingsTO(new WindowsGroupPermission[0]), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IEnvironmentModel>().Object, () => new Mock<IResourcePickerDialog>().Object) { IsResourceHelpVisible = true };

            //------------Execute Test---------------------------
            viewModel.CloseHelpCommand.Execute(null);

            //------------Assert Results-------------------------
            Assert.IsFalse(viewModel.IsResourceHelpVisible);
            Assert.IsFalse(viewModel.IsServerHelpVisible);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SecurityViewModel_Save")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SecurityViewModel_Save_NullPermissions_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------          
            var viewModel = new SecurityViewModel(new SecuritySettingsTO(new WindowsGroupPermission[0]), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IEnvironmentModel>().Object, () => new Mock<IResourcePickerDialog>().Object);

            //------------Execute Test---------------------------
            viewModel.Save(null);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("SecurityViewModel_Save")]
        public void SecurityViewModel_Save_InvalidPermissions_InvalidPermissionsAreRemoved()
        {
            //------------Setup for test--------------------------          
            var permissions = CreatePermissions();

            var invalidPermission = permissions[permissions.Count - 1];
            invalidPermission.WindowsGroup = "";

            var viewModel = new SecurityViewModel(new SecuritySettingsTO(permissions), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IEnvironmentModel>().Object, () => new Mock<IResourcePickerDialog>().Object);

            var target = new SecuritySettingsTO();

            var expectedCount = permissions.Count - 1;
            var expectedResourceCount = viewModel.ResourcePermissions.Count - 1;

            //------------Execute Test---------------------------
            viewModel.Save(target);

            //------------Assert Results-------------------------
            Assert.AreEqual(expectedCount, target.WindowsGroupPermissions.Count);
            Assert.AreEqual(expectedResourceCount, viewModel.ResourcePermissions.Count);
            foreach (var permission in target.WindowsGroupPermissions)
            {
                Assert.IsTrue(permission.IsValid);
                Assert.IsFalse(permission.IsNew);
            }
            Assert.AreEqual(1, viewModel.ResourcePermissions.Count(p => p.IsNew));
            Assert.AreEqual(1, viewModel.ServerPermissions.Count(p => p.IsNew));
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SecurityViewModel_Save")]
        public void SecurityViewModel_Save_DeletedPermissions_DeletedPermissionsAreRemoved()
        {
            //------------Setup for test--------------------------          
            var permissions = CreatePermissions();

            var deletedPermission = permissions[permissions.Count - 1];
            deletedPermission.IsDeleted = true;

            var viewModel = new SecurityViewModel(new SecuritySettingsTO(permissions), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IEnvironmentModel>().Object, () => new Mock<IResourcePickerDialog>().Object);

            var target = new SecuritySettingsTO();

            var expectedCount = permissions.Count - 1;
            var expectedResourceCount = viewModel.ResourcePermissions.Count - 1;

            //------------Execute Test---------------------------
            viewModel.Save(target);

            //------------Assert Results-------------------------
            Assert.AreEqual(expectedCount, target.WindowsGroupPermissions.Count);
            Assert.AreEqual(expectedResourceCount, viewModel.ResourcePermissions.Count);
            foreach (var permission in target.WindowsGroupPermissions)
            {
                Assert.IsTrue(permission.IsValid);
                Assert.IsFalse(permission.IsNew);
            }
            Assert.AreEqual(1, viewModel.ResourcePermissions.Count(p => p.IsNew));
            Assert.AreEqual(1, viewModel.ServerPermissions.Count(p => p.IsNew));
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SecurityViewModel_ServerDuplicates")]
        public void SecurityViewModel_ServerDuplicates_NoDuplicates_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var securityViewModel = new SecurityViewModel(new SecuritySettingsTO(), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IEnvironmentModel>().Object, () => new Mock<IResourcePickerDialog>().Object);
            securityViewModel.ServerPermissions.Add(new WindowsGroupPermission
            {
                WindowsGroup = "Some Group",
                ResourceID = Guid.Empty,
                IsServer = true
            });

            securityViewModel.ServerPermissions.Add(new WindowsGroupPermission
            {
                WindowsGroup = "Some Group2",
                ResourceID = Guid.Empty,
                IsServer = true
            });
            //------------Execute Test---------------------------
            var hasDuplicateServerPermissions = securityViewModel.HasDuplicateServerPermissions();
            //------------Assert Results-------------------------
            Assert.IsFalse(hasDuplicateServerPermissions);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SecurityViewModel_ServerDuplicates")]
        public void SecurityViewModel_ServerDuplicates_HasDuplicatesDeleted_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var securityViewModel = new SecurityViewModel(new SecuritySettingsTO(), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IEnvironmentModel>().Object, () => new Mock<IResourcePickerDialog>().Object);
            securityViewModel.ServerPermissions.Add(new WindowsGroupPermission
            {
                WindowsGroup = "Some Group",
                ResourceID = Guid.Empty,
                IsServer = true
            });

            securityViewModel.ServerPermissions.Add(new WindowsGroupPermission
            {
                WindowsGroup = "Some Group",
                ResourceID = Guid.Empty,
                IsServer = true,
                IsDeleted = true
            });
            //------------Execute Test---------------------------
            var hasDuplicateServerPermissions = securityViewModel.HasDuplicateServerPermissions();
            //------------Assert Results-------------------------
            Assert.IsFalse(hasDuplicateServerPermissions);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SecurityViewModel_ServerDuplicates")]
        public void SecurityViewModel_ServerDuplicates_Duplicates_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var securityViewModel = new SecurityViewModel(new SecuritySettingsTO(), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IEnvironmentModel>().Object, () => new Mock<IResourcePickerDialog>().Object);
            securityViewModel.ServerPermissions.Add(new WindowsGroupPermission
            {
                WindowsGroup = "Some Group",
                ResourceID = Guid.Empty,
                IsServer = true
            });

            securityViewModel.ServerPermissions.Add(new WindowsGroupPermission
            {
                WindowsGroup = "Some Group",
                ResourceID = Guid.Empty,
                IsServer = true
            });
            //------------Execute Test---------------------------
            var hasDuplicateServerPermissions = securityViewModel.HasDuplicateServerPermissions();
            //------------Assert Results-------------------------
            Assert.IsTrue(hasDuplicateServerPermissions);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SecurityViewModel_HasDuplicateResourcePermissions")]
        public void SecurityViewModel_HasDuplicateResourcePermissions_NoDuplicatesResourceID_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var securityViewModel = new SecurityViewModel(new SecuritySettingsTO(), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IEnvironmentModel>().Object, () => new Mock<IResourcePickerDialog>().Object);
            securityViewModel.ResourcePermissions.Add(new WindowsGroupPermission
            {
                WindowsGroup = "Some Group",
                ResourceID = Guid.NewGuid(),
                IsServer = false
            });

            securityViewModel.ResourcePermissions.Add(new WindowsGroupPermission
            {
                WindowsGroup = "Some Group",
                ResourceID = Guid.NewGuid(),
                IsServer = false
            });
            //------------Execute Test---------------------------
            var hasDuplicateResourcePermissions = securityViewModel.HasDuplicateResourcePermissions();
            //------------Assert Results-------------------------
            Assert.IsFalse(hasDuplicateResourcePermissions);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SecurityViewModel_HasDuplicateResourcePermissions")]
        public void SecurityViewModel_HasDuplicateResourcePermissions_NoDuplicatesWindowsGroup_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var securityViewModel = new SecurityViewModel(new SecuritySettingsTO(), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IEnvironmentModel>().Object, () => new Mock<IResourcePickerDialog>().Object);
            var resourceId = Guid.NewGuid();
            securityViewModel.ResourcePermissions.Add(new WindowsGroupPermission
            {
                WindowsGroup = "Some Group",
                ResourceID = resourceId,
                IsServer = false
            });

            securityViewModel.ResourcePermissions.Add(new WindowsGroupPermission
            {
                WindowsGroup = "Some Group2",
                ResourceID = resourceId,
                IsServer = false
            });
            //------------Execute Test---------------------------
            var hasDuplicateResourcePermissions = securityViewModel.HasDuplicateResourcePermissions();
            //------------Assert Results-------------------------
            Assert.IsFalse(hasDuplicateResourcePermissions);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SecurityViewModel_HasDuplicateResourcePermissions")]
        public void SecurityViewModel_HasDuplicateResourcePermissions_DuplicateDeleted_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var securityViewModel = new SecurityViewModel(new SecuritySettingsTO(), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IEnvironmentModel>().Object, () => new Mock<IResourcePickerDialog>().Object);
            var resourceId = Guid.NewGuid();
            securityViewModel.ResourcePermissions.Add(new WindowsGroupPermission
            {
                WindowsGroup = "Some Group",
                ResourceID = resourceId,
                IsServer = false
            });

            securityViewModel.ResourcePermissions.Add(new WindowsGroupPermission
            {
                WindowsGroup = "Some Group",
                ResourceID = resourceId,
                IsServer = false,
                IsDeleted = true
            });
            //------------Execute Test---------------------------
            var hasDuplicateResourcePermissions = securityViewModel.HasDuplicateResourcePermissions();
            //------------Assert Results-------------------------
            Assert.IsFalse(hasDuplicateResourcePermissions);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("SecurityViewModel_HasDuplicateResourcePermissions")]
        public void SecurityViewModel_ResourcePermissionsCompare_IsDeleted_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var securityViewModel = new SecurityViewModel(new SecuritySettingsTO(), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IEnvironmentModel>().Object, () => new Mock<IResourcePickerDialog>().Object);
            var resourceId = Guid.NewGuid();

            var groupPermission = new WindowsGroupPermission
            {
                WindowsGroup = "Some Group",
                ResourceID = resourceId,
                IsServer = false,
                IsDeleted = false,
                ResourceName = "a"

            };
            securityViewModel.ResourcePermissions.Clear();
            securityViewModel.ResourcePermissions.Add(groupPermission);
            var methodInfo = typeof(SecurityViewModel).GetMethod("ResourcePermissionsCompare", BindingFlags.Instance | BindingFlags.NonPublic);
            //------------Execute Test---------------------------
            var groupPermissions = new ObservableCollection<WindowsGroupPermission>()
            {
                        new WindowsGroupPermission
                            {
                                WindowsGroup = "Some Group",
                                ResourceID = resourceId,
                                IsServer = false,
                                IsDeleted = true,
                            ResourceName = "a"

                            }
            };
            var invoke = methodInfo.Invoke(securityViewModel, new object[] { groupPermissions , true});
            //------------Assert Results-------------------------
            Assert.IsFalse(bool.Parse(invoke.ToString()));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("SecurityViewModel_HasDuplicateResourcePermissions")]
        public void SecurityViewModel_ServerPermissionsCompare_IsDeleted_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var securityViewModel = new SecurityViewModel(new SecuritySettingsTO(), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IEnvironmentModel>().Object, () => new Mock<IResourcePickerDialog>().Object);
            var resourceId = Guid.NewGuid();

            var groupPermission = new WindowsGroupPermission
            {
                WindowsGroup = "Some Group",
                ResourceID = resourceId,
                IsServer = false,
                IsDeleted = false,
                ResourceName = "a"

            };
            securityViewModel.ResourcePermissions.Clear();
            securityViewModel.ResourcePermissions.Add(groupPermission);
            var methodInfo = typeof(SecurityViewModel).GetMethod("ServerPermissionsCompare", BindingFlags.Instance | BindingFlags.NonPublic);
            //------------Execute Test---------------------------
            var groupPermissions = new ObservableCollection<WindowsGroupPermission>()
            {
                        new WindowsGroupPermission
                            {
                                WindowsGroup = "Some Group",
                                ResourceID = resourceId,
                                IsServer = false,
                                IsDeleted = true,
                            ResourceName = "a"

                            }
            };
            var invoke = methodInfo.Invoke(securityViewModel, new object[] { groupPermissions, true });
            //------------Assert Results-------------------------
            Assert.IsFalse(bool.Parse(invoke.ToString()));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void IsInDomain_GivenNotInDomaint_ShouldReturnCollapsed()
        {
            //---------------Set up test pack-------------------
            var securityViewModel = new SecurityViewModel(new SecuritySettingsTO(), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IEnvironmentModel>().Object, () => new Mock<IResourcePickerDialog>().Object);
            var mock = new Mock<IDomain>();
            mock.Setup(domain => domain.GetComputerDomain()).Throws(new Exception());
            var type = typeof(SecurityViewModel);
            var fieldInfo = type.GetField("_domain", BindingFlags.Static | BindingFlags.NonPublic);
            fieldInfo?.SetValue(null, mock.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(securityViewModel);
            //---------------Execute Test ----------------------
            var methodInfo = type.GetMethod("IsInDomain", BindingFlags.Static | BindingFlags.NonPublic);
            var invoke = methodInfo.Invoke(null, null);
            //---------------Test Result -----------------------
            Assert.AreEqual(Visibility.Collapsed, invoke);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void IsInDomain_GivenInDomaint_ShouldReturnVisible()
        {
            //---------------Set up test pack-------------------
            var securityViewModel = new SecurityViewModel(new SecuritySettingsTO(), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IEnvironmentModel>().Object, () => new Mock<IResourcePickerDialog>().Object);
            var mock = new Mock<IDomain>();
            mock.Setup(domain => domain.GetComputerDomain()).Verifiable();
            var type = typeof(SecurityViewModel);
            var fieldInfo = type.GetField("_domain", BindingFlags.Static | BindingFlags.NonPublic);
            fieldInfo?.SetValue(null, mock.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(securityViewModel);
            //---------------Execute Test ----------------------
            var methodInfo = type.GetMethod("IsInDomain", BindingFlags.Static | BindingFlags.NonPublic);
            var invoke = methodInfo.Invoke(null, null);
            //---------------Test Result -----------------------
            Assert.AreEqual(Visibility.Visible, invoke);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("SecurityViewModel_HasDuplicateResourcePermissions")]
        public void SecurityViewModel_HasDuplicateResourcePermissions_DuplicateNotDeleted_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var securityViewModel = new SecurityViewModel(new SecuritySettingsTO(), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IEnvironmentModel>().Object, () => new Mock<IResourcePickerDialog>().Object);
            var resourceId = Guid.NewGuid();
            securityViewModel.ResourcePermissions.Add(new WindowsGroupPermission
            {
                WindowsGroup = "Some Group",
                ResourceID = resourceId,
                IsServer = false
            });

            securityViewModel.ResourcePermissions.Add(new WindowsGroupPermission
            {
                WindowsGroup = "Some Group",
                ResourceID = resourceId,
                IsServer = false,
            });
            //------------Execute Test---------------------------
            var hasDuplicateResourcePermissions = securityViewModel.HasDuplicateResourcePermissions();
            //------------Assert Results-------------------------
            Assert.IsTrue(hasDuplicateResourcePermissions);
        }

        static List<WindowsGroupPermission> CreatePermissions()
        {
            return new List<WindowsGroupPermission>(new[]
            {
                new WindowsGroupPermission
                {
                    IsServer = true, WindowsGroup = GlobalConstants.WarewolfGroup,
                    View = false, Execute = false, Contribute = true, DeployTo = true, DeployFrom = true, Administrator = true
                },
                new WindowsGroupPermission
                {
                    IsServer = true, WindowsGroup = "Deploy Admins",
                    View = false, Execute = false, Contribute = false, DeployTo = true, DeployFrom = true, Administrator = false
                },

                new WindowsGroupPermission
                {
                    ResourceID = Guid.NewGuid(), ResourceName = "Category1\\Workflow1",
                    WindowsGroup = "Windows Group 1", View = false, Execute = true, Contribute = false
                },
                new WindowsGroupPermission
                {
                    ResourceID = Guid.NewGuid(), ResourceName = "Category1\\Workflow1",
                    WindowsGroup = "Windows Group 2", View = false, Execute = false, Contribute = true
                },

                new WindowsGroupPermission
                {
                    ResourceID = Guid.NewGuid(), ResourceName = "Category1\\Workflow2",
                    WindowsGroup = "Windows Group 1", View = true, Execute = true, Contribute = false
                },

                new WindowsGroupPermission
                {
                    ResourceID = Guid.NewGuid(), ResourceName = "Category2\\Workflow3",
                    WindowsGroup = "Windows Group 3", View = true, Execute = false, Contribute = false
                },
                new WindowsGroupPermission
                {
                    ResourceID = Guid.NewGuid(), ResourceName = "Category2\\Workflow3",
                    WindowsGroup = "Windows Group 4", View = false, Execute = true, Contribute = false
                },


                new WindowsGroupPermission
                {
                    ResourceID = Guid.NewGuid(), ResourceName = "Category2\\Workflow4",
                    WindowsGroup = "Windows Group 3", View = false, Execute = false, Contribute = true
                },
                new WindowsGroupPermission
                {
                    ResourceID = Guid.NewGuid(), ResourceName = "Category1\\Workflow1",
                    WindowsGroup = "Windows Group 4", View = false, Execute = false, Contribute = true
                }
            });
        }
    }
}
