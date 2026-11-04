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
using System.Activities;
using System.Activities.Expressions;
using System.Activities.Statements;
using System.Activities.XamlIntegration;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using Dev2.Activities;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.DynamicServices.Objects;
using Dev2.Utilities;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.WfCorpus
{
    /// <summary>
    /// Builds the WF-removal activity corpus (WF Removal spec, T0.4): one <c>.bite</c> per concrete
    /// activity class, written through the current WF writer (<see cref="WorkflowHelper.GetXamlDefinition"/>,
    /// the same path <c>X6ToWorkflowConverter</c> uses) while that writer still exists. The files are the
    /// reference inputs for the WF-free reader's parity harness (T1.1).
    ///
    /// <para>Every settable property gets a deterministic, non-default value so property-level parity has
    /// something to compare. Strings are unique per path, so a reader that reorders or drops list items is
    /// caught. Only data-shaped types are instantiated: building the service behind an interface-typed
    /// property (IResourceCatalog and friends) has side effects, so those are left unset.</para>
    /// </summary>
    public static class WfCorpusGenerator
    {
        const string DecisionExpression = "Dev2.Data.Decision.Dev2DataListDecisionHandler.Instance.ExecuteDecisionStack(\"{!TheStack!:[{!Col1!:![[a]]!,!Col2!:!1!,!Col3!:!!,!PopulatedColumnCount!:2,!EvaluationFn!:!IsEqual!}],!TotalDecisions!:1,!ModelName!:!Dev2DecisionStack!,!Mode!:!AND!,!TrueArmText!:!True!,!FalseArmText!:!False!}\",AmbientDataList)";
        const string SwitchExpression = "Dev2.Data.Decision.Dev2DataListDecisionHandler.Instance.FetchSwitchData(\"[[a]]\",AmbientDataList)";
        const int MaxDepth = 2;

        /// <summary>Every concrete activity class the corpus must cover, in a stable order.</summary>
        public static IReadOnlyList<Type> ActivityTypes() =>
            typeof(DsfCommentActivity).Assembly.GetTypes()
                .Where(t => t.IsPublic && t.IsClass && !t.IsAbstract && !t.ContainsGenericParameters
                            && typeof(Activity).IsAssignableFrom(t)
                            && typeof(IDev2Activity).IsAssignableFrom(t)
                            && t.GetConstructor(Type.EmptyTypes) != null)
                .OrderBy(t => t.FullName, StringComparer.Ordinal)
                .ToList();

        /// <summary>Writes one <c>{TypeName}.bite</c> per activity type into <paramref name="outputDirectory"/>.</summary>
        public static void Generate(string outputDirectory)
        {
            Directory.CreateDirectory(outputDirectory);
            foreach (var type in ActivityTypes())
            {
                File.WriteAllText(Path.Combine(outputDirectory, type.Name + ".bite"), Build(type));
            }
        }

        /// <summary>The full <c>.bite</c> resource for one activity type.</summary>
        public static string Build(Type activityType)
        {
            var activity = (Activity)Activator.CreateInstance(activityType);
            Populate(activity, activityType.Name, 0);
            var xaml = BuildXaml(activityType.Name, activity);
            if (xaml.Length == 0)
            {
                // GetXamlDefinition logs and swallows writer exceptions, returning empty.
                throw new InvalidOperationException($"WF writer produced no XAML for {activityType.Name}.");
            }
            return Envelope(activityType.Name, xaml.ToString());
        }

        /// <summary>
        /// Loads a <c>.bite</c> the way the Lightweight engine does (<c>WorkflowExecutor.ExtractWorkflowParts</c>
        /// + <c>LoadDynamicActivity</c>) and parses it with <see cref="ActivityParser"/>: the current, WF-based path.
        /// </summary>
        public static IDev2Activity LoadAndParse(string bite)
        {
            var service = XElement.Parse(bite);
            var action = service.Element("Actions")?.Element("Action") ?? service.Element("Action");
            var xamlDefinition = new StringBuilder(action?.Element("XamlDefinition")?.Value
                ?? throw new InvalidOperationException("No <XamlDefinition> in resource."));

            if (GlobalConstants.RuntimeNamespaceClean)
            {
                xamlDefinition = new Dev2XamlCleaner().CleanServiceDef(xamlDefinition);
            }
            Dev2XamlLoader.RemoveWindowsElements(ref xamlDefinition);

            using (var stream = xamlDefinition.EncodeForXmlDocument())
            {
                var activity = ActivityXamlServices.Load(stream) as DynamicActivity
                    ?? throw new InvalidOperationException("Resource did not load as a DynamicActivity.");
                return new ActivityParser().Parse(activity);
            }
        }

        static StringBuilder BuildXaml(string name, Activity activity)
        {
            var helper = new WorkflowHelper();
            var builder = helper.CreateWorkflow(name);
            var chart = (Flowchart)builder.Implementation;
            var after1 = new FlowStep { Action = Comment("After1") };
            var after2 = new FlowStep { Action = Comment("After2") };
            FlowNode start;
            switch (activity)
            {
                case DsfFlowDecisionActivity decision:
                    start = new FlowDecision { DisplayName = decision.DisplayName, Condition = decision, True = after1, False = after2 };
                    break;
                case DsfFlowSwitchActivity switchActivity:
                    var flowSwitch = new FlowSwitch<string> { DisplayName = switchActivity.DisplayName, Expression = switchActivity, Default = after2 };
                    flowSwitch.Cases["1"] = after1;
                    start = flowSwitch;
                    break;
                default:
                    start = new FlowStep { Action = activity, Next = after1 };
                    break;
            }
            chart.StartNode = start;
            chart.Nodes.Add(start);
            chart.Nodes.Add(after1);
            chart.Nodes.Add(after2);
            helper.EnsureImplementation(builder, chart);
            return helper.GetXamlDefinition(builder);
        }

        static DsfCommentActivity Comment(string text) =>
            new DsfCommentActivity { DisplayName = text, Text = text, UniqueID = DeterministicGuid("comment:" + text).ToString() };

        static string Envelope(string name, string xaml)
        {
            var id = DeterministicGuid("resource:" + name);
            return $"<Service ID=\"{id}\" Version=\"1.0\" ServerID=\"00000000-0000-0000-0000-000000000000\" Name=\"WfCorpus/{name}\" ResourceType=\"WorkflowService\" IsValid=\"true\" ServerVersion=\"0.0.0.0\">" +
                   $"<DisplayName>WfCorpus/{name}</DisplayName><Category>WfCorpus/{name}</Category><IsNewWorkflow>false</IsNewWorkflow><AuthorRoles></AuthorRoles>" +
                   "<Comment>Generated by WfCorpusGenerator (WF removal T0.4). Do not edit by hand.</Comment><Tags></Tags><HelpLink></HelpLink><UnitTestTargetWorkflowService></UnitTestTargetWorkflowService>" +
                   "<DataList><a Description=\"\" IsEditable=\"True\" ColumnIODirection=\"Input\" /></DataList>" +
                   $"<Action Name=\"InvokeWorkflow\" Type=\"Workflow\"><XamlDefinition>{WebUtility.HtmlEncode(xaml)}</XamlDefinition></Action>" +
                   $"<ErrorMessages /><VersionInfo DateTimeStamp=\"2026-10-22T00:00:00Z\" Reason=\"Save\" User=\"WfCorpusGenerator\" VersionNumber=\"1\" ResourceId=\"{id}\" VersionId=\"{id}\" /></Service>";
        }

        static Guid DeterministicGuid(string value)
        {
            using (var md5 = MD5.Create())
            {
                return new Guid(md5.ComputeHash(Encoding.UTF8.GetBytes(value)));
            }
        }

        static void Populate(object target, string path, int depth)
        {
            foreach (var property in target.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (property.GetIndexParameters().Length > 0
                    || property.DeclaringType == typeof(Activity)
                    || property.DeclaringType?.Namespace?.StartsWith("System.Activities") == true)
                {
                    continue;
                }
                var propertyType = property.PropertyType;
                var isInArgument = IsGeneric(propertyType, typeof(InArgument<>));
                var isActivityFunc = IsGeneric(propertyType, typeof(ActivityFunc<,>));
                if (propertyType.Namespace?.StartsWith("System.Activities") == true && !isInArgument && !isActivityFunc)
                {
                    continue; // OutArgument/InOutArgument/Variable plumbing: the writer supplies that XAML shape
                }
                try
                {
                    SetProperty(target, property, path, depth, isInArgument, isActivityFunc);
                }
                catch (Exception)
                {
                    // A setter that rejects a generated value leaves the property at its default; the
                    // corpus is still valid, it just exercises that member less.
                }
            }
        }

        static void SetProperty(object target, PropertyInfo property, string path, int depth, bool isInArgument, bool isActivityFunc)
        {
            var propertyType = property.PropertyType;
            if (isActivityFunc && property.CanWrite)
            {
                // ForEach / SelectAndApply bodies: ActivityFunc + DelegateInArgument, as X6ToWorkflowConverter builds them.
                var func = Activator.CreateInstance(propertyType);
                var argumentType = typeof(DelegateInArgument<>).MakeGenericType(propertyType.GetGenericArguments()[0]);
                propertyType.GetProperty("Argument").SetValue(func, Activator.CreateInstance(argumentType, "explicitData_20261022000000"));
                propertyType.GetProperty("Handler").SetValue(func, Comment("Body"));
                property.SetValue(target, func);
                return;
            }
            if (isInArgument && property.CanWrite)
            {
                // InArgument<T> + Literal<T>: the fixed-argument shape used throughout real workflows.
                var argumentType = propertyType.GetGenericArguments()[0];
                var value = IsLeaf(argumentType) ? MakeValue(argumentType, $"{path}.{property.Name}", depth) : null;
                if (value != null)
                {
                    var literal = Activator.CreateInstance(typeof(Literal<>).MakeGenericType(argumentType), value);
                    property.SetValue(target, Activator.CreateInstance(propertyType, literal));
                }
                return;
            }
            if (propertyType.IsGenericType && property.GetValue(target) is IList children
                && typeof(Activity).IsAssignableFrom(propertyType.GetGenericArguments()[0]))
            {
                children.Add(Comment("Child")); // Sequence-style containers
                return;
            }
            if (!property.CanWrite || property.SetMethod?.IsPublic != true)
            {
                return;
            }
            object generated;
            switch (property.Name)
            {
                case "UniqueID":
                    generated = DeterministicGuid(path).ToString();
                    break;
                case "DisplayName":
                    generated = path;
                    break;
                case "ExpressionText":
                    generated = target is DsfFlowSwitchActivity ? SwitchExpression : DecisionExpression;
                    break;
                default:
                    generated = MakeValue(propertyType, $"{path}.{property.Name}", depth);
                    break;
            }
            if (generated != null)
            {
                property.SetValue(target, generated);
            }
        }

        static object MakeValue(Type type, string path, int depth)
        {
            var underlying = Nullable.GetUnderlyingType(type) ?? type;
            if (underlying == typeof(string))
            {
                return $"[[{string.Join("_", path.Split('.').Skip(1))}]]";
            }
            if (underlying == typeof(bool))
            {
                return true;
            }
            if (underlying == typeof(int) || underlying == typeof(long) || underlying == typeof(short))
            {
                return Convert.ChangeType(7, underlying);
            }
            if (underlying == typeof(double) || underlying == typeof(decimal) || underlying == typeof(float))
            {
                return Convert.ChangeType(7.5, underlying);
            }
            if (underlying == typeof(Guid))
            {
                return DeterministicGuid(path);
            }
            if (underlying.IsEnum)
            {
                var values = Enum.GetValues(underlying);
                return values.Length > 1 ? values.GetValue(values.Length - 1) : null;
            }
            if (depth >= MaxDepth)
            {
                return null;
            }
            if (IsGeneric(underlying, typeof(List<>)) || IsGeneric(underlying, typeof(IList<>))
                || IsGeneric(underlying, typeof(ICollection<>)) || IsGeneric(underlying, typeof(IEnumerable<>)))
            {
                return MakeList(underlying.GetGenericArguments()[0], path, depth);
            }
            if (underlying.IsInterface || underlying.IsAbstract || !IsDataShaped(underlying))
            {
                return null;
            }
            var instance = Activator.CreateInstance(underlying);
            Populate(instance, path, depth + 1);
            return instance;
        }

        static IList MakeList(Type itemType, string path, int depth)
        {
            var concrete = itemType.IsInterface || itemType.IsAbstract ? ConcreteListItemType(itemType) : itemType;
            if (concrete == null)
            {
                return null;
            }
            var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(itemType));
            for (var i = 1; i <= 2; i++)
            {
                object item;
                if (IsLeaf(concrete))
                {
                    item = MakeValue(concrete, $"{path}{i}", depth + 1);
                }
                else
                {
                    item = Activator.CreateInstance(concrete);
                    Populate(item, $"{path}{i}", depth + 1);
                }
                if (item == null)
                {
                    return null;
                }
                list.Add(item);
            }
            return list;
        }

        static bool IsGeneric(Type type, Type definition) => type.IsGenericType && type.GetGenericTypeDefinition() == definition;

        static bool IsLeaf(Type type)
        {
            var underlying = Nullable.GetUnderlyingType(type) ?? type;
            return underlying.IsPrimitive || underlying.IsEnum || underlying == typeof(string) || underlying == typeof(Guid)
                   || underlying == typeof(decimal) || underlying == typeof(DateTime);
        }

        static bool IsWarewolfAssembly(Assembly assembly)
        {
            var name = assembly.GetName().Name ?? "";
            return name.StartsWith("Dev2", StringComparison.Ordinal) || name.StartsWith("Warewolf", StringComparison.Ordinal);
        }

        // A DTO: Warewolf-owned, public parameterless constructor, not an activity, and every public
        // settable property is a leaf or a generic collection.
        static bool IsDataShaped(Type type) =>
            type.IsClass && !type.IsAbstract
            && type.GetConstructor(Type.EmptyTypes) != null
            && IsWarewolfAssembly(type.Assembly)
            && !typeof(Activity).IsAssignableFrom(type)
            && type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanWrite && p.SetMethod?.IsPublic == true && p.GetIndexParameters().Length == 0)
                .All(p => IsLeaf(p.PropertyType) || (p.PropertyType.IsGenericType && typeof(IEnumerable).IsAssignableFrom(p.PropertyType)));

        // Collection items only: prefer the conventional implementation (IServiceInput -> ServiceInput), else any DTO.
        // Never used for scalar properties, where the same rule would pick IResourceCatalog -> ResourceCatalog.
        static Type ConcreteListItemType(Type contract)
        {
            var candidates = WarewolfAssemblies()
                .SelectMany(LoadableTypes)
                .Where(t => t.IsClass && !t.IsAbstract && contract.IsAssignableFrom(t) && t.GetConstructor(Type.EmptyTypes) != null)
                .OrderBy(t => t.FullName, StringComparer.Ordinal)
                .ToList();
            return candidates.FirstOrDefault(t => contract.Name.Length > 1 && t.Name == contract.Name.Substring(1))
                   ?? candidates.FirstOrDefault(IsDataShaped);
        }

        // Dev2.Activities plus its transitive Warewolf references. Walked explicitly rather than read from the
        // AppDomain, whose contents depend on the host, so the generated corpus is the same wherever it runs.
        static IEnumerable<Assembly> WarewolfAssemblies()
        {
            var seen = new Dictionary<string, Assembly>(StringComparer.Ordinal);
            var pending = new Queue<Assembly>(new[] { typeof(DsfCommentActivity).Assembly });
            while (pending.Count > 0)
            {
                var assembly = pending.Dequeue();
                if (seen.ContainsKey(assembly.GetName().Name))
                {
                    continue;
                }
                seen[assembly.GetName().Name] = assembly;
                foreach (var reference in assembly.GetReferencedAssemblies().Where(r => !seen.ContainsKey(r.Name)))
                {
                    var name = reference.Name ?? "";
                    if (!name.StartsWith("Dev2", StringComparison.Ordinal) && !name.StartsWith("Warewolf", StringComparison.Ordinal))
                    {
                        continue;
                    }
                    try
                    {
                        pending.Enqueue(Assembly.Load(reference));
                    }
                    catch (FileNotFoundException)
                    {
                        // Not shipped alongside this host; its types cannot be candidates anyway.
                    }
                }
            }
            return seen.OrderBy(kv => kv.Key, StringComparer.Ordinal).Select(kv => kv.Value);
        }

        static IEnumerable<Type> LoadableTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null);
            }
        }
    }
}
