#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
﻿/*
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
using System;
using Dev2.Common;

namespace Dev2.ViewModels.Search
{
    public class SearchViewModel : ExplorerViewModel, ISearchViewModel
    {
        ICommand _searchInputCommand;
        ObservableCollection<ISearchResult> _searchResults;
        bool _isSearching;
        readonly IShellViewModel _shellViewModel;
        string _displayName;
        bool _canShowResults;
        string _versionConflictError;

        public SearchViewModel(IShellViewModel shellViewModel, IEventAggregator aggregator)
            : base(shellViewModel, aggregator, false)
        {
            _shellViewModel = shellViewModel;
            ConnectControlViewModel = new ConnectControlViewModel(shellViewModel.LocalhostServer, aggregator, shellViewModel.ExplorerViewModel.ConnectControlViewModel.Servers);
            ConnectControlViewModel.ServerConnected += async (sender, server) => { await ServerConnectedAsync(sender, server).ConfigureAwait(false); };
            ConnectControlViewModel.ServerDisconnected += ServerDisconnected;
            ConnectControlViewModel.SelectedEnvironmentChanged += UpdateServerCompareChanged;
            SelectedEnvironment = _environments.FirstOrDefault();
            RefreshCommand = new DelegateCommand((o) => RefreshEnvironment(SelectedEnvironment.ResourceId));
            SearchInputCommand = new DelegateCommand((o) => SearchWarewolf());
            OpenResourceCommand = new DelegateCommand((searchObject) =>
            {
                var searchResult = searchObject as ISearchResult;
                OpenResource(searchResult);
            });
            CanShowResults = true;
            SearchResults = new ObservableCollection<ISearchResult>();
            Search = new Common.Search.Search();
            SelectedEnvironment?.Server?.ResourceRepository?.Load(false);
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
                UpdateServerSearchAllowed();
            }
            return environmentViewModel;
        }

        void UpdateServerCompareChanged(object sender, Guid environmentid)
        {
            UpdateServerSearchAllowed();
        }

        private void UpdateServerSearchAllowed()
        {
            CanShowResults = true;
            Search.SearchInput = string.Empty;
            SearchResults.Clear();

            var serverVersion = Version.Parse(SelectedEnvironment?.Server?.GetServerVersion());
            var minServerVersion = Version.Parse(SelectedEnvironment?.Server?.GetMinSupportedVersion());

            if (serverVersion < ServerVersion)
            {
                CanShowResults = false;
                VersionConflictError = Warewolf.Studio.Resources.Languages.Core.SearchVersionConflictError +
                                        Environment.NewLine + GlobalConstants.ServerVersion + ServerVersion +
                                        Environment.NewLine + GlobalConstants.MinimumSupportedVersion + minServerVersion;
            }
        }

        public Version MinSupportedVersion => Version.Parse(_shellViewModel.LocalhostServer.GetServerVersion());

        public Version ServerVersion => Version.Parse(_shellViewModel.LocalhostServer.GetMinSupportedVersion());

        public event ServerState ServerStateChanged;

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

        void SearchWarewolf()
        {
            IsSearching = true;

            SearchResults.Clear();
            if (!string.IsNullOrWhiteSpace(Search.SearchInput))
            {
                var results = _shellViewModel.ActiveServer.ResourceRepository.Filter(Search);
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

        public ISearch Search { get; set; }
        public bool CanShowResults
        {
            get => _canShowResults;
            set
            {
                _canShowResults = value;
                OnPropertyChanged(() => CanShowResults);
            }
        }

        public string VersionConflictError
        {
            get => _versionConflictError;
            set
            {
                _versionConflictError = value;
                OnPropertyChanged(() => VersionConflictError);
            }
        }

        public new void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IShellViewModel>();
            mainViewModel?.HelpViewModel?.UpdateHelpText(helpText);
        }
    }
}