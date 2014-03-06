
#region Using

using System;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows;
using System.Windows.Media.Animation;

#endregion Using

namespace Infragistics.Windows.Chart
{
    /// <summary>
    /// This class keeps methods which create 3D primitive points, normals, 
    /// indices and texture points. 3D primitives are basic elements used to 
    /// create complex object models. For example, pie slice has many 
    /// 3D primitives: Top Slices, Edges, sides, main curves, etc.
    /// </summary>
    internal class Primitives3D : ChartSeries
    {
        #region Methods

        /// <summary>
        /// Returns point depth for 3D based chart types.
        /// </summary>
        /// <param name="depth">The depth of a data point</param>
        /// <param name="axisX">X axis values</param>
        /// <param name="axisZ">Y axis values</param>
        /// <param name="series">The series</param>
        protected void GetPointWidth(out double depth, AxisValue axisX, AxisValue axisZ, Series series)
        {
            // Get point width chart parameter
            double pointWidth = series.GetParameterValueDouble(ChartParameterType.PointWidth);
            if (series.IsParameterSet(ChartParameterType.PointWidth))
            {
                pointWidth *= 0.8;
            }

            depth = AxisZ.GetSize(pointWidth);
        }

        /// <summary>
        /// Returns width and depth for Column 3D based chart types.
        /// </summary>
        /// <param name="width">The width of a data point</param>
        /// <param name="depth">The depth of a data point</param>
        /// <param name="axisX">X axis values</param>
        /// <param name="axisZ">Y axis values</param>
        /// <param name="series">The series</param>
        protected void GetPointWidth(out double width, out double depth, AxisValue axisX, AxisValue axisZ, Series series)
        {
            // Get point width chart parameter
            double pointWidth = series.GetParameterValueDouble(ChartParameterType.PointWidth);
            if (series.IsParameterSet(ChartParameterType.PointWidth))
            {
                pointWidth *= 0.95;
            }

            width = AxisX.GetSize(pointWidth);
            depth = AxisZ.GetSize(pointWidth);
        }

        /// <summary>
        /// Finds a point of the circle which belongs to XY plane and the center is at (0, 0, z). 
        /// </summary>
        /// <param name="angle">An angle which is used to find a point from the circle.</param>
        /// <param name="radius">The radius of the circle.</param>
        /// <param name="z">Z position</param>
        /// <returns>The point from the circle.</returns>
        internal protected Point3D FindRadialPointZ(double angle, double radius, double z)
        {
            double x = radius * Math.Sin(angle);
            double y = radius * Math.Cos(angle);
            return new Point3D(x, y, z);
        }

        /// <summary>
        /// Finds a point of the circle which belongs to XZ plane and the center is at (0, y, 0). 
        /// </summary>
        /// <param name="angle">An angle which is used to find a point from the circle.</param>
        /// <param name="radius">The radius of the circle.</param>
        /// <param name="y">Y position</param>
        /// <returns>The point from the circle.</returns>
        internal protected Point3D FindRadialPointY(double angle, double radius, double y)
        {
            double x = radius * Math.Sin(angle);
            double z = radius * Math.Cos(angle);
            return new Point3D(x, y, z);
        }

        /// <summary>
        /// Finds a point of the circle which belongs to YZ plane and the center is at (x, 0, 0). 
        /// </summary>
        /// <param name="angle">An angle which is used to find a point from the circle.</param>
        /// <param name="radius">The radius of the circle.</param>
        /// <param name="x">X position</param>
        /// <returns>The point from the circle.</returns>
        internal protected Point3D FindRadialPointX(double angle, double radius, double x)
        {
            double z = radius * Math.Sin(angle);
            double y = radius * Math.Cos(angle);
            return new Point3D(x, y, z);
        }

        /// <summary>
        /// Finds a vector from (0, 0, 0) position in 3D space to the circle which belongs to XZ plane and the center is at (0, y, 0). 
        /// </summary>
        /// <param name="angle">An angle which is used to find a point from the circle.</param>
        /// <param name="radius">The radius of the circle.</param>
        /// <param name="y">Y position</param>
        /// <returns>The new vector</returns>
        internal protected Vector3D FindRadialVectorY(double angle, double radius, double y)
        {
            double x = radius * Math.Sin(angle);
            double z = radius * Math.Cos(angle);
            return new Vector3D(x, y, z);
        }

