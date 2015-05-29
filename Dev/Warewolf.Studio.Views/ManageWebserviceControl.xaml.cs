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
using Infragistics.Controls.Grids;

namespace Warewolf.Studio.Views
{
    /// <summary>
    /// Interaction logic for ManageWebserviceControl.xaml
    /// </summary>
    public partial class ManageWebserviceControl
    {
        public ManageWebserviceControl()
        {
            InitializeComponent();
        }
        private void Btn_OnClick(object sender, RoutedEventArgs e)
        {
            var manageWebservicePasteView = new ManageWebservicePasteView();
            manageWebservicePasteView.ShowView();
        }

        private void XamContextMenu_ItemClicked(object sender, Infragistics.Controls.Menus.ItemClickedEventArgs e)
        {

        }

        private void XamMenuItem_Click(object sender, EventArgs e)
        {
            
        }
    }
}
