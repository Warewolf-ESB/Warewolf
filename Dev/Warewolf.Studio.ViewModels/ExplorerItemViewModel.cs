/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Security;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Common.Interfaces.Threading;
using Dev2.Common.Interfaces.Versioning;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Microsoft.Practices.Prism.Mvvm;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Studio.Core;
using Warewolf.Studio.Core.Popup;
// ReSharper disable NonReadonlyMemberInGetHashCode
// ReSharper disable NonLocalizedString
// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable NotAccessedField.Local

namespace Warewolf.Studio.ViewModels
{
    public class ExplorerItemViewModel : BindableBase, IExplorerItemViewModel, IEquatable<ExplorerItemViewModel>
    {

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

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
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

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// true if the specified object  is equal to the current object; otherwise, false.
        /// </returns>
        /// <param name="obj">The object to compare with the current object. </param>
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

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
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
        string _filter;
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
        private IEnvironmentModel _environmentModel;
        private readonly ExplorerItemViewModelCommandController _explorerItemViewModelCommandController;
        private bool _forcedRefresh;
        private string _deployResourceCheckboxTooltip;
        private bool _isService;
        private bool _isFolder;
        private bool _canDuplicate;
        private bool _canCreateTest;
        private bool _canViewRunAllTests;
        private string _openVersionTooltip;
        private string _newServiceTooltip;
        private string _newServerSourceTooltip;
        private string _newDataBaseSourceTooltip;
        private string _newWebSourceTooltip;
        private string _newPluginSourceTooltip;
        private string _newComPluginSourceTooltip;
        private string _newEmailSourceTooltip;
        private string _newExchangeSourceTooltip;
        private string _newRabbitMqSourceTooltip;
        private string _newDropboxSourceTooltip;
        private string _newSharepointSourceTooltip;
        private string _debugInputsTooltip;
        private string _debugStudioTooltip;
        private string _debugBrowserTooltip;
        private string _scheduleTooltip;
        private string _newFolderTooltip;
        private string _renameTooltip;
        private string _deleteTooltip;
        private string _duplicateTooltip;
        private string _createTestTooltip;
        private string _runAllTestsTooltip;
        private string _deployTooltip;
        private string _dependenciesTooltip;
        private string _viewSwaggerTooltip;
        private string _viewApisJsonTooltip;
        private string _showHideVersionsTooltip;
        private string _rollbackTooltip;
        private string _openTooltip;
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

        public ExplorerItemViewModel(IServer server, IExplorerTreeItem parent, Action<IExplorerItemViewModel> selectAction, IShellViewModel shellViewModel, IPopupController popupController)
        {
            VerifyArgument.AreNotNull(new Dictionary<string, object> { { "server", server }, });

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
                ForcedRefresh = true;
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
            LostFocus = new DelegateCommand(o=>LostFocusCommand());
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
                _explorerItemViewModelCommandController.ViewApisJsonCommand(ResourcePath, EnvironmentModel.Connection.WebServerUri);
            });

            NewServerCommand = new DelegateCommand(o =>
            {
                _explorerItemViewModelCommandController.NewServerSourceCommand(ResourcePath, Server);
            });

            NewDatabaseSourceCommand = new DelegateCommand(o =>
            {
                _explorerItemViewModelCommandController.NewDatabaseSourceCommand(ResourcePath, Server);
            });

