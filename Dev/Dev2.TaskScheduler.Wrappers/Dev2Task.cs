using System;
using Dev2.TaskScheduler.Wrappers.Interfaces;
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