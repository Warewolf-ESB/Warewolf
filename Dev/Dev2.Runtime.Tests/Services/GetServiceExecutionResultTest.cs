using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.Runtime.ESB.Management.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Common;
using Dev2.Runtime.Auditing;
using Moq;
using Dev2.Interfaces;
using Warewolf.Storage;
using System.Security.Principal;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class GetServiceExecutionResultTest
    {
        Dev2StateAuditLogger _dev2StateAuditLogger;
        Mock<IDev2Activity> _activity;

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
            return mockedDataObject;
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
            var nextActivity = new Mock<IDev2Activity>();
            var expectedWorkflowName = "LogExecuteCompleteState_Workflow";
            TestAuditSetupWithAssignedInputs(expectedWorkflowId, expectedWorkflowName, expectedExecutionId, out _dev2StateAuditLogger, out _activity);
            _dev2StateAuditLogger.LogExecuteCompleteState(nextActivity.Object);

            var getLogDataService = new GetServiceExecutionResult();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(getLogDataService);
            //------------Execute Test---------------------------
            var stringBuilders = new Dictionary<string, StringBuilder> { { "ExecutionId", new StringBuilder("06385e0f-ac27-4cf0-af55-7642c3c08ba3") } };
            var logEntriesJson = getLogDataService.Execute(stringBuilders, null);
            var serializer = new Dev2JsonSerializer();
            Assert.IsNotNull(logEntriesJson);
            var logEntriesObject = serializer.Deserialize<List<AuditLog>>(logEntriesJson.ToString());
            Assert.AreEqual(expectedWorkflowId.ToString(), logEntriesObject[0].WorkflowID);

        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("GetLogDataService_Execute")]
        public void GetServiceExecutionResult_Execute_WithExecutionId_ShouldFilterLogData_NewFormat()
        {
            //------------Setup for test--------------------------
            var getLogDataService = new GetServiceExecutionResult();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(getLogDataService);
            //------------Execute Test---------------------------
            var stringBuilders = new Dictionary<string, StringBuilder> { { "ExecutionId", new StringBuilder("06385e0f-ac27-4cf0-af55-7642c3c08ba3") } };
            var logEntriesJson = getLogDataService.Execute(stringBuilders, null);

            var serializer = new Dev2JsonSerializer();
            var logEntry = serializer.Deserialize<LogEntry>(logEntriesJson.ToString());
            Assert.IsNotNull(logEntry);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("GetLogDataService_GetLogEntryValues")]
        public void GetServiceExecutionResult_GivenLogEntry_ExpectCorrectResult()
        {
            //------------Setup for test--------------------------
            const string logEntry = @"2017-07-13 08:02:52,799 DEBUG - [52cea226-d594-49eb-9c37-0598bd2803f5] - Request URL [ http://RSAKLFPETERB:3142/Examples\Loop Constructs - Select and Apply.XML ]";
            var dataServiceBase = new LogDataServiceBase();
            //---------------Assert Precondition----------------
            Assert.AreEqual(dataServiceBase.GetAuthorizationContextForService(), AuthorizationContext.Administrator);
            //------------Execute Test---------------------------
            var privateObject = new PrivateObject(dataServiceBase);
            var invoke = (string[])privateObject.Invoke("GetLogEntryValues", BindingFlags.NonPublic | BindingFlags.Instance, logEntry);
            Assert.AreEqual(5, invoke.Length);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("GetLogDataService_GetLogEntryValues")]
        public void GetServiceExecutionResult_GivenLogEntry_ExpectCorrectResult_NewFormat()
        {
            //------------Setup for test--------------------------
            const string logEntry = @"2017-07-13 10:16:55,613 DEBUG - [03659971-6b50-42e7-af3e-1177fc2e86ed] - Mapping Inputs from Environment";
            var dataServiceBase = new LogDataServiceBase();
            //---------------Assert Precondition----------------
            Assert.AreEqual(dataServiceBase.GetAuthorizationContextForService(), AuthorizationContext.Administrator);
            //------------Execute Test---------------------------
            var privateObject = new PrivateObject(dataServiceBase);
            var invoke = (string[])privateObject.Invoke("GetLogEntryValues", BindingFlags.NonPublic | BindingFlags.Instance, logEntry);
            Assert.AreEqual(5, invoke.Length);
        }
    }
}