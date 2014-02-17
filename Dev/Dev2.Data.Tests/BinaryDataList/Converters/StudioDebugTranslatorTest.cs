using Dev2.Common;
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
    }
}
