// Decompiled with JetBrains decompiler
// Type: System.Emission.Emitters.AbstractTypeEmitter
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Reflection.Emit;

namespace System.Emission.Emitters
{
  internal abstract class AbstractTypeEmitter
  {
    private const MethodAttributes DefaultAttributes = MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig;
    private Collection<ConstructorEmitter> _constructors;
    private Collection<EventEmitter> _events;
    private IDictionary<string, FieldReference> _fields = (IDictionary<string, FieldReference>) new Dictionary<string, FieldReference>((IEqualityComparer<string>) StringComparer.OrdinalIgnoreCase);
    private Collection<MethodEmitter> _methods;
    private Dictionary<string, GenericTypeParameterBuilder> _nameGenericTypeMapping;
    private Collection<NestedClassEmitter> _nested;
    private Collection<PropertyEmitter> _properties;
    private TypeBuilder _typeBuilder;
    private GenericTypeParameterBuilder[] _genericTypeParams;
    private string _dynamicAssemblyName;
    private TypeConstructorEmitter _classConstructor;

    public Type BaseType => !this._typeBuilder.IsInterface ? this._typeBuilder.BaseType : throw new InvalidOperationException("This emitter represents an interface; interfaces have no base types.");

    public TypeConstructorEmitter ClassConstructor => this._classConstructor;

    public Collection<ConstructorEmitter> Constructors => this._constructors;

    public GenericTypeParameterBuilder[] GenericTypeParams => this._genericTypeParams;

    public Collection<NestedClassEmitter> Nested => this._nested;

    public TypeBuilder TypeBuilder => this._typeBuilder;

    public string DynamicAssemblyName => this._dynamicAssemblyName;

    protected AbstractTypeEmitter(TypeBuilder typeBuilder, string dynamicAssemblyName)
    {
      this._typeBuilder = typeBuilder;
      this._dynamicAssemblyName = dynamicAssemblyName;
      this._nested = new Collection<NestedClassEmitter>();
      this._methods = new Collection<MethodEmitter>();
      this._constructors = new Collection<ConstructorEmitter>();
      this._properties = new Collection<PropertyEmitter>();
      this._events = new Collection<EventEmitter>();
      this._nameGenericTypeMapping = new Dictionary<string, GenericTypeParameterBuilder>();
    }

    public virtual Type BuildType()
    {
      this.EnsureBuildersAreInAValidState();
      Type type = this.CreateType(this._typeBuilder);
      foreach (AbstractTypeEmitter abstractTypeEmitter in this._nested)
        abstractTypeEmitter.BuildType();
      return type;
    }

    protected Type CreateType(TypeBuilder type) => type.CreateType();

    public ConstructorEmitter CreateConstructor(params ArgumentReference[] arguments)
    {
      if (this._typeBuilder.IsInterface)
        throw new InvalidOperationException("Interfaces cannot have constructors.");
      ConstructorEmitter constructor = new ConstructorEmitter(this, arguments);
      this._constructors.Add(constructor);
      return constructor;
    }

    public void CreateDefaultConstructor()
    {
      if (this._typeBuilder.IsInterface)
        throw new InvalidOperationException("Interfaces cannot have constructors.");
      this._constructors.Add(new ConstructorEmitter(this, Array.Empty<ArgumentReference>()));
    }

    public EventEmitter CreateEvent(string name, EventAttributes atts, Type type)
    {
      EventEmitter eventEmitter = new EventEmitter(this, name, atts, type);
      this._events.Add(eventEmitter);
      return eventEmitter;
    }

    public FieldReference CreateField(string name, Type fieldType) => this.CreateField(name, fieldType, FieldAttributes.Public);

    public FieldReference CreateField(string name, Type fieldType, FieldAttributes atts)
    {
      FieldReference field = new FieldReference(this._typeBuilder.DefineField(name, fieldType, atts));
      this._fields[name] = field;
      return field;
    }

    public MethodEmitter CreateMethod(
      string name,
      MethodAttributes attrs,
      Type returnType,
      params Type[] argumentTypes)
    {
      MethodEmitter method = new MethodEmitter(this, name, attrs, returnType, argumentTypes ?? Type.EmptyTypes);
      this._methods.Add(method);
      return method;
    }

    public MethodEmitter CreateMethod(string name, Type returnType, params Type[] parameterTypes) => this.CreateMethod(name, MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig, returnType, parameterTypes);

    public MethodEmitter CreateMethod(string name, MethodInfo methodToUseAsATemplate) => this.CreateMethod(name, MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig, methodToUseAsATemplate);

    public MethodEmitter CreateMethod(
      string name,
      MethodAttributes attributes,
      MethodInfo methodToUseAsATemplate)
    {
      MethodEmitter method = new MethodEmitter(this, name, attributes, methodToUseAsATemplate);
      this._methods.Add(method);
      return method;
    }

