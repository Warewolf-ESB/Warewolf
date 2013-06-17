using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Server.Datalist;
using Dev2.DataList.Contract;
using Dev2.Common;
using Dev2.DataList.Contract.Binary_Objects;

namespace Dev2.DynamicServices.Test.BinaryDataList {
    /// <summary>
    /// Summary description for ServerDataListCompilerTest
    /// </summary>
    [TestClass]
    public class ServerDataListCompilerTest {
        public ServerDataListCompilerTest() {
            //
            // TODO: Add constructor logic here
            //
        }

        private readonly IServerDataListCompiler sdlc = DataListFactory.CreateServerDataListCompiler();
        
        private static readonly string _dataListWellformed = "<DataList><scalar1/><rs1><f1/><f2/></rs1><scalar2/></DataList>";
        private static readonly string _dataListWellformedData = "<DataList><scalar1>1</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1><scalar2/></DataList>";
        private static readonly string _dataListWellformedDataWithInfinateEvals = "<DataList><scalar1>[[scalar2]]</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1><scalar2>[[scalar1]]</scalar2></DataList>";
        private static readonly string _dataListWellformedMult = "<DataList><scalar1/><scalar3/><rs1><f1/><f2/></rs1><rs2><f1a/></rs2><scalar2/></DataList>";

        private static DataListFormat xmlFormat = DataListFormat.CreateFormat(GlobalConstants._XML);
        private static DataListFormat binFormat = DataListFormat.CreateFormat(GlobalConstants._BINARY);


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext {
            get {
                return testContextInstance;
            }
            set {
                testContextInstance = value;
            }
        }

        #region Postive Evalaute Test

        [TestMethod]
        public void Evaluate_UserScalar_Expect_Value() {
            ErrorResultTO errors = new ErrorResultTO();
            byte[] data = (TestHelper.ConvertStringToByteArray(_dataListWellformedData));
            Guid dlID = sdlc.ConvertTo(null, xmlFormat, data, _dataListWellformed, out errors);
            
            IBinaryDataListEntry result = sdlc.Evaluate(null, dlID, Dev2.DataList.Contract.enActionType.User, "[[scalar1]]", out errors);

            Assert.IsFalse(errors.HasErrors());
            Assert.AreEqual("1", result.FetchScalar().TheValue);

        }

        [TestMethod]
        public void Evaluate_UserScalar_Same_Value_Inside_Expect_Value()
        {
            ErrorResultTO errors = new ErrorResultTO();
            byte[] data = (TestHelper.ConvertStringToByteArray(_dataListWellformedDataWithInfinateEvals));          
            Guid dlID = sdlc.ConvertTo(null, xmlFormat, data, _dataListWellformed, out errors);
            IBinaryDataList dl = sdlc.FetchBinaryDataList(null,dlID,out errors);


            dl.FetchAllEntries();
            sdlc.UpsertSystemTag(dlID, enSystemTag.Dev2DesignTimeBinding, "true", out errors);
            IBinaryDataListEntry result = sdlc.Evaluate(null, dlID, DataList.Contract.enActionType.User, "[[scalar1]]", out errors);

            Assert.IsFalse(errors.HasErrors());
            Assert.AreEqual("[[scalar1]]", result.FetchScalar().TheValue);

        }


        // Travis.Frisinger -  Bug 8608
        [TestMethod]
        public void Evaluate_UserRecordsetLastIndex_Expect_Value() {
            ErrorResultTO errors = new ErrorResultTO();
            string error = string.Empty;
            
            byte[] data = (TestHelper.ConvertStringToByteArray(_dataListWellformedData));
            Guid dlID = sdlc.ConvertTo(null, xmlFormat, data, _dataListWellformed, out errors);

            IBinaryDataListEntry result = sdlc.Evaluate(null, dlID, Dev2.DataList.Contract.enActionType.User, "[[rs1().f1]]", out errors);

            Assert.IsFalse(errors.HasErrors());
            Assert.AreEqual("f1.2", result.FetchScalar().TheValue);

        }

