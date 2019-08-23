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
using Dev2.Common.Interfaces.Data.TO;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Queue;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using Warewolf.Options;

namespace Warewolf.Trigger.Queue.Tests
{
    [TestClass]
    public class DummyTriggerQueueViewTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DummyTriggerQueueView))]
        public void DummyTriggerQueueView_Equals_Other_IsNull_Expect_False()
        {
            var triggerQueueView = new DummyTriggerQueueView();
            var equals = triggerQueueView.Equals(null);
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DummyTriggerQueueView))]
        public void DummyTriggerQueueView_ReferenceEquals_Match_Expect_True()
        {
            var triggerQueueView = new DummyTriggerQueueView { Concurrency = 1 };
            var otherDummyTriggerQueueView = triggerQueueView;
            var equals = triggerQueueView.Equals(otherDummyTriggerQueueView);
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DummyTriggerQueueView))]
        public void DummyTriggerQueueView_Equals_MisMatch_Expect_False()
        {
            var resourceId = Guid.NewGuid();
            var queueSinkResourceId = Guid.NewGuid();

            var mockOption = new Mock<IOption>();
            var mockInputs = new Mock<ICollection<IServiceInput>>();

            var triggerQueueView = new DummyTriggerQueueView
            {
                QueueSourceId = resourceId,
                QueueName = "Queue",
                WorkflowName = "Workflow",
                Concurrency = 100,
                UserName = "Bob",
                Password = "123456",
                Options = new IOption[] { mockOption.Object },
                QueueSinkId = queueSinkResourceId,
                DeadLetterQueue = "DeadLetterQueue",
                DeadLetterOptions = new IOption[] { mockOption.Object },
                Inputs = mockInputs.Object
            };

            var otherDummyTriggerQueueView = new DummyTriggerQueueView { Concurrency = 2 };
            var equals = triggerQueueView.Equals(otherDummyTriggerQueueView);
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(DummyTriggerQueueView))]
        public void DummyTriggerQueueView_Defaults_For_Coverage_To_Remove()
        {
            var mockErrorResultTO = new Mock<IErrorResultTO>();
            var resourceId = Guid.NewGuid();
            var triggerQueueView = new DummyTriggerQueueView
            {
                ResourceId = resourceId,
                IsDirty = true,
                OldQueueName = "OldName",
                Status = QueueStatus.Enabled,
                Errors = mockErrorResultTO.Object,
                Name = "Name"
            };

            Assert.AreEqual(resourceId, triggerQueueView.ResourceId);
            Assert.IsTrue(triggerQueueView.IsDirty);
            Assert.AreEqual("OldName", triggerQueueView.OldQueueName);
            Assert.AreEqual(QueueStatus.Enabled, triggerQueueView.Status);
            Assert.IsNotNull(triggerQueueView.Errors);
            Assert.IsFalse(triggerQueueView.IsNewQueue);
            Assert.AreEqual(string.Empty, triggerQueueView.NameForDisplay);
            Assert.AreEqual("Name", triggerQueueView.Name);
        }
    }
}
