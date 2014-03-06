using System;
using System.Diagnostics.CodeAnalysis;

namespace Infragistics
{
    /// <summary>
    /// Utility class for math operations.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704", Justification = "Util is a word.")]
    public static class MathUtil
    {
        /// <summary>
        /// Represents the golden mean.
        /// </summary>
        public static readonly double PHI = (1.0 + System.Math.Sqrt(5)) / 2.0;

        /// <summary>
        /// Represents the square root of 2.0
        /// </summary>
        public static readonly double SQRT2 = System.Math.Sqrt(2.0);

        /// <summary>
        /// Returns the inverse hyperbolic sine of the specified angle.
        /// </summary>
        /// <param name="angle">An angle, measured in radians</param>
        /// <returns>Inverse hyperbolic sine of the specified angle.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1704", Justification = "Asinh is the name of the function.")]
        public static double Asinh(double angle)
        {
            return System.Math.Log(angle + System.Math.Sqrt(angle * angle + 1));
        }

        /// <summary>
        /// Calculates the length of the hypotenuse of a right-angled triangle based
        /// on the lengths of two sides x and y.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Naming", "CA1704", Justification = "Hypot is the name of the function.")]
        public static double Hypot(double x, double y)
        {
            return System.Math.Sqrt(x * x + y * y);
        }

        /// <summary>
        /// Calculates the square of a x.
        /// </summary>
        /// <param name="x"></param>
        /// <returns>The square of x</returns>
        [SuppressMessage("Microsoft.Naming", "CA1704", Justification = "Sqr is the name of the function.")]
        public static double Sqr(double x)
        {
            return x * x;
        }

        /// <summary>
        /// Returns the natural logarithm of the gamma function, Γ(x).
        /// </summary>
        /// <param name="x">The value for which you want to calculate GammaLn.</param>
        /// <returns>The logarithm of the gamma function, NaN for x less than or equal to zero.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1704", Justification = "GammaLn is the name of the function.")]
        public static double GammaLn(double x)
        {
            if (x <= 0)
            {
                return double.NaN;  // throw ("bad arg in gammln");
            }

            double[] cof = new double[]
            {   
                57.1562356658629235,        -59.5979603554754912,
                14.1360979747417471,        -0.491913816097620199,
                0.339946499848118887e-4,    0.465236289270485756e-4,
                -0.983744753048795646e-4,   0.158088703224912494e-3,
                -0.210264441724104883e-3,   0.217439618115212643e-3,
                -0.164318106536763890e-3,   0.844182239838527433e-4,
                -0.261908384015814087e-4,   0.368991826595316234e-5
            };

            double y = x;
            double t = (x + 0.5) * System.Math.Log(x + 5.24218750000000000) - (x + 5.24218750000000000);
            double s = 0.999999999999997092;

            for (int j = 0; j < 14; j++)
            {
                s += cof[j] / ++y;
            }


            return t + System.Math.Log(2.5066282746310005 * s / x);
        }

        /// <summary>
        /// Returns the specified value clamped to the specified range.
        /// </summary>
        /// <param name="value">Value to clamp.</param>
        /// <param name="minimum">Range minimum.</param>
        /// <param name="maximum">Range maximum.</param>
        /// <returns>Clamped value.</returns>
        public static double Clamp(double value, double minimum, double maximum)
        {
            return System.Math.Min(maximum, System.Math.Max(minimum, value));
        }

        /// <summary>
        /// Converts the specified angle to radians.
        /// </summary>
        /// <param name="degrees">Angle in degrees.</param>
        /// <returns>Angle as radians.</returns>
        public static double Radians(double degrees) { return System.Math.PI * degrees / 180.0; }

        /// <summary>
        /// Constant used for converting degrees to radians.
        /// </summary>
        public const double DegreeAsRadian = System.Math.PI / 180.0;

        /// <summary>
        /// Converts the specified angle to degrees.
        /// </summary>
        /// <param name="radians">Angle in radians.</param>
        /// <returns>Angle as degrees.</returns>
        public static double Degrees(double radians) { return 180.0 * radians / System.Math.PI; }

        #region Perlin Noise Function

