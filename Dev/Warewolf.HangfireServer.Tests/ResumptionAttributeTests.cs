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
using Dev2.Communication;
using Hangfire;
using Hangfire.Common;
using Hangfire.Server;
using Hangfire.Storage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using HangfireServer;
using Warewolf.Auditing;
using Warewolf.Driver.Resume;
using Warewolf.Execution;

namespace Warewolf.HangfireServer.Tests
{
    [TestClass]
    [DoNotParallelize]
    public class ResumptionAttributeTests
    {
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
                {"versionNumber", new StringBuilder(startActivityId)}
            };

            var jobId = Guid.NewGuid().ToString();
            var performContext = new PerformContextMock(jobId, values);

            var mockLogger = new Mock<IExecutionLogPublisher>();
            mockLogger.Setup(o => o.LogResumedExecution(It.IsAny<Audit>())).Verifiable();

            var executeMessage = new ExecuteMessage
            {
                Message = new StringBuilder("Execution Completed."),
                HasError = false
            };

            var mockResumption = new Mock<IResumption>();
            mockResumption.Setup(o => o.Connect()).Returns(true);
            mockResumption.Setup(o => o.Resume(values)).Returns(executeMessage);

            var mockResumptionFactory = new Mock<IResumptionFactory>();
            mockResumptionFactory.Setup(o => o.New()).Returns(mockResumption.Object).Verifiable();

            var resumptionAttribute = new ResumptionAttribute(mockLogger.Object, mockResumptionFactory.Object);
            resumptionAttribute.OnPerformResume(new PerformingContext(performContext.Object));

            mockResumptionFactory.Verify(o => o.New(), Times.Once);
            mockResumption.Verify(o => o.Connect(), Times.Once);
            mockLogger.Verify(o => o.LogResumedExecution(It.IsAny<Audit>()), Times.Once);
            mockResumption.Verify(o => o.Resume(values), Times.Once);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
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
                {"versionNumber", new StringBuilder(startActivityId)}
            };

            var jobId = Guid.NewGuid().ToString();
            var performContext = new PerformContextMock(jobId, values);

            var mockLogger = new Mock<IExecutionLogPublisher>();
            mockLogger.Setup(o => o.LogResumedExecution(It.IsAny<Audit>())).Verifiable();
            mockLogger.Setup(o => o.Error("Failed to perform job {0}, could not establish a connection.", jobId)).Verifiable();

            var mockResumption = new Mock<IResumption>();
            mockResumption.Setup(o => o.Connect()).Returns(false);
            mockResumption.Setup(o => o.Resume(values)).Verifiable();

            var mockResumptionFactory = new Mock<IResumptionFactory>();
            mockResumptionFactory.Setup(o => o.New()).Returns(mockResumption.Object).Verifiable();

            var resumptionAttribute = new ResumptionAttribute(mockLogger.Object, mockResumptionFactory.Object);
            resumptionAttribute.OnPerformResume(new PerformingContext(performContext.Object));

            mockResumptionFactory.Verify(o => o.New(), Times.Once);
            mockResumption.Verify(o => o.Connect(), Times.Once);
            mockLogger.Verify(o => o.LogResumedExecution(It.IsAny<Audit>()), Times.Never);
            mockResumption.Verify(o => o.Resume(values), Times.Never);
            mockLogger.Verify(o => o.Error("Failed to perform job {0}, could not establish a connection.", jobId), Times.Once);
        }

        [TestMethod]
        [Owner("Candie Daniel")]
        [TestCategory(nameof(ResumptionAttribute))]
        [ExpectedException(typeof(InvalidOperationException))]
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
                {"versionNumber", new StringBuilder(startActivityId)}
            };

            var jobId = Guid.NewGuid().ToString();
            var performContext = new PerformContextMock(jobId, values);

            var mockLogger = new Mock<IExecutionLogPublisher>();
            mockLogger.Setup(o => o.LogResumedExecution(It.IsAny<Audit>())).Verifiable();

            var executeMessage = new ExecuteMessage
            {
                Message = new StringBuilder("Error In Execution."),
                HasError = true
            };

            var mockResumption = new Mock<IResumption>();
            mockResumption.Setup(o => o.Connect()).Returns(true);
            mockResumption.Setup(o => o.Resume(values)).Returns(executeMessage);

            var mockResumptionFactory = new Mock<IResumptionFactory>();
            mockResumptionFactory.Setup(o => o.New()).Returns(mockResumption.Object).Verifiable();

            var resumptionAttribute = new ResumptionAttribute(mockLogger.Object, mockResumptionFactory.Object);
            resumptionAttribute.OnPerformResume(new PerformingContext(performContext.Object));
        }

        class PerformContextMock
        {
            private readonly Lazy<PerformContext> _context;

            public PerformContextMock(string jobId, Dictionary<string, StringBuilder> values)
            {
                Connection = new Mock<IStorageConnection>();
                BackgroundJob = new BackgroundJobMock(jobId, values);
                CancellationToken = new Mock<IJobCancellationToken>();

                _context = new Lazy<PerformContext>(
                    () => new PerformContext(Connection.Object, BackgroundJob.Object, CancellationToken.Object));
            }

            public Mock<IStorageConnection> Connection { get; set; }
            public BackgroundJobMock BackgroundJob { get; set; }
            public Mock<IJobCancellationToken> CancellationToken { get; set; }

            public PerformContext Object => _context.Value;

            public static void SomeMethod()
            {
            }
        }

        class BackgroundJobMock
        {
            private readonly Lazy<BackgroundJob> _object;

            public BackgroundJobMock(string jobId, Dictionary<string, StringBuilder> values)
            {
                Id = jobId;
                Job = Job.FromExpression(() => ResumeWorkflow(values, null));
                CreatedAt = DateTime.UtcNow;

                _object = new Lazy<BackgroundJob>(
                    () => new BackgroundJob(Id, Job, CreatedAt));
            }

            public string Id { get; set; }
            public Job Job { get; set; }
            public DateTime CreatedAt { get; set; }

            public BackgroundJob Object => _object.Value;

            public static void ResumeWorkflow(Dictionary<string, StringBuilder> values, PerformContext context)
            {
            }
        }
    }
}