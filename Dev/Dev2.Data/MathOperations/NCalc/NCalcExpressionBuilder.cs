/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Dev2.MathOperations.NCalc.Functions;
using NCalc;
using NCalc.Handlers;

namespace Dev2.MathOperations.NCalc
{
    /// <summary>
    /// Central factory for NCalc <see cref="Expression"/> objects pre-wired with all custom function handlers.
    /// <para>
    /// The handler registry is built exactly once (static initializer) and shared across all threads —
    /// ideal for Azure Function hot-path where minimising per-request allocation is critical.
    /// Each call to <see cref="Build"/> allocates only a single <see cref="Expression"/> instance.
    /// </para>
    /// </summary>
    internal static class NCalcExpressionBuilder
    {
        /// <summary>
        /// Pre-compiled regex to replace Excel single-ampersand concatenation (<c>&amp;</c>) with
        /// NCalc addition (<c>+</c>). Double-ampersand logical AND (<c>&amp;&amp;</c>) is preserved.
        /// </summary>
        private static readonly Regex ConcatRegex =
            new(@"(?<![&])&(?![&])", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        /// <summary>
        /// Pre-compiled regex to replace <c>TRUE()</c> / <c>FALSE()</c> function-call syntax
        /// with NCalc-native boolean literals <c>true</c> / <c>false</c>.
        /// </summary>
        private static readonly Regex TrueRegex =
            new(@"\bTRUE\(\)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex FalseRegex =
            new(@"\bFALSE\(\)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// Static registry of all custom function handlers, keyed case-insensitively.
        /// Built once per AppDomain lifetime — zero allocation on every subsequent invocation.
        /// </summary>
        private static readonly IReadOnlyDictionary<string, Action<FunctionArgs>> Handlers;

        static NCalcExpressionBuilder()
        {
            var map = new Dictionary<string, Action<FunctionArgs>>(
                StringComparer.OrdinalIgnoreCase);

            MathFunctions.Register(map);
            StringFunctions.Register(map);
            DateTimeFunctions.Register(map);
            FinancialFunctions.Register(map);
            BaseConversionFunctions.Register(map);
            ComplexFunctions.Register(map);
            InformationFunctions.Register(map);

            Handlers = map;
        }

        /// <summary>
        /// Builds a ready-to-evaluate <see cref="Expression"/> for the given formula string.
        /// </summary>
        /// <param name="expression">Raw formula (may contain Excel-style <c>&amp;</c> concatenation).</param>
        /// <returns>
        /// A new <see cref="Expression"/> with <see cref="ExpressionOptions.IgnoreCaseAtBuiltInFunctions"/>
        /// and all custom handlers attached via the <c>EvaluateFunction</c> event.
        /// </returns>
        internal static Expression Build(string expression)
        {
            var processed = ConcatRegex.Replace(expression, "+");
            processed = TrueRegex.Replace(processed, "true");
            processed = FalseRegex.Replace(processed, "false");
            var expr = new Expression(processed, ExpressionOptions.IgnoreCaseAtBuiltInFunctions);
            expr.EvaluateFunction += HandleFunction;
            return expr;
        }

        /// <summary>
        /// Dispatches a function name to its registered handler.
        /// O(1) dictionary lookup — no switch/if chain on the hot path.
        /// Unrecognised names are left unhandled so NCalc2 can attempt its own built-in resolution.
        /// </summary>
        private static void HandleFunction(string name, FunctionArgs args)
        {
            if (Handlers.TryGetValue(name, out var handler))
                handler(args);
            // else: let NCalc fall through to its own built-ins (e.g. if, abs, sqrt, …)
        }
    }
}