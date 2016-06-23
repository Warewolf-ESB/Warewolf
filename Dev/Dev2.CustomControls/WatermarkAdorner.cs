using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace Dev2.CustomControls
{
    internal class WatermarkAdorner : Adorner
    {
        #region Private Fields

        private readonly ContentPresenter _contentPresenter;

        #endregion

        #region Constructor

        public WatermarkAdorner(UIElement adornedElement, object watermark) :
            base(adornedElement)
        {
            IsHitTestVisible = false;

            _contentPresenter = new ContentPresenter();
            TextBlock textBlock = new TextBlock
                                  {
                                      FontStyle = FontStyles.Italic,
                                      VerticalAlignment = VerticalAlignment.Top,
                                      Text = watermark.ToString(),
                                      Padding = new Thickness(4)
                                  };
            _contentPresenter.Content = textBlock;
            _contentPresenter.Opacity = 0.5;
            _contentPresenter.Margin = new Thickness(Control.Margin.Left + Control.Padding.Left, Control.Margin.Top + Control.Padding.Top, 0, 0);

            if (Control is ItemsControl && !(Control is ComboBox))
            {
                _contentPresenter.VerticalAlignment = VerticalAlignment.Center;
                _contentPresenter.HorizontalAlignment = HorizontalAlignment.Center;
            }

            // Hide the control adorner when the adorned element is hidden
            Binding binding = new Binding("IsVisible")
                              {
                                  Source = adornedElement,
                                  Converter = new BooleanToVisibilityConverter()
                              };
            SetBinding(VisibilityProperty, binding);
        }

        #endregion

        #region Protected Properties

        protected override int VisualChildrenCount => 1;

        #endregion

        #region Private Properties

        private Control Control => (Control)AdornedElement;

        #endregion

        #region Protected Overrides

        protected override Visual GetVisualChild(int index)
        {
            return _contentPresenter;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            // Here's the secret to getting the adorner to cover the whole control
            _contentPresenter.Measure(Control.RenderSize);
            return Control.RenderSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            _contentPresenter.Arrange(new Rect(finalSize));
            return finalSize;
        }

        #endregion
    }
}
