using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using Dev2.AppResources.Enums;
using Dev2.Data.ServiceModel;
using Dev2.Messages;
using Dev2.Network;
using Dev2.Providers.Logs;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Studio.Core;
using Dev2.Studio.Core.InterfaceImplementors;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Models;
using Dev2.Threading;
using Dev2.Webs;

// ReSharper disable CheckNamespace
namespace Dev2.UI
// ReSharper restore CheckNamespace
{
    public class ConnectControlViewModel : DependencyObject, INotifyPropertyChanged, IHandle<UpdateActiveEnvironmentMessage>, IHandle<SetConnectControlSelectedServerMessage>
    {
        #region Fields

        const string NewServerText = "New Remote Server...";

        readonly IAsyncWorker _asyncWorker;
        readonly IEnvironmentModel _activeEnvironment;
        readonly IEventAggregator _eventPublisher;

        ICommand _connectCommand;
        ICommand _editCommand;
        bool _isConnectButtonSpinnerVisible;
        bool _isEnabled;

        #endregion

        #region Ctor

        public ConnectControlViewModel(IEnvironmentModel activeEnvironment, IEventAggregator eventAggregator)
            : this(activeEnvironment, eventAggregator, new AsyncWorker())
        {

        }

        public ConnectControlViewModel(IEnvironmentModel activeEnvironment, IEventAggregator eventAggregator, IAsyncWorker asyncWorker)
        {
            _asyncWorker = asyncWorker;
            VerifyArgument.IsNotNull("eventPublisher", eventAggregator);
            eventAggregator.Subscribe(this);
            if(activeEnvironment != null)
            {
                _activeEnvironment = activeEnvironment;
            }
            ObservableCollection<IEnvironmentModel> observableCollection = new ObservableCollection<IEnvironmentModel> { CreateNewRemoteServerEnvironment() };
            EnvironmentRepository.Instance.ItemAdded += OnEnvironmentAdded;
            Servers = observableCollection;
            _eventPublisher = eventAggregator;
            IsEnabled = true;
        }

        void OnEnvironmentAdded(object sender, EventArgs args)
        {
            AddMissingServers();
        }

        #endregion

        #region Properties



        public bool IsEnabled
        {
            get
            {
                return _isEnabled;
            }
            set
            {
                _isEnabled = value;
                OnPropertyChanged("IsEnabled");
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
                OnPropertyChanged("IsConnectButtonSpinnerVisible");
            }
        }

        public ObservableCollection<IEnvironmentModel> Servers { get; set; }

        public IEnvironmentModel ActiveEnvironment
        {
            get
            {
                return _activeEnvironment;
            }
        }

        #endregion

        #region Dependancy Properties

        #region IsDropDownEnabled

        public bool IsDropDownEnabled
        {
            get { return (bool)GetValue(IsDropDownEnabledProperty); }
            set { SetValue(IsDropDownEnabledProperty, value); }
        }

        public static readonly DependencyProperty IsDropDownEnabledProperty =
            DependencyProperty.Register("IsDropDownEnabled", typeof(bool), typeof(ConnectControlViewModel), new PropertyMetadata(true));

        #endregion

        #region ConnectControlInstanceType

        public ConnectControlInstanceType ConnectControlInstanceType
        {
            get
            {
                return (ConnectControlInstanceType)GetValue(ConnectControlInstanceTypeProperty);
            }
            set
            {
                SetValue(ConnectControlInstanceTypeProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for ConnectControlInstanceType.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ConnectControlInstanceTypeProperty =
            DependencyProperty.Register("ConnectControlInstanceType", typeof(ConnectControlInstanceType), typeof(ConnectControlViewModel), new PropertyMetadata(ConnectControlInstanceType.Explorer));

        #endregion

        #region LabelText

        public string LabelText
        {
            get { return (string)GetValue(LabelTextProperty); }
            set { SetValue(LabelTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LabelText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LabelTextProperty =
            DependencyProperty.Register("LabelText", typeof(string),
                typeof(ConnectControlViewModel), new PropertyMetadata("Server "));

        #endregion

        #region Selected Server

        public IEnvironmentModel SelectedServer
        {
            get
            {
                return _selectedServer;
            }
            set
            {
                if(_selectedServer != null && _selectedServer.Equals(value))
                {
                    return;
                }
                _selectedServer = value;
                OnPropertyChanged("SelectedServer");
            }
        }

        public void SelectedServerHasChanged(IEnvironmentModel newValue, ConnectControlViewModel viewModel)
        {
            if(newValue != null)
            {
                if(newValue.IsConnected)
                {
                    viewModel.SetConnectedState();
                }
                else
                {
                    viewModel.SetDisconnectedState();
                }
                ActionForNewRemoteServer(newValue, viewModel);
                ActionForAlreadySavedServer(newValue, viewModel);
                viewModel._eventPublisher.Publish(new ServerSelectionChangedMessage(viewModel.SelectedServer, viewModel.ConnectControlInstanceType));
            }
        }

        static void ActionForAlreadySavedServer(IEnvironmentModel newValue, ConnectControlViewModel viewModel)
        {
            if(newValue.Name != NewServerText)
            {
                switch(viewModel.ConnectControlInstanceType)
                {
                    case ConnectControlInstanceType.Explorer:
                        viewModel._eventPublisher.Publish(new SetSelectedItemInExplorerTree(newValue.Name));
                        viewModel._eventPublisher.Publish(new AddServerToExplorerMessage(newValue));
                        viewModel._eventPublisher.Publish(new SetActiveEnvironmentMessage(newValue, true));
                        break;
                    case ConnectControlInstanceType.DeploySource:
                        viewModel._eventPublisher.Publish(new AddServerToDeployMessage(viewModel.SelectedServer, viewModel.ConnectControlInstanceType));
                        break;
                    case ConnectControlInstanceType.DeployTarget:
                        viewModel._eventPublisher.Publish(new AddServerToDeployMessage(viewModel.SelectedServer, viewModel.ConnectControlInstanceType));
                        break;
                    case ConnectControlInstanceType.Settings:
                        viewModel.AddMissingServers();
                        break;
                    case ConnectControlInstanceType.Scheduler:
                        viewModel.AddMissingServers();
                        break;
                    case ConnectControlInstanceType.RuntimeConfiguration:
                        break;
                }
            }
        }

        static void ActionForNewRemoteServer(IEnvironmentModel newValue, ConnectControlViewModel viewModel)
        {
            if(newValue.Name == NewServerText)
            {
                IEnvironmentModel localHost = viewModel.Servers.FirstOrDefault(c => c.IsLocalHost);
                if(localHost != null)
                {
                    viewModel.OpenNewConnectionWizard(localHost);

                    viewModel.AddMissingServers();
                }
            }
        }

        #endregion

        #region BindToActiveEnvironment

        public bool BindToActiveEnvironment
        {
            get { return (bool)GetValue(BindToActiveEnvironmentProperty); }
            set { SetValue(BindToActiveEnvironmentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BindToActiveEnvironment.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BindToActiveEnvironmentProperty =
            DependencyProperty.Register("BindToActiveEnvironment", typeof(bool), typeof(ConnectControlViewModel), new PropertyMetadata(false));
        IEnvironmentModel _selectedServer;

        #endregion

        #endregion

        #region Handlers

        #region Implementation of IHandle<SetActiveEnvironmentMessage>

        public void Handle(UpdateActiveEnvironmentMessage message)
        {
            this.TraceInfo(message.GetType().Name);
            if(message.EnvironmentModel != null && BindToActiveEnvironment)
            {
                SelectedServer = message.EnvironmentModel;
            }
        }

        #endregion

        #endregion

        #region Public Methods

        public virtual void OpenEditConnectionWizard(IEnvironmentModel localHost)
        {
            RootWebSite.ShowDialog(localHost, ResourceType.Server, null, string.Empty, SelectedServer.ID.ToString(), null, ConnectControlInstanceType, SelectedServer.Name);
        }

        public virtual void OpenNewConnectionWizard(IEnvironmentModel localHost)
        {
            RootWebSite.ShowDialog(localHost, ResourceType.Server, null, string.Empty, null, null, ConnectControlInstanceType);
        }

        public void AddMissingServers()
        {
            foreach(var environmentModel in EnvironmentRepository.Instance.All())
            {
                if(!Servers.Contains(environmentModel))
                {
                    Servers.Add(environmentModel);
                    if(BindToActiveEnvironment)
                    {
                        SelectedServer = environmentModel;
                    }
                }
            }
        }

        public void UpdateServer(IEnvironmentModel environmentToUpdate)
        {
            var environmentModel = EnvironmentRepository.Instance.FindSingle(c => c.ID == environmentToUpdate.ID);
            var index = Servers.IndexOf(environmentToUpdate);
            if(index != -1 && environmentModel != null)
            {
                Servers[index] = environmentModel;
            }
        }

        public void LoadServers(IEnvironmentModel envModel = null)
        {
            Servers.Clear();
            Servers.Add(CreateNewRemoteServerEnvironment());
            var servers = ServerProvider.Instance.Load();
            foreach(var server in servers)
            {
                Servers.Add(server);
            }
        }

        #endregion

        #region Private Methods

        public ICommand ConnectCommand
        {
            get
            {
                return _connectCommand ??
                       (_connectCommand = new RelayCommand(param => Connect()));
            }
        }

        public ICommand EditCommand
        {
            get
            {
                return _editCommand ??
                       (_editCommand = new RelayCommand(param => EditConnection()));
            }
        }


        void EditConnection()
        {
            if(SelectedServer != null)
            {
                var tempSelectedServer = SelectedServer;
                var localhost = Servers.FirstOrDefault(c => c.IsLocalHost);
                if(localhost != null)
                {
                    OpenEditConnectionWizard(localhost);
                    var firstOrDefault = Servers.FirstOrDefault(c => c.ID == tempSelectedServer.ID);
                    if(firstOrDefault != null)
                    {
                        UpdateServer(tempSelectedServer);
                        SelectedServer = tempSelectedServer;
                    }
                }
            }
        }

        void Connect()
        {
            if(SelectedServer != null)
            {
                if(!SelectedServer.IsConnected)
                {
                    SetBusyConnectingState();
                    var environment = SelectedServer;
                    _asyncWorker.Start(
                    environment.Connect,
                    () =>
                    {
                        _eventPublisher.Publish(new AddServerToExplorerMessage(environment));
                        SetConnectedState();
                    });
                }
                else
                {
                    SelectedServer.Disconnect();
                    SetDisconnectedState();
                }
            }
        }

        void SetConnectedState()
        {
            IsConnectButtonSpinnerVisible = false;
            IsEnabled = true;
        }

        void SetDisconnectedState()
        {
            IsConnectButtonSpinnerVisible = false;
            IsEnabled = true;
        }

        void SetBusyConnectingState()
        {
            IsConnectButtonSpinnerVisible = true;
            IsEnabled = false;
        }

        EnvironmentModel CreateNewRemoteServerEnvironment()
        {
            EnvironmentModel environmentModel = new EnvironmentModel(Guid.NewGuid(), new ServerProxy(new Uri("http://localhost:3142"))) { Name = NewServerText };
            return environmentModel;
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion INotifyPropertyChanged

        #region Implementation of IHandle<SetConnectControlSelectedServerMessage>

        public void Handle(SetConnectControlSelectedServerMessage message)
        {
            if(message != null && message.ConnectControlInstanceType == ConnectControlInstanceType && !Equals(message.SelectedServer, SelectedServer))
            {
                SelectedServer = message.SelectedServer;
            }
        }

        #endregion
    }
}