using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Dev2.DynamicServices;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Factories;
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
            set { SetValue(LabelTextProperty, value); }
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
                    if(server.Environment == null)
                    {
                        EnvironmentModelFactory.CreateEnvironmentModel(server);
                    }
                    Mediator.SendMessage(MediatorMessages.AddServerToExplorer, server.Environment);
                }

                // Clear selection
                TheServerComboBox.SelectedItem = null;
            }
        }

        #endregion

        #region OnConnectClick

        void OnConnectClick(object sender, RoutedEventArgs e)
        {
            RootWebSite.ShowDialog(EnvironmentRepository.DefaultEnvironment, enSourceType.Dev2Server);
        }

        #endregion

    }
}
