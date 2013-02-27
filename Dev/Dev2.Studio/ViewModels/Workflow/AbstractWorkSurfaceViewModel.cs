using Dev2.Studio.AppResources.AttachedProperties;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.ViewModels.Base;

namespace Dev2.Studio.ViewModels.Workflow
{
    public abstract class AbstractWorkSurfaceViewModel : BaseViewModel,
                                                         IWorkSurfaceViewModel
    {
        public virtual WorkSurfaceContext WorkSurfaceContext
        {
            get { return WorkSurfaceContext.Unknown; }
        }
    }
}