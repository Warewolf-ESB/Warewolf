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

        [TestMethod]
        public void InstanceReturnsSameAssemblyHashCodeEveryTime()
        {
            var provider1 = new SettingsProvider();
            var provider2 = new SettingsProvider();

            Assert.AreEqual(provider1.AssemblyHashCode, provider2.AssemblyHashCode);
        }

        #endregion

        #region CTOR

        [TestMethod]
        public void ConstructorExpectedInitializesAllProperties()
        {
            var provider = new SettingsProvider();

            Assert.IsNotNull(provider.Backup);
            Assert.IsNotNull(provider.Logging);
            Assert.IsNotNull(provider.Security);

            Assert.IsNotNull(provider.AssemblyHashCode);
            Assert.AreNotEqual(string.Empty, provider.AssemblyHashCode);
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

        #region ProcessRead

        [TestMethod]
        public void ProcessReadWithInvalidAssemblyHashCodeExpectedReturnsVersionConflictAndAllProperties()
        {
            var request = new Mock<ISettingsMessage>();
            request.SetupGet(m => m.Action).Returns(NetworkMessageAction.Read);
            request.SetupProperty(m => m.Result);
            request.SetupProperty(m => m.ConfigurationXml);
            request.SetupProperty(m => m.Assembly);
            request.SetupProperty(m => m.AssemblyHashCode);

            request.Object.AssemblyHashCode = "xxx";

            var provider = new SettingsProvider();

            var result = provider.ProcessMessage(request.Object);
            Assert.AreEqual(NetworkMessageResult.VersionConflict, result.Result);
            Assert.IsNotNull(result.ConfigurationXml);
            Assert.IsNotNull(result.Assembly);
            Assert.AreEqual(provider.AssemblyHashCode, result.AssemblyHashCode);

        }

        [TestMethod]
        public void ProcessReadWithValidAssemblyHashCodeExpectedReturnsSuccessAndConfigurationXmlOnly()
        {
            var request = new Mock<ISettingsMessage>();
            request.SetupGet(m => m.Action).Returns(NetworkMessageAction.Read);
            request.SetupProperty(m => m.Result);
            request.SetupProperty(m => m.ConfigurationXml);
            request.SetupProperty(m => m.Assembly);
            request.SetupProperty(m => m.AssemblyHashCode);

            var provider = new SettingsProvider();

            request.Object.AssemblyHashCode = provider.AssemblyHashCode;

            var result = provider.ProcessMessage(request.Object);
            Assert.AreEqual(NetworkMessageResult.Success, result.Result);
            Assert.IsNotNull(result.ConfigurationXml);
            Assert.IsNull(result.Assembly);
            Assert.AreEqual(provider.AssemblyHashCode, result.AssemblyHashCode);
        }

        #endregion
    }
}
