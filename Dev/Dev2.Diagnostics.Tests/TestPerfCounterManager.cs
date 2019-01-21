using System;
using System.Collections.Generic;
using System.Diagnostics;
using Dev2.Common;
using Dev2.Common.Interfaces.Monitoring;
using Dev2.PerformanceCounters.Counters;
using Dev2.PerformanceCounters.Management;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Diagnostics.Test
{
    [TestClass]
    public class TestPerfCounterManager
    {
        public static Guid CounterGuid = Guid.NewGuid();
        [TestInitialize]
        public void Init()
        {
            try
            {
                try
                {
                    PerformanceCounterCategory.Delete("Warewolf");
                }
                    
                catch
                {

                }

            }
            catch (Exception err)
            {
                // ignored
                Dev2Logger.Error(err, GlobalConstants.WarewolfError);
            }
        }


        [TestMethod]
        public void TestManagerLocater_ReturnsSameCounter()
        {
            var performanceCounterFactory = new Mock<IRealPerformanceCounterFactory>().Object;
            var register = new WarewolfPerformanceCounterRegister(new List<IPerformanceCounter>
                {
                    new WarewolfCurrentExecutionsPerformanceCounter(performanceCounterFactory),
                    new WarewolfNumberOfErrors(performanceCounterFactory),
                    new WarewolfRequestsPerSecondPerformanceCounter(performanceCounterFactory),
                    new WarewolfAverageExecutionTimePerformanceCounter(performanceCounterFactory),
                    new WarewolfNumberOfAuthErrors(performanceCounterFactory),
                    new WarewolfServicesNotFoundCounter(performanceCounterFactory),

                }, new List<IResourcePerformanceCounter>());
            var manager = new WarewolfPerformanceCounterManager(register.Counters, new List<IResourcePerformanceCounter>(), register, new Mock<IPerformanceCounterPersistence>().Object, performanceCounterFactory);

            var counter = manager.GetCounter(WarewolfPerfCounterType.ConcurrentRequests).FromSafe(); ;
            var counter2 = manager.GetCounter("Concurrent requests currently executing").FromSafe(); ;
            Assert.AreEqual(counter, counter2);

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("PerformanceCounterManager_CreateAndRetrieve")]
        public void PerformanceCounterManager_CreateAndRetrieve_CreateAndRetrieve_ExpectValid()
        {
            var performanceCounterFactory = new Mock<IRealPerformanceCounterFactory>().Object;
            var register = new WarewolfPerformanceCounterRegister(new List<IPerformanceCounter>
                {
                    new WarewolfCurrentExecutionsPerformanceCounter(performanceCounterFactory),
                    new WarewolfNumberOfErrors(performanceCounterFactory),
                    new WarewolfRequestsPerSecondPerformanceCounter(performanceCounterFactory),
                    new WarewolfAverageExecutionTimePerformanceCounter(performanceCounterFactory),
                    new WarewolfNumberOfAuthErrors(performanceCounterFactory),
                    new WarewolfServicesNotFoundCounter(performanceCounterFactory),

                }, new List<IResourcePerformanceCounter>());
            var manager = new WarewolfPerformanceCounterManager(register.Counters, new List<IResourcePerformanceCounter>(), register, new Mock<IPerformanceCounterPersistence>().Object, performanceCounterFactory);
            manager.CreateCounter(CounterGuid, WarewolfPerfCounterType.ExecutionErrors, "bob");
            var counter = manager.GetCounter(CounterGuid, WarewolfPerfCounterType.ExecutionErrors).FromSafe() as IResourcePerformanceCounter;
            Assert.IsNotNull(manager.EmptyCounter);
            Assert.IsNotNull(counter);
            Assert.IsFalse(counter is EmptyCounter);
            Assert.AreEqual("bob",counter.CategoryInstanceName);
            Assert.AreEqual(counter.ResourceId, CounterGuid);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("PerformanceCounterManager_CreateAndRetrieve")]
        public void PerformanceCounterManager_CreateAndRetrieve_CreateUnknownCreatesEmpty()
        {
            var performanceCounterFactory = new Mock<IRealPerformanceCounterFactory>().Object;
            var register = new WarewolfPerformanceCounterRegister(new List<IPerformanceCounter>
                {
                    new WarewolfCurrentExecutionsPerformanceCounter(performanceCounterFactory),
                    new WarewolfNumberOfErrors(performanceCounterFactory),
                    new WarewolfRequestsPerSecondPerformanceCounter(performanceCounterFactory),
                    new WarewolfAverageExecutionTimePerformanceCounter(performanceCounterFactory),
                    new WarewolfNumberOfAuthErrors(performanceCounterFactory),
                    new WarewolfServicesNotFoundCounter(performanceCounterFactory),

                }, new List<IResourcePerformanceCounter>());
            var manager = new WarewolfPerformanceCounterManager(register.Counters, new List<IResourcePerformanceCounter>(), register, new Mock<IPerformanceCounterPersistence>().Object, performanceCounterFactory);
            manager.CreateCounter(CounterGuid, WarewolfPerfCounterType.NotAuthorisedErrors, "bob");
            var counter = manager.GetCounter(CounterGuid, WarewolfPerfCounterType.NotAuthorisedErrors).FromSafe() as IResourcePerformanceCounter;
            Assert.IsTrue(counter is EmptyCounter);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("PerformanceCounterManager_CreateAndRetrieve")]
        public void PerformanceCounterManager_Remove_ExpectEmptyValidFromRetrieve()
        {
            var performanceCounterFactory = new Mock<IRealPerformanceCounterFactory>().Object;
            var register = new WarewolfPerformanceCounterRegister(new List<IPerformanceCounter>
                {
                    new WarewolfCurrentExecutionsPerformanceCounter(performanceCounterFactory),
                    new WarewolfNumberOfErrors(performanceCounterFactory),
                    new WarewolfRequestsPerSecondPerformanceCounter(performanceCounterFactory),
                    new WarewolfAverageExecutionTimePerformanceCounter(performanceCounterFactory),
                    new WarewolfNumberOfAuthErrors(performanceCounterFactory),
                    new WarewolfServicesNotFoundCounter(performanceCounterFactory),

                }, new List<IResourcePerformanceCounter>());
            var manager = new WarewolfPerformanceCounterManager(register.Counters, new List<IResourcePerformanceCounter>(), register, new Mock<IPerformanceCounterPersistence>().Object, performanceCounterFactory);
            manager.CreateCounter(CounterGuid, WarewolfPerfCounterType.ExecutionErrors, "bob");
            manager.RemoverCounter(CounterGuid, WarewolfPerfCounterType.ExecutionErrors,"bob");
            var counter = manager.GetCounter(CounterGuid, WarewolfPerfCounterType.ExecutionErrors).FromSafe() as IResourcePerformanceCounter;
            Assert.IsNotNull(manager.EmptyCounter);
            Assert.IsNotNull(counter);
            Assert.IsTrue(counter is EmptyCounter);

        }

    }
}