        // Travis.Frisinger - Bug 8608
        [TestMethod]
        public void Evaluate_UserRecordsetWithEvaluatedIndex_Expect_Value() {
            ErrorResultTO errors = new ErrorResultTO();
            byte[] data = (TestHelper.ConvertStringToByteArray(_dataListWellformedData));
            Guid dlID = sdlc.ConvertTo(null, xmlFormat, data, _dataListWellformed, out errors);
            string error = string.Empty;

            IBinaryDataListEntry result = sdlc.Evaluate(null, dlID, Dev2.DataList.Contract.enActionType.User, "[[rs1([[scalar1]]).f1]]", out errors);

            Assert.IsFalse(errors.HasErrors());
            Assert.AreEqual("f1.1", result.FetchScalar().TheValue);

        }

        [TestMethod]
        public void Evaluate_UserRecordsetWithStarIndex_Expect_Value() {
            ErrorResultTO errors = new ErrorResultTO();
            byte[] data = (TestHelper.ConvertStringToByteArray(_dataListWellformedData));
            Guid dlID = sdlc.ConvertTo(null, xmlFormat, data, _dataListWellformed, out errors);
            string error = string.Empty;

            IBinaryDataListEntry result = sdlc.Evaluate(null, dlID, Dev2.DataList.Contract.enActionType.User, "[[rs1(*).f1]]", out errors);

            Assert.IsFalse(errors.HasErrors());
            Assert.AreEqual(2, result.ItemCollectionSize());
            int idx = result.FetchLastRecordsetIndex();
            Assert.AreEqual("f1.2", (result.FetchRecordAt(idx, out error))[0].TheValue);

        }

        [TestMethod]
        public void Evaluate_UserPartialRecursiveExpression_Expect_Value() {
            ErrorResultTO errors = new ErrorResultTO();
            byte[] data = (TestHelper.ConvertStringToByteArray("<DataList><scalar1>scalar3</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1><rs2><f1a>rs2.f1</f1a></rs2><scalar2>scalar</scalar2></DataList>"));
            Guid dlID = sdlc.ConvertTo(null, xmlFormat, data, _dataListWellformedMult, out errors);
            string error = string.Empty;

            IBinaryDataListEntry result = sdlc.Evaluate(null, dlID, DataList.Contract.enActionType.User, "[[[[scalar2]]1]]", out errors);

            Assert.IsFalse(errors.HasErrors());
            var binaryDataListItem = result.FetchScalar();
            var theValue = binaryDataListItem.TheValue;
            Assert.AreEqual("scalar3", theValue);

        }

        [TestMethod]
        public void EvaluateRecordsetWithStarIndexAndStaticDataExpectStaticDataAppendedToAllRecordsetEntries()
        {
            ErrorResultTO errors = new ErrorResultTO();
            byte[] data = (TestHelper.ConvertStringToByteArray("<DataList><scalar1></scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1></DataList>"));
            Guid dlID = sdlc.ConvertTo(null, xmlFormat, data, _dataListWellformedMult, out errors);
            string error = string.Empty;

            IBinaryDataListEntry result = sdlc.Evaluate(null, dlID, DataList.Contract.enActionType.User, "[[rs1(*).f1]] some cool static data ;)", out errors);

            Assert.AreEqual("f1.1 some cool static data ;)", (result.FetchRecordAt(1, out error))[0].TheValue);
            Assert.AreEqual("f1.2 some cool static data ;)", (result.FetchRecordAt(2, out error))[0].TheValue);

        }

        [TestMethod]
        public void EvaluateRecordsetWithStarIndexAndStaticDataAndScalarExpectStaticDataAppendedToAllRecordsetEntries()
        {
            ErrorResultTO errors = new ErrorResultTO();
            byte[] data = (TestHelper.ConvertStringToByteArray("<DataList><scalar1>even more static data ;)</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1></DataList>"));
            Guid dlID = sdlc.ConvertTo(null, xmlFormat, data, _dataListWellformedMult, out errors);
            string error = string.Empty;

            IBinaryDataListEntry result = sdlc.Evaluate(null, dlID, DataList.Contract.enActionType.User, "[[rs1(*).f1]] some cool static data ;) [[scalar1]]", out errors);

            Assert.AreEqual("f1.1 some cool static data ;) even more static data ;)", (result.FetchRecordAt(1, out error))[0].TheValue);
            Assert.AreEqual("f1.2 some cool static data ;) even more static data ;)", (result.FetchRecordAt(2, out error))[0].TheValue);

        }
         
