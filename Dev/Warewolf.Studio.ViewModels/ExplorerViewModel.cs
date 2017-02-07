/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Interfaces;
using Dev2.Services.Security;
using Microsoft.Practices.Prism.Mvvm;

// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Warewolf.Studio.ViewModels
{
    public class ExplorerViewModelBase : BindableBase, IExplorerViewModel, IUpdatesHelp
    {
        // ReSharper disable once InconsistentNaming
        protected ObservableCollection<IEnvironmentViewModel> _environments;
        // ReSharper disable once InconsistentNaming
        protected string _searchText;
        private bool _isRefreshing;
        private IExplorerTreeItem _selectedItem;
        private object[] _selectedDataItems;
        bool _fromActivityDrop;
        bool _allowDrag;

        protected ExplorerViewModelBase()
        {
            RefreshCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(async () => await Refresh(true));
            ClearSearchTextCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(() => SearchText = "");
            CreateFolderCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(CreateFolder);
        }

        private void CreateFolder()
        {
            if (SelectedItem != null)
            {
                if (SelectedItem.CreateFolderCommand.CanExecute(null))
                {
                    SelectedItem.CreateFolderCommand.Execute(null);
                }
            }
        }

        public bool IsFromActivityDrop
        {
            get
            {
                return _fromActivityDrop;
            }
            set
            {
                if (value != _fromActivityDrop)
                {
                    _fromActivityDrop = value;
                    OnPropertyChanged(() => IsFromActivityDrop);
                }
            }
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
                OnPropertyChanged(() => IsRefreshing);
            }
        }

        public bool ShowConnectControl { get; set; }

        public IExplorerTreeItem SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (!Equals(_selectedItem, value))
                {
                    _selectedItem = value;

                    OnPropertyChanged(() => SelectedItem);
                    SelectedItemChanged?.Invoke(this, _selectedItem);
                }
            }
        }

        public object[] SelectedDataItems
        {
            get { return _selectedDataItems; }
            set
            {
                _selectedDataItems = value;
                if (_selectedDataItems.Any())
                {
                    SelectedItem = _selectedDataItems[0] as IExplorerTreeItem;
                }
                OnPropertyChanged(() => SelectedDataItems);
            }
        }

        public virtual ObservableCollection<IEnvironmentViewModel> Environments
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
        public IServer SelectedServer => SelectedEnvironment.Server;

        public virtual string SearchText
        {
            get
            {
                return _searchText;
            }
            set
            {
                if (_searchText == value)
                {
                    return;
                }
                _searchText = value;
                Filter(_searchText);
                OnPropertyChanged(() => SearchText);
            }
        }

        public string SearchToolTip => Resources.Languages.Tooltips.ExplorerSearchToolTip;

        public string ExplorerClearSearchTooltip => Resources.Languages.Tooltips.ExplorerClearSearchTooltip;

        public string RefreshToolTip => Resources.Languages.Tooltips.ExplorerRefreshToolTip;

        public async void RefreshEnvironment(Guid environmentId)
        {
            var environmentViewModel = Environments.FirstOrDefault(model => model.Server.EnvironmentID == environmentId);
            if (environmentViewModel != null)
            {
                
                await RefreshEnvironment(environmentViewModel, true);
            }
        }

        public async Task RefreshSelectedEnvironment()
        {
            if (SelectedEnvironment != null)
            {
                await RefreshEnvironment(SelectedEnvironment, true);
            }
        }
        protected virtual async Task Refresh(bool refresh)
        {
            IsRefreshing = true;
            var environmentId = ConnectControlViewModel?.SelectedConnection.EnvironmentID;
            var resourceName = ConnectControlViewModel?.SelectedConnection.ResourceName.Replace("(Connected)", "").Trim();
            var environmentViewModels = Environments.Where(model => resourceName != null && model.Server.EnvironmentID == environmentId && model.Server.ResourceName.Replace("(Connected)", "").Trim() == resourceName);
            foreach (var environmentViewModel in environmentViewModels)
            {
                await RefreshEnvironment(environmentViewModel, refresh);
            }
            Environments = new ObservableCollection<IEnvironmentViewModel>(Environments);
            IsRefreshing = false;
            //ConnectControlViewModel?.LoadServers();
        }

        private async Task RefreshEnvironment(IEnvironmentViewModel environmentViewModel, bool refresh)
        {
            IsRefreshing = true;
            environmentViewModel.IsConnecting = true;
            if (environmentViewModel.IsConnected)
            {
                environmentViewModel.ForcedRefresh = true;
                await environmentViewModel.Load(true, refresh);
                if (!string.IsNullOrEmpty(SearchText))
                {
                    Filter(SearchText);
                }
            }
            environmentViewModel.ForcedRefresh = false;
            IsRefreshing = false;
            environmentViewModel.IsConnecting = false;
        }

        public virtual void Filter(string filter)
        {
            if (Environments != null)
            {
                foreach (var environmentViewModel in Environments)
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

                if (env != null)
                {
                    if (env.Children.Contains(item))
                    {
                        env.RemoveChild(item);
                    }
                    else
                        env.RemoveItem(item);
                }
                OnPropertyChanged(() => Environments);
            }
        }
        
        public event SelectedExplorerItemChanged SelectedItemChanged;

        public void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IMainViewModel>();
            mainViewModel?.HelpViewModel.UpdateHelpText(helpText);
        }

        public ICommand ClearSearchTextCommand { get; }
        public ICommand CreateFolderCommand { get; }
        public bool AllowDrag
        {
            get
            {
                return _allowDrag;
            }
            set
            {
                _allowDrag = value;
                OnPropertyChanged(() => AllowDrag);
            }
        }

        public void SelectItem(Guid id)
        {
            if (id != Guid.Empty)
            {

                foreach (var environmentViewModel in Environments)
                {
                    environmentViewModel.SelectItem(id, a => SelectedItem = a);
                    environmentViewModel.SelectAction = a => SelectedItem = a;
                }
            }
        }
        public void SelectItem(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                foreach (var environmentViewModel in Environments)
                {
                    environmentViewModel.SelectItem(path, a => SelectedItem = a);
                    environmentViewModel.SelectAction = a => SelectedItem = a;
                }
            }
        }
        public IList<IExplorerItemViewModel> FindItems()
        {
            return null;
        }
        public IConnectControlViewModel ConnectControlViewModel { get; internal set; }

        public void Dispose()
        {
            foreach (var environmentViewModel in Environments)
            {
                environmentViewModel.Dispose();
            }
        }
    }

    public class ExplorerViewModel : ExplorerViewModelBase
    {
        readonly IShellViewModel _shellViewModel;
        readonly Action<IExplorerItemViewModel> _selectAction;
        bool _isLoading;

        public ExplorerViewModel(IShellViewModel shellViewModel, Microsoft.Practices.Prism.PubSubEvents.IEventAggregator aggregator,bool shouldUpdateActiveEnvironment, Action<IExplorerItemViewModel> selectAction = null, bool loadLocalHost = true)
        {
            if (shellViewModel == null)
            {
                throw new ArgumentNullException(nameof(shellViewModel));
            }
            var localhostEnvironment = CreateEnvironmentFromServer(shellViewModel.LocalhostServer, shellViewModel);
            _shellViewModel = shellViewModel;
            _selectAction = selectAction;
            localhostEnvironment.SelectAction = selectAction ?? (a => { });
            // ReSharper disable VirtualMemberCallInContructor
            Environments = new ObservableCollection<IEnvironmentViewModel> { localhostEnvironment };
            if (loadLocalHost)
#pragma warning disable 4014
                LoadEnvironment(localhostEnvironment);
#pragma warning restore 4014

            ConnectControlViewModel = new ConnectControlViewModel(shellViewModel.LocalhostServer, aggregator);
            ConnectControlViewModel.ShouldUpdateActiveEnvironment = shouldUpdateActiveEnvironment;
            ShowConnectControl = true;
            ConnectControlViewModel.ServerConnected += ServerConnected;
            ConnectControlViewModel.ServerDisconnected += ServerDisconnected;
            ConnectControlViewModel.ServerHasDisconnected += ServerDisconnectDetected;
            ConnectControlViewModel.ServerReConnected += ServerReConnected;
        }

        async void ServerConnected(object _, IServer server)
        {
            SearchText = string.Empty;
            IsLoading = true;
            var environmentModel = CreateEnvironmentFromServer(server, _shellViewModel);
            _environments.Add(environmentModel);
            Environments = _environments;
            var result = await LoadEnvironment(environmentModel, IsDeploy);
            IsLoading = result;
        }

        void ServerReConnected(object _, IServer server)
        {
            if (!IsLoading && server.EnvironmentID == Guid.Empty)
                Application.Current.Dispatcher.Invoke(async () =>
                {
                    IsLoading = true;

                    var existing = _environments.FirstOrDefault(a => a.ResourceId == server.EnvironmentID);
                    if (existing == null)
                    {
                        existing = CreateEnvironmentFromServer(server, _shellViewModel);
                        _environments.Add(existing);
                        OnPropertyChanged(() => Environments);
                    }
                    var result = await LoadEnvironment(existing, IsDeploy);

                    IsLoading = result;
                    ShowServerDownError = false;
                });
        }

        protected virtual void AfterLoad(Guid environmentId)
        {
            if (ConnectControlViewModel != null)
            {
                ConnectControlViewModel.IsLoading = false;
            }
            var env = Environments.FirstOrDefault(a => a.ResourceId == environmentId);
            if (env != null && env.IsConnected)
            {
                var perm = new WindowsGroupPermission();
                perm.Permissions = env.Server.GetPermissions(Guid.Empty);
                env.SetPropertiesForDialogFromPermissions(perm);
            }
        }

        public bool IsDeploy { get; set; }

        public virtual bool IsLoading
        {
            get
            {
                return _isLoading;
            }
            set
            {
                _isLoading = value;
                OnPropertyChanged(() => IsLoading);
            }
        }

        void ServerDisconnectDetected(object _, IServer server)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                IPopupController controller = CustomContainer.Get<IPopupController>();
                ServerDisconnected(_, server);
                if (!ShowServerDownError)
                {
                    controller.ShowServerNotConnected(server.DisplayName);
                    ShowServerDownError = true;
                }
            });
        }

        public bool ShowServerDownError { get; set; }

        void ServerDisconnected(object _, IServer server)
        {
            var environmentModel = _environments?.FirstOrDefault(model => model?.Server?.EnvironmentID == server?.EnvironmentID);
            if (environmentModel != null)
            {
                foreach (var a in environmentModel.AsList())
                {
                    a.IsVisible = false;
                }
                if (server.EnvironmentID != Guid.Empty)
                {
                    _environments.Remove(environmentModel);
                }
            }
            OnPropertyChanged(() => Environments);
            try
            {
                if (SelectedServer != null && this is DeployDestinationViewModel)
                {
                    OnPropertyChanged(() => SelectedServer.IsConnected);
                }
            }
            catch (Exception)
            {
                //
            }
            
        }

        protected virtual async Task<bool> LoadEnvironment(IEnvironmentViewModel localhostEnvironment, bool isDeploy = false)
        {
            IsLoading = true;
            localhostEnvironment.Connect();
            var result = await localhostEnvironment.Load(isDeploy, true);
            AfterLoad(localhostEnvironment.Server.EnvironmentID);
            IsLoading = false;
            return result;
        }

        IEnvironmentViewModel CreateEnvironmentFromServer(IServer server, IShellViewModel shellViewModel)
        {
            return new EnvironmentViewModel(server, shellViewModel, false, _selectAction);
        }
    }
}
