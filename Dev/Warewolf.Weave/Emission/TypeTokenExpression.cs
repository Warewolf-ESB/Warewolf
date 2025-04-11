// Decompiled with JetBrains decompiler
// Type: System.Emission.TypeTokenExpression
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Emission.Emitters;
using System.Reflection;
using System.Reflection.Emit;

namespace System.Emission
{
  internal class TypeTokenExpression : Expression
  {
    public static readonly MethodInfo GetTypeFromHandle = typeof (Type).GetMethod(nameof (GetTypeFromHandle));
    private readonly Type type;

    public TypeTokenExpression(Type type) => this.type = type;

    public override void Emit(IEmissionMemberEmitter member, ILGenerator gen)
    {
      gen.Emit(OpCodes.Ldtoken, this.type);
      gen.Emit(OpCodes.Call, TypeTokenExpression.GetTypeFromHandle);
    }
  }
}
