
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Data;
using Dev2.ConnectionHelpers;
using Dev2.Interfaces;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Webs;

// ReSharper disable CheckNamespace
namespace Dev2.CustomControls.Connections
// ReSharper restore CheckNamespace
{
    public class ConnectControlViewModel : INotifyPropertyChanged, IConnectControlViewModel
    {
        #region Fields
        ICommand _connectCommand;
        ICommand _editCommand;
        bool _isConnectButtonSpinnerVisible;
        bool _isEnabled;
        bool _isDropDownEnabled;
        string _labelText;
        readonly bool _bindToActiveEnvironment;
        int _selectedServerIndex;
        readonly Dispatcher _dispatcher;
        IConnectControlEnvironment _selectedServer;
        readonly IMainViewModel _mainViewModel;
        readonly IEnvironmentRepository _environmentRepository;
        readonly Action<IEnvironmentModel> _callbackHandler;
        readonly IConnectControlSingleton _connectControlSingleton;
        ObservableCollection<IConnectControlEnvironment> _servers;
        #endregion

        public ConnectControlViewModel(IMainViewModel mainViewModel, Action<IEnvironmentModel> callbackHandler,
                                       string labelText,
                                       bool bindToActiveEnvironment)
            : this(mainViewModel,
              EnvironmentRepository.Instance,
              callbackHandler,
              ConnectControlSingleton.Instance,
              labelText,
              bindToActiveEnvironment)
        {
        }

        public ConnectControlViewModel(Action<IEnvironmentModel> callbackHandler,
                                     string labelText,
                                     bool bindToActiveEnvironment)
            : this(null, EnvironmentRepository.Instance,
              callbackHandler,
              ConnectControlSingleton.Instance,
              labelText,
              bindToActiveEnvironment)
        {
        }



        internal ConnectControlViewModel(IMainViewModel mainViewModel,
                                        IEnvironmentRepository environmentRepository,
                                        Action<IEnvironmentModel> callbackHandler,
                                        IConnectControlSingleton connectControlSingleton,
                                        string labelText,
                                        bool bindToActiveEnvironment, Action<IEnvironmentModel, ResourceType, string, string, string, string, string> openWizard = null)
        {
            VerifyArgument.IsNotNull("callbackHandler", callbackHandler);
            VerifyArgument.IsNotNull("connectControlSingleton", connectControlSingleton);
            VerifyArgument.IsNotNull("labelText", labelText);
            VerifyArgument.IsNotNull("environmentRepository", environmentRepository);

            if(Application.Current != null)
            {
                _dispatcher = Application.Current.Dispatcher;
            }

            IsEnabled = true;
            IsDropDownEnabled = true;
            LabelText = labelText;
            _mainViewModel = mainViewModel;
            _environmentRepository = environmentRepository;
            _callbackHandler = callbackHandler;
            _connectControlSingleton = connectControlSingleton;
            _bindToActiveEnvironment = bindToActiveEnvironment;
            _connectControlSingleton.ConnectedStatusChanged += ConnectedStatusChanged;

            if(openWizard == null)
            {
                _openWizard = (environmentModel, resourceType, resourcePath, category, resourceId, sourceId, resourceName) => RootWebSite.ShowDialog(environmentModel, resourceType, resourcePath, category, resourceId, sourceId, resourceName);
            }
            else
            {
                _openWizard = openWizard;
            }

            SetSelectedEnvironment();
        }

        public void ConnectedServerChanged(object sender, IConnectedServerChangedEvent connectedServerChangedEvent)
        {
//            var index = Servers.IndexOf(Servers.FirstOrDefault(s => s.EnvironmentModel.ID == e.EnvironmentId));
//
//            if(index != -1)
//            {
//                SelectedServerIndex = index;
//            }
        }

        public void ConnectedStatusChanged(object sender, IConnectionStatusChangedEventArg connectionStatusChangedEventArg)
        {
//            var server = Servers.FirstOrDefault(c => c.EnvironmentModel.ID == e.EnvironmentId);
//            if(server == null)
//            {
//                return;
//            }
//
//            var isBusyStatus = e.ConnectedStatus == ConnectionEnumerations.ConnectedState.Busy;
//            bool isconnected;
//
//            if(isBusyStatus)
//            {
//                isconnected = false;
//                IsConnectButtonSpinnerVisible = true;
//                IsDropDownEnabled = false;
//            }
//            else
//            {
//                IsConnectButtonSpinnerVisible = false;
//                IsDropDownEnabled = true;
//                isconnected = e.ConnectedStatus == ConnectionEnumerations.ConnectedState.Connected;
//            }
//
//            server.IsConnected = isconnected;
//            server.AllowEdit = !server.EnvironmentModel.IsLocalHost && e.ConnectedStatus != ConnectionEnumerations.ConnectedState.Busy;
//
//            if(SelectedServer != null && SelectedServer.EnvironmentModel.ID == e.EnvironmentId)
//            {
//                if(e.DoCallback)
//                {
//                    _callbackHandler(server.EnvironmentModel);
//                }
//            }
        }

        public void SetTargetEnvironment()
        {
//            var otherConnectedServers = Servers.Where(s => s.IsConnected && !s.EnvironmentModel.IsLocalHost);
//            IEnumerable<IConnectControlEnvironment> connectControlEnvironments = otherConnectedServers as IConnectControlEnvironment[] ?? otherConnectedServers.ToArray();
//            if(connectControlEnvironments.Count() == 1)
//            {
//                SelectedServerIndex = Servers.IndexOf(connectControlEnvironments.First());
//            }
        }

        void SetSelectedEnvironment()
        {
            if(_bindToActiveEnvironment)
            {
                if(_mainViewModel == null)
                {
                    return;
                }

//                var server = Servers.FirstOrDefault(c => c.EnvironmentModel.IsLocalHost);
//                if(server == null)
//                {
//                    return;
//                }
//                _selectedServerIndex = Servers.IndexOf(server);
//                SelectedServer = server;
//                _connectControlSingleton.Refresh(server.EnvironmentModel.ID);
//                _mainViewModel.SetActiveEnvironment(server.EnvironmentModel);
            }
            else
            {
                if(SelectedServer == null && Servers.Any())
                {
//                    var selectedServerIndex = Servers.ToList().FindIndex(c => c.EnvironmentModel.IsLocalHost);
//                    if(selectedServerIndex >= 0)
//                    {
//                        SelectedServerIndex = selectedServerIndex;
//                    }
                }
            }
        }

        public bool IsEnabled
        {
            get
            {
                return _isEnabled;
            }
            set
            {
                _isEnabled = value;
                OnPropertyChanged();
            }
        }

        public bool IsConnectButtonSpinnerVisible
        {
            get
            {
                return _isConnectButtonSpinnerVisible;
            }
            set
            {
                _isConnectButtonSpinnerVisible = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<IConnectControlEnvironment> Servers
        {
            get
            {
                return _servers ?? (_servers = _connectControlSingleton.Servers ?? new ObservableCollection<IConnectControlEnvironment>());
            }
        }

        #region IsDropDownEnabled

        public bool IsDropDownEnabled
        {
            get
            {
                return _isDropDownEnabled;
            }
            set
            {
                _isDropDownEnabled = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region LabelText

        public string LabelText
        {
            get
            {
                return _labelText;
            }
            private set
            {
                _labelText = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Selected Server

        public IConnectControlEnvironment SelectedServer
        {
            get
            {
                return _selectedServer;
            }
            set
            {
                _selectedServer = value;
                OnPropertyChanged();
            }
        }

        public int SelectedServerIndex
        {
            get
            {
                return _selectedServerIndex;
            }
            set
            {
                if (value == -1 || value == _selectedServerIndex || Servers.Count <= value)
                {
                    return;
                }

                var origValue = _selectedServerIndex;
                _selectedServerIndex = value;

                var selectedServer = Servers[value];

                //                if(selectedServer.EnvironmentModel.Name == ConnectControlSingleton.NewServerText)
                //                {
                //                    int newServerIndex;
                //
                //                    if(!AddNewServer(out newServerIndex, OpenConnectionWizard))
                //                    {
                //                        if(_dispatcher != null)
                //                        {
                //                            _dispatcher.BeginInvoke(
                //                            new Action(() =>
                //                            {
                //                                _selectedServerIndex = origValue;
                //                                OnPropertyChanged();
                //                            }),
                //                            DispatcherPriority.ContextIdle,
                //                            null
                //                        );
                //                        }
                //                        else
                //                        {
                //                            _selectedServerIndex = origValue;
                //                            OnPropertyChanged();
                //                        }
                //                    }
                //                }
                //                else
                //                {
                //                    SelectedServer = selectedServer;
                //                    var environmentModel = EnvironmentRepository.Instance.Get(selectedServer.EnvironmentModel.ID);
                //                    //_callbackHandler(selectedServer.EnvironmentModel);
                //                    _callbackHandler(environmentModel);
                //                }
                //                OnPropertyChanged();
                //            }
            }
        }

        public bool AddNewServer(out int newServerIndex, Action<int> openDialog)
        {
            newServerIndex = -1;
//            IEnvironmentModel newEnvironment;
//            if(ActionForNewRemoteServer(out newEnvironment, openDialog))
//            {
//                var selectedServerIndex = Servers.ToList().FindIndex(c => c.EnvironmentModel.ID == newEnvironment.ID);
//                if(selectedServerIndex >= 0)
//                {
//                    newServerIndex = selectedServerIndex;
//                    return true;
//                }
//            }
            return false;
        }

        bool ActionForNewRemoteServer(out IEnvironmentModel newServer, Action<int> openDialog)
        {
            openDialog(-1);
            if(GetNewlyAddedServer(out newServer))
            {
                AddToServersCollection(newServer);
                return true;
            }
            newServer = null;
            return false;
        }

        void AddToServersCollection(IEnvironmentModel server)
        {
            Servers.Add(new ConnectControlEnvironment
            {
                EnvironmentModel = server,
                IsConnected = server.IsConnected,
                AllowEdit = !server.IsLocalHost
            });

            SelectedServer = Servers.Last();
            _selectedServerIndex = Servers.IndexOf(SelectedServer);
            if(!server.IsConnected)
            {
                _connectControlSingleton.ToggleConnection(SelectedServerIndex);
            }
        }

        bool GetNewlyAddedServer(out IEnvironmentModel environment)
        {
            var existingEnvironments = _environmentRepository.All();
            environment = null;
            if(existingEnvironments == null)
            {
                return false;
            }

            var newEnvironments = existingEnvironments.Except(Servers.Select(c => c.EnvironmentModel));
//            IEnumerable<IEnvironmentModel> environmentModels = newEnvironments as IEnvironmentModel[] ?? newEnvironments.ToArray();
//
//            if(!environmentModels.Any())
//            {
//                environment = null;
//                return false;
//            }

//            environment = environmentModels.ToList().Last();
            return true;
        }

        private readonly Action<IEnvironmentModel, ResourceType, string, string, string, string, string> _openWizard;

        public void OpenConnectionWizard(int selectedIndex)
        {
//            var localhost = Servers.FirstOrDefault(c => c.EnvironmentModel.IsLocalHost);
//            if(localhost != null)
//            {
//                if(selectedIndex >= 0)
//                {
//                    var environmentModel = Get(selectedIndex);
//                    _openWizard(localhost.EnvironmentModel, ResourceType.Server, null, string.Empty, environmentModel.ID.ToString(), null, environmentModel.Name);
//                }
//                else
//                {
//                    _openWizard(localhost.EnvironmentModel, ResourceType.Server, null, string.Empty, null, null, null);
//                }
//            }
        }

        IEnvironmentModel Get(int selectedIndex)
        {
            var selectedServer = Servers[selectedIndex];
            return selectedServer.EnvironmentModel as IEnvironmentModel;
        }

        public ICommand ConnectCommand
        {
            get
            {
                return _connectCommand ??
                       (_connectCommand = new RelayCommand(param => _connectControlSingleton.ToggleConnection(SelectedServerIndex)));
            }
        }

        public ICommand EditCommand
        {
            get
            {
                return _editCommand ??
                       (_editCommand = new RelayCommand(param => _connectControlSingleton.EditConnection(SelectedServerIndex, OpenConnectionWizard)));
            }
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion INotifyPropertyChanged

        public void UpdateActiveEnvironment(IEnvironmentModel environmentModel, bool isSetFromConnectControl)
        {
            if(isSetFromConnectControl)
            {
                return;
            }

            if(environmentModel != null && _bindToActiveEnvironment)
            {
//                var index = Servers.ToList().FindIndex(c => c.EnvironmentModel.ID == environmentModel.ID);
//                if(index >= 0)
//                {
//                    _selectedServerIndex = index;
//                    SelectedServer = Servers[index];
//                    // ReSharper disable ExplicitCallerInfoArgument
//                    OnPropertyChanged("SelectedServerIndex");
//                    // ReSharper restore ExplicitCallerInfoArgument
//                }
            }
        }
    }
}
