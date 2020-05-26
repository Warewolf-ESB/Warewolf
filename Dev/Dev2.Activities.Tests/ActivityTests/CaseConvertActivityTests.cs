/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities.Statements;
using System.Collections.Generic;
using ActivityUnitTests;
using Dev2.Common.Interfaces.Core.Convertors.Case;             
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Dev2.Activities.Factories.Case;
using Moq;
using Dev2.Communication;
using System.Linq;
using Dev2.Common.State;
using Dev2.Utilities;

namespace Dev2.Tests.Activities.ActivityTests
{
    /// <summary>
    /// Summary description for CaseConvertActivityTests
    /// </summary>
    [TestClass]
    
    public class CaseConvertActivityTests : BaseActivityUnitTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region AllUpper Tests

        [TestMethod]
        [Timeout(60000)]
        public void CaseConvert_AllUpper_Expected_AllUpperCase()
        {
            IList<ICaseConvertTO> convertCollection = new List<ICaseConvertTO> { CaseConverterFactory.CreateCaseConverterTO("[[testVar]]", "UPPER", "[[testVar]]", 1) };

            SetupArguments(@"<root><testVar>change this to upper case</testVar></root>", ActivityStrings.CaseConvert_DLShape, convertCollection);

            var result = ExecuteProcess();
            GetScalarValueFromEnvironment(result.Environment, "testVar", out string actual, out string error);
            const string expected = @"CHANGE THIS TO UPPER CASE";

            // remove test datalist ;)

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfCaseConvertActivity_GetOutputs")]
        public void DsfCaseConvertActivity_GetOutputs_Called_ShouldReturnListWithResultValueInIt()
        {
            //------------Setup for test--------------------------
            IList<ICaseConvertTO> convertCollection = new List<ICaseConvertTO> { CaseConverterFactory.CreateCaseConverterTO("[[testVar]]", "UPPER", "[[testVar]]", 1), CaseConverterFactory.CreateCaseConverterTO("[[testVar2]]", "UPPER", "[[testVar2]]", 1) };
            var act = new DsfCaseConvertActivity { ConvertCollection = convertCollection };
            //------------Execute Test---------------------------
            var outputs = act.GetOutputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(2, outputs.Count);
            Assert.AreEqual("[[testVar]]", outputs[0]);
            Assert.AreEqual("[[testVar2]]", outputs[1]);
        }

        [TestMethod]
        [Timeout(60000)]
        public void CaseConvertWithAllUpperAndMultipleRegionsExpectedAllUpperCase()
        {
            IList<ICaseConvertTO> convertCollection = new List<ICaseConvertTO> { CaseConverterFactory.CreateCaseConverterTO("[[testRecSet().field]], [[testVar]]", "UPPER", "[[testRecSet().field]], [[testVar]]", 1) };

            SetupArguments(ActivityStrings.CaseConvert_MixedRegionTypes_CurrentDL, ActivityStrings.CaseConvert_MixedRegionTypes_DLShape, convertCollection);

            var result = ExecuteProcess();
            GetScalarValueFromEnvironment(result.Environment, "testVar", out string actualScalar, out string error);
            GetRecordSetFieldValueFromDataList(result.Environment, "testRecSet", "field", out IList<string> actualRecset, out error);
            const string expected = @"CHANGE THIS TO UPPER CASE";

            // remove test datalist ;)

            Assert.AreEqual(expected, actualScalar);
            Assert.AreEqual(expected, actualRecset[1]);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Travis Frisinger")]
        [TestCategory("CaseConvert_Evaluate")]
        public void CaseConvert_Evaluate_WhenRecursiveRegion_ExpectSingleWellFormedRegionAsResult()
        {
            //------------Setup for test--------------------------
            IList<ICaseConvertTO> convertCollection = new List<ICaseConvertTO> { CaseConverterFactory.CreateCaseConverterTO("[[[[testVar]]]]", "UPPER", "[[[[testVar]]]]", 1) };

            SetupArguments(@"<root><NewVar>change this to upper case</NewVar><testVar>NewVar</testVar></root>", ActivityStrings.CaseConvert_DLShape, convertCollection);

            //------------Execute Test---------------------------
            var result = ExecuteProcess();
            GetScalarValueFromEnvironment(result.Environment, "NewVar", out string actual, out string error);

            //------------Assert Results-------------------------
            const string expected = @"CHANGE THIS TO UPPER CASE";
            Assert.AreEqual(expected, actual);

        }

        [TestMethod]
        [Timeout(60000)]
        public void CaseConvertWithAllUpperAndMultipleRegionsInStringToConvertWithSingleOutputTargetExpectedOneUpperCase()
        {
            IList<ICaseConvertTO> convertCollection = new List<ICaseConvertTO> { CaseConverterFactory.CreateCaseConverterTO("[[testRecSet().field]], [[testVar]]", "UPPER", "[[testRecSet().field]]", 1) };

            SetupArguments(ActivityStrings.CaseConvert_MixedRegionTypes_CurrentDL, ActivityStrings.CaseConvert_MixedRegionTypes_DLShape, convertCollection);

            var result = ExecuteProcess();
            GetScalarValueFromEnvironment(result.Environment, "testVar", out string actualScalar, out string error);
            GetRecordSetFieldValueFromDataList(result.Environment, "testRecSet", "field", out IList<string> actualRecset, out error);

            // remove test datalist ;)

            const string expected = @"CHANGE THIS TO UPPER CASE";
            StringAssert.Contains(actualRecset[1], expected);
            StringAssert.Contains(actualScalar, expected);
        }

        [TestMethod]
        [Timeout(60000)]
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
            var result = ExecuteProcess();
            GetScalarValueFromEnvironment(result.Environment, "testVar", out string actualScalar, out string error);
            GetRecordSetFieldValueFromDataList(result.Environment, "testRecSet", "field", out IList<string> actualRecset, out error);

            // remove test datalist ;)

            //------------Assert Results-------------------------
            const string expected = @"CHANGE THIS TO UPPER CASE";
            Assert.AreEqual(expected, actualRecset[0]);
        }

        [TestMethod]
        [Timeout(60000)]
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
            var result = ExecuteProcess();
            GetScalarValueFromEnvironment(result.Environment, "testVar", out string actualScalar, out string error);
            GetRecordSetFieldValueFromDataList(result.Environment, "testRecSet", "field", out IList<string> actualRecset, out error);

            // remove test datalist ;)

            //------------Assert Results-------------------------
            const string expected = @"CHANGE THIS TO UPPER CASE";
            Assert.AreEqual(expected, actualRecset[0]);
        }

