// Decompiled with JetBrains decompiler
// Type: System.Emission.EmissionUtility
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

namespace System.Emission
{
  internal static class EmissionUtility
  {
    private static Dictionary<Assembly, bool> _cache = new Dictionary<Assembly, bool>();
    private static ReaderWriterLockSlim _cacheLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

    public static bool IsInternal(this MethodBase method)
    {
      if (method.IsAssembly)
        return true;
      return method.IsFamilyAndAssembly && !method.IsFamilyOrAssembly;
    }

    public static bool IsInternalToDynamicProxy(this Assembly asm, string dynamicProxyName)
    {
      EmissionUtility._cacheLock.EnterUpgradeableReadLock();
      try
      {
        if (EmissionUtility._cache.ContainsKey(asm))
          return EmissionUtility._cache[asm];
        EmissionUtility._cacheLock.EnterWriteLock();
        try
        {
          if (EmissionUtility._cache.ContainsKey(asm))
            return EmissionUtility._cache[asm];
          InternalsVisibleToAttribute[] attributes = asm.GetAttributes<InternalsVisibleToAttribute>();
          bool dynamicProxy = false;
          for (int index = 0; index < attributes.Length; ++index)
          {
            if (EmissionUtility.VisibleToDynamicProxy(attributes[index], dynamicProxyName))
            {
              dynamicProxy = true;
              break;
            }
          }
          EmissionUtility._cache.Add(asm, dynamicProxy);
          return dynamicProxy;
        }
        finally
        {
          EmissionUtility._cacheLock.ExitWriteLock();
        }
      }
      finally
      {
        EmissionUtility._cacheLock.ExitUpgradeableReadLock();
      }
    }

    private static bool VisibleToDynamicProxy(
      InternalsVisibleToAttribute attribute,
      string dynamicProxyName)
    {
      return attribute.AssemblyName.Contains(dynamicProxyName);
    }

    public static bool IsAccessible(this MethodBase method, string dynamicAssemblyName) => method.IsPublic || method.IsFamily || method.IsFamilyOrAssembly || method.IsFamilyAndAssembly || method.DeclaringType.Assembly.IsInternalToDynamicProxy(dynamicAssemblyName) && method.IsAssembly;
  }
}
