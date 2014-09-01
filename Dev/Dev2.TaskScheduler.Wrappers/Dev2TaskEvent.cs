using System;
using System.Diagnostics.CodeAnalysis;
using Dev2.Common.Interfaces.WindowsTaskScheduler.Wrappers;
using Microsoft.Win32.TaskScheduler;

namespace Dev2.TaskScheduler.Wrappers
{
    [ExcludeFromCodeCoverage]
    public class Dev2TaskEvent : ITaskEvent
    {
        private readonly TaskEvent _nativeObject;

        public Dev2TaskEvent(TaskEvent nativeObject)
        {
            _nativeObject = nativeObject;
        }

        Guid? ITaskEvent.ActivityId
        {
            get { return _nativeObject.ActivityId; }
        }

        int ITaskEvent.EventId
        {
            get { return _nativeObject.EventId; }
        }

        string ITaskEvent.TaskCategory
        {
            get { return _nativeObject.TaskCategory; }
        }

        DateTime? ITaskEvent.TimeCreated
        {
            get { return _nativeObject.TimeCreated; }
        }

        string ITaskEvent.Correlation
        {
            get { return _nativeObject.Correlation; }
        }

        public string UserId
        {
            get { return _nativeObject.UserId.Value; }
        }
    }
}