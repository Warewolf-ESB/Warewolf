
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32.TaskScheduler;

namespace Dev2.TaskScheduler.Wrappers.Test
{
    [TestClass]
    public class Dev2TaskFolderTests
    {
        TaskFolder _folder;
        private TaskService _service;
        [TestInitialize]
        public void Init()
        {

             _service = new TaskService();
            _folder = _service.RootFolder.SubFolders.Any(a => a.Name == "WarewolfTestFolder") ? _service.GetFolder("WarewolfTestFolder") : _service.RootFolder.CreateFolder("WarewolfTestFolder");
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
        [TestCategory("TaskShedulerWrapper_TaskFolder_Construct")]
        public void TaskShedulerWrapper_TaskFolder_Construct()
        {
            var service = new TaskService();
            TaskFolder folder = service.RootFolder;
            var df = new Dev2TaskFolder(new TaskServiceConvertorFactory(),folder);
            Assert.AreEqual(df.Instance,folder);
            Assert.AreEqual(df.Name,folder.Name);
            Assert.AreEqual(df.Path,folder.Path);
            Assert.AreEqual(df.Tasks.Count(),folder.Tasks.Count);
        
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskShedulerWrapper_TaskFolder_Create")]
        public void TaskShedulerWrapper_TaskFolder_Create()
        {
            var folder = new Dev2TaskFolder(new TaskServiceConvertorFactory(), _folder);
            folder.CreateFolder("sub");
            Assert.IsTrue(_folder.SubFolders.Any(a => a.Name == "sub"));
            _folder.DeleteFolder("sub");

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskShedulerWrapper_TaskFolder_Delete")]
        public void TaskShedulerWrapper_TaskFolder_Delete()
        {
            var folder = new Dev2TaskFolder(new TaskServiceConvertorFactory(), _folder);
            folder.DeleteTask("TestTask");
            Assert.IsFalse(folder.Tasks.Any(a=>a.Name=="TestTask"));

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskShedulerWrapper_TaskFolder_Valid")]
        public void TaskShedulerWrapper_TaskFolder_Valid()
        {
            var folder = new Dev2TaskFolder(new TaskServiceConvertorFactory(), _folder);

            Assert.AreEqual(folder.ValidTasks.Count,1);

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskShedulerWrapper_TaskFolder_Exists")]
        public void TaskShedulerWrapper_TaskFolder_Exists()
        {
            using (var folder = new Dev2TaskFolder(new TaskServiceConvertorFactory(), _folder))
            {

                Assert.IsTrue(folder.TaskExists("TestTask"));
            }

        }

                [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("TaskShedulerWrapper_TaskFolder_Register")]
        public void TaskShedulerWrapper_TaskFolder_Register()
        {
            var folder = new Dev2TaskFolder(new TaskServiceConvertorFactory(), _folder);
            var task = _service.NewTask();
            task.Actions.Add(new ExecAction("b"));
                    folder.RegisterTaskDefinition("newn",
                                                  new Dev2TaskDefinition(new TaskServiceConvertorFactory(), task));
            Assert.AreEqual(folder.ValidTasks.Count,2);

        }




       
    }
}
