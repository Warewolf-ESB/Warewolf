
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
    /// Summary description for FindDirectoryServiceTest
    /// </summary>
    [TestClass]
    public class FindDirectoryServiceTest
    {
        private readonly string WebserverUrl = TestResource.WebserverURI_Local;

        [TestMethod]
        public void FindDirectoryService_ValidPath()
        {
            string PostData = String.Format("{0}{1}", WebserverUrl, @"FindDirectoryService?DirectoryPath=C:\");
            const string expected = "\"title\":\"Windows\", \"isFolder\": true";

            string responseData = TestHelper.PostDataToWebserver(PostData);
            Assert.IsTrue(responseData.Contains(expected), "Could not locate Directory Data");
        }

        [TestMethod]
        public void FindDirectoryService_ValidCredentials()
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
            string PostData = String.Format("{0}{1}", WebserverUrl, @"FindDirectoryService?DirectoryPath=c:\" + (inDomain ? "&Domain=DEV2" : string.Empty) + "&Username=" + TestResource.PathOperations_Correct_Username + "&Password=" + TestResource.PathOperations_Correct_Password);
            const string expected = "\"title\":\"Windows\", \"isFolder\": true";

            string responseData = TestHelper.PostDataToWebserver(PostData);
            Assert.IsTrue(responseData.Contains(expected), "Could not locate Directory Data");
        }

        [TestMethod]
        public void FindDirectoryService_InvalidCredentials()
        {
            string PostData = String.Format("{0}{1}", WebserverUrl, @"FindDirectoryService?DirectoryPath=c:\BPM&Domain=DEV2&Username=john.doe&Password=P@ssword");
            const string expected = @"<Result>Login Failure : Unknown username or password</Result>";

            string responseData = TestHelper.PostDataToWebserver(PostData);
            StringAssert.Contains(responseData, expected);
        }
    }
}
