using System.Collections.Generic;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Studio.ViewModels;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class ExplorerViewModelTests
    {

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerViewModel_Filter")]
        public void ExplorerViewModel_Filter_NullEnvironments_ShouldNotCallFilterOnEachEnvironment()
        {
            //------------Setup for test--------------------------
            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(model => model.LocalhostServer).Returns(new Mock<IServer>().Object);
            IExplorerViewModel explorerViewModel = new ExplorerViewModel(mockShellViewModel.Object,new Mock<IEventAggregator>().Object);
            var mockEnv1 = new Mock<IEnvironmentViewModel>();
            mockEnv1.Setup(model => model.Filter(It.IsAny<string>())).Verifiable();
            var mockEnv2 = new Mock<IEnvironmentViewModel>();
            mockEnv2.Setup(model => model.Filter(It.IsAny<string>())).Verifiable();
            explorerViewModel.Environments = null;
            //------------Execute Test---------------------------
            explorerViewModel.Filter("TestValue");
            //------------Assert Results-------------------------
            mockEnv1.Verify(model => model.Filter(It.IsAny<string>()),Times.Never());
            mockEnv2.Verify(model => model.Filter(It.IsAny<string>()),Times.Never());
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerViewModel_Filter")]
        public void ExplorerViewModel_Filter_ShouldCallFilterOnEachEnvironment()
        {
            //------------Setup for test--------------------------
            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(model => model.LocalhostServer).Returns(new Mock<IServer>().Object);
            IExplorerViewModel explorerViewModel = new ExplorerViewModel(mockShellViewModel.Object,new Mock<IEventAggregator>().Object);
            var mockEnv1 = new Mock<IEnvironmentViewModel>();
            mockEnv1.Setup(model => model.Filter(It.IsAny<string>())).Verifiable();
            var environment1 = mockEnv1.Object;
            var mockEnv2 = new Mock<IEnvironmentViewModel>();
            mockEnv2.Setup(model => model.Filter(It.IsAny<string>())).Verifiable();
            var environment2 = mockEnv2.Object;
            explorerViewModel.Environments = new List<IEnvironmentViewModel>{environment1,environment2};
            //------------Execute Test---------------------------
            explorerViewModel.Filter("TestValue");
            //------------Assert Results-------------------------
            mockEnv1.Verify();
            mockEnv2.Verify();
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerViewModel_SearchText")]
        public void ExplorerViewModel_SearchText_WhenUpdate_ShouldCallFilter()
        {
            //------------Setup for test--------------------------
            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(model => model.LocalhostServer).Returns(new Mock<IServer>().Object);
            ExplorerViewModel explorerViewModel = new ExplorerViewModel(mockShellViewModel.Object,new Mock<IEventAggregator>().Object);
            var mockEnv1 = new Mock<IEnvironmentViewModel>();
            mockEnv1.Setup(model => model.Filter(It.IsAny<string>())).Verifiable();
            var environment1 = mockEnv1.Object;
            var mockEnv2 = new Mock<IEnvironmentViewModel>();
            mockEnv2.Setup(model => model.Filter(It.IsAny<string>())).Verifiable();
            var environment2 = mockEnv2.Object;
            explorerViewModel.Environments = new List<IEnvironmentViewModel> { environment1, environment2 };
            var propertyChangedFired = false;
            explorerViewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "SearchText")
                {
                    propertyChangedFired = true;
                }
            };
            //------------Execute Test---------------------------
            explorerViewModel.SearchText = "TestValue";
            //------------Assert Results-------------------------
            mockEnv1.Verify();
            mockEnv2.Verify();
            Assert.IsTrue(propertyChangedFired);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerViewModel_SearchText")]
        public void ExplorerViewModel_SearchText_WhenNotUpdated_ShouldCallFilter()
        {
            //------------Setup for test--------------------------
            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(model => model.LocalhostServer).Returns(new Mock<IServer>().Object);
            ExplorerViewModel explorerViewModel = new ExplorerViewModel(mockShellViewModel.Object, new Mock<IEventAggregator>().Object);
            var mockEnv1 = new Mock<IEnvironmentViewModel>();
            mockEnv1.Setup(model => model.Filter(It.IsAny<string>())).Verifiable();
            var environment1 = mockEnv1.Object;
            var mockEnv2 = new Mock<IEnvironmentViewModel>();
            mockEnv2.Setup(model => model.Filter(It.IsAny<string>())).Verifiable();
            var environment2 = mockEnv2.Object;
            explorerViewModel.Environments = new List<IEnvironmentViewModel> { environment1, environment2 };
            explorerViewModel.SearchText = "TestValue";
            var propertyChangedFired = false;
            explorerViewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "SearchText")
                {
                    propertyChangedFired = true;
                }
            };
            
            //------------Execute Test---------------------------
            explorerViewModel.SearchText = "TestValue";
            //------------Assert Results-------------------------
            mockEnv1.Verify(model => model.Filter(It.IsAny<string>()),Times.Once());
            mockEnv2.Verify(model => model.Filter(It.IsAny<string>()), Times.Once());
            Assert.IsFalse(propertyChangedFired);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerViewModel_RefreshCommand")]
        public void ExplorerViewModel_RefreshCommand_Execute_CallsLoadOnEachEnvironment()
        {
            //------------Setup for test--------------------------
            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(model => model.LocalhostServer).Returns(new Mock<IServer>().Object);
            IExplorerViewModel explorerViewModel = new ExplorerViewModel(mockShellViewModel.Object, new Mock<IEventAggregator>().Object);
            var mockEnv1 = new Mock<IEnvironmentViewModel>();
            mockEnv1.Setup(model => model.IsConnected).Returns(true);
            mockEnv1.Setup(model => model.Load()).Verifiable();
            var environment1 = mockEnv1.Object;
            var mockEnv2 = new Mock<IEnvironmentViewModel>();
            mockEnv2.Setup(model => model.IsConnected).Returns(true);
            mockEnv2.Setup(model => model.Load()).Verifiable();
            var environment2 = mockEnv2.Object;
            explorerViewModel.Environments = new List<IEnvironmentViewModel> { environment1, environment2 };
            //------------Execute Test---------------------------
            explorerViewModel.RefreshCommand.Execute(null);
            //------------Assert Results-------------------------
            mockEnv1.Verify();
            mockEnv2.Verify();
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ExplorerViewModel_RefreshCommand")]
        public void ExplorerViewModel_RefreshCommand_Execute_CallsLoadOnEachConnectedEnvironment()
        {
            //------------Setup for test--------------------------
            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(model => model.LocalhostServer).Returns(new Mock<IServer>().Object);
            IExplorerViewModel explorerViewModel = new ExplorerViewModel(mockShellViewModel.Object, new Mock<IEventAggregator>().Object);
            var mockEnv1 = new Mock<IEnvironmentViewModel>();
            mockEnv1.Setup(model => model.IsConnected).Returns(true);
            mockEnv1.Setup(model => model.Load()).Verifiable();
            var environment1 = mockEnv1.Object;
            var mockEnv2 = new Mock<IEnvironmentViewModel>();
            mockEnv2.Setup(model => model.IsConnected).Returns(false);
            mockEnv2.Setup(model => model.Load()).Verifiable();
            var environment2 = mockEnv2.Object;
            explorerViewModel.Environments = new List<IEnvironmentViewModel> { environment1, environment2 };
            //------------Execute Test---------------------------
            explorerViewModel.RefreshCommand.Execute(null);
            //------------Assert Results-------------------------
            mockEnv1.Verify();
            mockEnv2.Verify(model => model.Load(),Times.Never());
        }
    }
}
