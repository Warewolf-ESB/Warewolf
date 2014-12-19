using System.Collections.Generic;
using Dev2.Common.Interfaces.Studio.ViewModels;
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
        public void ExplorerViewModel_Filter_ShouldCallFilterOnEachEnvironment()
        {
            //------------Setup for test--------------------------
            IExplorerViewModel explorerViewModel = new ExplorerViewModel();
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
    }
}
