using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Studio.ViewModels;
using Microsoft.Practices.Prism.Mvvm;

namespace Warewolf.Studio.ViewModels
{
    public class EnvironmentViewModel:BindableBase, IEnvironmentViewModel
    {
        public EnvironmentViewModel(IServer server)
        {
            if(server==null) throw new ArgumentNullException("server");
            Server = server;
        }

        public IServer Server { get; set; }

        public ICollection<IExplorerItemViewModel> ExplorerItemViewModels
        {
            get;
            set;
        }
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
                var explorerItemViewModels = CreateExplorerItems(explorerItems);
                ExplorerItemViewModels = explorerItemViewModels;
                IsLoaded = true;
            }
        }

        public void Filter(string filter)
        {
        }

        // ReSharper disable ParameterTypeCanBeEnumerable.Local
        IList<IExplorerItemViewModel> CreateExplorerItems(IList<IResource> explorerItems)
        // ReSharper restore ParameterTypeCanBeEnumerable.Local
        {
            if(explorerItems==null) return new List<IExplorerItemViewModel>();
            IList<IExplorerItemViewModel> explorerItemModels = new List<IExplorerItemViewModel>();
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var explorerItem in explorerItems)
            {
                explorerItemModels.Add(new ExplorerItemViewModel
                {
                    Resource = explorerItem,
                    Children = CreateExplorerItems(explorerItem.Children)
                });
            }
            return  explorerItemModels;
        }
    }
}