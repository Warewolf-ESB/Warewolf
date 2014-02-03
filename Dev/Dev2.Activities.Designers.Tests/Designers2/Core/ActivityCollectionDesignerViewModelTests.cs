using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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
    [ExcludeFromCodeCoverage]
    public class ActivityCollectionDesignerViewModelTests
    {
        [TestMethod]
        [TestCategory("ActivityCollectionDesignerViewModel_Constructor")]
        [Owner("Trevor Williams-Ros")]
        // ReSharper disable InconsistentNaming
        public void ActivityCollectionDesignerViewModel_Constructor_NoRows_TwoBlankRowsAdded()
        // ReSharper restore InconsistentNaming
        {
            //exe
            var vm = new TestActivityDesignerCollectionViewModelItemsInitialized(CreateModelItem(0));

            //assert
            VerifyCollection(vm, 2, allRowsBlank: true);
        }

        [TestMethod]
        [TestCategory("ActivityCollectionDesignerViewModel_Constructor")]
        [Owner("Trevor Williams-Ros")]
        // ReSharper disable InconsistentNaming
        public void ActivityCollectionDesignerViewModel_Constructor_OneBlankRow_OneBlankRowAdded()
        // ReSharper restore InconsistentNaming
        {
            //exe
            var vm = new TestActivityDesignerCollectionViewModelItemsInitialized(CreateModelItem(1));

            //assert
            VerifyCollection(vm, 2, allRowsBlank: true);
        }

        [TestMethod]
        [TestCategory("ActivityCollectionDesignerViewModel_Constructor")]
        [Owner("Trevor Williams-Ros")]
        // ReSharper disable InconsistentNaming
        public void ActivityCollectionDesignerViewModel_Constructor_TwoBlankRows_NoRowAdded()
        // ReSharper restore InconsistentNaming
        {
            var modelItem = CreateModelItem(2);
            //exe
            var vm = new TestActivityDesignerCollectionViewModelItemsInitialized(CreateModelItem(2));

            //assert
            VerifyCollection(vm, 2, allRowsBlank: true);
        }

        [TestMethod]
        [TestCategory("ActivityCollectionDesignerViewModel_Constructor")]
        [Owner("Trevor Williams-Ros")]
        // ReSharper disable InconsistentNaming
        public void ActivityCollectionDesignerViewModel_Constructor_NoBlankRows_BlankRowAdded()
        // ReSharper restore InconsistentNaming
        {
            var items = CreateItemsList(3);
            items.RemoveAt(2); // remove last blank row

            var modelItem = CreateModelItem(items);
            var expectedItemsByIndexNumber = items.ToDictionary(dto => dto.IndexNumber);

            //exe
            var vm = new TestActivityDesignerCollectionViewModelItemsInitialized(modelItem);

            //assert
            VerifyCollection(vm, 3, expectedItemsByIndexNumber);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ActivityCollectionDesignerViewModel_Constructor")]
        public void ActivityCollectionDesignerViewModel_Constructor_ValidModelItem_InstanceOfQuickVariableInputViewModelCreated()
        {
            var collectionsViewModel = new TestActivityDesignerCollectionViewModelItemsInitialized(CreateModelItem(0));
            Assert.IsNotNull(collectionsViewModel.QuickVariableInputViewModel);
        }



        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("ActivityCollectionDesignerViewModel_Validate")]
        public void ActivityCollectionDesignerViewModel_Validate_NoErrorsFound_IsValidIsTrue()
        {
            var collectionsViewModel = new TestActivityDesignerCollectionViewModelItemsInitialized(CreateModelItem(4));
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

            var modelItem = CreateModelItem(ItemCount);
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

            var modelItem = CreateModelItem(ItemCount);
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
                    Assert.IsTrue(dto.CanRemove());
                }
                else
                {
                    Assert.IsFalse(dto.CanRemove());
                    Assert.AreEqual("NewField" + i, dto.FieldName);
                }
            }
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActivityCollectionDesignerViewModel_AddToCollection")]
        public void ActivityCollectionDesignerViewModel_AddToCollection_OverwriteFalseAndNonBlankRows_ItemsInserted()
        {
            //------------Setup for test--------------------------
            var items = CreateItemsList(3); // 2 non-blank + 1 blank
            var modelItem = CreateModelItem(items);
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

            Assert.AreSame(items[0], mic[0].GetCurrentValue());
            Assert.AreSame(items[1], mic[1].GetCurrentValue());

            VerifyItem(mic[2], 3, "NewField1", "");
            VerifyItem(mic[3], 4, "NewField2", "");

            // check that the first blank row was not removed - just repositioned
            Assert.AreSame(items[2], mic[4].GetCurrentValue());
            VerifyItem(mic[4], 5, "", "");

            // ReSharper restore PossibleNullReferenceException

            Assert.AreEqual(string.Format("Activity ({0})", ExpectedItemCount - 1), viewModel.ModelItem.GetProperty("DisplayName"));
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActivityCollectionDesignerViewModel_AddToCollection")]
        public void ActivityCollectionDesignerViewModel_AddToCollection_OverwriteFalseAndTwoBlankRows_ItemsAdded()
        {
            //------------Setup for test--------------------------
            var items = CreateItemsList(2);     // all blank rows
            var modelItem = CreateModelItem(items);
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

            // check that the first blank row was not removed - just repositioned
            Assert.AreSame(items[0], mic[2].GetCurrentValue());
            VerifyItem(mic[2], 3, "", "");

            // ReSharper restore PossibleNullReferenceException

            Assert.AreEqual(string.Format("Activity ({0})", ExpectedItemCount - 1), viewModel.ModelItem.GetProperty("DisplayName"));
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActivityCollectionDesignerViewModel_UpdateItem")]
        public void ActivityCollectionDesignerViewModel_UpdateItem_MakesRowBlankAndIndexNumberInBounds_RowNotRemoved()
        {
            Verify_UpdateItem_MakesRowBlankAndIndexNumberInBounds_RowNotRemoved(4, 1);
            Verify_UpdateItem_MakesRowBlankAndIndexNumberInBounds_RowNotRemoved(4, 2);
            Verify_UpdateItem_MakesRowBlankAndIndexNumberInBounds_RowNotRemoved(4, 3);
        }

        void Verify_UpdateItem_MakesRowBlankAndIndexNumberInBounds_RowNotRemoved(int startItemCount, int indexNumber)
        {
            //------------Setup for test--------------------------
            var items = CreateItemsList(startItemCount);
            var modelItem = CreateModelItem(items);
            var viewModel = new TestActivityDesignerCollectionViewModelItemsInitialized(modelItem);

            for(var i = 0; i < startItemCount; i++)
            {
                Assert.AreEqual(i == startItemCount - 1, items[i].CanRemove());
            }

            var idx = indexNumber - 1;

            //------------Execute Test---------------------------
            items[idx].FieldName = string.Empty;  // CanRemove() checks this
            items[idx].FieldValue = string.Empty; // CanRemove() checks this


            //------------Assert Results-------------------------
            Assert.IsTrue(items[idx].CanRemove());

            var expectedItemCount = startItemCount;
            var expectedNonBlankItemCount = expectedItemCount - 1;
            Assert.AreEqual(expectedItemCount, viewModel.ItemCount);
            Assert.AreEqual(string.Format("Activity ({0})", expectedNonBlankItemCount), viewModel.ModelItem.GetProperty("DisplayName"));

            // ReSharper disable PossibleNullReferenceException
            var mic = viewModel.ModelItem.Properties[viewModel.CollectionName].Collection;

            for(int i = 0, j = 0; j < expectedItemCount; j++)
            {
                var dto = (ActivityDTO)mic[j].GetCurrentValue();
                Assert.AreSame(items[i++], dto);
                Assert.AreEqual(j + 1, dto.IndexNumber);
            }
            // ReSharper restore PossibleNullReferenceException
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActivityCollectionDesignerViewModel_UpdateItem")]
        public void ActivityCollectionDesignerViewModel_UpdateItem_MakesRowBlankAndIndexNumberOutOfBounds_RowNotRemoved()
        {
            Verify_UpdateItem_MakesRowBlankAndIndexNumberOutOfBounds_RowNotRemoved(3, -1);    // before first row
            Verify_UpdateItem_MakesRowBlankAndIndexNumberOutOfBounds_RowNotRemoved(3, 3);     // last row 
            Verify_UpdateItem_MakesRowBlankAndIndexNumberOutOfBounds_RowNotRemoved(3, 4);     // after last row
        }

        void Verify_UpdateItem_MakesRowBlankAndIndexNumberOutOfBounds_RowNotRemoved(int startItemCount, int indexNumber)
        {
            //------------Setup for test--------------------------
            var items = CreateItemsList(startItemCount);
            var modelItem = CreateModelItem(items);
            var viewModel = new TestActivityDesignerCollectionViewModelItemsInitialized(modelItem);

            Assert.IsFalse(items[0].CanRemove());
            Assert.IsFalse(items[1].CanRemove());
            Assert.IsTrue(items[2].CanRemove());

            const int Idx = 1;  // random row

            //------------Execute Test---------------------------
            items[Idx].IndexNumber = indexNumber;  // must be done first, so that the index number is correct before clearing the other fields
            items[Idx].FieldName = string.Empty;   // CanRemove() checks this
            items[Idx].FieldValue = string.Empty;  // CanRemove() checks this

            //------------Assert Results-------------------------
            Assert.IsTrue(items[Idx].CanRemove());

            var expectedItemCount = startItemCount;
            var expectedNonBlankItemCount = expectedItemCount - 1;
            Assert.AreEqual(expectedItemCount, viewModel.ItemCount);
            Assert.AreEqual(string.Format("Activity ({0})", expectedNonBlankItemCount), viewModel.ModelItem.GetProperty("DisplayName"));

            // ReSharper disable PossibleNullReferenceException
            var mic = viewModel.ModelItem.Properties[viewModel.CollectionName].Collection;

            Assert.AreSame(items[0], mic[0].GetCurrentValue());
            Assert.AreSame(items[1], mic[1].GetCurrentValue());
            Assert.AreSame(items[2], mic[2].GetCurrentValue());
            // ReSharper restore PossibleNullReferenceException
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActivityCollectionDesignerViewModel_UpdateItem")]
        public void ActivityCollectionDesignerViewModel_UpdateItem_RowCountIsTwoAndMakesFirstRowBlank_RowNotRemoved()
        {
            //------------Setup for test--------------------------

            var items = new List<ActivityDTO>
            {
                new ActivityDTO("field1", "value1", 1),
                new ActivityDTO("", "", 2)
            };

            Assert.IsFalse(items[0].CanRemove());
            Assert.IsTrue(items[1].CanRemove());

            var modelItem = CreateModelItem(items);
            var viewModel = new TestActivityDesignerCollectionViewModelItemsInitialized(modelItem);

            const int Idx = 0;

            //------------Execute Test---------------------------
            items[Idx].FieldName = string.Empty;   // CanRemove() checks this
            items[Idx].FieldValue = string.Empty;  // CanRemove() checks this

            //------------Assert Results-------------------------
            Assert.IsTrue(items[Idx].CanRemove());

            const int ExpectedItemCount = 2;
            const int ExpectedNonBlankItemCount = ExpectedItemCount - 1;
            Assert.AreEqual(2, viewModel.ItemCount);
            Assert.AreEqual(string.Format("Activity ({0})", ExpectedNonBlankItemCount), viewModel.ModelItem.GetProperty("DisplayName"));

            // ReSharper disable PossibleNullReferenceException
            var mic = viewModel.ModelItem.Properties[viewModel.CollectionName].Collection;

            Assert.AreSame(items[0], mic[0].GetCurrentValue());
            Assert.AreSame(items[1], mic[1].GetCurrentValue());
            // ReSharper restore PossibleNullReferenceException
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActivityCollectionDesignerViewModel_UpdateItem")]
        public void ActivityCollectionDesignerViewModel_UpdateItem_RowCountIsTwoBlanksAndMakesSecondRowNonBlank_NoRowsAddedOrRemoved()
        {
            //------------Setup for test--------------------------
            var items = new List<ActivityDTO>
            {
                new ActivityDTO("", "", 1),
                new ActivityDTO("", "", 2)
            };

            Assert.IsTrue(items[0].CanRemove());
            Assert.IsTrue(items[1].CanRemove());

            var modelItem = CreateModelItem(items);
            var viewModel = new TestActivityDesignerCollectionViewModelItemsInitialized(modelItem);

            const int Idx = 1;

            //------------Execute Test---------------------------
            items[Idx].FieldName = "test";   // CanRemove() checks this

            //------------Assert Results-------------------------
            Verify_CollectionUnchanged(items, viewModel);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActivityCollectionDesignerViewModel_UpdateItem")]
        public void ActivityCollectionDesignerViewModel_UpdateItem_MakesRowNotBlankAndLastRowIsBlank_RowNotAdded()
        {
            Verify_UpdateItem_MakesRowNotBlank(3, 1, isNewBlankRowAdded: false);
            Verify_UpdateItem_MakesRowNotBlank(3, 2, isNewBlankRowAdded: false);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActivityCollectionDesignerViewModel_UpdateItem")]
        public void ActivityCollectionDesignerViewModel_UpdateItem_MakesLastBlankRowNotBlank_RowAdded()
        {
            Verify_UpdateItem_MakesRowNotBlank(3, 3, isNewBlankRowAdded: true);
        }

        void Verify_UpdateItem_MakesRowNotBlank(int startItemCount, int updatedIndexNumber, bool isNewBlankRowAdded)
        {
            //------------Setup for test--------------------------
            var items = CreateItemsList(startItemCount);
            var modelItem = CreateModelItem(items);
            var viewModel = new TestActivityDesignerCollectionViewModelItemsInitialized(modelItem);

            for(var i = 0; i < startItemCount; i++)
            {
                Assert.AreEqual(i == startItemCount - 1, items[i].CanRemove());
            }

            var idx = updatedIndexNumber - 1;

            //------------Execute Test---------------------------
            items[idx].FieldValue = idx == startItemCount - 1 ? "test value" : string.Empty; // if field is blank, put text otherwise clear it


            //------------Assert Results-------------------------
            Assert.IsFalse(items[idx].CanRemove());

            var expectedItemCount = isNewBlankRowAdded ? startItemCount + 1 : startItemCount;
            var expectedNonBlankItemCount = expectedItemCount - 1;
            Assert.AreEqual(expectedItemCount, viewModel.ItemCount);
            Assert.AreEqual(string.Format("Activity ({0})", expectedNonBlankItemCount), viewModel.ModelItem.GetProperty("DisplayName"));

            // ReSharper disable PossibleNullReferenceException
            var mic = viewModel.ModelItem.Properties[viewModel.CollectionName].Collection;

            for(int i = 0, j = 0; j < expectedItemCount; j++)
            {
                var dto = (ActivityDTO)mic[j].GetCurrentValue();
                if(j == expectedItemCount - 1 && isNewBlankRowAdded)
                {
                    Assert.AreEqual("", dto.FieldName);
                    Assert.AreEqual("", dto.FieldValue);
                }
                else
                {
                    Assert.AreSame(items[i++], dto);
                }
                Assert.AreEqual(j + 1, dto.IndexNumber);
            }
            // ReSharper restore PossibleNullReferenceException
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActivityCollectionDesignerViewModel_CanRemoveAt")]
        public void ActivityCollectionDesignerViewModel_CanRemoveAt_AnyIndexWhenTwoRows_False()
        {
            var items = CreateItemsList(2);
            Verify_CanRemoveAt(items, indexNumber: 1, expected: false);
            Verify_CanRemoveAt(items, indexNumber: 2, expected: false);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActivityCollectionDesignerViewModel_CanRemoveAt")]
        public void ActivityCollectionDesignerViewModel_CanRemoveAt_IndexIsNotLastRow_True()
        {
            var items = CreateItemsList(3);
            Verify_CanRemoveAt(items, indexNumber: 1, expected: true);
            Verify_CanRemoveAt(items, indexNumber: 2, expected: true);

            items = CreateItemsList(4);
            Verify_CanRemoveAt(items, indexNumber: 1, expected: true);
            Verify_CanRemoveAt(items, indexNumber: 2, expected: true);
            Verify_CanRemoveAt(items, indexNumber: 3, expected: true);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActivityCollectionDesignerViewModel_CanRemoveAt")]
        public void ActivityCollectionDesignerViewModel_CanRemoveAt_IndexIsLastRow_False()
        {
            var items = CreateItemsList(2);
            Verify_CanRemoveAt(items, indexNumber: 2, expected: false);

            items = CreateItemsList(3);
            Verify_CanRemoveAt(items, indexNumber: 3, expected: false);

            items = CreateItemsList(4);
            Verify_CanRemoveAt(items, indexNumber: 4, expected: false);
        }

        void Verify_CanRemoveAt(List<ActivityDTO> items, int indexNumber, bool expected)
        {
            //------------Setup for test--------------------------
            var modelItem = CreateModelItem(items);
            var viewModel = new TestActivityDesignerCollectionViewModelItemsInitialized(modelItem);

            //------------Execute Test---------------------------
            var result = viewModel.CanRemoveAt(indexNumber);

            //------------Assert Results-------------------------
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActivityCollectionDesignerViewModel_CanInsertAt")]
        public void ActivityCollectionDesignerViewModel_CanInsertAt_IndexIsLastRow_False()
        {
            var items = CreateItemsList(2);
            Verify_CanInsertAt(items, indexNumber: 2, expected: false);

            items = CreateItemsList(3);
            Verify_CanInsertAt(items, indexNumber: 3, expected: false);

            items = CreateItemsList(4);
            Verify_CanInsertAt(items, indexNumber: 4, expected: false);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActivityCollectionDesignerViewModel_CanInsertAt")]
        public void ActivityCollectionDesignerViewModel_CanInsertAt_AnyIndexWhenTwoRowsOrLess_False()
        {
            var items = CreateItemsList(1);
            Verify_CanInsertAt(items, indexNumber: 1, expected: false);

            items = CreateItemsList(2);
            Verify_CanInsertAt(items, indexNumber: 1, expected: false);
            Verify_CanInsertAt(items, indexNumber: 2, expected: false);
        }

        void Verify_CanInsertAt(List<ActivityDTO> items, int indexNumber, bool expected)
        {
            //------------Setup for test--------------------------
            var modelItem = CreateModelItem(items);
            var viewModel = new TestActivityDesignerCollectionViewModelItemsInitialized(modelItem);

            //------------Execute Test---------------------------
            var result = viewModel.CanInsertAt(indexNumber);

            //------------Assert Results-------------------------
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActivityCollectionDesignerViewModel_RemoveAt")]
        public void ActivityCollectionDesignerViewModel_RemoveAt_AnyIndexWhenTwoRows_DoesNotRemoveRow()
        {
            var items = CreateItemsList(2);
            var modelItem = CreateModelItem(items);
            var viewModel = new TestActivityDesignerCollectionViewModelItemsInitialized(modelItem);

            viewModel.RemoveAt(1);
            Verify_CollectionUnchanged(items, viewModel);

            viewModel.RemoveAt(2);
            Verify_CollectionUnchanged(items, viewModel);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActivityCollectionDesignerViewModel_RemoveAt")]
        public void ActivityCollectionDesignerViewModel_RemoveAt_AnyIndexExceptLastWhenThreeOrMoreRows_RemovesRow()
        {
            Verify_RemoveAt_RemovesRow(startItemCount: 3, indexNumber: 1);
            Verify_RemoveAt_RemovesRow(startItemCount: 3, indexNumber: 2);

            Verify_RemoveAt_RemovesRow(startItemCount: 4, indexNumber: 1);
            Verify_RemoveAt_RemovesRow(startItemCount: 4, indexNumber: 2);
            Verify_RemoveAt_RemovesRow(startItemCount: 4, indexNumber: 3);

            Verify_RemoveAt_RemovesRow(startItemCount: 5, indexNumber: 1);
            Verify_RemoveAt_RemovesRow(startItemCount: 5, indexNumber: 2);
            Verify_RemoveAt_RemovesRow(startItemCount: 5, indexNumber: 3);
            Verify_RemoveAt_RemovesRow(startItemCount: 5, indexNumber: 4);
        }

        void Verify_RemoveAt_RemovesRow(int startItemCount, int indexNumber)
        {
            //------------Setup for test--------------------------
            var items = CreateItemsList(startItemCount);
            var modelItem = CreateModelItem(items);
            var viewModel = new TestActivityDesignerCollectionViewModelItemsInitialized(modelItem);

            //------------Execute Test---------------------------
            viewModel.RemoveAt(indexNumber);

            //------------Assert Results-------------------------
            var expectedItemCount = startItemCount - 1;
            var expectedNonBlankItemCount = expectedItemCount - 1;
            Assert.AreEqual(expectedItemCount, viewModel.ItemCount);
            Assert.AreEqual(string.Format("Activity ({0})", expectedNonBlankItemCount), viewModel.ModelItem.GetProperty("DisplayName"));

            // ReSharper disable PossibleNullReferenceException
            var mic = viewModel.ModelItem.Properties[viewModel.CollectionName].Collection;

            for(int i = 0, j = 0; i < expectedItemCount; i++, j++)
            {
                var expectedIndexNumber = i + 1;
                var dto = (ActivityDTO)mic[i].GetCurrentValue();
                Assert.AreEqual(expectedIndexNumber, dto.IndexNumber);

                if(i == indexNumber - 1)
                {
                    // skip item that was deleted
                    j++;
                }
                Assert.AreSame(items[j], dto);
            }

            // ReSharper restore PossibleNullReferenceException
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActivityCollectionDesignerViewModel_InsertAt")]
        public void ActivityCollectionDesignerViewModel_InsertAt_AnyIndexExceptLastWhenThreeOrMoreRows_InsertsRow()
        {
            Verify_InsertsAt_InsertsRow(startItemCount: 3, indexNumber: 1);
            Verify_InsertsAt_InsertsRow(startItemCount: 3, indexNumber: 2);

            Verify_InsertsAt_InsertsRow(startItemCount: 4, indexNumber: 1);
            Verify_InsertsAt_InsertsRow(startItemCount: 4, indexNumber: 2);
            Verify_InsertsAt_InsertsRow(startItemCount: 4, indexNumber: 3);

            Verify_InsertsAt_InsertsRow(startItemCount: 5, indexNumber: 1);
            Verify_InsertsAt_InsertsRow(startItemCount: 5, indexNumber: 2);
            Verify_InsertsAt_InsertsRow(startItemCount: 5, indexNumber: 3);
            Verify_InsertsAt_InsertsRow(startItemCount: 5, indexNumber: 4);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActivityCollectionDesignerViewModel_InsertAt")]
        public void ActivityCollectionDesignerViewModel_InsertAt_AnyIndexWhenTwoRows_DoesNotInsertRow()
        {
            var items = CreateItemsList(2);
            var modelItem = CreateModelItem(items);
            var viewModel = new TestActivityDesignerCollectionViewModelItemsInitialized(modelItem);

            viewModel.InsertAt(1);
            Verify_CollectionUnchanged(items, viewModel);

            viewModel.InsertAt(2);
            Verify_CollectionUnchanged(items, viewModel);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActivityCollectionDesignerViewModel_InsertAt")]
        public void ActivityCollectionDesignerViewModel_InsertAt_LastIndexWhenThreeOrMoreRows_DoesNotInsertRow()
        {
            Verify_InsertAt_LastIndex_DoesNotInsertRow(3);
            Verify_InsertAt_LastIndex_DoesNotInsertRow(4);
            Verify_InsertAt_LastIndex_DoesNotInsertRow(5);
        }

        void Verify_InsertAt_LastIndex_DoesNotInsertRow(int itemCount)
        {
            var items = CreateItemsList(itemCount);
            var modelItem = CreateModelItem(items);
            var viewModel = new TestActivityDesignerCollectionViewModelItemsInitialized(modelItem);

            viewModel.InsertAt(itemCount);
            Verify_CollectionUnchanged(items, viewModel);
        }

        void Verify_InsertsAt_InsertsRow(int startItemCount, int indexNumber)
        {
            //------------Setup for test--------------------------
            var items = CreateItemsList(startItemCount);
            var modelItem = CreateModelItem(items);
            var viewModel = new TestActivityDesignerCollectionViewModelItemsInitialized(modelItem);

            //------------Execute Test---------------------------
            viewModel.InsertAt(indexNumber);

            //------------Assert Results-------------------------
            var expectedItemCount = startItemCount + 1;
            var expectedNonBlankItemCount = expectedItemCount - 1;
            Assert.AreEqual(expectedItemCount, viewModel.ItemCount);
            Assert.AreEqual(string.Format("Activity ({0})", expectedNonBlankItemCount), viewModel.ModelItem.GetProperty("DisplayName"));

            // ReSharper disable PossibleNullReferenceException
            var mic = viewModel.ModelItem.Properties[viewModel.CollectionName].Collection;

            for(int i = 0, j = 0; i < expectedItemCount; i++)
            {
                var expectedIndexNumber = i + 1;
                var dto = (ActivityDTO)mic[i].GetCurrentValue();
                Assert.AreEqual(expectedIndexNumber, dto.IndexNumber);

                if(i != indexNumber - 1)
                {
                    Assert.AreSame(items[j++], dto);
                }
                else
                {
                    Assert.IsTrue(dto.CanRemove());
                }
            }

            // ReSharper restore PossibleNullReferenceException
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActivityCollectionDesignerViewModel_OnDTOPropertyChanged")]
        public void ActivityCollectionDesignerViewModel_OnDTOPropertyChanged_OnlyListensForCanRemoveProperty()
        {
            //------------Setup for test--------------------------
            var items = CreateItemsList(3);
            var modelItem = CreateModelItem(items);
            var viewModel = new TestActivityDesignerCollectionViewModelItemsInitialized(modelItem);

            //------------Execute Test---------------------------
            items[0].IsFieldNameFocused = true;

            //------------Assert Results-------------------------
            Verify_CollectionUnchanged(items, viewModel);
        }


        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActivityCollectionDesignerViewModel_OnSelectionChanged")]
        public void ActivityCollectionDesignerViewModel_OnSelectionChanged_OldRowIsBlank_RowRemoved()
        {
            Verify_OnSelectionChanged_OldRowIsBlank_RowRemoved(4, 1);
            Verify_OnSelectionChanged_OldRowIsBlank_RowRemoved(4, 2);
            Verify_OnSelectionChanged_OldRowIsBlank_RowRemoved(4, 3);
        }

        void Verify_OnSelectionChanged_OldRowIsBlank_RowRemoved(int startItemCount, int indexNumber)
        {
            //------------Setup for test--------------------------
            var items = CreateItemsList(startItemCount);
            var modelItem = CreateModelItem(items);
            var viewModel = new TestActivityDesignerCollectionViewModelItemsInitialized(modelItem);

            for(var i = 0; i < startItemCount; i++)
            {
                Assert.AreEqual(i == startItemCount - 1, items[i].CanRemove());
            }

            var idx = indexNumber - 1;

            //------------Execute Test---------------------------
            items[idx].FieldName = string.Empty;  // CanRemove() checks this
            items[idx].FieldValue = string.Empty; // CanRemove() checks this
            viewModel.OnSelectionChanged(viewModel.ModelItemCollection[idx], viewModel.ModelItemCollection[idx + 1]);

            //------------Assert Results-------------------------
            Assert.IsTrue(items[idx].CanRemove());

            var expectedItemCount = startItemCount - 1;
            var expectedNonBlankItemCount = expectedItemCount - 1;
            Assert.AreEqual(expectedItemCount, viewModel.ItemCount);
            Assert.AreEqual(string.Format("Activity ({0})", expectedNonBlankItemCount), viewModel.ModelItem.GetProperty("DisplayName"));

            // ReSharper disable PossibleNullReferenceException
            var mic = viewModel.ModelItem.Properties[viewModel.CollectionName].Collection;

            for(int i = 0, j = 0; j < expectedItemCount; j++)
            {
                if(i == idx)
                {
                    i++; // skip
                }
                var dto = (ActivityDTO)mic[j].GetCurrentValue();
                Assert.AreSame(items[i++], dto);
                Assert.AreEqual(j + 1, dto.IndexNumber);
            }
            // ReSharper restore PossibleNullReferenceException
        }


        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActivityCollectionDesignerViewModel_OnSelectionChanged")]
        public void ActivityCollectionDesignerViewModel_OnSelectionChanged_OldRowIsNotBlank_RowNotRemoved()
        {
            Verify_OnSelectionChanged_OldRowIsNotBlank_RowNotRemoved(4, 1);
            Verify_OnSelectionChanged_OldRowIsNotBlank_RowNotRemoved(4, 2);
            Verify_OnSelectionChanged_OldRowIsNotBlank_RowNotRemoved(4, 3);
        }

        void Verify_OnSelectionChanged_OldRowIsNotBlank_RowNotRemoved(int startItemCount, int indexNumber)
        {
            //------------Setup for test--------------------------
            var items = CreateItemsList(startItemCount);
            var modelItem = CreateModelItem(items);
            var viewModel = new TestActivityDesignerCollectionViewModelItemsInitialized(modelItem);

            for(var i = 0; i < startItemCount; i++)
            {
                Assert.AreEqual(i == startItemCount - 1, items[i].CanRemove());
            }

            var idx = indexNumber - 1;

            //------------Execute Test---------------------------
            items[idx].FieldName = string.Empty;  // CanRemove() checks this
            viewModel.OnSelectionChanged(viewModel.ModelItemCollection[idx], viewModel.ModelItemCollection[idx + 1]);

            //------------Assert Results-------------------------
            Assert.IsFalse(items[idx].CanRemove());

            Verify_CollectionUnchanged(items, viewModel);
        }

        static void VerifyItem(ModelItem modelItem, int indexNumber, string fieldName, string fieldValue)
        {
            // ReSharper disable PossibleNullReferenceException
            Assert.AreEqual(indexNumber, modelItem.Properties["IndexNumber"].ComputedValue);
            Assert.AreEqual(fieldName, modelItem.Properties["FieldName"].ComputedValue);
            Assert.AreEqual(fieldValue, modelItem.Properties["FieldValue"].ComputedValue);
            // ReSharper restore PossibleNullReferenceException
        }

        void Verify_CollectionUnchanged(List<ActivityDTO> items, TestActivityDesignerCollectionViewModelItemsInitialized viewModel)
        {
            var expectedItemCount = items.Count;
            var expectedNonBlankItemCount = expectedItemCount - 1;
            Assert.AreEqual(expectedItemCount, viewModel.ItemCount);
            Assert.AreEqual(string.Format("Activity ({0})", expectedNonBlankItemCount), viewModel.ModelItem.GetProperty("DisplayName"));

            // ReSharper disable PossibleNullReferenceException
            var mic = viewModel.ModelItem.Properties[viewModel.CollectionName].Collection;

            for(int i = 0, j = 0; i < expectedItemCount; i++, j++)
            {
                var expectedIndexNumber = i + 1;
                var dto = (ActivityDTO)mic[i].GetCurrentValue();
                Assert.AreEqual(expectedIndexNumber, dto.IndexNumber);
                Assert.AreSame(items[j], dto);
            }

            // ReSharper restore PossibleNullReferenceException
        }

        static void VerifyCollection(ActivityCollectionDesignerViewModel<ActivityDTO> viewModel, int expectedItemCount, Dictionary<int, ActivityDTO> expectedItemsByIndexNumber = null, bool allRowsBlank = false)
        {
            var expectedNonBlankItemCount = expectedItemCount - 1;
            Assert.AreEqual(expectedItemCount, viewModel.ItemCount);
            Assert.AreEqual(string.Format("Activity ({0})", expectedNonBlankItemCount), viewModel.ModelItem.GetProperty("DisplayName"));

            // ReSharper disable PossibleNullReferenceException
            var mic = viewModel.ModelItem.Properties[viewModel.CollectionName].Collection;

            for(var j = 0; j < expectedItemCount; j++)
            {
                var dto = (ActivityDTO)mic[j].GetCurrentValue();
                var expectedIndexNumber = j + 1;
                Assert.AreEqual(expectedIndexNumber, dto.IndexNumber);
                if(allRowsBlank)
                {
                    Assert.IsTrue(dto.CanRemove());
                }
                else
                {
                    // Only last row should be blank
                    Assert.AreEqual(j == expectedItemCount - 1, dto.CanRemove());
                }

                if(expectedItemsByIndexNumber != null)
                {
                    ActivityDTO expectedDTO;
                    if(expectedItemsByIndexNumber.TryGetValue(expectedIndexNumber, out expectedDTO))
                    {
                        Assert.AreSame(expectedDTO, dto);
                    }
                }
            }
            // ReSharper restore PossibleNullReferenceException
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

        static ModelItem CreateModelItem(int itemCount)
        {
            var items = CreateItemsList(itemCount);
            return CreateModelItem(items);
        }

        static ModelItem CreateModelItem(IEnumerable<ActivityDTO> items, string displayName = "Activity")
        {
            var modelItem = ModelItemUtils.CreateModelItem(new DsfMultiAssignActivity());
            modelItem.SetProperty("DisplayName", displayName);

            // ReSharper disable PossibleNullReferenceException
            var modelItemCollection = modelItem.Properties["FieldsCollection"].Collection;
            foreach(var dto in items)
            {
                modelItemCollection.Add(dto);
            }
            // ReSharper restore PossibleNullReferenceException

            return modelItem;
        }

        static List<ActivityDTO> CreateItemsList(int itemCount)
        {
            var result = new List<ActivityDTO>();
            for(int i = 0, indexNumber = 1; i < itemCount; i++, indexNumber++)
            {
                if(itemCount <= 2 || i == itemCount - 1)
                {
                    // Always make last row blank
                    result.Add(new ActivityDTO("", "", indexNumber));
                }
                else
                {
                    result.Add(new ActivityDTO("field" + indexNumber, "value" + indexNumber, indexNumber, i == 0));
                }
            }
            return result;
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ActivityCollectionDesignerViewModel_Validate")]
        public void ActivityCollectionDesignerViewModel_Validate_InvokesSubMethodsAndUpdatesErrors()
        {
            //------------Setup for test--------------------------
            var viewModel = new TestValidationActivityDesignerCollectionViewModel(CreateModelItem(2));
            
            //------------Execute Test---------------------------
            viewModel.Validate();

            //------------Assert Results-------------------------
            Assert.AreEqual(1, viewModel.ValidateThisHitCount);
            Assert.AreEqual(2, viewModel.ValidateCollectionItemHitCount);
            Assert.AreEqual(3, viewModel.Errors.Count);
        }
    }
}