// Decompiled with JetBrains decompiler
// Type: System.Emission.Collectors.NetworkInterfaceProxyMemberCollector
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Emission.Meta;
using System.Reflection;

namespace System.Emission.Collectors
{
  internal sealed class NetworkInterfaceProxyMemberCollector : EmissionMemberCollector
  {
    public NetworkInterfaceProxyMemberCollector(string dynamicAssemblyName, Type interfaceType)
      : base(interfaceType, dynamicAssemblyName)
    {
    }

    protected override MetaMethod GetMethodToGenerate(
      MethodInfo method,
      IEmissionProxyHook hook,
      bool isStandalone,
      MetaMethodSource source)
    {
      if (!method.IsAccessible(this.DynamicAsssemblyName))
        return (MetaMethod) null;
      bool proxyable = this.AcceptMethod(method, false, hook);
      return new MetaMethod(this.DynamicAsssemblyName, method, method, isStandalone, proxyable, false, source);
    }
  }
}
