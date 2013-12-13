using System;
using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests.Dev2.Application.Server.Tests.InternalServices {
    /// <summary>
    /// Summary description for FindDirectoryServiceTest
    /// </summary>
    [TestClass]
    [Ignore] // StringBuilder Refactor?!
    public class FindDirectoryServiceTest {
        private readonly string WebserverUrl = TestResource.WebserverURI_Local;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void FindDirectoryService_ValidPath() {
            string PostData = String.Format("{0}{1}", WebserverUrl, @"FindDirectoryService?DirectoryPath=C:\");
            string expected = "\"title\":\"Windows\", \"isFolder\": true";

            string responseData = TestHelper.PostDataToWebserver(PostData);
            Assert.IsTrue(responseData.Contains(expected), "Could not locate Directory Data");
        }

        [TestMethod]
        public void FindDirectoryService_ValidCredentials()
        {
            string PostData = String.Format("{0}{1}", WebserverUrl, @"FindDirectoryService?DirectoryPath=c:\&Domain=DEV2&Username=" + TestResource.PathOperations_Correct_Username + "&Password=" + TestResource.PathOperations_Correct_Password);
            string expected = "\"title\":\"Windows\", \"isFolder\": true";

            string responseData = TestHelper.PostDataToWebserver(PostData);
            StringAssert.Contains(responseData, expected);
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
