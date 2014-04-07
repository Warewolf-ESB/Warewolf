using Dev2.TaskScheduler.Wrappers.Interfaces;
using Microsoft.Win32.TaskScheduler;

namespace Dev2.Scheduler.Interfaces
{
    public interface IClientSchedulerFactory
    {

        IScheduleTrigger CreateTrigger(TaskState state, ITrigger trigger);
    }
}
