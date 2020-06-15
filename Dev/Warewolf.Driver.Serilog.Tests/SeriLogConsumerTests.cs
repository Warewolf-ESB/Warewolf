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
using Serilog;
using System;
using Warewolf.Data;
using Warewolf.Driver.Serilog;
using Warewolf.Interfaces.Auditing;
using Warewolf.Logging;
using ILogger = Serilog.ILogger;

namespace Warewolf.Driver.Serilog.Tests
{
    [TestClass]
    public class SeriLogConsumerTests
    {

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(SeriLogConsumer))]
        public void SeriLogConsumer_Consume_Success()
        {
            //------------------------------Arrange-----------------------------
            var audit = new AuditStub();
            audit.LogLevel = LogLevel.Info;
            audit.CustomTransactionID = "testing SeriLogConsumer";
            var message = audit;
            var mockLogger = new Mock<ILogger>();
            var mockLoggerPublisher = new Mock<ILoggerPublisher>();
            var mockLoggerContext = new Mock<ILoggerContext>();
            var mockLoggerSource = new Mock<ILoggerSource>();
            var mockLoggerConnection = new Mock<ILoggerConnection>();
            var mockLoggerConfig = new Mock<ILoggerConfig>();

            mockLoggerPublisher.Setup(p => p.Info(It.IsAny<string>(), It.IsAny<object[]>())).Verifiable();
            mockLoggerConnection.Setup(l => l.NewPublisher()).Returns(mockLoggerPublisher.Object);
            mockLoggerSource.Setup(s => s.NewConnection(It.IsAny<ILoggerConfig>())).Returns(mockLoggerConnection.Object);
            mockLoggerContext.Setup(c => c.Source).Returns(mockLoggerSource.Object);
            mockLoggerContext.Setup(c => c.LoggerConfig).Returns(mockLoggerConfig.Object);


            var seriLogConsumer = new SeriLogConsumer(mockLoggerContext.Object);

            //------------------------------Act---------------------------------
            var response = seriLogConsumer.Consume(message, null);
            //------------------------------Assert------------------------------
            mockLoggerPublisher.Verify(p => p.Info(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
            Assert.AreEqual(expected: ConsumerResult.Success, actual: response.Result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(SeriLogConsumer))]
        public void SeriLogConsumer_Consume_InfoMessage_ShouldCall_Information()
        {
            //------------------------------Arrange-----------------------------
            var audit = new AuditStub();
            audit.LogLevel = LogLevel.Info;
            audit.CustomTransactionID = "testing SeriLogConsumer";
            var message = audit;
            var mockLogger = new Mock<ILogger>();
            var mockLoggerPublisher = new Mock<ILoggerPublisher>();
            var mockLoggerContext = new Mock<ILoggerContext>();
            var mockLoggerSource = new Mock<ILoggerSource>();
            var mockLoggerConnection = new Mock<ILoggerConnection>();
            var mockLoggerConfig = new Mock<ILoggerConfig>();

            mockLoggerPublisher.Setup(p => p.Info(It.IsAny<string>(), It.IsAny<object[]>())).Verifiable();
            mockLoggerConnection.Setup(l => l.NewPublisher()).Returns(mockLoggerPublisher.Object);
            mockLoggerSource.Setup(s => s.NewConnection(It.IsAny<ILoggerConfig>())).Returns(mockLoggerConnection.Object);
            mockLoggerContext.Setup(c => c.Source).Returns(mockLoggerSource.Object);
            mockLoggerContext.Setup(c => c.LoggerConfig).Returns(mockLoggerConfig.Object);


            var seriLogConsumer = new SeriLogConsumer(mockLoggerContext.Object);

            //------------------------------Act---------------------------------
            var response = seriLogConsumer.Consume(message, null);
            //------------------------------Assert------------------------------
            mockLoggerPublisher.Verify(p => p.Info(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(SeriLogConsumer))]
        public void SeriLogConsumer_Consume_WarningMessage_ShouldCall_Warning()
        {
            //------------------------------Arrange-----------------------------
            var audit = new AuditStub();
            audit.LogLevel = LogLevel.Warn;
            audit.CustomTransactionID = "testing SeriLogConsumer";
            var message = audit;
            var mockLogger = new Mock<ILogger>();
            var mockLoggerPublisher = new Mock<ILoggerPublisher>();
            var mockLoggerContext = new Mock<ILoggerContext>();
            var mockLoggerSource = new Mock<ILoggerSource>();
            var mockLoggerConnection = new Mock<ILoggerConnection>();
            var mockLoggerConfig = new Mock<ILoggerConfig>();

            mockLoggerPublisher.Setup(p => p.Warn(It.IsAny<string>(), It.IsAny<object[]>())).Verifiable();
            mockLoggerConnection.Setup(l => l.NewPublisher()).Returns(mockLoggerPublisher.Object);
            mockLoggerSource.Setup(s => s.NewConnection(It.IsAny<ILoggerConfig>())).Returns(mockLoggerConnection.Object);
            mockLoggerContext.Setup(c => c.Source).Returns(mockLoggerSource.Object);
            mockLoggerContext.Setup(c => c.LoggerConfig).Returns(mockLoggerConfig.Object);


            var seriLogConsumer = new SeriLogConsumer(mockLoggerContext.Object);

            //------------------------------Act---------------------------------
            var response = seriLogConsumer.Consume(message, null);
            //------------------------------Assert------------------------------
            mockLoggerPublisher.Verify(p => p.Warn(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(SeriLogConsumer))]
        public void SeriLogConsumer_Consume_ErrorMessage_ShouldCall_Error()
        {
            //------------------------------Arrange-----------------------------
            var audit = new AuditStub();
            audit.LogLevel = LogLevel.Error;
            audit.CustomTransactionID = "testing SeriLogConsumer";
            var message = audit;
            var mockLogger = new Mock<ILogger>();
            var mockLoggerPublisher = new Mock<ILoggerPublisher>();
            var mockLoggerContext = new Mock<ILoggerContext>();
            var mockLoggerSource = new Mock<ILoggerSource>();
            var mockLoggerConnection = new Mock<ILoggerConnection>();
            var mockLoggerConfig = new Mock<ILoggerConfig>();

            mockLoggerPublisher.Setup(p => p.Error(It.IsAny<string>(), It.IsAny<object[]>())).Verifiable();
            mockLoggerConnection.Setup(l => l.NewPublisher()).Returns(mockLoggerPublisher.Object);
            mockLoggerSource.Setup(s => s.NewConnection(It.IsAny<ILoggerConfig>())).Returns(mockLoggerConnection.Object);
            mockLoggerContext.Setup(c => c.Source).Returns(mockLoggerSource.Object);
            mockLoggerContext.Setup(c => c.LoggerConfig).Returns(mockLoggerConfig.Object);


            var seriLogConsumer = new SeriLogConsumer(mockLoggerContext.Object);

            //------------------------------Act---------------------------------
            var response = seriLogConsumer.Consume(message, null);
            //------------------------------Assert------------------------------
            mockLoggerPublisher.Verify(p => p.Error(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(SeriLogConsumer))]
        public void SeriLogConsumer_Consume_FatalMessage_ShouldCall_Fatal()
        {
            //------------------------------Arrange-----------------------------
            var audit = new AuditStub();
            audit.LogLevel = LogLevel.Fatal;
            audit.CustomTransactionID = "testing SeriLogConsumer";
            var message = audit;
            var mockLogger = new Mock<ILogger>();
            var mockLoggerPublisher = new Mock<ILoggerPublisher>();
            var mockLoggerContext = new Mock<ILoggerContext>();
            var mockLoggerSource = new Mock<ILoggerSource>();
            var mockLoggerConnection = new Mock<ILoggerConnection>();
            var mockLoggerConfig = new Mock<ILoggerConfig>();

            mockLoggerPublisher.Setup(p => p.Fatal(It.IsAny<string>(), It.IsAny<object[]>())).Verifiable();
            mockLoggerConnection.Setup(l => l.NewPublisher()).Returns(mockLoggerPublisher.Object);
            mockLoggerSource.Setup(s => s.NewConnection(It.IsAny<ILoggerConfig>())).Returns(mockLoggerConnection.Object);
            mockLoggerContext.Setup(c => c.Source).Returns(mockLoggerSource.Object);
            mockLoggerContext.Setup(c => c.LoggerConfig).Returns(mockLoggerConfig.Object);


            var seriLogConsumer = new SeriLogConsumer(mockLoggerContext.Object);

            //------------------------------Act---------------------------------
            var response = seriLogConsumer.Consume(message, null);
            //------------------------------Assert------------------------------
            mockLoggerPublisher.Verify(p => p.Fatal(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(SeriLogConsumer))]
        public void SeriLogConsumer_Consume_NotMatchingType_ShouldCall_Debug()
        {
            //------------------------------Arrange-----------------------------
            var audit = new AuditStub();
            audit.LogLevel = LogLevel.Debug;
            audit.CustomTransactionID = "testing SeriLogConsumer";
            var message = audit;
            var mockLogger = new Mock<ILogger>();
            var mockLoggerPublisher = new Mock<ILoggerPublisher>();
            var mockLoggerContext = new Mock<ILoggerContext>();
            var mockLoggerSource = new Mock<ILoggerSource>();
            var mockLoggerConnection = new Mock<ILoggerConnection>();
            var mockLoggerConfig = new Mock<ILoggerConfig>();

            mockLoggerPublisher.Setup(p => p.Debug(It.IsAny<string>(), It.IsAny<object[]>())).Verifiable();
            mockLoggerConnection.Setup(l => l.NewPublisher()).Returns(mockLoggerPublisher.Object);
            mockLoggerSource.Setup(s => s.NewConnection(It.IsAny<ILoggerConfig>())).Returns(mockLoggerConnection.Object);
            mockLoggerContext.Setup(c => c.Source).Returns(mockLoggerSource.Object);
            mockLoggerContext.Setup(c => c.LoggerConfig).Returns(mockLoggerConfig.Object);


            var seriLogConsumer = new SeriLogConsumer(mockLoggerContext.Object);

            //------------------------------Act---------------------------------
            var response = seriLogConsumer.Consume(message, null);
            //------------------------------Assert------------------------------
            mockLoggerPublisher.Verify(p => p.Debug(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        public class AuditStub : IAudit
        {

            public LogLevel LogLevel { get; set; }
            public string AdditionalDetail { get; set; }
            public DateTime AuditDate { get; set; }
            public string AuditType { get; set; }
            public string Environment { get; set; }
            public Exception Exception { get; set; }
            public string ExecutingUser { get; set; }
            public string ExecutionID { get; set; }
            public string CustomTransactionID  { get; set; }
            public long ExecutionOrigin { get; set; }
            public string ExecutionOriginDescription { get; set; }
            public string ExecutionToken { get; set; }
            public int Id { get; set; }
            public bool IsRemoteWorkflow { get; set; }
            public bool IsSubExecution { get; set; }
            public string NextActivity { get; set; }
            public string NextActivityId { get; set; }
            public string NextActivityType { get; set; }
            public string ParentID { get; set; }
            public string PreviousActivity { get; set; }
            public string PreviousActivityId { get; set; }
            public string PreviousActivityType { get; set; }
            public string ServerID { get; set; }
            public string VersionNumber { get; set; }
            public string WorkflowID { get; set; }
            public string WorkflowName { get; set; }
        }
    }
}
