// Decompiled with JetBrains decompiler
// Type: System.Emission.ArgumentReference
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Diagnostics;
using System.Reflection.Emit;

namespace System.Emission
{
  [DebuggerDisplay("argument {Type}")]
  internal sealed class ArgumentReference : TypeReference
  {
    private int _position;

    internal int Position
    {
      get => this._position;
      set => this._position = value;
    }

    public ArgumentReference(Type argumentType)
      : base(argumentType)
    {
      this._position = -1;
    }

    public ArgumentReference(Type argumentType, int position)
      : base(argumentType)
    {
      this._position = position;
    }

    public override void LoadAddressOfReference(ILGenerator gen) => throw new NotSupportedException();

    public override void LoadReference(ILGenerator gen)
    {
      if (this._position == -1)
        throw new InvalidOperationException("ArgumentReference unitialized");
      switch (this._position)
      {
        case 0:
          gen.Emit(OpCodes.Ldarg_0);
          break;
        case 1:
          gen.Emit(OpCodes.Ldarg_1);
          break;
        case 2:
          gen.Emit(OpCodes.Ldarg_2);
          break;
        case 3:
          gen.Emit(OpCodes.Ldarg_3);
          break;
        default:
          gen.Emit(OpCodes.Ldarg_S, this.Position);
          break;
      }
    }

    public override void StoreReference(ILGenerator gen)
    {
      if (this._position == -1)
        throw new InvalidOperationException("ArgumentReference unitialized");
      gen.Emit(OpCodes.Starg, this.Position);
    }
  }
}
