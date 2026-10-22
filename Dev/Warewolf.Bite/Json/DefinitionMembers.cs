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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Warewolf.Bite.Json
{
    /// <summary>
    /// The members of an object that make up its persisted definition in the JSON body: public, readable, and either
    /// settable or a collection that items are added to. Shared by <see cref="BiteJsonWriter"/> and
    /// <see cref="BiteJsonReader"/> so what is written is exactly what can be read back.
    /// </summary>
    static class DefinitionMembers
    {
        const string WfNamespace = "System.Activities";

        // Runtime state and services, never definition (the same set the canonicalizer skips).
        static readonly HashSet<string> RuntimeState = new HashSet<string>(StringComparer.Ordinal)
        {
            "Compiler", "DataObject", "ExecutionEnvironment", "ScopingObject",
        };

        // Set by every activity's constructor to fixed VB references (DsfActivityAbstract); not worth storing.
        static readonly HashSet<string> ConstructorSet = new HashSet<string>(StringComparer.Ordinal)
        {
            "HasError", "IsValid", "InstructionList",
        };

        static readonly ConcurrentDictionary<Type, IReadOnlyList<PropertyInfo>> Cache = new ConcurrentDictionary<Type, IReadOnlyList<PropertyInfo>>();

        /// <summary>Definition members in a stable (ordinal name) order.</summary>
        public static IReadOnlyList<PropertyInfo> Of(Type type) => Cache.GetOrAdd(type, t =>
            t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.GetMethod?.IsPublic == true && p.GetIndexParameters().Length == 0)
                .GroupBy(p => p.Name, StringComparer.Ordinal)
                .Select(g => g.OrderByDescending(p => Depth(p.DeclaringType)).First())
                .Where(p => !RuntimeState.Contains(p.Name))
                // HasError & co are constructor-set plumbing only on activities; on a DTO they are real data.
                .Where(p => !ConstructorSet.Contains(p.Name) || !typeof(Dev2.IDev2Activity).IsAssignableFrom(t))
                .Where(p => p.DeclaringType?.Namespace?.StartsWith(WfNamespace, StringComparison.Ordinal) != true || p.Name == "DisplayName")
                .Where(p => p.SetMethod?.IsPublic == true || IsCollection(p.PropertyType))
                .OrderBy(p => p.Name, StringComparer.Ordinal)
                .ToList());

        public static PropertyInfo? Find(Type type, string name) => Of(type).FirstOrDefault(p => p.Name == name);

        public static bool IsCollection(Type type) =>
            type != typeof(string) && (typeof(IList).IsAssignableFrom(type) || typeof(IDictionary).IsAssignableFrom(type)
                                       || type.GetInterfaces().Append(type).Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICollection<>)));

        public static bool IsDictionary(Type type) =>
            typeof(IDictionary).IsAssignableFrom(type)
            || type.GetInterfaces().Append(type).Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>));

        /// <summary>The item type of an enumerable type (<c>T</c> of <c>IEnumerable&lt;T&gt;</c>), or object.</summary>
        public static Type ItemType(Type type) =>
            type.GetInterfaces().Append(type)
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                ?.GetGenericArguments()[0] ?? typeof(object);

        public static bool IsWarewolfType(Type type)
        {
            var name = type.Assembly.GetName().Name ?? "";
            return name.StartsWith("Dev2", StringComparison.Ordinal) || name.StartsWith("Warewolf", StringComparison.Ordinal);
        }

        static int Depth(Type? type)
        {
            var depth = 0;
            for (var t = type; t != null; t = t.BaseType)
            {
                depth++;
            }
            return depth;
        }
    }
}
