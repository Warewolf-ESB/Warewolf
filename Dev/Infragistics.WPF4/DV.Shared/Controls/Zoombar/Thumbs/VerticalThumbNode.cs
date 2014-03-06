using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Diagnostics;

namespace Infragistics.Controls
{
    internal class VerticalThumbNode : ThumbNode
    {
        public VerticalThumbNode()
        {

        }

        private ShapeElement _scaleTopElement;
        public ShapeElement ScaleTopElement
        {
            get { return this._scaleTopElement; }
            set { this._scaleTopElement = value; }
        }

        private ShapeElement _scaleBottomElement;
        public ShapeElement ScaleBottomElement
        {
            get { return this._scaleBottomElement; }
            set { this._scaleBottomElement = value; }
        }

        private ShapeElement _thumbBackground;
        public ShapeElement ThumbBackground
        {
            get { return this._thumbBackground; }
            set { this._thumbBackground = value; }
        }

        private bool _isTop;
        private bool IsTop
        {
            get { return _isTop; }
            set { _isTop = value; }
        }

        public override void ArrangeChildren(Rect oldBounds, Rect newBounds)
        {
            double y = newBounds.Y + this.Zoombar.TopScaleHeight / 2.0;
            double height = newBounds.Height - this.Zoombar.TopScaleHeight / 2.0 - this.Zoombar.BottomScaleHeight / 2.0;

            if (height < 0)
            {
                height = 0;
            }

            this.ThumbBackground.Bounds = new Rect(newBounds.X, y, newBounds.Width, height);

            double bottom = newBounds.Bottom - this.Zoombar.BottomScaleHeight;

            this.ScaleTopElement.Bounds = new Rect(newBounds.X, newBounds.Y, newBounds.Width, this.Zoombar.TopScaleHeight);
            this.ScaleBottomElement.Bounds = new Rect(newBounds.X, bottom, newBounds.Width, this.Zoombar.BottomScaleHeight);
        }

        public override ShapeElement HitResizeThumb(Point point)
        {
            if (this._scaleTopElement.HitTest(point))
            {
                this.IsTop = true;
                return this._scaleTopElement;
            }

            if (this._scaleBottomElement.HitTest(point))
            {
                this.IsTop = false;
                return this._scaleBottomElement;
            }

            return null;
        }

        public override void DoMove(Point pt)
        {
            double offset = pt.Y;

            double top = this.Bounds.Top + offset;
            double bottom = this.Bounds.Bottom + offset;

            double maxBottom = this.Zoombar.ActualHeight - this.Zoombar.BottomOffset;
            double minTop = this.Zoombar.TopOffset;

            if (top < minTop)
            {
                offset += minTop - top;
            }

            if (bottom > maxBottom)
            {
                offset -= bottom - maxBottom;
            }

            top = this.Bounds.Top + offset;
            bottom = this.Bounds.Bottom + offset;

            double min = this.Zoombar.CalculateValue(new Point(0, top));
            double max = this.Zoombar.CalculateValue(new Point(0, bottom));

            this.Zoombar.TempRange.Minimum = min;
            this.Zoombar.TempRange.Maximum = max;

            this.Zoombar.UpdateTempThumb();

            this.Zoombar.RaiseZoomChangingEvent();
        }
        public override void DoResize(Point offset)
        {
            double topOffset = this.Zoombar.TopOffset;

            double top = this.Bounds.Top;
            double bottom = this.Bounds.Bottom;

            double minHeight = this.Zoombar.TopScaleHeight + this.Zoombar.BottomScaleHeight;

            if (this.IsTop)
            {
                top += offset.Y;

                if (top < topOffset)
                {
                    top = topOffset;
                }

                double maxBottom = bottom - minHeight;

                if (top > maxBottom)
                {
                    top = maxBottom;
                }
            }
            else
            {
                bottom += offset.Y;

                double maxBottom = this.Zoombar.ActualHeight - this.Zoombar.BottomOffset;

                if (bottom > maxBottom)
                {
                    bottom = maxBottom;
                }

                double minTop = top + minHeight;

                if (bottom < minTop)
                {
                    bottom = minTop;
                }
            }

            double min = this.Zoombar.CalculateValue(new Point(0, top));
            double max = this.Zoombar.CalculateValue(new Point(0, bottom));

            this.Zoombar.TempRange.Minimum = min;
            this.Zoombar.TempRange.Maximum = max;

            this.Zoombar.UpdateTempThumb();

            this.Zoombar.RaiseZoomChangingEvent();
        }

        protected override void ChangeVisualState(string newState)
        {
            foreach (FrameworkElement frameworkElement in this.ScaleTopElement.FrameworkElements)
            {
                Control control = frameworkElement as Control;
                if (control != null)
                {
                    VisualStateManager.GoToState(control, newState, true);
                }
            }

            foreach (FrameworkElement frameworkElement in this.ScaleBottomElement.FrameworkElements)
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