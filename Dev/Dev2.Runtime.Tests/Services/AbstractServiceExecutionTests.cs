using System;
using System.Collections.Generic;
using Dev2.DataList.Contract;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Network;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Execution;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Unlimited.Framework.Converters.Graph.Interfaces;

namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class AbstractServiceExecutionTests
    {
        #region Create Service

        [TestMethod]
        [TestCategory("UnitTest")]
        [Description("Test ServiceExecutionAbstract's CreateService function: It is expected to get a service and a source resource")]
        [Owner("Ashley Lewis")]
        // ReSharper disable InconsistentNaming
        public void ServiceExecutionAbstract_ServiceExecutionAbstractUnitTest_CreateService_ResourceCatalogRetrievesBothResources()
        // ReSharper restore InconsistentNaming
        {
            //init
            var databaseService = new MockServiceExecutionAbstract<DbService, DbSource>(new DsfDataObject("<DataList></DataList>", Guid.NewGuid()), It.IsAny<bool>());
            var mockResourceCatalog = new Mock<ResourceCatalog>(It.IsAny<IEnumerable<DynamicService>>(), It.IsAny<IContextManager<IStudioNetworkSession>>());
            mockResourceCatalog.Setup(c => c.GetResource<DbService>(It.IsAny<Guid>(), It.IsAny<Guid>())).Verifiable();
            mockResourceCatalog.Setup(c => c.GetResource<DbService>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new DbService());
            mockResourceCatalog.Setup(c => c.GetResource<DbSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Verifiable();
            mockResourceCatalog.Setup(c => c.GetResource<DbSource>(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(new DbSource());

            //exe
            databaseService.MockCreateService(mockResourceCatalog.Object);

            //assert
            mockResourceCatalog.Verify(c => c.GetResource<DbService>(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once());
            mockResourceCatalog.Verify(c => c.GetResource<DbSource>(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Once());
        }
        
        #endregion

        #region Execute

        [TestMethod]
        [TestCategory("UnitTest")]
        [Description("Test ServiceExecutionAbstract's Execute function: It is expected to call execute service on a database service")]
        [Owner("Ashley Lewis")]
        // ReSharper disable InconsistentNaming
        public void ServiceExecutionAbstract_ServiceExecutionAbstractUnitTest_Execute_ExecuteServiceIsCalled()
            // ReSharper restore InconsistentNaming
        {
            //init
            var errors = new ErrorResultTO();
            var databaseService = new Mock<MockServiceExecutionAbstract<DbService, DbSource>>(new DsfDataObject("<DataList></DataList>", Guid.NewGuid()), It.IsAny<bool>());
            databaseService.Object.Service = new DbService { Method = new ServiceMethod() };
            databaseService.Protected().Setup("ExecuteService").Verifiable();
            databaseService.Protected().Setup<object>("ExecuteService").Returns("Test Execution Result");
            var compiler = new Mock<IDataListCompiler>();
            compiler.Setup(c => c.ShapeDev2DefinitionsToDataList(It.IsAny<IList<IDev2Definition>>(), It.IsAny<enDev2ArgumentType>(), It.IsAny<bool>(), out errors)).Returns(It.IsAny<string>);
            compiler.Setup(c => c.ConvertTo(It.IsAny<DataListFormat>(), It.IsAny<string>(), It.IsAny<string>(), out errors)).Returns(It.IsAny<Guid>());
            
            //exe
            databaseService.Object.MockExecuteImpl(compiler.Object, out errors);

            //assert
            Assert.IsFalse(errors.HasErrors(), "Errors where thrown executing a database service container");
            databaseService.Protected().Verify("ExecuteService", Times.Once());
        }

        #endregion
    }
}
