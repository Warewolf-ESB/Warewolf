using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Monitoring;
using Dev2.Communication;
using Dev2.PerformanceCounters.Management;
using Dev2.Runtime.ESB.Management.Services;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;


namespace Dev2.Tests.Runtime.Services
{
    [TestClass]
    public class FetchPerformanceCountersTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("GetResourceID")]
        public void GetResourceID_ShouldReturnEmptyGuid()
        {
            //------------Setup for test--------------------------
            var fetchPerformanceCounters = new FetchPerformanceCounters();

            //------------Execute Test---------------------------
            var resId = fetchPerformanceCounters.GetResourceID(new Dictionary<string, StringBuilder>());
            //------------Assert Results-------------------------
            Assert.AreEqual(Guid.Empty, resId);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("GetResourceID")]
        public void GetAuthorizationContextForService_ShouldReturnContext()
        {
            //------------Setup for test--------------------------
            var fetchPerformanceCounters = new FetchPerformanceCounters();

            //------------Execute Test---------------------------
            var resId = fetchPerformanceCounters.GetAuthorizationContextForService();
            //------------Assert Results-------------------------
            Assert.AreEqual(AuthorizationContext.Any, resId);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("FetchPerformanceCounters_HandlesType")]
        public void FetchPerformanceCounters_HandlesType_Get_ReturnsKnownString()
        {
            //------------Setup for test--------------------------
            var fetchPerformanceCounters = new FetchPerformanceCounters();
            
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.AreEqual(fetchPerformanceCounters.HandlesType(), "FetchPerformanceCounters");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("FetchPerformanceCounters_HandlesType")]
        public void FetchPerformanceCounters_HandlesType_Get_DynamicServiceEntry()
        {
            //------------Setup for test--------------------------
            var fetchPerformanceCounters = new FetchPerformanceCounters();

            //------------Execute Test---------------------------
           var entry =  fetchPerformanceCounters.CreateServiceEntry();
            //------------Assert Results-------------------------

           Assert.AreEqual("<DataList><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>", entry.DataListSpecification.ToString());
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("FetchPerformanceCounters_Manager")]

        public void FetchPerformanceCounters_Manager_ExceptionIfContainerNotRegistered()
        {
            //------------Setup for test--------------------------
            var fetchPerformanceCounters = new FetchPerformanceCounters();
            CustomContainer.DeRegister<IPerformanceCounterRepository>();
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            PrivateObject p = new PrivateObject(fetchPerformanceCounters);
            var nll =   p.GetProperty("Manager");
            Assert.IsNull(nll);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("FetchPerformanceCounters_Manager")]
        public void FetchPerformanceCounters_Manager_ValueIfRegistered()
        {

            //------------Setup for test--------------------------
            var mng = new Mock<IPerformanceCounterRepository>();
            CustomContainer.Register(mng.Object);
            var fetchPerformanceCounters = new FetchPerformanceCounters();

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            PrivateObject p = new PrivateObject(fetchPerformanceCounters);
           Assert.IsNotNull( p.GetProperty("Manager"));
           Assert.IsTrue(ReferenceEquals( mng.Object, p.GetProperty("Manager")));
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("FetchPerformanceCounters_Manager")]

        public void FetchPerformanceCounters_Manager_ExecuteReturnsAValidTo()
        {

            //------------Setup for test--------------------------
            var mng = new Mock<IPerformanceCounterRepository>();
            mng.Setup(a => a.Counters).Returns(new PerformanceCounterTo(new List<IPerformanceCounter>(), new List<IPerformanceCounter>()));
            CustomContainer.Register(mng.Object);
            var fetchPerformanceCounters = new FetchPerformanceCounters();

            //------------Execute Test---------------------------
           var output =  fetchPerformanceCounters.Execute(new Dictionary<string, StringBuilder>(),new Mock<IWorkspace>().Object);
            //------------Assert Results-------------------------
            Assert.IsNotNull(output);
            Dev2JsonSerializer ser = new Dev2JsonSerializer();
            var res =   ser.Deserialize<IPerformanceCounterTo>(output);
            Assert.IsNotNull(res);
        }

    }
}
