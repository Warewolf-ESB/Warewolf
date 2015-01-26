using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Common.Interfaces.Studio.ViewModels;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Warewolf.Studio.Core.Popup;

namespace Warewolf.Studio.ViewModels
{
    public class ExplorerItemViewModel : BindableBase,IExplorerItemViewModel
    {
        readonly IShellViewModel _shellViewModel;
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
        ICollection<IVersionInfoViewModel> _versions;
        bool _areVersionsVisible;
        string _versionHeader;
        ResourceType _resourceType;
        bool _userShouldEditValueNow;
        string _versionNumber;
        ICollection<IExplorerItemViewModel> _children;
        bool _canDrop;
        bool _canDrag;
        Guid _resourceId;
        bool _isExpanded;

        // ReSharper disable TooManyDependencies
        public ExplorerItemViewModel(IShellViewModel shellViewModel,IServer server,IExplorerHelpDescriptorBuilder builder,IExplorerItemViewModel parent):this(shellViewModel,server,builder)
            // ReSharper restore TooManyDependencies
        {
            RollbackCommand = new DelegateCommand(() =>
            {
                var output = _explorerRepository.Rollback(ResourceId, VersionNumber);
                parent.ShowVersionHistory.Execute(null);
                parent.ResourceName = output.DisplayName;

            });
            IsVersion = true;
        }

        public ExplorerItemViewModel(IShellViewModel shellViewModel,IServer server,IExplorerHelpDescriptorBuilder builder)
        {
            VerifyArgument.AreNotNull(new Dictionary<string, object> { { "shellViewModel" ,shellViewModel}, {"server",server }, {"builder",builder } });
            _shellViewModel = shellViewModel;
            LostFocus = new DelegateCommand(LostFocusCommand);

            Children = new ObservableCollection<IExplorerItemViewModel>();
            OpenCommand = new DelegateCommand(() => shellViewModel.AddService(Resource));
            DeployCommand = new DelegateCommand(() => shellViewModel.DeployService(this));
            RenameCommand = new DelegateCommand(()=>IsRenaming=true);
            ItemSelectedCommand = new DelegateCommand(()=>shellViewModel.UpdateHelpDescriptor(builder.Build(this,ExplorerEventContext.Selected)));
            Server = server;
            NewCommand = new DelegateCommand<ResourceType?>(shellViewModel.NewResource);
            CanCreateDbService = true;
            CanRename = true; //todo:remove
            CanDelete = true; //todo:remove
            CanCreatePluginService = true;
            _explorerRepository = server.ExplorerRepository;
            Server.PermissionsChanged += UpdatePermissions;
            ShowVersionHistory = new DelegateCommand((() => AreVersionsVisible = (!AreVersionsVisible)));
            DeleteCommand = new DelegateCommand(Delete);
            OpenVersionCommand = new DelegateCommand(() => { if (ResourceType == ResourceType.Version) _shellViewModel.OpenVersion(ResourceId, VersionNumber); });
            VersionHeader = "Show Version History";
            Builder = builder;
            IsVisible = true;
            IsVersion = false;
            Expand = new DelegateCommand<int?>(clickCount =>
            {
                if (clickCount!= null && clickCount == 2)
                IsExpanded = !IsExpanded;
            });

        }

        void LostFocusCommand()
        {
            //IsRenaming = false;
        }

        public IExplorerHelpDescriptorBuilder Builder { get; set; }

        void Delete()
        {
            if (_shellViewModel.ShowPopup(PopupMessages.GetDeleteConfirmation(ResourceName)))
            {
                _explorerRepository.Delete(this);
                _shellViewModel.RemoveServiceFromExplorer(this);
            }
        }

        public void UpdatePermissions(PermissionsChangedArgs args)
        {
            var resourcePermission = args.Permissions.FirstOrDefault(permission => permission.ResourceID == ResourceId);
            if (resourcePermission != null)
            {
                SetFromPermission(resourcePermission);
            }
            else
            {
                var serverPermission = args.Permissions.FirstOrDefault(permission => permission.IsServer && permission.ResourceID==Guid.Empty);
                if (serverPermission != null)
                {
                    SetFromServer(serverPermission);
                }
            }
        }

        void SetFromServer(IWindowsGroupPermission serverPermission)
        {
            CanEdit = serverPermission.Contribute;
            CanExecute = serverPermission.Contribute || serverPermission.Execute;
            CanView = serverPermission.View || serverPermission.Contribute;
            CanRename = serverPermission.Contribute || serverPermission.Administrator;
            CanDelete = serverPermission.Contribute || serverPermission.Administrator;
        }

        void SetFromPermission(IWindowsGroupPermission resourcePermission)
        {
            CanEdit = resourcePermission.Contribute;
            CanExecute = resourcePermission.Contribute || resourcePermission.Execute;
            CanView = resourcePermission.View || resourcePermission.Contribute;
            CanRename = resourcePermission.Contribute || resourcePermission.Administrator;
            CanDelete = resourcePermission.Contribute || resourcePermission.Administrator;
        }

