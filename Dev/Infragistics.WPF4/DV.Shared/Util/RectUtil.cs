
using System;
using System.Windows;
using System.Diagnostics.CodeAnalysis;

namespace Infragistics
{
    /// <summary>
    /// Utility class for rectangle-based calculations.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704", Justification = "Util is a word.")]
    public static class RectUtil
    {
        /// <summary>
        /// Gets the center of the current rectangle
        /// </summary>
        /// <param name="rect">The current rectangle.</param>
        /// <returns>Center point</returns>
        public static Point GetCenter(this Rect rect)
        {
            if (rect.IsEmpty)
            {
                return new Point(Double.NaN, Double.NaN);
            }

            return new Point(0.5 * (rect.Left + rect.Right), 0.5 * (rect.Bottom + rect.Top));
        }

        /// <summary>
        /// Calculates the area of the current rectangle.
        /// </summary>
        /// <param name="rect">The current rectangle.</param>
        /// <returns>The area of the current rectangle.</returns>
        public static double GetArea(this Rect rect)
        {
            if (rect.IsEmpty)
            {
                return 0.0;
            }

            return rect.Width * rect.Height;
        }

        /// <summary>
        /// Create a clone of the current rectangle
        /// </summary>
        /// <param name="rect"></param>
        /// <returns>A clone of the current rectangle</returns>
        public static Rect Duplicate(this Rect rect)
        {
            if (rect.IsEmpty)
            {
                return rect;
            }

            return new Rect(rect.Left, rect.Top, rect.Width, rect.Height);
        }

        /// <summary>
        /// Gets the attachment point on the current rectangle for the leader line
        /// to the specified anchor.
        /// </summary>
        /// <remarks>
        /// If the anchor lies within the current rectangle, the anchor is returned.  
        /// </remarks>
        /// <param name="rect">Area to join to the anchor</param>
        /// <param name="anchor">Anchor to join to the rectangle</param>
        /// <returns>Attachment point.</returns>
        public static Point GetLeader(this Rect rect, Point anchor)
        {
            if (rect.Contains(anchor))
            {
                return anchor;
            }

            Point C = new Point(rect.Left + 0.5 * rect.Width, rect.Top + 0.5 * rect.Height);
            Point D = new Point(anchor.X - C.X, anchor.Y - C.Y);
            double p;

            if (D.X != 0)
            {
                p = (rect.Left - C.X) / D.X;
                double y = C.Y + p * D.Y;

                if (y > rect.Top && y < rect.Bottom)
                {
                    return p > 0 ? new Point(rect.Left, y) : new Point(rect.Right, C.Y - p * D.Y);
                }
            }

            p = (rect.Top - C.Y) / D.Y;
            double x = C.X + p * D.X;

            return p > 0 ? new Point(x, rect.Top) : new Point(C.X - p * D.X, rect.Bottom);
        }

        /// <summary>
        /// Calculates the square of the distance from the current rectangle
        /// to the specfied point. 
        /// </summary>
        /// <remarks>
        /// If the point lies within the current rectangle, the separation is considered
        /// to be zero.
        /// </remarks>
        /// <param name="rect">Current rectangle.</param>
        /// <param name="pt">Point to test.</param>
        /// <returns>The square of the separation.</returns>
        public static double GetDistanceSquared(this Rect rect, Point pt)
        {
            if (rect.IsEmpty)
            {
                return Double.NaN;
            }

            return GetDistanceSquared(rect, pt.X, pt.Y);
        }

