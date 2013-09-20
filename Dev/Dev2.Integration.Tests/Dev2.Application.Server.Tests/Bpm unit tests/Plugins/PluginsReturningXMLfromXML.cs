using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Integration.Tests.Helpers;

namespace Dev2.Integration.Tests.Dev2.Application.Server.Tests.Bpm_unit_tests.Plugins
{
    [TestClass]
    public class PluginsReturningXMLfromXML
    {
        [TestMethod]
        public void TestPluginReturnsXMLFromXML()
        {
            string PostData = String.Format("{0}{1}", ServerSettings.WebserverURI, "PluginsReturningXMLfromXML");

            string expected = @"<Names><CompanyName>Dev2</CompanyName><DepartmentName>Dev</DepartmentName><EmployeeName>Brendon</EmployeeName></Names><Names><CompanyName>Dev2</CompanyName><DepartmentName>Dev</DepartmentName><EmployeeName>Jayd</EmployeeName></Names><Names><CompanyName>Dev2</CompanyName><DepartmentName>Accounts</DepartmentName><EmployeeName>Bob</EmployeeName></Names><Names><CompanyName>Dev2</CompanyName><DepartmentName>Accounts</DepartmentName><EmployeeName>Joe</EmployeeName></Names>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            StringAssert.Contains(ResponseData, expected," **** I expected { " + expected + " } but got { " + ResponseData + " }");
        }
    }
}
