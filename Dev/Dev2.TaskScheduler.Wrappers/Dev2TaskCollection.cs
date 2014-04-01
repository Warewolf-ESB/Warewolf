using System.Collections;
using System.Collections.Generic;
using Dev2.TaskScheduler.Wrappers.Interfaces;
using Microsoft.Win32.TaskScheduler;

namespace Dev2.TaskScheduler.Wrappers
{
    public class Dev2TaskCollection : ITaskCollection
    {
        private readonly TaskCollection _instance;
        private readonly ITaskServiceConvertorFactory _taskServiceConvertorFactory;

        public Dev2TaskCollection(ITaskServiceConvertorFactory taskServiceConvertorFactory, TaskCollection instance)
        {
            _taskServiceConvertorFactory = taskServiceConvertorFactory;
            _instance = instance;
        }

        public void Dispose()
        {
        }


        public TaskCollection Instance
        {
            get { return _instance; }
        }

        public IEnumerator<IDev2Task> GetEnumerator()
        {
            IEnumerator<Task> e = _instance.GetEnumerator();
            while (e.MoveNext())
            {
                yield return _taskServiceConvertorFactory.CreateTask(e.Current);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}