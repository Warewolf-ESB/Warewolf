using Dev2.Common.Interfaces.Resources;
using Dev2.Controller;
using Dev2.Data.ServiceModel;
using Dev2.Network;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Xml.Linq;

namespace Warewolf.Common.Framework48.Tests
{
    [TestClass]
    public class ResourceCatalogProxyTests
    {
        public readonly static string ServerURI = "http://t004178:3142";

        [TestMethod]
        public void ResourceCatalogProxy_Construct()
        {
            var proxy = GetResourceCatalog();
            Assert.IsNotNull(proxy);
        }

        [TestMethod]
        public void ResourceCatalogProxy_GetResourceById_ReturnsResource()
        {
            var environmentConnection = GetConnection(ServerURI);
            var proxy = new ResourceCatalogProxy(environmentConnection);
            var resourceId = Guid.Parse("5d82c480-505e-48e9-9915-aca0293be30c");
            //var resource = proxy.GetResourceById<IQueueSource>(Guid.Empty, resourceId);
            var resource = proxy.GetResourceById<RabbitMQSource>(Guid.Empty, resourceId);

            Assert.IsTrue(resource.IsSource);
            Assert.AreEqual("test", resource.UserName);
            Assert.AreEqual("test", resource.Password);
        }




        private static ResourceCatalogProxy GetResourceCatalog()
        {
            var environmentConnection = GetConnection(ServerURI);
            var proxy = new ResourceCatalogProxy(environmentConnection);
            return proxy;
        }
        private static ServerProxy GetConnection(string serverUri)
        {
            var applicationServerUri = serverUri;
            var actualWebServerUri = new Uri(applicationServerUri);// applicationServerUri.ToString().ToUpper().Replace("localhost".ToUpper(), Environment.MachineName));
            var environmentConnection = new ServerProxy(actualWebServerUri);

            Console.Write("connecting to server: " + actualWebServerUri + "...");
            environmentConnection.Connect(Guid.Empty);

            Assert.IsTrue(environmentConnection.IsConnected);
            return environmentConnection;
        }
    }
}
