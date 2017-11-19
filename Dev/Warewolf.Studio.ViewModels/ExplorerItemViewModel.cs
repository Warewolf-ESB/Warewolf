/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using Dev2;
using Dev2.Common;
using Dev2.Common.Interfaces.Security;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Common.Interfaces.Versioning;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Studio.Interfaces;
using Microsoft.Practices.Prism.Mvvm;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Studio.Core;
using Warewolf.Studio.Core.Popup;

namespace Warewolf.Studio.ViewModels
{
    public class ExplorerItemViewModel : BindableBase, IExplorerItemViewModel, IEquatable<ExplorerItemViewModel>
    {
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
        
        public bool Equals(ExplorerItemViewModel other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return Equals(this, other);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((ExplorerItemViewModel)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((ResourcePath?.GetHashCode() ?? 0) * 397) ^ ResourceId.GetHashCode();
            }
        }

        public Action<IExplorerItemViewModel> SelectAction { get; set; }
        string _resourceName;
        private bool _isVisible;
        bool _isRenaming;
        private readonly IExplorerRepository _explorerRepository;
        bool _canRename;
        bool _canExecute;
        bool _canEdit;
        bool _canView;
        bool _canDelete;
        bool _areVersionsVisible;
        string _versionHeader;
        string _resourceType;
        bool _userShouldEditValueNow;
        string _versionNumber;
        ObservableCollection<IExplorerItemViewModel> _children;
        bool _isExpanded;
        bool _canCreateFolder;
        bool _canDeploy;
#pragma warning disable S1450 // Private fields only used as local variables in methods should become local variables
        string _filter;
#pragma warning restore S1450 // Private fields only used as local variables in methods should become local variables
        private bool _isSelected;
        bool _canShowVersions;
        readonly IShellViewModel _shellViewModel;
        bool _canShowDependencies;
        bool _allowResourceCheck;
        bool _candrop;
        bool _canDrag;
        bool? _isResource;
        readonly IPopupController _popupController;
        IVersionInfo _versionInfo;
        private IServer _server;
        private readonly ExplorerItemViewModelCommandController _explorerItemViewModelCommandController;
        private bool _forcedRefresh;
        private string _deployResourceCheckboxTooltip;
        private bool _isService;
        private bool _isFolder;
        private bool _canDuplicate;
        private bool _canCreateTest;
        private bool _canViewRunAllTests;
        private bool _canCreateSource;
        private bool _canViewSwagger;
        private bool _canViewApisJson;
        private bool _canCreateWorkflowService;
        private bool _canDebugInputs;
        private bool _canDebugStudio;
        private bool _canDebugBrowser;
        private bool _canCreateSchedule;
        private bool _isVersion;
        private bool _isDependenciesVisible;
        private bool _isDebugBrowserVisible;
        private bool _isDebugStudioVisible;
        private bool _isDebugInputsVisible;
        private bool _isViewJsonApisVisible;
        private bool _isRunAllTestsVisible;
        private bool _isCreateTestVisible;
        private bool _isNewFolderVisible;
        private bool _isOpenVersionVisible;
        private bool _isRollbackVisible;
        private bool _isShowVersionHistoryVisible;
        private bool _isViewSwaggerVisible;
        private bool _isSource;
        private bool _isScheduleVisible;
        private bool _isDuplicateVisible;
        private bool _isNewFolder;
        private bool _isSaveDialog;
        private bool _isServer;

        public ExplorerItemViewModel(IServer server, IExplorerTreeItem parent, Action<IExplorerItemViewModel> selectAction, IShellViewModel shellViewModel, IPopupController popupController)
        {
            VerifyArgument.AreNotNull(new Dictionary<string, object> { { "server", server }, });

            ExplorerTooltips = CustomContainer.Get<IExplorerTooltips>();
            SelectAction = selectAction;
            _shellViewModel = shellViewModel;
            _popupController = popupController;
            _explorerRepository = server.ExplorerRepository;
            Server = server;
            Parent = parent;

            _explorerItemViewModelCommandController = new ExplorerItemViewModelCommandController(_shellViewModel, _popupController);

            _canShowVersions = true;
            Children = new ObservableCollection<IExplorerItemViewModel>();

            CanShowDependencies = true;
            AllowResourceCheck = false;
            IsResourceChecked = false;
            CanDuplicate = false;
            CanCreateTest = false;
            IsVisible = true;
            IsVersion = false;
            CanShowServerVersion = false;
            if (ForcedRefresh)
            {
                ForcedRefresh = true;
            }

            SetupCommands();

            _candrop = true;
            _canDrag = true;
            CanViewSwagger = false;
        }

