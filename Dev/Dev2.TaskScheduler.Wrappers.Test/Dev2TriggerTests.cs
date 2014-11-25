
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
using Dev2.Common.Interfaces.WindowsTaskScheduler.Wrappers;
using Dev2.TaskScheduler.Wrappers.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32.TaskScheduler;
using Moq;

namespace Dev2.TaskScheduler.Wrappers.Test
{

    /// <summary>
    /// Test trigger wrapper
    /// </summary>
    [TestClass]
    public class Dev2TriggerTests
    {


        private  Mock<ITaskServiceConvertorFactory> _taskServiceConvertorFactory;

        [TestInitialize]
        public void Init()
        {
            _taskServiceConvertorFactory = new Mock<ITaskServiceConvertorFactory>();


        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskShedulerWrapper_Dev2DailyTrigger_Construct")]
        public void Dev2DailyTrigger_Construct_Test()
        {

            var dailynative = new DailyTrigger(2);
            Dev2DailyTrigger daily = new Dev2DailyTrigger(_taskServiceConvertorFactory.Object,dailynative){RandomDelay = new TimeSpan(1,2,3)};
            Assert.AreEqual(2,daily.DaysInterval);
            Assert.AreEqual(dailynative.RandomDelay,daily.RandomDelay);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskShedulerWrapper_Dev2DailyTrigger_Construct")]
        public void Dev2EventTrigger_Construct_Test()
        {
            int? id = 0;
            var eventTrigger = new EventTrigger("bob","thomas",id);
            Dev2EventTrigger dev2EventTrigger = new Dev2EventTrigger(_taskServiceConvertorFactory.Object, eventTrigger) ;
            Assert.AreEqual(eventTrigger.Delay, dev2EventTrigger.Delay);
            Assert.AreEqual(eventTrigger.Subscription, dev2EventTrigger.Subscription);
            Assert.AreEqual(eventTrigger.ValueQueries, dev2EventTrigger.ValueQueries);
            Assert.AreEqual(eventTrigger.Delay, dev2EventTrigger.Delay);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskShedulerWrapper_Dev2DailyTrigger_Construct")]
        public void Dev2IdleTrigger_Construct_Test()
        {

            var idleTrigger = new IdleTrigger();
            Dev2IdleTrigger wrappedIdle = new Dev2IdleTrigger(_taskServiceConvertorFactory.Object, idleTrigger);
            Assert.AreEqual(idleTrigger, wrappedIdle.Instance);

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskShedulerWrapper_Dev2DailyTrigger_Construct")]
        public void Dev2RegistrationTrigger_Construct_Test()
        {

            var idleTrigger = new RegistrationTrigger();
            Dev2RegistrationTrigger wrappedIdle = new Dev2RegistrationTrigger(_taskServiceConvertorFactory.Object, idleTrigger);
            Assert.AreEqual(idleTrigger, wrappedIdle.Instance);

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskShedulerWrapper_Dev2DailyTrigger_Construct")]
        public void Dev2MonthlyTrigger_Construct_Test()
        {

            var native   = new MonthlyTrigger(2,MonthsOfTheYear.April);
            var wrapped = new Dev2MonthlyTrigger(_taskServiceConvertorFactory.Object, native);
            Assert.AreEqual(native, wrapped.Instance);
            Assert.AreEqual(native.DaysOfMonth[0], wrapped.DaysOfMonth[0]);
            Assert.AreEqual(native.MonthsOfYear,wrapped.MonthsOfYear);
            Assert.AreEqual(native.RandomDelay,wrapped.RandomDelay);
            Assert.AreEqual(native.RunOnLastDayOfMonth,wrapped.RunOnLastDayOfMonth);

        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskShedulerWrapper_Dev2MonthlyDOWTrigger_Construct")]
        public void Dev2MonthlyDOWTrigger_Construct_Test()
        {

            var native = new MonthlyDOWTrigger(DaysOfTheWeek.Monday,MonthsOfTheYear.August);
            Dev2MonthlyDowTrigger wrapped = new Dev2MonthlyDowTrigger(_taskServiceConvertorFactory.Object, native);
            Assert.AreEqual(native, wrapped.Instance);
            Assert.AreEqual(native.DaysOfWeek, wrapped.DaysOfWeek);
            Assert.AreEqual(native.Enabled, wrapped.Enabled);
            Assert.AreEqual(native.EndBoundary, wrapped.EndBoundary);
            Assert.AreEqual(native.ExecutionTimeLimit, wrapped.ExecutionTimeLimit);
            Assert.AreEqual(native.MonthsOfYear, wrapped.MonthsOfYear);
            Assert.AreEqual(native.RandomDelay, wrapped.RandomDelay);

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskShedulerWrapper_Dev2EventTrigger_Construct")]
        public void TaskShedulerWrapper_Dev2EventTrigger_Construct_Test()
        {

            var native = new EventTrigger("w", "ww",3);
            Dev2EventTrigger wrapped = new Dev2EventTrigger(_taskServiceConvertorFactory.Object, native);
            wrapped.Delay = new TimeSpan(2);
            wrapped.Subscription = "bob";
            
            Assert.AreEqual(wrapped.Delay,native.Delay);
            Assert.AreEqual(wrapped.Enabled,native.Enabled);
            Assert.AreEqual(wrapped.EndBoundary, native.EndBoundary);
            Assert.AreEqual(wrapped.Id,native.Id);
            Assert.AreEqual(wrapped.Instance,native);
            Assert.AreEqual(wrapped.Subscription,native.Subscription);

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskShedulerWrapper_Dev2EventTrigger_Construct")]
        public void TaskShedulerWrapper_Dev2DailyTrigger_Construct_Test()
        {

            var native = new DailyTrigger(3);
            Dev2DailyTrigger wrapped = new Dev2DailyTrigger(_taskServiceConvertorFactory.Object, native);
            wrapped.DaysInterval = 1;
            Assert.AreEqual(wrapped.DaysInterval, native.DaysInterval);
            Assert.AreEqual(wrapped.RandomDelay, native.RandomDelay);
            
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskShedulerWrapper_LogonTrigger_Construct")]
        public void TaskShedulerWrapper_Dev2LogonTrigger_Construct_Test()
        {

            var native = new LogonTrigger();
            var wrapped = new Dev2LogonTrigger(_taskServiceConvertorFactory.Object, native);
            wrapped.Delay =  new TimeSpan(1);
            Assert.AreEqual(wrapped.Delay, native.Delay);
            Assert.AreEqual(wrapped.UserId, native.UserId);
            Assert.AreEqual(wrapped.Instance,native);



        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskShedulerWrapper_WeeklyTrigger_Construct")]
        public void TaskShedulerWrapper_Dev2Weekly_Construct_Test()
        {

            var native = new WeeklyTrigger(DaysOfTheWeek.Saturday);
            var wrapped = new Dev2WeeklyTrigger(_taskServiceConvertorFactory.Object, native);
            wrapped.Delay = new TimeSpan(1);
  
            Assert.AreEqual(wrapped.DaysOfWeek, native.DaysOfWeek);
            Assert.AreEqual(wrapped.Instance, native);



        }


    }
}
