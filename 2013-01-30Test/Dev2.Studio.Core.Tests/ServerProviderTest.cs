using Dev2.Composition;
using Dev2.Studio.Core.InterfaceImplementors;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Dev2.Core.Tests
{
    [TestClass]
    public class ServerProviderTest
    {
        private static ImportServiceContext _importServiceContext;

        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            _importServiceContext = CompositionInitializer.InitializeMockedMainViewModel();
        }

        [TestInitialize()]
        public void EnvironmentRepositoryTestsInitialize()
        {
            ImportService.CurrentContext = _importServiceContext;
        }

        #region Load

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Load_WithNullEnvironmentRepository_Expected_ThrowsArgumentNullException()
        {
            ServerProvider.Load(null);
        }

        [TestMethod]
        public void Load_WithAddTargetEnvironmentTrue_Expected_ServersFromEnvironmentRepositoryWithTargetEnvironmentIncluded()
        {
            var targetEnv = new EnviromentRepositoryTest().CreateMockEnvironment(EnviromentRepositoryTest.Server1Source);

            var servers = ServerProvider.Load(targetEnv.Object);
            Assert.AreEqual(2, servers.Count);
        }

        [TestMethod]
        public void Load_WithAddTargetEnvironmentFalse_Expected_ServersFromEnvironmentRepositoryWithTargetEnvironmentExcluded()
        {
            var targetEnv = new EnviromentRepositoryTest().CreateMockEnvironment(EnviromentRepositoryTest.Server1Source);
            var servers = ServerProvider.Load(targetEnv.Object, false);
            Assert.AreEqual(1, servers.Count);
        }
        #endregion Tests
    }
}
