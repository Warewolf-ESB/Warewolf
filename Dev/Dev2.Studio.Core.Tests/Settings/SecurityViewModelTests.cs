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
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using Caliburn.Micro;
using CubicOrange.Windows.Forms.ActiveDirectory;
using Dev2.Common;
using Dev2.Common.Interfaces.PopupController;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.ConnectionHelpers;
using Dev2.Data;
using Dev2.Data.Interfaces;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Dialogs;
using Dev2.Services.Security;
using Dev2.Settings.Security;
using Dev2.Studio.Core;
using Dev2.Studio.Interfaces;
using Dev2.Studio.Interfaces.Enums;
using Dev2.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Data;
using Warewolf.Studio.Core.Popup;


namespace Dev2.Core.Tests.Settings
{
    [TestClass]
    [TestCategory("Studio Settings Core")]
    public class SecurityViewModelTests
    {
        static Mock<IResourceRepository> _resourceRepo = new Mock<IResourceRepository>();
        static Mock<IServer> _environmentModel;
        static IServerRepository _serverRepo;
        static Mock<IContextualResourceModel> _firstResource;

        const string _displayName = "test2";
        const string _serviceDefinition = "<x/>";
        static readonly Guid _serverID = Guid.NewGuid();
        public static Mock<IPopupController> _popupController;

        public static Mock<IContextualResourceModel> CreateResource(ResourceType resourceType, string _resourceName, Guid _resourceID)
        {
            var result = new Mock<IContextualResourceModel>();

            result.Setup(c => c.ResourceName).Returns(_resourceName);
            result.Setup(c => c.ResourceType).Returns(resourceType);
            result.Setup(c => c.DisplayName).Returns(_displayName);
            result.Setup(c => c.WorkflowXaml).Returns(new StringBuilder(_serviceDefinition));
            result.Setup(c => c.Category).Returns("Testing");
            result.Setup(c => c.Environment).Returns(_environmentModel.Object);
            result.Setup(c => c.ServerID).Returns(_serverID);
            result.Setup(c => c.ID).Returns(_resourceID);

            return result;
        }

        public static IServerRepository GetEnvironmentRepository()
        {
            var models = new List<IServer> {_environmentModel.Object};
            var mock = new Mock<IServerRepository>();
            mock.Setup(s => s.All()).Returns(models);
            mock.Setup(s => s.IsLoaded).Returns(true);
            mock.Setup(repository => repository.Source).Returns(_environmentModel.Object);
            mock.Setup(repository => repository.FindSingle(It.IsAny<Expression<Func<IServer, bool>>>())).Returns(_environmentModel.Object);
            _serverRepo = mock.Object;
            CustomContainer.Register(_serverRepo);
            return _serverRepo;
        }

        [TestInitialize]
        public void setup()
        {
            AppUsageStats.LocalHost = "http://localhost:3142";
            var mockShellViewModel = new Mock<IShellViewModel>();
            _environmentModel = new Mock<IServer>();
            _environmentModel.Setup(a => a.DisplayName).Returns("Localhost");
            mockShellViewModel.Setup(x => x.LocalhostServer).Returns(_environmentModel.Object);
            mockShellViewModel.Setup(x => x.ActiveServer).Returns(new Mock<IServer>().Object);
            var connectControlSingleton = new Mock<IConnectControlSingleton>();
            var explorerTooltips = new Mock<IExplorerTooltips>();

            CustomContainer.Register(mockShellViewModel.Object);
            CustomContainer.Register(new Mock<Microsoft.Practices.Prism.PubSubEvents.IEventAggregator>().Object);
            CustomContainer.Register(connectControlSingleton.Object);
            CustomContainer.Register(explorerTooltips.Object);
            ServerRepository.Instance.ActiveServer = _environmentModel.Object;
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory(nameof(SecurityViewModel))]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SecurityViewModel_Constructor_DirectoryObjectPickerDialogIsNull_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------

            new SecurityViewModel(new SecuritySettingsTO(), null, new Mock<IWin32Window>().Object, new Mock<IServer>().Object, () => new Mock<IResourcePickerDialog>().Object);


            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory(nameof(SecurityViewModel))]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SecurityViewModel_Constructor_ParentWindowIsNull_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------

