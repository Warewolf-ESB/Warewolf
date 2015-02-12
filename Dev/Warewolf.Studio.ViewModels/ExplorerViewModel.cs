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
    public class ExplorerViewModelBase : BindableBase, IExplorerViewModel
    {
        private ICollection<IEnvironmentViewModel> _environments;
        private string _searchText;
        private bool _isRefreshing;
        private IExplorerTreeItem _selectedItem;
        private object[] _selectedDataItems;

        public ExplorerViewModelBase()
        {
            
            RefreshCommand = new DelegateCommand(Refresh);
            
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

        public bool ShowConnectControl { get; set; }

        public IExplorerTreeItem SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                OnPropertyChanged(() => SelectedItem);

            }
        }

        public object[] SelectedDataItems
        {
            get { return _selectedDataItems; }
            set
            {
                _selectedDataItems = value;
                OnPropertyChanged(() => SelectedDataItems);
            }
        }

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

        public IEnvironmentViewModel SelectedEnvironment { get; set; }
        public IServer SelectedServer { get { return SelectedEnvironment.Server; }  }
        

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

        private void Refresh()
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

    

        public void SelectItem(Guid id)
        {
            foreach(var environmentViewModel in Environments)
            {
                environmentViewModel.SelectItem(id, (a=>SelectedItem =a));  
            }
        }

        public IList<IExplorerItemViewModel> FindItems(Func<IExplorerItemViewModel, bool> filterFunc)
        {
            return null;
        }
        public IConnectControlViewModel ConnectControlViewModel { get; internal set; }
        protected virtual void OnSelectedEnvironmentChanged(IEnvironmentViewModel e)
        {
            var handler = SelectedEnvironmentChanged;
            if (handler != null) handler(this, e);
        }
    }

    public class ExplorerViewModel:ExplorerViewModelBase
    {
        public ExplorerViewModel(IShellViewModel shellViewModel, IEventAggregator aggregator)
        {
            if (shellViewModel == null)
            {
                throw new ArgumentNullException("shellViewModel");
            }
            var localhostEnvironment = CreateEnvironmentFromServer(shellViewModel.LocalhostServer, shellViewModel);
            Environments = new ObservableCollection<IEnvironmentViewModel> { localhostEnvironment };
            localhostEnvironment.Connect();
            ConnectControlViewModel = new ConnectControlViewModel(shellViewModel.LocalhostServer, aggregator);
            ShowConnectControl = true;
        }

        

        IEnvironmentViewModel CreateEnvironmentFromServer(IServer server,IShellViewModel shellViewModel)
        {
            return new EnvironmentViewModel(server, shellViewModel);
        }
    }

    public class SingleEnvironmentExplorerViewModel : ExplorerViewModelBase
    {
        public SingleEnvironmentExplorerViewModel(IEnvironmentViewModel environmentViewModel)
        {
            Environments = new ObservableCollection<IEnvironmentViewModel> { environmentViewModel };
            ShowConnectControl = false;
        }
    }
}
