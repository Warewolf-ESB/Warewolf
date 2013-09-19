using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Integration.Tests.Helpers;

namespace Dev2.Integration.Tests.Dev2.Activities.Tests
{
    /// <summary>
    /// Summary description for DsfCalculateActivityTest
    /// </summary>
    [TestClass][Ignore]//Ashley: One of these tests may be causing the server to hang in a background thread, preventing windows 7 build server from performing any more builds
    public class DsfCalculateActivityTest
    {
        public DsfCalculateActivityTest()
        {

        }

        private string _webServerURI = ServerSettings.WebserverURI;

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }



        #region Calculate Static Values Tests

        [TestMethod] // - OK
        public void Calculate_Activity_StaticValueSum_Expected_CorrectlySummed()
        {
            string postData = string.Format("{0}{1}", _webServerURI, "Calculate_ScalarDataListItems_Multiplication");

            string expected = @"57.4456264653803";

            string actual = TestHelper.PostDataToWebserver(postData);

            Assert.IsTrue(actual.Contains(expected));
        }

        #endregion Calculate Static Values Tests

        #region Calculate Scalar Tests

        // You read a horriable comment because people cannot do their job!

        #endregion Calculate Scalar Tests

        #region Calculate RecordSet Tests

        [TestMethod] // - OK      
        public void Calculate_Activity_RecordSetEvaluation_Expected_RecordSetResolutionCorrectlyEvaluated()
        {
            string postData = string.Format("{0}{1}", _webServerURI, "Calculate_RecordSet_Subtract");
            string expected = @"<result>920</result>";

            string actual = TestHelper.PostDataToWebserver(postData);

            Assert.IsTrue(actual.Contains(expected));

        }

        #endregion Calculate RecordSet Tests
    }
}