        private void SetupCommands()
        {
            RollbackCommand = new DelegateCommand(o =>
                    {
                        var result = _popupController.ShowRollbackVersionMessage(VersionNumber);
                        if (result == MessageBoxResult.Yes)
                        {
                            _explorerItemViewModelCommandController.RollbackCommand(_explorerRepository, Parent, ResourceId, VersionNumber);
                        }
                    });
            DeployCommand = new DelegateCommand(a => ShellViewModel.AddDeploySurface(AsList().Union(new[] { this })));
            LostFocus = new DelegateCommand(o => LostFocusCommand());
            OpenCommand = new DelegateCommand(o =>
            {
                _explorerItemViewModelCommandController.OpenCommand(this, Server);
            });
            RenameCommand = new DelegateCommand(o => IsRenaming = true);

            ViewSwaggerCommand = new DelegateCommand(o =>
            {
                _explorerItemViewModelCommandController.ViewSwaggerCommand(ResourceId, Server);
            });
            ViewApisJsonCommand = new DelegateCommand(o =>
            {
                _explorerItemViewModelCommandController.ViewApisJsonCommand(ResourcePath, Server.Connection.WebServerUri);
            });

            NewServerCommand = new DelegateCommand(o =>
            {
                _explorerItemViewModelCommandController.NewServerSourceCommand(ResourcePath, Server);
            });

            NewSqlServerSourceCommand = new DelegateCommand(o =>
            {
                _explorerItemViewModelCommandController.NewSqlServerSourceCommand(ResourcePath, Server);
            });

            NewMySqlSourceCommand = new DelegateCommand(o =>
            {
                _explorerItemViewModelCommandController.NewMySqlSourceCommand(ResourcePath, Server);
            });

            NewPostgreSqlSourceCommand = new DelegateCommand(o =>
            {
                _explorerItemViewModelCommandController.NewPostgreSqlSourceCommand(ResourcePath, Server);
            });

            NewOracleSourceCommand = new DelegateCommand(o =>
            {
                _explorerItemViewModelCommandController.NewOracleSourceCommand(ResourcePath, Server);
            });

            NewOdbcSourceCommand = new DelegateCommand(o =>
            {
                _explorerItemViewModelCommandController.NewOdbcSourceCommand(ResourcePath, Server);
            });

            NewPluginSourceCommand = new DelegateCommand(o =>
            {
                _explorerItemViewModelCommandController.NewPluginSourceCommand(ResourcePath, Server);
            });

            NewComPluginSourceCommand = new DelegateCommand(o =>
            {
                _explorerItemViewModelCommandController.NewComPluginSourceCommand(ResourcePath, Server);
            });

            NewWcfSourceCommand = new DelegateCommand(o =>
            {
                _explorerItemViewModelCommandController.NewWcfSourceCommand(ResourcePath, Server);
            });

            NewWebSourceSourceCommand = new DelegateCommand(o =>
            {
                _explorerItemViewModelCommandController.NewWebSourceCommand(ResourcePath, Server);
            });

            NewEmailSourceSourceCommand = new DelegateCommand(o =>
            {
                _explorerItemViewModelCommandController.NewEmailSourceCommand(ResourcePath, Server);
            });

            NewExchangeSourceSourceCommand = new DelegateCommand(o =>
            {
                _explorerItemViewModelCommandController.NewExchangeSourceCommand(ResourcePath, Server);
            });

            NewRabbitMqSourceSourceCommand = new DelegateCommand(o =>
            {
                _explorerItemViewModelCommandController.NewRabbitMQSourceCommand(ResourcePath, Server);
            });

            NewSharepointSourceSourceCommand = new DelegateCommand(o =>
            {
                _explorerItemViewModelCommandController.NewSharepointSourceCommand(ResourcePath, Server);
            });

            NewDropboxSourceSourceCommand = new DelegateCommand(o =>
            {
                _explorerItemViewModelCommandController.NewDropboxSourceCommand(ResourcePath, Server);
            });
            NewServiceCommand = new DelegateCommand(type =>
            {
                _explorerItemViewModelCommandController.NewServiceCommand(ResourcePath, Server);
            });
            DebugStudioCommand = new DelegateCommand(type =>
            {
                _explorerItemViewModelCommandController.DebugStudioCommand(ResourceId, Server);
            });
            DebugBrowserCommand = new DelegateCommand(type =>
            {
                _explorerItemViewModelCommandController.DebugBrowserCommand(ResourceId, Server);
            });
            ScheduleCommand = new DelegateCommand(type =>
            {
                _explorerItemViewModelCommandController.ScheduleCommand(ResourceId);
            });
            RunAllTestsCommand = new DelegateCommand(type =>
            {
                _explorerItemViewModelCommandController.RunAllTestsCommand(ResourceId);
            });
            CopyUrlCommand = new DelegateCommand(type =>
            {
                _explorerItemViewModelCommandController.CopyUrlCommand(ResourceId, Server);
            });
            ShowDependenciesCommand = new DelegateCommand(o => ShowDependencies());
            ShowVersionHistory = new DelegateCommand(selecteExplorerItem =>
            {
                AreVersionsVisible = !AreVersionsVisible;
            });
            DeleteCommand = new DelegateCommand(o => Delete());
            DuplicateCommand = new DelegateCommand(o => DuplicateResource(), o => CanDuplicate);
            CreateTestCommand = new DelegateCommand(o => CreateTest(), o => CanCreateTest);
            VersionHeader = Resources.Languages.Core.ShowVersionHistoryLabel;
            Expand = new DelegateCommand(clickCount =>
            {
                if (clickCount != null && (int)clickCount == 2 && IsFolder)
                {
                    IsExpanded = !IsExpanded;
                }
                if (clickCount != null && (int)clickCount == 2 && ResourceType == "WorkflowService" && IsExpanded)
                {
                    IsExpanded = false;
                }
            });
            CreateFolderCommand = new DelegateCommand(o => CreateNewFolder());
            DeleteVersionCommand = new DelegateCommand(o => DeleteVersion());
        }

        private void DuplicateResource()
        {
            _explorerItemViewModelCommandController.DuplicateResource(this);
        }

        private void CreateTest()
        {
            _explorerItemViewModelCommandController.CreateTest(ResourceId);
        }

        public void ShowDependencies()
        {
            _explorerItemViewModelCommandController.ShowDependenciesCommand(ResourceId, Server, IsSource || IsServer);
        }

        void DeleteVersion()
        {
            _explorerItemViewModelCommandController.DeleteVersionCommand(_explorerRepository, this, Parent, ResourceName);
        }

        public string ActivityName => typeof(DsfActivity).AssemblyQualifiedName;

        public IExplorerTooltips ExplorerTooltips { get; set; }

        public IExplorerTreeItem Parent { get; set; }

        public void AddSibling(IExplorerItemViewModel sibling)
        {
            Parent?.AddChild(sibling);
        }

        public int ChildrenCount => GetChildrenCount();

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

        public void SelectItem(Guid id, Action<IExplorerItemViewModel> foundAction)
        {
            foreach (var a in Children)
            {
                if (a.ResourceId == id)
                {
                    a.IsExpanded = true;
                    a.IsSelected = true;
                    foundAction(a);
                    continue;
                }
                a.SelectItem(id, foundAction);
            }
        }

        public bool IsSource
        {
            get => _isSource;
            set
            {
                _isSource = value;
                SetContextMenuVisibility();
                OnPropertyChanged(() => IsSource);
            }
        }

