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

namespace Infragistics
{
    /// <summary>
    /// Provides extension methods for cloning shapes.
    /// </summary>
    public static class ShapeUtil
    {
        /// <summary>
        /// Duplicates the specified shape.
        /// </summary>
        /// <param name="shape">The shape.</param>
        /// <returns></returns>
        public static Shape Duplicate(this Shape shape)
        {
            Ellipse ellipse = shape as Ellipse;
            if (ellipse != null)
            {
                return ellipse.Duplicate();
            }

            Line line = shape as Line;
            if (line != null)
            {
                return line.Duplicate();
            }

            Rectangle rectangle = shape as Rectangle;
            if (rectangle != null)
            {
                return rectangle.Duplicate();
            }


            Polygon polygon = shape as Polygon;
            if (polygon != null)
            {
                return polygon.Duplicate();
            }

            Polyline polyline = shape as Polyline;
            if (polyline != null)
            {
                return polyline.Duplicate();
            }

            Path path = shape as Path;
            if (path != null)
            {
                return path.Duplicate();
            }

            return null;
        }

        /// <summary>
        /// Duplicates the specified ellipse.
        /// </summary>
        /// <param name="ellipse">The ellipse.</param>
        /// <returns></returns>
        public static Ellipse Duplicate(this Ellipse ellipse)
        {
            Ellipse newEllipse = new Ellipse();

            DuplicateShapeProperties(newEllipse, ellipse);

            return newEllipse;
        }
        /// <summary>
        /// Duplicates the specified line.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <returns></returns>
        public static Line Duplicate(this Line line)
        {
            Line newLine = new Line();

            newLine.X1 = line.X1;
            newLine.X2 = line.X2;

            newLine.Y1 = line.Y1;
            newLine.Y2 = line.Y2;

            DuplicateShapeProperties(newLine, line);

            return newLine;
        }
        /// <summary>
        /// Duplicates the specified rectangle.
        /// </summary>
        /// <param name="rectangle">The rectangle.</param>
        /// <returns></returns>
        public static Rectangle Duplicate(this Rectangle rectangle)
        {
            Rectangle newRectangle = new Rectangle();

            newRectangle.RadiusX = rectangle.RadiusX;
            newRectangle.RadiusY = rectangle.RadiusY;

            DuplicateShapeProperties(newRectangle, rectangle);

            return newRectangle;
        }

        /// <summary>
        /// Duplicates the specified polygon.
        /// </summary>
        /// <param name="polygon">The polygon.</param>
        /// <returns></returns>
        public static Polygon Duplicate(this Polygon polygon)
        {
            Polygon newPolygon = new Polygon();

            newPolygon.Points = polygon.Points.Duplicate();
            newPolygon.FillRule = polygon.FillRule;

            DuplicateShapeProperties(newPolygon, polygon);

            return newPolygon;
        }
        /// <summary>
        /// Duplicates the specified polyline.
        /// </summary>
        /// <param name="polyline">The polyline.</param>
        /// <returns></returns>
        public static Polyline Duplicate(this Polyline polyline)
        {
            Polyline newPolyline = new Polyline();

            newPolyline.Points = polyline.Points.Duplicate();
            newPolyline.FillRule = polyline.FillRule;

            DuplicateShapeProperties(newPolyline, polyline);

            return newPolyline;
        }
        /// <summary>
        /// Duplicates the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        public static Path Duplicate(this Path path)
        {
            Path newPath = new Path();

            Geometry geom = path.Data.Duplicate();

            newPath.Data = geom;

            DuplicateShapeProperties(newPath, path);

            return newPath;
        }

        private static void DuplicateShapeProperties(Shape newShape, Shape shape)
        {
            //newShape.Width = shape.Width;
            //newShape.Height = shape.Height;

            newShape.Fill = shape.Fill.Duplicate();
            newShape.Stroke = shape.Stroke.Duplicate();

            newShape.StrokeDashArray = shape.StrokeDashArray.Duplicate();
            newShape.StrokeDashCap = shape.StrokeDashCap;
            newShape.StrokeDashOffset = shape.StrokeDashOffset;
            newShape.StrokeEndLineCap = shape.StrokeEndLineCap;
            newShape.StrokeLineJoin = shape.StrokeLineJoin;
            newShape.StrokeMiterLimit = shape.StrokeMiterLimit;
            newShape.StrokeStartLineCap = shape.StrokeStartLineCap;
            newShape.StrokeThickness = shape.StrokeThickness;

            newShape.HorizontalAlignment = shape.HorizontalAlignment;
            newShape.VerticalAlignment = shape.VerticalAlignment;

            newShape.Visibility = shape.Visibility;
            newShape.Margin = shape.Margin;
            newShape.Stretch = shape.Stretch;

            newShape.RenderTransform = shape.RenderTransform.Duplicate();
            newShape.RenderTransformOrigin = shape.RenderTransformOrigin;

            newShape.IsHitTestVisible = shape.IsHitTestVisible;




        }
        /// <summary>
        /// Duplicates the specified double collection.
        /// </summary>
        /// <param name="doubleCollection">The double collection.</param>
        /// <returns></returns>
        public static DoubleCollection Duplicate(this DoubleCollection doubleCollection)
        {
            if (doubleCollection == null)
            {
                return null;
            }

            DoubleCollection newDoubleCollection = new DoubleCollection();

            foreach (Double value in doubleCollection)
            {
                newDoubleCollection.Add(value);
            }

            return newDoubleCollection;
        }

        /// <summary>
        /// Duplicates the specified point collection.
        /// </summary>
        /// <param name="pointCollection">The point collection.</param>
        /// <returns></returns>
        public static PointCollection Duplicate(this PointCollection pointCollection)
        {
            if (pointCollection == null)
            {
                return null;
            }

            PointCollection dup = new PointCollection();

            foreach (Point point in pointCollection)
            {
                dup.Add(point);
            }

            return dup;
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