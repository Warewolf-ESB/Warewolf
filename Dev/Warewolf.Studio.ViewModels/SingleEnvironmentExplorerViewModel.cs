using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Dev2.Common.Interfaces;

namespace Warewolf.Studio.ViewModels
{
    public class SingleEnvironmentExplorerViewModel : ExplorerViewModelBase
    {
        readonly Guid _selectedId;

        public SingleEnvironmentExplorerViewModel(IEnvironmentViewModel environmentViewModel, Guid selectedId, bool filterByType)
        {
            _selectedId = selectedId;
            environmentViewModel.SetPropertiesForDialog();
            // ReSharper disable once VirtualMemberCallInContructor
            Environments = new ObservableCollection<IEnvironmentViewModel>
            {
                environmentViewModel
            };

            FilterByType = filterByType;
            IsRefreshing = false;
            ShowConnectControl = false;
            SelectItem(_selectedId);
        }

        public override string SearchText
        {
            get
            {
                return _searchText;
            }
            set
            {
                if (_searchText == value)
                {
                    return;
                }
                _searchText = value;
                Environments.First().Filter(a => a.ResourceName.ToUpper().Contains(SearchText.ToUpper()) && (a.ResourceType == "Folder" || a.ResourceType == "WorkflowService"));

                OnPropertyChanged(() => SearchText);
            }
        }
        private bool FilterByType { get; set; }

        protected override async Task Refresh(bool refresh)
        {
            IsRefreshing = true;
            foreach (var environmentViewModel in Environments)
            {
                if (environmentViewModel.IsConnected)
                {
                    await environmentViewModel.LoadDialog(_selectedId);
                }
            }
            if (!string.IsNullOrEmpty(SearchText))
            {
                if (FilterByType)
                {
                    Environments.First().Filter(a => a.ResourceName.ToUpper().Contains(SearchText.ToUpper()) && (a.ResourceType == "Folder" || a.ResourceType == "WorkflowService"));
                }
                else
                {
                    Environments.First().Filter(a => a.ResourceName.ToUpper().Contains(SearchText.ToUpper()));
                }
            }
            else
            {
                if (FilterByType)
                    Environments.First().Filter(a => a.ResourceType == "Folder" || a.ResourceType == "WorkflowService");
            }
            IsRefreshing = false;
        }
    }
}