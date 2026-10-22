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
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Dev2;
using Warewolf.Bite.Flow;
using Warewolf.Bite.Xaml;

namespace Warewolf.Bite.Json
{
    /// <summary>
    /// Reads a version 1 JSON body into the same <see cref="FlowchartModel"/> the XAML reader produces, so both formats
    /// are linked by the one <see cref="FlowGraphBuilder"/>. Types go through the same allowlist as XAML. Fails with
    /// <see cref="BiteFormatException"/> naming the JSON path; never returns a partial model.
    /// </summary>
    public sealed class BiteJsonReader
    {
        readonly XamlTypeResolver _resolver;
        readonly string _idSeed;

        public BiteJsonReader(XamlTypeResolver resolver, string idSeed)
        {
            _resolver = resolver;
            _idSeed = idSeed;
        }

        public FlowchartModel Read(string json)
        {
            JsonDocument document;
            try
            {
                document = JsonDocument.Parse(json);
            }
            catch (JsonException e)
            {
                throw new BiteFormatException($"BiteDefinition is not valid JSON: {e.Message}", null, e);
            }
            using (document)
            {
                var root = document.RootElement;
                var nodesElement = Property(root, "nodes", "$");
                var chart = new FlowchartModel();
                var nodes = new Dictionary<string, FlowNodeModel>(StringComparer.Ordinal);
                var links = new List<Action>();

                // First pass: create every node so successors may point anywhere, forwards included.
                foreach (var entry in nodesElement.EnumerateObject())
                {
                    var path = $"$.nodes.{entry.Name}";
                    var kind = String(Property(entry.Value, "kind", path), path + ".kind");
                    FlowNodeModel node = kind switch
                    {
                        "step" => new FlowStepModel(),
                        "decision" => new FlowDecisionModel(),
                        "switch" => new FlowSwitchModel(),
                        _ => throw Error($"Unknown node kind '{kind}'", path),
                    };
                    node.Name = entry.Name;
                    nodes[entry.Name] = node;
                    chart.Nodes.Add(node);
                }

                FlowNodeModel? Link(JsonElement element, string path) =>
                    element.ValueKind == JsonValueKind.Null || element.ValueKind == JsonValueKind.Undefined ? null
                    : nodes.TryGetValue(String(element, path), out var target) ? target
                    : throw Error($"Successor '{element.GetString()}' is not a node", path);

                foreach (var entry in nodesElement.EnumerateObject())
                {
                    var path = $"$.nodes.{entry.Name}";
                    var element = entry.Value;
                    switch (nodes[entry.Name])
                    {
                        case FlowStepModel step:
                            step.Action = ReadValue(Optional(element, "activity"), typeof(object), path + ".activity");
                            step.Next = Link(Optional(element, "next"), path + ".next");
                            break;
                        case FlowDecisionModel decision:
                            decision.DisplayName = OptionalString(element, "displayName") ?? "Decision";
                            decision.Condition = ReadValue(Optional(element, "condition"), typeof(object), path + ".condition");
                            decision.True = Link(Optional(element, "true"), path + ".true");
                            decision.False = Link(Optional(element, "false"), path + ".false");
                            break;
                        case FlowSwitchModel flowSwitch:
                            flowSwitch.DisplayName = OptionalString(element, "displayName") ?? "Switch";
                            flowSwitch.Expression = ReadValue(Optional(element, "expression"), typeof(object), path + ".expression");
                            var cases = Optional(element, "cases");
                            if (cases.ValueKind == JsonValueKind.Object)
                            {
                                foreach (var c in cases.EnumerateObject())
                                {
                                    flowSwitch.Cases.Add(new KeyValuePair<string, FlowNodeModel?>(c.Name, Link(c.Value, $"{path}.cases.{c.Name}")));
                                }
                            }
                            flowSwitch.Default = Link(Optional(element, "default"), path + ".default");
                            break;
                    }
                }

                chart.StartNode = Link(Optional(root, "start"), "$.start");
                return chart;
            }
        }

