using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Studio.ViewModels;
using Microsoft.Practices.Prism.Mvvm;

namespace Warewolf.Studio.ViewModels
{
    public class EnvironmentViewModel:BindableBase,IEnvironmentViewModel
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
                Server.Load();
                IsLoaded = true;
            }
        }
    }
}