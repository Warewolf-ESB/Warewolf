using System;
using System.Linq;
using System.Reflection;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Monitoring;
using Dev2.Data.ServiceModel;
using Dev2.Data.TO;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Interfaces;
using Dev2.PerformanceCounters.Counters;
using Dev2.Runtime.ESB.Control;
using Dev2.Runtime.ESB.Execution;
using Dev2.Runtime.Interfaces;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Resource.Errors;
using Warewolf.Storage;
using Warewolf.Storage.Interfaces;



namespace Dev2.Tests.Runtime.ESB.Control
{
    [TestClass]
    public class EsbServicesEndpointTests
    {
        [ClassInitialize]
        public static void Init(TestContext context)
        {
            var pCounter = new Mock<IWarewolfPerformanceCounterLocater>();
            pCounter.Setup(locater => locater.GetCounter(It.IsAny<Guid>(), It.IsAny<WarewolfPerfCounterType>())).Returns(new EmptyCounter());
            pCounter.Setup(locater => locater.GetCounter(It.IsAny<WarewolfPerfCounterType>())).Returns(new EmptyCounter());
            pCounter.Setup(locater => locater.GetCounter(It.IsAny<string>())).Returns(new EmptyCounter());
            CustomContainer.Register(pCounter.Object);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void OnConstruction_ShouldNotThrowException()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
                var esbServicesEndpoint = new EsbServicesEndpoint();
            //---------------Test Result -----------------------
            Assert.IsNotNull(esbServicesEndpoint, "Cannot create new EsbServicesEndpoint object.");
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void SetRemoteExecutionDataList_GivenDataObject_ShouldSetValuesCorreclty()
        {
            //---------------Set up test pack-------------------
            var obj = new Mock<IDSFDataObject>();
            var sa = new ServiceAction();
            var dataObj = new Mock<IDSFDataObject>();
            dataObj.SetupAllProperties();
            var workspace = new Mock<IWorkspace>();
            var esbChannel = new Mock<IEsbChannel>();
            var cat = new Mock<IResourceCatalog>();
            var execContainer = new Mock<RemoteWorkflowExecutionContainer>(sa, dataObj.Object, workspace.Object, esbChannel.Object, cat.Object);
            execContainer.Setup(container => container.FetchRemoteResource(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(new SerializableResource()
            {
                DataList = "<DataList></DataList>",
                ResourceCategory = "Cat"
            });
            var methodInfo = typeof(EsbServicesEndpoint).GetMethod("SetRemoteExecutionDataList", BindingFlags.NonPublic | BindingFlags.Static);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(methodInfo);
            //---------------Execute Test ----------------------
            try
            {
                //---------------Test Result -----------------------
                methodInfo.Invoke(null, new object[] { obj.Object, execContainer.Object, new ErrorResultTO() });
                obj.VerifyAll();
            }
            catch (Exception ex)
            {
                Assert.AreEqual("", ex.Message);

            }
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CreateNewEnvironmentFromInputMappings_GivenInputsDefs_ShouldCreateNewEnvWithMappings()
        {
            //---------------Set up test pack-------------------
            var dataObj = new Mock<IDSFDataObject>();
            dataObj.SetupAllProperties();
            IExecutionEnvironment executionEnvironment = new ExecutionEnvironment();
            dataObj.Setup(o => o.Environment).Returns(executionEnvironment).Verifiable();
            dataObj.Setup(o => o.PushEnvironment(It.IsAny<IExecutionEnvironment>())).Verifiable();
            const string inputMappings = @"<Inputs><Input Name=""f1"" Source=""[[recset1(*).f1a]]"" Recordset=""recset1"" /><Input Name=""f2"" Source=""[[recset2(*).f2a]]"" Recordset=""recset2"" /></Inputs>";
            var esbServicesEndpoint = new EsbServicesEndpoint();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            esbServicesEndpoint.CreateNewEnvironmentFromInputMappings(dataObj.Object, inputMappings, 0);
            //---------------Test Result -----------------------
            dataObj.Verify(o => o.PushEnvironment(It.IsAny<IExecutionEnvironment>()), Times.Once);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void UpdatePreviousEnvironmentWithSubExecutionResultUsingOutputMappings_GivenOutPuts_ShouldReturnCorrectly()
        {
            //---------------Set up test pack-------------------
            const string outPuts = "<Outputs><Output Name=\"n1\" MapsTo=\"[[n1]]\" Value=\"[[n1]]\" IsObject=\"False\" /></Outputs>";
            var dataObj = new Mock<IDSFDataObject>();
            dataObj.SetupAllProperties();
            
            var mapManager = new Mock<IEnvironmentOutputMappingManager>();
            mapManager.Setup(manager => manager.UpdatePreviousEnvironmentWithSubExecutionResultUsingOutputMappings(It.IsAny<IDSFDataObject>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<ErrorResultTO>())).Returns(new ExecutionEnvironment());
            var esbServicesEndpoint = new EsbServicesEndpoint(mapManager.Object);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var executionEnvironment = esbServicesEndpoint.UpdatePreviousEnvironmentWithSubExecutionResultUsingOutputMappings(dataObj.Object, outPuts, 0, true, new ErrorResultTO());
            //---------------Test Result -----------------------
            Assert.IsNotNull(executionEnvironment);
            mapManager.VerifyAll();
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ExecuteSubRequest_GivenExecuteWorkflowAsync_ShouldCheckIsRemoteWorkflow2()
        {
            //---------------Set up test pack-------------------
            var dataObj = new Mock<IDSFDataObject>();
            var dataObjClon = new Mock<IDSFDataObject>();
            dataObjClon.Setup(o => o.ServiceName).Returns("Service Name");
            var rCat = new Mock<IResourceCatalog>();
            var mock = new Mock<IResource>();
            rCat.Setup(catalog => catalog.GetResource(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(mock.Object);
            var workRepo = new Mock<IWorkspaceRepository>();
            workRepo.Setup(repository => repository.Get(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(new Workspace(Guid.NewGuid()));
            dataObj.SetupAllProperties();
            dataObj.Setup(o => o.Environment).Returns(new ExecutionEnvironment());
            dataObj.Setup(o => o.IsRemoteWorkflow());
            dataObj.Setup(o => o.RunWorkflowAsync).Returns(true);
            dataObj.Setup(o => o.Clone()).Returns(dataObjClon.Object);
            var mapManager = new Mock<IEnvironmentOutputMappingManager>();
            var esbServicesEndpoint = new EsbServicesEndpoint();
            PrivateObject privateObject = new PrivateObject(esbServicesEndpoint);
            var invokerMock = new Mock<IEsbServiceInvoker>();
            var remoteWorkflowExecutionContainer = new RemoteWorkflowExecutionContainer(new ServiceAction(), dataObj.Object, new Mock<IWorkspace>().Object, new Mock<IEsbChannel>().Object);
            invokerMock.Setup(invoker => invoker.GenerateInvokeContainer(It.IsAny<IDSFDataObject>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<Guid>())).Returns(remoteWorkflowExecutionContainer);
            ErrorResultTO err = new ErrorResultTO();
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            object[] args = { dataObj.Object, "inputs", invokerMock.Object, false, Guid.Empty, err, 0 };
            privateObject.Invoke("ExecuteRequestAsync", args);
            Assert.IsNotNull(esbServicesEndpoint);
            var errorResultTO = args[5] as ErrorResultTO;
            //---------------Test Result -----------------------
            var errors = errorResultTO?.FetchErrors();
            Assert.IsNotNull(errors);
            Assert.IsTrue(errors.Count > 0);
            Assert.IsTrue(errors.Any(p => p.Contains("Asynchronous execution failed: Remote server unreachable")));
            Assert.IsTrue(errors.Any(p => p.Contains("Service not found")));
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ExecuteSubRequest_GivenValidArgs_ShouldCheckIsRemoteWorkflow()
        {
            //---------------Set up test pack-------------------
            var dataObj = new Mock<IDSFDataObject>();
            var rCat = new Mock<IResourceCatalog>();
            var mock = new Mock<IResource>();
            rCat.Setup(catalog => catalog.GetResource(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(mock.Object);
            var workRepo = new Mock<IWorkspaceRepository>();
            workRepo.Setup(repository => repository.Get(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(new Workspace(Guid.NewGuid()));
            dataObj.SetupAllProperties();
            dataObj.Setup(o => o.Environment).Returns(new ExecutionEnvironment());
            dataObj.Setup(o => o.IsRemoteWorkflow());
            dataObj.Setup(o => o.ServiceName).Returns("SomeName");
            var esbServicesEndpoint = new EsbServicesEndpoint();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(esbServicesEndpoint);
            //---------------Execute Test ----------------------
            esbServicesEndpoint.ExecuteSubRequest(dataObj.Object, Guid.NewGuid(), "", "", out ErrorResultTO err, 1, true);

            //---------------Test Result -----------------------
            dataObj.Verify(o => o.IsRemoteWorkflow(), Times.Once);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ExecuteSubRequest_GivenExecuteWorkflowAsync_ShouldCheckIsRemoteWorkflow()
        {
            //---------------Set up test pack-------------------
            var dataObj = new Mock<IDSFDataObject>();
            var dataObjClon = new Mock<IDSFDataObject>();
            dataObjClon.Setup(o => o.ServiceName).Returns("Service Name");
            var rCat = new Mock<IResourceCatalog>();
            var mock = new Mock<IResource>();
            rCat.Setup(catalog => catalog.GetResource(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(mock.Object);
            var workRepo = new Mock<IWorkspaceRepository>();
            workRepo.Setup(repository => repository.Get(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(new Workspace(Guid.NewGuid()));
            dataObj.SetupAllProperties();
            dataObj.Setup(o => o.Environment).Returns(new ExecutionEnvironment());
            dataObj.Setup(o => o.IsRemoteWorkflow());
            dataObj.Setup(o => o.RunWorkflowAsync).Returns(true);
            dataObj.Setup(o => o.Clone()).Returns(dataObjClon.Object);
            var mapManager = new Mock<IEnvironmentOutputMappingManager>();
            var esbServicesEndpoint = new EsbServicesEndpoint();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(esbServicesEndpoint);
            //---------------Execute Test ----------------------

            esbServicesEndpoint.ExecuteSubRequest(dataObj.Object, Guid.NewGuid(), "", "", out ErrorResultTO err, 1, true);

            //---------------Test Result -----------------------
            dataObj.Verify(o => o.IsRemoteWorkflow(), Times.Once);
            var contains = err.FetchErrors().Contains(ErrorResource.ResourceNotFound);
            Assert.IsTrue(contains);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ExecuteLogErrorRequest_GivenCorrectUri_ShouldNoThrowException()
        {
            //---------------Set up test pack-------------------
            var dataObj = new Mock<IDSFDataObject>();
            var rCat = new Mock<IResourceCatalog>();
            rCat.Setup(catalog => catalog.GetResource(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Verifiable();
            var workRepo = new Mock<IWorkspaceRepository>();
            workRepo.Setup(repository => repository.Get(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(new Workspace(Guid.NewGuid()));
            dataObj.SetupAllProperties();
            dataObj.Setup(o => o.Environment).Returns(new ExecutionEnvironment());

            var mapManager = new Mock<IEnvironmentOutputMappingManager>();
            var esbServicesEndpoint = new EsbServicesEndpoint();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(esbServicesEndpoint);
            //---------------Execute Test ----------------------
            try
            {
                esbServicesEndpoint.ExecuteLogErrorRequest(dataObj.Object, It.IsAny<Guid>(), "http://example.com/", out ErrorResultTO err, 1);
            }
            catch (Exception ex)
            {
                //---------------Test Result -----------------------
                Assert.Fail(ex.Message);
            }
        }


    }
}
