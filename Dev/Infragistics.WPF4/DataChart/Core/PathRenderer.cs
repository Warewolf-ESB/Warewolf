using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;



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

namespace Infragistics.Controls.Charts
{    
    internal class PathRenderer
    {
        public IFlattener Flattener { get; set; }

        public PathRenderer()
        {
            UnknownValuePlotting = UnknownValuePlotting.DontPlot;
        }

        public PathRenderer(IFlattener flattener)
        {
            Flattener = flattener;
        }

        public UnknownValuePlotting UnknownValuePlotting { get; set; }

        public void Render(Path path, IList<Point> points, double resolution)
        {
            var segments = NaNSegmenter.Segments(points, UnknownValuePlotting);
            var pathFigures = GetFigures(segments, resolution);
            PathGeometry data = new PathGeometry();
            foreach (PathFigure figure in pathFigures)
            {
                data.Figures.Add(figure);
            }
            path.Data = data;
        }

        private IEnumerable<PathFigure> GetFigures(IEnumerable<IList<Point>> segments, double resolution)
        {
            foreach (var segment in segments)
            {
                if (segment.Take(2).Count() >= 2)
                {
                    PathFigure figure = new PathFigure();
                    Point first = segment.First();
                    figure.StartPoint = first;
                    PolyLineSegment seg = CreatePolylineSegment(segment, resolution);
                    figure.Segments.Add(seg);
                    yield return figure;
                }
            }
        }

        private PolyLineSegment CreatePolylineSegment(IList<Point> segment, double resolution)
        {
            PolyLineSegment polySegment = new PolyLineSegment();
            if (Flattener == null)
            {
                foreach (Point point in segment.Skip(1))
                {
                    polySegment.Points.Add(point);
                }
            }
            else
            {
                foreach (Point point in Flattener.Flatten(segment, resolution).Skip(1))
                {
                    polySegment.Points.Add(point);
                }
            }
            return polySegment;
        }
    }

    internal class NaNSegmenter
    {
        public static IEnumerable<IList<Point>> Segments(IList<Point> points, UnknownValuePlotting mode)
        {
            IEnumerator<Point> pointEnumerator = points.GetEnumerator();
            IList<Point> curr = new List<Point>();
            bool header = true;
            while (pointEnumerator.MoveNext())
            {
                if (ShouldSkip(pointEnumerator.Current))
                {
                    if (!header && mode == UnknownValuePlotting.DontPlot)
                    {
                        header = true;
                        IList<Point> toReturn = curr;
                        curr = new List<Point>();
                        yield return new ReadOnlyCollection<Point>(toReturn);
                    }
                    else
                    {
                        continue;
                    }
                }
                else
                {
                    header = false;
                    curr.Add(pointEnumerator.Current);
                }
            }
            yield return new ReadOnlyCollection<Point>(curr);
        }

        public static bool ShouldSkip(Point p)
        {
            return (double.IsNaN(p.X) ||
                double.IsNaN(p.Y) ||
                double.IsInfinity(p.X) ||
                double.IsInfinity(p.Y));
        }

        public static bool ShouldTake(Point p)
        {
            return !ShouldSkip(p);
        }
    }

    internal interface IFlattener
    {
        IList<Point> Flatten(IList<Point> points, double resolution);
    }

    /// <summary>
    /// Represents the default flattener class.
    /// </summary>
    public class DefaultFlattener
        : IFlattener
    {
        private const int FlattenerChunking = 512;

        private IList<int> FlattenHelper(
            IList<int> result, 
            Func<int, double> X, 
            Func<int, double> Y, 
            int b, 
            int e, 
            double E)
        {
            List<int> indices = new List<int>();

            int start = b;
            int end = e;
            int toFlatten = end - start + 1;

            while (toFlatten > 0)
            {
                if (toFlatten <= FlattenerChunking)
                {
                    Flattener.Flatten(indices, X, Y, start, end, E);
                    start = end + 1;
                }
                else
                {
                    int currentEnd = start + FlattenerChunking - 1;
                    Flattener.Flatten(indices, X, Y, start, currentEnd, E);
                    start = currentEnd + 1;

                }
                toFlatten = end - start + 1;
            }

            return indices;
        }

        private List<int> FastFlattenHelper(
            double[] X,
            double[] Y,
            int b,
            int e,
            double E)
        {
            List<int> indices = new List<int>();

            int start = b;
            int end = e;
            int toFlatten = end - start + 1;

            while (toFlatten > 0)
            {
                if (toFlatten <= FlattenerChunking)
                {
                    Flattener.FastFlatten(indices, X, Y, start, end, E);
                    start = end + 1;
                }
                else
                {
                    int currentEnd = start + FlattenerChunking - 1;
                    Flattener.FastFlatten(indices, X, Y, start, currentEnd, E);
                    start = currentEnd + 1;

                }
                toFlatten = end - start + 1;
            }

            return indices;
        }

        /// <summary>
        /// Flattens a set of points
        /// </summary>
        /// <param name="points">Collection of points to flatten</param>
        /// <param name="resolution">Resolution</param>
        /// <returns>Collection of flattened points</returns>
        public IList<Point> Flatten(IList<Point> points, double resolution)
        {
            //IList<Point> pointsList = points.ToList();
            Func<int, double> x = (i) => GetX(points, i);
            Func<int, double> y = (i) => GetY(points, i);
            return GetFlattened(points, resolution, x, y);
        }

        /// <summary>
        /// Flattens a set of points
        /// </summary>
        /// <param name="x">The array of x values to flatten.</param>
        /// <param name="y">The array of y values to flatten.</param>
        /// <param name="count">The number of values being provided.</param>
        /// <param name="resolution">The resolution to flatten the points to.</param>
        /// <returns>The list of flattened points.</returns>
        public List<Point> FastFlatten(double[] x, double[] y, int count, double resolution)
        {
            //IList<Point> pointsList = points.ToList();
            return GetFastFlattened(x, y, count, resolution);
        }

        /// <summary>
        /// Gets the collection of flattened points.
        /// </summary>
        /// <param name="pointsList">List of points</param>
        /// <param name="resolution">Resolution</param>
        /// <param name="x">X-function</param>
        /// <param name="y">Y-function</param>
        /// <returns>Collection of flattened points</returns>
        public IList<Point> GetFlattened(IList<Point> pointsList, double resolution, Func<int, double> x, Func<int, double> y)
        {
            var indices = FlattenHelper(new List<int>(), x, y, 0, pointsList.Count - 1, resolution);
            var reordered = new RearrangedList<Point>(pointsList, indices);
            return reordered;
        }

        /// <summary>
        /// Get the collection of flattened points.
        /// </summary>
        /// <param name="x">The array of x values to flatten.</param>
        /// <param name="y">The array of y values to flatten.</param>
        /// <param name="count">The number of values being provided.</param>
        /// <param name="resolution">The resolution to flatten the points to.</param>
        /// <returns>The list of flattened points.</returns>
        public List<Point> GetFastFlattened(double[] x, double[] y, int count, double resolution)
        {
            var indices = FastFlattenHelper(x,y, 0, count - 1, resolution);
            List<Point> ret = new List<Point>();
            for (var i = 0; i < indices.Count; i++)
            {
                ret.Add(new Point(x[indices[i]], y[indices[i]]));
            }
            return ret;
        }

        private double GetX(IList<Point> list, int i)
        {
            return list[i].X;
        }

        private double GetY(IList<Point> list, int i)
        {
            return list[i].Y;
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