/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.WindowsTaskScheduler.Wrappers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32.TaskScheduler;
using Moq;
using System.Runtime.InteropServices;

namespace Dev2.TaskScheduler.Wrappers.Test
{
    [TestClass]
    public class Dev2TaskTest
    {
        TaskService _taskService;
        Mock<ITaskServiceConvertorFactory> _factory;
        
        [TestInitialize]
        public void Init()
        {
            _taskService = new TaskService();

            var newTask = _taskService.NewTask();
            var action = new ExecAction("bob.exe");
            newTask.Actions.Add(action);
            newTask.Triggers.Add(new DailyTrigger());
            try
            {
                _taskService.RootFolder.RegisterTaskDefinition("UnitTestTask", newTask, TaskCreation.Create, "LocalSchedulerAdmin", "987Sched#@!", TaskLogonType.None);
            }
            catch (COMException e)
            {
                if (e.Message != "Cannot create a file when that file already exists. (Exception from HRESULT: 0x800700B7)")
                {
                    throw e;
                }
            }
            _factory = new Mock<ITaskServiceConvertorFactory>();
        }
        
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskShedulerWrapper_Dev2Task_Construct")]
        public void Dev2Task_ConstructTest()
        {
            var task = _taskService.GetTask("UnitTestTask");
            var wrapper = new Dev2Task(_factory.Object,task);
            wrapper.Enabled = false;
            Assert.AreEqual(wrapper.Enabled,task.Enabled);
            Assert.AreEqual(task,wrapper.Instance);
        }
        
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskShedulerWrapper_Dev2Task_Properties")]
        public void Dev2Task_Properties()
        {
            var task = _taskService.GetTask("UnitTestTask");
            var wrapper = new Dev2Task(_factory.Object, task);
            new  Mock<IDev2TaskDefinition>();
        
            Assert.AreEqual(task, wrapper.Instance);
            Assert.AreEqual(task.IsActive,wrapper.IsActive);
            Assert.AreEqual(task.Name,wrapper.Name);
            Assert.AreEqual(task.NextRunTime, wrapper.NextRunTime);
            Assert.AreEqual(task.NumberOfMissedRuns,wrapper.NumberOfMissedRuns);
            Assert.AreEqual(task.Path, wrapper.Path);
            Assert.AreEqual(task.State, wrapper.State);
        }

        //pass through
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskShedulerWrapper_Dev2Task_Properties")]
        public void Dev2Task_IsValidDev2Task()
        {
            var task = _taskService.GetTask("UnitTestTask");
            var wrapper = new Dev2Task(_factory.Object, task);
            var t = new Mock<IDev2TaskDefinition>();

            _factory.Setup(a => a.CreateTaskDefinition(It.IsAny<TaskDefinition>())).Returns(t.Object);
            t.Setup(a => a.IsValidDev2Task()).Returns(true);

            Assert.IsTrue(wrapper.IsValidDev2Task());
            t.Setup(a => a.IsValidDev2Task()).Returns(false);
            Assert.IsFalse(wrapper.IsValidDev2Task());
        }
    }
}
