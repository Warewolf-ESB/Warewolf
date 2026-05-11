/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using System;
using System.Collections.Generic;
using NCalc;
using NCalc.Handlers;

namespace Dev2.MathOperations.NCalc.Functions
{
    /// <summary>
    /// Registers all base-conversion NCalc function handlers (DEC2BIN, HEX2DEC, etc.).
    /// Mirrors the Infragistics engineering/conversion function set.
    /// </summary>
    internal static class BaseConversionFunctions
    {
        internal static void Register(IDictionary<string, Action<FunctionArgs>> handlers)
        {
            handlers["DEC2BIN"] = Dec2Bin;
            handlers["DEC2HEX"] = Dec2Hex;
            handlers["DEC2OCT"] = Dec2Oct;
            handlers["BIN2DEC"] = Bin2Dec;
            handlers["BIN2HEX"] = Bin2Hex;
            handlers["BIN2OCT"] = Bin2Oct;
            handlers["HEX2BIN"] = Hex2Bin;
            handlers["HEX2DEC"] = Hex2Dec;
            handlers["HEX2OCT"] = Hex2Oct;
            handlers["OCT2BIN"] = Oct2Bin;
            handlers["OCT2DEC"] = Oct2Dec;
            handlers["OCT2HEX"] = Oct2Hex;
        }

        // ── Decimal → other bases ────────────────────────────────────────────────────────

        private static void Dec2Bin(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "DEC2BIN");
            var n = (long)args.Parameters.D(0);
            // Excel supports 10-bit signed: -512 to 511
            if (n < -512 || n > 511) throw new InvalidOperationException("DEC2BIN: number out of range (-512 to 511).");
            var result = n < 0 ? Convert.ToString(n & 0x3FF, 2) : Convert.ToString(n, 2);
            if (args.Parameters.Length >= 2)
                result = result.PadLeft(args.Parameters.I32(1), '0');
            args.Result = result;
        }

        private static void Dec2Hex(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "DEC2HEX");
            var n = (long)args.Parameters.D(0);
            var result = n < 0
                ? Convert.ToString(n & 0xFFFFFFFFFL, 16).ToUpperInvariant()
                : Convert.ToString(n, 16).ToUpperInvariant();
            if (args.Parameters.Length >= 2)
                result = result.PadLeft(args.Parameters.I32(1), '0');
            args.Result = result;
        }

        private static void Dec2Oct(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "DEC2OCT");
            var n = (long)args.Parameters.D(0);
            var result = n < 0
                ? Convert.ToString(n & 0x3FFFFFFFL, 8)
                : Convert.ToString(n, 8);
            if (args.Parameters.Length >= 2)
                result = result.PadLeft(args.Parameters.I32(1), '0');
            args.Result = result;
        }

        // ── Binary → other bases ─────────────────────────────────────────────────────────

        private static void Bin2Dec(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "BIN2DEC");
            var s = args.Parameters.S(0).TrimStart();
            var n = Convert.ToInt64(s, 2);
            // 10-bit sign extension
            if (s.Length == 10 && s[0] == '1')
                n -= 1024;
            args.Result = (double)n;
        }

        private static void Bin2Hex(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "BIN2HEX");
            var dec = BinToDec(args.Parameters.S(0));
            args.Result = dec < 0
                ? Convert.ToString(dec & 0xFFFFFFFFFL, 16).ToUpperInvariant()
                : Convert.ToString(dec, 16).ToUpperInvariant();
        }

        private static void Bin2Oct(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "BIN2OCT");
            var dec = BinToDec(args.Parameters.S(0));
            args.Result = dec < 0
                ? Convert.ToString(dec & 0x3FFFFFFFL, 8)
                : Convert.ToString(dec, 8);
        }

        // ── Hexadecimal → other bases ────────────────────────────────────────────────────

        private static void Hex2Bin(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "HEX2BIN");
            var n = HexToSignedLong(args.Parameters.S(0));
            args.Result = n < 0
                ? Convert.ToString(n & 0x3FF, 2)
                : Convert.ToString(n, 2);
        }

        private static void Hex2Dec(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "HEX2DEC");
            args.Result = (double)HexToSignedLong(args.Parameters.S(0));
        }

        private static void Hex2Oct(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "HEX2OCT");
            var n = HexToSignedLong(args.Parameters.S(0));
            args.Result = n < 0
                ? Convert.ToString(n & 0x3FFFFFFFL, 8)
                : Convert.ToString(n, 8);
        }

        // ── Octal → other bases ───────────────────────────────────────────────────────────

        private static void Oct2Bin(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "OCT2BIN");
            var n = OctToSignedLong(args.Parameters.S(0));
            var result = n < 0
                ? Convert.ToString(n & 0x3FF, 2)
                : Convert.ToString(n, 2);
            if (args.Parameters.Length >= 2)
                result = result.PadLeft(args.Parameters.I32(1), '0');
            args.Result = result;
        }

        private static void Oct2Dec(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "OCT2DEC");
            args.Result = (double)OctToSignedLong(args.Parameters.S(0));
        }

        private static void Oct2Hex(FunctionArgs args)
        {
            args.Parameters.RequireArgs(1, "OCT2HEX");
            var n = OctToSignedLong(args.Parameters.S(0));
            var result = n < 0
                ? Convert.ToString(n & 0xFFFFFFFFFL, 16).ToUpperInvariant()
                : Convert.ToString(n, 16).ToUpperInvariant();
            if (args.Parameters.Length >= 2)
                result = result.PadLeft(args.Parameters.I32(1), '0');
            args.Result = result;
        }

        // ── Private helpers ─────────────────────────────────────────────────────────────

        private static long BinToDec(string s)
        {
            s = s.TrimStart();
            var n = Convert.ToInt64(s, 2);
            if (s.Length == 10 && s[0] == '1') n -= 1024;
            return n;
        }

        private static long HexToSignedLong(string s)
        {
            s = s.TrimStart().ToUpperInvariant();
            // 10 hex digit = 40-bit; top bit set → negative
            var n = Convert.ToInt64(s, 16);
            if (s.Length == 10 && (n & 0x8000000000L) != 0)
                n -= 0x10000000000L;
            return n;
        }

        private static long OctToSignedLong(string s)
        {
            s = s.TrimStart();
            var n = Convert.ToInt64(s, 8);
            // 10 octal digit = 30-bit; top bit set → negative
            if (s.Length == 10 && (n & 0x1000000000L) != 0)
                n -= 0x2000000000L;
            return n;
        }
    }
}