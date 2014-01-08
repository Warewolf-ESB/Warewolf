using Dev2.Dialogs;
using System.Windows;

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