        #endregion AllUpper Tests

        #region AllLower Tests

        [TestMethod]
        [Timeout(60000)]
        public void CaseConvert_AllLower_Expected_AllLowerCase()
        {
            IList<ICaseConvertTO> convertCollection = new List<ICaseConvertTO> { CaseConverterFactory.CreateCaseConverterTO("[[testVar]]", "lower", "[[testVar]]", 1) };

            SetupArguments(@"<root><testVar>CHANGE THIS TO LOWER CASE</testVar>  </root>", ActivityStrings.CaseConvert_DLShape, convertCollection);
            var result = ExecuteProcess();

            const string expected = @"change this to lower case";
            GetScalarValueFromEnvironment(result.Environment, "testVar", out string actual, out string error);

            // remove test datalist ;)

            Assert.AreEqual(expected, actual);

        }

        #endregion AllLower Tests

        #region FirstUpper Tests

        [TestMethod]
        [Timeout(60000)]
        public void CaseConvert_FirstUpper_Expected_FirstLetterIsUpperCase()
        {
            IList<ICaseConvertTO> convertCollection = new List<ICaseConvertTO> { CaseConverterFactory.CreateCaseConverterTO("[[testVar]]", "Sentence", "[[testVar]]", 1) };

            SetupArguments(@"<root><testVar>change this to first leter upper case</testVar></root>", ActivityStrings.CaseConvert_DLShape, convertCollection);
            var result = ExecuteProcess();
            const string expected = @"Change this to first leter upper case";
            GetScalarValueFromEnvironment(result.Environment, "testVar", out string actual, out string error);

            // remove test datalist ;)

            Assert.AreEqual(expected, actual);

        }

        #endregion FirstUpper Tests

        #region AllFirstUpper Tests

