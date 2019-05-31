#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Studio.Interfaces;
using Microsoft.Practices.Prism;
using Microsoft.Practices.Prism.Commands;

namespace Warewolf.Studio.ViewModels
{
    public class MergeServiceViewModel : ExplorerViewModelBase, IMergeServiceViewModel
    {
        readonly IMergeView _view;
        readonly IShellViewModel _shellViewModel;
        IExplorerItemViewModel _selectedResource;
        string _resourceToMerge;
        IExplorerItemViewModel _selectMergeItem;
        IConnectControlViewModel _mergeConnectControlViewModel;
        ObservableCollection<IExplorerItemViewModel> _mergeResourceVersions;

        public MergeServiceViewModel(IShellViewModel shellViewModel, Microsoft.Practices.Prism.PubSubEvents.IEventAggregator aggregator, IExplorerItemViewModel selectedResource, IMergeView mergeView, IServer selectedServer)
        {
            if (shellViewModel == null)
            {
                throw new ArgumentNullException(nameof(shellViewModel));
            }
            var localhostEnvironment = CreateEnvironmentFromServer(shellViewModel.LocalhostServer, shellViewModel);
            localhostEnvironment.Children = shellViewModel.ExplorerViewModel?.Environments[0].Children;
            _shellViewModel = shellViewModel;
            _selectedResource = selectedResource;

            ResourceToMerge = _selectedResource.IsVersion ? _selectedResource.Parent.ResourceName : _selectedResource.ResourceName;

            _view = mergeView;

            Environments = new ObservableCollection<IEnvironmentViewModel> { localhostEnvironment };

            MergeConnectControlViewModel = new ConnectControlViewModel(_shellViewModel.LocalhostServer, aggregator, _shellViewModel.ExplorerViewModel.ConnectControlViewModel.Servers)
            {
                SelectedConnection = selectedServer
            };

            LoadVersions(selectedResource.Server);
            ShowConnectControl = true;
            MergeConnectControlViewModel.CanEditServer = false;
            MergeConnectControlViewModel.CanCreateServer = false;
            MergeConnectControlViewModel.ServerConnected += async (sender, server) => { await ServerConnectedAsync(server).ConfigureAwait(false); };
            MergeConnectControlViewModel.ServerDisconnected += ServerDisconnected;

            ShowConnectControl = false;
            IsRefreshing = false;
            RefreshCommand = new DelegateCommand(() => RefreshEnvironment(SelectedEnvironment.ResourceId).ConfigureAwait(false));
            CancelCommand = new DelegateCommand(Cancel);
            MergeCommand = new DelegateCommand(Merge, CanMerge);

            MergeConnectControlViewModel.SelectedEnvironmentChanged += async (sender, id) =>
            {
                await MergeExplorerViewModelSelectedEnvironmentChangedAsync(sender).ConfigureAwait(false);
            };
        }

        void LoadVersions(IServer server)
        {
            MergeResourceVersions = new ObservableCollection<IExplorerItemViewModel>();
            var versionInfos = server.ExplorerRepository.GetVersions(_selectedResource.ResourceId);
            if (versionInfos.Count <= 0)
            {
                return;
            }

            if (_selectedResource.IsVersion)
            {
                var versionInfo = _selectedResource.VersionInfo;
                versionInfos = versionInfos.Where(info => versionInfo != null && info.VersionId == versionInfo.VersionId && info.VersionNumber != versionInfo.VersionNumber).ToList();

                var explorerItemViewModel = _selectedResource.Parent as IExplorerItemViewModel;
                MergeResourceVersions.Add(explorerItemViewModel);

                if (versionInfos.Count <= 0)
                {
                    if (MergeResourceVersions.Count == 1)
                    {
                        SelectedMergeItem = MergeResourceVersions[0];
                    }
                    return;
                }
            }

            var children =
                new ObservableCollection<IExplorerItemViewModel>(
                    versionInfos.Select(
                        a => new VersionViewModel(server, _selectedResource, null, _shellViewModel, CustomContainer.Get<IPopupController>())
                        {
                            ResourceName =
                                "v." + a.VersionNumber + " " +
                                a.DateTimeStamp.ToString(CultureInfo.InvariantCulture) + " " +
                                a.Reason.Replace(".xml", "").Replace(".bite", ""),
                            VersionNumber = a.VersionNumber,
                            VersionInfo = a,
                            ResourceId = _selectedResource.ResourceId,
                            IsVersion = true,
                            ResourceType = "Version"
                        }));

            MergeResourceVersions.AddRange(children);
            if (MergeResourceVersions.Count == 1)
            {
                SelectedMergeItem = MergeResourceVersions[0];
            }
        }

