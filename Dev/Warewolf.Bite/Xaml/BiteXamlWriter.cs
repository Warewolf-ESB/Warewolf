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
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Dev2;
using Dev2.Activities.Flow;
using Warewolf.Bite.Json;

namespace Warewolf.Bite.Xaml
{
    /// <summary>
    /// Writes a workflow definition (<see cref="FlowchartModel"/>) as WF-compatible XAML without WF's
    /// <c>ActivityBuilder</c>/<c>XamlServices</c> (WF removal T3.2), so engines that still load through WF read it
    /// unchanged. Emits the boilerplate WF needs to load (the VB namespace imports and the Flowchart variables the
    /// <c>[HasError]</c>-style references resolve against), puts every node at Flowchart level with an <c>x:Name</c>,
    /// and links them with <c>x:Reference</c>, which covers shared successors and back-edges with one rule.
    /// </summary>
    public sealed class BiteXamlWriter
    {
        static readonly XNamespace Wf = XamlTypeResolver.WfNamespace;
        static readonly XNamespace X = XamlTypeResolver.XamlNamespace;

        static readonly Dictionary<Type, string> Intrinsics = new Dictionary<Type, string>
        {
            [typeof(string)] = "String", [typeof(bool)] = "Boolean", [typeof(char)] = "Char", [typeof(byte)] = "Byte",
            [typeof(short)] = "Int16", [typeof(int)] = "Int32", [typeof(long)] = "Int64", [typeof(float)] = "Single",
            [typeof(double)] = "Double", [typeof(decimal)] = "Decimal", [typeof(object)] = "Object", [typeof(TimeSpan)] = "TimeSpan",
            [typeof(Uri)] = "Uri",
        };

        // clr-namespace URI -> prefix, in first-use order so output is deterministic.
        readonly Dictionary<string, string> _prefixes = new Dictionary<string, string>(StringComparer.Ordinal);

        // The workflow-level arguments WPF Studio declared on every root (x:Members). They are the only ones any .bite
        // declares (211 of 903 corpus files, always these three), and an activity expression such as
        // [AmbientDataList] only compiles on the WF path when they are declared.
        static readonly (string Name, Type ArgumentType)[] StandardMembers =
        {
            ("AmbientDataList", typeof(List<string>)),
            ("ParentWorkflowInstanceId", typeof(Guid)),
            ("ParentServiceName", typeof(string)),
        };

        bool _referencesStandardMember;

        public static string Write(FlowchartModel chart) => new BiteXamlWriter().WriteDocument(chart);

        string WriteDocument(FlowchartModel chart)
        {
            var name = chart.Name ?? "Workflow";
            var ids = AssignIds(chart);
            var all = ids.Keys.ToList();

            var flowchart = new XElement(Wf + "Flowchart", new XAttribute("DisplayName", name), Variables());
            if (chart.StartNode != null)
            {
                flowchart.Add(new XElement(Wf + "Flowchart.StartNode", Reference(ids[chart.StartNode])));
            }
            foreach (var node in all)
            {
                flowchart.Add(Node(node, ids));
            }

            var root = new XElement(Wf + "Activity",
                new XAttribute(X + "Class", name),
                _referencesStandardMember ? StandardMembersDeclaration() : null,
                TextExpressionImports(),
                flowchart);
            root.Add(new XAttribute(XNamespace.Xmlns + "x", X.NamespaceName));
            foreach (var (uri, prefix) in _prefixes)
            {
                root.Add(new XAttribute(XNamespace.Xmlns + prefix, uri));
            }
            root.Add(new XAttribute("xmlns", Wf.NamespaceName));
            return root.ToString(SaveOptions.DisableFormatting);
        }

        static Dictionary<FlowNodeModel, string> AssignIds(FlowchartModel chart)
        {
            var ids = new Dictionary<FlowNodeModel, string>(ReferenceEqualityComparer.Instance);
            var nodes = chart.Nodes.ToList();
            if (chart.StartNode != null && !nodes.Contains(chart.StartNode))
            {
                nodes.Insert(0, chart.StartNode);
            }
            // A successor missing from Nodes is still part of the flowchart (WF's own writer serialises it too).
            for (var i = 0; i < nodes.Count; i++)
            {
                foreach (var successor in Successors(nodes[i]).Where(s => s != null && !nodes.Contains(s)))
                {
                    nodes.Add(successor!);
                }
            }
            var used = new HashSet<string>(StringComparer.Ordinal);
            foreach (var node in nodes)
            {
                var id = IsXamlName(node.Name) ? node.Name! : $"__ReferenceID{ids.Count}";
                while (!used.Add(id))
                {
                    id += "_";
                }
                ids[node] = id;
            }
            return ids;
        }

