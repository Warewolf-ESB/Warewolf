/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using NCalc;
using NCalc.Handlers;

namespace Dev2.MathOperations.NCalc.Functions
{
    /// <summary>
    /// Registers all math / numeric NCalc function handlers.
    /// All methods are static; no instance state — safe for concurrent Azure Function invocations.
    /// </summary>
    internal static class MathFunctions
    {
        [ThreadStatic]
        private static Random? _rng;

        private static Random Rng => _rng ??= new Random();

        internal static void Register(IDictionary<string, Action<FunctionArgs>> handlers)
        {
            // ── Aggregates ──────────────────────────────────────────────────────────────
            handlers["SUM"]         = Sum;
            handlers["AVERAGE"]     = Average;
            handlers["MIN"]         = Min;
            handlers["MAX"]         = Max;
            handlers["COUNT"]       = Count;
            handlers["COUNTA"]      = CountA;
            handlers["SUBTOTAL"]    = Subtotal;
            handlers["AVEDEV"]      = AveDev;
            handlers["MEDIAN"]      = Median;
            handlers["VAR"]         = Var;
            handlers["STDEV"]       = Stdev;

            // ── Rounding ────────────────────────────────────────────────────────────────
            handlers["ROUND"]       = Round;
            handlers["ROUNDDOWN"]   = RoundDown;
            handlers["ROUNDUP"]     = RoundUp;
            handlers["MROUND"]      = MRound;
            handlers["FLOOR"]       = Floor;
            handlers["CEILING"]     = Ceiling;
            handlers["TRUNC"]       = Trunc;
            handlers["INT"]         = Int;
            handlers["EVEN"]        = Even;
            handlers["ODD"]         = Odd;

            // ── Arithmetic / algebra ────────────────────────────────────────────────────
            handlers["MOD"]         = Mod;
            handlers["QUOTIENT"]    = Quotient;
            handlers["PRODUCT"]     = Product;
            handlers["POWER"]       = Power;
            handlers["ABS"]         = Abs;
            handlers["SIGN"]        = Sign;
            handlers["SQRT"]        = Sqrt;
            handlers["SQRTPI"]      = SqrtPi;
            handlers["EXP"]         = Exp;
            handlers["LN"]          = Ln;
            handlers["LOG"]         = Log;
            handlers["LOG10"]       = Log10;
            handlers["PI"]          = Pi;

            // ── Trigonometry ────────────────────────────────────────────────────────────
            handlers["SIN"]         = Sin;
            handlers["COS"]         = Cos;
            handlers["TAN"]         = Tan;
            handlers["ASIN"]        = Asin;
            handlers["ACOS"]        = Acos;
            handlers["ATAN"]        = Atan;
            handlers["ATAN2"]       = Atan2;
            handlers["SINH"]        = Sinh;
            handlers["COSH"]        = Cosh;
            handlers["TANH"]        = Tanh;
            handlers["ASINH"]       = Asinh;
            handlers["ACOSH"]       = Acosh;
            handlers["ATANH"]       = Atanh;
            handlers["DEGREES"]     = Degrees;
            handlers["RADIANS"]     = Radians;

            // ── Combinatorics / number-theory ───────────────────────────────────────────
            handlers["FACT"]        = Fact;
            handlers["FACTDOUBLE"]  = FactDouble;
            handlers["COMBIN"]      = Combin;
            handlers["MULTINOMIAL"] = Multinomial;
            handlers["GCD"]         = Gcd;
            handlers["LCM"]         = Lcm;
            handlers["DELTA"]       = Delta;
            handlers["GESTEP"]      = GeStep;

            // ── Randomness ──────────────────────────────────────────────────────────────
            handlers["RAND"]        = Rand;
            handlers["RANDBETWEEN"] = RandBetween;

            // ── Series ──────────────────────────────────────────────────────────────────
            handlers["SERIESSUM"]   = SeriesSum;
            handlers["ROMAN"]       = Roman;
        }

        // ── Aggregates ──────────────────────────────────────────────────────────────────

        private static void Sum(FunctionArgs args)
        {
            args.Result = args.Parameters.AllD().Sum();
        }

