using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests.Dev2.Application.Server.Tests.InternalServices {
    /// <summary>
    /// Summary description for FindDependenciesServiceTest
    /// </summary>
    [TestClass]
    public class FindDependenciesServiceTest {
        
        private readonly string _webserverURI = ServerSettings.WebserverURI;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void FindDependencies_ExistingService_Expected_AllDependanciesReturned() {
            string postData = String.Format("{0}{1}", _webserverURI, "FindDependencyService?ResourceName=TestCategory\\Bug9245");
            XElement response = XElement.Parse(TestHelper.PostDataToWebserver(postData));

            IEnumerable<XNode> nodes = response.DescendantNodes();
            int count = nodes.Count();
            // More than 2 nodes indicate that the service returned dependancies
            Assert.AreEqual(29, count);

        }

    }
}
