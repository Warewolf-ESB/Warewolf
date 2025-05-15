// Decompiled with JetBrains decompiler
// Type: System.Emission.ReturnStatement
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Emission.Emitters;
using System.Reflection.Emit;

namespace System.Emission
{
  internal class ReturnStatement : Statement
  {
    private Expression _expression;
    private Reference _reference;

    public ReturnStatement()
    {
    }

    public ReturnStatement(Reference reference) => this._reference = reference;

    public ReturnStatement(Expression expression) => this._expression = expression;

    public override void Emit(IEmissionMemberEmitter member, ILGenerator gen)
    {
      if (this._reference != null)
        ArgumentsUtil.EmitLoadOwnerAndReference(this._reference, gen);
      else if (this._expression != null)
        this._expression.Emit(member, gen);
      else if (member.ReturnType != typeof (void))
        OpCodeUtil.EmitLoadOpCodeForDefaultValueOfType(gen, member.ReturnType);
      gen.Emit(OpCodes.Ret);
    }
  }
}
