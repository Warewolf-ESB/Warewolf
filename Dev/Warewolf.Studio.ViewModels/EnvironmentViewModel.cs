using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Studio.ViewModels;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Moq;

namespace Warewolf.Studio.ViewModels
{
    public class EnvironmentViewModel:BindableBase, IEnvironmentViewModel
    {
        readonly IShellViewModel _shellViewModel;

        public EnvironmentViewModel(IServer server,IShellViewModel shellViewModel)
        {
            if(server==null) throw new ArgumentNullException("server");
            if (shellViewModel == null) throw new ArgumentNullException("shellViewModel");
            _shellViewModel = shellViewModel;
            Server = server;
            NewCommand = new DelegateCommand<ResourceType?>(_shellViewModel.AddService);
        }

        public IServer Server { get; set; }

        public ICollection<IExplorerItemViewModel> ExplorerItemViewModels
        {
            get;
            set;
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

        public void Connect()
        {
            IsConnected = Server.Connect();
        }

        public void Load()
        {
            if (IsConnected)
            {
                var explorerItems = Server.Load();
                var explorerItemViewModels = CreateExplorerItems(explorerItems,Server);
                ExplorerItemViewModels = explorerItemViewModels;
                IsLoaded = true;
            }
        }

        public void Filter(string filter)
        {
            foreach (var explorerItemViewModel in ExplorerItemViewModels)
            {
                if (explorerItemViewModel.ResourceName != null && explorerItemViewModel.ResourceName.ToLowerInvariant().Contains(filter.ToLowerInvariant()))
                {
                    explorerItemViewModel.IsVisible = true;
                }
            }
        }

        public ICollection<IExplorerItemViewModel> AsList()
        {
            return AsList(ExplorerItemViewModels);
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

        // ReSharper disable ParameterTypeCanBeEnumerable.Local
        IList<IExplorerItemViewModel> CreateExplorerItems(IList<IResource> explorerItems, IServer server)
        // ReSharper restore ParameterTypeCanBeEnumerable.Local
        {
            if(explorerItems==null) return new List<IExplorerItemViewModel>();
            IList<IExplorerItemViewModel> explorerItemModels = new List<IExplorerItemViewModel>();
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var explorerItem in explorerItems)
            {
                explorerItemModels.Add(new ExplorerItemViewModel(_shellViewModel, server, new Mock<IExplorerHelpDescriptorBuilder>().Object)
                {
                    Resource = explorerItem,
                    ResourceName = explorerItem.ResourceName,
                    Children = CreateExplorerItems(explorerItem.Children,server)
                });
            }
            return  explorerItemModels;
        }
    }
}