
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
using System.Linq;
using Dev2.Common.Interfaces.WindowsTaskScheduler.Wrappers;
using Dev2.TaskScheduler.Wrappers.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32.TaskScheduler;

namespace Dev2.TaskScheduler.Wrappers.Test
{
    [TestClass]
     public class TaskServiceConvertorFactoryTests
    {

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskShedulerWrapper_TaskServiceConvertorFactory_Sanitise")]
        public void TaskServiceConvertorFactory_SanitiseBoot_Test()
        {
            var fact = new TaskServiceConvertorFactory();

            Trigger trig = new BootTrigger();
            var wrapped = new Dev2Trigger(fact, trig);
            var sanitised = fact.SanitiseTrigger(wrapped);
            AssertEqual(trig, sanitised);

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskShedulerWrapper_TaskServiceConvertorFactory_Sanitise")]
        public void TaskServiceConvertorFactory_SanitiseDaily_Test()
        {
            var fact = new TaskServiceConvertorFactory();
            Trigger trig = new DailyTrigger();
            var wrapped = new Dev2Trigger(fact, trig);
            var sanitised = fact.SanitiseTrigger(wrapped);
            AssertEqual(trig, sanitised);
            AssertTriggerValues<IDailyTrigger, DailyTrigger>(sanitised, trig,
                                                             ((a, b) => a.DaysInterval == b.DaysInterval));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskShedulerWrapper_TaskServiceConvertorFactory_Sanitise")]
        public void TaskServiceConvertorFactory_SanitiseEvent_Test()
        {
            var fact = new TaskServiceConvertorFactory();

           
            Trigger trig = new EventTrigger("1","2",1);
            var wrapped = new Dev2Trigger(fact, trig);
            var sanitised = fact.SanitiseTrigger(wrapped);
            AssertEqual(trig, sanitised);
        
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskShedulerWrapper_TaskServiceConvertorFactory_Sanitise")]
        public void TaskServiceConvertorFactory_SanitiseIdle_Test()
        {
            var fact = new TaskServiceConvertorFactory();

            Trigger trig = new IdleTrigger();
            var wrapped = new Dev2Trigger(fact, trig);
            var sanitised = fact.SanitiseTrigger(wrapped);
            AssertEqual(trig, sanitised);

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskShedulerWrapper_TaskServiceConvertorFactory_Sanitise")]
        public void TaskServiceConvertorFactory_SanitiseMonthly_Test()
        {
            var fact = new TaskServiceConvertorFactory();

            Trigger trig = new MonthlyTrigger(2,MonthsOfTheYear.April);
            var wrapped = new Dev2Trigger(fact, trig);
            var sanitised = fact.SanitiseTrigger(wrapped);
            AssertEqual(trig, sanitised);
            AssertTriggerValues<IMonthlyTrigger, MonthlyTrigger>(sanitised, trig,
                                                            ((a, b) => a.DaysOfMonth.FirstOrDefault() == b.DaysOfMonth.FirstOrDefault() && a.MonthsOfYear==b.MonthsOfYear  ));

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskShedulerWrapper_TaskServiceConvertorFactory_Sanitise")]
        public void TaskServiceConvertorFactory_SanitiseMonthlyDOW_Test()
        {
            var fact = new TaskServiceConvertorFactory();

            Trigger trig = new MonthlyDOWTrigger(DaysOfTheWeek.Monday, MonthsOfTheYear.April,WhichWeek.FourthWeek);
            var wrapped = new Dev2Trigger(fact, trig);
            var sanitised = fact.SanitiseTrigger(wrapped);
            AssertEqual(trig, sanitised);
            AssertTriggerValues<IMonthlyDOWTrigger, MonthlyDOWTrigger>(sanitised, trig,
                                                            ((a, b) => a.DaysOfWeek == b.DaysOfWeek && a.MonthsOfYear == b.MonthsOfYear && a.RandomDelay == b.RandomDelay && a.WeeksOfMonth==b.WeeksOfMonth));

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskShedulerWrapper_TaskServiceConvertorFactory_Sanitise")]
        public void TaskServiceConvertorFactory_SanitiseRegistration_Test()
        {
            var fact = new TaskServiceConvertorFactory();

            Trigger trig = new RegistrationTrigger();
            var wrapped = new Dev2Trigger(fact, trig);
            var sanitised = fact.SanitiseTrigger(wrapped);
            AssertEqual(trig, sanitised);
          
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskShedulerWrapper_TaskServiceConvertorFactory_Sanitise")]
        public void TaskServiceConvertorFactory_SessionState_Test()
        {
            var fact = new TaskServiceConvertorFactory();

            Trigger trig = new SessionStateChangeTrigger();
            var wrapped = new Dev2Trigger(fact, trig);
            var sanitised = fact.SanitiseTrigger(wrapped);
            AssertEqual(trig, sanitised);

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskShedulerWrapper_TaskServiceConvertorFactory_Sanitise")]
        public void TaskServiceConvertorFactory_Time_Test()
        {
            var fact = new TaskServiceConvertorFactory();

            Trigger trig = new TimeTrigger(new DateTime(2005,1,1));
            var wrapped = new Dev2Trigger(fact, trig);
            var sanitised = fact.SanitiseTrigger(wrapped);
            AssertEqual(trig, sanitised);
            AssertTriggerValues<ITimeTrigger, TimeTrigger>(sanitised, trig,
                                                       ((a, b) => a.StartBoundary==b.StartBoundary));


        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskShedulerWrapper_TaskServiceConvertorFactory_Sanitise")]
        public void TaskServiceConvertorFactory_Weekly_Test()
        {
            var fact = new TaskServiceConvertorFactory();

            Trigger trig = new WeeklyTrigger(DaysOfTheWeek.Monday);
            var wrapped = new Dev2Trigger(fact, trig);
            var sanitised = fact.SanitiseTrigger(wrapped);
            AssertEqual(trig, sanitised);
            AssertTriggerValues<IWeeklyTrigger, WeeklyTrigger>(sanitised, trig,
                                                       ((a, b) => a.DaysOfWeek == b.DaysOfWeek));


        }
        private static void AssertTriggerValues<T,U>(ITrigger sanitised, Trigger trig, Func<T,U,bool> fun ) where T : class where U : class
        {
            var instance = sanitised as T;
            var native = trig as U;

            Assert.IsTrue(fun(instance,native));

        }


        private static void AssertEqual(Trigger trig, ITrigger sanitised)
        {
            Assert.AreEqual(trig.Repetition.Duration, sanitised.Repetition.Instance.Duration);
            Assert.AreEqual(trig.Repetition.Interval, sanitised.Repetition.Instance.Interval);
            Assert.AreEqual(trig.Repetition.StopAtDurationEnd, sanitised.Repetition.Instance.StopAtDurationEnd);
            Assert.AreEqual(trig.Enabled, sanitised.Enabled);
            Assert.AreEqual(trig.TriggerType, sanitised.TriggerType);
            Assert.AreEqual(trig.Id, sanitised.Id);
            Assert.AreEqual(trig.EndBoundary, sanitised.EndBoundary);
            Assert.AreEqual(trig.ExecutionTimeLimit, sanitised.ExecutionTimeLimit);
        }
    }
}
