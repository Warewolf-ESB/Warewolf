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
// ReSharper disable InconsistentNaming
        public void SortRecordsDesignerViewModel_Constructor_ModelItemIsValid_SelectedSortIsInitialized()
// ReSharper restore InconsistentNaming
        {
            var modelItem = CreateModelItem();
            var viewModel = new TestSortRecordsDesignerViewModel(modelItem);
            Assert.AreEqual("Forward", viewModel.SelectedSort);
            Assert.AreEqual("Forward", viewModel.SelectedSelectedSort);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("SortRecordsDesignerViewModel_Constructor")]
// ReSharper disable InconsistentNaming
        public void SortRecordsDesignerViewModel_Constructor_ModelItemIsValid_SortOrderTypesHasTwoItems()
// ReSharper restore InconsistentNaming
        {
            var modelItem = CreateModelItem();
            var viewModel = new TestSortRecordsDesignerViewModel(modelItem);
            Assert.AreEqual(2, viewModel.SortOrderTypes.Count);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("SortRecordsDesignerViewModel_SetSelectedSelectedSort")]
// ReSharper disable InconsistentNaming
        public void SortRecordsDesignerViewModel_SetSelectedSelectedSort_ValidOrderType_SelectedOrderTypeOnModelItemIsAlsoSet()
// ReSharper restore InconsistentNaming
        {
            var modelItem = CreateModelItem();
            var viewModel = new TestSortRecordsDesignerViewModel(modelItem);
            const string ExpectedValue = "Backwards";
            viewModel.SelectedSelectedSort = ExpectedValue;
            Assert.AreEqual(ExpectedValue, viewModel.SelectedSort);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("SortRecordsDesignerViewModel_RehydratesSortOrder")]
// ReSharper disable InconsistentNaming
        public void SortRecordsDesignerViewModel_RehydratesSortOrder_ValidOrderType_ExpectUnderlyingValueBackwards()
// ReSharper restore InconsistentNaming
        {
            var modelItem = CreateModelItem();
            modelItem.SetProperty("SelectedSort","Backwards");
            var viewModel = new TestSortRecordsDesignerViewModel(modelItem);
            const string ExpectedValue = "Backwards";
          
            Assert.AreEqual(ExpectedValue, viewModel.SelectedSort);
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("SortRecordsDesignerViewModel_Validate")]
// ReSharper disable InconsistentNaming
        public void SortRecordsDesignerViewModel_Validate_ValidatesSingleRegion_ExpectNoErrors()
// ReSharper restore InconsistentNaming
        {
            var modelItem = CreateModelItem();
            modelItem.SetProperty("SelectedSort", "Backwards");
            modelItem.SetProperty("SortField","[[rec().a]]");
            var viewModel = new TestSortRecordsDesignerViewModel(modelItem);
            viewModel.Validate();

            Assert.IsNull(viewModel.Errors);
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("SortRecordsDesignerViewModel_Validate")]
// ReSharper disable InconsistentNaming
        public void SortRecordsDesignerViewModel_Validate_ValidatesSingleRegion_ExpectErrors()
// ReSharper restore InconsistentNaming
        {
            var modelItem = CreateModelItem();
            modelItem.SetProperty("SelectedSort", "Backwards");
            modelItem.SetProperty("SortField", "[[rec.a()]],[[rec.b()]]");
            var viewModel = new TestSortRecordsDesignerViewModel(modelItem);
            viewModel.Validate();
            Assert.AreEqual(1, viewModel.Errors.Count);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("SortRecordsDesignerViewModel_Validate")]
// ReSharper disable InconsistentNaming
        public void SortRecordsDesignerViewModel_Validate_ValidatesSingleRegion_NoErrors()
// ReSharper restore InconsistentNaming
        {
            var modelItem = CreateModelItem();
            modelItem.SetProperty("SelectedSort", "Backwards");
            modelItem.SetProperty("SortField", "[[rec([[rec.b()]]).a]]");
            var viewModel = new TestSortRecordsDesignerViewModel(modelItem);
            viewModel.Validate();
            Assert.IsNull(viewModel.Errors);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("SortRecordsDesignerViewModel_RehydratesSortOrder")]
// ReSharper disable InconsistentNaming
        public void SortRecordsDesignerViewModel_RehydratesSortOrder_ValidOrderType_ExpectUnderlyingValueForwards()
// ReSharper restore InconsistentNaming
        {
            var modelItem = CreateModelItem();
            modelItem.SetProperty("SelectedSort", "Forward");

            var viewModel = new TestSortRecordsDesignerViewModel(modelItem);
            const string ExpectedValue = "Forward";

            Assert.AreEqual(ExpectedValue, viewModel.SelectedSort);
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("SortRecordsDesignerViewModel_RehydratesSortOrder")]
// ReSharper disable InconsistentNaming
        public void SortRecordsDesignerViewModel_RehydratesSortOrder_ValidOrderType_ExpectUnderlyingValueEmpty()
// ReSharper restore InconsistentNaming
        {
            var modelItem = CreateModelItem();
            modelItem.SetProperty("SelectedSort", "");
            var viewModel = new TestSortRecordsDesignerViewModel(modelItem);
            const string ExpectedValue = "Forward";

            Assert.AreEqual(ExpectedValue, viewModel.SelectedSort);
        }
        static ModelItem CreateModelItem()
        {
            return ModelItemUtils.CreateModelItem(new DsfSortRecordsActivity());
        }
    }
}