        [TestMethod]
        public void EvaluateRecordsetWithStarIndexAndStaticDataAndScalarPreFixExpectStaticDataAppendedToAllRecordsetEntries()
        {
            ErrorResultTO errors = new ErrorResultTO();
            byte[] data = (TestHelper.ConvertStringToByteArray("<DataList><scalar1>even more static data ;)</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1></DataList>"));
            Guid dlID = sdlc.ConvertTo(null, xmlFormat, data, _dataListWellformedMult, out errors);
            string error = string.Empty;

            IBinaryDataListEntry result = sdlc.Evaluate(null, dlID, DataList.Contract.enActionType.User, "[[scalar1]] [[rs1(*).f1]] some cool static data ;)", out errors);

            Assert.AreEqual("even more static data ;) f1.1 some cool static data ;)", (result.FetchRecordAt(1, out error))[0].TheValue);
            Assert.AreEqual("even more static data ;) f1.2 some cool static data ;)", (result.FetchRecordAt(2, out error))[0].TheValue);

        }

        [TestMethod]
        public void EvaluateRecordsetWithStarIndexAndStaticDataAndRecordsetWithIndexPreFixExpectStaticDataAppendedToAllRecordsetEntries()
        {
            ErrorResultTO errors = new ErrorResultTO();
            byte[] data = (TestHelper.ConvertStringToByteArray("<DataList><scalar1>even more static data ;)</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1><rs2><f1a>recordset data ;)</f1a></rs2></DataList>"));
            Guid dlID = sdlc.ConvertTo(null, xmlFormat, data, _dataListWellformedMult, out errors);
            string error = string.Empty;

            IBinaryDataListEntry result = sdlc.Evaluate(null, dlID, DataList.Contract.enActionType.User, "[[rs2(1).f1a]] [[rs1(*).f1]] some cool static data ;)", out errors);

            Assert.AreEqual("recordset data ;) f1.1 some cool static data ;)", (result.FetchRecordAt(1, out error))[0].TheValue);
            Assert.AreEqual("recordset data ;) f1.2 some cool static data ;)", (result.FetchRecordAt(2, out error))[0].TheValue);

        }

        // Travis
        [TestMethod]
        public void EvaluateRecordsetWithStarIndexAgainstAnotherRecordsetWithStarExpectCartesianProduct()
        {
            ErrorResultTO errors = new ErrorResultTO();
            byte[] data = (TestHelper.ConvertStringToByteArray("<DataList><scalar1>even more static data ;)</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1><rs2><f1a>recordset data ;)</f1a></rs2></DataList>"));
            Guid dlID = sdlc.ConvertTo(null, xmlFormat, data, _dataListWellformedMult, out errors);
            string error = string.Empty;

            IBinaryDataListEntry result = sdlc.Evaluate(null, dlID, DataList.Contract.enActionType.User, "[[rs2(*).f1a]] [[rs1(*).f1]] [[rs1(*).f1]]", out errors);

            Assert.AreEqual("recordset data ;) f1.1 some cool static data ;)", (result.FetchRecordAt(1, out error))[0].TheValue);
            Assert.AreEqual("recordset data ;) f1.2 some cool static data ;)", (result.FetchRecordAt(2, out error))[0].TheValue);

        }

        // Travis
        [TestMethod]
        public void EvaluateRecordsetWithStarIndexAgainstAnotherRecordsetWithStarAndScalarDataExpectCartesianProductWithScalar()
        {
            ErrorResultTO errors = new ErrorResultTO();
            byte[] data = (TestHelper.ConvertStringToByteArray("<DataList><scalar1>even more static data ;)</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1><rs2><f1a>recordset data ;)</f1a></rs2></DataList>"));
            Guid dlID = sdlc.ConvertTo(null, xmlFormat, data, _dataListWellformedMult, out errors);
            string error = string.Empty;

            IBinaryDataListEntry result = sdlc.Evaluate(null, dlID, DataList.Contract.enActionType.User, "[[rs2(*).f1a]] [[rs1(*).f1]] some cool static data ;)", out errors);

            Assert.AreEqual("recordset data ;) f1.1 some cool static data ;)", (result.FetchRecordAt(1, out error))[0].TheValue);
            Assert.AreEqual("recordset data ;) f1.2 some cool static data ;)", (result.FetchRecordAt(2, out error))[0].TheValue);

        }  

