// Decompiled with JetBrains decompiler
// Type: System.__IByteWriterBaseExtensions
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

namespace System
{
  public static class __IByteWriterBaseExtensions
  {
    public static void Write(this IByteWriterBase writer, bool[] array)
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

    public static void Write(
      this IByteWriterBase writer,
      bool[] array,
      int arrayIndex,
      int amount)
    {
      writer.Write(amount);
      for (int index = 0; index < amount; ++index)
        writer.Write(array[arrayIndex + index]);
    }

    public static void Write(this IByteWriterBase writer, string[] array)
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

    public static void Write(
      this IByteWriterBase writer,
      string[] array,
      int arrayIndex,
      int amount)
    {
      writer.Write(amount);
      for (int index = 0; index < amount; ++index)
        writer.Write(array[arrayIndex + index]);
    }

    public static void Write(this IByteWriterBase writer, sbyte[] array)
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

    public static void Write(this IByteWriterBase writer, byte[] array)
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

    public static void Write(
      this IByteWriterBase writer,
      sbyte[] array,
      int arrayIndex,
      int amount)
    {
      writer.Write(amount);
      for (int index = 0; index < amount; ++index)
        writer.Write(array[arrayIndex + index]);
    }

    public static void Write(this IByteWriterBase writer, short[] array)
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

    public static void Write(
      this IByteWriterBase writer,
      short[] array,
      int arrayIndex,
      int amount)
    {
      writer.Write(amount);
      for (int index = 0; index < amount; ++index)
        writer.Write(array[arrayIndex + index]);
    }

    public static void Write(this IByteWriterBase writer, int[] array)
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

    public static void Write(this IByteWriterBase writer, int[] array, int arrayIndex, int amount)
    {
      writer.Write(amount);
      for (int index = 0; index < amount; ++index)
        writer.Write(array[arrayIndex + index]);
    }

    public static void Write(this IByteWriterBase writer, long[] array)
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

    public static void Write(
      this IByteWriterBase writer,
      long[] array,
      int arrayIndex,
      int amount)
    {
      writer.Write(amount);
      for (int index = 0; index < amount; ++index)
        writer.Write(array[arrayIndex + index]);
    }

    public static void Write(this IByteWriterBase writer, Decimal[] array)
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

    public static void Write(
      this IByteWriterBase writer,
      Decimal[] array,
      int arrayIndex,
      int amount)
    {
      writer.Write(amount);
      for (int index = 0; index < amount; ++index)
        writer.Write(array[arrayIndex + index]);
    }

    public static void Write(this IByteWriterBase writer, ushort[] array)
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

    public static void Write(
      this IByteWriterBase writer,
      ushort[] array,
      int arrayIndex,
      int amount)
    {
      writer.Write(amount);
      for (int index = 0; index < amount; ++index)
        writer.Write(array[arrayIndex + index]);
    }

    public static void Write(this IByteWriterBase writer, uint[] array)
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

    public static void Write(
      this IByteWriterBase writer,
      uint[] array,
      int arrayIndex,
      int amount)
    {
      writer.Write(amount);
      for (int index = 0; index < amount; ++index)
        writer.Write(array[arrayIndex + index]);
    }

    public static void Write(this IByteWriterBase writer, ulong[] array)
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

    public static void Write(
      this IByteWriterBase writer,
      ulong[] array,
      int arrayIndex,
      int amount)
    {
      writer.Write(amount);
      for (int index = 0; index < amount; ++index)
        writer.Write(array[arrayIndex + index]);
    }

    public static void Write(this IByteWriterBase writer, float[] array)
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

    public static void Write(
      this IByteWriterBase writer,
      float[] array,
      int arrayIndex,
      int amount)
    {
      writer.Write(amount);
      for (int index = 0; index < amount; ++index)
        writer.Write(array[arrayIndex + index]);
    }

    public static void Write(this IByteWriterBase writer, double[] array)
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

    public static void Write(
      this IByteWriterBase writer,
      double[] array,
      int arrayIndex,
      int amount)
    {
      writer.Write(amount);
      for (int index = 0; index < amount; ++index)
        writer.Write(array[arrayIndex + index]);
    }
  }
}
