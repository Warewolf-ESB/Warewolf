/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

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
            a.Repetition.Duration = TimeSpan.FromMinutes(2);//new TimeSpan(2); // Minimum allowed time is 1 minute
            a.Repetition.Interval = TimeSpan.FromMinutes(3); //new TimeSpan(3); // Minimum allowed time is 1 minute
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
