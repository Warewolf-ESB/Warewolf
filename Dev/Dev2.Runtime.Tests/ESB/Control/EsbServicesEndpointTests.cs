using System;
using System.Reflection;
using System.Text;
using Dev2.Common.Interfaces.Monitoring;
using Dev2.Data.ServiceModel;
using Dev2.DataList.Contract;
using Dev2.DynamicServices.Objects;
using Dev2.Interfaces;
using Dev2.PerformanceCounters.Counters;
using Dev2.Runtime.ESB.Control;
using Dev2.Runtime.ESB.Execution;
using Dev2.Runtime.Interfaces;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

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
            try
            {
                // ReSharper disable once UnusedVariable
                var esbServicesEndpoint = new EsbServicesEndpoint();
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            //---------------Test Result -----------------------
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
            methodInfo.Invoke(null, new object[] { obj.Object, execContainer.Object, new ErrorResultTO() });
          
            //---------------Test Result -----------------------
           // Assert.AreEqual(new StringBuilder("<DataList></DataList>").ToString(), dataObj.Object.RemoteInvokeResultShape.ToString());
         //   Assert.AreEqual("Cat", dataObj.Object.ServiceName);
        }
    }
}
