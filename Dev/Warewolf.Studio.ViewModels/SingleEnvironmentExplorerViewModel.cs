#pragma warning disable
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Dev2.Common;
using Dev2.Studio.Interfaces;

namespace Warewolf.Studio.ViewModels
{
    public class SingleEnvironmentExplorerViewModel : ExplorerViewModelBase
    {
        readonly Guid _selectedId;

        public SingleEnvironmentExplorerViewModel(IEnvironmentViewModel environmentViewModel, Guid selectedId, bool filterByType)
        {
            _selectedId = selectedId;
            environmentViewModel.SetPropertiesForDialog();

            var versions = environmentViewModel.Children.Flatten(a => a.Children).Where(a => a.AreVersionsVisible);
            if (versions != null)
            {
                foreach (var version in versions)
                {
                    version.AreVersionsVisible = false;
                }
            }

            Environments = new ObservableCollection<IEnvironmentViewModel>
            {
                environmentViewModel
            };

            FilterByType = filterByType;
            IsRefreshing = false;
            ShowConnectControl = false;
            SelectItem(_selectedId);
            Filter();
        }

        public override string SearchText
        {
            get => base.SearchText;
            set
            {
                base.SearchText = value;
                Filter();
            }
        }

        bool FilterByType { get; set; }

        protected override async Task RefreshAsync(bool refresh)
        {
            IsRefreshing = true;
            foreach (var environmentViewModel in Environments)
            {
                if (environmentViewModel.IsConnected)
                {
                    await environmentViewModel.LoadDialogAsync(_selectedId).ConfigureAwait(true);
                }
            }
            Filter();
            IsRefreshing = false;
        }

        void Filter()
        {
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
                {
                    Environments.First().Filter(a => a.ResourceType == "Folder" || a.ResourceType == "WorkflowService");
                }
            }
        }
    }
}