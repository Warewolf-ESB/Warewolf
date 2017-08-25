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
    public class TestPerfCounters
    {
        public static Guid guid = Guid.NewGuid();
        [TestInitialize]
        public void Init()
        {
            try
            {
                try
                {
                    PerformanceCounterCategory.Delete("Warewolf");
                    PerformanceCounterCategory.Delete("Warewolf Services");
                }
                
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
                                                                new WarewolfServicesNotFoundCounter(),

                                                            }, new List<IResourcePerformanceCounter>{
                                                       new WarewolfCurrentExecutionsPerformanceCounterByResource(Guid.Empty, ""),
                                                       new WarewolfNumberOfErrorsByResource(Guid.Empty, ""),
                                                       new WarewolfRequestsPerSecondPerformanceCounterByResource(Guid.Empty, ""),
                                                       new WarewolfAverageExecutionTimePerformanceCounterByResource(Guid.Empty, ""),

                                                    });
                var manager = new WarewolfPerformanceCounterManager(register.Counters, new List<IResourcePerformanceCounter>(), register, new Mock<IPerformanceCounterPersistence>().Object);
                manager.CreateCounter( guid,WarewolfPerfCounterType.ExecutionErrors, "bob");
                manager.CreateCounter(guid, WarewolfPerfCounterType.AverageExecutionTime, "bob");
                manager.CreateCounter(guid, WarewolfPerfCounterType.RequestsPerSecond, "bob");
                manager.CreateCounter(guid, WarewolfPerfCounterType.ConcurrentRequests, "bob");
                                                                
                CustomContainer.Register<IWarewolfPerformanceCounterLocater>(manager);
            }
            catch (Exception err)
            {
                // ignored
                Dev2Logger.Error(err, GlobalConstants.WarewolfError);
            }
        }


        [TestMethod]
        public void TestLocater()
        {
            var counter = CustomContainer.Get<IWarewolfPerformanceCounterLocater>().GetCounter(WarewolfPerfCounterType.ConcurrentRequests).FromSafe(); ;
            var counter2 = CustomContainer.Get<IWarewolfPerformanceCounterLocater>().GetCounter("Concurrent requests currently executing").FromSafe(); ;
            Assert.AreEqual(counter, counter2);

        }

        [TestMethod]
        public void ConcurrentCounterTest()
        {
            var counter = CustomContainer.Get<IWarewolfPerformanceCounterLocater>().GetCounter(WarewolfPerfCounterType.ConcurrentRequests).FromSafe();
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
        public void ConcurrentCounterTestByResource()
        {
            var counter = CustomContainer.Get<IWarewolfPerformanceCounterLocater>().GetCounter(guid,WarewolfPerfCounterType.ConcurrentRequests).FromSafe();
            Assert.AreEqual(counter.Name, "Concurrent requests currently executing");
            Assert.AreEqual(counter.Category, GlobalConstants.WarewolfServices);
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
            var resourcePerformanceCounter = counter as IResourcePerformanceCounter;
            if (resourcePerformanceCounter != null)
            {
                Assert.AreEqual(guid, resourcePerformanceCounter.ResourceId);
            }
            else
            {
                Assert.Fail("Then you'll see, that it is not the spoon that bends, it is only yourself. ");
            }
        }



        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfErrorCounter_TestOps")]
        public void WarewolfErrorCounter_TestOps_Valid_ExpectValidValues()
        {
            var counter = CustomContainer.Get<IWarewolfPerformanceCounterLocater>().GetCounter(WarewolfPerfCounterType.ExecutionErrors).FromSafe(); ;
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
        [TestCategory("WarewolfErrorCounter_TestOps")]
        public void WarewolfErrorCounterResource_TestOps_Valid_ExpectValidValues()
        {
            var counter = CustomContainer.Get<IWarewolfPerformanceCounterLocater>().GetCounter(guid,WarewolfPerfCounterType.ExecutionErrors).FromSafe(); ;
            Assert.AreEqual(counter.Name, "Total Errors");
            Assert.AreEqual(counter.Category, GlobalConstants.WarewolfServices);
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
            var resourcePerformanceCounter = counter as IResourcePerformanceCounter;
            if(resourcePerformanceCounter != null)
            {
                Assert.AreEqual(guid, resourcePerformanceCounter.ResourceId);
            }
            else
            {
                Assert.Fail("Do not try and bend the spoon. That's impossible. Instead... only try to realize the truth.");
            }
        }



        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfEmptyCounter_TestOps")]
        public void WarewolfEmptyCounter_TestOps_Valid_ExpectValidValues()
        {
            var counter = CustomContainer.Get<IWarewolfPerformanceCounterLocater>().GetCounter(Guid.NewGuid(),WarewolfPerfCounterType.AverageExecutionTime).FromSafe(); ;
            Assert.AreEqual(counter.Name, "Empty");
            Assert.AreEqual(counter.Category, "Warewolf");
            PrivateObject po = new PrivateObject(counter);
            po.Invoke("Setup", new object[0]);

            Assert.IsNull(counter.CreationData());
            counter.Setup();

            counter.Increment();

            counter.Decrement();

            counter.IncrementBy(3);

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfRequestsPerSecondCounter_TestOps")]
        public void WarewolfRequestPerSecondCounter_TestOps_Valid_ExpectValidValues()
        {
            var counter = CustomContainer.Get<IWarewolfPerformanceCounterLocater>().GetCounter(WarewolfPerfCounterType.RequestsPerSecond).FromSafe(); ;
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
        [TestCategory("WarewolfRequestsPerSecondCounter_TestOps")]
        public void WarewolfRequestPerSecondCounterResource_TestOps_Valid_ExpectValidValues()
        {
            var counter = CustomContainer.Get<IWarewolfPerformanceCounterLocater>().GetCounter(guid, WarewolfPerfCounterType.RequestsPerSecond).FromSafe(); ;
            Assert.AreEqual(counter.Name, "Request Per Second");
            Assert.AreEqual(counter.Category, GlobalConstants.WarewolfServices);
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
            var resourcePerformanceCounter = counter as IResourcePerformanceCounter;
            if (resourcePerformanceCounter != null)
            {
                Assert.AreEqual(guid, resourcePerformanceCounter.ResourceId);
            }
            else
            {
                Assert.Fail("What truth? ");
            }
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfAverageExectionTimeCounter_TestOps")]
        public void WarewolfServicesNotFound_TestOps_Valid_ExpectValidValues()
        {
            var counter = CustomContainer.Get<IWarewolfPerformanceCounterLocater>().GetCounter(WarewolfPerfCounterType.ServicesNotFound).FromSafe(); ;
            Assert.AreEqual(counter.Name, "Count of requests for workflows which don't exist");
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
        public void WarewolfServicesNotFound_NotCauseErrorForResource_TestOps_Valid_ExpectValidValues()
        {
            var counter = CustomContainer.Get<IWarewolfPerformanceCounterLocater>().GetCounter(guid,WarewolfPerfCounterType.ServicesNotFound).FromSafe(); ;
            Assert.IsTrue(counter is EmptyCounter);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfAverageExectionTimeCounter_TestOps")]
        public void WarewolfAuthErrors_TestOps_Valid_ExpectValidValues()
        {
            var counter = CustomContainer.Get<IWarewolfPerformanceCounterLocater>().GetCounter(WarewolfPerfCounterType.NotAuthorisedErrors).FromSafe(); ;
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
            var counter = CustomContainer.Get<IWarewolfPerformanceCounterLocater>().GetCounter(WarewolfPerfCounterType.AverageExecutionTime).FromSafe(); ;
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


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfAverageExectionTimeCounter_TestOps")]
        public void WarewolfAverageExectionTimeCounterResource_TestOps_Valid_ExpectValidValues()
        {
            //------------Setup for test--------------------------
            var counter = CustomContainer.Get<IWarewolfPerformanceCounterLocater>().GetCounter(guid,WarewolfPerfCounterType.AverageExecutionTime).FromSafe(); ;
            Assert.AreEqual(counter.Name, "Average workflow execution time");
            Assert.AreEqual(counter.Category, GlobalConstants.WarewolfServices);
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
            var resourcePerformanceCounter = counter as IResourcePerformanceCounter;
            if (resourcePerformanceCounter != null)
            {
                Assert.AreEqual(guid, resourcePerformanceCounter.ResourceId);
            }
            else
            {
                Assert.Fail(@"There is no spoon.
Then you'll see, that it is not the spoon that bends, it is only yourself. ");
            }
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }



        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfErrorCounter_TestOps")]
        public void WarewolfErrorCounterResource_SafeCounterSwallowsExceptions()
        {
            bool incremented = false;
            bool decremented = false;
            bool incrementedBy = false;
            bool setup = true;
            var guid = Guid.NewGuid();
            var inner = new Mock<IResourcePerformanceCounter>();
            inner.Setup(a => a.Decrement()).Callback(() => decremented = true).Throws(new AccessViolationException());
            inner.Setup(a => a.Increment()).Callback(() => incremented = true).Throws(new AccessViolationException());
            inner.Setup(a => a.IncrementBy(5)).Callback((long a) => incrementedBy = true).Throws(new AccessViolationException());
            inner.Setup(a => a.CategoryInstanceName).Returns("bob");
            inner.Setup(a => a.ResourceId).Returns(guid);
            inner.Setup(a => a.Category).Returns("Neo");
            inner.Setup(a => a.Name).Returns("Morpheus");
            inner.Setup(a => a.IsActive).Returns(true);
            inner.Setup(a => a.Setup()).Callback(() => { setup = true; });
            var  safe = new SafeCounter(inner.Object);
            safe.Increment();
            Assert.IsTrue(incremented);
            safe.Decrement();
            Assert.IsTrue(decremented);
            safe.IncrementBy(5);
            Assert.IsTrue(incrementedBy);
            Assert.AreEqual(safe.InnerCounter,inner.Object);
            Assert.AreEqual("Neo",safe.Category);
            Assert.AreEqual(safe.Name, "Morpheus");
            Assert.IsTrue(setup);
        }
    }
}
