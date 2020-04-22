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
using Newtonsoft.Json;
using System;
using System.Security.Principal;
using Warewolf.Interfaces.Auditing;
using Warewolf.Storage;

namespace Warewolf.Auditing.Tests
{
    [TestClass]
    public class IStateAuditLoggerTests
    {
        IDSFDataObject _dSFDataObject;
        Mock<IDev2Activity> _activity;
        IStateAuditLogger _stateAuditLogger;
        IFile _fileWrapper;
        IDirectory _directoryWrapper;
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
            stateLoggerMock.Setup(o => o.LogExecuteException(exception, nextActivity)).Verifiable();
            stateLoggerMock.Setup(o => o.LogAdditionalDetail(message, detailMethodName)).Verifiable();
            stateLoggerMock.Setup(o => o.LogExecuteCompleteState(nextActivity)).Verifiable();
            stateLoggerMock.Setup(o => o.LogStopExecutionState(nextActivity)).Verifiable();
            var listener = stateLoggerMock.Object;
            // test
            notifier.Subscribe(listener);
            
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
        public void IStateAuditLogger_LogExecuteCompleteState_Tests()
        {
            var expectedWorkflowId = Guid.NewGuid();
            var nextActivity = new Mock<IDev2Activity>();
            var expectedWorkflowName = "LogExecuteCompleteState";

            var mockWebSocketPool = new Mock<IWebSocketPool>();
            mockWebSocketPool.Setup(o => o.Acquire(It.IsAny<string>())).Returns(new Mock<IWebSocketWrapper>().Object).Verifiable(); ;
            TestAuditSetupWithAssignedInputs(expectedWorkflowId, expectedWorkflowName, out _stateAuditLogger, out _activity, mockWebSocketPool.Object);

            //------------------------------Act------------------------------------
            _stateAuditLogger.NewStateListener(_dSFDataObject).LogExecuteCompleteState(nextActivity.Object);

            mockWebSocketPool.VerifyAll();
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(IStateAuditLogger))]
        public void IStateAuditLogger_LogPreExecuteState_Tests()
        {
            var expectedWorkflowId = Guid.NewGuid();
            var nextActivity = new Mock<IDev2Activity>();
            var expectedWorkflowName = "LogActivityExecuteState";

            var mockWebSocketPool = new Mock<IWebSocketPool>();
            mockWebSocketPool.Setup(o => o.Acquire(It.IsAny<string>())).Returns(new Mock<IWebSocketWrapper>().Object).Verifiable();
            TestAuditSetupWithAssignedInputs(expectedWorkflowId, expectedWorkflowName, out _stateAuditLogger, out _activity, mockWebSocketPool.Object);

            //------------------------------Act------------------------------------
            _stateAuditLogger.NewStateListener(_dSFDataObject).LogActivityExecuteState(nextActivity.Object);

            mockWebSocketPool.VerifyAll();
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(IStateAuditLogger))]
        public void IStateAuditLogger_LogStopExecutionState_Tests()
        {
            var expectedWorkflowId = Guid.NewGuid();
            var nextActivity = new Mock<IDev2Activity>();
            var expectedWorkflowName = "LogStopExecutionState";

            var mockWebSocketPool = new Mock<IWebSocketPool>();
            mockWebSocketPool.Setup(o => o.Acquire(It.IsAny<string>())).Returns(new Mock<IWebSocketWrapper>().Object).Verifiable(); ;
            TestAuditSetupWithAssignedInputs(expectedWorkflowId, expectedWorkflowName, out _stateAuditLogger, out _activity, mockWebSocketPool.Object);

            //------------------------------Act------------------------------------
            _stateAuditLogger.NewStateListener(_dSFDataObject).LogStopExecutionState(nextActivity.Object);

            mockWebSocketPool.VerifyAll();
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(IStateAuditLogger))]
        public void IStateAuditLogger_LogAdditionalDetail_Tests()
        {
            var expectedWorkflowId = Guid.NewGuid();
            var nextActivity = new Mock<IDev2Activity>();
            var expectedWorkflowName = "LogAdditionalDetail";

            var mockWebSocketPool = new Mock<IWebSocketPool>();
            mockWebSocketPool.Setup(o => o.Acquire(It.IsAny<string>())).Returns(new Mock<IWebSocketWrapper>().Object).Verifiable(); ;
            TestAuditSetupWithAssignedInputs(expectedWorkflowId, expectedWorkflowName, out _stateAuditLogger, out _activity, mockWebSocketPool.Object);

            //------------------------------Act------------------------------------
            var additionalDetailObject = new { Message = "Some Message" };
            _stateAuditLogger.NewStateListener(_dSFDataObject).LogAdditionalDetail(additionalDetailObject, "");

            mockWebSocketPool.VerifyAll();
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(IStateAuditLogger))]
        public void IStateAuditLogger_LogExecuteException_LogsTheGivenException_Success()
        {
            //------------------------------Arrange--------------------------------
            var expectedWorkflowId = Guid.NewGuid();
            var expectedWorkflowName = "LogExecuteCompleteState_Workflow";
            var expectedException = new Exception("this is a test exception message");

            var mockWebSocketWrapper = new Mock<IWebSocketWrapper>();
            var mockNextActivity = new Mock<IDev2Activity>();
            var mockWebSocketPool = new Mock<IWebSocketPool>();

            mockWebSocketPool.Setup(o => o.Acquire(It.IsAny<string>())).Returns(mockWebSocketWrapper.Object);

            TestAuditSetupWithAssignedInputs(expectedWorkflowId, expectedWorkflowName, out _stateAuditLogger, out _activity, mockWebSocketPool.Object);

            //------------------------------Act------------------------------------
            _stateAuditLogger.NewStateListener(_dSFDataObject).LogExecuteException(expectedException, mockNextActivity.Object);
            var expectedJsonAudit = "{\"Id\":0,\"WorkflowID\":\"4b412ed9-dac0-47ce-bd04-f8f55826b835\",\"ExecutionID\":\"f15124b5-df69-47c2-b1c8-af6629b05e5c\",\"ExecutionOrigin\":0,\"IsSubExecution\":false,\"IsRemoteWorkflow\":false,\"WorkflowName\":\"LogExecuteCompleteState_Workflow\",\"AuditType\":\"LogExecuteException\",\"PreviousActivity\":null,\"PreviousActivityType\":\"Castle.Proxies.IDev2ActivityProxy\",\"PreviousActivityID\":null,\"NextActivity\":null,\"NextActivityType\":null,\"NextActivityID\":null,\"ServerID\":\"00000000-0000-0000-0000-000000000000\",\"ParentID\":\"00000000-0000-0000-0000-000000000000\",\"ExecutingUser\":\"Mock<IPrincipal:1>.Object\",\"ExecutionOriginDescription\":null,\"ExecutionToken\":\"null\",\"AdditionalDetail\":\"this is a test exception message\",\"Environment\":\"{\\\"Environment\\\":{\\\"scalars\\\":{},\\\"record_sets\\\":{},\\\"json_objects\\\":{}},\\\"Errors\\\":[],\\\"AllErrors\\\":[]}\",\"VersionNumber\":\"0\",\"AuditDate\":\"2019-09-19T11:49:15.7016208+02:00\",\"Exception\":{\"ClassName\":\"System.Exception\",\"Message\":\"this is a test exception message\",\"Data\":null,\"InnerException\":null,\"HelpURL\":null,\"StackTraceString\":null,\"RemoteStackTraceString\":null,\"RemoteStackIndex\":0,\"ExceptionMethod\":null,\"HResult\":-2146233088,\"Source\":null,\"WatsonBuckets\":null}}";
            var actualAudit = JsonConvert.DeserializeObject<Audit>(expectedJsonAudit);

            //------------------------------Assert---------------------------------
            Assert.AreEqual(expected: expectedException.Message, actual: actualAudit.Exception.Message);
        }

        IStateAuditLogger GetIAuditStateLogger(IWebSocketPool webSocketPool) => new StateAuditLogger(webSocketPool);

        void TestSetup(out IFile fileWrapper, out IDirectory directoryWrapper, out Mock<IDev2Activity> activity)
        {
            SetupDataObject();
            fileWrapper = new FileWrapper();
            directoryWrapper = new DirectoryWrapper();
            activity = new Mock<IDev2Activity>();
        }

        Mock<IDSFDataObject> SetupDataObjectWithAssignedInputs(Guid resourceId, string workflowName, Guid executionId)
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

        Mock<IDSFDataObject> SetupDataObject()
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

        void TestAuditSetupWithAssignedInputs(Guid resourceId, string workflowName, out IStateAuditLogger auditStateLogger, out Mock<IDev2Activity> activity, IWebSocketPool webSocketPool)
        {
            GetMockedDataObject(resourceId, workflowName, out activity, out Mock<IDSFDataObject> mockedDataObject);
            auditStateLogger = GetIAuditStateLogger(webSocketPool);
        }

        void GetMockedDataObject(Guid resourceId, string workflowName, out Mock<IDev2Activity> activity, out Mock<IDSFDataObject> mockedDataObject)
        {
            var executionId = Guid.NewGuid();
            mockedDataObject = SetupDataObjectWithAssignedInputs(resourceId, workflowName, executionId);
            activity = new Mock<IDev2Activity>();
        }
    }
}
