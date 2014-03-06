using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Dev2.Activities.Designers2.FindRecordsMultipleCriteria;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities.Designers.Tests.FindRecordsMultipleCriteria
{
    // OnSearchTypeChanged moved from FindRecordsTO tests
    // ReSharper disable InconsistentNaming
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class FindRecordsMultipleCriteriaTests
    {
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("FindRecordsMultipleCriteriaViewModel_OnSearchTypeChanged")]
        public void FindRecordsMultipleCriteriaViewModel_OnSearchTypeChanged_Equal_RequiresCriteriaInput_IsCriteriaEnabledTrue()
        {
            Verify_OnSearchTypeChanged_IsSearchCriteriaEnabled(searchType: "=", isSearchCriteriaEnabled: true, isSearchCriteriaBlank: false);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("FindRecordsMultipleCriteriaViewModel_OnSearchTypeChanged")]
        public void FindRecordsMultipleCriteriaViewModel_OnSearchTypeChanged_NotContains_RequiresCriteriaInput_IsCriteriaEnabledTrue()
        {
            Verify_OnSearchTypeChanged_IsSearchCriteriaEnabled(searchType: "Doesn't Contain", isSearchCriteriaEnabled: true, isSearchCriteriaBlank: false);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("FindRecordsMultipleCriteriaViewModel_OnSearchTypeChanged")]
        public void FindRecordsMultipleCriteriaViewModel_OnSearchTypeChanged_Contains_RequiresCriteriaInput_IsCriteriaEnabledTrue()
        {
            Verify_OnSearchTypeChanged_IsSearchCriteriaEnabled(searchType: "Contains", isSearchCriteriaEnabled: true, isSearchCriteriaBlank: false);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("FindRecordsMultipleCriteriaViewModel_OnSearchTypeChanged")]
        public void FindRecordsMultipleCriteriaViewModel_OnSearchTypeChanged_NotEqual_RequiresCriteriaInput_IsCriteriaEnabledTrue()
        {
            Verify_OnSearchTypeChanged_IsSearchCriteriaEnabled("<> (Not Equal)", isSearchCriteriaEnabled: true, isSearchCriteriaBlank: false);

        }
        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("FindRecordsMultipleCriteriaViewModel_OnSearchTypeChanged")]
        public void FindRecordsMultipleCriteriaViewModel_OnSearchTypeChanged_EndsWith_RequiresCriteriaInput_IsCriteriaEnabledTrue()
        {
            Verify_OnSearchTypeChanged_IsSearchCriteriaEnabled(searchType: "Ends With", isSearchCriteriaEnabled: true, isSearchCriteriaBlank: false);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("FindRecordsMultipleCriteriaViewModel_OnSearchTypeChanged")]
        public void FindRecordsMultipleCriteriaViewModel_OnSearchTypeChanged_StartsWith_RequiresCriteriaInput_IsCriteriaEnabledTrue()
        {
            Verify_OnSearchTypeChanged_IsSearchCriteriaEnabled(searchType: "Starts With", isSearchCriteriaEnabled: true, isSearchCriteriaBlank: false);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("FindRecordsMultipleCriteriaViewModel_OnSearchTypeChanged")]
        public void FindRecordsMultipleCriteriaViewModel_OnSearchTypeChanged_Regex_RequiresCriteriaInput_IsCriteriaEnabledTrue()
        {
            Verify_OnSearchTypeChanged_IsSearchCriteriaEnabled(searchType: "Is Regex", isSearchCriteriaEnabled: true, isSearchCriteriaBlank: false);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("FindRecordsMultipleCriteriaViewModel_OnSearchTypeChanged")]
        public void FindRecordsMultipleCriteriaViewModel_OnSearchTypeChanged_GreaterThan_RequiresCriteriaInput_IsCriteriaEnabledTrue()
        {
            Verify_OnSearchTypeChanged_IsSearchCriteriaEnabled(searchType: ">", isSearchCriteriaEnabled: true, isSearchCriteriaBlank: false);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("FindRecordsMultipleCriteriaViewModel_OnSearchTypeChanged")]
        public void FindRecordsMultipleCriteriaViewModel_OnSearchTypeChanged_LessThan_RequiresCriteriaInput_IsCriteriaEnabledTrue()
        {
            Verify_OnSearchTypeChanged_IsSearchCriteriaEnabled(searchType: "<", isSearchCriteriaEnabled: true, isSearchCriteriaBlank: false);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("FindRecordsMultipleCriteriaViewModel_OnSearchTypeChanged")]
        public void FindRecordsMultipleCriteriaViewModel_OnSearchTypeChanged_LessThanEqual_RequiresCriteriaInput_IsCriteriaEnabledTrue()
        {
            Verify_OnSearchTypeChanged_IsSearchCriteriaEnabled("<=", isSearchCriteriaEnabled: true, isSearchCriteriaBlank: false);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("FindRecordsMultipleCriteriaViewModel_OnSearchTypeChanged")]
        public void FindRecordsMultipleCriteriaViewModel_OnSearchTypeChanged_GreaterThanEqual_RequiresCriteriaInput_IsCriteriaEnabledTrue()
        {
            Verify_OnSearchTypeChanged_IsSearchCriteriaEnabled(">=", isSearchCriteriaEnabled: true, isSearchCriteriaBlank: false);
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("FindRecordsMultipleCriteriaViewModel_OnSearchTypeChanged")]
        public void FindRecordsMultipleCriteriaViewModel_OnSearchTypeChanged_NotInRequiresCriteriaInputList_IsCriteriaEnabledFalseSearchCriteriaEmptyString()
        {
            Verify_OnSearchTypeChanged_IsSearchCriteriaEnabled(searchType: "Is Numeric", isSearchCriteriaEnabled: false, isSearchCriteriaBlank: true);
        }

        void Verify_OnSearchTypeChanged_IsSearchCriteriaEnabled(string searchType, bool isSearchCriteriaEnabled, bool isSearchCriteriaBlank)
        {
            //------------Setup for test--------------------------
            var findRecordsTO = new FindRecordsTO("xxxx", searchType, 1);

            var items = new List<FindRecordsTO>
            {
                findRecordsTO
            };

            var viewModel = new FindRecordsMultipleCriteriaDesignerViewModel(CreateModelItem(items));

            //------------Precondition---------------------------           
            Assert.IsFalse(findRecordsTO.IsSearchCriteriaEnabled);

            //------------Execute Test---------------------------
            viewModel.SearchTypeUpdatedCommand.Execute(0);

            //------------Assert Results-------------------------
            Assert.AreEqual(isSearchCriteriaEnabled, findRecordsTO.IsSearchCriteriaEnabled);
            Assert.AreEqual(isSearchCriteriaBlank, string.IsNullOrEmpty(findRecordsTO.SearchCriteria));
        }


        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("FindRecordsMultipleCriteriaViewModel_OnSearchTypeChanged")]
        public void FindRecordsMultipleCriteriaViewModel_OnSearchTypeChanged_IndexOutOfBounds_DoesNothing()
        {
            Verify_OnSearchTypeChanged_IndexOutOfBounds_DoesNothing(-2);
            Verify_OnSearchTypeChanged_IndexOutOfBounds_DoesNothing(2);
            Verify_OnSearchTypeChanged_IndexOutOfBounds_DoesNothing(3);
        }

        void Verify_OnSearchTypeChanged_IndexOutOfBounds_DoesNothing(int index)
        {
            //------------Setup for test--------------------------
            var items = new List<FindRecordsTO>
            {
                new FindRecordsTO("xxxx", "Equals", 1),
                new FindRecordsTO("yyyy", "Contains", 2)
            };

            var viewModel = new FindRecordsMultipleCriteriaDesignerViewModel(CreateModelItem(items));

            //------------Precondition---------------------------     
            foreach(var dto in items)
            {
                Assert.IsFalse(dto.IsSearchCriteriaEnabled);
                Assert.IsFalse(string.IsNullOrEmpty(dto.SearchCriteria));
            }

            //------------Execute Test---------------------------
            viewModel.SearchTypeUpdatedCommand.Execute(index);

            //------------Assert Results-------------------------
            foreach(var dto in items)
            {
                Assert.IsFalse(dto.IsSearchCriteriaEnabled);
                Assert.IsFalse(string.IsNullOrEmpty(dto.SearchCriteria));
            }
        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("FindRecordsMultipleCriteriaViewModel_Constructor")]
        public void FindRecordsMultipleCriteriaViewModel_Constructor_PropertiesInitialized()
        {
            //------------Setup for test--------------------------
            var items = new List<FindRecordsTO>
            {
                new FindRecordsTO("xxxx", "=", 1),
                new FindRecordsTO("yyyy", "Contains", 2)
            };


            //------------Execute Test---------------------------
            var viewModel = new FindRecordsMultipleCriteriaDesignerViewModel(CreateModelItem(items));

            //------------Assert Results-------------------------
            Assert.IsNotNull(viewModel.ModelItem);
            Assert.IsNotNull(viewModel.ModelItemCollection);
            Assert.AreEqual("ResultsCollection", viewModel.CollectionName);
            Assert.AreEqual(2, viewModel.TitleBarToggles.Count);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("FindRecordsMultipleCriteriaDesignerViewModel_ValidateThis")]
        public void FindRecordsMultipleCriteriaDesignerViewModel_ValidateThis_FieldsToSearchIsNotEmptyAndResultNotEmpty_DoesNotHaveErrors()
        {
            //------------Setup for test--------------------------
            var items = new List<FindRecordsTO> { new FindRecordsTO("", "", 0) };
            var mi = CreateModelItem(items);
            mi.SetProperty("FieldsToSearch", "[[recset().field]]");
            mi.SetProperty("Result", "[[res]]");
            var viewModel = new FindRecordsMultipleCriteriaDesignerViewModel(mi);


            //------------Execute Test---------------------------
            viewModel.Validate();

            //------------Assert Results-------------------------
            Assert.IsNull(viewModel.Errors);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("FindRecordsMultipleCriteriaDesignerViewModel_ValidateThis")]
        public void FindRecordsMultipleCriteriaDesignerViewModel_ValidateThis_FieldsToSearchAndResultIsEmptyOrWhiteSpace_DoesHaveErrors()
        {
            //------------Setup for test--------------------------
            var items = new List<FindRecordsTO> { new FindRecordsTO("", "", 0) };
            var mi = CreateModelItem(items);
            mi.SetProperty("FieldsToSearch", " ");
            mi.SetProperty("Result", " ");
            var viewModel = new FindRecordsMultipleCriteriaDesignerViewModel(mi);

            //------------Execute Test---------------------------
            viewModel.Validate();

            //------------Assert Results-------------------------
            Assert.AreEqual(2, viewModel.Errors.Count);
            StringAssert.Contains(viewModel.Errors[0].Message, "'In Field(s)' cannot be empty or only white space");
            StringAssert.Contains(viewModel.Errors[1].Message, "'Result' cannot be empty or only white space");
        }


        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("FindRecordsMultipleCriteriaDesignerViewModel_ValidateCollectionItem")]
        public void FindRecordsMultipleCriteriaDesignerViewModel_ValidateCollectionItem_ValidatesPropertiesOfTO()
        {
            //------------Setup for test--------------------------
            var mi = ModelItemUtils.CreateModelItem(new DsfFindRecordsMultipleCriteriaActivity());
            mi.SetProperty("DisplayName", "Find");
            mi.SetProperty("FieldsToSearch", "a,b");
            mi.SetProperty("Result", "[[res]]");

            var dto1 = new FindRecordsTO("", "Starts With", 0);
            var dto2 = new FindRecordsTO("", "Ends With", 1);
            var dto3 = new FindRecordsTO("", "Doesn't Start With", 2);
            var dto4 = new FindRecordsTO("", "Doesn't End With", 3);
            var dto5 = new FindRecordsTO("", "Is Between", 4);
            var dto6 = new FindRecordsTO("", "Is Not Between", 5);

            // ReSharper disable PossibleNullReferenceException
            var miCollection = mi.Properties["ResultsCollection"].Collection;
            var dtoModelItem1 = miCollection.Add(dto1);
            var dtoModelItem2 = miCollection.Add(dto2);
            var dtoModelItem3 = miCollection.Add(dto3);
            var dtoModelItem4 = miCollection.Add(dto4);
            var dtoModelItem5 = miCollection.Add(dto5);
            var dtoModelItem6 = miCollection.Add(dto6);
            // ReSharper restore PossibleNullReferenceException

            var viewModel = new FindRecordsMultipleCriteriaDesignerViewModel(mi);

            //------------Execute Test---------------------------
            viewModel.Validate();

            //------------Assert Results-------------------------
            Assert.AreEqual(8, viewModel.Errors.Count);

            StringAssert.Contains(viewModel.Errors[0].Message, "'Match' cannot be empty");
            Verify_IsFocused(dtoModelItem1, viewModel.Errors[0].Do, "IsSearchCriteriaFocused");

            StringAssert.Contains(viewModel.Errors[1].Message, "'Match' cannot be empty");
            Verify_IsFocused(dtoModelItem2, viewModel.Errors[1].Do, "IsSearchCriteriaFocused");

            StringAssert.Contains(viewModel.Errors[2].Message, "'Match' cannot be empty");
            Verify_IsFocused(dtoModelItem3, viewModel.Errors[2].Do, "IsSearchCriteriaFocused");

            StringAssert.Contains(viewModel.Errors[3].Message, "'Match' cannot be empty");
            Verify_IsFocused(dtoModelItem4, viewModel.Errors[3].Do, "IsSearchCriteriaFocused");

            StringAssert.Contains(viewModel.Errors[4].Message, "'From' cannot be empty");
            Verify_IsFocused(dtoModelItem5, viewModel.Errors[4].Do, "IsFromFocused");

            StringAssert.Contains(viewModel.Errors[5].Message, "'To' cannot be empty");
            Verify_IsFocused(dtoModelItem5, viewModel.Errors[5].Do, "IsToFocused");

            StringAssert.Contains(viewModel.Errors[6].Message, "'From' cannot be empty");
            Verify_IsFocused(dtoModelItem6, viewModel.Errors[6].Do, "IsFromFocused");

            StringAssert.Contains(viewModel.Errors[7].Message, "'To' cannot be empty");
            Verify_IsFocused(dtoModelItem6, viewModel.Errors[7].Do, "IsToFocused");

        }

        void Verify_IsFocused(ModelItem modelItem, Action doError, string isFocusedPropertyName)
        {
            Assert.IsFalse(modelItem.GetProperty<bool>(isFocusedPropertyName));
            doError.Invoke();
            Assert.IsTrue(modelItem.GetProperty<bool>(isFocusedPropertyName));
        }


        static ModelItem CreateModelItem(IEnumerable<FindRecordsTO> items, string displayName = "Find")
        {
            var modelItem = ModelItemUtils.CreateModelItem(new DsfFindRecordsMultipleCriteriaActivity());
            modelItem.SetProperty("DisplayName", displayName);

            // ReSharper disable PossibleNullReferenceException
            var modelItemCollection = modelItem.Properties["ResultsCollection"].Collection;
            foreach(var dto in items)
            {
                modelItemCollection.Add(dto);
            }
            // ReSharper restore PossibleNullReferenceException

            return modelItem;
        }
    }
}
