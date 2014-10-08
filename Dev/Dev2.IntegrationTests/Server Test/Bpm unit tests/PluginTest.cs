
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
using Dev2.Common.ExtMethods;
using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace
namespace Dev2.Integration.Tests.Dev2.Application.Server.Tests.Bpm_unit_tests
{
    /// <summary>
    /// Summary description for PluginTest
    /// </summary>
    [TestClass]
    public class PluginTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("Plugins_PrimitiveTypes")]
        public void Plugins_PrimitiveTypes_WhenTestingVariousPrimitivesTypes_ExpectPASS()
        {
            //------------Setup for test--------------------------
            string postData = String.Format("{0}{1}", ServerSettings.WebserverURI, "Integration Test Resources/PrimitiveReturnTypeTest");
            const string expected = @"<Result>PASS</Result>";

            //------------Execute Test---------------------------
            string responseData = TestHelper.PostDataToWebserver(postData);

            //------------Assert Results-------------------------
            StringAssert.Contains(responseData.Unescape(), expected);
        }

        [TestMethod]
        [TestCategory("PluginIntegrationTest")]
        [Description("Test for executing a remote plugin, specific data is expected to be returned back")]
        [Owner("Ashley Lewis")]
        public void Plugins_PluginIntegrationTest_Execution_CorrectResponse()
        {
            string postData = String.Format("{0}{1}", ServerSettings.WebserverURI, "Integration Test Resources/BUG_9966_RemotePlugins");
            const string expected = @"<DataList><Message>Exception of type 'HgCo.WindowsLive.SkyDrive.LogOnFailedException' was thrown.</Message></DataList>";

            string responseData = TestHelper.PostDataToWebserver(postData);

            StringAssert.Contains(responseData.Unescape(), expected);
        }

        // This test is failing because the data does not bind to it ;)
        [TestMethod]
        public void TestPluginReturnsXMLFromXML()
        {
            string postData = String.Format("{0}{1}", ServerSettings.WebserverURI, "Integration Test Resources/PluginsReturningXMLfromXML");

            const string expected = @"<DataList><Company_Departments index=""1""><DepartmentName>Dev</DepartmentName></Company_Departments><Company_Departments index=""2""><DepartmentName>Accounts</DepartmentName></Company_Departments><Company_Departments_Department_Employees index=""1""><PersonName>Brendon</PersonName><PersonSurename>Page</PersonSurename></Company_Departments_Department_Employees><Company_Departments_Department_Employees index=""2""><PersonName>Jayd</PersonName><PersonSurename>Page</PersonSurename></Company_Departments_Department_Employees><Company_Departments_Department_Employees index=""3""><PersonName>Bob</PersonName><PersonSurename>Soap</PersonSurename></Company_Departments_Department_Employees><Company_Departments_Department_Employees index=""4""><PersonName>Joe</PersonName><PersonSurename>Pants</PersonSurename></Company_Departments_Department_Employees><Company index=""1""><InlineRecordSet>
        RandomData
    </InlineRecordSet></Company><Company index=""2""><InlineRecordSet>
        RandomData1
    </InlineRecordSet></Company><Company_OuterNestedRecordSet index=""1""><InnerNestedRecordSetItemValue>val1</InnerNestedRecordSetItemValue></Company_OuterNestedRecordSet><Company_OuterNestedRecordSet index=""2""><InnerNestedRecordSetItemValue>val2</InnerNestedRecordSetItemValue></Company_OuterNestedRecordSet><Company_OuterNestedRecordSet index=""3""><InnerNestedRecordSetItemValue>val3</InnerNestedRecordSetItemValue></Company_OuterNestedRecordSet><Company_OuterNestedRecordSet index=""4""><InnerNestedRecordSetItemValue>val4</InnerNestedRecordSetItemValue></Company_OuterNestedRecordSet></DataList>";

            string responseData = TestHelper.PostDataToWebserver(postData);

            StringAssert.Contains(responseData.Unescape(), expected, " **** I expected { " + expected + " } but got { " + responseData + " }");
        }

        // Bug 8378
        [TestMethod]
        public void TestPluginsReturningXMLFromComplexType()
        {
            string postData = String.Format("{0}{1}", ServerSettings.WebserverURI, "Integration Test Resources/PluginsReturningXMLFromComplexType");
            const string expected = @"<Departments index=""1""><Name>Dev</Name><EmployeesCapacity>4</EmployeesCapacity><EmployeesCount>2</EmployeesCount></Departments><Departments index=""2""><Name>Accounts</Name><EmployeesCapacity>4</EmployeesCapacity><EmployeesCount>2</EmployeesCount></Departments><Departments_Employees index=""1""><Name>Brendon</Name></Departments_Employees><Departments_Employees index=""2""><Name>Jayd</Name></Departments_Employees><Departments_Employees index=""3""><Name>Bob</Name></Departments_Employees><Departments_Employees index=""4""><Name>Jo</Name></Departments_Employees>";

            string responseData = TestHelper.PostDataToWebserver(postData);

            Assert.IsTrue(responseData.IndexOf(expected, StringComparison.Ordinal) >= 0, "Got [ " + responseData + " ]");
        }

        // Bug 8378
        [TestMethod]
        public void TestPluginsReturningXMLFromJson()
        {
            string postData = String.Format("{0}{1}", ServerSettings.WebserverURI, "Integration Test Resources/PluginsReturningXMLFromJson");
            string expected = @"<DataList><PrimitiveRecordset index=""1""><PrimitiveRecordse>
        RandomData
    </PrimitiveRecordse></PrimitiveRecordset><PrimitiveRecordset index=""2""><PrimitiveRecordse>
        RandomData1
    </PrimitiveRecordse></PrimitiveRecordset><Departments_Employees index=""1""><Surename>Page</Surename></Departments_Employees><Departments_Employees index=""2""><Surename>Page</Surename></Departments_Employees><Departments_Employees index=""3""><Surename>Soap</Surename></Departments_Employees><Departments_Employees index=""4""><Surename>Pants</Surename></Departments_Employees><Contractors index=""1""><PhoneNumber>123</PhoneNumber></Contractors><Contractors index=""2""><PhoneNumber>1234</PhoneNumber></Contractors><Contractors index=""3""><PhoneNumber>1235</PhoneNumber></Contractors><Contractors index=""4""><PhoneNumber>1236</PhoneNumber></Contractors></DataList>";

            string responseData = TestHelper.PostDataToWebserver(postData);

            expected = TestHelper.CleanUp(expected);
            responseData = TestHelper.CleanUp(responseData);

            StringAssert.Contains(responseData, expected, " **** I expected { " + expected + " } but got { " + responseData + " }");
        }
    }
}
