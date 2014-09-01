using System;
using System.Windows;
using System.Windows.Navigation;

namespace Dev2.Views.Dialogs
{
    /// <summary>
    /// Interaction logic for WebLatestVersionDialog.xaml
    /// </summary>
    public partial class WebLatestVersionDialog : Window
    {
        public WebLatestVersionDialog()
        {
            InitializeComponent();
            Browser.Navigate(new Uri("http://www.warewolf.io/start_new.php"));
        }

        void wb_LoadCompleted(object sender, NavigationEventArgs e)
        {
            Browser.Width = Browser.ActualWidth + 32; 
            Browser.Height = Browser.ActualHeight + 32;
        }
    }
}