        object? ReadValue(JsonElement element, Type target, string path)
        {
            if (element.ValueKind == JsonValueKind.Null || element.ValueKind == JsonValueKind.Undefined)
            {
                return null;
            }
            if (WfArguments.IsArgumentType(target))
            {
                return ReadArgument(element, target, path);
            }
            if (WfArguments.IsDelegateType(target))
            {
                if (element.ValueKind != JsonValueKind.Object)
                {
                    throw Error("Expected {\"argument\", \"body\"}", path);
                }
                return WfArguments.CreateDelegate(target, OptionalString(element, "argument"), ReadValue(Optional(element, "body"), typeof(object), path + ".body"));
            }
            if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty("$type", out var typeName))
            {
                return ReadObject(element, String(typeName, path + ".$type"), target, path);
            }
            if (element.ValueKind == JsonValueKind.Object && DefinitionMembers.IsDictionary(target))
            {
                return ReadDictionary(element, target, path);
            }
            if (element.ValueKind == JsonValueKind.Array)
            {
                if (target.IsArray)
                {
                    var items = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(target.GetElementType()!))!;
                    FillCollection(items, element, target, path);
                    var array = Array.CreateInstance(target.GetElementType()!, items.Count);
                    items.CopyTo(array, 0);
                    return array;
                }
                var collection = CreateCollection(target, path);
                FillCollection(collection, element, target, path);
                return collection;
            }
            if (element.ValueKind == JsonValueKind.Object)
            {
                throw Error("An object value needs a \"$type\"", path);
            }
            return ReadScalar(element, target, path);
        }

        object ReadArgument(JsonElement element, Type argumentType, string path)
        {
            try
            {
                if (element.ValueKind == JsonValueKind.Object)
                {
                    if (element.TryGetProperty("$expr", out var expression))
                    {
                        return WfArguments.Expression(argumentType, String(expression, path + ".$expr"));
                    }
                    if (!element.EnumerateObject().Any())
                    {
                        return WfArguments.Empty(argumentType);
                    }
                    throw Error("An argument is a literal value, {\"$expr\": \"...\"}, or {}", path);
                }
                return WfArguments.Literal(argumentType, ReadValue(element, argumentType.GetGenericArguments()[0], path));
            }
            catch (FormatException e)
            {
                throw Error(e.Message, path);
            }
        }

        object ReadObject(JsonElement element, string typeName, Type target, string path)
        {
            var type = _resolver.ResolveFullName(typeName);
            if (!target.IsAssignableFrom(type))
            {
                throw Error($"'{typeName}' is not a {target.Name}", path);
            }
            object instance;
            try
            {
                instance = Activator.CreateInstance(type) ?? throw Error($"Could not create '{typeName}'", path);
            }
            catch (MissingMethodException e)
            {
                throw new BiteFormatException($"'{typeName}' has no public parameterless constructor (at {path})", null, e);
            }
            catch (TargetInvocationException e)
            {
                throw new BiteFormatException($"Constructing '{typeName}' failed at {path}: {e.InnerException?.Message}", null, e.InnerException);
            }

            var sawUniqueId = false;
            foreach (var member in element.EnumerateObject().Where(m => m.Name != "$type"))
            {
                var memberPath = $"{path}.{member.Name}";
                var property = DefinitionMembers.Find(type, member.Name) ?? throw Error($"'{type.Name}' has no member '{member.Name}'", memberPath);
                sawUniqueId |= member.Name == "UniqueID";
                if (property.SetMethod?.IsPublic != true)
                {
                    // A get-only collection: add the items to the instance the constructor made. Null means "as constructed".
                    if (member.Value.ValueKind != JsonValueKind.Null)
                    {
                        var existing = property.GetValue(instance) ?? throw Error($"'{member.Name}' is null and cannot be set", memberPath);
                        FillCollection(existing, member.Value, property.PropertyType, memberPath);
                    }
                    continue;
                }
                try
                {
                    property.SetValue(instance, ReadValue(member.Value, property.PropertyType, memberPath));
                }
                catch (Exception e) when (e is ArgumentException or TargetInvocationException)
                {
                    throw new BiteFormatException($"Could not set '{member.Name}' at {memberPath}: {(e.InnerException ?? e).Message}", null, e);
                }
            }
            if (instance is IDev2Activity activity && !sawUniqueId)
            {
                activity.UniqueID = StableId(path).ToString(); // hand-authored JSON without ids still gets stable ones
            }
            return instance;
        }

        object ReadDictionary(JsonElement element, Type target, string path)
        {
            var dictionary = (IDictionary)CreateCollection(target, path);
            var (keyType, valueType) = DictionaryTypes(dictionary.GetType());
            foreach (var entry in element.EnumerateObject())
            {
                dictionary.Add(ConvertText(entry.Name, keyType, $"{path}.{entry.Name}") ?? entry.Name, ReadValue(entry.Value, valueType, $"{path}.{entry.Name}"));
            }
            return dictionary;
        }

        void FillCollection(object collection, JsonElement array, Type declared, string path)
        {
            if (array.ValueKind != JsonValueKind.Array)
            {
                throw Error("Expected an array", path);
            }
            var itemType = DefinitionMembers.ItemType(collection.GetType());
            var index = 0;
            foreach (var item in array.EnumerateArray())
            {
                var value = ReadValue(item, itemType, $"{path}[{index++}]");
                try
                {
                    if (collection is IList list)
                    {
                        list.Add(value);
                    }
                    else
                    {
                        var add = collection.GetType().GetMethods().FirstOrDefault(m => m.Name == "Add" && m.GetParameters().Length == 1)
                                  ?? throw Error($"Cannot add items to '{collection.GetType().Name}'", path);
                        add.Invoke(collection, new[] { value });
                    }
                }
                catch (Exception e) when (e is ArgumentException or InvalidCastException or TargetInvocationException)
                {
                    throw new BiteFormatException($"Cannot add an item at {path}: {(e.InnerException ?? e).Message}", null, e);
                }
            }
        }

        // A concrete collection for a declared type: the type itself when concrete, else List<T>/Dictionary<K,V>.
        static object CreateCollection(Type declared, string path)
        {
            if (!declared.IsInterface && !declared.IsAbstract && declared != typeof(object))
            {
                return Activator.CreateInstance(declared) ?? throw Error($"Could not create '{declared.Name}'", path);
            }
            if (DefinitionMembers.IsDictionary(declared))
            {
                var generic = declared.GetInterfaces().Append(declared).FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>));
                var arguments = generic?.GetGenericArguments() ?? new[] { typeof(string), typeof(object) };
                return Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(arguments))!;
            }
            return Activator.CreateInstance(typeof(List<>).MakeGenericType(DefinitionMembers.ItemType(declared)))!;
        }

        static (Type Key, Type Value) DictionaryTypes(Type dictionaryType)
        {
            var generic = dictionaryType.GetInterfaces().Append(dictionaryType).FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>));
            return generic == null ? (typeof(object), typeof(object)) : (generic.GetGenericArguments()[0], generic.GetGenericArguments()[1]);
        }

        object? ReadScalar(JsonElement element, Type target, string path)
        {
            var underlying = Nullable.GetUnderlyingType(target) ?? target;
            switch (element.ValueKind)
            {
                case JsonValueKind.True:
                case JsonValueKind.False:
                    return underlying == typeof(object) || underlying == typeof(bool) ? element.GetBoolean() : throw Error($"A boolean is not a {underlying.Name}", path);
                case JsonValueKind.Number:
                    if (underlying == typeof(object))
                    {
                        return element.GetDecimal();
                    }
                    try
                    {
                        return Convert.ChangeType(element.GetRawText(), underlying, CultureInfo.InvariantCulture);
                    }
                    catch (Exception e) when (e is FormatException or OverflowException or InvalidCastException)
                    {
                        throw Error($"{element.GetRawText()} is not a valid {underlying.Name}", path);
                    }
                case JsonValueKind.String:
                    return ConvertText(element.GetString()!, underlying, path);
                default:
                    throw Error($"Unexpected {element.ValueKind}", path);
            }
        }

        object? ConvertText(string text, Type target, string path)
        {
            var underlying = Nullable.GetUnderlyingType(target) ?? target;
            try
            {
                if (underlying == typeof(string) || underlying == typeof(object))
                {
                    return text;
                }
                if (underlying.IsEnum)
                {
                    return Enum.Parse(underlying, text, ignoreCase: false);
                }
                if (underlying == typeof(Guid))
                {
                    return Guid.Parse(text);
                }
                if (underlying == typeof(DateTime))
                {
                    return DateTime.Parse(text, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
                }
                if (underlying == typeof(TimeSpan))
                {
                    return TimeSpan.ParseExact(text, "c", CultureInfo.InvariantCulture);
                }
                if (underlying == typeof(Uri))
                {
                    return new Uri(text, UriKind.RelativeOrAbsolute);
                }
                if (underlying == typeof(char))
                {
                    return text.Length == 1 ? text[0] : throw new FormatException("expected a single character");
                }
                if (underlying == typeof(Type))
                {
                    return _resolver.ResolveFullName(text);
                }
                if (underlying == typeof(bool) || underlying.IsPrimitive || underlying == typeof(decimal))
                {
                    return Convert.ChangeType(text, underlying, CultureInfo.InvariantCulture);
                }
            }
            catch (Exception e) when (e is FormatException or OverflowException or ArgumentException)
            {
                throw Error($"'{text}' is not a valid {underlying.Name}", path);
            }
            throw Error($"No conversion from text to '{underlying.Name}'", path);
        }

        static JsonElement Property(JsonElement element, string name, string path) =>
            element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out var value)
                ? value
                : throw Error($"Missing \"{name}\"", path);

        static JsonElement Optional(JsonElement element, string name) =>
            element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out var value) ? value : default;

        static string? OptionalString(JsonElement element, string name)
        {
            var value = Optional(element, name);
            return value.ValueKind == JsonValueKind.String ? value.GetString() : null;
        }

        static string String(JsonElement element, string path) =>
            element.ValueKind == JsonValueKind.String ? element.GetString()! : throw Error("Expected a string", path);

        static BiteFormatException Error(string message, string path) => new BiteFormatException($"{message} (at {path})");

        // Stable per resource and JSON path, like the XAML reader's per-element ids.
        Guid StableId(string path)
        {
            using (var sha = SHA1.Create())
            {
                var hash = sha.ComputeHash(Encoding.UTF8.GetBytes($"{_idSeed}|{path}"));
                var bytes = new byte[16];
                Array.Copy(hash, bytes, 16);
                bytes[7] = (byte)((bytes[7] & 0x0F) | 0x50);
                bytes[8] = (byte)((bytes[8] & 0x3F) | 0x80);
                return new Guid(bytes);
            }
        }
    }
}
