#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
﻿/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
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

        public void SelectTo(object obj, string expr, JsonPathResultAccumulator output)
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

            expr = Normalize(expr);

            if (expr.Length >= 1 && expr[0] == '$') // ^\$:?
            {
                expr = expr.Substring(expr.Length >= 2 && expr[1] == ';' ? 2 : 1);
            }

            i.Trace(expr, obj, "$");
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

        static string Normalize(string expr)
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

        sealed class Interpreter
        {
            static readonly char[] Colon = { ':' };
            static readonly char[] Semicolon = { ';' };
            readonly JsonPathScriptEvaluator _eval;
            readonly JsonPathResultAccumulator _output;
            readonly IJsonPathValueSystem _system;

            public Interpreter(JsonPathResultAccumulator output, IJsonPathValueSystem system,
                JsonPathScriptEvaluator eval)
            {
                Debug.Assert(output != null);

                _output = output;
                _eval = eval;
                _system = system;
            }

            public void Trace(string expr, object value, string path)
            {
                if (string.IsNullOrEmpty(expr))
                {
                    Store(path, value);
                    return;
                }

                var i = expr.IndexOf(';');
                var atom = i >= 0 ? expr.Substring(0, i) : expr;
                var tail = i >= 0 ? expr.Substring(i + 1) : string.Empty;

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
                else if (atom.Length > 2 && atom[0] == '(' && atom[atom.Length - 1] == ')')
                {
                    Trace(_eval(atom, value, path.Substring(path.LastIndexOf(';') + 1)) + ";" + tail, value, path);
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
                            Trace(part + ";" + tail, value, path);
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
                Trace(member + ";" + expr, value, path);
            }

            void WalkTree(object member, string loc, string expr, object value, string path)
            {
                var result = Index(value, member.ToString());
                if (result != null && !_system.IsPrimitive(result))
                {
                    Trace("..;" + expr, result, path + ";" + member);
                }
            }

            void WalkFiltered(object member, string loc, string expr, object value, string path)
            {
                var result = _eval(RegExp(@"^\?\((.*?)\)$").Replace(loc, "$1"),
                    Index(value, member.ToString()), member.ToString());

                if (Convert.ToBoolean(result, CultureInfo.InvariantCulture))
                {
                    Trace(member + ";" + expr, value, path);
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
                    Trace(i + ";" + expr, value, path);
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
