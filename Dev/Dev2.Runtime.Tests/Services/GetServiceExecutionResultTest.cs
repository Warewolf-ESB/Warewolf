using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.Runtime.ESB.Management.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Common;
using Moq;
using Dev2.Interfaces;
using Warewolf.Storage;
using System.Security.Principal;
using Warewolf.Auditing;
using Warewolf.Common.NetStandard20;
using Warewolf.OS;
using System.Threading.Tasks;
using System.Threading;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class GetServiceExecutionResultTest
	{
		IStateListener _dev2StateAuditLogger;
		static IStateListener GetDev2AuditStateLogger(Mock<IDSFDataObject> mockedDataObject)
        {
            return new StateAuditLogger(new WebSocketPool()).NewStateListener(mockedDataObject.Object);
        }

        static IStateListener TestAuditSetupWithAssignedInputs(Guid resourceId, string workflowName, Guid executionId)
        {
            // setup
            Mock<IDSFDataObject> mockedDataObject = SetupDataObjectWithAssignedInputs(resourceId, workflowName, executionId);
            return GetDev2AuditStateLogger(mockedDataObject);
        }

        static Mock<IDSFDataObject> SetupDataObjectWithAssignedInputs(Guid resourceId, string workflowName, Guid executionId)
        {
            // mocks
            var mockedDataObject = new Mock<IDSFDataObject>();
            mockedDataObject.Setup(o => o.Environment).Returns(() => new ExecutionEnvironment());
            mockedDataObject.Setup(o => o.ServiceName).Returns(() => workflowName);
            mockedDataObject.Setup(o => o.ResourceID).Returns(() => resourceId);
            mockedDataObject.Setup(o => o.ExecutionID).Returns(() => executionId);
			mockedDataObject.Setup(o => o.WebUrl).Returns(() => "http://localhost:3142/secure/" + workflowName + ".json");
			var principal = new Mock<IPrincipal>();
            principal.Setup(o => o.Identity).Returns(() => new Mock<IIdentity>().Object);
            mockedDataObject.Setup(o => o.ExecutingUser).Returns(() => principal.Object);
            return mockedDataObject;
        }

		void CreateLogDataToTestWith(Guid expectedWorkflowId, Guid expectedExecutionId, string expectedWorkflowName)
		{
			var mockLoggingServiceMonitorWithRestart = new LoggingServiceMonitorWithRestart(new Mock<ChildProcessTrackerWrapper>().Object, new Mock<ProcessWrapperFactory>().Object);
			var webSocketPool = new WebSocketPool();
			var mockExecutionLoggerFactory = new Mock<ExecutionLogger.IExecutionLoggerFactory>();
			var config = new StartupConfiguration
			{
				LoggingServiceMonitor = mockLoggingServiceMonitorWithRestart,
				WebSocketPool = webSocketPool,
				LoggerFactory = mockExecutionLoggerFactory.Object
			};
			var loggingProcessMonitor = config.LoggingServiceMonitor;
			loggingProcessMonitor.Start();
			bool isConnectedOkay;
            do
            {
                var webSocketWrapper = webSocketPool.Acquire(Config.Auditing.Endpoint);
                isConnectedOkay = webSocketWrapper.IsOpen();
            } while (!isConnectedOkay);
            Assert.IsTrue(isConnectedOkay);
            
			_dev2StateAuditLogger = TestAuditSetupWithAssignedInputs(expectedWorkflowId, expectedWorkflowName, expectedExecutionId);
			_dev2StateAuditLogger.LogExecuteCompleteState(new Audit()
			{
				ExecutionID = expectedExecutionId.ToString(),
				WorkflowID = expectedWorkflowId.ToString(),
				WorkflowName = expectedWorkflowName,
				Url = "http://localhost:3142/secure/" + expectedWorkflowName + ".json"
			});
			Thread.Sleep(10000);
		}

        [TestCleanup]
        public void Cleanup()
        {
            if (_dev2StateAuditLogger != null)
            {
                _dev2StateAuditLogger.Dispose();
            }
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("GetResourceID")]
        public void GetServiceExecutionResult_GetResourceID_ShouldReturnEmptyGuid()
        {
            //------------Setup for test--------------------------
            var getLogDataService = new GetServiceExecutionResult();
            //------------Execute Test---------------------------
            var resId = getLogDataService.GetResourceID(new Dictionary<string, StringBuilder>());
            //------------Assert Results-------------------------
            Assert.AreEqual(Guid.Empty, resId);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("GetResourceID")]
        public void GetServiceExecutionResult_GetAuthorizationContextForService_ShouldReturnContext()
        {
            //------------Setup for test--------------------------
            var getLogDataService = new GetServiceExecutionResult();
            //------------Execute Test---------------------------
            var resId = getLogDataService.GetAuthorizationContextForService();
            //------------Assert Results-------------------------
            Assert.AreEqual(AuthorizationContext.Administrator, resId);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("GetLogDataService_HandlesType")]
        public void GetServiceExecutionResult_HandlesType_ExpectName()
        {
            //------------Setup for test--------------------------
            var getLogDataService = new GetServiceExecutionResult();
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.AreEqual("GetServiceExecutionResult", getLogDataService.HandlesType());
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("GetLogDataService_Execute")]
        public void GetServiceExecutionResult_Execute_WithExecutionId_ShouldFilterLogData()
		{
			//------------Setup for test--------------------------
			var expectedWorkflowId = Guid.NewGuid();
			var expectedExecutionId = Guid.NewGuid();
			var expectedWorkflowName = "LogExecuteCompleteState_Workflow";
			CreateLogDataToTestWith(expectedWorkflowId, expectedExecutionId, expectedWorkflowName);

			var stringBuilders = new Dictionary<string, StringBuilder> { { "ExecutionID", new StringBuilder(expectedExecutionId.ToString()) } };
			var serializer = new Dev2JsonSerializer();
			var getLogDataService = new GetServiceExecutionResult();
			//---------------Assert Precondition----------------
			Assert.IsNotNull(getLogDataService);
			//------------Execute Test---------------------------
			var logEntriesJson = getLogDataService.Execute(stringBuilders, null);
			//---------------Assert Postconditions----------------
			Assert.IsNotNull(logEntriesJson);
			var logEntriesObject = serializer.Deserialize<Common.LogEntry>(logEntriesJson.ToString());
			Assert.AreEqual(expectedExecutionId.ToString(), logEntriesObject.ExecutionId);

		}

		[TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("GetLogDataService_Execute")]
        public void GetServiceExecutionResult_Execute_WithExecutionId_ShouldFilterLogData_NewFormat()
        {
            //------------Setup for test--------------------------
			var expectedWorkflowId = Guid.NewGuid();
			var expectedExecutionId = Guid.NewGuid();
			var expectedWorkflowName = "LogExecuteStartState_Workflow";
			CreateLogDataToTestWith(expectedWorkflowId, expectedExecutionId, expectedWorkflowName);

			var getLogDataService = new GetServiceExecutionResult();
			//---------------Assert Precondition----------------
			Assert.IsNotNull(getLogDataService);
			//------------Execute Test---------------------------
			var stringBuilders = new Dictionary<string, StringBuilder> { { "ExecutionID", new StringBuilder(expectedExecutionId.ToString()) } };
            var logEntriesJson = getLogDataService.Execute(stringBuilders, null);

            var serializer = new Dev2JsonSerializer();
            var logEntry = serializer.Deserialize<Common.LogEntry>(logEntriesJson.ToString());
            Assert.IsNotNull(logEntry);
        }
    }
}
