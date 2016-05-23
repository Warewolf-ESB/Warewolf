﻿using Caliburn.Micro;
using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Common.Interfaces.Security;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Services.Security;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Warewolf.Studio.Core;

namespace Warewolf.Studio.ViewModels
{
    public class EnvironmentViewModel : BindableBase, IEnvironmentViewModel
    {
        private ObservableCollection<IExplorerItemViewModel> _children;
        private bool _isConnecting;
        private bool _isConnected;
        private bool _isServerIconVisible;
        private bool _isServerUnavailableIconVisible;
        private bool _canCreateServerSource;
        private bool _isExpanded;
        private bool _isSelected;
        private bool _canCreateFolder;
        private bool _canShowServerVersion;
        private bool _canCreateWorkflowService;
        private readonly IShellViewModel _shellViewModel;
        private readonly bool _isDialog;
        private bool _allowEdit;
        private bool _allowResourceCheck;
        private bool? _isResourceChecked;
        private bool _isVisible;
        private bool _showContextMenu;
        private readonly IPopupController _controller;
        private bool _isLoading;
        private bool _canDrag;

        public EnvironmentViewModel(IServer server, IShellViewModel shellViewModel, bool isDialog = false, Action<IExplorerItemViewModel> selectAction = null)
        {
            if (server == null) throw new ArgumentNullException("server");
            if (shellViewModel == null) throw new ArgumentNullException("shellViewModel");
            _shellViewModel = shellViewModel;
            _isDialog = isDialog;
            _controller = CustomContainer.Get<IPopupController>();
            Server = server;
            _children = new ObservableCollection<IExplorerItemViewModel>();
            NewCommand = new DelegateCommand<string>(type =>
            {
                shellViewModel.SetActiveEnvironment(Server.EnvironmentID);
                shellViewModel.SetActiveServer(Server);
                shellViewModel.NewResource(type.ToString(), ResourcePath);
            });
            DisplayName = server.ResourceName;
            RefreshCommand = new DelegateCommand(async () =>
            {
                if (Children.Any(a => a.AllowResourceCheck))
                {
                    await Load(true);
                    ShowContextMenu = false;
                }
                else
                {
                    await Load();
                }
            });
            IsServerIconVisible = true;
            SelectAction = selectAction ?? (a => { });
            Expand = new DelegateCommand<int?>(clickCount =>
            {
                if (clickCount != null && clickCount == 2)
                {
                    IsExpanded = !IsExpanded;
                }
            });
            server.Connect();
            IsConnected = server.IsConnected;
            AllowEdit = server.AllowEdit;
            ShowServerVersionCommand = new DelegateCommand(ShowServerVersionAbout);
            CanCreateFolder = Server.UserPermissions == Permissions.Administrator || server.UserPermissions == Permissions.Contribute;
            CreateFolderCommand = new DelegateCommand(CreateFolder);
            Parent = null;
            ResourceType = "ServerSource";
            ResourcePath = "";
            ResourceName = DisplayName;
            CanShowServerVersion = true;
            AreVersionsVisible = false;
            IsVisible = true;
            SetPropertiesForDialogFromPermissions(new WindowsGroupPermission());
            SelectAll = () => { };
            CanDrag = false;
            CanDrop = false;
        }

        public IShellViewModel ShellViewModel
        {
            get
            {
                return _shellViewModel;
            }
        }

        public int ChildrenCount
        {
            get
            {
                return GetChildrenCount();
            }
        }

        private int GetChildrenCount()
        {
            int total = 0;
            foreach (var explorerItemModel in Children)
            {
                if (!explorerItemModel.IsResourceVersion && explorerItemModel.ResourceType != "Message")
                {
                    if (explorerItemModel.IsFolder)
                    {
                        total += explorerItemModel.ChildrenCount;
                    }
                    else
                    {
                        total++;
                    }
                }
            }
            return total;
        }

        public bool ShowContextMenu
        {
            get
            {
                return _showContextMenu;
            }
            set
            {
                _showContextMenu = value;
                OnPropertyChanged(() => ShowContextMenu);
            }
        }

        public bool CanCreateWorkflowService
        {
            get { return _canCreateWorkflowService; }
            set
            {
                _canCreateWorkflowService = value;
                OnPropertyChanged(() => CanCreateWorkflowService);
            }
        }

        public bool AreVersionsVisible { get; set; }

