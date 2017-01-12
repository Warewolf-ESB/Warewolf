using Caliburn.Micro;
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
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Warewolf.Studio.Core;
// ReSharper disable InconsistentNaming
// ReSharper disable ValueParameterNotUsed

namespace Warewolf.Studio.ViewModels
{
    public class EnvironmentViewModel : BindableBase, IEnvironmentViewModel
    {
        private ObservableCollection<IExplorerItemViewModel> _children;
        private bool _isConnecting;
        private bool _isConnected;
        private bool _isServerIconVisible;
        private bool _isServerUnavailableIconVisible;
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
        private bool _canDrag;
        private bool _forcedRefresh;
        private bool _isResourceCheckedEnabled;
        private string _deployResourceCheckboxTooltip;
        private bool? _isResource;
        private string _filter;
        private string _newServiceTooltip;
        private string _newServerSourceTooltip;
        private string _newSqlServerSourceTooltip;
        private string _newMySqlSourceTooltip;
        private string _newPostgreSqlSourceTooltip;
        private string _newOracleSourceTooltip;
        private string _newOdbcSourceTooltip;
        private string _newWebSourceTooltip;
        private string _newPluginSourceTooltip;
        private string _newComPluginSourceTooltip;
        private string _newEmailSourceTooltip;
        private string _newExchangeSourceTooltip;
        private string _newRabbitMqSourceTooltip;
        private string _newDropboxSourceTooltip;
        private string _newSharepointSourceTooltip;
        private bool _canCreateSource;
        private string _newFolderTooltip;
        private bool _canViewApisJson;
        private string _viewApisJsonTooltip;
        private string _serverVersionTooltip;
        private bool _canDeploy;
        private string _newWcfSourceTooltip;
        private bool _isSaveDialog;

