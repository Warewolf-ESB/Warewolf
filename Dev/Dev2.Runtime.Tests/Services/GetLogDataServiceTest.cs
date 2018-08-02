using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.Interfaces;
using Dev2.Runtime.Auditing;
using Dev2.Runtime.ESB.Management.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Warewolf.Storage;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class GetLogDataServiceTest
    {
        Dev2StateAuditLogger _dev2StateAuditLogger;
        Mock<IDev2Activity> _activity;

        [TestCleanup]
        public void Cleanup()
        {
            if (_dev2StateAuditLogger != null)
            {
                _dev2StateAuditLogger.Dispose();
            }
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("GetResourceID")]
        public void GetLogDataService_GetResourceID_ShouldReturnEmptyGuid()
        {
            //------------Setup for test--------------------------
            var getLogDataService = new GetLogDataService();
            //------------Execute Test---------------------------
            var resId = getLogDataService.GetResourceID(new Dictionary<string, StringBuilder>());
            //------------Assert Results-------------------------
            Assert.AreEqual(Guid.Empty, resId);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("GetResourceID")]
        public void GetLogDataService_GetAuthorizationContextForService_ShouldReturnContext()
        {
            //------------Setup for test--------------------------
            var getLogDataService = new GetLogDataService();
            //------------Execute Test---------------------------
            var resId = getLogDataService.GetAuthorizationContextForService();
            //------------Assert Results-------------------------
            Assert.AreEqual(AuthorizationContext.Administrator, resId);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("GetLogDataService_Execute")]
        public void GetLogDataService_Execute_WithLogData_ShouldReturnLogDataObject()
        {
            //------------Setup for test--------------------------
            var serializer = new Dev2JsonSerializer();
            var getLogDataService = new GetLogDataService();
            var expectedWorkflowId = Guid.NewGuid();
            var expectedExecutionId = Guid.NewGuid();
            var nextActivity = new Mock<IDev2Activity>();
            var expectedWorkflowName = "LogExecuteCompleteState_Workflow";
            var principal = new Mock<IPrincipal>();
            principal.Setup(o => o.Identity).Returns(() => new Mock<IIdentity>().Object);
            TestAuditSetupWithAssignedInputs(expectedWorkflowId, expectedWorkflowName, expectedExecutionId, out _dev2StateAuditLogger, out _activity, principal);
            _dev2StateAuditLogger.LogExecuteCompleteState(nextActivity.Object);
            //------------Execute Test---------------------------
            var logEntriesJson = getLogDataService.Execute(new Dictionary<string, StringBuilder>(), null);
            //------------Assert Results-------------------------
            Assert.IsNotNull(logEntriesJson);
            var logEntriesObject = serializer.Deserialize<List<AuditLog>>(logEntriesJson.ToString());
            Assert.IsNotNull(logEntriesObject);

        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        [TestCategory("GetLogDataService_Execute")]
        public void GetLogDataService_Execute_ShouldFilterLogData_WithAuditType()
        {
            var serializer = new Dev2JsonSerializer();
            var getLogDataService = new GetLogDataService();
            var expectedWorkflowId = Guid.NewGuid();
            var expectedExecutionId = Guid.NewGuid();
            var nextActivity = new Mock<IDev2Activity>();
            var expectedWorkflowName = "LogExecuteCompleteState_Workflow";
            var principal = new Mock<IPrincipal>();
            principal.Setup(o => o.Identity).Returns(() => new Mock<IIdentity>().Object);
            TestAuditSetupWithAssignedInputs(expectedWorkflowId, expectedWorkflowName, expectedExecutionId, out _dev2StateAuditLogger, out _activity, principal);
            _dev2StateAuditLogger.LogExecuteCompleteState(nextActivity.Object);
            //---------------Assert Precondition----------------
            var logEntriesJson = getLogDataService.Execute(new Dictionary<string, StringBuilder>(), null);
            Assert.IsNotNull(logEntriesJson);
            //------------Execute Test---------------------------
            var stringBuilders = new Dictionary<string, StringBuilder> { { "AuditType", "LogExecuteCompleteState".ToStringBuilder() } };
            logEntriesJson = getLogDataService.Execute(stringBuilders, null);
            //------------Assert Results-------------------------
            var logEntriesObject = serializer.Deserialize<List<AuditLog>>(logEntriesJson.ToString());
            foreach (var item in logEntriesObject)
            {
                Assert.AreEqual("LogExecuteCompleteState", item.AuditType);
            }
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("GetLogDataService_Execute")]
        public void GetLogDataService_Execute_ShouldFilterLogData_WithExecutingUser()
        {
            var serializer = new Dev2JsonSerializer();
            var getLogDataService = new GetLogDataService();
            var expectedWorkflowId = Guid.NewGuid();
            var expectedExecutionId = Guid.NewGuid();
            var nextActivity = new Mock<IDev2Activity>();
            var expectedWorkflowName = "LogExecuteCompleteState_Workflow";
            var principal = new Mock<IPrincipal>();
            principal.Setup(o => o.Identity).Returns(() => new Mock<IIdentity>().Object);

            TestAuditSetupWithAssignedInputs(expectedWorkflowId, expectedWorkflowName, expectedExecutionId, out _dev2StateAuditLogger, out _activity, principal);
            _dev2StateAuditLogger.LogExecuteCompleteState(nextActivity.Object);
            //---------------Assert Precondition----------------
            var stringBuilders = new Dictionary<string, StringBuilder> { { "ExecutingUser", principal.Object.ToString().ToStringBuilder() } };
            var logEntriesJson = getLogDataService.Execute(stringBuilders, null);
            //------------Assert Results-------------------------
            var logEntriesObject = serializer.Deserialize<List<AuditLog>>(logEntriesJson.ToString());
            foreach (var item in logEntriesObject)
            {
                Assert.AreEqual(principal.Object.ToString(), item.ExecutingUser);
            }
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("GetLogDataService_Execute")]
        public void GetLogDataService_Execute_ShouldFilterLogData_WithExecutionId()
        {
            var serializer = new Dev2JsonSerializer();
            var getLogDataService = new GetLogDataService();
            var expectedWorkflowId = Guid.NewGuid();
            var expectedExecutionId = Guid.NewGuid();
            var nextActivity = new Mock<IDev2Activity>();
            var expectedWorkflowName = "LogExecuteCompleteState_Workflow";
            var principal = new Mock<IPrincipal>();
            principal.Setup(o => o.Identity).Returns(() => new Mock<IIdentity>().Object);
            TestAuditSetupWithAssignedInputs(expectedWorkflowId, expectedWorkflowName, expectedExecutionId, out _dev2StateAuditLogger, out _activity, principal);
            _dev2StateAuditLogger.LogExecuteCompleteState(nextActivity.Object);
            //------------Execute Test---------------------------
            var stringBuilders = new Dictionary<string, StringBuilder> { { "ExecutionID", expectedExecutionId.ToString().ToStringBuilder() } };
            var logEntriesJson = getLogDataService.Execute(stringBuilders, null);
            //------------Assert Results-------------------------
            var logEntriesObject = serializer.Deserialize<List<AuditLog>>(logEntriesJson.ToString());
            foreach (var item in logEntriesObject)
            {
                Assert.AreEqual(expectedExecutionId.ToString(), item.ExecutionID);
            }
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("GetLogDataService_HandlesType")]
        public void GetLogDataService_HandlesType_ExpectName()
        {
            //------------Setup for test--------------------------
            var getLogDataService = new GetLogDataService();
            //------------Assert Results-------------------------
            Assert.AreEqual("GetLogDataService", getLogDataService.HandlesType());
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("GetLogDataService_HandlesType")]
        public void GetLogDataService_CreateServiceEntry_ExpectProperlyFormedDynamicService()
        {
            //------------Setup for test--------------------------
            var getLogDataService = new GetLogDataService();
            //------------Execute Test---------------------------
            var a = getLogDataService.CreateServiceEntry();
            //------------Assert Results-------------------------
            var b = a.DataListSpecification.ToString();
            Assert.AreEqual("<DataList><ResourceType ColumnIODirection=\"Input\"/><Roles ColumnIODirection=\"Input\"/><ResourceName ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>", b);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("GetLogDataService_FromDB_Execute")]
        public void GetLogDataService_Execute_ShouldFilterLogData_WithDatesandWorkflowID()
        {
            //------------Setup for test--------------------------
            var getLogDataService = new GetLogDataService();
            var expectedWorkflowId = Guid.NewGuid();
            var expectedExecutionId = Guid.NewGuid();
            var nextActivity = new Mock<IDev2Activity>();
            var expectedWorkflowName = "LogExecuteCompleteState_Workflow";
            var principal = new Mock<IPrincipal>();
            principal.Setup(o => o.Identity).Returns(() => new Mock<IIdentity>().Object);
            TestAuditSetupWithAssignedInputs(expectedWorkflowId, expectedWorkflowName, expectedExecutionId, out _dev2StateAuditLogger, out _activity, principal);
            _dev2StateAuditLogger.LogExecuteCompleteState(nextActivity.Object);

            //---------------Assert Precondition----------------
            var stringBuilders = new Dictionary<string, StringBuilder>();

            var longStartDateString = DateTime.ParseExact(DateTime.Now.AddMinutes(-5).ToString(GlobalConstants.LogFileDateFormat), GlobalConstants.LogFileDateFormat, System.Globalization.CultureInfo.InvariantCulture);
            var longEndDateString = DateTime.ParseExact(DateTime.Now.AddDays(1).ToString(GlobalConstants.LogFileDateFormat), GlobalConstants.LogFileDateFormat, System.Globalization.CultureInfo.InvariantCulture);
            stringBuilders.Add("StartDateTime", longStartDateString.ToString().ToStringBuilder());
            stringBuilders.Add("WorkflowID", expectedWorkflowId.ToString().ToStringBuilder());
            stringBuilders.Add("CompletedDateTime", longEndDateString.ToString().ToStringBuilder());
            var logEntriesJson = getLogDataService.Execute(stringBuilders, null);
            //------------Assert Results-------------------------
            Assert.IsNotNull(logEntriesJson);
            var serializer = new Dev2JsonSerializer();
            var logEntriesObject = serializer.Deserialize<List<AuditLog>>(logEntriesJson.ToString());
            foreach (var item in logEntriesObject)
            {
                Assert.AreEqual(expectedWorkflowId.ToString(), item.WorkflowID);
            }
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("GetLogDataService_FromDB_Execute")]
        public void GetLogDataService_Execute_ShouldFilterLogData_WithWorkflowID()
        {
            //------------Setup for test--------------------------
            var getLogDataService = new GetLogDataService();
            var expectedWorkflowId = Guid.NewGuid();
            var expectedExecutionId = Guid.NewGuid();
            var nextActivity = new Mock<IDev2Activity>();
            var expectedWorkflowName = "LogExecuteCompleteState_Workflow";
            var principal = new Mock<IPrincipal>();
            principal.Setup(o => o.Identity).Returns(() => new Mock<IIdentity>().Object);
            TestAuditSetupWithAssignedInputs(expectedWorkflowId, expectedWorkflowName, expectedExecutionId, out _dev2StateAuditLogger, out _activity, principal);
            _dev2StateAuditLogger.LogExecuteCompleteState(nextActivity.Object);
            //---------------Assert Precondition----------------
            var stringBuilders = new Dictionary<string, StringBuilder>();
            stringBuilders.Add("WorkflowID", expectedWorkflowId.ToString().ToStringBuilder());
            var logEntriesJson = getLogDataService.Execute(stringBuilders, null);
            //------------Assert Results-------------------------
            Assert.IsNotNull(logEntriesJson);
            var serializer = new Dev2JsonSerializer();
            var logEntriesObject = serializer.Deserialize<List<AuditLog>>(logEntriesJson.ToString());
            foreach (var item in logEntriesObject)
            {
                Assert.AreEqual(expectedWorkflowId.ToString(), item.WorkflowID);
            }
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("GetLogDataService_FromDB_Execute")]
        public void GetLogDataService_Execute_ShouldFilterLogData_WithParentID()
        {
            //------------Setup for test--------------------------
            var getLogDataService = new GetLogDataService();
            var expectedWorkflowId = Guid.NewGuid();
            var parentID = Guid.NewGuid();
            var serverID = Guid.NewGuid();
            var nextActivity = new Mock<IDev2Activity>();
            var expectedWorkflowName = "LogExecuteCompleteState_Workflow";
            var principal = new Mock<IPrincipal>();
            principal.Setup(o => o.Identity).Returns(() => new Mock<IIdentity>().Object);
            TestAuditSetupWithAssignedInputs(expectedWorkflowId, expectedWorkflowName, Guid.NewGuid(), out _dev2StateAuditLogger, out _activity, principal,false,false,serverID, parentID);
            _dev2StateAuditLogger.LogExecuteCompleteState(nextActivity.Object);
            //---------------Assert Precondition----------------
            var stringBuilders = new Dictionary<string, StringBuilder>();
            stringBuilders.Add("ParentID", parentID.ToString().ToStringBuilder());
            var logEntriesJson = getLogDataService.Execute(stringBuilders, null);
            //------------Assert Results-------------------------
            Assert.IsNotNull(logEntriesJson);
            var serializer = new Dev2JsonSerializer();
            var logEntriesObject = serializer.Deserialize<List<AuditLog>>(logEntriesJson.ToString());
            foreach (var item in logEntriesObject)
            {
                Assert.AreEqual(parentID.ToString(), item.ParentID);
            }
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("GetLogDataService_FromDB_Execute")]
        public void GetLogDataService_Execute_ShouldFilterLogData_WithserverID()
        {
            //------------Setup for test--------------------------
            var getLogDataService = new GetLogDataService();
            var expectedWorkflowId = Guid.NewGuid();
            var parentID = Guid.NewGuid();
            var serverID = Guid.NewGuid();
            var nextActivity = new Mock<IDev2Activity>();
            var expectedWorkflowName = "LogExecuteCompleteState_Workflow";
            var principal = new Mock<IPrincipal>();
            principal.Setup(o => o.Identity).Returns(() => new Mock<IIdentity>().Object);
            TestAuditSetupWithAssignedInputs(expectedWorkflowId, expectedWorkflowName, Guid.NewGuid(), out _dev2StateAuditLogger, out _activity, principal, false, false, serverID, parentID);
            _dev2StateAuditLogger.LogExecuteCompleteState(nextActivity.Object);
            //---------------Assert Precondition----------------
            var stringBuilders = new Dictionary<string, StringBuilder>();
            stringBuilders.Add("ServerID", serverID.ToString().ToStringBuilder());
            var logEntriesJson = getLogDataService.Execute(stringBuilders, null);
            //------------Assert Results-------------------------
            Assert.IsNotNull(logEntriesJson);
            var serializer = new Dev2JsonSerializer();
            var logEntriesObject = serializer.Deserialize<List<AuditLog>>(logEntriesJson.ToString());
            foreach (var item in logEntriesObject)
            {
                Assert.AreEqual(serverID.ToString(), item.ServerID);
            }
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("GetLogDataService_FromDB_Execute")]
        public void GetLogDataService_Execute_ShouldFilterLogData_WithexpectedWorkflowName()
        {
            //------------Setup for test--------------------------
            var getLogDataService = new GetLogDataService();
            var expectedWorkflowId = Guid.NewGuid();
            var expectedExecutionId = Guid.NewGuid();
            var nextActivity = new Mock<IDev2Activity>();
            var expectedWorkflowName = "LogExecuteCompleteState_Workflow";
            var principal = new Mock<IPrincipal>();
            principal.Setup(o => o.Identity).Returns(() => new Mock<IIdentity>().Object);
            TestAuditSetupWithAssignedInputs(expectedWorkflowId, expectedWorkflowName, expectedExecutionId, out _dev2StateAuditLogger, out _activity, principal);
            _dev2StateAuditLogger.LogExecuteCompleteState(nextActivity.Object);
            //---------------Assert Precondition----------------
            var stringBuilders = new Dictionary<string, StringBuilder>();
            stringBuilders.Add("WorkflowName", expectedWorkflowName.ToString().ToStringBuilder());
            var logEntriesJson = getLogDataService.Execute(stringBuilders, null);
            //------------Assert Results-------------------------
            Assert.IsNotNull(logEntriesJson);
            var serializer = new Dev2JsonSerializer();
            var logEntriesObject = serializer.Deserialize<List<AuditLog>>(logEntriesJson.ToString());
            foreach (var item in logEntriesObject)
            {
                Assert.AreEqual(expectedWorkflowName.ToString(), item.WorkflowName);
            }
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("GetLogDataService_FromDB_Execute")]
        public void GetLogDataService_Execute_ShouldFilterLogData_WithIsSubExecution()
        {
            //------------Setup for test--------------------------
            var getLogDataService = new GetLogDataService();
            var expectedWorkflowId = Guid.NewGuid();
            var expectedExecutionId = Guid.NewGuid();
            var nextActivity = new Mock<IDev2Activity>();
            var IsSubExecution = false;
            var expectedWorkflowName = "LogExecuteCompleteState_Workflow";
            var principal = new Mock<IPrincipal>();
            principal.Setup(o => o.Identity).Returns(() => new Mock<IIdentity>().Object);
            TestAuditSetupWithAssignedInputs(expectedWorkflowId, expectedWorkflowName, expectedExecutionId, out _dev2StateAuditLogger, out _activity, principal, IsSubExecution);
            _dev2StateAuditLogger.LogExecuteCompleteState(nextActivity.Object);
            //---------------Assert Precondition----------------
            var stringBuilders = new Dictionary<string, StringBuilder>();
            stringBuilders.Add("IsSubExecution", IsSubExecution.ToString().ToStringBuilder());
            var logEntriesJson = getLogDataService.Execute(stringBuilders, null);
            //------------Assert Results-------------------------
            Assert.IsNotNull(logEntriesJson);
            var serializer = new Dev2JsonSerializer();
            var logEntriesObject = serializer.Deserialize<List<AuditLog>>(logEntriesJson.ToString());
            foreach (var item in logEntriesObject)
            {
                
                Assert.AreEqual("0", item.IsSubExecution.ToString());
            }
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory("GetLogDataService_FromDB_Execute")]
        public void GetLogDataService_Execute_ShouldFilterLogData_WithIsRemoteWorkflow()
        {
            //------------Setup for test--------------------------
            var getLogDataService = new GetLogDataService();
            var expectedWorkflowId = Guid.NewGuid();
            var expectedExecutionId = Guid.NewGuid();
            var nextActivity = new Mock<IDev2Activity>();
            var IsRemoteWorkflow = false;
            var expectedWorkflowName = "LogExecuteCompleteState_Workflow";
            var principal = new Mock<IPrincipal>();
            principal.Setup(o => o.Identity).Returns(() => new Mock<IIdentity>().Object);
            TestAuditSetupWithAssignedInputs(expectedWorkflowId, expectedWorkflowName, expectedExecutionId, out _dev2StateAuditLogger, out _activity, principal,false , IsRemoteWorkflow);
            _dev2StateAuditLogger.LogExecuteCompleteState(nextActivity.Object);
            //---------------Assert Precondition----------------
            var stringBuilders = new Dictionary<string, StringBuilder>();
            stringBuilders.Add("IsRemoteWorkflow", IsRemoteWorkflow.ToString().ToStringBuilder());
            var logEntriesJson = getLogDataService.Execute(stringBuilders, null);
            //------------Assert Results-------------------------
            Assert.IsNotNull(logEntriesJson);
            var serializer = new Dev2JsonSerializer();
            var logEntriesObject = serializer.Deserialize<List<AuditLog>>(logEntriesJson.ToString());
            foreach (var item in logEntriesObject)
            {

                Assert.AreEqual("0", item.IsRemoteWorkflow.ToString());
            }
        }
        private static Dev2StateAuditLogger GetDev2AuditStateLogger(Mock<IDSFDataObject> mockedDataObject)
        {
            return new Dev2StateAuditLogger(mockedDataObject.Object);
        }

        private static void TestAuditSetupWithAssignedInputs(Guid resourceId, string workflowName, Guid executionId, out Dev2StateAuditLogger dev2AuditStateLogger, out Mock<IDev2Activity> activity, Mock<IPrincipal> principle, bool isSubExecution = false, bool isRemoteWorkflow = false, Guid serverID = default(Guid), Guid parentID = default(Guid))
        {
            // setup
            Mock<IDSFDataObject> mockedDataObject = SetupDataObjectWithAssignedInputs(resourceId, workflowName, executionId, principle, isSubExecution, isRemoteWorkflow, serverID, parentID);
            activity = new Mock<IDev2Activity>();
            dev2AuditStateLogger = GetDev2AuditStateLogger(mockedDataObject);
        }

        private static Mock<IDSFDataObject> SetupDataObjectWithAssignedInputs(Guid resourceId, string workflowName, Guid executionId, Mock<IPrincipal> principle, bool isSubExecution, bool isRemoteWorkflow, Guid serverID, Guid parentID)
        {
            // mocks
            var mockedDataObject = new Mock<IDSFDataObject>();
            mockedDataObject.Setup(o => o.Environment).Returns(() => new ExecutionEnvironment());
            mockedDataObject.Setup(o => o.ServiceName).Returns(() => workflowName);
            mockedDataObject.Setup(o => o.ResourceID).Returns(() => resourceId);
            mockedDataObject.Setup(o => o.ExecutionID).Returns(() => executionId);
            mockedDataObject.Setup(o => o.IsSubExecution).Returns(() => isSubExecution);
            mockedDataObject.Setup(o => o.ParentID).Returns(() => parentID);
            mockedDataObject.Setup(o => o.IsRemoteWorkflow()).Returns(() => isRemoteWorkflow);
            mockedDataObject.Setup(o => o.ServerID).Returns(() => serverID);
            mockedDataObject.Setup(o => o.ExecutingUser).Returns(() => principle.Object);
            return mockedDataObject;
        }
    }
}