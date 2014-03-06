using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Media;

namespace Infragistics
{
    /// <summary>
    /// Utility class for geometry operations.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704", Justification = "Util is a word.")]
    public static class GeometryUtil
    {
        /// <summary>
        /// Simplifies an angle to a value between 0 and 360.
        /// </summary>
        /// <param name="angle">The angle to simplified.</param>
        /// <returns>The angle, simplified to a value between 0 and 360.</returns>
        public static double SimplifyAngle(double angle)
        {
            if (double.IsNaN(angle) || double.IsInfinity(angle))
            {
                return angle;
            }
            while (angle > 360.0)
            {
                angle -= 360.0;
            }
            while (angle < 0.0)
            {
                angle += 360.0;
            }
            return angle;
        }
        /// <summary>
        /// Gets the angle of a line given its slope.
        /// </summary>
        /// <param name="slope">The slope.</param>
        /// <returns>The angle of a line given its slope.</returns>
        public static double AngleFromSlope(double slope)
        {
            return System.Math.Atan(slope);
        }
        /// <summary>
        /// Calculates the slope of a line given two known points.
        /// </summary>
        /// <param name="point1">One point on the line.</param>
        /// <param name="point2">A second point on the line.</param>
        /// <returns>The slope of the line containing the points point1 and point2.</returns>
        public static double Slope(Point point1, Point point2)
        {
            return (point2.Y - point1.Y) / (point2.X - point1.X);
        }
        /// <summary>
        /// Calculates the eccentricity of an ellipse with the given bounds.
        /// </summary>
        /// <param name="bounds">The bounding rectangle for the ellipse.</param>
        /// <returns>The eccentricity of an ellipse with the given bounds.</returns>
        public static double Eccentricity(Rect bounds)
        {
            return 1.0 - System.Math.Pow(bounds.Height / 2.0, 2.0) / System.Math.Pow(bounds.Width / 2.0, 2.0);
        }
        /// <summary>
        /// Gets a point on an ellipse at the given angle and extent.
        /// </summary>
        /// <param name="theta">The angle at which to find a point on the ellipse, expressed in radians.</param>
        /// <param name="eccentricity">The eccentricity of the ellipse.</param>
        /// <param name="center">The center point of the ellipse.</param>
        /// <param name="halfHeight">Half the height of the ellipse.</param>
        /// <param name="extent">Location of the point relative to the center and the surface of the ellipse, expressed as a value between 0 and 1, with 0 being the center and 1 being on the surface of the ellipse.</param>
        /// <returns>A point on the ellipse at the given angle and extent.</returns>
        public static Point PointOnEllipse(double theta, double eccentricity, Point center, double halfHeight, double extent)
        {
            double cos = System.Math.Cos(theta);
            double sin = System.Math.Sin(theta);
            double r = System.Math.Sqrt(halfHeight * halfHeight / (1.0 - (eccentricity * System.Math.Pow(cos, 2.0))));
            r *= extent;
            return new Point(r * cos + center.X, r * sin + center.Y);
        }

        /// <summary>
        /// Calculates the center of the current chart.
        /// </summary>
        /// <param name="width">Width of the control.</param>
        /// <param name="height">Height of the control.</param>
        /// <param name="exploded">Whether or not the point is exploded.</param>
        /// <param name="angle">Angle of the slice.</param>
        /// <param name="radius">Radius value.</param>
        /// <returns>The center point of the chart.</returns>
        public static Point FindCenter(double width, double height, bool exploded, double angle, double radius)
        {
            Point center;
            if (exploded)
            {
                center = FindRadialPoint(new Point(width / 2, height / 2), angle, radius);
            }
            else
            {
                center = new Point(width / 2, height / 2);
            }
            return center;
        }
        /// <summary>
        /// Finds a point in cartesian coordinates using radial parameters.
        /// </summary>
        /// <param name="center">The center of a circle to find a point on the border of.</param>
        /// <param name="angle">The angle at which to find a point.</param>
        /// <param name="radius">The radius of the circle.</param>
        /// <returns>The point on the border of a circle with the given center and radius, at the specified angle.</returns>
        public static Point FindRadialPoint(Point center, double angle, double radius)
        {
            angle = angle / 180 * System.Math.PI;
            double y = center.Y + radius * System.Math.Sin(angle);
            double x = center.X + radius * System.Math.Cos(angle);
            return new Point(x, y);
        }


