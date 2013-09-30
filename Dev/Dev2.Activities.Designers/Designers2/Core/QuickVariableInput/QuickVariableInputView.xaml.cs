using System.Windows;

namespace Dev2.Activities.Designers2.Core.QuickVariableInput
{
    public partial class QuickVariableInputView
    {
        public QuickVariableInputView()
        {
            InitializeComponent();
        }

        void VarialbeListOnLoaded(object sender, RoutedEventArgs args)
        {
            var element = (FrameworkElement)sender;
            element.Focus();
        }
    }
}
