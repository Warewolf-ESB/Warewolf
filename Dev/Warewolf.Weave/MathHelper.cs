// Decompiled with JetBrains decompiler
// Type: System.MathHelper
// Assembly: Weave, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 27ECE7EA-15BB-456D-882E-FA3F21C3F779
// Assembly location: C:\Builds\Warewolf\Dev\Binaries\Weave.dll

namespace System
{
  public static class MathHelper
  {
    private static System.Random _random = new System.Random();

    public static void EnsureRange(ref sbyte lower, ref sbyte upper)
    {
      if ((int) upper >= (int) lower)
        return;
      sbyte num = upper;
      upper = lower;
      lower = num;
    }

    public static void EnsureRange(ref byte lower, ref byte upper)
    {
      if ((int) upper >= (int) lower)
        return;
      byte num = upper;
      upper = lower;
      lower = num;
    }

    public static void EnsureRange(ref short lower, ref short upper)
    {
      if ((int) upper >= (int) lower)
        return;
      short num = upper;
      upper = lower;
      lower = num;
    }

    public static void EnsureRange(ref ushort lower, ref ushort upper)
    {
      if ((int) upper >= (int) lower)
        return;
      ushort num = upper;
      upper = lower;
      lower = num;
    }

    public static void EnsureRange(ref int lower, ref int upper)
    {
      if (upper >= lower)
        return;
      int num = upper;
      upper = lower;
      lower = num;
    }

    public static void EnsureRange(ref uint lower, ref uint upper)
    {
      if (upper >= lower)
        return;
      uint num = upper;
      upper = lower;
      lower = num;
    }

    public static void EnsureRange(ref long lower, ref long upper)
    {
      if (upper >= lower)
        return;
      long num = upper;
      upper = lower;
      lower = num;
    }

    public static void EnsureRange(ref ulong lower, ref ulong upper)
    {
      if (upper >= lower)
        return;
      ulong num = upper;
      upper = lower;
      lower = num;
    }

    public static void EnsureRange(ref float lower, ref float upper)
    {
      if ((double) upper >= (double) lower)
        return;
      float num = upper;
      upper = lower;
      lower = num;
    }

    public static void EnsureRange(ref double lower, ref double upper)
    {
      if (upper >= lower)
        return;
      double num = upper;
      upper = lower;
      lower = num;
    }

    public static void EnsureRange(ref Decimal lower, ref Decimal upper)
    {
      if (upper >= lower)
        return;
      Decimal num = upper;
      upper = lower;
      lower = num;
    }

    public static double Random() => MathHelper._random.NextDouble();

    public static sbyte Random(sbyte lowerInclusive, sbyte upperExclusive)
    {
      MathHelper.EnsureRange(ref lowerInclusive, ref upperExclusive);
      return (sbyte) MathHelper._random.Next((int) lowerInclusive, (int) upperExclusive);
    }

    public static sbyte Random(sbyte upperExclusive) => (sbyte) MathHelper._random.Next((int) upperExclusive);

    public static byte Random(byte lowerInclusive, byte upperExclusive)
    {
      MathHelper.EnsureRange(ref lowerInclusive, ref upperExclusive);
      return (byte) MathHelper._random.Next((int) lowerInclusive, (int) upperExclusive);
    }

    public static byte Random(byte upperExclusive) => (byte) MathHelper._random.Next((int) upperExclusive);

    public static short Random(short lowerInclusive, short upperExclusive)
    {
      MathHelper.EnsureRange(ref lowerInclusive, ref upperExclusive);
      return (short) MathHelper._random.Next((int) lowerInclusive, (int) upperExclusive);
    }

    public static short Random(short upperExclusive) => (short) MathHelper._random.Next((int) upperExclusive);

    public static ushort Random(ushort lowerInclusive, ushort upperExclusive)
    {
      MathHelper.EnsureRange(ref lowerInclusive, ref upperExclusive);
      return (ushort) MathHelper._random.Next((int) lowerInclusive, (int) upperExclusive);
    }

    public static ushort Random(ushort upperExclusive) => (ushort) MathHelper._random.Next((int) upperExclusive);

