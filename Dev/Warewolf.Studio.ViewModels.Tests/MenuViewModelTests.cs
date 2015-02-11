using System;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Studio.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class MenuViewModelTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("MenuViewModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void MenuViewModel_Constructor_NullShellViewModel_ExceptionThrown()
        {
            //------------Setup for test--------------------------
            
            //------------Execute Test---------------------------
            new MenuViewModel(null);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("MenuViewModel_Constructor")]
        public void MenuViewModel_Constructor_ShouldSetupNewCommand()
        {
            //------------Setup for test--------------------------
            ResourceType? resourceTypeParameter = null;
            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(model => model.NewResource(It.IsAny<ResourceType?>(), Guid.Empty)).Callback((ResourceType? resourceType) => resourceTypeParameter = resourceType);
            var menuViewModel = new MenuViewModel(mockShellViewModel.Object);
            //------------Execute Test---------------------------
            menuViewModel.NewCommand.Execute(ResourceType.Unknown);
            //------------Assert Results-------------------------
            mockShellViewModel.Verify(model => model.NewResource(It.IsAny<ResourceType>(), Guid.Empty), Times.Once());
            Assert.AreEqual(ResourceType.Unknown,resourceTypeParameter);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("MenuViewModel_Constructor")]
        public void MenuViewModel_Constructor_Workflow_ShouldNewCommandWithWorkflowParameter()
        {
            //------------Setup for test--------------------------
            ResourceType? resourceTypeParameter = null;
            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(model => model.NewResource(It.IsAny<ResourceType?>(), Guid.Empty)).Callback((ResourceType? resourceType) => resourceTypeParameter = resourceType);
            var menuViewModel = new MenuViewModel(mockShellViewModel.Object);
            //------------Execute Test---------------------------
            menuViewModel.NewCommand.Execute(ResourceType.WorkflowService);
            //------------Assert Results-------------------------
            mockShellViewModel.Verify(model => model.NewResource(It.IsAny<ResourceType>(), Guid.Empty), Times.Once());
            Assert.AreEqual(ResourceType.WorkflowService, resourceTypeParameter);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("MenuViewModel_Constructor")]
        public void MenuViewModel_Constructor_ShouldSetupDeployCommand()
        {
            //------------Setup for test--------------------------
            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(model => model.DeployService(It.IsAny<IExplorerItemViewModel>())).Verifiable();
            var menuViewModel = new MenuViewModel(mockShellViewModel.Object);
            //------------Execute Test---------------------------
            menuViewModel.DeployCommand.Execute(null);
            //------------Assert Results-------------------------
            mockShellViewModel.Verify();
        } 
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("MenuViewModel_Constructor")]
        public void MenuViewModel_Constructor_ShouldSetupSaveCommand()
        {
            //------------Setup for test--------------------------
            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(model => model.SaveService()).Verifiable();
            var menuViewModel = new MenuViewModel(mockShellViewModel.Object);
            //------------Execute Test---------------------------
            menuViewModel.SaveCommand.Execute(null);
            //------------Assert Results-------------------------
            mockShellViewModel.Verify();
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("MenuViewModel_Constructor")]
        public void MenuViewModel_Constructor_ShouldSetupOpenSettingsCommand()
        {
            //------------Setup for test--------------------------
            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(model => model.OpenSettings()).Verifiable();
            var menuViewModel = new MenuViewModel(mockShellViewModel.Object);
            //------------Execute Test---------------------------
            menuViewModel.OpenSettingsCommand.Execute(null);
            //------------Assert Results-------------------------
            mockShellViewModel.Verify();
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("MenuViewModel_Constructor")]
        public void MenuViewModel_Constructor_ShouldSetupOpenSchedulerCommand()
        {
            //------------Setup for test--------------------------
            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(model => model.OpenScheduler()).Verifiable();
            var menuViewModel = new MenuViewModel(mockShellViewModel.Object);
            //------------Execute Test---------------------------
            menuViewModel.OpenSchedulerCommand.Execute(null);
            //------------Assert Results-------------------------
            mockShellViewModel.Verify();
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("MenuViewModel_Constructor")]
        public void MenuViewModel_Constructor_ShouldSetupExecuteServiceCommand()
        {
            //------------Setup for test--------------------------
            var mockShellViewModel = new Mock<IShellViewModel>();
            mockShellViewModel.Setup(model => model.ExecuteService()).Verifiable();
            var menuViewModel = new MenuViewModel(mockShellViewModel.Object);
            //------------Execute Test---------------------------
            menuViewModel.ExecuteServiceCommand.Execute(null);
            //------------Assert Results-------------------------
            mockShellViewModel.Verify();
        }
    }
}
