using System;
using System.Diagnostics.CodeAnalysis;
using Dev2.Composition;
using Dev2.Studio.Core;
using Dev2.Studio.Core.InterfaceImplementors;
using Dev2.Studio.Core.Interfaces;
using Dev2.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests.Environments
{
    // BUG 9276 : TWR : 2013.04.19 - refactored so that we share environments

    [TestClass]    
    public class ServerProviderTest
    {

        #region Initialization/Cleanup

        [ClassInitialize]
        public static void MyClassInitialize(TestContext testContext)
        {
            EnviromentRepositoryTest.MyClassInitialize(testContext);
        }

        [TestInitialize]
        public void MyTestInitialize()
        {
            ImportService.CurrentContext = EnviromentRepositoryTest.EnviromentRepositoryImportServiceContext;
            AppSettings.LocalHost = "http://localhost:3142";
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

            Assert.AreSame(servers[0], targetEnv.Object);
            Assert.AreEqual(servers[0].ID, targetEnv.Object.ID);
            Assert.AreEqual(servers[0].Name, targetEnv.Object.Name);
            // remove the last two properties from mock ;)
        }

        #endregion

    }
}
