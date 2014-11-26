
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

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
