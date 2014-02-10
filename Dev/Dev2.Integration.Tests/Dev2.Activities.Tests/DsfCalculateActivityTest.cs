using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests.Dev2.Activities.Tests
{
    /// <summary>
    /// Summary description for DsfCalculateActivityTest
    /// </summary>
    [TestClass]
    public class DsfCalculateActivityTest
    {
        private readonly string _webServerURI = ServerSettings.WebserverURI;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

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
