using System;
using System.Collections.Generic;
using Dev2.TaskScheduler.Wrappers.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32.TaskScheduler;
using Moq;
namespace Dev2.Scheduler.Test
{
    [TestClass]
    public class ScheduleTriggerTest
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskSheduler_ ScheduleTrigger_Constructor")]
        public void ScheduleTrigger_AssertConstructor()
        {
            var t = new Mock<ITrigger>();
            var service = new Mock<IDev2TaskService>();
            var mockTask = new Mock<IDev2TaskDefinition>();
            var mockTriggers = new Mock<ITriggerCollection>();
            var mockAction = new Mock<IExecAction>();
            var mockFactory = new Mock<ITaskServiceConvertorFactory>();

            mockTask.Setup(a => a.XmlText).Returns("bob").Verifiable();
            mockTask.Setup(a => a.Triggers).Returns(mockTriggers.Object);
            mockFactory.Setup(a => a.CreateExecAction(It.IsAny<IAction>())).Returns(mockAction.Object);
            mockTriggers.Setup(a => a.Add(t.Object)).Verifiable();
            service.Setup(a => a.NewTask()).Returns(mockTask.Object);

            ScheduleTrigger trigger = new ScheduleTrigger(TaskState.Disabled, t.Object, service.Object, mockFactory.Object);
            Assert.AreEqual(trigger.NativeXML, "bob");
            Assert.AreEqual(trigger.State, TaskState.Disabled);



        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskSheduler_ ScheduleTrigger_Constructor")]
        public void ScheduleTrigger_AssertTriggerDeserialise()
        {
            var t = new Mock<ITrigger>();
            var service = new Mock<IDev2TaskService>();
            var mockTask = new Mock<IDev2TaskDefinition>();
            var mockTriggers = new MockTriggerCollection();
            var mockAction = new Mock<IExecAction>();
            var mockFactory = new Mock<ITaskServiceConvertorFactory>();

            mockTask.Setup(a => a.XmlText).Returns("bob").Verifiable();
            mockTask.Setup(a => a.Triggers).Returns(mockTriggers);
            mockFactory.Setup(a => a.CreateExecAction(It.IsAny<IAction>())).Returns(mockAction.Object);
            service.Setup(a => a.NewTask()).Returns(mockTask.Object);


            ScheduleTrigger trigger = new ScheduleTrigger(TaskState.Disabled, t.Object, service.Object, mockFactory.Object);
            var x = trigger.Trigger;
            mockTask.VerifySet(a => a.XmlText = "bob");
            Assert.AreEqual(trigger.NativeXML, "bob");
            Assert.AreEqual(trigger.State, TaskState.Disabled);



        }


    }
    public class MockTriggerCollection : List<ITrigger>, ITriggerCollection
    {
        public void Dispose()
        {

        }

        public TriggerCollection Instance { get; private set; }
        public new ITrigger Add(ITrigger unboundTrigger)
        {
            base.Add(unboundTrigger);
            AddCalled = true;
            return unboundTrigger;
        }

        public bool ContainsType(Type triggerType)
        {
            throw new NotImplementedException();
        }

        public bool AddCalled { get; set; }
    }
}
