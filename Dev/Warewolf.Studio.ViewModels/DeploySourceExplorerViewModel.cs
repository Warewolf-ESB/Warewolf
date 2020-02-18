#pragma warning disable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using Dev2.Common;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Studio.Interfaces;
using Dev2.Studio.Interfaces.Deploy;

namespace Warewolf.Studio.ViewModels
{
    public class DeploySourceExplorerViewModel : ExplorerViewModelBase, IDeploySourceExplorerViewModel
    {
        readonly IDeployStatsViewerViewModel _statsArea;
        readonly IShellViewModel _shellViewModel;
        readonly Action<IExplorerItemViewModel> _selectAction;
        bool _loaded;
        IEnumerable<IExplorerTreeItem> _preselected;
        Version _serverVersion;
        object _serverInformation;
        readonly IEnvironmentViewModel _selectedEnv;
        bool _isDeployLoading;

        public DeploySourceExplorerViewModel(IShellViewModel shellViewModel, Microsoft.Practices.Prism.PubSubEvents.IEventAggregator aggregator, IDeployStatsViewerViewModel statsArea)
            : this(shellViewModel, aggregator, statsArea, null)
        {
        }

        public DeploySourceExplorerViewModel(IShellViewModel shellViewModel, Microsoft.Practices.Prism.PubSubEvents.IEventAggregator aggregator, IDeployStatsViewerViewModel statsArea, IEnvironmentViewModel selectedEnvironment)
        {
            if (shellViewModel == null)
            {
                throw new ArgumentNullException(nameof(shellViewModel));
            }
            var localhostEnvironment = CreateEnvironmentFromServer(shellViewModel.LocalhostServer, shellViewModel);
            _shellViewModel = shellViewModel;
            _selectAction = SelectAction;
            _selectedEnv = selectedEnvironment;

            Environments = new ObservableCollection<IEnvironmentViewModel> { localhostEnvironment };
            LoadLocalHostEnvironment(localhostEnvironment);
            ConnectControlViewModel = new ConnectControlViewModel(_shellViewModel.LocalhostServer, aggregator, _shellViewModel.ExplorerViewModel.ConnectControlViewModel.Servers);

            ShowConnectControl = true;
            ConnectControlViewModel.ServerDisconnected += ServerDisconnected;
            _statsArea = statsArea;
            foreach (var environmentViewModel in _environments)
            {
                environmentViewModel.SelectAction = SelectAction;
            }

            if (ConnectControlViewModel.SelectedConnection != null && !ConnectControlViewModel.SelectedConnection.IsLocalHost)
            {
                UpdateItemForDeploy(ConnectControlViewModel.SelectedConnection.EnvironmentID);
            }
            IsRefreshing = false;
            ShowConnectControl = false;
            RefreshCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(() => RefreshEnvironment(SelectedEnvironment.ResourceId));
            ConnectControlViewModel.SelectedEnvironmentChanged += ConnectControlSelectedExplorerEnvironmentChanged;
            ConnectControlViewModel.ServerConnected += (sender, server) =>
            {
                IsDeployLoading = true;
                ServerConnectedAsync(server).ContinueWith(t =>
                {
                    IsDeployLoading = false;
                }, TaskContinuationOptions.ExecuteSynchronously);
            };
            IsDeploy = true;
        }

        void LoadLocalHostEnvironment(IEnvironmentViewModel localhostEnvironment)
        {
            if (!localhostEnvironment.IsConnected)
            {
                localhostEnvironment.Connect();
            }
            LoadEnvironment(localhostEnvironment);
        }

        public bool IsDeployLoading
        {
            get => _isDeployLoading;
            set
            {
                _isDeployLoading = value;
                OnPropertyChanged(()=>IsDeployLoading);
            }
        }

        async void ConnectControlSelectedExplorerEnvironmentChanged(object sender, Guid id)
        {
            IsDeployLoading = true;
            SearchText = string.Empty;

            var connect = sender as IConnectControlViewModel;
            IEnvironmentViewModel environmentModel = null;
            var foundEnvironment = _environments.FirstOrDefault(environmentViewModel => environmentViewModel.ResourceId == id);
            if (foundEnvironment == null)
            {
                environmentModel = CreateEnvironmentFromServer(connect.SelectedConnection, _shellViewModel);
                if (connect.SelectedConnection.IsConnected)
                {
                    _environments.Add(environmentModel);
                    await environmentModel.LoadAsync(IsDeploy, false).ConfigureAwait(true);
                    OnPropertyChanged(() => Environments);
                }
            }
            else
            {
                environmentModel = foundEnvironment;
            }

            if (environmentModel != null)
            {
                UpdateItemForDeploy(id);
            }

            IsDeployLoading = false;
        }