        public ObservableCollection<IExplorerItemViewModel> MergeResourceVersions
        {
            get => _mergeResourceVersions;
            set
            {
                _mergeResourceVersions = value;
                OnPropertyChanged(() => MergeResourceVersions);
            }
        }

        bool CanMerge() => SelectedMergeItem != null && SelectedMergeItem.ResourceId == SelectedResource.ResourceId;

        void Merge()
        {
            _view?.RequestClose();
            ViewResult = MessageBoxResult.OK;
            MergeConnectControlViewModel = null;
        }

        void Cancel()
        {
            _view?.RequestClose();
            ViewResult = MessageBoxResult.Cancel;
            MergeConnectControlViewModel = null;
        }

        MessageBoxResult ViewResult { get; set; }

        public override ObservableCollection<IEnvironmentViewModel> Environments
        {
            get => new ObservableCollection<IEnvironmentViewModel>(_environments.Where(a => a.IsVisible));
            set
            {
                _environments = value;
                OnPropertyChanged(() => Environments);
            }
        }

        public IExplorerItemViewModel SelectedMergeItem
        {
            get => _selectMergeItem;
            set
            {
                _selectMergeItem = value;
                OnPropertyChanged(() => SelectedMergeItem);
                ViewModelUtils.RaiseCanExecuteChanged(MergeCommand);
            }
        }

        async Task MergeExplorerViewModelSelectedEnvironmentChangedAsync(object sender)
        {
            var connectControlViewModel = sender as ConnectControlViewModel;
            var selectedConnection = connectControlViewModel?.SelectedConnection;
            if (selectedConnection != null && selectedConnection.IsConnected && _environments.Any(p => p.ResourceId != selectedConnection.EnvironmentID))
            {
                await CreateNewEnvironmentAsync(selectedConnection).ConfigureAwait(false);
                LoadVersions(selectedConnection);
            }
        }

        async Task<bool> ServerConnectedAsync(IServer server)
        {
            var isCreated = await CreateNewEnvironmentAsync(server).ConfigureAwait(false);
            LoadVersions(server);
            return isCreated;
        }

        async Task<bool> CreateNewEnvironmentAsync(IServer server)
        {
            var isLoaded = false;
            if (server == null)
            {
                return false;
            }
            var createNew = _environments.All(environmentViewModel => environmentViewModel.ResourceId != server.EnvironmentID);
            if (createNew)
            {
                var environmentModel = CreateEnvironmentFromServer(server, _shellViewModel);
                _environments.Add(environmentModel);
                isLoaded = await environmentModel.LoadAsync().ConfigureAwait(false);
                OnPropertyChanged(() => Environments);
            }
            return isLoaded;
        }

        void ServerDisconnected(object _, IServer server)
        {
            var environmentModel = _environments.FirstOrDefault(model => model.Server.EnvironmentID == server.EnvironmentID);
            if (environmentModel != null)
            {
                _environments.Remove(environmentModel);
            }
            OnPropertyChanged(() => Environments);
        }

        static IEnvironmentViewModel CreateEnvironmentFromServer(IServer server, IShellViewModel shellViewModel) => new EnvironmentViewModel(server, shellViewModel);

        public MessageBoxResult ShowMergeDialog()
        {
            _view.DataContext = this;
            _view.ShowView();

            return ViewResult;
        }

        public IExplorerItemViewModel SelectedResource
        {
            get => _selectedResource;
            set
            {
                _selectedResource = value;
                OnPropertyChanged(() => SelectedResource);
            }
        }

        public string ResourceToMerge
        {
            get => _resourceToMerge;
            set
            {
                _resourceToMerge = value;
                OnPropertyChanged(() => ResourceToMerge);
            }
        }

        public ICommand MergeCommand { get; set; }
        public ICommand CancelCommand { get; }

        public IConnectControlViewModel MergeConnectControlViewModel
        {
            get => _mergeConnectControlViewModel;
            private set
            {
                if (Equals(value, _mergeConnectControlViewModel))
                {
                    return;
                }
                _mergeConnectControlViewModel = value;
                OnPropertyChanged(() => MergeConnectControlViewModel);
            }
        }
    }
}
