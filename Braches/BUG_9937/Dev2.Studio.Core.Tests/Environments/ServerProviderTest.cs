using System;
using System.Threading;
using Dev2.Composition;
using Dev2.Core.Tests.Environments;
using Dev2.Studio.Core;
using Dev2.Studio.Core.InterfaceImplementors;
using Dev2.Studio.Core.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests
{
    // BUG 9276 : TWR : 2013.04.19 - refactored so that we share environments

    [TestClass]
    public class ServerProviderTest
    {
        readonly object _testGuard = new object();

        #region Initialization/Cleanup

        [ClassInitialize]
        public static void MyClassInitialize(TestContext testContext)
        {
            EnviromentRepositoryTest.MyClassInitialize(testContext);
        }

        [TestInitialize]
        public void MyTestInitialize()
        {
            Monitor.Enter(_testGuard);
            ImportService.CurrentContext = EnviromentRepositoryTest.EnviromentRepositoryImportServiceContext;
        }

        [TestCleanup]
        public void MyTestCleanup()
        {
            Monitor.Exit(_testGuard);
        }

        #endregion

        #region Instance

        [TestMethod]
        public void ServerProviderInstanceExpectedReturnsNonNull()
        {
            var provider = ServerProvider.Instance;
            Assert.IsNotNull(provider);
        }

        #endregion

        #region Load

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ServerProviderLoadWithNullEnvironmentRepositoryExpectedThrowsArgumentNullException()
        {
            var provider = new TestServerProvider();
            provider.Load(null);
        }

        [TestMethod]
        public void ServerProviderLoadWithEnvironmentRepositoryExpectedAllServersInEnvironmentRepository()
        {
            TestLoad(false);
        }

        [TestMethod]
        public void ServerProviderLoadWithNoParametersExpectedExpectedAllServersInEnvironmentRepository()
        {
            TestLoad(true);
        }

        #endregion

        #region TestLoad

        static void TestLoad(bool useParameterless)
        {
            var targetEnv = EnviromentRepositoryTest.CreateMockEnvironment(EnviromentRepositoryTest.Server1Source);
            var repository = new Mock<IEnvironmentRepository>();
            repository.Setup(r => r.All()).Returns(new[] { targetEnv.Object });

            if(useParameterless)
            {
                EnvironmentRepository.Instance.Clear();
                EnvironmentRepository.Instance.Save(targetEnv.Object);
                EnvironmentRepository.Instance.IsLoaded = true;  // so that we don't connect to a server!
            }

            var provider = new TestServerProvider();
            var servers = useParameterless ? provider.Load() : provider.Load(repository.Object);

            Assert.AreEqual(1, servers.Count);

            Assert.AreSame(servers[0].Environment, targetEnv.Object);
            Assert.AreEqual(servers[0].ID, targetEnv.Object.ID.ToString());
            Assert.AreEqual(servers[0].Alias, targetEnv.Object.Name);
            Assert.AreEqual(servers[0].AppAddress, targetEnv.Object.Connection.AppServerUri.AbsoluteUri);
            Assert.AreEqual(servers[0].WebAddress, targetEnv.Object.Connection.WebServerUri.AbsoluteUri);
        }

        #endregion

    }
}
