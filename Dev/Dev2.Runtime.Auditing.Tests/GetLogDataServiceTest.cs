/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Container;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.Interfaces;
using Dev2.Runtime.Auditing;
using Dev2.Runtime.ESB.Management.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Warewolf.Storage;

namespace Dev2.Tests.Runtime.Auditing
{
    [TestClass]
    public class GetLogDataServiceTest
    {
        IDev2StateAuditLogger _dev2StateAuditLogger;
        Mock<IDev2Activity> _activity;
        IDSFDataObject _dSFDataObject;
        
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
            var expectedWorkflowId = Guid.NewGuid();
            var serializer = new Dev2JsonSerializer();
            var getLogDataService = new GetLogDataService();
            var nextActivity = new Mock<IDev2Activity>();
            var expectedWorkflowName = "LogExecuteCompleteState_Workflow";

            TestAuditSetupWithAssignedInputs(expectedWorkflowId, expectedWorkflowName, out _dev2StateAuditLogger, out _activity);
            // test
            _dev2StateAuditLogger.NewStateListener(_dSFDataObject).LogExecuteCompleteState(nextActivity.Object);
            _dev2StateAuditLogger.Flush();

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
            TestAuditSetupWithAssignedInputs(expectedWorkflowId, expectedWorkflowName, out _dev2StateAuditLogger, out _activity);
            _dev2StateAuditLogger.NewStateListener(_dSFDataObject).LogExecuteCompleteState(nextActivity.Object);
            _dev2StateAuditLogger.Flush();
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
        [Owner("Hagashen Naidu")]
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