        /// <summary>
        /// Finds a vector from (0, 0, 0) position in 3D space to the circle which belongs to YZ plane and the center is at (x, 0, 0). 
        /// </summary>
        /// <param name="angle">An angle which is used to find a point from the circle.</param>
        /// <param name="radius">The radius of the circle.</param>
        /// <param name="x">X position</param>
        /// <returns>The new vector</returns>
        internal protected Vector3D FindRadialVectorX(double angle, double radius, double x)
        {
            double z = radius * Math.Sin(angle);
            double y = radius * Math.Cos(angle);
            return new Vector3D(x, y, z);
        }

        internal protected Vector3D NormalVector(Point3D vector1Start, Point3D vector1End, Point3D vector2Start, Point3D vector2End)
        {
            Vector3D vector1 = Point3D.Subtract(vector1End, vector1Start);
            Vector3D vector2 = Point3D.Subtract(vector2End, vector2Start);

            return Vector3D.CrossProduct(vector1, vector2);
        }

        internal protected Point3DCollection GenerateArcPoints(Point3D p1, Point3D p2, Point3D center, int numberOfSegments)
        {
            Point3DCollection points = new Point3DCollection();
            double radius1 = Math.Sqrt(Pow2(p1.X - center.X) + Pow2(p1.Y - center.Y) + Pow2(p1.Z - center.Z));
            double radius2 = Math.Sqrt(Pow2(p2.X - center.X) + Pow2(p2.Y - center.Y) + Pow2(p2.Z - center.Z));

            if (Math.Round(radius1, 5) != Math.Round(radius2, 5))
            {
                // Error Generating Arc points (Invalid Radius): The center point doesn�t have same distance from the start and the end point.
                //throw new ArgumentException(ErrorString.Exc14);
            }

            p1.X -= center.X;
            p1.Z -= center.Z;
            p2.X -= center.X;
            p2.Z -= center.Z;

            double angle1 = FindAngle(p1.X, p1.Z);
            double angle2 = FindAngle(p2.X, p2.Z);

            double stepAngle = (angle2 - angle1) / (double)numberOfSegments;

            double currentAngle = angle1;

            for (int index = 0; index <= numberOfSegments; index++)
            {
                double x = radius1 * Math.Cos(currentAngle) + center.X;
                double z = radius1 * Math.Sin(currentAngle) + center.Z;

                points.Add(new Point3D(x, 0, z));

                currentAngle += stepAngle;
            }

            return points;
        }

        private double FindAngle(double x, double y)
        {
            double radius = Math.Sqrt(x * x + y * y);
            double angle = Math.Asin(y / radius);

            if (x < 0)
            {
                angle = (Math.PI - angle);
            }

            return angle;
        }

        private bool LineBetweenPoints(double x1, double y1, double x2, double y2, out double a, out double b)
        {
            if (y2 == y1)
            {
                a = 0;
                b = 0;
                return false;
            }

            b = (Pow2(x2) + Pow2(y2) - Pow2(x1) - Pow2(y1)) / (2 * (y2 - y1));
            a = (x1 - x2) / (y2 - y1);

            return true;
        }

        protected void FindCenterPointFromRadius(
            double x1,
            double y1,
            double x2,
            double y2,
            double radius,
            double OrientationPointX,
            double OrientationPointY,
            out double centerX,
            out double centerY
            )
        {
            // Line Equation y = ax + b
            double a, b;

            // If line is not vertical
            if (LineBetweenPoints(x1, y1, x2, y2, out a, out b))
            {
                // Quadratic Equation Ax^2 + Bx + C = 0;

                double qA = 1 + Pow2(a);
                double qB = -2 * x1 - 2 * y1 * a + 2 * a * b;
                double qC = Pow2(x1) + Pow2(y1) + Pow2(b) - 2 * y1 * b - Pow2(radius);

                double rx1 = (-qB + Math.Sqrt(Pow2(qB) - 4 * qA * qC)) / (2 * qA);
                double rx2 = (-qB - Math.Sqrt(Pow2(qB) - 4 * qA * qC)) / (2 * qA);

                if (double.IsNaN(rx1) || double.IsNaN(rx2))
                {
                    // The radius is smaler then the distance between two points. (Primitives3D)
                    throw new ArgumentException(ErrorString.Exc15);
                }

                double ry1 = a * rx1 + b;
                double ry2 = a * rx2 + b;

                // Distance to the orientation point
                double rd1 = Pow2(rx1 - OrientationPointX) + Pow2(ry1 - OrientationPointY);
                double rd2 = Pow2(rx2 - OrientationPointX) + Pow2(ry2 - OrientationPointY);

                if (rd1 < rd2)
                {
                    centerX = rx1;
                    centerY = ry1;
                }
                else
                {
                    centerX = rx2;
                    centerY = ry2;
                }
            }
            else // Vertical Line
            {
                centerX = 0;
                centerY = 0;
                // The middle line is vertical. (Primitives3D)
                throw new ArgumentException(ErrorString.Exc16);
            }
        }

