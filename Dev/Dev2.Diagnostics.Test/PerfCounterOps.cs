using System;
using System.Collections.Generic;
using System.Diagnostics;
using Dev2.Common;
using Dev2.Common.Interfaces.Monitoring;
using Dev2.PerformanceCounters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
// ReSharper disable InconsistentNaming

namespace Dev2.Diagnostics.Test
{
    [TestClass]
    public class TestPerfCounters
    {
        [TestInitialize]
        public void Init()
        {
            try
            {
                try
                {
                    PerformanceCounterCategory.Delete("Warewolf");
                }
                // ReSharper disable once EmptyGeneralCatchClause
                catch
                {

                }

                WarewolfPerformanceCounterRegister register = new WarewolfPerformanceCounterRegister(new List<IPerformanceCounter>
                                                            {
                                                                new WarewolfCurrentExecutionsPerformanceCounter(),
                                                                new WarewolfNumberOfErrors(),    
                                                                new WarewolfRequestsPerSecondPerformanceCounter(),
                                                                new WarewolfAverageExecutionTimePerformanceCounter(),
                                                                new WarewolfNumberOfAuthErrors(),
                                                                new WarewolfServicesNotFoundCounter()
                                                            });
                CustomContainer.Register<IWarewolfPerformanceCounterLocater>(new WarewolfPerformanceCounterLocater(register.Counters,register));
            }
            catch (Exception err)
            {
                // ignored
                Dev2Logger.Error(err);
            }
        }

        [TestMethod]
        public void ConcurrentCounterTest()
        {
            var counter = CustomContainer.Get<IWarewolfPerformanceCounterLocater>().GetCounter(WarewolfPerfCounterType.ConcurrentRequests);
            Assert.AreEqual(counter.Name, "Concurrent requests currently executing");
            Assert.AreEqual(counter.Category, "Warewolf");
            PrivateObject po = new PrivateObject(counter);
            po.Invoke("Setup", new object[0]);
            var innerCounter = po.GetField("_counter") as PerformanceCounter;
            Assert.IsNotNull(innerCounter);
            Assert.AreEqual(innerCounter.RawValue, 0);
            counter.Increment();
            Assert.AreEqual(innerCounter.RawValue, 1);
            counter.Decrement();
            Assert.AreEqual(innerCounter.RawValue, 0);
            counter.IncrementBy(3);
            Assert.AreEqual(innerCounter.RawValue, 3);
        }

