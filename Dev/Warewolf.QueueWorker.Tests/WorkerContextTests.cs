﻿/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Triggers;
using Dev2.Data.ServiceModel;
using Dev2.Triggers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QueueWorker;
using System;
using System.Collections.Generic;
using Warewolf.Common;
using Warewolf.Options;
using Warewolf.Trigger.Queue;

namespace Warewolf.QueueWorker.Tests
{
    [TestClass]
    public class WorkerContextTests
    {
        private static readonly Guid _resourceId = Guid.Parse("a0a7ffdb-6b6e-4e6c-8ff1-9734db4f34e4");
        private static readonly Guid _sinkId = Guid.Parse("0e3b4559-e922-415f-8f2e-2c3c64d88f1a");
        private static readonly Guid _sourceId = Guid.Parse("375e2a80-98dd-47f7-8c6b-434a78495b89");
        private static readonly string _queueName = "SomeQueue";
        private static readonly IServiceInput _expectedIServiceInput = new Mock<IServiceInput>().Object;
        private static readonly string _workflowName = "Some Workflow";

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(QueueWorker))]
        public void WorkerContext_GivenValidConstruct_ExpectValidSource()
        {
            var context = ConstructWorkerContext(out var rabbitSource, out var rabbitSink);

            Assert.AreEqual(rabbitSource, context.Source);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(QueueWorker))]
        public void WorkerContext_GivenValidConstruct_ExpectValidDeadLetterSink()
        {
            var context = ConstructWorkerContext(out var rabbitSource, out var rabbitSink);

            Assert.AreEqual(rabbitSource, context.DeadLetterSink);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(QueueWorker))]
        public void WorkerContext_GivenValidConstruct_ExpectQueueName()
        {
            var context = ConstructWorkerContext(out var rabbitSource, out var rabbitSink);

            Assert.AreEqual(_queueName, context.QueueName);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(QueueWorker))]
        public void WorkerContext_GivenValidConstruct_ExpectValidInputs()
        {
            var context = ConstructWorkerContext(out var rabbitSource, out var rabbitSink);

            Assert.AreEqual(1, context.Inputs.Length);
            Assert.AreEqual(_expectedIServiceInput, context.Inputs[0]);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(QueueWorker))]
        public void WorkerContext_GivenValidConstruct_ExpectQueueConfig()
        {
            var context = ConstructWorkerContext(out var rabbitSource, out var rabbitSink);

            var config = context.QueueConfig;
            var queueName = config.GetType().GetProperty("QueueName");
            var durable = config.GetType().GetProperty("Durable");
            Assert.AreEqual(_queueName, queueName.GetValue(config));
            Assert.AreEqual(true, durable.GetValue(config));
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(QueueWorker))]
        public void WorkerContext_GivenValidConstruct_ExpectValidWorkflowUrl()
        {
            var context = ConstructWorkerContext(out var rabbitSource, out var rabbitSink);

            var url = context.WorkflowUrl;
            var expected = "http://somehost:1234//secure/Some Workflow.json";
            Assert.AreEqual(expected, url);
        }

        private static IWorkerContext ConstructWorkerContext(out RabbitMQSource rabbitSource, out RabbitMQSource rabbitSink)
        {
            var mockArgs = new Mock<IArgs>();
            mockArgs.Setup(o => o.TriggerId).Returns(_resourceId.ToString());
            mockArgs.Setup(o => o.ServerEndpoint).Returns(new Uri("http://somehost:1234"));
            var mockResourceCatalogProxy = new Mock<IResourceCatalogProxy>();

            rabbitSource = new RabbitMQSource
            {
                ResourceID = _sourceId,
                HostName = "somehost",
                ResourceName = "my somehost resource",
            };
            rabbitSink = new RabbitMQSource
            {
                ResourceID = _sourceId,
                HostName = "somehost",
                ResourceName = "my somehost resource",
            };
            mockResourceCatalogProxy.Setup(o => o.GetResourceById<RabbitMQSource>(GlobalConstants.ServerWorkspaceID, _sourceId)).Returns(rabbitSource);
            mockResourceCatalogProxy.Setup(o => o.GetResourceById<RabbitMQSource>(GlobalConstants.ServerWorkspaceID, _sinkId)).Returns(rabbitSink);
            var mockTriggerCatalog = new Mock<ITriggersCatalog>();

            var triggerQueue = new TriggerQueue
            {
                QueueSourceId = _sourceId,
                QueueSinkId = _sinkId,
                Options = new IOption[] { new OptionBool { Name = "Durable", Value = true } },
                ResourceId = _resourceId,
                QueueName = _queueName,
                WorkflowName = _workflowName,
                Inputs = new List<IServiceInput> { _expectedIServiceInput },
            };

            mockTriggerCatalog.Setup(o => o.LoadQueueTriggerFromFile(It.IsAny<string>())).Returns(triggerQueue);
            return new WorkerContext(mockArgs.Object, mockResourceCatalogProxy.Object, mockTriggerCatalog.Object);
        }
    }
}
