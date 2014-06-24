using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Dev2.Common;
using Dev2.Data.Factories;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DynamicServices.Test;
using Dev2.Server.Datalist;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
namespace Dev2.Data.Tests.BinaryDataList
{
    /// <summary>
    /// Summary description for ServerDataListCompilerTest
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class ServerDataListCompilerTest
    {
        private readonly IEnvironmentModelDataListCompiler _sdlc = DataListFactory.CreateServerDataListCompiler();

        const string _dataListWellformed = "<DataList><scalar1/><rs1><f1/><f2/></rs1><scalar2/></DataList>";
        const string _dataListWellformedData = "<DataList><scalar1>1</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1><scalar2/></DataList>";

        private static readonly DataListFormat xmlFormat = DataListFormat.CreateFormat(GlobalConstants._XML);


        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Positive Evaluate Test

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ServerDataListCompiler_UpsertWithDebug")]
        public void ServerDataListCompiler_UpsertWithDebug_WhenDebugActiveIndexOfStar_ExpectCorrectExpressionAuditData()
        {
            //------------Setup for test--------------------------
            ErrorResultTO errors;
            const string dl = "<DataList><idx/><rec><a/></rec><rec2><b/></rec2></DataList>";
            const string dlData = "<DataList><rec><a>1</a></rec><rec><a>2</a></rec><rec><a>3</a></rec></DataList>";

            byte[] data = (TestHelper.ConvertStringToByteArray(dlData));
            Guid dlID = _sdlc.ConvertTo(null, xmlFormat, data, dl, out errors);

            var payload = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder(false);
            payload.IsDebug = true;
            payload.Add("[[rec2().b]]", "[[rec(*).a]]");


            //------------Execute Test---------------------------
            Guid result = _sdlc.Upsert(null, dlID, payload, out errors);

            //------------Assert Results-------------------------
            var debugItems = payload.DebugOutputs;
            Assert.AreEqual(dlID, result);
            Assert.IsNotNull(debugItems);
            Assert.AreEqual(1, debugItems.Count);

            var item = debugItems[0];
            var leftAudit = item.LeftEntry.ComplexExpressionAuditor;
            var rightAudit = item.RightEntry.ComplexExpressionAuditor;
            var leftAuditItems = leftAudit.FetchAuditItems();
            var rightAuditItems = rightAudit.FetchAuditItems();

            StringAssert.Contains(item.Expression, "[[rec2(1).b]]");
            Assert.AreEqual(1, item.UsedRecordsetIndex);
            Assert.AreEqual(1, leftAuditItems.Count);
            Assert.AreEqual(3, rightAuditItems.Count);
            StringAssert.Contains(leftAuditItems[0].Expression, "[[rec2().b]]");
            StringAssert.Contains(leftAuditItems[0].RawExpression, "[[rec2().b]]");
            StringAssert.Contains(rightAuditItems[0].Expression, "[[rec(*).a]]");
            StringAssert.Contains(rightAuditItems[0].RawExpression, "[[rec(1).a]]");
            StringAssert.Contains(rightAuditItems[0].BoundValue, "1");
            StringAssert.Contains(rightAuditItems[1].Expression, "[[rec(*).a]]");
            StringAssert.Contains(rightAuditItems[1].RawExpression, "[[rec(2).a]]");
            StringAssert.Contains(rightAuditItems[1].BoundValue, "2");
            StringAssert.Contains(rightAuditItems[2].Expression, "[[rec(*).a]]");
            StringAssert.Contains(rightAuditItems[2].RawExpression, "[[rec(3).a]]");
            StringAssert.Contains(rightAuditItems[2].BoundValue, "3");
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ServerDataListCompiler_UpsertWithDebug")]
        public void ServerDataListCompiler_UpsertWithDebug_WhenDebugActiveAndEvaluatedIndexOfStarInFromExpression_ExpectCorrectExpressionAuditData()
        {
            //------------Setup for test--------------------------
            ErrorResultTO errors;
            const string dl = "<DataList><idx/><rec><a/></rec><rec2><b/></rec2></DataList>";
            const string dlData = "<DataList><idx>*</idx><rec><a>1</a></rec><rec><a>2</a></rec><rec><a>3</a></rec></DataList>";

            byte[] data = (TestHelper.ConvertStringToByteArray(dlData));
            Guid dlID = _sdlc.ConvertTo(null, xmlFormat, data, dl, out errors);

            var payload = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder(false);
            payload.IsDebug = true;
            payload.Add("[[rec2().b]]", "[[rec([[idx]]).a]]");


            //------------Execute Test---------------------------
            Guid result = _sdlc.Upsert(null, dlID, payload, out errors);

            //------------Assert Results-------------------------
            var debugItems = payload.DebugOutputs;
            Assert.AreEqual(dlID, result);
            Assert.IsNotNull(debugItems);
            Assert.AreEqual(1, debugItems.Count);

            var item = debugItems[0];
            var leftAudit = item.LeftEntry.ComplexExpressionAuditor;
            var rightAudit = item.RightEntry.ComplexExpressionAuditor;
            var leftAuditItems = leftAudit.FetchAuditItems();
            var rightAuditItems = rightAudit.FetchAuditItems();

            StringAssert.Contains(item.Expression, "[[rec2(1).b]]");
            Assert.AreEqual(1, item.UsedRecordsetIndex);
            Assert.AreEqual(1, leftAuditItems.Count);
            Assert.AreEqual(3, rightAuditItems.Count);
            StringAssert.Contains(leftAuditItems[0].Expression, "[[rec2().b]]");
            StringAssert.Contains(leftAuditItems[0].RawExpression, "[[rec2().b]]");
            StringAssert.Contains(rightAuditItems[0].Expression, "[[rec(*).a]]");
            StringAssert.Contains(rightAuditItems[0].RawExpression, "[[rec(1).a]]");
            StringAssert.Contains(rightAuditItems[0].BoundValue, "1");
            StringAssert.Contains(rightAuditItems[1].Expression, "[[rec(*).a]]");
            StringAssert.Contains(rightAuditItems[1].RawExpression, "[[rec(2).a]]");
            StringAssert.Contains(rightAuditItems[1].BoundValue, "2");
            StringAssert.Contains(rightAuditItems[2].Expression, "[[rec(*).a]]");
            StringAssert.Contains(rightAuditItems[2].RawExpression, "[[rec(3).a]]");
            StringAssert.Contains(rightAuditItems[2].BoundValue, "3");
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ServerDataListCompiler_UpsertWithDebug")]
        public void ServerDataListCompiler_UpsertWithDebug_WhenDebugActiveAndEvaluatedIndexOfNumberInFromExpression_ExpectCorrectExpressionAuditData()
        {
            //------------Setup for test--------------------------
            ErrorResultTO errors;
            const string dl = "<DataList><idx/><rec><a/></rec><rec2><b/></rec2></DataList>";
            const string dlData = "<DataList><idx>1</idx><rec><a>1</a></rec><rec><a>2</a></rec><rec><a>3</a></rec></DataList>";

            byte[] data = (TestHelper.ConvertStringToByteArray(dlData));
            Guid dlID = _sdlc.ConvertTo(null, xmlFormat, data, dl, out errors);

            var payload = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder(false);
            payload.IsDebug = true;
            payload.Add("[[rec2().b]]", "[[rec([[idx]]).a]]");


            //------------Execute Test---------------------------
            Guid result = _sdlc.Upsert(null, dlID, payload, out errors);

            //------------Assert Results-------------------------
            var debugItems = payload.DebugOutputs;
            Assert.AreEqual(dlID, result);
            Assert.IsNotNull(debugItems);
            Assert.AreEqual(1, debugItems.Count);

            var item = debugItems[0];
            var leftAudit = item.LeftEntry.ComplexExpressionAuditor;
            var rightAudit = item.RightEntry.ComplexExpressionAuditor;
            var leftAuditItems = leftAudit.FetchAuditItems();
            var rightAuditItems = rightAudit.FetchAuditItems();

            StringAssert.Contains(item.Expression, "[[rec2(1).b]]");
            Assert.AreEqual(1, item.UsedRecordsetIndex);
            Assert.AreEqual(1, leftAuditItems.Count);
            Assert.AreEqual(1, rightAuditItems.Count);
            StringAssert.Contains(leftAuditItems[0].Expression, "[[rec2().b]]");
            StringAssert.Contains(leftAuditItems[0].RawExpression, "[[rec2().b]]");
            StringAssert.Contains(rightAuditItems[0].Expression, "[[rec(1).a]]");
            StringAssert.Contains(rightAuditItems[0].RawExpression, "[[rec(1).a]]");
            StringAssert.Contains(rightAuditItems[0].BoundValue, "1");
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ServerDataListCompiler_UpsertWithDebug")]
        public void ServerDataListCompiler_UpsertWithDebug_WhenDebugActiveAndEvaluatedIndexOfBlankInFromExpression_ExpectCorrectExpressionAuditData()
        {
            //------------Setup for test--------------------------
            ErrorResultTO errors;
            const string dl = "<DataList><idx/><rec><a/></rec><rec2><b/></rec2></DataList>";
            const string dlData = "<DataList><idx></idx><rec><a>1</a></rec><rec><a>2</a></rec><rec><a>3</a></rec></DataList>";

            byte[] data = (TestHelper.ConvertStringToByteArray(dlData));
            Guid dlID = _sdlc.ConvertTo(null, xmlFormat, data, dl, out errors);

            var payload = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder(false);
            payload.IsDebug = true;
            payload.Add("[[rec2().b]]", "[[rec([[idx]]).a]]");


            //------------Execute Test---------------------------
            Guid result = _sdlc.Upsert(null, dlID, payload, out errors);

            //------------Assert Results-------------------------
            var debugItems = payload.DebugOutputs;
            Assert.AreEqual(dlID, result);
            Assert.IsNotNull(debugItems);
            Assert.AreEqual(1, debugItems.Count);

            var item = debugItems[0];
            var leftAudit = item.LeftEntry.ComplexExpressionAuditor;
            var rightAudit = item.RightEntry.ComplexExpressionAuditor;
            var leftAuditItems = leftAudit.FetchAuditItems();
            var rightAuditItems = rightAudit.FetchAuditItems();

            StringAssert.Contains(item.Expression, "[[rec2(1).b]]");
            Assert.AreEqual(1, item.UsedRecordsetIndex);
            Assert.AreEqual(1, leftAuditItems.Count);
            Assert.AreEqual(1, rightAuditItems.Count);
            StringAssert.Contains(leftAuditItems[0].Expression, "[[rec2().b]]");
            StringAssert.Contains(leftAuditItems[0].RawExpression, "[[rec2().b]]");
            StringAssert.Contains(rightAuditItems[0].Expression, "[[rec().a]]");
            StringAssert.Contains(rightAuditItems[0].RawExpression, "[[rec().a]]");
            StringAssert.Contains(rightAuditItems[0].BoundValue, "3");
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ServerDataListCompiler_UpsertWithDebug")]
        public void ServerDataListCompiler_UpsertWithDebug_WhenDebugActiveAndEvaluatedIndexOfStarInToExpression_ExpectCorrectExpressionAuditData()
        {
            //------------Setup for test--------------------------
            ErrorResultTO errors;
            const string dl = "<DataList><idx/><rec><a/></rec><rec2><b/></rec2></DataList>";
            const string dlData = "<DataList><idx>*</idx><rec><a>1</a></rec><rec><a>2</a></rec><rec><a>3</a></rec></DataList>";

            byte[] data = (TestHelper.ConvertStringToByteArray(dlData));
            Guid dlID = _sdlc.ConvertTo(null, xmlFormat, data, dl, out errors);

            var payload = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder(false);
            payload.IsDebug = true;
            payload.Add("[[rec2([[idx]]).b]]", "[[rec(*).a]]");


            //------------Execute Test---------------------------
            Guid result = _sdlc.Upsert(null, dlID, payload, out errors);

            //------------Assert Results-------------------------
            var debugItems = payload.DebugOutputs;
            Assert.AreEqual(dlID, result);
            Assert.IsNotNull(debugItems);
            Assert.AreEqual(1, debugItems.Count);

            var item = debugItems[0];
            var leftAudit = item.LeftEntry.ComplexExpressionAuditor;
            var rightAudit = item.RightEntry.ComplexExpressionAuditor;
            var leftAuditItems = leftAudit.FetchAuditItems();
            var rightAuditItems = rightAudit.FetchAuditItems();

            Assert.IsNull(item.Expression);
            Assert.AreEqual(1, item.UsedRecordsetIndex);
            Assert.AreEqual(1, leftAuditItems.Count);
            Assert.AreEqual(3, rightAuditItems.Count);
            StringAssert.Contains(leftAuditItems[0].Expression, "[[rec2(*).b]]");
            StringAssert.Contains(leftAuditItems[0].RawExpression, "[[rec2(1).b]]");
            StringAssert.Contains(rightAuditItems[0].Expression, "[[rec(*).a]]");
            StringAssert.Contains(rightAuditItems[0].RawExpression, "[[rec(1).a]]");
            StringAssert.Contains(rightAuditItems[0].BoundValue, "1");
            StringAssert.Contains(rightAuditItems[1].Expression, "[[rec(*).a]]");
            StringAssert.Contains(rightAuditItems[1].RawExpression, "[[rec(2).a]]");
            StringAssert.Contains(rightAuditItems[1].BoundValue, "2");
            StringAssert.Contains(rightAuditItems[2].Expression, "[[rec(*).a]]");
            StringAssert.Contains(rightAuditItems[2].RawExpression, "[[rec(3).a]]");
            StringAssert.Contains(rightAuditItems[2].BoundValue, "3");
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ServerDataListCompiler_UpsertWithDebug")]
        public void ServerDataListCompiler_UpsertWithDebug_WhenDebugActiveAndEvaluatedIndexOfStarInToAndFromExpression_ExpectCorrectExpressionAuditData()
        {
            //------------Setup for test--------------------------
            ErrorResultTO errors;
            const string dl = "<DataList><idx/><rec><a/></rec><rec2><b/></rec2></DataList>";
            const string dlData = "<DataList><idx>*</idx><rec><a>1</a></rec><rec><a>2</a></rec><rec><a>3</a></rec></DataList>";

            byte[] data = (TestHelper.ConvertStringToByteArray(dlData));
            Guid dlID = _sdlc.ConvertTo(null, xmlFormat, data, dl, out errors);

            var payload = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder(false);
            payload.IsDebug = true;
            payload.Add("[[rec2([[idx]]).b]]", "[[rec([[idx]]).a]]");


            //------------Execute Test---------------------------
            Guid result = _sdlc.Upsert(null, dlID, payload, out errors);

            //------------Assert Results-------------------------
            var debugItems = payload.DebugOutputs;
            Assert.AreEqual(dlID, result);
            Assert.IsNotNull(debugItems);
            Assert.AreEqual(1, debugItems.Count);

            var item = debugItems[0];
            var leftAudit = item.LeftEntry.ComplexExpressionAuditor;
            var rightAudit = item.RightEntry.ComplexExpressionAuditor;
            var leftAuditItems = leftAudit.FetchAuditItems();
            var rightAuditItems = rightAudit.FetchAuditItems();

            Assert.IsNull(item.Expression);
            Assert.AreEqual(1, item.UsedRecordsetIndex);
            Assert.AreEqual(1, leftAuditItems.Count);
            Assert.AreEqual(3, rightAuditItems.Count);
            StringAssert.Contains(leftAuditItems[0].Expression, "[[rec2(*).b]]");
            StringAssert.Contains(leftAuditItems[0].RawExpression, "[[rec2(1).b]]");
            StringAssert.Contains(rightAuditItems[0].Expression, "[[rec(*).a]]");
            StringAssert.Contains(rightAuditItems[0].RawExpression, "[[rec(1).a]]");
            StringAssert.Contains(rightAuditItems[0].BoundValue, "1");
            StringAssert.Contains(rightAuditItems[1].Expression, "[[rec(*).a]]");
            StringAssert.Contains(rightAuditItems[1].RawExpression, "[[rec(2).a]]");
            StringAssert.Contains(rightAuditItems[1].BoundValue, "2");
            StringAssert.Contains(rightAuditItems[2].Expression, "[[rec(*).a]]");
            StringAssert.Contains(rightAuditItems[2].RawExpression, "[[rec(3).a]]");
            StringAssert.Contains(rightAuditItems[2].BoundValue, "3");
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ServerDataListCompiler_UpsertWithDebug")]
        public void ServerDataListCompiler_UpsertWithDebug_WhenDebugActiveAndEvaluatedIndexOfNumberInToExpression_ExpectCorrectExpressionAuditData()
        {
            //------------Setup for test--------------------------
            ErrorResultTO errors;
            const string dl = "<DataList><idx/><rec><a/></rec><rec2><b/></rec2></DataList>";
            const string dlData = "<DataList><idx>1</idx><rec><a>1</a></rec><rec><a>2</a></rec><rec><a>3</a></rec></DataList>";

            byte[] data = (TestHelper.ConvertStringToByteArray(dlData));
            Guid dlID = _sdlc.ConvertTo(null, xmlFormat, data, dl, out errors);

            var payload = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder(false);
            payload.IsDebug = true;
            payload.Add("[[rec2([[idx]]).b]]", "[[rec(*).a]]");


            //------------Execute Test---------------------------
            Guid result = _sdlc.Upsert(null, dlID, payload, out errors);

            //------------Assert Results-------------------------
            var debugItems = payload.DebugOutputs;
            Assert.AreEqual(dlID, result);
            Assert.IsNotNull(debugItems);
            Assert.AreEqual(1, debugItems.Count);

            var item = debugItems[0];
            var leftAudit = item.LeftEntry.ComplexExpressionAuditor;
            var rightAudit = item.RightEntry.ComplexExpressionAuditor;
            var leftAuditItems = leftAudit.FetchAuditItems();
            var rightAuditItems = rightAudit.FetchAuditItems();

            Assert.IsNull(item.Expression);
            Assert.AreEqual(1, item.UsedRecordsetIndex);
            Assert.AreEqual(1, leftAuditItems.Count);
            Assert.AreEqual(3, rightAuditItems.Count);
            StringAssert.Contains(leftAuditItems[0].Expression, "[[rec2(1).b]]");
            StringAssert.Contains(leftAuditItems[0].RawExpression, "[[rec2(1).b]]");
            StringAssert.Contains(rightAuditItems[0].Expression, "[[rec(*).a]]");
            StringAssert.Contains(rightAuditItems[0].RawExpression, "[[rec(1).a]]");
            StringAssert.Contains(rightAuditItems[0].BoundValue, "1");
            StringAssert.Contains(rightAuditItems[1].Expression, "[[rec(*).a]]");
            StringAssert.Contains(rightAuditItems[1].RawExpression, "[[rec(2).a]]");
            StringAssert.Contains(rightAuditItems[1].BoundValue, "2");
            StringAssert.Contains(rightAuditItems[2].Expression, "[[rec(*).a]]");
            StringAssert.Contains(rightAuditItems[2].RawExpression, "[[rec(3).a]]");
            StringAssert.Contains(rightAuditItems[2].BoundValue, "3");
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ServerDataListCompiler_UpsertWithDebug")]
        public void ServerDataListCompiler_UpsertWithDebug_WhenDebugActiveAndEvaluatedIndexOfBlankInToExpression_ExpectCorrectExpressionAuditData()
        {
            //------------Setup for test--------------------------
            ErrorResultTO errors;
            const string dl = "<DataList><idx/><rec><a/></rec><rec2><b/></rec2></DataList>";
            const string dlData = "<DataList><idx></idx><rec><a>1</a></rec><rec><a>2</a></rec><rec><a>3</a></rec></DataList>";

            byte[] data = (TestHelper.ConvertStringToByteArray(dlData));
            Guid dlID = _sdlc.ConvertTo(null, xmlFormat, data, dl, out errors);

            var payload = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder(false);
            payload.IsDebug = true;
            payload.Add("[[rec2([[idx]]).b]]", "[[rec(*).a]]");


            //------------Execute Test---------------------------
            Guid result = _sdlc.Upsert(null, dlID, payload, out errors);

            //------------Assert Results-------------------------
            var debugItems = payload.DebugOutputs;
            Assert.AreEqual(dlID, result);
            Assert.IsNotNull(debugItems);
            Assert.AreEqual(1, debugItems.Count);

            var item = debugItems[0];
            var leftAudit = item.LeftEntry.ComplexExpressionAuditor;
            var rightAudit = item.RightEntry.ComplexExpressionAuditor;
            var leftAuditItems = leftAudit.FetchAuditItems();
            var rightAuditItems = rightAudit.FetchAuditItems();

            Assert.IsNull(item.Expression);
            Assert.AreEqual(1, item.UsedRecordsetIndex);
            Assert.AreEqual(1, leftAuditItems.Count);
            Assert.AreEqual(3, rightAuditItems.Count);
            StringAssert.Contains(leftAuditItems[0].Expression, "[[rec2().b]]");
            StringAssert.Contains(leftAuditItems[0].RawExpression, "[[rec2().b]]");
            StringAssert.Contains(rightAuditItems[0].Expression, "[[rec(*).a]]");
            StringAssert.Contains(rightAuditItems[0].RawExpression, "[[rec(1).a]]");
            StringAssert.Contains(rightAuditItems[0].BoundValue, "1");
            StringAssert.Contains(rightAuditItems[1].Expression, "[[rec(*).a]]");
            StringAssert.Contains(rightAuditItems[1].RawExpression, "[[rec(2).a]]");
            StringAssert.Contains(rightAuditItems[1].BoundValue, "2");
            StringAssert.Contains(rightAuditItems[2].Expression, "[[rec(*).a]]");
            StringAssert.Contains(rightAuditItems[2].RawExpression, "[[rec(3).a]]");
            StringAssert.Contains(rightAuditItems[2].BoundValue, "3");
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ServerDataListCompiler_UpsertWithDebug")]
        public void ServerDataListCompiler_UpsertWithDebug_WhenDebugActiveAndBadEvaluatedIndexInToExpression_ExpectCorrectExpressionAuditData()
        {
            //------------Setup for test--------------------------
            ErrorResultTO errors;
            const string dl = "<DataList><idx/><rec><a/></rec><rec2><b/></rec2></DataList>";
            const string dlData = "<DataList><idx>abc</idx><rec><a>1</a></rec><rec><a>2</a></rec><rec><a>3</a></rec></DataList>";

            byte[] data = (TestHelper.ConvertStringToByteArray(dlData));
            Guid dlID = _sdlc.ConvertTo(null, xmlFormat, data, dl, out errors);

            var payload = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder(false);
            payload.IsDebug = true;
            payload.Add("[[rec2([[idx]]).b]]", "[[rec(*).a]]");


            //------------Execute Test---------------------------
            _sdlc.Upsert(null, dlID, payload, out errors);

            //------------Assert Results-------------------------            
            Assert.IsTrue(errors.HasErrors());
            StringAssert.Contains(errors.FetchErrors()[0], "Error Parsing Recordset Index For Debug Item Generation");

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ServerDataListCompiler_UpsertWithDebug")]
        public void ServerDataListCompiler_UpsertWithDebug_WhenDebugActiveAndBadEvaluatedIndexInFromExpression_ExpectCorrectExpressionAuditData()
        {
            //------------Setup for test--------------------------
            ErrorResultTO errors;
            const string dl = "<DataList><idx/><rec><a/></rec><rec2><b/></rec2></DataList>";
            const string dlData = "<DataList><idx>abc</idx><rec><a>1</a></rec><rec><a>2</a></rec><rec><a>3</a></rec></DataList>";

            byte[] data = (TestHelper.ConvertStringToByteArray(dlData));
            Guid dlID = _sdlc.ConvertTo(null, xmlFormat, data, dl, out errors);

            var payload = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder(false);
            payload.IsDebug = true;
            payload.Add("[[rec2().b]]", "[[rec([[idx]]).a]]");


            //------------Execute Test---------------------------
            _sdlc.Upsert(null, dlID, payload, out errors);

            //------------Assert Results-------------------------            
            Assert.IsTrue(errors.HasErrors());
            StringAssert.Contains(errors.FetchErrors()[0], "Problems Evaluating Expression [ [[rec([[idx]]).a]] ]");

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ServerDataListCompiler_Evaluate")]
        public void ServerDataListCompiler_Evaluate_WhenRecusiveEvaluation_ExpectValue()
        {
            //------------Setup for test--------------------------
            ErrorResultTO errors;
            string error;
            const string dl = "<DataList><scalar1/><rs1><f1/><f2/></rs1><rs2><f1/></rs2><scalar2/></DataList>";
            const string dlData = "<DataList><scalar1>1</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1><rs2><f1>1</f1></rs2><scalar2/></DataList>";

            byte[] data = (TestHelper.ConvertStringToByteArray(dlData));
            Guid dlID = _sdlc.ConvertTo(null, xmlFormat, data, dl, out errors);

            //------------Execute Test---------------------------
            IBinaryDataListEntry result = _sdlc.Evaluate(null, dlID, enActionType.User, "[[rs1([[rs2(1).f1]]).f1]]", out errors);

            //------------Assert Results-------------------------
            var res1 = (result.FetchRecordAt(1, out error))[0].TheValue;

            Assert.IsFalse(errors.HasErrors());
            Assert.AreEqual("f1.1", res1);

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ServerDataListCompiler_Evaluate")]
        public void ServerDataListCompiler_Evaluate_WhenRecusiveEvaluationWithScalar_ExpectValue()
        {
            //------------Setup for test--------------------------
            ErrorResultTO errors;
            string error;
            const string dl = "<DataList><scalar1/><rs1><f1/><f2/></rs1><rs2><f1/></rs2><scalar2/></DataList>";
            const string dlData = "<DataList><scalar1>1</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1><rs2><f1>1</f1></rs2><scalar2/></DataList>";

            byte[] data = (TestHelper.ConvertStringToByteArray(dlData));
            Guid dlID = _sdlc.ConvertTo(null, xmlFormat, data, dl, out errors);

            //------------Execute Test---------------------------
            IBinaryDataListEntry result = _sdlc.Evaluate(null, dlID, enActionType.User, "[[rs1([[scalar1]]).f1]]", out errors);

            //------------Assert Results-------------------------
            var res1 = (result.FetchRecordAt(1, out error))[0].TheValue;

            Assert.IsFalse(errors.HasErrors());
            Assert.AreEqual("f1.1", res1);

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ServerDataListCompiler_Evaluate")]
        public void ServerDataListCompiler_Evaluate_WhenRecusiveEvaluationWithScalarNested_ExpectValue()
        {
            //------------Setup for test--------------------------
            ErrorResultTO errors;
            string error;
            const string dl = "<DataList><scalar1/><rs1><f1/><f2/></rs1><rs2><f1/></rs2><scalar2/></DataList>";
            const string dlData = "<DataList><scalar1>rs1(1).f1</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1><rs2><f1>1</f1></rs2><scalar2/></DataList>";

            byte[] data = (TestHelper.ConvertStringToByteArray(dlData));
            Guid dlID = _sdlc.ConvertTo(null, xmlFormat, data, dl, out errors);

            //------------Execute Test---------------------------
            IBinaryDataListEntry result = _sdlc.Evaluate(null, dlID, enActionType.User, "[[[[scalar1]]]]", out errors);

            //------------Assert Results-------------------------
            var res1 = (result.FetchRecordAt(1, out error))[0].TheValue;

            Assert.IsFalse(errors.HasErrors());
            Assert.AreEqual("f1.1", res1);

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ServerDataListCompiler_Evaluate")]
        public void ServerDataListCompiler_Evaluate_WhenRecusiveEvaluationWithRecordsetNested_ExpectValue()
        {
            //------------Setup for test--------------------------
            ErrorResultTO errors;
            string error;
            const string dl = "<DataList><scalar1/><rs1><f1/><f2/></rs1><rs2><f1/></rs2><scalar2/></DataList>";
            const string dlData = "<DataList><scalar1>rs1(1).f1</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1><rs2><f1>rs1(1).f1</f1></rs2><scalar2/></DataList>";

            byte[] data = (TestHelper.ConvertStringToByteArray(dlData));
            Guid dlID = _sdlc.ConvertTo(null, xmlFormat, data, dl, out errors);

            //------------Execute Test---------------------------
            IBinaryDataListEntry result = _sdlc.Evaluate(null, dlID, enActionType.User, "[[[[rs2(1).f1]]]]", out errors);

            //------------Assert Results-------------------------
            var res1 = (result.FetchRecordAt(1, out error))[0].TheValue;

            Assert.IsFalse(errors.HasErrors());
            Assert.AreEqual("f1.1", res1);

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ServerDataListCompiler_Evaluate")]
        public void ServerDataListCompiler_Evaluate_WhenRecusiveEvaluationWithRecordsetsSoNestedNoOneWillEverUseItThisWay_ExpectValue()
        {
            //------------Setup for test--------------------------
            ErrorResultTO errors;
            string error;
            const string dl = "<DataList><scalar1/><rs1><f1/><f2/></rs1><rs2><f1/></rs2><scalar2/></DataList>";
            const string dlData = "<DataList><scalar1>rs1(1).f1</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1><rs2><f1>scalar1</f1></rs2><scalar2/></DataList>";

            byte[] data = (TestHelper.ConvertStringToByteArray(dlData));
            Guid dlID = _sdlc.ConvertTo(null, xmlFormat, data, dl, out errors);

            //------------Execute Test---------------------------
            IBinaryDataListEntry result = _sdlc.Evaluate(null, dlID, enActionType.User, "[[[[[[rs2(1).f1]]]]]]", out errors);

            //------------Assert Results-------------------------
            var res1 = (result.FetchRecordAt(1, out error))[0].TheValue;

            Assert.IsFalse(errors.HasErrors());
            Assert.AreEqual("f1.1", res1);

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ServerDataListCompiler_Evaluate")]
        public void ServerDataListCompiler_Evaluate_WhenRecusiveEvaluationWithRecordsetsSoNestedNoOneWillEverUseItThisWayToRoot_ExpectValue()
        {
            //------------Setup for test--------------------------
            ErrorResultTO errors;
            const string dl = "<DataList><scalar1/><rs1><f1/><f2/></rs1><rs2><f1/></rs2><scalar2/></DataList>";
            const string dlData = "<DataList><scalar1>rs1(1).f1</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1><rs2><f1>scalar1</f1></rs2><scalar2/></DataList>";

            byte[] data = (TestHelper.ConvertStringToByteArray(dlData));
            Guid dlID = _sdlc.ConvertTo(null, xmlFormat, data, dl, out errors);

            //------------Execute Test---------------------------
            IBinaryDataListEntry result = _sdlc.Evaluate(null, dlID, enActionType.User, "[[[[[[rs2(1).f1]]]]]]", out errors, true);

            //------------Assert Results-------------------------
            var res1 = result.FetchScalar().TheValue;

            Assert.IsFalse(errors.HasErrors());
            Assert.AreEqual("[[rs1(1).f1]]", res1);

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ServerDataListCompiler_Evaluate")]
        public void ServerDataListCompiler_Evaluate_WhenRecusiveEvaluationToRoot_ExpectExpression()
        {
            //------------Setup for test--------------------------
            ErrorResultTO errors;
            const string dl = "<DataList><scalar1/><rs1><f1/><f2/></rs1><rs2><f1/></rs2><scalar2/></DataList>";
            const string dlData = "<DataList><scalar1>1</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1><rs2><f1>1</f1></rs2><scalar2/></DataList>";

            byte[] data = (TestHelper.ConvertStringToByteArray(dlData));
            Guid dlID = _sdlc.ConvertTo(null, xmlFormat, data, dl, out errors);

            //------------Execute Test---------------------------
            IBinaryDataListEntry result = _sdlc.Evaluate(null, dlID, enActionType.User, "[[rs1([[rs2(1).f1]]).f1]]", out errors, true);

            //------------Assert Results-------------------------
            var res1 = result.FetchScalar().TheValue;

            Assert.IsFalse(errors.HasErrors());
            Assert.AreEqual("[[rs1(1).f1]]", res1);

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ServerDataListCompiler_Evaluate")]
        public void ServerDataListCompiler_Evaluate_WhenRecusiveEvaluationToRootWithScalar_ExpectExpression()
        {
            //------------Setup for test--------------------------
            ErrorResultTO errors;
            const string dl = "<DataList><scalar1/><rs1><f1/><f2/></rs1><rs2><f1/></rs2><scalar2/></DataList>";
            const string dlData = "<DataList><scalar1>1</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1><rs2><f1>1</f1></rs2><scalar2/></DataList>";

            byte[] data = (TestHelper.ConvertStringToByteArray(dlData));
            Guid dlID = _sdlc.ConvertTo(null, xmlFormat, data, dl, out errors);

            //------------Execute Test---------------------------
            IBinaryDataListEntry result = _sdlc.Evaluate(null, dlID, enActionType.User, "[[rs1([[scalar1]]).f1]]", out errors, true);

            //------------Assert Results-------------------------
            var res1 = result.FetchScalar().TheValue;

            Assert.IsFalse(errors.HasErrors());
            Assert.AreEqual("[[rs1(1).f1]]", res1);

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ServerDataListCompiler_Evaluate")]
        public void ServerDataListCompiler_Evaluate_WhenRecusiveEvaluationToRootWithDoubleNestingScalar_ExpectExpression()
        {
            //------------Setup for test--------------------------
            ErrorResultTO errors;
            const string dl = "<DataList><scalar1/><rs1><f1/><f2/></rs1><rs2><f1/></rs2><scalar2/></DataList>";
            const string dlData = "<DataList><scalar1>rs1(1).f1</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1><rs2><f1>1</f1></rs2><scalar2/></DataList>";

            byte[] data = (TestHelper.ConvertStringToByteArray(dlData));
            Guid dlID = _sdlc.ConvertTo(null, xmlFormat, data, dl, out errors);

            //------------Execute Test---------------------------
            IBinaryDataListEntry result = _sdlc.Evaluate(null, dlID, enActionType.User, "[[[[scalar1]]]]", out errors, true);

            //------------Assert Results-------------------------
            var res1 = result.FetchScalar().TheValue;

            Assert.IsFalse(errors.HasErrors());
            Assert.AreEqual("[[rs1(1).f1]]", res1);

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ServerDataListCompiler_Evaluate")]
        public void ServerDataListCompiler_Evaluate_WhenRecusiveEvaluationToRootWithDoubleNestingRecordset_ExpectExpression()
        {
            //------------Setup for test--------------------------
            ErrorResultTO errors;
            const string dl = "<DataList><scalar1/><rs1><f1/><f2/></rs1><rs2><f1/></rs2><scalar2/></DataList>";
            const string dlData = "<DataList><scalar1>rs1(1).f1</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1><rs2><f1>scalar1</f1></rs2><scalar2/></DataList>";

            byte[] data = (TestHelper.ConvertStringToByteArray(dlData));
            Guid dlID = _sdlc.ConvertTo(null, xmlFormat, data, dl, out errors);

            //------------Execute Test---------------------------
            IBinaryDataListEntry result = _sdlc.Evaluate(null, dlID, enActionType.User, "[[[[rs2(1).f1]]]]", out errors, true);

            //------------Assert Results-------------------------
            var res1 = result.FetchScalar().TheValue;

            Assert.IsFalse(errors.HasErrors());
            Assert.AreEqual("[[scalar1]]", res1);

        }

        // Travis.Frisinger 
        [TestMethod]
        public void EvaluateTwoRecordsetsWithStar()
        {
            ErrorResultTO errors;
            string error;

            byte[] data = (TestHelper.ConvertStringToByteArray(_dataListWellformedData));
            Guid dlID = _sdlc.ConvertTo(null, xmlFormat, data, _dataListWellformed, out errors);

            IBinaryDataListEntry result = _sdlc.Evaluate(null, dlID, enActionType.User, "[[rs1(*).f1]] [[rs1(*).f1]]", out errors);

            var res1 = (result.FetchRecordAt(1, out error))[0].TheValue;
            var res2 = (result.FetchRecordAt(2, out error))[0].TheValue;

            Assert.IsFalse(errors.HasErrors());
            Assert.AreEqual("f1.1 f1.1", res1);
            Assert.AreEqual("f1.2 f1.2", res2);

        }

        [TestMethod]
        public void EvaluateTwoRecordsetsOneWithStarOneWithEvaluatedIndexAndScalar()
        {
            ErrorResultTO errors;
            string error;

            byte[] data = (TestHelper.ConvertStringToByteArray(_dataListWellformedData));
            Guid dlID = _sdlc.ConvertTo(null, xmlFormat, data, _dataListWellformed, out errors);

            IBinaryDataListEntry result = _sdlc.Evaluate(null, dlID, enActionType.User, "[[rs1(*).f1]] [[rs1([[scalar1]]).f1]] [[scalar1]]", out errors);

            var res1 = (result.FetchRecordAt(1, out error))[0].TheValue;
            var res2 = (result.FetchRecordAt(2, out error))[0].TheValue;

            Assert.IsFalse(errors.HasErrors());
            Assert.AreEqual("f1.1 f1.1 1", res1);
            Assert.AreEqual("f1.2 f1.1 1", res2);

        }

        [TestMethod]
        public void EvaluateTwoRecordsetsWithStarAndScalar()
        {
            ErrorResultTO errors;
            string error;

            byte[] data = (TestHelper.ConvertStringToByteArray(_dataListWellformedData));
            Guid dlID = _sdlc.ConvertTo(null, xmlFormat, data, _dataListWellformed, out errors);

            IBinaryDataListEntry result = _sdlc.Evaluate(null, dlID, enActionType.User, "[[rs1(*).f1]] [[rs1(*).f1]] [[scalar1]]", out errors);

            var res1 = (result.FetchRecordAt(1, out error))[0].TheValue;
            var res2 = (result.FetchRecordAt(2, out error))[0].TheValue;

            Assert.IsFalse(errors.HasErrors());
            Assert.AreEqual("f1.1 f1.1 1", res1);
            Assert.AreEqual("f1.2 f1.2 1", res2);

        }

        [TestMethod]
        public void Evaluate_UserScalar_Expect_Value()
        {
            ErrorResultTO errors;
            byte[] data = (TestHelper.ConvertStringToByteArray(_dataListWellformedData));
            Guid dlID = _sdlc.ConvertTo(null, xmlFormat, data, _dataListWellformed, out errors);

            IBinaryDataListEntry result = _sdlc.Evaluate(null, dlID, enActionType.User, "[[scalar1]]", out errors);

            var res1 = result.FetchScalar().TheValue;

            Assert.IsFalse(errors.HasErrors());
            Assert.AreEqual("1", res1);

        }

        // Travis.Frisinger -  Bug 8608
        [TestMethod]
        public void Evaluate_UserRecordsetLastIndex_Expect_Value()
        {
            ErrorResultTO errors;

            byte[] data = (TestHelper.ConvertStringToByteArray(_dataListWellformedData));
            Guid dlID = _sdlc.ConvertTo(null, xmlFormat, data, _dataListWellformed, out errors);

            IBinaryDataListEntry result = _sdlc.Evaluate(null, dlID, enActionType.User, "[[rs1().f1]]", out errors);

            var res1 = result.FetchScalar().TheValue;

            Assert.IsFalse(errors.HasErrors());
            Assert.AreEqual("f1.2", res1);

        }

        // Travis.Frisinger - Bug 8608
        [TestMethod]
        public void Evaluate_UserRecordsetWithEvaluatedIndex_Expect_Value()
        {
            ErrorResultTO errors;
            byte[] data = (TestHelper.ConvertStringToByteArray(_dataListWellformedData));
            Guid dlID = _sdlc.ConvertTo(null, xmlFormat, data, _dataListWellformed, out errors);

            IBinaryDataListEntry result = _sdlc.Evaluate(null, dlID, enActionType.User, "[[rs1([[scalar1]]).f1]]", out errors);

            var res1 = result.FetchScalar().TheValue;

            Assert.IsFalse(errors.HasErrors());
            Assert.AreEqual("f1.1", res1);

        }

        [TestMethod]
        public void Evaluate_UserRecordsetWithStarIndex_Expect_Value()
        {
            ErrorResultTO errors;
            byte[] data = (TestHelper.ConvertStringToByteArray(_dataListWellformedData));
            Guid dlID = _sdlc.ConvertTo(null, xmlFormat, data, _dataListWellformed, out errors);
            string error;

            IBinaryDataListEntry result = _sdlc.Evaluate(null, dlID, enActionType.User, "[[rs1(*).f1]]", out errors);

            Assert.IsFalse(errors.HasErrors());
            Assert.AreEqual(2, result.ItemCollectionSize());
            int idx = result.FetchLastRecordsetIndex();
            var actual = (result.FetchRecordAt(idx, out error))[0].TheValue;

            Assert.AreEqual("f1.2", actual, "Got  [ " + actual + "]");

        }

        [TestMethod]
        public void Evaluate_UserPartialRecursiveExpression_Expect_Value()
        {
            ErrorResultTO errors;
            byte[] data = (TestHelper.ConvertStringToByteArray("<DataList><scalar1>scalar3</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1><rs2><f1a>rs2.f1</f1a></rs2><scalar2>scalar</scalar2></DataList>"));
            Guid dlID = _sdlc.ConvertTo(null, xmlFormat, data, "<DataList><scalar1/><scalar3/><rs1><f1/><f2/></rs1><rs2><f1a/></rs2><scalar2/></DataList>", out errors);

            IBinaryDataListEntry result = _sdlc.Evaluate(null, dlID, enActionType.User, "[[[[scalar2]]1]]", out errors);

            Assert.IsFalse(errors.HasErrors());
            var binaryDataListItem = result.FetchScalar();
            var theValue = binaryDataListItem.TheValue;

            Assert.AreEqual("scalar3", theValue);

        }

        [TestMethod]
        public void EvaluateRecordsetWithStarIndexAndStaticDataExpectStaticDataAppendedToAllRecordsetEntries()
        {
            ErrorResultTO errors;
            byte[] data = (TestHelper.ConvertStringToByteArray("<DataList><scalar1></scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1></DataList>"));
            Guid dlID = _sdlc.ConvertTo(null, xmlFormat, data, "<DataList><scalar1/><scalar3/><rs1><f1/><f2/></rs1><rs2><f1a/></rs2><scalar2/></DataList>", out errors);
            string error;

            IBinaryDataListEntry result = _sdlc.Evaluate(null, dlID, enActionType.User, "[[rs1(*).f1]] some cool static data ;)", out errors);

            var res1 = (result.FetchRecordAt(1, out error))[0].TheValue;
            var res2 = (result.FetchRecordAt(2, out error))[0].TheValue;

            Assert.AreEqual("f1.1 some cool static data ;)", res1);
            Assert.AreEqual("f1.2 some cool static data ;)", res2);

        }

        [TestMethod]
        public void EvaluateRecordsetWithStarIndexAndStaticDataAndScalarExpectStaticDataAppendedToAllRecordsetEntries()
        {
            ErrorResultTO errors;
            byte[] data = (TestHelper.ConvertStringToByteArray("<DataList><scalar1>even more static data ;)</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1></DataList>"));
            Guid dlID = _sdlc.ConvertTo(null, xmlFormat, data, "<DataList><scalar1/><scalar3/><rs1><f1/><f2/></rs1><rs2><f1a/></rs2><scalar2/></DataList>", out errors);
            string error;

            IBinaryDataListEntry result = _sdlc.Evaluate(null, dlID, enActionType.User, "[[rs1(*).f1]] some cool static data ;) [[scalar1]]", out errors);

            var res1 = (result.FetchRecordAt(1, out error))[0].TheValue;
            var res2 = (result.FetchRecordAt(2, out error))[0].TheValue;

            Assert.AreEqual("f1.1 some cool static data ;) even more static data ;)", res1);
            Assert.AreEqual("f1.2 some cool static data ;) even more static data ;)", res2);

        }

        [TestMethod]
        public void EvaluateRecordsetWithStarIndexAndStaticDataAndScalarPreFixExpectStaticDataAppendedToAllRecordsetEntries()
        {
            ErrorResultTO errors;
            byte[] data = (TestHelper.ConvertStringToByteArray("<DataList><scalar1>even more static data ;)</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1></DataList>"));
            Guid dlID = _sdlc.ConvertTo(null, xmlFormat, data, "<DataList><scalar1/><scalar3/><rs1><f1/><f2/></rs1><rs2><f1a/></rs2><scalar2/></DataList>", out errors);
            string error;

            IBinaryDataListEntry result = _sdlc.Evaluate(null, dlID, enActionType.User, "[[scalar1]] [[rs1(*).f1]] some cool static data ;)", out errors);


            var res1 = (result.FetchRecordAt(1, out error))[0].TheValue;
            var res2 = (result.FetchRecordAt(2, out error))[0].TheValue;

            Assert.AreEqual("even more static data ;) f1.1 some cool static data ;)", res1);
            Assert.AreEqual("even more static data ;) f1.2 some cool static data ;)", res2);

        }

        [TestMethod]
        public void EvaluateRecordsetWithStarIndexAndStaticDataAndRecordsetWithIndexPreFixExpectStaticDataAppendedToAllRecordsetEntries()
        {
            ErrorResultTO errors;
            byte[] data = (TestHelper.ConvertStringToByteArray("<DataList><scalar1>even more static data ;)</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1><rs2><f1a>recordset data ;)</f1a></rs2></DataList>"));
            Guid dlID = _sdlc.ConvertTo(null, xmlFormat, data, "<DataList><scalar1/><scalar3/><rs1><f1/><f2/></rs1><rs2><f1a/></rs2><scalar2/></DataList>", out errors);
            string error;

            IBinaryDataListEntry result = _sdlc.Evaluate(null, dlID, enActionType.User, "[[rs2(1).f1a]] [[rs1(*).f1]] some cool static data ;)", out errors);

            var res1 = (result.FetchRecordAt(1, out error))[0].TheValue;
            var res2 = (result.FetchRecordAt(2, out error))[0].TheValue;


            Assert.AreEqual("recordset data ;) f1.1 some cool static data ;)", res1);
            Assert.AreEqual("recordset data ;) f1.2 some cool static data ;)", res2);

        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ServerDataListCompiler_Evaluate")]
        public void ServerDataListCompiler_Evaluate_Recordset_FullRow()
        {
            //------------Setup for test--------------------------
            ErrorResultTO errors;
            byte[] data = (TestHelper.ConvertStringToByteArray("<DataList><rs1><f1>f1.1</f1><f2>f2.1</f2></rs1><rs1><f1>f1.2</f1><f2>f2.2</f2></rs1></DataList>"));
            Guid dlID = _sdlc.ConvertTo(null, xmlFormat, data, "<DataList><scalar1/><scalar3/><rs1><f1/><f2/></rs1><rs2><f1a/></rs2><scalar2/></DataList>", out errors);
            string error;

            //------------Execute Test---------------------------
            IBinaryDataListEntry result = _sdlc.Evaluate(null, dlID, enActionType.User, "[[rs1(*)]]", out errors);
            //------------Assert Results-------------------------
            var res1 = (result.FetchRecordAt(1, out error))[0].TheValue;
            var res2 = (result.FetchRecordAt(2, out error))[0].TheValue;

            Assert.AreEqual("f1.1", res1);
            Assert.AreEqual("f1.2", res2);
        }

        #endregion Positive Evaluate Test

        #region Positive Upsert Test
        [TestMethod]
        public void UpsertScalar_Expect_Insert()
        {
            ErrorResultTO errors;
            byte[] data = (TestHelper.ConvertStringToByteArray("<DataList><scalar1>scalar3</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1><rs2><f1a>rs2.f1</f1a></rs2><scalar2>scalar</scalar2></DataList>"));
            Guid dlID = _sdlc.ConvertTo(null, xmlFormat, data, "<DataList><scalar1/><scalar3/><rs1><f1/><f2/></rs1><rs2><f1a/></rs2><scalar2/></DataList>", out errors);
            string error;

            string myDate = _sdlc.ConvertFrom(null, dlID, enTranslationDepth.Data, DataListFormat.CreateFormat(GlobalConstants._XML), out errors).FetchAsString();

            if(myDate != string.Empty)
            {
                Console.WriteLine(myDate);
            }

            IBinaryDataListItem toUpsert = Dev2BinaryDataListFactory.CreateBinaryItem("test_upsert_value", "scalar2");
            IBinaryDataListEntry upsertEntry = Dev2BinaryDataListFactory.CreateEntry("scalar2", string.Empty, dlID);
            upsertEntry.TryPutScalar(toUpsert, out error);

            Guid upsertID = _sdlc.Upsert(null, dlID, "[[scalar1]]", upsertEntry, out errors);

            Assert.AreEqual(upsertID, dlID);
            Assert.IsFalse(errors.HasErrors());

            IBinaryDataList bdl = _sdlc.FetchBinaryDataList(null, upsertID, out errors);
            IBinaryDataListEntry tmp;
            bdl.TryGetEntry("scalar1", out tmp, out error);
            var res = tmp.FetchScalar().TheValue;
            Assert.AreEqual("test_upsert_value", res);
        }

        [TestMethod]
        public void UpsertRecursiveEvaluatedScalar_Expect_Insert()
        {
            ErrorResultTO errors;
            byte[] data = (TestHelper.ConvertStringToByteArray("<DataList><scalar1>scalar3</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1><rs2><f1a>rs2.f1</f1a></rs2><scalar2>scalar</scalar2></DataList>"));
            Guid dlID = _sdlc.ConvertTo(null, xmlFormat, data, "<DataList><scalar1/><scalar3/><rs1><f1/><f2/></rs1><rs2><f1a/></rs2><scalar2/></DataList>", out errors);
            string error;

            IBinaryDataListItem toUpsert = Dev2BinaryDataListFactory.CreateBinaryItem("test_upsert_value", "scalar2");
            IBinaryDataListEntry upsertEntry = Dev2BinaryDataListFactory.CreateEntry("scalar2", string.Empty, dlID);
            upsertEntry.TryPutScalar(toUpsert, out error);

            Guid upsertID = _sdlc.Upsert(null, dlID, "[[[[scalar1]]]]", upsertEntry, out errors);

            Assert.AreEqual(upsertID, dlID);
            Assert.IsFalse(errors.HasErrors());

            IBinaryDataList bdl = _sdlc.FetchBinaryDataList(null, upsertID, out errors);
            IBinaryDataListEntry tmp;
            bdl.TryGetEntry("scalar3", out tmp, out error);
            var res = tmp.FetchScalar().TheValue;
            Assert.AreEqual("test_upsert_value", res);
        }

        [TestMethod]
        public void UpsertToRecordsetWithNumericIndex_Expect_Insert()
        {
            ErrorResultTO errors;
            byte[] data = (TestHelper.ConvertStringToByteArray("<DataList><scalar1>scalar3</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1><rs2><f1a>rs2.f1</f1a></rs2><scalar2>scalar</scalar2></DataList>"));
            Guid dlID = _sdlc.ConvertTo(null, xmlFormat, data, "<DataList><scalar1/><scalar3/><rs1><f1/><f2/></rs1><rs2><f1a/></rs2><scalar2/></DataList>", out errors);
            string error;

            IBinaryDataListItem toUpsert = Dev2BinaryDataListFactory.CreateBinaryItem("test_upsert_value", "scalar2");
            IBinaryDataListEntry upsertEntry = Dev2BinaryDataListFactory.CreateEntry("scalar2", string.Empty, dlID);
            upsertEntry.TryPutScalar(toUpsert, out error);

            Guid upsertID = _sdlc.Upsert(null, dlID, "[[rs1(5).f2]]", upsertEntry, out errors);

            Assert.AreEqual(upsertID, dlID);
            Assert.IsFalse(errors.HasErrors());

            IBinaryDataList bdl = _sdlc.FetchBinaryDataList(null, upsertID, out errors);
            IBinaryDataListEntry tmp;
            bdl.TryGetEntry("rs1", out tmp, out error);
            var res = tmp.TryFetchRecordsetColumnAtIndex("f2", 5, out error).TheValue;
            Assert.AreEqual("test_upsert_value", res);
        }

        [TestMethod]
        public void UpsertToRecordsetWithBlankIndex_Expect_Append()
        {
            ErrorResultTO errors;
            byte[] data = (TestHelper.ConvertStringToByteArray("<DataList><scalar1>scalar3</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1><rs2><f1a>rs2.f1</f1a></rs2><scalar2>scalar</scalar2></DataList>"));
            Guid dlID = _sdlc.ConvertTo(null, xmlFormat, data, "<DataList><scalar1/><scalar3/><rs1><f1/><f2/></rs1><rs2><f1a/></rs2><scalar2/></DataList>", out errors);
            string error;

            IBinaryDataListItem toUpsert = Dev2BinaryDataListFactory.CreateBinaryItem("test_upsert_value", "scalar2");
            IBinaryDataListEntry upsertEntry = Dev2BinaryDataListFactory.CreateEntry("scalar2", string.Empty, dlID);
            upsertEntry.TryPutScalar(toUpsert, out error);

            Guid upsertID = _sdlc.Upsert(null, dlID, "[[rs1().f2]]", upsertEntry, out errors);

            Assert.AreEqual(upsertID, dlID);
            Assert.IsFalse(errors.HasErrors());

            IBinaryDataList bdl = _sdlc.FetchBinaryDataList(null, upsertID, out errors);
            IBinaryDataListEntry tmp;
            bdl.TryGetEntry("rs1", out tmp, out error);

            var res = tmp.TryFetchRecordsetColumnAtIndex("f2", 3, out error).TheValue;

            Assert.AreEqual("test_upsert_value", res);
        }

        [TestMethod]
        public void UpsertToRecordsetWithStarIndex_Expect_Append()
        {
            ErrorResultTO errors;
            byte[] data = (TestHelper.ConvertStringToByteArray("<DataList><scalar1>scalar3</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1><rs2><f1a>rs2.f1</f1a></rs2><scalar2>scalar</scalar2></DataList>"));
            Guid dlID = _sdlc.ConvertTo(null, xmlFormat, data, "<DataList><scalar1/><scalar3/><rs1><f1/><f2/></rs1><rs2><f1a/></rs2><scalar2/></DataList>", out errors);
            string error;

            data = (TestHelper.ConvertStringToByteArray(_dataListWellformedData));
            Guid uID = _sdlc.ConvertTo(null, xmlFormat, data, _dataListWellformed, out errors);

            //IBinaryDataList tmp = sdlc.FetchBinaryDataList(null, uID, out errors);
            IBinaryDataListEntry toUpsert = _sdlc.Evaluate(null, uID, enActionType.User, "[[rs1(*).f1]]", out errors);

            //tmp.TryGetEntry("rs1", out toUpsert, out error);
            Guid upsertID = _sdlc.Upsert(null, dlID, "[[rs1(*).f1]]", toUpsert, out errors);

            Assert.AreEqual(upsertID, dlID);
            Assert.IsFalse(errors.HasErrors());

            IBinaryDataList bdl = _sdlc.FetchBinaryDataList(null, upsertID, out errors);
            IBinaryDataListEntry te;
            bdl.TryGetEntry("rs1", out te, out error);
            // f1.1, f1.2

            var res = te.TryFetchRecordsetColumnAtIndex("f1", 2, out error).TheValue;

            Assert.AreEqual("f1.2", res);
        }

        #endregion

        #region Basic Shaping For Sub Execution Test

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ServerDataListCompiler_Shape")]
        public void ServerDataListCompiler_Shape_WhenInputsMapToOutputs_ExpectOneDataList()
        {
            //------------Setup for test--------------------------
            ErrorResultTO errors;
            byte[] data = (TestHelper.ConvertStringToByteArray("<DataList><scalar1>scalar3</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1><rs2><f1a>rs2.f1</f1a></rs2><scalar2>scalar</scalar2></DataList>"));
            Guid dlID = _sdlc.ConvertTo(null, xmlFormat, data, "<DataList><scalar1/><scalar3/><rs1><f1/><f2/></rs1><rs2><f1a/></rs2><scalar2/></DataList>", out errors);
            string error;

            Guid childID = _sdlc.ConvertTo(null, xmlFormat, TestHelper.ConvertStringToByteArray(string.Empty), "<DataList><rs1><f1/></rs1></DataList>", out errors);

            const string inputs = @"<Inputs><Input Name=""f1"" Source=""[[rs2().f1a]]"" Recordset=""rs1"" /></Inputs>";
            const string outputs = @"<Outputs><Output Name=""f1a"" MapsTo=""f1a"" Value=""[[rs1(*).f1]]"" Recordset=""rs2"" /></Outputs>";

            //------------Execute Test---------------------------
            _sdlc.ShapeForSubExecution(null, dlID, childID, inputs, outputs, out errors);

            IBinaryDataList bdl = _sdlc.FetchBinaryDataList(null, dlID, out errors);

            IBinaryDataListEntry tmp;
            IBinaryDataListEntry tmpRS;
            bdl.TryGetEntry("rs1", out tmpRS, out error);

            // Check scalar value
            bdl.TryGetEntry("scalar1", out tmp, out error);

            var res = tmp.FetchScalar().TheValue;

            //------------Assert Results-------------------------
            Assert.AreEqual("scalar3", res);
            Assert.AreEqual("f1.1", tmpRS.TryFetchRecordsetColumnAtIndex("f1", 1, out error).TheValue);

        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ServerDataListCompiler_Shape")]
        public void ServerDataListCompiler_Shape_WhenInvalidInputs_ExpectErrors()
        {
            //------------Setup for test--------------------------
            ErrorResultTO errors;
            byte[] data = (TestHelper.ConvertStringToByteArray("<DataList><scalar1>scalar3</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1><rs2><f1a>rs2.f1</f1a></rs2><scalar2>scalar</scalar2></DataList>"));
            Guid dlID = _sdlc.ConvertTo(null, xmlFormat, data, "<DataList><scalar1/><scalar3/><rs1><f1/><f2/></rs1><rs2><f1a/></rs2><scalar2/></DataList>", out errors);
            Guid childID = _sdlc.ConvertTo(null, xmlFormat, TestHelper.ConvertStringToByteArray(string.Empty), "<DataList><rs1><f1/></rs1></DataList>", out errors);
            const string inputs = @"<Inputs><Input Name=""scalar2"" Source=""[[rs2(**).f1a]]"" /></Inputs>";
            //------------Execute Test---------------------------
            _sdlc.ShapeForSubExecution(null, dlID, childID, inputs, "", out errors);
            //------------Assert Results-------------------------
            Assert.IsTrue(errors.HasErrors());
            var fetchErrors = errors.FetchErrors();
            StringAssert.Contains(fetchErrors[0], "Recordset index (**) contains invalid character(s)");
            StringAssert.Contains(fetchErrors[1], "Invalid Recordset Index For { [[rs2(**).f1a]] }");
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ServerDataListCompiler_Shape")]
        public void ServerDataListCompiler_Shape_WhenOutputsContainTwoRecordsets_ExpectTwoAliasMaps()
        {
            //------------Setup for test--------------------------
            ErrorResultTO errors;
            byte[] data = (TestHelper.ConvertStringToByteArray("<DataList><scalar1>scalar3</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1><rs2><f1a>rs2.f1</f1a></rs2><scalar2>scalar</scalar2></DataList>"));
            Guid dlID = _sdlc.ConvertTo(null, xmlFormat, data, "<DataList><rs1><f1/><f2/></rs1><rs1a><f2a/></rs1a></DataList>", out errors);
            string error;

            Guid childID = _sdlc.ConvertTo(null, xmlFormat, TestHelper.ConvertStringToByteArray(string.Empty), "<DataList><rs1><f1/></rs1><rs1a><f2a/></rs1a><rs2><f1a/></rs2></DataList>", out errors);

            string inputs = string.Empty;
            const string outputs = @"<Outputs><Output Name=""f1a"" MapsTo=""f1a"" Value=""[[rs1(*).f1]]"" Recordset=""rs2"" /><Output Name=""f2a"" MapsTo=""f2a"" Value=""[[rs1a(*).f2]]"" Recordset=""rs2"" /></Outputs>";

            //------------Execute Test---------------------------
            _sdlc.ShapeForSubExecution(null, dlID, childID, inputs, outputs, out errors);

            IBinaryDataList bdl = _sdlc.FetchBinaryDataList(null, childID, out errors);

            IBinaryDataListEntry tmpRS1;
            IBinaryDataListEntry tmpRS2;
            bdl.TryGetEntry("rs1", out tmpRS1, out error);
            bdl.TryGetEntry("rs1a", out tmpRS2, out error);

            //------------Assert Results-------------------------

            var rs1Aliases = tmpRS1.FetchAlias();
            var rs2Aliases = tmpRS2.FetchAlias();

            // Check counts first ;)
            Assert.AreEqual(1, rs1Aliases.Count);
            Assert.AreEqual(1, rs2Aliases.Count);

            // Check mapping keys next ;)
            Assert.AreEqual("f1a", rs1Aliases.Keys.FirstOrDefault());
            Assert.AreEqual("f2a", rs2Aliases.Keys.FirstOrDefault());

            var aliasValue1 = rs1Aliases.Values.FirstOrDefault();
            var aliasValue2 = rs2Aliases.Values.FirstOrDefault();

            // Check the MasterNamespace
            if(aliasValue1 != null)
            {
                Assert.AreEqual("rs1", aliasValue1.MasterNamespace);
            }
            else
            {
                Assert.Fail();
            }
            if(aliasValue2 != null)
            {
                Assert.AreEqual("rs1a", aliasValue2.MasterNamespace);
            }
            else
            {
                Assert.Fail();
            }

            // Finally check the MasterKeyID
            Assert.AreEqual(dlID, aliasValue1.MasterKeyID);
            Assert.AreEqual(dlID, aliasValue2.MasterKeyID);

        }

        #endregion

        #region Positive Input Shape Test

        [TestMethod]
        public void ShapeInput_With_RecordsetAndScalar_Expect_New_DataList()
        {
            ErrorResultTO errors;
            byte[] data = (TestHelper.ConvertStringToByteArray("<DataList><scalar1>scalar3</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1><rs2><f1a>rs2.f1</f1a></rs2><scalar2>scalar</scalar2></DataList>"));
            Guid dlID = _sdlc.ConvertTo(null, xmlFormat, data, "<DataList><scalar1/><scalar3/><rs1><f1/><f2/></rs1><rs2><f1a/></rs2><scalar2/></DataList>", out errors);
            string error;

            const string inputs = @"<Inputs><Input Name=""f1"" Source=""[[rs2().f1a]]"" Recordset=""rs1"" /><Input Name=""scalar1"" Source=""[[scalar2]]"" /></Inputs>";

            Guid shapedInputID = _sdlc.Shape(null, dlID, enDev2ArgumentType.Input, inputs, out errors);

            IBinaryDataList bdl = _sdlc.FetchBinaryDataList(null, shapedInputID, out errors);

            IBinaryDataListEntry tmp;
            IBinaryDataListEntry tmpRS;
            bdl.TryGetEntry("rs1", out tmpRS, out error);

            // Check scalar value
            bdl.TryGetEntry("scalar1", out tmp, out error);

            var res = tmp.FetchScalar().TheValue;

            Assert.AreEqual("scalar", res);
            Assert.AreEqual("rs2.f1", tmpRS.TryFetchRecordsetColumnAtIndex("f1", 1, out error).TheValue);
        }

        [TestMethod]
        public void ShapeInput_With_RecordsetStarIndexAndScalar_Expect_New_DataList()
        {
            ErrorResultTO errors;
            byte[] data = (TestHelper.ConvertStringToByteArray("<DataList><scalar1>scalar3</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1><rs2><f1a>rs2.f1.1</f1a></rs2><rs2><f1a>rs2.f1.2</f1a></rs2><rs2><f1a>rs2.f1.3</f1a></rs2><scalar2>scalar</scalar2></DataList>"));
            Guid dlID = _sdlc.ConvertTo(null, xmlFormat, data, "<DataList><scalar1/><scalar3/><rs1><f1/><f2/></rs1><rs2><f1a/></rs2><scalar2/></DataList>", out errors);
            string error;

            const string inputs = @"<Inputs><Input Name=""f1"" Source=""[[rs2(*).f1a]]"" Recordset=""rs1"" /><Input Name=""scalar1"" Source=""[[scalar2]]"" /></Inputs>";

            Guid shapedInputID = _sdlc.Shape(null, dlID, enDev2ArgumentType.Input, inputs, out errors);

            Assert.IsFalse(errors.HasErrors());
            IBinaryDataList bdl = _sdlc.FetchBinaryDataList(null, shapedInputID, out errors);
            Assert.AreEqual(bdl.ParentUID, dlID);

            IBinaryDataListEntry tmp;
            IBinaryDataListEntry tmpRS;
            bdl.TryGetEntry("rs1", out tmpRS, out error);

            // Check scalar value
            bdl.TryGetEntry("scalar1", out tmp, out error);

            var res = tmp.FetchScalar().TheValue;

            Assert.AreEqual("scalar", res);
            Assert.AreEqual("rs2.f1.3", tmpRS.TryFetchRecordsetColumnAtIndex("f1", 3, out error).TheValue);
        }

        [TestMethod]
        public void ShapeInput_With_RecordsetStarIndexAndScalar_WithDefaultValue_Expect_New_DataList()
        {
            ErrorResultTO errors;
            byte[] data = (TestHelper.ConvertStringToByteArray("<DataList><scalar1>scalar3</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1><rs2><f1a>rs2.f1.1</f1a></rs2><rs2><f1a>rs2.f1.2</f1a></rs2><rs2><f1a>rs2.f1.3</f1a></rs2><scalar2>scalar</scalar2></DataList>"));
            Guid dlID = _sdlc.ConvertTo(null, xmlFormat, data, "<DataList><scalar1/><scalar3/><rs1><f1/><f2/></rs1><rs2><f1a/></rs2><scalar2/></DataList>", out errors);
            string error;

            const string inputs = @"<Inputs><Input Name=""f1"" Source=""[[rs2(*).f1a]]"" Recordset=""rs1"" /><Input Name=""scalar1"" Source="""" DefaultValue=""Default_Scalar""/></Inputs>";

            Guid shapedInputID = _sdlc.Shape(null, dlID, enDev2ArgumentType.Input, inputs, out errors);

            Assert.IsFalse(errors.HasErrors());
            IBinaryDataList bdl = _sdlc.FetchBinaryDataList(null, shapedInputID, out errors);
            Assert.AreEqual(bdl.ParentUID, dlID);

            IBinaryDataListEntry tmp;
            IBinaryDataListEntry tmpRS;
            bdl.TryGetEntry("rs1", out tmpRS, out error);

            // Check scalar value
            bdl.TryGetEntry("scalar1", out tmp, out error);

            var res = tmp.FetchScalar().TheValue;

            Assert.AreEqual("Default_Scalar", res);
            Assert.AreEqual("rs2.f1.3", tmpRS.TryFetchRecordsetColumnAtIndex("f1", 3, out error).TheValue);
        }


        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ServerDataListCompiler_Shape")]
        public void ServerDataListCompiler_Shape_WhenInputRequiredButExpressionEmpty_ErrorInErrorCollection()
        {
            //------------Setup for test--------------------------
            ErrorResultTO errors;
            byte[] data = (TestHelper.ConvertStringToByteArray("<DataList><scalar1>scalar3</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1><rs2><f1a>rs2.f1.1</f1a></rs2><rs2><f1a>rs2.f1.2</f1a></rs2><rs2><f1a>rs2.f1.3</f1a></rs2><scalar2>scalar</scalar2></DataList>"));
            Guid dlID = _sdlc.ConvertTo(null, xmlFormat, data, "<DataList><scalar1/><scalar3/><rs1><f1/><f2/></rs1><rs2><f1a/></rs2><scalar2/></DataList>", out errors);

            const string inputs = @"<Inputs><Input Name=""scalar1"" Source="""" DefaultValue=""""><Validator Type=""Required"" /></Input></Inputs>";

            //------------Execute Test---------------------------
            _sdlc.Shape(null, dlID, enDev2ArgumentType.Input, inputs, out errors);

            //------------Assert Results-------------------------
            Assert.AreEqual("Required input [[scalar1]] cannot be populated", errors.MakeDisplayReady());

        }

        #endregion

        #region Positive Output Shape Test

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ServerDataListCompiler_Shape")]
        public void ServerDataListCompiler_Shape_OuputShapedWhenRecordsetMixedNotation_ExpectProperMapping()
        {

            //------------Setup for test--------------------------
            ErrorResultTO errors;

            // Create parent dataList
            const string strData0 = "<DataList><dbo_proc_get_Rows><Column1>ZZZ</Column1></dbo_proc_get_Rows></DataList>";
            const string strShape0 = "<DataList><dbo_proc_get_Rows><BigID/><Column1/><Column2/></dbo_proc_get_Rows></DataList>";
            byte[] data = (TestHelper.ConvertStringToByteArray(strData0));
            Guid parentID = _sdlc.ConvertTo(null, xmlFormat, data, strShape0, out errors);
            // Create child list to branch from -- Emulate Input shaping
            const string strData = "<DataList><dbo_proc_get_Rows><BigID>1</BigID><Column1>1</Column1><Column2>1</Column2></dbo_proc_get_Rows><dbo_proc_get_Rows><BigID>2</BigID><Column1>2</Column1><Column2>2</Column2></dbo_proc_get_Rows></DataList>";
            data = (TestHelper.ConvertStringToByteArray(strData));
            Guid dlID = _sdlc.ConvertTo(null, xmlFormat, data, "<DataList><dbo_proc_get_Rows><BigID/><Column1/><Column2/></dbo_proc_get_Rows></DataList>", out errors);
            // Set ParentID
            _sdlc.SetParentUID(dlID, parentID, out errors);


            // Value is target shape
            const string outputs = @"<Outputs><Output Name=""BigID"" MapsTo=""BigID"" Value=""[[dbo_proc_get_Rows(*).BigID]]"" Recordset=""dbo_proc_get_Rows"" /><Output Name=""Column1"" MapsTo=""Column1"" Value=""[[dbo_proc_get_Rows().Column1]]"" Recordset=""dbo_proc_get_Rows"" /><Output Name=""Column2"" MapsTo=""Column2"" Value=""[[dbo_proc_get_Rows().Column2]]"" Recordset=""dbo_proc_get_Rows"" /></Outputs>";
            const string expected = @"<DataList><dbo_proc_get_Rows><BigID>1</BigID><Column1>ZZZ</Column1><Column2></Column2></dbo_proc_get_Rows><dbo_proc_get_Rows><BigID>2</BigID><Column1>1</Column1><Column2>1</Column2></dbo_proc_get_Rows><dbo_proc_get_Rows><BigID></BigID><Column1>2</Column1><Column2>2</Column2></dbo_proc_get_Rows></DataList>";
            //------------Execute Test---------------------------
            Guid shapedOutputID = _sdlc.Shape(null, dlID, enDev2ArgumentType.Output, outputs, out errors);

            //------------Assert Results-------------------------
            ErrorResultTO tmpErrors;
            var results = _sdlc.ConvertFrom(null, shapedOutputID, enTranslationDepth.Data,
                                            DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags),
                                            out tmpErrors);

            var resultStr = results.FetchAsString();

            StringAssert.Contains(resultStr, expected);

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ServerDataListCompiler_Shape")]
        public void ServerDataListCompiler_Shape_OuputShapedWhenRecordsetMixedNotationAndScalar_ExpectProperMapping()
        {

            //------------Setup for test--------------------------
            ErrorResultTO errors;

            // Create parent dataList
            const string strData0 = "<DataList><dbo_proc_get_Rows><Column1>ZZZ</Column1></dbo_proc_get_Rows></DataList>";
            const string strShape0 = "<DataList><scalar/><dbo_proc_get_Rows><BigID/><Column1/><Column2/></dbo_proc_get_Rows></DataList>";
            byte[] data = (TestHelper.ConvertStringToByteArray(strData0));
            Guid parentID = _sdlc.ConvertTo(null, xmlFormat, data, strShape0, out errors);
            // Create child list to branch from -- Emulate Input shaping
            const string strData = "<DataList><dbo_proc_get_Rows><BigID>1</BigID><Column1>1</Column1><Column2>1</Column2></dbo_proc_get_Rows><dbo_proc_get_Rows><BigID>2</BigID><Column1>2</Column1><Column2>2</Column2></dbo_proc_get_Rows></DataList>";
            data = (TestHelper.ConvertStringToByteArray(strData));
            Guid dlID = _sdlc.ConvertTo(null, xmlFormat, data, "<DataList><dbo_proc_get_Rows><BigID/><Column1/><Column2/></dbo_proc_get_Rows></DataList>", out errors);
            // Set ParentID
            _sdlc.SetParentUID(dlID, parentID, out errors);


            // Value is target shape
            const string outputs = @"<Outputs><Output Name=""BigID"" MapsTo=""BigID"" Value=""[[dbo_proc_get_Rows(*).BigID]]"" Recordset=""dbo_proc_get_Rows"" /><Output Name=""Column1"" MapsTo=""Column1"" Value=""[[dbo_proc_get_Rows().Column1]]"" Recordset=""dbo_proc_get_Rows"" /><Output Name=""Column2"" MapsTo=""Column2"" Value=""[[scalar]]"" Recordset=""dbo_proc_get_Rows"" /></Outputs>";
            const string expected = @"<DataList><scalar>2</scalar><dbo_proc_get_Rows><BigID>1</BigID><Column1>ZZZ</Column1><Column2></Column2></dbo_proc_get_Rows><dbo_proc_get_Rows><BigID>2</BigID><Column1>1</Column1><Column2></Column2></dbo_proc_get_Rows><dbo_proc_get_Rows><BigID></BigID><Column1>2</Column1><Column2></Column2></dbo_proc_get_Rows></DataList>";
            //------------Execute Test---------------------------
            Guid shapedOutputID = _sdlc.Shape(null, dlID, enDev2ArgumentType.Output, outputs, out errors);

            //------------Assert Results-------------------------
            ErrorResultTO tmpErrors;
            var results = _sdlc.ConvertFrom(null, shapedOutputID, enTranslationDepth.Data,
                                            DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags),
                                            out tmpErrors);

            var resultStr = results.FetchAsString();
            StringAssert.Contains(resultStr, expected);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ServerDataListCompiler_Shape")]
        public void ServerDataListCompiler_Shape_OuputShapedWhenRecordsetToScalar_ExpectScalarToHaveValue()
        {

            //------------Setup for test--------------------------
            ErrorResultTO errors;
            string error;

            // Create parent dataList
            const string strData0 = "<DataList><nullFlag/><result/></DataList>";
            byte[] data = (TestHelper.ConvertStringToByteArray(strData0));
            Guid parentID = _sdlc.ConvertTo(null, xmlFormat, data, strData0, out errors);
            // Create child list to branch from -- Emulate Input shaping
            const string strData = "<DataList><nullFlag></nullFlag><dbo_NullReturnsZZZ_NotNullReturnsAAA><result>ZZZ</result></dbo_NullReturnsZZZ_NotNullReturnsAAA></DataList>";
            data = (TestHelper.ConvertStringToByteArray(strData));
            Guid dlID = _sdlc.ConvertTo(null, xmlFormat, data, "<DataList><nullFlag/><dbo_NullReturnsZZZ_NotNullReturnsAAA><result/></dbo_NullReturnsZZZ_NotNullReturnsAAA></DataList>", out errors);
            // Set ParentID
            _sdlc.SetParentUID(dlID, parentID, out errors);


            // Value is target shape
            const string outputs = @"<Outputs><Output Name=""result"" MapsTo=""result"" Value=""[[result]]"" Recordset=""dbo_NullReturnsZZZ_NotNullReturnsAAA"" /></Outputs>";

            //------------Execute Test---------------------------
            Guid shapedOutputID = _sdlc.Shape(null, dlID, enDev2ArgumentType.Output, outputs, out errors);

            //------------Assert Results-------------------------
            var bdl = _sdlc.FetchBinaryDataList(null, shapedOutputID, out errors);

            IBinaryDataListEntry tmp;

            // Check scalar value
            bdl.TryGetEntry("result", out tmp, out error);

            var res = tmp.FetchScalar().TheValue;

            Assert.AreEqual("ZZZ", res);

        }


        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ServerDataListCompiler_Shape")]
        public void ServerDataListCompiler_Shape_OuputShapedWhenRecordsetToNonExistentScalar_ExpectScalarBeBlank()
        {

            //------------Setup for test--------------------------
            ErrorResultTO errors;
            string error;

            // Create parent dataList
            const string strData0 = "<DataList><nullFlag/><result/></DataList>";
            byte[] data = (TestHelper.ConvertStringToByteArray(strData0));
            Guid parentID = _sdlc.ConvertTo(null, xmlFormat, data, strData0, out errors);
            // Create child list to branch from -- Emulate Input shaping
            const string strData = "<DataList><nullFlag></nullFlag></DataList>";
            data = (TestHelper.ConvertStringToByteArray(strData));
            Guid dlID = _sdlc.ConvertTo(null, xmlFormat, data, "<DataList><nullFlag/><result/></DataList>", out errors);
            // Set ParentID
            _sdlc.SetParentUID(dlID, parentID, out errors);


            // Value is target shape
            const string outputs = @"<Outputs><Output Name=""result1"" MapsTo=""result1"" Value=""[[result]]"" /></Outputs>";

            //------------Execute Test---------------------------
            Guid shapedOutputID = _sdlc.Shape(null, dlID, enDev2ArgumentType.Output, outputs, out errors);

            //------------Assert Results-------------------------
            var bdl = _sdlc.FetchBinaryDataList(null, shapedOutputID, out errors);

            IBinaryDataListEntry tmp;

            // Check scalar value
            bdl.TryGetEntry("result", out tmp, out error);

            var res = tmp.FetchScalar().TheValue;

            Assert.AreEqual("", res);

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ServerDataListCompiler_Shape")]
        public void ServerDataListCompiler_Shape_WhenOutputsContainDifferentRecordsetsAndColumnsAndAppendNotation_ExpectMappingToOccurAppendStyle()
        {

            //------------Setup for test--------------------------
            ErrorResultTO errors;
            const string dataListWellformedMultData = "<DataList><scalar1>p1</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1><rs2><f1a>rs2.f1</f1a></rs2><scalar2>p2</scalar2></DataList>";
            byte[] pdata = (TestHelper.ConvertStringToByteArray(dataListWellformedMultData));

            var format = DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags);

            Guid parentID = _sdlc.ConvertTo(null, format, pdata, "<DataList><scalar1/><scalar3/><rs1><f1/><f2/></rs1><rs2><f1a/></rs2><scalar2/></DataList>", out errors);
            // Create child list to branch from -- Emulate Input shaping
            const string dataListWellformedComplexData = "<DataList><scalar1>c1</scalar1><rs2><f1a>rs2.f1.1</f1a></rs2><rs2><f1a>rs2.f1.2</f1a></rs2><rs2><f1a>rs2.f1.3</f1a></rs2><scalar2>c2</scalar2></DataList>";
            var data = (TestHelper.ConvertStringToByteArray(dataListWellformedComplexData));
            Guid dlID = _sdlc.ConvertTo(null, format, data, "<DataList><scalar1/><scalar3/><rs1><f1/><f2/></rs1><rs2><f1a/></rs2><scalar2/></DataList>", out errors);

            const string outputs = @"<Outputs><Output Name=""f1a"" MapsTo=""[[rs1().f1]]"" Value=""[[rs1().f1]]"" Recordset=""rs2"" /><Output Name=""scalar2"" MapsTo=""scalar2"" Value=""[[scalar1]]"" /></Outputs>";

            // Set ParentID
            _sdlc.SetParentUID(dlID, parentID, out errors);

            //------------Execute Test---------------------------
            Guid shapedOutputID = _sdlc.Shape(null, dlID, enDev2ArgumentType.Output, outputs, out errors);

            var bdl = _sdlc.FetchBinaryDataList(null, shapedOutputID, out errors);

            IBinaryDataListEntry tmp;
            IBinaryDataListEntry tmpRS;
            string error;
            bdl.TryGetEntry("rs1", out tmpRS, out error);

            // Check scalar value
            bdl.TryGetEntry("scalar1", out tmp, out error);

            var res = tmp.FetchScalar().TheValue;

            //------------Assert Results-------------------------
            Assert.AreEqual("c2", res);
            Assert.AreEqual("rs2.f1.3", tmpRS.TryFetchRecordsetColumnAtIndex("f1", 5, out error).TheValue);

        }

        [TestMethod]
        public void ShapeOutput_With_RecordsetStarIndexAndScalar_Expect_Merge()
        {

            ErrorResultTO errors;
            string error;

            // Create parent dataList
            byte[] data = (TestHelper.ConvertStringToByteArray("<DataList><scalar1>scalar3</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1><rs2><f1a>rs2.f1</f1a></rs2><scalar2>scalar</scalar2></DataList>"));
            Guid parentID = _sdlc.ConvertTo(null, xmlFormat, data, "<DataList><scalar1/><scalar3/><rs1><f1/><f2/></rs1><rs2><f1a/></rs2><scalar2/></DataList>", out errors);
            // Create child list to branch from -- Emulate Input shaping
            data = (TestHelper.ConvertStringToByteArray("<DataList><scalar1>scalar3</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1><rs2><f1a>rs2.f1.1</f1a></rs2><rs2><f1a>rs2.f1.2</f1a></rs2><rs2><f1a>rs2.f1.3</f1a></rs2><scalar2>scalar</scalar2></DataList>"));
            Guid dlID = _sdlc.ConvertTo(null, xmlFormat, data, "<DataList><scalar1/><scalar3/><rs1><f1/><f2/></rs1><rs2><f1a/></rs2><scalar2/></DataList>", out errors);
            // Set ParentID
            _sdlc.SetParentUID(dlID, parentID, out errors);


            // Value is target shape
            //const string outputs = @"<Outputs><Output Name=""f1a"" MapsTo=""f1a"" Value=""[[rs1(*).f1]]"" Recordset=""rs2"" /><Output Name=""scalar2"" MapsTo=""scalar2"" Value=""[[scalar1]]"" /></Outputs>";

            const string outputs = @"<Outputs><Output Name=""f1a"" MapsTo=""[[rs1(*).f1]]"" Value=""[[rs1(*).f1]]"" Recordset=""rs2"" /><Output Name=""scalar2"" MapsTo=""scalar2"" Value=""[[scalar1]]"" /></Outputs>";

            Guid shapedOutputID = _sdlc.Shape(null, dlID, enDev2ArgumentType.Output, outputs, out errors);

            var bdl = _sdlc.FetchBinaryDataList(null, shapedOutputID, out errors);

            IBinaryDataListEntry tmp;
            IBinaryDataListEntry tmpRS;
            bdl.TryGetEntry("rs1", out tmpRS, out error);

            // Check scalar value
            bdl.TryGetEntry("scalar1", out tmp, out error);

            var res = tmp.FetchScalar().TheValue;


            Assert.AreEqual("scalar", res);
            Assert.AreEqual("rs2.f1.3", tmpRS.TryFetchRecordsetColumnAtIndex("f1", 3, out error).TheValue);
        }

        [TestMethod]
        public void ShapeOutput_With_RecordsetNumericIndexAndScalar_Expect_Merge()
        {

            ErrorResultTO errors;
            string error;

            // Create parent dataList
            byte[] data = (TestHelper.ConvertStringToByteArray("<DataList><scalar1>scalar3</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1><rs2><f1a>rs2.f1</f1a></rs2><scalar2>scalar</scalar2></DataList>"));
            Guid parentID = _sdlc.ConvertTo(null, xmlFormat, data, "<DataList><scalar1/><scalar3/><rs1><f1/><f2/></rs1><rs2><f1a/></rs2><scalar2/></DataList>", out errors);
            // Create child list to branch from -- Emulate Input shaping
            data = (TestHelper.ConvertStringToByteArray("<DataList><scalar1>scalar3</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1><rs2><f1a>rs2.f1.1</f1a></rs2><rs2><f1a>rs2.f1.2</f1a></rs2><rs2><f1a>rs2.f1.3</f1a></rs2><scalar2>scalar</scalar2></DataList>"));
            Guid dlID = _sdlc.ConvertTo(null, xmlFormat, data, "<DataList><scalar1/><scalar3/><rs1><f1/><f2/></rs1><rs2><f1a/></rs2><scalar2/></DataList>", out errors);
            // Set ParentID
            _sdlc.SetParentUID(dlID, parentID, out errors);


            // Value is target shape
            const string outputs = @"<Outputs><Output Name=""f1a"" MapsTo=""f1a"" Value=""[[rs1(1).f1]]"" Recordset=""rs2"" /><Output Name=""scalar2"" MapsTo=""scalar2"" Value=""[[scalar1]]"" /></Outputs>";

            Guid shapedOutputID = _sdlc.Shape(null, dlID, enDev2ArgumentType.Output, outputs, out errors);

            Assert.IsFalse(errors.HasErrors());
            IBinaryDataList bdl = _sdlc.FetchBinaryDataList(null, dlID, out errors);
            Assert.AreEqual(bdl.ParentUID, parentID);

            bdl = _sdlc.FetchBinaryDataList(null, shapedOutputID, out errors);

            IBinaryDataListEntry tmp;
            bdl.TryGetEntry("rs1", out tmp, out error);
            Assert.AreEqual("rs2.f1.3", tmp.TryFetchRecordsetColumnAtIndex("f1", 1, out error).TheValue);
            // Check scalar value
            bdl.TryGetEntry("scalar1", out tmp, out error);

            var res = tmp.FetchScalar().TheValue;
            Assert.AreEqual("scalar", res);
        }

        #endregion

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("ServerDataListCompiler_ProcessRecordsetGroup")]
        public void ServerDataListCompiler_ProcessRecordsetGroup_SymmetricalInputsAndOutputs_Copied()
        {
            //------------Setup for test--------------------------
            ErrorResultTO errors;
            var data = (TestHelper.ConvertStringToByteArray("<DataList><rs1><f1>r1f1.1</f1><f2>r1f2.1</f2></rs1><rs1><f1>r1f1.2</f1><f2>r1f2.2</f2></rs1><rs2><f1>r2f1.1</f1><f2>r2f2.1</f2></rs2><rs2><f1>r2f1.2</f1><f2>r2f2.2</f2></rs2></DataList>"));
            var dlID = _sdlc.ConvertTo(null, xmlFormat, data, "<DataList><rs1><f1/><f2/></rs1><rs2><f1/><f2/></rs2></DataList>", out errors);

            const string inputs = @"<Inputs><Input Name=""f1"" Source=""[[rs1(*).f1]]"" Recordset=""rs2"" /><Input Name=""f2"" Source=""[[rs1(*).f2]]"" Recordset=""rs2"" /></Inputs>";


            //------------Execute Test---------------------------
            Guid shapedInputID = _sdlc.Shape(null, dlID, enDev2ArgumentType.Input, inputs, out errors);

            //------------Assert Results-------------------------
            Assert.IsFalse(errors.HasErrors());
            IBinaryDataList bdl = _sdlc.FetchBinaryDataList(null, shapedInputID, out errors);
            Assert.AreEqual(bdl.ParentUID, dlID);

            string error;
            IBinaryDataListEntry tmpRS;
            bdl.TryGetEntry("rs2", out tmpRS, out error);

            var actual = tmpRS.TryFetchRecordsetColumnAtIndex("f1", 2, out error).TheValue;
            Assert.AreEqual("r1f1.2", actual);

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ServerDataListCompiler_BuildOutputExpressionExtractor")]
        public void ServerDataListCompiler_BuildOutputExpressionExtractor_WhenInputContainsRecordsetAndScalar_ExpectScalarExpression()
        {
            //------------Setup for test--------------------------
            const string inputs = @"<Inputs><Input Name=""input"" Source=""[[rs1(1).f1]]"" /></Inputs>";
            var def = DataListFactory.CreateInputParser().Parse(inputs);

            //------------Execute Test---------------------------
            var expression = _sdlc.BuildOutputExpressionExtractor(enDev2ArgumentType.Input);

            var result = expression.Invoke(def.FirstOrDefault());

            //------------Assert Results-------------------------
            const string expected = "[[input]]";

            Assert.AreEqual(expected, result);

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ServerDataListCompiler_BuildOutputExpressionExtractor")]
        public void ServerDataListCompiler_BuildOutputExpressionExtractor_WhenInputContains2Recordset_ExpectRecordsetExpression()
        {
            //------------Setup for test--------------------------
            const string inputs = @"<Inputs><Input Name=""input"" Source=""[[rs1(1).f1]]"" Recordset=""rs2""/></Inputs>";
            var def = DataListFactory.CreateInputParser().Parse(inputs);

            //------------Execute Test---------------------------
            var expression = _sdlc.BuildOutputExpressionExtractor(enDev2ArgumentType.Input);

            var result = expression.Invoke(def.FirstOrDefault());

            //------------Assert Results-------------------------
            const string expected = "[[rs2(*).input]]";

            Assert.AreEqual(expected, result);

        }
    }
}