        [TestMethod]
        public void TestLocater()
        {
            var counter = CustomContainer.Get<IWarewolfPerformanceCounterLocater>().GetCounter(WarewolfPerfCounterType.ConcurrentRequests);
            var counter2 = CustomContainer.Get<IWarewolfPerformanceCounterLocater>().GetCounter("Concurrent requests currently executing");
            Assert.AreEqual(counter, counter2);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfAverageExectionTimeCounter_TestOps")]
        public void WarewolfErrorCounter_TestOps_Valid_ExpectValidValues()
        {
            var counter = CustomContainer.Get<IWarewolfPerformanceCounterLocater>().GetCounter(WarewolfPerfCounterType.ExecutionErrors);
            Assert.AreEqual(counter.Name, "Total Errors");
            Assert.AreEqual(counter.Category, "Warewolf");
            PrivateObject po = new PrivateObject(counter);
            po.Invoke("Setup", new object[0]);
            var innerCounter = po.GetField("_counter") as PerformanceCounter;
            Assert.IsNotNull(innerCounter);
            Assert.AreEqual(innerCounter.RawValue, 0);
            counter.Increment();
            Assert.AreEqual(innerCounter.RawValue, 1);
            counter.Decrement();
            Assert.AreEqual(innerCounter.RawValue, 0);
            counter.IncrementBy(3);
            Assert.AreEqual(innerCounter.RawValue, 3);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfAverageExectionTimeCounter_TestOps")]
        public void WarewolfRequestPerSecondCounter_TestOps_Valid_ExpectValidValues()
        {
            var counter = CustomContainer.Get<IWarewolfPerformanceCounterLocater>().GetCounter(WarewolfPerfCounterType.RequestsPerSecond);
            Assert.AreEqual(counter.Name, "Request Per Second");
            Assert.AreEqual(counter.Category, "Warewolf");
            PrivateObject po = new PrivateObject(counter);
            po.Invoke("Setup", new object[0]);
            var innerCounter = po.GetField("_counter") as PerformanceCounter;
            Assert.IsNotNull(innerCounter);
            Assert.AreEqual(innerCounter.RawValue, 0);
            counter.Increment();
            Assert.AreEqual(innerCounter.RawValue, 1);
            counter.Decrement();
            Assert.AreEqual(innerCounter.RawValue, 0);
            counter.IncrementBy(3);
            Assert.AreEqual(innerCounter.RawValue, 3);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfAverageExectionTimeCounter_TestOps")]
        public void WarewolfServicesNotFound_TestOps_Valid_ExpectValidValues()
        {
            var counter = CustomContainer.Get<IWarewolfPerformanceCounterLocater>().GetCounter(WarewolfPerfCounterType.ServicesNotFound);
            Assert.AreEqual(counter.Name, "Count of requests for workflows which don’t exist");
            Assert.AreEqual(counter.Category, "Warewolf");
            PrivateObject po = new PrivateObject(counter);
            po.Invoke("Setup", new object[0]);
            var innerCounter = po.GetField("_counter") as PerformanceCounter;
            Assert.IsNotNull(innerCounter);
            Assert.AreEqual(innerCounter.RawValue, 0);
            counter.Increment();
            Assert.AreEqual(innerCounter.RawValue, 1);
            counter.Decrement();
            Assert.AreEqual(innerCounter.RawValue, 0);
            counter.IncrementBy(3);
            Assert.AreEqual(innerCounter.RawValue, 3);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfAverageExectionTimeCounter_TestOps")]
        public void WarewolfAuthErrors_TestOps_Valid_ExpectValidValues()
        {
            var counter = CustomContainer.Get<IWarewolfPerformanceCounterLocater>().GetCounter(WarewolfPerfCounterType.NotAuthorisedErrors);
            Assert.AreEqual(counter.Name, "Count of Not Authorised errors");
            Assert.AreEqual(counter.Category, "Warewolf");
            PrivateObject po = new PrivateObject(counter);
            po.Invoke("Setup", new object[0]);
            var innerCounter = po.GetField("_counter") as PerformanceCounter;
            Assert.IsNotNull(innerCounter);
            Assert.AreEqual(innerCounter.RawValue, 0);
            counter.Increment();
            Assert.AreEqual(innerCounter.RawValue, 1);
            counter.Decrement();
            Assert.AreEqual(innerCounter.RawValue, 0);
            counter.IncrementBy(3);
            Assert.AreEqual(innerCounter.RawValue, 3);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfAverageExectionTimeCounter_TestOps")]
        public void WarewolfAverageExectionTimeCounter_TestOps_Valid_ExpectValidValues()
        {
            //------------Setup for test--------------------------
            var counter = CustomContainer.Get<IWarewolfPerformanceCounterLocater>().GetCounter(WarewolfPerfCounterType.AverageExecutionTime);
            Assert.AreEqual(counter.Name, "Average workflow execution time");
            Assert.AreEqual(counter.Category, "Warewolf");
            PrivateObject po = new PrivateObject(counter);
            po.Invoke("Setup", new object[0]);
            var innerCounter = po.GetField("_counter") as PerformanceCounter;
            var innerBase = po.GetField("_baseCounter") as PerformanceCounter;
            Assert.IsNotNull(innerCounter);
            Assert.IsNotNull(innerBase);
            Assert.AreEqual(innerCounter.RawValue, 0);
            Assert.AreEqual(innerBase.RawValue, 0);
            counter.Increment();
            Assert.AreEqual(innerCounter.RawValue, 1);
            Assert.AreEqual(innerBase.RawValue, 1);
            counter.Decrement();
            Assert.AreEqual(innerCounter.RawValue, 0);
            Assert.AreEqual(innerBase.RawValue, 0);
            counter.IncrementBy(3);
            Assert.AreEqual(innerCounter.RawValue, 3);
            Assert.AreEqual(innerBase.RawValue, 1);

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }
    }
}
