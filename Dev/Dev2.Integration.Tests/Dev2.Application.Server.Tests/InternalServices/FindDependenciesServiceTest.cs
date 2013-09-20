using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Integration.Tests.Helpers;
using System.Xml.Linq;

namespace Dev2.Integration.Tests.Dev2.Application.Server.Tests.InternalServices {
    /// <summary>
    /// Summary description for FindDependenciesServiceTest
    /// </summary>
    [TestClass][Ignore]//Ashley: round 2 hunting the evil test
    public class FindDependenciesServiceTest {
        public FindDependenciesServiceTest() {
            //
            // TODO: Add constructor logic here
            //
        }
        private string _webserverURI = ServerSettings.WebserverURI;
        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext {
            get {
                return testContextInstance;
            }
            set {
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
        public void FindDependencies_ExistingService_Expected_AllDependanciesReturned() {
            string postData = String.Format("{0}{1}", _webserverURI, "FindDependencyService?ResourceName=Button");
            XElement response = XElement.Parse(TestHelper.PostDataToWebserver(postData));

            IEnumerable<XNode> nodes = response.DescendantNodes();
            int count = nodes.Count();
            // More than 3 nodes indicate that the service returned dependancies
            Assert.IsTrue(count > 3);

        }

    }
}
