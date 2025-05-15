// Decompiled with JetBrains decompiler
// Type: System.__IByteReaderBaseExtensions
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

namespace System
{
  public static class __IByteReaderBaseExtensions
  {
    public static byte[] ReadByteArray(this IByteReaderBase reader)
    {
      int amount = reader.ReadInt32();
      return amount == 0 ? (byte[]) null : reader.ReadBytes(amount);
    }

    public static bool[] ReadBooleanArray(this IByteReaderBase reader)
    {
      int length = reader.ReadInt32();
      if (length == 0)
        return (bool[]) null;
      bool[] flagArray = new bool[length];
      for (int index = 0; index < flagArray.Length; ++index)
        flagArray[index] = reader.ReadBoolean();
      return flagArray;
    }

    public static string[] ReadStringArray(this IByteReaderBase reader)
    {
      int length = reader.ReadInt32();
      if (length == 0)
        return (string[]) null;
      string[] strArray = new string[length];
      for (int index = 0; index < strArray.Length; ++index)
        strArray[index] = reader.ReadString();
      return strArray;
    }

    public static sbyte[] ReadSByteArray(this IByteReaderBase reader)
    {
      int length = reader.ReadInt32();
      if (length == 0)
        return (sbyte[]) null;
      sbyte[] numArray = new sbyte[length];
      for (int index = 0; index < numArray.Length; ++index)
        numArray[index] = reader.ReadSByte();
      return numArray;
    }

    public static short[] ReadInt16Array(this IByteReaderBase reader)
    {
      int length = reader.ReadInt32();
      if (length == 0)
        return (short[]) null;
      short[] numArray = new short[length];
      for (int index = 0; index < numArray.Length; ++index)
        numArray[index] = reader.ReadInt16();
      return numArray;
    }

    public static int[] ReadInt32Array(this IByteReaderBase reader)
    {
      int length = reader.ReadInt32();
      if (length == 0)
        return (int[]) null;
      int[] numArray = new int[length];
      for (int index = 0; index < numArray.Length; ++index)
        numArray[index] = reader.ReadInt32();
      return numArray;
    }

    public static long[] ReadInt64Array(this IByteReaderBase reader)
    {
      int length = reader.ReadInt32();
      if (length == 0)
        return (long[]) null;
      long[] numArray = new long[length];
      for (int index = 0; index < numArray.Length; ++index)
        numArray[index] = reader.ReadInt64();
      return numArray;
    }

    public static Decimal[] ReadDecimalArray(this IByteReaderBase reader)
    {
      int length = reader.ReadInt32();
      if (length == 0)
        return (Decimal[]) null;
      Decimal[] numArray = new Decimal[length];
      for (int index = 0; index < numArray.Length; ++index)
        numArray[index] = reader.ReadDecimal();
      return numArray;
    }

    public static ushort[] ReadUInt16Array(this IByteReaderBase reader)
    {
      int length = reader.ReadInt32();
      if (length == 0)
        return (ushort[]) null;
      ushort[] numArray = new ushort[length];
      for (int index = 0; index < numArray.Length; ++index)
        numArray[index] = reader.ReadUInt16();
      return numArray;
    }

    public static uint[] ReadUInt32Array(this IByteReaderBase reader)
    {
      int length = reader.ReadInt32();
      if (length == 0)
        return (uint[]) null;
      uint[] numArray = new uint[length];
      for (int index = 0; index < numArray.Length; ++index)
        numArray[index] = reader.ReadUInt32();
      return numArray;
    }

    public static ulong[] ReadUInt64Array(this IByteReaderBase reader)
    {
      int length = reader.ReadInt32();
      if (length == 0)
        return (ulong[]) null;
      ulong[] numArray = new ulong[length];
      for (int index = 0; index < numArray.Length; ++index)
        numArray[index] = reader.ReadUInt64();
      return numArray;
    }

    public static float[] ReadSingleArray(this IByteReaderBase reader)
    {
      int length = reader.ReadInt32();
      if (length == 0)
        return (float[]) null;
      float[] numArray = new float[length];
      for (int index = 0; index < numArray.Length; ++index)
        numArray[index] = reader.ReadSingle();
      return numArray;
    }

    public static double[] ReadDoubleArray(this IByteReaderBase reader)
    {
      int length = reader.ReadInt32();
      if (length == 0)
        return (double[]) null;
      double[] numArray = new double[length];
      for (int index = 0; index < numArray.Length; ++index)
        numArray[index] = reader.ReadDouble();
      return numArray;
    }
  }
}
