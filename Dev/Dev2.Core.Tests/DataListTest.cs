using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Dev2.DataList.Contract;
using Dev2.Tests.Properties;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DataListTset
{
    /// <summary>
    /// Summary description for check-in
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class DataListTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Output Mapping Test

        [TestMethod]
        public void TestOutputParsingScalar()
        {
            IList<IDev2Definition> defs = DataListFactory.CreateOutputParser().Parse(TestStrings.sampleActivityMappingScalar);

            IDev2Definition d = defs[0];

            Assert.IsTrue(defs.Count == 3 && d.MapsTo == "fname" && d.Value == "firstName" && d.Name == "FirstName" && !d.IsRecordSet);

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DataListFactory_CreateRecordSetCollection")]
        public void DataListFactory_CreateRecordSetCollection_WhenTwoRecordsetsArePresent_ExpectTwoRecordsetDefinitions()
        {
            //------------Setup for test--------------------------
            const string arguments = @"<Outputs><Output Name=""MapLocationID"" MapsTo=""[[MapLocationID]]"" Value=""[[dbo_proc_GetAllMapLocations(*).MapLocationID]]"" Recordset=""dbo_proc_GetAllMapLocations"" /><Output Name=""StreetAddress"" MapsTo=""[[StreetAddress]]"" Value=""[[dbo_proc_GetAllMapLocations2(*).StreetAddress]]"" Recordset=""dbo_proc_GetAllMapLocations"" /><Output Name=""Latitude"" MapsTo=""[[Latitude]]"" Value=""[[dbo_proc_GetAllMapLocations(*).Latitude]]"" Recordset=""dbo_proc_GetAllMapLocations"" /><Output Name=""Longitude"" MapsTo=""[[Longitude]]"" Value=""[[dbo_proc_GetAllMapLocations(*).Longitude]]"" Recordset=""dbo_proc_GetAllMapLocations"" /></Outputs>";
            var defs = DataListFactory.CreateOutputParser().Parse(arguments);

            //------------Execute Test---------------------------
            var result = DataListFactory.CreateRecordSetCollection(defs, true);

            //------------Assert Results-------------------------

            // check counts ;)
            Assert.AreEqual(2, result.RecordSets.Count);
            Assert.AreEqual(3, result.RecordSets[0].Columns.Count);
            Assert.AreEqual(1, result.RecordSets[1].Columns.Count);

            // check set names ;)
            Assert.AreEqual("dbo_proc_GetAllMapLocations", result.RecordSets[0].SetName);
            Assert.AreEqual("dbo_proc_GetAllMapLocations2", result.RecordSets[1].SetName);

            // check #2's columns 
            Assert.AreEqual("StreetAddress", result.RecordSets[1].Columns[0].Name);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataListFactory_CreateRecordSetCollection")]
        public void DataListFactory_ParseAndAllowBlanks_WhenTwoRecordsetsArePresentWithBlankMapsToAndName_ExpectTwoRecordsetDefinitionsBlankColumnNotIncluded()
        {
            //------------Setup for test--------------------------
            const string arguments = @"<Outputs><Output Name=""MapLocationID"" MapsTo=""[[MapLocationID]]"" Value=""[[dbo_proc_GetAllMapLocations(*).MapLocationID]]"" Recordset=""dbo_proc_GetAllMapLocations"" /><Output Name=""StreetAddress"" MapsTo=""[[StreetAddress]]"" Value=""[[dbo_proc_GetAllMapLocations2(*).StreetAddress]]"" Recordset=""dbo_proc_GetAllMapLocations"" /><Output Name="""" MapsTo="""" Value=""[[dbo_proc_GetAllMapLocations(*).Latitude]]"" Recordset=""dbo_proc_GetAllMapLocations"" /><Output Name=""Longitude"" MapsTo=""[[Longitude]]"" Value=""[[dbo_proc_GetAllMapLocations(*).Longitude]]"" Recordset=""dbo_proc_GetAllMapLocations"" /></Outputs>";
            var defs = DataListFactory.CreateOutputParser().ParseAndAllowBlanks(arguments);

            //------------Execute Test---------------------------
            var result = DataListFactory.CreateRecordSetCollection(defs, true);

            //------------Assert Results-------------------------

            // check counts ;)
            Assert.AreEqual(2, result.RecordSets.Count);
            Assert.AreEqual(2, result.RecordSets[0].Columns.Count);
            Assert.AreEqual(1, result.RecordSets[1].Columns.Count);

            // check set names ;)
            Assert.AreEqual("dbo_proc_GetAllMapLocations", result.RecordSets[0].SetName);
            Assert.AreEqual("dbo_proc_GetAllMapLocations2", result.RecordSets[1].SetName);

            // check #2's columns 
            Assert.AreNotEqual("Latitude", result.RecordSets[0].Columns[1].Name);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DataListFactory_CreateRecordSetCollection")]
        public void DataListFactory_CreateRecordSetCollection_WhenInputMappingContainsStar_ExpectValidRecordsetDefinition()
        {
            //------------Setup for test--------------------------
            const string arguments = @"<Inputs><Input Name=""Prefix"" Source=""[[prefix(*).val]]"" /></Inputs>";
            var defs = DataListFactory.CreateInputParser().Parse(arguments);

            //------------Execute Test---------------------------
            var result = DataListFactory.CreateRecordSetCollection(defs, false);

            //------------Assert Results-------------------------

            // check counts ;)
            Assert.AreEqual(1, result.RecordSets.Count);
            Assert.AreEqual(1, result.RecordSets[0].Columns.Count);

            // check set names ;)
            Assert.AreEqual("prefix", result.RecordSets[0].SetName);

            // check #2's columns 
            Assert.AreEqual("Prefix", result.RecordSets[0].Columns[0].Name);
            Assert.AreEqual("prefix(*).val", result.RecordSets[0].Columns[0].MapsTo);
        }

        [TestMethod]
        public void TestOutputParsingRecordSet()
        {
            IList<IDev2Definition> defs = DataListFactory.CreateOutputParser().Parse(TestStrings.sampleActivityMappingRecordSets);
            IRecordSetCollection recCol = DataListFactory.CreateRecordSetCollection(defs, true);

            IDev2Definition d = defs[0];

            Assert.IsTrue(d.IsRecordSet);
            Assert.AreEqual("ppl", recCol.RecordSetNames[0]);
            Assert.AreEqual("ppl().firstName", recCol.RecordSets[0].Columns[0].Value);

        }

        [TestMethod]
        public void TestRecordSetCollectionCreationEnsure1Set()
        {
            IList<IDev2Definition> defs = DataListFactory.CreateOutputParser().Parse(TestStrings.sampleActivityMappingRecordSets);
            IRecordSetCollection recCol = DataListFactory.CreateRecordSetCollection(defs, true);

            Assert.IsTrue((recCol.RecordSetNames.Count == 1));
        }

        [TestMethod]
        public void TestScalarRecordSetMixedParsing()
        {
            IList<IDev2Definition> defs = DataListFactory.CreateOutputParser().Parse(TestStrings.sampleActivityMappingMixed);
            IRecordSetCollection recCol = DataListFactory.CreateRecordSetCollection(defs, true);
            IList<IDev2Definition> scalarList = DataListFactory.CreateScalarList(defs, true);


            Assert.IsTrue(scalarList.Count == 1 && recCol.RecordSetNames.Count == 1);
        }

        #endregion

        #region Util Test

        [TestMethod]
        public void StripDoubleBracketNormal()
        {
            const string canidate = "[[abc()]]";

            string result = DataListCleaningUtils.stripDoubleBracketsAndRecordsetNotation(canidate);

            Assert.AreEqual(result, "abc");
        }

        [TestMethod]
        public void StripDoubleBracketNone()
        {
            const string canidate = "abc()";

            string result = DataListCleaningUtils.stripDoubleBracketsAndRecordsetNotation(canidate);

            Assert.AreEqual(result, "abc()");
        }

        [TestMethod]
        public void StripDoubleBracketRecursiveEval()
        {
            const string canidate = "[[[[abc()]]]]";

            string result = DataListCleaningUtils.stripDoubleBracketsAndRecordsetNotation(canidate);

            Assert.AreEqual(result, "[[[[abc()]]]]");
        }

        #endregion

        #region DataList From Defs

        [TestMethod]
        public void Extract_Index_From_Recordset()
        {
            const string shape = @"<DL><recset><a/></recset></DL>";

            const string exp = "[[recset(1)]]";

            IDev2DataLanguageParser parser = DataListFactory.CreateLanguageParser();
            IList<IIntellisenseResult> results = parser.ParseDataLanguageForIntellisense(exp, shape, true);

            Assert.IsTrue(results.Count == 1 && results[0].Option.Recordset == "recset" && results[0].Option.RecordsetIndex == "1");

        }

        [TestMethod]
        public void Extract_Recordset()
        {
            const string shape = @"<DL><recset><a/></recset></DL>";

            const string exp = "[[recset()]]";

            IDev2DataLanguageParser parser = DataListFactory.CreateLanguageParser();
            IList<IIntellisenseResult> results = parser.ParseDataLanguageForIntellisense(exp, shape, true);

            Assert.IsTrue(results.Count == 1 && results[0].Option.Recordset == "recset" && results[0].Option.RecordsetIndex == "");

        }

        #endregion

    }
}
