
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


// ReSharper disable CheckNamespace

using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;

namespace Dev2.Studio.Views.Diagnostics
{
    /// <summary>
    /// Interaction logic for DebugOutputWindow.xaml
    /// </summary>
    public partial class DebugOutputView
    {
        public DebugOutputView()
        {
            InitializeComponent();
        }

        void DebugOutputGrid_OnLoaded(object sender, RoutedEventArgs e)
        {
            (sender as Grid).Parent.SetValue(AutomationProperties.AutomationIdProperty, "UI_DebugOutputGrid_AutoID");
        }
    }
}
