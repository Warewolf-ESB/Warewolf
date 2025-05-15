// Decompiled with JetBrains decompiler
// Type: System.Emission.Meta.MethodSignatureComparer
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Collections.Generic;
using System.Reflection;

namespace System.Emission.Meta
{
  internal sealed class MethodSignatureComparer : IEqualityComparer<MethodInfo>
  {
    public static readonly MethodSignatureComparer Instance = new MethodSignatureComparer();

    public bool EqualGenericParameters(MethodInfo x, MethodInfo y)
    {
      if (x.IsGenericMethod != y.IsGenericMethod)
        return false;
      if (x.IsGenericMethod)
      {
        Type[] genericArguments1 = x.GetGenericArguments();
        Type[] genericArguments2 = y.GetGenericArguments();
        if (genericArguments1.Length != genericArguments2.Length)
          return false;
        for (int index = 0; index < genericArguments1.Length; ++index)
        {
          if (genericArguments1[index].IsGenericParameter != genericArguments2[index].IsGenericParameter || !genericArguments1[index].IsGenericParameter && !genericArguments1[index].Equals(genericArguments2[index]))
            return false;
        }
      }
      return true;
    }

    public bool EqualParameters(MethodInfo x, MethodInfo y)
    {
      ParameterInfo[] parameters1 = x.GetParameters();
      ParameterInfo[] parameters2 = y.GetParameters();
      if (parameters1.Length != parameters2.Length)
        return false;
      for (int index = 0; index < parameters1.Length; ++index)
      {
        if (!this.EqualSignatureTypes(parameters1[index].ParameterType, parameters2[index].ParameterType))
          return false;
      }
      return true;
    }

    public bool EqualSignatureTypes(Type x, Type y)
    {
      if (x.IsGenericParameter != y.IsGenericParameter)
        return false;
      if (x.IsGenericParameter)
      {
        if (x.GenericParameterPosition != y.GenericParameterPosition)
          return false;
      }
      else if (!x.Equals(y))
        return false;
      return true;
    }

    public bool Equals(MethodInfo x, MethodInfo y)
    {
      if (x == (MethodInfo) null && y == (MethodInfo) null)
        return true;
      return !(x == (MethodInfo) null) && !(y == (MethodInfo) null) && this.EqualNames(x, y) && this.EqualGenericParameters(x, y) && this.EqualSignatureTypes(x.ReturnType, y.ReturnType) && this.EqualParameters(x, y);
    }

    public int GetHashCode(MethodInfo obj) => obj.Name.GetHashCode() ^ obj.GetParameters().Length;

    private bool EqualNames(MethodInfo x, MethodInfo y) => x.Name == y.Name;
  }
}
