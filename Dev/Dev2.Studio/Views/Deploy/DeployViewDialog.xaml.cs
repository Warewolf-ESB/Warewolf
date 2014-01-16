using Dev2.Dialogs;
using System.Windows;

namespace Dev2.Views.Deploy
{
    /// <summary>
    /// Interaction logic for DeployViewDialog.xaml
    /// </summary>
    public partial class DeployViewDialog : IDialog
    {
        public DeployViewDialog()
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
        }
    }
}
