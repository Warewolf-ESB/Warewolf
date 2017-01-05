/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.WindowsTaskScheduler.Wrappers;
using Microsoft.Win32.TaskScheduler;

namespace Dev2.TaskScheduler.Wrappers
{
    public class Dev2TaskService : IDev2TaskService
    {
        private readonly TaskService _nativeService;
        private readonly ITaskServiceConvertorFactory _taskServiceConvertorFactory;

        public Dev2TaskService(ITaskServiceConvertorFactory taskServiceConvertorFactory, TaskService service)
        {
            _nativeService = service;
            _taskServiceConvertorFactory = taskServiceConvertorFactory;
        }

        public Dev2TaskService(ITaskServiceConvertorFactory taskServiceConvertorFactory, string targetServer,
            string userName = null, string accountDomain = null, string password = null,
            bool forceV1 = false)
        {
            _taskServiceConvertorFactory = taskServiceConvertorFactory;
            _taskServiceConvertorFactory.CreateTaskService(targetServer, userName, accountDomain, password, forceV1);
        }

        public Dev2TaskService(ITaskServiceConvertorFactory taskServiceConvertorFactory)
        {
            _taskServiceConvertorFactory = taskServiceConvertorFactory;
            _nativeService = taskServiceConvertorFactory.CreateTaskService();
        }

        public void Dispose()
        {
            _nativeService.Dispose();
        }

        public bool Connected => _nativeService.Connected;

        public ITaskFolder RootFolder => _taskServiceConvertorFactory.CreateRootFolder(_nativeService.RootFolder);

        public ITaskFolder GetFolder(string folderName)
        {
            var taskFolder = _nativeService.GetFolder(folderName);
            if(taskFolder == null)
            {
                var newFolder = _nativeService.RootFolder.CreateFolder(folderName);
                return _taskServiceConvertorFactory.CreateRootFolder(newFolder);
            }
            return _taskServiceConvertorFactory.CreateRootFolder(taskFolder);
        }

        public IDev2Task GetTask(string taskPath)
        {
            return _taskServiceConvertorFactory.CreateTask(_nativeService.GetTask(taskPath));
        }

        public IDev2TaskDefinition NewTask()
        {
            return _taskServiceConvertorFactory.CreateTaskDefinition(_nativeService.NewTask());
        }

        public TaskService Instance => _nativeService;
    }
}