        private void SetContextMenuVisibility()
        {
            IsNewFolderVisible = _isFolder;
            IsCreateTestVisible = _isService;
            IsRunAllTestsVisible = _isService;
            IsViewSwaggerVisible = _isService;
            IsViewJsonApisVisible = _isService || _isFolder;

            IsDebugInputsVisible = _isService;
            IsDebugStudioVisible = _isService;
            IsDebugBrowserVisible = _isService;

            IsShowVersionHistoryVisible = _isService;
            IsRollbackVisible = _isService;
            IsOpenVersionVisible = _isService;

            IsDependenciesVisible = _isService || _isSource || _isServer;
            IsScheduleVisible = _isService;

            CanViewApisJson = (_isFolder || _isService) && _canView;
            CanViewSwagger = _isService && _canView;
        }

        public bool IsService
        {
            get => _isService;
            set
            {
                _isService = value;
                SetContextMenuVisibility();
                OnPropertyChanged(() => IsService);
            }
        }

        public bool IsFolder
        {
            get => _isFolder;
            set
            {
                _isFolder = value;
                SetContextMenuVisibility();
                OnPropertyChanged(() => IsFolder);
            }
        }

        public bool IsServer
        {
            get => _isServer;
            set
            {
                _isServer = value;
                SetContextMenuVisibility();
                OnPropertyChanged(() => IsServer);
            }
        }
        public bool IsResourceVersion
        {
            get;
            set;
        }

        public void UpdateChildrenCount()
        {
            OnPropertyChanged(() => ChildrenCount);
        }

        public void CreateNewFolder()
        {
            if (IsFolder)
            {
                IsExpanded = true;
                var id = Guid.NewGuid();
                var name = GetChildNameFromChildren();
                var child = _explorerItemViewModelCommandController.CreateChild(name, id, Server, this, SelectAction);

                AddChild(child);
            }
        }

        public void Apply(Action<IExplorerItemViewModel> action)
        {
            action(this);
            if (Children != null)
            {
                foreach (var explorerItemViewModel in Children)
                {
                    explorerItemViewModel.Apply(action);
                }
            }
        }

        public void Filter(Func<IExplorerItemViewModel, bool> filter)
        {
            foreach (var explorerItemViewModel in _children)
            {
                explorerItemViewModel.Filter(filter);
            }
            if (_children.Count > 0 && _children.Any(model => model.IsVisible && !model.IsResourceVersion))
            {
                IsVisible = true;
            }
            else
            {
                if (!string.IsNullOrEmpty(ResourceName) && !IsResourceVersion)
                {
                    IsVisible = filter(this);
                }
            }
            OnPropertyChanged(() => Children);
        }

        public bool CanMove { get; private set; }

        public IEnumerable<IExplorerItemViewModel> AsList()
        {
            if (UnfilteredChildren != null)
            {
                return UnfilteredChildren.Union(UnfilteredChildren.SelectMany(a => a.AsList()));
            }
            return new List<IExplorerItemViewModel>();
        }

        string GetChildNameFromChildren()
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

        void LostFocusCommand()
        {
            IsRenaming = false;
        }

        public void Delete()
        {
            _explorerItemViewModelCommandController.DeleteCommand(Parent, _explorerRepository, this, _popupController, Server);
        }

        public void SetPermissions(Permissions explorerItemPermissions) => SetPermissions(explorerItemPermissions, false);

        public void SetPermissions(Permissions explorerItemPermissions, bool isDeploy)
        {
            SetPermission(explorerItemPermissions, isDeploy);
            if (IsFolder)
            {
                CanEdit = false;
                CanExecute = false;
                return;
            }
            if (!IsService)
            {
                CanEdit = true;
                CanExecute = false;
                if (isDeploy || IsVersion)
                {
                    CanEdit = false;
                }
            }
        }

        public void SetPermission(Permissions permission) => SetPermission(permission, false);
        public void SetPermission(Permissions permission, bool isDeploy)
        {
            SetNonePermissions();

            if (Server.CanDeployFrom)
            {
                CanDeploy = true;
            }
            if (permission.HasFlag(Permissions.View))
            {
                SetViewPermissions(isDeploy);
            }
            if (permission.HasFlag(Permissions.Execute))
            {
                SetExecutePermissions(isDeploy);
            }
            if (permission.HasFlag(Permissions.Contribute))
            {
                SetContributePermissions(isDeploy);
            }
            if (permission.HasFlag(Permissions.Administrator))
            {
                SetAdministratorPermissions(isDeploy);
            }
        }

        private void SetExecutePermissions(bool isDeploy)
        {
            CanExecute = IsService && !isDeploy;
            CanViewApisJson = true;
            CanViewSwagger = true;
            CanDebugInputs = true;
            CanContribute = false;
            CanDebugStudio = true;
            CanDebugBrowser = true;
        }

        private void SetViewPermissions(bool isDeploy)
        {
            CanView = !isDeploy;
            CanShowDependencies = true;
            CanContribute = false;
            CanShowVersions = true;
            CanViewApisJson = true;
            CanViewSwagger = true;
        }

        private void SetNonePermissions()
        {
            CanRename = false;
            CanContribute = false;
            CanEdit = false;
            CanDuplicate = false;
            CanDebugInputs = false;
            CanDebugStudio = false;
            CanDebugBrowser = false;
            CanCreateSchedule = false;
            CanCreateTest = false;
            CanViewRunAllTests = false;
            CanCreateTest = false;
            CanDelete = false;
            CanCreateFolder = false;
            CanDeploy = false;
            CanCreateWorkflowService = false;
            CanShowDependencies = false;
            CanCreateSource = false;
            CanView = false;
            CanViewApisJson = false;
            CanMove = false;
            CanViewSwagger = false;
            CanShowVersions = false;
        }

        private void SetAdministratorPermissions(bool isDeploy)
        {
            CanRename = true;
            CanEdit = !isDeploy;
            CanDuplicate = true;
            CanCreateTest = true;
            CanViewRunAllTests = true;
            CanDelete = true;
            CanCreateFolder = true;
            CanContribute = true;
            CanDeploy = true;
            CanCreateWorkflowService = true;
            CanCreateSource = true;
            CanView = true;
            CanViewApisJson = true;
            CanMove = true;
            CanViewSwagger = true;
            CanShowVersions = true;
            CanShowDependencies = true;
            CanDebugInputs = true;
            CanDebugStudio = true;
            CanDebugBrowser = true;
            CanCreateSchedule = true;
            CanCreateTest = true;
        }

