using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common.Interfaces.Enums;
using Dev2.Runtime.ESB.Management.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Data;
using Dev2.Communication;
using Castle.Core.Resource;
using Dev2.Common;
using Dev2.Runtime.Interfaces;
using Dev2.Workspaces;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.DB;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class SavePluginServiceTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("GetResourceID")]
        public void GetResourceID_ShouldReturnEmptyGuid()
        {
            //------------Setup for test--------------------------
            var SavePluginService = new SavePluginService();

            //------------Execute Test---------------------------
            var resId = SavePluginService.GetResourceID(new Dictionary<string, StringBuilder>());
            //------------Assert Results-------------------------
            Assert.AreEqual(Guid.Empty, resId);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("GetResourceID")]
        public void GetAuthorizationContextForService_ShouldReturnContext()
        {
            //------------Setup for test--------------------------
            var SavePluginService = new SavePluginService();

            //------------Execute Test---------------------------
            var resId = SavePluginService.GetAuthorizationContextForService();
            //------------Assert Results-------------------------
            Assert.AreEqual(AuthorizationContext.Contribute, resId);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("SavePluginService_HandlesType")]
        public void SavePluginService_HandlesType_ExpectName()
        {
            //------------Setup for test--------------------------
            var SavePluginService = new SavePluginService();


            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.AreEqual("SavePluginService", SavePluginService.HandlesType());
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("SavePluginService_Execute")]
        public void SavePluginService_Execute_NullValues_ErrorResult()
        {
            //------------Setup for test--------------------------
            var SavePluginService = new SavePluginService();
            var serializer = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            var jsonResult = SavePluginService.Execute(null, null);
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.HasError);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("SavePluginService_Execute")]
        public void SavePluginService_Execute_ResourceIDNotPresent_ErrorResult()
        {
            //------------Setup for test--------------------------
            var values = new Dictionary<string, StringBuilder> { { "item", new StringBuilder() } };
            var SavePluginService = new SavePluginService();
            var serializer = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            var jsonResult = SavePluginService.Execute(values, null);
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.HasError);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("SavePluginService_Execute")]
        public void SavePluginService_Execute_ResourceIDNotGuid_ErrorResult()
        {
            //------------Setup for test--------------------------
            var values = new Dictionary<string, StringBuilder> { { "resourceID", new StringBuilder("ABCDE") } };
            var SavePluginService = new SavePluginService();
            var serializer = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            var jsonResult = SavePluginService.Execute(values, null);
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.HasError);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("SavePluginService_Execute")]
        public void SavePluginService_Execute_TestDefinitionsNotInValues_ErrorResult()
        {
            //------------Setup for test--------------------------
            var values = new Dictionary<string, StringBuilder> { { "resourceID", new StringBuilder(Guid.NewGuid().ToString()) } };
            var SavePluginService = new SavePluginService();
            var serializer = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            var jsonResult = SavePluginService.Execute(values, null);
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.HasError);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("SavePluginService_Execute")]
        public void SavePluginService_Execute_ItemToDeleteNotListOfServiceTestTO_ErrorResult()
        {
            //------------Setup for test--------------------------
            var values = new Dictionary<string, StringBuilder> { { "resourceID", new StringBuilder(Guid.NewGuid().ToString()) }, { "testDefinitions", new StringBuilder("This is not deserializable to ServerExplorerItem") } };
            var SavePluginService = new SavePluginService();
            var serializer = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            var jsonResult = SavePluginService.Execute(values, null);
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.HasError);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Execute_GivenNoSource_ShouldReturnNoSourceMsg()
        {
            //---------------Set up test pack-------------------
            var serializer = new Dev2JsonSerializer();
            var listOfTests = new List<ServiceTestModelTO>
            {
                new ServiceTestModelTO
                {
                    AuthenticationType = AuthenticationType.Public,
                    Enabled = true,
                    TestName = "Test MyWF"
                }
            };
            var serviceDef = new PluginServiceDefinition
            {
                Source = new PluginSourceDefinition(),
                OutputMappings = new List<IServiceOutputMapping>(),
                Constructor = new PluginConstructor
                {
                    Inputs = new List<IConstructorParameter>()
                },
                Action = new PluginAction()
            };
            var values = new Dictionary<string, StringBuilder> { { "PluginService", serializer.SerializeToBuilder(serviceDef) } };
            var SavePluginService = new SavePluginService();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var jsonResult = SavePluginService.Execute(values, null);
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            //---------------Test Result -----------------------
            Assert.IsTrue(result.HasError);
            Assert.AreEqual("Value cannot be null.\r\nParameter name: source", result.Message.ToString());
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Execute_GivenNullSource_ShouldReturnNullSourceMsg()
        {
            //------------Setup for test--------------------------
            var serializer = new Dev2JsonSerializer();
            var inputs = new Dictionary<string, StringBuilder>();
            var resourceID = Guid.NewGuid();
            var SavePluginService = new SavePluginService();
            
            var resourceCatalog = new Mock<IResourceCatalog>();
            resourceCatalog.Setup(catalog => catalog.GetResource(GlobalConstants.ServerWorkspaceID, resourceID)).Returns(default(Dev2.Common.Interfaces.Data.IResource));
            var ws = new Mock<IWorkspace>();

            var serviceDef = new PluginServiceDefinition
            {
                Source = new PluginSourceDefinition(),
                OutputMappings = new List<IServiceOutputMapping>(),
                Constructor = new PluginConstructor
                {
                    Inputs = new List<IConstructorParameter>()
                },
                Action = new PluginAction()
            };
            inputs.Add("PluginService", serializer.SerializeToBuilder(serviceDef));
            SavePluginService.ResourceCatalogue = resourceCatalog.Object;
            //------------Execute Test---------------------------
            var stringBuilder = SavePluginService.Execute(inputs, ws.Object);
            //------------Assert Results-------------------------
            var msg = serializer.Deserialize<ExecuteMessage>(stringBuilder);
            Assert.AreEqual("Value cannot be null.\r\nParameter name: source", msg.Message.ToString());
        } 
    }
}