        private static void Average(FunctionArgs args)
        {
            var values = args.Parameters.AllD();
            if (values.Length == 0) throw new InvalidOperationException("AVERAGE requires at least one argument.");
            args.Result = values.Average();
        }

        private static void Min(FunctionArgs args)
        {
            var values = args.Parameters.AllD();
            if (values.Length == 0) throw new InvalidOperationException("MIN requires at least one argument.");
            args.Result = values.Min();
        }

        private static void Max(FunctionArgs args)
        {
            var values = args.Parameters.AllD();
            if (values.Length == 0) throw new InvalidOperationException("MAX requires at least one argument.");
            args.Result = values.Max();
        }

        private static void Count(FunctionArgs args)
        {
            // COUNT counts all parameters (like Excel COUNT with literals)
            args.Result = (double)args.Parameters.Length;
        }

        private static void CountA(FunctionArgs args)
        {
            // COUNTA: count non-empty/non-null values
            var count = 0;
            foreach (var p in args.Parameters)
            {
                var v = p.Evaluate();
                if (v != null && v.ToString() != string.Empty)
                    count++;
            }
            args.Result = (double)count;
        }

        private static void Subtotal(FunctionArgs args)
        {
            args.Parameters.RequireArgs(2, "SUBTOTAL");
            // function_num, value1, value2, ...
            var fnNum = args.Parameters.I32(0);
            var values = args.Parameters.AllD().Skip(1).ToArray();

            args.Result = fnNum switch
            {
                1 or 101 => values.Length > 0 ? values.Average() : 0d,
                2 or 102 => (double)values.Length,
                3 or 103 => (double)values.Length,
                4 or 104 => values.Length > 0 ? values.Max() : 0d,
                5 or 105 => values.Length > 0 ? values.Min() : 0d,
                9 or 109 => values.Sum(),
                _ => throw new InvalidOperationException($"SUBTOTAL: unsupported function_num {fnNum}.")
            };
        }

        private static void AveDev(FunctionArgs args)
        {
            var values = args.Parameters.AllD();
            if (values.Length == 0) throw new InvalidOperationException("AVEDEV requires at least one argument.");
            var mean = values.Average();
            args.Result = values.Select(v => Math.Abs(v - mean)).Average();
        }

        private static void Median(FunctionArgs args)
        {
            var sorted = args.Parameters.AllD().OrderBy(x => x).ToArray();
            if (sorted.Length == 0) throw new InvalidOperationException("MEDIAN requires at least one argument.");
            var mid = sorted.Length / 2;
            args.Result = sorted.Length % 2 == 0
                ? (sorted[mid - 1] + sorted[mid]) / 2.0
                : sorted[mid];
        }

        private static void Var(FunctionArgs args)
        {
            var values = args.Parameters.AllD();
            if (values.Length < 2) throw new InvalidOperationException("VAR requires at least 2 arguments.");
            var mean = values.Average();
            args.Result = values.Select(v => (v - mean) * (v - mean)).Sum() / (values.Length - 1);
        }

        private static void Stdev(FunctionArgs args)
        {
            var values = args.Parameters.AllD();
            if (values.Length < 2) throw new InvalidOperationException("STDEV requires at least 2 arguments.");
            var mean = values.Average();
            args.Result = Math.Sqrt(values.Select(v => (v - mean) * (v - mean)).Sum() / (values.Length - 1));
        }

        // ── Rounding ────────────────────────────────────────────────────────────────────

        private static void Round(FunctionArgs args)
        {
            args.Parameters.RequireArgs(2, "ROUND");
            var x = args.Parameters.D(0);
            var digits = args.Parameters.I32(1);
            if (digits >= 0)
            {
                args.Result = Math.Round(x, digits, MidpointRounding.AwayFromZero);
            }
            else
            {
                var factor = Math.Pow(10, -digits);
                args.Result = Math.Round(x / factor, MidpointRounding.AwayFromZero) * factor;
            }
        }

        private static void RoundDown(FunctionArgs args)
        {
            args.Parameters.RequireArgs(2, "ROUNDDOWN");
            var x = args.Parameters.D(0);
            var d = args.Parameters.I32(1);
            var factor = Math.Pow(10, d);
            args.Result = Math.Truncate(x * factor) / factor;
        }

