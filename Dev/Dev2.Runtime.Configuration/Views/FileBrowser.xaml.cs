
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
using System.Windows.Controls;
using Dev2.Runtime.Configuration.ComponentModel;
using Dev2.Runtime.Configuration.ViewModels;

namespace Dev2.Runtime.Configuration.Views
{
    /// <summary>
    /// Interaction logic for FileBrowser.xaml
    /// </summary>
    public partial class FileBrowser
    {
        public FileBrowser()
        {
            InitializeComponent();
            AddHandler(TreeViewItem.ExpandedEvent, new RoutedEventHandler(TreeItemExpanded), true);
        }
        public LoggingViewModel Model
        {
            set
            {
                DataContext = value;
            }
        }
        private void TreeItemExpanded(object sender, RoutedEventArgs e)
        {
            var item = e.OriginalSource as TreeViewItem;
            if(item == null)
            { return; }
            var cat = item.DataContext as ComputerDrive;
            if(cat == null)
            { return; }
            cat.LoadChildren();
        }
    }
}
