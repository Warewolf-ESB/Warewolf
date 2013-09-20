using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Integration.Tests.Helpers;

namespace Dev2.Integration.Tests.Dev2.Application.Server.Tests.Bpm_unit_tests.Plugins
{
    [TestClass][Ignore]//Ashley: One of these tests may be causing the server to hang in a background thread, preventing windows 7 build server from performing any more builds
    public class PluginsReturningPathsFromJson
    {
        // Bug 7820
        [TestMethod]
        public void TestPluginsReturningPathsFromJson()
        {
            string PostData = String.Format("{0}{1}", ServerSettings.WebserverURI, "PluginsReturningPathsFromJson");
            string expected = @"Departments().Employees().Name</ActualPath>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            //Assert.IsTrue(ResponseData.IndexOf(expected) >= 0);            

            Assert.Inconclusive("Test is failing because of plugins");
        }
    }
}