        #endregion Positive Evaluate Test

        #region Positive Upsert Test
        [TestMethod]
        public void UpsertScalar_Expect_Insert() {
            ErrorResultTO errors = new ErrorResultTO();
            byte[] data = (TestHelper.ConvertStringToByteArray("<DataList><scalar1>scalar3</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1><rs2><f1a>rs2.f1</f1a></rs2><scalar2>scalar</scalar2></DataList>"));
            Guid dlID = sdlc.ConvertTo(null, xmlFormat, data, _dataListWellformedMult, out errors);
            string error = string.Empty;


            string myDate = sdlc.ConvertFrom(null, dlID, enTranslationDepth.Data, DataListFormat.CreateFormat(GlobalConstants._XML), out errors).FetchAsString();

            if(myDate != string.Empty)
            {
                Console.WriteLine(myDate);
            }

            IBinaryDataListEntry upsertEntry;
            IBinaryDataListItem toUpsert = Dev2BinaryDataListFactory.CreateBinaryItem("test_upsert_value", "scalar2");
            upsertEntry = Dev2BinaryDataListFactory.CreateEntry("scalar2",string.Empty, dlID);
            upsertEntry.TryPutScalar(toUpsert, out error);

            Guid upsertID = sdlc.Upsert(null, dlID, "[[scalar1]]", upsertEntry, out errors);

            Assert.AreEqual(upsertID, dlID);
            Assert.IsFalse(errors.HasErrors());

            IBinaryDataList bdl = sdlc.FetchBinaryDataList(null, upsertID, out errors);
            IBinaryDataListEntry tmp;
            bdl.TryGetEntry("scalar1", out tmp, out error);
            Assert.AreEqual("test_upsert_value", tmp.FetchScalar().TheValue);
        }

        [TestMethod]
        public void UpsertRecursiveEvaluatedScalar_Expect_Insert() {
            ErrorResultTO errors = new ErrorResultTO();
            byte[] data = (TestHelper.ConvertStringToByteArray("<DataList><scalar1>scalar3</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1><rs2><f1a>rs2.f1</f1a></rs2><scalar2>scalar</scalar2></DataList>"));
            Guid dlID = sdlc.ConvertTo(null, xmlFormat, data, _dataListWellformedMult, out errors);
            string error = string.Empty;

            IBinaryDataListEntry upsertEntry;
            IBinaryDataListItem toUpsert = Dev2BinaryDataListFactory.CreateBinaryItem("test_upsert_value", "scalar2");
            upsertEntry = Dev2BinaryDataListFactory.CreateEntry("scalar2", string.Empty, dlID);
            upsertEntry.TryPutScalar(toUpsert, out error);

            Guid upsertID = sdlc.Upsert(null , dlID, "[[[[scalar1]]]]", upsertEntry, out errors);

            Assert.AreEqual(upsertID, dlID);
            Assert.IsFalse(errors.HasErrors());

            IBinaryDataList bdl = sdlc.FetchBinaryDataList(null, upsertID, out errors);
            IBinaryDataListEntry tmp;
            bdl.TryGetEntry("scalar3", out tmp, out error);
            Assert.AreEqual("test_upsert_value", tmp.FetchScalar().TheValue);
        }

