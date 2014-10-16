
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace
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
            string postData = String.Format("{0}{1}", ServerSettings.WebserverURI, "Integration Test Resources/Bug_10247_Outter");
            string expected = @"<rs  index=""1""><result>2</result></rs><rs index=""2""><result>3</result></rs>";

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
            string PostData = String.Format("{0}{1}", ServerSettings.WebserverURI, "Integration Test Resources/Bug9490");
            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            const string expected1 = "<result index=\"1\"><val>abc_def_hij</val></result><result index=\"2\"><val>ABC_DEF_HIJ</val></result>";

            StringAssert.Contains(ResponseData, expected1, "But Got [ " + ResponseData + " ]");
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DBService_Execute")]
        public void DBervice_Execute_WhenForEachWithDifferentColumnMappings_ExpectPass()
        {
            //------------Setup for test--------------------------
            string postData = String.Format("{0}{1}", ServerSettings.WebserverURI, "Integration Test Resources/ForEach DB Test");

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
            string postData = String.Format("{0}{1}?{2}", ServerSettings.WebserverURI, "Integration Test Resources/Service Serialization Test", "");

            //------------Execute Test---------------------------
            string result = TestHelper.PostDataToWebserver(postData);

            //------------Assert Results-------------------------
            StringAssert.Contains(result, "<Result>PASS</Result>");
        }


        [TestMethod]
        public void TestDBNullInsert_Expected_clientID()
        {
            string postData = String.Format("{0}{1}?{2}", ServerSettings.WebserverURI, "Integration Test Resources/IntegrationTestDBEmptyToNull", "testType=insert");
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
                string postData = String.Format("{0}{1}?{2}", ServerSettings.WebserverURI, "Integration Test Resources/IntegrationTestDBEmptyToNull", "testType=nullActive");
                string result = TestHelper.PostDataToWebserver(postData);
                StringAssert.Contains(result, "<val>ZZZ</val>", "Got [ " + result + " ]");
            }
        }

        [TestMethod]
        public void TestDBNullLogicNotNullValue_Expected_AAA()
        {
            string postData = String.Format("{0}{1}?{2}", ServerSettings.WebserverURI, "Integration Test Resources/IntegrationTestDBEmptyToNull", "testType=logic&nullLogicValue=dummy");
            string result = TestHelper.PostDataToWebserver(postData);
            StringAssert.Contains(result, "<val>AAA</val>", "Got [ " + result + " ]");
        }

        [TestMethod]
        public void TestDBNullLogicEmptyNullConvertOffValue_Expected_ZZZ()
        {
            string postData = String.Format("{0}{1}?{2}", ServerSettings.WebserverURI, "Integration Test Resources/IntegrationTestDBEmptyToNull", "testType=nullActive");
            string result = TestHelper.PostDataToWebserver(postData);
            StringAssert.Contains(result, "<val>ZZZ</val>", "Got [ " + result + " ]");
        }

        [TestMethod]
        public void WorkflowWithDBActivity_Integration_ExpectedReturnsDatabaseData()
        {
            //------------Setup for test--------------------------
            string postData = String.Format("{0}{1}?{2}", ServerSettings.WebserverURI, "Integration Test Resources/PBI9135DBServiceTest", "");
            //------------Execute Test---------------------------
            string result = TestHelper.PostDataToWebserver(postData);
            //------------Assert Results-------------------------
            const string expectedReturnValue = @"<Countries index=""1""><CountryID>127</CountryID><Description>Solomon Islands</Description></Countries><Countries index=""2""><CountryID>128</CountryID><Description>Somalia</Description></Countries><Countries index=""3""><CountryID>129</CountryID><Description>South Africa</Description></Countries>";
            StringAssert.Contains(result, expectedReturnValue);
        }


    }
}
