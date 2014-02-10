using System;
using Dev2.Integration.Tests.Helpers;
using Dev2.Integration.Tests.Test_utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Integration.Tests.Runtime.ServiceModel
{
    [TestClass]
    public class PluginSourceTests
    {
        [TestMethod]
        public void PluginSource_Load_XmlReturnedProperly()
        {
            string webServerAddress = ServerSettings.WebserverURI.Replace("services/", "wwwroot/sources/pluginsource?rid=2f93aa19-d507-4ed0-9b7e-a8b1b07ce12f#");

            var result = TestHelper.GetResponseFromServer(webServerAddress);

            var allKeys = result.Headers.AllKeys;
            const string ContentType = "Content-Type";
            const string ContentDisposition = "Content-Disposition";
            CollectionAssert.Contains(allKeys, ContentType);
            CollectionAssert.DoesNotContain(allKeys, ContentDisposition);

            var contentTypeValue = result.Headers.Get(ContentType);

            Assert.AreNotEqual("http/xml", contentTypeValue);
        }

        [TestMethod]
        public void PluginSource_Save_GetsResponse()
        {
            string webServerAddress = ServerSettings.WebserverURI.Replace("services/", "Service/PluginSources/Save");
            string postData = String.Format("{0}{1}", webServerAddress, "{\"resourceID\":\"2f93aa19-d507-4ed0-9b7e-a8b1b07ce12f\",\"resourceType\":\"PluginSource\",\"resourceName\":\"Anything To Xml Hook Plugin\",\"resourcePath\":\"Conversion\",\"assemblyName\":\"\",\"assemblyLocation\":\"" + ServerCommonDirectory.Plugins + "\\Dev2.AnytingToXmlHook.Plugin.dll\"}");

            string responseData = TestHelper.PostDataToWebserver(postData);

            Assert.IsFalse(string.IsNullOrEmpty(responseData));
        }
    }
}
