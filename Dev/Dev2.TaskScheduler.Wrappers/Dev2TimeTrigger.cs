using System;
using Dev2.Common.Interfaces.WindowsTaskScheduler.Wrappers;
using Dev2.TaskScheduler.Wrappers.Interfaces;
using Microsoft.Win32.TaskScheduler;

namespace Dev2.TaskScheduler.Wrappers
{
    public class Dev2TimeTrigger : Dev2Trigger, ITimeTrigger
    {
        public Dev2TimeTrigger(ITaskServiceConvertorFactory _taskServiceConvertorFactory, TimeTrigger instance)
            : base(_taskServiceConvertorFactory, instance)
        {
        }

        public TimeSpan Delay
        {
            get { return ((ITriggerDelay) Instance).Delay; }
            set { ((ITriggerDelay) Instance).Delay = value; }
        }

        public TimeSpan RandomDelay
        {
            get { return Instance.RandomDelay; }
            set { Instance.RandomDelay = value; }
        }

        public new TimeTrigger Instance
        {
            get { return (TimeTrigger) base.Instance; }
        }
    }
}