        /// <summary>
        /// Returns the Perlin noise value at the specified location.
        /// </summary>
        /// <param name="x">x location in noise space</param>
        /// <param name="y">y location in noise space</param>
        /// <param name="z">z location in noise space</param>
        /// <returns>Noise value.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1704", Justification = "x, y and z are vector component names.")]
        public static double Noise(double x, double y, double z)
        {
            int X = (int)System.Math.Floor(x) & 0xff;
            int Y = (int)System.Math.Floor(y) & 0xff;
            int Z = (int)System.Math.Floor(z) & 0xff;

            x -= System.Math.Floor(x);
            y -= System.Math.Floor(y);
            z -= System.Math.Floor(z);

            double u = fade(x);
            double v = fade(y);
            double w = fade(z);

            int A = basis[X]+Y;
            int AA = basis[A]+Z;
            int AB = basis[A+1]+Z;
            int B = basis[X+1]+Y;
            int BA = basis[B]+Z;
            int BB = basis[B+1]+Z;

            return lerp(w,  lerp(v, lerp(u, grad(basis[AA  ], x  , y  , z   ),
                                            grad(basis[BA  ], x-1, y  , z   )),
                                    lerp(u, grad(basis[AB  ], x  , y-1, z   ),
                                            grad(basis[BB  ], x-1, y-1, z   ))),
                            lerp(v, lerp(u, grad(basis[AA+1], x  , y  , z-1 ),
                                            grad(basis[BA+1], x-1, y  , z-1 )),
                                    lerp(u, grad(basis[AB+1], x  , y-1, z-1 ),
                                            grad(basis[BB+1], x-1, y-1, z-1 ))));
        }

        private static double fade(double t)
        {
            return t * t * t * (t * (t * 6 - 15) + 10);
        }
        private static double lerp(double t, double a, double b)
        {
            return a + t * (b - a);
        }
        private static double grad(int hash, double x, double y, double z)
        {
            int h = hash & 15;
            double u = h<8 ? x : y;
            double v = h<4 ? y : h==12||h==14 ? x : z;

            return ((h&1) == 0 ? u : -u) + ((h&2) == 0 ? v : -v);
        }

        static int[] basis =
        {
            151, 160, 137, 91, 90, 15, 131, 13, 201, 95, 96, 53, 194, 233, 7, 225, 
            140, 36, 103, 30, 69, 142, 8, 99, 37, 240, 21, 10, 23, 190,  6, 148, 
            247, 120, 234, 75, 0, 26, 197, 62, 94, 252, 219, 203, 117, 35, 11, 32, 
            57, 177, 33, 88, 237, 149, 56, 87, 174, 20, 125, 136, 171, 168,  68, 175, 
            74, 165, 71, 134, 139, 48, 27, 166, 77, 146, 158, 231, 83, 111, 229, 122, 
            60, 211, 133, 230, 220, 105, 92, 41, 55, 46, 245, 40, 244, 102, 143, 54, 
            65, 25, 63, 161, 1, 216, 80, 73, 209, 76, 132, 187, 208, 89, 18, 169, 
            200, 196, 135, 130, 116, 188, 159, 86, 164, 100, 109, 198, 173, 186, 3, 64, 
            52, 217, 226, 250, 124, 123, 5, 202, 38, 147, 118, 126, 255, 82, 85, 212, 
            207, 206, 59, 227, 47, 16, 58, 17, 182, 189, 28, 42, 223, 183, 170, 213, 
            119, 248, 152, 2, 44, 154, 163, 70, 221, 153, 101, 155, 167, 43, 172, 9, 
            129, 22, 39, 253, 19, 98, 108, 110, 79, 113, 224, 232, 178, 185, 112, 104, 
            218, 246, 97, 228, 251, 34, 242, 193, 238, 210, 144, 12, 191, 179, 162, 241, 
            81, 51, 145, 235, 249, 14, 239, 107, 49, 192, 214, 31, 181, 199, 106, 157, 
            184, 84, 204, 176, 115, 121, 50, 45, 127, 4, 150, 254, 138, 236, 205, 93, 
            222, 114, 67, 29, 24, 72, 243, 141, 128, 195, 78, 66, 215, 61, 156, 180, 

            151, 160, 137, 91, 90, 15, 131, 13, 201, 95, 96, 53, 194, 233, 7, 225, 
            140, 36, 103, 30, 69, 142, 8, 99, 37, 240, 21, 10, 23, 190,  6, 148, 
            247, 120, 234, 75, 0, 26, 197, 62, 94, 252, 219, 203, 117, 35, 11, 32, 
            57, 177, 33, 88, 237, 149, 56, 87, 174, 20, 125, 136, 171, 168,  68, 175, 
            74, 165, 71, 134, 139, 48, 27, 166, 77, 146, 158, 231, 83, 111, 229, 122, 
            60, 211, 133, 230, 220, 105, 92, 41, 55, 46, 245, 40, 244, 102, 143, 54, 
            65, 25, 63, 161, 1, 216, 80, 73, 209, 76, 132, 187, 208, 89, 18, 169, 
            200, 196, 135, 130, 116, 188, 159, 86, 164, 100, 109, 198, 173, 186, 3, 64, 
            52, 217, 226, 250, 124, 123, 5, 202, 38, 147, 118, 126, 255, 82, 85, 212, 
            207, 206, 59, 227, 47, 16, 58, 17, 182, 189, 28, 42, 223, 183, 170, 213, 
            119, 248, 152, 2, 44, 154, 163, 70, 221, 153, 101, 155, 167, 43, 172, 9, 
            129, 22, 39, 253, 19, 98, 108, 110, 79, 113, 224, 232, 178, 185, 112, 104, 
            218, 246, 97, 228, 251, 34, 242, 193, 238, 210, 144, 12, 191, 179, 162, 241, 
            81, 51, 145, 235, 249, 14, 239, 107, 49, 192, 214, 31, 181, 199, 106, 157, 
            184, 84, 204, 176, 115, 121, 50, 45, 127, 4, 150, 254, 138, 236, 205, 93, 
            222, 114, 67, 29, 24, 72, 243, 141, 128, 195, 78, 66, 215, 61, 156, 180
        };
        #endregion

