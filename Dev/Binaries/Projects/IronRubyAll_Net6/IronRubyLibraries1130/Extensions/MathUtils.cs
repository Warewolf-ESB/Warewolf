using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Microsoft.Scripting.Utils;

namespace Microsoft.Scripting.Math.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Numerics;
    using System.Text;
    using Microsoft.Scripting.Utils;

    public static class MathUtils
    {
        private static readonly double[] _RoundPowersOfTens = new double[16]
        {
        1.0, 10.0, 100.0, 1000.0, 10000.0, 100000.0, 1000000.0, 10000000.0, 100000000.0, 1000000000.0,
        10000000000.0, 100000000000.0, 1000000000000.0, 10000000000000.0, 100000000000000.0, 1E+15
        };

        private static readonly double[] ErfNumerCoeffs = new double[5] { 3209.3775891384694, 377.485237685302, 113.86415415105016, 3.1611237438705655, 0.18577770618460315 };

        private static readonly double[] ErfDenomCoeffs = new double[5] { 2844.2368334391708, 1282.6165260773723, 244.02463793444417, 23.601290952344122, 1.0 };

        private static readonly double[] ErfcNumerCoeffs = new double[8] { 300.45926102016159, 451.91895371187292, 339.32081673434368, 152.98928504694041, 43.162227222056735, 7.2117582508830935, 0.564195517478974, -1.3686485738271671E-07 };

        private static readonly double[] ErfcDenomCoeffs = new double[8] { 300.45926095698331, 790.95092532789806, 931.35409485060961, 638.98026446563119, 277.58544474398764, 77.00015293522948, 12.782727319629423, 1.0 };

        private static readonly double[] GammaNumerCoeffs = new double[13]
        {
        44012138428004.609, 41590453358593.2, 18013842787117.996, 4728736263475.3887, 837910083628.40466, 105583707273.42993, 9701363618.4949989, 654914397.54820526, 32238322.942133565, 1128514.2194970914,
        26665.793784598591, 381.88012486329268, 2.5066282746310007
        };

        private static readonly double[] GammaDenomCoeffs = new double[13]
        {
        0.0, 39916800.0, 120543840.0, 150917976.0, 105258076.0, 45995730.0, 13339535.0, 2637558.0, 357423.0, 32670.0,
        1925.0, 66.0, 1.0
        };

        private static readonly uint[] maxCharsPerDigit = new uint[37]
        {
        0u, 0u, 31u, 20u, 15u, 13u, 12u, 11u, 10u, 10u,
        9u, 9u, 8u, 8u, 8u, 8u, 7u, 7u, 7u, 7u,
        7u, 7u, 7u, 7u, 6u, 6u, 6u, 6u, 6u, 6u,
        6u, 6u, 6u, 6u, 6u, 6u, 6u
        };

        private static readonly uint[] groupRadixValues = new uint[37]
        {
        0u, 0u, 2147483648u, 3486784401u, 1073741824u, 1220703125u, 2176782336u, 1977326743u, 1073741824u, 3486784401u,
        1000000000u, 2357947691u, 429981696u, 815730721u, 1475789056u, 2562890625u, 268435456u, 410338673u, 612220032u, 893871739u,
        1280000000u, 1801088541u, 2494357888u, 3404825447u, 191102976u, 244140625u, 308915776u, 387420489u, 481890304u, 594823321u,
        729000000u, 887503681u, 1073741824u, 1291467969u, 1544804416u, 1838265625u, 2176782336u
        };

        private const int BitsPerDigit = 32;

        /// <summary>
        /// Calculates the quotient of two 32-bit signed integers rounded towards negative infinity.
        /// </summary>
        /// <param name="x">Dividend.</param>
        /// <param name="y">Divisor.</param>
        /// <returns>The quotient of the specified numbers rounded towards negative infinity, or <code>(int)Floor((double)x/(double)y)</code>.</returns>
        /// <exception cref="T:System.DivideByZeroException"><paramref name="y" /> is 0.</exception>
        /// <remarks>The caller must check for overflow (x = Int32.MinValue, y = -1)</remarks>
        public static int FloorDivideUnchecked(int x, int y)
        {
            int num = x / y;
            if (x >= 0)
            {
                if (y > 0)
                {
                    return num;
                }
                if (x % y == 0)
                {
                    return num;
                }
                return num - 1;
            }
            if (y > 0)
            {
                if (x % y == 0)
                {
                    return num;
                }
                return num - 1;
            }
            return num;
        }

        /// <summary>
        /// Calculates the quotient of two 64-bit signed integers rounded towards negative infinity.
        /// </summary>
        /// <param name="x">Dividend.</param>
        /// <param name="y">Divisor.</param>
        /// <returns>The quotient of the specified numbers rounded towards negative infinity, or <code>(int)Floor((double)x/(double)y)</code>.</returns>
        /// <exception cref="T:System.DivideByZeroException"><paramref name="y" /> is 0.</exception>
        /// <remarks>The caller must check for overflow (x = Int64.MinValue, y = -1)</remarks>
        public static long FloorDivideUnchecked(long x, long y)
        {
            long num = x / y;
            if (x >= 0)
            {
                if (y > 0)
                {
                    return num;
                }
                if (x % y == 0L)
                {
                    return num;
                }
                return num - 1;
            }
            if (y > 0)
            {
                if (x % y == 0L)
                {
                    return num;
                }
                return num - 1;
            }
            return num;
        }

        /// <summary>
        /// Calculates the remainder of floor division of two 32-bit signed integers.
        /// </summary>
        /// <param name="x">Dividend.</param>
        /// <param name="y">Divisor.</param>
        /// <returns>The remainder of of floor division of the specified numbers, or <code>x - (int)Floor((double)x/(double)y) * y</code>.</returns>
        /// <exception cref="T:System.DivideByZeroException"><paramref name="y" /> is 0.</exception>
        public static int FloorRemainder(int x, int y)
        {
            if (y == -1)
            {
                return 0;
            }
            int num = x % y;
            if (x >= 0)
            {
                if (y > 0)
                {
                    return num;
                }
                if (num == 0)
                {
                    return 0;
                }
                return num + y;
            }
            if (y > 0)
            {
                if (num == 0)
                {
                    return 0;
                }
                return num + y;
            }
            return num;
        }

        /// <summary>
        /// Calculates the remainder of floor division of two 32-bit signed integers.
        /// </summary>
        /// <param name="x">Dividend.</param>
        /// <param name="y">Divisor.</param>
        /// <returns>The remainder of of floor division of the specified numbers, or <code>x - (int)Floor((double)x/(double)y) * y</code>.</returns>
        /// <exception cref="T:System.DivideByZeroException"><paramref name="y" /> is 0.</exception>
        public static long FloorRemainder(long x, long y)
        {
            if (y == -1)
            {
                return 0L;
            }
            long num = x % y;
            if (x >= 0)
            {
                if (y > 0)
                {
                    return num;
                }
                if (num == 0L)
                {
                    return 0L;
                }
                return num + y;
            }
            if (y > 0)
            {
                if (num == 0L)
                {
                    return 0L;
                }
                return num + y;
            }
            return num;
        }

        /// <summary>
        /// Behaves like Math.Round(value, MidpointRounding.AwayFromZero)
        /// Needed because CoreCLR doesn't support this particular overload of Math.Round
        /// </summary>
        [Obsolete("The method has been deprecated. Call MathUtils.Round(value, 0, MidpointRounding.AwayFromZero) instead.")]
        public static double RoundAwayFromZero(double value)
        {
            return RoundAwayFromZero(value, 0);
        }

        private static double GetPowerOf10(int precision)
        {
            if (precision >= 16)
            {
                return System.Math.Pow(10.0, precision);
            }
            return _RoundPowersOfTens[precision];
        }

        /// <summary>
        /// Behaves like Math.Round(value, precision, MidpointRounding.AwayFromZero)
        /// However, it works correctly on negative precisions and cases where precision is
        /// outside of the [-15, 15] range.
        ///
        /// (This function is also needed because CoreCLR lacks this overload.)
        /// </summary>
        [Obsolete("The method has been deprecated. Call MathUtils.Round(value, precision, MidpointRounding.AwayFromZero) instead.")]
        public static double RoundAwayFromZero(double value, int precision)
        {
            return Round(value, precision, MidpointRounding.AwayFromZero);
        }

        public static bool IsNegativeZero(double self)
        {
            if (self == 0.0)
            {
                return 1.0 / self < 0.0;
            }
            return false;
        }

        public static double Round(double value, int precision, MidpointRounding mode)
        {
            if (double.IsInfinity(value) || double.IsNaN(value))
            {
                return value;
            }
            if (precision >= 0)
            {
                if (precision > 308)
                {
                    return value;
                }
                double powerOf = GetPowerOf10(precision);
                return Math.Round(value * powerOf, mode) / powerOf;
            }
            if (precision >= -308)
            {
                double powerOf2 = GetPowerOf10(-precision);
                return Math.Round(value / powerOf2, mode) * powerOf2;
            }
            if (!(value < 0.0) && !(1.0 / value < 0.0))
            {
                return 0.0;
            }
            return -0.0;
        }

        public static double Erf(double v0)
        {
            if (v0 >= 10.0)
            {
                return 1.0;
            }
            if (v0 <= -10.0)
            {
                return -1.0;
            }
            if (v0 > 0.47 || v0 < -0.47)
            {
                return 1.0 - ErfComplement(v0);
            }
            double v = v0 * v0;
            double num = EvalPolynomial(v, ErfNumerCoeffs);
            double num2 = EvalPolynomial(v, ErfDenomCoeffs);
            return v0 * num / num2;
        }

        public static double ErfComplement(double v0)
        {
            if (v0 >= 30.0)
            {
                return 0.0;
            }
            if (v0 <= -10.0)
            {
                return 2.0;
            }
            double num = Math.Abs(v0);
            if (num < 0.47)
            {
                return 1.0 - Erf(v0);
            }
            double num4;
            if (num <= 4.0)
            {
                double num2 = EvalPolynomial(num, ErfcNumerCoeffs);
                double num3 = EvalPolynomial(num, ErfcDenomCoeffs);
                num4 = Math.Exp((0.0 - num) * num) * num2 / num3;
            }
            else
            {
                double num5 = num * num;
                num4 = 0.0;
                for (int num6 = 11; num6 > 0; num6--)
                {
                    double num7 = (double)(num6 * num6) * 0.25;
                    num4 += Math.Exp(0.0 - num7) / (num7 + num5);
                }
                num4 = 0.5 * num * Math.Exp(0.0 - num5) / Math.PI * (num4 * 2.0 + 1.0 / num5);
            }
            if (v0 < 0.0)
            {
                num4 = 2.0 - num4;
            }
            return num4;
        }

        public static double Gamma(double v0)
        {
            if (double.IsNegativeInfinity(v0))
            {
                return double.NaN;
            }
            double num = Math.Abs(v0);
            if (num % 1.0 == 0.0)
            {
                if (v0 <= 0.0)
                {
                    return double.NaN;
                }
                if (num <= 25.0)
                {
                    if (num <= 2.0)
                    {
                        return 1.0;
                    }
                    num -= 1.0;
                    v0 -= 1.0;
                    while ((v0 -= 1.0) > 1.0)
                    {
                        num *= v0;
                    }
                    return num;
                }
            }
            if (num < 1E-50)
            {
                return 1.0 / v0;
            }
            if (v0 < -150.0)
            {
                double num2 = v0 / 2.0;
                double num3 = Math.Pow(Math.PI, 1.5) / SinPi(v0);
                num3 *= Math.Pow(2.0, v0);
                num3 /= PositiveGamma(0.5 - num2);
                return num3 / PositiveGamma(1.0 - num2);
            }
            if (v0 < 0.001)
            {
                double num3 = Math.PI / SinPi(v0);
                double num4 = 1.0 - v0;
                if (v0 == 1.0 - num4)
                {
                    return num3 / PositiveGamma(num4);
                }
                return num3 / ((0.0 - v0) * PositiveGamma(0.0 - v0));
            }
            return PositiveGamma(v0);
        }

        public static double LogGamma(double v0)
        {
            if (double.IsInfinity(v0))
            {
                return double.PositiveInfinity;
            }
            double num = Math.Abs(v0);
            if (v0 <= 0.0 && num % 1.0 == 0.0)
            {
                return double.NaN;
            }
            if (num < 1E-50)
            {
                return 0.0 - Math.Log(num);
            }
            if (v0 < 0.0)
            {
                double num2 = Math.Log(Math.PI / AbsSinPi(v0));
                return num2 - PositiveLGamma(1.0 - v0);
            }
            return PositiveLGamma(v0);
        }

        public static double Hypot(double x, double y)
        {
            if (double.IsInfinity(x) || double.IsInfinity(y))
            {
                return double.PositiveInfinity;
            }
            if (x < 0.0)
            {
                x = 0.0 - x;
            }
            if (y < 0.0)
            {
                y = 0.0 - y;
            }
            if (x == 0.0)
            {
                return y;
            }
            if (y == 0.0)
            {
                return x;
            }
            if (x < y)
            {
                double num = y;
                y = x;
                x = num;
            }
            y /= x;
            return x * Math.Sqrt(1.0 + y * y);
        }

        /// <summary>
        /// Evaluates a polynomial in v0 where the coefficients are ordered in increasing degree
        /// </summary>
        private static double EvalPolynomial(double v0, double[] coeffs)
        {
            double num = 0.0;
            for (int num2 = coeffs.Length - 1; num2 >= 0; num2--)
            {
                num = num * v0 + coeffs[num2];
            }
            return num;
        }

        /// <summary>
        /// Evaluates a polynomial in v0 where the coefficients are ordered in increasing degree
        /// if reverse is false, and increasing degree if reverse is true.
        /// </summary>
        private static double EvalPolynomial(double v0, double[] coeffs, bool reverse)
        {
            if (!reverse)
            {
                return EvalPolynomial(v0, coeffs);
            }
            double num = 0.0;
            for (int i = 0; i < coeffs.Length; i++)
            {
                num = num * v0 + coeffs[i];
            }
            return num;
        }

        /// <summary>
        /// A numerically precise version of sin(v0 * pi)
        /// </summary>
        private static double SinPi(double v0)
        {
            double num = Math.Abs(v0) % 2.0;
            num = ((num < 0.25) ? Math.Sin(num * Math.PI) : ((num < 0.75) ? Math.Cos((num - 0.5) * Math.PI) : ((num < 1.25) ? (0.0 - Math.Sin((num - 1.0) * Math.PI)) : ((!(num < 1.75)) ? Math.Sin((num - 2.0) * Math.PI) : (0.0 - Math.Cos((num - 1.5) * Math.PI))))));
            if (!(v0 < 0.0))
            {
                return num;
            }
            return 0.0 - num;
        }

        /// <summary>
        /// A numerically precise version of |sin(v0 * pi)|
        /// </summary>
        private static double AbsSinPi(double v0)
        {
            double num = Math.Abs(v0) % 1.0;
            num = ((num < 0.25) ? Math.Sin(num * Math.PI) : ((!(num < 0.75)) ? Math.Sin((num - 1.0) * Math.PI) : Math.Cos((num - 0.5) * Math.PI)));
            return Math.Abs(num);
        }

        /// <summary>
        /// Take the quotient of the 2 polynomials forming the Lanczos approximation
        /// with N=13 and G=13.144565
        /// </summary>
        private static double GammaRationalFunc(double v0)
        {
            double num = 0.0;
            double num2 = 0.0;
            if (v0 < 1E+15)
            {
                num = EvalPolynomial(v0, GammaNumerCoeffs);
                num2 = EvalPolynomial(v0, GammaDenomCoeffs);
            }
            else
            {
                double v = 1.0 / v0;
                num = EvalPolynomial(v, GammaNumerCoeffs, reverse: true);
                num2 = EvalPolynomial(v, GammaDenomCoeffs, reverse: true);
            }
            return num / num2;
        }

        /// <summary>
        /// Computes the Gamma function on positive values, using the Lanczos approximation.
        /// Lanczos parameters are N=13 and G=13.144565.
        /// </summary>
        private static double PositiveGamma(double v0)
        {
            if (v0 > 200.0)
            {
                return double.PositiveInfinity;
            }
            double num = v0 + 12.644565;
            double num2 = GammaRationalFunc(v0);
            num2 /= Math.Exp(num);
            if (v0 < 120.0)
            {
                return num2 * Math.Pow(num, v0 - 0.5);
            }
            double num3 = Math.Pow(num, v0 / 2.0 - 0.25);
            num2 *= num3;
            return num2 * num3;
        }

        /// <summary>
        /// Computes the Log-Gamma function on positive values, using the Lanczos approximation.
        /// Lanczos parameters are N=13 and G=13.144565.
        /// </summary>
        private static double PositiveLGamma(double v0)
        {
            double d = v0 + 13.144565 - 0.5;
            return Math.Log(GammaRationalFunc(v0)) - 13.144565 + (v0 - 0.5) * (Math.Log(d) - 1.0);
        }

        internal static string BigIntegerToString(uint[] d, int sign, int radix, bool lowerCase)
        {
            if (radix < 2 || radix > 36)
            {
                throw ExceptionUtils.MakeArgumentOutOfRangeException("radix", radix, "radix must be between 2 and 36");
            }
            int nl = d.Length;
            if (nl == 0)
            {
                return "0";
            }
            List<uint> list = new List<uint>();
            uint d2 = groupRadixValues[radix];
            while (nl > 0)
            {
                uint item = div(d, ref nl, d2);
                list.Add(item);
            }
            StringBuilder stringBuilder = new StringBuilder();
            if (sign == -1)
            {
                stringBuilder.Append("-");
            }
            int num = list.Count - 1;
            char[] tmp = new char[maxCharsPerDigit[radix]];
            AppendRadix(list[num--], (uint)radix, tmp, stringBuilder, leadingZeros: false, lowerCase);
            while (num >= 0)
            {
                AppendRadix(list[num--], (uint)radix, tmp, stringBuilder, leadingZeros: true, lowerCase);
            }
            if (stringBuilder.Length != 0)
            {
                return stringBuilder.ToString();
            }
            return "0";
        }

        private static uint div(uint[] n, ref int nl, uint d)
        {
            ulong num = 0uL;
            int num2 = nl;
            bool flag = false;
            while (--num2 >= 0)
            {
                num <<= 32;
                num |= n[num2];
                if ((n[num2] = (uint)(num / d)) == 0)
                {
                    if (!flag)
                    {
                        nl--;
                    }
                }
                else
                {
                    flag = true;
                }
                num %= d;
            }
            return (uint)num;
        }

        private static void AppendRadix(uint rem, uint radix, char[] tmp, StringBuilder buf, bool leadingZeros, bool lowerCase)
        {
            string text = (lowerCase ? "0123456789abcdefghijklmnopqrstuvwxyz" : "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ");
            int num = tmp.Length;
            int num2 = num;
            while (num2 > 0 && (leadingZeros || rem != 0))
            {
                uint index = rem % radix;
                rem /= radix;
                tmp[--num2] = text[(int)index];
            }
            if (leadingZeros)
            {
                buf.Append(tmp);
            }
            else
            {
                buf.Append(tmp, num2, num - num2);
            }
        }

        private static uint GetWord(byte[] bytes, int start, int end)
        {
            uint num = 0u;
            int num2 = end - start;
            int num3 = 0;
            if (num2 > 32)
            {
                num2 = 32;
            }
            start /= 8;
            while (num2 > 0)
            {
                uint num4 = bytes[start];
                if (num2 < 8)
                {
                    num4 &= (uint)((1 << num2) - 1);
                }
                num4 <<= num3;
                num |= num4;
                num2 -= 8;
                num3 += 8;
                start++;
            }
            return num;
        }

        public static BigInteger GetRandBits(Action<byte[]> NextBytes, int bits)
        {
            ContractUtils.Requires(bits > 0, "bits");
            int num = ((bits % 8 == 0) ? (bits / 8) : (bits / 8 + 1));
            byte[] array = new byte[(bits % 8 == 0) ? (num + 1) : num];
            NextBytes(array);
            if (bits % 8 == 0)
            {
                array[^1] = 0;
            }
            else
            {
                array[^1] = (byte)(array[^1] & ((1 << bits % 8) - 1));
            }
            if (bits <= 32)
            {
                return GetWord(array, 0, bits);
            }
            if (bits <= 64)
            {
                long num2 = GetWord(array, 0, bits);
                ulong num3 = GetWord(array, 32, bits);
                return (ulong)num2 | (num3 << 32);
            }
            return new BigInteger(array);
        }

        public static BigInteger GetRandBits(this Random generator, int bits)
        {
            return GetRandBits(generator.NextBytes, bits);
        }

        public static BigInteger Random(this Random generator, BigInteger limit)
        {
            ContractUtils.Requires(limit.Sign > 0, "limit");
            ContractUtils.RequiresNotNull(generator, "generator");
            BigInteger zero = BigInteger.Zero;
            int ret;
            while (true)
            {
                if (limit == BigInteger.Zero)
                {
                    return zero;
                }
                if (limit.AsInt32(out ret))
                {
                    break;
                }
                byte[] array = limit.ToByteArray();
                int num = array.Length;
                while (array[--num] == 0)
                {
                }
                int num2;
                if (array[num] < 128)
                {
                    num2 = array[num] << 24;
                    array[num--] = 0;
                }
                else
                {
                    num2 = 0;
                }
                num2 |= array[num] << 16;
                array[num--] = 0;
                num2 |= array[num] << 8;
                array[num--] = 0;
                num2 |= array[num];
                array[num--] = 0;
                byte[] array2 = new byte[num + 2];
                generator.NextBytes(array2);
                array2[num + 1] = 0;
                zero += new BigInteger(array2);
                zero += (BigInteger)generator.Next(num2) << (num + 1) * 8;
                limit = new BigInteger(array);
            }
            return zero + generator.Next(ret);
        }

        public static bool TryToFloat64(this BigInteger self, out double result)
        {
            result = (double)self;
            if (double.IsInfinity(result))
            {
                result = 0.0;
                return false;
            }
            return true;
        }

        public static double ToFloat64(this BigInteger self)
        {
            if (self.TryToFloat64(out var result))
            {
                return result;
            }
            throw new OverflowException("Value was either too large or too small for a Double.");
        }

        public static int BitLength(BigInteger x)
        {
            if (x.IsZero)
            {
                return 0;
            }
            byte[] array = BigInteger.Abs(x).ToByteArray();
            int num = array.Length;
            while (array[--num] == 0)
            {
            }
            return num * 8 + BitLength(array[num]);
        }

        public static int BitLength(long x)
        {
            switch (x)
            {
                case 0L:
                    return 0;
                case long.MinValue:
                    return 64;
                default:
                    {
                        x = Math.Abs(x);
                        int num = 1;
                        if (x >= 4294967296L)
                        {
                            x >>= 32;
                            num += 32;
                        }
                        if (x >= 65536)
                        {
                            x >>= 16;
                            num += 16;
                        }
                        if (x >= 256)
                        {
                            x >>= 8;
                            num += 8;
                        }
                        if (x >= 16)
                        {
                            x >>= 4;
                            num += 4;
                        }
                        if (x >= 4)
                        {
                            x >>= 2;
                            num += 2;
                        }
                        if (x >= 2)
                        {
                            num++;
                        }
                        return num;
                    }
            }
        }

        [CLSCompliant(false)]
        public static int BitLengthUnsigned(ulong x)
        {
            if (x >= 9223372036854775808uL)
            {
                return 64;
            }
            return BitLength((long)x);
        }

        public static int BitLength(int x)
        {
            switch (x)
            {
                case 0:
                    return 0;
                case int.MinValue:
                    return 32;
                default:
                    {
                        x = Math.Abs(x);
                        int num = 1;
                        if (x >= 65536)
                        {
                            x >>= 16;
                            num += 16;
                        }
                        if (x >= 256)
                        {
                            x >>= 8;
                            num += 8;
                        }
                        if (x >= 16)
                        {
                            x >>= 4;
                            num += 4;
                        }
                        if (x >= 4)
                        {
                            x >>= 2;
                            num += 2;
                        }
                        if (x >= 2)
                        {
                            num++;
                        }
                        return num;
                    }
            }
        }

        [CLSCompliant(false)]
        public static int BitLengthUnsigned(uint x)
        {
            if (x >= 2147483648u)
            {
                return 32;
            }
            return BitLength((int)x);
        }

        public static bool AsInt32(this BigInteger self, out int ret)
        {
            if (self >= -2147483648L && self <= 2147483647L)
            {
                ret = (int)self;
                return true;
            }
            ret = 0;
            return false;
        }

        public static bool AsInt64(this BigInteger self, out long ret)
        {
            if (self >= long.MinValue && self <= long.MaxValue)
            {
                ret = (long)self;
                return true;
            }
            ret = 0L;
            return false;
        }

        [CLSCompliant(false)]
        public static bool AsUInt32(this BigInteger self, out uint ret)
        {
            if (self >= 0L && self <= 4294967295L)
            {
                ret = (uint)self;
                return true;
            }
            ret = 0u;
            return false;
        }

        [CLSCompliant(false)]
        public static bool AsUInt64(this BigInteger self, out ulong ret)
        {
            if (self >= 0uL && self <= ulong.MaxValue)
            {
                ret = (ulong)self;
                return true;
            }
            ret = 0uL;
            return false;
        }

        public static BigInteger Abs(this BigInteger self)
        {
            return BigInteger.Abs(self);
        }

        public static bool IsZero(this BigInteger self)
        {
            return self.IsZero;
        }

        public static bool IsPositive(this BigInteger self)
        {
            return self.Sign > 0;
        }

        public static bool IsNegative(this BigInteger self)
        {
            return self.Sign < 0;
        }

        public static double Log(this BigInteger self)
        {
            return BigInteger.Log(self);
        }

        public static double Log(this BigInteger self, double baseValue)
        {
            return BigInteger.Log(self, baseValue);
        }

        public static double Log10(this BigInteger self)
        {
            return BigInteger.Log10(self);
        }

        public static BigInteger Power(this BigInteger self, int exp)
        {
            return BigInteger.Pow(self, exp);
        }

        public static BigInteger Power(this BigInteger self, long exp)
        {
            if (exp < 0)
            {
                throw ExceptionUtils.MakeArgumentOutOfRangeException("exp", exp, "Must be at least 0");
            }
            if (exp <= int.MaxValue)
            {
                return BigInteger.Pow(self, (int)exp);
            }
            if (self.IsOne)
            {
                return BigInteger.One;
            }
            if (self.IsZero)
            {
                return BigInteger.Zero;
            }
            if (self == BigInteger.MinusOne)
            {
                if (exp % 2 == 0L)
                {
                    return BigInteger.One;
                }
                return BigInteger.MinusOne;
            }
            BigInteger one = BigInteger.One;
            while (exp != 0L)
            {
                if (exp % 2 != 0L)
                {
                    one *= self;
                }
                exp >>= 1;
                self *= self;
            }
            return one;
        }

        public static BigInteger ModPow(this BigInteger self, int power, BigInteger mod)
        {
            return BigInteger.ModPow(self, power, mod);
        }

        public static BigInteger ModPow(this BigInteger self, BigInteger power, BigInteger mod)
        {
            return BigInteger.ModPow(self, power, mod);
        }

        public static string ToString(this BigInteger self, int radix)
        {
            if (radix < 2 || radix > 36)
            {
                throw ExceptionUtils.MakeArgumentOutOfRangeException("radix", radix, "radix must be between 2 and 36");
            }
            bool flag = false;
            if (self < BigInteger.Zero)
            {
                self = -self;
                flag = true;
            }
            else if (self == BigInteger.Zero)
            {
                return "0";
            }
            List<char> list = new List<char>();
            while (self > 0L)
            {
                list.Add("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ"[(int)(self % radix)]);
                self /= (BigInteger)radix;
            }
            StringBuilder stringBuilder = new StringBuilder();
            if (flag)
            {
                stringBuilder.Append('-');
            }
            for (int num = list.Count - 1; num >= 0; num--)
            {
                stringBuilder.Append(list[num]);
            }
            return stringBuilder.ToString();
        }

        [CLSCompliant(false)]
        public static uint[] GetWords(this BigInteger self)
        {
            if (self.IsZero)
            {
                return new uint[1];
            }
            GetHighestByte(self, out var index, out var byteArray);
            uint[] array = new uint[(index + 1 + 3) / 4];
            int num = 0;
            int num2 = 0;
            uint num3 = 0u;
            int num4 = 0;
            while (num < byteArray.Length)
            {
                num3 |= (uint)(byteArray[num++] << num4);
                if (num % 4 == 0)
                {
                    array[num2++] = num3;
                    num3 = 0u;
                }
                num4 += 8;
            }
            if (num3 != 0)
            {
                array[num2] = num3;
            }
            return array;
        }

        [CLSCompliant(false)]
        public static uint GetWord(this BigInteger self, int index)
        {
            return self.GetWords()[index];
        }

        public static int GetWordCount(this BigInteger self)
        {
            GetHighestByte(self, out var index, out var _);
            return index / 4 + 1;
        }

        public static int GetByteCount(this BigInteger self)
        {
            GetHighestByte(self, out var index, out var _);
            return index + 1;
        }

        public static int GetBitCount(this BigInteger self)
        {
            if (self.IsZero)
            {
                return 1;
            }
            byte[] array = BigInteger.Abs(self).ToByteArray();
            int num = array.Length;
            while (array[--num] == 0)
            {
            }
            int num2 = num * 8;
            for (int num3 = array[num]; num3 > 0; num3 >>= 1)
            {
                num2++;
            }
            return num2;
        }

        private static byte GetHighestByte(BigInteger self, out int index, out byte[] byteArray)
        {
            byte[] array = BigInteger.Abs(self).ToByteArray();
            if (self.IsZero)
            {
                byteArray = array;
                index = 0;
                return 1;
            }
            int num = array.Length;
            byte b;
            do
            {
                b = array[--num];
            }
            while (b == 0);
            index = num;
            byteArray = array;
            return b;
        }

        public static Complex MakeReal(double real)
        {
            return new Complex(real, 0.0);
        }

        public static Complex MakeImaginary(double imag)
        {
            return new Complex(0.0, imag);
        }

        public static Complex MakeComplex(double real, double imag)
        {
            return new Complex(real, imag);
        }

        public static double Imaginary(this Complex self)
        {
            return self.Imaginary;
        }

        public static bool IsZero(this Complex self)
        {
            return self.Equals(Complex.Zero);
        }

        public static Complex Conjugate(this Complex self)
        {
            return new Complex(self.Real, 0.0 - self.Imaginary);
        }

        public static double Abs(this Complex self)
        {
            return Complex.Abs(self);
        }

        public static Complex Pow(this Complex self, Complex power)
        {
            return Complex.Pow(self, power);
        }
    }

}
