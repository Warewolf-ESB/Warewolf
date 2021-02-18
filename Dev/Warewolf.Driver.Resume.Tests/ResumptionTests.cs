/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;
using Dev2.Communication;
using Dev2.Network;
using Dev2.Studio.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Common;
using Warewolf.Execution;

namespace Warewolf.Driver.Resume.Tests
{
    [TestClass]
    public class ResumptionTests
    {
        [TestMethod]
        [TestCategory(nameof(Resumption))]
        [Owner("Pieter Terblanche")]
        public void Resumption_Connect_Result_False()
        {
            //--------------Arrange------------------------------
            var serverEndpoint = new Uri($"https://{System.Net.Dns.GetHostName()}:3143");
            //--------------Act----------------------------------
            var mockExecutionLogPublisher = new Mock<IExecutionLogPublisher>();
            mockExecutionLogPublisher.Setup(o => o.Info("Connecting to server: " + serverEndpoint + "..."));

            var mockEnvironmentConnection = new Mock<IEnvironmentConnection>();
            mockEnvironmentConnection.Setup(o => o.ConnectAsync(Guid.Empty)).ReturnsAsync(false);

            var mockServerProxy = new Mock<IServerProxyFactory>();
            mockServerProxy.Setup(o => o.New(serverEndpoint)).Returns(mockEnvironmentConnection.Object);

            var resumption = new Resumption(mockServerProxy.Object, null);
            var connect = resumption.Connect(mockExecutionLogPublisher.Object);
            //--------------Assert-------------------------------
            Assert.IsFalse(connect);
            mockExecutionLogPublisher.Verify(o => o.Info("Connecting to server: " + serverEndpoint + "..."), Times.Once);
            mockEnvironmentConnection.Verify(o => o.ConnectAsync(Guid.Empty), Times.Once);
            mockServerProxy.Verify(o => o.New(serverEndpoint), Times.Once);
        }

        [TestMethod]
        [TestCategory(nameof(Resumption))]
        [Owner("Pieter Terblanche")]
        public void Resumption_Connect_Result_True()
        {
            //--------------Arrange------------------------------
            var serverEndpoint = new Uri($"https://{System.Net.Dns.GetHostName()}:3143");
            //--------------Act----------------------------------
            var mockExecutionLogPublisher = new Mock<IExecutionLogPublisher>();
            mockExecutionLogPublisher.Setup(o => o.Info("Connecting to server: " + serverEndpoint + "..."));
            mockExecutionLogPublisher.Setup(o => o.Info("Connecting to server: " + serverEndpoint + "... successful"));

            var mockEnvironmentConnection = new Mock<IEnvironmentConnection>();
            mockEnvironmentConnection.Setup(o => o.ConnectAsync(Guid.Empty)).ReturnsAsync(true);

            var mockServerProxy = new Mock<IServerProxyFactory>();
            mockServerProxy.Setup(o => o.New(serverEndpoint)).Returns(mockEnvironmentConnection.Object);

            var resumption = new Resumption(mockServerProxy.Object, null);
            var connect = resumption.Connect(mockExecutionLogPublisher.Object);
            //--------------Assert-------------------------------
            Assert.IsTrue(connect);
            mockExecutionLogPublisher.Verify(o => o.Info("Connecting to server: " + serverEndpoint + "..."), Times.Once);
            mockExecutionLogPublisher.Verify(o => o.Info("Connecting to server: " + serverEndpoint + "... successful"), Times.Once);
            mockEnvironmentConnection.Verify(o => o.ConnectAsync(Guid.Empty), Times.Once);
            mockServerProxy.Verify(o => o.New(serverEndpoint), Times.Once);
        }

        [TestMethod]
        [TestCategory(nameof(Resumption))]
        [Owner("Pieter Terblanche")]
        public void Resumption_ConnectAsync_Exception_Expect_False()
        {
            //--------------Arrange------------------------------
            var serverEndpoint = new Uri($"https://{System.Net.Dns.GetHostName()}:3143");
            var exMessage = "Connecting to server: " + serverEndpoint + "... unsuccessful One or more errors occurred. failed to connect";
            //--------------Act----------------------------------
            var mockExecutionLogPublisher = new Mock<IExecutionLogPublisher>();
            mockExecutionLogPublisher.Setup(o => o.Info("Connecting to server: " + serverEndpoint + "..."));
            mockExecutionLogPublisher.Setup(o => o.Error(exMessage));

            var mockEnvironmentConnection = new Mock<IEnvironmentConnection>();

            var innerException = new Exception("Connection Error: ");
            var exception = new Exception("failed to connect", innerException);

            mockEnvironmentConnection.Setup(o => o.ConnectAsync(Guid.Empty)).ThrowsAsync(exception);

            var mockServerProxy = new Mock<IServerProxyFactory>();
            mockServerProxy.Setup(o => o.New(serverEndpoint)).Returns(mockEnvironmentConnection.Object);

            var resumption = new Resumption(mockServerProxy.Object, null);
            var connect = resumption.Connect(mockExecutionLogPublisher.Object);
            //--------------Assert-------------------------------
            Assert.IsFalse(connect);
            mockExecutionLogPublisher.Verify(o => o.Info("Connecting to server: " + serverEndpoint + "..."), Times.Once);
            mockExecutionLogPublisher.Verify(o => o.Error(exMessage), Times.Once);
            mockEnvironmentConnection.Verify(o => o.ConnectAsync(Guid.Empty), Times.Once);
            mockServerProxy.Verify(o => o.New(serverEndpoint), Times.Once);
        }

