using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Studio.ViewModels;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Moq;

namespace Warewolf.Studio.ViewModels
{
    public class EnvironmentViewModel:BindableBase, IEnvironmentViewModel
    {
        readonly IShellViewModel _shellViewModel;
        ICollection<IExplorerItemViewModel> _children;
        bool _isConnecting;
        bool _isConnected;
        bool _isExpanderVisible;
        ConnectionNetworkState _connectionState;
        bool _isServerIconVisible;
        bool _isServerUnavailableIconVisible;

        public EnvironmentViewModel(IServer server,IShellViewModel shellViewModel)
        {
            if(server==null) throw new ArgumentNullException("server");
            if (shellViewModel == null) throw new ArgumentNullException("shellViewModel");
            _shellViewModel = shellViewModel;
            Server = server;
            Server.NetworkStateChanged += Server_NetworkStateChanged;
            _children = new ObservableCollection<IExplorerItemViewModel>();
            NewCommand = new DelegateCommand<ResourceType?>(_shellViewModel.NewResource);
            DisplayName = server.ResourceName;
            RefreshCommand = new DelegateCommand(Load);
            IsServerIconVisible = true;
           
        }

        void Server_NetworkStateChanged(INetworkStateChangedEventArgs args)
        {
            switch(args.State)
            {
                    case ConnectionNetworkState.Connected:
                    Server.Connect();
                    if(!IsConnecting)
                         _shellViewModel.ExecuteOnDispatcher(Load);
                    break;
                    case ConnectionNetworkState.Disconnected:
                    _shellViewModel.ExecuteOnDispatcher(() =>
                    {
                        IsConnected = false;
                        Children = new ObservableCollection<IExplorerItemViewModel>();
     
                    });
                    break;
                    case ConnectionNetworkState.Connecting:

                    _shellViewModel.ExecuteOnDispatcher(() =>
                    {
                        if (!IsConnecting)
                            IsConnected = false;
                        Children = new ObservableCollection<IExplorerItemViewModel>();
                       
                    });
                    break;
            }
        }

        public IServer Server { get; set; }

        public ICollection<IExplorerItemViewModel> Children
        {
            get
            {
                if(_children == null) return _children;
                return new ObservableCollection<IExplorerItemViewModel>( _children.Where(a=>a.IsVisible));
            }
            set
            {
                _children = value;
                OnPropertyChanged(() => Children);
            }
        }
        public bool IsExpanderVisible
        {
            get
            {
                return Children.Count>0;
            }
            set
            {
                
            }
        }
        public ICommand NewCommand
        {
            get;
            set;
        }
        public ICommand DeployCommand
        {
            get;
            set;
        }
        public bool CanCreateDbService { get; set; }
        public bool CanCreateDbSource { get; set; }
        public bool CanCreateWebService { get; set; }
        public bool CanCreateWebSource { get; set; }
        public bool CanCreatePluginService { get; set; }
        public bool CanCreatePluginSource { get; set; }
        public bool CanRename { get; set; }
        public bool CanDelete { get; set; }
        public bool CanDeploy { get; set; }
        public bool CanShowVersions { get { return false; } }
        public bool CanRollback { get; set; }
        public bool IsExpanded { get; set; }
        public ICommand RenameCommand { get; set; }
        public ICommand DeleteCommand { get; set; }
        public ICommand ShowVersionHistory { get; set; }
        public ICommand RollbackCommand { get; set; }
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
        public bool IsLoaded { get; private set; }

        public async void Connect()
        {
            if(Server != null)
            {
                IsConnecting = true;
                IsConnected = false;
                IsConnected = await Server.Connect();
                Load();
             
                IsConnecting = false ;
            }
            
        }

        public bool IsConnecting
        {
            get
            {
                return _isConnecting;
            }
            private set
            {
                _isConnecting = value;
               IsServerIconVisible = !value;
               IsServerUnavailableIconVisible = !value;

                OnPropertyChanged(()=>IsConnecting);
       
            }
        }

        public async void Load()
        {
            if (IsConnected)
            {
                IsConnecting = true;
                var explorerItems = await Server.LoadExplorer();
                var explorerItemViewModels = CreateExplorerItems(explorerItems.Children,Server,null);
                Children = explorerItemViewModels;
                IsLoaded = true;
                IsConnecting = false;
            }
        }

        public void Filter(string filter)
        {
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
            return rootCollection.ToList();

        }
        public void SetItemCheckedState(Guid id, bool state)
        {
           var resource= AsList().FirstOrDefault(a => a.ResourceId == id);
            if(resource!=null)
            {
                resource.Checked = state;
            }
        }

        public void RemoveItem(IExplorerItemViewModel vm)
        {
            if(vm.ResourceType != ResourceType.Server)
            {
                var res = AsList(_children).FirstOrDefault(a => a.Children!= null && a.Children.Any(b=>b.ResourceId==vm.ResourceId));
                if(res != null)
                {
                    res.RemoveChild(res.Children.FirstOrDefault(a => a.ResourceId == vm.ResourceId));
                    OnPropertyChanged(()=>Children);
                }
            }
        }

        public ICommand RefreshCommand { get; set; }
        public bool IsServerIconVisible
        {
            get
            {
                return _isServerIconVisible&&IsConnected;
            }
            set
            {
                _isServerIconVisible = value;
                OnPropertyChanged(()=>IsServerIconVisible);
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
        ObservableCollection<IExplorerItemViewModel> CreateExplorerItems(IList<IExplorerItem> explorerItems, IServer server,IExplorerItemViewModel parent)
        // ReSharper restore ParameterTypeCanBeEnumerable.Local
        {
            if (explorerItems == null) return new ObservableCollection<IExplorerItemViewModel>();
            var explorerItemModels = new ObservableCollection<IExplorerItemViewModel>();
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var explorerItem in explorerItems)
            {
                var itemCreated = new ExplorerItemViewModel(_shellViewModel, server, new Mock<IExplorerHelpDescriptorBuilder>().Object,parent)
                {
                    ResourceName = explorerItem.DisplayName,
                    ResourceId = explorerItem.ResourceId,
                    ResourceType = explorerItem.ResourceType
                };
                itemCreated.Children = CreateExplorerItems(explorerItem.Children, server, itemCreated);
                explorerItemModels.Add(itemCreated);
                
            }
            return  explorerItemModels;
        }
    }
}