        /// <summary>
        /// Calculates the square of the distance from the current rectangle
        /// to the specfied rectangle. 
        /// </summary>
        /// <remarks>
        /// If the rectangles intersect, their separation is considered
        /// to be zero.
        /// </remarks>
        /// <param name="rect">Current rectangle.</param>
        /// <param name="rc">Rectangle to test.</param>
        /// <returns>The square of the separation.</returns>
        public static double GetDistanceSquared(this Rect rect, Rect rc)
        {
            if (rect.IsEmpty)
            {
                return Double.PositiveInfinity;
            }

            double d2 = GetDistanceSquared(rect, rc.Left, rc.Top);

            if (d2 > 0)
            {
                d2 = System.Math.Min(d2, GetDistanceSquared(rect, rc.Left, rc.Bottom));
            }

            if (d2 > 0)
            {
                d2 = System.Math.Min(d2, GetDistanceSquared(rect, rc.Right, rc.Bottom));
            }

            if (d2 > 0)
            {
                d2 = System.Math.Min(d2, GetDistanceSquared(rect, rc.Right, rc.Top));
            }

            if (d2 > 0)
            {
                d2 = System.Math.Min(d2, GetDistanceSquared(rc, rect.Left, rect.Top));
            }

            if (d2 > 0)
            {
                d2 = System.Math.Min(d2, GetDistanceSquared(rc, rect.Left, rect.Bottom));
            }

            if (d2 > 0)
            {
                d2 = System.Math.Min(d2, GetDistanceSquared(rc, rect.Right, rect.Bottom));
            }

            if (d2 > 0)
            {
                d2 = System.Math.Min(d2, GetDistanceSquared(rc, rect.Right, rect.Top));
            }

            return d2;
        }

        /// <summary>
        /// Calculates the square of the distance from the current rectangle
        /// to the specfied point. 
        /// </summary>
        /// <param name="rc">Current rectangle.</param>
        /// <param name="X">Point X coordinate.</param>
        /// <param name="Y">Point Y coordinate.</param>
        /// <returns></returns>
        private static double GetDistanceSquared(this Rect rc, double X, double Y)
        {
            double vs = X - rc.Left;
            double vt = Y - rc.Top;
            double s = rc.Width * vs;
            double t = rc.Height * vt;

            if (s > 0)
            {
                double s0 = rc.Width * rc.Width;

                if (s < s0)
                {
                    vs -= (s / s0) * rc.Width;
                }
                else
                {
                    vs -= rc.Width;
                }
            }

            if (t > 0)
            {
                double t0 = rc.Height * rc.Height;

                if (t < t0)
                {
                    vt -= (t / t0) * rc.Height;
                }
                else
                {
                    vt -= rc.Height;
                }
            }

            return vs * vs + vt * vt;
        }

        /// <summary>
        /// Indicates whether the current rectangle wholly contains the specified rectangle.
        /// </summary>
        /// <param name="rect">The current rectangle</param>
        /// <param name="rc">Rectangle to test for strict inclusion</param>
        /// <returns></returns>
        public static bool Contains(this Rect rect, Rect rc)
        {
            if (rect.IsEmpty || rc.IsEmpty)
            {
                return false;
            }

            if (rect.Left > rc.Left) { return false; }
            if (rect.Right < rc.Right) { return false; }
            if (rect.Top > rc.Top) { return false; }
            if (rect.Bottom < rc.Bottom) { return false; }

            return true;
        }

        /// <summary>
        /// Indicates whether the specified rectangle intersects with the current rectangle. 
        /// </summary>
        /// <param name="rect">The current rectangle</param>
        /// <param name="rc">The rectangle to check</param>
        /// <returns>true if the specified rectangle intersects with the current rectangle; otherwise, false.</returns>
        public static bool IntersectsWith(this Rect rect, Rect rc)
        {
            if (rect.IsEmpty || rc.IsEmpty)
            {
                return false;
            }

            if (rect.Right < rc.Left) { return false; }
            if (rect.Left > rc.Right) { return false; }
            if (rect.Top > rc.Bottom) { return false; }
            if (rect.Bottom < rc.Top) { return false; }

            return true;
        }

        /// <summary>
        /// Calculates the area of intersection between the specified rectangle and the current rectangle
        /// </summary>
        /// <param name="rect">The current rectangle</param>
        /// <param name="rc">The rectangle to check</param>
        /// <returns>The area of intersection or 0.0 if the rectangles do not intersect.</returns>
        public static double IntersectionArea(this Rect rect, Rect rc)
        {
            if (rect.IsEmpty || rc.IsEmpty)
            {
                return 0.0;
            }

            double width = System.Math.Min(rect.Right, rc.Right) - System.Math.Max(rect.Left, rc.Left);

            if (width <= 0)
            {
                return 0;
            }

            double height = System.Math.Min(rect.Bottom, rc.Bottom) - System.Math.Max(rect.Top, rc.Top);

            if (height <= 0)
            {
                return 0;
            }

            return width * height;
        }

