// Decompiled with JetBrains decompiler
// Type: System.Emission.MethodInvocationExpression
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Diagnostics;
using System.Emission.Emitters;
using System.Reflection;
using System.Reflection.Emit;

namespace System.Emission
{
  internal sealed class MethodInvocationExpression : Expression
  {
    private Expression[] _args;
    private MethodInfo _method;
    private Reference _owner;
    private bool _virtualCall;

    public bool VirtualCall
    {
      get => this._virtualCall;
      set => this._virtualCall = value;
    }

    public MethodInvocationExpression(MethodInfo method, params Expression[] args)
      : this((Reference) SelfReference.Self, method, args)
    {
    }

    public MethodInvocationExpression(MethodEmitter method, params Expression[] args)
      : this((Reference) SelfReference.Self, (MethodInfo) method.MethodBuilder, args)
    {
    }

    public MethodInvocationExpression(
      Reference owner,
      MethodEmitter method,
      params Expression[] args)
      : this(owner, (MethodInfo) method.MethodBuilder, args)
    {
    }

    public MethodInvocationExpression(Reference owner, MethodInfo method, params Expression[] args)
    {
      if (method == (MethodInfo) null)
        Debugger.Break();
      this._owner = owner;
      this._method = method;
      this._args = args;
    }

    public override void Emit(IEmissionMemberEmitter member, ILGenerator gen)
    {
      ArgumentsUtil.EmitLoadOwnerAndReference(this._owner, gen);
      foreach (Expression expression in this._args)
        expression.Emit(member, gen);
      if (this._virtualCall)
        gen.Emit(OpCodes.Callvirt, this._method);
      else
        gen.Emit(OpCodes.Call, this._method);
    }
  }
}
