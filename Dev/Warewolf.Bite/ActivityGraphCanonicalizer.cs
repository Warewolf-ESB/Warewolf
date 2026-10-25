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
using System.Reflection;
using System.Text.Json.Nodes;
using Dev2;

namespace Warewolf.Bite
{
    /// <summary>
    /// Serialises an <see cref="IDev2Activity"/> graph to canonical JSON so two readers' output can be
    /// compared property by property (WF removal T1.1 parity harness, T1.7 shadow mode).
    ///
    /// <para><b>Nodes</b> are keyed by traversal order from the start node (successors in property-name, then
    /// key, order), so <c>x:Reference</c> back-edges and shared successors appear once and identity does not
    /// depend on <see cref="IDev2Activity.UniqueID"/>, which is minted per load when the XAML lacks one. <b>Edges</b> are found structurally rather than by type:
    /// any property holding an activity, a collection or dictionary of activities, or a delegate wrapper
    /// (an object with a <c>Handler</c> activity) is an edge named after that property. That covers
    /// <c>NextNodes</c>, decision arms, switch cases, ForEach/Gate bodies and Sequence children alike.</para>
    ///
    /// <para>The form is <b>representation-neutral</b>: a WF <c>InArgument</c> holding a <c>Literal</c>
    /// serialises as the literal value, and a delegate wrapper as its argument name plus a body edge, so the
    /// comparison still holds after Workstream 2 swaps those WF types for plain ones. WF types are
    /// recognised by namespace, never referenced, so this assembly stays free of System.Activities.</para>
    /// </summary>
    public static class ActivityGraphCanonicalizer
    {
        const int MaxValueDepth = 8;
        // Backstop against walking a large runtime object graph; hitting it shows as "$truncated" in the output.
        const int MaxValuesPerProperty = 5000;
        const string WfNamespace = "System.Activities";

        // Members declared on WF base types are plumbing (Id, Constraints, ImplementationVersion, Result...) and are
        // skipped, except these, which are workflow definition: they are in the XAML and in debug output.
        static readonly HashSet<string> WfDefinitionMembers = new HashSet<string>(StringComparer.Ordinal)
        {
            "DisplayName",
        };

        // Runtime state and services rather than definition: never part of what a reader produces, and
        // ExecutionEnvironment in particular is a large graph that is expensive to walk.
        static readonly HashSet<string> IgnoredProperties = new HashSet<string>(StringComparer.Ordinal)
        {
            "Compiler",
            "DataObject",
            "ExecutionEnvironment",
            "ScopingObject",
        };

        /// <summary>Canonical JSON for the graph reachable from <paramref name="start"/>.</summary>
        public static JsonObject Canonicalize(IDev2Activity? start)
        {
            var nodes = new List<(string Id, JsonObject Node)>();
            var edges = new List<(string From, string Kind, string Key, string To)>();
            var visited = new Dictionary<object, string>(ReferenceEqualityComparer.Instance);
            var pending = new Queue<IDev2Activity>();

            if (start != null)
            {
                Enqueue(start, visited, pending);
            }
            while (pending.Count > 0)
            {
                var node = pending.Dequeue();
                var id = visited[node];
                nodes.Add((id, DescribeNode(node, id, visited, pending, edges)));
            }

            var edgeArray = new JsonArray();
            foreach (var edge in edges.OrderBy(e => e.From, StringComparer.Ordinal)
                                      .ThenBy(e => e.Kind, StringComparer.Ordinal)
                                      .ThenBy(e => e.Key, StringComparer.Ordinal)
                                      .ThenBy(e => e.To, StringComparer.Ordinal))
            {
                edgeArray.Add(new JsonArray(edge.From, edge.Kind, edge.Key, edge.To));
            }

            var nodeObject = new JsonObject();
            foreach (var (nodeId, node) in nodes)
            {
                nodeObject[nodeId] = node;
            }

            return new JsonObject
            {
                ["start"] = start == null ? null : visited[start],
                ["nodes"] = nodeObject,
                ["edges"] = edgeArray,
            };
        }