    public static int Random(int lowerInclusive, int upperExclusive)
    {
      MathHelper.EnsureRange(ref lowerInclusive, ref upperExclusive);
      return MathHelper._random.Next(lowerInclusive, upperExclusive);
    }

    public static int Random(int upperExclusive) => MathHelper._random.Next(upperExclusive);

    public static uint Random(uint lowerInclusive, uint upperExclusive)
    {
      MathHelper.EnsureRange(ref lowerInclusive, ref upperExclusive);
      return (uint) (MathHelper._random.NextDouble() * (double) (upperExclusive - lowerInclusive)) + lowerInclusive;
    }

    public static uint Random(uint upperExclusive) => (uint) (MathHelper._random.NextDouble() * (double) upperExclusive);

    public static long Random(long lowerInclusive, long upperExclusive)
    {
      MathHelper.EnsureRange(ref lowerInclusive, ref upperExclusive);
      return (long) (MathHelper._random.NextDouble() * (double) (upperExclusive - lowerInclusive)) + lowerInclusive;
    }

    public static long Random(long upperExclusive) => (long) (MathHelper._random.NextDouble() * (double) upperExclusive);

    public static float Random(float lowerInclusive, float upperExclusive)
    {
      MathHelper.EnsureRange(ref lowerInclusive, ref upperExclusive);
      return (float) (MathHelper._random.NextDouble() * ((double) upperExclusive - (double) lowerInclusive)) + lowerInclusive;
    }

    public static float Random(float upperExclusive) => (float) MathHelper._random.NextDouble() * upperExclusive;

    public static double Random(double lowerInclusive, double upperExclusive)
    {
      MathHelper.EnsureRange(ref lowerInclusive, ref upperExclusive);
      return MathHelper._random.NextDouble() * (upperExclusive - lowerInclusive) + lowerInclusive;
    }

    public static double Random(double upperExclusive) => MathHelper._random.NextDouble() * upperExclusive;

    public static int GreatestCommonDivisor(params int[] integers)
    {
      if (integers == null)
        return 0;
      int length = integers.Length;
      if (length < 3)
      {
        int num;
        switch (length)
        {
          case 1:
            num = integers[0];
            break;
          case 2:
            num = MathHelper.GreatestCommonDivisor(integers[0], integers[1]);
            break;
          default:
            num = 0;
            break;
        }
        return num;
      }
      int v = MathHelper.GreatestCommonDivisor(integers[0], integers[1]);
      for (int index = 2; index < integers.Length; ++index)
        v = MathHelper.GreatestCommonDivisor(integers[index], v);
      return v;
    }

    public static int GreatestCommonDivisor(int u, int v)
    {
      if (u == 0 || v == 0)
        return u | v;
      int num1 = 0;
      while (((u | v) & 1) == 0)
      {
        u >>= 1;
        v >>= 1;
        ++num1;
      }
      while ((u & 1) == 0)
        u >>= 1;
      while (true)
      {
        while ((v & 1) != 0)
        {
          if (u < v)
          {
            v -= u;
          }
          else
          {
            int num2 = u - v;
            u = v;
            v = num2;
          }
          v >>= 1;
          if (v == 0)
            return u << num1;
        }
        v >>= 1;
      }
    }

    public static int LeastCommonMultiple(params int[] integers)
    {
      if (integers == null)
        return 0;
      int length = integers.Length;
      if (length < 3)
      {
        int num;
        switch (length)
        {
          case 1:
            num = integers[0];
            break;
          case 2:
            num = MathHelper.LeastCommonMultiple(integers[0], integers[1]);
            break;
          default:
            num = 0;
            break;
        }
        return num;
      }
      int v = MathHelper.LeastCommonMultiple(integers[0], integers[1]);
      for (int index = 2; index < integers.Length; ++index)
        v = MathHelper.LeastCommonMultiple(integers[index], v);
      return v;
    }

    public static int LeastCommonMultiple(int u, int v) => u == 0 || v == 0 ? 0 : u / MathHelper.GreatestCommonDivisor(u, v) * v;
  }
}
