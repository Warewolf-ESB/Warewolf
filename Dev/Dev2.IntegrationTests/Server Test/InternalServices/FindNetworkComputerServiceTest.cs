
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


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
