using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Dev2.Activities.Designers2.DataMerge;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers.Tests.DataMerge
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class DataMergeDesignerViewModelTests
    {
        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DataMergeDesignerViewModel_Constructor")]
        public void DataMergeDesignerViewModel_Constructor__ModelItemIsValid_ListHasFourItems()
        {
            var items = new List<DataMergeDTO> { new DataMergeDTO("", "None", "", 0, "", "Left", false) };
            var viewModel = new DataMergeDesignerViewModel(CreateModelItem(items));
            var expected = new List<string> { "None", "Index", "Chars", "New Line", "Tab" };

            CollectionAssert.AreEqual(expected, viewModel.ItemsList.ToList());
        } //AlignmentTypes

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DataMergeDesignerViewModel_Constructor")]
        public void DataMergeDesignerViewModel_Constructor__ModelItemIsValid_AlignmentTypesHasTwoItems()
        {
            var items = new List<DataMergeDTO> { new DataMergeDTO("", "None", "", 0, "", "Left", false) };
            var viewModel = new DataMergeDesignerViewModel(CreateModelItem(items));
            var expected = new List<string> { "Left", "Right" };

            CollectionAssert.AreEqual(expected, viewModel.AlignmentTypes.ToList());
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DataMergeDesignerViewModel_Constructor")]
        public void DataMergeDesignerViewModel_Constructor__ModelItemIsValid_CollectionNameIsSetToMergeCollection()
        {
            var items = new List<DataMergeDTO> { new DataMergeDTO() };
            var viewModel = new DataMergeDesignerViewModel(CreateModelItem(items));
            Assert.AreEqual("MergeCollection", viewModel.CollectionName);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DataMergeDesignerViewModel_Constructor")]
        public void DataMergeDesignerViewModel_Constructor_ModelItemIsValid_MergeCollectionHasTwoItems()
        {
            var modelItem = ModelItemUtils.CreateModelItem(new DsfDataMergeActivity());
            var viewModel = new DataMergeDesignerViewModel(modelItem);
            dynamic mi = viewModel.ModelItem;
            Assert.AreEqual(2, mi.MergeCollection.Count);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DataMergeDesignerViewModel_Constructor")]
        public void DataMergeDesignerViewModel_Constructor_ModelItemIsInitializedWith4Items_MergeCollectionHasFourItems()
        {
            var items = new List<DataMergeDTO>
            {
                new DataMergeDTO("", "None", "", 0, "", "Left"),
                new DataMergeDTO("", "None", "", 0, "", "Left"),
                new DataMergeDTO("", "None", "", 0, "", "Left"),
                new DataMergeDTO("", "None", "", 0, "", "Left")
            };
            var viewModel = new DataMergeDesignerViewModel(CreateModelItem(items));
            dynamic mi = viewModel.ModelItem;
            Assert.AreEqual(5, mi.MergeCollection.Count);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DataMergeDesignerViewModel_OnMergeTypeChanged")]
        public void DataMergeDesignerViewModel_OnMergeTypeChanged_SetIndexToMergeTypeToNone_EnableAtIsSetToFalse()
        {
            VerifyMergeTypeAgaintsEnabledAt("None", false);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DataMergeDesignerViewModel_OnMergeTypeChanged")]
        public void DataMergeDesignerViewModel_OnMergeTypeChanged_SetIndexToMergeTypeToTab_EnableAtIsSetToFalse()
        {
            VerifyMergeTypeAgaintsEnabledAt("Tab", false);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DataMergeDesignerViewModel_OnMergeTypeChanged")]
        public void DataMergeDesignerViewModel_OnMergeTypeChanged_SetIndexToMergeTypeToNewLine_EnableAtIsSetToFalse()
        {
            VerifyMergeTypeAgaintsEnabledAt("New Line", false);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DataMergeDesignerViewModel_OnMergeTypeChanged")]
        public void DataMergeDesignerViewModel_OnMergeTypeChanged_SetIndexToMergeTypeToIndex_EnableAtIsSetToTrue()
        {
            VerifyMergeTypeAgaintsEnabledAt("Index", true);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DataMergeDesignerViewModel_OnMergeTypeChanged")]
        public void DataMergeDesignerViewModel_OnMergeTypeChanged_SetIndexToMergeTypeToChars_EnableAtIsSetToTrue()
        {
            VerifyMergeTypeAgaintsEnabledAt("Chars", true);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("DataMergeDesignerViewModel_OnMergeTypeChanged")]
        public void DataMergeDesignerViewModel_OnMergeTypeChanged_SetMergeTypeToChars_EnablePaddingIsSetToFalse()
        {
            VerifyMergeTypeAgaintsEnabledApdding("Chars", false);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("DataMergeDesignerViewModel_OnMergeTypeChanged")]
        public void DataMergeDesignerViewModel_OnMergeTypeChanged_SetMergeTypeToIndex_EnablePaddingIsSetToTrue()
        {
            VerifyMergeTypeAgaintsEnabledApdding("Index", true);
        }

        static void VerifyMergeTypeAgaintsEnabledApdding(string mergeType, bool expectedEnablePadding)
        {
            var items = new List<DataMergeDTO> { new DataMergeDTO("", mergeType, "", 0, "", "Left") };
            var viewModel = new DataMergeDesignerViewModel(CreateModelItem(items));
            viewModel.MergeTypeUpdatedCommand.Execute(0);
            dynamic mi = viewModel.ModelItemCollection[0];
            var at = mi.At as string;
            var actualEnablePadding = mi.EnablePadding as bool?;
            Assert.AreEqual("", at);
            Assert.AreEqual(expectedEnablePadding, actualEnablePadding);
        }

        static void VerifyMergeTypeAgaintsEnabledAt(string mergeType, bool expectedEnableAt)
        {
            var items = new List<DataMergeDTO> { new DataMergeDTO("", mergeType, "", 0, "", "Left") };
            var viewModel = new DataMergeDesignerViewModel(CreateModelItem(items));
            viewModel.MergeTypeUpdatedCommand.Execute(0);
            dynamic mi = viewModel.ModelItemCollection[0];
            var at = mi.At as string;
            var actualEnableAt = mi.EnableAt as bool?;
            Assert.AreEqual("", at);
            Assert.AreEqual(expectedEnableAt, actualEnableAt);
        }

        static ModelItem CreateModelItem(IEnumerable<DataMergeDTO> items, string displayName = "Merge")
        {
            var modelItem = ModelItemUtils.CreateModelItem(new DsfDataMergeActivity());
            modelItem.SetProperty("DisplayName", displayName);

            var modelItemCollection = modelItem.Properties["MergeCollection"].Collection;
            foreach(var dto in items)
            {
                modelItemCollection.Add(dto);
            }
            return modelItem;
        }



        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DataMergeDesignerViewModel_ValidateCollectionItem")]
        public void DataMergeDesignerViewModel_ValidateCollectionItem_ValidatesPropertiesOfDTO()
        {
            //------------Setup for test--------------------------
            var mi = ModelItemUtils.CreateModelItem(new DsfDataMergeActivity());
            mi.SetProperty("DisplayName", "Merge");

            var dto = new DataMergeDTO("", DataMergeDTO.MergeTypeIndex, "a]]", 0, "ab", "Left");

            // ReSharper disable PossibleNullReferenceException
            var miCollection = mi.Properties["MergeCollection"].Collection;
            var dtoModelItem = miCollection.Add(dto);
            // ReSharper restore PossibleNullReferenceException

            var viewModel = new DataMergeDesignerViewModel(mi);

            //------------Execute Test---------------------------
            viewModel.Validate();

            //------------Assert Results-------------------------
            Assert.AreEqual(2, viewModel.Errors.Count);

            StringAssert.Contains(viewModel.Errors[0].Message, "'Using' - Invalid expression: opening and closing brackets don't match");
            Verify_IsFocused(dtoModelItem, viewModel.Errors[0].Do, "IsAtFocused");

            StringAssert.Contains(viewModel.Errors[1].Message, "'Padding' must be a single character");
            Verify_IsFocused(dtoModelItem, viewModel.Errors[1].Do, "IsPaddingFocused");
        }

        void Verify_IsFocused(ModelItem modelItem, Action doError, string isFocusedPropertyName)
        {
            Assert.IsFalse(modelItem.GetProperty<bool>(isFocusedPropertyName));
            doError.Invoke();
            Assert.IsTrue(modelItem.GetProperty<bool>(isFocusedPropertyName));
        }

    }
}
