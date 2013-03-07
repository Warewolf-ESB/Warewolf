using System;
using Dev2.Network.Messaging.Messages;
using Dev2.Runtime.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Runtime.Configuration
{
    [TestClass]
    public class SettingsProviderTests
    {
        #region Instance

        [TestMethod]
        public void InstanceExpectedReturnsSingletonInstance()
        {
            var provider1 = SettingsProvider.Instance;
            var provider2 = SettingsProvider.Instance;
            Assert.AreSame(provider1, provider2);
        }

        [TestMethod]
        public void InstanceExpectedIsNotNull()
        {
            var provider = SettingsProvider.Instance;
            Assert.IsNotNull(provider);
        }

        #endregion

        #region CTOR

        [TestMethod]
        public void ConstructorExpectedInitializesProperties()
        {
            var provider = new SettingsProvider();
            Assert.IsNotNull(provider.Backup);
            Assert.IsNotNull(provider.Logging);
            Assert.IsNotNull(provider.Security);
        }

        #endregion

        #region ProcessMessage

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ProcessMessageWithNullArgumentsExpectedThrowsArgumentNullException()
        {
            var provider = new SettingsProvider();
            provider.ProcessMessage(null);
        }


        [TestMethod]
        public void ProcessMessageWithValidArgumentsExpectedDoesNotReturnNull()
        {
            var request = new Mock<ISettingsMessage>();
            var provider = new SettingsProvider();
            var result = provider.ProcessMessage(request.Object);
            Assert.IsNotNull(result);
        }

        #endregion
    }
}
