/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/
using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Wrappers;
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Security.Principal;
using Warewolf.Storage;

namespace Warewolf.Auditing.Tests
{
    [TestClass]
    public class IStateAuditLoggerTests
    {
        IDSFDataObject _dSFDataObject;
        Mock<IDev2Activity> _activity;
        IFile _fileWrapper;
        IDirectory _directoryWrapper;
        IStateAuditLogger _stateAuditLogger;
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(IStateAuditLogger))]
        public void IStateAuditLogger_SubscribeToEventNotifications_Tests()
        {
            TestSetup(out _fileWrapper, out _directoryWrapper, out _activity);
            var nextActivityMock = new Mock<IDev2Activity>();
            var nextActivity = nextActivityMock.Object;
            var exception = new Exception("some exception");
            var message = new { Message = "Some Message" };
            var detailMethodName = nameof(IStateAuditLogger_SubscribeToEventNotifications_Tests);

            var notifier = new StateNotifier();
            var stateLoggerMock = new Mock<IStateListener>();
            stateLoggerMock.Setup(o => o.LogPreExecuteState(_activity.Object)).Verifiable();
            stateLoggerMock.Setup(o => o.LogPostExecuteState(_activity.Object, nextActivity)).Verifiable();
            stateLoggerMock.Setup(o => o.LogExecuteException(exception, nextActivity)).Verifiable();
            stateLoggerMock.Setup(o => o.LogAdditionalDetail(message, detailMethodName)).Verifiable();
            stateLoggerMock.Setup(o => o.LogExecuteCompleteState(nextActivity)).Verifiable();
            stateLoggerMock.Setup(o => o.LogStopExecutionState(nextActivity)).Verifiable();
            var listener = stateLoggerMock.Object;
            // test
            notifier.Subscribe(listener);

            notifier.LogPreExecuteState(_activity.Object);
            notifier.LogPostExecuteState(_activity.Object, nextActivity);
            notifier.LogExecuteException(exception, nextActivity);
            notifier.LogAdditionalDetail(message, detailMethodName);
            notifier.LogExecuteCompleteState(nextActivity);
            notifier.LogStopExecutionState(nextActivity);

            // verify
            stateLoggerMock.Verify();
            notifier.Dispose();
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(IStateAuditLogger))]
        public void Dev2StateAuditLogger_LogExecuteCompleteState_Tests()
        {
            var expectedWorkflowId = Guid.NewGuid();
            var nextActivity = new Mock<IDev2Activity>();
            var expectedWorkflowName = "LogExecuteCompleteState_Workflow";
            TestAuditSetupWithAssignedInputs(expectedWorkflowId, expectedWorkflowName, out _stateAuditLogger, out _activity);
            // test
            _stateAuditLogger.NewStateListener(_dSFDataObject).LogExecuteCompleteState(nextActivity.Object);
     

            // verify
            //var str = expectedWorkflowId.ToString();
            //var results = Dev2StateAuditLogger.Query(a => (a.WorkflowID.Equals(str)
            //    || a.WorkflowName.Equals("LogExecuteCompleteState")
            //    || a.ExecutionID.Equals("")
            //    || a.AuditType.Equals("")));
            //_dev2StateAuditLogger.Dispose();

            //Assert.IsTrue(results.FirstOrDefault(a => a.WorkflowID == str) != null);
        }

        private IStateAuditLogger GetIAuditStateLogger(Mock<IDSFDataObject> mockedDataObject)
        {
            return new StateAuditLogger();
        }
        private void TestSetup(out IFile fileWrapper, out IDirectory directoryWrapper, out Mock<IDev2Activity> activity)
        {
            var mockedDataObject = SetupDataObject();
            fileWrapper = new FileWrapper();
            directoryWrapper = new DirectoryWrapper();
            activity = new Mock<IDev2Activity>();
        }
        private Mock<IDSFDataObject> SetupDataObjectWithAssignedInputs(Guid resourceId, string workflowName, Guid executionId)
        {
            var mockedDataObject = new Mock<IDSFDataObject>();
            mockedDataObject.Setup(o => o.Environment).Returns(() => new ExecutionEnvironment());
            mockedDataObject.Setup(o => o.ServiceName).Returns(() => workflowName);
            mockedDataObject.Setup(o => o.ResourceID).Returns(() => resourceId);
            mockedDataObject.Setup(o => o.ExecutionID).Returns(() => executionId);
            var principal = new Mock<IPrincipal>();
            principal.Setup(o => o.Identity).Returns(() => new Mock<IIdentity>().Object);
            mockedDataObject.Setup(o => o.ExecutingUser).Returns(() => principal.Object);
            mockedDataObject.Setup(o => o.ExecutionToken).Returns(() => new Mock<IExecutionToken>().Object);

            _dSFDataObject = mockedDataObject.Object;

            return mockedDataObject;
        }
        private Mock<IDSFDataObject> SetupDataObject()
        {
            var mockedDataObject = new Mock<IDSFDataObject>();
            mockedDataObject.Setup(o => o.Environment).Returns(() => new ExecutionEnvironment());
            mockedDataObject.Setup(o => o.ServiceName).Returns(() => "Some Workflow");
            mockedDataObject.Setup(o => o.ResourceID).Returns(() => Guid.NewGuid());
            var principal = new Mock<IPrincipal>();
            principal.Setup(o => o.Identity).Returns(() => new Mock<IIdentity>().Object);
            mockedDataObject.Setup(o => o.ExecutingUser).Returns(() => principal.Object);
            mockedDataObject.Setup(o => o.ExecutionToken).Returns(() => new Mock<IExecutionToken>().Object);

            _dSFDataObject = mockedDataObject.Object;

            return mockedDataObject;
        }
        private void TestAuditSetupWithAssignedInputs(Guid resourceId, string workflowName, out IStateAuditLogger auditStateLogger, out Mock<IDev2Activity> activity)
        {
            GetMockedDataObject(resourceId, workflowName, out activity, out Mock<IDSFDataObject> mockedDataObject);
            auditStateLogger = GetIAuditStateLogger(mockedDataObject);
        }
        private void GetMockedDataObject(Guid resourceId, string workflowName, out Mock<IDev2Activity> activity, out Mock<IDSFDataObject> mockedDataObject)
        {
            var executionId = Guid.NewGuid();
            mockedDataObject = SetupDataObjectWithAssignedInputs(resourceId, workflowName, executionId);
            activity = new Mock<IDev2Activity>();
        }
    }
}
