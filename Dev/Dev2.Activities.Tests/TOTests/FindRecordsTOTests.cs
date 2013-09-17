using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;
// ReSharper disable InconsistentNaming
namespace Dev2.Tests.Activities.TOTests
{
    [TestClass]
    public class FindRecordsTOTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("FindRecordsTO_Constructor")]
        public void FindRecordsTO_Constructor_Default_SetsProperties()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var findRecordsTO = new FindRecordsTO();
            //------------Assert Results-------------------------
            Assert.IsNotNull(findRecordsTO);
            Assert.AreEqual("Match On",findRecordsTO.SearchCriteria);
            Assert.AreEqual("Equal",findRecordsTO.SearchType);
            Assert.AreEqual(0,findRecordsTO.IndexNumber);
            Assert.IsFalse(findRecordsTO.Inserted);
            Assert.IsFalse(findRecordsTO.IsSearchCriteriaEnabled);

        }
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("FindRecordsTO_Constructor")]
        public void FindRecordsTO_ParameterConstructor_SetsProperties()
        {
            //------------Setup for test--------------------------
            const string searchCriteria = "Bob";
            const string searchType = ">";
            const int indexNum = 3;
            //------------Execute Test---------------------------
            var findRecordsTO = new FindRecordsTO(searchCriteria,searchType,indexNum);
            //------------Assert Results-------------------------
            Assert.IsNotNull(findRecordsTO);
            Assert.AreEqual(searchCriteria,findRecordsTO.SearchCriteria);
            Assert.AreEqual(searchType,findRecordsTO.SearchType);
            Assert.AreEqual(indexNum,findRecordsTO.IndexNumber);
            Assert.IsFalse(findRecordsTO.Inserted);
            Assert.IsFalse(findRecordsTO.IsSearchCriteriaEnabled);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("FindRecordsTO_SearchType")]
        public void FindRecordsTO_SearchType_SetValue_FiresNotifyPropertyChanged()
        {
            //------------Setup for test--------------------------
            var findRecordsTO = new FindRecordsTO();
            const string searchType = "MyValue";
            //------------Execute Test---------------------------
            var notifyPropertyChanged = TestUtils.PropertyChangedTester(findRecordsTO, () => findRecordsTO.SearchType, () => findRecordsTO.SearchType = searchType);
            //------------Assert Results-------------------------
            Assert.AreEqual(searchType,findRecordsTO.SearchType);
            Assert.IsTrue(notifyPropertyChanged);
        } 
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("FindRecordsTO_SearchType")]
        public void FindRecordsTO_SearchCriteria_SetValue_FiresNotifyPropertyChanged()
        {
            //------------Setup for test--------------------------
            var findRecordsTO = new FindRecordsTO();
            const string searchCriteria = "MyValue";
            //------------Execute Test---------------------------
            var notifyPropertyChanged = TestUtils.PropertyChangedTester(findRecordsTO, () => findRecordsTO.SearchCriteria, () => findRecordsTO.SearchCriteria = searchCriteria);
            //------------Assert Results-------------------------
            Assert.AreEqual(searchCriteria, findRecordsTO.SearchCriteria);
            Assert.IsTrue(notifyPropertyChanged);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("FindRecordsTO_IndexNum")]
        public void FindRecordsTO_IndexNum_SetValue_FiresNotifyPropertyChanged()
        {
            //------------Setup for test--------------------------
            var findRecordsTO = new FindRecordsTO();
            const int indexNum = 5;
            //------------Execute Test---------------------------
            var notifyPropertyChanged = TestUtils.PropertyChangedTester(findRecordsTO, () => findRecordsTO.IndexNumber, () => findRecordsTO.IndexNumber = indexNum);
            //------------Assert Results-------------------------
            Assert.AreEqual(indexNum, findRecordsTO.IndexNumber);
            Assert.IsTrue(notifyPropertyChanged);
        }  
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("FindRecordsTO_IsSearchCriteriaEnabled")]
        public void FindRecordsTO_IsSearchCriteriaEnabled_SetValue_FiresNotifyPropertyChanged()
        {
            //------------Setup for test--------------------------
            var findRecordsTO = new FindRecordsTO();
            const bool isSearchCriteriaEnabled = true;
            //------------Execute Test---------------------------
            var notifyPropertyChanged = TestUtils.PropertyChangedTester(findRecordsTO, () => findRecordsTO.IsSearchCriteriaEnabled, () => findRecordsTO.IsSearchCriteriaEnabled = isSearchCriteriaEnabled);
            //------------Assert Results-------------------------
            Assert.AreEqual(isSearchCriteriaEnabled, findRecordsTO.IsSearchCriteriaEnabled);
            Assert.IsTrue(notifyPropertyChanged);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("FindRecordsTO_SearchType")]
        public void FindRecordsTO_SearchType_Equal_RequiresCriteriaInput_IsCriteriaEnabledTrue()
        {
            //------------Setup for test--------------------------
            var findRecordsTO = new FindRecordsTO();
            const string searchType = "Equal";
            //------------Precondition---------------------------
            Assert.IsFalse(findRecordsTO.IsSearchCriteriaEnabled);
            //------------Execute Test---------------------------
            findRecordsTO.SearchType = searchType;
            //------------Assert Results-------------------------
            Assert.IsTrue(findRecordsTO.IsSearchCriteriaEnabled);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("FindRecordsTO_SearchType")]
        public void FindRecordsTO_SearchType_NotContains_RequiresCriteriaInput_IsCriteriaEnabledTrue()
        {
            //------------Setup for test--------------------------
            var findRecordsTO = new FindRecordsTO();
            const string searchType = "Not Contains";
            //------------Precondition---------------------------
            Assert.IsFalse(findRecordsTO.IsSearchCriteriaEnabled);
            //------------Execute Test---------------------------
            findRecordsTO.SearchType = searchType;
            //------------Assert Results-------------------------
            Assert.IsTrue(findRecordsTO.IsSearchCriteriaEnabled);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("FindRecordsTO_SearchType")]
        public void FindRecordsTO_SearchType_Contains_RequiresCriteriaInput_IsCriteriaEnabledTrue()
        {
            //------------Setup for test--------------------------
            var findRecordsTO = new FindRecordsTO();
            const string searchType = "Contains";
            //------------Precondition---------------------------
            Assert.IsFalse(findRecordsTO.IsSearchCriteriaEnabled);
            //------------Execute Test---------------------------
            findRecordsTO.SearchType = searchType;
            //------------Assert Results-------------------------
            Assert.IsTrue(findRecordsTO.IsSearchCriteriaEnabled);
        }
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("FindRecordsTO_SearchType")]
        public void FindRecordsTO_SearchType_NotEqual_RequiresCriteriaInput_IsCriteriaEnabledTrue()
        {
            //------------Setup for test--------------------------
            var findRecordsTO = new FindRecordsTO();
            const string searchType = "Not Equal";
            //------------Precondition---------------------------
            Assert.IsFalse(findRecordsTO.IsSearchCriteriaEnabled);
            //------------Execute Test---------------------------
            findRecordsTO.SearchType = searchType;
            //------------Assert Results-------------------------
            Assert.IsTrue(findRecordsTO.IsSearchCriteriaEnabled);
        }
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("FindRecordsTO_SearchType")]
        public void FindRecordsTO_SearchType_EndsWith_RequiresCriteriaInput_IsCriteriaEnabledTrue()
        {
            //------------Setup for test--------------------------
            var findRecordsTO = new FindRecordsTO();
            const string searchType = "Ends With";
            //------------Precondition---------------------------
            Assert.IsFalse(findRecordsTO.IsSearchCriteriaEnabled);
            //------------Execute Test---------------------------
            findRecordsTO.SearchType = searchType;
            //------------Assert Results-------------------------
            Assert.IsTrue(findRecordsTO.IsSearchCriteriaEnabled);
        }
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("FindRecordsTO_SearchType")]
        public void FindRecordsTO_SearchType_StartsWith_RequiresCriteriaInput_IsCriteriaEnabledTrue()
        {
            //------------Setup for test--------------------------
            var findRecordsTO = new FindRecordsTO();
            const string searchType = "Starts With";
            //------------Precondition---------------------------
            Assert.IsFalse(findRecordsTO.IsSearchCriteriaEnabled);
            //------------Execute Test---------------------------
            findRecordsTO.SearchType = searchType;
            //------------Assert Results-------------------------
            Assert.IsTrue(findRecordsTO.IsSearchCriteriaEnabled);
        }
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("FindRecordsTO_SearchType")]
        public void FindRecordsTO_SearchType_Regex_RequiresCriteriaInput_IsCriteriaEnabledTrue()
        {
            //------------Setup for test--------------------------
            var findRecordsTO = new FindRecordsTO();
            const string searchType = "Regex";
            //------------Precondition---------------------------
            Assert.IsFalse(findRecordsTO.IsSearchCriteriaEnabled);
            //------------Execute Test---------------------------
            findRecordsTO.SearchType = searchType;
            //------------Assert Results-------------------------
            Assert.IsTrue(findRecordsTO.IsSearchCriteriaEnabled);
        }
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("FindRecordsTO_SearchType")]
        public void FindRecordsTO_SearchType_GreaterThan_RequiresCriteriaInput_IsCriteriaEnabledTrue()
        {
            //------------Setup for test--------------------------
            var findRecordsTO = new FindRecordsTO();
            const string searchType = ">";
            //------------Precondition---------------------------
            Assert.IsFalse(findRecordsTO.IsSearchCriteriaEnabled);
            //------------Execute Test---------------------------
            findRecordsTO.SearchType = searchType;
            //------------Assert Results-------------------------
            Assert.IsTrue(findRecordsTO.IsSearchCriteriaEnabled);
        }
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("FindRecordsTO_SearchType")]
        public void FindRecordsTO_SearchType_LessThan_RequiresCriteriaInput_IsCriteriaEnabledTrue()
        {
            //------------Setup for test--------------------------
            var findRecordsTO = new FindRecordsTO();
            const string searchType = "<";
            //------------Precondition---------------------------
            Assert.IsFalse(findRecordsTO.IsSearchCriteriaEnabled);
            //------------Execute Test---------------------------
            findRecordsTO.SearchType = searchType;
            //------------Assert Results-------------------------
            Assert.IsTrue(findRecordsTO.IsSearchCriteriaEnabled);
        }
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("FindRecordsTO_SearchType")]
        public void FindRecordsTO_SearchType_LessThanEqual_RequiresCriteriaInput_IsCriteriaEnabledTrue()
        {
            //------------Setup for test--------------------------
            var findRecordsTO = new FindRecordsTO();
            const string searchType = "<=";
            //------------Precondition---------------------------
            Assert.IsFalse(findRecordsTO.IsSearchCriteriaEnabled);
            //------------Execute Test---------------------------
            findRecordsTO.SearchType = searchType;
            //------------Assert Results-------------------------
            Assert.IsTrue(findRecordsTO.IsSearchCriteriaEnabled);
        }
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("FindRecordsTO_SearchType")]
        public void FindRecordsTO_SearchType_GreaterThanEqual_RequiresCriteriaInput_IsCriteriaEnabledTrue()
        {
            //------------Setup for test--------------------------
            var findRecordsTO = new FindRecordsTO();
            const string searchType = ">=";
            //------------Precondition---------------------------
            Assert.IsFalse(findRecordsTO.IsSearchCriteriaEnabled);
            //------------Execute Test---------------------------
            findRecordsTO.SearchType = searchType;
            //------------Assert Results-------------------------
            Assert.IsTrue(findRecordsTO.IsSearchCriteriaEnabled);
        }  
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("FindRecordsTO_SearchType")]
        public void FindRecordsTO_SearchType_NotInRequiresCriteriaInputList_IsCriteriaEnabledFalseSearchCriteriaEmptyString()
        {
            //------------Setup for test--------------------------
            var findRecordsTO = new FindRecordsTO();
            const string searchType = "Is Numeric";
            //------------Precondition---------------------------
            Assert.IsFalse(findRecordsTO.IsSearchCriteriaEnabled);
            //------------Execute Test---------------------------
            findRecordsTO.SearchType = searchType;
            //------------Assert Results-------------------------
            Assert.IsFalse(findRecordsTO.IsSearchCriteriaEnabled);
            Assert.AreEqual(string.Empty,findRecordsTO.SearchCriteria);
        }
    }
}