        [TestMethod]
        [TestCategory(nameof(Resumption))]
        [Owner("Pieter Terblanche")]
        public void Resumption_Exception_Expect_False()
        {
            //--------------Arrange------------------------------
            var serverEndpoint = new Uri($"https://{System.Net.Dns.GetHostName()}:3143");
            var exMessage = "Connecting to server: " + serverEndpoint + "... unsuccessful Object reference not set to an instance of an object.";
            //--------------Act----------------------------------
            var mockExecutionLogPublisher = new Mock<IExecutionLogPublisher>();
            mockExecutionLogPublisher.Setup(o => o.Info("Connecting to server: " + serverEndpoint + "..."));
            mockExecutionLogPublisher.Setup(o => o.Error(exMessage));

            var mockEnvironmentConnection = new Mock<IEnvironmentConnection>();

            var innerException = new Exception("Connection Error: ");
            var exception = new Exception("failed to connect", innerException);

            mockEnvironmentConnection.Setup(o => o.ConnectAsync(Guid.Empty)).ThrowsAsync(exception);

            var resumption = new Resumption(null, null);
            var connect = resumption.Connect(mockExecutionLogPublisher.Object);
            //--------------Assert-------------------------------
            Assert.IsFalse(connect);
            mockExecutionLogPublisher.Verify(o => o.Info("Connecting to server: " + serverEndpoint + "..."), Times.Once);
            mockExecutionLogPublisher.Verify(o => o.Error(exMessage), Times.Once);
            mockEnvironmentConnection.Verify(o => o.ConnectAsync(Guid.Empty), Times.Never);
        }

        [TestMethod]
        [TestCategory(nameof(Resumption))]
        [Owner("Pieter Terblanche")]
        public void Resumption_Resume()
        {
            //--------------Arrange------------------------------
            var resourceId = Guid.NewGuid().ToString();
            const string environment = "";
            const string versionNumber = "0";
            var startActivityId = Guid.NewGuid().ToString();
            var currentuserprincipal = WindowsIdentity.GetCurrent().Name;

            var values = new Dictionary<string, StringBuilder>
            {
                {"resourceID", new StringBuilder(resourceId)},
                {"environment", new StringBuilder(environment)},
                {"startActivityId", new StringBuilder(versionNumber)},
                {"versionNumber", new StringBuilder(startActivityId)},
                {"currentuserprincipal", new StringBuilder(currentuserprincipal)}
            };
            //--------------Act----------------------------------

            var serverEndpoint = new Uri($"https://{System.Net.Dns.GetHostName()}:3143");

            var executeMessage = new ExecuteMessage {Message = new StringBuilder("Success")};

            var mockResourceCatalogProxy = new Mock<IResourceCatalogProxy>();
            mockResourceCatalogProxy
                .Setup(o => o.ResumeWorkflowExecution(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<string>(), It.IsAny<string>())).Returns(executeMessage);

            var mockEnvironmentConnection = new Mock<IEnvironmentConnection>();

            var mockResourceCatalogProxyFactory = new Mock<IResourceCatalogProxyFactory>();
            mockResourceCatalogProxyFactory.Setup(o => o.New(mockEnvironmentConnection.Object))
                .Returns(mockResourceCatalogProxy.Object);

            var mockServerProxy = new Mock<IServerProxyFactory>();
            mockServerProxy.Setup(o => o.New(serverEndpoint)).Returns(mockEnvironmentConnection.Object);

            var resumption = new Resumption(mockServerProxy.Object, mockResourceCatalogProxyFactory.Object);

            var message = resumption.Resume(values);
            //--------------Assert-------------------------------
            Assert.AreEqual("Success", message.Message.ToString());
        }
    }
}
