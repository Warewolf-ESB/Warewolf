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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml.Linq;
using Dev2.Activities.Flow;
using Warewolf.Bite.Flow;
using Warewolf.Bite.Xaml;

namespace Warewolf.Bite.Json
{
    /// <summary>
    /// Writes a workflow definition (<see cref="FlowchartModel"/>) as the version 1 JSON body (WF removal T1.5).
    /// Writes the definition, not the linked executable graph, so <see cref="Flow.FlowGraphBuilder"/>'s quirks are never
    /// persisted, and every flowchart node is kept, reachable or not. Output is deterministic: nodes in document
    /// order, members in ordinal name order, culture-invariant values. See docs/BiteDefinition-Format.md.
    /// </summary>
    public static class BiteJsonWriter
    {
        public const int FormatVersion = 1;

        // Written with Utf8JsonWriter (JsonNode.ToJsonString needs a type-info resolver here). Relaxed escaping keeps
        // [[variables]], quotes and <xml> readable; the body is XML-escaped inside <BiteDefinition> regardless.
        static readonly JsonWriterOptions WriterOptions = new JsonWriterOptions
        {
            Indented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };

        public static string Write(FlowchartModel chart)
        {
            using (var stream = new System.IO.MemoryStream())
            {
                using (var writer = new Utf8JsonWriter(stream, WriterOptions))
                {
                    ToJson(chart).WriteTo(writer);
                }
                return System.Text.Encoding.UTF8.GetString(stream.ToArray());
            }
        }

        /// <summary>Reads XAML with <see cref="BiteReader"/>'s object reader and writes it as the JSON body.</summary>
        public static string FromXaml(string xamlDefinition, string resourceId) =>
            Write(new BiteReader().ReadModel(xamlDefinition, resourceId)
                  ?? throw new BiteFormatException("The XamlDefinition has no Flowchart to convert"));

        /// <summary>
        /// A whole <c>.bite</c> resource with its <c>&lt;XamlDefinition&gt;</c> replaced by a version 1
        /// <c>&lt;BiteDefinition&gt;</c>; the envelope is otherwise untouched.
        /// </summary>
        public static string ConvertResource(string bite)
        {
            var service = XElement.Parse(bite, LoadOptions.PreserveWhitespace);
            var action = service.Element("Actions")?.Element("Action") ?? service.Element("Action")
                         ?? throw new BiteFormatException("Resource has no <Action>", service);
            var xaml = action.Element("XamlDefinition") ?? throw new BiteFormatException("Resource has no <XamlDefinition>", action);
            var json = FromXaml(xaml.Value, BiteReader.SeedFor(service, xaml.Value));
            xaml.ReplaceWith(new XElement("BiteDefinition", new XAttribute("FormatVersion", FormatVersion), json));
            return service.ToString(SaveOptions.DisableFormatting);
        }

        static JsonObject ToJson(FlowchartModel chart)
        {
            var ids = new Dictionary<FlowNodeModel, string>(ReferenceEqualityComparer.Instance);
            var all = chart.Nodes.ToList();
            if (chart.StartNode != null && !all.Contains(chart.StartNode))
            {
                all.Insert(0, chart.StartNode);
            }
            foreach (var node in all)
            {
                var id = node.Name ?? $"n{ids.Count}";
                while (ids.Values.Contains(id))
                {
                    id += "_";
                }
                ids[node] = id;
            }

            string? Id(FlowNodeModel? node) => node == null ? null
                : ids.TryGetValue(node, out var id) ? id
                : throw new InvalidOperationException("Flow node is not in the flowchart's node list");

            var nodes = new JsonObject();
            foreach (var node in all)
            {
                nodes[ids[node]] = node switch
                {
                    FlowStepModel step => new JsonObject
                    {
                        ["kind"] = "step",
                        ["activity"] = Value(step.Action, $"{ids[node]}.activity"),
                        ["next"] = Id(step.Next),
                    },
                    FlowDecisionModel decision => new JsonObject
                    {
                        ["kind"] = "decision",
                        ["displayName"] = decision.DisplayName,
                        ["condition"] = Value(decision.Condition, $"{ids[node]}.condition"),
                        ["true"] = Id(decision.True),
                        ["false"] = Id(decision.False),
                    },
                    FlowSwitchModel flowSwitch => new JsonObject
                    {
                        ["kind"] = "switch",
                        ["displayName"] = flowSwitch.DisplayName,
                        ["expression"] = Value(flowSwitch.Expression, $"{ids[node]}.expression"),
                        ["cases"] = new JsonObject(flowSwitch.Cases.Select(c => new KeyValuePair<string, JsonNode?>(c.Key, Id(c.Value)))),
                        ["default"] = Id(flowSwitch.Default),
                    },
                    _ => throw new InvalidOperationException($"Unknown flow node {node.GetType().Name}"),
                };
            }

            return new JsonObject
            {
                ["name"] = chart.Name,
                ["start"] = Id(chart.StartNode),
                ["nodes"] = nodes,
            };
        }