        public void CreateFolder()
        {
            IsExpanded = true;
            var id = Guid.NewGuid();
            var name = GetChildNameFromChildren();
            Server.ExplorerRepository.CreateFolder("root", name, id);
            var child = new ExplorerItemViewModel(Server, this, a => { SelectAction(a); }, _shellViewModel, _controller)
            {
                ResourceName = name,
                ResourceId = id,
                ResourceType = "Folder",
                IsFolder = true,
                ResourcePath = name,
                IsSelected = true,
                IsRenaming = true
            };
            if (_isDialog)
            {
                child.AllowResourceCheck = false;
                child.IsResourceChecked = false;
                child.CanCreateDbSource = false;
                child.CanCreateDropboxSource = false;
                child.CanCreateEmailSource = false;
                child.CanCreateRabbitMQSource = false;
                child.CanCreateExchangeSource = false;
                child.CanCreateServerSource = false;
                child.CanCreateSharePointSource = false;
                child.CanCreatePluginSource = false;
                child.CanCreateWebSource = false;
                child.CanCreateWorkflowService = false;
                child.ShowContextMenu = false;
                child.CanDeploy = false;
                child.CanShowDependencies = false;
            }
            else
            {
                child.AllowResourceCheck = AllowResourceCheck;
                child.IsResourceChecked = IsResourceChecked;
                child.CanCreateFolder = CanCreateFolder;
                child.CanCreateDbSource = CanCreateDbSource;
                child.CanShowVersions = CanShowVersions;
                child.CanRename = true;
                child.CanCreatePluginSource = CanCreatePluginSource;
                child.CanCreateEmailSource = CanCreateEmailSource;
                child.CanCreateRabbitMQSource = CanCreateRabbitMQSource;
                child.CanCreateExchangeSource = CanCreateExchangeSource;
                child.CanCreateDropboxSource = CanCreateDropboxSource;
                child.CanCreateSharePointSource = CanCreateSharePointSource;
                child.CanCreateServerSource = CanCreateServerSource;
                child.CanCreateWebSource = CanCreateWebSource;
                child.CanDeploy = CanDeploy;
                child.CanCreateWorkflowService = CanCreateWorkflowService;

                child.ShowContextMenu = ShowContextMenu;
            }
            _children.Insert(0, child);
            OnPropertyChanged(() => Children);
        }

        public ICommand ShowServerVersionCommand { get; set; }

        public Action<IExplorerItemViewModel> SelectAction { get; set; }

        public bool? IsFolderChecked
        {
            get
            {
                return _isResourceChecked;
            }
            set
            {
                if (value != null)
                {
                    _isResourceChecked = (bool)value;
                }
                OnPropertyChanged(() => IsVisible);
            }
        }

        public bool? IsResourceUnchecked { get; set; }

        public bool IsVisible
        {
            get
            {
                return _isVisible;
            }
            set
            {
                _isVisible = value;
                OnPropertyChanged(() => IsVisible);
            }
        }

        public System.Action SelectAll { get; set; }

        public void SelectItem(Guid id, Action<IExplorerItemViewModel> foundAction)
        {
            foreach (var explorerItemViewModel in Children)
            {
                if (explorerItemViewModel.ResourceId == id)
                {
                    if (!explorerItemViewModel.IsExpanded)
                    {
                        explorerItemViewModel.IsExpanded = true;
                    }
                    explorerItemViewModel.IsSelected = true;
                    foundAction(explorerItemViewModel);
                }
                else
                {
                    explorerItemViewModel.SelectItem(id, foundAction);
                }
            }
        }

        public bool IsSource
        {
            get;
            set;
        }
        public bool IsService
        {
            get;
            set;
        }
        public bool IsFolder
        {
            get;
            set;
        }
        public bool IsReservedService
        {
            get;
            set;
        }
        public bool IsServer
        {
            get;
            set;
        }
        public bool IsResourceVersion
        {
            get;
            set;
        }


        public void SelectItem(string path, Action<IExplorerItemViewModel> foundAction)
        {
            foreach (var explorerItemViewModel in Children)
            {
                explorerItemViewModel.Apply(a =>
                {
                    if (a.ResourcePath == path)
                    {
                        a.IsExpanded = true;
                        foundAction(a);
                    }
                });
            }
        }

