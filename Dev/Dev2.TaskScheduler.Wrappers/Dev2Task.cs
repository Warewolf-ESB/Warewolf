
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

        public Task Instance
        {
            get { return _nativeObject; }
        }

        public IDev2TaskDefinition Definition
        {
            get { return _taskServiceConvertorFactory.CreateTaskDefinition(_nativeObject.Definition); }
        }

        public bool Enabled
        {
            get { return _nativeObject.Enabled; }
            set { _nativeObject.Enabled = value; }
        }

        public bool IsActive
        {
            get { return _nativeObject.IsActive; }
        }

        public DateTime LastRunTime
        {
            get { return _nativeObject.LastRunTime; }
        }

        public int LastTaskResult
        {
            get { return _nativeObject.LastTaskResult; }
        }

        public string Name
        {
            get { return _nativeObject.Name; }
        }

        public DateTime NextRunTime
        {
            get { return _nativeObject.NextRunTime; }
        }

        public int NumberOfMissedRuns
        {
            get { return _nativeObject.NumberOfMissedRuns; }
        }

        public string Path
        {
            get { return _nativeObject.Path; }
        }

        public TaskState State
        {
            get { return _nativeObject.State; }
        }

        public void RegisterChanges()
        {
            _nativeObject.RegisterChanges();
        }


        public bool IsValidDev2Task()
        {
            return Definition.IsValidDev2Task();
        }

        public IAction Action
        {
            get { return Definition.Action; }
        }

        public ITrigger Trigger
        {
            get { return Definition.Trigger; }
        }

        public void Dispose()
        {
           _nativeObject.Dispose();
        }
    }
}
