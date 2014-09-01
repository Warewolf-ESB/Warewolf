using Dev2.Common.Interfaces.WindowsTaskScheduler.Wrappers;
using Dev2.TaskScheduler.Wrappers.Interfaces;
using Microsoft.Win32.TaskScheduler;

namespace Dev2.TaskScheduler.Wrappers
{
    public class Dev2IdleTrigger : Dev2Trigger, IIdleTrigger
    {
        public Dev2IdleTrigger(ITaskServiceConvertorFactory taskServiceConvertorFactory, IdleTrigger instance)
            : base(taskServiceConvertorFactory, instance)
        {
        }

        public new IdleTrigger Instance
        {
            get { return (IdleTrigger) base.Instance; }
        }
    }
}