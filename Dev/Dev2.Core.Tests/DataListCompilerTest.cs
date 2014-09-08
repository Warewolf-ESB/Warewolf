using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Dev2.Common.Interfaces.DataList.Contract;
using Dev2.Data.Binary_Objects;
using Dev2.Data.Factories;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Builders;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests
{
    /// <summary>
    /// Summary description for DataListCompilerTest
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class DataListCompilerTest
    {
        // ReSharper disable InconsistentNaming

        private IBinaryDataList _dl1;
        private IBinaryDataList _dl2;
        private ErrorResultTO _errors = new ErrorResultTO();
        private string _error;
        private IBinaryDataListEntry _entry;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Additional test attributes
        //Use TestInitialize to run code before running each test 
        [TestInitialize]
        public void MyTestInitialize()
        {
            string error;

            var dataListCompiler = DataListFactory.CreateDataListCompiler();

            _dl1 = Dev2BinaryDataListFactory.CreateDataList();
            _dl1.TryCreateScalarTemplate(string.Empty, "myScalar", "A scalar", true, out error);
            _dl1.TryCreateScalarValue("[[otherScalar]]", "myScalar", out error);

            _dl1.TryCreateScalarTemplate(string.Empty, "otherScalar", "A scalar", true, out error);
            _dl1.TryCreateScalarValue("testRegion", "otherScalar", out error);

            _dl1.TryCreateScalarTemplate(string.Empty, "scalar1", "A scalar", true, out error);
            _dl1.TryCreateScalarValue("foobar", "scalar1", out error);

            IList<Dev2Column> cols = new List<Dev2Column>();
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f1"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f2"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f3"));

            _dl1.TryCreateRecordsetTemplate("recset", "a recordset", cols, true, out error);

            _dl1.TryCreateRecordsetValue("r1.f1.value", "f1", "recset", 1, out error);
            _dl1.TryCreateRecordsetValue("r1.f2.value", "f2", "recset", 1, out error);
            _dl1.TryCreateRecordsetValue("r1.f3.value", "f3", "recset", 1, out error);

            _dl1.TryCreateRecordsetValue("r2.f1.value", "f1", "recset", 2, out error);
            _dl1.TryCreateRecordsetValue("r2.f2.value", "f2", "recset", 2, out error);
            _dl1.TryCreateRecordsetValue("r2.f3.value", "f3", "recset", 2, out error);

            // skip 3 ;)

            _dl1.TryCreateRecordsetValue("r4.f1.value", "f1", "recset", 4, out error);
            _dl1.TryCreateRecordsetValue("r4.f2.value", "f2", "recset", 4, out error);
            _dl1.TryCreateRecordsetValue("r4.f3.value", "f3", "recset", 4, out error);

            dataListCompiler.PushBinaryDataList(_dl1.UID, _dl1, out _errors);
            //_compiler.UpsertSystemTag(dl1.UID, enSystemTag.EvaluateIteration, "true", out errors);

            /*  list 2 */
            _dl2 = Dev2BinaryDataListFactory.CreateDataList();
            _dl2.TryCreateScalarTemplate(string.Empty, "idx", "A scalar", true, out error);
            _dl2.TryCreateScalarValue("1", "idx", out error);

            _dl2.TryCreateRecordsetTemplate("recset", "a recordset", cols, true, out error);

            _dl2.TryCreateRecordsetValue("r1.f1.value", "f1", "recset", 1, out error);
            _dl2.TryCreateRecordsetValue("r1.f2.value", "f2", "recset", 1, out error);
            _dl2.TryCreateRecordsetValue("r1.f3.value", "f3", "recset", 1, out error);

            _dl2.TryCreateRecordsetValue("r2.f1.value", "f1", "recset", 2, out error);
            _dl2.TryCreateRecordsetValue("r2.f2.value", "f2", "recset", 2, out error);
            _dl2.TryCreateRecordsetValue("r2.f3.value", "f3", "recset", 2, out error);

            dataListCompiler.PushBinaryDataList(_dl2.UID, _dl2, out _errors);
            //_compiler.UpsertSystemTag(dl2.UID, enSystemTag.EvaluateIteration, "true", out errors);
        }

        #endregion

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DataListCompiler_Evaluate")]
        public void DataListCompiler_Evaluate_ForNonPresentRecordsetIndex_BlankRowReturned()
        {
            //------------Setup for test--------------------------
            var dlc = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;
            string error;

            //------------Execute Test---------------------------
            var result = dlc.Evaluate(_dl1.UID, enActionType.User, "[[recset(3).f1]]", false, out errors);
            var res = result.TryFetchLastIndexedRecordsetUpsertPayload(out error);

            //------------Assert Results-------------------------

            DoNullVariableAssertion(res);

        }

        static void DoNullVariableAssertion(IBinaryDataListItem binaryDataListItem)
        {
            try
            {
                var val = binaryDataListItem.TheValue;
                Assert.IsNull(val);
            }
            catch(Exception e)
            {
                StringAssert.Contains(e.Message, string.Format("No Value assigned for: [[{0}]]", binaryDataListItem.DisplayValue));
            }
        }
        // Created by Michael for Bug 8597
        [TestMethod]
        public void HasErrors_Passed_Empty_GUID_Expected_No_NullReferenceException()
        {
            Guid id = Guid.Empty;

            var dataListCompiler = DataListFactory.CreateDataListCompiler();

            try
            {
                dataListCompiler.HasErrors(id);
            }
            catch(NullReferenceException)
            {
                Assert.Inconclusive("No NullReferenceException should be thrown.");
            }
        }

        [TestMethod]
        public void Iteration_Evaluation_Expect_Evaluation_For_1_Iteration()
        {
            // Iteration evaluation is tested via the shape method ;)
            var compiler = DataListFactory.CreateDataListCompiler();
            const string defs = @"<Inputs><Input Name=""scalar1"" Source=""[[myScalar]]"" /></Inputs>";
            Guid id = compiler.Shape(_dl1.UID, enDev2ArgumentType.Input, defs, out _errors);

            IBinaryDataList bdl = compiler.FetchBinaryDataList(id, out _errors);

            bdl.TryGetEntry("scalar1", out _entry, out _error);

            var res = _entry.FetchScalar().TheValue;

            Assert.AreEqual("[[otherScalar]]", res);

        }




        #region Generate Defintion Tests

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DataListCompiler_GenerateSerializableDefsFromDataList")]
        public void DataListCompiler_GenerateSerializableDefsFromDataList_WhenOutputs_ValidOutputs()
        {
            //------------Setup for test--------------------------

            const string datalistFragment = @"<DataList>
    <result Description="""" IsEditable=""True"" ColumnIODirection=""Output"" />
    <recset1 Description="""" IsEditable=""True"" ColumnIODirection=""None"">
      <f1 Description="""" IsEditable=""True"" ColumnIODirection=""None"" />
    </recset1>
    <recset2 Description="""" IsEditable=""True"" ColumnIODirection=""None"">
      <f2 Description="""" IsEditable=""True"" ColumnIODirection=""None"" />
    </recset2>
  </DataList>";

            var dataListCompiler = DataListFactory.CreateDataListCompiler();

            //------------Execute Test---------------------------

            var result = dataListCompiler.GenerateSerializableDefsFromDataList(datalistFragment, enDev2ColumnArgumentDirection.Output);

            //------------Assert Results-------------------------

            Assert.AreEqual(@"<Outputs><Output Name=""result"" MapsTo="""" Value="""" /></Outputs>", result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DataListCompiler_GenerateSerializableDefsFromDataList")]
        public void DataListCompiler_GenerateSerializableDefsFromDataList_WhenInputs_ValidInputs()
        {
            //------------Setup for test--------------------------

            const string datalistFragment = @"<DataList>
    <result Description="""" IsEditable=""True"" ColumnIODirection=""Input"" />
    <recset1 Description="""" IsEditable=""True"" ColumnIODirection=""None"">
      <f1 Description="""" IsEditable=""True"" ColumnIODirection=""None"" />
    </recset1>
    <recset2 Description="""" IsEditable=""True"" ColumnIODirection=""None"">
      <f2 Description="""" IsEditable=""True"" ColumnIODirection=""None"" />
    </recset2>
  </DataList>";

            var dataListCompiler = DataListFactory.CreateDataListCompiler();

            //------------Execute Test---------------------------

            var result = dataListCompiler.GenerateSerializableDefsFromDataList(datalistFragment, enDev2ColumnArgumentDirection.Input);

            //------------Assert Results-------------------------

            Assert.AreEqual(@"<Inputs><Input Name=""result"" Source="""" /></Inputs>", result);
        }

        [TestMethod]
        public void GenerateDefsFromDataListWhereDataListExpectResultsToMatchIODirectionForInput()
        {
            //------------Setup for test--------------------------
            var dataListCompiler = DataListFactory.CreateDataListCompiler();

            const string dataList = "<DataList><test ColumnIODirection=\"Both\" /><newvar ColumnIODirection=\"Input\" /><as ColumnIODirection=\"Output\" />" +
                                    "<sssdd ColumnIODirection=\"Both\" /><sss ColumnIODirection=\"Both\" />" +
                                    "<recset ColumnIODirection=\"Both\"><f1 ColumnIODirection=\"Both\" /><f2 ColumnIODirection=\"Output\" /></recset></DataList>";
            //------------Execute Test---------------------------
            var generateDefsFromDataList = dataListCompiler.GenerateDefsFromDataList(dataList, enDev2ColumnArgumentDirection.Input);
            //------------Assert Results-------------------------
            Assert.AreEqual(5, generateDefsFromDataList.Count);
        }

        [TestMethod]
        public void GenerateDefsFromDataListWhereDataListExpectResultsToMatchIODirectionForOutput()
        {
            //------------Setup for test--------------------------
            var dataListCompiler = DataListFactory.CreateDataListCompiler();
            const string dataList = "<DataList><test ColumnIODirection=\"Both\" /><newvar ColumnIODirection=\"Input\" /><as ColumnIODirection=\"Output\" />" +
                                    "<sssdd ColumnIODirection=\"Both\" /><sss ColumnIODirection=\"Both\" />" +
                                    "<recset ColumnIODirection=\"Both\"><f1 ColumnIODirection=\"Both\" /><f2 ColumnIODirection=\"Output\" /></recset></DataList>";
            //------------Execute Test---------------------------
            var generateDefsFromDataList = dataListCompiler.GenerateDefsFromDataList(dataList, enDev2ColumnArgumentDirection.Output);
            //------------Assert Results-------------------------
            Assert.AreEqual(6, generateDefsFromDataList.Count);
        }

        [TestMethod]
        public void GenerateDefsFromDataListWhereDataListExpectResultsToMatchIODirectionForBoth()
        {
            //------------Setup for test--------------------------
            var dataListCompiler = DataListFactory.CreateDataListCompiler();
            const string dataList = "<DataList><test ColumnIODirection=\"Both\" /><newvar ColumnIODirection=\"Input\" /><as ColumnIODirection=\"Output\" />" +
                                    "<sssdd ColumnIODirection=\"Both\" /><sss ColumnIODirection=\"Both\" />" +
                                    "<recset ColumnIODirection=\"Both\"><f1 ColumnIODirection=\"Both\" /><f2 ColumnIODirection=\"Output\" /></recset></DataList>";
            //------------Execute Test---------------------------
            var generateDefsFromDataList = dataListCompiler.GenerateDefsFromDataList(dataList, enDev2ColumnArgumentDirection.Both);
            //------------Assert Results-------------------------
            Assert.AreEqual(4, generateDefsFromDataList.Count);
        }




        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DataListCompiler_GenerateDefsFromDataList")]
        public void GenerateDefsFromDataListWhereDataListExpectResultsToMatchIODirectionForOnlyNestedColumns()
        {
            //------------Setup for test--------------------------
            var dataListCompiler = DataListFactory.CreateDataListCompiler();
            const string dataList = "<DataList><test ColumnIODirection=\"Both\" /><newvar ColumnIODirection=\"Input\" /><as ColumnIODirection=\"Output\" />" +
                                    "<sssdd ColumnIODirection=\"Both\" /><sss ColumnIODirection=\"Input\" />" +
                                    "<recset ColumnIODirection=\"None\"><f1 ColumnIODirection=\"Output\" /><f2 ColumnIODirection=\"Output\" /></recset></DataList>";
            //------------Execute Test---------------------------
            var generateDefsFromDataList = dataListCompiler.GenerateDefsFromDataList(dataList, enDev2ColumnArgumentDirection.Both);
            //------------Assert Results-------------------------
            Assert.AreEqual(2, generateDefsFromDataList.Count);
        }

        [TestMethod]
        public void GenerateDefsFromDataListWhereDataListExpectResultsToMatchIODirectionForRecsetHasNone()
        {
            //------------Setup for test--------------------------
            var dataListCompiler = DataListFactory.CreateDataListCompiler();
            const string dataList = "<DataList><test ColumnIODirection=\"Both\" /><newvar ColumnIODirection=\"Input\" /><as ColumnIODirection=\"Output\" />" +
                                    "<sssdd ColumnIODirection=\"Both\" /><sss ColumnIODirection=\"Both\" />" +
                                    "<recset ColumnIODirection=\"Both\"><f1 ColumnIODirection=\"Both\" /><f2 ColumnIODirection=\"None\" /></recset></DataList>";
            //------------Execute Test---------------------------
            var generateDefsFromDataList = dataListCompiler.GenerateDefsFromDataList(dataList, enDev2ColumnArgumentDirection.Output);
            //------------Assert Results-------------------------
            Assert.AreEqual(5, generateDefsFromDataList.Count);
        }

        #endregion Generate Defintion Tests

        #region Generate DataList From Defs Tests

        [TestMethod]
        public void GenerateWizardDataListFromDefs_Outputs_Expected_Correct_DataList()
        {
            var dataListCompiler = DataListFactory.CreateDataListCompiler();
            string defstring = ParserStrings.dlOutputMappingOutMapping;
            ErrorResultTO errors;
            string acctual = dataListCompiler.GenerateWizardDataListFromDefs(defstring, enDev2ArgumentType.Output, false, out errors, true);

            Assert.IsTrue(acctual.Contains(@"<ADL><required></required><validationClass></validationClass><cssClass>[[cssClass]]</cssClass><Dev2customStyle></Dev2customStyle>
</ADL>"));
        }

        [TestMethod]
        public void GenerateWizardDataListFromDefs_Inputs_Expected_Correct_DataList()
        {
            var dataListCompiler = DataListFactory.CreateDataListCompiler();
            string defstring = ParserStrings.dlInputMapping;
            ErrorResultTO errors;
            string acctual = dataListCompiler.GenerateWizardDataListFromDefs(defstring, enDev2ArgumentType.Input, false, out errors, true);

            Assert.IsTrue(acctual.Contains(@"<ADL><reg></reg><asdfsad>registration223</asdfsad><number></number>
</ADL>"));
        }

        [TestMethod]
        public void GenerateDataListFromDefs_Outputs_Expected_Correct_DataList()
        {
            var dataListCompiler = DataListFactory.CreateDataListCompiler();
            string defstring = ParserStrings.dlOutputMappingOutMapping;
            ErrorResultTO errors;
            string acctual = dataListCompiler.GenerateDataListFromDefs(defstring, enDev2ArgumentType.Output, false, out errors);

            Assert.IsTrue(acctual.Contains(@"<ADL><required/><validationClass/><cssClass/><Dev2customStyle/>
</ADL>"));
        }

        [TestMethod]
        public void GenerateDataListFromDefs_Inputs_Expected_Correct_DataList()
        {
            var dataListCompiler = DataListFactory.CreateDataListCompiler();
            string defstring = ParserStrings.dlInputMapping;
            ErrorResultTO errors;
            string acctual = dataListCompiler.GenerateDataListFromDefs(defstring, enDev2ArgumentType.Input, false, out errors);

            Assert.IsTrue(acctual.Contains(@"<ADL><reg/><asdfsad/><number/>
</ADL>"));
        }

        #endregion Generate DataList From Defs Tests

        #region Evaluation Test

        [TestMethod]
        public void UpsertWhereListStringExpectUpsertCorrectly()
        {
            //------------Setup for test--------------------------
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            IDev2DataListUpsertPayloadBuilder<List<string>> toUpsert = Dev2DataListBuilderFactory.CreateStringListDataListUpsertBuilder();
            toUpsert.Add("[[rec().f1]]", new List<string> { "test1", "test2" });
            IBinaryDataList dataList = Dev2BinaryDataListFactory.CreateDataList();
            string creationError;
            dataList.TryCreateRecordsetTemplate("rec", "recset", new List<Dev2Column> { DataListFactory.CreateDev2Column("f1", "f1") }, true, out creationError);
            ErrorResultTO localErrors;
            compiler.PushBinaryDataList(dataList.UID, dataList, out localErrors);
            //------------Execute Test---------------------------
            compiler.Upsert(dataList.UID, toUpsert, out _errors);
            //------------Assert Results-------------------------
            IList<IBinaryDataListEntry> binaryDataListEntries = dataList.FetchRecordsetEntries();
            IBinaryDataListEntry binaryDataListEntry = binaryDataListEntries[0];
            string errString;
            IList<IBinaryDataListItem> binaryDataListItems = binaryDataListEntry.FetchRecordAt(1, out errString);
            IBinaryDataListItem binaryDataListItem = binaryDataListItems[0];
            string theValue = binaryDataListItem.TheValue;
            Assert.AreEqual("test1", theValue);

            binaryDataListItems = binaryDataListEntry.FetchRecordAt(2, out errString);
            binaryDataListItem = binaryDataListItems[0];
            theValue = binaryDataListItem.TheValue;


            Assert.AreEqual("test2", theValue);
        }

        [TestMethod]
        public void TryFetchLastIndexedRecordsetUpsertPayload_ColumnName_FetchesForColumn()
        {
            //------------Setup for test--------------------------
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            IDev2DataListUpsertPayloadBuilder<List<string>> toUpsert = Dev2DataListBuilderFactory.CreateStringListDataListUpsertBuilder();
            toUpsert.Add("[[rec().f1]]", new List<string> { "test11", "test12" });
            toUpsert.Add("[[rec().f2]]", new List<string> { "test21", "test22" });
            IBinaryDataList dataList = Dev2BinaryDataListFactory.CreateDataList();
            string creationError;
            dataList.TryCreateRecordsetTemplate("rec", "recset", new List<Dev2Column> { DataListFactory.CreateDev2Column("f1", "f1"), DataListFactory.CreateDev2Column("f2", "f2") }, true, out creationError);
            ErrorResultTO localErrors;
            compiler.PushBinaryDataList(dataList.UID, dataList, out localErrors);
            compiler.Upsert(dataList.UID, toUpsert, out _errors);
            IBinaryDataListEntry recEntry;
            string error;
            dataList.TryGetEntry("rec", out recEntry, out error);
            //------------Assert Preconditions-------------------
            Assert.IsNotNull(recEntry);
            //------------Execute Test---------------------------
            var listItem = recEntry.TryFetchLastIndexedRecordsetUpsertPayload(out error, "f2");
            //------------Assert Results-------------------------
            Assert.AreEqual("test22", listItem.TheValue);
        }

        [TestMethod]
        public void UpsertWhereListStringExpectUpsertCorrectlyMultipleRecordset()
        {
            //------------Setup for test--------------------------
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            IDev2DataListUpsertPayloadBuilder<List<string>> toUpsert = Dev2DataListBuilderFactory.CreateStringListDataListUpsertBuilder();
            toUpsert.Add("[[rec().f1]]", new List<string> { "test11", "test12" });
            toUpsert.Add("[[rec().f2]]", new List<string> { "test21", "test22" });
            IBinaryDataList dataList = Dev2BinaryDataListFactory.CreateDataList();
            string creationError;
            dataList.TryCreateRecordsetTemplate("rec", "recset", new List<Dev2Column> { DataListFactory.CreateDev2Column("f1", "f1"), DataListFactory.CreateDev2Column("f2", "f2") }, true, out creationError);
            ErrorResultTO localErrors;
            compiler.PushBinaryDataList(dataList.UID, dataList, out localErrors);
            //------------Execute Test---------------------------
            compiler.Upsert(dataList.UID, toUpsert, out _errors);
            //------------Assert Results-------------------------
            IList<IBinaryDataListEntry> binaryDataListEntries = dataList.FetchRecordsetEntries();
            IBinaryDataListEntry binaryDataListEntry = binaryDataListEntries[0];
            string errString;
            IList<IBinaryDataListItem> binaryDataListItems = binaryDataListEntry.FetchRecordAt(1, out errString);
            IBinaryDataListItem binaryDataListItem = binaryDataListItems[0];
            IBinaryDataListItem binaryDataListItem2 = binaryDataListItems[1];
            string theValue = binaryDataListItem.TheValue;
            Assert.AreEqual("test11", theValue);
            theValue = binaryDataListItem2.TheValue;
            Assert.AreEqual("test21", theValue);
            binaryDataListItems = binaryDataListEntry.FetchRecordAt(2, out errString);
            binaryDataListItem = binaryDataListItems[0];
            binaryDataListItem2 = binaryDataListItems[1];
            theValue = binaryDataListItem.TheValue;
            Assert.AreEqual("test12", theValue);
            theValue = binaryDataListItem2.TheValue;

            Assert.AreEqual("test22", theValue);
        }

        // Bug 8609
        [TestMethod]
        public void Can_Sub_Recordset_With_Index_Expect()
        {
            var dataListCompiler = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;
            IBinaryDataListEntry binaryDataListEntry = dataListCompiler.Evaluate(_dl2.UID, enActionType.User, "[[recset(1).f1]]", false, out errors);

            Assert.AreEqual("r1.f1.value", binaryDataListEntry.FetchScalar().TheValue);
        }

        #endregion

        #region Generate Defintion Tests Debug



        [TestMethod]
        public void GenerateDefsFromData_Debug_ListWhereDataListExpectResultsToMatchIODirectionForInput()
        {
            //------------Setup for test--------------------------
            var dataListCompiler = DataListFactory.CreateDataListCompiler();

            const string dataList = "<DataList><test ColumnIODirection=\"Both\" /><newvar ColumnIODirection=\"Input\" /><as ColumnIODirection=\"Output\" />" +
                                    "<sssdd ColumnIODirection=\"Both\" /><sss ColumnIODirection=\"Both\" />" +
                                    "<recset ColumnIODirection=\"Both\"><f1 ColumnIODirection=\"Both\" /><f2 ColumnIODirection=\"Output\" /></recset></DataList>";
            //------------Execute Test---------------------------
            var generateDefsFromDataList = dataListCompiler.GenerateDefsFromDataListForDebug(dataList, enDev2ColumnArgumentDirection.Input);
            //------------Assert Results-------------------------
            Assert.AreEqual(5, generateDefsFromDataList.Count);// only outer values are inputs. no columns
        }

        [TestMethod]
        public void GenerateDefsFromDataListDebugWhereDataListExpectResultsToMatchIODirectionForOutput()
        {
            //------------Setup for test--------------------------
            var dataListCompiler = DataListFactory.CreateDataListCompiler();
            const string dataList = "<DataList><test ColumnIODirection=\"Both\" /><newvar ColumnIODirection=\"Input\" /><as ColumnIODirection=\"Output\" />" +
                                    "<sssdd ColumnIODirection=\"Both\" /><sss ColumnIODirection=\"Both\" />" +
                                    "<recset ColumnIODirection=\"Both\"><f1 ColumnIODirection=\"Both\" /><f2 ColumnIODirection=\"Output\" /></recset></DataList>";
            //------------Execute Test---------------------------
            var generateDefsFromDataList = dataListCompiler.GenerateDefsFromDataListForDebug(dataList, enDev2ColumnArgumentDirection.Output);
            //------------Assert Results-------------------------
            Assert.AreEqual(5, generateDefsFromDataList.Count); // nested column does not show up
        }

        [TestMethod]
        public void GenerateDefsFromDataListDebugWhereDataListExpectResultsToMatchIODirectionForBoth()
        {
            //------------Setup for test--------------------------
            var dataListCompiler = DataListFactory.CreateDataListCompiler();
            const string dataList = "<DataList><test ColumnIODirection=\"Both\" /><newvar ColumnIODirection=\"Input\" /><as ColumnIODirection=\"Output\" />" +
                                    "<sssdd ColumnIODirection=\"Both\" /><sss ColumnIODirection=\"Both\" />" +
                                    "<recset ColumnIODirection=\"Both\"><f1 ColumnIODirection=\"Both\" /><f2 ColumnIODirection=\"Output\" /></recset></DataList>";
            //------------Execute Test---------------------------
            var generateDefsFromDataList = dataListCompiler.GenerateDefsFromDataListForDebug(dataList, enDev2ColumnArgumentDirection.Both);
            //------------Assert Results-------------------------
            Assert.AreEqual(4, generateDefsFromDataList.Count); // ooly both and no inner values
        }




        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DataListCompiler_GenerateDefsFromDataList")]
        public void GenerateDefsFromDataListWhereDataListDebugExpectResultsToMatchIODirectionForOnlyNestedColumns()
        {
            //------------Setup for test--------------------------
            var dataListCompiler = DataListFactory.CreateDataListCompiler();
            const string dataList = "<DataList><test ColumnIODirection=\"Both\" /><newvar ColumnIODirection=\"Input\" /><as ColumnIODirection=\"Output\" />" +
                                    "<sssdd ColumnIODirection=\"Both\" /><sss ColumnIODirection=\"Input\" />" +
                                    "<recset ColumnIODirection=\"None\"><f1 ColumnIODirection=\"Output\" /><f2 ColumnIODirection=\"Output\" /></recset></DataList>";
            //------------Execute Test---------------------------
            var generateDefsFromDataList = dataListCompiler.GenerateDefsFromDataListForDebug(dataList, enDev2ColumnArgumentDirection.Both);
            //------------Assert Results-------------------------
            Assert.AreEqual(2, generateDefsFromDataList.Count);  //only2 outs
        }

        [TestMethod]
        public void GenerateDefsFromDataListWhereDataListDebugExpectResultsToMatchIODirectionForRecsetHasNone()
        {
            //------------Setup for test--------------------------
            var dataListCompiler = DataListFactory.CreateDataListCompiler();
            const string dataList = "<DataList><test ColumnIODirection=\"Both\" /><newvar ColumnIODirection=\"Input\" /><as ColumnIODirection=\"Output\" />" +
                                    "<sssdd ColumnIODirection=\"Both\" /><sss ColumnIODirection=\"Both\" />" +
                                    "<recset ColumnIODirection=\"Both\"><f1 ColumnIODirection=\"Both\" /><f2 ColumnIODirection=\"None\" /></recset></DataList>";
            //------------Execute Test---------------------------
            var generateDefsFromDataList = dataListCompiler.GenerateDefsFromDataListForDebug(dataList, enDev2ColumnArgumentDirection.Output);
            //------------Assert Results-------------------------
            Assert.AreEqual(5, generateDefsFromDataList.Count); // nested column must not appear
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DataListCompiler_GenerateDefsFromDataList")]
        public void GenerateDefsFromDataList_Debug_RecSetNotOut_CoulmnsOut_ExpectColumns()
        {
            //------------Setup for test--------------------------
            var dataListCompiler = DataListFactory.CreateDataListCompiler();
            const string dataList = "<DataList><test ColumnIODirection=\"Both\" /><newvar ColumnIODirection=\"Input\" /><as ColumnIODirection=\"Output\" />" +
                                    "<recset ColumnIODirection=\"Input\"><f1 ColumnIODirection=\"Both\" /><f2 ColumnIODirection=\"None\" /></recset></DataList>";
            //------------Execute Test---------------------------
            var generateDefsFromDataList = dataListCompiler.GenerateDefsFromDataListForDebug(dataList, enDev2ColumnArgumentDirection.Output);
            //------------Assert Results-------------------------
            Assert.AreEqual(3, generateDefsFromDataList.Count); // inner column is set as output
        }


        #endregion Generate Defintion Tests

        // ReSharper restore InconsistentNaming
    }
}
