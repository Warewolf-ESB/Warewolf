using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Integration.Tests.Helpers;

namespace Dev2.Integration.Tests.Dev2.Application.Server.Tests.InternalServices {
    /// <summary>
    /// Summary description for FindDriveServiceTest
    /// </summary>
    [TestClass]
    public class FindDriveServiceTest {
        public FindDriveServiceTest() {
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
        public void FindDriveService_NoParameters() {
            string PostData = String.Format("{0}{1}", WebserverUrl, @"FindDriveService");
            string expected = @"[{""driveLetter"":""C:/""";

            string responseData = TestHelper.PostDataToWebserver(PostData);
            Assert.IsTrue(responseData.Contains(expected));
        }
        [TestMethod]
        public void FindDriveService_ValidCredentials()
        {
            string PostData = String.Format("{0}{1}", WebserverUrl, @"FindDriveService?Domain=DEV2&Username=" + TestResource.PathOperations_Correct_Username + "&Password=" + TestResource.PathOperations_Correct_Password);
            string expected = @"[{""driveLetter"":""C:/""";

            string responseData = TestHelper.PostDataToWebserver(PostData);
            Assert.IsTrue(responseData.Contains(expected));
        }
        [TestMethod]
        public void FindDriveService_InvalidCredentials() {
            string PostData = String.Format("{0}{1}", WebserverUrl, @"FindDriveService?Domain=DEV2&Username=john.doe&Password=P@ssword");
            string expected = @"<result>Logon failure: unknown user name or bad password</result>";

            string responseData = TestHelper.PostDataToWebserver(PostData);
            StringAssert.Contains(responseData, expected);
        }
    }
}
