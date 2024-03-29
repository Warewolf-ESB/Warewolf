/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Studio.Core;
using Dev2.Studio.Core.InterfaceImplementors;
using Dev2.Studio.Interfaces;
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
            EnvironmentRepositoryTest.MyClassInitialize(testContext);
        }

        [TestInitialize]
        public void MyTestInitialize()
        {
            AppUsageStats.LocalHost = "http://localhost:3142";
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
            var targetEnv = EnvironmentRepositoryTest.CreateMockEnvironment(EnvironmentRepositoryTest.Server1Source);
            var repository = new Mock<IServerRepository>();
            repository.Setup(r => r.All()).Returns(new[] { targetEnv.Object });
            CustomContainer.DeRegister<IServerRepository>();
            CustomContainer.Register(repository.Object);
            if (useParameterless)
            {
                
                ServerRepository.Instance.IsLoaded = true;  // so that we don't connect to a server!
                ServerRepository.Instance.Clear();
                ServerRepository.Instance.Save(targetEnv.Object);
            }

            var provider = new TestServerProvider();
            var servers = useParameterless ? provider.Load() : provider.Load(repository.Object);

            Assert.AreEqual(1, servers.Count);

            Assert.AreSame(servers[0], targetEnv.Object);
            Assert.AreEqual(servers[0].EnvironmentID, targetEnv.Object.EnvironmentID);
            Assert.AreEqual(servers[0].Name, targetEnv.Object.Name);
            // remove the last two properties from mock ;)
        }

        #endregion

    }
}
