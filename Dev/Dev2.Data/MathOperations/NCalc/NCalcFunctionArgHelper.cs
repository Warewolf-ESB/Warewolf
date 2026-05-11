/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using System;
using System.Globalization;
using NCalc;

namespace Dev2.MathOperations.NCalc
{
    /// <summary>
    /// Zero-allocation typed argument-extraction helpers for NCalc <see cref="Expression"/> arrays.
    /// All methods are hot-path inlined; allocations are avoided by using value-type returns where possible.
    /// </summary>
    internal static class FunctionArgHelper
    {
        /// <summary>Evaluates parameter <paramref name="i"/> as a <see cref="double"/>.</summary>
        internal static double D(this Expression[] p, int i)
            => Convert.ToDouble(p[i].Evaluate(), CultureInfo.InvariantCulture);

        /// <summary>Evaluates parameter <paramref name="i"/> as a <see cref="string"/>.</summary>
        internal static string S(this Expression[] p, int i)
            => p[i].Evaluate()?.ToString() ?? string.Empty;

        /// <summary>Evaluates parameter <paramref name="i"/> as a 32-bit integer (truncated).</summary>
        internal static int I32(this Expression[] p, int i)
            => (int)D(p, i);

        /// <summary>Evaluates parameter <paramref name="i"/> as a <see cref="bool"/>.</summary>
        internal static bool Bool(this Expression[] p, int i)
            => Convert.ToBoolean(p[i].Evaluate());

        /// <summary>
        /// Evaluates all parameters as <see cref="double"/> values.
        /// Allocates a single array sized exactly <c>p.Length</c>.
        /// </summary>
        internal static double[] AllD(this Expression[] p)
        {
            var result = new double[p.Length];
            for (var i = 0; i < p.Length; i++)
                result[i] = Convert.ToDouble(p[i].Evaluate(), CultureInfo.InvariantCulture);
            return result;
        }

        /// <summary>
        /// Coerces an arbitrary <paramref name="value"/> to <see cref="DateTime"/>.
        /// Handles <see cref="DateTime"/>, OA-date <see cref="double"/>, and parseable strings.
        /// </summary>
        internal static DateTime ToDate(object? value)
        {
            return value switch
            {
                DateTime dt => dt,
                double d    => DateTime.FromOADate(d),
                int i       => FromExcelOADate(i),
                long l      => FromExcelOADate(l),
                float f     => DateTime.FromOADate(f),
                decimal dec => DateTime.FromOADate((double)dec),
                _           => DateTime.Parse(value?.ToString() ?? string.Empty, CultureInfo.InvariantCulture)
            };
        }

        /// <summary>
        /// Converts an Excel OADate serial number to DateTime, accounting for Excel's
        /// incorrect leap-year bug (fake Feb 29, 1900 = serial 60). For serials 1–59 Excel
        /// is one day ahead of .NET's DateTime.FromOADate; for serials ≥ 61 they agree.
        /// </summary>
        private static DateTime FromExcelOADate(double n)
        {
            if (n >= 61) return DateTime.FromOADate(n);
            if (n == 60) return new DateTime(1900, 3, 1); // Excel's non-existent Feb 29 → Mar 1
            if (n >= 1)  return DateTime.FromOADate(n + 1); // Excel 1–59 is 1 day ahead of .NET
            return DateTime.FromOADate(n);
        }

        /// <summary>
        /// Requires at least <paramref name="count"/> parameters; throws a descriptive
        /// <see cref="InvalidOperationException"/> when the constraint is violated.
        /// </summary>
        internal static void RequireArgs(this Expression[] p, int count, string functionName)
        {
            if (p.Length < count)
                throw new InvalidOperationException(
                    $"{functionName} requires at least {count} argument(s), got {p.Length}.");
        }
    }
}