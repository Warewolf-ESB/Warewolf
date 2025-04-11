// Decompiled with JetBrains decompiler
// Type: System.Emission.Emitters.EventEmitter
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Reflection;
using System.Reflection.Emit;

namespace System.Emission.Emitters
{
  internal sealed class EventEmitter : IEmissionMemberEmitter
  {
    private EventBuilder _eventBuilder;
    private Type _type;
    private AbstractTypeEmitter _typeEmitter;
    private MethodEmitter _addMethod;
    private MethodEmitter _removeMethod;

    public MemberInfo Member => (MemberInfo) null;

    public Type ReturnType => this._type;

    public EventEmitter(
      AbstractTypeEmitter typeEmitter,
      string name,
      EventAttributes attributes,
      Type type)
    {
      if (name == null)
        throw new ArgumentNullException(nameof (name));
      if (type == (Type) null)
        throw new ArgumentNullException(nameof (type));
      this._typeEmitter = typeEmitter;
      this._type = type;
      this._eventBuilder = typeEmitter.TypeBuilder.DefineEvent(name, attributes, type);
    }

    public MethodEmitter CreateAddMethod(
      string addMethodName,
      MethodAttributes attributes,
      MethodInfo methodToOverride)
    {
      if (this._addMethod != null)
        throw new InvalidOperationException("An add method exists");
      this._addMethod = new MethodEmitter(this._typeEmitter, addMethodName, attributes, methodToOverride);
      return this._addMethod;
    }

    public MethodEmitter CreateRemoveMethod(
      string removeMethodName,
      MethodAttributes attributes,
      MethodInfo methodToOverride)
    {
      if (this._removeMethod != null)
        throw new InvalidOperationException("A remove method exists");
      this._removeMethod = new MethodEmitter(this._typeEmitter, removeMethodName, attributes, methodToOverride);
      return this._removeMethod;
    }

    public void Generate()
    {
      if (this._addMethod == null)
        throw new InvalidOperationException("Event add method was not created");
      if (this._removeMethod == null)
        throw new InvalidOperationException("Event remove method was not created");
      this._addMethod.Generate();
      this._eventBuilder.SetAddOnMethod(this._addMethod.MethodBuilder);
      this._removeMethod.Generate();
      this._eventBuilder.SetRemoveOnMethod(this._removeMethod.MethodBuilder);
    }

    public void EnsureValidCodeBlock()
    {
      this._addMethod.EnsureValidCodeBlock();
      this._removeMethod.EnsureValidCodeBlock();
    }
  }
}
