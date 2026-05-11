/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NCalc;
using NCalc.Handlers;

namespace Dev2.MathOperations.NCalc.Functions
{
    /// <summary>
    /// Registers logical, information, and conditional NCalc function handlers.
    /// AND/OR/NOT/IFERROR/CHOOSE/IS-family all live here.
    /// </summary>
    internal static class InformationFunctions
    {
        internal static void Register(IDictionary<string, Action<FunctionArgs>> handlers)
        {
            // ── Logical ──────────────────────────────────────────────────────────────────
            handlers["AND"]        = And;
            handlers["OR"]         = Or;
            handlers["NOT"]        = Not;
            handlers["TRUE"]       = TrueFn;
            handlers["FALSE"]      = FalseFn;

            // ── Conditional ──────────────────────────────────────────────────────────────
            handlers["IF"]         = IfFn;
            handlers["IFERROR"]    = IfError;
            handlers["CHOOSE"]     = Choose;

            // ── Type / information ───────────────────────────────────────────────────────
            handlers["TYPE"]       = TypeFn;
            handlers["N"]          = N;
            handlers["NA"]         = Na;
            handlers["INFO"]       = Info;

            // ── IS-family ────────────────────────────────────────────────────────────────
            handlers["ISBLANK"]    = IsBlank;
            handlers["ISERR"]      = IsErr;
            handlers["ISLOGICAL"]  = IsLogical;
            handlers["ISNA"]       = IsNa;
            handlers["ISNONTEXT"]  = IsNonText;
            handlers["ISNUMBER"]   = IsNumber;
            handlers["ISREF"]      = IsRef;
            handlers["ISTEXT"]     = IsText;
            handlers["ISODD"]      = IsOdd;
            handlers["ISEVEN"]     = IsEven;
            handlers["ISNULL"]     = IsNull;
            handlers["ISDBNULL"]   = IsDbNull;
            handlers["NULL"]       = NullFn;
            handlers["DBNULL"]     = DbNullFn;
        }

        // ── Logical ──────────────────────────────────────────────────────────────────────

        private static void And(FunctionArgs args)
        {
            // Short-circuit: stops on first false
            args.Result = args.Parameters.All(p => Convert.ToBoolean(p.Evaluate()));
        }

        private static void Or(FunctionArgs args)
        {
            // Short-circuit: stops on first true
            args.Result = args.Parameters.Any(p => Convert.ToBoolean(p.Evaluate()));
        }

        private static void Not(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "NOT");
            args.Result = !Convert.ToBoolean(args.Parameters[0].Evaluate());
        }

        private static void TrueFn(FunctionArgs args)  => args.Result = true;
        private static void FalseFn(FunctionArgs args) => args.Result = false;

        // ── Conditional ──────────────────────────────────────────────────────────────────

        private static void IfFn(FunctionArgs args)
        {
            // IF(condition[, value_if_true[, value_if_false]])
            args.Parameters.RequireArgs(1, "IF");
            bool condition;
            try { condition = Convert.ToBoolean(args.Parameters[0].Evaluate()); }
            catch { condition = false; }

            if (condition)
            {
                args.Result = args.Parameters.Length >= 2 ? args.Parameters[1].Evaluate() : true;
            }
            else
            {
                args.Result = args.Parameters.Length >= 3 ? args.Parameters[2].Evaluate() : false;
            }
        }

        private static void IfError(FunctionArgs args)
        {
            args.Parameters.RequireArgs(2, "IFERROR");
            try
            {
                var val = args.Parameters[0].Evaluate();
                if (val is double d && (double.IsInfinity(d) || double.IsNaN(d)))
                {
                    args.Result = args.Parameters[1].Evaluate();
                }
                else
                {
                    args.Result = val;
                }
            }
            catch
            {
                args.Result = args.Parameters[1].Evaluate();
            }
        }

        private static void Choose(FunctionArgs args)
        {
            // CHOOSE(index_num, value1, value2, ...) — 1-based index
            args.Parameters.RequireArgs(2, "CHOOSE");
            var index = args.Parameters.I32(0);
            if (index < 1 || index >= args.Parameters.Length)
                throw new InvalidOperationException(
                    $"CHOOSE: index {index} is out of range (1–{args.Parameters.Length - 1}).");
            args.Result = args.Parameters[index].Evaluate();
        }