        private void SetContributePermissions(bool isDeploy)
        {
            CanEdit = !isDeploy;
            CanRename = true;
            CanView = true;
            CanDuplicate = true;
            CanCreateTest = true;
            CanViewRunAllTests = true;
            CanDelete = true;
            CanCreateFolder = true;
            CanShowDependencies = true;
            CanContribute = true;
            CanCreateWorkflowService = true;
            CanCreateSource = true;
            CanShowVersions = true;
            CanMove = true;
            CanViewApisJson = true;
            CanViewSwagger = true;
            CanDebugInputs = true;
            CanDebugStudio = true;
            CanDebugBrowser = true;
            CanCreateSchedule = true;
            CanCreateTest = true;
            CanViewRunAllTests = true;
        }

        bool UserShouldEditValueNow
        {
            get => _userShouldEditValueNow;
            set
            {
                _userShouldEditValueNow = value;
                OnPropertyChanged(() => UserShouldEditValueNow);
            }
        }

        public ICommand CreateFolderCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand DuplicateCommand { get; set; }
        public ICommand CreateTestCommand { get; set; }
        public ICommand ShowVersionHistory { get; set; }
        public ICommand RollbackCommand { get; set; }
        public bool IsRenaming
        {
            get => _isRenaming;
            set
            {
                _isRenaming = value;
                UserShouldEditValueNow = _isRenaming;
                OnPropertyChanged(() => IsRenaming);
            }
        }

        public string ResourceName
        {
            get => _resourceName;
            set
            {
                if (_resourceName != null && Parent != null && Parent.Children.Any(a => a.ResourceName == value) && value != _resourceName)
                {
                    _shellViewModel.ShowPopup(_popupController.GetDuplicateMessage(value));
                }
                else
                {
                    var newName = RemoveInvalidCharacters(value);

                    if (_explorerRepository != null && IsRenaming)
                    {
                        if (_resourceName == newName)
                        {
                            IsRenaming = false;
                        }
                        else
                        {
                            ValidateIfFolder(newName);
                        }
                    }
                    if (!IsRenaming)
                    {
                        _resourceName = newName;
                    }
                    IsRenaming = false;

                    OnPropertyChanged(() => ResourceName);
                }
            }
        }

        private void ValidateIfFolder(string newName)
        {
            if (IsNewFolder)
            {
                Server.ExplorerRepository.CreateFolder(Parent == null ? "root" : Parent.ResourcePath, NewName(newName), ResourceId);

                if (!ResourcePath.Contains("\\"))
                {
                    if (_resourceName != null)
                    {
                        ResourcePath = ResourcePath.Replace(_resourceName, newName);
                    }
                }
                else
                {
                    ResourcePath = ResourcePath.Substring(0, ResourcePath.LastIndexOf("\\", StringComparison.OrdinalIgnoreCase) + 1) + newName;
                }
                UpdateResourcePaths(this);
                _resourceName = newName;
                IsNewFolder = false;
            }
            else
            {
                RenameExistingResource(newName);
            }
        }

        private void RenameExistingResource(string newName)
        {
            var oldName = _resourceName;
            try
            {
                ShellViewModel.SetRefreshExplorerState(true);
                if (_explorerRepository.Rename(this, NewName(newName)))
                {
                    ResourcePath = !ResourcePath.Contains("\\") && _resourceName != null ? ResourcePath.Replace(_resourceName, newName) : ResourcePath.Substring(0, ResourcePath.LastIndexOf("\\", StringComparison.OrdinalIgnoreCase) + 1) + newName;
                    UpdateResourcePaths(this);
                    _resourceName = newName;
                }
                ShellViewModel.SetRefreshExplorerState(false);
            }
            catch (Exception exception)
            {
                Dev2Logger.Error(exception, "Warewolf Error");

                _popupController.Show(Resources.Languages.Core.FailedToRenameResource,
                    Resources.Languages.Core.FailedToRenameResourceHeader, MessageBoxButton.OK, MessageBoxImage.Error, "", false, true,
                    false, false, false, false);
                _resourceName = oldName;
                ShellViewModel.SetRefreshExplorerState(false);
            }
        }

        private string RemoveInvalidCharacters(string name)
        {
            var nameToFix = name.TrimStart(' ').TrimEnd(' ');
            if (string.IsNullOrEmpty(nameToFix) || IsDuplicateName(name))
            {
                nameToFix = GetChildNameFromChildren();
            }
            return Regex.Replace(nameToFix, @"[^a-zA-Z0-9._\s-]", "");
        }

        private bool IsDuplicateName(string requestedServiceName)
        {
            var hasDuplicate = Children.Any(model => model.ResourceName.ToLower(CultureInfo.InvariantCulture) == requestedServiceName.ToLower(CultureInfo.InvariantCulture) && model.ResourceType == "Folder");
            return hasDuplicate;
        }

        string NewName(string value)
        {
            return value;
        }

        public ObservableCollection<IExplorerItemViewModel> UnfilteredChildren { get; set; }

        public ObservableCollection<IExplorerItemViewModel> Children
        {
            get
            {
                if (_children == null)
                {
                    return new AsyncObservableCollection<IExplorerItemViewModel>();
                }
                var collection = _children;
                UnfilteredChildren = _children;
                return new AsyncObservableCollection<IExplorerItemViewModel>(collection.Where(a => a.IsVisible));
            }
            set
            {
                _children = value;
                UnfilteredChildren = value;
                OnPropertyChanged(() => Children);
                OnPropertyChanged(() => ChildrenCount);
            }
        }
        public bool Checked { get; set; }
        public Guid ResourceId { get; set; }
        public string ResourceType
        {
            get => _resourceType;
            set
            {
                _resourceType = value;
                IsVersion = _resourceType == "Version";
                OnPropertyChanged(() => CanView);
                OnPropertyChanged(() => CanShowVersions);
            }
        }
        public ICommand OpenCommand
        {
            get;
            set;
        }
        public ICommand ViewSwaggerCommand { get; set; }
        public bool CanViewExecutionLogging { get; set; }
        public ICommand ViewApisJsonCommand { get; set; }
        public ICommand ViewExecutionLoggingCommand { get; set; }

