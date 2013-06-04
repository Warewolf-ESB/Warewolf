using Dev2;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Dev2.Tests.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Activities.Statements;
using System.Collections.Generic;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace ActivityUnitTests.ActivityTests
{
    /// <summary>
    /// Summary description for CaseConvertActivityTests
    /// </summary>
    [TestClass]
    public class CaseConvertActivityTests : BaseActivityUnitTest
    {
        public CaseConvertActivityTests()
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

        #region AllUpper Tests

        [TestMethod]
        public void CaseConvert_AllUpper_Expected_AllUpperCase()
        {
            IList<ICaseConvertTO> convertCollection = new List<ICaseConvertTO>() { CaseConverterFactory.CreateCaseConverterTO("[[testVar]]", "UPPER", "[[testVar]]", 1) };

            SetupArguments(@"<root><testVar>change this to upper case</testVar></root>", ActivityStrings.CaseConvert_DLShape, convertCollection);

            IDSFDataObject result = ExecuteProcess();
            string actual = string.Empty;
            string error = string.Empty;
            GetScalarValueFromDataList(result.DataListID, "testVar", out actual, out error);
            string myNewResult = actual;
            string expected = @"CHANGE THIS TO UPPER CASE";
            Assert.AreEqual(expected, actual);
        }

        //2013.06.03: Ashley Lewis for bug 9498 - handle multiple regions in case convert result
        [TestMethod]
        public void CaseConvertWithAllUpperAndMultipleRegionsExpectedAllUpperCase()
        {
            IList<ICaseConvertTO> convertCollection = new List<ICaseConvertTO>() { CaseConverterFactory.CreateCaseConverterTO("[[testRecSet().field]], [[testVar]]", "UPPER", "[[testRecSet().field]], [[testVar]]", 1) };

            SetupArguments(ActivityStrings.CaseConvert_MixedRegionTypes_CurrentDL, ActivityStrings.CaseConvert_MixedRegionTypes_DLShape, convertCollection);

            IDSFDataObject result = ExecuteProcess();
            string actualScalar = string.Empty;
            IList<IBinaryDataListItem> actualRecset = new List<IBinaryDataListItem>();
            string error = string.Empty;
            GetScalarValueFromDataList(result.DataListID, "testVar", out actualScalar, out error);
            GetRecordSetFieldValueFromDataList(result.DataListID, "testRecSet", "field", out actualRecset, out error);
            string expected = @"CHANGE THIS TO UPPER CASE";
            Assert.AreEqual(expected, actualScalar);
            Assert.AreEqual(expected, actualRecset[1].TheValue);
        }
        [TestMethod]
        public void CaseConvertWithAllUpperAndMultipleRegionsInStringToConvertToConvertExpectedAllUpperCase()
        {
            IList<ICaseConvertTO> convertCollection = new List<ICaseConvertTO>() { CaseConverterFactory.CreateCaseConverterTO("[[testRecSet().field]], [[testVar]]", "UPPER", "[[testRecSet().field]]", 1) };

            SetupArguments(ActivityStrings.CaseConvert_MixedRegionTypes_CurrentDL, ActivityStrings.CaseConvert_MixedRegionTypes_DLShape, convertCollection);

            IDSFDataObject result = ExecuteProcess();
            string actualScalar = string.Empty;
            IList<IBinaryDataListItem> actualRecset = new List<IBinaryDataListItem>();
            string error = string.Empty;
            GetScalarValueFromDataList(result.DataListID, "testVar", out actualScalar, out error);
            GetRecordSetFieldValueFromDataList(result.DataListID, "testRecSet", "field", out actualRecset, out error);
            string expected = @"CHANGE THIS TO UPPER CASE";
            Assert.AreEqual(expected, actualScalar);
            Assert.AreEqual(expected, actualRecset[1].TheValue);
        }

        #endregion AllUpper Tests

        #region AllLower Tests

        [TestMethod]
        public void CaseConvert_AllLower_Expected_AllLowerCase()
        {
            IList<ICaseConvertTO> convertCollection = new List<ICaseConvertTO>() { CaseConverterFactory.CreateCaseConverterTO("[[testVar]]", "lower", "[[testVar]]", 1) };

            SetupArguments(@"<root><testVar>CHANGE THIS TO LOWER CASE</testVar>  </root>", ActivityStrings.CaseConvert_DLShape, convertCollection);
            IDSFDataObject result = ExecuteProcess();

            string expected = @"change this to lower case";
            string actual = string.Empty;
            string error = string.Empty;
            GetScalarValueFromDataList(result.DataListID, "testVar", out actual, out error);
            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual);
            }
            else
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }

        }

        #endregion AllLower Tests

        #region FirstUpper Tests

        [TestMethod]
        public void CaseConvert_FirstUpper_Expected_FirstLetterIsUpperCase()
        {
            IList<ICaseConvertTO> convertCollection = new List<ICaseConvertTO>() { CaseConverterFactory.CreateCaseConverterTO("[[testVar]]", "Sentence", "[[testVar]]", 1) };

            SetupArguments(@"<root><testVar>change this to first leter upper case</testVar></root>", ActivityStrings.CaseConvert_DLShape, convertCollection);
            IDSFDataObject result = ExecuteProcess();
            string expected = @"Change this to first leter upper case";
            string actual = string.Empty;
            string error = string.Empty;
            GetScalarValueFromDataList(result.DataListID, "testVar", out actual, out error);

            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual);
            }
            else
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }

        }

        #endregion FirstUpper Tests

        #region AllFirstUpper Tests

        [TestMethod]
        public void CaseConvert_AllFirstUpper_Expected_AllFirstLettersIsUpperCase()
        {
            IList<ICaseConvertTO> convertCollection = new List<ICaseConvertTO>() { CaseConverterFactory.CreateCaseConverterTO("[[testVar]]", "Title Case", "[[testVar]]", 1) };

            SetupArguments(@"<root><testVar>change this to first leter upper case</testVar></root>", ActivityStrings.CaseConvert_DLShape, convertCollection);

            IDSFDataObject result = ExecuteProcess();
            string expected = @"Change This To First Leter Upper Case";
            string actual = string.Empty;
            string error = string.Empty;
            GetScalarValueFromDataList(result.DataListID, "testVar", out actual, out error);
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
        public void CaseConvert_No_Result_Variable_Expected_AllFirstLettersIsUpperCase()
        {
            IList<ICaseConvertTO> convertCollection = new List<ICaseConvertTO>() { CaseConverterFactory.CreateCaseConverterTO("[[testVar]]", "Title Case", "", 1) };

            SetupArguments(@"<root><testVar>change this to first leter upper case</testVar></root>", ActivityStrings.CaseConvert_DLShape, convertCollection);

            IDSFDataObject result = ExecuteProcess();
            string expected = @"Change This To First Leter Upper Case";
            string actual = string.Empty;
            string error = string.Empty;
            GetScalarValueFromDataList(result.DataListID, "testVar", out actual, out error);
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
        public void CaseConvert_Numbers_In_StringToConvert_Expected_AllFirstLettersIsUpperCase()
        {
            IList<ICaseConvertTO> convertCollection = new List<ICaseConvertTO>() { CaseConverterFactory.CreateCaseConverterTO("[[testVar]]", "Title Case", "[[testVar]]", 1) };

            SetupArguments(@"<root><testVar>change this 5435123130t lete2435r upper case</testVar></root>", ActivityStrings.CaseConvert_DLShape, convertCollection);

            IDSFDataObject result = ExecuteProcess();

            string expected = @"Change This 5435123130t Lete2435r Upper Case";
            string actual = string.Empty;
            string error = string.Empty;

            GetScalarValueFromDataList(result.DataListID, "testVar", out actual, out error);
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
        public void CaseConvert_Blank_StringToConvert_Expected_AllFirstLettersIsUpperCase()
        {
            IList<ICaseConvertTO> convertCollection = new List<ICaseConvertTO>() { CaseConverterFactory.CreateCaseConverterTO("", "Title Case", "", 1) };

            SetupArguments(@"<root><testVar>change this 5435123130t lete2435r upper case</testVar></root>", ActivityStrings.CaseConvert_DLShape, convertCollection);

            IDSFDataObject result = ExecuteProcess();

            string expected = @"change this 5435123130t lete2435r upper case";
            string actual = string.Empty;
            string error = string.Empty;

            GetScalarValueFromDataList(result.DataListID, "testVar", out actual, out error);
            if (string.IsNullOrEmpty(error))
            {
                Assert.AreEqual(expected, actual);
            }
            else
            {
                Assert.Fail(string.Format("The following errors occured while retrieving datalist items\r\nerrors:{0}", error));
            }
        }

        #endregion AllFirstUpper Tests

        #region Blank  Test
        // Bug 8474 - Travis.Frisinger : Issue was a empty check, not language notation
        public void CaseConvert_BlankValue_Expected_BlankValue()
        {
            IList<ICaseConvertTO> convertCollection = new List<ICaseConvertTO>() { CaseConverterFactory.CreateCaseConverterTO("[[testVar]]", "UPPER", "[[testVar]]", 1) };

            SetupArguments(@"<root><testVar></testVar></root>", ActivityStrings.CaseConvert_DLShape, convertCollection);

            IDSFDataObject result = ExecuteProcess();
            string expected = @"";
            string actual = string.Empty;
            string error = string.Empty;
            GetScalarValueFromDataList(result.DataListID, "testVar", out actual, out error);
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

        #region Error Handling Tests

        [TestMethod]
        public void CaseConvert_ErrorHandeling_Expected_ErrorTag()
        {
            IList<ICaseConvertTO> convertCollection = new List<ICaseConvertTO>() { CaseConverterFactory.CreateCaseConverterTO("[[//().rec]]", "Title Case", "", 1) };
            SetupArguments(@"<root><testVar>change this 5435123130t lete2435r upper case</testVar></root>", ActivityStrings.CaseConvert_DLShape, convertCollection);

            IDSFDataObject result = ExecuteProcess();
            string actual = string.Empty;
            string error = string.Empty;

            GetScalarValueFromDataList(result.DataListID, "testVar", out actual, out error);

            Assert.IsTrue(Compiler.HasErrors(result.DataListID));
        }

        #endregion Error Handling Tests

        #region GetWizardData Tests

        [TestMethod]
        public void GetWizardData_Expected_Correct_IBinaryDataList()
        {
            bool passTest = true;
            IList<ICaseConvertTO> _convertCollection = new List<ICaseConvertTO>() { new CaseConvertTO("[[testStringToConvert]]", "UPPER", "[[result]]", 1), new CaseConvertTO("[[testStringToConvert]]", "lower", "[[result]]", 2) };

            DsfCaseConvertActivity testAct = new DsfCaseConvertActivity { ConvertCollection = _convertCollection };

            IBinaryDataList binaryDL = testAct.GetWizardData();
            var recsets = binaryDL.FetchRecordsetEntries();
            if (recsets.Count != 1)
            {
                passTest = false;
            }
            else
            {
                if (recsets[0].Columns.Count != 3)
                {
                    passTest = false;
                }
            }
            Assert.IsTrue(passTest);
        }

        #endregion GetWizardData Tests

        #region Get Debug Input/Output Tests

        /// <summary>
        /// Author : Massimo Guerrera Bug 8104 
        /// </summary>
        [TestMethod]
        public void CaseConvert_Get_Debug_Input_Output_With_Scalars_Expected_Pass()
        {
            IList<ICaseConvertTO> convertCollection = new List<ICaseConvertTO>() { new CaseConvertTO("[[CompanyName]]", "UPPER", "[[CompanyName]]", 1) };
            DsfCaseConvertActivity act = new DsfCaseConvertActivity { ConvertCollection = convertCollection };

            List<DebugItem> inRes;
            List<DebugItem> outRes;

            CheckActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape,
                                                                ActivityStrings.DebugDataListWithData, out inRes, out outRes);
            Assert.AreEqual(1, inRes.Count);
            Assert.AreEqual(7, inRes[0].FetchResultsList().Count);
            Assert.AreEqual(1, outRes.Count);
            Assert.AreEqual(4, outRes[0].FetchResultsList().Count);
        }

        /// <summary>
        /// Author : Massimo Guerrera Bug 8104 
        /// </summary>
        [TestMethod]
        public void CaseConvert_Get_Debug_Input_Output_With_Recordsets_Expected_Pass()
        {
            IList<ICaseConvertTO> convertCollection = new List<ICaseConvertTO>() { new CaseConvertTO("[[Customers(*).FirstName]]", "UPPER", "[[Customers(*).FirstName]]", 1) };
            DsfCaseConvertActivity act = new DsfCaseConvertActivity { ConvertCollection = convertCollection };

            List<DebugItem> inRes;
            List<DebugItem> outRes;

            CheckActivityDebugInputOutput(act, ActivityStrings.DebugDataListShape,
                                                                ActivityStrings.DebugDataListWithData, out inRes, out outRes);
            Assert.AreEqual(1, inRes.Count);
            Assert.AreEqual(34, inRes[0].FetchResultsList().Count);
            Assert.AreEqual(10, outRes.Count);
            Assert.AreEqual(4, outRes[0].FetchResultsList().Count);
        }

        #endregion

        #region RecordSet Tests

        [TestMethod]
        public void CaseConvert_WithRecordSetDataAndEmptyIndex_Expected_LastRecordCaseConverted()
        {
            IList<ICaseConvertTO> convertCollection = new List<ICaseConvertTO>() { CaseConverterFactory.CreateCaseConverterTO("[[testRecSet(2).testVar]]", "Title Case", "[[testRecSet().testVar]]", 3) };

            SetupArguments(@"<root><testRecSet><testVar>do not change this to first leter upper case</testVar></testRecSet><testRecSet><testVar>change this to first leter upper case</testVar></testRecSet></root>", ActivityStrings.CaseConvert_DLWithRecordSetShape, convertCollection);

            IDSFDataObject result = ExecuteProcess();
            string expected = @"Change This To First Leter Upper Case";
            IList<IBinaryDataListItem> actual = new List<IBinaryDataListItem>();
            string error = string.Empty;
            GetRecordSetFieldValueFromDataList(result.DataListID, "testRecSet", "testVar", out actual, out error);
           
            string actualValue = actual[2].TheValue;
            
            Assert.AreEqual(expected, actualValue);
        }

        [TestMethod]
        public void CaseConvert_WithRecordSetDataAndStar_Expected_AllRecordsConverted()
        {
            IList<ICaseConvertTO> convertCollection = new List<ICaseConvertTO>() { CaseConverterFactory.CreateCaseConverterTO("[[testRecSet(*).testVar]]", "Title Case", "[[testRecSet(*).testVar]]", 3) };

            SetupArguments(@"<root><testRecSet><testVar>do not change this to first leter upper case</testVar></testRecSet><testRecSet><testVar>change this to first leter upper case</testVar></testRecSet></root>", ActivityStrings.CaseConvert_DLWithRecordSetShape, convertCollection);

            IDSFDataObject result = ExecuteProcess();
            string expected = @"Change This To First Leter Upper Case";
            IList<IBinaryDataListItem> actual = new List<IBinaryDataListItem>();
            string error = string.Empty;
            GetRecordSetFieldValueFromDataList(result.DataListID, "testRecSet", "testVar", out actual, out error);

            // This should be an index of 2
            string actualValue = actual[1].TheValue;

            Assert.AreEqual(@"Do Not Change This To First Leter Upper Case", actual[0].TheValue);
            Assert.AreEqual(expected, actualValue);
        }

        // Bug 7912 - Travis.Frisinger 
        [TestMethod]
        public void CaseConvert_WithRecordSetDataWithInvalidIndex_Expected_Error()
        {
            IList<ICaseConvertTO> convertCollection = new List<ICaseConvertTO>() { CaseConverterFactory.CreateCaseConverterTO("[[testRecSet(0+1).testVar]]", "Title Case", "[[testRecSet(0+1).testVar]]", 3) };

            SetupArguments(@"<root><testRecSet><testVar>do not change this to first leter upper case</testVar></testRecSet><testRecSet><testVar>change this to first leter upper case</testVar></testRecSet></root>", ActivityStrings.CaseConvert_DLWithRecordSetShape, convertCollection);

            IDSFDataObject result = ExecuteProcess();
            IList<IBinaryDataListItem> actual = new List<IBinaryDataListItem>();
            string error = string.Empty;

            Assert.IsTrue(Compiler.HasErrors(result.DataListID));

        }

        #endregion RecordSet Tests

        #region Private Test Methods

        private void SetupArguments(string currentDL, string testData, IList<ICaseConvertTO> convertCollection)
        {
            TestStartNode = new FlowStep
            {
                Action = new DsfCaseConvertActivity { ConvertCollection = convertCollection }
            };

            CurrentDl = testData;
            TestData = currentDL;
        }

        #endregion Private Test Methods
    }
}