        [TestMethod]
        [Timeout(60000)]
        public void CaseConvert_AllFirstUpper_Expected_AllFirstLettersIsUpperCase()
        {
            IList<ICaseConvertTO> convertCollection = new List<ICaseConvertTO> { CaseConverterFactory.CreateCaseConverterTO("[[testVar]]", "Title Case", "[[testVar]]", 1) };

            SetupArguments(@"<root><testVar>change this to first leter upper case</testVar></root>", ActivityStrings.CaseConvert_DLShape, convertCollection);

            var result = ExecuteProcess();
            const string expected = @"Change This To First Leter Upper Case";
            GetScalarValueFromEnvironment(result.Environment, "testVar", out string actual, out string error);

            // remove test datalist ;)

            Assert.AreEqual(expected, actual);

        }

        [TestMethod]
        [Timeout(60000)]
        public void CaseConvert_No_Result_Variable_Expected_AllFirstLettersIsUpperCase()
        {
            IList<ICaseConvertTO> convertCollection = new List<ICaseConvertTO> { CaseConverterFactory.CreateCaseConverterTO("[[testVar]]", "Title Case", "", 1) };

            SetupArguments(@"<root><testVar>change this to first leter upper case</testVar></root>", ActivityStrings.CaseConvert_DLShape, convertCollection);

            var result = ExecuteProcess();
            const string expected = @"Change This To First Leter Upper Case";
            GetScalarValueFromEnvironment(result.Environment, "testVar", out string actual, out string error);

            // remove test datalist ;)

            Assert.AreEqual(expected, actual);

        }

        [TestMethod]
        [Timeout(60000)]
        public void CaseConvert_Numbers_In_StringToConvert_Expected_AllFirstLettersIsUpperCase()
        {
            IList<ICaseConvertTO> convertCollection = new List<ICaseConvertTO> { CaseConverterFactory.CreateCaseConverterTO("[[testVar]]", "Title Case", "[[testVar]]", 1) };

            SetupArguments(@"<root><testVar>change this 5435123130t lete2435r upper case</testVar></root>", ActivityStrings.CaseConvert_DLShape, convertCollection);

            var result = ExecuteProcess();

            const string expected = @"Change This 5435123130T Lete2435r Upper Case";

            GetScalarValueFromEnvironment(result.Environment, "testVar", out string actual, out string error);

            // remove test datalist ;)

            Assert.AreEqual(expected, actual);

        }

        [TestMethod]
        [Timeout(60000)]
        public void CaseConvert_Blank_StringToConvert_Expected_AllFirstLettersIsUpperCase()
        {
            IList<ICaseConvertTO> convertCollection = new List<ICaseConvertTO> { CaseConverterFactory.CreateCaseConverterTO("", "Title Case", "", 1) };

            SetupArguments(@"<root><testVar>change this 5435123130t lete2435r upper case</testVar></root>", ActivityStrings.CaseConvert_DLShape, convertCollection);

            var result = ExecuteProcess();

            const string expected = @"change this 5435123130t lete2435r upper case";

            GetScalarValueFromEnvironment(result.Environment, "testVar", out string actual, out string error);

            // remove test datalist ;)

            Assert.AreEqual(expected, actual);

        }

        #endregion AllFirstUpper Tests

        #region Error Handling Tests


        #endregion Error Handling Tests

        #region ForEach Update/Get Inputs/Outputs

