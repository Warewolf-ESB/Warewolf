/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Data.TO;
using Dev2.Common.Interfaces.DB;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using Warewolf.Options;

namespace Warewolf.Trigger.Queue.Tests
{
    [TestClass]
    public class TriggerQueueViewTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(TriggerQueueView))]
        public void TriggerQueueView_Equals_Other_IsNull_Expect_False()
        {
            var triggerQueueView = new TriggerQueueView();
            var equals = triggerQueueView.Equals(null);
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(TriggerQueueView))]
        public void TriggerQueueView_ReferenceEquals_Match_Expect_True()
        {
            var triggerQueueView = new TriggerQueueView { Concurrency = 1 };
            var otherTriggerQueueView = triggerQueueView;
            var equals = triggerQueueView.Equals(otherTriggerQueueView);
            Assert.IsTrue(equals);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(TriggerQueueView))]
        public void TriggerQueueView_Equals_MisMatch_Expect_False()
        {
            var resourceId = Guid.NewGuid();
            var queueSinkResourceId = Guid.NewGuid();

            var mockOption = new Mock<IOption>();
            var mockInputs = new Mock<ICollection<IServiceInput>>();

            var triggerQueueView = new TriggerQueueView
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

            var otherTriggerQueueView = new TriggerQueueView { Concurrency = 2 };
            var equals = triggerQueueView.Equals(otherTriggerQueueView);
            Assert.IsFalse(equals);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(TriggerQueueView))]
        public void TriggerQueueView_Defaults_For_Coverage_To_Remove()
        {
            var mockErrorResultTO = new Mock<IErrorResultTO>();
            var resourceId = Guid.NewGuid();
            var triggerQueueView = new TriggerQueueView
            {
                TriggerId = resourceId,
                ResourceId = resourceId,
                OldQueueName = "OldName",
                Enabled = true,
                Errors = mockErrorResultTO.Object,
                TriggerQueueName = "TriggerQueueName",
                NameForDisplay = "NameForDisplay",
                IsNewQueue = true
            };

            Assert.AreEqual(resourceId, triggerQueueView.TriggerId);
            Assert.AreEqual(resourceId, triggerQueueView.ResourceId);
            Assert.IsFalse(triggerQueueView.IsDirty);
            Assert.AreEqual("OldName", triggerQueueView.OldQueueName);
            Assert.IsTrue(triggerQueueView.Enabled);
            Assert.IsNotNull(triggerQueueView.Errors);
            Assert.IsTrue(triggerQueueView.IsNewQueue);
            Assert.AreEqual("TriggerQueueName", triggerQueueView.TriggerQueueName);
            Assert.AreEqual("NameForDisplay", triggerQueueView.NameForDisplay);
            Assert.IsTrue(triggerQueueView.IsNewQueue);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(TriggerQueueView))]
        public void TriggerQueueView_Item_IsDirty_True()
        {
            var resourceId = Guid.NewGuid();
            var triggerQueueView = new TriggerQueueView
            {
                ResourceId = resourceId,
                OldQueueName = "OldName",
                Enabled = true,
                TriggerQueueName = "TriggerQueueName",
                IsNewQueue = true
            };

            var triggerQueueViewItem = new TriggerQueueView
            {
                ResourceId = resourceId,
                OldQueueName = "OldName",
                Enabled = true,
                TriggerQueueName = "TriggerQueueName",
                IsNewQueue = true,
                Item = triggerQueueView
            };

            Assert.AreEqual("TriggerQueueName *", triggerQueueViewItem.NameForDisplay);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(TriggerQueueView))]
        public void TriggerQueueView_Item_IsDirty_False()
        {
            var resourceId1 = Guid.NewGuid();
            var resourceId2 = Guid.NewGuid();
            var triggerQueueView = new TriggerQueueView
            {
                ResourceId = resourceId1,
                OldQueueName = "OldName",
                Enabled = true,
                TriggerQueueName = "TriggerQueueName",
                IsNewQueue = true
            };

            var triggerQueueViewItem = new TriggerQueueView
            {
                ResourceId = resourceId2,
                OldQueueName = "OldName",
                Enabled = true,
                TriggerQueueName = "TriggerQueueName",
                IsNewQueue = true,
                Item = triggerQueueView
            };

            Assert.AreEqual("TriggerQueueName", triggerQueueViewItem.NameForDisplay);
        }
    }
}
