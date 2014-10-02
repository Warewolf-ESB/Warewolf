
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System
{
    public static class MathHelper
    {
        #region Instance Fields
        private static Random _random = new Random();
        #endregion

        #region EnsureRange(...)
        public static void EnsureRange(ref sbyte lower, ref sbyte upper)
        {
            if (upper >= lower) return;
            sbyte swap = upper;
            upper = lower;
            lower = swap;
        }

        public static void EnsureRange(ref byte lower, ref byte upper)
        {
            if (upper >= lower) return;
            byte swap = upper;
            upper = lower;
            lower = swap;
        }

        public static void EnsureRange(ref short lower, ref short upper)
        {
            if (upper >= lower) return;
            short swap = upper;
            upper = lower;
            lower = swap;
        }

        public static void EnsureRange(ref ushort lower, ref ushort upper)
        {
            if (upper >= lower) return;
            ushort swap = upper;
            upper = lower;
            lower = swap;
        }

        public static void EnsureRange(ref int lower, ref int upper)
        {
            if (upper >= lower) return;
            int swap = upper;
            upper = lower;
            lower = swap;
        }

        public static void EnsureRange(ref uint lower, ref uint upper)
        {
            if (upper >= lower) return;
            uint swap = upper;
            upper = lower;
            lower = swap;
        }

        public static void EnsureRange(ref long lower, ref long upper)
        {
            if (upper >= lower) return;
            long swap = upper;
            upper = lower;
            lower = swap;
        }

        public static void EnsureRange(ref ulong lower, ref ulong upper)
        {
            if (upper >= lower) return;
            ulong swap = upper;
            upper = lower;
            lower = swap;
        }

        public static void EnsureRange(ref float lower, ref float upper)
        {
            if (upper >= lower) return;
            float swap = upper;
            upper = lower;
            lower = swap;
        }

        public static void EnsureRange(ref double lower, ref double upper)
        {
            if (upper >= lower) return;
            double swap = upper;
            upper = lower;
            lower = swap;
        }

        public static void EnsureRange(ref decimal lower, ref decimal upper)
        {
            if (upper >= lower) return;
            decimal swap = upper;
            upper = lower;
            lower = swap;
        }
        #endregion

        #region Random(...)
        public static double Random()
        {
            return _random.NextDouble();
        }

        public static sbyte Random(sbyte lowerInclusive, sbyte upperExclusive)
        {
#if DEBUG
            EnsureRange(ref lowerInclusive, ref upperExclusive);
#endif
            return (sbyte)_random.Next(lowerInclusive, upperExclusive);
        }

        public static sbyte Random(sbyte upperExclusive)
        {
            return (sbyte)_random.Next(upperExclusive);
        }

        public static byte Random(byte lowerInclusive, byte upperExclusive)
        {
#if DEBUG
            EnsureRange(ref lowerInclusive, ref upperExclusive);
#endif
            return (byte)_random.Next(lowerInclusive, upperExclusive);
        }

        public static byte Random(byte upperExclusive)
        {
            return (byte)_random.Next(upperExclusive);
        }

        public static short Random(short lowerInclusive, short upperExclusive)
        {
#if DEBUG
            EnsureRange(ref lowerInclusive, ref upperExclusive);
#endif
            return (short)_random.Next(lowerInclusive, upperExclusive);
        }

        public static short Random(short upperExclusive)
        {
            return (short)_random.Next(upperExclusive);
        }

        public static ushort Random(ushort lowerInclusive, ushort upperExclusive)
        {
#if DEBUG
            EnsureRange(ref lowerInclusive, ref upperExclusive);
#endif
            return (ushort)_random.Next(lowerInclusive, upperExclusive);
        }

        public static ushort Random(ushort upperExclusive)
        {
            return (ushort)_random.Next(upperExclusive);
        }

        public static int Random(int lowerInclusive, int upperExclusive)
        {
#if DEBUG
            EnsureRange(ref lowerInclusive, ref upperExclusive);
#endif
            return _random.Next(lowerInclusive, upperExclusive);
        }

        public static int Random(int upperExclusive)
        {
            return _random.Next(upperExclusive);
        }

        public static uint Random(uint lowerInclusive, uint upperExclusive)
        {
#if DEBUG
            EnsureRange(ref lowerInclusive, ref upperExclusive);
#endif
            return ((uint)(_random.NextDouble() * (upperExclusive - lowerInclusive)) + lowerInclusive);
        }

        public static uint Random(uint upperExclusive)
        {
            return (uint)(_random.NextDouble() * upperExclusive);
        }

        public static long Random(long lowerInclusive, long upperExclusive)
        {
#if DEBUG
            EnsureRange(ref lowerInclusive, ref upperExclusive);
#endif
            return ((long)(_random.NextDouble() * (upperExclusive - lowerInclusive)) + lowerInclusive);
        }

        public static long Random(long upperExclusive)
        {
            return (long)(_random.NextDouble() * upperExclusive);
        }

        public static float Random(float lowerInclusive, float upperExclusive)
        {
#if DEBUG
            EnsureRange(ref lowerInclusive, ref upperExclusive);
#endif
            return ((float)(_random.NextDouble() * (upperExclusive - lowerInclusive)) + lowerInclusive);
        }

        public static float Random(float upperExclusive)
        {
            return (float)(_random.NextDouble() * upperExclusive);
        }

        public static double Random(double lowerInclusive, double upperExclusive)
        {
#if DEBUG
            EnsureRange(ref lowerInclusive, ref upperExclusive);
#endif
            return ((double)(_random.NextDouble() * (upperExclusive - lowerInclusive)) + lowerInclusive);
        }

        public static double Random(double upperExclusive)
        {
            return (double)(_random.NextDouble() * upperExclusive);
        }
        #endregion

        #region Common(...)
        public static int GreatestCommonDivisor(params int[] integers)
        {
            if (integers == null) return 0;
            int value = integers.Length;

            if (value < 3)
            {
                if (value == 2) return GreatestCommonDivisor(integers[0], integers[1]);
                else if (value == 1) return integers[0];
                return 0;
            }

            value = GreatestCommonDivisor(integers[0], integers[1]);
            for (int i = 2; i < integers.Length; i++) value = GreatestCommonDivisor(integers[i], value);
            return value;
        }

        public static int GreatestCommonDivisor(int u, int v)
        {
            if (u == 0 || v == 0) return u | v;

            int shift;

            for (shift = 0; ((u | v) & 1) == 0; ++shift)
            {
                u >>= 1;
                v >>= 1;
            }

            while ((u & 1) == 0) u >>= 1;

            do
            {
                while ((v & 1) == 0) v >>= 1;

                if (u < v) v -= u;
                else
                {
                    int diff = u - v;
                    u = v;
                    v = diff;
                }

                v >>= 1;
            }
            while (v != 0);

            return u << shift;
        }

        public static int LeastCommonMultiple(params int[] integers)
        {
            if (integers == null) return 0;
            int value = integers.Length;

            if (value < 3)
            {
                if (value == 2) return LeastCommonMultiple(integers[0], integers[1]);
                else if (value == 1) return integers[0];
                return 0;
            }

            value = LeastCommonMultiple(integers[0], integers[1]);
            for (int i = 2; i < integers.Length; i++) value = LeastCommonMultiple(integers[i], value);
            return value;
        }

        public static int LeastCommonMultiple(int u, int v)
        {
            if (u == 0 || v == 0) return 0;
            return (u / GreatestCommonDivisor(u, v)) * v;
        }
        #endregion
    }
}
