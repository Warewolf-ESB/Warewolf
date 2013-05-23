using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dev2.Data.ServiceModel;
using Dev2.Studio.Core;
using Dev2.Studio.Core.InterfaceImplementors;
using Dev2.Studio.Core.Interfaces;
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


        public string ConnectButtonAutomationID
        {
            get { return (string)GetValue(ConnectButtonAutomationIDProperty); }
            set { SetValue(ConnectButtonAutomationIDProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ConnectButtonAutomationID.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ConnectButtonAutomationIDProperty =
            DependencyProperty.Register("ConnectButtonAutomationID", typeof(string), typeof(ConnectControl), new PropertyMetadata("UI_ServerConnectBtn_AutoID"));

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
            DependencyProperty.Register("LabelText", typeof(string), typeof(ConnectControl), new PropertyMetadata("Server"));

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

        
        #endregion Dependency Properties

        #region LoadServers

        void LoadServers()
        {
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
                    // BUG 9276 : TWR : 2013.04.19 - refactored so that we share environments
                    var environment = server.Environment ?? EnvironmentRepository.Instance.Fetch(server);
                    environment.CanStudioExecute = true;

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

                // Clear selection
                TheServerComboBox.SelectedItem = null;
            }
        }

        #endregion

        #region OnConnectClick

        void OnConnectClick(object sender, RoutedEventArgs e)
        {
            RootWebSite.ShowDialog(EnvironmentRepository.Instance.Source, ResourceType.Server, null, context: Context);
        }

        #endregion
    }
}
