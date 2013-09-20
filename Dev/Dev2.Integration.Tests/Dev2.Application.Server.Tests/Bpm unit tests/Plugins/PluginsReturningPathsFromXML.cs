using System;
using System.Xml.Linq;
using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests.Dev2.Application.Server.Tests.Bpm_unit_tests.Plugins
{
    [TestClass][Ignore]//Ashley: One of these tests may be causing the server to hang in a background thread, preventing windows 7 build server from performing any more builds
    public class PluginsReturningPathsFromXML
    {
        // Bug 7820
        [TestMethod]
       
        public void TestPluginsReturningPathsFromXML(){
            string PostData = String.Format("{0}{1}", ServerSettings.WebserverURI, "PluginsReturningPathsFromXML");
            string expected = @"Company().OuterNestedRecordSet().InnerNestedRecordSet:ItemValue";
            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            Assert.IsTrue(ResponseData.IndexOf(expected) >= 0);
        }
    }
}
