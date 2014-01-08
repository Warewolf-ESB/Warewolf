using System.Windows;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Dev2MessageBoxView
    {
        public Dev2MessageBoxView()
        {
            InitializeComponent();

            if(Application.Current == null || Application.Current.MainWindow == null || ReferenceEquals(this, Application.Current.MainWindow))
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
            else
            {
                Owner = Application.Current.MainWindow;
            }
        }
    }
}
