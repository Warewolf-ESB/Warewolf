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
        private Version _serverVersion;
        private object _serverInformation;
        private readonly IEnvironmentViewModel _selectedEnv;

        public DeploySourceExplorerViewModel(IShellViewModel shellViewModel, Microsoft.Practices.Prism.PubSubEvents.IEventAggregator aggregator, IDeployStatsViewerViewModel statsArea)
            :this(shellViewModel, aggregator, statsArea, null)
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
            
            LoadEnvironment(localhostEnvironment);

            ConnectControlViewModel = new ConnectControlViewModel(_shellViewModel.LocalhostServer, aggregator, _shellViewModel.ExplorerViewModel.ConnectControlViewModel.Servers);

            ShowConnectControl = true;
            ConnectControlViewModel.ServerConnected += async (sender, server) => { await ServerConnected(server).ConfigureAwait(false); };
            ConnectControlViewModel.ServerDisconnected += ServerDisconnected;
            _statsArea = statsArea;
            foreach (var environmentViewModel in _environments)
            {
                environmentViewModel.SelectAction = SelectAction;
            }

            if (ConnectControlViewModel.SelectedConnection != null)
            {
                UpdateItemForDeploy(ConnectControlViewModel.SelectedConnection.EnvironmentID);
            }
            IsRefreshing = false;
            ShowConnectControl = false;
            RefreshCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(() => RefreshEnvironment(SelectedEnvironment.ResourceId));
            ConnectControlViewModel.SelectedEnvironmentChanged += async (sender, id) =>
            {
                await DeploySourceExplorerViewModelSelectedEnvironmentChanged(sender, id).ConfigureAwait(true);
            };
            IsDeploy = true;
        }

        async Task DeploySourceExplorerViewModelSelectedEnvironmentChanged(object sender, Guid environmentId)
        {
            var connectControlViewModel = sender as ConnectControlViewModel;
            if(connectControlViewModel?.SelectedConnection.IsConnected != null && connectControlViewModel.SelectedConnection.IsConnected && _environments.Any(p => p.ResourceId != connectControlViewModel.SelectedConnection.EnvironmentID))
            {
                var task = Task.Run(async () => { await CreateNewEnvironment(connectControlViewModel.SelectedConnection).ConfigureAwait(true); });
                task.Wait();
            }
            if (_environments.Count == _shellViewModel?.ExplorerViewModel?.Environments?.Count)
            {
                UpdateItemForDeploy(environmentId);
            }
            else
            {
                var environmentViewModel = _shellViewModel?.ExplorerViewModel?.Environments?.FirstOrDefault(
                    model => model.ResourceId == environmentId) ?? _environments.FirstOrDefault(p => p.ResourceId == environmentId);

                await CreateNewEnvironment(environmentViewModel?.Server).ConfigureAwait(true);
            }
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
                environmentViewModel.SelectAll = () => _statsArea.Calculate(environmentViewModel.AsList().Where(o => o.IsResourceChecked == true).Select(x => x as IExplorerTreeItem).ToList());

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
                    if (ConnectControlViewModel.SelectedConnection != null)
                    {
                        var server = ConnectControlViewModel.SelectedConnection;
                        if (server.Permissions == null)
                        {
                            server.Permissions = new List<IWindowsGroupPermission>();
                        }
                        ConnectControlViewModel.Connect(ConnectControlViewModel.SelectedConnection);
                    }
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

        private void UpdateItemForDeploy(Guid environmentId)
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
            if (ax.Parent?.ResourceType == @"Folder" || ax.Parent?.ResourceType == @"ServerSource")
            {
                ax.Parent.IsFolderChecked = ax.IsResourceChecked;
            }

            _statsArea.Calculate(SelectedItems.ToList());
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

        private IEnumerable<IExplorerItemViewModel> FlatUnfilteredChildren(IEnvironmentViewModel itemViewModelsModel)
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
            get
            {
                return _preselected;
            }
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

        private void SelectItemsForDeploy(IEnumerable<IExplorerTreeItem> selectedItems)
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

        async Task<bool> ServerConnected(IServer server)
        {
            var isCreated = await CreateNewEnvironment(server).ConfigureAwait(false);
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
                isLoaded = await environmentModel.Load(IsDeploy).ConfigureAwait(false);
                OnPropertyChanged(() => Environments);
                _statsArea.Calculate(environmentModel.AsList().Select(model => model as IExplorerTreeItem).ToList());
            }
            AfterLoad(server.EnvironmentID);
            return isLoaded;
        }

        private bool IsDeploy { get; set; }

        void ServerDisconnected(object _, IServer server)
        {
            var environmentModel = _environments.FirstOrDefault(model => model.Server.EnvironmentID == server.EnvironmentID);
            if (environmentModel != null)
            {
                _environments.Remove(environmentModel);
            }
            _statsArea.Calculate(new List<IExplorerTreeItem>());
            OnPropertyChanged(() => Environments);
        }

        protected virtual async void LoadEnvironment(IEnvironmentViewModel localhostEnvironment)
        {
            localhostEnvironment.Connect();
            await localhostEnvironment.Load(true, true).ConfigureAwait(true);
            var selectedEnvironment = SelectedEnvironment ?? _selectedEnv;
            if (selectedEnvironment?.DisplayName == localhostEnvironment.DisplayName)
            {
                AfterLoad(localhostEnvironment.Server.EnvironmentID);
            }
        }

        IEnvironmentViewModel CreateEnvironmentFromServer(IServer server, IShellViewModel shellViewModel)
        {
            return new EnvironmentViewModel(server, shellViewModel, false, _selectAction);
        }
    }
}