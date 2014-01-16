using System.Activities.Presentation.Model;
using System.Diagnostics.CodeAnalysis;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers.Tests.SortRecords
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class SortRecordsDesignerViewModelTests
    {
        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("SortRecordsDesignerViewModel_Constructor")]
        public void SortRecordsDesignerViewModel_Constructor_ModelItemIsValid_SelectedSortIsInitialized()
        {
            var modelItem = CreateModelItem();
            var viewModel = new TestSortRecordsDesignerViewModel(modelItem);
            Assert.AreEqual("Forward", viewModel.SelectedSort);
            Assert.AreEqual("Forward", viewModel.SelectedSelectedSort);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("SortRecordsDesignerViewModel_Constructor")]
        public void SortRecordsDesignerViewModel_Constructor_ModelItemIsValid_SortOrderTypesHasTwoItems()
        {
            var modelItem = CreateModelItem();
            var viewModel = new TestSortRecordsDesignerViewModel(modelItem);
            Assert.AreEqual(2, viewModel.SortOrderTypes.Count);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("SortRecordsDesignerViewModel_SetSelectedSelectedSort")]
        public void SortRecordsDesignerViewModel_SetSelectedSelectedSort_ValidOrderType_SelectedOrderTypeOnModelItemIsAlsoSet()
        {
            var modelItem = CreateModelItem();
            var viewModel = new TestSortRecordsDesignerViewModel(modelItem);
            const string ExpectedValue = "Backwards";
            viewModel.SelectedSelectedSort = ExpectedValue;
            Assert.AreEqual(ExpectedValue, viewModel.SelectedSort);
        }

        static ModelItem CreateModelItem()
        {
            return ModelItemUtils.CreateModelItem(new DsfSortRecordsActivity());
        }
    }
}