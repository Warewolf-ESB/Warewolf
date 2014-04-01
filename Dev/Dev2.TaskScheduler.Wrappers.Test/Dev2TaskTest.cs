using Dev2.TaskScheduler.Wrappers.Interfaces;
using Microsoft.Win32.TaskScheduler;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
namespace Dev2.TaskScheduler.Wrappers.Test
{

    /// <summary>
    /// Very limited as to what we can test here
    /// </summary>
    [TestClass]
    public class Dev2TaskTest
    {
        private TaskService _taskService;
        private Mock<ITaskServiceConvertorFactory> _factory;


        [TestInitialize]
        public void Init()
        {
            _taskService = new TaskService();

            var newTask = _taskService.NewTask();
            var action = new ExecAction("bob.exe");
            newTask.Actions.Add(action);
            newTask.Triggers.Add(new DailyTrigger());
            _taskService.RootFolder.RegisterTaskDefinition("UnitTestTask",newTask);
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
