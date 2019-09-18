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
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Data;
using Warewolf.Options;
using Warewolf.Triggers;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class SaveTriggerQueueServiceTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SaveTriggerQueueService))]
        public void SaveTriggerQueueService_GetResourceID_ShouldReturnEmptyGuid()
        {
            //------------Setup for test--------------------------
            var saveTriggerQueueService = new SaveTriggerQueueService();
            //------------Execute Test---------------------------
            var resourceId = saveTriggerQueueService.GetResourceID(new Dictionary<string, StringBuilder>());
            //------------Assert Results-------------------------
            Assert.AreEqual(Guid.Empty, resourceId);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SaveTriggerQueueService))]
        public void SaveTriggerQueueService_GetAuthorizationContextForService_ShouldReturnContext()
        {
            //------------Setup for test--------------------------
            var saveTriggerQueueService = new SaveTriggerQueueService();
            //------------Execute Test---------------------------
            var authorizationContext = saveTriggerQueueService.GetAuthorizationContextForService();
            //------------Assert Results-------------------------
            Assert.AreEqual(AuthorizationContext.Contribute, authorizationContext);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SaveTriggerQueueService))]
        public void SaveTriggerQueueService_CreateServiceEntry_ShouldReturnDynamicService()
        {
            //------------Setup for test--------------------------
            var saveTriggerQueueService = new SaveTriggerQueueService();
            //------------Execute Test---------------------------
            var serviceEntry = saveTriggerQueueService.CreateServiceEntry();

            //------------Assert Results-------------------------
            Assert.AreEqual("SaveTriggerQueueService", serviceEntry.Name);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SaveTriggerQueueService))]
        public void SaveTriggerQueueService_HandlesType_ExpectType()
        {
            //------------Setup for test--------------------------
            var saveTriggerQueueService = new SaveTriggerQueueService();
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.AreEqual("SaveTriggerQueueService", saveTriggerQueueService.HandlesType());
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SaveTriggerQueueService))]
        [ExpectedException(typeof(InvalidDataContractException))]
        public void SaveTriggerQueueService_GivenNullArgs_Returns_InvalidDataContractException()
        {
            //------------Setup for test-------------------------
            var saveTriggerQueueService = new SaveTriggerQueueService();
            var workspaceMock = new Mock<IWorkspace>();
            //------------Execute Test---------------------------           
            saveTriggerQueueService.Execute(null, workspaceMock.Object);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(SaveTriggerQueueService))]
        public void SaveTriggerQueueService_Execute()
        {
            var serializer = new Dev2JsonSerializer();
            var source = new TriggerQueueForTest();

            var values = new Dictionary<string, StringBuilder>
            {
                { "TriggerQueue", source.SerializeToJsonStringBuilder() }
            };

            var saveTriggerQueueService = new SaveTriggerQueueService();

            var jsonResult = saveTriggerQueueService.Execute(values, null);
            var result = serializer.Deserialize<ExecuteMessage>(jsonResult);
            Assert.IsFalse(result.HasError);
            var triggerId = Guid.Parse(result.Message.ToString());
            Assert.IsTrue(triggerId != Guid.Empty);
        }
    }

    internal class TriggerQueueForTest : ITriggerQueue
    {
        public Guid Id { get => TriggerId; }
        public Guid TriggerId { get; set; }
        public IResource QueueSource { get; set; }
        public string QueueName { get; set; }
        public string WorkflowName { get; set; }
        public int Concurrency { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public IOption[] Options { get; set; }
        public IResource QueueSink { get; set; }
        public string DeadLetterQueue { get; set; }
        public IOption[] DeadLetterOptions { get; set; }
        public bool MapEntireMessage { get; set; }
        public ICollection<IServiceInputBase> Inputs { get; set; }
        public Guid ResourceId { get; set; }
        public Guid QueueSourceId { get; set; }
        public Guid QueueSinkId { get; set; }
        public string Name { get; set; }

        public bool Equals(ITriggerQueue other) => throw new NotImplementedException();
    }
}
