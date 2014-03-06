using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows;
using System.ComponentModel;

namespace Infragistics
{
    /// <summary>
    /// Utility class for line flattening.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class Flattener
    {
        private Flattener() {  }

        #region Flattened Spiral
        /// <summary>
        /// Flatten the specified Archimidean spiral curve with the maximum specified area error.
        /// </summary>
        /// <param name="startAngle">Curve start angle in radians.</param>
        /// <param name="startRadius">Curve start radius.</param>
        /// <param name="endAngle">Curve end angle in radians.</param>
        /// <param name="endRadius">Curve end radius.</param>
        /// <param name="error">Maximum error between approximation and spiral.</param>
        /// <returns>List of interpolation parameters in the range [0, 1].</returns>
        public static List<double> Spiral(double startAngle, double startRadius,
                                            double endAngle, double endRadius,
                                            double error)
        {
            if (double.IsNaN(error) || error <= 0.0)
            {
                error = 1.0;
            }

            List<double> ret = new List<double>();

            Stack<SpiralTodo> todo = new Stack<SpiralTodo>();
            double b = (endRadius - startRadius) / (endAngle - startAngle);     // Archimidean spiral parameter
            double a = startRadius - b * startAngle;                            // Archimidean spiral parameter

            double b2 = b * b;
            double a2 = a * a;
            double ab = a * b;
            todo.Push(new SpiralTodo() { p0 = 0, p1 = 1 });                         // start with the full range

            while (todo.Count != 0)
            {
                SpiralTodo s = todo.Pop();

                double r0 = startRadius + s.p0 * (endRadius - startRadius);     // range start radius  
                double t0 = startAngle + s.p0 * (endAngle - startAngle);        // range start angle  
                double t02 = t0 * t0;
                double t03 = t02 * t0;

                double r1 = startRadius + s.p1 * (endRadius - startRadius);     // range end radius
                double t1 = startAngle + s.p1 * (endAngle - startAngle);        // range end angle
                double t12 = t1 * t1;
                double t13 = t12 * t1;

                double segment;
                if (b == 0)
                {
                    segment = a2 * (t1 - t0) / 2.0 + ab * (t12 - t02) / 2.0 + b2 * (t13 - t03) / 6.0;
                }
                else
                {
                    //only apply L'Hopital's rule in the edge case, as there are other cases were it does
                    //not supply correct results.
                    segment = (System.Math.Pow(a + b * t1, 3) - System.Math.Pow(a + b * t0, 3)) / (6.0 * b);
                }
                double triangle = 0.5 * r0 * r1 * System.Math.Sin(t1 - t0);            // area of triangular approixmation

                if (segment - triangle > error)
                {
                    double pm = 0.5 * (s.p0 + s.p1);                            // split the range in the middle

                    todo.Push(new SpiralTodo() { p0 = pm, p1 = s.p1 });         // recurse into upper half (do second)
                    todo.Push(new SpiralTodo() { p0 = s.p0, p1 = pm });         // recurse into lower half (do first)
                }
                else
                {
                    ret.Add(s.p0);                                              // save the range start
                }
            }

            ret.Add(1.0);                                                       // save the range end

            return ret;
        }

        #endregion

        #region Flattening
        /// <summary>
        /// Flatten a line according to the specified resolution using the Douglas-Peucker algorithm.
        /// </summary>
        /// <param name="count">Number of points in line.</param>
        /// <param name="X">x coordinate of ith point.</param>
        /// <param name="Y">y coordinate of ith point.</param>
        /// <param name="resolution">Maximum flattening error.</param>
        /// <returns>Indices of points forming a flattened version. The first and last points from the original line are guaranteed to
        /// be present.</returns>
        public static IList<int> Flatten(int count, Func<int, double> X, Func<int, double> Y, double resolution)
        {
            List<int> indices = new List<int>();
            Flatten(indices, X, Y, 0, count - 1, resolution); 
            return indices;
        }
        /// <summary>
        /// Flatten a line according to the specified resolution using the Douglas-Peucker algorithm.
        /// </summary>
        /// <param name="result">A list of indices of the flattened points.</param>
        /// <param name="X">x coordinate of ith point.</param>
        /// <param name="Y">y coordinate of ith point.</param>
        /// <param name="b">Beginning index of the flattening operation.</param>
        /// <param name="e">Ending index of the flattening operation.</param>
        /// <param name="E">Maximum flattening error.</param>
        /// <returns>Indices of points forming a flattened version. The first and last points from the original line are guaranteed to
        /// be present.</returns>
        public static IList<int> Flatten(IList<int> result, Func<int, double> X, Func<int, double> Y, int b, int e, double E)
        {
            return Flatten(result, (i) => { return i; }, X, Y, b, e, E);
        }
        /// <summary>
        /// Flatten a line according to the specified resolution using the Douglas-Peucker algorithm.
        /// </summary>
        /// <param name="result">A list of indices of the flattened points.</param>
        /// <param name="indices">The indices of the points being flattened.</param>
        /// <param name="X">x coordinate of ith point.</param>
        /// <param name="Y">y coordinate of ith point.</param>
        /// <param name="b">Beginning index of the flattening operation.</param>
        /// <param name="e">Ending index of the flattening operation.</param>
        /// <param name="E">Maximum flattening error.</param>
        /// <returns>Indices of points forming a flattened version. The first and last points from the original line are guaranteed to
        /// be present.</returns>
        public static IList<int> Flatten(IList<int> result, IList<int> indices, Func<int, double> X, Func<int, double> Y, int b, int e, double E)
        {
            return Flatten(result, (i) => { return indices[i]; }, X, Y, b, e, E);
        }

        private static IList<int> Flatten(IList<int> result, Func<int, int> getIndex, Func<int, double> X, Func<int, double> Y, int b, int e, double E)
        {
            #region NaN and index checks
            if (b > e)
            {
                return result;
            }

            double Xb = X(getIndex(b));
            double Yb = Y(getIndex(b));

            while ((double.IsNaN(Xb) || double.IsNaN(Yb)) && b < e)
            {
                ++b;
                Xb = X(getIndex(b));
                Yb = Y(getIndex(b));
            }

            double Xe = X(getIndex(e));
            double Ye = Y(getIndex(e));

            while ((double.IsNaN(Xe) || double.IsNaN(Ye)) && b < e)
            {
                --e;
                Xe = X(getIndex(e));
                Ye = Y(getIndex(e));
            }

            if (b == e)
            {
                result.Add(getIndex(b));
                return result;
            }
            #endregion

            result.Add(getIndex(b));
            FlattenRecursive(result, getIndex, X, Y, b, e, E);
            result.Add(getIndex(e));

            return result;
        }
        /// <summary>
        /// Performance optimized flattening routine.
        /// </summary>
        /// <param name="result">A list of indices of the flattened points.</param>
        /// <param name="X">x coordinate of ith point.</param>
        /// <param name="Y">y coordinate of ith point.</param>
        /// <param name="b">Beginning index of the flattening operation.</param>
        /// <param name="e">Ending index of the flattening operation.</param>
        /// <param name="E">Maximum flattening error.</param>
        /// <returns>Indices of points forming a flattened version. The first and last points from the original line are guaranteed to
        /// be present.</returns>
        public static List<int> FastFlatten(List<int> result, double[] X, double[] Y, int b, int e, double E)
        {
            #region NaN and index checks
            if (b > e)
            {
                return result;
            }

            double Xb = X[b];
            double Yb = Y[b];

            while ((double.IsNaN(Xb) || double.IsNaN(Yb)) && b < e)
            {
                ++b;
                Xb = X[b];
                Yb = Y[b];
            }

            double Xe = X[e];
            double Ye = Y[e];

            while ((double.IsNaN(Xe) || double.IsNaN(Ye)) && b < e)
            {
                --e;
                Xe = X[e];
                Ye = Y[e];
            }

            if (b == e)
            {
                result.Add(b);
                return result;
            }
            #endregion

            result.Add(b);
            FastFlattenRecursive(result, X, Y, b, e, E);
            result.Add(e);

            return result;
        }



#region Infragistics Source Cleanup (Region)








































































































#endregion // Infragistics Source Cleanup (Region)


        private static void FastFlattenRecursive(List<int> result, double[] X, double[] Y, int b, int e, double E)
        {
            #region NaN and index checks

            double Xb = X[b];
            double Yb = Y[b];

            while ((double.IsNaN(Xb) || double.IsNaN(Yb)) && b < e)
            {
                ++b;
                Xb = X[b];
                Yb = Y[b];
            }

            double Xe = X[e];
            double Ye = Y[e];

            while ((double.IsNaN(Xe) || double.IsNaN(Ye)) && b < e)
            {
                --e;
                Xe = X[e];
                Ye = Y[e];
            }

            if (b + 1 >= e)
            {
                return;
            }

            #endregion

            int si = -1;
            double se = E * E;
            double xDelt;
            double yDelt;

            xDelt = Xe - Xb;
            yDelt = Ye - Yb;
            double L = xDelt * xDelt + yDelt * yDelt;

            // consider the line segment from point {Xb, Yb} to {Xe, Ye}. iteratively consider points i.
            // for each i, compute the distance from point i to the line segment, and if it is the greatest distance heretofor observed, save the index.
            // at the end of the loop, variable si will be the index of the point with greatest distance to the line segment.
            

            if (L == 0.0) // in this case, line segment is degenerate
            {
                for (int i = b + 1; i < e; ++i)
                {
                    double Xi = X[i];
                    double Yi = Y[i];

                    if (double.IsNaN(Xi) || double.IsNaN(Yi))
                    {
                        continue;
                    }

                    xDelt = Xe - Xi;
                    yDelt = Ye - Yi;
                    double err = xDelt * xDelt + yDelt * yDelt;

                    if (err >= se)
                    {
                        se = err;
                        si = i;
                    }
                }
            }
            else
            {
                double vx = Xe - Xb;
                double vy = Ye - Yb;

                for (int i = b + 1; i < e; ++i)
                {
                    double Xi = X[i];
                    double Yi = Y[i];

                    if (double.IsNaN(Xi) || double.IsNaN(Yi))
                    {
                        continue;
                    }

                    double err = double.NaN;

                    double wx = X[i] - Xb;
                    double wy = Y[i] - Yb;

                    double c1 = vx * wx + vy * wy;

                    if (c1 <= 0.0)
                    {
                        xDelt = Xb - Xi;
                        yDelt = Yb - Yi;
                        err = xDelt * xDelt + yDelt * yDelt;
                    }
                    else
                    {
                        double c2 = vx * vx + vy * vy;

                        if (c2 <= c1)
                        {
                            xDelt = Xe - Xi;
                            yDelt = Ye - Yi;
                            err = xDelt * xDelt + yDelt * yDelt;
                        }
                        else
                        {
                            double p = c1 / c2;
                            xDelt = Xb + p * vx - Xi;
                            yDelt = Yb + p * vy - Yi;
                            err = xDelt * xDelt + yDelt * yDelt;
                        }
                    }

                    if (err >= se)
                    {
                        se = err;
                        si = i;
                    }
                }
            }

            if (si != -1)
            {
                FastFlattenRecursive(result, X, Y, b, si, E);      // big error - flatten from b to si
                result.Add(si);
                FastFlattenRecursive(result, X, Y, si, e, E);  // big error - flatten from si+1 to e
            }

            return;
        }



#region Infragistics Source Cleanup (Region)



























































































































































































































#endregion // Infragistics Source Cleanup (Region)


        private static void FlattenRecursive(IList<int> result, Func<int, int> getIndex, Func<int, double> X, Func<int, double> Y, int b, int e, double E)
        {
            #region NaN and index checks

            double Xb = X(getIndex(b));
            double Yb = Y(getIndex(b));

            while((double.IsNaN(Xb)||double.IsNaN(Yb))&&b<e)
            {
                ++b;
                Xb = X(getIndex(b));
                Yb = Y(getIndex(b));
            }

            double Xe = X(getIndex(e));
            double Ye = Y(getIndex(e));

            while ((double.IsNaN(Xe) || double.IsNaN(Ye)) && b < e)
            {
                --e;
                Xe = X(getIndex(e));
                Ye = Y(getIndex(e));
            }

            if (b + 1 >= e)
            {
                return;
            }

            #endregion

            int si = -1;
            double se = E;
            double L = MathUtil.Hypot(Xe - Xb, Ye - Yb);

            // consider the line segment from point {Xb, Yb} to {Xe, Ye}. iteratively consider points i.
            // for each i, compute the distance from point i to the line segment, and if it is the greatest distance heretofor observed, save the index.
            // at the end of the loop, variable si will be the index of the point with greatest distance to the line segment.

            if (L == 0.0) // in this case, line segment is degenerate
            {
                for (int i = b + 1; i < e; ++i)
                {
                    double Xi = X(getIndex(i));
                    double Yi = Y(getIndex(i));

                    if (double.IsNaN(Xi) || double.IsNaN(Yi))
                    {
                        continue;
                    }

                    double err = MathUtil.Hypot(Xe - Xi, Ye - Yi);

                    if (err >= se)
                    {
                        se = err;
                        si = i;
                    }
                }
            }
            else
            {
                double vx = Xe - Xb;
                double vy = Ye - Yb;

                for (int i = b + 1; i < e; ++i)
                {
                    double Xi = X(getIndex(i));
                    double Yi = Y(getIndex(i));

                    if (double.IsNaN(Xi) || double.IsNaN(Yi))
                    {
                        continue;
                    }

                    double err = double.NaN;

                    double wx = X(getIndex(i)) - Xb;
                    double wy = Y(getIndex(i)) - Yb;

                    double c1 = vx * wx + vy * wy;

                    if (c1 <= 0.0)
                    {
                        err = MathUtil.Hypot(Xb - Xi, Yb - Yi);
                    }
                    else
                    {
                        double c2 = vx * vx + vy * vy;

                        if (c2 <= c1)
                        {
                            err = MathUtil.Hypot(Xe - Xi, Ye - Yi);
                        }
                        else
                        {
                            double p = c1 / c2;
                            err = MathUtil.Hypot(Xb + p * vx - Xi, Yb + p * vy - Yi);
                        }
                    }

                    if (err >= se)
                    {
                        se = err;
                        si = i;
                    }
                }
            }

            if (si != -1)
            {
                FlattenRecursive(result, getIndex, X, Y, b, si, E);      // big error - flatten from b to si
                result.Add(getIndex(si));
                FlattenRecursive(result, getIndex, X, Y, si, e, E);  // big error - flatten from si+1 to e
            }

            return;


#region Infragistics Source Cleanup (Region)































































#endregion // Infragistics Source Cleanup (Region)

        }
        #endregion

        #region Smoothing

        /// <summary>
        /// Smooth a line into a PathFigure of quadratic and cubic bezier splines.
        /// </summary>
        /// <param name="count">Number of points in line.</param>
        /// <param name="X">X coordinate of ith point.</param>
        /// <param name="Y">Y coordinate of ith point.</param>
        /// <param name="resolution">Maximum flattening error.</param>
        /// <returns>Smooth line.</returns>
        public static PathFigure Smooth(int count, Func<int, double> X, Func<int, double> Y, double resolution)
        {
            IList<int> indices = Flatten(count, X, Y, resolution);

            return Smooth(indices.Count, (i) => { return X(indices[i]); }, (i) => { return Y(indices[i]); });
        }

        /// <summary>
        /// Returns the points for a spline with the given key points.
        /// </summary>
        /// <param name="count">The number of source points.</param>
        /// <param name="X">Delegate to find the X-coordinate of a key point at a given index.</param>
        /// <param name="Y">Delegate to find the Y-coordinate of a key point at a given index.</param>
        /// <returns>The points for a spline with the given key points.</returns>
        public static PointCollection Spline(int count, Func<int, double> X, Func<int, double> Y)
        {
            PointCollection spline = new PointCollection();

            if (count < 5)
            {
                for (int i = 0; i < count; ++i)
                {
                    spline.Add(new Point(X(i), Y(i)));
                }

                return spline;
            }
            spline.Add(new Point(X(0), Y(0))); // output the start point

            int n = count - 1;

            Point pa;
            Point pb = new Point(X(0), Y(0));
            Point pc = new Point(X(0 + 1), Y(0 + 1));
            Point pd = new Point(X(0 + 2), Y(0 + 2));

            Point eab;
            double mab;

            Point ebc = new Point(pc.X - pb.X, pc.Y - pb.Y);
            double mbc = MathUtil.Hypot(ebc.X, ebc.Y);

            Point ecd = new Point(pd.X - pc.X, pd.Y - pc.Y);
            double mcd = MathUtil.Hypot(ecd.X, ecd.Y);

            Point tc;
            double sc;

            double alpha = 0.10;
            double beta = 0.30;

            {
                tc = new Point(pd.X - pb.X, pd.Y - pb.Y); { double m = MathUtil.Hypot(tc.X, tc.Y); tc.X /= m; tc.Y /= m; }
                sc = 0.5 + (ebc.X * ecd.X + ebc.Y * ecd.Y) / (2.0 * mbc * mcd);

                spline.Add(new Point(pc.X - tc.X * (alpha + beta * sc) * mbc, pc.Y - tc.Y * (alpha + beta * sc) * mbc));
                spline.Add(pc);
            }

            for (int i = 1; i < n - 1; ++i)
            {
                pa = pb;
                pb = pc;
                pc = pd;
                pd = new Point(X(i + 2), Y(i + 2));

                eab = ebc;
                mab = mbc;

                ebc = ecd;
                mbc = mcd;

                ecd = new Point(pd.X - pc.X, pd.Y - pc.Y);
                mcd = MathUtil.Hypot(ecd.X, ecd.Y);

                Point tb = tc;
                double sb = sc;

                tc = new Point(pd.X - pb.X, pd.Y - pb.Y); { double m = MathUtil.Hypot(tc.X, tc.Y); tc.X /= m; tc.Y /= m; }
                sc = 0.5 + (ebc.X * ecd.X + ebc.Y * ecd.Y) / (2.0 * mbc * mcd);

                spline.Add(new Point(pb.X + tb.X * (alpha + beta * sb) * mbc, pb.Y + tb.Y * (alpha + beta * sb) * mbc));
                spline.Add(new Point(pc.X - tc.X * (alpha + beta * sc) * mbc, pc.Y - tc.Y * (alpha + beta * sc) * mbc));
                spline.Add(pc);
            }

            {
                pa = pb;
                pb = pc;
                pc = pd;
                // pd
                eab = ebc;
                mab = mbc;

                ebc = ecd;
                mbc = mcd;

                Point tb = tc;
                double sb = sc;

                spline.Add(new Point(pb.X + tb.X * (alpha + beta * sb) * mbc, pb.Y + tb.Y * (alpha + beta * sb) * mbc));
                spline.Add(pc);
            }

            return spline;
        }


        /// <summary>
        /// Smooth a line into a PathFigure of quadratic and cubic bezier splines.
        /// </summary>
        /// <param name="count">Number of points in line.</param>
        /// <param name="X">X coordinate of ith point.</param>
        /// <param name="Y">Y coordinate of ith point.</param>
        /// <returns>Smooth line.</returns>
        public static PathFigure Smooth(int count, Func<int, double> X, Func<int, double> Y)
        {
            PathFigure pathFigure = new PathFigure() { StartPoint = new Point(X(0), Y(0)) };

            int n = count - 1;

            Point pa;
            Point pb = new Point(X(0), Y(0));
            Point pc = new Point(X(0 + 1), Y(0 + 1));
            Point pd = new Point(X(0 + 2), Y(0 + 2));

            Point eab;
            double mab;

            Point ebc = new Point(pc.X - pb.X, pc.Y - pb.Y);
            double mbc = MathUtil.Hypot(ebc.X, ebc.Y);

            Point ecd = new Point(pd.X - pc.X, pd.Y - pc.Y);
            double mcd = MathUtil.Hypot(ecd.X, ecd.Y);

            Point tc;
            double sc;

            double alpha = 0.15;
            double beta = 0.45;

            {
                // first quadratic patch

                tc = new Point(pd.X - pb.X, pd.Y - pb.Y); { double m = MathUtil.Hypot(tc.X, tc.Y); tc.X /= m; tc.Y /= m; }
                sc = 0.5 + (ebc.X * ecd.X + ebc.Y * ecd.Y) / (2.0 * mbc * mcd);

                QuadraticBezierSegment segment = new QuadraticBezierSegment()
                {
                    Point1 = new Point(pc.X - tc.X * (alpha + beta * sc) * mbc, pc.Y - tc.Y * (alpha + beta * sc) * mbc),
                    Point2 = pc
                };
                pathFigure.Segments.Add(segment);
            }

            for (int i = 1; i < n - 1; ++i)
            {
                // intermediate cubic patches

                pa = pb;
                pb = pc;
                pc = pd;
                pd = new Point(X(i + 2), Y(i + 2));

                eab = ebc;
                mab = mbc;

                ebc = ecd;
                mbc = mcd;

                ecd = new Point(pd.X - pc.X, pd.Y - pc.Y);
                mcd = MathUtil.Hypot(ecd.X, ecd.Y);

                Point tb = tc;
                double sb = sc;

                tc = new Point(pd.X - pb.X, pd.Y - pb.Y); { double m = MathUtil.Hypot(tc.X, tc.Y); tc.X /= m; tc.Y /= m; }
                sc = 0.5 + (ebc.X * ecd.X + ebc.Y * ecd.Y) / (2.0 * mbc * mcd);

                BezierSegment segment = new BezierSegment()
                {
                    Point1 = new Point(pb.X + tb.X * (alpha + beta * sb) * mbc, pb.Y + tb.Y * (alpha + beta * sb) * mbc),
                    Point2 = new Point(pc.X - tc.X * (alpha + beta * sc) * mbc, pc.Y - tc.Y * (alpha + beta * sc) * mbc),
                    Point3 = pc
                };
                pathFigure.Segments.Add(segment);
            }

            {
                // last quadratic patch

                pa = pb;
                pb = pc;
                pc = pd;
                // pd
                eab = ebc;
                mab = mbc;

                ebc = ecd;
                mbc = mcd;

                Point tb = tc;
                double sb = sc;

                QuadraticBezierSegment segment = new QuadraticBezierSegment()
                {
                    Point1 = new Point(pb.X + tb.X * (alpha + beta * sb) * mbc, pb.Y + tb.Y * (alpha + beta * sb) * mbc),
                    Point2 = pc
                };
                pathFigure.Segments.Add(segment);
            }

            return pathFigure;
        }

        #endregion
    }




    internal struct SpiralTodo

    {
        public double p0;
        public double p1;
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