using System;
using Dev2.TaskScheduler.Wrappers.Interfaces;
using Microsoft.Win32.TaskScheduler;

namespace Dev2.TaskScheduler.Wrappers
{
    public class Dev2LogonTrigger : Dev2Trigger, ILogonTrigger
    {
        public Dev2LogonTrigger(ITaskServiceConvertorFactory taskServiceConvertorFactory, LogonTrigger instance)
            : base(taskServiceConvertorFactory, instance)
        {
        }

        public TimeSpan Delay
        {
            get { return (Instance).Delay; }
            set { (Instance).Delay = value; }
        }

        public string UserId
        {
            get { return Instance.UserId; }
            set { Instance.UserId = value; }
        }

        public new LogonTrigger Instance
        {
            get { return (LogonTrigger) base.Instance; }
        }
    }
}