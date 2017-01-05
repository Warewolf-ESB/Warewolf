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
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.ViewModels.Base;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.ViewModels.WorkSurface
{
    public class BaseWorkSurfaceViewModel : BaseViewModel,
        IWorkSurfaceViewModel
    {
        private WorkSurfaceContext _workSurfaceContext = WorkSurfaceContext.Unknown;
        private bool _hasVariables;
        private bool _hasDebugOutput;

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

        public virtual bool HasVariables
        {
            get { return _hasVariables; }
            set
            {
                _hasVariables = value; 
                NotifyOfPropertyChange(() => HasVariables);
            }
        }

        public virtual bool HasDebugOutput
        {
            get { return _hasDebugOutput; }
            set
            {
                _hasDebugOutput = value; 
                NotifyOfPropertyChange(() => HasDebugOutput);
            }
        }

        public virtual bool CanSave => true;
    }
}
