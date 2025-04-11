// Decompiled with JetBrains decompiler
// Type: System.Emission.IEmissionProxyHook
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Reflection;

namespace System.Emission
{
  internal interface IEmissionProxyHook
  {
    void MethodsInspected();

    void NonProxyableMemberNotification(Type type, MemberInfo memberInfo);

    bool ShouldInterceptMethod(Type type, MethodInfo methodInfo);
  }
}
