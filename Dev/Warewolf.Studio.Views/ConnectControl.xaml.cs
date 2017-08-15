/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Windows;

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

        void UIElement_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue.Equals(false) && e.NewValue.Equals(true))
            {
                TheServerComboBox.IsEnabled = false;
            }
            if (e.OldValue.Equals(true) && e.NewValue.Equals(false))
            {
                TheServerComboBox.IsEnabled = true;
            }
        }
    }
}
