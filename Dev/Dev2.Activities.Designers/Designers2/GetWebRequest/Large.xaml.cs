using System.Windows;

namespace Dev2.Activities.Designers2.GetWebRequest
{
    public partial class Large
    {
        public Large()
        {
            InitializeComponent();
        }

        private void UrlOnLoaded(object sender, RoutedEventArgs e)
        {
            var element = (FrameworkElement)sender;
            element.Focus();
        }
    }
}