        protected void AfterLoad(Guid environmentID)
        {
            _loaded = true;
            var environmentViewModel = _environments.FirstOrDefault(a => a.Server.EnvironmentID == environmentID);
            if (environmentViewModel != null)
            {
                UpdateItemForDeploy(environmentViewModel.Server.EnvironmentID);

                if (_serverInformation == null)
                {
                    _serverInformation = SelectedServer.GetServerInformation();
                }

                if (_serverVersion == null)
                {
                    var serverVersion = SelectedServer.GetServerVersion();
                    _serverVersion = serverVersion != Resources.Languages.Core.ServerVersionUnavailable ? Version.Parse(serverVersion) : null;
                }
                environmentViewModel.SelectAll = () => _statsArea.TryCalculate(environmentViewModel.AsList().Where(o => o.IsResourceChecked == true).Select(x => x as IExplorerTreeItem).ToList());
            }
            CheckPreselectedItems(environmentID);
            if (ConnectControlViewModel != null)
            {
                ConnectControlViewModel.IsLoading = false;
            }
        }

        void CheckPreselectedItems(Guid environmentID)
        {
            if (Preselected != null && Preselected.Any())
            {
                var envId = Preselected.First().Server.EnvironmentID;
                if (envId != environmentID)
                {
                    ConnectControlViewModel.SelectedConnection = ConnectControlViewModel.Servers.FirstOrDefault(a => a.EnvironmentID == envId);
                    if (ConnectControlViewModel.SelectedConnection?.Permissions == null)
                    {
                        ConnectControlViewModel.SelectedConnection.Permissions = new List<IWindowsGroupPermission>();
                    }
                    ConnectControlViewModel.TryConnectAsync(ConnectControlViewModel.SelectedConnection);
                }
                else
                {
                    SelectItemsForDeploy(Preselected);
                    Preselected = null;
                }
            }
        }

        public override ObservableCollection<IEnvironmentViewModel> Environments
        {
            get => new ObservableCollection<IEnvironmentViewModel>(_environments.Where(a => a.IsVisible));
            set
            {
                _environments = value;
                OnPropertyChanged(() => Environments);
            }
        }

        void UpdateItemForDeploy(Guid environmentId)
        {
            var environmentViewModel = _environments.FirstOrDefault(a => a.Server.EnvironmentID == environmentId);
            if (environmentViewModel != null)
            {
                environmentViewModel.IsVisible = true;
                SelectedEnvironment = environmentViewModel;
                environmentViewModel.ShowContextMenu = false;
                environmentViewModel.AsList().Apply(a =>
                {
                    a.CanExecute = false;
                    a.CanEdit = false;
                    a.CanView = false;
                    a.ShowContextMenu = false;
                    a.SelectAction = SelectAction;
                    a.AllowResourceCheck = true;
                    a.CanDrop = false;
                    a.CanDrag = false;
                });
            }
            if (SelectedEnvironment != null)
            {
                SelectedEnvironment.AllowResourceCheck = true;
            }
            foreach (var env in _environments.Where(a => a.Server.EnvironmentID != environmentId))
            {
                env.IsVisible = false;
            }
            if (SearchText != null)
            {
                environmentViewModel?.Filter(SearchText);
            }

            OnPropertyChanged(() => Environments);
        }

        void SelectAction(IExplorerItemViewModel ax)
        {
            if (ax?.Parent?.ResourceType == @"Folder" || ax?.Parent?.ResourceType == @"ServerSource")
            {
                ax.Parent.IsFolderChecked = ax.Parent.UnfilteredChildren?.Flatten(a => a.UnfilteredChildren ?? new ObservableCollection<IExplorerItemViewModel>()).All(a => a.IsResourceChecked == true);
            }

            _statsArea.TryCalculate(SelectedItems.ToList());
        }

        public virtual ICollection<IExplorerTreeItem> SelectedItems
        {
            get
            {
                var explorerItemViewModels = FlatUnfilteredChildren(SelectedEnvironment);
                var explorerTreeItems = explorerItemViewModels?.Select(a => a as IExplorerTreeItem)
                    .Where(a => a.IsResourceChecked.HasValue && a.IsResourceChecked.Value)
                    .ToList();
                return explorerTreeItems;
            }
            set
            {
                foreach (var explorerTreeItem in value)
                {
                    Select(explorerTreeItem);
                }
            }
        }

        static IEnumerable<IExplorerItemViewModel> FlatUnfilteredChildren(IEnvironmentViewModel itemViewModelsModel)
        {
            var itemViewModels = itemViewModelsModel?.AsList()?.Flatten(model => model.Children ?? new ObservableCollection<IExplorerItemViewModel>());
            var explorerItemViewModels = itemViewModelsModel?.UnfilteredChildren.Flatten(model => model.UnfilteredChildren ?? new ObservableCollection<IExplorerItemViewModel>());
            var viewModels = explorerItemViewModels?.Union(itemViewModels ?? new List<IExplorerItemViewModel>());
            return viewModels ?? new List<IExplorerItemViewModel>();
        }

