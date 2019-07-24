/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Tasks.QueueEvents;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            var queueEventsViewModel = new QueueEventsViewModel();

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
            var queueEventsViewModel = new QueueEventsViewModel();

            Assert.IsNull(queueEventsViewModel.QueueSources);
            Assert.IsNull(queueEventsViewModel.SelectedQueueSource);

            var queueSource1 = "QueueSource1";
            var queueSource2 = "QueueSource2";

            var queueSources = new System.Collections.ObjectModel.ObservableCollection<string>
            {
                queueSource1,
                queueSource2
            };

            queueEventsViewModel.QueueSources = queueSources;
            queueEventsViewModel.SelectedQueueSource = queueSource2;

            Assert.IsNotNull(queueEventsViewModel.QueueSources);
            Assert.AreEqual(2, queueEventsViewModel.QueueSources.Count);

            Assert.IsNotNull(queueEventsViewModel.SelectedQueueSource);
            Assert.AreEqual(queueSource2, queueEventsViewModel.SelectedQueueSource);
        }

        [TestMethod]
        [TestCategory(nameof(QueueEventsViewModel))]
        [Owner("Pieter Terblanche")]
        public void QueueEventsViewModel_QueueNames()
        {
            var queueEventsViewModel = new QueueEventsViewModel();

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
            var queueEventsViewModel = new QueueEventsViewModel();

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
            var queueEventsViewModel = new QueueEventsViewModel();

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
            var queueEventsViewModel = new QueueEventsViewModel();

            Assert.IsNull(queueEventsViewModel.QueueEvents);

            queueEventsViewModel.QueueEvents = new System.Collections.ObjectModel.ObservableCollection<string>();
            queueEventsViewModel.NewCommand.Execute(null);

            Assert.IsNotNull(queueEventsViewModel.QueueEvents);
            Assert.AreEqual(1, queueEventsViewModel.QueueEvents.Count);

            queueEventsViewModel.DeleteCommand.Execute(null);
            Assert.AreEqual(0, queueEventsViewModel.QueueEvents.Count);
        }
    }
}
