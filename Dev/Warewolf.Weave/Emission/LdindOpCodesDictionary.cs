// Decompiled with JetBrains decompiler
// Type: System.Emission.LdindOpCodesDictionary
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Collections.Generic;
using System.Reflection.Emit;

namespace System.Emission
{
  internal sealed class LdindOpCodesDictionary : Dictionary<Type, OpCode>
  {
    private static readonly LdindOpCodesDictionary dict = new LdindOpCodesDictionary();
    private static readonly OpCode emptyOpCode = new OpCode();

    public new OpCode this[Type type] => this.ContainsKey(type) ? base[type] : LdindOpCodesDictionary.EmptyOpCode;

    public static OpCode EmptyOpCode => LdindOpCodesDictionary.emptyOpCode;

    public static LdindOpCodesDictionary Instance => LdindOpCodesDictionary.dict;

    private LdindOpCodesDictionary()
    {
      this.Add(typeof (bool), OpCodes.Ldind_I1);
      this.Add(typeof (char), OpCodes.Ldind_I2);
      this.Add(typeof (sbyte), OpCodes.Ldind_I1);
      this.Add(typeof (short), OpCodes.Ldind_I2);
      this.Add(typeof (int), OpCodes.Ldind_I4);
      this.Add(typeof (long), OpCodes.Ldind_I8);
      this.Add(typeof (float), OpCodes.Ldind_R4);
      this.Add(typeof (double), OpCodes.Ldind_R8);
      this.Add(typeof (byte), OpCodes.Ldind_U1);
      this.Add(typeof (ushort), OpCodes.Ldind_U2);
      this.Add(typeof (uint), OpCodes.Ldind_U4);
      this.Add(typeof (ulong), OpCodes.Ldind_I8);
    }
  }
}
