using System;
using Dev2.DynamicServices.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Server.Datalist;
using Dev2.DataList.Contract;
using Dev2.Common;
using Dev2.DataList.Contract.Binary_Objects;

namespace Dev2.Data.Tests.BinaryDataList {
    /// <summary>
    /// Summary description for ServerDataListCompilerTest
    /// </summary>
    [TestClass]
    public class ServerDataListCompilerTest {
        private readonly IServerDataListCompiler _sdlc = DataListFactory.CreateServerDataListCompiler();
        
        private static readonly string _dataListWellformed = "<DataList><scalar1/><rs1><f1/><f2/></rs1><scalar2/></DataList>";
        private static readonly string _dataListWellformedData = "<DataList><scalar1>1</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1><scalar2/></DataList>";
        private static readonly string _dataListWellformedMult = "<DataList><scalar1/><scalar3/><rs1><f1/><f2/></rs1><rs2><f1a/></rs2><scalar2/></DataList>";

        private static DataListFormat xmlFormat = DataListFormat.CreateFormat(GlobalConstants._XML);


        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Postive Evalaute Test

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

            _sdlc.DeleteDataListByID(dlID, false);

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

            _sdlc.DeleteDataListByID(dlID, false);

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

            _sdlc.DeleteDataListByID(dlID, false);

            Assert.IsFalse(errors.HasErrors());
            Assert.AreEqual("f1.1 f1.1 1", res1);
            Assert.AreEqual("f1.2 f1.2 1", res2);

        }

        [TestMethod]
        public void Evaluate_UserScalar_Expect_Value() {
            ErrorResultTO errors;
            byte[] data = (TestHelper.ConvertStringToByteArray(_dataListWellformedData));
            Guid dlID = _sdlc.ConvertTo(null, xmlFormat, data, _dataListWellformed, out errors);
            
            IBinaryDataListEntry result = _sdlc.Evaluate(null, dlID, enActionType.User, "[[scalar1]]", out errors);

            var res1 =  result.FetchScalar().TheValue;

            _sdlc.DeleteDataListByID(dlID, false);

            Assert.IsFalse(errors.HasErrors());
            Assert.AreEqual("1",res1);

        }

        // Travis.Frisinger -  Bug 8608
        [TestMethod]
        public void Evaluate_UserRecordsetLastIndex_Expect_Value() {
            ErrorResultTO errors;

            byte[] data = (TestHelper.ConvertStringToByteArray(_dataListWellformedData));
            Guid dlID = _sdlc.ConvertTo(null, xmlFormat, data, _dataListWellformed, out errors);

            IBinaryDataListEntry result = _sdlc.Evaluate(null, dlID, enActionType.User, "[[rs1().f1]]", out errors);

            var res1 = result.FetchScalar().TheValue;

            _sdlc.DeleteDataListByID(dlID, false);

            Assert.IsFalse(errors.HasErrors());
            Assert.AreEqual("f1.2", res1);

        }

        // Travis.Frisinger - Bug 8608
        [TestMethod]
        public void Evaluate_UserRecordsetWithEvaluatedIndex_Expect_Value() {
            ErrorResultTO errors;
            byte[] data = (TestHelper.ConvertStringToByteArray(_dataListWellformedData));
            Guid dlID = _sdlc.ConvertTo(null, xmlFormat, data, _dataListWellformed, out errors);

            IBinaryDataListEntry result = _sdlc.Evaluate(null, dlID, enActionType.User, "[[rs1([[scalar1]]).f1]]", out errors);

            var res1 = result.FetchScalar().TheValue;

            _sdlc.DeleteDataListByID(dlID, false);

            Assert.IsFalse(errors.HasErrors());
            Assert.AreEqual("f1.1", res1);

        }

        [TestMethod]
        public void Evaluate_UserRecordsetWithStarIndex_Expect_Value() {
            ErrorResultTO errors;
            byte[] data = (TestHelper.ConvertStringToByteArray(_dataListWellformedData));
            Guid dlID = _sdlc.ConvertTo(null, xmlFormat, data, _dataListWellformed, out errors);
            string error;

            IBinaryDataListEntry result = _sdlc.Evaluate(null, dlID, enActionType.User, "[[rs1(*).f1]]", out errors);

            Assert.IsFalse(errors.HasErrors());
            Assert.AreEqual(2, result.ItemCollectionSize());
            int idx = result.FetchLastRecordsetIndex();
            var actual = (result.FetchRecordAt(idx, out error))[0].TheValue;

            _sdlc.DeleteDataListByID(dlID, false);

            Assert.AreEqual("f1.2", actual, "Got  [ " + actual + "]");

        }

