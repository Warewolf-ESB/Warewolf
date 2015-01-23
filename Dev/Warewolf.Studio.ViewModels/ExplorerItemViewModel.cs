using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Studio.ViewModels;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.Practices.ObjectBuilder2;
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

        ICommand _deleteCommand;
        ICollection<IVersionInfoViewModel> _versions;
        bool _canShowVersions;
        bool _areVersionsVisible;
        string _versionHeader;
        ResourceType _resourceType;
        bool _canRollback;
        ICommand _rollbackCommand;

        public ExplorerItemViewModel(IShellViewModel shellViewModel,IServer server,IExplorerHelpDescriptorBuilder builder)
        {
            VerifyArgument.AreNotNull(new Dictionary<string, object> { { "shellViewModel" ,shellViewModel}, {"server",server }, {"builder",builder } });
            _shellViewModel = shellViewModel;
          
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
            Versions = new ObservableCollection<IVersionInfoViewModel>();
            VersionHeader = "Show Version History";
            CanRollback = true;
        }

        void Delete()
        {
            if (_shellViewModel.ShowPopup(PopupMessages.GetDeleteConfirmation(ResourceName)))
            {
                _explorerRepository.Delete(this);
                _shellViewModel.RemoveServiceFromExplorer(this);
            }
        }

        void UpdatePermissions(PermissionsChangedArgs args)
        {
            var resourcePermission = args.Permissions.FirstOrDefault(permission => permission.ResourceID == ResourceId);
            if (resourcePermission != null)
            {
                CanEdit = resourcePermission.Contribute;
                CanExecute = resourcePermission.Contribute || resourcePermission.Execute;
                CanView = resourcePermission.View || resourcePermission.Contribute;
                CanRename = resourcePermission.Contribute || resourcePermission.Administrator;
                CanDelete = resourcePermission.Contribute || resourcePermission.Administrator;
            }
            else
            {
                var serverPermission = args.Permissions.FirstOrDefault(permission => permission.IsServer && permission.ResourceID==Guid.Empty);
                if (serverPermission != null)
                {
                    CanEdit = serverPermission.Contribute;
                    CanExecute = serverPermission.Contribute || serverPermission.Execute;
                    CanView = serverPermission.View || serverPermission.Contribute;
                    CanRename = serverPermission.Contribute || serverPermission.Administrator;
                    CanDelete = serverPermission.Contribute || serverPermission.Administrator;
                }
            }
        }

        public ICommand DeleteCommand
        {
            get
            {
                return _deleteCommand;  
            }
            set
            {
                _deleteCommand = value;
            }
        }
        public ICommand ShowVersionHistory { get; set; }
        public ICommand RollbackCommand
        {
            get
            {
                return _rollbackCommand;
            }
            set
            {
                _rollbackCommand = value;
            }
        }
        public bool IsRenaming
        {
            get
            {
                return _isRenaming;
            }
            set
            {

                _isRenaming = value;
               
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
            get;
            set;
        }
        public bool Checked { get; set; }
        public Guid ResourceId { get; set; }
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
                return _canRollback;
            }
            set
            {
                _canRollback = value;
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
                    Versions = new ObservableCollection<IVersionInfoViewModel>(_explorerRepository.GetVersions(ResourceId).Select(a => new VersionInfoViewModel(a,_explorerRepository,this)));
                    OnPropertyChanged(() => Versions);
                }
                else
                {
                    Versions = new Collection<IVersionInfoViewModel>();
                }
                OnPropertyChanged(()=>AreVersionsVisible);
                
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

        public ICollection<IVersionInfoViewModel> Versions
        {
            get
            {
                return _versions;
            }
            set
            {
                _versions = value;
                OnPropertyChanged(()=>Versions);
            }
        }

        public void Filter(string filter)
        {
            foreach (var explorerItemViewModel in Children)
            {
                explorerItemViewModel.Children.ForEach(model => model.Filter(filter));
                if (String.IsNullOrEmpty(filter) || explorerItemViewModel.Children.Any(model => model.IsVisible))
                {
                    explorerItemViewModel.IsVisible = true;
                }
                else
                {
                    explorerItemViewModel.IsVisible = false;
                }
                OnPropertyChanged(() => Children);
            }
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