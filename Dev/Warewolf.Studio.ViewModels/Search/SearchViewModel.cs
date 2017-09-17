using System.Collections.ObjectModel;
using System.Windows.Input;
using Dev2.Studio.Interfaces;
using Microsoft.Practices.Prism.PubSubEvents;
using Warewolf.Studio.ViewModels;
using System.Threading.Tasks;
using System.Linq;
using Dev2.Studio.Interfaces.Search;
using Dev2.Common;
using System;

namespace Dev2.ViewModels.Search
{
    public class SearchViewModel : ExplorerViewModel, ISearchViewModel
    {
        private bool _isAllSelected;
        private bool _isWorkflowNameSelected;
        private bool _isToolTitleSelected;
        private bool _isToolNameSelected;
        private bool _isInputFieldSelected;
        private bool _isScalarNameSelected;
        private bool _isObjectNameSelected;
        private bool _isRecSetNameSelected;
        private bool _isInputVariableSelected;
        private bool _isOutputVariableSelected;
        private bool _isTestNameSelected;
        private string _searchInput;
        private ICommand _searchInputCommand;
        private ObservableCollection<ISearchValue> _searchResults;

        public SearchViewModel(IShellViewModel shellViewModel, IEventAggregator aggregator)
            : base(shellViewModel, aggregator, false)
        {
            ConnectControlViewModel = new ConnectControlViewModel(shellViewModel.LocalhostServer, aggregator, shellViewModel.ExplorerViewModel.ConnectControlViewModel.Servers);
            ConnectControlViewModel.ServerConnected += async (sender, server) => { await ServerConnected(sender, server); };
            ConnectControlViewModel.ServerDisconnected += ServerDisconnected;
            SelectedEnvironment = _environments.FirstOrDefault();
            RefreshCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(() => RefreshEnvironment(SelectedEnvironment.ResourceId));
            SearchInputCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(() => SearchWarewolf());
            SearchResults = new ObservableCollection<ISearchValue>();
        }

        private void ServerDisconnected(object sender, IServer server)
        {
            if (SelectedEnvironment != null)
            {
                ServerStateChanged?.Invoke(this, SelectedEnvironment.Server);
            }
        }

        private async Task<IEnvironmentViewModel> ServerConnected(object sender, IServer server)
        {
            var environmentViewModel = await CreateEnvironmentViewModel(sender, server.EnvironmentID, true);
            environmentViewModel?.Server?.GetServerVersion();
            environmentViewModel?.Server?.GetMinSupportedVersion();
            SelectedEnvironment = environmentViewModel;
            if (environmentViewModel != null)
            {
                AfterLoad(environmentViewModel.ResourceId);
            }
            return environmentViewModel;
        }

        public event ServerSate ServerStateChanged;

        private void SearchWarewolf()
        {
            var shellViewModel = CustomContainer.Get<IShellViewModel>();
            SearchResults.Clear();
            if (!string.IsNullOrWhiteSpace(SearchInput))
            {
                if (IsWorkflowNameSelected)
                {
                    var children = SelectedEnvironment.UnfilteredChildren.Flatten(model => model.UnfilteredChildren).Where(model => model.ResourceName.ToLower().Contains(SearchInput.ToLower()));
                    foreach (var child in children)
                    {
                        var search = new SearchValue(child.ResourceId, child.ResourceName, child.ResourcePath, "Workflow", child.ResourceName, SelectedEnvironment);
                        SearchResults.Add(search);
                    }
                }
                if (IsTestNameSelected)
                {
                    var loadTests = SelectedEnvironment.Server?.ResourceRepository.LoadResourceTests(Guid.Empty);
                    if (loadTests != null)
                    {
                        var tests = loadTests.Where(model => model.TestName.ToLower().Contains(SearchInput.ToLower()));
                        foreach (var test in tests)
                        {
                            var resource = SelectedEnvironment.UnfilteredChildren.Flatten(model => model.UnfilteredChildren).FirstOrDefault(model => model.ResourceId == test.ResourceId);
                            var search = new SearchValue(resource.ResourceId, test.TestName, resource.ResourcePath, "Test", test.TestName, SelectedEnvironment);
                            SearchResults.Add(search);
                        }
                    }
                }
            }
        }

