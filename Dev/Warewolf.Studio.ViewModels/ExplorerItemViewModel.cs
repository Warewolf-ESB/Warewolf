using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Studio.ViewModels;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;

namespace Warewolf.Studio.ViewModels
{
    public class ExplorerItemViewModel : BindableBase,IExplorerItemViewModel
    {
        string _resourceName;
        private bool _isVisible;
        IServer _server;
        bool _allowEditing;
        bool _isRenaming;
        bool _canExecute;
        bool _canEdit;
        bool _canView;

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
                _resourceName = value;
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
        public IServer Server
        {
            get
            {
                return _server;
            }
            private set
            {
                _server = value;
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
    public class DeployItemMessage : IDeployItemMessage 
    {
        IExplorerItemViewModel _item;
        IExplorerItemViewModel _sourceServer;

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