        private double Pow2(double x)
        {
            return x * x;
        }

        internal protected void AddTriangle(Vector3DCollection normals, Point3DCollection points, PointCollection texturePoints, Int32Collection indices, Point3D point1, Point3D point2, Point3D point3)
        {
            int indicesNum = points.Count;

            // Add points
            points.Add(point1);
            points.Add(point2);
            points.Add(point3);

            // Find the normal vector for the polygon
            Vector3D normalVector = NormalVector(point1, point2, point2, point3);

            // Add normals
            for (int point = 0; point <= 2; point++)
            {
                normals.Add(normalVector);
            }

            texturePoints.Add(new Point(0, 0));
            texturePoints.Add(new Point(0, 1));
            texturePoints.Add(new Point(1, 1));

            indices.Add(0 + indicesNum);
            indices.Add(1 + indicesNum);
            indices.Add(2 + indicesNum);
        }


        internal protected void AddPolygon(Vector3DCollection normals, Point3DCollection points, PointCollection texturePoints, Int32Collection indices, Point3D point1, Point3D point2, Point3D point3, Point3D point4)
        {
            AddPolygon(normals, points, texturePoints, indices, point1, point2, point3, point4, new Point(0, 0), new Point(0, 1), new Point(1, 1), new Point(1, 0));
        }

        private void AddPolygon(Vector3DCollection normals, Point3DCollection points, PointCollection texturePoints, Int32Collection indices, Point3D point1, Point3D point2, Point3D point3, Point3D point4, Point txt1, Point txt2, Point txt3, Point txt4)
        {
            int indicesNum = points.Count;

            // Add points
            points.Add(point1);
            points.Add(point2);
            points.Add(point3);
            points.Add(point4);

            // Find the normal vector for the polygon
            Vector3D normalVector = NormalVector(point1, point2, point2, point3);

            // Add normals
            for (int point = 0; point <= 3; point++)
            {
                normals.Add(normalVector);
            }

            texturePoints.Add(txt1);
            texturePoints.Add(txt2);
            texturePoints.Add(txt3);
            texturePoints.Add(txt4);

            indices.Add(0 + indicesNum);
            indices.Add(1 + indicesNum);
            indices.Add(3 + indicesNum);
            indices.Add(3 + indicesNum);
            indices.Add(1 + indicesNum);
            indices.Add(2 + indicesNum);
        }

        private void AddPolygonPart(Vector3DCollection normals, Point3DCollection points, PointCollection texturePoints, Int32Collection indices, Point3D point1, Point3D point2, Point3D point3, Point3D point4, Point3D center, Point txt1, Point txt2, Point txt3, Point txt4)
        {
            int indicesNum = points.Count;

            // Add points
            points.Add(point1);
            points.Add(point2);
            points.Add(point3);
            points.Add(point4);

            // Find the normal vectors for the polygon
            normals.Add(Point3D.Subtract(MidPoint(point1,point4), center));
            normals.Add(Point3D.Subtract(MidPoint(point2, point3), center));
            normals.Add(Point3D.Subtract(MidPoint(point2, point3), center));
            normals.Add(Point3D.Subtract(MidPoint(point1, point4), center));

            texturePoints.Add(txt1);
            texturePoints.Add(txt2);
            texturePoints.Add(txt3);
            texturePoints.Add(txt4);

            indices.Add(0 + indicesNum);
            indices.Add(1 + indicesNum);
            indices.Add(3 + indicesNum);
            indices.Add(3 + indicesNum);
            indices.Add(1 + indicesNum);
            indices.Add(2 + indicesNum);
        }
        
