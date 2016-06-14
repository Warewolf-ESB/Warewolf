
/*
*  Warewolf - The Easy Service Bus
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
using Dev2.Activities;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Common.Interfaces.Versioning;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Studio.Core;
using Warewolf.Studio.Core.Popup;

namespace Warewolf.Studio.ViewModels
{

    public class ExplorerItemViewModel : BindableBase, IExplorerItemViewModel, IEquatable<ExplorerItemViewModel>
    {
        private IDeletedFileMetadata _fileMetadata;

        #region Equality members

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
            return ResourceId.Equals(other.ResourceId);
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
            return ResourceId.GetHashCode();
        }

        public static bool operator ==(ExplorerItemViewModel left, ExplorerItemViewModel right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ExplorerItemViewModel left, ExplorerItemViewModel right)
        {
            return !Equals(left, right);
        }

        #endregion

        public Action<IExplorerItemViewModel> SelectAction { get; set; }
        string _resourceName;
        private bool _isVisible;
        bool _allowEditing;
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
        bool _canCreateServerSource;
        bool _canCreateFolder;
        string _filter;
        private bool _isSelected;
        bool _canShowVersions;
        readonly IShellViewModel _shellViewModel;
        readonly Dictionary<string, Type> _activityNames;
        bool _canShowDependencies;
        bool _allowResourceCheck;
        bool _candrop;
        bool _canDrag;
        bool? _isResource;
        readonly IPopupController _popupController;
        IVersionInfo _versionInfo;
        private IEnvironmentModel _environmentModel;

        public ExplorerItemViewModel(IDeletedFileMetadata fileMetadata)
        {
            _fileMetadata = fileMetadata;
        }

        public ExplorerItemViewModel(IServer server, IExplorerTreeItem parent, Action<IExplorerItemViewModel> selectAction, IShellViewModel shellViewModel, IPopupController popupController)
            : this(new DeletedFileMetadata())
        {
            SelectAction = selectAction;
            _shellViewModel = shellViewModel;
            _popupController = popupController;
            RollbackCommand = new DelegateCommand(() =>
            {
                var output = _explorerRepository.Rollback(ResourceId, VersionNumber);
                parent.AreVersionsVisible = true;
                parent.ResourceName = output.DisplayName;
            });
            DeployCommand = new DelegateCommand<IExplorerItemViewModel>(a => ShellViewModel.AddDeploySurface(AsList().Union(new[] { this })));
            _canShowVersions = true;
            Parent = parent;
            VerifyArgument.AreNotNull(new Dictionary<string, object> { { "server", server }, });
            LostFocus = new DelegateCommand(LostFocusCommand);

            Children = new ObservableCollection<IExplorerItemViewModel>();
            OpenCommand = new DelegateCommand(() =>
            {
                if (ResourceType == "DbService" || ResourceType == "PluginService" || ResourceType == "WebService")
                {
                    return;
                }
                if (IsFolder)
                {
                    IsExpanded = !IsExpanded;
                }
                shellViewModel.SetActiveEnvironment(Server.EnvironmentID);
                shellViewModel.SetActiveServer(Server);
                shellViewModel.OpenResource(ResourceId, Server);
            });
            DebugCommand = new DelegateCommand(() =>
            {
                shellViewModel.OpenResource(ResourceId, Server);
                shellViewModel.Debug();
            });
            RenameCommand = new DelegateCommand(() => IsRenaming = true);
            Server = server;
            NewCommand = new DelegateCommand<string>(type =>
            {
                shellViewModel.SetActiveEnvironment(Server.EnvironmentID);
                shellViewModel.SetActiveServer(Server);
                shellViewModel.NewResource(type.ToString(), ResourcePath);
            });
            CanShowDependencies = true;
            ShowDependenciesCommand = new DelegateCommand(() =>
            {
                shellViewModel.ShowDependencies(ResourceId, Server);
            });
            AllowResourceCheck = false;
            IsResourceChecked = false;
            _explorerRepository = server.ExplorerRepository;
            Server.PermissionsChanged += UpdatePermissions;
            if (Server.Permissions != null)
            {
                SetPermissions(Server.Permissions);
            }
            ShowVersionHistory = new DelegateCommand(() => AreVersionsVisible = !AreVersionsVisible);
            DeleteCommand = new DelegateCommand(() =>
            {
                if (IsResourceVersion)
                {
                    DeleteVersion();
                }
                else Delete();
            });
            OpenVersionCommand = new DelegateCommand(() =>
            {
                if (IsResourceVersion)
                {
                    ShellViewModel.OpenVersion(parent.ResourceId, VersionInfo);
                }
            });
            VersionHeader = "Show Version History";
            //Builder = builder;
            IsVisible = true;
            IsVersion = false;
            Expand = new DelegateCommand<int?>(clickCount =>
            {
                if (clickCount != null && clickCount == 2 && IsFolder)
                {
                    IsExpanded = !IsExpanded;
                }
                if (clickCount != null && clickCount == 2 && ResourceType == "WorkflowService" && IsExpanded)
                {
                    IsExpanded = false;
                }
            });
            CreateFolderCommand = new DelegateCommand(CreateNewFolder);
            DeleteVersionCommand = new DelegateCommand(DeleteVersion);
            CanShowServerVersion = false;

            _activityNames = new Dictionary<string, Type>
                {
                    {
                        "DbService", typeof(DsfDatabaseActivity)
                    },
                    {
                        "PluginService", typeof(DsfPluginActivity)
                    },
                    {
                        "WebService", typeof(DsfWebserviceActivity)
                    }
                };
            _candrop = true;
            _canDrag = true;
        }

        public string ActivityName
        {
            get
            {
                return (_activityNames.ContainsKey(ResourceType) ? _activityNames[ResourceType] : typeof(DsfActivity)).AssemblyQualifiedName;
            }
        }

        void DeleteVersion()
        {
            if (_popupController.ShowDeleteVersionMessage(ResourceName) == MessageBoxResult.Yes)
            {
                _explorerRepository.Delete(this);
                if (Parent != null)
                {
                    Parent.RemoveChild(Parent.Children.First(a => a.ResourceName == ResourceName));
                }
            }
        }

        public IExplorerTreeItem Parent { get; set; }

        public void AddSibling(IExplorerItemViewModel sibling)
        {
            if (Parent != null)
            {
                Parent.AddChild(sibling);
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
                }
                else
                {
                    a.SelectItem(id, foundAction);
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

        public void UpdateChildrenCount()
        {
            OnPropertyChanged(()=>ChildrenCount);
        }


        public void CreateNewFolder()
        {
            if (IsFolder)
            {
                IsExpanded = true;
                var id = Guid.NewGuid();
                var name = GetChildNameFromChildren();
                _explorerRepository.CreateFolder(ResourcePath, name, id);
                var child = new ExplorerItemViewModel(Server, this, SelectAction, _shellViewModel, _popupController)
                {
                    ResourceName = name,
                    ResourceId = id,
                    ResourceType = "Folder",
                    AllowResourceCheck = AllowResourceCheck,
                    IsResourceChecked = IsResourceChecked,
                    CanCreateFolder = CanCreateFolder,
                    CanCreateDbSource = CanCreateDbSource,
                    CanShowVersions = CanShowVersions,
                    CanRename = CanRename,
                    CanCreatePluginSource = CanCreatePluginSource,
                    CanCreateEmailSource = CanCreateEmailSource,
                    CanCreateRabbitMQSource = CanCreateRabbitMQSource,
                    CanCreateExchangeSource = CanCreateExchangeSource,
                    CanCreateDropboxSource = CanCreateDropboxSource,
                    CanCreateSharePointSource = CanCreateSharePointSource,
                    CanCreateServerSource = CanCreateServerSource,
                    CanCreateWebSource = CanCreateWebSource,
                    CanDeploy = CanDeploy,
                    CanShowDependencies = CanShowDependencies,
                    ResourcePath = ResourcePath + "\\" + name,
                    CanCreateWorkflowService = CanCreateWorkflowService,
                    ShowContextMenu = ShowContextMenu,
                    IsSelected = true,
                    IsRenaming = true,
                    IsFolder = true
                };
                //child.SetFromServer(Server.Permissions.FirstOrDefault(a => a.IsServer));
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

        public IExplorerItemViewModel Find(string resourcePath)
        {
            if (!resourcePath.Contains("\\") && resourcePath == ResourceName)
                return this;
            if (Children != null && resourcePath.Contains("\\"))
            {
                string name = resourcePath.Substring(1 + resourcePath.IndexOf("\\", StringComparison.Ordinal));
                return Children.Select(explorerItemViewModel => explorerItemViewModel.Find(name)).FirstOrDefault(item => item != null);
            }
            return null;
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

        void LostFocusCommand()
        {
            IsRenaming = false;
        }

        //IExplorerHelpDescriptorBuilder Builder { get; set; }

        public void Delete()
        {
            try
            {
                var environmentModel = EnvironmentModel;

                if (environmentModel != null &&
                    _popupController.Show(PopupMessages.GetDeleteConfirmation(ResourceName)) == MessageBoxResult.Yes)
                {
                    ShellViewModel.CloseResource(ResourceId, environmentModel.ID);
                    // Remove the item from the parent for studio change to show, then do the delete from the server.
                    if (Parent != null)
                    {
                        Parent.RemoveChild(this);
                    }
                    //This Delete process is quite long and should happen after the studio change so that the user caqn continue without the studio hanging
                    _fileMetadata = _explorerRepository.Delete(this);
                    if (_fileMetadata.IsDeleted)
                    {
                        if (ResourceType == "ServerSource" || IsServer)
                        {
                            Server.UpdateRepository.FireServerSaved();
                        }
                    }
                    else
                    {
                        ResourceId = _fileMetadata.ResourceId;
                        ShellViewModel.ShowDependencies(ResourceId, Server);
                    }
                }
            }
            catch (Exception ex)
            {
                // If the delete did not happen, we need to add the item back to the original state for studio changes to re-occur
                if (Parent != null)
                {
                    Parent.AddChild(this);
                }
                ShowErrorMessage(ex.Message, "Delete not allowed");
            }
        }

        public void UpdatePermissions(PermissionsChangedArgs args)
        {
            SetPermissions(args.WindowsGroupPermissions);
        }

        public void SetPermissions(List<IWindowsGroupPermission> permissions, bool isDeploy = false)
        {
            if (permissions != null)
            {
                var resourcePermission = permissions.FirstOrDefault(permission => permission.ResourceID == ResourceId);
                if (resourcePermission != null)
                {
                    SetFromPermission(resourcePermission, isDeploy);
                }
                else
                {
                    var serverPermission =
                        permissions.FirstOrDefault(permission => permission.IsServer && permission.ResourceID == Guid.Empty);
                    if (serverPermission != null)
                    {
                        SetFromServer(serverPermission, isDeploy);
                    }
                }
            }
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

        void SetFromServer(IWindowsGroupPermission serverPermission, bool isDeploy = false)
        {
            bool containsOldService = ResourceType == "DbService" || ResourceType == "PluginService" || ResourceType == "WebService";

            CanEdit = serverPermission.Contribute && !isDeploy && !containsOldService;
            CanExecute = serverPermission.Execute && IsService && !isDeploy && !containsOldService;
            CanView = serverPermission.View && !isDeploy && !containsOldService;
            CanRename = serverPermission.Contribute || serverPermission.Administrator;
            CanDelete = serverPermission.Contribute || serverPermission.Administrator;
            CanCreateFolder = serverPermission.Contribute || serverPermission.Administrator;
            CanDeploy = serverPermission.DeployFrom || serverPermission.Administrator;
            CanShowVersions = serverPermission.Administrator;
            CanCreateWorkflowService = serverPermission.Contribute;
            CanCreateFolder = serverPermission.Contribute;
            CanCreateDbSource = serverPermission.Contribute;
            CanCreatePluginSource = serverPermission.Contribute;
            CanCreateEmailSource = serverPermission.Contribute;
            CanCreateRabbitMQSource = serverPermission.Contribute;
            CanCreateExchangeSource = serverPermission.Contribute;
            CanCreateDropboxSource = serverPermission.Contribute;
            CanCreateSharePointSource = serverPermission.Contribute;
            CanCreateServerSource = serverPermission.Contribute;
            CanCreateWebSource = serverPermission.Contribute;
        }

        void SetFromPermission(IWindowsGroupPermission resourcePermission, bool isDeploy = false)
        {
            bool containsOldService = ResourceType == "DbService" || ResourceType == "PluginService" || ResourceType == "WebService";

            CanEdit = resourcePermission.Contribute && !isDeploy && !containsOldService;
            CanExecute = resourcePermission.Execute && IsService && !isDeploy && !containsOldService;
            CanView = resourcePermission.View && !isDeploy && !containsOldService;
            CanRename = resourcePermission.Contribute || resourcePermission.Administrator;
            CanDelete = resourcePermission.Contribute || resourcePermission.Administrator;
            CanDeploy = resourcePermission.DeployFrom || resourcePermission.Administrator;
            CanShowVersions = resourcePermission.Administrator;
            CanCreateWorkflowService = resourcePermission.Contribute;
            CanCreateFolder = resourcePermission.Contribute;
            CanCreateDbSource = resourcePermission.Contribute;
            CanCreatePluginSource = resourcePermission.Contribute;
            CanCreateEmailSource = resourcePermission.Contribute;
            CanCreateRabbitMQSource = resourcePermission.Contribute;
            CanCreateExchangeSource = resourcePermission.Contribute;
            CanCreateDropboxSource = resourcePermission.Contribute;
            CanCreateSharePointSource = resourcePermission.Contribute;
            CanCreateServerSource = resourcePermission.Contribute;
            CanCreateWebSource = resourcePermission.Contribute;
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
                OnPropertyChanged(() => IsNotRenaming);
            }
        }

        public bool IsNotRenaming
        {
            get
            {
                return !_isRenaming;
            }
        }
        public string ResourceName
        {
            get
            {
                return _resourceName;
            }
            set
            {
                if (_resourceName != null && Parent != null && Parent.Children.Any(a => a.ResourceName == value) && value != _resourceName)
                {
                    _shellViewModel.ShowPopup(PopupMessages.GetDuplicateMessage(value));
                }
                else
                {
                    var newName = RemoveInvalidCharacters(value);

                    if (_explorerRepository != null && IsRenaming)
                    {
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

                            _resourceName = newName;
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
            if (!ResourcePath.Contains("\\"))
                return value;
            return ResourcePath.Substring(0, 1 + ResourcePath.LastIndexOf('\\')) + value;
        }

        public ObservableCollection<IExplorerItemViewModel> Children
        {
            get
            {
                return String.IsNullOrEmpty(_filter) ? _children : new AsyncObservableCollection<IExplorerItemViewModel>(_children.Where(a => a.IsVisible));
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
                if (Server.Permissions != null && ResourceType != "Version")
                {
                    SetPermissions(Server.Permissions);
                }
            }
        }
        public ICommand OpenCommand
        {
            get;
            set;
        }
        public ICommand DebugCommand
        {
            get;
            set;
        }
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
        public ICommand NewCommand { get; set; }
        public ICommand DeployCommand { get; set; }

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
                return _isResource;
            }
            set
            {
                _isResource = value;
                OnPropertyChanged(() => IsResourceChecked);
            }
        }
        public bool? IsResourceChecked
        {
            get
            {
                return _isResource;
            }
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
                    _isResource = value.HasValue && !IsFolder && value.Value;
                }
                if (SelectAction != null)
                {
                    SelectAction(this);
                }
                OnPropertyChanged(() => IsResourceChecked);
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
                    else if (Children.Any(a => !a.IsResourceChecked.HasValue || a.IsResourceChecked.Value))
                    {
                        _isResource = null;
                    }
                    else
                    {
                        _isResource = false;
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
        // ReSharper disable MemberCanBePrivate.Global
        public bool CanCreateWorkflowService { get; set; }
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
                OnPropertyChanged(() => CanRename);
                _canRename = value;
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
                OnPropertyChanged(() => CanDelete);
                _canDelete = value;
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
                OnPropertyChanged(() => CanCreateFolder);
            }
        }
        public bool CanDeploy { get; set; }
        public IServer Server
        {
            get;
            set;
        }
        public ICommand ItemSelectedCommand { get; set; }
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
                return _canView && (IsSource || IsService) && !IsResourceVersion;
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
                OnPropertyChanged(() => CanShowDependencies);
            }
        }

        public bool IsVersion { get; set; }
        public bool CanShowVersions
        {
            get
            {
                return IsService && _canShowVersions;
            }
            set
            {
                _canShowVersions = value;
                OnPropertyChanged(() => CanShowVersions);
            }
        }
        public bool CanRollback
        {
            get
            {
                return IsResourceVersion;
            }
        }

        public IShellViewModel ShellViewModel
        {
            get
            {
                return _shellViewModel;
            }
        }

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
                        CanCreatePluginSource = false,
                        CanCreateEmailSource = false,
                        CanCreateRabbitMQSource = false,
                        CanCreateExchangeSource = false,
                        CanCreateDropboxSource = false,
                        CanCreateSharePointSource = false,
                        CanCreateDbSource = false,
                        CanCreateWebSource = false,
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
        public bool AllowEditing
        {
            get
            {
                return _allowEditing;
            }
            set
            {
                OnPropertyChanged(() => AllowEditing);
                _allowEditing = value;
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
                    ShowErrorMessage("An unexpected error occured which prevented the resource from being moved.", "Move Not allowed");
                    Server.UpdateRepository.FireItemSaved();
                    return false;
                }
                destination.UpdateChildrenCount();
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex.Message, "Move Not allowed");
                Server.UpdateRepository.FireItemSaved();
                return false;
            }
            return true;
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
            if (String.IsNullOrEmpty(filter) || (_children.Count > 0 && _children.Any(model => model.IsVisible && !model.IsResourceVersion)))
            {
                IsVisible = true;
            }
            else
            {
                if (!String.IsNullOrEmpty(ResourceName) && !IsResourceVersion)
                {
                    IsVisible = ResourceName.ToLowerInvariant().Contains(filter.ToLowerInvariant());
                }
            }
            if (!String.IsNullOrEmpty(filter))
            {
                if (IsFolder)
                    IsExpanded = true;
            }
            else
            {
                if (IsFolder)
                    IsExpanded = false;
            }
            OnPropertyChanged(() => Children);
        }

        // ReSharper disable UnusedAutoPropertyAccessor.Global
        // ReSharper disable MemberCanBePrivate.Global
        public IResource Resource { get; set; }
        // ReSharper restore MemberCanBePrivate.Global
        // ReSharper restore UnusedAutoPropertyAccessor.Global
        public string Inputs { get; set; }
        public string Outputs { get; set; }
        public string ExecuteToolTip
        {
            get
            {
                return Resources.Languages.Core.ExplorerItemExecuteToolTip;
            }
        }
        public string EditToolTip
        {
            get
            {
                return Resources.Languages.Core.ExplorerItemEditToolTip;
            }
        }
        public string ResourcePath { get; set; }
        public IEnvironmentModel EnvironmentModel
        {
            get
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
            if (Server != null) Server.PermissionsChanged -= UpdatePermissions;
            if (Children != null)
                foreach (var explorerItemViewModel in Children)
                {
                    if (explorerItemViewModel != null) explorerItemViewModel.Dispose();
                }
        }
    }
    public class VersionViewModel : ExplorerItemViewModel
    {
        public VersionViewModel(IServer server, IExplorerTreeItem parent, Action<IExplorerItemViewModel> selectAction, IShellViewModel shellViewModel, IPopupController popupController)
            : base(server, parent, selectAction, shellViewModel, popupController)
        {
        }

        #region Equality members

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(VersionViewModel other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return ResourceName.Equals(other.ResourceName);
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
            return Equals((VersionViewModel)obj);
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            return ResourceId.GetHashCode();
        }

        public static bool operator ==(VersionViewModel left, VersionViewModel right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(VersionViewModel left, VersionViewModel right)
        {
            return !Equals(left, right);
        }

        #endregion
    }
}