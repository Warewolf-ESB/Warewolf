/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Net;
using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace Dev2.Integration.Tests.Sql
{
    [TestClass]
    public class WorkflowsTests
    {
        [TestMethod]
        public void RunWorkflowIntegration()
        {
            string reponseData = TestHelper.PostDataToWebserver(string.Format("{0}{1}", "http://localhost:3142/services/", "Acceptance Testing Resources/SampleEmployeesWorkflow?ResultType=Managers"));
            Assert.IsNotNull(reponseData);
        }

        [TestMethod]
        public void Warewolf_Community_HasUsers()
        {
            using (var client = new WebClient())
            {
                client.Credentials = CredentialCache.DefaultNetworkCredentials;
                var request = client.DownloadString("https://warewolf.userecho.com/api/v2/users.json?page=1&limit=1&access_token=vAAI14uAhYGFGzHMc8pxad2H2ktF7ykuh5vHREql");
                var jToken = JToken.Parse(request);
                var isObject = jToken.IsObject();
                Assert.IsTrue(isObject);
            }
        }
    }
}