        [TestMethod]
        public void UpsertToRecordsetWithNumericIndex_Expect_Insert() {
            ErrorResultTO errors = new ErrorResultTO();
            byte[] data = (TestHelper.ConvertStringToByteArray("<DataList><scalar1>scalar3</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1><rs2><f1a>rs2.f1</f1a></rs2><scalar2>scalar</scalar2></DataList>"));
            Guid dlID = sdlc.ConvertTo(null, xmlFormat, data, _dataListWellformedMult, out errors);
            string error = string.Empty;

            IBinaryDataListEntry upsertEntry;
            IBinaryDataListItem toUpsert = Dev2BinaryDataListFactory.CreateBinaryItem("test_upsert_value", "scalar2");
            upsertEntry = Dev2BinaryDataListFactory.CreateEntry("scalar2", string.Empty,dlID);
            upsertEntry.TryPutScalar(toUpsert, out error);

            Guid upsertID = sdlc.Upsert(null, dlID, "[[rs1(5).f2]]", upsertEntry, out errors);

            Assert.AreEqual(upsertID, dlID);
            Assert.IsFalse(errors.HasErrors());

            IBinaryDataList bdl = sdlc.FetchBinaryDataList(null, upsertID, out errors);
            IBinaryDataListEntry tmp;
            bdl.TryGetEntry("rs1", out tmp, out error);
            Assert.AreEqual("test_upsert_value", tmp.TryFetchRecordsetColumnAtIndex("f2", 5, out error).TheValue);
        }

        [TestMethod]
        public void UpsertToRecordsetWithBlankIndex_Expect_Append() {
            ErrorResultTO errors = new ErrorResultTO();
            byte[] data = (TestHelper.ConvertStringToByteArray("<DataList><scalar1>scalar3</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1><rs2><f1a>rs2.f1</f1a></rs2><scalar2>scalar</scalar2></DataList>"));
            Guid dlID = sdlc.ConvertTo(null, xmlFormat, data, _dataListWellformedMult, out errors);
            string error = string.Empty;

            IBinaryDataListEntry upsertEntry;
            IBinaryDataListItem toUpsert = Dev2BinaryDataListFactory.CreateBinaryItem("test_upsert_value", "scalar2");
            upsertEntry = Dev2BinaryDataListFactory.CreateEntry("scalar2", string.Empty,dlID);
            upsertEntry.TryPutScalar(toUpsert, out error);

            Guid upsertID = sdlc.Upsert(null, dlID, "[[rs1().f2]]", upsertEntry, out errors);

            Assert.AreEqual(upsertID, dlID);
            Assert.IsFalse(errors.HasErrors());

            IBinaryDataList bdl = sdlc.FetchBinaryDataList(null, upsertID, out errors);
            IBinaryDataListEntry tmp;
            bdl.TryGetEntry("rs1", out tmp, out error);
            Assert.AreEqual("test_upsert_value", tmp.TryFetchRecordsetColumnAtIndex("f2", 3, out error).TheValue);
        }

        [TestMethod]
        public void UpsertToRecordsetWithStarIndex_Expect_Append() {
            ErrorResultTO errors = new ErrorResultTO();
            byte[] data = (TestHelper.ConvertStringToByteArray("<DataList><scalar1>scalar3</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1><rs2><f1a>rs2.f1</f1a></rs2><scalar2>scalar</scalar2></DataList>"));
            Guid dlID = sdlc.ConvertTo(null, xmlFormat, data, _dataListWellformedMult, out errors);
            string error = string.Empty;

            data = (TestHelper.ConvertStringToByteArray(_dataListWellformedData));
            Guid uID = sdlc.ConvertTo(null, xmlFormat, data, _dataListWellformed, out errors);

            //IBinaryDataList tmp = sdlc.FetchBinaryDataList(null, uID, out errors);
            IBinaryDataListEntry toUpsert = sdlc.Evaluate(null, uID, Dev2.DataList.Contract.enActionType.User, "[[rs1(*).f1]]", out errors);

            //tmp.TryGetEntry("rs1", out toUpsert, out error);
            Guid upsertID = sdlc.Upsert(null, dlID, "[[rs1(*).f1]]", toUpsert, out errors);

            Assert.AreEqual(upsertID, dlID);
            Assert.IsFalse(errors.HasErrors());

            IBinaryDataList bdl = sdlc.FetchBinaryDataList(null, upsertID, out errors);
            IBinaryDataListEntry te;
            bdl.TryGetEntry("rs1", out te, out error);
            // f1.1, f1.2
            Assert.AreEqual("f1.2", te.TryFetchRecordsetColumnAtIndex("f1", 2, out error).TheValue);
        }

