/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Runtime.WebServer.Tests
{
    [TestClass]
    public class WebRequestExtentionsTests
    {
        [TestMethod]
        [TestCategory(nameof(WebRequestExtentions))]
        [Owner("Siphamandla Dube")]
        public void WebRequestExtentions_GetPathForAllResources_GIVEN_PathWithSpaces_ShouldEscape()
        {
            var result = WebRequestExtentions.GetPathForAllResources(new TransferObjects.WebRequestTO { WebServerUrl = "http://localhost:3110/secure/Folder%20With%20Spaces/workflowFolder.tests" });

            Assert.AreEqual("Folder With Spaces", result);
        }
    }
}
