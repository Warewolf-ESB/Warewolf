using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Scheduler.Test
{
    [TestClass]
    public class EventInfoTest
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskSheduler_EventInfo_Test")]
        public void TaskSheduler_EventInfo_ShouldconstructCorrectly()
        {

            EventInfo ie = new EventInfo(new DateTime(2000, 1, 1), TimeSpan.MaxValue, new DateTime(2001, 1, 1), true,
                                         "12345");
            Assert.AreEqual(new DateTime(2000, 1, 1), ie.StartDate);
            Assert.AreEqual(new DateTime(2001, 1, 1), ie.EndDate);
            Assert.AreEqual(TimeSpan.MaxValue, ie.Duration);
            Assert.AreEqual(true, ie.Success);
            Assert.AreEqual("12345", ie.EventId);


        }
    }
}