        #endregion

        #region Positive Input Shape Test
        [TestMethod]
        public void ShapeInput_With_RecordsetAndScalar_Expect_New_DataList() {
            ErrorResultTO errors = new ErrorResultTO();
            byte[] data = (TestHelper.ConvertStringToByteArray("<DataList><scalar1>scalar3</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1><rs2><f1a>rs2.f1</f1a></rs2><scalar2>scalar</scalar2></DataList>"));
            Guid dlID = sdlc.ConvertTo(null, xmlFormat, data, _dataListWellformedMult, out errors);
            string error = string.Empty;

            string inputs = @"<Inputs><Input Name=""f1"" Source=""[[rs2().f1a]]"" Recordset=""rs1"" /><Input Name=""scalar1"" Source=""[[scalar2]]"" /></Inputs>";

            Guid shapedInputID = sdlc.Shape(null, dlID, enDev2ArgumentType.Input, inputs, out errors);

            Assert.IsFalse(errors.HasErrors());
            IBinaryDataList bdl = sdlc.FetchBinaryDataList(null, shapedInputID, out errors);
            Assert.AreEqual(bdl.ParentUID, dlID);
            
            IBinaryDataListEntry tmp;
            bdl.TryGetEntry("rs1", out tmp, out error);
            Assert.AreEqual("rs2.f1", tmp.TryFetchRecordsetColumnAtIndex("f1", 1, out error).TheValue);
            // Check scalar value
            bdl.TryGetEntry("scalar1", out tmp, out error);
            Assert.AreEqual("scalar", tmp.FetchScalar().TheValue);
        }

        [TestMethod]
        public void ShapeInput_With_RecordsetStarIndexAndScalar_Expect_New_DataList() {
            ErrorResultTO errors = new ErrorResultTO();
            byte[] data = (TestHelper.ConvertStringToByteArray("<DataList><scalar1>scalar3</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1><rs2><f1a>rs2.f1.1</f1a></rs2><rs2><f1a>rs2.f1.2</f1a></rs2><rs2><f1a>rs2.f1.3</f1a></rs2><scalar2>scalar</scalar2></DataList>"));
            Guid dlID = sdlc.ConvertTo(null, xmlFormat, data, _dataListWellformedMult, out errors);
            string error = string.Empty;

            string inputs = @"<Inputs><Input Name=""f1"" Source=""[[rs2(*).f1a]]"" Recordset=""rs1"" /><Input Name=""scalar1"" Source=""[[scalar2]]"" /></Inputs>";

            Guid shapedInputID = sdlc.Shape(null, dlID, enDev2ArgumentType.Input, inputs, out errors);

            Assert.IsFalse(errors.HasErrors());
            IBinaryDataList bdl = sdlc.FetchBinaryDataList(null, shapedInputID, out errors);
            Assert.AreEqual(bdl.ParentUID, dlID);

            IBinaryDataListEntry tmp;
            bdl.TryGetEntry("rs1", out tmp, out error);
            Assert.AreEqual("rs2.f1.3", tmp.TryFetchRecordsetColumnAtIndex("f1", 3, out error).TheValue);
            // Check scalar value
            bdl.TryGetEntry("scalar1", out tmp, out error);
            Assert.AreEqual("scalar", tmp.FetchScalar().TheValue);
        }

