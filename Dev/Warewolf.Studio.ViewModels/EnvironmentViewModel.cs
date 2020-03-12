#pragma warning disable S104, CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001, RECS0061, RECS0063, RECS0165
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Caliburn.Micro;
using Dev2;
using Dev2.Common.Common;
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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Dev2.Common;
using Dev2.Studio.Core;
using Dev2.Studio.Interfaces;
using Warewolf.Studio.Core;
using Dev2.ConnectionHelpers;

namespace Warewolf.Studio.ViewModels
{
    public class EnvironmentViewModel : BindableBase, IEnvironmentViewModel
    {
        ObservableCollection<IExplorerItemViewModel> _children;
        bool _isConnecting;
        bool _isConnected;
        bool _isServerIconVisible;
        bool _isServerUnavailableIconVisible;
        bool _isExpanded;
        bool _isSelected;
        bool _canCreateFolder;
        bool _canShowServerVersion;
        bool _canCreateWorkflowService;
        readonly IShellViewModel _shellViewModel;
        readonly IConnectControlSingleton _connectControlSingleton;
        readonly bool _isDialog;
        bool _allowEdit;
        bool _allowResourceCheck;
        bool _isVisible;
        bool _showContextMenu;
        readonly IPopupController _controller;
        bool _canDrag;
        bool _forcedRefresh;
        bool _isResourceCheckedEnabled;
        bool? _isResource;
        string _filter;
        bool _canCreateSource;
        bool _canViewApisJson;
        bool _canDeploy;
        bool _isSaveDialog;
        bool _canViewExecutionLogging;
        private bool _isMergeVisible;

        EnvironmentViewModel()
        {
            var connectControlSingleton = CustomContainer.Get<IConnectControlSingleton>();
            _connectControlSingleton = connectControlSingleton ?? ConnectControlSingleton.Instance;
        }

        public EnvironmentViewModel(IServer server, IShellViewModel shellViewModel)
            : this(server, shellViewModel, false, null)
        {
        }


        public EnvironmentViewModel(IServer server, IShellViewModel shellViewModel, bool isDialog)
            : this(server, shellViewModel, isDialog, null)
        {
        }

