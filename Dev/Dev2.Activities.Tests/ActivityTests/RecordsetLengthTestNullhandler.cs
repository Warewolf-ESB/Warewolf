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
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;


namespace Dev2.Tests.Activities.ActivityTests
{
    /// <summary>
    /// Summary description for RecordsetLengthTestNullhandler
    /// </summary>
    [TestClass]
    public class RecordsetLengthTestNullhandler : BaseActivityUnitTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Store To Scalar Tests

        [TestMethod]
        [Timeout(60000)]
        public void RecordsetLengthOutputToScalar_Expected_ScalarValueCorrectlySetToRecordSetCount()
        {

            SetupArguments("<root>" + ActivityStrings.CountRecordsDataListShape + "</root>", "<root><recset1><field1/></recset1><TestCountvar/></root>", "[[recset1()]]", "[[TestCountvar]]");

            var result = ExecuteProcess();
            const string expected = @"5";
            GetScalarValueFromEnvironment(result.Environment, "TestCountvar", out string actual, out string error);

            // remove test datalist ;)

            Assert.AreEqual(expected, actual);

        }

        #endregion Store To Scalar Tests

        #region Store To RecordSet Tests

        [TestMethod]
        [Timeout(60000)]
        public void RecordsetLengthOutputToRecset()
        {
            SetupArguments("<root>" + ActivityStrings.CountRecordsDataListShape + "</root>", "<root><recset1><field1/></recset1><TestCountvar/></root>", "[[recset1()]]", "[[recset1().field1]]");
            var result = ExecuteProcess();

            const string expected = "5";
            GetRecordSetFieldValueFromDataList(result.Environment, "recset1", "field1", out IList<string> actual, out string error);
            var actualSet = actual.First(c => !string.IsNullOrEmpty(c));

            // remove test datalist ;)

            Assert.AreEqual(expected, actualSet);

        }

        #endregion Store To RecordSet Tests

        #region Error Test Cases

