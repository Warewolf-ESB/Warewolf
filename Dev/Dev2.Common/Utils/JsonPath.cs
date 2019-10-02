#pragma warning disable
//
// C# implementation of JSONPath[1]
//
// Copyright (c) 2007 Atif Aziz (http://www.raboof.com/)
// Licensed under The MIT License
//
// Supported targets:
//
//  - Mono 1.1 or later
//  - Microsoft .NET Framework 1.0 or later
//
// [1]  JSONPath - XPath for JSON
//      http://code.google.com/p/jsonpath/
//      Copyright (c) 2007 Stefan Goessner (goessner.net)
//      Licensed under The MIT License
// Version 0.5.1

#region The MIT License

//
// The MIT License
//
// Copyright (c) 2007 Atif Aziz (http://www.raboof.com/)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//

#endregion


using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Dev2.Common.Utils
{

    public delegate object JsonPathScriptEvaluator(string script, object value, string context);

    public delegate void JsonPathResultAccumulator(object value, string[] indicies);


    public interface IJsonPathValueSystem
    {
        bool HasMember(object value, string member);
        object GetMemberValue(object value, string member);
        IEnumerable GetMembers(object value);
        bool IsObject(object value);
        bool IsArray(object value);
        bool IsPrimitive(object value);
    }

    [Serializable]
    public sealed class JsonPathNode
    {
        readonly string _path;
        readonly object _value;

        public JsonPathNode(object value, string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }

            if (path.Length == 0)
            {
                throw new ArgumentException("path");
            }

            _value = value;
            _path = path;
        }

        public object Value => _value;

        public string Path => _path;

        public override string ToString() => Path + " = " + Value;
    }

    public class JsonPathContext
    {
        public static readonly JsonPathContext Default = new JsonPathContext();

        public JsonPathScriptEvaluator ScriptEvaluator { get; set; }

        public IJsonPathValueSystem ValueSystem { get; set; }

        public void SelectTo(object obj, string initialExpression, JsonPathResultAccumulator output)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            var i = new Interpreter(output, ValueSystem, ScriptEvaluator);

            var expr = Normalize(initialExpression);

            if (expr.Length >= 1 && expr[0] == '$') // ^\$:?
            {
                expr = expr.Substring(expr.Length >= 2 && expr[1] == ';' ? 2 : 1);
            }

            i.StoreExpressionTreeLeafNodes(expr, obj, "$");
        }

        public JsonPathNode[] SelectNodes(object obj, string expr)
        {
            var list = new ArrayList();
            SelectNodesTo(obj, expr, list);
            return (JsonPathNode[])list.ToArray(typeof(JsonPathNode));
        }

        public IList SelectNodesTo(object obj, string expr, IList output)
        {
            var accumulator = new ListAccumulator(output ?? new ArrayList());
            SelectTo(obj, expr, accumulator.Put);
            return output;
        }

        static Regex RegExp(string pattern) => new Regex(pattern, RegexOptions.ECMAScript);

        static string Normalize(string initialExpression)
        {
            var swap = new NormalizationSwap();
            var expr = RegExp(@"[\['](\??\(.*?\))[\]']").Replace(initialExpression, swap.Capture);
            expr = RegExp(@"'?\.'?|\['?").Replace(expr, ";");
            expr = RegExp(@";;;|;;").Replace(expr, ";..;");
            expr = RegExp(@";$|'?\]|'$").Replace(expr, string.Empty);
            expr = RegExp(@"#([0-9]+)").Replace(expr, swap.Yield);
            return expr;
        }

        public static string AsBracketNotation(string[] indicies)
        {
            if (indicies == null)
            {
                throw new ArgumentNullException("indicies");
            }

            var sb = new StringBuilder();

            foreach (string index in indicies)
            {
                if (sb.Length == 0)
                {
                    sb.Append('$');
                }
                else
                {
                    sb.Append('[');
                    if (RegExp(@"^[0-9*]+$").IsMatch(index))
                    {
                        sb.Append(index);
                    }
                    else
                    {
                        sb.Append('\'').Append(index).Append('\'');
                    }

                    sb.Append(']');
                }
            }

            return sb.ToString();
        }

        public sealed class Interpreter
        {
            static readonly char[] Colon = { ':' };
            static readonly char[] Semicolon = { ';' };
            internal readonly JsonPathScriptEvaluator _eval;
            internal readonly JsonPathResultAccumulator _output;
            internal readonly IJsonPathValueSystem _system;

            public Interpreter(JsonPathResultAccumulator output, IJsonPathValueSystem system,
                JsonPathScriptEvaluator eval)
            {
                Debug.Assert(output != null);

                _output = output;
                _eval = eval;
                _system = system;
            }

            public void StoreExpressionTreeLeafNodes(string expr, object value, string path)
            {
                if (string.IsNullOrEmpty(expr))
                {
                    Store(path, value);
                    return;
                }

                var i = expr.IndexOf(';');
                var atom = i >= 0 ? expr.Substring(0, i) : expr;
                var tail = i >= 0 ? expr.Substring(i + 1) : string.Empty;

                Trace(value, path, atom, tail);
            }

#pragma warning disable S1541 // Ignore complexity for the switch method
            private void Trace(object value, string path, string atom, string tail)
#pragma warning restore S1541 // Methods and properties should not be too complex
            {
                if (value != null && _system.HasMember(value, atom))
                {
                    StoreExpressionTreeLeafNodes(tail, Index(value, atom), path + ";" + atom);
                }
                else if (atom == "*")
                {
                    Walk(atom, tail, value, path, WalkWild);
                }
                else if (atom == "..")
                {
                    StoreExpressionTreeLeafNodes(tail, value, path);
                    Walk(atom, tail, value, path, WalkTree);
                }
                else if (atom.Length > 2 && atom[0] == '(' && atom[atom.Length - 1] == ')')
                {
                    StoreExpressionTreeLeafNodes(_eval(atom, value, path.Substring(path.LastIndexOf(';') + 1)) + ";" + tail, value, path);
                }
                else if (atom.Length > 3 && atom[0] == '?' && atom[1] == '(' && atom[atom.Length - 1] == ')')
                {
                    Walk(atom, tail, value, path, WalkFiltered);
                }
                else if (RegExp(@"^(-?[0-9]*):(-?[0-9]*):?([0-9]*)$").IsMatch(atom))
                {
                    Slice(atom, tail, value, path);
                }
                else
                {
                    if (atom.IndexOf(',') >= 0)
                    {
                        foreach (string part in RegExp(@"'?,'?").Split(atom))
                        {
                            StoreExpressionTreeLeafNodes(part + ";" + tail, value, path);
                        }
                    }
                }
            }

            void Store(string path, object value)
            {
                if (path != null)
                {
                    _output(value, path.Split(Semicolon));
                }
            }

            void Walk(string loc, string expr, object value, string path, WalkCallback callback)
            {
                if (_system.IsPrimitive(value))
                {
                    return;
                }

                if (_system.IsArray(value))
                {
                    var list = (IList)value;
                    for (int i = 0; i < list.Count; i++)
                    {
                        callback?.Invoke(i, loc, expr, value, path);
                    }
                }
                else
                {
                    if (_system.IsObject(value))
                    {
                        foreach (string key in _system.GetMembers(value))
                        {
                            callback?.Invoke(key, loc, expr, value, path);
                        }
                    }
                }
            }

            void WalkWild(object member, string loc, string expr, object value, string path)
            {
                StoreExpressionTreeLeafNodes(member + ";" + expr, value, path);
            }

            void WalkTree(object member, string loc, string expr, object value, string path)
            {
                var result = Index(value, member.ToString());
                if (result != null && !_system.IsPrimitive(result))
                {
                    StoreExpressionTreeLeafNodes("..;" + expr, result, path + ";" + member);
                }
            }

            void WalkFiltered(object member, string loc, string expr, object value, string path)
            {
                var result = _eval(RegExp(@"^\?\((.*?)\)$").Replace(loc, "$1"),
                    Index(value, member.ToString()), member.ToString());

                if (Convert.ToBoolean(result, CultureInfo.InvariantCulture))
                {
                    StoreExpressionTreeLeafNodes(member + ";" + expr, value, path);
                }
            }

            void Slice(string loc, string expr, object value, string path)
            {
                var list = value as IList;

                if (list == null)
                {
                    return;
                }

                var length = list.Count;
                var parts = loc.Split(Colon);
                var start = ParseInt(parts[0]);
                var end = ParseInt(parts[1], list.Count);
                var step = parts.Length > 2 ? ParseInt(parts[2], 1) : 1;
                start = start < 0 ? Math.Max(0, start + length) : Math.Min(length, start);
                end = end < 0 ? Math.Max(0, end + length) : Math.Min(length, end);
                for (int i = start; i < end; i += step)
                {
                    StoreExpressionTreeLeafNodes(i + ";" + expr, value, path);
                }
            }

            static int ParseInt(string str, int defaultValue = 0)
            {
                if (string.IsNullOrEmpty(str))
                {
                    return defaultValue;
                }

                try
                {
                    return int.Parse(str, NumberStyles.None, CultureInfo.InvariantCulture);
                }
                catch (FormatException)
                {
                    return defaultValue;
                }
            }

            object Index(object obj, string member) => _system.GetMemberValue(obj, member);

            delegate void WalkCallback(object member, string loc, string expr, object value, string path);
        }

        public sealed class ListAccumulator
        {
            readonly IList _list;

            public ListAccumulator(IList list)
            {
                Debug.Assert(list != null);

                _list = list;
            }

            public void Put(object value, string[] indicies)
            {
                _list.Add(new JsonPathNode(value, AsBracketNotation(indicies)));
            }
        }

        sealed class NormalizationSwap
        {
            readonly ArrayList _subx = new ArrayList(4);

            public string Capture(Match match)
            {
                Debug.Assert(match != null);

                var index = _subx.Add(match.Groups[1].Value);
                return "[#" + index.ToString(CultureInfo.InvariantCulture) + "]";
            }

            public string Yield(Match match)
            {
                Debug.Assert(match != null);

                var index = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
                return (string)_subx[index];
            }
        }
    }
}
