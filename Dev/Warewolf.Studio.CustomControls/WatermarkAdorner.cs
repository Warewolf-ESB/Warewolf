using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace Warewolf.Studio.CustomControls
{
    internal class WatermarkAdorner : Adorner
    {
        #region Private Fields

        private readonly ContentPresenter contentPresenter;

        #endregion

        #region Constructor

        public WatermarkAdorner(UIElement adornedElement, object watermark) :
            base(adornedElement)
        {
            this.IsHitTestVisible = false;

            this.contentPresenter = new ContentPresenter();
            TextBlock textBlock = new TextBlock();
            textBlock.FontStyle = FontStyles.Italic;
            textBlock.VerticalAlignment = VerticalAlignment.Top;
            textBlock.Text = watermark.ToString();
            textBlock.Padding = new Thickness(4);
            this.contentPresenter.Content = textBlock;
            this.contentPresenter.Opacity = 0.5;
            this.contentPresenter.Margin = new Thickness(Control.Margin.Left + Control.Padding.Left, Control.Margin.Top + Control.Padding.Top, 0, 0);

            if (this.Control is ItemsControl && !(this.Control is ComboBox))
            {
                this.contentPresenter.VerticalAlignment = VerticalAlignment.Center;
                this.contentPresenter.HorizontalAlignment = HorizontalAlignment.Center;
            }

            // Hide the control adorner when the adorned element is hidden
            Binding binding = new Binding("IsVisible");
            binding.Source = adornedElement;
            binding.Converter = new BooleanToVisibilityConverter();
            this.SetBinding(VisibilityProperty, binding);
        }

        #endregion

        #region Protected Properties

        protected override int VisualChildrenCount
        {
            get { return 1; }
        }

        #endregion

        #region Private Properties

        private Control Control
        {
            get { return (Control)this.AdornedElement; }
        }

        #endregion

        #region Protected Overrides

        protected override Visual GetVisualChild(int index)
        {
            return this.contentPresenter;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            // Here's the secret to getting the adorner to cover the whole control
            this.contentPresenter.Measure(Control.RenderSize);
            return Control.RenderSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            this.contentPresenter.Arrange(new Rect(finalSize));
            return finalSize;
        }

        #endregion
    }
}
