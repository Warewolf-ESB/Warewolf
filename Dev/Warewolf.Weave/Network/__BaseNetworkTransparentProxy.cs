// Decompiled with JetBrains decompiler
// Type: System.Network.__BaseNetworkTransparentProxy
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

using System.Collections.Generic;

namespace System.Network
{
  public class __BaseNetworkTransparentProxy
  {
    public virtual IByteReaderBase SendDuplexPacket(Packet packet) => throw new NotSupportedException();

    public virtual void SendSimplexPacket(Packet packet) => throw new NotSupportedException();

    public void Write(IByteWriterBase writer, bool[] array)
    {
      if (array == null)
      {
        writer.Write(0);
      }
      else
      {
        writer.Write(array.Length);
        for (int index = 0; index < array.Length; ++index)
          writer.Write(array[index]);
      }
    }

    public void Write(IByteWriterBase writer, string[] array)
    {
      if (array == null)
      {
        writer.Write(0);
      }
      else
      {
        writer.Write(array.Length);
        for (int index = 0; index < array.Length; ++index)
          writer.Write(array[index]);
      }
    }

    public void Write(IByteWriterBase writer, sbyte[] array)
    {
      if (array == null)
      {
        writer.Write(0);
      }
      else
      {
        writer.Write(array.Length);
        for (int index = 0; index < array.Length; ++index)
          writer.Write(array[index]);
      }
    }

    public void Write(IByteWriterBase writer, short[] array)
    {
      if (array == null)
      {
        writer.Write(0);
      }
      else
      {
        writer.Write(array.Length);
        for (int index = 0; index < array.Length; ++index)
          writer.Write(array[index]);
      }
    }

    public void Write(IByteWriterBase writer, int[] array)
    {
      if (array == null)
      {
        writer.Write(0);
      }
      else
      {
        writer.Write(array.Length);
        for (int index = 0; index < array.Length; ++index)
          writer.Write(array[index]);
      }
    }

    public void Write(IByteWriterBase writer, long[] array)
    {
      if (array == null)
      {
        writer.Write(0);
      }
      else
      {
        writer.Write(array.Length);
        for (int index = 0; index < array.Length; ++index)
          writer.Write(array[index]);
      }
    }

    public void Write(IByteWriterBase writer, Decimal[] array)
    {
      if (array == null)
      {
        writer.Write(0);
      }
      else
      {
        writer.Write(array.Length);
        for (int index = 0; index < array.Length; ++index)
          writer.Write(array[index]);
      }
    }

    public void Write(IByteWriterBase writer, ushort[] array)
    {
      if (array == null)
      {
        writer.Write(0);
      }
      else
      {
        writer.Write(array.Length);
        for (int index = 0; index < array.Length; ++index)
          writer.Write(array[index]);
      }
    }

    public void Write(IByteWriterBase writer, uint[] array)
    {
      if (array == null)
      {
        writer.Write(0);
      }
      else
      {
        writer.Write(array.Length);
        for (int index = 0; index < array.Length; ++index)
          writer.Write(array[index]);
      }
    }

    public void Write(IByteWriterBase writer, ulong[] array)
    {
      if (array == null)
      {
        writer.Write(0);
      }
      else
      {
        writer.Write(array.Length);
        for (int index = 0; index < array.Length; ++index)
          writer.Write(array[index]);
      }
    }

    public void Write(IByteWriterBase writer, float[] array)
    {
      if (array == null)
      {
        writer.Write(0);
      }
      else
      {
        writer.Write(array.Length);
        for (int index = 0; index < array.Length; ++index)
          writer.Write(array[index]);
      }
    }

    public void Write(IByteWriterBase writer, double[] array)
    {
      if (array == null)
      {
        writer.Write(0);
      }
      else
      {
        writer.Write(array.Length);
        for (int index = 0; index < array.Length; ++index)
          writer.Write(array[index]);
      }
    }

    public void Write(IByteWriterBase writer, IList<bool> array)
    {
      if (array == null)
      {
        writer.Write(0);
      }
      else
      {
        writer.Write(array.Count);
        for (int index = 0; index < array.Count; ++index)
          writer.Write(array[index]);
      }
    }

    public void Write(IByteWriterBase writer, IList<string> array)
    {
      if (array == null)
      {
        writer.Write(0);
      }
      else
      {
        writer.Write(array.Count);
        for (int index = 0; index < array.Count; ++index)
          writer.Write(array[index]);
      }
    }

