using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.ViewModels.Workflow;

namespace Dev2.Studio.ViewModels.WorkSurface
{
    public class BaseWorkSurfaceViewModel : BaseViewModel, 
        IWorkSurfaceViewModel
    {
        private string _iconPath;
        private WorkSurfaceContext _workSurfaceContext = WorkSurfaceContext.Unknown;

        public virtual WorkSurfaceContext WorkSurfaceContext
        {
            get { return _workSurfaceContext; }
            set
            {
                if (_workSurfaceContext == value)
                    return;

                _workSurfaceContext = value;
                NotifyOfPropertyChange(() => WorkSurfaceContext);
            }
        }

        public string IconPath
        {
            get
            {
                return _iconPath;
            }
            set
            {
                if (_iconPath == value) return;

                _iconPath = value;
                NotifyOfPropertyChange(() => IconPath);
            }
        }

    }
}
