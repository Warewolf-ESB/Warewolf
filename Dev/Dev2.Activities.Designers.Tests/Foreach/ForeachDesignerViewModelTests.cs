using System.Activities.Presentation.Model;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using Dev2.Data.Enums;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers.Tests.Foreach
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class ForeachDesignerViewModelTests
    {
        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ForeachDesignerViewModel_Constructor")]
        public void ForeachDesignerViewModel_Constructor_ModelItemIsValid_SelectedForeachTypeIsInitialized()
        {
            var modelItem = CreateModelItem();
            var viewModel = new TestForeachDesignerViewModel(modelItem);
            Assert.AreEqual(enForEachType.InRange, viewModel.ForEachType);
            Assert.AreEqual("* in Range", viewModel.SelectedForeachType);
            Assert.AreEqual(viewModel.FromVisibility, Visibility.Visible);
            Assert.AreEqual(viewModel.ToVisibility, Visibility.Visible);
            Assert.AreEqual(viewModel.CsvIndexesVisibility, Visibility.Hidden);
            Assert.AreEqual(viewModel.NumberVisibility, Visibility.Hidden);
            Assert.AreEqual(viewModel.RecordsetVisibility, Visibility.Hidden);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ForeachDesignerViewModel_Constructor")]
        public void ForeachDesignerViewModel_Constructor_ModelItemIsValid_ForeachTypesHasThreeItems()
        {
            var modelItem = CreateModelItem();
            var viewModel = new TestForeachDesignerViewModel(modelItem);
            Assert.AreEqual(4, viewModel.ForeachTypes.Count);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ForeachDesignerViewModel_SetSelectedForeachType")]
        public void ForeachDesignerViewModel_SetSelectedForeachTypeToinCsv_ValidForeachType_ForeachTypeOnModelItemIsAlsoSet()
        {
            var modelItem = CreateModelItem();
            var viewModel = new TestForeachDesignerViewModel(modelItem);
            const string ExpectedValue = "* in CSV";
            viewModel.SelectedForeachType = ExpectedValue;
            Assert.AreEqual(enForEachType.InCSV, viewModel.ForEachType);
            Assert.AreEqual(viewModel.FromVisibility, Visibility.Hidden);
            Assert.AreEqual(viewModel.ToVisibility, Visibility.Hidden);
            Assert.AreEqual(viewModel.CsvIndexesVisibility, Visibility.Visible);
            Assert.AreEqual(viewModel.NumberVisibility, Visibility.Hidden);
            Assert.AreEqual(viewModel.RecordsetVisibility, Visibility.Hidden);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ForeachDesignerViewModel_SetSelectedForeachType")]
        public void ForeachDesignerViewModel_SetSelectedForeachTypeToinRecordset_ValidForeachType_ForeachTypeOnModelItemIsAlsoSet()
        {
            var modelItem = CreateModelItem();
            var viewModel = new TestForeachDesignerViewModel(modelItem);
            const string ExpectedValue = "* in Recordset";
            viewModel.SelectedForeachType = ExpectedValue;
            Assert.AreEqual(enForEachType.InRecordset, viewModel.ForEachType);
            Assert.AreEqual(viewModel.FromVisibility, Visibility.Hidden);
            Assert.AreEqual(viewModel.ToVisibility, Visibility.Hidden);
            Assert.AreEqual(viewModel.CsvIndexesVisibility, Visibility.Visible);
            Assert.AreEqual(viewModel.NumberVisibility, Visibility.Hidden);
            Assert.AreEqual(viewModel.RecordsetVisibility, Visibility.Visible);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ForeachDesignerViewModel_SetSelectedForeachType")]
        public void ForeachDesignerViewModel_SetSelectedForeachTypeToNoOfExecution_ValidForeachType_ForeachTypeOnModelItemIsAlsoSet()
        {
            var modelItem = CreateModelItem();
            var viewModel = new TestForeachDesignerViewModel(modelItem);
            const string ExpectedValue = "No. of Executes";
            viewModel.SelectedForeachType = ExpectedValue;
            Assert.AreEqual(enForEachType.NumOfExecution, viewModel.ForEachType);
            Assert.AreEqual(viewModel.FromVisibility, Visibility.Hidden);
            Assert.AreEqual(viewModel.ToVisibility, Visibility.Hidden);
            Assert.AreEqual(viewModel.CsvIndexesVisibility, Visibility.Hidden);
            Assert.AreEqual(viewModel.NumberVisibility, Visibility.Visible);
            Assert.AreEqual(viewModel.RecordsetVisibility, Visibility.Hidden);
        }


        static ModelItem CreateModelItem()
        {
            return ModelItemUtils.CreateModelItem(new DsfForEachActivity());
        }
    }
}