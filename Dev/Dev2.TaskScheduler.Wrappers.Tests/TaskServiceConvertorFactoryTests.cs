/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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
using Moq;

// ReSharper disable InconsistentNaming

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
                                                             (a, b) => a.DaysInterval == b.DaysInterval);
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
                                                            (a, b) => a.DaysOfMonth.FirstOrDefault() == b.DaysOfMonth.FirstOrDefault() && a.MonthsOfYear==b.MonthsOfYear);

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
                                                            (a, b) => a.DaysOfWeek == b.DaysOfWeek && a.MonthsOfYear == b.MonthsOfYear && a.RandomDelay == b.RandomDelay && a.WeeksOfMonth==b.WeeksOfMonth);

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
                                                       (a, b) => a.StartBoundary==b.StartBoundary);


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
                                                       (a, b) => a.DaysOfWeek == b.DaysOfWeek);


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

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CreateRootFolder_GivenTaskFolder_ShouldReturnDevtaskFolder()
        {
            //---------------Set up test pack-------------------
            TaskServiceConvertorFactory factory = new TaskServiceConvertorFactory();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(factory);
            //---------------Execute Test ----------------------
            var rootFolder = factory.CreateRootFolder(It.IsAny<TaskFolder>());
            //---------------Test Result -----------------------
            Assert.IsNotNull(rootFolder);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CreateTask_GivenTask_ShouldReturnDev2Task()
        {
            //---------------Set up test pack-------------------
            TaskServiceConvertorFactory factory = new TaskServiceConvertorFactory();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(factory);
            //---------------Execute Test ----------------------
            var task = factory.CreateTask(It.IsAny<Task>());
            //---------------Test Result -----------------------
            Assert.IsNotNull(task);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CreateTaskDefinition_GivenTaskDefinition_ShouldReturnDev2TaskDefinition()
        {
            //---------------Set up test pack-------------------
            TaskServiceConvertorFactory factory = new TaskServiceConvertorFactory();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(factory);
            //---------------Execute Test ----------------------
            var taskDefinition = factory.CreateTaskDefinition(It.IsAny<TaskDefinition>());
            //---------------Test Result -----------------------
            Assert.IsNotNull(taskDefinition);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CreateActionCollection_GivenActionCollection_ShouldReturnDev2ActionCollection()
        {
            //---------------Set up test pack-------------------
            TaskServiceConvertorFactory factory = new TaskServiceConvertorFactory();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(factory);
            //---------------Execute Test ----------------------
            var actionCollection = factory.CreateActionCollection(It.IsAny<ActionCollection>());
            //---------------Test Result -----------------------
            Assert.IsNotNull(actionCollection);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CreateTaskSettings_GivenTaskSettings_ShouldReturnDev2TaskSettings()
        {
            //---------------Set up test pack-------------------
            TaskServiceConvertorFactory factory = new TaskServiceConvertorFactory();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(factory);
            //---------------Execute Test ----------------------
            var taskService = new TaskService().NewTask();            
            var taskSettings = factory.CreateTaskSettings(taskService.Settings);
            //---------------Test Result -----------------------
            Assert.IsNotNull(taskSettings);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CreateTriggerCollection_GivenTriggerCollection_ShouldReturnDev2TriggerCollection()
        {
            //---------------Set up test pack-------------------
            TaskServiceConvertorFactory factory = new TaskServiceConvertorFactory();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(factory);
            //---------------Execute Test ----------------------
            var triggerCollection = factory.CreateTriggerCollection(It.IsAny<TriggerCollection>());
            //---------------Test Result -----------------------
            Assert.IsNotNull(triggerCollection);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CreateTaskCollection_GivenTaskCollection_ShouldReturnDev2TaskCollection()
        {
            //---------------Set up test pack-------------------
            TaskServiceConvertorFactory factory = new TaskServiceConvertorFactory();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(factory);
            //---------------Execute Test ----------------------
            var taskCollection = factory.CreateTaskCollection(It.IsAny<TaskCollection>());
            //---------------Test Result -----------------------
            Assert.IsNotNull(taskCollection);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CreateTrigger_GivenTrigger_ShouldReturnDev2Trigger()
        {
            //---------------Set up test pack-------------------
            TaskServiceConvertorFactory factory = new TaskServiceConvertorFactory();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(factory);
            //---------------Execute Test ----------------------
            var trigger = factory.CreateTrigger(It.IsAny<Trigger>());
            //---------------Test Result -----------------------
            Assert.IsNotNull(trigger);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CreateRepetitionPattern_GivenRepetitionPattern_ShouldReturnDev2RepetitionPattern()
        {
            //---------------Set up test pack-------------------
            TaskServiceConvertorFactory factory = new TaskServiceConvertorFactory();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(factory);
            //---------------Execute Test ----------------------
            var repetitionPattern = factory.CreateRepetitionPattern(It.IsAny<RepetitionPattern>());
            //---------------Test Result -----------------------
            Assert.IsNotNull(repetitionPattern);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CreateAction_GivenRepetitionAction_ShouldReturnDev2Action()
        {
            //---------------Set up test pack-------------------
            TaskServiceConvertorFactory factory = new TaskServiceConvertorFactory();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(factory);
            //---------------Execute Test ----------------------
            var action = factory.CreateAction(It.IsAny<Microsoft.Win32.TaskScheduler.Action>());
            //---------------Test Result -----------------------
            Assert.IsNotNull(action);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CreateTaskService_GivenValidArgs_ShouldReturnCorrectly()
        {
            //---------------Set up test pack-------------------
            TaskServiceConvertorFactory factory = new TaskServiceConvertorFactory();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(factory);
            //---------------Execute Test ----------------------
            const string targetServer = "localHost";
            const string userName = "nathi";
            const string accountDomain = "local";
             string password = "";
            try
            {
                var dev2TaskService = factory.CreateTaskService(targetServer, userName, accountDomain, password, true);
                //---------------Test Result -----------------------
                Assert.IsNotNull(dev2TaskService);
            }
            catch(Exception ex)
            {
                password = "PWord";
                Assert.AreEqual("A username, password, and domain must be provided.", ex.Message);
                try
                {
                    var dev2TaskService = factory.CreateTaskService(targetServer, userName, accountDomain, password, true);
                }
                catch(Exception ex1)
                {
                    Assert.AreEqual("The network path was not found. (Exception from HRESULT: 0x80070035)",ex1.Message);
                }
            }
          
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CreateExecAction_GivenValidArgs_ShouldReturnCorreclty()
        {
            //---------------Set up test pack-------------------
            TaskServiceConvertorFactory factory = new TaskServiceConvertorFactory();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(factory);
            //---------------Execute Test ----------------------
            var execAction = factory.CreateExecAction("path", "args", "Home");
            //---------------Test Result -----------------------
            Assert.IsNotNull(execAction);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CreateExecAction_GivenAction_ShouldReturnCorreclty()
        {
            //---------------Set up test pack-------------------
            TaskServiceConvertorFactory factory = new TaskServiceConvertorFactory();
            var action = new Mock<IAction>();
            action.Setup(action1 => action1.Instance).Returns(Microsoft.Win32.TaskScheduler.Action.CreateAction(TaskActionType.SendEmail));
            //---------------Assert Precondition----------------
            Assert.IsNotNull(factory);
            //---------------Execute Test ----------------------
            var execAction = factory.CreateExecAction(action.Object);
            //---------------Test Result -----------------------
            Assert.IsNotNull(execAction);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CreateTaskEvent_GivenTaskEvent_ShouldDev2TaskEvent()
        {
            //---------------Set up test pack-------------------
            TaskServiceConvertorFactory factory = new TaskServiceConvertorFactory();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(factory);
            //---------------Execute Test ----------------------
            var taskEvent = factory.CreateTaskEvent(It.IsAny<TaskEvent>());
            //---------------Test Result -----------------------
            Assert.IsNotNull(taskEvent);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CreateTaskEventLog_Givenpath_ShouldReturnDev2TaskEventLog()
        {
            //---------------Set up test pack-------------------
            TaskServiceConvertorFactory factory = new TaskServiceConvertorFactory();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(factory);
            //---------------Execute Test ----------------------
            var taskEventLog = factory.CreateTaskEventLog(It.IsAny<string>());
            //---------------Test Result -----------------------
            Assert.IsNotNull(taskEventLog);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CreateTaskService_ShouldReturnCorrectly()
        {
            //---------------Set up test pack-------------------
            TaskServiceConvertorFactory factory = new TaskServiceConvertorFactory();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(factory);
            //---------------Execute Test ----------------------
            var taskEventLog = factory.CreateTaskService();
            //---------------Test Result -----------------------
            Assert.IsNotNull(taskEventLog);
        }
    }
}
