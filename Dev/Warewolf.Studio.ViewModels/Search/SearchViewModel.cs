/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

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
using Dev2.Studio.Factory;
using Dev2.Studio.Interfaces.DataList;
using Dev2.Runtime.Configuration.ViewModels.Base;
using System.Windows;

namespace Dev2.ViewModels.Search
{
    public class SearchViewModel : ExplorerViewModel, ISearchViewModel
    {
        bool _isAllSelected;
        bool _isWorkflowNameSelected;
        bool _isToolTitleSelected;
        bool _isToolNameSelected;
        bool _isInputFieldSelected;
        bool _isScalarNameSelected;
        bool _isObjectNameSelected;
        bool _isRecSetNameSelected;
        bool _isInputVariableSelected;
        bool _isOutputVariableSelected;
        bool _isTestNameSelected;
        bool _isMatchCaseSelected;
        bool _isMatchWholeWordSelected;
        string _searchInput;
        ICommand _searchInputCommand;
        ObservableCollection<ISearchValue> _searchResults;
        bool _isSearching;

        public SearchViewModel(IShellViewModel shellViewModel, IEventAggregator aggregator)
            : base(shellViewModel, aggregator, false)
        {
            ConnectControlViewModel = new ConnectControlViewModel(shellViewModel.LocalhostServer, aggregator, shellViewModel.ExplorerViewModel.ConnectControlViewModel.Servers);
            ConnectControlViewModel.ServerConnected += async (sender, server) => { await ServerConnectedAsync(sender, server).ConfigureAwait(false); };
            ConnectControlViewModel.ServerDisconnected += ServerDisconnected;
            SelectedEnvironment = _environments.FirstOrDefault();
            RefreshCommand = new DelegateCommand((o) => RefreshEnvironment(SelectedEnvironment.ResourceId));
            SearchInputCommand = new DelegateCommand(async (o) =>
            {
                SearchWarewolfAsync();
            });
            SearchResults = new ObservableCollection<ISearchValue>();
            IsAllSelected = true;
            SelectedEnvironment?.Server?.ResourceRepository?.Load();
            IsSearching = false;
        }

        void ServerDisconnected(object sender, IServer server)
        {
            if (SelectedEnvironment != null)
            {
                ServerStateChanged?.Invoke(this, SelectedEnvironment.Server);
            }
        }

