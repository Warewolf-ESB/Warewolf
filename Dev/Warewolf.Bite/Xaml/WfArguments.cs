/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Linq;

namespace Warewolf.Bite.Xaml
{
    /// <summary>
    /// Builds WF <c>InArgument</c>/<c>OutArgument</c>/<c>InOutArgument</c> values from their XAML attribute text, by
    /// reflection so this assembly has no compile-time WF reference. Goes away with Workstream 2 (T2.4), when these
    /// properties become plain types.
    ///
    /// <list type="bullet">
    /// <item><c>[expr]</c> is a VB expression: kept as text in a <c>VisualBasicValue</c>/<c>VisualBasicReference</c>, never compiled.</item>
    /// <item><c>%text</c> escapes a literal that would otherwise read as an expression: the <c>%</c> is dropped.</item>
    /// <item>Anything else is a <c>Literal</c> of the converted value.</item>
    /// </list>
    /// </summary>
    static class WfArguments
    {
        const string InArgument = "System.Activities.InArgument`1";
        const string OutArgument = "System.Activities.OutArgument`1";
        const string InOutArgument = "System.Activities.InOutArgument`1";

        public static bool IsArgumentType(Type type) =>
            type.IsGenericType && type.GetGenericTypeDefinition().FullName is InArgument or OutArgument or InOutArgument;

        public static object FromText(Type argumentType, string text, Func<string, Type, object?> convert)
        {
            var valueType = argumentType.GetGenericArguments()[0];
            var definition = argumentType.GetGenericTypeDefinition().FullName;
            var wf = argumentType.Assembly;

            object expression;
            if (text.Length >= 2 && text[0] == '[' && text[text.Length - 1] == ']')
            {
                var expressionTypeName = definition == InArgument
                    ? "Microsoft.VisualBasic.Activities.VisualBasicValue`1"
                    : "Microsoft.VisualBasic.Activities.VisualBasicReference`1";
                var expressionType = FindType(wf, expressionTypeName).MakeGenericType(valueType);
                expression = Activator.CreateInstance(expressionType, text.Substring(1, text.Length - 2))!;
            }
            else
            {
                if (definition != InArgument)
                {
                    throw new FormatException($"'{text}' is not a valid value for an {argumentType.Name}: only an [expression] can be assigned to.");
                }
                var literalText = text.StartsWith("%", StringComparison.Ordinal) ? text.Substring(1) : text;
                var literalType = FindType(wf, "System.Activities.Expressions.Literal`1").MakeGenericType(valueType);
                expression = Activator.CreateInstance(literalType, convert(literalText, valueType))!;
            }
            return Activator.CreateInstance(argumentType, expression)!;
        }

        static Type FindType(System.Reflection.Assembly wf, string fullName) =>
            wf.GetType(fullName, false)
            ?? AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType(fullName, false)).FirstOrDefault(t => t != null)
            ?? throw new InvalidOperationException($"WF type '{fullName}' is not available.");
    }
}