        [TestMethod]
        [Timeout(60000)]
        [Owner("Travis Frisinger")]
        [TestCategory("DsfCaseConvert_UpdateForEachInputs")]
        public void DsfCaseConvert_UpdateForEachInputs_WhenContainsMatchingStarAndOtherData_UpdateSuccessful()
        {
            //------------Setup for test--------------------------
            var fieldsCollection = new List<ICaseConvertTO>
	        {
		        new CaseConvertTO( "[[rs(*).val]] [[result]]","text", "[[result]]",1)
	        };


            var dsfCaseConvert = new DsfCaseConvertActivity { ConvertCollection = fieldsCollection };

            //------------Execute Test---------------------------
            dsfCaseConvert.UpdateForEachInputs(new List<Tuple<string, string>>
                {
		        new Tuple<string, string>("[[rs(*).val]]", "[[rs(1).val]]"),
	        });

            //------------Assert Results-------------------------

            var collection = dsfCaseConvert.ConvertCollection;

            Assert.AreEqual("[[rs(1).val]] [[result]]", collection[0].StringToConvert);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Travis Frisinger")]
        [TestCategory("DsfCaseConvert_UpdateForEachInputs")]
        public void DsfCaseConvert_UpdateForEachInputs_WhenContainsMatchingStar_UpdateSuccessful()
        {
            //------------Setup for test--------------------------
            var fieldsCollection = new List<ICaseConvertTO>
	        {
		        new CaseConvertTO( "[[rs(*).val]]","text", "[[result]]",1)
	        };


            var dsfCaseConvert = new DsfCaseConvertActivity { ConvertCollection = fieldsCollection };

            //------------Execute Test---------------------------
            dsfCaseConvert.UpdateForEachInputs(new List<Tuple<string, string>>
                {
		        new Tuple<string, string>("[[rs(*).val]]", "[[rs(1).val]]"),
	        });

            //------------Assert Results-------------------------

            var collection = dsfCaseConvert.ConvertCollection;

            Assert.AreEqual("[[rs(1).val]]", collection[0].StringToConvert);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Travis Frisinger")]
        [TestCategory("DsfCaseConvert_UpdateForEachOutputs")]
        public void DsfCaseConvert_UpdateForEachOutputs_WhenContainsMatchingStar_UpdateSuccessful()
        {
            //------------Setup for test--------------------------
            var fieldsCollection = new List<ICaseConvertTO>
	        {
		        new CaseConvertTO( "[[result]]","text", "[[rs(*).val]]",1)
	        };

            var dsfCaseConvert = new DsfCaseConvertActivity { ConvertCollection = fieldsCollection };

            //------------Execute Test---------------------------
            dsfCaseConvert.UpdateForEachOutputs(new List<Tuple<string, string>>
                {
		        new Tuple<string, string>("[[rs(*).val]]", "[[rs(1).val]]"),
	        });

            //------------Assert Results-------------------------

            var collection = dsfCaseConvert.ConvertCollection;

            Assert.AreEqual("[[rs(1).val]]", collection[0].Result);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Travis Frisinger")]
        [TestCategory("DsfCaseConvert_GetForEachInputs")]
        public void DsfCaseConvert_GetForEachInputs_Normal_UpdateSuccessful()
        {
            //------------Setup for test--------------------------
            var fieldsCollection = new List<ICaseConvertTO>
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
        [Timeout(60000)]
        [Owner("Travis Frisinger")]
        [TestCategory("DsfCaseConvert_GetForEachOutputs")]
        public void DsfCaseConvert_GetForEachOutputs_Normal_UpdateSuccessful()
        {
            //------------Setup for test--------------------------
            var fieldsCollection = new List<ICaseConvertTO>
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


        [TestMethod]
        [Timeout(60000)]
        public void DsfCaseConvertActivity_GetState_Returns_Inputs_And_Outputs()
        {            
            //------------Setup for test--------------------------
            var fieldsCollection = new List<ICaseConvertTO>
            {
                new CaseConvertTO( "[[result]]","text", "[[rs(*).val]]",1)
            };
            var dsfCaseConvert = new DsfCaseConvertActivity { ConvertCollection = fieldsCollection };
            var expectedResults = ActivityHelper.GetSerializedStateValueFromCollection(fieldsCollection);
            //------------Execute Test---------------------------
            var stateList = dsfCaseConvert.GetState();
            //------------Assert Results-------------------------
            Assert.IsNotNull(stateList);
            Assert.AreEqual(1, stateList.Count());
            Assert.AreEqual(StateVariable.StateType.InputOutput, stateList.ToList()[0].Type);
            Assert.AreEqual("Convert Collection", stateList.ToList()[0].Name);            
            Assert.AreEqual(expectedResults, stateList.ToList()[0].Value);
        }
        #endregion

        #region RecordSet Tests

        [TestMethod]
        [Timeout(60000)]
        public void CaseConvert_WithRecordSetDataAndEmptyIndex_Expected_LastRecordCaseConverted()
        {
            IList<ICaseConvertTO> convertCollection = new List<ICaseConvertTO> { CaseConverterFactory.CreateCaseConverterTO("[[testRecSet(2).testVar]]", "Title Case", "[[testRecSet().testVar]]", 3) };

            SetupArguments(@"<root><testRecSet><testVar>do not change this to first leter upper case</testVar></testRecSet><testRecSet><testVar>change this to first leter upper case</testVar></testRecSet></root>", ActivityStrings.CaseConvert_DLWithRecordSetShape, convertCollection);

            var result = ExecuteProcess();
            const string expected = @"Change This To First Leter Upper Case";
            GetRecordSetFieldValueFromDataList(result.Environment, "testRecSet", "testVar", out IList<string> actual, out string error);


            var actualValue = actual[1];

            // remove test datalist ;)

            Assert.AreEqual(expected, actualValue);
        }

        [TestMethod]
        [Timeout(60000)]
        public void CaseConvert_WithRecordSetDataAndStar_Expected_AllRecordsConverted()
        {
            IList<ICaseConvertTO> convertCollection = new List<ICaseConvertTO> { CaseConverterFactory.CreateCaseConverterTO("[[testRecSet(*).testVar]]", "Title Case", "[", 3) };

            SetupArguments(@"<root><testRecSet><testVar>do not change this to first leter upper case</testVar></testRecSet><testRecSet><testVar>change this to first leter upper case</testVar></testRecSet></root>", ActivityStrings.CaseConvert_DLWithRecordSetShape, convertCollection);

            var result = ExecuteProcess();
            const string expected = @"Change This To First Leter Upper Case";
            GetRecordSetFieldValueFromDataList(result.Environment, "testRecSet", "testVar", out IList<string> actual, out string error);


            // This should be an index of 2
            var actualValue = actual[1];

            // remove test datalist ;)

            Assert.AreEqual(@"Do Not Change This To First Leter Upper Case", actual[0]);
            Assert.AreEqual(expected, actualValue);
        }
        #endregion RecordSet Tests

        #region InsertToCollection
        
        [TestMethod]
        [Timeout(60000)]
        public void AddListToCollectionWhereNotOverwriteExpectInsertToCollection()
        {
            //------------Setup for test--------------------------
            IList<ICaseConvertTO> convertCollection = new List<ICaseConvertTO>
            {
                new CaseConvertTO("String to Convert", "UPPER", "[[testVar]]", 1),
                new CaseConvertTO("String to Convert", "UPPER", "[[testLanguage]]", 2)
            };
            var activity = new DsfCaseConvertActivity();
            activity.ConvertCollection = convertCollection;
            var modelItem = TestModelItemUtil.CreateModelItem(activity);
            //------------Execute Test---------------------------
            activity.AddListToCollection(new[] { "[[Var1]]" }, false, modelItem);
            //------------Assert Results-------------------------
            Assert.AreEqual(4, activity.ConvertCollection.Count);
        }

        [TestMethod]
        [Timeout(60000)]
        public void AddListToCollectionWhereNotOverwriteEmptyListExpectAddToCollection()
        {
            //------------Setup for test--------------------------
            IList<ICaseConvertTO> convertCollection = new List<ICaseConvertTO>()
            {
                new CaseConvertTO("", "UPPER", "[[testVar]]", 1)
            };
            var activity = new DsfCaseConvertActivity();
            activity.ConvertCollection = convertCollection;
            var modelItem = TestModelItemUtil.CreateModelItem(activity);
            //------------Execute Test---------------------------
            activity.AddListToCollection(new[] { "[[Var1]]" }, false, modelItem);
            //------------Assert Results-------------------------
            Assert.AreEqual(2, activity.ConvertCollection.Count);
        }

        [TestMethod]
        [Timeout(60000)]
        public void AddListToCollectionWhereOverwriteExpectAddToCollection()
        {
            //------------Setup for test--------------------------
            IList<ICaseConvertTO> convertCollection = new List<ICaseConvertTO>
            {
                new CaseConvertTO("String to Convert", "UPPER", "[[testVar]]", 1),
                new CaseConvertTO("String to Convert", "UPPER", "[[testLanguage]]", 2)
            };
            var activity = new DsfCaseConvertActivity();
            activity.ConvertCollection = convertCollection;
            var modelItem = TestModelItemUtil.CreateModelItem(activity);
            //------------Execute Test---------------------------
            activity.AddListToCollection(new[] { "[[Var1]]" }, true, modelItem);
            //------------Assert Results-------------------------
            Assert.AreEqual(2, activity.ConvertCollection.Count);
        }

        #endregion

        #region Private Test Methods

        void SetupArguments(string currentDL, string testData, IList<ICaseConvertTO> convertCollection)
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