        #endregion Error Test Cases

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void DsfRecordsetLengthActivity_GivenIsNew_ShouldTreatNullAsZero()
        {
            //---------------Set up test pack-------------------
            var act = new DsfRecordsetNullhandlerLengthActivity();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(act);
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsTrue(act.TreatNullAsZero);
        }
        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Execute_GivenEmptyRecordSet_ShouldResturnZero()
        {
            //---------------Set up test pack-------------------
            SetupArguments(ActivityStrings.EmptyRecordSet, ActivityStrings.EmptyRecordSetNoData, "[[recset1()]]", "[[res]]", true);
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

        [TestMethod]
        [Timeout(60000)]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DsfRecordsetLengthActivity_UpdateForEachInputs")]
        public void DsfRecordsetLengthActivity_UpdateForEachInputs_NullUpdates_DoesNothing()
        {
            //------------Setup for test--------------------------
            const string recordsetName = "[[Customers()]]";
            var act = new DsfRecordsetNullhandlerLengthActivity { RecordsetName = recordsetName, RecordsLength = "[[res]]" };
            //------------Execute Test---------------------------
            act.UpdateForEachInputs(null);
            //------------Assert Results-------------------------
            Assert.AreEqual(recordsetName, act.RecordsetName);
        }



        [TestMethod]
        [Timeout(60000)]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DsfRecordsetLengthActivity_UpdateForEachInputs")]
        public void DsfRecordsetLengthActivity_UpdateForEachInputs_MoreThan1Updates_DoesNothing()
        {
            //------------Setup for test--------------------------
            const string recordsetName = "[[Customers()]]";
            var act = new DsfRecordsetNullhandlerLengthActivity { RecordsetName = recordsetName, RecordsLength = "[[res]]" };
            var tuple1 = new Tuple<string, string>("Test", "Test");
            var tuple2 = new Tuple<string, string>("Test2", "Test2");
            //------------Execute Test---------------------------
            act.UpdateForEachInputs(new List<Tuple<string, string>> { tuple1, tuple2 });
            //------------Assert Results-------------------------
            Assert.AreEqual(recordsetName, act.RecordsetName);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DsfRecordsetLengthActivity_UpdateForEachInputs")]
        public void DsfRecordsetLengthActivity_UpdateForEachInputs_1Updates_UpdateRecordsetName()
        {
            //------------Setup for test--------------------------
            const string recordsetName = "[[Customers()]]";
            var act = new DsfRecordsetNullhandlerLengthActivity { RecordsetName = recordsetName, RecordsLength = "[[res]]" };
            var tuple1 = new Tuple<string, string>("Test", "Test");
            //------------Execute Test---------------------------
            act.UpdateForEachInputs(new List<Tuple<string, string>> { tuple1 });
            //------------Assert Results-------------------------
            Assert.AreEqual("Test", act.RecordsetName);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DsfRecordsetLengthActivity_UpdateForEachOutputs")]
        public void DsfRecordsetLengthActivity_UpdateForEachOutputs_NullUpdates_DoesNothing()
        {
            //------------Setup for test--------------------------
            const string recordsetName = "[[Customers()]]";
            var act = new DsfRecordsetNullhandlerLengthActivity { RecordsetName = recordsetName, RecordsLength = "[[res]]" };
            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(null);
            //------------Assert Results-------------------------
            Assert.AreEqual(recordsetName, act.RecordsetName);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DsfRecordsetLengthActivity_UpdateForEachOutputs")]
        public void DsfRecordsetLengthActivity_UpdateForEachOutputs_MoreThan1Updates_DoesNothing()
        {
            //------------Setup for test--------------------------
            const string recordsetName = "[[Customers()]]";
            var act = new DsfRecordsetNullhandlerLengthActivity { RecordsetName = recordsetName, RecordsLength = "[[res]]" };
            var tuple1 = new Tuple<string, string>("Test", "Test");
            var tuple2 = new Tuple<string, string>("Test2", "Test2");
            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(new List<Tuple<string, string>> { tuple1, tuple2 });
            //------------Assert Results-------------------------
            Assert.AreEqual(recordsetName, act.RecordsetName);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DsfRecordsetLengthActivity_UpdateForEachOutputs")]
        public void DsfRecordsetLengthActivity_UpdateForEachOutputs_1Updates_UpdateRecordsLength()
        {
            //------------Setup for test--------------------------
            const string recordsetName = "[[Customers()]]";
            var act = new DsfRecordsetNullhandlerLengthActivity { RecordsetName = recordsetName, RecordsLength = "[[res]]" };
            var tuple1 = new Tuple<string, string>("Test", "Test");
            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(new List<Tuple<string, string>> { tuple1 });
            //------------Assert Results-------------------------
            Assert.AreEqual("Test", act.RecordsLength);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DsfRecordsetLengthActivityGetForEachInputs")]
        public void DsfRecordsetLengthActivity_GetForEachInputs_WhenHasExpression_ReturnsInputList()
        {
            //------------Setup for test--------------------------
            const string recordsetName = "[[Customers()]]";
            var act = new DsfRecordsetNullhandlerLengthActivity { RecordsetName = recordsetName, RecordsLength = "[[res]]" };
            //------------Execute Test---------------------------
            var dsfForEachItems = act.GetForEachInputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, dsfForEachItems.Count);
            Assert.AreEqual(recordsetName, dsfForEachItems[0].Name);
            Assert.AreEqual(recordsetName, dsfForEachItems[0].Value);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DsfRecordsetLengthActivity_GetForEachOutputs")]
        public void DsfRecordsetLengthActivity_GetForEachOutputs_WhenHasResult_ReturnsInputList()
        {
            //------------Setup for test--------------------------
            const string recordsetName = "[[Customers()]]";
            var act = new DsfRecordsetNullhandlerLengthActivity { RecordsetName = recordsetName, RecordsLength = "[[res]]" };
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
        [TestCategory("DsfRecordsetNullhandlerLengthActivity_GetState")]
        public void DsfRecordsetNullhandlerLengthActivity_GetState_ReturnsStateVariable()
        {
            //---------------Set up test pack-------------------
            //------------Setup for test--------------------------
            var act = new DsfRecordsetNullhandlerLengthActivity { RecordsetName = "[[recset()]]",TreatNullAsZero=false, RecordsLength = "[[len]]" };
            //------------Execute Test---------------------------
            var stateItems = act.GetState();
            Assert.AreEqual(3, stateItems.Count());

            var expectedResults = new[]
            {
                new StateVariable
                {
                    Name = "RecordsetName",
                    Type = StateVariable.StateType.Input,
                    Value = "[[recset()]]"
                },
                 new StateVariable
                {
                    Name = "TreatNullAsZero",
                    Type = StateVariable.StateType.Input,
                    Value = "False"
                },
                new StateVariable
                {
                    Name="RecordsLength",
                    Type = StateVariable.StateType.Output,
                    Value = "[[len]]"
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

        #region Private Test Methods

        void SetupArguments(string currentDL, string testData, string recordSetName, string RecordsLength, bool treatNullasZero = false)
        {
            TestStartNode = new FlowStep
            {
                Action = new DsfRecordsetNullhandlerLengthActivity { RecordsetName = recordSetName, RecordsLength = RecordsLength, TreatNullAsZero = treatNullasZero }
            };

            CurrentDl = testData;
            TestData = currentDL;
        }

        #endregion Private Test Methods
    }
}
