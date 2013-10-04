using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Windows.Data;
using Dev2.Activities.Designers.Tests.Designers2.Core.Stubs;
using Dev2.Activities.Designers2.Core;
using Dev2.Providers.Errors;
using Dev2.Studio.Core.Activities.Utils;
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
            //const string ExpectedCollectionName = "FieldsCollection";
            //var collectionNameProp = new Mock<ModelProperty>();
            //var dtoListProp = new Mock<ModelProperty>();
            //var displayNameProp = new Mock<ModelProperty>();
            //var properties = new Dictionary<string, Mock<ModelProperty>>();
            //var propertyCollection = new Mock<ModelPropertyCollection>();
            //var mockModel = new Mock<ModelItem>();

            //collectionNameProp.Setup(p => p.ComputedValue).Returns(ExpectedCollectionName);
            //dtoListProp.Setup(p => p.ComputedValue).Returns(new List<ActivityDTO>());
            //displayNameProp.Setup(p => p.ComputedValue).Returns("Test Display Name");
            //properties.Add("CollectionName", collectionNameProp);
            //propertyCollection.Protected().Setup<ModelProperty>("Find", "CollectionName", true).Returns(collectionNameProp.Object);
            //propertyCollection.Protected().Setup<ModelProperty>("Find", ExpectedCollectionName, true).Returns(dtoListProp.Object);
            //propertyCollection.Protected().Setup<ModelProperty>("Find", "DisplayName", true).Returns(displayNameProp.Object);
            //mockModel.Setup(s => s.Properties).Returns(propertyCollection.Object);

            //exe
            var vm = new TestActivityDesignerCollectionViewModelItemsInitialized(CreateModelItem(0));

            //assert
            Assert.AreEqual(2, vm.ItemCount, "Collection view model base cannot initialized 2 blank rows from a model item with no rows");
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
        [TestCategory("ActivityCollectionDesignerViewModel_Validate")]
        public void ActivityCollectionDesignerViewModel_Validate_NoErrorsFound_IsValidIsTrue()
        {
            const int itemsCount = 4;
            var collectionsViewModel = new TestActivityDesignerCollectionViewModelItemsInitialized(CreateModelItem(itemsCount, false, false, false, true));
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
            Assert.IsFalse(collectionsViewModel.ShowSmall);
            Assert.IsFalse(collectionsViewModel.ShowLarge);
            collectionsViewModel.ShowQuickVariableInput = false;
            Assert.IsTrue(collectionsViewModel.ShowSmall);
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
            Assert.IsFalse(collectionsViewModel.ShowSmall);
            Assert.IsFalse(collectionsViewModel.ShowQuickVariableInput);
            collectionsViewModel.ShowLarge = false;
            Assert.IsTrue(collectionsViewModel.ShowSmall);
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
            Assert.IsTrue(viewModel.ShowSmall);
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
            Assert.IsTrue(viewModel.ShowSmall);
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

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActivityCollectionDesignerViewModel_UpdateDisplayName")]
        public void ActivityCollectionDesignerViewModel_UpdateDisplayName_OldDisplayNameHasOpeningAndClosingBrackets_BracketsRemovedAndReadded()
        {
            //------------Setup for test--------------------------
            const int ItemCount = 3;

            var modelItem = CreateModelItem(ItemCount, new bool[ItemCount]);
            modelItem.SetProperty("DisplayName", "Activity (12)");
            var viewModel = new TestActivityDesignerCollectionViewModelItemsInitialized(modelItem);

            //------------Execute Test---------------------------
            viewModel.UpdateDisplayName();

            //------------Assert Results-------------------------
            var actual = viewModel.ModelItem.GetProperty("DisplayName");

            Assert.AreEqual(string.Format("Activity ({0})", ItemCount - 1), actual);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActivityCollectionDesignerViewModel_AddToCollection")]
        public void ActivityCollectionDesignerViewModel_AddToCollection_OverwriteTrue_CollectionClearedAndNewItemsAdded()
        {
            //------------Setup for test--------------------------
            const int ItemCount = 3;

            var modelItem = CreateModelItem(ItemCount, new bool[ItemCount]);
            modelItem.SetProperty("DisplayName", "Activity");
            var viewModel = new TestActivityDesignerCollectionViewModelItemsInitialized(modelItem);

            const int ExpectedItemCount = 5;
            var source = new List<string>();
            for(var i = 0; i < ExpectedItemCount; i++)
            {
                source.Add("NewField" + i);
            }

            //------------Execute Test---------------------------
            viewModel.TestAddToCollection(source, true);

            //------------Assert Results-------------------------
            // ReSharper disable PossibleNullReferenceException
            var mic = viewModel.ModelItem.Properties[viewModel.CollectionName].Collection;
            // ReSharper restore PossibleNullReferenceException

            // Extra blank row is also added
            Assert.AreEqual(ExpectedItemCount + 1, viewModel.ItemCount);
            Assert.AreEqual(string.Format("Activity ({0})", ExpectedItemCount), viewModel.ModelItem.GetProperty("DisplayName"));

            for(var i = 0; i < mic.Count; i++)
            {
                var dto = (ActivityDTO)mic[i].GetCurrentValue();
                if(i == ExpectedItemCount)
                {
                    // last row is blank
                    Assert.AreEqual("", dto.FieldName);
                }
                else
                {
                    Assert.AreEqual("NewField" + i, dto.FieldName);
                }
            }
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActivityCollectionDesignerViewModel_AddToCollection")]
        public void ActivityCollectionDesignerViewModel_AddToCollection_OverwriteFalseAndSomeNonBlankRows_ItemsInserted()
        {
            //------------Setup for test--------------------------
            var modelItem = CreateModelItem(3, false, true, true);  // 2 blank rows
            modelItem.SetProperty("DisplayName", "Activity");
            var viewModel = new TestActivityDesignerCollectionViewModelItemsInitialized(modelItem);

            var source = new List<string>();
            for(var i = 0; i < 2; i++)
            {
                source.Add("NewField" + (i + 1));
            }

            const int ExpectedItemCount = 5;  // 3 old + 2 new

            //------------Execute Test---------------------------
            viewModel.TestAddToCollection(source, false);

            //------------Assert Results-------------------------
            // ReSharper disable PossibleNullReferenceException
            var mic = viewModel.ModelItem.Properties[viewModel.CollectionName].Collection;

            // Extra blank row is also added
            Assert.AreEqual(ExpectedItemCount, viewModel.ItemCount);

            VerifyItem(mic[0], 1, "field1", "value1");
            VerifyItem(mic[1], 2, "NewField1", "");
            VerifyItem(mic[2], 3, "NewField2", "");
            VerifyItem(mic[3], 3, "", "");
            VerifyItem(mic[4], 4, "", "");
            // ReSharper restore PossibleNullReferenceException

            Assert.AreEqual(string.Format("Activity ({0})", ExpectedItemCount - 1), viewModel.ModelItem.GetProperty("DisplayName"));
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActivityCollectionDesignerViewModel_AddToCollection")]
        public void ActivityCollectionDesignerViewModel_AddToCollection_OverwriteFalseAndAllBlankRows_ItemsAdded()
        {
            //------------Setup for test--------------------------
            var modelItem = CreateModelItem(2, true, true); // all blank rows
            modelItem.SetProperty("DisplayName", "Activity");
            var viewModel = new TestActivityDesignerCollectionViewModelItemsInitialized(modelItem);

            var source = new List<string>();
            for(var i = 0; i < 2; i++)
            {
                source.Add("NewField" + (i + 1));
            }

            const int ExpectedItemCount = 3;  

            //------------Execute Test---------------------------
            viewModel.TestAddToCollection(source, false);

            //------------Assert Results-------------------------
            // ReSharper disable PossibleNullReferenceException
            var mic = viewModel.ModelItem.Properties[viewModel.CollectionName].Collection;

            // Extra blank row is also added
            Assert.AreEqual(ExpectedItemCount, viewModel.ItemCount);

            VerifyItem(mic[0], 1, "NewField1", "");
            VerifyItem(mic[1], 2, "NewField2", "");
            VerifyItem(mic[2], 3, "", "");
            // ReSharper restore PossibleNullReferenceException

            Assert.AreEqual(string.Format("Activity ({0})", ExpectedItemCount - 1), viewModel.ModelItem.GetProperty("DisplayName"));
        }

        static void VerifyItem(ModelItem modelItem, int indexNumber, string fieldName, string fieldValue)
        {
            var dto = (ActivityDTO)modelItem.GetCurrentValue();
            Assert.AreEqual(indexNumber, dto.IndexNumber);
            Assert.AreEqual(fieldName, dto.FieldName);
            Assert.AreEqual(fieldValue, dto.FieldValue);
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

        static ModelItem CreateModelItem(int itemCount, params bool[] blankFieldAndValues)
        {
            var modelItem = ModelItemUtils.CreateModelItem(new DsfMultiAssignActivity());
            // ReSharper disable PossibleNullReferenceException
            var modelItemCollection = modelItem.Properties["FieldsCollection"].Collection;
            for(var i = 0; i < itemCount; i++)
            {
                var indexNumber = i + 1;
                var dto = blankFieldAndValues[i]
                    ? new ActivityDTO("", "", indexNumber)
                    : new ActivityDTO("field" + indexNumber, "value" + indexNumber, indexNumber, i == 0);

                modelItemCollection.Add(dto);
            }

            // ReSharper restore PossibleNullReferenceException
            return modelItem;
        }
    }
}