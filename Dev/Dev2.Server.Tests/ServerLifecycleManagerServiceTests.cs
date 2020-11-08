/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.WebServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Warewolf.Interfaces.Auditing;
using Warewolf.OS;
using Warewolf.Triggers;
using WarewolfCOMIPC.Client;

namespace Dev2.Server.Tests
{
    [TestClass]
    [DoNotParallelize]
    public class ServerLifecycleManagerServiceTests
    {
        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ServerLifecycleManagerService))]
        public void ServerLifecycleManagerService_Construct()
        {
            using (var service = new ServerLifecycleManagerService())
            {
                Assert.IsFalse(service.CanPauseAndContinue);
            }
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ServerLifecycleManagerService))]
        public void ServerLifecycleManagerService_Construct_Sets_ServerInteractive_False()
        {
            var mockServerLifeManager = new Mock<IServerLifecycleManager>();
            mockServerLifeManager.SetupSet(o => o.InteractiveMode = false).Verifiable();
            using (new ServerLifecycleManagerServiceTest(mockServerLifeManager.Object))
            {
            }

            mockServerLifeManager.Verify();
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ServerLifecycleManagerService))]
        public void ServerLifecycleManagerService_OnStart_Runs_Server()
        {
            var mockServerLifeManager = new Mock<IServerLifecycleManager>();

            using (var serverLifecycleManagerServiceTest = new ServerLifecycleManagerServiceTest(mockServerLifeManager.Object))
            {
                serverLifecycleManagerServiceTest.TestStart();
                Assert.IsTrue(serverLifecycleManagerServiceTest.RunSuccessful);
                mockServerLifeManager.Verify(o => o.Run(It.IsAny<IEnumerable<IServerLifecycleWorker>>()), Times.Once);
            }
        }


        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ServerLifecycleManagerService))]
        public void ServerLifecycleManagerService_OnStop_Stops_Server()
        {
            var mockServerLifeManager = new Mock<IServerLifecycleManager>();

            using (var serverLifecycleManagerServiceTest = new ServerLifecycleManagerServiceTest(mockServerLifeManager.Object))
            {
                serverLifecycleManagerServiceTest.TestStop();

                mockServerLifeManager.Verify(o => o.Stop(false, 0, false), Times.Once);
            }
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ServerLifecycleManagerService))]
        public void ServerLifecycleManagerService_Dispose_Disposes_IServerLifecycleManager()
        {
            var mockServerLifeManager = new Mock<IServerLifecycleManager>();

            using (var serverLifecycleManagerServiceTest = new ServerLifecycleManagerServiceTest(mockServerLifeManager.Object))
            {
                serverLifecycleManagerServiceTest.TestStop();

                mockServerLifeManager.Verify(o => o.Stop(false, 0, false), Times.Once);
            }

            mockServerLifeManager.Verify(o => o.Dispose(), Times.Once);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ServerLifecycleManager))]
        public void ServerLifecycleManager_OpenCOMStream_Fails()
        {
            //------------------------Arrange------------------------
            var mockEnvironmentPreparer = new Mock<IServerEnvironmentPreparer>();
            var mockServerLifeCycleWorker = new Mock<IServerLifecycleWorker>();

            var items = new List<IServerLifecycleWorker> {mockServerLifeCycleWorker.Object};
            //------------------------Act----------------------------
            mockServerLifeCycleWorker.Setup(o => o.Execute()).Throws(new System.Exception("The system cannot find the file specified")).Verifiable();
            using (var serverLifecycleManager = new ServerLifecycleManager(mockEnvironmentPreparer.Object))
            {
                serverLifecycleManager.Run(items).Wait();
            }

            //------------------------Assert-------------------------
            mockServerLifeCycleWorker.Verify();
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ServerLifecycleManager))]
        public void ServerLifecycleManager_IsServerOnline_True()
        {
            //------------------------Arrange------------------------
            var mockEnvironmentPreparer = new Mock<IServerEnvironmentPreparer>();
            var mockIpcClient = new Mock<IIpcClient>();
            var mockAssemblyLoader = new Mock<IAssemblyLoader>();
            var mockDirectory = new Mock<IDirectory>();
            var mockResourceCatalogFactory = new Mock<IResourceCatalogFactory>();
            var mockWebServerConfiguration = new Mock<IWebServerConfiguration>();
            var mockWriter = new Mock<IWriter>();
            var mockServerLifeCycleWorker = new Mock<IServerLifecycleWorker>();
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            var mockStartWebServer = new Mock<IStartWebServer>();
            var mockSecurityIdentityFactory = new Mock<ISecurityIdentityFactory>();
            var mockLoggingServiceMonitorWithRestart = new LoggingServiceMonitorWithRestart(new Mock<ChildProcessTrackerWrapper>().Object, new Mock<ProcessWrapperFactory>().Object);
            var mockHangfireServerMonitorWithRestart = new HangfireServerMonitorWithRestart(new Mock<ChildProcessTrackerWrapper>().Object, new Mock<ProcessWrapperFactory>().Object);
            var mockWebSocketPool = new Mock<IWebSocketPool>();
            var mockWebSocketWrapper = new Mock<IWebSocketWrapper>();

            var items = new List<IServerLifecycleWorker> {mockServerLifeCycleWorker.Object};

            EnvironmentVariables.IsServerOnline = true;

            mockResourceCatalogFactory.Setup(o => o.New()).Returns(mockResourceCatalog.Object);
            mockServerLifeCycleWorker.Setup(o => o.Execute()).Verifiable();
            mockAssemblyLoader.Setup(o => o.AssemblyNames(It.IsAny<Assembly>())).Returns(new AssemblyName[] {new AssemblyName {Name = "testAssemblyName"}});
            mockWebServerConfiguration.Setup(o => o.EndPoints).Returns(new Dev2Endpoint[] {new Dev2Endpoint(new IPEndPoint(0x40E9BB63, 8080), "Url", "path")});

            mockWebSocketWrapper.Setup(o => o.IsOpen()).Returns(true);
            mockWebSocketPool.Setup(o => o.Acquire(It.IsAny<string>())).Returns(mockWebSocketWrapper.Object);

            //------------------------Act----------------------------
            var config = new StartupConfiguration
            {
                ServerEnvironmentPreparer = mockEnvironmentPreparer.Object,
                IpcClient = mockIpcClient.Object,
                AssemblyLoader = mockAssemblyLoader.Object,
                Directory = mockDirectory.Object,
                ResourceCatalogFactory = mockResourceCatalogFactory.Object,
                WebServerConfiguration = mockWebServerConfiguration.Object,
                Writer = mockWriter.Object,
                StartWebServer = mockStartWebServer.Object,
                SecurityIdentityFactory = mockSecurityIdentityFactory.Object,
                LoggingServiceMonitor = mockLoggingServiceMonitorWithRestart,
                HangfireServerMonitor = mockHangfireServerMonitorWithRestart,
                WebSocketPool = mockWebSocketPool.Object,
            };
            using (var serverLifeCycleManager = new ServerLifecycleManager(config))
            {
                serverLifeCycleManager.Run(items).Wait();
                serverLifeCycleManager.Stop(false, 0, false);
            }

            //------------------------Assert-------------------------
            mockWriter.Verify(o => o.Write("Loading security provider...  "), Times.Once);
            mockWriter.Verify(o => o.Write("Opening named pipe client stream for COM IPC... "), Times.Once);
            mockWriter.Verify(o => o.Write("Loading resource catalog...  "), Times.Once);
            mockWriter.Verify(o => o.Write("Loading server workspace...  "), Times.Once);
            mockWriter.Verify(o => o.Write("Loading resource activity cache...  "), Times.Once);
            mockWriter.Verify(o => o.Write("Loading test catalog...  "), Times.Once);
            mockWriter.Verify(o => o.Write("Loading triggers catalog...  "), Times.Once);
            mockWriter.Verify(o => o.Write("Exiting with exitcode 0"), Times.Once);
            mockServerLifeCycleWorker.Verify();
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ServerLifecycleManager))]
        [DoNotParallelize]
        public void ServerLifecycleManager_WebSocketPool_IsOpen_False()
        {
            //------------------------Arrange------------------------
            var mockEnvironmentPreparer = new Mock<IServerEnvironmentPreparer>();
            var mockIpcClient = new Mock<IIpcClient>();
            var mockAssemblyLoader = new Mock<IAssemblyLoader>();
            var mockDirectory = new Mock<IDirectory>();
            var mockResourceCatalogFactory = new Mock<IResourceCatalogFactory>();
            var mockWebServerConfiguration = new Mock<IWebServerConfiguration>();
            var mockWriter = new Mock<IWriter>();
            var mockServerLifeCycleWorker = new Mock<IServerLifecycleWorker>();
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            var mockStartWebServer = new Mock<IStartWebServer>();
            mockStartWebServer.Setup(o => o.Dispose()).Callback(() => { EnvironmentVariables.IsServerOnline = false; });
            var mockSecurityIdentityFactory = new Mock<ISecurityIdentityFactory>();
            var mockLoggingServiceMonitorWithRestart = new LoggingServiceMonitorWithRestart(new Mock<ChildProcessTrackerWrapper>().Object, new Mock<ProcessWrapperFactory>().Object);
            var mockHangfireServerMonitorWithRestart = new HangfireServerMonitorWithRestart(new Mock<ChildProcessTrackerWrapper>().Object, new Mock<ProcessWrapperFactory>().Object);
            var mockWebSocketPool = new Mock<IWebSocketPool>();
            var mockWebSocketWrapper = new Mock<IWebSocketWrapper>();

            var items = new List<IServerLifecycleWorker> {mockServerLifeCycleWorker.Object};

            EnvironmentVariables.IsServerOnline = true;
            Config.Persistence.Enable = true;
            mockResourceCatalogFactory.Setup(o => o.New()).Returns(mockResourceCatalog.Object);
            mockServerLifeCycleWorker.Setup(o => o.Execute()).Verifiable();
            mockAssemblyLoader.Setup(o => o.AssemblyNames(It.IsAny<Assembly>())).Returns(new AssemblyName[] {new AssemblyName {Name = "testAssemblyName"}});
            mockWebServerConfiguration.Setup(o => o.EndPoints).Returns(new Dev2Endpoint[] {new Dev2Endpoint(new IPEndPoint(0x40E9BB63, 8080), "Url", "path")});

            mockWebSocketWrapper.Setup(o => o.IsOpen()).Returns(false);
            mockWebSocketPool.Setup(o => o.Acquire(It.IsAny<string>())).Returns(mockWebSocketWrapper.Object);

            //------------------------Act----------------------------
            var config = new StartupConfiguration
            {
                ServerEnvironmentPreparer = mockEnvironmentPreparer.Object,
                IpcClient = mockIpcClient.Object,
                AssemblyLoader = mockAssemblyLoader.Object,
                Directory = mockDirectory.Object,
                ResourceCatalogFactory = mockResourceCatalogFactory.Object,
                WebServerConfiguration = mockWebServerConfiguration.Object,
                Writer = mockWriter.Object,
                StartWebServer = mockStartWebServer.Object,
                SecurityIdentityFactory = mockSecurityIdentityFactory.Object,
                LoggingServiceMonitor = mockLoggingServiceMonitorWithRestart,
                HangfireServerMonitor = mockHangfireServerMonitorWithRestart,
                WebSocketPool = mockWebSocketPool.Object,
            };
            using (var serverLifeCycleManager = new ServerLifecycleManager(config))
            {
                serverLifeCycleManager.Run(items).Wait();
            }

            //------------------------Assert-------------------------
            mockWriter.Verify(o => o.Write("Loading security provider...  "), Times.Once);
            mockWriter.Verify(o => o.Write("Opening named pipe client stream for COM IPC... "), Times.Once);
            mockWriter.Verify(o => o.Write("Loading resource catalog...  "), Times.Once);
            mockWriter.Verify(o => o.Write("Loading server workspace...  "), Times.Once);
            mockWriter.Verify(o => o.Write("Loading resource activity cache...  "), Times.Once);
            mockWriter.Verify(o => o.Write("Loading test catalog...  "), Times.Once);
            mockWriter.Verify(o => o.Write("Loading triggers catalog...  "), Times.Once);
            mockWriter.Verify(o => o.WriteLine("unable to connect to logging server"), Times.Once);
            mockServerLifeCycleWorker.Verify();

            Assert.IsFalse(EnvironmentVariables.IsServerOnline, "when server fails to start expect IsServerOnline to be false");
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ServerLifecycleManager))]
        public void ServerLifecycleManager_Verify_QueueProcessMonitorStart_IsServerOnline_True()
        {
            //------------------------Arrange------------------------
            var mockEnvironmentPreparer = new Mock<IServerEnvironmentPreparer>();
            var mockIpcClient = new Mock<IIpcClient>();
            var mockAssemblyLoader = new Mock<IAssemblyLoader>();
            var mockDirectory = new Mock<IDirectory>();
            var mockResourceCatalogFactory = new Mock<IResourceCatalogFactory>();
            var mockWebServerConfiguration = new Mock<IWebServerConfiguration>();
            var mockWriter = new Mock<IWriter>();
            var mockServerLifeCycleWorker = new Mock<IServerLifecycleWorker>();
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            var mockStartWebServer = new Mock<IStartWebServer>();
            var mockSecurityIdentityFactory = new Mock<ISecurityIdentityFactory>();
            var mockQueueProcessMonitor = new Mock<IProcessMonitor>();
            var mockLoggingServiceMonitorWithRestart = new LoggingServiceMonitorWithRestart(new Mock<ChildProcessTrackerWrapper>().Object, new Mock<ProcessWrapperFactory>().Object);
            var mockHangfireServerMonitorWithRestart = new HangfireServerMonitorWithRestart(new Mock<ChildProcessTrackerWrapper>().Object, new Mock<ProcessWrapperFactory>().Object);
            var mockWebSocketPool = new Mock<IWebSocketPool>();
            var mockWebSocketWrapper = new Mock<IWebSocketWrapper>();

            var items = new List<IServerLifecycleWorker> {mockServerLifeCycleWorker.Object};

            EnvironmentVariables.IsServerOnline = true;
            Config.Persistence.Enable = true;
            mockResourceCatalogFactory.Setup(o => o.New()).Returns(mockResourceCatalog.Object);
            mockServerLifeCycleWorker.Setup(o => o.Execute()).Verifiable();
            mockAssemblyLoader.Setup(o => o.AssemblyNames(It.IsAny<Assembly>())).Returns(new AssemblyName[] {new AssemblyName {Name = "testAssemblyName"}});
            mockWebServerConfiguration.Setup(o => o.EndPoints).Returns(new Dev2Endpoint[] {new Dev2Endpoint(new IPEndPoint(0x40E9BB63, 8080), "Url", "path")});

            mockWebSocketWrapper.Setup(o => o.IsOpen()).Returns(true);
            mockWebSocketPool.Setup(o => o.Acquire(It.IsAny<string>())).Returns(mockWebSocketWrapper.Object);

            //------------------------Act----------------------------
            var config = new StartupConfiguration
            {
                ServerEnvironmentPreparer = mockEnvironmentPreparer.Object,
                IpcClient = mockIpcClient.Object,
                AssemblyLoader = mockAssemblyLoader.Object,
                Directory = mockDirectory.Object,
                ResourceCatalogFactory = mockResourceCatalogFactory.Object,
                WebServerConfiguration = mockWebServerConfiguration.Object,
                Writer = mockWriter.Object,
                StartWebServer = mockStartWebServer.Object,
                SecurityIdentityFactory = mockSecurityIdentityFactory.Object,
                QueueWorkerMonitor = mockQueueProcessMonitor.Object,
                LoggingServiceMonitor = mockLoggingServiceMonitorWithRestart,
                HangfireServerMonitor = mockHangfireServerMonitorWithRestart,
                WebSocketPool = mockWebSocketPool.Object,
            };
            using (var serverLifeCycleManager = new ServerLifecycleManager(config))
            {
                serverLifeCycleManager.Run(items).Wait();
                serverLifeCycleManager.Stop(false, 0, false);
            }

            //------------------------Assert-------------------------
            mockWriter.Verify(o => o.Write("Loading security provider...  "), Times.Once);
            mockWriter.Verify(o => o.Write("Opening named pipe client stream for COM IPC... "), Times.Once);
            mockWriter.Verify(o => o.Write("Loading resource catalog...  "), Times.Once);
            mockWriter.Verify(o => o.Write("Loading server workspace...  "), Times.Once);
            mockWriter.Verify(o => o.Write("Loading resource activity cache...  "), Times.Once);
            mockWriter.Verify(o => o.Write("Loading test catalog...  "), Times.Once);
            mockWriter.Verify(o => o.Write("Loading triggers catalog...  "), Times.Once);
            mockWriter.Verify(o => o.Write("Exiting with exitcode 0"), Times.Once);

            mockQueueProcessMonitor.Verify(o => o.Start(), Times.Once);
            mockServerLifeCycleWorker.Verify();
        }


        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ServerLifecycleManager))]
        public void ServerLifecycleManager_IsServerOnline_False()
        {
            //------------------------Arrange------------------------
            var mockEnvironmentPreparer = new Mock<IServerEnvironmentPreparer>();
            var mockIpcClient = new Mock<IIpcClient>();
            var mockAssemblyLoader = new Mock<IAssemblyLoader>();
            var mockDirectory = new Mock<IDirectory>();
            var mockResourceCatalogFactory = new Mock<IResourceCatalogFactory>();
            var mockWebServerConfiguration = new Mock<IWebServerConfiguration>();
            var mockWriter = new Mock<IWriter>();
            var mockServerLifeCycleWorker = new Mock<IServerLifecycleWorker>();
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            var mockStartWebServer = new Mock<IStartWebServer>();
            var mockSecurityIdentityFactory = new Mock<ISecurityIdentityFactory>();
            var mockLoggingServiceMonitorWithRestart = new LoggingServiceMonitorWithRestart(new Mock<ChildProcessTrackerWrapper>().Object, new Mock<ProcessWrapperFactory>().Object);
            var mockHangfireServerMonitorWithRestart = new HangfireServerMonitorWithRestart(new Mock<ChildProcessTrackerWrapper>().Object, new Mock<ProcessWrapperFactory>().Object);
            var items = new List<IServerLifecycleWorker> {mockServerLifeCycleWorker.Object};

            EnvironmentVariables.IsServerOnline = false;

            mockResourceCatalogFactory.Setup(o => o.New()).Returns(mockResourceCatalog.Object);
            mockServerLifeCycleWorker.Setup(o => o.Execute()).Verifiable();
            mockAssemblyLoader.Setup(o => o.AssemblyNames(It.IsAny<Assembly>())).Returns(new AssemblyName[] {new AssemblyName {Name = "testAssemblyName"}});
            mockWebServerConfiguration.Setup(o => o.EndPoints).Returns(new Dev2Endpoint[] {new Dev2Endpoint(new IPEndPoint(0x40E9BB63, 8080), "Url", "path")});

            //------------------------Act----------------------------
            var config = new StartupConfiguration
            {
                ServerEnvironmentPreparer = mockEnvironmentPreparer.Object,
                IpcClient = mockIpcClient.Object,
                AssemblyLoader = mockAssemblyLoader.Object,
                Directory = mockDirectory.Object,
                ResourceCatalogFactory = mockResourceCatalogFactory.Object,
                WebServerConfiguration = mockWebServerConfiguration.Object,
                Writer = mockWriter.Object,
                StartWebServer = mockStartWebServer.Object,
                SecurityIdentityFactory = mockSecurityIdentityFactory.Object,
                LoggingServiceMonitor = mockLoggingServiceMonitorWithRestart,
                HangfireServerMonitor = mockHangfireServerMonitorWithRestart
            };
            using (var serverLifeCycleManager = new ServerLifecycleManager(config))
            {
                serverLifeCycleManager.Run(items).Wait();
            }

            //------------------------Assert-------------------------
            mockWriter.Verify(o => o.Write("Loading security provider...  "), Times.Once);
            mockWriter.Verify(o => o.Write("Opening named pipe client stream for COM IPC... "), Times.Once);
            mockWriter.Verify(o => o.Write("Loading resource catalog...  "), Times.Once);
            mockWriter.Verify(o => o.Write("Loading server workspace...  "), Times.Once);
            mockWriter.Verify(o => o.Write("Loading resource activity cache...  "), Times.Once);
            mockWriter.Verify(o => o.Write("Loading test catalog...  "), Times.Once);
            mockWriter.Verify(o => o.Write("Loading triggers catalog...  "), Times.Once);
            mockServerLifeCycleWorker.Verify();
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(ServerLifecycleManager))]
        public void ServerLifecycleMananger_Run_ReturnsTask()
        {
            //------------------------Arrange------------------------
            var mockEnvironmentPreparer = new Mock<IServerEnvironmentPreparer>();
            var mockIpcClient = new Mock<IIpcClient>();
            var mockAssemblyLoader = new Mock<IAssemblyLoader>();
            var mockDirectory = new Mock<IDirectory>();
            var mockResourceCatalogFactory = new Mock<IResourceCatalogFactory>();
            var mockWebServerConfiguration = new Mock<IWebServerConfiguration>();
            var mockWriter = new Mock<IWriter>();
            var mockServerLifeCycleWorker = new Mock<IServerLifecycleWorker>();
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            var mockStartWebServer = new Mock<IStartWebServer>();
            var mockSecurityIdentityFactory = new Mock<ISecurityIdentityFactory>();
            var mockLoggingServiceMonitorWithRestart = new LoggingServiceMonitorWithRestart(new Mock<ChildProcessTrackerWrapper>().Object, new Mock<ProcessWrapperFactory>().Object);

            var items = new List<IServerLifecycleWorker> {mockServerLifeCycleWorker.Object};

            EnvironmentVariables.IsServerOnline = true;

            mockResourceCatalogFactory.Setup(o => o.New()).Returns(mockResourceCatalog.Object);
            mockServerLifeCycleWorker.Setup(o => o.Execute()).Verifiable();
            mockAssemblyLoader.Setup(o => o.AssemblyNames(It.IsAny<Assembly>())).Returns(new AssemblyName[] {new AssemblyName {Name = "testAssemblyName"}});
            mockWebServerConfiguration.Setup(o => o.EndPoints).Returns(new Dev2Endpoint[] {new Dev2Endpoint(new IPEndPoint(0x40E9BB63, 8080), "Url", "path")});

            //------------------------Act----------------------------
            var config = new StartupConfiguration
            {
                ServerEnvironmentPreparer = mockEnvironmentPreparer.Object,
                IpcClient = mockIpcClient.Object,
                AssemblyLoader = mockAssemblyLoader.Object,
                Directory = mockDirectory.Object,
                ResourceCatalogFactory = mockResourceCatalogFactory.Object,
                WebServerConfiguration = mockWebServerConfiguration.Object,
                Writer = mockWriter.Object,
                StartWebServer = mockStartWebServer.Object,
                SecurityIdentityFactory = mockSecurityIdentityFactory.Object,
                LoggingServiceMonitor = mockLoggingServiceMonitorWithRestart
            };
            using (var serverLifeCycleManager = new ServerLifecycleManager(config))
            {
                var t = serverLifeCycleManager.Run(items);
                //------------------------Assert-------------------------
                Assert.IsInstanceOfType(t, typeof(Task));
            }
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ServerLifecycleManager))]
        public void ServerLifecycleManager_IsServerOnline_And_HangfireServer_Fails_ExpectFailureMessage()
        {
            //------------------------Arrange------------------------
            var mockEnvironmentPreparer = new Mock<IServerEnvironmentPreparer>();
            var mockIpcClient = new Mock<IIpcClient>();
            var mockAssemblyLoader = new Mock<IAssemblyLoader>();
            var mockDirectory = new Mock<IDirectory>();
            var mockResourceCatalogFactory = new Mock<IResourceCatalogFactory>();
            var mockWebServerConfiguration = new Mock<IWebServerConfiguration>();
            var mockWriter = new Mock<IWriter>();
            var mockServerLifeCycleWorker = new Mock<IServerLifecycleWorker>();
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            var mockStartWebServer = new Mock<IStartWebServer>();
            var mockSecurityIdentityFactory = new Mock<ISecurityIdentityFactory>();

            var mockLoggingServiceMonitorWithRestart = new LoggingServiceMonitorWithRestart(new Mock<ChildProcessTrackerWrapper>().Object, new Mock<ProcessWrapperFactory>().Object);
            var process = new ProcessThreadForTesting(new Mock<IJobConfig>().Object);
            var mockHangfireServerMonitorWithRestart = new HangfireServerMonitorWithRestartTest(new Mock<IChildProcessTracker>().Object, new Mock<IProcessFactory>().Object, process);
            
            var mockWebSocketPool = new Mock<IWebSocketPool>();
            var mockWebSocketWrapper = new Mock<IWebSocketWrapper>();

            var items = new List<IServerLifecycleWorker> { mockServerLifeCycleWorker.Object };

            EnvironmentVariables.IsServerOnline = true;

            mockResourceCatalogFactory.Setup(o => o.New()).Returns(mockResourceCatalog.Object);
            mockServerLifeCycleWorker.Setup(o => o.Execute()).Verifiable();
            mockAssemblyLoader.Setup(o => o.AssemblyNames(It.IsAny<Assembly>())).Returns(new AssemblyName[] { new AssemblyName { Name = "testAssemblyName" } });
            mockWebServerConfiguration.Setup(o => o.EndPoints).Returns(new Dev2Endpoint[] { new Dev2Endpoint(new IPEndPoint(0x40E9BB63, 8080), "Url", "path") });

            mockWebSocketWrapper.Setup(o => o.IsOpen()).Returns(true);
            mockWebSocketPool.Setup(o => o.Acquire(It.IsAny<string>())).Returns(mockWebSocketWrapper.Object);

            //------------------------Act----------------------------
            var config = new StartupConfiguration
            {
                ServerEnvironmentPreparer = mockEnvironmentPreparer.Object,
                IpcClient = mockIpcClient.Object,
                AssemblyLoader = mockAssemblyLoader.Object,
                Directory = mockDirectory.Object,
                ResourceCatalogFactory = mockResourceCatalogFactory.Object,
                WebServerConfiguration = mockWebServerConfiguration.Object,
                Writer = mockWriter.Object,
                StartWebServer = mockStartWebServer.Object,
                SecurityIdentityFactory = mockSecurityIdentityFactory.Object,
                LoggingServiceMonitor = mockLoggingServiceMonitorWithRestart,
                HangfireServerMonitor = mockHangfireServerMonitorWithRestart,
                WebSocketPool = mockWebSocketPool.Object,
            };
            using (var serverLifeCycleManager = new ServerLifecycleManager(config))
            {
                serverLifeCycleManager.Run(items).Wait();
                if (process != null)
                {
                    process.ForceProcessDiedEvent(); //kill Hangfire Server thread
                }
                serverLifeCycleManager.Stop(false, 0, false);
            }

            //------------------------Assert-------------------------
            mockWriter.Verify(o => o.Write("Loading security provider...  "), Times.Once);
            mockWriter.Verify(o => o.Write("Opening named pipe client stream for COM IPC... "), Times.Once);
            mockWriter.Verify(o => o.Write("Loading resource catalog...  "), Times.Once);
            mockWriter.Verify(o => o.Write("Loading server workspace...  "), Times.Once);
            mockWriter.Verify(o => o.Write("Loading resource activity cache...  "), Times.Once);
            mockWriter.Verify(o => o.Write("Loading test catalog...  "), Times.Once);
            mockWriter.Verify(o => o.Write("Loading triggers catalog...  "), Times.Once);
            mockWriter.Verify(o => o.Write("Exiting with exitcode 0"), Times.Once);
            mockWriter.Verify(o => o.WriteLine("hangfire server exited"), Times.Once); //we might need to use the write like the above, inverstigate.
            mockServerLifeCycleWorker.Verify();
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ServerLifecycleManager))]
        public void ServerLifecycleManager_IsServerOnline_And_HangfireServer_Success_ExpectNoFailureMessage()
        {
            //------------------------Arrange------------------------
            var mockEnvironmentPreparer = new Mock<IServerEnvironmentPreparer>();
            var mockIpcClient = new Mock<IIpcClient>();
            var mockAssemblyLoader = new Mock<IAssemblyLoader>();
            var mockDirectory = new Mock<IDirectory>();
            var mockResourceCatalogFactory = new Mock<IResourceCatalogFactory>();
            var mockWebServerConfiguration = new Mock<IWebServerConfiguration>();
            var mockWriter = new Mock<IWriter>();
            var mockServerLifeCycleWorker = new Mock<IServerLifecycleWorker>();
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            var mockStartWebServer = new Mock<IStartWebServer>();
            var mockSecurityIdentityFactory = new Mock<ISecurityIdentityFactory>();

            var mockLoggingServiceMonitorWithRestart = new LoggingServiceMonitorWithRestart(new Mock<ChildProcessTrackerWrapper>().Object, new Mock<ProcessWrapperFactory>().Object);
            var process = new ProcessThreadForTesting(new Mock<IJobConfig>().Object);
            var mockHangfireServerMonitorWithRestart = new HangfireServerMonitorWithRestartTest(new Mock<IChildProcessTracker>().Object, new Mock<IProcessFactory>().Object, process);

            var mockWebSocketPool = new Mock<IWebSocketPool>();
            var mockWebSocketWrapper = new Mock<IWebSocketWrapper>();

            var items = new List<IServerLifecycleWorker> { mockServerLifeCycleWorker.Object };

            EnvironmentVariables.IsServerOnline = true;

            mockResourceCatalogFactory.Setup(o => o.New()).Returns(mockResourceCatalog.Object);
            mockServerLifeCycleWorker.Setup(o => o.Execute()).Verifiable();
            mockAssemblyLoader.Setup(o => o.AssemblyNames(It.IsAny<Assembly>())).Returns(new AssemblyName[] { new AssemblyName { Name = "testAssemblyName" } });
            mockWebServerConfiguration.Setup(o => o.EndPoints).Returns(new Dev2Endpoint[] { new Dev2Endpoint(new IPEndPoint(0x40E9BB63, 8080), "Url", "path") });

            mockWebSocketWrapper.Setup(o => o.IsOpen()).Returns(true);
            mockWebSocketPool.Setup(o => o.Acquire(It.IsAny<string>())).Returns(mockWebSocketWrapper.Object);

            //------------------------Act----------------------------
            var config = new StartupConfiguration
            {
                ServerEnvironmentPreparer = mockEnvironmentPreparer.Object,
                IpcClient = mockIpcClient.Object,
                AssemblyLoader = mockAssemblyLoader.Object,
                Directory = mockDirectory.Object,
                ResourceCatalogFactory = mockResourceCatalogFactory.Object,
                WebServerConfiguration = mockWebServerConfiguration.Object,
                Writer = mockWriter.Object,
                StartWebServer = mockStartWebServer.Object,
                SecurityIdentityFactory = mockSecurityIdentityFactory.Object,
                LoggingServiceMonitor = mockLoggingServiceMonitorWithRestart,
                HangfireServerMonitor = mockHangfireServerMonitorWithRestart,
                WebSocketPool = mockWebSocketPool.Object,
            };
            using (var serverLifeCycleManager = new ServerLifecycleManager(config))
            {
                serverLifeCycleManager.Run(items).Wait();
                serverLifeCycleManager.Stop(false, 0, false);
            }

            //------------------------Assert-------------------------
            mockWriter.Verify(o => o.Write("Loading security provider...  "), Times.Once);
            mockWriter.Verify(o => o.Write("Opening named pipe client stream for COM IPC... "), Times.Once);
            mockWriter.Verify(o => o.Write("Loading resource catalog...  "), Times.Once);
            mockWriter.Verify(o => o.Write("Loading server workspace...  "), Times.Once);
            mockWriter.Verify(o => o.Write("Loading resource activity cache...  "), Times.Once);
            mockWriter.Verify(o => o.Write("Loading test catalog...  "), Times.Once);
            mockWriter.Verify(o => o.Write("Loading triggers catalog...  "), Times.Once);
            mockWriter.Verify(o => o.Write("Exiting with exitcode 0"), Times.Once);
            mockWriter.Verify(o => o.WriteLine("hangfire server exited"), Times.Never); //we might need to add a Hangfire Server starting... and (done) for started or Loading like the above, inverstigate.
            mockServerLifeCycleWorker.Verify();
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ServerLifecycleManager))]
        public void ServerLifecycleManager_IsServerOnline_And_LoggingServer_Fails_ExpectFailureMessage()
        {
            //------------------------Arrange------------------------
            var mockEnvironmentPreparer = new Mock<IServerEnvironmentPreparer>();
            var mockIpcClient = new Mock<IIpcClient>();
            var mockAssemblyLoader = new Mock<IAssemblyLoader>();
            var mockDirectory = new Mock<IDirectory>();
            var mockResourceCatalogFactory = new Mock<IResourceCatalogFactory>();
            var mockWebServerConfiguration = new Mock<IWebServerConfiguration>();
            var mockWriter = new Mock<IWriter>();
            var mockServerLifeCycleWorker = new Mock<IServerLifecycleWorker>();
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            var mockStartWebServer = new Mock<IStartWebServer>();
            var mockSecurityIdentityFactory = new Mock<ISecurityIdentityFactory>();

            var process = new ProcessThreadForTesting(new Mock<IJobConfig>().Object);
            var mockLoggingServiceMonitorWithRestart = new LoggingServiceMonitorWithRestartTest(new Mock<ChildProcessTrackerWrapper>().Object, new Mock<ProcessWrapperFactory>().Object, process);
            var mockHangfireServerMonitorWithRestart = new HangfireServerMonitorWithRestart(new Mock<IChildProcessTracker>().Object, new Mock<IProcessFactory>().Object);

            var mockWebSocketPool = new Mock<IWebSocketPool>();
            var mockWebSocketWrapper = new Mock<IWebSocketWrapper>();

            var items = new List<IServerLifecycleWorker> { mockServerLifeCycleWorker.Object };

            EnvironmentVariables.IsServerOnline = true;

            mockResourceCatalogFactory.Setup(o => o.New()).Returns(mockResourceCatalog.Object);
            mockServerLifeCycleWorker.Setup(o => o.Execute()).Verifiable();
            mockAssemblyLoader.Setup(o => o.AssemblyNames(It.IsAny<Assembly>())).Returns(new AssemblyName[] { new AssemblyName { Name = "testAssemblyName" } });
            mockWebServerConfiguration.Setup(o => o.EndPoints).Returns(new Dev2Endpoint[] { new Dev2Endpoint(new IPEndPoint(0x40E9BB63, 8080), "Url", "path") });

            mockWebSocketWrapper.Setup(o => o.IsOpen()).Returns(true);
            mockWebSocketPool.Setup(o => o.Acquire(It.IsAny<string>())).Returns(mockWebSocketWrapper.Object);

            //------------------------Act----------------------------
            var config = new StartupConfiguration
            {
                ServerEnvironmentPreparer = mockEnvironmentPreparer.Object,
                IpcClient = mockIpcClient.Object,
                AssemblyLoader = mockAssemblyLoader.Object,
                Directory = mockDirectory.Object,
                ResourceCatalogFactory = mockResourceCatalogFactory.Object,
                WebServerConfiguration = mockWebServerConfiguration.Object,
                Writer = mockWriter.Object,
                StartWebServer = mockStartWebServer.Object,
                SecurityIdentityFactory = mockSecurityIdentityFactory.Object,
                LoggingServiceMonitor = mockLoggingServiceMonitorWithRestart,
                HangfireServerMonitor = mockHangfireServerMonitorWithRestart,
                WebSocketPool = mockWebSocketPool.Object,
            };
            using (var serverLifeCycleManager = new ServerLifecycleManager(config))
            {
                serverLifeCycleManager.Run(items).Wait();
                process.ForceProcessDiedEvent(); //kill Logging Server thread
                serverLifeCycleManager.Stop(false, 0, false);
            }

            //------------------------Assert-------------------------
            mockWriter.Verify(o => o.Write("Loading security provider...  "), Times.Once);
            mockWriter.Verify(o => o.Write("Opening named pipe client stream for COM IPC... "), Times.Once);
            mockWriter.Verify(o => o.Write("Loading resource catalog...  "), Times.Once);
            mockWriter.Verify(o => o.Write("Loading server workspace...  "), Times.Once);
            mockWriter.Verify(o => o.Write("Loading resource activity cache...  "), Times.Once);
            mockWriter.Verify(o => o.Write("Loading test catalog...  "), Times.Once);
            mockWriter.Verify(o => o.Write("Loading triggers catalog...  "), Times.Once);
            mockWriter.Verify(o => o.Write("Exiting with exitcode 0"), Times.Once);
            mockWriter.Verify(o => o.WriteLine("logging service exited"), Times.Once); //we might need to use the write like the above, inverstigate.
            mockServerLifeCycleWorker.Verify();
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ServerLifecycleManager))]
        public void ServerLifecycleManager_IsServerOnline_And_LoggingServer_Success_ExpectNoFailureMessage()
        {
            //------------------------Arrange------------------------
            var mockEnvironmentPreparer = new Mock<IServerEnvironmentPreparer>();
            var mockIpcClient = new Mock<IIpcClient>();
            var mockAssemblyLoader = new Mock<IAssemblyLoader>();
            var mockDirectory = new Mock<IDirectory>();
            var mockResourceCatalogFactory = new Mock<IResourceCatalogFactory>();
            var mockWebServerConfiguration = new Mock<IWebServerConfiguration>();
            var mockWriter = new Mock<IWriter>();
            var mockServerLifeCycleWorker = new Mock<IServerLifecycleWorker>();
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            var mockStartWebServer = new Mock<IStartWebServer>();
            var mockSecurityIdentityFactory = new Mock<ISecurityIdentityFactory>();

            var process = new ProcessThreadForTesting(new Mock<IJobConfig>().Object);
            var mockLoggingServiceMonitorWithRestart = new LoggingServiceMonitorWithRestartTest(new Mock<ChildProcessTrackerWrapper>().Object, new Mock<ProcessWrapperFactory>().Object, process);
            var mockHangfireServerMonitorWithRestart = new HangfireServerMonitorWithRestart(new Mock<IChildProcessTracker>().Object, new Mock<IProcessFactory>().Object);

            var mockWebSocketPool = new Mock<IWebSocketPool>();
            var mockWebSocketWrapper = new Mock<IWebSocketWrapper>();

            var items = new List<IServerLifecycleWorker> { mockServerLifeCycleWorker.Object };

            EnvironmentVariables.IsServerOnline = true;

            mockResourceCatalogFactory.Setup(o => o.New()).Returns(mockResourceCatalog.Object);
            mockServerLifeCycleWorker.Setup(o => o.Execute()).Verifiable();
            mockAssemblyLoader.Setup(o => o.AssemblyNames(It.IsAny<Assembly>())).Returns(new AssemblyName[] { new AssemblyName { Name = "testAssemblyName" } });
            mockWebServerConfiguration.Setup(o => o.EndPoints).Returns(new Dev2Endpoint[] { new Dev2Endpoint(new IPEndPoint(0x40E9BB63, 8080), "Url", "path") });

            mockWebSocketWrapper.Setup(o => o.IsOpen()).Returns(true);
            mockWebSocketPool.Setup(o => o.Acquire(It.IsAny<string>())).Returns(mockWebSocketWrapper.Object);

            //------------------------Act----------------------------
            var config = new StartupConfiguration
            {
                ServerEnvironmentPreparer = mockEnvironmentPreparer.Object,
                IpcClient = mockIpcClient.Object,
                AssemblyLoader = mockAssemblyLoader.Object,
                Directory = mockDirectory.Object,
                ResourceCatalogFactory = mockResourceCatalogFactory.Object,
                WebServerConfiguration = mockWebServerConfiguration.Object,
                Writer = mockWriter.Object,
                StartWebServer = mockStartWebServer.Object,
                SecurityIdentityFactory = mockSecurityIdentityFactory.Object,
                LoggingServiceMonitor = mockLoggingServiceMonitorWithRestart,
                HangfireServerMonitor = mockHangfireServerMonitorWithRestart,
                WebSocketPool = mockWebSocketPool.Object,
            };
            using (var serverLifeCycleManager = new ServerLifecycleManager(config))
            {
                serverLifeCycleManager.Run(items).Wait();
                serverLifeCycleManager.Stop(false, 0, false);
            }

            //------------------------Assert-------------------------
            mockWriter.Verify(o => o.Write("Loading security provider...  "), Times.Once);
            mockWriter.Verify(o => o.Write("Opening named pipe client stream for COM IPC... "), Times.Once);
            mockWriter.Verify(o => o.Write("Loading resource catalog...  "), Times.Once);
            mockWriter.Verify(o => o.Write("Loading server workspace...  "), Times.Once);
            mockWriter.Verify(o => o.Write("Loading resource activity cache...  "), Times.Once);
            mockWriter.Verify(o => o.Write("Loading test catalog...  "), Times.Once);
            mockWriter.Verify(o => o.Write("Loading triggers catalog...  "), Times.Once);
            mockWriter.Verify(o => o.Write("Exiting with exitcode 0"), Times.Once);
            mockWriter.Verify(o => o.WriteLine("logging service exited"), Times.Never); //we might need to add a Hangfire Server starting... and (done) for started or Loading like the above, inverstigate.
            mockServerLifeCycleWorker.Verify();
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ServerLifecycleManager))]
        public void ServerLifecycleManager_IsServerOnline_And_QueueProcess_Success_ExpectNoFailureMessage()
        {
            //------------------------Arrange------------------------
            var mockEnvironmentPreparer = new Mock<IServerEnvironmentPreparer>();
            var mockIpcClient = new Mock<IIpcClient>();
            var mockAssemblyLoader = new Mock<IAssemblyLoader>();
            var mockDirectory = new Mock<IDirectory>();
            var mockResourceCatalogFactory = new Mock<IResourceCatalogFactory>();
            var mockWebServerConfiguration = new Mock<IWebServerConfiguration>();
            var mockWriter = new Mock<IWriter>();
            var mockServerLifeCycleWorker = new Mock<IServerLifecycleWorker>();
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            var mockStartWebServer = new Mock<IStartWebServer>();
            var mockSecurityIdentityFactory = new Mock<ISecurityIdentityFactory>();

            var mockLoggingServiceMonitorWithRestart = new LoggingServiceMonitorWithRestart(new Mock<ChildProcessTrackerWrapper>().Object, new Mock<ProcessWrapperFactory>().Object);
            var mockHangfireServerMonitorWithRestart = new HangfireServerMonitorWithRestart(new Mock<IChildProcessTracker>().Object, new Mock<IProcessFactory>().Object);
            var expectedId = Guid.NewGuid();
            var mockConfig = new Mock<IJobConfig>();
            const string expectedResourceName = "Test Resource";
            mockConfig.Setup(o => o.Name).Returns(expectedResourceName).Verifiable();
            mockConfig.Setup(o => o.Id).Returns(expectedId).Verifiable();
            var process = new ProcessThreadForTesting(mockConfig.Object);
            var mockQueueWorkerMonitorWithRestart = new QueueWorkerMonitorTest(new Mock<IProcessFactory>().Object, new Mock<IQueueConfigLoader>().Object, new Mock<ITriggersCatalog>().Object, new Mock<IChildProcessTracker>().Object, process);

            var mockWebSocketPool = new Mock<IWebSocketPool>();
            var mockWebSocketWrapper = new Mock<IWebSocketWrapper>();

            var items = new List<IServerLifecycleWorker> { mockServerLifeCycleWorker.Object };

            EnvironmentVariables.IsServerOnline = true;

            mockResourceCatalogFactory.Setup(o => o.New()).Returns(mockResourceCatalog.Object);
            mockServerLifeCycleWorker.Setup(o => o.Execute()).Verifiable();
            mockAssemblyLoader.Setup(o => o.AssemblyNames(It.IsAny<Assembly>())).Returns(new AssemblyName[] { new AssemblyName { Name = "testAssemblyName" } });
            mockWebServerConfiguration.Setup(o => o.EndPoints).Returns(new Dev2Endpoint[] { new Dev2Endpoint(new IPEndPoint(0x40E9BB63, 8080), "Url", "path") });

            mockWebSocketWrapper.Setup(o => o.IsOpen()).Returns(true);
            mockWebSocketPool.Setup(o => o.Acquire(It.IsAny<string>())).Returns(mockWebSocketWrapper.Object);

            //------------------------Act----------------------------
            var config = new StartupConfiguration
            {
                ServerEnvironmentPreparer = mockEnvironmentPreparer.Object,
                IpcClient = mockIpcClient.Object,
                AssemblyLoader = mockAssemblyLoader.Object,
                Directory = mockDirectory.Object,
                ResourceCatalogFactory = mockResourceCatalogFactory.Object,
                WebServerConfiguration = mockWebServerConfiguration.Object,
                Writer = mockWriter.Object,
                StartWebServer = mockStartWebServer.Object,
                SecurityIdentityFactory = mockSecurityIdentityFactory.Object,
                LoggingServiceMonitor = mockLoggingServiceMonitorWithRestart,
                HangfireServerMonitor = mockHangfireServerMonitorWithRestart,
                QueueWorkerMonitor = mockQueueWorkerMonitorWithRestart,
                WebSocketPool = mockWebSocketPool.Object,
            };
            using (var serverLifeCycleManager = new ServerLifecycleManager(config))
            {
                serverLifeCycleManager.Run(items).Wait();
                serverLifeCycleManager.Stop(false, 0, false);
            }

            //------------------------Assert-------------------------
            mockWriter.Verify(o => o.Write("Loading security provider...  "), Times.Once);
            mockWriter.Verify(o => o.Write("Opening named pipe client stream for COM IPC... "), Times.Once);
            mockWriter.Verify(o => o.Write("Loading resource catalog...  "), Times.Once);
            mockWriter.Verify(o => o.Write("Loading server workspace...  "), Times.Once);
            mockWriter.Verify(o => o.Write("Loading resource activity cache...  "), Times.Once);
            mockWriter.Verify(o => o.Write("Loading test catalog...  "), Times.Once);
            mockWriter.Verify(o => o.Write("Loading triggers catalog...  "), Times.Once);
            mockWriter.Verify(o => o.Write("Exiting with exitcode 0"), Times.Once);
            mockWriter.Verify(o => o.WriteLine($"queue process died: {expectedResourceName}({expectedId})"), Times.Never); //we might need to add a Hangfire Server starting... and (done) for started or Loading like the above, inverstigate.
            mockServerLifeCycleWorker.Verify();
        }

        private class HangfireServerMonitorWithRestartTest : HangfireServerMonitorWithRestart
        {
            private IProcessThread _processThread;

            public HangfireServerMonitorWithRestartTest(IChildProcessTracker childProcessTracker, IProcessFactory processFactory, IProcessThread processThread)
                :base(childProcessTracker, processFactory)
            {
                _processThread = processThread;
            }

            protected override ProcessThreadList NewThreadList(IJobConfig config)
            {
                return new HangfireServerThreadListTest(config, _processThread);
            }
        }

        private class HangfireServerThreadListTest : ProcessThreadList
        {
            IProcessThread _processThead;

            public HangfireServerThreadListTest(IJobConfig jobConfig, IProcessThread processThread)
                :base(jobConfig)
            {
                _processThead = processThread;
            }

            protected override IProcessThread GetProcessThread() => _processThead;
        }

        private class LoggingServiceMonitorWithRestartTest : LoggingServiceMonitorWithRestart
        {
            private IProcessThread _processThread;
            public LoggingServiceMonitorWithRestartTest(ChildProcessTrackerWrapper childProcessTracker, ProcessWrapperFactory processFactory, IProcessThread processThread)
                : base(childProcessTracker, processFactory)
            {
                _processThread = processThread;
            }

            protected override ProcessThreadList NewThreadList(IJobConfig config)
            {
                return new LoggingServiceThreadListTest(config, _processThread);
            }
        }

        private class LoggingServiceThreadListTest : ProcessThreadList
        {
            IProcessThread _processThead;

            public LoggingServiceThreadListTest(IJobConfig jobConfig, IProcessThread processThread)
                :base(jobConfig)
            {
                _processThead = processThread;
            }

            protected override IProcessThread GetProcessThread() => _processThead;
        }

        private class QueueWorkerMonitorTest : QueueWorkerMonitor
        {
            private IProcessThread _processThread;
            public QueueWorkerMonitorTest(IProcessFactory processFactory, IQueueConfigLoader queueConfigLoader, ITriggersCatalog triggersCatalog, IChildProcessTracker childProcessTracker, IProcessThread processThread)
                : base(processFactory, queueConfigLoader, triggersCatalog, childProcessTracker)
            {
                _processThread = processThread;
            }

            protected override ProcessThreadList NewThreadList(IJobConfig config)
            {
                return new QueueProcessThreadListTest(config, _processThread);
            }
        }

        private class QueueProcessThreadListTest : ProcessThreadList
        {
            IProcessThread _processThead;

            public QueueProcessThreadListTest(IJobConfig jobConfig, IProcessThread processThread)
                :base(jobConfig)
            {
                _processThead = processThread;
            }

            protected override IProcessThread GetProcessThread() => _processThead;
        }

        private class ProcessThreadForTesting : IProcessThread
        {
            private readonly IJobConfig _config;

            public ProcessThreadForTesting(IJobConfig config)
            {
                _config = config;
            }

            public bool IsAlive { get; }

            public event ProcessDiedEvent OnProcessDied;

            public void ForceProcessDiedEvent()
            {
                OnProcessDied(_config);
            }

            public void Kill() { }
            public void Start() { }
        }

        private class ServerLifecycleManagerServiceTest : ServerLifecycleManagerService
        {
            public ServerLifecycleManagerServiceTest(IServerLifecycleManager serverLifecycleManager)
                : base(serverLifecycleManager)
            {
            }

            public void TestStart()
            {
                OnStart(null);
            }

            public void TestStop()
            {
                OnStop();
            }
        }
    }
}