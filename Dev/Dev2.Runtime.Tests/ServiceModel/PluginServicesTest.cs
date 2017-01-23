/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Common;
using Dev2.Runtime.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Tests.Runtime.JSON;
using Dev2.Tests.Runtime.Plugins;
using Dev2.Tests.Runtime.ServiceModel.Data;
using Dev2.Tests.Runtime.XML;
using DummyNamespaceForTest;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Runtime.ServiceModel
{
    // BUG 9500 - 2013.05.31 - TWR - Created
    [TestClass]
    public class PluginServicesTest
    {

        #region CTOR

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PluginServicesContructorWithNullResourceCatalogExpectedThrowsArgumentNullException()
        {
            new PluginServices(null, null);
        }

        #endregion

        #region Helpers

        public string UnpackDLL(string name)
        {
            return DllExtractor.UnloadToFileSystem(name, "Plugins");
        }

        #endregion

        #region DeserializeService

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void PluginServicesDeserializeServiceWithNullJsonExpectedThrowsArgumentNullException()
        {
            var services = new PluginServicesMock();
            services.DeserializeService(null);
        }

        [TestMethod]
        public void PluginServicesDeserializeServiceWithInvalidJsonExpectedReturnsNewPluginService()
        {
            var services = new PluginServicesMock();
            var result = services.DeserializeService("{'root' : 'hello' }");
            Assert.AreEqual(result.ResourceID, Guid.Empty);
        }

        [TestMethod]
        public void PluginServicesDeserializeServiceWithValidJsonExpectedReturnsPluginService()
        {
            var xml = XmlResource.Fetch("PluginService");
            var service = new PluginService(xml);

            var services = new PluginServicesMock();
            var result = services.DeserializeService(service.ToString());

            PluginServiceTests.VerifyEmbeddedPluginService(result as PluginService);
        }

        [TestMethod]
        public void PluginServicesDeserializeServiceWithNullXmlExpectedReturnsNewPluginService()
        {
            var services = new PluginServicesMock();
            var result = services.DeserializeService(null, "PluginService");

            Assert.AreEqual(result.ResourceID, Guid.Empty);
        }

        [TestMethod]
        public void PluginServicesDeserializeServiceWithValidXmlExpectedReturnsPluginService()
        {
            var xml = XmlResource.Fetch("PluginService");

            var services = new PluginServicesMock();
            var result = services.DeserializeService(xml, "PluginService");

            PluginServiceTests.VerifyEmbeddedPluginService(result as PluginService);
        }

        #endregion

        #region Namespaces

        [TestMethod]
        public void PluginServicesNamespacesWithNullArgsExpectedReturnsEmptyList()
        {
            var services = new PluginServices();
            var result = services.Namespaces(null, Guid.Empty, Guid.Empty);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void PluginServicesNamespacesWithInvalidArgsExpectedReturnsEmptyList()
        {
            var services = new PluginServices();
            var result = services.Namespaces(new PluginSource(), Guid.Empty, Guid.Empty);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void PluginServicesNamespacesWithValidArgsExpectedReturnsList()
        {
            var source = CreatePluginSource();
            var args = source.ToString();
            var workspaceID = Guid.NewGuid();

            EnvironmentVariables.GetWorkspacePath(workspaceID);

            var services = new PluginServices();
            var result = services.Namespaces(source, workspaceID, Guid.Empty);

            // DO NOT assert equality on Count as this will 
            // change as new namespaces are added to this assembly!
            Assert.IsTrue(result.Count > 0);
        }

        #endregion

        #region Methods

        [TestMethod]
        public void PluginServicesMethodsWithNullArgsExpectedReturnsEmptyList()
        {
            var services = new PluginServices();
            var result = services.Methods(null, Guid.Empty, Guid.Empty);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void PluginServicesMethodsWithReturnsNullArgsExpectedReturnsEmptyList()
        {
            var services = new PluginServices();
            var result = services.MethodsWithReturns(null, Guid.Empty, Guid.Empty);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void PluginServicesMethodsWithInvalidArgsExpectedReturnsEmptyList()
        {
            var services = new PluginServices();
            var result = services.Methods(new PluginService(), Guid.Empty, Guid.Empty);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void PluginServicesMethodsWithReturnsWithInvalidArgsExpectedReturnsEmptyList()
        {
            var services = new PluginServices();
            var result = services.MethodsWithReturns(new PluginService(), Guid.Empty, Guid.Empty);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void PluginServicesMethodsWithValidArgsExpectedReturnsList()
        {
            var service = CreatePluginService();
            var workspaceID = Guid.NewGuid();

            EnvironmentVariables.GetWorkspacePath(workspaceID);

            var services = new PluginServices();
            var result = services.Methods(service, workspaceID, Guid.Empty);

            Assert.AreEqual(9, result.Count);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void PluginServicesMethodsWithReturnsWithValidArgsExpectedReturnsList()
        {
            var service = CreatePluginService();
            var workspaceID = Guid.NewGuid();

            EnvironmentVariables.GetWorkspacePath(workspaceID);

            var services = new PluginServices();
            var result = services.MethodsWithReturns(service, workspaceID, Guid.Empty);

            Assert.AreEqual(7, result.Count);
        }

        #endregion

        #region Constuctors

        [TestMethod]
        public void PluginServicesConstuctorsWithNullArgsExpectedReturnsEmptyList()
        {
            var services = new PluginServices();
            var result = services.Constructors(null, Guid.Empty, Guid.Empty);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void PluginServicesConstuctorsWithInvalidArgsExpectedReturnsEmptyList()
        {
            var services = new PluginServices();
            var result = services.Constructors(new PluginService(), Guid.Empty, Guid.Empty);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void PluginServicesConstructorsWithValidArgsExpectedReturnsList()
        {
            var service = CreatePluginService();
            var workspaceID = Guid.NewGuid();

            EnvironmentVariables.GetWorkspacePath(workspaceID);

            var services = new PluginServices();
            var result = services.Constructors(service, workspaceID, Guid.Empty);

            Assert.AreEqual(2, result.Count);
        }

        #endregion

        #region Test

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("PluginServices_Test")]
        public void PluginServices_Test_WhenTestingPluginReturningBool_ExpectValidPaths()
        {
            //------------Setup for test--------------------------
            var path = UnpackDLL("PrimativesTestDLL");

            if(string.IsNullOrEmpty(path))
            {
                Assert.Fail("Failed to unpack required DLL [ PrimativesTestDLL ] ");
            }

            var pluginServices = new PluginServices();
            var serviceDef = JsonResource.Fetch("PrimitivePluginReturningBool");

            //------------Execute Test---------------------------
            string serializedResult;
            var result = pluginServices.Test(serviceDef, out serializedResult);
            ////------------Assert Results-------------------------
            Assert.AreEqual(1, result[0].Fields.Count);
            StringAssert.Contains(result[0].Fields[0].Alias, "PrimitiveReturnValue");
            StringAssert.Contains(result[0].Fields[0].Name, "PrimitiveReturnValue");
            StringAssert.Contains(result[0].Fields[0].Path.ActualPath, "PrimitiveReturnValue");
            StringAssert.Contains(result[0].Fields[0].Path.SampleData, "False");
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("PluginServices_Test")]
        public void PluginServices_Test_WhenTestingPluginReturningDouble_ExpectValidPaths()
        {
            //------------Setup for test--------------------------
            var path = UnpackDLL("PrimativesTestDLL");

            if(string.IsNullOrEmpty(path))
            {
                Assert.Fail("Failed to unpack required DLL [ PrimativesTestDLL ] ");
            }

            var pluginServices = new PluginServices();
            var serviceDef = JsonResource.Fetch("PrimitivePluginReturningDouble");

            //------------Execute Test---------------------------
            string serializedResult;
            var result = pluginServices.Test(serviceDef, out serializedResult);
            ////------------Assert Results-------------------------
            Assert.AreEqual(1, result[0].Fields.Count);
            StringAssert.Contains(result[0].Fields[0].Alias, "PrimitiveReturnValue");
            StringAssert.Contains(result[0].Fields[0].Name, "PrimitiveReturnValue");
            StringAssert.Contains(result[0].Fields[0].Path.ActualPath, "PrimitiveReturnValue");
            StringAssert.Contains(result[0].Fields[0].Path.SampleData, "3.1");
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("PluginServices_Test")]
        public void PluginServices_Test_WhenTestingPluginReturningPlainString_ExpectValidPaths()
        {
            //------------Setup for test--------------------------
            var path = UnpackDLL("PrimativesTestDLL");

            if(string.IsNullOrEmpty(path))
            {
                Assert.Fail("Failed to unpack required DLL [ PrimativesTestDLL ] ");
            }

            var pluginServices = new PluginServices();
            var serviceDef = JsonResource.Fetch("PrimitivePluginReturningPlainString");

            //------------Execute Test---------------------------
            string serializedResult;
            var result = pluginServices.Test(serviceDef, out serializedResult);
            ////------------Assert Results-------------------------
            Assert.AreEqual(1, result[0].Fields.Count);
            StringAssert.Contains(result[0].Fields[0].Alias, "PrimitiveReturnValue");
            StringAssert.Contains(result[0].Fields[0].Name, "PrimitiveReturnValue");
            StringAssert.Contains(result[0].Fields[0].Path.ActualPath, "PrimitiveReturnValue");
            StringAssert.Contains(result[0].Fields[0].Path.SampleData, "Hello__COMMA__ bob");
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("PluginServices_Test")]
        public void PluginServices_Test_WhenTestingPluginReturningXmlString_ExpectValidPaths()
        {
            //------------Setup for test--------------------------
            var path = UnpackDLL("PrimativesTestDLL");

            if(string.IsNullOrEmpty(path))
            {
                Assert.Fail("Failed to unpack required DLL [ PrimativesTestDLL ] ");
            }

            var pluginServices = new PluginServices();
            var serviceDef = JsonResource.Fetch("PrimitivePluginReturningXmlString");

            //------------Execute Test---------------------------
            string serializedResult;
            var result = pluginServices.Test(serviceDef, out serializedResult);
            ////------------Assert Results-------------------------
            Assert.AreEqual(1, result[0].Fields.Count);
            StringAssert.Contains(result[0].Fields[0].Alias, "Message");
            StringAssert.Contains(result[0].Fields[0].Name, "Message");
            StringAssert.Contains(result[0].Fields[0].Path.ActualPath, "Message");
            StringAssert.Contains(result[0].Fields[0].Path.SampleData, "Howdy__COMMA__");
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("PluginServices_Test")]
        public void PluginServices_Test_WhenTestingPluginReturningJsonString_ExpectValidPaths()
        {
            //------------Setup for test--------------------------
            var path = UnpackDLL("PrimativesTestDLL");

            if(string.IsNullOrEmpty(path))
            {
                Assert.Fail("Failed to unpack required DLL [ PrimativesTestDLL ] ");
            }

            var pluginServices = new PluginServices();
            var serviceDef = JsonResource.Fetch("PrimitivePluginReturningJsonString");

            //------------Execute Test---------------------------
            string serializedResult;
            var result = pluginServices.Test(serviceDef, out serializedResult);
            ////------------Assert Results-------------------------
            Assert.AreEqual(1, result[0].Fields.Count);
            StringAssert.Contains(result[0].Fields[0].Alias, "message");
            StringAssert.Contains(result[0].Fields[0].Name, "message");
            StringAssert.Contains(result[0].Fields[0].Path.ActualPath, "message");
            StringAssert.Contains(result[0].Fields[0].Path.SampleData, "Howzit__COMMA__");
        }

        [TestMethod]
        public void PluginServicesTestWithNullArgsExpectedReturnsRecordsetWithError()
        {
            //------------Setup for test--------------------------
            var services = new PluginServicesMock();
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
            var services = new PluginServicesMock();
            //------------Execute Test---------------------------
            string serializedResult;
            var result = services.Test("xxx", out serializedResult);
            //------------Assert Results-------------------------
            Assert.IsTrue(result[0].HasErrors);
        }
        
        #endregion

        #region CreatePluginService

        public static PluginService CreatePluginService()
        {
            return CreatePluginService(new ServiceMethod
            {
                Name = "DummyMethod"
            });
        }

        public static PluginService CreatePluginService(ServiceMethod method)
        {
            var type = typeof(DummyClassForPluginTest);

            var source = CreatePluginSource();
            var service = new PluginService
            {
                ResourceID = Guid.NewGuid(),
                ResourceName = "DummyPluginService",
                ResourceType = "PluginService",
                Namespace = type.FullName,
                Method = method,
                Source = source
            };
            return service;
        }

        #endregion

        #region CreatePluginSource

        static PluginSource CreatePluginSource()
        {
            var type = typeof(DummyClassForPluginTest);
            var assembly = type.Assembly;
            return new PluginSource
            {
                AssemblyLocation = assembly.Location,
                ResourceID = Guid.NewGuid(),
                ResourceName = "Dummy",
                ResourceType = "PluginSource"
            };
        }

        #endregion
    }
}
