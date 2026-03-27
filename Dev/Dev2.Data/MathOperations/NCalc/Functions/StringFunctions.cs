/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using NCalc;
using NCalc.Handlers;

namespace Dev2.MathOperations.NCalc.Functions
{
    /// <summary>
    /// Registers all string / text NCalc function handlers.
    /// Mirrors the Infragistics UltraCalcFunction string set for cross-platform parity.
    /// </summary>
    internal static class StringFunctions
    {
        internal static void Register(IDictionary<string, Action<FunctionArgs>> handlers)
        {
            handlers["LEFT"]        = Left;
            handlers["RIGHT"]       = Right;
            handlers["MID"]         = Mid;
            handlers["LEN"]         = Len;
            handlers["TRIM"]        = Trim;
            handlers["LOWER"]       = Lower;
            handlers["UPPER"]       = Upper;
            handlers["CONCATENATE"] = Concatenate;
            handlers["VALUE"]       = Value;
            handlers["TEXT"]        = Text;
            handlers["REPLACE"]     = Replace;
            handlers["FIND"]        = Find;
            handlers["SEARCH"]      = Search;
            handlers["SEARCHB"]     = Search;   // SEARCHB shares logic; bytes ≡ chars for UTF-16
            handlers["CHAR"]        = Char;
            handlers["CODE"]        = Code;
            handlers["REPT"]        = Rept;
            handlers["FIXED"]       = Fixed;
            handlers["ISERROR"]     = IsError;
            handlers["EXACT"]       = Exact;
            handlers["PROPER"]      = Proper;
            handlers["SUBSTITUTE"]  = Substitute;
            handlers["T"]           = T;
            handlers["CLEAN"]       = Clean;
        }

        // ── Extraction ───────────────────────────────────────────────────────────────────

        private static void Left(FunctionArgs args)
        {
            args.Parameters.RequireArgs(2, "LEFT");
            var s   = args.Parameters.S(0);
            var len = Math.Min(args.Parameters.I32(1), s.Length);
            args.Result = s.Substring(0, Math.Max(0, len));
        }

        private static void Right(FunctionArgs args)
        {
            args.Parameters.RequireArgs(2, "RIGHT");
            var s   = args.Parameters.S(0);
            var len = Math.Min(args.Parameters.I32(1), s.Length);
            args.Result = s.Substring(s.Length - Math.Max(0, len));
        }

        private static void Mid(FunctionArgs args)
        {
            args.Parameters.RequireArgs(3, "MID");
            var s     = args.Parameters.S(0);
            var start = args.Parameters.I32(1) - 1;   // 1-based → 0-based
            var len   = args.Parameters.I32(2);
            if (start >= s.Length) { args.Result = string.Empty; return; }
            start = Math.Max(0, start);
            args.Result = s.Substring(start, Math.Min(len, s.Length - start));
        }

        // ── Metrics ──────────────────────────────────────────────────────────────────────

        private static void Len(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "LEN");
            args.Result = (double)args.Parameters.S(0).Length;
        }

        // ── Case / whitespace ────────────────────────────────────────────────────────────

