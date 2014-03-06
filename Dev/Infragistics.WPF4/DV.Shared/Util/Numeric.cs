using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace Infragistics.Controls.Charts.Util
{
    /// <summary>
    /// Base class for algorithms which work on indexed series of numbers.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class Numeric
    {
        /// <summary>
        /// Creates and initialises a default, empty Numeric object.
        /// </summary>
        protected Numeric() {  }

        /// <summary>
        /// Delegate which returns an indexed comparable.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public delegate IComparable ComparableDelegate(int index);

        /// <summary>
        /// Solve a linear tridiagonal matrix system
        /// </summary>
        /// <returns>True if system has been correctly solved.</returns>
        internal static bool Solve(List<double> a, List<double> b, List<double> c, List<double> r, List<double> u)
        {
            int j;
            int n = a.Count;
            double[] gam = new double[n];

            if (b[0] == 0.0)
            {
                // throw new ArgumentException("Error 1 in tridag");
                return false;
            }

            double bet = b[0];

            u[0] = r[0] / (bet);

            for (j = 1; j < n; j++)
            {
                gam[j] = c[j - 1] / bet;
                bet = b[j] - a[j] * gam[j];

                if (bet == 0.0)
                {
                    // new ArgumentException("Error 2 in tridag");
                    return false;
                }

                u[j] = (r[j] - a[j] * u[j - 1]) / bet;
            }

            for (j = (n - 2); j >= 0; j--)
            {
                u[j] -= gam[j + 1] * u[j + 1];
            }

            return true;
        }

//#if !TINYCLR
        /// <summary>
        /// Solve a system of linear equations using gauss-jordan eliminiation
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns>True if system has been correctly solved.</returns>
        internal static bool Solve(double[,] a, double[] b)
        {
            int n = a.GetLength(0);

            int[] indxc = new int[n];
            int[] indxr = new int[n];
            int[] ipiv = new int[n];

            for (int i = 0; i < n; i++)
            {
                ipiv[i] = 0;
            }

            for (int i = 0; i < n; i++)
            {
                double big = 0.0;
                int irow = 0;
                int icol = 0;

                for (int j = 0; j < n; j++)
                {
                    if (ipiv[j] != 1)
                    {
                        for (int k = 0; k < n; k++)
                        {
                            if (ipiv[k] == 0)
                            {
                                if (Math.Abs(a[j, k]) >= big)
                                {
                                    big = Math.Abs(a[j, k]);
                                    irow = j;
                                    icol = k;
                                }
                            }
                        }
                    }
                }

                ++(ipiv[icol]);

                if (irow != icol)
                {
                    for (int j = 0; j < n; j++)
                    {
                        double t = a[irow, j];

                        a[irow, j] = a[icol, j];
                        a[icol, j] = t;
                    }

                    {
                        double t = b[irow];

                        b[irow] = b[icol];
                        b[icol] = t;
                    }
                }

                indxr[i] = irow;
                indxc[i] = icol;

                if (a[icol, icol] == 0.0)
                {
                    return false;   // matrix is singular
                }

                double pivinv = 1.0 / a[icol, icol];
                a[icol, icol] = 1.0;

                for (int j = 0; j < n; j++)
                {
                    a[icol, j] *= pivinv;
                }

                b[icol] *= pivinv;

                for (int j = 0; j < n; j++)
                {
                    if (j != icol)
                    {
                        double dum = a[j, icol];

                        a[j, icol] = 0.0;

                        for (int l = 0; l < n; l++)
                        {
                            a[j, l] -= a[icol, l] * dum;
                        }

                        b[j] -= b[icol] * dum;
                    }
                }
            }

            for (int i = n - 1; i >= 0; i--)
            {
                if (indxr[i] != indxc[i])
                {
                    for (int j = 0; j < n; j++)
                    {
                        double t = a[j, indxr[i]];

                        a[j, indxr[i]] = a[j, indxc[i]];
                        a[j, indxc[i]] = t;
                    }
                }
            }

            return true;
        }
//#endif



#region Infragistics Source Cleanup (Region)























































#endregion // Infragistics Source Cleanup (Region)

        /// <summary>
        /// Evaluates the coefficients for cubic spline interpolation
        /// of the tabultated function represented by Count, x, y.
        /// </summary>
        /// <param name="count">The number of samples.</param>
        /// <param name="x">Delegate returning the x value for the ith sample.</param>
        /// <param name="y">Delegate returning the x value for the ith sample.</param>
        /// <param name="yp1">First derivative at first point (use double.NaN for natural spline)</param>
        /// <param name="ypn">First derivative at last point (use double.NaN for natural spline)</param>
        /// <remarks>
        /// The presence of double.NaN in the input will result in local double.NaN
        /// in the output.
        /// </remarks>
        /// <returns>Coefficients for the cubic spline interpolation</returns>
        public static double[] SafeCubicSplineFit(int count, Func<int, double> x, Func<int, double> y, double yp1, double ypn)
        {
            List<double> ret=new List<double>();

            for (int i = 0; i < count; ++i)
            {
                while(i < count && (double.IsNaN(x(i))||double.IsNaN(y(i))))
                {
                    ret.Add(double.NaN);
                    ++i;
                }

                int j=i; 

                while(i < count && !double.IsNaN(x(i))&&!double.IsNaN(y(i)))
                {
                    ++i;
                }
     
                --i;

                if (i - j > 0) // [DN 3/12/2010:29564] 2 points is enough to use the CubicSplineFit function
                {
                    ret.AddRange(CubicSplineFit(j, i - j + 1, x, y, yp1, ypn));
                }
                else
                {
                    for (; j <= i; ++j)
                    {
                        ret.Add(double.NaN);
                    }
                }
            }

            return ret.ToArray();
        }

        /// <summary>
        /// Evaluates the coefficients for natural or clamped cubic spline interpolation
        /// of the tabultated function represented by x, y.
        /// </summary>
        /// <remarks>
        /// The presence of double.NaN in the input will result in completely invalid output.
        /// </remarks>
        /// <param name="start">The index of the first sample.</param>
        /// <param name="count">The number of samples.</param>
        /// <param name="x">Delegate returning the x value for the ith sample.</param>
        /// <param name="y">Delegate returning the x value for the ith sample.</param>
        /// <param name="yp1">First derivative at first point (use double.NaN for natural spline)</param>
        /// <param name="ypn">First derivative at last point (use double.NaN for natural spline)</param>
        /// <returns>Coefficients for the cubic spline interpolation</returns>
        public static double[] CubicSplineFit(int start, int count, Func<int, double> x, Func<int, double> y, double yp1, double ypn)
        {
            return CubicSplineFit(count, (i) => x(i + start), (i) => y(i + start), yp1, ypn);
        }

        /// <summary>
        /// Evaluates the coefficients for natural or clamped cubic spline interpolation
        /// of the tabultated function represented by x, y.
        /// </summary>
        /// <remarks>
        /// The presence of double.NaN in the input will result in completely invalid output.
        /// </remarks>
        /// <param name="count">The number of samples.</param>
        /// <param name="x">Delegate returning the x value for the ith sample.</param>
        /// <param name="y">Delegate returning the x value for the ith sample.</param>
        /// <param name="yp1">First derivative at first point (use double.NaN for natural spline)</param>
        /// <param name="ypn">First derivative at last point (use double.NaN for natural spline)</param>
        /// <returns>Coefficients for the cubic spline interpolation</returns>
        public static double[] CubicSplineFit(int count, Func<int, double> x, Func<int, double> y, double yp1, double ypn)
        {
            double[] u = new double[count - 1];
            double[] y2 = new double[count];

            // start point conditions

            y2[0] = double.IsNaN(yp1) ? 0.0 : -0.5;
            u[0] = double.IsNaN(yp1) ? 0.0 : (3.0 / (x(1) - x(0))) * ((y(1) - y(0)) / (x(1) - x(0)) - yp1);

            // tridiagonal decomposition

            for (int i = 1; i < count - 1; i++)
            {
                double sig = (x(i) - x(i - 1)) / (x(i + 1) - x(i - 1));
                double p = sig * y2[i - 1] + 2.0;

                y2[i] = (sig - 1.0) / p;
                u[i] = (y(i + 1) - y(i)) / (x(i + 1) - x(i)) - (y(i) - y(i - 1)) / (x(i) - x(i - 1));
                u[i] = (6.0 * u[i] / (x(i + 1) - x(i - 1)) - sig * u[i - 1]) / p;
            }

            // end point conditions

            double qn = double.IsNaN(ypn) ? 0.0 : 0.5;
            double un = double.IsNaN(ypn) ? 0.0 : (3.0 / (x(count - 1) - x(count - 2))) * (ypn - (y(count - 1) - y(count - 2)) / (x(count - 1) - x(count - 2)));

            y2[count - 1] = (un - qn * u[count - 2]) / (qn * y2[count - 2] + 1.0);

            // tridiagonal backsubstitution

            for (int i = count - 2; i >= 0; i--)
            {
                y2[i] = y2[i] * y2[i + 1] + u[i];
            }

            return y2;
        }

        /// <summary>
        /// Evaluates a cubic spline interpolation
        /// </summary>
        public double CubicSplineEvaluate(double x, double x1, double y1, double x2, double y2, double u1, double u2)
        {
            double h = x2 - x1;
            double a = (x2 - x) / h;
            double b = (x - x1) / h;

            return a * y1 + b * y2 + ((a * a * a - a) * u1 + (b * b * b - b) * u2) * (h * h) / 6.0;
        }






        /// <summary>
        /// Returns a 2D spline fitting of the data.
        /// </summary>
        /// <param name="count">The number of points being inputted.</param>
        /// <param name="x">Provides the X input values based on index.</param>
        /// <param name="y">Provides the Y input values based on index.</param>
        /// <param name="stiffness">The spline stiffness parameter to use.</param>
        /// <returns>The path figure collection representing the fitted spline.</returns>
        public static PathFigureCollection Spline2D(int count, Func<int, double> x, Func<int, double> y, double stiffness)
        {
            PathFigureCollection result = new PathFigureCollection();
            int currentSegmentStart = 0;
            int currentSegmentEnd = -1;
            double valueX = double.NaN;
            double valueY = double.NaN;
            for (int i = 0; i < count; i++)
            {
                valueX = x(i);
                valueY = y(i);
                if (double.IsNaN(valueX) || double.IsNaN(valueY))
                {
                    currentSegmentEnd = i - 1;
                    if (currentSegmentEnd - currentSegmentStart > 0)
                    {
                        result.Add(Numeric.Spline2D(currentSegmentStart, currentSegmentEnd, x, y, stiffness));
                    }
                    currentSegmentStart = i + 1;
                }
            }
            if (!double.IsNaN(valueX) && !double.IsNaN(valueY))
            {
                currentSegmentEnd = count - 1;
            }
            if (currentSegmentEnd - currentSegmentStart > 0)
            {
                result.Add(Numeric.Spline2D(currentSegmentStart, currentSegmentEnd, x, y, stiffness));
            }
            return result;
        }

        /// <summary>
        /// Returns a 2D spline fitting of the data.
        /// </summary>
        /// <param name="startIndex">The index to start from.</param>
        /// <param name="endIndex">The ending index.</param>
        /// <param name="x">Provides the X input values based on index.</param>
        /// <param name="y">Provides the Y input values based on index.</param>
        /// <param name="stiffness">The spline stiffness parameter to use.</param>
        /// <returns>The path figure collection representing the fitted spline.</returns>
        public static PathFigure Spline2D(int startIndex, int endIndex, Func<int, double> x, Func<int, double> y, double stiffness)
        {
            stiffness = 0.5 * MathUtil.Clamp(double.IsNaN(stiffness) ? 0.5 : stiffness, 0.0, 1.0);

            PathFigure pathFigure = new PathFigure();

            int count = endIndex - startIndex + 1;

            if (count < 2)
            {
                return pathFigure;
            }

            if (count == 2)
            {
                pathFigure.StartPoint = new Point(x(startIndex), y(startIndex));
                var newSeg = new LineSegment() { Point = new Point(x(startIndex + 1), y(startIndex + 1)) };
                pathFigure.Segments.Add(newSeg);

                return pathFigure;
            }

            PolyBezierSegment Segment = new PolyBezierSegment();

            double pix = x(startIndex);
            double piy = y(startIndex);

            double pixnext = x(startIndex + 1);
            double piynext = y(startIndex + 1);
            while (pixnext == pix && piynext == piy && 
                startIndex + 1 <= endIndex)
            {
                startIndex++;
                pixnext = x(startIndex + 1);
                piynext = y(startIndex + 1);
            }
            double tix = pixnext - pix;
            double tiy = piynext - piy;            

            double li = Math.Sqrt(tix * tix + tiy * tiy);

            for (int j = startIndex + 1; j < endIndex; ++j)
            {
                double pjx = x(j);
                double pjy = y(j);

                if (pjx == pix && pjy == piy)
                {
                    continue;
                }

                double tjx = x(j + 1) - x(j - 1);
                double tjy = y(j + 1) - y(j - 1);
                double lj = tjx * tjx + tjy * tjy;

                if (lj < 0.01)
                {
                    tjx = -(y(j + 1) - y(j));
                    tjy = x(j + 1) - x(j);
                    lj = tjx * tjx + tjy * tjy;
                }

                lj = Math.Sqrt(lj);

                double d = stiffness * Math.Sqrt((pjx - pix) * (pjx - pix) + (pjy - piy) * (pjy - piy));

                if (lj > 0.01)
                {
                    Segment.Points.Add(new Point(pix + tix * d / li, piy + tiy * d / li));
                    Segment.Points.Add(new Point(pjx - tjx * d / lj, pjy - tjy * d / lj));
                    Segment.Points.Add(new Point(pjx, pjy));

                    pix = pjx;
                    piy = pjy;
                    tix = tjx;
                    tiy = tjy;
                    li = lj;
                }
            }

            {
                int j = endIndex;

                double pjx = x(j);
                double pjy = y(j);
                double tjx = x(j) - x(j - 1);
                double tjy = y(j) - y(j - 1);
                double lj = tjx * tjx + tjy * tjy;
                double d = stiffness * Math.Sqrt((pjx - pix) * (pjx - pix) + (pjy - piy) * (pjy - piy));

                Segment.Points.Add(new Point(pix + tix * d / li, piy + tiy * d / li));
                Segment.Points.Add(new Point(pjx - tjx * d / lj, pjy - tjy * d / lj));
                Segment.Points.Add(new Point(pjx, pjy));
            }

            pathFigure.StartPoint = new Point(x(startIndex), y(startIndex));
            pathFigure.Segments.Add(Segment);

            return pathFigure;
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