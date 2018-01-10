/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using System.Linq;
using Dev2.Common.Interfaces.WindowsTaskScheduler.Wrappers;
using Microsoft.Win32.TaskScheduler;
using System;

namespace Dev2.TaskScheduler.Wrappers
{
    public class Dev2TaskFolder : ITaskFolder
    {
        readonly TaskFolder _instance;
        readonly ITaskServiceConvertorFactory _taskServiceConvertorFactory;

        public Dev2TaskFolder(ITaskServiceConvertorFactory taskServiceConvertorFactory, TaskFolder instance)
        {
            _taskServiceConvertorFactory = taskServiceConvertorFactory;
            _instance = instance;
        }

        public void Dispose()
        {
            Instance.Dispose();
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Cleanup
        }

        public string Name => Instance.Name;

        public string Path => Instance.Path;

        public ITaskCollection Tasks => _taskServiceConvertorFactory.CreateTaskCollection(Instance.Tasks);

        public ITaskFolder CreateFolder(string subFolderName) => CreateFolder(subFolderName, null);

        public ITaskFolder CreateFolder(string subFolderName, string sddlForm)
        {
            return _taskServiceConvertorFactory.CreateRootFolder(Instance.CreateFolder(subFolderName, sddlForm));
        }

        public void DeleteTask(string Name) => DeleteTask(Name, true);

        public void DeleteTask(string Name, bool exceptionOnNotExists)


        {
            Instance.DeleteTask(Name, exceptionOnNotExists);
        }

        public IList<IDev2Task> ValidTasks
        {
            get { return Tasks.Where(a => a.Definition.Actions.Count > 0).ToList(); }
        }

        public bool TaskExists(string name)
        {
            return _instance.Tasks.Any(a => a.Name == name);
        }

        public IDev2Task RegisterTaskDefinition(string Path, IDev2TaskDefinition definition)
        {
            return _taskServiceConvertorFactory.CreateTask(Instance.RegisterTaskDefinition(Path, definition.Instance));
        }

        public IDev2Task RegisterTaskDefinition(string Path, IDev2TaskDefinition definition, TaskCreation createType,
            string UserId) => RegisterTaskDefinition(Path, definition, createType, UserId, null, TaskLogonType.S4U, null);

        public IDev2Task RegisterTaskDefinition(string Path, IDev2TaskDefinition definition, TaskCreation createType,
            string UserId,
            string password, TaskLogonType LogonType) => RegisterTaskDefinition(Path, definition, createType, UserId, password, LogonType, null);

        public IDev2Task RegisterTaskDefinition(string Path, IDev2TaskDefinition definition, TaskCreation createType,
            string UserId,
            string password, TaskLogonType LogonType,
            string sddl = null)
        {
            return
                _taskServiceConvertorFactory.CreateTask(Instance.RegisterTaskDefinition(Path, definition.Instance,
                    createType, UserId, password,
                    LogonType, sddl));
        }

        public TaskFolder Instance => _instance;
    }
}