        // ── Type / information ────────────────────────────────────────────────────────────

        private static void TypeFn(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "TYPE");
            var val = args.Parameters[0].Evaluate();
            // Excel TYPE: 1=number, 2=text, 4=logical, 8=formula, 16=error, 64=array
            args.Result = val switch
            {
                null                            => 1d,
                bool                            => 4d,
                string                          => 2d,
                double or float or int or long  => 1d,
                _                               => double.TryParse(
                    val.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out _) ? 1d : 2d
            };
        }

        private static void N(FunctionArgs args)
        {
            // N(value) — converts to number; logical true=1, false=0, date=serial, text=0
            args.Parameters.RequireArgs(1, "N");
            var val = args.Parameters[0].Evaluate();
            args.Result = val switch
            {
                null    => 0d,
                bool b  => b ? 1d : 0d,
                DateTime dt => dt.ToOADate(),
                string s when double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var d) => d,
                string  => 0d,
                _       => Convert.ToDouble(val, CultureInfo.InvariantCulture)
            };
        }

        private static void Na(FunctionArgs args)
        {
            throw new InvalidOperationException("NA: #N/A error.");
        }

        private static void Info(FunctionArgs args)
        {
            // Minimal INFO() — returns environment info strings
            args.Parameters.RequireArgs(1, "INFO");
            var type = args.Parameters.S(0).ToUpperInvariant();
            args.Result = type switch
            {
                "OSVERSION"  => System.Runtime.InteropServices.RuntimeInformation.OSDescription,
                "NUMFILE"    => "1",
                "DIRECTORY"  => System.IO.Directory.GetCurrentDirectory(),
                "RELEASE"    => System.Environment.Version.ToString(),
                "SYSTEM"     => System.Runtime.InteropServices.RuntimeInformation.OSDescription,
                _ => throw new InvalidOperationException($"INFO: unknown type '{type}'.")
            };
        }

        // ── IS-family ─────────────────────────────────────────────────────────────────────

        private static void IsBlank(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "ISBLANK");
            var val = args.Parameters[0].Evaluate();
            args.Result = val == null || val.ToString() == string.Empty;
        }

        private static void IsErr(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "ISERR");
            try { args.Parameters[0].Evaluate(); args.Result = false; }
            catch (Exception ex) when (ex.Message != "NA: #N/A error.") { args.Result = true; }
            catch { args.Result = false; }
        }

        private static void IsLogical(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "ISLOGICAL");
            var val = args.Parameters[0].Evaluate();
            args.Result = val is bool;
        }

        private static void IsNa(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "ISNA");
            try { args.Parameters[0].Evaluate(); args.Result = false; }
            catch (Exception ex) { args.Result = ex.Message == "NA: #N/A error."; }
        }

        private static void IsNonText(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "ISNONTEXT");
            var val = args.Parameters[0].Evaluate();
            args.Result = val is not string;
        }

        private static void IsNumber(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "ISNUMBER");
            var val = args.Parameters[0].Evaluate();
            args.Result = val is double or float or int or long or decimal
                || (val is string s && double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out _));
        }

        private static void IsRef(FunctionArgs args)
        {
            // NCalc has no cell references — always false
            args.Result = false;
        }

        private static void IsText(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "ISTEXT");
            args.Result = args.Parameters[0].Evaluate() is string;
        }

        private static void IsOdd(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "ISODD");
            var n = (long)Convert.ToDouble(args.Parameters[0].Evaluate(), CultureInfo.InvariantCulture);
            args.Result = n % 2 != 0;
        }

        private static void IsEven(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "ISEVEN");
            var n = (long)Convert.ToDouble(args.Parameters[0].Evaluate(), CultureInfo.InvariantCulture);
            args.Result = n % 2 == 0;
        }

        private static void IsNull(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "ISNULL");
            args.Result = args.Parameters[0].Evaluate() == null;
        }

        private static void IsDbNull(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "ISDBNULL");
            var val = args.Parameters[0].Evaluate();
            args.Result = val == null || val == DBNull.Value;
        }

        private static void NullFn(FunctionArgs args)   => args.Result = null;
        private static void DbNullFn(FunctionArgs args) => args.Result = DBNull.Value;
    }
}