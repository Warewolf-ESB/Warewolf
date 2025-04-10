// Decompiled with JetBrains decompiler
// Type: System.Emission.ConstructorInvocationStatement
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Emission.Emitters;
using System.Reflection;
using System.Reflection.Emit;

namespace System.Emission
{
  internal class ConstructorInvocationStatement : Statement
  {
    private Expression[] _args;
    private ConstructorInfo _constructor;

    public ConstructorInvocationStatement(ConstructorInfo method, params Expression[] args)
    {
      if (method == (ConstructorInfo) null)
        throw new ArgumentNullException(nameof (method));
      if (args == null)
        throw new ArgumentNullException(nameof (args));
      this._constructor = method;
      this._args = args;
    }

    public override void Emit(IEmissionMemberEmitter member, ILGenerator gen)
    {
      gen.Emit(OpCodes.Ldarg_0);
      foreach (Expression expression in this._args)
        expression.Emit(member, gen);
      gen.Emit(OpCodes.Call, this._constructor);
    }
  }
}