        public void SetPropertiesForDialogFromPermissions(IWindowsGroupPermission permissions)
        {
            AllowResourceCheck = false;
            IsResourceChecked = false;
            CanCreateDbSource = permissions.Contribute;
            CanCreateFolder = permissions.Contribute;
            CanCreatePluginSource = permissions.Contribute;
            CanCreateEmailSource = permissions.Contribute;
            CanCreateRabbitMQSource = permissions.Contribute;
            CanCreateExchangeSource = permissions.Contribute;
            CanCreateDropboxSource = permissions.Contribute;
            CanCreateSharePointSource = permissions.Contribute;
            CanCreateServerSource = permissions.Contribute;
            CanCreateWebSource = permissions.Contribute;
            CanCreateWorkflowService = permissions.Contribute;
            CanDelete = false;
            CanDeploy = false;
            CanRename = false;
            CanRollback = false;
            CanShowVersions = false;
            ShowContextMenu = true;
        }

        public void SetPropertiesForDialog()
        {
            if (_isDialog)
            {
                AllowResourceCheck = false;
                IsResourceChecked = false;
                CanCreateDbSource = false;
                CanCreateDropboxSource = false;
                CanCreateEmailSource = false;
                CanCreateRabbitMQSource = false;
                CanCreateExchangeSource = false;
                CanCreateServerSource = false;
                CanCreateSharePointSource = false;
                CanCreatePluginSource = false;
                CanCreateWebSource = false;
                CanCreateWorkflowService = false;
                ShowContextMenu = false;
                CanDeploy = false;
            }
            else
            {
                AllowResourceCheck = false;
                IsResourceChecked = false;
                CanCreateDbSource = true;
                CanCreateFolder = true;
                CanCreatePluginSource = true;
                CanCreateEmailSource = true;
                CanCreateRabbitMQSource = true;
                CanCreateExchangeSource = true;
                CanCreateDropboxSource = true;
                CanCreateSharePointSource = true;
                CanCreateServerSource = true;
                CanCreateWebSource = true;
                CanDelete = false;
                CanDeploy = false;
                CanRename = false;
                CanRollback = false;
                CanShowVersions = false;
                CanCreateWorkflowService = true;
                ShowContextMenu = true;
            }
        }

        public IServer Server { get; set; }

        public ICommand Expand
        {
            get;
            set;
        }

        public ObservableCollection<IExplorerItemViewModel> Children
        {
            get
            {
                if (_children == null) return _children;
                return new AsyncObservableCollection<IExplorerItemViewModel>(_children.Where(a => a.IsVisible));
            }
            set
            {
                _children = new AsyncObservableCollection<IExplorerItemViewModel>(value);
                OnPropertyChanged(() => Children);
                OnPropertyChanged(() => ChildrenCount);
            }
        }

        public IExplorerTreeItem Parent { get; set; }

        public void AddChild(IExplorerItemViewModel child)
        {
            _children.Add(child);
            OnPropertyChanged(() => Children);
        }

        public void RemoveChild(IExplorerItemViewModel child)
        {
            _children.Remove(child);
            OnPropertyChanged(() => Children);
        }

        public string ResourceType { get; set; }
        public string ResourcePath { get; set; }
        public bool CanDrop { get; set; }

        public bool CanDrag
        {
            get
            {
                return _canDrag && (IsServer || ResourceType == "ServerSource") && string.IsNullOrWhiteSpace(ResourcePath);
            }
            set
            {
                _canDrag = value;
                OnPropertyChanged(() => CanDrag);
            }
        }

        public string ResourceName { get; set; }
        public Guid ResourceId { get; set; }

        public bool IsExpanderVisible
        {
            get
            {
                return Children.Count > 0;
            }
            set
            {
            }
        }

        public ICommand NewCommand
        {
            get;
            set;
        }

        public ICommand DeployCommand
        {
            get;
            set;
        }

        public bool CanCreateDbSource { get; set; }

        public bool CanCreateServerSource
        {
            get
            {
                return _canCreateServerSource;
            }
            set
            {
                _canCreateServerSource = value;
                OnPropertyChanged(() => CanCreateServerSource);
            }
        }

        public bool CanCreateWebSource { get; set; }
        public bool CanCreatePluginSource { get; set; }
        public bool CanCreateEmailSource { get; set; }
        public bool CanCreateRabbitMQSource { get; set; }
        public bool CanCreateExchangeSource { get; set; }
        public bool CanCreateDropboxSource { get; set; }
        public bool CanCreateSharePointSource { get; set; }
        public bool CanRename { get; set; }
        public bool CanDelete { get; set; }

