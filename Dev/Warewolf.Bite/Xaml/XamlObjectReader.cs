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
using System.Xml.Linq;
using Dev2;
using Warewolf.Bite.Flow;

namespace Warewolf.Bite.Xaml
{
    /// <summary>
    /// Reads the subset of XAML that <c>.bite</c> workflows use into a <see cref="FlowchartModel"/> whose steps hold
    /// fully constructed activities. Replaces <c>ActivityXamlServices.Load</c>: no WF schema, no metadata pass, and so
    /// no VB compilation (WF removal T1.2-T1.4).
    /// </summary>
    public sealed class XamlObjectReader
    {
        static readonly XNamespace Wf = XamlTypeResolver.WfNamespace;
        static readonly XNamespace X = XamlTypeResolver.XamlNamespace;

        // Designer-only namespaces: everything in them is skipped (view state, hint sizes, debugger symbols).
        static readonly HashSet<string> IgnoredNamespaces = new HashSet<string>(StringComparer.Ordinal)
        {
            "http://schemas.microsoft.com/netfx/2009/xaml/activities/presentation",
            "http://schemas.microsoft.com/netfx/2010/xaml/activities/presentation",
            "http://schemas.microsoft.com/netfx/2010/xaml/activities/debugger",
            "http://schemas.openxmlformats.org/markup-compatibility/2006",
            "http://schemas.microsoft.com/winfx/2006/xaml/presentation",
            "clr-namespace:Microsoft.VisualBasic.Activities;assembly=System.Activities",
        };

        // WF plumbing that never reaches the executable graph.
        static readonly HashSet<string> IgnoredWfMembers = new HashSet<string>(StringComparer.Ordinal)
        {
            "TextExpression.NamespacesForImplementation", "TextExpression.ReferencesForImplementation",
            "TextExpression.Namespaces", "TextExpression.References", "Flowchart.Variables",
        };

        // Set by every activity's constructor to the same VB references the XAML repeats on each element
        // (DsfActivityAbstract); re-reading them would only cost an expression object each.
        static readonly HashSet<string> ConstructorSetMembers = new HashSet<string>(StringComparer.Ordinal)
        {
            "HasError", "IsValid", "InstructionList",
        };

        readonly XamlTypeResolver _resolver;
        readonly string _idSeed;
        readonly Dictionary<string, object?> _names = new Dictionary<string, object?>(StringComparer.Ordinal);
        readonly List<Action> _fixups = new List<Action>();
        readonly List<FlowNodeModel> _flowNodes = new List<FlowNodeModel>();

        /// <param name="idSeed">Seeds the stable UniqueID minted for activities whose XAML has none (the resource ID).</param>
        public XamlObjectReader(XamlTypeResolver resolver, string idSeed)
        {
            _resolver = resolver;
            _idSeed = idSeed;
        }

        /// <summary>The flowchart of a <c>&lt;Activity&gt;</c> root, or null when its implementation is not a Flowchart.</summary>
        public FlowchartModel? ReadWorkflow(XElement root)
        {
            if (root.Name != Wf + "Activity")
            {
                throw new BiteFormatException($"Expected an <Activity> root, found <{root.Name.LocalName}>", root);
            }
            var implementation = root.Elements().FirstOrDefault(e => !IsSkipped(e) && !IsPropertyElement(e) && e.Name != X + "Members");
            if (implementation == null || implementation.Name != Wf + "Flowchart")
            {
                return null;
            }
            var chart = ReadFlowchart(implementation);
            chart.Nodes.AddRange(_flowNodes);
            foreach (var fixup in _fixups)
            {
                fixup();
            }
            return chart;
        }

        // ---------------------------------------------------------------- flow constructs

        FlowchartModel ReadFlowchart(XElement element)
        {
            var chart = new FlowchartModel();
            foreach (var child in element.Elements().Where(e => !IsSkipped(e)))
            {
                if (child.Name == Wf + "Flowchart.StartNode")
                {
                    ReadFlowSlot(SingleChild(child), node => chart.StartNode = node);
                }
                else if (IsPropertyElement(child))
                {
                    if (!IgnoredWfMembers.Contains(child.Name.LocalName))
                    {
                        throw new BiteFormatException($"Unsupported Flowchart member '{child.Name.LocalName}'", child);
                    }
                }
                else
                {
                    ReadFlowSlot(child, _ => { }); // Flowchart.Nodes: every node is recorded as it is read (see ReadFlowNode)
                }
            }
            return chart;
        }

