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
    public class DsfPluginActivityTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Plugin Service Execution

        [TestMethod]
        [TestCategory("UnitTest")]
        [Description("Test 'CleanDataList' for 'DsfPluginActivity': DsfPluginActivity uses RuntimeHelpers to clean the datalist prior to execution")]
        [Owner("Ashley")]
        // ReSharper disable InconsistentNaming
        public void DsfPluginActivity_DsfPluginActivityUnitTest_CleanDataList_RuntimeHelperCallsGetCorrectDataList()
        // ReSharper restore InconsistentNaming
        {
            //init
            var pluginActivity = new MockDsfPluginActivity();
            var mockRuntimeHelper = new Mock<RuntimeHelpers>();
            mockRuntimeHelper.Setup(c => c.GetCorrectDataList(It.IsAny<IDSFDataObject>(), It.IsAny<Guid>(), It.IsAny<ErrorResultTO>(), It.IsAny<IDataListCompiler>()));

            //exe
            pluginActivity.MockCleanDataList(mockRuntimeHelper.Object, It.IsAny<IDSFDataObject>(), It.IsAny<Guid>(), It.IsAny<ErrorResultTO>(), It.IsAny<IDataListCompiler>());

            //assert
            mockRuntimeHelper.Verify(c => c.GetCorrectDataList(It.IsAny<IDSFDataObject>(), It.IsAny<Guid>(), It.IsAny<ErrorResultTO>(), It.IsAny<IDataListCompiler>()), Times.Once());
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [Description("Test 'GetNewPluginServiceExecution' for 'DsfPluginActivity': A valid plugin service execution is constructed by DsfPluginActivity")]
        [Owner("Ashley")]
        // ReSharper disable InconsistentNaming
        public void DsfPluginActivity_DsfPluginActivityUnitTest_GetNewPluginServiceExecution_ServiceConstructed()
        // ReSharper restore InconsistentNaming
        {
            //init
            var expected = Guid.NewGuid();
            var pluginActivity = new MockDsfPluginActivity();
            var mockContext = new DsfDataObject("<DataList></DataList>", expected);

            //exe
            var actual = pluginActivity.MockGetNewPluginServiceExecution(mockContext);

            //assert
            Assert.AreEqual(typeof(PluginServiceExecution), actual.GetType(), "DsfPluginActivity did not construct a correct PluginServiceExecution");
            Assert.AreEqual(expected, actual.DataObj.DataListID, "The Plugin Service Execution constructed by DsfPluginActivity is using the incorrect datalist");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [Description("Test for Execution of 'PluginServiceExecution': A valid plugin service is executed")]
        [Owner("Ashley")]
        // ReSharper disable InconsistentNaming
        public void DsfPluginActivity_DsfPluginActivityUnitTest_ExecutePluginService_ServiceExecuted()
        // ReSharper restore InconsistentNaming
        {
            //init
            var pluginActivity = new MockDsfPluginActivity();
            var errors = new ErrorResultTO();
            var mockContainer = new Mock<PluginServiceExecution>(new DsfDataObject(It.IsAny<string>(), It.IsAny<Guid>()), It.IsAny<bool>());
            mockContainer.Setup(c => c.Execute(out errors)).Verifiable();

            //exe
            pluginActivity.MockExecutePluginService(mockContainer.Object);

            //assert
            Assert.IsFalse(errors.HasErrors(), "Errors where thrown while executing a plugin service");
            mockContainer.Verify(c => c.Execute(out errors), Times.Once());
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [Description("Test for Execution of 'DsfPluginActivity': A valid plugin activity is executed with mocks injected")]
        [Owner("Ashley")]
        // ReSharper disable InconsistentNaming
        public void DsfPluginActivity_DsfPluginActivityUnitTest_ExecutionImpl_PluginActivityExecutes()
        // ReSharper restore InconsistentNaming
        {
            //init
            ErrorResultTO errors;
            var pluginActivity = new Mock<MockDsfPluginActivity>();
            pluginActivity.Protected().Setup("CleanDataList", ItExpr.IsAny<RuntimeHelpers>(), ItExpr.IsAny<IDSFDataObject>(), ItExpr.IsAny<Guid>(), ItExpr.IsAny<IDataListCompiler>()).Verifiable();
            pluginActivity.Protected().Setup<PluginServiceExecution>("GetNewPluginServiceExecution", ItExpr.IsAny<IDSFDataObject>()).Verifiable();
            pluginActivity.Protected().Setup<PluginServiceExecution>("GetNewPluginServiceExecution", ItExpr.IsAny<IDSFDataObject>()).Returns(It.IsAny<PluginServiceExecution>());
            pluginActivity.Protected().Setup<Guid>("ExecutePluginService", ItExpr.IsAny<PluginServiceExecution>()).Verifiable();
            pluginActivity.Protected().Setup<Guid>("ExecutePluginService", ItExpr.IsAny<PluginServiceExecution>()).Returns(It.IsAny<Guid>());
            IDSFDataObject context = new DsfDataObject(It.IsAny<string>(), It.IsAny<Guid>());
            context.WorkspaceID = It.IsAny<Guid>();
            context.ResourceID = It.IsAny<Guid>();

            //exe
            pluginActivity.Object.MockExecutionImpl(It.IsAny<IEsbChannel>(), context, out errors);

            //assert
            Assert.IsFalse(errors.HasErrors(), "Errors where thrown while executing a plugin activity");
            pluginActivity.Protected().Verify("CleanDataList", Times.Once(), ItExpr.IsAny<RuntimeHelpers>(), ItExpr.IsAny<IDSFDataObject>(), ItExpr.IsAny<Guid>(), ItExpr.IsAny<IDataListCompiler>());
            pluginActivity.Protected().Verify<PluginServiceExecution>("GetNewPluginServiceExecution", Times.Once(), ItExpr.IsAny<IDSFDataObject>());
            pluginActivity.Protected().Verify<Guid>("ExecutePluginService", Times.Once(), ItExpr.IsAny<PluginServiceExecution>());
        }
        
        #endregion
    }
}
