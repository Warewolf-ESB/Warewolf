/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Auditing;
using Warewolf.Interfaces.Auditing;
using Warewolf.Streams;
using Warewolf.Triggers;

namespace Warewolf.Common.NetStandard20.Tests
{
    [TestClass]
    public class ExecutionLoggerTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ExecutionLogger))]
        public void ExecutionLogger_LogExecutionCompleted_ExecutionSucceeded()
        {
            var mockExecutionHistory = new Mock<IExecutionHistory>();

            var mockSerializer = new Mock<ISerializer>();
            mockSerializer.Setup(o => o.Serialize(It.IsAny<AuditCommand>())).Verifiable();

            var mockWebSocketPool = new Mock<IWebSocketPool>();
            var mockWebSocketWrapper = new Mock<IWebSocketWrapper>();
            mockWebSocketWrapper.Setup(o => o.SendMessage(It.IsAny<byte[]>())).Verifiable();
            mockWebSocketPool.Setup(o => o.Acquire(Dev2.Common.Config.Auditing.Endpoint)).Returns(mockWebSocketWrapper.Object).Verifiable();
            mockWebSocketPool.Setup(o => o.Release(It.IsAny<IWebSocketWrapper>())).Verifiable();

            var executionLogger = new ExecutionLogger(mockSerializer.Object, mockWebSocketPool.Object);
            executionLogger.ExecutionSucceeded(mockExecutionHistory.Object);

            mockSerializer.Verify(o => o.Serialize(It.IsAny<AuditCommand>()), Times.Once);
            mockWebSocketWrapper.Verify(o => o.SendMessage(It.IsAny<byte[]>()), Times.Once);
            mockWebSocketPool.Verify(o => o.Acquire(Dev2.Common.Config.Auditing.Endpoint), Times.Once);
            mockWebSocketPool.Verify(o => o.Release(It.IsAny<IWebSocketWrapper>()), Times.Once);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ExecutionLogger))]
        public void ExecutionLogger_LogExecutionCompleted_ExecutionFailed()
        {
            var mockExecutionHistory = new Mock<IExecutionHistory>();

            var mockSerializer = new Mock<ISerializer>();
            mockSerializer.Setup(o => o.Serialize(It.IsAny<AuditCommand>())).Verifiable();

            var mockWebSocketPool = new Mock<IWebSocketPool>();
            var mockWebSocketWrapper = new Mock<IWebSocketWrapper>();
            mockWebSocketWrapper.Setup(o => o.SendMessage(It.IsAny<byte[]>())).Verifiable();
            mockWebSocketPool.Setup(o => o.Acquire(Dev2.Common.Config.Auditing.Endpoint)).Returns(mockWebSocketWrapper.Object).Verifiable();
            mockWebSocketPool.Setup(o => o.Release(It.IsAny<IWebSocketWrapper>())).Verifiable();

            var executionLogger = new ExecutionLogger(mockSerializer.Object, mockWebSocketPool.Object);
            executionLogger.ExecutionFailed(mockExecutionHistory.Object);

            mockSerializer.Verify(o => o.Serialize(It.IsAny<AuditCommand>()), Times.Once);
            mockWebSocketWrapper.Verify(o => o.SendMessage(It.IsAny<byte[]>()), Times.Once);
            mockWebSocketPool.Verify(o => o.Acquire(Dev2.Common.Config.Auditing.Endpoint), Times.Once);
            mockWebSocketPool.Verify(o => o.Release(It.IsAny<IWebSocketWrapper>()), Times.Once);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ExecutionLogger))]
        public void ExecutionLogger_LogResumedExecution()
        {
            var mockAudit = new Mock<IAudit>();

            var mockSerializer = new Mock<ISerializer>();
            mockSerializer.Setup(o => o.Serialize(It.IsAny<AuditCommand>())).Verifiable();

            var mockWebSocketPool = new Mock<IWebSocketPool>();
            var mockWebSocketWrapper = new Mock<IWebSocketWrapper>();
            mockWebSocketWrapper.Setup(o => o.SendMessage(It.IsAny<byte[]>())).Verifiable();
            mockWebSocketPool.Setup(o => o.Acquire(Dev2.Common.Config.Auditing.Endpoint)).Returns(mockWebSocketWrapper.Object).Verifiable();
            mockWebSocketPool.Setup(o => o.Release(It.IsAny<IWebSocketWrapper>())).Verifiable();

            var executionLogger = new ExecutionLogger(mockSerializer.Object, mockWebSocketPool.Object);
            executionLogger.LogResumedExecution(mockAudit.Object);

            mockSerializer.Verify(o => o.Serialize(It.IsAny<AuditCommand>()), Times.Once);
            mockWebSocketWrapper.Verify(o => o.SendMessage(It.IsAny<byte[]>()), Times.Once);
            mockWebSocketPool.Verify(o => o.Acquire(Dev2.Common.Config.Auditing.Endpoint), Times.Once);
            mockWebSocketPool.Verify(o => o.Release(It.IsAny<IWebSocketWrapper>()), Times.Once);
        }
    }
}
