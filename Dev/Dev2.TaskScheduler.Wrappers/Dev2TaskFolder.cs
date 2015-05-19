/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Dev2.Common.Interfaces.WindowsTaskScheduler.Wrappers;
using Microsoft.Win32.TaskScheduler;

namespace Dev2.TaskScheduler.Wrappers
{
    public class Dev2TaskFolder : ITaskFolder
    {
        private readonly TaskFolder _instance;
        private readonly ITaskServiceConvertorFactory _taskServiceConvertorFactory;

        public Dev2TaskFolder(ITaskServiceConvertorFactory taskServiceConvertorFactory, TaskFolder instance)
        {
            _taskServiceConvertorFactory = taskServiceConvertorFactory;
            _instance = instance;
        }

        public void Dispose()
        {
            Instance.Dispose();
        }

        public string Name
        {
            get { return Instance.Name; }
        }

        public string Path
        {
            get { return Instance.Path; }
        }

        public ITaskCollection Tasks
        {
            get { return _taskServiceConvertorFactory.CreateTaskCollection(Instance.Tasks); }
        }


        public ITaskFolder CreateFolder(string subFolderName, string sddlForm = null)
        {
            return _taskServiceConvertorFactory.CreateRootFolder(Instance.CreateFolder(subFolderName, sddlForm));
        }

// ReSharper disable ParameterHidesMember
// ReSharper disable InconsistentNaming
        public void DeleteTask(string Name, bool exceptionOnNotExists = true)
// ReSharper restore InconsistentNaming
// ReSharper restore ParameterHidesMember
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

// ReSharper disable ParameterHidesMember
// ReSharper disable InconsistentNaming
        public IDev2Task RegisterTaskDefinition(string Path, IDev2TaskDefinition definition)
// ReSharper restore InconsistentNaming
// ReSharper restore ParameterHidesMember
        {
            return _taskServiceConvertorFactory.CreateTask(Instance.RegisterTaskDefinition(Path, definition.Instance));
        }

        [ExcludeFromCodeCoverage]
// ReSharper disable ParameterHidesMember
// ReSharper disable InconsistentNaming
        public IDev2Task RegisterTaskDefinition(string Path, IDev2TaskDefinition definition, TaskCreation createType,
// ReSharper restore InconsistentNaming
// ReSharper restore ParameterHidesMember
// ReSharper disable InconsistentNaming
            string UserId,
// ReSharper restore InconsistentNaming
// ReSharper disable InconsistentNaming
            string password = null, TaskLogonType LogonType = TaskLogonType.S4U,
// ReSharper restore InconsistentNaming
            string sddl = null)
        {
            return
                _taskServiceConvertorFactory.CreateTask(Instance.RegisterTaskDefinition(Path, definition.Instance,
                    createType, UserId, password,
                    LogonType, sddl));
        }

        public TaskFolder Instance
        {
            get { return _instance; }
        }
    }
}
