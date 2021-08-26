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
using Hangfire.Server;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using HangfireServer;
using Warewolf.Auditing;
using Warewolf.Driver.Resume;
using Warewolf.Execution;
using Dev2.Network;
using Dev2.Studio.Interfaces;
using System.Threading.Tasks;
using Warewolf.Common;
using Warewolf.Interfaces.Auditing;
using Warewolf.HangfireServer.Tests.Test_Utils;

namespace Warewolf.HangfireServer.Tests
{
    [TestClass]
    [DoNotParallelize]
    [TestCategory("CannotParallelize")]
    public class ResumptionAttributeTests
    {
        private readonly string _currentuserprincipal = WindowsIdentity.GetCurrent().Name;
        private readonly Uri _serverEndpoint = new Uri($"https://{System.Net.Dns.GetHostName()}:3143");

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(ResumptionAttribute))]
        public void ResumptionAttribute_LogResumption_Connect_True()
        {
            var workflowId = Guid.NewGuid().ToString();
            const string environment = "";
            const string versionNumber = "0";
            var startActivityId = Guid.NewGuid().ToString();

            var values = new Dictionary<string, StringBuilder>
            {
                {"resourceID", new StringBuilder(workflowId)},
                {"environment", new StringBuilder(environment)},
                {"startActivityId", new StringBuilder(versionNumber)},
                {"versionNumber", new StringBuilder(startActivityId)},
                {"currentuserprincipal", new StringBuilder(_currentuserprincipal)}
            };

            var jobId = Guid.NewGuid().ToString();
            var performContext = new PerformContextMock(jobId, values);

            var mockLogger = new Mock<IExecutionLogPublisher>();
            mockLogger.Setup(o => o.LogResumedExecution(It.IsAny<Audit>())).Verifiable();

            var executeMessage = Task.FromResult(new ExecuteMessage
            {
                Message = new StringBuilder("Execution Completed."),
                HasError = false
            });

            var mockResumption = new Mock<IResumption>();
            mockResumption.Setup(o => o.Connect()).Returns(true);
            mockResumption.Setup(o => o.ResumeAsync(values)).Returns(executeMessage);

            var mockResumptionFactory = new Mock<IResumptionFactory>();
            mockResumptionFactory.Setup(o => o.New(mockLogger.Object)).Returns(mockResumption.Object).Verifiable();

            var resumptionAttribute = new ResumptionAttribute(mockLogger.Object, mockResumptionFactory.Object);
            var audit = new Audit
            {
                AuditDate = DateTime.Now
            };
            resumptionAttribute.OnPerformResume(new PerformingContext(performContext.Object));

            mockResumptionFactory.Verify(o => o.New(mockLogger.Object), Times.Once);
            mockResumption.Verify(o => o.Connect(), Times.Once);
            mockLogger.Verify(o => o.LogResumedExecution(It.IsAny<Audit>()), Times.Never, "This call can be called inside the Resume method in Resumption.cs");
            mockResumption.Verify(o => o.ResumeAsync(values), Times.Once);
        }

        [TestMethod]
        [Owner("Siphamamandla Dube")]
        [TestCategory(nameof(ResumptionAttribute))]
        public void ResumptionAttribute_LogResumption_Connect_False()
        {
            var workflowId = Guid.NewGuid().ToString();
            const string environment = "";
            const string versionNumber = "0";
            var startActivityId = Guid.NewGuid().ToString();

            var values = new Dictionary<string, StringBuilder>
            {
                {"resourceID", new StringBuilder(workflowId)},
                {"environment", new StringBuilder(environment)},
                {"startActivityId", new StringBuilder(versionNumber)},
                {"versionNumber", new StringBuilder(startActivityId)},
                {"currentuserprincipal", new StringBuilder(_currentuserprincipal)}
            };

            var jobId = Guid.Parse("61577009-7c48-491b-849c-a89d2cd1d117").ToString();
            var performContext = new PerformContextMock(jobId, values);

            var mockLogger = new Mock<IExecutionLogPublisher>();
            mockLogger.Setup(o => o.LogResumedExecution(It.IsAny<Audit>())).Verifiable();
            mockLogger.Setup(o => o.Error("Failed to perform job {0}, could not establish a connection.", jobId)).Verifiable();

            var mockResumption = new Mock<IResumption>();
            mockResumption.Setup(o => o.Connect()).Returns(false);
            mockResumption.Setup(o => o.ResumeAsync(values)).Verifiable();

            var mockResumptionFactory = new Mock<IResumptionFactory>();
            mockResumptionFactory.Setup(o => o.New(mockLogger.Object)).Returns(mockResumption.Object).Verifiable();

            var resumptionAttribute = new ResumptionAttribute(mockLogger.Object, mockResumptionFactory.Object);

            resumptionAttribute.OnPerformResume(new PerformingContext(performContext.Object));//Fire and Forget
            //Assert.ThrowsException<AggregateException>(()=> resumptionAttribute.OnPerformResume(new PerformingContext(performContext.Object)));
            mockResumptionFactory.Verify(o => o.New(mockLogger.Object), Times.Once);
            mockResumption.Verify(o => o.Connect(), Times.Once);
            mockLogger.Verify(o => o.LogResumedExecution(It.IsAny<Audit>()), Times.Never);
            mockResumption.Verify(o => o.ResumeAsync(values), Times.Never);
            mockLogger.Verify(o => o.Error("Failed to perform job { 61577009-7c48-491b-849c-a89d2cd1d117 }, could not establish a connection.", new object[] {"61577009-7c48-491b-849c-a89d2cd1d117"}), Times.Once);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ResumptionAttribute))]
        public void ResumptionAttribute_LogResumption_HasError_FailsWithMessage()
        {
            var workflowId = Guid.NewGuid().ToString();
            const string environment = "";
            const string versionNumber = "0";
            var startActivityId = Guid.NewGuid().ToString();


            var values = new Dictionary<string, StringBuilder>
            {
                {"resourceID", new StringBuilder(workflowId)},
                {"environment", new StringBuilder(environment)},
                {"startActivityId", new StringBuilder(versionNumber)},
                {"versionNumber", new StringBuilder(startActivityId)},
                {"currentuserprincipal", new StringBuilder(_currentuserprincipal)}
            };

            var jobId = Guid.Parse("787fad72-86f7-4f20-8bff-8e5307441fc4").ToString();
            var performContext = new PerformContextMock(jobId, values);

            var mockLogger = new Mock<IExecutionLogPublisher>();
            mockLogger.Setup(o => o.LogResumedExecution(It.IsAny<Audit>())).Verifiable();

            var executeMessage = Task.FromResult(new ExecuteMessage
            {
                Message = new StringBuilder("Error In Execution."),
                HasError = true
            });

            var mockResumption = new Mock<IResumption>();
            mockResumption.Setup(o => o.Connect()).Returns(true);
            mockResumption.Setup(o => o.ResumeAsync(values)).Returns(executeMessage);

            var mockResumptionFactory = new Mock<IResumptionFactory>();
            mockResumptionFactory.Setup(o => o.New(mockLogger.Object)).Returns(mockResumption.Object).Verifiable();

            var resumptionAttribute = new ResumptionAttribute(mockLogger.Object, mockResumptionFactory.Object);

            resumptionAttribute.OnPerformResume(new PerformingContext(performContext.Object));//Fire and Forget
            //Assert.ThrowsException<AggregateException>(() => resumptionAttribute.OnPerformResume(new PerformingContext(performContext.Object)));

            mockLogger.Verify(o => o.Info("Performing Resume of job {787fad72-86f7-4f20-8bff-8e5307441fc4}, connection established.", new object[] { "787fad72-86f7-4f20-8bff-8e5307441fc4" }), Times.Once);
            mockLogger.Verify(o => o.Error("Failed to perform job { 787fad72-86f7-4f20-8bff-8e5307441fc4 }, Error In Execution.", new object[] {"787fad72-86f7-4f20-8bff-8e5307441fc4"}), Times.Once);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ResumptionAttribute))]
        public void ResumptionAttribute_LogResumption_Environment_in_Audit_IsBlank()
        {
            var workflowId = Guid.Parse("ba3f406b-cad6-4c77-be41-6ffae67aeae6").ToString();
            const string environment = "this should be nothing";
            const string versionNumber = "0";
            var startActivityId = Guid.Parse("115611f9-0c0b-4244-88b9-65b08d89dbb8").ToString();

            var values = new Dictionary<string, StringBuilder>
            {
                {"resourceID", new StringBuilder(workflowId)},
                {"environment", new StringBuilder(environment)},
                {"startActivityId", new StringBuilder(versionNumber)},
                {"versionNumber", new StringBuilder(startActivityId)},
                {"currentuserprincipal", new StringBuilder(_currentuserprincipal)}
            };

            var executeMessage = new ExecuteMessage
            {
                Message = new StringBuilder("Execution Completed."),
                HasError = false
            };

            var jobId = Guid.Parse("679680ae-ba65-4dcc-afb1-1004f237c325").ToString();
            var performContext = new PerformContextMock(jobId, values);

            var mockLogger = new Mock<IExecutionLogPublisher>();
            mockLogger.Setup(o => o.LogResumedExecution(It.IsAny<IAudit>())).Verifiable();

            var mockEnvironmentConnection = new Mock<IEnvironmentConnection>();
            mockEnvironmentConnection.Setup(o => o.ConnectAsync(Guid.Empty))
                .Returns(Task.FromResult(true));

            var mockServerProxyFactory = new Mock<IServerProxyFactory>();
            mockServerProxyFactory.Setup(o => o.New(It.IsAny<Uri>()))
                .Returns(mockEnvironmentConnection.Object);

            var mockResourceCatalogProxy = new Mock<IResourceCatalogProxy>();
            mockResourceCatalogProxy.Setup(o => o.ResumeWorkflowExecutionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))//(new StringBuilder(workflowId).ToString(), new StringBuilder(environment).ToString(), new StringBuilder(startActivityId).ToString(), new StringBuilder(versionNumber).ToString(), new StringBuilder(currentuserprincipal).ToString()))
                .Returns(Task.FromResult(executeMessage));

            var mockResourceCatalogProxyFactory = new Mock<IResourceCatalogProxyFactory>();
            mockResourceCatalogProxyFactory.Setup(o => o.New(mockEnvironmentConnection.Object))
                .Returns(mockResourceCatalogProxy.Object); 

            var resumption = new Resumption(mockLogger.Object, mockServerProxyFactory.Object, mockResourceCatalogProxyFactory.Object);

            var mockResumptionFactory = new Mock<IResumptionFactory>();
            mockResumptionFactory.Setup(o => o.New(mockLogger.Object)).Returns(resumption).Verifiable();

            var resumptionAttribute = new ResumptionAttribute(mockLogger.Object, mockResumptionFactory.Object);
            resumptionAttribute.OnPerformResume(new PerformingContext(performContext.Object));

            mockResumptionFactory.Verify(o => o.New(mockLogger.Object), Times.Once);
            mockLogger.Verify(o => o.LogResumedExecution(It.IsAny<IAudit>()), Times.Once);
            mockLogger.Verify(o => o.Info($"Connecting to server: {_serverEndpoint}..."), Times.Once);
            mockLogger.Verify(o => o.Info($"Connecting to server: {_serverEndpoint}... successful"), Times.Once);
            mockLogger.Verify(o => o.Info("Performing Resume of job {679680ae-ba65-4dcc-afb1-1004f237c325}, connection established.", new object[] { "679680ae-ba65-4dcc-afb1-1004f237c325" }), Times.Once);
            mockResourceCatalogProxy.Verify(o => o.ResumeWorkflowExecutionAsync("ba3f406b-cad6-4c77-be41-6ffae67aeae6", "this should be nothing", "0", "115611f9-0c0b-4244-88b9-65b08d89dbb8", _currentuserprincipal), Times.Once);
        }

        /*[TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(ResumptionAttribute))]
        public void ResumptionAttribute_LogResumption_Environment_in_Audit_IsNotBlank_Fails()
        {
            var workflowId = Guid.NewGuid().ToString();
            const string environment = "this should be nothing";
            const string versionNumber = "0";
            var startActivityId = Guid.NewGuid().ToString();

            var values = new Dictionary<string, StringBuilder>
            {
                {"resourceID", new StringBuilder(workflowId)},
                {"environment", new StringBuilder(environment)},
                {"startActivityId", new StringBuilder(versionNumber)},
                {"versionNumber", new StringBuilder(startActivityId)},
                {"currentuserprincipal", new StringBuilder(currentuserprincipal)}
            };

            var jobId = Guid.NewGuid().ToString();
            var performContext = new PerformContextMock(jobId, values);
            var auditDate = DateTime.Now;
            var initAudit = new Audit
            {
                AuditDate = auditDate,
            };

            var auditWithOverWrittenEnvironment = initAudit;
            auditWithOverWrittenEnvironment.WorkflowID = workflowId;
            auditWithOverWrittenEnvironment.Environment = String.Empty;
            auditWithOverWrittenEnvironment.AuditDate = auditDate;
            auditWithOverWrittenEnvironment.VersionNumber = versionNumber;
            auditWithOverWrittenEnvironment.NextActivityId = startActivityId;
            auditWithOverWrittenEnvironment.AuditType = "LogResumeExecutionState";
            auditWithOverWrittenEnvironment.LogLevel = LogLevel.Info;
            auditWithOverWrittenEnvironment.ExecutingUser = currentuserprincipal;

            var auditValidation = new Audit
            {
                WorkflowID = workflowId,
                Environment = environment,
                AuditDate = auditDate,
                VersionNumber = versionNumber,
                NextActivityId = startActivityId,
                AuditType = "LogResumeExecutionState",
                LogLevel = LogLevel.Info,
                ExecutingUser = currentuserprincipal
            };

            var mockLogger = new Mock<IExecutionLogPublisher>();
            mockLogger.Setup(o => o.LogResumedExecution(auditWithOverWrittenEnvironment)).Verifiable();

            var executeMessage = new ExecuteMessage
            {
                Message = new StringBuilder("Execution Completed."),
                HasError = false
            };

            var mockResumption = new Mock<IResumption>();
            mockResumption.Setup(o => o.Connect().Returns(true);
            mockResumption.Setup(o => o.Resume(values)).Returns(executeMessage);

            var mockResumptionFactory = new Mock<IResumptionFactory>();
            mockResumptionFactory.Setup(o => o.New()).Returns(mockResumption.Object).Verifiable();

            var resumptionAttribute = new ResumptionAttribute(mockLogger.Object, mockResumptionFactory.Object);
            resumptionAttribute.OnPerformResume(new PerformingContext(performContext.Object), initAudit);

            mockResumptionFactory.Verify(o => o.New(), Times.Once);
            mockResumption.Verify(o => o.Connect(mockLogger.Object), Times.Once);

            mockLogger.Verify(o => o.LogResumedExecution(auditWithOverWrittenEnvironment), Times.Once);
            mockLogger.Verify(o => o.LogResumedExecution(auditValidation), Times.Never);
            mockResumption.Verify(o => o.Resume(values), Times.Once);
        }*/

    }
}