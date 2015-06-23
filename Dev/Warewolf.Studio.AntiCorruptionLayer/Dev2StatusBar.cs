using System.Windows;
using System.Windows.Controls;

namespace Warewolf.Studio.AntiCorruptionLayer
{
    public class Dev2StatusBar : UserControl
    {
        public string StatusBarLabelText
        {
            get { return (string)GetValue(StatusBarLabelTextProperty); }
            set { SetValue(StatusBarLabelTextProperty, value); }
        }
        // Using a DependencyProperty as the backing store for StatusBarLabelText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StatusBarLabelTextProperty =
            DependencyProperty.Register("StatusBarLabelText", typeof(string), typeof(Dev2StatusBar), new PropertyMetadata(string.Empty));


        public Visibility ProgressBarVisiblity
        {
            get { return (Visibility)GetValue(ProgressBarVisiblityProperty); }
            set { SetValue(ProgressBarVisiblityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ProgressBarVisiblity.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ProgressBarVisiblityProperty =
            DependencyProperty.Register("ProgressBarVisiblity", typeof(Visibility), typeof(Dev2StatusBar), new PropertyMetadata(Visibility.Visible));

        public Dev2StatusBar()
        {
            DefaultStyleKey = typeof(Dev2StatusBar);
        }
    }
}