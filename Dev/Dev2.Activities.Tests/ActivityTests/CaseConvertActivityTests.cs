using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using ActivityUnitTests;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.ActivityTests
{
    /// <summary>
    /// Summary description for CaseConvertActivityTests
    /// </summary>
    [TestClass]    
    // ReSharper disable InconsistentNaming
    public class CaseConvertActivityTests : BaseActivityUnitTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region AllUpper Tests

        [TestMethod]
        public void CaseConvert_AllUpper_Expected_AllUpperCase()
        {
            IList<ICaseConvertTO> convertCollection = new List<ICaseConvertTO> { CaseConverterFactory.CreateCaseConverterTO("[[testVar]]", "UPPER", "[[testVar]]", 1) };

            SetupArguments(@"<root><testVar>change this to upper case</testVar></root>", ActivityStrings.CaseConvert_DLShape, convertCollection);

            IDSFDataObject result = ExecuteProcess();
            string actual;
            string error;
            GetScalarValueFromDataList(result.DataListID, "testVar", out actual, out error);
            const string expected = @"CHANGE THIS TO UPPER CASE";

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(expected, actual);
        }

        //2013.06.03: Ashley Lewis for bug 9498 - handle multiple regions in case convert result
        [TestMethod]
        public void CaseConvertWithAllUpperAndMultipleRegionsExpectedAllUpperCase()
        {
            IList<ICaseConvertTO> convertCollection = new List<ICaseConvertTO> { CaseConverterFactory.CreateCaseConverterTO("[[testRecSet().field]], [[testVar]]", "UPPER", "[[testRecSet().field]], [[testVar]]", 1) };

            SetupArguments(ActivityStrings.CaseConvert_MixedRegionTypes_CurrentDL, ActivityStrings.CaseConvert_MixedRegionTypes_DLShape, convertCollection);

            IDSFDataObject result = ExecuteProcess();
            string actualScalar;
            IList<IBinaryDataListItem> actualRecset;
            string error;
            GetScalarValueFromDataList(result.DataListID, "testVar", out actualScalar, out error);
            GetRecordSetFieldValueFromDataList(result.DataListID, "testRecSet", "field", out actualRecset, out error);
            const string expected = @"CHANGE THIS TO UPPER CASE";

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(expected, actualScalar);
            Assert.AreEqual(expected, actualRecset[1].TheValue);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("CaseConvert_Evaluate")]
        public void CaseConvert_Evaluate_WhenRecursiveRegion_ExpectSingleWellFormedRegionAsResult()
        {
            //------------Setup for test--------------------------
            IList<ICaseConvertTO> convertCollection = new List<ICaseConvertTO> { CaseConverterFactory.CreateCaseConverterTO("[[[[testVar]]]]", "UPPER", "[[[[testVar]]]]", 1) };

            SetupArguments(@"<root><NewVar>change this to upper case</NewVar><testVar>NewVar</testVar></root>", ActivityStrings.CaseConvert_DLShape, convertCollection);

            //------------Execute Test---------------------------
            IDSFDataObject result = ExecuteProcess();
            string actual;
            string error;
            GetScalarValueFromDataList(result.DataListID, "NewVar", out actual, out error);

            //------------Assert Results-------------------------
            const string expected = @"CHANGE THIS TO UPPER CASE";
            Assert.AreEqual(expected, actual);

        }

        [TestMethod]
        public void CaseConvertWithAllUpperAndMultipleRegionsInStringToConvertWithSingleOutputTargetExpectedOneUpperCase()
        {
            IList<ICaseConvertTO> convertCollection = new List<ICaseConvertTO> { CaseConverterFactory.CreateCaseConverterTO("[[testRecSet().field]], [[testVar]]", "UPPER", "[[testRecSet().field]]", 1) };

            SetupArguments(ActivityStrings.CaseConvert_MixedRegionTypes_CurrentDL, ActivityStrings.CaseConvert_MixedRegionTypes_DLShape, convertCollection);

            IDSFDataObject result = ExecuteProcess();
            string actualScalar;
            IList<IBinaryDataListItem> actualRecset;
            string error;
            GetScalarValueFromDataList(result.DataListID, "testVar", out actualScalar, out error);
            GetRecordSetFieldValueFromDataList(result.DataListID, "testRecSet", "field", out actualRecset, out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            const string expected = @"CHANGE THIS TO UPPER CASE";
            StringAssert.Contains(actualRecset[1].TheValue,expected);
            StringAssert.Contains(actualScalar, "change this to upper case");
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DsfCaseConvert_OnExecute")]
        public void DsfCaseConvert_OnExecute_StarNotation_ReplacesExistingData()
        {
            //------------Setup for test--------------------------

            // 27.08.2013
            // NOTE : The result must remain [ as this is how the fliping studio generates the result when using (*) notation ;)
            // There is a proper bug in to fix this issue, but since the studio is spaghetti I will leave this to the experts ;)
            IList<ICaseConvertTO> convertCollection = new List<ICaseConvertTO> { CaseConverterFactory.CreateCaseConverterTO("[[testRecSet(*).field]]", "UPPER", "[", 1) };

            SetupArguments(ActivityStrings.CaseConvert_MixedRegionTypes_CurrentDL, ActivityStrings.CaseConvert_MixedRegionTypes_DLShape, convertCollection);

            //------------Execute Test---------------------------
            IDSFDataObject result = ExecuteProcess();
            string actualScalar;
            IList<IBinaryDataListItem> actualRecset;
            string error;
            GetScalarValueFromDataList(result.DataListID, "testVar", out actualScalar, out error);
            GetRecordSetFieldValueFromDataList(result.DataListID, "testRecSet", "field", out actualRecset, out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            //------------Assert Results-------------------------
            const string expected = @"CHANGE THIS TO UPPER CASE";
            Assert.AreEqual(expected, actualRecset[0].TheValue);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DsfCaseConvert_OnExecute")]
        public void DsfCaseConvert_OnExecute_StarNotation_NoResultField_ReplacesExistingData()
        {
            //------------Setup for test--------------------------

            // 27.08.2013
            // NOTE : The result must remain [ as this is how the fliping studio generates the result when using (*) notation ;)
            // There is a proper bug in to fix this issue, but since the studio is spaghetti I will leave this to the experts ;)
            IList<ICaseConvertTO> convertCollection = new List<ICaseConvertTO> { CaseConverterFactory.CreateCaseConverterTO("[[testRecSet(*).field]]", "UPPER", "", 1) };

            SetupArguments(ActivityStrings.CaseConvert_MixedRegionTypes_CurrentDL, ActivityStrings.CaseConvert_MixedRegionTypes_DLShape, convertCollection);

            //------------Execute Test---------------------------
            IDSFDataObject result = ExecuteProcess();
            string actualScalar;
            IList<IBinaryDataListItem> actualRecset;
            string error;
            GetScalarValueFromDataList(result.DataListID, "testVar", out actualScalar, out error);
            GetRecordSetFieldValueFromDataList(result.DataListID, "testRecSet", "field", out actualRecset, out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            //------------Assert Results-------------------------
            const string expected = @"CHANGE THIS TO UPPER CASE";
            Assert.AreEqual(expected, actualRecset[0].TheValue);
        }

        #endregion AllUpper Tests

        #region AllLower Tests

        [TestMethod]
        public void CaseConvert_AllLower_Expected_AllLowerCase()
        {
            IList<ICaseConvertTO> convertCollection = new List<ICaseConvertTO> { CaseConverterFactory.CreateCaseConverterTO("[[testVar]]", "lower", "[[testVar]]", 1) };

            SetupArguments(@"<root><testVar>CHANGE THIS TO LOWER CASE</testVar>  </root>", ActivityStrings.CaseConvert_DLShape, convertCollection);
            IDSFDataObject result = ExecuteProcess();

            const string expected = @"change this to lower case";
            string actual;
            string error;
            GetScalarValueFromDataList(result.DataListID, "testVar", out actual, out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(expected, actual);

        }

        #endregion AllLower Tests

        #region FirstUpper Tests

        [TestMethod]
        public void CaseConvert_FirstUpper_Expected_FirstLetterIsUpperCase()
        {
            IList<ICaseConvertTO> convertCollection = new List<ICaseConvertTO> { CaseConverterFactory.CreateCaseConverterTO("[[testVar]]", "Sentence", "[[testVar]]", 1) };

            SetupArguments(@"<root><testVar>change this to first leter upper case</testVar></root>", ActivityStrings.CaseConvert_DLShape, convertCollection);
            IDSFDataObject result = ExecuteProcess();
            const string expected = @"Change this to first leter upper case";
            string actual;
            string error;
            GetScalarValueFromDataList(result.DataListID, "testVar", out actual, out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(expected, actual);

        }

        #endregion FirstUpper Tests

        #region AllFirstUpper Tests

        [TestMethod]
        public void CaseConvert_AllFirstUpper_Expected_AllFirstLettersIsUpperCase()
        {
            IList<ICaseConvertTO> convertCollection = new List<ICaseConvertTO> { CaseConverterFactory.CreateCaseConverterTO("[[testVar]]", "Title Case", "[[testVar]]", 1) };

            SetupArguments(@"<root><testVar>change this to first leter upper case</testVar></root>", ActivityStrings.CaseConvert_DLShape, convertCollection);

            IDSFDataObject result = ExecuteProcess();
            const string expected = @"Change This To First Leter Upper Case";
            string actual;
            string error;
            GetScalarValueFromDataList(result.DataListID, "testVar", out actual, out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(expected, actual);

        }

        [TestMethod]
        public void CaseConvert_No_Result_Variable_Expected_AllFirstLettersIsUpperCase()
        {
            IList<ICaseConvertTO> convertCollection = new List<ICaseConvertTO> { CaseConverterFactory.CreateCaseConverterTO("[[testVar]]", "Title Case", "", 1) };

            SetupArguments(@"<root><testVar>change this to first leter upper case</testVar></root>", ActivityStrings.CaseConvert_DLShape, convertCollection);

            IDSFDataObject result = ExecuteProcess();
            const string expected = @"Change This To First Leter Upper Case";
            string actual;
            string error;
            GetScalarValueFromDataList(result.DataListID, "testVar", out actual, out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(expected, actual);

        }

        [TestMethod]
        public void CaseConvert_Numbers_In_StringToConvert_Expected_AllFirstLettersIsUpperCase()
        {
            IList<ICaseConvertTO> convertCollection = new List<ICaseConvertTO> { CaseConverterFactory.CreateCaseConverterTO("[[testVar]]", "Title Case", "[[testVar]]", 1) };

            SetupArguments(@"<root><testVar>change this 5435123130t lete2435r upper case</testVar></root>", ActivityStrings.CaseConvert_DLShape, convertCollection);

            IDSFDataObject result = ExecuteProcess();

            const string expected = @"Change This 5435123130t Lete2435r Upper Case";
            string actual;
            string error;

            GetScalarValueFromDataList(result.DataListID, "testVar", out actual, out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(expected, actual);

        }

        [TestMethod]
        public void CaseConvert_Blank_StringToConvert_Expected_AllFirstLettersIsUpperCase()
        {
            IList<ICaseConvertTO> convertCollection = new List<ICaseConvertTO> { CaseConverterFactory.CreateCaseConverterTO("", "Title Case", "", 1) };

            SetupArguments(@"<root><testVar>change this 5435123130t lete2435r upper case</testVar></root>", ActivityStrings.CaseConvert_DLShape, convertCollection);

            IDSFDataObject result = ExecuteProcess();

            const string expected = @"change this 5435123130t lete2435r upper case";
            string actual;
            string error;

            GetScalarValueFromDataList(result.DataListID, "testVar", out actual, out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(expected, actual);

        }

        #endregion AllFirstUpper Tests

        #region Blank  Test
        // Bug 8474 - Travis.Frisinger : Issue was a empty check, not language notation
        public void CaseConvert_BlankValue_Expected_BlankValue()
        {
            IList<ICaseConvertTO> convertCollection = new List<ICaseConvertTO> { CaseConverterFactory.CreateCaseConverterTO("[[testVar]]", "UPPER", "[[testVar]]", 1) };

            SetupArguments(@"<root><testVar></testVar></root>", ActivityStrings.CaseConvert_DLShape, convertCollection);

            IDSFDataObject result = ExecuteProcess();
            const string expected = @"";
            string actual;
            string error;
            GetScalarValueFromDataList(result.DataListID, "testVar", out actual, out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(expected, actual);

        }

        #endregion

        #region Error Handling Tests

        [TestMethod]
        public void CaseConvert_ErrorHandeling_Expected_ErrorTag()
        {
            IList<ICaseConvertTO> convertCollection = new List<ICaseConvertTO> { CaseConverterFactory.CreateCaseConverterTO("[[//().rec]]", "Title Case", "", 1) };
            SetupArguments(@"<root><testVar>change this 5435123130t lete2435r upper case</testVar></root>", ActivityStrings.CaseConvert_DLShape, convertCollection);

            IDSFDataObject result = ExecuteProcess();
            string actual;
            string error;

            GetScalarValueFromDataList(result.DataListID, "testVar", out actual, out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.IsTrue(Compiler.HasErrors(result.DataListID));
        }

        #endregion Error Handling Tests

        #region ForEach Update/Get Inputs/Outputs

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DsfCaseConvert_UpdateForEachInputs")]
        public void DsfCaseConvert_UpdateForEachInputs_WhenContainsMatchingStarAndOtherData_UpdateSuccessful()
        {
            //------------Setup for test--------------------------
            List<ICaseConvertTO> fieldsCollection = new List<ICaseConvertTO>
	        {
		        new CaseConvertTO( "[[rs(*).val]] [[result]]","text", "[[result]]",1)
	        };


            var dsfCaseConvert = new DsfCaseConvertActivity { ConvertCollection = fieldsCollection };

            //------------Execute Test---------------------------
            dsfCaseConvert.UpdateForEachInputs(new List<Tuple<string, string>>
                {
		        new Tuple<string, string>("[[rs(*).val]]", "[[rs(1).val]]"),
	        }, null);

            //------------Assert Results-------------------------

            var collection = dsfCaseConvert.ConvertCollection;

            Assert.AreEqual("[[rs(1).val]] [[result]]", collection[0].StringToConvert);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DsfCaseConvert_UpdateForEachInputs")]
        public void DsfCaseConvert_UpdateForEachInputs_WhenContainsMatchingStar_UpdateSuccessful()
        {
            //------------Setup for test--------------------------
            List<ICaseConvertTO> fieldsCollection = new List<ICaseConvertTO>
	        {
		        new CaseConvertTO( "[[rs(*).val]]","text", "[[result]]",1)
	        };


            var dsfCaseConvert = new DsfCaseConvertActivity { ConvertCollection = fieldsCollection };

            //------------Execute Test---------------------------
            dsfCaseConvert.UpdateForEachInputs(new List<Tuple<string, string>>
                {
		        new Tuple<string, string>("[[rs(*).val]]", "[[rs(1).val]]"),
	        }, null);

            //------------Assert Results-------------------------

            var collection = dsfCaseConvert.ConvertCollection;

            Assert.AreEqual("[[rs(1).val]]", collection[0].StringToConvert);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DsfCaseConvert_UpdateForEachOutputs")]
        public void DsfCaseConvert_UpdateForEachOutputs_WhenContainsMatchingStar_UpdateSuccessful()
        {
            //------------Setup for test--------------------------
            List<ICaseConvertTO> fieldsCollection = new List<ICaseConvertTO>
	        {
		        new CaseConvertTO( "[[result]]","text", "[[rs(*).val]]",1)
	        };

            var dsfCaseConvert = new DsfCaseConvertActivity { ConvertCollection = fieldsCollection };

            //------------Execute Test---------------------------
            dsfCaseConvert.UpdateForEachOutputs(new List<Tuple<string, string>>
                {
		        new Tuple<string, string>("[[rs(*).val]]", "[[rs(1).val]]"),
	        }, null);

            //------------Assert Results-------------------------

            var collection = dsfCaseConvert.ConvertCollection;

            Assert.AreEqual("[[rs(1).val]]", collection[0].Result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DsfCaseConvert_GetForEachInputs")]
        public void DsfCaseConvert_GetForEachInputs_Normal_UpdateSuccessful()
        {
            //------------Setup for test--------------------------
            List<ICaseConvertTO> fieldsCollection = new List<ICaseConvertTO>
	        {
		        new CaseConvertTO( "[[rs(*).val]]","text", "[[result]]",1)
	        };

            var dsfCaseConvert = new DsfCaseConvertActivity { ConvertCollection = fieldsCollection };

            //------------Execute Test---------------------------
            var inputs = dsfCaseConvert.GetForEachInputs();

            //------------Assert Results-------------------------

            Assert.AreEqual("[[rs(*).val]]", inputs[0].Name);
            Assert.AreEqual("[[result]]", inputs[0].Value);
        }


        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DsfCaseConvert_GetForEachOutputs")]
        public void DsfCaseConvert_GetForEachOutputs_Normal_UpdateSuccessful()
        {
            //------------Setup for test--------------------------
            List<ICaseConvertTO> fieldsCollection = new List<ICaseConvertTO>
	        {
		        new CaseConvertTO( "[[result]]","text", "[[rs(*).val]]",1)
	        };

            var dsfCaseConvert = new DsfCaseConvertActivity { ConvertCollection = fieldsCollection };

            //------------Execute Test---------------------------
            var inputs = dsfCaseConvert.GetForEachOutputs();

            //------------Assert Results-------------------------

            Assert.AreEqual("[[result]]", inputs[0].Value);
            Assert.AreEqual("[[rs(*).val]]", inputs[0].Name);
        }

        #endregion

        #region RecordSet Tests

        [TestMethod]
        public void CaseConvert_WithRecordSetDataAndEmptyIndex_Expected_LastRecordCaseConverted()
        {
            IList<ICaseConvertTO> convertCollection = new List<ICaseConvertTO> { CaseConverterFactory.CreateCaseConverterTO("[[testRecSet(2).testVar]]", "Title Case", "[[testRecSet().testVar]]", 3) };

            SetupArguments(@"<root><testRecSet><testVar>do not change this to first leter upper case</testVar></testRecSet><testRecSet><testVar>change this to first leter upper case</testVar></testRecSet></root>", ActivityStrings.CaseConvert_DLWithRecordSetShape, convertCollection);

            IDSFDataObject result = ExecuteProcess();
            const string expected = @"Change This To First Leter Upper Case";
            IList<IBinaryDataListItem> actual;
            string error;
            GetRecordSetFieldValueFromDataList(result.DataListID, "testRecSet", "testVar", out actual, out error);


            string actualValue = actual[2].TheValue;

            // remove test datalist ;)
            DataListRemoval(result.DataListID);


            Assert.AreEqual(expected, actualValue);
        }

        [TestMethod]
        public void CaseConvert_WithRecordSetDataAndStar_Expected_AllRecordsConverted()
        {
            IList<ICaseConvertTO> convertCollection = new List<ICaseConvertTO> { CaseConverterFactory.CreateCaseConverterTO("[[testRecSet(*).testVar]]", "Title Case", "[", 3) };

            SetupArguments(@"<root><testRecSet><testVar>do not change this to first leter upper case</testVar></testRecSet><testRecSet><testVar>change this to first leter upper case</testVar></testRecSet></root>", ActivityStrings.CaseConvert_DLWithRecordSetShape, convertCollection);

            IDSFDataObject result = ExecuteProcess();
            const string expected = @"Change This To First Leter Upper Case";
            IList<IBinaryDataListItem> actual;
            string error;
            GetRecordSetFieldValueFromDataList(result.DataListID, "testRecSet", "testVar", out actual, out error);


            // This should be an index of 2
            string actualValue = actual[1].TheValue;

            // remove test datalist ;)
            DataListRemoval(result.DataListID);


            Assert.AreEqual(@"Do Not Change This To First Leter Upper Case", actual[0].TheValue);
            Assert.AreEqual(expected, actualValue);
        }

        // Bug 7912 - Travis.Frisinger 
        [TestMethod]
        public void CaseConvert_WithRecordSetDataWithInvalidIndex_Expected_Error()
        {
            IList<ICaseConvertTO> convertCollection = new List<ICaseConvertTO> { CaseConverterFactory.CreateCaseConverterTO("[[testRecSet(0+1).testVar]]", "Title Case", "[[testRecSet(0+1).testVar]]", 3) };

            SetupArguments(@"<root><testRecSet><testVar>do not change this to first leter upper case</testVar></testRecSet><testRecSet><testVar>change this to first leter upper case</testVar></testRecSet></root>", ActivityStrings.CaseConvert_DLWithRecordSetShape, convertCollection);

            IDSFDataObject result = ExecuteProcess();

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

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
