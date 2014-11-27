
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.DirectoryServices.ActiveDirectory;
using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests.Dev2.Application.Server.Tests.InternalServices
{
    /// <summary>
    /// Summary description for FindDriveServiceTest
    /// </summary>
    [TestClass]
    public class FindDriveServiceTest
    {

        private readonly string WebserverUrl = TestResource.WebserverURI_Local;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void FindDriveService_NoParameters()
        {
            string PostData = String.Format("{0}{1}", WebserverUrl, @"FindDriveService");
            const string expected = @"[{""driveLetter"":""C:/""";

            string responseData = TestHelper.PostDataToWebserver(PostData);
            Assert.IsTrue(responseData.Contains(expected));
        }

        [TestMethod]
        public void FindDriveService_ValidCredentials()
        {
            var inDomain = true;
            try
            {
                Domain.GetComputerDomain();
            }
            catch (ActiveDirectoryObjectNotFoundException)
            {
                inDomain = false;
            }
            string PostData = String.Format("{0}{1}", WebserverUrl, @"FindDriveService?" + (inDomain ? "Domain=DEV2&" : string.Empty) + "Username=" + TestResource.PathOperations_Correct_Username + "&Password=" + TestResource.PathOperations_Correct_Password);
            const string expected = @"[{""driveLetter"":""C:/""";

            string responseData = TestHelper.PostDataToWebserver(PostData);
            Assert.IsTrue(responseData.Contains(expected));
        }

        [TestMethod]
        public void FindDriveService_InvalidCredentials()
        {
            string PostData = String.Format("{0}{1}", WebserverUrl, @"FindDriveService?Domain=DEV2&Username=john.doe&Password=P@ssword");
            const string expected = @"<result>Logon failure: unknown user name or bad password</result>";

            string responseData = TestHelper.PostDataToWebserver(PostData);
            StringAssert.Contains(responseData, expected);
        }
    }
}
