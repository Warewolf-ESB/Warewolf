// Decompiled with JetBrains decompiler
// Type: System.Emission.NetworkInterfaceProxyGenerator
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Collections.Generic;
using System.Emission.Contributors;
using System.Emission.Emitters;
using System.Emission.Meta;
using System.Linq;
using System.Network;
using System.Reflection;

namespace System.Emission
{
  internal sealed class NetworkInterfaceProxyGenerator : BaseDisposable
  {
    private EmissionModule _module;
    private Type _targetType;
    private EmissionProxyOptions _emissionOptions;
    private FieldReference _targetField;

    private static void AssertNotGenericTypeDefinition(Type type, string argumentName)
    {
      if (type != (Type) null && type.IsGenericTypeDefinition)
        throw new ArgumentException("Type cannot be a generic type definition. Type: " + type.FullName, argumentName);
    }

    private static void AssertNotGenericTypeDefinitions(
      IEnumerable<Type> types,
      string argumentName)
    {
      if (types == null)
        return;
      foreach (Type type in types)
        NetworkInterfaceProxyGenerator.AssertNotGenericTypeDefinition(type, argumentName);
    }

    private static void EnsureValidBaseType(Type type)
    {
      if (type == (Type) null)
        throw new ArgumentException("Base type for proxy is null reference. Please set it to System.Object or some other valid type.");
      if (!type.IsClass)
        NetworkInterfaceProxyGenerator.ThrowInvalidBaseType(type, "it is not a class type");
      if (type.IsSealed)
        NetworkInterfaceProxyGenerator.ThrowInvalidBaseType(type, "it is sealed");
      ConstructorInfo constructor = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, (Binder) null, Type.EmptyTypes, (ParameterModifier[]) null);
      if (!(constructor == (ConstructorInfo) null) && !constructor.IsPrivate)
        return;
      NetworkInterfaceProxyGenerator.ThrowInvalidBaseType(type, "it does not have accessible parameterless constructor");
    }

    private static bool ImplementedByTarget(ICollection<Type> targetInterfaces, Type interfaceType) => targetInterfaces.Contains(interfaceType);

    private static void ThrowInvalidBaseType(
      Type type,
      string doesNotHaveAccessibleParameterlessConstructor)
    {
      throw new ArgumentException(string.Format("Type {0} is not valid base type for interface proxy, because {1}. Only a non-sealed class with non-private default constructor can be used as base type for interface proxy. Please use some other valid type.", (object) type, (object) doesNotHaveAccessibleParameterlessConstructor));
    }

    internal NetworkInterfaceProxyGenerator(EmissionModule module, Type targetType)
    {
      this._module = module;
      this._targetType = targetType;
      NetworkInterfaceProxyGenerator.AssertNotGenericTypeDefinition(targetType, nameof (targetType));
    }

    public Type Generate(Type proxyBaseType, EmissionProxyOptions options)
    {
      NetworkInterfaceProxyGenerator.AssertNotGenericTypeDefinition(proxyBaseType, nameof (proxyBaseType));
      NetworkInterfaceProxyGenerator.EnsureValidBaseType(options.InterfaceProxyBaseType);
      this._emissionOptions = options;
      return this.ObtainProxyType(new EmissionCacheKey((MemberInfo) proxyBaseType, this._targetType, Type.EmptyTypes, options), proxyBaseType);
    }

    private Type GenerateType(string typeName, Type proxyBaseType, IDesignatingScope namingScope)
    {
      IEnumerable<IEmissionContributor> contributors;
      IEnumerable<Type> implementerMapping = this.GetTypeImplementerMapping(Type.EmptyTypes, this._targetType, out contributors, namingScope);
      MetaType model = new MetaType();
      foreach (IEmissionContributor emissionContributor in contributors)
        emissionContributor.CollectElements(this._emissionOptions.Hook, model);
      this._emissionOptions.Hook.MethodsInspected();
      ClassEmitter emitter;
      Type baseType = this.Init(typeName, out emitter, proxyBaseType, implementerMapping);
      ConstructorEmitter staticConstructor = this.GenerateStaticConstructor(emitter);
      foreach (IEmissionContributor emissionContributor in contributors)
        emissionContributor.Generate(emitter, this._emissionOptions);
      List<FieldReference> fieldReferenceList = new List<FieldReference>()
      {
        this._targetField
      };
      FieldReference field1 = emitter.GetField("__selector");
      if (field1 != null)
        fieldReferenceList.Add(field1);
      FieldReference field2 = emitter.GetField("__template");
      if (field2 != null)
        fieldReferenceList.Add(field2);
      this.GenerateConstructors(emitter, baseType, fieldReferenceList.ToArray());
      this.CompleteInitCacheMethod(staticConstructor.CodeBuilder);
      Type builtType = emitter.BuildType();
      this.InitializeStaticFields(builtType);
      return builtType;
    }

