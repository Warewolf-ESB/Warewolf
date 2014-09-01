using System.Windows;
using Dev2.Dialogs;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Views.Workflow
{
    /// <summary>
    /// Interaction logic for DsfActivityDropWindow.xaml
    /// </summary>
    public partial class DsfActivityDropWindow : IDialog
    {
        public DsfActivityDropWindow()
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
        }
    }
}
