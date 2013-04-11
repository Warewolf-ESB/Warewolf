using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Integration.Tests.Helpers;

namespace Dev2.Integration.Tests.Dev2.Application.Server.Tests.InternalServices
{
    /// <summary>
    /// Summary description for FindNetworkComputerServiceTest
    /// </summary>
    [TestClass]
    public class FindNetworkComputerServiceTest
    {
        public FindNetworkComputerServiceTest()
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
        public void Service_Returns_JSON()
        {

            string postData = ServerSettings.WebserverURI + "FindNetworkComputersService";

            string result = TestHelper.PostDataToWebserver(postData);

            // It is JSON if the first two chars are [{, else it will be HTML
            StringAssert.Contains(result, "<JSON>[{");
            StringAssert.Contains(result, "}]</JSON>");
//            Assert.IsTrue(result.IndexOf("[{") == 0);

        }
    }
}
