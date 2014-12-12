using System;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Studio.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class EnvironmentViewModelTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("EnvironmentViewModel_Constructor")]
        public void EnvironmentViewModel_Constructor_ServerPassedIn_ShouldSetServerProperty()
        {
            //------------Setup for test--------------------------
            var server = new Mock<IServer>();
            
            //------------Execute Test---------------------------
            IEnvironmentViewModel environmentViewModel = new EnvironmentViewModel(server.Object);
            //------------Assert Results-------------------------
            Assert.IsNotNull(environmentViewModel);
            Assert.IsNotNull(environmentViewModel.Server);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("EnvironmentViewModel_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void EnvironmentViewModel_Constructor_NullServer_ArgumentNullException()
        {
            //------------Setup for test--------------------------
            
            
            //------------Execute Test---------------------------
            new EnvironmentViewModel(null);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("EnvironmentViewModel_Connect")]
        public void EnvironmentViewModel_Connect_ShouldCallConnectOnEnvironment()
        {
            //------------Setup for test--------------------------
            var server = new Mock<IServer>();
            server.Setup(server1 => server1.Connect()).Returns(false);
            var environmentViewModel = new EnvironmentViewModel(server.Object);
            
            //------------Execute Test---------------------------
            environmentViewModel.Connect();
            //------------Assert Results-------------------------
            server.Verify();
            Assert.IsFalse(environmentViewModel.IsConnected);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("EnvironmentViewModel_Connect")]
        public void EnvironmentViewModel_Connect_WhenServerDoesNotConnect_ShouldNotBeConnected()
        {
            //------------Setup for test--------------------------
            var server = new Mock<IServer>();
            server.Setup(server1 => server1.Connect()).Returns(true);
            var environmentViewModel = new EnvironmentViewModel(server.Object);
            
            //------------Execute Test---------------------------
            environmentViewModel.Connect();
            //------------Assert Results-------------------------
            server.Verify();
            Assert.IsTrue(environmentViewModel.IsConnected);
        }
    }
}
