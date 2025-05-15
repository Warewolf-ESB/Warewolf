// Decompiled with JetBrains decompiler
// Type: System.Emission.Meta.MetaTypeElement
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

namespace System.Emission.Meta
{
  internal abstract class MetaTypeElement
  {
    protected readonly Type sourceType;
    private string _dynamicAssemblyName;

    public string DynamicAssemblyName => this._dynamicAssemblyName;

    internal bool CanBeImplementedExplicitly => this.sourceType != (Type) null && this.sourceType.IsInterface;

    protected MetaTypeElement(Type sourceType, string dynamicAssemblyName)
    {
      this.sourceType = sourceType;
      this._dynamicAssemblyName = dynamicAssemblyName;
    }

    internal abstract void SwitchToExplicitImplementation();
  }
}
