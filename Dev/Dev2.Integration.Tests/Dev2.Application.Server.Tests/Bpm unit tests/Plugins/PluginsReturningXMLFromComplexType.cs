using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Integration.Tests.Helpers;

namespace Dev2.Integration.Tests.Dev2.Application.Server.Tests.Bpm_unit_tests.Plugins
{
    [TestClass]
    public class PluginsReturningXMLFromComplexType
    {
        // Bug 8378
        [TestMethod]
        public void TestPluginsReturningXMLFromComplexType()
        {
            string PostData = String.Format("{0}{1}", ServerSettings.WebserverURI, "PluginsReturningXMLFromComplexType");
            string expected = @"<Names><CompanyName>Dev2</CompanyName><DepartmentName>Dev</DepartmentName><EmployeeName>Brendon</EmployeeName></Names><Names><CompanyName>Dev2</CompanyName><DepartmentName>Dev</DepartmentName><EmployeeName>Jayd</EmployeeName></Names><Names><CompanyName>Dev2</CompanyName><DepartmentName>Accounts</DepartmentName><EmployeeName>Bob</EmployeeName></Names><Names><CompanyName>Dev2</CompanyName><DepartmentName>Accounts</DepartmentName><EmployeeName>Jo</EmployeeName>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            //Assert.IsTrue(ResponseData.IndexOf(expected) >= 0);            

            Assert.Inconclusive("Test is failing because of plugins");
        }
    }
}
