/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

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
using WarewolfCOMIPC.Client;

namespace Dev2.Server.Tests
{
    [TestClass]
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
            using (var serverLifecycleManagerServiceTest = new ServerLifecycleManagerServiceTest(mockServerLifeManager.Object))
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

                mockServerLifeManager.Verify(o => o.Stop(false, 0), Times.Once);
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

                mockServerLifeManager.Verify(o => o.Stop(false, 0), Times.Once);
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
            var mockSerLifeCycleWorker = new Mock<IServerLifecycleWorker>();

            var items = new List<IServerLifecycleWorker> { mockSerLifeCycleWorker.Object };
            //------------------------Act----------------------------
            mockSerLifeCycleWorker.Setup(o => o.Execute()).Throws(new System.Exception("The system cannot find the file specified")).Verifiable();
            using (var serverLifeCylcleManager = new ServerLifecycleManager(mockEnvironmentPreparer.Object))
            {
                serverLifeCylcleManager.Run(items).Wait();
            }
            //------------------------Assert-------------------------
            mockSerLifeCycleWorker.Verify();
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
            var mockSerLifeCycleWorker = new Mock<IServerLifecycleWorker>();
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            var mockStartWebServer = new Mock<IStartWebServer>();
            var mockSecurityIdentityFactory = new Mock<ISecurityIdentityFactory>();
            var mockLoggingServiceMonitorWithRestart = new LoggingServiceMonitorWithRestart(new Mock<ChildProcessTrackerWrapper>().Object, new Mock<ProcessWrapperFactory>().Object);
            var mockWebSocketPool = new Mock<IWebSocketPool>();
            var mockWebSocketWrapper = new Mock<IWebSocketWrapper>();

            var items = new List<IServerLifecycleWorker> { mockSerLifeCycleWorker.Object };

            EnvironmentVariables.IsServerOnline = true;

