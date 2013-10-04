using System.Windows;
using System.Windows.Input;

namespace Dev2.Activities.Designers2.FindRecordsMultipleCriteria
{
    public partial class Small
    {
        public Small()
        {
            InitializeComponent();
            DataGrid = SmallDataGrid;
        }

        void FocusedTextBoxOnLoaded(object sender, RoutedEventArgs args)
        {
            var element = (FrameworkElement)sender;
            Keyboard.Focus(element);
        }

        void FocusedTextBoxOnLostFocus(object sender, RoutedEventArgs e)
        {
            var focus = FocusManager.GetFocusedElement(this.Parent);
            if(focus != null)
            {
            }
        }
    }
}
