// Decompiled with JetBrains decompiler
// Type: System.Emission.LdcOpCodesDictionary
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Collections.Generic;
using System.Reflection.Emit;

namespace System.Emission
{
  internal sealed class LdcOpCodesDictionary : Dictionary<Type, OpCode>
  {
    private static readonly LdcOpCodesDictionary dict = new LdcOpCodesDictionary();
    private static readonly OpCode emptyOpCode = new OpCode();

    public new OpCode this[Type type] => this.ContainsKey(type) ? base[type] : LdcOpCodesDictionary.EmptyOpCode;

    public static OpCode EmptyOpCode => LdcOpCodesDictionary.emptyOpCode;

    public static LdcOpCodesDictionary Instance => LdcOpCodesDictionary.dict;

    private LdcOpCodesDictionary()
    {
      this.Add(typeof (bool), OpCodes.Ldc_I4);
      this.Add(typeof (char), OpCodes.Ldc_I4);
      this.Add(typeof (sbyte), OpCodes.Ldc_I4);
      this.Add(typeof (short), OpCodes.Ldc_I4);
      this.Add(typeof (int), OpCodes.Ldc_I4);
      this.Add(typeof (long), OpCodes.Ldc_I8);
      this.Add(typeof (float), OpCodes.Ldc_R4);
      this.Add(typeof (double), OpCodes.Ldc_R8);
      this.Add(typeof (byte), OpCodes.Ldc_I4_0);
      this.Add(typeof (ushort), OpCodes.Ldc_I4_0);
      this.Add(typeof (uint), OpCodes.Ldc_I4_0);
      this.Add(typeof (ulong), OpCodes.Ldc_I4_0);
    }
  }
}
