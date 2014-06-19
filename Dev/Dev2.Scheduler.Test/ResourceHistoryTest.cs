using System;
using System.Collections.Generic;
using Dev2.Diagnostics.Debug;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Scheduler.Test
{

    [TestClass]
    public class ResourceHistoryTest
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskSheduler_ResourceHistoryTest_Construct")]
        // ReSharper disable InconsistentNaming
        public void TaskSheduler_ResourceHistory_ShouldConstructCorrectly()
        // ReSharper restore InconsistentNaming
        {
            var a = new List<IDebugState>();
            var b = new EventInfo(new DateTime(2001, 01, 01), new TimeSpan(1, 0, 0), new DateTime(2001, 01, 01), false, "sdf");
            var res = new ResourceHistory("output", a, b, "bob");
            Assert.AreEqual(a, res.DebugOutput);
            Assert.AreEqual(b, res.TaskHistoryOutput);
            Assert.AreEqual("bob", res.UserName);

        }
    }
}
