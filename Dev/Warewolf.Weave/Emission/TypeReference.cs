// Decompiled with JetBrains decompiler
// Type: System.Emission.TypeReference
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

namespace System.Emission
{
  internal abstract class TypeReference : Reference
  {
    private Type _type;

    public Type Type => this._type;

    protected TypeReference(Type argumentType)
      : this((Reference) null, argumentType)
    {
    }

    protected TypeReference(Reference owner, Type type)
      : base(owner)
    {
      this._type = type;
    }
  }
}
