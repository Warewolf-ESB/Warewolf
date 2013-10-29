using System;
using System.Xml.Linq;
using Dev2.Integration.Tests.Dev2.Application.Server.Tests.Workspace.XML;
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

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DatabaseService_Mapping")]
        public void DatabaseService_WithInputsAndNoOutputs_WhenInsertingFromDataList_SameDataReturned()
        {

            //------------Setup for test--------------------------
            string PostData = String.Format("{0}{1}", WebserverURI, "DB Service With No Output");
            string expected = @"<Result>PASS</Result>";

            //------------Execute Test---------------------------
            string ResponseData = TestHelper.PostDataToWebserver(PostData);


            //------------Assert Results-------------------------
            StringAssert.Contains(ResponseData, expected);

        }


        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DatabaseService_Mapping")]
        public void DatabaseService_CanMapToMultipleRecordsets_WhenStraightFromDBService_ExpectPass()
        {

            //------------Setup for test--------------------------
            string PostData = String.Format("{0}{1}", WebserverURI, "Service Output To Multiple Recordsets");
            string expected = @"<result>PASS</result>";

            //------------Execute Test---------------------------
            string ResponseData = TestHelper.PostDataToWebserver(PostData);


            //------------Assert Results-------------------------
            StringAssert.Contains(ResponseData, expected);

        }

        [TestMethod]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("DatabaseService_Execute")]
        public void DatabaseService_Execute_CustomOutputMappings_DataReturned()
        {
            //------------Setup for test--------------------------
            var postData = String.Format("{0}{1}", WebserverURI, "10638 - Service IO - TEST");

            var expectedXml = XmlResource.Fetch("BUG_10638_Result.xml");
            var expected = expectedXml.ToString(SaveOptions.None);

            //------------Execute Test---------------------------
            var responseData = TestHelper.PostDataToWebserver(postData);

            var actualXml = XElement.Parse(responseData);
            var actual = actualXml.ToString(SaveOptions.None);

            //------------Assert Results-------------------------
            StringAssert.Contains(expected, actual);
        }
    }
}
