using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Dev2.TaskScheduler.Wrappers.Interfaces;
using Microsoft.Win32.TaskScheduler;

namespace Dev2.TaskScheduler.Wrappers
{
     // cant really test this. 
    public class Dev2TaskEventLog : ITaskEventLog
    {
        private readonly ITaskServiceConvertorFactory _factory;
        private readonly TaskEventLog _taskLog;

        public Dev2TaskEventLog(ITaskServiceConvertorFactory factory, TaskEventLog taskLog)
        {
            _taskLog = taskLog;
            _factory = factory;
        }

        public long Count
        {
            get { return _taskLog.Count; }
        }


        public IEnumerator<ITaskEvent> GetEnumerator()
        {
            IEnumerator<TaskEvent> en = _taskLog.GetEnumerator();
            while (en.MoveNext())
            {
                yield return _factory.CreateTaskEvent(en.Current);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}