using System;
using System.Windows;
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
        private ToggleButton _button;

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
                                },
                             ToolTip = ToolTip
                        };

                    if (ExpandedImageSourceUri != string.Empty)
                    {
                        _button.Checked += (o, e) =>
                            {
                                _button.Content = new Image
                                    {
                                        Source =
                                            new BitmapImage(new Uri(ExpandedImageSourceUri, UriKind.RelativeOrAbsolute))
                                    };
                                _button.ToolTip = ExpandedToolTip;
                            };
                        _button.Unchecked += (o, e) =>
                        {
                            _button.Content = new Image
                            {
                                Source =
                                    new BitmapImage(new Uri(ImageSourceUri, UriKind.RelativeOrAbsolute))
                            };
                            _button.ToolTip = ToolTip;
                        };
                    }

                    if (ExpandedToolTip != string.Empty)
                    {
                        _button.Checked += (o, e) =>
                        {
                            _button.ToolTip = ExpandedToolTip;
                        };
                        _button.Unchecked += (o, e) =>
                        {
                            _button.ToolTip = ToolTip;
                        };
                    }

                    _button.SetValue(AutomationProperties.AutomationIdProperty, OverlayType.GetButtonAutomationId());
                }
                return _button;
            }
        }

        #region dependency properties

        public string ExpandedImageSourceUri
        {
            get { return (string)GetValue(ExpandedImageSourceUriProperty); }
            set { SetValue(ExpandedImageSourceUriProperty, value); }
        }

        public static readonly DependencyProperty ExpandedImageSourceUriProperty =
            DependencyProperty.Register("ExpandedImageSourceUri", typeof(string), 
            typeof(OverlayAdornerPresenter), new PropertyMetadata(string.Empty));



        public string ExpandedToolTip
        {
            get { return (string)GetValue(ExpandedToolTipProperty); }
            set { SetValue(ExpandedToolTipProperty, value); }
        }

        public static readonly DependencyProperty ExpandedToolTipProperty =
            DependencyProperty.Register("ExpandedToolTip", typeof(string), 
            typeof(OverlayAdornerPresenter), new PropertyMetadata(string.Empty));

        
        #endregion
    }
}