    public PropertyEmitter CreateProperty(
      string name,
      PropertyAttributes attributes,
      Type propertyType,
      Type[] arguments)
    {
      PropertyEmitter property = new PropertyEmitter(this, name, attributes, propertyType, arguments);
      this._properties.Add(property);
      return property;
    }

    public FieldReference CreateStaticField(string name, Type fieldType) => this.CreateStaticField(name, fieldType, FieldAttributes.Public);

    public FieldReference CreateStaticField(string name, Type fieldType, FieldAttributes atts) => this.CreateField(name, fieldType, atts | FieldAttributes.Static);

    public ConstructorEmitter CreateTypeConstructor()
    {
      TypeConstructorEmitter typeConstructor = new TypeConstructorEmitter(this);
      this._constructors.Add((ConstructorEmitter) typeConstructor);
      this._classConstructor = typeConstructor;
      return (ConstructorEmitter) typeConstructor;
    }

    public void AddCustomAttributes(EmissionProxyOptions options)
    {
      foreach (CustomAttributeBuilder additionalAttribute in options.AdditionalAttributes)
        this._typeBuilder.SetCustomAttribute(additionalAttribute);
    }

    public void DefineCustomAttribute(CustomAttributeBuilder attribute) => this._typeBuilder.SetCustomAttribute(attribute);

    public void DefineCustomAttribute<TAttribute>(object[] constructorArguments) where TAttribute : Attribute => this._typeBuilder.SetCustomAttribute(AttributeUtil.CreateBuilder(typeof (TAttribute), constructorArguments));

    public void DefineCustomAttribute<TAttribute>() where TAttribute : Attribute, new() => this._typeBuilder.SetCustomAttribute(AttributeUtil.CreateBuilder<TAttribute>());

    public void DefineCustomAttributeFor<TAttribute>(FieldReference field) where TAttribute : Attribute, new()
    {
      CustomAttributeBuilder builder = AttributeUtil.CreateBuilder<TAttribute>();
      FieldBuilder fieldbuilder = field.Fieldbuilder;
      if ((FieldInfo) fieldbuilder == (FieldInfo) null)
        throw new ArgumentException("Invalid field reference.This reference does not point to field on type being generated", nameof (field));
      fieldbuilder.SetCustomAttribute(builder);
    }

    public IEnumerable<FieldReference> GetAllFields() => (IEnumerable<FieldReference>) this._fields.Values;

    public FieldReference GetField(string name)
    {
      if (string.IsNullOrEmpty(name))
        return (FieldReference) null;
      FieldReference field;
      this._fields.TryGetValue(name, out field);
      return field;
    }

    public Type GetGenericArgument(string genericArgumentName) => (Type) this._nameGenericTypeMapping[genericArgumentName];

    public Type[] GetGenericArgumentsFor(Type genericType)
    {
      List<Type> typeList = new List<Type>();
      foreach (Type genericArgument in genericType.GetGenericArguments())
      {
        if (genericArgument.IsGenericParameter)
          typeList.Add((Type) this._nameGenericTypeMapping[genericArgument.Name]);
        else
          typeList.Add(genericArgument);
      }
      return typeList.ToArray();
    }

    public Type[] GetGenericArgumentsFor(MethodInfo genericMethod)
    {
      List<Type> typeList = new List<Type>();
      foreach (Type genericArgument in genericMethod.GetGenericArguments())
        typeList.Add((Type) this._nameGenericTypeMapping[genericArgument.Name]);
      return typeList.ToArray();
    }

    public void SetGenericTypeParameters(
      GenericTypeParameterBuilder[] genericTypeParameterBuilders)
    {
      this._genericTypeParams = genericTypeParameterBuilders;
    }

    public void CopyGenericParametersFromMethod(MethodInfo methodToCopyGenericsFrom)
    {
      if (this._genericTypeParams != null)
        throw new InvalidOperationException("CopyGenericParametersFromMethod: cannot invoke me twice");
      this.SetGenericTypeParameters(GenericUtil.CopyGenericArguments(methodToCopyGenericsFrom, this._typeBuilder, this._nameGenericTypeMapping));
    }

    protected virtual void EnsureBuildersAreInAValidState()
    {
      if (!this._typeBuilder.IsInterface && this._constructors.Count == 0)
        this.CreateDefaultConstructor();
      foreach (PropertyEmitter property in this._properties)
      {
        property.EnsureValidCodeBlock();
        property.Generate();
      }
      foreach (EventEmitter eventEmitter in this._events)
      {
        eventEmitter.EnsureValidCodeBlock();
        eventEmitter.Generate();
      }
      foreach (ConstructorEmitter constructor in this._constructors)
      {
        constructor.EnsureValidCodeBlock();
        constructor.Generate();
      }
      foreach (MethodEmitter method in this._methods)
      {
        method.EnsureValidCodeBlock();
        method.Generate();
      }
    }
  }
}