            new SecurityViewModel(new SecuritySettingsTO(), new Mock<DirectoryObjectPickerDialog>().Object, null, new Mock<IServer>().Object, () => new Mock<IResourcePickerDialog>().Object);


            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory(nameof(SecurityViewModel))]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SecurityViewModel_Constructor_EnvironmentIsNull_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------

            new SecurityViewModel(new SecuritySettingsTO(), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, null, () => new Mock<IResourcePickerDialog>().Object);


            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory(nameof(SecurityViewModel))]
        public void SecurityViewModel_Constructor_SecuritySettingsIsNull_PropertiesInitialized()
        {
            Verify_Constructor_InitializesProperties(null);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory(nameof(SecurityViewModel))]
        public void SecurityViewModel_Constructor_AllParametersValid_PropertiesInitialized()
        {
            //------------Setup for test--------------------------
            var overrideResource = new NamedGuid
            {
                Name = "Resourcename",
                Value = Guid.NewGuid()
            };
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
            var secretKey = GenerateSecretKey();
            var securitySettingsTO = new SecuritySettingsTO(permissions, overrideResource,secretKey);

            Verify_Constructor_InitializesProperties(securitySettingsTO);
        }

        string GenerateSecretKey()
        {
            var hmac = new HMACSHA256();
            return Convert.ToBase64String(hmac.Key);
        }
        static void Verify_Constructor_InitializesProperties(SecuritySettingsTO securitySettingsTO)
        {
            //------------Execute Test---------------------------
            var viewModel = new SecurityViewModel(securitySettingsTO, new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IServer>().Object, () => new Mock<IResourcePickerDialog>().Object);

            //------------Assert Results-------------------------
            Assert.IsNotNull(viewModel.ServerHelpToggle);
            Assert.IsNotNull(viewModel.ResourceHelpToggle);
            Assert.IsTrue(viewModel.CloseHelpCommand.CanExecute(null));
            Assert.IsNotNull(viewModel.PickResourceCommand);
            Assert.IsTrue(viewModel.PickResourceCommand.CanExecute(null));
            VerifyWindowsPermission(securitySettingsTO, viewModel);
            Assert.IsNotNull(viewModel.ServerPermissions);
            Assert.IsNotNull(viewModel.ResourcePermissions);
            Assert.IsNotNull(viewModel.OverrideResource);
            var serverPerms = securitySettingsTO?.WindowsGroupPermissions.Where(p => p.IsServer).ToList() ?? new List<WindowsGroupPermission>();
            var resourcePerms = securitySettingsTO?.WindowsGroupPermissions.Where(p => !p.IsServer).ToList() ?? new List<WindowsGroupPermission>();
            var overrideResource = securitySettingsTO?.AuthenticationOverrideWorkflow ?? new NamedGuid();

            // constructor adds an extra "new"  permission
            Assert.AreEqual(overrideResource.Name, viewModel.OverrideResource.Name);
            Assert.AreEqual(overrideResource.Value, viewModel.OverrideResource.Value);
            Assert.AreEqual(serverPerms.Count + 1, viewModel.ServerPermissions.Count);
            Assert.AreEqual(resourcePerms.Count + 1, viewModel.ResourcePermissions.Count);
        }

        static void VerifyWindowsPermission(SecuritySettingsTO securitySettingsTO, SecurityViewModel viewModel)
        {
            Assert.IsNotNull(viewModel.PickWindowsGroupCommand);
            if (securitySettingsTO != null)
            {
                Assert.IsTrue(viewModel.PickWindowsGroupCommand.CanExecute(securitySettingsTO.WindowsGroupPermissions[0]));
            }
            else
            {
                Assert.IsFalse(viewModel.PickWindowsGroupCommand.CanExecute(null));
            }
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory(nameof(SecurityViewModel))]
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
            var viewModel = new SecurityViewModel(new SecuritySettingsTO(new[] {permission}), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IServer>().Object, () => new Mock<IResourcePickerDialog>().Object);

            Assert.IsFalse(viewModel.IsDirty);

            //------------Execute Test---------------------------
            permission.Administrator = true;

            //------------Assert Results-------------------------
            Assert.IsTrue(viewModel.IsDirty);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(SecurityViewModel))]
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
            var viewModel = new SecurityViewModel(new SecuritySettingsTO(new[] {permission}), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IServer>().Object, () => new Mock<IResourcePickerDialog>().Object);

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
        [TestCategory(nameof(SecurityViewModel))]
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
            var viewModel = new SecurityViewModel(new SecuritySettingsTO(new[] {permission}), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IServer>().Object, () => new Mock<IResourcePickerDialog>().Object);

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
        [TestCategory(nameof(SecurityViewModel))]
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
            var viewModel = new SecurityViewModel(new SecuritySettingsTO(new[] {permission}), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IServer>().Object, () => new Mock<IResourcePickerDialog>().Object);

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
        [TestCategory(nameof(SecurityViewModel))]
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
            var viewModel = new SecurityViewModel(new SecuritySettingsTO(new[] {permission}), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IServer>().Object, () => new Mock<IResourcePickerDialog>().Object);

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
        [TestCategory(nameof(SecurityViewModel))]
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
            var viewModel = new SecurityViewModel(new SecuritySettingsTO(new[] {permission}), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IServer>().Object, () => new Mock<IResourcePickerDialog>().Object);

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
        [TestCategory(nameof(SecurityViewModel))]
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
            var viewModel = new SecurityViewModel(new SecuritySettingsTO(new[] {permission}), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IServer>().Object, () => new Mock<IResourcePickerDialog>().Object);

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
        [TestCategory(nameof(SecurityViewModel))]
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
            var viewModel = new SecurityViewModel(new SecuritySettingsTO(new[] {permission}), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IServer>().Object, () => new Mock<IResourcePickerDialog>().Object);

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
        [TestCategory(nameof(SecurityViewModel))]
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
            var viewModel = new SecurityViewModel(new SecuritySettingsTO(new[] {permission}), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IServer>().Object, () => new Mock<IResourcePickerDialog>().Object);

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
        [TestCategory(nameof(SecurityViewModel))]
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
            var viewModel = new SecurityViewModel(new SecuritySettingsTO(new[] {permission}), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IServer>().Object, () => new Mock<IResourcePickerDialog>().Object);

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
        [TestCategory(nameof(SecurityViewModel))]
        public void SecurityViewModel_OnPermissionPropertyChanged_ServerPermissionWindowsGroupChangedToNonEmptyAndIsNew_NewServerPermissionIsAdded()
        {
            //------------Setup for test--------------------------
            var viewModel = new SecurityViewModel(new SecuritySettingsTO(), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IServer>().Object, () => new Mock<IResourcePickerDialog>().Object);

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
        [TestCategory(nameof(SecurityViewModel))]
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
            var viewModel = new SecurityViewModel(new SecuritySettingsTO(new[] {permission}), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IServer>().Object, () => new Mock<IResourcePickerDialog>().Object);

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
        [TestCategory(nameof(SecurityViewModel))]
        public void SecurityViewModel_OnPermissionPropertyChanged_ResourcePermissionWindowsGroupAndResourceNameChangedToNonEmptyAndIsNew_NewResourcePermissionIsAdded()
        {
            //------------Setup for test--------------------------
            var viewModel = new SecurityViewModel(new SecuritySettingsTO(), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IServer>().Object, () => new Mock<IResourcePickerDialog>().Object);

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
        [TestCategory(nameof(SecurityViewModel))]
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
            var viewModel = new SecurityViewModel(new SecuritySettingsTO(new[] {permission}), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IServer>().Object, () => new Mock<IResourcePickerDialog>().Object);

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
        [TestCategory(nameof(SecurityViewModel))]
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

            var viewModel = new TestSecurityViewModel(new SecuritySettingsTO(new[] {permission}), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IServer>().Object) {Result = DialogResult.Cancel, SelectedObjects = new[] {(DirectoryObject) null}};
            //------------Execute Test---------------------------
            viewModel.PickWindowsGroupCommand.Execute(permission);

            //------------Assert Results-------------------------
            Assert.AreEqual("Deploy Admins", viewModel.ResourcePermissions[0].WindowsGroup);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory(nameof(SecurityViewModel))]
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

            var viewModel = new TestSecurityViewModel(new SecuritySettingsTO(new[] {permission}), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IServer>().Object) {Result = DialogResult.OK, SelectedObjects = new[] {(DirectoryObject) null}};
            //------------Execute Test---------------------------
            viewModel.PickWindowsGroupCommand.Execute(permission);

            //------------Assert Results-------------------------
            Assert.AreEqual("Deploy Admins", viewModel.ResourcePermissions[0].WindowsGroup);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory(nameof(SecurityViewModel))]
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
            var viewModel = new SecurityViewModel(new SecuritySettingsTO(new[] {permission}), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IServer>().Object, () => new Mock<IResourcePickerDialog>().Object);

            //------------Execute Test---------------------------
            viewModel.PickWindowsGroupCommand.Execute(null);

            //------------Assert Results-------------------------
            Assert.AreEqual("Deploy Admins", viewModel.ResourcePermissions[0].WindowsGroup);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory(nameof(SecurityViewModel))]
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

            var viewModel = new TestSecurityViewModel(new SecuritySettingsTO(new[] {permission}), new DirectoryObjectPickerDialog(), new Mock<IWin32Window>().Object, new Mock<IServer>().Object) {Result = DialogResult.OK, SelectedObjects = new[] {(DirectoryObject) null}};
            //------------Execute Test---------------------------
            viewModel.PickWindowsGroupCommand.Execute(viewModel.ResourcePermissions[0]);

            //------------Assert Results-------------------------
            Assert.AreEqual("Deploy Admins", viewModel.ResourcePermissions[0].WindowsGroup);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory(nameof(SecurityViewModel))]
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

            var viewModel = new TestSecurityViewModel(new SecuritySettingsTO(new[] {permission}), new DirectoryObjectPickerDialog(), new Mock<IWin32Window>().Object, new Mock<IServer>().Object) {Result = DialogResult.OK, SelectedObjects = new[] {directoryObj}};
            //------------Execute Test---------------------------
            viewModel.PickWindowsGroupCommand.Execute(viewModel.ResourcePermissions[0]);

            //------------Assert Results-------------------------
            Assert.AreEqual(directoryObj.Name, viewModel.ResourcePermissions[0].WindowsGroup);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory(nameof(SecurityViewModel))]
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

            var viewModel = new TestSecurityViewModel(new SecuritySettingsTO(new[] {permission}), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IServer>().Object) {Result = DialogResult.Cancel, SelectedObjects = new DirectoryObject[0]};
            //------------Execute Test---------------------------
            viewModel.PickWindowsGroupCommand.Execute(permission);

            //------------Assert Results-------------------------
            Assert.AreEqual("Deploy Admins", viewModel.ResourcePermissions[0].WindowsGroup);
        }


        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory(nameof(SecurityViewModel))]
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

            var viewModel = new SecurityViewModel(new SecuritySettingsTO(new[] {permission}), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IServer>().Object, () => new Mock<IResourcePickerDialog>().Object);

            //------------Execute Test---------------------------
            viewModel.PickResourceCommand.Execute(null);

            //------------Assert Results-------------------------
            Assert.AreEqual(resourceID, viewModel.ResourcePermissions[0].ResourceID);
            Assert.AreEqual(ResourceName, viewModel.ResourcePermissions[0].ResourceName);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SecurityViewModel))]
        [ExpectedException(typeof(Exception), "Server does not exist")]
        public void SecurityViewModel_PickResourceCommand_WindowsGroupPermission_ExpectException()
        {
            //------------Setup for test--------------------------
            var resourceId = Guid.NewGuid();
            const string resourceName = "Cat\\Resource";
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
                ResourceID = resourceId,
                ResourceName = resourceName
            };

            var picker = new Mock<IResourcePickerDialog>();

            var viewModel = new SecurityViewModel(new SecuritySettingsTO(new[] {permission}), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IServer>().Object, () => new Mock<IResourcePickerDialog>().Object);

            //------------Execute Test---------------------------
            viewModel.PickResourceCommand.Execute(permission);

            //------------Assert Results-------------------------
            Assert.AreEqual(resourceId, viewModel.ResourcePermissions[0].ResourceID);
            Assert.AreEqual(resourceName, viewModel.ResourcePermissions[0].ResourceName);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SecurityViewModel))]
        public void SecurityViewModel_PickResourceCommand_WindowsGroupPermission()
        {
            //------------Setup for test--------------------------
            var resourceId = Guid.NewGuid();
            const string resourceName = "Cat\\Resource";
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
                ResourceID = resourceId,
                ResourceName = resourceName
            };

            var expectedResourceId = Guid.NewGuid();
            const string expectedResourceName= "Cat\\Resource";
            var mockExplorerTreeItem = new Mock<IExplorerTreeItem>();
            mockExplorerTreeItem.Setup(o => o.ResourceId).Returns(expectedResourceId);
            mockExplorerTreeItem.Setup(o => o.ResourceName).Returns(expectedResourceName);

            var picker = new Mock<IResourcePickerDialog>();
            picker.Setup(o => o.SelectResource(resourceId));
            picker.Setup(o => o.SelectedResource).Returns(mockExplorerTreeItem.Object);

            var mockResourceModel = new Mock<IResourceModel>();
            mockResourceModel.Setup(o => o.ResourceName).Returns("");
            mockResourceModel.Setup(o => o.ID).Returns(Guid.Empty);

            _resourceRepo.Setup(o => o.FindSingle(a => a.ID == resourceId))
                .Returns(mockResourceModel.Object);

            _environmentModel.Setup(m => m.ResourceRepository).Returns(_resourceRepo.Object);

            var viewModel = new SecurityViewModel(new SecuritySettingsTO(new[] {permission}), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, _environmentModel.Object, () => picker.Object);

            //------------Execute Test---------------------------
            viewModel.PickResourceCommand.Execute(permission);

            //------------Assert Results-------------------------
            _resourceRepo.Verify(o => o.FindSingle(It.IsAny<Expression<Func<IResourceModel, bool>>>()), Times.Once);
            Assert.AreEqual(resourceId, permission.ResourceID);
            Assert.AreEqual(resourceName, permission.ResourceName);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SecurityViewModel))]
        public void SecurityViewModel_PickResourceCommand_EnvironmentViewModel()
        {
            //------------Setup for test--------------------------
            var resourceId = Guid.NewGuid();
            const string resourceName = "localhost";
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
                ResourceID = resourceId,
                ResourceName = resourceName
            };

            var expectedResourceId = Guid.NewGuid();
            const string serverName= "localhost (Connected)";
            const string expectedResourceName= "localhost";
            var mockEnvironmentViewModel = new Mock<IEnvironmentViewModel>();
            mockEnvironmentViewModel.Setup(o => o.ResourceId).Returns(expectedResourceId);
            mockEnvironmentViewModel.Setup(o => o.ResourceName).Returns(serverName);
            mockEnvironmentViewModel.Setup(o => o.ResourcePath).Returns("");

            var picker = new Mock<IResourcePickerDialog>();
            picker.Setup(o => o.SelectResource(resourceId));
            picker.Setup(o => o.SelectedResource).Returns(mockEnvironmentViewModel.Object);
            picker.Setup(o => o.ShowDialog(It.IsAny<IServer>())).Returns(true);

            var mockResourceModel = new Mock<IResourceModel>();
            mockResourceModel.Setup(o => o.ResourceName).Returns(expectedResourceName);
            mockResourceModel.Setup(o => o.ID).Returns(expectedResourceId);
            mockResourceModel.Setup(o => o.GetSavePath()).Returns("");

            _resourceRepo.Setup(o => o.FindSingle(a => a.ID == resourceId))
                .Returns(mockResourceModel.Object);

            _environmentModel.Setup(m => m.ResourceRepository).Returns(_resourceRepo.Object);

            var viewModel = new SecurityViewModel(new SecuritySettingsTO(new[] {permission}), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, _environmentModel.Object, () => picker.Object);

            //------------Execute Test---------------------------
            viewModel.PickResourceCommand.Execute(permission);

            //------------Assert Results-------------------------
            Assert.AreEqual(expectedResourceId, permission.ResourceID);
            Assert.AreEqual(expectedResourceName, permission.ResourceName);
            Assert.AreEqual("", permission.ResourcePath);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SecurityViewModel))]
        public void SecurityViewModel_PickResourceCommand_ExplorerItemViewModel()
        {
            //------------Setup for test--------------------------
            var resourceId = Guid.NewGuid();
            const string resourceName = "Examples/Folder";
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
                ResourceID = resourceId,
                ResourceName = resourceName
            };

            var expectedResourceId = Guid.NewGuid();
            const string expectedResourceName= "Examples/Folder";
            var mockExplorerTreeItem = new Mock<IExplorerTreeItem>();
            mockExplorerTreeItem.Setup(o => o.ResourceId).Returns(expectedResourceId);
            mockExplorerTreeItem.Setup(o => o.ResourceName).Returns(expectedResourceName);
            mockExplorerTreeItem.Setup(o => o.ResourcePath).Returns(expectedResourceName);

            var picker = new Mock<IResourcePickerDialog>();
            picker.Setup(o => o.SelectResource(resourceId));
            picker.Setup(o => o.SelectedResource).Returns(mockExplorerTreeItem.Object);
            picker.Setup(o => o.ShowDialog(It.IsAny<IServer>())).Returns(true);

            var mockResourceModel = new Mock<IResourceModel>();
            mockResourceModel.Setup(o => o.ResourceName).Returns(expectedResourceName);
            mockResourceModel.Setup(o => o.ID).Returns(expectedResourceId);
            mockResourceModel.Setup(o => o.GetSavePath()).Returns(expectedResourceName);

            _resourceRepo.Setup(o => o.FindSingle(a => a.ID == resourceId))
                .Returns(mockResourceModel.Object);

            _environmentModel.Setup(m => m.ResourceRepository).Returns(_resourceRepo.Object);

            var viewModel = new SecurityViewModel(new SecuritySettingsTO(new[] {permission}), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, _environmentModel.Object, () => picker.Object);

            //------------Execute Test---------------------------
            viewModel.PickResourceCommand.Execute(permission);

            //------------Assert Results-------------------------
            Assert.AreEqual(expectedResourceId, permission.ResourceID);
            Assert.AreEqual(expectedResourceName, permission.ResourceName);
            Assert.AreEqual(expectedResourceName, permission.ResourcePath);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory(nameof(SecurityViewModel))]
        public void SecurityViewModel_HelpText_IsResourceHelpVisibleIsTrue_ContainsResourceHelpText()
        {
            //------------Setup for test--------------------------
            var viewModel = new SecurityViewModel(new SecuritySettingsTO(new WindowsGroupPermission[0]), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IServer>().Object, () => new Mock<IResourcePickerDialog>().Object) {IsResourceHelpVisible = true};

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.AreEqual(Warewolf.Studio.Resources.Languages.HelpText.SettingsSecurityResourceHelpResource, viewModel.HelpText);
            viewModel.UpdateHelpDescriptor("new help text");
            Assert.AreEqual("new help text", viewModel.HelpText);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory(nameof(SecurityViewModel))]
        public void SecurityViewModel_IsServerHelpVisible_ChangedToTrueAndIsResourceHelpVisibleIsTrue_IsResourceHelpVisibleIsFalse()
        {
            //------------Setup for test--------------------------
            var viewModel = new SecurityViewModel(new SecuritySettingsTO(new WindowsGroupPermission[0]), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IServer>().Object, () => new Mock<IResourcePickerDialog>().Object) {IsResourceHelpVisible = true, IsServerHelpVisible = true};

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.IsTrue(viewModel.IsServerHelpVisible);
            Assert.IsFalse(viewModel.IsResourceHelpVisible);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory(nameof(SecurityViewModel))]
        public void SecurityViewModel_IsResourceHelpVisible_ChangedToTrueAndIsServerHelpVisibleIsTrue_IsServerHelpVisibleIsFalse()
        {
            //------------Setup for test--------------------------
            var viewModel = new SecurityViewModel(new SecuritySettingsTO(new WindowsGroupPermission[0]), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IServer>().Object, () => new Mock<IResourcePickerDialog>().Object) {IsServerHelpVisible = true, IsResourceHelpVisible = true};

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.IsTrue(viewModel.IsResourceHelpVisible);
            Assert.IsFalse(viewModel.IsServerHelpVisible);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory(nameof(SecurityViewModel))]
        public void SecurityViewModel_CloseHelpCommand_IsServerHelpVisibleIsTrue_IsServerHelpVisibleIsFalse()
        {
            //------------Setup for test--------------------------
            var viewModel = new SecurityViewModel(new SecuritySettingsTO(new WindowsGroupPermission[0]), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IServer>().Object, () => new Mock<IResourcePickerDialog>().Object) {IsResourceHelpVisible = true};

            //------------Execute Test---------------------------
            viewModel.CloseHelpCommand.Execute(null);

            //------------Assert Results-------------------------
            Assert.IsFalse(viewModel.IsResourceHelpVisible);
            Assert.IsFalse(viewModel.IsServerHelpVisible);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory(nameof(SecurityViewModel))]
        public void SecurityViewModel_CloseHelpCommand_IsResourceHelpVisibleIsTrue_IsResourceHelpVisibleIsFalse()
        {
            //------------Setup for test--------------------------
            var viewModel = new SecurityViewModel(new SecuritySettingsTO(new WindowsGroupPermission[0]), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IServer>().Object, () => new Mock<IResourcePickerDialog>().Object) {IsResourceHelpVisible = true};

            //------------Execute Test---------------------------
            viewModel.CloseHelpCommand.Execute(null);

            //------------Assert Results-------------------------
            Assert.IsFalse(viewModel.IsResourceHelpVisible);
            Assert.IsFalse(viewModel.IsServerHelpVisible);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory(nameof(SecurityViewModel))]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SecurityViewModel_Save_NullPermissions_ThrowsArgumentNullException()
        {
            //------------Setup for test--------------------------
            var viewModel = new SecurityViewModel(new SecuritySettingsTO(new WindowsGroupPermission[0]), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IServer>().Object, () => new Mock<IResourcePickerDialog>().Object);

            //------------Execute Test---------------------------
            viewModel.Save(null);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory(nameof(SecurityViewModel))]
        public void SecurityViewModel_Save_InvalidPermissions_InvalidPermissionsAreRemoved()
        {
            //------------Setup for test--------------------------
            var permissions = CreatePermissions();

            var invalidPermission = permissions[permissions.Count - 1];
            invalidPermission.WindowsGroup = "";

            var viewModel = new SecurityViewModel(new SecuritySettingsTO(permissions), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IServer>().Object, () => new Mock<IResourcePickerDialog>().Object);

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
        [TestCategory(nameof(SecurityViewModel))]
        public void SecurityViewModel_Save_DeletedPermissions_DeletedPermissionsAreRemoved()
        {
            //------------Setup for test--------------------------
            var permissions = CreatePermissions();

            var deletedPermission = permissions[permissions.Count - 1];
            deletedPermission.IsDeleted = true;

            var viewModel = new SecurityViewModel(new SecuritySettingsTO(permissions), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IServer>().Object, () => new Mock<IResourcePickerDialog>().Object);

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
        [Owner("Sanele Mthembu")]
        [TestCategory(nameof(SecurityViewModel))]
        public void SecurityViewModel_HasInvalidResourcePermission_Given_Invalid_Resource_That_Is_Being_Deleted_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var securityViewModel = new SecurityViewModel(new SecuritySettingsTO(), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IServer>().Object, () => new Mock<IResourcePickerDialog>().Object);
            securityViewModel.ResourcePermissions.Add(new WindowsGroupPermission
            {
                ResourceName = "Resource1",
                WindowsGroup = "",
                IsDeleted = true,
                IsServer = false
            });
            //------------Execute Test---------------------------
            var hasDuplicateServerPermissions = securityViewModel.HasInvalidResourcePermission();
            //------------Assert Results-------------------------
            Assert.IsFalse(hasDuplicateServerPermissions);
        }


        [TestMethod]
        [Owner("Sanele Mthembu")]
        [TestCategory(nameof(SecurityViewModel))]
        public void SecurityViewModel_HasInvalidResourcePermission_Given_Resource_And_No_Group_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var securityViewModel = new SecurityViewModel(new SecuritySettingsTO(), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IServer>().Object, () => new Mock<IResourcePickerDialog>().Object);
            securityViewModel.ResourcePermissions.Add(new WindowsGroupPermission
            {
                ResourceName = "Resource1",
                WindowsGroup = "",
                IsServer = false
            });
            //------------Execute Test---------------------------
            var hasDuplicateServerPermissions = securityViewModel.HasInvalidResourcePermission();
            //------------Assert Results-------------------------
            Assert.IsTrue(hasDuplicateServerPermissions);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        [TestCategory(nameof(SecurityViewModel))]
        public void SecurityViewModel_HasInvalidResourcePermission_Given_Group_And_No_Resource_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var securityViewModel = new SecurityViewModel(new SecuritySettingsTO(), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IServer>().Object, () => new Mock<IResourcePickerDialog>().Object);
            securityViewModel.ResourcePermissions.Add(new WindowsGroupPermission
            {
                WindowsGroup = "Some Group",
                ResourceName = string.Empty,
                IsServer = false
            });
            //------------Execute Test---------------------------
            var hasDuplicateServerPermissions = securityViewModel.HasInvalidResourcePermission();
            //------------Assert Results-------------------------
            Assert.IsTrue(hasDuplicateServerPermissions);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(SecurityViewModel))]
        public void SecurityViewModel_ServerDuplicates_NoDuplicates_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var securityViewModel = new SecurityViewModel(new SecuritySettingsTO(), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IServer>().Object, () => new Mock<IResourcePickerDialog>().Object);
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
        [TestCategory(nameof(SecurityViewModel))]
        public void SecurityViewModel_ServerDuplicates_HasDuplicatesDeleted_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var securityViewModel = new SecurityViewModel(new SecuritySettingsTO(), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IServer>().Object, () => new Mock<IResourcePickerDialog>().Object);
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
        [TestCategory(nameof(SecurityViewModel))]
        public void SecurityViewModel_ServerDuplicates_Duplicates_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var securityViewModel = new SecurityViewModel(new SecuritySettingsTO(), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IServer>().Object, () => new Mock<IResourcePickerDialog>().Object);
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
        [TestCategory(nameof(SecurityViewModel))]
        public void SecurityViewModel_HasDuplicateResourcePermissions_NoDuplicatesResourceID_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var securityViewModel = new SecurityViewModel(new SecuritySettingsTO(), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IServer>().Object, () => new Mock<IResourcePickerDialog>().Object);
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
        [TestCategory(nameof(SecurityViewModel))]
        public void SecurityViewModel_HasDuplicateResourcePermissions_NoDuplicatesWindowsGroup_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var securityViewModel = new SecurityViewModel(new SecuritySettingsTO(), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IServer>().Object, () => new Mock<IResourcePickerDialog>().Object);
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
        [TestCategory(nameof(SecurityViewModel))]
        public void SecurityViewModel_HasDuplicateResourcePermissions_DuplicateDeleted_ReturnsFalse()
        {
            //------------Setup for test--------------------------
            var securityViewModel = new SecurityViewModel(new SecuritySettingsTO(), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IServer>().Object, () => new Mock<IResourcePickerDialog>().Object);
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
        [TestCategory(nameof(SecurityViewModel))]
        public void SecurityViewModel_ResourcePermissionsCompare_IsDeleted_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var securityViewModel = new SecurityViewModel(new SecuritySettingsTO(), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IServer>().Object, () => new Mock<IResourcePickerDialog>().Object);
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
            var invoke = methodInfo.Invoke(securityViewModel, new object[] {groupPermissions, true});
            //------------Assert Results-------------------------
            Assert.IsFalse(bool.Parse(invoke.ToString()));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(SecurityViewModel))]
        public void SecurityViewModel_ServerPermissionsCompare_IsDeleted_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var securityViewModel = new SecurityViewModel(new SecuritySettingsTO(), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IServer>().Object, () => new Mock<IResourcePickerDialog>().Object);
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
            var invoke = methodInfo.Invoke(securityViewModel, new object[] {groupPermissions, true});
            //------------Assert Results-------------------------
            Assert.IsFalse(bool.Parse(invoke.ToString()));
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(SecurityViewModel))]
        public void SecurityViewModel_HasDuplicateResourcePermissions_DuplicateNotDeleted_ReturnsTrue()
        {
            //------------Setup for test--------------------------
            var securityViewModel = new SecurityViewModel(new SecuritySettingsTO(), new Mock<DirectoryObjectPickerDialog>().Object, new Mock<IWin32Window>().Object, new Mock<IServer>().Object, () => new Mock<IResourcePickerDialog>().Object);
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

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(SecurityViewModel))]
        public void SecurityViewModel_PickOverrideResourceCommand_Sets_OverrideResource_Null()
        {
            //------------Setup for test--------------------------
            var resourceId = Guid.NewGuid();
            const string resourceName = "Cat\\Resource";
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
                ResourceID = resourceId,
                ResourceName = resourceName
            };
            var overrrideResourceId = Guid.NewGuid();
            var overrideResourceName = "AuthWorkflow";
            NamedGuid authenticationOverrideWorkflow = new NamedGuid
            {
                Value = overrrideResourceId,
                Name = overrideResourceName
            };
            var authResource = new NamedGuid()
            {
                Value = overrrideResourceId,
                Name = overrideResourceName
            };
            _serverRepo = GetEnvironmentRepository();
            _popupController = new Mock<IPopupController>();
            _resourceRepo = new Mock<IResourceRepository>();

            _firstResource = CreateResource(ResourceType.WorkflowService, overrideResourceName, overrrideResourceId);
            var coll = new Collection<IResourceModel> {_firstResource.Object};
            _resourceRepo.Setup(c => c.All()).Returns(coll);
            _environmentModel.Setup(m => m.ResourceRepository).Returns(_resourceRepo.Object);
            var secretKey = GenerateSecretKey();
            var viewModel = new SecurityViewModel(
                new SecuritySettingsTO(
                    new[]
                    {
                        permission
                    },
                    authenticationOverrideWorkflow,
                    secretKey),
                new Mock<DirectoryObjectPickerDialog>().Object,
                new Mock<IWin32Window>().Object,
                _environmentModel.Object, () => new Mock<IResourcePickerDialog>().Object);

            //------------Execute Test---------------------------
            viewModel.PickOverrideResourceCommand.Execute(null);

            //------------Assert Results-------------------------
            Assert.AreEqual(overrrideResourceId, viewModel.OverrideResource.Value);
            Assert.AreEqual(overrideResourceName, viewModel.OverrideResource.Name);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SecurityViewModel))]
        public void SecurityViewModel_PickOverrideResourceCommand_IsValidOverrideWorkflow_True()
        {
            //------------Setup for test--------------------------
            var overrideResourceId = Guid.NewGuid();
            var resourceId = Guid.NewGuid();
            const string overrideResourceName = "AuthWorkflow";
            const string resourceName = "Workflow";
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
                ResourceID = resourceId,
                ResourceName = resourceName
            };
            var publicPermission = new WindowsGroupPermission
            {
                IsServer = false,
                WindowsGroup = "Public",
                Execute = true,
                View = true,
                ResourceID = overrideResourceId,
                ResourceName = overrideResourceName,
            };

            var authenticationOverrideWorkflow = new NamedGuid
            {
                Value = resourceId,
                Name = resourceName
            };
            var authResource = new NamedGuid
            {
                Value = overrideResourceId,
                Name = overrideResourceName
            };
            var mockExplorerTreeItem = new Mock<IExplorerTreeItem>();
            var mockResourcePickerDialog = new Mock<IResourcePickerDialog>();
            var mockPopup = new Mock<IPopupController>();

            _serverRepo = GetEnvironmentRepository();

            var dataListItem = new DataListItem {Recordset = "UserGroups", Field = "Name"};
            var dataListItems = new OptomizedObservableCollection<IDataListItem> {dataListItem};

            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(o => o.GetOutputsFromWorkflow(overrideResourceId)).Returns(dataListItems);
            CustomContainer.Register(mockShellViewModel.Object);

            _firstResource = CreateResource(ResourceType.WorkflowService, overrideResourceName, overrideResourceId);
            var coll = new Collection<IResourceModel> {_firstResource.Object};
            _resourceRepo.Setup(c => c.All()).Returns(coll);
            _environmentModel.Setup(m => m.ResourceRepository).Returns(_resourceRepo.Object);

            mockExplorerTreeItem.Setup(explorerItem => explorerItem.ResourceId).Returns(overrideResourceId);
            mockExplorerTreeItem.Setup(explorerItem => explorerItem.ResourcePath).Returns(overrideResourceName);
            mockExplorerTreeItem.Setup(explorerItem => explorerItem.ResourceName).Returns(overrideResourceName);
            CustomContainer.Register(mockExplorerTreeItem.Object);
            mockResourcePickerDialog.Setup(resourcePicker => resourcePicker.SelectedResource).Returns(mockExplorerTreeItem.Object);
            mockResourcePickerDialog.Setup(resourcePicker => resourcePicker.ShowDialog(_environmentModel.Object)).Returns(true);
            CustomContainer.Register(mockResourcePickerDialog.Object);
            mockPopup.Setup(c => c.Show(It.IsAny<string>(), It.IsAny<string>(), MessageBoxButton.OK, MessageBoxImage.Error, "", false, false, true, false, false, false));
            var secretKey = GenerateSecretKey();
            var viewModel = new SecurityViewModel(
                new SecuritySettingsTO(new[] {permission, publicPermission}, authenticationOverrideWorkflow,secretKey),
                new Mock<DirectoryObjectPickerDialog>().Object,
                new Mock<IWin32Window>().Object,
                _environmentModel.Object, () => mockResourcePickerDialog.Object);

            //------------Execute Test---------------------------
            viewModel.PickOverrideResourceCommand.Execute(authResource);

            //------------Assert Results-------------------------
            Assert.AreEqual(overrideResourceId, viewModel.OverrideResource.Value);
            Assert.AreEqual(overrideResourceName, viewModel.OverrideResource.Name);
            mockShellViewModel.Verify(o => o.GetOutputsFromWorkflow(overrideResourceId), Times.Once);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SecurityViewModel))]
        public void SecurityViewModel_PickOverrideResourceCommand_IsValidOverrideWorkflow_GivenNoPublicPermissions_ExpectFalse()
        {
            //------------Setup for test--------------------------
            var overrideResourceId = Guid.NewGuid();
            var resourceId = Guid.NewGuid();
            const string overrideResourceName = "AuthWorkflow";
            const string resourceName = "Workflow";
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
                ResourceID = resourceId,
                ResourceName = resourceName
            };

            var authenticationOverrideWorkflow = new NamedGuid
            {
                Value = resourceId,
                Name = resourceName
            };
            var authResource = new NamedGuid
            {
                Value = overrideResourceId,
                Name = overrideResourceName
            };
            var mockExplorerTreeItem = new Mock<IExplorerTreeItem>();
            var mockResourcePickerDialog = new Mock<IResourcePickerDialog>();
            var mockPopup = new Mock<IPopupController>();

            _serverRepo = GetEnvironmentRepository();

            var dataListItem = new DataListItem {Recordset = "UserGroups", Field = "Name"};
            var dataListItems = new OptomizedObservableCollection<IDataListItem> {dataListItem};

            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(o => o.GetOutputsFromWorkflow(overrideResourceId)).Returns(dataListItems);
            CustomContainer.Register(mockShellViewModel.Object);

            _firstResource = CreateResource(ResourceType.WorkflowService, overrideResourceName, overrideResourceId);
            var coll = new Collection<IResourceModel> {_firstResource.Object};
            _resourceRepo.Setup(c => c.All()).Returns(coll);
            _environmentModel.Setup(m => m.ResourceRepository).Returns(_resourceRepo.Object);

            mockExplorerTreeItem.Setup(explorerItem => explorerItem.ResourceId).Returns(overrideResourceId);
            mockExplorerTreeItem.Setup(explorerItem => explorerItem.ResourcePath).Returns(overrideResourceName);
            mockExplorerTreeItem.Setup(explorerItem => explorerItem.ResourceName).Returns(overrideResourceName);
            CustomContainer.Register(mockExplorerTreeItem.Object);
            mockResourcePickerDialog.Setup(resourcePicker => resourcePicker.SelectedResource).Returns(mockExplorerTreeItem.Object);
            mockResourcePickerDialog.Setup(resourcePicker => resourcePicker.ShowDialog(_environmentModel.Object)).Returns(true);
            CustomContainer.Register(mockResourcePickerDialog.Object);
            mockPopup.Setup(c => c.Show(It.IsAny<string>(), It.IsAny<string>(), MessageBoxButton.OK, MessageBoxImage.Error, "", false, false, true, false, false, false));
            CustomContainer.Register<IPopupController>(mockPopup.Object);
            var secretKey = GenerateSecretKey();
            var viewModel = new SecurityViewModel(
                new SecuritySettingsTO(new[] {permission}, authenticationOverrideWorkflow,secretKey),
                new Mock<DirectoryObjectPickerDialog>().Object,
                new Mock<IWin32Window>().Object,
                _environmentModel.Object, () => mockResourcePickerDialog.Object);

            //------------Execute Test---------------------------
            viewModel.PickOverrideResourceCommand.Execute(authResource);

            //------------Assert Results-------------------------
            Assert.AreEqual(resourceId, viewModel.OverrideResource.Value);
            Assert.AreEqual(resourceName, viewModel.OverrideResource.Name);
            mockShellViewModel.Verify(o => o.GetOutputsFromWorkflow(overrideResourceId), Times.Never);
            mockPopup.Verify(o => o.Show(It.IsAny<IPopupMessage>()), Times.Once);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SecurityViewModel))]
        public void SecurityViewModel_PickOverrideResourceCommand_IsValidOverrideWorkflow_False()
        {
            //------------Setup for test--------------------------
            var overrideResourceId = Guid.NewGuid();
            var resourceId = Guid.NewGuid();
            const string overrideResourceName = "AuthWorkflow";
            const string resourceName = "Workflow";
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
                ResourceID = resourceId,
                ResourceName = resourceName
            };
            var publicPermission = new WindowsGroupPermission
            {
                IsServer = false,
                WindowsGroup = "Public",
                Execute = true,
                View = true,
                ResourceID = overrideResourceId,
                ResourceName = overrideResourceName,
            };

            var authenticationOverrideWorkflow = new NamedGuid
            {
                Value = resourceId,
                Name = resourceName
            };
            var authResource = new NamedGuid
            {
                Value = overrideResourceId,
                Name = overrideResourceName
            };
            var mockExplorerTreeItem = new Mock<IExplorerTreeItem>();
            var mockResourcePickerDialog = new Mock<IResourcePickerDialog>();

            _serverRepo = GetEnvironmentRepository();

            var dataListItem = new DataListItem {Recordset = "NoGroups", Field = "Name"};
            var dataListItems = new OptomizedObservableCollection<IDataListItem> {dataListItem};

            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(o => o.GetOutputsFromWorkflow(overrideResourceId)).Returns(dataListItems);
            CustomContainer.Register(mockShellViewModel.Object);

            _firstResource = CreateResource(ResourceType.WorkflowService, overrideResourceName, overrideResourceId);
            var coll = new Collection<IResourceModel> {_firstResource.Object};
            _resourceRepo.Setup(c => c.All()).Returns(coll);
            _environmentModel.Setup(m => m.ResourceRepository).Returns(_resourceRepo.Object);

            mockExplorerTreeItem.Setup(explorerItem => explorerItem.ResourceId).Returns(overrideResourceId);
            mockExplorerTreeItem.Setup(explorerItem => explorerItem.ResourcePath).Returns(overrideResourceName);
            mockExplorerTreeItem.Setup(explorerItem => explorerItem.ResourceName).Returns(overrideResourceName);
            CustomContainer.Register(mockExplorerTreeItem.Object);
            mockResourcePickerDialog.Setup(resourcePicker => resourcePicker.SelectedResource).Returns(mockExplorerTreeItem.Object);
            mockResourcePickerDialog.Setup(resourcePicker => resourcePicker.ShowDialog(_environmentModel.Object)).Returns(true);
            CustomContainer.Register(mockResourcePickerDialog.Object);

            var mockPopupController = new Mock<IPopupController>();
            mockPopupController.Setup(c => c.Show(It.IsAny<PopupMessage>()));
            CustomContainer.Register(mockPopupController.Object);

            var secretKey = GenerateSecretKey();
            var viewModel = new SecurityViewModel(
                new SecuritySettingsTO(new[] {permission, publicPermission}, authenticationOverrideWorkflow,secretKey),
                new Mock<DirectoryObjectPickerDialog>().Object,
                new Mock<IWin32Window>().Object,
                _environmentModel.Object, () => mockResourcePickerDialog.Object);

            //------------Execute Test---------------------------
            viewModel.PickOverrideResourceCommand.Execute(authResource);

            //------------Assert Results-------------------------
            Assert.AreEqual(resourceId, viewModel.OverrideResource.Value);
            Assert.AreEqual(resourceName, viewModel.OverrideResource.Name);
            mockShellViewModel.Verify(o => o.GetOutputsFromWorkflow(overrideResourceId), Times.Once);
            mockPopupController.Verify(o => o.Show(It.IsAny<PopupMessage>()), Times.Once);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SecurityViewModel))]
        public void SecurityViewModel_ShowDirectoryObjectPickerDialog()
        {
            var popupMessage = new PopupMessage
            {
                Description = "An error occured while trying to open the Windows Group Picker dialog. Please ensure that you are part of a Domain to use this feature.",
                Image = MessageBoxImage.Error,
                Buttons = MessageBoxButton.OK,
                IsError = true,
                Header = @"Error"
            };

            var mockPopupController = new Mock<IPopupController>();
            mockPopupController.Setup(o => o.Show(popupMessage)).Verifiable();
            CustomContainer.Register(mockPopupController.Object);

            var mockWin32Window = new Mock<IWin32Window>();
            var securityViewModel = new SecurityViewModel(new SecuritySettingsTO(), new Mock<DirectoryObjectPickerDialog>().Object, mockWin32Window.Object, new Mock<IServer>().Object, () => new Mock<IResourcePickerDialog>().Object);

            var dialogResult = securityViewModel.ShowDirectoryObjectPickerDialog(mockWin32Window.Object);
            Assert.AreEqual(DialogResult.Cancel, dialogResult);
            Assert.IsNotNull(securityViewModel.GetSelectedObjectsFromDirectoryObjectPickerDialog());
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SecurityViewModel))]
        public void SecurityViewModel_CreateResourcePickerDialog()
        {
            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(o => o.ActiveServer).Returns(new Mock<IServer>().Object);
            CustomContainer.Register(mockShellViewModel.Object);

            var mockWin32Window = new Mock<IWin32Window>();
            var securityViewModel = new SecurityViewModel(new SecuritySettingsTO(), new Mock<DirectoryObjectPickerDialog>().Object, mockWin32Window.Object, new Mock<IServer>().Object, () => new Mock<IResourcePickerDialog>().Object);

            var resourcePicker = securityViewModel.CreateResourcePickerDialog();
            Assert.IsNotNull(resourcePicker);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SecurityViewModel))]
        public void SecurityViewModel_Equals_ResourcePermissions_Null()
        {
            var groupPermission = new WindowsGroupPermission
            {
                IsServer = true, WindowsGroup = GlobalConstants.WarewolfGroup,
                View = false, Execute = false, Contribute = true, DeployTo = true, DeployFrom = true, Administrator = true
            };

            var groupPermissions = new List<WindowsGroupPermission>{groupPermission};

            var securitySettingsTo = new SecuritySettingsTO(groupPermissions);
            var mockPickerDialog = new Mock<DirectoryObjectPickerDialog>();
            var mockWin32Window = new Mock<IWin32Window>();
            var mockServer = new Mock<IServer>();
            var mockResourcePicker = new Mock<IResourcePickerDialog>();
            var viewModel = new SecurityViewModel(securitySettingsTo, mockPickerDialog.Object, mockWin32Window.Object, mockServer.Object, () => mockResourcePicker.Object)
            {
                ItemServerPermissions = new BindableCollection<WindowsGroupPermission> {groupPermission}
            };

            Assert.AreEqual(1, viewModel.ItemServerPermissions.Count);
            Assert.IsFalse(viewModel.IsDirty);
            viewModel.ItemServerPermissions[0].WindowsGroup = "User";
            Assert.IsTrue(viewModel.IsDirty);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SecurityViewModel))]
        public void SecurityViewModel_Equals_ServerPermissions_WindowsGroup()
        {
            var viewModel = PermissionsValidation();

            Assert.AreEqual(3, viewModel.ItemServerPermissions.Count);
            Assert.IsFalse(viewModel.IsDirty);

            viewModel.ItemServerPermissions[0].WindowsGroup = "User";
            Assert.IsTrue(viewModel.IsDirty);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SecurityViewModel))]
        public void SecurityViewModel_Equals_ServerPermissions_DeployTo()
        {
            var viewModel = PermissionsValidation();

            Assert.AreEqual(3, viewModel.ItemServerPermissions.Count);
            Assert.IsFalse(viewModel.IsDirty);
            viewModel.ItemServerPermissions[0].DeployTo = false;
            Assert.IsTrue(viewModel.IsDirty);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SecurityViewModel))]
        public void SecurityViewModel_Equals_ServerPermissions_DeployFrom()
        {
            var viewModel = PermissionsValidation();

            Assert.AreEqual(3, viewModel.ItemServerPermissions.Count);
            Assert.IsFalse(viewModel.IsDirty);
            viewModel.ItemServerPermissions[0].DeployFrom = false;
            Assert.IsTrue(viewModel.IsDirty);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SecurityViewModel))]
        public void SecurityViewModel_Equals_ServerPermissions_Administrator()
        {
            var viewModel = PermissionsValidation();

            Assert.AreEqual(3, viewModel.ItemServerPermissions.Count);
            Assert.IsFalse(viewModel.IsDirty);
            viewModel.ItemServerPermissions[0].Administrator = false;
            Assert.IsTrue(viewModel.IsDirty);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SecurityViewModel))]
        public void SecurityViewModel_Equals_ServerPermissions_View()
        {
            var viewModel = PermissionsValidation();

            Assert.AreEqual(3, viewModel.ItemServerPermissions.Count);
            Assert.IsFalse(viewModel.IsDirty);
            viewModel.ItemServerPermissions[0].View = true;
            Assert.IsTrue(viewModel.IsDirty);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SecurityViewModel))]
        public void SecurityViewModel_Equals_ServerPermissions_Execute()
        {
            var viewModel = PermissionsValidation();

            Assert.AreEqual(3, viewModel.ItemServerPermissions.Count);
            Assert.IsFalse(viewModel.IsDirty);
            viewModel.ItemServerPermissions[0].Execute = true;
            Assert.IsTrue(viewModel.IsDirty);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SecurityViewModel))]
        public void SecurityViewModel_Equals_ServerPermissions_Contribute()
        {
            var viewModel = PermissionsValidation();

            Assert.AreEqual(3, viewModel.ItemServerPermissions.Count);
            Assert.IsFalse(viewModel.IsDirty);
            viewModel.ItemServerPermissions[0].Contribute = false;
            Assert.IsTrue(viewModel.IsDirty);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SecurityViewModel))]
        public void SecurityViewModel_Equals_ServerPermissions_IsDeleted()
        {
            var viewModel = PermissionsValidation();

            Assert.AreEqual(3, viewModel.ItemServerPermissions.Count);
            Assert.IsFalse(viewModel.IsDirty);
            viewModel.ItemServerPermissions[0].IsDeleted = true;
            Assert.IsTrue(viewModel.IsDirty);
        }

        private static SecurityViewModel PermissionsValidation()
        {
            var serverPermission = new WindowsGroupPermission
            {
                IsServer = true, WindowsGroup = GlobalConstants.WarewolfGroup,
                View = false, Execute = false, Contribute = true, DeployTo = true, DeployFrom = true, Administrator = true
            };

            var resourcePermission = new WindowsGroupPermission
            {
                IsServer = true, WindowsGroup = GlobalConstants.WarewolfGroup,
                View = false, Execute = false, Contribute = true, DeployTo = true, DeployFrom = true, Administrator = true
            };

            var groupPermissions = new List<WindowsGroupPermission> {serverPermission, resourcePermission};

            var securitySettingsTo = new SecuritySettingsTO(groupPermissions);
            var mockPickerDialog = new Mock<DirectoryObjectPickerDialog>();
            var mockWin32Window = new Mock<IWin32Window>();
            var mockServer = new Mock<IServer>();
            var mockResourcePicker = new Mock<IResourcePickerDialog>();
            var viewModel = new SecurityViewModel(securitySettingsTo, mockPickerDialog.Object, mockWin32Window.Object,
                mockServer.Object, () => mockResourcePicker.Object)
            {
                ItemServerPermissions = new BindableCollection<WindowsGroupPermission>
                    {serverPermission, serverPermission, serverPermission},
                ItemResourcePermissions = new BindableCollection<WindowsGroupPermission> {resourcePermission},
                //ResourcePermissions = { resourcePermission }
            };
            return viewModel;
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SecurityViewModel))]
        public void SecurityViewModel_Equals_ResourcePermissions_ResourceName()
        {
            var viewModel = PermissionsValidation();

            Assert.AreEqual(1, viewModel.ItemResourcePermissions.Count);
            Assert.IsFalse(viewModel.IsDirty);
            viewModel.ItemResourcePermissions[0].ResourceName = "someName";
            Assert.IsTrue(viewModel.IsDirty);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SecurityViewModel))]
        public void SecurityViewModel_Equals_ResourcePermissions_WindowsGroup()
        {
            var viewModel = PermissionsValidation();

            Assert.AreEqual(1, viewModel.ItemResourcePermissions.Count);
            Assert.IsFalse(viewModel.IsDirty);
            viewModel.ItemResourcePermissions[0].WindowsGroup = "User";
            Assert.IsTrue(viewModel.IsDirty);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SecurityViewModel))]
        public void SecurityViewModel_Equals_ResourcePermissions_View()
        {
            var viewModel = PermissionsValidation();

            Assert.AreEqual(1, viewModel.ItemResourcePermissions.Count);
            Assert.IsFalse(viewModel.IsDirty);
            viewModel.ItemResourcePermissions[0].View = true;
            Assert.IsTrue(viewModel.IsDirty);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SecurityViewModel))]
        public void SecurityViewModel_Equals_ResourcePermissions_Execute()
        {
            var viewModel = PermissionsValidation();

            Assert.AreEqual(1, viewModel.ItemResourcePermissions.Count);
            Assert.IsFalse(viewModel.IsDirty);
            viewModel.ItemResourcePermissions[0].Execute = true;
            Assert.IsTrue(viewModel.IsDirty);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SecurityViewModel))]
        public void SecurityViewModel_Equals_ResourcePermissions_Contribute()
        {
            var viewModel = PermissionsValidation();

            Assert.AreEqual(1, viewModel.ItemResourcePermissions.Count);
            Assert.IsFalse(viewModel.IsDirty);
            viewModel.ItemResourcePermissions[0].Contribute = false;
            Assert.IsTrue(viewModel.IsDirty);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SecurityViewModel))]
        public void SecurityViewModel_Equals_ResourcePermissions_DeployTo()
        {
            var viewModel = PermissionsValidation();

            Assert.AreEqual(1, viewModel.ItemResourcePermissions.Count);
            Assert.IsFalse(viewModel.IsDirty);
            viewModel.ItemResourcePermissions[0].DeployTo = false;
            Assert.IsTrue(viewModel.IsDirty);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SecurityViewModel))]
        public void SecurityViewModel_Equals_ResourcePermissions_DeployFrom()
        {
            var viewModel = PermissionsValidation();

            Assert.AreEqual(1, viewModel.ItemResourcePermissions.Count);
            Assert.IsFalse(viewModel.IsDirty);
            viewModel.ItemResourcePermissions[0].DeployFrom = false;
            Assert.IsTrue(viewModel.IsDirty);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SecurityViewModel))]
        public void SecurityViewModel_Equals_ResourcePermissions_Administrator()
        {
            var viewModel = PermissionsValidation();

            Assert.AreEqual(1, viewModel.ItemResourcePermissions.Count);
            Assert.IsFalse(viewModel.IsDirty);
            viewModel.ItemResourcePermissions[0].Administrator = false;
            Assert.IsTrue(viewModel.IsDirty);
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