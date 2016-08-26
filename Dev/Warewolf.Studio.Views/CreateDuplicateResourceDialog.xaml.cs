using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Dev2.Common.Interfaces;
using Warewolf.Studio.Core;

namespace Warewolf.Studio.Views
{
    /// <summary>
    /// Interaction logic for CreateDuplicateResourceDialog.xaml
    /// </summary>
    public partial class CreateDuplicateResourceDialog : ICreateDuplicateResourceView
    {
        readonly Grid _blackoutGrid = new Grid();

        public CreateDuplicateResourceDialog()
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        private void CreateDuplicateResourceDialog_OnClosing(object sender, CancelEventArgs e)
        {
            PopupViewManageEffects.RemoveBlackOutEffect(_blackoutGrid);
        }

        #region Implementation of ICreateDuplicateResourceView

        public void CloseView()
        {
            PopupViewManageEffects.RemoveBlackOutEffect(_blackoutGrid);
            Close();
        }

        public void ShowView()
        {
            PopupViewManageEffects.AddBlackOutEffect(_blackoutGrid);
            ShowDialog();
        }

        #endregion
    }
}
