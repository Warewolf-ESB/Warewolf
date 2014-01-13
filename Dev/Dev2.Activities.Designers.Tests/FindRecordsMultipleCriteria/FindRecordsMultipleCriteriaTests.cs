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
            Verify_OnSearchTypeChanged_IndexOutOfBounds_DoesNothing(-1);
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
            Assert.AreEqual(1, viewModel.TitleBarToggles.Count);
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
