
using System;
using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests.Dev2.Application.Server.Tests
{
    /// <summary>
    /// Summary description for DynamicServicesHostTest
    /// </summary>
    [TestClass]
    public class DynamicServicesHostTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get;
            set;
        }


        [TestMethod]
        public void TestPluginNull_Expected_AnonymousSend()
        {
            string postData = String.Format("{0}{1}?{2}", ServerSettings.WebserverURI, "IntegrationTestPluginEmptyToNull", "testType=nullActive&sender=");
            string result = TestHelper.PostDataToWebserver(postData);

            StringAssert.Contains(result, "Anonymous email sent");
        }

        [TestMethod]
        public void TestPluginNonNull_Expected_FromInResult()
        {
            string postData = String.Format("{0}{1}?{2}", ServerSettings.WebserverURI, "IntegrationTestPluginEmptyToNull", "testType=nullActive&sender=test@domain.local");
            string result = TestHelper.PostDataToWebserver(postData);

            StringAssert.Contains(result, "from test@domain.local");

        }


        [TestMethod]
        public void WorkflowWithPluginActivity_Integration_ExpectedReturnsPluginData()
        {
            //------------Setup for test--------------------------
            string postData = String.Format("{0}{1}?{2}", ServerSettings.WebserverURI, "PBI9135PluginServiceTest", "");
            //------------Execute Test---------------------------
            string result = TestHelper.PostDataToWebserver(postData);
            //------------Assert Results-------------------------
            const string expectedReturnValue = "<Name>Dev2</Name><Departments><Name>Dev</Name></Departments><Departments><Name>Accounts</Name></Departments>" +
                                               "<Departments_Employees><Name>Brendon</Name></Departments_Employees><Departments_Employees><Name>Jayd</Name>" +
                                               "</Departments_Employees><Departments_Employees><Name>Bob</Name></Departments_Employees>" +
                                               "<Departments_Employees><Name>Jo</Name></Departments_Employees>";
            StringAssert.Contains(result, expectedReturnValue);
        }
    }
}