        static IEnumerable<FlowNodeModel?> Successors(FlowNodeModel node) => node switch
        {
            FlowStepModel step => new[] { step.Next },
            FlowDecisionModel decision => new[] { decision.True, decision.False },
            FlowSwitchModel flowSwitch => flowSwitch.Cases.Select(c => c.Value).Append(flowSwitch.Default),
            _ => Enumerable.Empty<FlowNodeModel?>(),
        };

        static bool IsXamlName(string? name) =>
            !string.IsNullOrEmpty(name) && (char.IsLetter(name[0]) || name[0] == '_') && name.All(c => char.IsLetterOrDigit(c) || c == '_');

        static XElement Reference(string id) => new XElement(X + "Reference", id);

        XElement Node(FlowNodeModel node, Dictionary<FlowNodeModel, string> ids)
        {
            void Link(XElement owner, string member, FlowNodeModel? target)
            {
                if (target != null)
                {
                    owner.Add(new XElement(Wf + member, Reference(ids[target])));
                }
            }

            switch (node)
            {
                case FlowStepModel step:
                {
                    var element = new XElement(Wf + "FlowStep", new XAttribute(X + "Name", ids[node]));
                    if (step.Action != null)
                    {
                        element.Add(Value(step.Action, "activity"));
                    }
                    Link(element, "FlowStep.Next", step.Next);
                    return element;
                }
                case FlowDecisionModel decision:
                {
                    var element = new XElement(Wf + "FlowDecision", new XAttribute(X + "Name", ids[node]), new XAttribute("DisplayName", decision.DisplayName ?? "Decision"));
                    if (decision.Condition != null)
                    {
                        element.Add(new XElement(Wf + "FlowDecision.Condition", Value(decision.Condition, "condition")));
                    }
                    Link(element, "FlowDecision.True", decision.True);
                    Link(element, "FlowDecision.False", decision.False);
                    return element;
                }
                case FlowSwitchModel flowSwitch:
                {
                    var element = new XElement(Wf + "FlowSwitch",
                        new XAttribute(X + "TypeArguments", "x:String"),
                        new XAttribute(X + "Name", ids[node]),
                        new XAttribute("DisplayName", flowSwitch.DisplayName ?? "Switch"));
                    if (flowSwitch.Expression != null)
                    {
                        element.Add(new XElement(Wf + "FlowSwitch.Expression", Value(flowSwitch.Expression, "expression")));
                    }
                    Link(element, "FlowSwitch.Default", flowSwitch.Default);
                    foreach (var c in flowSwitch.Cases.Where(c => c.Value != null))
                    {
                        // A keyed reference: <x:Reference>id<x:Key>k</x:Key></x:Reference>, as real .bite files carry it.
                        element.Add(new XElement(X + "Reference", ids[c.Value!], new XElement(X + "Key", c.Key)));
                    }
                    return element;
                }
                default:
                    throw new NotSupportedException($"Unknown flow node {node.GetType().Name}");
            }
        }

        // ------------------------------------------------------------------ boilerplate WF needs to load

        XElement StandardMembersDeclaration() =>
            new XElement(X + "Members", StandardMembers.Select(m =>
                new XElement(X + "Property",
                    new XAttribute("Name", m.Name),
                    new XAttribute("Type", $"InOutArgument({TypeName(m.ArgumentType)})"))));

        IEnumerable<XElement> TextExpressionImports()
        {
            var namespaces = new[]
            {
                "Dev2.Common", "Dev2.Data.Decisions.Operations", "Dev2.Data.SystemTemplates.Models", "Dev2.DataList.Contract",
                "Dev2.DataList.Contract.Binary_Objects", "Unlimited.Applications.BusinessDesignStudio.Activities",
            };
            yield return new XElement(Wf + "TextExpression.NamespacesForImplementation",
                new XElement(ElementName(typeof(List<>)), new XAttribute(X + "TypeArguments", "x:String"),
                    namespaces.Select(n => new XElement(X + "String", n))));
            yield return new XElement(Wf + "TextExpression.ReferencesForImplementation",
                new XElement(ElementName(typeof(System.Collections.ObjectModel.Collection<>)), new XAttribute(X + "TypeArguments", "AssemblyReference"),
                    new[] { "Dev2.Common", "Dev2.Data", "Dev2.Activities" }.Select(a => new XElement(Wf + "AssemblyReference", a))));
        }

        // The variables WorkflowHelper.SetVariables declares; WF compiles each activity's [HasError]/[IsValid]/
        // [InstructionList] references against them, and fails to load the workflow without them.
        XElement Variables()
        {
            XElement Variable(string typeName, string name) =>
                new XElement(Wf + "Variable", new XAttribute(X + "TypeArguments", typeName), new XAttribute("Name", name));
            var util = TypeName(typeof(Unlimited.Applications.BusinessDesignStudio.Activities.Util));
            var handler = TypeName(typeof(Dev2.Data.Decision.Dev2DataListDecisionHandler));
            return new XElement(Wf + "Flowchart.Variables",
                Variable(TypeName(typeof(List<string>)), "InstructionList"),
                Variable("x:String", "LastResult"),
                Variable("x:Boolean", "HasError"),
                Variable("x:String", "ExplicitDataList"),
                Variable("x:Boolean", "IsValid"),
                Variable(util, "t"),
                Variable(handler, "Dev2DecisionHandler"));
        }

