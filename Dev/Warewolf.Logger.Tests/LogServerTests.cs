/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Fleck;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using Warewolf.Auditing;
using Warewolf.Driver.Serilog;
using Warewolf.Interfaces.Auditing;
using Warewolf.Logging;
using Warewolf.Streams;

namespace Warewolf.Logger.Tests
{
    
    [TestClass]
    public class LogServerTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(LogServer))]
        public void LogServer_Constructor_WhenValidParameters_ShouldNotThrowException()
        {
            //--------------------------------Arrange-------------------------------
            //--------------------------------Act-----------------------------------
            var logServer = new LogServer(new Mock<IWebSocketServerFactory>().Object, new Mock<IWriter>().Object, new Mock<ILoggerContext>().Object);
            //--------------------------------Assert--------------------------------
            Assert.IsNotNull(logServer);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(LogServer))]
        [ExpectedException(typeof(ArgumentNullException))]
        public void LogServer_Constructor_WhenNullWebSocketFactory_ShouldNotThrowException()
        {
            //--------------------------------Arrange-------------------------------
            //--------------------------------Act-----------------------------------
            var logServer = new LogServer(null, new Mock<IWriter>().Object, new Mock<ILoggerContext>().Object);
            //--------------------------------Assert--------------------------------
            Assert.IsNull(logServer);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(LogServer))]
        [ExpectedException(typeof(ArgumentNullException))]
        public void LogServer_Constructor_WhenNullWriter_ShouldNotThrowException()
        {
            //--------------------------------Arrange-------------------------------
            //--------------------------------Act-----------------------------------
            var logServer = new LogServer(new Mock<IWebSocketServerFactory>().Object, null, new Mock<ILoggerContext>().Object);
            //--------------------------------Assert--------------------------------
            Assert.IsNull(logServer);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(LogServer))]
        [ExpectedException(typeof(ArgumentNullException))]
        public void LogServer_Constructor_WhenNullLoggerContext_ShouldNotThrowException()
        {
            //--------------------------------Arrange-------------------------------
            //--------------------------------Act-----------------------------------
            var logServer = new LogServer(new Mock<IWebSocketServerFactory>().Object, new Mock<IWriter>().Object, null);
            //--------------------------------Assert--------------------------------
            Assert.IsNull(logServer);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(LogServer))]
        public void LogServer_Start_ShouldCallWebSocketStart()
        {
            //--------------------------------Arrange-------------------------------
            var mockLoggerContext = new Mock<ILoggerContext>();
            mockLoggerContext.Setup(l => l.LoggerConfig).Returns(new Mock<ILoggerConfig>().Object);
            var mockWebSocketServerWrapper = new Mock<IWebSocketServerWrapper>();

            var mockWebSocketServerFactory = new Mock<IWebSocketServerFactory>();
            mockWebSocketServerFactory.Setup(ws => ws.New(It.IsAny<string>())).Returns(mockWebSocketServerWrapper.Object);

            var logServer = new LogServer(mockWebSocketServerFactory.Object, new Mock<IWriter>().Object, mockLoggerContext.Object);
            var mockClient = new Mock<IWebSocketConnection>();

            //--------------------------------Act-----------------------------------
            logServer.Start(new List<IWebSocketConnection>());
            //--------------------------------Assert--------------------------------
            Assert.IsNotNull(logServer);
            mockWebSocketServerWrapper.Verify(w => w.Start(It.IsAny<Action<IWebSocketConnection>>()), Times.Once);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(LogServer))]
        public void LogServer_PerformingStartAction_ShouldCallActionOnTheWrapper()
        {
            //--------------------------------Arrange-------------------------------
            var mockLoggerSource = new Mock<ILoggerSource>();
            mockLoggerSource.Setup(ls => ls.NewConnection(It.IsAny<ILoggerConfig>())).Returns(new Mock<ILoggerConnection>().Object);

            var mockLoggerContext = new Mock<ILoggerContext>();
            mockLoggerContext.Setup(l => l.LoggerConfig).Returns(new Mock<ILoggerConfig>().Object);
            mockLoggerContext.Setup(l => l.Source).Returns(mockLoggerSource.Object);

            Action<IWebSocketConnection> performedAction = null;
            var mockWebSocketServerWrapper = new Mock<IWebSocketServerWrapper>();
            mockWebSocketServerWrapper.Setup(ws => ws.Start(It.IsAny<Action<IWebSocketConnection>>())).Callback((Action<IWebSocketConnection> a) =>
            {
                performedAction = a;
            });

            var mockWebSocketServerFactory = new Mock<IWebSocketServerFactory>();
            mockWebSocketServerFactory.Setup(ws => ws.New(It.IsAny<string>())).Returns(mockWebSocketServerWrapper.Object);

            var logServer = new LogServer(mockWebSocketServerFactory.Object, new Mock<IWriter>().Object, mockLoggerContext.Object);

            var mockClient = new Mock<IWebSocketConnection>();
            mockClient.SetupAllProperties();
            logServer.Start(new List<IWebSocketConnection>());
            //--------------------------------Act-----------------------------------
            performedAction(mockClient.Object);
            //--------------------------------Assert--------------------------------
            mockClient.Verify();
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory(nameof(LogServer))]
        public void LogServer_PerformingClientOnOpen_ShouldCallAction()
        {
            //--------------------------------Arrange-------------------------------
            var mockLoggerSource = new Mock<ILoggerSource>();
            mockLoggerSource.Setup(ls => ls.NewConnection(It.IsAny<ILoggerConfig>())).Returns(new Mock<ILoggerConnection>().Object);

            var mockLoggerContext = new Mock<ILoggerContext>();
            mockLoggerContext.Setup(l => l.LoggerConfig).Returns(new Mock<ILoggerConfig>().Object);
            mockLoggerContext.Setup(l => l.Source).Returns(mockLoggerSource.Object);

            Action<IWebSocketConnection> performedAction = null;
            var mockWebSocketServerWrapper = new Mock<IWebSocketServerWrapper>();
            mockWebSocketServerWrapper.Setup(ws => ws.Start(It.IsAny<Action<IWebSocketConnection>>())).Callback((Action<IWebSocketConnection> a) =>
            {
                performedAction = a;
            });

            var mockWebSocketServerFactory = new Mock<IWebSocketServerFactory>();
            mockWebSocketServerFactory.Setup(ws => ws.New(It.IsAny<string>())).Returns(mockWebSocketServerWrapper.Object);
            var consoleString = "";
            var mockWriter = new Mock<IWriter>();
            mockWriter.Setup(w => w.WriteLine(It.IsAny<string>())).Callback((string s) => { consoleString = s; }) ;
            var logServer = new LogServer(mockWebSocketServerFactory.Object, mockWriter.Object, mockLoggerContext.Object);

            var mockClient = new Mock<IWebSocketConnection>();
            mockClient.SetupAllProperties();
            var clients = new List<IWebSocketConnection>();
            logServer.Start(clients);
            performedAction(mockClient.Object);
            //--------------------------------Act-----------------------------------
            mockClient.Object.OnOpen();
            //--------------------------------Assert--------------------------------
            Assert.AreEqual(1, clients.Count);
            Assert.AreSame(mockClient.Object, clients[0]);
            Assert.AreEqual("Logging Server OnOpen...", consoleString);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(LogServer))]
        public void LogServer_PerformingClientOnClose_ShouldCallAction_And_OnMessage_ShouldTryConsume_Success()
        {
            //--------------------------------Arrange-------------------------------
            var mockStreamConfig = new Mock<IStreamConfig>();
            var mockPublisher = new Mock<IPublisher>();
            var mockLeaderConnection = new Mock<IConnection>();
            var mockSourceConnectionFactory = new Mock<ISourceConnectionFactory>();

            var serializer = new JsonSerializer();
            var testMessage = serializer.Serialize<AuditCommand>(new AuditCommand());

            mockLeaderConnection.Setup(o => o.NewPublisher(It.IsAny<IStreamConfig>())).Returns(mockPublisher.Object);
            mockSourceConnectionFactory.Setup(o => o.NewConnection()).Returns(mockLeaderConnection.Object);

            var mockLoggerSource = new Mock<ILoggerSource>();
            mockLoggerSource.Setup(ls => ls.NewConnection(It.IsAny<ILoggerConfig>())).Returns(new Mock<ILoggerConnection>().Object);

            var mockLoggerContext = new Mock<ILoggerContext>();
            mockLoggerContext.Setup(l => l.LoggerConfig).Returns(new Mock<ILoggerConfig>().Object);
            mockLoggerContext.Setup(l => l.Source).Returns(mockLoggerSource.Object);
            mockLoggerContext.Setup(o => o.LeaderSource).Returns(mockSourceConnectionFactory.Object);
            mockLoggerContext.Setup(o => o.LeaderConfig).Returns(mockStreamConfig.Object);

            Action<IWebSocketConnection> performedAction = null;
            var mockWebSocketServerWrapper = new Mock<IWebSocketServerWrapper>();
            mockWebSocketServerWrapper.Setup(ws => ws.Start(It.IsAny<Action<IWebSocketConnection>>())).Callback((Action<IWebSocketConnection> a) =>
            {
                performedAction = a;
            });

            var mockWebSocketServerFactory = new Mock<IWebSocketServerFactory>();
            mockWebSocketServerFactory.Setup(ws => ws.New(It.IsAny<string>())).Returns(mockWebSocketServerWrapper.Object);
            var consoleString = "";
            var mockWriter = new Mock<IWriter>();
            mockWriter.Setup(w => w.WriteLine(It.IsAny<string>())).Callback((string s) => { consoleString = s; });
            var logServer = new LogServer(mockWebSocketServerFactory.Object, mockWriter.Object, mockLoggerContext.Object);

            var mockClient = new Mock<IWebSocketConnection>();
            mockClient.SetupAllProperties();
            var clients = new List<IWebSocketConnection> { mockClient.Object };
            logServer.Start(clients);
            performedAction(mockClient.Object);

            mockClient.Object.OnMessage(Encoding.UTF8.GetString(testMessage));
            //--------------------------------Act-----------------------------------
            mockClient.Object.OnClose();
            //--------------------------------Assert--------------------------------
            Assert.AreEqual(0, clients.Count);
            Assert.AreEqual("Logging Server OnClose...", consoleString);

            mockPublisher.Verify(o => o.Publish(It.IsAny<byte[]>()), Times.Once);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(LogServer))]
        public void LogServer_PerformingClientOnClose_ShouldCallAction_And_OnMessage_ShouldTryConsume_Fails()
        {
            //--------------------------------Arrange-------------------------------
            var mockStreamConfig = new Mock<IStreamConfig>();
            var mockPublisher = new Mock<IPublisher>();
            var mockLeaderConnection = new Mock<IConnection>();
            var mockSourceConnectionFactory = new Mock<ISourceConnectionFactory>();
            var mockAuditCommandConsumerFactory = new Mock<IAuditCommandConsumerFactory>();
            var mockAuditCommandConsumer = new Mock<IAuditCommandConsumer>();

            mockAuditCommandConsumerFactory.Setup(o => o.New(It.IsAny<SeriLogConsumer>(), It.IsAny<IWebSocketConnection>(), It.IsAny<IWriter>()))
                .Returns(mockAuditCommandConsumer.Object);

            var serializer = new JsonSerializer();
            var testMessage = serializer.Serialize<AuditCommand>(new AuditCommand());

            var falseException = new Exception("False exception from LogServerTests.cs");
            mockPublisher.Setup(o => o.Publish(It.IsAny<byte[]>())).Throws(falseException);
            mockLeaderConnection.Setup(o => o.NewPublisher(It.IsAny<IStreamConfig>())).Returns(mockPublisher.Object);
            mockSourceConnectionFactory.Setup(o => o.NewConnection()).Returns(mockLeaderConnection.Object);

            var mockLoggerSource = new Mock<ILoggerSource>();
            mockLoggerSource.Setup(ls => ls.NewConnection(It.IsAny<ILoggerConfig>())).Returns(new Mock<ILoggerConnection>().Object);

            var mockLoggerContext = new Mock<ILoggerContext>();
            mockLoggerContext.Setup(l => l.LoggerConfig).Returns(new Mock<ILoggerConfig>().Object);
            mockLoggerContext.Setup(l => l.Source).Returns(mockLoggerSource.Object);
            mockLoggerContext.Setup(o => o.LeaderSource).Returns(mockSourceConnectionFactory.Object);
            mockLoggerContext.Setup(o => o.LeaderConfig).Returns(mockStreamConfig.Object);

            Action<IWebSocketConnection> performedAction = null;
            var mockWebSocketServerWrapper = new Mock<IWebSocketServerWrapper>();
            mockWebSocketServerWrapper.Setup(ws => ws.Start(It.IsAny<Action<IWebSocketConnection>>())).Callback((Action<IWebSocketConnection> a) =>
            {
                performedAction = a;
            });

            var mockWebSocketServerFactory = new Mock<IWebSocketServerFactory>();
            mockWebSocketServerFactory.Setup(ws => ws.New(It.IsAny<string>())).Returns(mockWebSocketServerWrapper.Object);
            var consoleString = "";
            var mockWriter = new Mock<IWriter>();
            mockWriter.Setup(w => w.WriteLine(It.IsAny<string>())).Callback((string s) => { consoleString = s; });
            var logServer = new LogServer(mockWebSocketServerFactory.Object, mockWriter.Object, mockLoggerContext.Object, mockAuditCommandConsumerFactory.Object);

            var mockClient = new Mock<IWebSocketConnection>();
            mockClient.SetupAllProperties();
            var clients = new List<IWebSocketConnection> { mockClient.Object };
            logServer.Start(clients);
            performedAction(mockClient.Object);

            mockClient.Object.OnMessage(Encoding.UTF8.GetString(testMessage));
            //--------------------------------Act-----------------------------------
            mockClient.Object.OnClose();
            //--------------------------------Assert--------------------------------
            Assert.AreEqual(0, clients.Count);
            Assert.AreEqual("Logging Server OnClose...", consoleString);

            mockAuditCommandConsumerFactory.Verify(o => o.New(It.IsAny<SeriLogConsumer>(), It.IsAny<IWebSocketConnection>(), It.IsAny<IWriter>()));
        }

    }
}
