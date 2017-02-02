/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

// ReSharper disable InconsistentNaming
namespace Dev2.Tests.Activities.ActivityTests
{
    /// <summary>
    /// Summary description for CountRecordsTest
    /// </summary>
    [TestClass]
    public class CountRecordsTest : BaseActivityUnitTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Store To Scalar Tests

        [TestMethod]
        public void CountOutputToScalar_Expected_ScalarValueCorrectlySetToRecordSetCount()
        {

            SetupArguments("<root>" + ActivityStrings.CountRecordsDataListShape + "</root>", "<root><recset1><field1/></recset1><TestCountvar/></root>", "[[recset1()]]", "[[TestCountvar]]");

            IDSFDataObject result = ExecuteProcess();
            const string expected = @"5";
            string actual;
            string error;
            GetScalarValueFromEnvironment(result.Environment, "TestCountvar", out actual, out error);

            // remove test datalist ;)

            Assert.AreEqual(expected, actual);

        }

        //Bug 7853
        [TestMethod]
        public void CountOutputToScalar_With_EmptyRecSet_Expected_ScalarValueCorrectlySetTo0()
        {

            SetupArguments("<root><ADL><TestCountvar/></ADL></root>", "<root><recset1><field1/></recset1><TestCountvar/></root>", "[[recset1()]]", "[[TestCountvar]]");

            IDSFDataObject result = ExecuteProcess();
            const string expected = "";
            string actual;
            string error;
            GetScalarValueFromEnvironment(result.Environment, "TestCountvar", out actual, out error);

            // remove test datalist ;)

            Assert.AreEqual(expected, actual);

        } 
        

        #endregion Store To Scalar Tests

        #region Store To RecordSet Tests

        [TestMethod]
        public void CountOutputToRecset()
        {
            SetupArguments("<root>" + ActivityStrings.CountRecordsDataListShape + "</root>", "<root><recset1><field1/></recset1><TestCountvar/></root>", "[[recset1()]]", "[[recset1().field1]]");
            IDSFDataObject result = ExecuteProcess();

            const string expected = "5";
            IList<string> actual;
            string error;
            GetRecordSetFieldValueFromDataList(result.Environment, "recset1", "field1", out actual, out error);
            string actualSet = actual.First(c =>  !string.IsNullOrEmpty(c));

            // remove test datalist ;)

            Assert.AreEqual(expected, actualSet);

        }