        // A flow successor: an inline node, an <x:Reference>, or <x:Null/>; resolved after the document is read.
        void ReadFlowSlot(XElement? element, Action<FlowNodeModel?> assign)
        {
            if (element == null || element.Name == X + "Null")
            {
                assign(null);
                return;
            }
            if (element.Name == X + "Reference")
            {
                DeferFlowReference(ReferenceName(element), element, assign);
                return;
            }
            assign(ReadFlowNode(element));
        }

        void ReadFlowAttribute(XAttribute attribute, Action<FlowNodeModel?> assign)
        {
            switch (ParseMarkup(attribute.Value, attribute))
            {
                case NullMarkup:
                    assign(null);
                    break;
                case ReferenceMarkup reference:
                    DeferFlowReference(reference.Name, attribute, assign);
                    break;
                default:
                    throw new BiteFormatException($"Expected {{x:Reference}} or {{x:Null}} for '{attribute.Name.LocalName}'", attribute);
            }
        }

        void DeferFlowReference(string name, XObject at, Action<FlowNodeModel?> assign) =>
            _fixups.Add(() =>
            {
                if (!_names.TryGetValue(name, out var target))
                {
                    throw new BiteFormatException($"x:Reference to unknown name '{name}'", at);
                }
                assign(target as FlowNodeModel ?? throw new BiteFormatException($"x:Reference '{name}' is not a flow node", at));
            });

        FlowNodeModel ReadFlowNode(XElement element)
        {
            FlowNodeModel node;
            if (element.Name == Wf + "FlowStep")
            {
                node = ReadFlowStep(element);
            }
            else if (element.Name == Wf + "FlowDecision")
            {
                node = ReadFlowDecision(element);
            }
            else if (element.Name == Wf + "FlowSwitch")
            {
                node = ReadFlowSwitch(element);
            }
            else
            {
                throw new BiteFormatException($"Expected a flow node, found <{element.Name.LocalName}>", element);
            }
            Register(element, node);
            _flowNodes.Add(node);
            return node;
        }

        FlowStepModel ReadFlowStep(XElement element)
        {
            var step = new FlowStepModel();
            RejectUnknownAttributes(element);
            foreach (var child in element.Elements().Where(e => !IsSkipped(e)))
            {
                if (child.Name == Wf + "FlowStep.Next")
                {
                    ReadFlowSlot(SingleChild(child), next => step.Next = next);
                }
                else if (child.Name == X + "Key")
                {
                    // the key belongs to the enclosing FlowSwitch
                }
                else if (IsPropertyElement(child))
                {
                    throw new BiteFormatException($"Unsupported FlowStep member '{child.Name.LocalName}'", child);
                }
                else
                {
                    step.Action = ReadValue(child); // content property: FlowStep.Action
                }
            }
            return step;
        }

        FlowDecisionModel ReadFlowDecision(XElement element)
        {
            // WF's defaults when the attribute is absent; ActivityParser copies the switch's onto DsfSwitch.
            var decision = new FlowDecisionModel { DisplayName = (string?)element.Attribute("DisplayName") ?? "Decision" };
            foreach (var attribute in MemberAttributes(element))
            {
                switch (attribute.Name.LocalName)
                {
                    case "DisplayName":
                        break;
                    case "True":
                        ReadFlowAttribute(attribute, n => decision.True = n);
                        break;
                    case "False":
                        ReadFlowAttribute(attribute, n => decision.False = n);
                        break;
                    case "Condition":
                        DeferObjectReference(attribute, v => decision.Condition = v);
                        break;
                    default:
                        throw new BiteFormatException($"Unsupported FlowDecision member '{attribute.Name.LocalName}'", attribute);
                }
            }
            foreach (var child in element.Elements().Where(e => !IsSkipped(e)))
            {
                switch (child.Name.LocalName)
                {
                    case "FlowDecision.Condition":
                        decision.Condition = ReadValue(SingleChild(child)!);
                        break;
                    case "FlowDecision.True":
                        ReadFlowSlot(SingleChild(child), n => decision.True = n);
                        break;
                    case "FlowDecision.False":
                        ReadFlowSlot(SingleChild(child), n => decision.False = n);
                        break;
                    case "Key" when child.Name.Namespace == X:
                        break;
                    default:
                        throw new BiteFormatException($"Unsupported FlowDecision member '{child.Name.LocalName}'", child);
                }
            }
            return decision;
        }

