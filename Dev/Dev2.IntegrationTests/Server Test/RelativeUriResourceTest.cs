
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
using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests.Dev2.Application.Server.Tests
{
    [TestClass]
    public class RelativeUriResourceTest
    {
        private const string _workflowName = "RelativeUriResourceTest";

        public TestContext TestContext { get; set; }

        [TestMethod]
        public void RelativeUriResource_TestMethod()
        {
            string postData = String.Format("{0}{1}", ServerSettings.WebserverURI, "Integration Test Resources/" + _workflowName);
            string actualResult = TestHelper.PostDataToWebserver(postData);
            int result = actualResult.IndexOf("http://localhost", StringComparison.OrdinalIgnoreCase);
            Assert.AreEqual(result, -1);
        }
    }
}
