/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.DB;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using Warewolf.Options;

namespace Warewolf.Trigger.Queue.Tests
{
    [TestClass]
    public class TriggerQueueTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(TriggerQueue))]
        public void TriggerQueue_Equals_Other_IsNull_Expect_False()
        {
            var triggerQueue = new TriggerQueue();
            var equals = triggerQueue.Equals(null);
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(TriggerQueue))]
        public void TriggerQueue_ReferenceEquals_Match_Expect_True()
        {
            var triggerQueue = new TriggerQueue { Concurrency = 1 };
            var otherTriggerQueue = triggerQueue;
            var equals = triggerQueue.Equals(otherTriggerQueue);
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(TriggerQueue))]
        public void TriggerQueue_Equals_MisMatch_Expect_False()
        {
            var mockResource = new Mock<IResource>();
            mockResource.Setup(resource => resource.ResourceID).Returns(Guid.NewGuid());

            var mockOption = new Mock<IOption>();
            var mockInputs = new Mock<ICollection<IServiceInput>>();
            var resourceId = Guid.NewGuid();

            var triggerQueue = new TriggerQueue
            {
                QueueSource = mockResource.Object,
                QueueName = "Queue",
                WorkflowName = "Workflow",
                Concurrency = 100,
                UserName = "Bob",
                Password = "123456",
                Options = new IOption[] { mockOption.Object },
                QueueSink = mockResource.Object,
                DeadLetterQueue = "DeadLetterQueue",
                DeadLetterOptions = new IOption[] { mockOption.Object },
                Inputs = mockInputs.Object,
                ResourceId = resourceId
            };
            var otherTriggerQueue = new TriggerQueue { Concurrency = 2 };
            var equals = triggerQueue.Equals(otherTriggerQueue);
            Assert.IsFalse(equals);
            Assert.AreEqual(resourceId, triggerQueue.ResourceId);
        }
    }
}
