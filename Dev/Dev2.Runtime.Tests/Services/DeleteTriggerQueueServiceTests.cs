/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class DeleteTriggerQueueServiceTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DeleteTriggerQueueService))]
        public void DeleteTriggerQueueService_GetResourceID_ShouldReturnEmptyGuid()
        {
            //------------Setup for test--------------------------
            var deleteTriggerQueueService = new DeleteTriggerQueueService();
            //------------Execute Test---------------------------
            var resourceId = deleteTriggerQueueService.GetResourceID(new Dictionary<string, StringBuilder>());
            //------------Assert Results-------------------------
            Assert.AreEqual(Guid.Empty, resourceId);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DeleteTriggerQueueService))]
        public void DeleteTriggerQueueService_GetAuthorizationContextForService_ShouldReturnContext()
        {
            //------------Setup for test--------------------------
            var deleteTriggerQueueService = new DeleteTriggerQueueService();
            //------------Execute Test---------------------------
            var authorizationContext = deleteTriggerQueueService.GetAuthorizationContextForService();
            //------------Assert Results-------------------------
            Assert.AreEqual(AuthorizationContext.Contribute, authorizationContext);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DeleteTriggerQueueService))]
        public void DeleteTriggerQueueService_CreateServiceEntry_ShouldReturnDynamicService()
        {
            //------------Setup for test--------------------------
            var deleteTriggerQueueService = new DeleteTriggerQueueService();
            //------------Execute Test---------------------------
            var serviceEntry = deleteTriggerQueueService.CreateServiceEntry();

            //------------Assert Results-------------------------
            Assert.AreEqual("DeleteTriggerQueueService", serviceEntry.Name);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DeleteTriggerQueueService))]
        public void DeleteTriggerQueueService_HandlesType_ExpectType()
        {
            //------------Setup for test--------------------------
            var deleteTriggerQueueService = new DeleteTriggerQueueService();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.AreEqual("DeleteTriggerQueueService", deleteTriggerQueueService.HandlesType());
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DeleteTriggerQueueService))]
        [ExpectedException(typeof(InvalidDataContractException))]
        public void DeleteTriggerQueueService_GivenNullArgs_Returns_InvalidDataContractException()
        {
            //------------Setup for test-------------------------
            var deleteTriggerQueueService = new DeleteTriggerQueueService();
            var workspaceMock = new Mock<IWorkspace>();
            //------------Execute Test---------------------------           
            deleteTriggerQueueService.Execute(null, workspaceMock.Object);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DeleteTriggerQueueService))]
        public void DeleteTriggerQueueService_Execute()
        {
            var serializer = new Dev2JsonSerializer();
            var source = new TriggerQueueForTest();

            var values = new Dictionary<string, StringBuilder>
            {
                { "TriggerQueue", source.SerializeToJsonStringBuilder() }
            };

            var deleteTriggerQueueService = new SaveTriggerQueueService();

            var jsonResult = deleteTriggerQueueService.Execute(values, null);
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            Assert.IsFalse(result.HasError);
        }
    }
}
