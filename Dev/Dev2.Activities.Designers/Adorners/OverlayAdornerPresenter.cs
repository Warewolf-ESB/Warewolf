using System;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Imaging;

namespace Dev2.Activities.Adorners
{
    /// <summary>
    /// Wrapper used to wrap an adorner that hosts content as an overlay of an activity
    /// </summary>
    /// <author>Jurie.smit</author>
    /// <date>2013/07/24</date>
    public class OverlayAdornerPresenter : AdornerPresenterBase
    {
        private ButtonBase _button;

        /// <summary>
        /// The button which gets added to the adorner options of the activity
        /// </summary>
        /// <value>
        /// The button.
        /// </value>
        /// <date>2013/07/23</date>
        /// <author>Jurie.smit</author>
        /// <date>2013/07/24</date>
        public override ButtonBase Button
        {
            get
            {
                if (_button == null)
                {
                    _button = new AdornerToggleButton
                        {
                            Content = new Image
                                {
                                    Source = new BitmapImage(new Uri(ImageSourceUri, UriKind.RelativeOrAbsolute))
                                }
                        };
                    _button.SetValue(AutomationProperties.AutomationIdProperty, OverlayType.GetButtonAutomationId());
                }
                return _button;
            }
        }
    }
}
