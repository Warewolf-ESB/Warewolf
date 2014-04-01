using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32.TaskScheduler;

namespace Dev2.TaskScheduler.Wrappers.Test
{
    [TestClass]
    public class Dev2RepetitionPatternTest
    {

              [TestMethod]
              [Owner("Leon Rajindrapersadh")]
              [TestCategory("TaskShedulerWrapper_Dev2RepetitionPattern_Construct")]
              public void TaskShedulerWrapper_Dev2RepetitionPattern_Construct()
              {
                  Trigger a = new DailyTrigger(6);
                  var native = a.Repetition;
                  a.Repetition.Duration = new TimeSpan(2);
                  a.Repetition.Interval = new TimeSpan(3);
                  a.Repetition.StopAtDurationEnd = true;

                  var patt = new Dev2RepetitionPattern(a.Repetition);
                  Assert.AreEqual(patt.Duration,native.Duration);
                  Assert.AreEqual(patt.Interval,native.Interval);
                  Assert.AreEqual(patt.StopAtDurationEnd, native.StopAtDurationEnd);
                  Assert.AreEqual(patt.IsSet(),native.IsSet());
                  native.Dispose();

              }
    }
}