        [TestMethod]
        public void ShapeInput_With_RecordsetStarIndexAndScalar_WithDefaultValue_Expect_New_DataList() {
            ErrorResultTO errors = new ErrorResultTO();
            byte[] data = (TestHelper.ConvertStringToByteArray("<DataList><scalar1>scalar3</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1><rs2><f1a>rs2.f1.1</f1a></rs2><rs2><f1a>rs2.f1.2</f1a></rs2><rs2><f1a>rs2.f1.3</f1a></rs2><scalar2>scalar</scalar2></DataList>"));
            Guid dlID = sdlc.ConvertTo(null, xmlFormat, data, _dataListWellformedMult, out errors);
            string error = string.Empty;

            string inputs = @"<Inputs><Input Name=""f1"" Source=""[[rs2(*).f1a]]"" Recordset=""rs1"" /><Input Name=""scalar1"" Source="""" DefaultValue=""Default_Scalar""/></Inputs>";

            Guid shapedInputID = sdlc.Shape(null, dlID, enDev2ArgumentType.Input, inputs, out errors);

            Assert.IsFalse(errors.HasErrors());
            IBinaryDataList bdl = sdlc.FetchBinaryDataList(null, shapedInputID, out errors);
            Assert.AreEqual(bdl.ParentUID, dlID);

            IBinaryDataListEntry tmp;
            bdl.TryGetEntry("rs1", out tmp, out error);
            Assert.AreEqual("rs2.f1.3", tmp.TryFetchRecordsetColumnAtIndex("f1", 3, out error).TheValue);
            // Check scalar value
            bdl.TryGetEntry("scalar1", out tmp, out error);
            Assert.AreEqual("Default_Scalar", tmp.FetchScalar().TheValue);
        }

        #endregion

        #region Positive Output Shape Test
        [TestMethod]
        public void ShapeOutput_With_RecordsetAndScalar_Expect_Merge() {
            ErrorResultTO errors = new ErrorResultTO();
            string error = string.Empty;

            // Create parent dataList
            var dataListWellformedMultData = "<DataList><scalar1>p1</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1><rs2><f1a>rs2.f1</f1a></rs2><scalar2>p2</scalar2></DataList>";
            byte[] data = (TestHelper.ConvertStringToByteArray(dataListWellformedMultData));
            Guid parentID = sdlc.ConvertTo(null, xmlFormat, data, _dataListWellformedMult, out errors);
            // Create child list to branch from -- Emulate Input shaping
            var dataListWellformedComplexData = "<DataList><scalar1>c1</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1><rs2><f1a>rs2.f1.1</f1a></rs2><rs2><f1a>rs2.f1.2</f1a></rs2><rs2><f1a>rs2.f1.3</f1a></rs2><scalar2>c2</scalar2></DataList>";
            data = (TestHelper.ConvertStringToByteArray(dataListWellformedComplexData));
            Guid dlID = sdlc.ConvertTo(null, xmlFormat, data, _dataListWellformedMult, out errors);
            // Set ParentID
            sdlc.SetParentUID(dlID, parentID, out errors);

           
            // Value is target shape
            string outputs = @"<Outputs><Output Name=""f1a"" MapsTo=""f1a"" Value=""[[rs1().f1]]"" Recordset=""rs2"" /><Output Name=""scalar2"" MapsTo=""scalar2"" Value=""[[scalar1]]"" /></Outputs>";

            Guid shapedOutputID = sdlc.Shape(null, dlID, enDev2ArgumentType.Output, outputs, out errors);

            Assert.IsFalse(errors.HasErrors());
            IBinaryDataList bdl = sdlc.FetchBinaryDataList(null, dlID, out errors);
            Assert.AreEqual(bdl.ParentUID, parentID);

            bdl = sdlc.FetchBinaryDataList(null, shapedOutputID, out errors);

            IBinaryDataListEntry tmp;
            bdl.TryGetEntry("rs1", out tmp, out error);
            Assert.AreEqual("rs2.f1.3", tmp.TryFetchRecordsetColumnAtIndex("f1", 5, out error).TheValue);
            // Check scalar value
            bdl.TryGetEntry("scalar1", out tmp, out error);
            Assert.AreEqual("c2", tmp.FetchScalar().TheValue);
        }

        [TestMethod]
        public void ShapeOutput_With_RecordsetStarIndexAndScalar_Expect_Merge() {

            ErrorResultTO errors = new ErrorResultTO();
            string error = string.Empty;

            // Create parent dataList
            byte[] data = (TestHelper.ConvertStringToByteArray("<DataList><scalar1>scalar3</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1><rs2><f1a>rs2.f1</f1a></rs2><scalar2>scalar</scalar2></DataList>"));
            Guid parentID = sdlc.ConvertTo(null, xmlFormat, data, _dataListWellformedMult, out errors);
            // Create child list to branch from -- Emulate Input shaping
            data = (TestHelper.ConvertStringToByteArray("<DataList><scalar1>scalar3</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1><rs2><f1a>rs2.f1.1</f1a></rs2><rs2><f1a>rs2.f1.2</f1a></rs2><rs2><f1a>rs2.f1.3</f1a></rs2><scalar2>scalar</scalar2></DataList>"));
            Guid dlID = sdlc.ConvertTo(null, xmlFormat, data, _dataListWellformedMult, out errors);
            // Set ParentID
            sdlc.SetParentUID(dlID, parentID, out errors);


            // Value is target shape
            string outputs = @"<Outputs><Output Name=""f1a"" MapsTo=""f1a"" Value=""[[rs1(*).f1]]"" Recordset=""rs2"" /><Output Name=""scalar2"" MapsTo=""scalar2"" Value=""[[scalar1]]"" /></Outputs>";

            Guid shapedOutputID = sdlc.Shape(null, dlID, enDev2ArgumentType.Output, outputs, out errors);

            Assert.IsFalse(errors.HasErrors());
            IBinaryDataList bdl = sdlc.FetchBinaryDataList(null, dlID, out errors);
            Assert.AreEqual(bdl.ParentUID, parentID);

            bdl = sdlc.FetchBinaryDataList(null, shapedOutputID, out errors);

            IBinaryDataListEntry tmp;
            bdl.TryGetEntry("rs1", out tmp, out error);
            Assert.AreEqual("rs2.f1.3", tmp.TryFetchRecordsetColumnAtIndex("f1", 3, out error).TheValue);
            // Check scalar value
            bdl.TryGetEntry("scalar1", out tmp, out error);
            Assert.AreEqual("scalar", tmp.FetchScalar().TheValue);
        }

        [TestMethod]
        public void ShapeOutput_With_RecordsetNumericIndexAndScalar_Expect_Merge() {

            ErrorResultTO errors = new ErrorResultTO();
            string error = string.Empty;

            // Create parent dataList
            byte[] data = (TestHelper.ConvertStringToByteArray("<DataList><scalar1>scalar3</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1><rs2><f1a>rs2.f1</f1a></rs2><scalar2>scalar</scalar2></DataList>"));
            Guid parentID = sdlc.ConvertTo(null, xmlFormat, data, _dataListWellformedMult, out errors);
            // Create child list to branch from -- Emulate Input shaping
            data = (TestHelper.ConvertStringToByteArray("<DataList><scalar1>scalar3</scalar1><rs1><f1>f1.1</f1></rs1><rs1><f1>f1.2</f1></rs1><rs2><f1a>rs2.f1.1</f1a></rs2><rs2><f1a>rs2.f1.2</f1a></rs2><rs2><f1a>rs2.f1.3</f1a></rs2><scalar2>scalar</scalar2></DataList>"));
            Guid dlID = sdlc.ConvertTo(null, xmlFormat, data, _dataListWellformedMult, out errors);
            // Set ParentID
            sdlc.SetParentUID(dlID, parentID, out errors);


            // Value is target shape
            string outputs = @"<Outputs><Output Name=""f1a"" MapsTo=""f1a"" Value=""[[rs1(1).f1]]"" Recordset=""rs2"" /><Output Name=""scalar2"" MapsTo=""scalar2"" Value=""[[scalar1]]"" /></Outputs>";

            Guid shapedOutputID = sdlc.Shape(null, dlID, enDev2ArgumentType.Output, outputs, out errors);

            Assert.IsFalse(errors.HasErrors());
            IBinaryDataList bdl = sdlc.FetchBinaryDataList(null, dlID, out errors);
            Assert.AreEqual(bdl.ParentUID, parentID);

            bdl = sdlc.FetchBinaryDataList(null, shapedOutputID, out errors);

            IBinaryDataListEntry tmp;
            bdl.TryGetEntry("rs1", out tmp, out error);
            Assert.AreEqual("rs2.f1.3", tmp.TryFetchRecordsetColumnAtIndex("f1", 1, out error).TheValue);
            // Check scalar value
            bdl.TryGetEntry("scalar1", out tmp, out error);
            Assert.AreEqual("scalar", tmp.FetchScalar().TheValue);
        }

        #endregion
    }
}