        FlowSwitchModel ReadFlowSwitch(XElement element)
        {
            var flowSwitch = new FlowSwitchModel { DisplayName = (string?)element.Attribute("DisplayName") ?? "Switch" };
            foreach (var attribute in MemberAttributes(element))
            {
                switch (attribute.Name.LocalName)
                {
                    case "DisplayName":
                        break;
                    case "Default":
                        ReadFlowAttribute(attribute, n => flowSwitch.Default = n);
                        break;
                    case "Expression":
                        DeferObjectReference(attribute, v => flowSwitch.Expression = v);
                        break;
                    default:
                        throw new BiteFormatException($"Unsupported FlowSwitch member '{attribute.Name.LocalName}'", attribute);
                }
            }
            foreach (var child in element.Elements().Where(e => !IsSkipped(e)))
            {
                if (child.Name == Wf + "FlowSwitch.Expression")
                {
                    flowSwitch.Expression = ReadValue(SingleChild(child)!);
                }
                else if (child.Name == Wf + "FlowSwitch.Default")
                {
                    ReadFlowSlot(SingleChild(child), n => flowSwitch.Default = n);
                }
                else if (child.Name == X + "Key")
                {
                    // the switch's own key within an enclosing switch
                }
                else if (IsPropertyElement(child))
                {
                    throw new BiteFormatException($"Unsupported FlowSwitch member '{child.Name.LocalName}'", child);
                }
                else
                {
                    // content: FlowSwitch.Cases, each keyed by x:Key
                    var key = KeyOf(child) ?? throw new BiteFormatException("FlowSwitch case without x:Key", child);
                    var index = flowSwitch.Cases.Count;
                    flowSwitch.Cases.Add(new KeyValuePair<string, FlowNodeModel?>(key, null));
                    ReadFlowSlot(child, n => flowSwitch.Cases[index] = new KeyValuePair<string, FlowNodeModel?>(key, n));
                }
            }
            return flowSwitch;
        }

        static string? KeyOf(XElement element) =>
            (string?)element.Attribute(X + "Key") ?? element.Element(X + "Key")?.Value;

        // <x:Reference>name<x:Key>k</x:Key></x:Reference>: the name is the element's own text, not its descendants'.
        static string ReferenceName(XElement element) => string.Concat(element.Nodes().OfType<XText>().Select(t => t.Value)).Trim();

        // ---------------------------------------------------------------- objects

        object? ReadValue(XElement element)
        {
            if (element.Name == X + "Null")
            {
                return null;
            }
            if (element.Name.Namespace == X && element.Name.LocalName != "Reference")
            {
                var intrinsic = _resolver.Resolve(element.Name, Array.Empty<Type>(), element);
                return Convert(element.Value, intrinsic, element);
            }
            if (element.Name.Namespace == Wf && element.Name.LocalName is "FlowStep" or "FlowDecision" or "FlowSwitch")
            {
                return ReadFlowNode(element);
            }

            var type = _resolver.Resolve(element.Name, TypeArguments(element), element);
            object instance;
            try
            {
                instance = Activator.CreateInstance(type)
                           ?? throw new BiteFormatException($"Could not create '{type.Name}'", element);
            }
            catch (MissingMethodException e)
            {
                throw new BiteFormatException($"'{type.Name}' has no public parameterless constructor", element, e);
            }
            catch (TargetInvocationException e)
            {
                throw new BiteFormatException($"Constructing '{type.Name}' failed: {e.InnerException?.Message}", element, e.InnerException);
            }

            Register(element, instance);
            ApplyMembers(instance, element);
            if (instance is IDev2Activity activity && !HasMember(element, "UniqueID"))
            {
                activity.UniqueID = StableId(element).ToString();
            }
            return instance;
        }