        [TestMethod]
        public void Evaluate_UserPartialRecursiveExpression_Expect_Value() {
            ErrorResultTO errors;
            byte[] data = (TestHelper.ConvertStringToByteArray("<DataList><scalar1>scalar3</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1><rs2><f1a>rs2.f1</f1a></rs2><scalar2>scalar</scalar2></DataList>"));
            Guid dlID = _sdlc.ConvertTo(null, xmlFormat, data, _dataListWellformedMult, out errors);

            IBinaryDataListEntry result = _sdlc.Evaluate(null, dlID, enActionType.User, "[[[[scalar2]]1]]", out errors);

            Assert.IsFalse(errors.HasErrors());
            var binaryDataListItem = result.FetchScalar();
            var theValue = binaryDataListItem.TheValue;

            _sdlc.DeleteDataListByID(dlID, false);

            Assert.AreEqual("scalar3", theValue);

        }

        [TestMethod]
        public void EvaluateRecordsetWithStarIndexAndStaticDataExpectStaticDataAppendedToAllRecordsetEntries()
        {
            ErrorResultTO errors;
            byte[] data = (TestHelper.ConvertStringToByteArray("<DataList><scalar1></scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1></DataList>"));
            Guid dlID = _sdlc.ConvertTo(null, xmlFormat, data, _dataListWellformedMult, out errors);
            string error;

            IBinaryDataListEntry result = _sdlc.Evaluate(null, dlID, enActionType.User, "[[rs1(*).f1]] some cool static data ;)", out errors);

            var res1 = (result.FetchRecordAt(1, out error))[0].TheValue;
            var res2 = (result.FetchRecordAt(2, out error))[0].TheValue;

            _sdlc.DeleteDataListByID(dlID, false);

            Assert.AreEqual("f1.1 some cool static data ;)", res1);
            Assert.AreEqual("f1.2 some cool static data ;)", res2);

        }

        [TestMethod]
        public void EvaluateRecordsetWithStarIndexAndStaticDataAndScalarExpectStaticDataAppendedToAllRecordsetEntries()
        {
            ErrorResultTO errors;
            byte[] data = (TestHelper.ConvertStringToByteArray("<DataList><scalar1>even more static data ;)</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1></DataList>"));
            Guid dlID = _sdlc.ConvertTo(null, xmlFormat, data, _dataListWellformedMult, out errors);
            string error;

            IBinaryDataListEntry result = _sdlc.Evaluate(null, dlID, enActionType.User, "[[rs1(*).f1]] some cool static data ;) [[scalar1]]", out errors);

            var res1 = (result.FetchRecordAt(1, out error))[0].TheValue;
            var res2 = (result.FetchRecordAt(2, out error))[0].TheValue;

            _sdlc.DeleteDataListByID(dlID, false);

            Assert.AreEqual("f1.1 some cool static data ;) even more static data ;)", res1);
            Assert.AreEqual("f1.2 some cool static data ;) even more static data ;)", res2);

        }
         
        [TestMethod]
        public void EvaluateRecordsetWithStarIndexAndStaticDataAndScalarPreFixExpectStaticDataAppendedToAllRecordsetEntries()
        {
            ErrorResultTO errors;
            byte[] data = (TestHelper.ConvertStringToByteArray("<DataList><scalar1>even more static data ;)</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1></DataList>"));
            Guid dlID = _sdlc.ConvertTo(null, xmlFormat, data, _dataListWellformedMult, out errors);
            string error;

            IBinaryDataListEntry result = _sdlc.Evaluate(null, dlID, enActionType.User, "[[scalar1]] [[rs1(*).f1]] some cool static data ;)", out errors);


            var res1 = (result.FetchRecordAt(1, out error))[0].TheValue;
            var res2 = (result.FetchRecordAt(2, out error))[0].TheValue;

            _sdlc.DeleteDataListByID(dlID, false);

            Assert.AreEqual("even more static data ;) f1.1 some cool static data ;)", res1);
            Assert.AreEqual("even more static data ;) f1.2 some cool static data ;)", res2);

        }

