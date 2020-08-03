/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Runtime.Interfaces;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Trigger.Queue;
using Warewolf.Triggers;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class SaveTriggersTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SaveTriggers))]
        public void SaveTriggers_GetResourceID_ShouldReturnEmptyGuid()
        {
            //------------Setup for test--------------------------
            var saveTriggers = new SaveTriggers();

            //------------Execute Test---------------------------
            var resId = saveTriggers.GetResourceID(new Dictionary<string, StringBuilder>());
            //------------Assert Results-------------------------
            Assert.AreEqual(Guid.Empty, resId);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SaveTriggers))]
        public void SaveTriggers_GetAuthorizationContextForService_ShouldReturnContext()
        {
            //------------Setup for test--------------------------
            var saveTriggers = new SaveTriggers();

            //------------Execute Test---------------------------
            var resId = saveTriggers.GetAuthorizationContextForService();
            //------------Assert Results-------------------------
            Assert.AreEqual(AuthorizationContext.Contribute, resId);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SaveTriggers))]
        public void SaveTriggers_HandlesType_ExpectName()
        {
            //------------Setup for test--------------------------
            var saveTriggers = new SaveTriggers();


            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.AreEqual("SaveTriggers", saveTriggers.HandlesType());
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SaveTriggers))]
        public void SaveTriggers_Execute_NullValues_ErrorResult()
        {
            //------------Setup for test--------------------------
            var saveTriggers = new SaveTriggers();
            var serializer = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            var jsonResult = saveTriggers.Execute(null, null);
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.HasError);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SaveTriggers))]
        public void SaveTriggers_Execute_ResourceIDNotPresent_ErrorResult()
        {
            //------------Setup for test--------------------------
            var values = new Dictionary<string, StringBuilder> { { "item", new StringBuilder() } };
            var saveTriggers = new SaveTriggers();
            var serializer = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            var jsonResult = saveTriggers.Execute(values, null);
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.HasError);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SaveTriggers))]
        public void SaveTriggers_Execute_ResourceIDNotGuid_ErrorResult()
        {
            //------------Setup for test--------------------------
            var values = new Dictionary<string, StringBuilder> { { "resourceID", new StringBuilder("ABCDE") } };
            var saveTriggers = new SaveTriggers();
            var serializer = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            var jsonResult = saveTriggers.Execute(values, null);
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.HasError);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SaveTriggers))]
        public void SaveTriggers_Execute_TestDefinitionsNotInValues_ErrorResult()
        {
            //------------Setup for test--------------------------
            var values = new Dictionary<string, StringBuilder> { { "resourceID", new StringBuilder(Guid.NewGuid().ToString()) } };
            var saveTriggers = new SaveTriggers();
            var serializer = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            var jsonResult = saveTriggers.Execute(values, null);
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.HasError);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SaveTriggers))]
        public void SaveTriggers_Execute_ItemToDeleteNotListOfServiceTestTO_ErrorResult()
        {
            //------------Setup for test--------------------------
            var values = new Dictionary<string, StringBuilder> { { "resourceID", new StringBuilder(Guid.NewGuid().ToString()) }, { "testDefinitions", new StringBuilder("This is not deserializable to ServerExplorerItem") } };
            var saveTriggers = new SaveTriggers();
            var serializer = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            var jsonResult = saveTriggers.Execute(values, null);
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.HasError);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SaveTriggers))]
        public void SaveTriggers_Execute_GivenNoPath_ShouldReturnNoPathMsg()
        {
            //---------------Set up test pack-------------------
            var serializer = new Dev2JsonSerializer();
            var triggerQueues = new List<ITriggerQueue>
            {
                new TriggerQueue
                {
                    QueueName = "Test Queue"
                }
            };
            var compressedExecuteMessage = new CompressedExecuteMessage();
            compressedExecuteMessage.SetMessage(serializer.Serialize(triggerQueues));
            var values = new Dictionary<string, StringBuilder> { { "resourceID", new StringBuilder(Guid.NewGuid().ToString()) }, { "triggerDefinitions", serializer.SerializeToBuilder(compressedExecuteMessage) } };
            var saveTriggers = new SaveTriggers();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var jsonResult = saveTriggers.Execute(values, null);
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            //---------------Test Result -----------------------
            Assert.IsTrue(result.HasError);
            Assert.AreEqual("resourcePath is missing", result.Message.ToString());
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SaveTriggers))]
        public void SaveTriggers_Execute_GivenResourceDefinition_ShouldReturnResourceDefinitionMsg()
        {
            //---------------Set up test pack-------------------
            var serializer = new Dev2JsonSerializer();
            var triggerQueues = new List<ITriggerQueue>
            {
                new TriggerQueue
                {
                    QueueName = "Test Queue"
                }
            };
            var compressedExecuteMessage = new CompressedExecuteMessage();
            compressedExecuteMessage.SetMessage(serializer.Serialize(triggerQueues));
            var values = new Dictionary<string, StringBuilder> { { "resourceID", new StringBuilder(Guid.NewGuid().ToString()) }, { "resourcePath", "Home".ToStringBuilder() } };
            var saveTriggers = new SaveTriggers();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var jsonResult = saveTriggers.Execute(values, null);
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            //---------------Test Result -----------------------
            Assert.IsTrue(result.HasError);
            Assert.AreEqual("triggerDefinitions is missing", result.Message.ToString());
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SaveTriggers))]
        public void SaveTriggers_Execute_ExpectName()
        {
            //------------Setup for test--------------------------
            var serializer = new Dev2JsonSerializer();
            var inputs = new Dictionary<string, StringBuilder>();
            var resourceId = Guid.NewGuid();
            var saveTriggers = new SaveTriggers();

            var triggerQueues = new List<ITriggerQueue>
            {
                new TriggerQueue
                {
                    QueueName = "Test Queue"
                }
            };
            var emptyTriggerQueues = new List<ITriggerQueue>();
            var mockTriggersCatalog = new Mock<ITriggersCatalog>();
            var mockResourceCatalog = new Mock<IResourceCatalog>();

            var mockResource = new Mock<IResource>();
            mockResource.SetupGet(resource => resource.ResourceID).Returns(resourceId);
            mockResource.Setup(resource => resource.GetResourcePath(It.IsAny<Guid>())).Returns("somePath");
            mockResourceCatalog.Setup(catalog => catalog.GetResource(GlobalConstants.ServerWorkspaceID, resourceId)).Returns(mockResource.Object);
            var ws = new Mock<IWorkspace>();
            var resID = Guid.Empty;
            mockTriggersCatalog.Setup(a => a.SaveTriggers(It.IsAny<Guid>(), It.IsAny<List<ITriggerQueue>>())).Callback((Guid id,List<ITriggerQueue> queues)=>
            {
                resID = id;
                emptyTriggerQueues = queues;
            }).Verifiable();

            inputs.Add("resourceID", new StringBuilder(resourceId.ToString()));
            var compressedExecuteMessage = new CompressedExecuteMessage();
            compressedExecuteMessage.SetMessage(serializer.Serialize(triggerQueues));
            inputs.Add("triggerDefinitions", serializer.SerializeToBuilder(compressedExecuteMessage));
            inputs.Add("resourcePath", "somePath".ToStringBuilder());
            saveTriggers.TriggersCatalog = mockTriggersCatalog.Object;
            saveTriggers.ResourceCatalog = mockResourceCatalog.Object;
            //------------Execute Test---------------------------
            saveTriggers.Execute(inputs, ws.Object);
            //------------Assert Results-------------------------
            mockTriggersCatalog.Verify(a => a.SaveTriggers(It.IsAny<Guid>(), It.IsAny<List<ITriggerQueue>>()));
            Assert.AreEqual(triggerQueues.Count,emptyTriggerQueues.Count);
            Assert.AreEqual(triggerQueues[0].QueueName,emptyTriggerQueues[0].QueueName);
            Assert.AreEqual(resourceId,resID);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SaveTriggers))]
        public void SaveTriggers_Execute_GivenNullResource_ShouldReturnResourceDeletedMsg()
        {
            //------------Setup for test--------------------------
            var serializer = new Dev2JsonSerializer();
            var inputs = new Dictionary<string, StringBuilder>();
            var resourceId = Guid.NewGuid();
            var saveTriggers = new SaveTriggers();

            var triggerQueues = new List<ITriggerQueue>
            {
                new TriggerQueue
                {
                    QueueName = "Test Queue"
                }
            };
            var mockTriggersCatalog = new Mock<ITriggersCatalog>();
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            mockResourceCatalog.Setup(catalog => catalog.GetResource(GlobalConstants.ServerWorkspaceID, resourceId)).Returns(default(IResource));
            var ws = new Mock<IWorkspace>();

            inputs.Add("resourceID", new StringBuilder(resourceId.ToString()));
            var compressedExecuteMessage = new CompressedExecuteMessage();
            compressedExecuteMessage.SetMessage(serializer.Serialize(triggerQueues));
            inputs.Add("triggerDefinitions", serializer.SerializeToBuilder(compressedExecuteMessage));
            inputs.Add("resourcePath", "somePath".ToStringBuilder());
            saveTriggers.TriggersCatalog = mockTriggersCatalog.Object;
            saveTriggers.ResourceCatalog = mockResourceCatalog.Object;
            //------------Execute Test---------------------------
            var stringBuilder = saveTriggers.Execute(inputs, ws.Object);
            //------------Assert Results-------------------------
            var msg = serializer.Deserialize<ExecuteMessage>(stringBuilder);

            Assert.IsFalse(msg.HasError);
            Assert.IsTrue(msg.Message.Contains("Resource somePath has been deleted. No Triggers can be saved for this resource."));
        }


        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SaveTriggers))]
        public void SaveTriggers_Execute_GivenResourceMoved_ShouldReturnResourceMovedMsg()
        {
            //------------Setup for test--------------------------
            var serializer = new Dev2JsonSerializer();
            var inputs = new Dictionary<string, StringBuilder>();
            var resourceId = Guid.NewGuid();
            var saveTriggers = new SaveTriggers();

            var triggerQueues = new List<ITriggerQueue>
            {
                new TriggerQueue
                {
                    QueueName = "Test Queue"
                }
            };
            var mockResource = new Mock<IResource>();
            mockResource.SetupGet(resource => resource.ResourceID).Returns(resourceId);
            mockResource.Setup(resource => resource.GetResourcePath(It.IsAny<Guid>())).Returns("somePath");

            var mockTriggersCatalog = new Mock<ITriggersCatalog>();
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            mockResourceCatalog.Setup(catalog => catalog.GetResource(GlobalConstants.ServerWorkspaceID, resourceId)).Returns(mockResource.Object);
            mockTriggersCatalog.Setup(a => a.SaveTriggers(It.IsAny<Guid>(), It.IsAny<List<ITriggerQueue>>())).Verifiable();
            var ws = new Mock<IWorkspace>();

            inputs.Add("resourceID", new StringBuilder(resourceId.ToString()));
            var compressedExecuteMessage = new CompressedExecuteMessage();
            compressedExecuteMessage.SetMessage(serializer.Serialize(triggerQueues));
            inputs.Add("triggerDefinitions", serializer.SerializeToBuilder(compressedExecuteMessage));
            inputs.Add("resourcePath", "otherPath".ToStringBuilder());
            saveTriggers.TriggersCatalog = mockTriggersCatalog.Object;
            saveTriggers.ResourceCatalog = mockResourceCatalog.Object;
            //------------Execute Test---------------------------
            var stringBuilder = saveTriggers.Execute(inputs, ws.Object);
            //------------Assert Results-------------------------
            var msg = serializer.Deserialize<ExecuteMessage>(stringBuilder);

            Assert.IsFalse(msg.HasError);
            Assert.IsTrue(msg.Message.Contains("Resource otherPath has changed to somePath. Triggers have been saved for this resource."));
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SaveTriggers))]
        public void SaveTriggers_Execute_GivenResourceMoved_ShouldSaveTriggers()
        {
            //------------Setup for test--------------------------
            var serializer = new Dev2JsonSerializer();
            var inputs = new Dictionary<string, StringBuilder>();
            var resourceId = Guid.NewGuid();
            var saveTriggers = new SaveTriggers();

            var triggerQueues = new List<ITriggerQueue>
            {
                new TriggerQueue
                {
                    QueueName = "Test Queue"
                }
            };
            var mockResource = new Mock<IResource>();
            mockResource.SetupGet(resource => resource.ResourceID).Returns(resourceId);
            mockResource.Setup(resource => resource.GetResourcePath(It.IsAny<Guid>())).Returns("somePath");

            var mockTriggersCatalog = new Mock<ITriggersCatalog>();
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            mockResourceCatalog.Setup(catalog => catalog.GetResource(GlobalConstants.ServerWorkspaceID, resourceId)).Returns(mockResource.Object);
            mockTriggersCatalog.Setup(a => a.SaveTriggers(It.IsAny<Guid>(), It.IsAny<List<ITriggerQueue>>())).Verifiable();
            var ws = new Mock<IWorkspace>();

            inputs.Add("resourceID", new StringBuilder(resourceId.ToString()));
            var compressedExecuteMessage = new CompressedExecuteMessage();
            compressedExecuteMessage.SetMessage(serializer.Serialize(triggerQueues));
            inputs.Add("triggerDefinitions", serializer.SerializeToBuilder(compressedExecuteMessage));
            inputs.Add("resourcePath", "otherPath".ToStringBuilder());
            saveTriggers.TriggersCatalog = mockTriggersCatalog.Object;
            saveTriggers.ResourceCatalog = mockResourceCatalog.Object;
            var stringBuilder = saveTriggers.Execute(inputs, ws.Object);
            //---------------Assert Precondition----------------
            var msg = serializer.Deserialize<ExecuteMessage>(stringBuilder);

            Assert.IsFalse(msg.HasError);
            Assert.IsTrue(msg.Message.Contains("Resource otherPath has changed to somePath. Triggers have been saved for this resource."));
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            mockTriggersCatalog.Verify(a => a.SaveTriggers(It.IsAny<Guid>(), It.IsAny<List<ITriggerQueue>>()), Times.Once);
        }
    }
}