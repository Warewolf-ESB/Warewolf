using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml.Linq;
using Dev2.Integration.Tests.Helpers;

namespace Dev2.Integration.Tests.Dev2.Application.Server.Tests.InternalServices {
    /// <summary>
    /// Summary description for FindDirectoryServiceTest
    /// </summary>
    [TestClass]
    public class FindDirectoryServiceTest {
        public FindDirectoryServiceTest() {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;
        private string WebserverUrl = TestResource.WebserverURI_Local;
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
        public void FindDirectoryService_ValidPath() {
            string PostData = String.Format("{0}{1}", WebserverUrl, @"FindDirectoryService?DirectoryPath=C:\");
            string expected = "{\"title\":\"$RECYCLE.BIN\"";

            string responseData = TestHelper.PostDataToWebserver(PostData);
            Assert.IsTrue(responseData.Contains(expected), "Could not locate Directory Data");
        }

        [TestMethod]
        public void FindDirectoryService_ValidCredentials()
        {
            string PostData = String.Format("{0}{1}", WebserverUrl, @"FindDirectoryService?DirectoryPath=c:\&Domain=DEV2&Username=" + TestResource.PathOperations_Correct_Username + "&Password=" + TestResource.PathOperations_Correct_Password);
            string expected = "\"title\":\"Documents and Settings\", \"isFolder\": true";

            string responseData = TestHelper.PostDataToWebserver(PostData);
            Assert.IsTrue(responseData.Contains(expected), "Could not locate Directory Data");
        }

        [TestMethod]
        public void FindDirectoryService_InvalidCredentials() {
            string PostData = String.Format("{0}{1}", WebserverUrl, @"FindDirectoryService?DirectoryPath=c:\BPM&Domain=DEV2&Username=john.doe&Password=P@ssword");
            string expected = @"<Result>Login Failure : Unknown username or password</Result>";

            string responseData = TestHelper.PostDataToWebserver(PostData);
            StringAssert.Contains(responseData, expected);
        }
    }
}
