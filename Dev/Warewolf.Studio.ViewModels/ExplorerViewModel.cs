using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Studio.ViewModels;
using Microsoft.Practices.Prism.Mvvm;

namespace Warewolf.Studio.ViewModels
{
    public class ExplorerViewModel:BindableBase,IExplorerViewModel
    {
        public ExplorerViewModel(IShellViewModel shellViewModel)
        {
            if(shellViewModel == null)
            {
                throw new ArgumentNullException("shellViewModel");
            }
            ConnectControlViewModel = new ConnectControlViewModel(shellViewModel.LocalhostServer);
        }

        ICollection<IEnvironmentViewModel> _environments;
        public ICollection<IEnvironmentViewModel> Environments
        {
            get
            {
                return _environments;
            }
            set
            {
                _environments = value;
                OnPropertyChanged(() => Environments);
            }
        }

        public void Filter(string filter)
        {
            if (Environments != null)
            {
                foreach(var environmentViewModel in Environments)
                {
                    environmentViewModel.Filter(filter);
                }
            }
        }

        public event SelectedExplorerEnvironmentChanged SelectedEnvironmentChanged;
        public IEnvironmentViewModel SelectedEnvironment { get; set; }
        public IServer SelectedServer { get { return SelectedEnvironment.Server; }  }
        public IConnectControlViewModel ConnectControlViewModel { get; private set; }

        public IList<IExplorerItemViewModel> FindItems(Func<IExplorerItemViewModel, bool> filterFunc)
        {
            return null;
        }
    }
}
