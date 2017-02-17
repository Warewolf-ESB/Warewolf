using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Deploy;
using Dev2.Common.Interfaces.Studio.Controller;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

// ReSharper disable InconsistentNaming

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class SingleExplorerDeployViewModelTests
    {
        private Mock<IServer> _serverMock;
        private Mock<IServer> _differentServerMock;
        private Guid _serverEnvironmentId;
        private Mock<IEventAggregator> _eventAggregatorMock;
        private Mock<IStudioUpdateManager> _updateRepositoryMock;


        //SingleExplorerDeployViewModel(IDeployDestinationExplorerViewModel destination, IDeploySourceExplorerViewModel source, 
        //IEnumerable<IExplorerTreeItem> selectedItems, IDeployStatsViewerViewModel stats, IShellViewModel shell, IPopupController popupController)
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CanDeploytests_GivenCanSelectAllDependencies_ShouldMatch()
        {
            //---------------Set up test pack-------------------
            var destView = new Mock<IDeployDestinationExplorerViewModel>();
            var sourceView = new Mock<IDeploySourceExplorerViewModel>();
            sourceView.Setup(model => model.SelectedItems).Returns(new List<IExplorerTreeItem>());
            var statsView = new Mock<IDeployStatsViewerViewModel>();
            statsView.SetupAllProperties();
            var shellVm = new Mock<IShellViewModel>();
            var popupController = new Mock<IPopupController>();
            var connectControl = new Mock<IConnectControlViewModel>();
            connectControl.SetupAllProperties();
            destView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            sourceView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            var singleExplorerDeployViewModel = new SingleExplorerDeployViewModel(destView.Object, sourceView.Object, new List<IExplorerTreeItem>(), statsView.Object, shellVm.Object, popupController.Object);
            //---------------Assert Precondition----------------
            Assert.IsFalse(singleExplorerDeployViewModel.CanSelectDependencies);
            Assert.IsFalse(singleExplorerDeployViewModel.CanDeployTests);
            //---------------Execute Test ----------------------
            sourceView.Setup(model => model.SelectedItems).Returns(new List<IExplorerTreeItem>()
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
            var destView = new Mock<IDeployDestinationExplorerViewModel>();
            var sourceView = new Mock<IDeploySourceExplorerViewModel>();
            sourceView.Setup(model => model.SelectedItems).Returns(new List<IExplorerTreeItem>());
            var env = new Mock<IEnvironmentViewModel>();
            env.SetupGet(model => model.IsConnected).Returns(true);
            sourceView.Setup(model => model.SelectedEnvironment).Returns(env.Object);
            var statsView = new Mock<IDeployStatsViewerViewModel>();
            statsView.SetupAllProperties();
            var shellVm = new Mock<IShellViewModel>();
            var popupController = new Mock<IPopupController>();
            var connectControl = new Mock<IConnectControlViewModel>();
            connectControl.SetupAllProperties();
            connectControl.SetupGet(model => model.IsConnected).Returns(false);
            destView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            sourceView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            var singleExplorerDeployViewModel = new SingleExplorerDeployViewModel(destView.Object, sourceView.Object, new List<IExplorerTreeItem>(), statsView.Object, shellVm.Object, popupController.Object);
            //---------------Assert Precondition----------------
            Assert.IsFalse(singleExplorerDeployViewModel.CanSelectDependencies);
            Assert.IsFalse(singleExplorerDeployViewModel.CanDeployTests);
            //---------------Execute Test ----------------------
            sourceView.Setup(model => model.SelectedItems).Returns(new List<IExplorerTreeItem>()
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
            var destView = new Mock<IDeployDestinationExplorerViewModel>();
            var sourceView = new Mock<IDeploySourceExplorerViewModel>();
            sourceView.Setup(model => model.SelectedItems).Returns(new List<IExplorerTreeItem>());
            var env = new Mock<IEnvironmentViewModel>();
            env.SetupGet(model => model.IsConnected).Returns(true);
            sourceView.Setup(model => model.SelectedEnvironment).Returns(env.Object);
            var statsView = new Mock<IDeployStatsViewerViewModel>();
            statsView.SetupAllProperties();
            var shellVm = new Mock<IShellViewModel>();
            var popupController = new Mock<IPopupController>();
            var connectControl = new Mock<IConnectControlViewModel>();
            connectControl.SetupAllProperties();
            connectControl.SetupGet(model => model.IsConnected).Returns(false);
            destView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            sourceView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            var singleExplorerDeployViewModel = new SingleExplorerDeployViewModel(destView.Object, sourceView.Object, new List<IExplorerTreeItem>(), statsView.Object, shellVm.Object, popupController.Object);
            bool wasCalled = false;
            singleExplorerDeployViewModel.Destination.PropertyChanged += (sender, args) =>
            {
                if(args.PropertyName == "IsConnected")
                {
                    wasCalled = true;
                }
            };
            //---------------Assert Precondition----------------
            Assert.IsFalse(singleExplorerDeployViewModel.CanSelectDependencies);
            Assert.IsFalse(singleExplorerDeployViewModel.CanDeployTests);
            //---------------Execute Test ----------------------
            sourceView.Setup(model => model.SelectedItems).Returns(new List<IExplorerTreeItem>()
            {
                new Mock<IExplorerTreeItem>().Object

            });
            var propertyChangedEventArgs = new PropertyChangedEventArgs("IsConnected");
            destView.Raise(model => model.PropertyChanged += (sender, args) => {}, singleExplorerDeployViewModel, propertyChangedEventArgs );
            //---------------Test Result -----------------------
            Assert.IsTrue(wasCalled);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Destination_GivenNewDestinationIsCreated_ShouldHaveNewDestination()
        {
            //---------------Set up test pack-------------------
            var destView = new Mock<IDeployDestinationExplorerViewModel>();
            var sourceView = new Mock<IDeploySourceExplorerViewModel>();
            sourceView.Setup(model => model.SelectedItems).Returns(new List<IExplorerTreeItem>());
            var statsView = new Mock<IDeployStatsViewerViewModel>();
            statsView.SetupAllProperties();
            var shellVm = new Mock<IShellViewModel>();
            _serverMock = new Mock<IServer>();
            _eventAggregatorMock = new Mock<IEventAggregator>();
            _updateRepositoryMock = new Mock<IStudioUpdateManager>();
            _updateRepositoryMock.SetupProperty(manager => manager.ServerSaved);
            _serverMock.SetupGet(it => it.UpdateRepository).Returns(_updateRepositoryMock.Object);
            _serverMock.SetupGet(it => it.ResourceName).Returns("some text");
            _serverEnvironmentId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(_serverEnvironmentId);
            shellVm.Setup(model => model.LocalhostServer).Returns(_serverMock.Object);
            var popupController = new Mock<IPopupController>();
            var connectControl = new Mock<IConnectControlViewModel>();
            connectControl.SetupAllProperties();
            destView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            sourceView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            var singleExplorerDeployViewModel = new SingleExplorerDeployViewModel(destView.Object, sourceView.Object, new List<IExplorerTreeItem>(), statsView.Object, shellVm.Object, popupController.Object);
            //---------------Assert Precondition----------------
            var deployDestinationExplorerViewModelBefore = singleExplorerDeployViewModel.Destination;
            Assert.IsNotNull(deployDestinationExplorerViewModelBefore);
            //---------------Execute Test ----------------------
            singleExplorerDeployViewModel.Destination = new DeployDestinationViewModel(shellVm.Object, new EventAggregator());
            //---------------Test Result -----------------------
            Assert.IsFalse(Equals(deployDestinationExplorerViewModelBefore, singleExplorerDeployViewModel.Destination));
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void CanDeploy_GivenServerIsNotConnected_ShouldHaveFalse()
        {
            //---------------Set up test pack-------------------
            var destView = new Mock<IDeployDestinationExplorerViewModel>();
            var sourceView = new Mock<IDeploySourceExplorerViewModel>();
            sourceView.Setup(model => model.SelectedItems).Returns(new List<IExplorerTreeItem>());
            var statsView = new Mock<IDeployStatsViewerViewModel>();
            statsView.SetupAllProperties();
            var shellVm = new Mock<IShellViewModel>();
            _serverMock = new Mock<IServer>();
            _eventAggregatorMock = new Mock<IEventAggregator>();
            _updateRepositoryMock = new Mock<IStudioUpdateManager>();
            _updateRepositoryMock.SetupProperty(manager => manager.ServerSaved);
            _serverMock.SetupGet(it => it.UpdateRepository).Returns(_updateRepositoryMock.Object);
            _serverMock.SetupGet(it => it.IsConnected).Returns(false);
            _serverMock.SetupGet(it => it.ResourceName).Returns("some text");
            _serverEnvironmentId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(_serverEnvironmentId);
            shellVm.Setup(model => model.LocalhostServer).Returns(_serverMock.Object);
            var popupController = new Mock<IPopupController>();
            var connectControl = new Mock<IConnectControlViewModel>();
            connectControl.SetupAllProperties();
            destView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            sourceView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            var singleExplorerDeployViewModel = new SingleExplorerDeployViewModel(destView.Object, sourceView.Object, new List<IExplorerTreeItem>(), statsView.Object, shellVm.Object, popupController.Object)
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
        public void CanDeploy_GivenSelectedServerIsNull_ShouldHaveFalse()
        {
            //---------------Set up test pack-------------------
            var destView = new Mock<IDeployDestinationExplorerViewModel>();
            var sourceView = new Mock<IDeploySourceExplorerViewModel>();
            sourceView.Setup(model => model.SelectedItems).Returns(new List<IExplorerTreeItem>());
            var statsView = new Mock<IDeployStatsViewerViewModel>();
            statsView.SetupAllProperties();
            var shellVm = new Mock<IShellViewModel>();
            _serverMock = new Mock<IServer>();
            _eventAggregatorMock = new Mock<IEventAggregator>();
            _updateRepositoryMock = new Mock<IStudioUpdateManager>();
            _updateRepositoryMock.SetupProperty(manager => manager.ServerSaved);
            _serverMock.SetupGet(it => it.UpdateRepository).Returns(_updateRepositoryMock.Object);
            _serverMock.SetupGet(it => it.IsConnected).Returns(false);
            _serverMock.SetupGet(it => it.ResourceName).Returns("some text");
            _serverEnvironmentId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(_serverEnvironmentId);
            shellVm.Setup(model => model.LocalhostServer).Returns(_serverMock.Object);
            var popupController = new Mock<IPopupController>();
            var connectControl = new Mock<IConnectControlViewModel>();
            connectControl.SetupAllProperties();
            destView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            sourceView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            var singleExplorerDeployViewModel = new SingleExplorerDeployViewModel(destView.Object, sourceView.Object, new List<IExplorerTreeItem>(), statsView.Object, shellVm.Object, popupController.Object)
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
            var destView = new Mock<IDeployDestinationExplorerViewModel>();
            var sourceView = new Mock<IDeploySourceExplorerViewModel>();
            sourceView.Setup(model => model.SelectedItems).Returns(new List<IExplorerTreeItem>());
            var statsView = new Mock<IDeployStatsViewerViewModel>();
            statsView.SetupAllProperties();
            var shellVm = new Mock<IShellViewModel>();
            _serverMock = new Mock<IServer>();
            _eventAggregatorMock = new Mock<IEventAggregator>();
            _updateRepositoryMock = new Mock<IStudioUpdateManager>();
            _updateRepositoryMock.SetupProperty(manager => manager.ServerSaved);
            _serverMock.SetupGet(it => it.UpdateRepository).Returns(_updateRepositoryMock.Object);
            _serverMock.SetupGet(it => it.ResourceName).Returns("some text");
            _serverEnvironmentId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(_serverEnvironmentId);
            _serverMock.Setup(server => server.IsConnected).Returns(true);
            shellVm.Setup(model => model.LocalhostServer).Returns(_serverMock.Object);
            var popupController = new Mock<IPopupController>();
            var connectControl = new Mock<IConnectControlViewModel>();
            connectControl.SetupAllProperties();
            destView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            sourceView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            sourceView.SetupGet(model => model.SelectedEnvironment).Returns(It.IsAny<IEnvironmentViewModel>());
            var singleExplorerDeployViewModel = new SingleExplorerDeployViewModel(destView.Object, sourceView.Object, new List<IExplorerTreeItem>(), statsView.Object, shellVm.Object, popupController.Object)
            {
                Destination = new DeployDestinationViewModel(shellVm.Object, new EventAggregator()),
                Source = sourceView.Object,
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
            var destView = new Mock<IDeployDestinationExplorerViewModel>();
            var sourceView = new Mock<IDeploySourceExplorerViewModel>();
            sourceView.Setup(model => model.SelectedItems).Returns(new List<IExplorerTreeItem>());
            var statsView = new Mock<IDeployStatsViewerViewModel>();
            statsView.SetupAllProperties();
            var shellVm = new Mock<IShellViewModel>();
            _serverMock = new Mock<IServer>();
            _eventAggregatorMock = new Mock<IEventAggregator>();
            _updateRepositoryMock = new Mock<IStudioUpdateManager>();
            _updateRepositoryMock.SetupProperty(manager => manager.ServerSaved);
            _serverMock.SetupGet(it => it.UpdateRepository).Returns(_updateRepositoryMock.Object);
            _serverMock.SetupGet(it => it.ResourceName).Returns("some text");
            _serverEnvironmentId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(_serverEnvironmentId);
            _serverMock.Setup(server => server.IsConnected).Returns(true);
            shellVm.Setup(model => model.LocalhostServer).Returns(_serverMock.Object);
            var popupController = new Mock<IPopupController>();
            var connectControl = new Mock<IConnectControlViewModel>();
            var differentConnectControl = new Mock<IConnectControlViewModel>();
            connectControl.SetupAllProperties();
            differentConnectControl.SetupAllProperties();
            _differentServerMock = new Mock<IServer>();
            differentConnectControl.Setup(model => model.SelectedConnection).Returns(_differentServerMock.Object);
            destView.Setup(model => model.ConnectControlViewModel).Returns(differentConnectControl.Object);
            destView.Setup(model => model.SelectedEnvironment).Returns(It.IsAny<IEnvironmentViewModel>());            
            sourceView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            sourceView.Setup(model => model.SelectedEnvironment).Returns(new Mock<IEnvironmentViewModel>().Object);
            sourceView.Setup(model => model.SelectedEnvironment.IsConnected).Returns(true);
            var singleExplorerDeployViewModel = new SingleExplorerDeployViewModel(destView.Object, sourceView.Object, new List<IExplorerTreeItem>(), statsView.Object, shellVm.Object, popupController.Object)
            {
                Destination = new DeployDestinationViewModel(shellVm.Object, _eventAggregatorMock.Object)
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
            var destView = new Mock<IDeployDestinationExplorerViewModel>();
            var sourceView = new Mock<IDeploySourceExplorerViewModel>();
            sourceView.Setup(model => model.SelectedItems).Returns(new List<IExplorerTreeItem>());
            var statsView = new Mock<IDeployStatsViewerViewModel>();
            statsView.SetupAllProperties();
            var shellVm = new Mock<IShellViewModel>();
            _serverMock = new Mock<IServer>();
            _eventAggregatorMock = new Mock<IEventAggregator>();
            _updateRepositoryMock = new Mock<IStudioUpdateManager>();
            _updateRepositoryMock.SetupProperty(manager => manager.ServerSaved);
            _serverMock.SetupGet(it => it.UpdateRepository).Returns(_updateRepositoryMock.Object);
            _serverMock.SetupGet(it => it.ResourceName).Returns("some text");
            _serverEnvironmentId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(_serverEnvironmentId);
            _serverMock.Setup(server => server.IsConnected).Returns(true);
            shellVm.Setup(model => model.LocalhostServer).Returns(_serverMock.Object);
            var popupController = new Mock<IPopupController>();
            var connectControl = new Mock<IConnectControlViewModel>();
            connectControl.SetupAllProperties();
            destView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            destView.Setup(model => model.SelectedEnvironment).Returns(It.IsAny<IEnvironmentViewModel>());
            sourceView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            sourceView.Setup(model => model.SelectedEnvironment).Returns(new Mock<IEnvironmentViewModel>().Object);
            sourceView.Setup(model => model.SelectedEnvironment.IsConnected).Returns(true);
            var singleExplorerDeployViewModel = new SingleExplorerDeployViewModel(destView.Object, sourceView.Object, new List<IExplorerTreeItem>(), statsView.Object, shellVm.Object, popupController.Object);
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
            var destView = new Mock<IDeployDestinationExplorerViewModel>();
            var sourceView = new Mock<IDeploySourceExplorerViewModel>();
            sourceView.Setup(model => model.SelectedItems).Returns(new List<IExplorerTreeItem>());
            var statsView = new Mock<IDeployStatsViewerViewModel>();
            statsView.SetupAllProperties();
            var shellVm = new Mock<IShellViewModel>();
            _serverMock = new Mock<IServer>();            
            _eventAggregatorMock = new Mock<IEventAggregator>();
            _updateRepositoryMock = new Mock<IStudioUpdateManager>();
            _updateRepositoryMock.SetupProperty(manager => manager.ServerSaved);
            _serverMock.SetupGet(it => it.UpdateRepository).Returns(_updateRepositoryMock.Object);
            _serverMock.SetupGet(it => it.ResourceName).Returns("some text");
            _serverEnvironmentId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(_serverEnvironmentId);
            _serverMock.Setup(server => server.IsConnected).Returns(true);
            shellVm.Setup(model => model.LocalhostServer).Returns(_serverMock.Object);
            var popupController = new Mock<IPopupController>();
            var connectControl = new Mock<IConnectControlViewModel>();
            connectControl.SetupAllProperties();
            destView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            destView.Setup(model => model.SelectedEnvironment).Returns(It.IsAny<IEnvironmentViewModel>());
            sourceView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            sourceView.Setup(model => model.SelectedEnvironment).Returns(new Mock<IEnvironmentViewModel>().Object);
            sourceView.SetupGet(model => model.SelectedItems).Returns(new List<IExplorerTreeItem>
            {
                new ExplorerItemViewModel(_serverMock.Object, It.IsAny<IExplorerTreeItem>(), model => It.IsAny<IExplorerItemViewModel>(),shellVm.Object, new Mock<IPopupController>().Object)
            });
            sourceView.Setup(model => model.SelectedEnvironment.IsConnected).Returns(true);
            destView.Setup(model => model.ConnectControlViewModel.IsConnected).Returns(true);
            var singleExplorerDeployViewModel = new SingleExplorerDeployViewModel(destView.Object, sourceView.Object, new List<IExplorerTreeItem>(), statsView.Object, shellVm.Object, popupController.Object);
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
            var destView = new Mock<IDeployDestinationExplorerViewModel>();
            var sourceView = new Mock<IDeploySourceExplorerViewModel>();
            sourceView.Setup(model => model.SelectedItems).Returns(new List<IExplorerTreeItem>());
            var statsView = new Mock<IDeployStatsViewerViewModel>();
            statsView.SetupAllProperties();
            statsView.Setup(model => model.New).Returns(new List<IExplorerTreeItem>
            {
                new Mock<IEnvironmentViewModel>().Object
            });
            var shellVm = new Mock<IShellViewModel>();
            _serverMock = new Mock<IServer>();            
            _eventAggregatorMock = new Mock<IEventAggregator>();
            _updateRepositoryMock = new Mock<IStudioUpdateManager>();
            _updateRepositoryMock.SetupProperty(manager => manager.ServerSaved);
            _serverMock.SetupGet(it => it.UpdateRepository).Returns(_updateRepositoryMock.Object);
            _serverMock.SetupGet(it => it.ResourceName).Returns("some text");
            _serverEnvironmentId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(_serverEnvironmentId);
            _serverMock.Setup(server => server.IsConnected).Returns(true);
            shellVm.Setup(model => model.LocalhostServer).Returns(_serverMock.Object);
            var popupController = new Mock<IPopupController>();            
            var connectControl = new Mock<IConnectControlViewModel>();
            connectControl.SetupAllProperties();
            destView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            destView.Setup(model => model.SelectedEnvironment).Returns(It.IsAny<IEnvironmentViewModel>());
            sourceView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            sourceView.Setup(model => model.SelectedEnvironment).Returns(new Mock<IEnvironmentViewModel>().Object);
            sourceView.SetupGet(model => model.SelectedItems).Returns(new List<IExplorerTreeItem>
            {
                new ExplorerItemViewModel(_serverMock.Object, It.IsAny<IExplorerTreeItem>(), model => It.IsAny<IExplorerItemViewModel>(),shellVm.Object, new Mock<IPopupController>().Object)
            });
            var singleExplorerDeployViewModel = new SingleExplorerDeployViewModel(destView.Object, sourceView.Object, new List<IExplorerTreeItem>(), statsView.Object, shellVm.Object, popupController.Object);
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
            var destView = new Mock<IDeployDestinationExplorerViewModel>();
            var sourceView = new Mock<IDeploySourceExplorerViewModel>();
            sourceView.Setup(model => model.SelectedItems).Returns(new List<IExplorerTreeItem>());
            var statsView = new Mock<IDeployStatsViewerViewModel>();
            statsView.SetupAllProperties();
            var shellVm = new Mock<IShellViewModel>();
            _serverMock = new Mock<IServer>();
            _differentServerMock = new Mock<IServer>();
            _eventAggregatorMock = new Mock<IEventAggregator>();
            _updateRepositoryMock = new Mock<IStudioUpdateManager>();
            _updateRepositoryMock.SetupProperty(manager => manager.ServerSaved);
            _serverMock.SetupGet(it => it.UpdateRepository).Returns(_updateRepositoryMock.Object);
            _serverMock.SetupGet(it => it.ResourceName).Returns("some text");
            _serverEnvironmentId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(_serverEnvironmentId);
            _serverMock.Setup(server => server.IsConnected).Returns(true);
            _serverMock.Setup(server => server.VersionInfo.VersionNumber).Returns("2.0.0.0");
            shellVm.Setup(model => model.LocalhostServer).Returns(_serverMock.Object);
            var popupController = new Mock<IPopupController>();
            popupController.Setup(controller => controller.ShowDeployServerMinVersionConflict(It.IsAny<string>(), It.IsAny<string>())).Returns(MessageBoxResult.Cancel);
            var connectControl = new Mock<IConnectControlViewModel>();
            connectControl.SetupAllProperties();
            destView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            destView.Setup(model => model.SelectedEnvironment).Returns(It.IsAny<IEnvironmentViewModel>());
            destView.Setup(model => model.SelectedServer).Returns(_differentServerMock.Object);
            destView.Setup(model => model.MinSupportedVersion).Returns(new Version("2.0.0.0"));
            sourceView.Setup(model => model.ServerVersion).Returns(new Version("1.0.0.0"));
            sourceView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            sourceView.Setup(model => model.SelectedEnvironment).Returns(new Mock<IEnvironmentViewModel>().Object);
            sourceView.SetupGet(model => model.SelectedItems).Returns(new List<IExplorerTreeItem>
            {
                new ExplorerItemViewModel(_serverMock.Object, It.IsAny<IExplorerTreeItem>(), model => It.IsAny<IExplorerItemViewModel>(),shellVm.Object, new Mock<IPopupController>().Object)
            });
            var singleExplorerDeployViewModel = new SingleExplorerDeployViewModel(destView.Object, sourceView.Object, new List<IExplorerTreeItem>(), statsView.Object, shellVm.Object, popupController.Object);
            var privateObject = new PrivateObject(singleExplorerDeployViewModel);
            singleExplorerDeployViewModel.DestinationConnectControlViewModel.SelectedConnection = _differentServerMock.Object;
            singleExplorerDeployViewModel.SourceConnectControlViewModel.SelectedConnection = _serverMock.Object;
            //---------------Test Result -----------------------
            privateObject.Invoke("CheckVersionConflict");
            Assert.IsFalse(singleExplorerDeployViewModel.IsDeploying);
        }


        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void StatsViewModel_GivenNewInstance_ShouldNotBeNull()
        {
            //---------------Set up test pack-------------------
            var destView = new Mock<IDeployDestinationExplorerViewModel>();
            var sourceView = new Mock<IDeploySourceExplorerViewModel>();
            sourceView.Setup(model => model.SelectedItems).Returns(new List<IExplorerTreeItem>());
            var statsView = new Mock<IDeployStatsViewerViewModel>();
            statsView.SetupAllProperties();
            var shellVm = new Mock<IShellViewModel>();
            _serverMock = new Mock<IServer>();
            _eventAggregatorMock = new Mock<IEventAggregator>();
            _updateRepositoryMock = new Mock<IStudioUpdateManager>();
            _updateRepositoryMock.SetupProperty(manager => manager.ServerSaved);
            _serverMock.SetupGet(it => it.UpdateRepository).Returns(_updateRepositoryMock.Object);
            _serverMock.SetupGet(it => it.ResourceName).Returns("some text");
            _serverEnvironmentId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(_serverEnvironmentId);
            _serverMock.Setup(server => server.IsConnected).Returns(true);
            shellVm.Setup(model => model.LocalhostServer).Returns(_serverMock.Object);
            var popupController = new Mock<IPopupController>();
            var connectControl = new Mock<IConnectControlViewModel>();
            connectControl.SetupAllProperties();
            destView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            destView.Setup(model => model.SelectedEnvironment).Returns(It.IsAny<IEnvironmentViewModel>());
            sourceView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            sourceView.Setup(model => model.SelectedEnvironment).Returns(new Mock<IEnvironmentViewModel>().Object);
            sourceView.Setup(model => model.SelectedEnvironment.IsConnected).Returns(true);
            var singleExplorerDeployViewModel = new SingleExplorerDeployViewModel(destView.Object, sourceView.Object, new List<IExplorerTreeItem>(), statsView.Object, shellVm.Object, popupController.Object);
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
            var destView = new Mock<IDeployDestinationExplorerViewModel>();
            var sourceView = new Mock<IDeploySourceExplorerViewModel>();
            sourceView.Setup(model => model.SelectedItems).Returns(new List<IExplorerTreeItem>());
            var statsView = new Mock<IDeployStatsViewerViewModel>();
            statsView.SetupAllProperties();
            var shellVm = new Mock<IShellViewModel>();
            _serverMock = new Mock<IServer>();
            _eventAggregatorMock = new Mock<IEventAggregator>();
            _updateRepositoryMock = new Mock<IStudioUpdateManager>();
            _updateRepositoryMock.SetupProperty(manager => manager.ServerSaved);
            _serverMock.SetupGet(it => it.UpdateRepository).Returns(_updateRepositoryMock.Object);
            _serverMock.SetupGet(it => it.ResourceName).Returns("some text");
            _serverEnvironmentId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(_serverEnvironmentId);
            shellVm.Setup(model => model.LocalhostServer).Returns(_serverMock.Object);
            var popupController = new Mock<IPopupController>();
            var connectControl = new Mock<IConnectControlViewModel>();
            connectControl.SetupAllProperties();
            destView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            sourceView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            var singleExplorerDeployViewModel = new SingleExplorerDeployViewModel(destView.Object, sourceView.Object, new List<IExplorerTreeItem>(), statsView.Object, shellVm.Object, popupController.Object);
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
            var destView = new Mock<IDeployDestinationExplorerViewModel>();
            var sourceView = new Mock<IDeploySourceExplorerViewModel>();
            sourceView.Setup(model => model.SelectedItems).Returns(new List<IExplorerTreeItem>());
            var statsView = new Mock<IDeployStatsViewerViewModel>();
            statsView.SetupAllProperties();
            var shellVm = new Mock<IShellViewModel>();
            _serverMock = new Mock<IServer>();
            _eventAggregatorMock = new Mock<IEventAggregator>();
            _updateRepositoryMock = new Mock<IStudioUpdateManager>();
            _updateRepositoryMock.SetupProperty(manager => manager.ServerSaved);
            _serverMock.SetupGet(it => it.UpdateRepository).Returns(_updateRepositoryMock.Object);
            _serverMock.SetupGet(it => it.ResourceName).Returns("some text");
            _serverEnvironmentId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(_serverEnvironmentId);
            shellVm.Setup(model => model.LocalhostServer).Returns(_serverMock.Object);
            var popupController = new Mock<IPopupController>();
            var connectControl = new Mock<IConnectControlViewModel>();
            connectControl.SetupAllProperties();
            destView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            sourceView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            var singleExplorerDeployViewModel = new SingleExplorerDeployViewModel(destView.Object, sourceView.Object, new List<IExplorerTreeItem>(), statsView.Object, shellVm.Object, popupController.Object);
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
            var destView = new Mock<IDeployDestinationExplorerViewModel>();
            var sourceView = new Mock<IDeploySourceExplorerViewModel>();
            sourceView.Setup(model => model.SelectedItems).Returns(new List<IExplorerTreeItem>());
            var statsView = new Mock<IDeployStatsViewerViewModel>();
            statsView.SetupAllProperties();
            var shellVm = new Mock<IShellViewModel>();
            _serverMock = new Mock<IServer>();
            _eventAggregatorMock = new Mock<IEventAggregator>();
            _updateRepositoryMock = new Mock<IStudioUpdateManager>();
            _updateRepositoryMock.SetupProperty(manager => manager.ServerSaved);
            _serverMock.SetupGet(it => it.UpdateRepository).Returns(_updateRepositoryMock.Object);
            _serverMock.SetupGet(it => it.ResourceName).Returns("some text");
            _serverEnvironmentId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(_serverEnvironmentId);
            shellVm.Setup(model => model.LocalhostServer).Returns(_serverMock.Object);
            var popupController = new Mock<IPopupController>();
            var connectControl = new Mock<IConnectControlViewModel>();
            connectControl.SetupAllProperties();
            destView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            sourceView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            var singleExplorerDeployViewModel = new SingleExplorerDeployViewModel(destView.Object, sourceView.Object, new List<IExplorerTreeItem>(), statsView.Object, shellVm.Object, popupController.Object);
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
            var destView = new Mock<IDeployDestinationExplorerViewModel>();
            var sourceView = new Mock<IDeploySourceExplorerViewModel>();
            sourceView.Setup(model => model.SelectedItems).Returns(new List<IExplorerTreeItem>());
            var statsView = new Mock<IDeployStatsViewerViewModel>();
            statsView.SetupAllProperties();
            var shellVm = new Mock<IShellViewModel>();
            _serverMock = new Mock<IServer>();
            _eventAggregatorMock = new Mock<IEventAggregator>();
            _updateRepositoryMock = new Mock<IStudioUpdateManager>();
            _updateRepositoryMock.SetupProperty(manager => manager.ServerSaved);
            _serverMock.SetupGet(it => it.UpdateRepository).Returns(_updateRepositoryMock.Object);
            _serverMock.SetupGet(it => it.ResourceName).Returns("some text");
            _serverEnvironmentId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(_serverEnvironmentId);
            shellVm.Setup(model => model.LocalhostServer).Returns(_serverMock.Object);
            var popupController = new Mock<IPopupController>();
            var connectControl = new Mock<IConnectControlViewModel>();
            connectControl.SetupAllProperties();
            destView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            sourceView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            var singleExplorerDeployViewModel = new SingleExplorerDeployViewModel(destView.Object, sourceView.Object, new List<IExplorerTreeItem>(), statsView.Object, shellVm.Object, popupController.Object);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            singleExplorerDeployViewModel.DeploySuccessMessage = "Successful";
            singleExplorerDeployViewModel.ErrorMessage = "some error";
            //---------------Test Result -----------------------
            Assert.IsTrue(string.IsNullOrEmpty(singleExplorerDeployViewModel.DeploySuccessMessage));
            Assert.IsFalse(singleExplorerDeployViewModel.DeploySuccessfull);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void NewResourceCount_GivenEmptyStateItems_Should0()
        {
            //---------------Set up test pack-------------------
            var destView = new Mock<IDeployDestinationExplorerViewModel>();
            var sourceView = new Mock<IDeploySourceExplorerViewModel>();
            sourceView.Setup(model => model.SelectedItems).Returns(new List<IExplorerTreeItem>());
            var statsView = new Mock<IDeployStatsViewerViewModel>();
            statsView.SetupAllProperties();
            statsView.Setup(model => model.NewResources).Returns(5);
            var shellVm = new Mock<IShellViewModel>();
            _serverMock = new Mock<IServer>();
            _eventAggregatorMock = new Mock<IEventAggregator>();
            _updateRepositoryMock = new Mock<IStudioUpdateManager>();
            _updateRepositoryMock.SetupProperty(manager => manager.ServerSaved);
            _serverMock.SetupGet(it => it.UpdateRepository).Returns(_updateRepositoryMock.Object);
            _serverMock.SetupGet(it => it.ResourceName).Returns("some text");
            _serverEnvironmentId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(_serverEnvironmentId);
            shellVm.Setup(model => model.LocalhostServer).Returns(_serverMock.Object);
            var popupController = new Mock<IPopupController>();
            var connectControl = new Mock<IConnectControlViewModel>();
            connectControl.SetupAllProperties();
            destView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            sourceView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            var singleExplorerDeployViewModel = new SingleExplorerDeployViewModel(destView.Object, sourceView.Object, new List<IExplorerTreeItem>(), statsView.Object, shellVm.Object, popupController.Object) { NewResourcesCount = 0.ToString() };
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
            var destView = new Mock<IDeployDestinationExplorerViewModel>();
            var sourceView = new Mock<IDeploySourceExplorerViewModel>();
            sourceView.Setup(model => model.SelectedItems).Returns(new List<IExplorerTreeItem>());
            var statsView = new Mock<IDeployStatsViewerViewModel>();
            statsView.SetupAllProperties();
            var shellVm = new Mock<IShellViewModel>();
            _serverMock = new Mock<IServer>();
            _eventAggregatorMock = new Mock<IEventAggregator>();
            _updateRepositoryMock = new Mock<IStudioUpdateManager>();
            _updateRepositoryMock.SetupProperty(manager => manager.ServerSaved);
            _serverMock.SetupGet(it => it.UpdateRepository).Returns(_updateRepositoryMock.Object);
            _serverMock.SetupGet(it => it.ResourceName).Returns("some text");
            _serverEnvironmentId = Guid.NewGuid();
            _serverMock.SetupGet(it => it.EnvironmentID).Returns(_serverEnvironmentId);
            shellVm.Setup(model => model.LocalhostServer).Returns(_serverMock.Object);
            var popupController = new Mock<IPopupController>();
            var connectControl = new Mock<IConnectControlViewModel>();
            connectControl.SetupAllProperties();
            destView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            sourceView.Setup(model => model.ConnectControlViewModel).Returns(connectControl.Object);
            var singleExplorerDeployViewModel = new SingleExplorerDeployViewModel(destView.Object, sourceView.Object, new List<IExplorerTreeItem>(), statsView.Object, shellVm.Object, popupController.Object);
            //---------------Assert Precondition----------------
            var deploySourceExplorerViewModelBefore = singleExplorerDeployViewModel.Source;
            Assert.IsNotNull(deploySourceExplorerViewModelBefore);
            //---------------Execute Test ----------------------
            singleExplorerDeployViewModel.Source = new DeploySourceExplorerViewModel(shellVm.Object, new EventAggregator(), new DeployStatsViewerViewModel(new Mock<IDeployDestinationExplorerViewModel>().Object));
            //---------------Test Result -----------------------
            Assert.IsFalse(Equals(deploySourceExplorerViewModelBefore, singleExplorerDeployViewModel.Source));
        }
    }
}
