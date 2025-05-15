// Decompiled with JetBrains decompiler
// Type: System.Emission.GenericUtil
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Collections.Generic;
using System.Emission.Emitters;
using System.Reflection;
using System.Reflection.Emit;

namespace System.Emission
{
  internal class GenericUtil
  {
    public static GenericTypeParameterBuilder[] CopyGenericArguments(
      MethodInfo methodToCopyGenericsFrom,
      TypeBuilder builder,
      Dictionary<string, GenericTypeParameterBuilder> name2GenericType)
    {
      return GenericUtil.CopyGenericArguments(methodToCopyGenericsFrom, name2GenericType, new ApplyGenArgs(builder.DefineGenericParameters));
    }

    public static GenericTypeParameterBuilder[] CopyGenericArguments(
      MethodInfo methodToCopyGenericsFrom,
      MethodBuilder builder,
      Dictionary<string, GenericTypeParameterBuilder> name2GenericType)
    {
      return GenericUtil.CopyGenericArguments(methodToCopyGenericsFrom, name2GenericType, new ApplyGenArgs(builder.DefineGenericParameters));
    }

    public static Type ExtractCorrectType(
      Type paramType,
      Dictionary<string, GenericTypeParameterBuilder> name2GenericType)
    {
      if (paramType.IsArray)
      {
        int arrayRank = paramType.GetArrayRank();
        Type elementType = paramType.GetElementType();
        if (elementType.IsGenericParameter)
        {
          GenericTypeParameterBuilder parameterBuilder;
          if (!name2GenericType.TryGetValue(elementType.Name, out parameterBuilder))
            return paramType;
          return arrayRank == 1 ? parameterBuilder.MakeArrayType() : parameterBuilder.MakeArrayType(arrayRank);
        }
        return arrayRank == 1 ? elementType.MakeArrayType() : elementType.MakeArrayType(arrayRank);
      }
      GenericTypeParameterBuilder parameterBuilder1;
      return paramType.IsGenericParameter && name2GenericType.TryGetValue(paramType.Name, out parameterBuilder1) ? (Type) parameterBuilder1 : paramType;
    }

    public static Type[] ExtractParametersTypes(
      ParameterInfo[] baseMethodParameters,
      Dictionary<string, GenericTypeParameterBuilder> name2GenericType)
    {
      Type[] parametersTypes = new Type[baseMethodParameters.Length];
      for (int index = 0; index < baseMethodParameters.Length; ++index)
      {
        Type parameterType = baseMethodParameters[index].ParameterType;
        parametersTypes[index] = GenericUtil.ExtractCorrectType(parameterType, name2GenericType);
      }
      return parametersTypes;
    }

    public static Dictionary<string, GenericTypeParameterBuilder> GetGenericArgumentsMap(
      AbstractTypeEmitter parentEmitter)
    {
      if (parentEmitter.GenericTypeParams == null || parentEmitter.GenericTypeParams.Length == 0)
        return new Dictionary<string, GenericTypeParameterBuilder>(0);
      Dictionary<string, GenericTypeParameterBuilder> genericArgumentsMap = new Dictionary<string, GenericTypeParameterBuilder>(parentEmitter.GenericTypeParams.Length);
      foreach (GenericTypeParameterBuilder genericTypeParam in parentEmitter.GenericTypeParams)
        genericArgumentsMap.Add(genericTypeParam.Name, genericTypeParam);
      return genericArgumentsMap;
    }

    private static Type AdjustConstraintToNewGenericParameters(
      Type constraint,
      MethodInfo methodToCopyGenericsFrom,
      Type[] originalGenericParameters,
      GenericTypeParameterBuilder[] newGenericParameters)
    {
      if (constraint.IsGenericType)
      {
        Type[] genericArguments = constraint.GetGenericArguments();
        for (int index = 0; index < genericArguments.Length; ++index)
          genericArguments[index] = GenericUtil.AdjustConstraintToNewGenericParameters(genericArguments[index], methodToCopyGenericsFrom, originalGenericParameters, newGenericParameters);
        return constraint.GetGenericTypeDefinition().MakeGenericType(genericArguments);
      }
      if (!constraint.IsGenericParameter)
        return constraint;
      if (constraint.DeclaringMethod != (MethodBase) null)
      {
        int index = Array.IndexOf<Type>(originalGenericParameters, constraint);
        return (Type) newGenericParameters[index];
      }
      int index1 = Array.IndexOf<Type>(constraint.DeclaringType.GetGenericArguments(), constraint);
      return methodToCopyGenericsFrom.DeclaringType.GetGenericArguments()[index1];
    }

    private static Type[] AdjustGenericConstraints(
      MethodInfo methodToCopyGenericsFrom,
      GenericTypeParameterBuilder[] newGenericParameters,
      Type[] originalGenericArguments,
      Type[] constraints)
    {
      for (int index = 0; index < constraints.Length; ++index)
        constraints[index] = GenericUtil.AdjustConstraintToNewGenericParameters(constraints[index], methodToCopyGenericsFrom, originalGenericArguments, newGenericParameters);
      return constraints;
    }

    private static GenericTypeParameterBuilder[] CopyGenericArguments(
      MethodInfo methodToCopyGenericsFrom,
      Dictionary<string, GenericTypeParameterBuilder> name2GenericType,
      ApplyGenArgs genericParameterGenerator)
    {
      Type[] genericArguments = methodToCopyGenericsFrom.GetGenericArguments();
      if (genericArguments.Length == 0)
        return (GenericTypeParameterBuilder[]) null;
      string[] argumentNames = GenericUtil.GetArgumentNames(genericArguments);
      GenericTypeParameterBuilder[] newGenericParameters = genericParameterGenerator(argumentNames);
      for (int index = 0; index < newGenericParameters.Length; ++index)
      {
        try
        {
          GenericParameterAttributes parameterAttributes = genericArguments[index].GenericParameterAttributes;
          newGenericParameters[index].SetGenericParameterAttributes(parameterAttributes);
          Type[] typeArray = GenericUtil.AdjustGenericConstraints(methodToCopyGenericsFrom, newGenericParameters, genericArguments, genericArguments[index].GetGenericParameterConstraints());
          newGenericParameters[index].SetInterfaceConstraints(typeArray);
          GenericUtil.CopyNonInheritableAttributes(newGenericParameters[index], genericArguments[index]);
        }
        catch (NotSupportedException ex)
        {
          newGenericParameters[index].SetGenericParameterAttributes(GenericParameterAttributes.None);
        }
        name2GenericType[argumentNames[index]] = newGenericParameters[index];
      }
      return newGenericParameters;
    }

    private static void CopyNonInheritableAttributes(
      GenericTypeParameterBuilder newGenericParameter,
      Type originalGenericArgument)
    {
      foreach (CustomAttributeBuilder inheritableAttribute in originalGenericArgument.GetNonInheritableAttributes())
        newGenericParameter.SetCustomAttribute(inheritableAttribute);
    }

    private static string[] GetArgumentNames(Type[] originalGenericArguments)
    {
      string[] argumentNames = new string[originalGenericArguments.Length];
      for (int index = 0; index < argumentNames.Length; ++index)
        argumentNames[index] = originalGenericArguments[index].Name;
      return argumentNames;
    }
  }
}
