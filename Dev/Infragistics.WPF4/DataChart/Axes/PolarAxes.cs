using System;



using System.Linq;

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
    /// Performs polar axis logic than needs to refer to the two paired 
    /// axes in order to function appropriately.
    /// </summary>
    internal class PolarAxes
    {
        /// <summary>
        /// The radius axis to refer to when performing axis calculations.
        /// </summary>
        public NumericRadiusAxis RadiusAxis { get; set; }

        /// <summary>
        /// The angle axis to refer to when performing axis calculations.
        /// </summary>
        public NumericAngleAxis AngleAxis { get; set; }

        /// <summary>
        /// Constructs a PolarAxes instance.
        /// </summary>
        /// <param name="radiusAxis">The radius axis to refer to when performing axis calculations.</param>
        /// <param name="angleAxis">The angle axis to refer to when performing axis calculations.</param>
        public PolarAxes(NumericRadiusAxis radiusAxis,
            NumericAngleAxis angleAxis)
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
        /// <param name="cosStrategy">A strategy for calculating cosines.</param>
        /// <returns>The X value of the point in scaled coordinates.</returns>




        public double GetXValue(double angle, double radius,
            Rect windowRect, Rect viewportRect, Func<double, double> cosStrategy)

        {
            double X = center.X + (radius * cosStrategy(angle));
            return ViewportUtils.TransformXToViewport(X, windowRect, viewportRect);
        }

        public Point GetScaledPoint(double angleValue, double radiusValue,
            Rect windowRect, Rect viewportRect, bool angleLogarithmic, bool angleInverted,
            bool radiusLogarithmic, bool radiusInverted, double radiusExtentScale, double innerRadiusExtentScale)
        {
            double scaledAngle = AngleAxis.GetScaledAngle(
                angleValue, angleLogarithmic, angleInverted);
            double scaledRadius = RadiusAxis.GetScaledValue(
                radiusValue, radiusLogarithmic, radiusInverted,
                radiusExtentScale, innerRadiusExtentScale);

            double cX = center.X;
            double cY = center.Y;
            double x = cX + (scaledRadius * Math.Cos(scaledAngle));
            double y = cY + (scaledRadius * Math.Sin(scaledAngle));

            x = viewportRect.Left +
                viewportRect.Width * (x - windowRect.Left) / windowRect.Width;
            y = viewportRect.Top +
                viewportRect.Height * (y - windowRect.Top) / windowRect.Height;

            return new Point(x, y);
        }

        public void GetScaledPoints(IList<Point> points, IList<double> angleColumn, IList<double> radiusColumn,
            Rect windowRect, Rect viewportRect, Func<int, double, double> cosStrategy,
            Func<int, double, double> sinStrategy, bool excludeOutOfRange = true)
        {
            int count =
                    Math.Min(angleColumn != null ? angleColumn.Count : 0, radiusColumn != null ? radiusColumn.Count : 0);

            points.Clear();
            //bool fillInPlace = false;
            //if (points.Count == count)
            //{
            //    fillInPlace = true;
            //}

            double scaledAngle;
            double scaledRadius;
            double cX = center.X;
            double cY = center.Y;
            double X;
            double Y;

            double angleMin = Math.Min(AngleAxis.ActualMinimumValue, AngleAxis.ActualMaximumValue);
            double angleMax = Math.Max(AngleAxis.ActualMinimumValue, AngleAxis.ActualMaximumValue);
            double radiusMin = Math.Min(RadiusAxis.ActualMaximumValue, RadiusAxis.ActualMinimumValue);
            double radiusMax = Math.Max(RadiusAxis.ActualMaximumValue, RadiusAxis.ActualMinimumValue);

            bool angleLogarithmic = AngleAxis.IsReallyLogarithmic;
            bool angleInverted = AngleAxis.IsInverted;
            bool radiusLogarithmic = RadiusAxis.IsReallyLogarithmic;
            bool radiusInverted = RadiusAxis.IsInverted;
            double radiusExtentScale = RadiusAxis.ActualRadiusExtentScale;
            double innerRadiusExtentScale = RadiusAxis.ActualInnerRadiusExtentScale;

            for (int i = 0; i < count; i++)
            {
                if ((angleColumn[i] <= angleMax &&
                    angleColumn[i] >= angleMin &&
                    radiusColumn[i] <= radiusMax &&
                    radiusColumn[i] >= radiusMin)
                    || (double.IsNaN(angleColumn[i]) ||
                    double.IsNaN(radiusColumn[i])))
                {
                    scaledAngle = AngleAxis.GetScaledAngle(angleColumn[i], angleLogarithmic, angleInverted);
                    scaledRadius = RadiusAxis.GetScaledValue(radiusColumn[i], radiusLogarithmic, radiusInverted,
                                                             radiusExtentScale, innerRadiusExtentScale);

                    X = cX + (scaledRadius * cosStrategy(i, scaledAngle));
                    Y = cY + (scaledRadius * sinStrategy(i, scaledAngle));

                    X = viewportRect.Left +
                        viewportRect.Width * (X - windowRect.Left) / windowRect.Width;
                    Y = viewportRect.Top +
                        viewportRect.Height * (Y - windowRect.Top) / windowRect.Height;

                    //if (!fillInPlace)
                    //{
                    points.Add(new Point(X, Y));
                    //}
                    //else
                    //{
                    //    points[i] = new Point(X, Y);
                    //}
                }
                else
                {
                    points.Add(new Point(double.NaN, double.NaN));
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
    }

    /// <summary>
    /// Represents rendering functionality for the polar class of axes.
    /// </summary>
    internal class PolarAxisRenderingManager
    {

        public PathFigure DrawEllipse(
            double radius,
            Point center,
            double minAngle,
            double maxAngle,
            Rect windowRect,
            Rect viewportRect)
        {
            double radiusX =
                    ViewportUtils.TransformXToViewportLength(radius, windowRect, viewportRect);
            double radiusY =
                    ViewportUtils.TransformYToViewportLength(radius, windowRect, viewportRect);

            double centerX =
                    ViewportUtils.TransformXToViewport(center.X, windowRect, viewportRect);
            double centerY =
                    ViewportUtils.TransformYToViewport(center.Y, windowRect, viewportRect);

            if (maxAngle - minAngle < Math.PI &&
                maxAngle - minAngle > 0)
            {
                Point startPoint = new Point(
                    ViewportUtils.TransformXToViewport(center.X +
                        radius * Math.Cos(minAngle), windowRect, viewportRect),
                    ViewportUtils.TransformYToViewport(center.Y +
                        radius * Math.Sin(minAngle), windowRect, viewportRect));

                Point endPoint = new Point(
                    ViewportUtils.TransformXToViewport(center.X +
                        radius * Math.Cos(maxAngle), windowRect, viewportRect),
                    ViewportUtils.TransformYToViewport(center.Y +
                        radius * Math.Sin(maxAngle), windowRect, viewportRect));

                PathFigure pf = new PathFigure();
                pf.StartPoint = startPoint;
                pf.IsClosed = false;
                pf.Segments.Add(new ArcSegment()
                {
                    IsLargeArc = false,
                    Point = endPoint,
                    Size = new Size(radiusX, radiusY),
                    SweepDirection = SweepDirection.Clockwise
                });

                return pf;
            }
            else
            {
                PathFigure pf = new PathFigure();
                pf.StartPoint = new Point(centerX, centerY - radiusY);
                pf.IsClosed = true;
                pf.Segments.Add(new ArcSegment()
                                    {
                                        IsLargeArc = false,
                                        Point = new Point(centerX, centerY + radiusY),
                                        Size = new Size(radiusX, radiusY),
                                        SweepDirection = SweepDirection.Clockwise
                                    });
                pf.Segments.Add(new ArcSegment()
                                    {
                                        IsLargeArc = false,
                                        Point = new Point(centerX, centerY - radiusY),
                                        Size = new Size(radiusX, radiusY),
                                        SweepDirection = SweepDirection.Clockwise
                                    });

                return pf;
            }
        }

        /// <summary>
        /// Draws a concentric strip around the provided center point.
        /// </summary>
        /// <param name="geometry">The geometry to add the strip to.</param>
        /// <param name="radius0">The inner radius of the concentric strip.</param>
        /// <param name="radius1">The outer radius of the concentric strip.</param>
        /// <param name="viewportRect">The viewport rectangle.</param>
        /// <param name="windowRect">The window rectangle bounds.</param>
        /// <param name="center">The center around which to draw the concentric strip.</param>
        /// <param name="minAngle">Minimum angle</param>
        /// <param name="maxAngle">Maximum angle</param>
        public void ConcentricStrip(GeometryCollection geometry, double radius0, double radius1,
            Rect viewportRect, Rect windowRect, Point center, double minAngle, double maxAngle)
        {
            double minRadius = Math.Min(radius0, radius1);
            double maxRadius = Math.Max(radius0, radius1);

            PathGeometry strip = new PathGeometry();

            PathFigure innerFigure = null;
            LineSegment connector1 = null;
            PathFigure outerFigure = null;
            LineSegment connector2 = null;

            if (minRadius > 0)
            {
                innerFigure = DrawEllipse(minRadius, center, minAngle, maxAngle, windowRect, viewportRect);

                if (maxAngle - minAngle < Math.PI &&
                    maxAngle - minAngle > 0)
                {
                    ArcSegment seg = innerFigure.Segments[0] as ArcSegment;
                    if (seg != null)
                    {
                        Point startPoint = new Point(
                            ViewportUtils.TransformXToViewport(center.X +
                        maxRadius * Math.Cos(maxAngle), windowRect, viewportRect),
                            ViewportUtils.TransformYToViewport(center.Y +
                        maxRadius * Math.Sin(maxAngle), windowRect, viewportRect));

                        connector1 = new LineSegment() { Point = startPoint };
                    }
                }
            }
            if (maxRadius > 0.0)
            {
                outerFigure = DrawEllipse(maxRadius, center, minAngle, maxAngle, windowRect, viewportRect);
            }
            if (minRadius > 0)
            {
                if (maxAngle - minAngle < Math.PI &&
                   maxAngle - minAngle > 0)
                {
                    Point swap = outerFigure.StartPoint;
                    ArcSegment seg = outerFigure.Segments[0] as ArcSegment;
                    if (seg != null)
                    {
                        outerFigure.StartPoint = seg.Point;
                        seg.Point = swap;
                        seg.SweepDirection = SweepDirection.Counterclockwise;

                        Point startPoint = new Point(
                            ViewportUtils.TransformXToViewport(center.X +
                        minRadius * Math.Cos(minAngle), windowRect, viewportRect),
                            ViewportUtils.TransformYToViewport(center.Y +
                        minRadius * Math.Sin(minAngle), windowRect, viewportRect));

                        connector2 = new LineSegment() { Point = startPoint };
                    }
                }
            }

            if (connector1 != null &&
                connector2 != null)
            {
                innerFigure.Segments.Add(connector1);
                PathSegment seg = outerFigure.Segments[0];
                outerFigure.Segments.Remove(seg);
                innerFigure.Segments.Add(seg);
                innerFigure.Segments.Add(connector2);
                innerFigure.IsClosed = true;
                strip.Figures.Add(innerFigure);
            }
            else
            {
                if (innerFigure != null)
                {
                    strip.Figures.Add(innerFigure);




                }
                if (outerFigure != null)
                {
                    strip.Figures.Add(outerFigure);
                }
            }

            geometry.Add(strip);
        }

        private void ReverseArcFigure(PathFigure figure)
        {
            if (figure.Segments.Count > 1)
            {
                ArcSegment seg1 = figure.Segments[0] as ArcSegment;
                ArcSegment seg2 = figure.Segments[1] as ArcSegment;

                figure.Segments[0] = seg2;
                figure.Segments[1] = seg1;

                Point startPoint = seg2.Point;
                Point seg2Point = figure.StartPoint;
                Point seg1Point = seg1.Point;
                
                figure.StartPoint = startPoint;
                seg2.Point = seg1Point;
                seg1.Point = seg2Point;

                seg1.SweepDirection = SweepDirection.Counterclockwise;
                seg2.SweepDirection = SweepDirection.Counterclockwise;
            }
            else
            {
                Point swap = figure.StartPoint;
                ArcSegment seg = figure.Segments[0] as ArcSegment;
                if (seg != null)
                {
                    figure.StartPoint = seg.Point;
                    seg.Point = swap;
                    seg.SweepDirection = SweepDirection.Counterclockwise;
                }
            }
        }

        /// <summary>
        /// Draws a concentric line (a circle) around the provided center.
        /// </summary>
        /// <param name="geometry">The geometry to add a line to.</param>
        /// <param name="radius">The radius of the concentric circle.</param>
        /// <param name="viewportRect">The viewport rectangle.</param>
        /// <param name="windowRect">The window rectangle bounds.</param>
        /// <param name="center">The center around which to draw the concentric line.</param>
        /// <param name="startAngle">Start angle</param>
        /// <param name="endAngle">End angle</param>
        public void ConcentricLine(GeometryCollection geometry, double radius,
            Rect viewportRect, Rect windowRect, Point center, double startAngle, double endAngle)
        {
            if (radius > 0)
            {
                PathGeometry line = new PathGeometry();
                line.Figures.Add(DrawEllipse(radius, center, startAngle, endAngle, windowRect, viewportRect));

                geometry.Add(line);
            }
        }

        /// <summary>
        /// Draws a radial strip (a pie slice) around the provided center point.
        /// </summary>
        /// <param name="geometry">The geometry to add the strip to.</param>
        /// <param name="startAngle">The start angle of the strip.</param>
        /// <param name="endAngle">The end angle of the strip.</param>
        /// <param name="viewportRect">The viewport rectangle.</param>
        /// <param name="windowRect">The window rectangle bounds.</param>
        /// <param name="minLength">The inner extent of the strip.</param>
        /// <param name="maxLength">The outer extent of the strip.</param>
        /// <param name="center">The center around which to draw the radial strip.</param>
        public void RadialStrip(GeometryCollection geometry, double startAngle,
            double endAngle, Rect viewportRect, Rect windowRect,
            double minLength, double maxLength, Point center)
        {
            double angleMin = Math.Min(startAngle, endAngle);
            double angleMax = Math.Max(startAngle, endAngle);

            bool isLargeArc = false;
            if (angleMax - angleMin > Math.PI)
            {
                isLargeArc = true;
            }

            double cosAngleMin = Math.Cos(angleMin);
            double sinAngleMin = Math.Sin(angleMin);

            double startXmin = center.X + cosAngleMin * minLength;
            double startYmin = center.Y + sinAngleMin * minLength;
            double endXmin = center.X + cosAngleMin * maxLength;
            double endYmin = center.Y + sinAngleMin * maxLength;

            double cosAngleMax = Math.Cos(angleMax);
            double sinAngleMax = Math.Sin(angleMax);

            double startXmax = center.X + cosAngleMax * minLength;
            double startYmax = center.Y + sinAngleMax * minLength;
            double endXmax = center.X + cosAngleMax * maxLength;
            double endYmax = center.Y + sinAngleMax * maxLength;

            startXmin = ViewportUtils.TransformXToViewport(startXmin, windowRect, viewportRect);
            startYmin = ViewportUtils.TransformYToViewport(startYmin, windowRect, viewportRect);
            endXmin = ViewportUtils.TransformXToViewport(endXmin, windowRect, viewportRect);
            endYmin = ViewportUtils.TransformYToViewport(endYmin, windowRect, viewportRect);
            startXmax = ViewportUtils.TransformXToViewport(startXmax, windowRect, viewportRect);
            startYmax = ViewportUtils.TransformYToViewport(startYmax, windowRect, viewportRect);
            endXmax = ViewportUtils.TransformXToViewport(endXmax, windowRect, viewportRect);
            endYmax = ViewportUtils.TransformYToViewport(endYmax, windowRect, viewportRect);

            PathFigure pf = new PathFigure();

            pf.StartPoint = new Point(startXmin, startYmin);
            pf.IsClosed = true;
            pf.Segments.Add(new LineSegment() { Point = new Point(endXmin, endYmin) });

            //draw an arc at the end of the slice with the 
            //same radius as the chart (the strip outer radius).
            pf.Segments.Add(new ArcSegment()
            {
                Point = new Point(endXmax, endYmax),
                Size = new Size(
                    ViewportUtils.TransformXToViewportLength(maxLength, windowRect, viewportRect),
                    ViewportUtils.TransformYToViewportLength(maxLength, windowRect, viewportRect)),
                SweepDirection = SweepDirection.Clockwise,
                IsLargeArc = isLargeArc
            });
            pf.Segments.Add(new LineSegment() { Point = new Point(startXmax, startYmax) });

            pf.Segments.Add(new ArcSegment()
            {
                Point = new Point(startXmin, startYmin),
                Size = new Size(
                    ViewportUtils.TransformXToViewportLength(minLength, windowRect, viewportRect),
                    ViewportUtils.TransformYToViewportLength(minLength, windowRect, viewportRect)),
                SweepDirection = SweepDirection.Counterclockwise,
                IsLargeArc = isLargeArc
            });

            PathGeometry pg = new PathGeometry();
            pg.Figures.Add(pf);
            geometry.Add(pg);
        }

        /// <summary>
        /// Draws a radial line (spoke) around the center point.
        /// </summary>
        /// <param name="geometry">The geometry to add the line to.</param>
        /// <param name="angle">The angle of the line around the center point.</param>
        /// <param name="viewportRect">The viewport rectangle.</param>
        /// <param name="windowRect">The window rectangle bounds.</param>
        /// <param name="minLength">The minimum length of the line.</param>
        /// <param name="maxLength">The maximum length of the line.</param>
        /// <param name="center">The center point around which to draw the line.</param>
        public void RadialLine(GeometryCollection geometry, double angle,
            Rect viewportRect, Rect windowRect,
            double minLength, double maxLength, Point center)
        {
            LineGeometry radialLine = new LineGeometry();

            double cosX = Math.Cos(angle);
            double sinX = Math.Sin(angle);

            //determine the x and y of the start and end extent of the line.
            double startX = center.X + cosX * minLength;
            double startY = center.Y + sinX * minLength;
            double endX = center.X + cosX * maxLength;
            double endY = center.Y + sinX * maxLength;

            startX = ViewportUtils.TransformXToViewport(startX, windowRect, viewportRect);
            startY = ViewportUtils.TransformYToViewport(startY, windowRect, viewportRect);
            endX = ViewportUtils.TransformXToViewport(endX, windowRect, viewportRect);
            endY = ViewportUtils.TransformYToViewport(endY, windowRect, viewportRect);

            radialLine.StartPoint = new Point(startX, startY);
            radialLine.EndPoint = new Point(endX, endY);
            geometry.Add(radialLine);
        }

        /// <summary>
        /// Is the provided point within the x range of the bounding box.
        /// </summary>
        /// <param name="center">The point to check.</param>
        /// <param name="bounds">The bounding box.</param>
        /// <returns>True if it is in the X range.</returns>
        public bool InXBand(Point center, Rect bounds)
        {
            return center.X >= bounds.Left &&
                center.X <= bounds.Right;
        }

        /// <summary>
        /// Is the provided point in the y range of the bounding box.
        /// </summary>
        /// <param name="center">The point to check.</param>
        /// <param name="bounds">The bounding box.</param>
        /// <returns>True if it is in the Y range.</returns>
        public bool InYBand(Point center, Rect bounds)
        {
            return center.Y >= bounds.Top &&
                center.Y <= bounds.Bottom;
        }

        /// <summary>
        /// Determines the distance to the corner on the cartesian axis aligned bounding rectangle that 
        /// is closest to the provided point.
        /// </summary>
        /// <param name="center">The point to determine closeness to.</param>
        /// <param name="bounds">The cartesian axis aligned rectangle.</param>
        /// <returns>The distance to the closest corner.</returns>
        public double ClosestCorner(Point center, Rect bounds)
        {
            double dist1 = Math.Sqrt(Math.Pow(center.X - bounds.Left, 2) +
                Math.Pow(center.Y - bounds.Top, 2));
            double dist2 = Math.Sqrt(Math.Pow(center.X - bounds.Right, 2) +
                Math.Pow(center.Y - bounds.Top, 2));
            double dist3 = Math.Sqrt(Math.Pow(center.X - bounds.Right, 2) +
                Math.Pow(center.Y - bounds.Bottom, 2));
            double dist4 = Math.Sqrt(Math.Pow(center.X - bounds.Left, 2) +
                Math.Pow(center.Y - bounds.Bottom, 2));

            return Math.Min(dist1, Math.Min(dist2, Math.Min(dist3, dist4)));
        }

        /// <summary>
        /// Determines the distance to the corner on the cartesian axis aligned bounding rectangle that 
        /// is furthest from the provided point.
        /// </summary>
        /// <param name="center">The point to determine distance to.</param>
        /// <param name="bounds">The cartesian axis aligned rectangle.</param>
        /// <returns>The distance to the furthest corner.</returns>
        public double FurthestCorner(Point center, Rect bounds)
        {
            double dist1 = Math.Sqrt(Math.Pow(center.X - bounds.Left, 2) +
                Math.Pow(center.Y - bounds.Top, 2));
            double dist2 = Math.Sqrt(Math.Pow(center.X - bounds.Right, 2) +
                Math.Pow(center.Y - bounds.Top, 2));
            double dist3 = Math.Sqrt(Math.Pow(center.X - bounds.Right, 2) +
                Math.Pow(center.Y - bounds.Bottom, 2));
            double dist4 = Math.Sqrt(Math.Pow(center.X - bounds.Left, 2) +
                Math.Pow(center.Y - bounds.Bottom, 2));

            return Math.Max(dist1, Math.Max(dist2, Math.Max(dist3, dist4)));
        }

        /// <summary>
        /// Determines the smallest radius that is visible in the current view into 
        /// the chart.
        /// </summary>
        /// <param name="windowRect">The window rectangle.</param>
        /// <returns>The radius value.</returns>
        /// <remarks>
        /// This makes use of the fact that the view into the chart is aligned on the 
        /// cartesian axes. 
        /// 
        /// So, if the center point of the chart is inside the viewable 
        /// space, the minimum radius is 0. 
        /// 
        /// If the view into the chart is scrolled above or below the center point, 
        /// such that the center point is in the X range of the view, then the closest
        /// radius is just the distance to the top or bottom edge of the window rectangle.
        /// 
        /// Similarly, if the view into the chart is left of right of the center point,
        /// such that the center point is in the Y range of the view, then the closest
        /// radius is just the distance to the left of right edge of the window rectangle.
        /// 
        /// In the case where the view is deeper into the quadrants of the space, and the
        /// center point is neither in the X or Y range of the view, then we simply need to 
        /// determine what the distance is to the closest corner of the rectangle.</remarks>
        public double GetClosestRadiusValue(Rect windowRect)
        {
            Point center = new Point(0.5, 0.5);

            if (InXBand(center, windowRect) &&
                InYBand(center, windowRect))
            {
                return 0;
            }

            if (InXBand(center, windowRect))
            {
                if (center.Y < windowRect.Top)
                {
                    return windowRect.Top - center.Y;
                }
                else
                {
                    return center.Y - windowRect.Bottom;
                }
            }

            if (InYBand(center, windowRect))
            {
                if (center.X < windowRect.Left)
                {
                    return windowRect.Left - center.X;
                }
                else
                {
                    return center.X - windowRect.Right;
                }
            }

            return ClosestCorner(center, windowRect);
        }

        /// <summary>
        /// Determines the furthest radius value that is viewable in the current view into
        /// the chart.
        /// </summary>
        /// <param name="windowRect">The window rectangle.</param>
        /// <returns>The radius value.</returns>
        /// <remarks>In all cases the furthest distance will just be the distance to the furthest 
        /// corner of the viewing rectangle in scaled viewport space.</remarks>
        public double GetFurthestRadiusValue(Rect windowRect)
        {
            Point center = new Point(0.5, 0.5);

            return FurthestCorner(center, windowRect);
        }

        /// <summary>
        /// Gets the angle value to a specified point from the center point.
        /// </summary>
        /// <param name="center">The center point of the chart.</param>
        /// <param name="toPoint">The point to get the axis angle value to.</param>
        /// <returns></returns>
        protected double GetAngleTo(Point center, Point toPoint)
        {
            double radius = Math.Sqrt(Math.Pow(toPoint.X - center.X, 2) +
                Math.Pow(toPoint.Y - center.Y, 2));
            double angle = Math.Acos((toPoint.X - center.X) / radius);
            if ((toPoint.Y - center.Y) < 0)
            {
                angle = (2.0 * Math.PI) - angle;
            }
            return angle;
        }

        private double LineCheck(Point maxValueRadiusPoint, Point rectPoint)
        {
            return ((maxValueRadiusPoint.Y - .5) * rectPoint.X) + ((0.5 - maxValueRadiusPoint.X) * rectPoint.Y)
                + ((maxValueRadiusPoint.X * .5) - (.5 * maxValueRadiusPoint.Y));
        }

        /// <summary>
        /// Gets the min and max angle axis value that are visible in the current 
        /// view into the chart.
        /// </summary>
        /// <param name="windowRect">The window rectangle.</param>
        /// <param name="minAngle">Returns the minimum angle that is visible.</param>
        /// <param name="maxAngle">Returns the maximum angle that is visible.</param>
        public void GetMinMaxAngle(Rect windowRect,
            out double minAngle, out double maxAngle)
        {
            Point center = new Point(0.5, 0.5);

            //center is contained in window
            if (InXBand(center, windowRect)
                && InYBand(center, windowRect))
            {
                minAngle = 0;
                maxAngle = Math.PI * 2.0;
                return;
            }

            //determine the angles from the center to each corner of the window.
            double angle1 = GetAngleTo(center, new Point(windowRect.Left, windowRect.Top));
            double angle2 = GetAngleTo(center, new Point(windowRect.Right, windowRect.Top));
            double angle3 = GetAngleTo(center, new Point(windowRect.Right, windowRect.Bottom));
            double angle4 = GetAngleTo(center, new Point(windowRect.Left, windowRect.Bottom));


            //if view is right of the center, and the center is in the y range of the view
            //we have to wrap the angle around the maximum.
            if (InYBand(center, windowRect) && windowRect.Left > center.X)
            {
                minAngle = angle1;
                maxAngle = 2 * Math.PI + angle4;

                return;
            }

            minAngle = Math.Min(angle1, Math.Min(angle2, Math.Min(angle3, angle4)));
            maxAngle = Math.Max(angle1, Math.Max(angle2, Math.Max(angle3, angle4)));
        }

        public void DetermineView(Rect windowRect,
            AxisRenderingParametersBase renderingParams,
            double actualMinimumValue,
            double actualMaximumValue,
            bool isInverted,
            Func<double, double> getUnscaledAngle,
            double resolution)
        {
            double minAngle;
            double maxAngle;
            GetMinMaxAngle(windowRect, out minAngle, out maxAngle);
            double trueMinAngle = Math.Min(minAngle, maxAngle);
            double trueMaxAngle = Math.Max(minAngle, maxAngle);
            if (renderingParams is PolarAxisRenderingParameters)
            {
                (renderingParams as PolarAxisRenderingParameters).MinAngle = trueMinAngle;
                (renderingParams as PolarAxisRenderingParameters).MaxAngle = trueMaxAngle;
            }
            else if (renderingParams is RadialAxisRenderingParameters)
            {
                (renderingParams as RadialAxisRenderingParameters).MinAngle = trueMinAngle;
                (renderingParams as RadialAxisRenderingParameters).MaxAngle = trueMaxAngle;
            }

            if (minAngle == 0 && maxAngle == Math.PI * 2.0)
            {
                double visibleMinimum = actualMinimumValue;
                double visibleMaximum = actualMaximumValue;
                double trueVisibleMinimum = Math.Min(visibleMinimum, visibleMaximum);
                double trueVisibleMaximum = Math.Max(visibleMinimum, visibleMaximum);
                renderingParams.RangeInfos.Add(
                    new RangeInfo()
                    {
                        VisibleMinimum = trueVisibleMinimum,
                        VisibleMaximum = trueVisibleMaximum,
                        Resolution = resolution
                    });

                
#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

                return;
            }
            else
            {
                if (maxAngle > Math.PI * 2.0)
                {
                    maxAngle = maxAngle - Math.PI * 2.0;
                }
                double visibleMinimum = getUnscaledAngle(minAngle);
                double visibleMaximum = getUnscaledAngle(maxAngle);

                if (visibleMinimum < actualMinimumValue ||
                    visibleMinimum > actualMaximumValue)
                {
                    visibleMinimum = getUnscaledAngle(minAngle + Math.PI * 2.0);
                }
                if (visibleMaximum < actualMinimumValue ||
                    visibleMaximum > actualMaximumValue)
                {
                    visibleMaximum = getUnscaledAngle(maxAngle + Math.PI * 2.0);
                }

                double trueVisibleMinimum = Math.Min(visibleMinimum, visibleMaximum);
                double trueVisibleMaximum = Math.Max(visibleMinimum, visibleMaximum);

                if ((!isInverted && visibleMinimum > visibleMaximum) ||
                    (isInverted && visibleMinimum < visibleMaximum))
                {
                    double range1 = (actualMaximumValue - trueVisibleMaximum);
                    double range2 = (trueVisibleMinimum - actualMinimumValue);

                    renderingParams.RangeInfos.Add(
                        new RangeInfo()
                        {
                            VisibleMinimum = trueVisibleMaximum,
                            VisibleMaximum = actualMaximumValue,
                            Resolution = (range1 / (range1 + range2)) * resolution
                        });

                    renderingParams.RangeInfos.Add(
                        new RangeInfo()
                        {
                            VisibleMinimum = actualMinimumValue,
                            VisibleMaximum = trueVisibleMinimum,
                            Resolution = (range2 / (range1 + range2)) * resolution
                        });

                    
#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

                }
                else
                {
                    renderingParams.RangeInfos.Add(
                    new RangeInfo()
                    {
                        VisibleMinimum = trueVisibleMinimum,
                        VisibleMaximum = trueVisibleMaximum,
                        Resolution = resolution
                    });

                    
#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

                }
            }
        }

    }


    internal interface IPolarRadialRenderingParameters
    {
        /// <summary>
        /// The minimum extent of any spokes to be drawn.
        /// </summary>
        double MinLength { get; set; }

        /// <summary>
        /// The maximum extent of any spokes to be drawn.
        /// </summary>
        double MaxLength { get; set; }

        /// <summary>
        /// The center of the polar chart.
        /// </summary>
        Point Center { get; set; }

        double MinAngle { get; set; }
        double MaxAngle { get; set; }

        /// <summary>
        /// The angle at which the radius axis will cross
        /// </summary>
        double CrossingAngleRadians { get; set; }

        double EffectiveMaximum { get; set; }
    }

    /// <summary>
    /// Polar axis specific rendering parameters.
    /// </summary>
    internal class PolarAxisRenderingParameters
        : NumericAxisRenderingParameters, IPolarRadialRenderingParameters
    {
        /// <summary>
        /// The minimum extent of any spokes to be drawn.
        /// </summary>
        public double MinLength { get; set; }

        /// <summary>
        /// The maximum extent of any spokes to be drawn.
        /// </summary>
        public double MaxLength { get; set; }

        /// <summary>
        /// The center of the polar chart.
        /// </summary>
        public Point Center { get; set; }

        public double MinAngle { get; set; }
        public double MaxAngle { get; set; }

        /// <summary>
        /// The angle at which the radius axis will cross
        /// </summary>
        public double CrossingAngleRadians { get; set; }

        public double EffectiveMaximum { get; set; }
    }

    internal class RadialAxisRenderingParameters
        : CategoryAxisRenderingParameters, IPolarRadialRenderingParameters
    {
        /// <summary>
        /// The minimum extent of any spokes to be drawn.
        /// </summary>
        public double MinLength { get; set; }

        /// <summary>
        /// The maximum extent of any spokes to be drawn.
        /// </summary>
        public double MaxLength { get; set; }

        /// <summary>
        /// The center of the polar chart.
        /// </summary>
        public Point Center { get; set; }

        /// <summary>
        /// The angle at which the radius axis will cross
        /// </summary>
        public double CrossingAngleRadians { get; set; }

        public double MinAngle { get; set; }

        public double MaxAngle { get; set; }

        public double EffectiveMaximum { get; set; }
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