
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
using System.Collections.Generic;
using Dev2.Common.Interfaces.WindowsTaskScheduler.Wrappers;
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
            mockTask.VerifyGet(a => a.XmlText);
            Assert.AreEqual(trigger.NativeXML, "bob");
            Assert.AreEqual(trigger.State, TaskState.Disabled);
        }


    }
    public class MockTriggerCollection : List<ITrigger>, ITriggerCollection
    {
        public void Dispose()
        {

        }

        // ReSharper disable UnusedAutoPropertyAccessor.Local
        public TriggerCollection Instance { get; private set; }
        // ReSharper restore UnusedAutoPropertyAccessor.Local
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