        /// <summary>
        /// Expands or shrinks the rectangle by using the specified width and height
        /// amounts, in all directions. The
        /// size and position of the current rectangle are not changed.
        /// </summary>
        /// <remarks>
        /// The Width of the resulting rectangle is increased or decreased by twice the specified width offset, because it is applied to both the left and right sides of the rectangle. Likewise, the Height of the resulting rectangle is increased or
        /// decreased by twice the specified height.
        /// <para>
        /// If the specified width or height shrink the rectangle by more than its current
        /// Width or Height—giving the rectangle a negative area—the rectangle becomes the
        /// Empty rectangle.
        /// </para>
        /// </remarks>
        /// <param name="rect"></param>
        /// <param name="width">The amount by which to expand or shrink the left and right sides of the rectangle.</param>
        /// <param name="height">The amount by which to expand or shrink the top and bottom sides of the rectangle.</param>
        /// <returns>Inflated rectangle.</returns>
        public static Rect GetInflated(this Rect rect, Double width, Double height)
        {
            if (rect.IsEmpty)
            {
                return rect;
            }

            return new Rect(rect.X - width, rect.Y - height, System.Math.Max(0, rect.Width + 2 * width), System.Math.Max(0, rect.Height + 2 * height));
        }

        /// <summary>
        /// Inflates the specified rect.
        /// </summary>
        /// <param name="rect">The rect.</param>
        /// <param name="dx">The dx.</param>
        /// <param name="dy">The dy.</param>
        /// <returns></returns>
        public static Rect Inflate(this Rect rect, double dx, double dy)
        {
            rect.X = rect.X - dx;
            rect.Y = rect.Y - dy;

            double width = rect.Width + 2 * dx;
            double height = rect.Height + 2 * dy;

            rect.Width = width > 1 ? width : 1;
            rect.Height = height > 1 ? height : 1;

            return rect;
        }

        /// <summary>
        /// Inflates the specified rect.
        /// </summary>
        /// <param name="rect">The rect.</param>
        /// <param name="thickness">The thickness.</param>
        /// <returns></returns>
        public static Rect Inflate(this Rect rect, double thickness)
        {
            double newX = rect.X - thickness;
            double newY = rect.Y - thickness;

            double width = rect.Width + thickness + thickness;
            double height = rect.Height + thickness + thickness;

            double newWidth = width > 0 ? width : 0;
            double newHeight = height > 0 ? height : 0;

            return new Rect(newX, newY, newWidth, newHeight);
        }


        /// <summary>
        /// Inflates the specified rect.
        /// </summary>
        /// <param name="rect">The rect.</param>
        /// <param name="thickness">The thickness.</param>
        /// <returns></returns>
        public static Rect Inflate(this Rect rect, Thickness thickness)
        {
            rect.X -= thickness.Left;
            rect.Y -= thickness.Top;

            double width = rect.Width + thickness.Left + thickness.Right;
            double height = rect.Height + thickness.Top + thickness.Bottom;

            rect.Width = width > 0 ? width : 0;
            rect.Height = height > 0 ? height : 0;

            return rect;
        }
        /// <summary>
        /// Deflates the specified rect.
        /// </summary>
        /// <param name="rect">The rect.</param>
        /// <param name="thickness">The thickness.</param>
        /// <returns></returns>
        public static Rect Deflate(this Rect rect, Thickness thickness)
        {
            rect.X += thickness.Left;
            rect.Y += thickness.Top;

            double width = rect.Width - thickness.Left - thickness.Right;
            double height = rect.Height - thickness.Top - thickness.Bottom;

            rect.Width = width > 0 ? width : 0;
            rect.Height = height > 0 ? height : 0;

            return rect;
        }


        /// <summary>
        /// Rounds the specified rect.
        /// </summary>
        /// <param name="rect">The rect.</param>
        /// <returns></returns>
        public static Rect Round(this Rect rect)
        {
            rect.X = System.Math.Round(rect.X);
            rect.Y = System.Math.Round(rect.Y);

            rect.Width = System.Math.Round(rect.Width);
            rect.Height = System.Math.Round(rect.Height);

            return rect;
        }

        /// <summary>
        /// Determines whether the specified rect is null.
        /// </summary>
        /// <param name="rect">The rect.</param>
        /// <returns>
        /// 	<c>true</c> if the specified rect is null; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNull(this Rect rect)
        {
            return rect.X == 0 && rect.Y == 0 && rect.Width == 0 && rect.Height == 0;
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