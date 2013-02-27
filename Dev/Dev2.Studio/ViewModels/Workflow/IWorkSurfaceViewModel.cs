using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using Dev2.Studio.AppResources.AttachedProperties;
using Dev2.Studio.Core.AppResources.Enums;

namespace Dev2.Studio.ViewModels.Workflow
{
    public interface IWorkSurfaceViewModel : IScreen
    {
        WorkSurfaceContext WorkSurfaceContext { get; }
    }
}