        // ------------------------------------------------------------------ objects and members

        XElement Value(object value, string path)
        {
            var type = value.GetType();
            if (!DefinitionMembers.IsWarewolfType(type))
            {
                throw new NotSupportedException($"Cannot write a {type.FullName} object at '{path}' as XAML.");
            }
            var element = new XElement(ElementName(type));
            foreach (var member in DefinitionMembers.Of(type))
            {
                WriteMember(element, type, member.Name, member.GetValue(value), member.SetMethod?.IsPublic == true, $"{path}.{member.Name}");
            }
            return element;
        }

        void WriteMember(XElement owner, Type ownerType, string member, object? value, bool settable, string path)
        {
            var propertyName = XName.Get($"{ElementName(ownerType).LocalName}.{member}", ElementName(ownerType).NamespaceName);

            if (value == null)
            {
                if (settable)
                {
                    owner.Add(new XAttribute(member, "{x:Null}"));
                }
                return;
            }
            var type = value.GetType();
            if (WfArguments.IsArgumentType(type))
            {
                var (hasExpression, isLiteral, held) = WfArguments.Describe(value);
                if (!hasExpression)
                {
                    owner.Add(new XElement(propertyName, new XElement(Wf + ArgumentElementName(type), new XAttribute(X + "TypeArguments", TypeName(type.GetGenericArguments()[0])))));
                }
                else if (!isLiteral)
                {
                    _referencesStandardMember |= StandardMembers.Any(m => Regex.IsMatch((string?)held ?? "", $@"\b{m.Name}\b"));
                    owner.Add(new XAttribute(member, $"[{held}]"));
                }
                else
                {
                    owner.Add(new XAttribute(member, EscapeArgumentLiteral(Text(held, path))));
                }
                return;
            }
            if (WfArguments.IsDelegateType(type))
            {
                var (argumentName, handler) = WfArguments.DescribeDelegate(value);
                var wrapper = new XElement(Wf + DelegateElementName(type), new XAttribute(X + "TypeArguments", string.Join(", ", type.GetGenericArguments().Select(TypeName))));
                if (argumentName != null)
                {
                    var argumentType = type.GetGenericArguments()[0];
                    wrapper.Add(new XElement(Wf + $"{DelegateElementName(type)}.Argument",
                        new XElement(Wf + "DelegateInArgument", new XAttribute(X + "TypeArguments", TypeName(argumentType)), new XAttribute("Name", argumentName))));
                }
                if (handler != null)
                {
                    wrapper.Add(Value(handler, path + ".body"));
                }
                owner.Add(new XElement(propertyName, wrapper));
                return;
            }
            if (value is Type typeValue)
            {
                owner.Add(new XAttribute(member, TypeName(typeValue))); // e.g. DbColumn.DataType="x:String"
                return;
            }
            if (IsLeaf(type))
            {
                owner.Add(new XAttribute(member, EscapeBraces(Text(value, path))));
                return;
            }
            if (value is IEnumerable sequence && !(value is string))
            {
                var items = Items(sequence, path);
                // A get-only collection receives its items directly; a settable one gets its own collection element.
                owner.Add(settable
                    ? new XElement(propertyName, Collection(value, items))
                    : new XElement(propertyName, items));
                return;
            }
            owner.Add(new XElement(propertyName, Value(value, path)));
        }

        XElement Collection(object collection, IEnumerable<XElement> items)
        {
            var type = collection.GetType();
            var definition = type.IsGenericType ? type.GetGenericTypeDefinition() : type;
            return new XElement(ElementName(definition),
                type.IsGenericType ? new XAttribute(X + "TypeArguments", string.Join(", ", type.GetGenericArguments().Select(TypeName))) : null,
                items);
        }

        List<XElement> Items(IEnumerable sequence, string path)
        {
            var items = new List<XElement>();
            var index = 0;
            if (sequence is IDictionary dictionary)
            {
                foreach (var key in dictionary.Keys.Cast<object>().OrderBy(k => Text(k, path), StringComparer.Ordinal))
                {
                    var item = Item(dictionary[key], $"{path}[{key}]");
                    item.Add(new XAttribute(X + "Key", Text(key, path)));
                    items.Add(item);
                }
                return items;
            }
            foreach (var value in sequence)
            {
                items.Add(Item(value, $"{path}[{index++}]"));
            }
            return items;
        }