        public bool CanCreateFolder
        {
            get
            {
                return Server.Permissions != null && Server.Permissions.Any(a => (a.Contribute || a.Administrator) && a.IsServer);
            }
            set
            {
                if (_canCreateFolder != value)
                {
                    _canCreateFolder = value;
                    OnPropertyChanged(() => CanCreateFolder);
                }
            }
        }

        public bool CanDeploy { get; set; }

        public bool CanShowVersions
        {
            get { return false; }
            set
            {
            }
        }

        public bool CanRollback { get; set; }

        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                _isExpanded = value;
                OnPropertyChanged(() => IsExpanded);
            }
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged(() => IsSelected);
                if (_isSelected)
                {
                    _shellViewModel.SetActiveEnvironment(Server.EnvironmentID);
                    _shellViewModel.SetActiveServer(Server);
                }
            }
        }

        public bool CanShowServerVersion
        {
            get { return _canShowServerVersion; }
            set
            {
                _canShowServerVersion = value;
                OnPropertyChanged(() => CanShowServerVersion);
            }
        }

        public bool AllowResourceCheck
        {
            get
            {
                return _allowResourceCheck;
            }
            set
            {
                _allowResourceCheck = value;
                OnPropertyChanged(() => AllowResourceCheck);
            }
        }

        public bool? IsResourceChecked
        {
            get
            {
                return _isResourceChecked;
            }
            set
            {
                _isResourceChecked = value ?? false;

                OnPropertyChanged(() => IsResourceChecked);
                AsList().Where(o => (o.IsFolder && o.ChildrenCount >= 1) || !o.IsFolder).Apply(a => a.IsResourceUnchecked = value ?? false);
                if (SelectAll != null)
                    SelectAll();
            }
        }

        private void ShowServerVersionAbout()
        {
            ShellViewModel.ShowAboutBox();
        }

        private string GetChildNameFromChildren()
        {
            const string NewFolder = "New Folder";
            int count = 0;
            string folderName = NewFolder;
            while (Children.Any(a => a.ResourceName == folderName))
            {
                count++;
                folderName = NewFolder + " " + count;
            }
            return folderName;
        }

        public ICommand RenameCommand { get; set; }
        public ICommand CreateFolderCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand ShowVersionHistory { get; set; }
        public ICommand RollbackCommand { get; set; }

        public string DisplayName
        {
            get;
            set;
        }

        public bool IsConnected
        {
            get
            {
                return _isConnected;
            }
            private set
            {
                _isConnected = value;
                OnPropertyChanged(() => IsConnected);
            }
        }

        public bool AllowEdit
        {
            get
            {
                return _allowEdit;
            }
            private set
            {
                _allowEdit = value;
                OnPropertyChanged(() => AllowEdit);
            }
        }

        public bool IsLoaded { get; private set; }

        public bool Connect()
        {
            if (Server != null)
            {
                IsConnecting = true;
                IsConnected = false;
                Server.Connect();
                IsConnected = true;
                IsConnecting = false;
                return IsConnected;
            }
            return false;
        }

        public bool IsConnecting
        {
            get
            {
                return _isConnecting;
            }
            set
            {
                _isConnecting = value;
                IsServerIconVisible = !value;
                IsServerUnavailableIconVisible = !value;

                OnPropertyChanged(() => IsConnecting);
            }
        }

        public async Task<bool> Load(bool isDeploy = false)
        {
            if (!IsLoading)
            {
                try
                {
                    IsLoading = true;
                    var result = await LoadDialog(null, isDeploy);
                    return result;
                }
                finally
                {
                    IsLoading = false;
                }
            }
            return false;
        }

        public bool IsLoading
        {
            get
            {
                return _isLoading;
            }
            set
            {
                _isLoading = value;
            }
        }

        public async Task<bool> LoadDialog(string selectedPath, bool isDeploy = false)
        {
            if (IsConnected)
            {
                IsConnecting = true;
                var explorerItems = await Server.LoadExplorer();
                await CreateExplorerItems(explorerItems.Children, Server, this, selectedPath != null, Children.Any(a => AllowResourceCheck));
                IsLoaded = true;
                IsConnecting = false;
                IsExpanded = true;

                return IsLoaded;
            }
            return false;
        }

        public async Task<bool> LoadDialog(Guid selectedPath)
        {
            if (IsConnected)
            {
                IsConnecting = true;
                var explorerItems = await Server.LoadExplorer();
                await CreateExplorerItems(explorerItems.Children, Server, this, selectedPath != Guid.Empty);

                IsLoaded = true;
                IsConnecting = false;
                IsExpanded = true;
                return IsLoaded;
            }
            return false;
        }

        public void Filter(string filter)
        {
            foreach (var explorerItemViewModel in _children)
            {
                explorerItemViewModel.Filter(filter);
            }

            OnPropertyChanged(() => Children);
        }

        public void Filter(Func<IExplorerItemViewModel, bool> filter)
        {
            Children.Apply(a => a.IsVisible = filter(a));
            foreach (var explorerItemViewModel in _children)
            {
                explorerItemViewModel.Filter(filter);
            }
            OnPropertyChanged(() => Children);
        }

        public ICollection<IExplorerItemViewModel> AsList()
        {
            return AsList(Children);
        }

        // ReSharper disable once ParameterTypeCanBeEnumerable.Local
        private ICollection<IExplorerItemViewModel> AsList(ICollection<IExplorerItemViewModel> rootCollection)
        {
            return rootCollection.Union(rootCollection.SelectMany(a => a.AsList())).ToList();
        }

        public void SetItemCheckedState(Guid id, bool state)
        {
            var resource = AsList().FirstOrDefault(a => a.ResourceId == id);
            if (resource != null)
            {
                resource.Checked = state;
            }
        }

        public void RemoveItem(IExplorerItemViewModel vm)
        {
            if (!vm.IsServer)
            {
                var res = AsList(_children).FirstOrDefault(a => a.Children != null && a.Children.Any(b => b.ResourceId == vm.ResourceId));
                if (res != null)
                {
                    res.RemoveChild(res.Children.FirstOrDefault(a => a.ResourceId == vm.ResourceId));
                    OnPropertyChanged(() => Children);
                }
            }
        }

        public ICommand RefreshCommand { get; set; }

        public bool IsServerIconVisible
        {
            get
            {
                return _isServerIconVisible && IsConnected;
            }
            set
            {
                _isServerIconVisible = value;
                OnPropertyChanged(() => IsServerIconVisible);
            }
        }

        public bool IsServerUnavailableIconVisible
        {
            get
            {
                return _isServerUnavailableIconVisible && !IsConnected;
            }
            set
            {
                _isServerUnavailableIconVisible = value;
                OnPropertyChanged(() => IsServerIconVisible);
            }
        }

        // ReSharper disable ParameterTypeCanBeEnumerable.Local
