using Dev2.Common;
using Dev2.Data.Binary_Objects;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests.BinaryDataList.Converters
{
    /// <summary>
    /// Summary description for StudioDebugTranslatorTest
    /// </summary>
    [TestClass]
    public class StudioDebugTranslatorTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        // ReSharper disable InconsistentNaming

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("StudioDebugTranslator_ConvertAndOnlyMapInputs")]
        public void StudioDebugTranslator_ConvertAndOnlyMapInputs_WhenCallingNormally_ExpectNotImplementedException()
        {
            //------------Setup for test--------------------------
            var compiler = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;

            //------------Execute Test---------------------------
            compiler.ConvertAndOnlyMapInputs(DataListFormat.CreateFormat(GlobalConstants._Studio_Debug_XML), string.Empty, string.Empty, out errors);

            //------------Assert Results-------------------------
            var theErrors = errors.FetchErrors();
            Assert.AreEqual(1, theErrors.Count);
            StringAssert.Contains(theErrors[0], "The method or operation is not implemented.");

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("StudioDebugTranslator_ConvertTo")]
        public void StudioDebugTranslator_ConvertTo_WhenRecordsetColumnMappedAsInput_ExpectDataListWithColumnMappingDirection()
        {
            //------------Setup for test--------------------------
            var compiler = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;

            const string data = "<DataList></DataList>";
            const string shape = @"<DataList><rec Description="""" IsEditable=""True"" ColumnIODirection=""None"" ><a Description="""" IsEditable=""True"" ColumnIODirection=""Input"" /><b Description="""" IsEditable=""True"" ColumnIODirection=""None"" /></rec></DataList>";

            //------------Execute Test---------------------------
            var dlID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._Studio_Debug_XML), data, shape, out errors);
            var bdl = compiler.FetchBinaryDataList(dlID, out errors);

            //------------Assert Results-------------------------
            Assert.IsFalse(bdl.HasErrors());

            var recs = bdl.FetchRecordsetEntries();
            Assert.AreEqual(1, recs.Count);
            var cols = recs[0].Columns;
            Assert.AreEqual(2, cols.Count);
            var magicCol = cols[0];
            Assert.AreEqual("a", magicCol.ColumnName);
            Assert.AreEqual(enDev2ColumnArgumentDirection.Input, magicCol.ColumnIODirection);
            var nonMagicCol = cols[1];
            Assert.AreEqual("b", nonMagicCol.ColumnName);
            Assert.AreEqual(enDev2ColumnArgumentDirection.None, nonMagicCol.ColumnIODirection);

        }

        // ReSharper restore InconsistentNaming
    }
}
