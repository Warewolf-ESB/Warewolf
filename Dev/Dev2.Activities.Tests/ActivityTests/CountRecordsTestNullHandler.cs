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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.ActivityTests
{
    [TestClass]
    public class CountRecordsTestNullHandler : BaseActivityUnitTest
    {
        [TestMethod]
        [Timeout(60000)]
        public void CountOutputToBlank_Expected_BlankResultVariableError()
        {
            SetupArguments("<root>" + ActivityStrings.CountRecordsDataListShape + "</root>", "<root><recset1><field1/></recset1><TestCountvar/></root>", "[[recset1()]]", string.Empty);

            var result = ExecuteProcess();
            const string expected = @"5";
            GetScalarValueFromEnvironment(result.Environment, "TestCountvar", out string actual, out string error);

            Assert.AreEqual(1, result.Environment.Errors.Count);
            Assert.AreEqual("<InnerError>Blank result variable</InnerError>", result.Environment.Errors.FirstOrDefault());
        }

        #region Store To Scalar Tests

        [TestMethod]
        [Timeout(60000)]
        public void CountOutputToScalar_Expected_ScalarValueCorrectlySetToRecordSetCount()
        {
            SetupArguments("<root>" + ActivityStrings.CountRecordsDataListShape + "</root>", "<root><recset1><field1/></recset1><TestCountvar/></root>", "[[recset1()]]", "[[TestCountvar]]");

            var result = ExecuteProcess();
            const string expected = @"5";
            GetScalarValueFromEnvironment(result.Environment, "TestCountvar", out string actual, out string error);

            // remove test datalist ;)

            Assert.AreEqual(expected, actual);
        }

        //Bug 7853
        [TestMethod]
        [Timeout(60000)]
        public void CountOutputToScalar_With_EmptyRecSet_Expected_ScalarValueCorrectlySetTo0()
        {
            SetupArguments("<root><ADL><TestCountvar/></ADL></root>", "<root><recset1><field1/></recset1><TestCountvar/></root>", "[[recset1()]]", "[[TestCountvar]]");

            var result = ExecuteProcess();
            const string expected = "";
            GetScalarValueFromEnvironment(result.Environment, "TestCountvar", out string actual, out string error);

            // remove test datalist ;)

            Assert.AreEqual(expected, actual);
        } 
        
        #endregion Store To Scalar Tests

        #region Store To RecordSet Tests

        [TestMethod]
        [Timeout(60000)]
        public void CountOutputToRecset()
        {
            SetupArguments("<root>" + ActivityStrings.CountRecordsDataListShape + "</root>", "<root><recset1><field1/></recset1><TestCountvar/></root>", "[[recset1()]]", "[[recset1().field1]]");
            var result = ExecuteProcess();

            const string expected = "5";
            GetRecordSetFieldValueFromDataList(result.Environment, "recset1", "field1", out IList<string> actual, out string error);
            var actualSet = actual.First(c =>  !string.IsNullOrEmpty(c));

            // remove test datalist ;)

            Assert.AreEqual(expected, actualSet);

        }

        #endregion Store To RecordSet Tests
        
        [TestMethod]
        [Timeout(60000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfCountRecordsetActivity_UpdateForEachInputs")]
        public void DsfCountRecordsetActivity_UpdateForEachInputs_NullUpdates_DoesNothing()
        {
            //------------Setup for test--------------------------
            const string recordsetName = "[[Customers()]]";
            var act = new DsfCountRecordsetNullHandlerActivity { RecordsetName = recordsetName, CountNumber = "[[res]]" };
            //------------Execute Test---------------------------
            act.UpdateForEachInputs(null);
            //------------Assert Results-------------------------
            Assert.AreEqual(recordsetName, act.RecordsetName);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfCountRecordsetActivity_UpdateForEachInputs")]
        public void DsfCountRecordsetActivity_UpdateForEachInputs_MoreThan1Updates_DoesNothing()
        {
            //------------Setup for test--------------------------
            const string recordsetName = "[[Customers()]]";
            var act = new DsfCountRecordsetNullHandlerActivity { RecordsetName = recordsetName, CountNumber = "[[res]]" };
            var tuple1 = new Tuple<string, string>("Test", "Test");
            var tuple2 = new Tuple<string, string>("Test2", "Test2");
            //------------Execute Test---------------------------
            act.UpdateForEachInputs(new List<Tuple<string, string>> { tuple1, tuple2 });
            //------------Assert Results-------------------------
            Assert.AreEqual(recordsetName, act.RecordsetName);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfCountRecordsetActivity_UpdateForEachInputs")]
        public void DsfCountRecordsetActivity_UpdateForEachInputs_1Updates_UpdateRecordsetName()
        {
            //------------Setup for test--------------------------
            const string recordsetName = "[[Customers()]]";
            var act = new DsfCountRecordsetNullHandlerActivity { RecordsetName = recordsetName, CountNumber = "[[res]]" };
            var tuple1 = new Tuple<string, string>("Test", "Test");
            //------------Execute Test---------------------------
            act.UpdateForEachInputs(new List<Tuple<string, string>> { tuple1 });
            //------------Assert Results-------------------------
            Assert.AreEqual("Test", act.RecordsetName);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfCountRecordsetActivity_UpdateForEachOutputs")]
        public void DsfCountRecordsetActivity_UpdateForEachOutputs_NullUpdates_DoesNothing()
        {
            //------------Setup for test--------------------------
            const string recordsetName = "[[Customers()]]";
            var act = new DsfCountRecordsetNullHandlerActivity { RecordsetName = recordsetName, CountNumber = "[[res]]" };
            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(null);
            //------------Assert Results-------------------------
            Assert.AreEqual(recordsetName, act.RecordsetName);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfCountRecordsetActivity_UpdateForEachOutputs")]
        public void DsfCountRecordsetActivity_UpdateForEachOutputs_MoreThan1Updates_DoesNothing()
        {
            //------------Setup for test--------------------------
            const string recordsetName = "[[Customers()]]";
            var act = new DsfCountRecordsetNullHandlerActivity { RecordsetName = recordsetName, CountNumber = "[[res]]" };
            var tuple1 = new Tuple<string, string>("Test", "Test");
            var tuple2 = new Tuple<string, string>("Test2", "Test2");
            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(new List<Tuple<string, string>> { tuple1, tuple2 });
            //------------Assert Results-------------------------
            Assert.AreEqual(recordsetName, act.RecordsetName);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfCountRecordsetActivity_UpdateForEachOutputs")]
        public void DsfCountRecordsetActivity_UpdateForEachOutputs_1Updates_UpdateCountNumber()
        {
            //------------Setup for test--------------------------
            const string recordsetName = "[[Customers()]]";
            var act = new DsfCountRecordsetNullHandlerActivity { RecordsetName = recordsetName, CountNumber = "[[res]]" };
            var tuple1 = new Tuple<string, string>("[[res]]", "Test");
            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(new List<Tuple<string, string>> { tuple1 });
            //------------Assert Results-------------------------
            Assert.AreEqual("Test", act.CountNumber);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfCountRecordsetActivityGetForEachInputs")]
        public void DsfCountRecordsetActivity_GetForEachInputs_WhenHasExpression_ReturnsInputList()
        {
            //------------Setup for test--------------------------
            const string recordsetName = "[[Customers()]]";
            var act = new DsfCountRecordsetNullHandlerActivity { RecordsetName = recordsetName, CountNumber = "[[res]]" };
            //------------Execute Test---------------------------
            var dsfForEachItems = act.GetForEachInputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, dsfForEachItems.Count);
            Assert.AreEqual(recordsetName, dsfForEachItems[0].Name);
            Assert.AreEqual(recordsetName, dsfForEachItems[0].Value);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfCountRecordsetActivity_GetForEachOutputs")]
        public void DsfCountRecordsetActivity_GetForEachOutputs_WhenHasResult_ReturnsInputList()
        {
            //------------Setup for test--------------------------
            const string recordsetName = "[[Customers()]]";
            var act = new DsfCountRecordsetNullHandlerActivity { RecordsetName = recordsetName, CountNumber = "[[res]]" };
            //------------Execute Test---------------------------
            var dsfForEachItems = act.GetForEachOutputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, dsfForEachItems.Count);
            Assert.AreEqual("[[res]]", dsfForEachItems[0].Name);
            Assert.AreEqual("[[res]]", dsfForEachItems[0].Value);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfCountRecordsetNullHandlerActivity_GetState")]
        public void DsfCountRecordsetNullHandlerActivity_GetState_ReturnsStateVariable()
        {
            //---------------Set up test pack-------------------
            const string recordsetName = "[[Customers()]]";
            //------------Setup for test--------------------------
            var act = new DsfCountRecordsetNullHandlerActivity { RecordsetName = recordsetName, TreatNullAsZero=true, CountNumber = "[[res]]" };
            //------------Execute Test---------------------------
            var stateItems = act.GetState();
            Assert.AreEqual(3, stateItems.Count());

            var expectedResults = new[]
            {
                new StateVariable
                {
                    Name = "RecordsetName",
                    Type = StateVariable.StateType.Input,
                    Value = recordsetName
                },
                 new StateVariable
                {
                    Name = "TreatNullAsZero",
                    Type = StateVariable.StateType.Input,
                    Value = "True"
                },
                new StateVariable
                {
                    Name="CountNumber",
                    Type = StateVariable.StateType.Output,
                    Value = "[[res]]"
                }
            };

            var iter = act.GetState().Select(
                (item, index) => new
                {
                    value = item,
                    expectValue = expectedResults[index]
                }
                );

            //------------Assert Results-------------------------
            foreach (var entry in iter)
            {
                Assert.AreEqual(entry.expectValue.Name, entry.value.Name);
                Assert.AreEqual(entry.expectValue.Type, entry.value.Type);
                Assert.AreEqual(entry.expectValue.Value, entry.value.Value);
            }
        }

        #region |Valid Recordset Name|

        [TestMethod]
        [Timeout(60000)]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DsfCountRecordsetActivity_Execute")]
        public void DsfCountRecordsetActivity_Execute_EmptyRecordsetName_NoCount()
        {
            //------------Setup for test--------------------------
            SetupArguments(ActivityStrings.DeleteRecordsDataListWithData, ActivityStrings.DeleteRecordsDataListShape, "", "[[res]]");
            //------------Execute Test---------------------------
            var result = ExecuteProcess();
            //------------Assert Results-------------------------
            const string Expected = @"";
            GetScalarValueFromEnvironment(result.Environment, "res", out string actual, out string error);
            // remove test datalist ;)
            Assert.AreEqual(Expected, actual);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DsfCountRecordsetActivity_Execute")]
        public void DsfCountRecordsetActivity_Execute_RecordsetHasFieldName_NoCount()
        {
            //------------Setup for test--------------------------
            SetupArguments(ActivityStrings.DeleteRecordsDataListWithData, ActivityStrings.DeleteRecordsDataListShape, "[[recset1().field1]]", "[[res]]");
            //------------Execute Test---------------------------
            var result = ExecuteProcess();
            //------------Assert Results-------------------------
            const string Expected = @"";
            GetScalarValueFromEnvironment(result.Environment, "res", out string actual, out string error);
            // remove test datalist ;)
            Assert.AreEqual(Expected, actual);
        }
        
        [TestMethod]
        [Timeout(60000)]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DsfCountRecordsetActivity_Execute")]
        public void DsfCountRecordsetActivity_Execute_TwoInputVariables_NoCount()
        {
            //------------Setup for test--------------------------
            SetupArguments(ActivityStrings.DeleteRecordsDataListWithData, ActivityStrings.DeleteRecordsDataListShape, "[[recset1()]][[recset1()]]", "[[res]]");
            //------------Execute Test---------------------------
            var result = ExecuteProcess();
            //------------Assert Results-------------------------
            const string Expected = @"";
            GetScalarValueFromEnvironment(result.Environment, "res", out string actual, out string error);
            // remove test datalist ;)
            Assert.AreEqual(Expected, actual);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DsfCountRecordsetActivity_Execute")]
        public void DsfCountRecordsetActivity_Execute_InputIsAScalar_NoCount()
        {
            //------------Setup for test--------------------------
            SetupArguments(ActivityStrings.DeleteRecordsDataListWithData, ActivityStrings.DeleteRecordsDataListShape, "[[recset1]]", "[[res]]");
            //------------Execute Test---------------------------
            var result = ExecuteProcess();
            //------------Assert Results-------------------------
            const string Expected = @"";
            GetScalarValueFromEnvironment(result.Environment, "res", out string actual, out string error);
            // remove test datalist ;)
            Assert.AreEqual(Expected, actual);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void TreaNullAsZero_GivenActivityIsNew_ShouldhaveValueTrue()
        {
            //---------------Set up test pack-------------------
            var dsfCountRecordsetActivity = new DsfCountRecordsetNullHandlerActivity();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dsfCountRecordsetActivity);
            //---------------Execute Test ----------------------
            var treaNullAsZero = dsfCountRecordsetActivity.TreatNullAsZero;
            //---------------Test Result -----------------------
            Assert.IsTrue(treaNullAsZero);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Execute_GivenEmptyRecordSet_ShouldResturnZero()
        {
            //---------------Set up test pack-------------------
            SetupArguments(ActivityStrings.EmptyRecordSet, ActivityStrings.EmptyRecordSetNoData, "[[recset1()]]", "[[res]]",true);
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = ExecuteProcess();
            //---------------Test Result -----------------------
            const string Expected = "0";
            GetScalarValueFromEnvironment(result.Environment, "res", out string actual, out string error);
            Assert.AreEqual(Expected, actual);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Execute_GivenEmptyRecordSetTreatNullAsZeroFalse_ShouldResturnError()
        {
            //---------------Set up test pack-------------------
            SetupArguments(ActivityStrings.EmptyRecordSet, ActivityStrings.EmptyRecordSetNoData, "[[recset1()]]", "[[res]]");
            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var result = ExecuteProcess();
            //---------------Test Result -----------------------
            const string Expected = "";
            GetScalarValueFromEnvironment(result.Environment, "res", out string actual, out string error);
            Assert.AreEqual(Expected, actual);
        }

        #endregion

        #region Private Test Methods

        void SetupArguments(string currentDL, string testData, string recordSetName, string countNumber, bool treaNullAsZero = false)
        {
            TestStartNode = new FlowStep
            {
                Action = new DsfCountRecordsetNullHandlerActivity { RecordsetName = recordSetName, CountNumber = countNumber, TreatNullAsZero = treaNullAsZero }


            };

            CurrentDl = testData;
            TestData = currentDL;
        }

        #endregion Private Test Methods
    }
}
