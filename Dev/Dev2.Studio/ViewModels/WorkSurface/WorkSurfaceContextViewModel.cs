using System.Collections.ObjectModel;
using Caliburn.Micro;
using Dev2.Studio.AppResources.Comparers;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.ViewModels.Diagnostics;
using Dev2.Studio.ViewModels.Workflow;

namespace Dev2.Studio.ViewModels.WorkSurface
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
        private IDataListViewModel _dataListViewModel;
        private IWorkSurfaceViewModel _workSurfaceViewModel;

        #endregion private fields

        #region public properties

        public WorkSurfaceKey WorkSurfaceKey { get; private set; }

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

        public bool DeleteRequested { get; set; }

        public IDataListViewModel DataListViewModel
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
                if (DataListViewModel != null)
                    DataListViewModel.ConductWith(this);
            }
        }

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
                if (WorkSurfaceViewModel != null)
                    WorkSurfaceViewModel.ConductWith(this);
            }
        }

        #endregion public properties

        public WorkSurfaceContextViewModel(WorkSurfaceKey workSurfaceKey, IWorkSurfaceViewModel workSurfaceViewModel)
        {
            WorkSurfaceKey = workSurfaceKey;
            WorkSurfaceViewModel = workSurfaceViewModel;
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            DataListSingleton.SetDataList(_dataListViewModel);
        }
    }
}
