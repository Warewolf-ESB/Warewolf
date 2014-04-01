using System.Linq;
using System.Windows;
using System.Windows.Data;
using Caliburn.Micro;
using Dev2.AppResources.Enums;
using Dev2.Services.Events;
using Dev2.Studio;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.ViewModels;

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

        #region IsDropDownEnabled

        public bool IsDropDownEnabled
        {
            get { return (bool)GetValue(IsDropDownEnabledProperty); }
            set
            {
                SetValue(IsDropDownEnabledProperty, value);
            }
        }

        public static readonly DependencyProperty IsDropDownEnabledProperty =
            DependencyProperty.Register("IsDropDownEnabled", typeof(bool), typeof(ConnectControl), new PropertyMetadata(true));

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
            DependencyProperty.Register("ConnectControlInstanceType", typeof(ConnectControlInstanceType), typeof(ConnectControl), new PropertyMetadata(ConnectControlInstanceType.Explorer));

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

        public string ConnectButtonAutomationID
        {
            get { return (string)GetValue(ConnectButtonAutomationIDProperty); }
            set { SetValue(ConnectButtonAutomationIDProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ConnectButtonAutomationID.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ConnectButtonAutomationIDProperty =
            DependencyProperty.Register("ConnectButtonAutomationID", typeof(string), typeof(ConnectControl),
                new PropertyMetadata("UI_ConnectServerBtn_AutoID"));

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

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            var connectControlViewModel = ViewModel;
            if(connectControlViewModel != null)
            {
                #region Bindings
                BindingOperations.SetBinding(connectControlViewModel, ConnectControlViewModel.ConnectControlInstanceTypeProperty, new Binding(ConnectControlInstanceTypeProperty.Name)
                {
                    Source = this,
                    Mode = BindingMode.TwoWay,
                });

                BindingOperations.SetBinding(connectControlViewModel, ConnectControlViewModel.IsDropDownEnabledProperty, new Binding(IsDropDownEnabledProperty.Name)
                {
                    Source = this,
                    Mode = BindingMode.TwoWay,
                });

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

                #endregion

                connectControlViewModel.LoadServers();

                IMainViewModel mainViewModel = _mainApp.MainWindow.DataContext as IMainViewModel;
                if(ViewModel.BindToActiveEnvironment)
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