        XElement Item(object? value, string path)
        {
            if (value == null)
            {
                return new XElement(X + "Null");
            }
            var type = value.GetType();
            if (Intrinsics.TryGetValue(type, out var intrinsic) && type != typeof(object))
            {
                return new XElement(X + intrinsic, Text(value, path));
            }
            if (IsLeaf(type))
            {
                throw new NotSupportedException($"Cannot write a {type.Name} collection item at '{path}' as XAML.");
            }
            return Value(value, path);
        }

        // ------------------------------------------------------------------ text

        static string Text(object? value, string path) => value switch
        {
            null => "",
            string s => s,
            bool b => b ? "True" : "False",
            Enum e => e.ToString(),
            Guid g => g.ToString("D"),
            DateTime dt => dt.ToString("O", CultureInfo.InvariantCulture),
            TimeSpan ts => ts.ToString("c", CultureInfo.InvariantCulture),
            Uri uri => uri.OriginalString,
            char c => c.ToString(),
            IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
            _ => throw new NotSupportedException($"Cannot write a {value.GetType().Name} value at '{path}' as XAML text."),
        };

        // A literal that would read back as a VB expression gets WF's leading '%'; see WfArguments.IsEscapedLiteral.
        static string EscapeArgumentLiteral(string text)
        {
            var escaped = WfArguments.EscapeLiteral(text);
            return escaped.Length != text.Length ? escaped : EscapeBraces(text);
        }

        // XAML reads an attribute that starts with '{' as markup; "{}" makes it literal.
        static string EscapeBraces(string text) => text.StartsWith("{", StringComparison.Ordinal) ? "{}" + text : text;

        static bool IsLeaf(Type type)
        {
            var underlying = Nullable.GetUnderlyingType(type) ?? type;
            return underlying.IsPrimitive || underlying.IsEnum || underlying == typeof(string) || underlying == typeof(Guid)
                   || underlying == typeof(decimal) || underlying == typeof(DateTime) || underlying == typeof(TimeSpan) || underlying == typeof(Uri);
        }

        static string ArgumentElementName(Type argumentType) =>
            argumentType.GetGenericTypeDefinition().Name.Split('`')[0]; // InArgument / OutArgument / InOutArgument

        static string DelegateElementName(Type delegateType) => delegateType.Name.Split('`')[0]; // ActivityFunc / ActivityAction

        // ------------------------------------------------------------------ names and prefixes

        XName ElementName(Type type)
        {
            if (type.Namespace?.StartsWith("System.Activities", StringComparison.Ordinal) == true && type.Assembly == typeof(Unlimited.Applications.BusinessDesignStudio.Activities.DsfCommentActivity).BaseType?.Assembly)
            {
                return Wf + type.Name.Split('`')[0];
            }
            Prefix(type); // declare the namespace on the root with a readable prefix
            return XName.Get(type.Name.Split('`')[0], ClrNamespace(type));
        }

        /// <summary>A XAML type name for <c>x:TypeArguments</c>, e.g. <c>scg:List(uaba:ActivityDTO)</c>.</summary>
        string TypeName(Type type)
        {
            if (Intrinsics.TryGetValue(type, out var intrinsic))
            {
                return "x:" + intrinsic;
            }
            if (type.Namespace == "System.Activities")
            {
                return type.Name.Split('`')[0]; // default namespace
            }
            var name = $"{Prefix(type)}:{type.Name.Split('`')[0]}";
            return type.IsGenericType && !type.IsGenericTypeDefinition
                ? $"{name}({string.Join(", ", type.GetGenericArguments().Select(TypeName))})"
                : name;
        }

        string Prefix(Type type) => _prefixes.TryGetValue(ClrNamespace(type), out var existing) ? existing : Register(ClrNamespace(type), type.Namespace ?? "");

        // Core types are written against mscorlib, as every existing .bite has them: .NET resolves it by type forwarding,
        // and older Studios only know that name.
        static string ClrNamespace(Type type)
        {
            var assembly = type.Assembly == typeof(object).Assembly ? "mscorlib" : type.Assembly.GetName().Name;
            return $"clr-namespace:{type.Namespace};assembly={assembly}";
        }

        string Register(string uri, string clrNamespace)
        {
            // Initials of the namespace segments, as WF's own writer abbreviates (uaba, scg, ddd), made unique.
            var initials = new string(clrNamespace.Split('.').Where(s => s.Length > 0).Select(s => char.ToLowerInvariant(s[0])).ToArray());
            var prefix = initials.Length == 0 ? "ns" : initials;
            if (prefix == "x")
            {
                prefix = "x0";
            }
            var candidate = prefix;
            for (var n = 1; _prefixes.ContainsValue(candidate); n++)
            {
                candidate = prefix + n.ToString(CultureInfo.InvariantCulture);
            }
            _prefixes[uri] = candidate;
            return candidate;
        }
    }
}