    private Type Init(
      string typeName,
      out ClassEmitter emitter,
      Type proxyTargetType,
      IEnumerable<Type> interfaces)
    {
      Type interfaceProxyBaseType = this._emissionOptions.InterfaceProxyBaseType;
      emitter = this.BuildClassEmitter(typeName, interfaceProxyBaseType, interfaces);
      this.CreateFields(emitter, proxyTargetType);
      this.CreateTypeAttributes(emitter);
      return interfaceProxyBaseType;
    }

    private ClassEmitter BuildClassEmitter(
      string typeName,
      Type parentType,
      IEnumerable<Type> interfaces)
    {
      NetworkInterfaceProxyGenerator.AssertNotGenericTypeDefinition(parentType, nameof (parentType));
      NetworkInterfaceProxyGenerator.AssertNotGenericTypeDefinitions(interfaces, nameof (interfaces));
      return new ClassEmitter(this._module, typeName, parentType, interfaces);
    }

    private void CreateFields(ClassEmitter emitter, Type proxyTargetType)
    {
      emitter.CreateField("__template", typeof (PacketTemplate), FieldAttributes.Private);
      this._targetField = emitter.CreateField("__target", proxyTargetType, FieldAttributes.Private);
    }

    private void CreateTypeAttributes(ClassEmitter emitter) => emitter.AddCustomAttributes(this._emissionOptions);

    private void CompleteInitCacheMethod(ConstructorCodeBuilder constCodeBuilder) => constCodeBuilder.AddStatement((Statement) new ReturnStatement());

    private void InitializeStaticFields(Type builtType)
    {
    }

