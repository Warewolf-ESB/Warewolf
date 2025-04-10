// Decompiled with JetBrains decompiler
// Type: System.Emission.EmissionCacheKey
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Reflection;

namespace System.Emission
{
  internal sealed class EmissionCacheKey
  {
    private readonly MemberInfo _target;
    private readonly Type[] _interfaces;
    private readonly EmissionProxyOptions _options;
    private readonly Type _type;

    public EmissionCacheKey(
      MemberInfo target,
      Type type,
      Type[] interfaces,
      EmissionProxyOptions options)
    {
      this._target = target;
      this._type = type;
      this._interfaces = interfaces ?? Type.EmptyTypes;
      this._options = options;
    }

    public EmissionCacheKey(Type target, Type[] interfaces, EmissionProxyOptions options)
      : this((MemberInfo) target, (Type) null, interfaces, options)
    {
    }

    public override int GetHashCode()
    {
      int hashCode = this._target.GetHashCode();
      foreach (Type type in this._interfaces)
        hashCode += 29 + type.GetHashCode();
      if (this._options != null)
        hashCode = 29 * hashCode + this._options.GetHashCode();
      if (this._type != (Type) null)
        hashCode = 29 * hashCode + this._type.GetHashCode();
      return hashCode;
    }

    public override bool Equals(object obj)
    {
      if (this == obj)
        return true;
      if (!(obj is EmissionCacheKey emissionCacheKey) || !object.Equals((object) this._type, (object) emissionCacheKey._type) || !object.Equals((object) this._target, (object) emissionCacheKey._target) || this._interfaces.Length != emissionCacheKey._interfaces.Length)
        return false;
      for (int index = 0; index < this._interfaces.Length; ++index)
      {
        if (!object.Equals((object) this._interfaces[index], (object) emissionCacheKey._interfaces[index]))
          return false;
      }
      return object.Equals((object) this._options, (object) emissionCacheKey._options);
    }
  }
}
