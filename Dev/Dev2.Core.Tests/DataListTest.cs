using System.Collections.Generic;
using Dev2.DataList.Contract;
using Dev2.Tests.Properties;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataListTset
{
    /// <summary>
    /// Summary description for check-in
    /// </summary>
    [TestClass]
    public class DataListTest
    {
        private TestContext testContextInstance;

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

        #region Output Mapping Test

        [TestMethod]
        public void TestOutputParsingScalar()
        {
            IList<IDev2Definition> defs = DataListFactory.CreateOutputParser().Parse(TestStrings.sampleActivityMappingScalar);

            IDev2Definition d = defs[0];

            Assert.IsTrue(defs.Count == 3 && d.MapsTo == "fname" && d.Value == "firstName" && d.Name == "FirstName" && !d.IsRecordSet);

        }

        [TestMethod]
        public void TestOutputParsingRecordSet()
        {
            IList<IDev2Definition> defs = DataListFactory.CreateOutputParser().Parse(TestStrings.sampleActivityMappingRecordSets);
            IRecordSetCollection recCol = DataListFactory.CreateRecordSetCollection(defs);

            IDev2Definition d = defs[0];

            Assert.IsTrue(d.IsRecordSet && recCol.RecordSetNames[0] == "Person" && recCol.RecordSets[0].Columns[0].Value == "ppl().firstName");

        }

        [TestMethod]
        public void TestRecordSetCollectionCreationEnsure1Set()
        {
            IList<IDev2Definition> defs = DataListFactory.CreateOutputParser().Parse(TestStrings.sampleActivityMappingRecordSets);
            IRecordSetCollection recCol = DataListFactory.CreateRecordSetCollection(defs);

            Assert.IsTrue((recCol.RecordSetNames.Count == 1));
        }

        [TestMethod]
        public void TestScalarRecordSetMixedParsing()
        {
            IList<IDev2Definition> defs = DataListFactory.CreateOutputParser().Parse(TestStrings.sampleActivityMappingMixed);
            IRecordSetCollection recCol = DataListFactory.CreateRecordSetCollection(defs);
            IList<IDev2Definition> scalarList = DataListFactory.CreateScalarList(defs);


            Assert.IsTrue(scalarList.Count == 1 && recCol.RecordSetNames.Count == 1);
        }

        #endregion

        #region Activity Output Parsing

        [TestMethod]
        public void TestDataStreamParseBasedUponRecordSetOutput()
        {
            IList<IDev2Definition> defs = DataListFactory.CreateOutputParser().Parse(TestStrings.sampleActivityMappingRecordSets);
            IActivityDataParser dataParser = DataListFactory.CreateActivityDataParser();
            dataParser.ParseDataStream(TestStrings.sampleDataRecordSet, defs);

            IRecordSetCollection inst = dataParser.ParsedData.Recordsets;

            Assert.IsTrue((dataParser.ParsedData.Scalars.Count == 0) && (inst.RecordSetNames.Count == 1) && (inst.RecordSets[0].Columns.Count > 0));
        }

        [TestMethod]
        public void ExtractScalarBlank()
        {
            IList<IDev2Definition> defs = DataListFactory.CreateOutputParser().Parse(TestStrings.blankOutput);


            Assert.IsTrue(defs.Count == 1 && defs[0].MapsTo == "ABC");
        }

        [TestMethod]
        public void TestDataStreamParseBasedUponScalarOutput()
        {
            IList<IDev2Definition> defs = DataListFactory.CreateOutputParser().Parse(TestStrings.sampleActivityMappingScalar);
            IActivityDataParser dataParser = DataListFactory.CreateActivityDataParser();
            dataParser.ParseDataStream(TestStrings.sampleDataScalar, defs);

            Assert.IsTrue((dataParser.ParsedData.Recordsets.RecordSets.Count == 0) && (dataParser.ParsedData.Scalars.Count == 3) && (dataParser.ParsedData.Scalars[0].Value != null));
        }

        [TestMethod]
        public void TestDataStreamParseBasedUponMixedOutput()
        {
            IList<IDev2Definition> defs = DataListFactory.CreateOutputParser().Parse(TestStrings.sampleActivityMappingMixed);

            IActivityDataParser dataParser = DataListFactory.CreateActivityDataParser();
            dataParser.ParseDataStream(TestStrings.sampleDataMixed, defs);

            Assert.IsTrue((dataParser.ParsedData.Recordsets.RecordSets.Count == 1) && (dataParser.ParsedData.Scalars.Count == 1));
        }

        [TestMethod]
        public void TestDataStreamParseBasedUponRecordSetDataForScalar()
        {
            IList<IDev2Definition> defs = DataListFactory.CreateOutputParser().Parse(TestStrings.sampleActivityMappingScalar);

            IActivityDataParser dataParser = DataListFactory.CreateActivityDataParser();
            dataParser.ParseDataStream(TestStrings.sampleDataRecordSet, defs);

            Assert.IsTrue((dataParser.ParsedData.Recordsets.RecordSets.Count == 0) && (dataParser.ParsedData.Scalars.Count == 3));
        }

        #endregion

        #region Util Test

        [TestMethod]
        public void StripDoubleBracketNormal()
        {
            string canidate = "[[abc()]]";

            string result = DataListCleaningUtils.stripDoubleBracketsAndRecordsetNotation(canidate);

            Assert.AreEqual(result, "abc");
        }

        [TestMethod]
        public void StripDoubleBracketNone()
        {
            string canidate = "abc()";

            string result = DataListCleaningUtils.stripDoubleBracketsAndRecordsetNotation(canidate);

            Assert.AreEqual(canidate, "abc()");
        }

        [TestMethod]
        public void StripDoubleBracketRecursiveEval()
        {
            string canidate = "[[[[abc()]]]]";

            string result = DataListCleaningUtils.stripDoubleBracketsAndRecordsetNotation(canidate);

            Assert.AreEqual(result, "[[[[abc()]]]]");
        }

        #endregion

        #region DataList From Defs

        [TestMethod]
        public void Extract_Index_From_Recordset()
        {
            string shape = @"<DL><recset><a/></recset></DL>";

            string exp = "[[recset(1)]]";

            IDev2DataLanguageParser parser = DataListFactory.CreateLanguageParser();
            IList<IIntellisenseResult> results = parser.ParseDataLanguageForIntellisense(exp, shape, true);

            Assert.IsTrue(results.Count == 1 && results[0].Option.Recordset == "recset" && results[0].Option.RecordsetIndex == "1");

        }

        [TestMethod]
        public void Extract_Recordset()
        {
            string shape = @"<DL><recset><a/></recset></DL>";

            string exp = "[[recset()]]";

            IDev2DataLanguageParser parser = DataListFactory.CreateLanguageParser();
            IList<IIntellisenseResult> results = parser.ParseDataLanguageForIntellisense(exp, shape, true);

            Assert.IsTrue(results.Count == 1 && results[0].Option.Recordset == "recset" && results[0].Option.RecordsetIndex == "");

        }

        #endregion

    }
}