        #endregion Store To RecordSet Tests


        

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfCountRecordsetActivity_UpdateForEachInputs")]
        public void DsfCountRecordsetActivity_UpdateForEachInputs_NullUpdates_DoesNothing()
        {
            //------------Setup for test--------------------------
            const string recordsetName = "[[Customers()]]";
            var act = new DsfCountRecordsetActivity { RecordsetName = recordsetName, CountNumber = "[[res]]" };
            //------------Execute Test---------------------------
            act.UpdateForEachInputs(null);
            //------------Assert Results-------------------------
            Assert.AreEqual(recordsetName, act.RecordsetName);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfCountRecordsetActivity_GetOutputs")]
        public void DsfCountRecordsetActivity_GetOutputs_Called_ShouldReturnListWithResultValueInIt()
        {
            //------------Setup for test--------------------------
            const string recordsetName = "[[Customers()]]";
            var act = new DsfCountRecordsetActivity { RecordsetName = recordsetName, CountNumber = "[[res]]" };
            //------------Execute Test---------------------------
            var outputs = act.GetOutputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, outputs.Count);
            Assert.AreEqual("[[res]]", outputs[0]);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfCountRecordsetActivity_UpdateForEachInputs")]
        public void DsfCountRecordsetActivity_UpdateForEachInputs_MoreThan1Updates_DoesNothing()
        {
            //------------Setup for test--------------------------
            const string recordsetName = "[[Customers()]]";
            var act = new DsfCountRecordsetActivity { RecordsetName = recordsetName, CountNumber = "[[res]]" };
            var tuple1 = new Tuple<string, string>("Test", "Test");
            var tuple2 = new Tuple<string, string>("Test2", "Test2");
            //------------Execute Test---------------------------
            act.UpdateForEachInputs(new List<Tuple<string, string>> { tuple1, tuple2 });
            //------------Assert Results-------------------------
            Assert.AreEqual(recordsetName, act.RecordsetName);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfCountRecordsetActivity_UpdateForEachInputs")]
        public void DsfCountRecordsetActivity_UpdateForEachInputs_1Updates_UpdateRecordsetName()
        {
            //------------Setup for test--------------------------
            const string recordsetName = "[[Customers()]]";
            var act = new DsfCountRecordsetActivity { RecordsetName = recordsetName, CountNumber = "[[res]]" };
            var tuple1 = new Tuple<string, string>("Test", "Test");
            //------------Execute Test---------------------------
            act.UpdateForEachInputs(new List<Tuple<string, string>> { tuple1 });
            //------------Assert Results-------------------------
            Assert.AreEqual("Test", act.RecordsetName);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfCountRecordsetActivity_UpdateForEachOutputs")]
        public void DsfCountRecordsetActivity_UpdateForEachOutputs_NullUpdates_DoesNothing()
        {
            //------------Setup for test--------------------------
            const string recordsetName = "[[Customers()]]";
            var act = new DsfCountRecordsetActivity { RecordsetName = recordsetName, CountNumber = "[[res]]" };
            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(null);
            //------------Assert Results-------------------------
            Assert.AreEqual(recordsetName, act.RecordsetName);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfCountRecordsetActivity_UpdateForEachOutputs")]
        public void DsfCountRecordsetActivity_UpdateForEachOutputs_MoreThan1Updates_DoesNothing()
        {
            //------------Setup for test--------------------------
            const string recordsetName = "[[Customers()]]";
            var act = new DsfCountRecordsetActivity { RecordsetName = recordsetName, CountNumber = "[[res]]" };
            var tuple1 = new Tuple<string, string>("Test", "Test");
            var tuple2 = new Tuple<string, string>("Test2", "Test2");
            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(new List<Tuple<string, string>> { tuple1, tuple2 });
            //------------Assert Results-------------------------
            Assert.AreEqual(recordsetName, act.RecordsetName);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfCountRecordsetActivity_UpdateForEachOutputs")]
        public void DsfCountRecordsetActivity_UpdateForEachOutputs_1Updates_UpdateCountNumber()
        {
            //------------Setup for test--------------------------
            const string recordsetName = "[[Customers()]]";
            var act = new DsfCountRecordsetActivity { RecordsetName = recordsetName, CountNumber = "[[res]]" };
            var tuple1 = new Tuple<string, string>("[[res]]", "Test");
            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(new List<Tuple<string, string>> { tuple1 });
            //------------Assert Results-------------------------
            Assert.AreEqual("Test", act.CountNumber);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfCountRecordsetActivityGetForEachInputs")]
        public void DsfCountRecordsetActivity_GetForEachInputs_WhenHasExpression_ReturnsInputList()
        {
            //------------Setup for test--------------------------
            const string recordsetName = "[[Customers()]]";
            var act = new DsfCountRecordsetActivity { RecordsetName = recordsetName, CountNumber = "[[res]]" };
            //------------Execute Test---------------------------
            var dsfForEachItems = act.GetForEachInputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, dsfForEachItems.Count);
            Assert.AreEqual(recordsetName, dsfForEachItems[0].Name);
            Assert.AreEqual(recordsetName, dsfForEachItems[0].Value);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfCountRecordsetActivity_GetForEachOutputs")]
        public void DsfCountRecordsetActivity_GetForEachOutputs_WhenHasResult_ReturnsInputList()
        {
            //------------Setup for test--------------------------
            const string recordsetName = "[[Customers()]]";
            var act = new DsfCountRecordsetActivity { RecordsetName = recordsetName, CountNumber = "[[res]]" };
            //------------Execute Test---------------------------
            var dsfForEachItems = act.GetForEachOutputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, dsfForEachItems.Count);
            Assert.AreEqual("[[res]]", dsfForEachItems[0].Name);
            Assert.AreEqual("[[res]]", dsfForEachItems[0].Value);
        }

        #region |Valid Recordset Name|

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DsfCountRecordsetActivity_Execute")]
        public void DsfCountRecordsetActivity_Execute_EmptyRecordsetName_NoCount()
        {
            //------------Setup for test--------------------------
            SetupArguments(ActivityStrings.DeleteRecordsDataListWithData, ActivityStrings.DeleteRecordsDataListShape, "", "[[res]]");
            //------------Execute Test---------------------------
            IDSFDataObject result = ExecuteProcess();
            //------------Assert Results-------------------------
            const string Expected = @"";
            string actual;
            string error;
            GetScalarValueFromEnvironment(result.Environment, "res", out actual, out error);
            // remove test datalist ;)
            Assert.AreEqual(Expected, actual);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DsfCountRecordsetActivity_Execute")]
        public void DsfCountRecordsetActivity_Execute_RecordsetHasFieldName_NoCount()
        {
            //------------Setup for test--------------------------
            SetupArguments(ActivityStrings.DeleteRecordsDataListWithData, ActivityStrings.DeleteRecordsDataListShape, "[[recset1().field1]]", "[[res]]");
            //------------Execute Test---------------------------
            IDSFDataObject result = ExecuteProcess();
            //------------Assert Results-------------------------
            const string Expected = @"";
            string actual;
            string error;
            GetScalarValueFromEnvironment(result.Environment, "res", out actual, out error);
            // remove test datalist ;)
            Assert.AreEqual(Expected, actual);
        }
        
        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DsfCountRecordsetActivity_Execute")]
        public void DsfCountRecordsetActivity_Execute_TwoInputVariables_NoCount()
        {
            //------------Setup for test--------------------------
            SetupArguments(ActivityStrings.DeleteRecordsDataListWithData, ActivityStrings.DeleteRecordsDataListShape, "[[recset1()]][[recset1()]]", "[[res]]");
            //------------Execute Test---------------------------
            IDSFDataObject result = ExecuteProcess();
            //------------Assert Results-------------------------
            const string Expected = @"";
            string actual;
            string error;
            GetScalarValueFromEnvironment(result.Environment, "res", out actual, out error);
            // remove test datalist ;)
            Assert.AreEqual(Expected, actual);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DsfCountRecordsetActivity_Execute")]
        public void DsfCountRecordsetActivity_Execute_InputIsAScalar_NoCount()
        {
            //------------Setup for test--------------------------
            SetupArguments(ActivityStrings.DeleteRecordsDataListWithData, ActivityStrings.DeleteRecordsDataListShape, "[[recset1]]", "[[res]]");
            //------------Execute Test---------------------------
            IDSFDataObject result = ExecuteProcess();
            //------------Assert Results-------------------------
            const string Expected = @"";
            string actual;
            string error;
            GetScalarValueFromEnvironment(result.Environment, "res", out actual, out error);
            // remove test datalist ;)
            Assert.AreEqual(Expected, actual);
        }
        #endregion

        #region Private Test Methods

        private void SetupArguments(string currentDL, string testData, string recordSetName, string countNumber)
        {
            TestStartNode = new FlowStep
            {
                Action = new DsfCountRecordsetActivity { RecordsetName = recordSetName, CountNumber = countNumber }
            };

            CurrentDl = testData;
            TestData = currentDL;
        }

        #endregion Private Test Methods
    }
}
