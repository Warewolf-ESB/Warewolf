using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Studio.ViewModels;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;

namespace Warewolf.Studio.ViewModels
{
    public class ExplorerViewModel:BindableBase,IExplorerViewModel
    {
        public ExplorerViewModel(IShellViewModel shellViewModel, IEventAggregator aggregator)
        {
            if(shellViewModel == null)
            {
                throw new ArgumentNullException("shellViewModel");
            }
            
            RefreshCommand = new DelegateCommand(Refresh);
            var localhostEnvironment = CreateEnvironmentFromServer(shellViewModel.LocalhostServer,shellViewModel);
            Environments = new ObservableCollection<IEnvironmentViewModel>{localhostEnvironment};
            localhostEnvironment.Connect();
            ConnectControlViewModel = new ConnectControlViewModel(shellViewModel.LocalhostServer,aggregator);
        }

        IEnvironmentViewModel CreateEnvironmentFromServer(IServer server,IShellViewModel shellViewModel)
        {
            return new EnvironmentViewModel(server, shellViewModel);
        }

        void Refresh()
        {
            IsRefreshing = true;
            Environments.ForEach(model =>
            {
                if (model.IsConnected)
                {
                    model.Load();
                }
            });
            IsRefreshing = false;
        }

        public ICommand RefreshCommand { get; set; }
        public bool IsRefreshing
        {
            get
            {
                return _isRefreshing;
            }
            set
            {
                _isRefreshing = value;
                OnPropertyChanged(()=>IsRefreshing);
            }
        }

        ICollection<IEnvironmentViewModel> _environments;
        string _searchText;
        bool _isRefreshing;

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

        public void RemoveItem(IExplorerItemViewModel item)
        {
            if (Environments != null)
            {
                var env = Environments.FirstOrDefault(a => a.Server == item.Server);
                if(env!= null)
                {
                    env.RemoveItem(item);
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

        public string SearchToolTip
        {
            get
            {
                return Resources.Languages.Core.ExplorerSearchToolTip;
            }
        }

        public string RefreshToolTip
        {
            get
            {
                return Resources.Languages.Core.ExplorerRefreshToolTip;
            }
        }

    }
}
