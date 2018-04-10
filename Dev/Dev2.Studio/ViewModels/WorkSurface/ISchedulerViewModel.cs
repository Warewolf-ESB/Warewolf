using Dev2.Common.Interfaces.Scheduler.Interfaces;
using Dev2.Studio.ViewModels.WorkSurface;

namespace Dev2.ViewModels.WorkSurface
{
    public interface ISchedulerViewModel : IWorkSurfaceContextViewModel
    {
        IScheduledResource SelectedTask { get; set; }
    }
}
