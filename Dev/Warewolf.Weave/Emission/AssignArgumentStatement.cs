// Decompiled with JetBrains decompiler
// Type: System.Emission.AssignArgumentStatement
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Emission.Emitters;
using System.Reflection.Emit;

namespace System.Emission
{
  internal class AssignArgumentStatement : Statement
  {
    private ArgumentReference _argument;
    private Expression _expression;

    public AssignArgumentStatement(ArgumentReference argument, Expression expression)
    {
      this._argument = argument;
      this._expression = expression;
    }

    public override void Emit(IEmissionMemberEmitter member, ILGenerator gen)
    {
      ArgumentsUtil.EmitLoadOwnerAndReference((Reference) this._argument, gen);
      this._expression.Emit(member, gen);
    }
  }
}
