/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Caliburn.Micro;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Interfaces.Enums;

namespace Dev2.Studio.ViewModels.WorkSurface
{
    public class BaseWorkSurfaceViewModel : BaseViewModel,
        IWorkSurfaceViewModel
    {
        WorkSurfaceContext _workSurfaceContext = WorkSurfaceContext.Unknown;

        public BaseWorkSurfaceViewModel(IEventAggregator eventPublisher)
            : base(eventPublisher)
        {
        }

        public virtual WorkSurfaceContext WorkSurfaceContext
        {
            get { return _workSurfaceContext; }
            set
            {
                _workSurfaceContext = value;
                NotifyOfPropertyChange(() => WorkSurfaceContext);
            }
        }

        public virtual bool HasVariables => false;

        public virtual bool HasDebugOutput => false;

        public virtual bool CanSave => true;
    }
}
