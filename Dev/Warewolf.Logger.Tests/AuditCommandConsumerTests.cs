/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Text;
using Fleck;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Auditing;
using Warewolf.Interfaces.Auditing;
using Warewolf.Logging;
using Warewolf.Streams;
using Warewolf.Triggers;
using LogLevel = Warewolf.Logging.LogLevel;

namespace Warewolf.Logger.Tests
{
    [TestClass]
    public class AuditCommandConsumerTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(AuditCommandConsumer))]
        [DoNotParallelize]
        public void AuditCommandConsumer_Consume_LogEntry()
        {
            var workflowId = Guid.NewGuid().ToString();
            var audit = new Audit
            {
                WorkflowID = workflowId,
            };

            object parameters = new object();

            var auditCommand = new AuditCommand
            {
                Type = "LogEntry",
                Audit = audit,
            };

            var mockLoggerConsumer = new Mock<ILoggerConsumer<IAuditEntry>>();
            mockLoggerConsumer.Setup(o => o.Consume(auditCommand.Audit, parameters)).Verifiable();
            var mockSocket = new Mock<IWebSocketConnection>();
            var mockWriter = new Mock<IWriter>();
            mockWriter.Setup(o => o.WriteLine("Logging Server OnMessage: Type:" + auditCommand.Type)).Verifiable();

            var auditCommandConsumer = new AuditCommandConsumer(mockLoggerConsumer.Object, mockSocket.Object, mockWriter.Object);

            auditCommandConsumer.Consume(auditCommand, parameters);

            mockLoggerConsumer.Verify(o => o.Consume(auditCommand.Audit, parameters), Times.Once);
            mockWriter.Verify(o => o.WriteLine("Logging Server OnMessage: Type:" + auditCommand.Type), Times.Once);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(AuditCommandConsumer))]
        [DoNotParallelize]
        public void AuditCommandConsumer_Consume_ResumeExecution()
        {
            var workflowId = Guid.NewGuid().ToString();
            var audit = new Audit
            {
                WorkflowID = workflowId,
            };

            object parameters = new object();

            var auditCommand = new AuditCommand
            {
                Type = "LogResumeExecutionState",
                Audit = audit,
            };

            var mockLoggerConsumer = new Mock<ILoggerConsumer<IAuditEntry>>();
            mockLoggerConsumer.Setup(o => o.Consume(auditCommand.Audit, parameters)).Verifiable();
            var mockSocket = new Mock<IWebSocketConnection>();
            var mockWriter = new Mock<IWriter>();
            mockWriter.Setup(o => o.WriteLine("Logging Server OnMessage: Type:" + auditCommand.Type)).Verifiable();
            mockWriter.Setup(o => o.WriteLine("Resuming workflow: " + auditCommand.Audit.WorkflowID)).Verifiable();

            var auditCommandConsumer = new AuditCommandConsumer(mockLoggerConsumer.Object, mockSocket.Object, mockWriter.Object);

            auditCommandConsumer.Consume(auditCommand, parameters);

            mockLoggerConsumer.Verify(o => o.Consume(auditCommand.Audit, parameters), Times.Once);
            mockWriter.Verify(o => o.WriteLine("Logging Server OnMessage: Type:" + auditCommand.Type), Times.Once);
            mockWriter.Verify(o => o.WriteLine("Resuming workflow: " + auditCommand.Audit.WorkflowID), Times.Once);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(AuditCommandConsumer))]
        public void AuditCommandConsumer_Consume_LogQuery()
        {
            object parameters = new object();

            var auditCommand = new AuditCommand
            {
                Type = "LogQuery",
                Query = new Dictionary<string, StringBuilder>(),
            };

            byte[] expected = new byte[]{};

            var mockLoggerConsumer = new Mock<ILoggerConsumer<IAuditEntry>>();
            var mockSocket = new Mock<IWebSocketConnection>();
            mockSocket.Setup(o => o.Send(expected)).Verifiable();

            IEnumerable<IAudit> audits = new List<IAudit>();

            var mockWriter = new Mock<IWriter>();
            mockWriter.Setup(o => o.WriteLine("Logging Server OnMessage: Type:" + auditCommand.Type)).Verifiable();
            mockWriter.Setup(o => o.WriteLine("Executing query: " + auditCommand.Query)).Verifiable();
            mockWriter.Setup(o => o.WriteLine("sending QueryLog to server: " + audits + "...")).Verifiable();

            var mockSerializer = new Mock<ISerializer>();
            mockSerializer.Setup(o => o.Serialize(audits)).Returns(expected);
            mockSerializer.Setup(o => o.Serialize(audits)).Verifiable();

            var mockAuditQueryable = new Mock<IAuditQueryable>();
            mockAuditQueryable.Setup(o => o.QueryLogData(auditCommand.Query)).Returns(audits);

            var auditCommandConsumer = new AuditCommandConsumer(mockLoggerConsumer.Object, mockSocket.Object, mockWriter.Object, mockSerializer.Object, mockAuditQueryable.Object);

            auditCommandConsumer.Consume(auditCommand, parameters);

            mockWriter.Verify(o => o.WriteLine("Logging Server OnMessage: Type:" + auditCommand.Type), Times.Once);
            mockWriter.Verify(o => o.WriteLine("Executing query: " + auditCommand.Query), Times.Once);
            mockWriter.Verify(o => o.WriteLine("sending QueryLog to server: " + audits + "..."), Times.Once);

            mockSocket.Verify(o => o.Send(expected), Times.Once);
            mockSerializer.Verify(o => o.Serialize(audits), Times.Once);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(AuditCommandConsumer))]
        public void AuditCommandConsumer_Consume_TriggerQuery()
        {
            object parameters = new object();

            var auditCommand = new AuditCommand
            {
                Type = "TriggerQuery",
                Query = new Dictionary<string, StringBuilder>(),
            };

            var mockLoggerConsumer = new Mock<ILoggerConsumer<IAuditEntry>>();
            var mockSocket = new Mock<IWebSocketConnection>();
            mockSocket.Setup(o => o.Send(It.IsAny<string>())).Verifiable();

            IEnumerable<IExecutionHistory> histories = new List<IExecutionHistory>();

            var mockWriter = new Mock<IWriter>();
            mockWriter.Setup(o => o.WriteLine("Logging Server OnMessage: Type:" + auditCommand.Type)).Verifiable();
            mockWriter.Setup(o => o.WriteLine("Executing TriggerQuery: " + auditCommand.Query)).Verifiable();
            mockWriter.Setup(o => o.WriteLine("sending QueryTriggerLog to server: " + histories + "...")).Verifiable();

            byte[] expected = new byte[]{};
            var mockSerializer = new Mock<ISerializer>();
            mockSerializer.Setup(o => o.Serialize(histories)).Returns(expected);
            mockSerializer.Setup(o => o.Serialize(histories)).Verifiable();

            var mockAuditQueryable = new Mock<IAuditQueryable>();
            mockAuditQueryable.Setup(o => o.QueryTriggerData(auditCommand.Query)).Returns(histories);

            var auditCommandConsumer = new AuditCommandConsumer(mockLoggerConsumer.Object, mockSocket.Object, mockWriter.Object, mockSerializer.Object, mockAuditQueryable.Object);

            auditCommandConsumer.Consume(auditCommand, parameters);

            mockWriter.Verify(o => o.WriteLine("Logging Server OnMessage: Type:" + auditCommand.Type), Times.Once);
            mockWriter.Verify(o => o.WriteLine("Executing TriggerQuery: " + auditCommand.Query), Times.Once);
            mockWriter.Verify(o => o.WriteLine("sending QueryTriggerLog to server: " + histories + "..."), Times.Once);

            mockSocket.Verify(o => o.Send(expected), Times.Once);
            mockSerializer.Verify(o => o.Serialize(histories), Times.Once);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(AuditCommandConsumer))]
        [DoNotParallelize]
        public void AuditCommandConsumer_Consume_LogEntryCommand()
        {
            const string outputTemplate = "";
            var logEntry = new LogEntry(LogLevel.Info, outputTemplate, null);

            object parameters = new object();

            var auditCommand = new AuditCommand
            {
                Type = "LogEntryCommand",
                LogEntry = logEntry,
            };

            var mockLoggerConsumer = new Mock<ILoggerConsumer<IAuditEntry>>();
            mockLoggerConsumer.Setup(o => o.Consume(auditCommand.LogEntry, parameters)).Verifiable();
            var mockSocket = new Mock<IWebSocketConnection>();
            var mockWriter = new Mock<IWriter>();
            mockWriter.Setup(o => o.WriteLine("Logging Server OnMessage: Type:" + auditCommand.Type)).Verifiable();
            mockWriter.Setup(o => o.WriteLine(auditCommand.LogEntry.OutputTemplate)).Verifiable();

            var auditCommandConsumer = new AuditCommandConsumer(mockLoggerConsumer.Object, mockSocket.Object, mockWriter.Object);

            auditCommandConsumer.Consume(auditCommand, parameters);

            mockLoggerConsumer.Verify(o => o.Consume(auditCommand.LogEntry, parameters), Times.Once);
            mockWriter.Verify(o => o.WriteLine("Logging Server OnMessage: Type:" + auditCommand.Type), Times.Once);
            mockWriter.Verify(o => o.WriteLine(auditCommand.LogEntry.OutputTemplate), Times.Once);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(AuditCommandConsumer))]
        [DoNotParallelize]
        public void AuditCommandConsumer_Consume_ExecutionAuditCommand()
        {
            var resourceId = Guid.NewGuid();
            var executionHistory = new ExecutionHistory
            {
                ResourceId = resourceId,
            };

            object parameters = new object();

            var auditCommand = new AuditCommand
            {
                Type = "ExecutionAuditCommand",
                ExecutionHistory = executionHistory,
            };

            var mockLoggerConsumer = new Mock<ILoggerConsumer<IAuditEntry>>();
            mockLoggerConsumer.Setup(o => o.Consume(auditCommand.ExecutionHistory, parameters)).Verifiable();
            var mockSocket = new Mock<IWebSocketConnection>();
            var mockWriter = new Mock<IWriter>();
            mockWriter.Setup(o => o.WriteLine("Logging Server OnMessage: Type:" + auditCommand.Type)).Verifiable();
            mockWriter.Setup(o => o.WriteLine(auditCommand.ExecutionHistory.ResourceId.ToString())).Verifiable();

            var auditCommandConsumer = new AuditCommandConsumer(mockLoggerConsumer.Object, mockSocket.Object, mockWriter.Object);

            auditCommandConsumer.Consume(auditCommand, parameters);

            mockLoggerConsumer.Verify(o => o.Consume(auditCommand.ExecutionHistory, parameters), Times.Once);
            mockWriter.Verify(o => o.WriteLine("Logging Server OnMessage: Type:" + auditCommand.Type), Times.Once);
            mockWriter.Verify(o => o.WriteLine(auditCommand.ExecutionHistory.ResourceId.ToString()), Times.Once);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(AuditCommandConsumer))]
        [DoNotParallelize]
        public void AuditCommandConsumer_Consume_Default()
        {
            object parameters = new object();
            var auditCommand = new AuditCommand
            {
                Type = "",
            };

            var mockLoggerConsumer = new Mock<ILoggerConsumer<IAuditEntry>>();
            var mockSocket = new Mock<IWebSocketConnection>();
            var mockWriter = new Mock<IWriter>();
            mockWriter.Setup(o => o.WriteLine("Logging Server OnMessage: Type:" + auditCommand.Type)).Verifiable();
            mockWriter.Setup(o => o.WriteLine("Logging Server Invalid Message Type")).Verifiable();

            var auditCommandConsumer = new AuditCommandConsumer(mockLoggerConsumer.Object, mockSocket.Object, mockWriter.Object);

            auditCommandConsumer.Consume(auditCommand, parameters);

            mockWriter.Verify(o => o.WriteLine("Logging Server OnMessage: Type:" + auditCommand.Type), Times.Once);
            mockWriter.Verify(o => o.WriteLine("Logging Server Invalid Message Type"), Times.Once);
        }
    }
}
