using System.Windows;

namespace Dev2.CustomControls.Progress
{
    /// <summary>
    /// Interaction logic for ProgressDialog.xaml
    /// </summary>
    public partial class ProgressDialog
    {
        public ProgressDialog(Window owner)
        {
            Owner = owner;
            InitializeComponent();
        }
    }
}