#pragma warning disable 1998

        public async Task<ObservableCollection<IExplorerItemViewModel>> CreateExplorerItems(IList<IExplorerItem> explorerItems, IServer server, IExplorerTreeItem parent, bool isDialog = false, bool isDeploy = false)
#pragma warning restore 1998
        // ReSharper restore ParameterTypeCanBeEnumerable.Local
        {
            if (explorerItems == null) return null;
            var explorerItemModels = new ObservableCollection<IExplorerItemViewModel>();
            if (parent != null)
            {
                parent.Children = new AsyncObservableCollection<IExplorerItemViewModel>();
            }
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var explorerItem in explorerItems)
            {
                var itemCreated = new ExplorerItemViewModel(server, parent, a => { SelectAction(a); }, _shellViewModel, _controller)
                {
                    ResourceName = explorerItem.DisplayName,
                    ResourceId = explorerItem.ResourceId,
                    ResourceType = explorerItem.ResourceType,
                    ResourcePath = explorerItem.ResourcePath,
                    AllowResourceCheck = isDeploy,
                    ShowContextMenu = !isDeploy,
                    IsService =  explorerItem.IsService,
                    IsFolder = explorerItem.IsFolder,
                    IsSource = explorerItem.IsSource,
                    IsReservedService = explorerItem.IsReservedService,
                    IsResourceVersion = explorerItem.IsResourceVersion,
                    IsServer = explorerItem.IsServer
                    //Inputs = explorerItem.Inputs,
                    //Outputs = explorerItem.Outputs
                };
                if (isDeploy)
                {
                    itemCreated.CanExecute = false;
                    itemCreated.CanEdit = false;
                    itemCreated.CanView = false;
                }
                itemCreated.SetPermissions(server.Permissions, isDeploy);
                if (isDialog)
                {
                    SetPropertiesForDialog(itemCreated);
                }

                CreateExplorerItemsSync(explorerItem.Children, server, itemCreated, isDialog, isDeploy);
                //itemCreated.Children = CreateExplorerItems(explorerItem.Children, server, itemCreated, isDialog);
                explorerItemModels.Add(itemCreated);
            }
            if (parent != null)
            {
                var col = parent.Children as AsyncObservableCollection<IExplorerItemViewModel>;
                col.AddRange(explorerItemModels);
                parent.Children = col;
            }
            if (isDeploy)
            {
                ShowContextMenu = false;
            }
            //return explorerItemModels;
            return null;
        }

        // ReSharper disable ParameterTypeCanBeEnumerable.Local
        private void CreateExplorerItemsSync(IList<IExplorerItem> explorerItems, IServer server, IExplorerTreeItem parent, bool isDialog = false, bool isDeploy = false)
        // ReSharper restore ParameterTypeCanBeEnumerable.Local
        {
            if (explorerItems == null) return;
            var explorerItemModels = new ObservableCollection<IExplorerItemViewModel>();
            if (parent != null)
            {
                parent.Children = new AsyncObservableCollection<IExplorerItemViewModel>();
            }
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var explorerItem in explorerItems)
            {
                var itemCreated = new ExplorerItemViewModel(server, parent, a => { SelectAction(a); }, _shellViewModel, _controller)
                {
                    ResourceName = explorerItem.DisplayName,
                    ResourceId = explorerItem.ResourceId,
                    ResourceType = explorerItem.ResourceType,
                    ResourcePath = explorerItem.ResourcePath,
                    AllowResourceCheck = isDeploy,
                    ShowContextMenu = !isDeploy,
                    IsService = explorerItem.IsService,
                    IsFolder = explorerItem.IsFolder,
                    IsSource = explorerItem.IsSource,
                    IsReservedService = explorerItem.IsReservedService,
                    IsResourceVersion = explorerItem.IsResourceVersion,
                    IsServer = explorerItem.IsServer
                    //Inputs = explorerItem.Inputs,
                    //Outputs = explorerItem.Outputs
                };
                if (isDeploy)
                {
                    itemCreated.CanExecute = false;
                    itemCreated.CanView = false;
                    itemCreated.CanEdit = false;
                }
                itemCreated.SetPermissions(server.Permissions, isDeploy);
                if (isDialog)
                {
                    SetPropertiesForDialog(itemCreated);
                }

                CreateExplorerItemsSync(explorerItem.Children, server, itemCreated, isDialog, isDeploy);
                //itemCreated.Children = CreateExplorerItems(explorerItem.Children, server, itemCreated, isDialog);
                explorerItemModels.Add(itemCreated);
            }
            if (parent != null)
            {
                var col = parent.Children as AsyncObservableCollection<IExplorerItemViewModel>;
                col.AddRange(explorerItemModels);
                parent.Children = col;
            }
            if (isDeploy)
            {
                ShowContextMenu = false;
            }
            //return explorerItemModels;
        }

        private static void SetPropertiesForDialog(IExplorerItemViewModel itemCreated)
        {
            itemCreated.AllowResourceCheck = false;
            itemCreated.IsResourceChecked = false;
            itemCreated.CanCreateDbSource = false;
            itemCreated.CanCreatePluginSource = false;
            itemCreated.CanCreateEmailSource = false;
            itemCreated.CanCreateRabbitMQSource = false;
            itemCreated.CanCreateDropboxSource = false;
            itemCreated.CanCreateSharePointSource = false;
            itemCreated.CanCreateServerSource = false;
            itemCreated.CanCreateWebSource = false;
            itemCreated.CanCreateWorkflowService = false;
            itemCreated.ShowContextMenu = false;
            itemCreated.CanDeploy = false;
            itemCreated.CanShowVersions = false;
            itemCreated.CanEdit = false;
            itemCreated.CanView = false;
            itemCreated.CanExecute = false;
            itemCreated.CanShowDependencies = false;
            itemCreated.CanDrag = false;
            itemCreated.CanDrop = false;
            itemCreated.CanRename = false;
        }

        public void Dispose()
        {
            if (Children != null)
                foreach (var explorerItemViewModel in _children)
                {
                    if (explorerItemViewModel != null) explorerItemViewModel.Dispose();
                }
        }
    }
}