        [TestMethod]
        public void EvaluateRecordsetWithStarIndexAndStaticDataAndRecordsetWithIndexPreFixExpectStaticDataAppendedToAllRecordsetEntries()
        {
            ErrorResultTO errors;
            byte[] data = (TestHelper.ConvertStringToByteArray("<DataList><scalar1>even more static data ;)</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1><rs2><f1a>recordset data ;)</f1a></rs2></DataList>"));
            Guid dlID = _sdlc.ConvertTo(null, xmlFormat, data, _dataListWellformedMult, out errors);
            string error;

            IBinaryDataListEntry result = _sdlc.Evaluate(null, dlID, enActionType.User, "[[rs2(1).f1a]] [[rs1(*).f1]] some cool static data ;)", out errors);

            var res1 = (result.FetchRecordAt(1, out error))[0].TheValue;
            var res2 = (result.FetchRecordAt(2, out error))[0].TheValue;

            _sdlc.DeleteDataListByID(dlID, false);

            Assert.AreEqual("recordset data ;) f1.1 some cool static data ;)", res1);
            Assert.AreEqual("recordset data ;) f1.2 some cool static data ;)", res2);

        }

        #endregion Positive Evaluate Test

        #region Positive Upsert Test
        [TestMethod]
        public void UpsertScalar_Expect_Insert() {
            ErrorResultTO errors;
            byte[] data = (TestHelper.ConvertStringToByteArray("<DataList><scalar1>scalar3</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1><rs2><f1a>rs2.f1</f1a></rs2><scalar2>scalar</scalar2></DataList>"));
            Guid dlID = _sdlc.ConvertTo(null, xmlFormat, data, _dataListWellformedMult, out errors);
            string error;

            string myDate = _sdlc.ConvertFrom(null, dlID, enTranslationDepth.Data, DataListFormat.CreateFormat(GlobalConstants._XML), out errors).FetchAsString();

            if(myDate != string.Empty)
            {
                Console.WriteLine(myDate);
            }

            IBinaryDataListEntry upsertEntry;
            IBinaryDataListItem toUpsert = Dev2BinaryDataListFactory.CreateBinaryItem("test_upsert_value", "scalar2");
            upsertEntry = Dev2BinaryDataListFactory.CreateEntry("scalar2",string.Empty, dlID);
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
        public void UpsertRecursiveEvaluatedScalar_Expect_Insert() {
            ErrorResultTO errors;
            byte[] data = (TestHelper.ConvertStringToByteArray("<DataList><scalar1>scalar3</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1><rs2><f1a>rs2.f1</f1a></rs2><scalar2>scalar</scalar2></DataList>"));
            Guid dlID = _sdlc.ConvertTo(null, xmlFormat, data, _dataListWellformedMult, out errors);
            string error;

            IBinaryDataListEntry upsertEntry;
            IBinaryDataListItem toUpsert = Dev2BinaryDataListFactory.CreateBinaryItem("test_upsert_value", "scalar2");
            upsertEntry = Dev2BinaryDataListFactory.CreateEntry("scalar2", string.Empty, dlID);
            upsertEntry.TryPutScalar(toUpsert, out error);

            Guid upsertID = _sdlc.Upsert(null , dlID, "[[[[scalar1]]]]", upsertEntry, out errors);

            Assert.AreEqual(upsertID, dlID);
            Assert.IsFalse(errors.HasErrors());

            IBinaryDataList bdl = _sdlc.FetchBinaryDataList(null, upsertID, out errors);
            IBinaryDataListEntry tmp;
            bdl.TryGetEntry("scalar3", out tmp, out error);
            var res = tmp.FetchScalar().TheValue;
            Assert.AreEqual("test_upsert_value", res);
        }

        [TestMethod]
        public void UpsertToRecordsetWithNumericIndex_Expect_Insert() {
            ErrorResultTO errors;
            byte[] data = (TestHelper.ConvertStringToByteArray("<DataList><scalar1>scalar3</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1><rs2><f1a>rs2.f1</f1a></rs2><scalar2>scalar</scalar2></DataList>"));
            Guid dlID = _sdlc.ConvertTo(null, xmlFormat, data, _dataListWellformedMult, out errors);
            string error;

            IBinaryDataListEntry upsertEntry;
            IBinaryDataListItem toUpsert = Dev2BinaryDataListFactory.CreateBinaryItem("test_upsert_value", "scalar2");
            upsertEntry = Dev2BinaryDataListFactory.CreateEntry("scalar2", string.Empty,dlID);
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
        public void UpsertToRecordsetWithBlankIndex_Expect_Append() {
            ErrorResultTO errors;
            byte[] data = (TestHelper.ConvertStringToByteArray("<DataList><scalar1>scalar3</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1><rs2><f1a>rs2.f1</f1a></rs2><scalar2>scalar</scalar2></DataList>"));
            Guid dlID = _sdlc.ConvertTo(null, xmlFormat, data, _dataListWellformedMult, out errors);
            string error;

            IBinaryDataListEntry upsertEntry;
            IBinaryDataListItem toUpsert = Dev2BinaryDataListFactory.CreateBinaryItem("test_upsert_value", "scalar2");
            upsertEntry = Dev2BinaryDataListFactory.CreateEntry("scalar2", string.Empty,dlID);
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
        public void UpsertToRecordsetWithStarIndex_Expect_Append() {
            ErrorResultTO errors;
            byte[] data = (TestHelper.ConvertStringToByteArray("<DataList><scalar1>scalar3</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1><rs2><f1a>rs2.f1</f1a></rs2><scalar2>scalar</scalar2></DataList>"));
            Guid dlID = _sdlc.ConvertTo(null, xmlFormat, data, _dataListWellformedMult, out errors);
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

        #region Positive Input Shape Test

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ServerDataListCompiler_Shape")]
        public void ServerDataListCompiler_Shape_WhenInputDoesNotContainAllRecordsetColumnsAndMasterShapeSent_AllColumnsIncluded()
        {
            
            //------------Setup for test--------------------------
            ErrorResultTO errors;
            byte[] data = (TestHelper.ConvertStringToByteArray("<DataList><scalar1>scalar3</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1><rs2><f1a>rs2.f1.1</f1a></rs2><rs2><f1a>rs2.f1.2</f1a></rs2><rs2><f1a>rs2.f1.3</f1a></rs2><scalar2>scalar</scalar2></DataList>"));
            Guid dlID = _sdlc.ConvertTo(null, xmlFormat, data, _dataListWellformedMult, out errors);
            string error;

            const string inputs = @"<Inputs><Input Name=""f1"" Source=""[[rs2(*).f1a]]"" Recordset=""rs1"" /><Input Name=""scalar1"" Source=""[[scalar2]]"" /></Inputs>";

            //------------Execute Test---------------------------
            Guid shapedInputID = _sdlc.Shape(null, dlID, enDev2ArgumentType.Input, inputs, out errors, _dataListWellformedMult);

            Assert.IsFalse(errors.HasErrors());
            IBinaryDataList bdl = _sdlc.FetchBinaryDataList(null, shapedInputID, out errors);
            Assert.AreEqual(bdl.ParentUID, dlID);

            IBinaryDataListEntry tmp;
            bdl.TryGetEntry("rs1", out tmp, out error);

            var cols = tmp.Columns;

            //------------Assert Results-------------------------

            // col check, we need all of them ;)
            Assert.AreEqual("f1", cols[0].ColumnName);
            Assert.AreEqual("f2", cols[1].ColumnName);

            // clean up ;)
            _sdlc.DeleteDataListByID(bdl.ParentUID, false);

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ServerDataListCompiler_Shape")]
        public void ServerDataListCompiler_Shape_WhenInputDoesNotContainAllRecordsetColumnsAndNoMasterShapeSent_OnlyInputColumn()
        {

            //------------Setup for test--------------------------
            ErrorResultTO errors;
            byte[] data = (TestHelper.ConvertStringToByteArray("<DataList><scalar1>scalar3</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1><rs2><f1a>rs2.f1.1</f1a></rs2><rs2><f1a>rs2.f1.2</f1a></rs2><rs2><f1a>rs2.f1.3</f1a></rs2><scalar2>scalar</scalar2></DataList>"));
            Guid dlID = _sdlc.ConvertTo(null, xmlFormat, data, _dataListWellformedMult, out errors);
            string error;

            const string inputs = @"<Inputs><Input Name=""f1"" Source=""[[rs2(*).f1a]]"" Recordset=""rs1"" /><Input Name=""scalar1"" Source=""[[scalar2]]"" /></Inputs>";

            //------------Execute Test---------------------------
            Guid shapedInputID = _sdlc.Shape(null, dlID, enDev2ArgumentType.Input, inputs, out errors, string.Empty);

            Assert.IsFalse(errors.HasErrors());
            IBinaryDataList bdl = _sdlc.FetchBinaryDataList(null, shapedInputID, out errors);
            Assert.AreEqual(bdl.ParentUID, dlID);

            IBinaryDataListEntry tmp;
            bdl.TryGetEntry("rs1", out tmp, out error);

            var cols = tmp.Columns;

            //------------Assert Results-------------------------

            Assert.AreEqual("f1", cols[0].ColumnName);

            // clean up ;)
            _sdlc.DeleteDataListByID(bdl.ParentUID, false);

        }


        [TestMethod]
        public void ShapeInput_With_RecordsetAndScalar_Expect_New_DataList() {
            ErrorResultTO errors;
            byte[] data = (TestHelper.ConvertStringToByteArray("<DataList><scalar1>scalar3</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1><rs2><f1a>rs2.f1</f1a></rs2><scalar2>scalar</scalar2></DataList>"));
            Guid dlID = _sdlc.ConvertTo(null, xmlFormat, data, _dataListWellformedMult, out errors);
            string error;

            const string inputs = @"<Inputs><Input Name=""f1"" Source=""[[rs2().f1a]]"" Recordset=""rs1"" /><Input Name=""scalar1"" Source=""[[scalar2]]"" /></Inputs>";

            Guid shapedInputID = _sdlc.Shape(null, dlID, enDev2ArgumentType.Input, inputs, out errors, string.Empty);

            Assert.IsFalse(errors.HasErrors());
            IBinaryDataList bdl = _sdlc.FetchBinaryDataList(null, shapedInputID, out errors);
            Assert.AreEqual(bdl.ParentUID, dlID);
            
            IBinaryDataListEntry tmp;
            bdl.TryGetEntry("rs1", out tmp, out error);
            Assert.AreEqual("rs2.f1", tmp.TryFetchRecordsetColumnAtIndex("f1", 1, out error).TheValue);
            // Check scalar value
            bdl.TryGetEntry("scalar1", out tmp, out error);

            var res = tmp.FetchScalar().TheValue;

            Assert.AreEqual("scalar", res);
        }

        [TestMethod]
        public void ShapeInput_With_RecordsetStarIndexAndScalar_Expect_New_DataList() {
            ErrorResultTO errors;
            byte[] data = (TestHelper.ConvertStringToByteArray("<DataList><scalar1>scalar3</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1><rs2><f1a>rs2.f1.1</f1a></rs2><rs2><f1a>rs2.f1.2</f1a></rs2><rs2><f1a>rs2.f1.3</f1a></rs2><scalar2>scalar</scalar2></DataList>"));
            Guid dlID = _sdlc.ConvertTo(null, xmlFormat, data, _dataListWellformedMult, out errors);
            string error;

            const string inputs = @"<Inputs><Input Name=""f1"" Source=""[[rs2(*).f1a]]"" Recordset=""rs1"" /><Input Name=""scalar1"" Source=""[[scalar2]]"" /></Inputs>";

            Guid shapedInputID = _sdlc.Shape(null, dlID, enDev2ArgumentType.Input, inputs, out errors, string.Empty);

            Assert.IsFalse(errors.HasErrors());
            IBinaryDataList bdl = _sdlc.FetchBinaryDataList(null, shapedInputID, out errors);
            Assert.AreEqual(bdl.ParentUID, dlID);

            IBinaryDataListEntry tmp;
            bdl.TryGetEntry("rs1", out tmp, out error);
            Assert.AreEqual("rs2.f1.3", tmp.TryFetchRecordsetColumnAtIndex("f1", 3, out error).TheValue);
            // Check scalar value
            bdl.TryGetEntry("scalar1", out tmp, out error);

            var res = tmp.FetchScalar().TheValue;

            Assert.AreEqual("scalar", res);
        }

        [TestMethod]
        public void ShapeInput_With_RecordsetStarIndexAndScalar_WithDefaultValue_Expect_New_DataList() {
            ErrorResultTO errors;
            byte[] data = (TestHelper.ConvertStringToByteArray("<DataList><scalar1>scalar3</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1><rs2><f1a>rs2.f1.1</f1a></rs2><rs2><f1a>rs2.f1.2</f1a></rs2><rs2><f1a>rs2.f1.3</f1a></rs2><scalar2>scalar</scalar2></DataList>"));
            Guid dlID = _sdlc.ConvertTo(null, xmlFormat, data, _dataListWellformedMult, out errors);
            string error;

            const string inputs = @"<Inputs><Input Name=""f1"" Source=""[[rs2(*).f1a]]"" Recordset=""rs1"" /><Input Name=""scalar1"" Source="""" DefaultValue=""Default_Scalar""/></Inputs>";

            Guid shapedInputID = _sdlc.Shape(null, dlID, enDev2ArgumentType.Input, inputs, out errors, string.Empty);

            Assert.IsFalse(errors.HasErrors());
            IBinaryDataList bdl = _sdlc.FetchBinaryDataList(null, shapedInputID, out errors);
            Assert.AreEqual(bdl.ParentUID, dlID);

            IBinaryDataListEntry tmp;
            bdl.TryGetEntry("rs1", out tmp, out error);
            Assert.AreEqual("rs2.f1.3", tmp.TryFetchRecordsetColumnAtIndex("f1", 3, out error).TheValue);
            // Check scalar value
            bdl.TryGetEntry("scalar1", out tmp, out error);

            var res = tmp.FetchScalar().TheValue;

            Assert.AreEqual("Default_Scalar", res);
        }

        #endregion

        #region Positive Output Shape Test
        [TestMethod]
        public void ShapeOutput_With_RecordsetAndScalar_Expect_Merge() {
            ErrorResultTO errors;
            string error;

            // Create parent dataList
            const string dataListWellformedMultData = "<DataList><scalar1>p1</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1><rs2><f1a>rs2.f1</f1a></rs2><scalar2>p2</scalar2></DataList>";
            byte[] data = (TestHelper.ConvertStringToByteArray(dataListWellformedMultData));
            Guid parentID = _sdlc.ConvertTo(null, xmlFormat, data, _dataListWellformedMult, out errors);
            // Create child list to branch from -- Emulate Input shaping
            const string dataListWellformedComplexData = "<DataList><scalar1>c1</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1><rs2><f1a>rs2.f1.1</f1a></rs2><rs2><f1a>rs2.f1.2</f1a></rs2><rs2><f1a>rs2.f1.3</f1a></rs2><scalar2>c2</scalar2></DataList>";
            data = (TestHelper.ConvertStringToByteArray(dataListWellformedComplexData));
            Guid dlID = _sdlc.ConvertTo(null, xmlFormat, data, _dataListWellformedMult, out errors);
            // Set ParentID
            _sdlc.SetParentUID(dlID, parentID, out errors);

           
            // Value is target shape
            const string outputs = @"<Outputs><Output Name=""f1a"" MapsTo=""f1a"" Value=""[[rs1().f1]]"" Recordset=""rs2"" /><Output Name=""scalar2"" MapsTo=""scalar2"" Value=""[[scalar1]]"" /></Outputs>";

            Guid shapedOutputID = _sdlc.Shape(null, dlID, enDev2ArgumentType.Output, outputs, out errors, string.Empty);

            Assert.IsFalse(errors.HasErrors());
            IBinaryDataList bdl = _sdlc.FetchBinaryDataList(null, dlID, out errors);
            Assert.AreEqual(bdl.ParentUID, parentID);

            bdl = _sdlc.FetchBinaryDataList(null, shapedOutputID, out errors);

            IBinaryDataListEntry tmp;
            bdl.TryGetEntry("rs1", out tmp, out error);
            Assert.AreEqual("rs2.f1.3", tmp.TryFetchRecordsetColumnAtIndex("f1", 5, out error).TheValue);
            // Check scalar value
            bdl.TryGetEntry("scalar1", out tmp, out error);

            var res = tmp.FetchScalar().TheValue;

            _sdlc.DeleteDataListByID(parentID, false);
            _sdlc.DeleteDataListByID(dlID, false);

            Assert.AreEqual("c2", res);
        }

        [TestMethod]
        public void ShapeOutput_With_RecordsetStarIndexAndScalar_Expect_Merge() {

            ErrorResultTO errors;
            string error;

            // Create parent dataList
            byte[] data = (TestHelper.ConvertStringToByteArray("<DataList><scalar1>scalar3</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1><rs2><f1a>rs2.f1</f1a></rs2><scalar2>scalar</scalar2></DataList>"));
            Guid parentID = _sdlc.ConvertTo(null, xmlFormat, data, _dataListWellformedMult, out errors);
            // Create child list to branch from -- Emulate Input shaping
            data = (TestHelper.ConvertStringToByteArray("<DataList><scalar1>scalar3</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1><rs2><f1a>rs2.f1.1</f1a></rs2><rs2><f1a>rs2.f1.2</f1a></rs2><rs2><f1a>rs2.f1.3</f1a></rs2><scalar2>scalar</scalar2></DataList>"));
            Guid dlID = _sdlc.ConvertTo(null, xmlFormat, data, _dataListWellformedMult, out errors);
            // Set ParentID
            _sdlc.SetParentUID(dlID, parentID, out errors);


            // Value is target shape
            const string outputs = @"<Outputs><Output Name=""f1a"" MapsTo=""f1a"" Value=""[[rs1(*).f1]]"" Recordset=""rs2"" /><Output Name=""scalar2"" MapsTo=""scalar2"" Value=""[[scalar1]]"" /></Outputs>";

            Guid shapedOutputID = _sdlc.Shape(null, dlID, enDev2ArgumentType.Output, outputs, out errors, string.Empty);

            Assert.IsFalse(errors.HasErrors());
            IBinaryDataList bdl = _sdlc.FetchBinaryDataList(null, dlID, out errors);
            Assert.AreEqual(bdl.ParentUID, parentID);

            bdl = _sdlc.FetchBinaryDataList(null, shapedOutputID, out errors);

            IBinaryDataListEntry tmp;
            bdl.TryGetEntry("rs1", out tmp, out error);
            Assert.AreEqual("rs2.f1.3", tmp.TryFetchRecordsetColumnAtIndex("f1", 3, out error).TheValue);
            // Check scalar value
            bdl.TryGetEntry("scalar1", out tmp, out error);

            var res = tmp.FetchScalar().TheValue;

            _sdlc.DeleteDataListByID(parentID, false);
            _sdlc.DeleteDataListByID(dlID, false);

            Assert.AreEqual("scalar", res);
        }

        [TestMethod]
        public void ShapeOutput_With_RecordsetNumericIndexAndScalar_Expect_Merge() {

            ErrorResultTO errors;
            string error;

            // Create parent dataList
            byte[] data = (TestHelper.ConvertStringToByteArray("<DataList><scalar1>scalar3</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1><rs2><f1a>rs2.f1</f1a></rs2><scalar2>scalar</scalar2></DataList>"));
            Guid parentID = _sdlc.ConvertTo(null, xmlFormat, data, _dataListWellformedMult, out errors);
            // Create child list to branch from -- Emulate Input shaping
            data = (TestHelper.ConvertStringToByteArray("<DataList><scalar1>scalar3</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1><rs2><f1a>rs2.f1.1</f1a></rs2><rs2><f1a>rs2.f1.2</f1a></rs2><rs2><f1a>rs2.f1.3</f1a></rs2><scalar2>scalar</scalar2></DataList>"));
            Guid dlID = _sdlc.ConvertTo(null, xmlFormat, data, _dataListWellformedMult, out errors);
            // Set ParentID
            _sdlc.SetParentUID(dlID, parentID, out errors);


            // Value is target shape
            const string outputs = @"<Outputs><Output Name=""f1a"" MapsTo=""f1a"" Value=""[[rs1(1).f1]]"" Recordset=""rs2"" /><Output Name=""scalar2"" MapsTo=""scalar2"" Value=""[[scalar1]]"" /></Outputs>";

            Guid shapedOutputID = _sdlc.Shape(null, dlID, enDev2ArgumentType.Output, outputs, out errors, string.Empty);

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

            _sdlc.DeleteDataListByID(parentID, false);
            _sdlc.DeleteDataListByID(dlID, false);

            Assert.AreEqual("scalar", res);
        }

        #endregion
    }
}
