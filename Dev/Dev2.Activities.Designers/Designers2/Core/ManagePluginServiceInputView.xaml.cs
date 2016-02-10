using System.Windows;
using System.Windows.Input;

namespace Dev2.Activities.Designers2.Core
{
    /// <summary>
    /// Interaction logic for ManageServiceInputView.xaml
    /// </summary>
    public partial class ManagePluginServiceInputView
    {
        public ManagePluginServiceInputView()
        {
            InitializeComponent();
            DoneButton.IsEnabled = false;
        }

        public void ShowView()
        {
            Show();
        }

        public void RequestClose()
        {
            Close();
        }

        void TestActionButton_OnClick(object sender, RoutedEventArgs e)
        {
            DoneButton.IsEnabled = true;
        }

        void ManagePluginServiceInputView_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}