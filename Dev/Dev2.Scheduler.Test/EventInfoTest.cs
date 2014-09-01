using System;
using Dev2.Common.Interfaces.Scheduler.Interfaces;
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

            EventInfo ie = new EventInfo(new DateTime(2000, 1, 1), TimeSpan.MaxValue, new DateTime(2001, 1, 1),ScheduleRunStatus.Error, 
                                         "12345");
            Assert.AreEqual(new DateTime(2000, 1, 1), ie.StartDate);
            Assert.AreEqual(new DateTime(2001, 1, 1), ie.EndDate);
            Assert.AreEqual(TimeSpan.MaxValue, ie.Duration);
            Assert.AreEqual(ScheduleRunStatus.Error, ie.Success);
            Assert.AreEqual("12345", ie.EventId);


        }
    }
}
