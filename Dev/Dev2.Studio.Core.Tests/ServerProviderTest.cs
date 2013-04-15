using System;
using Dev2.Studio.Core.InterfaceImplementors;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests
{
    [TestClass]
    public class ServerProviderTest
    {
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
            var targetEnv = EnviromentRepositoryTest.CreateMockEnvironment(EnviromentRepositoryTest.Server1Source);

            var servers = ServerProvider.Load(targetEnv.Object);
            Assert.AreEqual(2, servers.Count);
        }

        [TestMethod]
        public void Load_WithAddTargetEnvironmentFalse_Expected_ServersFromEnvironmentRepositoryWithTargetEnvironmentExcluded()
        {
            var targetEnv = EnviromentRepositoryTest.CreateMockEnvironment(EnviromentRepositoryTest.Server1Source);
            var servers = ServerProvider.Load(targetEnv.Object, false);
            Assert.AreEqual(1, servers.Count);
        }
        #endregion Tests
    }
}
