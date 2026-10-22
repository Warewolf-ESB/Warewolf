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
using System.Collections.Generic;

namespace Warewolf.Bite.Xaml
{
    /// <summary>
    /// A XAML type name as written in <c>x:TypeArguments</c>: <c>prefix:Name</c>, optionally with generic arguments in
    /// parentheses, e.g. <c>scg:Dictionary(x:String, scg:List(x:String))</c>.
    /// </summary>
    public sealed class XamlTypeName
    {
        XamlTypeName(string? prefix, string name, IReadOnlyList<XamlTypeName> arguments)
        {
            Prefix = prefix;
            Name = name;
            Arguments = arguments;
        }

        public string? Prefix { get; }
        public string Name { get; }
        public IReadOnlyList<XamlTypeName> Arguments { get; }

        /// <summary>Parses a comma-separated list of type names (the form <c>x:TypeArguments</c> takes).</summary>
        public static IReadOnlyList<XamlTypeName> ParseList(string text)
        {
            var position = 0;
            var result = ParseListAt(text, ref position);
            SkipSpace(text, ref position);
            if (position != text.Length)
            {
                throw new FormatException($"Unexpected '{text[position]}' at {position} in type name '{text}'");
            }
            return result;
        }

        static List<XamlTypeName> ParseListAt(string text, ref int position)
        {
            var names = new List<XamlTypeName> { ParseAt(text, ref position) };
            SkipSpace(text, ref position);
            while (position < text.Length && text[position] == ',')
            {
                position++;
                names.Add(ParseAt(text, ref position));
                SkipSpace(text, ref position);
            }
            return names;
        }

        static XamlTypeName ParseAt(string text, ref int position)
        {
            SkipSpace(text, ref position);
            var start = position;
            while (position < text.Length && (char.IsLetterOrDigit(text[position]) || text[position] == '_' || text[position] == '.' || text[position] == ':'))
            {
                position++;
            }
            var qualified = text.Substring(start, position - start);
            if (qualified.Length == 0)
            {
                throw new FormatException($"Expected a type name at {start} in '{text}'");
            }
            var colon = qualified.IndexOf(':');
            var prefix = colon < 0 ? null : qualified.Substring(0, colon);
            var name = colon < 0 ? qualified : qualified.Substring(colon + 1);

            SkipSpace(text, ref position);
            IReadOnlyList<XamlTypeName> arguments = Array.Empty<XamlTypeName>();
            if (position < text.Length && text[position] == '(')
            {
                position++;
                arguments = ParseListAt(text, ref position);
                SkipSpace(text, ref position);
                if (position >= text.Length || text[position] != ')')
                {
                    throw new FormatException($"Missing ')' in type name '{text}'");
                }
                position++;
            }
            return new XamlTypeName(prefix, name, arguments);
        }

        static void SkipSpace(string text, ref int position)
        {
            while (position < text.Length && char.IsWhiteSpace(text[position]))
            {
                position++;
            }
        }
    }
}
