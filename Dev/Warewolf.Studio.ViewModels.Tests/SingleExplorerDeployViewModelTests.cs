using System.Collections.Generic;
using System.ComponentModel;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Deploy;
using Dev2.Common.Interfaces.Studio.Controller;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

// ReSharper disable InconsistentNaming

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class SingleExplorerDeployViewModelTests
    {
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
    }
}
