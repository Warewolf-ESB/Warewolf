using System;
using System.IO;
using System.Security.Principal;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Wrappers;
using Dev2.Interfaces;
using Dev2.Runtime.ESB.Execution;
using Dev2.Runtime.ESB.Execution.State;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Storage;
using System.Linq;

namespace Dev2.Tests.Runtime.ESB.Execution
{
    [TestClass]
    public class Dev2StateLoggerTests
    {
        IFile _fileWrapper;
        IDirectory _directoryWrapper;
        Dev2StateAuditLogger _dev2StateAuditLogger;
        Mock<IDev2Activity> _activity;

        [TestCleanup]
        public void Cleanup()
        {
            if (_directoryWrapper == null)
            {
                _directoryWrapper = new DirectoryWrapper();
            }
            _directoryWrapper.Delete(EnvironmentVariables.DetailLogPath, true);
        }
        [TestMethod, DeploymentItem("EnableDocker.txt")]
        public void StateNotifier_SubscribeToEventNotifications_Tests()
        {
            TestSetup(out _fileWrapper, out _directoryWrapper, out _activity);
            var nextActivityMock = new Mock<IDev2Activity>();
            var nextActivity = nextActivityMock.Object;
            var exception = new Exception("some exception");
            var message = new { Message = "Some Message" };
            var detailMethodName = nameof(StateNotifier_SubscribeToEventNotifications_Tests);

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

        [TestMethod, DeploymentItem("EnableDocker.txt")]
        public void Dev2StateAuditLogger_LogExecuteCompleteState_Tests()
        {
            var expectedWorkflowId = Guid.NewGuid();
            var expectedExecutionId = Guid.NewGuid();
            var nextActivity = new Mock<IDev2Activity>();
            var expectedWorkflowName = "LogExecuteCompleteState_Workflow";
            TestAuditSetupWithAssignedInputs(expectedWorkflowId, expectedWorkflowName, expectedExecutionId, out _dev2StateAuditLogger, out _activity);
            // test
            _dev2StateAuditLogger.LogExecuteCompleteState(nextActivity.Object);

            // verify
            var str = expectedWorkflowId.ToString();
            var results = Dev2StateAuditLogger.Query(a => (a.WorkflowID.Equals(str)
                || a.WorkflowName.Equals("LogExecuteCompleteState")
                || a.ExecutionID.Equals("")
                || a.AuditType.Equals("")));
            _dev2StateAuditLogger.Dispose();

            Assert.IsTrue(results.FirstOrDefault(a => a.WorkflowID == str) != null);
        }

        [TestMethod, DeploymentItem("EnableDocker.txt")]
        public void Dev2StateAuditLogger_LogExecuteException_Tests()
        {
            var expectedWorkflowId = Guid.NewGuid();
            var expectedExecutionId = Guid.NewGuid();
            var expectedWorkflowName = "LogExecuteException_Workflow";
            TestAuditSetupWithAssignedInputs(expectedWorkflowId, expectedWorkflowName, expectedExecutionId, out _dev2StateAuditLogger, out _activity);
            // test
            var activity = new Mock<IDev2Activity>();
            var exception = new Mock<Exception>();
            _dev2StateAuditLogger.LogExecuteException(exception.Object, activity.Object);

            // verify
            var str = expectedWorkflowId.ToString();
            var results = Dev2StateAuditLogger.Query(a => (a.WorkflowID.Equals(str)
                || a.WorkflowName.Equals("LogExecuteException")
                || a.ExecutionID.Equals("")
                || a.AuditType.Equals("")));
            _dev2StateAuditLogger.Dispose();

            Assert.IsTrue(results.FirstOrDefault(a => a.WorkflowID == str) != null);
        }

        [TestMethod, DeploymentItem("EnableDocker.txt")]
        public void Dev2StateAuditLogger_LogPostExecuteState_Tests()
        {
            var expectedWorkflowId = Guid.NewGuid();
            var expectedExecutionId = Guid.NewGuid();
            var expectedWorkflowName = "LogPostExecuteState_Workflow";
            TestAuditSetupWithAssignedInputs(expectedWorkflowId, expectedWorkflowName, expectedExecutionId, out _dev2StateAuditLogger, out _activity);
            // test
            var previousActivity = new Mock<IDev2Activity>();
            var nextActivity = new Mock<IDev2Activity>();
            _dev2StateAuditLogger.LogPostExecuteState(previousActivity.Object, nextActivity.Object);

            // verify
            var str = expectedWorkflowId.ToString();
            var results = Dev2StateAuditLogger.Query(a => (a.WorkflowID.Equals(str)
                || a.WorkflowName.Equals("LogPostExecuteState")
                || a.ExecutionID.Equals("")
                || a.AuditType.Equals("")));
            _dev2StateAuditLogger.Dispose();

            Assert.IsTrue(results.FirstOrDefault(a => a.WorkflowID == str) != null);
        }

        [TestMethod, DeploymentItem("EnableDocker.txt")]
        public void Dev2StateAuditLogger_LogAdditionalDetail_Tests()
        {
            var expectedWorkflowId = Guid.NewGuid();
            var expectedExecutionId = Guid.NewGuid();
            var expectedWorkflowName = "LogAdditionalDetail_Workflow";
            TestAuditSetupWithAssignedInputs(expectedWorkflowId, expectedWorkflowName, expectedExecutionId, out _dev2StateAuditLogger, out _activity);
            // test
            var additionalDetailObject = new { Message = "Some Message" };
            _dev2StateAuditLogger.LogAdditionalDetail(additionalDetailObject, "");


            // verify
            var str = expectedWorkflowId.ToString();
            var results = Dev2StateAuditLogger.Query(a => (a.WorkflowID.Equals(str)
                || a.WorkflowName.Equals("LogAdditionalDetail")
                || a.ExecutionID.Equals("")
                || a.AuditType.Equals("")));
            _dev2StateAuditLogger.Dispose();

            Assert.IsTrue(results.FirstOrDefault(a => a.WorkflowID == str) != null);
        }

        [TestMethod, DeploymentItem("EnableDocker.txt")]
        public void Dev2StateAuditLogger_LogPreExecuteState_Tests()
        {
            var expectedWorkflowId = Guid.NewGuid();
            var expectedExecutionId = Guid.NewGuid();
            var expectedWorkflowName = "LogPreExecuteState_Workflow";
            TestAuditSetupWithAssignedInputs(expectedWorkflowId, expectedWorkflowName, expectedExecutionId, out _dev2StateAuditLogger, out _activity);
            // test
            _dev2StateAuditLogger.LogPreExecuteState(_activity.Object);
            // verify
            var str = expectedWorkflowId.ToString();
            var results = Dev2StateAuditLogger.Query(item => true);
            _dev2StateAuditLogger.Dispose();

            foreach (var item in results)
            {

            }

            var result = results.FirstOrDefault(a => a.WorkflowID == str);
            Assert.IsTrue(result != null);
            Assert.AreEqual("{\"Environment\":{\"scalars\":{},\"record_sets\":{},\"json_objects\":{}},\"Errors\":[],\"AllErrors\":[]}",
                            result.Environment);
        }
        
        private static void TestSetup(out IFile fileWrapper, out IDirectory directoryWrapper, out Mock<IDev2Activity> activity)
        {
            // setup
            Mock<IDSFDataObject> mockedDataObject = SetupDataObject();
            fileWrapper = new FileWrapper();
            directoryWrapper = new DirectoryWrapper();
            activity = new Mock<IDev2Activity>();
        }
        
        private static Dev2StateAuditLogger GetDev2AuditStateLogger(Mock<IDSFDataObject> mockedDataObject)
        {
            return new Dev2StateAuditLogger(mockedDataObject.Object);
        }

        private static void TestAuditSetupWithAssignedInputs(Guid resourceId, string workflowName, Guid executionId, out Dev2StateAuditLogger dev2AuditStateLogger, out Mock<IDev2Activity> activity)
        {
            // setup
            Mock<IDSFDataObject> mockedDataObject = SetupDataObjectWithAssignedInputs(resourceId, workflowName, executionId);

            activity = new Mock<IDev2Activity>();
            dev2AuditStateLogger = GetDev2AuditStateLogger(mockedDataObject);
        }

        private static Mock<IDSFDataObject> SetupDataObjectWithAssignedInputs(Guid resourceId, string workflowName, Guid executionId)
        {
            // mocks
            var mockedDataObject = new Mock<IDSFDataObject>();
            mockedDataObject.Setup(o => o.Environment).Returns(() => new ExecutionEnvironment());
            mockedDataObject.Setup(o => o.ServiceName).Returns(() => workflowName);
            mockedDataObject.Setup(o => o.ResourceID).Returns(() => resourceId);
            mockedDataObject.Setup(o => o.ExecutionID).Returns(() => executionId);
            var principal = new Mock<IPrincipal>();
            principal.Setup(o => o.Identity).Returns(() => new Mock<IIdentity>().Object);
            mockedDataObject.Setup(o => o.ExecutingUser).Returns(() => principal.Object);
            mockedDataObject.Setup(o => o.ExecutionToken).Returns(() => new Mock<IExecutionToken>().Object);
            return mockedDataObject;
        }



        private static Mock<IDSFDataObject> SetupDataObject()
        {
            // mocks
            var mockedDataObject = new Mock<IDSFDataObject>();
            mockedDataObject.Setup(o => o.Environment).Returns(() => new ExecutionEnvironment());
            mockedDataObject.Setup(o => o.ServiceName).Returns(() => "Some Workflow");
            mockedDataObject.Setup(o => o.ResourceID).Returns(() => Guid.NewGuid());
            var principal = new Mock<IPrincipal>();
            principal.Setup(o => o.Identity).Returns(() => new Mock<IIdentity>().Object);
            mockedDataObject.Setup(o => o.ExecutingUser).Returns(() => principal.Object);
            mockedDataObject.Setup(o => o.ExecutionToken).Returns(() => new Mock<IExecutionToken>().Object);
            return mockedDataObject;
        }
    }
}