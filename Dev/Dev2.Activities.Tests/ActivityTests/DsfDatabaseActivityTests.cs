using System;
using Dev2.Activities;
using Dev2.DataList.Contract;
using Dev2.DynamicServices;
using Dev2.Runtime.Helpers;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Execution;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;

namespace Dev2.Tests.Activities.ActivityTests
{
    [TestClass]
    public class DsfDatabaseActivityTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Database Service Execution

        [TestMethod]
        [TestCategory("UnitTest")]
        [Description("Test 'CleanDataList' for 'DsfDatabaseActivity': DsfDatabaseActivity uses RuntimeHelpers to clean the datalist prior to execution")]
        [Owner("Ashley")]
        // ReSharper disable InconsistentNaming
        public void DsfDatabaseActivity_DsfDatabaseActivityUnitTest_CleanDataList_RuntimeHelperCallsGetCorrectDataList()
        // ReSharper restore InconsistentNaming
        {
            //init
            var databaseActivity = new MockDsfDatabaseActivity();
            var mockRuntimeHelper = new Mock<RuntimeHelpers>();
            mockRuntimeHelper.Setup(c => c.GetCorrectDataList(It.IsAny<IDSFDataObject>(), It.IsAny<Guid>(), It.IsAny<ErrorResultTO>(), It.IsAny<IDataListCompiler>()));

            //exe
            databaseActivity.MockCleanDataList(mockRuntimeHelper.Object, It.IsAny<IDSFDataObject>(), It.IsAny<Guid>(), It.IsAny<IDataListCompiler>());

            //assert
            mockRuntimeHelper.Verify(c => c.GetCorrectDataList(It.IsAny<IDSFDataObject>(), It.IsAny<Guid>(), It.IsAny<ErrorResultTO>(), It.IsAny<IDataListCompiler>()), Times.Once());
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [Description("Test 'GetNewDatabaseServiceExecution' for 'DsfDatabaseActivity': A valid database service execution is constructed by DsfDatabaseActivity")]
        [Owner("Ashley")]
        // ReSharper disable InconsistentNaming
        public void DsfDatabaseActivity_DsfDatabaseActivityUnitTest_GetNewDatabaseServiceExecution_ServiceConstructed()
        // ReSharper restore InconsistentNaming
        {
            //init
            var databaseActivity = new MockDsfDatabaseActivity();
            var mockContext = new Mock<IDSFDataObject>();

            //exe
            var actual = databaseActivity.MockGetNewDatabaseServiceExecution(mockContext.Object);

            //assert
            Assert.AreEqual(typeof(DatabaseServiceExecution), actual.GetType(), "DsfDatabaseActivity did not construct a correct DatabaseServiceExecution");
            Assert.AreEqual(mockContext.Object.DataListID, actual.DataObj.DataListID, "The Database Service Execution constructed by DsfDatabaseActivity is using the incorrect DbService resource");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [Description("Test for Execution of 'DsfDatabaseActivity': A valid database service is executed")]
        [Owner("Ashley")]
        // ReSharper disable InconsistentNaming
        public void DsfDatabaseActivity_DsfDatabaseActivityUnitTest_ExecutionImpl_ActivityExecuted()
        // ReSharper restore InconsistentNaming
        {
            //init
            var databaseActivity = new MockDsfDatabaseActivity();
            var errors = new ErrorResultTO();
            var mockContainer = new Mock<DatabaseServiceExecution>(new DsfDataObject(It.IsAny<string>(), It.IsAny<Guid>()));
            mockContainer.Setup(c => c.Execute(out errors)).Verifiable();

            //exe
            databaseActivity.MockExecuteDatabaseService(mockContainer.Object);

            //assert
            Assert.IsFalse(errors.HasErrors(), "Errors where thrown while executing a database service");
            mockContainer.Verify(c => c.Execute(out errors), Times.Once());
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [Description("Test for Execution of 'DsfDatabaseActivity': A valid database activity is executed with mocks injected")]
        [Owner("Ashley")]
        // ReSharper disable InconsistentNaming
        public void DsfDatabaseActivity_DsfDatabaseActivityUnitTest_ExecutionImpl_DatabaseActivityExecutes()
        // ReSharper restore InconsistentNaming
        {
            //init
            ErrorResultTO errors = null;
            var dbActivity = new Mock<MockDsfDatabaseActivity>();
            dbActivity.Protected().Setup("CleanDataList", ItExpr.IsAny<RuntimeHelpers>(), ItExpr.IsAny<IDSFDataObject>(), ItExpr.IsAny<Guid>(), ItExpr.IsAny<IDataListCompiler>()).Verifiable();
            dbActivity.Protected().Setup<DatabaseServiceExecution>("GetNewDatabaseServiceExecution", ItExpr.IsAny<IDSFDataObject>()).Verifiable();
            dbActivity.Protected().Setup<DatabaseServiceExecution>("GetNewDatabaseServiceExecution", ItExpr.IsAny<IDSFDataObject>()).Returns(It.IsAny<DatabaseServiceExecution>());
            dbActivity.Protected().Setup<Guid>("ExecuteDatabaseService", ItExpr.IsAny<DatabaseServiceExecution>()).Verifiable();
            dbActivity.Protected().Setup<Guid>("ExecuteDatabaseService", ItExpr.IsAny<DatabaseServiceExecution>()).Returns(It.IsAny<Guid>());
            IDSFDataObject context = new DsfDataObject(It.IsAny<string>(), It.IsAny<Guid>());
            context.WorkspaceID = It.IsAny<Guid>();
            context.ResourceID = It.IsAny<Guid>();

            //exe
            dbActivity.Object.MockExecutionImpl(It.IsAny<IEsbChannel>(), context, out errors);

            //assert
// ReSharper disable PossibleNullReferenceException
            Assert.IsFalse(errors.HasErrors(), "Errors where thrown while executing a plugin activity");
// ReSharper restore PossibleNullReferenceException
            dbActivity.Protected().Verify("CleanDataList", Times.Once(), ItExpr.IsAny<RuntimeHelpers>(), ItExpr.IsAny<IDSFDataObject>(), ItExpr.IsAny<Guid>(), ItExpr.IsAny<IDataListCompiler>());
            dbActivity.Protected().Verify<DatabaseServiceExecution>("GetNewDatabaseServiceExecution", Times.Once(), ItExpr.IsAny<IDSFDataObject>());
            dbActivity.Protected().Verify<Guid>("ExecuteDatabaseService", Times.Once(), ItExpr.IsAny<DatabaseServiceExecution>());
        }
        
        #endregion
    }
}