        private static void RoundUp(FunctionArgs args)
        {
            args.Parameters.RequireArgs(2, "ROUNDUP");
            var x = args.Parameters.D(0);
            var d = args.Parameters.I32(1);
            var factor = Math.Pow(10, d);
            args.Result = x >= 0
                ? Math.Ceiling(x * factor) / factor
                : Math.Floor(x * factor) / factor;
        }

        private static void MRound(FunctionArgs args)
        {
            args.Parameters.RequireArgs(2, "MROUND");
            var multiple = args.Parameters.D(1);
            if (multiple == 0) throw new InvalidOperationException("MROUND: multiple cannot be zero.");
            args.Result = Math.Round(args.Parameters.D(0) / multiple, MidpointRounding.AwayFromZero) * multiple;
        }

        private static void Floor(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "FLOOR");
            if (args.Parameters.Length >= 2)
            {
                var significance = args.Parameters.D(1);
                if (significance == 0) { args.Result = 0d; return; }
                args.Result = Math.Floor(args.Parameters.D(0) / significance) * significance;
            }
            else
            {
                args.Result = Math.Floor(args.Parameters.D(0));
            }
        }

        private static void Ceiling(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "CEILING");
            if (args.Parameters.Length >= 2)
            {
                var significance = args.Parameters.D(1);
                if (significance == 0) { args.Result = 0d; return; }
                args.Result = Math.Ceiling(args.Parameters.D(0) / significance) * significance;
            }
            else
            {
                args.Result = Math.Ceiling(args.Parameters.D(0));
            }
        }

        private static void Trunc(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "TRUNC");
            var x = args.Parameters.D(0);
            if (args.Parameters.Length >= 2)
            {
                var d = args.Parameters.I32(1);
                var factor = Math.Pow(10, d);
                args.Result = Math.Truncate(x * factor) / factor;
            }
            else
            {
                args.Result = Math.Truncate(x);
            }
        }

        private static void Int(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "INT");
            args.Result = Math.Floor(args.Parameters.D(0));
        }

        private static void Even(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "EVEN");
            var x = args.Parameters.D(0);
            var sign = x >= 0 ? 1 : -1;
            args.Result = (double)(sign * (((int)Math.Ceiling(Math.Abs(x)) + 1) / 2 * 2));
        }

        private static void Odd(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "ODD");
            var x = args.Parameters.D(0);
            var sign = x >= 0 ? 1 : -1;
            var abs = (int)Math.Ceiling(Math.Abs(x));
            args.Result = (double)(sign * (abs % 2 == 0 ? abs + 1 : abs));
        }

        // ── Arithmetic ──────────────────────────────────────────────────────────────────

        private static void Mod(FunctionArgs args)
        {
            args.Parameters.RequireArgs(2, "MOD");
            var divisor = args.Parameters.D(1);
            if (divisor == 0) throw new DivideByZeroException("MOD: divisor cannot be zero.");
            args.Result = args.Parameters.D(0) % divisor;
        }

        private static void Quotient(FunctionArgs args)
        {
            args.Parameters.RequireArgs(2, "QUOTIENT");
            var divisor = args.Parameters.D(1);
            if (divisor == 0) throw new DivideByZeroException("QUOTIENT: divisor cannot be zero.");
            args.Result = Math.Truncate(args.Parameters.D(0) / divisor);
        }

        private static void Product(FunctionArgs args)
        {
            var values = args.Parameters.AllD();
            if (values.Length == 0) throw new InvalidOperationException("PRODUCT requires at least one argument.");
            args.Result = values.Aggregate(1.0, (acc, v) => acc * v);
        }

        private static void Power(FunctionArgs args)
        {
            args.Parameters.RequireArgs(2, "POWER");
            args.Result = Math.Pow(args.Parameters.D(0), args.Parameters.D(1));
        }

        private static void Abs(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "ABS");
            args.Result = Math.Abs(args.Parameters.D(0));
        }

        private static void Sign(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "SIGN");
            args.Result = (double)Math.Sign(args.Parameters.D(0));
        }

        private static void Sqrt(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "SQRT");
            args.Result = Math.Sqrt(args.Parameters.D(0));
        }

        private static void SqrtPi(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "SQRTPI");
            args.Result = Math.Sqrt(args.Parameters.D(0) * Math.PI);
        }

        private static void Exp(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "EXP");
            args.Result = Math.Exp(args.Parameters.D(0));
        }

        private static void Ln(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "LN");
            args.Result = Math.Log(args.Parameters.D(0));
        }

        private static void Log(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "LOG");
            args.Result = args.Parameters.Length >= 2
                ? Math.Log(args.Parameters.D(0), args.Parameters.D(1))
                : Math.Log10(args.Parameters.D(0));
        }

        private static void Log10(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "LOG10");
            args.Result = Math.Log10(args.Parameters.D(0));
        }

        private static void Pi(FunctionArgs args) => args.Result = Math.PI;

        // ── Trigonometry ────────────────────────────────────────────────────────────────

        private static void Sin(FunctionArgs args)  { args.Parameters.RequireArgs(1, "SIN");  args.Result = Math.Sin(args.Parameters.D(0));  }
        private static void Cos(FunctionArgs args)  { args.Parameters.RequireArgs(1, "COS");  args.Result = Math.Cos(args.Parameters.D(0));  }
        private static void Tan(FunctionArgs args)  { args.Parameters.RequireArgs(1, "TAN");  args.Result = Math.Tan(args.Parameters.D(0));  }
        private static void Asin(FunctionArgs args) { args.Parameters.RequireArgs(1, "ASIN"); args.Result = Math.Asin(args.Parameters.D(0)); }
        private static void Acos(FunctionArgs args) { args.Parameters.RequireArgs(1, "ACOS"); args.Result = Math.Acos(args.Parameters.D(0)); }
        private static void Atan(FunctionArgs args) { args.Parameters.RequireArgs(1, "ATAN"); args.Result = Math.Atan(args.Parameters.D(0)); }

        private static void Atan2(FunctionArgs args)
        {
            args.Parameters.RequireArgs(2, "ATAN2");
            // Excel ATAN2(x, y) — note reversed argument order vs Math.Atan2(y, x)
            args.Result = Math.Atan2(args.Parameters.D(1), args.Parameters.D(0));
        }

        private static void Sinh(FunctionArgs args)  { args.Parameters.RequireArgs(1, "SINH");  args.Result = Math.Sinh(args.Parameters.D(0));  }
        private static void Cosh(FunctionArgs args)  { args.Parameters.RequireArgs(1, "COSH");  args.Result = Math.Cosh(args.Parameters.D(0));  }
        private static void Tanh(FunctionArgs args)  { args.Parameters.RequireArgs(1, "TANH");  args.Result = Math.Tanh(args.Parameters.D(0));  }
        private static void Asinh(FunctionArgs args) { args.Parameters.RequireArgs(1, "ASINH"); args.Result = Math.Asinh(args.Parameters.D(0)); }
        private static void Acosh(FunctionArgs args) { args.Parameters.RequireArgs(1, "ACOSH"); args.Result = Math.Acosh(args.Parameters.D(0)); }
        private static void Atanh(FunctionArgs args) { args.Parameters.RequireArgs(1, "ATANH"); args.Result = Math.Atanh(args.Parameters.D(0)); }

        private static void Degrees(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "DEGREES");
            args.Result = args.Parameters.D(0) * (180.0 / Math.PI);
        }

        private static void Radians(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "RADIANS");
            args.Result = args.Parameters.D(0) * (Math.PI / 180.0);
        }

        // ── Combinatorics ────────────────────────────────────────────────────────────────

        private static void Fact(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "FACT");
            var n = args.Parameters.I32(0);
            if (n < 0) throw new InvalidOperationException("FACT: argument must be non-negative.");
            args.Result = Factorial(n);
        }

        private static void FactDouble(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "FACTDOUBLE");
            var n = args.Parameters.I32(0);
            if (n < -1) throw new InvalidOperationException("FACTDOUBLE: argument must be >= -1.");
            if (n <= 0) { args.Result = 1.0; return; }
            double result = 1;
            for (var i = n; i > 0; i -= 2) result *= i;
            args.Result = result;
        }

        private static void Combin(FunctionArgs args)
        {
            args.Parameters.RequireArgs(2, "COMBIN");
            var n = args.Parameters.I32(0);
            var k = args.Parameters.I32(1);
            if (n < 0 || k < 0 || k > n) throw new InvalidOperationException("COMBIN: invalid arguments.");
            args.Result = Factorial(n) / (Factorial(k) * Factorial(n - k));
        }

        private static void Multinomial(FunctionArgs args)
        {
            var values = args.Parameters.AllD().Select(v => (int)v).ToArray();
            var sum = values.Sum();
            var result = Factorial(sum);
            foreach (var v in values)
                result /= Factorial(v);
            args.Result = result;
        }

        private static void Gcd(FunctionArgs args)
        {
            var values = args.Parameters.AllD().Select(v => (long)Math.Abs(v)).ToArray();
            args.Result = (double)values.Aggregate(Gcd);
        }

        private static void Lcm(FunctionArgs args)
        {
            var values = args.Parameters.AllD().Select(v => (long)Math.Abs(v)).ToArray();
            args.Result = (double)values.Aggregate(Lcm);
        }

        private static void Delta(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "DELTA");
            var a = args.Parameters.D(0);
            var b = args.Parameters.Length >= 2 ? args.Parameters.D(1) : 0.0;
            args.Result = a == b ? 1.0 : 0.0;
        }

        private static void GeStep(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "GESTEP");
            var number = args.Parameters.D(0);
            var step = args.Parameters.Length >= 2 ? args.Parameters.D(1) : 0.0;
            args.Result = number >= step ? 1.0 : 0.0;
        }

        // ── Randomness ──────────────────────────────────────────────────────────────────

        private static void Rand(FunctionArgs args)
        {
            args.Result = Rng.NextDouble();
        }

        private static void RandBetween(FunctionArgs args)
        {
            args.Parameters.RequireArgs(2, "RANDBETWEEN");
            var bottom = args.Parameters.I32(0);
            var top    = args.Parameters.I32(1);
            if (top < bottom) throw new InvalidOperationException("RANDBETWEEN: bottom must be <= top.");
            args.Result = (double)Rng.Next(bottom, top + 1);
        }

        // ── Series ──────────────────────────────────────────────────────────────────────

        private static void SeriesSum(FunctionArgs args)
        {
            // SERIESSUM(x, n, m, coefficients...)
            args.Parameters.RequireArgs(4, "SERIESSUM");
            var x = args.Parameters.D(0);
            var n = args.Parameters.D(1);
            var m = args.Parameters.D(2);
            double result = 0;
            for (var i = 3; i < args.Parameters.Length; i++)
                result += args.Parameters.D(i) * Math.Pow(x, n + (i - 3) * m);
            args.Result = result;
        }

        private static void Roman(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "ROMAN");
            var number = args.Parameters.I32(0);
            if (number < 0 || number > 3999)
                throw new InvalidOperationException("ROMAN: number must be between 0 and 3999.");
            if (number == 0) { args.Result = string.Empty; return; }

            var numerals = new[]
            {
                (1000,"M"),(900,"CM"),(500,"D"),(400,"CD"),(100,"C"),(90,"XC"),
                (50,"L"),(40,"XL"),(10,"X"),(9,"IX"),(5,"V"),(4,"IV"),(1,"I")
            };
            var sb = new System.Text.StringBuilder();
            foreach (var (val, sym) in numerals)
            {
                while (number >= val) { sb.Append(sym); number -= val; }
            }
            args.Result = sb.ToString();
        }

        // ── Private helpers ─────────────────────────────────────────────────────────────

        private static double Factorial(int n)
        {
            double result = 1;
            for (var i = 2; i <= n; i++) result *= i;
            return result;
        }

        private static long Gcd(long a, long b) => b == 0 ? a : Gcd(b, a % b);

        private static long Lcm(long a, long b) => a / Gcd(a, b) * b;
    }
}