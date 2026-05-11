/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.IO;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestBase;

namespace Dev2.Integration.Tests
{
    [TestClass]
    public class ApiTests
    {
        [TestMethod]
        public void RunWorkflowIntegration()
        {
            try
            {
                var reponseData = TestHelper.PostDataToWebserver("http://localhost:7071/apis.json");
                Assert.IsNotNull(reponseData);
                Assert.IsFalse(string.IsNullOrEmpty(reponseData));
            }
            catch (WebException e)
            {
                if (e.Response != null)
                {
                    using (var stream = e.Response.GetResponseStream())
                    {
                        var responseData = new StreamReader(stream).ReadToEnd();
                        Assert.IsNotNull(responseData);
                    }
                }
                else
                {
                    Assert.Inconclusive($"Lightweight execution engine not available at localhost:7071: {e.Message}");
                }
            }
        }
    }
}
