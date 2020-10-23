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
using Fleck;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Auditing;
using Warewolf.Interfaces.Auditing;
using Warewolf.Logging;

namespace Warewolf.Logger.Tests
{
    [TestClass]
    public class AuditCommandConsumerTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(AuditCommandConsumer))]
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
                Type = "ResumeExecution",
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
    }
}