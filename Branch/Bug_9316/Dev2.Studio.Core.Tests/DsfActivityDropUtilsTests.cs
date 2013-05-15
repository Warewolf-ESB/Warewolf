using Dev2.Composition;
using Dev2.Studio.Enums;
using Dev2.Studio.Utils;
using Dev2.Studio.ViewModels.Workflow;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests
{
    [TestClass]
    public class DsfActivityDropUtilsTests
    {
        [TestInitialize]
        public void MyTestInitialize()
        {            
            ImportService.CurrentContext = CompositionInitializer.ExplorerViewModelTest();
        }

        [TestMethod]
        [Ignore]
        public void CreateADsfActivityDropViewModelWithWorkflowsOnlyExpectedNavigationViewModelPropertySetToWorkflowsOnly()
        {
            DsfActivityDropViewModel vm = DsfActivityDropUtils.DetermineDropActivityType("DsfWorkflowActivity");
            Assert.IsTrue(vm.ExplorerViewModel.NavigationViewModel.DsfActivityType == enDsfActivityType.Workflow);
        }

        [TestMethod]
        public void CreateADsfActivityDropViewModelWithServicesOnlyExpectedNavigationViewModelPropertySetToServicesOnly()
        {
            DsfActivityDropViewModel vm = DsfActivityDropUtils.DetermineDropActivityType("DsfServiceActivity");
            Assert.IsTrue(vm.ExplorerViewModel.NavigationViewModel.DsfActivityType == enDsfActivityType.Service);
        }
    }
}
