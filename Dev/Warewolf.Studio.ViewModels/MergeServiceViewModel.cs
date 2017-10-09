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
        private readonly IMergeView _view;
        readonly IShellViewModel _shellViewModel;
        private IExplorerItemViewModel _selectedResource;
        private string _resourceToMerge;
        private IExplorerItemViewModel _selectMergeItem;
        private IConnectControlViewModel _mergeConnectControlViewModel;

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
            _view = mergeView;

            Environments = new ObservableCollection<IEnvironmentViewModel> { localhostEnvironment };

            MergeConnectControlViewModel = new ConnectControlViewModel(_shellViewModel.LocalhostServer, aggregator, _shellViewModel.ExplorerViewModel.ConnectControlViewModel.Servers)
            {
                SelectedConnection = selectedServer
            };

            LoadVersions(selectedResource, selectedResource.Server);
            ShowConnectControl = true;
            MergeConnectControlViewModel.CanEditServer = false;
            MergeConnectControlViewModel.CanCreateServer = false;
            MergeConnectControlViewModel.ServerConnected += async (sender, server) => { await ServerConnected(server).ConfigureAwait(false); };
            MergeConnectControlViewModel.ServerDisconnected += ServerDisconnected;

            ShowConnectControl = false;
            IsRefreshing = false;
            RefreshCommand = new DelegateCommand(() => RefreshEnvironment(SelectedEnvironment.ResourceId));
            CancelCommand = new DelegateCommand(Cancel);
            MergeCommand = new DelegateCommand(Merge, CanMerge);

            MergeConnectControlViewModel.SelectedEnvironmentChanged += async (sender, id) =>
            {
                await MergeExplorerViewModelSelectedEnvironmentChanged(sender).ConfigureAwait(false);
            };
        }

        private void LoadVersions(IExplorerItemViewModel selectedResource, IServer server)
        {
            MergeResourceVersions = new ObservableCollection<IExplorerItemViewModel>();
            var versionInfos = server.ExplorerRepository.GetVersions(selectedResource.ResourceId);
            if (versionInfos.Count <= 0)
            {
                return;
            }
            var popupController = CustomContainer.Get<IPopupController>();

            var children =
                new ObservableCollection<IExplorerItemViewModel>(
                    versionInfos.Select(
                        a => new VersionViewModel(server, selectedResource, null, _shellViewModel, popupController)
                        {
                            ResourceName =
                                "v." + a.VersionNumber + " " +
                                a.DateTimeStamp.ToString(CultureInfo.InvariantCulture) + " " +
                                a.Reason.Replace(".xml", "").Replace(".bite", ""),
                            VersionNumber = a.VersionNumber,
                            VersionInfo = a,
                            ResourceId = selectedResource.ResourceId,
                            IsVersion = true,
                            IsMergeVisible = true,
                            CanEdit = false,
                            CanCreateWorkflowService = false,
                            ShowContextMenu = true,
                            CanCreateSource = false,
                            IsResourceVersion = true,
                            AllowResourceCheck = false,
                            IsResourceChecked = false,
                            ResourceType = "Version"
                        }));

            MergeResourceVersions.AddRange(children);
        }

        public ObservableCollection<IExplorerItemViewModel> MergeResourceVersions { get; set; }

        private bool CanMerge() => SelectedMergeItem != null && SelectedMergeItem.ResourceId == SelectedResource.ResourceId;

        private void Merge()
        {
            _view?.RequestClose();
            ViewResult = MessageBoxResult.OK;
            MergeConnectControlViewModel = null;
        }

        private void Cancel()
        {
            _view?.RequestClose();
            ViewResult = MessageBoxResult.Cancel;
            MergeConnectControlViewModel = null;
        }

        MessageBoxResult ViewResult { get; set; }

        public override ObservableCollection<IEnvironmentViewModel> Environments
        {
            get
            {
                return new ObservableCollection<IEnvironmentViewModel>(_environments.Where(a => a.IsVisible));
            }
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
                ResourceToMerge = _selectMergeItem.ResourceName;
                OnPropertyChanged(() => SelectedMergeItem);
                ViewModelUtils.RaiseCanExecuteChanged(MergeCommand);
            }
        }

        async Task MergeExplorerViewModelSelectedEnvironmentChanged(object sender)
        {
            var connectControlViewModel = sender as ConnectControlViewModel;
            var selectedConnection = connectControlViewModel?.SelectedConnection;
            if (selectedConnection != null && selectedConnection.IsConnected && _environments.Any(p => p.ResourceId != selectedConnection.EnvironmentID))
            {
                await CreateNewEnvironment(selectedConnection).ConfigureAwait(false);
                LoadVersions(_selectedResource, selectedConnection);
            }
        }

        //private void UpdateItemForMerge(Guid environmentId)
        //{
        //    var environmentViewModel = _environments.FirstOrDefault(a => a.Server.EnvironmentID == environmentId);
        //    if (environmentViewModel != null)
        //    {
        //        environmentViewModel.IsVisible = true;
        //        SelectedEnvironment = environmentViewModel;
        //        environmentViewModel.ShowContextMenu = false;
        //        environmentViewModel.AsList().Apply(a =>
        //        {
        //            a.ShowContextMenu = false;
        //            a.CanView = false;
        //        });
        //    }
        //    if (SelectedEnvironment != null)
        //    {
        //        SelectedEnvironment.AllowResourceCheck = false;
        //    }
        //    foreach (var env in _environments.Where(a => a.Server.EnvironmentID != environmentId))
        //    {
        //        env.IsVisible = false;
        //    }
        //    if (SearchText != null)
        //    {
        //        environmentViewModel?.Filter(SearchText);
        //    }

        //    OnPropertyChanged(() => Environments);
        //}

        async Task<bool> ServerConnected(IServer server)
        {
            var isCreated = await CreateNewEnvironment(server).ConfigureAwait(false);
            LoadVersions(_selectedResource, server);
            return isCreated;
        }

        private async Task<bool> CreateNewEnvironment(IServer server)
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
                isLoaded = await environmentModel.Load().ConfigureAwait(false);
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

        IEnvironmentViewModel CreateEnvironmentFromServer(IServer server, IShellViewModel shellViewModel)
        {
            return new EnvironmentViewModel(server, shellViewModel);
        }

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
