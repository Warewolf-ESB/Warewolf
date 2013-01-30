using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Integration.Tests.Helpers;

namespace Dev2.Integration.Tests.Dev2.Application.Server.Tests.WebSeverTests
{
    /// <summary>
    /// Summary description for BasicWebServerTests
    /// </summary>
    [TestClass]
    public class BasicWebServerTests
    {
        public BasicWebServerTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        string WebserverURI = ServerSettings.WebserverURI;

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

        #region Server Asset Tests

        [TestMethod]
        public void IconFileExistsOnServerAndCanBeCalled_Expected_Positive()
        {
            string webServerAddress = WebserverURI.Replace("services/", "icons/");
            string PostData = String.Format("{0}{1}", webServerAddress, "ui-button.png");           

            string ResponseData = TestHelper.PostDataToWebserver(PostData);           

            Assert.IsFalse(string.IsNullOrEmpty(ResponseData));
        }

        #endregion Server Asset Tests

        #region List Service Tests

        /// <summary>
        /// Test to make sure that the icon files are in the correct place
        /// </summary>
        [TestMethod]
        public void ListIcons_Expected_ListOfIconsFromServer() 
        {
            string webServerAddress = WebserverURI.Replace("services/", "list/icons/*");
            string PostData = webServerAddress;

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            Assert.IsFalse(string.IsNullOrEmpty(ResponseData));
        }

        #endregion List Service Tests
    }
}
