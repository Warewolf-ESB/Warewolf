using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32.TaskScheduler;

namespace Dev2.TaskScheduler.Wrappers.Test
{
    [TestClass]
    public class Dev2TaskServiceTest
    {
        TaskFolder _folder;
        private TaskService _service;
        [TestInitialize]
        public void Init()
        {

            _service = new TaskService();
            _folder = _service.RootFolder.SubFolders.All(a => a.Name != "WarewolfTestFolder") ? _service.RootFolder.CreateFolder("WarewolfTestFolder") : _service.GetFolder("WarewolfTestFolder");
            var task = _service.NewTask();
            task.Actions.Add(new ExecAction("Notepad.exe"));
            _folder.RegisterTaskDefinition("TestTask", task);

        }

        [TestCleanup]
        public void Cleanup()
        {

            _folder = _service.GetFolder("WarewolfTestFolder");
            foreach (var task in _folder.Tasks)
            {
                _folder.DeleteTask(task.Name, false);
            }

            _service.RootFolder.DeleteFolder("WarewolfTestFolder");
            _service.Dispose();
            _folder.Dispose();

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskShedulerWrapper_Dev2TaskServiceTest_Constructor")]
        public void TaskShedulerWrapper_Dev2TaskServiceTest_Constructor()
        {
            var service = new Dev2TaskService(new TaskServiceConvertorFactory());
            Assert.IsNotNull(service.Instance);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskShedulerWrapper_Dev2TaskServiceTest_PassThrough")]
        public void TaskShedulerWrapper_Dev2TaskServiceTest_PassThrough()
        {
            using (var service = new Dev2TaskService(new TaskServiceConvertorFactory()))
            {
                Assert.AreEqual(service.RootFolder.Instance.Name, _service.RootFolder.Name);
            }
            Assert.IsTrue(_service.Connected);
           
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskShedulerWrapper_Dev2TaskServiceTest_GetFolder")]
        public void TaskShedulerWrapper_Dev2TaskServiceTest_GetFolder()
        {
            using (var service = new Dev2TaskService(new TaskServiceConvertorFactory()))
            {
                Assert.AreEqual(service.GetFolder("WarewolfTestFolder").Instance.Name,_folder.Name);
            }
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskShedulerWrapper_Dev2TaskServiceTest_GetTask")]
        public void TaskShedulerWrapper_Dev2TaskServiceTest_GetTask()
        {
            using (var service = new Dev2TaskService(new TaskServiceConvertorFactory()))
            {
                Assert.AreEqual(service.GetTask("\\WarewolfTestFolder\\TestTask").Instance.Name, "TestTask");
            }
        }
    }
}
