using System.Windows;
using System.Windows.Input;

namespace Dev2.Activities.Designers2.Core
{
    /// <summary>
    /// Interaction logic for ManageServiceInputView.xaml
    /// </summary>
    public partial class ManageWebServiceInputView
    {
        public ManageWebServiceInputView()
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
            ResponseGrid.Visibility = Visibility.Collapsed;
            OutputsGrid.Visibility = Visibility.Visible;
            DoneButton.IsEnabled = true;
        }

        void ManageWebServiceInputView_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}