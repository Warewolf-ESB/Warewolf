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
    public class DbServiceTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        readonly string _webserverURI = ServerSettings.WebserverURI;

        [TestMethod]
        public void CanExecuteDbServiceAndReturnItsOutput()
        {
            string postData = String.Format("{0}{1}", ServerSettings.WebserverURI, "Bug9139");
            const string expected = @"<DataList><result>PASS</result></DataList>";

            string responseData = TestHelper.PostDataToWebserver(postData);

            StringAssert.Contains(responseData, expected, "Expected [ " + expected + " ] But Got [ " + responseData + " ]");
        }

        [TestMethod]
        public void CanReturnDataInCorrectCase()
        {

            string postData = String.Format("{0}{1}", _webserverURI, "Bug9490");
            const string expected = @"<result><val>abc_def_hij</val></result><result><val>ABC_DEF_HIJ</val></result>";

            string responseData = TestHelper.PostDataToWebserver(postData);


            StringAssert.Contains(responseData, expected);

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DatabaseService_Mapping")]
        public void DatabaseService_MappedOutputsFetchedInInnerWorkflow_WhenFetchedWithDiffernedColumnsThanFetched_DataReturned()
        {

            //------------Setup for test--------------------------
            string postData = String.Format("{0}{1}", _webserverURI, "Bug 10475 Outer WF");
            const string expected = @"<Row><ID>1</ID></Row>";

            //------------Execute Test---------------------------
            string responseData = TestHelper.PostDataToWebserver(postData);


            //------------Assert Results-------------------------
            StringAssert.Contains(responseData, expected);

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DatabaseService_Mapping")]
        public void DatabaseService_WithInputsAndNoOutputs_WhenInsertingFromDataList_SameDataReturned()
        {

            //------------Setup for test--------------------------
            string postData = String.Format("{0}{1}", _webserverURI, "DB Service With No Output");
            const string expected = @"<Result>PASS</Result>";

            //------------Execute Test---------------------------
            string responseData = TestHelper.PostDataToWebserver(postData);


            //------------Assert Results-------------------------
            StringAssert.Contains(responseData, expected);

        }


        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DatabaseService_Mapping")]
        public void DatabaseService_CanMapToMultipleRecordsets_WhenStraightFromDBService_ExpectPass()
        {

            //------------Setup for test--------------------------
            string postData = String.Format("{0}{1}", _webserverURI, "Service Output To Multiple Recordsets");
            const string expected = @"<result>PASS</result>";

            //------------Execute Test---------------------------
            string responseData = TestHelper.PostDataToWebserver(postData);


            //------------Assert Results-------------------------
            StringAssert.Contains(responseData, expected);

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
