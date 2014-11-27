
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
namespace Unlimited.Applications.BusinessDesignStudio.Views
{
    /// <summary>
    /// Interaction logic for ConfigureDecisionWindow.xaml
    /// </summary>
    public partial class ConfigureDecisionWindow
    {
        public ConfigureDecisionWindow()
        {
            InitializeComponent();
        }

        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            dynamic data = DataContext;
            if(!data.CanSelect)
            {
                MessageBox.Show("Please choose a decision type", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                e.Cancel = true;
            }
        }
    }
}
