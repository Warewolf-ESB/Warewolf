/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.ActivityTests
{
    /// <summary>
    /// Summary description for CountRecordsTest
    /// </summary>
    [TestClass]
    // ReSharper disable InconsistentNaming
    public class DeleteRecordsActivityTest : BaseActivityUnitTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Delete Using Index

        [TestMethod]
        public void DeleteRecord_Using_Index_When_Record_Exists_Expected_RecordToBeRemovedFromRecordset_Success()
        {
            SetupArguments(ActivityStrings.DeleteRecordsDataListWithData, ActivityStrings.DeleteRecordsDataListShape, "[[recset1(1)]]", "[[res]]");

            IDSFDataObject result = ExecuteProcess();
            const string Expected = @"Success";
            string actual;
            string error;
            GetScalarValueFromEnvironment(result.Environment, "res", out actual, out error);
            List<string> recsetData = RetrieveAllRecordSetFieldValues(result.Environment, "recset1", "field1", out error);

            // remove test datalist ;)

            Assert.AreEqual(Expected, actual);
            Assert.AreEqual(5, recsetData.Count);
            Assert.AreEqual("f1r2", recsetData[0]);

        }

        #endregion Delete Using Index

        #region Delete Using Star

        [TestMethod]
        public void DeleteRecord_Using_Star_When_Record_Exists_Expected_WholeRecordsetToBeRemoved_Success()
        {
            SetupArguments(ActivityStrings.DeleteRecordsDataListWithData, ActivityStrings.DeleteRecordsDataListShape, "[[recset1(*)]]", "[[res]]");

            IDSFDataObject result = ExecuteProcess();
            const string Expected = @"Success";
            string actual;
            string error;
            GetScalarValueFromEnvironment(result.Environment, "res", out actual, out error);
            List<string> recsetData = RetrieveAllRecordSetFieldValues(result.Environment, "recset1", "field1", out error);
            // remove test datalist ;)

            Assert.AreEqual(Expected, actual);
            Assert.AreEqual(0, recsetData.Count);
        }

        #endregion Delete Using Star

        #region Delete Using Blank Index

        [TestMethod]
        public void DeleteRecord_Using_Blank_Index_When_Record_Exists_Expected_LastRecordToBeRemoved_Success()
        {
            SetupArguments(ActivityStrings.DeleteRecordsDataListWithData, ActivityStrings.DeleteRecordsDataListShape, "[[recset1()]]", "[[res]]");

            IDSFDataObject result = ExecuteProcess();
            const string Expected = @"Success";
            string actual;
            string error;
            GetScalarValueFromEnvironment(result.Environment, "res", out actual, out error);
            List<string> recsetData = RetrieveAllRecordSetFieldValues(result.Environment, "recset1", "field1", out error);

            // remove test datalist ;)

            Assert.AreEqual(Expected, actual);
            Assert.AreEqual(5, recsetData.Count);
            Assert.AreEqual("f1r5", recsetData[4]);

        }

        #endregion Delete Using Blank Index

        #region Delete Using Evalauted Index

        [TestMethod]
        [TestCategory("DeleteRecords,UnitTest")]
        [Description("Test that When Deleting a Record With An Valid Evaluated Index It Happens As Excepted")]
        [Owner("Travis Frisinger")]
        public void CanDeleteRecordWithValidEvaluatedIndex()
        {

            const string Data = @"<root>
	                    <recset1>
		                    <field1>f1r1</field1>
		                    <field2>f2r1</field2>		
	                    </recset1>
	                    <recset1>
		                    <field1>f1r2</field1>
		                    <field2>f2r2</field2>		
	                    </recset1>
	                    <recset1>
		                    <field1>f1r3</field1>
		                    <field2>f2r3</field2>		
	                    </recset1>
	                    <recset1>
		                    <field1>f1r4</field1>
		                    <field2>f2r4</field2>		
	                    </recset1>
	                    <recset1>
		                    <field1>f1r5</field1>
		                    <field2>f2r5</field2>		
	                    </recset1>
	                    <recset1>
		                    <field1>f1r6</field1>
		                    <field2>f2r6</field2>		
	                    </recset1>
	                    <res></res>
                        <idx>1</idx>
                    </root>";

            const string Shape = @"<root>
	                    <recset1>
		                    <field1></field1>
		                    <field2></field2>		
	                    </recset1>	
	                    <res></res>
                        <idx/>
                    </root>";

            SetupArguments(Data, Shape, "[[recset1([[idx]])]]", "[[res]]");

            IDSFDataObject result = ExecuteProcess();
            const string Expected = @"Success";
            string actual;
            string error;
            GetScalarValueFromEnvironment(result.Environment, "res", out actual, out error);
            List<string> recsetData = RetrieveAllRecordSetFieldValues(result.Environment, "recset1", "field1", out error);

            // remove test datalist ;)

            Assert.AreEqual(Expected, actual);
            Assert.AreEqual(5, recsetData.Count);
            Assert.AreEqual("f1r2", recsetData[0]);

        }

        #endregion

        #region Error Test Cases

        [TestMethod]
        public void DeleteRecord_Blank_RecordSet_Name_When_Record_Exists_Expected_No_Change_Failure()
        {
            SetupArguments(ActivityStrings.DeleteRecordsDataListWithData, ActivityStrings.DeleteRecordsDataListShape, "", "[[res]]");

            IDSFDataObject result = ExecuteProcess();
            string actual;
            string error;
            GetScalarValueFromEnvironment(result.Environment, "res", out actual, out error);
            List<string> recsetData = RetrieveAllRecordSetFieldValues(result.Environment, "recset1", "field1", out error);

            // remove test datalist ;)

            Assert.AreEqual("Failure", actual);
            Assert.AreEqual(6, recsetData.Count);

        }

        [TestMethod]
        public void DeleteRecord_Blank_Result_Field_When_Record_Exists_Expected_RecordRemove()
        {
            SetupArguments(ActivityStrings.DeleteRecordsDataListWithData, ActivityStrings.DeleteRecordsDataListShape, "[[recset1(1)]]", "");

            IDSFDataObject result = ExecuteProcess();
            string expected = string.Empty;
            string actual;
            string error;
            GetScalarValueFromEnvironment(result.Environment, "res", out actual, out error);
            List<string> recsetData = RetrieveAllRecordSetFieldValues(result.Environment, "recset1", "field1", out error);
            // remove test datalist ;)

            Assert.AreEqual(expected, actual);
            Assert.AreEqual(5, recsetData.Count);
            Assert.AreEqual("f1r2", recsetData[0]);

        }

        [TestMethod]
        public void DeleteRecord_Scalar_When_Record_Exists_Expected_NoChange()
        {
            SetupArguments(ActivityStrings.DeleteRecordsDataListWithData, ActivityStrings.DeleteRecordsDataListShape, "[[res]]", "[[res]]");

            IDSFDataObject result = ExecuteProcess();
            string actual;
            string error;
            GetScalarValueFromEnvironment(result.Environment, "res", out actual, out error);
            List<string> recsetData = RetrieveAllRecordSetFieldValues(result.Environment, "recset1", "field1", out error);
            // remove test datalist ;)

            Assert.AreEqual("Failure", actual);
            Assert.AreEqual(6, recsetData.Count);

        }

        [TestMethod]
        public void DeleteRecord_When_Index_Doesnt_Exist_Expected_NoChange()
        {
            SetupArguments(ActivityStrings.DeleteRecordsDataListWithData, ActivityStrings.DeleteRecordsDataListShape, "[[recset1(8)]]", "[[res]]");

            IDSFDataObject result = ExecuteProcess();
            string actual;
            string error;
            GetScalarValueFromEnvironment(result.Environment, "res", out actual, out error);
            List<string> recsetData = RetrieveAllRecordSetFieldValues(result.Environment, "recset1", "field1", out error);
            // remove test datalist ;)

            Assert.AreEqual("Failure", actual);
            Assert.AreEqual(6, recsetData.Count);

        }

        [TestMethod]
        public void DeleteRecord_When_Index_Is_Negative_Expected_No_Change_Failure()
        {
            SetupArguments(ActivityStrings.DeleteRecordsDataListWithData, ActivityStrings.DeleteRecordsDataListShape, "[[recset1(-1)]]", "[[res]]");

            IDSFDataObject result = ExecuteProcess();
            string actual;
            string error;
            GetScalarValueFromEnvironment(result.Environment, "res", out actual, out error);
            List<string> recsetData = RetrieveAllRecordSetFieldValues(result.Environment, "recset1", "field1", out error);
            // remove test datalist ;)

            Assert.AreEqual("Failure", actual);
            Assert.AreEqual(6, recsetData.Count);

        }

        [TestMethod]
        public void DeleteRecord_When_Index_Is_Zero_Expected_No_Change_Failure()
        {
            SetupArguments(ActivityStrings.DeleteRecordsDataListWithData, ActivityStrings.DeleteRecordsDataListShape, "[[recset1(0)]]", "[[res]]");

            IDSFDataObject result = ExecuteProcess();
            string actual;
            string error;
            GetScalarValueFromEnvironment(result.Environment, "res", out actual, out error);
            List<string> recsetData = RetrieveAllRecordSetFieldValues(result.Environment, "recset1", "field1", out error);

            // remove test datalist ;)

            Assert.AreEqual("Failure", actual);
            Assert.AreEqual(6, recsetData.Count);

        }

        #endregion Error Test Cases

        

        #region Private Test Methods

        private void SetupArguments(string currentDl, string testData, string recordSetName, string resultVar)
        {
            TestStartNode = new FlowStep
            {
                Action = new DsfDeleteRecordActivity { RecordsetName = recordSetName, Result = resultVar }
            };

            CurrentDl = testData;
            TestData = currentDl;
        }

        #endregion Private Test Methods



        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfDeleteRecordActivity_UpdateForEachInputs")]
        public void DsfDeleteRecordActivity_UpdateForEachInputs_NullUpdates_DoesNothing()
        {
            //------------Setup for test--------------------------
            const string recordsetName = "[[Numeric()]]";
            var act = new DsfDeleteRecordActivity { RecordsetName = recordsetName, Result = "[[res]]" };
            //------------Execute Test---------------------------
            act.UpdateForEachInputs(null);
            //------------Assert Results-------------------------
            Assert.AreEqual(recordsetName, act.RecordsetName);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfDeleteRecordActivity_GetOutputs")]
        public void DsfDeleteRecordActivity_GetOutputs_Called_ShouldReturnListWithResultValueInIt()
        {
            //------------Setup for test--------------------------
            const string recordsetName = "[[Numeric()]]";
            var act = new DsfDeleteRecordActivity { RecordsetName = recordsetName, Result = "[[res]]" };
            //------------Execute Test---------------------------
            var outputs = act.GetOutputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, outputs.Count);
            Assert.AreEqual("[[res]]", outputs[0]);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfDeleteRecordActivity_UpdateForEachInputs")]
        public void DsfDeleteRecordActivity_UpdateForEachInputs_MoreThan1Updates_DoesNothing()
        {
            //------------Setup for test--------------------------
            const string recordsetName = "[[Numeric()]]";
            var act = new DsfDeleteRecordActivity { RecordsetName = recordsetName, Result = "[[res]]" };
            var tuple1 = new Tuple<string, string>("Test", "Test");
            var tuple2 = new Tuple<string, string>(recordsetName, "Test2");
            //------------Execute Test---------------------------
            act.UpdateForEachInputs(new List<Tuple<string, string>> { tuple1, tuple2 });
            //------------Assert Results-------------------------
            Assert.AreEqual("Test2", act.RecordsetName);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfDeleteRecordActivity_UpdateForEachInputs")]
        public void DsfDeleteRecordActivity_UpdateForEachInputs_UpdatesNotMatching_DoesNotUpdateRecordsetName()
        {
            //------------Setup for test--------------------------
            const string recordsetName = "[[Numeric()]]";
            var act = new DsfDeleteRecordActivity { RecordsetName = recordsetName, Result = "[[res]]" };
            var tuple1 = new Tuple<string, string>("Test", "Test");
            //------------Execute Test---------------------------
            act.UpdateForEachInputs(new List<Tuple<string, string>> { tuple1 });
            //------------Assert Results-------------------------
            Assert.AreEqual(recordsetName, act.RecordsetName);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfDeleteRecordActivity_UpdateForEachOutputs")]
        public void DsfDeleteRecordActivity_UpdateForEachOutputs_NullUpdates_DoesNothing()
        {
            //------------Setup for test--------------------------
            const string result = "[[res]]";
            var act = new DsfDeleteRecordActivity { RecordsetName = "[[Numeric()]]", Result = result };
            act.UpdateForEachOutputs(null);
            //------------Assert Results-------------------------
            Assert.AreEqual(result, act.Result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfDeleteRecordActivity_UpdateForEachOutputs")]
        public void DsfDeleteRecordActivity_UpdateForEachOutputs_MoreThan1Updates_DoesNothing()
        {
            //------------Setup for test--------------------------
            const string result = "[[res]]";
            var act = new DsfDeleteRecordActivity { RecordsetName = "[[Numeric()]]", Result = result };
            var tuple1 = new Tuple<string, string>("Test", "Test");
            var tuple2 = new Tuple<string, string>("Test2", "Test2");
            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(new List<Tuple<string, string>> { tuple1, tuple2 });
            //------------Assert Results-------------------------
            Assert.AreEqual(result, act.Result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfDeleteRecordActivity_UpdateForEachOutputs")]
        public void DsfDeleteRecordActivity_UpdateForEachOutputs_1Updates_UpdateResult()
        {
            //------------Setup for test--------------------------
            var act = new DsfDeleteRecordActivity { RecordsetName = "[[Numeric()]]", Result = "[[res]]" };
            var tuple1 = new Tuple<string, string>("[[res]]", "Test");
            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(new List<Tuple<string, string>> { tuple1 });
            //------------Assert Results-------------------------
            Assert.AreEqual("Test", act.Result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfDeleteRecordActivity_GetForEachInputs")]
        public void DsfDeleteRecordActivity_GetForEachInputs_WhenHasExpression_ReturnsInputList()
        {
            //------------Setup for test--------------------------
            const string recordsetName = "[[Numeric()]]";
            var act = new DsfDeleteRecordActivity { RecordsetName = recordsetName, Result = "[[res]]" };
            //------------Execute Test---------------------------
            var dsfForEachItems = act.GetForEachInputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, dsfForEachItems.Count);
            Assert.AreEqual(recordsetName, dsfForEachItems[0].Name);
            Assert.AreEqual(recordsetName, dsfForEachItems[0].Value);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfDeleteRecordActivityGetForEachOutputs")]
        public void DsfDeleteRecordActivity_GetForEachOutputs_WhenHasResult_ReturnsOutputList()
        {
            //------------Setup for test--------------------------
            var act = new DsfDeleteRecordActivity { RecordsetName = "[[Numeric()]]", Result = "[[res]]" };
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
        [TestCategory("DsfDeleteRecordActivity_Execute")]
        public void DsfDeleteRecordActivity_Execute_EmptyRecordsetName_Failure()
        {
            //------------Setup for test--------------------------
            SetupArguments(ActivityStrings.DeleteRecordsDataListWithData, ActivityStrings.DeleteRecordsDataListShape, "", "[[res]]");
            //------------Execute Test---------------------------
            IDSFDataObject result = ExecuteProcess();
            //------------Assert Results-------------------------
            string actual;
            string error;
            GetScalarValueFromEnvironment(result.Environment, "res", out actual, out error);
            // remove test datalist ;)
            Assert.AreEqual("Failure", actual);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DsfDeleteRecordActivity_Execute")]
        public void DsfDeleteRecordActivity_Execute_RecordsetHasFieldName_Failure()
        {
            //------------Setup for test--------------------------
            SetupArguments(ActivityStrings.DeleteRecordsDataListWithData, ActivityStrings.DeleteRecordsDataListShape, "[[recset1().field1]]", "[[res]]");
            //------------Execute Test---------------------------
            IDSFDataObject result = ExecuteProcess();
            //------------Assert Results-------------------------
            string actual;
            string error;
            GetScalarValueFromEnvironment(result.Environment, "res", out actual, out error);
            // remove test datalist ;)
            Assert.AreEqual("Failure", actual);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DsfDeleteRecordActivity_Execute")]
        public void DsfDeleteRecordActivity_Execute_RecordsetHasAnIndex_Failure()
        {
            //------------Setup for test--------------------------
            SetupArguments(ActivityStrings.DeleteRecordsDataListWithData, ActivityStrings.DeleteRecordsDataListShape, "[[recset1(8)]]", "[[res]]");
            //------------Execute Test---------------------------
            IDSFDataObject result = ExecuteProcess();
            //------------Assert Results-------------------------
            string actual;
            string error;
            GetScalarValueFromEnvironment(result.Environment, "res", out actual, out error);
            // remove test datalist ;)
            Assert.AreEqual("Failure", actual);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DsfDeleteRecordActivity_Execute")]
        public void DsfDeleteRecordActivity_Execute_TwoInputVariables_Failure()
        {
            //------------Setup for test--------------------------
            SetupArguments(ActivityStrings.DeleteRecordsDataListWithData, ActivityStrings.DeleteRecordsDataListShape, "[[recset1()]][[recset1()]]", "[[res]]");
            //------------Execute Test---------------------------
            IDSFDataObject result = ExecuteProcess();
            //------------Assert Results-------------------------
            string actual;
            string error;
            GetScalarValueFromEnvironment(result.Environment, "res", out actual, out error);
            // remove test datalist ;)
            Assert.AreEqual("Failure", actual);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DsfDeleteRecordActivity_Execute")]
        public void DsfDeleteRecordActivity_Execute_InputIsAScalar_Failure()
        {
            //------------Setup for test--------------------------
            SetupArguments(ActivityStrings.DeleteRecordsDataListWithData, ActivityStrings.DeleteRecordsDataListShape, "[[recset1]]", "[[res]]");
            //------------Execute Test---------------------------
            IDSFDataObject result = ExecuteProcess();
            //------------Assert Results-------------------------
            string actual;
            string error;
            GetScalarValueFromEnvironment(result.Environment, "res", out actual, out error);
            // remove test datalist ;)
            Assert.AreEqual("Failure", actual);
        }
        #endregion
    }
}