        #region Nice Rounding
        /// <summary>
        /// Returns a nicely rounded value less than or equal to the specified value
        /// </summary>
        /// <param name="value">Value to round.</param>
        /// <returns></returns>
        public static double NiceFloor(double value)
        {
            if (value == 0.0) { return 0.0; }
            if (value < 0.0) { return -NiceCeiling(-value); }

            int expv = (int)System.Math.Floor(System.Math.Log10(value));
            double f = value / expt(10.0, expv);
            double nf = f < 2.0 ? 1.0 : (f < 5.0 ? 2.0 : (f < 10.0 ? 5.0 : 10.0));
            return nf * expt(10.0, expv);
        }

        /// <summary>
        /// Rounds a decimal value to the nearest nice number.
        /// </summary>
        /// <param name="value">Value to round.</param>
        /// <returns></returns>
        public static double NiceRound(double value)
        {
            if (value == 0.0) { return 0.0; }
            if (value < 0.0) { return -NiceRound(-value); }

            int expv = (int)System.Math.Floor(System.Math.Log10(value));
            double f = value / expt(10.0, expv);
            double nf = f < 1.0f ? 1.0 : (f < 3.0 ? 2.0 : (f < 7.0 ? 5.0 : 10.0));

            return nf * expt(10.0, expv);
        }

        /// <summary>
        /// Returns a nicely rounded value greater than or equal to the specified value
        /// </summary>
        /// <param name="value">Value to round.</param>
        /// <returns></returns>
        public static double NiceCeiling(double value)
        {
            if (value == 0.0) { return 0.0; }
            if (value < 0.0) { return -NiceFloor(-value); }

            int expv = (int)System.Math.Floor(System.Math.Log10(value));
            double f = value / expt(10.0, expv);
            double nf = f <= 1.0 ? 1.0 : (f <= 2.0 ? 2.0 : (f <= 5.0 ? 5.0 : 10.0));

            return nf * expt(10.0, expv);
        }

        private static double expt(double a, int n)
        {
            double x = 1.0;

            for (; n > 0; --n)
            {
                x *= a;
            }

            for (; n < 0; ++n)
            {
                x /= a;
            }

            return x;
        }
        #endregion

        /// <summary>
        /// Returns the lowest of the given numeric parameters.
        /// </summary>
        /// <param name="a">The numeric parameters from which to return the minimum value.</param>
        /// <returns>The lowest of the given numeric parameters.</returns>
        public static double Min(params double[] a)
        {
            double min = a[0];

            for (int i = 1; i < a.Length; ++i)
            {
                min = Math.Min(min, a[i]);
            }

            return min;
        }
        /// <summary>
        /// Returns the highest of the given numeric parameters.
        /// </summary>
        /// <param name="a">The numeric parameters from which to return the maximum value.</param>
        /// <returns>The highest of the given numeric parameters.</returns>
        public static double Max(params double[] a)
        {
            double max = a[0];

            for (int i = 1; i < a.Length; ++i)
            {
                max = Math.Max(max, a[i]);
            }

            return max;
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