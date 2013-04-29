using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests.Internal_Services
{
    /// <summary>
    /// Summary description for SystemServices
    /// </summary>
    [TestClass]
    public class SystemServicesTest
    {
        private string _webServerURI = ServerSettings.WebserverURI;

        public SystemServicesTest()
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
        public void DepenendcyViewerReturnsOnlyValidDependenciesExpectTwoDependencies()
        {
            string postData = string.Format("{0}{1}?{2}", _webServerURI, "FindDependencyService", "ResourceName=Bug6619");

            // The expected graph to be returned 
            const string expected = @"<graph title=""Dependency Graph Of Bug6619""><node id=""Bug6619"" x="""" y="""" broken=""false""><dependency id=""Bug6619Dep"" /></node><node id=""Bug6619Dep"" x="""" y="""" broken=""false""><dependency id=""Bug6619Dep2"" /></node><node id=""Bug6619Dep"" x="""" y="""" broken=""false""></node><node id=""Bug6619Dep2"" x="""" y="""" broken=""false""></node></graph>";

            string actual = TestHelper.PostDataToWebserver(postData);

            StringAssert.Contains(actual, expected);
        }
    }
}
