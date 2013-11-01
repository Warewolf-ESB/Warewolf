using System.Activities.Presentation.Model;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers.Tests.FindIndex
{
    [TestClass][ExcludeFromCodeCoverage]
    public class FindIndexDesignerViewModelTests
    {
        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("FindIndexDesignerViewModel_Constructor")]
        public void FindIndexDesignerViewModel_Constructor_ModelItemIsValid_SelectedIndexIsInitialized()
        {
            var modelItem = CreateModelItem();
            var viewModel = new TestFindIndexDesignerViewModel(modelItem);
            Assert.AreEqual("First Occurrence", viewModel.Index);
            Assert.AreEqual("First Occurrence", viewModel.SelectedIndex);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("FindIndexDesignerViewModel_Constructor")]
        public void FindIndexDesignerViewModel_Constructor_ModelItemIsValid_IndexListHasThreeItems()
        {
            var modelItem = CreateModelItem();
            var viewModel = new TestFindIndexDesignerViewModel(modelItem);
            Assert.AreEqual(3, viewModel.IndexList.Count);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("FindIndexDesignerViewModel_SetSelectedIndex")]
        public void FindIndexDesignerViewModel_SetSelectedIndex_ValidIndex_IndexOnModelItemIsAlsoSet()
        {
            var modelItem = CreateModelItem();
            var viewModel = new TestFindIndexDesignerViewModel(modelItem);
            const string ExpectedValue = "Last Occurrence";
            viewModel.SelectedIndex = ExpectedValue;
            Assert.AreEqual(ExpectedValue, viewModel.Index);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("FindDirectionDesignerViewModel_Constructor")]
        public void FindDirectionDesignerViewModel_Constructor_ModelItemIsValid_SelectedDirectionIsInitialized()
        {
            var modelItem = CreateModelItem();
            var viewModel = new TestFindIndexDesignerViewModel(modelItem);
            Assert.AreEqual("Left to Right", viewModel.Direction);
            Assert.AreEqual("Left to Right", viewModel.SelectedDirection);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("FindDirectionDesignerViewModel_Constructor")]
        public void FindDirectionDesignerViewModel_Constructor_ModelItemIsValid_DirectionListHasTowItems()
        {
            var modelItem = CreateModelItem();
            var viewModel = new TestFindIndexDesignerViewModel(modelItem);
            Assert.AreEqual(2, viewModel.DirectionList.Count);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("FindDirectionDesignerViewModel_SetSelectedDirection")]
        public void FindDirectionDesignerViewModel_SetSelectedDirection_ValidDirection_DirectionOnModelItemIsAlsoSet()
        {
            var modelItem = CreateModelItem();
            var viewModel = new TestFindIndexDesignerViewModel(modelItem);
            const string ExpectedValue = "Right to Left";
            viewModel.SelectedDirection = ExpectedValue;
            Assert.AreEqual(ExpectedValue, viewModel.Direction);
        }

        static ModelItem CreateModelItem()
        {
            return ModelItemUtils.CreateModelItem(new DsfIndexActivity());
        }
    }
}