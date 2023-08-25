using System;
using System.Linq;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Security;
using Dev2.Tests.Runtime.XML;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;


namespace Dev2.Tests.Runtime.ServiceModel
{
    [TestClass]
    [TestCategory("Runtime Hosting")]
    public class ComPluginServicesTest
    {

        #region CTOR

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        [TestCategory("WarewolfCOMIPCClient_Deprecated")]
        public void ComPluginServicesContructorWithNullResourceCatalogExpectedThrowsArgumentNullException()
        {
            new ComPluginServices(null, null);
        }

        #endregion

        #region DeserializeService

        class ComPluginServicesMock : ComPluginServices
        {
            public ComPluginServicesMock(IResourceCatalog resourceCatalog, IAuthorizationService authorizationService)
                : base(resourceCatalog, authorizationService)
            {
            }

            public new Service DeserializeService(string args)
            {
                return base.DeserializeService(args);
            }
            public new Service DeserializeService(XElement xml, string resourceType)
            {
                return base.DeserializeService(xml, resourceType);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        [TestCategory("WarewolfCOMIPCClient_Deprecated")]
        public void ComPluginServicesDeserializeServiceWithNullJsonExpectedThrowsArgumentNullException()
        {
            var services = new ComPluginServicesMock(new Mock<IResourceCatalog>().Object, new Mock<IAuthorizationService>().Object);
            services.DeserializeService(null);
        }

        [TestMethod]
        [TestCategory("WarewolfCOMIPCClient_Deprecated")]
        public void ComPluginServicesDeserializeServiceWithInvalidJsonExpectedReturnsNewPluginService()
        {
            var services = new ComPluginServicesMock(new Mock<IResourceCatalog>().Object, new Mock<IAuthorizationService>().Object);
            var result = services.DeserializeService("{'root' : 'hello' }");
            Assert.AreEqual(result.ResourceID, Guid.Empty);
        }

        [TestMethod]
        [TestCategory("WarewolfCOMIPCClient_Deprecated")]
        public void ComPluginServicesDeserializeServiceWithValidJsonExpectedReturnsPluginService()
        {
            var xml = XmlResource.Fetch("ComPluginService");
            var service = new ComPluginService(xml);

            var services = new ComPluginServicesMock(new Mock<IResourceCatalog>().Object, new Mock<IAuthorizationService>().Object);
            var result = services.DeserializeService(service.ToString());

            Assert.AreEqual(Guid.Parse("89098b76-ac11-40b2-b3e8-b175314cb3bb"), service.ResourceID);
            Assert.AreEqual("ComPluginService", service.ResourceType);
            Assert.AreEqual(Guid.Parse("00746beb-46c1-48a8-9492-e2d20817fcd5"), service.Source.ResourceID);
            Assert.AreEqual("ComPluginTesterSource", service.Source.ResourceName);
            Assert.AreEqual("Dev2.Terrain.Mountain", service.Namespace);
            Assert.AreEqual("Echo", service.Method.Name);

            Assert.AreEqual("<root>hello</root>", service.Method.Parameters.First(p => p.Name == "text").DefaultValue);

            Assert.AreEqual("reverb", service.Recordsets[0].Fields.First(f => f.Name == "echo").Alias);
        }

        [TestMethod]
        [TestCategory("WarewolfCOMIPCClient_Deprecated")]
        public void ComPluginServicesDeserializeServiceWithNullXmlExpectedReturnsNewPluginService()
        {
            var services = new ComPluginServicesMock(new Mock<IResourceCatalog>().Object, new Mock<IAuthorizationService>().Object);
            var result = (ComPluginService)services.DeserializeService(null, "ComPluginService");

            Assert.AreEqual(result.ResourceID, Guid.Empty);
        }

        [TestMethod]
        [TestCategory("WarewolfCOMIPCClient_Deprecated")]
        public void ComPluginServicesDeserializeServiceWithValidXmlExpectedReturnsPluginService()
        {
            var xml = XmlResource.Fetch("ComPluginService");

            var services = new ComPluginServicesMock(new Mock<IResourceCatalog>().Object, new Mock<IAuthorizationService>().Object);
            var service = (ComPluginService)services.DeserializeService(xml, "ComPluginService");

            Assert.AreEqual(Guid.Parse("89098b76-ac11-40b2-b3e8-b175314cb3bb"), service.ResourceID);
            Assert.AreEqual("ComPluginService", service.ResourceType);
            Assert.AreEqual(Guid.Parse("00746beb-46c1-48a8-9492-e2d20817fcd5"), service.Source.ResourceID);
            Assert.AreEqual("ComPluginTesterSource", service.Source.ResourceName);
            Assert.AreEqual("Dev2.Terrain.Mountain", service.Namespace);
            Assert.AreEqual("Echo", service.Method.Name);

            Assert.AreEqual("<root>hello</root>", service.Method.Parameters.First(p => p.Name == "text").DefaultValue);

            Assert.AreEqual("reverb", service.Recordsets[0].Fields.First(f => f.Name == "echo").Alias);
        }

        #endregion

        #region Namespaces

        [TestMethod]
        [TestCategory("WarewolfCOMIPCClient_Deprecated")]
        public void ComPluginServicesNamespacesWithNullArgsExpectedReturnsEmptyList()
        {
            var services = new ComPluginServices();
            var result = services.Namespaces(null, Guid.Empty, Guid.Empty);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        [TestCategory("WarewolfCOMIPCClient_Deprecated")]
        public void ComPluginServicesNamespacesWithInvalidArgsExpectedReturnsEmptyList()
        {
            var services = new ComPluginServices();
            var result = services.Namespaces(new ComPluginSource(), Guid.Empty, Guid.Empty);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]        
        [TestCategory("WarewolfCOMIPCClient_Deprecated")]
        public void ComPluginServicesNamespacesWithValidArgsExpectedReturnsList()
        {
            var source = CreatePluginSource();
            var args = source.ToString();
            var workspaceID = Guid.NewGuid();

            EnvironmentVariables.GetWorkspacePath(workspaceID);

            var services = new ComPluginServices();
            var result = services.Namespaces(source, workspaceID, Guid.Empty);

            Assert.IsTrue(result.Count > 0);
        }

        #endregion

        #region Methods

        [TestMethod]
        [TestCategory("WarewolfCOMIPCClient_Deprecated")]
        public void ComPluginServicesMethodsWithNullArgsExpectedReturnsEmptyList()
        {
            var services = new ComPluginServices();
            var result = services.Methods(null, Guid.Empty, Guid.Empty);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        [TestCategory("WarewolfCOMIPCClient_Deprecated")]
        public void ComPluginServicesMethodsWithInvalidArgsExpectedReturnsEmptyList()
        {
            var services = new ComPluginServices();
            var result = services.Methods(new ComPluginService(), Guid.Empty, Guid.Empty);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        [TestCategory("WarewolfCOMIPCClient_Deprecated")]
        public void ComPluginServicesMethodsWithValidArgsExpectedReturnsList()
        {
            var service = CreatePluginService();
            var workspaceID = Guid.NewGuid();

            EnvironmentVariables.GetWorkspacePath(workspaceID);

            var services = new ComPluginServices();
            var result = services.Methods(service, workspaceID, Guid.Empty);

            Assert.IsTrue(result.Count > 5, "Not enough items in COM server method list.");
        }

        #endregion

        #region Test
       
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("ComPluginServices_Test")]
        [TestCategory("WarewolfCOMIPCClient_Deprecated")]
        public void ComPluginServices_Test_WhenTestingPluginReturningJsonString_ExpectValidPaths()
        {
            var pluginServices = new ComPluginServices();
            var serviceDef = JsonResource.Fetch("ComPrimitivePluginReturningJsonString");
            //------------Execute Test---------------------------
            try
            {
                pluginServices.Test(serviceDef, out string serializedResult);
            }
            catch(Exception e)
            {
                Assert.AreEqual("[Microsoft][ODBC Driver Manager] Data source name not found and no default driver specified", e.InnerException.Message);
            }
        }

        [TestMethod]
        [TestCategory("WarewolfCOMIPCClient_Deprecated")]
        public void PluginServicesTestWithNullArgsExpectedReturnsRecordsetWithError()
        {
            //------------Setup for test--------------------------
            var services = new ComPluginServicesMock(new Mock<IResourceCatalog>().Object, new Mock<IAuthorizationService>().Object);
            //------------Execute Test---------------------------
            var result = services.Test(null, out string serializedResult);
            //------------Assert Results-------------------------
            Assert.IsTrue(result[0].HasErrors);
        }

        [TestMethod]
        [TestCategory("WarewolfCOMIPCClient_Deprecated")]
        public void PluginServicesTestWithInvalidArgsExpectedReturnsRecordsetWithError()
        {
            //------------Setup for test--------------------------
            var services = new ComPluginServicesMock(new Mock<IResourceCatalog>().Object, new Mock<IAuthorizationService>().Object);
            //------------Execute Test---------------------------
            var result = services.Test("xxx", out string serializedResult);
            //------------Assert Results-------------------------
            Assert.IsTrue(result[0].HasErrors);
        }

        #endregion

        #region CreatePluginService

        public static ComPluginService CreatePluginService()
        {
            return CreatePluginService(new ServiceMethod
            {
                Name = "DummyMethod"
            });
        }

        public static ComPluginService CreatePluginService(ServiceMethod method)
        {

            var source = CreatePluginSource();
            var service = new ComPluginService
            {
                ResourceID = Guid.NewGuid(),
                ResourceName = "DummyPluginService",
                ResourceType = "ComPluginService",
                Namespace = "Namespace",
                Method = method,
                Source = source,
                
            };
            return service;
        }

        #endregion

        #region CreatePluginSource

        static ComPluginSource CreatePluginSource()
        {
            return new ComPluginSource
            {

                ResourceID = Guid.NewGuid(),
                ResourceName = "Dummy",
                ResourceType = "ComPluginSource",
                ClsId = clsId
            };
        }
        const string clsId = "00000514-0000-0010-8000-00AA006D2EA4";
        #endregion
    }
}