        public ICommand DebugCommand => new DelegateCommand(o =>
        {
            _explorerItemViewModelCommandController.OpenCommand(this, Server);
            ShellViewModel.Debug();
        });

        public bool IsExpanderVisible
        {
            get => Children.Count > 0 && !AreVersionsVisible;
            set
            {
                _isVisible = value;
                OnPropertyChanged(() => IsExpanderVisible);
            }
        }
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
        public ICommand NewRabbitMqSourceSourceCommand { get; set; }
        public ICommand NewSharepointSourceSourceCommand { get; set; }
        public ICommand NewDropboxSourceSourceCommand { get; set; }
        public ICommand DeployCommand { get; set; }

        public ICommand DebugStudioCommand { get; set; }
        public ICommand DebugBrowserCommand { get; set; }
        public ICommand ScheduleCommand { get; set; }
        public ICommand RunAllTestsCommand { get; set; }
        public ICommand CopyUrlCommand { get; set; }

        public bool ForcedRefresh
        {
            get => Parent?.ForcedRefresh ?? _forcedRefresh;
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
                if (_isSelected != value)
                {
                    _isSelected = value;

                    OnPropertyChanged(() => IsSelected);
                    _shellViewModel?.SetActiveServer(Server.EnvironmentID);
                }
            }
        }
        public bool CanShowServerVersion { get; set; }
        public bool AllowResourceCheck
        {
            get => _allowResourceCheck;
            set
            {
                _allowResourceCheck = value;
                OnPropertyChanged(() => AllowResourceCheck);
            }
        }

        public bool? IsResourceUnchecked
        {
            get => !_isResource;
            set
            {
                _isResource = !value;
                OnPropertyChanged(() => IsResourceChecked);
                OnPropertyChanged(() => IsResourceUnchecked);
            }
        }

        public bool? IsResourceChecked
        {
            get => _isResource;
            set
            {
                bool? isResourceChecked;
                if (IsResourceCheckedEnabled)
                {
                    isResourceChecked = value;
                }
                else
                {
                    isResourceChecked = false;
                }
                if (IsFolder)
                {
                    if (ChildrenCount >= 1 && value != null)
                    {
                        Children.Apply(a => a.IsResourceChecked = isResourceChecked);
                    }
                }
                else
                {
                    IsResourceCheckedEnabled = CanDeploy;
                    _isResource = isResourceChecked.HasValue && !IsFolder && isResourceChecked.Value;
                }
                IsSelected = isResourceChecked != null && (bool)isResourceChecked;
                SelectAction?.Invoke(this);
                OnPropertyChanged(() => IsResourceChecked);
            }
        }

        public bool IsResourceCheckedEnabled
        {
            get => CanDeploy;
            set
            {
                DeployResourceCheckboxTooltip = Resources.Languages.Core.DeployResourceCheckbox;
                if (!value)
                {
                    DeployResourceCheckboxTooltip = Resources.Languages.Core.DeployResourceCheckboxViewPermissionError;
                }
                OnPropertyChanged(() => IsResourceCheckedEnabled);
                OnPropertyChanged(() => DeployResourceCheckboxTooltip);
                OnPropertyChanged(() => CanDeploy);
            }
        }

        public string DeployResourceCheckboxTooltip
        {
            get => _deployResourceCheckboxTooltip;
            set
            {
                _deployResourceCheckboxTooltip = value;
                OnPropertyChanged(() => DeployResourceCheckboxTooltip);
            }
        }

