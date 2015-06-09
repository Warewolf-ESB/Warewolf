using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Infragistics.Controls.Menus;
using Warewolf.Studio.Core;

namespace Warewolf.Studio.Views
{
    /// <summary>
    /// Interaction logic for ManagePluginSourceControl.xaml
    /// </summary>
    public partial class ManagePluginSourceControl : UserControl
    {
        public ManagePluginSourceControl()
        {
            InitializeComponent();
        }

        private void ExplorerTree_OnNodeExpansionChanging(object sender, CancellableNodeExpansionChangedEventArgs e)
        {
            if (DataContext != null)
            {
                var node = e.Node.Data as DllListingModel;
                if (node != null)
                {
                    node.IsExpanded = e.Node.IsExpanded;
                    node.ExpandingCommand.Execute(null);
                }
            }
        }
    }
}
