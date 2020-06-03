#pragma warning disable
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
using System.Linq;
using ActivityUnitTests;
using Dev2.Common.State;
using Dev2.Communication;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.ActivityTests
{
    [TestClass]    
    public class BaseConvertActivityTests : BaseActivityUnitTest
    {
        #region Base 64 Tests

        [TestMethod]
        [Timeout(60000)]
        public void BaseConvert_Base64_Expected_Base64()
        {
            IList<BaseConvertTO> convertCollection = new List<BaseConvertTO> { new BaseConvertTO("[[testVar]]", "Text", "Base 64", "[[testVar]]", 1) };

            SetupArguments(@"<root><testVar>change this base</testVar></root>", ActivityStrings.BaseConvert_DLShape, convertCollection);

            var result = ExecuteProcess();
            GetScalarValueFromEnvironment(result.Environment, "testVar", out string actual, out string error);
            const string expected = @"Y2hhbmdlIHRoaXMgYmFzZQ==";

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [Timeout(60000)]
        public void DsfBaseConvertActivity_GetOutputs_Called_ShouldReturnListWithResultValueInIt()
        {
            //------------Setup for test--------------------------
            IList<BaseConvertTO> convertCollection = new List<BaseConvertTO> { new BaseConvertTO("[[testVar]]", "Text", "Base 64", "[[testVar]]", 1), new BaseConvertTO("[[testVar2]]", "Text", "Base 64", "[[testVar2]]", 1) };
            var act = new DsfBaseConvertActivity { ConvertCollection = convertCollection };
            //------------Execute Test---------------------------
            var outputs = act.GetOutputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(2, outputs.Count);
            Assert.AreEqual("[[testVar]]", outputs[0]);
            Assert.AreEqual("[[testVar2]]", outputs[1]);
        }

        [TestMethod]
        [Timeout(60000)]
        public void BaseConvertWithBase64AndMultipleRegionsExpectedBase64()
        {
            IList<BaseConvertTO> convertCollection = new List<BaseConvertTO> { new BaseConvertTO("[[testRecSet().field]], [[testVar]]", "Text", "Base 64", "[[testRecSet().field]], [[testVar]]", 1) };

            SetupArguments(ActivityStrings.BaseConvert_MixedRegionTypes_CurrentDL, ActivityStrings.BaseConvert_MixedRegionTypes_DLShape, convertCollection);

            var result = ExecuteProcess();
            GetScalarValueFromEnvironment(result.Environment, "testVar", out string actualScalar, out string error);
            GetRecordSetFieldValueFromDataList(result.Environment, "testRecSet", "field", out IList<string> actualRecset, out error);
            const string expected = @"change this base";

            Assert.AreEqual(expected, actualScalar);
            Assert.AreEqual(expected, actualRecset[0]);
        }

        [TestMethod]
        [Timeout(60000)]
        public void BaseConvert_Evaluate_WhenRecursiveRegion_ExpectSingleWellFormedRegionAsResult()
        {
            //------------Setup for test--------------------------
            IList<BaseConvertTO> convertCollection = new List<BaseConvertTO> { new BaseConvertTO("[[[[testVar]]]]", "Text", "Base 64", "[[[[testVar]]]]", 1) };

            SetupArguments(@"<root><NewVar>change this base</NewVar><testVar>NewVar</testVar></root>", ActivityStrings.BaseConvert_DLShape, convertCollection);

            //------------Execute Test---------------------------
            var result = ExecuteProcess();
            GetScalarValueFromEnvironment(result.Environment, "NewVar", out string actual, out string error);

            //------------Assert Results-------------------------
            const string expected = @"Y2hhbmdlIHRoaXMgYmFzZQ==";
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [Timeout(60000)]
        public void BaseConvertWithBase64AndMultipleRegionsInFromExpressionWithSingleOutputTargetExpectedOneBase64()
        {
            IList<BaseConvertTO> convertCollection = new List<BaseConvertTO> { new BaseConvertTO("[[testRecSet().field]], [[testVar]]", "Text", "Base 64", "[[testRecSet().field]]", 1) };

            SetupArguments(ActivityStrings.BaseConvert_MixedRegionTypes_CurrentDL, ActivityStrings.BaseConvert_MixedRegionTypes_DLShape, convertCollection);

            var result = ExecuteProcess();
            GetScalarValueFromEnvironment(result.Environment, "testVar", out string actualScalar, out string error);
            GetRecordSetFieldValueFromDataList(result.Environment, "testRecSet", "field", out IList<string> actualRecset, out error);

            const string expected = @"change this base";
            StringAssert.Contains(actualRecset[0], expected);
            StringAssert.Contains(actualScalar, expected);
        }

        [TestMethod]
        [Timeout(60000)]
        public void DsfBaseConvert_OnExecute_StarNotation_ReplacesExistingData()
        {
            //------------Setup for test--------------------------
            IList<BaseConvertTO> convertCollection = new List<BaseConvertTO> { new BaseConvertTO("[[testRecSet(*).field]]", "Text", "Base 64", "[", 1) };

            SetupArguments(ActivityStrings.BaseConvert_MixedRegionTypes_CurrentDL, ActivityStrings.BaseConvert_MixedRegionTypes_DLShape, convertCollection);

            //------------Execute Test---------------------------
            var result = ExecuteProcess();
            GetScalarValueFromEnvironment(result.Environment, "testVar", out string actualScalar, out string error);
            GetRecordSetFieldValueFromDataList(result.Environment, "testRecSet", "field", out IList<string> actualRecset, out error);
            
            //------------Assert Results-------------------------
            const string expected = @"Y2hhbmdlIHRoaXMgYmFzZQ==";
            Assert.AreEqual(expected, actualRecset[0]);
        }

        [TestMethod]
        [Timeout(60000)]
        public void DsfBaseConvert_OnExecute_StarNotation_NoResultField_ReplacesExistingData()
        {
            //------------Setup for test--------------------------
            IList<BaseConvertTO> convertCollection = new List<BaseConvertTO> { new BaseConvertTO("[[testRecSet(*).field]]", "Text", "Base 64", "", 1) };

            SetupArguments(ActivityStrings.BaseConvert_MixedRegionTypes_CurrentDL, ActivityStrings.BaseConvert_MixedRegionTypes_DLShape, convertCollection);

            //------------Execute Test---------------------------
            var result = ExecuteProcess();
            GetScalarValueFromEnvironment(result.Environment, "testVar", out string actualScalar, out string error);
            GetRecordSetFieldValueFromDataList(result.Environment, "testRecSet", "field", out IList<string> actualRecset, out error);
            
            //------------Assert Results-------------------------
            const string expected = @"Y2hhbmdlIHRoaXMgYmFzZQ==";
            Assert.AreEqual(expected, actualRecset[0]);
        }

        #endregion Base 64 Tests

        #region Binary Tests

        [TestMethod]
        [Timeout(60000)]
        public void BaseConvert_Binary_Expected_BinaryBase()
        {
            IList<BaseConvertTO> convertCollection = new List<BaseConvertTO> { new BaseConvertTO("[[testVar]]", "Text", "Binary", "[[testVar]]", 1) };

            SetupArguments(@"<root><testVar>CHANGED THIS TO BINARY BASE</testVar>  </root>", ActivityStrings.BaseConvert_DLShape, convertCollection);
            var result = ExecuteProcess();

            const string expected = @"010000110100100001000001010011100100011101000101010001000010000001010100010010000100100101010011001000000101010001001111001000000100001001001001010011100100000101010010010110010010000001000010010000010101001101000101";
            GetScalarValueFromEnvironment(result.Environment, "testVar", out string actual, out string error);
            
            Assert.AreEqual(expected, actual);
        }

        #endregion Binary Tests

        #region Hex Tests

        [TestMethod]
        [Timeout(60000)]
        public void BaseConvert_Hex_Expected_HexBase()
        {
            IList<BaseConvertTO> convertCollection = new List<BaseConvertTO> { new BaseConvertTO("[[testVar]]", "Text", "Hex", "[[testVar]]", 1) };

            SetupArguments(@"<root><testVar>change this base</testVar></root>", ActivityStrings.BaseConvert_DLShape, convertCollection);
            var result = ExecuteProcess();
            const string expected = @"0x6368616e676520746869732062617365";
            GetScalarValueFromEnvironment(result.Environment, "testVar", out string actual, out string error);
            
            Assert.AreEqual(expected, actual);

        }

        [TestMethod]
        [Timeout(60000)]
        public void BaseConvert_No_Result_Variable_Expected_HexBase()
        {
            IList<BaseConvertTO> convertCollection = new List<BaseConvertTO> { new BaseConvertTO("[[testVar]]", "Text", "Hex", "", 1) };

            SetupArguments(@"<root><testVar>change this base</testVar></root>", ActivityStrings.BaseConvert_DLShape, convertCollection);

            var result = ExecuteProcess();
            const string expected = @"0x6368616e676520746869732062617365";
            GetScalarValueFromEnvironment(result.Environment, "testVar", out string actual, out string error);
            
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [Timeout(60000)]
        public void BaseConvert_Numbers_In_FromExpression_Expected_HexBase()
        {
            IList<BaseConvertTO> convertCollection = new List<BaseConvertTO> { new BaseConvertTO("[[testVar]]", "Text", "Hex", "[[testVar]]", 1) };

            SetupArguments(@"<root><testVar>change this 5435123130t lete2435r base</testVar></root>", ActivityStrings.BaseConvert_DLShape, convertCollection);

            var result = ExecuteProcess();

            const string expected = @"0x6368616e67652074686973203534333531323331333074206c65746532343335722062617365";

            GetScalarValueFromEnvironment(result.Environment, "testVar", out string actual, out string error);

            Assert.AreEqual(expected, actual);

        }

        [TestMethod]
        [Timeout(60000)]
        public void BaseConvert_Blank_FromExpression_Expected_HexBase()
        {
            IList<BaseConvertTO> convertCollection = new List<BaseConvertTO> { new BaseConvertTO("", "Text", "Hex", "", 1) };

            SetupArguments(@"<root><testVar>change this 5435123130t lete2435r base</testVar></root>", ActivityStrings.BaseConvert_DLShape, convertCollection);

            var result = ExecuteProcess();

            const string expected = @"change this 5435123130t lete2435r base";

            GetScalarValueFromEnvironment(result.Environment, "testVar", out string actual, out string error);

            Assert.AreEqual(expected, actual);

        }

        #endregion Hex Tests

        #region ForEach Update/Get Inputs/Outputs

        [TestMethod]
        [Timeout(60000)]
        public void DsfBaseConvert_UpdateForEachInputs_WhenContainsMatchingStarAndOtherData_UpdateSuccessful()
        {
            //------------Setup for test--------------------------
            var fieldsCollection = new List<BaseConvertTO>
	        {
		        new BaseConvertTO( "[[rs(*).val]] [[result]]", "Text", "Base 64", "[[result]]", 1)
	        };

            var dsfBaseConvert = new DsfBaseConvertActivity { ConvertCollection = fieldsCollection };

            //------------Execute Test---------------------------
            dsfBaseConvert.UpdateForEachInputs(new List<Tuple<string, string>>
            {
		        new Tuple<string, string>("[[rs(*).val]]", "[[rs(1).val]]"),
	        });

            //------------Assert Results-------------------------
            var collection = dsfBaseConvert.ConvertCollection;

            Assert.AreEqual("[[rs(1).val]] [[result]]", collection[0].FromExpression);
        }

        [TestMethod]
        [Timeout(60000)]
        public void DsfBaseConvert_UpdateForEachInputs_WhenContainsMatchingStar_UpdateSuccessful()
        {
            //------------Setup for test--------------------------
            var fieldsCollection = new List<BaseConvertTO>
	        {
		        new BaseConvertTO( "[[rs(*).val]]", "Text", "Base 64", "[[result]]", 1)
	        };

            var dsfBaseConvert = new DsfBaseConvertActivity { ConvertCollection = fieldsCollection };

            //------------Execute Test---------------------------
            dsfBaseConvert.UpdateForEachInputs(new List<Tuple<string, string>>
            {
		        new Tuple<string, string>("[[rs(*).val]]", "[[rs(1).val]]"),
	        });

            //------------Assert Results-------------------------
            var collection = dsfBaseConvert.ConvertCollection;

            Assert.AreEqual("[[rs(1).val]]", collection[0].FromExpression);
        }

        [TestMethod]
        [Timeout(60000)]
        public void DsfBaseConvert_UpdateForEachOutputs_WhenContainsMatchingStar_UpdateSuccessful()
        {
            //------------Setup for test--------------------------
            var fieldsCollection = new List<BaseConvertTO>
	        {
		        new BaseConvertTO( "[[result]]", "Text", "Base 64", "[[rs(*).val]]", 1)
	        };

            var dsfBaseConvert = new DsfBaseConvertActivity { ConvertCollection = fieldsCollection };

            //------------Execute Test---------------------------
            dsfBaseConvert.UpdateForEachOutputs(new List<Tuple<string, string>>
            {
		        new Tuple<string, string>("[[rs(*).val]]", "[[rs(1).val]]"),
	        });

            //------------Assert Results-------------------------
            var collection = dsfBaseConvert.ConvertCollection;

            Assert.AreEqual("[[rs(*).val]]", collection[0].ToExpression);
        }

        [TestMethod]
        [Timeout(60000)]
        public void DsfBaseConvert_GetForEachInputs_Normal_UpdateSuccessful()
        {
            //------------Setup for test--------------------------
            var fieldsCollection = new List<BaseConvertTO>
	        {
		        new BaseConvertTO( "[[rs(*).val]]", "Text", "Base 64", "[[result]]", 1)
	        };

            var dsfBaseConvert = new DsfBaseConvertActivity { ConvertCollection = fieldsCollection };

            //------------Execute Test---------------------------
            var inputs = dsfBaseConvert.GetForEachInputs();

            //------------Assert Results-------------------------
            Assert.AreEqual("[[rs(*).val]]", inputs[0].Name);
            Assert.AreEqual("[[rs(*).val]]", inputs[0].Value);
        }


        [TestMethod]
        [Timeout(60000)]
        public void DsfBaseConvert_GetForEachOutputs_Normal_UpdateSuccessful()
        {
            //------------Setup for test--------------------------
            var fieldsCollection = new List<BaseConvertTO>
	        {
		        new BaseConvertTO( "[[result]]", "Text", "Base 64", "[[rs(*).val]]", 1)
	        };

            var dsfBaseConvert = new DsfBaseConvertActivity { ConvertCollection = fieldsCollection };

            //------------Execute Test---------------------------
            var inputs = dsfBaseConvert.GetForEachOutputs();

            //------------Assert Results-------------------------
            Assert.AreEqual("[[result]]", inputs[0].Value);
            Assert.AreEqual("[[result]]", inputs[0].Name);
        }

        [TestMethod]
        [Timeout(60000)]
        public void DsfBaseConvertActivity_GetState_Returns_Inputs_And_Outputs()
        {
            //------------Setup for test--------------------------
            IList<BaseConvertTO> convertCollection = new List<BaseConvertTO> { new BaseConvertTO("[[testVar]]", "Text", "Base 64", "[[testVar]]", 1), new BaseConvertTO("[[testVar2]]", "Text", "Base 64", "[[testVar2]]", 1) };
            var act = new DsfBaseConvertActivity { ConvertCollection = convertCollection };
            //------------Execute Test---------------------------
            var stateList = act.GetState();
            //------------Assert Results-------------------------
            Assert.IsNotNull(stateList);
            Assert.AreEqual(1, stateList.Count());
            Assert.AreEqual(StateVariable.StateType.InputOutput, stateList.ToList()[0].Type);
            Assert.AreEqual("Convert Collection", stateList.ToList()[0].Name);
            var dev2JsonSerializer = new Dev2JsonSerializer();
            var expectedResults = dev2JsonSerializer.Serialize(convertCollection);
            Assert.AreEqual(expectedResults, stateList.ToList()[0].Value);
        }
        #endregion

        #region RecordSet Tests

        [TestMethod]
        [Timeout(60000)]
        public void BaseConvert_WithRecordSetDataAndEmptyIndex_Expected_LastRecordBaseConverted()
        {
            IList<BaseConvertTO> convertCollection = new List<BaseConvertTO> { new BaseConvertTO("[[testRecSet(2).testVar]]", "Text", "Hex", "[[testRecSet().testVar]]", 3) };

            SetupArguments(@"<root><testRecSet><testVar>do not change this base</testVar></testRecSet><testRecSet><testVar>change this base</testVar></testRecSet></root>", ActivityStrings.BaseConvert_DLWithRecordSetShape, convertCollection);

            var result = ExecuteProcess();
            const string expected = @"0x6368616e676520746869732062617365";
            GetRecordSetFieldValueFromDataList(result.Environment, "testRecSet", "testVar", out IList<string> actual, out string error);


            var actualValue = actual[1];

            Assert.AreEqual(expected, actualValue);
        }

        [TestMethod]
        [Timeout(60000)]
        public void BaseConvert_WithRecordSetDataAndStar_Expected_AllRecordsConverted()
        {
            IList<BaseConvertTO> convertCollection = new List<BaseConvertTO> { new BaseConvertTO("[[testRecSet(*).testVar]]", "Text", "Hex", "[", 3) };

            SetupArguments(@"<root><testRecSet><testVar>do not change this base</testVar></testRecSet><testRecSet><testVar>change this base</testVar></testRecSet></root>", ActivityStrings.BaseConvert_DLWithRecordSetShape, convertCollection);

            var result = ExecuteProcess();
            const string expected = @"Change This base";
            GetRecordSetFieldValueFromDataList(result.Environment, "testRecSet", "testVar", out IList<string> actual, out string error);


            // This should be an index of 2
            var actualValue = actual[1];

            Assert.AreEqual(@"0x646f206e6f74206368616e676520746869732062617365", actual[0]);
            Assert.AreEqual(@"0x6368616e676520746869732062617365", actualValue);
        }

        #endregion RecordSet Tests

        #region Private Test Methods

        void SetupArguments(string currentDL, string testData, IList<BaseConvertTO> convertCollection)
        {
            TestStartNode = new FlowStep
            {
                Action = new DsfBaseConvertActivity { ConvertCollection = convertCollection }
            };

            CurrentDl = testData;
            TestData = currentDL;
        }

        #endregion Private Test Methods
    }
}
