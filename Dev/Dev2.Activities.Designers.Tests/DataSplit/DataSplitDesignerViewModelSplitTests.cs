using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Dev2.Activities.Designers2.DataSplit;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers.Tests.DataSplit
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class DataSplitDesignerViewModelSplitTests
    {
        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DataSplitDesignerViewModel_Constructor")]
        public void DataSplitDesignerViewModel_Constructor__ModelItemIsValid_ListHasFourItems()
        {
            var items = new List<DataSplitDTO> { new DataSplitDTO() };
            var viewModel = new DataSplitDesignerViewModel(CreateModelItem(items));
            Assert.AreEqual(6, viewModel.ItemsList.Count);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DataSplitDesignerViewModel_Constructor")]
        public void DataSplitDesignerViewModel_Constructor__ModelItemIsValid_CollectionNameIsSetToResultsCollection()
        {
            var items = new List<DataSplitDTO> { new DataSplitDTO() };
            var viewModel = new DataSplitDesignerViewModel(CreateModelItem(items));
            Assert.AreEqual("ResultsCollection", viewModel.CollectionName);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DataSplitDesignerViewModel_Constructor")]
        public void DataSplitDesignerViewModel_Constructor_ModelItemIsValid_ResultsCollectionHasTwoItems()
        {
            var modelItem = ModelItemUtils.CreateModelItem(new DsfDataSplitActivity());
            var viewModel = new DataSplitDesignerViewModel(modelItem);
            dynamic mi = viewModel.ModelItem;
            Assert.AreEqual(2, mi.ResultsCollection.Count);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DataSplitDesignerViewModel_Constructor")]
        public void DataSplitDesignerViewModel_Constructor_ModelItemIsInitializedWith4Items_ResultsCollectionHasFourItems()
        {
            var items = new List<DataSplitDTO>
            {
                new DataSplitDTO("", "None", "", 0),
                new DataSplitDTO("", "None", "", 0),
                new DataSplitDTO("", "None", "", 0),
                new DataSplitDTO("", "None", "", 0)
            };
            var viewModel = new DataSplitDesignerViewModel(CreateModelItem(items));
            dynamic mi = viewModel.ModelItem;
            Assert.AreEqual(5, mi.ResultsCollection.Count);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DataSplitDesignerViewModel_OnSplitTypeChanged")]
        public void DataSplitDesignerViewModel_OnSplitTypeChanged_SetIndexToSplitTypeToNone_EnableAtIsSetToFalse()
        {
            VerifySplitTypeAgaintsEnabledAt("None", false);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DataSplitDesignerViewModel_OnSplitTypeChanged")]
        public void DataSplitDesignerViewModel_OnSplitTypeChanged_SetIndexToSplitTypeToTab_EnableAtIsSetToFalse()
        {
            VerifySplitTypeAgaintsEnabledAt("Tab", false);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DataSplitDesignerViewModel_OnSplitTypeChanged")]
        public void DataSplitDesignerViewModel_OnSplitTypeChanged_SetIndexToSplitTypeToNewLine_EnableAtIsSetToFalse()
        {
            VerifySplitTypeAgaintsEnabledAt("New Line", false);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DataSplitDesignerViewModel_OnSplitTypeChanged")]
        public void DataSplitDesignerViewModel_OnSplitTypeChanged_SetIndexToSplitTypeToIndex_EnableAtIsSetToTrue()
        {
            VerifySplitTypeAgaintsEnabledAt("Index", true);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DataSplitDesignerViewModel_OnSplitTypeChanged")]
        public void DataSplitDesignerViewModel_OnSplitTypeChanged_SetIndexToSplitTypeToChars_EnableAtIsSetToTrue()
        {
            VerifySplitTypeAgaintsEnabledAt("Chars", true);
        }

        static void VerifySplitTypeAgaintsEnabledAt(string SplitType, bool expectedEnableAt)
        {
            var items = new List<DataSplitDTO> { new DataSplitDTO("", SplitType, "", 0) };
            var viewModel = new DataSplitDesignerViewModel(CreateModelItem(items));
            viewModel.SplitTypeUpdatedCommand.Execute(0);
            dynamic mi = viewModel.ModelItemCollection[0];
            var at = mi.At as string;
            var actualEnableAt = mi.EnableAt as bool?;
            Assert.AreEqual("", at);
            Assert.AreEqual(expectedEnableAt, actualEnableAt);
        }

        static ModelItem CreateModelItem(IEnumerable<DataSplitDTO> items, string displayName = "Split")
        {
            var modelItem = ModelItemUtils.CreateModelItem(new DsfDataSplitActivity());
            modelItem.SetProperty("DisplayName", displayName);

            var modelItemCollection = modelItem.Properties["ResultsCollection"].Collection;
            foreach(var dto in items)
            {
                modelItemCollection.Add(dto);
            }
            return modelItem;
        }
    }
}
