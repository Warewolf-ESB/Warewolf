// Decompiled with JetBrains decompiler
// Type: System.Emission.TypeUtil
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace System.Emission
{
  internal static class TypeUtil
  {
    public static bool IsGetType(this MethodInfo methodInfo) => methodInfo.DeclaringType == typeof (object) && string.Equals("GetType", methodInfo.Name, StringComparison.OrdinalIgnoreCase);

    public static bool IsFinalizer(this MethodInfo methodInfo) => string.Equals("Finalize", methodInfo.Name) && methodInfo.GetBaseDefinition().DeclaringType == typeof (object);

    public static bool IsMemberwiseClone(this MethodInfo methodInfo) => methodInfo.DeclaringType == typeof (object) && string.Equals("MemberwiseClone", methodInfo.Name, StringComparison.OrdinalIgnoreCase);

    public static ICollection<Type> GetAllInterfaces(params Type[] types)
    {
      if (types == null)
        return (ICollection<Type>) Type.EmptyTypes;
      object obj = new object();
      IDictionary<Type, object> dictionary = (IDictionary<Type, object>) new Dictionary<Type, object>();
      foreach (Type type in types)
      {
        if (!(type == (Type) null))
        {
          if (type.IsInterface)
            dictionary[type] = obj;
          foreach (Type key in type.GetInterfaces())
            dictionary[key] = obj;
        }
      }
      return (ICollection<Type>) TypeUtil.Sort((IEnumerable<Type>) dictionary.Keys);
    }

    public static ICollection<Type> GetAllInterfaces(this Type type) => TypeUtil.GetAllInterfaces(new Type[1]
    {
      type
    });

    public static void SetStaticField(
      this Type type,
      string fieldName,
      BindingFlags additionalFlags,
      object value)
    {
      BindingFlags invokeAttr = additionalFlags | BindingFlags.Static | BindingFlags.SetField;
      try
      {
        type.InvokeMember(fieldName, invokeAttr, (Binder) null, (object) null, new object[1]
        {
          value
        });
      }
      catch (MissingFieldException ex)
      {
		 string message = $"Could not find field named '{fieldName}' on type {type}. This is likely a bug in Weave. Please report it.";
		 throw new InvalidOperationException(message, (Exception) ex);
      }
      catch (TargetException ex)
      {
		 if (!(ex.InnerException is TypeInitializationException))
		 {
		   throw;
		 }
		 else
		 {
		   string message = $"There was an error in static constructor on type {type}. This is likely a bug in Weave. Please report it.";
		   throw new InvalidOperationException(message, ex);
		 }
      }
      catch (TargetInvocationException ex)
      {
        if (!(ex.InnerException is TypeInitializationException))
        {
          throw;
        }
        else
        {
		  string message = $"There was an error in static constructor on type {type}. This is likely a bug in Weave. Please report it.";
		  throw new InvalidOperationException(message, ex);
        }
      }
    }

    public static MemberInfo[] Sort(MemberInfo[] members)
    {
      MemberInfo[] memberInfoArray = new MemberInfo[members.Length];
      Array.Copy((Array) members, (Array) memberInfoArray, members.Length);
      Array.Sort<MemberInfo>(memberInfoArray, (Comparison<MemberInfo>) ((l, r) => string.Compare(l.Name, r.Name, StringComparison.OrdinalIgnoreCase)));
      return memberInfoArray;
    }

    private static Type[] Sort(IEnumerable<Type> types)
    {
      Type[] array = types.ToArray<Type>();
      Array.Sort<Type>(array, (Comparison<Type>) ((l, r) => string.Compare(l.AssemblyQualifiedName, r.AssemblyQualifiedName, StringComparison.OrdinalIgnoreCase)));
      return array;
    }
  }
}