        void ApplyMembers(object instance, XElement element)
        {
            var type = instance.GetType();
            foreach (var attribute in MemberAttributes(element))
            {
                var name = attribute.Name.LocalName;
                if (name.Contains('.'))
                {
                    if (IgnoredWfMembers.Contains(name))
                    {
                        continue;
                    }
                    throw new BiteFormatException($"Unsupported attached member '{name}'", attribute);
                }
                if (ConstructorSetMembers.Contains(name) && attribute.Value == $"[{name}]")
                {
                    continue;
                }
                var property = Member(type, name, attribute);
                switch (ParseMarkup(attribute.Value, attribute))
                {
                    case NullMarkup:
                        Assign(instance, property, null, attribute);
                        break;
                    case ReferenceMarkup reference:
                        DeferObjectReference(attribute, v => Assign(instance, property, v, attribute), reference.Name);
                        break;
                    case string text:
                        Assign(instance, property, ConvertMember(text, property.PropertyType, attribute), attribute);
                        break;
                }
            }

            var contentChildren = new List<XElement>();
            foreach (var child in element.Elements().Where(e => !IsSkipped(e)))
            {
                if (child.Name == X + "Key" || child.Name == X + "Members")
                {
                    continue;
                }
                if (IsPropertyElement(child))
                {
                    var memberName = child.Name.LocalName.Substring(child.Name.LocalName.IndexOf('.') + 1);
                    if (IgnoredWfMembers.Contains(child.Name.LocalName))
                    {
                        continue;
                    }
                    SetFromElements(instance, Member(type, memberName, child), child.Elements().Where(e => !IsSkipped(e)).ToList(), child);
                }
                else
                {
                    contentChildren.Add(child);
                }
            }
            if (contentChildren.Count > 0 && IsCollection(type) && ContentPropertyName(type) == null)
            {
                // A collection's own content is its items: <scg:List><ActivityDTO/>...</scg:List>.
                foreach (var child in contentChildren)
                {
                    AddItem(instance, child.Name == X + "Reference" ? Resolved(ReferenceName(child), child) : ReadValue(child), KeyOf(child), child);
                }
            }
            else if (contentChildren.Count > 0)
            {
                var contentName = ContentPropertyName(type)
                    ?? throw new BiteFormatException($"'{type.Name}' has no content property for <{contentChildren[0].Name.LocalName}>", contentChildren[0]);
                SetFromElements(instance, Member(type, contentName, element), contentChildren, element);
            }
        }

        // A property element's or content's child elements: one value to assign, or items to add to a collection.
        void SetFromElements(object instance, PropertyInfo property, IReadOnlyList<XElement> children, XElement at)
        {
            if (children.Count == 1 && !IsAddTarget(instance, property, children[0]))
            {
                var child = children[0];
                if (child.Name == X + "Reference")
                {
                    DeferObjectReference(child, v => Assign(instance, property, v, child), ReferenceName(child));
                    return;
                }
                Assign(instance, property, ReadValue(child), child);
                return;
            }

            var collection = property.GetValue(instance);
            if (collection == null)
            {
                if (property.SetMethod?.IsPublic != true || property.PropertyType.IsInterface || property.PropertyType.IsAbstract)
                {
                    throw new BiteFormatException($"'{property.Name}' is null and cannot be created to hold items", at);
                }
                collection = Activator.CreateInstance(property.PropertyType)!;
                property.SetValue(instance, collection);
            }
            foreach (var child in children)
            {
                AddItem(collection, child.Name == X + "Reference" ? Resolved(ReferenceName(child), child) : ReadValue(child), KeyOf(child), child);
            }
        }

        // Several children always mean "add". One child means "add" when the property holds a collection the child
        // is not itself an instance of (e.g. <scg:List> assigned vs. a single <ActivityDTO> added).
        bool IsAddTarget(object instance, PropertyInfo property, XElement child)
        {
            if (!IsCollection(property.PropertyType) || child.Name == X + "Null" || child.Name == X + "Reference")
            {
                return false;
            }
            if (child.Name.Namespace == X || child.Name.Namespace == Wf && child.Name.LocalName.StartsWith("Flow", StringComparison.Ordinal))
            {
                return true;
            }
            var childType = _resolver.Resolve(child.Name, TypeArguments(child), child);
            return !property.PropertyType.IsAssignableFrom(childType);
        }

