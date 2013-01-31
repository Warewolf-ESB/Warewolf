using System;
using System.Collections.Generic;
using Dev2.Data.Decision;
using Dev2.Data.Decisions.Operations;
using Dev2.DataList.Contract.Binary_Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Data.SystemTemplates.Models;
using Dev2.DataList.Contract;

namespace Dev2.Data.Tests.SystemTemplates
{
    [TestClass]
    public class DecisionTest
    {

        #region Model Test
        [TestMethod]
        public void CanBootStrapModel_To_JSON()
        {
            Dev2DecisionStack dds = new Dev2DecisionStack() { TheStack = new List<Dev2Decision>(), Mode = Dev2DecisionMode.OR, FalseArmText = "False", TrueArmText = "True"};
            IDataListCompiler dlc = DataListFactory.CreateDataListCompiler();

            string result = dlc.ConvertModelToJson(dds);
            string expected = @"{""TheStack"":[],""TotalDecisions"":0,""ModelName"":""Dev2DecisionStack"",""Mode"":""OR"",""TrueArmText"":""True"",""FalseArmText"":""False""}";

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void CanConvertJSON_To_Model()
        {
            Dev2DecisionStack dds;
            IDataListCompiler dlc = DataListFactory.CreateDataListCompiler();

            string payload = @"{""TheStack"":[],""TotalDecisions"":0,""Version"":""1.0.0"",""ModelName"":""Dev2DecisionStack""}";


            dds = dlc.ConvertFromJsonToModel<Dev2DecisionStack>(payload);

            Assert.AreEqual(0, dds.TotalDecisions);

        }


        [TestMethod]
        public void CanAddModelItem_And_Convert_ToAndFrom()
        {
            Dev2DecisionStack dds = new Dev2DecisionStack() { TheStack = new List<Dev2Decision>(), Mode = Dev2DecisionMode.OR };
            IDataListCompiler dlc = DataListFactory.CreateDataListCompiler();

            dds.AddModelItem(new Dev2Decision(){Col1 = "[[A]]", Col2 = string.Empty, Col3 = string.Empty, EvaluationFn = enDecisionType.IsNumeric});

            string result = dlc.ConvertModelToJson(dds);

            Dev2DecisionStack convertedResult = dlc.ConvertFromJsonToModel<Dev2DecisionStack>(result);


            Assert.AreEqual(1, convertedResult.TotalDecisions);
            Assert.AreEqual(Dev2DecisionMode.OR, convertedResult.Mode);

        }

        #endregion

        #region Execution Test
        [TestMethod]
        public void CanPushModel_To_DataList()
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

        
        [TestMethod]
        public void CanConvert_SystemModel_Into_WebModel()
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

        #endregion

    }
}
