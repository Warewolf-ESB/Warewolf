
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using ActivityUnitTests;
using Dev2.Common.Interfaces.DataList.Contract;
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
    /// Summary description for RecordsetLengthTest
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class RecordsetLengthTest : BaseActivityUnitTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Store To Scalar Tests

        [TestMethod]
        public void RecordsetLengthOutputToScalar_Expected_ScalarValueCorrectlySetToRecordSetCount()
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

        [TestMethod]
        public void CountOutputToScalar_With_NonExistantRecSet_Expected_Errors()
        {

            SetupArguments("<root><ADL><TestCountvar/></ADL></root>", "<root><recset1><field1/></recset1><TestCountvar/></root>", "[[recset1()]]", "[[TestCountvar]]");

            IDSFDataObject result = ExecuteProcess();
            var res = Compiler.HasErrors(result.DataListID);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.IsTrue(res);
        }

        //Bug 7853
        [TestMethod]
        public void RecordsetLengthOutputToScalar_With_EmptyRecSet_Expected_ScalarValueCorrectlySetTo0()
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
        public void RecordsetLengthOutputToMultipleScalars_Expected_ErrorReturned()
        {

            SetupArguments("<root>" + ActivityStrings.CountRecordsDataListShapeWithExtraScalar + "</root>", "<root><recset1><field1/></recset1><TestCountvar/><AnotherTestCountvar/></root>", "[[recset1()]]", "[[TestCountvar]][[AnotherTestCountvar]]");

            IDSFDataObject result = ExecuteProcess();
            var res = Compiler.HasErrors(result.DataListID);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.IsTrue(res);

        }

        #endregion Store To Scalar Tests

        #region Store To RecordSet Tests

        [TestMethod]
        public void RecordsetLengthOutputToRecset()
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
        public void RecordsetLengthTwiceWithEmptyRecsetExpectedOutputToRecsetsSelf()
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
        public void RecordsetLengthWithNoRecsetName_Expected_ErrorPopulatedFromDataList()
        {
            SetupArguments("<root>" + ActivityStrings.CountRecordsDataListShape + "</root>", "<root><recset1><field1/></recset1><TestCountvar/></root>", string.Empty, "[[TestCountvar]]");
            IDSFDataObject result = ExecuteProcess();

            var res = Compiler.HasErrors(result.DataListID);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.IsTrue(res);
        }

        [TestMethod]
        public void RecordsetLengthWithNoOutputVariable()
        {
            SetupArguments("<root>" + ActivityStrings.CountRecordsDataListShape + "</root>", "<root><recset1><field1/></recset1><TestCountvar/></root>", "[[recset1()]]", string.Empty);
            IDSFDataObject result = ExecuteProcess();

            var res = Compiler.HasErrors(result.DataListID);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.IsTrue(res);
        }

        [TestMethod]
        public void RecordsetLengthOnScalar()
        {
            SetupArguments("<root>" + ActivityStrings.CountRecordsDataListShape + "</root>", "<root><recset1><field1/></recset1><TestCountvar/></root>", "[[TestCountVar]]", "[[TestCountVar]]");
            IDSFDataObject result = ExecuteProcess();

            var res = Compiler.HasErrors(result.DataListID);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.IsTrue(res);
        }


        [TestMethod]
        public void RecordsetLengthRecords_ErrorHandeling_Expected_ErrorTag()
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
        public void RecordsetLengthRecordsetActivity_GetInputs_Expected_One_Input()
        {
            DsfRecordsetLengthActivity testAct = new DsfRecordsetLengthActivity();

            IBinaryDataList inputs = testAct.GetInputs();

            var result = inputs.FetchAllEntries().Count;

            // remove test datalist ;)
            DataListRemoval(inputs.UID);

            Assert.AreEqual(1, result);
        }

        [TestMethod]
        public void RecordsetLengthRecordsetActivity_GetOutputs_Expected_One_Output()
        {
            DsfRecordsetLengthActivity testAct = new DsfRecordsetLengthActivity();

            IBinaryDataList outputs = testAct.GetOutputs();

            var result = outputs.FetchAllEntries().Count;

            // remove test datalist ;)
            DataListRemoval(outputs.UID);

            Assert.AreEqual(1, result);
        }

        #endregion Get Input/Output Tests

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DsfRecordsetLengthActivity_UpdateForEachInputs")]
        public void DsfRecordsetLengthActivity_UpdateForEachInputs_NullUpdates_DoesNothing()
        {
            //------------Setup for test--------------------------
            const string recordsetName = "[[Customers()]]";
            var act = new DsfRecordsetLengthActivity { RecordsetName = recordsetName, RecordsLength = "[[res]]" };
            //------------Execute Test---------------------------
            act.UpdateForEachInputs(null, null);
            //------------Assert Results-------------------------
            Assert.AreEqual(recordsetName, act.RecordsetName);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DsfRecordsetLengthActivity_UpdateForEachInputs")]
        public void DsfRecordsetLengthActivity_UpdateForEachInputs_MoreThan1Updates_DoesNothing()
        {
            //------------Setup for test--------------------------
            const string recordsetName = "[[Customers()]]";
            var act = new DsfRecordsetLengthActivity { RecordsetName = recordsetName, RecordsLength = "[[res]]" };
            var tuple1 = new Tuple<string, string>("Test", "Test");
            var tuple2 = new Tuple<string, string>("Test2", "Test2");
            //------------Execute Test---------------------------
            act.UpdateForEachInputs(new List<Tuple<string, string>> { tuple1, tuple2 }, null);
            //------------Assert Results-------------------------
            Assert.AreEqual(recordsetName, act.RecordsetName);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DsfRecordsetLengthActivity_UpdateForEachInputs")]
        public void DsfRecordsetLengthActivity_UpdateForEachInputs_1Updates_UpdateRecordsetName()
        {
            //------------Setup for test--------------------------
            const string recordsetName = "[[Customers()]]";
            var act = new DsfRecordsetLengthActivity { RecordsetName = recordsetName, RecordsLength = "[[res]]" };
            var tuple1 = new Tuple<string, string>("Test", "Test");
            //------------Execute Test---------------------------
            act.UpdateForEachInputs(new List<Tuple<string, string>> { tuple1 }, null);
            //------------Assert Results-------------------------
            Assert.AreEqual("Test", act.RecordsetName);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DsfRecordsetLengthActivity_UpdateForEachOutputs")]
        public void DsfRecordsetLengthActivity_UpdateForEachOutputs_NullUpdates_DoesNothing()
        {
            //------------Setup for test--------------------------
            const string recordsetName = "[[Customers()]]";
            var act = new DsfRecordsetLengthActivity { RecordsetName = recordsetName, RecordsLength = "[[res]]" };
            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(null, null);
            //------------Assert Results-------------------------
            Assert.AreEqual(recordsetName, act.RecordsetName);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DsfRecordsetLengthActivity_UpdateForEachOutputs")]
        public void DsfRecordsetLengthActivity_UpdateForEachOutputs_MoreThan1Updates_DoesNothing()
        {
            //------------Setup for test--------------------------
            const string recordsetName = "[[Customers()]]";
            var act = new DsfRecordsetLengthActivity { RecordsetName = recordsetName, RecordsLength = "[[res]]" };
            var tuple1 = new Tuple<string, string>("Test", "Test");
            var tuple2 = new Tuple<string, string>("Test2", "Test2");
            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(new List<Tuple<string, string>> { tuple1, tuple2 }, null);
            //------------Assert Results-------------------------
            Assert.AreEqual(recordsetName, act.RecordsetName);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DsfRecordsetLengthActivity_UpdateForEachOutputs")]
        public void DsfRecordsetLengthActivity_UpdateForEachOutputs_1Updates_UpdateRecordsLength()
        {
            //------------Setup for test--------------------------
            const string recordsetName = "[[Customers()]]";
            var act = new DsfRecordsetLengthActivity { RecordsetName = recordsetName, RecordsLength = "[[res]]" };
            var tuple1 = new Tuple<string, string>("Test", "Test");
            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(new List<Tuple<string, string>> { tuple1 }, null);
            //------------Assert Results-------------------------
            Assert.AreEqual("Test", act.RecordsLength);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DsfRecordsetLengthActivityGetForEachInputs")]
        public void DsfRecordsetLengthActivity_GetForEachInputs_WhenHasExpression_ReturnsInputList()
        {
            //------------Setup for test--------------------------
            const string recordsetName = "[[Customers()]]";
            var act = new DsfRecordsetLengthActivity { RecordsetName = recordsetName, RecordsLength = "[[res]]" };
            //------------Execute Test---------------------------
            var dsfForEachItems = act.GetForEachInputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, dsfForEachItems.Count);
            Assert.AreEqual(recordsetName, dsfForEachItems[0].Name);
            Assert.AreEqual(recordsetName, dsfForEachItems[0].Value);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DsfRecordsetLengthActivity_GetForEachOutputs")]
        public void DsfRecordsetLengthActivity_GetForEachOutputs_WhenHasResult_ReturnsInputList()
        {
            //------------Setup for test--------------------------
            const string recordsetName = "[[Customers()]]";
            var act = new DsfRecordsetLengthActivity { RecordsetName = recordsetName, RecordsLength = "[[res]]" };
            //------------Execute Test---------------------------
            var dsfForEachItems = act.GetForEachOutputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, dsfForEachItems.Count);
            Assert.AreEqual("[[res]]", dsfForEachItems[0].Name);
            Assert.AreEqual("[[res]]", dsfForEachItems[0].Value);
        }

        #region Private Test Methods

        private void SetupArguments(string currentDL, string testData, string recordSetName, string RecordsLength)
        {
            TestStartNode = new FlowStep
            {
                Action = new DsfRecordsetLengthActivity { RecordsetName = recordSetName, RecordsLength = RecordsLength }
            };

            CurrentDl = testData;
            TestData = currentDL;
        }

        #endregion Private Test Methods
    }
}
