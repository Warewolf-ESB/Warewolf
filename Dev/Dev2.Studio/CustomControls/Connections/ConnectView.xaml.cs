using System.Windows;

namespace Dev2.Studio.Views.Explorer
{
    /// <summary>
    /// Interaction logic for ConnectWindow.xaml
    /// </summary>
    public partial class ConnectView : Window 
    {
        public ConnectView() 
        {
            InitializeComponent();
            tbxServerName.Focus();
        }
    }
}
