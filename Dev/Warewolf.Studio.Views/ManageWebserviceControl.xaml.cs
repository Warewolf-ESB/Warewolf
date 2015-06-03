using System;
using System.Windows;

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
