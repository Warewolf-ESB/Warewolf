using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Caliburn.Micro;
using Dev2.Annotations;
using Dev2.Data.ServiceModel;
using Dev2.Messages;
using Dev2.Providers.Logs;
using Dev2.Services.Events;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.ViewModels;
using Dev2.Studio.Webs;
using Action = System.Action;

namespace Dev2.UI
{
    /// <summary>
    /// Interaction logic for ConnectControl.xaml
    /// </summary>
    public partial class ConnectControl : IConnectControl,INotifyPropertyChanged
    {
        readonly IEventAggregator _eventPublisher;
        ConnectControlViewModel _viewModel;

        #region CTOR

        public ConnectControl()
            : this(EventPublishers.Aggregator)
        {
        }

        public ConnectControl(IEventAggregator eventPublisher)
        {
            InitializeComponent();
            VerifyArgument.IsNotNull("eventPublisher", eventPublisher);
            _eventPublisher = eventPublisher;
            _eventPublisher.Subscribe(this);
            Loaded += OnLoaded;
        }

        #endregion

        #region Dependency Properties

        #region Server Changed Command

        public ICommand ServerChangedCommand
        {
            get
            {
                return (ICommand)GetValue(ServerChangedCommandProperty);
            }
            set
            {
                SetValue(ServerChangedCommandProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for ServerChangedCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ServerChangedCommandProperty =
            DependencyProperty.Register("ServerChangedCommand", typeof(ICommand), typeof(ConnectControl), new PropertyMetadata(null));

        #endregion

        #region Environment Changed Command

        public ICommand EnvironmentChangedCommand
        {
            get
            {
                return (ICommand)GetValue(EnvironmentChangedCommandProperty);
            }
            set
            {
                SetValue(EnvironmentChangedCommandProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for EnvironmentChangedCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EnvironmentChangedCommandProperty =
            DependencyProperty.Register("EnvironmentChangedCommand", typeof(ICommand), typeof(ConnectControl), new PropertyMetadata(null));

        #endregion

        #region Automation ID's

        public string ServerComboBoxAutomationID
        {
            get { return (string)GetValue(ServerComboBoxAutomationIDProperty); }
            set { SetValue(ServerComboBoxAutomationIDProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ServerComboBoxAutomationID.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ServerComboBoxAutomationIDProperty =
            DependencyProperty.Register("ServerComboBoxAutomationID", typeof(string), typeof(ConnectControl), new PropertyMetadata("UI_ServerCbx_AutoID"));


        public string EditButtonAutomationID
        {
            get { return (string)GetValue(EditButtonAutomationIDProperty); }
            set { SetValue(EditButtonAutomationIDProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ConnectButtonAutomationID.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EditButtonAutomationIDProperty =
            DependencyProperty.Register("EditButtonAutomationID", typeof(string), typeof(ConnectControl),
                new PropertyMetadata("UI_ServerEditBtn_AutoID"));

        public string NewButtonAutomationID
        {
            get { return (string)GetValue(NewButtonAutomationIDProperty); }
            set { SetValue(NewButtonAutomationIDProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ConnectButtonAutomationID.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NewButtonAutomationIDProperty =
            DependencyProperty.Register("NewButtonAutomationID", typeof(string), typeof(ConnectControl),
                new PropertyMetadata("UI_NewServerBtn_AutoID"));

        #endregion

        #region BindToActiveEnvironment

        public bool BindToActiveEnvironment
        {
            get { return (bool)GetValue(BindToActiveEnvironmentProperty); }
            set { SetValue(BindToActiveEnvironmentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BindToActiveEnvironment.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BindToActiveEnvironmentProperty =
            DependencyProperty.Register("BindToActiveEnvironment", typeof(bool), typeof(ConnectControl), new PropertyMetadata(false));

        #endregion

        

        #region LabelText

        public string LabelText
        {
            get { return (string)GetValue(LabelTextProperty); }
            set
            {
                SetValue(LabelTextProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for LabelText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LabelTextProperty =
            DependencyProperty.Register("LabelText", typeof(string),
                typeof(ConnectControl), new PropertyMetadata("Server "));

        #endregion

        

        public Guid? Context
        {
            get { return (Guid?)GetValue(ContextProperty); }
            set { SetValue(ContextProperty, value); }
        }

        public static readonly DependencyProperty ContextProperty =
            DependencyProperty.Register("Context", typeof(Guid?), typeof(ConnectControl));

       
        #endregion Dependency Properties


        #region OnServerDropDownOpened

        public ConnectControlViewModel ViewModel
        {
            get
            {
                return _viewModel;
            }
            set
            {
                _viewModel = value;     
                OnPropertyChanged();
            }
        }

        void OnServerDropDownOpened(object sender, EventArgs e)
        {
            ViewModel.LoadServers();
        }

        #endregion

        #region OnServerSelectionChanged

        void OnServerSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(e.AddedItems != null && e.AddedItems.Count > 0)
            {
                var server = e.AddedItems[0] as IServer;
                SelectionHasChanged(server);
            }
        }

        void SelectionHasChanged(IServer server)
        {
            //
            if(server != null && (ViewModel != null && ViewModel.IsSelectedFromDropDown))
            {
                InvokeCommands(server);
                Logger.TraceInfo("Publish message of type - " + typeof(SetSelectedItemInExplorerTree));
                _eventPublisher.Publish(new SetSelectedItemInExplorerTree(server.Environment.Name));
                Logger.TraceInfo("Publish message of type - " + typeof(SetActiveEnvironmentMessage));
                _eventPublisher.Publish(new SetActiveEnvironmentMessage(server.Environment));
            }
            else
            {
                if(ViewModel != null)
                {
                    ViewModel.IsEditEnabled = (ViewModel.SelectedServer != null && !ViewModel.SelectedServer.IsLocalHost);
                }
            }
        }

        void InvokeCommands(IServer server)
        {
            var environment = server.Environment ?? EnvironmentRepository.Instance.Fetch(server);
            environment.CanStudioExecute = true;

            //2013.06.02: Ashley Lewis for bug 9445 - environments do not autoconnect
            environment.Connect();

            //Used by deployviewmodel and settings - to do, please use only one.
            if(ServerChangedCommand != null && ServerChangedCommand.CanExecute(environment))
            {
                Dispatcher.BeginInvoke(new Action(() => ServerChangedCommand.Execute(server)));
            }

            //Used by rest.
            if(EnvironmentChangedCommand != null && EnvironmentChangedCommand.CanExecute(environment))
            {
                Dispatcher.BeginInvoke(new Action(() => EnvironmentChangedCommand.Execute(environment)));
            }
        }

        #endregion

        #region OnConnectClick

        void OnEditClick(object sender, RoutedEventArgs e)
        {
            RootWebSite.ShowDialog(ViewModel.ActiveEnvironment, ResourceType.Server, null, ViewModel.SelectedServer.ID, Context);
        }

        void OnNewClick(object sender, RoutedEventArgs e)
        {
            RootWebSite.ShowDialog(ViewModel.ActiveEnvironment, ResourceType.Server, null, null, Context);
        }

        #endregion

        #region Implementation of IHandle<SetActiveEnvironmentMessage>

        public void Handle(UpdateActiveEnvironmentMessage message)
        {
            Logger.TraceInfo(message.GetType().Name);
            if(message.EnvironmentModel != null && BindToActiveEnvironment)
            {
                if(ViewModel != null)
                {
                    ViewModel.ChangeSelected(message.EnvironmentModel);
                }
            }
        }

        #endregion

        void OnServerDropDownClosed(object sender, EventArgs e)
        {
            if(TheServerComboBox.SelectedItem == null)
            {
                if(ViewModel != null)
                {
                    ViewModel.ChangeSelected(ViewModel.ActiveEnvironment);
                    
                }
            }
            else
            {
                ViewModel.ChangeSelected(ViewModel.SelectedServer.Environment);
                SelectionHasChanged(ViewModel.SelectedServer);
            }
        }

        

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            ViewModel = ConnectControlViewModel.BuildConnectControlViewModel(((IMainViewModel)Application.Current.MainWindow.DataContext).DeployResource, ((IMainViewModel)Application.Current.MainWindow.DataContext).ActiveEnvironment);
            // 2013.09.02 - BUG 10221 - set default server selection
            ViewModel.LoadServers();
            ViewModel.SelectedServer = ViewModel.GetSelectedServer(ViewModel.Servers, LabelText);
            if(ViewModel.SelectedServer != null)
            {
                ViewModel.ChangeSelected(ViewModel.SelectedServer.Environment);
                SelectionHasChanged(ViewModel.SelectedServer);
            }
            Loaded -= OnLoaded;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if(handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
