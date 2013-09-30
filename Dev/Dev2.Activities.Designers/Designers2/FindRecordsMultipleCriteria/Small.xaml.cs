
using System.Windows;

namespace Dev2.Activities.Designers2.FindRecordsMultipleCriteria
{
    public partial class Small
    {
        public Small()
        {
            InitializeComponent();
        }
        
        void FocusedTextBoxOnLoaded(object sender, RoutedEventArgs args)
        {
            var element = (FrameworkElement)sender;
            element.Focus();
        }  
    }
}
