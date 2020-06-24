/*
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
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Data.ServiceModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QueueWorker;
using System;
using System.Collections.Generic;
using System.IO;
using Warewolf.Common;
using Warewolf.Data;
using Warewolf.Options;
using Warewolf.OS.IO;
using Warewolf.Trigger.Queue;
using Warewolf.Triggers;

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
            var context = ConstructWorkerContext(out var rabbitSource, out _);

            Assert.AreEqual(rabbitSource, context.Source);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(QueueWorker))]
        public void WorkerContext_GivenValidConstruct_ExpectValidSourceAndDeadLetterSink()
        {
            var context = ConstructWorkerContext(out var expectedSource, out var expectedDeadLetterSink);

            Assert.AreEqual(_sinkId, context.DeadLetterSink.ResourceID);
            Assert.AreEqual(_sourceId, context.Source.ResourceID);
            Assert.AreEqual(expectedSource, context.Source);
            Assert.AreEqual(expectedDeadLetterSink, context.DeadLetterSink);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(QueueWorker))]
        public void WorkerContext_GivenValidConstruct_ExpectQueueName()
        {
            var context = ConstructWorkerContext(out var _, out var _);

            Assert.AreEqual(_queueName, context.QueueName);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(QueueWorker))]
        public void WorkerContext_GivenValidConstruct_ExpectValidInputs()
        {
            var context = ConstructWorkerContext(out var _, out var _);

            Assert.AreEqual(1, context.Inputs.Length);
            Assert.AreEqual(_expectedIServiceInput, context.Inputs[0]);
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(QueueWorker))]
        public void WorkerContext_GivenValidConstruct_ExpectQueueConfig()
        {
            var context = ConstructWorkerContext(out var _, out var _);

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
            var context = ConstructWorkerContext(out var _, out var _);

            var url = context.WorkflowUrl;
            var expected = "http://somehost:1234//secure/Some Workflow.json";
            Assert.AreEqual(expected, url);
        }


        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(QueueWorker))]
        public void WorkerContext_WatchTriggerResource_ShouldHaveValidSetup()
        {
            var context = ConstructWorkerContext(out var _, out var _);

            var mockWatcher = new Mock<IFileSystemWatcher>();
            
            var watcher = mockWatcher.Object;
            context.WatchTriggerResource(watcher);

            Assert.IsTrue(watcher is IFileSystemWatcher);

            mockWatcher.VerifySet(o => o.Path = "C:\\ProgramData\\Warewolf\\Triggers\\Queue", Times.Once);
            mockWatcher.VerifySet(o => o.Filter = $"{_resourceId}.bite", Times.Once);
            mockWatcher.VerifySet(o => o.NotifyFilter = NotifyFilters.LastWrite
                    | NotifyFilters.FileName
                    | NotifyFilters.DirectoryName, Times.Once);
            mockWatcher.VerifySet(o => o.EnableRaisingEvents = false, Times.Once);
            mockWatcher.VerifySet(o => o.EnableRaisingEvents = true, Times.Once);
        }

        private static IWorkerContext ConstructWorkerContext(out RabbitMQSource rabbitSource, out RabbitMQSource rabbitSink)
        {
            var mockArgs = new Mock<IArgs>();
            mockArgs.Setup(o => o.TriggerId).Returns(_resourceId.ToString());
            mockArgs.Setup(o => o.ServerEndpoint).Returns(new Uri("http://somehost:1234"));
            var mockResourceCatalogProxy = new Mock<IResourceCatalogProxy>();
            var mockFilePath = new Mock<IFilePath>();
            mockFilePath.Setup(o => o.GetDirectoryName(It.IsAny<string>())).Returns("C:\\ProgramData\\Warewolf\\Triggers\\Queue");
            mockFilePath.Setup(o => o.GetFileName(It.IsAny<string>())).Returns(_resourceId.ToString()+".bite");

            rabbitSource = new RabbitMQSource
            {
                ResourceID = _sourceId,
                HostName = "somehost",
                ResourceName = "my somehost resource",
            };
            rabbitSink = new RabbitMQSource
            {
                ResourceID = _sinkId,
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
                Inputs = new List<IServiceInputBase> { _expectedIServiceInput },
            };

            mockTriggerCatalog.Setup(o => o.PathFromResourceId(It.IsAny<string>())).Returns("C:\\ProgramData\\Warewolf\\Triggers\\Queue");
            mockTriggerCatalog.Setup(o => o.LoadQueueTriggerFromFile(It.IsAny<string>())).Returns(triggerQueue);
            return new WorkerContext(mockArgs.Object, mockResourceCatalogProxy.Object, mockTriggerCatalog.Object, mockFilePath.Object);
        }
    }
}
