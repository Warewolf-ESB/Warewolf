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
using System.Security.Principal;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Wrappers;
using Dev2.Common.Wrappers;
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Storage;
using System.Linq;
using System.Data.Entity;
using Dev2.Common.Container;
using Dev2.Runtime.Auditing;

namespace Dev2.Tests.Runtime.Auditing
{
    [TestClass]
    public class Dev2StateLoggerTests
    {
        IFile _fileWrapper;
        IDirectory _directoryWrapper;
        IDev2StateAuditLogger _dev2StateAuditLogger;
        IDSFDataObject _dSFDataObject;
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
        [TestMethod]
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

        [TestMethod]
        public void Dev2StateAuditLogger_LogExecuteCompleteState_Tests()
        {
            var expectedWorkflowId = Guid.NewGuid();
            var nextActivity = new Mock<IDev2Activity>();
            var expectedWorkflowName = "LogExecuteCompleteState_Workflow";
            TestAuditSetupWithAssignedInputs(expectedWorkflowId, expectedWorkflowName, out _dev2StateAuditLogger, out _activity);
            // test
            _dev2StateAuditLogger.NewStateListener(_dSFDataObject).LogExecuteCompleteState(nextActivity.Object);

            _dev2StateAuditLogger.Flush();

            // verify
            var str = expectedWorkflowId.ToString();
            var results = Dev2StateAuditLogger.Query(a => (a.WorkflowID.Equals(str)
                || a.WorkflowName.Equals("LogExecuteCompleteState")
                || a.ExecutionID.Equals("")
                || a.AuditType.Equals("")));
            _dev2StateAuditLogger.Dispose();

            Assert.IsTrue(results.FirstOrDefault(a => a.WorkflowID == str) != null);
        }

        [TestMethod]
        public void Dev2StateAuditLogger_LogExecuteException_Tests()
        {
            var expectedWorkflowId = Guid.NewGuid();
            var expectedWorkflowName = "LogExecuteException_Workflow";
            TestAuditSetupWithAssignedInputs(expectedWorkflowId, expectedWorkflowName, out _dev2StateAuditLogger, out _activity);
            // test
            var activity = new Mock<IDev2Activity>();
            var exception = new Mock<Exception>();
            _dev2StateAuditLogger.NewStateListener(_dSFDataObject).LogExecuteException(exception.Object, activity.Object);

            _dev2StateAuditLogger.Flush();

            // verify
            var str = expectedWorkflowId.ToString();
            var results = Dev2StateAuditLogger.Query(a => (a.WorkflowID.Equals(str)
                || a.WorkflowName.Equals("LogExecuteException")
                || a.ExecutionID.Equals("")
                || a.AuditType.Equals("")));
            _dev2StateAuditLogger.Dispose();

            Assert.IsTrue(results.FirstOrDefault(a => a.WorkflowID == str) != null);
        }

        [TestMethod]
        public void Dev2StateAuditLogger_LogPostExecuteState_Tests()
        {
            var expectedWorkflowId = Guid.NewGuid();
            var expectedWorkflowName = "LogPostExecuteState_Workflow";
            TestAuditSetupWithAssignedInputs(expectedWorkflowId, expectedWorkflowName, out _dev2StateAuditLogger, out _activity);
            // test
            var previousActivity = new Mock<IDev2Activity>();
            var nextActivity = new Mock<IDev2Activity>();
            _dev2StateAuditLogger.NewStateListener(_dSFDataObject).LogPostExecuteState(previousActivity.Object, nextActivity.Object);

            _dev2StateAuditLogger.Flush();

            // verify
            var str = expectedWorkflowId.ToString();
            var results = Dev2StateAuditLogger.Query(a => (a.WorkflowID.Equals(str)
                || a.WorkflowName.Equals("LogPostExecuteState")
                || a.ExecutionID.Equals("")
                || a.AuditType.Equals("")));
            _dev2StateAuditLogger.Dispose();

            Assert.IsTrue(results.FirstOrDefault(a => a.WorkflowID == str) != null);
        }

        [TestMethod]
        public void Dev2StateAuditLogger_LogAdditionalDetail_Tests()
        {
            var expectedWorkflowId = Guid.NewGuid();
            var expectedWorkflowName = "LogAdditionalDetail_Workflow";
            TestAuditSetupWithAssignedInputs(expectedWorkflowId, expectedWorkflowName, out _dev2StateAuditLogger, out _activity);
            // test
            var additionalDetailObject = new { Message = "Some Message" };
            _dev2StateAuditLogger.NewStateListener(_dSFDataObject).LogAdditionalDetail(additionalDetailObject, "");

            _dev2StateAuditLogger.Flush();

            // verify
            var str = expectedWorkflowId.ToString();
            var results = Dev2StateAuditLogger.Query(a => (a.WorkflowID.Equals(str)
                || a.WorkflowName.Equals("LogAdditionalDetail")
                || a.ExecutionID.Equals("")
                || a.AuditType.Equals("")));
            _dev2StateAuditLogger.Dispose();

            Assert.IsTrue(results.FirstOrDefault(a => a.WorkflowID == str) != null);
        }

