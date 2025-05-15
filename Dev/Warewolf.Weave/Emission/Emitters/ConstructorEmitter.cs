// Decompiled with JetBrains decompiler
// Type: System.Emission.Emitters.ConstructorEmitter
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Reflection;
using System.Reflection.Emit;

namespace System.Emission.Emitters
{
  internal class ConstructorEmitter : IEmissionMemberEmitter
  {
    private ConstructorBuilder _builder;
    private AbstractTypeEmitter _mainType;
    private ConstructorCodeBuilder _codeBuilder;

    private bool ImplementedByRuntime => (this._builder.GetMethodImplementationFlags() & MethodImplAttributes.CodeTypeMask) != 0;

    public virtual ConstructorCodeBuilder CodeBuilder
    {
      get
      {
        if (this._codeBuilder == null)
          this._codeBuilder = new ConstructorCodeBuilder(this._mainType.BaseType, this._builder.GetILGenerator());
        return this._codeBuilder;
      }
    }

    public ConstructorBuilder ConstructorBuilder => this._builder;

    public MemberInfo Member => (MemberInfo) this._builder;

    public Type ReturnType => typeof (void);

    protected ConstructorEmitter(AbstractTypeEmitter maintype, ConstructorBuilder builder)
    {
      this._mainType = maintype;
      this._builder = builder;
    }

    internal ConstructorEmitter(AbstractTypeEmitter maintype, params ArgumentReference[] arguments)
    {
      this._mainType = maintype;
      Type[] parameterTypes = ArgumentsUtil.InitializeAndConvert(arguments);
      this._builder = maintype.TypeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, parameterTypes);
    }

    public virtual void Generate()
    {
      if (this.ImplementedByRuntime)
        return;
      this.CodeBuilder.Generate((IEmissionMemberEmitter) this, this._builder.GetILGenerator());
    }

    public virtual void EnsureValidCodeBlock()
    {
      if (this.ImplementedByRuntime || !this.CodeBuilder.IsEmpty)
        return;
      this.CodeBuilder.InvokeBaseConstructor();
      this.CodeBuilder.AddStatement((Statement) new ReturnStatement());
    }
  }
}