            mockResourceCatalogFactory.Setup(o => o.New()).Returns(mockResourceCatalog.Object);
            mockSerLifeCycleWorker.Setup(o => o.Execute()).Verifiable();
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
                WebSocketPool = mockWebSocketPool.Object,
            };
            using (var serverLifeCycleManager = new ServerLifecycleManager(config))
            {
                serverLifeCycleManager.Run(items).Wait();
                serverLifeCycleManager.Stop(false, 0);
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
            mockSerLifeCycleWorker.Verify();
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ServerLifecycleManager))]
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
            var mockSerLifeCycleWorker = new Mock<IServerLifecycleWorker>();
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            var mockStartWebServer = new Mock<IStartWebServer>();
            var mockSecurityIdentityFactory = new Mock<ISecurityIdentityFactory>();
            var mockLoggingServiceMonitorWithRestart = new LoggingServiceMonitorWithRestart(new Mock<ChildProcessTrackerWrapper>().Object, new Mock<ProcessWrapperFactory>().Object);
            var mockWebSocketPool = new Mock<IWebSocketPool>();
            var mockWebSocketWrapper = new Mock<IWebSocketWrapper>();

            var items = new List<IServerLifecycleWorker> { mockSerLifeCycleWorker.Object };

            EnvironmentVariables.IsServerOnline = true;

            mockResourceCatalogFactory.Setup(o => o.New()).Returns(mockResourceCatalog.Object);
            mockSerLifeCycleWorker.Setup(o => o.Execute()).Verifiable();
            mockAssemblyLoader.Setup(o => o.AssemblyNames(It.IsAny<Assembly>())).Returns(new AssemblyName[] { new AssemblyName { Name = "testAssemblyName" } });
            mockWebServerConfiguration.Setup(o => o.EndPoints).Returns(new Dev2Endpoint[] { new Dev2Endpoint(new IPEndPoint(0x40E9BB63, 8080), "Url", "path") });

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
            mockWriter.Verify(o => o.Write("Exiting with exitcode 0"), Times.Once);
            mockSerLifeCycleWorker.Verify();
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
            var mockSerLifeCycleWorker = new Mock<IServerLifecycleWorker>();
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            var mockStartWebServer = new Mock<IStartWebServer>();
            var mockSecurityIdentityFactory = new Mock<ISecurityIdentityFactory>();
            var mockQueueProcessMonitor = new Mock<IProcessMonitor>();
            var mockLoggingServiceMonitorWithRestart = new LoggingServiceMonitorWithRestart(new Mock<ChildProcessTrackerWrapper>().Object, new Mock<ProcessWrapperFactory>().Object);
            var mockWebSocketPool = new Mock<IWebSocketPool>();
            var mockWebSocketWrapper = new Mock<IWebSocketWrapper>();

            var items = new List<IServerLifecycleWorker> { mockSerLifeCycleWorker.Object };

            EnvironmentVariables.IsServerOnline = true;

            mockResourceCatalogFactory.Setup(o => o.New()).Returns(mockResourceCatalog.Object);
            mockSerLifeCycleWorker.Setup(o => o.Execute()).Verifiable();
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
                QueueWorkerMonitor = mockQueueProcessMonitor.Object,
                LoggingServiceMonitor = mockLoggingServiceMonitorWithRestart,
                WebSocketPool = mockWebSocketPool.Object,
            };
            using (var serverLifeCycleManager = new ServerLifecycleManager(config))
            {
                serverLifeCycleManager.Run(items).Wait();
                serverLifeCycleManager.Stop(false, 0);
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
            mockSerLifeCycleWorker.Verify();
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
            var mockSerLifeCycleWorker = new Mock<IServerLifecycleWorker>();
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            var mockStartWebServer = new Mock<IStartWebServer>();
            var mockSecurityIdentityFactory = new Mock<ISecurityIdentityFactory>();
            var mockLoggingServiceMonitorWithRestart = new LoggingServiceMonitorWithRestart(new Mock<ChildProcessTrackerWrapper>().Object, new Mock<ProcessWrapperFactory>().Object);

            var items = new List<IServerLifecycleWorker> { mockSerLifeCycleWorker.Object };

            EnvironmentVariables.IsServerOnline = false;

            mockResourceCatalogFactory.Setup(o => o.New()).Returns(mockResourceCatalog.Object);
            mockSerLifeCycleWorker.Setup(o => o.Execute()).Verifiable();
            mockAssemblyLoader.Setup(o => o.AssemblyNames(It.IsAny<Assembly>())).Returns(new AssemblyName[] { new AssemblyName { Name = "testAssemblyName" } });
            mockWebServerConfiguration.Setup(o => o.EndPoints).Returns(new Dev2Endpoint[] { new Dev2Endpoint(new IPEndPoint(0x40E9BB63, 8080), "Url", "path") });

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
            mockSerLifeCycleWorker.Verify();
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
            var mockSerLifeCycleWorker = new Mock<IServerLifecycleWorker>();
            var mockResourceCatalog = new Mock<IResourceCatalog>();
            var mockStartWebServer = new Mock<IStartWebServer>();
            var mockSecurityIdentityFactory = new Mock<ISecurityIdentityFactory>();
            var mockLoggingServiceMonitorWithRestart = new LoggingServiceMonitorWithRestart(new Mock<ChildProcessTrackerWrapper>().Object, new Mock<ProcessWrapperFactory>().Object);

            var items = new List<IServerLifecycleWorker> { mockSerLifeCycleWorker.Object };

            EnvironmentVariables.IsServerOnline = true;

            mockResourceCatalogFactory.Setup(o => o.New()).Returns(mockResourceCatalog.Object);
            mockSerLifeCycleWorker.Setup(o => o.Execute()).Verifiable();
            mockAssemblyLoader.Setup(o => o.AssemblyNames(It.IsAny<Assembly>())).Returns(new AssemblyName[] { new AssemblyName { Name = "testAssemblyName" } });
            mockWebServerConfiguration.Setup(o => o.EndPoints).Returns(new Dev2Endpoint[] { new Dev2Endpoint(new IPEndPoint(0x40E9BB63, 8080), "Url", "path") });

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


        class ServerLifecycleManagerServiceTest : ServerLifecycleManagerService
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
