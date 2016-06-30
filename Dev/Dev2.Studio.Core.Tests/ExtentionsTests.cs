using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
// ReSharper disable InconsistentNaming

namespace Dev2.Core.Tests
{
    [TestClass]
    public class ExtentionsTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void IsEnvironmentConnected_GivenIsConnected_ShouldReturnTrue()
        {
            //---------------Set up test pack-------------------
            var environment = new Mock<IEnvironmentModel>();
            environment.Setup(model => model.IsConnected).Returns(true);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            // ReSharper disable once InvokeAsExtensionMethod
            var isEnvironmentConnected = Extentions.IsEnvironmentConnected(environment.Object);
            //---------------Test Result -----------------------
            Assert.IsTrue(isEnvironmentConnected);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void IsEnvironmentConnected_GivenIsNotConnected_ShouldReturnFalse()
        {
            //---------------Set up test pack-------------------
            var environment = new Mock<IEnvironmentModel>();
            environment.Setup(model => model.IsConnected).Returns(false);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            // ReSharper disable once InvokeAsExtensionMethod
            var isEnvironmentConnected = Extentions.IsEnvironmentConnected(environment.Object);
            //---------------Test Result -----------------------
            Assert.IsFalse(isEnvironmentConnected);
        }
    }
}