            TestAuditSetupWithAssignedInputs(expectedWorkflowId, expectedWorkflowName, out _dev2StateAuditLogger, out _activity);
            _dev2StateAuditLogger.NewStateListener(_dSFDataObject).LogExecuteCompleteState(nextActivity.Object);
            _dev2StateAuditLogger.Flush();
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
        [Owner("Sanele Mthembu")]
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
            TestAuditSetupWithAssignedInputs(expectedWorkflowId, expectedWorkflowName, out _dev2StateAuditLogger, out _activity);
            _dev2StateAuditLogger.NewStateListener(_dSFDataObject).LogExecuteCompleteState(nextActivity.Object);
            _dev2StateAuditLogger.Flush();
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
        [Owner("Candice Daniel")]
        [TestCategory("GetLogDataService_Execute")]
        [DeploymentItem(@"TextFiles\LogFileWithFlatResultsNEwFormatWithErrors.txt", "TextFiles")]
        public void GetLogDataService_Execute_WithLogDataContainingURL_shouldReturnLogDataObjectWithUrlERROR()
        {
            //------------Setup for test--------------------------
            var getLogDataService = new GetLogDataService();
            //------------Assert Results-------------------------
            Assert.AreEqual("GetLogDataService", getLogDataService.HandlesType());
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        [TestCategory("GetLogDataService_Execute")]
        [DeploymentItem(@"TextFiles\LogFileWithFlatResultsNEwFormat.txt", "TextFiles")]
        public void GetLogDataService_Execute_WithStartDate_ShouldFilterLogData()
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
        [DeploymentItem(@"TextFiles\LogFileWithFlatResultsNEwFormat.txt", "TextFiles")]
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
            TestAuditSetupWithAssignedInputs(expectedWorkflowId, expectedWorkflowName, out _dev2StateAuditLogger, out _activity);
            _dev2StateAuditLogger.NewStateListener(_dSFDataObject).LogExecuteCompleteState(nextActivity.Object);
            _dev2StateAuditLogger.Flush();

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
        [DeploymentItem(@"TextFiles\LogFileWithFlatResultsNEwFormat.txt", "TextFiles")]
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
            TestAuditSetupWithAssignedInputs(expectedWorkflowId, expectedWorkflowName, out _dev2StateAuditLogger, out _activity);
            _dev2StateAuditLogger.NewStateListener(_dSFDataObject).LogExecuteCompleteState(nextActivity.Object);
            _dev2StateAuditLogger.Flush();
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
        [DeploymentItem(@"TextFiles\LogFileWithFlatResultsNEwFormat.txt", "TextFiles")]
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
            TestAuditSetupWithAssignedInputs(expectedWorkflowId, expectedWorkflowName, out _dev2StateAuditLogger, out _activity);
            _dev2StateAuditLogger.NewStateListener(_dSFDataObject).LogExecuteCompleteState(nextActivity.Object);
            _dev2StateAuditLogger.Flush();
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
        [DeploymentItem(@"TextFiles\LogFileWithFlatResultsNEwFormat.txt", "TextFiles")]
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
            TestAuditSetupWithAssignedInputs(expectedWorkflowId, expectedWorkflowName, out _dev2StateAuditLogger, out _activity);
            _dev2StateAuditLogger.NewStateListener(_dSFDataObject).LogExecuteCompleteState(nextActivity.Object);
            _dev2StateAuditLogger.Flush();
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
        [DeploymentItem(@"TextFiles\LogFileWithFlatResultsNEwFormat.txt", "TextFiles")]
        public void GetLogDataService_Execute_ShouldFilterLogData_WithexpectedWorkflowName()
        {
            //------------Setup for test--------------------------
            var getLogDataService = new GetLogDataService();
            var expectedWorkflowId = Guid.NewGuid();
            var nextActivity = new Mock<IDev2Activity>();
            var expectedWorkflowName = "LogExecuteCompleteState_Workflow";
            var principal = new Mock<IPrincipal>();
            principal.Setup(o => o.Identity).Returns(() => new Mock<IIdentity>().Object);
            TestAuditSetupWithAssignedInputs(expectedWorkflowId, expectedWorkflowName, out _dev2StateAuditLogger, out _activity);
            _dev2StateAuditLogger.NewStateListener(_dSFDataObject).LogExecuteCompleteState(nextActivity.Object);
            _dev2StateAuditLogger.Flush();
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
            var IsSubExecution = false;
            var nextActivity = new Mock<IDev2Activity>();
            var expectedWorkflowName = "LogExecuteCompleteState_Workflow";
            var principal = new Mock<IPrincipal>();
            principal.Setup(o => o.Identity).Returns(() => new Mock<IIdentity>().Object);
            TestAuditSetupWithAssignedInputs(expectedWorkflowId, expectedWorkflowName, out _dev2StateAuditLogger, out _activity);
            _dev2StateAuditLogger.NewStateListener(_dSFDataObject).LogExecuteCompleteState(nextActivity.Object);
            _dev2StateAuditLogger.Flush();

            //---------------Assert Precondition----------------
            var stringBuilders = new Dictionary<string, StringBuilder>();
            stringBuilders.Add("WorkflowName", expectedWorkflowName.ToString().ToStringBuilder());
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
            var getLogDataService = new GetLogDataService();
            var IsRemoteWorkflow = false;
            var expectedWorkflowId = Guid.NewGuid();
            var nextActivity = new Mock<IDev2Activity>();
            var expectedWorkflowName = "LogExecuteCompleteState_Workflow";
            var principal = new Mock<IPrincipal>();
            principal.Setup(o => o.Identity).Returns(() => new Mock<IIdentity>().Object);
            TestAuditSetupWithAssignedInputs(expectedWorkflowId, expectedWorkflowName, out _dev2StateAuditLogger, out _activity);
            _dev2StateAuditLogger.NewStateListener(_dSFDataObject).LogExecuteCompleteState(nextActivity.Object);
            _dev2StateAuditLogger.Flush();
           
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

        private Dev2StateAuditLogger GetDev2AuditStateLogger(Mock<IDSFDataObject> mockedDataObject)
        {
            return new Dev2StateAuditLogger(new DatabaseContextFactory(), new WarewolfQueue());
        }

        private void GetMockedDataObject(Guid resourceId, string workflowName, out Mock<IDev2Activity> activity, out Mock<IDSFDataObject> mockedDataObject)
        {
            var executionId = Guid.NewGuid();
            // setup
            mockedDataObject = SetupDataObjectWithAssignedInputs(resourceId, workflowName, executionId);
            activity = new Mock<IDev2Activity>();
        }
        private void TestAuditSetupWithAssignedInputs(Guid resourceId, string workflowName, out IDev2StateAuditLogger dev2AuditStateLogger, out Mock<IDev2Activity> activity)
        {
            GetMockedDataObject(resourceId, workflowName, out activity, out Mock<IDSFDataObject> mockedDataObject);
            dev2AuditStateLogger = GetDev2AuditStateLogger(mockedDataObject);
        }
        private Mock<IDSFDataObject> SetupDataObjectWithAssignedInputs(Guid resourceId, string workflowName, Guid executionId)
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

            _dSFDataObject = mockedDataObject.Object;

            return mockedDataObject;
        }
    }
}
