// Decompiled with JetBrains decompiler
// Type: System.Emission.EmissionProxyHook
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Reflection;

namespace System.Emission
{
  internal class EmissionProxyHook : IEmissionProxyHook
  {
    protected static readonly Type[] _ignoredMemberSourceTypes = new Type[3]
    {
      typeof (object),
      typeof (MarshalByRefObject),
      typeof (ContextBoundObject)
    };

    public virtual bool ShouldInterceptMethod(Type type, MethodInfo methodInfo)
    {
      Type declaringType = methodInfo.DeclaringType;
      for (int index = 0; index < EmissionProxyHook._ignoredMemberSourceTypes.Length; ++index)
      {
        if (EmissionProxyHook._ignoredMemberSourceTypes[index].Equals(declaringType))
          return false;
      }
      return true;
    }

    public virtual void NonProxyableMemberNotification(Type type, MemberInfo memberInfo)
    {
    }

    public virtual void MethodsInspected()
    {
    }

    public override bool Equals(object obj) => obj != null && obj.GetType() == this.GetType();

    public override int GetHashCode() => this.GetType().GetHashCode();
  }
}
