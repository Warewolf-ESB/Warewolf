using Dev2.Common.Interfaces.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;

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