        /// <summary>
        /// Creates and returns a copy of this Geometry object.
        /// </summary>
        /// <param name="G">The Geometry to copy.</param>
        /// <returns>A copy of this Geometry object.</returns>
        public static Geometry Duplicate(this Geometry G)
        {
            if (G == null)
            {
                return null;
            }


            return G.Clone();


#region Infragistics Source Cleanup (Region)






















































#endregion // Infragistics Source Cleanup (Region)

        }



        /// <summary>
        /// Gets an empty Geometry.
        /// </summary>
        public static Geometry UnitNone { get { return unitNone ?? (unitNone = new PathGeometry()); } }
        private static Geometry unitNone;
        
        /// <summary>
        /// Gets a Geometry shaped like a bubble.
        /// </summary>
        public static Geometry UnitBubble { get { return unitBubble ?? (unitBubble = new EllipseGeometry() { Center = new Point(0, 0), RadiusX = 1.0, RadiusY = 1.0 }); } }
        private static Geometry unitBubble;
        
        /// <summary>
        /// Gets a Geometry shaped like a triangle.
        /// </summary>
        public static Geometry UnitTriangle { get { return unitTriangle ?? (unitTriangle = PolygonGeometry(unitTriangleXY)); } }
        private static Geometry unitTriangle;
        private static double[] unitTriangleXY = { 0.0000, 1.0000, -0.8660, -0.5000, 0.8660, -0.5000 };
        /// <summary>
        /// Gets a Geometry shaped like a pyramid.
        /// </summary>
        public static Geometry UnitPyramid { get { return unitPyramid ?? (unitPyramid = PolygonGeometry(unitPyramidXY)); } }
        private static Geometry unitPyramid;
        private static double[] unitPyramidXY = { 0.0000, -1.0000, -0.8660, 0.5000, 0.8660, 0.5000 };
        /// <summary>
        /// Gets a Geometry shaped like a square.
        /// </summary>
        public static Geometry UnitSquare { get { return unitSquare ?? (unitSquare = new RectangleGeometry() { Rect = new Rect(-1, -1, 2, 2) }); } }
        private static Geometry unitSquare;
        /// <summary>
        /// Gets a Geometry shaped like a diamond.
        /// </summary>
        public static Geometry UnitDiamond { get { return unitDiamond ?? (unitDiamond = PolygonGeometry(unitDiamondXY)); } }
        private static Geometry unitDiamond;
        private static double[] unitDiamondXY = { 0.0000, -1.0000, -1, 0, 0, 1, 1, 0 };
        /// <summary>
        /// Gets a Geometry shaped like a pentagon.
        /// </summary>
        public static Geometry UnitPentagon { get { return unitPentagon ?? (unitPentagon = PolygonGeometry(unitPentagonXY)); } }
        private static Geometry unitPentagon;
        private static double[] unitPentagonXY = { 0.0000, -1.0000, -0.9511, -0.3090, -0.5875, 0.8090, 0.5875, 0.8090, 0.9511, -0.3090 };
        /// <summary>
        /// Gets a Geometry shaped like a hexagon.
        /// </summary>
        public static Geometry UnitHexagon { get { return unitHexagon ?? (unitHexagon = PolygonGeometry(unitHexagonXY)); } }
        private static Geometry unitHexagon;
        private static double[] unitHexagonXY = { 0.0000, -1.0000, -0.8660, -0.5, -0.8660, 0.5, 0, 1, 0.8660, 0.5, 0.8660, -0.5 };
        /// <summary>
        /// Gets a Geometry shaped like a tetragram.
        /// </summary>
        public static Geometry UnitTetragram { get { return unitTetragram ?? (unitTetragram = PolygonGeometry(unitTetragramXY)); } }
        private static Geometry unitTetragram;
        private static double[] unitTetragramXY = { 0.0000, -1.0000, -0.3536, -0.3536, -1.0000, 0.0000, -0.3536, 0.3536, 0.0000, 1.0000, 0.3536, 0.3536, 1.0000, 0.0000, 0.3536, -0.3536 };
        /// <summary>
        /// Gets a Geometry shaped like a pentagram.
        /// </summary>
        public static Geometry UnitPentagram { get { return unitPentagram ?? (unitPentagram = PolygonGeometry(unitPentagramXY)); } }
        private static Geometry unitPentagram;
        private static double[] unitPentagramXY = { 0.0000, -1.0000, -0.2939, -0.4045, -0.9511, -0.3090, -0.4755, 0.1545, -0.5878, 0.8090, 0.0000, 0.5000, 0.5878, 0.8090, 0.4755, 0.1545, 0.9511, -0.3090, 0.2939, -0.4045 };
        /// <summary>
        /// Gets a Geometry shaped like a hexagram.
        /// </summary>
        public static Geometry UnitHexagram { get { return unitHexagram ?? (unitHexagram = PolygonGeometry(unitHexagramXY)); } }
        private static Geometry unitHexagram;
        private static double[] unitHexagramXY = { 0.0000, -1.0000, -0.2500, -0.4330, -0.8660, -0.5000, -0.5000, 0.0000, -0.8660, 0.5000, -0.2500, 0.4330, 0.0000, 1.0000, 0.2500, 0.4330, 0.8660, 0.5000, 0.5000, 0.0000, 0.8660, -0.5000, 0.2500, -0.4330 };
        /// <summary>
        /// Gets a Geometry shaped like a thermometer.
        /// </summary>
        public static Geometry UnitThermometer
        {
            get
            {
                if (unitThermometer == null)
                {
                    PathFigure pathFigure = new PathFigure() { Segments = new PathSegmentCollection(), IsClosed = true, IsFilled = true, StartPoint = new Point(-0.2, -1.0) };

                    pathFigure.Segments.Add(new ArcSegment() { Point = new Point(0.2, -0.8), IsLargeArc = false, Size = new Size(0.2, 0.2), SweepDirection = SweepDirection.Clockwise });
                    pathFigure.Segments.Add(new LineSegment() { Point = new Point(0.2, 0.2) });
                    pathFigure.Segments.Add(new ArcSegment() { Point = new Point(-0.2, 0.2), IsLargeArc = true, Size = new Size(0.4, 0.4), SweepDirection = SweepDirection.Clockwise });

                    unitThermometer = new PathGeometry() { Figures = new PathFigureCollection() };
                    unitThermometer.Figures.Add(pathFigure);
                }

                return unitThermometer;
            }
        }
        private static PathGeometry unitThermometer;
        /// <summary>
        /// Gets a Geometry shaped like an hourglass.
        /// </summary>
        public static Geometry UnitHourglass
        {
            get
            {
                if (unitHourglass == null)
                {
                    PathFigure pathFigure = new PathFigure() { Segments = new PathSegmentCollection(), IsClosed = true, IsFilled = true, StartPoint = new Point(-0.6, -1.0) };

                    pathFigure.Segments.Add(new BezierSegment() { Point1 = new Point(-0.6, -0.4), Point2 = new Point(-0.1, -0.4), Point3 = new Point(-0.1, 0.0) });
                    pathFigure.Segments.Add(new BezierSegment() { Point1 = new Point(-0.1, 0.4), Point2 = new Point(-0.6, 0.4), Point3 = new Point(-0.6, 1.0) });
                    pathFigure.Segments.Add(new LineSegment() { Point = new Point(0.6, 1.0) });
                    pathFigure.Segments.Add(new BezierSegment() { Point1 = new Point(0.6, 0.4), Point2 = new Point(0.1, 0.4), Point3 = new Point(0.1, 0) });
                    pathFigure.Segments.Add(new BezierSegment() { Point1 = new Point(0.1, -0.4), Point2 = new Point(0.6, -0.4), Point3 = new Point(0.6, -1.0) });

                    unitHourglass = new PathGeometry() { Figures = new PathFigureCollection() };
                    unitHourglass.Figures.Add(pathFigure);
                }

                return unitHourglass;
            }
        }
        private static PathGeometry unitHourglass;
        /// <summary>
        /// Gets a Geometry shaped like a tube.
        /// </summary>
        public static Geometry UnitTube
        {
            get
            {
                if (unitTube == null)
                {
                    PathFigure pathFigure = new PathFigure() { Segments = new PathSegmentCollection(), IsClosed = true, IsFilled = true, StartPoint = new Point(-0.2, -0.8) };
                    pathFigure.Segments.Add(new LineSegment() { Point = new Point(-0.2, 0.8) });
                    pathFigure.Segments.Add(new ArcSegment() { Point = new Point(0.2, 0.8), IsLargeArc = true, Size = new Size(0.2, 0.2), SweepDirection = SweepDirection.Counterclockwise });
                    pathFigure.Segments.Add(new LineSegment() { Point = new Point(0.2, -0.8) });
                    pathFigure.Segments.Add(new ArcSegment() { Point = new Point(-0.2, -0.8), IsLargeArc = true, Size = new Size(0.2, 0.2), SweepDirection = SweepDirection.Counterclockwise });

                    unitTube = new PathGeometry() { Figures = new PathFigureCollection() };
                    unitTube.Figures.Add(pathFigure);
                }

                return unitTube;
            }
        }
        private static PathGeometry unitTube;
        /// <summary>
        /// Gets a Geometry shaped like a raindrop.
        /// </summary>
        public static Geometry UnitRaindrop
        {
            get
            {
                if (unitRaindrop == null)
                {
                    PathFigure pathFigure = new PathFigure() { Segments = new PathSegmentCollection(), IsClosed = true, IsFilled = true, StartPoint = new Point(0.0, -1.0) };

                    pathFigure.Segments.Add(new BezierSegment() { Point1 = new Point(0, -0.4), Point2 = new Point(0.6, -0.2), Point3 = new Point(0.6, 0.4) });
                    pathFigure.Segments.Add(new ArcSegment() { Point = new Point(-0.6, 0.4), IsLargeArc = true, Size = new Size(0.6, 0.6), SweepDirection = SweepDirection.Clockwise });
                    pathFigure.Segments.Add(new BezierSegment() { Point1 = new Point(-0.6, -0.2), Point2 = new Point(0.0, -0.4), Point3 = new Point(0.0, -1.0) });

                    unitRaindrop = new PathGeometry() { Figures = new PathFigureCollection() };
                    unitRaindrop.Figures.Add(pathFigure);
                }

                return unitRaindrop;
            }
        }
        private static PathGeometry unitRaindrop;
        /// <summary>
        /// Gets a Geometry shaped like a smiling face.
        /// </summary>
        public static Geometry UnitSmiley
        {
            get
            {
                if (unitSmiley == null)
                {
                    unitSmiley = new PathGeometry() { };
                    PathFigure pathFigure;

                    // create a pathfigure for the ellipse

                    pathFigure = new PathFigure() { StartPoint = new Point(0, -1), IsClosed = true, IsFilled = true };
                    pathFigure.Segments.Add(new ArcSegment() { Point = new Point(0, -1), IsLargeArc = true, Size = new Size(1, 1) });
                    unitSmiley.Figures.Add(pathFigure);

                    pathFigure = new PathFigure() { StartPoint = new Point(-0.31, -(0.32 - 0.17)), IsClosed = true, IsFilled = true };
                    pathFigure.Segments.Add(new ArcSegment() { Point = new Point(-0.31, -(0.32 - 0.17)), IsLargeArc = true, Size = new Size(0.24, 0.34) });
                    unitSmiley.Figures.Add(pathFigure);

                    pathFigure = new PathFigure() { StartPoint = new Point(0.31, -(0.32 - 0.17)), IsClosed = true, IsFilled = true };
                    pathFigure.Segments.Add(new ArcSegment() { Point = new Point(0.31, -(0.32 - 0.17)), IsLargeArc = true, Size = new Size(0.24, 0.34) });
                    unitSmiley.Figures.Add(pathFigure);

                    
#region Infragistics Source Cleanup (Region)










#endregion // Infragistics Source Cleanup (Region)

                }

                return unitSmiley;
            }
        }
        private static PathGeometry unitSmiley;
        private static Geometry PolygonGeometry(double[] xy)
        {
            PointCollection points = new PointCollection();

            for (int i = 2; i < xy.Length; i += 2)
            {
                points.Add(new Point(xy[i], xy[i + 1]));
            }

            PathFigure pathFigure = new PathFigure() { Segments = new PathSegmentCollection(), IsClosed = true, IsFilled = true, StartPoint = new Point(xy[0], xy[1]) };
            pathFigure.Segments.Add(new PolyLineSegment() { Points = points });

            PathGeometry pathGeometry = new PathGeometry() { Figures = new PathFigureCollection() };
            pathGeometry.Figures.Add(pathFigure);

            return pathGeometry;
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