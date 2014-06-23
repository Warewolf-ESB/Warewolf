using ActivityUnitTests;
using Dev2.DataList.Contract.Binary_Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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
            GetScalarValueFromDataList(result.DataListID, "TestCountvar", out actual, out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(expected, actual);

        }

        //Bug 7853
        [TestMethod]
        public void CountOutputToScalar_With_EmptyRecSet_Expected_ScalarValueCorrectlySetTo0()
        {

            SetupArguments("<root><ADL><TestCountvar/></ADL></root>", "<root><recset1><field1/></recset1><TestCountvar/></root>", "[[recset1()]]", "[[TestCountvar]]");

            IDSFDataObject result = ExecuteProcess();
            const string expected = @"0";
            string actual;
            string error;
            GetScalarValueFromDataList(result.DataListID, "TestCountvar", out actual, out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(expected, actual);

        }

        //2013.06.03: Ashley Lewis for bug 9498 - multiple regions in result
        [TestMethod]
        public void CountOutputToMultipleScalars_Expected_AllScalarValuesCorrectlySetToRecordSetCount()
        {

            SetupArguments("<root>" + ActivityStrings.CountRecordsDataListShapeWithExtraScalar + "</root>", "<root><recset1><field1/></recset1><TestCountvar/><AnotherTestCountvar/></root>", "[[recset1()]]", "[[TestCountvar]], [[AnotherTestCountvar]]");

            IDSFDataObject result = ExecuteProcess();
            const string expected = @"5";
            string firstActual;
            string secondActual;
            string error;
            GetScalarValueFromDataList(result.DataListID, "TestCountvar", out firstActual, out error);
            GetScalarValueFromDataList(result.DataListID, "AnotherTestCountvar", out secondActual, out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(expected, firstActual);
            Assert.AreEqual(expected, secondActual);

        }

        #endregion Store To Scalar Tests

        #region Store To RecordSet Tests

        [TestMethod]
        public void CountOutputToRecset()
        {
            SetupArguments("<root>" + ActivityStrings.CountRecordsDataListShape + "</root>", "<root><recset1><field1/></recset1><TestCountvar/></root>", "[[recset1()]]", "[[recset1().field1]]");
            IDSFDataObject result = ExecuteProcess();

            const string expected = "5";
            IList<IBinaryDataListItem> actual;
            string error;
            GetRecordSetFieldValueFromDataList(result.DataListID, "recset1", "field1", out actual, out error);
            string actualSet = actual.First(c => c.FieldName == "field1" && !string.IsNullOrEmpty(c.TheValue)).TheValue;

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(expected, actualSet);

        }

        //2013.02.12: Ashley Lewis - Bug 8725, Task 8831 DONE
        [TestMethod]
        public void CountTwiceWithEmptyRecsetExpectedOutputToRecsetsSelf()
        {
            SetupArguments("<root></root>", "<root><recset1><field1/></recset1><TestCountvar/></root>", "[[recset1()]]", "[[recset1().field1]]");
            IDSFDataObject result = ExecuteProcess();

            const string expected = "0";
            IList<IBinaryDataListItem> actual;
            string error;
            GetRecordSetFieldValueFromDataList(result.DataListID, "recset1", "field1", out actual, out error);
            string actualSet = actual.First(c => c.FieldName == "field1" && c.ItemCollectionIndex == 1).TheValue;

            SetupArguments("<root></root>", "<root><recset1><field1/></recset1><TestCountvar/></root>", "[[recset1()]]", "[[recset1().field1]]");
            result = ExecuteProcess();


            const string expected2 = "1";
            GetRecordSetFieldValueFromDataList(result.DataListID, "recset1", "field1", out actual, out error);
            string actualSet2 = actual.First(c => c.FieldName == "field1" && c.ItemCollectionIndex == 2).TheValue;

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(expected, actualSet);
            Assert.AreEqual(expected2, actualSet2);

        }

        #endregion Store To RecordSet Tests

        #region Error Test Cases

        [TestMethod]
        public void CountWithNoRecsetName_Expected_ErrorPopulatedFromDataList()
        {
            SetupArguments("<root>" + ActivityStrings.CountRecordsDataListShape + "</root>", "<root><recset1><field1/></recset1><TestCountvar/></root>", string.Empty, "[[TestCountvar]]");
            IDSFDataObject result = ExecuteProcess();

            var res = Compiler.HasErrors(result.DataListID);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.IsTrue(res);
        }

        [TestMethod]
        public void CountWithNoOutputVariable()
        {
            SetupArguments("<root>" + ActivityStrings.CountRecordsDataListShape + "</root>", "<root><recset1><field1/></recset1><TestCountvar/></root>", "[[recset1()]]", string.Empty);
            IDSFDataObject result = ExecuteProcess();

            var res = Compiler.HasErrors(result.DataListID);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.IsTrue(res);
        }

        [TestMethod]
        public void CountOnScalar()
        {
            SetupArguments("<root>" + ActivityStrings.CountRecordsDataListShape + "</root>", "<root><recset1><field1/></recset1><TestCountvar/></root>", "[[TestCountVar]]", "[[TestCountVar]]");
            IDSFDataObject result = ExecuteProcess();

            var res = Compiler.HasErrors(result.DataListID);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.IsTrue(res);
        }


        [TestMethod]
        public void CountRecords_ErrorHandeling_Expected_ErrorTag()
        {
            SetupArguments("<root>" + ActivityStrings.CountRecordsDataListShape + "</root>", "<root><recset1><field1/></recset1><TestCountvar/></root>", "[[recset1()]]", "[[//().rec]]");

            IDSFDataObject result = ExecuteProcess();

            var res = Compiler.HasErrors(result.DataListID);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.IsTrue(res);
        }

        #endregion Error Test Cases

        #region Get Input/Output Tests

        [TestMethod]
        public void CountRecordsetActivity_GetInputs_Expected_One_Input()
        {
            DsfCountRecordsetActivity testAct = new DsfCountRecordsetActivity();

            IBinaryDataList inputs = testAct.GetInputs();

            var result = inputs.FetchAllEntries().Count;

            // remove test datalist ;)
            DataListRemoval(inputs.UID);

            Assert.AreEqual(1, result);
        }

        [TestMethod]
        public void CountRecordsetActivity_GetOutputs_Expected_One_Output()
        {
            DsfCountRecordsetActivity testAct = new DsfCountRecordsetActivity();

            IBinaryDataList outputs = testAct.GetOutputs();

            var result = outputs.FetchAllEntries().Count;

            // remove test datalist ;)
            DataListRemoval(outputs.UID);

            Assert.AreEqual(1, result);
        }

        #endregion Get Input/Output Tests

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfCountRecordsetActivity_UpdateForEachInputs")]
        public void DsfCountRecordsetActivity_UpdateForEachInputs_NullUpdates_DoesNothing()
        {
            //------------Setup for test--------------------------
            const string recordsetName = "[[Customers()]]";
            var act = new DsfCountRecordsetActivity { RecordsetName = recordsetName, CountNumber = "[[res]]" };
            //------------Execute Test---------------------------
            act.UpdateForEachInputs(null, null);
            //------------Assert Results-------------------------
            Assert.AreEqual(recordsetName, act.RecordsetName);
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
            act.UpdateForEachInputs(new List<Tuple<string, string>> { tuple1, tuple2 }, null);
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
            act.UpdateForEachInputs(new List<Tuple<string, string>> { tuple1 }, null);
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
            act.UpdateForEachOutputs(null, null);
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
            act.UpdateForEachOutputs(new List<Tuple<string, string>> { tuple1, tuple2 }, null);
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
            act.UpdateForEachOutputs(new List<Tuple<string, string>> { tuple1 }, null);
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
