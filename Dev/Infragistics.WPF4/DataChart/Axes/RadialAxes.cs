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
using System.Collections.Generic;

namespace Infragistics.Controls.Charts
{
    /// <summary>
    /// Performs radial axis logic than needs to refer to the two paired 
    /// axes in order to function appropriately.
    /// </summary>
    internal class RadialAxes
    {
        /// <summary>
        /// The radius axis to refer to when performing axis calculations.
        /// </summary>
        public NumericRadiusAxis RadiusAxis { get; set; }

        /// <summary>
        /// The angle axis to refer to when performing axis calculations.
        /// </summary>
        public CategoryAngleAxis AngleAxis { get; set; }

        /// <summary>
        /// Constructs a PolarAxes instance.
        /// </summary>
        /// <param name="radiusAxis">The radius axis to refer to when performing axis calculations.</param>
        /// <param name="angleAxis">The angle axis to refer to when performing axis calculations.</param>
        public RadialAxes(NumericRadiusAxis radiusAxis,
            CategoryAngleAxis angleAxis)
        {
            RadiusAxis = radiusAxis;
            AngleAxis = angleAxis;
        }

        private Point center = new Point(0.5, 0.5);

        /// <summary>
        /// Gets the X coordinate in the viewport coordinate system given an angle
        /// and a radius.
        /// </summary>
        /// <param name="angle">The angle for the point.</param>
        /// <param name="radius">The radius for the point.</param>
        /// <param name="windowRect">The window rectangle bounds.</param>
        /// <param name="viewportRect">The viewport rectangle bounds.</param>
        /// <param name="cosStrategy">A strategy for performing cosine operations.</param>
        /// <returns>The X value of the point in scaled coordinates.</returns>

        public double GetXValue(double angle, double radius,
            Rect windowRect, Rect viewportRect, Func<double, double> cosStrategy)




        {
            double X = center.X + (radius * cosStrategy(angle));
            return ViewportUtils.TransformXToViewport(X, windowRect, viewportRect);
        }

        public void GetScaledPoints(IList<Point> points, IList<double> angleColumn, IList<double> radiusColumn,
            Rect windowRect, Rect viewportRect, Func<int, double, double> cosStrategy,
            Func<int, double, double> sinStrategy)
        {
            int count =
                    Math.Min(angleColumn != null ? angleColumn.Count : 0, radiusColumn != null ? radiusColumn.Count : 0);

            bool fillInPlace = false;
            if (points.Count == count)
            {
                fillInPlace = true;
            }

            double scaledAngle;
            double scaledRadius;
            double cX = center.X;
            double cY = center.Y;
            double X;
            double Y;

            for (int i = 0; i < count; i++)
            {
                scaledAngle = AngleAxis.GetScaledAngle(angleColumn[i]);
                scaledRadius = RadiusAxis.GetScaledValue(radiusColumn[i]);

                X = cX + (scaledRadius * cosStrategy(i, scaledAngle));
                Y = cY + (scaledRadius * sinStrategy(i, scaledAngle));

                X = viewportRect.Left +
                        viewportRect.Width * (X - windowRect.Left) / windowRect.Width;
                Y = viewportRect.Top +
                        viewportRect.Height * (Y - windowRect.Top) / windowRect.Height;

                if (!fillInPlace)
                {
                    points.Add(new Point(X, Y));
                }
                else
                {
                    points[i] = new Point(X, Y);
                }
            }
        }


        /// <summary>
        /// Gets the axis angle and radius values from a point in viewport coordinates.
        /// </summary>
        /// <param name="scaledXValue">The X value of the point in scaled coordinates.</param>
        /// <param name="scaledYValue">The Y value of the point in scaled coordinates.</param>
        /// <param name="windowRect">The window rectangle bounds.</param>
        /// <param name="viewportRect">The viewport rectangle bounds.</param>
        /// <param name="unscaledAngle">Returns the angle axis value of the point.</param>
        /// <param name="unscaledRadius">Returns the radius axis value of the point.</param>
        public void GetUnscaledValues(double scaledXValue, double scaledYValue, Rect windowRect, Rect viewportRect,
            out double unscaledAngle, out double unscaledRadius)
        {
            double X = ViewportUtils.TransformXFromViewport(scaledXValue, windowRect, viewportRect);
            double Y = ViewportUtils.TransformYFromViewport(scaledYValue, windowRect, viewportRect);

            double scaledRadius = Math.Sqrt(Math.Pow(X - center.X, 2) + Math.Pow(Y - center.Y, 2));
            double scaledAngle = Math.Acos((X - center.X) / scaledRadius);
            if ((Y - center.Y) < 0)
            {
                scaledAngle = (2.0 * Math.PI) - scaledAngle;
            }

            unscaledAngle = AngleAxis.GetUnscaledAngle(scaledAngle);
            unscaledRadius = RadiusAxis.GetUnscaledValue(scaledRadius);
        }

        /// <summary>
        /// Gets the Y coordinate in the viewport coordinate system given an angle
        /// and a radius.
        /// </summary>
        /// <param name="angle">The angle for the point.</param>
        /// <param name="radius">The radius for the point.</param>
        /// <param name="windowRect">The window rectangle bounds.</param>
        /// <param name="viewportRect">The viewport rectangle bounds.</param>
        /// <param name="sinStrategy">A strategy to calculate sines.</param>
        /// <returns>The Y value of the point in scaled coordinates.</returns>

        public double GetYValue(double angle, double radius,
           Rect windowRect, Rect viewportRect, Func<double, double> sinStrategy)




        {
            double Y = center.Y + (radius * sinStrategy(angle));
            return ViewportUtils.TransformYToViewport(Y, windowRect, viewportRect);
        }



        internal double GetAngleTo(Point world)
        {
            double radius = Math.Sqrt(Math.Pow(world.X - center.X, 2) +
               Math.Pow(world.Y - center.Y, 2));
            double angle = Math.Acos((world.X - center.X) / radius);
            if ((world.Y - center.Y) < 0)
            {
                angle = (2.0 * Math.PI) - angle;
            }
            return angle;
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