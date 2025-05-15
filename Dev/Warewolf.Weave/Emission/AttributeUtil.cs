// Decompiled with JetBrains decompiler
// Type: System.Emission.AttributeUtil
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace System.Emission
{
  internal static class AttributeUtil
  {
    public static bool HasAttribute<T>(this ICustomAttributeProvider member) where T : class => (object) ((IEnumerable<T>) member.GetAttributes<T>()).FirstOrDefault<T>() != null;

    public static CustomAttributeBuilder CreateBuilder<TAttribute>() where TAttribute : Attribute, new()
    {
      ConstructorInfo constructor = typeof (TAttribute).GetConstructor(Type.EmptyTypes);
      Debug.Assert(constructor != (ConstructorInfo) null, "constructor != null");
      return new CustomAttributeBuilder(constructor, new object[0]);
    }

    public static CustomAttributeBuilder CreateBuilder(
      Type attribute,
      object[] constructorArguments)
    {
      Debug.Assert(attribute != (Type) null, "attribute != null");
      Debug.Assert(typeof (Attribute).IsAssignableFrom(attribute), "typeof(Attribute).IsAssignableFrom(attribute)");
      Debug.Assert(constructorArguments != null, "constructorArguments != null");
      ConstructorInfo constructor = attribute.GetConstructor(AttributeUtil.GetTypes(constructorArguments));
      Debug.Assert(constructor != (ConstructorInfo) null, "constructor != null");
      return new CustomAttributeBuilder(constructor, constructorArguments);
    }

    private static Type[] GetTypes(object[] objects)
    {
      Type[] types = new Type[objects.Length];
      for (int index = 0; index < types.Length; ++index)
        types[index] = objects[index].GetType();
      return types;
    }

    public static CustomAttributeBuilder CreateBuilder(CustomAttributeData attribute)
    {
      Debug.Assert(attribute != null, "attribute != null");
      PropertyInfo[] properties;
      object[] propertyValues;
      FieldInfo[] fields;
      object[] fieldValues;
      AttributeUtil.GetSettersAndFields((IEnumerable<CustomAttributeNamedArgument>) attribute.NamedArguments, out properties, out propertyValues, out fields, out fieldValues);
      object[] arguments = AttributeUtil.GetArguments(attribute.ConstructorArguments);
      return new CustomAttributeBuilder(attribute.Constructor, arguments, properties, propertyValues, fields, fieldValues);
    }

    private static object[] GetArguments(
      IList<CustomAttributeTypedArgument> constructorArguments)
    {
      object[] arguments = new object[constructorArguments.Count];
      for (int index = 0; index < constructorArguments.Count; ++index)
        arguments[index] = AttributeUtil.ReadAttributeValue(constructorArguments[index]);
      return arguments;
    }

    private static object ReadAttributeValue(CustomAttributeTypedArgument argument)
    {
      object constructorArguments = argument.Value;
      if (!argument.ArgumentType.IsArray)
        return constructorArguments;
      object[] arguments = AttributeUtil.GetArguments((IList<CustomAttributeTypedArgument>) constructorArguments);
      Type elementType = argument.ArgumentType.GetElementType();
      if ((object) elementType == null)
        elementType = typeof (object);
      Array instance = Array.CreateInstance(elementType, arguments.Length);
      arguments.CopyTo(instance, 0);
      return (object) instance;
    }

    private static void GetSettersAndFields(
      IEnumerable<CustomAttributeNamedArgument> namedArguments,
      out PropertyInfo[] properties,
      out object[] propertyValues,
      out FieldInfo[] fields,
      out object[] fieldValues)
    {
      List<PropertyInfo> propertyInfoList = new List<PropertyInfo>();
      List<object> objectList1 = new List<object>();
      List<FieldInfo> fieldInfoList = new List<FieldInfo>();
      List<object> objectList2 = new List<object>();
      foreach (CustomAttributeNamedArgument namedArgument in namedArguments)
      {
        switch (namedArgument.MemberInfo.MemberType)
        {
          case MemberTypes.Field:
            fieldInfoList.Add(namedArgument.MemberInfo as FieldInfo);
            objectList2.Add(AttributeUtil.ReadAttributeValue(namedArgument.TypedValue));
            continue;
          case MemberTypes.Property:
            propertyInfoList.Add(namedArgument.MemberInfo as PropertyInfo);
            objectList1.Add(AttributeUtil.ReadAttributeValue(namedArgument.TypedValue));
            continue;
          default:
			string errorMessage = $"Unexpected member type {namedArgument.MemberInfo.MemberType} in custom attribute.";
            throw new ArgumentException(errorMessage);
        }
      }
      properties = propertyInfoList.ToArray();
      propertyValues = objectList1.ToArray();
      fields = fieldInfoList.ToArray();
      fieldValues = objectList2.ToArray();
    }

    public static T[] GetAttributes<T>(this ICustomAttributeProvider member) where T : class => typeof (T) != typeof (object) ? (T[]) member.GetCustomAttributes(typeof (T), false) : (T[]) member.GetCustomAttributes(false);

    public static IEnumerable<CustomAttributeBuilder> GetNonInheritableAttributes(
      this ParameterInfo parameter)
    {
      Debug.Assert(parameter != null, "parameter != null");
      foreach (CustomAttributeData customAttribute in (IEnumerable<CustomAttributeData>) CustomAttributeData.GetCustomAttributes(parameter))
      {
        if (!AttributeUtil.ShouldSkipAttributeReplication(customAttribute.Constructor.DeclaringType))
        {
          CustomAttributeBuilder builder = AttributeUtil.CreateBuilder(customAttribute);
          if (builder != null)
            yield return builder;
        }
      }
    }

    public static IEnumerable<CustomAttributeBuilder> GetNonInheritableAttributes(
      this MemberInfo member)
    {
      Debug.Assert(member != (MemberInfo) null, "member != null");
      foreach (CustomAttributeData customAttribute in (IEnumerable<CustomAttributeData>) CustomAttributeData.GetCustomAttributes(member))
      {
        Type declaringType = customAttribute.Constructor.DeclaringType;
        if (!AttributeUtil.ShouldSkipAttributeReplication(declaringType))
        {
          CustomAttributeBuilder builder;
          try
          {
            builder = AttributeUtil.CreateBuilder(customAttribute);
          }
          catch (ArgumentException ex)
          {
            throw new InvalidOperationException(string.Format("Due to limitations in CLR, Weave was unable to successfully replicate non-inheritable attribute {0} on {1}{2}. To avoid this error you can chose not to replicate this attribute type by calling '{3}.Add(typeof({0}))'.", (object) declaringType.FullName, member.ReflectedType == (Type) null ? (object) "" : (object) member.ReflectedType.FullName, (object) (member as Type) != null ? (object) "" : (object) ("." + member.Name), (object) typeof (AttributesToAvoidReplicating).FullName), (Exception) ex);
          }
          if (builder != null)
            yield return builder;
        }
      }
    }

    private static bool ShouldSkipAttributeReplication(Type attribute)
    {
      if (!attribute.IsPublic || AttributeUtil.SpecialCaseAttributThatShouldNotBeReplicated(attribute))
        return true;
      object[] customAttributes = attribute.GetCustomAttributes(typeof (AttributeUsageAttribute), true);
      return customAttributes.Length == 0 || ((AttributeUsageAttribute) customAttributes[0]).Inherited;
    }

    private static bool SpecialCaseAttributThatShouldNotBeReplicated(Type attribute) => AttributesToAvoidReplicating.Contains(attribute);
  }
}
