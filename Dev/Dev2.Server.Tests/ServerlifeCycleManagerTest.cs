using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Server.Tests
{
    [TestClass]
    public class ServerTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory("ServerLifecycleMananger")]
        public void ServerLifecycleMananger_Starts()
        {
            var serverLifecycleManager = new ServerLifecycleManager();
            
        }
    }
}
