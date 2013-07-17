using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests.Internal_Services
{
    /// <summary>
    /// Summary description for SystemServices
    /// </summary>
    [TestClass]
    public class GetCurrentLogFileServicesTest
    {
        private string _webServerURI = ServerSettings.WebserverURI;

        public GetCurrentLogFileServicesTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

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

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void FetchCurrentServerLogReturnsLogDataDeletesLogFile()
        {
            string postData = string.Format("{0}{1}?{2}", _webServerURI, "FetchCurrentServerLogService", "");
            string actual = TestHelper.PostDataToWebserver(postData);
            Assert.IsFalse(string.IsNullOrEmpty(actual));
            actual = TestHelper.PostDataToWebserver(postData);
            Assert.AreEqual("<DataList><Dev2System.ManagmentServicePayload></Dev2System.ManagmentServicePayload></DataList>", actual);
        }
        
    }
}