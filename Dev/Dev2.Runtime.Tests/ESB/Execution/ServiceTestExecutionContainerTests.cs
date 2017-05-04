using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using Dev2.Common;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.Data;
using Dev2.Data.TO;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Interfaces;
using Dev2.Runtime.ESB.Execution;
using Dev2.Runtime.Interfaces;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Storage;

namespace Dev2.Tests.Runtime.ESB.Execution
{
    [TestClass]
    public class ServiceTestExecutionContainerTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Constructor_GivenArgs_ShouldPassThrough()
        {
            //---------------Set up test pack-------------------
            const string Datalist = "<DataList><scalar1 ColumnIODirection=\"Input\"/><persistantscalar ColumnIODirection=\"Input\"/><rs><f1 ColumnIODirection=\"Input\"/><f2 ColumnIODirection=\"Input\"/></rs><recset><field1/><field2/></recset></DataList>";
            var serviceAction = new ServiceAction() { DataListSpecification = new StringBuilder(Datalist) };
            var dsfObj = new Mock<IDSFDataObject>();
            dsfObj.Setup(o => o.Environment).Returns(new ExecutionEnvironment());
            dsfObj.Setup(o => o.Environment.Eval(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>(), false))
                  .Returns(CommonFunctions.WarewolfEvalResult.NewWarewolfAtomResult(DataStorage.WarewolfAtom.NewDataString("Args")));
            var workSpace = new Mock<IWorkspace>();
            var channel = new Mock<IEsbChannel>();
            var esbExecuteRequest = new EsbExecuteRequest();
            var serviceTestExecutionContainer = new ServiceTestExecutionContainerMock(serviceAction, dsfObj.Object, workSpace.Object, channel.Object, esbExecuteRequest);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(serviceTestExecutionContainer);
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsNull(serviceTestExecutionContainer.InstanceOutputDefinition);
            Assert.IsNull(serviceTestExecutionContainer.InstanceInputDefinition);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Execute_GivenArgs_ShouldPassThrough_ReturnsExecutedResults()
        {
            //---------------Set up test pack-------------------
            var resourceId = Guid.NewGuid();
            const string Datalist = "<DataList><scalar1 ColumnIODirection=\"Input\"/><persistantscalar ColumnIODirection=\"Input\"/><rs><f1 ColumnIODirection=\"Input\"/><f2 ColumnIODirection=\"Input\"/></rs><recset><field1/><field2/></recset></DataList>";
            var serviceAction = new ServiceAction()
            {
                DataListSpecification = new StringBuilder(Datalist),
                Service = new DynamicService() { ID = resourceId }
            };
            var dsfObj = new Mock<IDSFDataObject>();
            dsfObj.SetupProperty(o => o.ResourceID);
            const string TestName = "test1";
            dsfObj.Setup(o => o.TestName).Returns(TestName);
            dsfObj.Setup(o => o.Environment).Returns(new ExecutionEnvironment());
            dsfObj.Setup(o => o.Environment.Eval(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>(), false))
                  .Returns(CommonFunctions.WarewolfEvalResult.NewWarewolfAtomResult(DataStorage.WarewolfAtom.NewDataString("Args")));
            dsfObj.Setup(o => o.Environment.AllErrors).Returns(new HashSet<string>());

            var fetch = JsonResource.Fetch("Sequence");
            Dev2JsonSerializer s = new Dev2JsonSerializer();
            var testModelTO = s.Deserialize<ServiceTestModelTO>(fetch);

            var cataLog = new Mock<ITestCatalog>();
            cataLog.Setup(cat => cat.SaveTest(It.IsAny<Guid>(), It.IsAny<IServiceTestModelTO>())).Verifiable();
            cataLog.Setup(cat => cat.FetchTest(resourceId, TestName)).Returns(testModelTO);
            var resourceCat = new Mock<IResourceCatalog>();
            var activity = new Mock<IDev2Activity>();
            resourceCat.Setup(catalog => catalog.Parse(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(activity.Object);
            var workSpace = new Mock<IWorkspace>();
            var channel = new Mock<IEsbChannel>();
            var esbExecuteRequest = new EsbExecuteRequest();
            var serviceTestExecutionContainer = new ServiceTestExecutionContainerMock(serviceAction, dsfObj.Object, workSpace.Object, channel.Object, esbExecuteRequest, cataLog.Object, resourceCat.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(serviceTestExecutionContainer, "ServiceTestExecutionContainer is Null.");
            Assert.IsNull(serviceTestExecutionContainer.InstanceOutputDefinition);
            Assert.IsNull(serviceTestExecutionContainer.InstanceInputDefinition);
            //---------------Execute Test ----------------------
            ErrorResultTO errors;
            Thread.CurrentPrincipal = GlobalConstants.GenericPrincipal;
            Common.Utilities.PerformActionInsideImpersonatedContext(GlobalConstants.GenericPrincipal, () =>
            {
                var execute = serviceTestExecutionContainer.Execute(out errors, 1);
                Assert.IsNotNull(execute, "serviceTestExecutionContainer execute results is Null.");
            });

            //---------------Test Result -----------------------
            try
            {
                var serviceTestModelTO = esbExecuteRequest.ExecuteResult.DeserializeToObject<ServiceTestModelTO>(new KnownTypesBinder()
                {
                    KnownTypes = typeof(ServiceTestModelTO).Assembly.GetExportedTypes()
                        .Union(typeof(TestRunResult).Assembly.GetExportedTypes()).ToList()
                });
                Assert.IsNotNull(serviceTestModelTO, "Execute results Deserialize returned Null.");
            }
            catch (Exception)
            {
                var serviceTestModelTO = esbExecuteRequest.ExecuteResult.DeserializeToObject<TestRunResult>(new KnownTypesBinder()
                {
                    KnownTypes = typeof(ServiceTestModelTO).Assembly.GetExportedTypes()
                        .Union(typeof(TestRunResult).Assembly.GetExportedTypes()).ToList()
                });
                Assert.IsNotNull(serviceTestModelTO, "Execute results Deserialize returned Null.");
            }

            //
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Execute_GivenStopExecutionAndUnAuthorized_ShouldAddFailureMessage()
        {
            //---------------Set up test pack-------------------
            var resourceId = Guid.NewGuid();
            const string Datalist = "<DataList><scalar1 ColumnIODirection=\"Input\"/><persistantscalar ColumnIODirection=\"Input\"/><rs><f1 ColumnIODirection=\"Input\"/><f2 ColumnIODirection=\"Input\"/></rs><recset><field1/><field2/></recset></DataList>";
            var serviceAction = new ServiceAction()
            {
                DataListSpecification = new StringBuilder(Datalist),
                Service = new DynamicService() { ID = resourceId }
            };
            var dsfObj = new Mock<IDSFDataObject>();
            dsfObj.SetupProperty(o => o.ResourceID);
            const string TestName = "test1";
            dsfObj.Setup(o => o.TestName).Returns(TestName);
            dsfObj.Setup(o => o.StopExecution).Returns(true);
            dsfObj.Setup(o => o.Environment).Returns(new ExecutionEnvironment());
            dsfObj.Setup(o => o.Environment.Eval(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>(), false))
                  .Returns(CommonFunctions.WarewolfEvalResult.NewWarewolfAtomResult(DataStorage.WarewolfAtom.NewDataString("Args")));
            dsfObj.Setup(o => o.Environment.AllErrors).Returns(new HashSet<string>());
            dsfObj.Setup(o => o.IsDebugMode()).Returns(true);
            dsfObj.Setup(o => o.Environment.HasErrors()).Returns(true);
            dsfObj.Setup(o => o.Environment.FetchErrors()).Returns("Failed: The user running the test is not authorized to execute resource .");
            var fetch = JsonResource.Fetch("UnAuthorizedHelloWorld");
            Dev2JsonSerializer s = new Dev2JsonSerializer();
            var testModelTO = s.Deserialize<ServiceTestModelTO>(fetch);

            var cataLog = new Mock<ITestCatalog>();
            cataLog.Setup(cat => cat.SaveTest(It.IsAny<Guid>(), It.IsAny<IServiceTestModelTO>())).Verifiable();
            cataLog.Setup(cat => cat.FetchTest(resourceId, TestName)).Returns(testModelTO);
            var resourceCat = new Mock<IResourceCatalog>();
            var activity = new Mock<IDev2Activity>();
            resourceCat.Setup(catalog => catalog.Parse(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(activity.Object);
            var workSpace = new Mock<IWorkspace>();
            var channel = new Mock<IEsbChannel>();
            var esbExecuteRequest = new EsbExecuteRequest();
            var mockImpesinator = new Mock<IImpersonator>();
            mockImpesinator.Setup(impersonator => impersonator.Impersonate(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(false);
            var serviceTestExecutionContainer = new ServiceTestExecutionContainerMock(mockImpesinator.Object,  serviceAction, dsfObj.Object, workSpace.Object, channel.Object, esbExecuteRequest, cataLog.Object, resourceCat.Object);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(serviceTestExecutionContainer, "ServiceTestExecutionContainer is Null.");
            Assert.IsNull(serviceTestExecutionContainer.InstanceOutputDefinition);
            Assert.IsNull(serviceTestExecutionContainer.InstanceInputDefinition);
            //---------------Execute Test ----------------------
            ErrorResultTO errors;
            Thread.CurrentPrincipal = null;
            GenericIdentity identity = new GenericIdentity("User");
            var currentPrincipal = new GenericPrincipal(identity, new[] { "Role1", "Roll2" });
            Thread.CurrentPrincipal = currentPrincipal;
            Common.Utilities.ServerUser = currentPrincipal;

            var execute = serviceTestExecutionContainer.Execute(out errors, 1);
            Assert.IsNotNull(execute, "serviceTestExecutionContainer execute results is Null.");

            //---------------Test Result -----------------------

           
            try
            {
                var serviceTestModelTO = esbExecuteRequest.ExecuteResult.DeserializeToObject<ServiceTestModelTO>(new KnownTypesBinder()
                {
                    KnownTypes = typeof(ServiceTestModelTO).Assembly.GetExportedTypes()
                        .Union(typeof(TestRunResult).Assembly.GetExportedTypes()).ToList()
                });
                Assert.IsNotNull(serviceTestModelTO, "Execute results Deserialize returned Null.");
                Assert.IsNotNull(serviceTestModelTO, "Execute results Deserialize returned Null.");
                dsfObj.Verify(o => o.IsDebugMode());
                dsfObj.Verify(o => o.Environment.HasErrors());
                dsfObj.Verify(o => o.Environment.FetchErrors());
                cataLog.Verify(cat => cat.SaveTest(It.IsAny<Guid>(), It.IsAny<IServiceTestModelTO>()), Times.Never);
                cataLog.Verify(cat => cat.FetchTest(resourceId, TestName));
                Assert.AreNotEqual("", serviceTestModelTO.FailureMessage);
            }
            catch (Exception)
            {
                var testRunResult = esbExecuteRequest.ExecuteResult.DeserializeToObject<TestRunResult>(new KnownTypesBinder()
                {
                    KnownTypes = typeof(ServiceTestModelTO).Assembly.GetExportedTypes()
                        .Union(typeof(TestRunResult).Assembly.GetExportedTypes()).ToList()
                });
                Assert.IsNotNull(testRunResult, "Execute results Deserialize returned Null.");
                Assert.IsNotNull(testRunResult, "Execute results Deserialize returned Null.");
                dsfObj.Verify(o => o.IsDebugMode());
                dsfObj.Verify(o => o.Environment.HasErrors());
                dsfObj.Verify(o => o.Environment.FetchErrors());
                cataLog.Verify(cat => cat.SaveTest(It.IsAny<Guid>(), It.IsAny<IServiceTestModelTO>()), Times.Never);
                cataLog.Verify(cat => cat.FetchTest(resourceId, TestName));
                Assert.AreNotEqual("", testRunResult.Message);
            }
          
           



            //
        }



        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CanExecute_GivenArgs_ShouldPassThrough()
        {
            //---------------Set up test pack-------------------
            const string Datalist = "<DataList><scalar1 ColumnIODirection=\"Input\"/><persistantscalar ColumnIODirection=\"Input\"/><rs><f1 ColumnIODirection=\"Input\"/><f2 ColumnIODirection=\"Input\"/></rs><recset><field1/><field2/></recset></DataList>";
            var serviceAction = new ServiceAction() { DataListSpecification = new StringBuilder(Datalist) };
            var dsfObj = new Mock<IDSFDataObject>();
            dsfObj.Setup(o => o.Environment).Returns(new ExecutionEnvironment());
            dsfObj.Setup(o => o.Environment.Eval(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>(), false))
                  .Returns(CommonFunctions.WarewolfEvalResult.NewWarewolfAtomResult(DataStorage.WarewolfAtom.NewDataString("Args")));
            var workSpace = new Mock<IWorkspace>();
            var channel = new Mock<IEsbChannel>();
            var esbExecuteRequest = new EsbExecuteRequest();
            var serviceTestExecutionContainer = new ServiceTestExecutionContainerMock(serviceAction, dsfObj.Object, workSpace.Object, channel.Object, esbExecuteRequest);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(serviceTestExecutionContainer);
            Assert.IsNull(serviceTestExecutionContainer.InstanceOutputDefinition);
            Assert.IsNull(serviceTestExecutionContainer.InstanceInputDefinition);
            //---------------Execute Test ----------------------
            var execute = serviceTestExecutionContainer.CanExecute(Guid.NewGuid(), dsfObj.Object, AuthorizationContext.Administrator);
            //---------------Test Result -----------------------
            Assert.IsTrue(execute);
        }
    }

    internal class ServiceTestExecutionContainerMock : ServiceTestExecutionContainer
    {
        public ServiceTestExecutionContainerMock(ServiceAction sa, IDSFDataObject dataObj, IWorkspace theWorkspace, IEsbChannel esbChannel, EsbExecuteRequest request)
            : base(sa, dataObj, theWorkspace, esbChannel, request)
        {

        }
        public ServiceTestExecutionContainerMock(ServiceAction sa, IDSFDataObject dataObj, IWorkspace theWorkspace, IEsbChannel esbChannel, EsbExecuteRequest request, ITestCatalog catalog, IResourceCatalog resourceCatalog)
            : base(sa, dataObj, theWorkspace, esbChannel, request)
        {
            TstCatalog = catalog;
            ResourceCat = resourceCatalog;
        }

        public ServiceTestExecutionContainerMock(IImpersonator imp, ServiceAction sa, IDSFDataObject dataObj, IWorkspace theWorkspace, IEsbChannel esbChannel, EsbExecuteRequest request, ITestCatalog catalog, IResourceCatalog resourceCatalog)
            : base(imp,sa, dataObj, theWorkspace, esbChannel, request)
        {
            TstCatalog = catalog;
            ResourceCat = resourceCatalog;
        }

    }
}