        static bool IsCollection(Type type) =>
            type != typeof(string) && (typeof(IList).IsAssignableFrom(type) || typeof(IDictionary).IsAssignableFrom(type)
                                       || type.GetInterfaces().Append(type).Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollection<>)));

        static void AddItem(object collection, object? item, string? key, XElement at)
        {
            try
            {
                if (collection is IDictionary dictionary)
                {
                    dictionary.Add(key ?? throw new BiteFormatException("Dictionary item without x:Key", at), item);
                    return;
                }
                if (collection is IList list)
                {
                    list.Add(item);
                    return;
                }
                var add = collection.GetType().GetMethods().FirstOrDefault(m => m.Name == "Add" && m.GetParameters().Length == 1)
                          ?? throw new BiteFormatException($"Cannot add items to '{collection.GetType().Name}'", at);
                add.Invoke(collection, new[] { item });
            }
            catch (Exception e) when (e is ArgumentException or InvalidCastException or TargetInvocationException)
            {
                // e.g. Recordset - Unique Records.bite nests a collection where an item belongs; WF rejects it too.
                throw new BiteFormatException($"Cannot add '{item?.GetType().Name ?? "null"}' to '{collection.GetType().Name}': {(e.InnerException ?? e).Message}", at, e);
            }
        }

        void Assign(object instance, PropertyInfo property, object? value, XObject at)
        {
            if (property.SetMethod?.IsPublic != true)
            {
                throw new BiteFormatException($"'{property.Name}' cannot be set", at);
            }
            try
            {
                property.SetValue(instance, value);
            }
            catch (Exception e) when (e is ArgumentException or TargetInvocationException)
            {
                throw new BiteFormatException($"Could not set '{property.Name}': {(e.InnerException ?? e).Message}", at, e);
            }
        }

        // ---------------------------------------------------------------- names and references

        void Register(XElement element, object? instance)
        {
            var name = (string?)element.Attribute(X + "Name");
            if (name == null)
            {
                return;
            }
            if (_names.ContainsKey(name))
            {
                throw new BiteFormatException($"Duplicate x:Name '{name}'", element);
            }
            _names[name] = instance;
        }

        object? Resolved(string name, XObject at) =>
            _names.TryGetValue(name, out var value) ? value : throw new BiteFormatException($"x:Reference to unknown name '{name}'", at);

        void DeferObjectReference(XObject at, Action<object?> assign, string? name = null)
        {
            if (name == null)
            {
                var markup = ParseMarkup(((XAttribute)at).Value, at);
                if (markup is NullMarkup)
                {
                    assign(null);
                    return;
                }
                name = (markup as ReferenceMarkup)?.Name ?? throw new BiteFormatException("Expected {x:Reference}", at);
            }
            var target = name;
            _fixups.Add(() => assign(Resolved(target, at)));
        }

        // ---------------------------------------------------------------- values

        sealed class NullMarkup
        {
            public static readonly NullMarkup Instance = new NullMarkup();
        }

        sealed class ReferenceMarkup
        {
            public ReferenceMarkup(string name) => Name = name;
            public string Name { get; }
        }

        static object ParseMarkup(string value, XObject at)
        {
            if (!value.StartsWith("{", StringComparison.Ordinal))
            {
                return value;
            }
            if (value.StartsWith("{}", StringComparison.Ordinal))
            {
                return value.Substring(2); // escaped literal brace
            }
            var body = value.Trim('{', '}').Trim();
            if (body == "x:Null")
            {
                return NullMarkup.Instance;
            }
            if (body.StartsWith("x:Reference ", StringComparison.Ordinal))
            {
                return new ReferenceMarkup(body.Substring("x:Reference ".Length).Trim());
            }
            throw new BiteFormatException($"Unsupported markup extension '{value}'", at);
        }

        object? ConvertMember(string text, Type type, XObject at) =>
            WfArguments.IsArgumentType(type)
                ? WrapFormat(() => WfArguments.FromText(type, text, (t, target) => Convert(t, target, at)), at)
                : Convert(text, type, at);

        object? Convert(string text, Type type, XObject at)
        {
            var target = Nullable.GetUnderlyingType(type) ?? type;
            try
            {
                if (target == typeof(string) || target == typeof(object))
                {
                    // System.Xaml normalises line ends in member text (&#xD;&#xA; arrives as "\n"); match it.
                    return text.Replace("\r\n", "\n").Replace('\r', '\n');
                }
                if (target == typeof(Type))
                {
                    // e.g. DbColumn.DataType="x:String": a XAML type name, resolved in the attribute's scope.
                    var scope = at as XElement ?? at.Parent ?? throw new FormatException("no scope for a type name");
                    return _resolver.Resolve(XamlTypeName.ParseList(text).Single(), scope);
                }
                if (target == typeof(bool))
                {
                    return bool.Parse(text);
                }
                if (target.IsEnum)
                {
                    return Enum.Parse(target, text, ignoreCase: true);
                }
                if (target == typeof(Guid))
                {
                    return Guid.Parse(text);
                }
                if (target == typeof(TimeSpan))
                {
                    return TimeSpan.Parse(text, CultureInfo.InvariantCulture);
                }
                if (target == typeof(DateTime))
                {
                    return DateTime.Parse(text, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
                }
                if (target == typeof(Uri))
                {
                    return new Uri(text, UriKind.RelativeOrAbsolute);
                }
                if (target == typeof(char))
                {
                    return text.Length == 1 ? text[0] : throw new FormatException("expected a single character");
                }
                if (target.IsPrimitive || target == typeof(decimal))
                {
                    return System.Convert.ChangeType(text, target, CultureInfo.InvariantCulture);
                }
            }
            catch (Exception e) when (e is FormatException or OverflowException or ArgumentException)
            {
                throw new BiteFormatException($"'{text}' is not a valid {target.Name}", at, e);
            }
            throw new BiteFormatException($"No conversion from text to '{target.Name}'", at);
        }

        static T WrapFormat<T>(Func<T> read, XObject at)
        {
            try
            {
                return read();
            }
            catch (FormatException e)
            {
                throw new BiteFormatException(e.Message, at, e);
            }
        }

        // ---------------------------------------------------------------- reflection helpers

        IReadOnlyList<Type> TypeArguments(XElement element)
        {
            var text = (string?)element.Attribute(X + "TypeArguments");
            if (text == null)
            {
                return Array.Empty<Type>();
            }
            try
            {
                return XamlTypeName.ParseList(text).Select(n => _resolver.Resolve(n, element)).ToList();
            }
            catch (FormatException e)
            {
                throw new BiteFormatException(e.Message, element, e);
            }
        }

        static PropertyInfo Member(Type type, string name, XObject at) =>
            type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.Name == name && p.GetIndexParameters().Length == 0)
                .OrderByDescending(p => Depth(p.DeclaringType))
                .FirstOrDefault()
            ?? throw new BiteFormatException($"'{type.Name}' has no member '{name}'", at);

        static int Depth(Type? type)
        {
            var depth = 0;
            for (var t = type; t != null; t = t.BaseType)
            {
                depth++;
            }
            return depth;
        }

        // [ContentProperty] is recognised by name: it lives in System.Xaml, which this assembly does not reference.
        static string? ContentPropertyName(Type type)
        {
            for (var t = type; t != null; t = t.BaseType)
            {
                var attribute = t.GetCustomAttributes(false).FirstOrDefault(a => a.GetType().Name == "ContentPropertyAttribute");
                if (attribute != null)
                {
                    return attribute.GetType().GetProperty("Name")?.GetValue(attribute) as string;
                }
            }
            return null;
        }

        static IEnumerable<XAttribute> MemberAttributes(XElement element) =>
            element.Attributes().Where(a => !a.IsNamespaceDeclaration && a.Name.Namespace != X && !IgnoredNamespaces.Contains(a.Name.NamespaceName));

        static void RejectUnknownAttributes(XElement element)
        {
            var unknown = MemberAttributes(element).FirstOrDefault();
            if (unknown != null)
            {
                throw new BiteFormatException($"Unsupported member '{unknown.Name.LocalName}' on <{element.Name.LocalName}>", unknown);
            }
        }

        static bool IsSkipped(XElement element) =>
            IgnoredNamespaces.Contains(element.Name.NamespaceName) || IgnoredWfMembers.Contains(element.Name.LocalName) && element.Name.Namespace == Wf;

        static bool IsPropertyElement(XElement element) => element.Name.LocalName.Contains('.');

        static bool HasMember(XElement element, string name) =>
            element.Attribute(name) != null || element.Elements().Any(e => e.Name.LocalName.EndsWith("." + name, StringComparison.Ordinal));

        static XElement? SingleChild(XElement propertyElement)
        {
            var children = propertyElement.Elements().Where(e => !IsSkipped(e)).ToList();
            return children.Count switch
            {
                0 => null,
                1 => children[0],
                _ => throw new BiteFormatException($"'{propertyElement.Name.LocalName}' holds more than one value", propertyElement),
            };
        }

        // Stable per resource and element position, so a workflow saved without UniqueIDs keeps the same ids across
        // loads (suspend/resume finds its node by UniqueID). Today's path mints a random Guid per load instead.
        Guid StableId(XElement element)
        {
            var path = new StringBuilder();
            for (var e = element; e != null; e = e.Parent)
            {
                path.Insert(0, $"/{e.ElementsBeforeSelf().Count()}");
            }
            using (var sha = SHA1.Create())
            {
                var hash = sha.ComputeHash(Encoding.UTF8.GetBytes($"{_idSeed}|{path}"));
                var bytes = new byte[16];
                Array.Copy(hash, bytes, 16);
                bytes[7] = (byte)((bytes[7] & 0x0F) | 0x50); // RFC 4122 version 5 (name-based, SHA-1)
                bytes[8] = (byte)((bytes[8] & 0x3F) | 0x80); // RFC 4122 variant
                return new Guid(bytes);
            }
        }
    }
}