        private static void Trim(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "TRIM");
            // Excel TRIM: remove leading/trailing and collapse internal runs to single space
            var words = args.Parameters.S(0).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            args.Result = string.Join(" ", words);
        }

        private static void Lower(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "LOWER");
            args.Result = args.Parameters.S(0).ToLowerInvariant();
        }

        private static void Upper(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "UPPER");
            args.Result = args.Parameters.S(0).ToUpperInvariant();
        }

        private static void Proper(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "PROPER");
            args.Result = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(
                args.Parameters.S(0).ToLowerInvariant());
        }

        private static void Clean(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "CLEAN");
            var sb = new StringBuilder();
            foreach (var ch in args.Parameters.S(0))
                if (ch >= 32) sb.Append(ch);
            args.Result = sb.ToString();
        }

        // ── Combining ────────────────────────────────────────────────────────────────────

        private static void Concatenate(FunctionArgs args)
        {
            var sb = new StringBuilder();
            foreach (var p in args.Parameters)
                sb.Append(p.Evaluate()?.ToString() ?? string.Empty);
            args.Result = sb.ToString();
        }

        // ── Conversion ───────────────────────────────────────────────────────────────────

        private static void Value(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "VALUE");
            var s = args.Parameters.S(0);
            if (double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
                args.Result = d;
            else if (double.TryParse(s, NumberStyles.Any, CultureInfo.CurrentCulture, out var d2))
                args.Result = d2;
            else
                throw new InvalidOperationException($"VALUE: '{s}' cannot be converted to a number.");
        }

        private static void Text(FunctionArgs args)
        {
            args.Parameters.RequireArgs(2, "TEXT");
            var value  = args.Parameters.D(0);
            var format = args.Parameters.S(1);
            args.Result = value.ToString(format, CultureInfo.InvariantCulture);
        }

        private static void Fixed(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "FIXED");
            var number   = args.Parameters.D(0);
            var decimals = args.Parameters.Length >= 2 ? args.Parameters.I32(1) : 2;
            var noCommas = args.Parameters.Length >= 3 && args.Parameters.Bool(2);
            var rounded  = Math.Round(number, decimals, MidpointRounding.AwayFromZero);
            var formatted = rounded.ToString("F" + decimals, CultureInfo.InvariantCulture);

            if (!noCommas)
            {
                var parts   = formatted.Split('.');
                var intPart = long.Parse(parts[0], CultureInfo.InvariantCulture);
                var sep     = intPart.ToString("N0", CultureInfo.InvariantCulture);
                formatted   = parts.Length > 1 ? sep + "." + parts[1] : sep;
            }
            args.Result = formatted;
        }

        private static void T(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "T");
            var val = args.Parameters[0].Evaluate();
            args.Result = val is string s ? s : string.Empty;
        }

        // ── Search / replace ─────────────────────────────────────────────────────────────

        private static void Find(FunctionArgs args)
        {
            // FIND(needle, haystack[, start]) — case-sensitive, 1-based
            args.Parameters.RequireArgs(2, "FIND");
            var needle   = args.Parameters.S(0);
            var haystack = args.Parameters.S(1);
            var start    = args.Parameters.Length >= 3 ? args.Parameters.I32(2) - 1 : 0;
            var idx      = haystack.IndexOf(needle, start, StringComparison.Ordinal);
            if (idx < 0) throw new InvalidOperationException($"FIND: '{needle}' was not found.");
            args.Result  = (double)(idx + 1);
        }

        private static void Search(FunctionArgs args)
        {
            // SEARCH(needle, haystack[, start]) — case-insensitive, 1-based
            args.Parameters.RequireArgs(2, "SEARCH");
            var needle   = args.Parameters.S(0);
            var haystack = args.Parameters.S(1);
            var start    = args.Parameters.Length >= 3 ? args.Parameters.I32(2) - 1 : 0;
            var idx      = haystack.IndexOf(needle, start, StringComparison.OrdinalIgnoreCase);
            if (idx < 0) throw new InvalidOperationException($"SEARCH: '{needle}' was not found.");
            args.Result  = (double)(idx + 1);
        }

        private static void Replace(FunctionArgs args)
        {
            // REPLACE(old_text, start_num, num_chars, new_text) — 1-based
            args.Parameters.RequireArgs(4, "REPLACE");
            var s       = args.Parameters.S(0);
            var start   = args.Parameters.I32(1) - 1;
            var numChar = args.Parameters.I32(2);
            var newText = args.Parameters.S(3);
            start   = Math.Max(0, Math.Min(start, s.Length));
            numChar = Math.Min(numChar, s.Length - start);
            args.Result = s.Remove(start, numChar).Insert(start, newText);
        }

        private static void Substitute(FunctionArgs args)
        {
            // SUBSTITUTE(text, old_text, new_text[, instance_num])
            args.Parameters.RequireArgs(3, "SUBSTITUTE");
            var text    = args.Parameters.S(0);
            var oldText = args.Parameters.S(1);
            var newText = args.Parameters.S(2);

            if (args.Parameters.Length >= 4)
            {
                var instance = args.Parameters.I32(3);
                var count = 0;
                var idx = 0;
                while ((idx = text.IndexOf(oldText, idx, StringComparison.Ordinal)) >= 0)
                {
                    count++;
                    if (count == instance)
                    {
                        args.Result = text.Remove(idx, oldText.Length).Insert(idx, newText);
                        return;
                    }
                    idx += oldText.Length;
                }
                args.Result = text;
            }
            else
            {
                args.Result = text.Replace(oldText, newText, StringComparison.Ordinal);
            }
        }

        // ── Character codes ──────────────────────────────────────────────────────────────

        private static void Char(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "CHAR");
            var code = args.Parameters.I32(0);
            if (code < 1 || code > 255) throw new InvalidOperationException($"CHAR: code {code} is out of range (1-255).");
            args.Result = ((char)code).ToString();
        }

        private static void Code(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "CODE");
            var s = args.Parameters.S(0);
            if (s.Length == 0) throw new InvalidOperationException("CODE: text is empty.");
            args.Result = (double)s[0];
        }

        private static void Rept(FunctionArgs args)
        {
            args.Parameters.RequireArgs(2, "REPT");
            var text  = args.Parameters.S(0);
            var times = args.Parameters.I32(1);
            if (times < 0) throw new InvalidOperationException("REPT: number_times must be non-negative.");
            var sb = new StringBuilder(text.Length * times);
            for (var i = 0; i < times; i++) sb.Append(text);
            args.Result = sb.ToString();
        }

        // ── Error-checking ───────────────────────────────────────────────────────────────

        private static void IsError(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "ISERROR");
            try { args.Parameters[0].Evaluate(); args.Result = false; }
            catch { args.Result = true; }
        }

        // ── Exact comparison ─────────────────────────────────────────────────────────────

        private static void Exact(FunctionArgs args)
        {
            args.Parameters.RequireArgs(2, "EXACT");
            args.Result = string.Equals(args.Parameters.S(0), args.Parameters.S(1), StringComparison.Ordinal);
        }
    }
}