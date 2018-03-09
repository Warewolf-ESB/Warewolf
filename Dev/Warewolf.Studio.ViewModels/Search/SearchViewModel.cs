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
using Dev2.Studio.Factory;
using Dev2.Studio.Interfaces.DataList;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Common.Interfaces.Search;
using Dev2.Common.Utils;
using Microsoft.Practices.Prism;
using Warewolf.ResourceManagement;
using Dev2.Runtime.Search;

namespace Dev2.ViewModels.Search
{
    public class SearchViewModel : ExplorerViewModel, ISearchViewModel
    {
        ICommand _searchInputCommand;
        ObservableCollection<ISearchResult> _searchResults;
        bool _isSearching;
        readonly IShellViewModel _shellViewModel;

        public SearchViewModel(IShellViewModel shellViewModel, IEventAggregator aggregator)
            : base(shellViewModel, aggregator, false)
        {
            _shellViewModel = shellViewModel;
            ConnectControlViewModel = new ConnectControlViewModel(shellViewModel.LocalhostServer, aggregator, shellViewModel.ExplorerViewModel.ConnectControlViewModel.Servers);
            ConnectControlViewModel.ServerConnected += async (sender, server) => { await ServerConnectedAsync(sender, server).ConfigureAwait(false); };
            ConnectControlViewModel.ServerDisconnected += ServerDisconnected;
            SelectedEnvironment = _environments.FirstOrDefault();
            RefreshCommand = new DelegateCommand((o) => RefreshEnvironment(SelectedEnvironment.ResourceId));
            SearchInputCommand = new DelegateCommand(async (o) =>
            {
                SearchWarewolfAsync();
            });
            SearchResults = new ObservableCollection<ISearchResult>();
            SearchValue = new SearchValue();
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

            SearchResults.Clear();
            if (!string.IsNullOrWhiteSpace(SearchValue.SearchInput))
            {
                if (SearchValue.SearchOptions.IsWorkflowNameSelected)
                {
                    FilterWorkflows();
                }
                if (SearchValue.SearchOptions.IsTestNameSelected)
                {
                    var results = _shellViewModel.ActiveServer.ResourceRepository.Filter(SearchValue);
                    SearchResults.AddRange(results);
                }
                if (IsAnyVariableOptionSelected)
                {
                    FilterVariables();
                }
                if (SearchValue.SearchOptions.IsToolTitleSelected)
                {
                    var results = _shellViewModel.ActiveServer.ResourceRepository.Filter(SearchValue);
                    SearchResults.AddRange(results);
                }
            }
            IsSearching = false;
        }

        private bool IsAnyVariableOptionSelected => SearchValue.SearchOptions.IsScalarNameSelected || SearchValue.SearchOptions.IsRecSetNameSelected || SearchValue.SearchOptions.IsObjectNameSelected || SearchValue.SearchOptions.IsInputVariableSelected || SearchValue.SearchOptions.IsOutputVariableSelected;

        private void FilterWorkflows()
        {
            var children = SelectedEnvironment.Children.Flatten(model => model.Children).Where(model => FilterText(model.ResourceName));
            foreach (var child in children)
            {
                var type = SearchItemType.WorkflowName;
                if (child.IsSource)
                {
                    type = SearchItemType.SourceName;
                }

                var search = new SearchResult(child.ResourceId, child.ResourceName, child.ResourcePath, type, child.ResourceName);
                SearchResults.Add(search);
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
                var search = new SearchResult(resource.ID, resource.ResourceName, resourcePath, SearchItemType.Scalar, scalar.Name);
                if (SearchValue.SearchOptions.IsScalarNameSelected && FilterText(scalar.Name))
                {
                    SearchResults.Add(search);
                }
                if (scalar.Input && SearchValue.SearchOptions.IsInputVariableSelected)
                {
                    search.Type = SearchItemType.ScalarInput;
                    SearchResults.Add(search);
                }
                if (scalar.Output && SearchValue.SearchOptions.IsOutputVariableSelected)
                {
                    search.Type = SearchItemType.ScalarOutput;
                    SearchResults.Add(search);
                }
            }
        }

        private void FilterRecordSets(IResourceModel resource, IDataListViewModel dtlist)
        {
            var resourcePath = resource.GetSavePath() + resource.ResourceName;
            foreach (var recset in dtlist.RecsetCollection)
            {
                var search = new SearchResult(resource.ID, resource.ResourceName, resourcePath, SearchItemType.RecordSet, recset.Name);
                if (SearchValue.SearchOptions.IsRecSetNameSelected && FilterText(recset.Name))
                {
                    SearchResults.Add(search);
                }
                if (recset.Input && SearchValue.SearchOptions.IsInputVariableSelected)
                {
                    search.Type = SearchItemType.RecordSetInput;
                    SearchResults.Add(search);
                }
                if (recset.Output && SearchValue.SearchOptions.IsOutputVariableSelected)
                {
                    search.Type = SearchItemType.RecordSetOutput;
                    SearchResults.Add(search);
                }
            }
        }

        private void FilterComplexObjects(IResourceModel resource, IDataListViewModel dtlist)
        {
            var resourcePath = resource.GetSavePath() + resource.ResourceName;
            foreach (var complexObj in dtlist.ComplexObjectCollection)
            {
                var search = new SearchResult(resource.ID, resource.ResourceName, resourcePath, SearchItemType.Object, complexObj.Name);
                if (SearchValue.SearchOptions.IsObjectNameSelected && FilterText(complexObj.Name))
                {
                    SearchResults.Add(search);
                }
                if (complexObj.Input && SearchValue.SearchOptions.IsInputVariableSelected)
                {
                    search.Type = SearchItemType.ObjectInput;
                    SearchResults.Add(search);
                }
                if (complexObj.Output && SearchValue.SearchOptions.IsOutputVariableSelected)
                {
                    search.Type = SearchItemType.ObjectOutput;
                    SearchResults.Add(search);
                }
            }
        }

        bool FilterText(string filterValue) => SearchUtils.FilterText(filterValue, SearchValue);

        public ICommand SearchInputCommand
        {
            get => _searchInputCommand;
            set
            {
                _searchInputCommand = value;
                OnPropertyChanged(() => SearchInputCommand);
            }
        }
        public ObservableCollection<ISearchResult> SearchResults
        {
            get => _searchResults;
            set
            {
                _searchResults = value;
                OnPropertyChanged(() => SearchResults);
            }
        }

        public ISearchValue SearchValue { get; set; }
    }
}