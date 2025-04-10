// Decompiled with JetBrains decompiler
// Type: System.Emission.OpCodeUtil
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Reflection.Emit;

namespace System.Emission
{
  internal abstract class OpCodeUtil
  {
    public static void EmitLoadIndirectOpCodeForType(ILGenerator gen, Type type)
    {
      if (type.IsEnum)
      {
        OpCodeUtil.EmitLoadIndirectOpCodeForType(gen, OpCodeUtil.GetUnderlyingTypeOfEnum(type));
      }
      else
      {
        if (type.IsByRef)
          throw new NotSupportedException("Cannot load ByRef values");
        if (type.IsPrimitive && type != typeof (IntPtr))
        {
          OpCode opcode = LdindOpCodesDictionary.Instance[type];
          if (opcode == LdindOpCodesDictionary.EmptyOpCode)
            throw new ArgumentException("Type " + (object) type + " could not be converted to a OpCode");
          gen.Emit(opcode);
        }
        else if (type.IsValueType)
          gen.Emit(OpCodes.Ldobj, type);
        else if (type.IsGenericParameter)
          gen.Emit(OpCodes.Ldobj, type);
        else
          gen.Emit(OpCodes.Ldind_Ref);
      }
    }

    public static void EmitLoadOpCodeForConstantValue(ILGenerator gen, object value)
    {
      switch (value)
      {
        case string _:
          gen.Emit(OpCodes.Ldstr, value.ToString());
          break;
        case int num:
          OpCode opcode1 = LdcOpCodesDictionary.Instance[value.GetType()];
          gen.Emit(opcode1, num);
          break;
        case bool _:
          OpCode opcode2 = LdcOpCodesDictionary.Instance[value.GetType()];
          gen.Emit(opcode2, Convert.ToInt32(value));
          break;
        default:
          throw new NotSupportedException();
      }
    }

    public static void EmitLoadOpCodeForDefaultValueOfType(ILGenerator gen, Type type)
    {
      if (type.IsPrimitive)
      {
        OpCode opcode = LdcOpCodesDictionary.Instance[type];
        switch (opcode.StackBehaviourPush)
        {
          case StackBehaviour.Pushi:
            gen.Emit(opcode, 0);
            if (!OpCodeUtil.Is64BitTypeLoadedAsInt32(type))
              break;
            gen.Emit(OpCodes.Conv_I8);
            break;
          case StackBehaviour.Pushi8:
            gen.Emit(opcode, 0L);
            break;
          case StackBehaviour.Pushr4:
            gen.Emit(opcode, 0.0f);
            break;
          case StackBehaviour.Pushr8:
            gen.Emit(opcode, 0.0);
            break;
          default:
            throw new NotSupportedException();
        }
      }
      else
        gen.Emit(OpCodes.Ldnull);
    }

    public static void EmitStoreIndirectOpCodeForType(ILGenerator gen, Type type)
    {
      if (type.IsEnum)
      {
        OpCodeUtil.EmitStoreIndirectOpCodeForType(gen, OpCodeUtil.GetUnderlyingTypeOfEnum(type));
      }
      else
      {
        if (type.IsByRef)
          throw new NotSupportedException("Cannot store ByRef values");
        if (type.IsPrimitive && type != typeof (IntPtr))
        {
          OpCode opCode = StindOpCodesDictionary.Instance[type];
          if (object.Equals((object) opCode, (object) StindOpCodesDictionary.EmptyOpCode))
            throw new ArgumentException("Type " + (object) type + " could not be converted to a OpCode");
          gen.Emit(opCode);
        }
        else if (type.IsValueType)
          gen.Emit(OpCodes.Stobj, type);
        else if (type.IsGenericParameter)
          gen.Emit(OpCodes.Stobj, type);
        else
          gen.Emit(OpCodes.Stind_Ref);
      }
    }

    private static Type GetUnderlyingTypeOfEnum(Type enumType)
    {
      switch (((Enum) Activator.CreateInstance(enumType)).GetTypeCode())
      {
        case TypeCode.SByte:
          return typeof (sbyte);
        case TypeCode.Byte:
          return typeof (byte);
        case TypeCode.Int16:
          return typeof (short);
        case TypeCode.UInt16:
          return typeof (ushort);
        case TypeCode.Int32:
          return typeof (int);
        case TypeCode.UInt32:
          return typeof (uint);
        case TypeCode.Int64:
          return typeof (long);
        case TypeCode.UInt64:
          return typeof (ulong);
        default:
          throw new NotSupportedException();
      }
    }

    private static bool Is64BitTypeLoadedAsInt32(Type type) => type == typeof (long) || type == typeof (ulong);
  }
}
