/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Hangfire.Server;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;
using Warewolf.Driver.Resume;
using Warewolf.Execution;
using Warewolf.HangfireServer.Tests.Test_Utils;

namespace Warewolf.HangfireServer.Tests
{
    [TestClass]
    public class WarewolfResumeWorkflowTests
    {
        private readonly static string _workflowId = Guid.Parse("ba3f406b-cad6-4c77-be41-6ffae67aeae6").ToString();
        private const string _environment = "this should be nothing";
        private const string _versionNumber = "0";
        private readonly static string _startActivityId = Guid.Parse("115611f9-0c0b-4244-88b9-65b08d89dbb8").ToString();
        private readonly static string _currentuserprincipal = WindowsIdentity.GetCurrent().Name;


        private readonly Dictionary<string, StringBuilder> _values = new Dictionary<string, StringBuilder>
        {
            {"resourceID", new StringBuilder(_workflowId)},
            {"environment", new StringBuilder(_environment)},
            {"startActivityId", new StringBuilder(_versionNumber)},
            {"versionNumber", new StringBuilder(_startActivityId)},
            {"currentuserprincipal", new StringBuilder(_currentuserprincipal)}
        };

        [TestMethod]
        [TestCategory(nameof(WarewolfResumeWorkflow))]
        [Owner("Siphamandla Dube")]
        public void WarewolfResumeWorkflow_PerformResumption_Given_ConnectionFails_ShouldThrowInvalidOperationException()
        {
            var jobId = Guid.Parse("dc80a09e-21f3-4f92-9ec0-6d3b348e49fa").ToString();

            var mockIResumption = new Mock<IResumption>();
            mockIResumption.Setup(o => o.Connect())
                .Returns(false);

            var mockExecutionLogPublisher = new Mock<IExecutionLogPublisher>();
            var mockResumptionFactory = new Mock<IResumptionFactory>();
            mockResumptionFactory.Setup(o => o.New(mockExecutionLogPublisher.Object))
                .Returns(mockIResumption.Object);

            var performingContextMock = new PerformContextMock(jobId, _values);

            var sut = new WarewolfResumeWorkflow(mockExecutionLogPublisher.Object, new PerformingContext(performingContextMock.Object), mockResumptionFactory.Object);

            Assert.ThrowsException<InvalidOperationException>(() => sut.PerformResumption());
            
            mockExecutionLogPublisher.Verify(o => o.Error("Failed to perform job {dc80a09e-21f3-4f92-9ec0-6d3b348e49fa}, could not establish a connection.", new object[] { "dc80a09e-21f3-4f92-9ec0-6d3b348e49fa" }), Times.Once);
            mockExecutionLogPublisher.Verify(o => o.Error("Failed to perform job {dc80a09e-21f3-4f92-9ec0-6d3b348e49fa}"), Times.Once); //PBI: this duplicate the above message
            mockIResumption.Verify(o => o.Connect(), Times.Once);
        }

        [TestMethod]
        [TestCategory(nameof(WarewolfResumeWorkflow))]
        [Owner("Siphamandla Dube")]
        public void WarewolfResumeWorkflow_PerformResumption_Given_ConnectionSuccess_Then_ResumeThrows_ShouldThrowInvalidOperationException()
        {
            var jobId = Guid.Parse("dc80a09e-21f3-4f92-9ec0-6d3b348e49fa").ToString();
            var resumeFalseExceptionMessage = "test resume exception";
            var resumeFalseInnerExceptionMessage = "test resume Inner exception";

            var mockIResumption = new Mock<IResumption>();
            mockIResumption.Setup(o => o.Connect())
                .Returns(true);
            mockIResumption.Setup(o => o.Resume(_values))
                .Throws(new Exception(resumeFalseExceptionMessage, new Exception(resumeFalseInnerExceptionMessage)));

            var mockExecutionLogPublisher = new Mock<IExecutionLogPublisher>();
            var mockResumptionFactory = new Mock<IResumptionFactory>();
            mockResumptionFactory.Setup(o => o.New(mockExecutionLogPublisher.Object))
                .Returns(mockIResumption.Object);

            var performingContextMock = new PerformContextMock(jobId, _values);

            var sut = new WarewolfResumeWorkflow(mockExecutionLogPublisher.Object, new PerformingContext(performingContextMock.Object), mockResumptionFactory.Object);

            Assert.ThrowsException<Exception>(() => sut.PerformResumption(), resumeFalseExceptionMessage);

            mockExecutionLogPublisher.Verify(o => o.Info("Performing Resume of job {dc80a09e-21f3-4f92-9ec0-6d3b348e49fa}, connection established.", new object [] {"dc80a09e-21f3-4f92-9ec0-6d3b348e49fa"}), Times.Once);
            mockExecutionLogPublisher.Verify(o => o.Error("Failed to perform job {dc80a09e-21f3-4f92-9ec0-6d3b348e49fa}System.Exception: test resume Inner exception"), Times.Once); //PBI: notice the System.Exception with our inner exception
            mockIResumption.Verify(o => o.Connect(), Times.Once);
        }

        [TestMethod]
        [TestCategory(nameof(WarewolfResumeWorkflow))]
        [Owner("Siphamandla Dube")]
        public void WarewolfResumeWorkflow_PerformResumption_Given_ConnectionSuccess_Then_ResumeFails_ShouldThrowInvalidOperationException()
        {
            var jobId = Guid.Parse("dc80a09e-21f3-4f92-9ec0-6d3b348e49fa").ToString();
            var resumeFalseFailureMessage = "test resume false failure";

            var mockIResumption = new Mock<IResumption>();
            mockIResumption.Setup(o => o.Connect())
                .Returns(true);
            mockIResumption.Setup(o => o.Resume(_values))
                .Returns(new Dev2.Communication.ExecuteMessage { HasError = true, Message = new StringBuilder(resumeFalseFailureMessage) });

            var mockExecutionLogPublisher = new Mock<IExecutionLogPublisher>();
            var mockResumptionFactory = new Mock<IResumptionFactory>();
            mockResumptionFactory.Setup(o => o.New(mockExecutionLogPublisher.Object))
                .Returns(mockIResumption.Object);

            var performingContextMock = new PerformContextMock(jobId, _values);

            var sut = new WarewolfResumeWorkflow(mockExecutionLogPublisher.Object, new PerformingContext(performingContextMock.Object), mockResumptionFactory.Object);

            Assert.ThrowsException<InvalidOperationException>(() => sut.PerformResumption(), resumeFalseFailureMessage);

            mockExecutionLogPublisher.Verify(o => o.Info("Performing Resume of job {dc80a09e-21f3-4f92-9ec0-6d3b348e49fa}, connection established.", new object[] { "dc80a09e-21f3-4f92-9ec0-6d3b348e49fa" }), Times.Once);
            mockExecutionLogPublisher.Verify(o => o.Error("Failed to perform job {dc80a09e-21f3-4f92-9ec0-6d3b348e49fa}System.Exception: test resume false failure"), Times.Once); //PBI: notice the System.Exception with our inner exception
            mockIResumption.Verify(o => o.Connect(), Times.Once);
        }
    }
}
