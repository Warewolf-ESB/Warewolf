/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Dev2.Common.Interfaces.Core;
using Dev2.ConnectionHelpers;
using Dev2.Interfaces;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;

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
        IConnectControlEnvironment _selectedServer;
        readonly IMainViewModel _mainViewModel;
        readonly IEnvironmentRepository _environmentRepository;
        readonly Action<IEnvironmentModel> _callbackHandler;
        readonly IConnectControlSingleton _connectControlSingleton;
        ObservableCollection<IConnectControlEnvironment> _servers;
        int _previousSelectedIndex=1;
        Action<int> _activeAction;
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
            : this(CustomContainer.Get<IMainViewModel>(), EnvironmentRepository.Instance,
              callbackHandler,
              ConnectControlSingleton.Instance,
              labelText,
              bindToActiveEnvironment)
        {
        }



        public ConnectControlViewModel(IMainViewModel mainViewModel,
                                        IEnvironmentRepository environmentRepository,
                                        Action<IEnvironmentModel> callbackHandler,
                                        IConnectControlSingleton connectControlSingleton,
                                        string labelText,
                                        bool bindToActiveEnvironment)
        {
            VerifyArgument.IsNotNull("callbackHandler", callbackHandler);
            VerifyArgument.IsNotNull("connectControlSingleton", connectControlSingleton);
            VerifyArgument.IsNotNull("labelText", labelText);
            VerifyArgument.IsNotNull("environmentRepository", environmentRepository);
            _activeAction = ActiveAction;
            if(Application.Current != null)
            {
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
            _connectControlSingleton.ConnectedServerChanged += ConnectedServerChanged;
            _connectControlSingleton.AfterReload += ConnectControlSingletonAfterReload; 
            SetSelectedEnvironment();
        }

        void ConnectControlSingletonAfterReload(object sender, ConnectedServerChangedEvent e)
        {
            SelectedServerIndex = _previousSelectedIndex;
        }

        public void ConnectedServerChanged(object sender, ConnectedServerChangedEvent e)
        {
            var index = Servers.IndexOf(Servers.FirstOrDefault(s => s.EnvironmentModel.ID == e.EnvironmentId));

            if(index != -1)
            {
                SelectedServerIndex = index;
            }
        }

        public void ConnectedStatusChanged(object sender, ConnectionStatusChangedEventArg e)
        {
            var server = Servers.FirstOrDefault(c => c.EnvironmentModel.ID == e.EnvironmentId);
            if(server == null)
            {
                return;
            }

            var isBusyStatus = e.ConnectedStatus == ConnectionEnumerations.ConnectedState.Busy;
            bool isconnected;

            if(isBusyStatus)
            {
                isconnected = false;
                IsConnectButtonSpinnerVisible = true;
                IsDropDownEnabled = false;
            }
            else
            {
                IsConnectButtonSpinnerVisible = false;
                IsDropDownEnabled = true;
                isconnected = e.ConnectedStatus == ConnectionEnumerations.ConnectedState.Connected;
            }

            server.IsConnected = isconnected;
            server.AllowEdit = !server.EnvironmentModel.IsLocalHost && e.ConnectedStatus != ConnectionEnumerations.ConnectedState.Busy;

            if(SelectedServer != null && SelectedServer.EnvironmentModel.ID == e.EnvironmentId)
            {
                if(e.DoCallback)
                {
                    _callbackHandler(server.EnvironmentModel);
                }
            }
        }

        void SetSelectedEnvironment()
        {
            if(_bindToActiveEnvironment)
            {
                if(_mainViewModel == null)
                {
                    return;
                }

                var server = Servers.FirstOrDefault(c => c.EnvironmentModel.IsLocalHost);
                if(server == null)
                {
                    return;
                }
                _selectedServerIndex = Servers.IndexOf(server);
                SelectedServer = server;
                _connectControlSingleton.Refresh(server.EnvironmentModel.ID);
                _mainViewModel.SetActiveEnvironment(server.EnvironmentModel);
            }
            else
            {
                if(SelectedServer == null && Servers.Any())
                {
                    var selectedServerIndex = Servers.ToList().FindIndex(c => c.EnvironmentModel.IsLocalHost);
                    if(selectedServerIndex >= 0)
                    {
                        SelectedServerIndex = selectedServerIndex;
                    }
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

        public ObservableCollection<IConnectControlEnvironment> Servers => _servers ?? (_servers = _connectControlSingleton.Servers ?? new ObservableCollection<IConnectControlEnvironment>());

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
                if(value == -1 || value == _selectedServerIndex || Servers.Count <= value)
                {
                    return;
                }

                _previousSelectedIndex = _selectedServerIndex;
                _selectedServerIndex = value;

                var selectedServer = Servers[value];

                if(selectedServer.EnvironmentModel.Name == ConnectControlSingleton.NewServerText)
                {
                    _mainViewModel.NewServerSourceCommand.Execute(string.Empty);
                }
                else
                {
                    SelectedServer = selectedServer;
                    var environmentModel = EnvironmentRepository.Instance.Get(selectedServer.EnvironmentModel.ID);
                    _callbackHandler(environmentModel);
                }
                OnPropertyChanged();
            }
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
            if(existingEnvironments == null)
            {
                environment = null;
                return false;
            }

            var newEnvironments = existingEnvironments.Except(Servers.Select(c => c.EnvironmentModel));
            IEnumerable<IEnvironmentModel> environmentModels = newEnvironments as IEnvironmentModel[] ?? newEnvironments.ToArray();

            if(!environmentModels.Any())
            {
                environment = null;
                return false;
            }

            environment = environmentModels.ToList().Last();
            return true;
        }

        public ICommand ConnectCommand
        {
            get
            {
                return _connectCommand ??
                       (_connectCommand = new RelayCommand(param => _connectControlSingleton.ToggleConnection(SelectedServerIndex)));
            }
        }

        public ICommand EditCommand => _editCommand ??
                                       (_editCommand = new RelayCommand(GetServerToEdit));

        void GetServerToEdit(object param)
        {

            var path = SelectedServer.EnvironmentModel.Category??string.Empty;
            _mainViewModel.EditServer(new ServerSource
            {
                Address = SelectedServer.EnvironmentModel.Connection.AppServerUri.ToString(), 
                ID = SelectedServer.EnvironmentModel.ID, 
                AuthenticationType = SelectedServer.EnvironmentModel.Connection.AuthenticationType,
                UserName = SelectedServer.EnvironmentModel.Connection.UserName, 
                Password = SelectedServer.EnvironmentModel.Connection.Password,
                Name = SelectedServer.DisplayName, 
                ResourcePath = path,
                ServerName = SelectedServer.EnvironmentModel.Connection.WebServerUri.Host,
      
            });
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion INotifyPropertyChanged

        void ActiveAction(int index)
        {
            Application.Current.Dispatcher.BeginInvoke(
                new Action(() =>
                {
                    _selectedServerIndex = index;
                    SelectedServer = Servers[index];

                    OnPropertyChanged("SelectedServer");
                    OnPropertyChanged("SelectedServerIndex");
                }),
                DispatcherPriority.ContextIdle,
                null
                );
        }
    }
}
