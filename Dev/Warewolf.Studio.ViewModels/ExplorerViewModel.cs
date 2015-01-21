using System;
using System.Collections.Generic;
using System.Windows.Input;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Studio.ViewModels;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Prism.Commands;
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
            RefreshCommand = new DelegateCommand(Refresh);
        }

        void Refresh()
        {
            Environments.ForEach(model =>
            {
                if (model.IsConnected)
                {
                    model.Load();
                }
            });
        }

        public ICommand RefreshCommand { get; set; }
        ICollection<IEnvironmentViewModel> _environments;
        string _searchText;
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
                OnPropertyChanged(() => Environments);
            }
        }

        public event SelectedExplorerEnvironmentChanged SelectedEnvironmentChanged;
        public IEnvironmentViewModel SelectedEnvironment { get; set; }
        public IServer SelectedServer { get { return SelectedEnvironment.Server; }  }
        public IConnectControlViewModel ConnectControlViewModel { get; private set; }
        public string SearchText
        {
            get
            {
                return _searchText;
            }
            set
            {
                if(_searchText == value)
                {
                    return;
                }
                _searchText = value;
                Filter(_searchText);
                OnPropertyChanged(() => SearchText);
            }
        }

        public IList<IExplorerItemViewModel> FindItems(Func<IExplorerItemViewModel, bool> filterFunc)
        {
            return null;
        }
    }
}
