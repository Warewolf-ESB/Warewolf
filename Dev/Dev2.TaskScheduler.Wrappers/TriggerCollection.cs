using System;
using System.Collections;
using System.Collections.Generic;
using Dev2.Common.Interfaces.WindowsTaskScheduler.Wrappers;
using Microsoft.Win32.TaskScheduler;

namespace Dev2.TaskScheduler.Wrappers
{
    public class Dev2TriggerCollection : ITriggerCollection
    {
        private readonly TriggerCollection _nativeInstance;
        private readonly ITaskServiceConvertorFactory _taskServiceConvertorFactory;

        public Dev2TriggerCollection(ITaskServiceConvertorFactory taskServiceConvertorFactory,
                                     TriggerCollection nativeInstance)
        {
            _taskServiceConvertorFactory = taskServiceConvertorFactory;
            _nativeInstance = nativeInstance;
        }

        public IEnumerator<ITrigger> GetEnumerator()
        {
            IEnumerator<Trigger> en = Instance.GetEnumerator();
            while (en.MoveNext())
            {
                yield return _taskServiceConvertorFactory.CreateTrigger(en.Current);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        void IDisposable.Dispose()
        {
            Instance.Dispose();
        }

        public ITrigger Add(ITrigger unboundTrigger)
        {
            Trigger instance = unboundTrigger.Instance;
            Trigger trigger = _nativeInstance.Add(instance);
            return _taskServiceConvertorFactory.CreateTrigger(trigger);
        }


        public TriggerCollection Instance
        {
            get { return _nativeInstance; }
        }
    }
}