            NewPluginSourceCommand = new DelegateCommand(o =>
            {
                _explorerItemViewModelCommandController.NewPluginSourceCommand(ResourcePath, Server);
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

            NewRabbitMQSourceSourceCommand = new DelegateCommand(o =>
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
            ShowDependenciesCommand = new DelegateCommand(o=>ShowDependencies());
            ShowVersionHistory = new DelegateCommand(o => AreVersionsVisible = !AreVersionsVisible);
            DeleteCommand = new DelegateCommand(o=>Delete());
            DuplicateCommand = new DelegateCommand(o=>DuplicateResource(), o => CanDuplicate);
            CreateTestCommand = new DelegateCommand(o=>CreateTest(), o => CanCreateTest);
            OpenVersionCommand = new DelegateCommand(o=>OpenVersion());
            VersionHeader = "Show Version History";
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
            CreateFolderCommand = new DelegateCommand(o=>CreateNewFolder());
            DeleteVersionCommand = new DelegateCommand(o=>DeleteVersion());
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
            _explorerItemViewModelCommandController.ShowDependenciesCommand(ResourceId, Server);
        }

        private void OpenVersion()
        {
            _explorerItemViewModelCommandController.OpenCommand(this, Server);
        }
        void DeleteVersion()
        {
            _explorerItemViewModelCommandController.DeleteVersionCommand(_explorerRepository, this, Parent, ResourceName);
        }

        public string ActivityName => typeof(DsfActivity).AssemblyQualifiedName;



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
            tempChildren.Insert(0, child);
            _children = tempChildren;
            OnPropertyChanged(() => Children);
        }

        public void RemoveChild(IExplorerItemViewModel child)
        {
            var tempChildren = new ObservableCollection<IExplorerItemViewModel>(_children);
            tempChildren.Remove(child);
            _children = tempChildren;
            OnPropertyChanged(() => Children);
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
            get { return _isSource; }
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
            IsDuplicateVisible = _isService;

            IsDebugInputsVisible = _isService;
            IsDebugStudioVisible = _isService;
            IsDebugBrowserVisible = _isService;

            IsShowVersionHistoryVisible = _isService;
            IsRollbackVisible = _isService;
            IsOpenVersionVisible = _isService;
            IsDependenciesVisible = _isService;
            IsScheduleVisible = _isService;

            CanViewApisJson = (_isFolder || _isService) && _canView;
            CanViewSwagger = _isService && _canView;
        }

        public bool IsService
        {
            get
            {
                return _isService; 
            }
            set
            {
                _isService = value;
                SetContextMenuVisibility();
                OnPropertyChanged(() => IsService);
            }
        }

        public bool IsFolder
        {
            get
            {
                return _isFolder; 
            }
            set
            {
                _isFolder = value;
                SetContextMenuVisibility();
                OnPropertyChanged(() => IsFolder);
            }
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
                _explorerItemViewModelCommandController.CreateFolderCommand(_explorerRepository, ResourcePath, name, id);
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
                if (!String.IsNullOrEmpty(ResourceName) && !IsResourceVersion)
                {
                    IsVisible = filter(this);
                }
            }
            OnPropertyChanged(() => Children);
        }

        public bool CanMove { get; private set; }

        public IEnumerable<IExplorerItemViewModel> AsList()
        {
            if (Children != null)
            {
                return Children.Union(Children.SelectMany(a => a.AsList()));
            }
            return new List<IExplorerItemViewModel>();
        }

        string GetChildNameFromChildren()
        {
            int count = 0;
            string folderName = Resources.Languages.Core.NewFolderLabel;
            while (Children.Any(a => a.ResourceName == folderName))
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
            _explorerItemViewModelCommandController.DeleteCommand(EnvironmentModel, Parent, _explorerRepository, this, _popupController, Server);
        }
        

