/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Wrappers;
using Dev2.Network;
using Dev2.Runtime.Hosting;
using Dev2.Studio.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QueueWorker;
using System;
using System.CodeDom;
using Warewolf.Common;
using Warewolf.Interfaces.Auditing;
using Warewolf.Streams;
using Warewolf.Triggers;
using static QueueWorker.ExecutionLogger;
using static QueueWorker.Program;
using Implementation = QueueWorker.Program.Implementation;

namespace Warewolf.QueueWorker.Tests
{
    [TestClass]
    public class QueueWorkerProgramTests
    {

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(QueueWorker))]
        public void QueueWorker_Implementation_Run_Given_ConnectionToServer_IsNotSuccessFul_ShouldTerminateConsole_AfterOneMinutes()
        {
            var id = Guid.NewGuid().ToString();
            
            //Do we not need to time this wait for connection so it teminates at some point?
            //When I tried this with the below the I think the wait is Infinite, my guess is this line: await _wrappedConnection.ConnectAsync(_wrappedConnection.ID).ConfigureAwait(true)
            //var environmentConnection = new ServerProxy(new Uri("https://localhost:3143/")); 
            var args = new Args
            {
                TriggerId = id,
                ServerEndpoint = new Uri("https://localhost:3143/")
            };

            var mockEnvironmentConnection = new Mock<IEnvironmentConnection>();
            mockEnvironmentConnection.Setup(o => o.ConnectAsync(Guid.Empty)).ReturnsAsync(false);

            var mockFilePath = new Mock<IFilePath>();

            var mockResourceCatalogProxy = new Mock<IResourceCatalogProxy>();
            var mockTriggersCatalog = new Mock<ITriggersCatalog>();
            var mockWorkerContextFactory = new Mock<IWorkerContextFactory>();
            mockWorkerContextFactory.Setup(o => o.New(args, mockResourceCatalogProxy.Object, mockTriggersCatalog.Object, mockFilePath.Object)).Throws(new Exception("false test exeption: catalog.LoadQueueTriggerFromFile(_path) => file not found, this might be a bug"));

            var implConfig = SetupQueueWorkerImplementationConfings(out Mock<IWriter> mockWriter, out Mock<IExecutionLogPublisher> mockExecutionLogPublisher,
                mockEnvironmentConnection.Object, mockResourceCatalogProxy.Object, mockWorkerContextFactory.Object, mockTriggersCatalog.Object, mockFilePath.Object, 
                new Mock<IFileSystemWatcherFactory>().Object, new Mock<IQueueWorkerImplementationFactory>().Object, new Mock<IEnvironmentWrapper>().Object);

            var sut = new Implementation(args, implConfig);

            var result = sut.Run();

            Assert.AreEqual(0, result, "");

            mockWriter.Verify(o => o.Write("Connecting to server: " + args.ServerEndpoint + "..."), Times.Once);
            mockWriter.Verify(o => o.WriteLine("failed."), Times.Once);

            mockExecutionLogPublisher.Verify(o => o.Info("Connecting to server: " + args.ServerEndpoint + "..."), Times.Once);
            mockExecutionLogPublisher.Verify(o => o.Info("Connecting to server: " + args.ServerEndpoint + "... unsuccessful"), Times.Once);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(QueueWorker))]
        public void QueueWorker_Implementation_Run_Given_ConnectionToServer_Throws_ShouldTerminateConsole()
        {
            var id = Guid.NewGuid().ToString();
            var args = new Args
            {
                TriggerId = id,
                ServerEndpoint = new Uri("https://localhost:3143/")
            };

            var mockEnvironmentConnection = new Mock<IEnvironmentConnection>();
            mockEnvironmentConnection.Setup(o => o.ConnectAsync(Guid.Empty)).Throws(new Exception("false test exception: the console should be able to print this exception for user feedback"));

            var mockFilePath = new Mock<IFilePath>();

            var mockResourceCatalogProxy = new Mock<IResourceCatalogProxy>();
            var mockTriggersCatalog = new Mock<ITriggersCatalog>();
            var mockWorkerContextFactory = new Mock<IWorkerContextFactory>();
            mockWorkerContextFactory.Setup(o => o.New(args, mockResourceCatalogProxy.Object, mockTriggersCatalog.Object, mockFilePath.Object)).Throws(new Exception("false test exeption: catalog.LoadQueueTriggerFromFile(_path) => file not found, this might be a bug"));

            var implConfig = SetupQueueWorkerImplementationConfings(out Mock<IWriter> mockWriter, out Mock<IExecutionLogPublisher> mockExecutionLogPublisher,
                mockEnvironmentConnection.Object, mockResourceCatalogProxy.Object, mockWorkerContextFactory.Object, mockTriggersCatalog.Object, mockFilePath.Object, 
                new Mock<IFileSystemWatcherFactory>().Object, new Mock<IQueueWorkerImplementationFactory>().Object, new Mock<IEnvironmentWrapper>().Object);

            var sut = new Implementation(args, implConfig);

            var result = sut.Run();

            Assert.AreEqual(0, result, "");

            mockWriter.Verify(o => o.Write("Connecting to server: " + args.ServerEndpoint + "..."), Times.Once);
            mockWriter.Verify(o => o.WriteLine("failed."), Times.Once);

            mockExecutionLogPublisher.Verify(o => o.Info("Connecting to server: " + args.ServerEndpoint + "..."), Times.Once);
            mockExecutionLogPublisher.Verify(o => o.Info("Connecting to server: " + args.ServerEndpoint + "... unsuccessful"), Times.Once);
            mockExecutionLogPublisher.Verify(o => o.Error("false test exception: the console should be able to print this exception for user feedback", args.ServerEndpoint), Times.Once);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(QueueWorker))]
        public void QueueWorker_Implementation_Run_Given_FileNotExist_ShouldTerminateConsole()
        {
            var id = Guid.NewGuid().ToString();
            var args = new Args
            {
                TriggerId = id,
                ServerEndpoint = new Uri("https://localhost:3143/")
            };

            var mockEnvironmentConnection = new Mock<IEnvironmentConnection>();
            mockEnvironmentConnection.Setup(o => o.ConnectAsync(Guid.Empty)).ReturnsAsync(true);

            var mockFilePath = new Mock<IFilePath>();

            var mockResourceCatalogProxy = new Mock<IResourceCatalogProxy>();
            var mockTriggersCatalog = new Mock<ITriggersCatalog>();
            var mockWorkerContextFactory = new Mock<IWorkerContextFactory>();
            mockWorkerContextFactory.Setup(o => o.New(args, mockResourceCatalogProxy.Object, mockTriggersCatalog.Object, mockFilePath.Object)).Throws(new Exception("false test exeption: catalog.LoadQueueTriggerFromFile(_path) => file not found, this might be a bug"));

            var implConfig = SetupQueueWorkerImplementationConfings(out Mock<IWriter> mockWriter, out Mock<IExecutionLogPublisher> mockExecutionLogPublisher,
                mockEnvironmentConnection.Object, mockResourceCatalogProxy.Object, mockWorkerContextFactory.Object, mockTriggersCatalog.Object, mockFilePath.Object, 
                new Mock<IFileSystemWatcherFactory>().Object, new Mock<IQueueWorkerImplementationFactory>().Object, new Mock<IEnvironmentWrapper>().Object);

            var sut = new Implementation(args, implConfig);

            var result = sut.Run();

            Assert.AreEqual(0, result, "");

            mockWriter.Verify(o => o.Write("Connecting to server: " + args.ServerEndpoint + "..."), Times.Once);
            mockWriter.Verify(o => o.WriteLine("done."), Times.Once);
            mockWriter.Verify(o => o.Write(@"Loading trigger resource: " + args.TriggerId + " ..."), Times.Once);
            mockWriter.Verify(o => o.WriteLine("failed."), Times.Once);
            mockWriter.Verify(o => o.Write("false test exeption: catalog.LoadQueueTriggerFromFile(_path) => file not found, this might be a bug"), Times.Once);

            mockExecutionLogPublisher.Verify(o => o.Info("Connecting to server: " + args.ServerEndpoint + "..."), Times.Once);
            mockExecutionLogPublisher.Verify(o => o.Info("Connecting to server: " + args.ServerEndpoint + "... successful"), Times.Once);
            mockExecutionLogPublisher.Verify(o => o.Info(@"Loading trigger resource: " + args.TriggerId + " ..."), Times.Once);
            mockExecutionLogPublisher.Verify(o => o.Error("false test exeption: catalog.LoadQueueTriggerFromFile(_path) => file not found, this might be a bug", args.TriggerId));
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(QueueWorker))]
        public void QueueWorker_Implementation_Run_Given_FileExist_ShouldNotTerminateConsole()
        {
            var id = Guid.NewGuid().ToString();

            var args = new Args
            {
                TriggerId = id,
                ServerEndpoint = new Uri("https://localhost:3143/")
            };

            var mockEnvironmentConnection = new Mock<IEnvironmentConnection>();
            mockEnvironmentConnection.Setup(o => o.ConnectAsync(Guid.Empty)).ReturnsAsync(true);

            var mockFilePath = new Mock<IFilePath>();

            var mockWorkerContext = new Mock<IWorkerContext>();
            
            var mockQueueWorkerImplementationFactory = new Mock<IQueueWorkerImplementationFactory>();
            mockQueueWorkerImplementationFactory.Setup(o => o.New(mockWorkerContext.Object)).Returns(new Mock<IQueueWorkerImplementation>().Object);

            var mockFileSystemWatcherWrapper = new Mock<IFileSystemWatcherWrapper>();
            var mockFileSystemWatcherFactory = new Mock<IFileSystemWatcherFactory>();
            mockFileSystemWatcherFactory.Setup(o => o.New()).Returns(mockFileSystemWatcherWrapper.Object);

            var mockEnvironmentWrapper = new Mock<IEnvironmentWrapper>();
            var environment = mockEnvironmentWrapper.Object;

            var mockResourceCatalogProxy = new Mock<IResourceCatalogProxy>();
            var mockTriggersCatalog = new Mock<ITriggersCatalog>();
            var mockWorkerContextFactory = new Mock<IWorkerContextFactory>();
            mockWorkerContextFactory.Setup(o => o.New(args, mockResourceCatalogProxy.Object, mockTriggersCatalog.Object, mockFilePath.Object)).Returns(mockWorkerContext.Object);

            var implConfig = SetupQueueWorkerImplementationConfings(out Mock<IWriter> mockWriter, out Mock<IExecutionLogPublisher> mockExecutionLogPublisher,
                mockEnvironmentConnection.Object, mockResourceCatalogProxy.Object, mockWorkerContextFactory.Object, mockTriggersCatalog.Object, mockFilePath.Object, 
                mockFileSystemWatcherFactory.Object, mockQueueWorkerImplementationFactory.Object, mockEnvironmentWrapper.Object);

            var sut = new Implementation(args, implConfig);

            var result = sut.Run();

            Assert.AreEqual(0, result, "");

            mockWriter.Verify(o => o.Write("Connecting to server: " + args.ServerEndpoint + "..."), Times.Once);
            mockWriter.Verify(o => o.Write(@"Loading trigger resource: " + args.TriggerId + " ..."), Times.Once);
            mockWriter.Verify(o => o.WriteLine("done."), Times.Exactly(2));
            mockWriter.Verify(o => o.Write("Start watching trigger resource: " + args.TriggerId), Times.Once);

            mockExecutionLogPublisher.Verify(o => o.Info("Connecting to server: " + args.ServerEndpoint + "..."), Times.Once);
            mockExecutionLogPublisher.Verify(o => o.Info("Connecting to server: " + args.ServerEndpoint + "... successful"), Times.Once);
            mockExecutionLogPublisher.Verify(o => o.Info(@"Loading trigger resource: " + args.TriggerId + " ..."), Times.Once);
            mockExecutionLogPublisher.Verify(o => o.Info(@"Loading trigger resource: " + args.TriggerId + " ... successful"), Times.Once);
            mockExecutionLogPublisher.Verify(o => o.Info("Start watching trigger resource: " + args.TriggerId), Times.Once);

        }

        private Implementation.Config SetupQueueWorkerImplementationConfings(out Mock<IWriter> mockWriter, out Mock<IExecutionLogPublisher> mockExecutionLogPublisher, 
            IEnvironmentConnection environmentConnection, IResourceCatalogProxy resourceCatalogProxy, IWorkerContextFactory workerContextFactory, ITriggersCatalog triggersCatalog, 
            IFilePath filePath, IFileSystemWatcherFactory fileSystemWatcherFactory, IQueueWorkerImplementationFactory queueWorkerImplementationFactory, IEnvironmentWrapper environmentWrapper)
        {
            mockWriter = new Mock<IWriter>();

            mockExecutionLogPublisher = new Mock<IExecutionLogPublisher>();
            var mockExecutionLoggerFactory = new Mock<IExecutionLoggerFactory>();
            mockExecutionLoggerFactory.Setup(o => o.New(It.IsAny<ISerializer>(), It.IsAny<IWebSocketPool>())).Returns(mockExecutionLogPublisher.Object);

            var mockResourceCatalogProxyFactory = new Mock<IResourceCatalogProxyFactory>();
            mockResourceCatalogProxyFactory.Setup(o => o.New(environmentConnection)).Returns(resourceCatalogProxy);

            var mockServerProxyFactory = new Mock<IServerProxyFactory>();
            mockServerProxyFactory.Setup(o => o.New(new Uri("https://localhost:3143/"))).Returns(environmentConnection);

            var mockTriggersCatalogFactory = new Mock<ITriggersCatalogFactory>();
            mockTriggersCatalogFactory.Setup(o => o.New()).Returns(triggersCatalog);

            var implConfig = new Implementation.Config
            {
                EnvironmentWrapper = environmentWrapper,
                ExecutionLoggerFactory = mockExecutionLoggerFactory.Object,
                FilePath = filePath,
                FileSystemWatcherFactory = fileSystemWatcherFactory,
                QueueWorkerImplementationFactory = queueWorkerImplementationFactory,
                ResourceCatalogProxyFactory = mockResourceCatalogProxyFactory.Object,
                ServerProxyFactory = mockServerProxyFactory.Object,
                TriggersCatalogFactory = mockTriggersCatalogFactory.Object,
                WorkerContextFactory = workerContextFactory,
                Writer = mockWriter.Object
            };

            return implConfig;
        }
    }
}
