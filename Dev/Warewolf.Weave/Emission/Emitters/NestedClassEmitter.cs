// Decompiled with JetBrains decompiler
// Type: System.Emission.Emitters.NestedClassEmitter
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Reflection;
using System.Reflection.Emit;

namespace System.Emission.Emitters
{
  internal sealed class NestedClassEmitter : AbstractTypeEmitter
  {
    private static TypeBuilder CreateTypeBuilder(
      AbstractTypeEmitter maintype,
      string name,
      TypeAttributes attributes,
      Type baseType,
      Type[] interfaces)
    {
      return maintype.TypeBuilder.DefineNestedType(name, attributes, baseType, interfaces);
    }

    public NestedClassEmitter(
      AbstractTypeEmitter maintype,
      string name,
      Type baseType,
      Type[] interfaces)
      : this(maintype, NestedClassEmitter.CreateTypeBuilder(maintype, name, TypeAttributes.NestedPublic | TypeAttributes.Sealed, baseType, interfaces))
    {
    }

    public NestedClassEmitter(
      AbstractTypeEmitter maintype,
      string name,
      TypeAttributes attributes,
      Type baseType,
      Type[] interfaces)
      : this(maintype, NestedClassEmitter.CreateTypeBuilder(maintype, name, attributes, baseType, interfaces))
    {
    }

    public NestedClassEmitter(AbstractTypeEmitter maintype, TypeBuilder typeBuilder)
      : base(typeBuilder, maintype.DynamicAssemblyName)
    {
      maintype.Nested.Add(this);
    }
  }
}
