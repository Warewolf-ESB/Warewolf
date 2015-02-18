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
using Warewolf.Core;
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
        bool _areVersionsVisible;
        string _versionHeader;
        ResourceType _resourceType;
        bool _userShouldEditValueNow;
        string _versionNumber;
        ICollection<IExplorerItemViewModel> _children;
        Guid _resourceId;
        bool _isExpanded;
        bool _canCreateServerSource;
        bool _canCreateFolder;
        string _filter;
        private bool _isSelected;
        bool _canShowVersions;

        // ReSharper disable TooManyDependencies
        public ExplorerItemViewModel(IShellViewModel shellViewModel, IServer server, IExplorerHelpDescriptorBuilder builder, IExplorerItemViewModel parent)
            // ReSharper restore TooManyDependencies
        {
            RollbackCommand = new DelegateCommand(() =>
            {
                var output = _explorerRepository.Rollback(ResourceId, VersionNumber);
                parent.AreVersionsVisible = true;
                parent.ResourceName = output.DisplayName;
            });
            _canShowVersions = true;
            Parent = parent;
            VerifyArgument.AreNotNull(new Dictionary<string, object> { { "shellViewModel", shellViewModel }, { "server", server }, { "builder", builder } , });
            _shellViewModel = shellViewModel;
            LostFocus = new DelegateCommand(LostFocusCommand);

            Children = new ObservableCollection<IExplorerItemViewModel>();
            OpenCommand = new DelegateCommand(() => shellViewModel.AddService(Resource));
            DeployCommand = new DelegateCommand(() => shellViewModel.DeployService(this));
            RenameCommand = new DelegateCommand(() => IsRenaming = true);
            Server = server;
            NewCommand = new DelegateCommand<ResourceType?>(type => shellViewModel.NewResource(type, ResourceId));
            CanCreateDbService = true; //todo:remove
            CanCreateWorkflowService = true; //todo:remove
            CanCreateServerSource = true; //todo:remove
            CanCreateDbSource = true; //todo:remove
            CanRename = true; //todo:remove
            CanDelete = true; //todo:remove
            CanCreatePluginService = true;
            _explorerRepository = server.ExplorerRepository;
            Server.PermissionsChanged += UpdatePermissions;
            ShowVersionHistory = new DelegateCommand((() => AreVersionsVisible = (!AreVersionsVisible)));
            DeleteCommand = new DelegateCommand(Delete);
            OpenVersionCommand = new DelegateCommand(() => { if (ResourceType == ResourceType.Version) ShellViewModel.OpenVersion(ResourceId, VersionNumber); });
            VersionHeader = "Show Version History";
            Builder = builder;
            IsVisible = true;
            IsVersion = false;
            Expand = new DelegateCommand<int?>(clickCount =>
            {
                if (clickCount != null && clickCount == 2 && ResourceType == ResourceType.Folder)
                {
                    IsExpanded = !IsExpanded;
                }
                if (clickCount != null && clickCount == 1 && ResourceType == ResourceType.WorkflowService && IsExpanded)
                {
                    IsExpanded = false;
                }

            });
            CreateFolderCommand = new DelegateCommand(CreateNewFolder);
            CanCreateFolder = true;
            DeleteVersionCommand = new DelegateCommand(DeleteVersion);
        }

        void DeleteVersion()
        {
            if (ShellViewModel.ShowPopup(PopupMessages.GetDeleteVersionMessage(ResourceName)))
            {
                _explorerRepository.Delete(this);
                if (Parent != null)
                {
                    Parent.RemoveChild(this);
                }

            }
        }

        public IExplorerTreeItem Parent { get; set; }

        public void AddSibling(IExplorerItemViewModel sibling)
        {
            if( Parent != null)
            {
                Parent.AddChild(sibling);
            }
        }

        public void AddChild(IExplorerItemViewModel child)
        {
            _children.Add(child);
            OnPropertyChanged(()=>Children);
        }

        public void RemoveChild(IExplorerItemViewModel child)
        {
            var tempChildren = new ObservableCollection<IExplorerItemViewModel>(_children);
            tempChildren.Remove(child);
            _children = tempChildren;
            OnPropertyChanged(()=>Children);
        }



        public void CreateNewFolder()
        {
            if(ResourceType == ResourceType.Folder)
            {
               IsExpanded = true;
                var id = Guid.NewGuid();
                var name = GetChildNameFromChildren();
                _explorerRepository.CreateFolder(ResourceId,name,id);
                var child = new ExplorerItemViewModel(ShellViewModel, Server, Builder, this)
               {
                   ResourceName = name,
                   ResourceId = id,
                   ResourceType = ResourceType.Folder,
                   CanCreateDbService = CanCreateDbService,
                   CanCreateFolder = CanCreateFolder,
                   CanCreateDbSource = CanCreateDbSource,
                   CanCreatePluginService = CanCreatePluginService,
                   CanShowVersions = CanShowVersions,
                   CanRename = CanRename,
                   CanCreatePluginSource = CanCreatePluginSource,
                   CanCreateServerSource = CanCreateServerSource,
                   CanCreateWebService = CanCreateWebService,
                   CanCreateWebSource = CanCreateDbService,
                   CanCreateWorkflowService = CanCreateWorkflowService 
                  
               };
               child.SetFromServer(Server.Permissions.FirstOrDefault(a=>a.IsServer));     
               
               AddChild(child);
                child.IsSelected = true;
               child.IsRenaming = true;
            }

        }

        public void Apply(Action<IExplorerItemViewModel> action)
        {
            action(this);
            if(Children != null)
            {
                foreach(var explorerItemViewModel in Children)
                {
                    explorerItemViewModel.Apply(action);
                }
            }
        }

        public IExplorerItemViewModel Find(string resourcePath)
        {
            if (!resourcePath.Contains("\\") && resourcePath==ResourceName)
                return this;
            if (Children != null && resourcePath.Contains("\\"))
            {
                string name = resourcePath.Substring(1+resourcePath.IndexOf("\\", StringComparison.Ordinal));
                return Children.Select(explorerItemViewModel => explorerItemViewModel.Find(name)).FirstOrDefault(item => item != null);
            }
            return null;
        }

        string GetChildNameFromChildren()
        {
            const string NewFolder = "New Folder";
            int count = 0;
            string folderName = NewFolder;
            while(Children.Any(a=>a.ResourceName == folderName ))
            {
                count++;
                folderName = NewFolder + " "+ count;
            }
            return folderName;
        }

        void LostFocusCommand()
        {
            IsRenaming = false;
        }

        IExplorerHelpDescriptorBuilder Builder { get; set; }

        void Delete()
        {
            if (ShellViewModel.ShowPopup(PopupMessages.GetDeleteConfirmation(ResourceName)))
            {
                _explorerRepository.Delete(this);
                if(Parent!= null)
                {
                   Parent.RemoveChild(this); 
                }
                else
                ShellViewModel.RemoveServiceFromExplorer(this);
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
            CanCreateFolder =  (serverPermission.Contribute || serverPermission.Administrator);
        }

        void SetFromPermission(IWindowsGroupPermission resourcePermission)
        {
            CanEdit = resourcePermission.Contribute;
            CanExecute = resourcePermission.Contribute || resourcePermission.Execute;
            CanView = resourcePermission.View || resourcePermission.Contribute;
            CanRename = resourcePermission.Contribute || resourcePermission.Administrator;
            CanDelete = resourcePermission.Contribute || resourcePermission.Administrator;
           

            
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
                if(Parent != null && Parent.Children.Any(a=>a.ResourceName == value))
                {
                    _shellViewModel.ShowPopup(PopupMessages.GetDuplicateMessage(value));

                }
                else
                {
                    if (IsRenaming  && _explorerRepository.Rename(this, value)  )
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
        }
        public ICollection<IExplorerItemViewModel> Children
        {
            get
            {
                return String.IsNullOrEmpty(_filter) ?_children: new ObservableCollection<IExplorerItemViewModel>(_children.Where(a=>a.IsVisible));
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
        public bool IsExpanderVisible
        {
            get
            {
                return Children.Count>0 && !AreVersionsVisible;
            }
            set
            {
                _isVisible = value;
                OnPropertyChanged(()=>IsExpanderVisible);
            }
        }
        public ICommand NewCommand { get; set; }
        public ICommand DeployCommand { get; set; }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged(() => IsSelected);
                if (_isSelected)
                {
                    var helpDescriptor = new HelpDescriptor("", string.Format("<body><H1>{0}</H1><a href=\"http://warewolf.io\">Warewolf</a><p>Inputs: {1}</p><p>Outputs: {2}</p></body>", ResourceName, Inputs, Outputs), null);
                    _shellViewModel.UpdateHelpDescriptor(helpDescriptor);
                }
            }
        }

        public ICommand RenameCommand { get; set; }
        public bool CanCreateDbService { get; set; }
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
        public bool CanCreateWebService { get; set; }
        public bool CanCreateWebSource { get; set; }
        public bool CanCreatePluginService { get; set; }
        public bool CanCreatePluginSource { get; set; }
        // ReSharper disable MemberCanBePrivate.Global
        public bool CanCreateWorkflowService { get; set; }
        // ReSharper restore MemberCanBePrivate.Global



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
        public bool CanCreateFolder
        {
            get
            {
                return (ResourceType== ResourceType.Folder || ResourceType == ResourceType.Server) && _canCreateFolder;
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
                return ResourceType == ResourceType.WorkflowService && _canShowVersions;
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
                    _children = new ObservableCollection<IExplorerItemViewModel>(_explorerRepository.GetVersions(ResourceId).Select(a => new ExplorerItemViewModel(ShellViewModel,Server,Builder,this)
                    {
                        ResourceName = "v."+a.VersionNumber +" "+ a.DateTimeStamp.ToString(CultureInfo.InvariantCulture)+" " + a.Reason,
                        VersionNumber = a.VersionNumber,
                        ResourceId =  ResourceId,
                         IsVersion = true,
                         CanCreatePluginSource = false,
                         CanCreateWebService =  false,
                         CanCreateDbService = false,
                         CanCreateDbSource = false,
                         CanCreatePluginService = false,
                         CanCreateWebSource = false
                         ,ResourceType =  ResourceType.Version
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
                
                 if (destination.ResourceType == ResourceType.Folder)
                 {
                     destination.AddChild(this);
                     Parent.RemoveChild(this);
                 }
                 else if (destination.ResourceType <= ResourceType.Folder)
                 {
                     destination.AddSibling(this);
                     Parent.RemoveChild(this);
                 }
                 else if (destination.Parent == null)
                 {
                     destination.AddSibling(this);
                     Parent.RemoveChild(this);
                 }
                 return true;
            }
            catch(Exception err)
            {
                ShellViewModel.Handle(err);
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
        public ICommand DeleteVersionCommand { get; set; }
        public bool IsExpanded
        {
            get
            {
                return _isExpanded;
            }
            set
            {
                if(Parent != null && Parent.IsExpanded != value)
                {
                    Parent.IsExpanded = value;
                }
                _isExpanded = value;
                OnPropertyChanged(()=>IsExpanded);
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
            if (String.IsNullOrEmpty(filter) || (_children.Count > 0 && _children.Any(model => model.IsVisible && model.ResourceType != ResourceType.Version)))
            {
                IsVisible = true;
            }
            else
            {
                if (!String.IsNullOrEmpty(ResourceName) && ResourceType!= ResourceType.Version)
                {
                    IsVisible = ResourceName.ToLowerInvariant().Contains(filter.ToLowerInvariant());
                }
            }
            OnPropertyChanged(() => Children);
        }

        // ReSharper disable UnusedAutoPropertyAccessor.Global
        // ReSharper disable MemberCanBePrivate.Global
        public  IResource Resource { get; set; }
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
        public IShellViewModel ShellViewModel
        {
            get
            {
                return _shellViewModel;
            }
        }
    }
}