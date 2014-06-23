using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Dev2.Common;
using Dev2.Data.Decision;
using Dev2.Data.Decisions.Operations;
using Dev2.Data.SystemTemplates.Models;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests.SystemTemplates
{
    [TestClass]    
    public class DecisionTest
    {

        #region Model Test
        /// <summary>
        /// Travis.Frisinger - Can the core system model conversion happen
        /// </summary>
        [TestMethod]
        public void CanBootStrapModel_To_JSON_Expect_ValidModel()
        {
            Dev2DecisionStack dds = new Dev2DecisionStack { TheStack = new List<Dev2Decision>(), Mode = Dev2DecisionMode.OR, FalseArmText = "False", TrueArmText = "True", DisplayText = "Is True" };
            IDataListCompiler dlc = DataListFactory.CreateDataListCompiler();

            string result = dlc.ConvertModelToJson(dds);
            string expected = @"{""TheStack"":[],""TotalDecisions"":0,""ModelName"":""Dev2DecisionStack"",""Mode"":""OR"",""TrueArmText"":""True"",""FalseArmText"":""False"",""DisplayText"":""Is True""}";

            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// Travis.Frisinger - Convert a JSON model to system model
        /// </summary>
        [TestMethod]
        public void CanConvertJSON_To_Model_Expect_ValidModel()
        {
            Dev2DecisionStack dds;
            IDataListCompiler dlc = DataListFactory.CreateDataListCompiler();

            string payload = @"{""TheStack"":[],""TotalDecisions"":0,""Version"":""1.0.0"",""ModelName"":""Dev2DecisionStack""}";


            dds = dlc.ConvertFromJsonToModel<Dev2DecisionStack>(payload);

            Assert.AreEqual(0, dds.TotalDecisions);

        }


        /// <summary>
        /// Travis.Frisinger - Convert betweeen system and web model
        /// </summary>
        [TestMethod]
        public void CanAddModelItem_And_Convert_ToAndFrom_Expect_ValidModel()
        {
            Dev2DecisionStack dds = new Dev2DecisionStack { TheStack = new List<Dev2Decision>(), Mode = Dev2DecisionMode.OR };
            IDataListCompiler dlc = DataListFactory.CreateDataListCompiler();

            dds.AddModelItem(new Dev2Decision { Col1 = "[[A]]", Col2 = string.Empty, Col3 = string.Empty, EvaluationFn = enDecisionType.IsNumeric });

            string result = dlc.ConvertModelToJson(dds);

            Dev2DecisionStack convertedResult = dlc.ConvertFromJsonToModel<Dev2DecisionStack>(result);


            Assert.AreEqual(1, convertedResult.TotalDecisions);
            Assert.AreEqual(Dev2DecisionMode.OR, convertedResult.Mode);

        }

        // Bug 8605
        [TestMethod]
        public void CanConvertModelItemWithChooseToModelWithChoose_ExpectValidModel()
        {
            string data = @"{""TheStack"":[{""Col1"":""[[A]]"",""Col2"":"""",""Col3"":"""",""PopulatedColumnCount"":1,""EvaluationFn"":""IsNumeric""}, {""Col1"":"""",""Col2"":"""",""Col3"":"""",""PopulatedColumnCount"":0,""EvaluationFn"":""Choose...""}],""TotalDecisions"":2,""ModelName"":""Dev2DecisionStack"",""Mode"":""OR"",""TrueArmText"":null,""FalseArmText"":null}";
            data = Dev2DecisionStack.RemoveDummyOptionsFromModel(data);
            IDataListCompiler dlc = DataListFactory.CreateDataListCompiler();

            Dev2DecisionStack dds = dlc.ConvertFromJsonToModel<Dev2DecisionStack>(data);

            Assert.AreEqual(1, dds.TotalDecisions);
        }

        #endregion

        #region Execution Test

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("Dev2DecisionStack_Evaluate")]
        public void Dev2DecisionStack_Evaluate_WhenTwoRecordsetsCountsDoNotBalance_NoExceptionThrown()
        {
            //------------Setup for test--------------------------

            IBinaryDataList bdl = Dev2BinaryDataListFactory.CreateDataList();

            string error;
            ErrorResultTO errors;

            bdl.TryCreateRecordsetTemplate("Recset", "Test recordset", new List<Dev2Column> { DataListFactory.CreateDev2Column("field", "test field") }, true, out error);
            bdl.TryCreateRecordsetTemplate("MyRec", "Test recordset", new List<Dev2Column> { DataListFactory.CreateDev2Column("field", "test field") }, true, out error);

            // add data to Recset
            bdl.TryCreateRecordsetValue("a", "field", "Recset", 1, out error);
            bdl.TryCreateRecordsetValue("b", "field", "Recset", 2, out error);

            // add data to MyRec
            bdl.TryCreateRecordsetValue("a", "field", "MyRec", 1, out error);

            IDataListCompiler c = DataListFactory.CreateDataListCompiler();
            c.PushBinaryDataList(bdl.UID, bdl, out errors);

            // ExecuteDecisionStack
            string payload = @"{""TheStack"":[{""Col1"":""[[MyRec(*).field]]"",""Col2"":""[[Recset(*).field]]"",""Col3"":"""",""PopulatedColumnCount"":2,""EvaluationFn"":""IsEqual""}],""TotalDecisions"":1,""Mode"":""AND"",""TrueArmText"":null,""FalseArmText"":null}";

            //------------Execute Test---------------------------
            bool result = Dev2DataListDecisionHandler.Instance.ExecuteDecisionStack(payload, new List<string> { bdl.UID.ToString() });

            //------------Assert Results-------------------------
            Assert.IsFalse(result);

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("Dev2DecisionStack_Evaluate")]
        public void Dev2DecisionStack_Evaluate_WhenThreeRecordsetsCountsDoNotBalance_NoExceptionThrown()
        {
            //------------Setup for test--------------------------

            IBinaryDataList bdl = Dev2BinaryDataListFactory.CreateDataList();

            string error;
            ErrorResultTO errors;

            bdl.TryCreateRecordsetTemplate("Recset", "Test recordset", new List<Dev2Column> { DataListFactory.CreateDev2Column("field", "test field") }, true, out error);
            bdl.TryCreateRecordsetTemplate("MyRec", "Test recordset", new List<Dev2Column> { DataListFactory.CreateDev2Column("field", "test field") }, true, out error);
            bdl.TryCreateRecordsetTemplate("TheRec", "Test recordset", new List<Dev2Column> { DataListFactory.CreateDev2Column("field", "test field") }, true, out error);

            // add data to Recset
            bdl.TryCreateRecordsetValue("a", "field", "Recset", 1, out error);
            bdl.TryCreateRecordsetValue("b", "field", "Recset", 2, out error);

            // add data to MyRec
            bdl.TryCreateRecordsetValue("a", "field", "MyRec", 1, out error);

            // add data to TheRec
            bdl.TryCreateRecordsetValue("a", "field", "TheRec", 1, out error);
            bdl.TryCreateRecordsetValue("b", "field", "TheRec", 2, out error);
            bdl.TryCreateRecordsetValue("c", "field", "TheRec", 3, out error);

            IDataListCompiler c = DataListFactory.CreateDataListCompiler();
            c.PushBinaryDataList(bdl.UID, bdl, out errors);

            // ExecuteDecisionStack
            string payload = @"{""TheStack"":[{""Col1"":""[[MyRec(*).field]]"",""Col2"":""[[Recset(*).field]]"",""Col3"":""[[TheRec(*).field]]"",""PopulatedColumnCount"":3,""EvaluationFn"":""IsBetween""}],""TotalDecisions"":1,""Mode"":""AND"",""TrueArmText"":null,""FalseArmText"":null}";

            //------------Execute Test---------------------------
            bool result = Dev2DataListDecisionHandler.Instance.ExecuteDecisionStack(payload, new List<string> { bdl.UID.ToString() });

            //------------Assert Results-------------------------
            Assert.IsFalse(result);

        }

        /// <summary>
        /// Travis.Frisinger - Can push a system model into the Data List
        /// </summary>
        [TestMethod]
        public void CanPushModel_To_DataList_Expect_ValidModel()
        {
            Dev2DecisionStack dds = new Dev2DecisionStack { TheStack = new List<Dev2Decision>(), Mode = Dev2DecisionMode.OR };
            IDataListCompiler dlc = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;

            dds.AddModelItem(new Dev2Decision { Col1 = "[[A]]", Col2 = string.Empty, Col3 = string.Empty, EvaluationFn = enDecisionType.IsNumeric });

            Guid id = dlc.PushSystemModelToDataList(dds, out errors);

            Assert.AreEqual(0, errors.FetchErrors().Count);

            string result = dlc.EvaluateSystemEntry(id, enSystemTag.SystemModel, out errors);

            string expected = @"{""TheStack"":[{""Col1"":""[[A]]"",""Col2"":"""",""Col3"":"""",""PopulatedColumnCount"":1,""EvaluationFn"":""IsNumeric""}],""TotalDecisions"":1,""ModelName"":""Dev2DecisionStack"",""Mode"":""OR"",""TrueArmText"":null,""FalseArmText"":null,""DisplayText"":null}";

            Assert.AreEqual(expected, result);
        }


        /// <summary>
        /// Travis.Frisinger - Can execute a decision stack with mult decisions
        /// </summary>
        [TestMethod]
        public void CanPushModelWithDecisionStack_To_DataList_Expect_ValidModel()
        {
            Dev2DecisionStack dds = new Dev2DecisionStack { TheStack = new List<Dev2Decision>(), Mode = Dev2DecisionMode.OR };
            IDataListCompiler dlc = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;

            dds.AddModelItem(new Dev2Decision { Col1 = "[[A]]", Col2 = string.Empty, Col3 = string.Empty, EvaluationFn = enDecisionType.IsNumeric });
            dds.AddModelItem(new Dev2Decision { Col1 = "[[B]]", Col2 = string.Empty, Col3 = string.Empty, EvaluationFn = enDecisionType.IsNotNumeric });

            Guid id = dlc.PushSystemModelToDataList(dds, out errors);

            Assert.AreEqual(0, errors.FetchErrors().Count);

            string result = dlc.EvaluateSystemEntry(id, enSystemTag.SystemModel, out errors);

            string expected = @"{""TheStack"":[{""Col1"":""[[A]]"",""Col2"":"""",""Col3"":"""",""PopulatedColumnCount"":1,""EvaluationFn"":""IsNumeric""},{""Col1"":""[[B]]"",""Col2"":"""",""Col3"":"""",""PopulatedColumnCount"":1,""EvaluationFn"":""IsNotNumeric""}],""TotalDecisions"":2,""ModelName"":""Dev2DecisionStack"",""Mode"":""OR"",""TrueArmText"":null,""FalseArmText"":null,""DisplayText"":null}";

            Assert.AreEqual(expected, result);

        }

        /// <summary>
        /// Travis.Frisinger - Can execute a decision stack with single decision
        /// </summary>
        [TestMethod]
        public void CanInvokeDecisionStack_SingleDecision_Expect_True()
        {
            IBinaryDataList bdl = Dev2BinaryDataListFactory.CreateDataList();

            string error;
            ErrorResultTO errors;

            bdl.TryCreateScalarTemplate(string.Empty, "A", string.Empty, true, out error);
            bdl.TryCreateScalarValue("1", "A", out error);

            IDataListCompiler c = DataListFactory.CreateDataListCompiler();
            c.PushBinaryDataList(bdl.UID, bdl, out errors);

            // ExecuteDecisionStack
            string payload = @"{""TheStack"":[{""Col1"":""[[A]]"",""Col2"":"""",""Col3"":"""",""PopulatedColumnCount"":1,""EvaluationFn"":""IsNumeric""}],""TotalDecisions"":1,""Mode"":""OR"",""TrueArmText"":null,""FalseArmText"":null}";

            bool result = Dev2DataListDecisionHandler.Instance.ExecuteDecisionStack(payload, new List<string> { bdl.UID.ToString() });

            Assert.IsTrue(result);
        }
        /// <summary>
        /// Travis.Frisinger - Can it fetch SwitchData
        /// </summary>
        [TestMethod]
        public void CanFetchSwitchData_Expect_Data()
        {
            IBinaryDataList bdl = Dev2BinaryDataListFactory.CreateDataList();

            string error;
            ErrorResultTO errors;

            bdl.TryCreateScalarTemplate(string.Empty, "A", string.Empty, true, out error);
            bdl.TryCreateScalarValue("1", "A", out error);

            IDataListCompiler c = DataListFactory.CreateDataListCompiler();
            c.PushBinaryDataList(bdl.UID, bdl, out errors);

            string result = Dev2DataListDecisionHandler.Instance.FetchSwitchData("[[A]]", new List<string> { bdl.UID.ToString() });
            string expected = "1";

            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// Travis.Frisinger - Can it return emtpy string with invalid DataList ID
        /// </summary>
        [TestMethod]
        public void FetchSwitchData_NullDataListID_Expect_NoData()
        {
            IBinaryDataList bdl = Dev2BinaryDataListFactory.CreateDataList();

            string error;
            ErrorResultTO errors;

            bdl.TryCreateScalarTemplate(string.Empty, "A", string.Empty, true, out error);
            bdl.TryCreateScalarValue("1", "A", out error);

            IDataListCompiler c = DataListFactory.CreateDataListCompiler();
            c.PushBinaryDataList(bdl.UID, bdl, out errors);

            string result = Dev2DataListDecisionHandler.Instance.FetchSwitchData("[[A]]", new List<string> { GlobalConstants.NullDataListID.ToString() });
            string expected = "";

            Assert.AreEqual(expected, result);
        }

        /// Travis.Frisinger - Will the execution of a decision stack fail with an invalid DataList ID
        [TestMethod]
        public void CanInvokeDecisionStack_SingleDecision_NullDataListID_Expect_Exception()
        {

            // ExecuteDecisionStack
            string payload = @"{""TheStack"":[{""Col1"":""[[A]]"",""Col2"":"""",""Col3"":"""",""PopulatedColumnCount"":1,""EvaluationFn"":""IsNumeric""}],""TotalDecisions"":1,""Mode"":""OR"",""TrueArmText"":null,""FalseArmText"":null}";

            try
            {
                Dev2DataListDecisionHandler.Instance.ExecuteDecisionStack(payload, new List<string> { GlobalConstants.NullDataListID.ToString() });

                Assert.Fail("Null DL ID Passes?!");
            }
            catch(Exception e)
            {
                Assert.AreEqual("Could not evaluate decision data - no DataList ID sent!", e.Message);
            }
        }


        /// <summary>
        /// Travis.Frisinger - Can it invoke a decision stack with OR condition
        /// </summary>
        [TestMethod]
        public void CanInvokeDecisionStack_MultipleDecision_With_OR_Expect_True()
        {
            IBinaryDataList bdl = Dev2BinaryDataListFactory.CreateDataList();

            string error;
            ErrorResultTO errors;

            bdl.TryCreateScalarTemplate(string.Empty, "A", string.Empty, true, out error);
            bdl.TryCreateScalarValue("1", "A", out error);

            bdl.TryCreateScalarTemplate(string.Empty, "B", string.Empty, true, out error);
            bdl.TryCreateScalarValue("1", "B", out error);

            IDataListCompiler c = DataListFactory.CreateDataListCompiler();
            c.PushBinaryDataList(bdl.UID, bdl, out errors);

            // ExecuteDecisionStack
            string payload = @"{""TheStack"":[{""Col1"":""[[A]]"",""Col2"":"""",""Col3"":"""",""PopulatedColumnCount"":1,""EvaluationFn"":""IsNumeric""}, {""Col1"":""[[B]]"",""Col2"":"""",""Col3"":"""",""PopulatedColumnCount"":1,""EvaluationFn"":""IsNumeric""}],""TotalDecisions"":2,""Mode"":""OR"",""TrueArmText"":null,""FalseArmText"":null}";

            bool result = Dev2DataListDecisionHandler.Instance.ExecuteDecisionStack(payload, new List<string> { bdl.UID.ToString() });

            Assert.IsTrue(result);
        }

        /// <summary>
        /// Travis.Frisinger - Can it invoke a decision stack with AND condition
        /// </summary>
        [TestMethod]
        public void CanInvokeDecisionStack_MultipleDecision_With_AND_Expect_False()
        {
            IBinaryDataList bdl = Dev2BinaryDataListFactory.CreateDataList();

            string error;
            ErrorResultTO errors;

            bdl.TryCreateScalarTemplate(string.Empty, "A", string.Empty, true, out error);
            bdl.TryCreateScalarValue("1", "A", out error);

            bdl.TryCreateScalarTemplate(string.Empty, "B", string.Empty, true, out error);
            bdl.TryCreateScalarValue(".", "B", out error);

            IDataListCompiler c = DataListFactory.CreateDataListCompiler();
            c.PushBinaryDataList(bdl.UID, bdl, out errors);

            // ExecuteDecisionStack
            string payload = @"{""TheStack"":[{""Col1"":""[[A]]"",""Col2"":"""",""Col3"":"""",""PopulatedColumnCount"":1,""EvaluationFn"":""IsNumeric""}, {""Col1"":""[[B]]"",""Col2"":"""",""Col3"":"""",""PopulatedColumnCount"":1,""EvaluationFn"":""IsNumeric""}],""TotalDecisions"":2,""Mode"":""AND"",""TrueArmText"":null,""FalseArmText"":null}";

            bool result = Dev2DataListDecisionHandler.Instance.ExecuteDecisionStack(payload, new List<string> { bdl.UID.ToString() });

            Assert.IsFalse(result);
        }

        /// <summary>
        /// Travis.Frisinger - Can it convert a system model into a web model correctly
        /// </summary>
        [TestMethod]
        public void CanConvert_SystemModel_Into_WebModel_Expect_ValidModel()
        {

            Dev2DecisionStack dds = new Dev2DecisionStack { TheStack = new List<Dev2Decision>(), Mode = Dev2DecisionMode.OR };
            IDataListCompiler dlc = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;

            dds.AddModelItem(new Dev2Decision { Col1 = "[[A]]", Col2 = string.Empty, Col3 = string.Empty, EvaluationFn = enDecisionType.IsNumeric });

            Guid id = dlc.PushSystemModelToDataList(dds, out errors);

            string result = dlc.FetchSystemModelAsWebModel<Dev2DecisionStack>(id, out errors);

            Assert.AreEqual(0, errors.FetchErrors().Count);

            string expected = @"{""TheStack"":[{""Col1"":""[[A]]"",""Col2"":"""",""Col3"":"""",""PopulatedColumnCount"":1,""EvaluationFn"":""IsNumeric""}],""TotalDecisions"":1,""ModelName"":""Dev2DecisionStack"",""Mode"":""OR"",""TrueArmText"":null,""FalseArmText"":null,""DisplayText"":null}";

            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// Travis.Frisinger - Can it convert a decision stack with a null list of decisions into a web model correctly
        /// </summary>
        [TestMethod]
        public void CanConvert_SystemModel_Into_WebModel_WithNullDecision_Expect_ValidModel()
        {

            Dev2DecisionStack dds = new Dev2DecisionStack { TheStack = new List<Dev2Decision>(), Mode = Dev2DecisionMode.OR };
            IDataListCompiler dlc = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;

            dds.AddModelItem(null);

            Guid id = dlc.PushSystemModelToDataList(dds, out errors);

            string result = dlc.FetchSystemModelAsWebModel<Dev2DecisionStack>(id, out errors);

            Assert.AreEqual(0, errors.FetchErrors().Count);

            string expected = @"{""TheStack"":[null],""TotalDecisions"":1,""ModelName"":""Dev2DecisionStack"",""Mode"":""OR"",""TrueArmText"":null,""FalseArmText"":null,""DisplayText"":null}";

            Assert.AreEqual(expected, result);
        }

        //2013.06.06: Ashley Lewis for PBI 9460 - evaluating recordsets with stared indexes
        [TestMethod]
        public void CanInvokeDecisionStackSingleDecisionWithANDModeAndRecordsetAtIndexStarExpectEveryRecordConsidered()
        {
            IBinaryDataList bdl = Dev2BinaryDataListFactory.CreateDataList();

            string error;
            ErrorResultTO errors;

            bdl.TryCreateRecordsetTemplate("Recset", "Test recordset", new List<Dev2Column> {DataListFactory.CreateDev2Column("field","test field")}, true, out error);
            bdl.TryCreateRecordsetValue("a", "field", "Recset", 1, out error);
            bdl.TryCreateRecordsetValue("b", "field", "Recset", 2, out error);
            bdl.TryCreateRecordsetValue("c", "field", "Recset", 3, out error);
            bdl.TryCreateRecordsetValue("1", "field", "Recset", 4, out error);
            bdl.TryCreateRecordsetValue("2", "field", "Recset", 5, out error);
            bdl.TryCreateRecordsetValue("3", "field", "Recset", 6, out error);

            IDataListCompiler c = DataListFactory.CreateDataListCompiler();
            c.PushBinaryDataList(bdl.UID, bdl, out errors);

            // ExecuteDecisionStack
            string payload = @"{""TheStack"":[{""Col1"":""[[Recset(*).field]]"",""Col2"":"""",""Col3"":"""",""PopulatedColumnCount"":1,""EvaluationFn"":""IsNumeric""}],""TotalDecisions"":1,""Mode"":""AND"",""TrueArmText"":null,""FalseArmText"":null}";

            bool result = Dev2DataListDecisionHandler.Instance.ExecuteDecisionStack(payload, new List<string> { bdl.UID.ToString() });


            Assert.IsFalse(result);
        }
        [TestMethod]
        public void CanInvokeDecisionStackManyDecisionWithANDModeAndRecordsetAtIndexStarExpectEveryRecordConsidered()
        {
            IBinaryDataList bdl = Dev2BinaryDataListFactory.CreateDataList();

            string error;
            ErrorResultTO errors;

            bdl.TryCreateRecordsetTemplate("Recset1", "First test recordset", new List<Dev2Column> { DataListFactory.CreateDev2Column("field", "test field") }, true, out error);
            bdl.TryCreateRecordsetValue("1", "field", "Recset1", 1, out error);
            bdl.TryCreateRecordsetValue("2", "field", "Recset1", 2, out error);
            bdl.TryCreateRecordsetValue("3", "field", "Recset1", 3, out error);
            bdl.TryCreateRecordsetValue("4", "field", "Recset1", 4, out error);
            bdl.TryCreateRecordsetValue("5", "field", "Recset1", 5, out error);
            bdl.TryCreateRecordsetValue("6", "field", "Recset1", 6, out error);

            bdl.TryCreateRecordsetTemplate("Recset2", "Second test recordset", new List<Dev2Column> { DataListFactory.CreateDev2Column("field", "test field") }, true, out error);
            bdl.TryCreateRecordsetValue("a", "field", "Recset2", 1, out error);
            bdl.TryCreateRecordsetValue("b", "field", "Recset2", 2, out error);
            bdl.TryCreateRecordsetValue("c", "field", "Recset2", 3, out error);
            bdl.TryCreateRecordsetValue("1", "field", "Recset2", 4, out error);
            bdl.TryCreateRecordsetValue("2", "field", "Recset2", 5, out error);
            bdl.TryCreateRecordsetValue("3", "field", "Recset2", 6, out error);

            IDataListCompiler c = DataListFactory.CreateDataListCompiler();
            c.PushBinaryDataList(bdl.UID, bdl, out errors);

            // ExecuteDecisionStack
            string payload = @"{""TheStack"":[{""Col1"":""[[Recset1(*).field]]"",""Col2"":"""",""Col3"":"""",""PopulatedColumnCount"":1,""EvaluationFn"":""IsNumeric""}, {""Col1"":""[[Recset2(*).field]]"",""Col2"":"""",""Col3"":"""",""PopulatedColumnCount"":1,""EvaluationFn"":""IsNumeric""}],""TotalDecisions"":2,""Mode"":""AND"",""TrueArmText"":null,""FalseArmText"":null}";

            bool result = Dev2DataListDecisionHandler.Instance.ExecuteDecisionStack(payload, new List<string> { bdl.UID.ToString() });

            Assert.IsFalse(result);
        }
        [TestMethod]
        public void CanInvokeDecisionStackSingleDecisionWithOrModeAndRecordsetAtIndexStarExpectEveryRecordConsidered()
        {
            IBinaryDataList bdl = Dev2BinaryDataListFactory.CreateDataList();

            string error;
            ErrorResultTO errors;

            bdl.TryCreateRecordsetTemplate("Recset", "Test recordset", new List<Dev2Column> { DataListFactory.CreateDev2Column("field", "test field") }, true, out error);
            bdl.TryCreateRecordsetValue("a", "field", "Recset", 1, out error);
            bdl.TryCreateRecordsetValue("b", "field", "Recset", 2, out error);
            bdl.TryCreateRecordsetValue("3", "field", "Recset", 3, out error);
            bdl.TryCreateRecordsetValue("d", "field", "Recset", 4, out error);
            bdl.TryCreateRecordsetValue("e", "field", "Recset", 5, out error);
            bdl.TryCreateRecordsetValue("f", "field", "Recset", 6, out error);

            IDataListCompiler c = DataListFactory.CreateDataListCompiler();
            c.PushBinaryDataList(bdl.UID, bdl, out errors);

            // ExecuteDecisionStack
            string payload = @"{""TheStack"":[{""Col1"":""[[Recset(*).field]]"",""Col2"":"""",""Col3"":"""",""PopulatedColumnCount"":1,""EvaluationFn"":""IsNumeric""}],""TotalDecisions"":1,""Mode"":""OR"",""TrueArmText"":null,""FalseArmText"":null}";

            bool result = Dev2DataListDecisionHandler.Instance.ExecuteDecisionStack(payload, new List<string> { bdl.UID.ToString() });

            Assert.IsTrue(result);
        }
        [TestMethod]
        public void CanInvokeDecisionStackManyDecisionWithOrModeAndRecordsetAtIndexStarExpectEveryRecordConsidered()
        {
            IBinaryDataList bdl = Dev2BinaryDataListFactory.CreateDataList();

            string error;
            ErrorResultTO errors;

            bdl.TryCreateRecordsetTemplate("Recset1", "First test recordset", new List<Dev2Column> { DataListFactory.CreateDev2Column("field", "test field") }, true, out error);
            bdl.TryCreateRecordsetValue("a", "field", "Recset1", 1, out error);
            bdl.TryCreateRecordsetValue("b", "field", "Recset1", 2, out error);
            bdl.TryCreateRecordsetValue("c", "field", "Recset1", 3, out error);
            bdl.TryCreateRecordsetValue("d", "field", "Recset1", 4, out error);
            bdl.TryCreateRecordsetValue("e", "field", "Recset1", 5, out error);
            bdl.TryCreateRecordsetValue("f", "field", "Recset1", 6, out error);

            bdl.TryCreateRecordsetTemplate("Recset2", "Second test recordset", new List<Dev2Column> { DataListFactory.CreateDev2Column("field", "test field") }, true, out error);
            bdl.TryCreateRecordsetValue("1", "field", "Recset2", 1, out error);
            bdl.TryCreateRecordsetValue("2", "field", "Recset2", 2, out error);
            bdl.TryCreateRecordsetValue("3", "field", "Recset2", 3, out error);
            bdl.TryCreateRecordsetValue("a", "field", "Recset2", 4, out error);
            bdl.TryCreateRecordsetValue("b", "field", "Recset2", 5, out error);
            bdl.TryCreateRecordsetValue("c", "field", "Recset2", 6, out error);

            IDataListCompiler c = DataListFactory.CreateDataListCompiler();
            c.PushBinaryDataList(bdl.UID, bdl, out errors);

            // ExecuteDecisionStack
            string payload = @"{""TheStack"":[{""Col1"":""[[Recset1(*).field]]"",""Col2"":"""",""Col3"":"""",""PopulatedColumnCount"":1,""EvaluationFn"":""IsNumeric""}, {""Col1"":""[[Recset2(*).field]]"",""Col2"":"""",""Col3"":"""",""PopulatedColumnCount"":1,""EvaluationFn"":""IsNumeric""}],""TotalDecisions"":2,""Mode"":""OR"",""TrueArmText"":null,""FalseArmText"":null}";

            bool result = Dev2DataListDecisionHandler.Instance.ExecuteDecisionStack(payload, new List<string> { bdl.UID.ToString() });

            Assert.IsTrue(result);
        }
        //Two recordsets both with starred indexes
        [TestMethod]
        public void CanInvokeDecisionStackSingleDecisionWithANDModeAndTwoRecordsetsAtIndexStarExpectEveryRecordConsidered()
        {
            IBinaryDataList bdl = Dev2BinaryDataListFactory.CreateDataList();

            string error;
            ErrorResultTO errors;

            bdl.TryCreateRecordsetTemplate("Recset", "Test recordset", new List<Dev2Column> { DataListFactory.CreateDev2Column("field", "test field") }, true, out error);
            bdl.TryCreateRecordsetValue("a", "field", "Recset", 1, out error);
            bdl.TryCreateRecordsetValue("b", "field", "Recset", 2, out error);
            bdl.TryCreateRecordsetValue("c", "field", "Recset", 3, out error);
            bdl.TryCreateRecordsetValue("1", "field", "Recset", 4, out error);
            bdl.TryCreateRecordsetValue("2", "field", "Recset", 5, out error);
            bdl.TryCreateRecordsetValue("3", "field", "Recset", 6, out error);

            bdl.TryCreateRecordsetTemplate("Recset2", "Second test recordset", new List<Dev2Column> { DataListFactory.CreateDev2Column("field", "test field") }, true, out error);
            bdl.TryCreateRecordsetValue("a", "field", "Recset2", 1, out error);
            bdl.TryCreateRecordsetValue("b", "field", "Recset2", 2, out error);
            bdl.TryCreateRecordsetValue("c", "field", "Recset2", 3, out error);
            bdl.TryCreateRecordsetValue("zingzopwowee", "field", "Recset2", 4, out error);
            bdl.TryCreateRecordsetValue("2", "field", "Recset2", 5, out error);
            bdl.TryCreateRecordsetValue("3", "field", "Recset2", 6, out error);

            IDataListCompiler c = DataListFactory.CreateDataListCompiler();
            c.PushBinaryDataList(bdl.UID, bdl, out errors);

            // ExecuteDecisionStack
            string payload = @"{""TheStack"":[{""Col1"":""[[Recset(*).field]]"",""Col2"":""[[Recset2(*).field]]"",""Col3"":"""",""PopulatedColumnCount"":2,""EvaluationFn"":""IsEqual""}],""TotalDecisions"":1,""Mode"":""AND"",""TrueArmText"":null,""FalseArmText"":null}";

            bool result = Dev2DataListDecisionHandler.Instance.ExecuteDecisionStack(payload, new List<string> { bdl.UID.ToString() });

            Assert.IsFalse(result);
        }
        [TestMethod]
        public void CanInvokeDecisionStackManyDecisionWithANDModeAndTwoRecordsetsAtIndexStarExpectEveryRecordConsidered()
        {
            IBinaryDataList bdl = Dev2BinaryDataListFactory.CreateDataList();

            string error;
            ErrorResultTO errors;

            bdl.TryCreateRecordsetTemplate("Recset1", "First test recordset", new List<Dev2Column> { DataListFactory.CreateDev2Column("field", "test field") }, true, out error);
            bdl.TryCreateRecordsetValue("1", "field", "Recset1", 1, out error);
            bdl.TryCreateRecordsetValue("2", "field", "Recset1", 2, out error);
            bdl.TryCreateRecordsetValue("3", "field", "Recset1", 3, out error);
            bdl.TryCreateRecordsetValue("4", "field", "Recset1", 4, out error);
            bdl.TryCreateRecordsetValue("5", "field", "Recset1", 5, out error);
            bdl.TryCreateRecordsetValue("6", "field", "Recset1", 6, out error);

            bdl.TryCreateRecordsetTemplate("Recset2", "Second test recordset", new List<Dev2Column> { DataListFactory.CreateDev2Column("field", "test field") }, true, out error);
            bdl.TryCreateRecordsetValue("a", "field", "Recset2", 1, out error);
            bdl.TryCreateRecordsetValue("b", "field", "Recset2", 2, out error);
            bdl.TryCreateRecordsetValue("c", "field", "Recset2", 3, out error);
            bdl.TryCreateRecordsetValue("1", "field", "Recset2", 4, out error);
            bdl.TryCreateRecordsetValue("2", "field", "Recset2", 5, out error);
            bdl.TryCreateRecordsetValue("3", "field", "Recset2", 6, out error);

            bdl.TryCreateRecordsetTemplate("Recset3", "Third test recordset", new List<Dev2Column> { DataListFactory.CreateDev2Column("field", "test field") }, true, out error);
            bdl.TryCreateRecordsetValue("1", "field", "Recset3", 1, out error);
            bdl.TryCreateRecordsetValue("2", "field", "Recset3", 2, out error);
            bdl.TryCreateRecordsetValue("3", "field", "Recset3", 3, out error);
            bdl.TryCreateRecordsetValue("4", "field", "Recset3", 4, out error);
            bdl.TryCreateRecordsetValue("5", "field", "Recset3", 5, out error);
            bdl.TryCreateRecordsetValue("6", "field", "Recset3", 6, out error);

            bdl.TryCreateRecordsetTemplate("Recset4", "Fourth test recordset", new List<Dev2Column> { DataListFactory.CreateDev2Column("field", "test field") }, true, out error);
            bdl.TryCreateRecordsetValue("a", "field", "Recset4", 1, out error);
            bdl.TryCreateRecordsetValue("b", "field", "Recset4", 2, out error);
            bdl.TryCreateRecordsetValue("c", "field", "Recset4", 3, out error);
            bdl.TryCreateRecordsetValue("1", "field", "Recset4", 4, out error);
            bdl.TryCreateRecordsetValue("zing", "field", "Recset4", 5, out error);
            bdl.TryCreateRecordsetValue("3", "field", "Recset4", 6, out error);

            IDataListCompiler c = DataListFactory.CreateDataListCompiler();
            c.PushBinaryDataList(bdl.UID, bdl, out errors);

            // ExecuteDecisionStack
            string payload = @"{""TheStack"":[{""Col1"":""[[Recset1(*).field]]"",""Col2"":""[[Recset2(*).field]]"",""Col3"":"""",""PopulatedColumnCount"":2,""EvaluationFn"":""IsEqual""}, {""Col1"":""[[Recset3(*).field]]"",""Col2"":""[[Recset4(*).field]]"",""Col3"":"""",""PopulatedColumnCount"":2,""EvaluationFn"":""IsEqual""}],""TotalDecisions"":2,""Mode"":""AND"",""TrueArmText"":null,""FalseArmText"":null}";

            bool result = Dev2DataListDecisionHandler.Instance.ExecuteDecisionStack(payload, new List<string> { bdl.UID.ToString() });


            Assert.IsFalse(result);
        }
        [TestMethod]
        public void CanInvokeDecisionStackSingleDecisionWithOrModeAndTwoRecordsetsAtIndexStarExpectEveryRecordConsidered()
        {
            IBinaryDataList bdl = Dev2BinaryDataListFactory.CreateDataList();

            string error;
            ErrorResultTO errors;

            bdl.TryCreateRecordsetTemplate("Recset", "Test recordset", new List<Dev2Column> { DataListFactory.CreateDev2Column("field", "test field") }, true, out error);
            bdl.TryCreateRecordsetValue("100", "field", "Recset", 1, out error);
            bdl.TryCreateRecordsetValue("200", "field", "Recset", 2, out error);
            bdl.TryCreateRecordsetValue("Charlie Chaplin", "field", "Recset", 3, out error);
            bdl.TryCreateRecordsetValue("400", "field", "Recset", 4, out error);
            bdl.TryCreateRecordsetValue("500", "field", "Recset", 5, out error);
            bdl.TryCreateRecordsetValue("600", "field", "Recset", 6, out error);

            bdl.TryCreateRecordsetTemplate("Recset1", "Test recordset", new List<Dev2Column> { DataListFactory.CreateDev2Column("field", "test field") }, true, out error);
            bdl.TryCreateRecordsetValue("alpha", "field", "Recset1", 1, out error);
            bdl.TryCreateRecordsetValue("beta", "field", "Recset1", 2, out error);
            bdl.TryCreateRecordsetValue("Charlie Chaplin", "field", "Recset1", 3, out error);
            bdl.TryCreateRecordsetValue("delta", "field", "Recset1", 4, out error);
            bdl.TryCreateRecordsetValue("echo", "field", "Recset1", 5, out error);
            bdl.TryCreateRecordsetValue("foxtrot", "field", "Recset1", 6, out error);

            IDataListCompiler c = DataListFactory.CreateDataListCompiler();
            c.PushBinaryDataList(bdl.UID, bdl, out errors);

            // ExecuteDecisionStack
            string payload = @"{""TheStack"":[{""Col1"":""[[Recset(*).field]]"",""Col2"":""[[Recset1(*).field]]"",""Col3"":"""",""PopulatedColumnCount"":2,""EvaluationFn"":""IsEqual""}],""TotalDecisions"":1,""Mode"":""OR"",""TrueArmText"":null,""FalseArmText"":null}";

            bool result = Dev2DataListDecisionHandler.Instance.ExecuteDecisionStack(payload, new List<string> { bdl.UID.ToString() });


            Assert.IsTrue(result);
        }
        [TestMethod]
        public void CanInvokeDecisionStackManyDecisionWithOrModeAndTwoRecordsetsAtIndexStarExpectEveryRecordConsidered()
        {
            IBinaryDataList bdl = Dev2BinaryDataListFactory.CreateDataList();

            string error;
            ErrorResultTO errors;

            bdl.TryCreateRecordsetTemplate("Recset1", "First test recordset", new List<Dev2Column> { DataListFactory.CreateDev2Column("field", "test field") }, true, out error);
            bdl.TryCreateRecordsetValue("a", "field", "Recset1", 1, out error);
            bdl.TryCreateRecordsetValue("b", "field", "Recset1", 2, out error);
            bdl.TryCreateRecordsetValue("c", "field", "Recset1", 3, out error);
            bdl.TryCreateRecordsetValue("d", "field", "Recset1", 4, out error);
            bdl.TryCreateRecordsetValue("e", "field", "Recset1", 5, out error);
            bdl.TryCreateRecordsetValue("f", "field", "Recset1", 6, out error);

            bdl.TryCreateRecordsetTemplate("Recset2", "Second test recordset", new List<Dev2Column> { DataListFactory.CreateDev2Column("field", "test field") }, true, out error);
            bdl.TryCreateRecordsetValue("1", "field", "Recset2", 1, out error);
            bdl.TryCreateRecordsetValue("2", "field", "Recset2", 2, out error);
            bdl.TryCreateRecordsetValue("3", "field", "Recset2", 3, out error);
            bdl.TryCreateRecordsetValue("4", "field", "Recset2", 4, out error);
            bdl.TryCreateRecordsetValue("5", "field", "Recset2", 5, out error);
            bdl.TryCreateRecordsetValue("6", "field", "Recset2", 6, out error);

            bdl.TryCreateRecordsetTemplate("Recset3", "Third test recordset", new List<Dev2Column> { DataListFactory.CreateDev2Column("field", "test field") }, true, out error);
            bdl.TryCreateRecordsetValue("a", "field", "Recset3", 1, out error);
            bdl.TryCreateRecordsetValue("b", "field", "Recset3", 2, out error);
            bdl.TryCreateRecordsetValue("300", "field", "Recset3", 3, out error);
            bdl.TryCreateRecordsetValue("d", "field", "Recset3", 4, out error);
            bdl.TryCreateRecordsetValue("e", "field", "Recset3", 5, out error);
            bdl.TryCreateRecordsetValue("f", "field", "Recset3", 6, out error);

            bdl.TryCreateRecordsetTemplate("Recset4", "Fourth test recordset", new List<Dev2Column> { DataListFactory.CreateDev2Column("field", "test field") }, true, out error);
            bdl.TryCreateRecordsetValue("1", "field", "Recset4", 1, out error);
            bdl.TryCreateRecordsetValue("2", "field", "Recset4", 2, out error);
            bdl.TryCreateRecordsetValue("300", "field", "Recset4", 3, out error);
            bdl.TryCreateRecordsetValue("4", "field", "Recset4", 4, out error);
            bdl.TryCreateRecordsetValue("5", "field", "Recset4", 5, out error);
            bdl.TryCreateRecordsetValue("6", "field", "Recset4", 6, out error);

            IDataListCompiler c = DataListFactory.CreateDataListCompiler();
            c.PushBinaryDataList(bdl.UID, bdl, out errors);

            // ExecuteDecisionStack
            string payload = @"{""TheStack"":[{""Col1"":""[[Recset1(*).field]]"",""Col2"":""[[Recset2(*).field]]"",""Col3"":"""",""PopulatedColumnCount"":2,""EvaluationFn"":""IsEqual""}, {""Col1"":""[[Recset3(*).field]]"",""Col2"":""[[Recset4(*).field]]"",""Col3"":"""",""PopulatedColumnCount"":2,""EvaluationFn"":""IsEqual""}],""TotalDecisions"":2,""Mode"":""OR"",""TrueArmText"":null,""FalseArmText"":null}";

            bool result = Dev2DataListDecisionHandler.Instance.ExecuteDecisionStack(payload, new List<string> { bdl.UID.ToString() });

            Assert.IsTrue(result);
        }
        //Three recordsets all with starred indexes!
        [TestMethod]
        public void CanInvokeDecisionStackSingleDecisionWithANDModeAndThreeRecordsetsAtIndexStarExpectEveryRecordConsidered()
        {
            IBinaryDataList bdl = Dev2BinaryDataListFactory.CreateDataList();

            string error;
            ErrorResultTO errors;

            bdl.TryCreateRecordsetTemplate("Recset", "Test recordset", new List<Dev2Column> { DataListFactory.CreateDev2Column("field", "test field") }, true, out error);
            bdl.TryCreateRecordsetValue("1", "field", "Recset", 1, out error);
            bdl.TryCreateRecordsetValue("2", "field", "Recset", 2, out error);
            bdl.TryCreateRecordsetValue("3", "field", "Recset", 3, out error);
            bdl.TryCreateRecordsetValue("4", "field", "Recset", 4, out error);
            bdl.TryCreateRecordsetValue("5", "field", "Recset", 5, out error);
            bdl.TryCreateRecordsetValue("6", "field", "Recset", 6, out error);

            bdl.TryCreateRecordsetTemplate("Recset2", "Second test recordset", new List<Dev2Column> { DataListFactory.CreateDev2Column("field", "test field") }, true, out error);
            bdl.TryCreateRecordsetValue("0", "field", "Recset2", 1, out error);
            bdl.TryCreateRecordsetValue("1", "field", "Recset2", 2, out error);
            bdl.TryCreateRecordsetValue("2", "field", "Recset2", 3, out error);
            bdl.TryCreateRecordsetValue("3", "field", "Recset2", 4, out error);
            bdl.TryCreateRecordsetValue("4", "field", "Recset2", 5, out error);
            bdl.TryCreateRecordsetValue("5", "field", "Recset2", 6, out error);

            bdl.TryCreateRecordsetTemplate("Recset3", "Third test recordset", new List<Dev2Column> { DataListFactory.CreateDev2Column("field", "test field") }, true, out error);
            bdl.TryCreateRecordsetValue("2", "field", "Recset2", 1, out error);
            bdl.TryCreateRecordsetValue("3", "field", "Recset2", 2, out error);
            bdl.TryCreateRecordsetValue("4", "field", "Recset2", 3, out error);
            bdl.TryCreateRecordsetValue("zingzopwowee", "field", "Recset2", 4, out error);
            bdl.TryCreateRecordsetValue("6", "field", "Recset2", 5, out error);
            bdl.TryCreateRecordsetValue("7", "field", "Recset2", 6, out error);

            IDataListCompiler c = DataListFactory.CreateDataListCompiler();
            c.PushBinaryDataList(bdl.UID, bdl, out errors);

            // ExecuteDecisionStack
            const string payload = @"{""TheStack"":[{""Col1"":""[[Recset(*).field]]"",""Col2"":""[[Recset2(*).field]]"",""Col3"":""[[Recset3(*).field]]"",""PopulatedColumnCount"":3,""EvaluationFn"":""IsBetween""}],""TotalDecisions"":1,""Mode"":""AND"",""TrueArmText"":null,""FalseArmText"":null}";

            bool result = Dev2DataListDecisionHandler.Instance.ExecuteDecisionStack(payload, new List<string> { bdl.UID.ToString() });

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void CanInvokeDecisionStackManyDecisionWithANDModeAndThreeRecordsetsAtIndexStarExpectEveryRecordConsidered()
        {
            IBinaryDataList bdl = Dev2BinaryDataListFactory.CreateDataList();

            string error;
            ErrorResultTO errors;

            bdl.TryCreateRecordsetTemplate("Recset1", "First test recordset", new List<Dev2Column> { DataListFactory.CreateDev2Column("field", "test field") }, true, out error);
            bdl.TryCreateRecordsetValue("1", "field", "Recset1", 1, out error);
            bdl.TryCreateRecordsetValue("2", "field", "Recset1", 2, out error);
            bdl.TryCreateRecordsetValue("3", "field", "Recset1", 3, out error);
            bdl.TryCreateRecordsetValue("4", "field", "Recset1", 4, out error);
            bdl.TryCreateRecordsetValue("5", "field", "Recset1", 5, out error);
            bdl.TryCreateRecordsetValue("6", "field", "Recset1", 6, out error);

            bdl.TryCreateRecordsetTemplate("Recset2", "Second test recordset", new List<Dev2Column> { DataListFactory.CreateDev2Column("field", "test field") }, true, out error);
            bdl.TryCreateRecordsetValue("0", "field", "Recset2", 1, out error);
            bdl.TryCreateRecordsetValue("1", "field", "Recset2", 2, out error);
            bdl.TryCreateRecordsetValue("2", "field", "Recset2", 3, out error);
            bdl.TryCreateRecordsetValue("3", "field", "Recset2", 4, out error);
            bdl.TryCreateRecordsetValue("4", "field", "Recset2", 5, out error);
            bdl.TryCreateRecordsetValue("5", "field", "Recset2", 6, out error);

            bdl.TryCreateRecordsetTemplate("Recset3", "Third test recordset", new List<Dev2Column> { DataListFactory.CreateDev2Column("field", "test field") }, true, out error);
            bdl.TryCreateRecordsetValue("2", "field", "Recset3", 1, out error);
            bdl.TryCreateRecordsetValue("3", "field", "Recset3", 2, out error);
            bdl.TryCreateRecordsetValue("4", "field", "Recset3", 3, out error);
            bdl.TryCreateRecordsetValue("5", "field", "Recset3", 4, out error);
            bdl.TryCreateRecordsetValue("6", "field", "Recset3", 5, out error);
            bdl.TryCreateRecordsetValue("7", "field", "Recset3", 6, out error);

            bdl.TryCreateRecordsetTemplate("Recset4", "Fourth test recordset", new List<Dev2Column> { DataListFactory.CreateDev2Column("field", "test field") }, true, out error);
            bdl.TryCreateRecordsetValue("1", "field", "Recset4", 1, out error);
            bdl.TryCreateRecordsetValue("2", "field", "Recset4", 2, out error);
            bdl.TryCreateRecordsetValue("3", "field", "Recset4", 3, out error);
            bdl.TryCreateRecordsetValue("4", "field", "Recset4", 4, out error);
            bdl.TryCreateRecordsetValue("5", "field", "Recset4", 5, out error);
            bdl.TryCreateRecordsetValue("6", "field", "Recset4", 6, out error);

            bdl.TryCreateRecordsetTemplate("Recset5", "Fifth test recordset", new List<Dev2Column> { DataListFactory.CreateDev2Column("field", "test field") }, true, out error);
            bdl.TryCreateRecordsetValue("0", "field", "Recset5", 1, out error);
            bdl.TryCreateRecordsetValue("1", "field", "Recset5", 2, out error);
            bdl.TryCreateRecordsetValue("2", "field", "Recset5", 3, out error);
            bdl.TryCreateRecordsetValue("3", "field", "Recset5", 4, out error);
            bdl.TryCreateRecordsetValue("4", "field", "Recset5", 5, out error);
            bdl.TryCreateRecordsetValue("5", "field", "Recset5", 6, out error);

            bdl.TryCreateRecordsetTemplate("Recset6", "Sixth test recordset", new List<Dev2Column> { DataListFactory.CreateDev2Column("field", "test field") }, true, out error);
            bdl.TryCreateRecordsetValue("2", "field", "Recset6", 1, out error);
            bdl.TryCreateRecordsetValue("3", "field", "Recset6", 2, out error);
            bdl.TryCreateRecordsetValue("4", "field", "Recset6", 3, out error);
            bdl.TryCreateRecordsetValue("5", "field", "Recset6", 4, out error);
            bdl.TryCreateRecordsetValue("zing", "field", "Recset6", 5, out error);
            bdl.TryCreateRecordsetValue("6", "field", "Recset6", 6, out error);

            IDataListCompiler c = DataListFactory.CreateDataListCompiler();
            c.PushBinaryDataList(bdl.UID, bdl, out errors);

            // ExecuteDecisionStack
            string payload = @"{""TheStack"":[{""Col1"":""[[Recset1(*).field]]"",""Col2"":""[[Recset2(*).field]]"",""Col3"":""[[Recset3(*).field]]"",""PopulatedColumnCount"":3,""EvaluationFn"":""IsBetween""}, {""Col1"":""[[Recset4(*).field]]"",""Col2"":""[[Recset5(*).field]]"",""Col3"":""[[Recset6(*).field]]"",""PopulatedColumnCount"":3,""EvaluationFn"":""IsEqual""}],""TotalDecisions"":2,""Mode"":""AND"",""TrueArmText"":null,""FalseArmText"":null}";

            bool result = Dev2DataListDecisionHandler.Instance.ExecuteDecisionStack(payload, new List<string> { bdl.UID.ToString() });

            Assert.IsFalse(result);
        }
        [TestMethod]
        public void CanInvokeDecisionStackSingleDecisionWithOrModeAndThreeRecordsetsAtIndexStarExpectEveryRecordConsidered()
        {
            IBinaryDataList bdl = Dev2BinaryDataListFactory.CreateDataList();

            string error;
            ErrorResultTO errors;

            bdl.TryCreateRecordsetTemplate("Recset", "Test recordset", new List<Dev2Column> { DataListFactory.CreateDev2Column("field", "test field") }, true, out error);
            bdl.TryCreateRecordsetValue("100", "field", "Recset", 1, out error);
            bdl.TryCreateRecordsetValue("200", "field", "Recset", 2, out error);
            bdl.TryCreateRecordsetValue("300", "field", "Recset", 3, out error);
            bdl.TryCreateRecordsetValue("400", "field", "Recset", 4, out error);
            bdl.TryCreateRecordsetValue("500", "field", "Recset", 5, out error);
            bdl.TryCreateRecordsetValue("600", "field", "Recset", 6, out error);

            bdl.TryCreateRecordsetTemplate("Recset1", "Test recordset", new List<Dev2Column> { DataListFactory.CreateDev2Column("field", "test field") }, true, out error);
            bdl.TryCreateRecordsetValue("alpha", "field", "Recset1", 1, out error);
            bdl.TryCreateRecordsetValue("beta", "field", "Recset1", 2, out error);
            bdl.TryCreateRecordsetValue("299", "field", "Recset1", 3, out error);
            bdl.TryCreateRecordsetValue("delta", "field", "Recset1", 4, out error);
            bdl.TryCreateRecordsetValue("echo", "field", "Recset1", 5, out error);
            bdl.TryCreateRecordsetValue("foxtrot", "field", "Recset1", 6, out error);

            bdl.TryCreateRecordsetTemplate("Recset2", "Test recordset", new List<Dev2Column> { DataListFactory.CreateDev2Column("field", "test field") }, true, out error);
            bdl.TryCreateRecordsetValue("alpha", "field", "Recset2", 1, out error);
            bdl.TryCreateRecordsetValue("beta", "field", "Recset2", 2, out error);
            bdl.TryCreateRecordsetValue("301", "field", "Recset2", 3, out error);
            bdl.TryCreateRecordsetValue("delta", "field", "Recset2", 4, out error);
            bdl.TryCreateRecordsetValue("echo", "field", "Recset2", 5, out error);
            bdl.TryCreateRecordsetValue("foxtrot", "field", "Recset1", 6, out error);

            IDataListCompiler c = DataListFactory.CreateDataListCompiler();
            c.PushBinaryDataList(bdl.UID, bdl, out errors);

            // ExecuteDecisionStack
            string payload = @"{""TheStack"":[{""Col1"":""[[Recset(*).field]]"",""Col2"":""[[Recset1(*).field]]"",""Col3"":""[[Recset2(*).field]]"",""PopulatedColumnCount"":3,""EvaluationFn"":""IsBetween""}],""TotalDecisions"":1,""Mode"":""OR"",""TrueArmText"":null,""FalseArmText"":null}";

            bool result = Dev2DataListDecisionHandler.Instance.ExecuteDecisionStack(payload, new List<string> { bdl.UID.ToString() });

            Assert.IsTrue(result);
        }
        [TestMethod]
        public void CanInvokeDecisionStackManyDecisionWithOrModeAndThreeRecordsetsAtIndexStarExpectEveryRecordConsidered()
        {
            IBinaryDataList bdl = Dev2BinaryDataListFactory.CreateDataList();

            string error;
            ErrorResultTO errors;

            bdl.TryCreateRecordsetTemplate("Recset1", "First test recordset", new List<Dev2Column> { DataListFactory.CreateDev2Column("field", "test field") }, true, out error);
            bdl.TryCreateRecordsetValue("a", "field", "Recset1", 1, out error);
            bdl.TryCreateRecordsetValue("b", "field", "Recset1", 2, out error);
            bdl.TryCreateRecordsetValue("c", "field", "Recset1", 3, out error);
            bdl.TryCreateRecordsetValue("d", "field", "Recset1", 4, out error);
            bdl.TryCreateRecordsetValue("e", "field", "Recset1", 5, out error);
            bdl.TryCreateRecordsetValue("f", "field", "Recset1", 6, out error);

            bdl.TryCreateRecordsetTemplate("Recset2", "Second test recordset", new List<Dev2Column> { DataListFactory.CreateDev2Column("field", "test field") }, true, out error);
            bdl.TryCreateRecordsetValue("a", "field", "Recset2", 1, out error);
            bdl.TryCreateRecordsetValue("b", "field", "Recset2", 2, out error);
            bdl.TryCreateRecordsetValue("c", "field", "Recset2", 3, out error);
            bdl.TryCreateRecordsetValue("d", "field", "Recset2", 4, out error);
            bdl.TryCreateRecordsetValue("e", "field", "Recset2", 5, out error);
            bdl.TryCreateRecordsetValue("f", "field", "Recset2", 6, out error);

            bdl.TryCreateRecordsetTemplate("Recset3", "Third test recordset", new List<Dev2Column> { DataListFactory.CreateDev2Column("field", "test field") }, true, out error);
            bdl.TryCreateRecordsetValue("a", "field", "Recset3", 1, out error);
            bdl.TryCreateRecordsetValue("b", "field", "Recset3", 2, out error);
            bdl.TryCreateRecordsetValue("c", "field", "Recset3", 3, out error);
            bdl.TryCreateRecordsetValue("d", "field", "Recset3", 4, out error);
            bdl.TryCreateRecordsetValue("e", "field", "Recset3", 5, out error);
            bdl.TryCreateRecordsetValue("f", "field", "Recset3", 6, out error);

            bdl.TryCreateRecordsetTemplate("Recset4", "Fourth test recordset", new List<Dev2Column> { DataListFactory.CreateDev2Column("field", "test field") }, true, out error);
            bdl.TryCreateRecordsetValue("1", "field", "Recset4", 1, out error);
            bdl.TryCreateRecordsetValue("2", "field", "Recset4", 2, out error);
            bdl.TryCreateRecordsetValue("3", "field", "Recset4", 3, out error);
            bdl.TryCreateRecordsetValue("4", "field", "Recset4", 4, out error);
            bdl.TryCreateRecordsetValue("5", "field", "Recset4", 5, out error);
            bdl.TryCreateRecordsetValue("6", "field", "Recset4", 6, out error);

            bdl.TryCreateRecordsetTemplate("Recset5", "Fifth test recordset", new List<Dev2Column> { DataListFactory.CreateDev2Column("field", "test field") }, true, out error);
            bdl.TryCreateRecordsetValue("1", "field", "Recset5", 1, out error);
            bdl.TryCreateRecordsetValue("2", "field", "Recset5", 2, out error);
            bdl.TryCreateRecordsetValue("2", "field", "Recset5", 3, out error);
            bdl.TryCreateRecordsetValue("4", "field", "Recset5", 4, out error);
            bdl.TryCreateRecordsetValue("5", "field", "Recset5", 5, out error);
            bdl.TryCreateRecordsetValue("6", "field", "Recset5", 6, out error);

            bdl.TryCreateRecordsetTemplate("Recset6", "Sixth test recordset", new List<Dev2Column> { DataListFactory.CreateDev2Column("field", "test field") }, true, out error);
            bdl.TryCreateRecordsetValue("1", "field", "Recset6", 1, out error);
            bdl.TryCreateRecordsetValue("2", "field", "Recset6", 2, out error);
            bdl.TryCreateRecordsetValue("4", "field", "Recset6", 3, out error);
            bdl.TryCreateRecordsetValue("4", "field", "Recset6", 4, out error);
            bdl.TryCreateRecordsetValue("5", "field", "Recset6", 5, out error);
            bdl.TryCreateRecordsetValue("6", "field", "Recset6", 6, out error);

            IDataListCompiler c = DataListFactory.CreateDataListCompiler();
            c.PushBinaryDataList(bdl.UID, bdl, out errors);

            // ExecuteDecisionStack
            string payload = @"{""TheStack"":[{""Col1"":""[[Recset1(*).field]]"",""Col2"":""[[Recset2(*).field]]"",""Col3"":""[[Recset3(*).field]]"",""PopulatedColumnCount"":3,""EvaluationFn"":""IsBetween""}, {""Col1"":""[[Recset4(*).field]]"",""Col2"":""[[Recset5(*).field]]"",""Col3"":""[[Recset6(*).field]]"",""PopulatedColumnCount"":3,""EvaluationFn"":""IsBetween""}],""TotalDecisions"":2,""Mode"":""OR"",""TrueArmText"":null,""FalseArmText"":null}";

            bool result = Dev2DataListDecisionHandler.Instance.ExecuteDecisionStack(payload, new List<string> { bdl.UID.ToString() });

            Assert.IsTrue(result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("Dev2DecisionStack_Evaluate")]
        public void Dev2DecisionStack_Evaluate_FilePathEvaulatedData_Expected_ResultEqualsTrue()
        {
            //------------Setup for test--------------------------

            IBinaryDataList bdl = Dev2BinaryDataListFactory.CreateDataList();

            string error;
            ErrorResultTO errors;

            bdl.TryCreateRecordsetTemplate("Recset", "Test recordset", new List<Dev2Column> { DataListFactory.CreateDev2Column("field", "test field") }, true, out error);           

            // add data to Recset
            bdl.TryCreateRecordsetValue(@"C:\Temp\PathOperationsTestFolder\OldFolder\OldFolderFirstInnerFolder\TextFile1.txt", "field", "Recset", 1, out error);          

            IDataListCompiler c = DataListFactory.CreateDataListCompiler();
            c.PushBinaryDataList(bdl.UID, bdl, out errors);

            // ExecuteDecisionStack
            string payload = @"{""TheStack"":[{""Col1"":""C:\\Temp\\PathOperationsTestFolder\\OldFolder\\OldFolderFirstInnerFolder\\TextFile1.txt"",""Col2"":""[[Recset(1).field]]"",""Col3"":"""",""PopulatedColumnCount"":2,""EvaluationFn"":""IsEqual""}],""TotalDecisions"":1,""Mode"":""AND"",""TrueArmText"":null,""FalseArmText"":null}";

            //------------Execute Test---------------------------
            bool result = Dev2DataListDecisionHandler.Instance.ExecuteDecisionStack(payload, new List<string> { bdl.UID.ToString() });

            //------------Assert Results-------------------------
            Assert.IsTrue(result);

        }

        #endregion

    }
}
