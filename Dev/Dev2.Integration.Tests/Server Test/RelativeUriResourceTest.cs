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
            string postData = String.Format("{0}{1}", ServerSettings.WebserverURI, _workflowName);
            string actualResult = TestHelper.PostDataToWebserver(postData);
            int result = actualResult.IndexOf("http://localhost", StringComparison.OrdinalIgnoreCase);
            Assert.AreEqual(result, -1);
        }
    }
}
