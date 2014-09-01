using System;
using Dev2.Common.Interfaces.WindowsTaskScheduler.Wrappers;
using Dev2.TaskScheduler.Wrappers.Interfaces;
using Microsoft.Win32.TaskScheduler;

namespace Dev2.TaskScheduler.Wrappers
{
    public class Dev2DailyTrigger : Dev2Trigger, IDailyTrigger
    {
        public Dev2DailyTrigger(ITaskServiceConvertorFactory taskServiceConvertorFactory, Trigger instance)
            : base(taskServiceConvertorFactory, instance)
        {
        }

        public new DailyTrigger Instance
        {
            get { return (DailyTrigger) base.Instance; }
        }

        public short DaysInterval
        {
            get { return Instance.DaysInterval; }
            set { Instance.DaysInterval = value; }
        }

        public TimeSpan RandomDelay
        {
            get { return Instance.RandomDelay; }
            set { Instance.RandomDelay = value; }
        }
    }
}