        static JsonNode? Value(object? value, string path) => Value(value, path, new HashSet<object>(ReferenceEqualityComparer.Instance));

        static JsonNode? Value(object? value, string path, HashSet<object> ancestors)
        {
            switch (value)
            {
                case null:
                    return null;
                case string s:
                    return s;
                case bool b:
                    return b;
                case char c:
                    return c.ToString();
                case Enum e:
                    return e.ToString();
                case Guid g:
                    return g.ToString("D");
                case DateTime dt:
                    return dt.ToString("O", CultureInfo.InvariantCulture);
                case TimeSpan ts:
                    return ts.ToString("c", CultureInfo.InvariantCulture);
                case Uri uri:
                    return uri.OriginalString;
                case Type t:
                    return t.IsGenericType ? throw Unsupported(path, "generic Type value") : t.FullName;
                case int or long or short or byte or sbyte or uint or ulong or ushort or float or double or decimal:
                    return JsonValue.Create(value);
            }

            var type = value.GetType();
            if (WfArguments.IsArgumentType(type))
            {
                var (hasExpression, isLiteral, held) = WfArguments.Describe(value);
                return !hasExpression ? new JsonObject()
                    : isLiteral ? Value(held, path, ancestors)
                    : new JsonObject { ["$expr"] = (string?)held };
            }
            if (!ancestors.Add(value))
            {
                throw Unsupported(path, "a cycle in an activity's values");
            }
            try
            {
                if (WfArguments.IsDelegateType(type))
                {
                    var (argument, handler) = WfArguments.DescribeDelegate(value);
                    return new JsonObject { ["argument"] = argument, ["body"] = Value(handler, path + ".body", ancestors) };
                }
                if (value is IDictionary dictionary)
                {
                    var map = new JsonObject();
                    foreach (var key in dictionary.Keys.Cast<object>().Select(k => Convert.ToString(k, CultureInfo.InvariantCulture) ?? "").OrderBy(k => k, StringComparer.Ordinal))
                    {
                        map[key] = Value(dictionary[ConvertKey(key, dictionary)], $"{path}[{key}]", ancestors);
                    }
                    return map;
                }
                if (value is IEnumerable sequence)
                {
                    var array = new JsonArray();
                    var index = 0;
                    foreach (var item in sequence)
                    {
                        array.Add(Value(item, $"{path}[{index++}]", ancestors));
                    }
                    return array;
                }
                if (!DefinitionMembers.IsWarewolfType(type))
                {
                    throw Unsupported(path, $"a {type.FullName} value");
                }
                var result = new JsonObject { ["$type"] = type.FullName };
                foreach (var member in DefinitionMembers.Of(type))
                {
                    result[member.Name] = Value(member.GetValue(value), $"{path}.{member.Name}", ancestors);
                }
                return result;
            }
            finally
            {
                ancestors.Remove(value);
            }
        }

        // Dictionary keys are written as text; find the original key object to read the value back.
        static object ConvertKey(string text, IDictionary dictionary) =>
            dictionary.Keys.Cast<object>().First(k => (Convert.ToString(k, CultureInfo.InvariantCulture) ?? "") == text);

        static NotSupportedException Unsupported(string path, string what) =>
            new NotSupportedException($"Cannot write {what} at '{path}' to a .bite JSON body.");
    }
}
