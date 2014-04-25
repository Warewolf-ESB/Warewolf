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
