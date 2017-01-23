using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Services.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming

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

       /* [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("SavePluginService_Execute")]
        public void SavePluginService_Execute_NullValues_ErrorResult()
        {
            //------------Setup for test--------------------------
            var SavePluginService = new SavePluginService();
            var serializer = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            StringBuilder jsonResult = SavePluginService.Execute(null, null);
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
            StringBuilder jsonResult = SavePluginService.Execute(values, null);
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
            StringBuilder jsonResult = SavePluginService.Execute(values, null);
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
            StringBuilder jsonResult = SavePluginService.Execute(values, null);
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
            StringBuilder jsonResult = SavePluginService.Execute(values, null);
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.HasError);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Execute_GivenNoPath_ShouldReturnNoPathMsg()
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
            var compressedExecuteMessage = new CompressedExecuteMessage();
            compressedExecuteMessage.SetMessage(serializer.Serialize(listOfTests));
            var values = new Dictionary<string, StringBuilder> { { "resourceID", new StringBuilder(Guid.NewGuid().ToString()) }, { "testDefinitions", serializer.SerializeToBuilder(compressedExecuteMessage) } };
            var SavePluginService = new SavePluginService();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            StringBuilder jsonResult = SavePluginService.Execute(values, null);
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            //---------------Test Result -----------------------
            Assert.IsTrue(result.HasError);
            Assert.AreEqual("resourcePath is missing", result.Message.ToString());
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Execute_GivenResourceDefination_ShouldReturnResourceDefinationMsg()
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
            var compressedExecuteMessage = new CompressedExecuteMessage();
            compressedExecuteMessage.SetMessage(serializer.Serialize(listOfTests));
            var values = new Dictionary<string, StringBuilder> { { "resourceID", new StringBuilder(Guid.NewGuid().ToString()) }, { "resourcePath", "Home".ToStringBuilder() } };
            var SavePluginService = new SavePluginService();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            StringBuilder jsonResult = SavePluginService.Execute(values, null);
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            //---------------Test Result -----------------------
            Assert.IsTrue(result.HasError);
            Assert.AreEqual("testDefinition is missing", result.Message.ToString());
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("SavePluginService_Execute")]
        public void SavePluginService_Execute_ExpectName()
        {
            //------------Setup for test--------------------------
            var serializer = new Dev2JsonSerializer();
            var inputs = new Dictionary<string, StringBuilder>();
            var resourceID = Guid.NewGuid();
            var SavePluginService = new SavePluginService();

            var listOfTests = new List<ServiceTestModelTO>
            {
                new ServiceTestModelTO
                {
                    AuthenticationType = AuthenticationType.Public,
                    Enabled = true,
                    TestName = "Test MyWF"
                }
            };
            var testModels = new List<IServiceTestModelTO>();
            var resourceCatalog = new Mock<IResourceCatalog>();
            
            var resourceMock = new Mock<IResource>();
            resourceMock.SetupGet(resource => resource.ResourceID).Returns(resourceID);
            resourceMock.Setup(resource => resource.GetResourcePath(It.IsAny<Guid>())).Returns("Home");
            resourceCatalog.Setup(catalog => catalog.GetResource(GlobalConstants.ServerWorkspaceID, resourceID)).Returns(resourceMock.Object);
            var ws = new Mock<IWorkspace>();
            var resID = Guid.Empty;
          

          
            inputs.Add("resourceID", new StringBuilder(resourceID.ToString()));
            var compressedExecuteMessage = new CompressedExecuteMessage();
            compressedExecuteMessage.SetMessage(serializer.Serialize(listOfTests));
            inputs.Add("testDefinitions", serializer.SerializeToBuilder(compressedExecuteMessage));            
            inputs.Add("resourcePath", "Home".ToStringBuilder());            
            SavePluginService.ResourceCatalogue = resourceCatalog.Object;
            //------------Execute Test---------------------------
            SavePluginService.Execute(inputs, ws.Object);
            //------------Assert Results-------------------------
            Assert.AreEqual(listOfTests.Count,testModels.Count);
            Assert.AreEqual(listOfTests[0].TestName,testModels[0].TestName);
            Assert.AreEqual(resourceID,resID);
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Execute_GivenNullResource_ShouldReturnResourceDeletedMsg()
        {
            //------------Setup for test--------------------------
            var serializer = new Dev2JsonSerializer();
            var inputs = new Dictionary<string, StringBuilder>();
            var resourceID = Guid.NewGuid();
            var SavePluginService = new SavePluginService();

            var listOfTests = new List<ServiceTestModelTO>
            {
                new ServiceTestModelTO
                {
                    AuthenticationType = AuthenticationType.Public,
                    Enabled = true,
                    TestName = "Test MyWF"
                }
            };
            var resourceCatalog = new Mock<IResourceCatalog>();
            resourceCatalog.Setup(catalog => catalog.GetResource(GlobalConstants.ServerWorkspaceID, resourceID)).Returns(default(IResource));
            var ws = new Mock<IWorkspace>();
          
            inputs.Add("resourceID", new StringBuilder(resourceID.ToString()));
            var compressedExecuteMessage = new CompressedExecuteMessage();
            compressedExecuteMessage.SetMessage(serializer.Serialize(listOfTests));
            inputs.Add("testDefinitions", serializer.SerializeToBuilder(compressedExecuteMessage));            
            inputs.Add("resourcePath", "Home".ToStringBuilder());            
            SavePluginService.ResourceCatalogue = resourceCatalog.Object;
            //------------Execute Test---------------------------
            var stringBuilder = SavePluginService.Execute(inputs, ws.Object);
            //------------Assert Results-------------------------
            var msg = serializer.Deserialize<ExecuteMessage>(stringBuilder);
            var testSaveResult = serializer.Deserialize<TestSaveResult>(msg.Message);

            Assert.AreEqual(SaveResult.ResourceDeleted, testSaveResult.Result);
            Assert.AreEqual("Resource Home has been deleted. No Tests can be saved for this resource.", testSaveResult.Message);
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Execute_GivenResourceMoved_ShouldReturnResourceMovedMsg()
        {
            //------------Setup for test--------------------------
            var serializer = new Dev2JsonSerializer();
            var inputs = new Dictionary<string, StringBuilder>();
            var resourceID = Guid.NewGuid();
            var SavePluginService = new SavePluginService();

            var listOfTests = new List<ServiceTestModelTO>
            {
                new ServiceTestModelTO
                {
                    AuthenticationType = AuthenticationType.Public,
                    Enabled = true,
                    TestName = "Test MyWF"
                }
            };
            var resourceMock = new Mock<IResource>();
            resourceMock.SetupGet(resource => resource.ResourceID).Returns(resourceID);
            resourceMock.Setup(resource => resource.GetResourcePath(It.IsAny<Guid>())).Returns("Home");

            var testCatalogMock = new Mock<ITestCatalog>();
            var resourceCatalog = new Mock<IResourceCatalog>();
            resourceCatalog.Setup(catalog => catalog.GetResource(GlobalConstants.ServerWorkspaceID, resourceID)).Returns(resourceMock.Object);
            testCatalogMock.Setup(a => a.SaveTests(It.IsAny<Guid>(), It.IsAny<List<IServiceTestModelTO>>())).Verifiable();
            var ws = new Mock<IWorkspace>();
          
            inputs.Add("resourceID", new StringBuilder(resourceID.ToString()));
            var compressedExecuteMessage = new CompressedExecuteMessage();
            compressedExecuteMessage.SetMessage(serializer.Serialize(listOfTests));
            inputs.Add("testDefinitions", serializer.SerializeToBuilder(compressedExecuteMessage));            
            inputs.Add("resourcePath", "AnathorPath".ToStringBuilder());            
            SavePluginService.ResourceCatalogue = resourceCatalog.Object;
            //------------Execute Test---------------------------
            var stringBuilder = SavePluginService.Execute(inputs, ws.Object);
            //------------Assert Results-------------------------
            var msg = serializer.Deserialize<ExecuteMessage>(stringBuilder);
            var testSaveResult = serializer.Deserialize<TestSaveResult>(msg.Message);

            Assert.AreEqual(SaveResult.ResourceUpdated, testSaveResult.Result);
            Assert.AreEqual("Resource AnathorPath has changed to Home. Tests have been saved for this resource.", testSaveResult.Message);
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Execute_GivenResourceMoved_ShouldSavePluginService()
        {
            //------------Setup for test--------------------------
            var serializer = new Dev2JsonSerializer();
            var inputs = new Dictionary<string, StringBuilder>();
            var resourceID = Guid.NewGuid();
            var SavePluginService = new SavePluginService();

            var listOfTests = new List<ServiceTestModelTO>
            {
                new ServiceTestModelTO
                {
                    AuthenticationType = AuthenticationType.Public,
                    Enabled = true,
                    TestName = "Test MyWF"
                }
            };
            var resourceMock = new Mock<IResource>();
            resourceMock.SetupGet(resource => resource.ResourceID).Returns(resourceID);
            resourceMock.Setup(resource => resource.GetResourcePath(It.IsAny<Guid>())).Returns("Home");

            var resourceCatalog = new Mock<IResourceCatalog>();
            resourceCatalog.Setup(catalog => catalog.GetResource(GlobalConstants.ServerWorkspaceID, resourceID)).Returns(resourceMock.Object);
            var ws = new Mock<IWorkspace>();
          
            inputs.Add("resourceID", new StringBuilder(resourceID.ToString()));
            var compressedExecuteMessage = new CompressedExecuteMessage();
            compressedExecuteMessage.SetMessage(serializer.Serialize(listOfTests));
            inputs.Add("testDefinitions", serializer.SerializeToBuilder(compressedExecuteMessage));            
            inputs.Add("resourcePath", "AnathorPath".ToStringBuilder());            
            SavePluginService.ResourceCatalogue = resourceCatalog.Object;
            var stringBuilder = SavePluginService.Execute(inputs, ws.Object);
            //---------------Assert Precondition----------------
            var msg = serializer.Deserialize<ExecuteMessage>(stringBuilder);
            var testSaveResult = serializer.Deserialize<TestSaveResult>(msg.Message);

            Assert.AreEqual(SaveResult.ResourceUpdated, testSaveResult.Result);
            Assert.AreEqual("Resource AnathorPath has changed to Home. Tests have been saved for this resource.", testSaveResult.Message);
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            resourceCatalog.Verify(a => a.SaveResource(It.IsAny<Guid>(), It.IsAny<List<IServiceTestModelTO>>()), Times.Once);
        }
        */
      
    }
}