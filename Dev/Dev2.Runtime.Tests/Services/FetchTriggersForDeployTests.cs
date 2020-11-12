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
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Trigger.Queue;
using Warewolf.Triggers;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class FetchTriggersForDeployTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(FetchTriggersForDeploy))]
        public void FetchTriggersForDeploy_GetResourceID_ShouldReturnEmptyGuid()
        {
            //------------Setup for test--------------------------
            var fetchTriggersForDeploy = new FetchTriggersForDeploy();
            //------------Execute Test---------------------------
            var resourceId = fetchTriggersForDeploy.GetResourceID(new Dictionary<string, StringBuilder>());
            //------------Assert Results-------------------------
            Assert.AreEqual(Guid.Empty, resourceId);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(FetchTriggersForDeploy))]
        public void FetchTriggersForDeploy_GetAuthorizationContextForService_ShouldReturnContext()
        {
            //------------Setup for test--------------------------
            var fetchTriggersForDeploy = new FetchTriggersForDeploy();
            //------------Execute Test---------------------------
            var authorizationContext = fetchTriggersForDeploy.GetAuthorizationContextForService();
            //------------Assert Results-------------------------
            Assert.AreEqual(AuthorizationContext.Any, authorizationContext);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(FetchTriggersForDeploy))]
        public void FetchTriggersForDeploy_HandlesType_ExpectName()
        {
            //------------Setup for test--------------------------
            var fetchTriggersForDeploy = new FetchTriggersForDeploy();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.AreEqual(nameof(FetchTriggersForDeploy), fetchTriggersForDeploy.HandlesType());
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(FetchTriggersForDeploy))]
        public void FetchTriggersForDeploy_Execute_NullValues_ErrorResult()
        {
            //------------Setup for test--------------------------
            var fetchTriggersForDeploy = new FetchTriggersForDeploy();
            var serializer = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            var jsonResult = fetchTriggersForDeploy.Execute(null, null);
            var result = serializer.Deserialize<CompressedExecuteMessage>(jsonResult);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.HasError);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(FetchTriggersForDeploy))]
        public void FetchTriggersForDeploy_Execute_ResourceIDNotPresent_ErrorResult()
        {
            //------------Setup for test--------------------------
            var values = new Dictionary<string, StringBuilder> { { "item", new StringBuilder() } };
            var fetchTriggersForDeploy = new FetchTriggersForDeploy();
            var serializer = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            var jsonResult = fetchTriggersForDeploy.Execute(values, null);
            var result = serializer.Deserialize<CompressedExecuteMessage>(jsonResult);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.HasError);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(FetchTriggersForDeploy))]
        public void FetchTriggersForDeploy_Execute_ResourceIDNotGuid_ErrorResult()
        {
            //------------Setup for test--------------------------
            var values = new Dictionary<string, StringBuilder> { { "resourceID", new StringBuilder("ABCDE") } };
            var fetchTriggersForDeploy = new FetchTriggersForDeploy();
            var serializer = new Dev2JsonSerializer();
            //------------Execute Test---------------------------
            var jsonResult = fetchTriggersForDeploy.Execute(values, null);
            var result = serializer.Deserialize<CompressedExecuteMessage>(jsonResult);
            //------------Assert Results-------------------------
            Assert.IsTrue(result.HasError);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(FetchTriggersForDeploy))]
        public void FetchTriggersForDeploy_Execute_ExpectTriggerQueuesList()
        {
            //------------Setup for test--------------------------
            var fetchTriggersForDeploy = new FetchTriggersForDeploy();

            var listOfTriggerQueues = new List<ITriggerQueue>
            {
                new TriggerQueue
                {
                    ResourceId = Guid.NewGuid(),
                    QueueName = "Test Queue",
                }
            };
            var mockTriggersCatalog = new Mock<ITriggersCatalog>();
            var ws = new Mock<IWorkspace>();
            var resourceId = Guid.Empty;
            mockTriggersCatalog.Setup(a => a.LoadQueuesByResourceId(It.IsAny<Guid>())).Callback((Guid id)=>
            {
                resourceId = id;
            }).Returns(listOfTriggerQueues).Verifiable();

            var serializer = new Dev2JsonSerializer();
            var inputs = new Dictionary<string, StringBuilder>();
            var resourceID = Guid.NewGuid();
            inputs.Add("resourceID", new StringBuilder(resourceID.ToString()));
            fetchTriggersForDeploy.TriggersCatalog = mockTriggersCatalog.Object;
            //------------Execute Test---------------------------
            var res = fetchTriggersForDeploy.Execute(inputs, ws.Object);
            var msg = serializer.Deserialize<CompressedExecuteMessage>(res);
            var triggerQueues = serializer.Deserialize<List<ITriggerQueue>>(msg.GetDecompressedMessage());
            //------------Assert Results-------------------------
            mockTriggersCatalog.Verify(a => a.LoadQueuesByResourceId(It.IsAny<Guid>()));
            Assert.AreEqual(listOfTriggerQueues.Count,triggerQueues.Count);
            Assert.AreEqual(listOfTriggerQueues[0].QueueName,triggerQueues[0].QueueName);
            Assert.AreEqual(resourceID,resourceId);
        }
    }
}
