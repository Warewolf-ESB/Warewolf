using Microsoft.Win32.TaskScheduler;

namespace Dev2.Scheduler.Interfaces
{
    public interface IServerSchedulerFactory
    {
        IScheduledResourceModel CreateModel(string schedulerFolderId);
        IScheduleTrigger CreateTrigger(Trigger trigger);

        IScheduledResource CreateResource(string name, SchedulerStatus status
                                           , Trigger trigger, string workflowName);

    }
}