    public void Write(IByteWriterBase writer, IList<sbyte> array)
    {
      if (array == null)
      {
        writer.Write(0);
      }
      else
      {
        writer.Write(array.Count);
        for (int index = 0; index < array.Count; ++index)
          writer.Write(array[index]);
      }
    }

    public void Write(IByteWriterBase writer, IList<short> array)
    {
      if (array == null)
      {
        writer.Write(0);
      }
      else
      {
        writer.Write(array.Count);
        for (int index = 0; index < array.Count; ++index)
          writer.Write(array[index]);
      }
    }

    public void Write(IByteWriterBase writer, IList<int> array)
    {
      if (array == null)
      {
        writer.Write(0);
      }
      else
      {
        writer.Write(array.Count);
        for (int index = 0; index < array.Count; ++index)
          writer.Write(array[index]);
      }
    }

    public void Write(IByteWriterBase writer, IList<long> array)
    {
      if (array == null)
      {
        writer.Write(0);
      }
      else
      {
        writer.Write(array.Count);
        for (int index = 0; index < array.Count; ++index)
          writer.Write(array[index]);
      }
    }

    public void Write(IByteWriterBase writer, IList<Decimal> array)
    {
      if (array == null)
      {
        writer.Write(0);
      }
      else
      {
        writer.Write(array.Count);
        for (int index = 0; index < array.Count; ++index)
          writer.Write(array[index]);
      }
    }

    public void Write(IByteWriterBase writer, IList<ushort> array)
    {
      if (array == null)
      {
        writer.Write(0);
      }
      else
      {
        writer.Write(array.Count);
        for (int index = 0; index < array.Count; ++index)
          writer.Write(array[index]);
      }
    }

    public void Write(IByteWriterBase writer, IList<uint> array)
    {
      if (array == null)
      {
        writer.Write(0);
      }
      else
      {
        writer.Write(array.Count);
        for (int index = 0; index < array.Count; ++index)
          writer.Write(array[index]);
      }
    }

    public void Write(IByteWriterBase writer, IList<ulong> array)
    {
      if (array == null)
      {
        writer.Write(0);
      }
      else
      {
        writer.Write(array.Count);
        for (int index = 0; index < array.Count; ++index)
          writer.Write(array[index]);
      }
    }

    public void Write(IByteWriterBase writer, IList<float> array)
    {
      if (array == null)
      {
        writer.Write(0);
      }
      else
      {
        writer.Write(array.Count);
        for (int index = 0; index < array.Count; ++index)
          writer.Write(array[index]);
      }
    }

    public void Write(IByteWriterBase writer, IList<double> array)
    {
      if (array == null)
      {
        writer.Write(0);
      }
      else
      {
        writer.Write(array.Count);
        for (int index = 0; index < array.Count; ++index)
          writer.Write(array[index]);
      }
    }

    public void Write<T>(IByteWriterBase writer, IList<T> array) where T : INetworkSerializable
    {
      if (array == null)
      {
        writer.Write(0);
      }
      else
      {
        writer.Write(array.Count);
        for (int index = 0; index < array.Count; ++index)
          array[index].Serialize(writer);
      }
    }

    public virtual void WriteUnhandled(IByteWriterBase writer, object unhandled) => throw new NotSupportedException();

    public virtual object ConstructUnhandled(Type type) => throw new NotSupportedException();

    public virtual object ReadUnhandled(IByteReaderBase reader, Type type) => throw new NotSupportedException();

    public byte[] ReadByteArray(IByteReaderBase reader)
    {
      int amount = reader.ReadInt32();
      return amount == 0 ? (byte[]) null : reader.ReadBytes(amount);
    }

    public bool[] ReadBooleanArray(IByteReaderBase reader)
    {
      int length = reader.ReadInt32();
      if (length == 0)
        return (bool[]) null;
      bool[] flagArray = new bool[length];
      for (int index = 0; index < flagArray.Length; ++index)
        flagArray[index] = reader.ReadBoolean();
      return flagArray;
    }

    public string[] ReadStringArray(IByteReaderBase reader)
    {
      int length = reader.ReadInt32();
      if (length == 0)
        return (string[]) null;
      string[] strArray = new string[length];
      for (int index = 0; index < strArray.Length; ++index)
        strArray[index] = reader.ReadString();
      return strArray;
    }

