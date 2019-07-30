/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Resources;
using Dev2.Studio.Interfaces;
using Dev2.Tasks.QueueEvents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;

namespace Dev2.Studio.Tests.ViewModels.Tasks.QueueEvents
{
    [TestClass]
    public class QueueEventsViewModelTests
    {
        [TestMethod]
        [TestCategory(nameof(QueueEventsViewModel))]
        [Owner("Pieter Terblanche")]
        public void QueueEventsViewModel_QueueEvents()
        {
            var mockServer = new Mock<IServer>();
            var queueEventsViewModel = new QueueEventsViewModel(mockServer.Object);

            Assert.IsNull(queueEventsViewModel.QueueEvents);
            Assert.IsNull(queueEventsViewModel.SelectedQueueEvent);

            var queue1 = "Queue1";
            var queue2 = "Queue2";

            var queueEvents = new System.Collections.ObjectModel.ObservableCollection<string>
            {
                queue1,
                queue2
            };

            queueEventsViewModel.QueueEvents = queueEvents;
            queueEventsViewModel.SelectedQueueEvent = queue2;

            Assert.IsNotNull(queueEventsViewModel.QueueEvents);
            Assert.AreEqual(2, queueEventsViewModel.QueueEvents.Count);

            Assert.IsNotNull(queueEventsViewModel.SelectedQueueEvent);
            Assert.AreEqual(queue2, queueEventsViewModel.SelectedQueueEvent);
        }

        [TestMethod]
        [TestCategory(nameof(QueueEventsViewModel))]
        [Owner("Pieter Terblanche")]
        public void QueueEventsViewModel_QueueSources()
        {
            var queueSourceID1 = Guid.NewGuid();
            var queueSourceName1 = "QueueSource1";

            var queueSourceID2 = Guid.NewGuid();
            var queueSourceName2 = "QueueSource2";

            var queueSource1 = new TestQueueSource { ResourceID = queueSourceID1, ResourceName = queueSourceName1 };
            var queueSource2 = new TestQueueSource { ResourceID = queueSourceID2, ResourceName = queueSourceName2 };

            var expectedList = new List<IQueueSource>
            {
                queueSource1, queueSource2
            };

            var mockServer = new Mock<IServer>();
            var mockResourceRepository = new Mock<IResourceRepository>();
            mockResourceRepository.Setup(resourceRepository => resourceRepository.FindResourcesByType<IQueueSource>(mockServer.Object)).Returns(expectedList);

            mockServer.Setup(server => server.ResourceRepository).Returns(mockResourceRepository.Object);

            var queueEventsViewModel = new QueueEventsViewModel(mockServer.Object);

            Assert.IsNotNull(queueEventsViewModel.QueueSources);
            Assert.IsNull(queueEventsViewModel.SelectedQueueSource);

            queueEventsViewModel.SelectedQueueSource = queueSource2;

            Assert.IsNotNull(queueEventsViewModel.QueueSources);
            Assert.AreEqual(2, queueEventsViewModel.QueueSources.Count);
            Assert.AreEqual(queueSourceID1, queueEventsViewModel.QueueSources[0].ResourceID);
            Assert.AreEqual(queueSourceName1, queueEventsViewModel.QueueSources[0].ResourceName);
            Assert.AreEqual(queueSourceID2, queueEventsViewModel.QueueSources[1].ResourceID);
            Assert.AreEqual(queueSourceName2, queueEventsViewModel.QueueSources[1].ResourceName);

            Assert.IsNotNull(queueEventsViewModel.SelectedQueueSource);
            Assert.AreEqual(queueSource2, queueEventsViewModel.SelectedQueueSource);
        }

        [TestMethod]
        [TestCategory(nameof(QueueEventsViewModel))]
        [Owner("Pieter Terblanche")]
        public void QueueEventsViewModel_QueueNames()
        {
            var mockServer = new Mock<IServer>();
            var queueEventsViewModel = new QueueEventsViewModel(mockServer.Object);

            Assert.IsNull(queueEventsViewModel.QueueNames);
            Assert.IsNull(queueEventsViewModel.QueueName);

            var queueName1 = "QueueName1";
            var queueName2 = "QueueName2";

            var queueNames = new System.Collections.ObjectModel.ObservableCollection<string>
            {
                queueName1,
                queueName2
            };

            queueEventsViewModel.QueueNames = queueNames;
            queueEventsViewModel.QueueName = queueName2;

            Assert.IsNotNull(queueEventsViewModel.QueueNames);
            Assert.AreEqual(2, queueEventsViewModel.QueueNames.Count);

            Assert.IsNotNull(queueEventsViewModel.QueueName);
            Assert.AreEqual(queueName2, queueEventsViewModel.QueueName);
        }

        [TestMethod]
        [TestCategory(nameof(QueueEventsViewModel))]
        [Owner("Pieter Terblanche")]
        public void QueueEventsViewModel_WorkflowName()
        {
            var mockServer = new Mock<IServer>();
            var queueEventsViewModel = new QueueEventsViewModel(mockServer.Object);

            Assert.IsNull(queueEventsViewModel.WorkflowName);

            var workflowName = "Workflow1";
            queueEventsViewModel.WorkflowName = workflowName;

            Assert.AreEqual(workflowName, queueEventsViewModel.WorkflowName);
        }

        [TestMethod]
        [TestCategory(nameof(QueueEventsViewModel))]
        [Owner("Pieter Terblanche")]
        public void QueueEventsViewModel_Concurrency()
        {
            var mockServer = new Mock<IServer>();
            var queueEventsViewModel = new QueueEventsViewModel(mockServer.Object);

            Assert.AreEqual(0, queueEventsViewModel.Concurrency);

            var concurrency = 5;
            queueEventsViewModel.Concurrency = concurrency;

            Assert.AreEqual(concurrency, queueEventsViewModel.Concurrency);
        }

        [TestMethod]
        [TestCategory(nameof(QueueEventsViewModel))]
        [Owner("Pieter Terblanche")]
        public void QueueEventsViewModel_QueueEvents_AddNew_And_Delete()
        {
            var mockServer = new Mock<IServer>();
            var queueEventsViewModel = new QueueEventsViewModel(mockServer.Object);

            Assert.IsNull(queueEventsViewModel.QueueEvents);

            queueEventsViewModel.QueueEvents = new System.Collections.ObjectModel.ObservableCollection<string>();
            queueEventsViewModel.NewCommand.Execute(null);

            Assert.IsNotNull(queueEventsViewModel.QueueEvents);
            Assert.AreEqual(1, queueEventsViewModel.QueueEvents.Count);

            queueEventsViewModel.DeleteCommand.Execute(null);
            Assert.AreEqual(0, queueEventsViewModel.QueueEvents.Count);
        }
    }

    public class TestQueueSource : IQueueSource
    {
        public Guid ResourceID { get; set; }
        public string ResourceName { get; set; }
    }
}
