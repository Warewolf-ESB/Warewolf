using Dev2.Data.Interfaces;
using Dev2.PathOperations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests.PathOperations
{
    [TestClass]
    public class ActivityIOFactoryTests
    {
        [TestMethod]
        [Owner("Rory McGuire")]
        public void Dev2ActivityIOBroker_CreateInstance_GivenThrowsNoExpetion_ShouldBeIActivityOperationsBroker()
        {
            var broker = ActivityIOFactory.CreateOperationsBroker();
            Assert.IsInstanceOfType(broker, typeof(IActivityOperationsBroker));
        }
    }
}