        public EnvironmentViewModel(IServer server, IShellViewModel shellViewModel, bool isDialog, Action<IExplorerItemViewModel> selectAction)
            : this()
        {
            Server = server ?? throw new ArgumentNullException(nameof(server));
            _shellViewModel = shellViewModel ?? throw new ArgumentNullException(nameof(shellViewModel));
            _isDialog = isDialog;
            _controller = CustomContainer.Get<IPopupController>();
            _children = new ObservableCollection<IExplorerItemViewModel>();
            ExplorerTooltips = CustomContainer.Get<IExplorerTooltips>();

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

            NewRedisSourceCommand = new DelegateCommand(() =>
            {
                UpdateActiveEnvironment(shellViewModel);
                shellViewModel.NewRedisSource(ResourcePath);
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

            NewRabbitMqSourceSourceCommand = new DelegateCommand(() =>
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
                var environmentModel = ServerRepository.Instance.FindSingle(model => model.EnvironmentID == Server.EnvironmentID);
                shellViewModel.ViewApisJson(ResourcePath, environmentModel.Connection.WebServerUri);
            });

            ViewExecutionLoggingCommand = new DelegateCommand(() =>
            {
                Process.Start(Resources.Languages.Core.MyWarewolfUrl);
            });

            DeployCommand = new DelegateCommand(() =>
            {
                shellViewModel.AddDeploySurface(AsList().Union<IExplorerTreeItem>(new[] { this }));
            });

            DisplayName = server.DisplayName;
            RefreshCommand = new DelegateCommand(async () =>
            {
                await RefreshAsync().ConfigureAwait(true);
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
            IsConnected = server.IsConnected;

            server.NetworkStateChanged += (args, server1) =>
            {
                IsConnected = server1.IsConnected;
                if (args.State == ConnectionNetworkState.Connected)
                {
                    Application.Current.Dispatcher.Invoke(async () =>
                    {
                        await RefreshAsync().ConfigureAwait(true);
                    }, DispatcherPriority.Background);

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
            IsMergeVisible = false;
            ResourceId = server.EnvironmentID;
            CanViewApisJson = true;
            CanViewExecutionLogging = true;
            if (ForcedRefresh)
            {
                ForcedRefresh = true;
            }
        }

        async Task RefreshAsync()
        {
            var isDeploy = Children.Any(a => AllowResourceCheck);
            await LoadAsync(isDeploy, true).ConfigureAwait(true);
            if (isDeploy)
            {
                ShowContextMenu = false;
            }
        }

        void UpdateActiveEnvironment(IShellViewModel shellViewModel)
        {
            shellViewModel.SetActiveServer(Server.EnvironmentID);
        }

        public IShellViewModel ShellViewModel => _shellViewModel;

        public int ChildrenCount => GetChildrenCount();

        int GetChildrenCount()
        {
            var total = 0;
            if (Children != null)
            {
                foreach (var explorerItemModel in Children)
                {
                    var increaseBy = 0;
                    if (!explorerItemModel.IsResourceVersion && explorerItemModel.ResourceType != "Message")
                    {
                        increaseBy = explorerItemModel.IsFolder ? explorerItemModel.ChildrenCount : 1;
                    }
                    total = total + increaseBy;
                }
            }

            return total;
        }

        public bool ShowContextMenu
        {
            get => _showContextMenu;
            set
            {
                _showContextMenu = value;
                OnPropertyChanged(() => ShowContextMenu);
            }
        }

        public bool CanCreateWorkflowService
        {
            get => _canCreateWorkflowService;
            set
            {
                _canCreateWorkflowService = value;
                ExplorerTooltips.NewServiceTooltip = _canCreateWorkflowService ? Resources.Languages.Tooltips.NewServiceTooltip : Resources.Languages.Tooltips.NoPermissionsToolTip;
                OnPropertyChanged(() => CanCreateWorkflowService);
            }
        }

        public bool IsSaveDialog
        {
            get => _isSaveDialog;
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

            var child = new ExplorerItemViewModel(Server, this, a => { SelectAction?.Invoke(a); }, _shellViewModel, _controller)
            {
                ResourceId = id,
                ResourceType = "Folder",
                IsFolder = true,
                ResourcePath = name,
                IsSelected = true,
                IsVisible = true,
                IsNewFolder = true,
                CanCreateWorkflowService = true,
                CanView = true,
                CanDeploy = true,
                CanViewApisJson = true,
                ResourceName = name,
                IsRenaming = true
            };

            if (_isDialog)
            {
                child.AllowResourceCheck = false;
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
            get => _isResource;
            set
            {
                Dev2Logger.Info("New environment selected: " + value, GlobalConstants.WarewolfInfo);
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
            get => _isVisible;
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
                    foundAction?.Invoke(explorerItemViewModel);
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
            get => _canViewApisJson;
            set
            {
                _canViewApisJson = value;
                ExplorerTooltips.ViewApisJsonTooltip = _canViewApisJson ? Resources.Languages.Tooltips.ViewApisJsonTooltip : Resources.Languages.Tooltips.NoPermissionsToolTip;
                OnPropertyChanged(() => CanViewApisJson);
            }
        }
        public bool CanViewExecutionLogging
        {
            get => _canViewExecutionLogging;
            set
            {
                _canViewExecutionLogging = value;
                ExplorerTooltips.ViewExecutionLoggingTooltip = _canViewExecutionLogging ? Resources.Languages.Tooltips.ViewExecutionLoggingTooltip : Resources.Languages.Tooltips.NoPermissionsToolTip;
                OnPropertyChanged(() => CanViewExecutionLogging);
            }
        }

        public void SelectItem(string selectedPath, Action<IExplorerItemViewModel> foundAction)
        {
            foreach (var explorerItemViewModel in Children)
            {
                explorerItemViewModel.Apply(a =>
                {
                    if (a.ResourcePath.Replace("\\", "\\\\") == selectedPath)
                    {
                        a.IsExpanded = true;
                        foundAction?.Invoke(a);
                    }
                });
            }
        }

        public void SetPropertiesForDialogFromPermissions(IWindowsGroupPermission permissions)
        {
            AllowResourceCheck = false;
            CanCreateSource = permissions.Contribute;
            CanCreateFolder = permissions.Contribute;
            CanCreateWorkflowService = permissions.Contribute;
            CanDelete = false;
            CanDeploy = permissions.DeployFrom;
            CanRename = false;
            CanRollback = false;
            ShowContextMenu = true;
        }

        public void UpdateChildrenCount()
        {
            OnPropertyChanged(() => ChildrenCount);
        }

        public void SetPropertiesForDialog()
        {
            AllowResourceCheck = false;
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
            get => _canDrag && (IsServer || ResourceType == "ServerSource") && string.IsNullOrWhiteSpace(ResourcePath);
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
            get => Children.Count > 0;
        }

        public bool CanCreateSource
        {
            get => _canCreateSource;
            set
            {
                _canCreateSource = value;
                ExplorerTooltips.SetSourceTooltips(_canCreateSource);
                OnPropertyChanged(() => CanCreateSource);
            }
        }
        public bool CanRename { get; set; }
        public bool CanDelete { get; set; }

        public bool CanCreateFolder
        {
            get => Server.Permissions != null && Server.Permissions.Any(a => (a.Contribute || a.Administrator) && a.IsServer);
            set
            {
                if (_canCreateFolder != value)
                {
                    _canCreateFolder = value;
                    OnPropertyChanged(() => CanCreateFolder);
                }
                ExplorerTooltips.NewFolderTooltip = _canCreateFolder ? Resources.Languages.Tooltips.NewFolderTooltip : Resources.Languages.Tooltips.NoPermissionsToolTip;
            }
        }

        public bool CanDeploy
        {
            get => _canDeploy;
            set
            {
                _canDeploy = value;
                OnPropertyChanged(() => CanDeploy);
            }
        }

        public bool CanShowVersions
        {
            get => false;
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
            get => _forcedRefresh;
            set
            {
                _forcedRefresh = value;
                OnPropertyChanged(() => ForcedRefresh);
            }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged(() => IsSelected);
            }
        }

        public bool CanShowServerVersion
        {
            get => _canShowServerVersion;
            set
            {
                _canShowServerVersion = value;
                ExplorerTooltips.ServerVersionTooltip = _canShowServerVersion ? Resources.Languages.Tooltips.ServerVersionTooltip : Resources.Languages.Tooltips.NoPermissionsToolTip;
                OnPropertyChanged(() => CanShowServerVersion);
            }
        }

        public bool AllowResourceCheck
        {
            get => _allowResourceCheck;
            set
            {
                _allowResourceCheck = value;
                OnPropertyChanged(() => AllowResourceCheck);
            }
        }

        public bool? IsResourceChecked
        {
            get => _isResource;
            set
            {
                if (value == _isResource)
                {
                    return;
                }
                if (ChildrenCount == 0)
                {
                    return;
                }
                bool? isResourceChecked;
                if (IsResourceCheckedEnabled)
                {
                    isResourceChecked = value;
                }
                else
                {
                    isResourceChecked = false;
                }
                _isResource = isResourceChecked.HasValue && isResourceChecked.Value;

                if (Children.Any())
                {
                    var isChecked = _isResource;
                    Children.Apply(a => a.SetIsResourceChecked(isChecked));
                }
                SelectAll?.Invoke();
                OnPropertyChanged(() => IsResourceChecked);
                OnPropertyChanged(() => Children);
            }
        }

        public bool IsResourceCheckedEnabled
        {
            get => _isResourceCheckedEnabled;
            set
            {
                ExplorerTooltips.DeployResourceCheckboxTooltip = Resources.Languages.Core.DeployResourceCheckbox;
                if (!value)
                {
                    ExplorerTooltips.DeployResourceCheckboxTooltip = Resources.Languages.Core.DeployResourceCheckboxViewPermissionError;
                }
                _isResourceCheckedEnabled = value;
                OnPropertyChanged(() => IsResourceCheckedEnabled);
                OnPropertyChanged(() => ExplorerTooltips.DeployResourceCheckboxTooltip);
            }
        }

        void ShowServerVersionAbout()
        {
            ShellViewModel.ShowAboutBox();
        }

        string GetChildNameFromChildren()
        {
            var count = 0;
            var folderName = Resources.Languages.Core.NewFolderLabel;
            while (UnfilteredChildren != null && UnfilteredChildren.Any(a => a.ResourceName == folderName))
            {
                count++;
                folderName = Resources.Languages.Core.NewFolderLabel + " " + count;
            }
            return folderName;
        }

        public ICommand NewPostgreSqlSourceCommand { get; set; }
        public ICommand NewOracleSourceCommand { get; set; }
        public ICommand NewOdbcSourceCommand { get; set; }
        public ICommand NewPluginSourceCommand { get; set; }
        public ICommand NewComPluginSourceCommand { get; set; }
        public ICommand NewWcfSourceCommand { get; set; }
        public ICommand NewWebSourceSourceCommand { get; set; }
        public ICommand NewRedisSourceCommand { get; set; }
        public ICommand NewEmailSourceSourceCommand { get; set; }
        public ICommand NewExchangeSourceSourceCommand { get; set; }
        public ICommand NewRabbitMqSourceSourceCommand { get; set; }
        public ICommand NewSharepointSourceSourceCommand { get; set; }
        public ICommand NewDropboxSourceSourceCommand { get; set; }
        public ICommand DeployCommand { get; set; }
        [ExcludeFromCodeCoverage]
        public ICommand RenameCommand { get; set; }
        public ICommand CreateFolderCommand { get; set; }
        [ExcludeFromCodeCoverage]
        public ICommand DeleteCommand { get; set; }
        public ICommand ShowVersionHistory { get; set; }
        public ICommand RollbackCommand { get; set; }
        public ICommand ShowServerVersionCommand { get; set; }
        public ICommand Expand { get; set; }
        public ICommand RefreshCommand { get; set; }
        public ICommand ViewApisJsonCommand { get; set; }
        public ICommand ViewExecutionLoggingCommand { get; set; }

        public string DisplayName
        {
            get;
            set;
        }

        public bool IsConnected
        {
            get => _isConnected;
            private set
            {
                _isConnected = value;
                OnPropertyChanged(() => IsConnected);
            }
        }

        public bool AllowEdit
        {
            get => _allowEdit;
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
            get => _isConnecting;
            set
            {
                _isConnecting = value;
                IsServerIconVisible = !value;
                IsServerUnavailableIconVisible = !value;

                OnPropertyChanged(() => IsConnecting);
            }
        }

        public async Task<bool> LoadAsync() => await LoadAsync(false, false).ConfigureAwait(true);

        public async Task<bool> LoadAsync(bool isDeploy) => await LoadAsync(isDeploy, false).ConfigureAwait(true);

        public async Task<bool> LoadAsync(bool isDeploy, bool reloadCatalogue)
        {
            if (!IsLoading || isDeploy)
            {
                try
                {
                    IsLoading = true;
                    var result = await LoadDialogAsync(null, isDeploy, reloadCatalogue).ConfigureAwait(true);
                    ReloadConnectControl(isDeploy);
                    return result;
                }
                finally
                {
                    IsLoading = false;
                }
            }
            return false;
        }

        public void ReloadConnectControl(bool isDeploy)
        {
            if (!isDeploy)
            {
                ReloadConnectControl();
            }
        }

        private void ReloadConnectControl()
        {
            var explorerViewModel = ShellViewModel?.ExplorerViewModel;
            if (explorerViewModel?.Environments != null)
            {
                ReloadConnectControl(explorerViewModel);
            }
        }

        private void ReloadConnectControl(IExplorerViewModel explorerViewModel)
        {
            var environmentViewModel = explorerViewModel?.Environments[0];

            var explorerServers = environmentViewModel?.Children?
                                                        .Flatten(model => model.Children ?? new ObservableCollection<IExplorerItemViewModel>())
                                                        .Where(y => y != null && y.ResourceType == "Dev2Server")
                                                        .ToList();
            var connectControlViewModel = explorerViewModel?.ConnectControlViewModel;
            if (explorerServers != null && (connectControlViewModel != null && explorerServers.Any()))
            {
                var connectControlServers = connectControlViewModel.Servers?.Where(o => !o.IsLocalHost).ToObservableCollection();

                if (connectControlServers?.Count > explorerServers?.Count())
                {
                    foreach (var serv in connectControlServers)
                    {
                        ReloadConnectControl(explorerServers, connectControlViewModel, serv.EnvironmentID);
                    }
                }
                else
                {
                    foreach (var server in explorerServers)
                    {
                        ReloadConnectControl(connectControlViewModel, connectControlServers, server.ResourceId);
                    }
                }
            }
        }

        void ReloadConnectControl(IConnectControlViewModel connectControlViewModel, ObservableCollection<IServer> connectControlServers, Guid resourceId)
        {
            var serverExists = connectControlServers?.FirstOrDefault(o => o.EnvironmentID == resourceId);
            if (serverExists == null)
            {
                _connectControlSingleton.ReloadServer();
                ShellViewModel?.LocalhostServer?.UpdateRepository?.FireServerSaved(resourceId);
                connectControlViewModel.LoadServers();
            }
        }

        void ReloadConnectControl(List<IExplorerItemViewModel> explorerServers, IConnectControlViewModel connectControlViewModel, Guid environmentID)
        {
            var found = explorerServers.FirstOrDefault(a => a.ResourceId == environmentID);
            if (found == null)
            {
                _connectControlSingleton.ReloadServer();
                ShellViewModel?.LocalhostServer?.UpdateRepository?.FireServerSaved(environmentID);
                connectControlViewModel.LoadServers();
            }
        }

        public bool IsLoading { get; set; }

        public async Task<bool> LoadDialogAsync(string selectedId) => await LoadDialogAsync(selectedId, false, false).ConfigureAwait(false);

        public async Task<bool> LoadDialogAsync(string selectedId, bool isDeploy, bool reloadCatalogue)
        {
            if (IsConnected && Server.IsConnected)
            {
                IsConnecting = true;
                var explorerItems = await Server.LoadExplorer(reloadCatalogue).ConfigureAwait(true);
                if (explorerItems != null)
                {
                    CreateExplorerItemsSync(explorerItems.Children, Server, this, selectedId != null, isDeploy);
                }
                IsResourceCheckedEnabled = isDeploy;
                IsLoaded = true;
                IsConnecting = false;
                IsExpanded = true;

                return IsLoaded;
            }
            return false;
        }

        public async Task<bool> LoadDialogAsync(Guid selectedPath)
        {
            if (IsConnected)
            {
                IsConnecting = true;
                var explorerItems = await Server.LoadExplorer().ConfigureAwait(true);
                if (explorerItems != null)
                {
                    CreateExplorerItemsSync(explorerItems.Children, Server, this, selectedPath != Guid.Empty);
                }
                IsLoaded = true;
                IsConnecting = false;
                IsExpanded = true;
                return IsLoaded;
            }
            return false;
        }

        public IExplorerTreeItem FindByPath(string path)
        {
            var allChildren = Children.Flatten(model =>
            {
                if (model?.Children != null)
                {
                    return model.Children;
                }
                return new List<IExplorerItemViewModel>();
            });
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
            SelectAll?.Invoke();
        }

        public void Filter(Func<IExplorerItemViewModel, bool> filter)
        {
            Children.Apply(a => a.IsVisible = filter?.Invoke(a) ?? default(bool));
            foreach (var explorerItemViewModel in _children)
            {
                explorerItemViewModel.Filter(filter);
            }
            OnPropertyChanged(() => Children);
        }

        public ICollection<IExplorerItemViewModel> AsList() => AsList(Children);

        ICollection<IExplorerItemViewModel> AsList(ICollection<IExplorerItemViewModel> rootCollection) => rootCollection.Union(rootCollection.SelectMany(a => a.AsList())).ToList();

        public bool IsServerIconVisible
        {
            get => _isServerIconVisible && IsConnected;
            set
            {
                _isServerIconVisible = value;
                OnPropertyChanged(() => IsServerIconVisible);
            }
        }

        public bool IsServerUnavailableIconVisible
        {
            get => _isServerUnavailableIconVisible && !IsConnected;
            set
            {
                _isServerUnavailableIconVisible = value;
                OnPropertyChanged(() => IsServerIconVisible);
            }
        }

        public IExplorerTooltips ExplorerTooltips { get; set; }
        public bool IsMergeVisible
        {
            get => _isMergeVisible;
            set
            {
                _isMergeVisible = value;
                OnPropertyChanged(() => IsMergeVisible);
            }
        }

        public void CreateExplorerItemsSync(IList<IExplorerItem> explorerItems, IServer server, IExplorerTreeItem parent) => CreateExplorerItemsSync(explorerItems, server, parent, false, false);

        public void CreateExplorerItemsSync(IList<IExplorerItem> explorerItems, IServer server, IExplorerTreeItem parent, bool isDialog) => CreateExplorerItemsSync(explorerItems, server, parent, isDialog, false);

        public void CreateExplorerItemsSync(IList<IExplorerItem> explorerItems, IServer server, IExplorerTreeItem parent, bool isDialog, bool isDeploy)
        {
            if (explorerItems == null)
            {
                return;
            }

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

            foreach (var explorerItem in explorerItems)
            {
                var existingItem = parent?.Children?.FirstOrDefault(model => model.ResourcePath.ToLowerInvariant() == explorerItem.ResourcePath.ToLower());
                if (existingItem != null)
                {
                    var isResourceChecked = existingItem.IsResourceChecked;
                    existingItem.SetPermissions(explorerItem.Permissions, isDeploy);
                    CreateExplorerItemsSync(explorerItem.Children, server, existingItem, isDialog, isDeploy);
                    if (existingItem.IsFolder && isResourceChecked != null)
                    {
                        existingItem.IsResourceChecked = isResourceChecked;
                    }
                    if (!explorerItemModels.Contains(existingItem))
                    {
                        explorerItemModels.Add(existingItem);
                    }
                }
                else
                {
                    var itemCreated = CreateExplorerItem(server, parent, isDialog, isDeploy, explorerItem);
                    CreateExplorerItemsSync(explorerItem.Children, server, itemCreated, isDialog, isDeploy);
                    if (!explorerItemModels.Contains(itemCreated))
                    {
                        explorerItemModels.Add(itemCreated);
                    }
                }
            }
            return explorerItemModels;
        }

        ExplorerItemViewModel CreateExplorerItem(IServer server, IExplorerTreeItem parent, bool isDialog, bool isDeploy, IExplorerItem explorerItem)
        {
            var itemCreated = new ExplorerItemViewModel(server, parent, a => { SelectAction?.Invoke(a); }, _shellViewModel, _controller)
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

        public IExplorerItemViewModel CreateExplorerItemFromResource(IServer server, IExplorerTreeItem parent, bool isDialog, bool isDeploy, IContextualResourceModel explorerItem)
        {
            var itemCreated = new ExplorerItemViewModel(server, parent, a => { SelectAction?.Invoke(a); }, _shellViewModel, _controller)
            {
                ResourceName = explorerItem.ResourceName,
                ResourceId = explorerItem.ID,
                ResourceType = explorerItem.ServerResourceType,
                ResourcePath = explorerItem.GetSavePath(),
                AllowResourceCheck = isDeploy,
                ShowContextMenu = !isDeploy,
                IsFolder = false,
                IsService = explorerItem.ResourceType == Dev2.Studio.Interfaces.Enums.ResourceType.WorkflowService,
                IsSource = explorerItem.ResourceType == Dev2.Studio.Interfaces.Enums.ResourceType.Source,
                IsServer = explorerItem.ResourceType == Dev2.Studio.Interfaces.Enums.ResourceType.Server
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

        static void SetPropertiesForDialog(IExplorerItemViewModel itemCreated)
        {
            itemCreated.AllowResourceCheck = false;
            itemCreated.CanCreateSource = false;
            itemCreated.CanCreateWorkflowService = false;
            itemCreated.ShowContextMenu = false;
            itemCreated.CanDeploy = false;
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
            {
                foreach (var explorerItemViewModel in _children)
                {
                    explorerItemViewModel?.Dispose();
                }
            }
        }

        public bool Equals(IExplorerTreeItem x, IExplorerTreeItem y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (ReferenceEquals(x, null))
            {
                return false;
            }

            if (ReferenceEquals(y, null))
            {
                return false;
            }

            if (x.GetType() != y.GetType())
            {
                return false;
            }

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