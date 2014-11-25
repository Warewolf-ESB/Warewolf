
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Windows;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Dev2MessageBoxView
    {
        public Dev2MessageBoxView()
        {
            InitializeComponent();

            if(Application.Current == null || Application.Current.MainWindow == null || ReferenceEquals(this, Application.Current.MainWindow))
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
            else
            {
                Owner = Application.Current.MainWindow;
            }
        }
    }
}