        [TestMethod]
        public void Dev2StateAuditLogger_LogPreExecuteState_Tests()
        {
            var expectedWorkflowId = Guid.NewGuid();
            var expectedWorkflowName = "LogPreExecuteState_Workflow";
            TestAuditSetupWithAssignedInputs(expectedWorkflowId, expectedWorkflowName, out _dev2StateAuditLogger, out _activity);
            // test
            _dev2StateAuditLogger.NewStateListener(_dSFDataObject).LogPreExecuteState(_activity.Object);
            // verify

            _dev2StateAuditLogger.Flush();

            var str = expectedWorkflowId.ToString();
            var results = Dev2StateAuditLogger.Query(a => a.WorkflowID == str);
            _dev2StateAuditLogger.Dispose();

            foreach (var item in results)
            {   

            }

            var result = results.FirstOrDefault(a => a.WorkflowID == str);
            Assert.IsTrue(result != null);
            Assert.AreEqual("{\"Environment\":{\"scalars\":{},\"record_sets\":{},\"json_objects\":{}},\"Errors\":[],\"AllErrors\":[]}",
                            result.Environment);
        }

        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void Dev2StateAuditLogger_LogPreExecuteState_ExpectedException_Tests()
        {
            var expectedWorkflowId = Guid.NewGuid();
            var expectedWorkflowName = "LogPreExecuteState_Workflow";
            TestAuditSetupWithAssignedInputsNullDbFactory(expectedWorkflowId, expectedWorkflowName, out _dev2StateAuditLogger, out _activity);
            // test
            _dev2StateAuditLogger.NewStateListener(_dSFDataObject).LogPreExecuteState(_activity.Object);
            // verify

            _dev2StateAuditLogger.Flush();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Dev2StateAuditLogger_LogAuditState_Exception_Tests()
        {
            var expectedWorkflowId = Guid.NewGuid();
            var expectedWorkflowName = "LogPreExecuteState_Workflow";
            TestAuditSetupWithAssignedInputs(expectedWorkflowId, expectedWorkflowName, out _dev2StateAuditLogger, out _activity);
            // test
            (_dev2StateAuditLogger as Dev2StateAuditLogger).LogAuditState(1);
        }

        private void TestSetup(out IFile fileWrapper, out IDirectory directoryWrapper, out Mock<IDev2Activity> activity)
        {
            // setup
            var mockedDataObject = SetupDataObject();
            fileWrapper = new FileWrapper();
            directoryWrapper = new DirectoryWrapper();
            activity = new Mock<IDev2Activity>();
        }
        
        private Dev2StateAuditLogger GetDev2AuditStateLogger(Mock<IDSFDataObject> mockedDataObject)
        {
            return new Dev2StateAuditLogger(new DatabaseContextFactory(), new WarewolfQueue());
        }

        private Dev2StateAuditLogger GetDev2AuditStateLoggerNullDbFactory(Mock<IDSFDataObject> mockedDataObject)
        {
            return new Dev2StateAuditLogger(null, new WarewolfQueue());
        }

        private void TestAuditSetupWithAssignedInputs(Guid resourceId, string workflowName, out IDev2StateAuditLogger dev2AuditStateLogger, out Mock<IDev2Activity> activity)
        {
            GetMockedDataObject(resourceId, workflowName, out activity, out Mock<IDSFDataObject> mockedDataObject);
            dev2AuditStateLogger = GetDev2AuditStateLogger(mockedDataObject);
        }

        private void GetMockedDataObject(Guid resourceId, string workflowName, out Mock<IDev2Activity> activity, out Mock<IDSFDataObject> mockedDataObject)
        {
            var executionId = Guid.NewGuid();
            // setup
            mockedDataObject = SetupDataObjectWithAssignedInputs(resourceId, workflowName, executionId);
            activity = new Mock<IDev2Activity>();
        }

        private void TestAuditSetupWithAssignedInputsNullDbFactory(Guid resourceId, string workflowName, out IDev2StateAuditLogger dev2AuditStateLogger, out Mock<IDev2Activity> activity)
        {
            GetMockedDataObject(resourceId, workflowName, out activity, out Mock<IDSFDataObject> mockedDataObject);
            dev2AuditStateLogger = GetDev2AuditStateLoggerNullDbFactory(mockedDataObject);
        }

        private void TestMockDatabaseContextFactoryWithMockDbContext<ExceptionT>(Guid resourceId, string workflowName, out IDev2StateAuditLogger dev2AuditStateLogger, out Mock<IDev2Activity> activity) where ExceptionT : Exception, new()
        {
            GetMockedDataObject(resourceId, workflowName, out activity, out Mock<IDSFDataObject> mockedDataObject);
            dev2AuditStateLogger = new Dev2StateAuditLogger(new MockDatabaseContextFactoryWithMockDbContext<ExceptionT>(), new WarewolfQueue());
        }

        class MockDatabaseContextFactory : IDatabaseContextFactory
        {
            public IAuditDatabaseContext Get()
            {
                return null;
            }
        }

        class MockDatabaseContextFactoryWithMockDbContext<ExceptionT> : IDatabaseContextFactory where ExceptionT : Exception, new()
        {
            class MockAuditDatabaseContext : IAuditDatabaseContext
            {
                public DbSet<AuditLog> Audits { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
                public bool IsDisposed { get; private set; }

                public Database Database => throw new NotImplementedException();

                public void Dispose()
                {
                    IsDisposed = true;
                }

                public int SaveChanges()
                {
                    throw new ExceptionT();
                }
            }

            public IAuditDatabaseContext Get()
            {
                return new MockAuditDatabaseContext();
            }
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

        private Mock<IDSFDataObject> SetupDataObject()
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

            _dSFDataObject = mockedDataObject.Object;

            return mockedDataObject;
        }
    }
}