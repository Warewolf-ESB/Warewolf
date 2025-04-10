// Decompiled with JetBrains decompiler
// Type: System.Emission.StindOpCodesDictionary
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Collections.Generic;
using System.Reflection.Emit;

namespace System.Emission
{
  internal sealed class StindOpCodesDictionary : Dictionary<Type, OpCode>
  {
    private static readonly StindOpCodesDictionary dict = new StindOpCodesDictionary();
    private static readonly OpCode emptyOpCode = new OpCode();

    public new OpCode this[Type type] => this.ContainsKey(type) ? base[type] : StindOpCodesDictionary.EmptyOpCode;

    public static OpCode EmptyOpCode => StindOpCodesDictionary.emptyOpCode;

    public static StindOpCodesDictionary Instance => StindOpCodesDictionary.dict;

    private StindOpCodesDictionary()
    {
      this.Add(typeof (bool), OpCodes.Stind_I1);
      this.Add(typeof (char), OpCodes.Stind_I2);
      this.Add(typeof (sbyte), OpCodes.Stind_I1);
      this.Add(typeof (short), OpCodes.Stind_I2);
      this.Add(typeof (int), OpCodes.Stind_I4);
      this.Add(typeof (long), OpCodes.Stind_I8);
      this.Add(typeof (float), OpCodes.Stind_R4);
      this.Add(typeof (double), OpCodes.Stind_R8);
      this.Add(typeof (byte), OpCodes.Stind_I1);
      this.Add(typeof (ushort), OpCodes.Stind_I2);
      this.Add(typeof (uint), OpCodes.Stind_I4);
      this.Add(typeof (ulong), OpCodes.Stind_I8);
    }
  }
}