        public bool? IsFolderChecked
        {
            get => _isResource;
            set
            {
                if (IsFolder)
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
                    SelectAction?.Invoke(this);
                }
                OnPropertyChanged(() => IsResourceChecked);
                Dev2Logger.Info("Explorer Item View Model Is Folder Checked set to " + value, GlobalConstants.WarewolfInfo);
            }
        }

        public ICommand RenameCommand { get; set; }

        public bool CanCreateSource
        {
            get => _canCreateSource && !IsSaveDialog;
            set
            {
                _canCreateSource = value;
                ExplorerTooltips.SetSourceTooltips(_canCreateSource);
                OnPropertyChanged(() => CanCreateSource);
            }
        }

        public bool CanViewSwagger
        {
            get => _canViewSwagger && !IsSaveDialog;
            set
            {
                _canViewSwagger = value;
                ExplorerTooltips.ViewSwaggerTooltip = _canViewSwagger ? Resources.Languages.Tooltips.ViewSwaggerToolTip : Resources.Languages.Tooltips.NoPermissionsToolTip;
                OnPropertyChanged(() => CanViewSwagger);
            }
        }

        public bool CanViewApisJson
        {
            get => _canViewApisJson && !IsSaveDialog;
            set
            {
                _canViewApisJson = value;
                ExplorerTooltips.ViewApisJsonTooltip = _canViewApisJson ? Resources.Languages.Tooltips.ViewApisJsonTooltip : Resources.Languages.Tooltips.NoPermissionsToolTip;
                OnPropertyChanged(() => CanViewApisJson);
            }
        }
        
        public bool CanCreateWorkflowService
        {
            get => _canCreateWorkflowService && !IsSaveDialog;
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

        public bool ShowContextMenu { get; set; }

        public bool CanRename
        {
            get => _canRename;
            set
            {
                _canRename = value;
                ExplorerTooltips.RenameTooltip = _isFolder ? _canRename ? Resources.Languages.Tooltips.RenameFolderTooltip : Resources.Languages.Tooltips.NoPermissionsToolTip : _canRename ? Resources.Languages.Tooltips.RenameItemTooltip : Resources.Languages.Tooltips.NoPermissionsToolTip;
                OnPropertyChanged(() => CanRename);
            }
        }
        public bool CanDuplicate
        {
            get => _canDuplicate && !IsSaveDialog;
            set
            {
                _canDuplicate = value;
                ExplorerTooltips.DuplicateTooltip = _canDuplicate ? Resources.Languages.Tooltips.DuplicateToolTip : Resources.Languages.Tooltips.NoPermissionsToolTip;
                OnPropertyChanged(() => CanDuplicate);
            }
        }
        public bool CanCreateTest
        {
            get => _canCreateTest && IsService && !IsSaveDialog;
            set
            {
                _canCreateTest = value;
                ExplorerTooltips.CreateTestTooltip = _canCreateTest ? Resources.Languages.Tooltips.TestEditorToolTip : Resources.Languages.Tooltips.NoPermissionsToolTip;
                OnPropertyChanged(() => CanCreateTest);
            }
        }
        public bool CanViewRunAllTests
        {
            get => _canViewRunAllTests && IsService && !IsSaveDialog;
            set
            {
                _canViewRunAllTests = value;
                ExplorerTooltips.RunAllTestsTooltip = _canViewRunAllTests ? Resources.Languages.Tooltips.RunAllTestsToolTip : Resources.Languages.Tooltips.NoPermissionsToolTip;
                OnPropertyChanged(() => CanCreateTest);
            }
        }

        public bool CanDelete
        {
            get
            {
                if (IsSaveDialog && IsFolder)
                {
                    return true;
                }
                return _canDelete;
            }
            set
            {
                _canDelete = value;
                ExplorerTooltips.DeleteTooltip = _isFolder ? _canDelete ? Resources.Languages.Tooltips.DeleteFolderTooltip : Resources.Languages.Tooltips.NoPermissionsToolTip : _canDelete ? Resources.Languages.Tooltips.DeleteItemTooltip : Resources.Languages.Tooltips.NoPermissionsToolTip;

                OnPropertyChanged(() => CanDelete);
            }
        }
        public bool CanCreateFolder
        {
            get => (IsFolder || IsServer) && _canCreateFolder;
            set
            {
                _canCreateFolder = value;
                ExplorerTooltips.NewFolderTooltip = _canCreateFolder ? Resources.Languages.Tooltips.NewFolderTooltip : Resources.Languages.Tooltips.NoPermissionsToolTip;
                OnPropertyChanged(() => CanCreateFolder);
            }
        }

        public bool CanDebugInputs
        {
            get => _canDebugInputs && !IsSaveDialog;
            set
            {
                _canDebugInputs = value;
                ExplorerTooltips.DebugInputsTooltip = _canDebugInputs ? Resources.Languages.Tooltips.DebugInputsToolTip : Resources.Languages.Tooltips.NoPermissionsToolTip;
                OnPropertyChanged(() => CanDebugInputs);
            }
        }

        public bool CanDebugStudio
        {
            get => _canDebugStudio && !IsSaveDialog;
            set
            {
                _canDebugStudio = value;
                ExplorerTooltips.DebugStudioTooltip = _canDebugStudio ? Resources.Languages.Tooltips.DebugStudioToolTip : Resources.Languages.Tooltips.NoPermissionsToolTip;
                OnPropertyChanged(() => CanDebugStudio);
            }
        }

        public bool CanDebugBrowser
        {
            get => _canDebugBrowser && !IsSaveDialog;
            set
            {
                _canDebugBrowser = value;
                ExplorerTooltips.DebugBrowserTooltip = _canDebugBrowser ? Resources.Languages.Tooltips.DebugBrowserToolTip : Resources.Languages.Tooltips.NoPermissionsToolTip;
                OnPropertyChanged(() => CanDebugBrowser);
            }
        }

        public bool CanCreateSchedule
        {
            get => _canCreateSchedule && !IsSaveDialog;
            set
            {
                _canCreateSchedule = value;
                ExplorerTooltips.ScheduleTooltip = _canCreateSchedule ? Resources.Languages.Tooltips.ScheduleToolTip : Resources.Languages.Tooltips.NoPermissionsToolTip;
                OnPropertyChanged(() => CanCreateSchedule);
            }
        }

        public bool CanDeploy
        {
            get => _canDeploy && !IsSaveDialog;
            set
            {
                if (_canDeploy != value)
                {
                    _canDeploy = value;
                    IsResourceCheckedEnabled = _canDeploy;
                    ExplorerTooltips.DeployTooltip = _canDeploy ? Resources.Languages.Tooltips.DeployToolTip : Resources.Languages.Tooltips.NoPermissionsToolTip;
                }
            }
        }

        public ICommand LostFocus { get; set; }
        public bool CanExecute
        {
            get => _canExecute && !IsFolder && !IsServer;
            set
            {
                if (_canExecute != value)
                {
                    _canExecute = value;
                    OnPropertyChanged(() => CanExecute);
                }
            }
        }
        public bool CanEdit
        {
            get => _canEdit;
            set
            {
                if (_canEdit != value)
                {
                    _canEdit = value;
                    OnPropertyChanged(() => CanEdit);
                }
            }
        }

        public bool CanContribute { get; set; }

        public bool CanView
        {
            get => _canView;
            set
            {
                if (_canView != value)
                {
                    _canView = value;
                    OnPropertyChanged(() => CanView);
                }
            }
        }

        public bool CanShowDependencies
        {
            get => _canShowDependencies && !IsFolder && !IsSaveDialog;
            set
            {
                _canShowDependencies = value;
                ExplorerTooltips.DependenciesTooltip = _canShowDependencies ? Resources.Languages.Tooltips.DependenciesToolTip : Resources.Languages.Tooltips.NoPermissionsToolTip;
                OnPropertyChanged(() => CanShowDependencies);
            }
        }

        public bool IsVersion
        {
            get => _isVersion;
            set
            {
                _isVersion = value;
                ExplorerTooltips.RollbackTooltip = _isVersion ? Resources.Languages.Tooltips.RollbackTooltip : Resources.Languages.Tooltips.NoPermissionsToolTip;
                OnPropertyChanged(() => IsVersion);
            }
        }

        public bool CanShowVersions
        {
            get => IsService && _canShowVersions && !IsSaveDialog;
            set
            {
                _canShowVersions = value;
                ExplorerTooltips.ShowHideVersionsTooltip = _canShowVersions ? Resources.Languages.Tooltips.ShowHideVersionsTooltip : Resources.Languages.Tooltips.NoPermissionsToolTip;
                OnPropertyChanged(() => CanShowVersions);
            }
        }
        public bool CanRollback => IsResourceVersion;

        public IShellViewModel ShellViewModel => _shellViewModel;

        public bool AreVersionsVisible
        {
            get => _areVersionsVisible;
            set
            {
                _areVersionsVisible = value;
                
                VersionHeader = !value ? Resources.Languages.Core.ShowVersionHistoryLabel : Resources.Languages.Core.HideVersionHistoryLabel;
                if (value)
                {
                    var versionInfos = _explorerRepository.GetVersions(ResourceId);
                    if (versionInfos.Count <= 0)
                    {
                        _areVersionsVisible = false;
                        VersionHeader = Resources.Languages.Core.ShowVersionHistoryLabel;
                    }
                    else
                    {
                        _children = new ObservableCollection<IExplorerItemViewModel>(versionInfos.Select(a => CreateNewVersion(a)));
                        OnPropertyChanged(() => Children);
                        if (Children.Count > 0)
                        {
                            UpdateResourceVersions();
                        }
                    }
                }
                else
                {
                    _children = new ObservableCollection<IExplorerItemViewModel>();
                    OnPropertyChanged(() => Children);
                }
                OnPropertyChanged(() => AreVersionsVisible);
            }
        }

        private VersionViewModel CreateNewVersion(IVersionInfo a)
        {
            return new VersionViewModel(Server, this, null, _shellViewModel, _popupController)
            {
                ResourceName =
                "v." + a.VersionNumber + " " +
                a.DateTimeStamp.ToString(CultureInfo.InvariantCulture) + " " +
                a.Reason.Replace(".xml", ""),
                VersionNumber = a.VersionNumber,
                VersionInfo = a,
                ResourceId = ResourceId,
                IsVersion = true,
                CanEdit = false,
                CanCreateWorkflowService = false,
                ShowContextMenu = true,
                CanCreateSource = false,
                IsResourceVersion = true,
                AllowResourceCheck = false,
                IsResourceChecked = false,
                CanDelete = CanDelete,
                ResourceType = "Version"
            };
        }

        private void UpdateResourceVersions()
        {
            IsExpanded = true;
            foreach (var child in Children)
            {
                if (child is ExplorerItemViewModel explorerItemViewModel && explorerItemViewModel.IsVersion)
                {
                    var permissions = Server?.GetPermissions(explorerItemViewModel.ResourceId);
                    if (permissions.HasValue)
                    {
                        explorerItemViewModel.SetPermissions(permissions.Value);
                        explorerItemViewModel.CanDelete = CanDelete;
                        explorerItemViewModel.CanRename = false;
                        explorerItemViewModel.CanDuplicate = false;
                        explorerItemViewModel.CanDeploy = false;
                    }
                    explorerItemViewModel.IsRollbackVisible = explorerItemViewModel.Parent.IsService;
                }
            }
        }

        public IVersionInfo VersionInfo
        {
            get => _versionInfo;
            set
            {
                _versionInfo = value;
                OnPropertyChanged(() => VersionInfo);
            }
        }
        public string VersionNumber
        {
            get => _versionNumber;
            set
            {
                _versionNumber = value;
                OnPropertyChanged(() => VersionNumber);
            }
        }
        public string VersionHeader
        {
            get => _versionHeader;
            set
            {
                _versionHeader = value;
                OnPropertyChanged(() => VersionHeader);
            }
        }
        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                if (_isVisible != value)
                {
                    _isVisible = value;
                    OnPropertyChanged(() => IsVisible);
                }
            }
        }

        public bool IsNewFolder
        {
            get => _isNewFolder;
            set
            {
                if (_isNewFolder != value)
                {
                    _isNewFolder = value;
                    OnPropertyChanged(() => IsNewFolder);
                }
            }
        }

        public async Task<bool> Move(IExplorerTreeItem destination)
        {
            try
            {
                if (Equals(this, destination))
                {
                    return false;
                }
                if (Parent != null && destination.Parent != null && Equals(Parent, destination))
                {
                    return false;
                }
                destination.AddChild(this);
                Parent?.RemoveChild(this);
                var moveResult = await _explorerRepository.Move(this, destination).ConfigureAwait(true);
                if (!moveResult)
                {
                    ShowErrorMessage(Resources.Languages.Core.ExplorerMoveFailedMessage, Resources.Languages.Core.ExplorerMoveFailedHeader);
                    Parent?.AddChild(this);
                    destination.RemoveChild(this);
                    return false;
                }
                Parent = destination;
                UpdateResourcePaths(destination);
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex.Message, Resources.Languages.Core.ExplorerMoveFailedHeader);
                return false;
            }
            return true;
        }

        private void UpdateResourcePaths(IExplorerTreeItem destination)
        {
            if (destination.IsFolder)
            {
                if (destination.Children.Any(a => a.ResourceName == ResourceName && a.IsFolder))
                {
                    var destfolder = destination.Children.FirstOrDefault(a => a.ResourceName == ResourceName && a.IsFolder);
                    if (destfolder != null)
                    {
                        destfolder.ResourcePath = destination.ResourcePath + "\\" + destfolder.ResourceName;
                        var resourcePath = destfolder.ResourcePath;
                        UpdateChildrenPath(resourcePath);
                    }
                }
                else
                {
                    string resourcePath = destination.ResourcePath;
                    UpdateChildrenPath(resourcePath);
                }
            }
            else
            {
                if (destination.ResourceType == "ServerSource")
                {
                    ResourcePath = destination.ResourcePath + (destination.ResourcePath == string.Empty ? "" : "\\") + ResourceName;
                }
            }
        }

        private void UpdateChildrenPath(string resourcePath)
        {
            foreach (var explorerItemViewModel in Children)
            {
                explorerItemViewModel.ResourcePath = resourcePath + "\\" + explorerItemViewModel.ResourceName;
            }
        }

        public void ShowErrorMessage(string errorMessage, string header)
        {
            var a = new PopupMessage
            {
                Buttons = MessageBoxButton.OK,
                Description = errorMessage,
                Header = header,
                Image = MessageBoxImage.Error
            };
            ShellViewModel.ShowPopup(a);
        }

        public bool CanDrop
        {
            get => IsFolder && _candrop;
            set
            {
                _candrop = value;
                OnPropertyChanged(() => CanDrop);
            }
        }
        public bool CanDrag
        {
            get => _canDrag && !IsResourceVersion;
            set
            {
                _canDrag = value;
                OnPropertyChanged(() => CanDrag);
            }
        }

        public ICommand DeleteVersionCommand { get; set; }
        public ICommand ShowDependenciesCommand { get; set; }

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

        public ICommand Expand { get; set; }

        public void Filter(string filter)
        {
            _filter = filter;

            foreach (var explorerItemViewModel in _children)
            {
                explorerItemViewModel.Filter(filter);
            }
            if (string.IsNullOrEmpty(filter) || (_children.Count > 0 && _children.Any(model => model.IsVisible && !model.IsResourceVersion)))
            {
                IsVisible = true;
            }
            else
            {
                if (!string.IsNullOrEmpty(ResourceName) && !IsResourceVersion)
                {
                    ValidateIsVisible(filter);
                }
            }
            ValidateFolderExpand(filter);
            IsSelected = false;
            OnPropertyChanged(() => Children);
        }

        private void ValidateIsVisible(string filter)
        {
            IsVisible = ResourceName.ToLowerInvariant().Contains(filter.ToLowerInvariant());

            if (IsVersion)
            {
                IsVisible = Parent.ResourceName.ToLowerInvariant().Contains(filter.ToLowerInvariant());
            }
        }

        private void ValidateFolderExpand(string filter)
        {
            if (!string.IsNullOrEmpty(filter))
            {
                IsExpanded = IsFolder;
            }
            else
            {
                if (IsFolder)
                {
                    IsExpanded = false;
                }
            }
        }

        public string Inputs { get; set; }
        public string Outputs { get; set; }
        public string ExecuteToolTip => Resources.Languages.Tooltips.ExplorerItemExecuteToolTip;
        public string EditToolTip => Resources.Languages.Tooltips.ExplorerItemEditToolTip;
        public string ResourcePath { get; set; }
        public IServer Server
        {
            get => _server ?? CustomContainer.Get<IServerRepository>().FindSingle(model => model.EnvironmentID == Server.EnvironmentID);
            set
            {
                _server = value;
            }
        }

        public void Dispose()
        {
            if (Children != null)
            {
                foreach (var explorerItemViewModel in Children)
                {
                    explorerItemViewModel?.Dispose();
                }
            }
        }

        public bool IsDependenciesVisible
        {
            get => _isDependenciesVisible && !IsSaveDialog;
            set
            {
                _isDependenciesVisible = value;
                OnPropertyChanged(() => IsDependenciesVisible);
            }
        }

        public bool IsScheduleVisible
        {
            get => _isScheduleVisible && !IsSaveDialog;
            set
            {
                _isScheduleVisible = value;
                OnPropertyChanged(() => IsScheduleVisible);
            }
        }

        public bool IsDuplicateVisible
        {
            get => _isDuplicateVisible && !IsSaveDialog;
            set
            {
                _isDuplicateVisible = value;
                OnPropertyChanged(() => IsDuplicateVisible);
            }
        }

        public bool IsViewSwaggerVisible
        {
            get => _isViewSwaggerVisible && !IsSaveDialog;
            set
            {
                _isViewSwaggerVisible = value;
                OnPropertyChanged(() => IsViewSwaggerVisible);
            }
        }

        public bool IsShowVersionHistoryVisible
        {
            get => _isShowVersionHistoryVisible && !IsSaveDialog;
            set
            {
                _isShowVersionHistoryVisible = value;
                OnPropertyChanged(() => IsShowVersionHistoryVisible);
            }
        }

        public bool IsRollbackVisible
        {
            get => _isRollbackVisible && !IsSaveDialog;
            set
            {
                _isRollbackVisible = value;
                OnPropertyChanged(() => IsRollbackVisible);
            }
        }

        public bool IsOpenVersionVisible
        {
            get => _isOpenVersionVisible && !IsSaveDialog;
            set
            {
                _isOpenVersionVisible = value;
                OnPropertyChanged(() => IsOpenVersionVisible);
            }
        }

        public bool IsNewFolderVisible
        {
            get => _isNewFolderVisible;
            set
            {
                _isNewFolderVisible = value;
                OnPropertyChanged(() => IsNewFolderVisible);
            }
        }

        public bool IsCreateTestVisible
        {
            get => _isCreateTestVisible && !IsSaveDialog;
            set
            {
                _isCreateTestVisible = value;
                OnPropertyChanged(() => IsCreateTestVisible);
            }
        }

        public bool IsRunAllTestsVisible
        {
            get => _isRunAllTestsVisible && !IsSaveDialog;
            set
            {
                _isRunAllTestsVisible = value;
                OnPropertyChanged(() => IsRunAllTestsVisible);
            }
        }

        public bool IsViewJsonApisVisible
        {
            get => _isViewJsonApisVisible && !IsSaveDialog;
            set
            {
                _isViewJsonApisVisible = value;
                OnPropertyChanged(() => IsViewJsonApisVisible);
            }
        }

        public bool IsDebugInputsVisible
        {
            get => _isDebugInputsVisible && !IsSaveDialog;
            set
            {
                _isDebugInputsVisible = value;
                OnPropertyChanged(() => IsDebugInputsVisible);
            }
        }

        public bool IsDebugStudioVisible
        {
            get => _isDebugStudioVisible && !IsSaveDialog;
            set
            {
                _isDebugStudioVisible = value;
                OnPropertyChanged(() => IsDebugStudioVisible);
            }
        }

        public bool IsDebugBrowserVisible
        {
            get => _isDebugBrowserVisible && !IsSaveDialog;
            set
            {
                _isDebugBrowserVisible = value;
                OnPropertyChanged(() => IsDebugBrowserVisible);
            }
        }
    }
}