        public bool UserShouldEditValueNow
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
                if (IsRenaming && _explorerRepository.Rename(this, value)  )
                {
                _resourceName = value;
                }
                if (!IsRenaming)
                {
                    _resourceName = value;
                }
                IsRenaming = false;
                OnPropertyChanged(() => ResourceName);
            }
        }
        public ICollection<IExplorerItemViewModel> Children
        {
            get
            {
                return new ObservableCollection<IExplorerItemViewModel>(_children.Where(a=>a.IsVisible));
            }
            set
            {
                
                _children = value;
                OnPropertyChanged(() => Children);
            }
        }
        public bool Checked { get; set; }
        public Guid ResourceId
        {
            get
            {
                return _resourceId;
            }
            set
            {
                _resourceId = value;
                if (ResourceName == ("Hello World")) 
                AreVersionsVisible = true;
            }
        }
        public ResourceType ResourceType
        {
            get
            {
                return _resourceType;
            }
            set
            {
                _resourceType = value;
                OnPropertyChanged(()=>CanShowVersions);
            }
        }
        public ICommand OpenCommand
        {
            get; set;
        }
        public ICommand NewCommand { get; set; }
        public ICommand DeployCommand { get; set; }
        public ICommand RenameCommand { get; set; }
        public bool CanCreateDbService { get; set; }
        public bool CanCreateDbSource { get; set; }
        public bool CanCreateWebService { get; set; }
        public bool CanCreateWebSource { get; set; }
        public bool CanCreatePluginService { get; set; }
        public bool CanCreatePluginSource { get; set; }
        public bool CanRename
        {
            get
            {
                return _canRename;
            }
            set
            {
                OnPropertyChanged(()=>CanRename);
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
                OnPropertyChanged(()=>CanDelete);
                _canDelete = value;
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
                return _canView;
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
        public bool IsVersion { get; set; }
        public bool CanShowVersions
        {
            get
            {
                return ResourceType == ResourceType.WorkflowService;
            }
        }
        public bool CanRollback
        {
            get
            {
                return ResourceType == ResourceType.Version;
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
                    Children = new ObservableCollection<IExplorerItemViewModel>(_explorerRepository.GetVersions(ResourceId).Select(a => new ExplorerItemViewModel(_shellViewModel,Server,Builder,this)
                    {
                        ResourceName = a.VersionNumber +" "+ a.DateTimeStamp.ToString(CultureInfo.InvariantCulture)+" " + a.Reason,
                        VersionNumber = a.VersionNumber,
                        ResourceId =  ResourceId
                    }
                    ));
                    OnPropertyChanged(() => Children);
                }
                else
                {
                    Children = new ObservableCollection<IExplorerItemViewModel>();
                    OnPropertyChanged(() => Children);
                }
                OnPropertyChanged(()=>AreVersionsVisible);
                
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
                OnPropertyChanged(()=>VersionNumber);
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
                OnPropertyChanged(()=>AllowEditing);
                _allowEditing = value;
            }
        }

        public bool Move(IExplorerItemViewModel destination)
        {
            try
            {

                 _explorerRepository.Move(this, destination);
                 return true;
            }
            catch(Exception err)
            {
                _shellViewModel.Handle(err);
                return false;
            }
        }

        public bool CanDrop
        {
            get
            {
                return ResourceType != ResourceType.Version  ;
            }

            set
            {
            }
        }
        public bool CanDrag
        {
            get
            {
                return ResourceType<ResourceType.Server && ResourceType!= ResourceType.Version;
            }

            set
            {
            }
        }

        public ICommand OpenVersionCommand { get; set; }
        public bool IsExpanded
        {
            get
            {
                return _isExpanded;
            }
            set
            {
                _isExpanded = value;
                OnPropertyChanged(()=>IsExpanded);
            }
        }
        public ICommand Expand { get; set; }

        public void Filter(string filter)
        {
            foreach (var explorerItemViewModel in _children)
            {
                explorerItemViewModel.Filter(filter);
            }
            if (String.IsNullOrEmpty(filter) || (_children.Count > 0 && _children.Any(model => model.IsVisible && model.ResourceType != ResourceType.Version)))
            {
                IsVisible = true;
            }
            else
            {
                IsVisible = ResourceName.Contains(filter);
            }
            OnPropertyChanged(() => Children);
        }

        public IResource Resource { get; set; }
    }

    public class NewItemMessage : INewItemMessage {
        readonly IExplorerItemViewModel _parent;
        readonly ResourceType _type;

        public NewItemMessage(ResourceType type, IExplorerItemViewModel parent)
        {
            _type = type;
            _parent = parent;
        }

        #region Implementation of INewItemMessage

        public IExplorerItemViewModel Parent
        {
            get
            {
                return _parent;
            }
        }
        public ResourceType Type
        {
            get
            {
                return _type;
            }
        }

        #endregion
    }
}