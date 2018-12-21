using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Server.Tests
{
    [TestClass]
    public class ServerlifeCycleManagerTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ServerlifeCycleManagerTests))]
        public void ServerLifecycleMananger_Starts()
        {
            var mockEnvironmentPreparer = new Mock<IServerEnvironmentPreparer>();
            using (var serverLifecycleManager = new ServerLifecycleManager(mockEnvironmentPreparer.Object))
            {
            }
        }

        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory(nameof(ServerlifeCycleManagerTests))]
        public void ServerLifecycleMananger_PreparesEnvironment()
        {
            var mockEnvironmentPreparer = new Mock<IServerEnvironmentPreparer>();
            using (var serverLifecycleManager = new ServerLifecycleManager(mockEnvironmentPreparer.Object))
            {
            }

            mockEnvironmentPreparer.Verify(o => o.PrepareEnvironment(), Times.Once);
        }
    }
}
