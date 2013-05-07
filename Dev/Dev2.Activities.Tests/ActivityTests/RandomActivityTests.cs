using System;
using System.Activities.Statements;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using ActivityUnitTests;
using Dev2.Activities;
using Dev2.Common;
using Dev2.Common.Enums;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.ActivityTests
{
    /// <summary>
    /// Summary description for RandomActivityTests
    /// </summary>
    [TestClass]
    public class RandomActivityTests : BaseActivityUnitTest
    {
        public RandomActivityTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        #region Numbers Tests

        [TestMethod]
        public void GenerateRandomNumberWithStaticInputsExpectedARandomNumberToBeOutput()
        {
            int start = 10;
            int end = 20;
            SetupArguments(ActivityStrings.RandomActivityDataListWithData, ActivityStrings.RandomActivityDataListShape, enRandomType.Numbers, start.ToString(), end.ToString(), string.Empty, "[[OutVar1]]");

            IDSFDataObject result = ExecuteProcess();

            string actual = string.Empty;
            string error = string.Empty;
            GetScalarValueFromDataList(result.DataListID, "OutVar1", out actual, out error);
            int actualNum;
            int.TryParse(actual, out actualNum);
            if (string.IsNullOrEmpty(error))
            {
                Assert.IsTrue(actualNum >= start && actualNum <= end);
            }
            else
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }
        }

        [TestMethod]
        public void GenerateRandomNumberWithRecordsetWithStarInFromAndToFieldsOutputToRecordsetExpectedRecordsetToHaveFiveNumbers()
        {
            SetupArguments(ActivityStrings.RandomActivityDataListWithData, ActivityStrings.RandomActivityDataListShape, enRandomType.Numbers, "[[recset2(*).field2]]", "[[recset1(*).field1]]", string.Empty, "[[recset2(*).field2]]");

            IDSFDataObject result = ExecuteProcess();
           
            string error = string.Empty;
            IList<IBinaryDataListItem> dataListItems;
            GetRecordSetFieldValueFromDataList(result.DataListID, "recset2", "field2", out dataListItems, out error);
            
            if (string.IsNullOrEmpty(error))
            {
                Assert.IsTrue(dataListItems.Count == 5);
                int firstRes;
                int.TryParse(dataListItems[0].TheValue, out firstRes);
                Assert.IsTrue(firstRes >= -10 && firstRes <= 10);
                int secondRes;
                int.TryParse(dataListItems[1].TheValue, out secondRes);
                Assert.IsTrue(secondRes >= -20 && secondRes <= 20);
                int thirdRes;
                int.TryParse(dataListItems[2].TheValue, out thirdRes);
                Assert.IsTrue(thirdRes >= -30 && thirdRes <= 30);
                int forthRes;
                int.TryParse(dataListItems[3].TheValue, out forthRes);
                Assert.IsTrue(forthRes >= -40 && forthRes <= 40);
                int fithRes;
                int.TryParse(dataListItems[4].TheValue, out fithRes);
                Assert.IsTrue(fithRes >= -50 && fithRes <= 50);
            }
            else
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }
        }

        [TestMethod]
        public void GenerateRandomNumberWithRecordsetWithBlankInOutputExpectedRecordsetToHaveTenNumbers()
        {
            SetupArguments(ActivityStrings.RandomActivityDataListWithData, ActivityStrings.RandomActivityDataListShape, enRandomType.Numbers, "[[recset2(*).field2]]", "[[recset1(*).field1]]", string.Empty, "[[recset2().field2]]");

            IDSFDataObject result = ExecuteProcess();

            string error = string.Empty;
            IList<IBinaryDataListItem> dataListItems;
            GetRecordSetFieldValueFromDataList(result.DataListID, "recset2", "field2", out dataListItems, out error);

            if (string.IsNullOrEmpty(error))
            {
                Assert.IsTrue(dataListItems.Count == 10);
                int firstRes;
                int.TryParse(dataListItems[5].TheValue, out firstRes);
                Assert.IsTrue(firstRes >= -10 && firstRes <= 10);
                int secondRes;
                int.TryParse(dataListItems[6].TheValue, out secondRes);
                Assert.IsTrue(secondRes >= -20 && secondRes <= 20);
                int thirdRes;
                int.TryParse(dataListItems[7].TheValue, out thirdRes);
                Assert.IsTrue(thirdRes >= -30 && thirdRes <= 30);
                int forthRes;
                int.TryParse(dataListItems[8].TheValue, out forthRes);
                Assert.IsTrue(forthRes >= -40 && forthRes <= 40);
                int fithRes;
                int.TryParse(dataListItems[9].TheValue, out fithRes);
                Assert.IsTrue(fithRes >= -50 && fithRes <= 50);
            }
            else
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }
        }

        [TestMethod]
        public void GenerateRandomNumberWithBlankStringInFromExpectedError()
        {
            SetupArguments(ActivityStrings.RandomActivityDataListWithData, ActivityStrings.RandomActivityDataListShape, enRandomType.Numbers, "", "[[recset1(1).field1]]", string.Empty, "[[recset2().field2]]");

            IDSFDataObject result = ExecuteProcess();

            string error = string.Empty;
            string expected = "<InnerError>Please ensure that you have entered an integer for Start.</InnerError>";
            string actual;
            GetScalarValueFromDataList(result.DataListID, GlobalConstants.ErrorPayload, out actual, out error);

            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual);                
            }
            else
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }
        }

        [TestMethod]
        public void GenerateRandomNumberWithBlankStringInToExpectedError()
        {
            SetupArguments(ActivityStrings.RandomActivityDataListWithData, ActivityStrings.RandomActivityDataListShape, enRandomType.Numbers, "[[recset2(1).field2]]", "", string.Empty, "[[recset2().field2]]");

            IDSFDataObject result = ExecuteProcess();

            string error = string.Empty;
            string expected = "<InnerError>Please ensure that you have entered an integer for End.</InnerError>";
            string actual;
            GetScalarValueFromDataList(result.DataListID, GlobalConstants.ErrorPayload, out actual, out error);

            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual);
            }
            else
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }
        }

        [TestMethod]
        public void GenerateRandomNumberWithFromHigherThenToExpectedError()
        {
            SetupArguments(ActivityStrings.RandomActivityDataListWithData, ActivityStrings.RandomActivityDataListShape, enRandomType.Numbers, "[[recset1(1).field1]]", "[[recset2(1).field2]]", string.Empty, "[[recset2().field2]]");

            IDSFDataObject result = ExecuteProcess();

            string error = string.Empty;
            string expected = "<InnerError>Please make sure that your start number is less than your end number.</InnerError>";
            string actual;
            GetScalarValueFromDataList(result.DataListID, GlobalConstants.ErrorPayload, out actual, out error);

            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual);
            }
            else
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }
        }

        [TestMethod]
        public void GenerateRandomNumberWithFromBlankExpectedError()
        {
            SetupArguments(ActivityStrings.RandomActivityDataListWithData, ActivityStrings.RandomActivityDataListShape, enRandomType.Numbers, "", "[[recset2(1).field2]]", string.Empty, "[[recset2().field2]]");

            IDSFDataObject result = ExecuteProcess();

            string error = string.Empty;
            string expected = "<InnerError>Please ensure that you have entered an integer for Start.</InnerError>";
            string actual;
            GetScalarValueFromDataList(result.DataListID, GlobalConstants.ErrorPayload, out actual, out error);

            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual);
            }
            else
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }
        }

        [TestMethod]
        public void GenerateRandomNumberWithToBlankExpectedError()
        {
            SetupArguments(ActivityStrings.RandomActivityDataListWithData, ActivityStrings.RandomActivityDataListShape, enRandomType.Numbers, "[[recset1(1).field1]]", "", string.Empty, "[[recset2().field2]]");

            IDSFDataObject result = ExecuteProcess();

            string error = string.Empty;
            string expected = "<InnerError>Please ensure that you have entered an integer for End.</InnerError>";
            string actual;
            GetScalarValueFromDataList(result.DataListID, GlobalConstants.ErrorPayload, out actual, out error);

            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual);
            }
            else
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }
        }

        [TestMethod]
        public void GenerateRandomNumberWithToFieldWithLettersExpectedError()
        {
            SetupArguments(ActivityStrings.RandomActivityDataListWithData, ActivityStrings.RandomActivityDataListShape, enRandomType.Numbers, "[[recset1(1).field1]]", "letters", "", "[[recset2().field2]]");

            IDSFDataObject result = ExecuteProcess();

            string error = string.Empty;
            string expected = "<InnerError>Please ensure that the End is an integer.</InnerError>";
            string actual;
            GetScalarValueFromDataList(result.DataListID, GlobalConstants.ErrorPayload, out actual, out error);

            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual);
            }
            else
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }
        }

        [TestMethod]
        public void GenerateRandomNumberWithFromFieldWithLettersExpectedError()
        {
            SetupArguments(ActivityStrings.RandomActivityDataListWithData, ActivityStrings.RandomActivityDataListShape, enRandomType.Numbers, "letters", "[[recset1(1).field1]]", string.Empty, "[[recset2().field2]]");

            IDSFDataObject result = ExecuteProcess();

            string error = string.Empty;
            string expected = "<InnerError>Please ensure that the Start is an integer.</InnerError>";
            string actual;
            GetScalarValueFromDataList(result.DataListID, GlobalConstants.ErrorPayload, out actual, out error);

            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual);
            }
            else
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }
        }

        #endregion

        #region Letters Tests

        [TestMethod]
        public void GenerateRandomLettersWithLengthOfTenStaticValueExpectedFiveRandomCharString()
        {
            int length = 10;
            SetupArguments(ActivityStrings.RandomActivityDataListWithData, ActivityStrings.RandomActivityDataListShape, enRandomType.Letters, string.Empty, string.Empty, length.ToString(), "[[OutVar1]]");

            IDSFDataObject result = ExecuteProcess();

            string error = string.Empty;            
            string actual;
            GetScalarValueFromDataList(result.DataListID, "OutVar1", out actual, out error);

            if (string.IsNullOrEmpty(error))
            {
                Assert.IsTrue(actual.Length == length);
            }
            else
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }
        }

        [TestMethod]
        public void GenerateRandomLettersWithLengthOfTenRecordsetWithIndexValueExpectedFiveRandomCharString()
        {           
            SetupArguments(ActivityStrings.RandomActivityDataListWithData, ActivityStrings.RandomActivityDataListShape, enRandomType.Letters, string.Empty, string.Empty, "[[recset1(1).field1]]", "[[OutVar1]]");

            IDSFDataObject result = ExecuteProcess();

            string error = string.Empty;
            string actual;
            GetScalarValueFromDataList(result.DataListID, "OutVar1", out actual, out error);

            if (string.IsNullOrEmpty(error))
            {
                Assert.IsTrue(actual.Length == 10);
            }
            else
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }
        }

        [TestMethod]
        public void GenerateRandomLettersWithLengthRecordsetWithStarIndexValueExpectedFiveRandomCharString()
        {
            SetupArguments(ActivityStrings.RandomActivityDataListWithData, ActivityStrings.RandomActivityDataListShape, enRandomType.Letters, string.Empty, string.Empty, "[[recset1(*).field1]]", "[[recset2(*).field2]]");

            IDSFDataObject result = ExecuteProcess();

            string error = string.Empty;
            IList<IBinaryDataListItem> dataListItems;
            GetRecordSetFieldValueFromDataList(result.DataListID, "recset2", "field2", out dataListItems, out error);
            
            if (string.IsNullOrEmpty(error))
            {
                Assert.IsTrue(dataListItems.Count == 5);           
                Assert.AreEqual(dataListItems[0].TheValue.Length, 10);
                Assert.AreEqual(dataListItems[1].TheValue.Length, 20);
                Assert.AreEqual(dataListItems[2].TheValue.Length, 30);
                Assert.AreEqual(dataListItems[3].TheValue.Length, 40);
                Assert.AreEqual(dataListItems[4].TheValue.Length, 50);                           
            }
            else
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }
        }

        [TestMethod]
        public void GenerateRandomLettersWithLengthOfNegativeTenExpectedError()
        {
            SetupArguments(ActivityStrings.RandomActivityDataListWithData, ActivityStrings.RandomActivityDataListShape, enRandomType.Letters, string.Empty, string.Empty, "[[recset2(1).field2]]", "[[OutVar1]]");

            IDSFDataObject result = ExecuteProcess();

            string error = string.Empty;
            string expected = "<InnerError>Please enter a positive integer for the Length.</InnerError>";
            string actual;
            GetScalarValueFromDataList(result.DataListID, GlobalConstants.ErrorPayload, out actual, out error);

            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual);
            }
            else
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }
        }

        [TestMethod]
        public void GenerateRandomLettersWithLengthOfLettersExpectedError()
        {
            SetupArguments(ActivityStrings.RandomActivityDataListWithData, ActivityStrings.RandomActivityDataListShape, enRandomType.Letters, string.Empty, string.Empty, "letters", "[[OutVar1]]");

            IDSFDataObject result = ExecuteProcess();

            string error = string.Empty;
            string expected = "<InnerError>Please ensure that the Length is an integer value.</InnerError>";
            string actual;
            GetScalarValueFromDataList(result.DataListID, GlobalConstants.ErrorPayload, out actual, out error);

            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual);
            }
            else
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }
        }

        [TestMethod]
        public void GenerateRandomLettersWithBlankLengthExpectedError()
        {
            SetupArguments(ActivityStrings.RandomActivityDataListWithData, ActivityStrings.RandomActivityDataListShape, enRandomType.Letters, string.Empty, string.Empty, "", "[[OutVar1]]");

            IDSFDataObject result = ExecuteProcess();

            string error = string.Empty;
            string expected = "<InnerError>Please ensure that you have entered an integer for Length.</InnerError>";
            string actual;
            GetScalarValueFromDataList(result.DataListID, GlobalConstants.ErrorPayload, out actual, out error);

            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual);
            }
            else
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }
        }

        #endregion

        #region GetDebugInputs/Outputs
        
        [TestMethod]        
        public void RandomGetDebugInputOutputWithRecordsetUsingStartNotationExpectedPass()        
        {
            DsfRandomActivity act = new DsfRandomActivity {RandomType = enRandomType.Numbers,From = "[[recset2(*).field2]]",To = "[[recset1(*).field1]]",Result = "[[OutVar1]]"};

            IList<IDebugItem> inRes;
            IList<IDebugItem> outRes;

            CheckActivityDebugInputOutput(act, ActivityStrings.RandomActivityDataListShape,
                                                                ActivityStrings.RandomActivityDataListWithData, out inRes, out outRes);

            Assert.AreEqual(2, inRes.Count);
            Assert.AreEqual(2, inRes[0].FetchResultsList().Count);
            Assert.AreEqual(32, inRes[1].FetchResultsList().Count);
            

            Assert.AreEqual(5, outRes.Count);
            Assert.AreEqual(3, outRes[0].FetchResultsList().Count);
            Assert.AreEqual(3, outRes[1].FetchResultsList().Count);
            Assert.AreEqual(3, outRes[2].FetchResultsList().Count);
            Assert.AreEqual(3, outRes[3].FetchResultsList().Count);
            Assert.AreEqual(3, outRes[4].FetchResultsList().Count);
        }

        [TestMethod]
        public void RandomGetDebugInputOutputWithRecordsetUsingNumericNotationExpectedPass()
        {
            DsfRandomActivity act = new DsfRandomActivity { RandomType = enRandomType.Letters,Length = "[[recset1(1).field1]]", Result = "[[OutVar1]]" };

            IList<IDebugItem> inRes;
            IList<IDebugItem> outRes;

            CheckActivityDebugInputOutput(act, ActivityStrings.RandomActivityDataListShape,
                                                                ActivityStrings.RandomActivityDataListWithData, out inRes, out outRes);

            Assert.AreEqual(2, inRes.Count);
            Assert.AreEqual(2, inRes[0].FetchResultsList().Count);
            Assert.AreEqual(5, inRes[1].FetchResultsList().Count);


            Assert.AreEqual(1, outRes.Count);
            Assert.AreEqual(3, outRes[0].FetchResultsList().Count);            
        }

        #endregion

        #region Private Test Methods

        private void SetupArguments(string currentDL, string testData, enRandomType randomType, string from, string to, string length, string result)
        {
            TestStartNode = new FlowStep
            {
                Action = new DsfRandomActivity { RandomType = randomType, From = from, To = to, Length = length, Result = result }
            };

            CurrentDl = testData;
            TestData = currentDL;
        }

        #endregion Private Test Methods
    }
}
