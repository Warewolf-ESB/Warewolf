using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Deploy;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Studio.Core;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

// ReSharper disable InconsistentNaming

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class SingleExplorerDeployViewModelTests
    {
        private Mock<IDeployDestinationExplorerViewModel> _destView;
        private Mock<IDeploySourceExplorerViewModel> _sourceView;
        private Mock<IDeployStatsViewerViewModel> _statsView;
        private Mock<IShellViewModel> _shellVm;
        private Mock<IServer> _serverMock;
        private Mock<IServer> _differentServerMock;
        private Mock<IEventAggregator> _eventAggregatorMock;
        private Mock<IStudioUpdateManager> _updateRepositoryMock;

        private Guid _serverEnvironmentId;

        [TestInitialize]
        public void Initialize()
        {
            _destView = new Mock<IDeployDestinationExplorerViewModel>();
            _sourceView = new Mock<IDeploySourceExplorerViewModel>();
            _statsView = new Mock<IDeployStatsViewerViewModel>();
            _statsView.SetupAllProperties();
            _shellVm = new Mock<IShellViewModel>();
            _serverMock = new Mock<IServer>();
            _differentServerMock = new Mock<IServer>();
            _eventAggregatorMock = new Mock<IEventAggregator>();
            _updateRepositoryMock = new Mock<IStudioUpdateManager>();
            _sourceView.Setup(model => model.SelectedItems).Returns(new List<IExplorerTreeItem>());
        }

        //SingleExplorerDeployViewModel(IDeployDestinationExplorerViewModel destination, IDeploySourceExplorerViewModel source, 
        //IEnumerable<IExplorerTreeItem> selectedItems, IDeployStatsViewerViewModel stats, IShellViewModel shell, IPopupController popupController)
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CanDeploytests_GivenCanSelectAllDependencies_ShouldMatch()
        {
            //---------------Set up test pack-------------------
            var popupController = new Mock<IPopupController>();
            var connectControl = new Mock<IConnectControlViewModel>();
            connectControl.SetupAllProperties();
            _destView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            _sourceView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            var singleExplorerDeployViewModel = new SingleExplorerDeployViewModel(_destView.Object, _sourceView.Object, new List<IExplorerTreeItem>(), _statsView.Object, _shellVm.Object, popupController.Object);
            //---------------Assert Precondition----------------
            Assert.IsFalse(singleExplorerDeployViewModel.CanSelectDependencies);
            Assert.IsFalse(singleExplorerDeployViewModel.CanDeployTests);
            //---------------Execute Test ----------------------
            _sourceView.Setup(model => model.SelectedItems).Returns(new List<IExplorerTreeItem>()
            {
                new Mock<IExplorerTreeItem>().Object
            });

            //---------------Test Result -----------------------
            Assert.IsTrue(singleExplorerDeployViewModel.CanSelectDependencies);
            Assert.IsTrue(singleExplorerDeployViewModel.CanDeployTests);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CanDeploy_GivenDestinationIsNotConnected_ShouldReturnFalseAndSetCorrectMessage()
        {
            //---------------Set up test pack-------------------
            var env = new Mock<IEnvironmentViewModel>();
            env.SetupGet(model => model.IsConnected).Returns(true);
            _sourceView.Setup(model => model.SelectedEnvironment).Returns(env.Object);
            var popupController = new Mock<IPopupController>();
            var connectControl = new Mock<IConnectControlViewModel>();
            connectControl.SetupAllProperties();
            connectControl.SetupGet(model => model.IsConnected).Returns(false);
            _destView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            _sourceView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            var singleExplorerDeployViewModel = new SingleExplorerDeployViewModel(_destView.Object, _sourceView.Object, new List<IExplorerTreeItem>(), _statsView.Object, _shellVm.Object, popupController.Object);
            //---------------Assert Precondition----------------
            Assert.IsFalse(singleExplorerDeployViewModel.CanSelectDependencies);
            Assert.IsFalse(singleExplorerDeployViewModel.CanDeployTests);
            //---------------Execute Test ----------------------
            _sourceView.Setup(model => model.SelectedItems).Returns(new List<IExplorerTreeItem>()
            {
                new Mock<IExplorerTreeItem>().Object

            });

            //---------------Test Result -----------------------
            Assert.IsFalse(singleExplorerDeployViewModel.DeployCommand.CanExecute(null));
            var errorMessage = singleExplorerDeployViewModel.ErrorMessage;
            Assert.AreEqual(Resources.Languages.Core.DeployDestinationNotConnected, errorMessage);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void DestinationOnPropertyChanged_GivenisConnectedChanged_ShouldHandleDeployChanged()
        {
            //---------------Set up test pack-------------------
            var env = new Mock<IEnvironmentViewModel>();
            env.SetupGet(model => model.IsConnected).Returns(true);
            _sourceView.Setup(model => model.SelectedEnvironment).Returns(env.Object);
            var popupController = new Mock<IPopupController>();
            var connectControl = new Mock<IConnectControlViewModel>();
            connectControl.SetupAllProperties();
            connectControl.SetupGet(model => model.IsConnected).Returns(false);
            _destView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            _sourceView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            var singleExplorerDeployViewModel = new SingleExplorerDeployViewModel(_destView.Object, _sourceView.Object, new List<IExplorerTreeItem>(), _statsView.Object, _shellVm.Object, popupController.Object);
            bool wasCalled = false;
            singleExplorerDeployViewModel.Destination.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "IsConnected")
                {
                    wasCalled = true;
                }
            };
            //---------------Assert Precondition----------------
            Assert.IsFalse(singleExplorerDeployViewModel.CanSelectDependencies);
            Assert.IsFalse(singleExplorerDeployViewModel.CanDeployTests);
            //---------------Execute Test ----------------------
            _sourceView.Setup(model => model.SelectedItems).Returns(new List<IExplorerTreeItem>()
            {
                new Mock<IExplorerTreeItem>().Object

            });
            var propertyChangedEventArgs = new PropertyChangedEventArgs("IsConnected");
            _destView.Raise(model => model.PropertyChanged += (sender, args) => { }, singleExplorerDeployViewModel, propertyChangedEventArgs);
            //---------------Test Result -----------------------
            Assert.IsTrue(wasCalled);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Destination_GivenNewDestinationIsCreated_ShouldHaveNewDestination()
        {
            //---------------Set up test pack-------------------
            _updateRepositoryMock.SetupProperty(manager => manager.ServerSaved);
            _serverMock.SetupGet(it => it.UpdateRepository).Returns(_updateRepositoryMock.Object);
            _serverMock.SetupGet(it => it.ResourceName).Returns("some text");
            _serverEnvironmentId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(_serverEnvironmentId);
            _shellVm.Setup(model => model.LocalhostServer).Returns(_serverMock.Object);
            var popupController = new Mock<IPopupController>();
            var connectControl = new Mock<IConnectControlViewModel>();
            connectControl.SetupAllProperties();
            _destView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            _sourceView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            var singleExplorerDeployViewModel = new SingleExplorerDeployViewModel(_destView.Object, _sourceView.Object, new List<IExplorerTreeItem>(), _statsView.Object, _shellVm.Object, popupController.Object);
            //---------------Assert Precondition----------------
            var deployDestinationExplorerViewModelBefore = singleExplorerDeployViewModel.Destination;
            Assert.IsNotNull(deployDestinationExplorerViewModelBefore);
            //---------------Execute Test ----------------------
            singleExplorerDeployViewModel.Destination = new DeployDestinationViewModel(_shellVm.Object, new EventAggregator());
            //---------------Test Result -----------------------
            Assert.IsFalse(Equals(deployDestinationExplorerViewModelBefore, singleExplorerDeployViewModel.Destination));
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void CanDeploy_GivenServerIsNotConnected_ShouldHaveFalse()
        {
            //---------------Set up test pack-------------------
            _updateRepositoryMock.SetupProperty(manager => manager.ServerSaved);
            _serverMock.SetupGet(it => it.UpdateRepository).Returns(_updateRepositoryMock.Object);
            _serverMock.SetupGet(it => it.IsConnected).Returns(false);
            _serverMock.SetupGet(it => it.ResourceName).Returns("some text");
            _serverEnvironmentId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(_serverEnvironmentId);
            _shellVm.Setup(model => model.LocalhostServer).Returns(_serverMock.Object);
            var popupController = new Mock<IPopupController>();
            var connectControl = new Mock<IConnectControlViewModel>();
            connectControl.SetupAllProperties();
            _destView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            _sourceView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            var singleExplorerDeployViewModel = new SingleExplorerDeployViewModel(_destView.Object, _sourceView.Object, new List<IExplorerTreeItem>(), _statsView.Object, _shellVm.Object, popupController.Object)
            {
                SourceConnectControlViewModel = { SelectedConnection = _serverMock.Object }
            };
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            PrivateObject privateObject = new PrivateObject(singleExplorerDeployViewModel);
            var canDeploy = privateObject.GetProperty("CanDeploy");
            //---------------Test Result -----------------------
            Assert.AreEqual(false, canDeploy);
            Assert.AreEqual(Resources.Languages.Core.DeploySourceNotConnected, singleExplorerDeployViewModel.ErrorMessage);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void CanDeploy_GivenSourceAndDestinationAreSameServer_ShouldHaveFalse()
        {
            //---------------Set up test pack-------------------
            _updateRepositoryMock.SetupProperty(manager => manager.ServerSaved);
            _serverMock.SetupGet(it => it.UpdateRepository).Returns(_updateRepositoryMock.Object);
            _serverMock.SetupGet(it => it.IsConnected).Returns(true);
            _serverMock.SetupGet(it => it.ResourceName).Returns("some text");
            _serverEnvironmentId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(_serverEnvironmentId);
            _shellVm.Setup(model => model.LocalhostServer).Returns(_serverMock.Object);
            var popupController = new Mock<IPopupController>();
            var connectControl = new Mock<IConnectControlViewModel>();
            connectControl.SetupAllProperties();
            var destEnv = new Mock<IEnvironmentViewModel>();
            destEnv.Setup(model => model.IsConnected).Returns(true);
            destEnv.Setup(model => model.Server).Returns(_serverMock.Object);
            _destView.Setup(model => model.SelectedEnvironment).Returns(destEnv.Object);
            _destView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            var sourceEnv = new Mock<IEnvironmentViewModel>();
            sourceEnv.Setup(model => model.IsConnected).Returns(true);
            sourceEnv.Setup(model => model.Server).Returns(_serverMock.Object);
            _sourceView.Setup(model => model.SelectedEnvironment).Returns(sourceEnv.Object);
            _sourceView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            var singleExplorerDeployViewModel = new SingleExplorerDeployViewModel(_destView.Object, _sourceView.Object, new List<IExplorerTreeItem>(), _statsView.Object, _shellVm.Object, popupController.Object)
            {
                SourceConnectControlViewModel = { SelectedConnection = _serverMock.Object },
            };
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            PrivateObject privateObject = new PrivateObject(singleExplorerDeployViewModel);
            var canDeploy = privateObject.GetProperty("CanDeploy");
            //---------------Test Result -----------------------
            Assert.AreEqual(false, canDeploy);
            Assert.AreEqual(Resources.Languages.Core.DeploySourceDestinationAreSame, singleExplorerDeployViewModel.ErrorMessage);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void CanDeploy_GivenSourceServerSelectedItemsIsNull_ShouldHaveFalse()
        {
            //---------------Set up test pack-------------------
            _updateRepositoryMock.SetupProperty(manager => manager.ServerSaved);
            _serverMock.SetupGet(it => it.UpdateRepository).Returns(_updateRepositoryMock.Object);
            _serverMock.SetupGet(it => it.IsConnected).Returns(true);
            _differentServerMock.SetupGet(it => it.IsConnected).Returns(true);
            _serverMock.SetupGet(it => it.ResourceName).Returns("some text");
            _serverEnvironmentId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(_serverEnvironmentId);
            _shellVm.Setup(model => model.LocalhostServer).Returns(_serverMock.Object);
            var popupController = new Mock<IPopupController>();
            var connectControl = new Mock<IConnectControlViewModel>();
            connectControl.SetupAllProperties();
            var connectControl2 = new Mock<IConnectControlViewModel>();
            connectControl2.SetupAllProperties();
            var destEnv = new Mock<IEnvironmentViewModel>();
            destEnv.Setup(model => model.IsConnected).Returns(true);
            destEnv.Setup(model => model.Server).Returns(_differentServerMock.Object);
            _destView.Setup(model => model.SelectedEnvironment).Returns(destEnv.Object);
            _destView.Setup(model => model.ConnectControlViewModel).Returns(connectControl2.Object);
            var sourceEnv = new Mock<IEnvironmentViewModel>();
            sourceEnv.Setup(model => model.IsConnected).Returns(true);
            sourceEnv.Setup(model => model.Server).Returns(_serverMock.Object);
            _sourceView.Setup(model => model.SelectedEnvironment).Returns(sourceEnv.Object);
            _sourceView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            var singleExplorerDeployViewModel = new SingleExplorerDeployViewModel(_destView.Object, _sourceView.Object, new List<IExplorerTreeItem>(), _statsView.Object, _shellVm.Object, popupController.Object)
            {
                SourceConnectControlViewModel = { SelectedConnection = _serverMock.Object },
                DestinationConnectControlViewModel = { SelectedConnection = _differentServerMock.Object },
            };
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            PrivateObject privateObject = new PrivateObject(singleExplorerDeployViewModel);
            var canDeploy = privateObject.GetProperty("CanDeploy");
            //---------------Test Result -----------------------
            Assert.AreEqual(false, canDeploy);
            Assert.AreEqual(Resources.Languages.Core.DeployNoResourcesSelected, singleExplorerDeployViewModel.ErrorMessage);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void CanDeploy_GivenNoSourceServerPermissions_ShouldHaveFalse()
        {
            //---------------Set up test pack-------------------
            _updateRepositoryMock.SetupProperty(manager => manager.ServerSaved);
            _serverMock.SetupGet(it => it.UpdateRepository).Returns(_updateRepositoryMock.Object);
            _serverMock.SetupGet(it => it.IsConnected).Returns(true);
            _serverMock.Setup(server => server.Permissions).Returns(new Mock<List<IWindowsGroupPermission>>().Object);
            _serverMock.Setup(server => server.CanDeployFrom).Returns(false);
            _differentServerMock.SetupGet(it => it.IsConnected).Returns(true);
            _serverMock.SetupGet(it => it.ResourceName).Returns("some text");
            _serverEnvironmentId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(_serverEnvironmentId);
            _shellVm.Setup(model => model.LocalhostServer).Returns(_serverMock.Object);
            var popupController = new Mock<IPopupController>();
            var connectControl = new Mock<IConnectControlViewModel>();
            connectControl.SetupAllProperties();
            var connectControl2 = new Mock<IConnectControlViewModel>();
            connectControl2.SetupAllProperties();
            var destEnv = new Mock<IEnvironmentViewModel>();
            destEnv.Setup(model => model.IsConnected).Returns(true);
            destEnv.Setup(model => model.Server).Returns(_differentServerMock.Object);
            _destView.Setup(model => model.SelectedEnvironment).Returns(destEnv.Object);
            _destView.Setup(model => model.ConnectControlViewModel).Returns(connectControl2.Object);
            var sourceEnv = new Mock<IEnvironmentViewModel>();
            sourceEnv.Setup(model => model.IsConnected).Returns(true);
            sourceEnv.Setup(model => model.Server).Returns(_serverMock.Object);
            _sourceView.Setup(model => model.SelectedEnvironment).Returns(sourceEnv.Object);
            _sourceView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            var explorerTreeItems = new List<IExplorerTreeItem>
            {
                new ExplorerItemViewModel(_serverMock.Object, It.IsAny<IExplorerTreeItem>(), model => It.IsAny<IExplorerItemViewModel>(),_shellVm.Object, new Mock<IPopupController>().Object)
            };
            _sourceView.SetupGet(model => model.SelectedItems).Returns(explorerTreeItems);
            var singleExplorerDeployViewModel = new SingleExplorerDeployViewModel(_destView.Object, _sourceView.Object, explorerTreeItems, _statsView.Object, _shellVm.Object, popupController.Object)
            {
                SourceConnectControlViewModel = { SelectedConnection = _serverMock.Object },
                DestinationConnectControlViewModel = { SelectedConnection = _differentServerMock.Object },
            };
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            PrivateObject privateObject = new PrivateObject(singleExplorerDeployViewModel);
            var canDeploy = privateObject.GetProperty("CanDeploy");
            //---------------Test Result -----------------------
            Assert.AreEqual(false, canDeploy);
            Assert.AreEqual(StringResources.SourcePermission_Error, singleExplorerDeployViewModel.ErrorMessage);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void CanDeploy_GivenNoDestinarionSourcePermissions_ShouldHaveFalse()
        {
            //---------------Set up test pack-------------------
            _updateRepositoryMock.SetupProperty(manager => manager.ServerSaved);
            _serverMock.SetupGet(it => it.UpdateRepository).Returns(_updateRepositoryMock.Object);
            _serverMock.SetupGet(it => it.IsConnected).Returns(true);
            _serverMock.Setup(server => server.Permissions).Returns(new Mock<List<IWindowsGroupPermission>>().Object);
            _serverMock.Setup(server => server.CanDeployFrom).Returns(true);
            _differentServerMock.SetupGet(it => it.IsConnected).Returns(true);
            _differentServerMock.Setup(server => server.Permissions).Returns(new Mock<List<IWindowsGroupPermission>>().Object);
            _differentServerMock.Setup(server => server.CanDeployTo).Returns(false);
            _serverMock.SetupGet(it => it.ResourceName).Returns("some text");
            _serverEnvironmentId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(_serverEnvironmentId);
            _shellVm.Setup(model => model.LocalhostServer).Returns(_serverMock.Object);
            var popupController = new Mock<IPopupController>();
            var connectControl = new Mock<IConnectControlViewModel>();
            connectControl.SetupAllProperties();
            var connectControl2 = new Mock<IConnectControlViewModel>();
            connectControl2.SetupAllProperties();
            var destEnv = new Mock<IEnvironmentViewModel>();
            destEnv.Setup(model => model.IsConnected).Returns(true);
            destEnv.Setup(model => model.Server).Returns(_differentServerMock.Object);
            _destView.Setup(model => model.SelectedEnvironment).Returns(destEnv.Object);
            _destView.Setup(model => model.ConnectControlViewModel).Returns(connectControl2.Object);
            var sourceEnv = new Mock<IEnvironmentViewModel>();
            sourceEnv.Setup(model => model.IsConnected).Returns(true);
            sourceEnv.Setup(model => model.Server).Returns(_serverMock.Object);
            _sourceView.Setup(model => model.SelectedEnvironment).Returns(sourceEnv.Object);
            _sourceView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            var explorerTreeItems = new List<IExplorerTreeItem>
            {
                new ExplorerItemViewModel(_serverMock.Object, It.IsAny<IExplorerTreeItem>(), model => It.IsAny<IExplorerItemViewModel>(),_shellVm.Object, new Mock<IPopupController>().Object)
            };
            _sourceView.SetupGet(model => model.SelectedItems).Returns(explorerTreeItems);
            var singleExplorerDeployViewModel = new SingleExplorerDeployViewModel(_destView.Object, _sourceView.Object, explorerTreeItems, _statsView.Object, _shellVm.Object, popupController.Object)
            {
                SourceConnectControlViewModel = { SelectedConnection = _serverMock.Object },
                DestinationConnectControlViewModel = { SelectedConnection = _differentServerMock.Object },
            };
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            PrivateObject privateObject = new PrivateObject(singleExplorerDeployViewModel);
            var canDeploy = privateObject.GetProperty("CanDeploy");
            //---------------Test Result -----------------------
            Assert.AreEqual(false, canDeploy);
            Assert.AreEqual(StringResources.DestinationPermission_Error, singleExplorerDeployViewModel.ErrorMessage);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void CanDeploy_GivenIsDeployingIsTrue_ShouldHaveFalse()
        {
            //---------------Set up test pack-------------------
            _updateRepositoryMock.SetupProperty(manager => manager.ServerSaved);
            _serverMock.SetupGet(it => it.UpdateRepository).Returns(_updateRepositoryMock.Object);
            _serverMock.SetupGet(it => it.IsConnected).Returns(true);
            _serverMock.Setup(server => server.Permissions).Returns(new Mock<List<IWindowsGroupPermission>>().Object);
            _serverMock.Setup(server => server.CanDeployFrom).Returns(true);
            _differentServerMock.SetupGet(it => it.IsConnected).Returns(true);
            _differentServerMock.Setup(server => server.Permissions).Returns(new Mock<List<IWindowsGroupPermission>>().Object);
            _differentServerMock.Setup(server => server.CanDeployTo).Returns(false);
            _serverMock.SetupGet(it => it.ResourceName).Returns("some text");
            _serverEnvironmentId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(_serverEnvironmentId);
            _shellVm.Setup(model => model.LocalhostServer).Returns(_serverMock.Object);
            var popupController = new Mock<IPopupController>();
            var connectControl = new Mock<IConnectControlViewModel>();
            connectControl.SetupAllProperties();
            var connectControl2 = new Mock<IConnectControlViewModel>();
            connectControl2.SetupAllProperties();
            var destEnv = new Mock<IEnvironmentViewModel>();
            destEnv.Setup(model => model.IsConnected).Returns(true);
            destEnv.Setup(model => model.Server).Returns(_differentServerMock.Object);
            _destView.Setup(model => model.SelectedEnvironment).Returns(destEnv.Object);
            _destView.Setup(model => model.ConnectControlViewModel).Returns(connectControl2.Object);
            var sourceEnv = new Mock<IEnvironmentViewModel>();
            sourceEnv.Setup(model => model.IsConnected).Returns(true);
            sourceEnv.Setup(model => model.Server).Returns(_serverMock.Object);
            _sourceView.Setup(model => model.SelectedEnvironment).Returns(sourceEnv.Object);
            _sourceView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            var explorerTreeItems = new List<IExplorerTreeItem>
            {
                new ExplorerItemViewModel(_serverMock.Object, It.IsAny<IExplorerTreeItem>(), model => It.IsAny<IExplorerItemViewModel>(),_shellVm.Object, new Mock<IPopupController>().Object)
            };
            _sourceView.SetupGet(model => model.SelectedItems).Returns(explorerTreeItems);
            var singleExplorerDeployViewModel = new SingleExplorerDeployViewModel(_destView.Object, _sourceView.Object, explorerTreeItems, _statsView.Object, _shellVm.Object, popupController.Object)
            {
                SourceConnectControlViewModel = { SelectedConnection = _serverMock.Object },
                DestinationConnectControlViewModel = { SelectedConnection = _differentServerMock.Object },
            };
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            PrivateObject privateObject = new PrivateObject(singleExplorerDeployViewModel);
            typeof(SingleExplorerDeployViewModel).GetProperty("IsDeploying").SetValue(privateObject.Target, true);
            var canDeploy = privateObject.GetProperty("CanDeploy");
            //---------------Test Result -----------------------
            Assert.AreEqual(false, canDeploy);
        }


        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void CanDeploy_GivenAllValidRequirements_ShouldHaveTrue()
        {
            //---------------Set up test pack-------------------
            _updateRepositoryMock.SetupProperty(manager => manager.ServerSaved);
            _serverMock.SetupGet(it => it.UpdateRepository).Returns(_updateRepositoryMock.Object);
            _serverMock.SetupGet(it => it.IsConnected).Returns(true);
            _serverMock.Setup(server => server.Permissions).Returns(new Mock<List<IWindowsGroupPermission>>().Object);
            _serverMock.Setup(server => server.CanDeployFrom).Returns(true);
            _differentServerMock.SetupGet(it => it.IsConnected).Returns(true);
            _differentServerMock.Setup(server => server.Permissions).Returns(new Mock<List<IWindowsGroupPermission>>().Object);
            _differentServerMock.Setup(server => server.CanDeployTo).Returns(true);
            _serverMock.SetupGet(it => it.ResourceName).Returns("some text");
            _serverEnvironmentId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(_serverEnvironmentId);
            _shellVm.Setup(model => model.LocalhostServer).Returns(_serverMock.Object);
            var popupController = new Mock<IPopupController>();
            var connectControl = new Mock<IConnectControlViewModel>();
            connectControl.SetupAllProperties();
            var connectControl2 = new Mock<IConnectControlViewModel>();
            connectControl2.SetupAllProperties();
            var destEnv = new Mock<IEnvironmentViewModel>();
            destEnv.Setup(model => model.IsConnected).Returns(true);
            destEnv.Setup(model => model.Server).Returns(_differentServerMock.Object);
            _destView.Setup(model => model.SelectedEnvironment).Returns(destEnv.Object);
            _destView.Setup(model => model.ConnectControlViewModel).Returns(connectControl2.Object);
            var sourceEnv = new Mock<IEnvironmentViewModel>();
            sourceEnv.Setup(model => model.IsConnected).Returns(true);
            sourceEnv.Setup(model => model.Server).Returns(_serverMock.Object);
            _sourceView.Setup(model => model.SelectedEnvironment).Returns(sourceEnv.Object);
            _sourceView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            var explorerTreeItems = new List<IExplorerTreeItem>
            {
                new ExplorerItemViewModel(_serverMock.Object, It.IsAny<IExplorerTreeItem>(), model => It.IsAny<IExplorerItemViewModel>(),_shellVm.Object, new Mock<IPopupController>().Object)
            };
            _sourceView.SetupGet(model => model.SelectedItems).Returns(explorerTreeItems);
            var singleExplorerDeployViewModel = new SingleExplorerDeployViewModel(_destView.Object, _sourceView.Object, explorerTreeItems, _statsView.Object, _shellVm.Object, popupController.Object)
            {
                SourceConnectControlViewModel = { SelectedConnection = _serverMock.Object },
                DestinationConnectControlViewModel = { SelectedConnection = _differentServerMock.Object },
            };
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            PrivateObject privateObject = new PrivateObject(singleExplorerDeployViewModel);
            var canDeploy = privateObject.GetProperty("CanDeploy");
            //---------------Test Result -----------------------
            Assert.AreEqual(true, canDeploy);
            Assert.IsTrue(string.IsNullOrEmpty(singleExplorerDeployViewModel.ErrorMessage));
        }


        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void CanDeploy_GivenSelectedServerIsNull_ShouldHaveFalse()
        {
            //---------------Set up test pack-------------------
            _updateRepositoryMock.SetupProperty(manager => manager.ServerSaved);
            _serverMock.SetupGet(it => it.UpdateRepository).Returns(_updateRepositoryMock.Object);
            _serverMock.SetupGet(it => it.IsConnected).Returns(false);
            _serverMock.SetupGet(it => it.ResourceName).Returns("some text");
            _serverEnvironmentId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(_serverEnvironmentId);
            _shellVm.Setup(model => model.LocalhostServer).Returns(_serverMock.Object);
            var popupController = new Mock<IPopupController>();
            var connectControl = new Mock<IConnectControlViewModel>();
            connectControl.SetupAllProperties();
            _destView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            _sourceView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            _sourceView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            var singleExplorerDeployViewModel = new SingleExplorerDeployViewModel(_destView.Object, _sourceView.Object, new List<IExplorerTreeItem>(), _statsView.Object, _shellVm.Object, popupController.Object)
            {
                SourceConnectControlViewModel =
                {
                    SelectedConnection = null
                }
            };
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            PrivateObject privateObject = new PrivateObject(singleExplorerDeployViewModel);
            var canDeploy = privateObject.GetProperty("CanDeploy");
            //---------------Test Result -----------------------
            Assert.AreEqual(false, canDeploy);
            Assert.AreEqual(Resources.Languages.Core.DeploySourceNotConnected, singleExplorerDeployViewModel.ErrorMessage);
        }


        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void CanDeploy_GivenNoSelectedItem_ShouldHaveFalse()
        {
            //---------------Set up test pack-------------------
            _updateRepositoryMock.SetupProperty(manager => manager.ServerSaved);
            _serverMock.SetupGet(it => it.UpdateRepository).Returns(_updateRepositoryMock.Object);
            _serverMock.SetupGet(it => it.ResourceName).Returns("some text");
            _serverEnvironmentId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(_serverEnvironmentId);
            _serverMock.Setup(server => server.IsConnected).Returns(true);
            _shellVm.Setup(model => model.LocalhostServer).Returns(_serverMock.Object);
            var popupController = new Mock<IPopupController>();
            var connectControl = new Mock<IConnectControlViewModel>();
            connectControl.SetupAllProperties();
            _destView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            _sourceView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            _sourceView.SetupGet(model => model.SelectedEnvironment).Returns(It.IsAny<IEnvironmentViewModel>());
            var singleExplorerDeployViewModel = new SingleExplorerDeployViewModel(_destView.Object, _sourceView.Object, new List<IExplorerTreeItem>(), _statsView.Object, _shellVm.Object, popupController.Object)
            {
                Destination = new DeployDestinationViewModel(_shellVm.Object, new EventAggregator()),
                Source = _sourceView.Object,
                SourceConnectControlViewModel = { SelectedConnection = _serverMock.Object }
            };
            //---------------Assert Precondition----------------
            //---------------Test Result -----------------------
            PrivateObject privateObject = new PrivateObject(singleExplorerDeployViewModel);
            var canDeploy = privateObject.GetProperty("CanDeploy");
            //---------------Test Result -----------------------
            Assert.AreEqual(false, canDeploy);
            Assert.AreEqual(Resources.Languages.Core.DeploySourceNotConnected, singleExplorerDeployViewModel.ErrorMessage);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void CanDeploy_GivenDestinationIsNotConnected_ShouldHaveFalse()
        {
            //---------------Set up test pack-------------------
            _updateRepositoryMock.SetupProperty(manager => manager.ServerSaved);
            _serverMock.SetupGet(it => it.UpdateRepository).Returns(_updateRepositoryMock.Object);
            _serverMock.SetupGet(it => it.ResourceName).Returns("some text");
            _serverEnvironmentId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(_serverEnvironmentId);
            _serverMock.Setup(server => server.IsConnected).Returns(true);
            _shellVm.Setup(model => model.LocalhostServer).Returns(_serverMock.Object);
            var popupController = new Mock<IPopupController>();
            var connectControl = new Mock<IConnectControlViewModel>();
            var differentConnectControl = new Mock<IConnectControlViewModel>();
            connectControl.SetupAllProperties();
            differentConnectControl.SetupAllProperties();
            _differentServerMock = new Mock<IServer>();
            differentConnectControl.Setup(model => model.SelectedConnection).Returns(_differentServerMock.Object);
            _destView.Setup(model => model.ConnectControlViewModel).Returns(differentConnectControl.Object);
            _destView.Setup(model => model.SelectedEnvironment).Returns(It.IsAny<IEnvironmentViewModel>());
            _sourceView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            _sourceView.Setup(model => model.SelectedEnvironment).Returns(new Mock<IEnvironmentViewModel>().Object);
            _sourceView.Setup(model => model.SelectedEnvironment.IsConnected).Returns(true);
            var singleExplorerDeployViewModel = new SingleExplorerDeployViewModel(_destView.Object, _sourceView.Object, new List<IExplorerTreeItem>(), _statsView.Object, _shellVm.Object, popupController.Object)
            {
                Destination = new DeployDestinationViewModel(_shellVm.Object, _eventAggregatorMock.Object)
            };
            //---------------Assert Precondition----------------
            //---------------Test Result -----------------------
            PrivateObject privateObject = new PrivateObject(singleExplorerDeployViewModel);
            var canDeploy = privateObject.GetProperty("CanDeploy");
            //---------------Test Result -----------------------
            Assert.AreEqual(false, canDeploy);
            Assert.AreEqual(Resources.Languages.Core.DeployDestinationNotConnected, singleExplorerDeployViewModel.ErrorMessage);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void DestinationServerStateChanged_GivenDestinationIsDisconnected_Should()
        {
            //---------------Set up test pack-------------------
            _updateRepositoryMock.SetupProperty(manager => manager.ServerSaved);
            _serverMock.SetupGet(it => it.UpdateRepository).Returns(_updateRepositoryMock.Object);
            _serverMock.SetupGet(it => it.ResourceName).Returns("some text");
            _serverEnvironmentId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(_serverEnvironmentId);
            _serverMock.Setup(server => server.IsConnected).Returns(true);
            _shellVm.Setup(model => model.LocalhostServer).Returns(_serverMock.Object);
            var popupController = new Mock<IPopupController>();
            var connectControl = new Mock<IConnectControlViewModel>();
            connectControl.SetupAllProperties();
            _destView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            _destView.Setup(model => model.SelectedEnvironment).Returns(It.IsAny<IEnvironmentViewModel>());
            _sourceView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            _sourceView.Setup(model => model.SelectedEnvironment).Returns(new Mock<IEnvironmentViewModel>().Object);
            _sourceView.Setup(model => model.SelectedEnvironment.IsConnected).Returns(true);
            var singleExplorerDeployViewModel = new SingleExplorerDeployViewModel(_destView.Object, _sourceView.Object, new List<IExplorerTreeItem>(), _statsView.Object, _shellVm.Object, popupController.Object);
            //---------------Assert Precondition----------------
            //---------------Test Result -----------------------
            PrivateObject privateObject = new PrivateObject(singleExplorerDeployViewModel);
            privateObject.Invoke("DestinationServerStateChanged", null, _serverMock.Object);
            //---------------Test Result -----------------------
        }


        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void DestinationServerStateChanged_GivenDestinationConnected_Should()
        {
            //---------------Set up test pack-------------------
            _updateRepositoryMock.SetupProperty(manager => manager.ServerSaved);
            _serverMock.SetupGet(it => it.UpdateRepository).Returns(_updateRepositoryMock.Object);
            _serverMock.SetupGet(it => it.ResourceName).Returns("some text");
            _serverEnvironmentId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(_serverEnvironmentId);
            _serverMock.Setup(server => server.IsConnected).Returns(true);
            _shellVm.Setup(model => model.LocalhostServer).Returns(_serverMock.Object);
            var popupController = new Mock<IPopupController>();
            var connectControl = new Mock<IConnectControlViewModel>();
            connectControl.SetupAllProperties();
            _destView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            _destView.Setup(model => model.SelectedEnvironment).Returns(It.IsAny<IEnvironmentViewModel>());
            _sourceView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            _sourceView.Setup(model => model.SelectedEnvironment).Returns(new Mock<IEnvironmentViewModel>().Object);
            _sourceView.SetupGet(model => model.SelectedItems).Returns(new List<IExplorerTreeItem>
            {
                new ExplorerItemViewModel(_serverMock.Object, It.IsAny<IExplorerTreeItem>(), model => It.IsAny<IExplorerItemViewModel>(),_shellVm.Object, new Mock<IPopupController>().Object)
            });
            _sourceView.Setup(model => model.SelectedEnvironment.IsConnected).Returns(true);
            _destView.Setup(model => model.ConnectControlViewModel.IsConnected).Returns(true);
            var singleExplorerDeployViewModel = new SingleExplorerDeployViewModel(_destView.Object, _sourceView.Object, new List<IExplorerTreeItem>(), _statsView.Object, _shellVm.Object, popupController.Object);
            //---------------Assert Precondition----------------
            //---------------Test Result -----------------------
            PrivateObject privateObject = new PrivateObject(singleExplorerDeployViewModel);
            privateObject.Invoke("DestinationServerStateChanged", null, _serverMock.Object);
            //---------------Test Result -----------------------
            Assert.IsFalse(singleExplorerDeployViewModel.ShowNewItemsList);
            Assert.IsFalse(singleExplorerDeployViewModel.ShowConflicts);
            Assert.IsFalse(singleExplorerDeployViewModel.ShowConflictItemsList);
            Assert.IsTrue(string.IsNullOrEmpty(singleExplorerDeployViewModel.ServicesCount));
            Assert.IsTrue(string.IsNullOrEmpty(singleExplorerDeployViewModel.OverridesCount));
            Assert.IsNotNull(singleExplorerDeployViewModel.OverridesViewCommand);
            Assert.IsNotNull(singleExplorerDeployViewModel.NewResourcesViewCommand);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void DefaultValues_GivenNewInstance_Should()
        {
            //---------------Set up test pack-------------------
            _statsView.Setup(model => model.New).Returns(new List<IExplorerTreeItem>
            {
                new Mock<IEnvironmentViewModel>().Object
            });
            _updateRepositoryMock.SetupProperty(manager => manager.ServerSaved);
            _serverMock.SetupGet(it => it.UpdateRepository).Returns(_updateRepositoryMock.Object);
            _serverMock.SetupGet(it => it.ResourceName).Returns("some text");
            _serverEnvironmentId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(_serverEnvironmentId);
            _serverMock.Setup(server => server.IsConnected).Returns(true);
            _shellVm.Setup(model => model.LocalhostServer).Returns(_serverMock.Object);
            var popupController = new Mock<IPopupController>();
            var connectControl = new Mock<IConnectControlViewModel>();
            connectControl.SetupAllProperties();
            _destView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            _destView.Setup(model => model.SelectedEnvironment).Returns(It.IsAny<IEnvironmentViewModel>());
            _sourceView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            _sourceView.Setup(model => model.SelectedEnvironment).Returns(new Mock<IEnvironmentViewModel>().Object);
            _sourceView.SetupGet(model => model.SelectedItems).Returns(new List<IExplorerTreeItem>
            {
                new ExplorerItemViewModel(_serverMock.Object, It.IsAny<IExplorerTreeItem>(), model => It.IsAny<IExplorerItemViewModel>(),_shellVm.Object, new Mock<IPopupController>().Object)
            });
            var singleExplorerDeployViewModel = new SingleExplorerDeployViewModel(_destView.Object, _sourceView.Object, new List<IExplorerTreeItem>(), _statsView.Object, _shellVm.Object, popupController.Object);
            //---------------Test Result -----------------------
            Assert.IsFalse(singleExplorerDeployViewModel.ShowNewItemsList);
            Assert.IsFalse(singleExplorerDeployViewModel.ShowConflicts);
            Assert.IsFalse(singleExplorerDeployViewModel.ShowConflictItemsList);
            Assert.IsTrue(string.IsNullOrEmpty(singleExplorerDeployViewModel.ServicesCount));
            Assert.IsTrue(string.IsNullOrEmpty(singleExplorerDeployViewModel.OverridesCount));
            Assert.IsNotNull(singleExplorerDeployViewModel.OverridesViewCommand);
            Assert.IsNotNull(singleExplorerDeployViewModel.NewResourcesViewCommand);
        }


        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void DeployConflics_GivenDifferntServerVersions_ShouldHaveIsDeployingFalse()
        {
            //---------------Set up test pack-------------------
            _differentServerMock = new Mock<IServer>();
            _updateRepositoryMock.SetupProperty(manager => manager.ServerSaved);
            _serverMock.SetupGet(it => it.UpdateRepository).Returns(_updateRepositoryMock.Object);
            _serverMock.SetupGet(it => it.ResourceName).Returns("some text");
            _serverEnvironmentId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(_serverEnvironmentId);
            _serverMock.Setup(server => server.IsConnected).Returns(true);
            _serverMock.Setup(server => server.VersionInfo.VersionNumber).Returns("2.0.0.0");
            _shellVm.Setup(model => model.LocalhostServer).Returns(_serverMock.Object);
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeployServerMinVersionConflict(It.IsAny<string>(), It.IsAny<string>())).Returns(MessageBoxResult.Cancel);
            var connectControl = new Mock<IConnectControlViewModel>();
            connectControl.SetupAllProperties();
            _destView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            _destView.Setup(model => model.SelectedEnvironment).Returns(It.IsAny<IEnvironmentViewModel>());
            _destView.Setup(model => model.SelectedServer).Returns(_differentServerMock.Object);
            _destView.Setup(model => model.MinSupportedVersion).Returns(new Version("2.0.0.0"));
            _sourceView.Setup(model => model.ServerVersion).Returns(new Version("1.0.0.0"));
            _sourceView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            _sourceView.Setup(model => model.SelectedEnvironment).Returns(new Mock<IEnvironmentViewModel>().Object);
            _sourceView.SetupGet(model => model.SelectedItems).Returns(new List<IExplorerTreeItem>
            {
                new ExplorerItemViewModel(_serverMock.Object, It.IsAny<IExplorerTreeItem>(), model => It.IsAny<IExplorerItemViewModel>(),_shellVm.Object, new Mock<IPopupController>().Object)
            });
            var singleExplorerDeployViewModel = new SingleExplorerDeployViewModel(_destView.Object, _sourceView.Object, new List<IExplorerTreeItem>(), _statsView.Object, _shellVm.Object, popupController.Object);
            var privateObject = new PrivateObject(singleExplorerDeployViewModel);

            singleExplorerDeployViewModel.DestinationConnectControlViewModel.SelectedConnection = _differentServerMock.Object;
            singleExplorerDeployViewModel.SourceConnectControlViewModel.SelectedConnection = _serverMock.Object;
            //---------------Test Result -----------------------
            privateObject.Invoke("CheckVersionConflict");
            Assert.IsFalse(singleExplorerDeployViewModel.IsDeploying);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void IsDeploying_GivenSameServerVersions_ShouldHaveIsDeployingTrue()
        {
            //---------------Set up test pack-------------------
            _differentServerMock = new Mock<IServer>();
            _updateRepositoryMock.SetupProperty(manager => manager.ServerSaved);
            _serverMock.SetupGet(it => it.UpdateRepository).Returns(_updateRepositoryMock.Object);
            _serverMock.SetupGet(it => it.ResourceName).Returns("some text");
            _serverEnvironmentId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(_serverEnvironmentId);
            _serverMock.Setup(server => server.IsConnected).Returns(true);
            _serverMock.Setup(server => server.VersionInfo.VersionNumber).Returns("1.0.0.0");
            _shellVm.Setup(model => model.LocalhostServer).Returns(_serverMock.Object);
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeployServerMinVersionConflict(It.IsAny<string>(), It.IsAny<string>())).Returns(MessageBoxResult.Cancel);
            var connectControl = new Mock<IConnectControlViewModel>();
            connectControl.SetupAllProperties();
            _destView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            _destView.Setup(model => model.SelectedEnvironment).Returns(It.IsAny<IEnvironmentViewModel>());
            _destView.Setup(model => model.SelectedServer).Returns(_differentServerMock.Object);
            _destView.Setup(model => model.MinSupportedVersion).Returns(new Version("1.0.0.0"));
            _sourceView.Setup(model => model.ServerVersion).Returns(new Version("1.0.0.0"));
            _sourceView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            _sourceView.Setup(model => model.SelectedEnvironment).Returns(new Mock<IEnvironmentViewModel>().Object);
            _sourceView.SetupGet(model => model.SelectedItems).Returns(new List<IExplorerTreeItem>
            {
                new ExplorerItemViewModel(_serverMock.Object, It.IsAny<IExplorerTreeItem>(), model => It.IsAny<IExplorerItemViewModel>(),_shellVm.Object, new Mock<IPopupController>().Object)
            });
            var singleExplorerDeployViewModel = new SingleExplorerDeployViewModel(_destView.Object, _sourceView.Object, new List<IExplorerTreeItem>(), _statsView.Object, _shellVm.Object, popupController.Object);
            var privateObject = new PrivateObject(singleExplorerDeployViewModel);
            typeof(SingleExplorerDeployViewModel).GetProperty("IsDeploying").SetValue(privateObject.Target, true);
            singleExplorerDeployViewModel.DestinationConnectControlViewModel.SelectedConnection = _differentServerMock.Object;
            singleExplorerDeployViewModel.SourceConnectControlViewModel.SelectedConnection = _serverMock.Object;
            //---------------Test Result -----------------------
            privateObject.Invoke("CheckVersionConflict");
            Assert.IsTrue(singleExplorerDeployViewModel.IsDeploying);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void IsDeploying_GivenSourceServerIsOldVersionAndPopupCancelClick_ShouldHaveIsDeployingTrue()
        {
            //---------------Set up test pack-------------------
            _differentServerMock = new Mock<IServer>();
            _updateRepositoryMock.SetupProperty(manager => manager.ServerSaved);
            _serverMock.SetupGet(it => it.UpdateRepository).Returns(_updateRepositoryMock.Object);
            _serverMock.SetupGet(it => it.ResourceName).Returns("some text");
            _serverEnvironmentId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(_serverEnvironmentId);
            _serverMock.Setup(server => server.IsConnected).Returns(true);
            _serverMock.Setup(server => server.VersionInfo.VersionNumber).Returns("1.0.0.0");
            _shellVm.Setup(model => model.LocalhostServer).Returns(_serverMock.Object);
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeployServerMinVersionConflict(It.IsAny<string>(), It.IsAny<string>())).Returns(MessageBoxResult.OK);
            var connectControl = new Mock<IConnectControlViewModel>();
            connectControl.SetupAllProperties();
            _destView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            _destView.Setup(model => model.SelectedEnvironment).Returns(It.IsAny<IEnvironmentViewModel>());
            _destView.Setup(model => model.SelectedServer).Returns(_differentServerMock.Object);
            _destView.Setup(model => model.MinSupportedVersion).Returns(new Version("2.0.0.0"));
            _sourceView.Setup(model => model.ServerVersion).Returns(new Version("1.0.0.0"));
            _sourceView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            _sourceView.Setup(model => model.SelectedEnvironment).Returns(new Mock<IEnvironmentViewModel>().Object);
            _sourceView.SetupGet(model => model.SelectedItems).Returns(new List<IExplorerTreeItem>
            {
                new ExplorerItemViewModel(_serverMock.Object, It.IsAny<IExplorerTreeItem>(), model => It.IsAny<IExplorerItemViewModel>(),_shellVm.Object, new Mock<IPopupController>().Object)
            });
            var singleExplorerDeployViewModel = new SingleExplorerDeployViewModel(_destView.Object, _sourceView.Object, new List<IExplorerTreeItem>(), _statsView.Object, _shellVm.Object, popupController.Object);
            var privateObject = new PrivateObject(singleExplorerDeployViewModel);
            typeof(SingleExplorerDeployViewModel).GetProperty("IsDeploying").SetValue(privateObject.Target, true);
            singleExplorerDeployViewModel.DestinationConnectControlViewModel.SelectedConnection = _differentServerMock.Object;
            singleExplorerDeployViewModel.SourceConnectControlViewModel.SelectedConnection = _serverMock.Object;
            //---------------Test Result -----------------------
            privateObject.Invoke("CheckVersionConflict");
            Assert.IsTrue(singleExplorerDeployViewModel.IsDeploying);
        }


        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void DeployConflics_GivenDestinationHasOldVersionServerVersions_ShouldHaveIsDeployingFalse()
        {
            //---------------Set up test pack-------------------
            _differentServerMock = new Mock<IServer>();
            _updateRepositoryMock.SetupProperty(manager => manager.ServerSaved);
            _serverMock.SetupGet(it => it.UpdateRepository).Returns(_updateRepositoryMock.Object);
            _serverMock.SetupGet(it => it.ResourceName).Returns("some text");
            _serverEnvironmentId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(_serverEnvironmentId);
            _serverMock.Setup(server => server.IsConnected).Returns(true);
            _shellVm.Setup(model => model.LocalhostServer).Returns(_serverMock.Object);
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeployServerMinVersionConflict(It.IsAny<string>(), It.IsAny<string>())).Returns(MessageBoxResult.Cancel);
            var connectControl = new Mock<IConnectControlViewModel>();
            connectControl.SetupAllProperties();
            _destView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            _destView.Setup(model => model.SelectedEnvironment).Returns(It.IsAny<IEnvironmentViewModel>());
            _destView.Setup(model => model.SelectedServer).Returns(_differentServerMock.Object);
            _destView.Setup(model => model.ServerVersion).Returns(new Version("1.0.0.0"));
            _sourceView.Setup(model => model.ServerVersion).Returns(new Version("2.0.0.0"));
            popupController.Setup(controller => controller.ShowDeployServerVersionConflict(It.IsAny<string>(), It.IsAny<string>())).Returns(MessageBoxResult.Cancel);
            _sourceView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            _sourceView.Setup(model => model.SelectedEnvironment).Returns(new Mock<IEnvironmentViewModel>().Object);
            _sourceView.SetupGet(model => model.SelectedItems).Returns(new List<IExplorerTreeItem>
            {
                new ExplorerItemViewModel(_serverMock.Object, It.IsAny<IExplorerTreeItem>(), model => It.IsAny<IExplorerItemViewModel>(),_shellVm.Object, new Mock<IPopupController>().Object)
            });
            var singleExplorerDeployViewModel = new SingleExplorerDeployViewModel(_destView.Object, _sourceView.Object, new List<IExplorerTreeItem>(), _statsView.Object, _shellVm.Object, popupController.Object);
            var privateObject = new PrivateObject(singleExplorerDeployViewModel);
            singleExplorerDeployViewModel.DestinationConnectControlViewModel.SelectedConnection = _differentServerMock.Object;
            singleExplorerDeployViewModel.SourceConnectControlViewModel.SelectedConnection = _serverMock.Object;
            //---------------Test Result -----------------------
            privateObject.Invoke("CheckVersionConflict");
            Assert.IsFalse(singleExplorerDeployViewModel.IsDeploying);
            popupController.Verify(model => model.ShowDeployServerVersionConflict(It.IsAny<string>(), It.IsAny<string>()));
        }


        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void StatsViewModel_GivenNewInstance_ShouldNotBeNull()
        {
            //---------------Set up test pack-------------------
            _updateRepositoryMock.SetupProperty(manager => manager.ServerSaved);
            _serverMock.SetupGet(it => it.UpdateRepository).Returns(_updateRepositoryMock.Object);
            _serverMock.SetupGet(it => it.ResourceName).Returns("some text");
            _serverEnvironmentId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(_serverEnvironmentId);
            _serverMock.Setup(server => server.IsConnected).Returns(true);
            _shellVm.Setup(model => model.LocalhostServer).Returns(_serverMock.Object);
            var popupController = new Mock<IPopupController>();
            var connectControl = new Mock<IConnectControlViewModel>();
            connectControl.SetupAllProperties();
            _destView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            _destView.Setup(model => model.SelectedEnvironment).Returns(It.IsAny<IEnvironmentViewModel>());
            _sourceView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            _sourceView.Setup(model => model.SelectedEnvironment).Returns(new Mock<IEnvironmentViewModel>().Object);
            _sourceView.Setup(model => model.SelectedEnvironment.IsConnected).Returns(true);
            var singleExplorerDeployViewModel = new SingleExplorerDeployViewModel(_destView.Object, _sourceView.Object, new List<IExplorerTreeItem>(), _statsView.Object, _shellVm.Object, popupController.Object);
            //---------------Assert Precondition----------------
            //---------------Test Result -----------------------
            PrivateObject privateObject = new PrivateObject(singleExplorerDeployViewModel);
            privateObject.Invoke("DestinationServerStateChanged", new object[] { null, null });
            //---------------Test Result -----------------------
            Assert.IsNotNull(singleExplorerDeployViewModel.StatsViewModel);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void ConflictNewResourceText_GivenViewOverrides_ShouldHaveText()
        {
            //---------------Set up test pack-------------------
            _updateRepositoryMock.SetupProperty(manager => manager.ServerSaved);
            _serverMock.SetupGet(it => it.UpdateRepository).Returns(_updateRepositoryMock.Object);
            _serverMock.SetupGet(it => it.ResourceName).Returns("some text");
            _serverEnvironmentId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(_serverEnvironmentId);
            _shellVm.Setup(model => model.LocalhostServer).Returns(_serverMock.Object);
            var popupController = new Mock<IPopupController>();
            var connectControl = new Mock<IConnectControlViewModel>();
            connectControl.SetupAllProperties();
            _destView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            _sourceView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            var singleExplorerDeployViewModel = new SingleExplorerDeployViewModel(_destView.Object, _sourceView.Object, new List<IExplorerTreeItem>(), _statsView.Object, _shellVm.Object, popupController.Object);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            PrivateObject privateObject = new PrivateObject(singleExplorerDeployViewModel);
            privateObject.Invoke("ViewOverrides");
            //---------------Test Result -----------------------
            Assert.IsFalse(string.IsNullOrEmpty(singleExplorerDeployViewModel.ConflictNewResourceText));
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void ConflictNewResourceText_GivenViewNewResources_ShouldHaveText()
        {
            //---------------Set up test pack-------------------
            _updateRepositoryMock.SetupProperty(manager => manager.ServerSaved);
            _serverMock.SetupGet(it => it.UpdateRepository).Returns(_updateRepositoryMock.Object);
            _serverMock.SetupGet(it => it.ResourceName).Returns("some text");
            _serverEnvironmentId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(_serverEnvironmentId);
            _shellVm.Setup(model => model.LocalhostServer).Returns(_serverMock.Object);
            var popupController = new Mock<IPopupController>();
            var connectControl = new Mock<IConnectControlViewModel>();
            connectControl.SetupAllProperties();
            _destView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            _sourceView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            var singleExplorerDeployViewModel = new SingleExplorerDeployViewModel(_destView.Object, _sourceView.Object, new List<IExplorerTreeItem>(), _statsView.Object, _shellVm.Object, popupController.Object);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            PrivateObject privateObject = new PrivateObject(singleExplorerDeployViewModel);
            privateObject.Invoke("ViewNewResources");
            //---------------Test Result -----------------------
            Assert.IsFalse(string.IsNullOrEmpty(singleExplorerDeployViewModel.ConflictNewResourceText));
            Assert.AreEqual("List of New Resources", singleExplorerDeployViewModel.ConflictNewResourceText);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void UpdateHelpDescriptor_GivenSomeMessage_ShouldHaveSomeMessage()
        {
            //---------------Set up test pack-------------------
            _updateRepositoryMock.SetupProperty(manager => manager.ServerSaved);
            _serverMock.SetupGet(it => it.UpdateRepository).Returns(_updateRepositoryMock.Object);
            _serverMock.SetupGet(it => it.ResourceName).Returns("some text");
            _serverEnvironmentId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(_serverEnvironmentId);
            _shellVm.Setup(model => model.LocalhostServer).Returns(_serverMock.Object);
            var popupController = new Mock<IPopupController>();
            var connectControl = new Mock<IConnectControlViewModel>();
            connectControl.SetupAllProperties();
            _destView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            _sourceView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            var singleExplorerDeployViewModel = new SingleExplorerDeployViewModel(_destView.Object, _sourceView.Object, new List<IExplorerTreeItem>(), _statsView.Object, _shellVm.Object, popupController.Object);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            singleExplorerDeployViewModel.UpdateHelpDescriptor("SomeMessage");
            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void DeploySuccessMessage_GivenSomeError_ShouldHaveEmptyText()
        {
            //---------------Set up test pack-------------------
            _updateRepositoryMock.SetupProperty(manager => manager.ServerSaved);
            _serverMock.SetupGet(it => it.UpdateRepository).Returns(_updateRepositoryMock.Object);
            _serverMock.SetupGet(it => it.ResourceName).Returns("some text");
            _serverEnvironmentId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(_serverEnvironmentId);
            _shellVm.Setup(model => model.LocalhostServer).Returns(_serverMock.Object);
            var popupController = new Mock<IPopupController>();
            var connectControl = new Mock<IConnectControlViewModel>();
            connectControl.SetupAllProperties();
            _destView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            _sourceView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            var singleExplorerDeployViewModel = new SingleExplorerDeployViewModel(_destView.Object, _sourceView.Object, new List<IExplorerTreeItem>(), _statsView.Object, _shellVm.Object, popupController.Object)
            {
                DeploySuccessMessage = "Successful",
                ErrorMessage = "some error"
            };
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsTrue(string.IsNullOrEmpty(singleExplorerDeployViewModel.DeploySuccessMessage));
            Assert.IsFalse(singleExplorerDeployViewModel.DeploySuccessfull);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void NewResourceCount_GivenEmptyStateItems_Should0()
        {
            //---------------Set up test pack-------------------
            _statsView.Setup(model => model.NewResources).Returns(5);
            _updateRepositoryMock.SetupProperty(manager => manager.ServerSaved);
            _serverMock.SetupGet(it => it.UpdateRepository).Returns(_updateRepositoryMock.Object);
            _serverMock.SetupGet(it => it.ResourceName).Returns("some text");
            _serverEnvironmentId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(_serverEnvironmentId);
            _shellVm.Setup(model => model.LocalhostServer).Returns(_serverMock.Object);
            var popupController = new Mock<IPopupController>();
            var connectControl = new Mock<IConnectControlViewModel>();
            connectControl.SetupAllProperties();
            _destView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            _sourceView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            var singleExplorerDeployViewModel = new SingleExplorerDeployViewModel(_destView.Object, _sourceView.Object, new List<IExplorerTreeItem>(), _statsView.Object, _shellVm.Object, popupController.Object) { NewResourcesCount = 0.ToString() };
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsFalse(string.IsNullOrEmpty(singleExplorerDeployViewModel.NewResourcesCount));
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Source_GivenNewSourceIsCreated_ShouldHaveNewSource()
        {
            //---------------Set up test pack-------------------
            _updateRepositoryMock.SetupProperty(manager => manager.ServerSaved);
            _serverMock.SetupGet(it => it.UpdateRepository).Returns(_updateRepositoryMock.Object);
            _serverMock.SetupGet(it => it.ResourceName).Returns("some text");
            _serverEnvironmentId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(_serverEnvironmentId);
            _shellVm.Setup(model => model.LocalhostServer).Returns(_serverMock.Object);
            var popupController = new Mock<IPopupController>();
            var connectControl = new Mock<IConnectControlViewModel>();
            connectControl.SetupAllProperties();
            _destView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            _sourceView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            var singleExplorerDeployViewModel = new SingleExplorerDeployViewModel(_destView.Object, _sourceView.Object, new List<IExplorerTreeItem>(), _statsView.Object, _shellVm.Object, popupController.Object);
            //---------------Assert Precondition----------------
            var deploySourceExplorerViewModelBefore = singleExplorerDeployViewModel.Source;
            Assert.IsNotNull(deploySourceExplorerViewModelBefore);
            //---------------Execute Test ----------------------
            singleExplorerDeployViewModel.Source = new DeploySourceExplorerViewModel(_shellVm.Object, new EventAggregator(), new DeployStatsViewerViewModel(new Mock<IDeployDestinationExplorerViewModel>().Object));
            //---------------Test Result -----------------------
            Assert.IsFalse(Equals(deploySourceExplorerViewModelBefore, singleExplorerDeployViewModel.Source));
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void NewItems_GivenCalculateAction_AreEqualTo_StatViewNew()
        {
            //---------------Set up test pack-------------------            
            _updateRepositoryMock.SetupProperty(manager => manager.ServerSaved);
            _serverMock.SetupGet(it => it.UpdateRepository).Returns(_updateRepositoryMock.Object);
            _serverMock.SetupGet(it => it.ResourceName).Returns("some text");
            _serverEnvironmentId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(_serverEnvironmentId);
            _shellVm.Setup(model => model.LocalhostServer).Returns(_serverMock.Object);
            var popupController = new Mock<IPopupController>();
            var connectControl = new Mock<IConnectControlViewModel>();
            connectControl.SetupAllProperties();
            _destView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            _sourceView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            var explorerTreeItems = new List<IExplorerTreeItem>();
            _statsView.Setup(model => model.New).Returns(explorerTreeItems);
            var singleExplorerDeployViewModel = new SingleExplorerDeployViewModel(_destView.Object, _sourceView.Object, new List<IExplorerTreeItem>(), _statsView.Object, _shellVm.Object, popupController.Object);
            //---------------Assert Precondition----------------
            Assert.IsNull(singleExplorerDeployViewModel.NewItems);
            //---------------Execute Test ----------------------
            _statsView.Object.CalculateAction.Invoke();
            //---------------Test Result -----------------------
            Assert.AreEqual(singleExplorerDeployViewModel.NewItems, _statsView.Object.New);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void SourcesCount_GivenCalculateAction_AreEqualTo_StatViewSources()
        {
            //---------------Set up test pack-------------------            
            _updateRepositoryMock.SetupProperty(manager => manager.ServerSaved);
            _serverMock.SetupGet(it => it.UpdateRepository).Returns(_updateRepositoryMock.Object);
            _serverMock.SetupGet(it => it.ResourceName).Returns("some text");
            _serverEnvironmentId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(_serverEnvironmentId);
            _shellVm.Setup(model => model.LocalhostServer).Returns(_serverMock.Object);
            var popupController = new Mock<IPopupController>();
            var connectControl = new Mock<IConnectControlViewModel>();
            connectControl.SetupAllProperties();
            _destView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            _sourceView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            _statsView.Setup(model => model.Sources).Returns(25);
            var singleExplorerDeployViewModel = new SingleExplorerDeployViewModel(_destView.Object, _sourceView.Object, new List<IExplorerTreeItem>(), _statsView.Object, _shellVm.Object, popupController.Object);
            //---------------Assert Precondition----------------
            Assert.IsNull(singleExplorerDeployViewModel.SourcesCount);
            //---------------Execute Test ----------------------
            _statsView.Object.CalculateAction.Invoke();
            //---------------Test Result -----------------------
            Assert.AreEqual(singleExplorerDeployViewModel.SourcesCount, _statsView.Object.Sources.ToString());
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void ConflictRenameErrors_GivenCalculateAction_ShowsThePopupMessage()
        {
            //---------------Set up test pack-------------------            
            _updateRepositoryMock.SetupProperty(manager => manager.ServerSaved);
            _serverMock.SetupGet(it => it.UpdateRepository).Returns(_updateRepositoryMock.Object);
            _serverMock.SetupGet(it => it.ResourceName).Returns("some text");
            _serverEnvironmentId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(_serverEnvironmentId);
            _shellVm.Setup(model => model.LocalhostServer).Returns(_serverMock.Object);
            var popupController = new Mock<IPopupController>();
            var connectControl = new Mock<IConnectControlViewModel>();
            connectControl.SetupAllProperties();
            _destView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            _sourceView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            var explorerTreeItems = new List<IExplorerTreeItem>();
            _statsView.Setup(model => model.New).Returns(explorerTreeItems);
            _statsView.Setup(model => model.RenameErrors).Returns("Conflicts in resource names");
            var singleExplorerDeployViewModel = new SingleExplorerDeployViewModel(_destView.Object, _sourceView.Object, new List<IExplorerTreeItem>(), _statsView.Object, _shellVm.Object, popupController.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(singleExplorerDeployViewModel);
            //---------------Execute Test ----------------------
            _statsView.Object.CalculateAction.Invoke();
            //---------------Test Result -----------------------
            popupController.Verify(controller => controller.ShowDeployNameConflict("Conflicts in resource names"));
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void ConflictItems_GivenCalculateAction_AreEqualToStatViewConflictItems()
        {
            //---------------Set up test pack-------------------            
            _updateRepositoryMock.SetupProperty(manager => manager.ServerSaved);
            _serverMock.SetupGet(it => it.UpdateRepository).Returns(_updateRepositoryMock.Object);
            _serverMock.SetupGet(it => it.ResourceName).Returns("some text");
            _serverEnvironmentId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(_serverEnvironmentId);
            _shellVm.Setup(model => model.LocalhostServer).Returns(_serverMock.Object);
            var popupController = new Mock<IPopupController>();
            var connectControl = new Mock<IConnectControlViewModel>();
            connectControl.SetupAllProperties();
            _destView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            _sourceView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            var explorerTreeItems = new List<IExplorerTreeItem>();
            _statsView.Setup(model => model.New).Returns(explorerTreeItems);
            var conflicts = new List<Conflict>();
            _statsView.Setup(model => model.Conflicts).Returns(conflicts);
            var singleExplorerDeployViewModel = new SingleExplorerDeployViewModel(_destView.Object, _sourceView.Object, new List<IExplorerTreeItem>(), _statsView.Object, _shellVm.Object, popupController.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(singleExplorerDeployViewModel);
            //---------------Execute Test ----------------------
            _statsView.Object.CalculateAction.Invoke();
            //---------------Test Result -----------------------
            Assert.AreEqual(singleExplorerDeployViewModel.ConflictItems, _statsView.Object.Conflicts);
        }
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void UpdateServerCompareChanged_ShouldRecalculate_Items()
        {
            //---------------Set up test pack-------------------            
            _updateRepositoryMock.SetupProperty(manager => manager.ServerSaved);
            _serverMock.SetupGet(it => it.UpdateRepository).Returns(_updateRepositoryMock.Object);
            _serverMock.SetupGet(it => it.ResourceName).Returns("some text");
            _serverEnvironmentId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(_serverEnvironmentId);
            _shellVm.Setup(model => model.LocalhostServer).Returns(_serverMock.Object);
            var popupController = new Mock<IPopupController>();
            var connectControl = new Mock<IConnectControlViewModel>();
            connectControl.SetupAllProperties();
            _destView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            _sourceView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            var explorerTreeItems = new List<IExplorerTreeItem>();
            _statsView.Setup(model => model.New).Returns(explorerTreeItems);
            _statsView.Setup(model => model.Services).Returns(5);
            _statsView.Setup(model => model.Sources).Returns(2);
            _statsView.Setup(model => model.NewResources).Returns(3);
            _statsView.Setup(model => model.Overrides).Returns(1);
            var singleExplorerDeployViewModel = new SingleExplorerDeployViewModel(_destView.Object, _sourceView.Object, new List<IExplorerTreeItem>(), _statsView.Object, _shellVm.Object, popupController.Object);
            //---------------Assert Precondition----------------
            _statsView.Object.CalculateAction();
            Assert.IsNotNull(singleExplorerDeployViewModel);
            Assert.AreEqual("5", singleExplorerDeployViewModel.ServicesCount);
            Assert.AreEqual("2", singleExplorerDeployViewModel.SourcesCount);
            Assert.AreEqual("3", singleExplorerDeployViewModel.NewResourcesCount);
            Assert.AreEqual("1", singleExplorerDeployViewModel.OverridesCount);
            //---------------Execute Test ----------------------
            _statsView.Setup(model => model.Services).Returns(50);
            _statsView.Setup(model => model.Sources).Returns(20);
            _statsView.Setup(model => model.NewResources).Returns(30);
            _statsView.Setup(model => model.Overrides).Returns(10);
            PrivateObject privateObject = new PrivateObject(singleExplorerDeployViewModel);
            privateObject.Invoke("UpdateServerCompareChanged", new object[] { null, null });
            //---------------Test Result -----------------------
            Assert.IsFalse(singleExplorerDeployViewModel.ShowConflicts);
            Assert.AreEqual("50", singleExplorerDeployViewModel.ServicesCount);
            Assert.AreEqual("20", singleExplorerDeployViewModel.SourcesCount);
            Assert.AreEqual("30", singleExplorerDeployViewModel.NewResourcesCount);
            Assert.AreEqual("10", singleExplorerDeployViewModel.OverridesCount);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void SelectDependencies_GivenSourceWithDependencies_ShouldHaveItemsSelected()
        {
            //---------------Set up test pack-------------------            
            _updateRepositoryMock.SetupProperty(manager => manager.ServerSaved);
            _serverMock.SetupGet(it => it.UpdateRepository).Returns(_updateRepositoryMock.Object);
            _serverMock.SetupGet(it => it.ResourceName).Returns("some text");
            _serverEnvironmentId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(_serverEnvironmentId);
            _shellVm.Setup(model => model.LocalhostServer).Returns(_serverMock.Object);
            var popupController = new Mock<IPopupController>();
            var connectControl = new Mock<IConnectControlViewModel>();
            connectControl.SetupAllProperties();
            var selectedEnv = new Mock<IEnvironmentViewModel>();
            var newServer = new Mock<IServer>();
            var explorerTreeItem1 = new Mock<IExplorerTreeItem>();
            Guid newGuid = Guid.NewGuid();
            explorerTreeItem1.Setup(item => item.ResourceId).Returns(newGuid);
            explorerTreeItem1.Setup(item => item.AllowResourceCheck).Returns(true);


            var explorerTreeItems = new List<IExplorerTreeItem>
            {
                explorerTreeItem1.Object
            };
            var query = new Mock<IQueryManager>();
            query.Setup(manager => manager.FetchDependenciesOnList(It.IsAny<IEnumerable<Guid>>())).Returns(explorerTreeItems.Select(item => item.ResourceId).ToList());
            newServer.Setup(server => server.QueryProxy).Returns(query.Object);
            selectedEnv.Setup(model => model.Server).Returns(newServer.Object);
            var explorerItemViewModels = new ObservableCollection<IExplorerItemViewModel>();
            var explorerItemViewModels2 = new ObservableCollection<IExplorerItemViewModel>();

            var mock = new Mock<IExplorerItemViewModel>();
            mock.SetupGet(model => model.ResourceId).Returns(newGuid);
            mock.Setup(model => model.UnfilteredChildren).Returns(explorerItemViewModels2);
            mock.SetupProperty(item => item.IsResourceChecked);
            explorerItemViewModels.Add(mock.Object);

            selectedEnv.Setup(model => model.UnfilteredChildren).Returns(explorerItemViewModels);
            _destView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            _sourceView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            _sourceView.Setup(model => model.SelectedEnvironment).Returns(selectedEnv.Object);
            _sourceView.Setup(model => model.SelectedItems).Returns(explorerTreeItems);
            var singleExplorerDeployViewModel = new SingleExplorerDeployViewModel(_destView.Object, _sourceView.Object, explorerTreeItems, _statsView.Object, _shellVm.Object, popupController.Object);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            PrivateObject privateObject = new PrivateObject(singleExplorerDeployViewModel);
            privateObject.Invoke("SelectDependencies");
            //---------------Test Result -----------------------
            mock.VerifySet(model => model.IsResourceChecked = true, Times.Once);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void DestinationConnectControlViewModel_GivenDestinationConnectControlViewModelIsSetToSameValue_ShouldReturn()
        {
            //---------------Set up test pack-------------------            
            _updateRepositoryMock.SetupProperty(manager => manager.ServerSaved);
            _serverMock.SetupGet(it => it.UpdateRepository).Returns(_updateRepositoryMock.Object);
            _serverMock.SetupGet(it => it.ResourceName).Returns("some text");
            _serverEnvironmentId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(_serverEnvironmentId);
            _shellVm.Setup(model => model.LocalhostServer).Returns(_serverMock.Object);
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeployResourceNameConflict(It.IsAny<string>())).Returns(MessageBoxResult.OK);
            var connectControl = new Mock<IConnectControlViewModel>();
            connectControl.SetupAllProperties();
            var explorerTreeItem1 = new Mock<IExplorerTreeItem>();
            explorerTreeItem1.Setup(item => item.ResourceId).Returns(Guid.NewGuid);
            explorerTreeItem1.Setup(model => model.ResourcePath).Returns("Somepath");
            var explorerTreeItems = new List<IExplorerTreeItem>
            {
                explorerTreeItem1.Object
            };
            _destView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            var explorerItemViewModels = new List<IExplorerItemViewModel>
            {
                new ExplorerItemViewModel(new Mock<IServer>().Object, new Mock<IExplorerTreeItem>().Object, a => { }, new Mock<IShellViewModel>().Object, new Mock<IPopupController>().Object) { ResourcePath = "Somepath" }
            };
            _destView.Setup(model => model.SelectedEnvironment.AsList()).Returns(explorerItemViewModels);
            _sourceView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            _sourceView.Setup(model => model.SelectedItems).Returns(explorerTreeItems);
            var singleExplorerDeployViewModel = new SingleExplorerDeployViewModel(_destView.Object, _sourceView.Object, explorerTreeItems, _statsView.Object, _shellVm.Object, popupController.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(singleExplorerDeployViewModel.DestinationConnectControlViewModel);
            Assert.IsNotNull(singleExplorerDeployViewModel.SourceConnectControlViewModel);
            //---------------Execute Test ----------------------
            PrivateObject privateObject = new PrivateObject(singleExplorerDeployViewModel);
            typeof(SingleExplorerDeployViewModel).GetProperty("DestinationConnectControlViewModel").SetValue(privateObject.Target, connectControl.Object);
            typeof(SingleExplorerDeployViewModel).GetProperty("SourceConnectControlViewModel").SetValue(privateObject.Target, connectControl.Object);
            //---------------Test Result -----------------------
            Assert.IsNotNull(singleExplorerDeployViewModel.DestinationConnectControlViewModel);
            Assert.IsNotNull(singleExplorerDeployViewModel.SourceConnectControlViewModel);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void CheckResourceNameConflict_GivenSourceWith1ItemAndDestinationWith1Item_ShouldSetIsDeployingToFalse()
        {
            //---------------Set up test pack-------------------            
            _updateRepositoryMock.SetupProperty(manager => manager.ServerSaved);
            _serverMock.SetupGet(it => it.UpdateRepository).Returns(_updateRepositoryMock.Object);
            _serverMock.SetupGet(it => it.ResourceName).Returns("some text");
            _serverEnvironmentId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(_serverEnvironmentId);
            _shellVm.Setup(model => model.LocalhostServer).Returns(_serverMock.Object);
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeployResourceNameConflict(It.IsAny<string>())).Returns(MessageBoxResult.OK);
            var connectControl = new Mock<IConnectControlViewModel>();
            connectControl.SetupAllProperties();
            var explorerTreeItem1 = new Mock<IExplorerTreeItem>();
            explorerTreeItem1.Setup(item => item.ResourceId).Returns(Guid.NewGuid);
            explorerTreeItem1.Setup(model => model.ResourcePath).Returns("Somepath");
            var explorerTreeItems = new List<IExplorerTreeItem>
            {
                explorerTreeItem1.Object
            };
            _destView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            var explorerItemViewModels = new List<IExplorerItemViewModel>
            {
                new ExplorerItemViewModel(new Mock<IServer>().Object, new Mock<IExplorerTreeItem>().Object, a => { }, new Mock<IShellViewModel>().Object, new Mock<IPopupController>().Object) { ResourcePath = "Somepath" }
            };
            _destView.Setup(model => model.SelectedEnvironment.AsList()).Returns(explorerItemViewModels);
            _sourceView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            _sourceView.Setup(model => model.SelectedItems).Returns(explorerTreeItems);
            var singleExplorerDeployViewModel = new SingleExplorerDeployViewModel(_destView.Object, _sourceView.Object, explorerTreeItems, _statsView.Object, _shellVm.Object, popupController.Object);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            PrivateObject privateObject = new PrivateObject(singleExplorerDeployViewModel);
            privateObject.Invoke("CheckResourceNameConflict");
            //---------------Test Result -----------------------
            Assert.IsFalse(singleExplorerDeployViewModel.IsDeploying);
            popupController.Verify(controller => controller.ShowDeployResourceNameConflict(It.IsAny<string>()));
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Deploy_ShouldSetIsDeployingToTrue()
        {
            //---------------Set up test pack-------------------
            _updateRepositoryMock.SetupProperty(manager => manager.ServerSaved);
            _serverMock.SetupGet(it => it.UpdateRepository).Returns(_updateRepositoryMock.Object);
            _serverMock.SetupGet(it => it.IsConnected).Returns(true);
            _serverMock.Setup(server => server.Permissions).Returns(new Mock<List<IWindowsGroupPermission>>().Object);
            _serverMock.Setup(server => server.CanDeployFrom).Returns(true);
            _differentServerMock.SetupGet(it => it.IsConnected).Returns(true);
            _differentServerMock.Setup(server => server.Permissions).Returns(new Mock<List<IWindowsGroupPermission>>().Object);
            _differentServerMock.Setup(server => server.CanDeployTo).Returns(true);
            _serverMock.SetupGet(it => it.ResourceName).Returns("some text");
            _serverEnvironmentId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(_serverEnvironmentId);
            _shellVm.Setup(model => model.LocalhostServer).Returns(_serverMock.Object);
            var popupController = new Mock<IPopupController>();
            var connectControl = new Mock<IConnectControlViewModel>();
            connectControl.SetupAllProperties();
            var connectControl2 = new Mock<IConnectControlViewModel>();
            connectControl2.SetupAllProperties();
            var destEnv = new Mock<IEnvironmentViewModel>();
            destEnv.Setup(model => model.IsConnected).Returns(true);
            destEnv.Setup(model => model.Server).Returns(_differentServerMock.Object);
            _destView.Setup(model => model.SelectedEnvironment).Returns(destEnv.Object);
            _destView.Setup(model => model.ConnectControlViewModel).Returns(connectControl2.Object);
            var explorerTreeItems = new List<IExplorerTreeItem>
            {
                new ExplorerItemViewModel(_serverMock.Object, It.IsAny<IExplorerTreeItem>(), model => It.IsAny<IExplorerItemViewModel>(),_shellVm.Object, new Mock<IPopupController>().Object) {ResourcePath = "SomeOtherpath"}
            };
            var explorerItemViewModels = new List<IExplorerItemViewModel>
            {
                new ExplorerItemViewModel(new Mock<IServer>().Object, new Mock<IExplorerTreeItem>().Object, a => { }, new Mock<IShellViewModel>().Object, new Mock<IPopupController>().Object) { ResourcePath = "Somepath" }
            };
            var sourceEnv = new Mock<IEnvironmentViewModel>();
            sourceEnv.Setup(model => model.IsConnected).Returns(true);
            sourceEnv.Setup(model => model.Server).Returns(_serverMock.Object);
            sourceEnv.Setup(model => model.AsList()).Returns(explorerItemViewModels);
            _sourceView.Setup(model => model.SelectedEnvironment).Returns(sourceEnv.Object);
            _sourceView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            var valueFunction = new Dictionary<string, string> { { "some key", "some value" } };
            _serverMock.Setup(server => server.GetServerInformation()).Returns(valueFunction);
            _sourceView.Setup(model => model.SelectedServer).Returns(_serverMock.Object);
            _sourceView.SetupGet(model => model.SelectedItems).Returns(explorerTreeItems);
            var environmentViewModels = new ObservableCollection<IEnvironmentViewModel> { sourceEnv.Object };
            _sourceView.SetupGet(model => model.Environments).Returns(environmentViewModels);
            _destView.Setup(model => model.SelectedEnvironment.AsList()).Returns(explorerItemViewModels);
            var singleExplorerDeployViewModel = new SingleExplorerDeployViewModel(_destView.Object, _sourceView.Object, explorerTreeItems, _statsView.Object, _shellVm.Object, popupController.Object)
            {
                SourceConnectControlViewModel = { SelectedConnection = _serverMock.Object },
                DestinationConnectControlViewModel = { SelectedConnection = _differentServerMock.Object },
            };
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            PrivateObject privateObject = new PrivateObject(singleExplorerDeployViewModel);
            privateObject.Invoke("Deploy");
            //---------------Test Result -----------------------
            Assert.IsTrue(singleExplorerDeployViewModel.DeploySuccessfull);
            Assert.IsTrue(singleExplorerDeployViewModel.DeploySuccessMessage.Contains("Deployed Successfully."));
            popupController.Verify(controller => controller.ShowDeploySuccessful(It.IsAny<string>()));
            _statsView.Verify(model => model.ReCalculate(), Times.Once);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Deploy_GivenConflictsAndMessageBoxResultCancel_ShouldSetIsDeployingToFalse()
        {
            //---------------Set up test pack-------------------
            _updateRepositoryMock.SetupProperty(manager => manager.ServerSaved);
            _serverMock.SetupGet(it => it.UpdateRepository).Returns(_updateRepositoryMock.Object);
            _serverMock.SetupGet(it => it.IsConnected).Returns(true);
            _serverMock.Setup(server => server.Permissions).Returns(new Mock<List<IWindowsGroupPermission>>().Object);
            _serverMock.Setup(server => server.CanDeployFrom).Returns(true);
            _differentServerMock.SetupGet(it => it.IsConnected).Returns(true);
            _differentServerMock.Setup(server => server.Permissions).Returns(new Mock<List<IWindowsGroupPermission>>().Object);
            _differentServerMock.Setup(server => server.CanDeployTo).Returns(true);
            _serverMock.SetupGet(it => it.ResourceName).Returns("some text");
            _serverEnvironmentId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(_serverEnvironmentId);
            _shellVm.Setup(model => model.LocalhostServer).Returns(_serverMock.Object);
            var popupController = new Mock<IPopupController>();
            var connectControl = new Mock<IConnectControlViewModel>();
            connectControl.SetupAllProperties();
            var connectControl2 = new Mock<IConnectControlViewModel>();
            connectControl2.SetupAllProperties();
            var destEnv = new Mock<IEnvironmentViewModel>();
            destEnv.Setup(model => model.IsConnected).Returns(true);
            destEnv.Setup(model => model.Server).Returns(_differentServerMock.Object);
            _destView.Setup(model => model.SelectedEnvironment).Returns(destEnv.Object);
            _destView.Setup(model => model.ConnectControlViewModel).Returns(connectControl2.Object);
            var explorerTreeItems = new List<IExplorerTreeItem>
            {
                new ExplorerItemViewModel(_serverMock.Object, It.IsAny<IExplorerTreeItem>(), model => It.IsAny<IExplorerItemViewModel>(),_shellVm.Object, new Mock<IPopupController>().Object) {ResourcePath = "SomeOtherpath"}
            };
            var explorerItemViewModels = new List<IExplorerItemViewModel>
            {
                new ExplorerItemViewModel(new Mock<IServer>().Object, new Mock<IExplorerTreeItem>().Object, a => { }, new Mock<IShellViewModel>().Object, new Mock<IPopupController>().Object) { ResourcePath = "Somepath" }
            };
            var sourceEnv = new Mock<IEnvironmentViewModel>();
            sourceEnv.Setup(model => model.IsConnected).Returns(true);
            sourceEnv.Setup(model => model.Server).Returns(_serverMock.Object);
            sourceEnv.Setup(model => model.AsList()).Returns(explorerItemViewModels);
            _sourceView.Setup(model => model.SelectedEnvironment).Returns(sourceEnv.Object);
            _sourceView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            _sourceView.Setup(model => model.SelectedServer).Returns(_serverMock.Object);
            _sourceView.SetupGet(model => model.SelectedItems).Returns(explorerTreeItems);
            var environmentViewModels = new ObservableCollection<IEnvironmentViewModel> { sourceEnv.Object };
            _sourceView.SetupGet(model => model.Environments).Returns(environmentViewModels);
            _destView.Setup(model => model.SelectedEnvironment.AsList()).Returns(explorerItemViewModels);
            var singleExplorerDeployViewModel = new SingleExplorerDeployViewModel(_destView.Object, _sourceView.Object, explorerTreeItems, _statsView.Object, _shellVm.Object, popupController.Object)
            {
                SourceConnectControlViewModel = { SelectedConnection = _serverMock.Object },
                DestinationConnectControlViewModel = { SelectedConnection = _differentServerMock.Object },
            };
            popupController.Setup(controller => controller.ShowDeployConflict(It.IsAny<int>())).Returns(MessageBoxResult.Cancel);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var conflicts = new List<Conflict>
            {
                new Conflict()
            };
            PrivateObject privateObject = new PrivateObject(singleExplorerDeployViewModel);
            typeof(SingleExplorerDeployViewModel).GetProperty("ConflictItems").SetValue(privateObject.Target, conflicts);
            privateObject.Invoke("Deploy");
            //---------------Test Result -----------------------
            popupController.Verify(controller => controller.ShowDeployConflict(It.IsAny<int>()), Times.AtLeastOnce);
            Assert.IsFalse(singleExplorerDeployViewModel.IsDeploying);
            Assert.IsTrue(singleExplorerDeployViewModel.ShowConflictItemsList);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Deploy_GivenConflictsAndMessageBoxResultOK_ShouldSetIsDeployingToTrue()
        {
            //---------------Set up test pack-------------------
            _updateRepositoryMock.SetupProperty(manager => manager.ServerSaved);
            _serverMock.SetupGet(it => it.UpdateRepository).Returns(_updateRepositoryMock.Object);
            _serverMock.SetupGet(it => it.IsConnected).Returns(true);
            _serverMock.Setup(server => server.Permissions).Returns(new Mock<List<IWindowsGroupPermission>>().Object);
            _serverMock.Setup(server => server.CanDeployFrom).Returns(true);
            _differentServerMock.SetupGet(it => it.IsConnected).Returns(true);
            _differentServerMock.Setup(server => server.Permissions).Returns(new Mock<List<IWindowsGroupPermission>>().Object);
            _differentServerMock.Setup(server => server.CanDeployTo).Returns(true);
            _serverMock.SetupGet(it => it.ResourceName).Returns("some text");
            _serverEnvironmentId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(_serverEnvironmentId);
            _shellVm.Setup(model => model.LocalhostServer).Returns(_serverMock.Object);
            var popupController = new Mock<IPopupController>();
            var connectControl = new Mock<IConnectControlViewModel>();
            connectControl.SetupAllProperties();
            var connectControl2 = new Mock<IConnectControlViewModel>();
            connectControl2.SetupAllProperties();
            var destEnv = new Mock<IEnvironmentViewModel>();
            destEnv.Setup(model => model.IsConnected).Returns(true);
            destEnv.Setup(model => model.Server).Returns(_differentServerMock.Object);
            _destView.Setup(model => model.SelectedEnvironment).Returns(destEnv.Object);
            _destView.Setup(model => model.ConnectControlViewModel).Returns(connectControl2.Object);
            var explorerTreeItems = new List<IExplorerTreeItem>
            {
                new ExplorerItemViewModel(_serverMock.Object, It.IsAny<IExplorerTreeItem>(), model => It.IsAny<IExplorerItemViewModel>(),_shellVm.Object, new Mock<IPopupController>().Object) {ResourcePath = "SomeOtherpath"}
            };
            var explorerItemViewModels = new List<IExplorerItemViewModel>
            {
                new ExplorerItemViewModel(new Mock<IServer>().Object, new Mock<IExplorerTreeItem>().Object, a => { }, new Mock<IShellViewModel>().Object, new Mock<IPopupController>().Object) { ResourcePath = "Somepath" }
            };
            var sourceEnv = new Mock<IEnvironmentViewModel>();
            sourceEnv.Setup(model => model.IsConnected).Returns(true);
            sourceEnv.Setup(model => model.Server).Returns(_serverMock.Object);
            sourceEnv.Setup(model => model.AsList()).Returns(explorerItemViewModels);
            _sourceView.Setup(model => model.SelectedEnvironment).Returns(sourceEnv.Object);
            _sourceView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            _sourceView.Setup(model => model.SelectedServer).Returns(_serverMock.Object);
            _sourceView.SetupGet(model => model.SelectedItems).Returns(explorerTreeItems);
            var environmentViewModels = new ObservableCollection<IEnvironmentViewModel> { sourceEnv.Object };
            _sourceView.SetupGet(model => model.Environments).Returns(environmentViewModels);
            _destView.Setup(model => model.SelectedEnvironment.AsList()).Returns(explorerItemViewModels);
            var singleExplorerDeployViewModel = new SingleExplorerDeployViewModel(_destView.Object, _sourceView.Object, explorerTreeItems, _statsView.Object, _shellVm.Object, popupController.Object)
            {
                SourceConnectControlViewModel = { SelectedConnection = _serverMock.Object },
                DestinationConnectControlViewModel = { SelectedConnection = _differentServerMock.Object },
            };
            popupController.Setup(controller => controller.ShowDeployConflict(It.IsAny<int>())).Returns(MessageBoxResult.OK);
            popupController.Setup(controller => controller.ShowDeploySuccessful(It.IsAny<string>())).Returns(MessageBoxResult.OK);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var conflicts = new List<Conflict>
            {
                new Conflict()
            };
            PrivateObject privateObject = new PrivateObject(singleExplorerDeployViewModel);
            typeof(SingleExplorerDeployViewModel).GetProperty("ConflictItems").SetValue(privateObject.Target, conflicts);
            privateObject.Invoke("Deploy");
            //---------------Test Result -----------------------
            popupController.Verify(controller => controller.ShowDeployConflict(It.IsAny<int>()), Times.AtLeastOnce);
            popupController.Verify(controller => controller.ShowDeploySuccessful(It.IsAny<string>()), Times.AtLeastOnce);
            Assert.IsFalse(singleExplorerDeployViewModel.IsDeploying);
        }
    }
}
