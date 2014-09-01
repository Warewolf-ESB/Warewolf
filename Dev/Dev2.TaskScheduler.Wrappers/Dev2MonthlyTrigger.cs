using System;
using Dev2.Common.Interfaces.WindowsTaskScheduler.Wrappers;
using Dev2.TaskScheduler.Wrappers.Interfaces;
using Microsoft.Win32.TaskScheduler;

namespace Dev2.TaskScheduler.Wrappers
{
    public class Dev2MonthlyTrigger : Dev2Trigger, IMonthlyTrigger
    {
        public Dev2MonthlyTrigger(ITaskServiceConvertorFactory taskServiceConvertorFactory, MonthlyTrigger instance)
            : base(taskServiceConvertorFactory, instance)
        {
        }

        public TimeSpan Delay
        {
            get { return ((ITriggerDelay) Instance).Delay; }
            set { ((ITriggerDelay) Instance).Delay = value; }
        }

        public new MonthlyTrigger Instance
        {
            get { return (MonthlyTrigger) base.Instance; }
        }

        public int[] DaysOfMonth
        {
            get { return Instance.DaysOfMonth; }
          
        }

        public MonthsOfTheYear MonthsOfYear
        {
            get { return Instance.MonthsOfYear; }
            
        }

        public TimeSpan RandomDelay
        {
            get { return Instance.RandomDelay; }
           
        }

        public bool RunOnLastDayOfMonth
        {
            get { return Instance.RunOnLastDayOfMonth; }
           
        }
    }
}