        public bool IsAllSelected
        {
            get
            {
                return _isAllSelected;
            }
            set
            {
                _isAllSelected = value;

                IsWorkflowNameSelected = value;
                IsToolTitleSelected = value;
                IsToolNameSelected = value;
                IsInputFieldSelected = value;
                IsScalarNameSelected = value;
                IsObjectNameSelected = value;
                IsRecSetNameSelected = value;
                IsInputVariableSelected = value;
                IsOutputVariableSelected = value;
                IsTestNameSelected = value;

                OnPropertyChanged(() => IsAllSelected);
            }
        }
        public bool IsWorkflowNameSelected
        {
            get
            {
                return _isWorkflowNameSelected;
            }
            set
            {
                _isWorkflowNameSelected = value;
                OnPropertyChanged(() => IsWorkflowNameSelected);
            }
        }
        public bool IsToolTitleSelected
        {
            get
            {
                return _isToolTitleSelected;
            }
            set
            {
                _isToolTitleSelected = value;
                OnPropertyChanged(() => IsToolTitleSelected);
            }
        }
        public bool IsToolNameSelected
        {
            get
            {
                return _isToolNameSelected;
            }
            set
            {
                _isToolNameSelected = value;
                OnPropertyChanged(() => IsToolNameSelected);
            }
        }
        public bool IsInputFieldSelected
        {
            get
            {
                return _isInputFieldSelected;
            }
            set
            {
                _isInputFieldSelected = value;
                OnPropertyChanged(() => IsInputFieldSelected);
            }
        }
        public bool IsScalarNameSelected
        {
            get
            {
                return _isScalarNameSelected;
            }
            set
            {
                _isScalarNameSelected = value;
                OnPropertyChanged(() => IsScalarNameSelected);
            }
        }
        public bool IsObjectNameSelected
        {
            get
            {
                return _isObjectNameSelected;
            }
            set
            {
                _isObjectNameSelected = value;
                OnPropertyChanged(() => IsObjectNameSelected);
            }
        }
        public bool IsRecSetNameSelected
        {
            get
            {
                return _isRecSetNameSelected;
            }
            set
            {
                _isRecSetNameSelected = value;
                OnPropertyChanged(() => IsRecSetNameSelected);
            }
        }
        public bool IsInputVariableSelected
        {
            get
            {
                return _isInputVariableSelected;
            }
            set
            {
                _isInputVariableSelected = value;
                OnPropertyChanged(() => IsInputVariableSelected);
            }
        }
        public bool IsOutputVariableSelected
        {
            get
            {
                return _isOutputVariableSelected;
            }
            set
            {
                _isOutputVariableSelected = value;
                OnPropertyChanged(() => IsOutputVariableSelected);
            }
        }
        public bool IsTestNameSelected
        {
            get
            {
                return _isTestNameSelected;
            }
            set
            {
                _isTestNameSelected = value;
                OnPropertyChanged(() => IsTestNameSelected);
            }
        }
        public string SearchInput
        {
            get
            {
                return _searchInput;
            }
            set
            {
                _searchInput = value;
                OnPropertyChanged(() => SearchInput);
            }
        }
        public ICommand SearchInputCommand
        {
            get
            {
                return _searchInputCommand;
            }
            set
            {
                _searchInputCommand = value;
                OnPropertyChanged(() => SearchInputCommand);
            }
        }
        public ObservableCollection<ISearchValue> SearchResults
        {
            get
            {
                return _searchResults;
            }
            set
            {
                _searchResults = value;
                OnPropertyChanged(() => SearchResults);
            }
        }
    }
}
