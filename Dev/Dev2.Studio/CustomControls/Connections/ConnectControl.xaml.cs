using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dev2.Common;
using Dev2.Composition;
using Dev2.Data.ServiceModel;
using Dev2.Studio.AppResources.ExtensionMethods;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Controller;
using Dev2.Studio.Core.InterfaceImplementors;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.ViewModels.Administration;
using Dev2.Studio.Webs;

namespace Dev2.UI
{
    /// <summary>
    /// Interaction logic for ConnectControl.xaml
    /// </summary>
    public partial class ConnectControl
    {
        #region CTOR

        public ConnectControl()
        {
            InitializeComponent();
            Servers = new ObservableCollection<IServer>();
            LoadServers();
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

        public IServer SelectedServer
        {
            get { return (IServer)GetValue(SelectedServerProperty); }
            set { SetValue(SelectedServerProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedServer.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedServerProperty =
            DependencyProperty.Register("SelectedServer", typeof(IServer), typeof(ConnectControl), 
            new PropertyMetadata(null, ServerChangedCallBack));

        private static void ServerChangedCallBack(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var control = (ConnectControl) dependencyObject;
            var selectedServer = e.NewValue as IServer;
            if (selectedServer != null)
            {
                var server = control.Servers.FirstOrDefault(s => s.ID == selectedServer.ID);
                control.TheServerComboBox.SelectedItem = server;
            }
        }

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

        #region Servers

        public IList<IServer> Servers
        {
            get { return (IList<IServer>)GetValue(ServersProperty); }
            private set { SetValue(ServersProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Servers.  This enables animation, styling, binding, etc...
        static readonly DependencyProperty ServersProperty =
            DependencyProperty.Register("Servers", typeof(IList<IServer>), typeof(ConnectControl));

        #endregion

        public Guid? Context
        {
            get { return (Guid?)GetValue(ContextProperty); }
            set { SetValue(ContextProperty, value); }
        }

       public static readonly DependencyProperty ContextProperty =
            DependencyProperty.Register("Context", typeof(Guid?), typeof(ConnectControl));

       public bool? IsEditEnabled
       {
           get { return (bool?)GetValue(IsEditEnabledProperty); }
           set { SetValue(IsEditEnabledProperty, value); }
       }

       public static readonly DependencyProperty IsEditEnabledProperty =
            DependencyProperty.Register("IsEditEnabled", typeof(bool?), typeof(ConnectControl));
        
        #endregion Dependency Properties

        #region LoadServers

        void LoadServers()
        {
            IsEditEnabled = false;
            Servers.Clear();
            var servers = ServerProvider.Instance.Load();
            foreach(var server in servers)
            {
                Servers.Add(server);
            }
        }

        #endregion

        #region OnServerDropDownOpened

        private void OnServerDropDownOpened(object sender, EventArgs e)
        {
            LoadServers();
        }

        #endregion

        #region OnServerSelectionChanged

        void OnServerSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(e.AddedItems != null && e.AddedItems.Count > 0)
            {
                var server = e.AddedItems[0] as IServer;
                if(server != null)
                {
                    InvokeCommands(server);
                }

                SelectedServer = server;

                IsEditEnabled = (SelectedServer != null && !SelectedServer.IsLocalHost);

                // Clear selection
                //TheServerComboBox.SelectedItem = null;
            }
        }

        private void InvokeCommands(IServer server)
        {
                    var environment = server.Environment ?? EnvironmentRepository.Instance.Fetch(server);
                    environment.CanStudioExecute = true;

                    //2013.06.02: Ashley Lewis for bug 9445 - environments do not autoconnect
                    environment.Connect();

                    //Used by deployviewmodel and settings - to do, please use only one.
            if (ServerChangedCommand != null && ServerChangedCommand.CanExecute(environment))
                    {
                        Dispatcher.BeginInvoke(new Action(() => ServerChangedCommand.Execute(server)));
                    }

                    //Used by rest.
            if (EnvironmentChangedCommand != null && EnvironmentChangedCommand.CanExecute(environment))
                    {
                        Dispatcher.BeginInvoke(new Action(() => EnvironmentChangedCommand.Execute(environment)));
                    }
                }

        #endregion

        #region OnConnectClick

        void OnEditClick(object sender, RoutedEventArgs e)
        {
            RootWebSite.ShowDialog(EnvironmentRepository.Instance.Source, ResourceType.Server, null, SelectedServer.ID, Context);
        }

        void OnNewClick(object sender, RoutedEventArgs e)
        {
            RootWebSite.ShowDialog(EnvironmentRepository.Instance.Source, ResourceType.Server, null, null, Context);
        }

        #endregion
    }
}
