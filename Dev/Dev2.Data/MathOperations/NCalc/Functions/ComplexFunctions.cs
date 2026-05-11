/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using NCalc;
using NCalc.Handlers;

namespace Dev2.MathOperations.NCalc.Functions
{
    /// <summary>
    /// Registers all complex-number NCalc function handlers (IMABS, IMSQRT, etc.).
    /// Uses <see cref="System.Numerics.Complex"/> — no third-party dependencies.
    /// </summary>
    internal static class ComplexFunctions
    {
        internal static void Register(IDictionary<string, Action<FunctionArgs>> handlers)
        {
            handlers["COMPLEX"]     = ComplexFn;
            handlers["IMABS"]       = ImAbs;
            handlers["IMAGINARY"]   = Imaginary;
            handlers["IMREAL"]      = ImReal;
            handlers["IMARGUMENT"]  = ImArgument;
            handlers["IMCONJUGATE"] = ImConjugate;
            handlers["IMCOS"]       = ImCos;
            handlers["IMSIN"]       = ImSin;
            handlers["IMDIV"]       = ImDiv;
            handlers["IMPRODUCT"]   = ImProduct;
            handlers["IMSUM"]       = ImSum;
            handlers["IMSUB"]       = ImSub;
            handlers["IMEXP"]       = ImExp;
            handlers["IMLN"]        = ImLn;
            handlers["IMLOG10"]     = ImLog10;
            handlers["IMLOG2"]      = ImLog2;
            handlers["IMSQRT"]      = ImSqrt;
            handlers["IMPOWER"]     = ImPower;
        }

        // ── Construction ─────────────────────────────────────────────────────────────────

        private static void ComplexFn(FunctionArgs args)
        {
            args.Parameters.RequireArgs(2, "COMPLEX");
            var real = args.Parameters.D(0);
            var imag = args.Parameters.D(1);
            args.Result = Format(new Complex(real, imag));
        }

        // ── Decomposition ────────────────────────────────────────────────────────────────

        private static void ImAbs(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "IMABS");
            args.Result = Complex.Abs(Parse(args.Parameters.S(0)));
        }

