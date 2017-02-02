/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Common.Interfaces.WindowsTaskScheduler.Wrappers;
using Microsoft.Win32.TaskScheduler;

namespace Dev2.TaskScheduler.Wrappers
{
    public class Dev2Task : IDev2Task
    {
        private readonly Task _nativeObject;
        private readonly ITaskServiceConvertorFactory _taskServiceConvertorFactory;

        public Dev2Task(ITaskServiceConvertorFactory taskServiceConvertorFactory, Task nativeObject)
        {
            _nativeObject = nativeObject;
            _taskServiceConvertorFactory = taskServiceConvertorFactory;
        }

        public Task Instance => _nativeObject;

        public IDev2TaskDefinition Definition => _taskServiceConvertorFactory.CreateTaskDefinition(_nativeObject.Definition);

        public bool Enabled
        {
            get { return _nativeObject.Enabled; }
            set { _nativeObject.Enabled = value; }
        }

        public bool IsActive => _nativeObject.IsActive;

        public DateTime LastRunTime => _nativeObject.LastRunTime;

        public int LastTaskResult => _nativeObject.LastTaskResult;

        public string Name => _nativeObject.Name;

        public DateTime NextRunTime => _nativeObject.NextRunTime;

        public int NumberOfMissedRuns => _nativeObject.NumberOfMissedRuns;

        public string Path => _nativeObject.Path;

        public TaskState State => _nativeObject.State;


        public bool IsValidDev2Task()
        {
            return Definition.IsValidDev2Task();
        }

        public IAction Action => Definition.Action;

        public ITrigger Trigger => Definition.Trigger;

        public void Dispose()
        {
            _nativeObject.Dispose();
        }
    }
}
