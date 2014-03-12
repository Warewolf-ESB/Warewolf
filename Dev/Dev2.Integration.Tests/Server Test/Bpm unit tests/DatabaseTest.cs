using System;
using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests.Server_Test.Bpm_unit_tests
{
    /// <summary>
    /// Summary description for DatabaseTest
    /// </summary>
    [TestClass]
    public class DatabaseTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }


        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("RecordsetMapping_NestedWorkflows")]
        // Ensure we can map portions of a recordset as input and other portions as output
        public void RecordsetMapping_NestedWorkflows_MixedInputAndOutput_ExpectValidResult()
        {
            //------------Setup for test--------------------------
            string postData = String.Format("{0}{1}", ServerSettings.WebserverURI, "Bug_10247_Outter");
            string expected = @"<rs  rowID=""1""><result>2</result></rs><rs rowID=""2""><result>3</result></rs>";

            //------------Execute Test---------------------------
            string responseData = TestHelper.PostDataToWebserver(postData);

            //------------Assert Results-------------------------

            // Standardize the outputs (Remove newlines, etc)
            expected = TestHelper.CleanUp(expected);
            responseData = TestHelper.CleanUp(responseData);
            StringAssert.Contains(responseData, expected);

        }

        [TestMethod]
        public void DataBaseTest_CanDbServiceReturnCorrectCase()
        {
            string PostData = String.Format("{0}{1}", ServerSettings.WebserverURI, "Bug9490");
            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            const string expected1 = "<result rowID=\"1\"><val>abc_def_hij</val></result><result rowID=\"2\"><val>ABC_DEF_HIJ</val></result>";

            StringAssert.Contains(ResponseData, expected1, "But Got [ " + ResponseData + " ]");
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DBService_Execute")]
        public void DBervice_Execute_WhenForEachWithDifferentColumnMappings_ExpectPass()
        {
            //------------Setup for test--------------------------
            string postData = String.Format("{0}{1}", ServerSettings.WebserverURI, "ForEach DB Test");

            //------------Execute Test---------------------------
            string result = TestHelper.PostDataToWebserver(postData);

            //------------Assert Results-------------------------
            StringAssert.Contains(result, "<Result>PASS</Result>");
        }


        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DBService_Execute")]
        public void DBervice_Execute_WhenDataTableUsedAndHtmlRetured_ExpectPass()
        {
            //------------Setup for test--------------------------
            string postData = String.Format("{0}{1}?{2}", ServerSettings.WebserverURI, "Service Serialization Test", "");

            //------------Execute Test---------------------------
            string result = TestHelper.PostDataToWebserver(postData);

            //------------Assert Results-------------------------
            StringAssert.Contains(result, "<Result>PASS</Result>");
        }


        [TestMethod]
        public void TestDBNullInsert_Expected_clientID()
        {
            string postData = String.Format("{0}{1}?{2}", ServerSettings.WebserverURI, "IntegrationTestDBEmptyToNull", "testType=insert");
            string result = TestHelper.PostDataToWebserver(postData);
            StringAssert.Contains(result, "<userID>");
        }

        [TestMethod]
        [TestCategory("WebURI, DB")]
        public void TestDBNullLogicNullValue_Expected_ZZZ_10Times()
        {
            // ensure we get the same result 10 times ;)
            for(int i = 0; i < 10; i++)
            {
                string postData = String.Format("{0}{1}?{2}", ServerSettings.WebserverURI, "IntegrationTestDBEmptyToNull", "testType=logic&nullLogicValue=");
                string result = TestHelper.PostDataToWebserver(postData);
                StringAssert.Contains(result, "<val>ZZZ</val>", "Got [ " + result + " ]");
            }
        }

        [TestMethod]
        public void TestDBNullLogicNotNullValue_Expected_AAA()
        {
            string postData = String.Format("{0}{1}?{2}", ServerSettings.WebserverURI, "IntegrationTestDBEmptyToNull", "testType=logic&nullLogicValue=dummy");
            string result = TestHelper.PostDataToWebserver(postData);
            StringAssert.Contains(result, "<val>AAA</val>", "Got [ " + result + " ]");
        }

        [TestMethod]
        public void TestDBNullLogicEmptyNullConvertOffValue_Expected_AAA()
        {
            string postData = String.Format("{0}{1}?{2}", ServerSettings.WebserverURI, "IntegrationTestDBEmptyToNull", "testType=nullActive&nullLogicValue=");
            string result = TestHelper.PostDataToWebserver(postData);
            StringAssert.Contains(result, "<val>ZZZ</val>", "Got [ " + result + " ]");
        }

        [TestMethod]
        public void WorkflowWithDBActivity_Integration_ExpectedReturnsDatabaseData()
        {
            //------------Setup for test--------------------------
            string postData = String.Format("{0}{1}?{2}", ServerSettings.WebserverURI, "PBI9135DBServiceTest", "");
            //------------Execute Test---------------------------
            string result = TestHelper.PostDataToWebserver(postData);
            //------------Assert Results-------------------------
            const string expectedReturnValue = @"<Countries rowID=""1""><CountryID>127</CountryID><Description>Solomon Islands</Description></Countries><Countries rowID=""2""><CountryID>128</CountryID><Description>Somalia</Description></Countries><Countries rowID=""3""><CountryID>129</CountryID><Description>South Africa</Description></Countries>";
            StringAssert.Contains(result, expectedReturnValue);
        }  
        

    }
}
