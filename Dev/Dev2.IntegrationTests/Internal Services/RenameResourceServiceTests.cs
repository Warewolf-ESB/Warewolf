
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests.Internal_Services
{
    /// <summary>
    /// Summary description for SystemServices
    /// </summary>
    [TestClass]
    public class RenameResourceServicesTest
    {
        private readonly string _webServerURI = ServerSettings.WebserverURI;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        [Description("Can call to RenameResource with correct arguments")]
        [Owner("Ashley Lewis")]
        public void RenameResourceService_WithValidArguments_ExpectSuccessResult()
        {
            string postData = string.Format("{0}{1}?{2}", _webServerURI, "RenameResourceService", "ResourceID=b88337db-36f5-4089-98b6-9cfd1925fc13&NewName=New-Test-Resource-Name&ResourceType=WorkflowService");
            string actual = TestHelper.PostDataToWebserver(postData);
            Assert.IsFalse(string.IsNullOrEmpty(actual));
            StringAssert.Contains(actual, "Renamed Resource 'b88337db-36f5-4089-98b6-9cfd1925fc13' to 'New-Test-Resource-Name'");
        }

    }
}
