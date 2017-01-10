using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Common.Interfaces.Security;
using Dev2.Common.Interfaces.Studio.Controller;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Studio.Core;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class EnvironmentViewModelTests
    {
        #region Fields

        private EnvironmentViewModel _target;

        private Mock<IServer> _serverMock;
        private Mock<IShellViewModel> _shellViewModelMock;
        private Mock<IPopupController> _popupControllerMock;

        #endregion Fields

        #region Test initialize

        [TestInitialize]
        public void TestInitialize()
        {
            _serverMock  = new Mock<IServer>();
            _shellViewModelMock = new Mock<IShellViewModel>();
            _popupControllerMock = new Mock<IPopupController>();
            CustomContainer.Register(_popupControllerMock.Object);
            _target = new EnvironmentViewModel(_serverMock.Object, _shellViewModelMock.Object);
        }

        #endregion Test initialize

        #region Test commands

        
        [TestMethod]
        public void TestRefreshCommandChildrenAllowResourceCheck()
        {
            //arrange
            _target.ShowContextMenu = true;
            var child = new Mock<IExplorerItemViewModel>();
            child.SetupGet(it => it.AllowResourceCheck).Returns(true);
            child.SetupGet(it => it.IsVisible).Returns(true);
            _target.Children = new ObservableCollection<IExplorerItemViewModel>() {child.Object};

            //act
            _target.RefreshCommand.Execute(null);
            Assert.IsTrue(_target.RefreshCommand.CanExecute(null));

            //assert
            Assert.IsFalse(_target.ShowContextMenu);
        }

        [TestMethod]
        public void TestRefreshCommandChildrenNoAllowResourceCheck()
        {
            //arrange
            _target.ShowContextMenu = true;
            _target.Children = new ObservableCollection<IExplorerItemViewModel>() { };

            //act
            _target.RefreshCommand.Execute(null);
            Assert.IsTrue(_target.RefreshCommand.CanExecute(null));

            //assert
            Assert.IsTrue(_target.ShowContextMenu);
        }

        [TestMethod]
        public void TestCommands()
        {
            //arrange
            var canCreateNewServiceCommand = _target.NewServiceCommand.CanExecute(null);
            var canCreateNewServerCommand = _target.NewServerCommand.CanExecute(null);
            var canCreateNewSqlServerSourceCommand = _target.NewSqlServerSourceCommand.CanExecute(null);
            var canCreateNewMySqlSourceCommand = _target.NewMySqlSourceCommand.CanExecute(null);
            var canCreateNewPostgreSqlSourceCommand = _target.NewPostgreSqlSourceCommand.CanExecute(null);
            var canCreateNewOracleSourceCommand = _target.NewOracleSourceCommand.CanExecute(null);
            var canCreateNewOdbcSourceCommand = _target.NewOdbcSourceCommand.CanExecute(null);
            var canCreateNewPluginSourceCommand = _target.NewPluginSourceCommand.CanExecute(null);
            var canCreateNewWebSourceSourceCommand = _target.NewWebSourceSourceCommand.CanExecute(null);
            var canCreateNewEmailSourceSourceCommand = _target.NewEmailSourceSourceCommand.CanExecute(null);
            var canCreateNewExchangeSourceSourceCommand = _target.NewExchangeSourceSourceCommand.CanExecute(null);
            var canCreateNewSharepointSourceSourceCommand = _target.NewSharepointSourceSourceCommand.CanExecute(null);
            var canCreateNewDropboxSourceSourceCommand = _target.NewDropboxSourceSourceCommand.CanExecute(null);
            var canCreateNewComPluginSourceCommand = _target.NewComPluginSourceCommand.CanExecute(null);
            var canCreateNewRabbitMQSourceSourceCommand = _target.NewRabbitMQSourceSourceCommand.CanExecute(null);
            var canCreateFolderCommand = _target.CreateFolderCommand.CanExecute(null);

            //act

            //assert
            Assert.IsTrue(canCreateNewServiceCommand);
            Assert.IsTrue(canCreateNewServerCommand);
            Assert.IsTrue(canCreateNewSqlServerSourceCommand);
            Assert.IsTrue(canCreateNewMySqlSourceCommand);
            Assert.IsTrue(canCreateNewPostgreSqlSourceCommand);
            Assert.IsTrue(canCreateNewOracleSourceCommand);
            Assert.IsTrue(canCreateNewOdbcSourceCommand);
            Assert.IsTrue(canCreateNewPluginSourceCommand);
            Assert.IsTrue(canCreateNewWebSourceSourceCommand);
            Assert.IsTrue(canCreateNewEmailSourceSourceCommand);
            Assert.IsTrue(canCreateNewExchangeSourceSourceCommand);
            Assert.IsTrue(canCreateNewSharepointSourceSourceCommand);
            Assert.IsTrue(canCreateNewDropboxSourceSourceCommand);
            Assert.IsTrue(canCreateNewComPluginSourceCommand);
            Assert.IsTrue(canCreateNewRabbitMQSourceSourceCommand);
            Assert.IsTrue(canCreateFolderCommand);
        }

        [TestMethod]
        public void TestExpand()
        {
            //arrange
            _target.IsExpanded = false;
            _target.ForcedRefresh = true;

            //act
            _target.Expand.Execute(2);
            Assert.IsTrue(_target.Expand.CanExecute(2));

            //assert
            Assert.IsTrue(_target.IsExpanded);
        }

        [TestMethod]
        public void TestShowServerVersionCommand()
        {
            //arrange

            //act
            _target.ShowServerVersionCommand.Execute(null);
            Assert.IsTrue(_target.ShowServerVersionCommand.CanExecute(null));

            //assert
            _shellViewModelMock.Verify(it=>it.ShowAboutBox());
        }

        #endregion Test commands

        #region Test properties

        [TestMethod]
        public void TestShellViewModel()
        {
            //arrange
            //act
            var value = _target.ShellViewModel;

            //assert    
            Assert.AreSame(_shellViewModelMock.Object, value);
        }

        [TestMethod]
        public void TestChildrenCount()
        {
            //arrange
            var childVersion = new Mock<IExplorerItemViewModel>();
            childVersion.SetupGet(it => it.IsVisible).Returns(true);
            childVersion.SetupGet(it => it.ResourceType).Returns("Version");
            var childMessage = new Mock<IExplorerItemViewModel>();
            childMessage.SetupGet(it => it.IsVisible).Returns(true);
            childMessage.SetupGet(it => it.ResourceType).Returns("Message");
            var childFolder = new Mock<IExplorerItemViewModel>();
            childFolder.SetupGet(it => it.IsVisible).Returns(true);
            childFolder.SetupGet(it => it.ResourceType).Returns("Folder");
            childFolder.SetupGet(it => it.IsFolder).Returns(true);
            childFolder.SetupGet(it => it.ChildrenCount).Returns(2);
            var childDbService = new Mock<IExplorerItemViewModel>();
            childDbService.SetupGet(it => it.IsVisible).Returns(true);
            childDbService.SetupGet(it => it.ResourceName).Returns("DbSource");
            childDbService.SetupGet(it => it.ResourcePath).Returns("DbSourcePath");
            childDbService.SetupGet(it => it.IsSource).Returns(true);
            _target.Children = new ObservableCollection<IExplorerItemViewModel>()
            {
                childVersion.Object,
                childMessage.Object,
                childFolder.Object,
                childDbService.Object
            };

            //act
            _target.IsSource = true;
            _target.IsService = false;
            _target.IsFolder = false;
            _target.IsReservedService = false;
            _target.IsResourceVersion = false;
            _target.IsServer = false;
            _target.ResourcePath = "";
            _target.ResourceName = "localhost";

            _target.UpdateChildrenCount();
            var value = _target.ChildrenCount;

            //assert    
            Assert.AreEqual(4, value);
            Assert.AreEqual("ServerSource", _target.ResourceType);
            Assert.AreEqual("localhost", _target.ResourceName);
            Assert.AreEqual(null, _target.Parent);
            Assert.AreEqual("", _target.ResourcePath);
            Assert.AreEqual(true, _target.IsSource);
            Assert.AreEqual(false, _target.IsService);
            Assert.AreEqual(false, _target.IsFolder);
            Assert.AreEqual(false, _target.IsReservedService);
            Assert.AreEqual(false, _target.IsResourceVersion);
            Assert.AreEqual(false, _target.IsServer);
            Assert.AreEqual("DbSource", _target.Children[3].ResourceName);
            Assert.AreEqual("DbSourcePath", _target.Children[3].ResourcePath);
            Assert.AreEqual(false, _target.Children[3].IsExpanderVisible);
            Assert.AreEqual(true, _target.Children[3].IsSource);
        }

        [TestMethod]
        public void TestShowContextMenu()
        {
            //arrange
            var isShowContextMenuChanged = false;
            _target.ShowContextMenu = false;
            _target.PropertyChanged += (s, e) =>
            {
                isShowContextMenuChanged = isShowContextMenuChanged || e.PropertyName == "ShowContextMenu";
            };

            //act
            _target.ShowContextMenu = !_target.ShowContextMenu;

            //assert
            Assert.IsTrue(_target.ShowContextMenu);
            Assert.IsTrue(isShowContextMenuChanged);
        }

        [TestMethod]
        public void TestIsFolderChecked()
        {
            //arrange
            var isResourceChanged = false;
            _target.IsResourceChecked = false;            
            _target.PropertyChanged += (s, e) =>
            {
                isResourceChanged = isResourceChanged || e.PropertyName == "IsResourceChecked";
            };            
            //act
            _target.IsFolderChecked = !_target.IsFolderChecked;
            //assert
            Assert.IsTrue(_target.IsFolderChecked.HasValue && !_target.IsFolderChecked.Value);
            Assert.IsTrue(isResourceChanged);
        }

        [TestMethod]
        public void TestIsVisible()
        {
            //arrange
            var isIsVisibleChanged = false;
            _target.IsVisible = false;
            _target.PropertyChanged += (s, e) =>
            {
                isIsVisibleChanged = isIsVisibleChanged || e.PropertyName == "IsVisible";
            };

            //act
            _target.IsVisible = !_target.IsVisible;

            //assert
            Assert.IsTrue(_target.IsVisible);
            Assert.IsTrue(isIsVisibleChanged);
        }

        [TestMethod]
        public void TestIsSelected()
        {
            //arrange
            var isIsSelectedChanged = false;
            _target.IsSelected = false;
            _target.PropertyChanged += (s, e) =>
            {
                isIsSelectedChanged = isIsSelectedChanged || e.PropertyName == "IsSelected";
            };

            //act
            _target.IsSelected = !_target.IsSelected;

            //assert
            Assert.IsTrue(_target.IsSelected);
            Assert.IsTrue(isIsSelectedChanged);
            Assert.IsFalse(_target.CanDrag);
            Assert.IsFalse(_target.CanDrop);
        }

        [TestMethod]
        public void TestCanShowServerVersion()
        {
            //arrange
            var isCanShowServerVersionChanged = false;
            _target.CanShowServerVersion = false;
            _target.PropertyChanged += (s, e) =>
            {
                isCanShowServerVersionChanged = isCanShowServerVersionChanged || e.PropertyName == "CanShowServerVersion";
            };

            //act
            _target.CanShowServerVersion = !_target.CanShowServerVersion;

            //assert
            Assert.IsTrue(_target.CanShowServerVersion);
            Assert.IsTrue(isCanShowServerVersionChanged);
        }

        [TestMethod]
        public void TestAllowResourceCheck()
        {
            //arrange
            var isAllowResourceCheckChanged = false;
            _target.AllowResourceCheck = false;
            _target.PropertyChanged += (s, e) =>
            {
                isAllowResourceCheckChanged = isAllowResourceCheckChanged || e.PropertyName == "AllowResourceCheck";
            };

            //act
            _target.AllowResourceCheck = !_target.AllowResourceCheck;

            //assert
            Assert.IsTrue(_target.AllowResourceCheck);
            Assert.IsTrue(isAllowResourceCheckChanged);
        }

        [TestMethod]
        public void TestCanCreateWorkflowService()
        {
            //arrange
            var isCanCreateWorkflowServiceChanged = false;
            _target.CanCreateWorkflowService = false;
            _target.PropertyChanged += (s, e) =>
            {
                isCanCreateWorkflowServiceChanged = isCanCreateWorkflowServiceChanged || e.PropertyName == "CanCreateWorkflowService";
            };

            //act
            _target.CanCreateWorkflowService = !_target.CanCreateWorkflowService;

            //assert
            Assert.IsTrue(_target.ShowContextMenu);
            Assert.IsTrue(isCanCreateWorkflowServiceChanged);
        }

        [TestMethod]
        public void TestCanCreateFolder()
        {
            //arrange
            var isCanCreateFolderChanged = false;
            _target.CanCreateFolder = false;
            _target.PropertyChanged += (s, e) =>
            {
                isCanCreateFolderChanged = isCanCreateFolderChanged || e.PropertyName == "CanCreateFolder";
            };

            //act
            _target.CanCreateFolder = true;

            //assert
            Assert.IsFalse(_target.CanCreateFolder);
            Assert.IsTrue(isCanCreateFolderChanged);
        }

        [TestMethod]
        public void TestCanCreateFolderTrue()
        {
            //arrange
            _target.CanCreateFolder = false;
            var windowsGroupPermissionMock = new Mock<IWindowsGroupPermission>();
            windowsGroupPermissionMock.SetupGet(it => it.Contribute).Returns(true);
            windowsGroupPermissionMock.SetupGet(it => it.Administrator).Returns(true);
            windowsGroupPermissionMock.SetupGet(it => it.IsServer).Returns(true);
            _serverMock.SetupGet(it => it.Permissions).Returns(new List<IWindowsGroupPermission>()
            {
                windowsGroupPermissionMock.Object
            });

            //act
            var actual = _target.CanCreateFolder;

            //assert
            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void TestAreVersionsVisible()
        {
            //arrange
            _target.AreVersionsVisible = false;

            //act
            _target.AreVersionsVisible = !_target.AreVersionsVisible;

            //assert
            Assert.IsTrue(_target.AreVersionsVisible);
        }

        [TestMethod]
        public void TestIsResourceUnchecked()
        {
            //arrange
            _target.IsResourceUnchecked = false;

            //act
            _target.IsResourceUnchecked = !_target.IsResourceUnchecked;

            //assert
            Assert.IsTrue(_target.IsResourceUnchecked ?? false);
        }

        [TestMethod]
        public void TestIsExpanderVisibleTrue()
        {
            //arrange
            var child = new Mock<IExplorerItemViewModel>();
            child.SetupGet(it => it.AllowResourceCheck).Returns(true);
            child.SetupGet(it => it.IsVisible).Returns(true);
            _target.Children = new ObservableCollection<IExplorerItemViewModel>() { child.Object };
           

            //act
            var actual = _target.IsExpanderVisible;

            //assert
            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void TestIsExpanderVisibleFalse()
        {
            //arrange
            _target.Children = new ObservableCollection<IExplorerItemViewModel>() {  };


            //act
            var actual = _target.IsExpanderVisible;

            //assert
            Assert.IsFalse(actual);
        }

        #endregion Test properties

        #region Test methods

        [TestMethod]
        [ExpectedException(typeof (ArgumentNullException))]
        public void TestConstructorServerNull()
        {
            new EnvironmentViewModel(null, _shellViewModelMock.Object);
        }

        [TestMethod]
        [ExpectedException(typeof (ArgumentNullException))]
        public void TestConstructorShellViewMockNull()
        {
            new EnvironmentViewModel(_serverMock.Object, null);
        }

        [TestMethod]
        public void TestSelectItemGuid()
        {
            //arrange
            var child = new Mock<IExplorerItemViewModel>();
            child.SetupGet(it => it.ResourceId).Returns(Guid.Empty);
            child.SetupGet(it => it.IsVisible).Returns(true);
            child.SetupGet(it => it.ResourceName).Returns("child");
            _target.AddChild(child.Object);
            var id = Guid.NewGuid();
            var child2 = new Mock<IExplorerItemViewModel>();
            child2.SetupGet(it => it.IsVisible).Returns(true);
            child2.SetupGet(it => it.ResourceId).Returns(id);
            child2.SetupGet(it => it.IsExpanded).Returns(false);
            child2.SetupGet(it => it.ResourceName).Returns("child2");
            _target.AddChild(child2.Object);

            //act
            _target.SelectItem(id, a=> {});

            //assert
            child.Verify(a => a.SelectItem(id, It.IsAny<Action<IExplorerItemViewModel>>()));
            child2.VerifySet(it => it.IsExpanded = true);
            child2.VerifySet(it => it.IsSelected = true);
        }

        [TestMethod]
        public void TestSelectItemStr()
        {
            //arrange
            var resourcePath = "somePath";
            var child = new Mock<IExplorerItemViewModel>();
            child.SetupGet(it => it.IsVisible).Returns(true);
            child.SetupGet(it => it.ResourcePath).Returns(resourcePath);
            child
                .Setup(it => it.Apply(It.IsAny<Action<IExplorerItemViewModel>>()))
                .Callback<Action<IExplorerItemViewModel>>(a => a(child.Object));
            _target.AddChild(child.Object);

            //act
            _target.SelectItem(resourcePath, a => { });

            //assert
            child.VerifySet(it => it.IsExpanded = true);
        }

        [TestMethod]
        public void TestDispose()
        {
            //arrange
            var child = new Mock<IExplorerItemViewModel>();
            _target.AddChild(child.Object);

            //act
            _target.Dispose();

            //assert
            child.Verify(a => a.Dispose());
        }

        [TestMethod]
        public void EnvironmentViewModelVerifyNewFolderShowsContextMenu_HasParentPermissions()
        {
            //arrange
            var explorerRepositoryMock = new Mock<IExplorerRepository>();
            _serverMock.Setup(a => a.ExplorerRepository).Returns(explorerRepositoryMock.Object);
            _target.CanCreateSource = true;
            _target.ShowContextMenu = true;
            var server = new Mock<IServer>();
            server.Setup(server1 => server1.UserPermissions).Returns(Permissions.Administrator);
            server.Setup(server1 => server1.GetPermissions(It.IsAny<Guid>())).Returns(Permissions.Administrator);
            _target.Server = server.Object;
            //act
            _target.CreateFolder();

            //assert
            Assert.AreEqual(_target.Children.Count,1);
            Assert.IsTrue(_target.Children[0].CanCreateSource);
            Assert.IsTrue(_target.Children[0].ShowContextMenu);
            Assert.IsTrue(_target.Children[0].CanDelete);
            Assert.IsTrue(_target.Children[0].CanCreateFolder);
            Assert.IsFalse(_target.Children[0].CanShowVersions);
            Assert.IsTrue(_target.Children[0].CanCreateWorkflowService);
        }

        [TestMethod]
        public void TestCreateFolder()
        {
            //arrange
            var explorerRepositoryMock = new Mock<IExplorerRepository>();
            _serverMock.SetupGet(it => it.ExplorerRepository).Returns(explorerRepositoryMock.Object);
            var isChildrenChanged = false;
            _target.CanCreateWorkflowService = false;
            _target.Children = new ObservableCollection<IExplorerItemViewModel>();
            _target.PropertyChanged += (s, e) =>
            {
                isChildrenChanged = isChildrenChanged || e.PropertyName == "Children";
            };

            //act
            _target.CreateFolder();

            //assert
            var folder = _target.Children[0];
            Assert.IsTrue(isChildrenChanged);
            Assert.IsTrue(_target.IsExpanded);
            Assert.AreEqual(_target.AllowResourceCheck, folder.AllowResourceCheck);
            Assert.AreEqual(_target.IsResourceChecked, folder.IsResourceChecked);
            Assert.AreEqual(_target.CanCreateFolder, folder.CanCreateFolder);
            Assert.AreEqual(_target.CanCreateSource, folder.CanCreateSource);
            Assert.AreEqual(_target.CanShowVersions, folder.CanShowVersions);
            Assert.IsTrue(folder.CanRename);
            Assert.AreEqual(_target.CanDeploy, folder.CanDeploy);
            Assert.AreEqual(_target.CanCreateWorkflowService, folder.CanCreateWorkflowService);
            Assert.AreEqual(_target.ShowContextMenu, folder.ShowContextMenu);
        }

        [TestMethod]
        public void TestCreateFolderIsDialog()
        {
            //arrange
            _target = new EnvironmentViewModel(_serverMock.Object, _shellViewModelMock.Object, true);
            var explorerRepositoryMock = new Mock<IExplorerRepository>();
            _serverMock.SetupGet(it => it.ExplorerRepository).Returns(explorerRepositoryMock.Object);
            var isChildrenChanged = false;
            _target.CanCreateWorkflowService = false;
            _target.Children = new ObservableCollection<IExplorerItemViewModel>();
            _target.PropertyChanged += (s, e) =>
            {
                isChildrenChanged = isChildrenChanged || e.PropertyName == "Children";
            };

            //act
            _target.CreateFolder();

            //assert
            var folder = _target.Children[0];
            Assert.IsTrue(isChildrenChanged);
            Assert.IsTrue(_target.IsExpanded);
            Assert.IsFalse(folder.AllowResourceCheck);
            Assert.IsFalse(folder.IsResourceChecked.HasValue && folder.IsResourceChecked.Value);
            Assert.IsFalse(folder.CanCreateSource);
            Assert.IsFalse(folder.CanCreateWorkflowService);
            Assert.IsFalse(folder.ShowContextMenu);
            Assert.IsFalse(folder.CanDeploy);
            Assert.IsFalse(folder.CanShowDependencies);
           
        }

        [TestMethod]
        public void TestSetPropertiesForDialogIsDialogFalse()
        {
            //arrange
            var windowsGroupPermissionMock = new Mock<IWindowsGroupPermission>();
            windowsGroupPermissionMock.SetupGet(it => it.Contribute).Returns(true);
            windowsGroupPermissionMock.SetupGet(it => it.Administrator).Returns(true);
            windowsGroupPermissionMock.SetupGet(it => it.IsServer).Returns(true);
            _serverMock.SetupGet(it => it.Permissions).Returns(new List<IWindowsGroupPermission>()
            {
                windowsGroupPermissionMock.Object
            });

            //act
            _target.SetPropertiesForDialog();

            //assert
            Assert.IsFalse(_target.AllowResourceCheck);
            Assert.IsFalse(_target.IsResourceChecked ?? true);
            Assert.IsTrue(_target.CanCreateSource);
            Assert.IsTrue(_target.CanCreateFolder);
            Assert.IsFalse(_target.CanDelete);
            Assert.IsFalse(_target.CanDeploy);
            Assert.IsFalse(_target.CanRename);
            Assert.IsFalse(_target.CanRollback);
            Assert.IsFalse(_target.CanShowVersions);
            Assert.IsTrue(_target.CanCreateWorkflowService);
            Assert.IsTrue(_target.ShowContextMenu);
        }

        [TestMethod]
        public void TestSetPropertiesForDialogIsDialogTrue()
        {
            //arrange
            _target = new EnvironmentViewModel(_serverMock.Object, _shellViewModelMock.Object, true);

            //act
            _target.SetPropertiesForDialog();

            //assert
            Assert.IsFalse(_target.AllowResourceCheck);
            Assert.IsFalse(_target.IsResourceChecked ?? true);
            Assert.IsFalse(_target.CanCreateSource);
            Assert.IsFalse(_target.CanCreateWorkflowService);
            Assert.IsFalse(_target.ShowContextMenu);
            Assert.IsFalse(_target.CanDeploy);
        }

        [TestMethod]
        public void TestRemoveChild()
        {
            //arrange
            var child = new Mock<IExplorerItemViewModel>();
            child.SetupGet(it => it.AllowResourceCheck).Returns(true);
            child.SetupGet(it => it.IsVisible).Returns(true);
            _target.Children = new ObservableCollection<IExplorerItemViewModel>() { child.Object };
            var isChildrenChanged = false;
            _target.PropertyChanged += (s, e) =>
             {
                 isChildrenChanged = isChildrenChanged || e.PropertyName == "Children";
             };

            //act
            _target.RemoveChild(child.Object);

            //assert
            Assert.IsFalse(_target.Children.Any());
            Assert.IsTrue(isChildrenChanged);
        }

        [TestMethod]
        public void TestRemoveItem()
        {
            //arrange
            var vm = new Mock<IExplorerItemViewModel>();
            vm.SetupGet(it=>it.ResourceType).Returns("Folder");
            var resId = Guid.NewGuid();
            vm.SetupGet(it => it.ResourceId).Returns(resId);
            var child = new Mock<IExplorerItemViewModel>();
            var grandChild = new Mock<IExplorerItemViewModel>();
            grandChild.SetupGet(it => it.ResourceId).Returns(resId);
            grandChild.SetupGet(it => it.IsVisible).Returns(true);
            child.SetupGet(it => it.IsVisible).Returns(true);
            child.SetupGet(it => it.Children).Returns(new ObservableCollection<IExplorerItemViewModel>() {grandChild.Object});

            _target.Children = new ObservableCollection<IExplorerItemViewModel>() { child.Object };
            var isChildrenChanged = false;
            _target.PropertyChanged += (s, e) =>
            {
                isChildrenChanged = isChildrenChanged || e.PropertyName == "Children";
            };

            //act
            _target.RemoveItem(vm.Object);

            //assert
            child.Verify(it=>it.RemoveChild(grandChild.Object));
            Assert.IsTrue(isChildrenChanged);
        }

        [TestMethod]
        public void TestSetItemCheckedState()
        {
            //arrange
            var child = new Mock<IExplorerItemViewModel>();
            var id = Guid.NewGuid();
            child.SetupGet(it => it.ResourceId).Returns(id);
            child.SetupGet(it => it.IsVisible).Returns(true);
            _target.Children = new ObservableCollection<IExplorerItemViewModel>() { child.Object };

            //act
            _target.SetItemCheckedState(id, true);

            //assert
            child.VerifySet(it => it.Checked = true);
        }

        [TestMethod]
        public void TestFilter()
        {
            //arrange
            var child = new Mock<IExplorerItemViewModel>();
            child.SetupGet(it => it.IsVisible).Returns(true);
            _target.Children = new ObservableCollection<IExplorerItemViewModel>() { child.Object };
            var isChildrenChanged = false;
            _target.PropertyChanged += (s, e) =>
            {
                isChildrenChanged = isChildrenChanged || e.PropertyName == "Children";
            };
            var filterText = "someFilterText";

            //act
            _target.Filter(filterText);

            //assert
            child.Verify(it=>it.Filter(filterText));
            Assert.IsTrue(isChildrenChanged);
        }

        [TestMethod]
        public void TestFilterFunc()
        {
            //arrange
            var child = new Mock<IExplorerItemViewModel>();
            child.SetupGet(it => it.IsVisible).Returns(true);
            _target.Children = new ObservableCollection<IExplorerItemViewModel>() { child.Object };
            var isChildrenChanged = false;
            _target.PropertyChanged += (s, e) =>
            {
                isChildrenChanged = isChildrenChanged || e.PropertyName == "Children";
            };

            //act
            _target.Filter(a => true);

            //assert
            child.Verify(it => it.Filter(It.IsAny<Func<IExplorerItemViewModel, bool>>()));
            Assert.IsTrue(isChildrenChanged);
        }

        [TestMethod]
        public async Task TestLoadDialog()
        {
            //arrange
            var selPath = Guid.NewGuid();
            var explorerItemMock = new Mock<IExplorerItem>();
            _serverMock.SetupGet(it=>it.IsConnected).Returns(true);
            _serverMock.Setup(it => it.LoadExplorer(false)).Returns(Task.FromResult(explorerItemMock.Object));
            _target = new EnvironmentViewModel(_serverMock.Object, _shellViewModelMock.Object);

            //act
            var result = await _target.LoadDialog(selPath);

            //assert
            Assert.IsFalse(_target.Children.Any());
        }

        [TestMethod]
        public void TestCreateExplorerItems()
        {
            //arrange
            var explorerItem = new Mock<IExplorerItem>();
            explorerItem.SetupGet(it => it.DisplayName).Returns("someDisplayName");
            explorerItem.SetupGet(it => it.ResourceId).Returns(Guid.NewGuid());
            explorerItem.SetupGet(it => it.ResourceType).Returns("Folder");
            explorerItem.SetupGet(it => it.IsFolder).Returns(true);
            explorerItem.SetupGet(it => it.ResourcePath).Returns("someDisplayName");
            var childExplorerItem = new Mock<IExplorerItem>();
            childExplorerItem.SetupGet(it => it.DisplayName).Returns("someDisplayName");
            childExplorerItem.SetupGet(it => it.ResourceId).Returns(Guid.NewGuid());
            childExplorerItem.SetupGet(it => it.ResourceType).Returns("Folder");
            childExplorerItem.SetupGet(it => it.IsFolder).Returns(true);
            childExplorerItem.SetupGet(it => it.ResourcePath).Returns("someDisplayName");
            var parentMock = new Mock<IExplorerTreeItem>();
            var collectionParent = new AsyncObservableCollection<IExplorerItemViewModel>();
            parentMock.SetupGet(it => it.Children).Returns(collectionParent);
            var collectionItem = new AsyncObservableCollection<IExplorerItem>() {childExplorerItem.Object};
            explorerItem.SetupGet(it => it.Children).Returns(collectionItem);
            var serverMock = new Mock<IServer>();
            var permissions = new List<IWindowsGroupPermission>();
            serverMock.SetupGet(it => it.Permissions).Returns(permissions);
            var items = new List<IExplorerItem>() {explorerItem.Object};
            _target.SelectAction = (a) => { };

            //act
            _target.CreateExplorerItemsSync(items,serverMock.Object,parentMock.Object,true,true);

            //assert
            Assert.IsFalse(_target.Children.Any());
            parentMock.VerifySet(it => it.Children = It.IsAny<ObservableCollection<IExplorerItemViewModel>>());
          //  Assert.IsTrue(collection.Any());
        }

        [TestMethod]
        public void TestCreateExplorerItemsSetsPermissions()
        {
            //arrange
            var explorerItem = new Mock<IExplorerItem>();
            explorerItem.SetupGet(it => it.DisplayName).Returns("someDisplayName");
            explorerItem.SetupGet(it => it.ResourceId).Returns(Guid.NewGuid());
            explorerItem.SetupGet(it => it.ResourceType).Returns("Folder");
            explorerItem.SetupGet(it => it.IsFolder).Returns(true);
            explorerItem.SetupGet(it => it.ResourcePath).Returns("someDisplayName");
            var childExplorerItem = new Mock<IExplorerItem>();
            childExplorerItem.SetupGet(it => it.DisplayName).Returns("someDisplayName");
            childExplorerItem.SetupGet(it => it.ResourceId).Returns(Guid.NewGuid());
            childExplorerItem.SetupGet(it => it.ResourceType).Returns("Folder");
            childExplorerItem.SetupGet(it => it.IsFolder).Returns(true);
            childExplorerItem.SetupGet(it => it.ResourcePath).Returns("someDisplayName");
            var parentMock = new Mock<IExplorerTreeItem>();
            var child = new Mock<IExplorerItemViewModel>();
            child.SetupGet(it => it.ResourcePath).Returns("someDisplayName");
            var collectionParent = new AsyncObservableCollection<IExplorerItemViewModel>() { child.Object };
            parentMock.SetupGet(it => it.Children).Returns(collectionParent);
            var collectionItem = new AsyncObservableCollection<IExplorerItem>() {childExplorerItem.Object};
            explorerItem.SetupGet(it => it.Children).Returns(collectionItem);
            var serverMock = new Mock<IServer>();
            var permissions = new List<IWindowsGroupPermission>();
            serverMock.SetupGet(it => it.Permissions).Returns(permissions);
            var items = new List<IExplorerItem>() {explorerItem.Object};
            _target.SelectAction = (a) => { };

            //act
            _target.CreateExplorerItemsSync(items,serverMock.Object,parentMock.Object,true,true);

            //assert
            Assert.IsFalse(_target.Children.Any());

            child.Verify(model => model.SetPermissions(It.IsAny<Permissions>(),It.IsAny<bool>()));
            parentMock.VerifySet(it => it.Children = It.IsAny<ObservableCollection<IExplorerItemViewModel>>());
          //  Assert.IsTrue(collection.Any());
        }

        #endregion Test methods
    }
}