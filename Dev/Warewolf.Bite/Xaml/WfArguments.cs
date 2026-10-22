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
using System.Reflection;

namespace Warewolf.Bite.Xaml
{
    /// <summary>
    /// Builds and takes apart WF <c>InArgument</c>/<c>OutArgument</c>/<c>InOutArgument</c> values and the WF delegate
    /// wrappers (<c>ActivityFunc</c>/<c>ActivityAction</c>), by reflection so this assembly has no compile-time WF
    /// reference. Goes away with Workstream 2 (T2.3, T2.4), when these properties become plain types.
    ///
    /// <para>XAML attribute text: <c>[expr]</c> is a VB expression, kept as text and never compiled; <c>%text</c>
    /// escapes a literal that would otherwise read as an expression; anything else is a <c>Literal</c>.</para>
    /// </summary>
    static class WfArguments
    {
        const string InArgument = "System.Activities.InArgument`1";
        const string OutArgument = "System.Activities.OutArgument`1";
        const string InOutArgument = "System.Activities.InOutArgument`1";

        public static bool IsArgumentType(Type type) =>
            type.IsGenericType && type.GetGenericTypeDefinition().FullName is InArgument or OutArgument or InOutArgument;

        /// <summary>ActivityFunc/ActivityAction: a <c>Handler</c> activity and an optional delegate argument.</summary>
        public static bool IsDelegateType(Type type) =>
            type.Namespace == "System.Activities" && (type.Name.StartsWith("ActivityFunc", StringComparison.Ordinal) || type.Name.StartsWith("ActivityAction", StringComparison.Ordinal));

        public static object FromText(Type argumentType, string text, Func<string, Type, object?> convert)
        {
            if (text.Length >= 2 && text[0] == '[' && text[text.Length - 1] == ']')
            {
                return Expression(argumentType, text.Substring(1, text.Length - 2));
            }
            var literalText = text.StartsWith("%", StringComparison.Ordinal) ? text.Substring(1) : text;
            return Literal(argumentType, convert(literalText, argumentType.GetGenericArguments()[0]));
        }

        /// <summary>An argument holding a VB expression (a value for InArgument, a reference otherwise), uncompiled.</summary>
        public static object Expression(Type argumentType, string expressionText)
        {
            var valueType = argumentType.GetGenericArguments()[0];
            var expressionTypeName = argumentType.GetGenericTypeDefinition().FullName == InArgument
                ? "Microsoft.VisualBasic.Activities.VisualBasicValue`1"
                : "Microsoft.VisualBasic.Activities.VisualBasicReference`1";
            var expressionType = FindType(argumentType.Assembly, expressionTypeName).MakeGenericType(valueType);
            return Activator.CreateInstance(argumentType, Activator.CreateInstance(expressionType, expressionText))!;
        }

        /// <summary>An InArgument holding a literal value. Out/InOut arguments can only hold an expression.</summary>
        public static object Literal(Type argumentType, object? value)
        {
            if (argumentType.GetGenericTypeDefinition().FullName != InArgument)
            {
                throw new FormatException($"A literal cannot be assigned to an {argumentType.Name}: only an [expression] can.");
            }
            var literalType = FindType(argumentType.Assembly, "System.Activities.Expressions.Literal`1").MakeGenericType(argumentType.GetGenericArguments()[0]);
            return Activator.CreateInstance(argumentType, Activator.CreateInstance(literalType, value))!;
        }

        /// <summary>An argument with no expression, as <c>&lt;InOutArgument x:TypeArguments="..." /&gt;</c> declares.</summary>
        public static object Empty(Type argumentType) => Activator.CreateInstance(argumentType)!;

        /// <summary>What an argument holds: nothing, a literal value, or expression text.</summary>
        public static (bool HasExpression, bool IsLiteral, object? Value) Describe(object argument)
        {
            var expression = Property(argument.GetType(), "Expression")?.GetValue(argument);
            if (expression == null)
            {
                return (false, false, null);
            }
            var expressionType = expression.GetType();
            if (expressionType.Name.StartsWith("Literal", StringComparison.Ordinal))
            {
                return (true, true, Property(expressionType, "Value")?.GetValue(expression));
            }
            var text = Property(expressionType, "ExpressionText")?.GetValue(expression) as string;
            return (true, false, text ?? expression.ToString());
        }

        public static object CreateDelegate(Type delegateType, string? argumentName, object? handler)
        {
            var wrapper = Activator.CreateInstance(delegateType)!;
            if (argumentName != null)
            {
                var argumentProperty = Property(delegateType, "Argument")
                    ?? throw new FormatException($"{delegateType.Name} has no argument to name '{argumentName}'");
                var argumentType = argumentProperty.PropertyType; // DelegateInArgument<T>
                argumentProperty.SetValue(wrapper, Activator.CreateInstance(argumentType, argumentName));
            }
            Property(delegateType, "Handler")!.SetValue(wrapper, handler);
            return wrapper;
        }

        public static (string? ArgumentName, object? Handler) DescribeDelegate(object wrapper)
        {
            var type = wrapper.GetType();
            var argument = Property(type, "Argument")?.GetValue(wrapper);
            var name = argument == null ? null : Property(argument.GetType(), "Name")?.GetValue(argument) as string;
            return (name, Property(type, "Handler")?.GetValue(wrapper));
        }

        // WF re-declares members with `new` (InArgument<T>.Expression hides Argument.Expression): take the most derived.
        static PropertyInfo? Property(Type type, string name) =>
            type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.Name == name && p.GetIndexParameters().Length == 0)
                .OrderByDescending(p => Depth(p.DeclaringType))
                .FirstOrDefault();

        static int Depth(Type? type)
        {
            var depth = 0;
            for (var t = type; t != null; t = t.BaseType)
            {
                depth++;
            }
            return depth;
        }

        static Type FindType(Assembly wf, string fullName) =>
            wf.GetType(fullName, false)
            ?? AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType(fullName, false)).FirstOrDefault(t => t != null)
            ?? throw new InvalidOperationException($"WF type '{fullName}' is not available.");
    }
}
