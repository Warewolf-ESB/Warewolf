using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Providers.Validation.Rules;
using Dev2.TO;
using Dev2.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Dev2.Tests.Activities.TOTests
{
    [TestClass]
    public class SharepointSearchToTests
    {
        [TestMethod]
        [Timeout(60000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("SharepointSearchTo_Constructor")]
        public void SharepointSearchTo_Constructor_Default_SetsProperties()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var searchTo = new SharepointSearchTo();
            //------------Assert Results-------------------------
            Assert.IsNotNull(searchTo);
            Assert.AreEqual("Field Name", searchTo.FieldName);
            Assert.AreEqual("", searchTo.ValueToMatch);
            Assert.AreEqual("Equal", searchTo.SearchType);
            Assert.AreEqual(0, searchTo.IndexNumber);
            Assert.IsFalse(searchTo.Inserted);
            Assert.IsFalse(searchTo.IsSearchCriteriaEnabled);

        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("SharepointSearchTo_Constructor")]
        public void SharepointSearchTo_ParameterConstructor_SetsProperties()
        {
            //------------Setup for test--------------------------
            const string searchCriteria = "Bob";
            const string searchType = ">";
            const int indexNum = 3;
            const string fieldName = "Title";
            //------------Execute Test---------------------------
            var searchTo = new SharepointSearchTo(fieldName,searchType, searchCriteria, indexNum);
            //------------Assert Results-------------------------
            Assert.IsNotNull(searchTo);
            Assert.AreEqual(fieldName, searchTo.FieldName);
            Assert.AreEqual(searchCriteria, searchTo.ValueToMatch);
            Assert.AreEqual(searchType, searchTo.SearchType);
            Assert.AreEqual(indexNum, searchTo.IndexNumber);
            Assert.IsFalse(searchTo.Inserted);
            Assert.IsFalse(searchTo.IsSearchCriteriaEnabled);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("SharepointSearchTo_SearchType")]
        public void SharepointSearchTo_SearchType_SetValue_FiresNotifyPropertyChanged()
        {
            //------------Setup for test--------------------------
            var searchTo = new SharepointSearchTo();
            const string searchType = "MyValue";
            //------------Execute Test---------------------------
            
            var notifyPropertyChanged = TestUtils.PropertyChangedTester(searchTo, () => searchTo.SearchType, () => searchTo.SearchType = searchType);
            
            //------------Assert Results-------------------------
            Assert.AreEqual(searchType, searchTo.SearchType);
            Assert.IsTrue(notifyPropertyChanged);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("SharepointSearchTo_SearchType")]
        public void SharepointSearchTo_SearchCriteria_SetValue_FiresNotifyPropertyChanged()
        {
            //------------Setup for test--------------------------
            var searchTo = new SharepointSearchTo();
            const string searchCriteria = "MyValue";
            //------------Execute Test---------------------------
            
            var notifyPropertyChanged = TestUtils.PropertyChangedTester(searchTo, () => searchTo.ValueToMatch, () => searchTo.ValueToMatch = searchCriteria);
            
            //------------Assert Results-------------------------
            Assert.AreEqual(searchCriteria, searchTo.ValueToMatch);
            Assert.IsTrue(notifyPropertyChanged);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("SharepointSearchTo_IndexNum")]
        public void SharepointSearchTo_IndexNum_SetValue_FiresNotifyPropertyChanged()
        {
            //------------Setup for test--------------------------
            var searchTo = new SharepointSearchTo();
            const int indexNum = 5;
            //------------Execute Test---------------------------
            var notifyPropertyChanged = TestUtils.PropertyChangedTester(searchTo, () => searchTo.IndexNumber, () => searchTo.IndexNumber = indexNum);
            //------------Assert Results-------------------------
            Assert.AreEqual(indexNum, searchTo.IndexNumber);
            Assert.IsTrue(notifyPropertyChanged);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("SharepointSearchTo_IsSearchCriteriaEnabled")]
        public void SharepointSearchTo_IsSearchCriteriaEnabled_SetValue_FiresNotifyPropertyChanged()
        {
            //------------Setup for test--------------------------
            var searchTo = new SharepointSearchTo();
            const bool isSearchCriteriaEnabled = true;
            //------------Execute Test---------------------------
            var notifyPropertyChanged = TestUtils.PropertyChangedTester(searchTo, () => searchTo.IsSearchCriteriaEnabled, () => searchTo.IsSearchCriteriaEnabled = isSearchCriteriaEnabled);
            //------------Assert Results-------------------------
            Assert.AreEqual(isSearchCriteriaEnabled, searchTo.IsSearchCriteriaEnabled);
            Assert.IsTrue(notifyPropertyChanged);
        }

        #region CanAdd Tests

        [TestMethod]
        [Timeout(60000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("SharepointSearchTo_CanAdd")]
        public void SharepointSearchTo_CanAdd_SearchTypeEmpty_ReturnFalse()
        {
            //------------Setup for test--------------------------
            var searchTo = new SharepointSearchTo { SearchType = string.Empty };
            //------------Execute Test---------------------------
            var canAdd = searchTo.CanAdd();
            //------------Assert Results-------------------------
            Assert.IsFalse(canAdd);
        }
        
        [TestMethod]
        [Timeout(60000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("SharepointSearchTo_CanAdd")]
        public void SharepointSearchTo_CanAdd_FieldNameEmpty_ReturnFalse()
        {
            //------------Setup for test--------------------------
            var searchTo = new SharepointSearchTo { SearchType = "Equal",FieldName = string.Empty};
            //------------Execute Test---------------------------
            var canAdd = searchTo.CanAdd();
            //------------Assert Results-------------------------
            Assert.IsFalse(canAdd);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("SharepointSearchTo_CanAdd")]
        public void SharepointSearchTo_CanAdd_SearchTypeWithData_ReturnTrue()
        {
            //------------Setup for test--------------------------
            var searchTo = new SharepointSearchTo { SearchType = "Contains", FieldName = "Title"};
            //------------Execute Test---------------------------
            var canAdd = searchTo.CanAdd();
            //------------Assert Results-------------------------
            Assert.IsTrue(canAdd);
        }

        #endregion

        #region CanRemove Tests

        [TestMethod]
        [Timeout(60000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("SharepointSearchTo_CanRemove")]
        public void SharepointSearchTo_CanRemove_FieldNameEmpty_ReturnTrue()
        {
            //------------Setup for test--------------------------
            var searchTo = new SharepointSearchTo { FieldName = string.Empty };
            //------------Execute Test---------------------------
            var canRemove = searchTo.CanRemove();
            //------------Assert Results-------------------------
            Assert.IsTrue(canRemove);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("SharepointSearchTo_CanRemove")]
        public void SharepointSearchTo_CanRemove_FieldNameEmptyWithData_ReturnFalse()
        {
            //------------Setup for test--------------------------
            var searchTo = new SharepointSearchTo { FieldName = "Contains"};
            //------------Execute Test---------------------------
            var canRemove = searchTo.CanRemove();
            //------------Assert Results-------------------------
            Assert.IsFalse(canRemove);
        }


        [TestMethod]
        [Timeout(60000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("SharepointSearchTo_GetRuleSet")]
        public void SharepointSearchTo_GetRuleSet_OnValueToMatchNoValue_ReturnTwoRules()
        {
            //------------Setup for test--------------------------
            var searchTo = new SharepointSearchTo { SearchType = "Equals", ValueToMatch = string.Empty };
            VerifyCorrectRulesForEachField(searchTo, "ValueToMatch", new List<Type> { typeof(IsStringEmptyRule), typeof(IsValidExpressionRule) });
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("SharepointSearchTo_GetRuleSet")]
        public void SharepointSearchTo_GetRuleSet_OnValueToMatchWithValue_ReturnOneRules()
        {
            //------------Setup for test--------------------------
            var searchTo = new SharepointSearchTo { SearchType = "Equals", ValueToMatch = "Bob" };
            VerifyCorrectRulesForEachField(searchTo, "ValueToMatch", new List<Type> { typeof(IsValidExpressionRule) });
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("SharepointSearchTo_GetRuleSet")]
        public void SharepointSearchTo_GetRuleSet_OnFieldNameNoValue_ReturnTwoRules()
        {
            //------------Setup for test--------------------------
            var searchTo = new SharepointSearchTo {  FieldName = string.Empty };
            VerifyCorrectRulesForEachField(searchTo, "FieldName",new List<Type> { typeof(IsStringEmptyRule) });
        }

        static void VerifyCorrectRulesForEachField(SharepointSearchTo searchTo, string fieldName,List<Type> ruleTypes)
        {
            //------------Execute Test---------------------------
            var rulesSet = searchTo.GetRuleSet(fieldName, "");
            //------------Assert Results-------------------------
            Assert.IsNotNull(rulesSet);
            Assert.AreEqual(ruleTypes.Count,rulesSet.Rules.Count);
            var returnedRuleTypes = rulesSet.Rules.Select(rule => rule.GetType());
            CollectionAssert.AreEqual(ruleTypes,returnedRuleTypes.ToList());
        }
        
        #endregion
    }
}