using System;
using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests.Dev2.Application.Server.Tests.Bpm_unit_tests
{
    /// <summary>
    /// Summary description for DBServiceTest
    /// </summary>
    [TestClass]
    public class DBServiceTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        readonly string WebserverURI = ServerSettings.WebserverURI;

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
        public void CanExecuteDbServiceAndReturnItsOutput()
        {
            string PostData = String.Format("{0}{1}", ServerSettings.WebserverURI, "Bug9139");
            string expected = @"<DataList><result>PASS</result></DataList>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            StringAssert.Contains(ResponseData, expected, "Expected [ " + expected + " ] But Got [ " + ResponseData + " ]");
        }

        [TestMethod]
        public void CanReturnDataInCorrectCase()
        {

            string PostData = String.Format("{0}{1}", WebserverURI, "Bug9490");
            string expected = @"<result><val>abc_def_hij</val></result><result><val>ABC_DEF_HIJ</val></result>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);


            StringAssert.Contains(ResponseData, expected);

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DatabaseService_Mapping")]
        public void DatabaseService_MappedOutputsFetchedInInnerWorkflow_WhenFetchedWithDiffernedColumnsThanFetched_DataReturned()
        {

            //------------Setup for test--------------------------
            string PostData = String.Format("{0}{1}", WebserverURI, "Bug 10475 Outer WF");
            string expected = @"<Rows><ID>1</ID></Rows>";

            //------------Execute Test---------------------------
            string ResponseData = TestHelper.PostDataToWebserver(PostData);


            //------------Assert Results-------------------------
            StringAssert.Contains(ResponseData, expected);

        }

    }
}
