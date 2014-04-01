using System;
using Dev2.TaskScheduler.Wrappers.Interfaces;
using Microsoft.Win32.TaskScheduler;

namespace Dev2.TaskScheduler.Wrappers
{
    public class Dev2WeeklyTrigger : Dev2Trigger, IWeeklyTrigger
    {
        public Dev2WeeklyTrigger(ITaskServiceConvertorFactory taskServiceConvertorFactory, Trigger instance)
            : base(taskServiceConvertorFactory, instance)
        {
        }

        public TimeSpan Delay
        {
            get { return ((ITriggerDelay) Instance).Delay; }
            set { ((ITriggerDelay) Instance).Delay = value; }
        }

        public new WeeklyTrigger Instance
        {
            get { return (WeeklyTrigger) base.Instance; }
        }

        public DaysOfTheWeek DaysOfWeek
        {
            get { return Instance.DaysOfWeek; }
   
        }

        public TimeSpan RandomDelay
        {
            get { return Instance.RandomDelay; }
           
        }

        public short WeeksInterval
        {
            get { return Instance.WeeksInterval; }
        
        }
    }
}