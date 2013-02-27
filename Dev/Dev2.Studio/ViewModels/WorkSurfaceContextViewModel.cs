using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Dev2.Studio.AppResources.AttachedProperties;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Core.ViewModels.DataList;
using Dev2.Studio.ViewModels.Diagnostics;
using Dev2.Studio.ViewModels.Workflow;

namespace Dev2.Studio.ViewModels
{
    /// <summary>
    /// Class used as unified context across the studio - coordination across different regions
    /// </summary>
    /// <author>Jurie.smit</author>
    /// <date>2/27/2013</date>
    public class WorkSurfaceContextViewModel : BaseViewModel
    {
        #region private fields

        private ObservableCollection<DebugTreeViewItemViewModel> _debugItems;
        private WorkflowDesignerViewModel _workflowDesignerViewModel;
        private DataListViewModel _dataListViewModel;

        #endregion private fields

        #region public properties

        public Tuple<Guid, Guid> ID { get; set; }

        /// <summary>
        /// Returns a observable collection containing the root level items
        /// in the debug tree, to which the DEbugOutput can bind.
        /// </summary>
        public ObservableCollection<DebugTreeViewItemViewModel> DebugItems
        {
            get
            {
                return _debugItems ?? (_debugItems = new ObservableCollection<DebugTreeViewItemViewModel>());
            }
        }

        public DataListViewModel DataListViewModel
        {
            get
            {
                return _dataListViewModel;
            }
            set
            {
                if (_dataListViewModel == value) return;

                _dataListViewModel = value;
                NotifyOfPropertyChange(() => DataListViewModel);
            }
        }

        private IWorkSurfaceViewModel _workSurfaceViewModel;

        public IWorkSurfaceViewModel WorkSurfaceViewModel
        {
            get
            {
                return _workSurfaceViewModel;
            }
            set
            {
                if (_workSurfaceViewModel == value) return;

                _workSurfaceViewModel = value;
                NotifyOfPropertyChange(() => WorkSurfaceViewModel);
            }
        }

        public WorkflowDesignerViewModel WorkflowDesignerViewModel
        {
            get
            {
                return _workflowDesignerViewModel;
            }
            set
            {
                if (_workflowDesignerViewModel == value) return;

                _workflowDesignerViewModel = value;
                NotifyOfPropertyChange(() => WorkflowDesignerViewModel);
            }
        }

        #endregion public properties
    }
}
