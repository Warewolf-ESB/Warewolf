using Caliburn.Micro;
using Dev2.Studio.Core.AppResources.Enums;

namespace Dev2.Studio.ViewModels.WorkSurface
{
    public interface IWorkSurfaceViewModel : IScreen
    {
        WorkSurfaceContext WorkSurfaceContext { get; set; }
        string IconPath { get; set; }
    }
}
