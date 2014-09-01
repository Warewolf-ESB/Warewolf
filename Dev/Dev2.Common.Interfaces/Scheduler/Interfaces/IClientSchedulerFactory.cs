using Dev2.Common.Interfaces.WindowsTaskScheduler.Wrappers;
using Microsoft.Win32.TaskScheduler;

namespace Dev2.Common.Interfaces.Scheduler.Interfaces
{
    public interface IClientSchedulerFactory
    {

        IScheduleTrigger CreateTrigger(TaskState state, ITrigger trigger);
    }
}
