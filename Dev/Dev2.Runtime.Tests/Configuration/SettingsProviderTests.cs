
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


//using System;
//using System.Diagnostics.CodeAnalysis;
//using System.IO;
//using System.Threading;
//using System.Xml.Linq;
//using Dev2.Common.Common;
//using Dev2.Network.Messaging.Messages;
//using Dev2.Runtime.Configuration;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System.Diagnostics.CodeAnalysis;
//
//namespace Dev2.Tests.Runtime.Configuration
//{
//    [TestClass]
//    [ExcludeFromCodeCoverage]
//    public class SettingsProviderTests
//    {
//        static readonly object WriterLock = new object();
//
//        [TestInitialize]
//        public void TestInitialize()
//        {
//            Monitor.Enter(WriterLock);
//            SettingsProvider.WebServerUri = "localhost";
//        }
//
//        [TestCleanup]
//        public void TestCleanup()
//        {
//            var filePath = SettingsProvider.GetFilePath();
//            var dir = Path.GetDirectoryName(filePath);
//            if(dir != null && Directory.Exists(dir))
//            {
//                DirectoryHelper.CleanUp(dir);
//            }
//            Monitor.Exit(WriterLock);
//        }
//
//        #region Instance
//
//        [TestMethod]
//        public void InstanceExpectedReturnsSingletonInstance()
//        {
//            SettingsProvider.WebServerUri = "localhost";
//            var provider1 = SettingsProvider.Instance;
//            var provider2 = SettingsProvider.Instance;
//            Assert.AreSame(provider1, provider2);
//        }
//
//        [TestMethod]
//        public void InstanceExpectedIsNotNull()
//        {
//            SettingsProvider.WebServerUri = "localhost";
//            var provider = SettingsProvider.Instance;
//            Assert.IsNotNull(provider);
//        }
//
//        [TestMethod]
//        public void InstanceReturnsSameAssemblyHashCodeEveryTime()
//        {
//            var provider1 = new SettingsProvider();
//            var provider2 = new SettingsProvider();
//
//            Assert.AreEqual(provider1.AssemblyHashCode, provider2.AssemblyHashCode);
//        }
//
//        #endregion
//
//        #region CTOR
//
//        [TestMethod]
//        public void ConstructorExpectedInitializesAllProperties()
//        {
//            var provider = new SettingsProvider();
//
//            Assert.IsNotNull(provider.Configuration);
//            Assert.IsNotNull(provider.AssemblyHashCode);
//            Assert.AreNotEqual(string.Empty, provider.AssemblyHashCode);
//        }
//
//        #endregion
//
//        #region ProcessMessage
//
//        [TestMethod]
//        [ExpectedException(typeof(ArgumentNullException))]
//        public void ProcessMessageWithNullArgumentsExpectedThrowsArgumentNullException()
//        {
//            SettingsProvider.WebServerUri = "localhost";
//            var provider = new SettingsProvider();
//            provider.ProcessMessage(null);
//        }
//
//
//        [TestMethod]
//        public void ProcessMessageWithValidArgumentsExpectedDoesNotReturnNull()
//        {
//            SettingsProvider.WebServerUri = "localhost";
//            //var request = new Mock<ISettingsMessage>();
//            var request = new SettingsMessage();
//            var provider = new SettingsProvider();
//            //var result = provider.ProcessMessage(request.Object);
//            var result = provider.ProcessMessage(request);
//            Assert.IsNotNull(result);
//        }
//
//        #endregion
//
//        #region ProcessRead
//
//        [TestMethod]
//        public void ProcessReadWithInvalidAssemblyHashCodeExpectedReturnsVersionConflictAndAllProperties()
//        {
//            //var request = new Mock<ISettingsMessage>();
//            //request.SetupGet(m => m.Action).Returns(NetworkMessageAction.Read);
//            //request.SetupProperty(m => m.Result);
//            //request.SetupProperty(m => m.ConfigurationXml);
//            //request.SetupProperty(m => m.Assembly);
//            //request.SetupProperty(m => m.AssemblyHashCode);
//
//            //request.Object.AssemblyHashCode = "xxx";
//
//            var provider = new SettingsProvider();
//
//            var request = new SettingsMessage
//            {
//                AssemblyHashCode = "xxx",
//                Action = NetworkMessageAction.Read
//            };
//
//            //var result = provider.ProcessMessage(request.Object);
//            var result = provider.ProcessMessage(request);
//            Assert.AreEqual(NetworkMessageResult.VersionConflict, result.Result);
//            Assert.IsNotNull(result.ConfigurationXml);
//            Assert.IsNotNull(result.Assembly);
//            Assert.AreEqual(provider.AssemblyHashCode, result.AssemblyHashCode);
//
//        }
//
//        [TestMethod]
//        public void ProcessReadWithValidAssemblyHashCodeExpectedReturnsSuccessAndConfigurationXmlOnly()
//        {
//            //var request = new Mock<ISettingsMessage>();
//            //request.SetupGet(m => m.Action).Returns(NetworkMessageAction.Read);
//            //request.SetupProperty(m => m.Result);
//            //request.SetupProperty(m => m.ConfigurationXml);
//            //request.SetupProperty(m => m.Assembly);
//            //request.SetupProperty(m => m.AssemblyHashCode);
//            SettingsProvider.WebServerUri = "localhost";
//            var provider = new SettingsProvider();
//
//            var request = new SettingsMessage
//            {
//                AssemblyHashCode = provider.AssemblyHashCode,
//                Action = NetworkMessageAction.Read
//            };
//            //request.Object.AssemblyHashCode = provider.AssemblyHashCode;
//
//            //var result = provider.ProcessMessage(request.Object);
//            var result = provider.ProcessMessage(request);
//            Assert.AreEqual(NetworkMessageResult.Success, result.Result);
//            Assert.IsNotNull(result.ConfigurationXml);
//            Assert.IsNull(result.Assembly);
//            Assert.AreEqual(provider.AssemblyHashCode, result.AssemblyHashCode);
//        }
//
//        #endregion
//
//        #region ProcessWrite
//
//        [TestMethod]
//        public void ProcessWriteWithOverwriteExpectedSavesConfigurationAndReturnsSuccess()
//        {
//            var configNew = new Dev2.Runtime.Configuration.Settings.Configuration("localhost");
//
//            //var request = new Mock<ISettingsMessage>();
//            //request.SetupGet(m => m.Action).Returns(NetworkMessageAction.Overwrite);
//            //request.SetupGet(m => m.ConfigurationXml).Returns(configNew.ToXml());
//            //request.SetupProperty(m => m.Result);
//
//            var request = new SettingsMessage
//            {
//                ConfigurationXml = configNew.ToXml(),
//                Action = NetworkMessageAction.Overwrite
//            };
//
//            var provider = new SettingsProvider();
//            var filePath = SettingsProvider.GetFilePath();
//
//            //var result = provider.ProcessMessage(request.Object);
//            var result = provider.ProcessMessage(request);
//            Assert.AreEqual(NetworkMessageResult.Success, result.Result);
//            Assert.IsTrue(File.Exists(filePath));
//
//            configNew.IncrementVersion();
//            var expected = configNew.ToXml().ToString(SaveOptions.DisableFormatting);
//            var actual = provider.Configuration.ToXml().ToString(SaveOptions.DisableFormatting);
//            Assert.AreEqual(expected, actual);
//            Assert.AreEqual(configNew.Version, provider.Configuration.Version);
//        }
//
//        [TestMethod]
//        public void ProcessWriteWithWriteAndNoFileOnDiskExpectedSavesConfigurationAndReturnsSuccess()
//        {
//            var configNew = new Dev2.Runtime.Configuration.Settings.Configuration("localhost");
//
//            //var request = new Mock<ISettingsMessage>();
//            //request.SetupGet(m => m.Action).Returns(NetworkMessageAction.Write);
//            //request.SetupGet(m => m.ConfigurationXml).Returns(configNew.ToXml());
//            //request.SetupProperty(m => m.Result);
//
//            var request = new SettingsMessage
//            {
//                ConfigurationXml = configNew.ToXml(),
//                Action = NetworkMessageAction.Write
//            };
//
//            var provider = new SettingsProvider();
//            var filePath = SettingsProvider.GetFilePath();
//
//            //var result = provider.ProcessMessage(request.Object);
//            var result = provider.ProcessMessage(request);
//            Assert.AreEqual(NetworkMessageResult.Success, result.Result);
//            Assert.IsTrue(File.Exists(filePath));
//
//            configNew.IncrementVersion();
//            var expected = configNew.ToXml().ToString(SaveOptions.DisableFormatting);
//            var actual = provider.Configuration.ToXml().ToString(SaveOptions.DisableFormatting);
//            Assert.AreEqual(expected, actual);
//            Assert.AreEqual(configNew.Version, provider.Configuration.Version);
//        }
//
//        [TestMethod]
//        public void ProcessWriteWithWriteAndOlderVersionOnDiskExpectedSavesConfigurationAndReturnsSuccess()
//        {
//            var configNew = new Dev2.Runtime.Configuration.Settings.Configuration("localhost") { Version = new Version(1, 2) };
//            var configOld = new Dev2.Runtime.Configuration.Settings.Configuration("localhost") { Version = new Version(1, 0) };
//
//            //var request = new Mock<ISettingsMessage>();
//            //request.SetupGet(m => m.Action).Returns(NetworkMessageAction.Write);
//            //request.SetupGet(m => m.ConfigurationXml).Returns(configNew.ToXml());
//            //request.SetupProperty(m => m.Result);
//
//            var request = new SettingsMessage
//            {
//                ConfigurationXml = configNew.ToXml(),
//                Action = NetworkMessageAction.Write
//            };
//
//            var provider = new SettingsProvider();
//            var filePath = SettingsProvider.GetFilePath();
//
//            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
//            var xmlOld = configOld.ToXml();
//            xmlOld.Save(filePath);
//
//            //var result = provider.ProcessMessage(request.Object);
//            var result = provider.ProcessMessage(request);
//            Assert.AreEqual(NetworkMessageResult.Success, result.Result);
//            Assert.IsTrue(File.Exists(filePath));
//
//            configNew.IncrementVersion();
//            var expected = configNew.ToXml().ToString(SaveOptions.DisableFormatting);
//            var actual = provider.Configuration.ToXml().ToString(SaveOptions.DisableFormatting);
//            Assert.AreEqual(expected, actual);
//            Assert.AreEqual(configNew.Version, provider.Configuration.Version);
//        }
//
//        [TestMethod]
//        public void ProcessWriteWithWriteAndSameVersionOnDiskExpectedSavesConfigurationAndReturnsSuccess()
//        {
//            var configNew = new Dev2.Runtime.Configuration.Settings.Configuration("localhost") { Version = new Version(1, 2) };
//            var configOld = new Dev2.Runtime.Configuration.Settings.Configuration("localhost") { Version = new Version(1, 2) };
//
//            //var request = new Mock<ISettingsMessage>();
//            //request.SetupGet(m => m.Action).Returns(NetworkMessageAction.Write);
//            //request.SetupGet(m => m.ConfigurationXml).Returns(configNew.ToXml());
//            //request.SetupProperty(m => m.Result);
//
//            var request = new SettingsMessage
//            {
//                ConfigurationXml = configNew.ToXml(),
//                Action = NetworkMessageAction.Write
//            };
//
//            var provider = new SettingsProvider();
//            var filePath = SettingsProvider.GetFilePath();
//
//            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
//            var xmlOld = configOld.ToXml();
//            xmlOld.Save(filePath);
//
//            //var result = provider.ProcessMessage(request.Object);
//            var result = provider.ProcessMessage(request);
//            Assert.AreEqual(NetworkMessageResult.Success, result.Result);
//            Assert.IsTrue(File.Exists(filePath));
//
//            configNew.IncrementVersion();
//            var expected = configNew.ToXml().ToString(SaveOptions.DisableFormatting);
//            var actual = provider.Configuration.ToXml().ToString(SaveOptions.DisableFormatting);
//            Assert.AreEqual(expected, actual);
//            Assert.AreEqual(configNew.Version, provider.Configuration.Version);
//        }
//
//
//        [TestMethod]
//        public void ProcessWriteWithWriteAndNewerVersionOnDiskExpectedDoesNotSaveConfigurationAndReturnsVersionConflict()
//        {
//            var configNew = new Dev2.Runtime.Configuration.Settings.Configuration("localhost") { Version = new Version(1, 0) };
//            var configOld = new Dev2.Runtime.Configuration.Settings.Configuration("localhost") { Version = new Version(1, 2) };
//
//            //var request = new Mock<ISettingsMessage>();
//            //request.SetupGet(m => m.Action).Returns(NetworkMessageAction.Write);
//            //request.SetupGet(m => m.ConfigurationXml).Returns(configNew.ToXml());
//            //request.SetupProperty(m => m.Result);
//
//            var request = new SettingsMessage
//            {
//                ConfigurationXml = configNew.ToXml(),
//                Action = NetworkMessageAction.Write
//            };
//
//            var provider = new SettingsProvider();
//            var filePath = SettingsProvider.GetFilePath();
//            var expectedVersion = provider.Configuration.Version;
//            var expectedXml = provider.Configuration.ToXml().ToString(SaveOptions.DisableFormatting);
//
//            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
//            var xmlOld = configOld.ToXml();
//            xmlOld.Save(filePath);
//
//            //var result = provider.ProcessMessage(request.Object);
//            var result = provider.ProcessMessage(request);
//
//            Assert.AreEqual(NetworkMessageResult.VersionConflict, result.Result);
//            var diskXml = XElement.Load(filePath);
//
//            Assert.AreEqual(xmlOld.ToString(SaveOptions.DisableFormatting), diskXml.ToString(SaveOptions.DisableFormatting));
//
//            var actualVersion = provider.Configuration.Version;
//            var actualXml = provider.Configuration.ToXml().ToString(SaveOptions.DisableFormatting);
//            Assert.AreEqual(expectedXml, actualXml);
//            Assert.AreEqual(expectedVersion, actualVersion);
//        }
//
//        #endregion
//
//
//    }
//}
