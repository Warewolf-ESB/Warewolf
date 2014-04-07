using System;
using Microsoft.Win32.TaskScheduler;

namespace Dev2.Scheduler.Interfaces
{
    public interface IServerSchedulerFactory:IDisposable
    {
        IScheduledResourceModel CreateModel(string schedulerFolderId, ISecurityWrapper securityWrapper);
        IScheduleTrigger CreateTrigger(Trigger trigger);

        IScheduledResource CreateResource(string name, SchedulerStatus status
                                           , Trigger trigger, string workflowName);

    }
}
