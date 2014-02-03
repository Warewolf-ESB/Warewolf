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

            Assert.Fail("fix this to use internal service");

            ////------------Setup for test--------------------------
            //string PostData = String.Format("{0}{1}", ServerSettings.WebserverURI, "WebServiceTest");

            //string expected = @"<result>PASS</result>";

            ////------------Execute Test---------------------------
            //string ResponseData = TestHelper.PostDataToWebserver(PostData);

            ////------------Assert Results-------------------------
            //StringAssert.Contains(ResponseData, expected, " **** I expected { " + expected + " } but got { " + ResponseData + " }"); 
        }

    }
}
