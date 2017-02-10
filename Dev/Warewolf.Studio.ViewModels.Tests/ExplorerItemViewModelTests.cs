using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Common.Interfaces.PopupController;
using Dev2.Common.Interfaces.Security;
using Dev2.Common.Interfaces.Threading;
using Dev2.Common.Interfaces.Versioning;
using Dev2.Studio.Core.Interfaces;
using Dev2.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using IPopupController = Dev2.Common.Interfaces.Studio.Controller.IPopupController;
// ReSharper disable PossibleUnintendedReferenceComparison
// ReSharper disable InconsistentNaming

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class ExplorerItemViewModelTests
    {
        #region Fields

        private ExplorerItemViewModel _target;
        private Mock<IServer> _serverMock;
        private Mock<IStudioUpdateManager> _updateManager;
        private Mock<IExplorerTreeItem> _explorerTreeItemMock;
        private Mock<IShellViewModel> _shellViewModelMock;
        private Mock<IExplorerRepository> _explorerRepositoryMock;
        private Mock<IPopupController> _popupControllerMock;
        private Mock<IWindowsGroupPermission> _windowsGroupPermissionsMock;

        #endregion Fields

        #region Test initialize

        [TestInitialize]
        public void TestInitialize()
        {
            _serverMock = new Mock<IServer>();
            _updateManager = new Mock<IStudioUpdateManager>();
            _serverMock.Setup(a => a.UpdateRepository).Returns(_updateManager.Object);

            _explorerTreeItemMock = new Mock<IExplorerTreeItem>();
            _shellViewModelMock = new Mock<IShellViewModel>();
            _explorerRepositoryMock = new Mock<IExplorerRepository>();
            _windowsGroupPermissionsMock = new Mock<IWindowsGroupPermission>();
            _serverMock.SetupGet(it => it.ExplorerRepository).Returns(_explorerRepositoryMock.Object);
            _serverMock.SetupGet(it => it.Permissions)
                .Returns(new List<IWindowsGroupPermission> { _windowsGroupPermissionsMock.Object });
            _popupControllerMock = new Mock<IPopupController>();
            _target = new ExplorerItemViewModel(_serverMock.Object, _explorerTreeItemMock.Object,
                a => { }, _shellViewModelMock.Object, _popupControllerMock.Object);
        }

        #endregion Test initialize

        #region Test commands

        [TestMethod]
        public void TestRollbackCommand()
        {
            _target.ResourceId = Guid.NewGuid();
            _target.VersionNumber = Guid.NewGuid().ToString();
            var outputDisplayName = Guid.NewGuid().ToString();
            var rollbackResultMock = new Mock<IRollbackResult>();
            _explorerRepositoryMock.Setup(it => it.Rollback(_target.ResourceId, _target.VersionNumber)).Returns(rollbackResultMock.Object);
            rollbackResultMock.SetupGet(it => it.DisplayName).Returns(outputDisplayName);

            _popupControllerMock.Setup(it => it.ShowRollbackVersionMessage(It.IsAny<string>())).Returns(MessageBoxResult.Yes);

            //act
            _target.RollbackCommand.Execute(null);
            Assert.IsTrue(_target.RollbackCommand.CanExecute(null));

            //assert
            _explorerTreeItemMock.VerifySet(it => it.AreVersionsVisible = true);
            _explorerTreeItemMock.VerifySet(it => it.ResourceName = outputDisplayName);
        }

        [TestMethod]
        public void TestDeployCommand()
        {
            //arrange
            //act
            _target.DeployCommand.Execute(null);
            Assert.IsTrue(_target.DeployCommand.CanExecute(null));

            //assert
            _shellViewModelMock.Verify(
                it =>
                    it.AddDeploySurface(
                        It.Is<IEnumerable<IExplorerTreeItem>>(
                            x => x.All(xitem => (_target.AsList().Union(new[] { _target })).Contains(xitem)))));
        }

        [TestMethod]
        public void TestLostFocusCommand()
        {
            //arrange
            _target.IsRenaming = true;

            //act
            _target.LostFocus.Execute(null);
            Assert.IsTrue(_target.LostFocus.CanExecute(null));

            //assert
            Assert.IsFalse(_target.IsRenaming);
        }

        [TestMethod]
        public void TestNewServerCommand()
        {
            //arrange
            _target.ResourceType = "ServerSource";
            _target.ResourceId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(Guid.NewGuid());

            //act
            _target.NewServerCommand.Execute(null);
            Assert.IsTrue(_target.NewServerCommand.CanExecute(null));

            //assert
            _shellViewModelMock.Verify(it => it.SetActiveEnvironment(_target.Server.EnvironmentID));
            _shellViewModelMock.Verify(it => it.SetActiveServer(_target.Server));
            _shellViewModelMock.Verify(it => it.NewServerSource(_target.ResourcePath));
        }

        [TestMethod]
        public void TestNewSqlServerSourceCommand()
        {
            //arrange
            _target.ResourceType = "DbSource";
            _target.ResourceId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(Guid.NewGuid());

            //act
            _target.NewSqlServerSourceCommand.Execute(null);
            Assert.IsTrue(_target.NewSqlServerSourceCommand.CanExecute(null));

            //assert
            _shellViewModelMock.Verify(it => it.SetActiveEnvironment(_target.Server.EnvironmentID));
            _shellViewModelMock.Verify(it => it.SetActiveServer(_target.Server));
            _shellViewModelMock.Verify(it => it.NewSqlServerSource(_target.ResourcePath));
        }

        [TestMethod]
        public void TestNewMySqlSourceCommand()
        {
            //arrange
            _target.ResourceType = "DbSource";
            _target.ResourceId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(Guid.NewGuid());

            //act
            _target.NewMySqlSourceCommand.Execute(null);
            Assert.IsTrue(_target.NewMySqlSourceCommand.CanExecute(null));

            //assert
            _shellViewModelMock.Verify(it => it.SetActiveEnvironment(_target.Server.EnvironmentID));
            _shellViewModelMock.Verify(it => it.SetActiveServer(_target.Server));
            _shellViewModelMock.Verify(it => it.NewMySqlSource(_target.ResourcePath));
        }

        [TestMethod]
        public void TestNewPostgreSqlSourceCommand()
        {
            //arrange
            _target.ResourceType = "DbSource";
            _target.ResourceId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(Guid.NewGuid());

            //act
            _target.NewPostgreSqlSourceCommand.Execute(null);
            Assert.IsTrue(_target.NewPostgreSqlSourceCommand.CanExecute(null));

            //assert
            _shellViewModelMock.Verify(it => it.SetActiveEnvironment(_target.Server.EnvironmentID));
            _shellViewModelMock.Verify(it => it.SetActiveServer(_target.Server));
            _shellViewModelMock.Verify(it => it.NewPostgreSqlSource(_target.ResourcePath));
        }

        [TestMethod]
        public void TestNewOracleSourceCommand()
        {
            //arrange
            _target.ResourceType = "DbSource";
            _target.ResourceId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(Guid.NewGuid());

            //act
            _target.NewOracleSourceCommand.Execute(null);
            Assert.IsTrue(_target.NewOracleSourceCommand.CanExecute(null));

            //assert
            _shellViewModelMock.Verify(it => it.SetActiveEnvironment(_target.Server.EnvironmentID));
            _shellViewModelMock.Verify(it => it.SetActiveServer(_target.Server));
            _shellViewModelMock.Verify(it => it.NewOracleSource(_target.ResourcePath));
        }

        [TestMethod]
        public void TestNewOdbcSourceCommand()
        {
            //arrange
            _target.ResourceType = "DbSource";
            _target.ResourceId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(Guid.NewGuid());

            //act
            _target.NewOdbcSourceCommand.Execute(null);
            Assert.IsTrue(_target.NewOdbcSourceCommand.CanExecute(null));

            //assert
            _shellViewModelMock.Verify(it => it.SetActiveEnvironment(_target.Server.EnvironmentID));
            _shellViewModelMock.Verify(it => it.SetActiveServer(_target.Server));
            _shellViewModelMock.Verify(it => it.NewOdbcSource(_target.ResourcePath));
        }

        [TestMethod]
        public void TestNewPluginSourceCommand()
        {
            //arrange
            _target.ResourceType = "PluginSource";
            _target.ResourceId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(Guid.NewGuid());

            //act
            _target.NewPluginSourceCommand.Execute(null);
            Assert.IsTrue(_target.NewServerCommand.CanExecute(null));

            //assert
            _shellViewModelMock.Verify(it => it.SetActiveEnvironment(_target.Server.EnvironmentID));
            _shellViewModelMock.Verify(it => it.SetActiveServer(_target.Server));
            _shellViewModelMock.Verify(it => it.NewPluginSource(_target.ResourcePath));
        }

        [TestMethod]
        public void TestNewWebSourceCommand()
        {
            //arrange
            _target.ResourceType = "WebSource";
            _target.ResourceId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(Guid.NewGuid());

            //act
            _target.NewWebSourceSourceCommand.Execute(null);
            Assert.IsTrue(_target.NewServerCommand.CanExecute(null));

            //assert
            _shellViewModelMock.Verify(it => it.SetActiveEnvironment(_target.Server.EnvironmentID));
            _shellViewModelMock.Verify(it => it.SetActiveServer(_target.Server));
            _shellViewModelMock.Verify(it => it.NewWebSource(_target.ResourcePath));
        }

        [TestMethod]
        public void TestNewEmailSourceCommand()
        {
            //arrange
            _target.ResourceType = "EmailSource";
            _target.ResourceId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(Guid.NewGuid());

            //act
            _target.NewEmailSourceSourceCommand.Execute(null);
            Assert.IsTrue(_target.NewServerCommand.CanExecute(null));

            //assert
            _shellViewModelMock.Verify(it => it.SetActiveEnvironment(_target.Server.EnvironmentID));
            _shellViewModelMock.Verify(it => it.SetActiveServer(_target.Server));
            _shellViewModelMock.Verify(it => it.NewEmailSource(_target.ResourcePath));
        }

        [TestMethod]
        public void TestNewExchangeSourceCommand()
        {
            //arrange
            _target.ResourceType = "ExchangeSource";
            _target.ResourceId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(Guid.NewGuid());

            //act
            _target.NewExchangeSourceSourceCommand.Execute(null);
            Assert.IsTrue(_target.NewServerCommand.CanExecute(null));

            //assert
            _shellViewModelMock.Verify(it => it.SetActiveEnvironment(_target.Server.EnvironmentID));
            _shellViewModelMock.Verify(it => it.SetActiveServer(_target.Server));
            _shellViewModelMock.Verify(it => it.NewExchangeSource(_target.ResourcePath));
        }

        [TestMethod]
        public void TestNewRabbitMqSourceCommand()
        {
            //arrange
            _target.ResourceType = "RabbitMqSource";
            _target.ResourceId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(Guid.NewGuid());

            //act
            _target.NewRabbitMQSourceSourceCommand.Execute(null);
            Assert.IsTrue(_target.NewServerCommand.CanExecute(null));

            //assert
            _shellViewModelMock.Verify(it => it.SetActiveEnvironment(_target.Server.EnvironmentID));
            _shellViewModelMock.Verify(it => it.SetActiveServer(_target.Server));
            _shellViewModelMock.Verify(it => it.NewRabbitMQSource(_target.ResourcePath));
        }

        [TestMethod]
        public void TestCreateNewTestCommand()
        {
            //arrange
            _target.ResourceType = "WorkflowService";
            _target.ResourceId = Guid.NewGuid();
            _target.IsService = true;
            _target.CanCreateTest = true;
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(Guid.NewGuid());

            //act
            _target.CreateTestCommand.Execute(null);
            Assert.IsTrue(_target.CreateTestCommand.CanExecute(null));

            //assert
            _shellViewModelMock.Verify(it => it.CreateTest(_target.ResourceId));
        }

        [TestMethod]
        public void TestNewSharepointSourceCommand()
        {
            //arrange
            _target.ResourceType = "SharepointSource";
            _target.ResourceId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(Guid.NewGuid());

            //act
            _target.NewSharepointSourceSourceCommand.Execute(null);
            Assert.IsTrue(_target.NewServerCommand.CanExecute(null));

            //assert
            _shellViewModelMock.Verify(it => it.SetActiveEnvironment(_target.Server.EnvironmentID));
            _shellViewModelMock.Verify(it => it.SetActiveServer(_target.Server));
            _shellViewModelMock.Verify(it => it.NewSharepointSource(_target.ResourcePath));
        }

        [TestMethod]
        public void TestNewDropboxSourceCommand()
        {
            //arrange
            _target.ResourceType = "DropboxSource";
            _target.ResourceId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(Guid.NewGuid());

            //act
            _target.NewDropboxSourceSourceCommand.Execute(null);
            Assert.IsTrue(_target.NewServerCommand.CanExecute(null));

            //assert
            _shellViewModelMock.Verify(it => it.SetActiveEnvironment(_target.Server.EnvironmentID));
            _shellViewModelMock.Verify(it => it.SetActiveServer(_target.Server));
            _shellViewModelMock.Verify(it => it.NewDropboxSource(_target.ResourcePath));
        }

        [TestMethod]
        public void TestNewServiceCommand()
        {
            //arrange
            _target.ResourceType = "WorkflowService";
            _target.ResourceId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(Guid.NewGuid());

            //act
            _target.NewServiceCommand.Execute(null);
            Assert.IsTrue(_target.NewServerCommand.CanExecute(null));

            //assert
            _shellViewModelMock.Verify(it => it.SetActiveEnvironment(_target.Server.EnvironmentID));
            _shellViewModelMock.Verify(it => it.SetActiveServer(_target.Server));
            _shellViewModelMock.Verify(it => it.NewService(_target.ResourcePath));
        }

        [TestMethod]
        public void TestContextMenuDebugCommand()
        {
            //arrange
            _target.ResourceId = Guid.NewGuid();

            //act
            _target.DebugCommand.Execute(null);
            Assert.IsTrue(_target.DebugCommand.CanExecute(null));

            //assert
            _shellViewModelMock.Verify(it => it.OpenResource(_target.ResourceId,_target.Server.EnvironmentID, _target.Server));
            _shellViewModelMock.Verify(it => it.Debug());
        }

        [TestMethod]
        public void TestDebugStudioCommand()
        {
            //arrange
            _target.ResourceType = "WorkflowService";
            _target.ResourceId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(Guid.NewGuid());

            //act
            _target.DebugStudioCommand.Execute(null);
            Assert.IsTrue(_target.DebugStudioCommand.CanExecute(null));

            //assert
            _shellViewModelMock.Verify(it => it.StudioDebug(_target.ResourceId, _target.Server));
        }

        [TestMethod]
        public void DebugBrowserCommand()
        {
            //arrange
            _target.ResourceType = "WorkflowService";
            _target.ResourceId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(Guid.NewGuid());

            //act
            _target.DebugBrowserCommand.Execute(null);
            Assert.IsTrue(_target.DebugBrowserCommand.CanExecute(null));

            //assert
            _shellViewModelMock.Verify(it => it.BrowserDebug(_target.ResourceId, _target.Server));
        }

        [TestMethod]
        public void ScheduleCommand()
        {
            //arrange
            _target.ResourceType = "WorkflowService";
            _target.ResourceId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(Guid.NewGuid());

            //act
            _target.ScheduleCommand.Execute(null);
            Assert.IsTrue(_target.ScheduleCommand.CanExecute(null));

            //assert
            _shellViewModelMock.Verify(it => it.NewSchedule(_target.ResourceId));
        }

        [TestMethod]
        public void RunAllTestsCommand()
        {
            //arrange
            _target.ResourceType = "WorkflowService";
            _target.ResourceId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(Guid.NewGuid());

            //act
            _target.RunAllTestsCommand.Execute(null);
            Assert.IsTrue(_target.RunAllTestsCommand.CanExecute(null));

            //assert
            _shellViewModelMock.Verify(it => it.RunAllTests(_target.ResourceId));
        }

        [TestMethod]
        public void TestViewSwaggerCommand()
        {
            //arrange
            _target.ResourceType = "WorkflowService";
            _target.ResourceId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(Guid.NewGuid());

            //act
            _target.ViewSwaggerCommand.Execute(null);
            Assert.IsTrue(_target.NewServerCommand.CanExecute(null));

            //assert
            
            _shellViewModelMock.Verify(it => it.ViewSwagger(_target.ResourceId, _target.Server));
        }

        [TestMethod]
        public void TestApisJsonCommand()
        {
            //arrange
            _target.ResourceType = "Folder";
            _target.ResourceId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(Guid.NewGuid());
            var connection = new Mock<IEnvironmentConnection>();
            connection.SetupGet(it => it.ID).Returns(Guid.NewGuid());
            //act
            var mock = new Mock<IEnvironmentModel>();
            mock.SetupGet(it => it.Connection).Returns(connection.Object);
            mock.SetupGet(it => it.Connection.WebServerUri).Returns(new Uri("http://localhost:3142"));
            _target.EnvironmentModel = mock.Object;
            _target.ViewApisJsonCommand.Execute(null);
            Assert.IsTrue(_target.NewServerCommand.CanExecute(null));

            //assert

            _shellViewModelMock.Verify(it => it.ViewApisJson(_target.ResourcePath, new Uri("http://localhost:3142")));
        }

        [TestMethod]
        public void TestOpenCommand()
        {
            //arrange
            _target.ResourceType = "DbSource";
            _target.ResourceId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(Guid.NewGuid());

            //act
            _target.OpenCommand.Execute(null);
            Assert.IsTrue(_target.OpenCommand.CanExecute(null));

            //assert
            _shellViewModelMock.Verify(it => it.SetActiveEnvironment(_target.Server.EnvironmentID));
            _shellViewModelMock.Verify(it => it.SetActiveServer(_target.Server));
            _shellViewModelMock.Verify(it => it.OpenResource(_target.ResourceId,_target.Server.EnvironmentID, _target.Server));
        }

        [TestMethod]
        public void TestDebugCommand()
        {
            //arrange
            _target.ResourceId = Guid.NewGuid();

            //act
            _target.DebugCommand.Execute(null);
            Assert.IsTrue(_target.DebugCommand.CanExecute(null));

            //assert
            _shellViewModelMock.Verify(it => it.OpenResource(_target.ResourceId,_target.Server.EnvironmentID, _target.Server));
            _shellViewModelMock.Verify(it => it.Debug());
        }

        [TestMethod]
        public void TestRenameCommand()
        {
            //arrange
            _target.IsRenaming = false;

            //act
            _target.RenameCommand.Execute(null);
            Assert.IsTrue(_target.RenameCommand.CanExecute(null));

            //assert
            Assert.IsTrue(_target.IsRenaming);
        }
        
        [TestMethod]
        public void TestShowDependenciesCommand()
        {
            //arrange
            _target.ResourceId = Guid.NewGuid();

            //act
            _target.ShowDependenciesCommand.Execute(null);
            Assert.IsTrue(_target.ShowDependenciesCommand.CanExecute(null));

            //assert
            _shellViewModelMock.Verify(it => it.ShowDependencies(_target.ResourceId, _target.Server,_target.IsSource));
        }

        [TestMethod]
        public void TestShowVersionHistory()
        {
            //arrange
            _target.AreVersionsVisible = false;
            var explorerRepositoryMock = new Mock<IExplorerRepository>();
            explorerRepositoryMock.Setup(it => it.GetVersions(It.IsAny<Guid>())).Returns(Enumerable.Empty<IVersionInfo>().ToList());
            _serverMock.SetupGet(it => it.ExplorerRepository).Returns(explorerRepositoryMock.Object);
            _explorerRepositoryMock.Setup(it => it.GetVersions(It.IsAny<Guid>())).Returns(new List<IVersionInfo>());

            //act
            _target.ShowVersionHistory.Execute(null);
            Assert.IsTrue(_target.ShowVersionHistory.CanExecute(null));

            //assert
            Assert.IsTrue(_target.AreVersionsVisible);
        }

        [TestMethod]
        public void TestDeleteCommandResourceTypeVersion()
        {
            //arrange
            _target.ResourceType = "Version";
            _target.IsResourceVersion = true;
            _target.ResourceName = Guid.NewGuid().ToString();
            _target.EnvironmentModel = new Mock<IEnvironmentModel>().Object;
            _explorerTreeItemMock.SetupGet(it => it.Children)
                .Returns(new ObservableCollection<IExplorerItemViewModel> { _target });

            _popupControllerMock.Setup(it => it.ShowDeleteVersionMessage(It.IsAny<string>())).Returns(MessageBoxResult.Yes);

            //act
            _target.DeleteCommand.Execute(null);
            Assert.IsTrue(_target.DeleteCommand.CanExecute(null));

            //assert
            _explorerRepositoryMock.Verify(it => it.Delete(_target));
            _explorerTreeItemMock.Verify(it => it.RemoveChild(_target));
        }

        [TestMethod]
        public void TestDeleteCommandResourceTypeVersionUserDeclined()
        {
            //arrange
            // var explorerRepositoryMock = new Mock<IExplorerRepository>();
            _target.ResourceType = "Version";
            _target.IsResourceVersion = true;
            //if (_popupController.ShowDeleteVersionMessage(ResourceName) == MessageBoxResult.Yes)
            //serverMock.SetupGet(it => it.ExplorerRepository).Returns(explorerRepositoryMock.Object);
            _popupControllerMock.Setup(it => it.ShowDeleteVersionMessage(It.IsAny<string>())).Returns(MessageBoxResult.No);

            //act
            _target.DeleteCommand.Execute(null);
            Assert.IsTrue(_target.DeleteCommand.CanExecute(null));

            //assert
            _explorerRepositoryMock.Verify(it => it.Delete(It.IsAny<IExplorerItemViewModel>()), Times.Never);
            _explorerTreeItemMock.Verify(it => it.RemoveChild(_target), Times.Never);
            //assert
            //DeleteVersion();
        }


        [TestMethod]
        public void TestDeleteCommandResourceTypeServerSourceDeleteSuccess()
        {
            //arrange
            var environmentModelMock = new Mock<IEnvironmentModel>();
            environmentModelMock.SetupGet(it => it.ID).Returns(Guid.NewGuid());
            _explorerRepositoryMock.Setup(it => it.Delete(_target)).Returns(new DeletedFileMetadata { IsDeleted = true });
            _target.EnvironmentModel = environmentModelMock.Object;
            _target.ResourceType = "ServerSource";
            _target.ResourceId = Guid.NewGuid();
            //if (_popupController.ShowDeleteVersionMessage(ResourceName) == MessageBoxResult.Yes)
            _popupControllerMock.Setup(it => it.Show(It.IsAny<IPopupMessage>())).Returns(MessageBoxResult.Yes);
            var studioManagerUpdateMock = new Mock<IStudioUpdateManager>();
            _serverMock.SetupGet(it => it.UpdateRepository).Returns(studioManagerUpdateMock.Object);

            //act
            _target.DeleteCommand.Execute(null);
            Assert.IsTrue(_target.DeleteCommand.CanExecute(null));

            //assert
            _shellViewModelMock.Verify(it => it.CloseResource(_target.ResourceId, environmentModelMock.Object.ID));
            _explorerRepositoryMock.Verify(it => it.Delete(_target));
            _explorerTreeItemMock.Verify(it => it.RemoveChild(_target));
            studioManagerUpdateMock.Verify(it => it.FireServerSaved(It.IsAny<Guid>()));
        }

        [TestMethod]
        public void TestDeleteCommandResourceTypeServerDeleteSuccess()
        {
            //arrange
            var environmentModelMock = new Mock<IEnvironmentModel>();
            environmentModelMock.SetupGet(it => it.ID).Returns(Guid.NewGuid());
            _explorerRepositoryMock.Setup(it => it.Delete(_target)).Returns(new DeletedFileMetadata { IsDeleted = true });
            _target.EnvironmentModel = environmentModelMock.Object;
            _target.ResourceType = "Server";
            _target.IsServer = true;
            _target.ResourceId = Guid.NewGuid();
            _popupControllerMock.Setup(it => it.Show(It.IsAny<IPopupMessage>())).Returns(MessageBoxResult.Yes);
            var studioManagerUpdateMock = new Mock<IStudioUpdateManager>();
            _serverMock.SetupGet(it => it.UpdateRepository).Returns(studioManagerUpdateMock.Object);

            //act
            _target.DeleteCommand.Execute(null);
            Assert.IsTrue(_target.DeleteCommand.CanExecute(null));

            //assert
            _shellViewModelMock.Verify(it => it.CloseResource(_target.ResourceId, environmentModelMock.Object.ID));
            _explorerRepositoryMock.Verify(it => it.Delete(_target));
            _explorerTreeItemMock.Verify(it => it.RemoveChild(_target));
            studioManagerUpdateMock.Verify(it => it.FireServerSaved(It.IsAny<Guid>()));
        }

        [TestMethod]
        public void TestDeleteCommandResourceTypeServerDeleteSuccess_ShowDependencies()
        {
            //arrange
            _shellViewModelMock.Setup(model => model.ShowDependencies(It.IsAny<Guid>(), It.IsAny<IServer>(), It.IsAny<bool>()));
            var environmentModelMock = new Mock<IEnvironmentModel>();
            environmentModelMock.SetupGet(it => it.ID).Returns(Guid.NewGuid());
            _explorerRepositoryMock.Setup(it => it.Delete(_target)).Returns(new DeletedFileMetadata { IsDeleted = false,ShowDependencies = true});
            _target.EnvironmentModel = environmentModelMock.Object;
            _target.ResourceType = "Server";
            _target.IsServer = true;
            _target.ResourceId = Guid.NewGuid();
            _popupControllerMock.Setup(it => it.Show(It.IsAny<IPopupMessage>())).Returns(MessageBoxResult.Yes);
            var studioManagerUpdateMock = new Mock<IStudioUpdateManager>();
            _serverMock.SetupGet(it => it.UpdateRepository).Returns(studioManagerUpdateMock.Object);

            //act
            _target.DeleteCommand.Execute(null);
            Assert.IsTrue(_target.DeleteCommand.CanExecute(null));

            //assert
            _explorerRepositoryMock.Verify(it => it.Delete(_target),Times.Once);
            _explorerTreeItemMock.Verify(it => it.RemoveChild(_target), Times.Never);
        }


        [TestMethod]
        public void TestOpenVersionCommandResourceTypeVersion()
        {
            //arrange
            _target.ResourceType = "Version";
            _target.IsResourceVersion = true;
            var versionInfoMock = new Mock<IVersionInfo>();
            _target.VersionInfo = versionInfoMock.Object;
            _explorerTreeItemMock.Setup(it => it.ResourceId).Returns(Guid.NewGuid());

            //act
            _target.OpenVersionCommand.Execute(null);
            Assert.IsTrue(_target.OpenVersionCommand.CanExecute(null));

            //assert
            _shellViewModelMock.Verify(it => it.OpenVersion(_target.Parent.ResourceId, _target.VersionInfo));
        }


        [TestMethod]
        public void TestExpandCommandResourceTypeFolderSingleClick()
        {
            //arrange
            _target.IsExpanded = false;
            _target.ResourceType = "Folder";

            //act
            _target.Expand.Execute(1);
            Assert.IsTrue(_target.Expand.CanExecute(1));

            //assert
            Assert.IsFalse(_target.IsExpanded);
        }

        [TestMethod]
        public void TestExpandCommandResourceTypeFolderDoubleClick()
        {
            //arrange
            _target.IsExpanded = false;
            _target.ResourceType = "Folder";
            _target.IsFolder = true;
            //act
            _target.Expand.Execute(2);
            Assert.IsTrue(_target.Expand.CanExecute(2));

            //assert
            Assert.IsTrue(_target.IsExpanded);
        }

        [TestMethod]
        public void TestExpandCommandResourceTypeWorkflowServiceSingleClick()
        {
            //arrange
            _target.IsExpanded = false;
            _target.ResourceType = "WorkflowService";

            //act
            _target.Expand.Execute(1);
            Assert.IsTrue(_target.Expand.CanExecute(1));

            //assert
            Assert.IsFalse(_target.IsExpanded);
        }

        [TestMethod]
        public void TestExpandCommandResourceTypeWorkflowServiceDoubleClick()
        {
            //arrange
            _target.IsExpanded = true;
            _target.ResourceType = "WorkflowService";

            //act
            _target.Expand.Execute(2);
            Assert.IsTrue(_target.Expand.CanExecute(2));

            //assert
            Assert.IsFalse(_target.IsExpanded);
        }

        [TestMethod]
        public void TestCreateFolderCommandResourceTypeFolder()
        {
            _serverMock.Setup(server => server.UserPermissions)
                       .Returns(Permissions.Administrator);

            _serverMock.Setup(server => server.GetPermissions(It.IsAny<Guid>())).Returns(Permissions.Administrator);

            //arrange
            _target.IsExpanded = false;
            _target.ResourceType = "Folder";
            _target.IsFolder = true;
            _target.Children.Clear();
            _target.AllowResourceCheck = true;
            _target.IsResourceChecked = true;
            _target.IsResourceUnchecked = false;
            _target.IsFolderChecked = true;
            _target.CanCreateFolder = true;
            _target.CanCreateSource = true;
            _target.CanShowVersions = true;
            _target.CanRename = true;
            _target.CanDuplicate = true;
            _target.CanCreateTest = true;
            _target.CanDeploy = true;
            _target.CanShowDependencies = true;
            _target.ResourcePath = Guid.NewGuid().ToString();
            _target.CanCreateWorkflowService = true;
            _target.ShowContextMenu = true;

            //act
            _target.CreateFolderCommand.Execute(null);
            Assert.IsTrue(_target.CreateFolderCommand.CanExecute(null));

            //assert
            Assert.IsTrue(_target.IsExpanded);
            //_explorerRepositoryMock.Verify(it => it.CreateFolder(_target.ResourcePath, "New Folder", It.IsAny<Guid>()));
            var createdFolder = _target.Children.Single();
            Assert.AreEqual("New Folder", createdFolder.ResourceName);
            Assert.AreEqual("Folder", createdFolder.ResourceType);
            Assert.AreEqual(_target.AllowResourceCheck, createdFolder.AllowResourceCheck);
            Assert.AreEqual(_target.IsResourceChecked, createdFolder.IsResourceChecked);
            Assert.AreEqual(_target.IsResourceUnchecked, createdFolder.IsResourceUnchecked);            
            Assert.AreEqual(_target.IsFolderChecked, createdFolder.IsFolderChecked);
            Assert.AreEqual(_target.CanCreateFolder, createdFolder.CanCreateFolder);
            Assert.AreEqual(_target.CanCreateSource, createdFolder.CanCreateSource);
            Assert.AreEqual(_target.CanShowVersions, createdFolder.CanShowVersions);
            Assert.AreEqual(_target.CanRename, createdFolder.CanRename);
            Assert.AreEqual(_target.CanDuplicate, createdFolder.CanDuplicate);
            Assert.AreEqual(_target.CanCreateTest, createdFolder.CanCreateTest);
            Assert.AreEqual(_target.CanRollback, createdFolder.CanRollback);
            Assert.AreEqual(_target.CanDeploy, createdFolder.CanDeploy);
            Assert.AreEqual(_target.CanShowDependencies, createdFolder.CanShowDependencies);
            Assert.AreEqual(_target.ResourcePath + "\\" + createdFolder.ResourceName, createdFolder.ResourcePath);
            Assert.AreEqual(_target.CanCreateWorkflowService, createdFolder.CanCreateWorkflowService);
            Assert.AreEqual(_target.ShowContextMenu, createdFolder.ShowContextMenu);
            Assert.IsTrue(createdFolder.IsRenaming);
        }

        [TestMethod]
        public void TestCanShowServerVersion()
        {
            //arrange
            _target.CanShowServerVersion = false;

            //act
            _target.CanShowServerVersion = !_target.CanShowServerVersion;

            //assert
            Assert.IsTrue(_target.CanShowServerVersion);
        }

        [TestMethod]
        public void TestCreateFolderCommandResourceTypeDbService()
        {
            //arrange
            _target.IsExpanded = false;
            _target.ResourceType = "DbService";
            _target.Children.Clear();

            //act
            _target.CreateFolderCommand.Execute(null);
            Assert.IsTrue(_target.CreateFolderCommand.CanExecute(null));

            //assert
            Assert.IsFalse(_target.IsExpanded);
            Assert.IsFalse(_target.Children.Any());
        }

        [TestMethod]
        public void TestDeleteVersionCommand()
        {
            //arrange
            _target.ResourceType = "Version";
            _target.ResourceName = Guid.NewGuid().ToString();
            _explorerTreeItemMock.SetupGet(it => it.Children)
                .Returns(new ObservableCollection<IExplorerItemViewModel>() { _target });
            //if (_popupController.ShowDeleteVersionMessage(ResourceName) == MessageBoxResult.Yes)

            _popupControllerMock.Setup(it => it.ShowDeleteVersionMessage(It.IsAny<string>())).Returns(MessageBoxResult.Yes);

            //act
            _target.DeleteVersionCommand.Execute(null);
            Assert.IsTrue(_target.DeleteVersionCommand.CanExecute(null));

            //assert
            _explorerRepositoryMock.Verify(it => it.Delete(_target));
            _explorerTreeItemMock.Verify(it => it.RemoveChild(_target));
        }

        [TestMethod]
        public void TestDeleteFolderCommandExpectException()
        {
            //arrange
            var childMock = new Mock<IExplorerItemViewModel>();
            childMock.SetupGet(it => it.ResourceType).Returns("WorkflowService");
            childMock.SetupGet(it => it.ResourceName).Returns("Message");
            childMock.SetupGet(it => it.IsVisible).Returns(true);
            _target.EnvironmentModel = new Mock<IEnvironmentModel>().Object;
            _target.ResourceName = "someResource";
            _target.ResourceType = "Folder";
            _target.IsFolder = true;
            _target.AddChild(childMock.Object);

            _explorerTreeItemMock.SetupGet(it => it.Children)
                .Returns(new ObservableCollection<IExplorerItemViewModel>() { _target });

            _popupControllerMock.Setup(it => it.Show(It.IsAny<IPopupMessage>())).Returns(MessageBoxResult.Yes);

            //act
            Assert.IsTrue(_target.DeleteCommand.CanExecute(null));
            _target.DeleteCommand.Execute(null);

            //assert
            _explorerRepositoryMock.Verify(it => it.Delete(_target));
            _explorerTreeItemMock.Verify(it => it.RemoveChild(_target),Times.Never());

            Assert.AreEqual(1, _target.ChildrenCount);
            _explorerTreeItemMock.Verify(it => it.AddChild(_target),Times.Never);
        }

        #endregion Test commands

        #region Test equality

        [TestMethod]
        public void TestEquals()
        {
            var otherSameId = new ExplorerItemViewModel(_serverMock.Object, _explorerTreeItemMock.Object, a => { },
                _shellViewModelMock.Object, _popupControllerMock.Object)
            { ResourceId = Guid.NewGuid() };
            var otherDifferentId = new ExplorerItemViewModel(_serverMock.Object, _explorerTreeItemMock.Object, a => { },
                _shellViewModelMock.Object, _popupControllerMock.Object)
            { ResourceId = Guid.NewGuid() };
            var otherDifferentType = new object();
            _target.ResourceId = otherSameId.ResourceId;
            _target.ResourceName = "Some res name";

            Assert.IsFalse(_target.Equals(null));
            Assert.IsTrue(_target.Equals(_target));
            Assert.IsTrue(_target.Equals(otherSameId));
            Assert.IsFalse(_target.Equals(otherDifferentId));

            Assert.IsFalse(_target.Equals((object)null));
            Assert.IsTrue(_target.Equals((object)_target));
            Assert.IsTrue(_target.Equals((object)otherSameId));
            Assert.IsFalse(_target.Equals((object)otherDifferentId));
            Assert.IsFalse(_target.Equals(otherDifferentType));

            Assert.IsFalse(_target == null);
            // ReSharper disable once EqualExpressionComparison
            Assert.IsTrue(Equals(_target, _target));
            Assert.IsFalse(_target == otherSameId);
            Assert.IsFalse(_target == otherDifferentId);
            Assert.IsFalse(ReferenceEquals(_target, otherDifferentType));

            Assert.IsTrue(_target != null);
            // ReSharper disable once EqualExpressionComparison
            Assert.IsFalse(!Equals(_target, _target));
            Assert.IsTrue(_target != otherSameId);
            Assert.IsTrue(_target != otherDifferentId);
#pragma warning disable 252,253
            Assert.IsTrue(_target != otherDifferentType);
#pragma warning restore 252,253
        }

        [TestMethod]
        public void TestHashcode()
        {
            _target.ResourceId = Guid.NewGuid();
            _target.ResourceName = "Some res name";

            Assert.AreEqual(_target.GetHashCode(), _target.ResourceId.GetHashCode());
        }

        #endregion Test equality

        #region Test properties

        [TestMethod]
        public void TestExecuteToolTipGet()
        {
            //assert
            Assert.IsNotNull(_target.ExecuteToolTip);
        }

        [TestMethod]
        public void TestEditToolTipGet()
        {
            //assert
            Assert.IsNotNull(_target.EditToolTip);
        }

        [TestMethod]
        public void TestCanDropGetFolder()
        {
            //arrange
            _target.ResourceType = "Folder";
            _target.CanDrop = true;
            _target.IsFolder = true;
            //act
            var actual = _target.CanDrop;
            //assert
            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void TestCanDropGetOther()
        {
            //arrange
            _target.ResourceType = "Server";
            _target.CanDrop = true;
            //act
            var actual = _target.CanDrop;
            //assert
            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void TestCanDropSet()
        {
            //arrange
            var isCanDropChanged = false;
            _target.PropertyChanged += (sender, e) =>
            {
                isCanDropChanged = isCanDropChanged || e.PropertyName == "CanDrop";
            };

            //act
            _target.CanDrop = true;

            //assert
            Assert.IsTrue(isCanDropChanged);
        }

        [TestMethod]
        public void TestCanDragGetFolder()
        {
            //arrange
            _target.ResourceType = "Folder";
            _target.CanDrag = true;
            _target.IsFolder = true;
            //act
            var actual = _target.CanDrag;
            //assert
            Assert.IsTrue(actual);
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
            var canCreateNewRabbitMqSourceSourceCommand = _target.NewRabbitMQSourceSourceCommand.CanExecute(null);
            var canViewSwaggerCommand = _target.ViewSwaggerCommand.CanExecute(null);
            var canViewApisJsonCommand = _target.ViewApisJsonCommand.CanExecute(null);
            
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
            Assert.IsTrue(canCreateNewRabbitMqSourceSourceCommand);
            Assert.IsTrue(canViewSwaggerCommand);
            Assert.IsTrue(canViewApisJsonCommand);
        }

        [TestMethod]
        public void TestCanDragGetOther()
        {
            //arrange
            _target.ResourceType = "Server";
            _target.CanDrag = true;
            //act
            var actual = _target.CanDrag;
            //assert
            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void TestCanDragSet()
        {
            //arrange
            var isCanDragChanged = false;
            _target.PropertyChanged += (sender, e) =>
            {
                isCanDragChanged = isCanDragChanged || e.PropertyName == "CanDrag";
            };

            //act
            _target.CanDrag = true;

            //assert
            Assert.IsTrue(isCanDragChanged);
        }

        [TestMethod]
        public void TestIsExpanderVisibleNoChildren()
        {
            //arrange
            _target.Children.Clear();
            //act
            var isExpanderVisible = _target.IsExpanderVisible;
            //assert
            Assert.IsFalse(isExpanderVisible);
        }

        [TestMethod]
        public void TestIsExpanderVisibleChildrenAreVersionsVisible()
        {
            //arrange
            _target.Children.Clear();
            var childMock = new Mock<IExplorerItemViewModel>();
            _target.Children.Add(childMock.Object);
            _explorerRepositoryMock.Setup(it => it.GetVersions(It.IsAny<Guid>()))
                .Returns(new List<IVersionInfo>());
            _target.AreVersionsVisible = true;
            //act
            var isExpanderVisible = _target.IsExpanderVisible;
            //assert
            Assert.IsFalse(isExpanderVisible);
        }

        [TestMethod]
        public void TestIsExpanderVisibleChildrenAreVersionsInvisible()
        {
            //arrange
            _target.Children.Clear();
            var childMock = new Mock<IExplorerItemViewModel>();
            childMock.Setup(model => model.IsVisible).Returns(true);
            _target.AreVersionsVisible = false;
            _target.AddChild(childMock.Object);
            //act
            var isExpanderVisible = _target.IsExpanderVisible;
            //assert
            Assert.IsTrue(isExpanderVisible);
        }

        [TestMethod]
        public void TestAreVersionsVisibleTrue()
        {
            //arrange
            var versionMock = new Mock<IVersionInfo>();
            var isAreVersionsVisibleChanged = false;
            var isChildrenChanged = false;
            var collection = new List<IVersionInfo>()
            {
                versionMock.Object
            };
            versionMock.SetupGet(it => it.VersionNumber).Returns("someVerNum");
            versionMock.SetupGet(it => it.DateTimeStamp).Returns(new DateTime(2013, 2, 2));
            versionMock.SetupGet(it => it.Reason).Returns("gfedew.xml");
            _target.ResourceId = Guid.NewGuid();
            _target.PropertyChanged += (sender, e) =>
            {
                isAreVersionsVisibleChanged = isAreVersionsVisibleChanged || e.PropertyName == "AreVersionsVisible";
                isChildrenChanged = isChildrenChanged || e.PropertyName == "Children";
            };
            _target.CanDelete = true;
            _explorerRepositoryMock.Setup(it => it.GetVersions(It.IsAny<Guid>())).Returns(collection);
            //act
            _target.AreVersionsVisible = true;
            //assert
            var version = (VersionViewModel)_target.Children[0];
            Assert.AreEqual("Hide Version History", _target.VersionHeader);
            Assert.IsTrue(version.IsVersion);
            Assert.AreEqual("v.someVerNum 02022013 000000 gfedew", version.ResourceName);
            Assert.AreEqual(_target.ResourceId, version.ResourceId);
            Assert.AreEqual(versionMock.Object.VersionNumber, version.VersionNumber);
            Assert.AreSame(versionMock.Object, version.VersionInfo);
            Assert.IsFalse(version.CanEdit);
            Assert.IsFalse(version.CanCreateWorkflowService);
            Assert.IsTrue(version.ShowContextMenu);
            Assert.IsFalse(version.CanCreateSource);
            Assert.IsFalse(version.AllowResourceCheck);
            Assert.IsTrue(version.IsResourceChecked.HasValue && !version.IsResourceChecked.Value);
            Assert.AreEqual(_target.CanDelete, version.CanDelete);
            Assert.AreEqual("Version", version.ResourceType);
            Assert.IsTrue(_target.IsExpanded);
            Assert.IsTrue(isAreVersionsVisibleChanged);
            Assert.IsTrue(isChildrenChanged);
        }

        [TestMethod]
        public void TestDeployIsResourceCheckedDisabled()
        {
            //arrange
            var windowsGroupPermissionMock = new Mock<IWindowsGroupPermission>();
            windowsGroupPermissionMock.SetupGet(it => it.IsServer).Returns(true);
            windowsGroupPermissionMock.SetupGet(it => it.View).Returns(false);
            windowsGroupPermissionMock.SetupGet(it => it.DeployFrom).Returns(true);
            _serverMock.SetupGet(it => it.Permissions).Returns(new List<IWindowsGroupPermission>()
            {
                windowsGroupPermissionMock.Object
            });
            _target.SetPermission(Permissions.DeployFrom);
            _target.IsFolder = false;
            _target.IsResourceChecked = false;
            //act
            //assert
            Assert.IsFalse(_target.CanDeploy);
            Assert.AreEqual(_target.CanDeploy,_target.IsResourceCheckedEnabled);
            Assert.AreEqual(_target.DeployResourceCheckboxTooltip, Resources.Languages.Core.DeployResourceCheckboxViewPermissionError);
        }
        [TestMethod]
        public void TestDeployIsResourceCheckedEnabled_GivenView_ThenChangedToAdministrator()
        {
            //arrange
            var windowsGroupPermissionMock = new Mock<IWindowsGroupPermission>();
            windowsGroupPermissionMock.SetupGet(it => it.IsServer).Returns(true);
            windowsGroupPermissionMock.SetupGet(it => it.View).Returns(true);
            windowsGroupPermissionMock.SetupGet(it => it.Administrator).Returns(true);
            _serverMock.SetupGet(it => it.Permissions).Returns(new List<IWindowsGroupPermission>()
            {
                windowsGroupPermissionMock.Object
            });
            _target.SetPermission(Permissions.Administrator);
            _target.IsFolder = false;
            _target.IsResourceChecked = false;
            //act
            //assert
            Assert.IsTrue(_target.CanDeploy);
            Assert.AreEqual(_target.CanDeploy,_target.IsResourceCheckedEnabled);
            Assert.AreEqual(_target.DeployResourceCheckboxTooltip, Resources.Languages.Core.DeployResourceCheckbox);
        }

        [TestMethod]
        public void TestDeployIsResourceCheckedEnabled_GivenAdministrator_ThenChangedToView()
        {
            //arrange
            var windowsGroupPermissionMock = new Mock<IWindowsGroupPermission>();
            windowsGroupPermissionMock.SetupGet(it => it.IsServer).Returns(true);
            windowsGroupPermissionMock.SetupGet(it => it.Administrator).Returns(true);
            windowsGroupPermissionMock.SetupGet(it => it.View).Returns(true);            
            _serverMock.SetupGet(it => it.Permissions).Returns(new List<IWindowsGroupPermission>()
            {
                windowsGroupPermissionMock.Object
            });
            _target.SetPermission(Permissions.Administrator);
            _target.IsFolder = false;
            _target.IsResourceChecked = false;
            //act
            //assert
            Assert.IsTrue(_target.CanDeploy);
            Assert.AreEqual(_target.CanDeploy,_target.IsResourceCheckedEnabled);
            Assert.AreEqual(_target.DeployResourceCheckboxTooltip, Resources.Languages.Core.DeployResourceCheckbox);
        }
        
        [TestMethod]
        public void TestAreVersionsVisibleFalse()
        {
            //arrange
            var isAreVersionsVisibleChanged = false;
            var isChildrenChanged = false;

            _target.PropertyChanged += (sender, e) =>
            {
                isAreVersionsVisibleChanged = isAreVersionsVisibleChanged || e.PropertyName == "AreVersionsVisible";
                isChildrenChanged = isChildrenChanged || e.PropertyName == "Children";
            };
            //act
            _target.AreVersionsVisible = false;
            //assert
            Assert.IsFalse(_target.Children.Any());
            Assert.AreEqual("Show Version History", _target.VersionHeader);

            Assert.IsTrue(isAreVersionsVisibleChanged);
            Assert.IsTrue(isChildrenChanged);
        }

        [TestMethod]
        public void TestSetIsExpanderVisible()
        {
            //arrange
            var isExpanderVisibleChanged = false;
            _target.PropertyChanged += (sender, e) => isExpanderVisibleChanged = e.PropertyName == "IsExpanderVisible";
            //act
            _target.IsExpanderVisible = true;
            //assert
            Assert.IsTrue(_target.IsVisible);
            Assert.IsTrue(isExpanderVisibleChanged);
        }

        [TestMethod]
        public void TestChecked()
        {
            //arrange
            //act
            _target.Checked = true;
            //assert
            Assert.IsTrue(_target.Checked);
        }

        [TestMethod]
        public void TestIsRenaming()
        {
            //arrange
            var isRenamingFired = false;
            _target.PropertyChanged += (sender, e) =>
            {
                isRenamingFired = isRenamingFired || e.PropertyName == "IsRenaming";
            };
            //act
            _target.IsRenaming = true;
            //assert
            Assert.IsTrue(isRenamingFired);
            Assert.IsTrue(_target.IsRenaming);
        }

        [TestMethod]
        public void TestResourceNameWithoutSlashes()
        {
            //arrange
            CustomContainer.Register<IAsyncWorker>(new SynchronousAsyncWorker());
            var isResourceNameFired = false;
            _target.PropertyChanged += (sender, e) =>
            {
                isResourceNameFired = e.PropertyName == "ResourceName";
            };
            _explorerTreeItemMock.SetupGet(it => it.Children).Returns(new ObservableCollection<IExplorerItemViewModel>());
            var newName = "SomeNewName";
            _target.IsRenaming = false;
            _target.ResourcePath = "someResPath";
            _target.ResourceName = "someResPath";
            _target.Children.Clear();
            _target.IsRenaming = true;
            _explorerRepositoryMock.Setup(it => it.Rename(_target, It.IsAny<string>())).Returns(true);
            //act
            _target.ResourceName = newName;
            //assert
            Assert.IsTrue(isResourceNameFired);
            Assert.IsFalse(_target.IsRenaming);
            Assert.IsTrue(_target.ResourcePath.Contains(newName));
            _explorerRepositoryMock.Verify(it => it.Rename(_target, newName));
        }
       
        [TestMethod]
        public void TestResourceNameWithSlashes()
        {
            //arrange
            CustomContainer.Register<IAsyncWorker>(new SynchronousAsyncWorker());
            var isResourceNameFired = false;
            _target.PropertyChanged += (sender, e) =>
            {
                isResourceNameFired = e.PropertyName == "ResourceName";
            };
            _explorerTreeItemMock.SetupGet(it => it.Children).Returns(new ObservableCollection<IExplorerItemViewModel>());
            var newName = "SomeNewName";
            _target.IsRenaming = false;
            _target.ResourcePath = "Some\\someResPath";
            _target.ResourceName = "someResPath";
            _target.Children.Clear();
            _target.IsRenaming = true;
            _explorerRepositoryMock.Setup(it => it.Rename(It.IsAny<ExplorerItemViewModel>(), It.IsAny<string>())).Returns(true);
            //act
            _target.ResourceName = newName;
            //assert
            Assert.IsTrue(isResourceNameFired);
            Assert.IsFalse(_target.IsRenaming);
            Assert.AreEqual("Some\\SomeNewName", _target.ResourcePath);
            _explorerRepositoryMock.Verify(it => it.Rename(It.IsAny<ExplorerItemViewModel>(), It.IsAny<string>()));
        }

        [TestMethod]
        public void TestResourceNameDuplicate()
        {
            //arrange
            var isResourceNameFired = false;
            _target.PropertyChanged += (sender, e) =>
            {
                isResourceNameFired = e.PropertyName == "ResourceName";
            };
            var newName = "SomeNewName";
            var childMock = new Mock<IExplorerItemViewModel>();
            childMock.SetupGet(it => it.ResourceName).Returns(newName);
            _explorerTreeItemMock.SetupGet(it => it.Children)
                .Returns(new ObservableCollection<IExplorerItemViewModel>() { childMock.Object });
            _target.IsRenaming = false;
            _target.ResourcePath = "Some\\someResPath";
            _target.ResourceName = "someResPath";
            _target.Children.Clear();
            _target.IsRenaming = true;
            _explorerRepositoryMock.Setup(it => it.Rename(_target, It.IsAny<string>())).Returns(true);
            //act
            _target.ResourceName = newName;
            //assert
            Assert.IsFalse(isResourceNameFired);
            Assert.IsTrue(_target.IsRenaming);
            Assert.AreEqual("Some\\someResPath", _target.ResourcePath);
            Assert.AreEqual("someResPath", _target.ResourceName);
            _shellViewModelMock.Verify(it => it.ShowPopup(It.IsAny<IPopupMessage>()));
        }

        [TestMethod]
        public void TestActivityName()
        {
            //assert
            _target.ResourceType = "Folder";
            Assert.IsTrue(_target.ActivityName.StartsWith("Unlimited.Applications.BusinessDesignStudio.Activities.DsfActivity"));
        }

        [TestMethod]
        public void TestChildrenCount()
        {
            //arrange
            var childMockVersion = new Mock<IExplorerItemViewModel>();
            childMockVersion.SetupGet(it => it.ResourceType).Returns("Version");
            childMockVersion.SetupGet(it => it.ResourceName).Returns("ResourceVersion");
            childMockVersion.SetupGet(it => it.IsVisible).Returns(true);
            var childMockMessage = new Mock<IExplorerItemViewModel>();
            childMockMessage.SetupGet(it => it.ResourceType).Returns("Message");
            childMockMessage.SetupGet(it => it.ResourceName).Returns("ResourceMessage");
            childMockMessage.SetupGet(it => it.IsVisible).Returns(true);
            var childMockFolder = new Mock<IExplorerItemViewModel>();
            childMockFolder.SetupGet(it => it.ResourceType).Returns("Folder");
            childMockFolder.SetupGet(it => it.ResourceName).Returns("ResourceFolder");
            childMockFolder.SetupGet(it => it.IsVisible).Returns(true);
            childMockFolder.SetupGet(it => it.IsFolder).Returns(true);
            childMockFolder.SetupGet(it => it.ChildrenCount).Returns(2);
            var childMockServer = new Mock<IExplorerItemViewModel>();
            childMockServer.SetupGet(it => it.ResourceType).Returns("Server");
            childMockServer.SetupGet(it => it.ResourceName).Returns("ResourceServer");
            childMockServer.SetupGet(it => it.IsVisible).Returns(true);

            _target.AddChild(childMockVersion.Object);
            _target.AddChild(childMockMessage.Object);
            _target.AddChild(childMockFolder.Object);
            _target.AddChild(childMockServer.Object);

            _target.UpdateChildrenCount();

            //act
            var childrenCount = _target.ChildrenCount;

            //assert
            Assert.AreEqual(4, childrenCount);
        }

        #endregion Test properties

        #region Test methods

        [TestMethod]
        public void TestDeleteClosesWindow()
        {
            //arrange
            var mock = new Mock<IExplorerRepository>();
            mock.Setup(metadata => metadata.Delete(It.IsAny<IExplorerItemViewModel>())).Returns(new DeletedFileMetadata() {IsDeleted = false});
            _popupControllerMock.Setup(a => a.Show(It.IsAny<IPopupMessage>())).Returns(MessageBoxResult.Yes);
            _target.EnvironmentModel = new Mock<IEnvironmentModel>().Object;
            var child = new Mock<IExplorerItemViewModel>();
            _target.Children.Add(child.Object);
            
            PrivateObject privateObject = new PrivateObject(_target);
            privateObject.SetField("_explorerRepository", mock.Object);
            //act
            _target.Delete();

            //assert
            _shellViewModelMock.Verify(a => a.CloseResource(It.IsAny<Guid>(), It.IsAny<Guid>()));
        }

        [TestMethod]
        public void TestDispose()
        {
            //arrange
            var child = new Mock<IExplorerItemViewModel>();
            child.Setup(model => model.IsVisible).Returns(true);
            _target.AddChild(child.Object);

            //act
            _target.Dispose();

            //assert
            child.Verify(a => a.Dispose());
        }

        [TestMethod]
        public void TestFilter()
        {
            //arrange
            var isChildrenChanged = false;
            var childMock = new Mock<IExplorerItemViewModel>();
            childMock.SetupGet(it => it.IsVisible).Returns(false);
            childMock.SetupGet(it => it.ResourceType).Returns("Folder");
            _target.ResourceName = "someFilter";
            _target.ResourceType = "Message";
            _target.AddChild(childMock.Object);
            _target.PropertyChanged += (sender, e) =>
            {
                isChildrenChanged = isChildrenChanged || e.PropertyName == "Children";
            };
            //act
            _target.Filter("someFilter");
            //assert
            Assert.IsTrue(isChildrenChanged);
            Assert.IsTrue(_target.IsVisible);
            childMock.Verify(it => it.Filter("someFilter"));
        }

        [TestMethod]
        public void TestFilterNoText()
        {
            //arrange
            var isChildrenChanged = false;
            var childMock = new Mock<IExplorerItemViewModel>();
            childMock.SetupGet(it => it.IsVisible).Returns(false);
            childMock.SetupGet(it => it.ResourceType).Returns("Folder");
            _target.ResourceName = "someFilter";
            _target.ResourceType = "Folder";
            _target.IsFolder = true;
            _target.Children.Add(childMock.Object);
            _target.PropertyChanged += (sender, e) =>
            {
                isChildrenChanged = isChildrenChanged || e.PropertyName == "Children";
            };
            //act
            _target.Filter("");
            //assert
            Assert.IsTrue(isChildrenChanged);
            Assert.IsTrue(_target.IsVisible);
        }

        [TestMethod]
        public void TestFilterVersion()
        {
            //arrange
            var isChildrenChanged = false;
            var childMock = new Mock<IExplorerItemViewModel>();
            childMock.SetupGet(it => it.IsVisible).Returns(false);
            childMock.SetupGet(it => it.ResourceType).Returns("Version");
            childMock.SetupGet(it => it.IsVersion).Returns(true);
            _target.ResourceName = "someFilter";
            _target.ResourceType = "Version";
            _target.IsVersion = true;

            var parent = new Mock<IExplorerTreeItem>();
            parent.SetupGet(it => it.ResourceName).Returns("parent");

            _target.Parent = parent.Object;
            _target.AddChild(childMock.Object);
            _target.PropertyChanged += (sender, e) =>
            {
                isChildrenChanged = isChildrenChanged || e.PropertyName == "Children";
            };
            //act
            _target.Filter("someFilter");
            //assert
            Assert.IsTrue(isChildrenChanged);
            Assert.IsFalse(_target.IsVisible);
            childMock.Verify(it => it.Filter("someFilter"));
        }

        [TestMethod]
        public void TestFilterInvisible()
        {
            //arrange
            var isChildrenChanged = false;
            var childMock = new Mock<IExplorerItemViewModel>();
            childMock.SetupGet(it => it.IsVisible).Returns(true);
            childMock.SetupGet(it => it.ResourceType).Returns("Folder");
            _target.ResourceType = "Message";
            _target.ResourceName = "someFilter";
            _target.AddChild(childMock.Object);
            _target.PropertyChanged += (sender, e) =>
            {
                isChildrenChanged = isChildrenChanged || e.PropertyName == "Children";
            };
            //act
            _target.Filter("1");
            //assert
            Assert.IsTrue(isChildrenChanged);
            Assert.IsTrue(_target.IsVisible);
            childMock.Verify(it => it.Filter("1"));
        }

        [TestMethod]
        public void TestFilterInvisibleNoChildren()
        {
            //arrange
            var isChildrenChanged = false;
            var childMock = new Mock<IExplorerItemViewModel>();
            childMock.SetupGet(it => it.IsVisible).Returns(false);
            childMock.SetupGet(it => it.ResourceType).Returns("Folder");
            _target.ResourceType = "Message";
            _target.ResourceName = "someFilter";
            _target.AddChild(childMock.Object);
            _target.PropertyChanged += (sender, e) =>
            {
                isChildrenChanged = isChildrenChanged || e.PropertyName == "Children";
            };
            //act
            _target.Filter("1");
            //assert
            Assert.IsTrue(isChildrenChanged);
            Assert.IsFalse(_target.IsVisible);
            childMock.Verify(it => it.Filter("1"));
        }

        [TestMethod]
        public void TestRemoveChild()
        {
            //arrange
            var child = new Mock<IExplorerItemViewModel>().Object;
            _target.Children.Add(child);
            bool wasCalled = false;
            _target.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "Children")
                {
                    wasCalled = true;
                }
            };
            //act
            _target.RemoveChild(child);
            //assert
            Assert.IsFalse(_target.Children.Contains(child));
            Assert.IsTrue(wasCalled);
        }

        [TestMethod]
        public void TestAddChild()
        {
            //arrange
            var mockChild = new Mock<IExplorerItemViewModel>();
            mockChild.Setup(model => model.IsVisible).Returns(true);
            var child = mockChild.Object;
            
            _target.Children.Clear();

            bool wasCalled = false;
            _target.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "Children")
                {
                    wasCalled = true;
                }
            };

            //act
            _target.AddChild(child);
            //assert
            Assert.IsTrue(_target.Children.Contains(child));
            Assert.IsTrue(wasCalled);
        }

        [TestMethod]
        public void TestSelectItem()
        {
            //arrange
            var id = Guid.NewGuid();
            var childSameId = new Mock<IExplorerItemViewModel>();
            childSameId.SetupGet(it => it.ResourceId).Returns(id);
            childSameId.SetupGet(it => it.ResourceName).Returns("ResourceName");
            childSameId.SetupGet(it => it.IsVisible).Returns(true);
            var childDifferentId = new Mock<IExplorerItemViewModel>();
            childDifferentId.SetupGet(it => it.ResourceId).Returns(Guid.NewGuid);
            childDifferentId.SetupGet(it => it.IsVisible).Returns(true);
            _target.AddChild(childSameId.Object);
            var foundActionRun = false;
            _target.AddChild(childDifferentId.Object);
            Action<IExplorerItemViewModel> foundAction = item => foundActionRun = ReferenceEquals(item, childSameId.Object);
            //act
            _target.SelectItem(id, foundAction);
            //assert
            childSameId.VerifySet(it => it.IsExpanded = true);
            childSameId.VerifySet(it => it.IsSelected = true);
            Assert.IsTrue(foundActionRun);
            childDifferentId.Verify(it => it.SelectItem(id, foundAction));
        }

        [TestMethod]
        public void TestCreateNewFolderResourceTypeDbService()
        {
            //arrange
            _target.ForcedRefresh = false;
            _target.IsExpanded = false;
            _target.ResourceType = "DbService";
            _target.Children.Clear();

            //act
            _target.CreateNewFolder();

            //assert
            Assert.IsFalse(_target.IsExpanded);
            Assert.IsFalse(_target.Children.Any());
        }

        [TestMethod]
        public void TestCreateNewFolderResourceTypeFolder()
        {
            //arrange
            _serverMock.Setup(server => server.UserPermissions)
                       .Returns(Permissions.Administrator);
            _serverMock.Setup(server => server.GetPermissions(It.IsAny<Guid>())).Returns(Permissions.Administrator);

            _target.IsExpanded = false;
            _target.ResourceType = "Folder";
            _target.IsFolder = true;
            _target.Children.Clear();
            _target.AllowResourceCheck = true;
            _target.IsResourceChecked = true;
            _target.IsFolderChecked = true;
            _target.CanCreateFolder = true;
            _target.CanCreateSource = true;
            _target.CanShowVersions = true;
            _target.CanRename = true;
            _target.CanDeploy = true;
            _target.CanShowDependencies = true;
            _target.ResourcePath = Guid.NewGuid().ToString();
            _target.CanCreateWorkflowService = true;
            _target.ShowContextMenu = true;

            //act
            _target.CreateNewFolder();

            //assert
            Assert.IsTrue(_target.IsExpanded);
            //_explorerRepositoryMock.Verify(it => it.CreateFolder(_target.ResourcePath, "New Folder", It.IsAny<Guid>()));
            var createdFolder = _target.Children.Single();
            Assert.AreEqual("New Folder", createdFolder.ResourceName);
            Assert.AreEqual("Folder", createdFolder.ResourceType);
            Assert.AreEqual(_target.IsFolder, createdFolder.IsFolder);
            Assert.AreEqual(_target.AllowResourceCheck, createdFolder.AllowResourceCheck);
            Assert.AreEqual(_target.IsResourceChecked, createdFolder.IsResourceChecked);
            Assert.AreEqual(_target.IsFolderChecked, createdFolder.IsFolderChecked);
            Assert.AreEqual(_target.CanCreateFolder, createdFolder.CanCreateFolder);
            Assert.AreEqual(_target.CanCreateSource, createdFolder.CanCreateSource);
            Assert.AreEqual(_target.CanShowVersions, createdFolder.CanShowVersions);
            Assert.AreEqual(_target.CanRename, createdFolder.CanRename);
            Assert.AreEqual(_target.CanRollback, createdFolder.CanRollback);
            Assert.AreEqual(_target.CanDeploy, createdFolder.CanDeploy);
            Assert.AreEqual(_target.CanShowDependencies, createdFolder.CanShowDependencies);
            Assert.AreEqual(_target.ResourcePath + "\\" + createdFolder.ResourceName, createdFolder.ResourcePath);
            Assert.AreEqual(_target.CanCreateWorkflowService, createdFolder.CanCreateWorkflowService);
            Assert.AreEqual(_target.ShowContextMenu, createdFolder.ShowContextMenu);
            Assert.IsTrue(createdFolder.IsRenaming);
        }

        [TestMethod]
        public void TestCreateNewFolderResourceTypeFolderNewFolder1()
        {
            //arrange
            const string newFolderName = "New Folder";
            var childMock = new Mock<IExplorerItemViewModel>();
            childMock.SetupGet(it => it.ResourceName).Returns(newFolderName);
            childMock.SetupGet(it => it.IsVisible).Returns(true);
            _target.IsExpanded = false;
            _target.ResourceType = "Folder";
            _target.IsFolder = true;
            _target.Children.Clear();
            _target.AddChild(childMock.Object);

            //act
            _target.CreateNewFolder();

            //assert
            Assert.IsTrue(_target.IsExpanded);
            //_explorerRepositoryMock.Verify(it => it.CreateFolder(_target.ResourcePath, "New Folder 1", It.IsAny<Guid>()));
        }

        [TestMethod]
        public void TestApply()
        {
            //arrange
            var child = new Mock<IExplorerItemViewModel>();
            child.Setup(model => model.IsVisible).Returns(true);
            bool actionRun = false;
            _target.AddChild(child.Object);
            Action<IExplorerItemViewModel> action = a => actionRun = ReferenceEquals(_target, a);
            //act
            _target.Apply(action);
            //assert
            Assert.IsTrue(actionRun);
            child.Verify(it => it.Apply(It.IsAny<Action<IExplorerItemViewModel>>()));
        }

        [TestMethod]
        public void TestFilterChildrenFound()
        {
            //arrange
            var propertyChangedRaised = false;
            var child = new Mock<IExplorerItemViewModel>();
            child.SetupGet(it => it.IsVisible).Returns(true);
            child.SetupGet(it => it.ResourceType).Returns("Folder");
            _target.AddChild(child.Object);
            Func<IExplorerItemViewModel, bool> filter = item => false;
            _target.PropertyChanged += (sender, e) => propertyChangedRaised = e.PropertyName == "Children";
            //act
            _target.Filter(filter);
            //assert
            Assert.IsTrue(propertyChangedRaised);
            child.Verify(it => it.Filter(filter));
            Assert.IsTrue(_target.IsVisible);
        }

        [TestMethod]
        public void TestFilterChildrenEmpty()
        {
            //arrange
            var propertyChangedRaised = false;
            _target.ResourceName = Guid.NewGuid().ToString();
            _target.ResourceType = "Folder";
#pragma warning disable 252,253
            Func<IExplorerItemViewModel, bool> filter = item => item == _target;
#pragma warning restore 252,253
            _target.PropertyChanged += (sender, e) => propertyChangedRaised = e.PropertyName == "Children";
            //act
            _target.Filter(filter);
            //assert
            Assert.IsTrue(propertyChangedRaised);
            Assert.IsTrue(_target.IsVisible);
        }

        [TestMethod]
        public void TestAsList()
        {
            //arrange
            var child = new Mock<IExplorerItemViewModel>();
            child.Setup(model => model.IsVisible).Returns(true);
            var child2 = new Mock<IExplorerItemViewModel>();
            child2.Setup(model => model.IsVisible).Returns(true);
            child.Setup(it => it.AsList()).Returns(new List<IExplorerItemViewModel> { child2.Object });
            _target.AddChild(child.Object);
            _target.Children.Add(child.Object);
            _target.Children.Add(child2.Object);
            //act
            var result = _target.AsList();
            //assert
            Assert.IsTrue(result.Contains(child2.Object));
        }

        [TestMethod]
        public void TestAsListChildrenNull()
        {
            //arrange
            _target.Children = null;
            //act
            var result = _target.AsList();
            //assert
            Assert.IsNotNull(result);
        }


        [TestMethod]
        public void TestSetPermissionsSameResource()
        {
            //arrange
            _target.ResourceId = Guid.NewGuid();
            _target.ResourceType = "WorkflowService";
            _target.IsService = true;           
            //act
            _target.SetPermissions(Permissions.Administrator);
            //assert
            Assert.IsTrue(_target.CanEdit);
            Assert.IsTrue(_target.CanView);
            Assert.IsTrue(_target.CanRename);
            Assert.IsTrue(_target.CanDuplicate);
            Assert.IsTrue(_target.CanCreateTest);
            Assert.IsTrue(_target.CanDelete);
            Assert.IsTrue(_target.CanMove);
            Assert.IsFalse(_target.CanCreateFolder);
            Assert.IsTrue(_target.CanDeploy);
            Assert.IsTrue(_target.CanShowVersions);
            Assert.IsTrue(_target.CanCreateWorkflowService);
            Assert.IsTrue(_target.CanCreateSource);
        }

        [TestMethod]
        public void TestNonrPermissionsSameResource()
        {
            //arrange
            _target.ResourceId = Guid.NewGuid();
            _target.ResourceType = "WorkflowService";
            _target.IsService = true;           
            //act
            _target.SetPermissions(Permissions.None);
            //assert
            Assert.IsFalse(_target.CanEdit);
            Assert.IsFalse(_target.CanView);
            Assert.IsFalse(_target.CanRename);
            Assert.IsFalse(_target.CanDuplicate);
            Assert.IsFalse(_target.CanCreateTest);
            Assert.IsFalse(_target.CanDelete);
            Assert.IsFalse(_target.CanMove);
            Assert.IsFalse(_target.CanCreateFolder);
            Assert.IsFalse(_target.CanDeploy);
            Assert.IsFalse(_target.CanShowVersions);
            Assert.IsFalse(_target.CanCreateWorkflowService);
            Assert.IsFalse(_target.CanCreateSource);
        }

        [TestMethod]
        public void TestSetPermissions_ContributePermission_AllowsMove()
        {
            //arrange
            _target.ResourceType = "WorkflowService";
            _target.IsService = true;
            //act
            _target.SetPermissions(Permissions.Contribute);
            //assert
            Assert.IsTrue(_target.CanEdit);
            Assert.IsTrue(_target.CanView);
            Assert.IsTrue(_target.CanRename);
            Assert.IsTrue(_target.CanDuplicate);
            Assert.IsTrue(_target.CanCreateTest);
            Assert.IsTrue(_target.CanDelete);
            Assert.IsTrue(_target.CanMove);
            Assert.IsFalse(_target.CanCreateFolder);
            Assert.IsFalse(_target.CanDeploy);
            Assert.IsTrue(_target.CanShowVersions);
            Assert.IsTrue(_target.CanCreateWorkflowService);
            Assert.IsTrue(_target.CanCreateSource);
        }

        [TestMethod]
        public void TestSetPermissionsServerPermission()
        {
            //arrange

            _target.ResourceId = Guid.NewGuid();
            _target.ResourceType = "WorkflowService";
            _target.IsService = true;
            //act
            _target.SetPermissions(Permissions.Administrator);
            //assert
            Assert.IsTrue(_target.CanEdit);
            Assert.IsTrue(_target.CanView);
            Assert.IsTrue(_target.CanRename);
            Assert.IsTrue(_target.CanMove);
            Assert.IsTrue(_target.CanDuplicate);
            Assert.IsTrue(_target.CanCreateTest);
            Assert.IsTrue(_target.CanDelete);
            Assert.IsFalse(_target.CanCreateFolder);
            Assert.IsTrue(_target.CanDeploy);
            Assert.IsTrue(_target.CanShowVersions);
            Assert.IsTrue(_target.CanCreateWorkflowService);
            Assert.IsTrue(_target.CanCreateSource);
        }

        [TestMethod]
        public void TestCanViewSwaggerIsVisible()
        {
            //arrange
            _target.ResourceType = "WorkflowService";
            _target.CanView = true;
            _target.IsService = true;
            _target.IsSource = false;
            
            //act

            //assert
            Assert.IsTrue(_target.IsService);
            Assert.IsTrue(_target.CanViewSwagger);
            Assert.IsTrue(_target.CanViewApisJson);
        }

        [TestMethod]
        public void TestCanViewSwaggerIsNotVisible()
        {
            //arrange
            _target.ResourceType = "Folder";
            _target.CanView = true;
            _target.IsService = false;
            _target.IsFolder = true;
            //act

            //assert
            Assert.IsTrue(_target.IsFolder);
            Assert.IsFalse(_target.CanViewSwagger);
            Assert.IsTrue(_target.CanViewApisJson);
        }

        [TestMethod]
        public void TestCanViewApisJsonIsVisible()
        {
            //arrange
            _target.ResourceType = "Folder";
            _target.CanView = true;
            _target.IsService = false;
            _target.IsFolder = true;
            //act

            //assert
            Assert.IsTrue(_target.IsFolder);
            Assert.IsFalse(_target.CanViewSwagger);
            Assert.IsTrue(_target.CanViewApisJson);
        }

        [TestMethod]
        public void TestSetPermissionsServerPermissionFolder()
        {
            //arrange

            _target.ResourceType = "Folder";
            _target.IsFolder = true;
            //act
            _target.SetPermissions(Permissions.Administrator);
            //assert
            Assert.IsFalse(_target.CanEdit);
            Assert.IsFalse(_target.CanExecute);
        }

        [TestMethod]
        public void TestSetPermissionsIsDeploy()
        {
            //arrange

            _target.ResourceType = "WorkflowService";

            //act
            _target.SetPermissions(Permissions.DeployFrom, true);
            //assert
            Assert.IsFalse(_target.CanEdit);
            Assert.IsFalse(_target.CanExecute);
        }

        [TestMethod]
        public void TestAddSibling()
        {
            //arrange
            var siblingMock = new Mock<IExplorerItemViewModel>();
            //act
            _target.AddSibling(siblingMock.Object);
            //assert
            _explorerTreeItemMock.Verify(it => it.AddChild(siblingMock.Object));
        }

        [TestMethod]
        public async System.Threading.Tasks.Task TestMoveAlreadyExist()
        {
            //arrange
            var movedItem = new Mock<IExplorerTreeItem>();
            var childDestItem = new Mock<IExplorerItemViewModel>();
            _target.ResourceName = "someName";
            _target.ResourceType = "EmailSource";
            childDestItem.SetupGet(it => it.ResourceName).Returns(_target.ResourceName);
            movedItem.SetupGet(it => it.Children).Returns(new ObservableCollection<IExplorerItemViewModel>()
            {
                childDestItem.Object
            });
            //act
            var result = await _target.Move(movedItem.Object);
            //assert
            Assert.IsFalse(result);
            _shellViewModelMock.Verify(it => it.ShowPopup(It.IsAny<IPopupMessage>()));
        }

        [TestMethod]
        public async System.Threading.Tasks.Task TestMoveToFolderExists()
        {
            //arrange
            var destinationMock = new Mock<IExplorerTreeItem>();
            destinationMock.Setup(it => it.ResourceType).Returns("Folder");
            var currentChildrenMock = new Mock<IExplorerItemViewModel>();
            currentChildrenMock.SetupGet(it => it.ResourceName).Returns("someResourceName");
            var childDestItem = new Mock<IExplorerItemViewModel>();
            _target.ResourceName = "someName";
            _target.ResourceType = "Folder";
            _target.IsFolder = true;
            _target.Children.Add(currentChildrenMock.Object);
            childDestItem.SetupGet(it => it.ResourceName).Returns(_target.ResourceName);
            childDestItem.SetupGet(it => it.ResourceType).Returns("Folder");
            childDestItem.SetupGet(it => it.ResourcePath).Returns("somePath");
            childDestItem.SetupGet(it => it.Children).Returns(new ObservableCollection<IExplorerItemViewModel>());
            destinationMock.SetupGet(it => it.Children).Returns(new ObservableCollection<IExplorerItemViewModel>()
            {
                childDestItem.Object
            });
            var studioUpdateManagerMock = new Mock<IStudioUpdateManager>();

            _serverMock.SetupGet(it => it.UpdateRepository).Returns(studioUpdateManagerMock.Object);
            //act
            var result = await _target.Move(destinationMock.Object);
            //assert
            Assert.IsFalse(result);
            _explorerRepositoryMock.Verify(it => it.Move(_target, destinationMock.Object));
        }
