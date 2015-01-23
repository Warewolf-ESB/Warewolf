using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Explorer;
using Dev2.Common.Interfaces.Studio.ViewModels;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Moq;

namespace Warewolf.Studio.ViewModels
{
    public class EnvironmentViewModel:BindableBase, IEnvironmentViewModel
    {
        readonly IShellViewModel _shellViewModel;
        ICollection<IExplorerItemViewModel> _children;

        public EnvironmentViewModel(IServer server,IShellViewModel shellViewModel)
        {
            if(server==null) throw new ArgumentNullException("server");
            if (shellViewModel == null) throw new ArgumentNullException("shellViewModel");
            _shellViewModel = shellViewModel;
            Server = server;
            NewCommand = new DelegateCommand<ResourceType?>(_shellViewModel.NewResource);
            DisplayName = server.ResourceName;
        }

        public IServer Server { get; set; }

        public ICollection<IExplorerItemViewModel> Children
        {
            get
            {
                return _children;
            }
            set
            {
                _children = value;
                OnPropertyChanged(() => Children);
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
        public string DisplayName
        {
            get;
            set;
        }
        public bool IsConnected { get; private set; }
        public bool IsLoaded { get; private set; }

        public async void Connect()
        {
            IsConnected = await Server.Connect();
            Load();
        }

        public async void Load()
        {
            if (IsConnected)
            {
                var explorerItems = await Server.LoadExplorer();
                var explorerItemViewModels = CreateExplorerItems(explorerItems.Children,Server);
                Children = explorerItemViewModels;
                IsLoaded = true;
            }
        }

        public void Filter(string filter)
        {
            foreach (var explorerItemViewModel in Children)
            {
                explorerItemViewModel.Children.ForEach(model => model.Filter(filter));
                if ((String.IsNullOrEmpty(filter) || explorerItemViewModel.Children.Any(model => model.IsVisible)) || (explorerItemViewModel.ResourceName != null && explorerItemViewModel.ResourceName.ToLowerInvariant().Contains(filter.ToLowerInvariant())))
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
                var res = AsList(Children).FirstOrDefault(a => a.Children!= null && a.Children.Any(b=>b.ResourceId==vm.ResourceId));
                if(res != null)
                {
                    res.Children.Remove(res.Children.FirstOrDefault(a => a.ResourceId == vm.ResourceId));
                    OnPropertyChanged(()=>Children);
                }
            }
        }

        // ReSharper disable ParameterTypeCanBeEnumerable.Local
        ObservableCollection<IExplorerItemViewModel> CreateExplorerItems(IList<IExplorerItem> explorerItems, IServer server)
        // ReSharper restore ParameterTypeCanBeEnumerable.Local
        {
            if (explorerItems == null) return new ObservableCollection<IExplorerItemViewModel>();
            var explorerItemModels = new ObservableCollection<IExplorerItemViewModel>();
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var explorerItem in explorerItems)
            {
                explorerItemModels.Add(new ExplorerItemViewModel(_shellViewModel, server, new Mock<IExplorerHelpDescriptorBuilder>().Object)
                {
                    ResourceName = explorerItem.DisplayName,
                    ResourceId = explorerItem.ResourceId,
                    ResourceType = explorerItem.ResourceType,
                    Children = CreateExplorerItems(explorerItem.Children,server)
                });
            }
            return  explorerItemModels;
        }
    }
}