        private static void Imaginary(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "IMAGINARY");
            args.Result = Parse(args.Parameters.S(0)).Imaginary;
        }

        private static void ImReal(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "IMREAL");
            args.Result = Parse(args.Parameters.S(0)).Real;
        }

        private static void ImArgument(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "IMARGUMENT");
            var c = Parse(args.Parameters.S(0));
            if (c == Complex.Zero) throw new InvalidOperationException("IMARGUMENT: argument is zero.");
            args.Result = Complex.Abs(c) == 0 ? 0d : Math.Atan2(c.Imaginary, c.Real);
        }

        private static void ImConjugate(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "IMCONJUGATE");
            args.Result = Format(Complex.Conjugate(Parse(args.Parameters.S(0))));
        }

        // ── Trigonometry ────────────────────────────────────────────────────────────────

        private static void ImCos(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "IMCOS");
            args.Result = Format(Complex.Cos(Parse(args.Parameters.S(0))));
        }

        private static void ImSin(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "IMSIN");
            args.Result = Format(Complex.Sin(Parse(args.Parameters.S(0))));
        }

        // ── Arithmetic ───────────────────────────────────────────────────────────────────

        private static void ImDiv(FunctionArgs args)
        {
            args.Parameters.RequireArgs(2, "IMDIV");
            var divisor = Parse(args.Parameters.S(1));
            if (divisor == Complex.Zero) throw new DivideByZeroException("IMDIV: divisor is zero.");
            args.Result = Format(Parse(args.Parameters.S(0)) / divisor);
        }

        private static void ImProduct(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "IMPRODUCT");
            var result = Complex.One;
            foreach (var p in args.Parameters)
                result *= Parse(p.Evaluate()?.ToString() ?? "0");
            args.Result = Format(result);
        }

        private static void ImSum(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "IMSUM");
            var result = Complex.Zero;
            foreach (var p in args.Parameters)
                result += Parse(p.Evaluate()?.ToString() ?? "0");
            args.Result = Format(result);
        }

        private static void ImSub(FunctionArgs args)
        {
            args.Parameters.RequireArgs(2, "IMSUB");
            args.Result = Format(Parse(args.Parameters.S(0)) - Parse(args.Parameters.S(1)));
        }

        // ── Exponential / logarithm ──────────────────────────────────────────────────────

        private static void ImExp(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "IMEXP");
            args.Result = Format(Complex.Exp(Parse(args.Parameters.S(0))));
        }

        private static void ImLn(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "IMLN");
            var z = Parse(args.Parameters.S(0));
            // Use Math.Log for pure real inputs to avoid precision loss in complex division
            if (z.Imaginary == 0)
            {
                args.Result = Math.Log(z.Real).ToString("G15", CultureInfo.InvariantCulture);
                return;
            }
            args.Result = Format(Complex.Log(z));
        }

        private static void ImLog10(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "IMLOG10");
            var z = Parse(args.Parameters.S(0));
            // Use Math.Log10 for pure real inputs to avoid precision loss in complex division
            if (z.Imaginary == 0)
            {
                args.Result = Math.Log10(z.Real).ToString("G15", CultureInfo.InvariantCulture);
                return;
            }
            args.Result = Format(Complex.Log10(z));
        }

        private static void ImLog2(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "IMLOG2");
            var z = Parse(args.Parameters.S(0));
            // Use Math.Log for pure real inputs to avoid precision loss in complex division
            if (z.Imaginary == 0)
            {
                args.Result = Math.Log2(z.Real).ToString("G15", CultureInfo.InvariantCulture);
                return;
            }
            args.Result = Format(Complex.Log(z, 2));
        }

        private static void ImSqrt(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "IMSQRT");
            // Accept both numeric (real) and complex-string inputs
            var raw = args.Parameters[0].Evaluate();
            Complex c;
            if (raw is string s)
                c = Parse(s);
            else
                c = new Complex(Convert.ToDouble(raw, CultureInfo.InvariantCulture), 0);
            args.Result = Format(Complex.Sqrt(c));
        }

        private static void ImPower(FunctionArgs args)
        {
            args.Parameters.RequireArgs(2, "IMPOWER");
            var number = Parse(args.Parameters.S(0));
            var power  = args.Parameters.D(1);
            args.Result = Format(Complex.Pow(number, power));
        }

        // ── Public helpers (used by FunctionEvaluator) ───────────────────────────────────

        /// <summary>
        /// Formats a <see cref="Complex"/> number as an Excel-style string ("a+bi", "bi", "a", "i", "-i", etc.).
        /// </summary>
        internal static string Format(Complex c)
        {
            if (c.Imaginary == 0) return c.Real.ToString("G15", CultureInfo.InvariantCulture);

            var img = c.Imaginary == 1  ? "i"
                    : c.Imaginary == -1 ? "-i"
                    : c.Imaginary.ToString("G15", CultureInfo.InvariantCulture) + "i";

            if (c.Real == 0) return img;
            return c.Imaginary < 0
                ? $"{c.Real.ToString("G15", CultureInfo.InvariantCulture)}{img}"
                : $"{c.Real.ToString("G15", CultureInfo.InvariantCulture)}+{img}";
        }

        /// <summary>
        /// Parses an Excel complex-number string ("3+4i", "2i", "-1", "i", "-3-2i") into a <see cref="Complex"/>.
        /// </summary>
        internal static Complex Parse(string s)
        {
            s = s.Trim();
            if (string.IsNullOrEmpty(s)) return Complex.Zero;

            // Pure real
            if (double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var real))
                return new Complex(real, 0);

            // Pure imaginary: "i", "-i", "3i", "-3i"
            if (s.EndsWith("i", StringComparison.OrdinalIgnoreCase))
            {
                var body = s[..^1];
                if (body is "" or "+") return new Complex(0, 1);
                if (body == "-") return new Complex(0, -1);
                if (double.TryParse(body, NumberStyles.Any, CultureInfo.InvariantCulture, out var imOnly))
                    return new Complex(0, imOnly);
            }

            // General form: split on last +/- that is not the leading sign
            for (var i = s.Length - 1; i > 0; i--)
            {
                if ((s[i] == '+' || s[i] == '-') && s[i - 1] != 'e' && s[i - 1] != 'E')
                {
                    var realPart = s[..i];
                    var imagPart = s[i..^1]; // strip trailing 'i'
                    if (double.TryParse(realPart, NumberStyles.Any, CultureInfo.InvariantCulture, out var r) &&
                        (imagPart is "" or "+" ? true :
                         imagPart == "-" ? true :
                         double.TryParse(imagPart, NumberStyles.Any, CultureInfo.InvariantCulture, out _)))
                    {
                        double im;
                        if (imagPart is "" or "+") im = 1;
                        else if (imagPart == "-") im = -1;
                        else double.TryParse(imagPart, NumberStyles.Any, CultureInfo.InvariantCulture, out im);
                        return new Complex(r, im);
                    }
                }
            }

            throw new InvalidOperationException($"Cannot parse complex number: '{s}'");
        }
    }
}