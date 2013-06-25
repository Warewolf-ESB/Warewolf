using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.DataList.Contract;
using Dev2.Common;

namespace Dev2.Data.Tests.ConverterTest
{
    /// <summary>
    /// Summary description for DataListConverterTest
    /// </summary>
    [TestClass]
    public class DataListConverterTest
    {
        public DataListConverterTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

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

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void CanConvertFromDataListXMLTranslatorWithOutSystemTags()
        {
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

            ErrorResultTO errors;
            Guid dlID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), "<root><scalar>s1</scalar><rs><val>1</val><val>2</val></rs></root>", "<root><scalar/><rs><val/></rs></root>", out errors);
            string data = compiler.ConvertFrom(dlID, DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), enTranslationDepth.Data, out errors);

            string expected = "<DataList><scalar>s1</scalar><rs><val>2</val></rs></DataList>";

            Assert.AreEqual(expected, data, "Expected [ " + expected + " ] but got [ " + data + " ]");
        }

        [TestMethod]
        public void CanConvertFromWithSingleScalarDataListXMLTranslatorWithOutSystemTags()
        {
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

            ErrorResultTO errors;
            Guid dlID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), "<root><scalar>s1</scalar></root>", "<root><scalar/></root>", out errors);
            string data = compiler.ConvertFrom(dlID, DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), enTranslationDepth.Data, out errors);

            string expected = "<DataList><scalar>s1</scalar></DataList>";

            Assert.AreEqual(expected, data, "Expected [ " + expected + " ] but got [ " + data + " ]");
        }
    }
}