        public void SetPermissions(Permissions explorerItemPermissions, bool isDeploy = false)
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
                if (isDeploy)
                {
                    CanEdit = false;
                }
            }
        }

        public void SetPermission(Permissions permission, bool isDeploy = false)
        {
            SetNonePermissions();
            if (permission.HasFlag(Permissions.DeployFrom))
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
            CanDebugStudio = true;
            CanDebugBrowser = true;
        }

        private void SetViewPermissions(bool isDeploy)
        {
            CanView = !isDeploy;
            CanShowDependencies = true;
            CanShowVersions = true;
            CanViewApisJson = true;
            CanViewSwagger = true;            
        }

        private void SetNonePermissions()
        {
            CanRename = false;
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
            get
            {
                return _userShouldEditValueNow;
            }
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
            get
            {
                return _isRenaming;
            }
            set
            {
                _isRenaming = value;
                UserShouldEditValueNow = _isRenaming;
                OnPropertyChanged(() => IsRenaming);
            }
        }

        public string ResourceName
        {
            get { return _resourceName; }
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
                            var asyncWorker = CustomContainer.Get<IAsyncWorker>();
                            asyncWorker?.Start(() =>
                            {
                                ShellViewModel.SetRefreshExplorerState(true);
                                if (_explorerRepository.Rename(this, NewName(newName)))
                                {
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
                                }
                            }, () =>
                            {
                                if (!IsRenaming)
                                {
                                    _resourceName = newName;
                                }
                                IsRenaming = false;
                                OnPropertyChanged(() => ResourceName);
                                ShellViewModel.SetRefreshExplorerState(false);
                            });
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
            var hasDuplicate = Children.Any(model => model.ResourceName.ToLower() == requestedServiceName.ToLower() && model.ResourceType == "Folder");
            return hasDuplicate;
        }

        string NewName(string value)
        {
            return value;
        }

        public ObservableCollection<IExplorerItemViewModel> Children
        {
            get
            {
                return _children == null ? new AsyncObservableCollection<IExplorerItemViewModel>() : new AsyncObservableCollection<IExplorerItemViewModel>(_children.Where(a => a.IsVisible));
            }
            set
            {
                _children = value;
                OnPropertyChanged(() => Children);
                OnPropertyChanged(() => ChildrenCount);
            }
        }
        public bool Checked { get; set; }
        public Guid ResourceId { get; set; }
        public string ResourceType
        {
            get
            {
                return _resourceType;
            }
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
        public ICommand ViewApisJsonCommand { get; set; }

        public ICommand DebugCommand => new DelegateCommand(o =>
        {
            _explorerItemViewModelCommandController.OpenCommand(this, Server);
            ShellViewModel.Debug();
        });

        public bool IsExpanderVisible
        {
            get
            {
                return Children.Count > 0 && !AreVersionsVisible;
            }
            set
            {
                _isVisible = value;
                OnPropertyChanged(() => IsExpanderVisible);
            }
        }
        public ICommand NewServiceCommand { get; set; }
        public ICommand NewServerCommand { get; set; }
        public ICommand NewDatabaseSourceCommand { get; set; }
        public ICommand NewPluginSourceCommand { get; set; }
        public ICommand NewWebSourceSourceCommand { get; set; }
        public ICommand NewEmailSourceSourceCommand { get; set; }
        public ICommand NewExchangeSourceSourceCommand { get; set; }
        public ICommand NewRabbitMQSourceSourceCommand { get; set; }
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
            get
            {
                return Parent?.ForcedRefresh ?? _forcedRefresh;
            }
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
                if (_isSelected != value)
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
        }
        public bool CanShowServerVersion { get; set; }
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

        public bool? IsResourceUnchecked
        {
            get
            {
                return !_isResource;
            }
            set
            {
                _isResource = !value;
                OnPropertyChanged(() => IsResourceChecked);
                OnPropertyChanged(() => IsResourceUnchecked);
            }
        }

        public bool? IsResourceChecked
        {
            get { return _isResource; }
            set
            {
                if (IsFolder)
                {
                    if (ChildrenCount >= 1)
                    {
                        Children.Apply(a => a.IsResourceChecked = value ?? false);
                        _isResource = value ?? false;
                        if (Parent.IsFolder)
                            Parent.IsFolderChecked = value;
                    }
                }
                else
                {
                    IsResourceCheckedEnabled = CanDeploy;
                    _isResource = value.HasValue && !IsFolder && value.Value;
                }
                if (value != null && (bool)value)
                {
                    IsSelected = true;
                }
                else
                {
                    IsSelected = false;
                }
                SelectAction?.Invoke(this);
                OnPropertyChanged(() => IsResourceChecked);
            }
        }

        public bool IsResourceCheckedEnabled
        {
            get { return CanDeploy; }
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
            get { return _deployResourceCheckboxTooltip; }
            set
            {
                _deployResourceCheckboxTooltip = value;
                OnPropertyChanged(() => DeployResourceCheckboxTooltip);
            }
        }

        public bool? IsFolderChecked
        {
            get
            {
                return _isResource;
            }
            // ReSharper disable once ValueParameterNotUsed
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
                    if (!_isResource.HasValue || _isResource.Value)
                    {
                        if (Parent.IsFolder)
                            Parent.IsFolderChecked = _isResource;
                    }
                }
                OnPropertyChanged(() => IsResourceChecked);
            }
        }

        public ICommand RenameCommand { get; set; }

        public bool CanCreateSource
        {
            get { return _canCreateSource; }
            set
            {
                _canCreateSource = value;

                NewServerSourceTooltip = _canCreateSource ? Resources.Languages.Core.NewServerSourceTooltip : Resources.Languages.Core.NoPermissionsToolTip;
                NewDataBaseSourceTooltip = _canCreateSource ? Resources.Languages.Core.NewDataBaseSourceTooltip : Resources.Languages.Core.NoPermissionsToolTip;
                NewWebSourceTooltip = _canCreateSource ? Resources.Languages.Core.NewWebSourceTooltip : Resources.Languages.Core.NoPermissionsToolTip;
                NewPluginSourceTooltip = _canCreateSource ? Resources.Languages.Core.NewPluginSourceTooltip : Resources.Languages.Core.NoPermissionsToolTip;
                NewComPluginSourceTooltip = _canCreateSource ? Resources.Languages.Core.NewComPluginSourceTooltip : Resources.Languages.Core.NoPermissionsToolTip;
                NewEmailSourceTooltip = _canCreateSource ? Resources.Languages.Core.NewEmailSourceTooltip : Resources.Languages.Core.NoPermissionsToolTip;
                NewExchangeSourceTooltip = _canCreateSource ? Resources.Languages.Core.NewExchangeSourceTooltip : Resources.Languages.Core.NoPermissionsToolTip;
                NewRabbitMqSourceTooltip = _canCreateSource ? Resources.Languages.Core.NewRabbitMqSourceTooltip : Resources.Languages.Core.NoPermissionsToolTip;
                NewDropboxSourceTooltip = _canCreateSource ? Resources.Languages.Core.NewDropboxSourceTooltip : Resources.Languages.Core.NoPermissionsToolTip;
                NewSharepointSourceTooltip = _canCreateSource ? Resources.Languages.Core.NewSharepointSourceTooltip : Resources.Languages.Core.NoPermissionsToolTip;

                OnPropertyChanged(() => CanCreateSource);
            }
        }

        public bool CanViewSwagger
        {
            get { return _canViewSwagger; }
            set
            {
                _canViewSwagger = value;

                ViewSwaggerTooltip = _canViewSwagger ? Resources.Languages.Core.ViewSwaggerToolTip : Resources.Languages.Core.NoPermissionsToolTip;
                OnPropertyChanged(() => CanViewSwagger);
            }
        }

        public bool CanViewApisJson
        {
            get { return _canViewApisJson; }
            set
            {
                _canViewApisJson = value;
                ViewApisJsonTooltip = _canViewApisJson ? Resources.Languages.Core.ViewApisJsonTooltip : Resources.Languages.Core.NoPermissionsToolTip;
                OnPropertyChanged(() => CanViewApisJson);
            }
        }

        // ReSharper disable MemberCanBePrivate.Global
        public bool CanCreateWorkflowService
        {
            get { return _canCreateWorkflowService; }
            set
            {
                _canCreateWorkflowService = value;
                NewServiceTooltip = _canCreateWorkflowService ? Resources.Languages.Core.NewServiceTooltip : Resources.Languages.Core.NoPermissionsToolTip;
                OnPropertyChanged(() => CanCreateWorkflowService);
            }
        }

        public bool ShowContextMenu { get; set; }
        // ReSharper restore MemberCanBePrivate.Global

        public bool CanRename
        {
            get
            {
                return _canRename;
            }
            set
            {
                _canRename = value;
                if (_isFolder)
                {
                    RenameTooltip = _canRename ? Resources.Languages.Core.RenameFolderTooltip : Resources.Languages.Core.NoPermissionsToolTip;
                }
                else
                {
                    RenameTooltip = _canRename ? Resources.Languages.Core.RenameItemTooltip : Resources.Languages.Core.NoPermissionsToolTip;
                }
                OnPropertyChanged(() => CanRename);
            }
        }
        public bool CanDuplicate
        {
            get
            {
                return _canDuplicate;
            }
            set
            {
                _canDuplicate = value;
                DuplicateTooltip = _canDuplicate ? Resources.Languages.Core.DuplicateToolTip : Resources.Languages.Core.NoPermissionsToolTip;
                OnPropertyChanged(() => CanDuplicate);
            }
        }
        public bool CanCreateTest
        {
            get
            {
                return _canCreateTest && IsService;
            }
            set
            {
                _canCreateTest = value;
                CreateTestTooltip = _canCreateTest ? Resources.Languages.Core.TestEditorToolTip : Resources.Languages.Core.NoPermissionsToolTip;
                OnPropertyChanged(() => CanCreateTest);
            }
        }
        public bool CanViewRunAllTests
        {
            get
            {
                return _canViewRunAllTests && IsService;
            }
            set
            {
                _canViewRunAllTests = value;
                RunAllTestsTooltip = _canViewRunAllTests ? Resources.Languages.Core.RunAllTestsToolTip : Resources.Languages.Core.NoPermissionsToolTip;
                OnPropertyChanged(() => CanCreateTest);
            }
        }
    
        public bool CanDelete
        {
            get
            {
                return _canDelete;
            }
            set
            {
                _canDelete = value;
                if (_isFolder)
                {
                    DeleteTooltip = _canDelete ? Resources.Languages.Core.DeleteFolderTooltip : Resources.Languages.Core.NoPermissionsToolTip;
                }
                else
                {
                    DeleteTooltip = _canDelete ? Resources.Languages.Core.DeleteItemTooltip : Resources.Languages.Core.NoPermissionsToolTip;
                }
                
                OnPropertyChanged(() => CanDelete);
            }
        }
        public bool CanCreateFolder
        {
            get
            {
                return (IsFolder || IsServer) && _canCreateFolder;
            }
            set
            {
                _canCreateFolder = value;
                NewFolderTooltip = _canCreateFolder ? Resources.Languages.Core.NewFolderTooltip : Resources.Languages.Core.NoPermissionsToolTip;
                OnPropertyChanged(() => CanCreateFolder);
            }
        }

        public bool CanDebugInputs
        {
            get
            {
                return _canDebugInputs;
            }
            set
            {
                _canDebugInputs = value;
                DebugInputsTooltip = _canDebugInputs ? Resources.Languages.Core.DebugInputsToolTip : Resources.Languages.Core.NoPermissionsToolTip;
                OnPropertyChanged(() => CanDebugInputs);
            }
        }

        public bool CanDebugStudio
        {
            get
            {
                return _canDebugStudio;
            }
            set
            {
                _canDebugStudio = value;
                DebugStudioTooltip = _canDebugStudio ? Resources.Languages.Core.DebugStudioToolTip : Resources.Languages.Core.NoPermissionsToolTip;
                OnPropertyChanged(() => CanDebugStudio);
            }
        }

        public bool CanDebugBrowser
        {
            get
            {
                return _canDebugBrowser;
            }
            set
            {
                _canDebugBrowser = value;
                DebugBrowserTooltip = _canDebugBrowser ? Resources.Languages.Core.DebugBrowserToolTip : Resources.Languages.Core.NoPermissionsToolTip;
                OnPropertyChanged(() => CanDebugBrowser);
            }
        }

        public bool CanCreateSchedule
        {
            get { return _canCreateSchedule; }
            set
            {
                _canCreateSchedule = value;
                ScheduleTooltip = _canCreateSchedule ? Resources.Languages.Core.ScheduleToolTip : Resources.Languages.Core.NoPermissionsToolTip;
                OnPropertyChanged(() => CanCreateSchedule);
            }
        }
        
        public bool CanDeploy
        {
            get
            {
                return _canDeploy;
            }
            set
            {
                _canDeploy = value;
                if(!_canDeploy)
                    IsResourceChecked = _canDeploy;
                IsResourceCheckedEnabled = _canDeploy;

                DeployTooltip = _canDeploy ? Resources.Languages.Core.DeployToolTip : Resources.Languages.Core.NoPermissionsToolTip;
            }
        }

        public IServer Server
        {
            get;
            set;
        }
        public ICommand LostFocus { get; set; }
        public bool CanExecute
        {
            get
            {
                return _canExecute;
            }
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
            get
            {
                return _canEdit;
            }
            set
            {
                if (_canEdit != value)
                {
                    _canEdit = value;
                    OnPropertyChanged(() => CanEdit);
                }
            }
        }
        public bool CanView
        {
            get
            {
                return _canView && !IsResourceVersion;
            }
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
            get
            {
                return _canShowDependencies && !IsFolder;
            }
            set
            {
                _canShowDependencies = value;
                DependenciesTooltip = _canShowDependencies ? Resources.Languages.Core.DependenciesToolTip : Resources.Languages.Core.NoPermissionsToolTip;
                OnPropertyChanged(() => CanShowDependencies);
            }
        }

        public bool IsVersion
        {
            get { return _isVersion; }
            set
            {
                _isVersion = value;
                OpenVersionTooltip = _isVersion ? Resources.Languages.Core.OpenVersionTooltip : Resources.Languages.Core.NoPermissionsToolTip;
                RollbackTooltip = _isVersion ? Resources.Languages.Core.RollbackTooltip : Resources.Languages.Core.NoPermissionsToolTip;
                OnPropertyChanged(() => IsVersion);
            }
        }

        public bool CanShowVersions
        {
            get
            {
                return IsService && _canShowVersions;
            }
            set
            {
                _canShowVersions = value;
                ShowHideVersionsTooltip = _canShowVersions ? Resources.Languages.Core.ShowHideVersionsTooltip : Resources.Languages.Core.NoPermissionsToolTip;
                OnPropertyChanged(() => CanShowVersions);
            }
        }
        public bool CanRollback => IsResourceVersion;

        public IShellViewModel ShellViewModel => _shellViewModel;

        public bool AreVersionsVisible
        {
            get
            {
                return _areVersionsVisible;
            }
            set
            {
                _areVersionsVisible = value;
                VersionHeader = !value ? "Show Version History" : "Hide Version History";
                if (value)
                {
                    _children = new ObservableCollection<IExplorerItemViewModel>(_explorerRepository.GetVersions(ResourceId).Select(a => new VersionViewModel(Server, this, null, _shellViewModel, _popupController)
                    {
                        ResourceName = "v." + a.VersionNumber + " " + a.DateTimeStamp.ToString(CultureInfo.InvariantCulture) + " " + a.Reason.Replace(".xml", ""),
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
                    }
                    ));
                    OnPropertyChanged(() => Children);
                    if (Children.Count > 0) IsExpanded = true;
                }
                else
                {
                    _children = new ObservableCollection<IExplorerItemViewModel>();
                    OnPropertyChanged(() => Children);
                }
                OnPropertyChanged(() => AreVersionsVisible);
            }
        }
        public IVersionInfo VersionInfo
        {
            get
            {
                return _versionInfo;
            }
            set
            {
                _versionInfo = value;
                OnPropertyChanged(() => VersionInfo);
            }
        }
        public string VersionNumber
        {
            get
            {
                return _versionNumber;
            }
            set
            {
                _versionNumber = value;
                OnPropertyChanged(() => VersionNumber);
            }
        }
        public string VersionHeader
        {
            get
            {
                return _versionHeader;
            }
            set
            {
                _versionHeader = value;
                OnPropertyChanged(() => VersionHeader);
            }
        }
        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                if (_isVisible != value)
                {
                    _isVisible = value;
                    OnPropertyChanged(() => IsVisible);
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
                if (Parent != null && destination.Parent != null)
                {
                    if (Equals(Parent, destination))
                    {
                        return false;
                    }
                }

                var moveResult = await _explorerRepository.Move(this, destination);
                if (!moveResult)
                {
                    ShowErrorMessage(Resources.Languages.Core.ExplorerMoveFailedMessage, Resources.Languages.Core.ExplorerMoveFailedHeader);
                    return false;
                }
                UpdateResourcePaths(destination);
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex.Message, Resources.Languages.Core.ExplorerMoveFailedHeader);
                return false;
            }
            finally
            {
                Server.UpdateRepository.FireItemSaved(true);
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
                        destfolder.Parent = destination;

                        var resourcePath = destfolder.ResourcePath;
                        foreach (var explorerItemViewModel in Children)
                        {
                            explorerItemViewModel.ResourcePath = resourcePath + "\\" + explorerItemViewModel.ResourceName;
                        }
                    }
                }
                else
                {
                    foreach (var explorerItemViewModel in Children)
                    {
                        explorerItemViewModel.ResourcePath = destination.ResourcePath + "\\" + explorerItemViewModel.ResourceName;
                        explorerItemViewModel.Parent = destination;
                    }
                }
            }
            else if (destination.ResourceType == "ServerSource")
            {
                ResourcePath = destination.ResourcePath + (destination.ResourcePath == string.Empty ? "" : "\\") + ResourceName;
                Parent = destination;
                Parent.Children.Add(this);
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
            get
            {
                return IsFolder && _candrop;
            }
            set
            {
                _candrop = value;
                OnPropertyChanged(() => CanDrop);
            }
        }
        public bool CanDrag
        {
            get
            {
                return _canDrag && (IsSource || IsFolder || IsService) && !IsResourceVersion;
            }
            set
            {
                _canDrag = value;
                OnPropertyChanged(() => CanDrag);
            }
        }

        public ICommand OpenVersionCommand { get; set; }
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
                    IsExpanded = false;
            }
        }

        public string Inputs { get; set; }
        public string Outputs { get; set; }
        public string ExecuteToolTip => Resources.Languages.Core.ExplorerItemExecuteToolTip;
        public string EditToolTip => Resources.Languages.Core.ExplorerItemEditToolTip;
        public string ResourcePath { get; set; }
        public IEnvironmentModel EnvironmentModel
        {
            private get
            {
                return _environmentModel ?? EnvironmentRepository.Instance.FindSingle(model => model.ID == Server.EnvironmentID);
            }
            set
            {
                _environmentModel = value;
            }
        }

        public void Dispose()
        {
            if (Children != null)
                foreach (var explorerItemViewModel in Children)
                {
                    explorerItemViewModel?.Dispose();
                }
        }

        public bool IsDependenciesVisible
        {
            get { return _isDependenciesVisible; }
            set
            {
                _isDependenciesVisible = value; 
                OnPropertyChanged(() => IsDependenciesVisible);
            }
        }

        public bool IsScheduleVisible
        {
            get { return _isScheduleVisible; }
            set
            {
                _isScheduleVisible = value;
                OnPropertyChanged(() => IsScheduleVisible);
            }
        }

        public bool IsDuplicateVisible
        {
            get { return _isDuplicateVisible; }
            set
            {
                _isDuplicateVisible = value;
                OnPropertyChanged(() => IsDuplicateVisible);
            }
        }

        public bool IsViewSwaggerVisible
        {
            get { return _isViewSwaggerVisible; }
            set
            {
                _isViewSwaggerVisible = value;
                OnPropertyChanged(() => IsViewSwaggerVisible);
            }
        }

        public bool IsShowVersionHistoryVisible
        {
            get { return _isShowVersionHistoryVisible; }
            set
            {
                _isShowVersionHistoryVisible = value;
                OnPropertyChanged(() => IsShowVersionHistoryVisible);
            }
        }

        public bool IsRollbackVisible
        {
            get { return _isRollbackVisible; }
            set
            {
                _isRollbackVisible = value;
                OnPropertyChanged(() => IsRollbackVisible);
            }
        }

        public bool IsOpenVersionVisible
        {
            get { return _isOpenVersionVisible; }
            set
            {
                _isOpenVersionVisible = value;
                OnPropertyChanged(() => IsOpenVersionVisible);
            }
        }

        public bool IsNewFolderVisible
        {
            get { return _isNewFolderVisible; }
            set
            {
                _isNewFolderVisible = value;
                OnPropertyChanged(() => IsNewFolderVisible);
            }
        }

        public bool IsCreateTestVisible
        {
            get { return _isCreateTestVisible; }
            set
            {
                _isCreateTestVisible = value;
                OnPropertyChanged(() => IsCreateTestVisible);
            }
        }

        public bool IsRunAllTestsVisible
        {
            get { return _isRunAllTestsVisible; }
            set
            {
                _isRunAllTestsVisible = value;
                OnPropertyChanged(() => IsRunAllTestsVisible);
            }
        }

        public bool IsViewJsonApisVisible
        {
            get { return _isViewJsonApisVisible; }
            set
            {
                _isViewJsonApisVisible = value;
                OnPropertyChanged(() => IsViewJsonApisVisible);
            }
        }

        public bool IsDebugInputsVisible
        {
            get { return _isDebugInputsVisible; }
            set
            {
                _isDebugInputsVisible = value;
                OnPropertyChanged(() => IsDebugInputsVisible);
            }
        }

        public bool IsDebugStudioVisible
        {
            get { return _isDebugStudioVisible; }
            set
            {
                _isDebugStudioVisible = value;
                OnPropertyChanged(() => IsDebugStudioVisible);
            }
        }

        public bool IsDebugBrowserVisible
        {
            get { return _isDebugBrowserVisible; }
            set
            {
                _isDebugBrowserVisible = value;
                OnPropertyChanged(() => IsDebugBrowserVisible);
            }
        }

        public string OpenVersionTooltip
        {
            get { return _openVersionTooltip; }
            set
            {
                _openVersionTooltip = value; 
                OnPropertyChanged(() => OpenVersionTooltip);
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
        public string NewDataBaseSourceTooltip
        {
            get { return _newDataBaseSourceTooltip; }
            set
            {
                _newDataBaseSourceTooltip = value; 
                OnPropertyChanged(() => NewDataBaseSourceTooltip);
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
        public string DebugInputsTooltip
        {
            get { return _debugInputsTooltip; }
            set
            {
                _debugInputsTooltip = value; 
                OnPropertyChanged(() => DebugInputsTooltip);
            }
        }
        public string DebugStudioTooltip
        {
            get { return _debugStudioTooltip; }
            set
            {
                _debugStudioTooltip = value; 
                OnPropertyChanged(() => DebugStudioTooltip);
            }
        }
        public string DebugBrowserTooltip
        {
            get { return _debugBrowserTooltip; }
            set
            {
                _debugBrowserTooltip = value; 
                OnPropertyChanged(() => DebugBrowserTooltip);
            }
        }
        public string ScheduleTooltip
        {
            get { return _scheduleTooltip; }
            set
            {
                _scheduleTooltip = value; 
                OnPropertyChanged(() => ScheduleTooltip);
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
        public string RenameTooltip
        {
            get { return _renameTooltip; }
            set
            {
                _renameTooltip = value; 
                OnPropertyChanged(() => RenameTooltip);
            }
        }
        public string DeleteTooltip
        {
            get { return _deleteTooltip; }
            set
            {
                _deleteTooltip = value; 
                OnPropertyChanged(() => DeleteTooltip);
            }
        }
        public string DuplicateTooltip
        {
            get { return _duplicateTooltip; }
            set
            {
                _duplicateTooltip = value; 
                OnPropertyChanged(() => DuplicateTooltip);
            }
        }
        public string CreateTestTooltip
        {
            get { return _createTestTooltip; }
            set
            {
                _createTestTooltip = value; 
                OnPropertyChanged(() => CreateTestTooltip);
            }
        }
        public string RunAllTestsTooltip
        {
            get { return _runAllTestsTooltip; }
            set
            {
                _runAllTestsTooltip = value; 
                OnPropertyChanged(() => RunAllTestsTooltip);
            }
        }
        public string DeployTooltip
        {
            get { return _deployTooltip; }
            set
            {
                _deployTooltip = value; 
                OnPropertyChanged(() => DeployTooltip);
            }
        }
        public string DependenciesTooltip
        {
            get { return _dependenciesTooltip; }
            set
            {
                _dependenciesTooltip = value; 
                OnPropertyChanged(() => DependenciesTooltip);
            }
        }
        public string ViewSwaggerTooltip
        {
            get { return _viewSwaggerTooltip; }
            set
            {
                _viewSwaggerTooltip = value; 
                OnPropertyChanged(() => ViewSwaggerTooltip);
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
        public string ShowHideVersionsTooltip
        {
            get { return _showHideVersionsTooltip; }
            set
            {
                _showHideVersionsTooltip = value; 
                OnPropertyChanged(() => ShowHideVersionsTooltip);
            }
        }
        public string RollbackTooltip
        {
            get { return _rollbackTooltip; }
            set
            {
                _rollbackTooltip = value; 
                OnPropertyChanged(() => RollbackTooltip);
            }
        }
        public string OpenTooltip
        {
            get { return _openTooltip; }
            set
            {
                _openTooltip = value; 
                OnPropertyChanged(() => OpenTooltip);
            }
        }
    }
}