        async Task<IEnvironmentViewModel> ServerConnectedAsync(object sender, IServer server)
        {
            var environmentViewModel = await CreateEnvironmentViewModel(sender, server.EnvironmentID).ConfigureAwait(false);
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

        public bool IsSearching
        {
            get => _isSearching;
            set
            {
                _isSearching = value;
                OnPropertyChanged(() => IsSearching);
            }
        }

        void SearchWarewolfAsync()
        {
            IsSearching = true;

            var shellViewModel = CustomContainer.Get<IShellViewModel>();
            SearchResults.Clear();
            if (!string.IsNullOrWhiteSpace(SearchInput))
            {
                if (IsWorkflowNameSelected)
                {
                    FilterWorkflows();
                }
                if (IsTestNameSelected)
                {
                    FilterTestNames();
                }
                if (IsAnyVariableOptionSelected)
                {
                    FilterVariables();
                }
            }
            IsSearching = false;
        }

        private bool IsAnyVariableOptionSelected => IsScalarNameSelected || IsRecSetNameSelected || IsObjectNameSelected || IsInputFieldSelected || IsOutputVariableSelected;

        private void FilterWorkflows()
        {
            var children = SelectedEnvironment.Children.Flatten(model => model.Children).Where(model => FilterText(model.ResourceName));
            foreach (var child in children)
            {
                var search = new SearchValue(child.ResourceId, child.ResourceName, child.ResourcePath, "Workflow", child.ResourceName, SelectedEnvironment);
                SearchResults.Add(search);
            }
        }

        private void FilterTestNames()
        {
            var loadTests = SelectedEnvironment.Server?.ResourceRepository.LoadResourceTests(Guid.Empty);
            if (loadTests != null)
            {
                var tests = loadTests.Where(model => FilterText(model.TestName));
                foreach (var test in tests)
                {
                    var resource = SelectedEnvironment.Children.Flatten(model => model.Children).FirstOrDefault(model => model.ResourceId == test.ResourceId);
                    var search = new SearchValue(resource.ResourceId, test.TestName, resource.ResourcePath, "Test", test.TestName, SelectedEnvironment);
                    SearchResults.Add(search);
                }
            }
        }

        private void FilterVariables()
        {
            var resources = SelectedEnvironment.Server?.ResourceRepository.All();
            if (resources != null)
            {
                foreach (var resource in resources)
                {
                    var dtlist = DataListViewModelFactory.CreateDataListViewModel(resource);
                    FilterScalars(resource, dtlist);
                    FilterRecordSets(resource, dtlist);
                    FilterComplexObjects(resource, dtlist);
                }
            }
        }

        private void FilterScalars(IResourceModel resource, IDataListViewModel dtlist)
        {
            var resourcePath = resource.GetSavePath() + resource.ResourceName;
            foreach (var scalar in dtlist.ScalarCollection)
            {
                var search = new SearchValue(resource.ID, resource.ResourceName, resourcePath, SelectedEnvironment)
                {
                    Match = scalar.Name
                };
                if (IsScalarNameSelected && FilterText(scalar.Name))
                {
                    search.Type = "Scalar";
                    SearchResults.Add(search);
                }
                if (scalar.Input && IsInputFieldSelected)
                {
                    search.Type = "Scalar Input";
                    SearchResults.Add(search);
                }
                if (scalar.Output && IsOutputVariableSelected)
                {
                    search.Type = "Scalar Output";
                    SearchResults.Add(search);
                }
            }
        }

        private void FilterRecordSets(IResourceModel resource, IDataListViewModel dtlist)
        {
            var resourcePath = resource.GetSavePath() + resource.ResourceName;
            foreach (var recset in dtlist.RecsetCollection)
            {
                var search = new SearchValue(resource.ID, resource.ResourceName, resourcePath, SelectedEnvironment)
                {
                    Match = recset.Name
                };
                if (IsRecSetNameSelected && FilterText(recset.Name))
                {
                    search.Type = "RecordSet";
                    SearchResults.Add(search);
                }
                if (recset.Input && IsInputFieldSelected)
                {
                    search.Type = "RecordSet Input";
                    SearchResults.Add(search);
                }
                if (recset.Output && IsOutputVariableSelected)
                {
                    search.Type = "RecordSet Output";
                    SearchResults.Add(search);
                }
            }
        }

        private void FilterComplexObjects(IResourceModel resource, IDataListViewModel dtlist)
        {
            var resourcePath = resource.GetSavePath() + resource.ResourceName;
            foreach (var complexObj in dtlist.ComplexObjectCollection)
            {
                var search = new SearchValue(resource.ID, resource.ResourceName, resourcePath, SelectedEnvironment)
                {
                    Match = complexObj.Name
                };
                if (IsObjectNameSelected && FilterText(complexObj.Name))
                {
                    search.Type = "Object";
                    SearchResults.Add(search);
                }
                if (complexObj.Input && IsInputFieldSelected)
                {
                    search.Type = "Object Input";
                    SearchResults.Add(search);
                }
                if (complexObj.Output && IsOutputVariableSelected)
                {
                    search.Type = "Object Output";
                    SearchResults.Add(search);
                }
            }
        }

        bool FilterText(string filterValue)
        {
            var searchText = filterValue;
            var valueToFilter = SearchInput;
            if (!IsMatchCaseSelected)
            {
                searchText = filterValue.ToLower();
                valueToFilter = SearchInput.ToLower();
            }
            var isMatch = IsMatchWholeWordSelected ? searchText.Equals(valueToFilter) : searchText.Contains(valueToFilter);
            return isMatch;
        }

        public bool IsAllSelected
        {
            get => _isAllSelected;
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
            get => _isWorkflowNameSelected;
            set
            {
                _isWorkflowNameSelected = value;
                OnPropertyChanged(() => IsWorkflowNameSelected);
            }
        }
        public bool IsTestNameSelected
        {
            get => _isTestNameSelected;
            set
            {
                _isTestNameSelected = value;
                OnPropertyChanged(() => IsTestNameSelected);
            }
        }
        public bool IsScalarNameSelected
        {
            get => _isScalarNameSelected;
            set
            {
                _isScalarNameSelected = value;
                OnPropertyChanged(() => IsScalarNameSelected);
            }
        }
        public bool IsObjectNameSelected
        {
            get => _isObjectNameSelected;
            set
            {
                _isObjectNameSelected = value;
                OnPropertyChanged(() => IsObjectNameSelected);
            }
        }
        public bool IsRecSetNameSelected
        {
            get => _isRecSetNameSelected;
            set
            {
                _isRecSetNameSelected = value;
                OnPropertyChanged(() => IsRecSetNameSelected);
            }
        }
        public bool IsToolNameSelected
        {
            get => _isToolNameSelected;
            set
            {
                _isToolNameSelected = value;
                OnPropertyChanged(() => IsToolNameSelected);
            }
        }
        public bool IsToolTitleSelected
        {
            get => _isToolTitleSelected;
            set
            {
                _isToolTitleSelected = value;
                OnPropertyChanged(() => IsToolTitleSelected);
            }
        }
        public bool IsInputFieldSelected
        {
            get => _isInputFieldSelected;
            set
            {
                _isInputFieldSelected = value;
                OnPropertyChanged(() => IsInputFieldSelected);
            }
        }
        public bool IsInputVariableSelected
        {
            get => _isInputVariableSelected;
            set
            {
                _isInputVariableSelected = value;
                OnPropertyChanged(() => IsInputVariableSelected);
            }
        }
        public bool IsOutputVariableSelected
        {
            get => _isOutputVariableSelected;
            set
            {
                _isOutputVariableSelected = value;
                OnPropertyChanged(() => IsOutputVariableSelected);
            }
        }
        public bool IsMatchCaseSelected
        {
            get => _isMatchCaseSelected;
            set
            {
                _isMatchCaseSelected = value;
                OnPropertyChanged(() => IsMatchCaseSelected);
            }
        }
        public bool IsMatchWholeWordSelected
        {
            get => _isMatchWholeWordSelected;
            set
            {
                _isMatchWholeWordSelected = value;
                OnPropertyChanged(() => IsMatchWholeWordSelected);
            }
        }
        public string SearchInput
        {
            get => _searchInput;
            set
            {
                _searchInput = value;
                OnPropertyChanged(() => SearchInput);
            }
        }
        public ICommand SearchInputCommand
        {
            get => _searchInputCommand;
            set
            {
                _searchInputCommand = value;
                OnPropertyChanged(() => SearchInputCommand);
            }
        }
        public ObservableCollection<ISearchValue> SearchResults
        {
            get => _searchResults;
            set
            {
                _searchResults = value;
                OnPropertyChanged(() => SearchResults);
            }
        }
    }
}