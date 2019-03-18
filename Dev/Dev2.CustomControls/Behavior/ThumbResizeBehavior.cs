#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Interactivity;

namespace Dev2.Studio.AppResources.Behaviors
{
    public class ThumbResizeBehavior : Behavior<Thumb>
    {
        const int ChangeThreshold = 1;

        public double MinWidthOffset { get; set; }

        public double MinHeightOffset { get; set; }

        #region Override Methods

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.Unloaded += AssociatedObjectOnUnloaded;
            AssociatedObject.DragDelta += AssociatedObjectOnDragDelta;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.Unloaded += AssociatedObjectOnUnloaded;
            AssociatedObject.DragDelta -= AssociatedObjectOnDragDelta;
        }

        #endregion Override Methods

        #region Properties

        // Using a DependencyProperty as the backing store for TargetElement.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TargetElementProperty =
            DependencyProperty.Register("TargetElement", typeof (FrameworkElement), typeof (ThumbResizeBehavior),
                new PropertyMetadata(null));

        // Using a DependencyProperty as the backing store for ContentElement.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ContentElementProperty =
            DependencyProperty.Register("ContentElement", typeof (FrameworkElement), typeof (ThumbResizeBehavior),
                new PropertyMetadata(null));

        public FrameworkElement TargetElement
        {
            get { return (FrameworkElement) GetValue(TargetElementProperty); }
            set { SetValue(TargetElementProperty, value); }
        }

        public FrameworkElement ContentElement
        {
            get { return (FrameworkElement) GetValue(ContentElementProperty); }
            set { SetValue(ContentElementProperty, value); }
        }

        #endregion

        #region Event Handlers

        void AssociatedObjectOnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            AssociatedObject.Unloaded -= AssociatedObjectOnUnloaded;
            AssociatedObject.DragDelta -= AssociatedObjectOnDragDelta;
            routedEventArgs.Handled = true;
        }

        void AssociatedObjectOnDragDelta(object sender, DragDeltaEventArgs e)
        {
            if (TargetElement == null)
            {
                return;
            }

            //
            // If the height or with is not a number get the actual value.
            //
            if (double.IsNaN(TargetElement.Height))
            {
                TargetElement.Height = TargetElement.ActualHeight;
            }

            if (double.IsNaN(TargetElement.Width))
            {
                TargetElement.Width = TargetElement.ActualWidth;
            }

            // Check for legacy usage - adorner framework MUST always use ContentElement - DO NOT REMOVE!!!
            var minWidth = ContentElement?.MinWidth ?? TargetElement.MinWidth;
            var maxWidth = ContentElement?.MaxWidth ?? TargetElement.MaxWidth;
            var minHeight = ContentElement?.MinHeight ?? TargetElement.MinHeight;
            var maxHeight = ContentElement?.MaxHeight ?? TargetElement.MaxHeight;

            if (TargetElement.Height + e.VerticalChange > 0 && Math.Abs(e.VerticalChange) > ChangeThreshold)
            {
                var newHeight = TargetElement.Height + e.VerticalChange;

                if ((minHeight.Equals(0D) || newHeight > minHeight + MinHeightOffset) &&
                    (maxHeight.Equals(double.PositiveInfinity) || newHeight < maxHeight))
                {
                    TargetElement.Height += e.VerticalChange;
                }
            }


            if (TargetElement.Width + e.HorizontalChange > 0 && Math.Abs(e.HorizontalChange) > ChangeThreshold)
            {
                var newWidth = TargetElement.Width + e.HorizontalChange;
                if ((minWidth.Equals(0D) || newWidth > minWidth + MinWidthOffset) &&
                    (maxWidth.Equals(double.PositiveInfinity) || newWidth < maxWidth))
                {
                    TargetElement.Width += e.HorizontalChange;
                }
            }

        }

        #endregion Event Handlers
    }
}