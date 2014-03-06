using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Infragistics.Controls
{
    /// <summary>
    /// General purpose utility methods for collections of points.
    /// </summary>
    public static class PointCollectionUtil
    {
        /// <summary>
        /// Creates a simplified version of the the open simplex or polyline formed by the current points
        /// by removing internal points with the specified error tolerance.
        /// </summary>
        /// <param name="points">Polygon or polyline to flatten.</param>
        /// <param name="list">Destination to which the flattened polygon or polyline will be written (if null, a new one will be created)</param>
        /// <param name="E">Maximum flattenning error.</param>
        /// <returns>Point collection containing the flattened polygon or polyline.</returns>
        /// <remarks>
        /// The first and last point of the currents are always part of the returned points
        /// </remarks>
        public static void FlattenTo(this IList<Point> points, IList<Point> list, double E)
        {
            if (list == null)
            {
                return;
            }

            list.Clear();

            if (points.Count >= 2)
            {
                IList<int> indices = Flattener.Flatten(points.Count, (i) => { return points[i].X; }, (i) => { return points[i].Y; }, E);
                foreach (int i in indices)
                {
                    list.Add(points[i]);
                }
            }
        }
  

        /// <summary>
        /// Gets the axis-aligned bounding box for the cloud of points.
        /// </summary>
        /// <param name="points"></param>
        /// <returns>axis-aligned bounding rectangle or Rect.Empty</returns>
        public static Rect GetBounds(this IEnumerable<Point> points)
        {
            double xmin = Double.PositiveInfinity;
            double ymin = Double.PositiveInfinity;
            double xmax = Double.NegativeInfinity;
            double ymax = Double.NegativeInfinity;

            foreach(Point point in points)
            {
                xmin = Math.Min(xmin, point.X);
                ymin = Math.Min(ymin, point.Y);

                xmax = Math.Max(xmax, point.X);
                ymax = Math.Max(ymax, point.Y);
            }

            if(Double.IsInfinity(xmin) || Double.IsInfinity(ymin) || Double.IsInfinity(ymin) || Double.IsInfinity(ymax))
            {
                return Rect.Empty;
            }

            return new Rect(xmin, ymin, xmax - xmin, ymax - ymin);
        }

        /// <summary>
        /// Returns a Rect representing the bounds of all the points in the given list.
        /// </summary>
        /// <param name="points">A list of list of points.</param>
        /// <returns>A Rect representing the bounds of all given points.</returns>
        /// <remarks>The nested lists of points data structure corresponds to that which is used in the Shapefile format and other geospatial data sources.</remarks>
        public static Rect GetBounds(this IEnumerable<IEnumerable<Point>> points)
        {
            Rect result = Rect.Empty;
            foreach (IEnumerable<Point> ring in points)
            {
                result.Union(ring.GetBounds());
            }
            return result;
        }
        /// <summary>
        /// Gets the bounding rectangle containing all of the given Points.
        /// </summary>
        /// <param name="points">The Points to find a bounding rectangle for.</param>
        /// <returns>The bounding rectangle containing all of the given Points.</returns>
        public static Rect GetBounds(this IList<Point> points)
        {
            double xmin = Double.PositiveInfinity;
            double ymin = Double.PositiveInfinity;
            double xmax = Double.NegativeInfinity;
            double ymax = Double.NegativeInfinity;

            Point p;
            for (var i = 0; i < points.Count; i++)
            {
                p = points[i];
                xmin = Math.Min(xmin, p.X);
                ymin = Math.Min(ymin, p.Y);

                xmax = Math.Max(xmax, p.X);
                ymax = Math.Max(ymax, p.Y);
            }

            if (Double.IsInfinity(xmin) || Double.IsInfinity(ymin) || Double.IsInfinity(ymin) || Double.IsInfinity(ymax))
            {
                return Rect.Empty;
            }

            return new Rect(xmin, ymin, xmax - xmin, ymax - ymin);
        }
        /// <summary>
        /// Gets the bounding rectangle containing all of the given points.
        /// </summary>
        /// <param name="points">The Points to find a bounding rectangle for.</param>
        /// <returns>The bounding rectangle containing all of the given points.</returns>
        public static Rect GetBounds(this List<List<Point>> points)
        {
            Rect result = Rect.Empty;
            List<Point> ring;
            for (var i = 0; i < points.Count; i++)
            {
                ring = points[i];
                result.Union(ring.GetBounds());
            }
            return result;
        }
        /// <summary>
        /// Gets the bounding rectangle containing all of the given points.
        /// </summary>
        /// <param name="points">The Points to find a bounding rectangle for.</param>
        /// <returns>The bounding rectangle containing all of the given points.</returns>
        public static Rect GetBounds(this List<PointCollection> points)
        {
            Rect result = Rect.Empty;
            PointCollection ring;
            for (var i = 0; i < points.Count; i++)
            {
                ring = points[i];
                result.Union(ring.GetBounds());
            }
            return result;
        }
        /// <summary>
        /// Clips the given points using the given Clipper.
        /// </summary>
        /// <param name="points">The points to clip using the given Clipper.</param>
        /// <param name="list">Not used.</param>
        /// <param name="clipper">The Clipper to use when clipping the given Points.</param>
        public static void ClipTo(this IList<Point> points, IList<Point> list, Clipper clipper)
        {
            var pointCount = points.Count;
            for (var i = 0; i < pointCount; i++)
            {
                clipper.Add(points[i]);
            }

            clipper.Target = null;
        }

        /// <summary>
        /// Gets the centroid of the current cloud of points
        /// </summary>
        /// <param name="points">Cloud of points for which to calculate the centroid.</param>
        /// <returns>Centroid of the specified cloud of points.</returns>
        public static Point GetCentroid(this IEnumerable<Point> points)
        {
            double x = 0.0;
            double y = 0.0;
            double c = 0.0;

            foreach (Point point in points)
            {
                x += point.X;
                y += point.Y;

                c += 1.0;
            }

            return new Point(x / c, y / c);
        }
        /// <summary>
        /// Converts the given list of points to a PointCollection object.
        /// </summary>
        /// <param name="points">The points to copy to the resulting PointCollection.</param>
        /// <returns>A PointCollection containing all of the given points.</returns>
        public static PointCollection ToPointCollection(this IEnumerable<Point> points)
        {
            PointCollection result = new PointCollection();
            foreach (Point p in points)
            {
                result.Add(p);
            }
            return result;
        }
        /// <summary>
        /// Converts an IEnumerable of Points to a List of Points.
        /// </summary>
        /// <param name="points">The IEnumerable of Points to convert.</param>
        /// <returns>A List of Points constructed from the given IEnumerable of Points.</returns>
        public static List<Point> ToPointList(this IEnumerable<Point> points)
        {
            List<Point> result = new List<Point>();
            foreach (Point p in points)
            {
                result.Add(p);
            }
            return result;
        }

        //public static PointCollection ToPointCollection(this IList<Point> points)
        //{
        //    return new PointCollection(points);
        //}
        /// <summary>
        /// Converts a List of Lists of Points to a List of PointCollections.
        /// </summary>
        /// <param name="points">The List of Lists of Points to convert.</param>
        /// <returns>A List of PointCollections constructed from the given List of List of Points.</returns>
        public static List<PointCollection> ToPointCollections(this List<List<Point>> points)
        {
            List<PointCollection> ret = new List<PointCollection>();
            List<Point> pointColl;
            int count = points.Count;
            for (var i = 0; i < count; i++)
            {
                pointColl = points[i];



                PointCollection coll = new PointCollection();
                Point p;
                int pointCount = pointColl.Count;
                for (var j = 0; j < pointCount; j++)
                {
                    p = pointColl[j];
                    coll.Add(p);
                }

                ret.Add(coll);
            }
            return ret;
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