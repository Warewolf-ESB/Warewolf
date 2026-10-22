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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Warewolf.Bite.Xaml
{
    /// <summary>
    /// Maps XAML type references to CLR types through an allowlist. A <c>.bite</c> can only instantiate Warewolf types,
    /// a fixed set of BCL value and collection types, and the handful of WF argument/delegate types the format uses.
    /// Anything else fails loudly: today's <c>ActivityXamlServices.Load</c> instantiates whatever type a file names.
    ///
    /// <para>Types are matched by namespace and name, never by the xmlns assembly: WPF-era files say
    /// <c>assembly=mscorlib</c>, and older ones use since-renamed Warewolf assemblies (see <c>Dev2XamlCleaner</c>).
    /// WF types are found by name in their assembly so this project has no compile-time WF reference.</para>
    /// </summary>
    public sealed class XamlTypeResolver
    {
        public const string WfNamespace = "http://schemas.microsoft.com/netfx/2009/xaml/activities";
        public const string XamlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";

        static readonly Dictionary<string, Type> XamlIntrinsics = new Dictionary<string, Type>(StringComparer.Ordinal)
        {
            ["String"] = typeof(string), ["Boolean"] = typeof(bool), ["Char"] = typeof(char), ["Byte"] = typeof(byte),
            ["Int16"] = typeof(short), ["Int32"] = typeof(int), ["Int64"] = typeof(long), ["Single"] = typeof(float),
            ["Double"] = typeof(double), ["Decimal"] = typeof(decimal), ["Object"] = typeof(object), ["TimeSpan"] = typeof(TimeSpan),
            ["Uri"] = typeof(Uri),
        };

        // BCL types a .bite may name through clr-namespace, by full (generic) name.
        static readonly Dictionary<string, Type> AllowedSystemTypes = new[]
        {
            typeof(string), typeof(bool), typeof(char), typeof(byte), typeof(short), typeof(int), typeof(long), typeof(float),
            typeof(double), typeof(decimal), typeof(object), typeof(Guid), typeof(DateTime), typeof(TimeSpan), typeof(Uri),
            typeof(Nullable<>), typeof(List<>), typeof(Dictionary<,>), typeof(KeyValuePair<,>), typeof(Collection<>), typeof(ObservableCollection<>),
        }.ToDictionary(t => t.FullName!, StringComparer.Ordinal);

        // WF types the format needs, by name within the WF assembly (names include generic arity).
        static readonly HashSet<string> AllowedWfTypes = new HashSet<string>(StringComparer.Ordinal)
        {
            // Only ever a type argument (Collection<Activity> of Sequence children); abstract, so never instantiated.
            "System.Activities.Activity",
            "System.Activities.InArgument`1", "System.Activities.OutArgument`1", "System.Activities.InOutArgument`1",
            "System.Activities.Expressions.Literal`1", "System.Activities.DelegateInArgument`1", "System.Activities.DelegateOutArgument`1",
            "System.Activities.ActivityFunc`1", "System.Activities.ActivityFunc`2", "System.Activities.ActivityFunc`3",
            "System.Activities.ActivityAction", "System.Activities.ActivityAction`1", "System.Activities.ActivityAction`2",
            "Microsoft.VisualBasic.Activities.VisualBasicValue`1", "Microsoft.VisualBasic.Activities.VisualBasicReference`1",
        };

        static readonly Lazy<ConcurrentDictionary<string, Type>> WarewolfIndex = new Lazy<ConcurrentDictionary<string, Type>>(BuildWarewolfIndex);
        static readonly Lazy<Assembly[]> WfAssemblies = new Lazy<Assembly[]>(FindWfAssemblies);
        readonly ConcurrentDictionary<string, Type?> _cache = new ConcurrentDictionary<string, Type?>(StringComparer.Ordinal);

        /// <summary>Resolves an element name (its namespace URI and local name) plus optional generic arguments.</summary>
        public Type Resolve(XName name, IReadOnlyList<Type> typeArguments, XObject at)
        {
            var arity = typeArguments.Count;
            var key = $"{name.NamespaceName}|{name.LocalName}`{arity}";
            var open = _cache.GetOrAdd(key, _ => Find(name.NamespaceName, name.LocalName, arity))
                       ?? throw new BiteFormatException($"Type '{name.LocalName}' in '{name.NamespaceName}' is not allowed in a .bite resource", at);
            if (arity == 0)
            {
                return open;
            }
            try
            {
                return open.MakeGenericType(typeArguments.ToArray());
            }
            catch (ArgumentException e)
            {
                throw new BiteFormatException($"Type arguments do not fit '{open.Name}'", at, e);
            }
        }

        /// <summary>Resolves an <c>x:TypeArguments</c>-style type name, e.g. <c>scg:List(x:String)</c>.</summary>
        public Type Resolve(XamlTypeName typeName, XElement scope)
        {
            var ns = typeName.Prefix == null
                ? scope.GetDefaultNamespace()
                : scope.GetNamespaceOfPrefix(typeName.Prefix) ?? throw new BiteFormatException($"Unknown xmlns prefix '{typeName.Prefix}'", scope);
            var arguments = typeName.Arguments.Select(a => Resolve(a, scope)).ToList();
            return Resolve(ns + typeName.Name, arguments, scope);
        }

        static Type? Find(string namespaceUri, string localName, int arity)
        {
            var genericName = arity == 0 ? localName : $"{localName}`{arity}";
            if (namespaceUri == XamlNamespace)
            {
                return arity == 0 && XamlIntrinsics.TryGetValue(localName, out var intrinsic) ? intrinsic : null;
            }
            if (namespaceUri == WfNamespace)
            {
                return FindWf("System.Activities." + genericName) ?? FindWf("System.Activities.Expressions." + genericName);
            }
            if (!namespaceUri.StartsWith("clr-namespace:", StringComparison.Ordinal))
            {
                return null;
            }

            var clr = namespaceUri.Substring("clr-namespace:".Length);
            var semicolon = clr.IndexOf(';');
            var clrNamespace = semicolon < 0 ? clr : clr.Substring(0, semicolon);
            var assemblyName = semicolon < 0 ? null : clr.Substring(semicolon + 1).Replace("assembly=", "").Trim();
            var fullName = $"{clrNamespace}.{genericName}";

            if (AllowedSystemTypes.TryGetValue(fullName, out var system))
            {
                return system;
            }
            if (FindWf(fullName) is { } wf)
            {
                return wf;
            }
            if (WarewolfIndex.Value.TryGetValue(fullName, out var warewolf))
            {
                return warewolf;
            }
            // A Warewolf assembly not yet loaded (e.g. Dev2.Runtime.Services for NamespaceItem): load it by name, never by path.
            if (assemblyName != null && IsWarewolfName(assemblyName) && TryLoad(assemblyName) is { } loaded)
            {
                Index(loaded, WarewolfIndex.Value);
                return WarewolfIndex.Value.TryGetValue(fullName, out warewolf) ? warewolf : null;
            }
            return null;
        }

        static Type? FindWf(string fullName) =>
            AllowedWfTypes.Contains(fullName) ? WfAssemblies.Value.Select(a => a.GetType(fullName, false)).FirstOrDefault(t => t != null) : null;

        static Assembly[] FindWfAssemblies()
        {
            // Activity's own assembly (System.Activities from the CoreWF package), located through an activity type.
            var activityBase = typeof(DsfCommentActivity).BaseType;
            while (activityBase != null && activityBase.FullName != "System.Activities.Activity")
            {
                activityBase = activityBase.BaseType;
            }
            return activityBase == null ? Array.Empty<Assembly>() : new[] { activityBase.Assembly };
        }

        static ConcurrentDictionary<string, Type> BuildWarewolfIndex()
        {
            var index = new ConcurrentDictionary<string, Type>(StringComparer.Ordinal);
            var pending = new Queue<Assembly>(AppDomain.CurrentDomain.GetAssemblies().Where(a => IsWarewolfName(a.GetName().Name)).Append(typeof(DsfCommentActivity).Assembly));
            var seen = new HashSet<string>(StringComparer.Ordinal);
            while (pending.Count > 0)
            {
                var assembly = pending.Dequeue();
                if (!seen.Add(assembly.GetName().Name ?? ""))
                {
                    continue;
                }
                Index(assembly, index);
                foreach (var reference in assembly.GetReferencedAssemblies().Where(r => IsWarewolfName(r.Name) && !seen.Contains(r.Name!)))
                {
                    if (TryLoad(reference.Name!) is { } next)
                    {
                        pending.Enqueue(next);
                    }
                }
            }
            return index;
        }

        static void Index(Assembly assembly, ConcurrentDictionary<string, Type> index)
        {
            Type?[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                types = e.Types;
            }
            foreach (var type in types)
            {
                if (type is { IsPublic: true, FullName: { } fullName })
                {
                    index.TryAdd(fullName, type);
                }
            }
        }

        static Assembly? TryLoad(string name)
        {
            try
            {
                return Assembly.Load(new AssemblyName(name));
            }
            catch (FileNotFoundException)
            {
                return null;
            }
            catch (FileLoadException)
            {
                return null;
            }
        }

        static bool IsWarewolfName(string? name) =>
            name != null && (name.StartsWith("Dev2", StringComparison.Ordinal) || name.StartsWith("Warewolf", StringComparison.Ordinal));
    }
}
