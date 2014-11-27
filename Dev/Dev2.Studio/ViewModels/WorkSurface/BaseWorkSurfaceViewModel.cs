
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Caliburn.Micro;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.ViewModels.Base;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.ViewModels.WorkSurface
{
    public class BaseWorkSurfaceViewModel : BaseViewModel,
        IWorkSurfaceViewModel
    {
        private string _iconPath;
        private WorkSurfaceContext _workSurfaceContext = WorkSurfaceContext.Unknown;

        public BaseWorkSurfaceViewModel(IEventAggregator eventPublisher)
            : base(eventPublisher)
        {
        }

        public virtual WorkSurfaceContext WorkSurfaceContext
        {
            get { return _workSurfaceContext; }
            set
            {
                if(_workSurfaceContext == value)
                    return;

                _workSurfaceContext = value;
                NotifyOfPropertyChange(() => WorkSurfaceContext);
            }
        }

        public virtual string IconPath
        {
            get
            {
                return _iconPath;
            }
            set
            {
                if(_iconPath == value) return;

                _iconPath = value;
                NotifyOfPropertyChange(() => IconPath);
            }
        }

        public virtual bool CanSave { get { return true; } }

    }
}
