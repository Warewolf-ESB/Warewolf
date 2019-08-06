/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Resources;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Interfaces;
using Dev2.Tasks.QueueEvents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Warewolf.Core;

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

            var queueSource1 = new Resource { ResourceID = queueSourceID1, ResourceName = queueSourceName1 };
            var queueSource2 = new Resource { ResourceID = queueSourceID2, ResourceName = queueSourceName2 };

            var expectedList = new List<IResource>
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

            Assert.IsNotNull(queueEventsViewModel.QueueSources);
            Assert.AreEqual(2, queueEventsViewModel.QueueSources.Count);
            Assert.AreEqual(queueSourceID1, queueEventsViewModel.QueueSources[0].ResourceID);
            Assert.AreEqual(queueSourceName1, queueEventsViewModel.QueueSources[0].ResourceName);
            Assert.AreEqual(queueSourceID2, queueEventsViewModel.QueueSources[1].ResourceID);
            Assert.AreEqual(queueSourceName2, queueEventsViewModel.QueueSources[1].ResourceName);
        }

        [TestMethod]
        [TestCategory(nameof(QueueEventsViewModel))]
        [Owner("Pieter Terblanche")]
        public void QueueEventsViewModel_DeadLetterQueueSources()
        {
            var queueSourceID1 = Guid.NewGuid();
            var queueSourceName1 = "QueueSource1";

            var queueSourceID2 = Guid.NewGuid();
            var queueSourceName2 = "QueueSource2";

            var queueSource1 = new Resource { ResourceID = queueSourceID1, ResourceName = queueSourceName1 };
            var queueSource2 = new Resource { ResourceID = queueSourceID2, ResourceName = queueSourceName2 };

            var expectedList = new List<IResource>
            {
                queueSource1, queueSource2
            };

            var mockServer = new Mock<IServer>();
            var mockResourceRepository = new Mock<IResourceRepository>();
            mockResourceRepository.Setup(resourceRepository => resourceRepository.FindResourcesByType<IQueueSource>(mockServer.Object)).Returns(expectedList);

            mockServer.Setup(server => server.ResourceRepository).Returns(mockResourceRepository.Object);

            var queueEventsViewModel = new QueueEventsViewModel(mockServer.Object);

            Assert.IsNotNull(queueEventsViewModel.DeadLetterQueueSources);
            Assert.IsNull(queueEventsViewModel.SelectedDeadLetterQueueSource);

            Assert.IsNotNull(queueEventsViewModel.DeadLetterQueueSources);
            Assert.AreEqual(2, queueEventsViewModel.DeadLetterQueueSources.Count);
            Assert.AreEqual(queueSourceID1, queueEventsViewModel.DeadLetterQueueSources[0].ResourceID);
            Assert.AreEqual(queueSourceName1, queueEventsViewModel.DeadLetterQueueSources[0].ResourceName);
            Assert.AreEqual(queueSourceID2, queueEventsViewModel.DeadLetterQueueSources[1].ResourceID);
            Assert.AreEqual(queueSourceName2, queueEventsViewModel.DeadLetterQueueSources[1].ResourceName);
        }

        [TestMethod]
        [TestCategory(nameof(QueueEventsViewModel))]
        [Owner("Pieter Terblanche")]
        public void QueueEventsViewModel_QueueNames()
        {
            var queueSourceID2 = Guid.NewGuid();
            var queueSourceName2 = "QueueSource2";

            var queueSource2 = new Resource { ResourceID = queueSourceID2, ResourceName = queueSourceName2 };

            string[] tempValues = new string[3];
            tempValues[0] = "value1";
            tempValues[1] = "value2";
            tempValues[2] = "value3";

            var expectedQueueNames = new Dictionary<string, string[]>();
            expectedQueueNames.Add("QueueNames", tempValues);

            var mockServer = new Mock<IServer>();
            var mockResourceRepository = new Mock<IResourceRepository>();
            mockResourceRepository.Setup(resourceRepository => resourceRepository.FindAutocompleteOptions(mockServer.Object, queueSource2)).Returns(expectedQueueNames);

            mockServer.Setup(server => server.ResourceRepository).Returns(mockResourceRepository.Object);

            var queueEventsViewModel = new QueueEventsViewModel(mockServer.Object);

            queueEventsViewModel.SelectedQueueSource = queueSource2;

            Assert.IsNotNull(queueEventsViewModel.SelectedQueueSource);
            Assert.AreEqual(queueSource2, queueEventsViewModel.SelectedQueueSource);
            Assert.IsNotNull(queueEventsViewModel.QueueNames);
            Assert.AreEqual(3, queueEventsViewModel.QueueNames.Count);
            Assert.AreEqual("value1", queueEventsViewModel.QueueNames[0].Value);
            Assert.AreEqual("value2", queueEventsViewModel.QueueNames[1].Value);
            Assert.AreEqual("value3", queueEventsViewModel.QueueNames[2].Value);

            Assert.IsNull(queueEventsViewModel.QueueName);

            queueEventsViewModel.QueueName = "value1";

            Assert.IsNotNull(queueEventsViewModel.QueueName);
            Assert.AreEqual("value1", queueEventsViewModel.QueueName);
        }

        [TestMethod]
        [TestCategory(nameof(QueueEventsViewModel))]
        [Owner("Pieter Terblanche")]
        public void QueueEventsViewModel_DeadLetterQueues()
        {
            var queueSourceID2 = Guid.NewGuid();
            var queueSourceName2 = "QueueSource2";

            var queueSource2 = new Resource { ResourceID = queueSourceID2, ResourceName = queueSourceName2 };

            string[] tempValues = new string[3];
            tempValues[0] = "value1";
            tempValues[1] = "value2";
            tempValues[2] = "value3";

            var expectedQueueNames = new Dictionary<string, string[]>();
            expectedQueueNames.Add("QueueNames", tempValues);

            var mockServer = new Mock<IServer>();
            var mockResourceRepository = new Mock<IResourceRepository>();
            mockResourceRepository.Setup(resourceRepository => resourceRepository.FindAutocompleteOptions(mockServer.Object, queueSource2)).Returns(expectedQueueNames);

            mockServer.Setup(server => server.ResourceRepository).Returns(mockResourceRepository.Object);

            var queueEventsViewModel = new QueueEventsViewModel(mockServer.Object);

            queueEventsViewModel.SelectedDeadLetterQueueSource = queueSource2;

            Assert.IsNotNull(queueEventsViewModel.SelectedDeadLetterQueueSource);
            Assert.AreEqual(queueSource2, queueEventsViewModel.SelectedDeadLetterQueueSource);
            Assert.IsNotNull(queueEventsViewModel.DeadLetterQueues);
            Assert.AreEqual(3, queueEventsViewModel.DeadLetterQueues.Count);
            Assert.AreEqual("value1", queueEventsViewModel.DeadLetterQueues[0].Value);
            Assert.AreEqual("value2", queueEventsViewModel.DeadLetterQueues[1].Value);
            Assert.AreEqual("value3", queueEventsViewModel.DeadLetterQueues[2].Value);

            Assert.IsNull(queueEventsViewModel.DeadLetterQueue);

            queueEventsViewModel.DeadLetterQueue = "value1";

            Assert.IsNotNull(queueEventsViewModel.DeadLetterQueue);
            Assert.AreEqual("value1", queueEventsViewModel.DeadLetterQueue);
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

        [TestMethod]
        [TestCategory(nameof(QueueEventsViewModel))]
        [Owner("Pieter Terblanche")]
        public void QueueEventsViewModel_QueueEvents_Inputs()
        {
            var mockServer = new Mock<IServer>();
            var queueEventsViewModel = new QueueEventsViewModel(mockServer.Object);

            Assert.IsNotNull(queueEventsViewModel.Inputs);

            var inputs = new ObservableCollection<IServiceInput>();
            inputs.Add(new ServiceInput("name1", "value1"));
            inputs.Add(new ServiceInput("name2", "value2"));

            queueEventsViewModel.Inputs = inputs;

            var inputsAsList = queueEventsViewModel.Inputs.ToList();

            Assert.AreEqual(2, queueEventsViewModel.Inputs.Count);
            Assert.AreEqual("name1", inputsAsList[0].Name);
            Assert.AreEqual("value1", inputsAsList[0].Value);
            Assert.AreEqual("name2", inputsAsList[1].Name);
            Assert.AreEqual("value2", inputsAsList[1].Value);
        }

        [TestMethod]
        [TestCategory(nameof(QueueEventsViewModel))]
        [Owner("Pieter Terblanche")]
        public void QueueEventsViewModel_QueueEvents_PasteResponseVisible()
        {
            var mockServer = new Mock<IServer>();
            var queueEventsViewModel = new QueueEventsViewModel(mockServer.Object);

            Assert.IsFalse(queueEventsViewModel.PasteResponseVisible);

            queueEventsViewModel.PasteResponseVisible = true;

            Assert.IsTrue(queueEventsViewModel.PasteResponseVisible);
        }

        [TestMethod]
        [TestCategory(nameof(QueueEventsViewModel))]
        [Owner("Pieter Terblanche")]
        public void QueueEventsViewModel_QueueEvents_PasteResponse()
        {
            var mockServer = new Mock<IServer>();
            var queueEventsViewModel = new QueueEventsViewModel(mockServer.Object);

            Assert.IsNull(queueEventsViewModel.PasteResponse);

            queueEventsViewModel.PasteResponse = "Paste Response";

            Assert.AreEqual("Paste Response", queueEventsViewModel.PasteResponse);
        }

        [TestMethod]
        [TestCategory(nameof(QueueEventsViewModel))]
        [Owner("Pieter Terblanche")]
        public void QueueEventsViewModel_QueueEvents_ViewQueueStats()
        {
            Uri uri = new Uri("https://www.rabbitmq.com/blog/tag/statistics/");

            var mockServer = new Mock<IServer>();
            var mockExternalProcessExecutor = new Mock<IExternalProcessExecutor>();
            mockExternalProcessExecutor.Setup(externalProcessExecutor => externalProcessExecutor.OpenInBrowser(uri)).Verifiable();

            var queueEventsViewModel = new QueueEventsViewModel(mockServer.Object, mockExternalProcessExecutor.Object);

            queueEventsViewModel.QueueStatsCommand.Execute(null);

            mockExternalProcessExecutor.Verify(externalProcessExecutor => externalProcessExecutor.OpenInBrowser(uri), Times.Once);
        }

        [TestMethod]
        [TestCategory(nameof(QueueEventsViewModel))]
        [Owner("Pieter Terblanche")]
        public void QueueEventsViewModel_QueueEvents_PasteResponseCommand()
        {
            var mockServer = new Mock<IServer>();

            var queueEventsViewModel = new QueueEventsViewModel(mockServer.Object);
            Assert.IsFalse(queueEventsViewModel.PasteResponseVisible);

            queueEventsViewModel.PasteResponseCommand.Execute(null);
            Assert.IsTrue(queueEventsViewModel.PasteResponseVisible);
        }

        [TestMethod]
        [TestCategory(nameof(QueueEventsViewModel))]
        [Owner("Pieter Terblanche")]
        public void QueueEventsViewModel_QueueEvents_TestCommand()
        {
            var mockServer = new Mock<IServer>();

            var queueEventsViewModel = new QueueEventsViewModel(mockServer.Object);
            Assert.IsNull(queueEventsViewModel.TestResults);
            Assert.IsFalse(queueEventsViewModel.IsTesting);
            Assert.IsFalse(queueEventsViewModel.IsTestResultsEmptyRows);

            queueEventsViewModel.TestCommand.Execute(null);

            Assert.IsTrue(queueEventsViewModel.TestResultsAvailable);
            Assert.IsFalse(queueEventsViewModel.IsTestResultsEmptyRows);
            Assert.IsFalse(queueEventsViewModel.IsTesting);
            Assert.IsTrue(queueEventsViewModel.TestPassed);
            Assert.IsFalse(queueEventsViewModel.TestFailed);
        }
    }
}
