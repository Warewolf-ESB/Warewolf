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
            var mockResource = new Mock<IResource>();
            mockResource.Setup(resource => resource.ResourceID).Returns(Guid.NewGuid());

            var mockOption = new Mock<IOption>();
            var mockInputs = new Mock<ICollection<IServiceInput>>();

            var triggerQueueView = new TriggerQueueView
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
                ResourceId = resourceId,
                IsDirty = true,
                OldName = "OldName",
                Status = QueueStatus.Enabled,
                Errors = mockErrorResultTO.Object,
                IsNew = true,
                Name = "Name"
            };

            Assert.AreEqual(resourceId, triggerQueueView.ResourceId);
            Assert.IsTrue(triggerQueueView.IsDirty);
            Assert.AreEqual("OldName", triggerQueueView.OldName);
            Assert.AreEqual(QueueStatus.Enabled, triggerQueueView.Status);
            Assert.IsNotNull(triggerQueueView.Errors);
            Assert.IsTrue(triggerQueueView.IsNew);
            Assert.IsFalse(triggerQueueView.IsNewItem);
            Assert.AreEqual(string.Empty, triggerQueueView.NameForDisplay);
            Assert.AreEqual("Name", triggerQueueView.Name);
        }
    }
}
