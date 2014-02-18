using System;
using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests.Dev2.Application.Server.Tests.Bpm_unit_tests
{
    /// <summary>
    /// Summary description for WebServiceTest
    /// </summary>
    [TestClass]
    public class WebServiceTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("WebService_Invoke")]
        public void WebService_Invoke_IntegrationTest_ExpectPass()
        {

            //------------Setup for test--------------------------
            string postData = String.Format("{0}{1}", ServerSettings.WebserverURI, "WebServiceTest");

            const string expected = @"<result>PASS</result>";

            //------------Execute Test---------------------------
            string ResponseData = TestHelper.PostDataToWebserver(postData);

            ////------------Assert Results-------------------------
            StringAssert.Contains(ResponseData, expected, " **** I expected { " + expected + " } but got { " + ResponseData + " }");
        }

    }
}
