using Dev2.Dialogs;

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
            Owner = App.Current.MainWindow;
        }
    }
}
