// Decompiled with JetBrains decompiler
// Type: System.Emission.Emitters.ClassEmitter
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace System.Emission.Emitters
{
  internal sealed class ClassEmitter : AbstractTypeEmitter
  {
    private const TypeAttributes DefaultAttributes = TypeAttributes.Public;
    private readonly EmissionModule _module;

    public EmissionModule ModuleScope => this._module;

    private static TypeBuilder CreateTypeBuilder(
      EmissionModule module,
      string name,
      Type baseType,
      IEnumerable<Type> interfaces,
      TypeAttributes flags)
    {
      return module.DefineType(name, flags);
    }

    public ClassEmitter(
      EmissionModule module,
      string name,
      Type baseType,
      IEnumerable<Type> interfaces)
      : this(module, name, baseType, interfaces, TypeAttributes.Public)
    {
    }

    public ClassEmitter(
      EmissionModule module,
      string name,
      Type baseType,
      IEnumerable<Type> interfaces,
      TypeAttributes flags)
      : this(ClassEmitter.CreateTypeBuilder(module, name, baseType, interfaces, flags), module.DynamicAssemblyName)
    {
      interfaces = this.InitializeGenericArgumentsFromBases(ref baseType, interfaces);
      if (interfaces != null)
      {
        foreach (Type interfaceType in interfaces)
          this.TypeBuilder.AddInterfaceImplementation(interfaceType);
      }
      this.TypeBuilder.SetParent(baseType);
      this._module = module;
    }

    public ClassEmitter(TypeBuilder typeBuilder, string dynamicAssemblyName)
      : base(typeBuilder, dynamicAssemblyName)
    {
    }

    private IEnumerable<Type> InitializeGenericArgumentsFromBases(
      ref Type baseType,
      IEnumerable<Type> interfaces)
    {
      if (baseType != (Type) null && baseType.IsGenericTypeDefinition)
        throw new NotSupportedException("ClassEmitter does not support open generic base types. Type: " + baseType.FullName);
      if (interfaces == null)
        return interfaces;
      foreach (Type type in interfaces)
      {
        if (type.IsGenericTypeDefinition)
          throw new NotSupportedException("ClassEmitter does not support open generic interfaces. Type: " + type.FullName);
      }
      return interfaces;
    }
  }
}