        private void AddPolygon(Vector3DCollection normals, Point3DCollection points, PointCollection texturePoints, Int32Collection indices, Point3D point1, Point3D point2, Point3D point3, Point3D point4, Point3D center, Point txt1, Point txt2, Point txt3, Point txt4)
        {
            double txtHeight = (txt4.Y - txt1.Y) / 4.0;

            Point3D midLPoint1 = MidPoint(point1, point2);
            Point3D midLPoint2 = MidPoint(point1, midLPoint1);
            Point3D midLPoint3 = MidPoint(midLPoint1, point2);

            Point3D midRPoint1 = MidPoint(point3, point4);
            Point3D midRPoint2 = MidPoint(point3, midRPoint1);
            Point3D midRPoint3 = MidPoint(midRPoint1, point4);

            AddPolygonPart(normals, points, texturePoints, indices, point1, midLPoint2, midRPoint3, point4, center, new Point(txt1.X, txt1.Y), new Point(txt4.X, txt1.Y + txtHeight), new Point(txt3.X, txt1.Y + txtHeight), new Point(txt2.X, txt2.Y));
            AddPolygonPart(normals, points, texturePoints, indices, midLPoint2, midLPoint1, midRPoint1, midRPoint3, center, new Point(txt1.X, txt1.Y + txtHeight), new Point(txt4.X, txt1.Y + 2 * txtHeight), new Point(txt3.X, txt1.Y + 2 * txtHeight), new Point(txt2.X, txt1.Y + txtHeight));
            AddPolygonPart(normals, points, texturePoints, indices, midLPoint1, midLPoint3, midRPoint2, midRPoint1, center, new Point(txt1.X, txt1.Y + 2 * txtHeight), new Point(txt4.X, txt1.Y + 3 * txtHeight), new Point(txt3.X, txt1.Y + 3 * txtHeight), new Point(txt2.X, txt1.Y + 2 * txtHeight));
            AddPolygonPart(normals, points, texturePoints, indices, midLPoint3, point2, point3, midRPoint2, center, new Point(txt1.X, txt1.Y + 3 * txtHeight), new Point(txt4.X, txt1.Y + 4 * txtHeight), new Point(txt3.X, txt1.Y + 4 * txtHeight), new Point(txt2.X, txt1.Y + 3 * txtHeight));
        }

        protected void AddPolygon(Vector3DCollection normals, Point3DCollection points, PointCollection texturePoints, Int32Collection indices, Point3D point1, Point3D point2, Point3D point3, Point3D point4, Point3D center, double txtWidth, double txtHeight)
        {
            double relHeight = 1;
            double relWidth = 1;
            if (txtWidth > txtHeight)
            {
                relHeight = txtHeight / txtWidth;
            }
            else
            {
                relWidth = txtWidth / txtHeight;
            }

            relHeight = txtHeight;
            relWidth = txtWidth;

            AddPolygon(normals, points, texturePoints, indices, point1, point2, point3, point4, center, new Point(0, 0), new Point(relWidth, 0), new Point(relWidth, relHeight), new Point(0, relHeight));

        }

        internal protected void AddTriangle(Vector3DCollection normals, Point3DCollection points, PointCollection texturePoints, Int32Collection indices, Point3D point1, Point3D point2, Point3D point3, Point3D center)
        {
           AddPolygonPart(normals, points, texturePoints, indices, point1, point2, point3, point1, center, new Point(0.5, 0), new Point(0, 1), new Point(1, 1), new Point(0.5, 0));    
        }

        internal protected Point3D MidPoint(Point3D point1, Point3D point2)
        {
            return new Point3D((point1.X + point2.X) / 2, (point1.Y + point2.Y) / 2, (point1.Z + point2.Z) / 2);
        }

        protected void CreateLabel(Point3DCollection points, Vector3DCollection normals, PointCollection texturePoints, Int32Collection indices, double width, double height, double depth, double depthCoeff)
        {
            depth *= depthCoeff;

            AddPolygon(normals, points, texturePoints, indices, new Point3D(-width, height, depth), new Point3D(-width, -height, depth), new Point3D(width, -height, depth), new Point3D(width, height, depth));
        }

        #endregion Methods
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