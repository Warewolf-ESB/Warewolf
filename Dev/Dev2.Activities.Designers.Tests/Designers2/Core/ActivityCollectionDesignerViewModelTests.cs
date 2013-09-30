using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Data;
using Dev2.Activities.Designers.Tests.Designers2.Core.Stubs;
using Dev2.Activities.Designers2.Core;
using Dev2.Providers.Errors;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers.Tests.Designers2.Core
{
    [TestClass]
    public class ActivityCollectionDesignerViewModelTests
    {
        [TestMethod]
        [TestCategory("ActivityCollectionDesignerViewModel_Constructor")]
        [Owner("Ashley Lewis")]
        // ReSharper disable InconsistentNaming
        public void ActivityCollectionDesignerViewModel_Constructor_NoRows_ViewModelConstructed()
        // ReSharper restore InconsistentNaming
        {
            //init
            const string ExpectedCollectionName = "FieldsCollection";
            var collectionNameProp = new Mock<ModelProperty>();
            var dtoListProp = new Mock<ModelProperty>();
            var displayNameProp = new Mock<ModelProperty>();
            var properties = new Dictionary<string, Mock<ModelProperty>>();
            var propertyCollection = new Mock<ModelPropertyCollection>();
            var mockModel = new Mock<ModelItem>();

            collectionNameProp.Setup(p => p.ComputedValue).Returns(ExpectedCollectionName);
            dtoListProp.Setup(p => p.ComputedValue).Returns(new List<ActivityDTO>());
            displayNameProp.Setup(p => p.ComputedValue).Returns("Test Display Name");
            properties.Add("CollectionName", collectionNameProp);
            propertyCollection.Protected().Setup<ModelProperty>("Find", "CollectionName", true).Returns(collectionNameProp.Object);
            propertyCollection.Protected().Setup<ModelProperty>("Find", ExpectedCollectionName, true).Returns(dtoListProp.Object);
            propertyCollection.Protected().Setup<ModelProperty>("Find", "DisplayName", true).Returns(displayNameProp.Object);
            mockModel.Setup(s => s.Properties).Returns(propertyCollection.Object);

            //exe
            var vm = new TestActivityCollectionDesignerViewModel<ActivityDTO>(mockModel.Object);

            //assert
            Assert.AreEqual(2, vm.Items.Count, "Collection view model base cannot initialized 2 blank rows from a model item with no rows");
            Assert.IsInstanceOfType(vm, typeof(ActivityCollectionDesignerViewModel), "Collection view model base cannot initialize");
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ActivityCollectionDesignerViewModel_Constructor")]
        public void ActivityCollectionDesignerViewModel_Constructor_ValidModelItem_InstanceOfQuickVariableInputViewModelCreated()
        {
            var mockModelItem = GenerateMockModelItem();
            var collectionsViewModel = new ActivityDesignerCollectionViewModelDerived(mockModelItem.Object);
            Assert.IsNotNull(collectionsViewModel.QuickVariableInputViewModel);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ActivityCollectionDesignerViewModel_Constructor")]
        public void ActivityCollectionDesignerViewModel_Constructor_ValidModelItem_TwoItemsAreAddedToBackingCollection()
        {
            var mockModelItem = GenerateMockModelItem();
            var collectionsViewModel = new ActivityDesignerCollectionViewModelDerived(mockModelItem.Object);
            Assert.AreEqual(2, collectionsViewModel.Items.Count);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ActivityCollectionDesignerViewModel_AddRows")]
        public void ActivityCollectionDesignerViewModel_AddRows_FieldNameAndFieldValueAreNotNull_ItemsCountIsIncremented()
        {
            var mockModelItem = GenerateMockModelItem();
            var collectionsViewModel = new ActivityDesignerCollectionViewModelDerived(mockModelItem.Object);
            collectionsViewModel.AddRows(new RoutedEventArgs());
            Assert.AreEqual(3, collectionsViewModel.Items.Count);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ActivityCollectionDesignerViewModel_DeleteItem")]
        public void ActivityCollectionDesignerViewModel_DeleteItem_NothingIsSelected_NothingIsDeleted()
        {
            var mockModelItem = GenerateMockModelItem();
            var collectionsViewModel = new ActivityDesignerCollectionViewModelDerived(mockModelItem.Object);
            collectionsViewModel.AddRow();
            var count = collectionsViewModel.Items.Count;
            collectionsViewModel.SelectedIndex = -1;
            collectionsViewModel.DeleteItem();
            Assert.AreEqual(3, count);
            Assert.AreEqual(3, collectionsViewModel.Items.Count);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ActivityCollectionDesignerViewModel_DeleteItem")]
        public void ActivityCollectionDesignerViewModel_DeleteItem_AnItemIsSelected_AnItemIsDeleted()
        {
            var mockModelItem = GenerateMockModelItem();
            var collectionsViewModel = new ActivityDesignerCollectionViewModelDerived(mockModelItem.Object);
            collectionsViewModel.AddRow();
            var count = collectionsViewModel.Items.Count;
            collectionsViewModel.SelectedIndex = 1;
            collectionsViewModel.DeleteItem();
            Assert.AreEqual(3, count);
            Assert.AreEqual(2, collectionsViewModel.Items.Count);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ActivityCollectionDesignerViewModel_DeleteItem")]
        public void ActivityCollectionDesignerViewModel_DeleteItem_DeletesItemAndAddAnEmptyRow()
        {
            var mockModelItem = GenerateMockModelItem();
            var collectionsViewModel = new ActivityDesignerCollectionViewModelDerived(mockModelItem.Object);
            var count = collectionsViewModel.Items.Count;
            collectionsViewModel.SelectedIndex = 0;
            collectionsViewModel.DeleteItem();
            Assert.AreEqual(2, count);
            Assert.AreEqual(2, collectionsViewModel.Items.Count);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ActivityCollectionDesignerViewModel_DeleteCommand")]
        public void ActivityCollectionDesignerViewModel_DeleteItemCommand_ExecutedWithAValidIndex_AnItemAtSpecificiedIndexIsRemoved()
        {
            var mockModelItem = GenerateMockModelItem();
            var collectionsViewModel = new ActivityDesignerCollectionViewModelDerived(mockModelItem.Object);
            collectionsViewModel.AddRow();
            var count = collectionsViewModel.Items.Count;
            collectionsViewModel.DeleteItemCommand.Execute(1);
            Assert.AreEqual(3, count);
            Assert.AreEqual(2, collectionsViewModel.Items.Count);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ActivityCollectionDesignerViewModel_InsertItem")]
        public void ActivityCollectionDesignerViewModel_InsertItem_FieldNameAndFieldValueAreNotNull_ItemsCountIsIncremented()
        {
            var mockModelItem = GenerateMockModelItem();
            var collectionsViewModel = new ActivityDesignerCollectionViewModelDerived(mockModelItem.Object);
            collectionsViewModel.SelectedIndex = 1;
            collectionsViewModel.InsertItem();
            Assert.AreEqual(3, collectionsViewModel.Items.Count);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ActivityCollectionDesignerViewModel_RomoveRow")]
        public void ActivityCollectionDesignerViewModel_RemoveRow_NotBlankRow_NothingIsDeleted()
        {
            var mockModelItem = GenerateMockModelItem();
            var collectionsViewModel = new ActivityDesignerCollectionViewModelDerived(mockModelItem.Object);
            collectionsViewModel.RemoveRow();
            Assert.AreEqual(2, collectionsViewModel.Items.Count);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ActivityCollectionDesignerViewModel_UpdateRows")]
        public void ActivityCollectionDesignerViewModel_UpdateRows_NotBlankRow_NothingIsDeleted()
        {
            const int itemsCount = 4;
            var mockModelItem = GenerateMockModelItem(itemsCount);
            var collectionsViewModel = new ActivityDesignerCollectionViewModelDerived(mockModelItem.Object);
            collectionsViewModel.UpdateRows();
            Assert.AreEqual(itemsCount, collectionsViewModel.Items.Count);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ActivityCollectionDesignerViewModel_Validate")]
        public void ActivityCollectionDesignerViewModel_Validate_NoErrorsFound_IsValidIsTrue()
        {
            const int itemsCount = 4;
            var mockModelItem = GenerateMockModelItem(itemsCount);
            var collectionsViewModel = new ActivityDesignerCollectionViewModelDerived(mockModelItem.Object);
            collectionsViewModel.Validate();
            Assert.IsTrue(collectionsViewModel.IsValid);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ActivityCollectionDesignerViewModel_ExecuteShowErrorsCommand")]
        public void ActivityCollectionDesignerViewModel_ExecuteShowErrorsCommand_ShowErrorsIsTrue_ShowErrorsIsSetToFalse()
        {
            var mockModelItem = GenerateMockModelItem();
            var collectionsViewModel = new ActivityDesignerCollectionViewModelDerived(mockModelItem.Object)
            {
                Errors = new List<IActionableErrorInfo> { new ActionableErrorInfo() }
            };
            collectionsViewModel.ShowErrorsToggleCommand.Execute(null);
            Assert.IsFalse(collectionsViewModel.ShowErrors);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ActivityCollectionDesignerViewModel_ExecuteShowHelpToggleCommand")]
        public void ActivityCollectionDesignerViewModel_ExecuteShowHelpToggleCommand_ShowHelpIsFalse_ShowHelpIsSetToTrue()
        {
            var mockModelItem = GenerateMockModelItem();
            var collectionsViewModel = new ActivityDesignerCollectionViewModelDerived(mockModelItem.Object);
            var showHelpInitialVal = collectionsViewModel.ShowHelp;
            collectionsViewModel.ShowHelpToggleCommand.Execute(null);
            Assert.IsFalse(showHelpInitialVal);
            Assert.IsTrue(collectionsViewModel.ShowHelp);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ActivityCollectionDesignerViewModel_ExecuteShowHelpToggleCommand")]
        public void ActivityCollectionDesignerViewModel_ExecuteShowHelpToggleCommand_ShowHelpIsTrue_ShowHelpIsSetToFalse()
        {
            var mockModelItem = GenerateMockModelItem();
            var collectionsViewModel = new ActivityDesignerCollectionViewModelDerived(mockModelItem.Object);
            collectionsViewModel.ShowHelp = true;
            collectionsViewModel.ShowHelpToggleCommand.Execute(null);
            Assert.IsFalse(collectionsViewModel.ShowHelp);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ActivityCollectionDesignerViewModel_ToggleQuickVariableInput")]
        public void ActivityCollectionDesignerViewModel_ToggleQuickVariableInput_ShowThenClose_SmallViewIsDisplayed()
        {
            var mockModelItem = GenerateMockModelItem();
            var collectionsViewModel = new ActivityDesignerCollectionViewModelDerived(mockModelItem.Object);

            collectionsViewModel.ShowQuickVariableInput = true;
            Assert.IsFalse(collectionsViewModel.IsSmallViewActive);
            Assert.IsFalse(collectionsViewModel.ShowLarge);
            collectionsViewModel.ShowQuickVariableInput = false;
            Assert.IsTrue(collectionsViewModel.IsSmallViewActive);
            Assert.IsFalse(collectionsViewModel.ShowLarge);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ActivityCollectionDesignerViewModel_ToggleLargeView")]
        public void ActivityCollectionDesignerViewModel_ToggleLargeView_ShowThenClose_SmallViewIsDisplayed()
        {
            var mockModelItem = GenerateMockModelItem();
            var collectionsViewModel = new ActivityDesignerCollectionViewModelDerived(mockModelItem.Object);
            collectionsViewModel.ShowLarge = true;
            Assert.IsFalse(collectionsViewModel.IsSmallViewActive);
            Assert.IsFalse(collectionsViewModel.ShowQuickVariableInput);
            collectionsViewModel.ShowLarge = false;
            Assert.IsTrue(collectionsViewModel.IsSmallViewActive);
            Assert.IsFalse(collectionsViewModel.ShowQuickVariableInput);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ActivityCollectionDesignerViewModel_PreviousViewIsQuickvariableInput")]
        public void ActivityCollectionDesignerViewModel_ShowLarge_PreviousViewIsQuickvariableInput_ShowQuickVariableInputIsSetToFalse()
        {
            var mockModelItem = GenerateMockModelItem();
            var collectionsViewModel = new ActivityDesignerCollectionViewModelDerived(mockModelItem.Object);

            collectionsViewModel.PreviousView = "ShowQuickVariableInput";
            collectionsViewModel.ShowLarge = true;

            Assert.IsFalse(collectionsViewModel.ShowQuickVariableInput);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ActivityCollectionDesignerViewModel_PreviousViewIsShowLarge")]
        public void ActivityCollectionDesignerViewModel_ShowLarge_PreviousViewIsShowLarge_ShowLargeIsSetToFalse()
        {
            var mockModelItem = GenerateMockModelItem();
            var collectionsViewModel = new ActivityDesignerCollectionViewModelDerived(mockModelItem.Object);

            collectionsViewModel.PreviousView = "ShowLarge";
            collectionsViewModel.ShowQuickVariableInput = true;

            Assert.IsFalse(collectionsViewModel.ShowLarge);
        }

        [TestMethod]
        [TestCategory("ActivityCollectionDesignerViewModel_UnitTest")]
        [Description("Collection view model base can delete rows from the datagrid")]
        [Owner("Ashley Lewis")]
        public void ActivityCollectionDesignerViewModel_DeleteRow_RowAllowsDelete_RowDeleted()
        {
            //init
            const string ExpectedCollectionName = "FieldsCollection";
            var collectionNameProp = new Mock<ModelProperty>();
            var dtoListProp = new Mock<ModelProperty>();
            var displayNameProp = new Mock<ModelProperty>();
            var properties = new Dictionary<string, Mock<ModelProperty>>();
            var propertyCollection = new Mock<ModelPropertyCollection>();
            var mockModel = new Mock<ModelItem>();

            collectionNameProp.Setup(p => p.ComputedValue).Returns(ExpectedCollectionName);
            dtoListProp.Setup(p => p.ComputedValue).Returns(new List<ActivityDTO> { new ActivityDTO(), new ActivityDTO(), new ActivityDTO(), new ActivityDTO() });
            displayNameProp.Setup(p => p.ComputedValue).Returns("Test Display Name");
            properties.Add("CollectionName", collectionNameProp);
            propertyCollection.Protected().Setup<ModelProperty>("Find", "CollectionName", true).Returns(collectionNameProp.Object);
            propertyCollection.Protected().Setup<ModelProperty>("Find", ExpectedCollectionName, true).Returns(dtoListProp.Object);
            propertyCollection.Protected().Setup<ModelProperty>("Find", "DisplayName", true).Returns(displayNameProp.Object);
            mockModel.Setup(s => s.Properties).Returns(propertyCollection.Object);
            var vm = new TestActivityCollectionDesignerViewModel<ActivityDTO>(mockModel.Object) { SelectedIndex = 2 };

            //exe
            Assert.AreEqual(4, vm.Items.Count, "Collection view model base did not initialize as expected");
            vm.DeleteItem();

            //assert
            Assert.AreEqual(3, vm.Items.Count, "Collection view model base cannot delete datagrid rows");
        }

        [TestMethod]
        [TestCategory("ActivityCollectionDesignerViewModel_UnitTest")]
        [Description("Collection view model base can insert rows into the datagrid")]
        [Owner("Ashley Lewis")]
        public void ActivityCollectionDesignerViewModel_InsertRow_BlankRow_RowInserted()
        {
            //init
            const string ExpectedCollectionName = "FieldsCollection";
            var collectionNameProp = new Mock<ModelProperty>();
            var dtoListProp = new Mock<ModelProperty>();
            var displayNameProp = new Mock<ModelProperty>();
            var properties = new Dictionary<string, Mock<ModelProperty>>();
            var propertyCollection = new Mock<ModelPropertyCollection>();
            var mockModel = new Mock<ModelItem>();

            collectionNameProp.Setup(p => p.ComputedValue).Returns(ExpectedCollectionName);
            dtoListProp.Setup(p => p.ComputedValue).Returns(new List<ActivityDTO> { new ActivityDTO(), new ActivityDTO(), new ActivityDTO() });
            displayNameProp.Setup(p => p.ComputedValue).Returns("Test Display Name");
            properties.Add("CollectionName", collectionNameProp);
            propertyCollection.Protected().Setup<ModelProperty>("Find", "CollectionName", true).Returns(collectionNameProp.Object);
            propertyCollection.Protected().Setup<ModelProperty>("Find", ExpectedCollectionName, true).Returns(dtoListProp.Object);
            propertyCollection.Protected().Setup<ModelProperty>("Find", "DisplayName", true).Returns(displayNameProp.Object);
            mockModel.Setup(s => s.Properties).Returns(propertyCollection.Object);
            var vm = new TestActivityCollectionDesignerViewModel<ActivityDTO>(mockModel.Object) { SelectedIndex = 2 };

            //exe
            Assert.AreEqual(3, vm.Items.Count, "Collection view model base did not initialize as expected");
            vm.InsertItem();

            //assert
            Assert.AreEqual(4, vm.Items.Count, "Collection view model base cannot insert datagrid rows");
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActivityCollectionDesignerViewModel_Collapse")]
        public void ActivityCollectionDesignerViewModel_Collapse_SmallViewActive()
        {
            //------------Setup for test--------------------------
            var mockModelItem = GenerateMockModelItem();
            var viewModel = new ActivityDesignerCollectionViewModelDerived(mockModelItem.Object);

            viewModel.ShowQuickVariableInput = true;
            viewModel.ShowLarge = true;

            //------------Execute Test---------------------------
            viewModel.Collapse();

            //------------Assert Results-------------------------
            Assert.IsTrue(viewModel.IsSmallViewActive);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActivityCollectionDesignerViewModel_Restore")]
        public void ActivityCollectionDesignerViewModel_RestoreFromPreviouslyViewedQuickVariableInput_QuickVariableInputActive()
        {
            //------------Setup for test--------------------------
            var mockModelItem = GenerateMockModelItem();
            var viewModel = new ActivityDesignerCollectionViewModelDerived(mockModelItem.Object);

            viewModel.PreviousView = ActivityCollectionDesignerViewModel.ShowQuickVariableInputProperty.Name;
            viewModel.ShowLarge = true;

            //------------Execute Test---------------------------
            viewModel.Restore();

            //------------Assert Results-------------------------
            Assert.IsTrue(viewModel.ShowQuickVariableInput);
            Assert.IsFalse(viewModel.ShowLarge);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActivityCollectionDesignerViewModel_Restore")]
        public void ActivityCollectionDesignerViewModel_RestoreFromPreviouslyViewedLargeView_LargeViewActive()
        {
            //------------Setup for test--------------------------
            var mockModelItem = GenerateMockModelItem();
            var viewModel = new ActivityDesignerCollectionViewModelDerived(mockModelItem.Object);


            viewModel.PreviousView = ActivityDesignerViewModel.ShowLargeProperty.Name;

            //------------Execute Test---------------------------
            viewModel.Restore();

            //------------Assert Results-------------------------
            Assert.IsTrue(viewModel.ShowLarge);
            Assert.IsFalse(viewModel.ShowQuickVariableInput);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActivityCollectionDesignerViewModel_Restore")]
        public void ActivityCollectionDesignerViewModel_RestoreFromPreviouslyViewedSmallView_SmallViewActive()
        {
            //------------Setup for test--------------------------
            var mockModelItem = GenerateMockModelItem();
            var viewModel = new ActivityDesignerCollectionViewModelDerived(mockModelItem.Object);


            viewModel.PreviousView = string.Empty;
            viewModel.ShowLarge = true;

            //------------Execute Test---------------------------
            viewModel.Restore();

            //------------Assert Results-------------------------
            Assert.IsTrue(viewModel.IsSmallViewActive);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActivityCollectionDesignerViewModel_AddTitleBarQuickVariableInputToggle")]
        public void ActivityCollectionDesignerViewModel_AddTitleBarQuickVariableInputToggle_Added()
        {
            //------------Setup for test--------------------------
            var mockModelItem = GenerateMockModelItem();
            var viewModel = new ActivityDesignerCollectionViewModelDerived(mockModelItem.Object);

            //------------Execute Test---------------------------
            viewModel.TestAddTitleBarQuickVariableInputToggle();

            //------------Assert Results-------------------------
            Assert.AreEqual(1, viewModel.TitleBarToggles.Count);

            var toggle = viewModel.TitleBarToggles[0];

            Assert.AreEqual("pack://application:,,,/Dev2.Activities.Designers;component/Images/ServiceQuickVariableInput-32.png", toggle.CollapseImageSourceUri);
            Assert.AreEqual("Close Quick Variable Input", toggle.CollapseToolTip);
            Assert.AreEqual("pack://application:,,,/Dev2.Activities.Designers;component/Images/ServiceQuickVariableInput-32.png", toggle.ExpandImageSourceUri);
            Assert.AreEqual("Open Quick Variable Input", toggle.ExpandToolTip);
            Assert.AreEqual("QuickVariableInputToggle", toggle.AutomationID);

            var binding = BindingOperations.GetBinding(viewModel, ActivityCollectionDesignerViewModel.ShowQuickVariableInputProperty);
            Assert.IsNotNull(binding);
            Assert.AreEqual(toggle, binding.Source);
            Assert.AreEqual(ActivityDesignerToggle.IsCheckedProperty.Name, binding.Path.Path);
            Assert.AreEqual(BindingMode.TwoWay, binding.Mode);
        }

        static Mock<ModelItem> GenerateMockModelItem(int noOfActivitiesInModelItem = 2)
        {
            var properties = new Dictionary<string, Mock<ModelProperty>>();
            var propertyCollection = new Mock<ModelPropertyCollection>();

            var fieldsCollection = new Mock<ModelProperty>();
            var activityDtos = new List<ActivityDTO>();

            for(var i = 0; i < noOfActivitiesInModelItem; i++)
            {
                activityDtos.Add(new ActivityDTO
                {
                    FieldName = "field" + i,
                    FieldValue = "val " + i
                });
            }

            fieldsCollection.Setup(p => p.ComputedValue).Returns(activityDtos);
            properties.Add("FieldsCollection", fieldsCollection);
            propertyCollection.Protected().Setup<ModelProperty>("Find", "FieldsCollection", true).Returns(fieldsCollection.Object);

            var displayName = new Mock<ModelProperty>();
            displayName.Setup(p => p.ComputedValue).Returns("Activity (21)");
            properties.Add("DisplayName", displayName);
            propertyCollection.Protected().Setup<ModelProperty>("Find", "DisplayName", true).Returns(displayName.Object);

            var mockModelItem = new Mock<ModelItem>();
            mockModelItem.Setup(s => s.Properties).Returns(propertyCollection.Object);

            return mockModelItem;
        }
    }
}