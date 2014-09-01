using System;
using Dev2.Common.Interfaces.WindowsTaskScheduler.Wrappers;
using Microsoft.Win32.TaskScheduler;

namespace Dev2.TaskScheduler.Wrappers
{
    public class Dev2RegistrationTrigger : Dev2Trigger, ITriggerDelay, IWrappedObject<RegistrationTrigger>
    {
        public Dev2RegistrationTrigger(ITaskServiceConvertorFactory taskServiceConvertorFactory,
                                       RegistrationTrigger instance)
            : base(taskServiceConvertorFactory, instance)
        {
        }

        public TimeSpan Delay
        {
            get { return Instance.Delay; }
            set { Instance.Delay = value; }
        }

        public new RegistrationTrigger Instance
        {
            get { return (RegistrationTrigger) base.Instance; }
        }
    }
}