using Dev2.Common.Interfaces.Scheduler.Interfaces;
using Dev2.Studio.ViewModels.WorkSurface;
using System;

namespace Dev2.ViewModels.WorkSurface
{
    public interface ISchedulerViewModel : IWorkSurfaceContextViewModel
    {
        IScheduledResource SelectedTask { get; set; }
        bool IsDirty { get; set; }
        void CreateNewTask();
        void UpdateScheduleWithResourceDetails(string resourcePath, Guid id, string resourceName);
    }
}
