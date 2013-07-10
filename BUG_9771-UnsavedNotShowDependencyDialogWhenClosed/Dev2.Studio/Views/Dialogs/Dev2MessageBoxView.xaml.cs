using System.Windows;

namespace Dev2.Studio.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Dev2MessageBoxView : Window
    {
        public Dev2MessageBoxView()
        {
            InitializeComponent();

            if (Application.Current == null || Application.Current.MainWindow == null || ReferenceEquals(this, App.Current.MainWindow))
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
            else
            {
                Owner = App.Current.MainWindow;   
            }
        }
    }
}