    public sbyte[] ReadSByteArray(IByteReaderBase reader)
    {
      int length = reader.ReadInt32();
      if (length == 0)
        return (sbyte[]) null;
      sbyte[] numArray = new sbyte[length];
      for (int index = 0; index < numArray.Length; ++index)
        numArray[index] = reader.ReadSByte();
      return numArray;
    }

    public short[] ReadInt16Array(IByteReaderBase reader)
    {
      int length = reader.ReadInt32();
      if (length == 0)
        return (short[]) null;
      short[] numArray = new short[length];
      for (int index = 0; index < numArray.Length; ++index)
        numArray[index] = reader.ReadInt16();
      return numArray;
    }

    public int[] ReadInt32Array(IByteReaderBase reader)
    {
      int length = reader.ReadInt32();
      if (length == 0)
        return (int[]) null;
      int[] numArray = new int[length];
      for (int index = 0; index < numArray.Length; ++index)
        numArray[index] = reader.ReadInt32();
      return numArray;
    }

    public long[] ReadInt64Array(IByteReaderBase reader)
    {
      int length = reader.ReadInt32();
      if (length == 0)
        return (long[]) null;
      long[] numArray = new long[length];
      for (int index = 0; index < numArray.Length; ++index)
        numArray[index] = reader.ReadInt64();
      return numArray;
    }

    public Decimal[] ReadDecimalArray(IByteReaderBase reader)
    {
      int length = reader.ReadInt32();
      if (length == 0)
        return (Decimal[]) null;
      Decimal[] numArray = new Decimal[length];
      for (int index = 0; index < numArray.Length; ++index)
        numArray[index] = reader.ReadDecimal();
      return numArray;
    }

    public ushort[] ReadUInt16Array(IByteReaderBase reader)
    {
      int length = reader.ReadInt32();
      if (length == 0)
        return (ushort[]) null;
      ushort[] numArray = new ushort[length];
      for (int index = 0; index < numArray.Length; ++index)
        numArray[index] = reader.ReadUInt16();
      return numArray;
    }

    public uint[] ReadUInt32Array(IByteReaderBase reader)
    {
      int length = reader.ReadInt32();
      if (length == 0)
        return (uint[]) null;
      uint[] numArray = new uint[length];
      for (int index = 0; index < numArray.Length; ++index)
        numArray[index] = reader.ReadUInt32();
      return numArray;
    }

    public ulong[] ReadUInt64Array(IByteReaderBase reader)
    {
      int length = reader.ReadInt32();
      if (length == 0)
        return (ulong[]) null;
      ulong[] numArray = new ulong[length];
      for (int index = 0; index < numArray.Length; ++index)
        numArray[index] = reader.ReadUInt64();
      return numArray;
    }

    public float[] ReadSingleArray(IByteReaderBase reader)
    {
      int length = reader.ReadInt32();
      if (length == 0)
        return (float[]) null;
      float[] numArray = new float[length];
      for (int index = 0; index < numArray.Length; ++index)
        numArray[index] = reader.ReadSingle();
      return numArray;
    }

    public double[] ReadDoubleArray(IByteReaderBase reader)
    {
      int length = reader.ReadInt32();
      if (length == 0)
        return (double[]) null;
      double[] numArray = new double[length];
      for (int index = 0; index < numArray.Length; ++index)
        numArray[index] = reader.ReadDouble();
      return numArray;
    }

    public IList<byte> ReadIListOfByte(IByteReaderBase reader)
    {
      int amount = reader.ReadInt32();
      return amount == 0 ? (IList<byte>) null : (IList<byte>) new List<byte>((IEnumerable<byte>) reader.ReadBytes(amount));
    }

    public IList<bool> ReadIListOfBoolean(IByteReaderBase reader)
    {
      int length = reader.ReadInt32();
      if (length == 0)
        return (IList<bool>) null;
      bool[] collection = new bool[length];
      for (int index = 0; index < collection.Length; ++index)
        collection[index] = reader.ReadBoolean();
      return (IList<bool>) new List<bool>((IEnumerable<bool>) collection);
    }

    public IList<string> ReadIListOfString(IByteReaderBase reader)
    {
      int length = reader.ReadInt32();
      if (length == 0)
        return (IList<string>) null;
      string[] collection = new string[length];
      for (int index = 0; index < collection.Length; ++index)
        collection[index] = reader.ReadString();
      return (IList<string>) new List<string>((IEnumerable<string>) collection);
    }

