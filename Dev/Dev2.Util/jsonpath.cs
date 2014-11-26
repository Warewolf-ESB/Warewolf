/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


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

namespace Dev2.Util
{

    #region Imports

    #endregion

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
        private readonly string _path;
        private readonly object _value;

        public JsonPathNode(object value, string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");

            if (path.Length == 0)
                throw new ArgumentException("path");

            _value = value;
            _path = path;
        }

        public object Value
        {
            get { return _value; }
        }

        public string Path
        {
            get { return _path; }
        }

        public override string ToString()
        {
            return Path + " = " + Value;
        }

        public static object[] ValuesFrom(ICollection nodes)
        {
            var values = new object[nodes != null ? nodes.Count : 0];

            if (values.Length > 0)
            {
                Debug.Assert(nodes != null);

                int i = 0;
                foreach (JsonPathNode node in nodes)
                    values[i++] = node.Value;
            }

            return values;
        }

        public static string[] PathsFrom(ICollection nodes)
        {
            var paths = new string[nodes != null ? nodes.Count : 0];

            if (paths.Length > 0)
            {
                Debug.Assert(nodes != null);

                int i = 0;
                foreach (JsonPathNode node in nodes)
                    paths[i++] = node.Path;
            }

            return paths;
        }
    }

    public sealed class JsonPathContext
    {
        public static readonly JsonPathContext Default = new JsonPathContext();

        public JsonPathScriptEvaluator ScriptEvaluator { get; set; }

        public IJsonPathValueSystem ValueSystem { get; set; }

        public void SelectTo(object obj, string expr, JsonPathResultAccumulator output)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            if (output == null)
                throw new ArgumentNullException("output");

            var i = new Interpreter(output, ValueSystem, ScriptEvaluator);

            expr = Normalize(expr);

            if (expr.Length >= 1 && expr[0] == '$') // ^\$:?
                expr = expr.Substring(expr.Length >= 2 && expr[1] == ';' ? 2 : 1);

            i.Trace(expr, obj, "$");
        }

        public JsonPathNode[] SelectNodes(object obj, string expr)
        {
            var list = new ArrayList();
            SelectNodesTo(obj, expr, list);
            return (JsonPathNode[]) list.ToArray(typeof (JsonPathNode));
        }

        public IList SelectNodesTo(object obj, string expr, IList output)
        {
            var accumulator = new ListAccumulator(output ?? new ArrayList());
            SelectTo(obj, expr, accumulator.Put);
            return output;
        }

        private static Regex RegExp(string pattern)
        {
            return new Regex(pattern, RegexOptions.ECMAScript);
        }

        private static string Normalize(string expr)
        {
            var swap = new NormalizationSwap();
            expr = RegExp(@"[\['](\??\(.*?\))[\]']").Replace(expr, swap.Capture);
            expr = RegExp(@"'?\.'?|\['?").Replace(expr, ";");
            expr = RegExp(@";;;|;;").Replace(expr, ";..;");
            expr = RegExp(@";$|'?\]|'$").Replace(expr, string.Empty);
            expr = RegExp(@"#([0-9]+)").Replace(expr, swap.Yield);
            return expr;
        }

        public static string AsBracketNotation(string[] indicies)
        {
            if (indicies == null)
                throw new ArgumentNullException("indicies");

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
                        sb.Append(index);
                    else
                        sb.Append('\'').Append(index).Append('\'');
                    sb.Append(']');
                }
            }

            return sb.ToString();
        }

        private static int ParseInt(string str, int defaultValue = 0)
        {
            if (string.IsNullOrEmpty(str))
                return defaultValue;

            try
            {
                return int.Parse(str, NumberStyles.None, CultureInfo.InvariantCulture);
            }
            catch (FormatException)
            {
                return defaultValue;
            }
        }

        private sealed class BasicValueSystem : IJsonPathValueSystem
        {
            public bool HasMember(object value, string member)
            {
                if (IsPrimitive(value))
                    return false;

                var dict = value as IDictionary;
                if (dict != null)
                    return dict.Contains(member);

                var list = value as IList;
                if (list != null)
                {
                    int index = ParseInt(member, -1);
                    return index >= 0 && index < list.Count;
                }

                return false;
            }

            public object GetMemberValue(object value, string member)
            {
                if (IsPrimitive(value))
                    throw new ArgumentException("value");

                var dict = value as IDictionary;
                if (dict != null)
                    return dict[member];

                var list = (IList) value;
                int index = ParseInt(member, -1);
                if (index >= 0 && index < list.Count)
                    return list[index];

                return null;
            }

            public IEnumerable GetMembers(object value)
            {
                return ((IDictionary) value).Keys;
            }

            public bool IsObject(object value)
            {
                return value is IDictionary;
            }

            public bool IsArray(object value)
            {
                return value is IList;
            }

            public bool IsPrimitive(object value)
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                return Type.GetTypeCode(value.GetType()) != TypeCode.Object;
            }
        }

        private sealed class Interpreter
        {
            private static readonly IJsonPathValueSystem DefaultValueSystem = new BasicValueSystem();

            private static readonly char[] Colon = {':'};
            private static readonly char[] Semicolon = {';'};
            private readonly JsonPathScriptEvaluator _eval;
            private readonly JsonPathResultAccumulator _output;
            private readonly IJsonPathValueSystem _system;

            public Interpreter(JsonPathResultAccumulator output, IJsonPathValueSystem valueSystem,
                JsonPathScriptEvaluator eval)
            {
                Debug.Assert(output != null);

                _output = output;
                _eval = eval ?? NullEval;
                _system = valueSystem ?? DefaultValueSystem;
            }

            public void Trace(string expr, object value, string path)
            {
                if (string.IsNullOrEmpty(expr))
                {
                    Store(path, value);
                    return;
                }

                int i = expr.IndexOf(';');
                string atom = i >= 0 ? expr.Substring(0, i) : expr;
                string tail = i >= 0 ? expr.Substring(i + 1) : string.Empty;

                if (value != null && _system.HasMember(value, atom))
                {
                    Trace(tail, Index(value, atom), path + ";" + atom);
                }
                else if (atom == "*")
                {
                    Walk(atom, tail, value, path, WalkWild);
                }
                else if (atom == "..")
                {
                    Trace(tail, value, path);
                    Walk(atom, tail, value, path, WalkTree);
                }
                else if (atom.Length > 2 && atom[0] == '(' && atom[atom.Length - 1] == ')') // [(exp)]
                {
                    Trace(_eval(atom, value, path.Substring(path.LastIndexOf(';') + 1)) + ";" + tail, value, path);
                }
                else if (atom.Length > 3 && atom[0] == '?' && atom[1] == '(' && atom[atom.Length - 1] == ')') // [?(exp)]
                {
                    Walk(atom, tail, value, path, WalkFiltered);
                }
                else if (RegExp(@"^(-?[0-9]*):(-?[0-9]*):?([0-9]*)$").IsMatch(atom))
                    // [start:end:step] Phyton slice syntax
                {
                    Slice(atom, tail, value, path);
                }
                else if (atom.IndexOf(',') >= 0) // [name1,name2,...]
                {
                    foreach (string part in RegExp(@"'?,'?").Split(atom))
                        Trace(part + ";" + tail, value, path);
                }
            }

            private void Store(string path, object value)
            {
                if (path != null)
                    _output(value, path.Split(Semicolon));
            }

            private void Walk(string loc, string expr, object value, string path, WalkCallback callback)
            {
                if (_system.IsPrimitive(value))
                    return;

                if (_system.IsArray(value))
                {
                    var list = (IList) value;
                    for (int i = 0; i < list.Count; i++)
                        callback(i, loc, expr, value, path);
                }
                else if (_system.IsObject(value))
                {
                    foreach (string key in _system.GetMembers(value))
                        callback(key, loc, expr, value, path);
                }
            }

            private void WalkWild(object member, string loc, string expr, object value, string path)
            {
                Trace(member + ";" + expr, value, path);
            }

            private void WalkTree(object member, string loc, string expr, object value, string path)
            {
                object result = Index(value, member.ToString());
                if (result != null && !_system.IsPrimitive(result))
                    Trace("..;" + expr, result, path + ";" + member);
            }

            private void WalkFiltered(object member, string loc, string expr, object value, string path)
            {
                object result = _eval(RegExp(@"^\?\((.*?)\)$").Replace(loc, "$1"),
                    Index(value, member.ToString()), member.ToString());

                if (Convert.ToBoolean(result, CultureInfo.InvariantCulture))
                    Trace(member + ";" + expr, value, path);
            }

            private void Slice(string loc, string expr, object value, string path)
            {
                var list = value as IList;

                if (list == null)
                    return;

                int length = list.Count;
                string[] parts = loc.Split(Colon);
                int start = ParseInt(parts[0]);
                int end = ParseInt(parts[1], list.Count);
                int step = parts.Length > 2 ? ParseInt(parts[2], 1) : 1;
                start = (start < 0) ? Math.Max(0, start + length) : Math.Min(length, start);
                end = (end < 0) ? Math.Max(0, end + length) : Math.Min(length, end);
                for (int i = start; i < end; i += step)
                    Trace(i + ";" + expr, value, path);
            }

            private object Index(object obj, string member)
            {
                return _system.GetMemberValue(obj, member);
            }

            private static object NullEval(string expr, object value, string context)
            {
                return null;
            }

            private delegate void WalkCallback(object member, string loc, string expr, object value, string path);
        }

        private sealed class ListAccumulator
        {
            private readonly IList _list;

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

        private sealed class NormalizationSwap
        {
            private readonly ArrayList _subx = new ArrayList(4);

            public string Capture(Match match)
            {
                Debug.Assert(match != null);

                int index = _subx.Add(match.Groups[1].Value);
                return "[#" + index.ToString(CultureInfo.InvariantCulture) + "]";
            }

            public string Yield(Match match)
            {
                Debug.Assert(match != null);

                int index = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
                return (string) _subx[index];
            }
        }
    }
}