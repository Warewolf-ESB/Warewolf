using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Integration.Tests.Helpers;

namespace Dev2.Integration.Tests.Dev2.Application.Server.Tests.Bpm_unit_tests.Plugins
{
    [TestClass]
    [Ignore]
    public class PluginsReturningXMLFromJson
    {
        // Bug 8378
        [TestMethod]
        public void TestPluginsReturningXMLFromJson()
        {
            string PostData = String.Format("{0}{1}", ServerSettings.WebserverURI, "PluginsReturningXMLFromJson");
            string expected = @"<ScalarName>Dev2</ScalarName><Names><DepartmentName>Dev</DepartmentName><EmployeeName>Brendon</EmployeeName></Names><Names><DepartmentName>Dev</DepartmentName><EmployeeName>Jayd</EmployeeName></Names><Names><DepartmentName>Accounts</DepartmentName><EmployeeName>Bob</EmployeeName></Names><Names><DepartmentName>Accounts</DepartmentName><EmployeeName>Joe</EmployeeName></Names><OtherNames><Name>RandomData</Name></OtherNames><OtherNames><Name>RandomData1</Name></OtherNames>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            expected = TestHelper.CleanUp(expected);
            ResponseData = TestHelper.CleanUp(ResponseData);

            StringAssert.Contains(ResponseData, expected, " **** I expected { " + expected + " } but got { " + ResponseData + " }");
        }
    }
}