        /// <summary>Indented canonical JSON text; equal graphs give byte-identical text.</summary>
        public static string ToJson(IDev2Activity? start) =>
            Canonicalize(start).ToJsonString(new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

        static string Enqueue(IDev2Activity activity, Dictionary<object, string> visited, Queue<IDev2Activity> pending)
        {
            if (visited.TryGetValue(activity, out var existing))
            {
                return existing;
            }
            // Identity is traversal order, not UniqueID: an activity whose XAML carries no UniqueID gets a fresh
            // Guid from its constructor on every load (DsfNativeActivity), so UniqueID is not stable for those.
            // UniqueID is still compared, as an ordinary property.
            var id = $"n{visited.Count:D4}";
            visited[activity] = id;
            pending.Enqueue(activity);
            return id;
        }

        static JsonObject DescribeNode(IDev2Activity node, string id, Dictionary<object, string> visited,
            Queue<IDev2Activity> pending, List<(string, string, string, string)> edges)
        {
            var properties = new SortedDictionary<string, JsonNode?>(StringComparer.Ordinal);
            // Name order fixes the order successors are discovered in, and so their traversal-order ids.
            foreach (var property in ReadableProperties(node.GetType()).OrderBy(p => p.Name, StringComparer.Ordinal))
            {
                if (IgnoredProperties.Contains(property.Name))
                {
                    continue;
                }
                object? value;
                try
                {
                    value = property.GetValue(node);
                }
                catch (Exception e)
                {
                    properties[property.Name] = $"!error:{(e.InnerException ?? e).GetType().Name}";
                    continue;
                }

                if (TryAddEdges(id, property, value, visited, pending, edges, out var edgeAnnotation))
                {
                    if (edgeAnnotation != null)
                    {
                        properties[property.Name] = edgeAnnotation;
                    }
                    continue;
                }
                properties[property.Name] = Value(value, 0, new Walk());
            }

            var result = new JsonObject { ["$type"] = node.GetType().Name };
            foreach (var pair in properties)
            {
                result[pair.Key] = pair.Value;
            }
            return result;
        }

        // Edge-shaped values. Null and empty edge collections are equivalent: both mean "no successor".
        static bool TryAddEdges(string from, PropertyInfo property, object? value, Dictionary<object, string> visited,
            Queue<IDev2Activity> pending, List<(string, string, string, string)> edges, out JsonNode? annotation)
        {
            annotation = null;
            if (value is IDev2Activity single)
            {
                edges.Add((from, property.Name, "", Enqueue(single, visited, pending)));
                return true;
            }
            if (value is IDictionary dictionary && dictionary.Values.Cast<object?>().All(v => v is IDev2Activity))
            {
                foreach (var entry in Entries(dictionary).OrderBy(e => Convert.ToString(e.Key, CultureInfo.InvariantCulture), StringComparer.Ordinal))
                {
                    edges.Add((from, property.Name, Convert.ToString(entry.Key, CultureInfo.InvariantCulture) ?? "", Enqueue((IDev2Activity)entry.Value!, visited, pending)));
                }
                return dictionary.Count > 0 || IsActivityContainerType(property.PropertyType);
            }
            if (value is IEnumerable sequence && value is not string)
            {
                var items = sequence.Cast<object?>().ToList();
                if ((items.Count > 0 && items.All(i => i is IDev2Activity)) || (items.Count == 0 && IsActivityContainerType(property.PropertyType)))
                {
                    for (var i = 0; i < items.Count; i++)
                    {
                        edges.Add((from, property.Name, i.ToString(CultureInfo.InvariantCulture), Enqueue((IDev2Activity)items[i]!, visited, pending)));
                    }
                    return true;
                }
            }
            if (value == null && IsActivityContainerType(property.PropertyType))
            {
                return true;
            }
            if (value != null && IsDelegateWrapper(value.GetType(), out var handlerProperty, out var argumentProperty))
            {
                var handler = handlerProperty.GetValue(value);
                var argumentName = argumentProperty?.GetValue(value) is { } argument
                    ? Property(argument.GetType(), "Name")?.GetValue(argument) as string
                    : null;
                annotation = new JsonObject { ["argument"] = argumentName };
                if (handler is IDev2Activity body)
                {
                    edges.Add((from, property.Name, "", Enqueue(body, visited, pending)));
                }
                return true;
            }
            return false;
        }

        // LINQ's Cast<DictionaryEntry>() fails on generic dictionaries (they enumerate KeyValuePair); go through
        // the non-generic enumerator instead, which every IDictionary yields as DictionaryEntry.
        static IEnumerable<DictionaryEntry> Entries(IDictionary dictionary)
        {
            var enumerator = dictionary.GetEnumerator();
            while (enumerator.MoveNext())
            {
                yield return enumerator.Entry;
            }
        }

        static bool IsActivityContainerType(Type type)
        {
            if (typeof(IDev2Activity).IsAssignableFrom(type))
            {
                return true;
            }
            var element = type.IsGenericType ? type.GetGenericArguments().LastOrDefault() : null;
            return element != null && typeof(IEnumerable).IsAssignableFrom(type)
                   && (typeof(IDev2Activity).IsAssignableFrom(element) || element.FullName == WfNamespace + ".Activity");
        }

        // ActivityFunc/ActivityAction (and their Workstream 2 replacement): a Handler activity plus an optional argument.
        static bool IsDelegateWrapper(Type type, out PropertyInfo handler, out PropertyInfo? argument)
        {
            handler = Property(type, "Handler")!;
            argument = Property(type, "Argument");
            return handler != null && type.Namespace?.StartsWith(WfNamespace, StringComparison.Ordinal) == true;
        }

        static IEnumerable<PropertyInfo> ReadableProperties(Type type) =>
            type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                // Public getters only: reflection will happily call a protected one, and DsfNativeActivity.ResourceCatalog's
                // protected getter falls back to ResourceCatalog.Instance, which loads the server catalog from disk.
                .Where(p => p.GetMethod?.IsPublic == true && p.GetIndexParameters().Length == 0
                            && (p.DeclaringType?.Namespace?.StartsWith(WfNamespace, StringComparison.Ordinal) != true
                                || WfDefinitionMembers.Contains(p.Name)))
                .GroupBy(p => p.Name, StringComparer.Ordinal)
                .Select(g => g.OrderByDescending(p => Depth(p.DeclaringType)).First()); // `new`-hidden members: most derived wins

        // GetProperty(name) throws AmbiguousMatchException on WF types, which re-declare members with `new`
        // (InArgument<T>.Expression hides Argument.Expression); take the most derived one.
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

        static JsonNode? Value(object? value, int depth, Walk walk)
        {
            if (++walk.Count > MaxValuesPerProperty)
            {
                return new JsonObject { ["$truncated"] = true };
            }
            switch (value)
            {
                case null:
                    return null;
                case string s:
                    return s;
                case bool b:
                    return b;
                case Enum e:
                    return e.ToString();
                case Guid g:
                    return g.ToString("D");
                case DateTime dt:
                    return dt.ToString("O", CultureInfo.InvariantCulture);
                case IFormattable f when value.GetType().IsPrimitive || value is decimal:
                    return f.ToString(value is double || value is float ? "R" : null, CultureInfo.InvariantCulture);
                case IDev2Activity activity:
                    return new JsonObject { ["$ref"] = activity.UniqueID };
                case Type t:
                    return t.FullName;
            }

            var type = value.GetType();
            if (type.Namespace?.StartsWith(WfNamespace, StringComparison.Ordinal) == true)
            {
                return WfValue(value, depth, walk);
            }
            if (depth >= MaxValueDepth || !walk.Seen.Add(value))
            {
                return new JsonObject { ["$type"] = type.Name, ["$truncated"] = true };
            }
            try
            {
                if (value is IDictionary dictionary)
                {
                    var map = new JsonObject();
                    foreach (var entry in Entries(dictionary).OrderBy(e => Convert.ToString(e.Key, CultureInfo.InvariantCulture), StringComparer.Ordinal))
                    {
                        map[Convert.ToString(entry.Key, CultureInfo.InvariantCulture) ?? ""] = Value(entry.Value, depth + 1, walk);
                    }
                    return map;
                }
                if (value is IEnumerable sequence)
                {
                    var array = new JsonArray();
                    foreach (var item in sequence)
                    {
                        if (walk.Count >= MaxValuesPerProperty)
                        {
                            array.Add(new JsonObject { ["$truncated"] = true }); // also stops an endless lazy sequence
                            break;
                        }
                        array.Add(Value(item, depth + 1, walk));
                    }
                    return array;
                }
                if (!IsWarewolfType(type))
                {
                    return new JsonObject { ["$type"] = type.FullName };
                }
                var result = new JsonObject { ["$type"] = type.Name };
                foreach (var property in ReadableProperties(type).OrderBy(p => p.Name, StringComparer.Ordinal))
                {
                    try
                    {
                        result[property.Name] = Value(property.GetValue(value), depth + 1, walk);
                    }
                    catch (Exception e)
                    {
                        result[property.Name] = $"!error:{(e.InnerException ?? e).GetType().Name}";
                    }
                }
                return result;
            }
            catch (Exception e)
            {
                // A lazily-evaluated collection can throw while enumerating; record it rather than fail the walk.
                return $"!error:{(e.InnerException ?? e).GetType().Name}";
            }
        }

        sealed class Walk
        {
            public HashSet<object> Seen { get; } = new HashSet<object>(ReferenceEqualityComparer.Instance);
            public int Count { get; set; }
        }

        // InArgument/OutArgument/InOutArgument and expressions: a Literal is its value; anything else is its
        // expression text, which is what the XAML carried.
        static JsonNode? WfValue(object value, int depth, Walk walk)
        {
            var type = value.GetType();
            var expression = Property(type, "Expression")?.GetValue(value);
            if (expression == null)
            {
                return Property(type, "Expression") != null ? null : new JsonObject { ["$type"] = type.Name };
            }
            var expressionType = expression.GetType();
            if (expressionType.Name.StartsWith("Literal", StringComparison.Ordinal))
            {
                return Value(Property(expressionType, "Value")?.GetValue(expression), depth + 1, walk);
            }
            var text = Property(expressionType, "ExpressionText")?.GetValue(expression) as string;
            return new JsonObject { ["$expr"] = text ?? expression.ToString() };
        }

        static bool IsWarewolfType(Type type)
        {
            var name = type.Assembly.GetName().Name ?? "";
            return name.StartsWith("Dev2", StringComparison.Ordinal) || name.StartsWith("Warewolf", StringComparison.Ordinal);
        }
    }
}
