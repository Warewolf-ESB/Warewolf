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
using System.ComponentModel;
using System.Windows;
using Dev2.Common.Interfaces;
using Dev2.ConnectionHelpers;

namespace Warewolf.Studio.Views
{
    /// <summary>
    /// Interaction logic for ConnectControl.xaml
    /// </summary>
    public partial class ConnectControl
    {
        public ConnectControl()
        {
            InitializeComponent();
        }

        #region Automation ID's

        // ReSharper disable InconsistentNaming
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
        public IServer SelectedServer
        {
            get
            {
                return (IServer)TheServerComboBox.SelectedItem;
            }
            set
            {
                _selectedServer = value;
            }
        }

        // Using a DependencyProperty as the backing store for ConnectButtonAutomationID.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ConnectButtonAutomationIDProperty =
            DependencyProperty.Register("ConnectButtonAutomationID", typeof(string), typeof(ConnectControl),
                new PropertyMetadata("UI_ConnectServerBtn_AutoID"));
        IServer _selectedServer;

        #endregion

        public IConnectControlEnvironment SelectServer(string server)
        {
            foreach(var item in TheServerComboBox.Items)
            {
                var env = item as IConnectControlEnvironment;
                if (env != null && env.DisplayName == server)
                    try
                    {
                        TheServerComboBox.SelectedItem = env;
                        return env;
                    }
                    catch(Exception)
                    {
                        return env;
                       
                    }
                  
            }
            return null;
        }

        #region Implementation of IComponentConnector

        /// <summary>
        /// Attaches events and names to compiled content. 
        /// </summary>
        /// <param name="connectionId">An identifier token to distinguish calls.</param><param name="target">The target to connect events and names to.</param>
        public void Connect(int connectionId, object target)
        {
        }

        #endregion

        void UIElement_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue.Equals(false) && e.NewValue.Equals(true))
            {
                TheServerComboBox.IsEnabled = false;
                ConnectButton.Visibility = Visibility.Collapsed;
            }
            if (e.OldValue.Equals(true) && e.NewValue.Equals(false))
            {
                TheServerComboBox.IsEnabled = true;
                ConnectButton.Visibility = Visibility.Visible;
            }
        }

        void TheServerComboBox_OnDropDownClosing(object sender, CancelEventArgs e)
        {
            if (SelectedServer != null)
            {
                if (EditButton != null)
                {
                    EditButton.IsEnabled = SelectedServer.AllowEdit ;
                }

                if (SelectedServer.DisplayName.Contains("New Remote Server"))
                {
                    foreach (var item in TheServerComboBox.Items)
                    {
                        var env = item.Data as IServer;
                        if (env != null && env.DisplayName.Contains("localhost"))
                        {
                            try
                            {
                                TheServerComboBox.SelectedItem = env;
                            }
                            catch (Exception)
                            {
                                // ignored
                            }
                        }
                    }
                }
            }
        }
    }
}
