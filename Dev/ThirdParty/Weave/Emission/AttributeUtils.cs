
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace System.Emission
{
    internal static class AttributeUtil
    {
        /// <summary>
        ///   Gets the attribute.
        /// </summary>
        /// <param name = "member">The member.</param>
        /// <returns>The member attribute.</returns>
        public static bool HasAttribute<T>(this ICustomAttributeProvider member) where T : class
        {
            return GetAttributes<T>(member).FirstOrDefault() != null;
        }

        public static CustomAttributeBuilder CreateBuilder<TAttribute>() where TAttribute : Attribute, new()
        {
            ConstructorInfo constructor = typeof(TAttribute).GetConstructor(Type.EmptyTypes);
            Debug.Assert(constructor != null, "constructor != null");
            return new CustomAttributeBuilder(constructor, new object[0]);
        }

        public static CustomAttributeBuilder CreateBuilder(Type attribute, object[] constructorArguments)
        {
            Debug.Assert(attribute != null, "attribute != null");
            Debug.Assert(typeof(Attribute).IsAssignableFrom(attribute), "typeof(Attribute).IsAssignableFrom(attribute)");
            Debug.Assert(constructorArguments != null, "constructorArguments != null");

            ConstructorInfo constructor = attribute.GetConstructor(GetTypes(constructorArguments));
            Debug.Assert(constructor != null, "constructor != null");

            return new CustomAttributeBuilder(constructor, constructorArguments);
        }

        private static Type[] GetTypes(object[] objects)
        {
            Type[] types = new Type[objects.Length];

            for (int i = 0; i < types.Length; i++) types[i] = objects[i].GetType();
            return types;
        }

        public static CustomAttributeBuilder CreateBuilder(CustomAttributeData attribute)
        {
            Debug.Assert(attribute != null, "attribute != null");

            PropertyInfo[] properties;
            object[] propertyValues;
            FieldInfo[] fields;
            object[] fieldValues;

            GetSettersAndFields(attribute.NamedArguments, out properties, out propertyValues, out fields, out fieldValues);
            object[] constructorArgs = GetArguments(attribute.ConstructorArguments);
            return new CustomAttributeBuilder(attribute.Constructor, constructorArgs, properties, propertyValues, fields, fieldValues);
        }

        private static object[] GetArguments(IList<CustomAttributeTypedArgument> constructorArguments)
        {
            object[] arguments = new object[constructorArguments.Count];
            for (int i = 0; i < constructorArguments.Count; i++) arguments[i] = ReadAttributeValue(constructorArguments[i]);
            return arguments;
        }

        private static object ReadAttributeValue(CustomAttributeTypedArgument argument)
        {
            object value = argument.Value;
            if (argument.ArgumentType.IsArray == false) return value;
            object[] arguments = GetArguments((IList<CustomAttributeTypedArgument>)value);
            Array array = Array.CreateInstance(argument.ArgumentType.GetElementType() ?? typeof(object), arguments.Length);
            arguments.CopyTo(array, 0);
            return array;
        }

        private static void GetSettersAndFields(IEnumerable<CustomAttributeNamedArgument> namedArguments, out PropertyInfo[] properties, out object[] propertyValues, out FieldInfo[] fields, out object[] fieldValues)
        {
            List<PropertyInfo> propertyList = new List<PropertyInfo>();
            List<object> propertyValuesList = new List<object>();
            List<FieldInfo> fieldList = new List<FieldInfo>();
            List<object> fieldValuesList = new List<object>();

            foreach (var argument in namedArguments)
            {
                switch (argument.MemberInfo.MemberType)
                {
                    case MemberTypes.Property:
                        propertyList.Add(argument.MemberInfo as PropertyInfo);
                        propertyValuesList.Add(ReadAttributeValue(argument.TypedValue));
                        break;
                    case MemberTypes.Field:
                        fieldList.Add(argument.MemberInfo as FieldInfo);
                        fieldValuesList.Add(ReadAttributeValue(argument.TypedValue));
                        break;
                    default:
                        throw new ArgumentException(string.Format("Unexpected member type {0} in custom attribute.", argument.MemberInfo.MemberType));
                }
            }

            properties = propertyList.ToArray();
            propertyValues = propertyValuesList.ToArray();
            fields = fieldList.ToArray();
            fieldValues = fieldValuesList.ToArray();
        }

        /// <summary>
        ///   Gets the attributes. Does not consider inherited attributes!
        /// </summary>
        /// <param name = "member">The member.</param>
        /// <returns>The member attributes.</returns>
        public static T[] GetAttributes<T>(this ICustomAttributeProvider member) where T : class
        {
            if (typeof(T) != typeof(object)) return (T[])member.GetCustomAttributes(typeof(T), false);
            return (T[])member.GetCustomAttributes(false);
        }

        public static IEnumerable<CustomAttributeBuilder> GetNonInheritableAttributes(this ParameterInfo parameter)
        {
            Debug.Assert(parameter != null, "parameter != null");
            IList<CustomAttributeData> attributes = CustomAttributeData.GetCustomAttributes(parameter);

            foreach (CustomAttributeData attribute in attributes)
            {
                Type attributeType = attribute.Constructor.DeclaringType;
                if (ShouldSkipAttributeReplication(attributeType)) continue;

                CustomAttributeBuilder builder = CreateBuilder(attribute);
                if (builder != null) yield return builder;
            }
        }

        public static IEnumerable<CustomAttributeBuilder> GetNonInheritableAttributes(this MemberInfo member)
        {
            Debug.Assert(member != null, "member != null");
            IList<CustomAttributeData> attributes = CustomAttributeData.GetCustomAttributes(member);

            foreach (var attribute in attributes)
            {
                var attributeType = attribute.Constructor.DeclaringType;
                if (ShouldSkipAttributeReplication(attributeType))
                {
                    continue;
                }

                CustomAttributeBuilder builder;
                try
                {
                    builder = CreateBuilder(attribute);
                }
                catch (ArgumentException e)
                {
                    string message = string.Format("Due to limitations in CLR, Weave was unable to successfully replicate non-inheritable attribute {0} on {1}{2}. To avoid this error you can chose not to replicate this attribute type by calling '{3}.Add(typeof({0}))'.", attributeType.FullName, (member.ReflectedType == null) ? "" : member.ReflectedType.FullName, (member is Type) ? "" : ("." + member.Name), typeof(AttributesToAvoidReplicating).FullName);
                    throw new InvalidOperationException(message, e);
                }
                if (builder != null)
                {
                    yield return builder;
                }
            }
        }

        /// <summary>
        ///   Attributes should be replicated if they are non-inheritable,
        ///   but there are some special cases where the attributes means
        ///   something to the CLR, where they should be skipped.
        /// </summary>
        private static bool ShouldSkipAttributeReplication(Type attribute)
        {
            if (attribute.IsPublic == false)
            {
                return true;
            }

            if (SpecialCaseAttributThatShouldNotBeReplicated(attribute))
            {
                return true;
            }

            var attrs = attribute.GetCustomAttributes(typeof(AttributeUsageAttribute), true);

            if (attrs.Length != 0)
            {
                var usage = (AttributeUsageAttribute)attrs[0];

                return usage.Inherited;
            }

            return true;
        }

        private static bool SpecialCaseAttributThatShouldNotBeReplicated(Type attribute)
        {
            return AttributesToAvoidReplicating.Contains(attribute);
        }
    }

    internal static class AttributesToAvoidReplicating
    {
        private static readonly IList<Type> attributes = new List<Type>();

        static AttributesToAvoidReplicating()
        {
            Add<ComImportAttribute>();

            Add<SecurityPermissionAttribute>();


			Add<TypeIdentifierAttribute>();

        }

        public static void Add(Type attribute)
        {
            if (attributes.Contains(attribute) == false)
            {
                attributes.Add(attribute);
            }
        }

        public static void Add<T>()
        {
            Add(typeof(T));
        }

        public static bool Contains(Type type)
        {
            return attributes.Contains(type);
        }
    }
}
