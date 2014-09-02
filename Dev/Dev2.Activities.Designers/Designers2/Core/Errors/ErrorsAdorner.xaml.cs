using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;

namespace Dev2.Activities.Designers2.Core.Errors
{
    public partial class ErrorsAdorner
    {
        int ErrorsCounter = 0;

        public ErrorsAdorner(FrameworkElement adornedElement)
            : base(adornedElement)
        {
            InitializeComponent();
        }

        void ErrorTextBlock_Loaded(object sender, RoutedEventArgs e)
        {
            var textBlock = sender as TextBlock;
            if(textBlock != null)
            {
                textBlock.SetValue(AutomationProperties.AutomationIdProperty, "UI_Error" + ErrorsCounter++ + "_AutoID");
            }
        }

        void ErrorsGrid_Loaded(object sender, RoutedEventArgs e)
        {
            ErrorsCounter = 0;
        }
    }
}
