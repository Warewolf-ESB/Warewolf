using System;
using System.Reflection;
using Dev2.Common.Interfaces.Monitoring;
using Dev2.Data.ServiceModel;
using Dev2.Data.TO;
using Dev2.DynamicServices.Objects;
using Dev2.Interfaces;
using Dev2.PerformanceCounters.Counters;
using Dev2.Runtime.ESB.Control;
using Dev2.Runtime.ESB.Execution;
using Dev2.Runtime.Interfaces;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Storage;

// ReSharper disable InconsistentNaming

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


       
    }
}
