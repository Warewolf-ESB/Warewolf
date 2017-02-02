using System;
using System.Linq;
using System.Xml.Linq;
using Dev2.Common;
using Dev2.Runtime.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Tests.Runtime.JSON;
using Dev2.Tests.Runtime.XML;
using Microsoft.VisualStudio.TestTools.UnitTesting;
// ReSharper disable InconsistentNaming
// ReSharper disable PossibleNullReferenceException

namespace Dev2.Tests.Runtime.ServiceModel
{
    [TestClass]
    public class ComPluginServicesTest
    {

        #region CTOR

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ComPluginServicesContructorWithNullResourceCatalogExpectedThrowsArgumentNullException()
        {
            new ComPluginServices(null, null);
        }

        #endregion

        #region DeserializeService

        private class ComPluginServicesMock : ComPluginServices
        {
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
        public void ComPluginServicesDeserializeServiceWithNullJsonExpectedThrowsArgumentNullException()
        {
            var services = new ComPluginServicesMock();
            services.DeserializeService(null);
        }

        [TestMethod]
        public void ComPluginServicesDeserializeServiceWithInvalidJsonExpectedReturnsNewPluginService()
        {
            var services = new ComPluginServicesMock();
            var result = services.DeserializeService("{'root' : 'hello' }");
            Assert.AreEqual(result.ResourceID, Guid.Empty);
        }

        [TestMethod]
        public void ComPluginServicesDeserializeServiceWithValidJsonExpectedReturnsPluginService()
        {
            var xml = XmlResource.Fetch("ComPluginService");
            var service = new ComPluginService(xml);

            var services = new ComPluginServicesMock();
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
        public void ComPluginServicesDeserializeServiceWithNullXmlExpectedReturnsNewPluginService()
        {
            var services = new ComPluginServicesMock();
            var result = (ComPluginService)services.DeserializeService(null, "ComPluginService");

            Assert.AreEqual(result.ResourceID, Guid.Empty);
        }

        [TestMethod]
        public void ComPluginServicesDeserializeServiceWithValidXmlExpectedReturnsPluginService()
        {
            var xml = XmlResource.Fetch("ComPluginService");

            var services = new ComPluginServicesMock();
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
        public void ComPluginServicesNamespacesWithNullArgsExpectedReturnsEmptyList()
        {
            var services = new ComPluginServices();
            var result = services.Namespaces(null, Guid.Empty, Guid.Empty);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void ComPluginServicesNamespacesWithInvalidArgsExpectedReturnsEmptyList()
        {
            var services = new ComPluginServices();
            var result = services.Namespaces(new ComPluginSource(), Guid.Empty, Guid.Empty);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
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
        public void ComPluginServicesMethodsWithNullArgsExpectedReturnsEmptyList()
        {
            var services = new ComPluginServices();
            var result = services.Methods(null, Guid.Empty, Guid.Empty);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void ComPluginServicesMethodsWithInvalidArgsExpectedReturnsEmptyList()
        {
            var services = new ComPluginServices();
            var result = services.Methods(new ComPluginService(), Guid.Empty, Guid.Empty);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void ComPluginServicesMethodsWithValidArgsExpectedReturnsList()
        {
            var service = CreatePluginService();
            var workspaceID = Guid.NewGuid();

            EnvironmentVariables.GetWorkspacePath(workspaceID);

            var services = new ComPluginServices();
            var result = services.Methods(service, workspaceID, Guid.Empty);

            Assert.AreEqual(55, result.Count);
        }

        #endregion

        #region Test
       

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("ComPluginServices_Test")]
        public void ComPluginServices_Test_WhenTestingPluginReturningJsonString_ExpectValidPaths()
        {
            var pluginServices = new ComPluginServices();
            var serviceDef = JsonResource.Fetch("ComPrimitivePluginReturningJsonString");
            //------------Execute Test---------------------------
            try
            {
                string serializedResult;
                pluginServices.Test(serviceDef, out serializedResult);
            }
            catch(Exception e)
            {
                //Calls the execution correctly;
                // ReSharper disable once PossibleNullReferenceException
                Assert.AreEqual("[Microsoft][ODBC Driver Manager] Data source name not found and no default driver specified", e.InnerException.Message);
                
            }
            
          
        }

        [TestMethod]
        public void PluginServicesTestWithNullArgsExpectedReturnsRecordsetWithError()
        {
            //------------Setup for test--------------------------
            var services = new ComPluginServicesMock();
            //------------Execute Test---------------------------
            string serializedResult;
            var result = services.Test(null, out serializedResult);
            //------------Assert Results-------------------------
            Assert.IsTrue(result[0].HasErrors);
        }

        [TestMethod]
        public void PluginServicesTestWithInvalidArgsExpectedReturnsRecordsetWithError()
        {
            //------------Setup for test--------------------------
            var services = new ComPluginServicesMock();
            //------------Execute Test---------------------------
            string serializedResult;
            var result = services.Test("xxx", out serializedResult);
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
        private const string clsId = "00000514-0000-0010-8000-00AA006D2EA4";
        #endregion
    }
}