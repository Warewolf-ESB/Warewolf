/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Auditing;
using Warewolf.Interfaces.Auditing;
using Warewolf.Streams;

namespace Warewolf.Common.Framework48.Tests
{
    [TestClass]
    public class NetworkLoggerTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(NetworkLogger))]
        public void NetworkLogger_Debug_IsOpen_True()
        {
            var mockAuditEntry = new Mock<IAuditEntry>();
            var logTemplate = GlobalConstants.WarewolfLogsTemplate;

            var mockSerializer = new Mock<ISerializer>();
            mockSerializer.Setup(o => o.Serialize(It.IsAny<AuditCommand>())).Verifiable();

            var mockWebSocketPool = new Mock<IWebSocketPool>();
            var mockWebSocketWrapper = new Mock<IWebSocketWrapper>();
            mockWebSocketWrapper.Setup(o => o.SendMessage(It.IsAny<byte[]>())).Verifiable();
            mockWebSocketWrapper.Setup(o => o.IsOpen()).Returns(true);
            mockWebSocketPool.Setup(o => o.Acquire(Config.Auditing.Endpoint)).Returns(mockWebSocketWrapper.Object).Verifiable();
            mockWebSocketPool.Setup(o => o.Release(It.IsAny<IWebSocketWrapper>())).Verifiable();

            var networkLogger = new NetworkLogger(mockSerializer.Object, mockWebSocketPool.Object);
            networkLogger.Debug(logTemplate, mockAuditEntry.Object);

            mockSerializer.Verify(o => o.Serialize(It.IsAny<AuditCommand>()), Times.Once);
            mockWebSocketWrapper.Verify(o => o.SendMessage(It.IsAny<byte[]>()), Times.Once);
            mockWebSocketWrapper.Verify(o => o.IsOpen(), Times.Once);
            mockWebSocketWrapper.Verify(o => o.Connect(), Times.Never);
            mockWebSocketPool.Verify(o => o.Acquire(Config.Auditing.Endpoint), Times.Once);
            mockWebSocketPool.Verify(o => o.Release(It.IsAny<IWebSocketWrapper>()), Times.Once);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(NetworkLogger))]
        public void NetworkLogger_Debug()
        {
            var mockAuditEntry = new Mock<IAuditEntry>();
            var logTemplate = GlobalConstants.WarewolfLogsTemplate;

            var mockSerializer = new Mock<ISerializer>();
            mockSerializer.Setup(o => o.Serialize(It.IsAny<AuditCommand>())).Verifiable();

            var mockWebSocketPool = new Mock<IWebSocketPool>();
            var mockWebSocketWrapper = new Mock<IWebSocketWrapper>();
            mockWebSocketWrapper.Setup(o => o.SendMessage(It.IsAny<byte[]>())).Verifiable();
            mockWebSocketPool.Setup(o => o.Acquire(Config.Auditing.Endpoint)).Returns(mockWebSocketWrapper.Object).Verifiable();
            mockWebSocketPool.Setup(o => o.Release(It.IsAny<IWebSocketWrapper>())).Verifiable();

            var networkLogger = new NetworkLogger(mockSerializer.Object, mockWebSocketPool.Object);
            networkLogger.Debug(logTemplate, mockAuditEntry.Object);

            mockSerializer.Verify(o => o.Serialize(It.IsAny<AuditCommand>()), Times.Once);
            mockWebSocketWrapper.Verify(o => o.SendMessage(It.IsAny<byte[]>()), Times.Once);
            mockWebSocketWrapper.Verify(o => o.IsOpen(), Times.Once);
            mockWebSocketWrapper.Verify(o => o.Connect(), Times.Once);
            mockWebSocketPool.Verify(o => o.Acquire(Config.Auditing.Endpoint), Times.Once);
            mockWebSocketPool.Verify(o => o.Release(It.IsAny<IWebSocketWrapper>()), Times.Once);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(NetworkLogger))]
        public void NetworkLogger_Error()
        {
            var mockAuditEntry = new Mock<IAuditEntry>();
            var logTemplate = GlobalConstants.WarewolfLogsTemplate;

            var mockSerializer = new Mock<ISerializer>();
            mockSerializer.Setup(o => o.Serialize(It.IsAny<AuditCommand>())).Verifiable();

            var mockWebSocketPool = new Mock<IWebSocketPool>();
            var mockWebSocketWrapper = new Mock<IWebSocketWrapper>();
            mockWebSocketWrapper.Setup(o => o.SendMessage(It.IsAny<byte[]>())).Verifiable();
            mockWebSocketPool.Setup(o => o.Acquire(Config.Auditing.Endpoint)).Returns(mockWebSocketWrapper.Object).Verifiable();
            mockWebSocketPool.Setup(o => o.Release(It.IsAny<IWebSocketWrapper>())).Verifiable();

            var networkLogger = new NetworkLogger(mockSerializer.Object, mockWebSocketPool.Object);
            networkLogger.Error(logTemplate, mockAuditEntry.Object);

            mockSerializer.Verify(o => o.Serialize(It.IsAny<AuditCommand>()), Times.Once);
            mockWebSocketWrapper.Verify(o => o.SendMessage(It.IsAny<byte[]>()), Times.Once);
            mockWebSocketWrapper.Verify(o => o.IsOpen(), Times.Once);
            mockWebSocketWrapper.Verify(o => o.Connect(), Times.Once);
            mockWebSocketPool.Verify(o => o.Acquire(Config.Auditing.Endpoint), Times.Once);
            mockWebSocketPool.Verify(o => o.Release(It.IsAny<IWebSocketWrapper>()), Times.Once);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(NetworkLogger))]
        public void NetworkLogger_Fatal()
        {
            var mockAuditEntry = new Mock<IAuditEntry>();
            var logTemplate = GlobalConstants.WarewolfLogsTemplate;

            var mockSerializer = new Mock<ISerializer>();
            mockSerializer.Setup(o => o.Serialize(It.IsAny<AuditCommand>())).Verifiable();

            var mockWebSocketPool = new Mock<IWebSocketPool>();
            var mockWebSocketWrapper = new Mock<IWebSocketWrapper>();
            mockWebSocketWrapper.Setup(o => o.SendMessage(It.IsAny<byte[]>())).Verifiable();
            mockWebSocketPool.Setup(o => o.Acquire(Config.Auditing.Endpoint)).Returns(mockWebSocketWrapper.Object).Verifiable();
            mockWebSocketPool.Setup(o => o.Release(It.IsAny<IWebSocketWrapper>())).Verifiable();

            var networkLogger = new NetworkLogger(mockSerializer.Object, mockWebSocketPool.Object);
            networkLogger.Fatal(logTemplate, mockAuditEntry.Object);

            mockSerializer.Verify(o => o.Serialize(It.IsAny<AuditCommand>()), Times.Once);
            mockWebSocketWrapper.Verify(o => o.SendMessage(It.IsAny<byte[]>()), Times.Once);
            mockWebSocketWrapper.Verify(o => o.IsOpen(), Times.Once);
            mockWebSocketWrapper.Verify(o => o.Connect(), Times.Once);
            mockWebSocketPool.Verify(o => o.Acquire(Config.Auditing.Endpoint), Times.Once);
            mockWebSocketPool.Verify(o => o.Release(It.IsAny<IWebSocketWrapper>()), Times.Once);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(NetworkLogger))]
        public void NetworkLogger_Info()
        {
            var mockAuditEntry = new Mock<IAuditEntry>();
            var logTemplate = GlobalConstants.WarewolfLogsTemplate;

            var mockSerializer = new Mock<ISerializer>();
            mockSerializer.Setup(o => o.Serialize(It.IsAny<AuditCommand>())).Verifiable();

            var mockWebSocketPool = new Mock<IWebSocketPool>();
            var mockWebSocketWrapper = new Mock<IWebSocketWrapper>();
            mockWebSocketWrapper.Setup(o => o.SendMessage(It.IsAny<byte[]>())).Verifiable();
            mockWebSocketPool.Setup(o => o.Acquire(Config.Auditing.Endpoint)).Returns(mockWebSocketWrapper.Object).Verifiable();
            mockWebSocketPool.Setup(o => o.Release(It.IsAny<IWebSocketWrapper>())).Verifiable();

            var networkLogger = new NetworkLogger(mockSerializer.Object, mockWebSocketPool.Object);
            networkLogger.Info(logTemplate, mockAuditEntry.Object);

            mockSerializer.Verify(o => o.Serialize(It.IsAny<AuditCommand>()), Times.Once);
            mockWebSocketWrapper.Verify(o => o.SendMessage(It.IsAny<byte[]>()), Times.Once);
            mockWebSocketWrapper.Verify(o => o.IsOpen(), Times.Once);
            mockWebSocketWrapper.Verify(o => o.Connect(), Times.Once);
            mockWebSocketPool.Verify(o => o.Acquire(Config.Auditing.Endpoint), Times.Once);
            mockWebSocketPool.Verify(o => o.Release(It.IsAny<IWebSocketWrapper>()), Times.Once);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(NetworkLogger))]
        public void NetworkLogger_Warn()
        {
            var mockAuditEntry = new Mock<IAuditEntry>();
            var logTemplate = GlobalConstants.WarewolfLogsTemplate;

            var mockSerializer = new Mock<ISerializer>();
            mockSerializer.Setup(o => o.Serialize(It.IsAny<AuditCommand>())).Verifiable();

            var mockWebSocketPool = new Mock<IWebSocketPool>();
            var mockWebSocketWrapper = new Mock<IWebSocketWrapper>();
            mockWebSocketWrapper.Setup(o => o.SendMessage(It.IsAny<byte[]>())).Verifiable();
            mockWebSocketPool.Setup(o => o.Acquire(Config.Auditing.Endpoint)).Returns(mockWebSocketWrapper.Object).Verifiable();
            mockWebSocketPool.Setup(o => o.Release(It.IsAny<IWebSocketWrapper>())).Verifiable();

            var networkLogger = new NetworkLogger(mockSerializer.Object, mockWebSocketPool.Object);
            networkLogger.Warn(logTemplate, mockAuditEntry.Object);

            mockSerializer.Verify(o => o.Serialize(It.IsAny<AuditCommand>()), Times.Once);
            mockWebSocketWrapper.Verify(o => o.SendMessage(It.IsAny<byte[]>()), Times.Once);
            mockWebSocketWrapper.Verify(o => o.IsOpen(), Times.Once);
            mockWebSocketWrapper.Verify(o => o.Connect(), Times.Once);
            mockWebSocketPool.Verify(o => o.Acquire(Config.Auditing.Endpoint), Times.Once);
            mockWebSocketPool.Verify(o => o.Release(It.IsAny<IWebSocketWrapper>()), Times.Once);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(NetworkLogger))]
        public void NetworkLogger_Publish()
        {
            var mockSerializer = new Mock<ISerializer>();

            var mockWebSocketPool = new Mock<IWebSocketPool>();
            var mockWebSocketWrapper = new Mock<IWebSocketWrapper>();
            mockWebSocketWrapper.Setup(o => o.SendMessage(It.IsAny<byte[]>())).Verifiable();
            mockWebSocketPool.Setup(o => o.Acquire(Config.Auditing.Endpoint)).Returns(mockWebSocketWrapper.Object).Verifiable();
            mockWebSocketPool.Setup(o => o.Release(It.IsAny<IWebSocketWrapper>())).Verifiable();

            var networkLogger = new NetworkLogger(mockSerializer.Object, mockWebSocketPool.Object);
            networkLogger.Publish(new byte[]{});

            mockWebSocketWrapper.Verify(o => o.SendMessage(It.IsAny<byte[]>()), Times.Once);
            mockWebSocketPool.Verify(o => o.Acquire(Config.Auditing.Endpoint), Times.Once);
            mockWebSocketPool.Verify(o => o.Release(It.IsAny<IWebSocketWrapper>()), Times.Once);
        }
    }
}
