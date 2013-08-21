using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace Dev2.Studio.CustomControls
{
    /// <summary>
    /// A Button-derived control that adds an IsSplitterExpanded property for custom template triggering.
    /// </summary>
    public class SplitterExpansionButton : System.Windows.Controls.Button
    {
        static SplitterExpansionButton()
        {
            //This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
            //This style is defined in Resources\SplitterExpansionButton.xaml
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SplitterExpansionButton), new FrameworkPropertyMetadata(typeof(SplitterExpansionButton)));
        }

        #region IsSplitterExpanded

        /// <summary>
        /// Identifies the 'IsMenuExpanded' dependency property
        /// </summary>
        public static readonly DependencyProperty IsSplitterExpandedProperty = DependencyProperty.Register("IsSplitterExpanded",
        typeof(bool), typeof(SplitterExpansionButton), new FrameworkPropertyMetadata());

        /// <summary>
        /// The image that was selected
        /// </summary>
        [Description("Indicates whether the splitter is expanded or not.")]
        [Category("Behavior")]
        public bool IsSplitterExpanded
        {
            get
            {
                return (bool)GetValue(IsSplitterExpandedProperty);
            }
            set
            {
                SetValue(IsSplitterExpandedProperty, value);
            }
        }

        #endregion
    }
}
