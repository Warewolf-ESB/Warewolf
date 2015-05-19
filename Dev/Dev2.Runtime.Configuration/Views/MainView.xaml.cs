
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
using Dev2.Runtime.Configuration.ComponentModel;
using Dev2.Runtime.Configuration.ViewModels;

namespace Dev2.Runtime.Configuration.Views
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class MainView
    {
        public MainView()
        {
            InitializeComponent();
        }

        private void TreeView_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            MainViewModel mainViewModel = DataContext as MainViewModel;

            if(mainViewModel != null)
            {
                mainViewModel.SelectedSettingsObjects = e.NewValue as SettingsObject;
            }
        }
    }
}
