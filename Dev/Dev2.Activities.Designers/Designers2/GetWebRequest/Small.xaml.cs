
using System.Windows;

namespace Dev2.Activities.Designers2.GetWebRequest
{
    public partial class Small
    {
        public Small()
        {
            InitializeComponent();
        }

        void FocusedTextBox_OnLoaded(object sender, RoutedEventArgs args)
        {
            var element = (FrameworkElement)sender;
            element.Focus();
        }      
    }
}
