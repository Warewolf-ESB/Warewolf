/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using Dev2.Common;

namespace Dev2.Activities.Designers2.Core.Adorners
{
    [ContentProperty("Content")]
    public class AdornerControl : Adorner
    {
        const double OffsetX = 0.0;
        const double OffsetY = 0.0;

        private const double PositionX = Double.NaN;
        private const double PositionY = Double.NaN;

        AdornerLayer _adornerLayer;

        public AdornerControl(FrameworkElement adornedElement)
            : base(adornedElement)
        {
            SetBinding(DataContextProperty, new Binding("DataContext")
            {
                Source = adornedElement,
                Mode = BindingMode.TwoWay
            });

            adornedElement.SizeChanged += (sender, args) => InvalidateMeasure();
        }

        public AdornerLayer AdornerLayer => _adornerLayer ?? (_adornerLayer = AdornerLayer.GetAdornerLayer(AdornedElement));

        /// <summary>
        ///     Override AdornedElement from base class for less type-checking.
        /// </summary>
        public new FrameworkElement AdornedElement => (FrameworkElement)base.AdornedElement;

        public FrameworkElement Content { get { return (FrameworkElement)GetValue(ContentProperty); } set { SetValue(ContentProperty, value); } }

        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register("Content", typeof(FrameworkElement), typeof(AdornerControl), new PropertyMetadata(null));

        public bool IsAdornerVisible { get { return (bool)GetValue(IsAdornerVisibleProperty); } set { SetValue(IsAdornerVisibleProperty, value); } }

        public static readonly DependencyProperty IsAdornerVisibleProperty =
            DependencyProperty.Register("IsAdornerVisible", typeof(bool), typeof(AdornerControl), new PropertyMetadata(false, OnIsAdornerVisibleChanged));

        static void OnIsAdornerVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var adorner = (AdornerControl)d;
            adorner?.Toggle();
        }

        protected override Int32 VisualChildrenCount => 1;

        protected override IEnumerator LogicalChildren
        {
            get
            {
                var list = new ArrayList { Content };
                return list.GetEnumerator();
            }
        }

        protected override Visual GetVisualChild(Int32 index)
        {
            return Content;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            Content.Measure(constraint);
            return Content.DesiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var x = PositionX;
            if(Double.IsNaN(x))
            {
                x = DetermineX();
            }
            var y = PositionY;
            if(Double.IsNaN(y))
            {
                y = DetermineY();
            }
            var adornerWidth = DetermineWidth();
            var adornerHeight = DetermineHeight();
            Content.Arrange(new Rect(x, y, adornerWidth, adornerHeight));
            return finalSize;
        }

        double DetermineX()
        {
            var adornedWidth = AdornedElement.ActualWidth;
            var adornerWidth = Content.DesiredSize.Width;
            switch(Content.HorizontalAlignment)
            {
                case HorizontalAlignment.Left:
                    return -adornerWidth + OffsetX;

                case HorizontalAlignment.Right:
                    return adornedWidth + OffsetX;

                case HorizontalAlignment.Center:
                    var x = adornedWidth / 2 - adornerWidth / 2;
                    return x + OffsetX;

                case HorizontalAlignment.Stretch:
                    return 0.0;
            }
            return 0.0;
        }

        double DetermineY()
        {
            var adornedHeight = AdornedElement.ActualHeight;
            var adornerHeight = Content.DesiredSize.Height;
            switch(Content.VerticalAlignment)
            {
                case VerticalAlignment.Top:
                    return OffsetY;

                case VerticalAlignment.Bottom:
                    return adornedHeight - adornerHeight + OffsetY;

                case VerticalAlignment.Center:
                    var x = adornedHeight / 2 - adornerHeight / 2;
                    return x + OffsetY;

                case VerticalAlignment.Stretch:
                    return 0.0;
            }
            return 0.0;
        }

        double DetermineWidth()
        {
            if(!Double.IsNaN(PositionX))
            {
                return Content.DesiredSize.Width;
            }

            switch(Content.HorizontalAlignment)
            {
                case HorizontalAlignment.Left:
                case HorizontalAlignment.Right:
                case HorizontalAlignment.Center:
                    return Content.DesiredSize.Width;
                case HorizontalAlignment.Stretch:
                    return AdornedElement.ActualWidth;
            }

            return 0.0;
        }

        double DetermineHeight()
        {
            var height = Math.Max(Content.MinHeight, Content.DesiredSize.Height);
            if(!Double.IsNaN(PositionY))
            {
                return height;
            }

            switch(Content.VerticalAlignment)
            {
                case VerticalAlignment.Top:
                case VerticalAlignment.Bottom:
                case VerticalAlignment.Center:
                    return height;
                case VerticalAlignment.Stretch:
                    return AdornedElement.ActualHeight;
            }
            return 0.0;
        }

        void Toggle()
        {
            try
            {
                if(IsAdornerVisible)
                {
                    AddLogicalChild(Content);
                    AddVisualChild(Content);
                    AdornerLayer?.Add(this);
                }
                else
                {
                    AdornerLayer?.Remove(this);
                    RemoveLogicalChild(Content);
                    RemoveVisualChild(Content);
                }
            }
            catch(Exception e)
            {
                Dev2Logger.Error("Error toggling adorner: ",e);
            }
        }
    }
}