        public ICollection<IExplorerTreeItem> SourceLoadedItems
        {
            get
            {
                var explorerItemViewModels = FlatUnfilteredChildren(SelectedEnvironment);
                return explorerItemViewModels?.Select(a => a as IExplorerTreeItem)
                    .ToList() ?? new List<IExplorerTreeItem>();
            }
        }


        public IEnumerable<IExplorerTreeItem> Preselected
        {
            get => _preselected;
            set
            {
                _preselected = value;
                if (_loaded && _preselected != null && _preselected.Any())
                {
                    CheckPreselectedItems(Preselected.First().Server.EnvironmentID);
                }
            }
        }

        void Select(IExplorerTreeItem explorerTreeItem)
        {
            var explorerItemViewModels = SelectedEnvironment?.AsList();
            var item = explorerItemViewModels?.FirstOrDefault(a => a.ResourceId == explorerTreeItem.ResourceId);
            if (item != null)
            {
                item.IsSelected = true;
            }
        }

        public virtual Version ServerVersion => _serverVersion ?? (_serverVersion = Version.Parse(SelectedServer.GetServerVersion()));

        void SelectItemsForDeploy(IEnumerable<IExplorerTreeItem> selectedItems)
        {
            var count = SelectedEnvironment.AsList().Count + 1;
            var explorerTreeItems = selectedItems as IExplorerTreeItem[] ?? selectedItems.ToArray();
            var count1 = explorerTreeItems.Length;

            if (count == count1)
            {
                SelectedEnvironment.IsResourceChecked = true;
            }

            SelectedEnvironment.AsList().Apply(a => a.IsResourceChecked = explorerTreeItems.Contains(a));

            foreach (var explorerTreeItem in explorerTreeItems)
            {
                if (explorerTreeItem.ResourceType != null && (explorerTreeItem.ResourceType.Equals("Folder", StringComparison.CurrentCultureIgnoreCase) && explorerTreeItem.Children.Count == 0))
                {
                    var itemViewModels = SelectedEnvironment.AsList()
                        .Where(model => model.ResourceName.Equals(explorerTreeItem.ResourceName, StringComparison.InvariantCultureIgnoreCase))
                        .Flatten(model => model.Children);
                    SelectedEnvironment.AsList()
                        .Where(model => model.ResourceType.Equals("Folder", StringComparison.InvariantCultureIgnoreCase))
                        .Apply(a => a.IsResourceChecked = itemViewModels.Contains(a));
                }
            }
        }

        async Task ServerConnectedAsync(IServer server)
        {
            await CreateNewEnvironmentAsync(server).ConfigureAwait(true);
        }

        async Task<bool> CreateNewEnvironmentAsync(IServer server)
        {
            var isLoaded = false;
            if (server == null)
            {
                return false;
            }
            IEnvironmentViewModel environmentModel = null;
            var foundEnvironment = _environments.FirstOrDefault(environmentViewModel => environmentViewModel.ResourceId == server.EnvironmentID);
            if (foundEnvironment == null)
            {
                environmentModel = CreateEnvironmentFromServer(server, _shellViewModel);
                if (server.IsConnected)
                {
                    _environments.Add(environmentModel);
                }
            }
            else
            {
                environmentModel = foundEnvironment;
            }
            isLoaded = await environmentModel.LoadAsync(IsDeploy, false).ConfigureAwait(true);
            OnPropertyChanged(() => Environments);
            _statsArea.TryCalculate(environmentModel.AsList().Select(model => model as IExplorerTreeItem).ToList());
            AfterLoad(server.EnvironmentID);
            return isLoaded;
        }

        bool IsDeploy { get; set; }

        void ServerDisconnected(object _, IServer server)
        {
            var environmentModel = _environments.FirstOrDefault(model => model.Server.EnvironmentID == server.EnvironmentID);
            if (environmentModel != null)
            {
                _environments.Remove(environmentModel);
            }
            _statsArea.TryCalculate(new List<IExplorerTreeItem>());
            OnPropertyChanged(() => Environments);
        }

        protected virtual async void LoadEnvironment(IEnvironmentViewModel localhostEnvironment)
        {
            await localhostEnvironment.LoadAsync(true, false).ConfigureAwait(true);
            AfterLoad(localhostEnvironment.Server.EnvironmentID);
        }

        IEnvironmentViewModel CreateEnvironmentFromServer(IServer server, IShellViewModel shellViewModel) => new EnvironmentViewModel(server, shellViewModel, false, _selectAction);

        public override void Dispose()
        {
            base.Dispose();
            ConnectControlViewModel.SelectedEnvironmentChanged -= ConnectControlSelectedExplorerEnvironmentChanged;
            foreach (var environmentViewModel in _environments)
            {
                environmentViewModel.IsResourceChecked = false;
            }
        }
    }
}