// 
// /*
// *  Warewolf - The Easy Service Bus
// *  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
// *  Licensed under GNU Affero General Public License 3.0 or later. 
// *  Some rights reserved.
// *  Visit our website for more information <http://warewolf.io/>
// *  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
// *  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
// */

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Dev2.Common.Interfaces.Monitoring;
using Dev2.Communication;
using Dev2.PerformanceCounters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Diagnostics.Test
{
    public class TestCounter : IPerformanceCounter {
        #region Implementation of IPerformanceCounter

        public TestCounter()
        {
            Category = "";
            Name = "bob";
            PerfCounterType = WarewolfPerfCounterType.AverageExecutionTime;
        }

        public void Increment()
        {
        }

        public void IncrementBy(long ticks)
        {
        }

        public void Decrement()
        {
        }

        public string Category { get; private set; }
        public string Name { get; private set; }
        public WarewolfPerfCounterType PerfCounterType { get; private set; }

        public IList<CounterCreationData> CreationData()
        {
            return null;
        }

        public bool IsActive { get; set; }

        #endregion
    }

    [TestClass]
    public class PerformanceCounterPersistenceTest
    {

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("PerformanceCounterPersistence_Load")]
        public void CtorTest()
        {
            PerformanceCounterPersistence obj = new PerformanceCounterPersistence();
            Assert.IsNotNull(obj);
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("PerformanceCounterPersistence_Load")]
        public void PerformanceCounterPersistence_SaveEmpty_ExpectCountersSaved()
        {
            
            PerformanceCounterPersistence obj = new PerformanceCounterPersistence();
            IList<IPerformanceCounter> counters = new List<IPerformanceCounter>();
            var fileName = Path.GetTempFileName();
            obj.Save(counters, fileName);
            var saved =  File.ReadAllText(fileName);
            Dev2JsonSerializer  serialiser = new Dev2JsonSerializer();
            var persisted = serialiser.Deserialize<IList<IPerformanceCounter>>(saved);

        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("PerformanceCounterPersistence_Load")]
        public void PerformanceCounterPersistence_Save_ExpectCountersSaved()
        {

            PerformanceCounterPersistence obj = new PerformanceCounterPersistence();
            IList<IPerformanceCounter> counters = new List<IPerformanceCounter>();
            counters.Add(new TestCounter());
            var fileName = Path.GetTempFileName();
            obj.Save(counters, fileName);
            var saved = File.ReadAllText(fileName);
            Dev2JsonSerializer serialiser = new Dev2JsonSerializer();
            var persisted = serialiser.Deserialize<IList<IPerformanceCounter>>(saved);
            Assert.AreEqual(persisted.Count,1);
            Assert.IsTrue(persisted.First() is TestCounter);
        }





        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("PerformanceCounterPersistence_Load")]
        public void PerformanceCounterPersistence_Load_ExpectCountersLoaded()
        {

            PerformanceCounterPersistence obj = new PerformanceCounterPersistence();
            IList<IPerformanceCounter> counters = new List<IPerformanceCounter>();
            counters.Add(new TestCounter());
            var fileName = Path.GetTempFileName();
            obj.Save(counters, fileName);
            var saved = File.ReadAllText(fileName);
            Dev2JsonSerializer serialiser = new Dev2JsonSerializer();
            var persisted = obj.LoadOrCreate(fileName);
            Assert.AreEqual(persisted.Count, 1);
            Assert.IsTrue(persisted.First() is TestCounter);
        }

    }
}