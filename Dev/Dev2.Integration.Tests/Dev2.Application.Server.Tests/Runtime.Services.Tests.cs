using System;
using Dev2.Common.ServiceModel;
using Dev2.Integration.Tests.Helpers;
using Dev2.Runtime.Diagnostics;
using Dev2.Runtime.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Dev2.Integration.Tests.Dev2.Application.Server.Tests
{
    [TestClass]
    public class ConnectionsTests
    {
        [TestMethod]
        public void ConnectionsTest_ValidServer_Expected_PositiveValidationResult()
        {
            //Create Connection
            Connection conn = SetupDefaultConnection();
            Connections connections = new Connections();

            //Attemp to test the connection
            ValidationResult validationResult = connections.Test(conn.ToString(), Guid.Empty, Guid.Empty);

            Assert.IsTrue(validationResult.IsValid);

        }

        [TestMethod]
        public void ConnectionsTest_InvalidServer_Expected_NegativeValidationResult()
        {
            //Create Connection
            Connection conn = SetupDefaultConnection();
            // Invalidate connection
            conn.Address = "http://someserverImadeup:77/dsf";
            Connections connections = new Connections();
            // Attempt to test the connection
            ValidationResult validationResult = connections.Test(conn.ToString(), Guid.Empty, Guid.Empty);
            Assert.IsFalse(validationResult.IsValid);

        }

        #region Plugin Sources

        [TestMethod]
        public void LoadExpectedXmlReturnedProperly()
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
        public void SaveExpectsResponse()
        {
            string webServerAddress = ServerSettings.WebserverURI.Replace("services/", "Service/PluginSources/Save?");
            string postData = String.Format("{0}{1}", webServerAddress, "{\"resourceID\":\"2f93aa19-d507-4ed0-9b7e-a8b1b07ce12f\",\"resourceType\":\"PluginSource\",\"resourceName\":\"Anything To Xml Hook Plugin\",\"resourcePath\":\"Conversion\",\"assemblyName\":\"\",\"assemblyLocation\":\"" + ServerCommonDirectory.Plugins + "\\Dev2.AnytingToXmlHook.Plugin.dll\"}");

            string responseData = TestHelper.PostDataToWebserver(postData);

            Assert.Inconclusive("There is no precedent for any Service Model tests, they are covered by unit tests");
            //Assert.IsFalse(string.IsNullOrEmpty(responseData));
        } 

        #endregion

        #region Private Test Methods

        private Connection SetupDefaultConnection()
        {
            var testConnection = new Connection
            {
                Address = "http://localhost:77/dsf",
                AuthenticationType = AuthenticationType.Windows,
                Password = "secret",
                ResourceID = Guid.NewGuid(),
                ResourceName = "TestResourceIMadeUp",
                ResourcePath = @"host\Server",
                ResourceType = ResourceType.Server,
                UserName = @"Domain\User",
                WebServerPort = 1234
            };

            return testConnection;
        }

        #endregion Private Test Methods
    }
}
