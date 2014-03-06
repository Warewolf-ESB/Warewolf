using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Diagnostics;

namespace Infragistics.Controls
{
    internal class HorizontalThumbNode : ThumbNode
    {
        public HorizontalThumbNode()
        {

        }

        private ShapeElement _scaleLeftElement;
        public ShapeElement ScaleLeftElement
        {
            get { return this._scaleLeftElement; }
            set { this._scaleLeftElement = value; }
        }

        private ShapeElement _scaleRightElement;
        public ShapeElement ScaleRightElement
        {
            get { return this._scaleRightElement; }
            set { this._scaleRightElement = value; }
        }

        private ShapeElement _thumbBackground;
        public ShapeElement ThumbBackground
        {
            get { return this._thumbBackground; }
            set { this._thumbBackground = value; }
        }

        private bool _isLeft;
        private bool IsLeft
        {
            get { return _isLeft; }
            set { _isLeft = value; }
        }

        public override void ArrangeChildren(Rect oldBounds, Rect newBounds)
        {
            double x = newBounds.X + this.Zoombar.LeftScaleWidth / 2.0;
            double width = newBounds.Width - this.Zoombar.LeftScaleWidth / 2.0 - this.Zoombar.RightScaleWidth / 2.0;

            if (width < 0)
            {
                width = 0;
            }

            this.ThumbBackground.Bounds = new Rect(x, newBounds.Y, width, newBounds.Height);

            double right = newBounds.Right - this.Zoombar.RightScaleWidth;

            this.ScaleLeftElement.Bounds = new Rect(newBounds.X, newBounds.Y, this.Zoombar.LeftScaleWidth, newBounds.Height);
            this.ScaleRightElement.Bounds = new Rect(right, newBounds.Y, this.Zoombar.RightScaleWidth, newBounds.Height);
        }

        public override ShapeElement HitResizeThumb(Point point)
        {
            if (this.ScaleLeftElement.HitTest(point))
            {
                this.IsLeft = true;
                return this.ScaleLeftElement;
            }

            if (this.ScaleRightElement.HitTest(point))
            {
                this.IsLeft = false;
                return this.ScaleRightElement;
            }

            return null;
        }

        public override void DoMove(Point pt)
        {
            double offset = pt.X;

            double left = this.Bounds.Left + offset;
            double right = this.Bounds.Right + offset;

            double maxRight = this.Zoombar.ActualWidth - this.Zoombar.RightOffset;
            double minLeft = this.Zoombar.LeftOffset;

            if (left < minLeft)
            {
                offset += minLeft - left;
            }

            if (right > maxRight)
            {
                offset -= right - maxRight;
            }

            left = this.Bounds.Left + offset;
            right = this.Bounds.Right + offset;

            double min = this.Zoombar.CalculateValue(new Point(left, 0));
            double max = this.Zoombar.CalculateValue(new Point(right, 0));

            this.Zoombar.TempRange.Minimum = min;
            this.Zoombar.TempRange.Maximum = max;

            this.Zoombar.UpdateTempThumb();

            this.Zoombar.RaiseZoomChangingEvent();
        }
        public override void DoResize(Point offset)
        {
            double left = this.Bounds.Left;
            double right = this.Bounds.Right;

            double minWidth = this.Zoombar.LeftScaleWidth + this.Zoombar.RightScaleWidth;

            if (this.IsLeft)
            {
                left += offset.X;

                if (left < this.Zoombar.LeftOffset)
                {
                    left = this.Zoombar.LeftOffset;
                }

                double maxRight = right - minWidth;

                if (left > maxRight)
                {
                    left = maxRight;
                }
            }
            else
            {
                right += offset.X;

                double maxRight = this.Zoombar.ActualWidth - this.Zoombar.RightOffset;

                if (right > maxRight)
                {
                    right = maxRight;
                }

                double minLeft = left + minWidth;

                if (right < minLeft)
                {
                    right = minLeft;
                }
            }

            double min = this.Zoombar.CalculateValue(new Point(left, 0));
            double max = this.Zoombar.CalculateValue(new Point(right, 0));

            this.Zoombar.TempRange.Minimum = min;
            this.Zoombar.TempRange.Maximum = max;

            this.Zoombar.UpdateTempThumb();

            this.Zoombar.RaiseZoomChangingEvent();
        }

        protected override void ChangeVisualState(string newState)
        {
            foreach (FrameworkElement frameworkElement in this.ScaleLeftElement.FrameworkElements)
            {
                Control control = frameworkElement as Control;
                if (control != null)
                {
                    VisualStateManager.GoToState(control, newState, true);
                }
            }

            foreach (FrameworkElement frameworkElement in this.ScaleRightElement.FrameworkElements)
            {
                Control control = frameworkElement as Control;
                if (control != null)
                {
                    VisualStateManager.GoToState(control, newState, true);
                }
            }

            foreach (FrameworkElement frameworkElement in this.ThumbBackground.FrameworkElements)
            {
                Control control = frameworkElement as Control;
                if (control != null)
                {
                    VisualStateManager.GoToState(control, newState, true);
                }
            }
        }
    }
}

#region Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved
/* ---------------------------------------------------------------------*
*                           Infragistics, Inc.                          *
*              Copyright (c) 2001-2012 All Rights reserved               *
*                                                                       *
*                                                                       *
* This file and its contents are protected by United States and         *
* International copyright laws.  Unauthorized reproduction and/or       *
* distribution of all or any portion of the code contained herein       *
* is strictly prohibited and will result in severe civil and criminal   *
* penalties.  Any violations of this copyright will be prosecuted       *
* to the fullest extent possible under law.                             *
*                                                                       *
* THE SOURCE CODE CONTAINED HEREIN AND IN RELATED FILES IS PROVIDED     *
* TO THE REGISTERED DEVELOPER FOR THE PURPOSES OF EDUCATION AND         *
* TROUBLESHOOTING. UNDER NO CIRCUMSTANCES MAY ANY PORTION OF THE SOURCE *
* CODE BE DISTRIBUTED, DISCLOSED OR OTHERWISE MADE AVAILABLE TO ANY     *
* THIRD PARTY WITHOUT THE EXPRESS WRITTEN CONSENT OF INFRAGISTICS, INC. *
*                                                                       *
* UNDER NO CIRCUMSTANCES MAY THE SOURCE CODE BE USED IN WHOLE OR IN     *
* PART, AS THE BASIS FOR CREATING A PRODUCT THAT PROVIDES THE SAME, OR  *
* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY INFRAGISTICS PRODUCT.    *
*                                                                       *
* THE REGISTERED DEVELOPER ACKNOWLEDGES THAT THIS SOURCE CODE           *
* CONTAINS VALUABLE AND PROPRIETARY TRADE SECRETS OF INFRAGISTICS,      *
* INC.  THE REGISTERED DEVELOPER AGREES TO EXPEND EVERY EFFORT TO       *
* INSURE ITS CONFIDENTIALITY.                                           *
*                                                                       *
* THE END USER LICENSE AGREEMENT (EULA) ACCOMPANYING THE PRODUCT        *
* PERMITS THE REGISTERED DEVELOPER TO REDISTRIBUTE THE PRODUCT IN       *
* EXECUTABLE FORM ONLY IN SUPPORT OF APPLICATIONS WRITTEN USING         *
* THE PRODUCT.  IT DOES NOT PROVIDE ANY RIGHTS REGARDING THE            *
* SOURCE CODE CONTAINED HEREIN.                                         *
*                                                                       *
* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *
* --------------------------------------------------------------------- *
*/
#endregion Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved