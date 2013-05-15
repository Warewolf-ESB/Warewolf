using Dev2.Composition;
using Dev2.Diagnostics;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.ViewModels.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace Dev2.Core.Tests
{
    [TestClass]
    public class DebugStateTreeViewItemViewModelTests
    {
        static ImportServiceContext _importContext;

        [ClassInitialize()]
        public static void Initialize(TestContext testContext)
        {
            _importContext = CompositionInitializer.DefaultInitialize();
        }

        // BUG 8373: TWR
        [TestMethod]
        public void Constructor_With_EnvironmentRepository_Expected_SetsDebugStateServer()
        {
            var serverID = Guid.NewGuid();
            const string ServerName = "Myserver";

            var env = new Mock<IEnvironmentModel>();
            env.Setup(e => e.ID).Returns(serverID);
            env.Setup(e => e.Name).Returns(ServerName);

            var env2 = new Mock<IEnvironmentModel>();
            env2.Setup(e => e.ID).Returns(Guid.NewGuid());

            var envRep = new Mock<IEnvironmentRepository>();
            envRep.Setup(e => e.All()).Returns(() => new[] { env.Object, env2.Object });

            var content = new DebugState { ServerID = serverID };
            var vm = new DebugStateTreeViewItemViewModel(envRep.Object, content);
            Assert.AreEqual(ServerName, content.Server);
        }
    }
}
