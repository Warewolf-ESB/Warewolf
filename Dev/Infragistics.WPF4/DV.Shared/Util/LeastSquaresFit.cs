using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Infragistics.Controls.Charts.Util
{
    /// <summary>
    /// Methods for linear least squares fitting.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class LeastSquaresFit : Numeric
    {
        /// <summary>
        /// Runs the built-in test suite.
        /// </summary>
        /// <returns>true if all tests pass</returns>
        public static bool Test()
        {
            return LinearTest() &&
                    LogarithmicTest() &&
                    ExponentialTest() &&
                    PowerLawTest() &&
                    QuadraticTest() &&
                    CubicTest() &&
                    QuarticTest() &&
                    QuinticTest();
        }

        private LeastSquaresFit() {  }

        #region Linear
        /// <summary>
        /// Creates coefficients for the linear least squares fit y=A+Bx
        /// </summary>
        /// <param name="n">Number of items in the series.</param>
        /// <param name="x">Delegate which returns the nth x value.</param>
        /// <param name="y">Delegate which returns the nth y value.</param>
        /// <remarks>
        /// If either of the x or y for a given point are NaN, the point is not taken
        /// into account for the fitting operation. Infinite values are valid, but
        /// are likely to result in numerical instability.
        /// <para>
        /// Weisstein, Eric W. "Least Squares Fitting." From MathWorld--A Wolfram Web Resource.
        /// http://mathworld.wolfram.com/LeastSquaresFitting.html 
        /// </para>
        /// </remarks>
        /// <returns>A, B as an array of doubles or null if no solution.</returns>
        public static double[] LinearFit(int n, Func<int, double> x, Func<int, double> y)
        {
            double s0 = 0.0;
            double s1 = 0.0;
            double s2 = 0.0;
            double s3 = 0.0;
            int N = 0;

            for (int i = 0; i < n; ++i)
            {
                double xi = x(i);
                double yi = y(i);

                if (!double.IsNaN(xi) && !double.IsNaN(yi))
                {
                    s0 += yi;
                    s1 += xi * xi;
                    s2 += xi;
                    s3 += xi * yi;

                    ++N;
                }
            }

            if (N < 2)
            {
                return null;
            }

            double A = (s0 * s1 - s2 * s3) / (N * s1 - s2 * s2);
            double B = (N * s3 - s2 * s0) / (N * s1 - s2 * s2);

            return new double[] { A, B };
        }

        /// <summary>
        /// Evaluate the linear function y=a[0]+a[1]*x
        /// </summary>
        /// <param name="a">function coefficients</param>
        /// <param name="x">function evaluation point</param>
        /// <returns>Evaluated function, or NaN if the function cannot be evaluated</returns>
        public static double LinearEvaluate(double[] a, double x)
        {
            if (a.Length != 2)
            {
                return double.NaN;
            }

            return a[0] + a[1] * x;
        }

        /// <summary>
        /// Tests linear fitting.
        /// </summary>
        /// <returns>true if tests pass.</returns>
        public static bool LinearTest()
        {
            Random random = new Random();
            double[] coeffs = new double[2];

            for (int i = 0; i < coeffs.Length; ++i)
            {
                coeffs[i] = 10.0 * random.NextDouble();
            }

            List<double> x = new List<double>();
            List<double> y = new List<double>();

            for (int i = -100; i < 100; ++i)
            {
                double X = i;
                double Y = LinearEvaluate(coeffs, X);

                if (!double.IsNaN(Y))
                {
                    x.Add(X);
                    y.Add(Y);
                }
            }

            double[] fit = LinearFit(x.Count, delegate(int i) { return x[i]; }, delegate(int i) { return y[i]; });

            for (int i = 0; i < coeffs.Length; ++i)
            {
                if (Math.Abs(coeffs[i] - fit[i]) > 0.0001)
                {
                }
            }

            return true;
        }

        #endregion

        #region Logarithmic
        /// <summary>
        /// Creates coefficients for the logarithmic least squares fit y=A+Blnx
        /// </summary>
        /// <param name="n">Number of items in the series.</param>
        /// <param name="x">Delegate which returns the nth x value.</param>
        /// <param name="y">Delegate which returns the nth y value.</param>
        /// <remarks>
        /// If either of the x or y for a given point are NaN or the x is not positive, the point is not taken
        /// into account for the fitting operation. Infinite values are valid, but
        /// are likely to result in numerical instability.
        /// <para>
        /// Weisstein, Eric W. "Least Squares Fitting--Logarithmic." From MathWorld--A Wolfram Web Resource.
        /// http://mathworld.wolfram.com/LeastSquaresFittingLogarithmic.html 
        /// </para>
        /// </remarks>
        /// <returns>A, B as an array of doubles or null if no solution.</returns>
        public static double[] LogarithmicFit(int n, Func<int, double> x, Func<int, double> y)
        {
            double s0 = 0.0;
            double s1 = 0.0;
            double s2 = 0.0;
            double s3 = 0.0;

            int N = 0;

            for (int i = 0; i < n; ++i)
            {
                double xi = x(i);
                double yi = y(i);

                if (!double.IsNaN(xi) && !double.IsNaN(yi) && xi > 0.0)
                {
                    double lnxi = Math.Log(xi);

                    s0 += yi * lnxi;
                    s1 += yi;
                    s2 += lnxi;
                    s3 += lnxi * lnxi;

                    ++N;
                }
            }

            if (N < 2)
            {
                return null;
            }

            double B = (N * s0 - s1 * s2) / (N * s3 - s2 * s2);
            double A = (s1 - B * s2) / N;

            return new double[] { A, B };
        }

        /// <summary>
        /// Evaluate the linear function y=a[0]+a[1]*ln(x)
        /// </summary>
        /// <param name="a">function coefficients</param>
        /// <param name="x">function evaluation point</param>
        /// <returns>Evaluated function, or NaN if the function cannot be evaluated</returns>
        public static double LogarithmicEvaluate(double[] a, double x)
        {
            if (a.Length != 2 || x < 0.0 || double.IsInfinity(x) || double.IsNaN(x))
            {
                return double.NaN;
            }

            return a[0] + a[1] * Math.Log(x);
        }

        /// <summary>
        /// Tests logarithmic fitting.
        /// </summary>
        /// <returns>true if tests pass.</returns>
        public static bool LogarithmicTest()
        {
            Random random = new Random();
            double[] coeffs = new double[2];

            for (int i = 0; i < coeffs.Length; ++i)
            {
                coeffs[i] = 10.0 * random.NextDouble();
            }

            List<double> x = new List<double>();
            List<double> y = new List<double>();

            for (int i = 1; i < 100; ++i)
            {
                double X = i;
                double Y = LogarithmicEvaluate(coeffs, X);

                if (!double.IsNaN(Y))
                {
                    x.Add(X);
                    y.Add(Y);
                }
            }

            double[] fit = LogarithmicFit(x.Count, delegate(int i) { return x[i]; }, delegate(int i) { return y[i]; });

            for (int i = 0; i < coeffs.Length; ++i)
            {
                if (Math.Abs(coeffs[i] - fit[i]) > 0.0001)
                {
                }
            }

            return true;
        }

        #endregion

        #region Exponential
        /// <summary>
        /// Creates coefficients for the exponential least squares fit y=Ae^(Bx)
        /// </summary>
        /// <param name="n">Number of items in the series.</param>
        /// <param name="x">Delegate which returns the nth x value.</param>
        /// <param name="y">Delegate which returns the nth y value.</param>
        /// <remarks>
        /// If either of the x or y for a given point are NaN or the y is not positive, the point is not taken
        /// into account for the fitting operation. Infinite values are valid, but
        /// are likely to result in numerical instability.
        /// <para>
        /// Weisstein, Eric W. "Least Squares Fitting--Exponential." From MathWorld--A Wolfram Web Resource.
        /// http://mathworld.wolfram.com/LeastSquaresFittingExponential.html 
        /// </para>
        /// </remarks>
        /// <returns>A, B as an array of doubles or null if no solution.</returns>
        public static double[] ExponentialFit(int n, Func<int, double> x, Func<int, double> y)
        {
            double s0 = 0.0;
            double s1 = 0.0;
            double s2 = 0.0;
            double s3 = 0.0;
            double s4 = 0.0;

            int N = 0;

            for (int i = 0; i < n; ++i)
            {
                double xi = x(i);
                double yi = y(i);

                if (!double.IsNaN(xi) && !double.IsNaN(yi) && yi > 0.0)
                {
                    double lnyi = Math.Log(yi);

                    s0 += xi * xi * yi;
                    s1 += yi * lnyi;
                    s2 += xi * yi;
                    s3 += xi * yi * lnyi;
                    s4 += yi;

                    ++N;
                }
            }

            if (N < 2)
            {
                return null;
            }

            double a = (s0 * s1 - s2 * s3) / (s4 * s0 - s2 * s2);
            double B = (s4 * s3 - s2 * s1) / (s4 * s0 - s2 * s2);

            return new double[] { Math.Exp(a), B };
        }

        /// <summary>
        /// Evaluate the function y=a[0]*e^(a[1]*x)
        /// </summary>
        /// <param name="a">function coefficients</param>
        /// <param name="x">function evaluation point</param>
        /// <returns>Evaluated function, or NaN if the function cannot be evaluated</returns>
        public static double ExponentialEvaluate(double[] a, double x)
        {
            if (a.Length != 2 || x < 0.0 || double.IsInfinity(x) || double.IsNaN(x))
            {
                return double.NaN;
            }

            return a[0] * Math.Exp(a[1] * x);
        }

        /// <summary>
        /// Tests exponential fitting.
        /// </summary>
        /// <returns>true if tests pass.</returns>
        public static bool ExponentialTest()
        {
            Random random = new Random();
            double[] coeffs = new double[2];

            for (int i = 0; i < coeffs.Length; ++i)
            {
                coeffs[i] = 2.0 * random.NextDouble();
            }

            List<double> x = new List<double>();
            List<double> y = new List<double>();

            for (int i = 1; i < 100; ++i)
            {
                double X = i;
                double Y = ExponentialEvaluate(coeffs, X);

                if (!double.IsNaN(Y))
                {
                    x.Add(X);
                    y.Add(Y);
                }
            }

            double[] fit = ExponentialFit(x.Count, delegate(int i) { return x[i]; }, delegate(int i) { return y[i]; });

            for (int i = 0; i < coeffs.Length; ++i)
            {
                if (Math.Abs(coeffs[i] - fit[i]) > 0.0001)
                {
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region PowerLaw
        /// <summary>
        /// Creates coefficients for the power law least squares fit y=A(x^B)
        /// </summary>
        /// <param name="n">Number of items in the series.</param>
        /// <param name="x">Delegate which returns the nth x value.</param>
        /// <param name="y">Delegate which returns the nth y value.</param>
        /// <remarks>
        /// If either of the x or y for a given point are NaN not positive, the point is not taken
        /// into account for the fitting operation. Infinite values are valid, but
        /// are likely to result in numerical instability.
        /// <para>
        /// Weisstein, Eric W. "Least Squares Fitting--Power Law." From MathWorld--A Wolfram Web Resource.
        /// http://mathworld.wolfram.com/LeastSquaresFittingPowerLaw.html 
        /// </para>
        /// </remarks>
        /// <returns>A, B as an array of doubles or null if no solution.</returns>
        public static double[] PowerLawFit(int n, Func<int, double> x, Func<int, double> y)
        {
            double s0 = 0.0;
            double s1 = 0.0;
            double s2 = 0.0;
            double s3 = 0.0;

            int N = 0;

            for (int i = 0; i < n; ++i)
            {
                double xi = x(i);
                double yi = y(i);

                if (!double.IsNaN(xi) && !double.IsNaN(yi) && xi > 0.0 && yi > 0.0)
                {
                    double lnxi = Math.Log(x(i));
                    double lnyi = Math.Log(y(i));

                    s0 += lnxi * lnyi;
                    s1 += lnxi;
                    s2 += lnyi;
                    s3 += lnxi * lnxi;

                    ++N;
                }
            }

            if (N < 2)
            {
                return null;
            }

            double B = (N * s0 - s1 * s2) / (N * s3 - s1 * s1);
            double A = Math.Exp((s2 - B * s1) / N);

            return new double[] { A, B };
        }

        /// <summary>
        /// Evaluate the function y=a[0]*(a[1]^x)
        /// </summary>
        /// <param name="a">function coefficients</param>
        /// <param name="x">function evaluation point</param>
        /// <returns>Evaluated function, or NaN if the function cannot be evaluated</returns>
        public static double PowerLawEvaluate(double[] a, double x)
        {
            if (a.Length != 2 || x < 0.0 || double.IsInfinity(x) || double.IsNaN(x))
            {
                return double.NaN;
            }

            return a[0] * Math.Pow(x, a[1]);
        }

        /// <summary>
        /// Tests power law fitting.
        /// </summary>
        /// <returns>true if tests pass.</returns>
        public static bool PowerLawTest()
        {
            Random random = new Random();
            double[] coeffs = new double[2];

            for (int i = 0; i < coeffs.Length; ++i)
            {
                coeffs[i] = 10.0 * random.NextDouble();
            }

            List<double> x = new List<double>();
            List<double> y = new List<double>();

            for (int i = -100; i < 100; ++i)
            {
                x.Add(i);
                y.Add(PowerLawEvaluate(coeffs, i));
            }

            double[] fit = PowerLawFit(x.Count, delegate(int i) { return x[i]; }, delegate(int i) { return y[i]; });

            for (int i = 0; i < coeffs.Length; ++i)
            {
                if (Math.Abs(coeffs[i] - fit[i]) > 0.0001)
                {
                    return false;
                }
            }

            return true;
        }
        #endregion

        #region Quadratic
        /// <summary>
        /// Creates coefficients for the polynomial least squares fit y=a0+a1x+a2x^2
        /// </summary>
        /// <param name="n">Number of items in the series.</param>
        /// <param name="x">Delegate which returns the nth x value.</param>
        /// <param name="y">Delegate which returns the nth y value.</param>
        /// <remarks>
        /// If either of the x or y for a given point are NaN, the point is not taken
        /// into account for the fitting operation. Infinite values are valid, but
        /// are likely to result in numerical instability.
        /// <para>
        /// Weisstein, Eric W. "Least Squares Fitting--Polynomial." From MathWorld--A Wolfram Web Resource.
        /// http://mathworld.wolfram.com/LeastSquaresFittingPolynomial.html
        /// </para>
        /// </remarks>
        /// <returns>Polynomial coefficients a0, a1, .. ak as an array of doubles or null if no solution.</returns>
        public static double[] QuadraticFit(int n, Func<int, double> x, Func<int, double> y)
        {
            return PolynomialFit(n, 2, x, y);
        }

        /// <summary>
        /// Evaluate the function y=a[0]+a[1]*x+a[2]*x^2
        /// </summary>
        /// <param name="a">function coefficients</param>
        /// <param name="x">function evaluation point</param>
        /// <returns>Evaluated function, or NaN if the function cannot be evaluated</returns>
        public static double QuadraticEvaluate(double[] a, double x)
        {
            return PolynomialEvaluate(a, x);
        }

        /// <summary>
        /// Tests quadratic fitting.
        /// </summary>
        /// <returns>true if tests pass.</returns>
        public static bool QuadraticTest()
        {
            return PolynomialTest(2);
        }
        #endregion

        #region Cubic
        /// <summary>
        /// Creates coefficients for the polynomial least squares fit y=a0+a1x+a2x^2+a3x^3
        /// </summary>
        /// <param name="n">Number of items in the series.</param>
        /// <param name="x">Delegate which returns the nth x value.</param>
        /// <param name="y">Delegate which returns the nth y value.</param>
        /// <remarks>
        /// If either of the x or y for a given point are NaN, the point is not taken
        /// into account for the fitting operation. Infinite values are valid, but
        /// are likely to result in numerical instability.
        /// <para>
        /// Weisstein, Eric W. "Least Squares Fitting--Polynomial." From MathWorld--A Wolfram Web Resource.
        /// http://mathworld.wolfram.com/LeastSquaresFittingPolynomial.html
        /// </para>
        /// </remarks>
        /// <returns>Polynomial coefficients a0, a1, .. ak as an array of doubles or null if no solution.</returns>
        public static double[] CubicFit(int n, Func<int, double> x, Func<int, double> y)
        {
            return PolynomialFit(n, 3, x, y);
        }

        /// <summary>
        /// Evaluate the function y=a[0]+a[1]*x+a[2]*x^2+a3x^3
        /// </summary>
        /// <param name="a">function coefficients</param>
        /// <param name="x">function evaluation point</param>
        /// <returns>Evaluated function, or NaN if the function cannot be evaluated</returns>
        public static double CubicEvaluate(double[] a, double x)
        {
            return PolynomialEvaluate(a, x);
        }

        /// <summary>
        /// Tests cubic fitting.
        /// </summary>
        /// <returns>true if tests pass.</returns>
        public static bool CubicTest()
        {
            return PolynomialTest(3);
        }
        #endregion

        #region Quartic
        /// <summary>
        /// Creates coefficients for the polynomial least squares fit y=a0+a1x+a2x^2+a3x^3+a4x^4
        /// </summary>
        /// <param name="n">Number of items in the series.</param>
        /// <param name="x">Delegate which returns the nth x value.</param>
        /// <param name="y">Delegate which returns the nth y value.</param>
        /// <remarks>
        /// If either of the x or y for a given point are NaN, the point is not taken
        /// into account for the fitting operation. Infinite values are valid, but
        /// are likely to result in numerical instability.
        /// <para>
        /// Weisstein, Eric W. "Least Squares Fitting--Polynomial." From MathWorld--A Wolfram Web Resource.
        /// http://mathworld.wolfram.com/LeastSquaresFittingPolynomial.html
        /// </para>
        /// </remarks>
        /// <returns>Polynomial coefficients a0, a1, .. ak as an array of doubles or null if no solution.</returns>
        public static double[] QuarticFit(int n, Func<int, double> x, Func<int, double> y)
        {
            return PolynomialFit(n, 4, x, y);
        }

        /// <summary>
        /// Evaluate the function y=a[0]+a[1]*x+a[2]*x^2+a3x^3
        /// </summary>
        /// <param name="a">function coefficients</param>
        /// <param name="x">function evaluation point</param>
        /// <returns>Evaluated function, or NaN if the function cannot be evaluated</returns>
        public static double QuarticEvaluate(double[] a, double x)
        {
            return PolynomialEvaluate(a, x);
        }

        /// <summary>
        /// Tests quartic fitting.
        /// </summary>
        /// <returns>true if tests pass.</returns>
        public static bool QuarticTest()
        {
            return PolynomialTest(4);
        }
        #endregion

        #region Quintic
        /// <summary>
        /// Creates coefficients for the polynomial least squares fit y=a0+a1x+a2x^2+a3x^3+a4x^4+a5x^5
        /// </summary>
        /// <param name="n">Number of items in the series.</param>
        /// <param name="x">Delegate which returns the nth x value.</param>
        /// <param name="y">Delegate which returns the nth y value.</param>
        /// <remarks>
        /// If either of the x or y for a given point are NaN, the point is not taken
        /// into account for the fitting operation. Infinite values are valid, but
        /// are likely to result in numerical instability.
        /// <para>
        /// Weisstein, Eric W. "Least Squares Fitting--Polynomial." From MathWorld--A Wolfram Web Resource.
        /// http://mathworld.wolfram.com/LeastSquaresFittingPolynomial.html
        /// </para>
        /// </remarks>
        /// <returns>Polynomial coefficients a0, a1, .. ak as an array of doubles or null if no solution.</returns>
        public static double[] QuinticFit(int n, Func<int, double> x, Func<int, double> y)
        {
            return PolynomialFit(n, 5, x, y);
        }

        /// <summary>
        /// Evaluate the linear function y=a[0]+a[1]*x+a[2]*x^2+a3x^3
        /// </summary>
        /// <param name="a">function coefficients</param>
        /// <param name="x">function evaluation point</param>
        /// <returns>Evaluated function, or NaN if the function cannot be evaluated</returns>
        public static double QuinticEvaluate(double[] a, double x)
        {
            return PolynomialEvaluate(a, x);
        }

        /// <summary>
        /// Tests quintic fitting.
        /// </summary>
        /// <returns>true if tests pass.</returns>
        public static bool QuinticTest()
        {
            return PolynomialTest(5);
        }
        #endregion

        #region Polynomial
        /// <summary>
        /// Creates coefficients for the polynomial least squares fit y=a0+a1x+a2x^2..
        /// </summary>
        /// <param name="n">Number of items in the series.</param>
        /// <param name="k">Polynomial order.</param>
        /// <param name="x">Delegate which returns the nth x value.</param>
        /// <param name="y">Delegate which returns the nth y value.</param>
        /// <remarks>
        /// If either of the x or y for a given point are NaN, the point is not taken
        /// into account for the fitting operation. Infinite values are valid, but
        /// are likely to result in significant numerical instability.
        /// <para>
        /// Weisstein, Eric W. "Least Squares Fitting--Polynomial." From MathWorld--A Wolfram Web Resource.
        /// http://mathworld.wolfram.com/LeastSquaresFittingPolynomial.html
        /// </para>
        /// </remarks>
        /// <returns>Polynomial coefficients a0, a1, .. ak as an array of doubles or null if no solution.</returns>
        public static double[] PolynomialFit(int n, int k, Func<int, double> x, Func<int, double> y)
        {
//#if !TINYCLR
            double[] ps = new double[1 + 2 * k];







            double[,] A = new double[k + 1, k + 1];
            double[] B = new double[k + 1];







            int N = 0;

            for (int i = 0; i < n; ++i)
            {
                double s = 1.0;
                double xi = x(i);

                if (!double.IsNaN(xi) && !double.IsNaN(y(i)))
                {
                    for (int p = 0; p < ps.Length; ++p)
                    {
                        ps[p] += s;
                        s *= xi;

                        ++N;
                    }
                }
            }

            if (N < k)
            {
                return null;
            }

            for (int i = 0; i <= k; ++i)
            {
                for (int j = 0; j <= k; ++j)
                {
                    A[i, j] = ps[i + j];
                }
            }

            for (int i = 0; i < n; ++i)
            {
                double xi = x(i);
                double yi = y(i);

                if (!double.IsNaN(xi) &&!double.IsNaN(yi))
                {
                    for (int j = 0; j <= k; ++j)
                    {
                        B[j] += (Math.Pow(xi, j) * yi);
                    }
                }
            }

            return Numeric.Solve(A, B) ? B : null;
//#else
//            return null;
//#endif
        }

        /// <summary>
        /// Evaluate the function y=a[0]+a[1]*x+a[2]*x^2+ .. +a[n-1]*x^[n-1]
        /// </summary>
        /// <param name="a">function coefficients</param>
        /// <param name="x">function evaluation point</param>
        /// <returns>Evaluated function, or NaN if the function cannot be evaluated</returns>
        public static double PolynomialEvaluate(double[] a, double x)
        {
            if (a.Length < 1 || double.IsInfinity(x) || double.IsNaN(x))
            {
                return double.NaN;
            }

            double y = 0.0;

            for (int i = 0; i < a.Length; ++i)
            {
                y += a[i] * Math.Pow(x, i);
            }

            return y;
        }
        /// <summary>
        /// Tests polynomial fitting.
        /// </summary>
        /// <param name="k">Polynomial order to test.</param>
        /// <returns>true if tests pass.</returns>
        public static bool PolynomialTest(int k)
        {
            Random random = new Random();

            double[] coeffs = new double[k + 1];

            // create a polynomial

            for (int i = 0; i < coeffs.Length; ++i)
            {
                coeffs[i] = 2.0 * random.NextDouble();
            }

            // evaluate polynomial

            List<double> x = new List<double>();
            List<double> y = new List<double>();

            for (int i = -100; i < 100; ++i)
            {
                double X = i;
                double Y = PolynomialEvaluate(coeffs, X);

                if (!double.IsNaN(Y))
                {
                    x.Add(X);
                    y.Add(Y);
                }
            }

            // fit the polynomial

            double[] fit = PolynomialFit(x.Count, k, delegate(int i) { return x[i]; }, delegate(int i) { return y[i]; });

            // compare coefficients

            for (int i = 0; i < k; ++i)
            {
                if (Math.Abs(coeffs[i] - fit[i]) > 0.0001)
                {
                    return false;
                }
            }

            return true;
        }

        #endregion
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