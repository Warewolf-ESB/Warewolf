using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Deploy;
using Dev2.Common.Interfaces.Infrastructure;

// ReSharper disable InconsistentNaming

namespace Warewolf.Studio.ViewModels
{
    public class DeploySourceExplorerViewModel : ExplorerViewModelBase, IDeploySourceExplorerViewModel
    {
        readonly IDeployStatsViewerViewModel _statsArea;

        #region Implementation of IDeployDestinationExplorerViewModel

        readonly IShellViewModel _shellViewModel;
        readonly Action<IExplorerItemViewModel> _selectAction;
        bool _loaded;
        IEnumerable<IExplorerTreeItem> _preselected;
        private Version _serverVersion;
        private object _serverInformation;

        public DeploySourceExplorerViewModel(IShellViewModel shellViewModel, Microsoft.Practices.Prism.PubSubEvents.IEventAggregator aggregator, IDeployStatsViewerViewModel statsArea)
        {
            if (shellViewModel == null)
            {
                throw new ArgumentNullException(nameof(shellViewModel));
            }
            var localhostEnvironment = CreateEnvironmentFromServer(shellViewModel.LocalhostServer, shellViewModel);
            _shellViewModel = shellViewModel;
            _selectAction = SelectAction;
            // ReSharper disable once VirtualMemberCallInContructor
            Environments = new ObservableCollection<IEnvironmentViewModel> { localhostEnvironment };
            // ReSharper disable once VirtualMemberCallInContructor
            LoadEnvironment(localhostEnvironment);

            ConnectControlViewModel = new ConnectControlViewModel(shellViewModel.LocalhostServer, aggregator) { ShouldUpdateActiveEnvironment = false };
            ShowConnectControl = true;
            ConnectControlViewModel.ServerConnected += ServerConnected;
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
            ConnectControlViewModel.SelectedEnvironmentChanged += DeploySourceExplorerViewModelSelectedEnvironmentChanged;
            IsDeploy = true;
        }

        void DeploySourceExplorerViewModelSelectedEnvironmentChanged(object sender, Guid environmentId)
        {
            UpdateItemForDeploy(environmentId);
        }

        #region Overrides of ExplorerViewModel

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
                    _serverVersion = Version.Parse(SelectedServer.GetServerVersion());
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

        #endregion

        public override ObservableCollection<IEnvironmentViewModel> Environments
        {
            get
            {
                return new ObservableCollection<IEnvironmentViewModel>(_environments.Where(a => a.IsVisible));
            }
            set
            {
                if (value?.Count == 0)
                {

                }
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
            SelectedEnvironment.AllowResourceCheck = true;
            foreach (var env in _environments.Where(a => a.Server.EnvironmentID != environmentId))
            {
                env.IsVisible = false;
            }
            if (SearchText != null)
            {
                environmentViewModel?.Filter(SearchText);
            }

            //var windowsGroupPermission = environmentViewModel?.Server?.Permissions?[0];
            //if (windowsGroupPermission != null)
            //environmentViewModel.SetPropertiesForDialogFromPermissions(windowsGroupPermission);

            //var permissions = environmentViewModel?.Server?.GetPermissions(environmentViewModel.ResourceId);
            //if (permissions != null)
            //{
            //    if (environmentViewModel.Children != null)
            //        foreach (var explorerItemViewModel in environmentViewModel.Children.Flatten(model => model.Children))
            //        {
            //            explorerItemViewModel.SetPermissions((Permissions)permissions, true);
            //        }
            //}

            OnPropertyChanged(() => Environments);
        }

        void SelectAction(IExplorerItemViewModel ax)
        {
            if (ax.Parent?.ResourceType == @"Folder" || ax.Parent?.ResourceType == @"ServerSource")
                ax.Parent.IsFolderChecked = ax.IsResourceChecked;
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
                    CheckPreselectedItems(Preselected.First().Server.EnvironmentID);
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

        /// <summary>
        /// used to select a list of items from the explorer
        /// </summary>
        /// <param name="selectedItems"></param>
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

        #endregion

        async void ServerConnected(object _, IServer server)
        {
            if (server != null)
            {
                var environmentModel = CreateEnvironmentFromServer(server, _shellViewModel);
                _environments.Add(environmentModel);
                await environmentModel.Load(IsDeploy);
                OnPropertyChanged(() => Environments);
                AfterLoad(server.EnvironmentID);
                _statsArea.Calculate(environmentModel.AsList().Select(model => model as IExplorerTreeItem).ToList());
            }
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
            await localhostEnvironment.Load(true, true);
            AfterLoad(localhostEnvironment.Server.EnvironmentID);
        }

        IEnvironmentViewModel CreateEnvironmentFromServer(IServer server, IShellViewModel shellViewModel)
        {
            return new EnvironmentViewModel(server, shellViewModel, false, _selectAction);
        }
    }
}