    private void GenerateConstructors(
      ClassEmitter emitter,
      Type baseType,
      params FieldReference[] fields)
    {
      foreach (ConstructorInfo constructor in baseType.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
      {
        if (this.IsConstructorVisible(constructor))
          this.GenerateConstructor(emitter, constructor, fields);
      }
    }

    private void GenerateConstructor(
      ClassEmitter emitter,
      ConstructorInfo baseConstructor,
      params FieldReference[] fields)
    {
      ParameterInfo[] source = (ParameterInfo[]) null;
      if (baseConstructor != (ConstructorInfo) null)
        source = baseConstructor.GetParameters();
      ArgumentReference[] sourceArray;
      if (source != null && source.Length != 0)
      {
        sourceArray = new ArgumentReference[fields.Length + source.Length];
        int length = fields.Length;
        for (int index = length; index < length + source.Length; ++index)
        {
          ParameterInfo parameterInfo = source[index - length];
          sourceArray[index] = new ArgumentReference(parameterInfo.ParameterType);
        }
      }
      else
        sourceArray = new ArgumentReference[fields.Length];
      for (int index = 0; index < fields.Length; ++index)
        sourceArray[index] = new ArgumentReference(fields[index].Reference.FieldType);
      ConstructorEmitter constructor = emitter.CreateConstructor(sourceArray);
      if (source != null && source.Length != 0)
      {
        ParameterInfo member = ((IEnumerable<ParameterInfo>) source).Last<ParameterInfo>();
        if (member.ParameterType.IsArray && member.HasAttribute<ParamArrayAttribute>())
          constructor.ConstructorBuilder.DefineParameter(sourceArray.Length, ParameterAttributes.None, member.Name).SetCustomAttribute(AttributeUtil.CreateBuilder<ParamArrayAttribute>());
      }
      for (int index = 0; index < fields.Length; ++index)
        constructor.CodeBuilder.AddStatement((Statement) new AssignStatement((Reference) fields[index], sourceArray[index].ToExpression()));
      if (baseConstructor != (ConstructorInfo) null)
      {
        ArgumentReference[] destinationArray = new ArgumentReference[source.Length];
        Array.Copy((Array) sourceArray, fields.Length, (Array) destinationArray, 0, source.Length);
        constructor.CodeBuilder.InvokeBaseConstructor(baseConstructor, destinationArray);
      }
      else
        constructor.CodeBuilder.InvokeBaseConstructor();
      constructor.CodeBuilder.AddStatement((Statement) new ReturnStatement());
    }

    private ConstructorEmitter GenerateStaticConstructor(ClassEmitter emitter) => emitter.CreateTypeConstructor();

    private bool IsConstructorVisible(ConstructorInfo constructor) => constructor.IsPublic || constructor.IsFamily || constructor.IsFamilyOrAssembly;

    private IEnumerable<Type> GetTypeImplementerMapping(
      Type[] interfaces,
      Type proxyTargetType,
      out IEnumerable<IEmissionContributor> contributors,
      IDesignatingScope designatingScope)
    {
      IDictionary<Type, IEmissionContributor> dictionary = (IDictionary<Type, IEmissionContributor>) new Dictionary<Type, IEmissionContributor>();
      ICollection<Type> allInterfaces1 = proxyTargetType.GetAllInterfaces();
      ICollection<Type> allInterfaces2 = TypeUtil.GetAllInterfaces(interfaces);
      IEmissionContributor emissionContributor = this.AddMapping(dictionary, proxyTargetType, allInterfaces1, allInterfaces2, designatingScope);
      NetworkInterfaceProxyContributor implementer = new NetworkInterfaceProxyContributor(designatingScope, this._module.DynamicAssemblyName);
      foreach (Type type in (IEnumerable<Type>) allInterfaces2)
      {
        if (!dictionary.ContainsKey(type))
        {
          implementer.AddInterfaceToProxy(type);
          this.AddMappingUnsafe(type, (IEmissionContributor) implementer, dictionary);
        }
      }
      contributors = (IEnumerable<IEmissionContributor>) new List<IEmissionContributor>()
      {
        emissionContributor,
        (IEmissionContributor) implementer
      };
      return (IEnumerable<Type>) dictionary.Keys;
    }

    private IEmissionContributor AddMapping(
      IDictionary<Type, IEmissionContributor> interfaceTypeImplementerMapping,
      Type proxyTargetType,
      ICollection<Type> targetInterfaces,
      ICollection<Type> additionalInterfaces,
      IDesignatingScope designatingScope)
    {
      NetworkInterfaceProxyContributor implementer = new NetworkInterfaceProxyContributor(designatingScope, this._module.DynamicAssemblyName);
      foreach (Type allInterface in (IEnumerable<Type>) this._targetType.GetAllInterfaces())
      {
        implementer.AddInterfaceToProxy(allInterface);
        this.AddMappingUnsafe(allInterface, (IEmissionContributor) implementer, interfaceTypeImplementerMapping);
      }
      return (IEmissionContributor) implementer;
    }

    private void AddMappingUnsafe(
      Type interfaceType,
      IEmissionContributor implementer,
      IDictionary<Type, IEmissionContributor> mapping)
    {
      mapping.Add(interfaceType, implementer);
    }

    private Type ObtainProxyType(EmissionCacheKey cacheKey, Type proxyBaseType)
    {
      this._module.CacheLock.EnterUpgradeableReadLock();
      try
      {
        Type fromCache1 = this._module.GetFromCache(cacheKey);
        if (fromCache1 != (Type) null)
          return fromCache1;
        this._module.CacheLock.EnterWriteLock();
        try
        {
          Type fromCache2 = this._module.GetFromCache(cacheKey);
          if (fromCache2 != (Type) null)
            return fromCache2;
          Type type = this.GenerateType(this._module.DesignatingScope.GenerateDesignation("Weave.Proxies." + this._targetType.Name + "Proxy"), proxyBaseType, this._module.DesignatingScope.GenerateChildScope("Weave.Proxies.ChildScope"));
          this._module.RegisterInCache(cacheKey, type);
          return type;
        }
        finally
        {
          this._module.CacheLock.ExitWriteLock();
        }
      }
      finally
      {
        this._module.CacheLock.ExitUpgradeableReadLock();
      }
    }

    protected override void OnDispose()
    {
      this._module = (EmissionModule) null;
      this._emissionOptions = (EmissionProxyOptions) null;
      this._targetType = (Type) null;
    }
  }
}
