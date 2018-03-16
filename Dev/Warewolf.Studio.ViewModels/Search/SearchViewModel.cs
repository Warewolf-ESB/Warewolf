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
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Common.Interfaces.Search;
using Microsoft.Practices.Prism;
using Dev2.Common.Search;

namespace Dev2.ViewModels.Search
{
    public class SearchViewModel : ExplorerViewModel, ISearchViewModel
    {
        ICommand _searchInputCommand;
        ObservableCollection<ISearchResult> _searchResults;
        bool _isSearching;
        readonly IShellViewModel _shellViewModel;
        string _displayName;

        public SearchViewModel(IShellViewModel shellViewModel, IEventAggregator aggregator)
            : base(shellViewModel, aggregator, false)
        {
            _shellViewModel = shellViewModel;
            ConnectControlViewModel = new ConnectControlViewModel(shellViewModel.LocalhostServer, aggregator, shellViewModel.ExplorerViewModel.ConnectControlViewModel.Servers);
            ConnectControlViewModel.ServerConnected += async (sender, server) => { await ServerConnectedAsync(sender, server).ConfigureAwait(false); };
            ConnectControlViewModel.ServerDisconnected += ServerDisconnected;
            SelectedEnvironment = _environments.FirstOrDefault();
            RefreshCommand = new DelegateCommand((o) => RefreshEnvironment(SelectedEnvironment.ResourceId));
            SearchInputCommand = new DelegateCommand((o) => SearchWarewolfAsync());
            OpenResourceCommand = new DelegateCommand((searchObject) =>
            {
                var searchResult = searchObject as ISearchResult;
                OpenResource(searchResult);
            });
            SearchResults = new ObservableCollection<ISearchResult>();
            SearchValue = new SearchValue();
            SelectedEnvironment?.Server?.ResourceRepository?.Load();
            IsSearching = false;
            DisplayName = "Search";
            UpdateHelpDescriptor(Warewolf.Studio.Resources.Languages.HelpText.MenuSearchHelp);
        }

        private void OpenResource(ISearchResult searchResult)
        {
            if (searchResult.Type == SearchItemType.TestName)
            {
                _shellViewModel.OpenSelectedTest(searchResult.ResourceId, searchResult.Match);
            }
            else
            {
                _shellViewModel.OpenResource(searchResult.ResourceId, _shellViewModel.ActiveServer.EnvironmentID, _shellViewModel.ActiveServer);
            }
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

        public string DisplayName
        {
            get => _displayName;
            set
            {
                _displayName = value;
                OnPropertyChanged(() => DisplayName);
            }
        }

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
                var results = _shellViewModel.ActiveServer.ResourceRepository.Filter(SearchValue);
                if (results != null)
                {
                    SearchResults.AddRange(results);
                }
            }
            IsSearching = false;
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
        public ICommand OpenResourceCommand { get; set; }
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

        public new void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IShellViewModel>();
            mainViewModel?.HelpViewModel?.UpdateHelpText(helpText);
        }
    }
}