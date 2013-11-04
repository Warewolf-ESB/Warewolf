using System.Diagnostics.CodeAnalysis;
using Dev2.Composition;
using Dev2.Studio.Enums;
using Dev2.Studio.Utils;
using Dev2.Studio.ViewModels.Workflow;
using Microsoft.VisualStudio.TestTools.UnitTesting;using System.Diagnostics.CodeAnalysis;

namespace Dev2.Core.Tests
{
    [TestClass][ExcludeFromCodeCoverage]
    public class DsfActivityDropUtilsTests
    {
        [TestInitialize]
        public void MyTestInitialize()
        {            
            ImportService.CurrentContext = CompositionInitializer.ExplorerViewModelTest();
        }

        [TestMethod]
        [Ignore] //Bad Mocking Needs to be fixed... See MainViewModel OnImportsStatisfied
        public void CreateADsfActivityDropViewModelWithWorkflowsOnlyExpectedNavigationViewModelPropertySetToWorkflowsOnly()
        {
            //DsfActivityDropViewModel vm = DsfActivityDropUtils.DetermineDropActivityType("DsfWorkflowActivity");
            //Assert.IsTrue(vm.ExplorerViewModel.NavigationViewModel.DsfActivityType == enDsfActivityType.Workflow);
        }

        [TestMethod]
        [Ignore] //Bad Mocking Needs to be fixed... See MainViewModel OnImportsStatisfied
        public void CreateADsfActivityDropViewModelWithServicesOnlyExpectedNavigationViewModelPropertySetToServicesOnly()
        {
            //DsfActivityDropViewModel vm = DsfActivityDropUtils.DetermineDropActivityType("DsfServiceActivity");
            //Assert.IsTrue(vm.ExplorerViewModel.NavigationViewModel.DsfActivityType == enDsfActivityType.Service);
        }
    }
}
