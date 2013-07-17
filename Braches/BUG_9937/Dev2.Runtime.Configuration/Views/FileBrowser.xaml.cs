using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Dev2.Runtime.Configuration.ComponentModel;
using Dev2.Runtime.Configuration.ViewModels;

namespace Dev2.Runtime.Configuration.Views
{
    /// <summary>
    /// Interaction logic for FileBrowser.xaml
    /// </summary>
    public partial class FileBrowser : Window
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
                this.DataContext = value;
            }
        }
        private void TreeItemExpanded(object sender, RoutedEventArgs e)
        {
            var item = e.OriginalSource as TreeViewItem;
            if (item == null)
            { return; }
            var cat = item.DataContext as ComputerDrive;
            if (cat == null)
            { return; }
            cat.LoadChildren();
        }
    }
}