//
//        [TestMethod]
//        public async System.Threading.Tasks.Task TestMoveToFolderNotExists()
//        {
//            //arrange
//            var destinationMock = new Mock<IExplorerTreeItem>();
//            destinationMock.Setup(it => it.ResourceType).Returns("Folder");
//            destinationMock.SetupGet(it => it.ResourcePath).Returns("someDestPath");
//            var currentChildrenMock = new Mock<IExplorerItemViewModel>();
//            currentChildrenMock.SetupGet(it => it.ResourceName).Returns("someResourceName");
//            var childDestItem = new Mock<IExplorerItemViewModel>();
//            _target.ResourceName = "someName";
//            _target.ResourceType = "Folder";
//            _target.Children.Add(currentChildrenMock.Object);
//            childDestItem.SetupGet(it => it.ResourceName).Returns("someOtherName");
//            childDestItem.SetupGet(it => it.ResourceType).Returns("Folder");
//            childDestItem.SetupGet(it => it.ResourcePath).Returns("somePath");
//            childDestItem.SetupGet(it => it.Children).Returns(new ObservableCollection<IExplorerItemViewModel>());
//            destinationMock.SetupGet(it => it.Children).Returns(new ObservableCollection<IExplorerItemViewModel>()
//            {
//                childDestItem.Object
//            });
//            var studioUpdateManagerMock = new Mock<IStudioUpdateManager>();
//
//            _serverMock.SetupGet(it => it.UpdateRepository).Returns(studioUpdateManagerMock.Object);
//            //act
//            var result = await _target.Move(destinationMock.Object);
//            //assert
//            Assert.IsTrue(result);
//            _explorerRepositoryMock.Verify(it => it.Move(_target, destinationMock.Object));
//        }
//
//        [TestMethod]
//        public async System.Threading.Tasks.Task TestMoveToLtFolder()
//        {
//            //arrange
//            var destinationMock = new Mock<IExplorerTreeItem>();
//            destinationMock.Setup(it => it.ResourceType).Returns("ServerSource");
//            destinationMock.SetupGet(it => it.ResourcePath).Returns("someDestPath");
//            var childDestItem = new Mock<IExplorerItemViewModel>();
//            childDestItem.SetupGet(it => it.ResourceName).Returns("someOtherName");
//            childDestItem.SetupGet(it => it.ResourceType).Returns("Folder");
//            childDestItem.SetupGet(it => it.ResourcePath).Returns("somePath");
//            childDestItem.SetupGet(it => it.Children).Returns(new ObservableCollection<IExplorerItemViewModel>());
//            destinationMock.SetupGet(it => it.Children).Returns(new ObservableCollection<IExplorerItemViewModel>()
//            {
//                childDestItem.Object
//            });
//            _target.ResourceName = "someName";
//            _target.ResourceType = "WebSource";
//
//            var studioUpdateManagerMock = new Mock<IStudioUpdateManager>();
//
//            _serverMock.SetupGet(it => it.UpdateRepository).Returns(studioUpdateManagerMock.Object);
//            //act
//            var result = await _target.Move(destinationMock.Object);
//            //assert
//            Assert.IsTrue(result);
//        }
////
//        [TestMethod]
//        public async System.Threading.Tasks.Task TestMoveParentNull()
//        {
//            //arrange
//            var destinationMock = new Mock<IExplorerTreeItem>();
//            destinationMock.Setup(it => it.ResourceType).Returns("DropboxSource");
//            destinationMock.SetupGet(it => it.ResourcePath).Returns("someDestPath");
//            destinationMock.SetupGet(it => it.Parent).Returns((IExplorerTreeItem)null);
//            var childDestItem = new Mock<IExplorerItemViewModel>();
//            childDestItem.SetupGet(it => it.ResourceName).Returns("someOtherName");
//            childDestItem.SetupGet(it => it.ResourceType).Returns("Folder");
//            childDestItem.SetupGet(it => it.ResourcePath).Returns("somePath");
//            childDestItem.SetupGet(it => it.Children).Returns(new ObservableCollection<IExplorerItemViewModel>());
//            destinationMock.SetupGet(it => it.Children).Returns(new ObservableCollection<IExplorerItemViewModel>()
//            {
//                childDestItem.Object
//            });
//            _target.ResourceName = "someName";
//            _target.ResourceType = "WebSource";
//
//            var studioUpdateManagerMock = new Mock<IStudioUpdateManager>();
//
//            _serverMock.SetupGet(it => it.UpdateRepository).Returns(studioUpdateManagerMock.Object);
//            //act
//            var result = await _target.Move(destinationMock.Object);
//            //assert
//            Assert.IsTrue(result);
//        }

        [TestMethod]
        public async System.Threading.Tasks.Task TestMoveException()
        {
            //arrange
            var destinationMock = new Mock<IExplorerTreeItem>();
            destinationMock.Setup(it => it.ResourceType).Returns("ServerSource");
            destinationMock.SetupGet(it => it.ResourcePath).Returns("someDestPath");
            var childDestItem = new Mock<IExplorerItemViewModel>();
            childDestItem.SetupGet(it => it.ResourceName).Returns("someOtherName");
            childDestItem.SetupGet(it => it.ResourceType).Returns("Folder");
            childDestItem.SetupGet(it => it.ResourcePath).Returns("somePath");
            childDestItem.SetupGet(it => it.Children).Returns(new ObservableCollection<IExplorerItemViewModel>());
            destinationMock.Setup(it => it.AddChild(_target)).Throws(new Exception());
            destinationMock.SetupGet(it => it.Children).Returns(new ObservableCollection<IExplorerItemViewModel>()
            {
                childDestItem.Object
            });
            _target.ResourceName = "someName";
            _target.ResourceType = "WebSource";

            var studioUpdateManagerMock = new Mock<IStudioUpdateManager>();
            _explorerRepositoryMock.Setup(a => a.Move(It.IsAny<IExplorerItemViewModel>(), It.IsAny<IExplorerTreeItem>())).Throws(new Exception());
            _serverMock.SetupGet(it => it.UpdateRepository).Returns(studioUpdateManagerMock.Object);
            //act
            var result = await _target.Move(destinationMock.Object);
            //assert
            Assert.IsFalse(result);
        }

        #endregion Test methods
    }
}
