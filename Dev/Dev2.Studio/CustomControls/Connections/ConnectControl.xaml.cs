using System;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using Caliburn.Micro;
using Dev2.Data.ServiceModel;
using Dev2.Services.Events;
using Dev2.Studio;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.ViewModels;
using Dev2.Studio.Webs;

// ReSharper disable CheckNamespace
namespace Dev2.UI
{
    /// <summary>
    /// Interaction logic for ConnectControl.xaml
    /// </summary>
    public partial class ConnectControl
    {
        readonly IEventAggregator _eventPublisher;
        readonly IApp _mainApp;

        #region CTOR

        public ConnectControl()
            : this(EventPublishers.Aggregator, Application.Current as IApp)
        {
        }

        public ConnectControl(IEventAggregator eventPublisher, IApp mainApp)
        {
            InitializeComponent();
            VerifyArgument.IsNotNull("eventPublisher", eventPublisher);
            VerifyArgument.IsNotNull("mainApp", mainApp);
            _eventPublisher = eventPublisher;
            _mainApp = mainApp;
            _eventPublisher.Subscribe(this);
            ViewModel = new ConnectControlViewModel(null, _eventPublisher);
            DataContext = ViewModel;
            Loaded += OnLoaded;
        }

        #endregion

        public ConnectControlViewModel ViewModel { get; set; }

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

        #region LabelText

        public string LabelText
        {
            get { return (string)GetValue(LabelTextProperty); }
            set { SetValue(LabelTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LabelText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LabelTextProperty =
            DependencyProperty.Register("LabelText", typeof(string),
                typeof(ConnectControl), new PropertyMetadata("Server "));

        #endregion

        #region Context

        public Guid? Context
        {
            get { return (Guid?)GetValue(ContextProperty); }
            set { SetValue(ContextProperty, value); }
        }

        public static readonly DependencyProperty ContextProperty =
            DependencyProperty.Register("Context", typeof(Guid?), typeof(ConnectControl));

        #endregion

        void OnEditClick(object sender, RoutedEventArgs e)
        {
            RootWebSite.ShowDialog(ViewModel.SelectedServer, ResourceType.Server, null, ViewModel.SelectedServer.ID.ToString(), null, Context);
            ViewModel.LoadServers();
        }

        void OnNewClick(object sender, RoutedEventArgs e)
        {
            IEnvironmentModel localHost = ViewModel.Servers.FirstOrDefault(c => c.IsLocalHost);
            if(localHost != null)
            {
                RootWebSite.ShowDialog(localHost, ResourceType.Server, null, null, null, Context);
                ViewModel.LoadServers();
            }
        }

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            var connectControlViewModel = ViewModel;
            if(connectControlViewModel != null)
            {
                BindingOperations.SetBinding(connectControlViewModel, ConnectControlViewModel.BindToActiveEnvironmentProperty, new Binding(BindToActiveEnvironmentProperty.Name)
                {
                    Source = this,
                    Mode = BindingMode.TwoWay,
                });

                BindingOperations.SetBinding(connectControlViewModel, ConnectControlViewModel.LabelTextProperty, new Binding(LabelTextProperty.Name)
                {
                    Source = this,
                    Mode = BindingMode.TwoWay,
                });

                BindingOperations.SetBinding(connectControlViewModel, ConnectControlViewModel.ContextProperty, new Binding(ContextProperty.Name)
                {
                    Source = this,
                    Mode = BindingMode.TwoWay,
                });

                connectControlViewModel.LoadServers();

                IMainViewModel mainViewModel = _mainApp.MainWindow.DataContext as IMainViewModel;
                if(ViewModel.IsSourceServer || ViewModel.BindToActiveEnvironment)
                {
                    if(mainViewModel != null)
                    {
                        ViewModel.SelectedServer = mainViewModel.ActiveEnvironment;
                    }
                }
                else
                {
                    IEnvironmentModel environmentModel = EnvironmentRepository.Instance.FindSingle(c => c.IsConnected && c.ID != mainViewModel.ActiveEnvironment.ID);
                    if(environmentModel != null)
                    {
                        ViewModel.SelectedServer = environmentModel;
                    }
                }

                if(ViewModel.SelectedServer == null && ViewModel.Servers.Any())
                {
                    IEnvironmentModel localhost = ViewModel.Servers.FirstOrDefault(c => c.IsLocalHost);
                    if(localhost != null)
                    {
                        ViewModel.SelectedServer = localhost;
                    }
                }
            }

            Loaded -= OnLoaded;
        }
    }
}
