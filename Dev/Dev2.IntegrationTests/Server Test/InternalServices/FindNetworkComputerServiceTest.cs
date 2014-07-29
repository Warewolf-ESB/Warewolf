using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests.Dev2.Application.Server.Tests.InternalServices
{
    /// <summary>
    /// Summary description for FindNetworkComputerServiceTest
    /// </summary>
    [TestClass]
    public class FindNetworkComputerServiceTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void Service_Returns_JSON()
        {

            string postData = ServerSettings.WebserverURI + "FindNetworkComputersService";

            string result = TestHelper.PostDataToWebserver(postData);

            // It is JSON if the first two chars are [{, else it will be HTML
            StringAssert.Contains(result, "[{");
            StringAssert.Contains(result, "}]");

        }
    }
}