    public IList<sbyte> ReadIListOfSByte(IByteReaderBase reader)
    {
      int length = reader.ReadInt32();
      if (length == 0)
        return (IList<sbyte>) null;
      sbyte[] collection = new sbyte[length];
      for (int index = 0; index < collection.Length; ++index)
        collection[index] = reader.ReadSByte();
      return (IList<sbyte>) new List<sbyte>((IEnumerable<sbyte>) collection);
    }

    public IList<short> ReadIListOfInt16(IByteReaderBase reader)
    {
      int length = reader.ReadInt32();
      if (length == 0)
        return (IList<short>) null;
      short[] collection = new short[length];
      for (int index = 0; index < collection.Length; ++index)
        collection[index] = reader.ReadInt16();
      return (IList<short>) new List<short>((IEnumerable<short>) collection);
    }

    public IList<int> ReadIListOfInt32(IByteReaderBase reader)
    {
      int length = reader.ReadInt32();
      if (length == 0)
        return (IList<int>) null;
      int[] collection = new int[length];
      for (int index = 0; index < collection.Length; ++index)
        collection[index] = reader.ReadInt32();
      return (IList<int>) new List<int>((IEnumerable<int>) collection);
    }

    public IList<long> ReadIListOfInt64(IByteReaderBase reader)
    {
      int length = reader.ReadInt32();
      if (length == 0)
        return (IList<long>) null;
      long[] collection = new long[length];
      for (int index = 0; index < collection.Length; ++index)
        collection[index] = reader.ReadInt64();
      return (IList<long>) new List<long>((IEnumerable<long>) collection);
    }

    public IList<Decimal> ReadIListOfDecimal(IByteReaderBase reader)
    {
      int length = reader.ReadInt32();
      if (length == 0)
        return (IList<Decimal>) null;
      Decimal[] collection = new Decimal[length];
      for (int index = 0; index < collection.Length; ++index)
        collection[index] = reader.ReadDecimal();
      return (IList<Decimal>) new List<Decimal>((IEnumerable<Decimal>) collection);
    }

    public IList<ushort> ReadIListOfUInt16(IByteReaderBase reader)
    {
      int length = reader.ReadInt32();
      if (length == 0)
        return (IList<ushort>) null;
      ushort[] collection = new ushort[length];
      for (int index = 0; index < collection.Length; ++index)
        collection[index] = reader.ReadUInt16();
      return (IList<ushort>) new List<ushort>((IEnumerable<ushort>) collection);
    }

    public IList<uint> ReadIListOfUInt32(IByteReaderBase reader)
    {
      int length = reader.ReadInt32();
      if (length == 0)
        return (IList<uint>) null;
      uint[] collection = new uint[length];
      for (int index = 0; index < collection.Length; ++index)
        collection[index] = reader.ReadUInt32();
      return (IList<uint>) new List<uint>((IEnumerable<uint>) collection);
    }

    public IList<ulong> ReadIListOfUInt64(IByteReaderBase reader)
    {
      int length = reader.ReadInt32();
      if (length == 0)
        return (IList<ulong>) null;
      ulong[] collection = new ulong[length];
      for (int index = 0; index < collection.Length; ++index)
        collection[index] = reader.ReadUInt64();
      return (IList<ulong>) new List<ulong>((IEnumerable<ulong>) collection);
    }

    public IList<float> ReadIListOfSingle(IByteReaderBase reader)
    {
      int length = reader.ReadInt32();
      if (length == 0)
        return (IList<float>) null;
      float[] collection = new float[length];
      for (int index = 0; index < collection.Length; ++index)
        collection[index] = reader.ReadSingle();
      return (IList<float>) new List<float>((IEnumerable<float>) collection);
    }

    public IList<double> ReadIListOfDouble(IByteReaderBase reader)
    {
      int length = reader.ReadInt32();
      if (length == 0)
        return (IList<double>) null;
      double[] collection = new double[length];
      for (int index = 0; index < collection.Length; ++index)
        collection[index] = reader.ReadDouble();
      return (IList<double>) new List<double>((IEnumerable<double>) collection);
    }

    public IList<T> ReadIListOfT<T>(IByteReaderBase reader) where T : INetworkSerializable, new()
    {
      int num = reader.ReadInt32();
      if (num == 0)
        return (IList<T>) null;
      List<T> objList = new List<T>();
      for (int index = 0; index < num; ++index)
      {
        T obj = new T();
        obj.Deserialize(reader);
        objList.Add(obj);
      }
      return (IList<T>) objList;
    }
  }
}