        public EnvironmentViewModel(IServer server, IShellViewModel shellViewModel, bool isDialog = false, Action<IExplorerItemViewModel> selectAction = null)
        {            
            if (server == null)
                throw new ArgumentNullException(nameof(server));
            if (shellViewModel == null)
                throw new ArgumentNullException(nameof(shellViewModel));
            _shellViewModel = shellViewModel;
            _isDialog = isDialog;
            _controller = CustomContainer.Get<IPopupController>();
            Server = server;
            _children = new ObservableCollection<IExplorerItemViewModel>();

            NewServiceCommand = new DelegateCommand(() =>
            {
                UpdateActiveEnvironment(shellViewModel);
                shellViewModel.NewService(ResourcePath);
            });

            NewServerCommand = new DelegateCommand(() =>
            {
                UpdateActiveEnvironment(shellViewModel);
                shellViewModel.NewServerSource(ResourcePath);
            });

            NewSqlServerSourceCommand = new DelegateCommand(() =>
            {
                UpdateActiveEnvironment(shellViewModel);
                shellViewModel.NewSqlServerSource(ResourcePath);
            });

            NewMySqlSourceCommand = new DelegateCommand(() =>
            {
                UpdateActiveEnvironment(shellViewModel);
                shellViewModel.NewMySqlSource(ResourcePath);
            });

            NewPostgreSqlSourceCommand = new DelegateCommand(() =>
            {
                UpdateActiveEnvironment(shellViewModel);
                shellViewModel.NewPostgreSqlSource(ResourcePath);
            });

            NewOracleSourceCommand = new DelegateCommand(() =>
            {
                UpdateActiveEnvironment(shellViewModel);
                shellViewModel.NewOracleSource(ResourcePath);
            });

            NewOdbcSourceCommand = new DelegateCommand(() =>
            {
                UpdateActiveEnvironment(shellViewModel);
                shellViewModel.NewOdbcSource(ResourcePath);
            });

            NewPluginSourceCommand = new DelegateCommand(() =>
            {
                UpdateActiveEnvironment(shellViewModel);
                shellViewModel.NewPluginSource(ResourcePath);
            });
            NewComPluginSourceCommand = new DelegateCommand(() =>
            {
                UpdateActiveEnvironment(shellViewModel);
                shellViewModel.NewComPluginSource(ResourcePath);
            });
            NewWcfSourceCommand = new DelegateCommand(() =>
            {
                UpdateActiveEnvironment(shellViewModel);
                shellViewModel.NewWcfSource(ResourcePath);
            });

            NewWebSourceSourceCommand = new DelegateCommand(() =>
            {
                UpdateActiveEnvironment(shellViewModel);
                shellViewModel.NewWebSource(ResourcePath);
            });

            NewEmailSourceSourceCommand = new DelegateCommand(() =>
            {
                UpdateActiveEnvironment(shellViewModel);
                shellViewModel.NewEmailSource(ResourcePath);
            });

            NewExchangeSourceSourceCommand = new DelegateCommand(() =>
            {
                UpdateActiveEnvironment(shellViewModel);
                shellViewModel.NewExchangeSource(ResourcePath);
            });

            NewRabbitMQSourceSourceCommand = new DelegateCommand(() =>
            {
                UpdateActiveEnvironment(shellViewModel);
                shellViewModel.NewRabbitMQSource(ResourcePath);
            });

            NewSharepointSourceSourceCommand = new DelegateCommand(() =>
            {
                UpdateActiveEnvironment(shellViewModel);
                shellViewModel.NewSharepointSource(ResourcePath);
            });           

            NewDropboxSourceSourceCommand = new DelegateCommand(() =>
            {
                UpdateActiveEnvironment(shellViewModel);
                shellViewModel.NewDropboxSource(ResourcePath);
            });

            ViewApisJsonCommand = new DelegateCommand(() =>
            {
                var environmentModel = EnvironmentRepository.Instance.FindSingle(model => model.ID == Server.EnvironmentID);
                shellViewModel.ViewApisJson(ResourcePath, environmentModel.Connection.WebServerUri);
            });

            DeployCommand = new DelegateCommand(() =>
            {
                shellViewModel.AddDeploySurface(AsList().Union<IExplorerTreeItem>(new [] { this }));
            });

            DisplayName = server.ResourceName;
            RefreshCommand = new DelegateCommand(async () =>
            {
                await Refresh();
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
            
            server.NetworkStateChanged += (args, server1) =>
             {
                 if (args.State == ConnectionNetworkState.Connected)
                 {
                     Application.Current.Dispatcher.Invoke(async () =>
                     {
                         await Refresh();
                     },DispatcherPriority.Background);
                     
                 }
             };
            
            AllowEdit = server.AllowEdit;
            ShowServerVersionCommand = new DelegateCommand(ShowServerVersionAbout);
            CanCreateFolder = Server.UserPermissions == Permissions.Administrator || server.UserPermissions == Permissions.Contribute;
            CanDeploy = Server.UserPermissions == Permissions.Administrator || server.UserPermissions == Permissions.Contribute;
            CreateFolderCommand = new DelegateCommand(CreateFolder);
            Parent = null;
            ResourceType = @"ServerSource";
            ResourcePath = string.Empty;
            ResourceName = DisplayName;
            CanShowServerVersion = true;
            AreVersionsVisible = false;
            IsVisible = true;
            SetPropertiesForDialogFromPermissions(new WindowsGroupPermission());
            SelectAll = () => { };
            CanDrag = false;
            CanDrop = false;
            ResourceId = server.EnvironmentID;
            CanViewApisJson = true;
            if (ForcedRefresh)
                ForcedRefresh = true;
        }

        private async Task Refresh()
        {
            if (Children.Any(a => a.AllowResourceCheck))
            {
                await Load(true,true);
                ShowContextMenu = false;
            }
            else
            {
                await Load(false,true);
            }
        }

        private void UpdateActiveEnvironment(IShellViewModel shellViewModel)
        {
            shellViewModel.SetActiveEnvironment(Server.EnvironmentID);
            shellViewModel.SetActiveServer(Server);
        }

        public IShellViewModel ShellViewModel => _shellViewModel;

        public int ChildrenCount => GetChildrenCount();

        private int GetChildrenCount()
        {
            int total = 0;
            if (Children != null)
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
                NewServiceTooltip = _canCreateWorkflowService ? Resources.Languages.Tooltips.NewServiceTooltip : Resources.Languages.Tooltips.NoPermissionsToolTip;
                OnPropertyChanged(() => CanCreateWorkflowService);
            }
        }

        public bool IsSaveDialog
        {
            get { return _isSaveDialog; }
            set
            {
                _isSaveDialog = value;
                OnPropertyChanged(() => IsSaveDialog);
            }
        }

        public bool AreVersionsVisible { get; set; }

        public void CreateFolder()
        {
            IsExpanded = true;
            var id = Guid.NewGuid();
            var name = GetChildNameFromChildren();
            // ReSharper disable once UseObjectOrCollectionInitializer
            var child = new ExplorerItemViewModel(Server, this, a => { SelectAction(a); }, _shellViewModel, _controller)
            {
                ResourceId = id,
                ResourceType = "Folder",
                IsFolder = true,
                ResourcePath = name,
                IsSelected = true,
                IsRenaming = true,
                IsVisible = true,
                IsNewFolder = true,
                CanCreateWorkflowService = true,
                CanView = true,
                CanDeploy = true,
                CanViewApisJson = true
            };
            child.ResourceName = name;
            child.IsRenaming = true;

            if (_isDialog)
            {
                child.AllowResourceCheck = false;
                child.IsResourceChecked = false;
                child.CanCreateSource = false;
                child.CanCreateWorkflowService = false;
                child.ShowContextMenu = false;
                child.CanDeploy = false;
                child.CanShowDependencies = false;
            }
            else
            {
                var permissions = Server.GetPermissions(ResourceId);
                child.SetPermissions(permissions);

                child.CanRename = true;
                child.CanDelete = true;
                child.IsSaveDialog = IsSaveDialog;
                child.ShowContextMenu = ShowContextMenu;
            }
            AddChild(child);
        }

        public Action<IExplorerItemViewModel> SelectAction { get; set; }

        public bool? IsFolderChecked
        {
            get
            {
                return _isResource;
            }
            set
            {
                if (Children.Any() && Children.All(a => a.IsResourceChecked.HasValue && a.IsResourceChecked.Value))
                {
                    _isResource = true;
                }
                else if (Children.All(a => a.IsResourceChecked.HasValue && !a.IsResourceChecked.Value))
                {
                    _isResource = false;
                }
                else
                {
                    _isResource = null;
                }                
                OnPropertyChanged(() => IsResourceChecked);
            }
        }

        public bool? IsResourceUnchecked { get; set; }

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

        public bool IsSource { get; set; }
        public bool IsService { get; set; }
        public bool IsFolder { get; set; }
        public bool IsReservedService { get; set; }
        public bool IsServer { get; set; }
        public bool IsResourceVersion { get; set; }
        public bool CanViewApisJson
        {
            get { return _canViewApisJson; }
            set
            {
                _canViewApisJson = value;
                ViewApisJsonTooltip = _canViewApisJson ? Resources.Languages.Tooltips.ViewApisJsonTooltip : Resources.Languages.Tooltips.NoPermissionsToolTip;
                OnPropertyChanged(() => CanViewApisJson);
            }
        }

        public void SelectItem(string path, Action<IExplorerItemViewModel> foundAction)
        {
            foreach (var explorerItemViewModel in Children)
            {
                explorerItemViewModel.Apply(a =>
                {
                    if (a.ResourcePath.Replace("\\", "\\\\") == path)
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
            CanCreateSource = permissions.Contribute;
            CanCreateFolder = permissions.Contribute;
            CanCreateWorkflowService = permissions.Contribute;
            CanDelete = false;
            CanDeploy = permissions.DeployFrom;
            CanRename = false;
            CanRollback = false;
            CanShowVersions = false;
            ShowContextMenu = true;
        }

        public void UpdateChildrenCount()
        {
            OnPropertyChanged(()=>ChildrenCount);
        }

        public void SetPropertiesForDialog()
        {
            AllowResourceCheck = false;
            IsResourceChecked = false;
            CanDeploy = false;
            CanCreateSource = false;
            CanCreateWorkflowService = false;
            ShowContextMenu = false;
            if (!_isDialog)
            {
                CanCreateSource = true;
                CanCreateFolder = true;
                CanDelete = false;
                CanRename = false;
                CanRollback = false;
                CanShowVersions = false;
                CanCreateWorkflowService = true;
                ShowContextMenu = true;
            }
        }

        public IServer Server { get; set; }

        public ObservableCollection<IExplorerItemViewModel> UnfilteredChildren { get; set; }

        public ObservableCollection<IExplorerItemViewModel> Children
        {
            get
            {
                if (_children == null)
                {
                    return new AsyncObservableCollection<IExplorerItemViewModel>();
                }
                var orderedCollection = _children.OrderByDescending(a => a.IsFolder).ThenBy(b => b.ResourceName).ToObservableCollection();
                UnfilteredChildren = orderedCollection;
                return new AsyncObservableCollection<IExplorerItemViewModel>(orderedCollection.Where(a => a.IsVisible));
            }
            set
            {
                _children = value;
                OnPropertyChanged(() => Children);
                OnPropertyChanged(() => ChildrenCount);
            }
        }

        public IExplorerTreeItem Parent { get; set; }

        public void AddChild(IExplorerItemViewModel child)
        {
            var tempChildren = new ObservableCollection<IExplorerItemViewModel>(_children);
            var exists = tempChildren.FirstOrDefault(model => model.ResourceName.Equals(child.ResourceName, StringComparison.InvariantCultureIgnoreCase));
            if (exists != null)
            {
                foreach (var explorerItemViewModel in child.Children)
                {
                    exists.AddChild(explorerItemViewModel);
                }
            }
            else
            {
                tempChildren.Insert(0, child);
            }
            _children = tempChildren;
            OnPropertyChanged(() => Children);
            OnPropertyChanged(() => ChildrenCount);
        }

        public void RemoveChild(IExplorerItemViewModel child)
        {
            var tempChildren = new ObservableCollection<IExplorerItemViewModel>(_children);
            tempChildren.Remove(child);
            _children = tempChildren;
            OnPropertyChanged(() => Children);
            OnPropertyChanged(() => ChildrenCount);
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

        public string NewServiceTooltip
        {
            get { return _newServiceTooltip; }
            set
            {
                _newServiceTooltip = value;
                OnPropertyChanged(() => NewServiceTooltip);
            }
        }
        public string NewServerSourceTooltip
        {
            get { return _newServerSourceTooltip; }
            set
            {
                _newServerSourceTooltip = value;
                OnPropertyChanged(() => NewServerSourceTooltip);
            }
        }
        public string NewSqlServerSourceTooltip
        {
            get { return _newSqlServerSourceTooltip; }
            set
            {
                _newSqlServerSourceTooltip = value;
                OnPropertyChanged(() => NewSqlServerSourceTooltip);
            }
        }
        public string NewMySqlSourceTooltip
        {
            get { return _newMySqlSourceTooltip; }
            set
            {
                _newMySqlSourceTooltip = value;
                OnPropertyChanged(() => NewMySqlSourceTooltip);
            }
        }
        public string NewPostgreSqlSourceTooltip
        {
            get { return _newPostgreSqlSourceTooltip; }
            set
            {
                _newPostgreSqlSourceTooltip = value;
                OnPropertyChanged(() => NewPostgreSqlSourceTooltip);
            }
        }
        public string NewOracleSourceTooltip
        {
            get { return _newOracleSourceTooltip; }
            set
            {
                _newOracleSourceTooltip = value;
                OnPropertyChanged(() => NewOracleSourceTooltip);
            }
        }
        public string NewOdbcSourceTooltip
        {
            get { return _newOdbcSourceTooltip; }
            set
            {
                _newOdbcSourceTooltip = value;
                OnPropertyChanged(() => NewOdbcSourceTooltip);
            }
        }
        public string NewWebSourceTooltip
        {
            get { return _newWebSourceTooltip; }
            set
            {
                _newWebSourceTooltip = value;
                OnPropertyChanged(() => NewWebSourceTooltip);
            }
        }
        public string NewPluginSourceTooltip
        {
            get { return _newPluginSourceTooltip; }
            set
            {
                _newPluginSourceTooltip = value;
                OnPropertyChanged(() => NewPluginSourceTooltip);
            }
        }
        public string NewComPluginSourceTooltip
        {
            get { return _newComPluginSourceTooltip; }
            set
            {
                _newComPluginSourceTooltip = value;
                OnPropertyChanged(() => NewComPluginSourceTooltip);
            }
        }
        public string NewWcfSourceTooltip
        {
            get { return _newWcfSourceTooltip; }
            set
            {
                _newWcfSourceTooltip = value;
                OnPropertyChanged(() => NewWcfSourceTooltip);
            }
        }
        public string NewEmailSourceTooltip
        {
            get { return _newEmailSourceTooltip; }
            set
            {
                _newEmailSourceTooltip = value;
                OnPropertyChanged(() => NewEmailSourceTooltip);
            }
        }
        public string NewExchangeSourceTooltip
        {
            get { return _newExchangeSourceTooltip; }
            set
            {
                _newExchangeSourceTooltip = value;
                OnPropertyChanged(() => NewExchangeSourceTooltip);
            }
        }
        public string NewRabbitMqSourceTooltip
        {
            get { return _newRabbitMqSourceTooltip; }
            set
            {
                _newRabbitMqSourceTooltip = value;
                OnPropertyChanged(() => NewRabbitMqSourceTooltip);
            }
        }
        public string NewDropboxSourceTooltip
        {
            get { return _newDropboxSourceTooltip; }
            set
            {
                _newDropboxSourceTooltip = value;
                OnPropertyChanged(() => NewDropboxSourceTooltip);
            }
        }
        public string NewSharepointSourceTooltip
        {
            get { return _newSharepointSourceTooltip; }
            set
            {
                _newSharepointSourceTooltip = value;
                OnPropertyChanged(() => NewSharepointSourceTooltip);
            }
        }
        public string NewFolderTooltip
        {
            get { return _newFolderTooltip; }
            set
            {
                _newFolderTooltip = value;
                OnPropertyChanged(() => NewFolderTooltip);
            }
        }
        public string ViewApisJsonTooltip
        {
            get { return _viewApisJsonTooltip; }
            set
            {
                _viewApisJsonTooltip = value;
                OnPropertyChanged(() => ViewApisJsonTooltip);
            }
        }

        public string ResourceName { get; set; }
        public Guid ResourceId { get; set; }

        public bool IsExpanderVisible
        {
            get { return Children.Count > 0; }
            set { }
        }

        public bool CanCreateSource
        {
            get { return _canCreateSource; }
            set
            {
                _canCreateSource = value;

                NewServerSourceTooltip = _canCreateSource ? Resources.Languages.Tooltips.NewServerSourceTooltip : Resources.Languages.Tooltips.NoPermissionsToolTip;
                NewSqlServerSourceTooltip = _canCreateSource ? Resources.Languages.Tooltips.NewSqlServerSourceTooltip : Resources.Languages.Tooltips.NoPermissionsToolTip;
                NewMySqlSourceTooltip = _canCreateSource ? Resources.Languages.Tooltips.NewMySqlSourceTooltip : Resources.Languages.Tooltips.NoPermissionsToolTip;
                NewPostgreSqlSourceTooltip = _canCreateSource ? Resources.Languages.Tooltips.NewPostgreSqlSourceTooltip : Resources.Languages.Tooltips.NoPermissionsToolTip;
                NewOracleSourceTooltip = _canCreateSource ? Resources.Languages.Tooltips.NewOracleSourceTooltip : Resources.Languages.Tooltips.NoPermissionsToolTip;
                NewOdbcSourceTooltip = _canCreateSource ? Resources.Languages.Tooltips.NewOdbcSourceTooltip : Resources.Languages.Tooltips.NoPermissionsToolTip;
                NewWebSourceTooltip = _canCreateSource ? Resources.Languages.Tooltips.NewWebSourceTooltip : Resources.Languages.Tooltips.NoPermissionsToolTip;
                NewPluginSourceTooltip = _canCreateSource ? Resources.Languages.Tooltips.NewPluginSourceTooltip : Resources.Languages.Tooltips.NoPermissionsToolTip;
                NewComPluginSourceTooltip = _canCreateSource ? Resources.Languages.Tooltips.NewComPluginSourceTooltip : Resources.Languages.Tooltips.NoPermissionsToolTip;
                NewEmailSourceTooltip = _canCreateSource ? Resources.Languages.Tooltips.NewEmailSourceTooltip : Resources.Languages.Tooltips.NoPermissionsToolTip;
                NewExchangeSourceTooltip = _canCreateSource ? Resources.Languages.Tooltips.NewExchangeSourceTooltip : Resources.Languages.Tooltips.NoPermissionsToolTip;
                NewRabbitMqSourceTooltip = _canCreateSource ? Resources.Languages.Tooltips.NewRabbitMqSourceTooltip : Resources.Languages.Tooltips.NoPermissionsToolTip;
                NewDropboxSourceTooltip = _canCreateSource ? Resources.Languages.Tooltips.NewDropboxSourceTooltip : Resources.Languages.Tooltips.NoPermissionsToolTip;
                NewSharepointSourceTooltip = _canCreateSource ? Resources.Languages.Tooltips.NewSharepointSourceTooltip : Resources.Languages.Tooltips.NoPermissionsToolTip;

                OnPropertyChanged(() => CanCreateSource);
            }
        }
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
                NewFolderTooltip = _canCreateFolder ? Resources.Languages.Tooltips.NewFolderTooltip : Resources.Languages.Tooltips.NoPermissionsToolTip;
            }
        }

        public bool CanDeploy
        {
            get { return _canDeploy; }
            set
            {
                _canDeploy = value;
                OnPropertyChanged(() => CanDeploy);
            }
        }

        public bool CanShowVersions
        {
            get { return false; }
            set { }
        }

        public bool CanRollback { get; set; }

        public bool IsExpanded
        {
            get
            {
                if (ForcedRefresh && _isExpanded)
                {
                    return true;
                }
                return _isExpanded;
            }
            set
            {
                _isExpanded = value;
                OnPropertyChanged(() => IsExpanded);
            }
        }

        public bool ForcedRefresh
        {
            get { return _forcedRefresh; }
            set
            {
                _forcedRefresh = value;
                OnPropertyChanged(() => ForcedRefresh);
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
                ServerVersionTooltip = _canShowServerVersion ? Resources.Languages.Tooltips.ServerVersionTooltip : Resources.Languages.Tooltips.NoPermissionsToolTip;
                OnPropertyChanged(() => CanShowServerVersion);
            }
        }

        public string ServerVersionTooltip
        {
            get { return _serverVersionTooltip; }
            set
            {
                _serverVersionTooltip = value;
                OnPropertyChanged(() => ServerVersionTooltip);
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
                AsList().Where(o => (o.IsFolder && o.ChildrenCount >= 1) || !o.IsFolder).Apply(a => a.IsResourceChecked = _isResourceChecked);
                SelectAll?.Invoke();
                IsResourceCheckedEnabled = true;
                OnPropertyChanged(() => IsResourceCheckedEnabled);
            }
        }

        public bool IsResourceCheckedEnabled
        {
            get { return _isResourceCheckedEnabled; }
            set
            {
                DeployResourceCheckboxTooltip = Resources.Languages.Core.DeployResourceCheckbox;
                if (!value)
                {
                    DeployResourceCheckboxTooltip = Resources.Languages.Core.DeployResourceCheckboxViewPermissionError;
                }
                _isResourceCheckedEnabled = value;
                OnPropertyChanged(() => IsResourceCheckedEnabled);
                OnPropertyChanged(() => DeployResourceCheckboxTooltip);
            }
        }

        public string DeployResourceCheckboxTooltip
        {
            get { return _deployResourceCheckboxTooltip; }
            set
            {
                _deployResourceCheckboxTooltip = value;
                OnPropertyChanged(() => DeployResourceCheckboxTooltip);
            }
        }

        private void ShowServerVersionAbout()
        {
            ShellViewModel.ShowAboutBox();
        }

        private string GetChildNameFromChildren()
        {
            int count = 0;
            string folderName = Resources.Languages.Core.NewFolderLabel;
            while (UnfilteredChildren != null && UnfilteredChildren.Any(a => a.ResourceName == folderName))
            {
                count++;
                folderName = Resources.Languages.Core.NewFolderLabel + " " + count;
            }
            return folderName;
        }

        #region COMMANDS

        public ICommand NewServiceCommand { get; set; }
        public ICommand NewServerCommand { get; set; }
        public ICommand NewSqlServerSourceCommand { get; set; }
        public ICommand NewMySqlSourceCommand { get; set; }
        public ICommand NewPostgreSqlSourceCommand { get; set; }
        public ICommand NewOracleSourceCommand { get; set; }
        public ICommand NewOdbcSourceCommand { get; set; }
        public ICommand NewPluginSourceCommand { get; set; }
        public ICommand NewComPluginSourceCommand { get; set; }
        public ICommand NewWcfSourceCommand { get; set; }
        public ICommand NewWebSourceSourceCommand { get; set; }
        public ICommand NewEmailSourceSourceCommand { get; set; }
        public ICommand NewExchangeSourceSourceCommand { get; set; }
        public ICommand NewRabbitMQSourceSourceCommand { get; set; }
        public ICommand NewSharepointSourceSourceCommand { get; set; }
        public ICommand NewDropboxSourceSourceCommand { get; set; }
        public ICommand DeployCommand { get; set; }
        public ICommand RenameCommand { get; set; }
        public ICommand CreateFolderCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand ShowVersionHistory { get; set; }
        public ICommand RollbackCommand { get; set; }
        public ICommand ShowServerVersionCommand { get; set; }
        public ICommand Expand { get; set; }
        public ICommand RefreshCommand { get; set; }
        public ICommand ViewApisJsonCommand { get; set; }

        #endregion

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
                IsConnected = Server.IsConnected;
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

        public async Task<bool> Load(bool isDeploy = false,bool reloadCatalogue=false)
        {
            if (!IsLoading || isDeploy)
            {
                try
                {
                    IsLoading = true;
                    var result = await LoadDialog(null, isDeploy, reloadCatalogue);
                    return result;
                }
                finally
                {
                    IsLoading = false;
                }
            }
            return false;
        }

        public bool IsLoading { get; set; }

        public async Task<bool> LoadDialog(string selectedPath, bool isDeploy = false, bool reloadCatalogue = false)
        {
            if (IsConnected && Server.IsConnected)
            {
                IsConnecting = true;
                var explorerItems = await Server.LoadExplorer(reloadCatalogue);
                if(explorerItems != null)
                {
                    CreateExplorerItemsSync(explorerItems.Children, Server, this, selectedPath != null, Children.Any(a => AllowResourceCheck));
                }
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

                CreateExplorerItemsSync(explorerItems.Children, Server, this, selectedPath != Guid.Empty);
                IsLoaded = true;
                IsConnecting = false;
                IsExpanded = true;
                return IsLoaded;
            }
            return false;
        }

        public IExplorerTreeItem FindByPath(string path)
        {
            var allChildren = Children.Flatten(model => model.Children);
            if (path.EndsWith("\\"))
            {
                path = path.TrimEnd('\\');
            }
            if (path.StartsWith("\\"))
            {
                path = path.TrimStart('\\');
            }
            var found = allChildren.FirstOrDefault(model => model.ResourcePath == path);
            if (found != null)
            {
                return found;
            }
            return this;            
        }

        public void Filter(string filter)
        {
            _filter = filter;
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
        public void CreateExplorerItemsSync(IList<IExplorerItem> explorerItems, IServer server, IExplorerTreeItem parent, bool isDialog = false, bool isDeploy = false)
        // ReSharper restore ParameterTypeCanBeEnumerable.Local
        {
            if (explorerItems == null) return;
            var explorerItemModels = CreateExplorerItemModels(explorerItems, server, parent, isDialog, isDeploy);
            if (parent != null)
            {
                parent.Children = explorerItemModels;
            }
            if (isDeploy)
            {
                ShowContextMenu = false;
            }
        }

        public ObservableCollection<IExplorerItemViewModel> CreateExplorerItemModels(IEnumerable<IExplorerItem> explorerItems, IServer server, IExplorerTreeItem parent, bool isDialog, bool isDeploy)
        {
            var explorerItemModels = new ObservableCollection<IExplorerItemViewModel>();
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach(var explorerItem in explorerItems)
            {
                var existingItem = parent?.Children?.FirstOrDefault(model => model.ResourcePath.ToLowerInvariant() == explorerItem.ResourcePath.ToLower());
                if(existingItem != null)
                {
                    existingItem.SetPermissions(explorerItem.Permissions, isDeploy);
                    CreateExplorerItemsSync(explorerItem.Children, server, existingItem, isDialog, isDeploy);
                    explorerItemModels.Add(existingItem);
                }
                else
                {
                    var itemCreated = CreateExplorerItem(server, parent, isDialog, isDeploy, explorerItem);
                    CreateExplorerItemsSync(explorerItem.Children, server, itemCreated, isDialog, isDeploy);
                    explorerItemModels.Add(itemCreated);
                }
            }
            return explorerItemModels;
        }

        private ExplorerItemViewModel CreateExplorerItem(IServer server, IExplorerTreeItem parent, bool isDialog, bool isDeploy, IExplorerItem explorerItem)
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
                IsServer = explorerItem.IsServer
            };
            if (isDeploy)
            {
                itemCreated.CanExecute = false;
                itemCreated.CanView = false;
                itemCreated.CanEdit = false;
            }
            itemCreated.SetPermissions(explorerItem.Permissions, isDeploy);
            if (isDialog)
            {
                SetPropertiesForDialog(itemCreated);
            }
            return itemCreated;
        }


        public ExplorerItemViewModel CreateExplorerItemFromResource(IServer server, IExplorerTreeItem parent, bool isDialog, bool isDeploy, IContextualResourceModel explorerItem)
        {
            var itemCreated = new ExplorerItemViewModel(server, parent, a => { SelectAction(a); }, _shellViewModel, _controller)
            {
                ResourceName = explorerItem.ResourceName,
                ResourceId = explorerItem.ID,
                ResourceType = explorerItem.ServerResourceType,
                ResourcePath = explorerItem.GetSavePath(),
                AllowResourceCheck = isDeploy,
                ShowContextMenu = !isDeploy,
                IsFolder = false,
                IsService = explorerItem.ResourceType == Dev2.Studio.Core.AppResources.Enums.ResourceType.WorkflowService,
                IsSource = explorerItem.ResourceType == Dev2.Studio.Core.AppResources.Enums.ResourceType.Source,
                IsServer = explorerItem.ResourceType == Dev2.Studio.Core.AppResources.Enums.ResourceType.Server
            };

            if (string.IsNullOrWhiteSpace(itemCreated.ResourcePath))
            {
                itemCreated.ResourcePath = itemCreated.ResourceName;
            }

            if (isDeploy)
            {
                itemCreated.CanExecute = false;
                itemCreated.CanView = false;
                itemCreated.CanEdit = false;
            }
            itemCreated.SetPermissions(explorerItem.UserPermissions, isDeploy);
            if (isDialog)
            {
                SetPropertiesForDialog(itemCreated);
            }
            return itemCreated;
        }

        private static void SetPropertiesForDialog(IExplorerItemViewModel itemCreated)
        {
            itemCreated.AllowResourceCheck = false;
            itemCreated.IsResourceChecked = false;
            itemCreated.CanCreateSource = false;
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
                    explorerItemViewModel?.Dispose();
                }
        }

        
        public bool Equals(IExplorerTreeItem x, IExplorerTreeItem y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return string.Equals(x.ResourcePath, y.ResourcePath) && x.ResourceId.Equals(y.ResourceId);
        }

        public int GetHashCode(IExplorerTreeItem obj)
        {
            unchecked
            {
                return ((obj.ResourcePath?.GetHashCode() ?? 0) * 397) ^ obj.ResourceId.GetHashCode();
            }
        }
    }
}