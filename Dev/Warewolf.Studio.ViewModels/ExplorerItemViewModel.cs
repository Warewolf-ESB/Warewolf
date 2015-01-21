using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Studio.ViewModels;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;

namespace Warewolf.Studio.ViewModels
{
    public class ExplorerItemViewModel : BindableBase,IExplorerItemViewModel
    {
        string _resourceName;
        private bool _isVisible;
        bool _allowEditing;
        bool _isRenaming;
        private IExplorerRepository _explorerRepository;
        public ExplorerItemViewModel(IShellViewModel shellViewModel,IServer server,IExplorerHelpDescriptorBuilder builder)
        {
            if(shellViewModel == null)
            {
                throw new ArgumentNullException("shellViewModel");
            }
            Children = new ObservableCollection<IExplorerItemViewModel>();
            OpenCommand = new DelegateCommand(() => shellViewModel.AddService(Resource));
            DeployCommand = new DelegateCommand(() => shellViewModel.DeployService(this));
            RenameCommand = new DelegateCommand(()=>IsRenaming=true);
            ItemSelectedCommand = new DelegateCommand(()=>shellViewModel.UpdateHelpDescriptor(builder.Build(this,ExplorerEventContext.Selected)));
            Server = server;
            NewCommand = new DelegateCommand<ResourceType?>(shellViewModel.NewResource);
            CanCreateDbService = true;
            CanRename = true;
            CanCreatePluginService = true;
            _explorerRepository = server.ExplorerRepository;
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
                if (_explorerRepository.Rename(this, value))
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
        public ResourceType ResourceType { get; set; }
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
        public bool CanRename { get; set; }
        public bool CanDelete { get; set; }
        public bool CanDeploy { get; set; }
        public ICommand ItemSelectedCommand { get; set; }

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
        public IServer Server { get; private set; }

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
    public class DeployItemMessage : IDeployItemMessage 
    {
        readonly IExplorerItemViewModel _item;
        readonly IExplorerItemViewModel _sourceServer;

        public DeployItemMessage(IExplorerItemViewModel item, IExplorerItemViewModel sourceServer)
        {
            _item = item;
            _sourceServer = sourceServer;
        }

        #region Implementation of IDeployItemMessage

        public IExplorerItemViewModel Item
        {
            get
            {
                return _item;
            }
        }
        public IExplorerItemViewModel SourceServer
        {
            get
            {
                return _sourceServer;
            }
        }

        #endregion
    }
}