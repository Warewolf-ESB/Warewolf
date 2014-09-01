using System;
using System.Linq;
using Dev2.Common;
using Dev2.Common.Interfaces.WindowsTaskScheduler.Wrappers;
using Dev2.TaskScheduler.Wrappers.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Microsoft.Win32.TaskScheduler;
namespace Dev2.TaskScheduler.Wrappers.Test
{
    [TestClass]
    public class Dev2TaskDefinitionTest
    {

        private Mock<ITaskServiceConvertorFactory> _factory;
        private TaskService _service;
        [TestInitialize]
        public void Init()
        {
            _factory = new Mock<ITaskServiceConvertorFactory>();
            _service = new TaskService();
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskShedulerWrapper_Dev2TaskDefinitionTest_Construct")]
        public void Dev2TaskDefinitionTest_Construction_Test()
        {
            using (var native = _service.NewTask()) 
            {
                var defn = new Dev2TaskDefinition(_factory.Object, native);
                Assert.AreEqual(defn.Instance, native);
                Assert.AreEqual(defn.UserName, native.Principal.UserId);
                Assert.AreEqual(defn.XmlText, native.XmlText);
            }
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskShedulerWrapper_Dev2TaskDefinitionTest_ValidTask")]
        public void Dev2TaskDefinitionTest_IsValid_TestValid()
        {
            var native = _service.NewTask();
            native.Actions.Add(new ExecAction(GlobalConstants.SchedulerAgentPath, "\"a:1\" \"b:2\""));
            var factory = new TaskServiceConvertorFactory();

            var defn = new Dev2TaskDefinition(factory, native);
            Assert.IsTrue(defn.IsValidDev2Task());
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskShedulerWrapper_Dev2TaskDefinitionTest_InValidTaskArgs")]
        public void Dev2TaskDefinitionTest_IsValid_TestInValidArgs()
        {
            var native = _service.NewTask();
            native.Actions.Add(new ExecAction(GlobalConstants.SchedulerAgentPath, "\"a\" \"b:2\""));
            var factory = new TaskServiceConvertorFactory();

            var defn = new Dev2TaskDefinition(factory, native);
            Assert.IsFalse(defn.IsValidDev2Task());
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskShedulerWrapper_Dev2TaskDefinitionTest_InValidTaskPath")]
        public void Dev2TaskDefinitionTest_IsValid_TestInValidPath()
        {
            var native = _service.NewTask();
            native.Actions.Add(new ExecAction("notepad.exe", "\"a:1\" \"b:2\""));
            var factory = new TaskServiceConvertorFactory();

            var defn = new Dev2TaskDefinition(factory, native);
            Assert.IsFalse(defn.IsValidDev2Task());
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskShedulerWrapper_Dev2TaskDefinitionTest_InValidAction")]
        public void Dev2TaskDefinitionTest_IsValid_TestInValidAction()
        {
            var native = _service.NewTask();
            native.Actions.Add(new ShowMessageAction("s","y"));
            var factory = new TaskServiceConvertorFactory();

            var defn = new Dev2TaskDefinition(factory, native);
            Assert.IsFalse(defn.IsValidDev2Task());
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskShedulerWrapper_Dev2TaskDefinitionTest_NoActions")]
        public void Dev2TaskDefinitionTest_IsValid_TestNoAction()
        {
            var native = _service.NewTask();

            var factory = new TaskServiceConvertorFactory();

            var defn = new Dev2TaskDefinition(factory, native);
            Assert.IsFalse(defn.IsValidDev2Task());
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskShedulerWrapper_Dev2TaskDefinitionTest_PassThroughActions")]
        public void Dev2TaskDefinitionTest_Actions()
        {
             AssertPassThrough((a,b)=>a.Actions==b.Actions.Instance);
          
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskShedulerWrapper_Dev2TaskDefinitionTest_PassThroughTriggers")]
        public void Dev2TaskDefinitionTest_Triggers()
        {
             AssertPassThrough((a, b) => a.Triggers == b.Triggers.Instance);

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskShedulerWrapper_Dev2TaskDefinitionTest_PassThroughSettings")]
        public void Dev2TaskDefinitionTest_Settings()
        {
             AssertPassThrough((a, b) => a.Settings == b.Settings.Instance);

        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskShedulerWrapper_Dev2TaskDefinitionTest_PassThroughSettings")]
        public void Dev2TaskDefinitionTest_Data()
        {
            AssertPassThrough((a, b) => ReferenceEquals( a.Data ,b.Data));

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskShedulerWrapper_Dev2TaskDefinitionTest_Action")]
        public void Dev2TaskDefinitionTest_Action()
        {
           AssertPassThrough((a, b) => (a.Actions.First().Id== b.Action.Instance.Id));

        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskShedulerWrapper_Dev2TaskDefinitionTest_Trigger")]
        public void Dev2TaskDefinitionTest_Trigger()
        {
            AssertPassThrough((a, b) => (a.Triggers.First().Id== b.Trigger.Instance.Id));

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskShedulerWrapper_Dev2TaskDefinitionTest_AddTrigger")]
        public void Dev2TaskDefinitionTest_Addrigger()
        {
            var defn = AssertPassThrough((a, b) => (a.Triggers.First().Id == b.Trigger.Instance.Id));
            defn.AddTrigger(new Dev2DailyTrigger(new TaskServiceConvertorFactory(), new DailyTrigger(1)));
            Assert.AreEqual(defn.Triggers.Count(),2);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskShedulerWrapper_Dev2TaskDefinitionTest_AddAction")]
        public void Dev2TaskDefinitionTest_ActionAdd()
        {
            var defn = AssertPassThrough((a, b) => (a.Triggers.First().Id == b.Trigger.Instance.Id));
            defn.AddAction(new Dev2ExecAction(new TaskServiceConvertorFactory(), new ExecAction("a", "b", "c")));
            Assert.AreEqual(defn.Actions.Count(), 2);
        }



        private Dev2TaskDefinition AssertPassThrough(Func<TaskDefinition,IDev2TaskDefinition,bool> func)
        {
            var native = NativeTaskDefinition();
            var defn = Dev2TaskDefinition(native);
            Assert.IsTrue(func(native,defn));
            return defn;
        }

        private static Dev2TaskDefinition Dev2TaskDefinition(TaskDefinition native)
        {
            var factory = new TaskServiceConvertorFactory();

            var defn = new Dev2TaskDefinition(factory, native);
            return defn;
        }

        private TaskDefinition NativeTaskDefinition()
        {
            var native = _service.NewTask();
            native.Actions.Add(new ExecAction(GlobalConstants.SchedulerAgentPath, "\"a:1\" \"b:2\""));
            native.Triggers.Add(new DailyTrigger());
            return native;
        }
    }
}
