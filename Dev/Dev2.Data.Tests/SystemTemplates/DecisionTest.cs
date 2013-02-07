using System;
using System.Collections.Generic;
using Dev2.Data.Decision;
using Dev2.Data.Decisions.Operations;
using Dev2.DataList.Contract.Binary_Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Data.SystemTemplates.Models;
using Dev2.DataList.Contract;
using Dev2.Common;

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
            Dev2DecisionStack dds = new Dev2DecisionStack() { TheStack = new List<Dev2Decision>(), Mode = Dev2DecisionMode.OR, FalseArmText = "False", TrueArmText = "True"};
            IDataListCompiler dlc = DataListFactory.CreateDataListCompiler();

            string result = dlc.ConvertModelToJson(dds);
            string expected = @"{""TheStack"":[],""TotalDecisions"":0,""ModelName"":""Dev2DecisionStack"",""Mode"":""OR"",""TrueArmText"":""True"",""FalseArmText"":""False""}";

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
            Dev2DecisionStack dds = new Dev2DecisionStack() { TheStack = new List<Dev2Decision>(), Mode = Dev2DecisionMode.OR };
            IDataListCompiler dlc = DataListFactory.CreateDataListCompiler();

            dds.AddModelItem(new Dev2Decision(){Col1 = "[[A]]", Col2 = string.Empty, Col3 = string.Empty, EvaluationFn = enDecisionType.IsNumeric});

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

            Assert.AreEqual(1,dds.TotalDecisions);
        }

        #endregion

        #region Execution Test
        // Sashen: 31-01-2012 : No expected outcome from the test - FIXED

        /// <summary>
        /// Travis.Frisinger - Can push a system model into the Data List
        /// </summary>
        [TestMethod]
        public void CanPushModel_To_DataList_Expect_ValidModel()
        {
            Dev2DecisionStack dds = new Dev2DecisionStack() { TheStack = new List<Dev2Decision>(), Mode = Dev2DecisionMode.OR };
            IDataListCompiler dlc = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors = new ErrorResultTO();

            dds.AddModelItem(new Dev2Decision() { Col1 = "[[A]]", Col2 = string.Empty, Col3 = string.Empty, EvaluationFn = enDecisionType.IsNumeric });

            Guid id = dlc.PushSystemModelToDataList(dds, out errors);
            
            Assert.AreEqual(0, errors.FetchErrors().Count);

            string result = dlc.EvaluateSystemEntry(id, enSystemTag.SystemModel, out errors);

            string expected = @"{""TheStack"":[{""Col1"":""[[A]]"",""Col2"":"""",""Col3"":"""",""PopulatedColumnCount"":1,""EvaluationFn"":""IsNumeric""}],""TotalDecisions"":1,""ModelName"":""Dev2DecisionStack"",""Mode"":""OR"",""TrueArmText"":null,""FalseArmText"":null}";

            Assert.AreEqual(expected, result);

        }


        /// <summary>
        /// Travis.Frisinger - Can execute a decision stack with mult decisions
        /// </summary>
        [TestMethod]
        public void CanPushModelWithDecisionStack_To_DataList_Expect_ValidModel()
        {
            Dev2DecisionStack dds = new Dev2DecisionStack() { TheStack = new List<Dev2Decision>(), Mode = Dev2DecisionMode.OR };
            IDataListCompiler dlc = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors = new ErrorResultTO();

            dds.AddModelItem(new Dev2Decision() { Col1 = "[[A]]", Col2 = string.Empty, Col3 = string.Empty, EvaluationFn = enDecisionType.IsNumeric });
            dds.AddModelItem(new Dev2Decision() { Col1 = "[[B]]", Col2 = string.Empty, Col3 = string.Empty, EvaluationFn = enDecisionType.IsNotNumeric });

            Guid id = dlc.PushSystemModelToDataList(dds, out errors);

            Assert.AreEqual(0, errors.FetchErrors().Count);

            string result = dlc.EvaluateSystemEntry(id, enSystemTag.SystemModel, out errors);

            string expected = @"{""TheStack"":[{""Col1"":""[[A]]"",""Col2"":"""",""Col3"":"""",""PopulatedColumnCount"":1,""EvaluationFn"":""IsNumeric""},{""Col1"":""[[B]]"",""Col2"":"""",""Col3"":"""",""PopulatedColumnCount"":1,""EvaluationFn"":""IsNotNumeric""}],""TotalDecisions"":2,""ModelName"":""Dev2DecisionStack"",""Mode"":""OR"",""TrueArmText"":null,""FalseArmText"":null}";

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

            bool result = Dev2DataListDecisionHandler.Instance.ExecuteDecisionStack(payload, new List<string>(){bdl.UID.ToString()});


            c.DeleteDataListByID(bdl.UID); // clean up ;)

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

            string result = Dev2DataListDecisionHandler.Instance.FetchSwitchData("[[A]]", new List<string>() { bdl.UID.ToString() });
            string expected = "1";

            c.DeleteDataListByID(bdl.UID); // clean up ;)

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

            string result = Dev2DataListDecisionHandler.Instance.FetchSwitchData("[[A]]", new List<string>() { GlobalConstants.NullDataListID.ToString() });
            string expected = "";

            c.DeleteDataListByID(bdl.UID); // clean up ;)

            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// Travis.Frisinger - Will the execution of a decision stack fail with an invalid DataList ID
        [TestMethod]
        // [ExpectedException(InvalidExpressionException)] - Test will not import name space keeps giving error
        public void CanInvokeDecisionStack_SingleDecision_NullDataListID_Expect_Exception()
        {
            IBinaryDataList bdl = Dev2BinaryDataListFactory.CreateDataList();

            // ExecuteDecisionStack
            string payload = @"{""TheStack"":[{""Col1"":""[[A]]"",""Col2"":"""",""Col3"":"""",""PopulatedColumnCount"":1,""EvaluationFn"":""IsNumeric""}],""TotalDecisions"":1,""Mode"":""OR"",""TrueArmText"":null,""FalseArmText"":null}";

            try
            {
                bool result = Dev2DataListDecisionHandler.Instance.ExecuteDecisionStack(payload, new List<string>() { GlobalConstants.NullDataListID.ToString() });

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

            bool result = Dev2DataListDecisionHandler.Instance.ExecuteDecisionStack(payload, new List<string>() { bdl.UID.ToString() });

            c.DeleteDataListByID(bdl.UID); // clean up ;)

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

            bool result = Dev2DataListDecisionHandler.Instance.ExecuteDecisionStack(payload, new List<string>() { bdl.UID.ToString() });

            c.DeleteDataListByID(bdl.UID); // clean up ;)

            Assert.IsFalse(result);
        }

        /// <summary>
        /// Travis.Frisinger - Can it convert a system model into a web model correctly
        /// </summary>
        [TestMethod]
        public void CanConvert_SystemModel_Into_WebModel_Expect_ValidModel()
        {

            Dev2DecisionStack dds = new Dev2DecisionStack() { TheStack = new List<Dev2Decision>(), Mode = Dev2DecisionMode.OR };
            IDataListCompiler dlc = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors = new ErrorResultTO();

            dds.AddModelItem(new Dev2Decision() { Col1 = "[[A]]", Col2 = string.Empty, Col3 = string.Empty, EvaluationFn = enDecisionType.IsNumeric });

            Guid id = dlc.PushSystemModelToDataList(dds, out errors);

            string result = dlc.FetchSystemModelAsWebModel<Dev2DecisionStack>(id, out errors);

            Assert.AreEqual(0, errors.FetchErrors().Count);

            string expected = @"{""TheStack"":[{""Col1"":""[[A]]"",""Col2"":"""",""Col3"":"""",""PopulatedColumnCount"":1,""EvaluationFn"":""IsNumeric""}],""TotalDecisions"":1,""ModelName"":""Dev2DecisionStack"",""Mode"":""OR"",""TrueArmText"":null,""FalseArmText"":null}";

            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// Travis.Frisinger - Can it convert a decision stack with a null list of decisions into a web model correctly
        /// </summary>
        [TestMethod]
        public void CanConvert_SystemModel_Into_WebModel_WithNullDecision_Expect_ValidModel()
        {

            Dev2DecisionStack dds = new Dev2DecisionStack() { TheStack = new List<Dev2Decision>(), Mode = Dev2DecisionMode.OR };
            IDataListCompiler dlc = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors = new ErrorResultTO();

            dds.AddModelItem(null);

            Guid id = dlc.PushSystemModelToDataList(dds, out errors);

            string result = dlc.FetchSystemModelAsWebModel<Dev2DecisionStack>(id, out errors);

            Assert.AreEqual(0, errors.FetchErrors().Count);

            string expected = @"{""TheStack"":[null],""TotalDecisions"":1,""ModelName"":""Dev2DecisionStack"",""Mode"":""OR"",""TrueArmText"":null,""FalseArmText"":null}";

            Assert.AreEqual(expected, result);
        }

        #endregion

    }
}
