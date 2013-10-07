using System;
using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
        [TestCategory("PluginIntegrationTest")]
        [Description("Test for executing a remote plugin, specific data is expected to be returned back")]
        [Owner("Ashley")]
        public void Plugins_PluginIntegrationTest_Execution_CorrectResponse()
        {
            string postData = String.Format("{0}{1}", ServerSettings.WebserverURI, "BUG_9966_RemotePlugins");
            const string expected = @"<Message>Exception of type 'HgCo.WindowsLive.SkyDrive.LogOnFailedException' was thrown.</Message>";

            string responseData = TestHelper.PostDataToWebserver(postData);

            StringAssert.Contains(responseData, expected);
        }

        [TestMethod]
        public void TestPluginReturnsXMLFromXML()
        {
            string postData = String.Format("{0}{1}", ServerSettings.WebserverURI, "PluginsReturningXMLfromXML");

            const string expected = @"<Names><CompanyName>Dev2</CompanyName><DepartmentName>Dev</DepartmentName><EmployeeName>Brendon</EmployeeName></Names><Names><CompanyName>Dev2</CompanyName><DepartmentName>Dev</DepartmentName><EmployeeName>Jayd</EmployeeName></Names><Names><CompanyName>Dev2</CompanyName><DepartmentName>Accounts</DepartmentName><EmployeeName>Bob</EmployeeName></Names><Names><CompanyName>Dev2</CompanyName><DepartmentName>Accounts</DepartmentName><EmployeeName>Joe</EmployeeName></Names>";

            string responseData = TestHelper.PostDataToWebserver(postData);

            StringAssert.Contains(responseData, expected, " **** I expected { " + expected + " } but got { " + responseData + " }");
        }


        // Bug 7820
        [TestMethod]
        [Ignore] // Pick up when Wizards are corrected
        public void TestPluginsReturningPathsFromXML()
        {
            string postData = String.Format("{0}{1}", ServerSettings.WebserverURI, "PluginsReturningPathsFromXML");
            const string expected = @"Company().OuterNestedRecordSet().InnerNestedRecordSet:ItemValue";
            string responseData = TestHelper.PostDataToWebserver(postData);

            Assert.IsTrue(responseData.IndexOf(expected, StringComparison.Ordinal) >= 0);
        }


        // Bug 7820
        [TestMethod]
        [Ignore] // Pick up when Wizards are corrected
        public void TestPluginsReturningPathsFromComplexType()
        {
            string postData = String.Format("{0}{1}", ServerSettings.WebserverURI, "PluginsReturningPathsFromComplexType");
            const string expected = @"<InterrogationResult><z:anyType xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:d1p1=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph.Ouput"" i:type=""d1p1:OutputDescription"" xmlns:z=""http://schemas.microsoft.com/2003/10/Serialization/""><d1p1:DataSourceShapes xmlns:d2p1=""http://schemas.microsoft.com/2003/10/Serialization/Arrays""><d2p1:anyType i:type=""d1p1:DataSourceShape""><d1p1:Paths><d2p1:anyType xmlns:d5p1=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph.Poco"" i:type=""d5p1:PocoPath""><ActualPath xmlns=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph"">Name</ActualPath><DisplayPath xmlns=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph"">Name</DisplayPath><OutputExpression xmlns=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph"" /><SampleData xmlns=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph"">Dev2</SampleData></d2p1:anyType><d2p1:anyType xmlns:d5p1=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph.Poco"" i:type=""d5p1:PocoPath""><ActualPath xmlns=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph"">Departments.Capacity</ActualPath><DisplayPath xmlns=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph"">Departments.Capacity</DisplayPath><OutputExpression xmlns=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph"" /><SampleData xmlns=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph"">4</SampleData></d2p1:anyType><d2p1:anyType xmlns:d5p1=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph.Poco"" i:type=""d5p1:PocoPath""><ActualPath xmlns=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph"">Departments.Count</ActualPath><DisplayPath xmlns=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph"">Departments.Count</DisplayPath><OutputExpression xmlns=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph"" /><SampleData xmlns=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph"">2</SampleData></d2p1:anyType><d2p1:anyType xmlns:d5p1=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph.Poco"" i:type=""d5p1:PocoPath""><ActualPath xmlns=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph"">Departments().Name</ActualPath><DisplayPath xmlns=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph"">Departments().Name</DisplayPath><OutputExpression xmlns=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph"" /><SampleData xmlns=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph"">Dev,Accounts</SampleData></d2p1:anyType><d2p1:anyType xmlns:d5p1=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph.Poco"" i:type=""d5p1:PocoPath""><ActualPath xmlns=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph"">Departments().Employees.Capacity</ActualPath><DisplayPath xmlns=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph"">Departments().Employees.Capacity</DisplayPath><OutputExpression xmlns=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph"" /><SampleData xmlns=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph"">4,4</SampleData></d2p1:anyType><d2p1:anyType xmlns:d5p1=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph.Poco"" i:type=""d5p1:PocoPath""><ActualPath xmlns=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph"">Departments().Employees.Count</ActualPath><DisplayPath xmlns=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph"">Departments().Employees.Count</DisplayPath><OutputExpression xmlns=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph"" /><SampleData xmlns=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph"">2,2</SampleData></d2p1:anyType><d2p1:anyType xmlns:d5p1=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph.Poco"" i:type=""d5p1:PocoPath""><ActualPath xmlns=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph"">Departments().Employees().Name</ActualPath><DisplayPath xmlns=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph"">Departments.Employees().Name</DisplayPath><OutputExpression xmlns=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph"" /><SampleData xmlns=""http://schemas.datacontract.org/2004/07/Unlimited.Framework.Converters.Graph"">Brendon,Jayd,Bob,Jo</SampleData></d2p1:anyType></d1p1:Paths></d2p1:anyType></d1p1:DataSourceShapes><d1p1:Format>ShapedXML</d1p1:Format></z:anyType></InterrogationResult>";

            string responseData = TestHelper.PostDataToWebserver(postData);

            StringAssert.Contains(responseData, expected);
        }

        

        // Bug 8378
        [TestMethod]
        [Ignore] // Pick up when Wizards are corrected
        public void TestPluginsReturningXMLFromComplexType()
        {
            string postData = String.Format("{0}{1}", ServerSettings.WebserverURI, "PluginsReturningXMLFromComplexType");
            const string expected = @"<Names><CompanyName>Dev2</CompanyName><DepartmentName>Dev</DepartmentName><EmployeeName>Brendon</EmployeeName></Names><Names><CompanyName>Dev2</CompanyName><DepartmentName>Dev</DepartmentName><EmployeeName>Jayd</EmployeeName></Names><Names><CompanyName>Dev2</CompanyName><DepartmentName>Accounts</DepartmentName><EmployeeName>Bob</EmployeeName></Names><Names><CompanyName>Dev2</CompanyName><DepartmentName>Accounts</DepartmentName><EmployeeName>Jo</EmployeeName>";

            string responseData = TestHelper.PostDataToWebserver(postData);

            Assert.IsTrue(responseData.IndexOf(expected, StringComparison.Ordinal) >= 0, "Got [ " + responseData + " ]");            
        }

        // Bug 8378
        [TestMethod]
        public void TestPluginsReturningXMLFromJson()
        {
            string postData = String.Format("{0}{1}", ServerSettings.WebserverURI, "PluginsReturningXMLFromJson");
            string expected = @"<ScalarName>Dev2</ScalarName><Names><DepartmentName>Dev</DepartmentName><EmployeeName>Brendon</EmployeeName></Names><Names><DepartmentName>Dev</DepartmentName><EmployeeName>Jayd</EmployeeName></Names><Names><DepartmentName>Accounts</DepartmentName><EmployeeName>Bob</EmployeeName></Names><Names><DepartmentName>Accounts</DepartmentName><EmployeeName>Joe</EmployeeName></Names><OtherNames><Name>RandomData</Name></OtherNames><OtherNames><Name>RandomData1</Name></OtherNames>";

            string responseData = TestHelper.PostDataToWebserver(postData);

            expected = TestHelper.CleanUp(expected);
            responseData = TestHelper.CleanUp(responseData);

            StringAssert.Contains(responseData, expected, " **** I expected { " + expected + " } but got { " + responseData + " }");
        }

        
        // Bug 7820
        [TestMethod]
        [Ignore] // Pick up when Wizards are corrected
        public void TestPluginsReturningPathsFromJson()
        {
            string postData = String.Format("{0}{1}", ServerSettings.WebserverURI, "PluginsReturningPathsFromJson");
            const string expected = @"Departments().Employees().Name</ActualPath>";

            string responseData = TestHelper.PostDataToWebserver(postData);

            Assert.IsTrue(responseData.IndexOf(expected, StringComparison.Ordinal) >= 0);            
        }
    }
}
