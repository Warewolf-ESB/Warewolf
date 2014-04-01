using System;
using Dev2.Common;
using Dev2.Scheduler.Interfaces;
using Dev2.TaskScheduler.Wrappers;
using Dev2.TaskScheduler.Wrappers.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32.TaskScheduler;
using Moq;
namespace Dev2.Scheduler.Test
{
    [TestClass]
    public class ServerSchedulerFactoryTest
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskSheduler_ServerSchedulerFactory_Constructor")]
        public void TaskSheduler_ServerSchedulerFactory_Constructor()
        {
            IDev2TaskService service = new Mock<IDev2TaskService>().Object;
            ITaskServiceConvertorFactory cFactory = new Mock<ITaskServiceConvertorFactory>().Object;
            var factory = new ServerSchedulerFactory(service, cFactory, new DirectoryHelper());
            Assert.AreEqual(cFactory, factory.ConvertorFactory);
            Assert.AreEqual(service, factory.TaskService);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskSheduler_ServerSchedulerFactory_Model")]
        public void TaskSheduler_ServerSchedulerFactory_CreateModel()
        {
            IDev2TaskService service = new Mock<IDev2TaskService>().Object;
            ITaskServiceConvertorFactory cFactory = new Mock<ITaskServiceConvertorFactory>().Object;
            var factory = new ServerSchedulerFactory(service, cFactory, new DirectoryHelper());
            ScheduledResourceModel model = (ScheduledResourceModel)factory.CreateModel("bob");
            Assert.AreEqual("bob", model.WarewolfFolderPath);
            Assert.IsTrue( model.WarewolfAgentPath.Contains(GlobalConstants.SchedulerAgentPath));
            Assert.IsTrue(model.DebugHistoryPath.Contains(GlobalConstants.SchedulerDebugPath));

        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskSheduler_ServerSchedulerFactory_CreateResource")]
        public void TaskSheduler_ServerSchedulerFactory_CreateResource()
        {
            var service = new Mock<IDev2TaskService>();

            var cFactory = new Mock<ITaskServiceConvertorFactory>();
            cFactory.Setup(f => f.CreateExecAction("notepad", null, null))
                .Returns(new Dev2ExecAction(new TaskServiceConvertorFactory(), new ExecAction("notepad.exe", null, null)));
            var mockTask = new Mock<IDev2TaskDefinition>();
            mockTask.Setup(a => a.AddAction(It.IsAny<IAction>()));
            mockTask.Setup(a => a.AddTrigger(It.IsAny<ITrigger>()));
            mockTask.Setup(a => a.XmlText).Returns("bob");
            service.Setup(a => a.NewTask()).Returns(mockTask.Object);

            var factory = new ServerSchedulerFactory(service.Object, cFactory.Object, new DirectoryHelper());
            var trig = new DailyTrigger();
            var res = factory.CreateResource("A", SchedulerStatus.Disabled, trig, "c");
            Assert.AreEqual("A", res.Name);
            Assert.AreEqual(SchedulerStatus.Disabled, res.Status);

            Assert.AreEqual("c", res.WorkflowName);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskSheduler_ServerSchedulerFactory_CreateDailyTrigger")]
        public void TaskSheduler_ServerSchedulerFactory_CreateBootTrigger()
        {
            CheckTriggerTypes(new BootTrigger());
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskSheduler_ServerSchedulerFactory_CreateDailyTrigger")]
        public void TaskSheduler_ServerSchedulerFactory_CreateDailyTrigger()
        {
            CheckTriggerTypes(new DailyTrigger());
        }



        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskSheduler_ServerSchedulerFactory_CreateEventTrigger")]
        public void TaskSheduler_ServerSchedulerFactory_CreateEventTrigger()
        {
            CheckTriggerTypes(new EventTrigger("log", "bob", 111));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskSheduler_ServerSchedulerFactory_CreateIdleTrigger")]
        public void TaskSheduler_ServerSchedulerFactory_CreateIdleTrigger()
        {
            CheckTriggerTypes(new IdleTrigger());
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskSheduler_ServerSchedulerFactory_CreateLogonTrigger")]
        public void TaskSheduler_ServerSchedulerFactory_CreateLogonTrigger()
        {
            CheckTriggerTypes(new LogonTrigger());
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskSheduler_ServerSchedulerFactory_CreateMonthlyTrigger")]
        public void TaskSheduler_ServerSchedulerFactory_CreateMonthlyTrigger()
        {
            CheckTriggerTypes(new MonthlyTrigger(1, MonthsOfTheYear.AllMonths));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskSheduler_ServerSchedulerFactory_CreateMonthlyDOWTrigger")]
        public void TaskSheduler_ServerSchedulerFactory_CreateMonthlyDowTrigger()
        {
            CheckTriggerTypes(new MonthlyDOWTrigger(DaysOfTheWeek.AllDays, MonthsOfTheYear.AllMonths, WhichWeek.AllWeeks));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskSheduler_ServerSchedulerFactory_CreateRegistrationTrigger")]
        public void TaskSheduler_ServerSchedulerFactory_CreateRegistrationTrigger()
        {
            CheckTriggerTypes(new RegistrationTrigger());
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskSheduler_ServerSchedulerFactory_CreateSessionChangeTrigger")]
        public void TaskSheduler_ServerSchedulerFactory_CreateSessionChangeTrigger()
        {
            CheckTriggerTypes(new SessionStateChangeTrigger());
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskSheduler_ServerSchedulerFactory_CreateTimeTrigger")]
        public void TaskSheduler_ServerSchedulerFactory_CreateTimeTrigger()
        {
            CheckTriggerTypes(new TimeTrigger(DateTime.Now));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskSheduler_ServerSchedulerFactory_CreateWeeklyTrigger")]
        public void TaskSheduler_ServerSchedulerFactory_CreateWeeklyTrigger()
        {
            CheckTriggerTypes(new WeeklyTrigger(DaysOfTheWeek.AllDays, 1));
        }


        private static void CheckTriggerTypes(Trigger t)
        {
            IDev2TaskService s = new Dev2TaskService(new TaskServiceConvertorFactory());
            ITaskServiceConvertorFactory fact = new TaskServiceConvertorFactory();
            ServerSchedulerFactory schedulerFactory = new ServerSchedulerFactory(s, fact, new DirectoryHelper());


            var trig = schedulerFactory.CreateTrigger(t);
            Assert.AreEqual(t.TriggerType, trig.Trigger.TriggerType);
            Assert.AreEqual(t.ToString(), trig.Trigger.ToString());
        }
    }
}
