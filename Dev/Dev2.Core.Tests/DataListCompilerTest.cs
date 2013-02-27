using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.TO;
using Dev2.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Unlimited.UnitTest.Framework
{
    /// <summary>
    /// Summary description for DataListCompilerTest
    /// </summary>
    [TestClass]
    public class DataListCompilerTest
    {


        private IDataListCompiler _compiler = DataListFactory.CreateDataListCompiler();
        private IBinaryDataList dl1;
        private IBinaryDataList dl2;
        private ErrorResultTO errors = new ErrorResultTO();
        private TestContext testContextInstance;
        private string error;
        private IBinaryDataListEntry entry;

        public DataListCompilerTest()
        {
        }

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //Use TestInitialize to run code before running each test 
        [TestInitialize()]
        public void MyTestInitialize()
        {
            string error = string.Empty;

            dl1 = Dev2BinaryDataListFactory.CreateDataList();
            dl1.TryCreateScalarTemplate(string.Empty, "myScalar", "A scalar", true, out error);
            dl1.TryCreateScalarValue("[[otherScalar]]", "myScalar", out error);

            dl1.TryCreateScalarTemplate(string.Empty, "otherScalar", "A scalar", true, out error);
            dl1.TryCreateScalarValue("testRegion", "otherScalar", out error);

            dl1.TryCreateScalarTemplate(string.Empty, "scalar1", "A scalar", true, out error);
            dl1.TryCreateScalarValue("foobar", "scalar1", out error);

            IList<Dev2Column> cols = new List<Dev2Column>();
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f1"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f2"));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f3"));

            dl1.TryCreateRecordsetTemplate("recset", "a recordset", cols, true, out error);

            dl1.TryCreateRecordsetValue("r1.f1.value", "f1", "recset", 1, out error);
            dl1.TryCreateRecordsetValue("r1.f2.value", "f2", "recset", 1, out error);
            dl1.TryCreateRecordsetValue("r1.f3.value", "f3", "recset", 1, out error);

            dl1.TryCreateRecordsetValue("r2.f1.value", "f1", "recset", 2, out error);
            dl1.TryCreateRecordsetValue("r2.f2.value", "f2", "recset", 2, out error);
            dl1.TryCreateRecordsetValue("r2.f3.value", "f3", "recset", 2, out error);

            // skip 3 ;)

            dl1.TryCreateRecordsetValue("r4.f1.value", "f1", "recset", 4, out error);
            dl1.TryCreateRecordsetValue("r4.f2.value", "f2", "recset", 4, out error);
            dl1.TryCreateRecordsetValue("r4.f3.value", "f3", "recset", 4, out error);

            _compiler.PushBinaryDataList(dl1.UID, dl1, out errors);
            _compiler.UpsertSystemTag(dl1.UID, enSystemTag.EvaluateIteration, "true", out errors);

            /*  list 2 */
            dl2 = Dev2BinaryDataListFactory.CreateDataList();
            dl2.TryCreateScalarTemplate(string.Empty, "idx", "A scalar", true, out error);
            dl2.TryCreateScalarValue("1", "idx", out error);

            dl2.TryCreateRecordsetTemplate("recset", "a recordset", cols, true, out error);

            dl2.TryCreateRecordsetValue("r1.f1.value", "f1", "recset", 1, out error);
            dl2.TryCreateRecordsetValue("r1.f2.value", "f2", "recset", 1, out error);
            dl2.TryCreateRecordsetValue("r1.f3.value", "f3", "recset", 1, out error);

            dl2.TryCreateRecordsetValue("r2.f1.value", "f1", "recset", 2, out error);
            dl2.TryCreateRecordsetValue("r2.f2.value", "f2", "recset", 2, out error);
            dl2.TryCreateRecordsetValue("r2.f3.value", "f3", "recset", 2, out error);

            _compiler.PushBinaryDataList(dl2.UID, dl2, out errors);
            _compiler.UpsertSystemTag(dl2.UID, enSystemTag.EvaluateIteration, "true", out errors);
        }

        #endregion

        // Created by Michael for Bug 8597
        [TestMethod]
        public void HasErrors_Passed_Empty_GUID_Expected_No_NullReferenceException()
        {
            Guid id = Guid.Empty;
            try
            {
                _compiler.HasErrors(id);
            }
            catch (NullReferenceException)
            {
                Assert.Inconclusive("No NullReferenceException should be thrown.");
            }
        }

        [TestMethod]
        public void Iteration_Evaluation_Expect_Evaluation_For_1_Iteration()
        {
            // Iteration evaluation is tested via the shape method ;)
            string defs = @"<Inputs><Input Name=""scalar1"" Source=""[[myScalar]]"" /></Inputs>"; ;
            Guid id = _compiler.Shape(dl1.UID, enDev2ArgumentType.Input, defs, out errors);

            IBinaryDataList bdl = _compiler.FetchBinaryDataList(id, out errors);

            bdl.TryGetEntry("scalar1", out entry, out error);

            Assert.AreEqual("[[otherScalar]]", entry.FetchScalar().TheValue);

        }

        [TestMethod]
        public void Iteration_Evaluation_Expect_Evaluation_For_2_Iterations()
        {
            // Iteration evaluation is tested via the shape method ;)
            string defs = @"<Inputs><Input Name=""scalar1"" Source=""[[[[myScalar]]]]"" /></Inputs>"; ;
            Guid id = _compiler.Shape(dl1.UID, enDev2ArgumentType.Input, defs, out errors);

            IBinaryDataList bdl = _compiler.FetchBinaryDataList(id, out errors);

            bdl.TryGetEntry("scalar1", out entry, out error);

            Assert.AreEqual("[[testRegion]]", entry.FetchScalar().TheValue);

        }

        [TestMethod]
        public void Iteration_Evaluation_Expect_Evaluation_For_0_Iterations()
        {
            // Iteration evaluation is tested via the shape method ;)
            string defs = @"<Inputs><Input Name=""scalar1"" Source=""foobar"" /></Inputs>"; ;
            Guid id = _compiler.Shape(dl1.UID, enDev2ArgumentType.Input, defs, out errors);

            IBinaryDataList bdl = _compiler.FetchBinaryDataList(id, out errors);

            bdl.TryGetEntry("scalar1", out entry, out error);

            Assert.AreEqual("foobar", entry.FetchScalar().TheValue);

        }

        [TestMethod]
        public void Iteration_Evaluation_Recordset_Expect_Evaluation_For_2_Iterations()
        {
            // Iteration evaluation is tested via the shape method ;)
            string defs = @"<Inputs><Input Name=""scalar1"" Source=""[[recset([[idx]]).f1]]"" /></Inputs>"; ;
            Guid id = _compiler.Shape(dl2.UID, enDev2ArgumentType.Input, defs, out errors);

            IBinaryDataList bdl = _compiler.FetchBinaryDataList(id, out errors);

            bdl.TryGetEntry("scalar1", out entry, out error);

            Assert.AreEqual("r1.f1.value", entry.FetchScalar().TheValue);

        }

        #region FixedWizardTest
        [TestMethod]
        public void FixedWizardScalar_Converter_Expected_FixedDataListPortion()
        {
            string wizDL = @"<DataList>
       <TestVar IsEditable=""False"" Description=""""/>
       <Port IsEditable=""False"" Description=""""/>
       <From IsEditable=""False"" Description=""""/>
       <To IsEditable=""False"" Description=""""/>
       <Subject IsEditable=""False"" Description=""""/>
       <BodyType IsEditable=""False"" Description=""""/>
       <Body IsEditable=""False"" Description=""""/>
       <Attachment IsEditable=""False"" Description=""""/>
       <FailureMessage IsEditable=""False"" Description=""""/>
       <Message IsEditable=""False"" Description=""""/>
</DataList>
";
            string serviceDL = @"<DataList>
       <Movember IsEditable=""False"" Description=""""/>
       <Port IsEditable=""False"" Description=""""/>
       <From IsEditable=""False"" Description=""""/>
       <To IsEditable=""False"" Description=""""/>
       <Subject IsEditable=""False"" Description=""""/>
       <BodyType IsEditable=""False"" Description=""""/>
       <Body IsEditable=""False"" Description=""""/>
       <Attachment IsEditable=""False"" Description=""""/>
       <FailureMessage IsEditable=""False"" Description=""""/>
       <Message IsEditable=""False"" Description=""""/>
</DataList>
";
            string expected = @"<DataList><Movember IsEditable=""False"" ></Movember><Port IsEditable=""False"" ></Port><From IsEditable=""False"" ></From><To IsEditable=""False"" ></To><Subject IsEditable=""False"" ></Subject><BodyType IsEditable=""False"" ></BodyType><Body IsEditable=""False"" ></Body><Attachment IsEditable=""False"" ></Attachment><FailureMessage IsEditable=""False"" ></FailureMessage><Message IsEditable=""False"" ></Message></DataList>";

            WizardDataListMergeTO result = _compiler.MergeFixedWizardDataList(wizDL, serviceDL);

            Assert.AreEqual(expected, result.IntersectedDataList);
            Assert.AreEqual("Movember", result.AddedRegions[0].FetchScalar().FieldName);
            Assert.AreEqual("TestVar", result.RemovedRegions[0].FetchScalar().FieldName);
        }

        [TestMethod]
        public void FixedWizardRecordset_Converter_Expected_FixedDataListPortion()
        {
            string wizDL = @"<DataList>
       <Port IsEditable=""False"" Description=""""/>
       <From IsEditable=""False"" Description=""""/>
       <To IsEditable=""False"" Description=""""/>
       <Subject IsEditable=""False"" Description=""""/>
       <BodyType IsEditable=""False"" Description=""""/>
       <Body IsEditable=""False"" Description=""""/>
       <Attachment IsEditable=""False"" Description=""""/>
       <FailureMessage IsEditable=""False"" Description=""""/>
       <Message IsEditable=""False"" Description=""""/>
</DataList>
";
            string serviceDL = @"<DataList>
       <Movember IsEditable=""False"" Description=""""/>
       <Recordset IsEditable=""False"" Description="""">
            <FirstName/>
        </Recordset>
       <Port IsEditable=""False"" Description=""""/>
       <From IsEditable=""False"" Description=""""/>
       <To IsEditable=""False"" Description=""""/>
       <Subject IsEditable=""False"" Description=""""/>
       <BodyType IsEditable=""False"" Description=""""/>
       <Body IsEditable=""False"" Description=""""/>
       <Attachment IsEditable=""False"" Description=""""/>
       <FailureMessage IsEditable=""False"" Description=""""/>
       <Message IsEditable=""False"" Description=""""/>
</DataList>
";
            string expected = @"<DataList><Movember IsEditable=""False"" ></Movember><Recordset IsEditable=""False"" ><FirstName IsEditable=""False"" ></FirstName></Recordset><Port IsEditable=""False"" ></Port><From IsEditable=""False"" ></From><To IsEditable=""False"" ></To><Subject IsEditable=""False"" ></Subject><BodyType IsEditable=""False"" ></BodyType><Body IsEditable=""False"" ></Body><Attachment IsEditable=""False"" ></Attachment><FailureMessage IsEditable=""False"" ></FailureMessage><Message IsEditable=""False"" ></Message></DataList>";
            string error = string.Empty;

            WizardDataListMergeTO result = _compiler.MergeFixedWizardDataList(wizDL, serviceDL);

            Assert.AreEqual(expected, result.IntersectedDataList);
            Assert.AreEqual("Movember", result.AddedRegions[0].FetchScalar().FieldName);
            Assert.AreEqual("Recordset", (result.AddedRegions[1].FetchRecordAt(1, out error))[0].Namespace);
            Assert.AreEqual(0, result.RemovedRegions.Count);
        }

        #endregion

        #region Generate Defintion Tests

        [TestMethod]
        public void GenerateDefsFromWebpageXMl_Standard_WebpageXMl_Expected_Correct_Deffintions()
        {
            string webpageXml = ParserStrings.WebPageXMLConfig;
            IList<IDev2Definition> defs = _compiler.GenerateDefsFromWebpageXMl(webpageXml);

            Assert.IsTrue(defs.Count == 5);
        }

        #endregion Generate Defintion Tests

        #region Generate DataList From Defs Tests

        [TestMethod]
        public void GenerateWizardDataListFromDefs_Outputs_Expected_Correct_DataList()
        {
            string defstring = ParserStrings.dlOutputMappingOutMapping;
            ErrorResultTO errors;
            string acctual = _compiler.GenerateWizardDataListFromDefs(defstring, enDev2ArgumentType.Output, false, out errors, true);

            Assert.IsTrue(acctual.Contains(@"<ADL><required></required><validationClass></validationClass><cssClass>[[cssClass]]</cssClass><Dev2customStyle></Dev2customStyle>
</ADL>"));
        }

        [TestMethod]
        public void GenerateWizardDataListFromDefs_Inputs_Expected_Correct_DataList()
        {
            string defstring = ParserStrings.dlInputMapping;
            ErrorResultTO errors;
            string acctual = _compiler.GenerateWizardDataListFromDefs(defstring, enDev2ArgumentType.Input, false, out errors, true);

            Assert.IsTrue(acctual.Contains(@"<ADL><reg></reg><asdfsad>registration223</asdfsad><number></number>
</ADL>"));
        }

        [TestMethod]
        public void GenerateDataListFromDefs_Outputs_Expected_Correct_DataList()
        {
            string defstring = ParserStrings.dlOutputMappingOutMapping;
            ErrorResultTO errors;
            string acctual = _compiler.GenerateDataListFromDefs(defstring, enDev2ArgumentType.Output, false, out errors);

            Assert.IsTrue(acctual.Contains(@"<ADL><required/><validationClass/><cssClass/><Dev2customStyle/>
</ADL>"));
        }

        [TestMethod]
        public void GenerateDataListFromDefs_Inputs_Expected_Correct_DataList()
        {
            string defstring = ParserStrings.dlInputMapping;
            ErrorResultTO errors;
            string acctual = _compiler.GenerateDataListFromDefs(defstring, enDev2ArgumentType.Input, false, out errors);

            Assert.IsTrue(acctual.Contains(@"<ADL><reg/><asdfsad/><number/>
</ADL>"));
        }

        #endregion Generate DataList From Defs Tests


        #region Evaluation Test

        // Bug 8609
        [TestMethod]
        public void Can_Sub_Recordset_With_Index_Expect()
        {
            ErrorResultTO errors = new ErrorResultTO();
            IBinaryDataListEntry entry = _compiler.Evaluate(dl2.UID, enActionType.User, "[[recset(1).f1]]", false, out errors);

            Assert.AreEqual("r1.f1.value", entry.FetchScalar().TheValue);
        }

        #endregion
    }
}
